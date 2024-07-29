IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'RecentSBAActivityReport' )
BEGIN
	DROP PROCEDURE RecentSBAActivityReport
END
GO

CREATE PROCEDURE RecentSBAActivityReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@DaysToLookBack int,
@Division int
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200)

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Recent SBA Activity Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #SBAPlansWithRecentActivity
	(
		SBAPlanID int not null,
		ResponsibleContract nvarchar(50) null,
		ResponsibleCOId int
	)

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table for Recent SBA Activity Report.'
		goto ERROREXIT
	END

	insert into #SBAPlansWithRecentActivity
	( SBAPlanID )
	select distinct p.SBAPlanID
	from tbl_sba_SBAPlan p join tbl_sba_Projection j on p.SBAPlanID = j.SBAPlanID
	where ( p.SBAPlanID in ( select distinct SBAPlanID from tbl_sba_Projection z
							where z.LastModificationDate between DATEADD( dd, ( -1 * @DaysToLookBack ), GETDATE() ) and GETDATE() )
			or  p.SBAPlanID in ( select distinct SBAPlanID from tbl_sba_SBAPlan g
							where g.LastModificationDate between DATEADD( dd, ( -1 * @DaysToLookBack ), GETDATE() ) and GETDATE() ))				
	and j.EndDate = ( select MAX(x.EndDate) from tbl_sba_Projection x where x.SBAPlanID = p.SBAPlanID )

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting into temp table for Recent SBA Activity Report.'
		goto ERROREXIT
	END

	update #SBAPlansWithRecentActivity
	set ResponsibleContract = dbo.GetContractResponsibleForSBAPlanFunction( SBAPlanID, getdate())	

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating responsible contract for Recent SBA Activity Report.'
		goto ERROREXIT
	END

	update #SBAPlansWithRecentActivity
	set ResponsibleCOId = ( select CO_ID from tbl_Cntrcts c where c.CntrctNum = t.ResponsibleContract )
	from #SBAPlansWithRecentActivity t

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating responsible CO for Recent SBA Activity Report.'
		goto ERROREXIT
	END

	select p.PlanName, p.SBAPlanID, j.StartDate, j.EndDate, t.ResponsibleContract, t.ResponsibleCOId, u.FullName as ResponsibleCOName, u.LastName, p.Plan_Admin_Name, p.Plan_Admin_Address1, p.Plan_Admin_City, 
		p.Plan_Admin_State, p.Plan_Admin_Zip, p.Plan_Admin_Phone, p.Plan_Admin_Fax, p.Plan_Admin_email,
		j.CreationDate as ProjectionCreationDate, j.CreatedBy as ProjectionCreatedBy, 
		j.LastModificationDate as ProjectionLastModificationDate, j.LastModifiedBy as ProjectionLastModifiedBy,
		p.LastModificationDate as PlanLastModificationDate, p.LastModifiedBy as PlanLastModifiedBy

	from tbl_sba_SBAPlan p join tbl_sba_Projection j on p.SBAPlanID = j.SBAPlanID
	join #SBAPlansWithRecentActivity t on p.SBAPlanID = t.SBAPlanID
	join tbl_Cntrcts c on t.ResponsibleContract = c.CntrctNum
	join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
	join NACSEC.dbo.SEC_UserProfile u on t.ResponsibleCOId = u.CO_ID
	where s.Division = @Division
	and j.EndDate = ( select MAX(x.EndDate) from tbl_sba_Projection x where x.SBAPlanID = t.SBAPlanID )
	order by j.LastModificationDate desc

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting Recent SBA Activity Report.'
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


