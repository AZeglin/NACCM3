IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'RebateDueReport' )
BEGIN
	DROP PROCEDURE RebateDueReport
END
GO

CREATE PROCEDURE RebateDueReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractingOfficerId int, /* -1 = all */
@ScheduleNumber int, /* may be -1 = all */
@ActiveHistoricalRebate char(1), /* 'H' - historical only, 'A' - active only, 'B' = both */
@IncludeCustom char(1), /* 'S' = standard only, 'C' = custom only, 'B' = both */
@StartingYear int,
@StartingQuarter int,
@EndingYear int,
@EndingQuarter int,
@RebateStartingYear int,
@RebateStartingQuarter int,
@RebateEndingYear int,
@RebateEndingQuarter int,
@ContractNumber nvarchar(50) = null,
@VendorName nvarchar(75) = null
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(max),    /*  4491 char */
		@query1 nvarchar(max),  /* 908 char */
		@union nvarchar(50),
		@query2 nvarchar(max),   /* 1656 char */
		@sqlParms nvarchar(1000),
		@joinSecurityServerName nvarchar(1000),
		@whereCustom nvarchar(30),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@whereActiveRebate1 nvarchar(200),
		@whereActiveRebate2 nvarchar(200),
		@whereContractNumber nvarchar(100),
		@contractNumberOverride bit,
		@whereVendorName nvarchar(200),

		@contractStartingDate datetime,
		@contractEndingDate datetime,
		@rebateStartingQuarterId int,
		@rebateEndingQuarterId int,
		@currentQuarterId int,
		@rebateStartingDate datetime,
		@rebateEndingDate datetime,

		@percentString nvarchar(12),
		@thresholdString nvarchar(14),
		@rebatePercentOfSales numeric(8,3),
		@rebateThreshold money,
		@rebatePercentOfSalesString nvarchar(20),
		@rebateThresholdString nvarchar(30),
		@whereRebateDates1 nvarchar(1000),
		@whereRebateDates2 nvarchar(1000)
	

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Rebate Due Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	IF OBJECT_ID('tempdb..#RebateDueReport') IS NOT NULL 
	BEGIN
		drop table #RebateDueReport
	
		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error dropping RebateReport temp table.'
			goto ERROREXIT
		END
	END

	create table #RebateDueReport
	(
		ContractNumber nvarchar(50),
		VendorName nvarchar(75),
		TIN nvarchar(9),
		AwardDate datetime,
		EffectiveDate datetime,
		ExpirationDate datetime,

		EstimatedContractValue money,
		Description nvarchar(200),
		COId int,
		ContractingOfficerName nvarchar(50),
		ContractingOfficerLastName nvarchar(20),

		ContractAdministrator nvarchar(30),
		ContractAdministratorPhone nvarchar(15),
		ContractAdministratorPhoneExt nvarchar(5),
		ContractAdministratorFax nvarchar(15),
		ContractAdministratorEmail nvarchar(50),
		
		VendorAddress1  nvarchar(100),
		VendorAddress2  nvarchar(100),
		VendorCity  nvarchar(20),
		VendorStateCode nvarchar(2),
		VendorZip  nvarchar(10),

		ScheduleNumber int,
		ScheduleName nvarchar(75),
		DivisionId int,

		RebateRequired bit,
		OldRebateTerms nvarchar(255),
		
		RebateId int,
		RebateStartQuarterId int,
		RebateStartDate datetime,
		RebateEndQuarterId int,
		RebateEndDate datetime,
		CustomStartDate datetime,
		CustomStartQuarter int,
		CustomEndDate datetime,  /* always 1 year minus 1 day after custom start date */
		CustomEndQuarter int,
		RebatePercentOfSales numeric(8,3),
		RebateThreshold money,
		IsCustom bit,

		TotalSales money,
		SalesMinusThreshold money,
		RebateDue money,

		RebateClause nvarchar(4000)
	)

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table for rebate request.'
		goto ERROREXIT
	END

	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating security where clause for rebate request.'
		goto ERROREXIT
	END
	
	select @contractStartingDate = Start_Date
		from tlkup_year_qtr
		where Year = @StartingYear
		and Qtr = @StartingQuarter

	select @contractEndingDate = End_Date
		from tlkup_year_qtr
		where Year = @EndingYear
		and Qtr = @EndingQuarter		

	select @rebateStartingQuarterId = Quarter_ID,
		@rebateStartingDate = Start_Date
		from tlkup_year_qtr
		where Year = @RebateStartingYear
		and Qtr = @RebateStartingQuarter
		
	select @rebateEndingQuarterId = Quarter_ID,
		@rebateEndingDate = End_Date	
		from tlkup_year_qtr
		where Year = @RebateEndingYear
		and Qtr = @RebateEndingQuarter	

	select @currentQuarterId = Quarter_ID
		from tlkup_year_qtr
		where GETDATE() between Start_Date and End_Date	

	select @query1 = 'insert into #RebateDueReport
	(
		ContractNumber,
		VendorName,
		TIN,
		AwardDate,
		EffectiveDate,
		ExpirationDate,

		EstimatedContractValue,
		Description,
		COId,
		ContractingOfficerName,
		ContractingOfficerLastName,

		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorPhoneExt,
		ContractAdministratorFax,
		ContractAdministratorEmail,
		
		VendorAddress1,
		VendorAddress2,
		VendorCity,
		VendorStateCode,
		VendorZip,

		ScheduleNumber,
		ScheduleName,
		DivisionId,

		RebateRequired,
		OldRebateTerms,

		RebateId,
		RebateStartQuarterId,
		RebateStartDate,     /* c */
		RebateEndQuarterId,
		RebateEndDate,    /* c */
		CustomStartDate,
		CustomStartQuarter, /* c */
		CustomEndDate,   /* c */
		CustomEndQuarter,  /* c */
		RebatePercentOfSales,
		RebateThreshold,
		IsCustom,
		SalesMinusThreshold,
		RebateDue

	) '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END

	select @query2 = ' select c.CntrctNum,
		c.Contractor_Name,
		c.TIN,
		c.Dates_CntrctAward,
		c.Dates_Effective,
		c.Dates_CntrctExp,

		c.Estimated_Contract_Value,
		c.Drug_Covered,
		c.CO_ID,
		s.FullName,
		s.LastName,

		c.POC_Primary_Name,
		c.POC_Primary_Phone,
		c.POC_Primary_Ext,
		c.POC_Primary_Fax,
		c.POC_Primary_Email,

		c.Primary_Address_1,
		c.Primary_Address_2,
		c.Primary_City,
		c.Primary_State,
		c.Primary_Zip,

		c.Schedule_Number,
		t.Schedule_Name,
		t.Division,

		c.RebateRequired,
		c.Annual_Rebate,

		r.RebateId,
		r.StartQuarterId,
		( select Start_Date from tlkup_year_qtr where Quarter_ID = r.StartQuarterId ),
		r.EndQuarterId,
		( select End_Date from tlkup_year_qtr where Quarter_ID = r.EndQuarterId ),
		r.CustomStartDate,
		( select Quarter_ID from tlkup_year_qtr where r.CustomStartDate between Start_Date and End_Date ),
		DATEADD( dd, -1, DATEADD( yy, 1, r.CustomStartDate )),
		( select Quarter_ID from tlkup_year_qtr where DATEADD( dd, -1, DATEADD( yy, 1, r.CustomStartDate )) between Start_Date and End_Date ),
		r.RebatePercentOfSales,
		r.RebateThreshold,
		r.IsCustom, 
		0,
		0

	from  tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] s on c.CO_ID = s.CO_ID
		join [tlkup_Sched/Cat] t on c.Schedule_Number = t.Schedule_Number
		join tbl_Rebates r on c.CntrctNum = r.ContractNumber
		where t.Division = 1 		
		and c.RebateRequired = 1
		and ( c.Dates_Effective between ''' + convert( nvarchar(12), @contractStartingDate, 101 ) + ''' and ''' + convert( nvarchar(12), @contractEndingDate, 101 ) + ''''
		+ ' or c.Dates_CntrctExp between ''' + convert( nvarchar(12), @contractStartingDate, 101 ) + ''' and ''' + convert( nvarchar(12), @contractEndingDate, 101 ) + ''''
		+ ' or ''' + convert( nvarchar(12), @contractStartingDate, 101 ) + ''' between c.Dates_Effective and c.Dates_CntrctExp 
		or  ''' + convert( nvarchar(12), @contractEndingDate, 101 ) + ''' between c.Dates_Effective and c.Dates_CntrctExp ) '
	
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2'
		goto ERROREXIT
	END
			
	select @union = ' union '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning union string'
		goto ERROREXIT
	END

	select @whereRebateDates1 = ' and ( r.StartQuarterId between @rebateStartingQuarterId_parm and @rebateEndingQuarterId_parm 
								 or r.EndQuarterId  between @rebateStartingQuarterId_parm and @rebateEndingQuarterId_parm
								 or @rebateStartingQuarterId_parm between r.StartQuarterId and r.EndQuarterId
								 or @rebateEndingQuarterId_parm between r.StartQuarterId and r.EndQuarterId ) 
								 and r.CustomStartDate is null '
								   
	select @whereRebateDates2 = ' and ( r.CustomStartDate between @rebateStartingDate_parm and @rebateEndingDate_parm 
								 or ( DATEADD( dd, -1, DATEADD( yy, 1, r.CustomStartDate )) between @rebateStartingDate_parm and @rebateEndingDate_parm )
								 or ( @rebateStartingDate_parm between r.CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, r.CustomStartDate )) )
								 or ( @rebateEndingDate_parm between r.CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, r.CustomStartDate ))) ) 
								  and r.CustomStartDate is not null '

	select @whereCustom = ' '
	if @IncludeCustom = 'C'
	BEGIN
		select @whereCustom = ' and r.IsCustom = 1 '
	END
	else if @IncludeCustom = 'S'
	BEGIN
		select @whereCustom = ' and r.IsCustom = 0 '
	END

	if @ScheduleNumber <> -1
	BEGIN
		select @whereSchedule = ' and t.Schedule_Number = ' + convert( nvarchar(10), @ScheduleNumber )
	END
	else
	BEGIN
		select @whereSchedule = ' '
	END

	if @ContractingOfficerId <> -1
	BEGIN
		select @whereContractingOfficer = ' and c.CO_ID = ' + convert( nvarchar(10), @ContractingOfficerId )
	END
	else
	BEGIN
		select @whereContractingOfficer = ' '
	END
	

	/* date criteria with standard quarters */
	/* @@ActiveHistoricalRebate 'H' - historical only,  'A' - active only, 'B' - both */
	select @whereActiveRebate1 = ' '
	if @ActiveHistoricalRebate = 'A'
	BEGIN
		select @whereActiveRebate1 = ' and @currentQuarterId_parm between r.StartQuarterId and r.EndQuarterId '
	END			
	else if @ActiveHistoricalRebate = 'H'
	BEGIN
		select @whereActiveRebate1 = ' and r.EndQuarterId < @currentQuarterId_parm '
	END
	
	/* date criteria for 2nd half of union with custom dates */
	/* @@ActiveHistoricalRebate 'H' - historical only,  'A' - active only, 'F' - future only, 'B' - both future and active */
	select @whereActiveRebate2 = ' '
	if @ActiveHistoricalRebate = 'A'
	BEGIN
		select @whereActiveRebate2 = ' and ( getdate() between r.CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, r.CustomStartDate ))) '
	END
	else if @ActiveHistoricalRebate = 'H'
	BEGIN
		select @whereActiveRebate2 = ' and DATEADD( dd, -1, DATEADD( yy, 1, r.CustomStartDate )) < getdate() '
	END

	select @whereVendorName = ' '
	if @VendorName is not null 
	BEGIN
		if LEN(LTRIM(RTRIM( @VendorName ))) > 0
		BEGIN
			select @whereVendorName = ' and c.Contractor_Name like ''%' + LTRIM(RTRIM( @VendorName )) + '%'''
		END
	END

	select @sqlParms = '@currentQuarterId_parm int, @rebateStartingQuarterId_parm int, @rebateEndingQuarterId_parm int, @rebateStartingDate_parm datetime, @rebateEndingDate_parm datetime '

	-- contract number supersedes all other optional criteria
	select @whereContractNumber = ' '
	select @contractNumberOverride = 0
	if @ContractNumber is not null 
	BEGIN
		if LEN(LTRIM(RTRIM( @ContractNumber ))) > 0
		BEGIN
			select @whereContractNumber = ' and c.CntrctNum = ''' + @ContractNumber + ''''
			select @contractNumberOverride = 1
		END
	END

	if @contractNumberOverride = 1
	BEGIN
		select @query = @query1 + @query2 + @whereRebateDates1 + @whereContractNumber + @whereActiveRebate1 + @union + @query2 + @whereRebateDates2 + @whereContractNumber + @whereActiveRebate2 
	END
	else
	BEGIN
		select @query = @query1 + @query2 + @whereRebateDates1 + @whereCustom + @whereSchedule + @whereContractingOfficer + @whereActiveRebate1 + @whereVendorName + @union + @query2 + + @whereRebateDates2 + @whereCustom + @whereSchedule + @whereContractingOfficer + @whereActiveRebate2 + @whereVendorName
	END

	insert into TempRebateReportRequestTracking
	( RebateQueryText, ParmList, RequestDate )
	values
	( @query, @sqlParms, GETDATE() )

	exec SP_EXECUTESQL @query , @sqlParms, @currentQuarterId_parm = @currentQuarterId, @rebateStartingQuarterId_parm = @rebateStartingQuarterId, @rebateEndingQuarterId_parm = @rebateEndingQuarterId,  @rebateStartingDate_parm = @rebateStartingDate, @rebateEndingDate_parm = @rebateEndingDate

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for rebate report.'
		goto ERROREXIT
	END

	select @percentString = '{percent}'
	select @thresholdString = '{threshold}'

	update #RebateDueReport 
	set RebateClause = REPLACE( REPLACE( s.RebateClause, @thresholdString, CONVERT( nvarchar(30), t.RebateThreshold ) ), @percentString, CONVERT( nvarchar(20), t.RebatePercentOfSales ) )
	from #RebateDueReport t join tbl_RebatesStandardRebateTerms k on k.RebateId = t.RebateId
	join tbl_StandardRebateTerms s on k.StandardRebateTermId = s.StandardRebateTermId
	where t.IsCustom = 0

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with clause (1) for rebate report.'
		goto ERROREXIT
	END

	update #RebateDueReport
	set RebateClause = c.RebateClause
	from #RebateDueReport t join tbl_CustomRebateTerms c on t.RebateId = c.RebateId
	where t.IsCustom = 1

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with clause (2) for rebate report.'
		goto ERROREXIT
	END

	-- per requirements, this report uses rebate term for sales
	update #RebateDueReport
	set TotalSales = ( select SUM( s.VA_Sales ) + SUM( s.OGA_Sales ) + SUM( s.SLG_Sales )
					from tbl_Cntrcts_Sales s 
					where s.CntrctNum = r.ContractNumber
					and s.Quarter_ID between r.RebateStartQuarterId and r.RebateEndQuarterId )
	from #RebateDueReport r
	where r.CustomStartDate is null

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with sales (1) for rebate report.'
		goto ERROREXIT
	END

	
	-- per requirements, this report uses rebate term for sales
	update #RebateDueReport
	set TotalSales = ( select SUM( s.VA_Sales ) + SUM( s.OGA_Sales ) + SUM( s.SLG_Sales )
					from tbl_Cntrcts_Sales s 
					where s.CntrctNum = r.ContractNumber
					and s.Quarter_ID between r.CustomStartQuarter and r.CustomEndQuarter )
	from #RebateDueReport r
	where r.CustomStartDate is not null

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with sales (2) for rebate report.'
		goto ERROREXIT
	END

	update #RebateDueReport
	set SalesMinusThreshold = ( case when ( TotalSales - RebateThreshold ) < 0 then 0 else  TotalSales - RebateThreshold  end )
	where RebateThreshold is not null
	--and IsCustom = 0   -- removed 11/9/2012

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with sales minus threshold (1) for rebate report.'
		goto ERROREXIT
	END

	update #RebateDueReport
	set SalesMinusThreshold = TotalSales
	where RebateThreshold is null
	--and IsCustom = 0   -- removed 11/9/2012

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with sales minus threshold (2) for rebate report.'
		goto ERROREXIT
	END

	update #RebateDueReport
	set RebateDue = ( RebatePercentOfSales / 100 ) * SalesMinusThreshold
	where RebatePercentOfSales is not null      -- IsCustom = 0  -- removed 11/9/2012

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with rebate due for rebate report.'
		goto ERROREXIT
	END

	select ContractNumber,
			VendorName,
			TIN,
			AwardDate,
			EffectiveDate,
			ExpirationDate,

			COId,
			ContractingOfficerName,
			ContractingOfficerLastName,

			ContractAdministrator,
			ContractAdministratorPhone,
			ContractAdministratorPhoneExt,
			ContractAdministratorFax,
			ContractAdministratorEmail,
		
			VendorAddress1,
			VendorAddress2,
			VendorCity,
			VendorStateCode,
			VendorZip,

			ScheduleNumber,
			ScheduleName,
			DivisionId,

			RebateRequired,
			OldRebateTerms,
			
			RebateId,
			RebateStartQuarterId,
			RebateStartDate,
			RebateEndQuarterId,
			RebateEndDate,
			CustomStartDate,
			CustomStartQuarter,
			CustomEndDate, 
			CustomEndQuarter,
			RebatePercentOfSales,
			RebateThreshold,
			IsCustom,

			TotalSales,
			SalesMinusThreshold,
			RebateDue,

			RebateClause

		from #RebateDueReport
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting active contract report results.'
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


