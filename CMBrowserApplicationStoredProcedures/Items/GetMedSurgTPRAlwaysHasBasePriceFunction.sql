IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetMedSurgTPRAlwaysHasBasePriceFunction')
	BEGIN
		DROP  Function  GetMedSurgTPRAlwaysHasBasePriceFunction
	END

GO

CREATE Function GetMedSurgTPRAlwaysHasBasePriceFunction
(
@ItemId int,
@PriceStartDate datetime,   
@PriceEndDate datetime,      
@IsTemporary bit
)

returns bit

AS

BEGIN

	DECLARE @TPRAlwaysHasBasePrice bit

	if @IsTemporary = 1
	BEGIN

		if exists( 		
			( select [Date] as TPRDate from AllDates
				where [Date] between @PriceStartDate and @PriceEndDate 
				and [Date] not in
	
				( select t.TPRDate from 

				( select [Date] as TPRDate from AllDates
					where [Date] between @PriceStartDate and @PriceEndDate ) t,
		
				( select PriceStartDate, PriceStopDate
					from CM_ItemPrice
					where ItemId = @ItemId
						and IsTemporary = 0						
						and Price <> -1      -- ignore placeholder prices since they are not valid prices

						and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceEndDate ) or
							PriceStartDate between @PriceStartDate and @PriceEndDate or
							PriceStopDate between @PriceStartDate and @PriceEndDate or
							( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceEndDate ) or
							( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceEndDate ) or
							PriceStartDate = @PriceEndDate  or
							PriceStopDate = @PriceStartDate )
				) b
		
				where t.TPRDate between  b.PriceStartDate and b.PriceStopDate )
			)
		)
		BEGIN
			select @TPRAlwaysHasBasePrice = 0
		END
		else
		BEGIN
			select @TPRAlwaysHasBasePrice = 1
		END
	END
	else
	BEGIN
		select @TPRAlwaysHasBasePrice = 0 -- not a TPR
	END

	return @TPRAlwaysHasBasePrice

END


