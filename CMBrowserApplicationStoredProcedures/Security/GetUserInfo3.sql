IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetUserInfo3')
	BEGIN
		DROP  Procedure  GetUserInfo3
	END

GO

-- version used in export upload application
CREATE Procedure GetUserInfo3
(
@UserId uniqueidentifier
)
AS
DECLARE
	@errorMsg nvarchar(128),
    @status nvarchar(20),
    @error int,
    @rowcount int

BEGIN

	select UserName as 'LoginId',
	FirstName, LastName, FullName, UserId, User_Email as 'Email', User_Phone as 'Phone', 
	case Inactive when 1 then 'INACTIVE' else 'ACTIVE' end as 'status', CO_ID as 'oldUserId', Division
	from SEC_UserProfile
	where UserId = @UserId	

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
			Select @errorMsg = 'Error encountered obtaining information for User : ' + convert( nvarchar(150), @UserId )
			goto ERROREXIT
	END

	if @rowcount < 1
	BEGIN
			Select @errorMsg = 'User with id: ' + convert( nvarchar(150), @UserId ) + ' does not exist.'
			goto ERROREXIT
	END

	/* duplicate logins are allowed if only one is active */
	if @rowcount > 1
	BEGIN
	
		select UserName as 'LoginId',
		FirstName, LastName, FullName, UserId, User_Email as 'Email', User_Phone as 'Phone', 
		case Inactive when 1 then 'INACTIVE' else 'ACTIVE' end as 'status', CO_ID as 'oldUserId', Division
		from SEC_UserProfile
		where UserId = @UserId	
		and Inactive = 0

		select @error = @@error, @rowcount = @@rowcount

		if @error <> 0 or @rowcount <> 1
		BEGIN
				Select @errorMsg = 'Error encountered obtaining information for User (2) : ' + convert( nvarchar(150), @UserId )
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
