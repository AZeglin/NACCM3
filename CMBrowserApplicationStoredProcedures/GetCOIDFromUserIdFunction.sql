IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetCOIDFromUserIdFunction')
	BEGIN
		DROP  Function  GetCOIDFromUserIdFunction
	END

GO

CREATE Function GetCOIDFromUserIdFunction
(
@UserId uniqueidentifier
)

RETURNS int

AS

BEGIN

	Declare 
			@COID int

	select @COID = CO_ID 
	from tlkup_UserProfile
	where UserId = @UserId

	return @COID
	
END
