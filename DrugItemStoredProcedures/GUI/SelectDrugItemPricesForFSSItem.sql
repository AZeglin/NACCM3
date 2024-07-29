IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SelectDrugItemPricesForFSSItem]') AND type in (N'P', N'PC'))
DROP PROCEDURE [SelectDrugItemPricesForFSSItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure SelectDrugItemPricesForFSSItem
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@DrugItemId int,
@FutureHistoricalSelectionCriteria nchar(1),  -- H historical, F future, A active, B both future and active
@WithAddPrice bit = 0
)

AS

/* note, this SP presumes that the item is still active. */

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	
	@DrugItemPriceId int,
	@PriceId int,              
	@PriceStartDate datetime,         
	@PriceEndDate datetime,       
	@Price decimal(9,2),      
	@IsTemporary bit,
	@IsFSS bit,                                           	                  
	@IsBIG4 bit,                                          	                  
	@IsVA bit,                                            	                  
	@IsBOP bit,                                           	                  
	@IsCMOP bit,                                          	                  
	@IsDOD bit,                                           	                  
	@IsHHS bit,                                           	                  
	@IsIHS bit,                                           	                  
	@IsIHS2 bit,                                          	                  
	@IsDIHS bit,                                          	                  
	@IsNIH bit,                                           	                  
	@IsPHS bit,                                           	                  
	@IsSVH bit,                                           	                  
	@IsSVH1 bit,                                          	                  
	@IsSVH2 bit,                                          	                  
	@IsTMOP bit,                                          	                  
	@IsUSCG bit, 
	@IsFHCC bit,
	@FCP int,
	@Covered nchar(1),          
	@DualPriceDesignation nchar(1),
	@VAIFF decimal( 8, 4 ),            			                   	                  
	@AwardedFSSTrackingCustomerRatio decimal(10,2),        
	@CurrentTrackingCustomerPrice decimal(10,2),      
	@TrackingCustomerName nvarchar(120),
	@ExcludeFromExport bit,
	@LastModificationType nchar(1),   
	@PrimeVendorChangedDate datetime,         
	@ModificationStatusId int,
	@CreatedBy nvarchar(120),
	@CreationDate datetime,
	@LastModifiedBy nvarchar(120),
	@LastModificationDate datetime,
	@IsNewBlankRow bit,
	@IsFromHistory bit,
	@IsHistoryFromArchive bit,
	@TPRAlwaysHasBasePrice bit,
	@Notes nvarchar(2), -- not used for history
	@DrugItemPriceHistoryId int,  -- not used for history
	@DrugItemSubItemId int,
	@SubItemIdentifier nchar(1),
	@dateWithoutTime datetime

