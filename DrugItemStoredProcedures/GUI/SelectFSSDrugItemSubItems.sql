IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectFSSDrugItemSubItems')
	BEGIN
		DROP  Procedure  SelectFSSDrugItemSubItems
	END

GO

CREATE Procedure SelectFSSDrugItemSubItems
(
@CurrentUser uniqueidentifier,
@DrugItemId int,
@WithAdd bit = 0
)

AS

DECLARE
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@DefaultPackageDescription as nvarchar(14),
	@DefaultGeneric as nvarchar(64),
	@DefaultTradeName as nvarchar(45),
	@DefaultDispensingUnit as nvarchar(10),
	@DrugItemSubItemId int,
	@SubItemIdentifier as nchar(1),
	@PackageDescription as nvarchar(14),
	@Generic as nvarchar(64),
	@TradeName as nvarchar(45),
	@DispensingUnit as nvarchar(10),
	@LastModificationType as nchar(1),
	@ModificationStatusId as int,
	@CreatedBy as nvarchar(120),
	@CreationDate as datetime,
	@LastModifiedBy as nvarchar(120),
	@LastModificationDate as datetime,
	@IsNewBlankRow bit


BEGIN

	if @WithAdd = 0
	BEGIN
		select  DrugItemSubItemId, DrugItemId, SubItemIdentifier, PackageDescription, Generic, TradeName,
			DispensingUnit, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate,
			0 as IsNewBlankRow
		from DI_DrugItemSubItems
		where DrugItemId = @DrugItemId
		order by SubItemIdentifier

		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving subitems for DrugItemId=' + convert( nvarchar(20), @DrugItemId )
			raiserror( @errorMsg, 16, 1 )
		END
	END
	else
	BEGIN
		/* gather defaults from master item record */
		select @DefaultPackageDescription = PackageDescription,
			@DefaultGeneric = Generic,
			@DefaultTradeName = TradeName,
			@DefaultDispensingUnit = DispensingUnit
		from DI_DrugItems
		where DrugItemId = @DrugItemId
		
		select @error = @@error, @rowcount = @@rowcount
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error retrieving default descriptions from master drug item record for DrugItemId=' + convert( nvarchar(20), @DrugItemId )	
			raiserror( @errorMsg, 16, 1 )
		END
	
	
		/* new blank row definition */
		select @DrugItemSubItemId = -1,
		@SubItemIdentifier = '',
		@PackageDescription = @DefaultPackageDescription,
		@Generic = @DefaultGeneric,
		@TradeName = @DefaultTradeName,
		@DispensingUnit = @DefaultDispensingUnit,
		@LastModificationType = 'C',
		@ModificationStatusId = -1,
		@CreatedBy = '',
		@CreationDate = getdate(),
		@LastModifiedBy = '',
		@LastModificationDate = getdate(),
		@IsNewBlankRow = 1
	
	
	
		select  DrugItemSubItemId, DrugItemId, SubItemIdentifier, PackageDescription, Generic, TradeName,
			DispensingUnit, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate,
			0 as IsNewBlankRow
		from DI_DrugItemSubItems
		where DrugItemId = @DrugItemId
		
		union
		
		select @DrugItemSubItemId as DrugItemSubItemId,
			@DrugItemId as DrugItemId,
			@SubItemIdentifier as SubItemIdentifier,
			@PackageDescription as PackageDescription,
			@Generic as Generic,
			@TradeName as TradeName,
			@DispensingUnit as DispensingUnit,
			@LastModificationType as LastModificationType,
			@ModificationStatusId as ModificationStatusId,
			@CreatedBy as CreatedBy,
			@CreationDate as CreationDate,
			@LastModifiedBy as LastModifiedBy,
			@LastModificationDate as LastModificationDate,
			@IsNewBlankRow as IsNewBlankRow
	
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving subitems for DrugItemId=' + convert( nvarchar(20), @DrugItemId )
			raiserror( @errorMsg, 16, 1 )
		END
	END
END

