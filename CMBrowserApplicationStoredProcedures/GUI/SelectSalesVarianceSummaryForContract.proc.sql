IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSalesVarianceSummaryForContract' )
BEGIN
	DROP PROCEDURE SelectSalesVarianceSummaryForContract
END
GO

CREATE PROCEDURE SelectSalesVarianceSummaryForContract
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	IF OBJECT_ID('tempdb..#ContractSalesVariance') IS NOT NULL 
	BEGIN
		drop table #ContractSalesVariance
	END
	
	create table #ContractSalesVariance
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
		select @errorMsg = 'Error creating temp table for on-screen sales variance summary for contract'
		goto ERROREXIT
	END

	insert into #ContractSalesVariance
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
	SELECT		s.CntrctNum,
				c.Schedule_Number,
				s.Quarter_ID, 
				y.Year, 
				y.Qtr, 
				y.Title,
				SUM(s.VA_Sales) AS [VA Sales], 
				SUM(s.OGA_Sales) AS [OGA Sales], 
				SUM(s.SLG_Sales) AS [SLG Sales], 
				SUM(s.VA_Sales + s.OGA_Sales + s.SLG_Sales) AS Total_Sales,
				SUM(CAST(s.VA_Sales * i.VA_IFF AS decimal(19, 2))) AS VA_IFF, 
				SUM(CAST(s.OGA_Sales * i.OGA_IFF AS decimal(19, 2))) AS OGA_IFF, 
				SUM(CAST(s.SLG_Sales * i.SLG_IFF AS decimal(19, 2))) AS SLG_IFF, 
				SUM(CAST((s.VA_Sales *i.VA_IFF ) + ( s.OGA_Sales * i.OGA_IFF) + ( s.SLG_Sales * i.SLG_IFF ) AS decimal(19, 2))) AS Total_IFF, 
				i.VA_IFF, 
				i.OGA_IFF, 
				i.SLG_IFF      
		FROM dbo.tbl_Cntrcts_Sales s INNER JOIN dbo.tlkup_year_qtr y ON s.Quarter_ID = y.Quarter_ID 
				INNER JOIN dbo.tbl_Cntrcts c ON s.CntrctNum = c.CntrctNum 
				INNER JOIN dbo.[tlkup_Sched/Cat] t ON c.Schedule_Number = t.Schedule_Number 
				INNER JOIN dbo.tbl_IFF i ON t.Schedule_Number = i.Schedule_Number 
					AND y.start_quarter_id = i.Start_Quarter_Id
				
		where s.CntrctNum = @ContractNumber
				
		GROUP BY s.CntrctNum, 
				c.Schedule_Number,
				y.Year, 
				y.Qtr, 
				y.Title,
				i.VA_IFF, 
				i.OGA_IFF, 
				i.SLG_IFF, 
				s.Quarter_ID, 
				c.Schedule_Number
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting into temp table for on-screen sales variance summary for contract'
		goto ERROREXIT
	END
		
	update #ContractSalesVariance	
		set PreviousQuarter_ID = Quarter_ID - 1,
			PreviousYearQuarter_ID = Quarter_ID - 4
		from #ContractSalesVariance
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past quarter Ids for variance summary'
		goto ERROREXIT
	END
		
	update #ContractSalesVariance		

		set PreviousQuarterVA_Sales = ( select SUM(s.VA_Sales) from dbo.tbl_Cntrcts_Sales s where v.PreviousQuarter_ID = s.Quarter_ID
			AND v.CntrctNum = s.CntrctNum )

		from #ContractSalesVariance v
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past quarter va sales for variance summary'
		goto ERROREXIT
	END
		
	update #ContractSalesVariance		

		set PreviousQuarterOGA_Sales = ( select SUM(s.OGA_Sales) from dbo.tbl_Cntrcts_Sales s where v.PreviousQuarter_ID = s.Quarter_ID
			AND v.CntrctNum = s.CntrctNum )
	
		from #ContractSalesVariance v
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past quarter oga sales for variance summary'
		goto ERROREXIT
	END
		
	update #ContractSalesVariance		

		set PreviousQuarterSLG_Sales = ( select SUM(s.SLG_Sales) from dbo.tbl_Cntrcts_Sales s where v.PreviousQuarter_ID = s.Quarter_ID
			AND v.CntrctNum = s.CntrctNum ) 
	
		from #ContractSalesVariance v
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past quarter slg sales for variance summary'
		goto ERROREXIT
	END
		
	update #ContractSalesVariance		

		set PreviousQuarterTotal_Sales = ( select SUM(s.VA_Sales + s.OGA_Sales + s.SLG_Sales) from dbo.tbl_Cntrcts_Sales s where v.PreviousQuarter_ID = s.Quarter_ID
			AND v.CntrctNum = s.CntrctNum )
	
		from #ContractSalesVariance v
	
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past quarter total sales for variance summary'
		goto ERROREXIT
	END

	update #ContractSalesVariance		

		set PreviousYearVA_Sales = ( select SUM(s.VA_Sales) from dbo.tbl_Cntrcts_Sales s where v.PreviousYearQuarter_ID = s.Quarter_ID
			AND v.CntrctNum = s.CntrctNum )

		from #ContractSalesVariance v
	

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past year va sales for variance summary'
		goto ERROREXIT
	END

	update #ContractSalesVariance		

		set PreviousYearOGA_Sales = ( select SUM(s.OGA_Sales) from dbo.tbl_Cntrcts_Sales s where v.PreviousYearQuarter_ID = s.Quarter_ID
			AND v.CntrctNum = s.CntrctNum )
	
		from #ContractSalesVariance v
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past year oga sales for variance summary'
		goto ERROREXIT
	END
		
	update #ContractSalesVariance		

		set PreviousYearSLG_Sales = ( select SUM(s.SLG_Sales) from dbo.tbl_Cntrcts_Sales s where v.PreviousYearQuarter_ID = s.Quarter_ID
			AND v.CntrctNum = s.CntrctNum ) 
	
		from #ContractSalesVariance v
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past year slg sales for variance summary'
		goto ERROREXIT
	END
		
	update #ContractSalesVariance		

		set PreviousYearTotal_Sales = ( select SUM(s.VA_Sales + s.OGA_Sales + s.SLG_Sales) from dbo.tbl_Cntrcts_Sales s where v.PreviousYearQuarter_ID = s.Quarter_ID
			AND v.CntrctNum = s.CntrctNum )
	
		from #ContractSalesVariance v

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past year total sales for variance summary'
		goto ERROREXIT
	END
			
	update #ContractSalesVariance
		set VarianceQuarterVA = dbo.SalesVarianceFunction( VA_Sales, PreviousQuarterVA_Sales )
		from #ContractSalesVariance
		where PreviousQuarterVA_Sales is not null
		and PreviousQuarterVA_Sales <> 0
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past quarter va variance for variance summary'
		goto ERROREXIT
	END
			
	update #ContractSalesVariance
		set VarianceQuarterOGA = dbo.SalesVarianceFunction( OGA_Sales, PreviousQuarterOGA_Sales )
		from #ContractSalesVariance
		where PreviousQuarterOGA_Sales is not null
		and PreviousQuarterOGA_Sales <> 0
		
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past quarter oga variance for variance summary'
		goto ERROREXIT
	END
		
	update #ContractSalesVariance
		set VarianceQuarterSLG = dbo.SalesVarianceFunction( SLG_Sales, PreviousQuarterSLG_Sales ) 
		from #ContractSalesVariance
		where PreviousQuarterSLG_Sales is not null
		and PreviousQuarterSLG_Sales <> 0
		
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past quarter slg variance for variance summary'
		goto ERROREXIT
	END			

	update #ContractSalesVariance
		set VarianceQuarterTotal = dbo.SalesVarianceFunction( Total_Sales, PreviousQuarterTotal_Sales ) 
		from #ContractSalesVariance
		where PreviousQuarterTotal_Sales is not null
		and PreviousQuarterTotal_Sales <> 0
			
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past quarter total variance for variance summary'
		goto ERROREXIT
	END	

	update #ContractSalesVariance
		set VarianceYearVA = dbo.SalesVarianceFunction( VA_Sales, PreviousYearVA_Sales ) 
		from #ContractSalesVariance
		where PreviousYearVA_Sales is not null
		and PreviousYearVA_Sales <> 0
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past year va variance for variance summary'
		goto ERROREXIT
	END	
			
	update #ContractSalesVariance
		set VarianceYearOGA = dbo.SalesVarianceFunction( OGA_Sales, PreviousYearOGA_Sales ) 
		from #ContractSalesVariance
		where PreviousYearOGA_Sales is not null
		and PreviousYearOGA_Sales <> 0
		
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past year oga variance for variance summary'
		goto ERROREXIT
	END	
		
	update #ContractSalesVariance
		set VarianceYearSLG = dbo.SalesVarianceFunction( SLG_Sales, PreviousYearSLG_Sales ) 
		from #ContractSalesVariance
		where PreviousYearSLG_Sales is not null
		and PreviousYearSLG_Sales <> 0
			
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past year slg variance for variance summary'
		goto ERROREXIT
	END	

	update #ContractSalesVariance
		set VarianceYearTotal = dbo.SalesVarianceFunction( Total_Sales, PreviousYearTotal_Sales ) 
		from #ContractSalesVariance
		where PreviousYearTotal_Sales is not null
		and PreviousYearTotal_Sales <> 0
							

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating past year total variance for variance summary'
		goto ERROREXIT
	END	

	select Quarter_ID,
		Title, 
		Year,
		Qtr,
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
	
	from #ContractSalesVariance
	order by Quarter_ID desc

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales variance summary for contract'
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



