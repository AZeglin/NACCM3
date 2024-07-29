IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopyItem')
	BEGIN
		DROP  Procedure  CopyItem
	END

GO

CREATE Procedure CopyItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@CopyType nvarchar(20),     /* CopyToSame, CopyToDestination */
@SourceContractNumber nvarchar(20),
@DrugItemId int,
@FdaAssignedLabelerCode char(5),
@ProductCode char(4),
@PackageCode char(2),
@TradeName nvarchar(45),
@GenericName nvarchar(64),
@DispensingUnit nvarchar(10),
@PackageDescription nvarchar(14),
@UnitOfSale nchar(2),
@QuantityInUnitOfSale decimal(5,0),
@UnitPackage nchar(2),
@QuantityInUnitPackage decimal(13,5),
@UnitOfMeasure nchar(2),
@DestinationContractNumber nvarchar(20),
@CopyPrices bit,
@CopySubItems bit,
@ModificationStatusId int,
@NewDrugItemId int OUTPUT,
@NewDrugItemNDCId int OUTPUT,
@NewDrugItemPackageId int OUTPUT,
@DestinationContractId int OUTPUT
)

AS

DECLARE @SourceContractId int,
	@OldDrugItemNDCId int,
	@DrugItemNDCId int,
	@DiscontinuationDate datetime,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@currentUserLogin nvarchar(120),
	@HistoricalNValue nchar(1),
	@Covered nchar(1),
	@OldGenericName nvarchar(64),
	@OldTradeName nvarchar(45),
	@OldDiscontinuationDateOfOriginalItem datetime,
	@OldDiscontinuationEnteredDateOfOriginalItem datetime,
	@OldDiscontinuationReasonId int,
	@DateEnteredMarket datetime,
	@PrimeVendor nchar(1),
	@PrimeVendorChangedDate datetime,
	@PassThrough nchar(1),
	@OldDispensingUnit nvarchar(10),
	@VAClass nvarchar(5),
	@DualPriceDesignation nchar(1),
	@ExcludeFromExport bit,
	@NonTAA bit, 
	@IncludedFETAmount float,
	@OldPackageDescription nvarchar(14),
	@DrugItemPackageId int,
	@OriginalCreatedBy nvarchar(120),
	@OriginalCreationDate datetime,
	@ParentDrugItemId int,
	@IsBPA bit,
	@ScheduleNumber int,
	@PriceMultiplier int, 
	@PriceDivider int,
	@PriceCount int,
	@TargetContractNumber nvarchar(20), /* the target is the source or the destination depending on the copy type */
	@TargetContractId int,   /* same comment */
	@TargetDrugItemId int, /* if the item already exists on the target */
	@RetVal int,
	@Division int


