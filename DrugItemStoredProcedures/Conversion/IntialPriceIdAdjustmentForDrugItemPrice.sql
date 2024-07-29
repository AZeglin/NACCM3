IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[IntialPriceIdAdjustmentForDrugItemPrice]') AND type in (N'P', N'PC'))
DROP PROCEDURE [IntialPriceIdAdjustmentForDrugItemPrice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[IntialPriceIdAdjustmentForDrugItemPrice]
As
	Declare @drugitemid int,@drugItemSubItemId int

	Create table #drugItemPrice
	(tempId int identity(1,1),
	DrugItemPriceId int,
	DrugItemId int, 
	)


	Declare DrugItems_Cursor Cursor For
		Select distinct DrugitemId
		From Di_DrugItemPrice
		where drugitemsubitemid is null
	Open DrugItems_Cursor
	Fetch Next From DrugItems_Cursor
	INTO @drugitemId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Insert into #drugItemPrice
		(DrugItemPriceId,DrugItemId)
		Select 
		 DrugItemPriceId,DrugItemId
		From Di_DrugItemPrice
		Where DrugItemId = @drugitemid
		And DrugItemSubItemId is null
		And ISFSS = 1
		Order by PriceStartDate,PriceStopDate
		
		Update a
			set a.priceId = b.tempId
		From Di_DrugItemPrice a
		Join #drugItemPrice b
		on a.drugitempriceId = b.drugitempriceid

		Truncate table #drugItemPrice
		dbcc checkident(#drugItemPrice,reseed,1)

		Insert into #drugItemPrice
		(DrugItemPriceId,DrugItemId)
		Select 
		 DrugItemPriceId,DrugItemId
		From Di_DrugItemPrice
		Where DrugItemId = @drugitemid
		And DrugItemSubItemId is null
		And ISBIG4 = 1
		Order by PriceStartDate,PriceStopDate

		Update a
			set a.priceId = b.tempId
		From Di_DrugItemPrice a
		Join #drugItemPrice b
		on a.drugitempriceId = b.drugitempriceid

		Truncate table #drugItemPrice
		dbcc checkident(#drugItemPrice,reseed,1)


		Insert into #drugItemPrice
		(DrugItemPriceId,DrugItemId)
		Select 
		 DrugItemPriceId,DrugItemId
		From Di_DrugItemPrice
		Where DrugItemId = @drugitemid
		And DrugItemSubItemId is null
		And ISFSS = 0
		And ISBIG4 = 0
		Order by PriceStartDate,PriceStopDate

		Update a
			set a.priceId = b.tempId
		From Di_DrugItemPrice a
		Join #drugItemPrice b
		on a.drugitempriceId = b.drugitempriceid

		Truncate table #drugItemPrice
		dbcc checkident(#drugItemPrice,reseed,1)


		Fetch Next From DrugItems_Cursor
		INTO @drugitemId
	End
	Close DrugItems_Cursor
	DeAllocate DrugItems_Cursor



	Declare DrugItems_Cursor Cursor For
		Select distinct DrugitemId,DrugItemSubItemId
		From Di_DrugItemPrice
		where drugitemsubitemid is not null
	Open DrugItems_Cursor
	Fetch Next From DrugItems_Cursor
	INTO @drugitemId, @drugItemSubItemId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Insert into #drugItemPrice
		(DrugItemPriceId,DrugItemId)
		Select 
		 DrugItemPriceId,DrugItemId
		From Di_DrugItemPrice
		Where DrugItemId = @drugitemid
		And DrugItemSubItemId = @drugItemSubItemId
		And ISFSS = 1
		Order by PriceStartDate,PriceStopDate
		
		Update a
			set a.priceId = b.tempId
		From Di_DrugItemPrice a
		Join #drugItemPrice b
		on a.drugitempriceId = b.drugitempriceid

		Truncate table #drugItemPrice
		dbcc checkident(#drugItemPrice,reseed,1)

		Insert into #drugItemPrice
		(DrugItemPriceId,DrugItemId)
		Select 
		 DrugItemPriceId,DrugItemId
		From Di_DrugItemPrice
		Where DrugItemId = @drugitemid
		And DrugItemSubItemId = @drugItemSubItemId
		And ISBIG4 = 1
		Order by PriceStartDate,PriceStopDate

		Update a
			set a.priceId = b.tempId
		From Di_DrugItemPrice a
		Join #drugItemPrice b
		on a.drugitempriceId = b.drugitempriceid

		Truncate table #drugItemPrice
		dbcc checkident(#drugItemPrice,reseed,1)


		Insert into #drugItemPrice
		(DrugItemPriceId,DrugItemId)
		Select 
		 DrugItemPriceId,DrugItemId
		From Di_DrugItemPrice
		Where DrugItemId = @drugitemid
		And DrugItemSubItemId = @drugItemSubItemId
		And ISFSS = 0
		And ISBIG4 = 0
		Order by PriceStartDate,PriceStopDate

		Update a
			set a.priceId = b.tempId
		From Di_DrugItemPrice a
		Join #drugItemPrice b
		on a.drugitempriceId = b.drugitempriceid

		Truncate table #drugItemPrice
		dbcc checkident(#drugItemPrice,reseed,1)


		Fetch Next From DrugItems_Cursor
		INTO @drugitemId,@drugItemSubItemId
	End
	Close DrugItems_Cursor
	DeAllocate DrugItems_Cursor

	Drop table #drugItemPrice
