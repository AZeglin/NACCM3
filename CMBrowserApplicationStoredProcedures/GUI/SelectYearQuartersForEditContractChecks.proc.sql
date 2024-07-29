IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectYearQuartersForEditContractChecks' )
BEGIN
	DROP PROCEDURE SelectYearQuartersForEditContractChecks
END
GO

CREATE PROCEDURE SelectYearQuartersForEditContractChecks
(
@currentUser uniqueidentifier,
@ContractNumber nvarchar(50)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@CurrentDateWithoutTime datetime,
		@ContractEffectiveDate datetime,
		@ContractEndDate datetime,
		@EffectiveQuarterId int,
		@ContractEndQuarterId int,
		@CurrentQuarterId int,
		@SalesEndQuarterId int,
		@CheckEndQuarterId int  /* calculated */
		
BEGIN TRANSACTION

	
	select @CurrentDateWithoutTime = CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME )

	select @CurrentQuarterId = Quarter_ID
	from tlkup_year_qtr
	where @CurrentDateWithoutTime between Start_Date and End_Date

	select @ContractEffectiveDate = Dates_Effective, 
		@ContractEndDate = ISNULL( Dates_Completion, Dates_CntrctExp )
	from tbl_Cntrcts
	where CntrctNum = @ContractNumber

	select @EffectiveQuarterId = Quarter_ID
	from tlkup_year_qtr
	where @ContractEffectiveDate between Start_Date and End_Date

	select @ContractEndQuarterId = Quarter_ID
	from tlkup_year_qtr
	where @ContractEndDate between Start_Date and End_Date

	/* want to be able to add checks for sales even if past end of contract */
	select @SalesEndQuarterId = max( isnull( Quarter_ID, -1 )) from tbl_Cntrcts_Sales where CntrctNum = @ContractNumber

	if @ContractEndQuarterId > @SalesEndQuarterId
	BEGIN
		select @CheckEndQuarterId = @ContractEndQuarterId + 1
	END
	else
	BEGIN
		select @CheckEndQuarterId = @SalesEndQuarterId + 1
	END

	select -1 as Quarter_ID, 0 as Year, -1 as Qtr, '--Select--' as YearQuarterDescription

	union

	select Quarter_ID, Year, Qtr, Title as YearQuarterDescription
	from tlkup_year_qtr
	where Quarter_ID between @EffectiveQuarterId and @CheckEndQuarterId

	order by Quarter_ID

	select @error = @@ERROR, @rowCount = @@ROWCOUNT

	if @error <> 0 or @rowCount < 1
	BEGIN
		select @errorMsg = 'Error selecting quarters for edit check quarter list.'
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


