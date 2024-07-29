IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetContractNumbersForSBAPlanFunction')
	BEGIN
		DROP  Function  GetContractNumbersForSBAPlanFunction
	END

GO

CREATE FUNCTION GetContractNumbersForSBAPlanFunction
(
@SBAPlanID int,
@ActiveOrBoth char(1),  --  'A' for Active, 'B' for Both
@Division int           -- 1 = FSS or 2 = National
)

RETURNS nvarchar(MAX)

AS

BEGIN

	DECLARE  @contractList as nvarchar(MAX)
	set @contractList = null

	if @ActiveOrBoth = 'A'
	BEGIN
		select @contractList = COALESCE( @contractList + ', ', '') + CntrctNum
		from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		where SBAPlanID = @SBAPlanID
		and dbo.IsContractActiveFunction( CntrctNum, getdate() ) = 1
		and s.Division = @Division
	END
	else
	BEGIN
		select @contractList = COALESCE( @contractList + ', ', '') + [CntrctNum] 
		from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
		where SBAPlanID = @SBAPlanID
		and s.Division = @Division
	END

	RETURN @contractList
END


