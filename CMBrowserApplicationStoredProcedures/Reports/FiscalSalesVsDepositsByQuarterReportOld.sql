IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'FiscalSalesVsDepositsByQuarterReport')
	BEGIN
		DROP  Procedure  FiscalSalesVsDepositsByQuarterReport
	END

GO

CREATE Procedure FiscalSalesVsDepositsByQuarterReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@StartingYear int,
@StartingQuarter int,
@EndingYear int,
@EndingQuarter int,
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int, /* allow for -1 = all NAC, however not currently allowing on report parm list */
@Difference nvarchar(20)  /* All Positive Negative Both */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@StartingQuarterId int,
		@EndingQuarterId int,
		@query1 nvarchar(4000),
		@SQLParms nvarchar(1000),
		@whereSchedule nvarchar(400),
		@whereDivisionExclusion nvarchar(430),
		@orderBy nvarchar(400),
		@groupByString nvarchar(400),
		@joinSecurityServerName nvarchar(1000)
		
BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Fiscal Sales Vs Deposits By Quarter Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @StartingQuarterId = Quarter_ID
		from tlkup_year_qtr
		where Year = @StartingYear
		and Qtr = @StartingQuarter
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting starting quarter id.'
		goto ERROREXIT
	END	
		
	select @EndingQuarterId = Quarter_ID
		from tlkup_year_qtr
		where Year = @EndingYear
		and Qtr = @EndingQuarter		

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting ending quarter id.'
		goto ERROREXIT
	END	
	
	create table #FiscalSalesVsDepositsByQuarterReport
	(
		CntrctNum			nvarchar(50)	NULL,
		COID				int				NULL,
		COFullName			nvarchar(80)	NULL,
		COLastName			nvarchar(40)	NULL,
		Schedule_Number		int				NULL,
		Division			int				NULL,
		Short_Sched_Name	nvarchar(12)	NULL,
		Contractor_Name		nvarchar(75)	NULL,
		Quarter_ID			int				NULL,
		Year				nvarchar(4)		NULL,
		Qtr					nvarchar(1)		NULL,
		YearQtr				nvarchar(20)	NULL,
		VAIFF				money			NULL,
		OGAIFF				money			NULL,
		SLGIFF				money			NULL,
		TotalIFF			money			NULL,
		CheckID				int				NULL,
		CheckAmt			money			NULL,
		Difference			money			NULL,
		DateReceived		datetime		NULL,
		CheckNum			nvarchar(50)	NULL,
		DepositNum			varchar(50)		NULL,
		Comments			nvarchar(255)	NULL,
		CheckSequence		int				NULL,
		VA					money			NULL,
		OGA					money			NULL,
		SLG					money			NULL,
		TotalSales			money			NULL,
		NoSalesReported		bit				NULL
	)

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table for report.'
		goto ERROREXIT
	END	

	-- contracts and sales
	insert into #FiscalSalesVsDepositsByQuarterReport
	( CntrctNum, COID, Schedule_Number, Contractor_Name, VA, OGA, SLG, TotalSales, Quarter_ID, NoSalesReported )
	select 	s.CntrctNum, c.CO_ID, c.Schedule_Number, c.Contractor_Name,
		sum(s.VA_Sales), 
		sum(s.OGA_Sales), 
		sum(s.SLG_Sales), 
		sum(s.VA_Sales) + sum(s.OGA_Sales) + sum(s.SLG_Sales), 
		s.Quarter_ID,
		0 as NoSalesReported 
	from  tbl_Cntrcts_Sales s join tbl_Cntrcts c on s.CntrctNum = c.CntrctNum
	where s.Quarter_ID between @StartingQuarterId and @EndingQuarterId
	group by s.CntrctNum, c.CO_ID, c.Schedule_Number, c.Contractor_Name, s.Quarter_ID

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding sales to results.'
		goto ERROREXIT
	END	

	-- iff 
	update #FiscalSalesVsDepositsByQuarterReport
	set VAIFF = ROUND( ( m.VA * i.VA_IFF ) , 2 ), 
		OGAIFF = ROUND( ( m.OGA * i.OGA_IFF ) , 2 ), 
		SLGIFF = ROUND( ( m.SLG * i.SLG_IFF ), 2 ), 
		TotalIFF = ROUND( ( m.VA * i.VA_IFF ) + ( m.OGA * i.OGA_IFF ) + ( m.SLG * i.SLG_IFF ), 2 )
	from #FiscalSalesVsDepositsByQuarterReport m join tbl_IFF i on m.Quarter_ID between i.Start_Quarter_Id and i.End_Quarter_Id 
									and i.Schedule_Number = m.Schedule_Number
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding iff to results.'
		goto ERROREXIT
	END	

	-- checks for those with sales
	update #FiscalSalesVsDepositsByQuarterReport
	set CheckID = k.ID,
		CheckAmt = isnull( k.CheckAmt, 0 ),
		Difference = isnull( k.CheckAmt - m.TotalIFF, 0 ),
		DateReceived = k.DateRcvd,
		CheckNum =  k.CheckNum,
		DepositNum = k.DepositNum,
		Comments = k.Comments,
		CheckSequence =	1
	from  #FiscalSalesVsDepositsByQuarterReport m join tbl_Cntrcts_Checks k on k.CntrctNum = m.CntrctNum
													and k.Quarter_ID = m.Quarter_ID
	where k.ID = ( select min(c.ID) from tbl_Cntrcts_Checks c 
						where c.CntrctNum = m.CntrctNum 
						and c.Quarter_ID = m.Quarter_ID 
						and c.CntrctNum = k.CntrctNum
						and c.Quarter_ID = k.Quarter_ID )

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding checks (1) to results.'
		goto ERROREXIT
	END	

	-- checks for those without sales
		insert into #FiscalSalesVsDepositsByQuarterReport
	( CntrctNum, COID, Schedule_Number, Contractor_Name, VA, OGA, SLG, TotalSales, Quarter_ID, NoSalesReported, 
		CheckID, CheckAmt, Difference, DateReceived, CheckNum, DepositNum, Comments, CheckSequence )
	select k.CntrctNum, c.CO_ID, c.Schedule_Number, c.Contractor_Name, 0, 0, 0, 0, k.Quarter_ID, 1,
		k.ID,
		isnull( k.CheckAmt, 0 ),
		isnull( k.CheckAmt, 0 ),  -- no expected IFF on these records
		k.DateRcvd,
		k.CheckNum,
		k.DepositNum,
		k.Comments,
		2
	from tbl_Cntrcts_Checks k join tbl_Cntrcts c on k.CntrctNum = c.CntrctNum
	where k.Quarter_ID between @StartingQuarterId and @EndingQuarterId
	and k.ID not in ( select m.CheckID from #FiscalSalesVsDepositsByQuarterReport m where m.CheckID is not null )
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding checks (2) to results.'
		goto ERROREXIT
	END	

	-- misc extra fields for report
	update #FiscalSalesVsDepositsByQuarterReport
		set Short_Sched_Name = s.Short_Sched_Name,
		Division = s.Division,
		Year = q.Year,
		Qtr = q.Qtr,
		YearQtr = q.Title,
		COFullName = u.FullName,
		COLastName = u.LastName
	from [tlkup_Sched/Cat] s join #FiscalSalesVsDepositsByQuarterReport m on s.Schedule_Number = m.Schedule_Number
	join tlkup_year_qtr q on m.Quarter_ID = q.Quarter_ID
	join tlkup_UserProfile u on m.COID = u.CO_ID

	-- old way of getting sequence saved for reference
	-- ROW_NUMBER() OVER( PARTITION BY c.CntrctNum, q.Quarter_ID  ORDER BY c.CntrctNum, q.Quarter_ID ) as ''CheckSequence'',
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding misc extra fields for report.'
		goto ERROREXIT
	END	
	
	if @ScheduleNumber <> -1
	BEGIN
		delete #FiscalSalesVsDepositsByQuarterReport where Schedule_Number <> @ScheduleNumber 
	END
	else
	BEGIN
		if @DivisionId <> -1
		BEGIN
			delete #FiscalSalesVsDepositsByQuarterReport where Division <> @DivisionId  -- leave only the selected division
		END
		else
		BEGIN
			delete #FiscalSalesVsDepositsByQuarterReport where Division = 6 -- All NAC excludes SAC 
		END
	END	

	if @DivisionId = 2
	BEGIN
		delete #FiscalSalesVsDepositsByQuarterReport where CntrctNum in ( 'VA797-P-0191', 'VA797P-12-D-0001', 'VA797P-12-D-0011' )
	END
	
	if @Difference = 'All'
	BEGIN
		select CntrctNum,  -- 2
			COID,
			COFullName,  -- 3
			COLastName,
			Schedule_Number	,
			Division,
			Short_Sched_Name,  --4
			Contractor_Name,  --5
			Quarter_ID,
			Year,
			Qtr	,
			YearQtr	,  -- 1
			VAIFF,
			OGAIFF,
			SLGIFF	,
			TotalIFF,  --9
			CheckID	,
			CheckAmt,  --10
			Difference	,  --11
			DateReceived,
			CheckNum,
			DepositNum	,
			Comments,
			CheckSequence,
			VA	, -- 6
			OGA	,  -- 7
			SLG	,  -- 8
			TotalSales,  --5
			NoSalesReported	
		from #FiscalSalesVsDepositsByQuarterReport 
		where TotalSales > 0 or CheckAmt > 0
		order by CntrctNum asc, YearQtr desc

		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting contracts for Fiscal Sales Vs Deposits By Quarter Report(1).'
			goto ERROREXIT
		END
	END
	else if @Difference = 'Positive'
	BEGIN
		select CntrctNum,  -- 2
			COID,
			COFullName,  -- 3
			COLastName,
			Schedule_Number	,
			Division,
			Short_Sched_Name,  --4
			Contractor_Name,  --5
			Quarter_ID,
			Year,
			Qtr	,
			YearQtr	,  -- 1
			VAIFF,
			OGAIFF,
			SLGIFF	,
			TotalIFF,  --9
			CheckID	,
			CheckAmt,  --10
			Difference	,  --11
			DateReceived,
			CheckNum,
			DepositNum	,
			Comments,
			CheckSequence,
			VA	, -- 6
			OGA	,  -- 7
			SLG	,  -- 8
			TotalSales,  --5
			NoSalesReported	
		from #FiscalSalesVsDepositsByQuarterReport 
		where ( TotalSales > 0 or CheckAmt > 0 )
		and Difference > 0
		order by CntrctNum asc, YearQtr desc

		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting contracts for Fiscal Sales Vs Deposits By Quarter Report(2).'
			goto ERROREXIT
		END
	END
	else if @Difference = 'Negative'
	BEGIN
		select CntrctNum,  -- 2
			COID,
			COFullName,  -- 3
			COLastName,
			Schedule_Number	,
			Division,
			Short_Sched_Name,  --4
			Contractor_Name,  --5
			Quarter_ID,
			Year,
			Qtr	,
			YearQtr	,  -- 1
			VAIFF,
			OGAIFF,
			SLGIFF	,
			TotalIFF,  --9
			CheckID	,
			CheckAmt,  --10
			Difference	,  --11
			DateReceived,
			CheckNum,
			DepositNum	,
			Comments,
			CheckSequence,
			VA	, -- 6
			OGA	,  -- 7
			SLG	,  -- 8
			TotalSales,  --5
			NoSalesReported	
		from #FiscalSalesVsDepositsByQuarterReport 
		where ( TotalSales > 0 or CheckAmt > 0 )
		and Difference < 0
		order by CntrctNum asc, YearQtr desc

		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting contracts for Fiscal Sales Vs Deposits By Quarter Report(3).'
			goto ERROREXIT
		END
	END
	else if @Difference = 'Both'
	BEGIN
		select CntrctNum,  -- 2
			COID,
			COFullName,  -- 3
			COLastName,
			Schedule_Number	,
			Division,
			Short_Sched_Name,  --4
			Contractor_Name,  --5
			Quarter_ID,
			Year,
			Qtr	,
			YearQtr	,  -- 1
			VAIFF,
			OGAIFF,
			SLGIFF	,
			TotalIFF,  --9
			CheckID	,
			CheckAmt,  --10
			Difference	,  --11
			DateReceived,
			CheckNum,
			DepositNum	,
			Comments,
			CheckSequence,
			VA	, -- 6
			OGA	,  -- 7
			SLG	,  -- 8
			TotalSales,  --5
			NoSalesReported	
		from #FiscalSalesVsDepositsByQuarterReport 
		where ( TotalSales > 0 or CheckAmt > 0 )
		and Difference <> 0 
		order by CntrctNum asc, YearQtr desc

		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting contracts for Fiscal Sales Vs Deposits By Quarter Report(4).'
			goto ERROREXIT
		END
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


