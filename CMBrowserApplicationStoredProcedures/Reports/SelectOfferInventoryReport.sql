IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectOfferInventoryReport')
	BEGIN
		DROP  Procedure  SelectOfferInventoryReport
	END

GO

CREATE Procedure SelectOfferInventoryReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@OfferProposalType int, /* -1 = all, 1 = offer proposal, 2 = contract extension proposal */
@CompletionStatus int,    /* -1 = all, 0,1 */
@ContractingOfficerId int, /* -1 = all */
@StartYear int,
@StartMonth int, 
@EndYear int,
@EndMonth int,
@Duration int  /* -1 = all */
)


AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(4000),
		@sqlParameters nvarchar(1000),
		@orderByClause nvarchar(200),
		@joinSecurityServerName nvarchar(1000),
		@whereDates1 nvarchar(500),
		@whereDates2 nvarchar(500),
		@whereComplete nvarchar(100),
		@whereProposalType nvarchar(100),
		@whereContractingOfficer nvarchar(100),
		@whereDuration nvarchar(100),
		@startDate datetime,
		@endDate datetime		

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Offer Inventory Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #OfferInventoryReport
	(
		Offer_ID int not null, 
		OfferNumber nvarchar(30) null,  -- new
		Solicitation_ID int not null, 
		Solicitation_Number nvarchar(50) not null, 
		CO_ID int not null, 
		FullName nvarchar(50) null, 
		LastName nvarchar(20) null,
		UserName nvarchar(30) null,
		Schedule_Number int not null, 
		Schedule_Name nvarchar(75) null, 
		Division smallint null, 
		Proposal_Type_ID int not null,
		Proposal_Type_Description nvarchar(30) not null, 
		Action_ID int not null, 
		Action_Description nvarchar(30) not null, 
		IsActionComplete bit not null,
		Contractor_Name nvarchar(75) not null,
		Dates_Received datetime not null,
		Dates_Assigned datetime null, 
		Dates_Reassigned datetime null, 
		Dates_Action datetime not null, 
		Dates_Expected_Completion datetime null, 
		Dates_Expiration datetime null, 
		Dates_Sent_for_Preaward datetime null, 
		Dates_Returned_to_Office datetime null,
		Audit_Indicator bit not null, 
		ContractNumber nvarchar(20) null, 
		ExtendsContractNumber nvarchar(20) null,  -- new
		ExtendsContractEffectiveDate datetime null,  -- new
		ExtendsContractExpirationDate datetime null,  -- new
		Date_Entered datetime null, 
		Date_Modified datetime null, 
		DaysFromReceiptToToday int not null,
		DaysFromAssignmentToToday int null, 
		DaysFromReassignmentToToday int null, 
		DaysFromLastActionToToday int not null, 
		DaysFromReceiptToAssignment int null,
		DaysFromAssignmentToReassignment int null, 
		DaysFromAssignmentToLastAction int  null, 
		DaysFromReassignmentToLastAction int null,
		DaysFromAssignmentToLastModification int  null,   -- new
		DaysFromReassignmentToLastModification int null,   -- new
		DaysFromReceiptToLastAction int null,
		Comments nvarchar(4000) null     -- new
	)



	select @startDate = convert( datetime, convert( nvarchar(2), @StartMonth ) + '/1/' + convert( nvarchar(4), @StartYear ))

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting start date.'
		goto ERROREXIT
	END
	
	select @endDate = convert( datetime, convert( nvarchar(2), @EndMonth ) + '/' + convert( nvarchar(2), dbo.GetLastDateOfMonthFunction( @EndMonth, @EndYear ) ) + '/' + convert( nvarchar(4), @EndYear ))

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting end date.'
		goto ERROREXIT
	END
	
	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'
	
	select @query = 'insert into #OfferInventoryReport
		(	Offer_ID,
			OfferNumber,
			Solicitation_ID,
			Solicitation_Number,
			CO_ID,
			FullName,
			LastName,
			UserName,
			Schedule_Number,
			Schedule_Name ,
			Division,
			Proposal_Type_ID,
			Proposal_Type_Description, 
			Action_ID,
			Action_Description,
			IsActionComplete,
			Contractor_Name,
			Dates_Received,
			Dates_Assigned,
			Dates_Reassigned,
			Dates_Action,
			Dates_Expected_Completion,
			Dates_Expiration,
			Dates_Sent_for_Preaward,
			Dates_Returned_to_Office,
			Audit_Indicator,
			ContractNumber,
			ExtendsContractNumber,
			ExtendsContractEffectiveDate,
			ExtendsContractExpirationDate,
			Date_Entered,
			Date_Modified,
			DaysFromReceiptToToday,
			DaysFromAssignmentToToday,
			DaysFromReassignmentToToday,
			DaysFromLastActionToToday,
			DaysFromReceiptToAssignment,
			DaysFromAssignmentToReassignment,
			DaysFromAssignmentToLastAction,
			DaysFromReassignmentToLastAction,
			DaysFromAssignmentToLastModification,
			DaysFromReassignmentToLastModification,
			DaysFromReceiptToLastAction,
			Comments
		)	
	
	 select o.Offer_ID, o.OfferNumber, o.Solicitation_ID, n.Solicitation_Number, o.CO_ID, p.FullName, p.LastName, p.UserName, o.Schedule_Number, s.Schedule_Name, s.Division, 
		o.Proposal_Type_ID, t.Proposal_Type_Description, o.Action_ID, a.Action_Description, a.Complete as IsActionComplete, o.Contractor_Name,
		o.Dates_Received, o.Dates_Assigned, o.Dates_Reassigned, o.Dates_Action, o.Dates_Expected_Completion, o.Dates_Expiration, o.Dates_Sent_for_Preaward, o.Dates_Returned_to_Office,
		o.Audit_Indicator, o.ContractNumber, o.ExtendsContractNumber, x.Dates_Effective, x.Dates_CntrctExp, o.Date_Entered, o.Date_Modified, 
		DATEDIFF( dd, o.Dates_Received, getdate() ) as DaysFromReceiptToToday, 
		DATEDIFF( dd, o.Dates_Assigned, getdate() ) as DaysFromAssignmentToToday, 
		DATEDIFF( dd, o.Dates_Reassigned, getdate() ) as DaysFromReassignmentToToday, 
		DATEDIFF( dd, o.Dates_Action, getdate() ) as DaysFromLastActionToToday,
		DATEDIFF( dd, o.Dates_Received, o.Dates_Assigned ) as DaysFromReceiptToAssignment,
		DATEDIFF( dd, o.Dates_Assigned, o.Dates_Reassigned ) as DaysFromAssignmentToReassignment,
		DATEDIFF( dd, o.Dates_Assigned, o.Dates_Action ) as DaysFromAssignmentToLastAction,
		DATEDIFF( dd, o.Dates_Reassigned, o.Dates_Action ) as DaysFromReassignmentToLastAction,
		DATEDIFF( dd, o.Dates_Assigned, o.Date_Modified ) as DaysFromAssignmentToLastModification,
		DATEDIFF( dd, o.Dates_Reassigned, o.Date_Modified ) as DaysFromReassignmentToLastModification,
		DATEDIFF( dd, o.Dates_Received, o.Dates_Action ) as DaysFromReceiptToLastAction,
		o.Comments
	
	from tbl_Offers o join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] p on o.CO_ID = p.CO_ID
	join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
	join tlkup_Offers_Proposal_Type t on o.Proposal_Type_ID = t.Proposal_Type_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number 
	join tlkup_Solicitation_Numbers n on o.Solicitation_ID = n.Solicitation_ID 
	left outer join tbl_Cntrcts x on o.ExtendsContractNumber = x.CntrctNum '
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string.'
		goto ERROREXIT
	END

	select @WhereDates1 = '	where ( o.Dates_Received between @startDate_parm and @endDate_parm
	or ( o.Dates_Assigned between @startDate_parm and @endDate_parm and o.Dates_Assigned is not null )
	or ( o.Dates_Reassigned between @startDate_parm and @endDate_parm and o.Dates_Reassigned is not null )
	or ( o.Date_Modified between @startDate_parm and @endDate_parm and o.Date_Modified is not null )
	or ( o.Dates_Action between @startDate_parm and @endDate_parm and o.Dates_Action is not null )) '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning where 1 string.'
		goto ERROREXIT
	END

	select @WhereDates2 = '	where o.Dates_Action between @startDate_parm and @endDate_parm and o.Dates_Action is not null '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning where 2 string.'
		goto ERROREXIT
	END

	if @CompletionStatus = 1
	BEGIN
		select @query = @query + @whereDates2
	END
	else
	BEGIN
		select @query = @query + @whereDates1
	END

	if @CompletionStatus <> -1
	BEGIN	
		select @whereComplete = ' and a.Complete = ' + convert( char(1), convert( bit, ( case when ( @CompletionStatus = 0 ) then 0 else 1 end )))
	END
	else
	BEGIN
		select @whereComplete = ''
	END
	
	if @OfferProposalType <> -1
	BEGIN
		select @whereProposalType = ' and o.Proposal_Type_ID = ' + convert( nvarchar(10), @OfferProposalType )
	END
	else
	BEGIN
		select @whereProposalType = ' '
	END
	
	if @ContractingOfficerId <> -1
	BEGIN
		select @whereContractingOfficer = ' and o.CO_ID = ' + convert( nvarchar(10), @ContractingOfficerId )
	END
	else
	BEGIN
		select @whereContractingOfficer = ' '
	END

	if @Duration <> -1
	BEGIN
		if @Duration = 120
		BEGIN
			select @whereDuration = ' and DATEDIFF( dd, o.Dates_Received, getdate() ) between 0 and 120 '
		END
		else if @Duration = 180
		BEGIN
			select @whereDuration = ' and DATEDIFF( dd, o.Dates_Received, getdate() ) between 121 and 180 '
		END
		else if @Duration = 240
		BEGIN
			select @whereDuration = ' and DATEDIFF( dd, o.Dates_Received, getdate() ) between 181 and 240 '
		END
		else if @Duration = 365
		BEGIN
			select @whereDuration = ' and DATEDIFF( dd, o.Dates_Received, getdate() ) between 241 and 365 '
		END
		else -- gt 365
		BEGIN
			select @whereDuration = ' and DATEDIFF( dd, o.Dates_Received, getdate() ) > 365 '
		END
	END
	else
	BEGIN
		select @whereDuration = ''
	END

	select @orderByClause = ' order by o.Dates_Assigned '

	select @sqlParameters = '@startDate_parm datetime, @endDate_parm datetime'
	
	select @query = @query + @whereComplete + @whereProposalType + @whereContractingOfficer + @whereDuration + @orderByClause
	
	exec SP_EXECUTESQL @query, @sqlParameters, @startDate_parm = @startDate, @endDate_parm = @endDate

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting offers into temp table for report.'
		goto ERROREXIT
	END

	select Offer_ID,
			OfferNumber,
			Solicitation_ID,
			Solicitation_Number,
			CO_ID,
			FullName,
			LastName,
			UserName,
			Schedule_Number,
			Schedule_Name ,
			Division,
			Proposal_Type_ID,
			Proposal_Type_Description, 
			Action_ID,
			Action_Description,
			IsActionComplete,
			Contractor_Name,
			Dates_Received,
			Dates_Assigned,
			Dates_Reassigned,
			Dates_Action,
			Dates_Expected_Completion,
			Dates_Expiration,
			Dates_Sent_for_Preaward,
			Dates_Returned_to_Office,
			Audit_Indicator,
			ContractNumber,
			ExtendsContractNumber,
			ExtendsContractEffectiveDate,
			ExtendsContractExpirationDate,
			Date_Entered,
			Date_Modified,
			DaysFromReceiptToToday,
			DaysFromAssignmentToToday,
			DaysFromReassignmentToToday,
			DaysFromLastActionToToday,
			DaysFromReceiptToAssignment,
			DaysFromAssignmentToReassignment,
			DaysFromAssignmentToLastAction,
			DaysFromReassignmentToLastAction,
			DaysFromAssignmentToLastModification,
			DaysFromReassignmentToLastModification,
			DaysFromReceiptToLastAction,
			Comments

	from #OfferInventoryReport
		

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


