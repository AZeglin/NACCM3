IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectPersonalizedNotification' )
BEGIN
	DROP PROCEDURE SelectPersonalizedNotification
END
GO

CREATE PROCEDURE SelectPersonalizedNotification
(
@CurrentUser uniqueidentifier, 
@IncludeSubordinates bit
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@CurrentSalesQuarter int,
		@LastFullQuarterId int,
		@ReportingQuarterId int,
		@CurrentUserCOID int



BEGIN TRANSACTION

	select @CurrentUserCOID = CO_ID 
	from NACSEC.dbo.SEC_UserProfile
	where UserId = @CurrentUser 

	select @error = @@ERROR

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting current user COID'
		goto ERROREXIT
	END

	IF OBJECT_ID('tempdb..#SubordinateUsers') IS NOT NULL 
	BEGIN
		drop table #SubordinateUsers
	
		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error dropping #SubordinateUsers temp table.'
			goto ERROREXIT
		END
	END

	create table #SubordinateUsers
	( 
		CO_ID int,
		UserId uniqueidentifier,
		FirstName nvarchar(40),
		LastName nvarchar(40),
		FullName nvarchar(80),
		Inactive bit,
		Division smallint
	)

	select @error = @@ERROR

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table #SubordinateUsers'
		goto ERROREXIT
	END

	if @IncludeSubordinates = 1
	BEGIN
		insert into #SubordinateUsers
		exec NACSEC.dbo.GetSubordinates2 @CurrentUserCOID

		select @error = @@ERROR, @rowCount = @@ROWCOUNT

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error finding current users subordinates.'
			goto ERROREXIT
		END
	END
	
	-- include current user, since they are not part of the subordinates list
	insert into #SubordinateUsers
	( CO_ID, UserId, FirstName, LastName, FullName, Inactive, Division )
	select CO_ID, UserId, FirstName, LastName, FullName, Inactive, Division
	from NACSEC.dbo.SEC_UserProfile u
	where u.UserId = @CurrentUser

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
		
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error finding current user.'
		goto ERROREXIT
	END
	

	IF OBJECT_ID('tempdb..#Notifications') IS NOT NULL 
	BEGIN
		drop table #Notifications
	
		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error dropping #Notifications temp table.'
			goto ERROREXIT
		END
	END

	create table #Notifications
	(
		PersonalizedNotificationId int identity(1,1) NOT NULL,
		CO_ID int,
		LastName nvarchar(40),
		FullName nvarchar(80),
		ContractNumber nvarchar(50),
		Contract_Record_ID int,
		Schedule_Number int,
		BPAContractNumber nvarchar(50),
		VendorName nvarchar(75),
		EffectiveDate datetime,
		ExpirationDate datetime,
		CompletionDate datetime,
		NotificationRank int,
		Countdown int,
	    NotificationMessage nvarchar(1000)
	)

	select @error = @@ERROR

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table #Notifications'
		goto ERROREXIT
	END

	insert into #Notifications
	( CO_ID, LastName, FullName, ContractNumber, Contract_Record_ID, Schedule_Number, VendorName, EffectiveDate, ExpirationDate, CompletionDate, 
		NotificationRank, Countdown, NotificationMessage )
	select s.CO_ID, s.LastName, s.FullName, c.CntrctNum, c.Contract_Record_ID, c.Schedule_Number, c.Contractor_Name, c.Dates_Effective, c.Dates_CntrctExp, c.Dates_Completion,
		1, DATEDIFF( DAY, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0), COALESCE( Dates_Completion, Dates_CntrctExp ) ) AS Countdown,
		'Expiring in ' + CONVERT( nvarchar(30), DATEDIFF( DAY, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0), COALESCE( Dates_Completion, Dates_CntrctExp ) )) + ' Days' AS NotificationMessage
	
	from tbl_Cntrcts c join #SubordinateUsers s on c.CO_ID = s.CO_ID
	where ( Dates_CntrctExp <= DATEADD( DAY, 90, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) )
	and Dates_CntrctExp >=  DATEADD( DAY, -1, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) )
	and Dates_Completion is null )
	or ( Dates_Completion <= DATEADD( DAY, 90, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) )
	and Dates_Completion >=  DATEADD( DAY, -1, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) )
	and Dates_Completion is not null )

	select @error = @@ERROR
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting personalized notifications for expiring contracts'
		goto ERROREXIT
	END

	insert into #Notifications
	( CO_ID, LastName, FullName, ContractNumber, BPAContractNumber, Contract_Record_ID, Schedule_Number, VendorName, EffectiveDate, ExpirationDate, CompletionDate, 
		NotificationRank, Countdown, NotificationMessage )
	select s.CO_ID, s.LastName, s.FullName, 
	  ParentContract.CntrctNum, c.CntrctNum as 'BPAContract', c.Contract_Record_ID, c.Schedule_Number, ParentContract.Contractor_Name, 
	  ParentContract.Dates_Effective, ParentContract.Dates_CntrctExp, ParentContract.Dates_Completion,
	  1, DATEDIFF( DAY, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0), COALESCE( ParentContract.Dates_Completion, ParentContract.Dates_CntrctExp ) ) AS Countdown,
	  'FSS Counterpart Expiring in ' + CONVERT( nvarchar(30), DATEDIFF( DAY, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0), COALESCE( ParentContract.Dates_Completion, ParentContract.Dates_CntrctExp ) )) + ' Days' AS NotificationMessage
	
	from tbl_Cntrcts c LEFT OUTER JOIN tbl_Cntrcts AS ParentContract ON c.BPA_FSS_Counterpart = ParentContract.CntrctNum
	join #SubordinateUsers s on c.CO_ID = s.CO_ID
	where c.BPA_FSS_Counterpart is not null
	and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
	and ( ParentContract.Dates_CntrctExp <= DATEADD( DAY, 90, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) )
	and ParentContract.Dates_CntrctExp >=  DATEADD( DAY, -1, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) )
	and ParentContract.Dates_Completion is null )
	or ( ParentContract.Dates_Completion <= DATEADD( DAY, 90, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) )
	and ParentContract.Dates_Completion >=  DATEADD( DAY, -1, DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) )
	and ParentContract.Dates_Completion is not null )

	select @error = @@ERROR
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting personalized notifications for expiring parent FSS contracts'
		goto ERROREXIT
	END

	insert into #Notifications
	( CO_ID, LastName, FullName, ContractNumber, Contract_Record_ID, Schedule_Number, VendorName, EffectiveDate, ExpirationDate, CompletionDate, 
		NotificationRank, Countdown, NotificationMessage )
	select s.CO_ID, s.LastName, s.FullName, 
		c.CntrctNum, c.Contract_Record_ID, c.Schedule_Number, c.Contractor_Name, c.Dates_Effective, c.Dates_CntrctExp, c.Dates_Completion,
		3, 0,	
		'Rebate term has completed as of ' + CONVERT( nvarchar(20),
													(SELECT     End_Date
													FROM          tlkup_year_qtr
													WHERE      Quarter_ID = EndQuarterId), 101) + '. ' AS RebateNote
	from tbl_Rebates r join tbl_Cntrcts c on r.ContractNumber = c.CntrctNum
	join #SubordinateUsers s on c.CO_ID = s.CO_ID
	where GETDATE() BETWEEN (SELECT End_Date
							FROM tlkup_year_qtr
							WHERE Quarter_ID = EndQuarterId) 
					AND DATEADD(day, 60, (SELECT End_Date
							FROM tlkup_year_qtr
							WHERE Quarter_ID = EndQuarterId)) 
	AND CustomStartDate IS NULL
	UNION
	select s.CO_ID, s.LastName, s.FullName, 
		c.CntrctNum, c.Contract_Record_ID, c.Schedule_Number, c.Contractor_Name, c.Dates_Effective, c.Dates_CntrctExp, c.Dates_Completion,
		3, 0,	
		'Rebate term has completed as of ' + CONVERT(nvarchar(20), DATEADD(year, 1, CustomStartDate), 101) + '. ' AS RebateNote
	from tbl_Rebates r join tbl_Cntrcts c on r.ContractNumber = c.CntrctNum
	join #SubordinateUsers s on c.CO_ID = s.CO_ID
	where GETDATE() BETWEEN DATEADD(year, 1, CustomStartDate) 
					AND DATEADD(day, 60, DATEADD(year, 1, CustomStartDate)) 
	AND CustomStartDate IS NOT NULL

	select @error = @@ERROR
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting personalized notifications for completed rebate terms'
		goto ERROREXIT
	END

	IF OBJECT_ID('tempdb..#SelectedSalesContracts') IS NOT NULL 
	BEGIN
		drop table #SelectedSalesContracts
	
		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error dropping #SelectedSalesContracts temp table.'
			goto ERROREXIT
		END
	END

	create table #SelectedSalesContracts
	(
		ContractNumber nvarchar(50),
		Contract_Record_ID int,
		Schedule_Number int,
		VendorName nvarchar(75),
		CO_ID int,
		LastName nvarchar(40), 
		FullName nvarchar(80),
		EffectiveDate datetime,
		EffectiveQuarterId int,
		ExpirationDate datetime,
		ExpiredQuarterId int,
		CompletionDate datetime,
		CompletedQuarterId int
	)

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table #SelectedSalesContracts'
		goto ERROREXIT
	END
		
	insert into #SelectedSalesContracts
	( ContractNumber, Contract_Record_ID, Schedule_Number, VendorName, CO_ID, LastName, FullName, EffectiveDate, EffectiveQuarterId, ExpirationDate, ExpiredQuarterId, CompletionDate, CompletedQuarterId )
	select c.CntrctNum, c.Contract_Record_ID, c.Schedule_Number, c.Contractor_Name, s.CO_ID, s.LastName, s.FullName, c.Dates_Effective, 
	( select y.Quarter_ID from tlkup_year_qtr y where c.Dates_Effective between y.Start_Date and y.End_Date ) as EffectiveQuarterId,
	c.Dates_CntrctExp,
	( select y.Quarter_ID from tlkup_year_qtr y where c.Dates_CntrctExp between y.Start_Date and y.End_Date ) as ExpiredQuarterId,
	c.Dates_Completion,
	case when ( c.Dates_Completion is null ) then -1 else ( select y.Quarter_ID from tlkup_year_qtr y where c.Dates_Completion between y.Start_Date and y.End_Date ) end as CompletedQuarterId
	
	from tbl_Cntrcts c join #SubordinateUsers s on c.CO_ID = s.CO_ID
	where c.Schedule_Number  <> 48 -- exclude FSSBPA
	-- special case where contract is terminated before its effective date
	and datediff( dd, c.Dates_Effective, convert( datetime, isnull( c.Dates_Completion, c.Dates_Effective ))) >= 0

	select @error = @@ERROR 

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales contracts for personalized notification'
		goto ERROREXIT
	END

	IF OBJECT_ID('tempdb..#MissingContractQuarters') IS NOT NULL 
	BEGIN
		drop table #MissingContractQuarters
	
		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error dropping #MissingContractQuarters temp table.'
			goto ERROREXIT
		END
	END

	create table #MissingContractQuarters
	(
		ContractNumber nvarchar(50),
		Contract_Record_ID int, 
		Schedule_Number int,
		MissingQuarterId int,
		MissingOrZero nchar(1),    -- M = Missing;  0 = Zero
		YearQuarterDescription nvarchar(20)
	)

	select @error = @@ERROR
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table #MissingContractQuarters'
		goto ERROREXIT
	END
	
	select @CurrentSalesQuarter = Quarter_ID from tlkup_year_qtr where GETDATE() between Start_Date and End_Date
	select @ReportingQuarterId = @CurrentSalesQuarter - 1
	select @LastFullQuarterId = @CurrentSalesQuarter - 2


	insert into #MissingContractQuarters
	(
		ContractNumber,	
		Contract_Record_ID, 
		Schedule_Number,
		MissingQuarterId,
		MissingOrZero,
		YearQuarterDescription
	)		
	select m.ContractNumber,
		m.Contract_Record_ID, 
		m.Schedule_Number,
		y.Quarter_ID,
		'M',
		y.Title
	from #SelectedSalesContracts m, tlkup_year_qtr y	 
	where y.Quarter_ID not in ( select s.Quarter_ID from tbl_Cntrcts_Sales s
									where s.CntrctNum = m.ContractNumber )
	and y.Quarter_ID between @ReportingQuarterId - 3 and @ReportingQuarterId										
	and ((( y.Quarter_ID between m.EffectiveQuarterId and m.ExpiredQuarterId ) and m.CompletedQuarterId = -1 )
	or	(( y.Quarter_ID between m.EffectiveQuarterId and m.CompletedQuarterId ) and m.CompletedQuarterId <> -1  ))			

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting missing sales quarters'
		goto ERROREXIT
	END
			
	-- include this for zero sales		
	--declare @CurrentSalesQuarter int,
	-- @LastFullQuarterId int,
	--@ReportingQuarterId int

	--select @CurrentSalesQuarter = Quarter_ID from tlkup_year_qtr where GETDATE() between Start_Date and End_Date
	--select @ReportingQuarterId = @CurrentSalesQuarter - 1
	--select @LastFullQuarterId = @CurrentSalesQuarter - 2		
		
	--	insert into #MissingContractQuarters
	--		(
	--			ContractNumber,	
	--			MissingQuarterId,
	--			MissingOrZero,
	--			YearQuarterDescription
	--		)
	--		select m.ContractNumber,
	--			y.Quarter_ID,
	--			'0',
	--			y.Title
	--		from #SelectedSalesContracts m join tbl_Cntrcts_Sales s on m.ContractNumber = s.CntrctNum
	--		join tlkup_year_qtr y on y.Quarter_ID = s.Quarter_ID
	--		and y.Quarter_ID  between @ReportingQuarterId - 3 and @ReportingQuarterId
	--		and ((( y.Quarter_ID between m.EffectiveQuarterId and m.ExpiredQuarterId ) and m.CompletedQuarterId = -1 )
	--		or	(( y.Quarter_ID between m.EffectiveQuarterId and m.CompletedQuarterId ) and m.CompletedQuarterId <> -1  ))			
	--		group by m.ContractNumber, y.Quarter_ID, y.Title
		
	--		having sum( s.VA_Sales ) = 0 and sum( s.OGA_Sales ) = 0 and sum( s.SLG_Sales ) = 0
					
	insert into #Notifications
	( CO_ID, LastName, FullName, ContractNumber, Contract_Record_ID, Schedule_Number, VendorName, EffectiveDate, ExpirationDate, CompletionDate, 
		NotificationRank, Countdown, NotificationMessage )
	select s.CO_ID, s.LastName, s.FullName, 
		s.ContractNumber, s.Contract_Record_ID, s.Schedule_Number, s.VendorName, s.EffectiveDate, s.ExpirationDate, s.CompletionDate,
		2, m.MissingQuarterId,	
		'Missing ' + m.YearQuarterDescription  + ' Sales' as Missing_Sales_Note
	from #MissingContractQuarters m join #SelectedSalesContracts s on m.ContractNumber = s.ContractNumber	

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting personalized notifications for missing sales'
		goto ERROREXIT
	END

	update #Notifications
	set NotificationRank = NotificationRank + 1000
	where CO_ID <> @CurrentUserCOID 

	select @error = @@ERROR
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating rank'
		goto ERROREXIT
	END

	select 	PersonalizedNotificationId, CO_ID, LastName, FullName, ContractNumber, Contract_Record_ID, Schedule_Number, BPAContractNumber, VendorName, EffectiveDate, ExpirationDate, CompletionDate, 
		NotificationRank, Countdown, NotificationMessage 
	from #Notifications
	order by NotificationRank, Countdown, LastName

	select @error = @@ERROR
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting final resultset for personalized notifications'
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


