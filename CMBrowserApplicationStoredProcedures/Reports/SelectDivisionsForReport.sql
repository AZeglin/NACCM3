IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectDivisionsForReport')
	BEGIN
		DROP  Procedure  SelectDivisionsForReport
	END

GO

CREATE Procedure SelectDivisionsForReport
(
@IncludeAll bit = 0
)

AS

BEGIN

	DECLARE @All int,		
		@AllDescription nvarchar(10),		
		@AllOrdinality int

	select @All = -1
	select @AllDescription = 'All'	
	select @AllOrdinality = -1

	if @IncludeAll = 0
	BEGIN
		select Division, Description, Ordinality 
		from SEC_Divisions 
		order by Ordinality
	END
	else
	BEGIN
		select Division, Description, Ordinality
		from SEC_Divisions

		union

		select @All as Division, @AllDescription as Description, @AllOrdinality as Ordinality
		
		order by Ordinality
	END	
END

