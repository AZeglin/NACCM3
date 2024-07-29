IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectDivisions')
	BEGIN
		DROP  Procedure  SelectDivisions
	END

GO

CREATE Procedure SelectDivisions
(
@UserId uniqueidentifier,
@Select bit   /* 1 = include 'All' in the output */
)

AS

BEGIN

	DECLARE @All int,		
		@AllDescription nvarchar(10),		
		@AllOrdinality int
		

	select @All = -1
	select @AllDescription = 'All'	
	select @AllOrdinality = -1

	if @Select = 0
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
