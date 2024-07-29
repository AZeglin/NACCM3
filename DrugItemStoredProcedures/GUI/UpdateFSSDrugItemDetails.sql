IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UpdateFSSDrugItemDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [UpdateFSSDrugItemDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[UpdateFSSDrugItemDetails]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemId int,
@DrugItemPackageId int,      /* -1 = insert new */     
@ModificationStatusId int,

@DateEnteredMarket datetime,
--@DualPriceDesignation nchar(1),
@PrimeVendor nchar(1),
@PrimeVendorChangedDate datetime,
@PassThrough nchar(1),
@VAClass nvarchar(5),
@ExcludeFromExport bit,
@NonTAA bit,
@IncludedFETAmount float,

@UnitOfSale nchar(2),
@QuantityInUnitOfSale decimal(5,0),
@UnitPackage nchar(2),
@QuantityInUnitPackage decimal(13,5),
@UnitOfMeasure nchar(2),
@PriceMultiplier int, 
@PriceDivider int,

@NewDrugItemPackageId int OUTPUT
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@retVal int,
	@currentUserName nvarchar(120),
	@ExistingUnitOfSale nchar(2)

	
BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserName OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during update of item details for fss contract for contract ' + @ContractNumber
		goto ERROREXIT
	END

	Insert into Di_DrugItemsHistory
	(DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
	 DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
	 Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
	 ExcludeFromExport, NonTAA, IncludedFETAmount, ParentDrugItemId,LastModificationType,ModificationStatusId,Notes,CreatedBy,CreationDate,
	 LastModifiedBy,LastModificationDate 
	)
	Select 
		DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
		DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
		Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
		ExcludeFromExport, NonTAA, IncludedFETAmount, ParentDrugItemId,LastModificationType,ModificationStatusId,'UpdateFSSDrugItemDetails',
		CreatedBy,CreationDate,@currentUserName,getdate() 
	From Di_DrugItems
	Where DrugItemId = @drugitemId

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting items history for contract ' + @ContractNumber
		GOTO ERROREXIT
	END

	update DI_DrugItems
	set DateEnteredMarket = @DateEnteredMarket,
	--	DualPriceDesignation = @DualPriceDesignation,
		PrimeVendor = @PrimeVendor,
		PrimeVendorChangedDate = @PrimeVendorChangedDate,
		PassThrough = @PassThrough,
		VAClass = @VAClass,
		ExcludeFromExport = @ExcludeFromExport,
		NonTAA = @NonTAA,
		IncludedFETAmount = @IncludedFETAmount,
		LastModifiedBy = @currentUserName,
		LastModificationDate = getdate(),
		LastModificationType = 'C',
		ModificationStatusId = @ModificationStatusId
	--from DI_DrugItems i
	where DrugItemId = @DrugItemId
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error updating fss drug item details for contract ' + @ContractNumber
		goto ERROREXIT
	END

	if @DrugItemPackageId = -1
	BEGIN
		insert into DI_DrugItemPackage
		( DrugItemId, UnitOfSale, QuantityInUnitOfSale, UnitPackage, QuantityInUnitPackage, UnitOfMeasure, PriceMultiplier, PriceDivider, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		values
		( @DrugItemId, @UnitOfSale, @QuantityInUnitOfSale, @UnitPackage, @QuantityInUnitPackage, @UnitOfMeasure, @PriceMultiplier, @PriceDivider, @ModificationStatusId, @currentUserName, getdate(), @currentUserName, getdate() )

		select @error = @@error, @rowcount = @@rowcount, @NewDrugItemPackageId = @@Identity
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error inserting new drug item packaging details for fss contract ' + @ContractNumber
			goto ERROREXIT
		END

	END
	else
	BEGIN
		Insert into Di_DrugitempackageHistory
		(DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
		 ModificationStatusId,Notes,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate	
		)
		Select 
			DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure, PriceMultiplier, PriceDivider,
			@modificationStatusId,'UpdateFSSDrugItemDetails',CreatedBy,CreationDate,@currentUserName,getdate()
		From Di_Drugitempackage
		Where DrugItemId = @drugitemId

		select @error = @@error		

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting item package history for contract ' + @ContractNumber
			GOTO ERROREXIT
		END

		/* save unit of sale info for DLA logging purposes */
		select @ExistingUnitOfSale = UnitOfSale
		from DI_DrugItemPackage where DrugItemPackageId = @DrugItemPackageId

		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving original UnitOfSale when updating drug item for contract ' + @ContractNumber
			goto ERROREXIT
		END
				
		update DI_DrugItemPackage
			set	UnitOfSale = @UnitOfSale,
			QuantityInUnitOfSale = @QuantityInUnitOfSale,
			UnitPackage = @UnitPackage,
			QuantityInUnitPackage = @QuantityInUnitPackage,
			UnitOfMeasure = @UnitOfMeasure,
			PriceMultiplier = @PriceMultiplier, 
			PriceDivider = @PriceDivider,
			LastModifiedBy = @currentUserName,
			LastModificationDate = getdate()
		where DrugItemPackageId = @DrugItemPackageId
		
		select @error = @@error, @rowcount = @@rowcount
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error updating drug item packaging details for fss contract ' + @ContractNumber
			goto ERROREXIT
		END


		Exec @retVal = InsertDLAItemChangeLog  @currentUserName, @ContractNumber, 'G', 'U', @DrugItemId, null, null, null, null, null, null, null, null, 
										@UnitOfSale, @ExistingUnitOfSale
		select @error = @@error
		
		if @error <> 0 or @retVal = -1
		BEGIN
			select @errorMsg = 'Error inserting unit of sale into DLAItemChangeLog for contract ' + @ContractNumber
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




