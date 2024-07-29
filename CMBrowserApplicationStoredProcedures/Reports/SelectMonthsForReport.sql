IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectMonthsForReport')
	BEGIN
		DROP  Procedure  SelectMonthsForReport
	END

GO

CREATE Procedure SelectMonthsForReport
(
@AllMonthsSelectFlag int = 0 /* -1 = include 'All' in the result set */
)

AS

BEGIN


	if @AllMonthsSelectFlag = -1
	BEGIN
		select -1 as MonthValue,  'All' as MonthName, -1 as SortOrder  
		union
		select 1 as MonthValue, 'January' as MonthName, 1 as SortOrder			
		union
		select 2 as MonthValue, 'February' as MonthName, 2 as SortOrder			
		union
		select 3 as MonthValue, 'March' as MonthName, 3 as SortOrder			
		union
		select 4 as MonthValue, 'April' as MonthName, 4 as SortOrder			
		union
		select 5 as MonthValue, 'May' as MonthName, 5 as SortOrder			
		union
		select 6 as MonthValue, 'June' as MonthName, 6 as SortOrder			
		union
		select 7 as MonthValue, 'July' as MonthName, 7 as SortOrder			
		union
		select 8 as MonthValue, 'August' as MonthName, 8 as SortOrder			
		union
		select 9 as MonthValue, 'September' as MonthName, 9 as SortOrder			
		union
		select 10 as MonthValue, 'October' as MonthName, 10 as SortOrder			
		union
		select 11 as MonthValue, 'November' as MonthName, 11 as SortOrder			
		union
		select 12 as MonthValue, 'December' as MonthName, 12 as SortOrder			

		order by SortOrder
	END
	else
	BEGIN
		select 1 as MonthValue, 'January' as MonthName, 1 as SortOrder			
		union
		select 2 as MonthValue, 'February' as MonthName, 2 as SortOrder			
		union
		select 3 as MonthValue, 'March' as MonthName, 3 as SortOrder			
		union
		select 4 as MonthValue, 'April' as MonthName, 4 as SortOrder			
		union
		select 5 as MonthValue, 'May' as MonthName, 5 as SortOrder			
		union
		select 6 as MonthValue, 'June' as MonthName, 6 as SortOrder			
		union
		select 7 as MonthValue, 'July' as MonthName, 7 as SortOrder			
		union
		select 8 as MonthValue, 'August' as MonthName, 8 as SortOrder			
		union
		select 9 as MonthValue, 'September' as MonthName, 9 as SortOrder			
		union
		select 10 as MonthValue, 'October' as MonthName, 10 as SortOrder			
		union
		select 11 as MonthValue, 'November' as MonthName, 11 as SortOrder			
		union
		select 12 as MonthValue, 'December' as MonthName, 12 as SortOrder			

		order by SortOrder
	END

END