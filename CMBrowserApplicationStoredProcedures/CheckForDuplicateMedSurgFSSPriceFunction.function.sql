IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'CheckForDuplicateMedSurgFSSPriceFunction')
	BEGIN
		DROP  Function  CheckForDuplicateMedSurgFSSPriceFunction
	END

GO

CREATE FUNCTION [dbo].[CheckForDuplicateMedSurgFSSPriceFunction]
(
@ContractNumber nvarchar(20),
@ContractorCatalogNumber nvarchar(50), 
@ProductLongDescription nvarchar(800), 
@FSSPrice decimal(18,2), 
@PackageSizePricedOnContract nvarchar(2), 
@SIN nvarchar(50), 
@ServiceDescriptionId int,
@DateEffective datetime, 
@ExpirationDate datetime, 
@LogNumberBeingUpdated int = null, -- this is populated during an update
@ComparePrice bit = 0,
@CompareServiceDescriptionId bit = 0
)

RETURNS bit

AS
BEGIN

	DECLARE @PriceExists int
	
	select @PriceExists = 0

	if @CompareServiceDescriptionId = 1
	BEGIN
		if @ComparePrice = 1
		BEGIN
			if @LogNumberBeingUpdated is null
			BEGIN
				if exists (
				select LogNumber from tbl_pricelist
					where CntrctNum = @ContractNumber
					and LTRIM(RTRIM( [Contractor Catalog Number] )) = LTRIM(RTRIM( @ContractorCatalogNumber ))
					and LTRIM(RTRIM( [Product Long Description] )) = LTRIM(RTRIM( @ProductLongDescription ))
					and [FSS Price] = @FSSPrice
					and [Package Size Priced on Contract] = @PackageSizePricedOnContract
					and [SIN] = @SIN
					and [621I_Category_ID] = @ServiceDescriptionId
	                and((
						datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                    datediff(dd,@DateEffective, [ExpirationDate])>=0
                        )
                        or
                        (
                        datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                        datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[DateEffective])>=0  and 
                        datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                        datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
						))
					and Removed = 0
					)
					BEGIN
						select @PriceExists = 1
					END
			END
			else
			BEGIN
				if exists (
				select LogNumber from tbl_pricelist
					where CntrctNum = @ContractNumber
					and LTRIM(RTRIM( [Contractor Catalog Number] )) = LTRIM(RTRIM( @ContractorCatalogNumber ))
					and LTRIM(RTRIM( [Product Long Description] )) = LTRIM(RTRIM( @ProductLongDescription ))
					and [FSS Price] = @FSSPrice
					and [Package Size Priced on Contract] = @PackageSizePricedOnContract
					and [SIN] = @SIN
					and [621I_Category_ID] = @ServiceDescriptionId
	                and((
						datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                    datediff(dd,@DateEffective, [ExpirationDate])>=0
                        )
                        or
                        (
                        datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                        datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[DateEffective])>=0  and 
                        datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                        datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
						))
					and Removed = 0
					and LogNumber <> @LogNumberBeingUpdated
					)
					BEGIN
						select @PriceExists = 1
					END
			END
		END
		else
		BEGIN
			if @LogNumberBeingUpdated is null
			BEGIN
				if exists (
				select LogNumber from tbl_pricelist
					where CntrctNum = @ContractNumber
					and LTRIM(RTRIM( [Contractor Catalog Number] )) = LTRIM(RTRIM( @ContractorCatalogNumber ))
					and LTRIM(RTRIM( [Product Long Description] )) = LTRIM(RTRIM( @ProductLongDescription ))
					and [Package Size Priced on Contract] = @PackageSizePricedOnContract
					and [SIN] = @SIN
					and [621I_Category_ID] = @ServiceDescriptionId
		            and((
						datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                    datediff(dd,@DateEffective, [ExpirationDate])>=0
                        )
                        or
                        (
                        datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                        datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[DateEffective])>=0  and 
                        datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                        datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
						))
					and Removed = 0
					)
					BEGIN
						select @PriceExists = 1
					END
			END
			else
			BEGIN
				if exists (
				select LogNumber from tbl_pricelist
					where CntrctNum = @ContractNumber
					and LTRIM(RTRIM( [Contractor Catalog Number] )) = LTRIM(RTRIM( @ContractorCatalogNumber ))
					and LTRIM(RTRIM( [Product Long Description] )) = LTRIM(RTRIM( @ProductLongDescription ))
					and [Package Size Priced on Contract] = @PackageSizePricedOnContract
					and [SIN] = @SIN
					and [621I_Category_ID] = @ServiceDescriptionId
		            and((
						datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                    datediff(dd,@DateEffective, [ExpirationDate])>=0
                        )
                        or
                        (
                        datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                        datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[DateEffective])>=0  and 
                        datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                        datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
						))
					and Removed = 0
					and LogNumber <> @LogNumberBeingUpdated
					)
					BEGIN
						select @PriceExists = 1
					END
			END
		END
	END
	else -- not 621I
	BEGIN
		if @ComparePrice = 1
		BEGIN
			if @LogNumberBeingUpdated is null
			BEGIN
				if exists (
				select LogNumber from tbl_pricelist
					where CntrctNum = @ContractNumber
					and LTRIM(RTRIM( [Contractor Catalog Number] )) = LTRIM(RTRIM( @ContractorCatalogNumber ))
					and LTRIM(RTRIM( [Product Long Description] )) = LTRIM(RTRIM( @ProductLongDescription ))
					and [FSS Price] = @FSSPrice
					and [Package Size Priced on Contract] = @PackageSizePricedOnContract
					and [SIN] = @SIN
	                and((
						datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                    datediff(dd,@DateEffective, [ExpirationDate])>=0
                        )
                        or
                        (
                        datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                        datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[DateEffective])>=0  and 
                        datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                        datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
						))
					and Removed = 0
					)
					BEGIN
						select @PriceExists = 1
					END
			END
			else
			BEGIN
				if exists (
				select LogNumber from tbl_pricelist
					where CntrctNum = @ContractNumber
					and LTRIM(RTRIM( [Contractor Catalog Number] )) = LTRIM(RTRIM( @ContractorCatalogNumber ))
					and LTRIM(RTRIM( [Product Long Description] )) = LTRIM(RTRIM( @ProductLongDescription ))
					and [FSS Price] = @FSSPrice
					and [Package Size Priced on Contract] = @PackageSizePricedOnContract
					and [SIN] = @SIN
	                and((
						datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                    datediff(dd,@DateEffective, [ExpirationDate])>=0
                        )
                        or
                        (
                        datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                        datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[DateEffective])>=0  and 
                        datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                        datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
						))
					and Removed = 0
					and LogNumber <> @LogNumberBeingUpdated
					)
					BEGIN
						select @PriceExists = 1
					END
			END
		END
		else
		BEGIN
			if @LogNumberBeingUpdated is null
			BEGIN
				if exists (
				select LogNumber from tbl_pricelist
					where CntrctNum = @ContractNumber
					and LTRIM(RTRIM( [Contractor Catalog Number] )) = LTRIM(RTRIM( @ContractorCatalogNumber ))
					and LTRIM(RTRIM( [Product Long Description] )) = LTRIM(RTRIM( @ProductLongDescription ))
					and [Package Size Priced on Contract] = @PackageSizePricedOnContract
					and [SIN] = @SIN
		            and((
						datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                    datediff(dd,@DateEffective, [ExpirationDate])>=0
                        )
                        or
                        (
                        datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                        datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[DateEffective])>=0  and 
                        datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                        datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
						))
					and Removed = 0
					)
					BEGIN
						select @PriceExists = 1
					END
			END
			else
			BEGIN
				if exists (
				select LogNumber from tbl_pricelist
					where CntrctNum = @ContractNumber
					and LTRIM(RTRIM( [Contractor Catalog Number] )) = LTRIM(RTRIM( @ContractorCatalogNumber ))
					and LTRIM(RTRIM( [Product Long Description] )) = LTRIM(RTRIM( @ProductLongDescription ))
					and [Package Size Priced on Contract] = @PackageSizePricedOnContract
					and [SIN] = @SIN
		            and((
						datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                    datediff(dd,@DateEffective, [ExpirationDate])>=0
                        )
                        or
                        (
                        datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                        datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[DateEffective])>=0  and 
                        datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                        datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
						))
					and Removed = 0
					and LogNumber <> @LogNumberBeingUpdated
					)
					BEGIN
						select @PriceExists = 1
					END
			END

		END
	END

	RETURN @PriceExists

END