IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSecurityMatrixFromSchedCat')
	BEGIN
		DROP  Procedure  SelectSecurityMatrixFromSchedCat
	END

GO

CREATE Procedure SelectSecurityMatrixFromSchedCat
(
@CurrentUser uniqueidentifier
)
AS


BEGIN

	select Schedule_Number as ScheduleNumber,
	Schedule_Name as ScheduleName,
	Short_Sched_Name as ShortScheduleName,
	Division,
	~Inactive as Active,
	'M' as Role,   /* schedule manager */
	Schedule_Manager as OldUserId,
	0 as Ordinality
	from [tlkup_Sched/Cat]
	
	union
	
	select Schedule_Number as ScheduleNumber,
	Schedule_Name as ScheduleName,
	Short_Sched_Name as ShortScheduleName,
	Division,
	~Inactive as Active,
	'A' as Role,   /* assistant director */
	Asst_Director as OldUserId,
	0 as Ordinality
	from [tlkup_Sched/Cat]
	
	union
	
	select Schedule_Number as ScheduleNumber,
	Schedule_Name as ScheduleName,
	Short_Sched_Name as ShortScheduleName,
	Division,
	~Inactive as Active,
	'D' as Role,   /* director */
	Director as OldUserId,
	0 as Ordinality
	from [tlkup_Sched/Cat]
	
	union
	
	select Schedule_Number as ScheduleNumber,
	Schedule_Name as ScheduleName,
	Short_Sched_Name as ShortScheduleName,
	Division,
	~Inactive as Active,
	'F' as Role,   /* data entry full */
	Data_Entry_Full_1 as OldUserId	,
	0 as Ordinality
	from [tlkup_Sched/Cat]

	union 
	
	select Schedule_Number as ScheduleNumber,
	Schedule_Name as ScheduleName,
	Short_Sched_Name as ShortScheduleName,
	Division,
	~Inactive as Active,
	'F' as Role,   /* data entry full */
	Data_Entry_Full_2 as OldUserId	,
	1 as Ordinality
	from [tlkup_Sched/Cat]

	union 
	
	select Schedule_Number as ScheduleNumber,
	Schedule_Name as ScheduleName,
	Short_Sched_Name as ShortScheduleName,
	Division,
	~Inactive as Active,
	'B' as Role,   /* data entry SBA */
	Data_Entry_SBA_1 as OldUserId,
	0 as Ordinality	
	from [tlkup_Sched/Cat]
	
	union 
	
	select Schedule_Number as ScheduleNumber,
	Schedule_Name as ScheduleName,
	Short_Sched_Name as ShortScheduleName,
	Division,
	~Inactive as Active,
	'B' as Role,   /* data entry SBA */
	Data_Entry_SBA_2 as OldUserId,
	1 as Ordinality	
	from [tlkup_Sched/Cat]
	
	union 
	
	select Schedule_Number as ScheduleNumber,
	Schedule_Name as ScheduleName,
	Short_Sched_Name as ShortScheduleName,
	Division,
	~Inactive as Active,
	'S' as Role,   /* data entry sales */
	Data_Entry_Sales_1 as OldUserId,
	0 as Ordinality
	from [tlkup_Sched/Cat]
	
	union 
	
	select Schedule_Number as ScheduleNumber,
	Schedule_Name as ScheduleName,
	Short_Sched_Name as ShortScheduleName,
	Division,
	~Inactive as Active,
	'S' as Role,   /* data entry sales */
	Data_Entry_Sales_2 as OldUserId,
	1 as Ordinality
	from [tlkup_Sched/Cat]
	
END
