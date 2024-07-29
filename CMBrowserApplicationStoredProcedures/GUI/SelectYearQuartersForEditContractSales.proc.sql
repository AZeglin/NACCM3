IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectYearQuartersForEditContractSales' )
BEGIN
	DROP PROCEDURE SelectYearQuartersForEditContractSales
END
GO

CREATE PROCEDURE SelectYearQuartersForEditContractSales
(
@CurrentUser uniqueidentifier,
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
		@LastReportedQuarterId int,
		@MinEndQuarterId int,
		@TempMinQuarterId int
		
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

	--select @LastReportedQuarterId = isnull( max( Quarter_ID ), @EffectiveQuarterId )
	--from tbl_Cntrcts_Sales
	--where CntrctNum = @ContractNumber

	

	-- select @TempMinQuarterId = case when @LastReportedQuarterId + 2 > @CurrentQuarterId - 1 then @LastReportedQuarterId + 2 else @CurrentQuarterId - 1 end
	-- 7/14/2016 if the current quarter is within normal reporting i.e., through contract end plus 1, then display through current ( i.e., no future reporting is shown ) else show 2 years beyond end of contract.
	select @MinEndQuarterId = case when @CurrentQuarterId <= @ContractEndQuarterId + 1 then @CurrentQuarterId else @ContractEndQuarterId + 8 end



	select -1 as Quarter_ID, 0 as Year, -1 as Qtr, '--Select--' as Title

	union

	select Quarter_ID, Year, Qtr, Title 
	from tlkup_year_qtr
	where Quarter_ID between @EffectiveQuarterId and @MinEndQuarterId
	and Quarter_ID not in ( select Quarter_ID 
							from tbl_Cntrcts_Sales
							where CntrctNum = @ContractNumber )
	order by Quarter_ID

	select @error = @@ERROR, @rowCount = @@ROWCOUNT

	if @error <> 0 or @rowCount < 1
	BEGIN
		select @errorMsg = 'Error selecting quarters for edit sales quarter list.'
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


