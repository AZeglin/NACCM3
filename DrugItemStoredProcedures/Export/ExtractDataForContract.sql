IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ExtractDataForContract')
	BEGIN
		DROP  Procedure  ExtractDataForContract
	END

GO

CREATE Procedure ExtractDataForContract
(
@contractNumber nvarchar(20)
)
As

	Declare 
		@cnt_no nvarchar(20),
		@contractId int,
		@drugItemNDCId int,
		@identity int,
		@error int,
		@errorMsg nvarchar(512),
		@ndc_1 char(5),
		@ndc_2 char(4),
		@ndc_3 char(2),
		@n char(1),
		@drugitemid int,
		@pkg nvarchar(14),
		@generic nvarchar(64),
		@trade_name nvarchar(45),
		@disc_date datetime,
		@disc_edat datetime,
		@ndc_link int,
		@cov nchar(1),
		@pv nchar(1),
		@pv_date datetime,
		@pt nchar(1),
		@dsp_unt nvarchar(10),
		@va_class nvarchar(5),
		@unit_sale nchar(2),
		@qty_u_sale decimal(5,0),
		@unit_pkg nchar(2),
		@qty_u_pkg decimal(13,5),
		@unit_meas nchar(2),
		@NDCWithNId int,
		@discontinuationDate datetime,
		@discontinuationEnteredDate datetime,
		@subitemid int,
		@pr_mult int,
		@pr_divide int,
		@temp bit,
		@fss bit,
		@big4 bit,
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
		@cnt_start datetime,
		@cnt_stop datetime,
		@price	decimal(9,2),
		@edate datetime,
		@chg_date datetime,
		@pv_chg_dat datetime,
		@dtscommand varchar(1000),
		@countFSS int,
		@countBIG4 int,
		@countFSSR int	


	If exists (Select top 1 1 From DI_ModificationStatus 
					where ContractNumber = @contractNumber 
					and ModificationType = 'SS'
			  )
	Begin
		Select @errorMsg  = 'Do nothing'
	End
	Else 
	Begin
		Declare Item_Cursor CURSOR For
			Select NDC_1,NDC_2,NDC_3,N,CNT_NO,PKG,Generic,Trade_Name,Disc_Date,Disc_Edat,Cov,
					PV,PV_Date,PT, DSP_UNT,VA_Class,Unit_Sale,Qty_U_Sale,Unit_Pkg,Qty_U_Pkg,
					Unit_Meas,Cast(Pr_Mult as int),Cast(Pr_Divide as int)
			From FSSDataRefresh
			Where CNT_NO = @contractNumber
			And (len(N) = 0 or N is null)
			Order by CNT_NO, NDC_1,NDC_2,NDC_3,N

		Open Item_Cursor
		FETCH NEXT FROM Item_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@pkg,@generic,@trade_name,@disc_date,@disc_edat,
			 @cov,@pv,@pv_date,@pt,@dsp_unt,@va_class,@unit_sale,@qty_u_sale,@unit_pkg,
			 @qty_u_pkg,@unit_meas,@pr_mult,@pr_divide	

		WHILE @@FETCH_STATUS = 0
		BEGIN
		
			Select @contractId = ContractId
			From DI_Contracts
			Where ContractNumber = @cnt_no

			Select @drugItemNDCId = DrugItemNDCId
			From Di_DrugItemNDC
			Where FDAAssignedLabelerCode = @ndc_1
			And ProductCode = @ndc_2
			And PackageCode = @ndc_3
		
			If @contractId is Not null  
			Begin
				If @drugItemNDCId is null
				Begin
					Insert into Di_DrugItemNDC
					(FdaAssignedLabelerCode,ProductCode,PackageCode,ModificationStatusId,CreatedBy,
					 CreationDate,LastModifiedBy,LastModificationDate)
					Select	@ndc_1,@ndc_2,@ndc_3,0,'Re Fresh ',getdate(),user_name(),getdate()
					
					select @drugItemNDCId = @@identity
					
					Select @drugitemid = DrugItemId
					From DI_DrugItems
					Where ContractId = 	@contractId
					And DrugItemNDCId = @drugItemNDCId				
				End
				Else
				Begin
					Select @drugitemid = DrugItemId
					From DI_DrugItems
					Where ContractId = 	@contractId
					And DrugItemNDCId = @drugItemNDCId		
				End
				
				If @drugitemid is null 
				Begin
					Insert into DI_DrugItems
					(ContractID,DrugItemNDCId,
					PackageDescription,Generic,TradeName,DiscontinuationDate,DiscontinuationEnteredDate,
					 Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,
					 DualPriceDesignation,ExcludeFromExport,NonTAA, IncludedFETAmount,LastModificationType,ModificationStatusId,
					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					Select @contractId,	@drugItemNDCId,
						   @pkg,@generic,@trade_name,
						   Case 
								When year(@disc_date) > 2050 then null
								When year(@disc_date) < 1950 then null
								else @disc_date
						   End,
						   Case 
								When year(@disc_edat) > 2050 then null
								When year(@disc_edat) < 1950 then null
								else @disc_edat
						   End,					   
						   @cov,@pv,
						   Case 
								When year(@pv_date) > 2050 then null
								When year(@pv_date) < 1950 then null
								else @pv_date
						   End,					   
						   @pt,@dsp_unt,@va_class,
						   'F',0,0,0,'I',0,
						   'Re Fresh',getdate(),user_name(),getdate()	
						   
					select @drugItemId = @@identity	
					
					Insert into DI_drugItemPackage
					(DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,
					 UnitOfMeasure,ModificationStatusId,pricemultiplier,PriceDivider,CreatedBy,CreationDate,LastModifiedBy,
					 LastModificationDate)
					Select 
						@drugitemid,@unit_sale,@qty_u_sale,@unit_pkg,@qty_u_pkg,
						@unit_meas,0,@pr_mult,@pr_divide,'Re Fresh',getdate(),user_name(),getdate()				
				End
				Else
				Begin
					Update DI_DrugItems
						Set DiscontinuationDate = 
								Case 
									When year(@disc_date) < 1950 then null
									When year(@disc_edat) > 2050 then null								 
									else @disc_date
								End,
							DiscontinuationEnteredDate = 
								Case 
									When year(@disc_edat) < 1950 then null 
									When year(@disc_edat) > 2050 then null								
									else @disc_edat
								End,
							Covered = @cov						
					Where DrugItemId = @drugitemid
				End
				

				Delete from DI_DrugItemPrice
				Where DrugItemId = @drugitemid
				And DrugItemSubItemId is null
				
				Declare Price_Cursor CURSOR For
					Select  a.cnt_start,a.cnt_stop,a.price,a.Temp,a.FSS,a.Big4,a.VA,a.bop,
							a.hhs,a.ihs,a.dihs,a.svh1,a.svh2,a.dod,a.phs,a.uscg,a.tmop,a.cmop,
							a.nih,a.edate,a.chg_date,a.pv_chg_dat
					From 
					(
						Select cnt_start,cnt_stop,price,Temp,1 as FSS,0 as Big4,0 as VA,0 as bop,
								0 as hhs,0 as ihs,0 as dihs,0 as svh1,0 as svh2,0 as dod,
								0 as phs,0 as uscg,0 as tmop,0 as cmop,0 as nih,
								edate,chg_date,pv_chg_dat
						From FSSPriceRefresh
						Where CNT_NO = @contractNumber
						And NDC_1 = @ndc_1
						And NDC_2 = @ndc_2
						And NDC_3 = @ndc_3
						And (len(N) = 0 or N is null)
						And DateDiff(day,getdate(),CNT_STOP )>=0
						Union
						Select cnt_start,cnt_stop,price,Temp,0 as FSS,1 as Big4,0 as VA,0 as bop,
								0 as hhs,0 as ihs,0 as dihs,0 as svh1,0 as svh2,0 as dod,
								0 as phs,0 as uscg,0 as tmop,0 as cmop,0 as nih,
								edate,chg_date,pv_chg_dat
						From FCPPriceRefresh  
						Where CNT_NO = @contractNumber
						And NDC_1 = @ndc_1
						And NDC_2 = @ndc_2
						And NDC_3 = @ndc_3
						And (len(N) = 0 or N is null)
						And DateDiff(day,getdate(),CNT_STOP )>=0
						Union
						Select  cnt_start,cnt_stop,price,Temp,0 as FSS,0 as Big4,VA, bop,
								hhs,ihs,dihs,svh1,svh2,dod,phs,uscg,tmop,cmop,nih,
								edate,chg_date,pv_chg_dat
						From FSSRPricRefresh 
						Where CNT_NO = @contractNumber
						And NDC_1 = @ndc_1
						And NDC_2 = @ndc_2
						And NDC_3 = @ndc_3
						And (len(N) = 0 or N is null)
						And DateDiff(day,getdate(),CNT_STOP )>=0				
					) a
					order by a.FSS Desc,a.Big4 Desc,a.Cnt_Start, a.Cnt_Stop								


				Open Price_Cursor
				FETCH NEXT FROM Price_Cursor
				INTO @cnt_start,@cnt_stop,@price,@temp,@fss,@big4,@va,@bop,@hhs,@ihs,@dihs,@svh1,@svh2,
					@dod,@phs,@uscg,@tmop,@cmop,@nih,@edate,@chg_date,@pv_chg_dat

				WHILE @@FETCH_STATUS = 0
				BEGIN
					Insert into DI_DrugItemPrice
					(DrugItemId,DrugItemSubItemId,HistoricalNValue,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,
					 IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,
					 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,AwardedFSSTrackingCustomerRatio,
					 TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
					 ModificationStatusId,CreatedBy,CreationDate,
					 LastModifiedBy,LastModificationDate
					)
					Select
						@drugitemid,null,null,1,@cnt_start,@cnt_stop,@price,@temp,@fss,@big4,@va,@bop,@cmop,@dod,@hhs,@ihs,0,
						@dihs,@nih,@phs,0,@svh1,@svh2,@tmop,@uscg,
						null,null,null,0,'I',0,
						'Re Fresh',
						getdate(),
						user_name(),
						getdate()
						
					FETCH NEXT FROM Price_Cursor
					INTO @cnt_start,@cnt_stop,@price,@temp,@fss,@big4,@va,@bop,@hhs,@ihs,@dihs,@svh1,@svh2,
						@dod,@phs,@uscg,@tmop,@cmop,@nih,@edate,@chg_date,@pv_chg_dat					

				End
				Close Price_Cursor
				DeAllocate Price_Cursor

			End
--FSS			
			If exists (Select top 1 1 From DI_DrugItemPrice 
						where DrugItemId = @drugitemid 
						and DrugItemSubItemId is null
						and IsFSS = 1 
						and IsTemporary = 0
						and datediff(day,cast('12/31/2010' as datetime),PriceStartDate)>0
					  )
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId is null					
				and a.IsFSS = 1
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))<=0				
			End
			Else
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId is null				
				and a.IsFSS = 1
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))> 0					
			End
