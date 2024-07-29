IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'PriceListExportForFSSRData')
	BEGIN
		DROP  Procedure  PriceListExportForFSSRData
	END

GO

CREATE Procedure PriceListExportForFSSRData
((@contractNumber nvarchar(20),
 @covered char(1),
 @priceStartDate datetime = null,
 @priceStopDate datetime = null
)
As 
	Declare @startDay int, @stopDay int, @startMonth int, @stopMonth int,
			@startYear int, @stopYear int

	Select  @startDay = DAY(@priceStartDate),
			@stopDay = DAY(@priceStopDate),
			@startMonth = MONTH(@priceStartDate),
			@stopMonth = MONTH(@priceStopDate),	
			@startYear = YEAR(@priceStartDate),	
			@stopYear = YEAR(@priceStopDate)

	If @covered = 'T'
	Begin
		If (@priceStartDate is null or @priceStopDate is null ) 
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
--				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current Price],
				Null as [Proposed Price without IFF],
				Null as [Proposed Price with IFF],
--				FCP.FCP as [FCP],
				Case	
					When year(pricestartdate) <= YEAR(GETDATE()) then
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(GETDATE())) 
					else
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(pricestartdate)) 					
				End as [FCP],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)
				End As [Start Date],
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [End Date],
				Null as [Remove   ( 1 = Remove Price )],
				Null as [Temporary Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary Price with IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)		 
				End As [Temporary Price Start Date],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [Temporary Price End Date],
--				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
--				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure],
--				Itm.IsFSS,
--				Itm.IsBig4,
				Case
					When Itm.IsVA= 0 then 0 else 1
				End as VA,
				Case
					When Itm.IsDOD= 0 then 0 else 1
				End as DOD,				
				Case
					When Itm.IsBOP= 0 then 0 else 1
				End as BOP,					
				Case
					When Itm.IsHHS= 0 then 0 else 1
				End as HHS,	
				Case
					When Itm.IsIHS= 0 then 0 else 1
				End as IHS,	
				Case
					When Itm.IsIHS2= 0 then 0 else 1
				End as IHS2,	
				Case
					When Itm.IsDIHS= 0 then 0 else 1
				End as DIHS,	
				Case
					When Itm.IsSVH= 0 then 0 else 1
				End as SVH,
				Case
					When Itm.IsSVH1= 0 then 0 else 1
				End as SVH1,
				Case
					When Itm.IsSVH2= 0 then 0 else 1
				End as SVH2,
				Case
					When Itm.IsPHS= 0 then 0 else 1
				End as PHS,
				Case
					When Itm.IsUSCG= 0 then 0 else 1
				End as USCG,
				Case
					When Itm.IsTMOP= 0 then 0 else 1
				End as TMOP,
				Case
					When Itm.IsCMOP= 0 then 0 else 1
				End as CMOP,
				Case
					When Itm.IsNIH= 0 then 0 else 1
				End as NIH,
				Case
					When Itm.IsFHCC= 0 then 0 else 1
				End as FHCC				
			 From Di_Contracts Cnt 
			 join 
				(	Select  a.DrugItemId,ContractId,DrugItemNDCId,Null as DrugItemSubItemId,Covered,
							Generic,
							TradeName,
							DispensingUnit,
							PackageDescription,
							b.DrugItemPriceId,
							b.Price,
							b.IsTemporary,
							b.priceStartDate,
							b.PriceStopDate,
							b.CurrentTrackingCustomerPrice,
							b.AwardedFSSTrackingCustomerRatio,
							b.IsFSS,
							b.IsBig4,
							b.IsVA,
							b.IsDOD,
							b.IsBOP,
							b.IsHHS,
							b.IsIHS,
							b.IsIHS2,
							b.IsDIHS,
							b.IsSVH,
							b.IsSVH1,
							b.IsSVH2,
							b.IsPHS,
							b.IsUSCG,
							b.IsTMOP,
							b.IsCMOP,
							b.IsNIH,
							b.IsFHCC
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'T'
							and b.IsBig4 = 0 and b.IsFSS = 0
							and b.DrugItemSubitemId is null						
					Union
					Select a.DrugItemId,a.ContractId,a.DrugItemNDCId,b.DrugItemSubItemId,a.Covered,
							Case 
								When b.DrugItemSubItemId is not null then b.Generic
								Else a.Generic
							End as Generic,
							Case 
								When b.DrugItemSubItemId is not null then b.TradeName
								Else a.TradeName
							End as TradeName,
							Case 
								When b.DrugItemSubItemId is not null then b.DispensingUnit
								Else a.DispensingUnit
							End as Dispensingunit,	
							Case
								When b.DrugItemSubItemId is not null then b.PackageDescription
								Else a.PackageDescription
							End as PackageDescription,
							c.DrugItemPriceId,
							c.Price,
							c.IsTemporary,				
							c.priceStartDate,
							c.PriceStopDate,
							c.CurrentTrackingCustomerPrice,
							c.AwardedFSSTrackingCustomerRatio,
							c.IsFSS,
							c.IsBig4,
							c.IsVA,
							c.IsDOD,
							c.IsBOP,
							c.IsHHS,
							c.IsIHS,
							c.IsIHS2,
							c.IsDIHS,
							c.IsSVH,
							c.IsSVH1,
							c.IsSVH2,
							c.IsPHS,
							c.IsUSCG,
							c.IsTMOP,
							c.IsCMOP,
							c.IsNIH,
							c.IsFHCC
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'T' 
							and c.IsBig4 = 0 and c.IsFSS = 0
							and c.DrugItemSubitemId = b.DrugItemSubitemId
				) Itm
				on cnt.ContractId = Itm.ContractId
			 join DI_DrugItemNDC ndc 
				on Itm.DrugItemNDCId = ndc.DrugItemNDCId
			 left outer join DI_DrugItemPackage ItmPkg 
				on Itm.DrugItemid = ItmPkg.DrugItemid
--			 left outer join DI_FCP FCP
--				on FCP.DrugItemId = Itm.DrugItemId
			 Where (	
						cnt.ContractNumber = @contractNumber or
						cnt.NACCMContractNumber = @contractNumber
				   )
			order by NDC1,NDC2,NDC3,[Temporary Price Start Date],[Temporary Price End Date]
		End
