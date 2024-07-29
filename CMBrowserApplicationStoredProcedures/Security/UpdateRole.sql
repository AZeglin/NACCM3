IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateRole')
	BEGIN
		DROP  Procedure  UpdateRole
	END

GO

CREATE Procedure UpdateRole
(
@UserLogin nvarchar(120),
@RoleId int,
@RoleDescription nvarchar(200)
)

AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250)
	
BEGIN

	update SEC_Roles
	set RoleDescription = @RoleDescription,
		LastModifiedBy = @UserLogin,
		LastModificationDate = GETDATE()
	where RoleId = @RoleId
	
	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating role'
		Raiserror( @errorMsg, 16, 1 )		
	END

END

