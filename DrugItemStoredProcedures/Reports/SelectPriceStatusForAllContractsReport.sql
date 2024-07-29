IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectPriceStatusForAllContractsReport')
	BEGIN
		DROP  Procedure  SelectPriceStatusForAllContractsReport
	END

GO

CREATE Procedure [dbo].[SelectPriceStatusForAllContractsReport]
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@FutureHistoricalSelectionCriteria nchar(1)  -- F future, A active
)
with recompile
AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@currentYear int,
	@query nvarchar(1200),
	@activeContractTestDate datetime,
	@fcpComparisonTestDate datetime,
	@QuarterIdForTestDate int,
	@IFF numeric(18,4),  -- the IFF used for the purpose of this report is the IFF assigned to schedule 1, which is the pharma schedule 65IB, for the current quarter
	@SQL nvarchar(2400),
	@SQLParms nvarchar(1000)

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertDrugItemUserActivity  @ReportUserLoginId, 'R', 'Select Price Status For All Contracts Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @currentYear = year(getdate())
	if( @FutureHistoricalSelectionCriteria = 'F' )
	BEGIN
		select @currentYear = @currentYear + 1
		
		select @activeContractTestDate = convert( datetime, '1/1/' + convert(nvarchar(4), @currentYear ) )
		select @fcpComparisonTestDate = convert( datetime, '1/1/' + convert(nvarchar(4), @currentYear ) )
	END
	else
	BEGIN
		select @activeContractTestDate = getdate()
		select @fcpComparisonTestDate = getdate()
	END

	/* get IFF for use with price vs FCP comparisons */
	select @SQL = N'select @QuarterIdForTestDate_parm = Quarter_ID from [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].dbo.tlkup_year_qtr where @activeContractTestDate_parm between Start_Date and End_Date'
	
	select @SQLParms = N'@activeContractTestDate_parm datetime, @QuarterIdForTestDate_parm int OUTPUT'

	Exec SP_executeSQL @SQL, @SQLParms, @activeContractTestDate_parm = @activeContractTestDate, @QuarterIdForTestDate_parm = @QuarterIdForTestDate OUTPUT

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving quarter id for test date.' 
		goto ERROREXIT
	END


	select @SQL = N'select @IFF_parm = VA_IFF from [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].dbo.tbl_IFF where Schedule_Number = 1 and @QuarterIdForTestDate_parm between Start_Quarter_Id and End_Quarter_Id'

	select @SQLParms = N'@QuarterIdForTestDate_parm int, @IFF_parm numeric(18,4) OUTPUT'

	Exec SP_executeSQL @SQL, @SQLParms, @QuarterIdForTestDate_parm = @QuarterIdForTestDate, @IFF_parm = @IFF OUTPUT

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving IFF for quarter id ( and schedule number = 1 ).' 
		goto ERROREXIT
	END


	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#ActiveContractCounts')
	BEGIN
		delete #ActiveContractCounts
	END
	
	create table #ActiveContractCounts 
	(
		ContractId int NOT NULL, 
		NACCMContractNumber nvarchar(20) NOT NULL, 
		NValueCount int NULL,
		LinkedButNotDiscontinued bit NULL,
		CoveredItemCountWithoutN int NULL, 
		CoveredItemCountWithN int NULL,
		CoveredItemCount int NULL,
		ValidFCPCountWithoutN int NULL,
		ValidFCPCountWithN int NULL,
		ValidFCPCount int NULL, 
		VendorName nvarchar(75) NULL,
		ContractExpirationDate datetime NULL,
		ContractCompletionDate datetime NULL,
		ItemsWithFETValues int NULL,
		ItemsWithNoPricesCountWithoutN int NULL,
		ItemsWithNoPricesCountWithN int NULL,
		ItemsWithNoPricesCount int NULL,
		ItemsWithNoFSSPricesCountWithoutN int NULL,
		ItemsWithNoFSSPricesCountWithN int NULL,
		ItemsWithNoFSSPricesCount int NULL,
		ItemsWithDualPricesCountWithoutN int NULL,
		ItemsWithDualPricesCountWithN int NULL,
		ItemsWithDualPricesCount int NULL,
		ItemsWithFSSGTFCPWithoutN int NULL,
		ItemsWithFSSGTFCPWithN int NULL,
		ItemsWithFSSGTFCP int NULL,
		ItemsWithBIG4GTFCPWithoutN int NULL,
		ItemsWithBIG4GTFCPWithN int NULL,
		ItemsWithBIG4GTFCP int NULL,
		ItemsWithPricesThatAreZero int NULL,
		ItemsWithPricesThatAreLTFET int NULL,
		ItemsWithIdenticalFSSAndBIG4PricesWithoutN int NULL,
		ItemsWithIdenticalFSSAndBIG4PricesWithN int NULL,
		ItemsWithIdenticalFSSAndBIG4Prices int NULL,
		DualPriceItemsWithoutFSSPriceCountWithoutN int NULL,
		DualPriceItemsWithoutFSSPriceCountWithN int NULL,
		DualPriceItemsWithoutFSSPriceCount int NULL,
		MissingKnownNFAMPItemCountWithoutN int NULL,
		MissingKnownNFAMPItemCountWithN int NULL,
		MissingKnownNFAMPItemCount int NULL,
		DiscontinuedInNFAMPButNotInNACCMCountWithoutN int NULL,
		DiscontinuedInNFAMPButNotInNACCMCountWithN int NULL,
		DiscontinuedInNFAMPButNotInNACCMCount int NULL,
		ItemsInNFAMPMarkedAsNonCoveredWithoutN int NULL,
		ItemsInNFAMPMarkedAsNonCoveredWithN int NULL,
		ItemsInNFAMPMarkedAsNonCovered int NULL,
		SingleDualFromMailout nchar(1) NULL,
		VendorIsIncludedInMailout bit NULL,
		CountOfPrimeVendorItems int NULL,
		PrimeVendorIndicatedInMailout bit NULL
	)

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table'
		goto ERROREXIT
	END

	/* get all active contracts */
	insert into #ActiveContractCounts
		( ContractId, NACCMContractNumber )
	select c.ContractId, c.NACCMContractNumber
		from DI_Contracts c
		where dbo.IsContractActiveFunction( c.NACCMContractNumber, @activeContractTestDate ) = 1
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving active contract list'
		goto ERROREXIT
	END

	select @SQL = N'update #ActiveContractCounts 
		set VendorName = c.Contractor_Name,
		ContractExpirationDate = c.Dates_CntrctExp,
		ContractCompletionDate = c.Dates_Completion 
		from [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].dbo.tbl_Cntrcts c,
				#ActiveContractCounts a
		where c.CntrctNum = a.NACCMContractNumber' 
		
	Exec SP_executeSQL @SQL
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting contract vendor name'
		goto ERROREXIT
	END
	
	update #ActiveContractCounts
		set NValueCount = ( select count(*)
			from DI_DrugItems d 
			where d.ContractId = c.ContractId
			and d.Covered = 'T'
			and d.ExcludeFromExport = 0 
			and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) 
			and d.DrugItemId in ( select DrugItemId from DI_DrugItemSubItems )) 
	from #ActiveContractCounts c	
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error detecting N values'
		goto ERROREXIT
	END			
	
	/* items with FET values */
	update #ActiveContractCounts
		set ItemsWithFETValues = ( select count(*)
			from DI_DrugItems d 
			where d.ContractId = c.ContractId
			and d.Covered = 'T'
			and d.ExcludeFromExport = 0 
			and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) 
			and d.IncludedFETAmount <> 0 )
	from #ActiveContractCounts c	
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error detecting FET values'
		goto ERROREXIT
	END			

	/*LinkedButNotDiscontinued*/
	update #ActiveContractCounts 
		set LinkedButNotDiscontinued = case when exists ( select n.OldContractId
												from DI_ContractNDCNumberChange n 
												where n.OldContractId = c.ContractId
												and n.ChangeStatus = 'C' ) then 1 else 0 end
	from #ActiveContractCounts c	
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error checking LinkedButNotDiscontinued'
		goto ERROREXIT
	END			
	
	update #ActiveContractCounts
		set CoveredItemCountWithoutN = ( select count(*) 
								from DI_DrugItems d
								where d.ContractId = c.ContractId
								and d.Covered = 'T'
								and d.ExcludeFromExport = 0 
								and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) 
								and d.DrugItemId not in ( select DrugItemId from DI_DrugItemSubItems ))
								
		from #ActiveContractCounts c
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting covered items without N'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
		set CoveredItemCountWithN = ( select count(*) 
							from DI_DrugItems d join DI_DrugItemSubItems s on d.DrugItemId = s.DrugItemId
							where d.ContractId = c.ContractId
							and d.Covered = 'T'
							and d.ExcludeFromExport = 0 
							and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) )
							
							
		from #ActiveContractCounts c
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting covered items with N'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
	set CoveredItemCount = CoveredItemCountWithoutN + CoveredItemCountWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of covered items'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
		set ValidFCPCountWithoutN = ( Select count(*)
							from DI_FCP f
							Join di_yearlookup y
							on f.YearId = y.YearId
							join DI_DrugItemNDC n
							on f.ndc_1 = n.FdaAssignedLabelerCode and f.ndc_2 = n.ProductCode and f.ndc_3 = n.PackageCode
							join DI_DrugItems i
							on n.DrugItemNDCId = i.DrugItemNDCId
							where y.YearValue = @currentYear
							and f.n is null
							and i.ContractId = c.ContractId
							and f.ContractId = c.ContractId
							and f.FCP is not null 
							and f.FCP <> 0
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ))
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting fcp values without N'
		goto ERROREXIT
	END							
	
	update #ActiveContractCounts
		set ValidFCPCountWithN = ( Select count(*)
							From DI_FCP f
							Join di_yearlookup y
							on f.YearId = y.YearId
							join DI_DrugItemNDC n
							on f.ndc_1 = n.FdaAssignedLabelerCode and f.ndc_2 = n.ProductCode and f.ndc_3 = n.PackageCode
							join DI_DrugItems i
							on n.DrugItemNDCId = i.DrugItemNDCId
							join DI_DrugItemSubItems s on i.DrugItemId = s.DrugItemId
							where y.YearValue = @currentYear
							and f.n is not null
							and s.SubItemIdentifier = f.n
							and i.ContractId = c.ContractId
							and f.ContractId = c.ContractId
							and f.FCP is not null 
							and f.FCP <> 0
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ))
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting fcp values with N'
		goto ERROREXIT
	END							
	
	update #ActiveContractCounts
	set ValidFCPCount = ValidFCPCountWithoutN + ValidFCPCountWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of valid fcps'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
		set ItemsWithNoPricesCountWithoutN = ( 
	
									select COUNT(*) from DI_DrugItems d 
									where d.ContractId = c.ContractId
									and d.Covered = 'T'
									and d.ExcludeFromExport = 0
									and ( d.DiscontinuationDate >= GETDATE() or d.DiscontinuationDate is null )
									and d.DrugItemId not in ( select DrugItemId from DI_DrugItemSubItems )
									and d.DrugItemId not in
									(
									select p.DrugItemId from DI_DrugItemPrice p 
									where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate 
									and p.DrugItemId in 
									( 
									select x.DrugItemId from DI_DrugItems x
										where x.ContractId = c.ContractId
									) 
									)	
	
								)
		from #ActiveContractCounts c
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items without N with no prices'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
		set ItemsWithNoPricesCountWithN = ( 
		
									select COUNT(*) from DI_DrugItemSubItems s join DI_DrugItems d on s.DrugItemId = d.DrugItemId
									where d.ContractId = c.ContractId
									and d.Covered = 'T'
									and d.ExcludeFromExport = 0
									and ( d.DiscontinuationDate >= GETDATE() or d.DiscontinuationDate is null )
									and s.DrugItemSubItemId not in
									(
									select p.DrugItemSubItemId from DI_DrugItemPrice p 
									where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate 
									and p.DrugItemSubItemId in 
									( 
									select s.DrugItemSubItemId from DI_DrugItemSubItems s join DI_DrugItems d on s.DrugItemId = d.DrugItemId
									where d.ContractId = c.ContractId
									) 
									)
								
								)
		from #ActiveContractCounts c
		
		
		
