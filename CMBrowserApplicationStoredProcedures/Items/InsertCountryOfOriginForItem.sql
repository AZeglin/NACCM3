IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertCountryOfOriginForItem' )
BEGIN
	DROP PROCEDURE InsertCountryOfOriginForItem
END
GO

CREATE PROCEDURE InsertCountryOfOriginForItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ItemId int,
@CountryId int,
@ModificationStatusId int,
@ModificationType nchar(1),
@ItemCountryId int OUTPUT
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
		select @errorMsg = 'Error getting current user login during insert of country for @ItemId=' + convert( nvarchar(20), @ItemId )
		goto ERROREXIT
	END		

	if not exists ( select ItemCountryId from CM_ItemCountries 	where ItemId = @ItemId 	and CountryId = @CountryId )
	BEGIN
		insert into CM_ItemCountries
		( ItemId, CountryId, LastModificationType, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate  )
		values
		( @ItemId, @CountryId, @ModificationType, @ModificationStatusId, @UserName, getdate(), @UserName, getdate() )

		select @error = @@ERROR, @ItemCountryId = SCOPE_IDENTITY()
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting country of origin for item.'
			goto ERROREXIT
		END
	END
	else
	BEGIN
		select @ItemCountryId = -1
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



