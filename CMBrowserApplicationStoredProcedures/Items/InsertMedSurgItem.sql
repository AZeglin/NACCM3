IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertMedSurgItem' )
BEGIN
	DROP PROCEDURE InsertMedSurgItem
END
GO

CREATE PROCEDURE InsertMedSurgItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ContractId int,
@ModificationStatusId int,
@CatalogNumber nvarchar(70),
@ItemDescription nvarchar(800),
@SIN nvarchar(50),
@PackageAsPriced nvarchar(2),
@ParentItemId int = null,
@ServiceCategoryId int = null,
@SearchText nvarchar(50),
@ItemId int OUTPUT,
@InsertedRowNumber int OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserLogin nvarchar(120),
		@itemExists bit,
		@CompareServiceCategoryId bit, 
		@CompareParentItemId bit,  
		
		@ManufacturersCatalogNumber nvarchar(100),
		@ManufacturersName nvarchar(100),
		@LetterOfCommitmentDate datetime,
		@CommercialListPrice decimal(10,2),
		@CommercialPricelistDate datetime,
		@CommercialPricelistFOBTerms nvarchar(40),
		@ManufacturersCommercialListPrice decimal(10,2),
		@TrackingMechanism	nvarchar(100),
		@AcquisitionCost decimal(10,2),
		@TypeOfContractor nvarchar(100),
		--@CountryOfOrigin int,
		--@ItemCountryId int,
		@ParentContractId int,
		@searchWhere nvarchar(600),
		@query nvarchar(max),
		@SQLParms nvarchar(1200)

