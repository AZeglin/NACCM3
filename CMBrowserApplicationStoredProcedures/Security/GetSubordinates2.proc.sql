IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetSubordinates2' )
BEGIN
	DROP PROCEDURE GetSubordinates2
END
GO

CREATE PROCEDURE GetSubordinates2
(
@TargetCOID int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@operationalStatusIdList nvarchar(1000),
		@isAllowedByOperationalStatusIdList nvarchar(1000),
		@operationalStatusGroupIdList nvarchar(1000),
		@TargetDivision int,
		@subordinateIdCount int,
		@bossRoleCount int


BEGIN TRANSACTION

	-- determine if user is in any boss roles
	create table #TargetBossRoles
	(
		RoleId int,
		TreeLevel int
	)

	insert into #TargetBossRoles
	( RoleId, TreeLevel )
	select r.RoleId, 0 
	from SEC_Roles r join SEC_UserProfileUserRoles u on u.RoleId = r.RoleId 
	where u.CO_ID = @TargetCOID
	and r.IsBossRole = 1

	select @bossRoleCount = COUNT(*) from #TargetBossRoles

	-- no subordinates
	if @bossRoleCount = 0
	BEGIN
		goto OKEXIT
	END

	-- gather subordinate boss roles for 2 levels only
	insert into #TargetBossRoles
	( RoleId, TreeLevel )
	select r.RoleId, 1 
	from SEC_Roles r join #TargetBossRoles t on r.ImmediateBossRoleId = t.RoleId
	where t.TreeLevel = 0
	and r.IsBossRole = 1

	insert into #TargetBossRoles
	( RoleId, TreeLevel )
	select r.RoleId, 2 
	from SEC_Roles r join #TargetBossRoles t on r.ImmediateBossRoleId = t.RoleId
	where t.TreeLevel = 1
	and r.IsBossRole = 1

	-- gather subordinate non-boss roles for all gathered boss roles
	insert into #TargetBossRoles
	( RoleId, TreeLevel )
	select r.RoleId, 3 
	from SEC_Roles r join #TargetBossRoles t on r.ImmediateBossRoleId = t.RoleId
	where r.IsBossRole = 0

	create table #UsersWithContractCreationAccessPoint
	(
		CO_ID int not null,
		Inactive bit not null
	)

	insert into #UsersWithContractCreationAccessPoint
	( CO_ID, Inactive )
	select distinct u.CO_ID, u.Inactive
	from SEC_UserProfile u join SEC_UserProfileUserRoles r on r.CO_ID = u.CO_ID
		join SEC_RoleAccessPoints p on p.RoleId = r.RoleId
		join SEC_AccessPoints a on a.AccessPointId = p.AccessPointId
	where a.RequiredForContractingOfficer = 1

	select @error = @@error
		
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting users with contract access points.'
		goto ERROREXIT
	END

	-- get user's division
	select @TargetDivision = Division 
	from SEC_UserProfile
	where CO_ID = @TargetCOID

	select @error = @@error, @rowCount = @@ROWCOUNT
		
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error selecting target division.'
		goto ERROREXIT
	END

	select distinct r.CO_ID, r.UserId, u.FirstName, u.LastName, u.FullName, u.Inactive, u.Division
	from SEC_UserProfileUserRoles r join SEC_UserProfile u on r.CO_ID = u.CO_ID
	join #UsersWithContractCreationAccessPoint t on t.CO_ID = u.CO_ID
	join #TargetBossRoles b on r.RoleId = b.RoleId
	where u.Division = @TargetDivision
	and u.CO_ID <> @TargetCOID
		

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error making final selection of subordinates.'
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


