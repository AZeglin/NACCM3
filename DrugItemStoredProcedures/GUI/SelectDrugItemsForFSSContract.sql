IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SelectDrugItemsForFSSContract]') AND type in (N'P', N'PC'))
DROP PROCEDURE [SelectDrugItemsForFSSContract]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure SelectDrugItemsForFSSContract
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@WithAdd bit = 0,
@CoveredSelectionCriteria nchar(1) = 'B',
@IsBPA bit = 0
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@desiredCoveredSelectionCriteria nchar(1),
	@dateWithoutTime datetime,
	
	@DrugItemId int,
	@DrugItemNDCId int,   
	@FdaAssignedLabelerCode char(5),
	@ProductCode char(4),
	@PackageCode char(2),
	@PackageDescription nvarchar(14),
	@Generic nvarchar(64),
	@TradeName nvarchar(45),
	@DiscontinuationDate as datetime,
	@DiscontinuationEnteredDate as datetime,
	@DiscontinuationReason as nvarchar(512),
	@Covered nchar(1),
	@PrimeVendor nchar(1),
	@PrimeVendorChangedDate datetime,
	@FCP decimal(9,2),
	@CurrentFSSPrice decimal(9,2),
	@PriceStartDate datetime,
	@PriceEndDate datetime,
	@PassThrough nchar(1),
	@DispensingUnit nvarchar(10),
	@VAClass nvarchar(5),
	@DualPriceDesignation nchar(1),
	@ExcludeFromExport bit,
	@NonTAA bit, 
	@IncludedFETAmount float,
	@ParentDrugItemId int,
	@LastModificationType nchar(1),
	@ModificationStatusId int,
	@CreatedBy nvarchar(120),
	@CreationDate datetime,
	@LastModifiedBy nvarchar(120),
	@LastModificationDate datetime,
	@IsNewBlankRow bit,
	@PotentiallyHasBPA bit,
	@HasBPA bit

