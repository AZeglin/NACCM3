IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetLoginNameFromUserIdProc')
	BEGIN
		DROP  Procedure  GetLoginNameFromUserIdProc
	END

GO

CREATE Procedure GetLoginNameFromUserIdProc
(
@UserId uniqueidentifier,
@LoginName  nvarchar(120) OUTPUT
)

AS

BEGIN

	select @LoginName = NAC_CM.dbo.GetLoginNameFromUserIdFunction( @UserId )
	
END
