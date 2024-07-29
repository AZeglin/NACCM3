IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteUserRole')
	BEGIN
		DROP  Procedure  DeleteUserRole
	END

GO

CREATE Procedure DeleteUserRole
(
@UserLogin nvarchar(120),
@UserProfileUserRoleId int
)

AS

DECLARE  @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
BEGIN

	delete SEC_UserProfileUserRoles
	where UserProfileUserRoleId = @UserProfileUserRoleId

	select @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting role from user'
		Raiserror( @errorMsg, 16, 1 )
	END
END