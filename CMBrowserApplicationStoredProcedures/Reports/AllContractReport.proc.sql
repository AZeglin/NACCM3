IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'AllContractReport' )
BEGIN
	DROP PROCEDURE AllContractReport
END
GO


CREATE PROCEDURE AllContractReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractingOfficerId int, /* -1 = all */
@SIN nvarchar(20),  /* may have "All" = all */
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int, /* may be -1 = all NAC */
@ActiveExpiredBoth  char(1),   /* 'A' 'E' 'B' */
@StartDate Datetime, 
@EndDate Datetime 
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(4000),
		@GroupBy nvarchar(3000),
		@joinSecurityServerName nvarchar(1000),
		@whereContractingOfficer nvarchar(100),
		@whereSchedule nvarchar(100),
		@whereSIN nvarchar(100),
		@SERVERNAME nvarchar(255),
		@whereActiveExpired nvarchar(1000),
		@sqlParms nvarchar(1000)

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'All Contract Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	
	
	create table #AllContractReport
	(
		ContractNumber nvarchar(50),
		VendorName nvarchar(75),
		AwardDate datetime,
		EffectiveDate datetime,
		ExpirationDate datetime,
		CompletionDate datetime,
		BusinessSize nvarchar(20),
		EstimatedContractValue money,
		Description nvarchar(200),
		COId int,
		ContractingOfficerName nvarchar(50),
		VendorPrimaryPOCName nvarchar(30),
		VendorPrimaryPOCAddress1 nvarchar(100),
		VendorPrimaryPOCAddress2 nvarchar(100),
		VendorPrimaryPOCCity nvarchar(20),
		VendorPrimaryPOCState nvarchar(2),
		VendorPrimaryPOCZip nvarchar(10),
		VendorPrimaryPOCEmail nvarchar(50),
		VendorPrimaryPOCPhone nvarchar(15),
		VendorPrimaryPOCPhoneExt nvarchar(5),
		VendorAltPOCName nvarchar(30),
		VendorAltPOCEmail nvarchar(50),
		VendorAltPOCPhone nvarchar(15),
		VendorAltPOCPhoneExt nvarchar(5),
		VendorSalesPOCName nvarchar(30),
		VendorSalesPOCEmail nvarchar(50),
		VendorSalesPOCPhone nvarchar(15),
		VendorSalesPOCPhoneExt nvarchar(5),
		ScheduleNumber int,
		ScheduleName nvarchar(75),
		DivisionId int,
		Active bit,
		TotalSales money
	)

	
	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'
	
	
	select @query = 'insert into #AllContractReport
	(
		ContractNumber,
		VendorName,
		AwardDate,
		EffectiveDate,
		ExpirationDate,
		CompletionDate,
		BusinessSize,
		EstimatedContractValue,
		Description,
		COId,
		ContractingOfficerName,
		VendorPrimaryPOCName,
		VendorPrimaryPOCAddress1,
		VendorPrimaryPOCAddress2,
		VendorPrimaryPOCCity,
		VendorPrimaryPOCState,
		VendorPrimaryPOCZip,
		VendorPrimaryPOCEmail,
		VendorPrimaryPOCPhone,
		VendorPrimaryPOCPhoneExt,
		VendorAltPOCName,
		VendorAltPOCEmail,
		VendorAltPOCPhone,
		VendorAltPOCPhoneExt,
		VendorSalesPOCName,
		VendorSalesPOCEmail,
		VendorSalesPOCPhone,
		VendorSalesPOCPhoneExt,

		ScheduleNumber,
		ScheduleName,
		DivisionId,
		Active,
		TotalSales
	)
	select
		c.CntrctNum,
		c.Contractor_Name,
		c.Dates_CntrctAward,
		c.Dates_Effective,
		c.Dates_CntrctExp,
		c.Dates_Completion,
		case when ( c.Socio_Business_Size_ID = 1 ) then ''Small'' else ''Large'' end as BusinessSize,
		c.Estimated_Contract_Value,
		c.Drug_Covered,
		c.CO_ID,
		u.FullName,
		c.POC_Primary_Name,
		c.Primary_Address_1,
		c.Primary_Address_2,
		c.Primary_City,
		c.Primary_State,
		c.Primary_Zip,
		c.POC_Primary_Email,
		c.POC_Primary_Phone,
		c.POC_Primary_Ext,
		c.POC_Alternate_Name,
		c.POC_Alternate_Email,
		c.POC_Alternate_Phone,
		c.POC_Alternate_Ext,
		c.POC_Sales_Name,
		c.POC_Sales_Email,
		c.POC_Sales_Phone,
		c.POC_Sales_Ext,
		c.Schedule_Number,
		t.Schedule_Name,
		t.Division,
		dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) as Active,
		sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales)

	from  tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] t
		on c.Schedule_Number = t.Schedule_Number
		full join tbl_Cntrcts_Sales s on c.CntrctNum = s.CntrctNum
		where c.Schedule_Number <> 0 '
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
			
	select @GroupBy = 'group by	c.CntrctNum,
		c.Contractor_Name,
		c.Dates_CntrctAward,
		c.Dates_Effective,
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
		c.Primary_Zip,
		c.Dates_Completion,
		c.POC_Primary_Ext,
		c.POC_Alternate_Name,
		c.POC_Alternate_Email,
		c.POC_Alternate_Phone,
		c.POC_Alternate_Ext,
		c.POC_Sales_Name,
		c.POC_Sales_Email,
		c.POC_Sales_Phone,
		c.POC_Sales_Ext,
		t.Schedule_Name '
				
