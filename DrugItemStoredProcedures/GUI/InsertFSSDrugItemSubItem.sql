IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertFSSDrugItemSubItem')
	BEGIN
		DROP  Procedure  InsertFSSDrugItemSubItem
	END

GO

CREATE Procedure InsertFSSDrugItemSubItem
( 
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemId int,
@SubItemIdentifier nchar(1),
@PackageDescription nvarchar(14),
@Generic nvarchar(64),
@TradeName nvarchar(45),
@DispensingUnit nvarchar(10),
@ModificationStatusId int,
@DrugItemSubItemId int OUTPUT
)

AS


DECLARE @error int,
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
				and SubItemIdentifier = @SubItemIdentifier )
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
		
			insert into DI_DrugItemSubItems
			( DrugItemId, SubItemIdentifier, PackageDescription, Generic, TradeName, DispensingUnit, LastModificationType, ModificationStatusId,
				CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
			values
			( @DrugItemId, @SubItemIdentifier, @PackageDescription, @Generic, @TradeName, @DispensingUnit, 'C', @ModificationStatusId,
				@currentUserLogin, getdate(), @currentUserLogin, getdate() )

			select @error = @@error, @DrugItemSubItemId = @@identity
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting sub-item for fss contract ' + @ContractNumber
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

