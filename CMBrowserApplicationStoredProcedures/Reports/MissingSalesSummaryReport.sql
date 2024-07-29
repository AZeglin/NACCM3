IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'MissingSalesSummaryReport')
	BEGIN
		DROP  Procedure  MissingSalesSummaryReport
	END

GO

CREATE Procedure MissingSalesSummaryReport
(
@ReportUserLoginId nvarchar(100), /* the user running the report */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@StartingYear int,
@StartingQuarter int,
@EndingYear int,
@EndingQuarter int,
@MissingOrZeroSalesToInclude nchar(1), /* M = missing only, 0 = zero sales only, B = both */
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int /* may be -1 = all NAC */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(300),
		@StartingQuarterId int,
		@EndingQuarterId int,
		@currentQuarterId int,
		@startingQuarterStartDate datetime,
		@endingQuarterEndDate datetime,
		@reportingQuarterId int,
		@query nvarchar(3200),
		@SQLParms nvarchar(1000),
		@whereSchedule nvarchar(100),
		@groupByString nvarchar(400),
		@joinSecurityServerName nvarchar(1000),
		@ReportUserCOID int,
		@MissingZeroContractsForSelectedDateRange int,
		@TotalContractsForSelectedDateRange int

	


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Missing Sales Summary Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @StartingQuarterId = Quarter_ID,
		@startingQuarterStartDate = Start_Date
	from tlkup_year_qtr
	where Year = @StartingYear
	and Qtr = @StartingQuarter
		
	select @EndingQuarterId = Quarter_ID,
	@endingQuarterEndDate = End_Date
	from tlkup_year_qtr
	where Year = @EndingYear
	and Qtr = @EndingQuarter		
		
	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'
	

	create table #SelectedContracts
	(
		ContractNumber nvarchar(50),
		
		VendorName nvarchar(75),
		EffectiveDate datetime,
		EffectiveQuarterId int,
		ExpirationDate datetime,
		ExpiredQuarterId int,
		CompletionDate datetime,
		CompletedQuarterId int,
	
		ContractAdministrator nvarchar(30),
		ContractAdministratorPhone nvarchar(15),
		ContractAdministratorPhoneExt nvarchar(5),
		ContractAdministratorEmail nvarchar(50),

		AlternateContractAdministrator nvarchar(30),
		AlternateContractAdministratorPhone nvarchar(15),
		AlternateContractAdministratorPhoneExt nvarchar(5),
		AlternateContractAdministratorEmail nvarchar(50),
		
		SalesAdministrator nvarchar(30),
		SalesAdministratorPhone nvarchar(15),
		SalesAdministratorPhoneExt nvarchar(5),
		SalesAdministratorEmail nvarchar(50),		
	
		VendorAddress1  nvarchar(100),
		VendorAddress2  nvarchar(100),
		VendorCity  nvarchar(20),
		VendorStateCode nvarchar(2),
		VendorZip  nvarchar(10),
		
		COId int,
		ContractingOfficerName nvarchar(50),
		ScheduleNumber int,
		ScheduleName nvarchar(75),
		ListOfSinsForContract nvarchar(3000),
		DivisionId int,
		MissingZeroContractsForSelectedDateRange int, -- same for every row
		TotalContractsForSelectedDateRange int  -- same for every row
	)

		
	create table #MissingContractQuarters
	(
		ContractNumber nvarchar(50),
		MissingQuarterId int,
		MissingOrZero nchar(1),    -- M = Missing;  0 = Zero
		YearQuarterDescription nvarchar(20)
	)
	
		
	select @query = 'insert into #SelectedContracts
	(
		ContractNumber,	
		VendorName,
		EffectiveDate,
		EffectiveQuarterId,
		ExpirationDate,
		ExpiredQuarterId,
		CompletionDate,
		CompletedQuarterId,
	
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorPhoneExt,
		ContractAdministratorEmail,

		AlternateContractAdministrator,
		AlternateContractAdministratorPhone,
		AlternateContractAdministratorPhoneExt,
		AlternateContractAdministratorEmail,

		SalesAdministrator,
		SalesAdministratorPhone,
		SalesAdministratorPhoneExt,
		SalesAdministratorEmail,	
			
		VendorAddress1,
		VendorAddress2,
		VendorCity,
		VendorStateCode,
		VendorZip,
				
		COId,
		ContractingOfficerName,
		ScheduleNumber,
		ScheduleName,
		DivisionId
	)
	select
		c.CntrctNum,
		c.Contractor_Name,
		c.Dates_Effective,
		( select y.Quarter_ID from tlkup_year_qtr y where c.Dates_Effective between y.Start_Date and y.End_Date ) as EffectiveQuarterId,
		c.Dates_CntrctExp,
		( select y.Quarter_ID from tlkup_year_qtr y where c.Dates_CntrctExp between y.Start_Date and y.End_Date ) as ExpiredQuarterId,
		c.Dates_Completion,
		case when ( c.Dates_Completion is null ) then -1 else ( select y.Quarter_ID from tlkup_year_qtr y where c.Dates_Completion between y.Start_Date and y.End_Date ) end as CompletedQuarterId,		
	
		c.POC_Primary_Name,
		c.POC_Primary_Phone,
		c.POC_Primary_Ext,
		c.POC_Primary_Email,
		
		c.POC_Alternate_Name,
		c.POC_Alternate_Phone,
		c.POC_Alternate_Ext,
		c.POC_Alternate_Email,
		
		c.POC_Sales_Name,
		c.POC_Sales_Phone,
		c.POC_Sales_Ext,
		c.POC_Sales_Email,		
	
		c.Primary_Address_1,
		c.Primary_Address_2,
		c.Primary_City,
		c.Primary_State,
		c.Primary_Zip,
				
		c.CO_ID,
		u.FullName,
		c.Schedule_Number,
		t.Schedule_Name,
		t.Division

	from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] t on c.Schedule_Number = t.Schedule_Number 
	where dbo.IsContractActiveForSalesFunction( c.CntrctNum, @startingQuarterStartDate_parm, @endingQuarterEndDate_parm ) = 1 '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2'
		goto ERROREXIT
	END

	if @ScheduleNumber <> -1
	BEGIN
		select @whereSchedule = ' and t.Schedule_Number = ' + convert( nvarchar(10), @ScheduleNumber )
	END
	else
	BEGIN
		if @DivisionId <> -1
		BEGIN
			select @whereSchedule = ' and t.Division = ' + convert( nvarchar(10), @DivisionId ) + ' and t.Schedule_Number <> 48 '
		END
		else
		BEGIN
			select @whereSchedule = ' and t.Schedule_Number <> 48 and t.Division <> 6 ' -- All NAC excludes SAC 
		END
	END	

	select @query = @query + @whereSchedule
					
	select @SQLParms = N'@startingQuarterStartDate_parm datetime, @endingQuarterEndDate_parm datetime'

	exec SP_EXECUTESQL @query , @SQLParms, @startingQuarterStartDate_parm = @startingQuarterStartDate, @endingQuarterEndDate_parm = @endingQuarterEndDate

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for Missing Sales Summary report.'
		goto ERROREXIT
	END

	/* backfill the sin list */
	update #SelectedContracts
	set ListOfSinsForContract = dbo.GetListOfSinsForContractFunction( t.ContractNumber )
	from #SelectedContracts t

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error backfilling SINs for Missing Sales Summary report.'
		goto ERROREXIT
	END

	/* backfill the row count for report header */
	select @TotalContractsForSelectedDateRange = COUNT(*) from #SelectedContracts

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error performing rowcount for Missing Sales Summary report.'
		goto ERROREXIT
	END

	update #SelectedContracts
	set TotalContractsForSelectedDateRange = @TotalContractsForSelectedDateRange

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating rowcount into temp table for Missing Sales Summary report.'
		goto ERROREXIT
	END


	if @MissingOrZeroSalesToInclude = 'M' or @MissingOrZeroSalesToInclude = 'B'
	BEGIN
		insert into #MissingContractQuarters
		(
			ContractNumber,	
			MissingQuarterId,
			MissingOrZero,
			YearQuarterDescription
		)		
		select m.ContractNumber,
			y.Quarter_ID,
			'M',
			y.Title
		from #SelectedContracts m, tlkup_year_qtr y	 
		where y.Quarter_ID not in ( select s.Quarter_ID from tbl_Cntrcts_Sales s
										where s.CntrctNum = m.ContractNumber )
		and y.Quarter_ID between @StartingQuarterId and @EndingQuarterId										
		and ((( y.Quarter_ID between m.EffectiveQuarterId and m.ExpiredQuarterId ) and m.CompletedQuarterId = -1 )
		or	(( y.Quarter_ID between m.EffectiveQuarterId and m.CompletedQuarterId ) and m.CompletedQuarterId <> -1  ))			
		
			
		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting missing contract quarters for Missing Sales Summary report.'
			goto ERROREXIT
		END	
	END
	
	if @MissingOrZeroSalesToInclude = '0' or @MissingOrZeroSalesToInclude = 'B'
	BEGIN
		insert into #MissingContractQuarters
		(
			ContractNumber,	
			MissingQuarterId,
			MissingOrZero,
			YearQuarterDescription
		)
		select m.ContractNumber,
			y.Quarter_ID,
			'0',
			y.Title
		from #SelectedContracts m join tbl_Cntrcts_Sales s on m.ContractNumber = s.CntrctNum
		join tlkup_year_qtr y on y.Quarter_ID = s.Quarter_ID
		and y.Quarter_ID between @StartingQuarterId and @EndingQuarterId
		group by m.ContractNumber, y.Quarter_ID, y.Title
		
		having sum( s.VA_Sales ) = 0 and sum( s.OGA_Sales ) = 0 and sum( s.SLG_Sales ) = 0
				
		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting zero sales contract quarters for Missing Sales Summary report.'
			goto ERROREXIT
		END	
	END

	select @MissingZeroContractsForSelectedDateRange = COUNT( distinct ContractNumber )
	from #MissingContractQuarters

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error counting distinct missing zero contracts for Missing Sales Summary report.'
		goto ERROREXIT
	END	

	update #SelectedContracts
	set MissingZeroContractsForSelectedDateRange = @MissingZeroContractsForSelectedDateRange

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating count of distinct missing zero contracts for Missing Sales Summary report.'
		goto ERROREXIT
	END	

	select m.ContractNumber,
		q.MissingQuarterId,
		q.MissingOrZero,
		q.YearQuarterDescription,
		m.VendorName,
		m.EffectiveDate,
		m.EffectiveQuarterId,
		m.ExpirationDate,
		
		m.ContractAdministrator,
		m.ContractAdministratorPhone,
		m.ContractAdministratorPhoneExt,
		m.ContractAdministratorEmail,

		m.AlternateContractAdministrator,
		m.AlternateContractAdministratorPhone,
		m.AlternateContractAdministratorPhoneExt,
		m.AlternateContractAdministratorEmail,

		m.SalesAdministrator,
		m.SalesAdministratorPhone,
		m.SalesAdministratorPhoneExt,
		m.SalesAdministratorEmail,
		
		m.VendorAddress1,
		m.VendorAddress2,
		m.VendorCity,
		m.VendorStateCode,
		m.VendorZip,
		m.COId,
		m.ContractingOfficerName,
		m.ScheduleNumber,
		m.ScheduleName,
		m.ListOfSinsForContract,
		m.DivisionId,
		m.MissingZeroContractsForSelectedDateRange,
		m.TotalContractsForSelectedDateRange

	from #SelectedContracts m join #MissingContractQuarters q on m.ContractNumber = q.ContractNumber
	order by m.ContractNumber

		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting final join for Missing Sales Summary report.'
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