BEGIN

	select @dateWithoutTime = convert( datetime, convert( nvarchar(2), DatePart( month, getdate() )) + '/' + convert( nvarchar(2), DatePart( day, getdate() )) + '/' + convert( nvarchar(4), DatePart( year, getdate() )))
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error getting date without time ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

	-- historical only case
	if @FutureHistoricalSelectionCriteria = 'H'
	BEGIN
	
		select 
			h.DrugItemPriceHistoryId,
			h.DrugItemPriceId ,
			h.DrugItemId,             
			h.PriceId,              
			h.PriceStartDate,         
			h.PriceStopDate as PriceEndDate,       
			h.Price,      
			h.IsTemporary,                                     	                  
			h.IsFSS,                                           	                  
			h.IsBIG4,                                          	                  
			h.IsVA,                                            	                  
			h.IsBOP,                                           	                  
			h.IsCMOP,                                          	                  
			h.IsDOD,                                           	                  
			h.IsHHS,                                           	                  
			h.IsIHS,                                           	                  
			h.IsIHS2,                                          	                  
			h.IsDIHS,                                          	                  
			h.IsNIH,                                           	                  
			h.IsPHS,                                           	                  
			h.IsSVH,                                           	                  
			h.IsSVH1,                                          	                  
			h.IsSVH2,                                          	                  
			h.IsTMOP,                                          	                  
			h.IsUSCG, 
			h.IsFHCC,
			i.Covered,    
			case dbo.GetItemDualPriceStatusForDrugItemId( @DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,     
			dbo.GetVAIFF( @ContractNumber, h.PriceStartDate ) as VAIFF,
			dbo.GetFCPValueForDrugItem( @DrugItemId, YEAR(h.PriceStartDate) ) as FCP,                        	                  
			h.AwardedFSSTrackingCustomerRatio,        
			h.TrackingCustomerName,        
			h.CurrentTrackingCustomerPrice,         
			h.ExcludeFromExport,
			h.LastModificationType,				
			h.ModificationStatusId,                  
			h.CreatedBy,     
			h.CreationDate,        
			h.LastModifiedBy,        
			h.LastModificationDate,
			0 as IsNewBlankRow, 
			1 as IsFromHistory,
			0 as IsHistoryFromArchive,
			0 as TPRAlwaysHasBasePrice,
			h.Notes,
			DrugItemSubItemId,
			dbo.GetSubItemIdentifierForHistoricalPriceFunction( DrugItemSubItemId ) as SubItemIdentifier
		from DI_DrugItemPriceHistory h, DI_DrugItems i
		where i.DrugItemId = @DrugItemId 
		and h.DrugItemId = i.DrugItemId
	
		union

		select 
			a.DrugItemPriceHistoryId,
			a.DrugItemPriceId ,
			a.DrugItemId,             
			a.PriceId,              
			a.PriceStartDate,         
			a.PriceStopDate as PriceEndDate,       
			a.Price,      
			a.IsTemporary,                                     	                  
			a.IsFSS,                                           	                  
			a.IsBIG4,                                          	                  
			a.IsVA,                                            	                  
			a.IsBOP,                                           	                  
			a.IsCMOP,                                          	                  
			a.IsDOD,                                           	                  
			a.IsHHS,                                           	                  
			a.IsIHS,                                           	                  
			a.IsIHS2,                                          	                  
			a.IsDIHS,                                          	                  
			a.IsNIH,                                           	                  
			a.IsPHS,                                           	                  
			a.IsSVH,                                           	                  
			a.IsSVH1,                                          	                  
			a.IsSVH2,                                          	                  
			a.IsTMOP,                                          	                  
			a.IsUSCG, 
			a.IsFHCC,
			i.Covered,    
			case dbo.GetItemDualPriceStatusForDrugItemId( @DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,     
			dbo.GetVAIFF( @ContractNumber, a.PriceStartDate ) as VAIFF,
			dbo.GetFCPValueForDrugItem( @DrugItemId, YEAR(a.PriceStartDate) ) as FCP,                        	                  
			a.AwardedFSSTrackingCustomerRatio,        
			a.TrackingCustomerName,        
			a.CurrentTrackingCustomerPrice,         
			a.ExcludeFromExport,
			a.LastModificationType,				
			a.ModificationStatusId,                  
			a.CreatedBy,     
			a.CreationDate,        
			a.LastModifiedBy,        
			a.LastModificationDate,
			0 as IsNewBlankRow, 
			1 as IsFromHistory,
			1 as IsHistoryFromArchive,
			0 as TPRAlwaysHasBasePrice,
			'' as Notes,
			DrugItemSubItemId,
			dbo.GetSubItemIdentifierForHistoricalPriceFunction( DrugItemSubItemId ) as SubItemIdentifier
		from DI_DrugItemPriceHistoryArchive a, DI_DrugItems i
		where i.DrugItemId = @DrugItemId 
		and a.DrugItemId = i.DrugItemId

		order by PriceStartDate

		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving drug item prices for fss contract (H) ' + @ContractNumber
			raiserror( @errorMsg, 16, 1 )
		END

	END
	else
	BEGIN -- non historical

		if @FutureHistoricalSelectionCriteria = 'A'
		BEGIN

			if @WithAddPrice = 0
			BEGIN
					select 
						-1 as DrugItemPriceHistoryId,
						p.DrugItemPriceId ,
						p.DrugItemId,             
						p.PriceId,              
						p.PriceStartDate,         
						p.PriceStopDate as PriceEndDate,       
						p.Price,      
						p.IsTemporary,                                     	                  
						p.IsFSS,                                           	                  
						p.IsBIG4,                                          	                  
						p.IsVA,                                            	                  
						p.IsBOP,                                           	                  
						p.IsCMOP,                                          	                  
						p.IsDOD,                                           	                  
						p.IsHHS,                                           	                  
						p.IsIHS,                                           	                  
						p.IsIHS2,                                          	                  
						p.IsDIHS,                                          	                  
						p.IsNIH,                                           	                  
						p.IsPHS,                                           	                  
						p.IsSVH,                                           	                  
						p.IsSVH1,                                          	                  
						p.IsSVH2,                                          	                  
						p.IsTMOP,                                          	                  
						p.IsUSCG,
						p.IsFHCC,
						i.Covered,
						case dbo.GetItemDualPriceStatusForDrugItemId( @DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,     
						dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,
						dbo.GetFCPValueForDrugItem( @DrugItemId, YEAR(p.PriceStartDate) ) as FCP,
						p.AwardedFSSTrackingCustomerRatio,        
						p.TrackingCustomerName,        
						p.CurrentTrackingCustomerPrice,         
						p.ExcludeFromExport,
						p.LastModificationType,
						p.ModificationStatusId,                  
						p.CreatedBy,     
						p.CreationDate,        
						p.LastModifiedBy,        
						p.LastModificationDate,
						0 as IsNewBlankRow,
						0 as IsFromHistory,
						0 as IsHistoryFromArchive,
						dbo.GetTPRAlwaysHasBasePriceFunction( p.DrugItemId,          
									p.PriceStartDate,         
									p.PriceStopDate,       
									p.IsTemporary,                                     	                  
									p.IsFSS,                                           	                  
									p.IsBIG4,                                          	                  
									p.IsVA,                                            	                  
									p.IsBOP,                                           	                  
									p.IsCMOP,                                          	                  
									p.IsDOD,                                           	                  
									p.IsHHS,                                           	                  
									p.IsIHS,                                           	                  
									p.IsIHS2,                                          	                  
									p.IsDIHS,                                          	                  
									p.IsNIH,                                           	                  
									p.IsPHS,                                           	                  
									p.IsSVH,                                           	                  
									p.IsSVH1,                                          	                  
									p.IsSVH2,                                          	                  
									p.IsTMOP,                                          	                  
									p.IsUSCG,
									p.IsFHCC ) as TPRAlwaysHasBasePrice,  
						'' as Notes,
						p.DrugItemSubItemId,
						s.SubItemIdentifier
					from DI_DrugItemPrice p join DI_DrugItems i
						on p.DrugItemId = i.DrugItemId
					left outer join DI_DrugItemSubItems s
						on i.DrugItemId = s.DrugItemId
						and p.DrugItemSubItemId = s.DrugItemSubItemId
					where i.DrugItemId = @DrugItemId 
					and @dateWithoutTime between p.PriceStartDate and p.PriceStopDate
					
					
					order by p.PriceStartDate
					
					select @error = @@error
					
					if @error <> 0
					BEGIN
						select @errorMsg = 'Error retrieving drug item prices for fss contract (A1) ' + @ContractNumber
						raiserror( @errorMsg, 16, 1 )
					END
				END
			ELSE
				BEGIN
					/* blank row definition */
					select @DrugItemPriceHistoryId = -1,
					@DrugItemPriceId = 0,
					@PriceId = 0,              
					@PriceStartDate = null,         
					@PriceEndDate = null,      
					@Price = null,      
					@IsTemporary = 0,                                     	                  
					@IsFSS = 0,                                           	                  
					@IsBIG4 = 0,                                          	                  
					@IsVA = 0,                                            	                  
					@IsBOP = 0,                                           	                  
					@IsCMOP = 0,                                          	                  
					@IsDOD = 0,                                           	                  
					@IsHHS = 0,                                           	                  
					@IsIHS = 0,                                           	                  
					@IsIHS2 = 0,                                          	                  
					@IsDIHS = 0,                                          	                  
					@IsNIH = 0,                                           	                  
					@IsPHS = 0,                                           	                  
					@IsSVH = 0,                                           	                  
					@IsSVH1 = 0,                                          	                  
					@IsSVH2 = 0,                                          	                  
					@IsTMOP = 0,                                          	                  
					@IsUSCG = 0,  
					@IsFHCC = 0, 
					@FCP = 0,
					@Covered = 'F',     
					@DualPriceDesignation = 'F',   
					@VAIFF = 0,                               	                  
					@AwardedFSSTrackingCustomerRatio = null,        
					@TrackingCustomerName = '',
					@CurrentTrackingCustomerPrice = null,       
					@ExcludeFromExport = 0,
					@LastModificationType = '',			     
					@ModificationStatusId = 0,                  
					@CreatedBy = 'Fred',     
					@CreationDate = getdate(),        
					@LastModifiedBy = 'Fred',        
					@LastModificationDate = getdate(),
					@IsNewBlankRow = 1,
					@IsFromHistory = 0,
					@IsHistoryFromArchive = 0,
					@TPRAlwaysHasBasePrice = 0,
					@Notes = '',
					@DrugItemSubItemId = -1,
					@SubItemIdentifier = ''
		
		
				select 
					-1 as DrugItemPriceHistoryId,
					p.DrugItemPriceId ,
					p.DrugItemId,             
					p.PriceId,              
					p.PriceStartDate,         
					p.PriceStopDate as PriceEndDate,       
					p.Price,      
					p.IsTemporary,                                     	                  
					p.IsFSS,                                           	                  
					p.IsBIG4,                                          	                  
					p.IsVA,                                            	                  
					p.IsBOP,                                           	                  
					p.IsCMOP,                                          	                  
					p.IsDOD,                                           	                  
					p.IsHHS,                                           	                  
					p.IsIHS,                                           	                  
					p.IsIHS2,                                          	                  
					p.IsDIHS,                                          	                  
					p.IsNIH,                                           	                  
					p.IsPHS,                                           	                  
					p.IsSVH,                                           	                  
					p.IsSVH1,                                          	                  
					p.IsSVH2,                                          	                  
					p.IsTMOP,                                          	                  
					p.IsUSCG, 
					p.IsFHCC,
					i.Covered,       
					case dbo.GetItemDualPriceStatusForDrugItemId( @DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,     
					dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,      
					dbo.GetFCPValueForDrugItem( @DrugItemId, YEAR(p.PriceStartDate) ) as FCP,                        	                  
					p.AwardedFSSTrackingCustomerRatio,        
					p.TrackingCustomerName,        
					p.CurrentTrackingCustomerPrice,         
					p.ExcludeFromExport,
					p.LastModificationType,				
					p.ModificationStatusId,                  
					p.CreatedBy,     
					p.CreationDate,        
					p.LastModifiedBy,        
					p.LastModificationDate,
					0 as IsNewBlankRow,
					0 as IsFromHistory,
					0 as IsHistoryFromArchive,
						dbo.GetTPRAlwaysHasBasePriceFunction( p.DrugItemId,          
									p.PriceStartDate,         
									p.PriceStopDate,       
									p.IsTemporary,                                     	                  
									p.IsFSS,                                           	                  
									p.IsBIG4,                                          	                  
									p.IsVA,                                            	                  
									p.IsBOP,                                           	                  
									p.IsCMOP,                                          	                  
									p.IsDOD,                                           	                  
									p.IsHHS,                                           	                  
									p.IsIHS,                                           	                  
									p.IsIHS2,                                          	                  
									p.IsDIHS,                                          	                  
									p.IsNIH,                                           	                  
									p.IsPHS,                                           	                  
									p.IsSVH,                                           	                  
									p.IsSVH1,                                          	                  
									p.IsSVH2,                                          	                  
									p.IsTMOP,                                          	                  
									p.IsUSCG,
									p.IsFHCC ) as TPRAlwaysHasBasePrice,  
					'' as Notes,
					p.DrugItemSubItemId,
					s.SubItemIdentifier
				from DI_DrugItemPrice p join DI_DrugItems i
					on p.DrugItemId = i.DrugItemId
				left outer join DI_DrugItemSubItems s
					on i.DrugItemId = s.DrugItemId
					and p.DrugItemSubItemId = s.DrugItemSubItemId
					where i.DrugItemId = @DrugItemId 
					and @dateWithoutTime between p.PriceStartDate and p.PriceStopDate
								
					
				UNION
				
				/* return a new blank row at the applications request */
				select 
					@DrugItemPriceHistoryId as DrugItemPriceHistoryId,
					@DrugItemPriceId as DrugItemPriceId ,
					@DrugItemId as DrugItemId,             
					@PriceId as PriceId,              
					@PriceStartDate as PriceStartDate,         
					@PriceEndDate as PriceEndDate,       
					@Price as Price,      
					@IsTemporary as IsTemporary,                                     	                  
					@IsFSS  as IsFSS,                                           	                  
					@IsBIG4  as IsBIG4,                                          	                  
					@IsVA  as IsVA,                                            	                  
					@IsBOP  as IsBOP,                                           	                  
					@IsCMOP  as IsCMOP,                                          	                  
					@IsDOD  as IsDOD,                                           	                  
					@IsHHS  as IsHHS,                                           	                  
					@IsIHS  as IsIHS,                                           	                  
					@IsIHS2  as IsIHS2,                                          	                  
					@IsDIHS  as IsDIHS,                                          	                  
					@IsNIH  as IsNIH,                                           	                  
					@IsPHS  as IsPHS,                                           	                  
					@IsSVH  as IsSVH,                                           	                  
					@IsSVH1  as IsSVH1,                                          	                  
					@IsSVH2  as IsSVH2,                                          	                  
					@IsTMOP  as IsTMOP,                                          	                  
					@IsUSCG  as IsUSCG, 
					@IsFHCC as IsFHCC,          
					@Covered as Covered,
					@DualPriceDesignation as DualPriceDesignation,
					@VAIFF as VAIFF,
					@FCP as FCP,                               	                  
					@AwardedFSSTrackingCustomerRatio  as  AwardedFSSTrackingCustomerRatio, 		
					@TrackingCustomerName  as TrackingCustomerName,
					@CurrentTrackingCustomerPrice  as  CurrentTrackingCustomerPrice,         
					@ExcludeFromExport as ExcludeFromExport,
					@LastModificationType as LastModificationType,			     
					@ModificationStatusId  as  ModificationStatusId,                  
					@CreatedBy  as  CreatedBy,     
					@CreationDate  as  CreationDate,        
					@LastModifiedBy  as  LastModifiedBy,        
					@LastModificationDate  as  LastModificationDate,
					@IsNewBlankRow as IsNewBlankRow,
					@IsFromHistory as IsFromHistory,
					@IsHistoryFromArchive as IsHistoryFromArchive,
					@TPRAlwaysHasBasePrice as TPRAlwaysHasBasePrice,
					@Notes as Notes,
					@DrugItemSubItemId as DrugItemSubItemId,
					@SubItemIdentifier as SubItemIdentifier
					
				select @error = @@error
				
				if @error <> 0
				BEGIN
					select @errorMsg = 'Error retrieving drug item prices for fss contract (A2) ' + @ContractNumber
					raiserror( @errorMsg, 16, 1 )
				END
			END
		END -- active only
		else
		BEGIN -- either future or both
			if @FutureHistoricalSelectionCriteria = 'F'
			BEGIN

				if @WithAddPrice = 0
				BEGIN
						select 
							-1 as DrugItemPriceHistoryId,
							p.DrugItemPriceId ,
							p.DrugItemId,             
							p.PriceId,              
							p.PriceStartDate,         
							p.PriceStopDate as PriceEndDate,       
							p.Price,      
							p.IsTemporary,                                     	                  
							p.IsFSS,                                           	                  
							p.IsBIG4,                                          	                  
							p.IsVA,                                            	                  
							p.IsBOP,                                           	                  
							p.IsCMOP,                                          	                  
							p.IsDOD,                                           	                  
							p.IsHHS,                                           	                  
							p.IsIHS,                                           	                  
							p.IsIHS2,                                          	                  
							p.IsDIHS,                                          	                  
							p.IsNIH,                                           	                  
							p.IsPHS,                                           	                  
							p.IsSVH,                                           	                  
							p.IsSVH1,                                          	                  
							p.IsSVH2,                                          	                  
							p.IsTMOP,                                          	                  
							p.IsUSCG,
							p.IsFHCC,
							i.Covered,
							case dbo.GetItemDualPriceStatusForDrugItemId( @DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,     
							dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,      
							dbo.GetFCPValueForDrugItem( @DrugItemId, YEAR(p.PriceStartDate) ) as FCP,
							p.AwardedFSSTrackingCustomerRatio,        
							p.TrackingCustomerName,        
							p.CurrentTrackingCustomerPrice,         
							p.ExcludeFromExport,
							p.LastModificationType,
							p.ModificationStatusId,                  
							p.CreatedBy,     
							p.CreationDate,        
							p.LastModifiedBy,        
							p.LastModificationDate,
							0 as IsNewBlankRow,
							0 as IsFromHistory,
							0 as IsHistoryFromArchive,
							dbo.GetTPRAlwaysHasBasePriceFunction( p.DrugItemId,          
									p.PriceStartDate,         
									p.PriceStopDate,       
									p.IsTemporary,                                     	                  
									p.IsFSS,                                           	                  
									p.IsBIG4,                                          	                  
									p.IsVA,                                            	                  
									p.IsBOP,                                           	                  
									p.IsCMOP,                                          	                  
									p.IsDOD,                                           	                  
									p.IsHHS,                                           	                  
									p.IsIHS,                                           	                  
									p.IsIHS2,                                          	                  
									p.IsDIHS,                                          	                  
									p.IsNIH,                                           	                  
									p.IsPHS,                                           	                  
									p.IsSVH,                                           	                  
									p.IsSVH1,                                          	                  
									p.IsSVH2,                                          	                  
									p.IsTMOP,                                          	                  
									p.IsUSCG,
									p.IsFHCC ) as TPRAlwaysHasBasePrice,  
							'' as Notes,
							p.DrugItemSubItemId,
							s.SubItemIdentifier
						from DI_DrugItemPrice p join DI_DrugItems i
							on p.DrugItemId = i.DrugItemId
						left outer join DI_DrugItemSubItems s
							on i.DrugItemId = s.DrugItemId
							and p.DrugItemSubItemId = s.DrugItemSubItemId

							where i.DrugItemId = @DrugItemId 
							and p.PriceStartDate > @dateWithoutTime
										

							order by p.PriceStartDate

						select @error = @@error
						
						if @error <> 0
						BEGIN
							select @errorMsg = 'Error retrieving drug item prices for fss contract (F1) ' + @ContractNumber
							raiserror( @errorMsg, 16, 1 )
						END
					END
				ELSE
					BEGIN
						/* blank row definition */
						select @DrugItemPriceHistoryId = -1,
						@DrugItemPriceId = 0,
						@PriceId = 0,              
						@PriceStartDate = null,         
						@PriceEndDate = null,      
						@Price = null,      
						@IsTemporary = 0,                                     	                  
						@IsFSS = 0,                                           	                  
						@IsBIG4 = 0,                                          	                  
						@IsVA = 0,                                            	                  
						@IsBOP = 0,                                           	                  
						@IsCMOP = 0,                                          	                  
						@IsDOD = 0,                                           	                  
						@IsHHS = 0,                                           	                  
						@IsIHS = 0,                                           	                  
						@IsIHS2 = 0,                                          	                  
						@IsDIHS = 0,                                          	                  
						@IsNIH = 0,                                           	                  
						@IsPHS = 0,                                           	                  
						@IsSVH = 0,                                           	                  
						@IsSVH1 = 0,                                          	                  
						@IsSVH2 = 0,                                          	                  
						@IsTMOP = 0,                                          	                  
						@IsUSCG = 0, 
						@IsFHCC = 0,  
						@FCP = 0,
						@Covered = 'F',     
						@DualPriceDesignation = 'F',      
						@VAIFF = 0,                            	                  
						@AwardedFSSTrackingCustomerRatio = null,        
						@TrackingCustomerName = '',
						@CurrentTrackingCustomerPrice = null,       
						@ExcludeFromExport = 0,
						@LastModificationType = '',			     
						@ModificationStatusId = 0,                  
						@CreatedBy = 'Fred',     
						@CreationDate = getdate(),        
						@LastModifiedBy = 'Fred',        
						@LastModificationDate = getdate(),
						@IsNewBlankRow = 1,
						@IsFromHistory = 0,
						@IsHistoryFromArchive = 0,
						@TPRAlwaysHasBasePrice = 0,
						@Notes = '',
						@DrugItemSubItemId = -1,
						@SubItemIdentifier = ''
			
					select 
						-1 as DrugItemPriceHistoryId,
						p.DrugItemPriceId ,
						p.DrugItemId,             
						p.PriceId,              
						p.PriceStartDate,         
						p.PriceStopDate as PriceEndDate,       
						p.Price,      
						p.IsTemporary,                                     	                  
						p.IsFSS,                                           	                  
						p.IsBIG4,                                          	                  
						p.IsVA,                                            	                  
						p.IsBOP,                                           	                  
						p.IsCMOP,                                          	                  
						p.IsDOD,                                           	                  
						p.IsHHS,                                           	                  
						p.IsIHS,                                           	                  
						p.IsIHS2,                                          	                  
						p.IsDIHS,                                          	                  
						p.IsNIH,                                           	                  
						p.IsPHS,                                           	                  
						p.IsSVH,                                           	                  
						p.IsSVH1,                                          	                  
						p.IsSVH2,                                          	                  
						p.IsTMOP,                                          	                  
						p.IsUSCG,
						p.IsFHCC, 
						i.Covered,        
						case dbo.GetItemDualPriceStatusForDrugItemId( @DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,     
						dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,           
						dbo.GetFCPValueForDrugItem( @DrugItemId, YEAR(p.PriceStartDate) ) as FCP,                        	                  
						p.AwardedFSSTrackingCustomerRatio,        
						p.TrackingCustomerName,        
						p.CurrentTrackingCustomerPrice,         
						p.ExcludeFromExport,
						p.LastModificationType,				
						p.ModificationStatusId,                  
						p.CreatedBy,     
						p.CreationDate,        
						p.LastModifiedBy,        
						p.LastModificationDate,
						0 as IsNewBlankRow,
						0 as IsFromHistory,
						0 as IsHistoryFromArchive,
						dbo.GetTPRAlwaysHasBasePriceFunction( p.DrugItemId,          
									p.PriceStartDate,         
									p.PriceStopDate,       
									p.IsTemporary,                                     	                  
									p.IsFSS,                                           	                  
									p.IsBIG4,                                          	                  
									p.IsVA,                                            	                  
									p.IsBOP,                                           	                  
									p.IsCMOP,                                          	                  
									p.IsDOD,                                           	                  
									p.IsHHS,                                           	                  
									p.IsIHS,                                           	                  
									p.IsIHS2,                                          	                  
									p.IsDIHS,                                          	                  
									p.IsNIH,                                           	                  
									p.IsPHS,                                           	                  
									p.IsSVH,                                           	                  
									p.IsSVH1,                                          	                  
									p.IsSVH2,                                          	                  
									p.IsTMOP,                                          	                  
									p.IsUSCG,
									p.IsFHCC ) as TPRAlwaysHasBasePrice,  
						'' as Notes,
						p.DrugItemSubItemId,
						s.SubItemIdentifier
					from DI_DrugItemPrice p join DI_DrugItems i
						on p.DrugItemId = i.DrugItemId
					left outer join DI_DrugItemSubItems s
						on i.DrugItemId = s.DrugItemId
						and p.DrugItemSubItemId = s.DrugItemSubItemId
						where i.DrugItemId = @DrugItemId 
						and p.PriceStartDate > @dateWithoutTime
									

					UNION
					
					/* return a new blank row at the applications request */
					select 
						@DrugItemPriceHistoryId as DrugItemPriceHistoryId,
						@DrugItemPriceId as DrugItemPriceId ,
						@DrugItemId as DrugItemId,             
						@PriceId as PriceId,              
						@PriceStartDate as PriceStartDate,         
						@PriceEndDate as PriceEndDate,       
						@Price as Price,      
						@IsTemporary as IsTemporary,                                     	                  
						@IsFSS  as IsFSS,                                           	                  
						@IsBIG4  as IsBIG4,                                          	                  
						@IsVA  as IsVA,                                            	                  
						@IsBOP  as IsBOP,                                           	                  
						@IsCMOP  as IsCMOP,                                          	                  
						@IsDOD  as IsDOD,                                           	                  
						@IsHHS  as IsHHS,                                           	                  
						@IsIHS  as IsIHS,                                           	                  
						@IsIHS2  as IsIHS2,                                          	                  
						@IsDIHS  as IsDIHS,                                          	                  
						@IsNIH  as IsNIH,                                           	                  
						@IsPHS  as IsPHS,                                           	                  
						@IsSVH  as IsSVH,                                           	                  
						@IsSVH1  as IsSVH1,                                          	                  
						@IsSVH2  as IsSVH2,                                          	                  
						@IsTMOP  as IsTMOP,                                          	                  
						@IsUSCG  as IsUSCG,    
						@IsFHCC as IsFHCC,       
						@Covered as Covered,
						@DualPriceDesignation as DualPriceDesignation,
						@VAIFF as VAIFF,
						@FCP as FCP,                               	                  
						@AwardedFSSTrackingCustomerRatio  as  AwardedFSSTrackingCustomerRatio, 		
						@TrackingCustomerName  as TrackingCustomerName,
						@CurrentTrackingCustomerPrice  as  CurrentTrackingCustomerPrice,         
						@ExcludeFromExport as ExcludeFromExport,
						@LastModificationType as LastModificationType,			     
						@ModificationStatusId  as  ModificationStatusId,                  
						@CreatedBy  as  CreatedBy,     
						@CreationDate  as  CreationDate,        
						@LastModifiedBy  as  LastModifiedBy,        
						@LastModificationDate  as  LastModificationDate,
						@IsNewBlankRow as IsNewBlankRow,
						@IsFromHistory as IsFromHistory,
						@IsHistoryFromArchive as IsHistoryFromArchive,
						@TPRAlwaysHasBasePrice as TPRAlwaysHasBasePrice,
						@Notes as Notes,
						@DrugItemSubItemId as DrugItemSubItemId,
						@SubItemIdentifier as SubItemIdentifier
						
					select @error = @@error
					
					if @error <> 0
					BEGIN
						select @errorMsg = 'Error retrieving drug item prices for fss contract (F2) ' + @ContractNumber
						raiserror( @errorMsg, 16, 1 )
					END
				END		
			END -- future only
			else
			BEGIN -- both future and active
				if @FutureHistoricalSelectionCriteria = 'B'
				BEGIN

					if @WithAddPrice = 0
					BEGIN
							select 
								-1 as DrugItemPriceHistoryId,
								p.DrugItemPriceId ,
								p.DrugItemId,             
								p.PriceId,              
								p.PriceStartDate,         
								p.PriceStopDate as PriceEndDate,       
								p.Price,      
								p.IsTemporary,                                     	                  
								p.IsFSS,                                           	                  
								p.IsBIG4,                                          	                  
								p.IsVA,                                            	                  
								p.IsBOP,                                           	                  
								p.IsCMOP,                                          	                  
								p.IsDOD,                                           	                  
								p.IsHHS,                                           	                  
								p.IsIHS,                                           	                  
								p.IsIHS2,                                          	                  
								p.IsDIHS,                                          	                  
								p.IsNIH,                                           	                  
								p.IsPHS,                                           	                  
								p.IsSVH,                                           	                  
								p.IsSVH1,                                          	                  
								p.IsSVH2,                                          	                  
								p.IsTMOP,                                          	                  
								p.IsUSCG,
								p.IsFHCC,
								i.Covered,
								case dbo.GetItemDualPriceStatusForDrugItemId( @DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,     
								dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,      
								dbo.GetFCPValueForDrugItem( @DrugItemId, YEAR(p.PriceStartDate) ) as FCP,
								p.AwardedFSSTrackingCustomerRatio,        
								p.TrackingCustomerName,        
								p.CurrentTrackingCustomerPrice,         
								p.ExcludeFromExport,
								p.LastModificationType,
								p.ModificationStatusId,                  
								p.CreatedBy,     
								p.CreationDate,        
								p.LastModifiedBy,        
								p.LastModificationDate,
								0 as IsNewBlankRow,
								0 as IsFromHistory,
								0 as IsHistoryFromArchive,
								dbo.GetTPRAlwaysHasBasePriceFunction( p.DrugItemId,          
											p.PriceStartDate,         
											p.PriceStopDate,       
											p.IsTemporary,                                     	                  
											p.IsFSS,                                           	                  
											p.IsBIG4,                                          	                  
											p.IsVA,                                            	                  
											p.IsBOP,                                           	                  
											p.IsCMOP,                                          	                  
											p.IsDOD,                                           	                  
											p.IsHHS,                                           	                  
											p.IsIHS,                                           	                  
											p.IsIHS2,                                          	                  
											p.IsDIHS,                                          	                  
											p.IsNIH,                                           	                  
											p.IsPHS,                                           	                  
											p.IsSVH,                                           	                  
											p.IsSVH1,                                          	                  
											p.IsSVH2,                                          	                  
											p.IsTMOP,                                          	                  
											p.IsUSCG,
											p.IsFHCC ) as TPRAlwaysHasBasePrice,  
								'' as Notes,
								p.DrugItemSubItemId,
								s.SubItemIdentifier
							from DI_DrugItemPrice p join DI_DrugItems i
								on p.DrugItemId = i.DrugItemId
							left outer join DI_DrugItemSubItems s
								on i.DrugItemId = s.DrugItemId
								and p.DrugItemSubItemId = s.DrugItemSubItemId

							where i.DrugItemId = @DrugItemId 
							and ( @dateWithoutTime between p.PriceStartDate and p.PriceStopDate
							or p.PriceStartDate > @dateWithoutTime )

							order by p.PriceStartDate

							select @error = @@error
							
							if @error <> 0
							BEGIN
								select @errorMsg = 'Error retrieving drug item prices for fss contract (B1) ' + @ContractNumber
								raiserror( @errorMsg, 16, 1 )
							END
						END
					ELSE
						BEGIN
							/* blank row definition */
							select @DrugItemPriceHistoryId = -1,
							@DrugItemPriceId = 0,
							@PriceId = 0,              
							@PriceStartDate = null,         
							@PriceEndDate = null,      
							@Price = null,      
							@IsTemporary = 0,                                     	                  
							@IsFSS = 0,                                           	                  
							@IsBIG4 = 0,                                          	                  
							@IsVA = 0,                                            	                  
							@IsBOP = 0,                                           	                  
							@IsCMOP = 0,                                          	                  
							@IsDOD = 0,                                           	                  
							@IsHHS = 0,                                           	                  
							@IsIHS = 0,                                           	                  
							@IsIHS2 = 0,                                          	                  
							@IsDIHS = 0,                                          	                  
							@IsNIH = 0,                                           	                  
							@IsPHS = 0,                                           	                  
							@IsSVH = 0,                                           	                  
							@IsSVH1 = 0,                                          	                  
							@IsSVH2 = 0,                                          	                  
							@IsTMOP = 0,                                          	                  
							@IsUSCG = 0,   
							@IsFHCC = 0,
							@FCP = 0,
							@Covered = 'F',    
							@DualPriceDesignation = 'F',
							@VAIFF = 0,                                   	                  
							@AwardedFSSTrackingCustomerRatio = null,        
							@TrackingCustomerName = '',
							@CurrentTrackingCustomerPrice = null,       
							@ExcludeFromExport = 0,
							@LastModificationType = '',			     
							@ModificationStatusId = 0,                  
							@CreatedBy = 'Fred',     
							@CreationDate = getdate(),        
							@LastModifiedBy = 'Fred',        
							@LastModificationDate = getdate(),
							@IsNewBlankRow = 1,
							@IsFromHistory = 0,
							@IsHistoryFromArchive = 0,
							@TPRAlwaysHasBasePrice = 0,
							@Notes = '',
							@DrugItemSubItemId = -1,
							@SubItemIdentifier = ''
				
						select 
							-1 as DrugItemPriceHistoryId,
							p.DrugItemPriceId ,
							p.DrugItemId,             
							p.PriceId,              
							p.PriceStartDate,         
							p.PriceStopDate as PriceEndDate,       
							p.Price,      
							p.IsTemporary,                                     	                  
							p.IsFSS,                                           	                  
							p.IsBIG4,                                          	                  
							p.IsVA,                                            	                  
							p.IsBOP,                                           	                  
							p.IsCMOP,                                          	                  
							p.IsDOD,                                           	                  
							p.IsHHS,                                           	                  
							p.IsIHS,                                           	                  
							p.IsIHS2,                                          	                  
							p.IsDIHS,                                          	                  
							p.IsNIH,                                           	                  
							p.IsPHS,                                           	                  
							p.IsSVH,                                           	                  
							p.IsSVH1,                                          	                  
							p.IsSVH2,                                          	                  
							p.IsTMOP,                                          	                  
							p.IsUSCG,
							p.IsFHCC, 
							i.Covered,        
							case dbo.GetItemDualPriceStatusForDrugItemId( @DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,     
							dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,           
							dbo.GetFCPValueForDrugItem( @DrugItemId, YEAR(p.PriceStartDate) ) as FCP,                        	                  
							p.AwardedFSSTrackingCustomerRatio,        
							p.TrackingCustomerName,        
							p.CurrentTrackingCustomerPrice,         
							p.ExcludeFromExport,
							p.LastModificationType,				
							p.ModificationStatusId,                  
							p.CreatedBy,     
							p.CreationDate,        
							p.LastModifiedBy,        
							p.LastModificationDate,
							0 as IsNewBlankRow,
							0 as IsFromHistory,
							0 as IsHistoryFromArchive,
							dbo.GetTPRAlwaysHasBasePriceFunction( p.DrugItemId,          
											p.PriceStartDate,         
											p.PriceStopDate,       
											p.IsTemporary,                                     	                  
											p.IsFSS,                                           	                  
											p.IsBIG4,                                          	                  
											p.IsVA,                                            	                  
											p.IsBOP,                                           	                  
											p.IsCMOP,                                          	                  
											p.IsDOD,                                           	                  
											p.IsHHS,                                           	                  
											p.IsIHS,                                           	                  
											p.IsIHS2,                                          	                  
											p.IsDIHS,                                          	                  
											p.IsNIH,                                           	                  
											p.IsPHS,                                           	                  
											p.IsSVH,                                           	                  
											p.IsSVH1,                                          	                  
											p.IsSVH2,                                          	                  
											p.IsTMOP,                                          	                  
											p.IsUSCG,
											p.IsFHCC ) as TPRAlwaysHasBasePrice,  
							'' as Notes,
							p.DrugItemSubItemId,
							s.SubItemIdentifier
						from DI_DrugItemPrice p join DI_DrugItems i
							on p.DrugItemId = i.DrugItemId
						left outer join DI_DrugItemSubItems s
							on i.DrugItemId = s.DrugItemId
							and p.DrugItemSubItemId = s.DrugItemSubItemId

							where i.DrugItemId = @DrugItemId 
							and ( @dateWithoutTime between p.PriceStartDate and p.PriceStopDate
							or p.PriceStartDate > @dateWithoutTime )
			
							
						UNION
						
						/* return a new blank row at the applications request */
						select 
							@DrugItemPriceHistoryId as DrugItemPriceHistoryId,
							@DrugItemPriceId as DrugItemPriceId ,
							@DrugItemId as DrugItemId,             
							@PriceId as PriceId,              
							@PriceStartDate as PriceStartDate,         
							@PriceEndDate as PriceEndDate,       
							@Price as Price,      
							@IsTemporary as IsTemporary,                                     	                  
							@IsFSS  as IsFSS,                                           	                  
							@IsBIG4  as IsBIG4,                                          	                  
							@IsVA  as IsVA,                                            	                  
							@IsBOP  as IsBOP,                                           	                  
							@IsCMOP  as IsCMOP,                                          	                  
							@IsDOD  as IsDOD,                                           	                  
							@IsHHS  as IsHHS,                                           	                  
							@IsIHS  as IsIHS,                                           	                  
							@IsIHS2  as IsIHS2,                                          	                  
							@IsDIHS  as IsDIHS,                                          	                  
							@IsNIH  as IsNIH,                                           	                  
							@IsPHS  as IsPHS,                                           	                  
							@IsSVH  as IsSVH,                                           	                  
							@IsSVH1  as IsSVH1,                                          	                  
							@IsSVH2  as IsSVH2,                                          	                  
							@IsTMOP  as IsTMOP,                                          	                  
							@IsUSCG  as IsUSCG,
							@IsFHCC as IsFHCC,           
							@Covered as Covered,
							@DualPriceDesignation as DualPriceDesignation,
							@VAIFF as VAIFF,
							@FCP as FCP,                               	                  
							@AwardedFSSTrackingCustomerRatio  as  AwardedFSSTrackingCustomerRatio, 		
							@TrackingCustomerName  as TrackingCustomerName,
							@CurrentTrackingCustomerPrice  as  CurrentTrackingCustomerPrice,         
							@ExcludeFromExport as ExcludeFromExport,
							@LastModificationType as LastModificationType,			     
							@ModificationStatusId  as  ModificationStatusId,                  
							@CreatedBy  as  CreatedBy,     
							@CreationDate  as  CreationDate,        
							@LastModifiedBy  as  LastModifiedBy,        
							@LastModificationDate  as  LastModificationDate,
							@IsNewBlankRow as IsNewBlankRow,
							@IsFromHistory as IsFromHistory,
							@IsHistoryFromArchive as IsHistoryFromArchive,
							@TPRAlwaysHasBasePrice as TPRAlwaysHasBasePrice,
							@Notes as Notes,
							@DrugItemSubItemId as DrugItemSubItemId,
							@SubItemIdentifier as SubItemIdentifier
							
						select @error = @@error
						
						if @error <> 0
						BEGIN
							select @errorMsg = 'Error retrieving drug item prices for fss contract (B2) ' + @ContractNumber
							raiserror( @errorMsg, 16, 1 )
						END
					END		
				END -- "B"
				else
				BEGIN -- incorrect parameter
					select @errorMsg = 'Incorrect filter ' + @FutureHistoricalSelectionCriteria + ' while retrieving drug item prices for fss contract ' + @ContractNumber
					raiserror( @errorMsg, 16, 1 )		
				END
			END -- both future and active
		END -- either future or both
	END -- non historical
END
