IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetLoginNameFromUserIdLocalFunction]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [GetLoginNameFromUserIdLocalFunction]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Function [GetLoginNameFromUserIdLocalFunction]
(
@UserId uniqueidentifier
)

RETURNS nvarchar(120)

AS

BEGIN

	Declare @LoginName  nvarchar(120)
		
	exec AMMHINSQL1.NAC_CM.dbo.GetLoginNameFromUserIdProc  @UserId = @UserId, @LoginName = @LoginName OUTPUT 

	return @LoginName
	
END
