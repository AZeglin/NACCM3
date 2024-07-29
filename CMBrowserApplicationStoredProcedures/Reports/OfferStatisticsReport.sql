IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'OfferStatisticsReport')
	BEGIN
		DROP  Procedure  OfferStatisticsReport
	END

GO

CREATE Procedure OfferStatisticsReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@ScheduleNumber int, /* -1 = all */
@StartingYear int,
@StartingMonth int,
@EndingYear int,
@EndingMonth int
)

AS

/* FSS ONLY */

DECLARE		@month int,
			@year int,
			@rowCount int,
			@error int,
			@errorMsg nvarchar(200)

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Offer Statistics Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	create table #OfferStatisticsReport
	(
		[Year] int,
		MonthNumber int,
		Year90DaysEarlier int,
		Month90DaysEarlier int,
		ScheduleNumber int,
		AssignedOfferCount int,
		AwardedOfferCount int,
		NoAwardOfferCount int,
		WithdrawnOfferCount int,
		CumulativeActiveOfferCount int,
		CumulativeActiveOffersOlderThan90Days int
	)
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table 1'
		goto ERROREXIT
	END
	
	create table #Months
	(
		MonthNumber int
	)
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table 2'
		goto ERROREXIT
	END
	
	select @month = 1
	while @month < 13
	BEGIN
		insert into #Months
		( MonthNumber )
		values
		( @month )
		
		select @month = @month + 1
	END

	if @StartingYear > @EndingYear
	BEGIN
		select @errorMsg = 'Selected starting year cannot be greater than selected ending year.'
		goto ERROREXIT

	END

	create table #Years
	(
		[Year] int
	)

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table 3'
		goto ERROREXIT
	END
	
	select @year = @StartingYear
	while @year < @EndingYear + 1
	BEGIN
		insert into #Years
		( [Year] )
		values
		( @year )
		
		select @year = @year + 1	
	END
	
	if @ScheduleNumber <> -1
	BEGIN
		insert into #OfferStatisticsReport
		( [Year], MonthNumber, Year90DaysEarlier, Month90DaysEarlier, ScheduleNumber )
		select y.Year, m.MonthNumber, 
			YEAR( DATEADD( dd, -90, convert( datetime, '1/' + convert( nvarchar(2), m.MonthNumber ) + '/' + convert( nvarchar(4), y.Year )))),
			MONTH( DATEADD( dd, -90, convert( datetime, '1/' + convert( nvarchar(2), m.MonthNumber ) + '/' + convert( nvarchar(4), y.Year )))),
			s.Schedule_Number
		from [tlkup_Sched/Cat] s, #Months m, #Years y
		where s.Schedule_Number = @ScheduleNumber
		and s.Division = 1
		
		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting base records into temp table 1 for a particular schedule'
			goto ERROREXIT
		END
	END
	else
	BEGIN
		insert into #OfferStatisticsReport
		( [Year], MonthNumber, Year90DaysEarlier, Month90DaysEarlier, ScheduleNumber )
		select y.Year, m.MonthNumber, 
			YEAR( DATEADD( dd, -90, convert( datetime, '1/' + convert( nvarchar(2), m.MonthNumber ) + '/' + convert( nvarchar(4), y.Year )))),
			MONTH( DATEADD( dd, -90, convert( datetime, '1/' + convert( nvarchar(2), m.MonthNumber ) + '/' + convert( nvarchar(4), y.Year )))),
			s.Schedule_Number
		from [tlkup_Sched/Cat] s, #Months m, #Years y
		where s.Division = 1
		
		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting base records into temp table 1 for all fss schedules'
			goto ERROREXIT
		END	
	END
	
	/* eliminate extra months from the start and end using the specified range */
	delete #OfferStatisticsReport
	where [Year] = @StartingYear
	and MonthNumber < @StartingMonth
	
	delete #OfferStatisticsReport
	where [Year] = @EndingYear
	and MonthNumber > @EndingMonth
	
	update #OfferStatisticsReport 
	set AssignedOfferCount = ( select count( o.Offer_ID ) 
							from tbl_Offers o
							where Year( o.Dates_Assigned ) = s.Year
							and Month( o.Dates_Assigned ) = s.MonthNumber 
							and o.Schedule_Number = s.ScheduleNumber )
	from #OfferStatisticsReport s							
							
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error counting assigned offers.'
		goto ERROREXIT
	END						
	
	update #OfferStatisticsReport 
	set AwardedOfferCount = ( select count( o.Offer_ID ) 
							from tbl_Offers o join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
							where Year( o.Dates_Action ) = s.Year
							and Month( o.Dates_Action ) = s.MonthNumber 
							and o.Schedule_Number = s.ScheduleNumber
							and a.Action_Description = 'Awarded' )
	from #OfferStatisticsReport s							
							
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error counting awarded offers.'
		goto ERROREXIT
	END					
	
	update #OfferStatisticsReport 
	set NoAwardOfferCount = ( select count( o.Offer_ID ) 
							from tbl_Offers o join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
							where Year( o.Dates_Action ) = s.Year
							and Month( o.Dates_Action ) = s.MonthNumber 
							and o.Schedule_Number = s.ScheduleNumber
							and a.Action_Description = 'No Award' )
	from #OfferStatisticsReport s							
							
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error counting no-award offers.'
		goto ERROREXIT
	END					
	
	update #OfferStatisticsReport 
	set WithdrawnOfferCount = ( select count( o.Offer_ID ) 
							from tbl_Offers o join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
							where Year( o.Dates_Action ) = s.Year
							and Month( o.Dates_Action ) = s.MonthNumber 
							and o.Schedule_Number = s.ScheduleNumber
							and a.Action_Description = 'Withdrawn' )
	from #OfferStatisticsReport s							
							
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error counting withdrawn offers.'
		goto ERROREXIT
	END					
	
	/* cumulative includes offers that are still active plus */
	/* those that were active as of the date specified but completed later */
	/* also, those assigned and completed in the same month will not be counted */
	update #OfferStatisticsReport 
	set CumulativeActiveOfferCount = ( select count( o.Offer_ID ) 
							from tbl_Offers o join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
							where 
								((( Year( o.Dates_Assigned ) = s.Year
								and Month( o.Dates_Assigned ) <= s.MonthNumber )
								or Year( o.Dates_Assigned ) < s.Year )
								and o.Schedule_Number = s.ScheduleNumber
								and a.Complete = 0 )
							or
								((( Year( o.Dates_Assigned ) = s.Year
								and Month( o.Dates_Assigned ) <= s.MonthNumber )
								or Year( o.Dates_Assigned ) < s.Year )
								and (( Year( Dates_Action ) = s.Year
								and Month( Dates_Action ) > s.MonthNumber )
								or Year( Dates_Action ) > s.Year )
								and o.Schedule_Number = s.ScheduleNumber
								and a.Complete = 1 )
							)
	from #OfferStatisticsReport s							
							
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error counting cumulative active offers.'
		goto ERROREXIT
	END					
	
	/* cumulative includes offers active > 90 days that are still active plus */
	/* those that were active > 90 days as of the date specified but completed later */
	update #OfferStatisticsReport 
	set CumulativeActiveOffersOlderThan90Days = ( select count( o.Offer_ID ) 
							from tbl_Offers o join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
							where 
								((( Year( o.Dates_Assigned ) = s.Year90DaysEarlier
								and Month( o.Dates_Assigned ) <= s.Month90DaysEarlier )
								or Year( o.Dates_Assigned ) < s.Year90DaysEarlier )
								and o.Schedule_Number = s.ScheduleNumber
								and a.Complete = 0 )
							or
								((( Year( o.Dates_Assigned ) = s.Year90DaysEarlier
								and Month( o.Dates_Assigned ) <= s.Month90DaysEarlier )
								or Year( o.Dates_Assigned ) < s.Year90DaysEarlier )
								and (( Year( Dates_Action ) = s.Year
								and Month( Dates_Action ) > s.MonthNumber )
								or Year( Dates_Action ) > s.Year )
								and o.Schedule_Number = s.ScheduleNumber
								and a.Complete = 1 )
							)	
	from #OfferStatisticsReport s							
							
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error counting cumulative active offers older than 90 days.'
		goto ERROREXIT
	END					
	
	select r.[Year],
			r.MonthNumber,
			r.ScheduleNumber,
			s.Schedule_Name,
			r.AssignedOfferCount,
			r.AwardedOfferCount,
			r.NoAwardOfferCount,
			r.WithdrawnOfferCount,
			r.CumulativeActiveOfferCount,
			r.CumulativeActiveOffersOlderThan90Days
	from #OfferStatisticsReport r join [tlkup_Sched/Cat] s on r.ScheduleNumber = s.Schedule_Number
	order by r.[Year], r.MonthNumber, r.ScheduleNumber
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error making final select of offer statistics.'
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


