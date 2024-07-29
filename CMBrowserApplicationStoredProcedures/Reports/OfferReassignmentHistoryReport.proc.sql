IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'OfferReassignmentHistoryReport' )
BEGIN
	DROP PROCEDURE OfferReassignmentHistoryReport
END
GO

CREATE PROCEDURE OfferReassignmentHistoryReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@OfferProposalType int, /* -1 = all, 1 = offer proposal, 2 = contract extension proposal */
@CompletionStatus int,    /* -1 = all, 0,1 */
@StartYear int,
@StartMonth int, 
@EndYear int,
@EndMonth int,
@ContractingOfficerId int	 /* -1 = all */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3000),
		@sqlParameters nvarchar(1000),
		@orderByClause nvarchar(200),
		@joinSecurityServerName nvarchar(1000),
		@whereComplete nvarchar(100),
		@whereProposalType nvarchar(100),
		@whereContractingOfficer nvarchar(100),
		@whereDuration nvarchar(100),
		@startDate datetime,
		@endDate datetime,
		@ListOfAuditID nvarchar(1000),
		@Offer_ID int

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Offer Reassignment History Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #OfferReassignmentHistoryReport
	(
		AuditId int not null,
		GroupID int not null,
		Offer_ID int not null, 
		Solicitation_Number nvarchar(50) not null, 
		Schedule_Name nvarchar(75) null, 
		Division smallint null, 
		Contractor_Name nvarchar(75) not null,
		Proposal_Type_Description nvarchar(30) not null, 
		Action_Description nvarchar(30) not null, 
		IsActionComplete bit not null,
		IsInitialAssignment bit null,
		OldCOFullName nvarchar(50) null, 
		OldCOLastName nvarchar(20) null,
		NewCOFullName nvarchar(50) null, 
		NewCOLastName nvarchar(20) null,
		ReassignmentDate datetime null,
		ReassignmentDateSource int null,
		LastModificationDate datetime null,
		ModifiedBy nvarchar(120)
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
	

	select @query = 'insert into #OfferReassignmentHistoryReport
	( AuditId, GroupID, Offer_ID, Solicitation_Number, Schedule_Name, Division, Contractor_Name, 
		Proposal_Type_Description, Action_Description, IsActionComplete, IsInitialAssignment,
	    OldCOFullName, OldCOLastName, NewCOFullName, NewCOLastName, LastModificationDate, ModifiedBy )
	select a.AuditId, a.GroupID, a.Offer_ID, x.Solicitation_Number, s.Schedule_Name, s.Division,  o.Contractor_Name, 
		t.Proposal_Type_Description, c.Action_Description, c.Complete as IsActionComplete, 0,
		u.FullName as OldCOFullName, u.LastName as OldCOLastName, n.FullName as NewCOFullName, n.LastName as NewCOLastName, a.LastModificationDate, 
		case when m.FullName like ''%zeglin'' or m.FullName like ''%andem'' then ''Administrator'' else m.FullName end as ModifiedBy
	
	from Audit_tbl_Offers a join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on a.OldValue = u.CO_ID
	join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile]  n on a.NewValue = n.CO_ID
	join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] m on a.LastModifiedBy = m.UserName
	join tbl_Offers o on a.Offer_ID = o.Offer_ID
	join tlkup_Offers_Action_Type c on o.Action_ID = c.Action_ID
	join tlkup_Offers_Proposal_Type t on o.Proposal_Type_ID = t.Proposal_Type_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number 
	join tlkup_Solicitation_Numbers x on o.Solicitation_ID = x.Solicitation_ID
	where a.FieldName = ''CO_ID'' and a.OldValue is not null
	and ( o.Dates_Assigned between @startDate_parm and @endDate_parm 
	or ( o.Dates_Reassigned between @startDate_parm and @endDate_parm and o.Dates_Reassigned is not null )
	or ( o.Dates_Action between @startDate_parm and @endDate_parm and o.Dates_Action is not null )
	or ( a.LastModificationDate between @startDate_parm and @endDate_parm )) '
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string.'
		goto ERROREXIT
	END

	if @CompletionStatus <> -1
	BEGIN
		select @whereComplete = ' and c.Complete = ' + convert( char(1), convert( bit, ( case when ( @CompletionStatus = 0 ) then 0 else 1 end )))
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
		select @whereContractingOfficer = ' and ( a.OldValue = ' + convert( nvarchar(10), @ContractingOfficerId ) + ' or a.NewValue = '  + convert( nvarchar(10), @ContractingOfficerId ) + ' ) '
	END
	else
	BEGIN
		select @whereContractingOfficer = ' '
	END
	
	select @sqlParameters = '@startDate_parm datetime, @endDate_parm datetime'
	
	select @query = @query + @whereComplete + @whereProposalType + @whereContractingOfficer
	
	exec SP_EXECUTESQL @query, @sqlParameters, @startDate_parm = @startDate, @endDate_parm = @endDate


	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting offer history into temp table.'
		goto ERROREXIT
	END

	/* backfill IsInitialAssignment flag based on NULL assignment date in same group */
	update #OfferReassignmentHistoryReport
		set IsInitialAssignment = 1
	from #OfferReassignmentHistoryReport r	
	where exists ( select a.NewValue from Audit_tbl_Offers a
								where a.Offer_ID =  r.Offer_ID
								and a.GroupID = r.GroupID
								and a.FieldName = 'Dates_Assigned'
								and a.OldValue is null )
	
	/* backfill reassignment dates from the same group */
	update #OfferReassignmentHistoryReport
		set ReassignmentDate = ( select top 1 a.NewValue from Audit_tbl_Offers a
								where a.Offer_ID =  r.Offer_ID
								and a.GroupID = r.GroupID
								and a.FieldName = 'Dates_Reassigned'
								and a.NewValue is not null ),
			ReassignmentDateSource = 1
	from #OfferReassignmentHistoryReport r


	/* backfill missing reassignment dates from the next applicable group, if possible */
	update #OfferReassignmentHistoryReport
		set ReassignmentDate = ( select top 1 a.NewValue from Audit_tbl_Offers a
								where a.Offer_ID =  r.Offer_ID
								and a.FieldName = 'Dates_Reassigned'
								and a.NewValue is not null 
								and a.AuditId between r.AuditId 
										and ( select min(AuditId) from Audit_tbl_Offers y 
												where y.Offer_ID = r.Offer_ID
												and y.FieldName = 'CO_ID'
												and y.AuditId > r.AuditId
												and y.NewValue is not null )						
								),
		ReassignmentDateSource = 2
	from #OfferReassignmentHistoryReport r
	where r.ReassignmentDate is null

	/* backfill missing reassignment dates from the last applicable group, if possible */
	update #OfferReassignmentHistoryReport
		set ReassignmentDate = ( select top 1 a.NewValue from Audit_tbl_Offers a
								where a.Offer_ID =  r.Offer_ID
								and a.FieldName = 'Dates_Reassigned'
								and a.NewValue is not null 
								and a.AuditId > r.AuditId ),
		ReassignmentDateSource = 3
	from #OfferReassignmentHistoryReport r
	where r.ReassignmentDate is null

	select 	AuditId, Offer_ID, GroupID, Solicitation_Number, Schedule_Name, Division, Contractor_Name, 
		Proposal_Type_Description, Action_Description, IsActionComplete,
	    OldCOFullName, OldCOLastName, NewCOFullName, NewCOLastName, ReassignmentDate, ReassignmentDateSource, LastModificationDate, ModifiedBy
	from #OfferReassignmentHistoryReport
	where IsInitialAssignment = 0
	order by Contractor_Name, LastModificationDate 

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting offer history.'
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


