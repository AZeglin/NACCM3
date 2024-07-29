IF EXISTS (SELECT * FROM sysobjects WHERE type in (N'FN', N'IF', N'TF', N'FS', N'FT') AND name = 'IsContractActiveFunction')
	BEGIN
		DROP  Function  IsContractActiveFunction
	END

GO

Create Function [dbo].[IsContractActiveFunction]
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
	
	 if exists ( select c.Contract_Record_ID
				from NAC_CM.dbo.tbl_Cntrcts c
				where c.CntrctNum = @CntrctNum
				and (( c.Dates_CntrctExp >= @CurrentDateWithoutTime and c.Dates_Completion is null ) or
								( c.Dates_Completion is not null and c.Dates_CntrctExp >= @CurrentDateWithoutTime and c.Dates_Completion >= @CurrentDateWithoutTime )) )
	BEGIN
		select @IsActive = 1
	END

	return @IsActive	
END


