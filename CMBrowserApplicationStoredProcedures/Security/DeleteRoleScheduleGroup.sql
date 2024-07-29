IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteRoleScheduleGroup')
	BEGIN
		DROP  Procedure  DeleteRoleScheduleGroup
	END

GO

CREATE Procedure DeleteRoleScheduleGroup
(
@UserLogin nvarchar(120),
@RoleScheduleGroupId int
)

AS

DECLARE  @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
BEGIN

	delete SEC_RoleScheduleGroups
	where RoleScheduleGroupId = @RoleScheduleGroupId

	select @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting schedule group from role'
		Raiserror( @errorMsg, 16, 1 )
	END
END
