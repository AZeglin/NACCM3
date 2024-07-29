IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractDetailsDiscount' )
BEGIN
	DROP PROCEDURE UpdateContractDetailsDiscount
END
GO

CREATE PROCEDURE UpdateContractDetailsDiscount
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractId int,
@ContractNumber nvarchar(20),
@BasicDiscount nvarchar(255),
@AdditionalDiscount nvarchar(255), 
@EndOfYearDiscount nvarchar(255),
@PromptPayDiscount nvarchar(255), 
@QuantityDiscount nvarchar(255), 
@CreditCardDiscount nvarchar(255)
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
	set Discount_Basic = @BasicDiscount,
		Discount_Credit_Card = @CreditCardDiscount,
		Discount_Prompt_Pay = @PromptPayDiscount,
		Discount_Quantity = @QuantityDiscount,
		Annual_Rebate = @EndOfYearDiscount,
		Incentive_Description = @AdditionalDiscount,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where CntrctNum = @ContractNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating discount information for contract ' + @ContractNumber
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


