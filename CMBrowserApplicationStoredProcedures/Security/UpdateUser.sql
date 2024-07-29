IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateUser')
	BEGIN
		DROP  Procedure  UpdateUser
	END

GO

CREATE Procedure UpdateUser
(
@UserLogin nvarchar(120), -- of the user running the app
@COID int,
@FirstName nvarchar(40),
@LastName nvarchar(40),
@FullName nvarchar(80),
@UserName nvarchar(30),
@UserEmail nvarchar(50),
@UserPhone nvarchar(20),
@Inactive bit,
@Division int
)

AS

DECLARE
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200)

BEGIN


	update SEC_UserProfile
	set FirstName = @FirstName, 
		LastName = @LastName, 
		FullName = @FullName, 
		UserName = @UserName, 
		User_Email = @UserEmail, 
		User_Phone = @UserPhone, 
		Inactive = @Inactive, 
		Division = @Division,
		LastModifiedBy = @UserLogin,
		LastModificationDate = GETDATE()
	where CO_ID = @COID
		
	select  @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating user.'
		Raiserror( @errorMsg, 16, 1 )
	END
	
END
