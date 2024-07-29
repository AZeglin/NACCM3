IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertSINForContract' )
BEGIN
	DROP PROCEDURE InsertSINForContract
END
GO

CREATE PROCEDURE InsertSINForContract
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(50),
@SIN nvarchar(10),
@Recoverable bit,
@ID int output
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

	if exists ( select SINs from tbl_Cntrcts_SINs where SINs = @SIN and CntrctNum = @ContractNumber and Inactive = 1 )
	BEGIN

		update s 
			set Inactive = 0,
			Recoverable = @Recoverable,
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = GETDATE()
		from tbl_Cntrcts_SINs s
		where s.SINs = @SIN
		and s.CntrctNum = @ContractNumber
		and s.Inactive = 1

		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @ID = SCOPE_IDENTITY()
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error restoring SIN into contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else
	BEGIN

		insert into tbl_Cntrcts_SINs
		( CntrctNum, SINs, Recoverable, LexicalSIN, Inactive, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		select @ContractNumber, 
			@SIN, 
			@Recoverable, 
			s.LexicalSIN, 
			0,
			@currentUserLogin, 
			GETDATE(),
			@currentUserLogin, 
			GETDATE() 
		from tbl_SINs s, tbl_Cntrcts c 
		where s.SIN = @SIN
		and c.CntrctNum = @ContractNumber
		and s.[Schedule_ Number] = c.Schedule_Number
	
		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @ID = SCOPE_IDENTITY()
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting new SIN into contract ' + @ContractNumber
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


