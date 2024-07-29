IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ProcessNCFItemPriceData')
	BEGIN
		DROP  Procedure  ProcessNCFItemPriceData
	END

GO

CREATE Procedure ProcessNCFItemPriceData
As

Declare @cnt_no nvarchar(20),
	@contractId int,
	@drugItemNDCId int,
	@identity int,
	@error int,
	@errorMsg nvarchar(512),
	@ndc_1 char(5),
	@ndc_2 char(4),
	@ndc_3 char(2),
	@n char(1),
	@cnt_start datetime,
	@cnt_stop datetime,
	@price decimal(9,2),
	@edate datetime,
	@chg_date datetime,
	@pv_chg_dat datetime,
	@drugitemid int,
	@priceId int,
	@NDCWithNId int,
	@ncf_cnt nvarchar(20),
	@ParentFSScontractId int,
	@Va bit,
	@Dod bit,
	@Bop bit,
	@Hhs bit,
	@Ihs bit,
	@Dihs bit,
	@Svh bit,
	@Svh1 bit,
	@Svh2 bit,
	@Ihs2 bit,
	@fhcc bit,
	@subitemid int,
	@drugitempriceid int
	

	Declare NCFPrice_Cursor CURSOR For
		Select NDC_1,NDC_2,NDC_3,n,ncf_cnt,price,cnt_start,cnt_stop,Va,Dod,Bop,Hhs,Ihs,Dihs,svh,Svh1,
				Svh2,Ihs2,fhcc
		From NCfprice
		where (len(N) = 0 or N is null)
		Order by ncf_cnt,NDC_1,NDC_2,NDC_3,cnt_start,cnt_stop		

	Open NCFPrice_Cursor
	FETCH NEXT FROM NCFPrice_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@price,@cnt_start,@cnt_stop,@Va,@Dod,@Bop,@Hhs,@Ihs,@Dihs,
		 @svh,@Svh1,@Svh2,@Ihs2,@fhcc

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Select @drugitemid = Null,@contractid = Null, @drugItemNDCId = Null,
				@subitemid = null,@drugitempriceid = null

		Select @contractId = ContractId
		From DI_Contracts
		Where ContractNumber = @cnt_no

		Select @drugItemNDCId = DrugItemNDCId
		From Di_DrugItemNDC
		Where FDAAssignedLabelerCode = @ndc_1
		And ProductCode = @ndc_2
		And PackageCode = @ndc_3

		If exists (Select top 1 1 from DI_DrugItems where contractid = @contractId 
					and drugitemndcid = @drugItemNDCId 
				   )
		Begin
			Select @drugitemid = Drugitemid
			from DI_DrugItems where contractid = @contractId 
			and drugitemndcid = @drugItemNDCId 


			Insert into DI_DrugItemPrice
			(DrugItemId,HistoricalNValue,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,
			 IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,
			 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,IsFHCC,AwardedFSSTrackingCustomerRatio,
			TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
			 ModificationStatusId,CreatedBy,CreationDate,
			 LastModifiedBy,LastModificationDate
			)
			Select
				@drugitemid,null,null,@cnt_start,@cnt_stop,@price,0,0,0,@Va,@Bop,0,@Dod,@Hhs,
