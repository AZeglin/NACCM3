IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteRole')
	BEGIN
		DROP  Procedure  DeleteRole
	END

GO

CREATE Procedure DeleteRole
(
@UserLogin nvarchar(120),
@RoleId int
)

AS

DECLARE  @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
BEGIN

	delete SEC_Roles
	where RoleId = @RoleId

	select @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting role'
		Raiserror( @errorMsg, 16, 1 )
	END
END