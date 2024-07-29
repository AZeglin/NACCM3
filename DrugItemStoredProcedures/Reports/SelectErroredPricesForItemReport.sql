IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectErroredPricesForItemReport')
	BEGIN
		DROP  Procedure  SelectErroredPricesForItemReport
	END

GO

CREATE Procedure SelectErroredPricesForItemReport
(
@ContractNumber nvarchar(20),
@ContractId int,
@IsBPA bit,
@Division int,
@DrugItemId int,
@DrugItemNDCId int,
@FdaAssignedLabelerCode  	char(5),
@ProductCode         	char(4),
@PackageCode          	char(2),
@FutureHistoricalSelectionCriteria nchar(1),  -- F future, A active
@Covered nchar(1),
@currentYearNFAMPTableName nvarchar(11),
@SingleDualFromMailout nchar(1) =null,
@ItemDualPriceStatus nchar(1) = NULL,  -- based on current active prices
@FCP decimal(9,2) =null,
@IncludedFETAmount decimal(10,2) = null,
@DrugItemSubItemId int =NULL
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@PriceWithoutIFF decimal(18,2),
	@PercentageRepresentingPriceSwing decimal(9,2),
	@CurrentYear int,
	@LastDayOfLastYear datetime

	
BEGIN TRANSACTION

	select @PercentageRepresentingPriceSwing = 0.50 /* this should match percentage used in parent report */

	select @CurrentYear = year(getdate())

	if( @FutureHistoricalSelectionCriteria = 'F' )
	BEGIN
		select @CurrentYear = @CurrentYear + 1
	END
	
	select @LastDayOfLastYear = convert( datetime, '12/31/' + convert(nvarchar(4), @CurrentYear - 1 ) )
	

	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#ErroredPrices')
	BEGIN
		delete #ErroredPrices
	END
	else
	BEGIN
	
		create table #ErroredPrices 
		(
			DrugItemPriceId  int NULL,
			DrugItemSubItemId int NULL,
			SubItemIdentifier nchar(1) NULL,
			HistoricalNValue  nchar(1) NULL,
			PriceId   int NULL,
			PriceStartDate datetime NULL,
			PriceStopDate datetime NULL,
			Price decimal(18,2) NULL,
			MostRecentYearsHistoricalPrice decimal(18,2) NULL,
			PriceWithoutIFF decimal(18,2) NULL,
			CorrespondingFSSPriceWithoutIFFFromOtherContract decimal(18,2) NULL,
			IsTemporary  bit NULL,
			IsFSS  bit NULL,
			IsBIG4  bit NULL,
			PriceApplicabilityString nvarchar(200) NULL,
			VAIFF decimal(18,4) NULL,
			ExcludeFromExport bit NULL,
			LastModificationType nchar(1) NULL,
			ModificationStatusId   int NULL,                  
			CreatedBy nvarchar(120) NULL,     
			CreationDate datetime NULL,        
			LastModifiedBy nvarchar(120) NULL,             
			LastModificationDate datetime NULL,
			ErrorDescription nvarchar(1000) NULL
		)
	END
	
	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error creating temp table for errored prices'
		goto ERROREXIT
	END


	if @FutureHistoricalSelectionCriteria = 'A'
	BEGIN

		insert into #ErroredPrices
		(
			DrugItemPriceId,
			DrugItemSubItemId,
			SubItemIdentifier,
			HistoricalNValue,
			PriceId,
			PriceStartDate,
			PriceStopDate,
			Price,
			IsTemporary,
			IsFSS,
			IsBIG4,
			PriceApplicabilityString,
			VAIFF,
			ExcludeFromExport,	
			LastModificationType ,
			ModificationStatusId ,
			CreatedBy  ,
			CreationDate ,
			LastModifiedBy,
			LastModificationDate,
			ErrorDescription
		)			
		select p.DrugItemPriceId,
			p.DrugItemSubItemId,
			s.SubItemIdentifier,
			p.HistoricalNValue,
			p.PriceId,
			p.PriceStartDate,
			p.PriceStopDate,
			p.Price,
			p.IsTemporary,
			p.IsFSS,
			p.IsBIG4,
			dbo.GetPriceApplicabilityStringForReportFunction( p.DrugItemPriceId, @FutureHistoricalSelectionCriteria ) as PriceApplicabilityString,
			dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,
			p.ExcludeFromExport,
			p.LastModificationType,
			p.ModificationStatusId,                  
			p.CreatedBy,     
			p.CreationDate,        
			p.LastModifiedBy,        
			p.LastModificationDate,
			'' -- debug 'FET parm value was ' + convert( nvarchar(100), @IncludedFETAmount )
		from DI_DrugItemPrice p left outer join DI_DrugItemSubItems s on p.DrugItemSubItemId = s.DrugItemSubItemId
		where p.DrugItemId = @DrugItemId
		and	datediff( d, PriceStopDate, GETDATE() ) <= 0 
		and datediff( d, PriceStartDate, GETDATE() ) >= 0 
		order by p.PriceStartDate
			
		select @error = @@error

		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving drug item prices (A) for DrugItemId ' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END
	END
	else /* future */
	BEGIN
	
		insert into #ErroredPrices
		(
			DrugItemPriceId,
			DrugItemSubItemId,
			SubItemIdentifier,
			HistoricalNValue,
			PriceId,
			PriceStartDate,
			PriceStopDate,
			Price,
			IsTemporary,
			IsFSS,
			IsBIG4,
			PriceApplicabilityString,
			VAIFF,
			ExcludeFromExport,	
			LastModificationType ,
			ModificationStatusId ,
			CreatedBy  ,
			CreationDate ,
			LastModifiedBy,
			LastModificationDate
		)			
		select p.DrugItemPriceId,
			p.DrugItemSubItemId,
			s.SubItemIdentifier,
			p.HistoricalNValue,
			p.PriceId,
			p.PriceStartDate,
			p.PriceStopDate,
			p.Price,
			p.IsTemporary,
			p.IsFSS,
			p.IsBIG4,
			dbo.GetPriceApplicabilityStringForReportFunction( p.DrugItemPriceId, @FutureHistoricalSelectionCriteria ) as PriceApplicabilityString,
			dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) as VAIFF,
			p.ExcludeFromExport,
			p.LastModificationType,
			p.ModificationStatusId,                  
			p.CreatedBy,     
			p.CreationDate,        
			p.LastModifiedBy,        
			p.LastModificationDate
		from DI_DrugItemPrice p left outer join DI_DrugItemSubItems s on p.DrugItemSubItemId = s.DrugItemSubItemId
		where p.DrugItemId = @DrugItemId
		and	datediff( d, GETDATE(), PriceStartDate ) > 0 
		order by p.PriceStartDate

		select @error = @@error

		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving drug item prices (F) for DrugItemId ' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END
	
	END

	/* remove non-applicable prices from the analysis */
	if @DrugItemSubItemId is null
	BEGIN
		delete #ErroredPrices where DrugItemSubItemId is not null
	END
	else
	BEGIN
		delete #ErroredPrices where DrugItemSubItemId is null
	END

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error removing non-applicable prices from the analysis for item ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	
	select @rowcount = count(*)
	from #ErroredPrices
	
	/* no prices */
	if @rowcount = 0
	BEGIN
		goto OKEXIT
	END

	/* check for BIG4 price when mailout says single pricer */
	/*********    PBM discontinued use of mailout table in 2015    *********/
	/*
	if @SingleDualFromMailout = 'S'
	BEGIN
		update #ErroredPrices 
			set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'BIG4 price conflicts with single price status in PBM mailout table; '
		from #ErroredPrices e
		where e.IsBIG4 = 1
	END

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for BIG4=1 when mailout indicates single ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	
	if @rowcount > 0
	BEGIN
		select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' price errors for mailout single'
		print @errorMsg
	END
	*/

	/* check for only BIG4 price and no FSS price ( without N ) */
	update #ErroredPrices 
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'BIG4 price present without corresponding FSS price; '
	from #ErroredPrices e
	where e.IsBIG4 = 1
	and e.DrugItemSubItemId is null
	and not exists ( select DrugItemPriceId from #ErroredPrices where IsFSS = 1 and DrugItemSubItemId is null )

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for BIG4 without FSS ( without N ) ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	
	if @rowcount > 0
	BEGIN
		select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' price errors for big4 without fss'
		print @errorMsg
	END

	/* check for only BIG4 price and no FSS price ( with N ) */
	update #ErroredPrices 
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'BIG4 price present without corresponding FSS price; '
	from #ErroredPrices e
	where e.IsBIG4 = 1
	and e.DrugItemSubItemId is not null
	and e.DrugItemSubItemId = @DrugItemSubItemId
	and not exists ( select DrugItemPriceId 
						from #ErroredPrices p 
						where p.IsFSS = 1 and p.DrugItemSubItemId = e.DrugItemSubItemId )

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for BIG4 without FSS ( with N ) ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	
	if @rowcount > 0
	BEGIN
		select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' price errors for big4 without fss (with N)'
		print @errorMsg
	END

	/* check for zero price */
	update #ErroredPrices 
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Zero price is invalid; '
	from #ErroredPrices e
	where e.Price = 0

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for zero price ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	
	if @rowcount > 0
	BEGIN
		select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' price errors for zero price'
		print @errorMsg
	END

	/* price less than FET */
	update #ErroredPrices 
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Price is <= FET; '
	from #ErroredPrices e
	where e.Price <= @IncludedFETAmount
	and e.Price <> .01


	/* check FSS price against FCP */
	update #ErroredPrices
	set PriceWithoutIFF = ( Price - @IncludedFETAmount ) - ( ( Price - @IncludedFETAmount ) * VAIFF )
	where Price is not null
	and Price > 0

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error calculating price without IFF ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	

	if @ItemDualPriceStatus = 'T'
	BEGIN
		update #ErroredPrices 
			set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Price ( without IFF ) is > FCP; '
		from #ErroredPrices e
		where e.IsBIG4 = 1
		and e.PriceWithoutIFF > @FCP
		and e.Price > 0
	END
	else /* single pricer */
	BEGIN
		update #ErroredPrices 
			set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Price ( without IFF ) is > FCP; '
		from #ErroredPrices e
		where e.IsFSS = 1
		and e.PriceWithoutIFF > @FCP
		and e.Price > 0

	END

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error message for price > FCP ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	
	if @rowcount > 0
	BEGIN
		select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' price errors for price > FCP'
		print @errorMsg
	END

	/* warning for identical FSS and BIG4 price ( without N ) */
	update #ErroredPrices 
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Warning: FSS and BIG4 have same values; '
	from #ErroredPrices e
	where e.IsBIG4 = 1
	and e.DrugItemSubItemId is null
	and exists ( select DrugItemPriceId 
					from #ErroredPrices p 
					where p.IsFSS = 1 
					and p.DrugItemSubItemId is null
					and p.Price = e.Price )

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging warning message for fss price = big4 price ( without N ) ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	
	if @rowcount > 0
	BEGIN
		select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' price errors for fss = big4'
		print @errorMsg
	END
	
	/* warning for identical FSS and BIG4 price ( with N ) */
	update #ErroredPrices 
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Warning: FSS and BIG4 have same values; '
	from #ErroredPrices e
	where e.IsBIG4 = 1
	and e.DrugItemSubItemId is not null
	and e.DrugItemSubItemId = @DrugItemSubItemId
	and exists ( select DrugItemPriceId 
					from #ErroredPrices p 
					where p.IsFSS = 1 
					and p.DrugItemSubItemId = e.DrugItemSubItemId
					and p.Price = e.Price )

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging warning message for fss price = big4 price ( with N ) ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
	
	if @rowcount > 0
	BEGIN
		select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' price errors for fss = big4 (with N)'
		print @errorMsg
	END
	
	/* set up historical prices for swing comparison */
	if @FutureHistoricalSelectionCriteria = 'A'
	BEGIN
		/* look up historical fss price from history table */
		update  #ErroredPrices
		set MostRecentYearsHistoricalPrice = isnull( h.Price, -1 )
		from #ErroredPrices e, DI_DrugItemPriceHistory h
		where h.DrugItemId = @DrugItemId
		and h.IsFSS = 1
		and e.IsFSS = 1
		and @LastDayOfLastYear between h.PriceStartDate and h.PriceStopDate
		and h.PriceStartDate = ( select max( PriceStartDate ) 
									from DI_DrugItemPriceHistory 
									where DrugItemId = @DrugItemId 
									and IsFSS = 1 
									and @LastDayOfLastYear between h.PriceStartDate and h.PriceStopDate )
		
		select @error = @@error, @rowcount = @@rowcount

		if @error <> 0
		BEGIN
			select @errorMsg = 'Error looking up historical fss price from history table ' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END
	
		if @rowcount > 0
		BEGIN
			select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' ( should be 1 ) historical FSS price '
			print @errorMsg
		END

	
		/* look up historical big4 price from history table */
		update  #ErroredPrices
		set MostRecentYearsHistoricalPrice = isnull( h.Price, -1 )
		from #ErroredPrices e, DI_DrugItemPriceHistory h
		where h.DrugItemId = @DrugItemId
		and h.IsBIG4 = 1
		and e.IsBIG4 = 1
		and @LastDayOfLastYear between h.PriceStartDate and h.PriceStopDate
		and h.PriceStartDate = ( select max( PriceStartDate ) 
									from DI_DrugItemPriceHistory 
									where DrugItemId = @DrugItemId 
									and IsBIG4 = 1 
									and @LastDayOfLastYear between h.PriceStartDate and h.PriceStopDate )
		
		select @error = @@error, @rowcount = @@rowcount

		if @error <> 0
		BEGIN
			select @errorMsg = 'Error looking up historical big4 price from history table ' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END

		if @rowcount > 0
		BEGIN
			select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' ( should be 1 ) historical BIG4 price '
			print @errorMsg
		END

	END
	else /* future */
	BEGIN
		/* look up historical fss price from active price table */
		update #ErroredPrices
		set MostRecentYearsHistoricalPrice = isnull( p.Price, -1 )
		from #ErroredPrices e, DI_DrugItemPrice p
		where p.DrugItemId = @DrugItemId
		and e.IsFSS = 1
		and p.IsFSS = 1
		and @LastDayOfLastYear between p.PriceStartDate and p.PriceStopDate

		select @error = @@error, @rowcount = @@rowcount

		if @error <> 0
		BEGIN
			select @errorMsg = 'Error looking up historical fss price from active price table ' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END
	
		if @rowcount > 0
		BEGIN
			select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' ( should be 1 ) active historical FSS price '
			print @errorMsg
		END
	
		/* look up historical big4 price from active price table */
		update #ErroredPrices
		set MostRecentYearsHistoricalPrice = isnull( p.Price, -1 )
		from #ErroredPrices e, DI_DrugItemPrice p
		where p.DrugItemId = @DrugItemId
		and e.IsBIG4 = 1
		and p.IsBIG4 = 1
		and @LastDayOfLastYear between p.PriceStartDate and p.PriceStopDate
		
		select @error = @@error, @rowcount = @@rowcount

		if @error <> 0
		BEGIN
			select @errorMsg = 'Error looking up historical big4 price from active price table ' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END

		if @rowcount > 0
		BEGIN
			select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' ( should be 1 ) active historical BIG4 price '
			print @errorMsg
		END

	END		
				
	/* price swings vs historical price */
	update #ErroredPrices 
		set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Warning: Price swing > ' + convert( nvarchar(12), @PercentageRepresentingPriceSwing * 100 ) + '%; '
	from #ErroredPrices e
	where e.Price >= e.MostRecentYearsHistoricalPrice + ( e.MostRecentYearsHistoricalPrice * @PercentageRepresentingPriceSwing )
	or e.Price <= e.MostRecentYearsHistoricalPrice - ( e.MostRecentYearsHistoricalPrice * @PercentageRepresentingPriceSwing )
	and e.MostRecentYearsHistoricalPrice <> -1
	
	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error description for price swings vs historical price ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
		
	if @rowcount > 0
	BEGIN
		select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' price swings '
		print @errorMsg
	END
		
	/* fss price significantly less than fcp */
	if @FCP is not null 
	BEGIN
		if @ItemDualPriceStatus <> 'T'
		BEGIN
			update #ErroredPrices 
				set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Warning: FSS Price ' + convert( nvarchar(12), @PercentageRepresentingPriceSwing * 100 ) + '% < FCP; '
			from #ErroredPrices e
			where e.Price <= @FCP - ( @FCP * @PercentageRepresentingPriceSwing )
			and e.IsFSS = 1
		END
		else /* BIG4 price significantly less than fcp */
		BEGIN
			update #ErroredPrices 
				set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'Warning: BIG4 Price ' + convert( nvarchar(12), @PercentageRepresentingPriceSwing * 100 ) + '% < FCP; '
			from #ErroredPrices e
			where e.Price <= @FCP - ( @FCP * @PercentageRepresentingPriceSwing )	
			and e.IsBIG4 = 1
		END					
	END
		
	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error logging error description for price significantly less than fcp ' + convert( nvarchar(20), @DrugItemId )
		goto ERROREXIT
	END
			
	if @rowcount > 0
	BEGIN
		select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' prices significantly less than fcp '
		print @errorMsg
	END
				
	/* if BPA or National Contract price, set up fss price for greater than comparison */
	if @IsBPA = 1 OR @Division = 2
	BEGIN
	
		/* look up fss contract price for the item*/
		/* note: this would be on a different contract */
		update #ErroredPrices
		set CorrespondingFSSPriceWithoutIFFFromOtherContract =  convert( decimal(18,2), ( p.Price - ( p.Price * dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) )))
		from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
		join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
		where n.FdaAssignedLabelerCode = @FdaAssignedLabelerCode
		and n.ProductCode = @ProductCode
		and n.PackageCode = @PackageCode
		and n.DrugItemNDCId <> @DrugItemNDCId
		and p.IsFSS = 1
		and	datediff( d, p.PriceStopDate, #ErroredPrices.PriceStartDate ) <= 0 
		and datediff( d, p.PriceStartDate, #ErroredPrices.PriceStartDate ) >= 0  
	
		select @error = @@error, @rowcount = @@rowcount
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error looking up CorrespondingFSSPriceWithoutIFFFromOtherContract from active price table ' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END
			
		if @rowcount > 0
		BEGIN
			select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' FSS prices for the current BPA or National Contract '
			print @errorMsg
		END
										
		update #ErroredPrices 
			set ErrorDescription = convert( nvarchar(1000), isnull( ErrorDescription, '' ) ) + 'NC or BPA Price > FSS Price. '
		from #ErroredPrices e
		where PriceWithoutIFF > CorrespondingFSSPriceWithoutIFFFromOtherContract
				
		select @error = @@error, @rowcount = @@rowcount

		if @error <> 0
		BEGIN
			select @errorMsg = 'Error logging error description for price > FSS Price ' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END

		if @rowcount > 0
		BEGIN
			select @errorMsg = 'Debug: logged ' + convert( nvarchar(20), @rowcount ) + ' BPA or NC price > FSS price on other contract '
			print @errorMsg
		END
						
	END					
									
	select DrugItemPriceId,
			DrugItemSubItemId,
			SubItemIdentifier,
			HistoricalNValue,
			PriceId,
			PriceStartDate,
			PriceStopDate,
			Price,
			IsTemporary,
			IsFSS,
			IsBIG4,
			PriceApplicabilityString,
			VAIFF,
			ExcludeFromExport,	
			LastModificationType ,
			ModificationStatusId ,
			CreatedBy  ,
			CreationDate ,
			LastModifiedBy,
			LastModificationDate,
			ErrorDescription 
	from #ErroredPrices
	where ErrorDescription is not null and LEN( LTRIM( RTRIM( ErrorDescription ))) > 0
	order by PriceStartDate

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error with final select of errored prices for subreport' + convert( nvarchar(20), @DrugItemId )
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

ENDEXIT:






	






