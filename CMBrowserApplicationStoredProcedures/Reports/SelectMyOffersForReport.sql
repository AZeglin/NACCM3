IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectMyOffersForReport')
	BEGIN
		DROP  Procedure  SelectMyOffersForReport
	END

GO

CREATE Procedure SelectMyOffersForReport
(
@ReportUserLoginId nvarchar(100), /* also used as a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@CompletionStatus int    /* -1 = all */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3000),
		@SQLParms nvarchar(1000),
		@joinSecurityServerName nvarchar(1000),
		@whereComplete nvarchar(100),
	 	@ReportUserCOID int

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Offers Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'

	select @query = 'select @ReportUserCOID_parm = u.CO_ID
	from ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u
	where u.UserName = @ReportUserLoginId_parm '
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
	
	select @SQLParms = N'@ReportUserLoginId_parm nvarchar(100), @ReportUserCOID_parm int OUTPUT'

	exec SP_EXECUTESQL @query, @SQLParms, @ReportUserLoginId_parm = @ReportUserLoginId, @ReportUserCOID_parm = @ReportUserCOID OUTPUT

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting CO_ID for LoginId for My Offers report.'
		goto ERROREXIT
	END	
	
	select @query = 'select o.Offer_ID, o.Solicitation_ID, n.Solicitation_Number, o.CO_ID, p.FullName, o.Schedule_Number, s.Schedule_Name, s.Division, 
		o.Proposal_Type_ID, t.Proposal_Type_Description, o.Action_ID, a.Action_Description, a.Complete as IsActionComplete, o.Contractor_Name, o.Dates_Received, 
		o.Dates_Assigned, o.Dates_Reassigned, o.Dates_Action, o.Dates_Expected_Completion, o.Dates_Expiration, o.Dates_Sent_for_Preaward, o.Dates_Returned_to_Office,
		o.Audit_Indicator, o.ContractNumber, o.Date_Entered, o.Date_Modified, 
		case when o.Dates_Reassigned is null then DATEDIFF( dd, o.Dates_Assigned, o.Dates_Action ) else DATEDIFF( dd, o.Dates_Reassigned, o.Dates_Action ) end as DaysFromAssignmentReassignmentToLastAction,	
		DATEDIFF( dd, o.Dates_Assigned, getdate() ) as DaysFromAssignmentToToday, 
		DATEDIFF( dd, o.Dates_Action, getdate() ) as DaysFromLastActionToToday,
		DATEDIFF( dd, o.Dates_Reassigned, getdate() ) as DaysFromReassignmentToToday,
		case when o.Dates_Reassigned is null then DATEDIFF( dd, o.Dates_Assigned, getdate() ) else DATEDIFF( dd, o.Dates_Reassigned, getdate() ) end as DaysFromAssignmentReassignmentToToday,
		case when o.Dates_Reassigned is null then DATEDIFF( dd, o.Dates_Assigned, o.Date_Modified ) else DATEDIFF( dd, o.Dates_Reassigned, o.Date_Modified ) end as DaysFromAssignmentReassignmentToLastModification	
	from tbl_Offers o join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] p on o.CO_ID = p.CO_ID
	join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
	join tlkup_Offers_Proposal_Type t on o.Proposal_Type_ID = t.Proposal_Type_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number 
	join tlkup_Solicitation_Numbers n on o.Solicitation_ID = n.Solicitation_ID 
	where o.CO_ID = ' + convert( nvarchar(10), @ReportUserCOID )

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
	
	select @query = @query + @whereComplete 
	
	exec SP_EXECUTESQL @query 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for my offers report.'
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


