IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ValidatePriceCheckOneOrganizationFunction]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [ValidatePriceCheckOneOrganizationFunction]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Function ValidatePriceCheckOneOrganizationFunction
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@DrugItemPriceId int,
@DrugItemId int,
@PriceStartDate datetime,         
@PriceStopDate datetime,       
@IsTemporary bit,   
@OrganizationCode nvarchar( 4 ),
@DrugItemSubItemId int = null
)

Returns bit

AS

/* List of possible organizations
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
*/

BEGIN

	Declare @IsPriceOk bit

	select @IsPriceOk = 0

	if @OrganizationCode = 'FSS'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsFSS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsFSS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	else if @OrganizationCode = 'BIG4'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsBIG4 = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsBIG4 = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )

		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	
	else if @OrganizationCode = 'VA'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsVA = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsVA = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )

		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
		
	else if @OrganizationCode = 'BOP'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsBOP = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsBOP = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )

		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	
	else if @OrganizationCode = 'CMOP'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsCMOP = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsCMOP = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )

		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
		
	else if @OrganizationCode = 'DOD'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsDOD = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsDOD = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )

		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
			
	else if @OrganizationCode = 'HHS'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsHHS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsHHS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	else if @OrganizationCode = 'IHS'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsIHS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsIHS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	else if @OrganizationCode = 'IHS2'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsIHS2 = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary 
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsIHS2 = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	else if @OrganizationCode = 'DIHS'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsDIHS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsDIHS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	
	
	else if @OrganizationCode = 'NIH'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsNIH = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary 
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsNIH = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	else if @OrganizationCode = 'PHS'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsPHS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary 
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsPHS = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	else if @OrganizationCode = 'SVH'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsSVH = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsSVH = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
		
	else if @OrganizationCode = 'SVH1'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsSVH1 = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary 
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsSVH1 = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
    else if @OrganizationCode = 'SVH2'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsSVH2 = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary 
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsSVH2 = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	else if @OrganizationCode = 'TMOP'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsTMOP = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary 
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsTMOP = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	else if @OrganizationCode = 'USCG'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsUSCG = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsUSCG = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	else if @OrganizationCode = 'FHCC'
	BEGIN
		if exists ( select Price from DI_DrugItemPrice 
					where IsFHCC = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId is null 
					and @DrugItemSubItemId is null ) 
			or exists ( select Price from DI_DrugItemPrice 
					where IsFHCC = 1
					and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
						PriceStartDate between @PriceStartDate and @PriceStopDate or
						PriceStopDate between @PriceStartDate and @PriceStopDate or
						( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
						( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
						PriceStartDate = @PriceStopDate  or
						PriceStopDate = @PriceStartDate )
					and DrugItemId = @DrugItemId
					and DrugItemPriceId <> @DrugItemPriceId
					and IsTemporary = @IsTemporary
					and DrugItemSubItemId = @DrugItemSubItemId
					and DrugItemSubItemId is not null
					and @DrugItemSubItemId is not null )
		BEGIN
			select @IsPriceOk = 0
		END
		else
		BEGIN
			select @IsPriceOk = 1
		END
	END
	
	return @IsPriceOk
	
END



