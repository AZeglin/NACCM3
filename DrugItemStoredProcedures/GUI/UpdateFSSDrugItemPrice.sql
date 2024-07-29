IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UpdateFSSDrugItemPrice]') AND type in (N'P', N'PC'))
DROP PROCEDURE [UpdateFSSDrugItemPrice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure UpdateFSSDrugItemPrice
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemPriceId int,
@PriceStartDate datetime,         
@PriceEndDate datetime,       
@Price decimal(9,2),   
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
@ModificationStatusId int,
@DrugItemSubItemId int = null                                    	                  
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@currentUserName nvarchar(120),
	@retVal int,
	@Notes nvarchar(2000),	
	@ExistingPriceStartDate datetime,         
	@ExistingIsFSS bit,                                           	                  
	@ExistingIsBIG4 bit,
	@ExistingDrugItemId int
	
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

	Select @Notes = 'UpdateFSSDrugItemPrice;'

	Exec @retVal = CopyItemPriceIntoPriceHistory @CurrentUser, @SecurityServerName, @SecurityDatabaseName, 
								@ContractNumber, @ModificationStatusId, @DrugItemPriceId, @Notes

	SELECT @error = @@ERROR
	IF @retVal = -1 OR @error > 0
	BEGIN
		select @errorMsg = 'Error returned when Inserting price history for contract ' + @ContractNumber
  		GOTO ERROREXIT
	END

	select @ExistingDrugItemId = DrugItemId,
		@ExistingPriceStartDate = PriceStartDate,
		@ExistingIsFSS = IsFSS,                                           	                  
		@ExistingIsBIG4 = IsBIG4
	from DI_DrugItemPrice
	where DrugItemPriceId = @DrugItemPriceId

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting existing drug item price info for DLA log for contract ' + @ContractNumber
		goto ERROREXIT
	END


	update DI_DrugItemPrice
	set PriceStartDate = @PriceStartDate,
		PriceStopDate = @PriceEndDate,
		Price = @Price,
		IsTemporary = @IsTemporary,
		IsFSS = @IsFSS,                                           	                  
		IsBIG4 = @IsBIG4,                                          	                  
		IsVA = @IsVA,                                            	                  
		IsBOP = @IsBOP,                                           	                  
		IsCMOP = @IsCMOP,                                          	                  
		IsDOD = @IsDOD,                                           	                  
		IsHHS = @IsHHS,                                           	                  
		IsIHS = @IsIHS,                                           	                  
		IsIHS2 = @IsIHS2,                                          	                  
		IsDIHS = @IsDIHS,                                          	                  
		IsNIH = @IsNIH,                                           	                  
		IsPHS = @IsPHS,                                           	                  
		IsSVH = @IsSVH,                                           	                  
		IsSVH1 = @IsSVH1,                                          	                  
		IsSVH2 = @IsSVH2,                                          	                  
		IsTMOP = @IsTMOP,                                          	                  
		IsUSCG = @IsUSCG, 
		IsFHCC = @IsFHCC,    
		DrugItemSubItemId = @DrugItemSubItemId,
		ModificationStatusId = @ModificationStatusId,
		LastModifiedBy = @currentUserName,
		LastModificationDate = getdate(),
		LastModificationType = 'C'

	where DrugItemPriceId = @DrugItemPriceId

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error updating drug item price for fss contract ' + @ContractNumber
		goto ERROREXIT
	END


	Exec @retVal = dbo.AdjustPriceSequence @DrugItemPriceId,@ModificationStatusId,@currentUserName, @ContractNumber, 'G'

	SELECT @error = @@ERROR
	IF @retVal = -1 OR @error > 0
	BEGIN
		select @errorMsg = 'Error returned when Adjusting price sqeuence for contract ' + @ContractNumber
  		GOTO ERROREXIT
	END

	Exec @retVal = InsertDLAPriceChangeLog @currentUserName, @ContractNumber, 'G', 'B', @ExistingDrugItemId,
			@DrugItemPriceId, @DrugItemPriceId, 
			@PriceStartDate, @ExistingPriceStartDate,
			@IsFSS, @ExistingIsFSS,
			@IsBIG4, @ExistingIsBIG4

	SELECT @error = @@ERROR

	IF @retVal = -1 OR @error > 0
	BEGIN
		select @errorMsg = 'Error returned when calling InsertDLAPriceChangeLog for contract ' + @ContractNumber
  		GOTO ERROREXIT
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







