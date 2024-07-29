IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'AuthorizeUser')
	BEGIN
		DROP  Procedure  AuthorizeUser
	END

GO

CREATE Procedure AuthorizeUser
(
@UserId uniqueidentifier,
@AccessPointDescription nvarchar(200),
@ScheduleNumber int,
@IsAuthorized bit OUTPUT
)

AS

/* authorize a user for the particular access point and schedule */

Declare 		@accessPointId int,
				@currentCOID int,
				@contractUserId int,
				@errorMsg nvarchar(256),
				@error int,
				@count int,
				@scheduleGroupIdList nvarchar(240), -- comma delimited list of ScheduleGroupId's 
				@query nvarchar(600),
				@resultCount int


BEGIN


	select @currentCOID = CO_ID
	from SEC_UserProfile
	where UserId = @UserId
	
	select @error = @@ERROR, @count = @@ROWCOUNT
		
	if @error <> 0 OR @count <> 1
	BEGIN
		select @errorMsg = 'Invalid UserId (GUID)'
		goto ERROREXIT
	END		
	
	select @accessPointId = AccessPointId
	from SEC_AccessPoints
	where AccessPointDescription = @AccessPointDescription
	
	select @error = @@ERROR, @count = @@ROWCOUNT
		
	if @error <> 0 OR @count <> 1
	BEGIN
		select @errorMsg = 'Invalid AccessPointDescription ' + @AccessPointDescription
		goto ERROREXIT
	END		
	
	select @scheduleGroupIdList = ScheduleGroupIdList
	from SEC_GeneratedGroupSchedules
	where ScheduleNumber = @ScheduleNumber
	
	select @error = @@ERROR, @count = @@ROWCOUNT
		
	if @error <> 0 OR @count <> 1
	BEGIN
		select @errorMsg = 'ScheduleNumber was not found in any defined groups (1) ' + convert( nvarchar(10), @ScheduleNumber )
		goto ERROREXIT
	END		
	
	if LEN(@scheduleGroupIdList) < 1
	BEGIN
		select @errorMsg = 'ScheduleNumber was not found in any defined groups (2) ' + convert( nvarchar(10), @ScheduleNumber )
		goto ERROREXIT
	END
	
	
	select @query = 'select @p_ResultCount = count(*)
	from SEC_UserProfileUserRoles u
	join SEC_Roles r
		on u.RoleId = r.RoleId
	join SEC_RoleScheduleGroups s
		on r.RoleId = s.RoleId
	join SEC_RoleAccessPoints a
		on r.RoleId = a.RoleId
	join SEC_ScheduleGroups g
		on s.ScheduleGroupId = g.ScheduleGroupId
	join SEC_AccessPoints p
		on a.AccessPointId = p.AccessPointId
	where u.UserId = @p_UserId
	and a.AccessPointId = @p_AccessPointId
	and s.ScheduleGroupId in ( ' + @scheduleGroupIdList + ' )'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string'
		goto ERROREXIT
	END
	
	exec SP_EXECUTESQL @query, N'@p_UserId uniqueidentifier, @p_AccessPointId int, @p_ResultCount bit OUTPUT', @p_UserId = @UserId, @p_AccessPointId = @accessPointId, @p_ResultCount = @resultCount OUTPUT

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error running access point query'
		goto ERROREXIT
	END
	
	if @resultCount > 0
	BEGIN
		select @IsAuthorized = 1
	END
	else
	BEGIN
		select @IsAuthorized = 0
	END
	
	goto EXITOK
	
ERROREXIT:

	raiserror( @errorMsg, 16, 1 )
	
EXITOK:

END

