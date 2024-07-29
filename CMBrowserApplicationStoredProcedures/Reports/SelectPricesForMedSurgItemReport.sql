IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectPricesForMedSurgItemReport')
BEGIN
	DROP  Procedure  SelectPricesForMedSurgItemReport
END

GO

CREATE Procedure SelectPricesForMedSurgItemReport
(
@ItemId int,
@IncludeHistory bit = 0
)
AS

BEGIN

	if @IncludeHistory = 0
	BEGIN
		select 0 as IsFromHistory, ItemPriceId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms
		from CM_ItemPrice
		where ItemId = @ItemId
		order by PriceStartDate

	END
	else
	BEGIN

		select  0 as IsFromHistory, ItemPriceId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms
		from CM_ItemPrice
		where ItemId = @ItemId

		union

		select  1 as IsFromHistory, ItemPriceId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms
		from CM_ItemPriceHistory
		where ItemId = @ItemId

		order by PriceId
	END
END

