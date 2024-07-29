IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DefineUserRoles')
	BEGIN
		DROP  Procedure  DefineUserRoles
	END

GO

CREATE Procedure DefineUserRoles
(
@UserLogin nvarchar(120),
@COID int,
@UserId uniqueidentifier,
@RoleIdList nvarchar(240)  -- comma delimited list of RoleId's
)

AS

DECLARE
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(600)

BEGIN TRANSACTION

	delete SEC_UserProfileUserRoles
	where CO_ID = @COID

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting existing roles from user'
		goto ERROREXIT
	END
	
	/* no roles to add */
	if LEN( @RoleIdList ) = 0
	BEGIN
		goto OKEXIT
	END
	
	select @query = 'insert into SEC_UserProfileUserRoles
	( CO_ID, UserId, RoleId, LastModifiedBy, LastModificationDate )
	select @p_COID, @p_UserId, RoleId, @p_UserLogin, getdate()
	from SEC_Roles
	where RoleId in ( ' + @RoleIdList + ' )'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string'
		goto ERROREXIT
	END
	
	/*   */
	exec SP_EXECUTESQL @query, N'@p_COID int, @p_UserId uniqueidentifier, @p_UserLogin nvarchar(120)', @p_COID = @COID, @p_UserId = @UserId, @p_UserLogin = @UserLogin
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error adding roles to user'
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






	





