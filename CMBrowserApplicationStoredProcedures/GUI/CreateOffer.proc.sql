IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CreateOffer' )
BEGIN
	DROP PROCEDURE CreateOffer
END
GO

CREATE PROCEDURE CreateOffer
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),

@OfferNumber nvarchar(30),
@ScheduleNumber int,
@COID int,
@ProposalTypeId int,
@ExtendsContractNumber nvarchar(20),
@VendorName nvarchar(75),
@SolicitationId int,

@DateReceived datetime,
@DateAssigned datetime = null,
@DateReassigned datetime = null,

@ActionId int,
@ActionDate datetime,
@ExpectedCompletionDate datetime = null,
@ExpirationDate datetime = null ,
@AuditIndicator bit,
@DateSentForPreaward datetime = null,
@DateReturnedToOffice datetime = null,

@VendorPrimaryContactName nvarchar(30),
@VendorPrimaryContactPhone nvarchar(15),
@VendorPrimaryContactExtension nvarchar(5),
@VendorPrimaryContactFax nvarchar(15),
@VendorPrimaryContactEmail nvarchar(50),

@VendorAddress1 nvarchar(100),
@VendorAddress2 nvarchar(100),
@VendorCity nvarchar(20),
@VendorState nvarchar(2),
@VendorZip nvarchar(10),
@VendorCountry nvarchar(50),
@VendorCountryId int,
@VendorWebAddress nvarchar(50),

@OfferComment nvarchar(4000),

@OfferId int OUTPUT,
@ScheduleName nvarchar(75) OUTPUT
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

	if exists ( select OfferNumber from tbl_Offers where OfferNumber = @OfferNumber )
	BEGIN
		select @errorMsg = 'Offer Number ' + @OfferNumber + ' already exists.'
		GOTO ERROREXIT
	END

	insert into tbl_Offers
	(
		Solicitation_ID, OfferNumber, Schedule_Number, CO_ID, Proposal_Type_ID, Contractor_Name,
		Dates_Received, Dates_Assigned, Dates_Reassigned,
		Action_ID, Dates_Action, Dates_Expected_Completion, Dates_Expiration,
		Audit_Indicator, Dates_Sent_for_Preaward, Dates_Returned_to_Office,
		POC_Primary_Name, POC_Primary_Phone, POC_Primary_Ext, POC_Primary_Fax, POC_Primary_Email,
		Primary_Address_1, Primary_Address_2, Primary_City, Primary_State, Primary_Zip, Country, Primary_CountryId, POC_VendorWeb,
		Comments, ExtendsContractNumber,
		CreatedBy, Date_Entered, LastModifiedBy, Date_Modified
	)
	values
	(
		@SolicitationId, @OfferNumber, @ScheduleNumber, @COID, @ProposalTypeId, @VendorName,
		@DateReceived, @DateAssigned, @DateReassigned,
		@ActionId, @ActionDate, @ExpectedCompletionDate, @ExpirationDate,
		@AuditIndicator, @DateSentForPreaward, @DateReturnedToOffice,
		@VendorPrimaryContactName, @VendorPrimaryContactPhone, @VendorPrimaryContactExtension, @VendorPrimaryContactFax, @VendorPrimaryContactEmail,
		@VendorAddress1, @VendorAddress2, @VendorCity, @VendorState, @VendorZip, @VendorCountry, @VendorCountryId, @VendorWebAddress,
		@OfferComment, @ExtendsContractNumber,
		@currentUserLogin, GETDATE(), @currentUserLogin, GETDATE()		
	)

	select @error = @@ERROR, @rowCount = @@ROWCOUNT, @OfferId = SCOPE_IDENTITY()
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error inserting new offer for offer number = ' + @OfferNumber
		goto ERROREXIT
	END

	-- select back the schedule name for display in the offer header after save
	select @ScheduleName = Schedule_Name 
	from [tlkup_Sched/Cat] where Schedule_Number = @ScheduleNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error selecting back schedule name for offer number = ' + @OfferNumber
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


