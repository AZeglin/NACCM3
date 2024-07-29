IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertContractCheck' )
BEGIN
	DROP PROCEDURE InsertContractCheck
END
GO

CREATE PROCEDURE InsertContractCheck
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@QuarterId int,
@CheckAmount money,
@CheckNumber nvarchar(50),
@DepositNumber nvarchar(50),
@DateReceived datetime,
@CheckComments nvarchar(255),
@CheckId int OUTPUT
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

	insert into tbl_Cntrcts_Checks
	( CntrctNum, Quarter_ID, CheckAmt, CheckNum, DepositNum, DateRcvd, Comments, LastModifiedBy, LastModificationDate )
	values
	( @ContractNumber, @QuarterId, @CheckAmount, @CheckNumber, @DepositNumber, @DateReceived, @CheckComments, @currentUserLogin, GETDATE() )


	select @error = @@ERROR, @rowCount = @@ROWCOUNT, @CheckId = SCOPE_IDENTITY()
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error inserting check for contract.'
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