/*		Else If (   @startDay = 1 And @startMonth = 1 And 
					@stopDay = 31 And @stopMonth = 12 And @startYear = @stopYear
				)
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
--				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current Price],
				Null as [Proposed Price without IFF],
				Null as [Proposed Price with IFF],
--				FCP.FCP as [FCP],
				dbo.GetFCPValueForDrugItem(Itm.DrugItemID,null) as [FCP],
				Case 
					When Itm.IsTemporary = 1 then Null 
					When Itm.IsTemporary = 0 
						And ((Day(Itm.PriceStartDate) =1 And MONTH(Itm.PriceStartDate) = 1 ) OR
							 (YEAR(Itm.PriceStartDate)<YEAR(@priceStartDate))
							)
						then
							substring(Convert(Varchar(10), @priceStartDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),1,4)				
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)
				End As [Start Date],
				Case 
					When Itm.IsTemporary = 1 then Null 
					When Itm.IsTemporary = 0 
						And ((Day(Itm.PriceStopDate) =12 And MONTH(Itm.PriceStopDate) = 31 )OR
							 (YEAR(Itm.PriceStopDate)>YEAR(@priceStopDate))
							) then	
							substring(Convert(Varchar(10), @priceStopDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),1,4)										
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [End Date],
				Null as [Remove   ( 1 = Remove Price )],
				Null as [Temporary Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary Price with IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					When Itm.IsTemporary = 1 
						And ((Day(Itm.PriceStartDate) =1 And MONTH(Itm.PriceStartDate) = 1 ) OR
							 (YEAR(Itm.PriceStartDate)<YEAR(@priceStartDate))
							)
						then
							substring(Convert(Varchar(10), @priceStartDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),1,4)				
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)		 
				End As [Temporary Price Start Date],
				Case 
					When Itm.IsTemporary = 0 then Null 
					When Itm.IsTemporary = 1 
						And ((Day(Itm.PriceStopDate) =12 And MONTH(Itm.PriceStopDate) = 31 )OR
							 (YEAR(Itm.PriceStopDate)>YEAR(@priceStopDate))
							) then	
							substring(Convert(Varchar(10), @priceStopDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),1,4)										
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [Temporary Price End Date],
--				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
--				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure],
--				Itm.IsFSS,
--				Itm.IsBig4,
				Case
					When Itm.IsVA= 0 then 0 else 1
				End as VA,
				Case
					When Itm.IsDOD= 0 then 0 else 1
				End as DOD,				
				Case
					When Itm.IsBOP= 0 then 0 else 1
				End as BOP,					
				Case
					When Itm.IsHHS= 0 then 0 else 1
				End as HHS,	
				Case
					When Itm.IsIHS= 0 then 0 else 1
				End as IHS,	
				Case
					When Itm.IsIHS2= 0 then 0 else 1
				End as IHS2,	
				Case
					When Itm.IsDIHS= 0 then 0 else 1
				End as DIHS,	
				Case
					When Itm.IsSVH= 0 then 0 else 1
				End as SVH,
				Case
					When Itm.IsSVH1= 0 then 0 else 1
				End as SVH1,
				Case
					When Itm.IsSVH2= 0 then 0 else 1
				End as SVH2,
				Case
					When Itm.IsPHS= 0 then 0 else 1
				End as PHS,
				Case
					When Itm.IsUSCG= 0 then 0 else 1
				End as USCG,
				Case
					When Itm.IsTMOP= 0 then 0 else 1
				End as TMOP,
				Case
					When Itm.IsCMOP= 0 then 0 else 1
				End as CMOP,
				Case
					When Itm.IsNIH= 0 then 0 else 1
				End as NIH,
				Case
					When Itm.IsFHCC= 0 then 0 else 1
				End as FHCC				
			 From Di_Contracts Cnt 
			 join 
				(	Select  a.DrugItemId,ContractId,DrugItemNDCId,Null as DrugItemSubItemId,Covered,
							Generic,
							TradeName,
							DispensingUnit,
							PackageDescription,
							b.DrugItemPriceId,
							b.Price,
							b.IsTemporary,
							b.priceStartDate,
							b.PriceStopDate,
							b.CurrentTrackingCustomerPrice,
							b.AwardedFSSTrackingCustomerRatio,
							b.IsFSS,
							b.IsBig4,
							b.IsVA,
							b.IsDOD,
							b.IsBOP,
							b.IsHHS,
							b.IsIHS,
							b.IsIHS2,
							b.IsDIHS,
							b.IsSVH,
							b.IsSVH1,
							b.IsSVH2,
							b.IsPHS,
							b.IsUSCG,
							b.IsTMOP,
							b.IsCMOP,
							b.IsNIH,
							b.IsFHCC
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'T'
							and b.IsBig4 = 0 and b.IsFSS = 0
							and b.DrugItemSubitemId is null						
					Union
					Select a.DrugItemId,a.ContractId,a.DrugItemNDCId,b.DrugItemSubItemId,a.Covered,
							Case 
								When b.DrugItemSubItemId is not null then b.Generic
								Else a.Generic
							End as Generic,
							Case 
								When b.DrugItemSubItemId is not null then b.TradeName
								Else a.TradeName
							End as TradeName,
							Case 
								When b.DrugItemSubItemId is not null then b.DispensingUnit
								Else a.DispensingUnit
							End as Dispensingunit,	
							Case
								When b.DrugItemSubItemId is not null then b.PackageDescription
								Else a.PackageDescription
							End as PackageDescription,
							c.DrugItemPriceId,
							c.Price,
							c.IsTemporary,				
							c.priceStartDate,
							c.PriceStopDate,
							c.CurrentTrackingCustomerPrice,
							c.AwardedFSSTrackingCustomerRatio,
							c.IsFSS,
							c.IsBig4,
							c.IsVA,
							c.IsDOD,
							c.IsBOP,
							c.IsHHS,
							c.IsIHS,
							c.IsIHS2,
							c.IsDIHS,
							c.IsSVH,
							c.IsSVH1,
							c.IsSVH2,
							c.IsPHS,
							c.IsUSCG,
							c.IsTMOP,
							c.IsCMOP,
							c.IsNIH,
							c.IsFHCC
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'T'
							and c.IsBig4 = 0 and c.IsFSS = 0
							and c.DrugItemSubitemId = b.DrugItemSubitemId
				) Itm
				on cnt.ContractId = Itm.ContractId
			 join DI_DrugItemNDC ndc 
				on Itm.DrugItemNDCId = ndc.DrugItemNDCId
			 left outer join DI_DrugItemPackage ItmPkg 
				on Itm.DrugItemid = ItmPkg.DrugItemid
--			 left outer join DI_FCP FCP
--				on FCP.DrugItemId = Itm.DrugItemId
			 Where (	
						cnt.ContractNumber = @contractNumber or
						cnt.NACCMContractNumber = @contractNumber
				   )
			 And (
					(	DATEDIFF(day,@priceStartDate,Itm.PriceStartDate) > =0 And
						DATEDIFF(day,Itm.PriceStartDate,@priceStopDate) > =0
					) OR
					(
						DATEDIFF(day,Itm.PriceStartDate,@priceStartDate) > =0 And				
						DATEDIFF(day,@priceStartDate,Itm.PriceStopDate) > =0					
					)
				 )
			order by NDC1,NDC2,NDC3,[Temporary Price Start Date],[Temporary Price End Date]
		End
*/		Else
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
--				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current Price],
				Null as [Proposed Price without IFF],
				Null as [Proposed Price with IFF],
--				FCP.FCP as [FCP],
				Case	
					When   month(getdate()) = 10 or month(getdate()) = 11 or month(getdate()) = 12 then
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(GETDATE())+1) 
					When year(pricestartdate) <= YEAR(GETDATE()) then
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(GETDATE())) 
					else
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(pricestartdate)) 					
				End as [FCP],
				Case 
					When Itm.IsTemporary = 1 then Null 
					When  month(getdate()) = 10 or month(getdate()) = 11 or month(getdate()) = 12
						then 
							'01-01-' + CAST(year(getdate())+1 as varchar(4))					
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)
				End As [Start Date],
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [End Date],
				Null as [Remove   ( 1 = Remove Price )],
				Null as [Temporary Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary Price with IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)		 
				End As [Temporary Price Start Date],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [Temporary Price End Date],
