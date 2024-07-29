IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectYearsForReport')
	BEGIN
		DROP  Procedure  SelectYearsForReport
	END

GO

CREATE Procedure SelectYearsForReport
(
@Order bit = 1,  /* 1 = asc */
@FutureYears int = 0,  /* years beyond this year */
@IncludedStartingYear int = null,   /* optional starting year to include */
@AllYearsSelectFlag int = 0 /* -1 = include 'All' in the result set */
)

AS

	declare @MaxYear int,
			@MinYear int

BEGIN

	select @MaxYear = YEAR( getdate() ) + @FutureYears
	select @MinYear = MIN( Year ) from tlkup_year_qtr
	
	if @IncludedStartingYear is not null
	BEGIN
		select @MinYear = @IncludedStartingYear
	END

	if @AllYearsSelectFlag = -1
	BEGIN
		if @Order = 1
		BEGIN
			select distinct Year as YearDescription, Year as YearValue, Year as SortOrder 
			from tlkup_year_qtr
			where Year <= @MaxYear
			and Year >= @MinYear
			
			union
			
			select 'All' as YearDescription, -1 as YearValue, -1 as SortOrder  
			from tlkup_year_qtr
			where Year <= @MaxYear
			and Year >= @MinYear

			order by SortOrder
		END
		else
		BEGIN
			select distinct Year as YearDescription, Year as YearValue, Year as SortOrder  
			from tlkup_year_qtr
			where Year <= @MaxYear
			and Year >= @MinYear
			
			union
			
			select 'All' as YearDescription, -1 as YearValue, @MaxYear + 1 as SortOrder  
			from tlkup_year_qtr
			where Year <= @MaxYear
			and Year >= @MinYear

			order by SortOrder desc
		END
	END
	else
	BEGIN
		if @Order = 1
		BEGIN
			select distinct Year as YearDescription, Year as YearValue, Year as SortOrder  
			from tlkup_year_qtr
			where Year <= @MaxYear
			and Year >= @MinYear
			order by SortOrder
		END
		else
		BEGIN
			select distinct Year as YearDescription, Year as YearValue, Year as SortOrder  
			from tlkup_year_qtr
			where Year <= @MaxYear
			and Year >= @MinYear
			order by SortOrder desc
		END
	END
END