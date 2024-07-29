IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertRole')
	BEGIN
		DROP  Procedure  InsertRole
	END

GO

CREATE Procedure InsertRole
(
@UserLogin nvarchar(120),
@RoleDescription nvarchar(200),
@RoleId int OUTPUT
)

AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250)
	
BEGIN

	if exists ( select RoleId from SEC_Roles where RoleDescription = @RoleDescription )
	BEGIN
		select @errorMsg = 'A role with that description already exists'
		Raiserror( @errorMsg, 16, 1 )
	END
	else
	BEGIN
		insert into SEC_Roles
		( RoleDescription, LastModifiedBy, LastModificationDate )
		values
		( @RoleDescription, @UserLogin, GETDATE() )
		
		select @RoleId = SCOPE_IDENTITY(), @error = @@error, @rowCount = @@rowcount
		
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error inserting new role'
			Raiserror( @errorMsg, 16, 1 )		
		END

	END
END
