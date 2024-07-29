IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractHeaders2')
	BEGIN
		DROP  Procedure  SelectContractHeaders2
	END

GO

CREATE Procedure SelectContractHeaders2
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
		@query nvarchar(3900),
		@query1 nvarchar(900),
		@query2 nvarchar(900),
		@query3 nvarchar(900),
		@sqlParms nvarchar(800),
		@tempQuery nvarchar(1200),
		@whereClause nvarchar(600),
		@tempWhereClause nvarchar(600),
		@whereStatement nvarchar(60),
		@filterValueCleansed nvarchar(200),
		@filterValueClause nvarchar(350),
		@orderByClause nvarchar(280)
	
BEGIN TRANSACTION

	select @CurrentDateWithoutTime = CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME )
	select @whereClause = ''
	
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
	
	else 
	BEGIN

		if LEN( LTRIM( RTRIM( @FilterValue ))) > 0 or LEN( RTRIM( LTRIM( @SortExpression ))) > 0
		BEGIN
	
			select @filterValueClause = '%' + LTRIM( RTRIM( @FilterValue )) + '%'

			if @FilterType <> 'X'
			BEGIN
				
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

			select @query1 = 'SELECT distinct c.Contract_Record_ID, 
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
				c.Dates_Completion, '

			select @error = @@error
	
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error assigning query string (1)' 
				goto ERROREXIT
			END
	
			select @query3 = ' c.BPA_FSS_Counterpart,
				c.Offer_ID

			FROM tbl_Cntrcts c join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
				join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number 
				left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId '
			
			select @error = @@error
	
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error assigning query string (3)' 
				goto ERROREXIT
			END

			/* active only */
			if @ContractStatusFilter = 'T'
			BEGIN
				/* for active, want to only indicate 'yes' if the BPA is active as well */
				select @query2 = '	case when ( exists ( select FSSContractNumber from CM_BPALookup b join tbl_Cntrcts x on b.BPAContractNumber = x.CntrctNum 
													where b.FSSContractNumber = c.CntrctNum 
													and (( x.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) and x.Dates_Completion is null ) or
														( x.Dates_Completion is not null and x.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) 
															and x.Dates_Completion >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) ))
														)) then ''Yes'' else ''No'' end as HasBPA, '

				select @error = @@error
	
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error assigning query string (2.1)' 
					goto ERROREXIT
				END

				if LEN( LTRIM( RTRIM( @whereClause ))) > 0
				BEGIN
					select @whereClause = @whereClause + ' AND '
				END

				if @contractOwnerFilter = 'M'
				BEGIN					
					select @whereClause =  @whereClause + ' c.CO_ID = ' + convert( nvarchar(10), @COID ) + ' AND (( c.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) and c.Dates_Completion is null ) or
								( c.Dates_Completion is not null and c.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) and c.Dates_Completion >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) )) '
				END
				else
				BEGIN
					select @whereClause = @whereClause + ' (( c.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) and c.Dates_Completion is null ) or
								( c.Dates_Completion is not null and c.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) and c.Dates_Completion >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) )) '
				END
			END
			/* both closed and active */
			else if @ContractStatusFilter = 'A'
			BEGIN

				select @query2 = 'case when ( exists ( select FSSContractNumber from CM_BPALookup b where b.FSSContractNumber = c.CntrctNum )) then ''Yes'' else ''No'' end as HasBPA, '

				select @error = @@error
	
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error assigning query string (2.2)' 
					goto ERROREXIT
				END

				if @contractOwnerFilter = 'M'
				BEGIN
					if LEN( LTRIM( RTRIM( @whereClause ))) > 0
					BEGIN
						select @whereClause = @whereClause + ' AND '
					END

					select @whereClause =  @whereClause + ' c.CO_ID = ' + convert( nvarchar(10), @COID ) 
				END
				
				/* else any date any owner */
				
			END
			/* closed only */
			else if @ContractStatusFilter = 'C'
			BEGIN
				select @query2 = 'case when ( exists ( select FSSContractNumber from CM_BPALookup b where b.FSSContractNumber = c.CntrctNum )) then ''Yes'' else ''No'' end as HasBPA, '

				select @error = @@error
	
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error assigning query string (2.3)' 
					goto ERROREXIT
				END

				if LEN( LTRIM( RTRIM( @whereClause ))) > 0
				BEGIN
					select @whereClause = @whereClause + ' AND '
				END

				if @contractOwnerFilter = 'M'
				BEGIN
					select @whereClause =  @whereClause + ' c.CO_ID = ' + convert( nvarchar(10), @COID ) + ' AND ( c.Dates_CntrctExp < CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) or c.Dates_Completion < CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME )) '
				END
				else
				BEGIN
					select @whereClause =  @whereClause + ' ( c.Dates_CntrctExp < CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) or c.Dates_Completion < CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME )) '
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

			select @query = @query1 + @query2 + @query3 + @whereStatement + @whereClause + @orderByClause
	
			select @error = @@error
	
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error assigning query string' 
				goto ERROREXIT
			END
		
			exec SP_EXECUTESQL @query
			--select @query
	

			select @error = @@error
	
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error selecting contracts for status = ' + @ContractStatusFilter + ' owner = ' + @ContractOwnerFilter + ' type = ' + @FilterType + ' value = ' + @FilterValue
				goto ERROREXIT
			END
		
		END
		else /* no specific filtering */
		BEGIN
		
			/* active only */
			/* for active, want to only indicate 'yes' if the BPA is active as well */
			if @ContractStatusFilter = 'T'
			BEGIN
				if @contractOwnerFilter = 'M'
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
						case when ( exists ( select FSSContractNumber from CM_BPALookup b join tbl_Cntrcts x on b.BPAContractNumber = x.CntrctNum 
													where b.FSSContractNumber = c.CntrctNum 
													and (( x.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) and x.Dates_Completion is null ) or
														( x.Dates_Completion is not null and x.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) 
															and x.Dates_Completion >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) ))
														)) then 'Yes' else 'No' end as HasBPA,
						c.BPA_FSS_Counterpart,
						c.Offer_ID

					FROM tbl_Cntrcts c join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
						join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number
						left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId
						

					WHERE c.CO_ID = @COID
					AND (( c.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) and c.Dates_Completion is null ) or
							( c.Dates_Completion is not null and c.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) 
								and c.Dates_Completion >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) ))
					ORDER BY c.CntrctNum
				END
				else
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
							case when ( exists ( select FSSContractNumber from CM_BPALookup b join tbl_Cntrcts x on b.BPAContractNumber = x.CntrctNum 
												where b.FSSContractNumber = c.CntrctNum 
												and (( x.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) and x.Dates_Completion is null ) or
													( x.Dates_Completion is not null and x.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) 
														and x.Dates_Completion >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) ))
													)) then 'Yes' else 'No' end as HasBPA,
							c.BPA_FSS_Counterpart,
							c.Offer_ID

						FROM tbl_Cntrcts c join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
							join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number	
							left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId

						WHERE  (( c.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) and c.Dates_Completion is null ) or
								( c.Dates_Completion is not null and c.Dates_CntrctExp >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) 
									and c.Dates_Completion >= CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) ))
						ORDER BY c.CntrctNum
				END
			END
			/* both closed and active */
			else if @ContractStatusFilter = 'A'
			BEGIN
				if @contractOwnerFilter = 'M'
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
						case when ( exists ( select FSSContractNumber from CM_BPALookup b where b.FSSContractNumber = c.CntrctNum )) then 'Yes' else 'No' end as HasBPA,
						c.BPA_FSS_Counterpart,
						c.Offer_ID

					FROM tbl_Cntrcts c join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
						join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number
						left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId

					WHERE c.CO_ID = @COID
					ORDER BY c.CntrctNum
				END
				else
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
							case when ( exists ( select FSSContractNumber from CM_BPALookup b where b.FSSContractNumber = c.CntrctNum )) then 'Yes' else 'No' end as HasBPA,
							c.BPA_FSS_Counterpart,
							c.Offer_ID

						FROM tbl_Cntrcts c join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
							join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number	
							left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId
						ORDER BY c.CntrctNum
				END
			END
			/* closed only */
			else if @ContractStatusFilter = 'C'
			BEGIN
				if @contractOwnerFilter = 'M'
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
						case when ( exists ( select FSSContractNumber from CM_BPALookup b where b.FSSContractNumber = c.CntrctNum )) then 'Yes' else 'No' end as HasBPA,
						c.BPA_FSS_Counterpart,
						c.Offer_ID

					FROM tbl_Cntrcts c join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
						join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number	
						left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId

					WHERE c.CO_ID = @COID
						and ( c.Dates_CntrctExp < CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME )
						or ( c.Dates_Completion is not null  and  c.Dates_Completion < CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) ))
					ORDER BY c.CntrctNum
				END
				else
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
						case when ( exists ( select FSSContractNumber from CM_BPALookup b where b.FSSContractNumber = c.CntrctNum )) then 'Yes' else 'No' end as HasBPA,
						c.BPA_FSS_Counterpart,
						c.Offer_ID

					FROM tbl_Cntrcts c join [NACSEC].[dbo].[SEC_UserProfile] u ON u.CO_ID = c.CO_ID
						join dbo.[tlkup_Sched/Cat] s ON c.Schedule_Number = s.Schedule_Number		
						left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId

					WHERE c.Dates_CntrctExp < CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME )
						or ( c.Dates_Completion is not null  and  c.Dates_Completion < CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME ) )
					ORDER BY c.CntrctNum
				
				END
			END

		END
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

