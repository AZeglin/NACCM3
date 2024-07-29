IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectDivisions')
	BEGIN
		DROP  Procedure  SelectDivisions
	END

GO

CREATE Procedure SelectDivisions
(
@CurrentUser uniqueidentifier,
@COID int,
@LoginId nvarchar(120)
)

AS

DECLARE @error int,
		@rowcount int,
		@errorMsg nvarchar(200)
		
BEGIN

	select distinct Division as DivisionId, Division_Description as DivisionName, Director, ( case when Division_Description = 'FSS' then 0 else case when Division_Description = 'National Contracts' then 1 else case when Division_Description = 'DALC' then 2 else 3 end end end ) as DisplayOrder
	from [tlkup_Sched/Cat]
	where Inactive = 0
	
	union
	
	select -1 as DivisionId, '--select--' as DivisionName, 0 as Director, -1 as DisplayOrder
	
	order by DisplayOrder
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount < 1
	BEGIN
		select @errorMsg = 'Error retrieving list of divisions from schedcat, for user ' + @LoginId
		raiserror( @errorMsg, 16, 1 )
	END

END
