IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteDrugItemDistributor' )
BEGIN
	DROP PROCEDURE DeleteDrugItemDistributor
END
GO

CREATE PROCEDURE DeleteDrugItemDistributor
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@DrugItemDistributorId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
	@currentUserLogin nvarchar(120)



BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during delete distributor.'
		goto ERROREXIT
	END

	insert into DI_DrugItemDistributorsHistory
	( DrugItemDistributorId, DrugItemId, DistributorName, Phone, ContactPerson, Notes, IsDeleted, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	select DrugItemDistributorId, DrugItemId, DistributorName, Phone, ContactPerson, Notes, 1, CreatedBy, CreationDate, @currentUserLogin, GETDATE()
	from DI_DrugItemDistributors
	where DrugItemDistributorId = @DrugItemDistributorId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error inserting distributor history during delete for drugItemDistributorId=' + CONVERT(nvarchar(20), @DrugItemDistributorId )
		goto ERROREXIT
	END

	delete DI_DrugItemDistributors
	where DrugItemDistributorId = @DrugItemDistributorId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting distributor for drug item.'
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


