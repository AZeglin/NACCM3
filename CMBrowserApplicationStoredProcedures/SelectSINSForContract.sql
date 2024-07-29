IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSINSForContract')
	BEGIN
		DROP  Procedure  SelectSINSForContract
	END

GO

CREATE Procedure SelectSINSForContract
(
@ContractNumber nvarchar(50),
@IncludeInvalid int
)

AS

DECLARE @Division int,
		@error int,
		@rowcount int,
		@retVal int,
		@errorMsg nvarchar(250)

BEGIN TRANSACTION

	select s.SINs as Sins, s.Recoverable, s.SINs as SortOrder
	from tbl_Cntrcts_SINs s where s.CntrctNum = @ContractNumber

	-- note the below addition of placeholder SINs for National Contracts is no longer reqd as of 8/2013 since the SINs are added to the table

	--select @Division = s.Division
	--from [tlkup_Sched/Cat] s join tbl_Cntrcts c on s.Schedule_Number = c.Schedule_Number
	--where c.CntrctNum = @ContractNumber
	
	--if @error <> 0 or @rowcount <> 1
	--BEGIN
	--	select @errorMsg = 'Unable to select division for contract number: ' + @ContractNumber
	--	GOTO ERROREXIT					
	--END

	--if @Division = 2  /* National */
	--BEGIN
	
	--	if @IncludeInvalid = 1
	--	BEGIN

	--		select s.SINs as Sins, s.Recoverable, s.SINs as SortOrder
	--		from tbl_Cntrcts_SINs s where s.CntrctNum = @ContractNumber
			
	--		union
			
	--		select 'Invalid' as Sins, 0 as Recoverable, '-1' as SortOrder

	--		union 
			
	--		select 'NC' as Sins, 0 as Recoverable, '0' as SortOrder

	--		order by SortOrder
			
	--	END
	--	else
	--	BEGIN
	--		select s.SINs as Sins, s.Recoverable, s.SINs as SortOrder
	--		from tbl_Cntrcts_SINs s where s.CntrctNum = @ContractNumber

	--		union 
			
	--		select 'NC' as Sins, 0 as Recoverable, '0' as SortOrder

	--		order by SortOrder

	--	END
	--END
	--else
	--BEGIN
	--	if @IncludeInvalid = 1
	--	BEGIN

	--		select s.SINs as Sins, s.Recoverable, s.SINs as SortOrder
	--		from tbl_Cntrcts_SINs s where s.CntrctNum = @ContractNumber
			
	--		union
			
	--		select 'Invalid' as Sins, 0 as Recoverable, '-1' as SortOrder

	--		order by SortOrder
			
	--	END
	--	else
	--	BEGIN
	--		select s.SINs as Sins, s.Recoverable, s.SINs as SortOrder
	--		from tbl_Cntrcts_SINs s where s.CntrctNum = @ContractNumber
	--		order by s.SINs	
	--	END	
	--END


GOTO OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 ) 

	IF @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
	/* only rollback iff this the highest level */ 
		ROLLBACK TRANSACTION
	END

	RETURN (-1)

OKEXIT:

	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)

