IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ContractSalesReport')
	BEGIN
		DROP  Procedure  ContractSalesReport
	END

GO

CREATE Procedure ContractSalesReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20)
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(3200),
		@SQLParms nvarchar(1000),
		@startQuarterId int,
		@endQuarterId int,
		@whereSchedule nvarchar(100),
		@groupByString nvarchar(400),
		@joinSecurityServerName nvarchar(1000),
		@SERVERNAME nvarchar(255)

	


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Contract Sales Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
		
	select c.Dates_Effective, c.Dates_CntrctExp
	from tbl_Cntrcts c
	where CntrctNum = @ContractNumber
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting start quarter.'
		goto ERROREXIT
	END
			
	select @startQuarterId = y.Quarter_ID
	from tlkup_year_qtr y
	where y.Year = @StartYear
	and y.Qtr = @StartQuarter

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting start quarter.'
		goto ERROREXIT
	END
	
	select @endQuarterId = y.Quarter_ID
	from tlkup_year_qtr y
	where getdate() between y.Start_Date and y.End_Date

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting end quarter.'
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
	

	select 	y.Title as 'YearQuarterDescription',	
		sum(s.VA_Sales) as 'VASales',
		sum(s.OGA_Sales) as 'OGASales',
		sum(s.SLG_Sales) as 'SLGSales',
		sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales) as 'TotalSales'

	from tbl_Cntrcts_Sales s join tlkup_year_qtr y
		on s.Quarter_ID = y.Quarter_ID
		join tbl_Cntrcts c
		on s.CntrctNum = c.CntrctNum 
	where c.CntrctNum = @ContractNumber
	and y.

	 group by c.CntrctNum, c.Contractor_Name, c.Dates_Effective, c.Dates_CntrctExp, c.POC_Primary_Name, c.POC_Primary_Phone,
		c.POC_Primary_Email, c.Primary_Address_1, c.Primary_Address_2, c.Primary_City, c.Primary_State, c.Primary_Zip, c.CO_ID, u.FullName, c.Schedule_Number, t.Schedule_Name, t.Division 
	

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales for contract sales report.'
		goto ERROREXIT
	END


	
	select ContractNumber,
		VendorName,
		EffectiveDate,
		ExpirationDate,
		ContractAdministrator,
		ContractAdministratorPhone,
		ContractAdministratorEmail,
		VendorAddress1,
		VendorAddress2,
		VendorCity,
		VendorStateCode,
		VendorZip,
		COId,
		ScheduleNumber,
		ScheduleName,
		DivisionId,

		VASales,
		OGASales,
		SLGSales,
		TotalSales
	
		from #SalesSummaryReport
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales summary report results.'
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


