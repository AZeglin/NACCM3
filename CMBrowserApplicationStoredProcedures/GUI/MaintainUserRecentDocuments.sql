IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'MaintainUserRecentDocuments' )
BEGIN
	DROP PROCEDURE MaintainUserRecentDocuments
END
GO

CREATE PROCEDURE MaintainUserRecentDocuments
(
@MostRecentCount int = 20
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@query nvarchar(4000),
		@SQLParms nvarchar(400),
		@CurrentUser uniqueidentifier,
		@DocumentType nchar(1),  -- 'C' = contract or BPA, 'O' = offer
		@DocumentNumber nvarchar(50), -- contract number or offer number
		@DocumentId int, -- contractId or offerId
		@PreferenceKey nvarchar(28)

BEGIN TRANSACTION


	
	create table #TempPreferencesToKeep
	(
		UserPreferenceId int not null
	)

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table during recent documents cleanup.'
		goto ERROREXIT
	END

	Declare UserIdCursor CURSOR For
	Select distinct UserId
	from CM_UserPreferences
	where Removed = 0

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error declaring cursor during recent documents cleanup.'
		goto ERROREXIT
	END

	Open UserIdCursor
	
	FETCH NEXT FROM UserIdCursor
	INTO @CurrentUser

	WHILE @@FETCH_STATUS = 0
	BEGIN

		-- identify recent contracts to keep
		select @query = 'insert into #TempPreferencesToKeep
		( 
			UserPreferenceId
		)
		select top ' + convert( varchar(10), @MostRecentCount ) + ' UserPreferenceId 
		from CM_UserPreferences
		where UserId = @CurrentUser_parm
		and PreferenceType = ''D'' 
		and PreferenceKey = ''RecentContract''
		and Removed = 0
		order by LastModificationDate desc, Ordinality desc '

		select @error = @@error
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error assigning query 1 during recent documents cleanup.'
			goto ERROREXIT
		END

		select @SQLParms = '@CurrentUser_parm uniqueidentifier'

		exec SP_EXECUTESQL @query, @SQLParms, @CurrentUser_parm = @CurrentUser

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting most recent contracts.'
			goto ERROREXIT
		END

		-- identify recent offers to keep
		select @query = 'insert into #TempPreferencesToKeep
		( 
			UserPreferenceId
		)
		select top ' + convert( varchar(10), @MostRecentCount ) + ' UserPreferenceId 
		from CM_UserPreferences
		where UserId = @CurrentUser_parm
		and PreferenceType = ''D'' 
		and PreferenceKey = ''RecentOffer''
		and Removed = 0
		order by LastModificationDate desc, Ordinality desc '

		select @error = @@error
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error assigning query 2 during recent documents cleanup.'
			goto ERROREXIT
		END

		select @SQLParms = '@CurrentUser_parm uniqueidentifier'

		exec SP_EXECUTESQL @query, @SQLParms, @CurrentUser_parm = @CurrentUser

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting most recent offers.'
			goto ERROREXIT
		END


		update CM_UserPreferences
		set Removed = 1
		where UserId = @CurrentUser
		and Removed = 0
		and PreferenceType = 'D'
		and UserPreferenceId not in ( select UserPreferenceId from #TempPreferencesToKeep )

		select @error = @@error
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error updating removed status during recent documents cleanup.'
			goto ERROREXIT
		END

		delete #TempPreferencesToKeep

		select @error = @@error
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error clearing temp table during recent documents cleanup.'
			goto ERROREXIT
		END

		FETCH NEXT FROM UserIdCursor
		INTO @CurrentUser
	
	END
	
	Close UserIdCursor
	DeAllocate UserIdCursor


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


