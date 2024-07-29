IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractChecks' )
BEGIN
	DROP PROCEDURE SelectContractChecks
END
GO

CREATE PROCEDURE SelectContractChecks
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@WithAdd bit = 0
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@CheckId int, 
		@QuarterId int, 
		@CheckAmount money,
		@CheckNumber nvarchar(50), 
		@DepositNumber nvarchar(50), 
		@DateReceived datetime, 
		@CheckComments nvarchar(255),
		@IsNewBlankRow bit,
		@YearQuarterDescription nvarchar(20)


BEGIN TRANSACTION



	if @WithAdd = 0
	BEGIN

		SELECT k.ID as CheckId, k.CntrctNum as ContractNumber, k.Quarter_ID as QuarterId, k.CheckAmt as CheckAmount, 
			k.CheckNum as CheckNumber, k.DepositNum as DepositNumber, k.DateRcvd as DateReceived, k.Comments as CheckComments, y.Title as YearQuarterDescription,
			0 as IsNewBlankRow
		FROM tbl_Cntrcts_Checks k join tlkup_year_qtr y ON k.Quarter_ID = y.Quarter_ID 
		WHERE k.CntrctNum = @ContractNumber 
		ORDER BY k.Quarter_ID DESC

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting checks for contract (WithAdd = 0).'
			goto ERROREXIT
		END
	END
	else
	BEGIN

		/* blank row definition */
		select @CheckId = -1,
			@QuarterId = -1,
			@CheckAmount = 0,
			@CheckNumber = '',
			@DepositNumber = '',
			@DateReceived = GETDATE(),
			@CheckComments = '',
			@YearQuarterDescription = '',
			@IsNewBlankRow = 1
			

		SELECT @CheckId as CheckId,
			@ContractNumber as ContractNumber, 
			@QuarterId as QuarterId,
			@CheckAmount as CheckAmount,
			@CheckNumber as CheckNumber,
			@DepositNumber as DepositNumber,
			@DateReceived as DateReceived,
			@CheckComments as CheckComments,
			@YearQuarterDescription as YearQuarterDescription,
			@IsNewBlankRow as IsNewBlankRow

		UNION

		SELECT k.ID as CheckId, k.CntrctNum as ContractNumber, k.Quarter_ID as QuarterId, k.CheckAmt as CheckAmount, 
			k.CheckNum as CheckNumber, k.DepositNum as DepositNumber, k.DateRcvd as DateReceived, k.Comments as CheckComments, y.Title as YearQuarterDescription,
			0 as IsNewBlankRow
		FROM tbl_Cntrcts_Checks k join tlkup_year_qtr y ON k.Quarter_ID = y.Quarter_ID 
		WHERE k.CntrctNum = @ContractNumber 
		ORDER BY IsNewBlankRow DESC, QuarterId DESC

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting checks for contract (WithAdd = 1).'
			goto ERROREXIT
		END

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


