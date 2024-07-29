IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetNextDateScenario]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetNextDateScenario]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[GetNextDateScenario]
(
@scenario int,
@currentStartDateNumber int,
@currentEndDateNumber int,
@nextStartDateNumber int OUTPUT,
@nextEndDateNumber int OUTPUT
)

As

BEGIN

	if @scenario = 1
	BEGIN

		if @currentStartDateNumber = 0 AND @currentEndDateNumber = 1
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 2
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 2
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 3
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 3
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 5
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 5
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 6
		END
			else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 3
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 3
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 5
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 5
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 6
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 4
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 4
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 5
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 5
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 6
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 7
		END
		else
		BEGIN
			select @nextStartDateNumber = -1
			select @nextEndDateNumber = -1
		END
	END
	else if @scenario = 2
	BEGIN

		if @currentStartDateNumber = 0 AND @currentEndDateNumber = 1
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 2
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 2
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 3
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 3
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 5
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 5
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 6
		END
			else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 3
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 3
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 5
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 5
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 6
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 9
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 11
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 12
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 4
		END		
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 4
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 5
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 5
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 6
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 6
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 7
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 7
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 10
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 10
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 11
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 11 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 13
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 13
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 14
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 14
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 14 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 14
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 14 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 14
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 14 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 15
			select @nextEndDateNumber = 16
		END
		else if @currentStartDateNumber = 15 AND @currentEndDateNumber = 16
		BEGIN
			select @nextStartDateNumber = 15
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 15 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 15
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 15 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 17
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 17 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 18
			select @nextEndDateNumber = 19
		END
		else
		BEGIN
			select @nextStartDateNumber = -1
			select @nextEndDateNumber = -1
		END
	END
	else if @scenario = 3 OR @scenario = 4
	BEGIN

		if @currentStartDateNumber = 0 AND @currentEndDateNumber = 1
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 2
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 2
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 3
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 3
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 5
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 5
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 6
		END
			else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 14
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 0
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 0 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 3
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 3
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 5
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 5
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 6
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 9
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 11
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 12
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 14
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 15
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 17
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 2
			select @nextEndDateNumber = 18
		END		
		else if @currentStartDateNumber = 2 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 4
		END		
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 4
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 5
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 5
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 6
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 14
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 3
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 3 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 6
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 6
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 14
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 5
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 5 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 7
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 7
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 8
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 8
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 14
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 6
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 6 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 9
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 9
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 14
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 8
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 8 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 10
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 10
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 11
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 11
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 14
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 9
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 9 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 10
			select @nextEndDateNumber = 19
		END
		else if @currentStartDateNumber = 10 AND @currentEndDateNumber = 19
		BEGIN
			select @nextStartDateNumber = 11
			select @nextEndDateNumber = 12
		END
		else if @currentStartDateNumber = 11 AND @currentEndDateNumber = 12
		BEGIN
			select @nextStartDateNumber = 11
			select @nextEndDateNumber = 14
		END
		else if @currentStartDateNumber = 11 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 11
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 11 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 11
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 11 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 11
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 11 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 13
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 13
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 14
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 14
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 12
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 12 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 14
			select @nextEndDateNumber = 15
		END
		else if @currentStartDateNumber = 14 AND @currentEndDateNumber = 15
		BEGIN
			select @nextStartDateNumber = 14
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 14 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 14
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 14 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 15
			select @nextEndDateNumber = 16
		END
		else if @currentStartDateNumber = 15 AND @currentEndDateNumber = 16
		BEGIN
			select @nextStartDateNumber = 15
			select @nextEndDateNumber = 17
		END
		else if @currentStartDateNumber = 15 AND @currentEndDateNumber = 17
		BEGIN
			select @nextStartDateNumber = 15
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 15 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 17
			select @nextEndDateNumber = 18
		END
		else if @currentStartDateNumber = 17 AND @currentEndDateNumber = 18
		BEGIN
			select @nextStartDateNumber = 18
			select @nextEndDateNumber = 19
		END
		else
		BEGIN
			select @nextStartDateNumber = -1
			select @nextEndDateNumber = -1
		END
	END	
END
