IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteSINFromContract2' )
BEGIN
	DROP PROCEDURE DeleteSINFromContract2
END
GO

CREATE PROCEDURE DeleteSINFromContract2
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(50),
@SIN varchar(10)
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
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	--DELETE FROM tbl_Cntrcts_SINs 
	--Output 'tbl_Cntrcts_SINs', Deleted.ID, @currentUserLogin, GETDATE() into Audit_Deleted_Data_By_User
	--WHERE CntrctNum = @ContractNumber 
	--AND SINs = @SIN

	update tbl_Cntrcts_SINs
	set Inactive = 1,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where CntrctNum = @ContractNumber
		and SINs = @SIN

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


