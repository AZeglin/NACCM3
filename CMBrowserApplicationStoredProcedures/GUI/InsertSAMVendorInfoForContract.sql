IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertSAMVendorInfoForContract' )
BEGIN
	DROP PROCEDURE InsertSAMVendorInfoForContract
END
GO

CREATE PROCEDURE InsertSAMVendorInfoForContract
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(50),
@NewContractId int,
@ParentContractNumber nvarchar(50),
@SAMVendorInfoId int OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),	
		@currentUserLogin nvarchar(120),
		@parentContractId int



BEGIN TRANSACTION
	
	-- called from CreateContract sp

	exec dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 

	Select @error = @@error		
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	if @ParentContractNumber is not null 
	BEGIN
		select @parentContractId = Contract_Record_ID
		from tbl_Cntrcts
		where CntrctNum = @ParentContractNumber

		select @error = @@ERROR, @rowCount = @@ROWCOUNT

		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error retrieving parent contract id when inserting SAM Vendor Info for contract ' + @ContractNumber
			goto ERROREXIT
		END

		-- adding SAMUEI from parent
		insert into CM_SAMVendorInfo
		( ContractId, BPAUsesParentInfo, SAMUEI, RetrievalError, ErrorMessage, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, SourcedFromDoug, SourcedFromRay, SourcedFromShawn )
		select
		@NewContractId, 1, SAMUEI, 0, null, @currentUserLogin, getdate(), @currentUserLogin, getdate(), 0, 0, 0
		from CM_SAMVendorInfo
		where ContractId = @parentContractId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @SAMVendorInfoId = SCOPE_IDENTITY()
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error inserting SAM Vendor Info from parent contract for contract ' + @ContractNumber
			goto ERROREXIT
		END

	END
	else
	BEGIN
	
		insert into CM_SAMVendorInfo
		( ContractId, BPAUsesParentInfo, SAMUEI, RetrievalError, ErrorMessage, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, SourcedFromDoug, SourcedFromRay, SourcedFromShawn )
		select
		@NewContractId, 0, null, 0, null, @currentUserLogin, getdate(), @currentUserLogin, getdate(), 0, 0, 0
		
		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @SAMVendorInfoId = SCOPE_IDENTITY()
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error inserting SAM Vendor Info for contract ' + @ContractNumber
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



