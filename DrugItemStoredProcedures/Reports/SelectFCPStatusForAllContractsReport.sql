IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectFCPStatusForAllContractsReport')
	BEGIN
		DROP  Procedure  SelectFCPStatusForAllContractsReport
	END

GO

CREATE Procedure SelectFCPStatusForAllContractsReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@FutureHistoricalSelectionCriteria nchar(1)  -- F future, A active
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@currentYear int,
	@SQL nvarchar(2400)


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertDrugItemUserActivity @ReportUserLoginId, 'R', 'FCP Status For All Contracts Report', '2'
	
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
	END

	create table #ActiveContractCounts 
	(
		ContractId int NOT NULL, 
		NACCMContractNumber nvarchar(20) NOT NULL, 
		CoveredItemCount int NULL, 
		ValidFCPCount int NULL, 
		VendorName nvarchar(75) NULL
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
		where dbo.IsContractActiveFunction( c.NACCMContractNumber, GETDATE() ) = 1
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving active contract list'
		goto ERROREXIT
	END

	select @SQL = N'update #ActiveContractCounts 
		set VendorName = c.Contractor_Name
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
		set CoveredItemCount = ( select count(*) 
								from DI_DrugItems d
								where d.ContractId = c.ContractId
								and d.Covered = 'T'
								and d.ExcludeFromExport = 0 
								and ( d.DiscontinuationDate >= getdate() or d.DiscontinuationDate is null ) 
								-- adding this clause for 2012 PL due to change in process for discontinuing items, need to remove this when disc date is used again
								and exists ( select p.DrugItemPriceId from DI_DrugItemPrice p where p.DrugItemId = d.DrugItemId )
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
							where y.YearValue = @currentYear
							and i.ContractId = c.ContractId
							and f.ContractId = c.ContractId
							and f.FCP is not null 
							and f.FCP <> 0
							and i.Covered = 'T'
							and i.ExcludeFromExport = 0 
							and ( i.DiscontinuationDate >= getdate() or i.DiscontinuationDate is null )
							-- adding this clause for 2012 PL due to change in process for discontinuing items, need to remove this when disc date is used again
							and exists ( select p.DrugItemPriceId from DI_DrugItemPrice p where p.DrugItemId = i.DrugItemId ))
	from #ActiveContractCounts c		
					
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting fcp values'
		goto ERROREXIT
	END							
	
	select ContractId , 
		NACCMContractNumber, 
		CoveredItemCount, 
		ValidFCPCount, 
		VendorName
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






	




