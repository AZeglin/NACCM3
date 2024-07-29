IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectPLUploadProgressForAllContractsReport')
	BEGIN
		DROP  Procedure  SelectPLUploadProgressForAllContractsReport
	END

GO

CREATE Procedure  [dbo].[SelectPLUploadProgressForAllContractsReport]
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@FutureHistoricalSelectionCriteria nchar(1)   /* F Future is 10/1 of current year till present, C Current is 1/1 of current year till present, P Past is 10/1 of last year till 9/30 of present year */
)

AS


BEGIN TRANSACTION


	DECLARE @error int,
		@rowcount int,
		@errorMsg nvarchar(250),
		@currentYear int,
		@SQL nvarchar(2400),
		@uploadTestStartDate datetime,
		@uploadTestEndDate datetime,
		@startOfNewYear datetime,
		@lastYear int,
		@nextYear int,
		@fcpYear int



	/* log the request for the report */
	exec InsertDrugItemUserActivity @ReportUserLoginId, 'R', 'PL Upload Status For All Contracts Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @currentYear = year(getdate())
	select @lastYear = @currentYear - 1
	select @nextYear = @currentYear + 1

	-- future
	if( @FutureHistoricalSelectionCriteria = 'F' )
	BEGIN
		select @uploadTestStartDate = convert( datetime, '10/1/' + convert( nvarchar(4), @currentYear ))		
		select @startOfNewYear = convert( datetime, '1/1/' + convert( nvarchar(4), @nextYear ))
		select @uploadTestEndDate = getdate()
		select @fcpYear = @nextYear
	END
	-- past
	else if( @FutureHistoricalSelectionCriteria = 'P' )
	BEGIN		
		select @uploadTestStartDate = convert( datetime, '10/1/' + convert( nvarchar(4), @lastYear ))
		select @startOfNewYear = convert( datetime, '1/1/' + convert( nvarchar(4), @currentYear ))
		select @uploadTestEndDate = convert( datetime, '9/30/' + convert( nvarchar(4), @currentYear ))
		select @fcpYear = @lastYear
	END
	else
	-- if looking at current year presume its midyear and only look at uploads and active prices for the current calendar year
	BEGIN
		select @uploadTestStartDate = convert( datetime, '1/1/' + convert( nvarchar(4), @currentYear ))		
		select @startOfNewYear = convert( datetime, '1/1/' + convert( nvarchar(4), @currentYear ))
		select @uploadTestEndDate = getdate()
		select @fcpYear = @currentYear
	END
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error initializing dates.'
		goto ERROREXIT
	END
	
	create table #ActiveContractCounts 
	(
		ContractId int NOT NULL, 
		NACCMContractNumber nvarchar(20) NOT NULL, 		
		VendorName nvarchar(75) NULL,
		COID int NULL,
		COName nvarchar(120) NULL,
		CoveredItemCount int NULL, 
		ValidFCPCount int NULL, 		
		UploadAttempts int NULL,
		UploadedFSSPricesCount int NULL,
		UploadedBIG4PricesCount int NULL,
		UploadedRestrictedPricesCount int NULL,
		UploadErrorCount int NULL,
		ModifedFSSPriceCount int NULL,
		ModifiedBIG4PriceCount int NULL,
		ModifiedRestrictedPriceCount int NULL		
	)

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table'
		goto ERROREXIT
	END

	/* get all active contracts with covered items and active prices */
	insert into #ActiveContractCounts
		( ContractId, NACCMContractNumber, VendorName, COID, COName )
	select c.ContractId, c.NACCMContractNumber, x.Contractor_Name, x.CO_ID, u.FullName
		from DI_Contracts c join NAC_CM.dbo.tbl_Cntrcts x on c.NACCMContractNumber = x.CntrctNum
		join NAC_CM.dbo.tlkup_UserProfile u on x.CO_ID = u.CO_ID
		where dbo.IsContractActiveFunction( c.NACCMContractNumber, GETDATE() ) = 1
		and exists ( select Price from DrugItem.dbo.DI_DrugItemPrice p join DrugItem.dbo.DI_DrugItems i on p.DrugItemId = i.DrugItemId
					where i.ContractId = c.ContractId
					and p.IsFSS = 1 
					and p.IsTemporary = 0
					and i.Covered = 'T' 
					and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ) 	
					and getdate() between p.PriceStartDate and p.PriceStopDate )
						
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving active contract list'
		goto ERROREXIT
	END

	update #ActiveContractCounts
		set CoveredItemCount = ( select count(*) 
								from DI_DrugItems d
								where d.ContractId = c.ContractId
								and d.Covered = 'T'
								and d.ExcludeFromExport = 0 
								and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) 								
								and exists ( select p.DrugItemPriceId from DI_DrugItemPrice p where p.DrugItemId = d.DrugItemId 
											and getdate() between p.PriceStartDate and p.PriceStopDate )
								)
		from #ActiveContractCounts c
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting covered items'
		goto ERROREXIT
	END		
	
	update #ActiveContractCounts
		set ValidFCPCount = ( Select count(*)
							From DI_FCP f
							Join di_yearlookup y
							on f.YearId = y.YearId
							join DI_DrugItemNDC n
							on f.ndc_1 = n.FdaAssignedLabelerCode and f.ndc_2 = n.ProductCode and f.ndc_3 = n.PackageCode
							join DI_DrugItems i
							on n.DrugItemNDCId = i.DrugItemNDCId
							where y.YearValue = @fcpYear
							and i.ContractId = c.ContractId
							and f.ContractId = c.ContractId
							and f.FCP is not null 
							and f.FCP <> 0
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )							
							and exists ( select p.DrugItemPriceId from DI_DrugItemPrice p where p.DrugItemId = i.DrugItemId 
											and getdate() between p.PriceStartDate and p.PriceStopDate )
							)
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting fcp values'
		goto ERROREXIT
	END							
	
	update #ActiveContractCounts
		set UploadAttempts = ( Select count(*) 
								from ExportUpload.dbo.EU_Activity a left outer join ExportUpload.dbo.EU_PharmaceuticalActivityDetails d on a.ActivityId = d.ActivityId
								join ExportUpload.dbo.EU_ActivityTypes t on a.ActivityType = t.ActivityType								
								join ExportUpload.dbo.EU_Statuses s on a.ExportUploadStatus = s.ExportUploadStatus
								where a.ActivityDataType = 'P' 
								and t.ActivityTypeDescription = 'Upload'
								and a.ContractNumber = c.NACCMContractNumber
								and CONVERT( datetime, convert( nvarchar(20), a.CreationDate, 101 )) between @uploadTestStartDate and @uploadTestEndDate
						)						
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting upload attempts'
		goto ERROREXIT
	END						

	Update #ActiveContractCounts	
	set UploadedFSSPricesCount = isnull( x.UploadedFSSPricesCount, 0 ),
		UploadedBIG4PricesCount = isnull( x.UploadedBIG4PricesCount, 0 ), 
		UploadedRestrictedPricesCount = isnull( x.UploadedRestrictedPricesCount, 0 ),
		UploadErrorCount = isnull( x.UploadErrorCount, 0 )
		
	from ( select a.ContractNumber,
				sum(d.ChangedFSSPricesCount) as UploadedFSSPricesCount, 
				sum(d.ChangedBIG4PricesCount) as UploadedBIG4PricesCount, 
				sum(d.ChangedRestrictedPricesCount) as UploadedRestrictedPricesCount,
				sum(d.FSSErrorCount + d.BIG4ErrorCount + d.RestrictedErrorCount) as UploadErrorCount	
			from ExportUpload.dbo.EU_Activity a join ExportUpload.dbo.EU_PharmaceuticalActivityDetails d on a.ActivityId = d.ActivityId				
				where  CONVERT( datetime, convert( nvarchar(20), a.CreationDate, 101 )) between @uploadTestStartDate and @uploadTestEndDate
				group by a.ContractNumber
	) x join #ActiveContractCounts c on x.ContractNumber = c.NACCMContractNumber
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items uploaded'
		goto ERROREXIT
	END				


	update #ActiveContractCounts
		set ModifedFSSPriceCount = ( select count(*) 
								from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
								where i.ContractId = c.ContractId
								and i.Covered = 'T'
								and i.ExcludeFromExport = 0 
								and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ) 
								and p.IsFSS = 1		
								and p.IsTemporary = 0						
								and p.PriceStartDate >= @startOfNewYear
								and p.LastModificationDate between @uploadTestStartDate and @uploadTestEndDate								
								)
		from #ActiveContractCounts c
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting modified fss prices'
		goto ERROREXIT
	END					
	
	update #ActiveContractCounts
		set ModifiedBIG4PriceCount = ( select count(*) 
								from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
								where i.ContractId = c.ContractId
								and i.Covered = 'T'
								and i.ExcludeFromExport = 0 
								and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ) 
								and p.IsBIG4 = 1		
								and p.IsTemporary = 0									
								and p.PriceStartDate >= @startOfNewYear
								and p.LastModificationDate between @uploadTestStartDate and @uploadTestEndDate								
								)
		from #ActiveContractCounts c
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting modified big4 prices'
		goto ERROREXIT
	END					
		
	update #ActiveContractCounts
		set ModifiedRestrictedPriceCount = ( select count(*) 
								from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
								where i.ContractId = c.ContractId
								and i.Covered = 'T'
								and i.ExcludeFromExport = 0 
								and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null ) 
								and p.IsFSS = 0
								and p.IsBIG4 = 0
								and p.IsTemporary = 0							
								and p.PriceStartDate >= @startOfNewYear
								and p.LastModificationDate between @uploadTestStartDate and @uploadTestEndDate								
								)
		from #ActiveContractCounts c	

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting modified restricted prices'
		goto ERROREXIT
	END			
			
	select ContractId , 
		NACCMContractNumber, 		
		VendorName,	
		COID,
		COName,
		CoveredItemCount, 
		ValidFCPCount, 		
		isnull( UploadAttempts, 0 ) as UploadAttempts,
		isnull( UploadedFSSPricesCount, 0 ) as UploadedFSSPricesCount,
		isnull( UploadedBIG4PricesCount, 0 ) as UploadedBIG4PricesCount,
		isnull( UploadedRestrictedPricesCount, 0 ) as UploadedRestrictedPricesCount,
		isnull( UploadErrorCount, 0 ) as UploadErrorCount,
		ModifedFSSPriceCount,
		ModifiedBIG4PriceCount,
		ModifiedRestrictedPriceCount		
				
	from #ActiveContractCounts
	order by NACCMContractNumber
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting temptable for report'
		goto ERROREXIT
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

ENDEXIT:






	




