IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSchedulesForReport')
	BEGIN
		DROP  Procedure  SelectSchedulesForReport
	END

GO

CREATE Procedure SelectSchedulesForReport
(
@DivisionId as int = -1,
@Type nvarchar(50) = null
)
AS

	Declare @AllScheduleName nvarchar(75),
		@AllScheduleValue int
		
BEGIN

	if @Type = 'BPA'
	BEGIN
		select @AllScheduleName = 'All BPA Schedules'
	END
	else
	BEGIN
		select @AllScheduleName = 'All Schedules'
	END

	select @AllScheduleValue = -1

	if @DivisionId = -1
	BEGIN
		if @Type is null
		BEGIN
			select @AllScheduleValue as Schedule_Number, @AllScheduleName as Schedule_Name, '1' as Ordinality
			union
			select Schedule_Number, Schedule_Name, Schedule_Name as Ordinality
			from [tlkup_Sched/Cat]
			order by Ordinality
		END
		else
		BEGIN
			select @AllScheduleValue as Schedule_Number, @AllScheduleName as Schedule_Name, '1' as Ordinality
			union
			select Schedule_Number, Schedule_Name, Schedule_Name as Ordinality
			from [tlkup_Sched/Cat]
			where Type = @Type
			order by Ordinality		
		END
	END
	else
	BEGIN
		if @Type is null
		BEGIN
			select @AllScheduleValue as Schedule_Number, @AllScheduleName as Schedule_Name, '1' as Ordinality
			union
			select Schedule_Number, Schedule_Name, Schedule_Name as Ordinality
			from [tlkup_Sched/Cat]
			where Division = @DivisionId
			order by Ordinality
		END
		else
		BEGIN
	select @AllScheduleValue as Schedule_Number, @AllScheduleName as Schedule_Name, '1' as Ordinality
			union
			select Schedule_Number, Schedule_Name, Schedule_Name as Ordinality
			from [tlkup_Sched/Cat]
			where Division = @DivisionId
			and Type = @Type
			order by Ordinality
		END
	END

END