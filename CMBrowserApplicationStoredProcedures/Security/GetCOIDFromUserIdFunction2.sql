IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetCOIDFromUserIdFunction2')
	BEGIN
		DROP  Function  GetCOIDFromUserIdFunction2
	END

GO

CREATE Function GetCOIDFromUserIdFunction2
(
@UserId uniqueidentifier
)

RETURNS int

AS

BEGIN

	Declare 
			@COID int

	select @COID = CO_ID 
	from SEC_UserProfile
	where UserId = @UserId

	return @COID
	
END
