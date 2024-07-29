IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ChangeNDC')
	BEGIN
		DROP  Procedure  ChangeNDC
	END

GO

CREATE Procedure ChangeNDC
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),       /* not used */
@NACCMDatabaseName nvarchar(255),		/* not used */
@ContractNumber nvarchar(20),
@DrugItemId int,
@FdaAssignedLabelerCode char(5),
@ProductCode char(4),
@PackageCode char(2),
@DiscontinuationDate datetime =NULL,
@EffectiveDate datetime,
@CopyPrices bit,
@CopySubItems bit,
@ModificationStatusId int,
@NewDrugItemId int OUTPUT,
@NewDrugItemNDCId int OUTPUT,
@NewDrugItemPackageId int OUTPUT
)

AS

DECLARE @ContractId int,
	@OldDrugItemNDCId int,
	@DrugItemNDCId int,
	@TargetDrugItemId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(350),
	@currentUserLogin nvarchar(120),
	@HistoricalNValue nchar(1),
	@Covered nchar(1),
	@GenericName nvarchar(64),
	@TradeName nvarchar(45),
	@OldDiscontinuationDateOfOriginalItem datetime,
	@OldDiscontinuationEnteredDateOfOriginalItem datetime,
	@OldDiscontinuationReasonId int,
	@DateEnteredMarket datetime,
	@PrimeVendor nchar(1),
	@PrimeVendorChangedDate datetime,
	@PassThrough nchar(1),
	@DispensingUnit nvarchar(10),
	@VAClass nvarchar(5),
	@DualPriceDesignation nchar(1),
	@ExcludeFromExport bit,
	@NonTAA bit, 
	@IncludedFETAmount float,
	@PackageDescription nvarchar(14),
	@DrugItemPackageId int,
	@OriginalCreatedBy nvarchar(120),
	@OriginalCreationDate datetime,
	@ParentDrugItemId int,	
	@UnitOfSale nchar(2),
	@QuantityInUnitOfSale decimal(5,0),
	@UnitPackage nchar(2),
	@QuantityInUnitPackage decimal(13,5),
	@UnitOfMeasure nchar(2),
	@PriceMultiplier int, 
	@PriceDivider int,
	@TargetDiscontinuationDate datetime


