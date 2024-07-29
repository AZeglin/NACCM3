IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessNCFFSSContractsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessNCFFSSContractsData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[ProcessNCFFSSContractsData]
As 

Declare @contractNumber nvarchar(20),
		@naccmContractNumber nvarchar(20),
	@identity int,
	@error int,
	@errorMsg nvarchar(512),
	@naccmContractId int

-- Insert Contracts

	Declare Contracts_Cursor CURSOR For
		Select ContractNumber,NACCMContractNumber
		From NCFFSSContractsLookUp

	Open Contracts_Cursor
	FETCH NEXT FROM Contracts_Cursor
	INTO @contractNumber,@naccmContractNumber

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Select @naccmContractId = Contract_Record_Id
		From AMMHINSQL1.NAC_CM.dbo.tbl_Cntrcts
		Where CntrctNum = @naccmContractNumber		

		If @naccmContractId is null and @contractNumber = 'A000A-0000A'
		Begin
			select @errorMsg = 'Contract already exists in Di_Contracts'
		End
		Else If @naccmContractId is null
		Begin
			Select @errorMsg = 'NACCM Contract ID cannot be found when processing NCF FSS Contracts'
			
			Insert into DI_ContractStatus
			(ContractNumber,ErrorMessage,CreatedBy,CreationDate)
			Select @contractNumber,@errorMsg,user_name(),getdate()
		End
		Else
		Begin
			If exists (Select top 1 1 From Di_Contracts Where ContractNumber = @contractNumber
														or NACCMContractId = @naccmContractId
					  )
			Begin
					update Di_Contracts
					Set NACCMContractId = @naccmContractId,NACCMContractNumber=ltrim(rtrim(@naccmContractNumber))
					Where ContractNumber = @contractNumber

					select @errorMsg = 'Contract already exists in Di_Contracts'
			End
			Else
			Begin
				Insert into DI_Contracts
				(ContractNumber,NACCMContractNumber,NACCMContractId,ModificationStatusID,CreatedBy,CreationDate,LastModifiedBy,
				 LastModificationDate)
				Select	
					ltrim(rtrim(@contractNumber)),ltrim(rtrim(@naccmContractNumber)),@naccmContractId,0,user_name(),getdate(),user_name(),getdate()

				select @identity = @@identity,@error = @@ERROR

				If @error <> 0 
				BEGIN
					select @errorMsg = 'Error when inserting into Contracts table'

					Insert into DI_ContractStatus
					(ContractNumber,ErrorMessage,CreatedBy,CreationDate)
					Select
						@contractNumber,@errorMsg,user_name(),getdate()
				End
			End
		End


		Set @naccmContractId = null

		FETCH NEXT FROM Contracts_Cursor
		INTO @contractNumber,@naccmContractNumber
	End
	CLose Contracts_Cursor
	DeAllocate Contracts_Cursor

