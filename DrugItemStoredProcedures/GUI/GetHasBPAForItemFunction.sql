IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetHasBPAForItemFunction')
	BEGIN
		DROP  Function  GetHasBPAForItemFunction
	END

GO

CREATE Function GetHasBPAForItemFunction
(
@DrugItemId int
)

Returns bit

AS

BEGIN

	DECLARE @HasBPA bit
	
	if exists( select ParentDrugItemId
			from DI_DrugItems
			where ParentDrugItemId = @DrugItemId )
	BEGIN
		select @HasBPA = 1
	END
	else
	BEGIN
		select @HasBPA = 0
	END
	
	return @HasBPA

END


