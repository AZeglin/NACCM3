IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetParentItemDescription' )
BEGIN
	DROP PROCEDURE GetParentItemDescription
END
GO

CREATE PROCEDURE GetParentItemDescription
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ItemId int,
@ItemDescription nvarchar(800) OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)
		
BEGIN TRANSACTION

	select @ItemDescription = ItemDescription
    from CM_Items
	where ItemId = @ItemId
                                               
	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount = 0
	BEGIN
		select @errorMsg = 'Error selecting parent item description for itemId=' + convert( nvarchar(20), @ItemId )
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



