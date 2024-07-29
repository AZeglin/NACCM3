IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectItemDiscontinuationReasons' )
BEGIN
	DROP PROCEDURE SelectItemDiscontinuationReasons
END
GO

CREATE PROCEDURE SelectItemDiscontinuationReasons
(
@CurrentUser uniqueidentifier,
@ItemDiscontinuationCategory nchar(1) -- 'A' = all;
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	if @ItemDiscontinuationCategory = 'A'
	BEGIN
		select DiscontinuationReasonId, DiscontinuationReason, DiscontinuationReasonCategory
		from DI_ItemDiscontinuationReasons
		where IsActive = 1
		order by DiscontinuationReason
	END
	else
	BEGIN
		select DiscontinuationReasonId, DiscontinuationReason, DiscontinuationReasonCategory
		from DI_ItemDiscontinuationReasons
		where IsActive = 1
		and DiscontinuationReasonCategory = @ItemDiscontinuationCategory
		order by DiscontinuationReason
	END

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error'
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


