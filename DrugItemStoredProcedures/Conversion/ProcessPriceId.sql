IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessPriceId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessPriceId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[ProcessPriceId]
As

Declare @drugItemId int,
		@drugItemPriceId int,
		@priceStartDate datetime,
		@priceStopDate datetime,
		@priceid int
	

	Declare DrugItemId_Cursor CURSOR For
		Select Distinct DrugItemId
		From Di_DrugItemPrice
		

	Open DrugItemId_Cursor
	FETCH NEXT FROM DrugItemId_Cursor
	INTO @drugItemId

	WHILE @@FETCH_STATUS = 0
	BEGIN
-- FSS Cursor
		Set @priceid = null
		Declare FSSItemPriceId_Cursor CURSOR For
			Select DrugItemPriceId,PriceStartDate,PriceStopDate
			From Di_DrugItemPrice
			Where DrugItemId = @drugItemId
			And IsFSS = 1
			Order by 2,3

		Open FSSItemPriceId_Cursor
		FETCH NEXT FROM FSSItemPriceId_Cursor
		INTO @drugItemPriceId,@priceStartDate,@priceStopDate

		WHILE @@FETCH_STATUS = 0
		BEGIN	
			Select @priceid =
						Case 
							when max(PriceId) is null then 1
							else max(PriceId)+1
						  End
			From Di_DrugItemPrice
			Where DrugItemId = @drugItemId
			And IsFSS = 1


			Update Di_DrugItemPrice
			Set PriceId = @priceid
			Where DrugItemPriceId = @drugItemPriceId
	
			FETCH NEXT FROM FSSItemPriceId_Cursor
			INTO @drugItemPriceId,@priceStartDate,@priceStopDate
		End
		Close FSSItemPriceId_Cursor
		DeAllocate FSSItemPriceId_Cursor


		Set @priceid = null

-- FCP Cursor
		Declare FCPItemPriceId_Cursor CURSOR For
			Select DrugItemPriceId,PriceStartDate,PriceStopDate
			From Di_DrugItemPrice 
			Where DrugItemId = @drugItemId
			And IsBig4 = 1
			Order by 2,3

		Open FCPItemPriceId_Cursor
		FETCH NEXT FROM FCPItemPriceId_Cursor
		INTO @drugItemPriceId,@priceStartDate,@priceStopDate

		WHILE @@FETCH_STATUS = 0
		BEGIN	
			Select @priceid =
						Case 
							when max(PriceId) is null then 1
							else max(PriceId)+1
						  End
			From Di_DrugItemPrice
			Where DrugItemId = @drugItemId
			And IsBig4 = 1


			Update Di_DrugItemPrice
			Set PriceId = @priceid
			Where DrugItemPriceId = @drugItemPriceId

			FETCH NEXT FROM FCPItemPriceId_Cursor
			INTO @drugItemPriceId,@priceStartDate,@priceStopDate
		End
		Close FCPItemPriceId_Cursor
		DeAllocate FCPItemPriceId_Cursor

-- FSSR Cursor
		Set @priceid = null
		Declare FSSRItemPriceId_Cursor CURSOR For
			Select DrugItemPriceId,PriceStartDate,PriceStopDate
			From Di_DrugItemPrice 
			Where DrugItemId = @drugItemId
			And IsFSS = 0 
			And IsBig4 = 0
			Order by 2,3

		Open FSSRItemPriceId_Cursor
		FETCH NEXT FROM FSSRItemPriceId_Cursor
		INTO @drugItemPriceId,@priceStartDate,@priceStopDate

		WHILE @@FETCH_STATUS = 0
		BEGIN	
			Select @priceid =
						Case 
							when max(PriceId) is null then 1
							else max(PriceId)+1
						  End
			From Di_DrugItemPrice
			Where DrugItemId = @drugItemId
			And IsFSS = 0
			And IsBig4 = 0


			Update Di_DrugItemPrice
			Set PriceId = @priceid
			Where DrugItemPriceId = @drugItemPriceId

			FETCH NEXT FROM FSSRItemPriceId_Cursor
			INTO @drugItemPriceId,@priceStartDate,@priceStopDate
		End
		Close FSSRItemPriceId_Cursor
		DeAllocate FSSRItemPriceId_Cursor


		FETCH NEXT FROM DrugItemId_Cursor
		INTO @drugItemId
	End
	Close DrugItemId_Cursor
	DeAllocate DrugItemId_Cursor

	