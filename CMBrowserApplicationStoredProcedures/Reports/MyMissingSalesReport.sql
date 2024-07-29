IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'MyMissingSalesReport')
	BEGIN
		DROP  Procedure  MyMissingSalesReport
	END

GO

CREATE Procedure MyMissingSalesReport
(
@ReportUserLoginId nvarchar(100), /* the user running the report */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@QuartersToInclude nchar(1)   /* A = all quarters back to effective date, 1 = 1 qtr beyond reporting quarter */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@currentQuarterId int,
		@reportingQuarterStartDate datetime,
		@reportingQuarterId int,
		@query nvarchar(3200),
		@SQLParms nvarchar(1000),
		@whereSchedule nvarchar(100),
		@groupByString nvarchar(400),
		@joinSecurityServerName nvarchar(1000),
		@ReportUserCOID int

	


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'My Missing Sales Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'

	select @query = 'select @ReportUserCOID_parm = u.CO_ID
	from ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u
	where u.UserName = @ReportUserLoginId_parm 
	and Inactive = 0 '
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
	
	select @SQLParms = N'@ReportUserLoginId_parm nvarchar(100), @ReportUserCOID_parm int OUTPUT'

	exec SP_EXECUTESQL @query, @SQLParms, @ReportUserLoginId_parm = @ReportUserLoginId, @ReportUserCOID_parm = @ReportUserCOID OUTPUT

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting CO_ID for LoginId for My Missing Sales report.'
		goto ERROREXIT
	END
	
	select @currentQuarterId = y.Quarter_ID
	from tlkup_year_qtr y
	where getdate() between y.Start_Date and y.End_Date

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting current quarter.'
		goto ERROREXIT
	END
	
	select @reportingQuarterId = @currentQuarterId - 1
	
	select @reportingQuarterStartDate = y.Start_Date
	from tlkup_year_qtr y
	where y.Quarter_ID = @reportingQuarterId

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting reporting quarter start date.'
		goto ERROREXIT
	END

	create table #MyContracts
	(
		ContractNumber nvarchar(50),
		
		VendorName nvarchar(75),
		EffectiveDate datetime,
		EffectiveQuarterId int,
		ExpirationDate datetime,
		ContractAdministrator nvarchar(30),
		ContractAdministratorPhone nvarchar(15),
		ContractAdministratorEmail nvarchar(50),
		
		SalesAdministrator nvarchar(30),
		SalesAdministratorPhone nvarchar(15),
		SalesAdministratorEmail nvarchar(50),
		
		COId int,
		ContractingOfficerName nvarchar(50),
		ScheduleNumber int,
		ScheduleName nvarchar(75),
		DivisionId int
	)

		
	create table #MissingContractQuarters
	(
		ContractNumber nvarchar(50),
		MissingQuarterId int
	)
	
	
	create table #MyMissingSalesReport
	(
		ContractNumber nvarchar(50),
		MissingQuarterId int,
		
		VendorName nvarchar(75),
		EffectiveDate datetime,
		EffectiveQuarterId int,
		ExpirationDate datetime,
		ContractAdministrator nvarchar(30),
		ContractAdministratorPhone nvarchar(15),
		ContractAdministratorEmail nvarchar(50),
		
		SalesAdministrator nvarchar(30),
		SalesAdministratorPhone nvarchar(15),
		SalesAdministratorEmail nvarchar(50),
		
		COId int,
		ContractingOfficerName nvarchar(50),
		ScheduleNumber int,
		ScheduleName nvarchar(75),
		DivisionId int
	)

		
	select @query = 'insert into #MyContracts
	(
		ContractNumber,	
		VendorName,
		EffectiveDate,
		EffectiveQuarterId,
		ExpirationDate,
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorEmail,
		SalesAdministrator,
		SalesAdministratorPhone,
		SalesAdministratorEmail,
		
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
		c.POC_Primary_Name,
		c.POC_Primary_Phone,
		c.POC_Primary_Email,
		c.POC_Sales_Name,
		c.POC_Sales_Phone,
		c.POC_Sales_Email,
		
		c.CO_ID,
		u.FullName,
		c.Schedule_Number,
		t.Schedule_Name,
		t.Division

	from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] t on c.Schedule_Number = t.Schedule_Number 
	where ( c.CO_ID = @ReportUserCOID_parm OR t.Asst_Director = @ReportUserCOID_parm OR t.Director = @ReportUserCOID_parm OR t.Schedule_Manager = @ReportUserCOID_parm )
	and dbo.IsContractActiveFunction( c.CntrctNum, @reportingQuarterStartDate_parm ) = 1 '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2'
		goto ERROREXIT
	END
					
	select @SQLParms = N'@ReportUserCOID_parm int, @reportingQuarterStartDate_parm datetime'

	exec SP_EXECUTESQL @query, @SQLParms, @ReportUserCOID_parm = @ReportUserCOID, @reportingQuarterStartDate_parm = @reportingQuarterStartDate

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for My Missing Sales report.'
		goto ERROREXIT
	END

	insert into #MissingContractQuarters
	(
		ContractNumber,	
		MissingQuarterId
	)
	select m.ContractNumber,
		y.Quarter_ID
	from #MyContracts m, tlkup_year_qtr y	 
	where y.Quarter_ID not in ( select s.Quarter_ID from tbl_Cntrcts_Sales s
									where s.CntrctNum = m.ContractNumber )
	and y.Quarter_ID between m.EffectiveQuarterId and @reportingQuarterId
		
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting missing contract quarters for My Missing Sales report.'
		goto ERROREXIT
	END	
		
	insert into #MyMissingSalesReport
	(
		ContractNumber,	
		MissingQuarterId,
		VendorName,
		EffectiveDate,
		EffectiveQuarterId,
		ExpirationDate,
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorEmail,
		SalesAdministrator,
		SalesAdministratorPhone,
		SalesAdministratorEmail,
		
		COId,
		ContractingOfficerName,
		ScheduleNumber,
		ScheduleName,
		DivisionId
	)
	select m.ContractNumber,
		q.MissingQuarterId,
		m.VendorName,
		m.EffectiveDate,
		m.EffectiveQuarterId,
		m.ExpirationDate,
		m.ContractAdministrator,
		m.ContractAdministratorPhone,
		m.ContractAdministratorEmail,
		m.SalesAdministrator,
		m.SalesAdministratorPhone,
		m.SalesAdministratorEmail,
		m.COId,
		m.ContractingOfficerName,
		m.ScheduleNumber,
		m.ScheduleName,
		m.DivisionId
	from #MyContracts m join #MissingContractQuarters q on m.ContractNumber = q.ContractNumber

		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting final join for My Missing Sales report.'
		goto ERROREXIT
	END	
		
	if @QuartersToInclude = '1'
	BEGIN
	
		select m.ContractNumber,
			m.MissingQuarterId,
			y.Title as 'YearQuarterDescription',
			m.VendorName,
			m.EffectiveDate,
			m.EffectiveQuarterId,
			m.ExpirationDate,
			m.ContractAdministrator,
			m.ContractAdministratorPhone,
			m.ContractAdministratorEmail,
			m.SalesAdministrator,
			m.SalesAdministratorPhone,
			m.SalesAdministratorEmail,
			m.COId,
			m.ContractingOfficerName,
			m.ScheduleNumber,
			m.ScheduleName,
			m.DivisionId

		from #MyMissingSalesReport m join tlkup_year_qtr y  on m.MissingQuarterId = y.Quarter_ID
		where m.MissingQuarterId = @reportingQuarterId or m.MissingQuarterId = @reportingQuarterId - 1
	END
	else
	BEGIN
	
		select m.ContractNumber,
			m.MissingQuarterId,
			y.Title as 'YearQuarterDescription',
			m.VendorName,
			m.EffectiveDate,
			m.EffectiveQuarterId,
			m.ExpirationDate,
			m.ContractAdministrator,
			m.ContractAdministratorPhone,
			m.ContractAdministratorEmail,
			m.SalesAdministrator,
			m.SalesAdministratorPhone,
			m.SalesAdministratorEmail,
			m.COId,
			m.ContractingOfficerName,
			m.ScheduleNumber,
			m.ScheduleName,
			m.DivisionId

		from #MyMissingSalesReport m join tlkup_year_qtr y  on m.MissingQuarterId = y.Quarter_ID
		
	END
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting My Missing Sales report results.'
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



