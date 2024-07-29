IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[InsertTieredPriceForDrugItemPrice]') AND type in (N'P', N'PC'))
DROP PROCEDURE [InsertTieredPriceForDrugItemPrice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure InsertTieredPriceForDrugItemPrice
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemPriceId int,

@TieredPriceStartDate datetime,         
@TieredPriceStopDate datetime,       
@Price decimal(9,2),      
@Minimum nvarchar(200),
@MinimumValue int,

@ModificationStatusId int,
@DrugItemTieredPriceId int OUTPUT
)

AS

DECLARE @ContractId int,
	@loginName  nvarchar(120),
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	

BEGIN Transaction

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @loginName OUTPUT 

	if @error <> 0 or @loginName is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
    	GOTO ERROREXIT
	END
	
	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId for contract ' + @ContractNumber
    	GOTO ERROREXIT
	END
	
	insert into DI_DrugItemTieredPrice
	( DrugItemPriceId, TieredPriceStartDate, TieredPriceStopDate, Price, Minimum, MinimumValue, ModificationStatusId,
		CreatedBy,
		CreationDate,
		LastModifiedBy, 
		LastModificationDate )
	values
	( @DrugItemPriceId, @TieredPriceStartDate, @TieredPriceStopDate, @Price, @Minimum, @MinimumValue, @ModificationStatusId,
		@loginName,
		getdate(),
		@loginName, 
		getdate() )

	select @error = @@error, @DrugItemTieredPriceId = @@identity
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting tiered price for contract ' + @ContractNumber
    	GOTO ERROREXIT
	END

	goto OKEXIT

 
	ERROREXIT:

		  raiserror( @errorMsg, 16, 1 )
		  if @@TRANCOUNT > 1
		  BEGIN
				COMMIT TRANSACTION
		  END
		  Else if @@TRANCOUNT = 1
		  BEGIN
				/* only rollback iff this the highest level */
				ROLLBACK TRANSACTION
		  END

		  RETURN( -1 )

	OKEXIT:
		If @@TRANCOUNT > 0
		BEGIN
			COMMIT TRANSACTION
		END
		RETURN( 0 )	

