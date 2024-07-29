IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetTPRAlwaysHasBasePrice')
	BEGIN
		DROP  Procedure  GetTPRAlwaysHasBasePrice
	END

GO

CREATE Procedure GetTPRAlwaysHasBasePrice
(
@DrugItemId int,
@PriceStartDate datetime,   /* presumes price passed in is TPR */      
@PriceEndDate datetime,           
@IsFSS bit,                                           	                  
@IsBIG4 bit,                                          	                  
@IsVA bit,                                            	                  
@IsBOP bit,                                           	                  
@IsCMOP bit,                                          	                  
@IsDOD bit,                                           	                  
@IsHHS bit,                                           	                  
@IsIHS bit,                                           	                  
@IsIHS2 bit,                                          	                  
@IsDIHS bit,                                          	                  
@IsNIH bit,                                           	                  
@IsPHS bit,                                           	                  
@IsSVH bit,                                           	                  
@IsSVH1 bit,                                          	                  
@IsSVH2 bit,                                          	                  
@IsTMOP bit,                                          	                  
@IsUSCG bit,
@IsFHCC bit,
@TPRAlwaysHasBasePrice bit OUTPUT
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)

BEGIN

	create table #TPRDates
	(
		TPRDate  datetime  NOT NULL,
		HasBase  bit NOT NULL
	)

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table'
		raiserror( @errorMsg, 16, 1 )
	END

	create table #BasePriceDates
	(
		PriceStartDate datetime NOT NULL,
		PriceStopDate datetime NOT NULL
	)

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table'
		raiserror( @errorMsg, 16, 1 )
	END


	insert into #TPRDates
	( TPRDate, HasBase )
	select [Date], 0
	from AllDates
	where [Date] between @PriceStartDate and @PriceEndDate

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting into #TPRDates'
		raiserror( @errorMsg, 16, 1 )
	END

	if not exists (Select * from DI_DrugItemPrice
							where DrugItemId = @DrugItemId
							and IsTemporary = 0
							and IsFSS = @IsFSS                                          	                  
							and IsBIG4 = @IsBIG4                                          	                  
							and IsVA = @IsVA                                            	                  
							and IsBOP = @IsBOP                                           	                  
							and IsCMOP = @IsCMOP                                          	                  
							and IsDOD = @IsDOD                                           	                  
							and IsHHS = @IsHHS                                           	                  
							and IsIHS = @IsIHS                                           	                  
							and IsIHS2 = @IsIHS2                                          	                  
							and IsDIHS = @IsDIHS                                          	                  
							and IsNIH = @IsNIH                                           	                  
							and IsPHS = @IsPHS                                           	                  
							and IsSVH = @IsSVH                                           	                  
							and IsSVH1 = @IsSVH1                                          	                  
							and IsSVH2 = @IsSVH2                                          	                  
							and IsTMOP = @IsTMOP                                          	                  
							and IsUSCG = @IsUSCG
							and IsFHCC = @IsFHCC
							and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceEndDate ) or
								PriceStartDate between @PriceStartDate and @PriceEndDate or
								PriceStopDate between @PriceStartDate and @PriceEndDate or
								( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceEndDate ) or
								( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceEndDate ) or
								PriceStartDate = @PriceEndDate  or
								PriceStopDate = @PriceStartDate ))
	Begin
		select @TPRAlwaysHasBasePrice = 0
	End
	Else 
	Begin
		insert into #BasePriceDates
		( PriceStartDate, PriceStopDate )
		select PriceStartDate, PriceStopDate
		from DI_DrugItemPrice
		where DrugItemId = @DrugItemId
			and IsTemporary = 0
			and IsFSS = @IsFSS                                          	                  
			and IsBIG4 = @IsBIG4                                          	                  
			and IsVA = @IsVA                                            	                  
			and IsBOP = @IsBOP                                           	                  
			and IsCMOP = @IsCMOP                                          	                  
			and IsDOD = @IsDOD                                           	                  
			and IsHHS = @IsHHS                                           	                  
			and IsIHS = @IsIHS                                           	                  
			and IsIHS2 = @IsIHS2                                          	                  
			and IsDIHS = @IsDIHS                                          	                  
			and IsNIH = @IsNIH                                           	                  
			and IsPHS = @IsPHS                                           	                  
			and IsSVH = @IsSVH                                           	                  
			and IsSVH1 = @IsSVH1                                          	                  
			and IsSVH2 = @IsSVH2                                          	                  
			and IsTMOP = @IsTMOP                                          	                  
			and IsUSCG = @IsUSCG
			and IsFHCC = @IsFHCC
			
			and (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceEndDate ) or
				PriceStartDate between @PriceStartDate and @PriceEndDate or
				PriceStopDate between @PriceStartDate and @PriceEndDate or
				( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceEndDate ) or
				( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceEndDate ) or
				PriceStartDate = @PriceEndDate  or
				PriceStopDate = @PriceStartDate )
			
		-- not tested
		update #TPRDates
		set HasBase = 1
		from #TPRDates t, #BasePriceDates b
		where t.TPRDate between  b.PriceStartDate and b.PriceStopDate

	End	


	if exists ( select HasBase from #TPRDates where HasBase = 0 )
	BEGIN
		select @TPRAlwaysHasBasePrice = 0
	END
	else
	BEGIN
		select @TPRAlwaysHasBasePrice = 1
	END

END


