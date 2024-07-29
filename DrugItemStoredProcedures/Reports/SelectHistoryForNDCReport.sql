IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectHistoryForNDCReport')
	BEGIN
		DROP  Procedure  SelectHistoryForNDCReport
	END

GO

Create Procedure SelectHistoryForNDCReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@FdaAssignedLabelerCode char(5),
@ProductCode char(4),
@PackageCode char(2),
@Direction char(1)    /* 'B' both, 'F' future, 'P' past */
)

As


DECLARE 	@error int,
			@rowcount int,
			@errorMsg nvarchar(250), 
			@StartingDrugItemNDCId int,
			@retVal int,
			@SQL nvarchar(2400),
			@SQLParms nvarchar(1000)


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertDrugItemUserActivity  @ReportUserLoginId, 'R', 'Select History For NDC Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @StartingDrugItemNDCId = DrugItemNDCId
	from DI_DrugItemNDC
	where FdaAssignedLabelerCode = @FdaAssignedLabelerCode
		and ProductCode = @ProductCode
		and PackageCode = @PackageCode
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'NDC Not found.'
		goto ERROREXIT
	END

	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#NDCInnerTraversalList' ) 
	BEGIN
		DROP TABLE #NDCInnerTraversalList
	END
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error dropping #NDCInnerTraversalList temp table'
		goto ERROREXIT
	END
	
	create table #NDCInnerTraversalList
	(
		[ContractNDCNumberChangeId]                       	int              NOT NULL,
		[OldContractId]                                   	int              NOT NULL,
		[OldDrugItemNDCId]                                	int              NOT NULL,
		[NewContractId]                                   	int              NOT NULL,
		[NewDrugItemNDCId]                                	int              NOT NULL,
		[ChangeStatus]                                    	char(1)              NULL,
		[ModificationId]                                  	int              NOT NULL,
		[EffectiveDate]                                   	datetime             NULL,
		[EndDate]                                         	datetime             NULL,
		[LastModifiedBy]                                  	nvarchar(120)    NOT NULL,
		[LastModificationDate]                            	datetime         NOT NULL,
		[Processed]					bit NULL
	)
		   
		   
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #NDCInnerTraversalList temp table'
		goto ERROREXIT
	END
	
	
	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#NDCTraversalList' ) 
	BEGIN
		DROP TABLE #NDCTraversalList
	END
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error dropping #NDCTraversalList temp table'
		goto ERROREXIT
	END
	
	create table #NDCTraversalList
	(
		[ContractNDCNumberChangeId]                       	int              NOT NULL,
		[ContractId]                                   	int              NOT NULL,
		[DrugItemNDCId]                                	int              NOT NULL,
		[HistoricalNValue]                             	char(1)              NULL,
		[SourceColumn]		char(1) NOT NULL,
		[ChangeStatus]                                    	char(1)              NULL,
		[ModificationId]                                  	int              NOT NULL,
		[EffectiveDate]                                   	datetime             NULL,
		[EndDate]                                         	datetime             NULL,
		[LastModifiedBy]                                  	nvarchar(120)    NOT NULL,
		[LastModificationDate]                            	datetime         NOT NULL,
		[TraversalDirection]			char(1) NOT NULL,
		[Contract_Record_ID]		int NULL,
		[CntrctNum]				nvarchar(50) NULL,
		[Schedule_Name]				nvarchar(75) NULL,
		[Division_Description]	 nvarchar(50) NULL,
		[FullName]				nvarchar(50) NULL, 
		[Contractor_Name]		nvarchar(75) NULL,
		[Dates_CntrctAward]		datetime NULL,
		[Dates_Effective]		datetime NULL,
		[Dates_CntrctExp]		datetime NULL,
		[Dates_Completion]		datetime NULL,
		[Type]					nvarchar(50) NULL
	)
		   
		   
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #NDCTraversalList temp table'
		goto ERROREXIT
	END
	
	/* fill the traversal temp table */
	if @Direction = 'B' or @Direction = 'F'
	BEGIN
		exec @retVal = ForwardTraverseChangeTableForNDCReport @StartingDrugItemNDCId 
	END
		
	if @Direction = 'B' or @Direction = 'P'
	BEGIN
		exec @retVal = BackwardTraverseChangeTableForNDCReport @StartingDrugItemNDCId 
	END
		
	/* backfill the contract data */
	
	select @SQL = N'update #NDCTraversalList 
		set Contract_Record_ID = c.Contract_Record_ID,
		CntrctNum = c.CntrctNum,
		Schedule_Name = s.Schedule_Name,
		Division_Description = s.Division_Description,
		FullName = u.FullName, 
		Contractor_Name = c.Contractor_Name,
		Dates_CntrctAward = c.Dates_CntrctAward,
		Dates_Effective = c.Dates_Effective,
		Dates_CntrctExp = c.Dates_CntrctExp,
		Dates_Completion = c.Dates_Completion,
		Type = s.Type
	
	FROM [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].[dbo].[tbl_Cntrcts] c join [' + @SecurityServerName + '].[' + @SecurityDatabaseName + '].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
		join [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].[dbo].[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number
		join DI_Contracts n ON c.CntrctNum = n.NACCMContractNumber
		join #NDCTraversalList t ON t.ContractId = n.ContractId'
	
	print @SQL

	Exec SP_executeSQL @SQL
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving contract info for NDC history report'
		goto ERROREXIT
	END
	
	/* backfill known contract number for contracts that were not found in NAC_CM */
	
	update #NDCTraversalList 
		set CntrctNum = n.ContractNumber
	FROM DI_Contracts n join #NDCTraversalList t ON n.ContractId = t.ContractId
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving contract number for NDC history report'
		goto ERROREXIT
	END
	
	/* union of active with historical item info */
	
	select t.ContractNDCNumberChangeId,
			t.ContractId ,
			t.DrugItemNDCId ,
			n.FdaAssignedLabelerCode,
			n.ProductCode,
			n.PackageCode,
			t.HistoricalNValue,
			t.SourceColumn,
			t.ChangeStatus,
			t.ModificationId,
			t.EffectiveDate,
			t.EndDate,
			t.LastModifiedBy as ChangeTableLastModifiedBy,
			t.LastModificationDate as ChangeTableLastModificationDate,
			t.TraversalDirection,
			t.Contract_Record_ID, 
			t.CntrctNum, 
			t.Schedule_Name, 
			t.Division_Description,
			t.FullName, 
			t.Contractor_Name, 
			t.Dates_CntrctAward, 
			t.Dates_Effective, 
			t.Dates_CntrctExp, 
			t.Dates_Completion,
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
			i.LastModificationDate,
			'A' as ItemSourceTable
	from #NDCTraversalList t join DI_DrugItems i on i.DrugItemNDCId = t.DrugItemNDCId	
	and i.ContractId = t.ContractId 
	left outer join DI_DrugItemNDC n on n.DrugItemNDCId = t.DrugItemNDCId

	UNION
	
	select t.ContractNDCNumberChangeId,
			t.ContractId ,
			t.DrugItemNDCId ,
			n.FdaAssignedLabelerCode,
			n.ProductCode,
			n.PackageCode,
			t.HistoricalNValue,
			t.SourceColumn,
			t.ChangeStatus,
			t.ModificationId,
			t.EffectiveDate,
			t.EndDate,
			t.LastModifiedBy as ChangeTableLastModifiedBy,
			t.LastModificationDate as ChangeTableLastModificationDate,
			t.TraversalDirection,
			t.Contract_Record_ID, 
			t.CntrctNum, 
			t.Schedule_Name, 
			t.Division_Description,
			t.FullName, 
			t.Contractor_Name, 
			t.Dates_CntrctAward, 
			t.Dates_Effective, 
			t.Dates_CntrctExp, 
			t.Dates_Completion,
			t.Type,
			i.DrugItemId,
			i.PackageDescription,
			i.Generic,
			i.TradeName,
			i.DiscontinuationDate,
			i.Covered,
			i.PrimeVendor,
			i.DispensingUnit,
			-1 as ParentDrugItemId,
			i.LastModifiedBy,
			i.LastModificationDate,
			'H' as ItemSourceTable
	from #NDCTraversalList t left outer join DI_DrugItemsHistory i on i.DrugItemNDCId = t.DrugItemNDCId	
										and i.ContractId = t.ContractId 
	left outer join DI_DrugItemNDC n on n.DrugItemNDCId = t.DrugItemNDCId
	where not exists( select i.DrugItemId 
						from DI_DrugItems i 
						where i.DrugItemNDCId = t.DrugItemNDCId	
							and i.ContractId = t.ContractId )
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error in resultset query for NDC history report'
		goto ERROREXIT
	END

	delete #NDCTraversalList					

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



