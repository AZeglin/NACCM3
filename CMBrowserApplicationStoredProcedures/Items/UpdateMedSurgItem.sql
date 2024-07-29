IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateMedSurgItem' )
BEGIN
	DROP PROCEDURE UpdateMedSurgItem
END
GO

CREATE PROCEDURE UpdateMedSurgItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ContractId int,
@ModificationStatusId int,
@ItemId int,
@CatalogNumber nvarchar(70),
@ItemDescription nvarchar(800),
@SIN nvarchar(50),
@PackageAsPriced nvarchar(2),
@ParentItemId int = null,
@ServiceCategoryId int = null,
@SearchText nvarchar(50),
@UpdatedRowNumber int OUTPUT
)

AS

Declare @itemExists bit,		
		@compareParentItemId bit,
		@compareServiceCategoryId bit,
		@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserLogin nvarchar(120),
		@ordinality int,
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
		select @compareParentItemId = 1
	END
	else
	BEGIN
		select @compareParentItemId = 0
	END

	if @ServiceCategoryId is not null
	BEGIN
		select @compareServiceCategoryId = 1
	END
	else
	BEGIN
		select @compareServiceCategoryId = 0
	END

	select @itemExists = dbo.CheckForDuplicateMedSurgItemFunction( @ContractId, @CatalogNumber, @ItemDescription, @PackageAsPriced,  @SIN, @ServiceCategoryId, @ParentItemId, @ItemId, @compareServiceCategoryId, @compareParentItemId )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error checking for item existence during item update.'
		goto ERROREXIT
	END

	if @itemExists = 1
	BEGIN
		select @errorMsg = 'Error updating item. An item with the same catalog number, description, etc. already exists for this contract.'
		goto ERROREXIT
	END
	else
	BEGIN

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

		-- note an updated item will never reference an historical parent item
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

			select @query = 'select @UpdatedRowNumber_parm = x.RowNumber
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
			select @query = 'select @UpdatedRowNumber_parm = x.RowNumber
			from ( select ROW_NUMBER() OVER ( order by ItemDescription ) as RowNumber, 
						i.ItemId
					from CM_Items i
					where i.ContractId = @ContractId_parm ' + @searchWhere + ' ) x
			where x.ItemId = @ItemId_parm ' 
		END
		else -- fss or national 
		BEGIN
			select @query = 'select @UpdatedRowNumber_parm = x.RowNumber
			from ( select ROW_NUMBER() OVER ( order by CatalogNumber ) as RowNumber, 
						i.ItemId,
						i.CatalogNumber,
						i.ItemDescription
					from CM_Items i
					where i.ContractId = @ContractId_parm ' + @searchWhere + ' ) x
			where x.ItemId = @ItemId_parm ' 
		END

		select @SQLParms = N'@ContractId_parm int, @ParentContractId_parm int, @ItemId_parm int, @UpdatedRowNumber_parm int OUTPUT'
		
		exec SP_EXECUTESQL  @query, @SQLParms, @ContractId_parm = @ContractId, @ParentContractId_parm = @ParentContractId, @ItemId_parm = @ItemId, @UpdatedRowNumber_parm = @UpdatedRowNumber OUTPUT
																					
		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting items for contract (1)'
			goto ERROREXIT
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
				'UpdateMedSurgItem', @currentUserLogin, getdate() 
		from CM_Items
		where ItemId = @ItemId
	
		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error moving item into history.'
			goto ERROREXIT
		END

		/* BPAs use an un-editable default SIN */
		if @ParentItemId is not null
		BEGIN		
			update CM_Items
			set ModificationStatusId = @ModificationStatusId,
				ParentItemId = @ParentItemId,
				CatalogNumber = convert( nvarchar(70), LTRIM(RTRIM( dbo.RemoveNonPrintable( @CatalogNumber )))),			
				ItemDescription = convert( nvarchar(800), LTRIM(RTRIM( dbo.RemoveNonPrintable( @ItemDescription )))),
				ServiceCategoryId = @ServiceCategoryId,
				PackageAsPriced = @PackageAsPriced,
				LastModificationType = 'C',
				LastModifiedBy = @currentUserLogin,
				LastModificationDate = getdate()
			where ItemId = @ItemId

			select @error = @@ERROR, @rowCount = @@ROWCOUNT
			if @error <> 0 or @rowCount <> 1
			BEGIN
				select @errorMsg = 'Error updating BPA item for contract.'
				goto ERROREXIT
			END

		END
		else
		BEGIN
			update CM_Items
			set ModificationStatusId = @ModificationStatusId,
				ParentItemId = @ParentItemId,
				CatalogNumber = convert( nvarchar(70), LTRIM(RTRIM( dbo.RemoveNonPrintable( @CatalogNumber )))),			
				ItemDescription = convert( nvarchar(800), LTRIM(RTRIM( dbo.RemoveNonPrintable( @ItemDescription )))),
				[SIN] = @SIN,
				ServiceCategoryId = @ServiceCategoryId,
				PackageAsPriced = @PackageAsPriced,
				LastModificationType = 'C',
				LastModifiedBy = @currentUserLogin,
				LastModificationDate = getdate()
			where ItemId = @ItemId

			select @error = @@ERROR, @rowCount = @@ROWCOUNT
			if @error <> 0 or @rowCount <> 1
			BEGIN
				select @errorMsg = 'Error updating item for contract.'
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



