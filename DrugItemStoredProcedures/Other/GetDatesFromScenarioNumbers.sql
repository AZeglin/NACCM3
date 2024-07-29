IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetDatesFromScenarioNumbers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetDatesFromScenarioNumbers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[GetDatesFromScenarioNumbers]
(
	@scenarioNumber int,
	@scenarioStartDateNumber int,
	@scenarioEndDateNumber int,
	@scenarioStartDate datetime OUTPUT,
	@scenarioEndDate datetime OUTPUT,
	
	@existingStartDate1 datetime OUTPUT,
	@existingEndDate1 datetime OUTPUT,
	@existingStartDate2 datetime OUTPUT,
	@existingEndDate2 datetime OUTPUT,
	@existingStartDate3 datetime = null OUTPUT,
	@existingEndDate3 datetime = null OUTPUT
)
As

/* only used for scenario 2 and upwards */
/* converts date numbers into dates for various date scenarios */

DECLARE

	@date0 datetime,
	@date1 datetime,
	@date2 datetime,
	@date3 datetime,
	@date4 datetime,
	@date5 datetime,
	@date6 datetime,
	@date7 datetime,
	@date8 datetime,
	@date9 datetime,
	@date10 datetime,
	@date11 datetime,
	@date12 datetime,
	@date13 datetime,
	@date14 datetime,
	@date15 datetime,
	@date16 datetime,
	@date17 datetime,
	@date18 datetime,
	@date19 datetime
	
	
