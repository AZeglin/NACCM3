IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateOfferPrimaryContact' )
BEGIN
	DROP PROCEDURE UpdateOfferPrimaryContact
END
GO

CREATE PROCEDURE UpdateOfferPrimaryContact
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@OfferId int,
@VendorPrimaryContactName nvarchar(30),
@VendorPrimaryContactPhone nvarchar(15),
@VendorPrimaryContactExtension nvarchar(5),
@VendorPrimaryContactFax nvarchar(15),
@VendorPrimaryContactEmail nvarchar(50)
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
	set POC_Primary_Name = @VendorPrimaryContactName,
		POC_Primary_Phone = @VendorPrimaryContactPhone,
		POC_Primary_Ext = @VendorPrimaryContactExtension,
		POC_Primary_Fax = @VendorPrimaryContactFax,
		POC_Primary_Email = @VendorPrimaryContactEmail,
		Date_Modified = GETDATE(),
		LastModifiedBy = @currentUserLogin		
	where Offer_ID = @OfferId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating offer contact information for OfferId= ' + CONVERT( nvarchar(20), @OfferId )
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


