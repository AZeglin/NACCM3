IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdatePricelistDetailForMedSurgServiceItem')
	BEGIN
		DROP  Procedure  UpdatePricelistDetailForMedSurgServiceItem
	END

GO

CREATE Procedure UpdatePricelistDetailForMedSurgServiceItem
(
@CurrentUser uniqueidentifier,
@FSSLogNumber as int,
@ContractorCatalogNumber as nvarchar(50),
@ProductLongDescription as nvarchar(800),
@FSSPrice as decimal(18,2),
@PackageSizePricedOnContract as nvarchar(2),
@SIN as nvarchar(50),
--@OuterPackUOM as nvarchar(2),
--@OuterPackUnitOfConversionFactor as int,
--@OuterPackUnitShippable as bit,
--@OuterPackUPN as nvarchar(20),
--@IntermediatePackUOM as nvarchar(2),
--@IntermediatePackUnitOfConversionFactor as int,
--@IntermediatePackShippable as bit,
--@IntermediatePackUPN as nvarchar(20),
--@BasePackUOM as nvarchar(2),
--@BasePackUnitOfConversionFactor as int,
--@BasePackUnitShippable as bit,
--@BasePackUPN as nvarchar(20),
@Tier1Price as decimal(18,2),
@Tier2Price as decimal(18,2),
@Tier3Price as decimal(18,2),
@Tier4Price as decimal(18,2),
@Tier5Price as decimal(18,2),
@Tier1Note as nvarchar(255),
@Tier2Note as nvarchar(255),
@Tier3Note as nvarchar(255),
@Tier4Note as nvarchar(255),
@Tier5Note as nvarchar(255),
@FSSEffectiveDate as datetime,
@FSSExpirationDate as datetime,
@ServiceCategoryId as int,
@LastModifiedBy nvarchar(120)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@PriceExists bit,
		@ExistingContractNumber nvarchar(50)



BEGIN TRANSACTION

	select @ExistingContractNumber = CntrctNum
	from tbl_pricelist 
	where LogNumber = @FSSLogNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating med/surg item into pricelist. Could not retrieve contract number of existing item.'
		goto ERROREXIT
	END

	if DATEDIFF( dd, @FSSEffectiveDate, @FSSExpirationDate ) < 0
	BEGIN
		select @errorMsg = 'Effective date must be before expiration date.' 
		goto ERROREXIT
	END

	select @PriceExists = dbo.CheckForDuplicateMedSurgFSSPriceFunction( @ExistingContractNumber, @ContractorCatalogNumber, @ProductLongDescription, @FSSPrice, @PackageSizePricedOnContract, @SIN, @ServiceCategoryId, @FSSEffectiveDate, @FSSExpirationDate, @FSSLogNumber, 0, 1 )

	if @PriceExists = 0
	BEGIN

      UPDATE tbl_pricelist
      SET   [Contractor Catalog Number] = @ContractorCatalogNumber, 
            [Product Long Description] = @ProductLongDescription, 
            [FSS Price] = @FSSPrice, 
            [Package Size Priced on Contract] = @PackageSizePricedonContract, 
            [SIN] = @SIN, 
            --[Outer Pack UOM] = @OuterPackUOM, 
            --[Outer Pack Unit of Conversion Factor] = @OuterPackUnitofConversionFactor, 
            --[Outer Pack Unit Shippable] = @OuterPackUnitShippable, 
            --[Outer Pack UPN] = @OuterPackUPN, 
            --[Intermediate Pack UOM] = @IntermediatePackUOM, 
            --[Intermediate Pack Unit of Conversion Factor] = @IntermediatePackUnitofConversionFactor, 
            --[Intermediate Pack Shippable] = @IntermediatePackShippable, 
            --[Intermediate Pack UPN] = @IntermediatePackUPN, 
            --[Base Packaging UOM] = @BasePackUOM, 
            --[Base Packaging Unit of Conversion Factor] = @BasePackUnitofConversionFactor,
            --[Base Packaging Unit Shippable] = @BasePackUnitShippable, 
            --[Base Packaging UPN] = @BasePackUPN, 
            [Tier 1 Price] = @Tier1Price, 
            [Tier 2 Price] = @Tier2Price, 
            [Tier 3 Price] = @Tier3Price, 
            [Tier 4 Price] = @Tier4Price, 
            [Tier 5 Price] = @Tier5Price, 
            [Tier 1 Note] = @Tier1Note, 
            [Tier 2 Note] = @Tier2Note, 
            [Tier 3 Note] = @Tier3Note, 
            [Tier 4 Note] = @Tier4Note, 
            [Tier 5 Note] = @Tier5Note,
            [621I_Category_ID] = @ServiceCategoryId,
            [Date_Modified] = getdate(),
            DateEffective = @FSSEffectiveDate, 
            ExpirationDate = @FSSExpirationDate,
			LastModifiedBy = @LastModifiedBy
        WHERE LogNumber = @FSSLogNumber

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating med/surg service item into pricelist.'
			goto ERROREXIT
		END

	END
	else
	BEGIN
		select @errorMsg = 'The item/price being updated (service) matches another item already existing in the database. If implementing a future item/price, consider inserting a new price record and choosing an effective date range that does not overlap the date range of an existing item/price.'
		goto ERROREXIT
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
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )



