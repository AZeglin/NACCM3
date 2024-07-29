IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractHeaders')
	BEGIN
		DROP  Procedure  SelectContractHeaders
	END

GO

CREATE Procedure SelectContractHeaders
(
@CurrentUser uniqueidentifier,
@COID int,
@LoginId nvarchar(120),
@ContractStatusFilter nchar(1),  /* A - All, T - Active, C - Closed, N - none */
@ContractOwnerFilter nchar(1), /* A - All, M - Mine */
@FilterType nchar(1), /* N - Number, O - CO Name, V - Vendor, D - Description, S - Schedule, X = none */
@FilterValue nvarchar(200),
@SortExpression nvarchar(100),
@SortDirection nvarchar(20)
)
    
AS

Declare @errorMsg nvarchar(1300),
		@error int,
		@rowcount int,
		@CurrentDateWithoutTime datetime,
		@query nvarchar(1200),
		@sqlParms nvarchar(800),
		@tempQuery nvarchar(1200),
		@whereClause nvarchar(600),
		@tempWhereClause nvarchar(600),
		@whereStatement nvarchar(60),
		@filterValueClause nvarchar(350),
		@orderByClause nvarchar(280)
	
BEGIN TRANSACTION

	select @CurrentDateWithoutTime = CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME )

	-- list of active contracts used in subsequent queries
	create table #ActiveContractNumbers
	(
		ActiveContractNumber nvarchar(50)
	)

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table 1 during contract select.'
		goto ERROREXIT
	END
	
	insert into #ActiveContractNumbers
	( ActiveContractNumber )
	select c.CntrctNum 
	from tbl_Cntrcts c
	where (( c.Dates_CntrctExp >= @CurrentDateWithoutTime and c.Dates_Completion is null ) or
		( c.Dates_Completion is not null and c.Dates_CntrctExp >= @CurrentDateWithoutTime and c.Dates_Completion >= @CurrentDateWithoutTime )) 

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting active contracts during contract select.'
		goto ERROREXIT
	END
	
	/* express select of no contracts for initial screen for performance */
	if @ContractStatusFilter = 'N'
	BEGIN

		SELECT distinct c.Contract_Record_ID, 
				c.CntrctNum, 
				s.Schedule_Name, 
				s.Schedule_Number, 
				u.FullName AS CO_Name, 
				c.CO_ID, 
				c.Contractor_Name, 
				v.SAMUEI,
				c.DUNS, 
				c.Drug_Covered, 
				c.Dates_CntrctAward, 
				c.Dates_Effective, 
				c.Dates_CntrctExp, 
				c.Dates_Completion, 
				'No' as HasBPA,
				c.BPA_FSS_Counterpart,
				c.Offer_ID

		FROM tbl_Cntrcts c join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
			join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number
			left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId
		where c.CO_ID = -1
	
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error selecting contracts for status = N.'
			goto ERROREXIT
		END

		goto OKEXIT

	END

	create table #ContractSelectBPAList
	(
		ParentCntrctNum nvarchar(50),
		BPACntrctNum nvarchar(50),
		BPAAwardDate datetime,
		BPAEffectiveDate datetime,
		BPAExpirationDate datetime,
		BPACompletionDate datetime,
		IsBPAActive bit
	)

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table 2 during contract select.'
		goto ERROREXIT
	END
	

	if @contractStatusFilter = 'T'
	BEGIN
		insert into #ContractSelectBPAList
			( ParentCntrctNum, BPACntrctNum, BPAAwardDate, BPAEffectiveDate, 
				BPAExpirationDate, BPACompletionDate, IsBPAActive )
			select c.BPA_FSS_Counterpart,
				c.CntrctNum,
				c.Dates_CntrctAward,
				c.Dates_Effective,
				c.Dates_CntrctExp,
				c.Dates_Completion,
				1
			from tbl_Cntrcts c
			where BPA_FSS_Counterpart is not null 
			and c.BPA_FSS_Counterpart in ( select ActiveContractNumber from #ActiveContractNumbers )
			and c.CntrctNum in ( select ActiveContractNumber from #ActiveContractNumbers )
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error selecting BPA contracts during contract select.'
			goto ERROREXIT
		END		
			
	END
	else if @contractStatusFilter = 'C'
	BEGIN
		insert into #ContractSelectBPAList
			( ParentCntrctNum, BPACntrctNum, BPAAwardDate, BPAEffectiveDate, 
				BPAExpirationDate, BPACompletionDate, IsBPAActive )
			select c.BPA_FSS_Counterpart,
				c.CntrctNum,
				c.Dates_CntrctAward,
				c.Dates_Effective,
				c.Dates_CntrctExp,
				c.Dates_Completion,
				0 -- placeholder value to potentially be updated
			from tbl_Cntrcts c
			where BPA_FSS_Counterpart is not null 
			and c.BPA_FSS_Counterpart not in ( select ActiveContractNumber from #ActiveContractNumbers )
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error selecting BPA contracts during contract select.'
			goto ERROREXIT
		END		
		
		update #ContractSelectBPAList
		set IsBPAActive = 1
		where BPACntrctNum in ( select ActiveContractNumber from #ActiveContractNumbers )
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error updating BPA contract status during contract select.'
			goto ERROREXIT
		END		
	END
	else
	BEGIN
		insert into #ContractSelectBPAList
			( ParentCntrctNum, BPACntrctNum, BPAAwardDate, BPAEffectiveDate, 
				BPAExpirationDate, BPACompletionDate, IsBPAActive )
			select c.BPA_FSS_Counterpart,
				c.CntrctNum,
				c.Dates_CntrctAward,
				c.Dates_Effective,
				c.Dates_CntrctExp,
				c.Dates_Completion,
				0 -- placeholder value to potentially be updated
			from tbl_Cntrcts c
			where BPA_FSS_Counterpart is not null 	
			
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error selecting BPA contracts during contract select.'
			goto ERROREXIT
		END		
		
		update #ContractSelectBPAList
		set IsBPAActive = 1
		where BPACntrctNum in ( select ActiveContractNumber from #ActiveContractNumbers )
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error updating BPA contract status during contract select.'
			goto ERROREXIT
		END					
	END
	
	
	if @contractStatusFilter = 'T'
	BEGIN
		select @whereClause = ' c.CntrctNum in ( select ActiveContractNumber from #ActiveContractNumbers ) '
	END
	else if @contractStatusFilter = 'C'
	BEGIN
		select @whereClause = ' c.CntrctNum not in ( select ActiveContractNumber from #ActiveContractNumbers ) '
	END
	else if @contractStatusFilter = 'N' -- none! (now handled earlier)
	BEGIN
		select @whereClause = ' c.CntrctNum <> c.CntrctNum '
	END
	else
	BEGIN
		select @whereClause = ''
	END
	
	
	if @contractOwnerFilter = 'M'
	BEGIN
		if LEN( LTRIM( RTRIM( @whereClause ))) > 0
		BEGIN
			select @whereClause = @whereClause + ' AND '
		END
	
		select @whereClause = @whereClause + ' c.CO_ID = ' + convert( nvarchar(10), @COID )
	
	END

	if LEN( LTRIM( RTRIM( @FilterValue ))) > 0
	BEGIN
	
		select @filterValueClause = '%' + LTRIM( RTRIM( @FilterValue )) + '%'
	
		if @FilterType <> 'X'
		BEGIN
			if LEN( LTRIM( RTRIM( @whereClause ))) > 0
			BEGIN
				select @whereClause = @whereClause + ' AND '
			END
	
			if @FilterType = 'N'
			BEGIN
				select @whereClause = @whereClause + ' c.CntrctNum like ''' + @filterValueClause + ''''
			END
			else if @FilterType = 'O'
			BEGIN
				select @whereClause = @whereClause + ' u.FullName like ''' + @filterValueClause + ''''
			END
			else if @FilterType = 'V'
			BEGIN
				select @whereClause = @whereClause + ' c.Contractor_Name like ''' + @filterValueClause + ''''
			END
			else if @FilterType = 'D'
			BEGIN
				select @whereClause = @whereClause + ' c.Drug_Covered like ''' + @filterValueClause + ''''
			END
			else if @FilterType = 'S'
			BEGIN
				select @whereClause = @whereClause + ' s.Schedule_Name like ''' + @filterValueClause + ''''
			END

		END

	END

	if LEN( RTRIM( LTRIM ( @whereClause ))) > 0
	BEGIN
		select @whereStatement = ' WHERE '
	END
	else
	BEGIN
		select @whereStatement = ''
	END

	if RTRIM( LTRIM( @SortExpression )) = 'Schedule_Name'
	BEGIN
		select @orderByClause = ' ORDER BY s.Schedule_Name '
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'CO_Name'
	BEGIN
		select @orderByClause = ' ORDER BY CO_Name '	
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'Contractor_Name'
	BEGIN
		select @orderByClause = ' ORDER BY c.Contractor_Name '	
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'Drug_Covered'
	BEGIN
		select @orderByClause = ' ORDER BY c.Drug_Covered '	
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'Dates_CntrctAward'
	BEGIN
		select @orderByClause = ' ORDER BY c.Dates_CntrctAward '	
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'Dates_CntrctExp'
	BEGIN
		select @orderByClause = ' ORDER BY c.Dates_CntrctExp '	
	END	
	else
	BEGIN
		select @orderByClause = ' ORDER BY c.CntrctNum'		
	END
	
	if RTRIM( LTRIM( @SortDirection )) = 'Descending'
	BEGIN
		select @orderByClause = @orderByClause + ' DESC '
	END
	else
	BEGIN
		select @orderByClause = @orderByClause + ' ASC '
	END

	select @query = 'SELECT distinct c.Contract_Record_ID, 
			c.CntrctNum, 
			s.Schedule_Name, 
			s.Schedule_Number, 
			u.FullName AS CO_Name, 
			c.CO_ID, 
			c.Contractor_Name, 
			v.SAMUEI,
			c.DUNS, 
			c.Drug_Covered, 
			c.Dates_CntrctAward, 
			c.Dates_Effective, 
			c.Dates_CntrctExp, 
			c.Dates_Completion, 
			case when ( b.ParentCntrctNum is not null  ) then ''Yes'' else ''No'' end as HasBPA,
			c.BPA_FSS_Counterpart,
			c.Offer_ID

		FROM tbl_Cntrcts c join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
			join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number
			left outer join #ContractSelectBPAList b on b.ParentCntrctNum = c.CntrctNum 
			left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId '

		+ @whereStatement + @whereClause + @orderByClause
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2' 
		goto ERROREXIT
	END
		
	exec SP_EXECUTESQL @query
	

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting contracts for status = ' + @ContractStatusFilter + ' owner = ' + @ContractOwnerFilter + ' type = ' + @FilterType + ' value = ' + @FilterValue
		goto ERROREXIT
	END
		
        
GOTO OKEXIT

ERROREXIT:

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

