IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetLastModifiedReportDate')
	BEGIN
		DROP  Function  GetLastModifiedReportDate
	END

GO

CREATE FUNCTION GetLastModifiedReportDate
()

RETURNS datetime

AS

/* presumes report database is on the same server as current database (NAC_CM) */

BEGIN

	DECLARE @LastModifiedDate datetime

	set @LastModifiedDate = getdate()

	select @LastModifiedDate = max(ModifiedDate) from ReportServer.dbo.Catalog

	RETURN @LastModifiedDate
END

