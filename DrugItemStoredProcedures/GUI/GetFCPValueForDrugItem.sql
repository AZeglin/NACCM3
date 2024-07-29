IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetFCPValueForDrugItem]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [GetFCPValueForDrugItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Function GetFCPValueForDrugItem
(
@DrugItemId int,
@Year int = null
)

returns Decimal(9,2)

as

BEGIN

DECLARE @fcp Decimal(9,2),
		@month int, 
		@currentYear int,
		@yearToTest int

	Select @month = month(getdate()), @currentYear = year(getdate())

	/* must make up a year */
	if @Year is null 
	BEGIN
		If ( @month >=1 and @month <=9 )
		BEGIN
			select @yearToTest = @currentYear
		END
		else
		BEGIN
			select @yearToTest = @currentYear + 1		
		END
	END
	else
	BEGIN
		select @yearToTest = @Year
	END
	
	
	Select @fcp = f.FCP
	From DI_FCP f
--	Join di_yearlookup y
--	on f.YearId = y.YearId
--	join DI_DrugItemNDC n
--	on f.ndc_1 = n.FdaAssignedLabelerCode and f.ndc_2 = n.ProductCode and f.ndc_3 = n.PackageCode
	join DI_DrugItems i
	on f.DrugItemId = i.DrugItemId
	where i.DrugItemId = @DrugItemId
	and f.YearId = @yearToTest

	/*Select @fcp = a.FCP
	From DI_FCP a
	Join di_yearlookup b
	on a.YearId = b.YearId
	Where a.DrugItemId = @drugItemId
	And b.YearValue = @Year  */

	return @fcp

END