BEGIN TRANSACTION

	exec dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 
	Select @error = @@error		
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert( nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	select @itemExists = 0
	if @ParentItemId is not null
	BEGIN
		select @CompareParentItemId = 1
	END
	else
	BEGIN
		select @CompareParentItemId = 0
	END

	if @ServiceCategoryId is not null
	BEGIN
		select @CompareServiceCategoryId = 1
	END
	else
	BEGIN
		select @CompareServiceCategoryId = 0
	END

	select @itemExists = dbo.CheckForDuplicateMedSurgItemFunction( @ContractId, @CatalogNumber, @ItemDescription, @PackageAsPriced,  @SIN, @ServiceCategoryId, @ParentItemId, null, @CompareServiceCategoryId, @CompareParentItemId )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error checking for item existence during item insert.'
		goto ERROREXIT
	END

	if @itemExists = 1
	BEGIN
		select @errorMsg = 'Error inserting item. An item with the same catalog number, description, etc. already exists for this contract.'
		goto ERROREXIT
	END
	else
	BEGIN

		-- default item details values based on if bpa or service
		if @CompareParentItemId = 1
		BEGIN
			select @ManufacturersCatalogNumber = 'Reference parent manuf part number'
		END
		else
		BEGIN
			if @CompareServiceCategoryId = 1
			BEGIN
				select @ManufacturersCatalogNumber = 'Services'
			END
			else
			BEGIN
				select @ManufacturersCatalogNumber = ''
			END

		END

		-- default other item details values
		select @ManufacturersName = '', 
		@LetterOfCommitmentDate = null, 
		@CommercialListPrice = null,  
		@CommercialPricelistDate = null, 
		@CommercialPricelistFOBTerms = '', 
		@ManufacturersCommercialListPrice = null, 
		@TrackingMechanism = '', 
		@AcquisitionCost = null, 
		@TypeOfContractor = ''

		
		

		insert into CM_Items
		( ContractId, CatalogNumber, ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor, 
			ItemDescription, [SIN], ServiceCategoryId, PackageAsPriced, ParentItemId, 
			LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationdate )
		values
		( @ContractId, convert( nvarchar(70), LTRIM(RTRIM(dbo.RemoveNonPrintable( @CatalogNumber )))), @ManufacturersCatalogNumber, @ManufacturersName, @LetterOfCommitmentDate, @CommercialListPrice, @CommercialPricelistDate, @CommercialPricelistFOBTerms, @ManufacturersCommercialListPrice, @TrackingMechanism, @AcquisitionCost, @TypeOfContractor, 
			convert( nvarchar(800), LTRIM(RTRIM(dbo.RemoveNonPrintable( @ItemDescription )))), @SIN, @ServiceCategoryId, @PackageAsPriced, @ParentItemId, 
			'C', @ModificationStatusId, @currentUserLogin, getdate(), @currentUserLogin, getdate() )

		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @ItemId = SCOPE_IDENTITY()
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error inserting item into contract.'
			goto ERROREXIT
		END

		---- default country to usa
		--select @CountryOfOrigin = ( select CountryId from CM_Countries where CountryName = 'United States Of America' )
		
		--select @error = @@ERROR, @rowCount = @@ROWCOUNT
		--if @error <> 0 or @rowCount <> 1
		--BEGIN
		--	select @errorMsg = 'Error selecting default country when inserting item into contract.'
		--	goto ERROREXIT
		--END

		---- default country for the item
		--exec InsertCountryOfOriginForItem @CurrentUser = @CurrentUser,
		--		@SecurityServerName = @SecurityServerName,
		--		@SecurityDatabaseName = @SecurityDatabaseName,
		--		@ItemId = @ItemId,
		--		@CountryId = @CountryOfOrigin,
		--		@ModificationStatusId = @ModificationStatusId,
		--		@ModificationType = 'C',
		--		@ItemCountryId = @ItemCountryId OUTPUT

		--select @error = @@ERROR
		--if @error <> 0 
		--BEGIN
		--	select @errorMsg = 'Error inserting country of origin for item.'
		--	goto ERROREXIT
		--END

		-- use search text to match existing sort order when determining row index
		if @SearchText is not null
		BEGIN
			if LEN(LTRIM(RTRIM(@SearchText))) > 0
			BEGIN
				if @compareParentItemId = 0
				BEGIN
 					select @searchWhere = ' and ( i.CatalogNumber like ''%' + @SearchText + '%'' or i.ItemDescription like ''%' + @SearchText + '%'' ) '
				END
				else
				BEGIN
					select @searchWhere = ' and (( d.ParentItemId is not null and ( d.ParentCatalogNumber like ''%' + @SearchText + '%'' or d.ParentItemDescription like ''%' + @SearchText + '%'' ))
											or ( i.ItemId is not null and  ( i.CatalogNumber like ''%' + @SearchText + '%'' or i.ItemDescription like ''%' + @SearchText + '%'' ))) '
				END
			END		
			else
			BEGIN
				select @searchWhere = ''
			END
		END
		else
		BEGIN
			select @searchWhere = ''
		END


		-- get row number of new row for return
		-- note these must match the "order by" of the item select SP

		-- note an inserted item will never reference an historical parent item
		if @CompareParentItemId = 1
		BEGIN			
			
			select @ParentContractId = ContractId
			from CM_Items where ItemId = @ParentItemId

			select @error = @@ERROR, @rowCount = @@ROWCOUNT
			if @error <> 0 or @rowCount <> 1
			BEGIN
				select @errorMsg = 'Error getting parent contract from BPA item.'
				goto ERROREXIT
			END

			select @query = 'select @InsertedRowNumber_parm = x.RowNumber
			from ( select ROW_NUMBER() OVER ( order by ParentCatalogNumber ) as RowNumber, 
					i.ItemId
					from CM_Items i  left outer join
					( select z.ItemId as ParentItemId, z.CatalogNumber as ParentCatalogNumber, z.ItemDescription as ParentItemDescription
						from CM_Items z where z.ContractId = @ParentContractId_parm ) d on i.ParentItemId = d.ParentItemId
					where i.ContractId = @ContractId_parm ' + @searchWhere + ' ) x
			where x.ItemId = @ItemId_parm '


		END
		else if @CompareServiceCategoryId = 1
		BEGIN
			select @query = 'select @InsertedRowNumber_parm = x.RowNumber
			from ( select ROW_NUMBER() OVER ( order by i.ItemDescription ) as RowNumber, 
						i.ItemId
					from CM_Items i
					where i.ContractId = @ContractId_parm ' + @searchWhere + ' ) x
			where x.ItemId = @ItemId_parm '
		END
		else -- fss or national 
		BEGIN
			select @query = 'select @InsertedRowNumber_parm = x.RowNumber
			from ( select ROW_NUMBER() OVER ( order by i.CatalogNumber ) as RowNumber, 
						i.ItemId
					from CM_Items i
					where i.ContractId = @ContractId_parm ' + @searchWhere + ' ) x
			where x.ItemId = @ItemId_parm '
		END

		select @SQLParms = N'@ContractId_parm int, @ParentContractId_parm int, @ItemId_parm int, @InsertedRowNumber_parm int OUTPUT'
		
		exec SP_EXECUTESQL  @query, @SQLParms, @ContractId_parm = @ContractId, @ParentContractId_parm = @ParentContractId, @ItemId_parm = @ItemId, @InsertedRowNumber_parm = @InsertedRowNumber OUTPUT
																					
		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting items for contract (1)'
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



