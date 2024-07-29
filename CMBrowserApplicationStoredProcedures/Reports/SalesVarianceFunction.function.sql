IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'SalesVarianceFunction')
	BEGIN
		DROP  Function  SalesVarianceFunction
	END

GO

CREATE Function SalesVarianceFunction
(
@CurrentSales money,
@PreviousSales money
)

RETURNS numeric(18, 4)

AS

BEGIN

	Declare @variance numeric(18, 4)
	
	if @PreviousSales is null
	BEGIN
		if @CurrentSales is null
		BEGIN
			select @variance = 0
		END
		else if @CurrentSales = 0
		BEGIN
			select @variance = 0
		END
		else
		BEGIN
			select @variance = 1.0000  -- should be null
		END
	END
	else if @PreviousSales = 0
	BEGIN
		if @CurrentSales is null
		BEGIN
			select @variance = 0
		END
		else if @CurrentSales = 0
		BEGIN
			select @variance = 0
		END
		else
		BEGIN
			select @variance = 1.0000 -- should be null
		END
	END			
	else -- previous has a value
	BEGIN
		if @CurrentSales is null
		BEGIN
			select @variance = -1.0000
		END
		else if @CurrentSales = 0
		BEGIN
			select @variance = -1.0000
		END
		else
		BEGIN
			select @variance = ( @CurrentSales - @PreviousSales ) / @PreviousSales
		END
	END

	return @variance
	
END