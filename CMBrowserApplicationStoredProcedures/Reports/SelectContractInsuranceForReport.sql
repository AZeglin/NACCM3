IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractInsuranceForReport')
	BEGIN
		DROP  Procedure  SelectContractInsuranceForReport
	END

GO

CREATE Procedure SelectContractInsuranceForReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractingOfficerId int, /* -1 = all */
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int /* may be -1 = all NAC */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3000),
		@joinSecurityServerName nvarchar(1000),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@SERVERNAME nvarchar(255)
	
BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Contract Insurance Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	create table #ContractInsuranceReport
	(
		ContractNumber nvarchar(50),
		VendorName nvarchar(75),
		InsuranceEffectiveDate datetime,
		InsuranceExpirationDate datetime,
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
		DivisionId int
	)

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
	
	select @query = 'insert into #ContractInsuranceReport
	(
		ContractNumber,
		VendorName,
		InsuranceEffectiveDate,
		InsuranceExpirationDate,
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
		DivisionId
	)
	select
		c.CntrctNum,
		c.Contractor_Name,
		c.Insurance_Policy_Effective_Date,
		c.Insurance_Policy_Expiration_Date,
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
		t.Division

	from  tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] s on c.CO_ID = s.CO_ID
		join [tlkup_Sched/Cat] t
		on c.Schedule_Number = t.Schedule_Number
		where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 '
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
			

	
	if @ScheduleNumber <> -1
	BEGIN
		select @whereSchedule = ' and t.Schedule_Number = ' + convert( nvarchar(10), @ScheduleNumber )
	END
	else
	BEGIN
		if @DivisionId <> -1
		BEGIN
			select @whereSchedule = ' and t.Division = ' + convert( nvarchar(10), @DivisionId )
		END
		else
		BEGIN
			select @whereSchedule = ' and t.Division <> 6 ' -- All NAC excludes SAC 
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
	
	exec SP_EXECUTESQL @query 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for contract insurance report.'
		goto ERROREXIT
	END

	select ContractNumber,
		VendorName,
		InsuranceEffectiveDate,
		InsuranceExpirationDate,
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
		DivisionId
		from #ContractInsuranceReport
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contract insurance report results.'
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




