IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectPricelistDetailForMedSurgItem')
	BEGIN
		DROP  Procedure  SelectPricelistDetailForMedSurgItem
	END

GO

CREATE Procedure SelectPricelistDetailForMedSurgItem
(
@CurrentUser uniqueidentifier,
@PricelistType nchar(2),
@LogNumber int
)

AS

BEGIN
	/* select records for the nac cm web version pricelist details from */
	if @PricelistType = 'F'
	BEGIN
		SELECT LogNumber,
			isnull([Contractor Catalog Number], '' ) AS 'Contractor_Catalog_Number', 
			isnull([FSS Price], 0 ) AS 'FSS_Price', 
			isnull([Package Size Priced on Contract], '' ) AS 'Package_Size_Priced_on_Contract', 
			isnull([SIN], '' ) AS 'FSS_SIN', 
			isnull([Product Long Description], '' ) AS 'Product_Long_Description', 
			--isnull([Outer Pack UOM], '' ) AS 'Outer_Pack_UOM', 
			--isnull([Outer Pack Unit of Conversion Factor], 0 ) AS 'Outer_Pack_Unit_of_Conversion_Factor', 
			--isnull([Outer Pack Unit Shippable], 0 ) AS 'Outer_Pack_Unit_Shippable', 
			--isnull([Outer Pack UPN], '' ) AS 'Outer_Pack_UPN', 
			--isnull([Intermediate Pack UOM], '' ) AS 'Intermediate_Pack_UOM', 
			--isnull([Intermediate Pack Unit of Conversion Factor], 0 ) AS 'Intermediate_Pack_Unit_of_Conversion_Factor', 
			--isnull([Intermediate Pack Shippable], 0 ) AS 'Intermediate_Pack_Shippable', 
			--isnull([Intermediate Pack UPN], '' ) AS 'Intermediate_Pack_UPN', 
			--isnull([Base Packaging UOM], '' ) AS 'Base_Packaging_UOM', 
			--isnull([Base Packaging Unit of Conversion Factor],0 ) AS 'Base_Packaging_Unit_of_Conversion_Factor', 
			--isnull([Base Packaging Unit Shippable], 0 ) AS 'Base_Packaging_Unit_Shippable', 
			--isnull([Base Packaging UPN], '' ) AS 'Base_Packaging_UPN', 
			[Tier 1 Price] AS 'Tier_1_Price', 
			[Tier 2 Price] AS 'Tier_2_Price', 
			[Tier 3 Price] AS 'Tier_3_Price', 
			[Tier 4 Price] AS 'Tier_4_Price', 
			[Tier 5 Price] AS 'Tier_5_Price', 
			isnull([Tier 1 Note], '' ) AS 'Tier_1_Note', 
			isnull([Tier 2 Note], '' ) AS 'Tier_2_Note', 
			isnull([Tier 3 Note], '' ) AS 'Tier_3_Note', 
			isnull([Tier 4 Note], '' ) AS 'Tier_4_Note', 
			isnull([Tier 5 Note], '' ) AS 'Tier_5_Note',
			isnull(DateEffective, '' ) AS 'FSSDateEffective', 
			isnull(ExpirationDate, '' ) AS 'FSSExpirationDate'
		FROM tbl_pricelist 
		WHERE LogNumber = @LogNumber 
	
	END
	else if @PricelistType = '6'
	BEGIN
		SELECT LogNumber,
			isnull([Contractor Catalog Number], '' ) AS 'Contractor_Catalog_Number', 
			isnull([Product Long Description], '' ) AS 'Product_Long_Description', 
			isnull([FSS Price], 0 ) AS 'FSS_Price', 
			isnull([Package Size Priced on Contract], '' ) AS 'Package_Size_Priced_on_Contract', 
			isnull([SIN], '' ) AS 'FSS_SIN', 
			--isnull([Outer Pack UOM], '' ) AS 'Outer_Pack_UOM', 
			--isnull([Outer Pack Unit of Conversion Factor], 0 ) AS 'Outer_Pack_Unit_of_Conversion_Factor', 
			--isnull([Outer Pack Unit Shippable], 0 ) AS 'Outer_Pack_Unit_Shippable', 
			--isnull([Outer Pack UPN], '' ) AS 'Outer_Pack_UPN', 
			--isnull([Intermediate Pack UOM], '' ) AS 'Intermediate_Pack_UOM', 
			--isnull([Intermediate Pack Unit of Conversion Factor], 0 ) AS 'Intermediate_Pack_Unit_of_Conversion_Factor', 
			--isnull([Intermediate Pack Shippable], 0 ) AS 'Intermediate_Pack_Shippable', 
			--isnull([Intermediate Pack UPN], '' ) AS 'Intermediate_Pack_UPN', 
			--isnull([Base Packaging UOM], '' ) AS 'Base_Packaging_UOM', 
			--isnull([Base Packaging Unit of Conversion Factor], '' ) AS 'Base_Packaging_Unit_of_Conversion_Factor', 
			--isnull([Base Packaging Unit Shippable], '' ) AS 'Base_Packaging_Unit_Shippable', 
			--isnull([Base Packaging UPN], '' ) AS 'Base_Packaging_UPN', 
			isnull([Tier 1 Price], 0 ) AS 'Tier_1_Price', 
			isnull([Tier 2 Price], 0 ) AS 'Tier_2_Price', 
			isnull([Tier 3 Price], 0 ) AS 'Tier_3_Price', 
			isnull([Tier 4 Price], 0 ) AS 'Tier_4_Price', 
			isnull([Tier 5 Price], 0 ) AS 'Tier_5_Price', 
			isnull([Tier 1 Note], '' ) AS 'Tier_1_Note', 
			isnull([Tier 2 Note], '' ) AS 'Tier_2_Note', 
			isnull([Tier 3 Note], '' ) AS 'Tier_3_Note', 
			isnull([Tier 4 Note], '' ) AS 'Tier_4_Note', 
			isnull([Tier 5 Note], '' ) AS 'Tier_5_Note',
			isnull([621I_Category_ID], '' ) AS 'ServiceCategoryId',
			isnull(DateEffective, '' ) AS 'FSSDateEffective', 
			isnull(ExpirationDate, '' ) AS 'FSSExpirationDate'
		FROM tbl_pricelist WHERE LogNumber = @LogNumber 	
	
	END
	else if @PricelistType = 'B' 
	BEGIN
	
			SELECT b.BPALogNumber,
			b.FSSLogNumber,
			isnull(p.[Contractor Catalog Number], '' ) AS 'Contractor_Catalog_Number', 
			isnull(p.[FSS Price], 0 ) AS 'FSS_Price', 
			isnull(p.[Package Size Priced on Contract], '' ) AS 'Package_Size_Priced_on_Contract', 
			isnull(p.[SIN], '' ) AS 'FSS_SIN',  
			isnull(p.[Product Long Description], '' ) AS 'Product_Long_Description', 
			--isnull(p.[Outer Pack UOM], '' ) AS 'Outer_Pack_UOM', 
			--isnull(p.[Outer Pack Unit of Conversion Factor], 0 ) AS 'Outer_Pack_Unit_of_Conversion_Factor', 
			--isnull(p.[Outer Pack Unit Shippable], 0 ) AS 'Outer_Pack_Unit_Shippable', 
			--isnull(p.[Outer Pack UPN], '' ) AS 'Outer_Pack_UPN', 
			--isnull(p.[Intermediate Pack UOM], '' ) AS 'Intermediate_Pack_UOM', 
			--isnull(p.[Intermediate Pack Unit of Conversion Factor], 0 ) AS 'Intermediate_Pack_Unit_of_Conversion_Factor', 
			--isnull(p.[Intermediate Pack Shippable], 0 ) AS 'Intermediate_Pack_Shippable', 
			--isnull(p.[Intermediate Pack UPN], '' ) AS 'Intermediate_Pack_UPN', 
			--isnull(p.[Base Packaging UOM], '' ) AS 'Base_Packaging_UOM', 
			--isnull(p.[Base Packaging Unit of Conversion Factor], 0 ) AS 'Base_Packaging_Unit_of_Conversion_Factor', 
			--isnull(p.[Base Packaging Unit Shippable], 0 ) AS 'Base_Packaging_Unit_Shippable', 
			--isnull(p.[Base Packaging UPN], '' ) AS 'Base_Packaging_UPN', 
			isnull(p.[Tier 1 Price], 0 ) AS 'Tier_1_Price', 
			isnull(p.[Tier 2 Price], 0 ) AS 'Tier_2_Price', 
			isnull(p.[Tier 3 Price], 0 ) AS 'Tier_3_Price', 
			isnull(p.[Tier 4 Price], 0 ) AS 'Tier_4_Price', 
			isnull(p.[Tier 5 Price], 0 ) AS 'Tier_5_Price', 
			isnull(p.[Tier 1 Note], '' ) AS 'Tier_1_Note', 
			isnull(p.[Tier 2 Note], '' ) AS 'Tier_2_Note', 
			isnull(p.[Tier 3 Note], '' ) AS 'Tier_3_Note', 
			isnull(p.[Tier 4 Note], '' ) AS 'Tier_4_Note', 
			isnull(p.[Tier 5 Note], '' ) AS 'Tier_5_Note',
			isnull(p.DateEffective, '' ) AS 'FSSDateEffective', 
			isnull(p.ExpirationDate, '' ) AS 'FSSExpirationDate', 			
			isnull(b.Description, '' ) AS 'BPADescription',
			isnull(b.[BPA/BOA Price], 0 ) AS 'BPAPrice',
			isnull(b.DateEffective, '' ) AS 'BPADateEffective', 
			isnull(b.ExpirationDate, '' ) AS 'BPAExpirationDate'
		
		FROM tbl_pricelist p, tbl_BPA_Pricelist b
		WHERE b.BPALogNumber = @LogNumber 
		AND p.LogNumber = b.FSSLogNumber
		
	END
	else if @PricelistType = 'NB' 
	BEGIN
	
			SELECT b.BPALogNumber,
			isnull(b.Description, '' ) AS 'BPADescription',
			isnull(b.[BPA/BOA Price], 0 ) AS 'BPAPrice',
			isnull(b.DateEffective, '' ) AS 'BPADateEffective', 
			isnull(b.ExpirationDate, '' ) AS 'BPAExpirationDate'
		
		FROM tbl_BPA_Pricelist b
		WHERE b.BPALogNumber = @LogNumber 
		
	END
END