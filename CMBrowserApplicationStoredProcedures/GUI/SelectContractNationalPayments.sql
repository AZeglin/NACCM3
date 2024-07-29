IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractNationalPayments' )
BEGIN
	DROP PROCEDURE SelectContractNationalPayments
END
GO

CREATE PROCEDURE SelectContractNationalPayments
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@ContractId int,
@WithAdd bit = 0
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@QuarterId int,
		@PaymentAmount decimal(18,2), 
		@SubmissionDate datetime, 
		@SubmittedByUserName nvarchar(100), 
		@PaymentMethod nvarchar(50), 
		@PaymentSource nvarchar(20),
		@TransactionId nvarchar(120), 
		@PayGovTrackingId nvarchar(120), 
		@CheckNumber nvarchar(50),
		@DepositTicketNumber nvarchar(50),
		@DebitVoucherNumber nvarchar(50),
		@SettlementDate datetime,
		@Comments nvarchar(255),
		@SRPActivityId int,
		@IsNewBlankRow bit,
		@YearQuarterDescription nvarchar(20)


BEGIN TRANSACTION
						


	if @WithAdd = 0
	BEGIN

		SELECT k.SRPActivityId as SRPActivityId, k.ContractNumber as ContractNumber, k.ContractId as ContractId, k.QuarterId as QuarterId, k.PaymentAmount as PaymentAmount, k.SubmissionDate as SubmissionDate, k.SubmittedByUserName as SubmittedByUserName, k.PaymentMethod as PaymentMethod,
			k.PaymentSource as PaymentSource, k.TransactionId as TransactionId, k.PayGovTrackingId as PayGovTrackingId, k.DepositTicketNumber as DepositTicketNumber, k.DebitVoucherNumber as DebitVoucherNumber, k.CheckNumber as CheckNumber,
			k.SettlementDate as SettlementDate, k.Comments as Comments,
			y.Title as YearQuarterDescription,
			0 as IsNewBlankRow
		FROM CM_PaymentsReceived k join tlkup_year_qtr y ON k.QuarterId = y.Quarter_ID 
		WHERE k.ContractNumber = @ContractNumber 
		ORDER BY k.QuarterId DESC

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting payments for National contract (WithAdd = 0).'
			goto ERROREXIT
		END
	END
	else
	BEGIN

		/* blank row definition */
		select @SRPActivityId = -1,
			@QuarterId = -1,
			@PaymentAmount = 0,
			@SubmissionDate = getdate(),
			@SubmittedByUserName = '',
			@PaymentMethod = '',
			@PaymentSource = 'VA',
			@TransactionId = '',
			@PayGovTrackingId = '',

			@CheckNumber = '',
			@DepositTicketNumber = '',
			@DebitVoucherNumber = '',

			@SettlementDate = GETDATE(),
			@Comments = '',
			@YearQuarterDescription = '',
			@IsNewBlankRow = 1
			

		SELECT @SRPActivityId as SRPActivityId,
			@ContractNumber as ContractNumber, 
			@ContractId as ContractId,
			@QuarterId as QuarterId,
			@PaymentAmount as PaymentAmount,
			@SubmissionDate as SubmissionDate,
			@SubmittedByUserName as SubmittedByUserName,
			@PaymentMethod as PaymentMethod,
			@PaymentSource as PaymentSource,
			@TransactionId as TransactionId,
			@PayGovTrackingId as PayGovTrackingId,
			
			@DepositTicketNumber as DepositTicketNumber,
			@DebitVoucherNumber as DebitVoucherNumber,
			@CheckNumber as CheckNumber,

			@SettlementDate as SettlementDate,
			@Comments as Comments,			
			@YearQuarterDescription as YearQuarterDescription,
			@IsNewBlankRow as IsNewBlankRow

		UNION

		SELECT k.SRPActivityId as SRPActivityId, k.ContractNumber as ContractNumber, k.ContractId as ContractId, k.QuarterId as QuarterId, k.PaymentAmount as PaymentAmount, k.SubmissionDate as SubmissionDate, k.SubmittedByUserName as SubmittedByUserName, k.PaymentMethod as PaymentMethod,
			k.PaymentSource as PaymentSource, k.TransactionId as TransactionId, k.PayGovTrackingId as PayGovTrackingId, k.DepositTicketNumber as DepositTicketNumber, k.DebitVoucherNumber as DebitVoucherNumber, k.CheckNumber as CheckNumber,
			k.SettlementDate as SettlementDate, k.Comments as Comments,
			y.Title as YearQuarterDescription,
			0 as IsNewBlankRow
		FROM CM_PaymentsReceived k join tlkup_year_qtr y ON k.QuarterId = y.Quarter_ID 
		WHERE k.ContractNumber = @ContractNumber 
	
	    ORDER BY IsNewBlankRow DESC, QuarterId DESC


		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting payments for National contract (WithAdd = 1).'
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