------------ Schecule Number ---------------	
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
--------------- SIN ----------------------
	if LTRIM(RTRIM(@SIN)) <> 'All'
	BEGIN
		select @whereSIN = ' and c.CntrctNum in ( select s.CntrctNum from [tbl_Cntrcts_SINs] s where s.Inactive = 0 and s.[SINs] = ''' + LTRIM(RTRIM(@SIN)) + ''' )'
	END
	else
	BEGIN
		select @whereSIN = ' '
	END	
-------------- Contrating Officer ------------------	
	if @ContractingOfficerId <> -1
	BEGIN
		select @whereContractingOfficer = ' and c.CO_ID = ' + convert( nvarchar(10), @ContractingOfficerId )
	END
	else
	BEGIN
		select @whereContractingOfficer = ' '
	END
-------------- Active ----------------------------
	if @ActiveExpiredBoth = 'A'
	BEGIN
		if @StartDate is not null and @EndDate is not null
		BEGIN
			select @whereActiveExpired = ' and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 
																	and (c.Dates_CntrctAward between @StartDate_parm and @EndDate_parm
																			or c.Dates_Effective between @StartDate_parm and @EndDate_parm
																			or c.Dates_CntrctExp between @StartDate_parm and @EndDate_parm
																			or ( c.Dates_Completion is not null and c.Dates_Completion between @StartDate_parm and @EndDate_parm )) '
																	
		END
		else 
		BEGIN
			select @whereActiveExpired = ' and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 '
		END
	END
	else if @ActiveExpiredBoth = 'E'
	BEGIN
		if @StartDate is not null and @EndDate is not null
		BEGIN
			select @whereActiveExpired =' and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 0 
																	and (c.Dates_CntrctAward between @StartDate_parm and @EndDate_parm
																			or c.Dates_Effective between @StartDate_parm and @EndDate_parm
																			or c.Dates_CntrctExp between @StartDate_parm and @EndDate_parm
																			or ( c.Dates_Completion is not null and c.Dates_Completion between @StartDate_parm and @EndDate_parm )) '
		END
		else 
		BEGIN
			select @whereActiveExpired = ' and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 0 '
		END
	END
	else
	BEGIN
		if @StartDate is not null and @EndDate is not null
		BEGIN
			select @whereActiveExpired = ' and (c.Dates_CntrctAward between @StartDate_parm and @EndDate_parm
																			or c.Dates_Effective between @StartDate_parm and @EndDate_parm
																			or c.Dates_CntrctExp between @StartDate_parm and @EndDate_parm
																			or ( c.Dates_Completion is not null and c.Dates_Completion between @StartDate_parm and @EndDate_parm )) '
		END
		else 
		BEGIN
			select @whereActiveExpired = ' '
		END
	END
	
	select @sqlParms = '@StartDate_parm datetime, @EndDate_parm datetime'
	
	select @query = @query + @whereSchedule + @whereSIN + @whereContractingOfficer + @whereActiveExpired + @GroupBy 
	
	exec SP_EXECUTESQL @query, @sqlParms, @StartDate_parm = @StartDate, @EndDate_parm = @EndDate 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for all contract report.'
		goto ERROREXIT
	END

	select ContractNumber,
		VendorName,
		AwardDate,
		EffectiveDate,
		ExpirationDate,
		CompletionDate,
		BusinessSize,
		EstimatedContractValue,
		Description,
		COId,
		ContractingOfficerName,
		VendorPrimaryPOCName,
		VendorPrimaryPOCAddress1,
		VendorPrimaryPOCAddress2,
		VendorPrimaryPOCCity,
		VendorPrimaryPOCState,
		VendorPrimaryPOCZip,
		VendorPrimaryPOCEmail,
		VendorPrimaryPOCPhone,
		VendorPrimaryPOCPhoneExt,
		VendorAltPOCName,
		VendorAltPOCEmail,
		VendorAltPOCPhone,
		VendorAltPOCPhoneExt,
		VendorSalesPOCName,
		VendorSalesPOCEmail,
		VendorSalesPOCPhone,
		VendorSalesPOCPhoneExt,
		ScheduleNumber,
		ScheduleName,
		DivisionId,
		Active,
		TotalSales
		from #AllContractReport
		
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