--				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
--				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracing Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure],
--				Itm.IsFSS,
--				Itm.IsBig4,
				Case
					When Itm.IsVA= 0 then 0 else 1
				End as VA,
				Case
					When Itm.IsDOD= 0 then 0 else 1
				End as DOD,				
				Case
					When Itm.IsBOP= 0 then 0 else 1
				End as BOP,					
				Case
					When Itm.IsHHS= 0 then 0 else 1
				End as HHS,	
				Case
					When Itm.IsIHS= 0 then 0 else 1
				End as IHS,	
				Case
					When Itm.IsIHS2= 0 then 0 else 1
				End as IHS2,	
				Case
					When Itm.IsDIHS= 0 then 0 else 1
				End as DIHS,	
				Case
					When Itm.IsSVH= 0 then 0 else 1
				End as SVH,
				Case
					When Itm.IsSVH1= 0 then 0 else 1
				End as SVH1,
				Case
					When Itm.IsSVH2= 0 then 0 else 1
				End as SVH2,
				Case
					When Itm.IsPHS= 0 then 0 else 1
				End as PHS,
				Case
					When Itm.IsUSCG= 0 then 0 else 1
				End as USCG,
				Case
					When Itm.IsTMOP= 0 then 0 else 1
				End as TMOP,
				Case
					When Itm.IsCMOP= 0 then 0 else 1
				End as CMOP,
				Case
					When Itm.IsNIH= 0 then 0 else 1
				End as NIH,
				Case
					When Itm.IsFHCC= 0 then 0 else 1
				End as FHCC				
			 From Di_Contracts Cnt 
			 join 
				(	Select  a.DrugItemId,ContractId,DrugItemNDCId,Null as DrugItemSubItemId,Covered,
							Generic,
							TradeName,
							DispensingUnit,
							PackageDescription,
							b.DrugItemPriceId,
							b.Price,
							b.IsTemporary,
							b.priceStartDate,
							b.PriceStopDate,
							b.CurrentTrackingCustomerPrice,
							b.AwardedFSSTrackingCustomerRatio,
							b.IsFSS,
							b.IsBig4,
							b.IsVA,
							b.IsDOD,
							b.IsBOP,
							b.IsHHS,
							b.IsIHS,
							b.IsIHS2,
							b.IsDIHS,
							b.IsSVH,
							b.IsSVH1,
							b.IsSVH2,
							b.IsPHS,
							b.IsUSCG,
							b.IsTMOP,
							b.IsCMOP,
							b.IsNIH,
							b.IsFHCC
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'T'
							and b.IsBig4 = 0 and b.IsFSS = 0
							and b.DrugItemSubitemId is null						
					Union
					Select a.DrugItemId,a.ContractId,a.DrugItemNDCId,b.DrugItemSubItemId,a.Covered,
							Case 
								When b.DrugItemSubItemId is not null then b.Generic
								Else a.Generic
							End as Generic,
							Case 
								When b.DrugItemSubItemId is not null then b.TradeName
								Else a.TradeName
							End as TradeName,
							Case 
								When b.DrugItemSubItemId is not null then b.DispensingUnit
								Else a.DispensingUnit
							End as Dispensingunit,	
							Case
								When b.DrugItemSubItemId is not null then b.PackageDescription
								Else a.PackageDescription
							End as PackageDescription,
							c.DrugItemPriceId,
							c.Price,
							c.IsTemporary,				
							c.priceStartDate,
							c.PriceStopDate,
							c.CurrentTrackingCustomerPrice,
							c.AwardedFSSTrackingCustomerRatio,
							c.IsFSS,
							c.IsBig4,
							c.IsVA,
							c.IsDOD,
							c.IsBOP,
							c.IsHHS,
							c.IsIHS,
							c.IsIHS2,
							c.IsDIHS,
							c.IsSVH,
							c.IsSVH1,
							c.IsSVH2,
							c.IsPHS,
							c.IsUSCG,
							c.IsTMOP,
							c.IsCMOP,
							c.IsNIH,
							c.IsFHCC
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'T'
							and c.IsBig4 = 0 and c.IsFSS = 0
							and c.DrugItemSubitemId = b.DrugItemSubitemId
				) Itm
				on cnt.ContractId = Itm.ContractId
			 join DI_DrugItemNDC ndc 
				on Itm.DrugItemNDCId = ndc.DrugItemNDCId
			 left outer join DI_DrugItemPackage ItmPkg 
				on Itm.DrugItemid = ItmPkg.DrugItemid
--			 left outer join DI_FCP FCP
--				on FCP.DrugItemId = Itm.DrugItemId
			 Where (	
						cnt.ContractNumber = @contractNumber or
						cnt.NACCMContractNumber = @contractNumber
				   )
			 And (
					(	DATEDIFF(day,@priceStartDate,Itm.PriceStartDate) > =0 
					) OR
					(
						DATEDIFF(day,Itm.PriceStartDate,@priceStartDate) > =0 And				
						DATEDIFF(day,@priceStartDate,Itm.PriceStopDate) > =0					
					)
				 )
			 order by NDC1,NDC2,NDC3,[Temporary Price Start Date],[Temporary Price End Date]
		End
	End
	Else if @covered = 'F'
	Begin
		If @priceStartDate is null or @priceStopDate is null
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
--				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current Price],
				Null as [Proposed Price without IFF],
				Null as [Proposed Price with IFF],
--				FCP.FCP as [FCP],
				Case	
					When year(pricestartdate) <= YEAR(GETDATE()) then
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(GETDATE())) 
					else
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(pricestartdate)) 					
				End as [FCP],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)
				End As [Start Date],
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [End Date],
				Null as [Remove   ( 1 = Remove Price )],
				Null as [Temporary Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary Price with IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)		 
				End As [Temporary Price Start Date],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [Temporary Price End Date],
