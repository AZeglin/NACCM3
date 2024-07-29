IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ContractAwardsReport')
	BEGIN
		DROP  Procedure  ContractAwardsReport
	END

GO

CREATE Procedure ContractAwardsReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@AwardYear int,				/* may be -1 all */
@AwardQuarter int,			/* may be -1 all */
@ScheduleNumber int,		/* may be -1 = all */
@DivisionId int,			/* may be -1 = all NAC */
@ContractingOfficerId int	 /* -1 = all */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3000),
		@sqlParms nvarchar(1000),
		@startDate datetime,
		@endDate datetime,
		@joinSecurityServerName nvarchar(1000),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@whereAwardDate nvarchar(100),
		@orderBy nvarchar(100),
		@SERVERNAME nvarchar(255),
		@MinAwardYear int
		
BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Contract Awards Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	
	CREATE TABLE #ContractAwardsReport
	(
		CntrctNum nvarchar(50)  not null,
		Contractor_Name nvarchar(75) not null,
		Dates_CntrctAward datetime not null,
		Dates_Effective datetime not null,
		Dates_CntrctExp datetime not null,
		Schedule_Number int not null,
		Schedule_Name nvarchar(75) not null,
		Estimated_Contract_Value money  null,
		Socio_Business_Size_ID int not null,
		Socio_SDB bit not null,
		Socio_8a bit not null,
		Socio_Woman bit not null,
		Socio_HubZone bit not null,
		Socio_VetStatus_ID int not null,
		CO_ID int not null,
		FullName nvarchar(50) not null,
		BPA_FSS_Counterpart nvarchar(20) null,

		Business_Size nvarchar(10) null,
		SmallDisadvantagedBusiness nvarchar(50) null,
		EightABusiness  nvarchar(50) null,
		WomanOwnedBusiness  nvarchar(50) null,
		HubZoneBusiness  nvarchar(50) null,
		VeteranOwnedBusiness  nvarchar(50) null,
		CreationDate datetime not null,
		LastModificationDate datetime not null
	)
		
	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'
	
	select @query = 'insert into #ContractAwardsReport
	(
		CntrctNum,
		Contractor_Name,
		Dates_CntrctAward,
		Dates_Effective,
		Dates_CntrctExp,
		Schedule_Number,
		Schedule_Name,
		Estimated_Contract_Value,
		Socio_Business_Size_ID,
		Socio_SDB,
		Socio_8a,
		Socio_Woman,
		Socio_HubZone,
		Socio_VetStatus_ID,
		CO_ID,
		FullName,
		BPA_FSS_Counterpart,

		Business_Size,
		SmallDisadvantagedBusiness,
		EightABusiness,
		WomanOwnedBusiness,
		HubZoneBusiness,
		VeteranOwnedBusiness,
		CreationDate,
		LastModificationDate
	)
	select c.CntrctNum, c.Contractor_Name, c.Dates_CntrctAward, c.Dates_Effective, c.Dates_CntrctExp, c.Schedule_Number, s.Schedule_Name, c.Estimated_Contract_Value, c.Socio_Business_Size_ID, 
	c.Socio_SDB, c.Socio_8a, c.Socio_Woman, c.Socio_HubZone, c.Socio_VetStatus_ID, c.CO_ID, p.FullName, c.BPA_FSS_Counterpart,
	b.Business_Size,
	case when ( c.Socio_SDB = 1 ) then ''Small Disadvantaged'' else '''' end as SmallDisadvantagedBusiness,
	case when ( c.Socio_8a = 1 ) then ''8a'' else '''' end as EightABusiness,
	case when ( c.Socio_Woman = 1 ) then ''Woman Owned'' else '''' end as WomanOwnedBusiness,
	case when ( c.Socio_HubZone = 1 ) then ''Hub Zone'' else '''' end as HubZoneBusiness,
	case when ( c.Socio_VetStatus_ID = 1 or c.Socio_VetStatus_ID = 3 ) then v.VetStatus_Description else '''' end as VeteranOwnedBusiness,
	c.CreationDate,
	c.LastModificationDate

	from tbl_Cntrcts c join ' + @joinSecurityServerName + '.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
	join tlkup_Business_Size b on c.Socio_Business_Size_ID = b.Business_Size_ID
	join tlkup_VetStatus v on c.Socio_VetStatus_ID = v.VetStatus_ID
	join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number 
	where c.Dates_CntrctAward between @startDate_parm and @endDate_parm '
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END

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
	
	if @AwardYear = -1
	BEGIN
		-- 6/19/1990 is the earliest contract in the NACCM observed as of 10/2011
		select @startDate = convert( datetime, '6/1/1990' )
		select @endDate = convert( datetime,  '12/31/' + convert( nvarchar(4), YEAR( getdate() )))		
	END
	else
	BEGIN		
		if @AwardQuarter = -1
		BEGIN
			select @startDate = convert( datetime, '10/1/' + convert( nvarchar(4), @AwardYear - 1 ))
			select @endDate = convert( datetime,  '9/30/' + convert( nvarchar(4), @AwardYear ))
		END

		if @AwardQuarter = 1
		BEGIN
			select @startDate = convert( datetime, '10/1/' + convert( nvarchar(4), @AwardYear - 1 ))
			select @endDate = convert( datetime,  '12/31/' + convert( nvarchar(4), @AwardYear - 1 ))	
		END
		
		if @AwardQuarter = 2
		BEGIN
			select @startDate = convert( datetime, '1/1/' + convert( nvarchar(4), @AwardYear ))
			select @endDate = convert( datetime,  '3/31/' + convert( nvarchar(4), @AwardYear ))	
		END

		if @AwardQuarter = 3
		BEGIN
			select @startDate = convert( datetime, '4/1/' + convert( nvarchar(4), @AwardYear ))
			select @endDate = convert( datetime,  '6/30/' + convert( nvarchar(4), @AwardYear ))		
		END
				
		if @AwardQuarter = 4
		BEGIN
			select @startDate = convert( datetime, '7/1/' + convert( nvarchar(4), @AwardYear ))
			select @endDate = convert( datetime,  '9/30/' + convert( nvarchar(4), @AwardYear ))		
		END
	END
	
	select @orderBy = ' order by c.Dates_CntrctAward '
	
	select @query = @query + @whereSchedule + @whereContractingOfficer + @orderBy 
	
	select @sqlParms = '@startDate_parm datetime, @endDate_parm datetime'
	
	exec SP_EXECUTESQL @query, @sqlParms, @startDate_parm = @startDate, @endDate_parm = @endDate

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for contract awards report.'
		goto ERROREXIT
	END
	
	-- backfill BPA's with socio values from their parent FSS contracts
	update #ContractAwardsReport
	set Socio_Business_Size_ID = c.Socio_Business_Size_ID,
		Socio_SDB = c.Socio_SDB,
		Socio_8a = c.Socio_8a,
		Socio_Woman = c.Socio_Woman,
		Socio_HubZone = c.Socio_HubZone,
		Socio_VetStatus_ID = c.Socio_VetStatus_ID,

		Business_Size = b.Business_Size,
		SmallDisadvantagedBusiness = case when ( c.Socio_SDB = 1 ) then 'Small Disadvantaged' else '' end,
		EightABusiness = case when ( c.Socio_8a = 1 ) then '8a' else '' end,
		WomanOwnedBusiness = case when ( c.Socio_Woman = 1 ) then 'Woman Owned' else '' end,
		HubZoneBusiness = case when ( c.Socio_HubZone = 1 ) then 'Hub Zone' else '' end,
		VeteranOwnedBusiness = case when ( c.Socio_VetStatus_ID = 1 or c.Socio_VetStatus_ID = 3 ) then v.VetStatus_Description else '' end

	from tbl_Cntrcts c join #ContractAwardsReport r on c.CntrctNum = r.BPA_FSS_Counterpart
	join tlkup_Business_Size b on c.Socio_Business_Size_ID = b.Business_Size_ID
	join tlkup_VetStatus v on c.Socio_VetStatus_ID = v.VetStatus_ID
	where r.BPA_FSS_Counterpart is not null
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting parent socio info for BPAs for contract awards report.'
		goto ERROREXIT
	END
		
	select CntrctNum,
		Contractor_Name,
		Dates_CntrctAward,
		Dates_Effective,
		Dates_CntrctExp,
		Schedule_Number,
		Schedule_Name,
		Estimated_Contract_Value,
		Socio_Business_Size_ID,
		Socio_SDB,
		Socio_8a,
		Socio_Woman,
		Socio_HubZone,
		Socio_VetStatus_ID,
		CO_ID,
		FullName,
		BPA_FSS_Counterpart,
		Business_Size,
		SmallDisadvantagedBusiness,
		EightABusiness,
		WomanOwnedBusiness,
		HubZoneBusiness,
		VeteranOwnedBusiness,
		CreationDate,
		LastModificationDate
	from #ContractAwardsReport	
	order by Dates_CntrctAward	

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error on final select for contract awards report.'
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
