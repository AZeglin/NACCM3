IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopyMedSurgItemsForContract2')
	BEGIN
		DROP  Procedure  CopyMedSurgItemsForContract2
	END

GO

CREATE Procedure CopyMedSurgItemsForContract2
(
	@CopyContractLogId int,
	@OldContractNumber nvarchar(50),
	@OldContractId int,
	@NewContractNumber nvarchar(50),
	@NewContractId int,
	@EffectiveDate datetime,
	@ExpirationDate datetime,
	@UserLogin nvarchar(120)
)
As

	Declare @itemCount int,
			@error int,
			@rowcount int,
			@errorMsg nvarchar(250),
			@retVal int,

			@oldItemId int,
			@newItemId int,
			@oldItemPriceId int,
			@bpaCheckItemPriceId int,
			@newItemPriceId int,

			@IsBPA bit,
			@IsTemporary bit

	BEGIN TRANSACTION
	
		/* only items with active prices get copied */
		select @itemCount = 0
		
		
		Declare ItemCursor cursor For														
			Select i.ItemId
			From CM_Items i 
			where i.ContractId = @OldContractId
			and exists ( select p.ItemPriceId from CM_ItemPrice p
			where p.ItemId = i.ItemId
			and datediff( dd, p.PriceStartDate, getdate() ) >= 0
			and datediff( dd, p.PriceStopDate, getdate() ) <= 0 
			and p.IsTemporary = 0 )
					
		Open ItemCursor
		FETCH NEXT FROM ItemCursor
		INTO @oldItemId

		WHILE @@FETCH_STATUS = 0
		BEGIN			

			insert into CM_Items
				( ContractId, CatalogNumber, ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor, 
					ItemDescription, [SIN], ServiceCategoryId, PackageAsPriced, ParentItemId, 
					LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationdate )
			select @NewContractId, convert( nvarchar(70), LTRIM(RTRIM(dbo.RemoveNonPrintable( CatalogNumber )))), ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor, 
				convert( nvarchar(800), LTRIM(RTRIM(dbo.RemoveNonPrintable( ItemDescription )))), [SIN], ServiceCategoryId, PackageAsPriced, ParentItemId, 
				'Y', -1, @UserLogin, getdate(), @UserLogin, getdate() 
			from CM_Items 
			where ItemId = @oldItemId
					
			select @error = @@ERROR, @newItemId = SCOPE_IDENTITY()
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error copying med/surg item for copy contract.'
				goto ERROREXIT
			END

			select @itemCount = @itemCount + 1

			/* copy item countries */			
			insert into CM_ItemCountries
			( ItemId, CountryId, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate  )
			select 	
				@newItemId, CountryId, 'Y', -1, @UserLogin, getdate(), @UserLogin, getdate()
			from CM_ItemCountries
			where ItemId = @oldItemId
			
			select @error = @@ERROR
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error copying countries of origin for item.'
				goto ERROREXIT
			END
			
			/* only prices which are not temporary get copied */
			Declare PriceCursor cursor For														
			Select p.ItemPriceId					
			From CM_ItemPrice p 
			where p.ItemId = @oldItemId
			and datediff( dd, p.PriceStartDate, getdate() ) >= 0
			and datediff( dd, p.PriceStopDate, getdate() ) <= 0
			and p.IsTemporary = 0
					
			Open PriceCursor
			FETCH NEXT FROM PriceCursor
			INTO @oldItemPriceId

			WHILE @@FETCH_STATUS = 0
			BEGIN			

				insert into CM_ItemPrice
					( ItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsBPA, IsTemporary, TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms,
					LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationdate )
				select @newItemId, PriceId, @EffectiveDate, @ExpirationDate, Price, IsBPA, IsTemporary, TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms,
					'Y', -1, @UserLogin, getdate(), @UserLogin, getdate() 			
				from CM_ItemPrice
				where ItemPriceId = @oldItemPriceId
				

				select @error = @@ERROR, @newItemPriceId = SCOPE_IDENTITY() 
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error copying price for copy contract.'
					goto ERROREXIT
				END

				if exists ( select ItemTieredPriceId from CM_ItemTieredPrice where ItemPriceId = @oldItemPriceId )
				BEGIN

					insert into CM_ItemTieredPrice
						( ItemPriceId, TieredPriceStartDate, TieredPriceStopDate, Price, TierSequence, TierCriteria, MinimumValue, 
						LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationdate )
					select @newItemPriceId, @EffectiveDate, @ExpirationDate, Price, TierSequence, TierCriteria, MinimumValue, 
						'Y', -1, @UserLogin, getdate(), @UserLogin, getdate() 
					from CM_ItemTieredPrice
					where ItemPriceId = @oldItemPriceId
					
					select @error = @@ERROR
					if @error <> 0 
					BEGIN
						select @errorMsg = 'Error copying tiered prices for copy contract.'
						goto ERROREXIT
					END

				END
						
				-- any price for any item of a BPA should be marked BPA						
				select @bpaCheckItemPriceId = @oldItemPriceId

				FETCH NEXT FROM PriceCursor 
				INTO @oldItemPriceId
			END

			Close PriceCursor
			DeAllocate PriceCursor

			FETCH NEXT FROM ItemCursor 
			INTO @oldItemId
		END

		Close ItemCursor
		DeAllocate ItemCursor

		select top 1 @IsBPA = isnull( IsBPA, 0 )
			from CM_ItemPrice
			where ItemPriceId = @bpaCheckItemPriceId
		
		Select @error = @@ERROR
		IF @error <> 0
		BEGIN
			select @errorMsg = 'Error looking up BPA status when updating tbl_CopyContractsLog for contract: ' + @NewContractNumber
			GOTO ERROREXIT
		END		

		Update tbl_CopyContractsLog
			Set TotalPriceListItems = @itemCount, 
			TotalBPAPriceListItems = ( case when @IsBPA = 1 then @itemCount else 0 end )
		Where CopyContractLogId = @CopyContractLogId
			
		Select @error = @@ERROR
		IF @error <> 0
		BEGIN
			select @errorMsg = 'Error updating tbl_CopyContractsLog for contract: ' + @NewContractNumber
			GOTO ERROREXIT
		END			
				

GOTO OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 ) 

	IF @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
	/* only rollback iff this the highest level */ 
		ROLLBACK TRANSACTION
	END

	RETURN (-1)

OKEXIT:
	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)


	