BEGIN TRANSACTION

	/* copy an item to either the same contract or a different contract */

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 
	
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END

	/* determine target contract number */
	if @CopyType = 'CopyToSame'
	BEGIN
		select @TargetContractNumber = @SourceContractNumber
	END
	else if @CopyType = 'CopyToDestination'
	BEGIN
		
		if @SourceContractNumber = @DestinationContractNumber
		BEGIN
			select @errorMsg = 'Error please specify a different contract number.'
			GOTO ERROREXIT		
		END
		
		select @TargetContractNumber = @DestinationContractNumber

		if not exists ( select ContractId from DI_Contracts where ContractNumber = @TargetContractNumber )
		BEGIN
			select @errorMsg = 'Target contract does not exist or is not a pharmaceutical contract.'
			GOTO ERROREXIT	
		END	
	END
	else
	BEGIN
		select @errorMsg = 'Error unknown copy type.'
		GOTO ERROREXIT	
	END

	/* from above, target contract may be same as source contract */
	select @TargetContractId = ContractId 
	from DI_Contracts 
	where ContractNumber = @TargetContractNumber

	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving destination contract id during CopyItem for contract ' + @TargetContractNumber 
		GOTO ERROREXIT
	END

	/* this is temporary to avoid changing the parameter name for now */
	select @DestinationContractId = @TargetContractId

	/* get elements of original item */	
	select @SourceContractId = ContractId,
		@OldDrugItemNDCId = DrugItemNDCId,
	 	@HistoricalNValue = HistoricalNValue,
		@OldPackageDescription = PackageDescription,
		@OldGenericName = Generic,
		@OldTradeName = TradeName,
		@OldDiscontinuationDateOfOriginalItem = DiscontinuationDate,
		@OldDiscontinuationEnteredDateOfOriginalItem = DiscontinuationEnteredDate,
		@OldDiscontinuationReasonId = DiscontinuationReasonId,
		@DateEnteredMarket = DateEnteredMarket,
		@Covered = Covered,
		@PrimeVendor = PrimeVendor,
		@PrimeVendorChangedDate = PrimeVendorChangedDate,
		@PassThrough = PassThrough,	
		@OldDispensingUnit = DispensingUnit,
		@VAClass = VAClass,
		@DualPriceDesignation = DualPriceDesignation,
		@ExcludeFromExport = ExcludeFromExport,
		@NonTAA = NonTAA,
		@IncludedFETAmount = IncludedFETAmount, 
		@ParentDrugItemId = ParentDrugItemId,
		@OriginalCreatedBy = CreatedBy,
		@OriginalCreationDate = CreationDate
	from DI_DrugItems
	where DrugItemId = @DrugItemId
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving item details during CopyItem from contract ' + @SourceContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
		GOTO ERROREXIT
	END

	/* BPA copy is blocked by the GUI, this is a failsafe */
	if @ParentDrugItemId is not null and @ParentDrugItemId <> -1
	BEGIN
		select @errorMsg = 'Copy of BPA Items is not supported. BPA Items are added by selecting a parent FSS item.'
		goto ERROREXIT
	END

	/* packaging parms are not prompted from the user for copy to destination, so use values from existing record */
	if @CopyType = 'CopyToDestination'
	BEGIN
		if exists ( select top 1 1 from DI_DrugItemPackage
						where DrugItemId = @DrugItemId )
		BEGIN
			/* get elements of original item package */
			select @DrugItemPackageId = DrugItemPackageId,
				@UnitOfSale = UnitOfSale,
				@QuantityInUnitOfSale = QuantityInUnitOfSale,
				@UnitPackage = UnitPackage,
				@QuantityInUnitPackage = QuantityInUnitPackage,
				@UnitOfMeasure = UnitOfMeasure,
				@PriceMultiplier = PriceMultiplier, 
				@PriceDivider = PriceDivider
			from DI_DrugItemPackage
			where DrugItemId = @DrugItemId
	
			select @error = @@error, @rowcount = @@rowcount
	
			if @error <> 0 or @rowcount <> 1
			BEGIN
				select @errorMsg = 'Error retrieving packaging information during CopyItem from contract ' + @SourceContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
				GOTO ERROREXIT
			END
		END
	END
	/* get NDCId */
	if exists ( select top 1 1 from DI_DrugItemNDC Where FdaAssignedLabelerCode = @FdaAssignedLabelerCode
				and ProductCode = @ProductCode and PackageCode = @PackageCode )
	BEGIN
		Select @DrugItemNDCId = DrugItemNDCId
		From DI_DrugItemNDC 
		Where FdaAssignedLabelerCode = @FdaAssignedLabelerCode
				and ProductCode = @ProductCode and PackageCode = @PackageCode 
	END
	ELSE
	BEGIN
		insert into DI_DrugItemNDC
		( FdaAssignedLabelerCode, ProductCode, PackageCode, ModificationStatusId, LastModifiedBy, LastModificationDate, CreatedBy, CreationDate )
		values
		( @FdaAssignedLabelerCode, @ProductCode, @PackageCode, @ModificationStatusId, @currentUserLogin, getdate(), @currentUserLogin, getdate() )
		
		select @error = @@error, @rowcount = @@rowcount, @DrugItemNDCId = SCOPE_IDENTITY()
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error inserting new NDC ' + @FdaAssignedLabelerCode + @ProductCode +  @PackageCode
			goto ERROREXIT
		END
	END


	/* if the item is already on the target */
	if exists( select top 1 1 from DI_DrugItems
					where DrugItemNDCId = @DrugItemNDCId
					and ContractId = @TargetContractId )
	BEGIN
		Select @TargetDrugItemId = DrugitemId,
			@DiscontinuationDate = DiscontinuationDate
		From DI_DrugItems 
		where ContractId = @TargetContractId
		And DrugItemNDCId = @DrugItemNDCId

		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error selecting target DrugItemId for existing item on copy.'
			goto ERROREXIT
		END

		/* if discontinued */
		if @DiscontinuationDate is not null and DATEDIFF( dd, @DiscontinuationDate, getdate() ) > 0 
		BEGIN
			/* reuse existing discontinued item on target */

			/* save item to history prior to edit */
			insert into DI_DrugItemsHistory
			( DrugItemId,ContractId,DrugItemNDCId,HistoricalNValue,PackageDescription,Generic,TradeName,
				DiscontinuationDate,DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,Covered,FCP,PrimeVendor,
				PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,ExcludeFromExport,NonTAA, IncludedFETAmount,
				ParentDrugItemId,LastModificationType,ModificationStatusId,Notes,CreatedBy,CreationDate,LastModifiedBy,
				LastModificationDate )
			select DrugItemId,ContractId,DrugItemNDCId,HistoricalNValue,PackageDescription,Generic,TradeName,
				DiscontinuationDate,DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,Covered,null,PrimeVendor,
				PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,ExcludeFromExport,NonTAA, IncludedFETAmount,
				ParentDrugItemId,LastModificationType,ModificationStatusId,'Item Exists During Copy',CreatedBy,CreationDate,
				@currentUserLogin,GETDATE()
			from DI_DrugItems
			where DrugItemId = @TargetDrugItemId

			select @error = @@error		
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting existing drug item into history during item copy for contract ' + @SourceContractNumber
				goto ERROREXIT
			END		
		
			update DI_DrugItems
				Set Covered = @Covered,  /* from old */
					Generic = @GenericName,   /* from parameter ( copy dialog ) */
					TradeName = @TradeName,  /* parm */
					DiscontinuationDate = null,   /* clearing these */
					DiscontinuationEnteredDate = null,
					DiscontinuationReasonId = null,
					DispensingUnit = @DispensingUnit,    /* parm */
					PackageDescription = @PackageDescription,    /* parm */					
					DualPriceDesignation = 'F', 
					ExcludeFromExport = 0,
					NonTAA = @NonTAA,  /* from old */
					IncludedFETAmount = @IncludedFETAmount,   /* from old */
					DateEnteredMarket = @DateEnteredMarket,		/* from old */	
					PrimeVendor = @PrimeVendor,					/* from old */	
					PrimeVendorChangedDate = @PrimeVendorChangedDate,   /* from old */	
					PassThrough = @PassThrough,			/* from old */		
					VAClass = @VAClass,		/* from old */	
					ParentDrugItemId = @ParentDrugItemId,		/* from old */	
					ModificationStatusId = @ModificationStatusId,  /* parm */
					LastModificationType = 'O',  /* copy */
					LastModifiedBy= @currentUserLogin, 
					LastModificationDate = getdate()
			where DrugItemId = @TargetDrugItemId
		
			select @error = @@error		
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error updating existing drug item for insert for contract ' + @SourceContractNumber
				goto ERROREXIT
			END		
		
			/* since updating an existing itemId as if it was a copy of the original, */
			/* need to also move all of the item's sub-items, prices, tiered prices and packaging to history */
		
			insert into DI_DrugItemSubItemsHistory
			( DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,LastModificationType,
				ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate )
			select DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit, LastModificationType,
				ModificationStatusId,CreatedBy,CreationDate, @currentUserLogin, GETDATE()
			from DI_DrugItemSubItems
			where DrugItemId = @TargetDrugItemId
		
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting sub-items into history from reused itemid for fss contract during copy' + @SourceContractNumber
				goto ERROREXIT
			END		
		
			delete DI_DrugItemSubItems
			where DrugItemId = @TargetDrugItemId
	
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting sub-items from reused itemid for fss contract ' + @SourceContractNumber
				goto ERROREXIT
			END		
	

			insert into DI_DrugItemTieredPriceHistory
			(	DrugItemTieredPriceId,DrugItemPriceId,TieredPriceStartDate,TieredPriceStopDate,Price,Minimum,MinimumValue,
				ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate 
			)
			select DrugItemTieredPriceId,DrugItemPriceId,TieredPriceStartDate,TieredPriceStopDate,Price,Minimum,MinimumValue,
					@ModificationStatusId,CreatedBy,CreationDate,@currentUserLogin,getdate()
			From DI_DrugItemTieredPrice
			Where DrugItemPriceId in ( select DrugItemPriceId 
										from DI_DrugItemPrice
										where DrugItemId = @TargetDrugItemId )

			select @error = @@error		

			IF  @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting into DI_DrugItemTieredPriceHistory from reused itemid for fss contract ' + @SourceContractNumber
				GOTO ERROREXIT
			END
	
			delete DI_DrugItemTieredPrice
			Where DrugItemPriceId in ( select DrugItemPriceId 
										from DI_DrugItemPrice
										where DrugItemId = @TargetDrugItemId )

			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting tiered prices from reused itemid for fss contract ' + @SourceContractNumber
				goto ERROREXIT
			END		


			insert into DI_DrugItemPriceHistory
			(   DrugItemPriceId, DrugItemId, DrugItemSubItemId,HistoricalNValue, PriceId, 
				PriceStartDate, PriceStopDate, Price, IsDeleted,IsTemporary, IsFSS, IsBIG4,                                          	                  
				IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, IsNIH, IsPHS, 
				IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG, IsFHCC,AwardedFSSTrackingCustomerRatio,
				TrackingCustomerName, CurrentTrackingCustomerPrice, ExcludeFromExport,
				LastModificationType, ModificationStatusId, Notes, CreatedBy,
				CreationDate, LastModifiedBy, LastModificationDate 
			)
			Select DrugItemPriceId, DrugItemId, DrugItemSubItemId, HistoricalNValue, PriceId, 
					PriceStartDate, PriceStopDate, Price, 1, IsTemporary, IsFSS, IsBIG4,                                          	                  
				IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, IsNIH, IsPHS, 
				IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG, IsFHCC,AwardedFSSTrackingCustomerRatio,
				TrackingCustomerName, CurrentTrackingCustomerPrice, ExcludeFromExport,
				LastModificationType, ModificationStatusId, 'CopyItem', CreatedBy,
				CreationDate, LastModifiedBy, getdate() 
			From DI_DrugItemPrice
			where DrugItemId = @TargetDrugItemId
		
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting prices into price history from reused itemid for fss contract during copy ' + @SourceContractNumber
				goto ERROREXIT
			END		
	
			delete  DI_DrugItemPrice
			where DrugItemId = @TargetDrugItemId
		
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting prices from reused itemid for fss contract ' + @SourceContractNumber
				goto ERROREXIT
			END		


			insert into DI_DrugItemPackageHistory
			( DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
				ModificationStatusId,Notes,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate )
			select 
				DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
				@modificationStatusId, 'CopyItem', CreatedBy,CreationDate,@currentUserLogin,getdate()
			From Di_DrugItemPackage
			Where DrugItemId = @TargetDrugItemId

			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting item package history from reused itemid for contract during copy ' + @SourceContractNumber
				GOTO ERROREXIT
			END
		
			delete DI_DrugItemPackage
			where DrugItemId = @TargetDrugItemId

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting package from reused itemid for fss contract ' + @SourceContractNumber
				goto ERROREXIT
			END		

			insert into DI_DrugItemDistributorsHistory
			( DrugItemDistributorId, DrugItemId, DistributorName, Phone, ContactPerson, Notes, IsDeleted, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
			select DrugItemDistributorId, DrugItemId, DistributorName, Phone, ContactPerson, Notes + 'CopyItem', 0, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate 
			from DI_DrugItemDistributors
			where DrugItemId = @TargetDrugItemId

			select @error = @@ERROR

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting distributor history during copy item for drugItemId=' + CONVERT( nvarchar(20), @TargetDrugItemId )
				goto ERROREXIT
			END

			delete DI_DrugItemDistributors
			where DrugItemId = @TargetDrugItemId

			select @error = @@ERROR

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting distributor from reused itemid during copy item for drugItemId=' + CONVERT( nvarchar(20), @TargetDrugItemId )
				goto ERROREXIT
			END
		END
		else
		BEGIN
			select @errorMsg = 'Error: The proposed item already exists on the destination contract and is not discontinued. The same NDC may not be added multiple times to the same contract.'
			goto ERROREXIT
		END
	END
	else /* the NDC is currently not on the target contract */
	BEGIN

		/* create the new item on the target */
		insert into DI_DrugItems
		( ContractId, DrugItemNDCId, Covered, PrimeVendor, PrimeVendorChangedDate, PassThrough, Generic, TradeName, DispensingUnit, 
			HistoricalNValue, PackageDescription, DualPriceDesignation, DateEnteredMarket, ExcludeFromExport, VAClass, NonTAA, IncludedFETAmount, 
			ModificationStatusId, LastModificationType, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		values
		( @TargetContractId, @DrugItemNDCId, @Covered, @PrimeVendor, @PrimeVendorChangedDate, @PassThrough, @GenericName, @TradeName, @DispensingUnit, 
			@HistoricalNValue, @PackageDescription, 'F', @DateEnteredMarket, @ExcludeFromExport, @VAClass, @NonTAA, @IncludedFETAmount, 
			@ModificationStatusId, 'O', @currentUserLogin, getdate(), @currentUserLogin, getdate() )

		select @error = @@error, @TargetDrugItemId = SCOPE_IDENTITY()
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error copying drug item for contract ' + @SourceContractNumber
			goto ERROREXIT
		END
	END

	/* create the corresponding package record for new or existing item */
	insert into DI_DrugItemPackage
	( DrugItemId, UnitOfSale, QuantityInUnitOfSale, UnitPackage, QuantityInUnitPackage, UnitOfMeasure, 
		PriceMultiplier, PriceDivider, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	values
	( @TargetDrugItemId, @UnitOfSale, @QuantityInUnitOfSale, @UnitPackage, @QuantityInUnitPackage, @UnitOfMeasure, 
		@PriceMultiplier, @PriceDivider, @ModificationStatusId, @currentUserLogin, getdate(), @currentUserLogin, getdate() )
	
	select @error = @@error, @NewDrugItemPackageId = SCOPE_IDENTITY()
		
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting drug item package for drug item id ' + CONVERT(nvarchar(20), @TargetDrugItemId )
		goto ERROREXIT
	END		
	
	/* set return parameters */
	select @NewDrugItemNDCId = @DrugItemNDCId

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error setting return parameters (1) for drug item id ' + CONVERT(nvarchar(20), @TargetDrugItemId )
		goto ERROREXIT
	END		

	select @NewDrugItemId = @TargetDrugItemId

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error setting return parameters (2) for drug item id ' + CONVERT(nvarchar(20), @TargetDrugItemId )
		goto ERROREXIT
	END		

	/* per Ted 10/3/2017 - always copy distributors during copy or ndc change */
	insert into DI_DrugItemDistributors
	( DrugItemId, DistributorName, Address1, Address2, City, State, Zip, Phone, Extension, Fax, Email, ContactPerson,
		WebPage, Notes, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	select
	@TargetDrugItemId, DistributorName, Address1, Address2, City, State, Zip, Phone, Extension, Fax, Email, ContactPerson,
		WebPage, Notes + ' CopyItem', @currentUserLogin, GETDATE(),@currentUserLogin, GETDATE()
	from DI_DrugItemDistributors
	where DrugItemId = @DrugItemId

	Select @error = @@ERROR		
	IF @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting Drug Item Distributors for drug item id ' + CONVERT(nvarchar(20), @TargetDrugItemId )
		GOTO ERROREXIT
	END		


	if @CopySubItems = 1
	BEGIN
		insert into DI_DrugItemSubItems
			( DrugItemId, SubItemIdentifier, PackageDescription, Generic, TradeName, DispensingUnit, LastModificationType, ModificationStatusId,
			CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		select @NewDrugItemId, SubItemIdentifier, PackageDescription, Generic, TradeName, DispensingUnit, LastModificationType, 
			@ModificationStatusId,
			@currentUserLogin,
			getdate(),
			@currentUserLogin, 
			getdate()
		from DI_DrugItemSubItems 
		where DrugItemId = @DrugItemId

		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error copying sub-items to new NDC during CopyItem for contract ' + @TargetContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
			GOTO ERROREXIT
		END
		
	END
	
	/* not brining forward the exclude, also not bringing forward the tracking customer pricing as each item will have a different price */
	/* also not bringing forward tiered pricing */
	if @CopyPrices = 1
	BEGIN
		if @CopySubItems = 1
		BEGIN			
			insert into DI_DrugItemPrice
			( DrugItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsTemporary, IsFSS, IsBIG4,                                          	                  
				IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, 
				IsNIH, IsPHS, IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG, IsFHCC,
				DrugItemSubItemId, 				
				LastModificationType,
				ModificationStatusId,
				CreatedBy,
				CreationDate,
				LastModifiedBy, 
				LastModificationDate 
			)
			select @NewDrugItemId, p.PriceId, p.PriceStartDate, p.PriceStopDate, p.Price, p.IsTemporary, p.IsFSS, p.IsBIG4,                                          	                  
				p.IsVA, p.IsBOP, p.IsCMOP, p.IsDOD, p.IsHHS, p.IsIHS, p.IsIHS2, p.IsDIHS, 
				p.IsNIH, p.IsPHS, p.IsSVH, p.IsSVH1, p.IsSVH2, p.IsTMOP, p.IsUSCG, p.IsFHCC,
				p.DrugItemSubItemId,  /* old id */							
				'O',  /* copy */
				@ModificationStatusId,
				@OriginalCreatedBy,
				@OriginalCreationDate,
				@currentUserLogin, 
				getdate()
			from DI_DrugItemPrice p
			where p.DrugItemId = @DrugItemId
					
			select @error = @@error
		
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error copying prices (1) to new NDC during CopyItem for contract ' + @TargetContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
				GOTO ERROREXIT
			END
		END
		else
		BEGIN
			insert into DI_DrugItemPrice
			( DrugItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsTemporary, IsFSS, IsBIG4,                                          	                  
				IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, 
				IsNIH, IsPHS, IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG, IsFHCC,
				DrugItemSubItemId, 				
				LastModificationType,
				ModificationStatusId,
				CreatedBy,
				CreationDate,
				LastModifiedBy, 
				LastModificationDate 
			)
			select @NewDrugItemId, p.PriceId, p.PriceStartDate, p.PriceStopDate, p.Price, p.IsTemporary, p.IsFSS, p.IsBIG4,                                          	                  
				p.IsVA, p.IsBOP, p.IsCMOP, p.IsDOD, p.IsHHS, p.IsIHS, p.IsIHS2, p.IsDIHS, 
				p.IsNIH, p.IsPHS, p.IsSVH, p.IsSVH1, p.IsSVH2, p.IsTMOP, p.IsUSCG, p.IsFHCC,
				p.DrugItemSubItemId,				
				'O',  /* copy */
				@ModificationStatusId,
				@OriginalCreatedBy,
				@OriginalCreationDate,
				@currentUserLogin, 
				getdate()
			from DI_DrugItemPrice p
			where p.DrugItemId = @DrugItemId
			and p.DrugItemSubItemId is null
					
			select @error = @@error
		
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error copying prices (2) to new NDC during CopyItem for contract ' + @TargetContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
				GOTO ERROREXIT
			END
		END
		
		/* update with new sub-item ids */
		if @CopySubItems = 1
		BEGIN
			update DI_DrugItemPrice
				set DrugItemSubItemId = ( select s.DrugItemSubItemId 
					from DI_DrugItemSubItems s 
					where s.DrugItemId = @NewDrugItemId
					and s.SubItemIdentifier = ( select t.SubItemIdentifier 
												from DI_DrugItemSubItems t 
												where t.DrugItemId = @DrugItemId
												and t.DrugItemSubItemId = p.DrugItemSubItemId ) )
			from DI_DrugItemPrice p 
			where p.DrugItemId = @NewDrugItemId
			and p.DrugItemSubItemId is not null
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error updating sub item ids for the new NDC during CopyItem for contract ' + @TargetContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
				GOTO ERROREXIT
			END	
		END
		
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


