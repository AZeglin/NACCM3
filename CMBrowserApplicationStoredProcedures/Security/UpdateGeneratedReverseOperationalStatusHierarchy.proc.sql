IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateGeneratedReverseOperationalStatusHierarchy' )
BEGIN
	DROP PROCEDURE UpdateGeneratedReverseOperationalStatusHierarchy
END
GO

CREATE PROCEDURE UpdateGeneratedReverseOperationalStatusHierarchy
(
@UserLogin nvarchar(120)
)
AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250),
	@operationalStatusId int,
	@allowedByOperationalStatusId int,
	@allowedOperationalStatusIdList nvarchar(240),
	@isAllowedByOperationalStatusIdList nvarchar(240)
	
	
	/*  This SP should be manually run whenever the manually updated  SEC_OperationalStatusHierarchy is changed. This SP
		basically populates the SEC_GeneratedReverseOperationalStatusHierarchy table to be used as a reverse lookup.  The table is 
		used when determining the list of subordinates of a particular user based on the user's ability to modify another user's
		documents as specified in the hierarchy table. This algorithm presumes that if a user has the ability to modify another
		users document, then that user may be considered as higher up on the management tree.  The only exclusion to this
		is if the OSG is listed as allowed by itself.
	*/

BEGIN TRANSACTION
	
	delete SEC_GeneratedReverseOperationalStatusHierarchy

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error clearing Generated Reverse Operational Status Hierarchy table'
		goto ERROREXIT
	END
	
	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#ReverseOperationalStatusIdList' ) 
	BEGIN
		DROP TABLE #ReverseOperationalStatusIdList
	END
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error dropping #ReverseOperationalStatusIdList temp table'
		goto ERROREXIT
	END
	
	create table #ReverseOperationalStatusIdList
	(
		OperationalStatusId int NOT NULL,
		AllowedByOperationalStatusId int NOT NULL
	)
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #ReverseOperationalStatusIdList temp table'
		goto ERROREXIT
	END
	
	Declare GenerateReverseOperationalStatusListCursor CURSOR For
	Select OperationalStatusId, AllowedOperationalStatusIdList
	from SEC_OperationalStatusHierarchy

	Open GenerateReverseOperationalStatusListCursor
	
	FETCH NEXT FROM GenerateReverseOperationalStatusListCursor
	INTO @operationalStatusId, @allowedOperationalStatusIdList

	WHILE @@FETCH_STATUS = 0
	BEGIN

		delete #ReverseOperationalStatusIdList

		insert into #ReverseOperationalStatusIdList
		( OperationalStatusId, AllowedByOperationalStatusId )
		select Value, @operationalStatusId 
		from dbo.SplitFunction( @allowedOperationalStatusIdList, ',' )
		where Value <> @operationalStatusId
		
		select @error = @@error, @rowCount = @@rowcount
		
		if @error <> 0 or @rowCount = 0
		BEGIN
			select @errorMsg = 'Error splitting list for id= ' + convert( nvarchar(10), @operationalStatusId )
			goto ERROREXIT
		END

		select @rowCount = count(*) from #ReverseOperationalStatusIdList
		
		while @rowCount > 0
		BEGIN

			if exists ( select * from SEC_GeneratedReverseOperationalStatusHierarchy 
						where OperationalStatusId = ( select top 1 OperationalStatusId from #ReverseOperationalStatusIdList ))
			BEGIN
				select top 1 @operationalStatusId = OperationalStatusId,
					@allowedByOperationalStatusId = AllowedByOperationalStatusId 
				from #ReverseOperationalStatusIdList

				select @isAllowedByOperationalStatusIdList = IsAllowedByOperationalStatusIdList 
				from SEC_GeneratedReverseOperationalStatusHierarchy
				where OperationalStatusId = @operationalStatusId
				
				select @isAllowedByOperationalStatusIdList = @isAllowedByOperationalStatusIdList + ',' + convert( nvarchar(10), @allowedByOperationalStatusId )
				
				update SEC_GeneratedReverseOperationalStatusHierarchy
				set IsAllowedByOperationalStatusIdList = @isAllowedByOperationalStatusIdList,
				LastModifiedBy = @UserLogin, 
				LastModificationDate = GETDATE()
				where OperationalStatusId = @operationalStatusId

			END
			else
			BEGIN
				select top 1 @operationalStatusId = OperationalStatusId,
					@allowedByOperationalStatusId = AllowedByOperationalStatusId 
				from #ReverseOperationalStatusIdList

				insert into SEC_GeneratedReverseOperationalStatusHierarchy
				( OperationalStatusId, IsAllowedByOperationalStatusIdList, LastModifiedBy, LastModificationDate )
				select @operationalStatusId, convert( nvarchar(4), @allowedByOperationalStatusId ), @UserLogin, GETDATE() 
			
			END

			delete #ReverseOperationalStatusIdList
			where OperationalStatusId = @operationalStatusId

			select @rowCount = count(*) from #ReverseOperationalStatusIdList

		END
		
	FETCH NEXT FROM GenerateReverseOperationalStatusListCursor
	INTO @operationalStatusId, @allowedOperationalStatusIdList
	
	END
	
	Close GenerateReverseOperationalStatusListCursor
	DeAllocate GenerateReverseOperationalStatusListCursor

GOTO OKEXIT

ERROREXIT:

	Close GenerateReverseOperationalStatusListCursor
	DeAllocate GenerateReverseOperationalStatusListCursor

	raiserror( @errorMsg, 16 , 1 )
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
		/* only rollback iff this the highest level */
		ROLLBACK TRANSACTION
	END
	
	RETURN ( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN ( 0 )




