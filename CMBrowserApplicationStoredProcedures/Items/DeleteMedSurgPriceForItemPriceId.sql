IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteMedSurgPriceForItemPriceId' )
BEGIN
	DROP PROCEDURE DeleteMedSurgPriceForItemPriceId
END
GO

CREATE PROCEDURE DeleteMedSurgPriceForItemPriceId
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ModificationStatusId int,
@ItemPriceId int
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
	( ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms, Removed, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 
			Notes, MovedToHistoryBy, DateMovedToHistory )
	select ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms, 1, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 
			'DeleteMedSurgPriceForItemPriceId', @currentUserLogin, getdate() 
	from CM_ItemPrice
	where ItemPriceId = @ItemPriceId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error moving item price into history.'
		goto ERROREXIT
	END

	delete CM_ItemPrice
	Output 'CM_ItemPrice', Deleted.ItemPriceId, @currentUserLogin, GETDATE() into Audit_Deleted_Data_By_User
	where ItemPriceId = @ItemPriceId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting item price after move to history.'
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
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )



