USE [NAC_CM]
GO

/****** Object:  StoredProcedure [dbo].[ContractsWithExpiringSBAPlans]    Script Date: 02/28/2017 15:39:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Batch submitted through debugger: SQLQuery4.sql|9|0|C:\Users\AMMHINVORUGS.VHAMASTER\AppData\Local\Temp\3\~vs48E.sql








CREATE PROCEDURE [dbo].[ContractsWithExpiringSBAPlans]
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@Division int, /* -1 = All NAC */
--@ExpiringPeriod nvarchar(255),
@StartDate datetime,
@EndDate datetime
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
		@WhereDivision nvarchar(140),
		@Expirationwindow nvarchar (255)


BEGIN TRANSACTION


	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Contracts With Expiring SBA Plans Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #ContractsWithExpiringSBAPlans
	(
		CntrctNum					nvarchar(50),
		MasterContract				nvarchar(50),
		Assigned_CS					nvarchar(80),
		Contractor_Name				nvarchar(75),
		DUNS						nvarchar(9), 
		Dates_CntrctAward			Date, 
		Dates_Effective				Date, 
		Dates_CntrctExp				Date, 
		Dates_Completion			Date, 
		Schedule_Number				int, 
		Schedule_Name				nvarchar(75),  
		CO_ID						int, 
		FullName					nvarchar(80), 
		POC_Primary_Name			nvarchar(30), 
		POC_Primary_Phone			nvarchar(15), 
		POC_Primary_Email			nvarchar(50), 
		Estimated_Contract_Value	money, 
		SBA_Plan_Exempt				bit,
		SBA_Plan_Enddate			date,
		SBA_Plan_Name				nvarchar(50),
		SBA_Plan_Admin_Name			nvarchar(50),
		SBA_Plan_Admin_Email		nvarchar(50)
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

--	IF @ExpiringPeriod <> -1
	if (@StartDate is not null) and (@EndDate is not null)
	BEGIN
		select @query = 'insert into #ContractsWithExpiringSBAPlans
						(CntrctNum, 
						MasterContract,
						Assigned_CS,
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
						SBA_Plan_Enddate,
						SBA_Plan_Name,
						SBA_Plan_Admin_Name,
						SBA_Plan_Admin_Email)
						select c.CntrctNum,
						m.CntrctNum as Mastercontract,
						mp.FullName as Assigned_CS,
						c.Contractor_Name, 
						c.DUNS, 
						cast (c.Dates_CntrctAward as date) as Award_Date, 
						cast (c.Dates_Effective as date) as Effective_Date, 
						cast (c.Dates_CntrctExp as date) as Expiration_Date, 
						cast (c.Dates_Completion as date) as Completion_Date, 
						c.Schedule_Number, 
						s.Schedule_Name, 
						u.CO_ID, 
						u.FullName, 
						c.POC_Primary_Name, 
						c.POC_Primary_Phone, 
						c.POC_Primary_Email, 
						cast (c.Estimated_Contract_Value as numeric(20,2)) as Estimated_Contract_Value, 
						c.SBA_Plan_Exempt,
						cast (sp.EndDate as date) as SBA_Plan_EndDate,
						sb.PlanName,
						sb.Plan_Admin_Name,
						sb.Plan_Admin_email
						from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
						join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
						join tbl_sba_SBAPlan sb on c.SBAPlanID = sb.SBAPlanID
						join tbl_Cntrcts m on m.CntrctNum = (dbo.GetContractResponsibleForSBAPlanFunction (c.SBAPlanID, GETDATE()))
						join tlkup_UserProfile mp on m.CO_ID = mp.CO_ID
						cross apply (select top 1 EndDate from NAC_CM.dbo.tbl_sba_Projection 
						where SBAPlanID = c.SBAPlanID order by EndDate desc) as sp
						where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 
						and (( c.SBAPlanId is not null and exists ( select p.SBAPlanId from tbl_sba_SBAPlan p where p.SBAPlanId = c.SBAPlanId ))
						or ( c.SBAPlanId != 511 ))
						and c.Schedule_Number not in ( 14, 15, 39, 41, 48, 52 )
						and c.Socio_Business_Size_ID = 2 
						and c.Estimated_Contract_Value >= dbo.GetSBAContractValueThresholdFunction( c.Dates_CntrctAward )
						and c.SBA_Plan_Exempt = 0 '
/*						select c.CntrctNum, 
						c.Contractor_Name, 
						c.DUNS, 
						cast (c.Dates_CntrctAward as date), 
						cast (c.Dates_Effective as date), 
						cast (c.Dates_CntrctExp as date), 
						cast (c.Dates_Completion as date), 
						c.Schedule_Number, 
						s.Schedule_Name, 
						u.CO_ID, 
						u.FullName, 
						c.POC_Primary_Name, 
						c.POC_Primary_Phone, 
						c.POC_Primary_Email, 
						cast (Estimated_Contract_Value as numeric(20,2)), 
						c.SBA_Plan_Exempt,
						cast (sp.EndDate as date),
						sb.PlanName,
						sb.Plan_Admin_Name,
						sb.Plan_Admin_email
						from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
						join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
						join tbl_sba_SBAPlan sb on c.SBAPlanID = sb.SBAPlanID
						cross apply (select top 1 EndDate from NAC_CM.dbo.tbl_sba_Projection 
						where SBAPlanID = c.SBAPlanID order by EndDate desc) as sp
						where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
						and (( c.SBAPlanId is not null and exists ( select p.SBAPlanId from tbl_sba_SBAPlan p where p.SBAPlanId = c.SBAPlanId ))
						or ( c.SBAPlanId != 511 ))
						and c.Schedule_Number not in ( 14, 15, 39, 41, 48, 52 )
						and c.Socio_Business_Size_ID = 2 
						and c.Estimated_Contract_Value >= dbo.GetSBAContractValueThresholdFunction( c.Dates_CntrctAward )
						and c.SBA_Plan_Exempt = 0 
*/
	END
				
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string'
		goto ERROREXIT
	END

	if @Division <> -1
	BEGIN
		select @WhereDivision = ' and s.Division = ' + convert( nvarchar(10), @Division )
	END
	else
	BEGIN
		select @WhereDivision = ' and s.Division <> 6 ' -- All NAC excludes SAC 
	END
	
	if (@StartDate is not null) and (@EndDate is not null)
		BEGIN
			select @Expirationwindow = ' and sp.EndDate between @StartDate_parm and @EndDate_parm '  
		END
/*
	if @ExpiringPeriod = 1
		BEGIN
			select @Expirationwindow = ' and sp.EndDate between GETDATE() and (dateadd(d, datediff(d, -30, GETDATE()), 0)) '
		END
	else if @ExpiringPeriod = 2
		BEGIN
			select @Expirationwindow = ' and sp.EndDate between (dateadd(d, datediff(d, -31, GETDATE()), 0)) and (dateadd(d, datediff(d, -60, GETDATE()), 0)) '
		END	
	else if @ExpiringPeriod = 3
		BEGIN
			select @Expirationwindow = ' and sp.EndDate between (dateadd(d, datediff(d, -61, GETDATE()), 0)) and (dateadd(d, datediff(d, -90, GETDATE()), 0)) '
		END
	else if @ExpiringPeriod = 4
		BEGIN
			select @Expirationwindow = ' and sp.EndDate between (dateadd(d, datediff(d, -91, GETDATE()), 0)) and (dateadd(d, datediff(d, -120, GETDATE()), 0)) '
		END
	else
		BEGIN
			select @Expirationwindow = ' '
		END

*/
	select @sqlParms = '@StartDate_parm datetime, @EndDate_parm datetime'

	select @query = @query + @WhereDivision + @Expirationwindow

	exec SP_EXECUTESQL @query, @SQLParms, @StartDate_parm = @StartDate, @EndDate_parm = @EndDate 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting Contracts Without SBA Plans Report.'
		goto ERROREXIT
	END
	
	select CntrctNum,
		MasterContract,
		Assigned_CS,
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
		SBA_Plan_EndDate,
		SBA_Plan_Name,
		SBA_Plan_Admin_Name,
		SBA_Plan_Admin_Email
	from #ContractsWithExpiringSBAPlans

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting Contracts Without SBA Plans Report from temp table.'
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


