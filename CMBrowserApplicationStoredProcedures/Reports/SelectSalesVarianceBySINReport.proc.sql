IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSalesVarianceBySINReport' )
BEGIN
	DROP PROCEDURE SelectSalesVarianceBySINReport
END
GO

CREATE PROCEDURE SelectSalesVarianceBySINReport
(
@UserLogin nvarchar(120),
@ContractNumber nvarchar(20),
@Year int,
@Qtr int,
@DataSetId int
)

AS

Declare 	@error int,
		@rowCount int,
		@SelectedQuarter_ID int,
		@PreviousQuarter_ID int,
		@PreviousYearQuarter_ID int,
		@errorMsg nvarchar(1000)


/* on screen report obtained by clicking on a year/qtr for a particular contract from the sales summary grid */

BEGIN TRANSACTION

	select @SelectedQuarter_ID = Quarter_ID
	from tlkup_year_qtr y
	where y.Year = CONVERT( nvarchar(4), @Year )
		and y.Qtr = CONVERT( nvarchar(1), @Qtr )
			
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting quarter id for on-screen report sales variance by SIN'
		goto ERROREXIT
	END

	select @PreviousQuarter_ID = @SelectedQuarter_ID - 1,
			@PreviousYearQuarter_ID  = @SelectedQuarter_ID - 4


	IF OBJECT_ID('tempdb..#ContractSalesVarianceBySIN') IS NOT NULL 
	BEGIN
		drop table #ContractSalesVarianceBySIN
	END
	
	create table #ContractSalesVarianceBySIN
	(
		CntrctNum nvarchar(50) NOT NULL,
		Schedule_Number int NOT NULL,
		Quarter_ID  int              NOT NULL,
		Year		nvarchar(4)		not null,
		Qtr			nvarchar(1)		not null,
		Title		nvarchar(20)	not null,
		[SIN]    	varchar(10)      NOT NULL,
		LexicalSIN  varchar(10)		NOT NULL,
		VA_Sales   	money            NOT NULL,
		OGA_Sales 	money            NOT NULL,
		SLG_Sales  	money            NOT NULL,
		Total_Sales money			   not null,
		VA_IFF			decimal(19, 2)	   not null,
		OGA_IFF			decimal(19, 2)	   not null,
		SLG_IFF			decimal(19, 2)	   not null,
		Total_IFF		decimal(19, 2)	    not null,
		VAPercentage		numeric(18, 4)		not null,
		OGAPercentage		numeric(18, 4)    not null,
		SLGPercentage		numeric(18, 4)    not null,
		PreviousQuarter_ID			int			 null,
		PreviousQuarterVA_Sales		money             NULL,
		PreviousQuarterOGA_Sales	money             NULL,
		PreviousQuarterSLG_Sales	money             NULL,
		PreviousQuarterTotal_Sales	money             NULL,
		PreviousYearQuarter_ID		int				  null,
		PreviousYearVA_Sales		money             NULL,
		PreviousYearOGA_Sales		money             NULL,
		PreviousYearSLG_Sales		money             NULL,
		PreviousYearTotal_Sales		money             NULL,
		VarianceQuarterVA numeric(18, 4) NULL,
		VarianceQuarterOGA numeric(18, 4) NULL,
		VarianceQuarterSLG numeric(18, 4) NULL,
		VarianceQuarterTotal numeric(18, 4) NULL,
		VarianceYearVA numeric(18, 4) NULL,
		VarianceYearOGA numeric(18, 4) NULL,
		VarianceYearSLG numeric(18, 4) NULL,
		VarianceYearTotal numeric(18, 4) NULL
	)	
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table for on-screen report sales variance by SIN'
		goto ERROREXIT
	END

	IF OBJECT_ID('tempdb..#ContractSalesVarianceForQuarter') IS NOT NULL 
	BEGIN
		drop table #ContractSalesVarianceForQuarter
	END

	create table #ContractSalesVarianceForQuarter
	(
		CntrctNum nvarchar(50) NOT NULL,
		Schedule_Number int NOT NULL,
		Quarter_ID  int              NOT NULL,
		Year		nvarchar(4)		not null,
		Qtr			nvarchar(1)		not null,
		Title		nvarchar(20)	not null,
		VA_Sales   	money            NOT NULL,
		OGA_Sales 	money            NOT NULL,
		SLG_Sales  	money            NOT NULL,
		Total_Sales money			   not null,
		VA_IFF			decimal(19, 2)	   not null,
		OGA_IFF			decimal(19, 2)	   not null,
		SLG_IFF			decimal(19, 2)	   not null,
		Total_IFF		decimal(19, 2)	    not null,
		VAPercentage		numeric(18, 4)		not null,
		OGAPercentage		numeric(18, 4)    not null,
		SLGPercentage		numeric(18, 4)    not null,
		PreviousQuarter_ID			int			 null,
		PreviousQuarterVA_Sales		money             NULL,
		PreviousQuarterOGA_Sales	money             NULL,
		PreviousQuarterSLG_Sales	money             NULL,
		PreviousQuarterTotal_Sales	money             NULL,
		PreviousYearQuarter_ID		int				  null,
		PreviousYearVA_Sales		money             NULL,
		PreviousYearOGA_Sales		money             NULL,
		PreviousYearSLG_Sales		money             NULL,
		PreviousYearTotal_Sales		money             NULL,
		VarianceQuarterVA numeric(18, 4) NULL,
		VarianceQuarterOGA numeric(18, 4) NULL,
		VarianceQuarterSLG numeric(18, 4) NULL,
		VarianceQuarterTotal numeric(18, 4) NULL,
		VarianceYearVA numeric(18, 4) NULL,
		VarianceYearOGA numeric(18, 4) NULL,
		VarianceYearSLG numeric(18, 4) NULL,
		VarianceYearTotal numeric(18, 4) NULL
	)	
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table for on-screen report sales variance by SIN (summary portion)'
		goto ERROREXIT
	END


	insert into #ContractSalesVarianceBySIN
	(
		CntrctNum,
		Schedule_Number,
		Quarter_ID,
		Year,
		Qtr,
		Title,
		[SIN],
		LexicalSIN,
		VA_Sales,
		OGA_Sales,
		SLG_Sales,
		Total_Sales,
		VA_IFF,
		OGA_IFF,
		SLG_IFF,
		Total_IFF,
		VAPercentage,
		OGAPercentage,
		SLGPercentage
	)
	SELECT		s.CntrctNum,
				c.Schedule_Number,
				s.Quarter_ID, 
				y.Year, 
				y.Qtr, 
				y.Title,
				s.SIN,
				x.LexicalSIN,
				s.VA_Sales, 
				s.OGA_Sales, 
				s.SLG_Sales, 
				s.VA_Sales + s.OGA_Sales + s.SLG_Sales,
				CAST(s.VA_Sales * i.VA_IFF AS decimal(19, 2)) AS VA_IFF, 
				CAST(s.OGA_Sales * i.OGA_IFF AS decimal(19, 2)) AS OGA_IFF, 
				CAST(s.SLG_Sales * i.SLG_IFF AS decimal(19, 2)) AS SLG_IFF, 
				CAST((s.VA_Sales *i.VA_IFF) + (s.OGA_Sales * i.OGA_IFF) + (s.SLG_Sales * i.SLG_IFF) AS decimal(19, 2)) AS Total_IFF, 
				i.VA_IFF as VAPercentage, 
				i.OGA_IFF as OGAPercentage, 
				i.SLG_IFF as SLGPercentage      
		FROM dbo.tbl_Cntrcts_Sales s INNER JOIN dbo.tlkup_year_qtr y ON s.Quarter_ID = y.Quarter_ID 
				INNER JOIN dbo.tbl_Cntrcts c ON s.CntrctNum = c.CntrctNum 
				INNER JOIN dbo.[tlkup_Sched/Cat] t ON c.Schedule_Number = t.Schedule_Number 
				INNER JOIN dbo.tbl_IFF i ON t.Schedule_Number = i.Schedule_Number 
					AND y.start_quarter_id = i.Start_Quarter_Id
				INNER JOIN dbo.tbl_Cntrcts_SINs x ON x.CntrctNum = s.CntrctNum and x.[SINs] = s.[SIN]
		where s.CntrctNum = @ContractNumber
		and y.Quarter_ID between @PreviousYearQuarter_ID and @SelectedQuarter_ID
		and x.Inactive = 0
	
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting into temp table for on-screen report sales variance by SIN'
		goto ERROREXIT
	END
		
	/* one row: contains the summary for the selected quarter */
	insert into #ContractSalesVarianceForQuarter
	(
		CntrctNum,
		Schedule_Number,
		Quarter_ID,
		Year,
		Qtr,
		Title,
		VA_Sales,
		OGA_Sales,
		SLG_Sales,
		Total_Sales,
		VA_IFF,
		OGA_IFF,
		SLG_IFF,
		Total_IFF,
		VAPercentage,
		OGAPercentage,
		SLGPercentage
	)
	SELECT		t.CntrctNum,
				t.Schedule_Number,
				t.Quarter_ID, 
				t.Year, 
				t.Qtr, 
				t.Title,
				SUM(t.VA_Sales), 
				SUM(t.OGA_Sales), 
				SUM(t.SLG_Sales), 
				SUM(t.Total_Sales),
				SUM(t.VA_IFF), 
				SUM(t.OGA_IFF), 
				SUM(t.SLG_IFF), 
				SUM(t.Total_IFF), 
				VAPercentage, 
				OGAPercentage, 
				SLGPercentage      
		FROM #ContractSalesVarianceBySIN t
		where t.Quarter_ID = @SelectedQuarter_ID

		GROUP BY t.CntrctNum,
				t.Schedule_Number,
				t.Quarter_ID, 
				t.Year, 
				t.Qtr, 
				t.Title,
				VAPercentage, 
				OGAPercentage, 
				SLGPercentage   
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting into temp table for on-screen report sales variance by SIN (header portion)'
		goto ERROREXIT
	END

	/* previous quarter sales */
	update #ContractSalesVarianceForQuarter
	set PreviousQuarterVA_Sales = ( select SUM(t.VA_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousQuarter_ID ),
		PreviousQuarterOGA_Sales = ( select SUM(t.OGA_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousQuarter_ID ),
		PreviousQuarterSLG_Sales = ( select SUM(t.SLG_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousQuarter_ID ),
		PreviousQuarterTotal_Sales = ( select SUM(t.Total_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousQuarter_ID )
	
		FROM #ContractSalesVarianceBySIN t
		where t.Quarter_ID = @PreviousQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous quarter for sales variance by SIN report (header portion)'
		goto ERROREXIT
	END

	/* previous year quarter sales */		
	update #ContractSalesVarianceForQuarter
	set PreviousYearVA_Sales = ( select SUM(t.VA_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousYearQuarter_ID ),
		PreviousYearOGA_Sales =  ( select SUM(t.OGA_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousYearQuarter_ID ), 
		PreviousYearSLG_Sales =  ( select SUM(t.SLG_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousYearQuarter_ID ), 
		PreviousYearTotal_Sales =  ( select SUM(t.Total_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousYearQuarter_ID )
	
		FROM #ContractSalesVarianceBySIN t
		where t.Quarter_ID = @PreviousYearQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous year quarter for sales variance by SIN report (header portion)'
		goto ERROREXIT
	END
	
	/* previous quarter variance */
	update #ContractSalesVarianceForQuarter
	set VarianceQuarterVA = dbo.SalesVarianceFunction( VA_Sales, PreviousQuarterVA_Sales ),
		 VarianceQuarterOGA = dbo.SalesVarianceFunction( OGA_Sales, PreviousQuarterOGA_Sales ),
		 VarianceQuarterSLG = dbo.SalesVarianceFunction( SLG_Sales, PreviousQuarterSLG_Sales ),
		 VarianceQuarterTotal = dbo.SalesVarianceFunction( Total_Sales, PreviousQuarterTotal_Sales )
	FROM #ContractSalesVarianceForQuarter

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous quarter variance for sales variance by SIN report (header portion)'
		goto ERROREXIT
	END

	/* previous year variance */
	update #ContractSalesVarianceForQuarter
	set VarianceYearVA = dbo.SalesVarianceFunction( VA_Sales, PreviousYearVA_Sales ),
		 VarianceYearOGA = dbo.SalesVarianceFunction( OGA_Sales, PreviousYearOGA_Sales ),
		 VarianceYearSLG = dbo.SalesVarianceFunction( SLG_Sales, PreviousYearSLG_Sales ),
		 VarianceYearTotal = dbo.SalesVarianceFunction( Total_Sales, PreviousYearTotal_Sales )
	FROM #ContractSalesVarianceForQuarter

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous year quarter variance for sales variance by SIN report (header portion)'
		goto ERROREXIT
	END

	/* previous quarter sales for each SIN */		
	update #ContractSalesVarianceBySIN		
		set PreviousQuarterVA_Sales = ( select SUM(t.VA_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousQuarter_ID and t.[SIN] = s.[SIN] )
	from #ContractSalesVarianceBySIN s
	where s.Quarter_ID = @SelectedQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous quarter VA sales for sales variance by SIN report'
		goto ERROREXIT
	END

	update #ContractSalesVarianceBySIN		
		set PreviousQuarterOGA_Sales = ( select SUM(t.OGA_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousQuarter_ID and t.[SIN] = s.[SIN] )
	from #ContractSalesVarianceBySIN s
	where s.Quarter_ID = @SelectedQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous quarter OGA sales for sales variance by SIN report'
		goto ERROREXIT
	END

	update #ContractSalesVarianceBySIN		
		set PreviousQuarterSLG_Sales = ( select SUM(t.SLG_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousQuarter_ID and t.[SIN] = s.[SIN] )
	from #ContractSalesVarianceBySIN s
	where s.Quarter_ID = @SelectedQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous quarter SLG sales for sales variance by SIN report'
		goto ERROREXIT
	END

	update #ContractSalesVarianceBySIN		
		set PreviousQuarterTotal_Sales = ( select SUM(t.Total_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousQuarter_ID and t.[SIN] = s.[SIN] )
	from #ContractSalesVarianceBySIN s
	where s.Quarter_ID = @SelectedQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous quarter total sales for sales variance by SIN report'
		goto ERROREXIT
	END	

	/* previous quarter variance */
	update #ContractSalesVarianceBySIN
	set VarianceQuarterVA = dbo.SalesVarianceFunction( VA_Sales, PreviousQuarterVA_Sales ),
		 VarianceQuarterOGA = dbo.SalesVarianceFunction( OGA_Sales, PreviousQuarterOGA_Sales ),
		 VarianceQuarterSLG = dbo.SalesVarianceFunction( SLG_Sales, PreviousQuarterSLG_Sales ),
		 VarianceQuarterTotal = dbo.SalesVarianceFunction( Total_Sales, PreviousQuarterTotal_Sales )
	FROM #ContractSalesVarianceBySIN
	where Quarter_ID = @SelectedQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous quarter variance for sales variance by SIN report'
		goto ERROREXIT
	END

	/* previous year quarter sales for each SIN */		
	update #ContractSalesVarianceBySIN		
		set PreviousYearVA_Sales = ( select SUM(t.VA_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousYearQuarter_ID and t.[SIN] = s.[SIN] )
	from #ContractSalesVarianceBySIN s
	where s.Quarter_ID = @SelectedQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous year quarter sales for VA sales in variance by SIN report'
		goto ERROREXIT
	END

	update #ContractSalesVarianceBySIN		
		set PreviousYearOGA_Sales = ( select SUM(t.OGA_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousYearQuarter_ID and t.[SIN] = s.[SIN] )
	from #ContractSalesVarianceBySIN s
	where s.Quarter_ID = @SelectedQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous year quarter sales for OGA sales in variance by SIN report'
		goto ERROREXIT
	END

	update #ContractSalesVarianceBySIN		
		set PreviousYearSLG_Sales = ( select SUM(t.SLG_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousYearQuarter_ID and t.[SIN] = s.[SIN] )
	from #ContractSalesVarianceBySIN s
	where s.Quarter_ID = @SelectedQuarter_ID
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous year quarter sales for SLG sales in variance by SIN report'
		goto ERROREXIT
	END

	update #ContractSalesVarianceBySIN		
		set PreviousYearTotal_Sales = ( select SUM(t.Total_Sales) FROM #ContractSalesVarianceBySIN t where t.Quarter_ID = @PreviousYearQuarter_ID and t.[SIN] = s.[SIN] )
	from #ContractSalesVarianceBySIN s
	where s.Quarter_ID = @SelectedQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous year quarter sales for total sales in variance by SIN report'
		goto ERROREXIT
	END

	/* previous year variance */
	update #ContractSalesVarianceBySIN	
	set VarianceYearVA = dbo.SalesVarianceFunction( VA_Sales, PreviousYearVA_Sales ),
		 VarianceYearOGA = dbo.SalesVarianceFunction( OGA_Sales, PreviousYearOGA_Sales ),
		 VarianceYearSLG = dbo.SalesVarianceFunction( SLG_Sales, PreviousYearSLG_Sales ),
		 VarianceYearTotal = dbo.SalesVarianceFunction( Total_Sales, PreviousYearTotal_Sales )
	FROM #ContractSalesVarianceBySIN s
	where s.Quarter_ID = @SelectedQuarter_ID

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous year quarter variance for total sales in variance by SIN report'
		goto ERROREXIT
	END
		
	if @DataSetId = 1
		BEGIN
		/* select summary row for the selected quarter */
		select Title, 
			VA_Sales, 
			OGA_Sales, 
			SLG_Sales, 
			Total_Sales, 
			VA_IFF, 
			OGA_IFF, 
			SLG_IFF, 
			Total_IFF,
			VarianceQuarterVA,
			VarianceQuarterOGA,
			VarianceQuarterSLG,
			VarianceQuarterTotal,
			VarianceYearVA,
			VarianceYearOGA,
			VarianceYearSLG,
			VarianceYearTotal
		from #ContractSalesVarianceForQuarter
	
		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting sales variance summary for selected quarter'
			goto ERROREXIT
		END
	END
	else
	BEGIN
		/* select SIN rows for the selected quarter */
		select [SIN],
			LexicalSIN,
			VA_Sales, 
			OGA_Sales, 
			SLG_Sales, 
			Total_Sales, 
			VA_IFF, 
			OGA_IFF, 
			SLG_IFF, 
			Total_IFF,
			VarianceQuarterVA,
			VarianceQuarterOGA,
			VarianceQuarterSLG,
			VarianceQuarterTotal,
			VarianceYearVA,
			VarianceYearOGA,
			VarianceYearSLG,
			VarianceYearTotal
		from #ContractSalesVarianceBySIN
		where Quarter_ID = @SelectedQuarter_ID
		order by LexicalSIN

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting sales variance by SIN for selected quarter'
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
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )


