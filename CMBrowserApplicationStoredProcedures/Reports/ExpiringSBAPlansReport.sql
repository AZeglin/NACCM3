IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ExpiringSBAPlansReport')
	BEGIN
		DROP  Procedure  ExpiringSBAPlansReport
	END

GO

CREATE Procedure ExpiringSBAPlansReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@DaysThreshold int
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
 
 	/* this report is being retired and this SP was never fully implemented, tested or deployed */

BEGIN TRANSACTION


	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Expiring SBA Plans Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #ExpiringSBAPlans
	(
		SBAPlanID int not null,
		ResponsibleContract nvarchar(50) null,
		ProjectionExpirationDate datetime
	)


	Insert into #ExpiringSBAPlans
	( SBAPlanID )
	select distinct p.SBAPlanID from tbl_sba_Projection p
			where p.EndDate between getdate() and DATEADD( d, @DaysThreshold, getdate() )
			and p.EndDate = ( select max( q.EndDate ) from tbl_sba_Projection q
								where p.SBAPlanID = q.SBAPlanID )
			and p.SBAPlanID is not null

	update #ExpiringSBAPlans
	set ProjectionExpirationDate = ( select max( q.EndDate ) from tbl_sba_Projection q
								where q.SBAPlanID = #ExpiringSBAPlans.SBAPlanID )

	update #ExpiringSBAPlans
	set ResponsibleContract = dbo.GetContractResponsibleForSBAPlanFunction( SBAPlanID, getdate())	
		
	select x.SBAPlanID,
		x.ResponsibleContract,
		x.ProjectionExpirationDate,
		c.Dates_CntrctExp as ExpirationDateOfMasterContract,
		p.FullName as ContractingOfficerOfMasterContract,
		c.Contractor_Name as VendorOfMasterContract,
		z.FullName as AssistantDirectorOfMasterContract
	from #ExpiringSBAPlans x join tbl_Cntrcts c on x.ResponsibleContract = c.CntrctNum
	join tlkup_UserProfile p on p.CO_ID = c.CO_ID
	join [tlkup_Sched/Cat] s on s.Schedule_Number = c.Schedule_Number
	join tlkup_UserProfile z on z.CO_ID = s.Asst_Director
	where c.SBA_Plan_Exempt = 0
	order by ProjectionExpirationDate 

	
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


