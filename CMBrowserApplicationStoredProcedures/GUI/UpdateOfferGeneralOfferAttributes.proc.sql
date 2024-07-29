IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateOfferGeneralOfferAttributes' )
BEGIN
	DROP PROCEDURE UpdateOfferGeneralOfferAttributes
END
GO

CREATE PROCEDURE UpdateOfferGeneralOfferAttributes
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@OfferId int,
@OfferNumber nvarchar(30),
@ScheduleNumber int,
@COID int,
@ProposalTypeId int,
@VendorName nvarchar(75),
@SolicitationId int,
@ExtendsContractNumber nvarchar(20)
)

AS

  

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),	
		@currentUserLogin nvarchar(120),
		@retVal int

BEGIN TRANSACTION

	exec dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 

	Select @error = @@error		
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	update tbl_Offers
	set Solicitation_ID = @SolicitationId,
		OfferNumber = @OfferNumber,
		Schedule_Number = @ScheduleNumber,
		CO_ID = @COID,
		Proposal_Type_ID = @ProposalTypeId,
		Contractor_Name = @VendorName,
		ExtendsContractNumber = @ExtendsContractNumber,
		Date_Modified = GETDATE(),
		LastModifiedBy = @currentUserLogin		
	where Offer_ID = @OfferId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating general offer attributes for OfferId= ' + CONVERT( nvarchar(20), @OfferId )
		goto ERROREXIT
	END

	-- call to maintain the offer number
	exec @retVal = InsertUserRecentDocument @CurrentUser = @CurrentUser, @DocumentType = 'O', @DocumentNumber = @OfferNumber, @DocumentId = @OfferId

	select @error = @@ERROR
	if @error <> 0 or @retVal <> 0
	BEGIN
		select @errorMsg = 'Error updating recent document list when updating general offer attributes for OfferId= ' + CONVERT( nvarchar(20), @OfferId )
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


