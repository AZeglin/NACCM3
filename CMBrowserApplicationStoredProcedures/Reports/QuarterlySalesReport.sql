IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'QuarterlySalesReport')
	BEGIN
		DROP  Procedure  QuarterlySalesReport
	END

GO

CREATE Procedure QuarterlySalesReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@Year int,
@Quarter int,    /* may be -1 all */
@ContractingOfficerId int,  /* may be -1 all */
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int /* may be -1 = all NAC */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3200),
		@whereQuarter nvarchar(100),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@yearString nvarchar(4),
		@qtrString nvarchar(1),
		@groupByString nvarchar(200)
	


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Quarterly Sales Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #QuarterlySalesReport
	(
		ContractNumber nvarchar(50),
		VendorName nvarchar(75),
		Year int,
		Quarter int,
		YearQuarterId int,
		YearQuarterDescription nvarchar(20),
		YearQuarterIdForPreviousQuarter int,
		YearQuarterIdForOneYearAgoQuarter int,
	
		COId int,
		ScheduleNumber int,
		DivisionId int,
		
		VASales money,
		OGASales money,
		SLGSales money,
		TotalSales money,
		
		VAIFF money,
		OGAIFF money,
		SLGIFF money,
		TotalIFF money,
		
		VASalesForPreviousQuarter money,
		OGASalesForPreviousQuarter money,
		SLGSalesForPreviousQuarter money,
		TotalSalesForPreviousQuarter money,

		VASalesForYearAgoQuarter money,
		OGASalesForYearAgoQuarter money,
		SLGSalesForYearAgoQuarter money,
		TotalSalesForYearAgoQuarter money
	)


	select @query = 'insert into #QuarterlySalesReport
	(
		ContractNumber,
		VendorName,
		Year,
		Quarter,
		YearQuarterId,
		YearQuarterDescription,
		COId,
		ScheduleNumber,
		DivisionId,
		VASales,
		OGASales,
		SLGSales,
		TotalSales 
	)
	select
		c.CntrctNum,
		c.Contractor_Name,
		y.Year,
		y.Qtr,
		y.Quarter_ID,
		y.Title,
		c.CO_ID,
		c.Schedule_Number,
		t.Division,
		sum(s.VA_Sales),
		sum(s.OGA_Sales),
		sum(s.SLG_Sales),
		sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales)

	from tbl_Cntrcts_Sales s join tlkup_year_qtr y
		on s.Quarter_ID = y.Quarter_ID 
		join tbl_Cntrcts c
		on s.CntrctNum = c.CntrctNum 
		join [tlkup_Sched/Cat] t
		on c.Schedule_Number = t.Schedule_Number '
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
			
	select @yearString = convert( nvarchar(4), @Year )
	
	/* yearly */
	if @Quarter = -1
	BEGIN
		select @whereQuarter = ' where y.Year = ' + @yearString + ' and y.Qtr in ( 1, 2, 3, 4 ) '	
		select @groupByString = ' group by y.Quarter_ID, c.CntrctNum, c.Contractor_Name, y.Year, y.Qtr, y.Title, c.CO_ID, c.Schedule_Number, t.Division '
	END
	else /* quarterly */
	BEGIN
		select @qtrString = convert( nvarchar(1), @Quarter )
		select @whereQuarter = ' where y.Year =  ' + @yearString + ' and y.Qtr = ' + @qtrString + ' '
		select @groupByString = ' group by c.CntrctNum, c.Contractor_Name, y.Quarter_ID, y.Year, y.Qtr,	y.Title, c.CO_ID, c.Schedule_Number, t.Division '
	END
	
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
	
	if @ContractingOfficerId <> -1
	BEGIN
		select @whereContractingOfficer = ' and c.CO_ID = ' + convert( nvarchar(10), @ContractingOfficerId )
	END
	else
	BEGIN
		select @whereContractingOfficer = ' ' 
	END
	
	
	select @query = @query + @whereQuarter + @whereSchedule + @whereContractingOfficer + @groupByString
	
	exec SP_EXECUTESQL @query 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales for quarterly sales report.'
		goto ERROREXIT
	END

	/* backfill previous quarters */
	update #QuarterlySalesReport
	set YearQuarterIdForPreviousQuarter = YearQuarterId - 1,
		YearQuarterIdForOneYearAgoQuarter = YearQuarterId - 4
	from #QuarterlySalesReport
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error backfilling previous quarter ids for quarterly sales report.'
		goto ERROREXIT
	END

	update #QuarterlySalesReport
	set VASalesForPreviousQuarter = ( select sum(s.VA_Sales) 
												from tbl_Cntrcts_Sales s 
													where s.Quarter_ID = r.YearQuarterIdForPreviousQuarter 
													and s.CntrctNum = r.ContractNumber ),
		OGASalesForPreviousQuarter = ( select sum(s.OGA_Sales)
												from tbl_Cntrcts_Sales s 
													where s.Quarter_ID = r.YearQuarterIdForPreviousQuarter 
													and s.CntrctNum = r.ContractNumber ),
		SLGSalesForPreviousQuarter = ( select sum(s.SLG_Sales)
												from tbl_Cntrcts_Sales s 
													where s.Quarter_ID = r.YearQuarterIdForPreviousQuarter 
													and s.CntrctNum = r.ContractNumber ),
		TotalSalesForPreviousQuarter = ( select sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales)
												from tbl_Cntrcts_Sales s
													where s.Quarter_ID = r.YearQuarterIdForPreviousQuarter 
													and s.CntrctNum = r.ContractNumber )
		
	from tbl_Cntrcts_Sales s join #QuarterlySalesReport r
		on s.Quarter_ID = r.YearQuarterIdForPreviousQuarter 
		and s.CntrctNum = r.ContractNumber

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error backfilling previous quarter sales for quarterly sales report.'
		goto ERROREXIT
	END
	
	update #QuarterlySalesReport
	set VASalesForYearAgoQuarter = ( select sum(s.VA_Sales)
	 									from tbl_Cntrcts_Sales s 
										where s.Quarter_ID = r.YearQuarterIdForOneYearAgoQuarter 
										and s.CntrctNum = r.ContractNumber ),

		OGASalesForYearAgoQuarter = ( select sum(s.OGA_Sales)
	 									from tbl_Cntrcts_Sales s
										where s.Quarter_ID = r.YearQuarterIdForOneYearAgoQuarter 
										and s.CntrctNum = r.ContractNumber ),
		
		SLGSalesForYearAgoQuarter = ( select sum(s.SLG_Sales)
	 									from tbl_Cntrcts_Sales s 
										where s.Quarter_ID = r.YearQuarterIdForOneYearAgoQuarter 
										and s.CntrctNum = r.ContractNumber ),
		
		TotalSalesForYearAgoQuarter = ( select sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales)
	 									from tbl_Cntrcts_Sales s 
										where s.Quarter_ID = r.YearQuarterIdForOneYearAgoQuarter 
										and s.CntrctNum = r.ContractNumber )
		
	from tbl_Cntrcts_Sales s join #QuarterlySalesReport r
		on s.Quarter_ID = r.YearQuarterIdForOneYearAgoQuarter 
		and s.CntrctNum = r.ContractNumber

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error backfilling previous year sales for quarterly sales report.'
		goto ERROREXIT
	END


	update #QuarterlySalesReport
		set VAIFF = VASales * i.VA_IFF,
		OGAIFF = OGASales * i.OGA_IFF,
		SLGIFF = SLGSales * i.SLG_IFF,
		TotalIFF = ( VASales * i.VA_IFF ) + ( OGASales * i.OGA_IFF ) + ( SLGSales * i.SLG_IFF )
	from tbl_IFF i join #QuarterlySalesReport r
	on i.Schedule_Number = r.ScheduleNumber
	and r.YearQuarterId between i.Start_Quarter_Id and End_Quarter_Id
															
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting IFF for quarterly sales report.'
		goto ERROREXIT
	END
	
	select ContractNumber,
		VendorName,
		Year,
		Quarter,
		YearQuarterId,
		YearQuarterDescription,
		COId,
		ScheduleNumber,
		DivisionId,
		VASales,
		OGASales,
		SLGSales,
		TotalSales,
		VAIFF,
		OGAIFF,
		SLGIFF,
		TotalIFF,
		VASalesForPreviousQuarter,
		OGASalesForPreviousQuarter,
		SLGSalesForPreviousQuarter,
		TotalSalesForPreviousQuarter,

		VASalesForYearAgoQuarter,
		OGASalesForYearAgoQuarter,
		SLGSalesForYearAgoQuarter,
		TotalSalesForYearAgoQuarter 
		from #QuarterlySalesReport
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting quarterly sales report results.'
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


