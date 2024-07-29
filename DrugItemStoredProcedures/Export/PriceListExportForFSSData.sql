IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'PriceListExportForFSSData')
	BEGIN
		DROP  Procedure  PriceListExportForFSSData
	END

GO

CREATE Procedure PriceListExportForFSSData
(@contractNumber nvarchar(20),
 @Covered char(1),
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

	If @Covered = 'T'
	Begin
		If (@priceStartDate is null or @priceStopDate is null) 
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current FSS Price],
				Null as [Proposed FSS Price without IFF],
				Null as [Proposed FSS Price with IFF],
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
				Null as [Temporary FSS Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary FSS Price with IFF],
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
				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure]
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
							b.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.covered = 'T'
							and b.IsFss = 1
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
							c.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.covered = 'T'
							and c.IsFss = 1
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
				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current FSS Price],
				Null as [Proposed FSS Price without IFF],
				Null as [Proposed FSS Price with IFF],
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
				Null as [Temporary FSS Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary FSS Price with IFF],
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
				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure]
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
							b.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.covered = 'T'
							and b.IsFss = 1
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
							c.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.covered = 'T' 
							and c.IsFss = 1
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
				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current FSS Price],
				Null as [Proposed FSS Price without IFF],
				Null as [Proposed FSS Price with IFF],
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
				Null as [Temporary FSS Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary FSS Price with IFF],
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
				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure]
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
							b.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.covered = 'T'
							and b.IsFss = 1
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
							c.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.covered = 'T'
							and c.IsFss = 1
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
	Else If @Covered = 'F'
	Begin
		If @priceStartDate is null or @priceStopDate is null
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current FSS Price],
				Null as [Proposed FSS Price without IFF],
				Null as [Proposed FSS Price with IFF],
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
				Null as [Temporary FSS Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary FSS Price with IFF],
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
				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure]
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
							b.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.covered = 'F'
							and b.IsFss = 1
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
							c.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.covered = 'F'
							and c.IsFss = 1
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
				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current FSS Price],
				Null as [Proposed FSS Price without IFF],
				Null as [Proposed FSS Price with IFF],
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
				Null as [Temporary FSS Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary FSS Price with IFF],
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
				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure]
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
							b.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.covered = 'F'
							and b.IsFss = 1
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
							c.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.covered = 'F' 
							and c.IsFss = 1
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
				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current FSS Price],
				Null as [Proposed FSS Price without IFF],
				Null as [Proposed FSS Price with IFF],
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
				Null as [Temporary FSS Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary FSS Price with IFF],
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
				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure]
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
							b.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where a.covered = 'F'
							and b.IsFss = 1
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
							c.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where a.covered = 'F'
							and c.IsFss = 1
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
	Else If @Covered = 'B'
	Begin
		If @priceStartDate is null or @priceStopDate is null
		Begin
			select Distinct
				Itm.DrugItemPriceId as [Index ( Do Not Alter )],
				ndc.FdaAssignedLabelerCode as [NDC1],
				ndc.ProductCode as [NDC2],
				ndc.PackageCode as [NDC3],
				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current FSS Price],
				Null as [Proposed FSS Price without IFF],
				Null as [Proposed FSS Price with IFF],
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
				Null as [Temporary FSS Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary FSS Price with IFF],
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
				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure]
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
							b.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where b.IsFss = 1
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
							c.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where c.IsFss = 1
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
				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current FSS Price],
				Null as [Proposed FSS Price without IFF],
				Null as [Proposed FSS Price with IFF],
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
				Null as [Temporary FSS Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary FSS Price with IFF],
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
				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure]
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
							b.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where b.IsFss = 1
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
							c.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where c.IsFss = 1
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
				Itm.Covered as [Covered Indicator],
				Itm.Generic as [Generic Name],
				Itm.TradeName as [Trade Name],
				Itm.DispensingUnit as [Dispensing unit],	
				Itm.PackageDescription as [Package Size],	
				Case 
					When Itm.IsTemporary = 1 then Null 
					Else Itm.Price
				End As [Current FSS Price],
				Null as [Proposed FSS Price without IFF],
				Null as [Proposed FSS Price with IFF],
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
				Null as [Temporary FSS Price without IFF],
				Case 
					When Itm.IsTemporary = 0 then Null 
					Else Itm.Price
				End As [Temporary FSS Price with IFF],
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
				Itm.CurrentTrackingCustomerPrice as [Current Tracking Customer Price],
				Itm.AwardedFSSTrackingCustomerRatio as [Awarded Fss Tracking Customer Ratio],
				ItmPkg.UnitofSale as [Unit Of Sale],
				ItmPkg.QuantityInUnitOfSale as [Quantity In Unit Of Sale],
				ItmPkg.UnitPackage as [Unit Of Packaging],
				ItmPkg.QuantityInUnitPackage as [Quantity In Unit Of Packaging],
				ItmPkg.UnitOfMeasure as [Unit Of Measure]
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
							b.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice b
							Join DI_DrugItems a
							on a.DrugItemId = b.DrugItemId
							Where b.IsFss = 1
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
							c.AwardedFSSTrackingCustomerRatio
							From DI_DrugItemPrice c
							Join DI_DrugItems a 
								on c.DrugItemId = a.DrugItemId
							Join DI_DrugItemSubItems b 
								on a.DrugItemId = b.DrugItemId
							Where c.IsFss = 1
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
