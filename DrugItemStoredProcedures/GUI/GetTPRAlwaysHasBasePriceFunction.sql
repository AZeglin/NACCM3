IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetTPRAlwaysHasBasePriceFunction')
	BEGIN
		DROP  Function  GetTPRAlwaysHasBasePriceFunction
	END

GO

CREATE Function GetTPRAlwaysHasBasePriceFunction
(
@DrugItemId int,
@PriceStartDate datetime,   
@PriceEndDate datetime,      
@IsTemporary bit,
@IsFSS bit,                                           	                  
@IsBIG4 bit,                                          	                  
@IsVA bit,                                            	                  
@IsBOP bit,                                           	                  
@IsCMOP bit,                                          	                  
@IsDOD bit,                                           	                  
@IsHHS bit,                                           	                  
@IsIHS bit,                                           	                  
@IsIHS2 bit,                                          	                  
@IsDIHS bit,                                          	                  
@IsNIH bit,                                           	                  
@IsPHS bit,                                           	                  
@IsSVH bit,                                           	                  
@IsSVH1 bit,                                          	                  
@IsSVH2 bit,                                          	                  
@IsTMOP bit,                                          	                  
@IsUSCG bit,
@IsFHCC bit
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
					from DI_DrugItemPrice
					where DrugItemId = @DrugItemId
						and IsTemporary = 0
						and IsFSS = @IsFSS                                          	                  
						and IsBIG4 = @IsBIG4                                          	                  
						and IsVA = @IsVA                                            	                  
						and IsBOP = @IsBOP                                           	                  
						and IsCMOP = @IsCMOP                                          	                  
						and IsDOD = @IsDOD                                           	                  
						and IsHHS = @IsHHS                                           	                  
						and IsIHS = @IsIHS                                           	                  
						and IsIHS2 = @IsIHS2                                          	                  
						and IsDIHS = @IsDIHS                                          	                  
						and IsNIH = @IsNIH                                           	                  
						and IsPHS = @IsPHS                                           	                  
						and IsSVH = @IsSVH                                           	                  
						and IsSVH1 = @IsSVH1                                          	                  
						and IsSVH2 = @IsSVH2                                          	                  
						and IsTMOP = @IsTMOP                                          	                  
						and IsUSCG = @IsUSCG
						and IsFHCC = @IsFHCC
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


