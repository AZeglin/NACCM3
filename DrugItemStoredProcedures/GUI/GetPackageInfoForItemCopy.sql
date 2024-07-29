IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetPackageInfoForItemCopy')
	BEGIN
		DROP  Procedure  GetPackageInfoForItemCopy
	END

GO

CREATE Procedure GetPackageInfoForItemCopy
(
@CurrentUser uniqueidentifier,
@DrugItemId int,
@UnitOfSale nchar(2) OUTPUT,
@QuantityInUnitOfSale decimal(5,0) OUTPUT,
@UnitPackage nchar(2) OUTPUT,
@QuantityInUnitPackage decimal(13,5) OUTPUT,
@UnitOfMeasure nchar(2)  OUTPUT
)

AS

DECLARE @error int,
		@rowCount int,
		@errorMsg nvarchar(200)

BEGIN

	select @UnitOfSale = UnitOfSale, 
		@QuantityInUnitOfSale = QuantityInUnitOfSale, 
		@UnitPackage = UnitPackage, 
		@QuantityInUnitPackage = QuantityInUnitPackage, 
		@UnitOfMeasure = UnitOfMeasure
	from DI_DrugItemPackage 
	where DrugItemId = @DrugItemId

	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving packaging information for drug item id = ' + convert( nvarchar(20), @DrugItemId )
		raiserror( @errorMsg, 16, 1 )	
	END

END