IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'OfferStatisticsByRoleReport' )
BEGIN
	DROP PROCEDURE OfferStatisticsByRoleReport
END
GO

CREATE PROCEDURE OfferStatisticsByRoleReport
(
@ReportUserLoginId nvarchar(100) /* running the report, not a selection criteria */
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
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Offer Statistics By Role Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	create table #OffersByGroup
	(
	COName nvarchar(120) NULL,
	RoleDescription nvarchar(120) NULL,
	Dates_Received datetime null,
	Dates_Assigned datetime NULL,
	Dates_Reassigned datetime NULL,
	Contractor_Name nvarchar(100) NULL,
	Schedule_Name nvarchar(100) NULL,
	Proposal_Type_Description nvarchar(100), 
	Action_Description nvarchar(100),
	Dates_Action datetime NULL,
	TotalDuration int NULL,
	Complete bit NULL
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
	RoleDescription,
	Dates_Received,
	Dates_Assigned,
	Dates_Reassigned,
	Contractor_Name,
	Schedule_Name,
	Proposal_Type_Description, 
	Action_Description,
	Dates_Action,
	TotalDuration ,
	Complete
	)
	select u.FullName, 
	( select top 1 r.RoleDescription from NACSEC.dbo.SEC_Roles r join NACSEC.dbo.SEC_UserProfileUserRoles f on f.RoleId = r.RoleId where f.CO_ID = o.CO_ID and r.RoleId in ( 15, 27, 28, 29, 30, 31, 32, 33, 34, 35 ) ) as RoleDescription,
	o.Dates_Received, o.Dates_Assigned, o.Dates_Reassigned, o.Contractor_Name, s.Schedule_Name, p.Proposal_Type_Description, t.Action_Description, o.Dates_Action,
	DATEDIFF( dd, o.Dates_Assigned, GETDATE() ) as TotalDuration,
	t.Complete
	from tbl_Offers o join tlkup_Offers_Action_Type t on o.Action_ID = t.Action_ID
	join tlkup_UserProfile u on o.CO_ID = u.CO_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number
	join tlkup_Offers_Proposal_Type p on o.Proposal_Type_ID = p.Proposal_Type_ID

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting into temp table 1 for Offer Statistics By Role Report'
		goto ERROREXIT
	END

	create table #RoleDescriptions
	(
	RoleDescription nvarchar(100) NULL
	)

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table 1B for Offer Statistics By Role Report'
		goto ERROREXIT
	END

	insert into #RoleDescriptions
	( RoleDescription )
	select distinct r.RoleDescription 
	from NACSEC.dbo.SEC_Roles r where r.RoleId in ( 15, 27, 28, 29, 30, 31, 32, 33, 34, 35 )

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting role descriptions into temp table'
		goto ERROREXIT
	END

	create table #ProposalTypeDescriptions
	( 
	Proposal_Type_Description nvarchar(100) NULL
	)

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table 1C for Offer Statistics By Role Report'
		goto ERROREXIT
	END

	insert into #ProposalTypeDescriptions
	( Proposal_Type_Description )
	select distinct Proposal_Type_Description 
	from tlkup_Offers_Proposal_Type

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting proposal type descriptions into temp table'
		goto ERROREXIT
	END

	create table #OfferCounts
	(
	Category nvarchar(100) NULL,
	OfferCount int NULL,
	Proposal_Type_Description nvarchar(100) NULL,
	RoleDescription nvarchar(100) NULL,
	ShortRoleDescription nvarchar(100) NULL
	)

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table 2 for Offer Statistics By Role Report'
		goto ERROREXIT
	END

	insert into #OfferCounts
	(
	Category,
	OfferCount,
	Proposal_Type_Description,
	RoleDescription
	)
	select '120 or less' as Category, COUNT(*) as OfferCount, Proposal_Type_Description, RoleDescription 
	from #OffersByGroup
	where TotalDuration between 0 and 120
	and Complete = 0
	group by Proposal_Type_Description, RoleDescription 

	union 

	select '120 or less' as Category, 0 as OfferCount, p.Proposal_Type_Description, r.RoleDescription 
	from #ProposalTypeDescriptions p, #RoleDescriptions r

	order by Proposal_Type_Description, RoleDescription 

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting 120 or less into temp table 2'
		goto ERROREXIT
	END

	insert into #OfferCounts
	(
	Category,
	OfferCount,
	Proposal_Type_Description,
	RoleDescription
	)
	select '121 to 180' as Category, COUNT(*) as OfferCount, Proposal_Type_Description, RoleDescription 
	from #OffersByGroup
	where TotalDuration between 121 and 180
	and Complete = 0
	group by Proposal_Type_Description, RoleDescription 
	
	union 

	select '121 to 180' as Category, 0 as OfferCount, p.Proposal_Type_Description, r.RoleDescription 
	from #ProposalTypeDescriptions p, #RoleDescriptions r

	order by Proposal_Type_Description, RoleDescription 

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting 121 to 180 into temp table 2'
		goto ERROREXIT
	END

	insert into #OfferCounts
	(
	Category,
	OfferCount,
	Proposal_Type_Description,
	RoleDescription
	)
	select '181 to 240' as Category, COUNT(*) as OfferCount, Proposal_Type_Description, RoleDescription 
	from #OffersByGroup
	where TotalDuration between 181 and 240
	and Complete = 0
	group by Proposal_Type_Description, RoleDescription 
	
	union 

	select '181 to 240' as Category, 0 as OfferCount, p.Proposal_Type_Description, r.RoleDescription 
	from #ProposalTypeDescriptions p, #RoleDescriptions r

	order by Proposal_Type_Description, RoleDescription 

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting 181 to 240 into temp table 2'
		goto ERROREXIT
	END

	insert into #OfferCounts
	(
	Category,
	OfferCount,
	Proposal_Type_Description,
	RoleDescription
	)
	select '241 to 365' as Category, COUNT(*) as OfferCount, Proposal_Type_Description, RoleDescription 
	from #OffersByGroup
	where TotalDuration between 241 and 365
	and Complete = 0
	group by Proposal_Type_Description, RoleDescription 

	union 

	select '241 to 365' as Category, 0 as OfferCount, p.Proposal_Type_Description, r.RoleDescription 
	from #ProposalTypeDescriptions p, #RoleDescriptions r

	order by Proposal_Type_Description, RoleDescription 

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting 241 to 365 into temp table 2'
		goto ERROREXIT
	END

	insert into #OfferCounts
	(
	Category,
	OfferCount,
	Proposal_Type_Description,
	RoleDescription
	)
	select '366 to 730' as Category, COUNT(*) as OfferCount, Proposal_Type_Description, RoleDescription 
	from #OffersByGroup
	where TotalDuration between 366 and 730
	and Complete = 0
	group by Proposal_Type_Description, RoleDescription 

	union 

	select '366 to 730' as Category, 0 as OfferCount, p.Proposal_Type_Description, r.RoleDescription 
	from #ProposalTypeDescriptions p, #RoleDescriptions r

	order by Proposal_Type_Description, RoleDescription 

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting 366 to 730 into temp table 2'
		goto ERROREXIT
	END

	insert into #OfferCounts
	(
	Category,
	OfferCount,
	Proposal_Type_Description,
	RoleDescription
	)
	select '731 or more' as Category, COUNT(*) as OfferCount, Proposal_Type_Description, RoleDescription 
	from #OffersByGroup
	where TotalDuration >= 731
	and Complete = 0
	group by Proposal_Type_Description, RoleDescription 

	union 

	select '731 or more' as Category, 0 as OfferCount, p.Proposal_Type_Description, r.RoleDescription 
	from #ProposalTypeDescriptions p, #RoleDescriptions r

	order by Proposal_Type_Description, RoleDescription 

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting 731 or more into temp table 2'
		goto ERROREXIT
	END

	select @today = CONVERT( DATE, CONVERT( NVARCHAR(4), YEAR(GETDATE())) + '/' + CONVERT( NVARCHAR(2), MONTH(GETDATE())) + '/' + CONVERT( NVARCHAR(2), DAY(GETDATE())))	

	select @sevenDaysAgo = CONVERT( DATE, CONVERT( NVARCHAR(4), YEAR(DATEADD(dd, -7, getdate() ))) + '/' + CONVERT( NVARCHAR(2), MONTH(DATEADD(dd, -7, getdate() ))) + '/' + CONVERT( NVARCHAR(2), DAY(DATEADD(dd, -7, getdate() ))))

	insert into #OfferCounts
	(
	Category,
	OfferCount,
	Proposal_Type_Description,
	RoleDescription
	)
	select 'Completed' as Category, COUNT(*) as OfferCount, Proposal_Type_Description, RoleDescription 
	from #OffersByGroup
	where Dates_Action >= @sevenDaysAgo
	and Dates_Action <  @today
	and Complete = 1
	group by Proposal_Type_Description, RoleDescription 

	union 

	select 'Completed' as Category, 0 as OfferCount, p.Proposal_Type_Description, r.RoleDescription 
	from #ProposalTypeDescriptions p, #RoleDescriptions r

	order by Proposal_Type_Description, RoleDescription 

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting completed offers into temp table 2'
		goto ERROREXIT
	END

	update #OfferCounts
	set ShortRoleDescription = 'Services'
	where RoleDescription like '%Lambda%'

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table 2 with role name 1'
		goto ERROREXIT
	END

	update #OfferCounts
	set ShortRoleDescription = 'PharmDental'
	where RoleDescription like '%Omega%'

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table 2 with role name 2'
		goto ERROREXIT
	END

	update #OfferCounts
	set ShortRoleDescription = 'MedSurg'
	where RoleDescription like '%Alpha%'

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table 2 with role name 3'
		goto ERROREXIT
	END
	
	update #OfferCounts
	set ShortRoleDescription = 'PMRS'
	where RoleDescription like '%Entry%'
	
	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating temp table 2 with role name 4'
		goto ERROREXIT
	END

	create table #OfferCounts2
	(
	Category nvarchar(100) NULL,
	OfferCount int NULL,
	Proposal_Type_Description nvarchar(100) NULL,
	ShortRoleDescription nvarchar(100) NULL
	)

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table 3 for Offer Statistics By Role Report'
		goto ERROREXIT
	END

	insert into #OfferCounts2
	( Proposal_Type_Description, ShortRoleDescription, Category, OfferCount )
	select Proposal_Type_Description, ShortRoleDescription, Category, SUM(OfferCount) as OfferCount
	from #OfferCounts
	group by Category, Proposal_Type_Description, ShortRoleDescription
	order by Proposal_Type_Description desc, ShortRoleDescription, Category

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting into temp table 3.'
		goto ERROREXIT
	END			

	/* regroup categories based on user report requirements */
	update #OfferCounts2
	set Category = '180 or less'
	where Proposal_Type_Description = 'Offer Proposal'
	and ( Category = '120 or less' or Category = '121 to 180' )

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error regrouping categories(1) into temp table 3.'
		goto ERROREXIT
	END		

	update #OfferCounts2
	set Category = '121 to 240'
	where Proposal_Type_Description = 'Contract Extension Proposal'
	and ( Category = '121 to 180' or Category = '181 to 240' )

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error regrouping categories(2) into temp table 3.'
		goto ERROREXIT
	END		

	select Proposal_Type_Description, ShortRoleDescription, Category, sum(OfferCount) as 'OfferCount'
	from #OfferCounts2
	group by Category, Proposal_Type_Description, ShortRoleDescription
	order by Proposal_Type_Description desc, ShortRoleDescription, Category

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error making final select from temp table 3.'
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