BEGIN

	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId from fss contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
	
	if exists( select ContractId
				from DI_Contracts
				where ParentFSSContractId = @ContractId )
	BEGIN
		select @PotentiallyHasBPA = 1
	END
	else
	BEGIN
		select @PotentiallyHasBPA = 0
	END
	
	select @dateWithoutTime = convert( datetime, convert( nvarchar(2), DatePart( month, getdate() )) + '/' + convert( nvarchar(2), DatePart( day, getdate() )) + '/' + convert( nvarchar(4), DatePart( year, getdate() )))
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error getting date without time ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

	if @CoveredSelectionCriteria = 'B'
	BEGIN
	
		if @WithAdd = 0
		BEGIN
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
				r.DiscontinuationReason,
				i.Covered,         
				i.PrimeVendor,
				i.PrimeVendorChangedDate , 
				dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( p.PriceStartDate )) as FCP,     
				dbo.GetFSSPriceForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as CurrentFSSPrice,
				dbo.GetFSSPriceStartDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceStartDate,
				dbo.GetFSSPriceEndDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceEndDate,
				case @PotentiallyHasBPA when 1 then dbo.GetHasBPAForItemFunction( i.DrugItemId ) else 0 end as HasBPA,
			--	p.Price as CurrentFSSPrice,
			--	p.PriceStartDate as PriceStartDate,
			--	p.PriceStopDate as PriceEndDate,	      	             
				i.PassThrough,            
				i.DispensingUnit ,        
				i.VAClass , 
				case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
				i.ExcludeFromExport,
				i.NonTAA, 
				i.IncludedFETAmount,
				i.ParentDrugItemId,
				i.LastModificationType,       
				i.ModificationStatusId  ,                 
				i.CreatedBy ,     
				i.CreationDate ,         
				i.LastModifiedBy ,    
				i.LastModificationDate,
				0 as IsNewBlankRow
				
			from    DI_DrugItems i left outer join DI_DrugItemPrice p
					on i.DrugItemId = p.DrugItemId
					and p.IsFSS = 1
					and p.IsTemporary = 0
					and p.DrugItemSubItemId is null
					and getdate() between p.PriceStartDate and p.PriceStopDate 
				  join DI_DrugItemNDC n
					on i.DrugItemNDCId = n.DrugItemNDCId
 				left outer join DI_ItemDiscontinuationReasons r on i.DiscontinuationReasonId = r.DiscontinuationReasonId

				where i.ContractId = @ContractId    
			--	and i.DiscontinuationDate is null
				and ( i.DiscontinuationDate >= @dateWithoutTime or i.DiscontinuationDate is null ) 
				
				order by n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode

		--	from DI_DrugItems i, DI_DrugItemPrice p, DI_DrugItemNDC n
		--	where i.ContractId = @ContractId      
		--	and i.DrugItemNDCId = n.DrugItemNDCId
		--	and i.DrugItemId left outer join p.DrugItemId
		--	and p.IsFSS = 1
		--	and getdate() between p.PriceStartDate and p.PriceStopDate      
		--	and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ) 
		--	order by i.FdaAssignedLabelerCode, i.ProductCode, i.PackageCode
			
			select @error = @@error
			
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error retrieving drug items for fss contract ' + @ContractNumber
				raiserror( @errorMsg, 16, 1 )
			END
		END
		else
		BEGIN
				/* blank row definition */
				select @DrugItemId = 0,
				@DrugItemNDCId = 0,
				@FdaAssignedLabelerCode = '',
				@ProductCode = '',
				@PackageCode = '',
				@PackageDescription = '',
				@Generic = '',
				@TradeName = '',
				@DiscontinuationDate = null,
				@DiscontinuationEnteredDate = null,
				@DiscontinuationReason = null,
				@Covered = 'F',
				@PrimeVendor = 'F',
				@PrimeVendorChangedDate = null,
				@FCP = null,
				@CurrentFSSPrice = null,
				@PriceStartDate = null,
				@PriceEndDate = null,
				@HasBPA = 0,
				@PassThrough = 'F',
				@DispensingUnit = null,
				@VAClass = '',
				@DualPriceDesignation = '',
				@ExcludeFromExport = 0,
				@NonTAA = 0, 
				@IncludedFETAmount = 0,
				@ParentDrugItemId = -1,
				@LastModificationType = '',
				@ModificationStatusId = 0,
				@CreatedBy = 'Fred',
				@CreationDate = getdate(),
				@LastModifiedBy = 'Fred',
				@LastModificationDate = getdate(),
				@IsNewBlankRow = 1

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
				r.DiscontinuationReason,    	             
				i.Covered,         
				i.PrimeVendor,
				i.PrimeVendorChangedDate , 
				dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( p.PriceStartDate )) as FCP,        
				dbo.GetFSSPriceForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as CurrentFSSPrice,
				dbo.GetFSSPriceStartDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceStartDate,
				dbo.GetFSSPriceEndDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceEndDate,
				case @PotentiallyHasBPA when 1 then dbo.GetHasBPAForItemFunction( i.DrugItemId ) else 0 end as HasBPA,
			--	p.Price as CurrentFSSPrice,
			--	p.PriceStartDate as PriceStartDate,
			--	p.PriceStopDate as PriceEndDate,	      	             
				i.PassThrough,            
				i.DispensingUnit ,        
				i.VAClass ,        
				case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
				i.ExcludeFromExport,
				i.NonTAA, 
				i.IncludedFETAmount,
				i.ParentDrugItemId,
				i.LastModificationType,       
				i.ModificationStatusId  ,                 
				i.CreatedBy ,     
				i.CreationDate ,         
				i.LastModifiedBy ,    
				i.LastModificationDate,
				0 as IsNewBlankRow
			from  DI_DrugItems i left outer join DI_DrugItemPrice p
					on i.DrugItemId = p.DrugItemId
					and p.IsFSS = 1
					and p.IsTemporary = 0
					and getdate() between p.PriceStartDate and p.PriceStopDate 
				  join DI_DrugItemNDC n
					on i.DrugItemNDCId = n.DrugItemNDCId
				left outer join DI_ItemDiscontinuationReasons r on i.DiscontinuationReasonId = r.DiscontinuationReasonId

				where i.ContractId = @ContractId    
			--	and i.DiscontinuationDate is null
				and ( i.DiscontinuationDate >= @dateWithoutTime or i.DiscontinuationDate is null ) 

		--	from DI_DrugItems i, DI_DrugItemPrice p, DI_DrugItemNDC n
		--	where i.ContractId = @ContractId      
		--	and i.DrugItemNDCId = n.DrugItemNDCId
		--	and i.DrugItemId left outer join p.DrugItemId
		--	and p.IsFSS = 1
		--	and getdate() between p.PriceStartDate and p.PriceStopDate          	          
		--	and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ) 
		
			/* return a new blank row at the applications request */
			union
			
				select @DrugItemId as DrugItemId,  
				@DrugItemNDCId as DrugItemNDCId,
				@FdaAssignedLabelerCode as FdaAssignedLabelerCode,    
				@ProductCode as ProductCode,       
				@PackageCode as PackageCode,         
				@PackageDescription as PackageDescription  ,        
				@Generic as Generic ,   
				@TradeName as TradeName ,     
				@DiscontinuationDate as DiscontinuationDate,                             	             
				@DiscontinuationEnteredDate as DiscontinuationEnteredDate,
				@DiscontinuationReason as DiscontinuationReason,
				@Covered as Covered,         
				@PrimeVendor as PrimeVendor,
				@PrimeVendorChangedDate as PrimeVendorChangedDate ,		
				@FCP as FCP ,     
				@CurrentFSSPrice as CurrentFSSPrice,
				@PriceStartDate as PriceStartDate,
				@PriceEndDate as PriceEndDate,		
				@HasBPA as HasBPA,
				@PassThrough as PassThrough,            
				@DispensingUnit as DispensingUnit ,        
				@VAClass as VAClass ,        
				@DualPriceDesignation as DualPriceDesignation,
				@ExcludeFromExport as ExcludeFromExport,
				@NonTAA as NonTAA,
				@IncludedFETAmount as IncludedFETAmount,
				@ParentDrugItemId as ParentDrugItemId,
				@LastModificationType as LastModificationType,
				@ModificationStatusId as ModificationStatusId  ,                 
				@CreatedBy as CreatedBy ,     
				@CreationDate as CreationDate ,         
				@LastModifiedBy as LastModifiedBy ,    
				@LastModificationDate as LastModificationDate,
				@IsNewBlankRow as IsNewBlankRow
			
			
			select @error = @@error
			
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error retrieving drug items for fss contract ' + @ContractNumber
				raiserror( @errorMsg, 16, 1 )
			END
		END
	END
