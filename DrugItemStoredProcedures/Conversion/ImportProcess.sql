IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ImportProcess]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ImportProcess]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc Proc [dbo].[ImportProcess]
as


Exec UpdateDatesForPBMTables

truncate table DI_ItemStatus
dbcc checkident(DI_ItemStatus,reseed,1)

truncate table DI_NDCStatus
dbcc checkident(DI_NDCStatus,reseed,1)

truncate table DI_FSSDataFixStatus
dbcc checkident(DI_FSSDataFixStatus,reseed,1)

truncate table DI_ItemFCPPriceStatus
dbcc checkident(DI_ItemFCPPriceStatus,reseed,1)

truncate table DI_ItemFSSPriceStatus
dbcc checkident(DI_ItemFSSPriceStatus,reseed,1)

truncate table DI_ItemFSSRPriceStatus
dbcc checkident(DI_ItemFSSRPriceStatus,reseed,1)

truncate table DI_ItemNCFPriceStatus
dbcc checkident(DI_ItemNCFPriceStatus,reseed,1)

truncate table DI_ItemPackageStatus
dbcc checkident(DI_ItemPackageStatus,reseed,1)

truncate table DI_ContractNDCNumberChangeStatus
dbcc checkident(DI_ContractNDCNumberChangeStatus,reseed,1)

truncate table DI_DrugItemTieredPriceHistory
dbcc checkident(DI_DrugItemTieredPriceHistory,reseed,1)

truncate table DI_DrugItemTieredPrice
dbcc checkident(DI_DrugItemTieredPrice,reseed,1)

truncate table DI_ContractNDCNumberChange
dbcc checkident(DI_ContractNDCNumberChange,reseed,1)

truncate table DI_ModificationStatus
dbcc checkident(DI_ModificationStatus,reseed,1)

truncate table DI_DrugItemNFAMPHistory
dbcc checkident(DI_DrugItemNFAMPHistory,reseed,1)

truncate table DI_DrugItemNFAMP
dbcc checkident(DI_DrugItemNFAMP,reseed,1)

truncate table DI_DrugItemSubItemsHistory
dbcc checkident(DI_DrugItemSubItemsHistory,reseed,1)

truncate table DI_DrugItemSubItems
dbcc checkident(DI_DrugItemSubItems,reseed,1)

truncate table DI_DrugItemPriceHistory
dbcc checkident(DI_DrugItemPriceHistory,reseed,1)

truncate table DI_DrugItemPrice
dbcc checkident(DI_DrugItemPrice,reseed,1)

truncate table DI_DrugItemPackageHistory
dbcc checkident(DI_DrugItemPackageHistory,reseed,1)

truncate table DI_DrugItemPackage
dbcc checkident(DI_DrugItemPackage,reseed,1)

truncate table DI_DrugItemNDC
dbcc checkident(DI_DrugItemNDC,reseed,1)

truncate table DI_DrugItemsHistory
dbcc checkident(DI_DrugItemsHistory,reseed,1)

truncate table DI_DrugItems
dbcc checkident(DI_DrugItems,reseed,1)

truncate table DI_ContractsHistory
dbcc checkident(DI_ContractsHistory,reseed,1)

truncate table DI_Contracts
dbcc checkident(DI_Contracts,reseed,1)

truncate table di_itembpaitemstatus
dbcc checkident(di_itembpaitemstatus,reseed,1)

truncate table di_itembpapricestatus
dbcc checkident(di_itembpapricestatus,reseed,1)


Alter table DI_drugItemPrice
Alter Column PriceId int null

Alter table DI_drugItemPriceHistory
Alter Column PriceId int null

exec dbo.ProcessContractsData
exec dbo.ProcessNCFContractsData

exec dbo.ProcessNDCData

exec dbo.ProcessItemsAndPackageData
exec dbo.FixNDCLinkItems


exec dbo.ProcessNCFItemsData

exec dbo.ProcessFSSItemsData


exec dbo.ProcessFCPItemsData


exec dbo.ProcessFSSRItemsData

     

exec dbo.ProcessNCFItemPriceData


exec dbo.ProcessHistoryItemPrices

exec dbo.IntialPriceIdAdjustmentForDrugItemPrice
exec dbo.IntialPriceIdAdjustmentForDrugItemPriceHistory


Alter table DI_drugItemPrice
Alter Column PriceId int not null

Alter table DI_drugItemPriceHistory
Alter Column PriceId int not null

