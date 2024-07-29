IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopySAMVendorInfoForContract' )
BEGIN
	DROP PROCEDURE CopySAMVendorInfoForContract
END
GO

CREATE PROCEDURE CopySAMVendorInfoForContract
(
	@CopyContractLogId int,
	@OldContractNumber nvarchar(50),
	@NewContractNumber nvarchar(50),
	@OldContractId int,
	@NewContractId int,
	@UserLogin nvarchar(120)
)
As

	Declare @count int,
			@error int,
			@rowcount int,
			@errorMsg nvarchar(250),
			@retVal int	

	BEGIN TRANSACTION

		Select @count = Count(*) 
		From CM_SAMVendorInfo
		Where ContractId = @OldContractId
		
		IF @count = 0
		BEGIN
			Update tbl_CopyContractsLog
				Set TotalSAMVendorInfoCopied = 0
			Where CopyContractLogId = @CopyContractLogId
			
			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error updating tbl_CopyContractsLog for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END			
		END
		ELSE
		BEGIN
			Insert Into CM_SAMVendorInfo
			( ContractId, BPAUsesParentInfo, SAMUEI, RetrievalError, ErrorMessage, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, SourcedFromDoug, SourcedFromRay, SourcedFromShawn )
			select
				@NewContractId, BPAUsesParentInfo, SAMUEI, RetrievalError, ErrorMessage, @UserLogin, getdate(), @UserLogin, getdate(), SourcedFromDoug, SourcedFromRay, SourcedFromShawn
			from CM_SAMVendorInfo
			where ContractId = @OldContractId

			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting CM_SAMVendorInfo for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END	

			Update tbl_CopyContractsLog
				Set TotalSAMVendorInfoCopied = @count
			Where CopyContractLogId = @CopyContractLogId

			Select @error = @@ERROR			
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error updating tbl_CopyContractsLog for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END				
		END


GOTO OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 ) 

	IF @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
	/* only rollback iff this the highest level */ 
		ROLLBACK TRANSACTION
	END

	RETURN (-1)

OKEXIT:
	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)


	