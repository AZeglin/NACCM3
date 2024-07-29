IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSchedulesForDivision')
	BEGIN
		DROP  Procedure  SelectSchedulesForDivision
	END

GO

CREATE Procedure SelectSchedulesForDivision
(
@CurrentUser uniqueidentifier,
@COID int,
@LoginId nvarchar(120),
@DivisionId int
)

AS

DECLARE @error int,
		@rowcount int,
		@errorMsg nvarchar(200)
		
BEGIN

	select Schedule_Number, Schedule_Name, Short_Sched_Name, Schedule_Manager, Asst_Director, Section, Section_Description, Director
	from [tlkup_Sched/Cat]
	where Inactive = 0
	and Division = @DivisionId

	union
	
	select -1 as Schedule_Number, '--select--' as Schedule_Name, '--select--' as Short_Sched_Name, -1 as Schedule_Manager, -1 as Asst_Director, -1 as Section, '' as Section_Description, -1 as Director

	order by Schedule_Number
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount < 1
	BEGIN
		select @errorMsg = 'Error retrieving list of schedules from schedcat, for DivisionId=' + convert( nvarchar(10), @DivisionId )
		raiserror( @errorMsg, 16, 1 )
	END

END
