IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DefineRoleOperationalStatusGroups' )
BEGIN
	DROP PROCEDURE DefineRoleOperationalStatusGroups
END
GO

CREATE PROCEDURE DefineRoleOperationalStatusGroups
(
@UserLogin nvarchar(120),
@RoleId int,
@RoleOperationalStatusGroupList nvarchar(240)   -- comma delimited list of OperationalStatusGroupId's 
)

AS

DECLARE
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(600)

BEGIN TRANSACTION

	delete SEC_RoleOperationalStatusGroups
	where RoleId = @RoleId

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting existing operational status groups from role'
		goto ERROREXIT
	END

	/* no schedule groups to add */
	if LEN( @RoleOperationalStatusGroupList ) = 0
	BEGIN
		goto OKEXIT
	END
	
	select @query = 'insert into SEC_RoleOperationalStatusGroups
	( RoleId, OperationalStatusGroupId, LastModifiedBy, LastModificationDate )
	select @p_RoleId, OperationalStatusGroupId, @p_UserLogin, getdate()
	from SEC_OperationalStatusGroups
	where OperationalStatusGroupId in ( ' + @RoleOperationalStatusGroupList + ' )'
	
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
		select @errorMsg = 'Error adding operational status groups to role'
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






	
