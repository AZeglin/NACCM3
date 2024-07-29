IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertMedSurgItemPrice' )
BEGIN
	DROP PROCEDURE InsertMedSurgItemPrice
END
GO

CREATE PROCEDURE InsertMedSurgItemPrice
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ModificationStatusId int,
@ItemId int,
@PriceStartDate datetime,
@PriceEndDate datetime,
@Price decimal(18,2),
@IsBPA bit,
@IsTemporary bit,
@ItemPriceId int OUTPUT
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
	

	insert into CM_ItemPrice
	( ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	values
	( @ItemId, 0, @PriceStartDate, @PriceEndDate, @Price, @IsBPA, @IsTemporary, 'C', @ModificationStatusId, @currentUserLogin, getdate(), @currentUserLogin, getdate() )

	select @error = @@ERROR, @rowCount = @@ROWCOUNT, @ItemPriceId = SCOPE_IDENTITY()
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error inserting price for item.'
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


