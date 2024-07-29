IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SalesByScheduleReport')
	BEGIN
		DROP  Procedure  SalesByScheduleReport
	END

GO

CREATE Procedure SalesByScheduleReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@StartYear int,
@StartQuarter int, 
@EndYear int,
@EndQuarter int,
@DivisionId int, /* may be -1 = all NAC */
@ActiveAll nvarchar(12)  /* may be 'Active' or 'All' */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@startQuarterId int,
		@endQuarterId int,
		@query nvarchar(3200),
		@SQLParms nvarchar(1000),
		@whereDivision nvarchar(100),
		@groupByString nvarchar(400),
		@activeAllClause nvarchar(200),
		@joinSecurityServerName nvarchar(1000),
		@SERVERNAME nvarchar(255)

	


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Sales By Schedule Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @startQuarterId = y.Quarter_ID
	from tlkup_year_qtr y
	where y.Year = @StartYear
	and y.Qtr = @StartQuarter

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting start quarter.'
		goto ERROREXIT
	END
	
	select @endQuarterId = y.Quarter_ID
	from tlkup_year_qtr y
	where y.Year = @EndYear
	and y.Qtr = @EndQuarter

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting end quarter.'
		goto ERROREXIT
	END
	
	-- test for SQL1 usage
	SELECT @SERVERNAME = @@SERVERNAME
	
	if @SERVERNAME is null
	BEGIN
		select @joinSecurityServerName = '[' + @SecurityDatabaseName + ']'
	END
	else
	BEGIN
		select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'
	END
	
	create table #SalesByScheduleReport
	(
		ContractNumber nvarchar(50),
		VendorName nvarchar(75),

		EffectiveDate datetime,
		ExpirationDate datetime,
		ContractAdministrator nvarchar(30),
		ContractAdministratorPhone nvarchar(15),
		ContractAdministratorEmail nvarchar(50),
		VendorAddress1  nvarchar(100),
		VendorAddress2  nvarchar(100),
		VendorCity  nvarchar(20),
		VendorStateCode nvarchar(2),
		VendorZip  nvarchar(10),
	
		COId int,
		ContractingOfficerName nvarchar(50),
		ScheduleNumber int,
		ScheduleName nvarchar(75),
		DivisionId int,
		
		VASales money,
		OGASales money,
		SLGSales money,
		TotalSales money,
	)


	select @query = 'insert into #SalesByScheduleReport
	(
		ContractNumber,
		VendorName,
		
		EffectiveDate,
		ExpirationDate,
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorEmail,
		VendorAddress1,
		VendorAddress2,
		VendorCity,
		VendorStateCode,
		VendorZip,

		COId,
		ContractingOfficerName,
		ScheduleNumber,
		ScheduleName,
		DivisionId,

		VASales,
		OGASales,
		SLGSales,
		TotalSales 
	)
	select
		c.CntrctNum,
		c.Contractor_Name,

		c.Dates_Effective,
		c.Dates_CntrctExp,
		c.POC_Primary_Name,
		c.POC_Primary_Phone,
		c.POC_Primary_Email,
		c.Primary_Address_1,
		c.Primary_Address_2,
		c.Primary_City,
		c.Primary_State,
		c.Primary_Zip,

		c.CO_ID,
		u.FullName,
		c.Schedule_Number,
		t.Schedule_Name,
		t.Division,
		
		sum(s.VA_Sales),
		sum(s.OGA_Sales),
		sum(s.SLG_Sales),
		sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales)

	from tbl_Cntrcts_Sales s join tlkup_year_qtr y
		on s.Quarter_ID = y.Quarter_ID
		join tbl_Cntrcts c
		on s.CntrctNum = c.CntrctNum 
		join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] t
		on c.Schedule_Number = t.Schedule_Number 
		where y.Quarter_ID between @startQuarterId_parm and @endQuarterId_parm '

		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END

	if @ActiveAll= 'Active'
	BEGIN		
		select @activeAllClause = ' and c.Dates_CntrctExp >= getdate()
								and c.Dates_Completion is null '
	END
	else
	BEGIN
		select @activeAllClause = ''
	END

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2'
		goto ERROREXIT
	END

	select @groupByString = ' group by c.CntrctNum, c.Contractor_Name, c.Dates_Effective, c.Dates_CntrctExp, c.POC_Primary_Name, c.POC_Primary_Phone,
		c.POC_Primary_Email, c.Primary_Address_1, c.Primary_Address_2, c.Primary_City, c.Primary_State, c.Primary_Zip, c.CO_ID, u.FullName, c.Schedule_Number, t.Schedule_Name, t.Division ' 
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 3'
		goto ERROREXIT
	END
		
	if @DivisionId <> -1
	BEGIN
		select @whereDivision = ' and t.Division = ' + convert( nvarchar(10), @DivisionId )
	END
	else
	BEGIN
		select @whereDivision = ' and t.Division <> 6 ' -- All NAC excludes SAC  
	END	
	
	select @query = @query + @activeAllClause + @whereDivision + @groupByString
	
	select @SQLParms = N'@startQuarterId_parm int, @endQuarterId_parm int'

	exec SP_EXECUTESQL @query, @SQLParms, @startQuarterId_parm = @startQuarterId, @endQuarterId_parm = @endQuarterId

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales for sales by schedule report.'
		goto ERROREXIT
	END


	
	select ContractNumber,
		VendorName,
		EffectiveDate,
		ExpirationDate,
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorEmail,
		VendorAddress1,
		VendorAddress2,
		VendorCity,
		VendorStateCode,
		VendorZip,
		COId,
		ScheduleNumber,
		ScheduleName,
		DivisionId,
		
		VASales,
		OGASales,
		SLGSales,
		TotalSales
	
		from #SalesByScheduleReport
		order by DivisionId, ScheduleName
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales by schedule report results.'
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


