IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'FiscalDepositsReport')
	BEGIN
		DROP  Procedure  FiscalDepositsReport
	END

GO

CREATE Procedure FiscalDepositsReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@StartingYear int,
@StartingQuarter int,
@EndingYear int,
@EndingQuarter int,
@DivisionId int
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
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Fiscal Deposits Report', '2'
	
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
	


	--if @DivisionId = 2
	--BEGIN
	--	select c.CntrctNum,
	--			q.Year, 
	--			q.Qtr, 
	--			q.Title as YearQtr,
	--			ROUND( ( m.VA * i.VA_IFF ) , 2 ) as VAIFF, 
	--			ROUND( ( m.OGA * i.OGA_IFF ) , 2 ) as OGAIFF, 
	--			ROUND( ( m.SLG * i.SLG_IFF ), 2 ) as SLGIFF, 
	--			ROUND( ( m.VA * i.VA_IFF ) + ( m.OGA * i.OGA_IFF ) + ( m.SLG * i.SLG_IFF ), 2 ) as TotalIFF, 
	--			isnull( k.CheckAmt, 0 ) as CheckAmt,
	--			isnull( k.CheckAmt - (ROUND(( m.VA * i.VA_IFF ) + ( m.OGA * i.OGA_IFF ) + ( m.SLG * i.SLG_IFF ),2)), 0 ) as Difference,
	--			k.DateRcvd as DateReceived,
	--			k.CheckNum,
	--			k.DepositNum,
	--			ROW_NUMBER() OVER( PARTITION BY c.CntrctNum, q.Quarter_ID  ORDER BY c.CntrctNum, q.Quarter_ID ) as 'CheckSequence',
	--			k.Comments			
	--	from tlkup_year_qtr q join
	--					(
	--					select s.Quarter_ID,  
	--					s.CntrctNum,
	--					sum(s.VA_Sales) as VA, 
	--					sum(s.OGA_Sales) as OGA, 
	--					sum(s.SLG_Sales) as SLG 
	--					from  tbl_Cntrcts_Sales s
	--					where s.Quarter_ID between @StartingQuarterId and @EndingQuarterId
	--					and s.CntrctNum not in ( 'VA797-P-0191', 'VA797P-12-D-0001', 'VA797P-12-D-0011' )
	--					group by s.CntrctNum, s.Quarter_ID
	--					) m
	--	on q.Quarter_ID = m.Quarter_ID

	--	join tbl_Cntrcts c on c.CntrctNum = m.CntrctNum
	--	join tbl_IFF i on m.Quarter_ID between i.Start_Quarter_Id and i.End_Quarter_Id 
	--									and i.Schedule_Number = c.Schedule_Number
	--	join tbl_Cntrcts_Checks k on k.CntrctNum = c.CntrctNum
	--					and k.Quarter_ID = m.Quarter_ID
	--	where c.CntrctNum not in ( 'VA797-P-0191', 'VA797P-12-D-0001', 'VA797P-12-D-0011' )
	--	order by c.CntrctNum asc, q.Title desc
	--END
	--else
	--BEGIN
	--	select c.CntrctNum,
	--			q.Year, 
	--			q.Qtr, 
	--			q.Title as YearQtr,
	--			ROUND( ( m.VA * i.VA_IFF ) , 2 ) as VAIFF, 
	--			ROUND( ( m.OGA * i.OGA_IFF ) , 2 ) as OGAIFF, 
	--			ROUND( ( m.SLG * i.SLG_IFF ), 2 ) as SLGIFF, 
	--			ROUND( ( m.VA * i.VA_IFF ) + ( m.OGA * i.OGA_IFF ) + ( m.SLG * i.SLG_IFF ), 2 ) as TotalIFF, 
	--			isnull( k.CheckAmt, 0 ) as CheckAmt,
	--			isnull( k.CheckAmt - (ROUND(( m.VA * i.VA_IFF ) + ( m.OGA * i.OGA_IFF ) + ( m.SLG * i.SLG_IFF ),2)), 0 ) as Difference,
	--			k.DateRcvd as DateReceived,
	--			k.CheckNum,
	--			k.DepositNum,
	--			ROW_NUMBER() OVER( PARTITION BY c.CntrctNum, q.Quarter_ID  ORDER BY c.CntrctNum, q.Quarter_ID ) as 'CheckSequence',
	--			k.Comments			
	--	from tlkup_year_qtr q join
	--					(
	--					select s.Quarter_ID,  
	--					s.CntrctNum,
	--					sum(s.VA_Sales) as VA, 
	--					sum(s.OGA_Sales) as OGA, 
	--					sum(s.SLG_Sales) as SLG 
	--					from  tbl_Cntrcts_Sales s
	--					where s.Quarter_ID between @StartingQuarterId and @EndingQuarterId
	--					group by s.CntrctNum, s.Quarter_ID
	--					) m
	--	on q.Quarter_ID = m.Quarter_ID

	--	join tbl_Cntrcts c on c.CntrctNum = m.CntrctNum
	--	join tbl_IFF i on m.Quarter_ID between i.Start_Quarter_Id and i.End_Quarter_Id 
	--									and i.Schedule_Number = c.Schedule_Number
	--	join tbl_Cntrcts_Checks k on k.CntrctNum = c.CntrctNum
	--					and k.Quarter_ID = m.Quarter_ID
	--	order by c.CntrctNum asc, q.Title desc
	--END

	create table #FiscalDepositsByQuarterReport
	(
		CntrctNum			nvarchar(50)	NULL,
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
		CheckID				int				NULL,  -- SRPActivityId
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
	insert into #FiscalDepositsByQuarterReport
	( CntrctNum, Schedule_Number, Contractor_Name, VA, OGA, SLG, TotalSales, Quarter_ID, NoSalesReported )
	select 	s.CntrctNum, c.Schedule_Number, c.Contractor_Name,
		sum(s.VA_Sales), 
		sum(s.OGA_Sales), 
		sum(s.SLG_Sales), 
		sum(s.VA_Sales) + sum(s.OGA_Sales) + sum(s.SLG_Sales), 
		s.Quarter_ID,
		0 as NoSalesReported 
	from  tbl_Cntrcts_Sales s join tbl_Cntrcts c on s.CntrctNum = c.CntrctNum
	where s.Quarter_ID between @StartingQuarterId and @EndingQuarterId
	group by s.CntrctNum, c.Schedule_Number, c.Contractor_Name, s.Quarter_ID

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding sales to results.'
		goto ERROREXIT
	END	

	-- iff 
	update #FiscalDepositsByQuarterReport
	set VAIFF = ROUND( ( m.VA * i.VA_IFF ) , 2 ), 
		OGAIFF = ROUND( ( m.OGA * i.OGA_IFF ) , 2 ), 
		SLGIFF = ROUND( ( m.SLG * i.SLG_IFF ), 2 ), 
		TotalIFF = ROUND( ( m.VA * i.VA_IFF ) + ( m.OGA * i.OGA_IFF ) + ( m.SLG * i.SLG_IFF ), 2 )
	from #FiscalDepositsByQuarterReport m join tbl_IFF i on m.Quarter_ID between i.Start_Quarter_Id and i.End_Quarter_Id 
									and i.Schedule_Number = m.Schedule_Number
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding iff to results.'
		goto ERROREXIT
	END	

	-- checks for those with sales
	update #FiscalDepositsByQuarterReport
	set CheckID = k.SRPActivityId,
		CheckAmt = isnull( k.PaymentAmount, 0 ),
		Difference = isnull( k.PaymentAmount - ( m.VAIFF + m.OGAIFF + m.SLGIFF ), 0 ),
		DateReceived = k.CreationDate,
		CheckNum =  k.CheckNumber,
		DepositNum = k.DepositTicketNumber,
		Comments = k.Comments,
		CheckSequence =	1
	from  #FiscalDepositsByQuarterReport m join CM_PaymentsReceived k on k.ContractNumber = m.CntrctNum
													and k.QuarterId = m.Quarter_ID
	where k.SRPActivityId = ( select min(c.SRPActivityId) from CM_PaymentsReceived c 
						where c.ContractNumber = m.CntrctNum 
						and c.QuarterId = m.Quarter_ID 
						and c.ContractNumber = k.ContractNumber
						and c.QuarterId = k.QuarterId )

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding checks (1) to results.'
		goto ERROREXIT
	END	

	-- checks for those without sales
		insert into #FiscalDepositsByQuarterReport
	( CntrctNum, Schedule_Number, Contractor_Name, VA, OGA, SLG, TotalSales, Quarter_ID, NoSalesReported, 
		CheckID, CheckAmt, Difference, DateReceived, CheckNum, DepositNum, Comments, CheckSequence )
	select k.ContractNumber, c.Schedule_Number, c.Contractor_Name, 0, 0, 0, 0, k.QuarterId, 1,
		k.SRPActivityId,
		isnull( k.PaymentAmount, 0 ) as CheckAmt,
		isnull( k.PaymentAmount, 0 ) as Difference,  -- no expected IFF on these records
		k.CreationDate as DateReceived,
		k.CheckNumber as CheckNum,
		k.DepositTicketNumber as DepositNum,
		k.Comments,
		2
	from CM_PaymentsReceived k join tbl_Cntrcts c on k.ContractNumber = c.CntrctNum
	where k.QuarterId between @StartingQuarterId and @EndingQuarterId
	and k.SRPActivityId not in ( select m.CheckID from #FiscalDepositsByQuarterReport m where m.CheckID is not null )
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding checks (2) to results.'
		goto ERROREXIT
	END	

	-- misc extra fields for report
	update #FiscalDepositsByQuarterReport
		set Short_Sched_Name = s.Short_Sched_Name,
		Division = s.Division,
		Year = q.Year,
		Qtr = q.Qtr,
		YearQtr = q.Title
	from [tlkup_Sched/Cat] s join #FiscalDepositsByQuarterReport m on s.Schedule_Number = m.Schedule_Number
	join tlkup_year_qtr q on m.Quarter_ID = q.Quarter_ID

	-- old way of getting sequence saved for reference
	-- ROW_NUMBER() OVER( PARTITION BY c.CntrctNum, q.Quarter_ID  ORDER BY c.CntrctNum, q.Quarter_ID ) as ''CheckSequence'',
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding misc extra fields for report.'
		goto ERROREXIT
	END	
	
	delete #FiscalDepositsByQuarterReport where Division <> @DivisionId 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error trimming recordset for report.'
		goto ERROREXIT
	END	

	if @DivisionId = 2
	BEGIN
		delete #FiscalDepositsByQuarterReport where CntrctNum in ( 'VA797-P-0191', 'VA797P-12-D-0001', 'VA797P-12-D-0011' )
	END
	
	select CntrctNum,  -- 1
		Schedule_Number	,
		Division,
		Short_Sched_Name,  
		Contractor_Name,  
		Quarter_ID,
		Year,
		Qtr	,
		YearQtr	,  -- 2
		VAIFF,  -- 3
		OGAIFF,  -- 4
		SLGIFF	,  -- 5
		TotalIFF,  -- 6
		CheckID	,
		CheckAmt,  -- 7
		Difference	,  -- 8
		DateReceived,  -- 9
		CheckNum, -- 10
		DepositNum	, -- 11
		Comments,  -- 12
		CheckSequence,
		VA	,
		OGA	,
		SLG	,
		TotalSales,  
		NoSalesReported	
	from #FiscalDepositsByQuarterReport 
	where CheckAmt is not null and  CheckAmt > 0
	order by CntrctNum asc, YearQtr desc, DateReceived asc

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for Deposits Report.'
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