--				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
--				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure],
--				Itm.IsFSS,
--				Itm.IsBig4,
				Case
					When Itm.IsVA= 0 then 0 else 1
				End as VA,
				Case
					When Itm.IsDOD= 0 then 0 else 1
				End as DOD,				
				Case
					When Itm.IsBOP= 0 then 0 else 1
				End as BOP,					
				Case
					When Itm.IsHHS= 0 then 0 else 1
				End as HHS,	
				Case
					When Itm.IsIHS= 0 then 0 else 1
				End as IHS,	
				Case
					When Itm.IsIHS2= 0 then 0 else 1
				End as IHS2,	
				Case
					When Itm.IsDIHS= 0 then 0 else 1
				End as DIHS,	
				Case
					When Itm.IsSVH= 0 then 0 else 1
				End as SVH,
				Case
					When Itm.IsSVH1= 0 then 0 else 1
				End as SVH1,
				Case
					When Itm.IsSVH2= 0 then 0 else 1
				End as SVH2,
				Case
					When Itm.IsPHS= 0 then 0 else 1
				End as PHS,
				Case
					When Itm.IsUSCG= 0 then 0 else 1
				End as USCG,
				Case
					When Itm.IsTMOP= 0 then 0 else 1
				End as TMOP,
				Case
					When Itm.IsCMOP= 0 then 0 else 1
				End as CMOP,
				Case
					When Itm.IsNIH= 0 then 0 else 1
				End as NIH,
				Case
					When Itm.IsFHCC= 0 then 0 else 1
				End as FHCC				
			 From Di_Contracts Cnt 
			 join 
				(	Select  a.DrugItemId,ContractId,DrugItemNDCId,Null as DrugItemSubItemId,Covered,
							Generic,
							TradeName,
							DispensingUnit,
							PackageDescription,
							b.DrugItemPriceId,
							b.Price,
							b.IsTemporary,
							b.priceStartDate,
							b.PriceStopDate,
							b.CurrentTrackingCustomerPrice,
							b.AwardedFSSTrackingCustomerRatio,
							b.IsFSS,
							b.IsBig4,
							b.IsVA,
							b.IsDOD,
							b.IsBOP,
							b.IsHHS,
							b.IsIHS,
							b.IsIHS2,
							b.IsDIHS,
							b.IsSVH,
							b.IsSVH1,
							b.IsSVH2,
							b.IsPHS,
							b.IsUSCG,
							b.IsTMOP,
							b.IsCMOP,
							b.IsNIH,
							b.IsFHCC
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'F'
							and b.IsBig4 = 0 and b.IsFSS = 0
							and b.DrugItemSubitemId is null						
					Union
					Select a.DrugItemId,a.ContractId,a.DrugItemNDCId,b.DrugItemSubItemId,a.Covered,
							Case 
								When b.DrugItemSubItemId is not null then b.Generic
								Else a.Generic
							End as Generic,
							Case 
								When b.DrugItemSubItemId is not null then b.TradeName
								Else a.TradeName
							End as TradeName,
							Case 
								When b.DrugItemSubItemId is not null then b.DispensingUnit
								Else a.DispensingUnit
							End as Dispensingunit,	
							Case
								When b.DrugItemSubItemId is not null then b.PackageDescription
								Else a.PackageDescription
							End as PackageDescription,
							c.DrugItemPriceId,
							c.Price,
							c.IsTemporary,				
							c.priceStartDate,
							c.PriceStopDate,
							c.CurrentTrackingCustomerPrice,
							c.AwardedFSSTrackingCustomerRatio,
							c.IsFSS,
							c.IsBig4,
							c.IsVA,
							c.IsDOD,
							c.IsBOP,
							c.IsHHS,
							c.IsIHS,
							c.IsIHS2,
							c.IsDIHS,
							c.IsSVH,
							c.IsSVH1,
							c.IsSVH2,
							c.IsPHS,
							c.IsUSCG,
							c.IsTMOP,
							c.IsCMOP,
							c.IsNIH,
							c.IsFHCC
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'F' 
							and c.IsBig4 = 0 and c.IsFSS = 0
							and c.DrugItemSubitemId = b.DrugItemSubitemId
				) Itm
				on cnt.ContractId = Itm.ContractId
			 join DI_DrugItemNDC ndc 
				on Itm.DrugItemNDCId = ndc.DrugItemNDCId
			 left outer join DI_DrugItemPackage ItmPkg 
				on Itm.DrugItemid = ItmPkg.DrugItemid
--			 left outer join DI_FCP FCP
--				on FCP.DrugItemId = Itm.DrugItemId
			 Where (	
						cnt.ContractNumber = @contractNumber or
						cnt.NACCMContractNumber = @contractNumber
				   )
			order by NDC1,NDC2,NDC3,[Temporary Price Start Date],[Temporary Price End Date]
		End
/*		Else If (   @startDay = 1 And @startMonth = 1 And 
					@stopDay = 31 And @stopMonth = 12 And @startYear = @stopYear
				)
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
--				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current Price],
				Null as [Proposed Price without IFF],
				Null as [Proposed Price with IFF],
--				FCP.FCP as [FCP],
				dbo.GetFCPValueForDrugItem(Itm.DrugItemID,null) as [FCP],
				Case 
					When Itm.IsTemporary = 1 then Null 
					When Itm.IsTemporary = 0 
						And ((Day(Itm.PriceStartDate) =1 And MONTH(Itm.PriceStartDate) = 1 ) OR
							 (YEAR(Itm.PriceStartDate)<YEAR(@priceStartDate))
							)
						then
							substring(Convert(Varchar(10), @priceStartDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),1,4)				
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)
				End As [Start Date],
				Case 
					When Itm.IsTemporary = 1 then Null 
					When Itm.IsTemporary = 0 
						And ((Day(Itm.PriceStopDate) =12 And MONTH(Itm.PriceStopDate) = 31 )OR
							 (YEAR(Itm.PriceStopDate)>YEAR(@priceStopDate))
							) then	
							substring(Convert(Varchar(10), @priceStopDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),1,4)										
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [End Date],
				Null as [Remove   ( 1 = Remove Price )],
				Null as [Temporary Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary Price with IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					When Itm.IsTemporary = 1 
						And ((Day(Itm.PriceStartDate) =1 And MONTH(Itm.PriceStartDate) = 1 ) OR
							 (YEAR(Itm.PriceStartDate)<YEAR(@priceStartDate))
							)
						then
							substring(Convert(Varchar(10), @priceStartDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),1,4)				
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)		 
				End As [Temporary Price Start Date],
				Case 
					When Itm.IsTemporary = 0 then Null 
					When Itm.IsTemporary = 1 
						And ((Day(Itm.PriceStopDate) =12 And MONTH(Itm.PriceStopDate) = 31 )OR
							 (YEAR(Itm.PriceStopDate)>YEAR(@priceStopDate))
							) then	
							substring(Convert(Varchar(10), @priceStopDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),1,4)										
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [Temporary Price End Date],
--				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
--				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure],
--				Itm.IsFSS,
--				Itm.IsBig4,
				Case
					When Itm.IsVA= 0 then 0 else 1
				End as VA,
				Case
					When Itm.IsDOD= 0 then 0 else 1
				End as DOD,				
				Case
					When Itm.IsBOP= 0 then 0 else 1
				End as BOP,					
				Case
					When Itm.IsHHS= 0 then 0 else 1
				End as HHS,	
				Case
					When Itm.IsIHS= 0 then 0 else 1
				End as IHS,	
				Case
					When Itm.IsIHS2= 0 then 0 else 1
				End as IHS2,	
				Case
					When Itm.IsDIHS= 0 then 0 else 1
				End as DIHS,	
				Case
					When Itm.IsSVH= 0 then 0 else 1
				End as SVH,
				Case
					When Itm.IsSVH1= 0 then 0 else 1
				End as SVH1,
				Case
					When Itm.IsSVH2= 0 then 0 else 1
				End as SVH2,
				Case
					When Itm.IsPHS= 0 then 0 else 1
				End as PHS,
				Case
					When Itm.IsUSCG= 0 then 0 else 1
				End as USCG,
				Case
					When Itm.IsTMOP= 0 then 0 else 1
				End as TMOP,
				Case
					When Itm.IsCMOP= 0 then 0 else 1
				End as CMOP,
				Case
					When Itm.IsNIH= 0 then 0 else 1
				End as NIH,
				Case
					When Itm.IsFHCC= 0 then 0 else 1
				End as FHCC				
			 From Di_Contracts Cnt 
			 join 
				(	Select  a.DrugItemId,ContractId,DrugItemNDCId,Null as DrugItemSubItemId,Covered,
							Generic,
							TradeName,
							DispensingUnit,
							PackageDescription,
							b.DrugItemPriceId,
							b.Price,
							b.IsTemporary,
							b.priceStartDate,
							b.PriceStopDate,
							b.CurrentTrackingCustomerPrice,
							b.AwardedFSSTrackingCustomerRatio,
							b.IsFSS,
							b.IsBig4,
							b.IsVA,
							b.IsDOD,
							b.IsBOP,
							b.IsHHS,
							b.IsIHS,
							b.IsIHS2,
							b.IsDIHS,
							b.IsSVH,
							b.IsSVH1,
							b.IsSVH2,
							b.IsPHS,
							b.IsUSCG,
							b.IsTMOP,
							b.IsCMOP,
							b.IsNIH,
							b.ISFHCC
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'F'
							and b.IsBig4 = 0 and b.IsFSS = 0
							and b.DrugItemSubitemId is null						
					Union
					Select a.DrugItemId,a.ContractId,a.DrugItemNDCId,b.DrugItemSubItemId,a.Covered,
							Case 
								When b.DrugItemSubItemId is not null then b.Generic
								Else a.Generic
							End as Generic,
							Case 
								When b.DrugItemSubItemId is not null then b.TradeName
								Else a.TradeName
							End as TradeName,
							Case 
								When b.DrugItemSubItemId is not null then b.DispensingUnit
								Else a.DispensingUnit
							End as Dispensingunit,	
							Case
								When b.DrugItemSubItemId is not null then b.PackageDescription
								Else a.PackageDescription
							End as PackageDescription,
							c.DrugItemPriceId,
							c.Price,
							c.IsTemporary,				
							c.priceStartDate,
							c.PriceStopDate,
							c.CurrentTrackingCustomerPrice,
							c.AwardedFSSTrackingCustomerRatio,
							c.IsFSS,
							c.IsBig4,
							c.IsVA,
							c.IsDOD,
							c.IsBOP,
							c.IsHHS,
							c.IsIHS,
							c.IsIHS2,
							c.IsDIHS,
							c.IsSVH,
							c.IsSVH1,
							c.IsSVH2,
							c.IsPHS,
							c.IsUSCG,
							c.IsTMOP,
							c.IsCMOP,
							c.IsNIH,
							c.IsFHCC
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'F'
							and c.IsBig4 = 0 and c.IsFSS = 0
							and c.DrugItemSubitemId = b.DrugItemSubitemId
				) Itm
				on cnt.ContractId = Itm.ContractId
			 join DI_DrugItemNDC ndc 
				on Itm.DrugItemNDCId = ndc.DrugItemNDCId
			 left outer join DI_DrugItemPackage ItmPkg 
				on Itm.DrugItemid = ItmPkg.DrugItemid
--			 left outer join DI_FCP FCP
--				on FCP.DrugItemId = Itm.DrugItemId
			 Where (	
						cnt.ContractNumber = @contractNumber or
						cnt.NACCMContractNumber = @contractNumber
				   )
			 And (
					(	DATEDIFF(day,@priceStartDate,Itm.PriceStartDate) > =0 And
						DATEDIFF(day,Itm.PriceStartDate,@priceStopDate) > =0
					) OR
					(
						DATEDIFF(day,Itm.PriceStartDate,@priceStartDate) > =0 And				
						DATEDIFF(day,@priceStartDate,Itm.PriceStopDate) > =0					
					)
				 )
			order by NDC1,NDC2,NDC3,[Temporary Price Start Date],[Temporary Price End Date]
		End
*/		Else
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
--				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current Price],
				Null as [Proposed Price without IFF],
				Null as [Proposed Price with IFF],
