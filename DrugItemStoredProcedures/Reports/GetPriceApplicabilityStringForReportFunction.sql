IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetPriceApplicabilityStringForReportFunction')
	BEGIN
		DROP  Function  GetPriceApplicabilityStringForReportFunction
	END

GO

CREATE Function GetPriceApplicabilityStringForReportFunction
(
@DrugItemPriceId int,
@FutureHistoricalSelectionCriteria nchar(1)  -- H historical, F future, A active, B both future and active
)

Returns nvarchar(200)

AS

BEGIN

	DECLARE @priceApplicabilityString nvarchar(200),
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
	
	if @FutureHistoricalSelectionCriteria = 'H'
	BEGIN
	
			select @IsFSS = h.IsFSS,
			@IsBIG4 = h.IsBIG4,                                          	                  
			@IsVA = h.IsVA,                                            	                  
			@IsBOP = h.IsBOP,                                           	                  
			@IsCMOP = h.IsCMOP,                                          	                  
			@IsDOD = h.IsDOD,                                           	                  
			@IsHHS = h.IsHHS,                                           	                  
			@IsIHS = h.IsIHS,                                           	                  
			@IsIHS2 = h.IsIHS2,                                          	                  
			@IsDIHS = h.IsDIHS,                                          	                  
			@IsNIH = h.IsNIH,                                           	                  
			@IsPHS = h.IsPHS,                                           	                  
			@IsSVH = h.IsSVH,                                           	                  
			@IsSVH1 = h.IsSVH1,                                          	                  
			@IsSVH2 = h.IsSVH2,                                          	                  
			@IsTMOP = h.IsTMOP,                                          	                  
			@IsUSCG = h.IsUSCG,
			@IsFHCC = h.IsFHCC
		from DI_DrugItemPriceHistory h
		where h.DrugItemPriceId = @DrugItemPriceId
		
	END
	else
	BEGIN

		select @IsFSS = p.IsFSS,
			@IsBIG4 = p.IsBIG4,                                          	                  
			@IsVA = p.IsVA,                                            	                  
			@IsBOP = p.IsBOP,                                           	                  
			@IsCMOP = p.IsCMOP,                                          	                  
			@IsDOD = p.IsDOD,                                           	                  
			@IsHHS = p.IsHHS,                                           	                  
			@IsIHS = p.IsIHS,                                           	                  
			@IsIHS2 = p.IsIHS2,                                          	                  
			@IsDIHS = p.IsDIHS,                                          	                  
			@IsNIH = p.IsNIH,                                           	                  
			@IsPHS = p.IsPHS,                                           	                  
			@IsSVH = p.IsSVH,                                           	                  
			@IsSVH1 = p.IsSVH1,                                          	                  
			@IsSVH2 = p.IsSVH2,                                          	                  
			@IsTMOP = p.IsTMOP,                                          	                  
			@IsUSCG = p.IsUSCG,
			@IsFHCC = p.IsFHCC
		from DI_DrugItemPrice p 
		where p.DrugItemPriceId = @DrugItemPriceId

	END
	
	select @priceApplicabilityString = ''
	
	if @IsFSS = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'FSS '
	END
	if @IsBIG4 = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'BIG4 '
	END
	if @IsVA = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'VA '
	END
	if @IsBOP = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'BOP '
	END
	if @IsCMOP = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'CMOP '
	END
	if @IsDOD = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'DOD '
	END	
	if @IsHHS = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'HHS '
	END	
	if @IsIHS = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'IHS '
	END		
	if @IsIHS2 = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'IHS2 '
	END			
	if @IsDIHS = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'DIHS '
	END	
	if @IsNIH = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'NIH '
	END		
	if @IsPHS = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'PHS '
	END		
	if @IsSVH = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'SVH '
	END		
	if @IsSVH1 = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'SVH1 '
	END		
	if @IsSVH2 = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'SVH2 '
	END		
	if @IsTMOP = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'TMOP '
	END		
	if @IsUSCG = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'USCG '
	END		
	if @IsFHCC = 1
	BEGIN
		select @priceApplicabilityString = @priceApplicabilityString + 'FHCC '
	END		
		
return @priceApplicabilityString

END


