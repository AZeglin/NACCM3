IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'MonthlyAwardsByBusinessTypeReport')
	BEGIN
		DROP  Procedure  MonthlyAwardsByBusinessTypeReport
	END

GO

CREATE Procedure MonthlyAwardsByBusinessTypeReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@StartYear int,
@StartMonth int, 
@EndYear int,
@EndMonth int,
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int /* may be -1 = all NAC */
)

AS

/* draft only, need to edit out sales and edit in awards */

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@startDate datetime,
		@endDate datetime,
		@query nvarchar(4000),
		@SQLParms nvarchar(1000),
		@whereSchedule nvarchar(100),
		@groupByString nvarchar(1000),
		@joinSecurityServerName nvarchar(1000),
		@SERVERNAME nvarchar(255),
		@TotalAwards decimal(15,2)


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Monthly Awards By Business Type', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @startDate = convert( datetime, convert( nvarchar(2), @StartMonth ) + '/1/' + convert( nvarchar(4), @StartYear ))

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting start date.'
		goto ERROREXIT
	END
	
	select @endDate = convert( datetime, convert( nvarchar(2), @EndMonth ) + '/' + convert( nvarchar(2), dbo.GetLastDateOfMonthFunction( @EndMonth, @EndYear ) ) + '/' + convert( nvarchar(4), @EndYear ))

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting end date.'
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
	
	create table #MonthlyAwardsByBusinessType
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
		SmallBusiness bit

	)


	select @query = 'insert into #MonthlyAwardsByBusinessType
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
		SmallBusiness
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
		case when c.Socio_Business_Size_ID = 1 then 1 else 0 end as SmallBusiness

	from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] t
		on c.Schedule_Number = t.Schedule_Number 
		where c.Dates_CntrctAward between @startDate_parm and @endDate_parm '
		
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
	
	select @SQLParms = N'@startDate_parm datetime, @endDate_parm datetime'

	exec SP_EXECUTESQL @query, @SQLParms, @startDate_parm = @StartDate, @endDate_parm = @EndDate

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting monthly awards by business type report.'
		goto ERROREXIT
	END


	/*
		Total Awards
		HubZone,
		WomanOwnedSmallBusiness,
		EightA,
		SDB,
		VeteranOwned,
		DisabledVeteranOwned,
		SmallBusiness, 
	*/
	
	select @TotalAwards = COUNT(*)
		from #MonthlyAwardsByBusinessType
		
	select 'Total Awards' as AwardCategory,		
		convert( int, @TotalAwards ) as AwardCount,
		1 as PercentageOfTotalAwards,
		1 as SortOrder
		from #MonthlyAwardsByBusinessType
		
		union
		
		select 'Small Business' as AwardCategory,
		count(*) as AwardCount,
		count(*)/@TotalAwards  as PercentageOfTotalAwards,
		2 as SortOrder
		from #MonthlyAwardsByBusinessType
		where SmallBusiness = 1
		
		union
		
		select 'Women Owned Small Business' as SalesCategory,
		count(*) as AwardCount,
		count(*)/@TotalAwards as PercentageOfTotalAwards,
		3 as SortOrder
		from #MonthlyAwardsByBusinessType
		where WomanOwnedSmallBusiness = 1
		
		union
		
		select 'Small Disabled Owned Business' as SalesCategory,
		count(*) as AwardCount,
		count(*)/@TotalAwards as PercentageOfTotalAwards,
		4 as SortOrder
		from #MonthlyAwardsByBusinessType
		where SDB = 1
		
		union
		
		select '8(a)' as SalesCategory,
		count(*) as AwardCount,
		count(*)/@TotalAwards as PercentageOfTotalAwards,
		5 as SortOrder
		from #MonthlyAwardsByBusinessType
		where EightA = 1
		
		union
		
		select 'Hub Zone' as SalesCategory,
		count(*) as AwardCount,
		count(*)/@TotalAwards as PercentageOfTotalAwards,
		6 as SortOrder
		from #MonthlyAwardsByBusinessType
		where HubZone = 1
		
		union	
		
		select 'Veteran Owned' as SalesCategory,
		count(*) as AwardCount,
		count(*)/@TotalAwards as PercentageOfTotalAwards,
		7 as SortOrder
		from #MonthlyAwardsByBusinessType
		where VeteranOwned = 1 or DisabledVeteranOwned = 1
		
		union
		
		select 'Disabled Veteran Owned' as SalesCategory,
		count(*) as AwardCount,
		count(*)/@TotalAwards as PercentageOfTotalAwards,
		8 as SortOrder
		from #MonthlyAwardsByBusinessType
		where DisabledVeteranOwned = 1
		
		order by SortOrder
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting monthly awards by business type report results.'
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




