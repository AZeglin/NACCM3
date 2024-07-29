IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectOfferActionsForReport')
	BEGIN
		DROP  Procedure  SelectOfferActionsForReport
	END

GO

CREATE Procedure SelectOfferActionsForReport
(
@SelectFlag int   -- -1 = include initial value of 'All' for reports
)

AS

BEGIN
	if @SelectFlag = -1
	BEGIN
	
		select -1 as Action_ID, 'All' as Action_Description, 0 as IsActionComplete
		
		union

		select Action_ID, Action_Description, Complete as IsActionComplete
		from tlkup_Offers_Action_Type
		order by Action_Description
	END
	else
	BEGIN
		select Action_ID, Action_Description, Complete as IsActionComplete
		from tlkup_Offers_Action_Type
		order by Action_Description
	END

END

