IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectGeneratedGroupOperationalStatuses' )
BEGIN
	DROP PROCEDURE SelectGeneratedGroupOperationalStatuses
END
GO

CREATE PROCEDURE SelectGeneratedGroupOperationalStatuses
(
@CurrentUser uniqueidentifier
)

AS

BEGIN

	select OperationalStatusId, OperationalStatusGroupIdList
	from SEC_GeneratedGroupOperationalStatuses
	order by OperationalStatusId

END




