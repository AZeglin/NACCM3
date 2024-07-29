IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteContractSBAProjection' )
BEGIN
	DROP PROCEDURE DeleteContractSBAProjection
END
GO

CREATE PROCEDURE DeleteContractSBAProjection
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ProjectionId int
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

	DELETE FROM tbl_sba_Projection 
	Output 'tbl_sba_Projection', Deleted.ProjectionID, ( @currentUserLogin ), ( GETDATE() ) into Audit_Deleted_Data_By_User 
	WHERE ProjectionID = @ProjectionId

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting sba plan projection from contract.'
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


