IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteUser')
	BEGIN
		DROP  Procedure  DeleteUser
	END

GO

CREATE Procedure DeleteUser
(
@UserLogin nvarchar(120),  -- of user running the app
@UserId uniqueidentifier
)

AS

DECLARE
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200)

BEGIN


	delete SEC_UserProfile
	where UserId = @UserId
		
	select  @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting user.'
		Raiserror( @errorMsg, 16, 1 )
	END
	
END
