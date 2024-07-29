IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ProcessMissingContractsForScheduleNumber1')
	BEGIN
		DROP  Procedure  ProcessMissingContractsForScheduleNumber1
	END

GO

CREATE Proc [dbo].[ProcessMissingContractsForScheduleNumber1]
As

	Declare @naccmContractId int,
			@naccmContractNumber nvarchar(20),
			@errorMsg varchar(1000),
			@error int

	Declare Contracts_Cursor CURSOR For 
		Select Contract_Record_Id, CntrctNum
		From NAC_CM.dbo.tbl_Cntrcts
		Where Schedule_Number in (1,18,28,29,30,31,32,37,39,43,47,48)

	Open Contracts_Cursor
	FETCH NEXT FROM Contracts_Cursor
	INTO @naccmContractId,@naccmContractNumber

	WHILE @@FETCH_STATUS = 0
	BEGIN
		
		If exists (Select top 1 1 From DI_Contracts Where NACCMContractNumber = @naccmContractNumber
													OR	  NACCMContractId = @naccmContractId
				  )
		Begin
			Select @errorMsg = 'Exists'
		End
		Else
		Begin
			Insert into DI_Contracts
			(ContractNumber,NACCMContractNumber,NACCMContractId,ModificationStatusID,
			 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate)
			Select	
				@naccmContractNumber,@naccmContractNumber,@naccmContractId,0,
				'Re Extract',getdate(),user_name(),getdate()
			
			Select @error = @@error
			If @error <> 0
			Begin
				select @errorMsg = 'Error when inserting into Contracts table For ScheduleNumber1'

				Insert into DI_ContractStatus
				(ContractNumber,ErrorMessage,CreatedBy,CreationDate)
				Select
					@naccmContractNumber,@errorMsg,user_name(),getdate()	
			End		

		End
			
		Select @naccmContractId = null,@naccmContractNumber = null

		FETCH NEXT FROM Contracts_Cursor
		INTO @naccmContractId,@naccmContractNumber
	End
	CLose Contracts_Cursor
	DeAllocate Contracts_Cursor
