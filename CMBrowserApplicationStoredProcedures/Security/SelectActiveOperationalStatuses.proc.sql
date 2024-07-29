IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectActiveOperationalStatuses' )
BEGIN
	DROP PROCEDURE SelectActiveOperationalStatuses
END
GO

CREATE PROCEDURE SelectActiveOperationalStatuses
(
@UserId uniqueidentifier
)
AS

BEGIN

	select OperationalStatusId,
		OperationalStatusDescription
	from SEC_OperationalStatuses
	where Inactive = 0
	order by OperationalStatusDescription

END

