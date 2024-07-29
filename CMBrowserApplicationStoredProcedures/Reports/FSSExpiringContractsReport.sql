IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'FSSExpiringContractsReport')
	BEGIN
		DROP  Procedure  FSSExpiringContractsReport
	END

GO

CREATE Procedure FSSExpiringContractsReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractingOfficerId int, /* -1 = all */
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int, /* may be -1 = all NAC */
@ExpiringInDays int  /* will be 365, 90, 60, 30, more or less */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3000),
		@GroupBy nvarchar(3000),
		@sqlParms nvarchar(1000),
		@ExpiringInDaysNegative int,
		@joinSecurityServerName nvarchar(1000),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@SERVERNAME nvarchar(255)
	
BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'FSS Expiring Contracts Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	create table #FSSExpiringContractReport
	(
		ContractNumber nvarchar(50),
		VendorName nvarchar(75),
		AwardDate datetime,
		ExpirationDate datetime,
		TotalOptionYears int,
		CurrentOptionYear nvarchar(4),
		BusinessSize nvarchar(20),
		EstimatedContractValue money,
		Description nvarchar(200),
		COId int,
		ContractingOfficerName nvarchar(50),
		ScheduleNumber int,
		ScheduleName nvarchar(75)   null,
		DivisionId int,
		ContractAdministrator nvarchar(30),
		ContractAdministratorPhone nvarchar(15),
		ContractAdministratorEmail nvarchar(50),
		VendorAddress1  nvarchar(100),
		VendorAddress2  nvarchar(100),
		VendorCity  nvarchar(20),
		VendorStateCode nvarchar(2),
		VendorZip  nvarchar(10),
		TotalSales money,

		ExtensionInHouse bit,	-- the following fields to support FSS offer in-house
		OfferNumber  	nvarchar(30)  null,
		ReceivedDate   datetime  null,
		AssignmentDate datetime  null,
		ReassignmentDate  datetime  null,
		ActionDate datetime  null,
		ActionId	int  null,
		ActionDescription nvarchar(30) null,
		OfferCOId int  null,
		OfferContractingOfficerLastName  nvarchar(40) null,
		OfferContractingOfficerName   nvarchar(50) null,
		OfferLastModificationDate  datetime null,
		OfferLastModifiedBy	nvarchar(120) null
	)

	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'

	select @query = 'insert into #FSSExpiringContractReport
	(
		ContractNumber,
		VendorName,
		AwardDate,
		ExpirationDate,
		TotalOptionYears,
		CurrentOptionYear,
		BusinessSize,
		EstimatedContractValue,
		Description,
		COId,
		ContractingOfficerName,
		ScheduleNumber,
		DivisionId,
		
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorEmail,
		VendorAddress1,
		VendorAddress2,
		VendorCity,
		VendorStateCode,
		VendorZip,
		TotalSales,
		ExtensionInHouse
	)
	select
		c.CntrctNum,
		c.Contractor_Name,
		c.Dates_CntrctAward,
		c.Dates_CntrctExp,
	    c.Dates_TotOptYrs as TOY,
		case when ( c.Dates_TotOptYrs is not null ) then case when ( DATEDIFF( day, CONVERT( datetime, CONVERT( nvarchar(2),  DATEPART( MONTH, GETDATE() )) + ''/'' + CONVERT( nvarchar(2), DATEPART( DAY, GETDATE() ))  + ''/'' + CONVERT( nvarchar(4), DATEPART( YEAR,  c.Dates_Effective )) ), c.Dates_Effective ) < 0 ) then convert( nvarchar(4), (YEAR(GETDATE()) - YEAR(c.Dates_Effective))) else convert( nvarchar(4), (( YEAR(GETDATE()) - YEAR(c.Dates_Effective) ) - 1 )) end else ''N/A'' end as COY,
		case when ( c.Socio_Business_Size_ID = 1 ) then ''Small'' else ''Large'' end as BusinessSize,
		c.Estimated_Contract_Value,
		c.Drug_Covered,
		c.CO_ID,
		u.FullName,
		c.Schedule_Number,
		t.Division,

		c.POC_Primary_Name,
		c.POC_Primary_Phone,
		c.POC_Primary_Email,
		c.Primary_Address_1,
		c.Primary_Address_2,
		c.Primary_City,
		c.Primary_State,
		c.Primary_Zip,
		
		sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales),
		0

	from  tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] t
		on c.Schedule_Number = t.Schedule_Number
		join tbl_Cntrcts_Sales s on c.CntrctNum = s.CntrctNum 
		where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 
		and getdate() between DATEADD( d, @ExpiringInDays_parm, c.Dates_CntrctExp ) and c.Dates_CntrctExp '
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
			
	select @GroupBy = 'group by	c.CntrctNum,
		c.Contractor_Name,
		c.Dates_CntrctAward,
		c.Dates_CntrctExp,
		c.Dates_Effective,
		c.Dates_TotOptYrs,
		c.Socio_Business_Size_ID,
		c.Estimated_Contract_Value,
		c.Drug_Covered,
		c.CO_ID,
		u.FullName,
		c.Schedule_Number,
		t.Division,

		c.POC_Primary_Name,
		c.POC_Primary_Phone,
		c.POC_Primary_Email,
		c.Primary_Address_1,
		c.Primary_Address_2,
		c.Primary_City,
		c.Primary_State,
		c.Primary_Zip '			
				
	if @ExpiringInDays > 1100 or @ExpiringInDays < 1
	BEGIN
		select @errorMsg = 'Expiring in days parameter is out of range.'
		goto ERROREXIT
	END
		
	select @ExpiringInDaysNegative = @ExpiringInDays * -1		
		
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
	
	select @sqlParms = '@ExpiringInDays_parm int'

	select @query = @query + @whereSchedule + @whereContractingOfficer + @GroupBy 
	
	exec SP_EXECUTESQL @query, @sqlParms, @ExpiringInDays_parm = @ExpiringInDaysNegative

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for active contract report.'
		goto ERROREXIT
	END

	update #FSSExpiringContractReport
	set ScheduleName = s.Schedule_Name
	from [tlkup_Sched/Cat] s join #FSSExpiringContractReport t on s.Schedule_Number = t.ScheduleNumber

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating schedule name for active contract report.'
		goto ERROREXIT
	END

	-- offer related fields
	update #FSSExpiringContractReport
	set ExtensionInHouse = 1,
		OfferNumber = o.OfferNumber,
		ReceivedDate = o.Dates_Received,
		AssignmentDate = o.Dates_Assigned,
		ReassignmentDate = o.Dates_Reassigned,
		ActionDate = o.Dates_Action,
		ActionId = o.Action_ID,
		ActionDescription = a.Action_Description,		
		OfferCOId = o.CO_ID,
		OfferContractingOfficerLastName = u.LastName,
		OfferContractingOfficerName = u.FullName,
		OfferLastModificationDate = o.Date_Modified,
		OfferLastModifiedBy = o.LastModifiedBy

	from tbl_Offers o join #FSSExpiringContractReport t on t.ContractNumber = o.ExtendsContractNumber
	join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
	join tlkup_UserProfile u on o.CO_ID = u.CO_ID

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating offer extension info for active contract report.'
		goto ERROREXIT
	END



	select ContractNumber,
		VendorName,
		AwardDate,
		ExpirationDate,		
		TotalOptionYears,
		CurrentOptionYear,
		BusinessSize,
		EstimatedContractValue,
		Description,
		COId,
		ContractingOfficerName,
		ContractAdministratorEmail,
		ScheduleNumber,
		ScheduleName,
		DivisionId,
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorEmail,
		VendorAddress1,
		VendorAddress2,
		VendorCity,
		VendorStateCode,
		VendorZip,
		TotalSales,

		ExtensionInHouse,	
		OfferNumber,
		ReceivedDate,		
		ActionDate,
		ActionDescription,	
		OfferContractingOfficerLastName,
		OfferContractingOfficerName

		from #FSSExpiringContractReport
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting active contract report results.'
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


