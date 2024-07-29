IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateOfferGeneralOfferDates' )
BEGIN
	DROP PROCEDURE UpdateOfferGeneralOfferDates
END
GO

CREATE PROCEDURE UpdateOfferGeneralOfferDates
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@OfferId int,
@DateReceived datetime,
@DateAssigned datetime,
@DateReassigned datetime
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
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	update tbl_Offers
	set Dates_Received = @DateReceived,
		Dates_Assigned = @DateAssigned,
		Dates_Reassigned = @DateReassigned,
		Date_Modified = GETDATE(),
		LastModifiedBy = @currentUserLogin		
	where Offer_ID = @OfferId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating offer comment for OfferId= ' + CONVERT( nvarchar(20), @OfferId )
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


