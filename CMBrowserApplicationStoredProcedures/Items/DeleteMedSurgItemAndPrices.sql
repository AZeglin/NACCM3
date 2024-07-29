IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteMedSurgItemAndPrices' )
BEGIN
	DROP PROCEDURE DeleteMedSurgItemAndPrices
END
GO

CREATE PROCEDURE DeleteMedSurgItemAndPrices
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ContractId int,
@ModificationStatusId int,
@ItemId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserLogin nvarchar(120),
		@ordinality int
		


BEGIN TRANSACTION

	exec dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 
	Select @error = @@error		
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert( nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	insert into CM_ItemPriceHistory
	( ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, Removed, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerPrice, TrackingCustomerFOBTerms, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 
			Notes, MovedToHistoryBy, DateMovedToHistory )
	select ItemPriceId, ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, 1, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerPrice, TrackingCustomerFOBTerms, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 
			'DeleteMedSurgItemAndPrices', @currentUserLogin, getdate() 
	from CM_ItemPrice
	where ItemId = @ItemId

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error moving item prices into history.'
		goto ERROREXIT
	END

	delete CM_ItemPrice
	Output 'CM_ItemPrice', Deleted.ItemPriceId, @currentUserLogin, GETDATE() into Audit_Deleted_Data_By_User
	where ItemId = @ItemId

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting item prices after move to history.'
		goto ERROREXIT
	END

	select @ordinality = 0

	select @ordinality = isnull( max( Ordinality ), 0 ) + 1
	from CM_ItemsHistory
	where ItemId = @ItemId

	insert into CM_ItemsHistory
	( ItemId, Ordinality, ContractId, CatalogNumber, ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor, CountryOfOrigin,
		ItemDescription, [SIN], ServiceCategoryId, PackageAsPriced, Removed, ParentItemId, 
		LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationdate,
			Notes, MovedToHistoryBy, DateMovedToHistory )
	select ItemId, @ordinality, ContractId, CatalogNumber, ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor, CountryOfOrigin,
			ItemDescription, [SIN], ServiceCategoryId, PackageAsPriced, 1, ParentItemId, 
		LastModificationType, ModificationStatusId, CreatedBy, CreationDate, @currentUserLogin, LastModificationdate,
			'DeleteMedSurgItemAndPrices', @currentUserLogin, getdate() 
	from CM_Items
	where ItemId = @ItemId
	
	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error moving item into history.'
		goto ERROREXIT
	END

	delete CM_Items	
	Output 'CM_Items', Deleted.ItemId, @currentUserLogin, GETDATE() into Audit_Deleted_Data_By_User
	where ItemId = @ItemId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0  or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting item after move to history.'
		goto ERROREXIT
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



