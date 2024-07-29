IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractItemPriceErrorsReport')
	BEGIN
		DROP  Procedure  SelectContractItemPriceErrorsReport
	END

GO

CREATE Procedure SelectContractItemPriceErrorsReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@FutureHistoricalSelectionCriteria nchar(1)  -- F future, A active
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@dateWithoutTime datetime,
	@IsBPA bit,
	@ScheduleNumber int,
	@currentYear int,
	@currentYearNFAMPTableName nvarchar(11), -- passed as a parm, but not used
	@query nvarchar(1200),
	@SQLParms nvarchar(1000),
	@p_nfampTableName nvarchar(11),
	@activeContractTestDate datetime,
	@PercentageRepresentingPriceSwing decimal(9,2),
	@Division int



BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertDrugItemUserActivity @ReportUserLoginId, 'R', 'Contract Item Price Errors Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @PercentageRepresentingPriceSwing = 0.50 /* this should match percentage used in sub-report */

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
	
	select @currentYear = year(getdate())
	if( @FutureHistoricalSelectionCriteria = 'F' )
	BEGIN
		select @currentYear = @currentYear + 1
		
		select @activeContractTestDate = convert( datetime, '1/1/' + convert(nvarchar(4), @currentYear ) )
	END
	else
	BEGIN
		select @activeContractTestDate = getdate()
	END

	select @currentYearNFAMPTableName = 'nfamp' + convert(nvarchar(4), @currentYear )

	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#ErroredItemPrices')
	BEGIN
		delete #ErroredItemPrices
	END
	
	create table #ErroredItemPrices 
	(
		ContractId int NOT NULL, 
		NACCMContractNumber nvarchar(20) NOT NULL, 
		IsBPA	bit NULL,
		Division int NULL,
		CurrentYearNFAMPTableName nvarchar(11) NULL,
		DrugItemId                                      	int              NULL,
		DrugItemSubItemId	int NULL,
		DrugItemNDCId                                   	int              NULL,
		FdaAssignedLabelerCode                          	char(5)          NULL,
		ProductCode                                     	char(4)          NULL,
		PackageCode                                     	char(2)          NULL,
		SubItemIdentifier nchar(1) NULL,
		HistoricalNValue                                	nchar(1)         NULL,
		PackageDescription                              	nvarchar(14)     NULL,
		Generic                                         	nvarchar(64)     NULL,
		TradeName                                       	nvarchar(45)     NULL,
		DiscontinuationDate                             	datetime             NULL,
		DiscontinuationEnteredDate                      	datetime             NULL,
		DateEnteredMarket                               	datetime             NULL,
		Covered                                         	nchar(1)         NULL,
		PrimeVendor                                     	nchar(1)             NULL,
		PrimeVendorChangedDate                          	datetime             NULL,
		PassThrough                                     	nchar(1)             NULL,
		DispensingUnit                                  	nvarchar(10)         NULL,
		VAClass                                         	nvarchar(5)          NULL,
		ExcludeFromExport                               	bit              NULL,
		NonTAA												bit				NULL,
		IncludedFETAmount									decimal(10,2)    NULL,
		ParentDrugItemId                                	int              NULL,
		LastModificationType                            	nchar(1)         NULL,
		ModificationStatusId                            	int              NULL,
		CreatedBy                                       	nvarchar(120)    NULL,
		CreationDate                                    	datetime         NULL,
		LastModifiedBy                                  	nvarchar(120)    NULL,
		LastModificationDate                            	datetime         NULL,
		FCP								decimal(9,2) NULL,
		MostRecentHistoricalFCP decimal(9,2) NULL,
		SingleDualFromMailout nchar(1) NULL,
		ItemDualPriceStatus nchar(1) NULL, -- based on current active prices
		HasPricingErrors bit NULL, -- true if has any pricing errors - used as lookahead to help include or exclude item from report
		IsMissingItem bit NULL,
		IsDiscontinuedItem nchar(1) NULL,
		ErrorDescription				nvarchar(1000) NULL
	)

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table'
		goto ERROREXIT
	END

	insert into #ErroredItemPrices 
		( ContractId, 
			NACCMContractNumber, 
			IsBPA,
			Division,
			CurrentYearNFAMPTableName,
			DrugItemId,  
			DrugItemSubItemId,
			DrugItemNDCId  ,
			FdaAssignedLabelerCode ,
			ProductCode ,
			PackageCode ,
			SubItemIdentifier,
			HistoricalNValue ,
			IsMissingItem,
			IsDiscontinuedItem,
			PackageDescription ,
			Generic ,
			TradeName  ,
			DiscontinuationDate  ,
			DiscontinuationEnteredDate  ,
			DateEnteredMarket  ,
			Covered  ,
			PrimeVendor  ,
			PrimeVendorChangedDate ,
			PassThrough  ,
			DispensingUnit,
			VAClass   ,
			ExcludeFromExport ,
			NonTAA,
			IncludedFETAmount,
			ParentDrugItemId  ,
			LastModificationType ,
			ModificationStatusId ,
			CreatedBy  ,
			CreationDate ,
			LastModifiedBy,
			LastModificationDate,
			ItemDualPriceStatus )	
			
		select @ContractId,   /* with N */
			@ContractNumber,
			@IsBPA,
			@Division,
			@CurrentYearNFAMPTableName,
			i.DrugItemId, 
			s.DrugItemSubItemId,
			i.DrugItemNDCId,
			n.FdaAssignedLabelerCode,    
			n.ProductCode,       
			n.PackageCode,     
			s.SubItemIdentifier,     
			i.HistoricalNValue,
			0,
			'N',
			i.PackageDescription,  		      
			i.Generic ,   
			i.TradeName ,     
			i.DiscontinuationDate,                             	             
			i.DiscontinuationEnteredDate,
			i.DateEnteredMarket,
			i.Covered,         
			i.PrimeVendor,
			i.PrimeVendorChangedDate , 
			i.PassThrough,
			i.DispensingUnit ,        
			i.VAClass , 
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
			case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as ItemDualPriceStatus
		from DI_DrugItems i join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
			join DI_DrugItemSubItems s on i.DrugItemId = s.DrugItemId
		where i.ContractId = @ContractId  
			and i.Covered = 'T'  

		union
		
			select @ContractId,  /* without N */
			@ContractNumber,
			@IsBPA,
			@Division,
			@CurrentYearNFAMPTableName,
			i.DrugItemId, 
			null,
			i.DrugItemNDCId,
			n.FdaAssignedLabelerCode,    
			n.ProductCode,       
			n.PackageCode,     
			null,     
			i.HistoricalNValue,
			0,
			'N',
			i.PackageDescription,  		      
			i.Generic ,   
			i.TradeName ,     
			i.DiscontinuationDate,                             	             
			i.DiscontinuationEnteredDate,
			i.DateEnteredMarket,
			i.Covered,         
			i.PrimeVendor,
			i.PrimeVendorChangedDate , 
			i.PassThrough,
			i.DispensingUnit ,        
			i.VAClass , 
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
			case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then 'T' else 'F' end as ItemDualPriceStatus
		from DI_DrugItems i join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
		where i.ContractId = @ContractId  
			and i.Covered = 'T'  
			
		order by n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode, s.SubItemIdentifier
		
		
		select @error = @@error

		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving drug items for report for fss contract ' + @ContractNumber
			goto ERROREXIT
		END

	/* backfill FCP */
	/* without N */
	update #ErroredItemPrices
		set FCP = f.FCP
	from #ErroredItemPrices e left outer join DI_FCP f
		on f.ndc_1 = e.FdaAssignedLabelerCode and f.ndc_2 = e.ProductCode and f.ndc_3 = e.PackageCode
		Join di_yearlookup y
		on f.YearId = y.YearId
	where y.YearValue = @currentYear
		and e.DrugItemSubItemId is null
		and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
		and f.FCP <> 0
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error backfilling FCP without N for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	/* backfill FCP */
	/* with N */
	update #ErroredItemPrices
		set FCP = f.FCP
	from #ErroredItemPrices e left outer join DI_FCP f
		on f.ndc_1 = e.FdaAssignedLabelerCode and f.ndc_2 = e.ProductCode and f.ndc_3 = e.PackageCode and f.n = e.SubItemIdentifier
		Join di_yearlookup y
		on f.YearId = y.YearId
	where y.YearValue = @currentYear
		and e.DrugItemSubItemId is not null
		and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
		and f.FCP <> 0
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error backfilling FCP with N for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END

	update #ErroredItemPrices
	set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'FCP not available for covered item; '
	where FCP is null and Covered = 'T'
	and ( DiscontinuationDate >= @dateWithoutTime or DiscontinuationDate is null )

	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for missing fcp values for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	/* backfill historical FCP */
	/* without N */
	update #ErroredItemPrices
		set MostRecentHistoricalFCP = f.FCP
	from #ErroredItemPrices e left outer join DI_FCP f
		on f.ndc_1 = e.FdaAssignedLabelerCode and f.ndc_2 = e.ProductCode and f.ndc_3 = e.PackageCode
		Join di_yearlookup y
		on f.YearId = y.YearId
	where y.YearValue = @currentYear - 1
		and e.DrugItemSubItemId is null
		and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
		and f.FCP <> 0
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error backfilling historical FCP without N for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	/* backfill historical FCP */
	/* with N */
	update #ErroredItemPrices
		set MostRecentHistoricalFCP = f.FCP
	from #ErroredItemPrices e left outer join DI_FCP f
		on f.ndc_1 = e.FdaAssignedLabelerCode and f.ndc_2 = e.ProductCode and f.ndc_3 = e.PackageCode and f.n = e.SubItemIdentifier
		Join di_yearlookup y
		on f.YearId = y.YearId
	where y.YearValue = @currentYear - 1
		and e.DrugItemSubItemId is not null
		and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
		and f.FCP <> 0
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error backfilling historical FCP with N for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	update #ErroredItemPrices
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Warning: swing in FCP vs historical FCP > ' + convert( nvarchar(12), @PercentageRepresentingPriceSwing * 100 ) + '%; '
	from #ErroredItemPrices e
	where e.FCP >= e.MostRecentHistoricalFCP + ( e.MostRecentHistoricalFCP * @PercentageRepresentingPriceSwing )
	or e.FCP <= e.MostRecentHistoricalFCP - ( e.MostRecentHistoricalFCP * @PercentageRepresentingPriceSwing )
	and e.FCP is not null
	and e.MostRecentHistoricalFCP is not null
	and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for large swing in FCP for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	update #ErroredItemPrices
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Item has no prices; '
	from #ErroredItemPrices e 
	where e.DrugItemSubItemId is null
	and e.Covered = 'T'
	and e.ExcludeFromExport = 0 
	and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
	and not exists
		(
		select p.DrugItemId from DI_DrugItemPrice p
		where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate
		and p.DrugItemId = e.DrugItemId
		and p.DrugItemSubItemId is null
		)

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for missing prices ( without N ) for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	update #ErroredItemPrices
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Item has no prices; '
	from #ErroredItemPrices e 
	where e.DrugItemSubItemId is not null
	and e.Covered = 'T'
	and e.ExcludeFromExport = 0 	
	and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
	and not exists
		(
		select p.DrugItemSubItemId from DI_DrugItemPrice p 
		where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate 
		and p.DrugItemSubItemId = e.DrugItemSubItemId
		and p.DrugItemId = e.DrugItemId
		)
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for missing prices ( with N ) for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
			
	update #ErroredItemPrices
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Item has no FSS price; '
	from #ErroredItemPrices e 
	where e.DrugItemSubItemId is null
	and e.Covered = 'T'
	and e.ExcludeFromExport = 0 
	and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
	and not exists
		(
		select p.DrugItemId from DI_DrugItemPrice p
		where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate
		and p.DrugItemId = e.DrugItemId
		and p.IsFSS = 1
		and p.DrugItemSubItemId is null
		)

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for missing FSS price ( without N ) for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	update #ErroredItemPrices
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Item has no FSS price; '
	from #ErroredItemPrices e 
	where e.DrugItemSubItemId is not null
	and e.Covered = 'T'
	and e.ExcludeFromExport = 0 	
	and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
	and not exists
		(
		select p.DrugItemSubItemId from DI_DrugItemPrice p 
		where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate 
		and p.DrugItemSubItemId = e.DrugItemSubItemId
		and p.DrugItemId = e.DrugItemId
		and p.IsFSS = 1
		)
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for missing FSS price ( with N ) for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END			
			
	/* FET Comparison */
	update #ErroredItemPrices
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Item has price(s) that are less than or equal to the FET; '
	from #ErroredItemPrices e 
	where e.DrugItemSubItemId is null
	and e.Covered = 'T'
	and e.ExcludeFromExport = 0 
	and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
	and exists
		(
		select p.DrugItemPriceId from DI_DrugItemPrice p
		where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate
		and p.DrugItemId = e.DrugItemId		
		and p.DrugItemSubItemId is null
		and p.Price <= e.IncludedFETAmount
		)

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for prices less than or equal to FET ( without N ) for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	update #ErroredItemPrices
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Item has price(s) that are less than or equal to the FET; '
	from #ErroredItemPrices e 
	where e.DrugItemSubItemId is not null
	and e.Covered = 'T'
	and e.ExcludeFromExport = 0 	
	and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
	and exists	
		(
		select p.DrugItemPriceId from DI_DrugItemPrice p 
		where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate 
		and p.DrugItemSubItemId = e.DrugItemSubItemId
		and p.DrugItemId = e.DrugItemId
		and p.Price <= e.IncludedFETAmount
		)
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for prices less than or equal to FET ( with N ) for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END			


	insert into #ErroredItemPrices
		( ContractId,
			NACCMContractNumber,
			DrugItemId,
			FdaAssignedLabelerCode,
			ProductCode,
			PackageCode,
			SubItemIdentifier,
			IsMissingItem,
			IsDiscontinuedItem,
			ErrorDescription )
		select @ContractId,
			f.cnt_no,
			-1,
			f.ndc_1, 
			f.ndc_2, 
			f.ndc_3,
			'',
			1,
			'N',
			'NDC in NFAMP table is missing from this contract; '
		from DI_FCP f Join di_yearlookup y on f.YearId = y.YearId
		where f.cnt_no = @ContractNumber 
		and y.YearValue = @currentYear
		and LTRIM(RTRIM(isnull(f.QA_Exempt,''))) <> 'Y'
		and ( f.n is null or LEN( f.n ) = 0 )
		and f.ndc_1 + f.ndc_2 + f.ndc_3 not in 
		( select e.FdaAssignedLabelerCode + e.ProductCode + e.PackageCode as 'combinedNDC'
			from #ErroredItemPrices e )
						
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting NFAMP items missing from NACCM without N'
		goto ERROREXIT
	END	
		
	insert into #ErroredItemPrices
		( ContractId,
			NACCMContractNumber,
			DrugItemId,
			FdaAssignedLabelerCode,
			ProductCode,
			PackageCode,
			SubItemIdentifier,
			IsMissingItem,
			IsDiscontinuedItem,
			ErrorDescription )
		select @ContractId,
		    f.cnt_no,
		    -1,
			f.ndc_1, 
			f.ndc_2, 
			f.ndc_3,
			f.n,
			1,
			'N',
			'NDC in NFAMP table is missing from this contract; '
		from DI_FCP f Join di_yearlookup y on f.YearId = y.YearId
		where f.cnt_no = @ContractNumber
		and y.YearValue = @currentYear
		and LTRIM(RTRIM(isnull(f.QA_Exempt,''))) <> 'Y'	
		and f.n is not null
		and len(f.n) > 0
		and f.ndc_1 + f.ndc_2 + f.ndc_3 + f.n not in 
		( select e.FdaAssignedLabelerCode + e.ProductCode + e.PackageCode + e.SubItemIdentifier as 'combinedNDC'
			from #ErroredItemPrices e )
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting NFAMP items missing from NACCM with N'
		goto ERROREXIT
	END	
		
	update #ErroredItemPrices
			set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'NDC in NFAMP table is discontinued ' + convert( nvarchar(24), f.disc_date ) + '; ',
			IsDiscontinuedItem = 'Y'
		from #ErroredItemPrices e join DI_FCP f 
		on f.ndc_1 + f.ndc_2 + f.ndc_3 = e.FdaAssignedLabelerCode + e.ProductCode + e.PackageCode
		Join di_yearlookup y on f.YearId = y.YearId
		where e.DrugItemSubItemId is null
		and e.DiscontinuationDate is null 
		and f.disc_date is not null 
		and y.YearValue = @currentYear
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting discontinued NFAMP items not discontinued in NACCM without N'
		goto ERROREXIT
	END	

	update #ErroredItemPrices
			set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'NDC in NFAMP table is discontinued ' + convert( nvarchar(24), f.disc_date ) + '; ',
			IsDiscontinuedItem = 'Y'
		from #ErroredItemPrices e join DI_FCP f
		on f.ndc_1 + f.ndc_2 + f.ndc_3 + f.n = e.FdaAssignedLabelerCode + e.ProductCode + e.PackageCode + e.SubItemIdentifier
		Join di_yearlookup y on f.YearId = y.YearId
		where e.DrugItemSubItemId is not null
		and e.DiscontinuationDate is null 
		and f.disc_date is not null 
		and y.YearValue = @currentYear
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting discontinued NFAMP items not discontinued in NACCM with N'
		goto ERROREXIT
	END	

	update #ErroredItemPrices
			set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'NDC in NFAMP table is marked as non-covered in NACCM; '
		from #ErroredItemPrices e join DI_FCP f
		on f.ndc_1 + f.ndc_2 + f.ndc_3 = e.FdaAssignedLabelerCode + e.ProductCode + e.PackageCode
		Join di_yearlookup y on f.YearId = y.YearId
		where e.DrugItemSubItemId is null
		and e.Covered = 'F'
		and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null ) 
		and y.YearValue = @currentYear
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting NFAMP items marked as non-covered in NACCM without N'
		goto ERROREXIT
	END	

	update #ErroredItemPrices
			set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'NDC in NFAMP table is marked as non-covered in NACCM; '
		from #ErroredItemPrices e join DI_FCP f
		on f.ndc_1 + f.ndc_2 + f.ndc_3 + f.n = e.FdaAssignedLabelerCode + e.ProductCode + e.PackageCode + e.SubItemIdentifier
		Join di_yearlookup y on f.YearId = y.YearId
		where e.DrugItemSubItemId is not null
		and e.Covered = 'F'
		and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )
		and y.YearValue = @currentYear
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting NFAMP items marked as non-covered in NACCM with N'
		goto ERROREXIT
	END	


	/* mailout comparison */
	/*********    PBM discontinued use of mailout table in 2015    *********/
	/*
	update #ErroredItemPrices
		set SingleDualFromMailout = s_d_pricin
	from mailout m
		where #ErroredItemPrices.NACCMContractNumber = m.cnt_no  
		and ( #ErroredItemPrices.DiscontinuationDate >= @dateWithoutTime or #ErroredItemPrices.DiscontinuationDate is null )

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error getting single-dual status from mailout'
		goto ERROREXIT
	END	

	update #ErroredItemPrices 
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Item prime vendor indicator conflicts with mailout; '
	from #ErroredItemPrices e left outer join mailout m on m.cnt_no = e.NACCMContractNumber
	where e.Covered = 'T'
		and e.ExcludeFromExport = 0 
		and ((( m.PV = 'T' or m.PV = 'Y' ) and e.PrimeVendor = 'F' ) or (( m.PV = 'F' or m.PV = 'N' ) and e.PrimeVendor = 'T' ))
		and ( e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null )

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for prime vendor conflict with mailout for report for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	*/

	/* lookahead to pricing errors */
	update #ErroredItemPrices 
	set HasPricingErrors = dbo.DrugItemHasPricingErrorsFunction( @ContractNumber,
													@ContractId,
													@IsBPA,
													@Division,
													e.DrugItemId,
													e.DrugItemNDCId,
													e.FdaAssignedLabelerCode,    
													e.ProductCode,       
													e.PackageCode,     
													@FutureHistoricalSelectionCriteria, 
													e.Covered,
													@CurrentYearNFAMPTableName,   -- passed but not used
													e.SingleDualFromMailout,
													e.ItemDualPriceStatus,
													e.FCP,
													e.IncludedFETAmount,
													e.DrugItemSubItemId )
	from #ErroredItemPrices e
	where e.DiscontinuationDate >= @dateWithoutTime or e.DiscontinuationDate is null 

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error calling DrugItemHasPricingErrorsFunction() for fss contract ' + @ContractNumber
		goto ERROREXIT
	END

	select 	ContractId, 
			NACCMContractNumber, 
			IsBPA,
			Division,
			CurrentYearNFAMPTableName,
			DrugItemId,  
			DrugItemSubItemId,
			DrugItemNDCId  ,
			FdaAssignedLabelerCode ,
			ProductCode ,
			PackageCode ,
			SubItemIdentifier,
			HistoricalNValue ,
			PackageDescription ,
			Generic ,
			TradeName  ,
			DiscontinuationDate  ,
			DiscontinuationEnteredDate  ,
			DateEnteredMarket  ,
			Covered  ,
			PrimeVendor  ,
			PrimeVendorChangedDate ,
			PassThrough  ,
			DispensingUnit,
			VAClass   ,
			ExcludeFromExport ,
			NonTAA,
			IncludedFETAmount,
			ParentDrugItemId  ,
			LastModificationType ,
			ModificationStatusId ,
			CreatedBy  ,
			CreationDate ,
			LastModifiedBy,
			LastModificationDate,
			FCP,
			SingleDualFromMailout,
			ItemDualPriceStatus,
			HasPricingErrors,
			ErrorDescription
		from #ErroredItemPrices
		where ( ( ErrorDescription is not null and LEN( LTRIM( RTRIM( ErrorDescription ))) > 0 ) 
				and ( IsMissingItem = 0 or IsDiscontinuedItem = 'N' ))      -- eliminates both missing and disc which is ok
			or HasPricingErrors = 1
			and ( DiscontinuationDate >= @dateWithoutTime or DiscontinuationDate is null )

		order by FdaAssignedLabelerCode, ProductCode, PackageCode, SubItemIdentifier

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

ENDEXIT:






	





