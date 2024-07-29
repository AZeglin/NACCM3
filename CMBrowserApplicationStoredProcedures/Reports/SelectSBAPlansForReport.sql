IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSBAPlansForReport')
	BEGIN
		DROP  Procedure  SelectSBAPlansForReport
	END

GO

CREATE Procedure SelectSBAPlansForReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@StartingYear int,
@StartingMonth int
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
		
BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'SBA Plans Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	create table #SBAPlanReport
	(
		SBAPlanID int, 
		PlanName nvarchar(50), 
		PlanTypeID int, 
		PlanTypeDescription nvarchar(50),
		Plan_Admin_Name nvarchar(50), 
		Plan_Admin_email nvarchar(50), 
		ResponsibleContract nvarchar(20),
		ResponsibleContractScheduleNumber int,
		ResponsibleContractScheduleName nvarchar(75),
		ResponsibleContractContractorName nvarchar(75),
		ProjectionID int,
		ProjectionStartDate datetime,
		ProjectionEndDate datetime,
		ContractAwardDate datetime,
		ContractExpirationDate datetime
	)
	
	insert into #SBAPlanReport
	( SBAPlanID, PlanName, PlanTypeID, PlanTypeDescription, Plan_Admin_Name, Plan_Admin_email, ProjectionID, ProjectionStartDate, ProjectionEndDate )
	select s.SBAPlanID, s.PlanName, s.PlanTypeID, d.PlanTypeDescription, s.Plan_Admin_Name, s.Plan_Admin_email, p.ProjectionID, p.StartDate, p.EndDate 	
	
	from tbl_sba_SBAPlan s join tbl_sba_Projection p on s.SBAPlanID = p.SBAPlanID
	join tbl_sba_PlanType d on s.PlanTypeID = d.PlanTypeID
	where p.ProjectionID = ( select max( x.ProjectionID ) from tbl_sba_projection x
								where x.SBAPlanID = s.SBAPlanID	
								and x.EndDate =  ( select max( y.EndDate ) 
													from tbl_sba_Projection y
													where y.SBAPlanID = s.SBAPlanID
													and YEAR( y.StartDate ) >= @StartingYear
													and Month( y.StartDate ) >= @StartingMonth  ))
												
	update #SBAPlanReport
	set ResponsibleContract = dbo.GetContractResponsibleForSBAPlanFunction( SBAPlanID, getdate())	
	
	update #SBAPlanReport
	set ContractAwardDate = c.Dates_CntrctAward
	from tbl_Cntrcts c  join #SBAPlanReport r on c.CntrctNum = r.ResponsibleContract
	where r.ResponsibleContract is not null
	
	update #SBAPlanReport
	set ContractExpirationDate = c.Dates_CntrctExp
	from tbl_Cntrcts c  join #SBAPlanReport r on c.CntrctNum = r.ResponsibleContract
	where r.ResponsibleContract is not null

	update #SBAPlanReport
	set ResponsibleContractScheduleNumber = Schedule_Number,
		ResponsibleContractContractorName  = Contractor_Name
	from tbl_Cntrcts c  join #SBAPlanReport r on c.CntrctNum = r.ResponsibleContract
	where r.ResponsibleContract is not null

	update #SBAPlanReport
	set ResponsibleContractScheduleName = Schedule_Name
	from #SBAPlanReport r join [tlkup_Sched/Cat] s on r.ResponsibleContractScheduleNumber = s.Schedule_Number
	where r.ResponsibleContract is not null

	select SBAPlanID, 
		PlanName, 
		PlanTypeID, 
		PlanTypeDescription,
		Plan_Admin_Name, 
		Plan_Admin_email, 
		ResponsibleContract,
		ResponsibleContractScheduleNumber,
		ResponsibleContractScheduleName,
		ResponsibleContractContractorName,
		ProjectionID,
		ProjectionStartDate,
		ProjectionEndDate,
		ContractAwardDate,
		ContractExpirationDate
	from #SBAPlanReport
	order by ProjectionEndDate desc

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


