IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteContractCheck2' )
BEGIN
	DROP PROCEDURE DeleteContractCheck2
END
GO

CREATE PROCEDURE DeleteContractCheck2
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ContractId int,
@CheckId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserLogin nvarchar(120)
	
BEGIN TRANSACTION

	exec dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 
	Select @error = @@error		
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert( nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	DELETE FROM CM_Checks 
		Output 'CM_Checks', Deleted.CheckId, ( @currentUserLogin ), ( GETDATE() ) into Audit_Deleted_Data_By_User 
		WHERE CheckId = @CheckId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting check from contract.'
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


