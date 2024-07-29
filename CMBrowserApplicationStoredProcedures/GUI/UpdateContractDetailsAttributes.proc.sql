IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractDetailsAttributes' )
BEGIN
	DROP PROCEDURE UpdateContractDetailsAttributes
END
GO

CREATE PROCEDURE UpdateContractDetailsAttributes
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractId int,
@ContractNumber nvarchar(20),
@EstimatedContractValue money, 
@FPRFreeFormatDateString nvarchar(255), 
@IffTypeId int,
@SolicitationNumber nvarchar(40),
@TrackingCustomerName nvarchar(255),
@Ratio nvarchar(255), 
@MinimumOrder nvarchar(255)
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
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	update tbl_Cntrcts
	set Estimated_Contract_Value = @EstimatedContractValue,
		Mininum_Order = @MinimumOrder,
		BF_Offer = @FPRFreeFormatDateString,
		IFF_Type_ID = @IffTypeId,
		Solicitation_Number = @SolicitationNumber,
		Tracking_Customer = @TrackingCustomerName,
		Ratio = @Ratio,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where CntrctNum = @ContractNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating detailed attributes for contract ' + @ContractNumber
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