/*				Case 
					when @Va = 'T' then 1
					when @Va = 'F' then 0
					Else 0
				End ,
				Case 
					when @Bop = 'T' then 1
					when @Bop = 'F' then 0
					Else 0
				End ,
				0,
				Case 
					when @Dod = 'T' then 1
					when @Dod = 'F' then 0
					Else 0
				End ,
				Case 
					when @Hhs = 'T' then 1
					when @Hhs = 'F' then 0
					Else 0
				End ,
				Case 
					when @Ihs = 'T' then 1
					when @Ihs = 'F' then 0
					Else 0
				End ,
				Case 
					when @Ihs2 = 'T' then 1
					when @Ihs2 = 'F' then 0
					Else 0
				End ,
				Case 
					when @Dihs = 'T' then 1
					when @Dihs = 'F' then 0
					Else 0
				End ,				
				0,0,
				Case 
					when @Svh = 'T' then 1
					when @Svh = 'F' then 0
					Else 0
				End ,				
				Case 
					when @Svh1 = 'T' then 1
					when @Svh1 = 'F' then 0
					Else 0
				End ,
				Case 
					when @Svh2 = 'T' then 1
					when @Svh2 = 'F' then 0
					Else 0
				End ,
*/				
				@Ihs,@Ihs2,@Dihs,0,0,@Svh,@Svh1,@Svh2,0,0,@fhcc,null,null,null,0,'I',0,
				'Re Extract',
				getdate(),
				user_name(),
				getdate()

			select @drugitempriceid = @@identity,@error = @@ERROR
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error when inserting into Price table'
				Insert into DI_ItemNCFPriceStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
				ErrorMessage,CreatedBy,CreationDate)
				Select
				@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
				@errorMsg,user_name(),getdate()
			End
			Else
			Begin
				Insert into DI_ItemNCFPriceStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
				DrugitemId,DrugitemPriceId,ErrorMessage,CreatedBy,CreationDate)
				Select
				@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
				@drugitemid,@drugitempriceid,'Inserted',user_name(),getdate()			
			End
		End	
		Else
		Begin
			select @errorMsg = 'NDC Price Item not found'

			Insert into DI_ItemNCFPriceStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
			ErrorMessage,CreatedBy,CreationDate)
			Select
			@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
			@errorMsg,user_name(),getdate()
		End			

		FETCH NEXT FROM NCFPrice_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@price,@cnt_start,@cnt_stop,@Va,@Dod,@Bop,@Hhs,@Ihs,@Dihs,
			 @svh,@Svh1,@Svh2,@Ihs2,@fhcc
	End
	Close NCFPrice_Cursor
	DeAllocate NCFPrice_Cursor


	Declare NCFPrice_Cursor CURSOR For
		Select NDC_1,NDC_2,NDC_3,n,ncf_cnt,price,cnt_start,cnt_stop,Va,Dod,Bop,Hhs,Ihs,Dihs,svh,Svh1,
				Svh2,Ihs2,fhcc
		From NCfprice
		where (len(N) > 0)
		Order by ncf_cnt,NDC_1,NDC_2,NDC_3,cnt_start,cnt_stop		

	Open NCFPrice_Cursor
	FETCH NEXT FROM NCFPrice_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@price,@cnt_start,@cnt_stop,@Va,@Dod,@Bop,@Hhs,@Ihs,@Dihs,
		 @svh,@Svh1,@Svh2,@Ihs2,@fhcc

	WHILE @@FETCH_STATUS = 0
	BEGIN

		Select @drugitemid = Null,@contractid = Null, @drugItemNDCId = Null,
				@subitemid = null,@drugitempriceid = null

		Select @contractId = ContractId
		From DI_Contracts
		Where ContractNumber = @cnt_no

		Select @drugItemNDCId = DrugItemNDCId
		From Di_DrugItemNDC
		Where FDAAssignedLabelerCode = @ndc_1
		And ProductCode = @ndc_2
		And PackageCode = @ndc_3

		If exists (Select top 1 1 from DI_DrugItems a Join DI_DrugItemsubItems b
					on a.drugitemid = b.drugitemid
					where a.contractid = @contractId 
					and a.drugitemndcid = @drugItemNDCId 
					and b.SubItemIdentifier = @n
				   )
		Begin
			Select @drugitemId = a.Drugitemid, @subitemid = b.DrugItemSubItemId
			from DI_DrugItems a 
			Join DI_DrugItemsubItems b
			on a.drugitemid = b.drugitemid
			where a.contractid = @contractId 
			and a.drugitemndcid = @drugItemNDCId 
			and b.SubItemIdentifier = @n


			Insert into DI_DrugItemPrice
			(DrugItemId,DrugItemSubItemId,HistoricalNValue,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,
			 IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,
			 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,IsFHCC,AwardedFSSTrackingCustomerRatio,
			TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
			 ModificationStatusId,CreatedBy,CreationDate,
			 LastModifiedBy,LastModificationDate
			)
			Select
				@drugitemid,@subitemid,@n,null,@cnt_start,@cnt_stop,@price,0,0,0,@Va,@Bop,0,@Dod,@Hhs,
/*				Case 
					when @Va = 'T' then 1
					when @Va = 'F' then 0
					Else 0
				End ,
				Case 
					when @Bop = 'T' then 1
					when @Bop = 'F' then 0
					Else 0
				End ,
				0,
				Case 
					when @Dod = 'T' then 1
					when @Dod = 'F' then 0
					Else 0
				End ,
				Case 
					when @Hhs = 'T' then 1
					when @Hhs = 'F' then 0
					Else 0
				End ,
				Case 
					when @Ihs = 'T' then 1
					when @Ihs = 'F' then 0
					Else 0
				End ,
				Case 
					when @Ihs2 = 'T' then 1
					when @Ihs2 = 'F' then 0
					Else 0
				End ,
				Case 
					when @Dihs = 'T' then 1
					when @Dihs = 'F' then 0
					Else 0
				End ,				
				0,0,
				Case 
					when @Svh = 'T' then 1
					when @Svh = 'F' then 0
					Else 0
				End ,				
				Case 
					when @Svh1 = 'T' then 1
					when @Svh1 = 'F' then 0
					Else 0
				End ,
				Case 
					when @Svh2 = 'T' then 1
					when @Svh2 = 'F' then 0
					Else 0
				End ,
*/				
				@Ihs,@Ihs2,@Dihs,0,0,@Svh,@Svh1,@Svh2,0,0,@fhcc,null,null,null,0,'I',0,
				'Re Extract',
				getdate(),
				user_name(),
				getdate()

			select @drugitempriceid = @@identity,@error = @@ERROR
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error when inserting into Price table'
				Insert into DI_ItemNCFPriceStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
				ErrorMessage,CreatedBy,CreationDate)
				Select
				@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
				@errorMsg,user_name(),getdate()
			End
			Else
			Begin
				Insert into DI_ItemNCFPriceStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
				DrugitemId,DrugitemPriceId,ErrorMessage,CreatedBy,CreationDate)
				Select
				@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
				@drugitemid,@drugitempriceid,'Inserted',user_name(),getdate()			
			End
		End	
		Else
		Begin
			select @errorMsg = 'NDC Price Item not found'

			Insert into DI_ItemNCFPriceStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
			ErrorMessage,CreatedBy,CreationDate)
			Select
			@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
			@errorMsg,user_name(),getdate()
		End			

		FETCH NEXT FROM NCFPrice_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@price,@cnt_start,@cnt_stop,@Va,@Dod,@Bop,@Hhs,@Ihs,@Dihs,
			 @svh,@Svh1,@Svh2,@Ihs2,@fhcc
	End
	Close NCFPrice_Cursor
	DeAllocate NCFPrice_Cursor
