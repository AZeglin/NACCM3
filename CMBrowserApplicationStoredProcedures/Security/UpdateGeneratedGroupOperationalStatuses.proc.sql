IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateGeneratedGroupOperationalStatuses' )
BEGIN
	DROP PROCEDURE UpdateGeneratedGroupOperationalStatuses
END
GO

CREATE PROCEDURE UpdateGeneratedGroupOperationalStatuses
(
@UserLogin nvarchar(120)
)
AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250),
	@operationalStatusGroupId int,
	@operationalStatusList nvarchar(240),
	@operationalStatusGroupIdList nvarchar(240)
	
BEGIN TRANSACTION
	
	delete SEC_GeneratedGroupOperationalStatuses

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error clearing Generated Group Operational Statuses table'
		goto ERROREXIT
	END
	
	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#OperationalStatusList' ) 
	BEGIN
		DROP TABLE #OperationalStatusList
	END
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error dropping #OperationalStatusList temp table'
		goto ERROREXIT
	END
	
	create table #OperationalStatusList
	(
		OperationalStatusId int NOT NULL
	)
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #OperationalStatusList temp table'
		goto ERROREXIT
	END
	
	Declare GenerateGroupOperationalStatusesCursor CURSOR For
	Select OperationalStatusGroupId, OperationalStatusIdList
	from SEC_OperationalStatusGroups

	Open GenerateGroupOperationalStatusesCursor
	
	FETCH NEXT FROM GenerateGroupOperationalStatusesCursor
	INTO @operationalStatusGroupId, @operationalStatusList

	WHILE @@FETCH_STATUS = 0
	BEGIN

		delete #OperationalStatusList

		insert into #OperationalStatusList
		select Value from dbo.SplitFunction( @operationalStatusList, ',' )
		
		select @error = @@error, @rowCount = @@rowcount
		
		if @error <> 0 or @rowCount = 0
		BEGIN
			select @errorMsg = 'Error splitting list for group id= ' + convert( nvarchar(10), @operationalStatusGroupId )
			goto ERROREXIT
		END

		select @rowCount = count(*) from #OperationalStatusList
		
		while @rowCount > 0
		BEGIN

			if exists ( select * from SEC_GeneratedGroupOperationalStatuses 
						where OperationalStatusId = ( select top 1 OperationalStatusId from #OperationalStatusList ))
			BEGIN
				select @operationalStatusGroupIdList = OperationalStatusGroupIdList 
				from SEC_GeneratedGroupOperationalStatuses
				where OperationalStatusId = ( select top 1 OperationalStatusId from #OperationalStatusList )
				
				select @operationalStatusGroupIdList = @operationalStatusGroupIdList + ',' + convert( nvarchar(10), @operationalStatusGroupId )
				
				update SEC_GeneratedGroupOperationalStatuses
				set OperationalStatusGroupIdList = @operationalStatusGroupIdList,
				LastModifiedBy = @UserLogin, 
				LastModificationDate = GETDATE()
				where OperationalStatusId = ( select top 1 OperationalStatusId from #OperationalStatusList )
			END
			else
			BEGIN
				insert into SEC_GeneratedGroupOperationalStatuses
				( OperationalStatusId, OperationalStatusGroupIdList, LastModifiedBy, LastModificationDate )
				select top 1 OperationalStatusId, convert( nvarchar(4), @operationalStatusGroupId ), @UserLogin, GETDATE() from #OperationalStatusList
			
			END

			delete #OperationalStatusList
			where OperationalStatusId = ( select top 1 OperationalStatusId from #OperationalStatusList )

			select @rowCount = count(*) from #OperationalStatusList

		END
		
		FETCH NEXT FROM GenerateGroupOperationalStatusesCursor
		INTO @operationalStatusGroupId, @operationalStatusList
	
	END
	
	Close GenerateGroupOperationalStatusesCursor
	DeAllocate GenerateGroupOperationalStatusesCursor

GOTO OKEXIT

ERROREXIT:

	Close GenerateGroupOperationalStatusesCursor
	DeAllocate GenerateGroupOperationalStatusesCursor

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




