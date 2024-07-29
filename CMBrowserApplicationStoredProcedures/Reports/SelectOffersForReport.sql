IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectOffersForReport')
	BEGIN
		DROP  Procedure  SelectOffersForReport
	END

GO

CREATE Procedure SelectOffersForReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@CompletionStatus int,    /* -1 = all */
@CurrentActionId int, /* -1 = all */
@ScheduleNumber int, /* -1 = all */
@ContractingOfficerId int, /* -1 = all */
@AssignmentYear int, /* -1 all */
@AssignmentMonth int, /* -1 all */
@LastActionYear int, /* -1 all */
@LastActionMonth int, /* -1 all */
@DaysFromAssignmentToLastAction int /* -1 all */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3000),
		@joinSecurityServerName nvarchar(1000),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@whereComplete nvarchar(100),
		@whereCurrentAction nvarchar(100),
		@whereAssignmentYear nvarchar(100),
		@whereAssignmentMonth nvarchar(100),
		@whereLastActionYear nvarchar(100),
		@whereLastActionMonth nvarchar(100),
		@whereDaysFromAssignmentToLastAction nvarchar(100),
	 	@SERVERNAME nvarchar(255)

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
	
	if @AssignmentYear <> -1
	BEGIN
		select @whereAssignmentYear = ' and YEAR(o.Dates_Assigned) = ' + convert( nvarchar(10), @AssignmentYear )
	END
	else
	BEGIN
		select @whereAssignmentYear = ''
	END
	
	if @AssignmentMonth <> -1
	BEGIN
		select @whereAssignmentMonth = ' and MONTH(o.Dates_Assigned) = ' + convert( nvarchar(10), @AssignmentMonth )
	END
	else
	BEGIN
		select @whereAssignmentMonth = ''
	END
	
	if @LastActionYear <> -1
	BEGIN
		select @whereLastActionYear = ' and YEAR(o.Dates_Action) = ' + convert( nvarchar(10), @LastActionYear )
	END
	else
	BEGIN
		select @whereLastActionYear = ''
	END
	
	if @LastActionMonth <> -1
	BEGIN
		select @whereLastActionMonth = ' and MONTH(o.Dates_Action) = ' + convert( nvarchar(10), @LastActionMonth )
	END
	else
	BEGIN
		select @whereLastActionMonth = ''
	END
	
	if @DaysFromAssignmentToLastAction <> -1
	BEGIN
		select @whereDaysFromAssignmentToLastAction = ' and DATEDIFF( dd, o.Dates_Assigned, o.Dates_Action ) >= ' + convert( nvarchar(10),  @DaysFromAssignmentToLastAction )
	END
	else
	BEGIN
		select @whereDaysFromAssignmentToLastAction = ''
	END
	
	select @query = @query + @whereComplete + @whereCurrentAction + @whereSchedule + @whereContractingOfficer + @whereAssignmentYear + @whereAssignmentMonth + @whereLastActionYear + @whereLastActionMonth + @whereDaysFromAssignmentToLastAction

	
	exec SP_EXECUTESQL @query 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for active contract report.'
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


