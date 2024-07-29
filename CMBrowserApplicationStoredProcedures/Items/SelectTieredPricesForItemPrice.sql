IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectTieredPricesForItemPrice' )
BEGIN
	DROP PROCEDURE SelectTieredPricesForItemPrice
END
GO

CREATE PROCEDURE SelectTieredPricesForItemPrice
(
@CurrentUser uniqueidentifier,
@ItemPriceId int,
@ContractExpirationDate datetime,
@ItemPriceHistoryId int = 0,
@IsFromHistory bit = 0,
@WithAdd int = 0
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserName nvarchar(120),
		@ModificationStatusId int,		
		@TieredPriceStartDate datetime,
		@TieredPriceStopDate datetime,
		@Price decimal(18,2),
		@TierSequence int,
		@TierCriteria nvarchar(255),
		@MinimumValue int,
		@ItemPriceIdForInsert int,
		@ItemTieredPriceId int,
		@LastModificationType nchar(1),
		@LastModifiedBy nvarchar(120),
		@LastModificationDate datetime
		
BEGIN TRANSACTION

	if @IsFromHistory = 0
	BEGIN
		if @WithAdd = 0
		BEGIN

			select  ItemTieredPriceId, ItemPriceId, TieredPriceStartDate, TieredPriceStopDate, Price, TierSequence, TierCriteria, MinimumValue,	LastModificationType, LastModifiedBy, LastModificationDate
			from CM_ItemTieredPrice
			where ItemPriceId = @ItemPriceId

			order by TierSequence

			select @error = @@ERROR

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error encountered when attempting to retrieve tiered prices.'
				goto ERROREXIT
			END
		END
		else 
		BEGIN
			select @ItemTieredPriceId = -1,
				@ItemPriceIdForInsert = @ItemPriceId,
				@TieredPriceStartDate = getdate(),
				@TieredPriceStopDate = @ContractExpirationDate,
				@Price = 0,
				@TierSequence = 1,
				@TierCriteria = '',
				@MinimumValue = 0,
				@LastModificationType = '',
				@LastModifiedBy = '',
				@LastModificationDate = getdate()

			select @ItemTieredPriceId as ItemTieredPriceId,
				@ItemPriceIdForInsert as ItemPriceId,
				@TieredPriceStartDate as TieredPriceStartDate,
				@TieredPriceStopDate as TieredPriceStopDate,
				@Price as Price,
				@TierSequence as TierSequence,
				@TierCriteria as TierCriteria,
				@MinimumValue as MinimumValue,
				@LastModificationType as LastModificationType,
				@LastModifiedBy as LastModifiedBy,
				@LastModificationDate as LastModificationDate

			union

			select  ItemTieredPriceId, ItemPriceId, TieredPriceStartDate, TieredPriceStopDate, Price, TierSequence, TierCriteria, MinimumValue,	LastModificationType, LastModifiedBy, LastModificationDate
			from CM_ItemTieredPrice
			where ItemPriceId = @ItemPriceId

			order by ItemTieredPriceId

			select @error = @@ERROR

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error encountered when attempting to retrieve tiered prices.'
				goto ERROREXIT
			END

		END
	END
	else
	BEGIN
		select  ItemTieredPriceId, ItemPriceId, TieredPriceStartDate, TieredPriceStopDate, Price, TierSequence, TierCriteria, MinimumValue,	LastModificationType, LastModifiedBy, LastModificationDate
		from CM_ItemTieredPriceHistory
		where ItemPriceId = @ItemPriceId

		order by TieredPriceStartDate

		select @error = @@ERROR

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error encountered when attempting to retrieve historical tiered prices.'
			goto ERROREXIT
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





