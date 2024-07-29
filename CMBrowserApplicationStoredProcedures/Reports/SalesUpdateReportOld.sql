IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SalesUpdateReport')
	BEGIN
		DROP  Procedure  SalesUpdateReport
	END

GO

CREATE Procedure SalesUpdateReport
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
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Sales update Report', '2'
	
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
	
	create table #salesupdatereport
	(
	ContractNumber nvarchar (50),
	CompanyName nvarchar (75),
	ScheduleName nvarchar (75),
	DateContractEffective datetime,
	DateContractExpire datetime,
	DateContractComplete datetime,
	Year_Quarter nvarchar (20),
	[SIN] nvarchar (10),
	FieldName nvarchar (128),
	OldValue nvarchar (1000),
	NewValue nvarchar (1000),
	PaymentPosted nvarchar (20),
	PaymentpostedDate nvarchar (20),
	LastModificationDate datetime,
	LastModifiedBy nvarchar (128)
	)
	
	select @query1 = '
	insert into #salesupdatereport
	(
	ContractNumber,
	CompanyName,
	ScheduleName,
	DateContractEffective,
	DateContractExpire,
	DateContractComplete,
	Year_Quarter,
	[SIN],
	FieldName,
	OldValue,
	NewValue,
	PaymentPosted,
	PaymentpostedDate,
	LastModificationDate,
	LastModifiedBy
	)
	select	
	c.CntrctNum,
	a.Contractor_Name,
	b.Schedule_Name,
	a.Dates_Effective,
	a.Dates_CntrctExp,
	a.Dates_Completion,
	f.Title,
	c.SIN,
	e.FieldName,
	e.OldValue,
	e.NewValue,
	case when d.CheckAmt is NULL then ''NO'' else ''YES'' end as paymentposted ,
	case when d.DateRcvd is NULL then ''N/A'' else convert (nvarchar(20), d.DateRcvd, 101) end as Paymentposteddate,
	e.LastModificationDate,
	g.FullName
	from NAC_CM.dbo.tbl_Cntrcts a
	join NAC_CM.dbo.[tlkup_Sched/Cat] b
	on a.Schedule_Number = b.Schedule_Number
	join NAC_CM.dbo.tbl_Cntrcts_Sales c
	on a.CntrctNum = c.CntrctNum
	join NAC_CM.dbo.Audit_tbl_Cntrcts_Sales e
	on c.ID = e.ID
	join NAC_CM.dbo.tlkup_year_qtr f
	on c.Quarter_ID = f.Quarter_ID
	join NAC_CM.dbo.tlkup_UserProfile g
	on e.LastModifiedBy = g.UserName
	left outer join NAC_CM.dbo.tbl_Cntrcts_Checks d
	on c.CntrctNum = d.CntrctNum and c.Quarter_ID = d.Quarter_ID'

		select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
		
		select @query2 = ' 
	where (e.FieldName in (''VA_Sales'',''OGA_Sales'',''SLG_Sales'')) and (c.Quarter_ID between @StartquarterID and @EndquarterID)'

	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2'
		goto ERROREXIT
	END


	select @groupByString = ' 
	group by e.LastModificationDate,
	c.CntrctNum,
	a.Contractor_Name,
	b.Schedule_Name,			
	a.Dates_Effective,
	a.Dates_CntrctExp,
	a.Dates_Completion,
	f.Title,
	c.SIN,
	e.FieldName,
	e.OldValue,
	e.NewValue,
	g.FullName,
	d.CntrctNum,
	c.Quarter_ID,
	d.Quarter_ID,
	d.DateRcvd,
	d.CheckAmt'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning group string'
		goto ERROREXIT
	END

	select @orderbystring = ' 
	order by c.CntrctNum'

	set @query3 = '
	select x.ContractNumber,
	x.CompanyName,
	x.ScheduleName, 
	x.DateContractEffective,
	x.DateContractExpire,
	x.DateContractComplete,
	x.Year_Quarter,
	x.[SIN],
	x.FieldName,
	x.OldValue,
	x.NewValue,
	x.PaymentPosted,
	y.Recent_Payment_posted_Date,
	x.LastModificationDate,
	x.LastModifiedBy from #salesupdatereport x
	inner join (select contractNumber, Year_Quarter, MAX(PaymentpostedDate) as recent_payment_posted_date from #salesupdatereport group by contractNumber, Year_Quarter) as y
	on x.ContractNumber = y.ContractNumber and x.Year_Quarter = y.Year_Quarter
	group by x.ContractNumber,
	x.CompanyName,
	x.ScheduleName,
	x.DateContractEffective,
	x.DateContractExpire,
	x.DateContractComplete,
	x.Year_Quarter,
	x.[SIN],
	x.FieldName,
	x.OldValue,
	x.NewValue,
	x.PaymentPosted,
	y.recent_payment_posted_date,
	x.LastModificationDate,
	x.LastModifiedBy
	order by ContractNumber,
	Year_Quarter,
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
					IF OBJECT_ID(''#salesupdatereport'') IS NOT NULL
					TRUNCATE table #salesupdatereport'

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
					IF OBJECT_ID(''#salesupdatereport'') IS NOT NULL
					TRUNCATE table #salesupdatereport'

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
					IF OBJECT_ID(''#salesupdatereport'') IS NOT NULL
					TRUNCATE table #salesupdatereport'

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



