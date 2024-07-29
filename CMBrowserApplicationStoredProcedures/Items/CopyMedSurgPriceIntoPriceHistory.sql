IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[CopyMedSurgPriceIntoPriceHistory]') AND type in (N'P', N'PC'))
DROP PROCEDURE [CopyMedSurgPriceIntoPriceHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[CopyMedSurgPriceIntoPriceHistory]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ModificationStatusId int,
@ItemPriceId int,
@Notes nvarchar(2000)
)

As
	Declare @error int,
			@errorMsg nvarchar(1000), 
			@UserName nvarchar(120), 
			@retVal int,
			@ItemId int,
			@PriceStartDate datetime,
			@PriceStopDate datetime,
			@Price decimal(18,2),
						
			@IsTemporary bit,
			@TrackingCustomerPrice decimal(10,2),
			@TrackingCustomerRatio nvarchar(100),
			@TrackingCustomerName nvarchar(100),
			@TrackingCustomerFOBTerms nvarchar(40),

			@LastModificationType nchar(1), 
			@IsBPA bit,
			
			@CreatedBy nvarchar(120),
			@CreationDate datetime,
			@LastModifiedBy nvarchar(120),
			@LastModificationDate datetime

	Begin Tran
	
		EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @UserName OUTPUT 

		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error getting current user login during delete of item price for @ItemPriceId=' + convert( nvarchar(20), @ItemPriceId )
			goto ERROREXIT
		END		

		Select 	@ItemId = ItemId,
				@PriceStartDate = PriceStartDate,
				@PriceStopDate = PriceStopDate,
				@Price = Price,
			
				@IsBPA = IsBPA,
				@IsTemporary = IsTemporary,
								
				@TrackingCustomerPrice = TrackingCustomerPrice,
				@TrackingCustomerRatio = TrackingCustomerRatio,
				@TrackingCustomerName  = TrackingCustomerName,
				@TrackingCustomerFOBTerms = TrackingCustomerFOBTerms,
			
				@LastModificationType = LastModificationType,
				@CreatedBy = CreatedBy,
				@CreationDate = CreationDate,
				@LastModifiedBy = LastModifiedBy,
				@LastModificationDate = LastModificationDate
								
		From CM_ItemPrice
		Where ItemPriceId = @ItemPriceId

		Select @Notes = @Notes + 'CopyItemPriceIntoPriceHistory;'

		Exec @retVal = dbo.InsertMedSurgItemPriceHistory 
							@UserName,@ContractNumber,@ItemId,@ItemPriceId,
							@PriceStartDate,@PriceStopDate,@Price,
							@IsBPA,@IsTemporary, @TrackingCustomerPrice, @TrackingCustomerRatio, @TrackingCustomerName,
							@TrackingCustomerFOBTerms, @LastModificationType,
							@ModificationStatusId,@CreatedBy,@CreationDate,@LastModifiedBy,@LastModificationDate,
							@Notes	

		SELECT @error = @@ERROR
		IF @retVal = -1 OR @error > 0
		BEGIN
			select @errorMsg = 'Error returned when Inserting price history for contract ' + @ContractNumber
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