exec dbo.ProcessContractNumberChange

-- update naccmcontractid
update a
set a.naccmcontractid = b.contract_record_id,
	 a.naccmcontractnumber = b.cntrctnum
from di_contracts a
join NAC_CM.dbo.tbl_Cntrcts b
on a.ContractNumber = b.cntrctnum 
where a.NACCMContractId is null	

-- process missing contract numbers
exec dbo.ProcessMissingContractsForScheduleNumber1

-- update parent fss contract 

update a	
	set a.parentfsscontractid = 
	(Select contractid from di_contracts where naccmcontractid = b.Parent_contract_Record_Id)
From DI_Contracts a
join 
(Select a.Contract_Record_ID as BPA_Contract_Record_Id,
		b.Contract_Record_ID as Parent_contract_Record_Id
From nac_cm.dbo.tbl_cntrcts a
join nac_cm.dbo.tbl_cntrcts b
on a.bpa_fss_counterpart = b.cntrctnum
where a.bpa_fss_counterpart is not null
)b
on a.NACCMContractId = b.BPA_Contract_Record_Id

-- Update Tracking Cutomer ratio
update a
	set a.AwardedFSSTrackingCustomerRatio = b.AwardedFSSTrackingCustomerRatio,
		a.CurrentTrackingCustomerPrice = b.CurrentTrackingCustomerPrice
From DI_DrugItemPrice a
join
(
	Select c.NACCMContractNumber,d.FdaAssignedLabelerCode,d.ProductCode,d.PackageCode,
	e.AwardedFSSTrackingCustomerRatio,e.CurrentTrackingCustomerPrice,
	a.drugitempriceid		
	From DI_DrugItemPrice a
	join DI_DrugItems b
	on a.DrugItemId = b.DrugItemId
	join DI_Contracts c
	on b.ContractId = c.ContractId
	join DI_DrugItemNDC d
	on b.DrugItemNDCId = d.DrugItemNDCId
	join DI_TrackingCustomerRatio e
	on c.NACCMContractNumber = e.NACCMContractNumber
	and e.FdaAssignedLabelerCode = d.FdaAssignedLabelerCode
	and e.ProductCode = d.ProductCode
	and e.PackageCode = d.PackageCode
	where a.IsFSS = 1
	and e.IsFSS = 1
) b
on a.DrugItemPriceId = b.DrugItemPriceId


update a
	set a.AwardedFSSTrackingCustomerRatio = b.AwardedFSSTrackingCustomerRatio,
		a.CurrentTrackingCustomerPrice = b.CurrentTrackingCustomerPrice
From DI_DrugItemPrice a
join
(
	Select c.NACCMContractNumber,d.FdaAssignedLabelerCode,d.ProductCode,d.PackageCode,
	e.AwardedFSSTrackingCustomerRatio,e.CurrentTrackingCustomerPrice,
	a.drugitempriceid		
	From DI_DrugItemPrice a
	join DI_DrugItems b
	on a.DrugItemId = b.DrugItemId
	join DI_Contracts c
	on b.ContractId = c.ContractId
	join DI_DrugItemNDC d
	on b.DrugItemNDCId = d.DrugItemNDCId
	join DI_TrackingCustomerRatio e
	on c.NACCMContractNumber = e.NACCMContractNumber
	and e.FdaAssignedLabelerCode = d.FdaAssignedLabelerCode
	and e.ProductCode = d.ProductCode
	and e.PackageCode = d.PackageCode
	where a.Isbig4 = 1
	and e.Isbig4 = 1
) b
on a.DrugItemPriceId = b.DrugItemPriceId

-- BPA transformation

Exec ProcessBPAContractsAndItems

exec processbpaprices

-- Update price stop dates

truncate table testNFAMPProcess
dbcc checkident(testNFAMPProcess,reseed,1)


exec ProcessPriceStopDateForFSS
exec ProcessPriceStopDateForBig4
exec ProcessPriceStopDateForFSSR

update z
	set z.PriceStopDate = a.ExpectedStopDate,
		z.LastModificationDate = '2011-04-27 18:00:00.000'		
 from testNFAMPProcess a
  join di_drugitemprice z
 on a.drugitempriceid = z.drugitempriceid
where a.ExpectedStopDate is not null
and z.PriceStopDate <> a.ExpectedStopDate


update DI_Contracts
	set LastModificationDate = '2011-04-27 18:00:00.000'
where ContractNumber like '797-FSS%'


