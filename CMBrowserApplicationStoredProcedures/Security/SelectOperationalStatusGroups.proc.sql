IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectOperationalStatusGroups' )
BEGIN
	DROP PROCEDURE SelectOperationalStatusGroups
END
GO

CREATE PROCEDURE SelectOperationalStatusGroups

AS


BEGIN

	select OperationalStatusGroupId, OperationalStatusIdList, OperationalStatusGroupDescription
	from SEC_OperationalStatusGroups
	order by Ordinality, OperationalStatusGroupDescription

END
