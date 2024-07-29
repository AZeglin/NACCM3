IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectUsersInRole')
	BEGIN
		DROP  Procedure  SelectUsersInRole
	END

GO

CREATE Procedure SelectUsersInRole
(
@RoleId int
)

AS

BEGIN

	select r.CO_ID, u.FullName
	from SEC_UserProfileUserRoles r, SEC_UserProfile u
	where r.CO_ID = u.CO_ID
	and r.RoleId = @RoleId
	order by u.FullName


END