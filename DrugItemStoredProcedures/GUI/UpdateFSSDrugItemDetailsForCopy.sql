IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UpdateFSSDrugItemDetailsForCopy]') AND type in (N'P', N'PC'))
DROP PROCEDURE [UpdateFSSDrugItemDetailsForCopy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[UpdateFSSDrugItemDetailsForCopy]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@SourceDrugItemId int,
@DestinationDrugItemId int,
@SourceDrugItemPackageId int,      
@CopyType nvarchar(20),     /* CopyToSame, CopyToDestination */

@ModificationStatusId int,

@DateEnteredMarket datetime,
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
@DestinationCreatedBy nvarchar(120), /* may want to preserve original creator */
@DestinationCreationDate datetime,
@LastModificationType nchar(1),  /* 'O' = copy; 'N' = NDC Change */
@NewDrugItemPackageId int OUTPUT
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@currentUserName nvarchar(120)

	
BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserName OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during update of item details for copy for fss contract for contract ' + @ContractNumber
		goto ERROREXIT
	END

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
		ExcludeFromExport,NonTAA,IncludedFETAmount,ParentDrugItemId,LastModificationType,ModificationStatusId,'UpdateFSSDrugItemDetailsForCopy',
		@DestinationCreatedBy,@DestinationCreationDate,@currentUserName,getdate() 
	From Di_DrugItems
	Where DrugItemId = @DestinationDrugItemId

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting items history for contract ' + @ContractNumber
		GOTO ERROREXIT
	END

	update DI_DrugItems
	set DateEnteredMarket = @DateEnteredMarket,
		PrimeVendor = @PrimeVendor,
		PrimeVendorChangedDate = @PrimeVendorChangedDate,
		PassThrough = @PassThrough,
		VAClass = @VAClass,
		ExcludeFromExport = @ExcludeFromExport,
		NonTAA = @NonTAA, 
		IncludedFETAmount = @IncludedFETAmount,
		CreatedBy = @DestinationCreatedBy,
		CreationDate = @DestinationCreationDate,
		LastModifiedBy = @currentUserName,
		LastModificationDate = getdate(),
		LastModificationType = @LastModificationType,   /* O = copy or N = NDC Change */
		ModificationStatusId = @ModificationStatusId
	where DrugItemId = @DestinationDrugItemId
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error updating fss drug item details for copy for contract ' + @ContractNumber
		goto ERROREXIT
	END

	if @CopyType = 'CopyToSame'
	BEGIN
		/* always insert, since user was prompted during copy operation */
		insert into DI_DrugItemPackage
		( DrugItemId, UnitOfSale, QuantityInUnitOfSale, UnitPackage, QuantityInUnitPackage, UnitOfMeasure, PriceMultiplier, PriceDivider, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		values
		( @DestinationDrugItemId, @UnitOfSale, @QuantityInUnitOfSale, @UnitPackage, @QuantityInUnitPackage, @UnitOfMeasure, @PriceMultiplier, @PriceDivider, @ModificationStatusId, @DestinationCreatedBy, @DestinationCreationDate, @currentUserName, getdate() )

		select @error = @@error, @rowcount = @@rowcount, @NewDrugItemPackageId = @@Identity
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error inserting new drug item packaging details for copy for fss contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else if @CopyType = 'CopyToDestination'
	BEGIN
		/* if the source had package info, insert it for destination as well */
		if @SourceDrugItemPackageId <> -1
		BEGIN
			insert into DI_DrugItemPackage
			( DrugItemId, UnitOfSale, QuantityInUnitOfSale, UnitPackage, QuantityInUnitPackage, UnitOfMeasure, PriceMultiplier, PriceDivider, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
			values
			( @DestinationDrugItemId, @UnitOfSale, @QuantityInUnitOfSale, @UnitPackage, @QuantityInUnitPackage, @UnitOfMeasure, @PriceMultiplier, @PriceDivider, @ModificationStatusId, @DestinationCreatedBy, @DestinationCreationDate, @currentUserName, getdate() )

			select @error = @@error, @rowcount = @@rowcount, @NewDrugItemPackageId = @@Identity
			
			if @error <> 0 or @rowcount <> 1
			BEGIN
				select @errorMsg = 'Error inserting new drug item packaging details for copy for fss contract ' + @ContractNumber
				goto ERROREXIT
			END
		END
	END
	else
	BEGIN
		select @errorMsg = 'Error unknown copy type.'
		GOTO ERROREXIT	
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




 