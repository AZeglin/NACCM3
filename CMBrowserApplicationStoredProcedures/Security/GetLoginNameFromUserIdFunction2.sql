IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetLoginNameFromUserIdFunction2')
	BEGIN
		DROP  Function  GetLoginNameFromUserIdFunction2
	END

GO

CREATE Function GetLoginNameFromUserIdFunction2
(
@UserId uniqueidentifier
)

RETURNS nvarchar(120)

AS

BEGIN

	Declare 
			@loginName  nvarchar(120)

	select @loginName = UserName 
	from SEC_UserProfile
	where UserId = @UserId

	return @loginName
	
END