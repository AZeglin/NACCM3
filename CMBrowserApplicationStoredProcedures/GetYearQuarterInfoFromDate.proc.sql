IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetYearQuarterInfoFromDate' )
BEGIN
	DROP PROCEDURE GetYearQuarterInfoFromDate
END
GO

CREATE PROCEDURE GetYearQuarterInfoFromDate
(
@UserLogin nvarchar(120),
@TestDate datetime,
@YearQuarterDescription nvarchar(20) OUTPUT,
@FiscalYear int OUTPUT,
@Quarter int OUTPUT,
@QuarterStartDate datetime OUTPUT,
@QuarterEndDate datetime OUTPUT,
@CalendarYear int OUTPUT,
@TestQuarterId int OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	select @TestQuarterId = Quarter_ID
	from tlkup_year_qtr
	where @TestDate between Start_Date and End_Date

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <= 0
	BEGIN
		select @errorMsg = 'Error selecting year qtr info for date=' + CONVERT( nvarchar(20), @TestDate )
		goto ERROREXIT
	END

	select @FiscalYear = YEAR, 
		@Quarter = Qtr, 
		@YearQuarterDescription = Title, 
		@QuarterStartDate = Start_Date, 
		@QuarterEndDate = End_Date, 
		@CalendarYear = Calendar_Year
	from tlkup_year_qtr
	where Quarter_ID = @TestQuarterId


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <= 0
	BEGIN
		select @errorMsg = 'Error selecting year qtr info for date for quarterId=' + CONVERT( nvarchar(20), @TestQuarterId )
		goto ERROREXIT
	END

goto OKEXIT

ERROREXIT:

	raiserror( @errorMsg, 16, 1 )
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )


