IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetUserInfo2')
	BEGIN
		DROP  Procedure  GetUserInfo2
	END

GO

CREATE Procedure GetUserInfo2
(
@LoginId nvarchar(120)
)
AS
DECLARE
	@errorMsg nvarchar(128),
    @status nvarchar(20),
    @error int,
    @rowcount int

BEGIN

/*	If exists (Select top 1 1 from sys.database_principals where name = @loginId) '{CACFACE5-0F06-4def-95E9-397361148239}'*/
/*	If exists (Select top 1 1 from sysusers where name = @loginId)
	Begin
		select UserId as 'UserGuid',
		FirstName, LastName, UserName as 'LoginId', User_Email as 'Email', User_Phone as 'Phone', 
		case Inactive when 1 then 'INACTIVE' else 'ACTIVE' end as 'status', CO_ID as 'oldUserId', UserTitle, Division, Admin
		from tlkup_UserProfile
		where UserName = @loginId	
	End	
	Else
	Begin  
		select UserId as 'UserGuid',
		FirstName, LastName, UserName as 'LoginId', User_Email as 'Email', User_Phone as 'Phone', 
		'DOESNOTEXIST' as 'status', CO_ID as 'oldUserId', UserTitle, Division, Admin
		from tlkup_UserProfile
		where UserName = @loginId	
	End */

	select UserId as 'UserGuid',
	FirstName, LastName, FullName, UserName as 'LoginId', User_Email as 'Email', User_Phone as 'Phone', 
	case Inactive when 1 then 'INACTIVE' else 'ACTIVE' end as 'status', CO_ID as 'oldUserId', Division
	from SEC_UserProfile
	where UserName = @LoginId	

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
			Select @errorMsg = 'Error encountered obtaining information for User : ' + @LoginId
			goto ERROREXIT
	END

	if @rowcount < 1
	BEGIN
			Select @errorMsg = 'User : ' + @LoginId + ' does not exist.'
			goto ERROREXIT
	END

	/* duplicate logins are allowed if only one is active */
	if @rowcount > 1
	BEGIN
	
		select UserId as 'UserGuid',
		FirstName, LastName, FullName, UserName as 'LoginId', User_Email as 'Email', User_Phone as 'Phone', 
		case Inactive when 1 then 'INACTIVE' else 'ACTIVE' end as 'status', CO_ID as 'oldUserId', Division
		from SEC_UserProfile
		where UserName = @LoginId	
		and Inactive = 0

		select @error = @@error, @rowcount = @@rowcount

		if @error <> 0 or @rowcount <> 1
		BEGIN
				Select @errorMsg = 'Error encountered obtaining information for User (2) : ' + @LoginId
				goto ERROREXIT
		END

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

END
