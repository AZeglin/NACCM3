IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectItemsAndPricesForContractReport')
	BEGIN
		DROP  Procedure  SelectItemsAndPricesForContractReport
	END

GO

CREATE Procedure SelectItemsAndPricesForContractReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@FutureHistoricalSelectionCriteria nchar(1),  -- H historical, F future, A active, B both future and active
@CoveredSelectionCriteria nchar(1) -- B both covered and non-covered, C covered only, N non-covered only
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@dateWithoutTime datetime,
	@IsBPA bit,
	@ScheduleNumber int,
	@desiredCoveredSelectionCriteria nchar(1),
	@Division int


BEGIN TRANSACTION 

	/* log the request for the report */
	exec InsertDrugItemUserActivity  @ReportUserLoginId, 'R', 'Items And Prices For Contract Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId from fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
--	exec dbo.GetDrugItemContractInfoLocal @ContractNumber, @ScheduleNumber OUTPUT, @IsBPA OUTPUT, @Division OUTPUT
	exec dbo.GetDrugItemContractInfoLocal @ContractNumber = @ContractNumber, @NACCMServerName = @NACCMServerName, @NACCMDatabaseName = @NACCMDatabaseName, @ScheduleNumber = @ScheduleNumber OUTPUT, @IsBPA = @IsBPA OUTPUT, @Division = @Division OUTPUT

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error retrieving contract info from contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	select @dateWithoutTime = convert( datetime, convert( nvarchar(2), DatePart( month, getdate() )) + '/' + convert( nvarchar(2), DatePart( day, getdate() )) + '/' + convert( nvarchar(4), DatePart( year, getdate() )))
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error getting date without time ' + @ContractNumber
		goto ERROREXIT
	END
	
	if @CoveredSelectionCriteria = 'B'
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
			i.Covered,         
			i.PrimeVendor,
			i.PrimeVendorChangedDate , 
			dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( p.PriceStartDate )) as FCP,     
			dbo.GetFSSPriceForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as CurrentFSSPrice,
			dbo.GetFSSPriceStartDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceStartDate,
			dbo.GetFSSPriceEndDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceEndDate,
			i.PassThrough,            
			i.DispensingUnit ,        
			i.VAClass , 
			case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
			i.ExcludeFromExport,
			i.ParentDrugItemId,
			i.LastModificationType,       
			i.ModificationStatusId  ,                 
			i.CreatedBy ,     
			i.CreationDate ,         
			i.LastModifiedBy ,    
			i.LastModificationDate
			
		from    DI_DrugItems i left outer join DI_DrugItemPrice p
				on i.DrugItemId = p.DrugItemId
				and p.IsFSS = 1
				and p.IsTemporary = 0
				and p.DrugItemSubItemId is null
				and	@dateWithoutTime between p.PriceStopDate and p.PriceStartDate
			  join DI_DrugItemNDC n
				on i.DrugItemNDCId = n.DrugItemNDCId
			where i.ContractId = @ContractId    
			and ( i.DiscontinuationDate >= @dateWithoutTime or i.DiscontinuationDate is null ) 
			
			order by n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode

			select @error = @@error

			if @error <> 0
			BEGIN
				select @errorMsg = 'Error retrieving drug items for fss contract ' + @ContractNumber
				goto ERROREXIT
			END
	END
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
			i.Covered,         
			i.PrimeVendor,
			i.PrimeVendorChangedDate , 
			dbo.GetFCPValueForDrugItem( i.DrugItemId, YEAR( p.PriceStartDate )) as FCP,     
			dbo.GetFSSPriceForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as CurrentFSSPrice,
			dbo.GetFSSPriceStartDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceStartDate,
			dbo.GetFSSPriceEndDateForItemFunction( i.DrugItemId, i.ParentDrugItemId, @IsBPA ) as PriceEndDate,
			i.PassThrough,            
			i.DispensingUnit ,        
			i.VAClass , 
			case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as DualPriceDesignation,
			i.ExcludeFromExport,
			i.ParentDrugItemId,
			i.LastModificationType,       
			i.ModificationStatusId  ,                 
			i.CreatedBy ,     
			i.CreationDate ,         
			i.LastModifiedBy ,    
			i.LastModificationDate
			
		from    DI_DrugItems i left outer join DI_DrugItemPrice p
				on i.DrugItemId = p.DrugItemId
				and p.IsFSS = 1
				and p.IsTemporary = 0
				and p.DrugItemSubItemId is null
				and	@dateWithoutTime between p.PriceStopDate and p.PriceStartDate
			  join DI_DrugItemNDC n
				on i.DrugItemNDCId = n.DrugItemNDCId
			where i.ContractId = @ContractId    
			and i.Covered = @desiredCoveredSelectionCriteria
			and ( i.DiscontinuationDate >= @dateWithoutTime or i.DiscontinuationDate is null ) 
			
			order by n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode

			select @error = @@error

			if @error <> 0
			BEGIN
				select @errorMsg = 'Error retrieving drug items for fss contract ' + @ContractNumber
				goto ERROREXIT
			END
		
			
	END
	
	goto OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 )
	
  	if @@TRANCOUNT > 1
  	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 




