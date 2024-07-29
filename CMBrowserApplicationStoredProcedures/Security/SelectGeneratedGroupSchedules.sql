IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectGeneratedGroupSchedules')
	BEGIN
		DROP  Procedure  SelectGeneratedGroupSchedules
	END

GO

CREATE Procedure SelectGeneratedGroupSchedules
(
@CurrentUser uniqueidentifier
)

AS

BEGIN

	select ScheduleNumber, ScheduleGroupIdList
	from SEC_GeneratedGroupSchedules
	order by ScheduleNumber

END