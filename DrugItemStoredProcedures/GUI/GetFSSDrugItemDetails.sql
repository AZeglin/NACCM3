IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetFSSDrugItemDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetFSSDrugItemDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure GetFSSDrugItemDetails
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@DrugItemId int
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@priceStartDate datetime,
	@priceStartYear int
	
BEGIN
	
			/* get the price start date */
			select @priceStartDate = PriceStartDate
			from DI_DrugItemPrice
			where DrugItemId = @DrugItemId
			and IsFSS = 1
			and IsTemporary = 0
			and DrugItemSubItemId is null
			and getdate() between PriceStartDate and PriceStopDate

			select @error = @@error, @rowcount = @@rowcount
			
			if @error <> 0 or @rowcount <> 1
			BEGIN
				select @priceStartYear = YEAR( getdate() )
			END
			else
			BEGIN
				select @priceStartYear = YEAR( @priceStartDate )
			END
	
			select i.DrugItemId, 
			i.DrugItemNDCId,
			n.FdaAssignedLabelerCode,    
			n.ProductCode,       
			n.PackageCode,          
			i.PackageDescription  ,        
			i.Generic ,   
			i.TradeName ,     
			i.DiscontinuationDate,                             	             
			i.DiscontinuationEnteredDate,
			i.DiscontinuationReasonId,
			i.Covered,         
			i.PrimeVendor,
			i.PrimeVendorChangedDate , 
			dbo.GetFCPValueForDrugItem( i.DrugItemId, @priceStartYear ) as FCP,     
		--	p.Price as CurrentFSSPrice,
		--	p.PriceStartDate as PriceStartDate,
		--	p.PriceStopDate as PriceEndDate,	
			DateEnteredMarket,      	             
			i.PassThrough,            
			i.DispensingUnit ,        
			i.VAClass , 
			case dbo.GetItemDualPriceStatusForDrugItemId( @DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
			i.ExcludeFromExport,
			i.NonTAA,
			i.IncludedFETAmount,
			i.LastModificationType,       
			i.ModificationStatusId  ,                 
			i.CreatedBy ,     
			i.CreationDate ,         
			i.LastModifiedBy ,    
			i.LastModificationDate,
			
			isnull( k.DrugItemPackageId, -1 ) as DrugItemPackageId,
			k.UnitOfSale,
			k.QuantityInUnitOfSale,
			k.UnitPackage,
			k.QuantityInUnitPackage,
			k.UnitOfMeasure,
			k.PriceMultiplier,
			k.PriceDivider
		
	--	from DI_DrugItems i left outer join DI_DrugItemPrice p  
	--			on i.DrugItemId = p.DrugItemId
	--				and p.IsFSS = 1
	--				and p.IsTemporary = 0
	--				and getdate() between p.PriceStartDate and p.PriceStopDate
		from DI_DrugItems i left outer join DI_DrugItemPackage k
				on i.DrugItemId = k.DrugItemid
			join DI_DrugItemNDC n
				on i.DrugItemNDCId = n.DrugItemNDCId
		where i.DrugItemId = @DrugItemId
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving drug item details for fss contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
		
ERROREXIT:		
		
END