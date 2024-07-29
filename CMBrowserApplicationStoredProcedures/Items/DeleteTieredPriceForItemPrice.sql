IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteTieredPriceForItemPrice' )
BEGIN
	DROP PROCEDURE DeleteTieredPriceForItemPrice
END
GO

CREATE PROCEDURE DeleteTieredPriceForItemPrice
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ModificationStatusId int,
@ItemTieredPriceId int
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

	insert into CM_ItemTieredPriceHistory
	( ItemTieredPriceId, ItemPriceId, TieredPriceStartDate, TieredPriceStopDate, Price, TierSequence, TierCriteria, MinimumValue, LastModificationType, ModificationStatusId, Removed, LastModifiedBy, LastModificationDate,
			Notes, MovedToHistoryBy, DateMovedToHistory )
	select ItemTieredPriceId, ItemPriceId, TieredPriceStartDate, TieredPriceStopDate, Price, TierSequence, TierCriteria, MinimumValue, LastModificationType, @ModificationStatusId, 1, LastModifiedBy, LastModificationDate,
			'DeleteTieredPriceForItemPrice', @currentUserLogin, getdate() 
	from CM_ItemTieredPrice
	where ItemTieredPriceId = @ItemTieredPriceId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving item tiered price into history during delete.'
		goto ERROREXIT
	END

	delete CM_ItemTieredPrice		
	Output 'CM_ItemTieredPrice', Deleted.ItemTieredPriceId, @currentUserLogin, GETDATE() into Audit_Deleted_Data_By_User
	where ItemTieredPriceId = @ItemTieredPriceId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating tiered price.'
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


