IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UpdateFSSDrugItemPriceDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [UpdateFSSDrugItemPriceDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure UpdateFSSDrugItemPriceDetails
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemId int,
@DrugItemPriceId int, 
@ModificationStatusId int,
                  	                  
@TrackingCustomerRatio decimal(10,2),        
@TrackingCustomerPrice decimal(10,2),
@TrackingCustomerName nvarchar(200),
@ExcludeFromExport bit
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@currentUserName nvarchar(120),
	@retVal int,
	@Notes nvarchar(2000)

BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserName OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during update of item price details for fss contract for contract ' + @ContractNumber
		goto ERROREXIT
	END

	Select @Notes = 'UpdateFSSDrugItemPriceDetails;'

	Exec @retVal = CopyItemPriceIntoPriceHistory @CurrentUser, @SecurityServerName, @SecurityDatabaseName, 
								@ContractNumber, @ModificationStatusId, @DrugItemPriceId, @Notes

	SELECT @error = @@ERROR
	IF @retVal = -1 OR @error > 0
	BEGIN
		select @errorMsg = 'Error returned when Inserting price history for contract ' + @ContractNumber
  		GOTO ERROREXIT
	END

	
	update DI_DrugItemPrice
		set AwardedFSSTrackingCustomerRatio = @TrackingCustomerRatio,        
		CurrentTrackingCustomerPrice = @TrackingCustomerPrice,         
		TrackingCustomerName = @TrackingCustomerName,         
		ExcludeFromExport = @ExcludeFromExport,
		LastModifiedBy = @currentUserName,
		LastModificationDate = getdate(),
		LastModificationType = 'C'

	where DrugItemPriceId = @DrugItemPriceId

	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error updating drug item price details for fss contract ' + @ContractNumber
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





