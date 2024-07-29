IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetLastDateOfMonthFunction')
	BEGIN
		DROP  Function  GetLastDateOfMonthFunction
	END

GO

CREATE Function GetLastDateOfMonthFunction
(
@MonthNumber int,
@YearNumber int
)

Returns int

AS

BEGIN

	DECLARE @lastDateOfMonth int,
		@datePlusOne datetime,
		@nextMonth int,
		@nextYear int
	
	if @MonthNumber <= 12
	BEGIN	
		if @MonthNumber = 12
		BEGIN
			select @nextMonth = 1
			select @nextYear = @YearNumber + 1
		END
		else
		BEGIN
			select @nextMonth = @MonthNumber + 1
			select @nextYear = @YearNumber
		END

			
		select @datePlusOne = convert( datetime, convert( nvarchar(2), @nextMonth ) + '/1/' + convert( nvarchar(4), @nextYear ))

		select @lastDateOfMonth = DAY( DATEADD( dd, -1, @datePlusOne ))
		
	END
	else
	BEGIN
		select @lastDateOfMonth = 1	
	END
	
	return @lastDateOfMonth
END
