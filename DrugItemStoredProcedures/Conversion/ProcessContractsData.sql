IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessContractsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessContractsData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[ProcessContractsData]
As 


Declare @contractNumber nvarchar(20),
	@identity int,
	@error int,
	@errorMsg nvarchar(512),
	@naccmContractId int,
	@naccmContractNumber nvarchar(20),
	@ContractId int



	Declare Contracts_Cursor CURSOR For
	Select distinct a.CNT_NO From
	(Select distinct cnt_no 
	from fssdata
	union
	Select distinct cnt_new as cnt_no
	from ndclink
	union
	Select distinct cnt_old as cnt_no
	from ndclink
	) a
	Order by a.CNT_NO

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
				select @errorMsg = 'Error when inserting into Contracts table'

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
