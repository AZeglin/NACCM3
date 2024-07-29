IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ValidatePriceAllowUserOverrideCheckFunction]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [ValidatePriceAllowUserOverrideCheckFunction]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Function ValidatePriceAllowUserOverrideCheckFunction
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@DrugItemPriceId int,
@DrugItemId int,
@PriceStartDate datetime,         
@PriceStopDate datetime,       
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
@IsFHCC bit,
@DrugItemSubItemId int = null
)

Returns bit

AS

BEGIN
	/* if all flags match, but dates do not match, then this is a valid scenario for the user to */
	/* allow the insert or update to proceed to the next step in which the adjust function will */
	/* resequence the prices by date. Presume this gets called only if there was some overlap detected in */
	/* the matrix and the dates */
	Declare @UserCanOverride bit

	select @UserCanOverride = 0

	if exists ( select Price from DI_DrugItemPrice 
				where DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null  
					and IsTemporary = @IsTemporary
					and	IsFSS = @IsFSS                                  	                  
					and	IsBIG4 = @IsBIG4
					and	IsVA = @IsVA                                         	                  
					and	IsBOP = @IsBOP
					and	IsCMOP = @IsCMOP
					and	IsDOD = @IsDOD
					and	IsHHS = @IsHHS                                           	                  
					and	IsIHS = @IsIHS                                           	                  
					and	IsIHS2 = @IsIHS2
					and	IsDIHS = @IsDIHS                                          	                  
					and	IsNIH = @IsNIH                                          	                  
					and	IsPHS = @IsPHS                                           	                  
					and	IsSVH = @IsSVH                                           	                  
					and	IsSVH1 = @IsSVH1
					and	IsSVH2 = @IsSVH2
					and	IsTMOP = @IsTMOP                                          	                  
					and	IsUSCG = @IsUSCG  
					and IsFHCC = @IsFHCC   
					and ( PriceStartDate <> @PriceStartDate or PriceStopDate <> @PriceStopDate ) )

		or exists ( select Price from DI_DrugItemPrice 
					where DrugItemId = @DrugItemId
						and DrugItemPriceId <> @DrugItemPriceId
						and DrugItemSubItemId is not null 
						and @DrugItemSubItemId is not null
						and DrugItemSubItemId = @DrugItemSubItemId  
						and IsTemporary = @IsTemporary
						and	IsFSS = @IsFSS                                  	                  
						and	IsBIG4 = @IsBIG4
						and	IsVA = @IsVA                                         	                  
						and	IsBOP = @IsBOP
						and	IsCMOP = @IsCMOP
						and	IsDOD = @IsDOD
						and	IsHHS = @IsHHS                                           	                  
						and	IsIHS = @IsIHS                                           	                  
						and	IsIHS2 = @IsIHS2
						and	IsDIHS = @IsDIHS                                          	                  
						and	IsNIH = @IsNIH                                          	                  
						and	IsPHS = @IsPHS                                           	                  
						and	IsSVH = @IsSVH                                           	                  
						and	IsSVH1 = @IsSVH1
						and	IsSVH2 = @IsSVH2
						and	IsTMOP = @IsTMOP                                          	                  
						and	IsUSCG = @IsUSCG 
						and IsFHCC = @IsFHCC    
						and ( PriceStartDate <> @PriceStartDate or PriceStopDate <> @PriceStopDate ) )
	BEGIN
		select @UserCanOverride = 1
	END
	else
	BEGIN
		select @UserCanOverride = 0
	END

	return @UserCanOverride 
	
END	