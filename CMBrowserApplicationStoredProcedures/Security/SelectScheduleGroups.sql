IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectScheduleGroups')
	BEGIN
		DROP  Procedure  SelectScheduleGroups
	END

GO

CREATE Procedure SelectScheduleGroups

AS

BEGIN

	select ScheduleGroupId, ScheduleNumberList, ScheduleGroupDescription
	from SEC_ScheduleGroups
	order by Ordinality, ScheduleGroupDescription

END
