IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectAccessPoints')
	BEGIN
		DROP  Procedure  SelectAccessPoints
	END

GO

CREATE Procedure SelectAccessPoints
(
@UserId uniqueidentifier
)

AS

BEGIN

	select AccessPointId, AccessPointDescription
	from SEC_AccessPoints
	order by AccessPointDescription
	
END
