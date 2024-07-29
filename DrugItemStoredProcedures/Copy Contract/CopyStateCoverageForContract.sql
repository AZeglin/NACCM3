IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopyStateCoverageForContract')
	BEGIN
		DROP  Procedure  CopyStateCoverageForContract
	END

GO

CREATE Procedure CopyStateCoverageForContract
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
		From tbl_Cntrcts_State_Coverage
		Where CntrctNum = @OldContractNumber
		
		IF @count = 0
		BEGIN
			Update tbl_CopyContractsLog
				Set TotalStateCoveragesCopied = 0
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
			Insert Into tbl_Cntrcts_State_Coverage
			(CntrctNum,Abbr)
			Select @NewContractNumber, Abbr
			From tbl_Cntrcts_State_Coverage
			Where CntrctNum = @OldContractNumber

			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting tbl_Cntrcts_State_Coverage for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END	

			Update tbl_CopyContractsLog
				Set TotalStateCoveragesCopied = @count
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


	