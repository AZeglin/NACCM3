IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectRoleOperationalStatusGroups' )
BEGIN
	DROP PROCEDURE SelectRoleOperationalStatusGroups
END
GO

CREATE PROCEDURE SelectRoleOperationalStatusGroups
(
@RoleId int
)

AS

BEGIN

	select r.RoleOperationalStatusGroupId, r.RoleId, o.OperationalStatusGroupId, o.OperationalStatusGroupDescription, o.OperationalStatusIdList
	from SEC_RoleOperationalStatusGroups r join SEC_OperationalStatusGroups o
		on r.OperationalStatusGroupId = o.OperationalStatusGroupId
	where r.RoleId = @RoleId
	order by o.OperationalStatusGroupDescription

END
