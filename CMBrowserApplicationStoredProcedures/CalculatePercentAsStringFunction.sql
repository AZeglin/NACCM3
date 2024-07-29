IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'CalculatePercentAsStringFunction')
	BEGIN
		DROP  Function  CalculatePercentAsStringFunction
	END

GO

CREATE Function CalculatePercentAsStringFunction
(
@CurrentValue money,
@PastValue money,
@NAString nvarchar(10)
)

RETURNS nvarchar(20)

AS

BEGIN

	Declare 
			@returnString  nvarchar(20),
			@percent  float

	if @PastValue is null OR @PastValue = 0
	BEGIN
		select @returnString = @NAString
	END
	else
	BEGIN
		/* want whole number percent */
		select @percent = ROUND((( @CurrentValue - @PastValue )/ @PastValue ) * 100, 0, 1 )
		
		if @percent < 0
		BEGIN
			select @returnString = '-' + convert( nvarchar(19), convert( int, @percent )) + '%'	
		END
		else
		BEGIN
			select @returnString = convert( nvarchar(20), convert( int, @percent )) + '%'	
		END				
	END

	return @returnString
END

