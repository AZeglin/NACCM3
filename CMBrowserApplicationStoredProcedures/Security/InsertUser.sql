IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertUser')
	BEGIN
		DROP  Procedure  InsertUser
	END

GO

CREATE Procedure InsertUser
(
@UserLogin nvarchar(120),  -- of user running the security app
@FirstName nvarchar(40),
@LastName nvarchar(40),
@FullName nvarchar(80),
@UserName nvarchar(30),
@UserEmail nvarchar(50),
@UserPhone nvarchar(20),
@Division int,
@COID int OUTPUT,
@UserId uniqueidentifier OUTPUT
)

AS

DECLARE
		@identity int,
		@newUserId uniqueidentifier,
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200)

BEGIN TRANSACTION

	select @newUserId = NEWID()
	
	select @COID = max( CO_ID ) + 1 from SEC_UserProfile
	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error selecting max(CO_ID)'
		goto ERROREXIT
	END
	
	insert into SEC_UserProfile
	( FirstName, LastName, FullName, UserName, User_Email, User_Phone, Inactive, UserId, Division, LastModifiedBy, LastModificationDate )
	values
	( @FirstName, @LastName, @FullName, @UserName, @UserEmail, @UserPhone, 0, @newUserId, @Division, @UserLogin, GETDATE() )
	
	select @error = @@error, @rowCount = @@rowcount, @identity = SCOPE_IDENTITY() 

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error inserting new user.'
		goto ERROREXIT
	END
	
	select @UserId = @newUserId, @COID = @identity

	goto OKEXIT
	
ERROREXIT:
	
	raiserror( @errorMsg, 16, 1 )
	
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	else if @@TRANCOUNT = 1
	BEGIN
		ROLLBACK TRANSACTION
	END
OKEXIT:

	if @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	


