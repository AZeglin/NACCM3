IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'MedSurgPricelistStatusByContractReport')
	BEGIN
		DROP  Procedure  MedSurgPricelistStatusByContractReport
	END

GO

CREATE Procedure MedSurgPricelistStatusByContractReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ScheduleNumber int, /* may be -1 = all */
@DivisionId int, /* may be -1 = all NAC */
@IncludeErrors char(1)  /* E for errors only, otherwise all */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@SERVERNAME nvarchar(255),
		@joinSecurityServerName nvarchar(1000),
		@query nvarchar(4000),
		@SQLParms nvarchar(1000),
		@whereSchedule nvarchar(100)



BEGIN TRANSACTION


	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'MedSurg Pricelist Status By Contract Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	create table #PricelistReport
	(
		ContractNumber nvarchar(50),
		ContractId int,
		CO_ID int,
		ContractingOfficerName nvarchar(50),
		ContractorName nvarchar(75),
		ContractExpirationDate datetime,
		ScheduleNumber int,
		ScheduleName nvarchar(75),
		PrimeVendor bit,
		PricelistVerified bit,
		PricelistVerificationDate datetime,
		PricelistVerifiedBy nvarchar(25),
		LastRecordedModNumber nvarchar(20),
		PricelistNotes nvarchar(255),
		PriceCount int,
		ActivePriceCount int, /* has a current date */
		ExpiredPriceCount int, /* has an expired date */
		PricesExpiringBeforeContract int,
		FuturePriceCount int,
		NullDateCount int
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

	select @query = 'insert into #PricelistReport
		( ContractNumber, ContractId, CO_ID, ContractingOfficerName, ContractorName, ContractExpirationDate, ScheduleNumber, ScheduleName, PrimeVendor,
			PricelistVerified, PricelistVerificationDate, PricelistVerifiedBy, LastRecordedModNumber, PricelistNotes )
		select c.CntrctNum, c.Contract_Record_ID, u.CO_ID, u.FullName, c.Contractor_Name, c.Dates_CntrctExp, c.Schedule_Number, s.Schedule_Name, c.PV_Participation,
			c.Pricelist_Verified, c.Verification_Date, c.Verified_By, c.Current_Mod_Number, c.Pricelist_Notes
		from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] u on c.CO_ID = u.CO_ID
		join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
		and c.Schedule_Number not in ( 1, 18, 28, 29, 30, 31, 32, 37, 39, 43, 47, 48, 50 ) '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string'
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
		else
		BEGIN
			select @whereSchedule = ' and s.Division <> 6 ' -- All NAC excludes SAC  
		END
	END
	
	select @query = @query + @whereSchedule
	
	exec SP_EXECUTESQL @query 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracts for MedSurg Pricelist Status By Contract Report.'
		goto ERROREXIT
	END

	/* get the price counts from the pricelist table */
	/* under the new schema, the concept of expired and active prices has changed. all items prices in the active tables are current or future */
	update #PricelistReport
	set PriceCount = ( select count(*) from CM_Items i join CM_ItemPrice p on i.ItemId = p.ItemId where i.ContractId = #PricelistReport.ContractId  ) 	

	update #PricelistReport
	set ActivePriceCount = ( select count(*) from CM_Items i join CM_ItemPrice p on i.ItemId = p.ItemId where i.ContractId = #PricelistReport.ContractId and getdate() between p.PriceStartDate and p.PriceStopDate ) 	

	update #PricelistReport
	set ExpiredPriceCount = ( select count(*) from CM_Items i join CM_ItemPrice p on i.ItemId = p.ItemId where i.ContractId = #PricelistReport.ContractId and p.PriceStopDate < getdate() ) 	
		
	update #PricelistReport
	set PricesExpiringBeforeContract = ( select count(*) from CM_Items i join CM_ItemPrice p on i.ItemId = p.ItemId where i.ContractId = #PricelistReport.ContractId and p.PriceStopDate < #PricelistReport.ContractExpirationDate ) 	
		
	update #PricelistReport
	set FuturePriceCount = ( select count(*) from CM_Items i join CM_ItemPrice p on i.ItemId = p.ItemId where i.ContractId = #PricelistReport.ContractId and p.PriceStartDate > getdate() ) 	
							
	update #PricelistReport
	set NullDateCount = 0
												
	/* note: not returning pricelist notes */	
	/* errors only */
	if @IncludeErrors = 'E'
	BEGIN
		select ContractNumber, CO_ID, ContractingOfficerName, ContractorName, ContractExpirationDate, ScheduleNumber, ScheduleName, PrimeVendor,
				PricelistVerified, PricelistVerificationDate, PricelistVerifiedBy, LastRecordedModNumber,
				PriceCount, ActivePriceCount, ExpiredPriceCount, PricesExpiringBeforeContract,
				FuturePriceCount, NullDateCount
		from #PricelistReport
		where ExpiredPriceCount > 0 or PricesExpiringBeforeContract > 0 or NullDateCount > 0
		order by ContractNumber
		
		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting MedSurg Pricelist Status By Contract Report.'
			goto ERROREXIT
		END
	END
	else
	BEGIN
		select ContractNumber, CO_ID, ContractingOfficerName, ContractorName, ContractExpirationDate, ScheduleNumber, ScheduleName, PrimeVendor,
				PricelistVerified, PricelistVerificationDate, PricelistVerifiedBy, LastRecordedModNumber,
				PriceCount, ActivePriceCount, ExpiredPriceCount, PricesExpiringBeforeContract,
				FuturePriceCount, NullDateCount
		from #PricelistReport
		order by ContractNumber
		
		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting MedSurg Pricelist Status By Contract Report.'
			goto ERROREXIT
		END	
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



