IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessHistoryForExpiredContractsItemsPackageandPrices]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessHistoryForExpiredContractsItemsPackageandPrices]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[ProcessHistoryForExpiredContractsItemsPackageandPrices]
As

Declare @error int,
		@errorMsg nvarchar(512),
		@contractId int,
		@drugItemId int
		

	Declare Contracts_Cursor CURSOR For
		Select ContractId
		From Di_Contracts
		Where NACCMContractId is null

	Open Contracts_Cursor
	FETCH NEXT FROM Contracts_Cursor
	INTO @contractId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Declare Items_Cursor CURSOR For
			Select DrugItemId
			From Di_DrugItems
			Where ContractID = @contractId

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
				ModificationStatusId,'ProcessHistoryForExpiredContractsItemsPackageandPrices',
				CreatedBy,CreationDate,LastModifiedBy,getdate()
			From di_Drugitemprice
			where drugitemId = @drugItemId
			And PriceStopDate < GETDATE()

			delete from di_Drugitemprice
			where drugitemId = @drugItemId
			And PriceStopDate < GETDATE()

			FETCH NEXT FROM Items_Cursor
			INTO @drugItemId
		End
		Close Items_Cursor
		DeAllocate Items_Cursor


		FETCH NEXT FROM Contracts_Cursor
		INTO @contractId
	End
	Close Contracts_Cursor
	DeAllocate Contracts_Cursor