--$$$
	else if @CoveredSelectionCriteria = 'D'
	BEGIN
		-- cannot add new if viewing discontinued
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
			r.DiscontinuationReason,
			i.Covered,         
			i.PrimeVendor,
			i.PrimeVendorChangedDate , 
			dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( p.PriceStartDate )) as FCP,     
			dbo.GetFSSPriceForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as CurrentFSSPrice,
			dbo.GetFSSPriceStartDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceStartDate,
			dbo.GetFSSPriceEndDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceEndDate,
			case @PotentiallyHasBPA when 1 then dbo.GetHasBPAForItemFunction( i.DrugItemId ) else 0 end as HasBPA,
		--	p.Price as CurrentFSSPrice,
		--	p.PriceStartDate as PriceStartDate,
		--	p.PriceStopDate as PriceEndDate,	      	             
			i.PassThrough,            
			i.DispensingUnit ,        
			i.VAClass , 
			case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
			i.ExcludeFromExport,
			i.NonTAA, 
			i.IncludedFETAmount,
			i.ParentDrugItemId,
			i.LastModificationType,       
			i.ModificationStatusId  ,                 
			i.CreatedBy ,     
			i.CreationDate ,         
			i.LastModifiedBy ,    
			i.LastModificationDate,
			0 as IsNewBlankRow
				
		from    DI_DrugItems i left outer join DI_DrugItemPrice p
				on i.DrugItemId = p.DrugItemId
				and p.IsFSS = 1
				and p.IsTemporary = 0
				and p.DrugItemSubItemId is null
				and getdate() between p.PriceStartDate and p.PriceStopDate 
				join DI_DrugItemNDC n
				on i.DrugItemNDCId = n.DrugItemNDCId
				left outer join DI_ItemDiscontinuationReasons r on i.DiscontinuationReasonId = r.DiscontinuationReasonId
			where i.ContractId = @ContractId    
			and i.DiscontinuationDate is not null
			and i.DiscontinuationDate < @dateWithoutTime
				
			order by n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode
			
		select @error = @@error
			
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving discontinued drug items for fss contract ' + @ContractNumber
			raiserror( @errorMsg, 16, 1 )
		END


	END
