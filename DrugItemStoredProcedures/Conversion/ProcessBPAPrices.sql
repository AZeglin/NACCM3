IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ProcessBPAPrices')
	BEGIN
		DROP  Procedure  ProcessBPAPrices
	END

GO

CREATE Procedure ProcessBPAPrices
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
	@cnt_startdate datetime
	
	
	
Declare BPA_Cursor  Cursor For
	select [FSS Prices#cnt_no],ndc_1,ndc_2,ndc_3,[BPA Number: 797-FSSBPA-],
		[BPA Start Date:],cnt_stop,cnt_Start
	from BPA_Verification
	where [BPA Number: 797-FSSBPA-] is not null
	order by 1,2,3,4

Open BPA_Cursor
FETCH NEXT FROM BPA_Cursor
Into  @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date,@cnt_startdate

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

	Select @bpadrugitemid = DrugItemId
	From DI_DrugItems
	where ContractId = @bpacontractId
	and DrugItemNDCId = @drugItemNDCId
	
	Select @drugitemid = DrugItemId
	From DI_DrugItems
	where ContractId = @contractId
	and DrugItemNDCId = @drugItemNDCId	


	IF @bpadrugitemid is null or @drugitemid is null
	Begin
		Insert into DI_ItemBPAPriceStatus
		(ContractNumber,NDC_1,NDC_2,NDC_3,BPAContractNumber,BPAStartDate,BPAStopDate,
		 ContractId,BPAContractId,DrugItemId,BPADrugItemId,DrugItemPriceid,
		 ErrorMessage,CreatedBy,CreationDate
		)
		Select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date,
			@contractId,@bpacontractId,@drugitemid,@bpadrugitemid,null,
			'Error Drugitemid cannot be null',USER_NAME(),GETDATE()	
	End
	Else
	Begin
		Select @drugitempriceid = DrugitemPriceid
		From DI_DrugItemPrice
		Where DrugItemId = @drugitemid
		and IsFSS = 0 and IsBIG4 = 0
		
		Insert into DI_DrugItemPriceHistory
		(DrugItemPriceId,DrugItemId,DrugItemSubItemId,PriceId,PriceStartDate,PriceStopDate,Price,IsDeleted,IsTemporary,IsFSS,IsBIG4,
		 IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,
		 IsUSCG,AwardedFSSTrackingCustomerRatio,TrackingCustomerName,
		 CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
		 ModificationStatusId,Notes,CreatedBy,
		 CreationDate,LastModifiedBy,LastModificationDate)
		Select 
			DrugItemPriceId,DrugItemId,DrugItemSubItemId,PriceId,PriceStartDate,GETDATE(),Price,1,IsTemporary,IsFSS,
			IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,IsSVH1,IsSVH2,
			IsTMOP,IsUSCG,AwardedFSSTrackingCustomerRatio,TrackingCustomerName,
			CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
			ModificationStatusId,'BPA Transformation',
			'Re Extract',CreationDate,LastModifiedBy,'2011-04-27 18:00:00.000'
		from di_Drugitemprice
		where DrugItemPriceId = @drugitempriceid		
	
		If @error <> 0
		Begin
			Insert into DI_ItemBPAPriceStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,BPAContractNumber,BPAStartDate,BPAStopDate,
			 ContractId,BPAContractId,DrugItemId,BPADrugItemId,DrugItemPriceid,
			 ErrorMessage,CreatedBy,CreationDate
			)
			Select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date,
				@contractId,@bpacontractId,@drugitemid,@bpadrugitemid,@drugitempriceid,
				'Error when inserting into price history table',USER_NAME(),GETDATE()
		End	
		Else
		Begin 
			Update DI_DrugItemPrice
				set DrugItemId = @bpadrugitemid,
					PriceStartDate = 
						Case 
							when @bpa_start_date is not null then @bpa_start_date
								else @cnt_startdate
						End,
					PriceStopDate = @bpa_stop_date,
					CreatedBy  = 'BPA Transformation',
					LastModificationDate = '2011-04-27 18:00:00.000'
			where DrugItemPriceId = @drugitempriceid
			
			If @error <> 0
			Begin
				Insert into DI_ItemBPAPriceStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,BPAContractNumber,BPAStartDate,BPAStopDate,
				 ContractId,BPAContractId,DrugItemId,BPADrugItemId,DrugItemPriceid,
				 ErrorMessage,CreatedBy,CreationDate
				)
				Select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date,
					@contractId,@bpacontractId,@drugitemid,@bpadrugitemid,@drugitempriceid,
					'Error when updating price table',USER_NAME(),GETDATE()					
			End
			Else
			Begin
				Insert into DI_ItemBPAPriceStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,BPAContractNumber,BPAStartDate,BPAStopDate,
				 ContractId,BPAContractId,DrugItemId,BPADrugItemId,DrugItemPriceid,
				 ErrorMessage,CreatedBy,CreationDate
				)
				Select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date,
					@contractId,@bpacontractId,@drugitemid,@bpadrugitemid,@drugitempriceid,
					'Inserted',USER_NAME(),GETDATE()					
			End
		End								
	End

	Select @contractId = null, @bpacontractId = null,@drugitemid = null,@bpadrugitemid = null,
		@drugitempriceid = null,@drugItemNDCId = null
	
	Fetch next  FROM BPA_Cursor
	Into  @cnt_no,@ndc_1,@ndc_2,@ndc_3,@bpa_cnt_no,@bpa_start_date,@bpa_stop_date,@cnt_startdate
	
End
Close BPA_Cursor
DeAllocate BPA_Cursor

