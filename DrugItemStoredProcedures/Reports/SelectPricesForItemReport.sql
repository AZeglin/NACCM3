IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectPricesForItemReport')
	BEGIN
		DROP  Procedure  SelectPricesForItemReport
	END

GO

CREATE Procedure SelectPricesForItemReport
(
@ContractNumber nvarchar(20),
@DrugItemId int,
@FutureHistoricalSelectionCriteria nchar(1)  -- H historical, F future, A active, B both future and active
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	-- historical only case
	if @FutureHistoricalSelectionCriteria = 'H'
	BEGIN
			select 
			h.DrugItemPriceHistoryId,
			h.DrugItemPriceId ,
			h.PriceId,              
			h.PriceStartDate,         
			h.PriceStopDate,       
			h.Price,      
			h.IsTemporary,                                     	                  
			h.IsFSS,                                           	                  
			h.IsBIG4,                                          	                  
			dbo.GetPriceApplicabilityStringForReportFunction( h.DrugItemPriceId, @FutureHistoricalSelectionCriteria ) as PriceApplicabilityString,
			dbo.GetVAIFF( @ContractNumber, h.PriceStartDate ) as VAIFF,
			h.AwardedFSSTrackingCustomerRatio,        
			h.TrackingCustomerName,        
			h.CurrentTrackingCustomerPrice,         
			h.ExcludeFromExport,
			h.LastModificationType,				
			h.ModificationStatusId,                  
			h.CreatedBy,     
			h.CreationDate,        
			h.LastModifiedBy,        
			h.LastModificationDate
		from DI_DrugItemPriceHistory h
		where h.DrugItemId = @DrugItemId 
		
		union

		select 
			h.DrugItemPriceHistoryId,
			h.DrugItemPriceId ,
			h.PriceId,              
			h.PriceStartDate,         
			h.PriceStopDate,       
			h.Price,      
			h.IsTemporary,                                     	                  
			h.IsFSS,                                           	                  
			h.IsBIG4,                                          	                  
			dbo.GetPriceApplicabilityStringForReportFunction( h.DrugItemPriceId, @FutureHistoricalSelectionCriteria ) as PriceApplicabilityString,
			dbo.GetVAIFF( @ContractNumber, h.PriceStartDate ) as VAIFF,
			h.AwardedFSSTrackingCustomerRatio,        
			h.TrackingCustomerName,        
			h.CurrentTrackingCustomerPrice,         
			h.ExcludeFromExport,
			h.LastModificationType,				
			h.ModificationStatusId,                  
			h.CreatedBy,     
			h.CreationDate,        
			h.LastModifiedBy,        
			h.LastModificationDate
		from DI_DrugItemPriceHistoryArchive h
		where h.DrugItemId = @DrugItemId 

		order by h.PriceStartDate

		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving drug item prices (H) for DrugItemId ' + convert( nvarchar(20), @DrugItemId )
			raiserror( @errorMsg, 16, 1 )
		END
	END
	else
	BEGIN -- non historical

		if @FutureHistoricalSelectionCriteria = 'A'
		BEGIN
	
			select -1 as DrugItemPriceHistoryId,
			    p.DrugItemPriceId,
				p.DrugItemSubItemId,
				p.HistoricalNValue,
				p.PriceId,
				p.PriceStartDate,
				p.PriceStopDate,
				p.Price,
				p.IsTemporary,
				p.IsFSS,
				p.IsBIG4,
				dbo.GetPriceApplicabilityStringForReportFunction( p.DrugItemPriceId, @FutureHistoricalSelectionCriteria ) as PriceApplicabilityString,
				dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,
				p.AwardedFSSTrackingCustomerRatio,        
				p.TrackingCustomerName,        
				p.CurrentTrackingCustomerPrice,         
				p.ExcludeFromExport,
				p.LastModificationType,
				p.ModificationStatusId,                  
				p.CreatedBy,     
				p.CreationDate,        
				p.LastModifiedBy,        
				p.LastModificationDate
			from DI_DrugItemPrice p 
			where p.DrugItemId = @DrugItemId
			and	datediff( d, PriceStopDate, GETDATE() ) <= 0 
			and datediff( d, PriceStartDate, GETDATE() ) >= 0 
			order by p.PriceStartDate
				
			select @error = @@error

			if @error <> 0
			BEGIN
				select @errorMsg = 'Error retrieving drug item prices (A) for DrugItemId ' + convert( nvarchar(20), @DrugItemId )
				raiserror( @errorMsg, 16, 1 )
			END
		END
		else
		BEGIN -- either future or both
			if @FutureHistoricalSelectionCriteria = 'F'
			BEGIN
			
				select -1 as DrugItemPriceHistoryId,
				    p.DrugItemPriceId,
					p.DrugItemSubItemId,
					p.HistoricalNValue,
					p.PriceId,
					p.PriceStartDate,
					p.PriceStopDate,
					p.Price,
					p.IsTemporary,
					p.IsFSS,
					p.IsBIG4,
					dbo.GetPriceApplicabilityStringForReportFunction( p.DrugItemPriceId, @FutureHistoricalSelectionCriteria ) as PriceApplicabilityString,
					dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,
					p.AwardedFSSTrackingCustomerRatio,        
					p.TrackingCustomerName,        
					p.CurrentTrackingCustomerPrice,         
					p.ExcludeFromExport,
					p.LastModificationType,
					p.ModificationStatusId,                  
					p.CreatedBy,     
					p.CreationDate,        
					p.LastModifiedBy,        
					p.LastModificationDate
				from DI_DrugItemPrice p 
				where p.DrugItemId = @DrugItemId
				and	datediff( d, GETDATE(), PriceStartDate ) > 0 
				order by p.PriceStartDate

				select @error = @@error

				if @error <> 0
				BEGIN
					select @errorMsg = 'Error retrieving drug item prices (F) for DrugItemId ' + convert( nvarchar(20), @DrugItemId )
					raiserror( @errorMsg, 16, 1 )
				END
			
			END
			else
			BEGIN -- both future and active
				if @FutureHistoricalSelectionCriteria = 'B'
				BEGIN
				
					select -1 as DrugItemPriceHistoryId,
					    p.DrugItemPriceId,
						p.DrugItemSubItemId,
						p.HistoricalNValue,
						p.PriceId,
						p.PriceStartDate,
						p.PriceStopDate,
						p.Price,
						p.IsTemporary,
						p.IsFSS,
						p.IsBIG4,
						dbo.GetPriceApplicabilityStringForReportFunction( p.DrugItemPriceId, @FutureHistoricalSelectionCriteria ) as PriceApplicabilityString,
						dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,
						p.AwardedFSSTrackingCustomerRatio,        
						p.TrackingCustomerName,        
						p.CurrentTrackingCustomerPrice,         
						p.ExcludeFromExport,
						p.LastModificationType,
						p.ModificationStatusId,                  
						p.CreatedBy,     
						p.CreationDate,        
						p.LastModifiedBy,        
						p.LastModificationDate
					from DI_DrugItemPrice p 
					where p.DrugItemId = @DrugItemId
					and	(( datediff( d, PriceStopDate, GETDATE() ) <= 0 
						and datediff( d, PriceStartDate, GETDATE() ) >= 0 )
						or datediff( d, GETDATE(), PriceStartDate ) > 0 )
					order by p.PriceStartDate

					select @error = @@error

					if @error <> 0
					BEGIN
						select @errorMsg = 'Error retrieving drug item prices (B) for DrugItemId ' + convert( nvarchar(20), @DrugItemId )
						raiserror( @errorMsg, 16, 1 )
					END
				
				END
			END
		END
	END
END