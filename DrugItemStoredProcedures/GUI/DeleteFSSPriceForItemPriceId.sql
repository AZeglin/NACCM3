IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DeleteFSSPriceForItemPriceId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [DeleteFSSPriceForItemPriceId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[DeleteFSSPriceForItemPriceId]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ModificationStatusId int,
@DrugItemPriceId int
)

As
	Declare @error int,@errorMsg nvarchar(1000), @UserName nvarchar(120), @retVal int,
			@DrugItemId int,@PriceStartDate datetime,@PriceStopDate datetime,@Price decimal(18,2),
			@IsTemporary bit,@IsFSS bit,@IsBIG4 bit,@IsVA bit,@IsBOP bit,@IsCMOP bit,@IsDOD bit,
			@IsHHS bit,@IsIHS bit,@IsIHS2 bit,@IsDIHS bit,@IsNIH bit,@IsPHS bit,@IsSVH bit,@IsSVH1 bit,
			@IsSVH2 bit,@IsTMOP bit,@IsUSCG bit,@IsFHCC bit,@AwardedFSSTrackingCustomerRatio decimal(9,2),
			@TrackingCustomerName nvarchar(120),@CurrentTrackingCustomerPrice decimal(9,2),
			@ExcludeFromExport bit,@LastModificationType nchar(1),@Notes nvarchar(2000)

	Begin Tran
	
		EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @UserName OUTPUT 

		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error getting current user login during delete of item price for @DrugItemPriceId=' + convert( nvarchar(20), @DrugItemPriceId )
			goto ERROREXIT
		END

		Select 	@DrugItemId = DrugItemId,
				@PriceStartDate = PriceStartDate,
				@PriceStopDate = PriceStopDate,
				@Price = Price,
				@IsTemporary = IsTemporary,
				@IsFSS = IsFSS,
				@IsBIG4 = IsBIG4,
				@IsVA = IsVA,
				@IsBOP = IsBOP,
				@IsCMOP = IsCMOP,
				@IsDOD = IsDOD,
				@IsHHS = IsHHS,
				@IsIHS = IsIHS,
				@IsIHS2 =IsIHS2,
				@IsDIHS =IsDIHS,
				@IsNIH = IsNIH,
				@IsPHS = IsPHS,
				@IsSVH = IsSVH,
				@IsSVH1 = IsSVH1,
				@IsSVH2 = IsSVH2,
				@IsTMOP = IsTMOP,
				@IsUSCG = IsUSCG,
				@IsFHCC = IsFHCC,
				@AwardedFSSTrackingCustomerRatio = AwardedFSSTrackingCustomerRatio,
				@TrackingCustomerName = TrackingCustomerName,
				@CurrentTrackingCustomerPrice = CurrentTrackingCustomerPrice,
				@ExcludeFromExport = ExcludeFromExport,
				@LastModificationType = LastModificationType				
		From DI_DrugItemPrice
		Where DrugItemPriceId = @DrugItemPriceId

		Select @Notes = 'DeleteFSSPriceForItemPriceId;'

		Exec @retVal = dbo.InsertFSSDrugItemPriceHistory 
							@UserName,@ContractNumber,@DrugItemId,@DrugItemPriceId,
							@PriceStartDate,@PriceStopDate,@Price,@IsTemporary,
							@IsFSS,@IsBIG4,@IsVA,@IsBOP,@IsCMOP,@IsDOD,@IsHHS,@IsIHS,
							@IsIHS2,@IsDIHS,@IsNIH,@IsPHS,@IsSVH,@IsSVH1,@IsSVH2,@IsTMOP,
							@IsUSCG,@IsFHCC,@AwardedFSSTrackingCustomerRatio,@TrackingCustomerName,
							@CurrentTrackingCustomerPrice,@ExcludeFromExport,@LastModificationType,
							@ModificationStatusId,@Notes	

		SELECT @error = @@ERROR
		IF @retVal = -1 OR @error > 0
		BEGIN
			select @errorMsg = 'Error returned when Inserting price history for contract ' + @ContractNumber
  			GOTO ERROREXIT
		END

		Delete From DI_DrugItemPrice
		Where DrugItempriceId = @DrugItemPriceId

		SELECT @error = @@ERROR
		IF  @error <> 0
		BEGIN
			select @errorMsg = 'Error returned when deleting from DI_DrugItemPrice  for @DrugItemPriceId=' + convert( nvarchar(20), @DrugItemPriceId )
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
				/* only rollback iff this the highest level */
				ROLLBACK TRANSACTION
		  END

		  RETURN( -1 )

	OKEXIT:
		If @@TRANCOUNT > 0
		BEGIN
			COMMIT TRANSACTION
		END
		RETURN( 0 )
