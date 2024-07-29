IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessContractNumberChange]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessContractNumberChange]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[ProcessContractNumberChange]
As 

Declare 
	@ndc1_new char(5),
	@ndc2_new char(4),
	@ndc3_new char(2),
	@n_new char(1),
	@cnt_new char(11),
	@ndc1_old char(5),
	@ndc2_old char(4),
	@ndc3_old char(2),
	@n_old char(1),
	@cnt_old char(11),
	@eff_date datetime,
	@edate datetime,
	@newContractId int,
	@oldContractId int,
	@newDrugItemNDCId int,
	@oldDrugItemNDCId int,
	@newNDCWithNId int,
	@oldNDCWithNId int,
	@identity int,
	@error int,
	@errorMsg nvarchar(512),
	@olddrugitemId int,
	@olddrugitemsubitemid int,
	@newdrugitemId int,
	@newdrugitemsubitemid int	
	

-- Insert Contracts

	Declare NDC_Cursor CURSOR For
	Select ndc1_new ,ndc2_new ,ndc3_new ,n_new,cnt_new,
		ndc1_old ,ndc2_old ,ndc3_old ,n_old,cnt_old,eff_date,edate
	from ndclink
	
	Open NDC_Cursor
	FETCH NEXT FROM NDC_Cursor
	INTO @ndc1_new ,@ndc2_new ,@ndc3_new ,@n_new,@cnt_new,
		@ndc1_old ,@ndc2_old ,@ndc3_old ,@n_old,@cnt_old,@eff_date,@edate

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Select @oldContractId = ContractId
		From DI_Contracts
		Where ContractNumber = @cnt_old

		Select @newContractId = ContractId
		From DI_Contracts
		Where ContractNumber = @cnt_new

		Select @oldDrugItemNDCId = DrugItemNDCId
		From di_drugitemndc
		Where FdaAssignedLabelerCode = @ndc1_old
		And ProductCode = @ndc2_old
		And PackageCode =@ndc3_old

		Select @newDrugItemNDCId = DrugItemNDCId
		From DI_DrugItemNDC
		Where FdaAssignedLabelerCode = @ndc1_new
		And ProductCode = @ndc2_new
		And PackageCode = @ndc3_new

		IF @newContractId is null or @newDrugItemNDCId is null
		Begin
			select @errorMsg = 'Error new Contract Id or NDC Id cannot be null'

			Insert into DI_ContractNDCNumberChangeStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,N,
			 NewContractNumber,NewNDC_1,NewNDC_2,NewNDC_3,NewN,
			 ErrorMessage,CreatedBy,CreationDate)
			Select
				@cnt_old,@ndc1_old,@ndc2_old,@ndc3_old,@n_old,
				@cnt_new,@ndc1_new,@ndc2_new,@ndc3_new,@n_new,
				@errorMsg,user_name(),getdate()
		End
		Else If @oldContractId is null or @oldDrugItemNDCId is null 
		Begin
			select @errorMsg = 'Error Old Contract Id or NDC Id cannot be null'

			Insert into DI_ContractNDCNumberChangeStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,N,
			 NewContractNumber,NewNDC_1,NewNDC_2,NewNDC_3,NewN,
			 ErrorMessage,CreatedBy,CreationDate)
			Select
				@cnt_old,@ndc1_old,@ndc2_old,@ndc3_old,@n_old,
				@cnt_new,@ndc1_new,@ndc2_new,@ndc3_new,@n_new,
				@errorMsg,user_name(),getdate()
		End
		Else
		Begin
			If LEN(@n_old) > 0
			Begin
				Select @olddrugitemId = a.drugitemid,
						@olddrugitemsubitemid = drugitemsubitemid
				From DI_DrugItems a
				join DI_DrugItemSubItems b
				on a.DrugItemId = b.drugitemid
				where a.ContractId = @oldContractId
				and a.DrugItemNDCId = @oldDrugItemNDCId
				and b.SubItemIdentifier = @n_old				
			End		

			If LEN(@n_new) > 0
			Begin
				Select @newdrugitemId = a.drugitemid,
						@newdrugitemsubitemid = drugitemsubitemid
				From DI_DrugItems a
				join DI_DrugItemSubItems b
				on a.DrugItemId = b.drugitemid
				where a.ContractId = @newContractId
				and a.DrugItemNDCId = @newDrugItemNDCId
				and b.SubItemIdentifier = @n_new
			End		
						
			Insert into DI_ContractNDCNumberChange
			(NewContractId,NewDrugItemNDCId,NewDrugItemId,NewDrugItemSubItemId,
				NewHistoricalNValue,
				OldContractId,OldDrugItemNDCId,OldDrugItemId,OldDrugItemSubItemId,
				OldHistoricalNValue,
				ModificationId,EffectiveDate,EndDate,LastModifiedBy,LastModificationDate)
			Select @newContractId,@newDrugItemNDCId,
					@newdrugitemId,@newdrugitemsubitemid,
					Case 
						When len(@n_new) > 0 then @n_new
						Else Null
					End,
					@oldContractId,@oldDrugItemNDCId,
					@olddrugitemId,@olddrugitemsubitemid,
					Case 
						When len(@n_old) > 0 then @n_old
						Else Null
					End,					
					0,@eff_date,@edate,'Initial run',getdate()

			select @identity = @@identity,@error = @@ERROR

			If @error <> 0 
			BEGIN
				select @errorMsg = 'Error when inserting into DI_ContractNDCNumberChange table'

				Insert into DI_ContractNDCNumberChangeStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,N,
				 NewContractNumber,NewNDC_1,NewNDC_2,NewNDC_3,NewN,
				 ErrorMessage,CreatedBy,CreationDate)
				Select
					@cnt_old,@ndc1_old,@ndc2_old,@ndc3_old,@n_old,
					@cnt_new,@ndc1_new,@ndc2_new,@ndc3_new,@n_new,
					@errorMsg,user_name(),getdate()

			End				
		End


		Select 	@newContractId=null,@oldContractId=null,@newDrugItemNDCId=null,@oldDrugItemNDCId=null
				
		FETCH NEXT FROM NDC_Cursor
		INTO @ndc1_new ,@ndc2_new ,@ndc3_new ,@n_new,@cnt_new,
			@ndc1_old ,@ndc2_old ,@ndc3_old ,@n_old,@cnt_old,@eff_date,@edate
	End
	CLose NDC_Cursor
	DeAllocate NDC_Cursor