--				FCP.FCP as [FCP],
				Case	
					When   month(getdate()) = 10 or month(getdate()) = 11 or month(getdate()) = 12 then
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(GETDATE())+1) 
					When year(pricestartdate) <= YEAR(GETDATE()) then
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(GETDATE())) 
					else
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(pricestartdate)) 					
				End as [FCP],
				Case 
					When Itm.IsTemporary = 1 then Null 
					When  month(getdate()) = 10 or month(getdate()) = 11 or month(getdate()) = 12
						then 
							'01-01-' + CAST(year(getdate())+1 as varchar(4))				
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)
				End As [Start Date],
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [End Date],
				Null as [Remove   ( 1 = Remove Price )],
				Null as [Temporary Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary Price with IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)		 
				End As [Temporary Price Start Date],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [Temporary Price End Date],
--				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
--				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracing Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure],
--				Itm.IsFSS,
--				Itm.IsBig4,
				Case
					When Itm.IsVA= 0 then 0 else 1
				End as VA,
				Case
					When Itm.IsDOD= 0 then 0 else 1
				End as DOD,				
				Case
					When Itm.IsBOP= 0 then 0 else 1
				End as BOP,					
				Case
					When Itm.IsHHS= 0 then 0 else 1
				End as HHS,	
				Case
					When Itm.IsIHS= 0 then 0 else 1
				End as IHS,	
				Case
					When Itm.IsIHS2= 0 then 0 else 1
				End as IHS2,	
				Case
					When Itm.IsDIHS= 0 then 0 else 1
				End as DIHS,	
				Case
					When Itm.IsSVH= 0 then 0 else 1
				End as SVH,
				Case
					When Itm.IsSVH1= 0 then 0 else 1
				End as SVH1,
				Case
					When Itm.IsSVH2= 0 then 0 else 1
				End as SVH2,
				Case
					When Itm.IsPHS= 0 then 0 else 1
				End as PHS,
				Case
					When Itm.IsUSCG= 0 then 0 else 1
				End as USCG,
				Case
					When Itm.IsTMOP= 0 then 0 else 1
				End as TMOP,
				Case
					When Itm.IsCMOP= 0 then 0 else 1
				End as CMOP,
				Case
					When Itm.IsNIH= 0 then 0 else 1
				End as NIH,
				Case
					When Itm.IsFHCC= 0 then 0 else 1
				End as FHCC				
			 From Di_Contracts Cnt 
			 join 
				(	Select  a.DrugItemId,ContractId,DrugItemNDCId,Null as DrugItemSubItemId,Covered,
							Generic,
							TradeName,
							DispensingUnit,
							PackageDescription,
							b.DrugItemPriceId,
							b.Price,
							b.IsTemporary,
							b.priceStartDate,
							b.PriceStopDate,
							b.CurrentTrackingCustomerPrice,
							b.AwardedFSSTrackingCustomerRatio,
							b.IsFSS,
							b.IsBig4,
							b.IsVA,
							b.IsDOD,
							b.IsBOP,
							b.IsHHS,
							b.IsIHS,
							b.IsIHS2,
							b.IsDIHS,
							b.IsSVH,
							b.IsSVH1,
							b.IsSVH2,
							b.IsPHS,
							b.IsUSCG,
							b.IsTMOP,
							b.IsCMOP,
							b.IsNIH,
							b.IsFHCC
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'F'
							and b.IsBig4 = 0 and b.IsFSS = 0
							and b.DrugItemSubitemId is null						
					Union
					Select a.DrugItemId,a.ContractId,a.DrugItemNDCId,b.DrugItemSubItemId,a.Covered,
							Case 
								When b.DrugItemSubItemId is not null then b.Generic
								Else a.Generic
							End as Generic,
							Case 
								When b.DrugItemSubItemId is not null then b.TradeName
								Else a.TradeName
							End as TradeName,
							Case 
								When b.DrugItemSubItemId is not null then b.DispensingUnit
								Else a.DispensingUnit
							End as Dispensingunit,	
							Case
								When b.DrugItemSubItemId is not null then b.PackageDescription
								Else a.PackageDescription
							End as PackageDescription,
							c.DrugItemPriceId,
							c.Price,
							c.IsTemporary,				
							c.priceStartDate,
							c.PriceStopDate,
							c.CurrentTrackingCustomerPrice,
							c.AwardedFSSTrackingCustomerRatio,
							c.IsFSS,
							c.IsBig4,
							c.IsVA,
							c.IsDOD,
							c.IsBOP,
							c.IsHHS,
							c.IsIHS,
							c.IsIHS2,
							c.IsDIHS,
							c.IsSVH,
							c.IsSVH1,
							c.IsSVH2,
							c.IsPHS,
							c.IsUSCG,
							c.IsTMOP,
							c.IsCMOP,
							c.IsNIH,
							c.IsFHCC
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.Covered = 'F'
							and c.IsBig4 = 0 and c.IsFSS = 0
							and c.DrugItemSubitemId = b.DrugItemSubitemId
				) Itm
				on cnt.ContractId = Itm.ContractId
			 join DI_DrugItemNDC ndc 
				on Itm.DrugItemNDCId = ndc.DrugItemNDCId
			 left outer join DI_DrugItemPackage ItmPkg 
				on Itm.DrugItemid = ItmPkg.DrugItemid
