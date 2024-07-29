IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UpdateTieredPriceForDrugItemPrice]') AND type in (N'P', N'PC'))
DROP PROCEDURE [UpdateTieredPriceForDrugItemPrice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure UpdateTieredPriceForDrugItemPrice
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemPriceId int,
@DrugItemTieredPriceId int,
@ModificationStatusId int,
@TieredPriceStartDate datetime,
@TieredPriceStopDate datetime,
@Price decimal(9,2),
@Minimum nvarchar(200),
@MinimumValue int
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@currentUserName nvarchar(120)

BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserName OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during update of tiered price for fss contract for contract ' + @ContractNumber
		goto ERROREXIT
	END

	
	Insert into DI_DrugItemTieredPriceHistory
	(DrugItemTieredPriceId,DrugItemPriceId,TieredPriceStartDate,TieredPriceStopDate,Price,Minimum,MinimumValue,
	 ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate)
	Select DrugItemTieredPriceId,DrugItemPriceId,TieredPriceStartDate,TieredPriceStopDate,Price,Minimum,MinimumValue,
			@ModificationStatusId,CreatedBy,CreationDate,@currentUserName,getdate()
	From DI_DrugItemTieredPrice
	Where DrugItemTieredPriceId = @DrugItemTieredPriceId

	SELECT @error = @@ERROR
	IF  @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting into DI_DrugItemTieredPriceHistory  for @DrugItemTieredPriceId=' + convert( nvarchar(20), @DrugItemTieredPriceId )
		GOTO ERROREXIT
	END

		
	update DI_DrugItemTieredPrice
		set TieredPriceStartDate = @TieredPriceStartDate,
		TieredPriceStopDate = @TieredPriceStopDate,
		Price = @Price,
		Minimum = @Minimum,
		MinimumValue = @MinimumValue,
		ModificationStatusId = @ModificationStatusId,
		LastModifiedBy = @currentUserName,
		LastModificationDate = getdate()
		-- LastModificationType = 'C'
	where DrugItemTieredPriceId = @DrugItemTieredPriceId
	and DrugItemPriceId = @DrugItemPriceId

	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error updating tiered price for fss contract ' + @ContractNumber
		goto ERROREXIT
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
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 

ENDEXIT:






	

