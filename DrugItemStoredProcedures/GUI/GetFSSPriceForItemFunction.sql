IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetFSSPriceForItemFunction')
	BEGIN
		DROP  Function  GetFSSPriceForItemFunction
	END

GO

CREATE Function GetFSSPriceForItemFunction
(
@DrugItemId int,
@ParentDrugItemId int,
@IsBPA bit
)

Returns decimal(9,2)

AS

BEGIN

	DECLARE @price decimal(9,2), 
		@priceDate datetime,
		@month int,
		@year int
	
	-- works differently during public law period
	select @month = MONTH(getdate())
	if @month = 10 or @month = 11 or @month = 12
	BEGIN
		select @year = YEAR(getdate()) + 1
		select @priceDate = CONVERT( datetime, '1/2/' + convert( varchar(4), @year ))
	END
	else
	BEGIN
		select @priceDate = getdate()
	END

	if @IsBPA = 1
	BEGIN
		select @price = p.Price
		from  DI_DrugItemPrice p
			where p.DrugItemId = @ParentDrugItemId
				and p.IsFSS = 1
				and p.IsTemporary = 0
				and @priceDate between p.PriceStartDate and p.PriceStopDate 
	END
	else
	BEGIN
		select @price = p.Price
		from  DI_DrugItemPrice p
			where p.DrugItemId = @DrugItemId
				and p.IsFSS = 1
				and p.IsTemporary = 0
				and @priceDate between p.PriceStartDate and p.PriceStopDate 
	END

return @price

END


