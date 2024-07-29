IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractingOfficers3' )
BEGIN
	DROP PROCEDURE SelectContractingOfficers3
END
GO

CREATE PROCEDURE SelectContractingOfficers3
(
@DivisionId as int = -1,   -- -1 = all ( used for reports )
@SelectFlag as int = 0,   -- -1 = include initial value of '--select--';  -2 = include initial value of 'All' for reports;  0 = dont add any extra values
@OrderByLastNameFullName as nchar(1) = 'L', -- 'L' = order by last name ( use when select flag is not 0 ); 'F' = order by full name
@IsExpired as bit = 0  -- expired contracts may reference non-active personnel
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@literalFullName nvarchar(50),
		@query nvarchar(1600),
		@orderBy nvarchar(120),
		@union nvarchar(200),
		@operationalStatusGroupIdList nvarchar(240),
		@whereInactive nvarchar(220)


BEGIN TRANSACTION
	
	-- determine users in a particular division
	create table #OperationalStatusGroupIdLists
	(
		OperationalStatusGroupIdList nvarchar(240) NOT NULL
	)

	if @DivisionId = -1
	BEGIN

		insert into #OperationalStatusGroupIdLists
		( OperationalStatusGroupIdList )
		select OperationalStatusGroupIdList
		from SEC_GeneratedGroupOperationalStatuses
		where OperationalStatusId in (
									select OperationalStatusId 
									from SEC_OperationalStatuses
									where Inactive = 0
									and Division > 0 -- excludes admin
									)								
	END
	else
	BEGIN

		insert into #OperationalStatusGroupIdLists
		( OperationalStatusGroupIdList )
		select OperationalStatusGroupIdList
		from SEC_GeneratedGroupOperationalStatuses
		where OperationalStatusId in (
									select OperationalStatusId 
									from SEC_OperationalStatuses
									where Inactive = 0
									and Division = @DivisionId
									)

	END

	create table #OperationalStatusGroupIds
	(
		OperationalStatusGroupId int NOT NULL
	)


	Declare FlattenOutGroupIdsCursor CURSOR For
	Select OperationalStatusGroupIdList
	from #OperationalStatusGroupIdLists

	Open FlattenOutGroupIdsCursor
	
	FETCH NEXT FROM FlattenOutGroupIdsCursor
	INTO @operationalStatusGroupIdList

	WHILE @@FETCH_STATUS = 0
	BEGIN

		insert into #OperationalStatusGroupIds
		select Value from dbo.SplitFunction( @operationalStatusGroupIdList, ',' )
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error splitting list for operational status group.'
			goto ERROREXIT
		END
		
		FETCH NEXT FROM FlattenOutGroupIdsCursor
		INTO @operationalStatusGroupIdList
	
	END
	
	Close FlattenOutGroupIdsCursor
	DeAllocate FlattenOutGroupIdsCursor

	create table #UsersInDivision
	(
		CO_ID int not null,
		Inactive bit not null
	)

	insert into #UsersInDivision
	( CO_ID, Inactive )
	select distinct u.CO_ID, u.Inactive
	from SEC_UserProfile u join SEC_UserProfileUserRoles r on r.CO_ID = u.CO_ID
		join SEC_RoleOperationalStatusGroups s on s.RoleId = r.RoleId
	where s.OperationalStatusGroupId in ( select distinct OperationalStatusGroupId from #OperationalStatusGroupIds )
	

	select @error = @@error
		
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting users in division.'
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


	select @query = ' select distinct u.CO_ID, u.FirstName, u.LastName, u.FullName, u.UserName, u.User_Email, u.User_Phone, u.UserId
		from SEC_UserProfile u join #UsersInDivision d on d.CO_ID = u.CO_ID
		join #UsersWithContractCreationAccessPoint a on a.CO_ID = d.CO_ID '

		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error assigning query string.'
			goto ERROREXIT
		END

	if @IsExpired = 1
	BEGIN
		select @whereInactive = ' '
	END
	else
	BEGIN
		select @whereInactive = ' where d.Inactive = 0 and a.Inactive = 0 '
	END

	if @SelectFlag = -1 or @SelectFlag = -2
	BEGIN
	
		if @SelectFlag = -1
		BEGIN
			select @literalFullName = '--select--'
		END
		else
		BEGIN
			select @literalFullName = 'All'	
		END

		select @union = 'union			
			select -1 as CO_ID, '''' as FirstName, '''' as LastName,  ''' + @literalFullName + ''' as FullName, '''' as UserName, '''' as User_Email, '''' as User_Phone, ''00000000-0000-0000-0000-000000000000'' as UserId	'		
	END
	else
	BEGIN
		select @union = ' '
	END

	if @OrderByLastNameFullName = 'F'
	BEGIN
		select @orderBy = ' order by u.FullName '

		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error assigning order by string.'
			goto ERROREXIT
		END
	END
	else
	BEGIN
		select @orderBy = ' order by u.LastName '

		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error assigning order by string.'
			goto ERROREXIT
		END
	END	
	
	select @query = @query + @whereInactive + @union + @orderBy

	select @error = @@error
		
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error assigning query string 2.'
		goto ERROREXIT
	END

	exec SP_EXECUTESQL @query

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting contracting officers.'
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


