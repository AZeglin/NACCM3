IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectActiveItemsForNDCReport')
	BEGIN
		DROP  Procedure  SelectActiveItemsForNDCReport
	END

GO

CREATE Procedure SelectActiveItemsForNDCReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@FdaAssignedLabelerCode char(5),
@ProductCode char(4),
@PackageCode char(2)
)

As


DECLARE 	@error int,
			@errorMsg nvarchar(250),
			@SQL nvarchar(2400),
			@SQLParms nvarchar(1000)

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertDrugItemUserActivity @ReportUserLoginId, 'R', 'Active Items For NDC Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#ActiveItemsForNDCReport' ) 
	BEGIN
		DROP TABLE #ActiveItemsForNDCReport
	END
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error dropping #ActiveItemsForNDCReport temp table'
		goto ERROREXIT
	END
	
	create table #ActiveItemsForNDCReport
	(
		Contract_Record_ID int NULL,
		CntrctNum nvarchar(50) NULL,
		Schedule_Name nvarchar(75) NULL,
		Schedule_Number int NULL,
		Division int NULL,
		Division_Description nvarchar(50) NULL,
		FullName nvarchar(50) NULL, 
		CO_ID int NULL,
		Contractor_Name nvarchar(75) NULL,
		DUNS nvarchar(9) NULL,
		PV_Participation bit NULL,
		Drug_Covered nvarchar(50) NULL,
		Dates_CntrctAward datetime NULL,
		Dates_Effective datetime NULL,
		Dates_CntrctExp datetime NULL,
		Dates_Completion datetime NULL,
		BPA_FSS_Counterpart nvarchar(20) NULL,
		Offer_ID int NULL,
		Type nvarchar(50) NULL
	)
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #ActiveItemsForNDCReport temp table'
		goto ERROREXIT
	END

	select @SQL = N'insert into #ActiveItemsForNDCReport
	(
		Contract_Record_ID,
		CntrctNum,
		Schedule_Name,
		Schedule_Number,
		Division,
		Division_Description,
		FullName, 
		CO_ID,
		Contractor_Name,
		DUNS,
		PV_Participation,
		Drug_Covered,
		Dates_CntrctAward,
		Dates_Effective,
		Dates_CntrctExp,
		Dates_Completion,
		BPA_FSS_Counterpart,
		Offer_ID,
		Type
	)
	
	SELECT  c.Contract_Record_ID, 
		c.CntrctNum, 
		s.Schedule_Name, 
		s.Schedule_Number, 
		s.Division,
		s.Division_Description,
		u.FullName, 
		c.CO_ID, 
		c.Contractor_Name, 
		c.DUNS, 
		c.PV_Participation,
		c.Drug_Covered, 
		c.Dates_CntrctAward, 
		c.Dates_Effective, 
		c.Dates_CntrctExp, 
		c.Dates_Completion,
		c.BPA_FSS_Counterpart,
		c.Offer_ID,
		s.Type
	FROM [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].[dbo].[tbl_Cntrcts] c join [' + @SecurityServerName + '].[' + @SecurityDatabaseName + '].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
		join [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].[dbo].[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number
	where c.CntrctNum in ( select c.NACCMContractNumber
							from DI_DrugItems i, DI_Contracts c
							where i.DrugItemNDCId in ( select DrugItemNDCId 
									from DI_DrugItemNDC
									where FdaAssignedLabelerCode = @FdaAssignedLabelerCode_parm
									and ProductCode = @ProductCode_parm
									and PackageCode = @PackageCode_parm )
							and i.ContractId = c.ContractId )'
							
	select @SQLParms = N'@FdaAssignedLabelerCode_parm char(5), @ProductCode_parm char(4), @PackageCode_parm char(2)'
	
	Exec SP_executeSQL @SQL, @SQLParms, @FdaAssignedLabelerCode_parm = @FdaAssignedLabelerCode, @ProductCode_parm = @ProductCode, @PackageCode_parm = @PackageCode
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving contract info for active item report'
		goto ERROREXIT
	END

	SELECT  t.Contract_Record_ID, 
		t.CntrctNum, 
		t.Schedule_Name, 
		t.Schedule_Number, 
		t.Division,
		t.Division_Description,
		t.FullName, 
		t.CO_ID, 
		t.Contractor_Name, 
		t.DUNS, 
		t.PV_Participation,
		t.Drug_Covered, 
		t.Dates_CntrctAward, 
		t.Dates_Effective, 
		t.Dates_CntrctExp, 
		t.Dates_Completion,
		t.BPA_FSS_Counterpart,
		t.Offer_ID,
		t.Type,
		i.DrugItemId,
		i.PackageDescription,
		i.Generic,
		i.TradeName,
		i.DiscontinuationDate,
		i.Covered,
		i.PrimeVendor,
		i.DispensingUnit,
		i.ParentDrugItemId,
		i.LastModifiedBy,
		i.LastModificationDate
	from #ActiveItemsForNDCReport t join DI_Contracts c on t.CntrctNum = c.NACCMContractNumber
		join DI_DrugItems i on i.ContractId = c.ContractId
	where i.DrugItemNDCId in ( select DrugItemNDCId 
								from DI_DrugItemNDC
								where FdaAssignedLabelerCode = @FdaAssignedLabelerCode
								and ProductCode = @ProductCode
								and PackageCode = @PackageCode )
		
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving active item info for item ' + @FdaAssignedLabelerCode + ' ' + @ProductCode + ' ' + @PackageCode
		goto ERROREXIT
	END
	
								
	delete #ActiveItemsForNDCReport

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



