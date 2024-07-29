IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetItemDualPriceStatusForDrugItemSubItemIdFunction')
	BEGIN
		DROP  Function  GetItemDualPriceStatusForDrugItemSubItemIdFunction
	END

GO

CREATE function [dbo].[GetItemDualPriceStatusForDrugItemSubItemIdFunction]
(
@DrugItemSubItemId int
)

returns bit

as

BEGIN

	DECLARE @IsDualPricer bit, @month int, @year int,@date datetime, @newYear varchar(4)
	Select @month = month(getdate()), @year = Year(getdate())

	If (@month >=1 and @month <=9)
	Begin
		if exists (Select * from DI_DrugItemPrice
							where DrugItemSubItemId = @DrugItemSubItemId
							and IsBIG4 = 1
							and getdate() between PriceStartDate and PriceStopDate
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
							where DrugItemSubItemId = @DrugItemSubItemId
							and IsBIG4 = 1
							and @date between PriceStartDate and PriceStopDate
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
