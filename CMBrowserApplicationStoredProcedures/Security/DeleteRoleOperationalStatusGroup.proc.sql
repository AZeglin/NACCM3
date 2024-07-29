IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteRoleOperationalStatusGroup' )
BEGIN
	DROP PROCEDURE DeleteRoleOperationalStatusGroup
END
GO

CREATE PROCEDURE DeleteRoleOperationalStatusGroup
(
@UserLogin nvarchar(120),
@RoleOperationalStatusGroupId int
)

AS

DECLARE  @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
BEGIN

	delete SEC_RoleOperationalStatusGroups
	where RoleOperationalStatusGroupId = @RoleOperationalStatusGroupId

	select @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting operational status group from role'
		Raiserror( @errorMsg, 16, 1 )
	END
END
