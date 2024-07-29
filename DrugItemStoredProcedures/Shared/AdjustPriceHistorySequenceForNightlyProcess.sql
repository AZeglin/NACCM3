IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'AdjustPriceHistorySequenceForNightlyProcess')
	BEGIN
		DROP  Procedure  AdjustPriceHistorySequenceForNightlyProcess
	END

GO

CREATE proc [dbo].[AdjustPriceHistorySequenceForNightlyProcess]
As


Declare @drugitemid int
		
    

	Create table #AdjustPriceHistoryTableNightly
	(
	AdjustPriceTableId int identity(1,1), 
	DrugItemPriceHistoryId int
	)


	Declare FSSHistory_Cursor CURSOR For
		Select Distinct DrugItemId
		From DI_DrugItemPricehistory
		Where IsFSS = 1

	Open FSSHistory_Cursor
	FETCH NEXT FROM FSSHistory_Cursor
	INTO  @drugitemid

	WHILE @@FETCH_STATUS = 0
	BEGIN

		Insert into #AdjustPriceHistoryTableNightly
		(DrugItemPriceHistoryId)
		Select DrugItemPriceHistoryId
		From DI_DrugItemPricehistory
		Where DrugItemId = @DrugItemId 
		And DrugItemSubItemId is null
		And IsFss = 1
		Order by PriceStartDate,PriceStopDate

		Update a
			Set a.PriceId = b.AdjustPriceTableId
		From DI_DrugItemPricehistory a
		Join #AdjustPriceHistoryTableNightly b
		on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId
		
		Truncate table #AdjustPriceHistoryTableNightly
		Dbcc CheckIdent(#AdjustPriceHistoryTableNightly,reseed,1)		
		
		Insert into #AdjustPriceHistoryTableNightly
		(DrugItemPriceHistoryId)
		Select DrugItemPriceHistoryId
		From DI_DrugItemPricehistory
		Where DrugItemId = @DrugItemId 
		And DrugItemSubItemId is not null
		And IsFss = 1
		Order by PriceStartDate,PriceStopDate

		Update a
			Set a.PriceId = b.AdjustPriceTableId
		From DI_DrugItemPricehistory a
		Join #AdjustPriceHistoryTableNightly b
		on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId		
		
		
		FETCH NEXT FROM FSSHistory_Cursor
		INTO  @drugitemid
	End
	Close FSSHistory_Cursor
	DeAllocate FSSHistory_Cursor

	Truncate table #AdjustPriceHistoryTableNightly
	Dbcc CheckIdent(#AdjustPriceHistoryTableNightly,reseed,1)


	Declare Big4History_Cursor CURSOR For
		Select Distinct DrugItemId
		From DI_DrugItemPricehistory
		Where Isbig4 = 1

	Open Big4History_Cursor
	FETCH NEXT FROM Big4History_Cursor
	INTO  @drugitemid

	WHILE @@FETCH_STATUS = 0
	BEGIN

		Insert into #AdjustPriceHistoryTableNightly
		(DrugItemPriceHistoryId)
		Select DrugItemPriceHistoryId
		From DI_DrugItemPricehistory
		Where DrugItemId = @DrugItemId 
		And DrugItemSubItemId is  null		
		And Isbig4 = 1
		Order by PriceStartDate,PriceStopDate

		Update a
			Set a.PriceId = b.AdjustPriceTableId
		From DI_DrugItemPricehistory a
		Join #AdjustPriceHistoryTableNightly b
		on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId
		
		Truncate table #AdjustPriceHistoryTableNightly
		Dbcc CheckIdent(#AdjustPriceHistoryTableNightly,reseed,1)
		
		Insert into #AdjustPriceHistoryTableNightly
		(DrugItemPriceHistoryId)
		Select DrugItemPriceHistoryId
		From DI_DrugItemPricehistory
		Where DrugItemId = @DrugItemId 
		And DrugItemSubItemId is not null		
		And Isbig4 = 1
		Order by PriceStartDate,PriceStopDate

		Update a
			Set a.PriceId = b.AdjustPriceTableId
		From DI_DrugItemPricehistory a
		Join #AdjustPriceHistoryTableNightly b
		on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId		
		
		FETCH NEXT FROM Big4History_Cursor
		INTO  @drugitemid
	End
	Close Big4History_Cursor
	DeAllocate Big4History_Cursor

	Truncate table #AdjustPriceHistoryTableNightly
	Dbcc CheckIdent(#AdjustPriceHistoryTableNightly,reseed,1)


	Declare RestrictedHistory_Cursor CURSOR For
		Select Distinct DrugItemId
		From DI_DrugItemPricehistory
		Where Isbig4 = 0 and IsFSS = 0

	Open RestrictedHistory_Cursor
	FETCH NEXT FROM RestrictedHistory_Cursor
	INTO  @drugitemid

	WHILE @@FETCH_STATUS = 0
	BEGIN

		Insert into #AdjustPriceHistoryTableNightly
		(DrugItemPriceHistoryId)
		Select DrugItemPriceHistoryId
		From DI_DrugItemPricehistory
		Where DrugItemId = @DrugItemId 
		And Isbig4 = 0 and IsFSS = 0
		And DrugItemSubItemId is  null			
		Order by PriceStartDate,PriceStopDate

		Update a
			Set a.PriceId = b.AdjustPriceTableId
		From DI_DrugItemPricehistory a
		Join #AdjustPriceHistoryTableNightly b
		on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId


		Truncate table #AdjustPriceHistoryTableNightly
		Dbcc CheckIdent(#AdjustPriceHistoryTableNightly,reseed,1)
		
		Insert into #AdjustPriceHistoryTableNightly
		(DrugItemPriceHistoryId)
		Select DrugItemPriceHistoryId
		From DI_DrugItemPricehistory
		Where DrugItemId = @DrugItemId 
		And Isbig4 = 0 and IsFSS = 0
		And DrugItemSubItemId is not  null			
		Order by PriceStartDate,PriceStopDate

		Update a
			Set a.PriceId = b.AdjustPriceTableId
		From DI_DrugItemPricehistory a
		Join #AdjustPriceHistoryTableNightly b
		on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId
			
		
		FETCH NEXT FROM RestrictedHistory_Cursor
		INTO  @drugitemid
	End
	Close RestrictedHistory_Cursor
	DeAllocate RestrictedHistory_Cursor

	Drop table #AdjustPriceHistoryTableNightly

		