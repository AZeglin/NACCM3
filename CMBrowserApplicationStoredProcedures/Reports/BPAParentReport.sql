IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'BPAParentReport')
	BEGIN
		DROP  Procedure  BPAParentReport
	END

GO

CREATE Procedure BPAParentReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractingOfficerId int, /* -1 = all */
@ScheduleNumber int, /* may be -1 = all  - BPA Schedules Only */
@DivisionId int /* may be -1 all including SAC */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3000),
		@joinSecurityServerName nvarchar(1000),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100)
	
BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'BPA Parent Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	create table #BPAParentReport
	(
		BPAContractNumber nvarchar(50),
		ParentContractNumber nvarchar(50),
		VendorName nvarchar(75),
		BPAAwardDate datetime,
		BPAEffectiveDate datetime,
		BPAExpirationDate datetime,
		AwardDate datetime,
		EffectiveDate datetime,
		ExpirationDate datetime,
		CompletionDate datetime,
		BusinessSize nvarchar(20),
		EstimatedContractValue money,
		Description nvarchar(200),
		COId int,
		ContractingOfficerName nvarchar(50),
		ContractingOfficerPhone nvarchar(20),
		ParentCOId int,
		ParentContractingOfficerName nvarchar(50),
		ParentContractingOfficerPhone nvarchar(20),
		VendorPrimaryPOCEmail nvarchar(50),
		ScheduleNumber int,
		ScheduleName nvarchar(75),
		DivisionId int
	)


	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'
	
	
	select @query = 'insert into #BPAParentReport
	(
		BPAContractNumber,
		ParentContractNumber,
		VendorName,
		BPAAwardDate,
		BPAEffectiveDate,
		BPAExpirationDate,
		AwardDate,
		EffectiveDate,
		ExpirationDate,
		CompletionDate,
		BusinessSize,
		EstimatedContractValue,
		Description,
		COId,
		ContractingOfficerName,
		ContractingOfficerPhone,
		ParentCOId,
		ParentContractingOfficerName,
		ParentContractingOfficerPhone,
		VendorPrimaryPOCEmail,
		ScheduleNumber,
		ScheduleName,
		DivisionId
	)
	select
		c.CntrctNum,
		c.BPA_FSS_Counterpart,
		c.Contractor_Name,
		c.Dates_CntrctAward,
		c.Dates_Effective,
		c.Dates_CntrctExp,
		p.Dates_CntrctAward,
		p.Dates_Effective,
		p.Dates_CntrctExp,
		p.Dates_Completion,
		case when ( p.Socio_Business_Size_ID = 1 ) then ''Small'' else ''Large'' end as BusinessSize,
		p.Estimated_Contract_Value,
		p.Drug_Covered,
		p.CO_ID,
		u.FullName,
		u.User_Phone,
		c.CO_ID,
		x.FullName,
		x.User_Phone,
		p.POC_Primary_Email,
		c.Schedule_Number,
		s.Schedule_Name,
		s.Division

	from  tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		join tbl_Cntrcts p on c.BPA_FSS_Counterpart = p.CntrctNum
		join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] x on p.CO_ID = x.CO_ID
		where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 '
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
			

	
	if @ScheduleNumber <> -1
	BEGIN
		select @whereSchedule = ' and s.Schedule_Number = ' + convert( nvarchar(10), @ScheduleNumber )
	END
	else
	BEGIN
		if @DivisionId <> -1
		BEGIN
			select @whereSchedule = ' and s.Division = ' + convert( nvarchar(10), @DivisionId )
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
		select @errorMsg = 'Error selecting contracts for BPA Parent report.'
		goto ERROREXIT
	END

	select BPAContractNumber,
		ParentContractNumber,
		VendorName,
		BPAAwardDate,
		BPAEffectiveDate,
		BPAExpirationDate,
		AwardDate,
		EffectiveDate,
		ExpirationDate,
		CompletionDate,
		BusinessSize,
		EstimatedContractValue,
		Description,
		COId,
		ContractingOfficerName,
		ContractingOfficerPhone,
		ParentCOId,
		ParentContractingOfficerName,
		ParentContractingOfficerPhone,
		VendorPrimaryPOCEmail,
		ScheduleNumber,
		ScheduleName,
		DivisionId
		from #BPAParentReport
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting BPA Parent report results.'
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


