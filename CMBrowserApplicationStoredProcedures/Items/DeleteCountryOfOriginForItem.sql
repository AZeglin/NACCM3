IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteCountryOfOriginForItem' )
BEGIN
	DROP PROCEDURE DeleteCountryOfOriginForItem
END
GO

CREATE PROCEDURE DeleteCountryOfOriginForItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ItemId int,
@CountryId int,
@ModificationStatusId int,
@ModificationType nchar(1)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@UserName nvarchar(120)
	
		
BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @UserName OUTPUT 

	select @error = @@error
		
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during insert of country.'
		goto ERROREXIT
	END		

	if exists ( select ItemCountryId from CM_ItemCountries 	where ItemId = @ItemId 	and CountryId = @CountryId )
	BEGIN
		insert into CM_ItemCountriesHistory    
		( ItemCountryId, ItemId, CountryId, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, ReasonMovedToHistory, Notes, MovedToHistoryBy, DateMovedToHistory )
		select ItemCountryId, ItemId, CountryId, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate, 'D', 'DeleteCountryOfOriginForItem', @UserName, getdate() 
		from CM_ItemCountries
		where ItemId = @ItemId
		and CountryId = @CountryId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error inserting country of origin to history during delete.'
			goto ERROREXIT
		END
	
		delete CM_ItemCountries	
		Output 'CM_ItemCountries', Deleted.ItemCountryId, @UserName, GETDATE() into Audit_Deleted_Data_By_User
		where ItemId = @ItemId
		and CountryId = @CountryId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error deleting country of origin from item.'
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
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )



