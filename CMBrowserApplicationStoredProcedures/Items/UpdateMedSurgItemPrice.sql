IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateMedSurgItemPrice' )
BEGIN
	DROP PROCEDURE UpdateMedSurgItemPrice
END
GO

CREATE PROCEDURE UpdateMedSurgItemPrice
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ContractId int,
@ModificationStatusId int,
@ItemPriceId int,
@PriceStartDate datetime,
@PriceEndDate datetime,
@IsTemporary bit,
@Price decimal(18,2)
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
			'UpdateMedSurgItemPrice', @currentUserLogin, getdate() 
	from CM_ItemPrice
	where ItemPriceId = @ItemPriceId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving item price into history.'
		goto ERROREXIT
	END

	update CM_ItemPrice
		set PriceStartDate = @PriceStartDate, 
			PriceStopDate = @PriceEndDate, 
			IsTemporary = @IsTemporary,
			Price = @Price, 
			LastModificationType = 'C', 
			ModificationStatusId = @ModificationStatusId, 
			LastModifiedBy = @currentUserLogin, 
			LastModificationDate = getdate()
	where ItemPriceId = @ItemPriceId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating price.'
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


