IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectUserRoles')
	BEGIN
		DROP  Procedure  SelectUserRoles
	END

GO

CREATE Procedure SelectUserRoles
(
@COID int
)

AS

BEGIN

	select u.UserProfileUserRoleId, u.CO_ID, r.RoleId, r.RoleDescription
	from SEC_UserProfileUserRoles u
	join SEC_Roles r
		on u.RoleId = r.RoleId
	where u.CO_ID = @COID
	order by RoleDescription
	

END
