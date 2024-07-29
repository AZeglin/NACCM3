IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetLoginNameFromUserIdFunction')
	BEGIN
		DROP  Function  GetLoginNameFromUserIdFunction
	END

GO

CREATE Function GetLoginNameFromUserIdFunction
(
@UserId uniqueidentifier
)

RETURNS nvarchar(120)

AS

BEGIN

	Declare 
			@loginName  nvarchar(120)

	select @loginName = UserName 
	from tlkup_UserProfile
	where UserId = @UserId

	return @loginName
	
END