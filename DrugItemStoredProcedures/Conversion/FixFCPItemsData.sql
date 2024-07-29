IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FixFCPItemsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FixFCPItemsData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[FixFCPItemsData]
As	

	Declare 
			@ndc_1 char(5),
			@ndc_2 char(4),
			@ndc_3 char(2),
			@n char(1),
			@identity int,
			@cnt_no nvarchar(20),
			@newndc1 char(5),
			@newndc2 char(4),
			@newndc3 char(2),
			@newn char(1),
			@newcnt nvarchar(20),
			@statusId int,
			@errormsg nvarchar(500),
			@cnt_start datetime,
			@cnt_stop datetime,
			@price decimal(9,2),
			@edate datetime,
			@chg_date datetime,
			@pv_chg_dat datetime,
			@drugitemid int,
			@contractId int,
			@drugItemNDCId int,
			@error int,
			@NDCWithNId int,
			@subitemid int,
			@drugitempriceid int,
			@temp bit
		
	Declare FCPPrice_Cursor CURSOR For
		Select itemfcppricestatusid,ContractNumber,NDC_1,NDC_2,NDC_3,N
		From di_itemFCPpricestatus 
		Where errormessage like 'NDC%'

	Open FCPPrice_Cursor
	FETCH NEXT FROM FCPPrice_Cursor
	INTO @statusId,@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n

	WHILE @@FETCH_STATUS = 0
	BEGIN

		exec CheckIfNDCChangedProc
			@ndc_1,
			@ndc_2,
			@ndc_3,
			@cnt_no,
			@ndc1_new = @newndc1 OUTPUT,
			@ndc2_new = @newndc2 OUTPUT,
			@ndc3_new = @newndc3 OUTPUT,
			@cnt_new  = @newcnt OUTPUT


		If @newndc1 is null
		Begin
			select @errorMsg = 'Not found'
		End
		Else
		Begin
			Select  @price = Price,
					@cnt_start = CNT_Start,
					@cnt_stop = CNT_Stop,
					@edate = edate,
					@chg_date = chg_date,
					@pv_chg_dat = pv_chg_Dat,
					@temp =Temp
			from FCPPrice
			Where Cnt_No = @cnt_no
			And NDC_1 = @ndc_1
			And NDC_2 = @ndc_2
			And NDC_3 = @ndc_3

			Select @contractId = ContractId
			From DI_Contracts
			Where ContractNumber = @newcnt

			Select @drugItemNDCId = DrugItemNDCId
			From Di_DrugItemNDC
			Where FDAAssignedLabelerCode = @newndc1
			And ProductCode = @newndc2
			And PackageCode = @newndc3


			If  exists (Select top 1 1 from DI_DrugItems where contractid = @contractId 
						and drugitemndcid = @drugItemNDCId
 					   )
			Begin

				Select @drugitemid = Drugitemid
				from DI_DrugItems where contractid = @contractId 
				and drugitemndcid = @drugItemNDCId 

				If len(@n)>0
				Begin
					If exists (Select top 1 1 From DI_DrugItemSubItems Where DrugItemId = @drugitemid
																	And SubItemIdentifier = @n
							   )
					Begin
						Select @subitemid = Drugitemsubitemid
						from DI_DrugItemSubItems 
						Where DrugItemId = @drugitemid
						And SubItemIdentifier = @n
					End
				End

				Insert into DI_DrugItemPrice
				(DrugItemId,DrugItemSubItemId,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,
				 IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,
				 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,AwardedFSSTrackingCustomerRatio,
				 TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
				 ModificationStatusId,CreatedBy,CreationDate,
				 LastModifiedBy,LastModificationDate
				)
				Select
					@drugitemid,@subitemid,null,@cnt_start,@cnt_stop,@price,
					Case 
						When len(@temp)= 0 or @temp is null then 0
						else @temp
					End,
					0,1,0,0,0,0,0,0,0,0,0,0,
					0,0,0,0,0,null,null,null,0,'I',0,
					'Re Extract',
					GETDATE(),
					user_name(),
					GETDATE()

				select @drugitempriceid = @@identity,@error = @@ERROR
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error when fixing into Price table'

					Update DI_ItemFCPPriceStatus
					Set ErrorMessage = @errorMsg
					Where itemFCPpricestatusid = @statusId
				End
				Else
				Begin
					select @errorMsg = 'Updated'

					Update DI_ItemFCPPriceStatus
					Set ErrorMessage = @errorMsg,
						ContractId = @contractId,
						DrugItemNDCId = @drugItemNDCId,
						DrugItemId = @drugitemid,
						DrugItemPriceId = @drugitempriceid
					Where itemFCPpricestatusid = @statusId
				End				
			End
		End				



		Select @newndc1 = null,@newndc2 = null,@newndc3 =null,@newn=null,@newcnt = null,@drugitemid = null,
				@contractId = null, @drugItemNDCId = null, @errorMsg = null,
				@subitemid = null,@drugitempriceid = null

		FETCH NEXT FROM FCPPrice_Cursor
		INTO @statusId,@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n
	End
	Close FCPPrice_Cursor
	DeAllocate FCPPrice_Cursor
