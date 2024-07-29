IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetContractOwnerInfo' )
BEGIN
	DROP PROCEDURE GetContractOwnerInfo
END
GO

CREATE PROCEDURE GetContractOwnerInfo
(
@CurrentUser uniqueidentifier,
@UserLogin nvarchar(120),
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@COID int OUTPUT,
@ContractingOfficerFullName nvarchar(80) OUTPUT,
@ContractingOfficerPhone nvarchar(20) OUTPUT,
@ContractingOfficerUserId uniqueidentifier OUTPUT,
@SeniorContractSpecialistCOID int OUTPUT,
@SeniorContractSpecialistName nvarchar(80) OUTPUT,
@AssistantDirectorCOID int OUTPUT,
@AssistantDirectorName nvarchar(80) OUTPUT
)

AS

Declare @scheduleNumber int,
	 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@query nvarchar(4000),
		@SQLParms nvarchar(400),
		@joinSecurityServerName nvarchar(300)
		
BEGIN TRANSACTION

	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'

	select @COID = CO_ID,
		@scheduleNumber = Schedule_Number
	from tbl_Cntrcts
	where CntrctNum = @ContractNumber
		
	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving contract info for CO lookup.'
		goto ERROREXIT
	END

	select @SeniorContractSpecialistCOID = Schedule_Manager,
		@AssistantDirectorCOID = Asst_Director
	from [tlkup_Sched/Cat] 
	where Schedule_Number = @scheduleNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving schedule info for CO lookup.'
		goto ERROREXIT
	END

	select @query = 'select @ContractingOfficerFullName_parm = FullName,
		@ContractingOfficerPhone_parm = User_Phone,
		@ContractingOfficerUserId_parm = UserId 
	from ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] 
	where CO_ID = @COID_parm '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string when retrieving CO details for CO lookup.'
		goto ERROREXIT
	END

	select @SQLParms = N' @COID_parm int, @ContractingOfficerFullName_parm nvarchar(80) OUTPUT, @ContractingOfficerPhone_parm nvarchar(20) OUTPUT, @ContractingOfficerUserId_parm uniqueidentifier OUTPUT'

	exec SP_EXECUTESQL @query, @SQLParms, @COID_parm = @COID, @ContractingOfficerFullName_parm = @ContractingOfficerFullName OUTPUT, @ContractingOfficerPhone_parm = @ContractingOfficerPhone OUTPUT, @ContractingOfficerUserId_parm = @ContractingOfficerUserId OUTPUT

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving CO details for CO lookup.'
		goto ERROREXIT
	END
	
	select @query = 'select @SeniorContractSpecialistName_parm = FullName
	from ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] 
	where CO_ID = @SeniorContractSpecialistCOID_parm '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string (2) when retrieving CO details for CO lookup.'
		goto ERROREXIT
	END

	select @SQLParms = N' @SeniorContractSpecialistCOID_parm int, @SeniorContractSpecialistName_parm nvarchar(80) OUTPUT'

	exec SP_EXECUTESQL @query, @SQLParms, @SeniorContractSpecialistCOID_parm = @SeniorContractSpecialistCOID, @SeniorContractSpecialistName_parm = @SeniorContractSpecialistName OUTPUT

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving senior CO details for CO lookup.'
		goto ERROREXIT
	END
	
	select @query = 'select @AssistantDirectorName_parm = FullName
	from ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] 
	where CO_ID = @AssistantDirectorCOID_parm'

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string (3) when retrieving CO details for CO lookup.'
		goto ERROREXIT
	END

	select @SQLParms = N' @AssistantDirectorCOID_parm int, @AssistantDirectorName_parm nvarchar(80) OUTPUT'

	exec SP_EXECUTESQL @query, @SQLParms, @AssistantDirectorCOID_parm = @AssistantDirectorCOID, @AssistantDirectorName_parm = @AssistantDirectorName OUTPUT

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving AD details for CO lookup.'
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