--BIG4	
			If exists (Select top 1 1 From DI_DrugItemPrice 
						where DrugItemId = @drugitemid
						and DrugItemSubItemId is null						 
						and IsBig4 = 1 
						and IsTemporary = 0
						and datediff(day,cast('12/31/2010' as datetime),PriceStartDate)>0
					  )
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId is null					
				and a.IsBig4 = 1
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))<=0				
			End
			Else
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId is null				
				and a.IsBig4 = 1
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))> 0					
			End
--FSSR
			If exists (Select top 1 1 From DI_DrugItemPrice 
						where DrugItemId = @drugitemid 
						and DrugItemSubItemId is null
						and IsFSS = 0 and  IsBIG4 = 0
						and IsTemporary = 0
						and datediff(day,cast('12/31/2010' as datetime),PriceStartDate)>0
					  )
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId is null					
				and IsFSS = 0 and  IsBIG4 = 0
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))<=0				
			End
			Else
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId is null					
				and IsFSS = 0 and  IsBIG4 = 0
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))> 0					
			End			
			
			
			Select @contractId = null,@drugitemid = null, @drugItemNDCId = null
		
			FETCH NEXT FROM Item_Cursor
			INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@pkg,@generic,@trade_name,@disc_date,@disc_edat,
				 @cov,@pv,@pv_date,@pt,@dsp_unt,@va_class,@unit_sale,@qty_u_sale,@unit_pkg,
				 @qty_u_pkg,@unit_meas,@pr_mult,@pr_divide	
			
		End
		Close Item_Cursor
		DeAllocate Item_Cursor
		

		Declare Item_Cursor CURSOR For
			Select NDC_1,NDC_2,NDC_3,N,CNT_NO,PKG,Generic,Trade_Name,Disc_Date,Disc_Edat,Cov,
					PV,PV_Date,PT, DSP_UNT,VA_Class,Unit_Sale,Qty_U_Sale,Unit_Pkg,Qty_U_Pkg,
					Unit_Meas,Cast(Pr_Mult as int),Cast(Pr_Divide as int)
			From FSSDataRefresh  
			Where CNT_NO = @contractNumber
			And (len(N) > 0)
			Order by CNT_NO, NDC_1,NDC_2,NDC_3,N

		Open Item_Cursor
		FETCH NEXT FROM Item_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@pkg,@generic,@trade_name,@disc_date,@disc_edat,
			 @cov,@pv,@pv_date,@pt,@dsp_unt,@va_class,@unit_sale,@qty_u_sale,@unit_pkg,
			 @qty_u_pkg,@unit_meas,@pr_mult,@pr_divide	

		WHILE @@FETCH_STATUS = 0
		BEGIN
		
			Select @contractId = ContractId
			From DI_Contracts
			Where ContractNumber = @cnt_no

			Select @drugItemNDCId = DrugItemNDCId
			From Di_DrugItemNDC
			Where FDAAssignedLabelerCode = @ndc_1
			And ProductCode = @ndc_2
			And PackageCode = @ndc_3
		
			If @contractId is Not null  
			Begin
				If @drugItemNDCId is null
				Begin
					Insert into Di_DrugItemNDC
					(FdaAssignedLabelerCode,ProductCode,PackageCode,ModificationStatusId,CreatedBy,
					 CreationDate,LastModifiedBy,LastModificationDate)
					Select	@ndc_1,@ndc_2,@ndc_3,0,'Re Fresh ',getdate(),user_name(),getdate()
					
					select @drugItemNDCId = @@identity
					
					Select @drugitemid = DrugItemId
					From DI_DrugItems
					Where ContractId = 	@contractId
					And DrugItemNDCId = @drugItemNDCId				
				End
				Else
				Begin
					Select @drugitemid = DrugItemId
					From DI_DrugItems
					Where ContractId = 	@contractId
					And DrugItemNDCId = @drugItemNDCId		
				End
				
				If @drugitemid is null 
				Begin
					Insert into DI_DrugItems
					(ContractID,DrugItemNDCId,
					PackageDescription,Generic,TradeName,DiscontinuationDate,DiscontinuationEnteredDate,
					 Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,
					 DualPriceDesignation,ExcludeFromExport,NonTAA, IncludedFETAmount,LastModificationType,ModificationStatusId,
					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					Select @contractId,	@drugItemNDCId,
						   @pkg,@generic,@trade_name,@disc_date,@disc_edat,
						   @cov,@pv,@pv_date,@pt,@dsp_unt,@va_class,
						   'F',0,0,0,'I',0,
						   'Re Fresh',getdate(),user_name(),getdate()	
						   
					select @drugItemId = @@identity	
					
					Insert into DI_drugItemPackage
					(DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,
					 UnitOfMeasure,ModificationStatusId,pricemultiplier,PriceDivider,CreatedBy,CreationDate,LastModifiedBy,
					 LastModificationDate)
					Select 
						@drugitemid,@unit_sale,@qty_u_sale,@unit_pkg,@qty_u_pkg,
						@unit_meas,0,@pr_mult,@pr_divide,'Re Fresh',getdate(),user_name(),getdate()				
						
					Insert into DI_DrugItemSubItems
						(DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,
						 LastModificationType,ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,
						 LastModificationDate)
					Select 
						@drugitemid,@n,@pkg,@generic,@trade_name,@dsp_unt,'I',0,'Re Fresh',getdate(),user_name(),getdate()					
						
					select @subItemId = @@identity						
				End
				Else
				Begin
/*					Update DI_DrugItems
						Set DiscontinuationDate = 
								Case 
									When year(@disc_date) < 1950 then null 
									else @disc_date
								End,
							DiscontinuationEnteredDate = 
								Case 
									When year(@disc_edat) < 1950 then null 
									else @disc_edat
								End,
							Covered = @cov						
					Where DrugItemId = @drugitemid
*/							
				
					Select @subitemid = DrugItemSubItemId
					From DI_DrugItemSubItems
					Where DrugItemId =  @drugitemid
					And SubItemIdentifier = @n	
					
					If @subitemid is null
					Begin
						Insert into DI_DrugItemSubItems
							(DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,
							 LastModificationType,ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,
							 LastModificationDate)
						Select 
							@drugitemid,@n,@pkg,@generic,@trade_name,@dsp_unt,'I',0,'Re Fresh',getdate(),user_name(),getdate()					
							
						select @subItemId = @@identity							
					End
				End
				

				Delete from DI_DrugItemPrice
				Where DrugItemId = @drugitemid
				And DrugItemSubItemId = @subItemId
				
				Declare Price_Cursor CURSOR For
					Select  a.cnt_start,a.cnt_stop,a.price,a.Temp,a.FSS,a.Big4,a.VA,a.bop,
							a.hhs,a.ihs,a.dihs,a.svh1,a.svh2,a.dod,a.phs,a.uscg,a.tmop,a.cmop,
							a.nih,a.edate,a.chg_date,a.pv_chg_dat
					From 
					(
						Select cnt_start,cnt_stop,price,Temp,1 as FSS,0 as Big4,0 as VA,0 as bop,
								0 as hhs,0 as ihs,0 as dihs,0 as svh1,0 as svh2,0 as dod,
								0 as phs,0 as uscg,0 as tmop,0 as cmop,0 as nih,
								edate,chg_date,pv_chg_dat
						From FSSPriceRefresh
						Where CNT_NO = @contractNumber
						And NDC_1 = @ndc_1
						And NDC_2 = @ndc_2
						And NDC_3 = @ndc_3
						And (len(N) > 0 and N = @n)
						And DateDiff(day,getdate(),CNT_STOP )>=0
						Union
						Select cnt_start,cnt_stop,price,Temp,0 as FSS,1 as Big4,0 as VA,0 as bop,
								0 as hhs,0 as ihs,0 as dihs,0 as svh1,0 as svh2,0 as dod,
								0 as phs,0 as uscg,0 as tmop,0 as cmop,0 as nih,
								edate,chg_date,pv_chg_dat
						From FCPPriceRefresh 
						Where CNT_NO = @contractNumber
						And NDC_1 = @ndc_1
						And NDC_2 = @ndc_2
						And NDC_3 = @ndc_3
						And (len(N) > 0 and N = @n)
						And DateDiff(day,getdate(),CNT_STOP )>=0
						Union
						Select  cnt_start,cnt_stop,price,Temp,0 as FSS,0 as Big4,VA, bop,
								hhs,ihs,dihs,svh1,svh2,dod,phs,uscg,tmop,cmop,nih,
								edate,chg_date,pv_chg_dat
						From FSSRPricRefresh  
						Where CNT_NO = @contractNumber
						And NDC_1 = @ndc_1
						And NDC_2 = @ndc_2
						And NDC_3 = @ndc_3
						And (len(N) > 0 and N = @n)
						And DateDiff(day,getdate(),CNT_STOP )>=0				
					) a
					order by a.FSS Desc,a.Big4 Desc,a.Cnt_Start, a.Cnt_Stop	


				Open Price_Cursor
				FETCH NEXT FROM Price_Cursor
				INTO @cnt_start,@cnt_stop,@price,@temp,@fss,@big4,@va,@bop,@hhs,@ihs,@dihs,@svh1,@svh2,
					@dod,@phs,@uscg,@tmop,@cmop,@nih,@edate,@chg_date,@pv_chg_dat

				WHILE @@FETCH_STATUS = 0
				BEGIN
					Insert into DI_DrugItemPrice
					(DrugItemId,DrugItemSubItemId,HistoricalNValue,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,
					 IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,
					 IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,AwardedFSSTrackingCustomerRatio,
					 TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
					 ModificationStatusId,CreatedBy,CreationDate,
					 LastModifiedBy,LastModificationDate
					)
					Select
						@drugitemid,@subItemId,null,1,@cnt_start,@cnt_stop,@price,@temp,@fss,@big4,@va,@bop,@cmop,@dod,@hhs,@ihs,0,
						@dihs,@nih,@phs,0,@svh1,@svh2,@tmop,@uscg,
						null,null,null,0,'I',0,
						'Re Fresh',
						getdate(),
						user_name(),
						getdate()
						
					FETCH NEXT FROM Price_Cursor
					INTO @cnt_start,@cnt_stop,@price,@temp,@fss,@big4,@va,@bop,@hhs,@ihs,@dihs,@svh1,@svh2,
						@dod,@phs,@uscg,@tmop,@cmop,@nih,@edate,@chg_date,@pv_chg_dat					

				End
				Close Price_Cursor
				DeAllocate Price_Cursor

			End
--FSS			
			If exists (Select top 1 1 From DI_DrugItemPrice 
						where DrugItemId = @drugitemid 
						and DrugItemSubItemId = @subItemId
						and IsFSS = 1 
						and IsTemporary = 0
						and datediff(day,cast('12/31/2010' as datetime),PriceStartDate)>0
					  )
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId = @subItemId				
				and a.IsFSS = 1
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))<=0				
			End
			Else
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId = @subItemId			
				and a.IsFSS = 1
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))> 0					
			End
