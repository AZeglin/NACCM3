IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'InsertDLAItemChangeLog') AND type in (N'P', N'PC'))
	DROP PROCEDURE InsertDLAItemChangeLog
GO

CREATE Procedure InsertDLAItemChangeLog
(
@loginName  nvarchar(120),
@ContractNumber nvarchar(20),
@UpdateSource char(1),
@UpdateType char(1), -- 'N' = NDC, 'U' = unit of sale, 'B' = both
@DrugItemId int,

@DrugItemNDCId int = null,
@FdaAssignedLabelerCode char(5) = null,
@ProductCode char(4) = null,
@PackageCode char(2) = null,

@ExistingDrugItemNDCId int = null,
@ExistingFdaAssignedLabelerCode char(5) = null,
@ExistingProductCode char(4) = null,
@ExistingPackageCode char(2) = null,

@UnitOfSale nchar(2) = null,
@ExistingUnitOfSale nchar(2)  = null
)

AS

DECLARE @error int,		
		@errorMsg nvarchar(250)
		

BEGIN TRANSACTION	

	if @UpdateType = 'N' or @UpdateType = 'B'    
	BEGIN

		if @DrugItemNDCId <> @ExistingDrugItemNDCId or @FdaAssignedLabelerCode <> @ExistingFdaAssignedLabelerCode or @ProductCode <> @ExistingProductCode or @PackageCode <> @ExistingPackageCode
		BEGIN

			insert into DI_DLAItemChangeLog
			(
				UpdateSource, UpdateType, DrugItemId, ExistingDrugItemNDCId, ExistingFdaAssignedLabelerCode, ExistingProductCode, ExistingPackageCode,  NewDrugItemNDCId, NewFdaAssignedLabelerCode, NewProductCode, NewPackageCode, ExistingUnitOfSale, NewUnitOfSale, LastModificationDate
			)
			values
			(
				@UpdateSource,'N',  @DrugItemId, @ExistingDrugItemNDCId, @ExistingFdaAssignedLabelerCode, @ExistingProductCode, @ExistingPackageCode, @DrugItemNDCId, @FdaAssignedLabelerCode, @ProductCode, @PackageCode, null, null, getdate() 
			)

			select @error = @@error
	
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error logging ndc change to DLAItemChangeLog for contract ' + @ContractNumber
				GOTO ERROREXIT
			END
		END
	END

	if @UpdateType = 'U' or @UpdateType = 'B'    
	BEGIN

		if @UnitOfSale <> @ExistingUnitOfSale
		BEGIN

			insert into DI_DLAItemChangeLog
			(
				UpdateSource, UpdateType, DrugItemId, ExistingDrugItemNDCId, ExistingFdaAssignedLabelerCode, ExistingProductCode, ExistingPackageCode,  NewDrugItemNDCId, NewFdaAssignedLabelerCode, NewProductCode, NewPackageCode, ExistingUnitOfSale, NewUnitOfSale, LastModificationDate
			)
			values
			(
				@UpdateSource,'U',  @DrugItemId, null, null, null, null, null, null, null, null, @ExistingUnitOfSale, @UnitOfSale, getdate() 
			)

			select @error = @@error
	
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error logging unit of sale change to DLAItemChangeLog for contract ' + @ContractNumber
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