--			 left outer join DI_FCP FCP
--				on FCP.DrugItemId = Itm.DrugItemId
			 Where (	
						cnt.ContractNumber = @contractNumber or
						cnt.NACCMContractNumber = @contractNumber
				   )
			 And (
					(	DATEDIFF(day,@priceStartDate,Itm.PriceStartDate) > =0 
					) OR
					(
						DATEDIFF(day,Itm.PriceStartDate,@priceStartDate) > =0 And				
						DATEDIFF(day,@priceStartDate,Itm.PriceStopDate) > =0					
					)
				 )
			 order by NDC1,NDC2,NDC3,[Temporary Price Start Date],[Temporary Price End Date]
		End	
	End
	Else If @covered = 'B'
	Begin
		If @priceStartDate is null or @priceStopDate is null
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
--				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current Price],
				Null as [Proposed Price without IFF],
				Null as [Proposed Price with IFF],
--				FCP.FCP as [FCP],
				Case	
					When year(pricestartdate) <= YEAR(GETDATE()) then
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(GETDATE())) 
					else
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(pricestartdate)) 					
				End as [FCP],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)
				End As [Start Date],
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [End Date],
				Null as [Remove   ( 1 = Remove Price )],
				Null as [Temporary Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary Price with IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)		 
				End As [Temporary Price Start Date],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [Temporary Price End Date],
--				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
--				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure],
--				Itm.IsFSS,
--				Itm.IsBig4,
				Case
					When Itm.IsVA= 0 then 0 else 1
				End as VA,
				Case
					When Itm.IsDOD= 0 then 0 else 1
				End as DOD,				
				Case
					When Itm.IsBOP= 0 then 0 else 1
				End as BOP,					
				Case
					When Itm.IsHHS= 0 then 0 else 1
				End as HHS,	
				Case
					When Itm.IsIHS= 0 then 0 else 1
				End as IHS,	
				Case
					When Itm.IsIHS2= 0 then 0 else 1
				End as IHS2,	
				Case
					When Itm.IsDIHS= 0 then 0 else 1
				End as DIHS,	
				Case
					When Itm.IsSVH= 0 then 0 else 1
				End as SVH,
				Case
					When Itm.IsSVH1= 0 then 0 else 1
				End as SVH1,
				Case
					When Itm.IsSVH2= 0 then 0 else 1
				End as SVH2,
				Case
					When Itm.IsPHS= 0 then 0 else 1
				End as PHS,
				Case
					When Itm.IsUSCG= 0 then 0 else 1
				End as USCG,
				Case
					When Itm.IsTMOP= 0 then 0 else 1
				End as TMOP,
				Case
					When Itm.IsCMOP= 0 then 0 else 1
				End as CMOP,
				Case
					When Itm.IsNIH= 0 then 0 else 1
				End as NIH,
				Case
					When Itm.IsFHCC= 0 then 0 else 1
				End as FHCC					
			 From Di_Contracts Cnt 
			 join 
				(	Select  a.DrugItemId,ContractId,DrugItemNDCId,Null as DrugItemSubItemId,Covered,
							Generic,
							TradeName,
							DispensingUnit,
							PackageDescription,
							b.DrugItemPriceId,
							b.Price,
							b.IsTemporary,
							b.priceStartDate,
							b.PriceStopDate,
							b.CurrentTrackingCustomerPrice,
							b.AwardedFSSTrackingCustomerRatio,
							b.IsFSS,
							b.IsBig4,
							b.IsVA,
							b.IsDOD,
							b.IsBOP,
							b.IsHHS,
							b.IsIHS,
							b.IsIHS2,
							b.IsDIHS,
							b.IsSVH,
							b.IsSVH1,
							b.IsSVH2,
							b.IsPHS,
							b.IsUSCG,
							b.IsTMOP,
							b.IsCMOP,
							b.IsNIH,
							b.IsFHCC
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where b.IsBig4 = 0 and b.IsFSS = 0
							and b.DrugItemSubitemId is null						
					Union
					Select a.DrugItemId,a.ContractId,a.DrugItemNDCId,b.DrugItemSubItemId,a.Covered,
							Case 
								When b.DrugItemSubItemId is not null then b.Generic
								Else a.Generic
							End as Generic,
							Case 
								When b.DrugItemSubItemId is not null then b.TradeName
								Else a.TradeName
							End as TradeName,
							Case 
								When b.DrugItemSubItemId is not null then b.DispensingUnit
								Else a.DispensingUnit
							End as Dispensingunit,	
							Case
								When b.DrugItemSubItemId is not null then b.PackageDescription
								Else a.PackageDescription
							End as PackageDescription,
							c.DrugItemPriceId,
							c.Price,
							c.IsTemporary,				
							c.priceStartDate,
							c.PriceStopDate,
							c.CurrentTrackingCustomerPrice,
							c.AwardedFSSTrackingCustomerRatio,
							c.IsFSS,
							c.IsBig4,
							c.IsVA,
							c.IsDOD,
							c.IsBOP,
							c.IsHHS,
							c.IsIHS,
							c.IsIHS2,
							c.IsDIHS,
							c.IsSVH,
							c.IsSVH1,
							c.IsSVH2,
							c.IsPHS,
							c.IsUSCG,
							c.IsTMOP,
							c.IsCMOP,
							c.IsNIH,
							c.IsFHCC
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where c.IsBig4 = 0 and c.IsFSS = 0
							and c.DrugItemSubitemId = b.DrugItemSubitemId
				) Itm
				on cnt.ContractId = Itm.ContractId
			 join DI_DrugItemNDC ndc 
				on Itm.DrugItemNDCId = ndc.DrugItemNDCId
			 left outer join DI_DrugItemPackage ItmPkg 
				on Itm.DrugItemid = ItmPkg.DrugItemid
