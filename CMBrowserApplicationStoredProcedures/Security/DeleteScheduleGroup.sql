IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteScheduleGroup')
	BEGIN
		DROP  Procedure  DeleteScheduleGroup
	END

GO

CREATE Procedure DeleteScheduleGroup
(
@UserLogin nvarchar(120),
@ScheduleGroupId int
)

AS

DECLARE  @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
		
BEGIN TRANSACTION

	delete SEC_ScheduleGroups
	where ScheduleGroupId = @ScheduleGroupId

	select @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting schedule group'
		goto ERROREXIT	
	END
	
	exec UpdateGeneratedGroupSchedules @UserLogin = @UserLogin

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error calling UpdateGeneratedGroupSchedules from within DeleteScheduleGroup for groupId ' + convert( nvarchar(10), @ScheduleGroupId )
		goto ERROREXIT	
	END
	
GOTO OKEXIT

ERROREXIT:

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








