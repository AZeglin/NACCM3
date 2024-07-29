IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'IsUserInBossRoleFunction')
	BEGIN
		DROP  Function  IsUserInBossRoleFunction
	END

GO

CREATE Function [dbo].[IsUserInBossRoleFunction]
(
@COID int
)

RETURNS bit

AS

BEGIN

	Declare 
			@IsInBossRole bit

	select @IsInBossRole = 0
	
	if exists( select UserProfileUserRoleId 
				from SEC_UserProfileUserRoles p join SEC_Roles r on p.RoleId = r.RoleId
				where p.CO_ID = @COID 
				and r.IsBossRole = 1 )
	BEGIN
		select @IsInBossRole = 1
	END

	return @IsInBossRole
	
END


