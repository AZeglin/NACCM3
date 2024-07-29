IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertDistributorForDrugItem' )
BEGIN
	DROP PROCEDURE InsertDistributorForDrugItem
END
GO

CREATE PROCEDURE InsertDistributorForDrugItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@DrugItemId int,
@DistributorName nvarchar(100),
@Phone nvarchar(15),
@Notes nvarchar(800),
@ContactPerson nvarchar(30),
@DrugItemDistributorId int OUTPUT
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
		select @errorMsg = 'Error getting current user login during insert distributor.'
		goto ERROREXIT
	END
	insert into DI_DrugItemDistributors
	( DrugItemId, DistributorName, Phone, ContactPerson, Notes, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	values
	( @DrugItemId, @DistributorName, @Phone, @ContactPerson, @Notes, @currentUserLogin, GETDATE(), @currentUserLogin, GETDATE() )

	select @error = @@ERROR, @rowCount = @@ROWCOUNT, @DrugItemDistributorId = @@IDENTITY
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error inserting new distributor for drugItemId=' + CONVERT(nvarchar(20), @DrugItemId )
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


