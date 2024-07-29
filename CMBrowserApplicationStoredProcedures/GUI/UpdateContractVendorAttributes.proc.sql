IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractVendorAttributes' )
BEGIN
	DROP PROCEDURE UpdateContractVendorAttributes
END
GO

CREATE PROCEDURE UpdateContractVendorAttributes
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractId int,
@ContractNumber nvarchar(20),
@SAMUEI nvarchar(12),
@DUNS nvarchar(9), 
@TIN nvarchar(9), 
@VendorTypeId int, 
@CreditCardAccepted bit, 
@HazardousMaterial bit
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

	update tbl_Cntrcts
	set DUNS = @DUNS,
		TIN = @TIN,
		Dist_Manf_ID = @VendorTypeId,
		Credit_Card_Accepted = @CreditCardAccepted,
		Hazard = @HazardousMaterial,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where CntrctNum = @ContractNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating vendor attributes for contract ' + @ContractNumber
		goto ERROREXIT
	END

	/* insert into history */
	insert into CM_SAMVendorInfoHistory	
	( SAMVendorInfoId, ContractId, BPAUsesParentInfo, SAMUEI, RetrievalError, ErrorMessage, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 
	  SourcedFromDoug, SourcedFromRay, SourcedFromShawn, MovedToHistoryBy, DateMovedToHistory )
	select
	  SAMVendorInfoId, ContractId, BPAUsesParentInfo, SAMUEI, RetrievalError, ErrorMessage, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 
	  SourcedFromDoug, SourcedFromRay, SourcedFromShawn, @currentUserLogin, getdate()
	from CM_SAMVendorInfo
	where ContractId = @ContractId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error saving vendor attributes to history before update for contract ' + @ContractNumber
		goto ERROREXIT
	END

	update CM_SAMVendorInfo
		set SAMUEI = @SAMUEI,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where ContractId = @ContractId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating vendor attributes (2) for contract ' + @ContractNumber
		goto ERROREXIT
	END

	/* update the SAM vendor record for any associated BPA's for data elements which are not independently editable ( e.g., UEI ) */
	/* note the current contract may not have any BPA's or may be a BPA itself, in which case there will not be any additional records to update */	

	/* first save to history */
	insert into CM_SAMVendorInfoHistory	
	( SAMVendorInfoId, ContractId, BPAUsesParentInfo, SAMUEI, RetrievalError, ErrorMessage, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 
	  SourcedFromDoug, SourcedFromRay, SourcedFromShawn, MovedToHistoryBy, DateMovedToHistory )
	select
	  SAMVendorInfoId, ContractId, BPAUsesParentInfo, SAMUEI, RetrievalError, ErrorMessage, v.CreatedBy, v.CreationDate, v.LastModifiedBy, v.LastModificationDate, 
	  SourcedFromDoug, SourcedFromRay, SourcedFromShawn, @currentUserLogin, getdate()
	from CM_SAMVendorInfo v join tbl_Cntrcts c on v.ContractId = c.Contract_Record_ID
	join CM_BPALookup b on b.BPAContractNumber = c.CntrctNum
	where b.FSSContractNumber = @ContractNumber
	and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error saving vendor attributes to history (2) before update for contract ' + @ContractNumber
		goto ERROREXIT
	END

	update v
		set SAMUEI = @SAMUEI,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	from CM_SAMVendorInfo v join tbl_Cntrcts c on v.ContractId = c.Contract_Record_ID
	join CM_BPALookup b on b.BPAContractNumber = c.CntrctNum
	where b.FSSContractNumber = @ContractNumber
	and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating vendor attributes (3) for contract ' + @ContractNumber
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


