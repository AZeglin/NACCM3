IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetSubordinates' )
BEGIN
	DROP PROCEDURE GetSubordinates
END
GO

CREATE PROCEDURE GetSubordinates
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
		@subordinateIdCount int



BEGIN TRANSACTION

	--- THIS VERSION IS NOT USED, instead, use GetSubordinates2
	-- use the generated reverse operational status hierarchy to define subordinates. Prune the list by including
	-- only CO action points and only users in the same division.

	create table #OperationalStatusIdList
	(
		OperationalStatusId int NOT NULL
	)
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #OperationalStatusIdList temp table'
		goto ERROREXIT
	END

	create table #SubordinateOperationalStatusIdList
	(
		OperationalStatusId int NOT NULL
	)
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #SubordinateOperationalStatusIdList temp table'
		goto ERROREXIT
	END

	create table #SubordinateOperationalStatusGroupIdList
	(
		OperationalStatusGroupId int NOT NULL
	)
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #SubordinateOperationalStatusGroupIdList temp table'
		goto ERROREXIT
	END

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

	-- gather target operational status Id's into temp table
	Declare FindOperationalStatusIdsCursor CURSOR For
	select OperationalStatusIdList
	from SEC_OperationalStatusGroups
	where OperationalStatusGroupId in
	(
		select OperationalStatusGroupId
		from SEC_RoleOperationalStatusGroups
		where RoleId in
		(
			select u.RoleId from SEC_UserProfileUserRoles u join SEC_Roles r on u.RoleId = r.RoleId
			where r.IsBossRole = 1 -- only consider user's boss roles
			and u.CO_ID = @TargetCOID
		)
	)

	Open FindOperationalStatusIdsCursor
	
	FETCH NEXT FROM FindOperationalStatusIdsCursor
	INTO @operationalStatusIdList

	WHILE @@FETCH_STATUS = 0
	BEGIN

		insert into #OperationalStatusIdList
		select Value from dbo.SplitFunction( @operationalStatusIdList, ',' )
		where Value not in ( select OperationalStatusId from #OperationalStatusIdList )
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error splitting list from operational status group.'
			goto ERROREXIT
		END
		

		FETCH NEXT FROM FindOperationalStatusIdsCursor
		INTO @operationalStatusIdList

	END

	close FindOperationalStatusIdsCursor
	deallocate FindOperationalStatusIdsCursor

	-- remove operational status Ids not from the target division
	delete #OperationalStatusIdList
	where OperationalStatusId not in ( select OperationalStatusId from SEC_OperationalStatuses where Division = @TargetDivision )

	select @error = @@error
		
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error removing operational statuses not in target division.'
		goto ERROREXIT
	END

	-- gather subordinate ( relative to target ) operational status Id's from the reverse lookup table
	Declare FindSubordinateOperationalStatusIdsCursor CURSOR For
	select IsAllowedByOperationalStatusIdList
	from SEC_GeneratedReverseOperationalStatusHierarchy h join #OperationalStatusIdList o on h.OperationalStatusId = o.OperationalStatusId

	Open FindSubordinateOperationalStatusIdsCursor
	
	FETCH NEXT FROM FindSubordinateOperationalStatusIdsCursor
	INTO @isAllowedByOperationalStatusIdList

	WHILE @@FETCH_STATUS = 0
	BEGIN

		insert into #SubordinateOperationalStatusIdList
		select Value from dbo.SplitFunction( @isAllowedByOperationalStatusIdList, ',' )
		where Value not in ( select OperationalStatusId from #SubordinateOperationalStatusIdList )
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error splitting list from subordinate operational status from reverse hierarchy.'
			goto ERROREXIT
		END
		

		FETCH NEXT FROM FindSubordinateOperationalStatusIdsCursor
		INTO @isAllowedByOperationalStatusIdList

	END

	close FindSubordinateOperationalStatusIdsCursor
	deallocate FindSubordinateOperationalStatusIdsCursor

	-- remove subordinate operational status Ids not from the target division
	delete #SubordinateOperationalStatusIdList
	where OperationalStatusId not in ( select OperationalStatusId from SEC_OperationalStatuses where Division = @TargetDivision )

	select @error = @@error
		
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error removing subordinate operational statuses not in target division.'
		goto ERROREXIT
	END

	-- make sure there is at least one
	select @subordinateIdCount = COUNT(*) from #SubordinateOperationalStatusIdList

	if @subordinateIdCount = 0
	BEGIN
		insert into #SubordinateOperationalStatusIdList
		( OperationalStatusId )
		select OperationalStatusId from #OperationalStatusIdList
	END

	select @error = @@error
		
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error reinserting target operational statuses in place of empty subordinate list.'
		goto ERROREXIT
	END

	-- lookup operational status group ids based on subordinate operational status Ids
	Declare FindSubordinateOperationalStatusGroupIdsCursor CURSOR For
	select OperationalStatusGroupIdList
	from SEC_GeneratedGroupOperationalStatuses g join #SubordinateOperationalStatusIdList s on g.OperationalStatusId = s.OperationalStatusId

	Open FindSubordinateOperationalStatusGroupIdsCursor
	
	FETCH NEXT FROM FindSubordinateOperationalStatusGroupIdsCursor
	INTO @operationalStatusGroupIdList

	WHILE @@FETCH_STATUS = 0
	BEGIN

		insert into #SubordinateOperationalStatusGroupIdList
		select Value from dbo.SplitFunction( @operationalStatusGroupIdList, ',' )
		where Value not in ( select OperationalStatusGroupId from #SubordinateOperationalStatusGroupIdList )
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error splitting list from subordinate operational status groups from generated group operational statuses.'
			goto ERROREXIT
		END
		

		FETCH NEXT FROM FindSubordinateOperationalStatusGroupIdsCursor
		INTO @operationalStatusGroupIdList

	END

	close FindSubordinateOperationalStatusGroupIdsCursor
	deallocate FindSubordinateOperationalStatusGroupIdsCursor

	-- select subordinate users based on subordinate operational status group ids -> corresponding role Ids -> user roles
	-- prune by allowing only contract creation access points
	select distinct r.CO_ID, r.UserId, u.FirstName, u.LastName, u.FullName, u.Inactive, u.Division
	from SEC_UserProfileUserRoles r join SEC_UserProfile u on r.CO_ID = u.CO_ID
	join #UsersWithContractCreationAccessPoint t on t.CO_ID = u.CO_ID
	where RoleId in ( select RoleId from SEC_RoleOperationalStatusGroups g
				where g.OperationalStatusGroupId in ( select OperationalStatusGroupId from #SubordinateOperationalStatusGroupIdList ))
	and u.Division = @TargetDivision
	and u.CO_ID <> @TargetCOID
		

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error making final selection of subordinates.'
		goto ERROREXIT
	END


goto OKEXIT

ERROREXIT:

	close FindSubordinateOperationalStatusIdsCursor
	deallocate FindSubordinateOperationalStatusIdsCursor

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


