IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteFSSDrugItemSubItem')
	BEGIN
		DROP  Procedure  DeleteFSSDrugItemSubItem
	END

GO

CREATE Procedure [dbo].[DeleteFSSDrugItemSubItem]
( 
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemSubItemId int,
@DrugItemId int,
@SubItemIdentifier nchar(1),
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
	
	/* block deletion if there are sub-item prices, however */
	/* do not hold up deletion for expired prices which will be latently removed anyway */
	if exists ( select DrugItemPriceId from DI_DrugItemPrice 
				where DrugItemId = @DrugItemId 
				and DrugItemSubItemId = @DrugItemSubItemId
				and getdate() between PriceStartDate and PriceStopDate )
	BEGIN
		select @errorMsg = 'Cannot delete SubItemIdentifier ' + @SubItemIdentifier + ' because it has a price associated with it. DrugItemId=' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT

	END
	else
	BEGIN
		Insert into DI_DrugItemSubItemsHistory
		(
			DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,
			TradeName,DispensingUnit,LastModificationType,ModificationStatusId,
			IsDeleted,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
		)
		Select 
			DrugItemSubItemId,DrugItemId,SubItemIdentifier,PackageDescription,Generic,
			TradeName,DispensingUnit,LastModificationType,ModificationStatusId,
			1,CreatedBy,CreationDate,@currentUserLogin,GETDATE()
		From DI_DrugItemSubItems
		Where DrugItemId = @DrugItemId
		and DrugItemSubItemId = @DrugItemSubItemId	
	
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error inserting sub-item history for fss contract ' + @ContractNumber
			goto ERROREXIT
		END
	
	
		delete DI_DrugItemSubItems
		where DrugItemId = @DrugItemId
		and DrugItemSubItemId = @DrugItemSubItemId	

		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error deleting sub-item for fss contract ' + @ContractNumber
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
