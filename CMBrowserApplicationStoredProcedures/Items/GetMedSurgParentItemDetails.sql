IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetMedSurgParentItemDetails' )
BEGIN
	DROP PROCEDURE GetMedSurgParentItemDetails
END
GO

CREATE PROCEDURE GetMedSurgParentItemDetails
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),   
@ItemId int
)

AS

Declare 	@error int,	
		@errorMsg nvarchar(1000)

/* used when retrieving details of parent item when displaying selected parent info in a BPA item during edit -- GRID ONLY, NOT DETAILS SCREEN */		
BEGIN TRANSACTION

	select i.ItemId, i.CatalogNumber, i.ItemDescription, i.[SIN], i.ServiceCategoryId, i.PackageAsPriced, i.ParentItemId, 1 as ParentActive, 0 as ParentHistorical, -1 as ItemHistoryId, i.LastModificationType, i.ModificationStatusId, i.LastModifiedBy, i.LastModificationDate
				
		from CM_Items i 
		where i.ItemId = @ItemId
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting item details for contract ' + @ContractNumber
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



