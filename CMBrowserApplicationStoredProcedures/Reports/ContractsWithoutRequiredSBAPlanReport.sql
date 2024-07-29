IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ContractsWithoutRequiredSBAPlanReport')
	BEGIN
		DROP  Procedure  ContractsWithoutRequiredSBAPlanReport
	END

GO

CREATE Procedure ContractsWithoutRequiredSBAPlanReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255)
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@SERVERNAME nvarchar(255),
		@joinSecurityServerName nvarchar(1000),
		@query nvarchar(4000),
		@SQLParms nvarchar(1000)


	/* this report is being retired and this SP was never fully implemented, tested or deployed */
BEGIN TRANSACTION


	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Contracts Without Required SBA Plan Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END


	-- test for SQL1 usage
	SELECT @SERVERNAME = @@SERVERNAME
	
	if @SERVERNAME is null
	BEGIN
		select @joinSecurityServerName = '[' + @SecurityDatabaseName + ']'
	END
	else
	BEGIN
		select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'
	END

	select @query = 'select c.CntrctNum, u.CO_ID, u.FullName, c.Contractor_Name, s.Schedule_Name
		from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
		and c.Schedule_Number not in ( 15, 39, 41 )
		and c.Estimated_Contract_Value >= 500000
		and c.SBA_Plan_Exempt = 0
		and c.Socio_Business_Size_ID = 2
		and c.SBAPlanID IS NULL '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string'
		goto ERROREXIT
	END

	exec SP_EXECUTESQL @query

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting Contracts Without Required SBA Plan report.'
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


