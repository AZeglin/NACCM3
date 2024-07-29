IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SelectSalePackagingMeasureUnits]') AND type in (N'P', N'PC'))
DROP PROCEDURE [SelectSalePackagingMeasureUnits]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure SelectSalePackagingMeasureUnits
(
@CurrentUser uniqueidentifier,
@UnitType nchar(1) /* S = Sale, P = Package, M = Measure */
)

AS

BEGIN

	if @UnitType = 'S'
	BEGIN
		select SelectableUnit, Description
		from DI_SalePackagingMeasureUnits
		where IsUnitOfSale = 1
		order by SelectableUnit
	END
	else if @UnitType = 'P'
	BEGIN
		select SelectableUnit, Description
		from DI_SalePackagingMeasureUnits
		where IsUnitPackage = 1
		order by SelectableUnit
	END
	else if @UnitType = 'M'
	BEGIN
		select SelectableUnit, Description
		from DI_SalePackagingMeasureUnits
		where IsUnitOfMeasure = 1
		order by SelectableUnit
	END

END