/*

select COUNT(*) from DI_DrugItems d 
where d.ContractId = 327
and d.Covered = 'T'
and d.ExcludeFromExport = 0
and ( d.DiscontinuationDate >= GETDATE() or d.DiscontinuationDate is null )
and d.DrugItemId not in ( select DrugItemId from DI_DrugItemSubItems )
and d.DrugItemId not in
(
select p.DrugItemId from DI_DrugItemPrice p 
where '1/1/2011' between p.PriceStartDate and p.PriceStopDate 
and p.DrugItemId in 
( 
select x.DrugItemId from DI_DrugItems x
	where x.ContractId = 327
) 
)


select COUNT(*) from DI_DrugItemSubItems s join DI_DrugItems d on s.DrugItemId = d.DrugItemId
where d.ContractId = 327
and d.Covered = 'T'
and d.ExcludeFromExport = 0
and ( d.DiscontinuationDate >= GETDATE() or d.DiscontinuationDate is null )
and s.DrugItemSubItemId not in
(
select p.DrugItemSubItemId from DI_DrugItemPrice p 
where '1/1/2011' between p.PriceStartDate and p.PriceStopDate 
and p.DrugItemSubItemId in 
( 
select s.DrugItemSubItemId from DI_DrugItemSubItems s join DI_DrugItems d on s.DrugItemId = d.DrugItemId
where d.ContractId = 327
) 
)
*/		
		
		
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items with no prices'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
	set ItemsWithNoPricesCount = ItemsWithNoPricesCountWithoutN + ItemsWithNoPricesCountWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of items without prices'
		goto ERROREXIT
	END		
	
		update #ActiveContractCounts
		set ItemsWithNoFSSPricesCountWithoutN = ( 
	
									select COUNT(*) from DI_DrugItems d 
									where d.ContractId = c.ContractId
									and d.Covered = 'T'
									and d.ExcludeFromExport = 0
									and ( d.DiscontinuationDate >= GETDATE() or d.DiscontinuationDate is null )
									and d.DrugItemId not in ( select DrugItemId from DI_DrugItemSubItems )
									and d.DrugItemId not in
									(
									select p.DrugItemId from DI_DrugItemPrice p 
									where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate 
									and p.IsFSS = 1
									and p.DrugItemId in 
									( 
									select x.DrugItemId from DI_DrugItems x
										where x.ContractId = c.ContractId
									) 
									)	
	
								)
		from #ActiveContractCounts c
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items without N with no FSS prices'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
		set ItemsWithNoFSSPricesCountWithN = ( 
		
									select COUNT(*) from DI_DrugItemSubItems s join DI_DrugItems d on s.DrugItemId = d.DrugItemId
									where d.ContractId = c.ContractId
									and d.Covered = 'T'
									and d.ExcludeFromExport = 0
									and ( d.DiscontinuationDate >= GETDATE() or d.DiscontinuationDate is null )
									and s.DrugItemSubItemId not in
									(
									select p.DrugItemSubItemId from DI_DrugItemPrice p 
									where @activeContractTestDate between p.PriceStartDate and p.PriceStopDate 
									and p.IsFSS = 1
									and p.DrugItemSubItemId in 
									( 
									select s.DrugItemSubItemId from DI_DrugItemSubItems s join DI_DrugItems d on s.DrugItemId = d.DrugItemId
									where d.ContractId = c.ContractId
									) 
									)
								
								)
		from #ActiveContractCounts c
				
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items with no FSS prices'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
	set ItemsWithNoFSSPricesCount = ItemsWithNoFSSPricesCountWithoutN + ItemsWithNoFSSPricesCountWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of items without FSS prices'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
		set ItemsWithDualPricesCountWithoutN = ( select count(*) 
								from DI_DrugItems d 
								where d.ContractId = c.ContractId
								and d.Covered = 'T'
								and d.ExcludeFromExport = 0 
								and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) 
								and d.DrugItemId not in ( select DrugItemId from DI_DrugItemSubItems )
								and dbo.GetItemDualPriceStatusForDrugItemId( d.DrugItemId ) = 1
								)
		from #ActiveContractCounts c
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items without N with dual prices'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
		set ItemsWithDualPricesCountWithN = ( select count(*) 
								from DI_DrugItems d join DI_DrugItemSubItems s on d.DrugItemId = s.DrugItemId
								where d.ContractId = c.ContractId
								and d.Covered = 'T'
								and d.ExcludeFromExport = 0 
								and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) 
								and dbo.GetItemDualPriceStatusForDrugItemSubItemIdFunction( s.DrugItemSubItemId ) = 1
								)
		from #ActiveContractCounts c
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items with N with dual prices'
		goto ERROREXIT
	END			
	
	update #ActiveContractCounts
	set ItemsWithDualPricesCount = ItemsWithDualPricesCountWithoutN + ItemsWithDualPricesCountWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of items with dual prices'
		goto ERROREXIT
	END		
	
	
	update #ActiveContractCounts
		set ItemsWithFSSGTFCPWithoutN = ( Select count(*)
							From DI_FCP f
							Join di_yearlookup y
							on f.YearId = y.YearId
							join DI_DrugItemNDC n
							on f.ndc_1 = n.FdaAssignedLabelerCode and f.ndc_2 = n.ProductCode and f.ndc_3 = n.PackageCode
							join DI_DrugItems i
							on n.DrugItemNDCId = i.DrugItemNDCId
							join DI_DrugItemPrice p 
							on i.DrugItemId = p.DrugItemId
							where y.YearValue = @currentYear
							and i.ContractId = c.ContractId
							and f.FCP is not null 
							and f.FCP <> 0
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )
							and i.DrugItemId not in ( select DrugItemId from DI_DrugItemSubItems )
							and f.n is null
							and  dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) = 0
							and p.IsFSS = 1
							and @fcpComparisonTestDate between p.PriceStartDate and p.PriceStopDate
							and convert( decimal(9,2), ( p.Price - ( p.Price * @IFF ))) > f.FCP ) 
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting FSS prices without N GT FCP'
		goto ERROREXIT
	END							
	
	update #ActiveContractCounts
		set ItemsWithFSSGTFCPWithN = ( Select count(*)
							From DI_FCP f
							Join di_yearlookup y
							on f.YearId = y.YearId
							join DI_DrugItemNDC n
							on f.ndc_1 = n.FdaAssignedLabelerCode and f.ndc_2 = n.ProductCode and f.ndc_3 = n.PackageCode
							join DI_DrugItems i
							on n.DrugItemNDCId = i.DrugItemNDCId
							join DI_DrugItemSubItems s
							on s.DrugItemId = i.DrugItemId
							join DI_DrugItemPrice p 
							on i.DrugItemId = p.DrugItemId
							where y.YearValue = @currentYear
							and i.ContractId = c.ContractId
							and f.FCP is not null 
							and f.FCP <> 0
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )
							and f.n is not null
							and f.n = s.SubItemIdentifier
							and p.DrugItemSubItemId = s.DrugItemSubItemId
							and  dbo.GetItemDualPriceStatusForDrugItemSubItemIdFunction( s.DrugItemSubItemId ) = 0
							and p.IsFSS = 1
							and @fcpComparisonTestDate between p.PriceStartDate and p.PriceStopDate
							and convert( decimal(9,2), ( p.Price - ( p.Price * @IFF ))) > f.FCP ) 
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting FSS prices with N GT FCP'
		goto ERROREXIT
	END							
	
	update #ActiveContractCounts
	set ItemsWithFSSGTFCP = ItemsWithFSSGTFCPWithoutN + ItemsWithFSSGTFCPWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of items with FSS prices GT FCP'
		goto ERROREXIT
	END		
	
	
	update #ActiveContractCounts
		set ItemsWithBIG4GTFCPWithoutN = ( Select count(*)
							From DI_FCP f
							Join di_yearlookup y
							on f.YearId = y.YearId
							join DI_DrugItemNDC n
							on f.ndc_1 = n.FdaAssignedLabelerCode and f.ndc_2 = n.ProductCode and f.ndc_3 = n.PackageCode
							join DI_DrugItems i
							on n.DrugItemNDCId = i.DrugItemNDCId
							join DI_DrugItemPrice p 
							on i.DrugItemId = p.DrugItemId
							where y.YearValue = @currentYear
							and i.ContractId = c.ContractId
							and f.FCP is not null 
							and f.FCP <> 0
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )
							and i.DrugItemId not in ( select DrugItemId from DI_DrugItemSubItems )
							and f.n is null
							and  dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) = 1
							and p.IsBIG4 = 1
							and @fcpComparisonTestDate between p.PriceStartDate and p.PriceStopDate
						and convert( decimal(9,2), ( p.Price - ( p.Price * @IFF ))) > f.FCP ) 
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting BIG4 prices without N GT FCP'
		goto ERROREXIT
	END							
	

		update #ActiveContractCounts
		set ItemsWithBIG4GTFCPWithN = ( Select count(*)
							From DI_FCP f
							Join di_yearlookup y
							on f.YearId = y.YearId
							join DI_DrugItemNDC n
							on f.ndc_1 = n.FdaAssignedLabelerCode and f.ndc_2 = n.ProductCode and f.ndc_3 = n.PackageCode
							join DI_DrugItems i
							on n.DrugItemNDCId = i.DrugItemNDCId
							join DI_DrugItemSubItems s
							on s.DrugItemId = i.DrugItemId
							join DI_DrugItemPrice p 
							on i.DrugItemId = p.DrugItemId
							where y.YearValue = @currentYear
							and i.ContractId = c.ContractId
							and f.FCP is not null 
							and f.FCP <> 0
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )
							and f.n is not null
							and f.n = s.SubItemIdentifier
							and p.DrugItemSubItemId = s.DrugItemSubItemId
							and dbo.GetItemDualPriceStatusForDrugItemSubItemIdFunction( s.DrugItemSubItemId ) = 1
							and p.IsBIG4 = 1
							and @fcpComparisonTestDate between p.PriceStartDate and p.PriceStopDate
							and convert( decimal(9,2), ( p.Price - ( p.Price * @IFF ))) > f.FCP ) 
							
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting BIG4 prices with N GT FCP'
		goto ERROREXIT
	END							
	
	update #ActiveContractCounts
	set ItemsWithBIG4GTFCP = ItemsWithBIG4GTFCPWithoutN + ItemsWithBIG4GTFCPWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of items with BIG4 prices GT FCP'
		goto ERROREXIT
	END		
	

	update #ActiveContractCounts
		set ItemsWithPricesThatAreZero = ( Select count(*) from ( select distinct i.DrugItemId
							from DI_DrugItems i join DI_DrugItemPrice p 
							on i.DrugItemId = p.DrugItemId
							where i.ContractId = c.ContractId
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )
							and p.Price = 0 ) d )
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items with prices that are zero'
		goto ERROREXIT
	END							
	
	/* price <= FET */
	update #ActiveContractCounts
		set ItemsWithPricesThatAreLTFET = ( Select count(*) from ( select distinct i.DrugItemId
							from DI_DrugItems i join DI_DrugItemPrice p 
							on i.DrugItemId = p.DrugItemId
							where i.ContractId = c.ContractId
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )
							and p.Price <= i.IncludedFETAmount
							and i.IncludedFETAmount > 0 ) d )
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items with prices that are less than or equal to FET'
		goto ERROREXIT
	END				

	update #ActiveContractCounts
		set ItemsWithIdenticalFSSAndBIG4PricesWithoutN = ( Select count(*)
							from DI_DrugItems i join DI_DrugItemPrice f 
							on i.DrugItemId = f.DrugItemId
							join DI_DrugItemPrice b
							on i.DrugItemId = b.DrugItemId
							where i.ContractId = c.ContractId
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )
							and f.IsFSS = 1
							and @activeContractTestDate between f.PriceStartDate and f.PriceStopDate
							and f.DrugItemSubItemId is null
							and b.IsBIG4 = 1
							and @activeContractTestDate between b.PriceStartDate and b.PriceStopDate
							and b.DrugItemSubItemId is null
							and f.Price = b.Price )
	from #ActiveContractCounts c		
			
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items that have identical FSS and BIG4 prices without N'
		goto ERROREXIT
	END							
	
	update #ActiveContractCounts
		set ItemsWithIdenticalFSSAndBIG4PricesWithN = ( Select count(*)
							from DI_DrugItems i join DI_DrugItemSubItems s
							on i.DrugItemId = s.DrugItemId						
							join DI_DrugItemPrice f 
							on i.DrugItemId = f.DrugItemId
							join DI_DrugItemPrice b
							on i.DrugItemId = b.DrugItemId
							where i.ContractId = c.ContractId
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )
							and f.IsFSS = 1
							and @activeContractTestDate between f.PriceStartDate and f.PriceStopDate
							and b.IsBIG4 = 1
							and @activeContractTestDate between b.PriceStartDate and b.PriceStopDate
							and b.DrugItemSubItemId is not null
							and f.DrugItemSubItemId is not null
							and b.DrugItemSubItemId = f.DrugItemSubItemId
							and f.Price = b.Price )
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items that have identical FSS and BIG4 prices with N'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
	set ItemsWithIdenticalFSSAndBIG4Prices = ItemsWithIdenticalFSSAndBIG4PricesWithoutN + ItemsWithIdenticalFSSAndBIG4PricesWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of items that have identical FSS and BIG4 prices'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
		set DualPriceItemsWithoutFSSPriceCountWithoutN = ( select count(*) 
				from DI_DrugItems d 
				where d.ContractId = c.ContractId
				and d.Covered = 'T'
				and d.ExcludeFromExport = 0 
				and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) 
				and d.DrugItemId not in ( select DrugItemId from DI_DrugItemSubItems )
				and dbo.GetItemDualPriceStatusForDrugItemId( d.DrugItemId ) = 1
				and not exists ( select p.Price from DI_DrugItemPrice p
								where p.IsFSS = 1
								and @activeContractTestDate between p.PriceStartDate and p.PriceStopDate
								and p.DrugItemId = d.DrugItemId )
								)
		from #ActiveContractCounts c
		
	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting dual price items without N value that are missing an FSS price'
		goto ERROREXIT
	END	
	
	update #ActiveContractCounts
		set DualPriceItemsWithoutFSSPriceCountWithN = ( select count(*) 
				from DI_DrugItems d join DI_DrugItemSubItems s
				on d.DrugItemId = s.DrugItemId
				where d.ContractId = c.ContractId
				and d.Covered = 'T'
				and d.ExcludeFromExport = 0 
				and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) 
				and d.DrugItemId not in ( select DrugItemId from DI_DrugItemSubItems )
				and dbo.GetItemDualPriceStatusForDrugItemId( d.DrugItemId ) = 1
				and not exists ( select p.Price from DI_DrugItemPrice p
								where p.IsFSS = 1
								and @activeContractTestDate between p.PriceStartDate and p.PriceStopDate
								and p.DrugItemId = d.DrugItemId
								and p.DrugItemSubItemId = s.DrugItemSubItemId )
								)
		from #ActiveContractCounts c
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting dual price items with N value that are missing an FSS price'
		goto ERROREXIT
	END	

	update #ActiveContractCounts
	set DualPriceItemsWithoutFSSPriceCount = DualPriceItemsWithoutFSSPriceCountWithoutN + DualPriceItemsWithoutFSSPriceCountWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of dual price items that are missing an FSS price'
		goto ERROREXIT
	END		

	/* Comparisons to NFAMP table */
					
	update #ActiveContractCounts
	set MissingKnownNFAMPItemCountWithoutN = 
	( select count(*) 						
		from DI_FCP f Join di_yearlookup y on f.YearId = y.YearId
		where f.cnt_no = c.NACCMContractNumber
		and y.YearValue = @currentYear		
		and LTRIM(RTRIM(isnull(f.QA_Exempt,''))) <> 'Y'
		and ( f.n is null or LEN( f.n ) = 0 )
		and f.ndc_1 + f.ndc_2 + f.ndc_3 not in 
		( select n.FdaAssignedLabelerCode + n.ProductCode + n.PackageCode as 'combinedNDC'
		from DI_DrugItemNDC n join DI_DrugItems d on n.DrugItemNDCId = d.DrugItemNDCId
		where d.ContractId = c.ContractId ))
	from #ActiveContractCounts c
					
	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting NFAMP items missing from NACCM without N'
		goto ERROREXIT
	END	
			
	update #ActiveContractCounts
	set MissingKnownNFAMPItemCountWithN = 
	( select count(*) 						
		from DI_FCP f Join di_yearlookup y on f.YearId = y.YearId
		where f.cnt_no = c.NACCMContractNumber
		and y.YearValue = @currentYear		
		and LTRIM(RTRIM(isnull(f.QA_Exempt,''))) <> 'Y'
		and f.n is not null and LEN( f.n ) > 0 
		and f.ndc_1 + f.ndc_2 + f.ndc_3 + f.n not in 
		( select n.FdaAssignedLabelerCode + n.ProductCode + n.PackageCode + s.SubItemIdentifier as 'combinedNDC'
		from DI_DrugItemNDC n join DI_DrugItems d on n.DrugItemNDCId = d.DrugItemNDCId
		join DI_DrugItemSubItems s on d.DrugItemId = s.DrugItemId
		where d.ContractId = c.ContractId ))
	from #ActiveContractCounts c
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting NFAMP items missing from NACCM with N'
		goto ERROREXIT
	END	
		
		
	update #ActiveContractCounts
	set MissingKnownNFAMPItemCount = MissingKnownNFAMPItemCountWithoutN + MissingKnownNFAMPItemCountWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of NFAMP items missing from NACCM'
		goto ERROREXIT
	END		
					
	update #ActiveContractCounts
	set DiscontinuedInNFAMPButNotInNACCMCountWithoutN = 
	( select count(*) 						
		from DI_FCP f Join di_yearlookup y on f.YearId = y.YearId
		where f.cnt_no = c.NACCMContractNumber
		and y.YearValue = @currentYear		
		and ( f.n is null or LEN( f.n ) = 0 )
		and f.ndc_1 + f.ndc_2 + f.ndc_3 in 
			( select n.FdaAssignedLabelerCode + n.ProductCode + n.PackageCode as 'combinedNDC'
			from DI_DrugItemNDC n join DI_DrugItems d on n.DrugItemNDCId = d.DrugItemNDCId
			where d.ContractId = c.ContractId
			and d.DiscontinuationDate is null )
		and f.disc_date is not null )
	from #ActiveContractCounts c
					
	select @error = @@error
		
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting discontinued NFAMP items not discontinued in NACCM ( without N )'
		goto ERROREXIT
	END	
	
	update #ActiveContractCounts
	set DiscontinuedInNFAMPButNotInNACCMCountWithN = 
	( select count(*) 						
		from DI_FCP f Join di_yearlookup y on f.YearId = y.YearId
		where f.cnt_no = c.NACCMContractNumber
		and y.YearValue = @currentYear		
		and f.n is not null and LEN( f.n ) > 0 
		and f.ndc_1 + f.ndc_2 + f.ndc_3 + f.n in 
			( select n.FdaAssignedLabelerCode + n.ProductCode + n.PackageCode + s.SubItemIdentifier as 'combinedNDC'
			from DI_DrugItemNDC n join DI_DrugItems d on n.DrugItemNDCId = d.DrugItemNDCId
			join DI_DrugItemSubItems s on d.DrugItemId = s.DrugItemId
			where d.ContractId = c.ContractId
			and d.DiscontinuationDate is null )
		and f.disc_date is not null )
	from #ActiveContractCounts c
						
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting discontinued NFAMP items not discontinued in NACCM ( with N )'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
	set DiscontinuedInNFAMPButNotInNACCMCount = DiscontinuedInNFAMPButNotInNACCMCountWithoutN + DiscontinuedInNFAMPButNotInNACCMCountWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of discontinued NFAMP items not discontinued in NACCM'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
	set ItemsInNFAMPMarkedAsNonCoveredWithoutN = 
	( select count(*)  						
		from DI_FCP f Join di_yearlookup y on f.YearId = y.YearId
		where f.cnt_no = c.NACCMContractNumber
		and y.YearValue = @currentYear	
		and ( f.n is null or LEN( f.n ) = 0 )
		and f.ndc_1 + f.ndc_2 + f.ndc_3 in 
			( select n.FdaAssignedLabelerCode + n.ProductCode + n.PackageCode as 'combinedNDC'
			from DI_DrugItemNDC n join DI_DrugItems d on n.DrugItemNDCId = d.DrugItemNDCId
			where d.ContractId = c.ContractId
			and d.Covered = 'F' ))
	from #ActiveContractCounts c
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting NFAMP items marked as non-covered in NACCM ( without N )'
		goto ERROREXIT
	END	
	
	update #ActiveContractCounts
	set ItemsInNFAMPMarkedAsNonCoveredWithN = 
	( select count(*)  						
		from DI_FCP f Join di_yearlookup y on f.YearId = y.YearId
		where f.cnt_no = c.NACCMContractNumber
		and y.YearValue = @currentYear		
		and f.n is not null and LEN( f.n ) > 0 
		and f.ndc_1 + f.ndc_2 + f.ndc_3 + f.n in 
		( select n.FdaAssignedLabelerCode + n.ProductCode + n.PackageCode + s.SubItemIdentifier as 'combinedNDC'
		from DI_DrugItemNDC n join DI_DrugItems d on n.DrugItemNDCId = d.DrugItemNDCId
		join DI_DrugItemSubItems s on d.DrugItemId = s.DrugItemId
		where d.ContractId = c.ContractId
		and d.Covered = 'F' ))
	from #ActiveContractCounts c

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting NFAMP items marked as non-covered in NACCM ( with N )'
		goto ERROREXIT
	END	
	
	update #ActiveContractCounts
	set ItemsInNFAMPMarkedAsNonCovered = ItemsInNFAMPMarkedAsNonCoveredWithoutN + ItemsInNFAMPMarkedAsNonCoveredWithN
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding count of NFAMP items marked as non-covered in NACCM'
		goto ERROREXIT
	END		
	
	/* Comparisons to mailout table */ 
	/*********    PBM discontinued use of mailout table in 2015    *********/
	/*
	update #ActiveContractCounts
		set SingleDualFromMailout = s_d_pricin
		from mailout m
		where #ActiveContractCounts.NACCMContractNumber = m.cnt_no 
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error getting single-dual status from mailout'
		goto ERROREXIT
	END	
	
	update #ActiveContractCounts 
		set VendorIsIncludedInMailout = ( case when exists ( select m.cnt_no from mailout m
																where m.cnt_no = c.NACCMContractNumber ) then 1 else 0 end )																
	from #ActiveContractCounts c
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error checking existence in mailout'
		goto ERROREXIT
	END	

	update #ActiveContractCounts 
		set PrimeVendorIndicatedInMailout = ( case when exists ( select m.cnt_no from mailout m
																where m.cnt_no = c.NACCMContractNumber
																and ( m.PV = 'T' or m.PV = 'Y' )) then 1 else 0 end )																
	from #ActiveContractCounts c
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error checking prime vendor indicator in mailout'
		goto ERROREXIT
	END	
	
	--update #ActiveContractCounts	
	--set PrimeVendorIndicatorConflictWithMailout = 
	--case when exists 
	--( select i.PrimeVendor 
	--from DI_DrugItems i join #ActiveContractCounts c
	--	on i.ContractId = c.ContractId
	--join mailout m
	--	on RIGHT( RTRIM( m.cnt_no ), 5 ) = c.NACCMContractNumber
	--where ( i.PrimeVendor = 'T' and m.pv = 'F' ) or ( i.PrimeVendor = 'F' and m.pv = 'T' )) then 1 else 0 end	
	
	*/

	update #ActiveContractCounts
	set CountOfPrimeVendorItems = ( select count(*)
	from DI_DrugItems i 
	where i.ContractId = c.ContractId 
	and  i.PrimeVendor = 'T' )
	from #ActiveContractCounts c
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting prime vendor items in NACCM'
		goto ERROREXIT
	END	
	
	select ContractId , 
		NACCMContractNumber, 
		ItemsWithFETValues,
		NValueCount,
		LinkedButNotDiscontinued,
		CoveredItemCount, 
		ValidFCPCount, 
		VendorName,
		VendorIsIncludedInMailout,
		SingleDualFromMailout,
		ItemsWithNoPricesCount,
		ItemsWithNoFSSPricesCount,
		ItemsWithDualPricesCount,
		ItemsWithFSSGTFCP,
		ItemsWithBIG4GTFCP,
		ItemsWithPricesThatAreZero,
		ItemsWithPricesThatAreLTFET,
		ItemsWithIdenticalFSSAndBIG4Prices, 
		DualPriceItemsWithoutFSSPriceCount,  
		MissingKnownNFAMPItemCount,      
		DiscontinuedInNFAMPButNotInNACCMCount,
		ItemsInNFAMPMarkedAsNonCovered,
		PrimeVendorIndicatedInMailout,
		CountOfPrimeVendorItems
	from #ActiveContractCounts
	order by NACCMContractNumber
	


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






	




