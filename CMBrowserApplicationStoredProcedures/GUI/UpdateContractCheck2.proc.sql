IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractCheck2' )
BEGIN
	DROP PROCEDURE UpdateContractCheck2
END
GO

CREATE PROCEDURE UpdateContractCheck2
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ContractId int,
@CheckId int,
@QuarterId int,
@CheckAmount money,
@CheckNumber nvarchar(50),
@DepositTicketNumber nvarchar(20),
@DateReceived datetime,
@SettlementDate datetime,
@CheckComments nvarchar(255)
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

	UPDATE CM_Checks 
		SET QuarterId = @QuarterId, 
			CheckAmount = @CheckAmount, 
			CheckNumber = @CheckNumber, 
			DepositTicketNumber = @DepositTicketNumber, 
			DateReceived = @DateReceived, 
			SettlementDate = @SettlementDate,
			Comments = @CheckComments, 
			LastModifiedBy = @currentUserLogin, 
			LastModificationDate = GETDATE(),
			SentToGSA = 0,							-- the update wasn't sent
			SentDate = null
		WHERE CheckId = @CheckId
  
	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating check for contract.'
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


