IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopyBPAItemsForContract') 
	BEGIN
		DROP  Procedure  CopyBPAItemsForContract
	END

GO

CREATE Procedure CopyBPAItemsForContract
(
	@CopyContractLogId int,
	@OldContractNumber nvarchar(50),
	@NewContractNumber nvarchar(50),
	@EffectiveDate datetime,
	@ExpirationDate datetime,
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
		From tbl_BPA_PriceList
		Where CntrctNum = @OldContractNumber
		and removed = 0
		
		IF @count = 0
		BEGIN
			Update tbl_CopyContractsLog
				Set TotalBPAPriceListItems = 0
			Where CopyContractLogId = @CopyContractLogId
			
			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error updating tbl_CopyContractsLog for contract ' + @NewContractNumber
				GOTO ERROREXIT
			END			
		END
		ELSE
		BEGIN
			Insert Into tbl_BPA_PriceList
				(CntrctNum,[Description],[BPA/BOA Price],FSSLogNumber,Removed,DateEffective,ExpirationDate,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate)
			Select 
				 @NewContractNumber,[Description],[BPA/BOA Price],FSSLogNumber,Removed,@EffectiveDate,@ExpirationDate,@UserLogin,GETDATE(),@UserLogin,GETDATE()
			From tbl_BPA_PriceList
			Where CntrctNum = @OldContractNumber
			And Removed = 0

			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting tbl_BPA_PriceList for contract ' + @NewContractNumber
				GOTO ERROREXIT
			END	

			Update tbl_CopyContractsLog
				Set TotalBPAPriceListItems = @count
			Where CopyContractLogId = @CopyContractLogId
			
			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error updating tbl_CopyContractsLog for contract ' + @NewContractNumber
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


	