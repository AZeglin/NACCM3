IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'IsUserInRoleFunction')
	BEGIN
		DROP  Function  IsUserInRoleFunction
	END

GO

CREATE Function [dbo].[IsUserInRoleFunction]
(
@COID int,
@RoleId int
)

RETURNS bit

AS

BEGIN

	Declare 
			@IsInRole bit

	select @IsInRole = 0
	
	if exists( select UserProfileUserRoleId 
				from SEC_UserProfileUserRoles
				where CO_ID = @COID 
				and RoleId = @RoleId )
	BEGIN
		select @IsInRole = 1
	END

	return @IsInRole
	
END


