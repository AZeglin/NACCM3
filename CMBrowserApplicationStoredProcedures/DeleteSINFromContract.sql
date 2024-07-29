IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteSINFromContract' )
BEGIN
	DROP PROCEDURE DeleteSINFromContract
END
GO

CREATE PROCEDURE DeleteSINFromContract
(
@UserLogin nvarchar(120),
@UserId uniqueidentifier,
@ContractNumber nvarchar(20),
@SIN varchar(10)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	DELETE FROM tbl_Cntrcts_SINs 
	Output 'tbl_Cntrcts_SINs', Deleted.ID, @UserLogin, GETDATE() into Audit_Deleted_Data_By_User
	WHERE CntrctNum = @ContractNumber 
	AND SINs = @SIN

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error encountered deleting SIN ' + @SIN  + ' from contract ' + @ContractNumber
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


