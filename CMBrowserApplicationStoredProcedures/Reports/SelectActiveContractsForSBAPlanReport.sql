IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectActiveContractsForSBAPlanReport')
	BEGIN
		DROP  Procedure  SelectActiveContractsForSBAPlanReport
	END

GO

CREATE Procedure SelectActiveContractsForSBAPlanReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@SBAPlanId int
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3000),
		@SQLParms nvarchar(1000),
		@joinSecurityServerName nvarchar(1000),
		@SERVERNAME nvarchar(255)

		
BEGIN TRANSACTION


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
	
	select @query = 'select c.CntrctNum, c.Contractor_Name, c.Dates_CntrctAward, c.Dates_CntrctExp, c.Schedule_Number, s.Schedule_Name, c.Estimated_Contract_Value, c.CO_ID, p.FullName
	from tbl_Cntrcts c join ' + @joinSecurityServerName + '.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
	join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number 
	where c.SBAPlanID = @SBAPlanId_parm
	and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
	order by c.Estimated_Contract_Value desc '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END

	select @SQLParms = N'@SBAPlanId_parm int'
	
	exec SP_EXECUTESQL @query, @SQLParms, @SBAPlanId_parm = @SBAPlanId

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for sba sub-report.'
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

