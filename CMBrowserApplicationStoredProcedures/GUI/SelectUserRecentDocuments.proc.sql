IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectUserRecentDocuments' )
BEGIN
	DROP PROCEDURE SelectUserRecentDocuments
END
GO

CREATE PROCEDURE SelectUserRecentDocuments
(
@CurrentUser uniqueidentifier,
@MostRecentCount int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@query nvarchar(4000),
		@SQLParms nvarchar(400)


BEGIN TRANSACTION

	-- list of active contracts used in subsequent queries
	create table #RecentContractNumbers
	(
		UserPreferenceId int, 
		Ordinality int, 
		ActiveContractNumber nvarchar(50),
		ActiveContractId int,
		LastModificationDate datetime
	)

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table 1 during contract select.'
		goto ERROREXIT
	END

	-- list of active offers used in subsequent queries
	create table #RecentOfferNumbers
	(
		UserPreferenceId int, 
		Ordinality int, 
		ActiveOfferNumber nvarchar(50),
		ActiveOfferId int,
		LastModificationDate datetime
	)

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table 2 during contract select.'
		goto ERROREXIT
	END

	-- 'D' indicates recent document
	select @query = 'insert into #RecentContractNumbers
	( UserPreferenceId, Ordinality, ActiveContractNumber, ActiveContractId, LastModificationDate )
	select top ' + CONVERT( nvarchar(10), @MostRecentCount ) +
	' UserPreferenceId, Ordinality, PreferenceStringValue, PreferenceNumericValue, LastModificationDate
	from CM_UserPreferences
	where UserId = @CurrentUser_parm
	and PreferenceType = ''D'' 
	and PreferenceKey = ''RecentContract''
	and Removed = 0
	order by LastModificationDate desc '

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string when selecting most recent contracts.'
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

	select @query = 'insert into #RecentOfferNumbers
	( UserPreferenceId, Ordinality, ActiveOfferNumber, ActiveOfferId, LastModificationDate )
	select top ' + CONVERT( nvarchar(10), @MostRecentCount ) +
	' UserPreferenceId, Ordinality, case when ( PreferenceStringValue is null or LEN( PreferenceStringValue ) = 0 ) then ''Number Not Provided'' else PreferenceStringValue end as PreferenceStringValue, PreferenceNumericValue, LastModificationDate
	from CM_UserPreferences
	where UserId = @CurrentUser_parm
	and PreferenceType = ''D'' 
	and PreferenceKey = ''RecentOffer''
	and Removed = 0
	order by LastModificationDate desc '

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string when selecting most recent offers.'
		goto ERROREXIT
	END

	exec SP_EXECUTESQL @query, @SQLParms, @CurrentUser_parm = @CurrentUser

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting most recent offers.'
		goto ERROREXIT
	END

	select @query = 'select top ' +  CONVERT(nvarchar(10), @MostRecentCount ) +  ' UserPreferenceId, Ordinality, DocumentType, DocumentNumber, DocumentId, LastModificationDate, Schedule_Number, Schedule_Name, CO_Name, 
			CO_ID, Contractor_Name, DocumentDate, ActiveStatus, CompletionStatus
	from
	(
	select UserPreferenceId, Ordinality, ''Contract'' as DocumentType, ActiveContractNumber as DocumentNumber, c.Contract_Record_ID as DocumentId, t.LastModificationDate, s.Schedule_Number, s.Schedule_Name, u.FullName AS CO_Name, 
			c.CO_ID, c.Contractor_Name, ''Awarded On '' + CONVERT( nvarchar(10), c.Dates_CntrctAward, 101 ) as DocumentDate, dbo.IsContractActiveFunction( c.CntrctNum, GETDATE() ) as ActiveStatus, case when c.Dates_Completion is null then 0 else 1 end as CompletionStatus
	from #RecentContractNumbers t join tbl_Cntrcts c on t.ActiveContractNumber = c.CntrctNum
	join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
	join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number

	union

	select UserPreferenceId, Ordinality, ''Offer'' as DocumentType, ActiveOfferNumber as DocumentNumber, ActiveOfferId as DocumentId, t.LastModificationDate, o.Schedule_Number, s.Schedule_Name, u.FullName AS CO_Name, 
		o.CO_ID, o.Contractor_Name, ''Received On '' + CONVERT( nvarchar(10), o.Dates_Received, 101 ) as DocumentDate, case when a.Complete = 0 then 1 else 0 end as ActiveStatus, a.Complete as CompletionStatus
	from #RecentOfferNumbers t join tbl_Offers o on t.ActiveOfferId = o.Offer_ID
	join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = o.CO_ID
	join dbo.[tlkup_Sched/Cat] s ON o.Schedule_Number = s.Schedule_Number
	join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
	) a

	order by LastModificationDate desc '

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2.'
		goto ERROREXIT
	END

	exec SP_EXECUTESQL @query

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting most recent documents.'
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


