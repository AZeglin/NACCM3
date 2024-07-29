IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetItemDualPriceStatusForDrugItemId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [GetItemDualPriceStatusForDrugItemId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function GetItemDualPriceStatusForDrugItemId
(
@DrugItemId int
)

returns bit

as

BEGIN

	DECLARE @IsDualPricer bit, @month int, @year int,@date datetime, @newYear varchar(4)
	Select @month = month(getdate()), @year = Year(getdate())

	If (@month >=1 and @month <=9)
	Begin
		if exists (Select * from DI_DrugItemPrice
							where DrugItemId = @DrugItemId
							and IsBIG4 = 1
						--	and getdate() between PriceStartDate and PriceStopDate
							and	datediff( d, PriceStopDate, GETDATE() ) <= 0 
							and datediff( d, PriceStartDate, GETDATE() ) >= 0  
					)
		Begin
			select @IsDualPricer = 1
		End
		Else 
		Begin
			select @IsDualPricer = 0
		End	
	End
	If (@month >=10 and @month <=12)
	Begin
		Select @newYear = cast(@year+1 as varchar(4))
		Select @date = Cast('1/1/'+@newYear as datetime)

		if exists (Select * from DI_DrugItemPrice
							where DrugItemId = @DrugItemId
							and IsBIG4 = 1
						--	and @date between PriceStartDate and PriceStopDate
							and	datediff( d, PriceStopDate, @date ) <= 0 
							and datediff( d, PriceStartDate, @date ) >= 0  
					)
		Begin
			select @IsDualPricer = 1
		End
		Else 
		Begin
			select @IsDualPricer = 0
		End	
	end


	return @IsDualPricer
END


