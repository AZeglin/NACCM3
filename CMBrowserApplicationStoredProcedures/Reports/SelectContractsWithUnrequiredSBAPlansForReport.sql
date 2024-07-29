IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractsWithUnrequiredSBAPlansForReport')
	BEGIN
		DROP  Procedure  SelectContractsWithUnrequiredSBAPlansForReport
	END

GO

CREATE Procedure SelectContractsWithUnrequiredSBAPlansForReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@DivisionId int /* -1 = All NAC */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@SERVERNAME nvarchar(255),
		@joinSecurityServerName nvarchar(1000),
		@query nvarchar(4000),
		@SQLParms nvarchar(1000),
		@OrderBy nvarchar(100),
		@WhereDivision nvarchar(140)


BEGIN TRANSACTION


	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Contracts With Unrequired SBA Plans Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #ContractsWithUnrequiredSBAPlans
	(
		CntrctNum					nvarchar(50),
		Contractor_Name				nvarchar(75),
		DUNS						nvarchar(9), 
		Dates_CntrctAward			DateTime, 
		Dates_Effective				DateTime, 
		Dates_CntrctExp				DateTime, 
		Dates_Completion			DateTime, 
		Schedule_Number				int, 
		Schedule_Name				nvarchar(75),  
		CO_ID						int, 
		FullName					nvarchar(80), 
		POC_Primary_Name			nvarchar(30), 
		POC_Primary_Phone			nvarchar(15), 
		POC_Primary_Email			nvarchar(50), 
		Estimated_Contract_Value	money, 
		SBA_Plan_Exempt				bit,
		Socio_Business_Size_ID		int,
		Threshold					int,
		PlanId						int,
		Division_Description		nvarchar(40),
		LastProjectionEndDate		nvarchar(20)  NULL
	)

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table'
		goto ERROREXIT
	END

	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning server name'
		goto ERROREXIT
	END

	-- SBAPlanId 511 is "No Plan"
	select @query = 'insert into #ContractsWithUnrequiredSBAPlans
			( CntrctNum, Contractor_Name, DUNS, Dates_CntrctAward, Dates_Effective, Dates_CntrctExp, Dates_Completion, Schedule_Number, Schedule_Name,  CO_ID, FullName, 
			POC_Primary_Name, POC_Primary_Phone, POC_Primary_Email, Estimated_Contract_Value, SBA_Plan_Exempt, Socio_Business_Size_ID, Threshold, PlanId, Division_Description )
		select c.CntrctNum, c.Contractor_Name, c.DUNS, c.Dates_CntrctAward, c.Dates_Effective, c.Dates_CntrctExp, c.Dates_Completion, c.Schedule_Number, s.Schedule_Name,  u.CO_ID, u.FullName, 
			c.POC_Primary_Name, c.POC_Primary_Phone, c.POC_Primary_Email, c.Estimated_Contract_Value, c.SBA_Plan_Exempt, c.Socio_Business_Size_ID,
			dbo.GetSBAContractValueThresholdFunction( c.Dates_CntrctAward ) as Threshold, c.SBAPlanID, s.Division_Description
		from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
		and c.SBAPlanId is not null
		and exists (  select p.SBAPlanId from tbl_sba_SBAPlan p where p.SBAPlanId = c.SBAPlanId )
		and c.SBAPlanId <> 511
		and ( c.Schedule_Number in ( 14, 15, 39, 41, 48, 52 )
				or c.Socio_Business_Size_ID <> 2 
				or c.Estimated_Contract_Value < dbo.GetSBAContractValueThresholdFunction( c.Dates_CntrctAward )
				or c.SBA_Plan_Exempt = 1 ) '
								
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string'
		goto ERROREXIT
	END

	if @DivisionId <> -1
	BEGIN
		select @WhereDivision = ' and s.Division = ' + convert( nvarchar(10), @DivisionId )
	END
	else
	BEGIN
		select @WhereDivision = ' and s.Division <> 6 ' -- All NAC excludes SAC 
	END

	select @query = @query + @WhereDivision 

	exec SP_EXECUTESQL @query

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting Contracts With Unrequired SBA Plans Report.'
		goto ERROREXIT
	END
	
	update x 
	set x.LastProjectionEndDate = ( select max( EndDate ) from tbl_sba_Projection p join #ContractsWithUnrequiredSBAPlans t on p.SBAPlanID = t.PlanId 
									where t.PlanId = x.PlanId and t.CntrctNum = x.CntrctNum )
	from #ContractsWithUnrequiredSBAPlans x
	
	select CntrctNum, 
		Contractor_Name, 
		DUNS, 
		Dates_CntrctAward, 
		Dates_Effective, 
		Dates_CntrctExp, 
		Dates_Completion, 
		Schedule_Number, 
		Schedule_Name,  
		CO_ID, 
		FullName, 
		POC_Primary_Name, 
		POC_Primary_Phone, 
		POC_Primary_Email, 
		Estimated_Contract_Value, 
		SBA_Plan_Exempt,
		case when ( Socio_Business_Size_ID = 1 ) then 'Small' else 'Large' end as BusinessSize,
		Threshold, 
		PlanId, 
		Division_Description,
		isnull( LastProjectionEndDate, 'none' ) as LastProjectionEndDate
	from #ContractsWithUnrequiredSBAPlans
	order by CntrctNum

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting Contracts With Unrequired SBA Plans Report from temp table.'
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



