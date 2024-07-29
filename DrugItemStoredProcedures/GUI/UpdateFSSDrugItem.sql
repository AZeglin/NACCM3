IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UpdateFSSDrugItem]') AND type in (N'P', N'PC'))
DROP PROCEDURE [UpdateFSSDrugItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[UpdateFSSDrugItem]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemId int,
@DrugItemNDCId int,
@FdaAssignedLabelerCode char(5),
@ProductCode char(4),
@PackageCode char(2),
@Covered nchar(1),
@Generic nvarchar(64),
@TradeName nvarchar(45),
@DispensingUnit nvarchar(10),
@PackageDescription nvarchar(14),
@ParentDrugItemId int,
@ModificationStatusId int,
@IsBPA bit


)

AS

DECLARE @contractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@currentUserLogin nvarchar(120),
	@ExistingFdaAssignedLabelerCode char(5),
	@ExistingProductCode char(4),
	@ExistingPackageCode char(2),
	@retVal int,
	@OriginalParentDrugItemId int,
	@UnitOfSale nchar(2),
	@ExistingUnitOfSale nchar(2),
	@Notes nvarchar(2000)

BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during update for contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	select @contractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId for contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	select 	@ExistingFdaAssignedLabelerCode  = FdaAssignedLabelerCode,
	@ExistingProductCode = ProductCode,
	@ExistingPackageCode = PackageCode
	from DI_DrugItemNDC
	where DrugItemNDCId = @DrugItemNDCId
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting NDC for contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	select @Notes = 'UpdateFSSDrugItem'
	
	if @FdaAssignedLabelerCode <> @ExistingFdaAssignedLabelerCode OR
		@ProductCode <> @ExistingProductCode OR
		@PackageCode <> @ExistingPackageCode
	BEGIN
	
		Exec @retVal = UpdateDrugItemNDC  @DrugItemId,@DrugItemNDCId,@FdaAssignedLabelerCode,
										  @ProductCode,@PackageCode,@currentUserLogin,@ModificationStatusId, @Notes

		select @error = @@error
		
		if @error <> 0 or @retVal = -1
		BEGIN
			select @errorMsg = 'Error updating drug item NDC for contract ' + @ContractNumber
			goto ERROREXIT
		END	
	
		Exec @retVal = InsertDLAItemChangeLog  @currentUserLogin, @ContractNumber, 'G', 'N', @DrugItemId, @DrugItemNDCId, @FdaAssignedLabelerCode, @ProductCode, @PackageCode,
								@DrugItemNDCId, @ExistingFdaAssignedLabelerCode, @ExistingProductCode, @ExistingPackageCode, null, null

		select @error = @@error
		
		if @error <> 0 or @retVal = -1
		BEGIN
			select @errorMsg = 'Error inserting into DLAItemChangeLog for contract ' + @ContractNumber
			goto ERROREXIT
		END	
	
	END
	else /* note: history is updated if NDC has changed, dont want to update it twice */
	BEGIN 
		Insert into Di_DrugItemsHistory
		(DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
		 DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
		 Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
		 ExcludeFromExport,NonTAA,IncludedFETAmount,ParentDrugItemId,LastModificationType,ModificationStatusId,Notes,CreatedBy,CreationDate,
		 LastModifiedBy,LastModificationDate 
		)
		Select 
			DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
			DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
			Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
			ExcludeFromExport,NonTAA,IncludedFETAmount,ParentDrugItemId,LastModificationType,ModificationStatusId,'UpdateFSSDrugItem',
			CreatedBy,CreationDate,@currentUserLogin,getdate() 
		From Di_DrugItems
		Where DrugItemId = @drugitemId

		select @error = @@error
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting items history for drugitem Id: '+ cast(@drugitemId as varchar)
			goto ERROREXIT
		END	
	END
	
	if @IsBPA = 0
	BEGIN

		update DI_DrugItems
		set	Covered = @Covered,
			Generic = LTRIM(RTRIM(@Generic)),
			TradeName = LTRIM(RTRIM(@TradeName)),
			DispensingUnit = @DispensingUnit,
			PackageDescription = @PackageDescription,
			ParentDrugItemId = @ParentDrugItemId,
			ModificationStatusId = @ModificationStatusId,
			LastModificationType = 'C',
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = getdate()
		where DrugItemId = @DrugItemId
	    
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error updating drug item for contract ' + @ContractNumber
			goto ERROREXIT
		END
			
	END
	else
	BEGIN
	
		/* determine if the parent was reselected */
		select @OriginalParentDrugItemId = ParentDrugItemId
		from DI_DrugItems 
		where DrugItemId = @DrugItemId
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving original ParentDrugItemId when updating drug item for contract ' + @ContractNumber
			goto ERROREXIT
		END
		
		if @OriginalParentDrugItemId <> @ParentDrugItemId
		BEGIN

			/* update details from new parent */
			update i
			set	Covered = @Covered,
				Generic = @Generic,
				TradeName = @TradeName,
				DispensingUnit = @DispensingUnit,
				PackageDescription = @PackageDescription,
				ParentDrugItemId = @ParentDrugItemId,
				DateEnteredMarket = p.DateEnteredMarket,
				PrimeVendor = p.PrimeVendor,
				PrimeVendorChangedDate = p.PrimeVendorChangedDate,
				PassThrough = p.PassThrough,
				VAClass = p.VAClass,	
				ModificationStatusId = @ModificationStatusId,
				LastModificationType = 'C',
				LastModifiedBy = @currentUserLogin,
				LastModificationDate = getdate()
			from DI_DrugItems i, DI_DrugItems p
			where p.DrugItemId = @ParentDrugItemId
			and i.DrugItemId = @DrugItemId
		    
			select @error = @@error
			
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error updating drug item with parent details for contract ' + @ContractNumber
				goto ERROREXIT
			END
		
			/* update packaging from new parent, if available */
			if exists ( select DrugItemId from DI_DrugItemPackage where DrugItemId = @ParentDrugItemId ) 
			BEGIN
				/* update or insert new */
				if exists ( select DrugItemId from DI_DrugItemPackage where DrugItemId = @DrugItemId ) 
				BEGIN

					/* save unit of sale info for DLA logging purposes */
					select @ExistingUnitOfSale = UnitOfSale
					from DI_DrugItemPackage where DrugItemId = @OriginalParentDrugItemId

					select @error = @@error
		
					if @error <> 0
					BEGIN
						select @errorMsg = 'Error retrieving original UnitOfSale when updating drug item for contract ' + @ContractNumber
						goto ERROREXIT
					END

					select @UnitOfSale = UnitOfSale
					from DI_DrugItemPackage where DrugItemId = @ParentDrugItemId

					select @error = @@error
		
					if @error <> 0
					BEGIN
						select @errorMsg = 'Error retrieving parent UnitOfSale when updating drug item for contract ' + @ContractNumber
						goto ERROREXIT
					END

					update p 
					set UnitOfSale = n.UnitOfSale, 
						QuantityInUnitOfSale = n.QuantityInUnitOfSale, 
						UnitPackage = n.UnitPackage, 
						QuantityInUnitPackage = n.QuantityInUnitPackage, 
						UnitOfMeasure = n.UnitOfMeasure, 
						ModificationStatusId = @ModificationStatusId, 
						LastModifiedBy = @currentUserLogin,
						LastModificationDate = getdate()
					from DI_DrugItemPackage p, DI_DrugItemPackage n
					where p.DrugItemId = @DrugItemId
					and n.DrugItemId = @ParentDrugItemId

					select @error = @@error, @rowcount = @@rowcount
					
					if @error <> 0 or @rowcount <> 1
					BEGIN
						select @errorMsg = 'Error retrieving drug item packaging details from parent for update for contract ' + @ContractNumber
						goto ERROREXIT
					END
					
					Exec @retVal = InsertDLAItemChangeLog  @currentUserLogin, @ContractNumber, 'G', 'U', @DrugItemId, null, null, null, null, null, null, null, null, 
										@UnitOfSale, @ExistingUnitOfSale
					select @error = @@error
		
					if @error <> 0 or @retVal = -1
					BEGIN
						select @errorMsg = 'Error inserting unit of sale into DLAItemChangeLog for contract ' + @ContractNumber
						goto ERROREXIT
					END	
				
				END
				else
				BEGIN
					insert into DI_DrugItemPackage
					( DrugItemId, UnitOfSale, QuantityInUnitOfSale, UnitPackage, QuantityInUnitPackage, UnitOfMeasure, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
					select @DrugItemId, 
						UnitOfSale, 
						QuantityInUnitOfSale, 
						UnitPackage, 
						QuantityInUnitPackage, 
						UnitOfMeasure, 
						@ModificationStatusId, 
						@currentUserLogin, 
						getdate(), 
						@currentUserLogin, 
						getdate() 
					from DI_DrugItemPackage
					where DrugItemId = @ParentDrugItemId

					select @error = @@error, @rowcount = @@rowcount
					
					if @error <> 0 or @rowcount <> 1
					BEGIN
						select @errorMsg = 'Error retrieving drug item packaging details from parent for insert for contract ' + @ContractNumber
						goto ERROREXIT
					END
					
				END
			END
		END
		else
		BEGIN
		
			update DI_DrugItems
			set	Covered = @Covered,
				Generic = @Generic,
				TradeName = @TradeName,
				DispensingUnit = @DispensingUnit,
				PackageDescription = @PackageDescription,
				ParentDrugItemId = @ParentDrugItemId,
				ModificationStatusId = @ModificationStatusId,
				LastModificationType = 'C',
				LastModifiedBy = @currentUserLogin,
				LastModificationDate = getdate()
			where DrugItemId = @DrugItemId
		    
			select @error = @@error
			
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error updating drug item (2) for contract ' + @ContractNumber
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



