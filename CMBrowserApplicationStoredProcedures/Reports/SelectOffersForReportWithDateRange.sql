IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectOffersForReportWithDateRange')
	BEGIN
		DROP  Procedure  SelectOffersForReportWithDateRange
	END

GO

CREATE Procedure SelectOffersForReportWithDateRange
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@CompletionStatus int,    /* -1 = all */
@CurrentActionId int, /* -1 = all */
@ScheduleNumber int, /* -1 = all */
@ContractingOfficerId int, /* -1 = all */
@AssignmentStartYear int,  /* -1 = all */
@AssignmentEndYear int,  /* -1 = all */
@AssignmentStartMonth int,  /* -1 = all */
@AssignmentEndMonth int, /* -1 = all */
@LastActionStartYear int,  /* -1 = all */
@LastActionEndYear int,  /* -1 = all */
@LastActionStartMonth int,  /* -1 = all */
@LastActionEndMonth int,  /* -1 = all */
@DaysFromAssignmentToLastAction int 
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3400),
		@joinSecurityServerName nvarchar(1000),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@whereComplete nvarchar(100),
		@whereCurrentAction nvarchar(100),
		@whereAssignmentDate nvarchar(400),
		@whereLastActionDate nvarchar(400),
		@whereDaysFromAssignmentToLastAction nvarchar(100),
	 	@SERVERNAME nvarchar(255),
	 	@startDate datetime,
	 	@endDate datetime

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Offers Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
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
	
	select @query = 'select o.Offer_ID, o.Solicitation_ID, n.Solicitation_Number, o.CO_ID, p.FullName, o.Schedule_Number, s.Schedule_Name, s.Division, 
		o.Proposal_Type_ID, t.Proposal_Type_Description, o.Action_ID, a.Action_Description, a.Complete as IsActionComplete, o.Contractor_Name,
		o.Dates_Assigned, o.Dates_Reassigned, o.Dates_Action, o.Dates_Expected_Completion, o.Dates_Expiration, o.Dates_Sent_for_Preaward, o.Dates_Returned_to_Office,
		o.Audit_Indicator, o.ContractNumber, o.Date_Entered, o.Date_Modified, DATEDIFF( dd, o.Dates_Assigned, o.Dates_Action ) as DaysFromAssignmentToLastAction,
		DATEDIFF( dd, o.Dates_Assigned, getdate() ) as DaysFromAssignmentToToday, DATEDIFF( dd, o.Dates_Action, getdate() ) as DaysFromLastActionToToday
	from tbl_Offers o join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] p on o.CO_ID = p.CO_ID
	join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
	join tlkup_Offers_Proposal_Type t on o.Proposal_Type_ID = t.Proposal_Type_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number 
	join tlkup_Solicitation_Numbers n on o.Solicitation_ID = n.Solicitation_ID 
	where o.Offer_ID > 0 '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END

	if @CompletionStatus <> -1
	BEGIN
		select @whereComplete = ' and a.Complete = ' + convert( char(1), convert( bit, ( case when ( @CompletionStatus = 0 ) then 0 else 1 end )))
	END
	else
	BEGIN
		select @whereComplete = ''
	END

	if @CurrentActionId <> -1
	BEGIN
		select @whereCurrentAction = ' and o.Action_ID = ' + convert( nvarchar(10), @CurrentActionId )
	END
	else
	BEGIN
		select @whereCurrentAction = ''
	END

	if @ScheduleNumber <> -1
	BEGIN
		select @whereSchedule = ' and o.Schedule_Number = ' + convert( nvarchar(10), @ScheduleNumber )
	END
	else
	BEGIN
		select @whereSchedule = ' '
	END

	if @ContractingOfficerId <> -1
	BEGIN
		select @whereContractingOfficer = ' and o.CO_ID = ' + convert( nvarchar(10), @ContractingOfficerId )
	END
	else
	BEGIN
		select @whereContractingOfficer = ' '
	END
	

	if @AssignmentStartYear = -1 OR @AssignmentEndYear = -1 
	BEGIN
		select @whereAssignmentDate = ''
	END
	else
	BEGIN

		if @AssignmentStartMonth = -1 OR @AssignmentEndMonth = -1
		BEGIN
			select @whereAssignmentDate = ' and YEAR(o.Dates_Assigned) between ' + convert( nvarchar(10), @AssignmentStartYear ) + ' and ' + convert( nvarchar(10), @AssignmentEndYear )
		END
		else
		BEGIN
			select @startDate = convert( datetime, convert( nvarchar(2), @AssignmentStartMonth ) + '/1/' + convert( nvarchar(4), @AssignmentStartYear ))
			select @endDate = convert( datetime, convert( nvarchar(2), @AssignmentEndMonth ) + '/' + convert( nvarchar(2), dbo.GetLastDateOfMonthFunction( @AssignmentEndMonth, @AssignmentEndYear ) ) + '/' + convert( nvarchar(4), @AssignmentEndYear ))
			select @whereAssignmentDate = ' and o.Dates_Assigned between ''' + convert( nvarchar(10), @startDate, 101 ) + ''' and ''' + convert( nvarchar(10), @endDate, 101 ) + ''''
		END
	END
	
	if @LastActionStartYear = -1 OR @LastActionEndYear = -1 
	BEGIN
		select @whereLastActionDate = ''
	END
	else
	BEGIN

		if @LastActionStartMonth = -1 OR @LastActionEndMonth = -1
		BEGIN
			select @whereLastActionDate = ' and YEAR(o.Dates_Action) between ' + convert( nvarchar(10), @LastActionStartYear ) + ' and ' + convert( nvarchar(10), @LastActionEndYear )
		END
		else
		BEGIN
			select @startDate = convert( datetime, convert( nvarchar(2), @LastActionStartMonth ) + '/1/' + convert( nvarchar(4), @LastActionStartYear ))
			select @endDate = convert( datetime, convert( nvarchar(2), @LastActionEndMonth ) + '/' + convert( nvarchar(2), dbo.GetLastDateOfMonthFunction( @LastActionEndMonth, @LastActionEndYear ) ) + '/' + convert( nvarchar(4), @LastActionEndYear ))
			select @whereLastActionDate = ' and o.Dates_Action between ''' + convert( nvarchar(10), @startDate, 101 ) + ''' and ''' + convert( nvarchar(10), @endDate, 101 ) + ''''
	END
	END
	
	if @DaysFromAssignmentToLastAction <> -1
	BEGIN
		select @whereDaysFromAssignmentToLastAction = ' and DATEDIFF( dd, o.Dates_Assigned, o.Dates_Action ) >= ' + convert( nvarchar(10),  @DaysFromAssignmentToLastAction )
	END
	else
	BEGIN
		select @whereDaysFromAssignmentToLastAction = ''
	END
	
	select @query = @query + @whereComplete + @whereCurrentAction + @whereSchedule + @whereContractingOfficer + @whereAssignmentDate + @whereLastActionDate + @whereDaysFromAssignmentToLastAction

	
	exec SP_EXECUTESQL @query 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting offers for report.'
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



