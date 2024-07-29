IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'QuarterlySalesByBusinessTypeReport')
	BEGIN
		DROP  Procedure  QuarterlySalesByBusinessTypeReport
	END

GO

CREATE Procedure QuarterlySalesByBusinessTypeReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@StartYear int,
@StartQuarter int, 
@EndYear int,
@EndQuarter int,
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int /* may be -1 = all NAC */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@startQuarterId int,
		@endQuarterId int,
		@query nvarchar(4000),
		@SQLParms nvarchar(1000),
		@whereSchedule nvarchar(100),
		@groupByString nvarchar(1000),
		@joinSecurityServerName nvarchar(1000),
		@SERVERNAME nvarchar(255),
		@TotalSales decimal(15,2)


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Quarterly Sales By Business Type', '2'
	
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
	
	create table #QuarterlySalesByBusinessType
	(
		ContractNumber nvarchar(50),
		VendorName nvarchar(75),

		EffectiveDate datetime,
		ExpirationDate datetime,
		VendorStateCode nvarchar(2),
	
		COId int,
		ContractingOfficerName nvarchar(50),
		ScheduleNumber int,
		ScheduleName nvarchar(75),
		DivisionId int,
		
		HubZone bit,
		WomanOwnedSmallBusiness bit,
		EightA bit,
		SDB bit,
		VeteranOwned bit,
		DisabledVeteranOwned bit,
		SmallBusiness bit, 
		
		
		VASales money,
		OGASales money,
		SLGSales money,
		TotalSales money,
	)


	select @query = 'insert into #QuarterlySalesByBusinessType
	(
		ContractNumber,
		VendorName,
		
		EffectiveDate,
		ExpirationDate,
		VendorStateCode,

		COId,
		ContractingOfficerName,
		ScheduleNumber,
		ScheduleName,
		DivisionId,
		
		HubZone,
		WomanOwnedSmallBusiness,
		EightA,
		SDB,
		VeteranOwned,
		DisabledVeteranOwned,
		SmallBusiness, 
		
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
		c.Primary_State,

		c.CO_ID,
		u.FullName,
		c.Schedule_Number,
		t.Schedule_Name,
		t.Division,
		
		c.Socio_HubZone,
		c.Socio_Woman,
		c.Socio_8a,
		c.Socio_SDB,
		case when c.Socio_VetStatus_ID = 1 then 1 else 0 end as VeteranOwned,
		case when c.Socio_VetStatus_ID = 3 then 1 else 0 end as DisabledVeteranOwned,
		case when c.Socio_Business_Size_ID = 1 then 1 else 0 end as SmallBusiness,

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
				
	select @groupByString = ' group by c.CntrctNum, c.Contractor_Name, c.Dates_Effective, c.Dates_CntrctExp, c.POC_Primary_Name, c.Primary_State, c.CO_ID, u.FullName, c.Schedule_Number, t.Schedule_Name, t.Division, c.Socio_HubZone, c.Socio_Woman, c.Socio_8a, c.Socio_SDB, c.Socio_VetStatus_ID, c.Socio_Business_Size_ID ' 
	
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
			select @whereSchedule = ' and t.Division = ' + convert( nvarchar(10), @DivisionId )
		END
		else
		BEGIN
			select @whereSchedule = ' and t.Division <> 6 ' -- All NAC excludes SAC 
		END
	END	
	
	select @query = @query + @whereSchedule + @groupByString
	
	select @SQLParms = N'@startQuarterId_parm int, @endQuarterId_parm int'

	exec SP_EXECUTESQL @query, @SQLParms, @startQuarterId_parm = @startQuarterId, @endQuarterId_parm = @endQuarterId

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting quarterly sales by business type report.'
		goto ERROREXIT
	END


	/*
		Total Sales
		HubZone,
		WomanOwnedSmallBusiness,
		EightA,
		SDB,
		VeteranOwned,
		DisabledVeteranOwned,
		SmallBusiness, 
	*/
	
	select @TotalSales = SUM(TotalSales)
		from #QuarterlySalesByBusinessType
		
	select 'Total Sales' as SalesCategory,		
		SUM(VASales) as VASales,
		SUM(OGASales) as OGASales,
		SUM(SLGSales) as SLGSales,
		SUM(TotalSales) as TotalSales,
		1 as PercentageOfTotalSales,
		1 as SortOrder
		from #QuarterlySalesByBusinessType
		
		union
		
		select 'Small Business' as SalesCategory,
		SUM(VASales) as VASales,
		SUM(OGASales) as OGASales,
		SUM(SLGSales) as SLGSales,
		SUM(TotalSales) as TotalSales,
		SUM(TotalSales)/@TotalSales as PercentageOfTotalSales,
		2 as SortOrder
		from #QuarterlySalesByBusinessType
		where SmallBusiness = 1
		
		union
		
		select 'Women Owned Small Business' as SalesCategory,
		SUM(VASales) as VASales,
		SUM(OGASales) as OGASales,
		SUM(SLGSales) as SLGSales,
		SUM(TotalSales) as TotalSales,
		SUM(TotalSales)/@TotalSales as PercentageOfTotalSales,
		3 as SortOrder
		from #QuarterlySalesByBusinessType
		where WomanOwnedSmallBusiness = 1
		
		union
		
		select 'Small Disadvantaged Business' as SalesCategory,
		SUM(VASales) as VASales,
		SUM(OGASales) as OGASales,
		SUM(SLGSales) as SLGSales,
		SUM(TotalSales) as TotalSales,
		SUM(TotalSales)/@TotalSales as PercentageOfTotalSales,
		4 as SortOrder
		from #QuarterlySalesByBusinessType
		where SDB = 1
		
		union
		
		select '8(a)' as SalesCategory,
		SUM(VASales) as VASales,
		SUM(OGASales) as OGASales,
		SUM(SLGSales) as SLGSales,
		SUM(TotalSales) as TotalSales,
		SUM(TotalSales)/@TotalSales as PercentageOfTotalSales,
		5 as SortOrder
		from #QuarterlySalesByBusinessType
		where EightA = 1
		
		union
		
		select 'Hub Zone' as SalesCategory,
		SUM(VASales) as VASales,
		SUM(OGASales) as OGASales,
		SUM(SLGSales) as SLGSales,
		SUM(TotalSales) as TotalSales,
		SUM(TotalSales)/@TotalSales as PercentageOfTotalSales,
		6 as SortOrder
		from #QuarterlySalesByBusinessType
		where HubZone = 1
		
		union	
		
		select 'Veteran Owned' as SalesCategory,
		SUM(VASales) as VASales,
		SUM(OGASales) as OGASales,
		SUM(SLGSales) as SLGSales,
		SUM(TotalSales) as TotalSales,
		SUM(TotalSales)/@TotalSales as PercentageOfTotalSales,
		7 as SortOrder
		from #QuarterlySalesByBusinessType
		where VeteranOwned = 1 or DisabledVeteranOwned = 1
		
		union
		
		select 'Disabled Veteran Owned' as SalesCategory,
		SUM(VASales) as VASales,
		SUM(OGASales) as OGASales,
		SUM(SLGSales) as SLGSales,
		SUM(TotalSales) as TotalSales,
		SUM(TotalSales)/@TotalSales as PercentageOfTotalSales,
		8 as SortOrder
		from #QuarterlySalesByBusinessType
		where DisabledVeteranOwned = 1
		
		order by SortOrder
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting quarterly sales by business type report results.'
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


