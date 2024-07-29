IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectPricelistDetailNewFSSForBPA')
	BEGIN
		DROP  Procedure  SelectPricelistDetailNewFSSForBPA
	END

GO

CREATE Procedure SelectPricelistDetailNewFSSForBPA
(
@CurrentUser uniqueidentifier,
@FSSLogNumber as int
)

AS

BEGIN

		SELECT  [Contractor Catalog Number] AS Contractor_Catalog_Number, 
			[FSS Price] AS FSS_Price, 
			[Package Size Priced on Contract] AS Package_Size_Priced_on_Contract, 
			[SIN], 
			[Product Long Description] AS Product_Long_Description, 
			[Outer Pack UOM] AS Outer_Pack_UOM, 
			[Outer Pack Unit of Conversion Factor] AS Outer_Pack_Unit_of_Conversion_Factor, 
			[Outer Pack Unit Shippable] AS Outer_Pack_Unit_Shippable, 
			[Outer Pack UPN] AS Outer_Pack_UPN, 
			[Intermediate Pack UOM] AS Intermediate_Pack_UOM, 
			[Intermediate Pack Unit of Conversion Factor] AS Intermediate_Pack_Unit_of_Conversion_Factor, 
			[Intermediate Pack Shippable] AS Intermediate_Pack_Shippable, 
			[Intermediate Pack UPN] AS Intermediate_Pack_UPN, 
			[Base Packaging UOM] AS Base_Packaging_UOM, 
			[Base Packaging Unit of Conversion Factor] AS Base_Packaging_Unit_of_Conversion_Factor, 
			[Base Packaging Unit Shippable] AS Base_Packaging_Unit_Shippable, 
			[Base Packaging UPN] AS Base_Packaging_UPN, 
			[Tier 1 Price] AS Tier_1_Price, 
			[Tier 2 Price] AS Tier_2_Price, 
			[Tier 3 Price] AS Tier_3_Price, 
			[Tier 4 Price] AS Tier_4_Price, 
			[Tier 5 Price] AS Tier_5_Price, 
			[Tier 1 Note] AS Tier_1_Note, 
			[Tier 2 Note] AS Tier_2_Note, 
			[Tier 3 Note] AS Tier_3_Note, 
			[Tier 4 Note] AS Tier_4_Note, 
			[Tier 5 Note] AS Tier_5_Note,
			CONVERT( DateTime, DateEffective, 101 ) as FSSDateEffective, 
			CONVERT( DateTime, ExpirationDate, 101 ) as FSSExpirationDate
		FROM tbl_pricelist 
		WHERE LogNumber = @FSSLogNumber 

END

