IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSINItemCountsPerContract' )
BEGIN
	DROP PROCEDURE SelectSINItemCountsPerContract
END
GO

CREATE PROCEDURE [dbo].[SelectSINItemCountsPerContract]
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@Division int,   
@ScheduleNumber int,   /* -1 for all */
@DrugItemServerName nvarchar(255),
@DrugItemDatabaseName nvarchar(255),
@Socio_Business_Size_ID int, /* 1(small) or 2(large) */
@Socio_VetStatus_ID int, /* 0(not a vet),1(vet), or 3(disabled vet) */
@Socio_Woman bit, /* 0 or 1 */
@Socio_SDB bit, /* 0 or 1(small disadvantaged bus) */
@Socio_8a bit, /* 0 or 1 */
@Socio_HubZone bit /* 0 or 1 */
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@SQL nvarchar(3000)

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'SIN Item Counts Per Contract Report', '2'

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	IF OBJECT_ID('tempdb..#SINItemCounts') IS NOT NULL 
	BEGIN
		drop table #SINItemCounts
	
		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error dropping #SINItemCounts temp table.'
			goto ERROREXIT
		END
	END

	create table #SINItemCounts 
	(
		ContractNumber nvarchar(20) NOT NULL, 
		ContractId int NOT NULL,
		ScheduleNumber int NOT NULL,
		Division int NOT NULL,
		[SIN] nvarchar(10) NOT NULL,
		ItemCount int NULL,
		VendorName nvarchar(75) NULL,
		ScheduleType nvarchar(50) NOT NULL,
		IsDrugItem bit NULL,
		Socio_Business_Size_ID int NOT NULL,
		Socio_VetStatus_ID int NOT NULL,
		Socio_Woman bit NOT NULL,
		Socio_SDB bit NOT NULL,
		Socio_8a bit NOT NULL,
		Socio_HubZone bit NOT NULL
	)

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table'
		goto ERROREXIT
	END

	create table #SINContractCounts 
	(
		[SIN] nvarchar(10) NOT NULL,
		ContractCount int NULL
	)

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table 2'
		goto ERROREXIT
	END

	/* all schedules for the selected division */
	if @ScheduleNumber = -1
	BEGIN
		insert into #SINItemCounts
		( ContractNumber, ContractId, ScheduleNumber, Division, ScheduleType, [SIN], VendorName, IsDrugItem, Socio_Business_Size_ID, Socio_VetStatus_ID, Socio_Woman, Socio_SDB, Socio_8a, Socio_HubZone )
		select c.CntrctNum, c.Contract_Record_ID, c.Schedule_Number, s.Division, s.[Type], n.[SINs], c.Contractor_Name, 0, c.Socio_Business_Size_ID, c.Socio_VetStatus_ID, c.Socio_Woman, c.Socio_SDB, c.Socio_8a, c.Socio_HubZone
		from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		join tbl_Cntrcts_SINs n on n.CntrctNum = c.CntrctNum
		where dbo.IsContractActiveFunction( c.CntrctNum, GETDATE() ) = 1
		and s.Division = @Division
		and n.Inactive = 0
	END
	else /* a particular schedule */
	BEGIN 
		insert into #SINItemCounts
		( ContractNumber, ContractId, ScheduleNumber, Division, ScheduleType, [SIN], VendorName, IsDrugItem, Socio_Business_Size_ID, Socio_VetStatus_ID, Socio_Woman, Socio_SDB, Socio_8a, Socio_HubZone )
		select c.CntrctNum, c.Contract_Record_ID, c.Schedule_Number, s.Division, s.[Type], n.[SINs], c.Contractor_Name, 0, c.Socio_Business_Size_ID, c.Socio_VetStatus_ID, c.Socio_Woman, c.Socio_SDB, c.Socio_8a, c.Socio_HubZone
		from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		join tbl_Cntrcts_SINs n on n.CntrctNum = c.CntrctNum
		where dbo.IsContractActiveFunction( c.CntrctNum, GETDATE() ) = 1
		and s.Schedule_Number = @ScheduleNumber
		and n.Inactive = 0
	END

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting active contracts and SINs'
		goto ERROREXIT
	END		

	/* id drug items */
	update #SINItemCounts
	set IsDrugItem = 1
	where [SIN] like '42-2%'

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error updating #SINItemCounts IsDrugItem flag'
		goto ERROREXIT
	END		

	/* med surg counts  ( in new schema, include BPA ) */
	update #SINItemCounts 
	set ItemCount = ( select COUNT(*) 
						from CM_Items i join CM_ItemPrice p on i.ItemId = p.ItemId 
							join #SINItemCounts t on i.ContractId = t.ContractId and i.[SIN] = t.[SIN] 
							where t.ContractId = x.ContractId
							and t.[SIN] = x.[SIN]							
							and getdate() between p.PriceStartDate and p.PriceStopDate )
	from #SINItemCounts x 
	where  x.IsDrugItem = 0
	and x.ScheduleType <> 'BPA'

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting med/surg items'
		goto ERROREXIT
	END		

	--/* bpa counts */
	--update #SINItemCounts 
	--set ItemCount = ( select COUNT(*) 
	--					from tbl_BPA_Pricelist p join #SINItemCounts t on p.CntrctNum = t.ContractNumber 
	--						join tbl_Pricelist f on p.FSSLogNumber = f.LogNumber 
	--						where t.ContractNumber = x.ContractNumber
	--						and t.[SIN] = x.[SIN]
	--						and f.[SIN] = t.[SIN]
	--						and p.Removed = 0
	--						and f.Removed = 0
	--						and getdate() between p.DateEffective and p.ExpirationDate )
	--from #SINItemCounts x 
	--where x.IsDrugItem = 0
	--and x.ScheduleType = 'BPA'

	--select @error = @@error

	--if @error <> 0
	--BEGIN
	--	select @errorMsg = 'Error counting BPA items'
	--	goto ERROREXIT
	--END		

	/* covered pharm item counts */
	select @SQL = N'update #SINItemCounts 
	set ItemCount = ( select COUNT(*) 
						from [' + @DrugItemServerName + '].[' + @DrugItemDatabaseName + '].dbo.DI_DrugItems i join DrugItem.dbo.DI_Contracts c on i.ContractId = c.ContractId
						join #SINItemCounts t on t.ContractNumber = c.NACCMContractNumber
						
						where t.ContractNumber = x.ContractNumber
						and t.[SIN] = x.[SIN]
						and  ( i.DiscontinuationDate is null or DATEDIFF( d, getdate(), i.DiscontinuationDate ) >= 0 )
						and exists ( select  p.Price from [' + @DrugItemServerName + '].[' + @DrugItemDatabaseName + '].dbo.DI_DrugItemPrice p 
										where p.DrugItemId = i.DrugItemId
										and GETDATE() between p.PriceStartDate and p.PriceStopDate ) 
						and i.Covered = ''T'' )

	from #SINItemCounts x 
	where x.[SIN] = ''42-2A''
	and x.IsDrugItem = 1 '
	
	Exec SP_executeSQL @SQL
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting covered pharm items'
		goto ERROREXIT
	END		
	
	
	/* non-covered pharm item counts */
	select @SQL = N'update #SINItemCounts 
	set ItemCount = ( select COUNT(*) 
						from [' + @DrugItemServerName + '].[' + @DrugItemDatabaseName + '].dbo.DI_DrugItems i join DrugItem.dbo.DI_Contracts c on i.ContractId = c.ContractId
						join #SINItemCounts t on t.ContractNumber = c.NACCMContractNumber
						
						where t.ContractNumber = x.ContractNumber
						and t.[SIN] = x.[SIN]
						and  ( i.DiscontinuationDate is null or DATEDIFF( d, getdate(), i.DiscontinuationDate ) >= 0 )
						and exists ( select p.Price from [' + @DrugItemServerName + '].[' + @DrugItemDatabaseName + '].dbo.DI_DrugItemPrice p 
										where p.DrugItemId = i.DrugItemId
										and GETDATE() between p.PriceStartDate and p.PriceStopDate ) 
						and i.Covered = ''F'' )

	from #SINItemCounts x 
	where x.[SIN] = ''42-2B''
	and x.IsDrugItem = 1 '
	
	Exec SP_executeSQL @SQL
		
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting non-covered pharm items'
		goto ERROREXIT
	END		

	/* contract counts */
	insert into #SINContractCounts
	( [SIN] )
	select distinct [SIN] from #SINItemCounts

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error adding SINs to contract count temp table'
		goto ERROREXIT
	END		

	if @Socio_Business_Size_ID = 2
	BEGIN
	update #SINContractCounts
	set ContractCount = ( select COUNT(*) from #SINItemCounts t where t.[SIN] = c.[SIN] and t.Socio_Business_Size_ID = @Socio_Business_Size_ID )
	from #SINContractCounts c
	END
	if @Socio_Business_Size_ID = 1
	BEGIN
	update #SINContractCounts
	set ContractCount = ( select COUNT(*) from #SINItemCounts t where t.[SIN] = c.[SIN] and t.Socio_Business_Size_ID = @Socio_Business_Size_ID 
	and t.Socio_VetStatus_ID = @Socio_VetStatus_ID and t.Socio_Woman = @Socio_Woman 
	and t.Socio_SDB = @Socio_SDB and t.Socio_8a = @Socio_8a and t.Socio_HubZone = @Socio_HubZone)
	from #SINContractCounts c
	END

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting contracts for temp table'
		goto ERROREXIT
	END		

	if @Socio_Business_Size_ID = 2
	select 	t.Socio_Business_Size_ID,
		t.Socio_VetStatus_ID,
		t.Socio_Woman,
		t.Socio_SDB,
		t.Socio_8a,
		t.Socio_HubZone,
		t.Division,
		s.Division_Description,
		t.ScheduleNumber,
		s.Schedule_Name,
		t.ScheduleType,
		t.[SIN],
		sum(t.ItemCount) as ItemCount,
		( select ContractCount from #SINContractCounts where [SIN] = t.[SIN]) as ContractCount
	from #SINItemCounts t join [tlkup_Sched/Cat] s on t.ScheduleNumber = s.Schedule_Number
	where t.Socio_Business_Size_ID =2
	group by t.Socio_Business_Size_ID,
		t.Socio_VetStatus_ID,
		t.Socio_Woman,
		t.Socio_SDB,
		t.Socio_8a,
		t.Socio_HubZone,t.Division, s.Division_Description, t.ScheduleNumber, s.Schedule_Name, t.ScheduleType, t.[SIN]
	order by s.Schedule_Name, t.[SIN]

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting temptable for report'
		goto ERROREXIT
	END
	else
	BEGIN
	if @Socio_Business_Size_ID = 1
	select 	t.Socio_Business_Size_ID,
		t.Socio_VetStatus_ID,
		t.Socio_Woman,
		t.Socio_SDB,
		t.Socio_8a,
		t.Socio_HubZone,
		t.Division,
		s.Division_Description,
		t.ScheduleNumber,
		s.Schedule_Name,
		t.ScheduleType,
		t.[SIN],
		sum(t.ItemCount) as ItemCount,
		( select ContractCount from #SINContractCounts where [SIN] = t.[SIN] ) as ContractCount
	from #SINItemCounts t join [tlkup_Sched/Cat] s on t.ScheduleNumber = s.Schedule_Number
	where t.Socio_Business_Size_ID = @Socio_Business_Size_ID and t.Socio_VetStatus_ID = @Socio_VetStatus_ID and t.Socio_Woman = @Socio_Woman 
	and t.Socio_SDB = @Socio_SDB and t.Socio_8a = @Socio_8a and t.Socio_HubZone = @Socio_HubZone
	group by t.Socio_Business_Size_ID,
		t.Socio_VetStatus_ID,
		t.Socio_Woman,
		t.Socio_SDB,
		t.Socio_8a,
		t.Socio_HubZone,t.Division, s.Division_Description, t.ScheduleNumber, s.Schedule_Name, t.ScheduleType, t.[SIN]
	order by s.Schedule_Name, t.[SIN]

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting temptable for report'
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

ENDEXIT:

