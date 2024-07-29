IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteRoleAccessPoint')
	BEGIN
		DROP  Procedure  DeleteRoleAccessPoint
	END

GO

CREATE Procedure DeleteRoleAccessPoint
(
@UserLogin nvarchar(120),
@RoleAccessPointId int
)

AS

DECLARE  @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
BEGIN

	delete SEC_RoleAccessPoints
	where RoleAccessPointId = @RoleAccessPointId

	select @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting access point from role'
		Raiserror( @errorMsg, 16, 1 )
	END
END
