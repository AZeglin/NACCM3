 IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateGeneratedGroupSchedules')
	BEGIN
		DROP  Procedure  UpdateGeneratedGroupSchedules
	END

GO

CREATE Procedure UpdateGeneratedGroupSchedules
(
@UserLogin nvarchar(120)
)
AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250),
	@scheduleGroupId int,
	@scheduleList nvarchar(240),
	@scheduleGroupIdList nvarchar(240)
	
BEGIN TRANSACTION
	
	delete SEC_GeneratedGroupSchedules

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error clearing Generated Group Schedules table'
		goto ERROREXIT
	END
	
	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#ScheduleList' ) 
	BEGIN
		DROP TABLE #ScheduleList
	END
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error dropping #ScheduleList temp table'
		goto ERROREXIT
	END
	
	create table #ScheduleList
	(
		ScheduleNumber int NOT NULL
	)
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #ScheduleList temp table'
		goto ERROREXIT
	END
	
	Declare GenerateGroupSchedulesCursor CURSOR For
	Select ScheduleGroupId, ScheduleNumberList
	from SEC_ScheduleGroups

	Open GenerateGroupSchedulesCursor
	
	FETCH NEXT FROM GenerateGroupSchedulesCursor
	INTO @scheduleGroupId, @scheduleList

	WHILE @@FETCH_STATUS = 0
	BEGIN

		delete #ScheduleList

		insert into #ScheduleList
		select Value from dbo.SplitFunction( @scheduleList, ',' )
		
		select @error = @@error, @rowCount = @@rowcount
		
		if @error <> 0 or @rowCount = 0
		BEGIN
			select @errorMsg = 'Error splitting list for group id= ' + convert( nvarchar(10), @scheduleGroupId )
			goto ERROREXIT
		END

		select @rowCount = count(*) from #ScheduleList
		
		while @rowCount > 0
		BEGIN

			if exists ( select * from SEC_GeneratedGroupSchedules 
						where ScheduleNumber = ( select top 1 ScheduleNumber from #ScheduleList ))
			BEGIN
				select @scheduleGroupIdList = ScheduleGroupIdList 
				from SEC_GeneratedGroupSchedules
				where ScheduleNumber = ( select top 1 ScheduleNumber from #ScheduleList )
				
				select @scheduleGroupIdList = @scheduleGroupIdList + ',' + convert( nvarchar(10), @scheduleGroupId )
				
				update SEC_GeneratedGroupSchedules
				set ScheduleGroupIdList = @scheduleGroupIdList,
				LastModifiedBy = @UserLogin,
				LastModificationDate = GETDATE()
				where ScheduleNumber = ( select top 1 ScheduleNumber from #ScheduleList )
			END
			else
			BEGIN
				insert into SEC_GeneratedGroupSchedules
				( ScheduleNumber, ScheduleGroupIdList, LastModifiedBy, LastModificationDate )
				select top 1 ScheduleNumber, convert( nvarchar(4), @scheduleGroupId ), @UserLogin, GETDATE() from #ScheduleList
			
			END

			delete #ScheduleList
			where ScheduleNumber = ( select top 1 ScheduleNumber from #ScheduleList )

			select @rowCount = count(*) from #ScheduleList

		END
		
		FETCH NEXT FROM GenerateGroupSchedulesCursor
		INTO @scheduleGroupId, @scheduleList
	
	END
	
	Close GenerateGroupSchedulesCursor
	DeAllocate GenerateGroupSchedulesCursor

GOTO OKEXIT

ERROREXIT:

	Close GenerateGroupSchedulesCursor
	DeAllocate GenerateGroupSchedulesCursor

	raiserror( @errorMsg, 16 , 1 )
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
		/* only rollback iff this the highest level */
		ROLLBACK TRANSACTION
	END
	
	RETURN ( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN ( 0 )




