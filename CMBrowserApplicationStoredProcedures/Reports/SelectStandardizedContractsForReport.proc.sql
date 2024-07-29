IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectStandardizedContractsForReport' )
BEGIN
	DROP PROCEDURE SelectStandardizedContractsForReport
END
GO

CREATE PROCEDURE SelectStandardizedContractsForReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ActiveAll nvarchar(12)  /* may be 'Active' or 'All' */
)

AS

Declare 	@error int,
			@rowCount int,
			@errorMsg nvarchar(1000),
			@query nvarchar(3200),
			@SQLParms nvarchar(1000),
			@activeAllClause nvarchar(200),
			@joinSecurityServerName nvarchar(1000)


BEGIN TRANSACTION


	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Standardized Contract Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #StandardizedContractReport
	(
		ContractNumber nvarchar(50),
		VendorName nvarchar(75),
		AwardDate datetime,
		ExpirationDate datetime,
		BusinessSize nvarchar(20),
		EstimatedContractValue money,
		Description nvarchar(200),
		COId int,
		ContractingOfficerName nvarchar(50),
		
		ContractAdministrator nvarchar(30),
		ContractAdministratorPhone nvarchar(15),
		ContractAdministratorPhoneExt nvarchar(5),
		ContractAdministratorEmail nvarchar(50),
		
		ScheduleNumber int,
		ScheduleName nvarchar(75),
		DivisionId int
	)

	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'


	if @ActiveAll= 'Active'
	BEGIN		
		select @activeAllClause = ' and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 '
	END
	else
	BEGIN
		select @activeAllClause = ''
	END

	select @query = 'insert into #StandardizedContractReport
	(
		ContractNumber,
		VendorName,
		AwardDate,
		ExpirationDate,
		BusinessSize,
		EstimatedContractValue,
		Description,
		COId,
		ContractingOfficerName,
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorPhoneExt,
		ContractAdministratorEmail,
		ScheduleNumber,
		ScheduleName,
		DivisionId
	)
	select
		c.CntrctNum,
		c.Contractor_Name,
		c.Dates_CntrctAward,
		c.Dates_CntrctExp,
		case when ( c.Socio_Business_Size_ID = 1 ) then ''Small'' else ''Large'' end as BusinessSize,
		c.Estimated_Contract_Value,
		c.Drug_Covered,
		c.CO_ID,
		s.FullName,
		c.POC_Primary_Name,
		c.POC_Primary_Phone,
		c.POC_Primary_Ext,
		c.POC_Primary_Email,
		c.Schedule_Number,
		t.Schedule_Name,
		t.Division

	from  tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] s on c.CO_ID = s.CO_ID
		join [tlkup_Sched/Cat] t on c.Schedule_Number = t.Schedule_Number 
		where Standardized = 1 '
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string'
		goto ERROREXIT
	END

	select @query = @query + @activeAllClause
	
	exec SP_EXECUTESQL @query 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for standardized contract report.'
		goto ERROREXIT
	END

	select ContractNumber,
		VendorName,
		AwardDate,
		ExpirationDate,
		BusinessSize,
		EstimatedContractValue,
		Description,
		COId,
		ContractingOfficerName,
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorPhoneExt,
		ContractAdministratorEmail,
		ScheduleNumber,
		ScheduleName,
		DivisionId
		from #StandardizedContractReport
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting standardized contract report results.'
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


