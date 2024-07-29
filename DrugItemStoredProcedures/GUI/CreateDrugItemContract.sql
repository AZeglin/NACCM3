IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CreateDrugItemContract')
	BEGIN
		DROP  Procedure  CreateDrugItemContract
	END

GO

Create Proc [dbo].[CreateDrugItemContract]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(50),
@ContractRecordId int,
@ModificationStatusId int,
@ScheduleNumber int,
@ParentContractNumber nvarchar(50) = null,
@ContractId int OUTPUT
)
As

	Declare @error int,
			@errorMsg nvarchar(1000),
			@rowCount int,
			@UserName nvarchar(120),
			@NormalizedContractNumber nvarchar(20),
			@ParentFSSContractId int


	Begin Tran
	
		EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @UserName OUTPUT 

		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error getting current user login '
			goto ERROREXIT
		END

		If len(@ContractNumber) = 0 or @ContractRecordId is null or @ContractNumber is null
		begin
			select @errorMsg = 'Contract Number cannot be null '
			goto ERROREXIT
		End

		If exists (Select top 1 1 From DI_Contracts where NACCMContractNumber = @ContractNumber
													OR	  NACCMContractId = @ContractRecordId
				  )
		Begin
			select @errorMsg = 'Contract Number: ' + @ContractNumber + ' already exists '
			goto ERROREXIT
		End


		-- if this is a BPA, a parent contract number should be provided

		select @ParentFSSContractId = null

		if @ParentContractNumber is not null
		BEGIN
			select @ParentFSSContractId = ContractId
			from DI_Contracts where ContractNumber = @ParentContractNumber

			Select @error = @@error, @rowCount = @@ROWCOUNT

			IF 	@error <> 0 or @rowCount <> 1
			Begin
				select @errorMsg = 'Error looking up parent contract number: ' + @ParentContractNumber + ' from DI_Contracts.'
				goto ERROREXIT
			End	
		END

		-- changes to presume contract numbers have prefix
		-- select @NormalizedContractNumber = dbo.NormalizeContractNumberForDrugItemContractsFunction( @ContractNumber, @ScheduleNumber )
		
		Insert into DI_Contracts
		(
			ContractNumber,NACCMContractNumber,NACCMContractId,ParentFSSContractId,ModificationStatusID,
			CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
		)
		Select
			@ContractNumber,@ContractNumber,@ContractRecordId,@ParentFSSContractId,@ModificationStatusId,
			@UserName,getdate(),@UserName,getdate()

		--	@NormalizedContractNumber,@ContractNumber,@ContractRecordId,null,@ModificationStatusId,
		--	@UserName,getdate(),@UserName,getdate()

		Select @ContractId = @@identity,@error = @@error
		IF 	@error <> 0 or @ContractId < 1
		Begin
			select @errorMsg = 'Error inserting contract number: ' + @ContractNumber + ' into DI_Contracts'
			goto ERROREXIT
		End	
	
		
goto OKEXIT

 
ERROREXIT:

	  raiserror( @errorMsg, 16, 1 )
	  if @@TRANCOUNT > 1
	  BEGIN
			COMMIT TRANSACTION
	  END
	  Else if @@TRANCOUNT = 1
	  BEGIN
			/* only rollback iff this the highest level */
			ROLLBACK TRANSACTION
	  END

	  RETURN( -1 )

OKEXIT:
	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )	

