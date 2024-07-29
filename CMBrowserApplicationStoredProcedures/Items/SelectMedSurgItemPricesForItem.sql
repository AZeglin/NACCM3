IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectMedSurgItemPricesForItem' )
BEGIN
	DROP PROCEDURE SelectMedSurgItemPricesForItem
END
GO

CREATE PROCEDURE SelectMedSurgItemPricesForItem
(
@CurrentUser uniqueidentifier,
@FutureHistoricalSelectionCriteria nvarchar(1),  -- A = Active, F = Future, B = Both active and future, H = Historical
@ContractNumber nvarchar(20),
@ContractId int,
@ContractExpirationDate datetime,
@ItemId int,
@WithAddPrice bit = 0
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@ItemPriceHistoryId int,
		@ItemPriceId int, 
		@PriceId int, 
		@PriceStartDate datetime, 
		@PriceStopDate datetime, 
		@Price decimal(18,2), 
		@IsBPA bit, 
		@IsTemporary bit,
		@IsFromHistory bit,
		@TPRAlwaysHasBasePrice bit,
		@LastModificationType nchar(1), 
		@ModificationStatusId int, 
		@LastModifiedBy nvarchar(120), 
		@LastModificationDate datetime,
		@ReasonMovedToHistory nvarchar(30),
		@MovedToHistoryBy nvarchar(120),
		@DateMovedToHistory datetime


BEGIN TRANSACTION

	-- historical only
	if @FutureHistoricalSelectionCriteria = 'H'
	BEGIN
		select ItemPriceHistoryId, ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, 1 as IsFromHistory, 0 as TPRAlwaysHasBasePrice, LastModificationType, ModificationStatusId, LastModifiedBy, LastModificationDate, 
			case when ( PATINDEX( '%DeleteMedSurgItemAndPrices%', Notes ) > 0 ) then 'Deleted With Item' else 
				case when ( PATINDEX( '%DeleteMedSurgPriceForItemPriceId%', Notes ) > 0 ) then 'Deleted' else 
				case when ( PATINDEX( '%UpdateMedSurgItemPrice%', Notes ) > 0 ) then 'Updated' else 
				case when ( PATINDEX( '%UpdateMedSurgItemPriceDetails%', Notes ) > 0 ) then 'Details Updated' else 
				case when ( PATINDEX( '%DeleteItemForMedSurgItemUpload2%', Notes ) > 0 ) then 'Deleted via Upload'  else 
				case when ( PATINDEX( '%UpdateItemForMedSurgItemUpload2%', Notes ) > 0 ) then 'Updated via Upload' else 
				case when ( PATINDEX( '%Initial%', Notes ) > 0 ) then 'Initial Conversion' else
				case when ( PATINDEX( '%NightlyBatchProcess%', Notes ) > 0 ) then 'Price Expired' 
				else 'Unknown' end end end end end end end end as ReasonMovedToHistory,
			case when ( PATINDEX( '%Initial%', MovedToHistoryBy ) > 0 ) then 'Unknown' else MovedToHistoryBy end as MovedToHistoryBy,
			DateMovedToHistory
				
		from CM_ItemPriceHistory
		where ItemId = @ItemId
		order by PriceStopDate desc
	
		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting prices for item (1).'
			goto ERROREXIT
		END
	END
	else
	BEGIN

		if @WithAddPrice = 0
		BEGIN

			if @FutureHistoricalSelectionCriteria = 'A'
			BEGIN
				select -1 as ItemPriceHistoryId, ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, 0 as IsFromHistory, 
				dbo.GetMedSurgTPRAlwaysHasBasePriceFunction( ItemId, PriceStartDate, PriceStopDate, IsTemporary ) as TPRAlwaysHasBasePrice,  
					LastModificationType, ModificationStatusId, LastModifiedBy, LastModificationDate,
					'' as ReasonMovedToHistory,
					'' as MovedToHistoryBy,
					null as DateMovedToHistory
				from CM_ItemPrice
				where ItemId = @ItemId
				and datediff( DD, PriceStartDate, getdate() ) >= 0
					and datediff( DD, getdate(), PriceStopDate ) >= 0
				order by PriceStartDate

				select @error = @@ERROR
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error selecting prices for item (2).'
					goto ERROREXIT
				END

			END
			else if @FutureHistoricalSelectionCriteria = 'B'
			BEGIN
				select -1 as ItemPriceHistoryId, ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, 0 as IsFromHistory, 
					dbo.GetMedSurgTPRAlwaysHasBasePriceFunction( ItemId, PriceStartDate, PriceStopDate, IsTemporary ) as TPRAlwaysHasBasePrice,  
					LastModificationType, ModificationStatusId, LastModifiedBy, LastModificationDate,
					'' as ReasonMovedToHistory,
					'' as MovedToHistoryBy,
					null as DateMovedToHistory
				from CM_ItemPrice
				where ItemId = @ItemId
				and datediff( DD, getdate(), PriceStopDate ) >= 0
				order by PriceStartDate

				select @error = @@ERROR
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error selecting prices for item (3).'
					goto ERROREXIT
				END

			END
			else if @FutureHistoricalSelectionCriteria = 'F'
			BEGIN
				select -1 as ItemPriceHistoryId, ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, 0 as IsFromHistory, 
					dbo.GetMedSurgTPRAlwaysHasBasePriceFunction( ItemId, PriceStartDate, PriceStopDate, IsTemporary ) as TPRAlwaysHasBasePrice,  
					LastModificationType, ModificationStatusId, LastModifiedBy, LastModificationDate,
					'' as ReasonMovedToHistory,
					'' as MovedToHistoryBy,
					null as DateMovedToHistory
				from CM_ItemPrice
				where ItemId = @ItemId
				and datediff( DD, getdate(), PriceStartDate  ) >= 0
					and datediff( DD, getdate(), PriceStopDate ) >= 0
				order by PriceStartDate

				select @error = @@ERROR
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error selecting prices for item (4).'
					goto ERROREXIT
				END
			END
		END
		else  -- adding
		BEGIN
			select  @ItemPriceHistoryId = -1,
				@ItemPriceId = -1,
				@PriceId = -1,
				@PriceStartDate = getdate(),
				@PriceStopDate = @ContractExpirationDate,
				@Price = 0,
				@IsBPA = 0,
				@IsTemporary = 0,
				@IsFromHistory = 0,
				@TPRAlwaysHasBasePrice = 0,
				@LastModificationType = '',
				@ModificationStatusId  = -1,
				@LastModifiedBy = '',
				@LastModificationDate = getdate(),
				@ReasonMovedToHistory = '',
				@MovedToHistoryBy = '',
				@DateMovedToHistory = getdate()

			if @FutureHistoricalSelectionCriteria = 'A'
			BEGIN

				select
					@ItemPriceHistoryId as ItemPriceHistoryId,
					@ItemPriceId as ItemPriceId,
					@ItemId as ItemId,
					@PriceId as PriceId,
					@PriceStartDate as PriceStartDate,
					@PriceStopDate as PriceStopDate,
					@Price as Price,
					@IsBPA as IsBPA,
					@IsTemporary as IsTemporary,
					@IsFromHistory as IsFromHistory,
					@TPRAlwaysHasBasePrice as TPRAlwaysHasBasePrice,
					@LastModificationType as LastModificationType,
					@ModificationStatusId as ModificationStatusId,
					@LastModifiedBy as LastModifiedBy,
					@LastModificationDate as LastModificationDate,
					@ReasonMovedToHistory as ReasonMovedToHistory,
					@MovedToHistoryBy as MovedToHistoryBy,
					@DateMovedToHistory as DateMovedToHistory

				union

				select -1 as ItemPriceHistoryId, ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, 0 as IsFromHistory, 
				dbo.GetMedSurgTPRAlwaysHasBasePriceFunction( ItemId, PriceStartDate, PriceStopDate, IsTemporary ) as TPRAlwaysHasBasePrice,  
					LastModificationType, ModificationStatusId, LastModifiedBy, LastModificationDate,
					'' as ReasonMovedToHistory,
					'' as MovedToHistoryBy,
					null as DateMovedToHistory
				from CM_ItemPrice
				where ItemId = @ItemId
				and datediff( DD, PriceStartDate, getdate() ) >= 0
					and datediff( DD, getdate(), PriceStopDate ) >= 0
			
				order by ItemPriceId, PriceStartDate

				select @error = @@ERROR
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error selecting prices for item (5).'
					goto ERROREXIT
				END

			END
			else if @FutureHistoricalSelectionCriteria = 'B'
			BEGIN
				select
					@ItemPriceHistoryId as ItemPriceHistoryId,
					@ItemPriceId as ItemPriceId,
					@ItemId as ItemId,
					@PriceId as PriceId,
					@PriceStartDate as PriceStartDate,
					@PriceStopDate as PriceStopDate,
					@Price as Price,
					@IsBPA as IsBPA,
					@IsTemporary as IsTemporary,
					@IsFromHistory as IsFromHistory,
					@TPRAlwaysHasBasePrice as TPRAlwaysHasBasePrice,
					@LastModificationType as LastModificationType,
					@ModificationStatusId as ModificationStatusId,
					@LastModifiedBy as LastModifiedBy,
					@LastModificationDate as LastModificationDate,
					@ReasonMovedToHistory as ReasonMovedToHistory,
					@MovedToHistoryBy as MovedToHistoryBy,
					@DateMovedToHistory as DateMovedToHistory

				union

				select -1 as ItemPriceHistoryId, ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, 0 as IsFromHistory, 
				dbo.GetMedSurgTPRAlwaysHasBasePriceFunction( ItemId, PriceStartDate, PriceStopDate, IsTemporary ) as TPRAlwaysHasBasePrice,
					LastModificationType, ModificationStatusId, LastModifiedBy, LastModificationDate,
					'' as ReasonMovedToHistory,
					'' as MovedToHistoryBy,
					null as DateMovedToHistory
				from CM_ItemPrice
				where ItemId = @ItemId
				and datediff( DD, getdate(), PriceStopDate ) >= 0

				order by ItemPriceId, PriceStartDate
				
				select @error = @@ERROR
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error selecting prices for item (6).'
					goto ERROREXIT
				END
			END
			else if @FutureHistoricalSelectionCriteria = 'F'
			BEGIN
				select
					@ItemPriceHistoryId as ItemPriceHistoryId,
					@ItemPriceId as ItemPriceId,
					@ItemId as ItemId,
					@PriceId as PriceId,
					@PriceStartDate as PriceStartDate,
					@PriceStopDate as PriceStopDate,
					@Price as Price,
					@IsBPA as IsBPA,
					@IsTemporary as IsTemporary,
					@IsFromHistory as IsFromHistory,
					@TPRAlwaysHasBasePrice as TPRAlwaysHasBasePrice,
					@LastModificationType as LastModificationType,
					@ModificationStatusId as ModificationStatusId,
					@LastModifiedBy as LastModifiedBy,
					@LastModificationDate as LastModificationDate,
					@ReasonMovedToHistory as ReasonMovedToHistory,
					@MovedToHistoryBy as MovedToHistoryBy,
					@DateMovedToHistory as DateMovedToHistory

				union

				select -1 as ItemPriceHistoryId, ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, 0 as IsFromHistory, 
				dbo.GetMedSurgTPRAlwaysHasBasePriceFunction( ItemId, PriceStartDate, PriceStopDate, IsTemporary ) as TPRAlwaysHasBasePrice,  
				LastModificationType, ModificationStatusId, LastModifiedBy, LastModificationDate,
					'' as ReasonMovedToHistory,
					'' as MovedToHistoryBy,
					null as DateMovedToHistory
				from CM_ItemPrice
				where ItemId = @ItemId
				and datediff( DD, getdate(), PriceStartDate  ) >= 0
					and datediff( DD, getdate(), PriceStopDate ) >= 0
				
				order by ItemPriceId, PriceStartDate
				
				select @error = @@ERROR
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error selecting prices for item (7).'
					goto ERROREXIT
				END
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
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )

