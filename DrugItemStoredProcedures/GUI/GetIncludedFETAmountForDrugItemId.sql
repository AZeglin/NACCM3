IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetIncludedFETAmountForDrugItemId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [GetIncludedFETAmountForDrugItemId]
GO

CREATE function GetIncludedFETAmountForDrugItemId
(
@DrugItemId int
)

returns decimal(10,2)

as

BEGIN

	DECLARE @FETAmount decimal(10,2)

	select @FETAmount = isnull( IncludedFETAmount, 0 )
	from DI_DrugItems
	where DrugItemId = @DrugItemId

	return @FETAmount
END


