IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetFSSDrugItemPriceDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetFSSDrugItemPriceDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure GetFSSDrugItemPriceDetails
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@DrugItemId int,
@DrugItemPriceId int,
@IsFromHistory bit,
@IsHistoryFromArchive bit,
@DrugItemPriceHistoryId int = -1 -- this is DrugItemPriceHistoryId from the archive when IsHistoryFromArchive = 1
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN
	
	if @IsFromHistory = 0
	BEGIN
		select 
			p.DrugItemPriceId ,
			p.DrugItemId,        
			p.HistoricalNValue,     
			p.PriceId, 
			n.FdaAssignedLabelerCode,    
			n.ProductCode,       
			n.PackageCode,          
			i.PackageDescription  ,        
			i.Generic ,   
			i.TradeName ,     
			i.DiscontinuationDate,                             	             
	--		i.NDCLinkId ,        	                  
			i.Covered,     
			case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
			dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( p.PriceStartDate )) as FCP,                
			p.PriceStartDate,         
			p.PriceStopDate as PriceEndDate,       
			p.Price,      
			p.IsTemporary,                                     	                  
			p.IsFSS,                                           	                  
			p.IsBIG4,                                          	                  
			p.AwardedFSSTrackingCustomerRatio as TrackingCustomerRatio,     
			p.TrackingCustomerName,   
			p.CurrentTrackingCustomerPrice as TrackingCustomerPrice,         
			p.ExcludeFromExport,
			p.LastModificationType,
			p.ModificationStatusId,                  
			p.CreatedBy,     
			p.CreationDate,        
			p.LastModifiedBy,        
			p.LastModificationDate


		from DI_DrugItems i, DI_DrugItemPrice p, DI_DrugItemNDC n
		where  p.DrugItemId = @DrugItemId
		and p.DrugItemPriceId = @DrugItemPriceId
		and p.DrugItemId = i.DrugItemId 
		and i.DrugItemNDCId = n.DrugItemNDCId
		
		select @error = @@error, @rowcount = @@rowcount
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error retrieving drug item price details for fss contract ' + @ContractNumber
			raiserror( @errorMsg, 16, 1 )
		END
	END
	else
	BEGIN
		if @IsHistoryFromArchive = 1
		BEGIN
			select 
				a.DrugItemPriceId ,
				a.DrugItemId,             
				a.HistoricalNValue,     
				a.PriceId, 
				n.FdaAssignedLabelerCode,    
				n.ProductCode,       
				n.PackageCode,          
				i.PackageDescription  ,        
				i.Generic ,   
				i.TradeName ,     
				i.DiscontinuationDate,                             	             
		--		i.NDCLinkId ,        	                  
				i.Covered,     
				case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
				dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( a.PriceStartDate )) as FCP,                
				a.PriceStartDate,         
				a.PriceStopDate as PriceEndDate,       
				a.Price,      
				a.IsTemporary,                                     	                  
				a.IsFSS,                                           	                  
				a.IsBIG4,                                          	                  
				a.AwardedFSSTrackingCustomerRatio as TrackingCustomerRatio,     
				a.TrackingCustomerName,   
				a.CurrentTrackingCustomerPrice as TrackingCustomerPrice,         
				a.ExcludeFromExport,
				a.LastModificationType,
				a.ModificationStatusId,                  
				a.CreatedBy,     
				a.CreationDate,        
				a.LastModifiedBy,        
				a.LastModificationDate


			from DI_DrugItems i, DI_DrugItemPriceHistoryArchive a, DI_DrugItemNDC n
			where  a.DrugItemId = @DrugItemId
			and a.DrugItemPriceId = @DrugItemPriceId
			and a.DrugItemPriceHistoryId = @DrugItemPriceHistoryId
			and a.DrugItemId = i.DrugItemId 
			and i.DrugItemNDCId = n.DrugItemNDCId
	-- Andem 05/27/2010, remove price items where priceid is -1 -- this comment was copied along with regular history code section
	-- Zeglin 6/12/2015, changed to use max price id in case -1 is the only price id in which case the query bombs
			and a.PriceId = ( select MAX(PriceId) from DI_DrugItemPriceHistoryArchive where DrugItemPriceHistoryId = @DrugItemPriceHistoryId )
				
			select @error = @@error, @rowcount = @@rowcount
		
			if @error <> 0 or @rowcount <> 1
			BEGIN
				select @errorMsg = 'Error retrieving drug item ( archived ) price details for fss contract ' + @ContractNumber
				raiserror( @errorMsg, 16, 1 )
			END
		END
		else -- regular history
		BEGIN
			select 
				p.DrugItemPriceId ,
				p.DrugItemId,             
				p.HistoricalNValue,     
				p.PriceId, 
				n.FdaAssignedLabelerCode,    
				n.ProductCode,       
				n.PackageCode,          
				i.PackageDescription  ,        
				i.Generic ,   
				i.TradeName ,     
				i.DiscontinuationDate,                             	             
		--		i.NDCLinkId ,        	                  
				i.Covered,     
				case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
				dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( p.PriceStartDate )) as FCP,                
				p.PriceStartDate,         
				p.PriceStopDate as PriceEndDate,       
				p.Price,      
				p.IsTemporary,                                     	                  
				p.IsFSS,                                           	                  
				p.IsBIG4,                                          	                  
				p.AwardedFSSTrackingCustomerRatio as TrackingCustomerRatio,     
				p.TrackingCustomerName,   
				p.CurrentTrackingCustomerPrice as TrackingCustomerPrice,         
				p.ExcludeFromExport,
				p.LastModificationType,
				p.ModificationStatusId,                  
				p.CreatedBy,     
				p.CreationDate,        
				p.LastModifiedBy,        
				p.LastModificationDate


			from DI_DrugItems i, DI_DrugItemPriceHistory p, DI_DrugItemNDC n
			where  p.DrugItemId = @DrugItemId
			and p.DrugItemPriceId = @DrugItemPriceId
			and p.DrugItemPriceHistoryId = @DrugItemPriceHistoryId
			and p.DrugItemId = i.DrugItemId 
			and i.DrugItemNDCId = n.DrugItemNDCId
	-- Andem 05/27/2010, remove price items where priceid is -1
	-- Zeglin 6/12/2015, changed to use max price id in case -1 is the only price id in which case the query bombs
			and p.PriceId = ( select MAX(PriceId) from DI_DrugItemPriceHistory where DrugItemPriceHistoryId = @DrugItemPriceHistoryId )
				
				
			select @error = @@error, @rowcount = @@rowcount
		
			if @error <> 0 or @rowcount <> 1
			BEGIN
				select @errorMsg = 'Error retrieving drug item ( historical ) price details for fss contract ' + @ContractNumber
				raiserror( @errorMsg, 16, 1 )
			END
		END
	END		
END