BEGIN TRANSACTION

	/* to change an NDC, discontinue the old item and insert a new item */

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 
	
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT 
	END

	/* get elements of original item */	
	select @ContractId = ContractId,
		@OldDrugItemNDCId = DrugItemNDCId,
	 	@HistoricalNValue = HistoricalNValue,
		@PackageDescription = PackageDescription,
		@GenericName = Generic,
		@TradeName = TradeName,
		@OldDiscontinuationDateOfOriginalItem = DiscontinuationDate,
		@OldDiscontinuationEnteredDateOfOriginalItem = DiscontinuationEnteredDate,
		@OldDiscontinuationReasonId = DiscontinuationReasonId,
		@DateEnteredMarket = DateEnteredMarket,
		@Covered = Covered,
		@PrimeVendor = PrimeVendor,
		@PrimeVendorChangedDate = PrimeVendorChangedDate,
		@PassThrough = PassThrough,	
		@DispensingUnit = DispensingUnit,
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
		select @errorMsg = 'Error retrieving item details during NDCChange for contract ' + @ContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
		GOTO ERROREXIT
	END

	/* BPA ndc change is blocked by the GUI, this is a failsafe */
	if @ParentDrugItemId is not null and @ParentDrugItemId <> -1
	BEGIN
		select @errorMsg = 'NDC Change of BPA Items is not supported. BPA Items are added by selecting a parent FSS item.'
		goto ERROREXIT
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

	/* set the return value for the id */
	select @NewDrugItemNDCId = @DrugItemNDCId

	/* if the item is already on the target */
	if exists( select top 1 1 from DI_DrugItems
					where DrugItemNDCId = @DrugItemNDCId
					and ContractId = @ContractId )
	BEGIN
		Select @TargetDrugItemId = DrugitemId,
			@TargetDiscontinuationDate = DiscontinuationDate
		From DI_DrugItems 
		where ContractId = @ContractId
		And DrugItemNDCId = @DrugItemNDCId

		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error selecting target DrugItemId for existing item on ndc change.'
			goto ERROREXIT
		END

		/* if discontinued */
		if @TargetDiscontinuationDate is not null and DATEDIFF( dd, @TargetDiscontinuationDate, getdate() ) > 0 
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
				ParentDrugItemId,LastModificationType,ModificationStatusId,'Item Exists During NDC Change',CreatedBy,CreationDate,
				@currentUserLogin,GETDATE()
			from DI_DrugItems
			where DrugItemId = @TargetDrugItemId

			select @error = @@error		
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting existing drug item into history during ndc change for contract ' + @ContractNumber
				goto ERROREXIT
			END		
		
			update DI_DrugItems
				Set Covered = @Covered,  /* from old */
					Generic = @GenericName,   /* from parameter ( ndc change dialog ) */
					TradeName = @TradeName,  /* parm */
					DiscontinuationDate = null,   /* clearing these */
					DiscontinuationEnteredDate = null,
					DiscontinuationReasonId = null,
					DispensingUnit = @DispensingUnit,    /* from old */
					PackageDescription = @PackageDescription,    /* from old */					
					DualPriceDesignation = @DualPriceDesignation, /* from old */ 
					ExcludeFromExport = @ExcludeFromExport, /* from old */
					NonTAA = @NonTAA,  /* from old */
					IncludedFETAmount = @IncludedFETAmount,   /* from old */					
					DateEnteredMarket = @DateEnteredMarket,		/* from old */	
					PrimeVendor = @PrimeVendor,					/* from old */	
					PrimeVendorChangedDate = @PrimeVendorChangedDate,   /* from old */	
					PassThrough = @PassThrough,			/* from old */		
					VAClass = @VAClass,		/* from old */	
					ParentDrugItemId = @ParentDrugItemId,		/* from old */	
					ModificationStatusId = @ModificationStatusId,  /* parm */
					LastModificationType = 'N',
					LastModifiedBy= @currentUserLogin, 
					LastModificationDate = getdate()
			where DrugItemId = @TargetDrugItemId
		
			select @error = @@error		
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error updating existing drug item for insert for contract ' + @ContractNumber
				goto ERROREXIT
			END		

			/* set return value for id ( also used later )*/
			select @NewDrugItemId = @TargetDrugItemId

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
				select @errorMsg = 'Error inserting sub-items into history from reused itemid during ndc change for contract' + @ContractNumber
				goto ERROREXIT
			END		
		
			delete DI_DrugItemSubItems
			where DrugItemId = @TargetDrugItemId
	
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting sub-items from reused itemid during ndc change for contract' + @ContractNumber
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
				select @errorMsg = 'Error inserting into DI_DrugItemTieredPriceHistory from reused itemid during ndc change for contract' + @ContractNumber
				GOTO ERROREXIT
			END
	
			delete DI_DrugItemTieredPrice
			Where DrugItemPriceId in ( select DrugItemPriceId 
										from DI_DrugItemPrice
										where DrugItemId = @TargetDrugItemId )

			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting tiered prices from reused itemid during ndc change for contract' + @ContractNumber
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
				LastModificationType, ModificationStatusId, 'Item Exists During NDC Change', CreatedBy,
				CreationDate, @currentUserLogin, getdate() 
			From DI_DrugItemPrice
			where DrugItemId = @TargetDrugItemId
		
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting prices into price history from reused itemid during ndc change for contract' + @ContractNumber
			END		
	
			delete  DI_DrugItemPrice
			where DrugItemId = @TargetDrugItemId
		
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting prices from reused itemid during ndc change for contract' + @ContractNumber
				goto ERROREXIT
			END		


			insert into DI_DrugItemPackageHistory
			( DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
				ModificationStatusId,Notes,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate )
			select 
				DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
				@modificationStatusId, 'Item Exists During NDC Change', CreatedBy,CreationDate,@currentUserLogin,getdate()
			From Di_DrugItemPackage
			Where DrugItemId = @TargetDrugItemId

			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting item package history from reused itemid during ndc change for contract' + @ContractNumber
				GOTO ERROREXIT
			END
		
			delete DI_DrugItemPackage
			where DrugItemId = @TargetDrugItemId

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting package from reused itemid during ndc change for contract' + @ContractNumber
			END		

			insert into DI_DrugItemDistributorsHistory
			( DrugItemDistributorId, DrugItemId, DistributorName, Phone, ContactPerson, Notes, IsDeleted, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
			select DrugItemDistributorId, DrugItemId, DistributorName, Phone, ContactPerson, Notes + 'ChangeNDC', 0, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate 
			from DI_DrugItemDistributors
			where DrugItemId = @TargetDrugItemId

			select @error = @@ERROR

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting distributor history during ndc change for drugItemId=' + CONVERT( nvarchar(20), @TargetDrugItemId )
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
		else /* not discontinued -error */
		BEGIN
			select @errorMsg = 'Error: The proposed item already exists on the contract and is not discontinued. The same NDC may not be added multiple times to the same contract.'
			goto ERROREXIT
		END
	END
	else  /* does not exist, so create */
	BEGIN
		/* create the new item on the target */
		insert into DI_DrugItems
		( ContractId, DrugItemNDCId, Covered, PrimeVendor, PrimeVendorChangedDate, PassThrough, Generic, TradeName, DispensingUnit,				
				HistoricalNValue, PackageDescription, DualPriceDesignation, DateEnteredMarket, ExcludeFromExport, VAClass, NonTAA, IncludedFETAmount, 
				ModificationStatusId, LastModificationType, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		values
		( @ContractId, @DrugItemNDCId, @Covered, @PrimeVendor, @PrimeVendorChangedDate, @PassThrough, @GenericName, @TradeName, @DispensingUnit, 
				@HistoricalNValue, @PackageDescription, @DualPriceDesignation, @DateEnteredMarket, @ExcludeFromExport, @VAClass, @NonTAA, @IncludedFETAmount, 
				@ModificationStatusId, 'N', @currentUserLogin, getdate(), @currentUserLogin, getdate() )

		select @error = @@error, @NewDrugItemId = SCOPE_IDENTITY()
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error inserting new drug item for NDCChange for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END

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
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error retrieving item package info during NDCChange for contract ' + @ContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
			GOTO ERROREXIT
		END

	
		insert into DI_DrugItemPackage
		( DrugItemId, UnitOfSale, QuantityInUnitOfSale, UnitPackage, QuantityInUnitPackage, UnitOfMeasure, 
			PriceMultiplier, PriceDivider, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		values
		( @NewDrugItemId, @UnitOfSale, @QuantityInUnitOfSale, @UnitPackage, @QuantityInUnitPackage, @UnitOfMeasure, 
			@PriceMultiplier, @PriceDivider, @ModificationStatusId, @currentUserLogin, getdate(), @currentUserLogin, getdate() )
	
		select @error = @@error, @NewDrugItemPackageId = SCOPE_IDENTITY()
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error inserting drug item package during ndc change for drug item id ' + CONVERT(nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END		
	END


	/* per Ted 10/3/2017 - always copy distributors during copy or ndc change */
	insert into DI_DrugItemDistributors
	( DrugItemId, DistributorName, Address1, Address2, City, State, Zip, Phone, Extension, Fax, Email, ContactPerson,
		WebPage, Notes, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	select
		@NewDrugItemId, DistributorName, Address1, Address2, City, State, Zip, Phone, Extension, Fax, Email, ContactPerson,
		WebPage, Notes + ' ChangeNDC', @currentUserLogin, GETDATE(),@currentUserLogin, GETDATE()
	from DI_DrugItemDistributors
	where DrugItemId = @DrugItemId

	Select @error = @@ERROR		
	IF @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting Drug Item Distributors for drug item id ' + CONVERT( nvarchar(20), @DrugItemId )
		GOTO ERROREXIT
	END		


	if @CopySubItems = 1
	BEGIN
		insert into DI_DrugItemSubItems
			( DrugItemId, SubItemIdentifier, PackageDescription, Generic, TradeName, DispensingUnit, LastModificationType, ModificationStatusId,
			CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		select @NewDrugItemId, SubItemIdentifier, PackageDescription, Generic, TradeName, DispensingUnit, 'N', 
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
			select @errorMsg = 'Error copying sub-items to new NDC during NDCChange for contract ' + @ContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
			GOTO ERROREXIT
		END
		
	END
	
	/* first insert the prices for the new item, then update the start dates if applicable */
	if @CopyPrices = 1
	BEGIN
		if @CopySubItems = 1
		BEGIN

			insert into DI_DrugItemPrice
			( DrugItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsTemporary, IsFSS, IsBIG4,                                          	                  
				IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, 
				IsNIH, IsPHS, IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG, IsFHCC,
				DrugItemSubItemId, 
				ExcludeFromExport,
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
				p.ExcludeFromExport,
				'N',  /* NDC Change */
				@ModificationStatusId,
				@OriginalCreatedBy,
				@OriginalCreationDate,
				@currentUserLogin, 
				getdate()
			from DI_DrugItemPrice p
			where p.DrugItemId = @DrugItemId
			and p.PriceStopDate >= @EffectiveDate 
					
			select @error = @@error
		
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error copying prices (1) to new NDC during NDCChange for contract ' + @ContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
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
				ExcludeFromExport,
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
				NULL,
				p.ExcludeFromExport,
				'N',  /* NDC Change */
				@ModificationStatusId,
				@OriginalCreatedBy,
				@OriginalCreationDate,
				@currentUserLogin, 
				getdate()
			from DI_DrugItemPrice p
			where p.DrugItemId = @DrugItemId
			and p.DrugItemSubItemId is null  /* only non-sub-item prices */
			and p.PriceStopDate >= @EffectiveDate 
				
			select @error = @@error
		
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error copying prices (2) to new NDC during NDCChange for contract ' + @ContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
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
				select @errorMsg = 'Error updating sub item ids for the new NDC during NDCChange for contract ' + @ContractNumber + ' and drug item id ' + convert( nvarchar(20), @NewDrugItemId )
				GOTO ERROREXIT
			END	
		END
		
	END
		
		
	/* update the start dates of new prices so that they do not start before the effective date */
	/* but future prices can start after the effective date so they are left alone */
	update DI_DrugItemPrice
		set PriceStartDate = @EffectiveDate
	where DrugItemId = @NewDrugItemId		
	and PriceStartDate < @EffectiveDate
	and PriceStopDate >= @EffectiveDate 
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating price start dates for the new NDC during NDCChange for contract ' + @ContractNumber + ' and drug item id ' + convert( nvarchar(20), @NewDrugItemId )
		GOTO ERROREXIT
	END	
		
	if @DiscontinuationDate is not null 
	BEGIN
		EXEC DiscontinueFSSDrugItemFromContract @CurrentUser = @CurrentUser,	
			@SecurityServerName = @SecurityServerName,
			@SecurityDatabaseName = @SecurityDatabaseName,
			@ContractNumber = @ContractNumber,
			@DrugItemId = @DrugItemId, 
			@DiscontinuationDate = @DiscontinuationDate, 
			@DiscontinuationReason = 'NDC Change',
			@ModificationStatusId = @ModificationStatusId,
			@LastModificationType = 'N'  /* NDC Change */

		select @error = @@error, @rowcount = @@rowcount
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error calling DiscontinueFSSDrugItemFromContract during NDCChange for contract ' + @ContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
			GOTO ERROREXIT
		END
	END
	
	
	
	/* finally, make an entry in the mapping table */
	insert into DI_ContractNDCNumberChange
	( NewContractId, NewDrugItemNDCId, NewDrugItemId, NewHistoricalNValue, OldContractId, OldDrugItemNDCId, OldDrugItemId, OldHistoricalNValue, ChangeStatus, ModificationId, EffectiveDate, EndDate, LastModifiedBy, LastModificationDate )
	values
	( @ContractId, @NewDrugItemNDCId, @NewDrugItemId, @HistoricalNValue, @ContractId, @OldDrugItemNDCId, @DrugItemId, @HistoricalNValue, 'I', @ModificationStatusId, @EffectiveDate, null, @currentUserLogin, getdate() )
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error making an entry in the mapping ( DI_ContractNDCNumberChange ) table during NDCChange for contract ' + @ContractNumber + ' and drug item id ' + convert( nvarchar(20), @DrugItemId )
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


