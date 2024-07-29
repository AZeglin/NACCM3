IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectYearQuartersForDateRange' )
BEGIN
	DROP PROCEDURE SelectYearQuartersForDateRange
END
GO

CREATE PROCEDURE SelectYearQuartersForDateRange
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@StartDate datetime,
@EndDate datetime
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@minQuarterId int,
		@maxQuarterId int



BEGIN TRANSACTION

	if @StartDate > @EndDate
	BEGIN
		select @errorMsg = 'Error encountered when selecting year qtr: startdate cannot be greater than enddate'
		goto ERROREXIT	
	END

	select @minQuarterId = Quarter_ID
	from tlkup_year_qtr
	where @StartDate between Start_Date and End_Date

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error selecting @minQuarterId'
		goto ERROREXIT
	END

	select @maxQuarterId = Quarter_ID
	from tlkup_year_qtr
	where @EndDate between Start_Date and End_Date

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error selecting @maxQuarterId'
		goto ERROREXIT
	END

	select Quarter_ID, YEAR, Qtr, Title as YearQuarterDescription, Start_Date, End_Date, Calendar_Year
	from tlkup_year_qtr
	where Quarter_ID between @minQuarterId and @maxQuarterId


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <= 0
	BEGIN
		select @errorMsg = 'Error selecting year qtr'
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


