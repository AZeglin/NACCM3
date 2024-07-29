IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetOperationalStatusesForDocumentOwner' )
BEGIN
	DROP PROCEDURE GetOperationalStatusesForDocumentOwner
END
GO

CREATE PROCEDURE GetOperationalStatusesForDocumentOwner
(
@COID int
)

AS

BEGIN

	select g.OperationalStatusGroupId, g.OperationalStatusIdList from SEC_OperationalStatusGroups g join SEC_RoleOperationalStatusGroups s on g.OperationalStatusGroupId = s.OperationalStatusGroupId
	join SEC_UserProfileUserRoles r on s.RoleId = r.RoleId
	join SEC_UserProfile p on r.CO_ID = p.CO_ID
	where p.CO_ID = @COID

END


