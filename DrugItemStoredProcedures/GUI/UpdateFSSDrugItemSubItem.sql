IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateFSSDrugItemSubItem')
	BEGIN
		DROP  Procedure  UpdateFSSDrugItemSubItem
	END

GO

CREATE Procedure UpdateFSSDrugItemSubItem
( 
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemSubItemId int,
@DrugItemId int,
@SubItemIdentifier nchar(1),
@PackageDescription nvarchar(14),
@Generic nvarchar(64),
@TradeName nvarchar(45),
@DispensingUnit nvarchar(10),
@ModificationStatusId int
)

AS


DECLARE @rowcount int,
	 @error int,
	@errorMsg nvarchar(250),
	@currentUserLogin nvarchar(120)

BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during insert sub-item for fss contract ' + @ContractNumber
		goto ERROREXIT
	END
	
	if exists ( select SubItemIdentifier from DI_DrugItemSubItems
				where DrugItemId = @DrugItemId 
				and SubItemIdentifier = @SubItemIdentifier
				and DrugItemSubItemId <> @DrugItemSubItemId )
	BEGIN	
		select @errorMsg = 'SubItemIdentifier ' + @SubItemIdentifier + ' already exists for item with DrugItemId=' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	else
	BEGIN
		/* attempted SubItemIdentifier may be in use in the master item */
		if exists ( select HistoricalNValue 
					from DI_DrugItems
					where DrugItemId = @DrugItemId
					and HistoricalNValue = @SubItemIdentifier )
		BEGIN
			select @errorMsg = 'SubItemIdentifier ' + @SubItemIdentifier + ' chosen is in use ( internally ) by the master item. Please choose a different identifier. DrugItemId=' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END
		else
		BEGIN
			Insert into DI_DrugItemSubItemsHistory
			(DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,LastModificationType,
				ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
			)
			Select DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,'C',
				ModificationStatusId,CreatedBy,CreationDate,USER_NAME(),GETDATE()
			From DI_DrugItemSubItems
			Where DrugItemSubItemId = @DrugItemSubItemId	
			And DrugItemId = @DrugItemId
			
			if @error <> 0 or @rowcount <> 1
			BEGIN
				select @errorMsg = 'Error Inserting sub-item into History for fss contract ' + @ContractNumber
				goto ERROREXIT
			END		
		
			update DI_DrugItemSubItems
			set SubItemIdentifier = @SubItemIdentifier,
				PackageDescription = @PackageDescription,
				Generic = @Generic,
				TradeName = @TradeName,
				DispensingUnit = @DispensingUnit,
				LastModificationType = 'C',
				LastModifiedBy = @currentUserLogin,
				LastModificationDate = getdate()
			where DrugItemId = @DrugItemId
			and DrugItemSubItemId = @DrugItemSubItemId	

			select @error = @@error, @rowcount = @@rowcount
			
			if @error <> 0 or @rowcount <> 1
			BEGIN
				select @errorMsg = 'Error updating sub-item for fss contract ' + @ContractNumber
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

