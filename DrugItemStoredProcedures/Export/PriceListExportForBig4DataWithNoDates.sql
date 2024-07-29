IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'PriceListExportForBig4DataWithNoDates')
	BEGIN
		DROP  Procedure  PriceListExportForBig4DataWithNoDates
	END

GO

CREATE Procedure PriceListExportForBig4DataWithNoDates
(@contractNumber nvarchar(20),
 @covered char(1)
)
As 

	If @covered = 'T'
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
			End As [Current Big4 Price],
			Null as [Proposed Big4 Price without IFF],
			Null as [Proposed Big4 Price with IFF],
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
			Null as [Temporary Big4 Price without IFF],
			Case 
				When Itm.IsTemporary = 0 then Null 
				Else Itm.Price
			End As [Temporary Big4 Price with IFF],
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
						Where a.Covered = 'T'
						and b.IsBig4 = 1
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
						Where a.Covered = 'T'
						and c.IsBig4 = 1
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
	Else 
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
			End As [Current Big4 Price],
			Null as [Proposed Big4 Price without IFF],
			Null as [Proposed Big4 Price with IFF],
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
			Null as [Temporary Big4 Price without IFF],
			Case 
				When Itm.IsTemporary = 0 then Null 
				Else Itm.Price
			End As [Temporary Big4 Price with IFF],
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
						Where b.IsBig4 = 1
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
						Where c.IsBig4 = 1
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
