IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetParentDrugItemDescription')
	BEGIN
		DROP  Procedure  GetParentDrugItemDescription
	END

GO

CREATE Procedure GetParentDrugItemDescription
( 
@ParentDrugItemId int,
@OverallDescription nvarchar(200) OUTPUT
)

AS

BEGIN

	select @OverallDescription = n.FdaAssignedLabelerCode + ' ' + n.ProductCode + ' ' + n.PackageCode + ' ' + i.Generic
	from DI_DrugItems i
	join DI_DrugItemNDC n
		on i.DrugItemNDCId = n.DrugItemNDCId
	where i.DrugItemId = @ParentDrugItemId

END