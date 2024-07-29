IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DefineRoleAccessPoints')
	BEGIN
		DROP  Procedure  DefineRoleAccessPoints
	END

GO

CREATE Procedure DefineRoleAccessPoints
(
@UserLogin nvarchar(120),
@RoleId int,
@RoleAccessPointList nvarchar(240)   -- comma delimited list of AccessPointId's
)

AS

DECLARE
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(600)

BEGIN TRANSACTION

	delete SEC_RoleAccessPoints
	where RoleId = @RoleId

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting existing access points from role'
		goto ERROREXIT
	END

	/* no access points to add */
	if LEN( @RoleAccessPointList ) = 0
	BEGIN
		goto OKEXIT
	END

	select @query = 'insert into SEC_RoleAccessPoints
	( RoleId, AccessPointId, LastModifiedBy, LastModificationDate )
	select @p_RoleId, AccessPointId, @p_UserLogin, getdate()
	from SEC_AccessPoints
	where AccessPointId in ( ' + @RoleAccessPointList + ' )'
	
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
		select @errorMsg = 'Error adding access points to role'
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






	




