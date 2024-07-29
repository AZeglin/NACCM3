IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'RestoreHistoricalItem' )
BEGIN
	DROP PROCEDURE RestoreHistoricalItem
END
GO

CREATE PROCEDURE RestoreHistoricalItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ItemHistoryId int,
@ContractId int,
@ItemId int,
@ModificationStatusId int,
@LastModificationType nchar(1)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserLogin nvarchar(120),
		@restoredItemId int
		
BEGIN TRANSACTION

	exec dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 
	Select @error = @@error		
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert( nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	if exists( select ItemId from CM_Items where ItemId = @ItemId and ContractId = @ContractId )	
	BEGIN
		select @errorMsg = 'Error restoring historical item. An item with the same item id already exists for this contract. Please use and/or edit the currently active item.'
		goto ERROREXIT
	END
	else
	BEGIN

		insert into CM_Items
		( ContractId, CatalogNumber, ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor,
			ItemDescription, [SIN], ServiceCategoryId, PackageAsPriced, ParentItemId, 
			LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationdate )
		select  ContractId, CatalogNumber, ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor, 
			ItemDescription, SIN, ServiceCategoryId, PackageAsPriced, ParentItemId, 
			'R', @ModificationStatusId, @currentUserLogin, getdate(), @currentUserLogin, getdate() 
		from CM_ItemsHistory
		where ContractId = @ContractId
		and ItemId = @ItemId
		and ItemHistoryId = @ItemHistoryId


		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @restoredItemId = SCOPE_IDENTITY()
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error restoring item to contract.'
			goto ERROREXIT
		END

		delete CM_ItemsHistory
		where ItemHistoryId = @ItemHistoryId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error removing restored item from history.'
			goto ERROREXIT
		END

		/* retain traceability with existing prices */
		update CM_ItemPriceHistory
		set ItemId = @restoredItemId,
		Notes = Notes + 'RestoreHistoricalItem;OldItemId=' + convert( nvarchar(20), @ItemId )
		where ItemId = @ItemId

		select @error = @@ERROR

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error retaining item traceability in price history table.'
			goto ERROREXIT   
		END

		/* restore item countries */
		if exists( select ItemCountryHistoryId from CM_ItemCountriesHistory where ItemId = @ItemId and ReasonMovedToHistory = 'R' )
		BEGIN
			
			-- R = restored
			insert into CM_ItemCountries
			( ItemId, CountryId, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
			select @restoredItemId, CountryId, 'R', -1, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate
			from CM_ItemCountriesHistory
			where ItemId = @ItemId 
			and ReasonMovedToHistory = 'R' -- R = item was removed

			select @error = @@ERROR

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error restoring item countries from history.'
				goto ERROREXIT   
			END

			delete CM_ItemCountriesHistory
			where ItemId = @ItemId 
			and ReasonMovedToHistory = 'R' 

			select @error = @@ERROR

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting restored item countries from history.'
				goto ERROREXIT   
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

