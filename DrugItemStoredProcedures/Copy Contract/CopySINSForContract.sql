IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopySINSForContract')
	BEGIN
		DROP  Procedure  CopySINSForContract
	END

GO

CREATE Procedure CopySINSForContract
(
	@CopyContractLogId int,
	@OldContractNumber nvarchar(50),
	@NewContractNumber nvarchar(50),
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
		From tbl_Cntrcts_SINs
		Where CntrctNum = @OldContractNumber
		and Inactive = 0
		
		IF @count = 0
		BEGIN
			Update tbl_CopyContractsLog
				Set TotalSINSCopied = 0
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
			Insert Into tbl_Cntrcts_SINs
			(CntrctNum,SINs,Recoverable,Inactive,LexicalSIN,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate)
			Select @NewContractNumber,SINs,Recoverable,0,LexicalSIN,@UserLogin,GETDATE(),@UserLogin,GETDATE()
			From tbl_Cntrcts_SINs
			Where CntrctNum = @OldContractNumber
			and Inactive = 0

			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting tbl_Cntrcts_SINs for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END	

			Update tbl_CopyContractsLog
				Set TotalSINSCopied = @count
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
