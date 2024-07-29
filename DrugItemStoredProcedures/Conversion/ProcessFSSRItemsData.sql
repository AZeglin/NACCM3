IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessFSSRItemsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessFSSRItemsData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[ProcessFSSRItemsData]
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
	@va bit,
	@bop bit,
	@hhs bit,
	@ihs bit,
	@dihs bit,
	@svh1 bit,
	@svh2 bit,
	@dod bit,
	@phs bit,
	@uscg bit,
	@tmop bit,
	@cmop bit,
	@nih bit,
	@fhcc bit,
	@priceId int,
	@NDCWithNId int,
	@subitemid int,
	@drugitempriceid int,
	@IsVA bit,
	@IsBOP bit,
	@IsCMOP bit,
	@IsDOD bit,
	@IsHHS bit,
	@IsIHS bit,
    @IsDIHS bit,
    @IsNIH bit,
    @IsPHS bit,
    @IsSVH1 bit,
    @IsSVH2 bit,
    @IsTMOP bit,			
    @IsUSCG bit,
    @IsFHCC bit,
    @temp bit
    



	Declare FSSRPrice_Cursor CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,cnt_start,cnt_stop,price,va,bop,hhs,ihs,dihs,svh1,svh2,
				dod,phs,uscg,tmop,cmop,nih,fhcc,edate,chg_date,pv_chg_dat,temp
		From FSSRPric where (len(N) = 0 or N is null)
		Order by CNT_NO, NDC_1,NDC_2,NDC_3,N,cnt_start,cnt_stop

	Open FSSRPrice_Cursor
	FETCH NEXT FROM FSSRPrice_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@cnt_start,@cnt_stop,@price,@va,@bop,@hhs,@ihs,@dihs,@svh1,@svh2,
				@dod,@phs,@uscg,@tmop,@cmop,@nih,@fhcc,@edate,@chg_date,@pv_chg_dat,@temp

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
			 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,ISFHCC,AwardedFSSTrackingCustomerRatio,
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
				0,0,@va,@bop,@cmop,@dod,@hhs,@ihs,0,
				@dihs,@nih,@phs,0,@svh1,@svh2,@tmop,@uscg,@fhcc,
				null,null,null,0,'I',0,
				'Re Extract',
				getdate(),
				user_name(),
				getdate()

			select @drugitempriceid = @@identity,@error = @@ERROR
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error when inserting into Price table'
				Insert into DI_ItemFSSRPriceStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
				ErrorMessage,CreatedBy,CreationDate)
				Select
				@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
				@errorMsg,user_name(),getdate()
			End
			Else
			Begin
				Insert into DI_ItemFSSRPriceStatus
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
			Insert into DI_ItemFSSRPriceStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
			ErrorMessage,CreatedBy,CreationDate)
			Select
			@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
			@errorMsg,user_name(),getdate()

		End

		FETCH NEXT FROM FSSRPrice_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@cnt_start,@cnt_stop,@price,@va,@bop,@hhs,@ihs,@dihs,@svh1,@svh2,
				@dod,@phs,@uscg,@tmop,@cmop,@nih,@fhcc,@edate,@chg_date,@pv_chg_dat,@temp
	End
	Close FSSRPrice_Cursor
	DeAllocate FSSRPrice_Cursor


	Declare FSSRPrice_Cursor CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,cnt_start,cnt_stop,price,va,bop,hhs,ihs,dihs,svh1,svh2,
				dod,phs,uscg,tmop,cmop,nih,fhcc,edate,chg_date,pv_chg_dat,temp
		From FSSRPric where (len(N) > 0 )
		Order by CNT_NO, NDC_1,NDC_2,NDC_3,N,cnt_start,cnt_stop

	Open FSSRPrice_Cursor
	FETCH NEXT FROM FSSRPrice_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@cnt_start,@cnt_stop,@price,@va,@bop,@hhs,@ihs,@dihs,@svh1,@svh2,
				@dod,@phs,@uscg,@tmop,@cmop,@nih,@fhcc,@edate,@chg_date,@pv_chg_dat,@temp

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

		If exists (Select top 1 1 from DI_DrugItems 
					where contractid = @contractId 
					and drugitemndcid = @drugItemNDCId 
				   )
		Begin
			Select @drugitemId = Drugitemid
			from DI_DrugItems 
			where contractid = @contractId 
			and drugitemndcid = @drugItemNDCId 
			
			Select @IsVA = ISVA,
				   @IsBOP = ISBOP,
				   @IsCMOP = IsCMOP,
				   @IsDOD = IsDOD,
				   @IsHHS = IsHHS,
				   @IsIHS = IsIHS,
--				   IsIHS2 = IsIHS2,
				   @IsDIHS = IsDIHS,
				   @IsNIH = IsNIH,
				   @IsPHS = IsPHS,
