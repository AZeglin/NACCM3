IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectActiveSchedules')
	BEGIN
		DROP  Procedure  SelectActiveSchedules
	END

GO

CREATE Procedure SelectActiveSchedules
AS

BEGIN

	select Schedule_Number as ScheduleNumber,
		Schedule_Name as ScheduleName,
		Short_Sched_Name as ShortScheduleName,
	Division
	from NAC_CM.dbo.[tlkup_Sched/Cat] 
	where Inactive = 0
	order by ScheduleName
	
END
