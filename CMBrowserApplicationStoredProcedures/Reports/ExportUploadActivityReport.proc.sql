IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ExportUploadActivityReport' )
BEGIN
	DROP PROCEDURE ExportUploadActivityReport
END
GO

CREATE PROCEDURE ExportUploadActivityReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractingOfficerId int, /* -1 = all */
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int, /* may be -1 = all NAC */
@StartDate datetime,
@EndDate datetime
)

AS

DECLARE @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query1 nvarchar(2000),
		@query2 nvarchar(2000),
		@query nvarchar(3000),
		@joinSecurityServerName nvarchar(1000),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@orderByClause nvarchar(200),
		@sqlParms nvarchar(1000)
	
BEGIN TRANSACTION

	/* log the request for the report */
	exec [NAC_CM].dbo.InsertUserActivity @ReportUserLoginId, 'R', 'Export Upload Activity Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating security where clause for rebate request.'
		goto ERROREXIT
	END

	if @StartDate is null or @EndDate is null
	BEGIN
		select @StartDate = DATEADD( dd, -30, getdate() )
		select @EndDate = getdate()
	END


	select @query1 = '
	select a.ContractNumber, o.FullName as COName, o.LastName as COLastName, a.CreationDate as ExportUploadDate, x.ActivityDataTypeDescription, t.ActivityTypeDescription, s.StatusDescription,
	isnull( d.UpdateCount, '''' ) as UpdateCount, isnull( d.AdditionCount, '''' ) as AdditionCount, isnull( d.RemovalCount, '''' ) as RemovalCount, 
	isnull( d.UpdateErrorCount + d.AdditionErrorCount + d.RemovalErrorCount, '''' ) as ErrorCount,
	u.FullName as ExportedUploadedBy, u.LastName  as ExportedUploadedByLastName, h.Schedule_Name, h.Division

	from EU_Activity a join ' + @joinSecurityServerName + '.dbo.SEC_UserProfile u on a.UserId = u.UserId
	join [NAC_CM].dbo.tbl_Cntrcts c on a.ContractNumber = c.CntrctNum
	join ' + @joinSecurityServerName + '.dbo.SEC_UserProfile o on c.CO_ID = o.CO_ID
	join [NAC_CM].dbo.[tlkup_Sched/Cat] h on c.Schedule_Number = h.Schedule_Number
	join EU_ActivityTypes t on a.ActivityType = t.ActivityType
	join EU_ActivityDataTypes x on a.ActivityDataType = x.ActivityDataType
	left outer join EU_MedSurgActivityDetails d on a.ActivityId = d.ActivityId
	join EU_Statuses s on a.ExportUploadStatus = s.ExportUploadStatus
	where a.ActivityDataType = ''M'' 
	and CONVERT( datetime, convert( nvarchar(20), a.CreationDate, 101 )) between @StartDate_parm and @EndDate_parm '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END

	select @query2 = '

	union

	select a.ContractNumber, o.FullName as COName, o.LastName as COLastName, a.CreationDate as ExportUploadDate, x.ActivityDataTypeDescription, t.ActivityTypeDescription, s.StatusDescription,
	isnull( d.ChangedFSSPricesCount + d.ChangedBIG4PricesCount + d.ChangedRestrictedPricesCount, '''' ) as  UpdateCount,
	0 as AdditionCount,
	isnull( d.RemovedFSSPricesCount + d.RemovedBIG4PricesCount + d.RemovedRestrictedPricesCount, '''' ) as RemovalCount,
	isnull( d.FSSErrorCount + d.BIG4ErrorCount + d.RestrictedErrorCount, '''' ) as ErrorCount,
	u.FullName as ExportedUploadedBy, u.LastName as ExportedUploadedByLastName, h.Schedule_Name, h.Division

	from EU_Activity a join ' + @joinSecurityServerName + '.dbo.SEC_UserProfile u on a.UserId = u.UserId
	join [NAC_CM].dbo.tbl_Cntrcts c on a.ContractNumber = c.CntrctNum
	join ' + @joinSecurityServerName + '.dbo.SEC_UserProfile o on c.CO_ID = o.CO_ID
	join [NAC_CM].dbo.[tlkup_Sched/Cat] h on c.Schedule_Number = h.Schedule_Number
	join EU_ActivityTypes t on a.ActivityType = t.ActivityType
	join EU_ActivityDataTypes x on a.ActivityDataType = x.ActivityDataType
	left outer join EU_PharmaceuticalActivityDetails d on a.ActivityId = d.ActivityId
	join EU_Statuses s on a.ExportUploadStatus = s.ExportUploadStatus
	where a.ActivityDataType = ''P'' 
	and CONVERT( datetime, convert( nvarchar(20), a.CreationDate, 101 )) between @StartDate_parm and @EndDate_parm '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2'
		goto ERROREXIT
	END

	select @orderByClause = ' order by ExportUploadDate desc '
		
	if @ScheduleNumber <> -1
	BEGIN
		select @whereSchedule = ' and h.Schedule_Number = ' + convert( nvarchar(10), @ScheduleNumber )
	END
	else
	BEGIN
		if @DivisionId <> -1
		BEGIN
			select @whereSchedule = ' and h.Division = ' + convert( nvarchar(10), @DivisionId )
		END
		else
		BEGIN
			select @whereSchedule = ' and h.Division <> 6 ' -- All NAC excludes SAC  
		END
	END

	if @ContractingOfficerId <> -1
	BEGIN
		select @whereContractingOfficer = ' and c.CO_ID = ' + convert( nvarchar(10), @ContractingOfficerId )
	END
	else
	BEGIN
		select @whereContractingOfficer = ' '
	END
	
	select @sqlParms = '@StartDate_parm datetime, @EndDate_parm datetime'

	select @query = @query1  + @whereSchedule + @whereContractingOfficer + @query2 + @whereSchedule + @whereContractingOfficer + @orderByClause
	
	exec SP_EXECUTESQL @query, @sqlParms, @StartDate_parm = @StartDate, @EndDate_parm = @EndDate

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error'
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


