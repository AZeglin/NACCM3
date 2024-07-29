IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UpdateDrugItemNDC]') AND type in (N'P', N'PC'))
DROP PROCEDURE [UpdateDrugItemNDC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [UpdateDrugItemNDC]
(
 @DrugItemId int,
 @CurrentDrugItemNDCId int,
 @newfdaAssignedLabelerCode char(5) ,
 @newproductCode char(4) ,
 @newpackageCode char(2) ,
 @loginName nvarchar(120),
 @modificationStatusId int,
 @Notes nvarchar(2000)

)
As

	Declare @newDrugItemNDCId int, @error int, @errorMsg varchar(1000),@contractId int, @rowcount int, 
		@discontinuationDate datetime, @effectiveDate datetime, @historicalNValue nchar(1),
		@discontinuedDrugItemIdToBeRemoved int, @ContractNumber nvarchar(20)

BEGIN TRANSACTION

	Select @contractId = ContractId,
	@HistoricalNValue = HistoricalNValue 
	From DI_DrugItems
	Where DrugItemId = @DrugItemId
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId for DrugItemId: ' + Cast(@DrugItemId as varchar)
		goto ERROREXIT
	END

	if( exists ( select top 1 1 from DI_DrugItemNDC
					where FdaAssignedLabelerCode = @newfdaAssignedLabelerCode
					and ProductCode = @newproductCode
					and PackageCode = @newpackageCode ))
	BEGIN
		Select @newDrugItemNDCId = DrugItemNDCId
		from DI_DrugItemNDC
		where FdaAssignedLabelerCode = @newfdaAssignedLabelerCode
			and ProductCode = @newproductCode
			and PackageCode = @newpackageCode
		
		Select @error = @@error
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error (1) updating to new NDC:  ' + @newfdaAssignedLabelerCode  + ' ' +  @newproductCode + ' ' +@newpackageCode
			GOTO ERROREXIT
		END
		
		/* error if already on the same contract and is active */
		if( exists ( select top 1 1 from DI_DrugItems
					where ContractId = @ContractId
					and DrugItemNDCId = @newDrugItemNDCId
					and ( DiscontinuationDate is null
					or DiscontinuationDate >= CONVERT( datetime, CONVERT( varchar(50), getdate(), 101 )) )))
		BEGIN
			select @errorMsg = 'Error New NDC already exists on this contract:  ' + @newfdaAssignedLabelerCode + ' ' +  @newproductCode + ' ' + @newpackageCode
			GOTO ERROREXIT	
		END

		/* if exists and is already on the same contract and has been discontinued */
		/* move the discontinued item to history */
		if( exists ( select top 1 1 from DI_DrugItems
					where ContractId = @ContractId
					and DrugItemNDCId = @newDrugItemNDCId
					and DiscontinuationDate is not null 
					and DiscontinuationDate < CONVERT( datetime, CONVERT( varchar(50), getdate(), 101 )) ))
		BEGIN
			/* get the id of the item to be removed to history */
			select @discontinuedDrugItemIdToBeRemoved = DrugItemId
			from DI_DrugItems
			where ContractId = @ContractId
			and DrugItemNDCId = @newDrugItemNDCId
			and DiscontinuationDate is not null 
			and DiscontinuationDate < CONVERT( datetime, CONVERT( varchar(50), getdate(), 101 )) 
			
			select @error = @@error
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error in UpdateDrugItemNDC selecting @discontinuedDrugItemIdToBeRemoved for ContractId: '+ cast( @ContractId as varchar ) + ' and NDCId: ' + cast( @newDrugItemNDCId as varchar )
				GOTO ERROREXIT
			END	
		
			Insert into Di_DrugItemsHistory
				( DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
				 DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
				 Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
				 ExcludeFromExport,NonTAA,IncludedFETAmount,LastModificationType,ModificationStatusId,Notes,CreatedBy,CreationDate,
				 LastModifiedBy,LastModificationDate 
				)
			Select 
				DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
				DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
				Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
				ExcludeFromExport,NonTAA,IncludedFETAmount,LastModificationType,ModificationStatusId, @Notes + 'UpdateDrugItemNDC',
				CreatedBy,CreationDate,@loginName,getdate() 
			from DI_DrugItems
			where DrugItemId = @discontinuedDrugItemIdToBeRemoved
			
			select @error = @@error
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting item history for removal of discontinued drugitem Id: '+ cast(@discontinuedDrugItemIdToBeRemoved as varchar)
				GOTO ERROREXIT
			END	
		
			/* remove the offending item */
			delete DI_DrugItems
			where DrugItemId = @discontinuedDrugItemIdToBeRemoved
		
			select @error = @@error
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error on ndc change when deleting item for removal of discontinued drugitem Id: '+ cast(@discontinuedDrugItemIdToBeRemoved as varchar)
				GOTO ERROREXIT
			END	
		
			/* since removing the item to history,  */
			/* need to also move all of the item's sub-items, prices, tiered prices and packaging to history */
			/* added 2/14/2011 */
			insert into DI_DrugItemSubItemsHistory
			( DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,LastModificationType,
				ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate )
			select DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,'C',
				ModificationStatusId,CreatedBy,CreationDate, @loginName, GETDATE()
			from DI_DrugItemSubItems
			where DrugItemId = @discontinuedDrugItemIdToBeRemoved
			
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting sub-items into history from reused ndc for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		

			delete DI_DrugItemSubItems
			where DrugItemId = @discontinuedDrugItemIdToBeRemoved
		
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting sub-items from reused ndc for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		

			insert into DI_DrugItemTieredPriceHistory
			(	DrugItemTieredPriceId,DrugItemPriceId,TieredPriceStartDate,TieredPriceStopDate,Price,Minimum,MinimumValue,
				ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate 
			)
			select DrugItemTieredPriceId,DrugItemPriceId,TieredPriceStartDate,TieredPriceStopDate,Price,Minimum,MinimumValue,
					@ModificationStatusId,CreatedBy,CreationDate,@loginName,getdate()
			From DI_DrugItemTieredPrice
			Where DrugItemPriceId in ( select DrugItemPriceId 
										from DI_DrugItemPrice
										where DrugItemId = @discontinuedDrugItemIdToBeRemoved )

			select @error = @@error		

			IF  @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting into DI_DrugItemTieredPriceHistory from reused ndc for fss contract ' + @ContractNumber
				GOTO ERROREXIT
			END
			
			delete DI_DrugItemTieredPrice
			Where DrugItemPriceId in ( select DrugItemPriceId 
										from DI_DrugItemPrice
										where DrugItemId = @discontinuedDrugItemIdToBeRemoved )

			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting tiered prices from reused ndc for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		


			/* Note: prices should have been moved to history already for the reused, discontinued item. */
			/* this is a failsafe */
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
				LastModificationType, ModificationStatusId, @Notes + 'UpdateDrugItemNDC', CreatedBy,
				CreationDate, LastModifiedBy, LastModificationDate 
			From DI_DrugItemPrice
			where DrugItemId = @discontinuedDrugItemIdToBeRemoved
			
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting prices into price history from reused ndc for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		
			
			delete  DI_DrugItemPrice
			where DrugItemId = @discontinuedDrugItemIdToBeRemoved
			
			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting prices from reused ndc for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		

			insert into DI_DrugItemPackageHistory
			( DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
			 ModificationStatusId,Notes,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate )
			select 
				DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
				@modificationStatusId, @Notes + 'UpdateDrugItemNDC', CreatedBy,CreationDate,@loginName,getdate()
			From Di_DrugItemPackage
			Where DrugItemId = @discontinuedDrugItemIdToBeRemoved

			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting item package history from reused ndc for contract ' + @ContractNumber
				GOTO ERROREXIT
			END
			
			delete DI_DrugItemPackage
			Where DrugItemId = @discontinuedDrugItemIdToBeRemoved

			select @error = @@error		

			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error deleting package from reused ndc for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		

		END

	END
	else
	BEGIN	
		Insert into DI_DrugItemNDC
		(FdaAssignedLabelerCode,ProductCode,PackageCode,ModificationStatusId,
		 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
		)
		Select @newfdaAssignedLabelerCode,@newproductCode,@newpackageCode,@modificationStatusId,
				@loginName,getdate(),@loginName,getdate()
		
		Select @newDrugItemNDCId = @@identity, @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error (2) updating to new NDC:  ' + @newfdaAssignedLabelerCode + ' ' +  @newproductCode + ' ' + @newpackageCode
			GOTO ERROREXIT
		END

	END

	Insert into Di_DrugItemsHistory
		( DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
		 DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
		 Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
		 ExcludeFromExport,NonTAA,IncludedFETAmount,LastModificationType,ModificationStatusId,Notes,CreatedBy,CreationDate,
		 LastModifiedBy,LastModificationDate 
		)
	Select 
		DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
		DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
		Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
		ExcludeFromExport,NonTAA,IncludedFETAmount,LastModificationType,ModificationStatusId, @Notes + 'UpdateDrugItemNDC',
		CreatedBy,CreationDate,@loginName,getdate() 
	From Di_DrugItems
	Where DrugItemId = @drugitemId

	select @error = @@error
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting items history for drugitem Id: '+ cast(@drugitemId as varchar)
		GOTO ERROREXIT
	END

	Update DI_DrugItems
		Set DrugItemNDCId = @newDrugItemNDCId,
			ModificationStatusId = @modificationStatusId,
			LastModifiedBy = @loginName,
			LastModificationDate= getdate()
	Where DrugItemId = @DrugItemId
		
	Select @error = @@error
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error Updating New NDC ID  ' + @newfdaAssignedLabelerCode + @newproductCode + @newpackageCode
		GOTO ERROREXIT
	END

	/* for in-place NDC change, effective date is day of edit */
	select @effectiveDate = CONVERT( datetime, CONVERT( varchar(50), getdate(), 101 ))
	
	insert into DI_ContractNDCNumberChange
	( NewContractId, NewDrugItemNDCId, NewDrugItemId, NewHistoricalNValue, OldContractId, OldDrugItemNDCId, OldDrugItemId, OldHistoricalNValue, ChangeStatus, ModificationId, EffectiveDate, EndDate, LastModifiedBy, LastModificationDate )
	values
	( @contractId, @newDrugItemNDCId, @drugitemId, @historicalNValue, @contractId, @CurrentDrugItemNDCId, @drugitemId, @historicalNValue, 'I', @modificationStatusId, @effectiveDate, null, @loginName, getdate() )

	Select @error = @@error
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting into table DI_ContractNDCNumberChange  '
		GOTO ERROREXIT
	END

GOTO OKEXIT

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

