IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetVAIFF]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [GetVAIFF]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create function [GetVAIFF]
(
@ContractNumber nvarchar(20),
@EffectiveDate datetime
)

returns decimal( 18,4 )

as

BEGIN

DECLARE @VAIFF decimal( 18,4 ),
	@quarterId int

	select @VAIFF = -1

	select @quarterId = Quarter_ID 
	from NAC_CM.dbo.tlkup_year_qtr
	where @EffectiveDate between Start_Date and End_Date

	if @@ERROR = 0 AND @@ROWCOUNT = 1
	BEGIN
		select @VAIFF = isnull( f.VA_IFF, -1 )
		from NAC_CM.dbo.tbl_IFF f
		where f.Schedule_Number = ( select Schedule_Number 
								from NAC_CM.dbo.tbl_Cntrcts c
								where c.CntrctNum = @ContractNumber )
		and @quarterId between f.Start_Quarter_Id and f.End_Quarter_Id
	END

	return @VAIFF

END


