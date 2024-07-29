IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'OffersByRoleReport' )
BEGIN
	DROP PROCEDURE OffersByRoleReport
END
GO

CREATE PROCEDURE OffersByRoleReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SelectedRole char(1)   /* A = Alpha, O = Omega, L = Lambda, P = PMRS, X = all */
)

AS


DECLARE		@month int,
			@year int,
			@rowCount int,
			@error int,
			@errorMsg nvarchar(200),
			@sevenDaysAgo date,
			@today date

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Offers By Role Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	create table #OffersByGroup
	(
	COName nvarchar(120) NULL,
	LastName nvarchar(120) NULL,
	RoleDescription nvarchar(120) NULL,
	ShortRoleDescription nvarchar(120) NULL,
	RoleKey char(1) NULL,
	Dates_Received datetime null,
	Dates_Assigned datetime NULL,
	Dates_Reassigned datetime NULL,
	Contractor_Name nvarchar(100) NULL,
	Schedule_Name nvarchar(100) NULL,
	Proposal_Type_Description nvarchar(100), 
	Action_Description nvarchar(100),
	Dates_Action datetime NULL,
	TotalDurationSinceReceipt int NULL,
	TotalDurationSinceAssignment int NULL,
	TotalDurationSinceReassignment int NULL,
	Complete bit NULL,
	Comment nvarchar(4000) null
	)

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table 1 for Offer Statistics By Role Report'
		goto ERROREXIT
	END

	insert into #OffersByGroup
	(
	COName,
	LastName,
	RoleDescription,
	Dates_Received,
	Dates_Assigned,
	Dates_Reassigned,
	Contractor_Name,
	Schedule_Name,
	Proposal_Type_Description, 
	Action_Description,
	Dates_Action,
	TotalDurationSinceReceipt,
	TotalDurationSinceAssignment ,
	TotalDurationSinceReassignment,
	Complete,
	Comment
	)
	select u.FullName, u.LastName,
	( select top 1 r.RoleDescription from NACSEC.dbo.SEC_Roles r join NACSEC.dbo.SEC_UserProfileUserRoles f on f.RoleId = r.RoleId where f.CO_ID = o.CO_ID and r.RoleId in ( 15, 27, 28, 29, 30, 31, 32, 33, 34, 35 ) ) as RoleDescription,
	o.Dates_Received, o.Dates_Assigned, o.Dates_Reassigned, o.Contractor_Name, s.Schedule_Name, p.Proposal_Type_Description, t.Action_Description, o.Dates_Action,
	DATEDIFF( dd, o.Dates_Received, GETDATE() ) as TotalDurationSinceReceipt,
	DATEDIFF( dd, o.Dates_Assigned, GETDATE() ) as TotalDurationSinceAssignment,
	DATEDIFF( dd, o.Dates_Reassigned, GETDATE() ) as TotalDurationSinceReassignment,
	t.Complete,
	o.Comments
	from tbl_Offers o join tlkup_Offers_Action_Type t on o.Action_ID = t.Action_ID
	join tlkup_UserProfile u on o.CO_ID = u.CO_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number
	join tlkup_Offers_Proposal_Type p on o.Proposal_Type_ID = p.Proposal_Type_ID
	where t.Complete = 0

	union

	select u.FullName, u.LastName,
	( select top 1 r.RoleDescription from NACSEC.dbo.SEC_Roles r join NACSEC.dbo.SEC_UserProfileUserRoles f on f.RoleId = r.RoleId where f.CO_ID = o.CO_ID and r.RoleId in ( 15, 27, 28, 29, 30, 31, 32, 33, 34, 35 ) ) as RoleDescription,
	o.Dates_Received, o.Dates_Assigned, o.Dates_Reassigned, o.Contractor_Name, s.Schedule_Name, p.Proposal_Type_Description, t.Action_Description, o.Dates_Action,
	DATEDIFF( dd, o.Dates_Received, o.Dates_Action ) as TotalDurationSinceReceipt,
	DATEDIFF( dd, o.Dates_Assigned, o.Dates_Action ) as TotalDurationSinceAssignment,
	DATEDIFF( dd, o.Dates_Reassigned, o.Dates_Action ) as TotalDurationSinceReassignment,
	t.Complete,
	o.Comments
	from tbl_Offers o join tlkup_Offers_Action_Type t on o.Action_ID = t.Action_ID
	join tlkup_UserProfile u on o.CO_ID = u.CO_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number
	join tlkup_Offers_Proposal_Type p on o.Proposal_Type_ID = p.Proposal_Type_ID
	where t.Complete = 1

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting into temp table 1 for Offer Statistics By Role Report'
		goto ERROREXIT
	END

	select @today = CONVERT( DATE, CONVERT( NVARCHAR(4), YEAR(GETDATE())) + '/' + CONVERT( NVARCHAR(2), MONTH(GETDATE())) + '/' + CONVERT( NVARCHAR(2), DAY(GETDATE())))	

	select @sevenDaysAgo = CONVERT( DATE, CONVERT( NVARCHAR(4), YEAR(DATEADD(dd, -7, getdate() ))) + '/' + CONVERT( NVARCHAR(2), MONTH(DATEADD(dd, -7, getdate() ))) + '/' + CONVERT( NVARCHAR(2), DAY(DATEADD(dd, -7, getdate() ))))

	/* remove completed that are older than 7 days HOWEVER, include today's completions - this is different than the count version of the report */
	delete #OffersByGroup
	where Complete = 1
	and Dates_Action < @sevenDaysAgo

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting older completions from temp table'
		goto ERROREXIT
	END

	update #OffersByGroup
	set ShortRoleDescription = 'Services',
	RoleKey = 'L'
	where RoleDescription like '%Lambda%'

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with role name 1'
		goto ERROREXIT
	END

	update #OffersByGroup
	set ShortRoleDescription = 'PharmDental',
	RoleKey = 'O'
	where RoleDescription like '%Omega%'

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with role name 2'
		goto ERROREXIT
	END

	update #OffersByGroup
	set ShortRoleDescription = 'MedSurg',
	RoleKey = 'A'
	where RoleDescription like '%Alpha%'

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with role name 3'
		goto ERROREXIT
	END
	
	update #OffersByGroup
	set ShortRoleDescription = 'PMRS',
	RoleKey = 'P'
	where RoleDescription like '%Entry%'
	
	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table with role name 4'
		goto ERROREXIT
	END

	if @SelectedRole = 'X'
	BEGIN
		select COName,
			LastName,
			ShortRoleDescription,
			RoleKey,
			Dates_Received,
			Dates_Assigned ,
			Dates_Reassigned,
			Contractor_Name,
			Schedule_Name ,
			Proposal_Type_Description, 
			Action_Description,
			Dates_Action,
			TotalDurationSinceReceipt,
			TotalDurationSinceAssignment ,
			TotalDurationSinceReassignment,
			Complete,
			Comment
		from #OffersByGroup
		order by Proposal_Type_Description, ShortRoleDescription, Schedule_Name

		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error making final select(1) from temp table.'
			goto ERROREXIT
		END			
	END
	else
	BEGIN
		select COName,
			LastName,
			ShortRoleDescription,
			RoleKey,
			Dates_Received,
			Dates_Assigned ,
			Dates_Reassigned,
			Contractor_Name,
			Schedule_Name ,
			Proposal_Type_Description, 
			Action_Description,
			Dates_Action,
			TotalDurationSinceReceipt,
			TotalDurationSinceAssignment ,
			TotalDurationSinceReassignment,
			Complete,
			Comment
		from #OffersByGroup
		where RoleKey = @SelectedRole
		order by Proposal_Type_Description, ShortRoleDescription, Schedule_Name

		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error making final select(2) from temp table.'
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
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 


