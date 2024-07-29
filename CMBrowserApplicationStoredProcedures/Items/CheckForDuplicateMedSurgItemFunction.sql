IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'CheckForDuplicateMedSurgItemFunction')
	BEGIN
		DROP  Function  CheckForDuplicateMedSurgItemFunction
	END

GO

CREATE FUNCTION [dbo].[CheckForDuplicateMedSurgItemFunction]
(
@ContractId int,
@CatalogNumber nvarchar(70), 
@ItemDescription nvarchar(800), 
@PackageAsPriced nvarchar(2), 
@SIN nvarchar(50), 
@ServiceCategoryId int,
@ParentItemId int,
@ItemIdBeingUpdated int = null, -- this is populated during an update
@CompareServiceCategoryId bit = 0,
@CompareParentItemId bit = 0  -- these 2 bits must be mutex
)

RETURNS bit

AS
BEGIN
	/* new version 2 check that only looks at item table, not price */

	DECLARE @ItemExists int
	
	select @ItemExists = 0

	if @ItemIdBeingUpdated is not null
	BEGIN
		if @CompareServiceCategoryId = 1
			BEGIN
				if exists (
					select ItemId from CM_Items
						where ContractId = @ContractId
							--and LTRIM(RTRIM( dbo.RemoveNonPrintable( CatalogNumber ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @CatalogNumber )))
							and LTRIM(RTRIM( dbo.RemoveNonPrintable( ItemDescription ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @ItemDescription )))			
							and [SIN] = @SIN
							and PackageAsPriced = @PackageAsPriced
							and ItemId <> @ItemIdBeingUpdated
							and ServiceCategoryId = @ServiceCategoryId
						)
						BEGIN
							select @ItemExists = 1
						END
			END
			else if @CompareParentItemId = 1
			BEGIN
				if exists (
					select ItemId from CM_Items
						where ContractId = @ContractId
							--and LTRIM(RTRIM( dbo.RemoveNonPrintable( CatalogNumber ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @CatalogNumber )))
							--and LTRIM(RTRIM( dbo.RemoveNonPrintable( ItemDescription ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @ItemDescription )))			
							and [SIN] = @SIN
							and PackageAsPriced = @PackageAsPriced
							and ItemId <> @ItemIdBeingUpdated
							and ParentItemId = @ParentItemId
						)
						BEGIN
							select @ItemExists = 1
						END
			END
			else
			BEGIN
				if exists (
						select ItemId from CM_Items
							where ContractId = @ContractId
								and LTRIM(RTRIM( dbo.RemoveNonPrintable( CatalogNumber ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @CatalogNumber )))
								and LTRIM(RTRIM( dbo.RemoveNonPrintable( ItemDescription ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @ItemDescription )))			
								and [SIN] = @SIN
								and PackageAsPriced = @PackageAsPriced
								and ItemId <> @ItemIdBeingUpdated
							)
							BEGIN
								select @ItemExists = 1
							END
			END
	END
	else
	BEGIN

		if @CompareServiceCategoryId = 1
		BEGIN
			if exists (
				select ItemId from CM_Items
					where ContractId = @ContractId
						--and LTRIM(RTRIM( dbo.RemoveNonPrintable( CatalogNumber ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @CatalogNumber )))
						and LTRIM(RTRIM( dbo.RemoveNonPrintable( ItemDescription ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @ItemDescription )))			
						and [SIN] = @SIN
						and PackageAsPriced = @PackageAsPriced
						and ServiceCategoryId = @ServiceCategoryId
					)
					BEGIN
						select @ItemExists = 1
					END
		END
		else if @CompareParentItemId = 1
		BEGIN
			if exists (
				select ItemId from CM_Items
					where ContractId = @ContractId
						--and LTRIM(RTRIM( dbo.RemoveNonPrintable( CatalogNumber ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @CatalogNumber )))
						--and LTRIM(RTRIM( dbo.RemoveNonPrintable( ItemDescription ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @ItemDescription )))			
						and [SIN] = @SIN
						and PackageAsPriced = @PackageAsPriced
						and ParentItemId = @ParentItemId
					)
					BEGIN
						select @ItemExists = 1
					END
		END
		else
		BEGIN
			if exists (
				select ItemId from CM_Items
					where ContractId = @ContractId
						and LTRIM(RTRIM( dbo.RemoveNonPrintable( CatalogNumber ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @CatalogNumber )))
						and LTRIM(RTRIM( dbo.RemoveNonPrintable( ItemDescription ))) = LTRIM(RTRIM( dbo.RemoveNonPrintable( @ItemDescription )))			
						and [SIN] = @SIN
						and PackageAsPriced = @PackageAsPriced
					)
					BEGIN
						select @ItemExists = 1
					END
		END
	END

	RETURN @ItemExists

END