--			 left outer join DI_FCP FCP
--				on FCP.DrugItemId = Itm.DrugItemId
			 Where (	
						cnt.ContractNumber = @contractNumber or
						cnt.NACCMContractNumber = @contractNumber
				   )
			order by NDC1,NDC2,NDC3,[Temporary Price Start Date],[Temporary Price End Date]
		End
/*		Else If (   @startDay = 1 And @startMonth = 1 And 
					@stopDay = 31 And @stopMonth = 12 And @startYear = @stopYear
				)
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
--				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current Price],
				Null as [Proposed Price without IFF],
				Null as [Proposed Price with IFF],
--				FCP.FCP as [FCP],
				dbo.GetFCPValueForDrugItem(Itm.DrugItemID,null) as [FCP],
				Case 
					When Itm.IsTemporary = 1 then Null 
					When Itm.IsTemporary = 0 
						And ((Day(Itm.PriceStartDate) =1 And MONTH(Itm.PriceStartDate) = 1 ) OR
							 (YEAR(Itm.PriceStartDate)<YEAR(@priceStartDate))
							)
						then
							substring(Convert(Varchar(10), @priceStartDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),1,4)				
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)
				End As [Start Date],
				Case 
					When Itm.IsTemporary = 1 then Null 
					When Itm.IsTemporary = 0 
						And ((Day(Itm.PriceStopDate) =12 And MONTH(Itm.PriceStopDate) = 31 )OR
							 (YEAR(Itm.PriceStopDate)>YEAR(@priceStopDate))
							) then	
							substring(Convert(Varchar(10), @priceStopDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),1,4)										
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [End Date],
				Null as [Remove   ( 1 = Remove Price )],
				Null as [Temporary Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary Price with IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					When Itm.IsTemporary = 1 
						And ((Day(Itm.PriceStartDate) =1 And MONTH(Itm.PriceStartDate) = 1 ) OR
							 (YEAR(Itm.PriceStartDate)<YEAR(@priceStartDate))
							)
						then
							substring(Convert(Varchar(10), @priceStartDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStartDate ,20),1,4)				
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)		 
				End As [Temporary Price Start Date],
				Case 
					When Itm.IsTemporary = 0 then Null 
					When Itm.IsTemporary = 1 
						And ((Day(Itm.PriceStopDate) =12 And MONTH(Itm.PriceStopDate) = 31 )OR
							 (YEAR(Itm.PriceStopDate)>YEAR(@priceStopDate))
							) then	
							substring(Convert(Varchar(10), @priceStopDate ,20),6,2) + '-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),9,2) +'-'+
							substring(Convert(Varchar(10), @priceStopDate ,20),1,4)										
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [Temporary Price End Date],
--				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
--				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure],
--				Itm.IsFSS,
--				Itm.IsBig4,
				Case
					When Itm.IsVA= 0 then 0 else 1
				End as VA,
				Case
					When Itm.IsDOD= 0 then 0 else 1
				End as DOD,				
				Case
					When Itm.IsBOP= 0 then 0 else 1
				End as BOP,					
				Case
					When Itm.IsHHS= 0 then 0 else 1
				End as HHS,	
				Case
					When Itm.IsIHS= 0 then 0 else 1
				End as IHS,	
				Case
					When Itm.IsIHS2= 0 then 0 else 1
				End as IHS2,	
				Case
					When Itm.IsDIHS= 0 then 0 else 1
				End as DIHS,	
				Case
					When Itm.IsSVH= 0 then 0 else 1
				End as SVH,
				Case
					When Itm.IsSVH1= 0 then 0 else 1
				End as SVH1,
				Case
					When Itm.IsSVH2= 0 then 0 else 1
				End as SVH2,
				Case
					When Itm.IsPHS= 0 then 0 else 1
				End as PHS,
				Case
					When Itm.IsUSCG= 0 then 0 else 1
				End as USCG,
				Case
					When Itm.IsTMOP= 0 then 0 else 1
				End as TMOP,
				Case
					When Itm.IsCMOP= 0 then 0 else 1
				End as CMOP,
				Case
					When Itm.IsNIH= 0 then 0 else 1
				End as NIH,
				Case
					When Itm.IsFHCC= 0 then 0 else 1
				End as FHCC					
			 From Di_Contracts Cnt 
			 join 
				(	Select  a.DrugItemId,ContractId,DrugItemNDCId,Null as DrugItemSubItemId,Covered,
							Generic,
							TradeName,
							DispensingUnit,
							PackageDescription,
							b.DrugItemPriceId,
							b.Price,
							b.IsTemporary,
							b.priceStartDate,
							b.PriceStopDate,
							b.CurrentTrackingCustomerPrice,
							b.AwardedFSSTrackingCustomerRatio,
							b.IsFSS,
							b.IsBig4,
							b.IsVA,
							b.IsDOD,
							b.IsBOP,
							b.IsHHS,
							b.IsIHS,
							b.IsIHS2,
							b.IsDIHS,
							b.IsSVH,
							b.IsSVH1,
							b.IsSVH2,
							b.IsPHS,
							b.IsUSCG,
							b.IsTMOP,
							b.IsCMOP,
							b.IsNIH,
							b.IsFHCC
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where b.IsBig4 = 0 and b.IsFSS = 0
							and b.DrugItemSubitemId is null						
					Union
					Select a.DrugItemId,a.ContractId,a.DrugItemNDCId,b.DrugItemSubItemId,a.Covered,
							Case 
								When b.DrugItemSubItemId is not null then b.Generic
								Else a.Generic
							End as Generic,
							Case 
								When b.DrugItemSubItemId is not null then b.TradeName
								Else a.TradeName
							End as TradeName,
							Case 
								When b.DrugItemSubItemId is not null then b.DispensingUnit
								Else a.DispensingUnit
							End as Dispensingunit,	
							Case
								When b.DrugItemSubItemId is not null then b.PackageDescription
								Else a.PackageDescription
							End as PackageDescription,
							c.DrugItemPriceId,
							c.Price,
							c.IsTemporary,				
							c.priceStartDate,
							c.PriceStopDate,
							c.CurrentTrackingCustomerPrice,
							c.AwardedFSSTrackingCustomerRatio,
							c.IsFSS,
							c.IsBig4,
							c.IsVA,
							c.IsDOD,
							c.IsBOP,
							c.IsHHS,
							c.IsIHS,
							c.IsIHS2,
							c.IsDIHS,
							c.IsSVH,
							c.IsSVH1,
							c.IsSVH2,
							c.IsPHS,
							c.IsUSCG,
							c.IsTMOP,
							c.IsCMOP,
							c.IsNIH,
							c.IsFHCC
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where c.IsBig4 = 0 and c.IsFSS = 0
							and c.DrugItemSubitemId = b.DrugItemSubitemId
				) Itm
				on cnt.ContractId = Itm.ContractId
			 join DI_DrugItemNDC ndc 
				on Itm.DrugItemNDCId = ndc.DrugItemNDCId
			 left outer join DI_DrugItemPackage ItmPkg 
				on Itm.DrugItemid = ItmPkg.DrugItemid
--			 left outer join DI_FCP FCP
--				on FCP.DrugItemId = Itm.DrugItemId
			 Where (	
						cnt.ContractNumber = @contractNumber or
						cnt.NACCMContractNumber = @contractNumber
				   )
			 And (
					(	DATEDIFF(day,@priceStartDate,Itm.PriceStartDate) > =0 And
						DATEDIFF(day,Itm.PriceStartDate,@priceStopDate) > =0
					) OR
					(
						DATEDIFF(day,Itm.PriceStartDate,@priceStartDate) > =0 And				
						DATEDIFF(day,@priceStartDate,Itm.PriceStopDate) > =0					
					)
				 )
			order by NDC1,NDC2,NDC3,[Temporary Price Start Date],[Temporary Price End Date]
		End
*/		Else
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
--				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current Price],
				Null as [Proposed Price without IFF],
				Null as [Proposed Price with IFF],
