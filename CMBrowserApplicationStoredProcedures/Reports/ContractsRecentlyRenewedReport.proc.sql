IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ContractsRecentlyRenewedReport' )
BEGIN
	DROP PROCEDURE ContractsRecentlyRenewedReport
END
GO

CREATE PROCEDURE ContractsRecentlyRenewedReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@StartYear int,
@StartMonth int, 
@EndYear int,
@EndMonth int,
@ScheduleNumber int,		/* may be -1 = all */
@DivisionId int,			/* may be -1 = all NAC */
@ContractingOfficerId int	 /* -1 = all */
)

AS

Declare 	@error int,
		@errorMsg nvarchar(1000),
		@startDate datetime,
		@endDate datetime,
		@joinSecurityServerName nvarchar(1000),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@query nvarchar(3000),
		@sqlParms nvarchar(1000)
	
BEGIN TRANSACTION


	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Contracts Recently Renewed Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.' 
		goto ERROREXIT
	END
	
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
	
	CREATE TABLE #ContractsRecentlyRenewedReport
	(
		ContractNumber nvarchar(50)  not null,
		Schedule_Name nvarchar(75) not null,
		Dates_CntrctAward datetime not null,
		Dates_Effective datetime not null,
		OldValue datetime null,
		NewValue datetime null,
		Contractor_Name nvarchar(75) null,
		LastModificationDate datetime null,
		BPA_FSS_Counterpart nvarchar(20) null,
		Business_Size nvarchar(10) null,
		SmallDisadvantagedBusiness nvarchar(50) null,
		EightABusiness  nvarchar(50) null,
		WomanOwnedBusiness  nvarchar(50) null,
		HubZoneBusiness  nvarchar(50) null,
		VeteranOwnedBusiness  nvarchar(50) null,
		CO_ID int null,
		FullName nvarchar(50) null,
		LastName nvarchar(20) null
	)

	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'

	select @query = 'insert into #ContractsRecentlyRenewedReport
		( ContractNumber, Schedule_Name,  Dates_CntrctAward, Dates_Effective, Contractor_Name, BPA_FSS_Counterpart, Business_Size,
		 SmallDisadvantagedBusiness, EightABusiness, WomanOwnedBusiness, HubZoneBusiness, VeteranOwnedBusiness, CO_ID, FullName, LastName  )
		select distinct a.ContractNumber, s.Schedule_Name, c.Dates_CntrctAward, c.Dates_Effective, c.Contractor_Name, c.BPA_FSS_Counterpart, b.Business_Size,
			case when ( c.Socio_SDB = 1 ) then ''Small Disadvantaged'' else '''' end as SmallDisadvantagedBusiness,
			case when ( c.Socio_8a = 1 ) then ''8a'' else '''' end as EightABusiness,
			case when ( c.Socio_Woman = 1 ) then ''Woman Owned'' else '''' end as WomanOwnedBusiness,
			case when ( c.Socio_HubZone = 1 ) then ''Hub Zone'' else '''' end as HubZoneBusiness,
			case when ( c.Socio_VetStatus_ID = 1 or c.Socio_VetStatus_ID = 3 ) then v.VetStatus_Description else '''' end as VeteranOwnedBusiness,
			c.CO_ID, p.FullName, p.LastName
		from [Audit_tbl_cntrcts] a join tbl_Cntrcts c on a.ContractNumber = c.CntrctNum
		join tlkup_Business_Size b on c.Socio_Business_Size_ID = b.Business_Size_ID
		join tlkup_VetStatus v on c.Socio_VetStatus_ID = v.VetStatus_ID
		join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		join ' + @joinSecurityServerName + '.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
		where a.ContractNumber in ( select distinct ContractNumber from [Audit_tbl_cntrcts]
									where LastModificationDate between @startDate_parm and @endDate_parm
									and FieldName = ''Dates_CntrctExp''
									and Convert( datetime, OldValue, 112) < CONVERT( datetime, NewValue, 112 )
									and YEAR( Convert( datetime, OldValue, 112)) < YEAR( CONVERT( datetime, NewValue, 112 )) ) '

	if @ScheduleNumber <> -1
	BEGIN
		select @whereSchedule = ' and s.Schedule_Number = ' + convert( nvarchar(10), @ScheduleNumber )
	END
	else
	BEGIN
		if @DivisionId <> -1
		BEGIN
			select @whereSchedule = ' and s.Division = ' + convert( nvarchar(10), @DivisionId )
		END
		else
		BEGIN
			select @whereSchedule = ' and s.Division <> 6 ' -- All NAC excludes SAC 
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
	
	select @query = @query + @whereSchedule + @whereContractingOfficer 
	
	select @sqlParms = '@startDate_parm datetime, @endDate_parm datetime'
	
	exec SP_EXECUTESQL @query, @sqlParms, @startDate_parm = @startDate, @endDate_parm = @endDate


	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error executing query for report.'
		goto ERROREXIT
	END

	-- backfill with the old date from the audit record
	select @query = 'update #ContractsRecentlyRenewedReport
		set OldValue = a.OldValue	
		from  [Audit_tbl_cntrcts] a join  #ContractsRecentlyRenewedReport t on a.ContractNumber = t.ContractNumber
		where a.LastModificationDate between @startDate_parm and @endDate_parm
		and a.FieldName = ''Dates_CntrctExp''
		and a.AuditId = ( select MIN(c.AuditId) from Audit_tbl_cntrcts c
							where c.ContractNumber = t.ContractNumber
							and c.FieldName = ''Dates_CntrctExp''
							and c.LastModificationDate between @startDate_parm and @endDate_parm ) '
	
	select @sqlParms = '@startDate_parm datetime, @endDate_parm datetime'
	
	exec SP_EXECUTESQL @query, @sqlParms, @startDate_parm = @startDate, @endDate_parm = @endDate

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error executing query 2 for report.'
		goto ERROREXIT
	END

	-- backfill with the new date from the audit record
	select @query = 'update #ContractsRecentlyRenewedReport
		set NewValue = a.NewValue,
			LastModificationDate = a.LastModificationDate	
		from  [Audit_tbl_cntrcts] a join  #ContractsRecentlyRenewedReport t on a.ContractNumber = t.ContractNumber
		where a.LastModificationDate between @startDate_parm and @endDate_parm
		and a.FieldName = ''Dates_CntrctExp''
		and a.AuditId = ( select MAX(c.AuditId) from Audit_tbl_cntrcts c
							where c.ContractNumber = t.ContractNumber
							and c.FieldName = ''Dates_CntrctExp''
							and c.LastModificationDate between @startDate_parm and @endDate_parm ) '
	
	select @sqlParms = '@startDate_parm datetime, @endDate_parm datetime'
	
	exec SP_EXECUTESQL @query, @sqlParms, @startDate_parm = @startDate, @endDate_parm = @endDate

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error executing query 3 for report.'
		goto ERROREXIT
	END


	-- backfill BPA's with socio values from their parent FSS contracts
	update #ContractsRecentlyRenewedReport
	set Business_Size = b.Business_Size,
		SmallDisadvantagedBusiness = case when ( c.Socio_SDB = 1 ) then 'Small Disadvantaged' else '' end,
		EightABusiness = case when ( c.Socio_8a = 1 ) then '8a' else '' end,
		WomanOwnedBusiness = case when ( c.Socio_Woman = 1 ) then 'Woman Owned' else '' end,
		HubZoneBusiness = case when ( c.Socio_HubZone = 1 ) then 'Hub Zone' else '' end,
		VeteranOwnedBusiness = case when ( c.Socio_VetStatus_ID = 1 or c.Socio_VetStatus_ID = 3 ) then v.VetStatus_Description else '' end

	from tbl_Cntrcts c join #ContractsRecentlyRenewedReport r on c.CntrctNum = r.BPA_FSS_Counterpart
	join tlkup_Business_Size b on c.Socio_Business_Size_ID = b.Business_Size_ID
	join tlkup_VetStatus v on c.Socio_VetStatus_ID = v.VetStatus_ID
	where r.BPA_FSS_Counterpart is not null
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting parent socio info for BPAs for contracts recently renewed report.'
		goto ERROREXIT
	END
	
	select ContractNumber,
		Schedule_Name,
		Dates_CntrctAward,
		Dates_Effective,
		OldValue,
		NewValue,
		Contractor_Name,
		LastModificationDate,
		BPA_FSS_Counterpart,
		Business_Size,
		SmallDisadvantagedBusiness,
		EightABusiness,
		WomanOwnedBusiness,
		HubZoneBusiness,
		VeteranOwnedBusiness,
		CO_ID,
		FullName,
		LastName
	from  #ContractsRecentlyRenewedReport
	where Convert( datetime, OldValue, 112) < CONVERT( datetime, NewValue, 112 )
	order by ContractNumber

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for report.'
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


