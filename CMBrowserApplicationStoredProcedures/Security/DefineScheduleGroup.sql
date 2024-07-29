IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DefineScheduleGroup')
	BEGIN
		DROP  Procedure  DefineScheduleGroup
	END

GO

CREATE Procedure DefineScheduleGroup
(
@UserLogin nvarchar(120),
@ScheduleGroupId int,
@ScheduleNumberList nvarchar(240)
)

AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250),
	@commaSearch int,
	/* ordinality helps to define the sort order. */
	/* currently the desired outcome is to have specialized groups ( created manually ) appear at the top of the list */
	@ordinality int 
BEGIN

	if LEN( @ScheduleNumberList ) = 0
	BEGIN
		select @errorMsg = 'Error updating schedule group. List of schedules was blank.'
		Raiserror( @errorMsg, 16, 1 )		
	END

	select @commaSearch = CHARINDEX( ',', @ScheduleNumberList, 0 )

	if @commaSearch = 0
	BEGIN
		select @ordinality = 9
	END
	else
	BEGIN
		select @ordinality = 5
	END
	
	update SEC_ScheduleGroups
	set ScheduleNumberList = @ScheduleNumberList,
		Ordinality = @ordinality,
		LastModifiedBy = @UserLogin,
		LastModificationDate = GETDATE()
	where ScheduleGroupId = @ScheduleGroupId

	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating schedule group with new schedule list'
		Raiserror( @errorMsg, 16, 1 )		
	END

END



