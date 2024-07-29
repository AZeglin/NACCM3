IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'DrugItemHasPricingErrorsFunction')
	BEGIN
		DROP  Function  DrugItemHasPricingErrorsFunction
	END

GO

CREATE Function DrugItemHasPricingErrorsFunction
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

returns bit  /* returns 1 as soon as any pricing error is encountered */

AS

BEGIN

	DECLARE @error int,
		@rowcount int,
		@errorMsg nvarchar(250),
		@PriceWithoutIFF decimal(18,2),
		@PercentageRepresentingPriceSwing decimal(9,2),
		@CurrentYear int,
		@LastDayOfLastYear datetime,
		@MostRecentYearsHistoricalFSSPrice decimal(18,2),
		@MostRecentYearsHistoricalBIG4Price decimal(18,2),
		@CurrentComparableFSSPrice decimal(18,2),
		@DateOfCurrentComparableFSSPrice datetime

		
	
	select @PercentageRepresentingPriceSwing = 0.50 /* this should match percentage used in parent report */

	select @CurrentYear = year(getdate())

	if( @FutureHistoricalSelectionCriteria = 'F' )
	BEGIN
		select @CurrentYear = @CurrentYear + 1
	END
	
	select @LastDayOfLastYear = convert( datetime, '12/31/' + convert(nvarchar(4), @CurrentYear - 1 ) )
	
	/* no prices */
	if @FutureHistoricalSelectionCriteria = 'A'
	BEGIN
		select @rowcount = ( select count(*)  
			from DI_DrugItemPrice p 
			where p.DrugItemId = @DrugItemId
			and	datediff( d, PriceStopDate, GETDATE() ) <= 0 
			and datediff( d, PriceStartDate, GETDATE() ) >= 0  )
	END
	else /* future */
	BEGIN
			select @rowcount = ( select count(*)  
			from DI_DrugItemPrice p 
			where p.DrugItemId = @DrugItemId
			and	datediff( d, GETDATE(), PriceStartDate ) > 0 )
	END
	
	if @rowcount = 0
	BEGIN
		goto ERROREXIT	
	END

	/* check for BIG4 price when mailout says single pricer */
	/*********    PBM discontinued use of mailout table in 2015    *********/
	/*
	if @SingleDualFromMailout = 'S'
	BEGIN
		if @FutureHistoricalSelectionCriteria = 'A'
		BEGIN
			if exists ( select DrugItemPriceId
								from DI_DrugItemPrice p 
								where p.DrugItemId = @DrugItemId
								and	datediff( d, PriceStopDate, GETDATE() ) <= 0 
								and datediff( d, PriceStartDate, GETDATE() ) >= 0  
								and p.IsBIG4 = 1 )
			BEGIN
				goto ERROREXIT
			END
		END
		else /* future */
		BEGIN
			if exists ( select DrugItemPriceId
								from DI_DrugItemPrice p 
								where p.DrugItemId = @DrugItemId
								and	datediff( d, GETDATE(), PriceStartDate ) > 0 
								and p.IsBIG4 = 1 )
			BEGIN
				goto ERROREXIT
			END
		END		
	END
	*/

	/* check for only BIG4 price and no FSS price ( without N ) */
	if @DrugItemSubItemId is null
	BEGIN
		if @FutureHistoricalSelectionCriteria = 'A'
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
							and	datediff( d, PriceStopDate, GETDATE() ) <= 0 
							and datediff( d, PriceStartDate, GETDATE() ) >= 0  
							and p.IsBIG4 = 1
							and p.DrugItemSubItemId is null
							and not exists ( select r.DrugItemPriceId 
												from DI_DrugItemPrice r 
												where r.DrugItemId = @DrugItemId
													and	datediff( d, r.PriceStopDate, GETDATE() ) <= 0 
													and datediff( d, r.PriceStartDate, GETDATE() ) >= 0  
													and r.IsFSS = 1 
													and r.DrugItemSubItemId is null ))
			BEGIN
				goto ERROREXIT
			END				
		END
		else /* future */
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
							and	datediff( d, GETDATE(), PriceStartDate ) > 0 
							and p.IsBIG4 = 1
							and p.DrugItemSubItemId is null
							and not exists ( select r.DrugItemPriceId 
											from DI_DrugItemPrice r 
											where r.DrugItemId = @DrugItemId
												and	datediff( d, GETDATE(), r.PriceStartDate ) > 0 
												and r.IsFSS = 1 
												and r.DrugItemSubItemId is null ))
			BEGIN
				goto ERROREXIT
			END				
		END
	END
	else
	BEGIN
		/* check for only BIG4 price and no FSS price ( with N ) */
		if @FutureHistoricalSelectionCriteria = 'A'
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
							and	datediff( d, p.PriceStopDate, GETDATE() ) <= 0 
							and datediff( d, p.PriceStartDate, GETDATE() ) >= 0  
							and p.IsBIG4 = 1
							and p.DrugItemSubItemId is not null
							and p.DrugItemSubItemId = @DrugItemSubItemId
							and not exists ( select r.DrugItemPriceId 
												from DI_DrugItemPrice r 
												where r.DrugItemId = @DrugItemId
													and	datediff( d, r.PriceStopDate, GETDATE() ) <= 0 
													and datediff( d, r.PriceStartDate, GETDATE() ) >= 0  
													and r.IsFSS = 1 
													and r.DrugItemSubItemId = p.DrugItemSubItemId ))
			BEGIN
				goto ERROREXIT
			END				
		END
		else /* future */
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
							and	datediff( d, GETDATE(), p.PriceStartDate ) > 0 
							and p.IsBIG4 = 1
							and p.DrugItemSubItemId is not null
							and p.DrugItemSubItemId = @DrugItemSubItemId
							and not exists ( select r.DrugItemPriceId 
											from DI_DrugItemPrice r 
											where r.DrugItemId = @DrugItemId
												and	datediff( d, GETDATE(), r.PriceStartDate ) > 0 
												and r.IsFSS = 1 
												and r.DrugItemSubItemId = p.DrugItemSubItemId ))
			BEGIN
				goto ERROREXIT
			END				
		END
	END
	
	/* check for zero price */
	if @FutureHistoricalSelectionCriteria = 'A'
	BEGIN
		if exists ( select DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
							and	datediff( d, p.PriceStopDate, GETDATE() ) <= 0 
							and datediff( d, p.PriceStartDate, GETDATE() ) >= 0  
							and p.Price = 0 )
		BEGIN
			goto ERROREXIT
		END				
	END
	else /* future */
	BEGIN
		if exists ( select DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
							and	datediff( d, GETDATE(), p.PriceStartDate ) > 0 
							and p.Price = 0 )
		BEGIN
			goto ERROREXIT
		END				
	END	

	/* check for price less than or equal to FET */
	if @FutureHistoricalSelectionCriteria = 'A'
	BEGIN
		if exists ( select DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
							and	datediff( d, p.PriceStopDate, GETDATE() ) <= 0 
							and datediff( d, p.PriceStartDate, GETDATE() ) >= 0  
							and p.Price <= @IncludedFETAmount )
		BEGIN
			goto ERROREXIT
		END				
	END
	else /* future */
	BEGIN
		if exists ( select DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
							and	datediff( d, GETDATE(), p.PriceStartDate ) > 0 
							and p.Price <= @IncludedFETAmount )
		BEGIN
			goto ERROREXIT
		END				
	END	

	if @FutureHistoricalSelectionCriteria = 'A'
	BEGIN
		if @ItemDualPriceStatus = 'T'
		BEGIN
			if exists ( select DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
							and	datediff( d, p.PriceStopDate, GETDATE() ) <= 0 
							and datediff( d, p.PriceStartDate, GETDATE() ) >= 0  
							and p.IsBIG4 = 1
							and convert( decimal(18,2), (( p.Price - @IncludedFETAmount ) - (( p.Price - @IncludedFETAmount ) * dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) ))) > @FCP 
							and p.Price > 0 )

			BEGIN
				goto ERROREXIT
			END				
		END
		else /* single pricer */
		BEGIN
			if exists ( select DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
							and	datediff( d, p.PriceStopDate, GETDATE() ) <= 0 
							and datediff( d, p.PriceStartDate, GETDATE() ) >= 0  
							and p.IsFSS = 1
							and convert( decimal(18,2), (( p.Price - @IncludedFETAmount ) - (( p.Price - @IncludedFETAmount ) * dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) ))) > @FCP 
							and p.Price > 0 )

			BEGIN
				goto ERROREXIT
			END			
		END
	END
	else /* future */
	BEGIN
		if @ItemDualPriceStatus = 'T'
		BEGIN
			if exists ( select DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
							and	datediff( d, GETDATE(), p.PriceStartDate ) > 0 
							and p.IsBIG4 = 1
							and convert( decimal(18,2), (( p.Price - @IncludedFETAmount ) - (( p.Price - @IncludedFETAmount ) * dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) ))) > @FCP 
							and p.Price > 0 )

			BEGIN
				goto ERROREXIT
			END				
		END
		else /* single pricer */
		BEGIN
			if exists ( select DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
							and	datediff( d, GETDATE(), p.PriceStartDate ) > 0 
							and p.IsFSS = 1
							and convert( decimal(18,2), (( p.Price - @IncludedFETAmount ) - (( p.Price - @IncludedFETAmount ) * dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) ))) > @FCP 
							and p.Price > 0 )

			BEGIN
				goto ERROREXIT
			END			
		END

	END
	
	
	/* warning for identical FSS and BIG4 price ( without N ) */
	if @DrugItemSubItemId is null
	BEGIN
		if @FutureHistoricalSelectionCriteria = 'A'
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
								and	datediff( d, p.PriceStopDate, GETDATE() ) <= 0 
								and datediff( d, p.PriceStartDate, GETDATE() ) >= 0  
								and p.IsBIG4 = 1
								and p.DrugItemSubItemId is null
								and exists ( select r.DrugItemPriceId 
												from DI_DrugItemPrice r
												where r.DrugItemId = @DrugItemId
													and	datediff( d, r.PriceStopDate, GETDATE() ) <= 0 
													and datediff( d, r.PriceStartDate, GETDATE() ) >= 0  
													and r.IsFSS = 1 
													and r.DrugItemSubItemId is null
													and r.Price = p.Price ))									
			BEGIN
				goto ERROREXIT
			END				
		END
		else /* future */
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
								and	datediff( d, GETDATE(), p.PriceStartDate ) > 0 
								and p.IsBIG4 = 1
								and p.DrugItemSubItemId is null
								and exists ( select r.DrugItemPriceId 
												from DI_DrugItemPrice r
												where r.DrugItemId = @DrugItemId
													and	datediff( d, GETDATE(), r.PriceStartDate ) > 0 
													and r.IsFSS = 1 
													and r.DrugItemSubItemId is null
													and r.Price = p.Price ))									
			BEGIN
				goto ERROREXIT
			END				
		END
	END
	else
	BEGIN
		/* warning for identical FSS and BIG4 price ( with N ) */
		if @FutureHistoricalSelectionCriteria = 'A'
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
								and	datediff( d, p.PriceStopDate, GETDATE() ) <= 0 
								and datediff( d, p.PriceStartDate, GETDATE() ) >= 0  
								and p.IsBIG4 = 1
								and p.DrugItemSubItemId is not null
								and p.DrugItemSubItemId = @DrugItemSubItemId
								and exists ( select r.DrugItemPriceId 
												from DI_DrugItemPrice r
												where r.DrugItemId = @DrugItemId
													and	datediff( d, r.PriceStopDate, GETDATE() ) <= 0 
													and datediff( d, r.PriceStartDate, GETDATE() ) >= 0  
													and r.IsFSS = 1 
													and r.DrugItemSubItemId is not null
													and r.DrugItemSubItemId = p.DrugItemSubItemId
													and r.Price = p.Price ))									
			BEGIN
				goto ERROREXIT
			END				
		END
		else /* future */
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
								and	datediff( d, GETDATE(), p.PriceStartDate ) > 0 
								and p.IsBIG4 = 1
								and p.DrugItemSubItemId is not null
								and p.DrugItemSubItemId = @DrugItemSubItemId
								and exists ( select r.DrugItemPriceId 
												from DI_DrugItemPrice r
												where r.DrugItemId = @DrugItemId
													and	datediff( d, GETDATE(), r.PriceStartDate ) > 0 
													and r.IsFSS = 1 
													and r.DrugItemSubItemId is not null
													and r.DrugItemSubItemId = p.DrugItemSubItemId
													and r.Price = p.Price ))									
			BEGIN
				goto ERROREXIT
			END				
		END
	END
	
	/* set up historical prices for swing comparison */
	if @FutureHistoricalSelectionCriteria = 'A'
	BEGIN
	
		/* look up historical fss price from history table */
		select @MostRecentYearsHistoricalFSSPrice = isnull( h.Price, -1 )
		from DI_DrugItemPriceHistory h 
		where h.DrugItemId = @DrugItemId
		and h.IsFSS = 1
		and @LastDayOfLastYear between h.PriceStartDate and h.PriceStopDate
		and h.PriceStartDate = ( select max( PriceStartDate ) 
									from DI_DrugItemPriceHistory 
									where DrugItemId = @DrugItemId 
									and IsFSS = 1 
									and @LastDayOfLastYear between h.PriceStartDate and h.PriceStopDate )
	
		select @error = @@error, @rowcount = @@rowcount
		
		if @rowcount < 1
		BEGIN
			select @MostRecentYearsHistoricalFSSPrice = -1
		END
	
		if @error <> 0
		BEGIN
			goto ERROREXIT
		END
	
		/* look up historical big4 price from history table */
		select @MostRecentYearsHistoricalBIG4Price = isnull( h.Price, -1 )
		from DI_DrugItemPriceHistory h 
		where h.DrugItemId = @DrugItemId
		and h.IsBIG4 = 1
		and @LastDayOfLastYear between h.PriceStartDate and h.PriceStopDate
		and h.PriceStartDate = ( select max( PriceStartDate ) 
									from DI_DrugItemPriceHistory 
									where DrugItemId = @DrugItemId 
									and IsBIG4 = 1
									and @LastDayOfLastYear between h.PriceStartDate and h.PriceStopDate )

		select @error = @@error, @rowcount = @@rowcount
		
		if @rowcount < 1
		BEGIN
			select @MostRecentYearsHistoricalBIG4Price = -1
		END
	
		if @error <> 0
		BEGIN
			goto ERROREXIT
		END
		
	END
	else /* future */
	BEGIN
		/* look up historical fss price from active price table */
		select @MostRecentYearsHistoricalFSSPrice = isnull( p.Price, -1 )
		from DI_DrugItemPrice p 
		where p.DrugItemId = @DrugItemId
		and p.IsFSS = 1
		and @LastDayOfLastYear between p.PriceStartDate and p.PriceStopDate

		select @error = @@error, @rowcount = @@rowcount
		
		if @rowcount < 1
		BEGIN
			select @MostRecentYearsHistoricalFSSPrice = -1
		END
	
		if @error <> 0
		BEGIN
			goto ERROREXIT
		END
		
		/* look up historical big4 price from active price table */
		select @MostRecentYearsHistoricalBIG4Price = isnull( p.Price, -1 )
		from DI_DrugItemPrice p 
		where p.DrugItemId = @DrugItemId
		and p.IsBIG4 = 1
		and @LastDayOfLastYear between p.PriceStartDate and p.PriceStopDate

		select @error = @@error, @rowcount = @@rowcount
		
		if @rowcount < 1
		BEGIN
			select @MostRecentYearsHistoricalBIG4Price = -1
		END
	
		if @error <> 0
		BEGIN
			goto ERROREXIT
		END
		
	END
	
				
	/* price swings vs historical price */
	if @MostRecentYearsHistoricalFSSPrice <> -1
	BEGIN
		if exists ( select p.DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
						and p.IsFSS = 1
						and ( p.Price >= @MostRecentYearsHistoricalFSSPrice + ( @MostRecentYearsHistoricalFSSPrice * @PercentageRepresentingPriceSwing )
						or p.Price <= @MostRecentYearsHistoricalFSSPrice - ( @MostRecentYearsHistoricalFSSPrice * @PercentageRepresentingPriceSwing )))
		BEGIN
			goto ERROREXIT
		END		
	END 

	if @MostRecentYearsHistoricalBIG4Price <> -1
	BEGIN
		if exists ( select p.DrugItemPriceId
						from DI_DrugItemPrice p 
						where p.DrugItemId = @DrugItemId
						and p.IsBIG4 = 1
						and ( p.Price >= @MostRecentYearsHistoricalBIG4Price + ( @MostRecentYearsHistoricalBIG4Price * @PercentageRepresentingPriceSwing )
						or p.Price <= @MostRecentYearsHistoricalBIG4Price - ( @MostRecentYearsHistoricalBIG4Price * @PercentageRepresentingPriceSwing )))
		BEGIN
			goto ERROREXIT
		END		
	END 

	/* fss price significantly less than fcp */
	if @FCP is not null 
	BEGIN
		if @ItemDualPriceStatus <> 'T'
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
							and p.Price <= @FCP - ( @FCP * @PercentageRepresentingPriceSwing )
							and p.IsFSS = 1 )
			BEGIN
				goto ERROREXIT
			END				
		END
		else /* BIG4 price significantly less than fcp */
		BEGIN
			if exists ( select p.DrugItemPriceId
							from DI_DrugItemPrice p 
							where p.DrugItemId = @DrugItemId
							and p.Price <= @FCP - ( @FCP * @PercentageRepresentingPriceSwing )
							and p.IsBIG4 = 1 )
			BEGIN
				goto ERROREXIT
			END				
		END					
	END
			
			
			
	/* if BPA or National Contract price, set up fss price for greater than comparison */
	if @IsBPA = 1 OR @Division = 2
	BEGIN
		if @FutureHistoricalSelectionCriteria = 'A'
		BEGIN
	
			select @DateOfCurrentComparableFSSPrice = GETDATE()
	
			/* look up fss contract price for the item*/
			/* note: this would be on a different contract */
			select @CurrentComparableFSSPrice = convert( decimal(18,2), ( p.Price - ( p.Price * dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) )))
			from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
			join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
			where n.FdaAssignedLabelerCode = @FdaAssignedLabelerCode
			and n.ProductCode = @ProductCode
			and n.PackageCode = @PackageCode
			and n.DrugItemNDCId <> @DrugItemNDCId
			and p.IsFSS = 1
			and	datediff( d, p.PriceStopDate, GETDATE() ) <= 0 
			and datediff( d, p.PriceStartDate, GETDATE() ) >= 0  
		
			select @error = @@error, @rowcount = @@rowcount
		
			if @error <> 0
			BEGIN
				goto ERROREXIT
			END
		
			if @rowcount <> 1
			BEGIN
				goto NOCOMPARABLEFSSPRICE
			END
				
		END
		else /* future */
		BEGIN
			/* note: using the minimum future date only */
			select @DateOfCurrentComparableFSSPrice = min( p.PriceStartDate )
			from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
				join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
			where n.FdaAssignedLabelerCode = @FdaAssignedLabelerCode
				and n.ProductCode = @ProductCode
				and n.PackageCode = @PackageCode
				and n.DrugItemNDCId <> @DrugItemNDCId
				and p.IsFSS = 1
				and	datediff( d, GETDATE(), p.PriceStartDate ) > 0 	
			
			select @error = @@error, @rowcount = @@rowcount
				
			if @error <> 0
			BEGIN
				goto ERROREXIT
			END
			
			if @rowcount <> 1
			BEGIN
				goto NOCOMPARABLEFSSPRICE
			END
			
			/* look up future fss contract price for the item*/
			/* note: this would be on a different contract */
			select @CurrentComparableFSSPrice = min( convert( decimal(18,2), ( p.Price - ( p.Price * dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) ))))
			from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
			join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
			where n.FdaAssignedLabelerCode = @FdaAssignedLabelerCode
			and n.ProductCode = @ProductCode
			and n.PackageCode = @PackageCode
			and n.DrugItemNDCId <> @DrugItemNDCId
			and p.IsFSS = 1
			and	datediff( d, p.PriceStopDate, @DateOfCurrentComparableFSSPrice ) <= 0 
			and datediff( d, p.PriceStartDate, @DateOfCurrentComparableFSSPrice ) >= 0  
	
			select @error = @@error, @rowcount = @@rowcount
				
			if @error <> 0 or @rowcount <> 1
			BEGIN
				goto ERROREXIT
			END
			
		END
		
		/* BPA or National Contract Price should be lower than the FSS price */
		/* for any price that is effective at the same time as the comparable FSS price */
		if exists ( select p.DrugItemPriceId
			from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
			where i.DrugItemNDCId = @DrugItemNDCId
			and p.IsFSS <> 1
			and	datediff( d, p.PriceStopDate, @DateOfCurrentComparableFSSPrice ) <= 0 
			and datediff( d, p.PriceStartDate, @DateOfCurrentComparableFSSPrice ) >= 0  
			and convert( decimal(18,2), ( p.Price - ( p.Price * dbo.GetVAIFF( @ContractNumber, p.PriceStartDate ) ))) > @CurrentComparableFSSPrice )
		BEGIN
			goto ERROREXIT
		END

	END			
			
NOCOMPARABLEFSSPRICE:			
						
	return 0
	
ERROREXIT:

	return 1

OKEXIT:

	return 0
	
END