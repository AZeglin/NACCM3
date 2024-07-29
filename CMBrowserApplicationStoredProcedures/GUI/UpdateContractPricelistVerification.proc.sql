IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractPricelistVerification' )
BEGIN
	DROP PROCEDURE UpdateContractPricelistVerification
END
GO

CREATE PROCEDURE UpdateContractPricelistVerification
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractId int,
@ContractNumber nvarchar(20),
@PricelistVerificationDate datetime,
@PricelistVerifiedBy nvarchar(25),
@CurrentModNumber nvarchar(20),
@IsPricelistVerified bit
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
	set Verification_Date = @PricelistVerificationDate,
		Verified_By = @PricelistVerifiedBy,
		Current_Mod_Number = @CurrentModNumber,
		Pricelist_Verified = @IsPricelistVerified,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where CntrctNum = @ContractNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating pricelist verification information for contract ' + @ContractNumber
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


