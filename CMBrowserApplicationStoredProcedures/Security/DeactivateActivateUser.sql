IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeactivateActivateUser')
	BEGIN
		DROP  Procedure  DeactivateActivateUser
	END

GO

CREATE Procedure DeactivateActivateUser
(
@UserId uniqueidentifier,
@Active bit
)

AS

DECLARE
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200)
		
BEGIN

	update SEC_UserProfile
	set Inactive = @Active
	where UserId = @UserId

	select  @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deactivating/activating user.'
		Raiserror( @errorMsg, 16, 1 )
	END
	
END
