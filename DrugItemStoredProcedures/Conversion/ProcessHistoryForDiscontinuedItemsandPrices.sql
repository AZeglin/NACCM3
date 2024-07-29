IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessHistoryForDiscontinuedItemsandPrices]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessHistoryForDiscontinuedItemsandPrices]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[ProcessHistoryForDiscontinuedItemsandPrices]
As

Declare @error int,
		@errorMsg nvarchar(512),
		@contractId int,
		@drugItemId int
		

	Declare Items_Cursor CURSOR For
		Select DrugItemId
		From Di_DrugItems
		Where discontinuationdate is not null

	Open Items_Cursor
	FETCH NEXT FROM Items_Cursor
	INTO @drugItemId

	WHILE @@FETCH_STATUS = 0
	BEGIN		

		Insert into DI_DrugItemPriceHistory
		(DrugItemPriceId,DrugItemId,DrugItemSubItemId,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,IsFSS,IsBIG4,
		 IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,
		 IsUSCG,AwardedFSSTrackingCustomerRatio,TrackingCustomerName,
		 CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
		 ModificationStatusId,Notes,CreatedBy,
		 CreationDate,LastModifiedBy,LastModificationDate)
		Select 
			DrugItemPriceId,DrugItemId,DrugItemSubItemId,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,IsFSS,
			IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,IsSVH1,IsSVH2,
			IsTMOP,IsUSCG,AwardedFSSTrackingCustomerRatio,TrackingCustomerName,
			CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,
			ModificationStatusId,'ProcessHistoryForDiscontinuedItemsandPrices',
			CreatedBy,CreationDate,LastModifiedBy,getdate()
		From di_Drugitemprice
		where drugitemId = @drugItemId

		delete from di_Drugitemprice
		where drugitemId = @drugItemId


		FETCH NEXT FROM Items_Cursor
		INTO @drugItemId
	End
	Close Items_Cursor
	DeAllocate Items_Cursor


