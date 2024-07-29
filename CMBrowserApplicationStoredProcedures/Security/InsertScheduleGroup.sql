IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertScheduleGroup')
	BEGIN
		DROP  Procedure  InsertScheduleGroup
	END

GO

CREATE Procedure InsertScheduleGroup
(
@UserLogin nvarchar(120),
@ScheduleGroupDescription nvarchar(60),
@ScheduleGroupId int OUTPUT
)

AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250)
	
BEGIN TRANSACTION

	if exists ( select ScheduleGroupDescription from SEC_ScheduleGroups where ScheduleGroupDescription = @ScheduleGroupDescription )
	BEGIN
		select @errorMsg = 'An entry for that group already exists'
			goto ERROREXIT	
	END
	else
	BEGIN
	
		/* 9 is default ordinality; actual ordinality is set when schedule number list is added */
		insert into SEC_ScheduleGroups
		( ScheduleGroupDescription, ScheduleNumberList, Ordinality, LastModifiedBy, LastModificationDate )
		values
		( @ScheduleGroupDescription, '', 9, @UserLogin, GETDATE() )
		
		select @ScheduleGroupId = SCOPE_IDENTITY(), @error = @@error, @rowCount = @@rowcount
		
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error inserting new schedule group'
			goto ERROREXIT	
		END

		exec UpdateGeneratedGroupSchedules @UserLogin = @UserLogin

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error calling UpdateGeneratedGroupSchedules from within InsertScheduleGroup for group ' + @ScheduleGroupDescription
			goto ERROREXIT	
		END

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






