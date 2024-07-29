IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateMedSurgItemDetails' )
BEGIN
	DROP PROCEDURE UpdateMedSurgItemDetails
END
GO

CREATE PROCEDURE UpdateMedSurgItemDetails
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ContractId int,
@ModificationStatusId int,
@ItemId int,
@ManufacturersCatalogNumber nvarchar(100),
@ManufacturersName nvarchar(100),
@LetterOfCommitmentDate datetime,
@CommercialListPrice decimal(10,2),
@CommercialPricelistDate datetime,
@CommercialPricelistFOBTerms nvarchar(40),
@ManufacturersCommercialListPrice decimal(10,2),
@TrackingMechanism	nvarchar(100),
@AcquisitionCost decimal(10,2),
@TypeOfContractor nvarchar(100)
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

	select @ordinality = 0

	select @ordinality = isnull( max( Ordinality ), 0 ) + 1
	from CM_ItemsHistory
	where ItemId = @ItemId

	insert into CM_ItemsHistory
	( ItemId, Ordinality, ContractId, CatalogNumber, ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor,
			ItemDescription, [SIN], ServiceCategoryId, PackageAsPriced, Removed, ParentItemId, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationdate,
			Notes, MovedToHistoryBy, DateMovedToHistory )
	select ItemId, @ordinality, ContractId, CatalogNumber, ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor, 
			ItemDescription, [SIN], ServiceCategoryId, PackageAsPriced, 0, ParentItemId, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, @currentUserLogin, LastModificationdate,
			'UpdateMedSurgItemDetails', @currentUserLogin, getdate() 
	from CM_Items
	where ItemId = @ItemId
	
	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error moving item into history.'
		goto ERROREXIT
	END

	update CM_Items
	set ModificationStatusId = @ModificationStatusId,
				
		ManufacturersCatalogNumber = @ManufacturersCatalogNumber, 
		ManufacturersName = @ManufacturersName, 
		LetterOfCommitmentDate = @LetterOfCommitmentDate, 
		CommercialListPrice = @CommercialListPrice, 
		CommercialPricelistDate = @CommercialPricelistDate, 
		CommercialPricelistFOBTerms = @CommercialPricelistFOBTerms, 
		ManufacturersCommercialListPrice = @ManufacturersCommercialListPrice, 		
		TrackingMechanism = @TrackingMechanism,
		AcquisitionCost = @AcquisitionCost,
		TypeOfContractor = @TypeOfContractor,				
		LastModificationType = 'C',
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = getdate()
	where ItemId = @ItemId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating item details for contract.'
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



