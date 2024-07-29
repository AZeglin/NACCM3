IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'COProfileReport')
	BEGIN
		DROP  Procedure  COProfileReport
	END

GO

CREATE Procedure COProfileReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@DivisionId int /* may be -1 = all NAC */
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@currentQuarterId int,
		@currentQuarterDescription nvarchar(20),
		@firstDayOfCurrentQuarter datetime,
		@lastDayOfCurrentQuarter datetime,
		@previousQuarterId int,
		@previousQuarterDescription nvarchar(20),
		@query nvarchar(3200),
		@InsertQuery nvarchar(3200),
		@SelectQuery1 nvarchar(3200),
		@SelectQuery2 nvarchar(3200),
		@SQLParms nvarchar(1000),
		@whereDivision nvarchar(100),
		@groupByString nvarchar(400),
		@joinSecurityServerName nvarchar(1000),
		@SERVERNAME nvarchar(255)

	


BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'CO Profile Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @currentQuarterId = y.Quarter_ID
	from tlkup_year_qtr y
	where getdate() between y.Start_Date and y.End_Date

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting current quarter.'
		goto ERROREXIT
	END
	
	select @currentQuarterId = @currentQuarterId - 1 -- to get reporting quarter
	
	select @currentQuarterDescription = y.Title
	from tlkup_year_qtr y
	where y.Quarter_ID = @currentQuarterId

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting current quarter.'
		goto ERROREXIT
	END
	
	
	select @firstDayOfCurrentQuarter = y.Start_Date,
		@lastDayOfCurrentQuarter = y.End_Date
	from tlkup_year_qtr y
	where y.Quarter_ID = @currentQuarterId
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting first day of current quarter.'
		goto ERROREXIT
	END
	
	select @previousQuarterId = @currentQuarterId - 1
	
	select @previousQuarterDescription = y.Title
	from tlkup_year_qtr y
	where y.Quarter_ID = @previousQuarterId
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting previous quarter description.'
		goto ERROREXIT
	END
	
	-- test for SQL1 usage
	SELECT @SERVERNAME = @@SERVERNAME
	
	if @SERVERNAME is null
	BEGIN
		select @joinSecurityServerName = '[' + @SecurityDatabaseName + ']'
	END
	else
	BEGIN
		select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'
	END
	
	create table #ActiveContracts
	(
		CntrctNum nvarchar(50),
		COId int,
		ContractingOfficerName nvarchar(50),
		
		ItemCount int,
		ContractValue money,
		
		PreviousQuarterTotalSales money,
		CurrentQuarterTotalSales money,
		
		ZeroSalesCountFromPreviousQuarters int,
		
		ExpectedSBAPlan bit,
		HasSBAPlan bit,
		MissingSomeContractData bit,
		MissingPricelistVerification bit,
		
		IsActiveNow bit,
		WasActiveDuringCurrentReportingQuarter bit
	)
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table #ActiveContracts for report.'
		goto ERROREXIT
	END
	
	select @InsertQuery = 'insert into #ActiveContracts
	(
		CntrctNum,
		COId,
		ContractingOfficerName,
		ItemCount,
		ContractValue,
		ExpectedSBAPlan,
		HasSBAPlan,
		IsActiveNow,
		WasActiveDuringCurrentReportingQuarter
	) '
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1'
		goto ERROREXIT
	END
		
	select @SelectQuery1 = ' select c.CntrctNum,
		 c.CO_ID, 
		s.FullName,
		( select count(*) from CM_Items i join CM_ItemPrice p on i.ItemId = p.ItemId where i.ContractId = c.Contract_Record_ID and getdate() between p.PriceStartDate and p.PriceStopDate ) as ItemCount,
		c.Estimated_Contract_Value,		
		case when ( c.Schedule_Number not in ( 14, 15, 39, 41, 48, 52 ) and c.Estimated_Contract_Value >= dbo.GetSBAContractValueThresholdFunction( c.Dates_CntrctAward ) and c.SBA_Plan_Exempt = 0 and c.Socio_Business_Size_ID = 2 ) then 1 else 0 end as ExpectedSBAPlan,
		case when ( c.SBAPlanId is not null and exists( select p.SBAPlanId from tbl_sba_SBAPlan p where p.SBAPlanId = c.SBAPlanId ) and c.SBAPlanId <> 511 ) then 1 else 0 end as HasSBAPlan,
		1,
		0	
	from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] s on c.CO_ID = s.CO_ID
		join [tlkup_Sched/Cat] t on c.Schedule_Number = t.Schedule_Number
		where dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 '
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2'
		goto ERROREXIT
	END
	
	
	if @DivisionId <> -1
	BEGIN
		select @whereDivision = ' and t.Division = ' + convert( nvarchar(10), @DivisionId )
	END
	else
	BEGIN
		select @whereDivision = ' and t.Division <> 6 ' -- All NAC excludes SAC 
	END	

	select @query = @InsertQuery + @SelectQuery1 + @whereDivision 
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 3'
		goto ERROREXIT
	END
	
	exec SP_EXECUTESQL @query

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting active contract list for CO Profile Report.'
		goto ERROREXIT
	END

	--  old item count clause for reference	
	--	( select count(*) from tbl_pricelist p where p.CntrctNum = c.CntrctNum and p.Removed = 0 ) as ItemCount,
	
	select @SelectQuery2 = ' select c.CntrctNum,
		 c.CO_ID, 
		s.FullName,
		( select count(*) from CM_Items i join CM_ItemPrice p on i.ItemId = p.ItemId where i.ContractId = c.Contract_Record_ID and getdate() between p.PriceStartDate and p.PriceStopDate ) as ItemCount,

		c.Estimated_Contract_Value,		
		case when ( c.Schedule_Number not in ( 14, 15, 39, 41, 48, 52 ) and c.Estimated_Contract_Value >= dbo.GetSBAContractValueThresholdFunction( c.Dates_CntrctAward ) and c.SBA_Plan_Exempt = 0 and c.Socio_Business_Size_ID = 2 ) then 1 else 0 end as ExpectedSBAPlan,
		case when ( c.SBAPlanId is not null and exists( select p.SBAPlanId from tbl_sba_SBAPlan p where p.SBAPlanId = c.SBAPlanId ) and c.SBAPlanId <> 511 ) then 1 else 0 end as HasSBAPlan,
		0,
		1
	from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] s on c.CO_ID = s.CO_ID
		join [tlkup_Sched/Cat] t on c.Schedule_Number = t.Schedule_Number
		where dbo.IsContractActiveForSalesFunction( c.CntrctNum, @firstDayOfCurrentQuarter_parm, @lastDayOfCurrentQuarter_parm ) = 1 
		and c.CntrctNum not in ( select CntrctNum from #ActiveContracts ) '
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 4'
		goto ERROREXIT
	END
				
	select @SQLParms = '@firstDayOfCurrentQuarter_parm datetime, @lastDayOfCurrentQuarter_parm datetime'				
				
	select @query = @InsertQuery + @SelectQuery2 + @whereDivision 
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 5'
		goto ERROREXIT
	END
	
	exec SP_EXECUTESQL @query, @SQLParms, @firstDayOfCurrentQuarter_parm = @firstDayOfCurrentQuarter, @lastDayOfCurrentQuarter_parm = @lastDayOfCurrentQuarter

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting active ( for reporting quarter ) contract list for CO Profile Report.'
		goto ERROREXIT
	END

	--select * from #ActiveContracts where COId = 336
	
	/* backfill for contracts that were active in both tests */
	update #ActiveContracts 
		set MissingSomeContractData = 1
	from tbl_Cntrcts c join #ActiveContracts a on c.CntrctNum = a.CntrctNum
	where ( c.TIN is null 
		or c.DUNS is null
		or c.Primary_Address_1 is null
		or c.Primary_City is null
		or c.Primary_State is null
		or c.Primary_Zip is null
		or c.POC_VendorWeb is null
		and c.Schedule_Number <> 15 )
	or c.POC_Primary_Name is null
	or c.POC_Primary_Phone is null
	or c.POC_Primary_Email is null
	or c.Estimated_Contract_Value is null
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating MissingSomeContractData for CO Profile Report.'
		goto ERROREXIT
	END

	
	update #ActiveContracts
	set WasActiveDuringCurrentReportingQuarter = 1
	from #ActiveContracts a
	where a.WasActiveDuringCurrentReportingQuarter = 0
	and dbo.IsContractActiveForSalesFunction( a.CntrctNum, @firstDayOfCurrentQuarter, @lastDayOfCurrentQuarter ) = 1 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating active ( for reporting quarter ) contract list for CO Profile Report.'
		goto ERROREXIT
	END
	
	update #ActiveContracts
		set CurrentQuarterTotalSales = ( select sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales)
									from tbl_Cntrcts_Sales s 
									where s.Quarter_ID = @currentQuarterId 
									and s.CntrctNum = a.CntrctNum )
		from #ActiveContracts a								
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating current sales for CO Profile Report.'
		goto ERROREXIT
	END
	
	
	update #ActiveContracts
		set PreviousQuarterTotalSales = ( select sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales)
									from tbl_Cntrcts_Sales s 
									where s.Quarter_ID = @previousQuarterId 
									and s.CntrctNum = a.CntrctNum )
		from #ActiveContracts a		
			
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous quarter sales for CO Profile Report.'
		goto ERROREXIT
	END

	update #ActiveContracts
		set ZeroSalesCountFromPreviousQuarters = ( select count(*)
										from tbl_Cntrcts_Sales s 
										where s.CntrctNum = a.CntrctNum
										and s.Quarter_ID <= @previousQuarterId
										having sum(s.VA_Sales + s.OGA_Sales + s.SLG_Sales) = 0 )
	from #ActiveContracts a										
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating previous quarter zero sales for CO Profile Report.'
		goto ERROREXIT
	END
	
	update #ActiveContracts
		set MissingPricelistVerification = ~c.Pricelist_Verified
	from tbl_Cntrcts c join #ActiveContracts a on c.CntrctNum = a.CntrctNum
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating pricelist verification for CO Profile Report.'
		goto ERROREXIT
	END

	create table #COProfileReport
	(
		COId int,
		ContractingOfficerName nvarchar(50),

		ActiveContractCount int,
		ItemCount int,
		NonverifiedPricelistCount int,
		ContractValue money,
		
		PreviousQuarterTotalSales money,
		
		CurrentQuarterTotalSales money,
		
		ZeroSalesCount int,
		ActiveContractsDuringCurrentQuarterCount int,
		CurrentQuarterReportedSalesCount int,
		CurrentQuarterMissingSalesCount int,
		CurrentQuarterZeroSalesCount int,
		
		ExpectedSBAPlansCount int,	
		ActualSBAPlanCount int,

		MissingSBAPlansCount int,
		ExtraSBAPlansCount int,
		
		MissingSomeContractDataCount int
	)

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating final temp table for CO Profile Report.'
		goto ERROREXIT
	END
	
	insert into #COProfileReport
	( 
		COId,
		ContractingOfficerName,
		ExpectedSBAPlansCount,
		ActualSBAPlanCount,
		MissingSBAPlansCount,
		ExtraSBAPlansCount
	)
	select distinct COId, ContractingOfficerName, 0, 0, 0, 0  from #ActiveContracts
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting COs for final temp table for CO Profile Report.'
		goto ERROREXIT
	END
	


	update #COProfileReport
	set ActiveContractCount = ( select count(*)
									from #ActiveContracts a 
									where a.COId = r.COId
									and a.IsActiveNow = 1 )
	from #COProfileReport r									

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting ActiveContractCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END

	
	update #COProfileReport
	set ActiveContractsDuringCurrentQuarterCount = ( select count(*)
													from #ActiveContracts a 
													where a.COId = r.COId
													and a.WasActiveDuringCurrentReportingQuarter = 1 )
	from #COProfileReport r									
													

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting ActiveContractsDuringCurrentQuarterCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END
	
	
	update #COProfileReport
	set CurrentQuarterReportedSalesCount = ( select count(*)
											from #ActiveContracts a 
											where a.COId = r.COId
											and a.CurrentQuarterTotalSales is not null )
	from #COProfileReport r									
											

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting CurrentQuarterReportedSalesCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END

	
	update #COProfileReport
	set CurrentQuarterMissingSalesCount = ActiveContractsDuringCurrentQuarterCount - CurrentQuarterReportedSalesCount
			
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting CurrentQuarterMissingSalesCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END			


				
	update #COProfileReport 
		set ItemCount = ( select isnull( sum( a.ItemCount ), 0 )
							from #ActiveContracts a 
							where a.COId = r.COId
							and a.IsActiveNow = 1 )
	from #COProfileReport r									
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting ItemCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END			

	update #COProfileReport 
		set ContractValue = ( select isnull( sum( a.ContractValue ), 0 )
								from #ActiveContracts a 
								where a.COId = r.COId
								and a.IsActiveNow = 1
								and a.ContractValue is not null )
	from #COProfileReport r									
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting ContractValue for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		
	
	update #COProfileReport 
		set NonverifiedPricelistCount = ( select isnull( sum( convert( int, a.MissingPricelistVerification )), 0 )
											from #ActiveContracts a
											where a.COId = r.COId
											and a.IsActiveNow = 1 )
	from #COProfileReport r									

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting NonverifiedPricelistCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		
	

	update #COProfileReport 
		set CurrentQuarterTotalSales = ( select sum( a.CurrentQuarterTotalSales )
										 from #ActiveContracts a
										 where a.COId = r.COId )
	from #COProfileReport r									
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting CurrentQuarterTotalSales for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		
	
		
	update #COProfileReport 
		set PreviousQuarterTotalSales = ( select sum( a.PreviousQuarterTotalSales )
										 from #ActiveContracts a
										 where a.COId = r.COId )
	from #COProfileReport r									
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting PreviousQuarterTotalSales for final temp table for CO Profile Report.'
		goto ERROREXIT
	END	

	
	update #COProfileReport
		set CurrentQuarterZeroSalesCount = ( select count(*)
										from #ActiveContracts a 
										where a.COId = r.COId
										and a.CurrentQuarterTotalSales = 0 )
	from #COProfileReport r									

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting CurrentQuarterZeroSalesCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		
	
	update #COProfileReport
		set ExpectedSBAPlansCount = ( select isnull( sum( convert( int, a.ExpectedSBAPlan )), 0 )
										from #ActiveContracts a 
										where a.COId = r.COId
										and a.IsActiveNow = 1 
										and a.ContractValue is not null )
	from #COProfileReport r									
										

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting ExpectedSBAPlansCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		
	
	update #COProfileReport
		set	ActualSBAPlanCount = ( select isnull( sum( convert( int, a.HasSBAPlan )), 0 )
									from #ActiveContracts a 
									where a.COId = r.COId
									and a.IsActiveNow = 1 
									and a.ContractValue is not null )
	from #COProfileReport r									

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting ActualSBAPlanCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		
	
		update #COProfileReport
		set	MissingSBAPlansCount = ( select isnull( sum(  convert( int, a.ExpectedSBAPlan ) - convert( int, a.HasSBAPlan ) ), 0 )
									from #ActiveContracts a 
									where a.COId = r.COId
									and a.IsActiveNow = 1 
									and a.ContractValue is not null
									and a.ExpectedSBAPlan > a.HasSBAPlan )
	from #COProfileReport r									

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting MissingSBAPlansCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		
	

		update #COProfileReport
		set	ExtraSBAPlansCount = ( select isnull( sum(  convert( int, a.HasSBAPlan ) - convert( int, a.ExpectedSBAPlan ) ), 0 )
									from #ActiveContracts a 
									where a.COId = r.COId
									and a.IsActiveNow = 1 
									and a.ContractValue is not null 
									and a.HasSBAPlan > a.ExpectedSBAPlan )
	from #COProfileReport r									

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting ExtraSBAPlansCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		
	
	update #COProfileReport
		set MissingSomeContractDataCount = ( select count(*)
										from #ActiveContracts a 
										where a.COId = r.COId
										and a.MissingSomeContractData = 1
										and a.IsActiveNow = 1 )
	from #COProfileReport r									

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting MissingSomeContractDataCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		
	
	
	update #COProfileReport
		set ZeroSalesCount = ( select sum( a.ZeroSalesCountFromPreviousQuarters )
								from #ActiveContracts a 
								where a.COId = r.COId )
	from #COProfileReport r									

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting ZeroSalesCount for final temp table for CO Profile Report.'
		goto ERROREXIT
	END		

	select 
		COId,
		ContractingOfficerName,

		ActiveContractCount,
		ItemCount,
		NonverifiedPricelistCount,
		ContractValue,
		
		@previousQuarterId as PreviousQuarterId,
		@previousQuarterDescription as PreviousQuarterDescription,
		PreviousQuarterTotalSales,
		
		@currentQuarterId as CurrentQuarterId,
		@currentQuarterDescription as CurrentQuarterDescription,
		CurrentQuarterTotalSales,
		
		ZeroSalesCount,
		ActiveContractsDuringCurrentQuarterCount,
		CurrentQuarterReportedSalesCount,
		CurrentQuarterMissingSalesCount,
		CurrentQuarterZeroSalesCount,
		
		ExpectedSBAPlansCount,
		ActualSBAPlanCount,

		MissingSBAPlansCount,
		ExtraSBAPlansCount,
		
		MissingSomeContractDataCount
		from #COProfileReport
		order by ContractingOfficerName
		
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales by CO Profile report results.'
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


