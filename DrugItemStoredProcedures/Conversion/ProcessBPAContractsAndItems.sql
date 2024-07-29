IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ProcessBPAContractsAndItems')
	BEGIN
		DROP  Procedure  ProcessBPAContractsAndItems
	END

GO

CREATE Procedure ProcessBPAContractsAndItems
As

Declare
	@cnt_no nvarchar(20),
	@contractId int,
	@bpacontractid int,
	@drugItemNDCId int,	
	@drugitemid int,
	@bpadrugitemid int,
	@identity int,
	@ndc_1 char(5),
	@ndc_2 char(4),
	@ndc_3 char(2),
	@bpa_cnt_no nvarchar(20),	
	@bpa_start_date datetime,
	@bpa_stop_date datetime,
	@parentdrugitemid int,
	@error int,
	@drugitempriceid int,
	@errorMsg nvarchar(100)
	
	
	
Declare BPA_Cursor  Cursor For
	select [FSS Prices#cnt_no],ndc_1,ndc_2,ndc_3,[BPA Number: 797-FSSBPA-],
		[BPA Start Date:],cnt_stop
	from BPA_Verification
	where [BPA Number: 797-FSSBPA-] is not null
	order by 1,2,3,4

Open BPA_Cursor
FETCH NEXT FROM BPA_Cursor
Into  @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date

WHILE @@FETCH_STATUS = 0
BEGIN

	Select @contractId = Contractid
	From DI_Contracts 
	where NACCMContractNumber = @cnt_no
	
	Select @drugItemNDCId = DrugItemNDCId
	From Di_DrugItemNDC
	Where FDAAssignedLabelerCode = @ndc_1
	And ProductCode = @ndc_2
	And PackageCode = @ndc_3
	
	Select @bpacontractId = Contractid
	From DI_Contracts 
	where NACCMContractNumber = @bpa_cnt_no	

	If exists (Select top 1 1 from DI_DrugItems where contractid = @contractId 
				and drugitemndcid = @drugItemNDCId 
			   )
	Begin	
		Select @drugitemid = Drugitemid
		from DI_DrugItems where contractid = @contractId 
		and drugitemndcid = @drugItemNDCId 	
		
		
		If exists (Select top 1 1 from DI_DrugItems where contractid = @bpacontractId 
					and drugitemndcid = @drugItemNDCId 
					)
		Begin
			Select @errorMsg = 'Do Nothing'
		End
		Else
		Begin
			Insert into DI_DrugItems 
			(ContractId,DrugItemNDCId,HistoricalNValue,PackageDescription,Generic,TradeName,
				DiscontinuationDate,DiscontinuationEnteredDate,DateEnteredMarket,Covered,
				PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,
				DualPriceDesignation,ExcludeFromExport,ParentDrugItemId,LastModificationType,
				ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate	
			)
			Select 
				@bpacontractId,DrugItemNDCId,HistoricalNValue,PackageDescription,Generic,TradeName,
				DiscontinuationDate,DiscontinuationEnteredDate,DateEnteredMarket,Covered,
				PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,
				DualPriceDesignation,ExcludeFromExport,@drugitemid,LastModificationType,
				ModificationStatusId,'BPA Transformation',GETDATE(),USER_NAME(),'2011-04-27 18:00:00.000'
			From DI_DrugItems 
			where DrugItemId = @drugitemid
		
			select @bpadrugitemid = @@identity,@error = @@ERROR		
			If @error <> 0
			Begin
				Insert into DI_ItemBPAItemStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,BPAContractNumber,BPAStartDate,BPAStopDate,
				 ContractId,BPAContractId,DrugItemId,BPADrugItemId,
				ErrorMessage,CreatedBy,CreationDate
				)
				Select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date,
				@contractId,@bpacontractId,@drugitemid,@bpadrugitemid,
				'Error when inserting into items table',USER_NAME(),GETDATE()
			End
			Else
			Begin
				Insert into DI_ItemBPAItemStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,BPAContractNumber,BPAStartDate,BPAStopDate,
				 ContractId,BPAContractId,DrugItemId,BPADrugItemId,
				ErrorMessage,CreatedBy,CreationDate
				)
				Select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date,
				@contractId,@bpacontractId,@drugitemid,@bpadrugitemid,
				'Inserted',USER_NAME(),GETDATE()			
			End
		End
	End
	Else
	Begin
		Insert into DI_ItemBPAItemStatus
		(ContractNumber,NDC_1,NDC_2,NDC_3,BPAContractNumber,BPAStartDate,BPAStopDate,
		 ContractId,BPAContractId,DrugItemId,BPADrugItemId,
		ErrorMessage,CreatedBy,CreationDate
		)
		Select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date,
		@contractId,@bpacontractId,@drugitemid,@bpadrugitemid,
		'Error Contract Id or NDC Id not found',USER_NAME(),GETDATE()	
	End
	
	Select @contractId = null, @bpacontractId = null,@drugitemid = null,@bpadrugitemid = null,
		@drugItemNDCId = null
	
	Fetch next  FROM BPA_Cursor
	Into  @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date
	
End
Close BPA_Cursor
DeAllocate BPA_Cursor