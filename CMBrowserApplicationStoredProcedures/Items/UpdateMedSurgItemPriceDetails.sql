IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateMedSurgItemPriceDetails' )
BEGIN
	DROP PROCEDURE UpdateMedSurgItemPriceDetails
END
GO

CREATE PROCEDURE UpdateMedSurgItemPriceDetails
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ItemPriceId int,
@ModificationStatusId int,
@TrackingCustomerPrice decimal(10,2),
@TrackingCustomerRatio nvarchar(100),
@TrackingCustomerName nvarchar(100),
@TrackingCustomerFOBTerms nvarchar(40)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserLogin nvarchar(120)
		
BEGIN TRANSACTION

	exec dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 
	Select @error = @@error		
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert( nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	insert into CM_ItemPriceHistory
	( ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, Removed, TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 
			Notes, MovedToHistoryBy, DateMovedToHistory )
	select ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, 0, TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 
			'UpdateMedSurgItemPriceDetails', @currentUserLogin, getdate() 
	from CM_ItemPrice
	where ItemPriceId = @ItemPriceId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving item price details into history.'
		goto ERROREXIT
	END

	update CM_ItemPrice
	set TrackingCustomerPrice = @TrackingCustomerPrice,
		TrackingCustomerRatio = @TrackingCustomerRatio,
		TrackingCustomerName = @TrackingCustomerName,
		TrackingCustomerFOBTerms = @TrackingCustomerFOBTerms,
		ModificationStatusId = @ModificationStatusId,
		LastModificationType = 'C',
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = getdate()
	where ItemPriceId = @ItemPriceId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating price details.'
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





