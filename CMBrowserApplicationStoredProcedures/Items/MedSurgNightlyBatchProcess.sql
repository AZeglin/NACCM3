IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'MedSurgNightlyBatchProcess' )
BEGIN
	DROP PROCEDURE MedSurgNightlyBatchProcess
END
GO

CREATE PROCEDURE MedSurgNightlyBatchProcess
( 
@DaysToHoldItemWithoutPrice int = 6,
@MostRecentCount int = 20
)
AS

Declare @error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	insert into CM_ItemTieredPriceHistory
	(
		ItemTieredPriceId,
		ItemPriceId,
		TieredPriceStartDate,
		TieredPriceStopDate,
		Price,
		TierSequence,
		TierCriteria,
		MinimumValue,
		LastModificationType,
		ModificationStatusId,
		Removed,
		CreatedBy,
		CreationDate,
		LastModifiedBy,
		LastModificationDate,
		Notes,
		MovedToHistoryBy,
		DateMovedToHistory
	)
	select
		ItemTieredPriceId,
		ItemPriceId,
		TieredPriceStartDate,
		TieredPriceStopDate,
		Price,
		TierSequence,
		TierCriteria,
		MinimumValue,
		LastModificationType,
		ModificationStatusId,
		0,
		CreatedBy,
		CreationDate,
		LastModifiedBy,
		LastModificationDate,
		'MedSurgNightlyBatchProcess',
		'dbo',
		getdate()
	from CM_ItemTieredPrice
	where datediff( DD, getdate(), TieredPriceStopDate ) < 0

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired tiered prices to history (1).'
		goto ERROREXIT
	END

	delete CM_ItemTieredPrice
	where datediff( DD, getdate(), TieredPriceStopDate ) < 0

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired tiered prices to history (D1).'
		goto ERROREXIT
	END

	insert into CM_ItemPriceHistory
	(
		ItemPriceId,
		ItemId,
		PriceId,
		PriceStartDate,
		PriceStopDate,
		Price,
		IsBPA,
		IsTemporary,
		TrackingCustomerPrice,
		TrackingCustomerRatio,
		TrackingCustomerName,
		TrackingCustomerFOBTerms,
		Removed,		
		LastModificationType,
		ModificationStatusId,
		CreatedBy,
		CreationDate,
		LastModifiedBy,
		LastModificationDate,
		Notes,
		MovedToHistoryBy,
		DateMovedToHistory
	)
	select
		ItemPriceId,
		ItemId,
		PriceId,
		PriceStartDate,
		PriceStopDate,
		Price,
		IsBPA,
		IsTemporary,
		TrackingCustomerPrice,
		TrackingCustomerRatio,
		TrackingCustomerName,
		TrackingCustomerFOBTerms,
		0,		
		LastModificationType,
		ModificationStatusId,
		CreatedBy,
		CreationDate,
		LastModifiedBy,
		LastModificationDate,
		'MedSurgNightlyBatchProcess',
		'dbo',
		getdate()
	from CM_ItemPrice
	where datediff( DD, getdate(), PriceStopDate ) < 0

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired prices to history (1).'
		goto ERROREXIT
	END

	create table #TempPricesMovingToHistory
	( 
		ItemPriceId int not null
	)

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired prices to history (C1).'
		goto ERROREXIT
	END

	insert into #TempPricesMovingToHistory
	( ItemPriceId )
	select ItemPriceId
	from CM_ItemPrice
	where datediff( DD, getdate(), PriceStopDate ) < 0

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired prices to history (2).'
		goto ERROREXIT
	END

	delete CM_ItemPrice
	where datediff( DD, getdate(), PriceStopDate ) < 0

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired prices to history (D).'
		goto ERROREXIT
	END

	insert into CM_ItemTieredPriceHistory
	(
		ItemTieredPriceId,
		ItemPriceId,
		TieredPriceStartDate,
		TieredPriceStopDate,
		Price,
		TierSequence,
		TierCriteria,
		MinimumValue,
		LastModificationType,
		ModificationStatusId,
		Removed,
		CreatedBy,
		CreationDate,
		LastModifiedBy,
		LastModificationDate,
		Notes,
		MovedToHistoryBy,
		DateMovedToHistory
	)
	select
		ItemTieredPriceId,
		ItemPriceId,
		TieredPriceStartDate,
		TieredPriceStopDate,
		Price,
		TierSequence,
		TierCriteria,
		MinimumValue,
		LastModificationType,
		ModificationStatusId,
		0,
		CreatedBy,
		CreationDate,
		LastModifiedBy,
		LastModificationDate,
		'MedSurgNightlyBatchProcessDueToMainPriceExp',
		'dbo',
		getdate()
	from CM_ItemTieredPrice
	where ItemPriceId in ( select ItemPriceId from #TempPricesMovingToHistory )

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired tiered prices to history (2).'
		goto ERROREXIT
	END

	delete CM_ItemTieredPrice
	where ItemPriceId in ( select ItemPriceId from #TempPricesMovingToHistory )

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired tiered prices to history (D2).'
		goto ERROREXIT
	END

	create table #TempItemsMovingToHistory
	( 
		ItemId int not null
	)

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired items to history staging (C2).'
		goto ERROREXIT
	END

	insert into #TempItemsMovingToHistory
	( ItemId )
	select distinct ItemId
	from CM_ItemPriceHistory
	where ItemPriceId in ( select ItemPriceId from #TempPricesMovingToHistory )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired items to history staging (1).'
		goto ERROREXIT
	END

	delete #TempItemsMovingToHistory
	where ItemId in ( select ItemId from CM_ItemPrice )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired items to history staging (2).'
		goto ERROREXIT
	END

	-- stage the new ones for a future move
	insert into CM_ItemsHistoryStaging
	( ItemId, ContractId, CatalogNumberAtTimeOfStaging, ParentItemId, DateMovedToHistoryStaging, DateToMoveToHistory, ProcessingComplete )
	select t.ItemId, i.ContractId, i.CatalogNumber, i.ParentItemId, getdate(), DATEADD( DD, @DaysToHoldItemWithoutPrice, getdate()), 0
	from #TempItemsMovingToHistory t join CM_Items i on t.ItemId = i.ItemId

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired items to history staging (3).'
		goto ERROREXIT
	END

	-- stage items which have had prices manually deleted today
	insert into CM_ItemsHistoryStaging
	( ItemId, ContractId, CatalogNumberAtTimeOfStaging, ParentItemId, DateMovedToHistoryStaging, DateToMoveToHistory, ProcessingComplete )
	select i.ItemId, i.ContractId, i.CatalogNumber, i.ParentItemId, getdate(), DATEADD( DD, @DaysToHoldItemWithoutPrice, getdate()), 0
	from CM_Items i 
	where i.ItemId in ( select ItemId from CM_ItemPriceHistory
						where Removed = 1
						and DATEDIFF( DD, LastModificationDate, getdate() ) = 0 )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving items with deleted prices to history staging (4).'
		goto ERROREXIT
	END


	-- unstage staged items which have a new price
	delete CM_ItemsHistoryStaging
	where ItemId in ( select ItemId from CM_ItemPrice )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving expired items to history staging (4).'
		goto ERROREXIT
	END


	-- moved staged items to history

	create table #TempStagedItemsToMove
	(
		ItemId int not null,
		Ordinality int not null
	)

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table when moving staged items to history.'
		goto ERROREXIT
	END

	insert into #TempStagedItemsToMove
	( ItemId, Ordinality )
	select ItemId, 0 
	from CM_ItemsHistoryStaging
	where DateDiff( DD, DateToMoveToHistory, getdate() ) >= 0 
			and ProcessingComplete =  0 

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting from staging table when moving staged items to history.'
		goto ERROREXIT
	END

	update t
	set t.Ordinality = ( select isnull( max( h.Ordinality ), 0 )
						from CM_ItemsHistory h
						where h.ItemId = t.ItemId )
	from #TempStagedItemsToMove t 

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating ordinality when moving staged items to history.'
		goto ERROREXIT
	END

	insert into CM_ItemsHistory
	( ItemId, Ordinality, ContractId, CatalogNumber, ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor, 
		ItemDescription, [SIN], ServiceCategoryId, PackageAsPriced, Removed, ParentItemId, LastModificationType,
		ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, Notes, MovedToHistoryBy, DateMovedToHistory )
	select i.ItemId, t.Ordinality, i.ContractId, i.CatalogNumber, i.ManufacturersCatalogNumber, i.ManufacturersName, i.LetterOfCommitmentDate, i.CommercialListPrice, i.CommercialPricelistDate, i.CommercialPricelistFOBTerms, i.ManufacturersCommercialListPrice, i.TrackingMechanism, i.AcquisitionCost, i.TypeOfContractor, 
		i.ItemDescription, i.[SIN], i.ServiceCategoryId, i.PackageAsPriced, 0, i.ParentItemId, i.LastModificationType,
		i.ModificationStatusId, i.CreatedBy, i.CreationDate, i.LastModifiedBy, i.LastModificationDate,
		'MedSurgNightlyBatchProcess',
		'dbo',
		getdate()
	from CM_Items i join #TempStagedItemsToMove t on i.ItemId = t.ItemId
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving staged expired items to history.'
		goto ERROREXIT
	END

	-- reflect move in the staging table
	update CM_ItemsHistoryStaging
	set ProcessingComplete = 1
	where ItemId in ( select ItemId from #TempStagedItemsToMove )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating status in staging table after moving staged items to history.'
		goto ERROREXIT
	END

	-- delete the items from the main table
	delete CM_Items
	where ItemId in ( select ItemId from #TempStagedItemsToMove )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting items after moving staged items to history.'
		goto ERROREXIT
	END

	-- move item countries to history
	insert into CM_ItemCountriesHistory
	( ItemCountryId, ItemId, CountryId, LastModificationType, ModificationStatusId, 
	CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, ReasonMovedToHistory,
	Notes, MovedToHistoryBy, DateMovedToHistory )
	select c.ItemCountryId, c.ItemId, c.CountryId, c.LastModificationType, c.ModificationStatusId, 
	c.CreatedBy, c.CreationDate, c.LastModifiedBy, c.LastModificationDate, 'B',
	'MedSurgNightlyBatchProcess', 'dbo', getdate() 
	from CM_ItemCountries c join #TempStagedItemsToMove t on c.ItemId = t.ItemId
	  
	select @error = @@ERROR

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error encountered when inserting into CM_ItemCountriesHistory.'
		goto ERROREXIT
	END

	delete CM_ItemCountries
	where ItemId in ( select ItemId from #TempStagedItemsToMove )

	select @error = @@ERROR

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error encountered when deleting CM_ItemCountries after move to history.'
		goto ERROREXIT
	END

	-- clean up very old, processed items out of the staging table
	delete CM_ItemsHistoryStaging
	where ProcessingComplete = 1
	and DateDiff( MM, DateMovedToHistoryStaging, getdate() ) >= 7 -- remove 7 months or older 

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error removing very old processed items out of the staging table.'
		goto ERROREXIT
	END

	-- clean up preferences
	exec MaintainUserRecentDocuments @MostRecentCount

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error cleaning up preferences.'
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


