IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessFCPItemsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessFCPItemsData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[ProcessFCPItemsData]
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
	@newndc1 char(5) ,
	@newndc2 char(4) ,
	@newndc3 char(2) ,
	@newn char(1) ,
	@newcnt nvarchar(20),
	@priceId int,
	@NDCWithNId int,
	@subitemid int,
	@drugitempriceid int,
	@temp bit



	Declare FCPPrice_Cursor CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,cnt_start,cnt_stop,price,edate,chg_date,pv_chg_dat,temp
		From FCPPrice where (len(N) = 0 or N is null)
		Order by CNT_NO, NDC_1,NDC_2,NDC_3,N,cnt_start,cnt_stop

	Open FCPPrice_Cursor
	FETCH NEXT FROM FCPPrice_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@cnt_start,@cnt_stop,@price,@edate,@chg_date,@pv_chg_dat,@temp

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
			Select @drugitemId = DrugItemId
			from DI_DrugItems where contractid = @contractId 
			and drugitemndcid = @drugItemNDCId


			Insert into DI_DrugItemPrice
			(DrugItemId,DrugItemSubItemId,HistoricalNValue,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,
			 IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,
			 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,AwardedFSSTrackingCustomerRatio,
			 TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
			 ModificationStatusId,CreatedBy,CreationDate,
			 LastModifiedBy,LastModificationDate
			)
			Select
				@drugitemid,null,null,null,@cnt_start,@cnt_stop,@price,
				Case 
					When len(@temp)= 0 or @temp is null then 0
					else @temp
				End,
				0,1,0,0,0,0,0,0,0,0,0,0,
				0,0,0,0,0,null,null,null,0,'I',0,
				'Re Extract',
				getdate(),
				user_name(),
				getdate()

			select @drugitempriceid = @@identity,@error = @@ERROR
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error when inserting into Price table'
				Insert into DI_ItemFCPPriceStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
				ErrorMessage,CreatedBy,CreationDate)
				Select
				@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
				@errorMsg,user_name(),getdate()
			End
			Else
			Begin
				Insert into DI_ItemFCPPriceStatus
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
			Insert into DI_ItemFCPPriceStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
			ErrorMessage,CreatedBy,CreationDate)
			Select
			@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
			@errorMsg,user_name(),getdate()

		End
	
		FETCH NEXT FROM FCPPrice_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@cnt_start,@cnt_stop,@price,@edate,@chg_date,@pv_chg_dat,@temp
	End
	Close FCPPrice_Cursor
	DeAllocate FCPPrice_Cursor



	Declare FCPPrice_Cursor CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,cnt_start,cnt_stop,price,edate,chg_date,pv_chg_dat,temp
		From FCPPrice where (len(N) > 0 )
		Order by CNT_NO,NDC_1,NDC_2,NDC_3,N,cnt_start,cnt_stop		

	Open FCPPrice_Cursor
	FETCH NEXT FROM FCPPrice_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@cnt_start,@cnt_stop,@price,@edate,@chg_date,@pv_chg_dat,@temp

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
			 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,AwardedFSSTrackingCustomerRatio,
			 TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
			 ModificationStatusId,CreatedBy,CreationDate,
			 LastModifiedBy,LastModificationDate
			)
			Select
				@drugitemid,@subitemid,
				@n
				,null,@cnt_start,@cnt_stop,@price,
				Case 
					When len(@temp)= 0 or @temp is null then 0
					else @temp
				End,
				0,1,0,0,0,0,0,0,0,0,0,0,
				0,0,0,0,0,null,null,null,0,'I',0,
				'Re Extract',
				getdate(),
				user_name(),
				getdate()

			select @drugitempriceid = @@identity,@error = @@ERROR
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error when inserting into Price table'
				Insert into DI_ItemFCPPriceStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
				ErrorMessage,CreatedBy,CreationDate)
				Select
				@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
				@errorMsg,user_name(),getdate()
			End
			Else
			Begin
				Insert into DI_ItemFCPPriceStatus
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
			Insert into DI_ItemFCPPriceStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
			ErrorMessage,CreatedBy,CreationDate)
			Select
			@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
			@errorMsg,user_name(),getdate()

		End
	
		FETCH NEXT FROM FCPPrice_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@cnt_start,@cnt_stop,@price,@edate,@chg_date,@pv_chg_dat,@temp
	End
	Close FCPPrice_Cursor
	DeAllocate FCPPrice_Cursor
