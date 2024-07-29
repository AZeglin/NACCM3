IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectUserScheduleAccessPoints')
	BEGIN
		DROP  Procedure  SelectUserScheduleAccessPoints
	END

GO

CREATE Procedure SelectUserScheduleAccessPoints
(
@UserId uniqueidentifier
)

AS

BEGIN

	select u.UserProfileUserRoleId, u.CO_ID, r.RoleId, r.RoleDescription, s.RoleScheduleGroupId, s.ScheduleGroupId, g.ScheduleNumberList, g.ScheduleGroupDescription,
		a.RoleAccessPointId, a.AccessPointId, p.AccessPointDescription
	from SEC_UserProfileUserRoles u
	join SEC_Roles r
		on u.RoleId = r.RoleId
	join SEC_RoleScheduleGroups s
		on r.RoleId = s.RoleId
	join SEC_RoleAccessPoints a
		on r.RoleId = a.RoleId
	join SEC_ScheduleGroups g
		on s.ScheduleGroupId = g.ScheduleGroupId
	join SEC_AccessPoints p
		on a.AccessPointId = p.AccessPointId
	where u.UserId = @UserId
	

END