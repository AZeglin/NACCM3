IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ExpiringDrugItemPricesReport')
	BEGIN
		DROP  Procedure  ExpiringDrugItemPricesReport
	END

GO

CREATE Procedure ExpiringDrugItemPricesReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@DaysToExpire int,  /* 30, 60 or 90 days or -1 for all to include all TPRs */
@PriceTypesToInclude nchar(1),  /* T = TPRs only; N = non-TPRs only; B = Both */
@IncludeExpiringWithContract nchar(1), /* T = include prices with stop date = cntrct exp, F = do not include */
@ContractNumber nvarchar(20) = null,  /* null or blank = all */
@COID int = -1, /* -1 = all */
@Division int = -1 /* -1 = all */
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@SQL nvarchar(3800),
	@SQLParms nvarchar(1000),
	@whereClausePrefix nvarchar(1300),
	@whereDivision nvarchar(120),
	@whereContract nvarchar(120),
	@whereContractingOfficer nvarchar(120),
	@orderByClause nvarchar(120),
	@wherePriceType nvarchar(120),
	@whereStopDate nvarchar(120),
	@dateWithoutTime datetime,
	@futureExpirationDate datetime


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertDrugItemUserActivity @ReportUserLoginId, 'R', 'Expiring Drug Item Prices Report', '2'

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @dateWithoutTime = convert( datetime, convert( nvarchar(2), DatePart( month, getdate() )) + '/' + convert( nvarchar(2), DatePart( day, getdate() )) + '/' + convert( nvarchar(4), DatePart( year, getdate() )))

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error getting date without time ' 
		goto ERROREXIT
	END
	
	if @DaysToExpire <> -1
	BEGIN
		select @futureExpirationDate = DATEADD( day, @DaysToExpire, @dateWithoutTime )
	END

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error assigning futureExpirationDate' 
		goto ERROREXIT
	END
	
	select @SQL = '	select	c.ContractNumber,
			b.Schedule_Number,
			b.CO_ID,
			b.Dates_CntrctExp as ContractExpirationDate,
			b.Dates_Completion as ContractCompletionDate,
			t.Schedule_Name,
			u.FullName,
			u.LastName,
			i.DrugItemId, 
			i.DrugItemNDCId,
			n.FdaAssignedLabelerCode,    
			n.ProductCode,       
			n.PackageCode,          
			i.PackageDescription  ,        
			i.Generic ,   
			i.TradeName ,     
			i.DiscontinuationDate,                             	             
			i.DiscontinuationEnteredDate,
			i.Covered,         
			i.PrimeVendor,
			case dbo.GetItemDualPriceStatusForDrugItemId( i.DrugItemId ) when 1 then ''T'' else ''F'' end as DualPriceDesignation,     
			i.CreatedBy as ItemCreatedBy ,     
			i.CreationDate as ItemCreationDate,         
			i.LastModifiedBy as ItemLastModifiedBy,    
			i.LastModificationDate as ItemLastModificationDate,
			p.DrugItemPriceId ,
			p.DrugItemId,             
			p.PriceId,              
			p.PriceStartDate,         
			p.PriceStopDate as PriceEndDate,       
			p.Price,      
			p.IsTemporary,                                     	                  
			dbo.GetPriceApplicabilityStringForReportFunction( p.DrugItemPriceId, ''A'' ) as PriceApplicabilityString,
			p.LastModificationType,
			p.ModificationStatusId,                  
			p.CreatedBy,     
			p.CreationDate,        
			p.LastModifiedBy,        
			p.LastModificationDate,
			p.DrugItemSubItemId,
			s.SubItemIdentifier
		from DI_DrugItemPrice p join DI_DrugItems i
			on p.DrugItemId = i.DrugItemId
		left outer join DI_DrugItemSubItems s
			on i.DrugItemId = s.DrugItemId
			and p.DrugItemSubItemId = s.DrugItemSubItemId
		join DI_DrugItemNDC n on n.DrugItemNDCId = i.DrugItemNDCId
		join DI_Contracts c on i.ContractId = c.ContractId	
		join [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].dbo.tbl_Cntrcts b on b.CntrctNum = c.NACCMContractNumber
		join [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].dbo.[tlkup_Sched/Cat] t on t.Schedule_Number = b.Schedule_Number
		join [' + @SecurityServerName + '].[' + @SecurityDatabaseName + '].dbo.[SEC_UserProfile] u on u.CO_ID = b.CO_ID '
		

		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error assigning @SQL statement 1' 
			goto ERROREXIT
		END

		if @DaysToExpire = -1
		BEGIN
			select @whereClausePrefix = 'where @dateWithoutTime_parm between p.PriceStartDate and p.PriceStopDate 
			and ( i.DiscontinuationDate is null or i.DiscontinuationDate > @dateWithoutTime_parm ) '	
		END
		else
		BEGIN		
			select @whereClausePrefix = 'where @dateWithoutTime_parm between p.PriceStartDate and p.PriceStopDate
			and p.PriceStopDate <= @futureExpirationDate_parm
			and ( i.DiscontinuationDate is null or i.DiscontinuationDate > @futureExpirationDate_parm ) '
		END

		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error assigning @whereClausePrefix' 
			goto ERROREXIT
		END
	
		select @orderByClause = ' order by p.PriceStopDate asc '

		if @PriceTypesToInclude = 'T'
		BEGIN
			select @wherePriceType = ' and p.IsTemporary = 1 '
		END
		else if @PriceTypesToInclude = 'N'
		BEGIN
			select @wherePriceType = ' and p.IsTemporary = 0 '
		END
		else
		BEGIN
			select @wherePriceType = ' '
		END

		select @whereContractingOfficer = ''
		select @whereDivision = ''
		select @whereStopDate = ''

		if @ContractNumber is null or LEN( @ContractNumber ) = 0
		BEGIN
			select @whereContract = ' and dbo.IsContractActiveFunction( c.NACCMContractNumber, getdate() ) = 1 '
			
			if @COID = -1
			BEGIN
				select @whereContractingOfficer = ' '
				
				if @Division = -1
				BEGIN
					select @whereDivision = ' '
				END
				else
				BEGIN
					select @whereDivision = ' and t.Division = ' + convert( nvarchar( 10 ), @Division )
				END
				
			END
			else
			BEGIN
				select @whereContractingOfficer = ' and b.CO_ID = ' + convert( nvarchar( 10 ), @COID )
			END
			
		END
		else
		BEGIN
			select @whereContract = ' and c.NACCMContractNumber = ''' + @ContractNumber + ''''
		END

		if @IncludeExpiringWithContract = 'F'
		BEGIN
			select @whereStopDate = ' and p.PriceStopDate <> b.Dates_CntrctExp '
		END

		select @SQL = @SQL + @whereClausePrefix + @wherePriceType + @whereContract + @whereContractingOfficer + @whereDivision + @whereStopDate + @orderByClause
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error assigning @SQL statement 2' 
			goto ERROREXIT
		END
			
		select @SQLParms = '@futureExpirationDate_parm datetime, @dateWithoutTime_parm datetime'
		
		Exec SP_executeSQL @SQL, @SQLParms, @futureExpirationDate_parm = @futureExpirationDate, @dateWithoutTime_parm = @dateWithoutTime
	
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving expiring drug item prices'
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
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 



	