--$$$
	else -- handle specific covered selection criteria
	BEGIN
		if @CoveredSelectionCriteria = 'C'
		BEGIN
			select @desiredCoveredSelectionCriteria = 'T'
		END
		else
		BEGIN
			select @desiredCoveredSelectionCriteria = 'F'
		END
		
		if @WithAdd = 0
		BEGIN
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
				r.DiscontinuationReason,
				i.Covered,         
				i.PrimeVendor,
				i.PrimeVendorChangedDate , 
				dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( p.PriceStartDate )) as FCP,     
				dbo.GetFSSPriceForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as CurrentFSSPrice,
				dbo.GetFSSPriceStartDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceStartDate,
				dbo.GetFSSPriceEndDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceEndDate,
				case @PotentiallyHasBPA when 1 then dbo.GetHasBPAForItemFunction( i.DrugItemId ) else 0 end as HasBPA,
			--	p.Price as CurrentFSSPrice,
			--	p.PriceStartDate as PriceStartDate,
			--	p.PriceStopDate as PriceEndDate,	      	             
				i.PassThrough,            
				i.DispensingUnit ,        
				i.VAClass , 
				case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
				i.ExcludeFromExport,
				i.NonTAA, 
				i.IncludedFETAmount,
				i.ParentDrugItemId,
				i.LastModificationType,       
				i.ModificationStatusId  ,                 
				i.CreatedBy ,     
				i.CreationDate ,         
				i.LastModifiedBy ,    
				i.LastModificationDate,
				0 as IsNewBlankRow
				
			from    DI_DrugItems i left outer join DI_DrugItemPrice p
					on i.DrugItemId = p.DrugItemId
					and p.IsFSS = 1
					and p.IsTemporary = 0
					and getdate() between p.PriceStartDate and p.PriceStopDate 
				  join DI_DrugItemNDC n
					on i.DrugItemNDCId = n.DrugItemNDCId
				left outer join DI_ItemDiscontinuationReasons r on i.DiscontinuationReasonId = r.DiscontinuationReasonId

				where i.ContractId = @ContractId    
			--	and i.DiscontinuationDate is null
				and i.Covered = @desiredCoveredSelectionCriteria
				
				and ( i.DiscontinuationDate >= @dateWithoutTime or i.DiscontinuationDate is null ) 

				order by n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode

		--	from DI_DrugItems i, DI_DrugItemPrice p, DI_DrugItemNDC n
		--	where i.ContractId = @ContractId      
		--	and i.DrugItemNDCId = n.DrugItemNDCId
		--	and i.DrugItemId left outer join p.DrugItemId
		--	and p.IsFSS = 1
		--	and getdate() between p.PriceStartDate and p.PriceStopDate      
		--	and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ) 
		--	order by i.FdaAssignedLabelerCode, i.ProductCode, i.PackageCode
			
			select @error = @@error
			
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error retrieving drug items for fss contract ' + @ContractNumber
				raiserror( @errorMsg, 16, 1 )
			END
		END
		else
		BEGIN
				/* blank row definition */
				select @DrugItemId = 0,
				@DrugItemNDCId = 0,
				@FdaAssignedLabelerCode = '',
				@ProductCode = '',
				@PackageCode = '',
				@PackageDescription = '',
				@Generic = '',
				@TradeName = '',
				@DiscontinuationDate = null,
				@DiscontinuationEnteredDate = null,
				@DiscontinuationReason = null,
				@Covered = @desiredCoveredSelectionCriteria,
				@PrimeVendor = 'F',
				@PrimeVendorChangedDate = null,
				@FCP = null,
				@CurrentFSSPrice = null,
				@PriceStartDate = null,
				@PriceEndDate = null,
				@HasBPA = 0,
				@PassThrough = 'F',
				@DispensingUnit = null,
				@VAClass = '',
				@DualPriceDesignation = '',
				@ExcludeFromExport = 0,
				@NonTAA = 0, 
				@IncludedFETAmount = 0,
				@ParentDrugItemId = -1,
				@LastModificationType = '',
				@ModificationStatusId = 0,
				@CreatedBy = 'Fred',
				@CreationDate = getdate(),
				@LastModifiedBy = 'Fred',
				@LastModificationDate = getdate(),
				@IsNewBlankRow = 1

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
				r.DiscontinuationReason,   	             
				i.Covered,         
				i.PrimeVendor,
				i.PrimeVendorChangedDate , 
				dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( p.PriceStartDate )) as FCP,        
				dbo.GetFSSPriceForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as CurrentFSSPrice,
				dbo.GetFSSPriceStartDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceStartDate,
				dbo.GetFSSPriceEndDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceEndDate,
				case @PotentiallyHasBPA when 1 then dbo.GetHasBPAForItemFunction( i.DrugItemId ) else 0 end as HasBPA,
			--	p.Price as CurrentFSSPrice,
			--	p.PriceStartDate as PriceStartDate,
			--	p.PriceStopDate as PriceEndDate,	      	             
				i.PassThrough,            
				i.DispensingUnit ,        
				i.VAClass ,        
				case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
				i.ExcludeFromExport,
				i.NonTAA, 
				i.IncludedFETAmount,
				i.ParentDrugItemId,
				i.LastModificationType,       
				i.ModificationStatusId  ,                 
				i.CreatedBy ,     
				i.CreationDate ,         
				i.LastModifiedBy ,    
				i.LastModificationDate,
				0 as IsNewBlankRow
			from  DI_DrugItems i left outer join DI_DrugItemPrice p
					on i.DrugItemId = p.DrugItemId
					and p.IsFSS = 1
					and p.IsTemporary = 0
    				and getdate() between p.PriceStartDate and p.PriceStopDate 
				  join DI_DrugItemNDC n
					on i.DrugItemNDCId = n.DrugItemNDCId
  				left outer join DI_ItemDiscontinuationReasons r on i.DiscontinuationReasonId = r.DiscontinuationReasonId

				where i.ContractId = @ContractId    
			--	and i.DiscontinuationDate is null
				and i.Covered = @desiredCoveredSelectionCriteria

				and ( i.DiscontinuationDate >= @dateWithoutTime or i.DiscontinuationDate is null ) 

		--	from DI_DrugItems i, DI_DrugItemPrice p, DI_DrugItemNDC n
		--	where i.ContractId = @ContractId      
		--	and i.DrugItemNDCId = n.DrugItemNDCId
		--	and i.DrugItemId left outer join p.DrugItemId
		--	and p.IsFSS = 1
		--	and getdate() between p.PriceStartDate and p.PriceStopDate          	          
		--	and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ) 
		
			/* return a new blank row at the applications request */
			union
			
				select @DrugItemId as DrugItemId,  
				@DrugItemNDCId as DrugItemNDCId,
				@FdaAssignedLabelerCode as FdaAssignedLabelerCode,    
				@ProductCode as ProductCode,       
				@PackageCode as PackageCode,         
				@PackageDescription as PackageDescription  ,        
				@Generic as Generic ,   
				@TradeName as TradeName ,     
				@DiscontinuationDate as DiscontinuationDate,                             	             
				@DiscontinuationEnteredDate as DiscontinuationEnteredDate,
				@DiscontinuationReason as DiscontinuationReason,
				@Covered as Covered,         
				@PrimeVendor as PrimeVendor,
				@PrimeVendorChangedDate as PrimeVendorChangedDate ,		
				@FCP as FCP ,     
				@CurrentFSSPrice as CurrentFSSPrice,
				@PriceStartDate as PriceStartDate,
				@PriceEndDate as PriceEndDate,	
				@HasBPA as HasBPA,	
				@PassThrough as PassThrough,            
				@DispensingUnit as DispensingUnit ,        
				@VAClass as VAClass ,        
				@DualPriceDesignation as DualPriceDesignation,
				@ExcludeFromExport as ExcludeFromExport,
				@NonTAA as NonTAA, 
				@IncludedFETAmount as IncludedFETAmount,
				@ParentDrugItemId as ParentDrugItemId,
				@LastModificationType as LastModificationType,
				@ModificationStatusId as ModificationStatusId  ,                 
				@CreatedBy as CreatedBy ,     
				@CreationDate as CreationDate ,         
				@LastModifiedBy as LastModifiedBy ,    
				@LastModificationDate as LastModificationDate,
				@IsNewBlankRow as IsNewBlankRow
			
			
			select @error = @@error
			
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error retrieving drug items for fss contract ' + @ContractNumber
				raiserror( @errorMsg, 16, 1 )
			END
		END	
		
	
	END
END
