IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertTieredPriceForItemPrice' )
BEGIN
	DROP PROCEDURE InsertTieredPriceForItemPrice
END
GO

CREATE PROCEDURE InsertTieredPriceForItemPrice
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ModificationStatusId int,
@ItemPriceId int,
@TieredPriceStartDate datetime,
@TieredPriceStopDate datetime,
@Price decimal(18,2),
@TierSequence int,
@TierCriteria nvarchar(255),
@TierMinimumValue int,
@ItemTieredPriceId int OUTPUT
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

	insert into CM_ItemTieredPrice
	( ItemPriceId, TieredPriceStartDate, TieredPriceStopDate, Price, TierSequence, TierCriteria, MinimumValue, LastModificationType, ModificationStatusId, LastModifiedBy, LastModificationDate )
	values
	( @ItemPriceId, @TieredPriceStartDate, @TieredPriceStopDate, @Price, @TierSequence, @TierCriteria, @TierMinimumValue, 'C', @ModificationStatusId, @currentUserLogin, getdate() )
	
	select @error = @@ERROR, @rowCount = @@ROWCOUNT, @ItemTieredPriceId = SCOPE_IDENTITY()
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting item tiered price.'
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


