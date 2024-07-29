IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractGeneralContractAttributes' )
BEGIN
	DROP PROCEDURE UpdateContractGeneralContractAttributes
END
GO

CREATE PROCEDURE UpdateContractGeneralContractAttributes
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractId int,
@ContractNumber nvarchar(20),
@ContractDescription as nvarchar(50), 
@VADOD as bit,
@PrimeVendor as bit, 
@TradeAgreementActCompliance as nchar(1), 
@StimulusAct as bit,
@Standardized  as bit
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

	--waitfor delay '00:00:09'

	update tbl_Cntrcts
	set Drug_Covered = @ContractDescription,
		VA_DOD = @VADOD,
		PV_Participation = @PrimeVendor,
		TradeAgreementActCompliance = @TradeAgreementActCompliance,
		StimulusAct = @StimulusAct,
		Standardized = @Standardized,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where CntrctNum = @ContractNumber

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating contract attributes for contract ' + @ContractNumber
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