--				   IsSVH = IsSVH,
				   @IsSVH1 = IsSVH1,
				   @IsSVH2 = IsSVH2,
				   @IsTMOP = IsTMOP,			
				   @IsUSCG = IsUSCG,
				   @IsFHCC = IsFHCC
			From DI_drugitemprice
			where drugitemid = 	@drugitemId
			and IsFSS = 0
			and ISBIG4 = 0
			and HistoricalNValue is null

			If (@IsVA = @va	And @IsBOP = @bop And @IsCMOP = @cmop And @IsDOD = @dod And
				@IsHHS = @hhs And @IsIHS = @ihs And @IsDIHS = @dihs And @IsNIH = @nih And				   
				@IsPHS = @phs And @IsSVH1 = @svh1 And @IsSVH2 = @svh2 And @IsTMOP = @tmop
				And @IsUSCG = @uscg and @IsFHCC = @fhcc
			   )
			Begin
				Select @subitemid = DrugItemSubItemId
				from DI_DrugItemsubItems 
				Where drugitemid = @drugitemId 
				and SubItemIdentifier = @n			
			
				Insert into DI_DrugItemPrice
				(DrugItemId,DrugItemSubItemId,HistoricalNValue,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,
				 IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,
				 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,IsFHCC,AwardedFSSTrackingCustomerRatio,
				 TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
				 ModificationStatusId,CreatedBy,CreationDate,
				 LastModifiedBy,LastModificationDate
				)
				Select
					@drugitemid,@subitemid,
					null,
					null,@cnt_start,@cnt_stop,@price,
					Case 
						When len(@temp)= 0 or @temp is null then 0
						else @temp
					End,
					0,0,@va,@bop,@cmop,@dod,@hhs,@ihs,0,
					@dihs,@nih,@phs,0,@svh1,@svh2,@tmop,@uscg,@fhcc,
					null,null,null,0,'I',0,
					'Re Extract',
					getdate(),
					user_name(),
					getdate()

				select @drugitempriceid = @@identity, @error = @@ERROR
				
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error when inserting into Price table'
					Insert into DI_ItemFSSRPriceStatus
					(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
					ErrorMessage,CreatedBy,CreationDate)
					Select
					@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
					@errorMsg,user_name(),getdate()
				End
				Else
				Begin
					Insert into DI_ItemFSSRPriceStatus
					(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
					DrugitemId,DrugitemPriceId,ErrorMessage,CreatedBy,CreationDate)
					Select
					@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
					@drugitemid,@drugitempriceid,'Inserted',user_name(),getdate()			
				End	
			End
			Else
			Begin
				Insert into DI_DrugItemPrice
				(DrugItemId,DrugItemSubItemId,HistoricalNValue,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,
				 IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,
				 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,IsFHCC,AwardedFSSTrackingCustomerRatio,
				 TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
				 ModificationStatusId,CreatedBy,CreationDate,
				 LastModifiedBy,LastModificationDate
				)
				Select
					@drugitemid,null,
					@n,
					null,@cnt_start,@cnt_stop,@price,
					Case 
						When len(@temp)= 0 or @temp is null then 0
						else @temp
					End,					
					0,0,@va,@bop,@cmop,@dod,@hhs,@ihs,0,
					@dihs,@nih,@phs,0,@svh1,@svh2,@tmop,@uscg,@fhcc,
					null,null,null,0,'I',0,
					'Re Extract',
					getdate(),
					user_name(),
					getdate()

				select @drugitempriceid = @@identity, @error = @@ERROR
				
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error when inserting into Price table'
					Insert into DI_ItemFSSRPriceStatus
					(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
					ErrorMessage,CreatedBy,CreationDate)
					Select
					@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
					@errorMsg,user_name(),getdate()
				End
				Else
				Begin
					Insert into DI_ItemFSSRPriceStatus
					(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
					DrugitemId,DrugitemPriceId,ErrorMessage,CreatedBy,CreationDate)
					Select
					@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
					@drugitemid,@drugitempriceid,'Inserted',user_name(),getdate()			
				End				
			End		
			
		End
		Else
		Begin
			select @errorMsg = 'NDC Price Item not found'
			Insert into DI_ItemFSSRPriceStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,N,price,PriceStartdate,PriceStopdate,
			ErrorMessage,CreatedBy,CreationDate)
			Select
			@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@price,@cnt_start, @cnt_stop,
			@errorMsg,user_name(),getdate()

		End

		FETCH NEXT FROM FSSRPrice_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@cnt_start,@cnt_stop,@price,@va,@bop,@hhs,@ihs,@dihs,@svh1,@svh2,
				@dod,@phs,@uscg,@tmop,@cmop,@nih,@fhcc,@edate,@chg_date,@pv_chg_dat,@temp
	End
	Close FSSRPrice_Cursor
	DeAllocate FSSRPrice_Cursor
