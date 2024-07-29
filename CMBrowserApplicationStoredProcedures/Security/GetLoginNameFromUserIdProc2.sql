IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetLoginNameFromUserIdProc2')
	BEGIN
		DROP  Procedure  GetLoginNameFromUserIdProc2
	END

GO

CREATE Procedure GetLoginNameFromUserIdProc2
(
@UserId uniqueidentifier,
@LoginName  nvarchar(120) OUTPUT
)

AS

BEGIN

	select @LoginName = [NAC_CM].[dbo].GetLoginNameFromUserIdFunction2( @UserId )
	
END
