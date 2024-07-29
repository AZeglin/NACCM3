IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'IsUserInFSSChiefRoleFunction')
	BEGIN
		DROP  Function  IsUserInFSSChiefRoleFunction
	END

GO

CREATE Function [dbo].[IsUserInFSSChiefRoleFunction]
(
@COID int
)

RETURNS bit

AS

BEGIN

	Declare 
			@IsInChiefRole bit

	select @IsInChiefRole = 0
	
	if exists( select UserProfileUserRoleId 
				from SEC_UserProfileUserRoles p join SEC_Roles r on p.RoleId = r.RoleId
				where p.CO_ID = @COID 
				and r.IsBossRole = 1
				and r.ImmediateBossRoleId = 1 )
	BEGIN
		select @IsInChiefRole = 1
	END

	return @IsInChiefRole
	
END


