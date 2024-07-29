IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractNationalPayment' )
BEGIN
	DROP PROCEDURE UpdateContractNationalPayment
END
GO

CREATE PROCEDURE UpdateContractNationalPayment
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@SRPActivityId int,
@ContractNumber nvarchar(20),
@ContractId int,
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
@Comments nvarchar(255)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserLogin nvarchar(120)
	
BEGIN TRANSACTION		

	exec dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 
	Select @error = @@error		
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert( nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	
		update a		
			set a.QuarterId = @QuarterId, 
			a.SubmissionDate = @SubmissionDate,
			a.SubmittedByUserName = @SubmittedByUserName, 
			a.PaymentAmount = @PaymentAmount, 
			a.PaymentMethod = @PaymentMethod, 
			a.PaymentSource = @PaymentSource, 
			a.TransactionId = @TransactionId,
			a.PayGovTrackingId = @PayGovTrackingId, 
			a.DepositTicketNumber = @DepositTicketNumber, 
			a.DebitVoucherNumber = @DebitVoucherNumber,
			a.CheckNumber = @CheckNumber,  
			a.SettlementDate = @SettlementDate,  
			a.Comments = @Comments,
			a.VendorLastModifiedBy = @currentUserLogin, 
			a.VendorLastModificationDate =  GETDATE(), 
			a.LastModifiedBy = @currentUserLogin,   
			a.LastModificationDate =  GETDATE() 

		from NAC_CM.dbo.CM_PaymentsReceived a
		where a.SRPActivityId = @SRPActivityId
  
	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating payment for National contract.'
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


