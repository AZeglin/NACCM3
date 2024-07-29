IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'CheckForOverlappingRebateDateFunction')
	BEGIN
		DROP  Function  CheckForOverlappingRebateDateFunction
	END

GO


CREATE FUNCTION dbo.CheckForOverlappingRebateDateFunction
(
@ContractNumber nvarchar(20),
@RebateId int = null,
@StartQuarterId int, 
@EndQuarterId int, 
@CustomStartDate datetime = null
)

RETURNS bit

AS
BEGIN

	DECLARE @RebateExists bit,
		@rebateStartingDate datetime,
		@rebateEndingDate datetime,
		@rebateCustomStartingQuarterId int,
		@rebateCustomEndingQuarterId int,
		@CustomEndDate datetime

	select @RebateExists = 0

	select 	@rebateStartingDate = Start_Date
	from tlkup_year_qtr
	where Quarter_ID = @StartQuarterId
		
	select 	@rebateEndingDate = End_Date
	from tlkup_year_qtr
	where Quarter_ID = @EndQuarterId


	if @CustomStartDate is not null
	BEGIN
		select @CustomEndDate = DATEADD( dd, -1, dateadd( yy, 1, @CustomStartDate ))

		select @rebateCustomStartingQuarterId = Quarter_ID
		from tlkup_year_qtr
		where @CustomStartDate between Start_Date and End_Date

		select @rebateCustomEndingQuarterId = Quarter_ID
		from tlkup_year_qtr
		where @CustomEndDate between Start_Date and End_Date
	END

	/* insert */
	if @RebateId is null 
	BEGIN
		if @CustomStartDate is null
		BEGIN
			if exists( select RebateId from tbl_Rebates
						where ContractNumber = @ContractNumber
						and CustomStartDate is null
						and ( @StartQuarterId between StartQuarterId and EndQuarterId
						or @EndQuarterId between StartQuarterId and EndQuarterId
						or StartQuarterId between @StartQuarterId and @EndQuarterId
						or EndQuarterId between @StartQuarterId and @EndQuarterId ))
			BEGIN
				select @RebateExists = 1
				goto DONEEXIT
			END

			if exists( select RebateId from tbl_Rebates
						where ContractNumber = @ContractNumber
						and CustomStartDate is not null

						and ( @rebateStartingDate between CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate ))
						or @rebateEndingDate between CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate ))
						or CustomStartDate between @rebateStartingDate and @rebateEndingDate
						or DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate )) between @rebateStartingDate and @rebateEndingDate ))
			BEGIN
				select @RebateExists = 1
				goto DONEEXIT
			END
		END
		else /* compare with @CustomStartDate */
		BEGIN
			if exists( select RebateId from tbl_Rebates
						where ContractNumber = @ContractNumber
						and CustomStartDate is null
						and ( @rebateCustomStartingQuarterId between StartQuarterId and EndQuarterId
						or @rebateCustomEndingQuarterId between StartQuarterId and EndQuarterId
						or StartQuarterId between @rebateCustomStartingQuarterId and @rebateCustomEndingQuarterId
						or EndQuarterId between @rebateCustomStartingQuarterId and @rebateCustomEndingQuarterId ))
			BEGIN
				select @RebateExists = 1
				goto DONEEXIT
			END

			if exists( select RebateId from tbl_Rebates
						where ContractNumber = @ContractNumber
						and CustomStartDate is not null
						and ( @CustomStartDate between  CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate ))
						or @CustomEndDate between CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate )) 
						or CustomStartDate between @CustomStartDate and @CustomEndDate
						or DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate )) between @CustomStartDate and @CustomEndDate ) )
			BEGIN
				select @RebateExists = 1
				goto DONEEXIT
			END

		END
	END
	else /* update */
	BEGIN
		if @CustomStartDate is null
		BEGIN
			if exists( select RebateId from tbl_Rebates
						where ContractNumber = @ContractNumber
						and CustomStartDate is null
						and RebateId <> @RebateId
						and ( @StartQuarterId between StartQuarterId and EndQuarterId
						or @EndQuarterId between StartQuarterId and EndQuarterId
						or StartQuarterId between @StartQuarterId and @EndQuarterId
						or EndQuarterId between @StartQuarterId and @EndQuarterId ))
			BEGIN
				select @RebateExists = 1
				goto DONEEXIT
			END

			if exists( select RebateId from tbl_Rebates
						where ContractNumber = @ContractNumber
						and CustomStartDate is not null
						and RebateId <> @RebateId
						and ( @rebateStartingDate between CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate ))
						or @rebateEndingDate between CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate ))
						or CustomStartDate between @rebateStartingDate and @rebateEndingDate
						or DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate )) between @rebateStartingDate and @rebateEndingDate ))
			BEGIN
				select @RebateExists = 1
				goto DONEEXIT
			END
		END
		else /* compare with @CustomStartDate */
		BEGIN
			if exists( select RebateId from tbl_Rebates
						where ContractNumber = @ContractNumber
						and CustomStartDate is null
						and RebateId <> @RebateId
						and ( @rebateCustomStartingQuarterId between StartQuarterId and EndQuarterId
						or @rebateCustomEndingQuarterId between StartQuarterId and EndQuarterId
						or StartQuarterId between @rebateCustomStartingQuarterId and @rebateCustomEndingQuarterId
						or EndQuarterId between @rebateCustomStartingQuarterId and @rebateCustomEndingQuarterId ))
			BEGIN
				select @RebateExists = 1
				goto DONEEXIT
			END

			if exists( select RebateId from tbl_Rebates
						where ContractNumber = @ContractNumber
						and CustomStartDate is not null
						and RebateId <> @RebateId
						and ( @CustomStartDate between  CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate ))
						or @CustomEndDate between CustomStartDate and DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate )) 
						or CustomStartDate between @CustomStartDate and @CustomEndDate
						or DATEADD( dd, -1, DATEADD( yy, 1, CustomStartDate )) between @CustomStartDate and @CustomEndDate ) )
			BEGIN
				select @RebateExists = 1
				goto DONEEXIT
			END

		END

	END


DONEEXIT:

	Return @RebateExists
END

