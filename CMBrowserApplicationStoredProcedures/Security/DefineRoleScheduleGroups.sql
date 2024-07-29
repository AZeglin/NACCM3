IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DefineRoleScheduleGroups')
	BEGIN
		DROP  Procedure  DefineRoleScheduleGroups
	END

GO

CREATE Procedure DefineRoleScheduleGroups
(
@UserLogin nvarchar(120),
@RoleId int,
@RoleScheduleGroupList nvarchar(240)   -- comma delimited list of ScheduleGroupId's 
)

AS

DECLARE
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(600)

BEGIN TRANSACTION

	delete SEC_RoleScheduleGroups
	where RoleId = @RoleId

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting existing schedule groups from role'
		goto ERROREXIT
	END

	/* no schedule groups to add */
	if LEN( @RoleScheduleGroupList ) = 0
	BEGIN
		goto OKEXIT
	END
	
	select @query = 'insert into SEC_RoleScheduleGroups
	( RoleId, ScheduleGroupId, LastModifiedBy, LastModificationDate )
	select @p_RoleId, ScheduleGroupId, @p_UserLogin, getdate()
	from SEC_ScheduleGroups
	where ScheduleGroupId in ( ' + @RoleScheduleGroupList + ' )'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string'
		goto ERROREXIT
	END
	
	exec SP_EXECUTESQL @query, N'@p_RoleId int, @p_UserLogin nvarchar(120)', @p_RoleId = @RoleId, @p_UserLogin = @UserLogin

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding schedules to role'
		goto ERROREXIT
	END

goto OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 )
	
  	if @@TRANCOUNT > 1
  	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 

ENDEXIT:






	





