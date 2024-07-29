IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateOfferVendorBusinessAddress' )
BEGIN
	DROP PROCEDURE UpdateOfferVendorBusinessAddress
END
GO

CREATE PROCEDURE UpdateOfferVendorBusinessAddress
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@OfferId int,
@VendorAddress1 nvarchar(100),
@VendorAddress2 nvarchar(100),
@VendorCity nvarchar(20),
@VendorState nvarchar(2),
@VendorZip nvarchar(10),
@VendorCountry nvarchar(50),
@VendorCountryId int,
@VendorWebAddress nvarchar(50)
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
	set Primary_Address_1 = @VendorAddress1,
		Primary_Address_2 = @VendorAddress2,
		Primary_City = @VendorCity,
		Primary_State = @VendorState,
		Primary_Zip = @VendorZip,
		Country = @VendorCountry,
		Primary_CountryId = @VendorCountryId,
		POC_VendorWeb = @VendorWebAddress,
		Date_Modified = GETDATE(),
		LastModifiedBy = @currentUserLogin		
	where Offer_ID = @OfferId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating offer vendor address information for OfferId= ' + CONVERT( nvarchar(20), @OfferId )
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


