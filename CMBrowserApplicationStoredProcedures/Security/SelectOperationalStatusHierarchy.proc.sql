IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectOperationalStatusHierarchy' )
BEGIN
	DROP PROCEDURE SelectOperationalStatusHierarchy
END
GO

CREATE PROCEDURE SelectOperationalStatusHierarchy

AS

BEGIN

	select OperationalStatusHierarchyId, OperationalStatusId, AllowedOperationalStatusIdList
	from SEC_OperationalStatusHierarchy
	order by OperationalStatusId

END