--BIG4	
			If exists (Select top 1 1 From DI_DrugItemPrice 
						where DrugItemId = @drugitemid
						and DrugItemSubItemId = @subItemId						 
						and IsBig4 = 1 
						and IsTemporary = 0
						and datediff(day,cast('12/31/2010' as datetime),PriceStartDate)>0
					  )
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId = @subItemId					
				and a.IsBig4 = 1
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))<=0				
			End
			Else
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId = @subItemId			
				and a.IsBig4 = 1
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))> 0					
			End
--FSSR
			If exists (Select top 1 1 From DI_DrugItemPrice 
						where DrugItemId = @drugitemid 
						and DrugItemSubItemId = @subItemId
						and IsFSS = 0 and  IsBIG4 = 0
						and IsTemporary = 0
						and datediff(day,cast('12/31/2010' as datetime),PriceStartDate)>0
					  )
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId = @subItemId				
				and IsFSS = 0 and  IsBIG4 = 0
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))<=0				
			End
			Else
			Begin
				Update a
					Set a.pricestopdate = 
						Case 
							When b.DiscontinuationDate is not null Then
								Case 
									When a.PriceStopDate < b.DiscontinuationDate
										then a.PriceStopDate
									Else b.DiscontinuationDate
								End
							When b.DiscontinuationDate is null Then
								Case 
									When d.Dates_Completion is not null Then
										Case 
											When a.PriceStopDate < d.Dates_Completion and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate 
											Else d.Dates_Completion
										End	
									When d.Dates_Completion is  null Then
										Case 
											When a.PriceStopDate < d.Dates_cntrctExp and a.PriceStopDate < '12/31/2010'
												then a.PriceStopDate
											Else d.Dates_cntrctExp
										End
								End												
						End
				From DI_DrugItemPrice a
				Join DI_DrugItems b
				on a.DrugItemId = b.DrugItemId
				Join DI_Contracts c
				on b.ContractId = c.ContractId
				Join Nac_cm.dbo.tbl_cntrcts d
				on c.NACCMContractId = d.contract_record_id
				Where a.DrugItemId = @drugitemid
				and a.DrugItemSubItemId = @subItemId					
				and IsFSS = 0 and  IsBIG4 = 0
				and a.IsTemporary = 0
				and datediff(day,a.pricestartdate, cast('1/1/2011' as datetime))> 0					
			End					
			
			Select @contractId = null,@drugitemid = null, @drugItemNDCId = null
			
			FETCH NEXT FROM Item_Cursor
			INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@pkg,@generic,@trade_name,@disc_date,@disc_edat,
				 @cov,@pv,@pv_date,@pt,@dsp_unt,@va_class,@unit_sale,@qty_u_sale,@unit_pkg,
				 @qty_u_pkg,@unit_meas,@pr_mult,@pr_divide	
			
		End
		Close Item_Cursor
		DeAllocate Item_Cursor	

	End

