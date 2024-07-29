IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetLoginNameFromUserIdLocalProc]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetLoginNameFromUserIdLocalProc]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [GetLoginNameFromUserIdLocalProc]
(
@UserId uniqueidentifier, 
@LoginName  nvarchar(120) OUTPUT
)

AS

BEGIN
	
	exec AMMHINSQL1.NAC_CM.dbo.GetLoginNameFromUserIdProc  @UserId = @UserId, @LoginName = @LoginName OUTPUT 
	
END
