IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectRoleScheduleGroups')
	BEGIN
		DROP  Procedure  SelectRoleScheduleGroups
	END

GO

CREATE Procedure SelectRoleScheduleGroups
(
@RoleId int
)

AS

BEGIN

	select r.RoleScheduleGroupId, r.RoleId, s.ScheduleGroupId, s.ScheduleGroupDescription, s.ScheduleNumberList
	from SEC_RoleScheduleGroups r join SEC_ScheduleGroups s
		on r.ScheduleGroupId = s.ScheduleGroupId
	where r.RoleId = @RoleId
	order by s.ScheduleGroupDescription

END
