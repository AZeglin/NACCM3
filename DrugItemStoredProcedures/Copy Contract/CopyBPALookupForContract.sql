IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopyBPALookupForContract')
	BEGIN
		DROP  Procedure  CopyBPALookupForContract
	END

GO

CREATE Procedure CopyBPALookupForContract
(
	@CopyContractLogId int,
	@OldContractNumber nvarchar(50),
	@NewContractNumber nvarchar(50),	
	@UserLogin nvarchar(120)
)
As

	Declare @Count int,
			@error int,
			@rowcount int,
			@errorMsg nvarchar(250),
			@retVal int,
			@BPA_FSS_Counterpart nvarchar(50)

	BEGIN TRANSACTION
		select @BPA_FSS_Counterpart = ''
		select @Count = 0

		select @BPA_FSS_Counterpart = BPA_FSS_Counterpart
		from tbl_Cntrcts
		where CntrctNum = @OldContractNumber


		if @BPA_FSS_Counterpart is not null
		BEGIN
			if LEN(@BPA_FSS_Counterpart) > 0
			BEGIN
				exec UpdateBPALookup @UserLogin, @BPA_FSS_Counterpart, @NewContractNumber, @Count OUTPUT

				Select @error = @@ERROR
				IF @error <> 0 OR @Count = 0
				BEGIN
					select @errorMsg = 'Error updating tbl_CopyContractsLog for contract ' + @NewContractNumber
					GOTO ERROREXIT
				END	
			END
		END

		Update tbl_CopyContractsLog
			Set TotalBPALookupsCopied = @Count
		Where CopyContractLogId = @CopyContractLogId
			
		Select @error = @@ERROR
		IF @error <> 0
		BEGIN
			select @errorMsg = 'Error updating tbl_CopyContractsLog for contract ' + @NewContractNumber
			GOTO ERROREXIT
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


	