--				FCP.FCP as [FCP],
				Case	
					When   month(getdate()) = 10 or month(getdate()) = 11 or month(getdate()) = 12 then
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(GETDATE())+1) 
					When year(pricestartdate) <= YEAR(GETDATE()) then
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(GETDATE())) 
					else
						dbo.GetFCPValueForDrugItem(Itm.DrugItemID,YEAR(pricestartdate)) 					
				End as [FCP],
				Case 
					When Itm.IsTemporary = 1 then Null 
					When  month(getdate()) = 10 or month(getdate()) = 11 or month(getdate()) = 12
						then 
							'01-01-' + CAST(year(getdate())+1 as varchar(4))					
					Else 
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)
				End As [Start Date],
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [End Date],
				Null as [Remove   ( 1 = Remove Price )],
				Null as [Temporary Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary Price with IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStartDate ,20),1,4)		 
				End As [Temporary Price Start Date],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else 
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),6,2) + '-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),9,2) +'-'+
						substring(Convert(Varchar(10), Itm.PriceStopDate ,20),1,4)		
				End As [Temporary Price End Date],
--				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
--				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracing Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure],
--				Itm.IsFSS,
--				Itm.IsBig4,
				Case
					When Itm.IsVA= 0 then 0 else 1
				End as VA,
				Case
					When Itm.IsDOD= 0 then 0 else 1
				End as DOD,				
				Case
					When Itm.IsBOP= 0 then 0 else 1
				End as BOP,					
				Case
					When Itm.IsHHS= 0 then 0 else 1
				End as HHS,	
				Case
					When Itm.IsIHS= 0 then 0 else 1
				End as IHS,	
				Case
					When Itm.IsIHS2= 0 then 0 else 1
				End as IHS2,	
				Case
					When Itm.IsDIHS= 0 then 0 else 1
				End as DIHS,	
				Case
					When Itm.IsSVH= 0 then 0 else 1
				End as SVH,
				Case
					When Itm.IsSVH1= 0 then 0 else 1
				End as SVH1,
				Case
					When Itm.IsSVH2= 0 then 0 else 1
				End as SVH2,
				Case
					When Itm.IsPHS= 0 then 0 else 1
				End as PHS,
				Case
					When Itm.IsUSCG= 0 then 0 else 1
				End as USCG,
				Case
					When Itm.IsTMOP= 0 then 0 else 1
				End as TMOP,
				Case
					When Itm.IsCMOP= 0 then 0 else 1
				End as CMOP,
				Case
					When Itm.IsNIH= 0 then 0 else 1
				End as NIH,
				Case
					When Itm.IsFHCC= 0 then 0 else 1
				End as FHCC					
			 From Di_Contracts Cnt 
			 join 
				(	Select  a.DrugItemId,ContractId,DrugItemNDCId,Null as DrugItemSubItemId,Covered,
							Generic,
							TradeName,
							DispensingUnit,
							PackageDescription,
							b.DrugItemPriceId,
							b.Price,
							b.IsTemporary,
							b.priceStartDate,
							b.PriceStopDate,
							b.CurrentTrackingCustomerPrice,
							b.AwardedFSSTrackingCustomerRatio,
							b.IsFSS,
							b.IsBig4,
							b.IsVA,
							b.IsDOD,
							b.IsBOP,
							b.IsHHS,
							b.IsIHS,
							b.IsIHS2,
							b.IsDIHS,
							b.IsSVH,
							b.IsSVH1,
							b.IsSVH2,
							b.IsPHS,
							b.IsUSCG,
							b.IsTMOP,
							b.IsCMOP,
							b.IsNIH,
							b.IsFHCC
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where b.IsBig4 = 0 and b.IsFSS = 0
							and b.DrugItemSubitemId is null						
					Union
					Select a.DrugItemId,a.ContractId,a.DrugItemNDCId,b.DrugItemSubItemId,a.Covered,
							Case 
								When b.DrugItemSubItemId is not null then b.Generic
								Else a.Generic
							End as Generic,
							Case 
								When b.DrugItemSubItemId is not null then b.TradeName
								Else a.TradeName
							End as TradeName,
							Case 
								When b.DrugItemSubItemId is not null then b.DispensingUnit
								Else a.DispensingUnit
							End as Dispensingunit,	
							Case
								When b.DrugItemSubItemId is not null then b.PackageDescription
								Else a.PackageDescription
							End as PackageDescription,
							c.DrugItemPriceId,
							c.Price,
							c.IsTemporary,				
							c.priceStartDate,
							c.PriceStopDate,
							c.CurrentTrackingCustomerPrice,
							c.AwardedFSSTrackingCustomerRatio,
							c.IsFSS,
							c.IsBig4,
							c.IsVA,
							c.IsDOD,
							c.IsBOP,
							c.IsHHS,
							c.IsIHS,
							c.IsIHS2,
							c.IsDIHS,
							c.IsSVH,
							c.IsSVH1,
							c.IsSVH2,
							c.IsPHS,
							c.IsUSCG,
							c.IsTMOP,
							c.IsCMOP,
							c.IsNIH,
							c.IsFHCC
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where c.IsBig4 = 0 and c.IsFSS = 0
							and c.DrugItemSubitemId = b.DrugItemSubitemId
				) Itm
				on cnt.ContractId = Itm.ContractId
			 join DI_DrugItemNDC ndc 
				on Itm.DrugItemNDCId = ndc.DrugItemNDCId
			 left outer join DI_DrugItemPackage ItmPkg 
				on Itm.DrugItemid = ItmPkg.DrugItemid
--			 left outer join DI_FCP FCP
--				on FCP.DrugItemId = Itm.DrugItemId
			 Where (	
						cnt.ContractNumber = @contractNumber or
						cnt.NACCMContractNumber = @contractNumber
				   )
			 And (
					(	DATEDIFF(day,@priceStartDate,Itm.PriceStartDate) > =0 
					) OR
					(
						DATEDIFF(day,Itm.PriceStartDate,@priceStartDate) > =0 And				
						DATEDIFF(day,@priceStartDate,Itm.PriceStopDate) > =0					
					)
				 )
			 order by NDC1,NDC2,NDC3,[Temporary Price Start Date],[Temporary Price End Date]
		End	
	End
