IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'IFFSalesAdjustmentReport')
	BEGIN
		DROP  Procedure  IFFSalesAdjustmentReport
	END

GO

CREATE Procedure IFFSalesAdjustmentReport
(
	@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
	@StartYear int,
	@StartQuarter int, 
	@EndYear int,
	@EndQuarter int,
	@contractnumber nvarchar (50), /* required */
	@DivisionId int, /* may be -1 = all */
	@Paymentposted int /* may be -1 = all */
)

AS

declare 
	@error int,
	@query nvarchar(4000),
	@query0 nvarchar (500),
	@query1 nvarchar (4000),
	@query2 nvarchar (500),
	@query3 nvarchar (4000),
	@groupByString nvarchar(1000),
	@orderbystring nvarchar (100),
	@SQLParms nvarchar (1000),
	@errormsg nvarchar(300),
	@joinSecurityServerName nvarchar(1000),
	@SERVERNAME nvarchar(255),
	@whereSchedule nvarchar(100),
	@Posted nvarchar(1000),
	@cntrctnum nvarchar (100),
	@QuarterID int,
	@StartQuarterID int,
	@EndQuarterID int


BEGIN TRANSACTION

	IF @contractnumber = ''
	set @contractnumber = NULL
	
	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'IFF Sales Adjustment Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	------ select the Start QuarterID -----------
	set @StartQuarterID = (select Quarter_ID
		from NAC_CM.dbo.tlkup_year_qtr
		where Year = @StartYear
		and Qtr = @StartQuarter)

	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting start quarter.'
		goto ERROREXIT
	END
	------ select the END QuarterID ------------
	set @EndQuarterID = (select Quarter_ID
		from NAC_CM.dbo.tlkup_year_qtr
		where Year = @EndYear
		and Qtr = @EndQuarter)		
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting end quarter.'
		goto ERROREXIT
	END
	
	create table #iffsalesadjustmentreport
	(
	[Fiscal_Year_Quarter] nvarchar (20),
	CompanyName nvarchar (75),
	ContractNumber nvarchar (50),
	[IFF_Calculation] numeric(18,4),
	[SIN] nvarchar (10),
	Old_VA_Sales money,
	Old_OGA_Sales money,
	Old_SLG_Sales money,
	Old_Total_Sales money,
	Old_IFF money,
	New_VA_Sales money,
	New_OGA_Sales money,
	New_SLG_Sales money,
	New_Total_Sales money,
	New_IFF_Due money,
	Sales_Difference money,
	IFF_Difference money,
	LastModificationDate datetime,
	LastModifiedBy nvarchar (128)
	)
	
	select @query1 = '
	insert into #iffsalesadjustmentreport
	(
	[Fiscal_Year_Quarter],
	CompanyName,
	ContractNumber,
	[IFF_Calculation],
	[SIN],
	Old_VA_Sales,
	Old_OGA_Sales,
	Old_SLG_Sales,
	Old_Total_Sales,
	Old_IFF,
	New_VA_Sales,
	New_OGA_Sales,
	New_SLG_Sales,
	New_Total_Sales,
	New_IFF_Due,
	Sales_Difference,
	IFF_Difference,
	LastModificationDate,
	LastModifiedBy
	)
	select
	f.Title,
	a.Contractor_Name,
	c.CntrctNum,
	i.VA_IFF,
	c.SIN,
	s.VA_Sales,
	s.OGA_Sales,
	s.SLG_Sales,
	s.VA_Sales + s.OGA_Sales + s.SLG_Sales,
	(s.VA_Sales + s.OGA_Sales + s.SLG_Sales)*i.VA_IFF,
	c.VA_Sales,
	c.OGA_Sales,
	c.SLG_Sales,
	c.VA_Sales + c.OGA_Sales + c.SLG_Sales,
	(c.VA_Sales + c.OGA_Sales + c.SLG_Sales)*i.VA_IFF,
	(c.VA_Sales + c.OGA_Sales + c.SLG_Sales) - (s.VA_Sales + s.OGA_Sales + s.SLG_Sales),
	(c.VA_Sales + c.OGA_Sales + c.SLG_Sales)*i.VA_IFF - (s.VA_Sales + s.OGA_Sales + s.SLG_Sales)*i.VA_IFF,
	c.LastModificationDate,
	g.FullName
	from NAC_CM.dbo.tbl_Cntrcts a 
	join NAC_CM.dbo.[tlkup_Sched/Cat] b
	on a.Schedule_Number = b.Schedule_Number
	join NAC_CM.dbo.tbl_IFF i 
	on b.Schedule_Number = i.Schedule_Number
	join NAC_CM.dbo.tbl_Cntrcts_Sales c
	on a.CntrctNum = c.CntrctNum
	join NAC_CM.dbo.Audit_tbl_Cntrcts_Sales_History s
	on c.CntrctNum = s.CntrctNum and c.Quarter_ID = s.Quarter_ID and c.SIN = s.SIN
	join NAC_CM.dbo.tlkup_year_qtr f
	on c.Quarter_ID = f.Quarter_ID and (f.Quarter_ID between i.Start_Quarter_ID and i.End_Quarter_ID)
	join NAC_CM.dbo.tlkup_UserProfile g
	on c.LastModifiedBy = g.UserName
	left outer join NAC_CM.dbo.tbl_Cntrcts_Checks d
	on c.CntrctNum = d.CntrctNum and c.Quarter_ID = d.Quarter_ID'

		select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
		
		select @query2 = ' 
	where (c.Quarter_ID between @StartquarterID and @EndquarterID)'

	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2'
		goto ERROREXIT
	END


	select @groupByString = ' 
	group by c.LastModificationDate,
	f.Title,
	a.Contractor_Name,
	c.CntrctNum,
	i.VA_IFF,
	c.SIN,
	s.VA_Sales,
	s.OGA_Sales,
	s.SLG_Sales,
	c.VA_Sales,
	c.OGA_Sales,
	c.SLG_Sales,
	g.FullName,
	d.CntrctNum,
	c.LastModifiedBy,
	c.Quarter_ID,
	d.Quarter_ID'
	

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning group string'
		goto ERROREXIT
	END

	select @orderbystring = ' 
	order by c.CntrctNum'

	
	set @query3 = '
	select x.[Fiscal_Year_Quarter],
	x.CompanyName,
	x.ContractNumber,
	x.[IFF_Calculation],
	x.[SIN],
	x.Old_VA_Sales,
	x.Old_OGA_Sales,
	x.Old_SLG_Sales,
	x.Old_Total_Sales,
	x.Old_IFF,
	x.New_VA_Sales,
	x.New_OGA_Sales,
	x.New_SLG_Sales,
	x.New_Total_Sales,
	x.New_IFF_Due,
	x.Sales_Difference,
	x.IFF_Difference,
	x.LastModificationDate,
	x.LastModifiedBy
	from #iffsalesadjustmentreport x
	inner join (select contractNumber, [Fiscal_Year_Quarter] from #iffsalesadjustmentreport group by contractNumber, [Fiscal_Year_Quarter]) as y
	on x.ContractNumber = y.ContractNumber and x.[Fiscal_Year_Quarter] = y.[Fiscal_Year_Quarter]
	group by x.[Fiscal_Year_Quarter],
	x.CompanyName,
	x.ContractNumber,
	x.[IFF_Calculation],
	x.[SIN],
	x.Old_VA_Sales,
	x.Old_OGA_Sales,
	x.Old_SLG_Sales,
	x.Old_Total_Sales,
	x.Old_IFF,
	x.New_VA_Sales,
	x.New_OGA_Sales,
	x.New_SLG_Sales,
	x.New_Total_Sales,
	x.New_IFF_Due,
	x.Sales_Difference,
	x.IFF_Difference,
	x.LastModificationDate,
	x.LastModifiedBy
	order by ContractNumber,
	[Fiscal_Year_Quarter],
	SIN'

	
	
	---- Ignoring the schedule names while the contract number is provided ----------	
	IF LEN (@contractnumber) is not NULL
		BEGIN
			set @cntrctnum = ' and (a.CntrctNum = ''' + @contractnumber + ''')
			 '
			set @whereSchedule = ' '
		END	
	ELSE
		BEGIN
			set @cntrctnum = ' '
			
			If @DivisionId <> -1
				BEGIN
					set @whereSchedule = ' and (b.Division = ' + cast (@DivisionId as nvarchar) + ')
					 '
				END
			ELSE
				BEGIN
					set @whereSchedule = ' '

				END
					
		END

	If @Paymentposted <> -1
		BEGIN
			------- Sales update details for the Posted Payments ---------	
			IF @Paymentposted = 1
				BEGIN
					
					set @Posted = 'and d.CheckAmt is not null'
										
					select @query0 = '
					IF OBJECT_ID(''#iffsalesadjustmentreport'') IS NOT NULL
					TRUNCATE table #iffsalesadjustmentreport'

					select @query = @query0 + @query1 + @query2 + @cntrctnum + @whereSchedule  + @posted + 
					@groupByString + @orderbystring
					
					select @SQLParms = N'@StartQuarterID int, @EndQuarterID int'
					
					exec SP_EXECUTESQL @query, @SQLParms, @startQuarterId, @endQuarterId
					
					EXEC SP_EXECUTESQL @query3

					select @error = @@error
					if @error <> 0 
					BEGIN
						select @errorMsg = 'Error selecting sales for sales update report.'
						goto ERROREXIT
					END

				END

			------- Sales update details for the Non-Posted Payments ---------		
			ELSE IF @Paymentposted = 2
				BEGIN

					set @Posted = ' and d.CheckAmt is null '

					select @query0 = '
					IF OBJECT_ID(''#iffsalesadjustmentreport'') IS NOT NULL
					TRUNCATE table #iffsalesadjustmentreport'

					select @query = @query0 + @query1 + @query2 + @cntrctnum + @whereSchedule  + @posted + 
					@groupByString + @orderbystring
					
					select @SQLParms = N'@StartQuarterID int, @EndQuarterID int'
					
					exec SP_EXECUTESQL @query, @SQLParms, @startQuarterId, @endQuarterId
					
					EXEC SP_EXECUTESQL @query3
					
					select @error = @@error
					if @error <> 0 
					BEGIN
						select @errorMsg = 'Error selecting sales for sales update report.'
						goto ERROREXIT
					END
					
					
					
				END
		END
	------- Sales update details for both posted and non posted payments ---------	
	ELSE
		BEGIN
				BEGIN
						
					set @Posted = ' and d.CheckAmt is not null '

					select @query0 = '
					IF OBJECT_ID(''#iffsalesadjustmentreport'') IS NOT NULL
					TRUNCATE table #iffsalesadjustmentreport'

					select @query = @query0 + @query1 + @query2 + @cntrctnum + @whereSchedule  + @posted + 
					@groupByString + @orderbystring
					
					select @SQLParms = N'@StartQuarterID int, @EndQuarterID int'

					exec SP_EXECUTESQL @query, @SQLParms, @startQuarterId, @endQuarterId
					
					select @error = @@error
					
					if @error <> 0 
					BEGIN
						select @errorMsg = 'Error selecting sales for sales update report.'
						goto ERROREXIT
					END
				END
				
				BEGIN
						
					set @Posted = ' and d.CheckAmt is null '

					select @query0 = ' '
					
					select @query = @query0 + @query1 + @query2 + @cntrctnum + @whereSchedule  + @posted + 
					@groupByString + @orderbystring
					
					select @SQLParms = N'@StartQuarterID int, @EndQuarterID int'
					
					exec SP_EXECUTESQL @query, @SQLParms, @startQuarterId, @endQuarterId
					
					EXEC SP_EXECUTESQL @query3
					
					select @error = @@error
					
					if @error <> 0 
					BEGIN
						select @errorMsg = 'Error selecting sales for sales update report.'
						goto ERROREXIT
					END
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




GO


