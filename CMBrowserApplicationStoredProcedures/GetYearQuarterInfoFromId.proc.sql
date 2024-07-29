IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetYearQuarterInfoFromId' )
BEGIN
	DROP PROCEDURE GetYearQuarterInfoFromId
END
GO

CREATE PROCEDURE GetYearQuarterInfoFromId
(
@UserLogin nvarchar(120),
@QuarterId int,
@YearQuarterDescription nvarchar(20) OUTPUT,
@FiscalYear int OUTPUT,
@Quarter int OUTPUT,
@QuarterStartDate datetime OUTPUT,
@QuarterEndDate datetime OUTPUT,
@CalendarYear int OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	select @FiscalYear = YEAR, 
		@Quarter = Qtr, 
		@YearQuarterDescription = Title, 
		@QuarterStartDate = Start_Date, 
		@QuarterEndDate = End_Date, 
		@CalendarYear = Calendar_Year
	from tlkup_year_qtr
	where Quarter_ID = @QuarterId


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <= 0
	BEGIN
		select @errorMsg = 'Error selecting year qtr info for quarterId=' + CONVERT( nvarchar(20), @QuarterId )
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


