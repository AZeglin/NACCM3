IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateOfferGeneralActionStatus' )
BEGIN
	DROP PROCEDURE UpdateOfferGeneralActionStatus
END
GO

CREATE PROCEDURE UpdateOfferGeneralActionStatus
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@OfferId int,
@ActionId int,
@ActionDate datetime,
@ExpectedCompletionDate datetime = null,
@ExpirationDate datetime = null,
@IsOfferCompleted bit OUTPUT 
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
	set Action_ID = @ActionId,
		Dates_Action = @ActionDate,
		Dates_Expected_Completion = @ExpectedCompletionDate,
		Dates_Expiration = @ExpirationDate,
		Date_Modified = GETDATE(),
		LastModifiedBy = @currentUserLogin		
	where Offer_ID = @OfferId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating offer action info for OfferId= ' + CONVERT( nvarchar(20), @OfferId )
		goto ERROREXIT
	END

	select @IsOfferCompleted = Complete
	from tlkup_Offers_Action_Type
	where Action_ID = @ActionId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving completion status after updating offer action info for OfferId= ' + CONVERT( nvarchar(20), @OfferId )
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


