IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractContact' )
BEGIN
	DROP PROCEDURE UpdateContractContact
END
GO

CREATE PROCEDURE UpdateContractContact
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractId int,
@ContractNumber nvarchar(20),
@ContactType nvarchar(4),  -- 'ADM' - administrator; 'ALT' - alternate; 'TECH' - technical; 'EMER' - emergency; 'ORD' - ordering; 'SALE' - sales;  'BUS' - business address
@Name nvarchar(75),  -- length 30 for most person names, but need 75 for vendorname
@Phone nvarchar(15),
@Extension nvarchar(5),
@Fax nvarchar(15),
@Email nvarchar(50),
@Address1 nvarchar(100),
@Address2 nvarchar(100),
@City nvarchar(20),
@CountryId int,
@State nvarchar(2),
@Zip nvarchar(10),
@WebAddress nvarchar(50)
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

	if @ContactType = 'ADM'
	BEGIN

		update tbl_Cntrcts
		set POC_Primary_Name = @Name,
			POC_Primary_Phone = @Phone,
			POC_Primary_Ext = @Extension,
			POC_Primary_Fax = @Fax,
			POC_Primary_Email = @Email,
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = GETDATE()
		where CntrctNum = @ContractNumber

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating contract information type ' + @ContactType + ' for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else if @ContactType = 'ALT'
	BEGIN

		update tbl_Cntrcts
		set POC_Alternate_Name = @Name,
			POC_Alternate_Phone = @Phone,
			POC_Alternate_Ext = @Extension,
			POC_Alternate_Fax = @Fax,
			POC_Alternate_Email = @Email,
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = GETDATE()
		where CntrctNum = @ContractNumber

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating contract information type ' + @ContactType + ' for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else if @ContactType = 'TECH'
	BEGIN

		update tbl_Cntrcts
		set POC_Tech_Name = @Name,
			POC_Tech_Phone = @Phone,
			POC_Tech_Ext = @Extension,
			POC_Tech_Fax = @Fax,
			POC_Tech_Email = @Email,
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = GETDATE()
		where CntrctNum = @ContractNumber

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating contract information type ' + @ContactType + ' for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else if @ContactType = 'EMER'
	BEGIN

		update tbl_Cntrcts
		set POC_Emergency_Name = @Name,
			POC_Emergency_Phone = @Phone,
			POC_Emergency_Ext = @Extension,
			POC_Emergency_Fax = @Fax,
			POC_Emergency_Email = @Email,
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = GETDATE()
		where CntrctNum = @ContractNumber

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating contract information type ' + @ContactType + ' for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else if @ContactType = 'ORD'
	BEGIN

		update tbl_Cntrcts
		set Ord_Address_1 = @Address1,
			Ord_Address_2 = @Address2,
			Ord_City = @City,
			Ord_CountryId = @CountryId,
			Ord_State = @State,
			Ord_Zip = @Zip,
			Ord_Telephone = @Phone,
			Ord_Ext = @Extension,
			Ord_Fax = @Fax,
			Ord_EMail = @Email,
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = GETDATE()
		where CntrctNum = @ContractNumber

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating contract information type ' + @ContactType + ' for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else if @ContactType = 'SALE'
	BEGIN

		update tbl_Cntrcts
		set POC_Sales_Name = @Name,
			POC_Sales_Phone = @Phone,
			POC_Sales_Ext = @Extension,
			POC_Sales_Fax = @Fax,
			POC_Sales_Email = @Email,
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = GETDATE()
		where CntrctNum = @ContractNumber

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating contract information type ' + @ContactType + ' for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else if @ContactType = 'BUS'
	BEGIN

		update tbl_Cntrcts
		set Contractor_Name = @Name,
			Primary_Address_1 = @Address1,
			Primary_Address_2 = @Address2,
			Primary_City = @City,
			Primary_State = @State,
			Primary_Zip = @Zip,
			Primary_CountryId = @CountryId,
			POC_VendorWeb = @WebAddress,
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = GETDATE()
		where CntrctNum = @ContractNumber

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating contract information type ' + @ContactType + ' for contract ' + @ContractNumber
			goto ERROREXIT
		END
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


