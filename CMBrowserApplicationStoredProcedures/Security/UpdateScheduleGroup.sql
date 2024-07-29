IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateScheduleGroup')
	BEGIN
		DROP  Procedure  UpdateScheduleGroup
	END

GO

CREATE Procedure UpdateScheduleGroup
(
@UserLogin nvarchar(120),
@ScheduleGroupId int,
@ScheduleGroupDescription nvarchar(60)
)

AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250)
	
BEGIN TRANSACTION

	update SEC_ScheduleGroups
	set ScheduleGroupDescription = @ScheduleGroupDescription,
		LastModifiedBy = @UserLogin,
		LastModificationDate = GETDATE()
	where ScheduleGroupId = @ScheduleGroupId
		
	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating schedule group'
		goto ERROREXIT	
	END
	
	exec UpdateGeneratedGroupSchedules @UserLogin = @UserLogin

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error calling UpdateGeneratedGroupSchedules from within UpdateScheduleGroup for group ' + @ScheduleGroupDescription
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



