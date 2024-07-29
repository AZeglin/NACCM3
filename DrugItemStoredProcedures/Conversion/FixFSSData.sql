IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FixFSSData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FixFSSData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[FixFSSData]
As	

	Declare 
			@ndc_1 char(5),
			@ndc_2 char(4),
			@ndc_3 char(2),
			@n char(1),
			@cnt_no nvarchar(20),
			@newndc1 char(5),
			@newndc2 char(4),
			@newndc3 char(2),
			@newn char(1),
			@newcnt nvarchar(20),
			@statusId int,
			@errormsg nvarchar(500),
			@drugItemNDCId int,
			@contractId int,
			@ndcwithNid int,
			@newcontract_Id int,
			@newdrugitemndc_id int,
			@newNDCwithN_id int,
			@error  int,
			@identity int
		
	Declare FSSData_Cursor CURSOR For
		select a.DrugitemId,c.ContractNumber,b.DrugItemNDCId,b.FdaAssignedLabelerCode,
				b.ProductCode,b.PackageCode,a.ContractId
		from di_drugitems a
		join di_Drugitemndc b
		on a.drugitemndcid = b.drugitemndcid
		join di_contracts c
		on a.contractid = c.contractid
		order by 2

	Open FSSData_Cursor
	FETCH NEXT FROM FSSData_Cursor
	INTO @statusId,@cnt_no,@drugItemNDCId,@ndc_1,@ndc_2,@ndc_3,@contractId

	WHILE @@FETCH_STATUS = 0
	BEGIN

		exec CheckIfNDCChangedProc1 
			@ndc_1,
			@ndc_2,
			@ndc_3,
			@cnt_no,
			@ndc1_new = @newndc1 OUTPUT,
			@ndc2_new = @newndc2 OUTPUT,
			@ndc3_new = @newndc3 OUTPUT,
			@cnt_new  = @newcnt OUTPUT,
			@newContractId = @newcontract_Id OUTPUT,
			@newDrugItemNDCId = @newdrugitemndc_id OUTPUT


		If @newndc1 is null
		Begin
			select @errorMsg = 'Not found'
/*			Insert into DI_FSSDataFixStatus
			(DrugItemId,ContractNumber,NDC_1,NDC_2,NDC_3,N,NewContractNumber,NewNDC_1,NewNDC_2,NewNDC_3,
				NewN,ErrorMessage,CreatedBy,CreationDate
			)
			Select @statusId,@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@newcnt,@newndc1,@newndc2,@newndc3,
					@newn,@errorMsg,user_name(),getdate()	
*/

		End
		Else
		Begin
			select @errorMsg = 'Found'

			Insert into DI_FSSDataFixStatus
			(DrugItemId,ContractId,ContractNumber,DrugItemNDCId,NDC_1,NDC_2,NDC_3,
				NewContractId,NewContractNumber,NewDrugItemNDCId,NewNDC_1,NewNDC_2,NewNDC_3,
				ErrorMessage,CreatedBy,CreationDate
			)
			Select @statusId,@contractId,@cnt_no,@drugItemNDCId,@ndc_1,@ndc_2,@ndc_3,@newcontract_Id,
					@newcnt,@newdrugitemndc_id,@newndc1,@newndc2,@newndc3,
					@errorMsg,user_name(),getdate()	
		
			select @identity = @@IDENTITY
		
			If exists (Select top 1 1 from Di_DrugItems
						where ContractId = @newcontract_Id and DrugItemNDCId = @newdrugitemndc_id
					  )
			Begin
				select @errorMsg = 'New Item Exists'
				
				update DI_FSSDataFixStatus
					set ErrorMessage = @errorMsg
				where FSSDataFixStatusId = @identity
		
			End
			Else
			Begin					  
				Update Di_DrugItems
				Set ContractId = @newcontract_Id,
					DrugItemNDCId = @newdrugitemndc_id
				Where DrugItemId = @statusId

				select @error = @@ERROR
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error when Updating into DI_DrugItems table'

					Insert into DI_FSSDataFixStatus
					(	DrugItemId,ContractId,ContractNumber,DrugItemNDCId,NDC_1,NDC_2,NDC_3,
						NewContractId,NewContractNumber,NewDrugItemNDCId,NewNDC_1,NewNDC_2,NewNDC_3,
						ErrorMessage,CreatedBy,CreationDate
					)
					Select 
						@statusId,@contractId,@cnt_no,@drugItemNDCId,@ndc_1,@ndc_2,@ndc_3,@newcontract_Id,
						@newcnt,@newdrugitemndc_id,@newndc1,@newndc2,@newndc3,
						@errorMsg,user_name(),getdate()	
		
				End
			End
		End


		Select @newndc1 = null,@newndc2 = null,@newndc3 =null,@newcnt = null,
				@newcontract_Id = null, @newdrugitemndc_id = null,
				@errorMsg = null
		

		FETCH NEXT FROM FSSData_Cursor
		INTO @statusId,@cnt_no,@drugItemNDCId,@ndc_1,@ndc_2,@ndc_3,@contractId
	End
	Close FSSData_Cursor
	DeAllocate FSSData_Cursor
