IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'FOIAReport')
	BEGIN
		DROP  Procedure  FOIAReport
	END

GO

CREATE Procedure FOIAReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@Year int,
@Quarter int,    /* may be -1 all */
@ContractNumber nvarchar(20),  /* may be blank = all */
@SIN nvarchar(20),  /* may have "All" = all */
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int /* may be -1 = all NAC */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3000),
		@whereQuarter nvarchar(100),
		@whereContracts nvarchar(100),
		@whereSchedule nvarchar(100),
		@whereSIN nvarchar(100),
		@yearString nvarchar(4),
		@qtrString nvarchar(1)
	


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'FOIA Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	create table #FOIAReport
	(
		ContractNumber nvarchar(50),
		VendorName nvarchar(75),
		Year int,
		Quarter int,
		YearQuarterId int,
		YearQuarterDescription nvarchar(20),
		COId int,
		ScheduleNumber int,
		DivisionId int,
		VASales money,
		OGASales money,
		SLGSales money,
		TotalSales money,
		SpecialItemNumber nvarchar(20),
		SpecialItemNumberDescription varchar(600)
	)


	select @query = 'insert into #FOIAReport
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
		TotalSales ,
		SpecialItemNumber,
		SpecialItemNumberDescription
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
		s.VA_Sales,
		s.OGA_Sales,
		s.SLG_Sales,
		s.VA_Sales + s.OGA_Sales + s.SLG_Sales,		
		s.[SIN],
		n.Description

	from tbl_Cntrcts_Sales s join tlkup_year_qtr y
		on s.Quarter_ID = y.Quarter_ID 
		join tbl_Cntrcts c
		on s.CntrctNum = c.CntrctNum 
		join [tlkup_Sched/Cat] t
		on c.Schedule_Number = t.Schedule_Number 
		join tbl_SINs n
		on s.[SIN] = n.[SIN] 
	where n.[Schedule_ Number] = t.Schedule_Number'
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
			
	select @yearString = convert( nvarchar(4), @Year )
	
	if @Quarter = -1
	BEGIN
		select @whereQuarter = ' and y.Year = ' + @yearString + ' and y.Qtr in ( 1, 2, 3, 4 ) '	
	END
	else
	BEGIN
		select @qtrString = convert( nvarchar(1), @Quarter )
		select @whereQuarter = ' and y.Year =  ' + @yearString + ' and y.Qtr = ' + @qtrString + ' '
	END
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2'
		goto ERROREXIT
	END
	
	if LEN(LTRIM(RTRIM(@ContractNumber))) > 0
	BEGIN
		select @whereContracts = ' and c.CntrctNum = ''' + LTRIM(RTRIM( @ContractNumber )) + ''''
	END
	else
	BEGIN
		select @whereContracts = ' '
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
	
	if LTRIM(RTRIM(@SIN)) <> 'All'
	BEGIN
		select @whereSIN = ' and n.[SIN] = ''' + LTRIM(RTRIM(@SIN)) + ''''
	END
	else
	BEGIN
		select @whereSIN = ' '
	END	
	
	select @query = @query + @whereQuarter + @whereContracts + @whereSchedule + @whereSIN
	
	exec SP_EXECUTESQL @query 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales for FOIA report.'
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
		TotalSales ,
		SpecialItemNumber,
		SpecialItemNumberDescription	
		from #FOIAReport
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting FOIA report results.'
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


