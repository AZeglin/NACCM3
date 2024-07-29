IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DeleteFSSItemAndPrices]') AND type in (N'P', N'PC'))
DROP PROCEDURE [DeleteFSSItemAndPrices]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[DeleteFSSItemAndPrices]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ModificationStatusId int,
@DrugItemId int
)

As
	
	Declare @error int,@errorMsg nvarchar(1000), @UserName nvarchar(120),@drugItemPriceId int,
			@retval int

	Begin Tran
	
		EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @UserName OUTPUT 

		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error getting current user login during delete of all item prices for @DrugItemId=' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END


		Exec @retVal = DeleteFSSPricesForItemId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @ContractNumber, @ModificationStatusId, @DrugItemId

		select @error = @@error
		
		if @retVal = -1 or @error <> 0 
		BEGIN
			select @errorMsg = 'Error deleting prices for @DrugItemId=' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END		

		Insert into Di_DrugitempackageHistory
		(DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure,
		 ModificationStatusId,IsDeleted,Notes,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate	
		)
		Select 
			DrugItemPackageId,DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure,
			ModificationStatusId,1,'DeleteFSSItemAndPrices',CreatedBy,CreationDate,@UserName,getdate()
		From Di_Drugitempackage
		Where DrugItemId = @drugitemId

		select @error = @@error

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting item package history for drugitem Id: '+ cast(@drugitemId as varchar)
			goto ERROREXIT
		END		
		
		Delete From Di_Drugitempackage
		Where DrugItemId = @drugitemId	

		select @error = @@error

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error deleting item package for drugitem Id: '+ cast(@drugitemId as varchar)
			goto ERROREXIT
		END	


		Insert into DI_DrugItemSubItemsHistory
		(
			DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,
			TradeName,DispensingUnit,LastModificationType,ModificationStatusId,
			IsDeleted,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
		)
		Select 
			DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,
			TradeName,DispensingUnit,LastModificationType,ModificationStatusId,
			1,CreatedBy,CreationDate,@UserName,GETDATE()
		From DI_DrugItemSubItems
		Where DrugItemId = @DrugItemId
		
	
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error inserting sub-item history for fss contract ' + @ContractNumber
			goto ERROREXIT
		END
	
	
		delete DI_DrugItemSubItems
		where DrugItemId = @DrugItemId
		

		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error deleting sub-item for fss contract ' + @ContractNumber
			goto ERROREXIT
		END



		Insert into Di_DrugItemsHistory
		(DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
		 DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
		 Covered,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
		 ExcludeFromExport,NonTAA, IncludedFETAmount,ParentDrugItemId,LastModificationType,ModificationStatusId,IsDeleted,Notes,CreatedBy,CreationDate,
		 LastModifiedBy,LastModificationDate 
		)
		Select 
			DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
			DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
			Covered,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
			ExcludeFromExport,NonTAA, IncludedFETAmount,ParentDrugItemId,LastModificationType,ModificationStatusId,1,'DeleteFSSItemAndPrices',
			CreatedBy,CreationDate,@UserName,getdate() 
		From Di_DrugItems
		Where DrugItemId = @drugitemId

		select @error = @@error

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting item history for drugitem Id: '+ cast(@drugitemId as varchar)
			goto ERROREXIT
		END	

		Delete From Di_Drugitems
		Where DrugItemId = @drugitemId	

		select @error = @@error

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error deleting item for drugitem Id: '+ cast(@drugitemId as varchar)
			goto ERROREXIT
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
			/* only rollback iff this the highest level */
			ROLLBACK TRANSACTION
	  END

	  RETURN( -1 )

OKEXIT:
	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )
