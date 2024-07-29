IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectOfferActionHistoryReport')
	BEGIN
		DROP  Procedure  SelectOfferActionHistoryReport
	END

GO

CREATE Procedure SelectOfferActionHistoryReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
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
		@whereComplete nvarchar(100),
		@whereProposalType nvarchar(100),
		@whereContractingOfficer nvarchar(100),
		@whereDuration nvarchar(100),
		@startDate datetime,
		@endDate datetime		

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Offer Action History Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #OfferActionHistoryReport
	(
	Type char(1) not null,
	Proposal_Type_ID int not null,
	LastModificationDate datetime not null,
	Offer_ID int not null,
	[Order] int null,
	OfferNumber nvarchar(30) null,
	Contractor_Name nvarchar(75) not null,
	OriginalCOID int null,
	OriginalCOName nvarchar(80) null,
	NewCOID int null,
	NewCOName nvarchar(80) null,
	ActionDate datetime null,
	CombinedDate datetime not null,
	Action_ID int not null,
	ActionDescription nvarchar(30) null,
	DaysSinceLastAction int null
	)

	create table #FinalOfferActionHistoryReport
	(
	Type char(1) not null,
	Proposal_Type_ID int not null,
	LastModificationDate datetime not null,
	Offer_ID int not null,
	[Order] int null,
	OfferNumber nvarchar(30) null,
	Contractor_Name nvarchar(75) not null,
	OriginalCOID int null,
	OriginalCOName nvarchar(80) null,
	NewCOID int null,
	NewCOName nvarchar(80) null,
	ActionDate datetime null,
	CombinedDate datetime not null,
	Action_ID int not null,
	ActionDescription nvarchar(30) null,
	DaysSinceLastAction int null
	)

	create table #OfferRank
	(
	TheRank int not null,
	Offer_ID int not null,
	LastModificationDate datetime not null 
	)
	
	insert into #OfferActionHistoryReport
	( Type, Proposal_Type_ID, LastModificationDate, Offer_ID, OfferNumber, Contractor_Name, OriginalCOID, OriginalCOName, NewCOID, NewCOName, 
	ActionDate, CombinedDate, Action_ID, ActionDescription, DaysSinceLastAction )
	select a.Type, o.Proposal_Type_ID, a.LastModificationDate, a.Offer_ID, o.OfferNumber, o.Contractor_Name, isnull( x.CO_ID, '' ), isnull( x.FullName, 'none' ) as OriginalCOName, y.CO_ID, y.FullName as NewCOName,
	s.ActionDate, 
	ISNULL( s.ActionDate, a.LastModificationDate ) as CombinedDate,
	isnull( t.Action_ID, 0 ) as Action_ID,
	isnull( t.Action_Description, 'no action' ) as ActionDescription, 0

	from Audit_tbl_Offers a join tbl_Offers o on a.Offer_ID = o.Offer_ID 
	left outer join tlkup_UserProfile x on a.OldValue = x.CO_ID
	join tlkup_UserProfile y on a.NewValue = y.CO_ID

	left outer join
	(
	select Type, GroupID, LastModificationDate, Offer_ID, NewValue as ActionDate
	from Audit_tbl_Offers d
	where d.FieldName = 'Dates_Action'
	) s on a.GroupID = s.GroupID

	left outer join
	(
	select Type, GroupID, LastModificationDate, Offer_ID, NewValue as ActionId
	from Audit_tbl_Offers v
	where v.FieldName = 'Action_ID'
	) q on a.GroupID = q.GroupID

	left outer join tlkup_Offers_Action_Type t on t.Action_ID = q.ActionId

	where a.FieldName = 'CO_ID'


	
	insert into #OfferRank
	( TheRank, Offer_ID, LastModificationDate )
	select RANK() over ( Partition by Offer_ID order by Offer_ID, LastModificationDate ) as ranks, Offer_ID, LastModificationDate
				from #OfferActionHistoryReport t
				group by t.Offer_ID, t.LastModificationDate 
				order by t.Offer_ID, t.LastModificationDate 



	update #OfferActionHistoryReport
	set [Order] = TheRank
	from #OfferActionHistoryReport t join #OfferRank r on t.Offer_ID = r.Offer_ID and t.LastModificationDate = r.LastModificationDate

	
	update x
	set x.DaysSinceLastAction = DATEDIFF( dd, t.CombinedDate, x.CombinedDate )
	from #OfferActionHistoryReport t join #OfferActionHistoryReport x on t.Offer_ID = x.Offer_ID
	where t.[Order] = 1
	and x.[Order] = 2


	update x
	set x.DaysSinceLastAction = DATEDIFF( dd, t.CombinedDate, x.CombinedDate )
	from #OfferActionHistoryReport t join #OfferActionHistoryReport x on t.Offer_ID = x.Offer_ID
	where t.[Order] = 2
	and x.[Order] = 3


	update x
	set x.DaysSinceLastAction = DATEDIFF( dd, t.CombinedDate, x.CombinedDate )
	from #OfferActionHistoryReport t join #OfferActionHistoryReport x on t.Offer_ID = x.Offer_ID
	where t.[Order] = 3
	and x.[Order] = 4


	update x
	set x.DaysSinceLastAction = DATEDIFF( dd, t.CombinedDate, x.CombinedDate )
	from #OfferActionHistoryReport t join #OfferActionHistoryReport x on t.Offer_ID = x.Offer_ID
	where t.[Order] = 4
	and x.[Order] = 5


	update x
	set x.DaysSinceLastAction = DATEDIFF( dd, t.CombinedDate, x.CombinedDate )
	from #OfferActionHistoryReport t join #OfferActionHistoryReport x on t.Offer_ID = x.Offer_ID
	where t.[Order] = 5
	and x.[Order] = 6


	update x
	set x.DaysSinceLastAction = DATEDIFF( dd, t.CombinedDate, x.CombinedDate )
	from #OfferActionHistoryReport t join #OfferActionHistoryReport x on t.Offer_ID = x.Offer_ID
	where t.[Order] = 6
	and x.[Order] = 7

	-- zero
	update x
	set x.DaysSinceLastAction = DATEDIFF( dd, t.CombinedDate, x.CombinedDate )
	from #OfferActionHistoryReport t join #OfferActionHistoryReport x on t.Offer_ID = x.Offer_ID
	where t.[Order] = 7
	and x.[Order] = 8

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
	
	select @query = 'insert into #FinalOfferActionHistoryReport
	( Type, Proposal_Type_ID, LastModificationDate, Offer_ID, [Order], OfferNumber, Contractor_Name, OriginalCOID, OriginalCOName, NewCOID, NewCOName, 
	ActionDate, CombinedDate, Action_ID, ActionDescription, DaysSinceLastAction )

	select t.Type, t.Proposal_Type_ID, t.LastModificationDate, t.Offer_ID, t.[Order], t.OfferNumber, t.Contractor_Name, t.OriginalCOID, t.OriginalCOName, t.NewCOID, t.NewCOName, 
	t.ActionDate, t.CombinedDate, t.Action_ID, t.ActionDescription, t.DaysSinceLastAction
	from #OfferActionHistoryReport  t
	
	left outer join tlkup_Offers_Action_Type a on t.Action_ID = a.Action_ID 
	
	where ( t.LastModificationDate between @startDate_parm and @endDate_parm
	or ( t.ActionDate between @startDate_parm and @endDate_parm and t.ActionDate is not null )) '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string.'
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
	
	if @OfferProposalType <> -1
	BEGIN
		select @whereProposalType = ' and t.Proposal_Type_ID = ' + convert( nvarchar(10), @OfferProposalType )
	END
	else
	BEGIN
		select @whereProposalType = ' '
	END
	
	if @ContractingOfficerId <> -1
	BEGIN
		select @whereContractingOfficer = ' and ( t.OriginalCOID = ' + convert( nvarchar(10), @ContractingOfficerId ) + ' or t.NewCOID = ' + convert( nvarchar(10), @ContractingOfficerId ) + ' ) '
	END
	else
	BEGIN
		select @whereContractingOfficer = ' '
	END

	if @Duration <> -1
	BEGIN
		if @Duration = 120
		BEGIN
			select @whereDuration = ' and t.DaysSinceLastAction between 0 and 120 '
		END
		else if @Duration = 180
		BEGIN
			select @whereDuration = ' and t.DaysSinceLastAction  between 121 and 180 '
		END
		else if @Duration = 240
		BEGIN
			select @whereDuration = ' and t.DaysSinceLastAction  between 181 and 240 '
		END
		else if @Duration = 365
		BEGIN
			select @whereDuration = ' and t.DaysSinceLastAction  between 241 and 365 '
		END
		else -- gt 365
		BEGIN
			select @whereDuration = ' and t.DaysSinceLastAction  > 365 '
		END
	END
	else
	BEGIN
		select @whereDuration = ''
	END

	select @sqlParameters = '@startDate_parm datetime, @endDate_parm datetime'
	
	select @query = @query + @whereComplete + @whereProposalType + @whereContractingOfficer + @whereDuration 
	
	exec SP_EXECUTESQL @query, @sqlParameters, @startDate_parm = @startDate, @endDate_parm = @endDate

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting offers into temp table for report.'
		goto ERROREXIT
	END

	select 

	t.Offer_ID,
	t.[Order],
	t.OfferNumber,
	t.Contractor_Name,
	t.OriginalCOName,
	t.NewCOName,
	t.ActionDate,
	t.LastModificationDate,
	t.CombinedDate,
	t.ActionDescription,
	t.DaysSinceLastAction,
	s.Schedule_Name,
	p.Proposal_Type_Description,
	o.POC_Primary_Name, o.POC_Primary_Phone, o.POC_Primary_Email,
	o.Dates_Received, o.Dates_Assigned, o.Dates_Reassigned, o.Dates_Action, o.Dates_Expected_Completion, o.Dates_Expiration,
	o.Dates_Sent_for_Preaward, o.Dates_Returned_to_Office, o.Audit_Indicator, o.ContractNumber, o.Date_Entered, o.Date_Modified, o.CreatedBy, o.LastModifiedBy, o.Comments
	from #FinalOfferActionHistoryReport t join tbl_Offers o on t.Offer_ID = o.Offer_ID	
	join tlkup_Offers_Proposal_Type p on p.Proposal_Type_ID = o.Proposal_Type_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number
	order by t.Offer_ID, t.[Order]

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


