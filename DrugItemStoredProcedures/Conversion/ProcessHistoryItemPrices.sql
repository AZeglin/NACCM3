IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessHistoryItemPrices]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessHistoryItemPrices]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[ProcessHistoryItemPrices]
As

Declare @error int,
		@errorMsg nvarchar(512)


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
		ModificationStatusId,'ProcessHistoryItemPrices',
		'Re Extract',CreationDate,LastModifiedBy,getdate()
	from di_Drugitemprice
	where pricestopdate < getdate()
	order by 2,8,9,4,5

	delete from di_Drugitemprice
	where pricestopdate < getdate()
