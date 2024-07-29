IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractGeneralContractDates' )
BEGIN
	DROP PROCEDURE UpdateContractGeneralContractDates
END
GO

CREATE PROCEDURE UpdateContractGeneralContractDates
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractId int,
@ContractNumber nvarchar(20),
@ContractAwardDate as DateTime,
@ContractEffectiveDate as DateTime,
@ContractExpirationDate as DateTime,
@ContractCompletionDate as DateTime = null,
@TerminatedByConvenience as bit, 
@TerminatedByDefault as bit, 
@TotalOptionYears as int
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

	update tbl_Cntrcts
	set Dates_CntrctAward = @ContractAwardDate,
		Dates_Effective = @ContractEffectiveDate,
		Dates_CntrctExp = @ContractExpirationDate,
		Dates_Completion = @ContractCompletionDate,
		Dates_TotOptYrs = @TotalOptionYears,
		Terminated_Convenience = @TerminatedByConvenience,
		Terminated_Default = @TerminatedByDefault,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where CntrctNum = @ContractNumber

	select @error = @@ERROR 

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating contract dates for contract ' + @ContractNumber
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


