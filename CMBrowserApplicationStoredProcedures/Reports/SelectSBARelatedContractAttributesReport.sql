IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSBARelatedContractAttributesReport')
	BEGIN
		DROP  Procedure  SelectSBARelatedContractAttributesReport
	END

GO

CREATE Procedure [dbo].[SelectSBARelatedContractAttributesReport]
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@DivisionId int, /* -1 = All */
@ProjectionAvailability int  /* 0 = no projections, 1 = outdated projections, 2  = current projection */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@SERVERNAME nvarchar(255),
		@joinSecurityServerName nvarchar(1000),
		@query nvarchar(4000),
		@SQLParms nvarchar(1000),
		@WhereDivision nvarchar(140),
		@WhereMaxProjection nvarchar(500),
		@WhereMaxProjectionID nvarchar(500)


BEGIN TRANSACTION


	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'SBA Plans By Contract Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END


	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'

	IF OBJECT_ID('tempdb..#SBABYCONTRACTREPORT') IS NOT NULL 
	BEGIN
		drop table #SBABYCONTRACTREPORT
	
		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error dropping #SBABYCONTRACTREPORT temp table.'
			goto ERROREXIT
		END
	END

	create table #SBABYCONTRACTREPORT
	(
		CntrctNum nvarchar(50) not null,
		Dates_Effective datetime not null,
		Dates_CntrctExp datetime not null,
		CO_ID int not null,
		FullName nvarchar(80) null,
		Contractor_Name nvarchar(75) not null,
		Schedule_Name nvarchar(75) null,
		Estimated_Contract_Value money null,
		DUNS nvarchar(9) null,
		SBA_Plan_Exempt bit not null,
		BusinessSize nvarchar(5) not null,
		SBAPlanID int null,
		PlanName nvarchar(50) null,
		PlanTypeID int null,
		PlanTypeDescription nvarchar(50) null,
		ContractResponsible nvarchar(50) null,
		ContractResponsibleCOID int null,
		ContractResponsibleDivision nvarchar(50) null,
		ContractResponsibleCO nvarchar(80) null,
		Plan_Admin_Name nvarchar(50) null,
		Plan_Admin_state nvarchar(2) null,
		Plan_Admin_email nvarchar(50) null,
		Plan_Admin_Phone nvarchar(30) null,
		MaxProjectionEndDate datetime null,
		ProjectionID int null,
		SBAPlanID1 int null,
		SBDollars money null,
		SB money null,
		SDBDollars money null,
		SDB money null,
		WomenOwnedDollars money null,
		WO money null,
		DisabledVetDollars money null,
		DisabledVO money null,
		HubZoneDollars money null,
		HubZone money null,
		HBCUDollars money null,
		HBCU money null,
		VeteranOwnedDollars money null,
		VO money null,
		TotalSubConDollars money null,
		Comments nvarchar(255) null,
		ProjectionStartDate smalldatetime null,
		ProjectionEndDate smalldatetime null
	)

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table #SBABYCONTRACTREPORT for report.'
		goto ERROREXIT
	END

	select @query = 'insert into #SBABYCONTRACTREPORT
		(
			CntrctNum,
			Dates_Effective,
			Dates_CntrctExp,
			CO_ID,
			FullName,
			Contractor_Name,
			Schedule_Name,
			Estimated_Contract_Value,
			DUNS,
			SBA_Plan_Exempt,
			BusinessSize,
			SBAPlanID,
			PlanName,
			PlanTypeID,
			PlanTypeDescription,
			ContractResponsible,
			ContractResponsibleCOID,
			ContractResponsibleDivision,
			ContractResponsibleCO,
			Plan_Admin_Name,
			Plan_Admin_State,
			Plan_Admin_email,
			Plan_Admin_Phone,
			MaxProjectionEndDate,
			ProjectionID,
			SBAPlanID1,
			SBDollars,
			SB,
			SDBDollars,
			SDB,
			WomenOwnedDollars,
			WO,
			DisabledVetDollars,
			DisabledVO,
			HubZoneDollars,
			Hubzone,
			HBCUDollars,
			HBCU,
			VeteranOwnedDollars,
			VO,
			TotalSubConDollars,
			Comments,
			ProjectionStartDate,
			ProjectionEndDate
		)
		select c.CntrctNum, c.Dates_Effective, c.Dates_CntrctExp, 
		u.CO_ID, u.FullName, 
		c.Contractor_Name, s.Schedule_Name, c.Estimated_Contract_Value, c.DUNS, c.SBA_Plan_Exempt, 
		case when ( c.Socio_Business_Size_ID = 1 ) then ''Small'' else ''Large'' end as BusinessSize,
		c.SBAPlanID, b.PlanName, b.PlanTypeID, d.PlanTypeDescription,
		dbo.GetContractResponsibleForSBAPlanFunction( c.SBAPlanID, getdate() ) as ContractResponsible,
		x.CO_ID as ContractResponsibleCOID, z.Division_Description as DivisionResponsible,
		y.FullName as ContractResponsibleCO,
		b.Plan_Admin_Name, b.Plan_Admin_State, b.Plan_Admin_email, b.Plan_Admin_Phone, 
		( select max(p.EndDate) from tbl_sba_Projection p where p.SBAPlanID = b.SBAPlanID ) as MaxProjectionEndDate,
		p.projectionID, p.SBAPlanID, p.SBDollars, (p.SBDollars*100/p.TotalSubConDollars) as ''SB%'', p.SDBDollars, (p.SDBDollars*100/p.TotalSubConDollars) as ''SDB%'', p.WomenOwnedDollars, (p.WomenOwnedDollars*100/p.TotalSubConDollars) as ''WO%'', 
		p.DisabledVetDollars, (p.DisabledVetDollars*100/p.TotalSubConDollars) as ''DisabledVO%'', p.HubZoneDollars, (p.HubZoneDollars*100/p.TotalSubConDollars) as ''HubZone%'', p.HBCUDollars, (p.HBCUDollars*100/p.TotalSubConDollars) as ''HBCU%'', 
		p.VeteranOwnedDOllars, (p.VeteranOwnedDollars*100/p.TotalSubConDollars) as ''VO%'', p.TotalSubConDollars,
		p.Comments, ( select max(p.StartDate) from tbl_sba_Projection p where p.SBAPlanID = b.SBAPlanID ) as StartDate, ( select max(p.EndDate) from tbl_sba_Projection p where p.SBAPlanID = b.SBAPlanID ) as EndDate 
		from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		left outer join tbl_sba_SBAPlan b on c.SBAPlanID = b.SBAPlanID
		left outer join tbl_sba_Projection p on b.SBAPlanID = p.SBAPlanID
		left outer join tbl_sba_PlanType d on b.PlanTypeID = d.PlanTypeID
		left outer join tbl_Cntrcts x on x.CntrctNum = dbo.GetContractResponsibleForSBAPlanFunction( c.SBAPlanID, getdate() )
		left outer join [tlkup_Sched/Cat] z on z.Schedule_Number = x.Schedule_Number
		left outer join  ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] y on x.CO_ID = y.CO_ID
		
		where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
		and c.Schedule_Number not in ( 14, 15, 39, 41, 48, 52 )
		and c.Socio_Business_Size_ID = 2 
		and c.Estimated_Contract_Value >= dbo.GetSBAContractValueThresholdFunction( c.Dates_CntrctAward )
		and c.SBA_Plan_Exempt = 0'
		
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
		select @WhereDivision = ' '
	END

	/* @ProjectionAvailability int  0 = no projections, 1 = outdated projections, 2  = current projection */

	If @ProjectionAvailability = 0
	BEGIN
		select @WhereMaxProjectionID = ' '--and p.ProjectionID = (select max(p1.ProjectionID) from tbl_sba_Projection p1 where p1.SBAPlanID = b.SBAPlanID and p1.EndDate IS NULL) '
		select @WhereMaxProjection = ' and ( select max(p.EndDate) from tbl_sba_Projection p where p.SBAPlanID = b.SBAPlanID ) is NULL '
	END 
	else if @ProjectionAvailability = 1
	BEGIN
		select @WhereMaxProjectionID = ' and p.ProjectionID = (select max(p1.ProjectionID) from tbl_sba_Projection p1 where p1.SBAPlanID = b.SBAPlanID and p1.EndDate < GETDATE()) '
		select @WhereMaxProjection = ' and ( select max(p.EndDate) from tbl_sba_Projection p where p.SBAPlanID = b.SBAPlanID ) < GETDATE() '
	END
	else if @ProjectionAvailability = 2
	BEGIN
		select @WhereMaxProjectionID = ' and p.ProjectionID = (select max(p1.ProjectionID) from tbl_sba_Projection p1 where p1.SBAPlanID = b.SBAPlanID and p1.EndDate >= GETDATE()) '
		select @WhereMaxProjection = ' and ( select max(p.EndDate) from tbl_sba_Projection p where p.SBAPlanID = b.SBAPlanID ) >= GETDATE() '
	END
	else
	BEGIN
		select @WhereMaxProjection = ' and (p.ProjectionID = (select max(p1.ProjectionID) from tbl_sba_Projection p1 where p1.SBAPlanID = b.SBAPlanID) or p.ProjectionID IS NULL) '
		select @WhereMaxProjectionID = ' '
	END
	
	select @query = @query + @WhereDivision + @WhereMaxProjection +@WhereMaxProjectionID

	exec SP_EXECUTESQL @query

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting into temp table for report.'
		goto ERROREXIT
	END

		select 	CntrctNum,
			Dates_Effective,
			Dates_CntrctExp,
			CO_ID,
			FullName,
			Contractor_Name,
			Schedule_Name,
			Estimated_Contract_Value,
			DUNS,
			SBA_Plan_Exempt,
			BusinessSize,
			SBAPlanID,
			PlanName,
			PlanTypeID,
			PlanTypeDescription,
			ContractResponsible,
			ContractResponsibleDivision,
			ContractResponsibleCOID,
			ContractResponsibleCO,
			Plan_Admin_Name,
			Plan_Admin_state,
			Plan_Admin_email,
			Plan_Admin_Phone,
			MaxProjectionEndDate,
			ProjectionID,
			SBAPlanID1,
			SBDollars,
			SB,
			SDBDollars,
			SDB,
			WomenOwnedDollars,
			WO,
			DisabledVetDollars,
			DisabledVO,
			HubZoneDollars,
			HubZone,
			HBCUDollars,
			HBCU,
			VeteranOwnedDollars,
			VO,
			TotalSubConDollars,
			Comments,
			ProjectionStartDate,
			ProjectionEndDate	
		from #SBABYCONTRACTREPORT

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting SBA Related Contract Attributes Report.'
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




GO






