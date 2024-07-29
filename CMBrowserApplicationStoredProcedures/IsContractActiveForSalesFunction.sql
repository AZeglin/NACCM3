IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'IsContractActiveForSalesFunction')
	BEGIN
		DROP  Function  IsContractActiveForSalesFunction
	END

GO

CREATE Function IsContractActiveForSalesFunction
(
@CntrctNum nvarchar(20),
@StartDate datetime,
@EndDate datetime
)

RETURNS bit

AS

BEGIN

	/* this version is for use with sales and needs to also look at the contract effective date */

	DECLARE @IsActive bit,
			@StartDateWithoutTime datetime,
			@EndDateWithoutTime datetime
		
	select @IsActive = 0	
	select @StartDateWithoutTime = CAST( CONVERT( CHAR(8), @StartDate, 112 ) as DATETIME )
	select @EndDateWithoutTime = CAST( CONVERT( CHAR(8), @EndDate, 112 ) as DATETIME )
	
	 if exists ( select Contract_Record_ID
				from tbl_Cntrcts
				where CntrctNum = @CntrctNum
				
				and (( tbl_Cntrcts.Dates_CntrctExp >= @StartDateWithoutTime
				and tbl_Cntrcts.Dates_Effective <= @EndDateWithoutTime
				and tbl_Cntrcts.Dates_Completion is null )
				or ( tbl_Cntrcts.Dates_Completion >= @StartDateWithoutTime
				and tbl_Cntrcts.Dates_Effective <= @EndDateWithoutTime
				and tbl_Cntrcts.Dates_Completion is not null 				
				and datediff( dd, tbl_Cntrcts.Dates_Effective, tbl_Cntrcts.Dates_Completion ) > 0 )))   -- special case to eliminate contracts terminated before their effective date
								
	BEGIN
		select @IsActive = 1
	END

	return @IsActive
	
END

