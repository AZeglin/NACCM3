IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[InsertFSSDrugItem]') AND type in (N'P', N'PC'))
DROP PROCEDURE [InsertFSSDrugItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure InsertFSSDrugItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@FdaAssignedLabelerCode char(5),
@ProductCode char(4),
@PackageCode char(2),
@Covered nchar(1),
@Generic nvarchar(64),
@TradeName nvarchar(45),
@DispensingUnit nvarchar(10),
@PackageDescription nvarchar(14),
@ModificationStatusId int,
@ParentDrugItemId int,
@IsBPA bit,
@DrugItemId int OUTPUT,
@DrugItemNDCId int OUTPUT 
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(450),
	@currentUserLogin nvarchar(120),
	@DiscontinuationDate datetime,
	@DrugItemPackageId int

/* this SP performs the insert for the GUI insert operation.  */

BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during insert item for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId during insert for contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	-- Added Check to make sure NDC is created only if it doesn't exist
	-- ANDEM 11/30/2009
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
	
	-- Added Check to make sure Item is created only if it doesn't exist
	-- ANDEM 07/02/2010
	If exists ( select top 1 1 From DI_DrugItems 
				where ContractId = @ContractId and DrugItemNDCId = @DrugItemNDCId )			
	BEGIN

		/* Item exists.  Prepare old item for reuse, only if it has been discontinued */

		Select @DrugItemId = DrugitemId,
			@DiscontinuationDate = DiscontinuationDate
		From DI_DrugItems 
		where ContractId = @ContractId
		And DrugItemNDCId = @DrugItemNDCId

		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error selecting DrugItemId for existing item on insert.'
			goto ERROREXIT
		END

		/* Existing item was discontinued */
		if @DiscontinuationDate is not null and DATEDIFF( dd, @DiscontinuationDate, getdate() ) > 0 
		BEGIN

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
				ParentDrugItemId,LastModificationType,ModificationStatusId,'Item Exists During Insert',CreatedBy,CreationDate,
				@currentUserLogin,GETDATE()
			from DI_DrugItems
			where DrugItemId = @DrugItemId

			select @error = @@error		
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting existing drug item into history during insert for contract ' + @ContractNumber
				goto ERROREXIT
			END		
		
			update DI_DrugItems
				Set Covered = @Covered,
					Generic = @Generic,
					TradeName = @TradeName,
					DiscontinuationDate = null,
					DiscontinuationEnteredDate = null,
					DiscontinuationReasonId = null,
					DispensingUnit = @DispensingUnit,
					PackageDescription = @PackageDescription,  
					ParentDrugItemId = @ParentDrugItemId,  
					DualPriceDesignation = 'F', 
					ExcludeFromExport = 0,
				--	NonTAA = 0,     /* retain these values */
				--	IncludedFETAmount = 0,
					ModificationStatusId = @ModificationStatusId, 
					LastModificationType = 'C',
					LastModifiedBy= @currentUserLogin, 
					LastModificationDate = getdate()
			where DrugItemId = @DrugItemId
		
			select @error = @@error		
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error updating existing drug item for insert for contract ' + @ContractNumber
				goto ERROREXIT
			END		
		
			/* since updating an existing itemId as if it was newly inserted, */
			/* need to also move all of the item's sub-items, prices, tiered prices and packaging to history */
		
			insert into DI_DrugItemSubItemsHistory
			( DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,LastModificationType,
				ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate )
			select DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,'C',
				ModificationStatusId,CreatedBy,CreationDate, @currentUserLogin, GETDATE()
			from DI_DrugItemSubItems
			where DrugItemId = @DrugItemId
		
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting sub-items into history from reused itemid for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		
		
			delete DI_DrugItemSubItems
			where DrugItemId = @DrugItemId
	
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting sub-items from reused itemid for fss contract ' + @ContractNumber
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
										where DrugItemId = @DrugItemId )

			select @error = @@error		

			IF  @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting into DI_DrugItemTieredPriceHistory from reused itemid for fss contract ' + @ContractNumber
				GOTO ERROREXIT
			END
	
			delete DI_DrugItemTieredPrice
			Where DrugItemPriceId in ( select DrugItemPriceId 
										from DI_DrugItemPrice
										where DrugItemId = @DrugItemId )

			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting tiered prices from reused itemid for fss contract ' + @ContractNumber
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
				LastModificationType, ModificationStatusId, 'Item Exists During Insert', CreatedBy,
				CreationDate, @currentUserLogin, getdate() 
			From DI_DrugItemPrice
			where DrugItemId = @DrugItemId
		
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting prices into price history from reused itemid for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		
	
			delete  DI_DrugItemPrice
			where DrugItemId = @DrugItemId
		
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting prices from reused itemid for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		


			insert into DI_DrugItemPackageHistory
			( DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
				ModificationStatusId,Notes,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate )
			select 
				DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
				@modificationStatusId, 'Item Exists During Insert', CreatedBy,CreationDate,@currentUserLogin,getdate()
			From Di_DrugItemPackage
			Where DrugItemId = @drugitemId

			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting item package history from reused itemid for contract ' + @ContractNumber
				GOTO ERROREXIT
			END
		
			/* no delete of package record */

			if @IsBPA = 1
			BEGIN
				/* get remaining item details from parent */
				update i
				set DateEnteredMarket = p.DateEnteredMarket,
					PrimeVendor = p.PrimeVendor,
					PrimeVendorChangedDate = p.PrimeVendorChangedDate,
					PassThrough = p.PassThrough,
					VAClass = p.VAClass,
					NonTAA = p.NonTAA, 
					IncludedFETAmount = p.IncludedFETAmount
				from DI_DrugItems i, DI_DrugItems p
				where p.DrugItemId = @ParentDrugItemId
				and i.DrugItemId = @DrugItemId
		
				select @error = @@error, @rowcount = @@rowcount
		
				if @error <> 0 or @rowcount <> 1
				BEGIN
					select @errorMsg = 'Error retrieving drug item details from parent for contract ' + @ContractNumber
					goto ERROREXIT
				END
		
				/* update package info from parent */
				update k
				set UnitOfSale = p.UnitOfSale,
					QuantityInUnitOfSale = p.QuantityInUnitOfSale, 
					UnitPackage = p.UnitPackage, 
					QuantityInUnitPackage = p.QuantityInUnitPackage, 
					UnitOfMeasure = p.UnitOfMeasure, 
					PriceMultiplier = p.PriceMultiplier,
					PriceDivider = p.PriceDivider,
					LastModifiedBy = @currentUserLogin, 
					LastModificationDate = getdate()
				from DI_DrugItemPackage k, DI_DrugItemPackage p
				where k.DrugItemId = @DrugItemId
				and p.DrugItemId = @ParentDrugItemId

				select @error = @@error, @rowcount = @@rowcount
		
				if @error <> 0 /* or @rowcount <> 1 allowing there to be no bpa parent packaging info 4/13/2010 */
				BEGIN
					select @errorMsg = 'Error retrieving drug item packaging details from parent for contract ' + @ContractNumber
					goto ERROREXIT
				END
			
			END /* is BPA */
		END /* is discontinued */
		else
		BEGIN
			select @errorMsg = 'Error: The item being added already exists on this contract and is not discontinued. The same NDC may not be added multiple times to the same contract.'
			goto ERROREXIT
		END
	END
	else
	BEGIN

		insert into DI_DrugItems
		( ContractId, DrugItemNDCId, Covered, PrimeVendor, PassThrough, Generic, TradeName, DispensingUnit, PackageDescription, ParentDrugItemId, DualPriceDesignation, ExcludeFromExport, NonTAA, IncludedFETAmount, ModificationStatusId, LastModificationType, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		values
		( @ContractId, @DrugItemNDCId, @Covered, 'F', 'F', @Generic, @TradeName, @DispensingUnit, @PackageDescription, @ParentDrugItemId, 'F', 0, 0, 0, @ModificationStatusId, 'C', @currentUserLogin, getdate(), @currentUserLogin, getdate() )

		select @error = @@error, @DrugItemId = SCOPE_IDENTITY()
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error inserting drug item for contract ' + @ContractNumber
			goto ERROREXIT
		END

		/* create the corresponding package record */
		insert into DI_DrugItemPackage
		( DrugItemId, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		values
		( @DrugItemId, @ModificationStatusId, @currentUserLogin, getdate(), @currentUserLogin, getdate() )
	
		select @error = @@error, @DrugItemPackageId = SCOPE_IDENTITY()
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error inserting drug item package for drug item id ' + @DrugItemId
			goto ERROREXIT
		END

		if @IsBPA = 1
		BEGIN
			/* get remaining item details from parent */
			update i
			set DateEnteredMarket = p.DateEnteredMarket,
				PrimeVendor = p.PrimeVendor,
				PrimeVendorChangedDate = p.PrimeVendorChangedDate,
				PassThrough = p.PassThrough,
				VAClass = p.VAClass,
				NonTAA = p.NonTAA, 
				IncludedFETAmount = p.IncludedFETAmount
			from DI_DrugItems i, DI_DrugItems p
			where p.DrugItemId = @ParentDrugItemId
			and i.DrugItemId = @DrugItemId
		
			select @error = @@error, @rowcount = @@rowcount
		
			if @error <> 0 or @rowcount <> 1
			BEGIN
				select @errorMsg = 'Error retrieving drug item details from parent for contract ' + @ContractNumber
				goto ERROREXIT
			END
		
			/* update package info from parent */
			update k
			set UnitOfSale = p.UnitOfSale,
				QuantityInUnitOfSale = p.QuantityInUnitOfSale, 
				UnitPackage = p.UnitPackage, 
				QuantityInUnitPackage = p.QuantityInUnitPackage, 
				UnitOfMeasure = p.UnitOfMeasure, 
				PriceMultiplier = p.PriceMultiplier,
				PriceDivider = p.PriceDivider,
				LastModifiedBy = @currentUserLogin, 
				LastModificationDate = getdate()
			from DI_DrugItemPackage k, DI_DrugItemPackage p
			where k.DrugItemId = @DrugItemId
			and p.DrugItemId = @ParentDrugItemId


			select @error = @@error, @rowcount = @@rowcount
		
			if @error <> 0 /* or @rowcount <> 1 allowing there to be no bpa parent packaging info 4/13/2010 */
			BEGIN
				select @errorMsg = 'Error retrieving drug item packaging details from parent for contract ' + @ContractNumber
				goto ERROREXIT
			END	
		END /* is BPA */
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
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 

ENDEXIT:

