IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'IsContractActiveFunction')
	BEGIN
		DROP  Function  IsContractActiveFunction
	END

GO

CREATE Function IsContractActiveFunction
(
@CntrctNum nvarchar(20),
@CurrentDate datetime
)

RETURNS bit

AS

BEGIN

	DECLARE @IsActive bit,
			@CurrentDateWithoutTime datetime
		
	select @IsActive = 0	
	select @CurrentDateWithoutTime = CAST( CONVERT( CHAR(8), @CurrentDate, 112 ) as DATETIME )
	
	 if exists ( select Contract_Record_ID
				from tbl_Cntrcts
				where CntrctNum = @CntrctNum
				and (( tbl_Cntrcts.Dates_CntrctExp >= @CurrentDateWithoutTime and tbl_Cntrcts.Dates_Completion is null ) or
								( tbl_Cntrcts.Dates_Completion is not null and tbl_Cntrcts.Dates_CntrctExp >= @CurrentDateWithoutTime and tbl_Cntrcts.Dates_Completion >= @CurrentDateWithoutTime )) )
	BEGIN
		select @IsActive = 1
	END

	return @IsActive
	
END

