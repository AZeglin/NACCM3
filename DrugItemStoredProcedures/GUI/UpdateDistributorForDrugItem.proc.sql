IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateDistributorForDrugItem' )
BEGIN
	DROP PROCEDURE UpdateDistributorForDrugItem
END
GO

CREATE PROCEDURE UpdateDistributorForDrugItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@DrugItemId int,
@DrugItemDistributorId int,
@DistributorName nvarchar(100),
@Phone nvarchar(15),
@ContactPerson nvarchar(30),
@Notes nvarchar(800)
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
		select @errorMsg = 'Error getting current user login during update distributor.'
		goto ERROREXIT
	END

	insert into DI_DrugItemDistributorsHistory
	( DrugItemDistributorId, DrugItemId, DistributorName, Phone, ContactPerson, Notes, IsDeleted, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	select DrugItemDistributorId, DrugItemId, DistributorName, Phone, ContactPerson, Notes, 0, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate 
	from DI_DrugItemDistributors
	where DrugItemDistributorId = @DrugItemDistributorId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error inserting distributor history during update for drugItemId=' + CONVERT(nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END

	update DI_DrugItemDistributors
	set DistributorName = @DistributorName,
		Phone = @Phone,
		ContactPerson = @ContactPerson,
		Notes = @Notes,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where DrugItemDistributorId = @DrugItemDistributorId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating distributor for drugItemId=' + CONVERT(nvarchar(20), @DrugItemId )
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