BEGIN

	/* dates for new price scenarios */
	if @scenarioNumber = 2 
	BEGIN
		select @date0 = DATEADD( day, -9, getdate() )
		select @date1 = DATEADD( day, -8, getdate() )
		select @date2 = DATEADD( day, -7, getdate() )
		select @date3 = DATEADD( day, -6, getdate() )
		select @date4 = DATEADD( day, -5, getdate() )
		select @date5 = DATEADD( day, -4, getdate() )
		select @date6 = DATEADD( day, -3, getdate() )
		select @date7 = DATEADD( day, -2, getdate() )
		select @date8 = DATEADD( day, -1, getdate() )
		select @date9 = getdate() 
		select @date10 = DATEADD( day, 1, getdate() )
		select @date11 = DATEADD( day, 2, getdate() )
		select @date12 = DATEADD( day, 3, getdate() )
		select @date13 = DATEADD( day, 4, getdate() )
		select @date14 = DATEADD( day, 5, getdate() )
		select @date15 = DATEADD( day, 6, getdate() )
		select @date16 = DATEADD( day, 7, getdate() )
		select @date17 = DATEADD( day, 8, getdate() )
		select @date18 = DATEADD( day, 9, getdate() )
		select @date19 = DATEADD( day, 10, getdate() )
	END
	else
		BEGIN
		select @date0 = DATEADD( day, -9, getdate() )
		select @date1 = DATEADD( day, -8, getdate() )
		select @date2 = DATEADD( day, -7, getdate() )
		select @date3 = DATEADD( day, -6, getdate() )
		select @date4 = DATEADD( day, -5, getdate() )
		select @date5 = DATEADD( day, -4, getdate() )
		select @date6 = DATEADD( day, -3, getdate() )
		select @date7 = DATEADD( day, -2, getdate() )
		select @date8 = DATEADD( day, -1, getdate() )
		select @date9 = getdate() 
		select @date10 = DATEADD( day, 1, getdate() )
		select @date11 = DATEADD( day, 2, getdate() )
		select @date12 = DATEADD( day, 3, getdate() )
		select @date13 = DATEADD( day, 4, getdate() )
		select @date14 = DATEADD( day, 5, getdate() )
		select @date15 = DATEADD( day, 6, getdate() )
		select @date16 = DATEADD( day, 7, getdate() )
		select @date17 = DATEADD( day, 8, getdate() )
		select @date18 = DATEADD( day, 9, getdate() )
		select @date19 = DATEADD( day, 10, getdate() )
	END
	/* dates of current prices for each scenario are assigned below */
	if @scenarioNumber = 2
	BEGIN
			select @existingStartDate1 = @date8
			select @existingEndDate1 = @date11
			
			select @existingStartDate2 = @date14
			select @existingEndDate2 = @date17
	END
	else if @scenarioNumber = 3 OR @scenarioNumber = 4
	BEGIN		
			select @existingStartDate1 = @date8
			select @existingEndDate1 = @date11

			select @existingStartDate2 = @date14
			select @existingEndDate2 = @date16

			select @existingStartDate3 = @date18
			select @existingEndDate3 = @date19

	END			
	else if @scenarioNumber = 42 -- called by scenario 4 for deletion test
	BEGIN
			select @existingStartDate1 = @date2
			select @existingEndDate1 = @date5
			
			select @existingStartDate2 = @date6
			select @existingEndDate2 = @date9

			select @existingStartDate3 = @date10
			select @existingEndDate3 = @date13
	END			
	/* start date */
	if @scenarioStartDateNumber = 0
	BEGIN
		select @scenarioStartDate = @date0
	END
	else if @scenarioStartDateNumber = 1
	BEGIN
		select @scenarioStartDate = @date1
	END
	else if @scenarioStartDateNumber = 2
	BEGIN
		select @scenarioStartDate = @date2
	END
	else if @scenarioStartDateNumber = 3
	BEGIN
		select @scenarioStartDate = @date3
	END
	else if @scenarioStartDateNumber = 4
	BEGIN
		select @scenarioStartDate = @date4
	END
	else if @scenarioStartDateNumber = 5
	BEGIN
		select @scenarioStartDate = @date5
	END
	else if @scenarioStartDateNumber = 6
	BEGIN
		select @scenarioStartDate = @date6
	END
	else if @scenarioStartDateNumber = 7
	BEGIN
		select @scenarioStartDate = @date7
	END			
	else if @scenarioStartDateNumber = 8
	BEGIN
		select @scenarioStartDate = @date8
	END			
	else if @scenarioStartDateNumber = 9
	BEGIN
		select @scenarioStartDate = @date9
	END			
	else if @scenarioStartDateNumber = 10
	BEGIN
		select @scenarioStartDate = @date10
	END			
	else if @scenarioStartDateNumber = 11
	BEGIN
		select @scenarioStartDate = @date11
	END			
	else if @scenarioStartDateNumber = 12
	BEGIN
		select @scenarioStartDate = @date12
	END			
	else if @scenarioStartDateNumber = 13
	BEGIN
		select @scenarioStartDate = @date13
	END			
	else if @scenarioStartDateNumber = 14
	BEGIN
		select @scenarioStartDate = @date14
	END			
	else if @scenarioStartDateNumber = 15
	BEGIN
		select @scenarioStartDate = @date15
	END			
	else if @scenarioStartDateNumber = 16
	BEGIN
		select @scenarioStartDate = @date16
	END			
	else if @scenarioStartDateNumber = 17
	BEGIN
		select @scenarioStartDate = @date17
	END			
	else if @scenarioStartDateNumber = 18
	BEGIN
		select @scenarioStartDate = @date18
	END			
	else if @scenarioStartDateNumber = 19
	BEGIN
		select @scenarioStartDate = @date19
	END			
	/* end date */
	if @scenarioEndDateNumber = 0
	BEGIN
		select @scenarioEndDate = @date0
	END
	else if @scenarioEndDateNumber = 1
	BEGIN
		select @scenarioEndDate = @date1
	END
	else if @scenarioEndDateNumber = 2
	BEGIN
		select @scenarioEndDate = @date2
	END
	else if @scenarioEndDateNumber = 3
	BEGIN
		select @scenarioEndDate = @date3
	END
	else if @scenarioEndDateNumber = 4
	BEGIN
		select @scenarioEndDate = @date4
	END
	else if @scenarioEndDateNumber = 5
	BEGIN
		select @scenarioEndDate = @date5
	END
	else if @scenarioEndDateNumber = 6
	BEGIN
		select @scenarioEndDate = @date6
	END
	else if @scenarioEndDateNumber = 7
	BEGIN
		select @scenarioEndDate = @date7
	END
	else if @scenarioEndDateNumber = 8
	BEGIN
		select @scenarioEndDate = @date8
	END	
	else if @scenarioEndDateNumber = 9
	BEGIN
		select @scenarioEndDate = @date9
	END	
	else if @scenarioEndDateNumber = 10
	BEGIN
		select @scenarioEndDate = @date10
	END
	else if @scenarioEndDateNumber = 11
	BEGIN
		select @scenarioEndDate = @date11
	END
	else if @scenarioEndDateNumber = 12
	BEGIN
		select @scenarioEndDate = @date12
	END
	else if @scenarioEndDateNumber = 13
	BEGIN
		select @scenarioEndDate = @date13
	END
	else if @scenarioEndDateNumber = 14
	BEGIN
		select @scenarioEndDate = @date14
	END
	else if @scenarioEndDateNumber = 15
	BEGIN
		select @scenarioEndDate = @date15
	END
	else if @scenarioEndDateNumber = 16
	BEGIN
		select @scenarioEndDate = @date16
	END
	else if @scenarioEndDateNumber = 17
	BEGIN
		select @scenarioEndDate = @date17
	END
	else if @scenarioEndDateNumber = 18
	BEGIN
		select @scenarioEndDate = @date18
	END
	else if @scenarioEndDateNumber = 19
	BEGIN
		select @scenarioEndDate = @date19
	END
	

END
