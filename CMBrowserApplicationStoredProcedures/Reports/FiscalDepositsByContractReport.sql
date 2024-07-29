IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'FiscalDepositsByContractReport')
	BEGIN
		DROP  Procedure  FiscalDepositsByContractReport
	END

GO

CREATE Procedure FiscalDepositsByContractReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(50), /* required */
@StartingYear int,
@StartingQuarter int,
@EndingYear int,
@EndingQuarter int
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@StartingQuarterId int,
		@EndingQuarterId int 
		

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'Fiscal Deposits By Contract Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
	select @StartingQuarterId = Quarter_ID
		from tlkup_year_qtr
		where Year = @StartingYear
		and Qtr = @StartingQuarter
		
	select @EndingQuarterId = Quarter_ID
		from tlkup_year_qtr
		where Year = @EndingYear
		and Qtr = @EndingQuarter		

	select 	q.Year, 
			q.Qtr, 
			q.Title as YearQtr,
			isnull( k.PaymentAmount, 0 ) as CheckAmt,
			k.CreationDate as DateReceived,
			k.CheckNumber as CheckNum,
			k.DepositTicketNumber as DepositNum,
			k.Comments		
			
	from CM_PaymentsReceived k join tbl_Cntrcts c on k.ContractNumber = c.CntrctNum
	join tlkup_year_qtr q on k.QuarterId =	q.Quarter_ID
	
	where c.CntrctNum = @ContractNumber
	and q.Quarter_ID between @StartingQuarterId and @EndingQuarterId
	order by q.Title desc
	
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


