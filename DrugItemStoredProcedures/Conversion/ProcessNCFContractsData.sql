IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessNCFContractsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessNCFContractsData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[ProcessNCFContractsData]
As 


Declare @contractNumber nvarchar(20),
	@identity int,
	@error int,
	@errorMsg nvarchar(512),
	@naccmContractId int,
	@naccmContractNumber nvarchar(20),
	@ContractId int

-- Insert Contracts

	Declare Contracts_Cursor CURSOR For

	Select distinct ncf_cnt as cnt_no
	from ncfprice

	Order by CNT_NO

	Open Contracts_Cursor
	FETCH NEXT FROM Contracts_Cursor
	INTO @contractNumber

	WHILE @@FETCH_STATUS = 0
	BEGIN
		If exists (Select top 1 1 From DI_Contracts 
						where ContractNumber = @contractNumber
					)
		Begin
			select @errorMsg = 'Contract Exists do nothing'	

			Insert into DI_ContractStatus
			(ContractNumber,ErrorMessage,CreatedBy,CreationDate)
			Select
				@contractNumber,@errorMsg,user_name(),getdate()					
		End
		Else
		Begin
			Select @naccmContractId = Contract_Record_Id,@naccmContractNumber = CntrctNum
			From NAC_CM.dbo.tbl_Cntrcts
			Where CntrctNum = ltrim(rtrim(@contractNumber)) or CntrctNum = ltrim(rtrim(right(@contractNumber,5)))			

			Insert into DI_Contracts
			(ContractNumber,NACCMContractNumber,NACCMContractId,ModificationStatusID,CreatedBy,CreationDate,LastModifiedBy,
			 LastModificationDate)
			Select	
				@contractNumber,@naccmContractNumber,@naccmContractId,0,'Re Extract',getdate(),user_name(),getdate()

			select @identity = @@identity,@error = @@ERROR

			If @error <> 0 
			BEGIN
				select @errorMsg = 'Error when inserting into table For National contracts'

				Insert into DI_ContractStatus
				(ContractNumber,ErrorMessage,CreatedBy,CreationDate)
				Select
					@contractNumber,@errorMsg,user_name(),getdate()
			End
		End
		
		Select @naccmContractId = null,@naccmContractNumber = null

		FETCH NEXT FROM Contracts_Cursor
		INTO @contractNumber
	End
	CLose Contracts_Cursor
	DeAllocate Contracts_Cursor
