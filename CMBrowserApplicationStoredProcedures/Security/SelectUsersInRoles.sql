IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectUsersInRoles')
	BEGIN
		DROP  Procedure  SelectUsersInRoles
	END

GO

CREATE Procedure SelectUsersInRoles
(
@RoleIdList nvarchar(240)
)

AS

DECLARE
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(600)

BEGIN TRANSACTION

	/* no roles */
	if LEN( @RoleIdList ) = 0
	BEGIN
		goto OKEXIT
	END
	
	select @query = 'select r.CO_ID, u.FullName
	from SEC_UserProfileUserRoles r, SEC_UserProfile u
	where r.CO_ID = u.CO_ID
	and r.RoleId in ( ' + @RoleIdList + ' )	order by u.FullName'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string'
		goto ERROREXIT
	END
	
	exec SP_EXECUTESQL @query

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting users in role'
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






	





