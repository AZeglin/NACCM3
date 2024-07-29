IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectRoleAccessPoints')
	BEGIN
		DROP  Procedure  SelectRoleAccessPoints
	END

GO

CREATE Procedure SelectRoleAccessPoints
(
@RoleId int
)

AS

BEGIN

	select r.RoleAccessPointId, r.RoleId, a.AccessPointId, a.AccessPointDescription
	from SEC_RoleAccessPoints r join SEC_AccessPoints a
		on r.AccessPointId = a.AccessPointId
	where r.RoleId = @RoleId
	order by a.AccessPointDescription

END

