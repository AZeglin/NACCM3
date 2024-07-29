IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetMedSurgItemDetails' )
BEGIN
	DROP PROCEDURE GetMedSurgItemDetails
END
GO

CREATE PROCEDURE GetMedSurgItemDetails
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),   
@ContractId int,
@ItemId int
)

AS

Declare 	@error int,	
		@errorMsg nvarchar(1000)

/* used when retrieving details of item for item details screen */		
BEGIN TRANSACTION

	/* selected item may be from history */
	if exists ( select ItemId from CM_Items where ItemId = @ItemId )
	BEGIN

		select i.ItemId, i.CatalogNumber, 	
			i.ManufacturersCatalogNumber, i.ManufacturersName, i.LetterOfCommitmentDate, i.CommercialListPrice, i.CommercialPricelistDate, i.CommercialPricelistFOBTerms, 
			i.ManufacturersCommercialListPrice, i.TrackingMechanism, i.AcquisitionCost, i.TypeOfContractor, 
			i.ItemDescription, i.[SIN], i.ServiceCategoryId, i.PackageAsPriced, i.ParentItemId, 1 as ParentActive, 0 as ParentHistorical, -1 as ItemHistoryId, i.LastModificationType, i.ModificationStatusId, i.LastModifiedBy, i.LastModificationDate
				
			from CM_Items i 
			where i.ItemId = @ItemId 
			and i.ContractId = @ContractId
	
		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting item details for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else
	BEGIN
		select h.ItemId, h.CatalogNumber, 	
			h.ManufacturersCatalogNumber, h.ManufacturersName, h.LetterOfCommitmentDate, h.CommercialListPrice, h.CommercialPricelistDate, h.CommercialPricelistFOBTerms, 
			h.ManufacturersCommercialListPrice, h.TrackingMechanism, h.AcquisitionCost, h.TypeOfContractor, 
			h.ItemDescription, h.[SIN], h.ServiceCategoryId, h.PackageAsPriced, h.ParentItemId, 1 as ParentActive, 0 as ParentHistorical, h.ItemHistoryId, h.LastModificationType, h.ModificationStatusId, h.LastModifiedBy, h.LastModificationDate
				
			from CM_ItemsHistory h 
			where h.ItemId = @ItemId 
			and h.ContractId = @ContractId
			and h.Ordinality = ( select max(ordinality) from CM_ItemsHistory where ItemId = h.ItemId and ContractId = h.ContractId )
	
		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting item details from history for contract ' + @ContractNumber
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



