IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectNonPharmaceuticalSchedulesForReport')
	BEGIN
		DROP  Procedure  SelectNonPharmaceuticalSchedulesForReport
	END

GO

CREATE Procedure SelectNonPharmaceuticalSchedulesForReport
(
@DivisionId as int = -1
)
AS

	Declare @AllScheduleName nvarchar(75),
		@AllScheduleValue int
BEGIN

	select @AllScheduleName = 'All'
	select @AllScheduleValue = -1

	if @DivisionId = -1
	BEGIN
		select @AllScheduleValue as Schedule_Number, @AllScheduleName as Schedule_Name
		union
		select Schedule_Number, Schedule_Name
		from [tlkup_Sched/Cat]
		where Schedule_Number not in ( 1, 18, 28, 29, 30, 31, 32, 37, 39, 43, 47, 48 ) 
		order by Schedule_Name
	END
	else
	BEGIN
		select @AllScheduleValue as Schedule_Number, @AllScheduleName as Schedule_Name
		union
		select Schedule_Number, Schedule_Name
		from [tlkup_Sched/Cat]
		where Division = @DivisionId
		and Schedule_Number not in ( 1, 18, 28, 29, 30, 31, 32, 37, 39, 43, 47, 48 ) 
		order by Schedule_Name
	END

END