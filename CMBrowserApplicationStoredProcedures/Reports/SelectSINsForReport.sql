IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSINsForReport')
	BEGIN
		DROP  Procedure  SelectSINsForReport
	END

GO

CREATE Procedure SelectSINsForReport
(
@DivisionId as int = -1,
@AllSINsSelectFlag int = 0 /* -1 = include 'All' in the result set */
)
AS

BEGIN

	if @DivisionId = 1
	BEGIN
		if @AllSINsSelectFlag = -1
		BEGIN
			select 'All' as [DisplaySIN],  'All' as [SIN], '' as SortOrder
			union
			select distinct c.SINs as [DisplaySIN], c.SINs as [SIN], c.SINs as SortOrder 
			from tbl_Cntrcts_SINs c join tbl_SINs t on c.SINs = t.SIN
			join [tlkup_Sched/Cat] s on t.[Schedule_ Number] = s.Schedule_Number
			where s.Division = @DivisionId
			and c.Inactive = 0
			order by SortOrder
		END
		else
		BEGIN
			select distinct c.SINs as [DisplaySIN], c.SINs as [SIN], c.SINs as SortOrder 
			from tbl_Cntrcts_SINs c join tbl_SINs t on c.SINs = t.SIN
			join [tlkup_Sched/Cat] s on t.[Schedule_ Number] = s.Schedule_Number
			where s.Division = @DivisionId
			and c.Inactive = 0
			order by SortOrder	
		END
	END
	else if @DivisionId = 2
	BEGIN
		select 'All' as [DisplaySIN],  'All' as [SIN], '' as SortOrder
		union
		select distinct c.SINs as [DisplaySIN], c.SINs as [SIN], c.SINs as SortOrder 
		from tbl_Cntrcts_SINs c join tbl_SINs t on c.SINs = t.SIN
		join [tlkup_Sched/Cat] s on t.[Schedule_ Number] = s.Schedule_Number
		where s.Division = @DivisionId
		and c.Inactive = 0
		order by SortOrder	
	END
	else
	BEGIN
		if @AllSINsSelectFlag = -1
		BEGIN
			select 'All' as [DisplaySIN],  'All' as [SIN], '' as SortOrder
			union
			select distinct c.SINs as [DisplaySIN], c.SINs as [SIN], c.SINs as SortOrder 
			from tbl_Cntrcts_SINs c join tbl_SINs t on c.SINs = t.SIN
			join [tlkup_Sched/Cat] s on t.[Schedule_ Number] = s.Schedule_Number
			where s.Division = @DivisionId
			and c.Inactive = 0
			order by SortOrder	
		END
		else
		BEGIN
			select distinct c.SINs as [DisplaySIN], c.SINs as [SIN], c.SINs as SortOrder 
			from tbl_Cntrcts_SINs c join tbl_SINs t on c.SINs = t.SIN
			join [tlkup_Sched/Cat] s on t.[Schedule_ Number] = s.Schedule_Number
			where s.Division = @DivisionId
			and c.Inactive = 0
			order by SortOrder		
		END
	END

END
