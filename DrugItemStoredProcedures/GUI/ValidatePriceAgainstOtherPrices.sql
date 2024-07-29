IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ValidatePriceAgainstOtherPrices]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ValidatePriceAgainstOtherPrices]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure ValidatePriceAgainstOtherPrices
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
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
@DrugItemSubItemId int = null,
@IsPriceOk bit OUTPUT,
@UserCanOverride bit OUTPUT,
@ValidationMessage nvarchar(1250) OUTPUT                           	                  
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@currentUserName nvarchar(120),
	@OrganizationCode nvarchar(4),
	@TotalPriceProblemsEncountered bit,
	@TPRAlwaysHasBasePrice bit

BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserName OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during update of item price for fss contract for contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	/* check the input price info for another price with the same pattern. Duplicates are not allowed */
	/* check each IsXXX in turn to see if there is another like it with overlapping or same date range */
	/* for a particular item */
	select @IsPriceOk = 0
	select @UserCanOverride = 0
	select @TotalPriceProblemsEncountered = 0
	select @ValidationMessage = 'Price validation failed for the following selections: '


	if @IsFSS = 1
	BEGIN
		select @OrganizationCode = 'FSS'

		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsBIG4 = 1
	BEGIN
		select @OrganizationCode = 'BIG4'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsVA = 1
	BEGIN
		select @OrganizationCode = 'VA'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsBOP = 1
	BEGIN
		select @OrganizationCode = 'BOP'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsCMOP = 1
	BEGIN
		select @OrganizationCode = 'CMOP'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsDOD = 1
	BEGIN
		select @OrganizationCode = 'DOD'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsHHS = 1
	BEGIN
		select @OrganizationCode = 'HHS'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsIHS = 1
	BEGIN
		select @OrganizationCode = 'IHS'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsIHS2 = 1
	BEGIN
		select @OrganizationCode = 'IHS2'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsDIHS = 1
	BEGIN
		select @OrganizationCode = 'DIHS'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsNIH = 1
	BEGIN
		select @OrganizationCode = 'NIH'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsPHS = 1
	BEGIN
		select @OrganizationCode = 'PHS'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsSVH = 1
	BEGIN
		select @OrganizationCode = 'SVH'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsSVH1 = 1
	BEGIN
		select @OrganizationCode = 'SVH1'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsSVH2 = 1
	BEGIN
		select @OrganizationCode = 'SVH2'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	if @IsTMOP = 1
	BEGIN
		select @OrganizationCode = 'TMOP'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	
	if @IsUSCG = 1
	BEGIN
		select @OrganizationCode = 'USCG'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END

	if @IsFHCC = 1
	BEGIN
		select @OrganizationCode = 'FHCC'
		select @IsPriceOk = dbo.ValidatePriceCheckOneOrganizationFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, @PriceStartDate, @PriceStopDate, @IsTemporary, @OrganizationCode, @DrugItemSubItemId )

		select @error = @@error
		select @errorMsg = 'Error validating drug item price for ' + @OrganizationCode
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
		
		if @IsPriceOk <> 1
		BEGIN
			select @ValidationMessage = @ValidationMessage + ' ' + @OrganizationCode
			select @TotalPriceProblemsEncountered = @TotalPriceProblemsEncountered + 1
		END		
	END
	
	if @TotalPriceProblemsEncountered > 0
	BEGIN
		select @IsPriceOk = 0
		
		/* determine if user can override and let the split take over */
		select @UserCanOverride = dbo.ValidatePriceAllowUserOverrideCheckFunction( @CurrentUser, @ContractNumber, @DrugItemPriceId, @DrugItemId, 
						@PriceStartDate, @PriceStopDate, @IsTemporary, @IsFSS, @IsBIG4,
								@IsVA, @IsBOP, @IsCMOP, @IsDOD, @IsHHS, @IsIHS, @IsIHS2, @IsDIHS,
								@IsNIH, @IsPHS, @IsSVH, @IsSVH1, @IsSVH2, @IsTMOP, @IsUSCG, @IsFHCC, @DrugItemSubItemId )
		
		select @error = @@error
		select @errorMsg = 'Error validating drug item price to allow user override'
		
		if @error <> 0 
		BEGIN
			goto ERROREXIT
		END
	END
	else
	BEGIN
		/* if TPR, check for base price */
		if @IsTemporary = 1
		BEGIN
		select @TPRAlwaysHasBasePrice = dbo.GetTPRAlwaysHasBasePriceFunction( @DrugItemId,          
									@PriceStartDate,         
									@PriceStopDate,       
									@IsTemporary,                                     	                  
									@IsFSS,                                           	                  
									@IsBIG4,                                          	                  
									@IsVA,                                            	                  
									@IsBOP,                                           	                  
									@IsCMOP,                                          	                  
									@IsDOD,                                           	                  
									@IsHHS,                                           	                  
									@IsIHS,                                           	                  
									@IsIHS2,                                          	                  
									@IsDIHS,                                          	                  
									@IsNIH,                                           	                  
									@IsPHS,                                           	                  
									@IsSVH,                                           	                  
									@IsSVH1,                                          	                  
									@IsSVH2,                                          	                  
									@IsTMOP,                                          	                  
									@IsUSCG,
									@IsFHCC ) 
			if @TPRAlwaysHasBasePrice = 0
			BEGIN
				select @IsPriceOk = 1
				select @UserCanOverride = 1
				/* code keys off of 'base price' being in this message */
				select @ValidationMessage = 'An active base price was not defined for the current TPR. Please add a corresponding base price.' 
			END
		END
	END

goto OKEXIT


ERROREXIT:
	raiserror( @errorMsg, 16, 1 )
	
  	if @@TRANCOUNT > 1
  	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 

ENDEXIT:








