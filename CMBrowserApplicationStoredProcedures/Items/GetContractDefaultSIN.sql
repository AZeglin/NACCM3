IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetContractDefaultSIN')
	BEGIN
		DROP  Procedure  GetContractDefaultSIN
	END

GO

CREATE PROCEDURE GetContractDefaultSIN
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@SIN varchar(10) OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserName nvarchar(120)
			
BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserName OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login ' + @ContractNumber
		goto ERROREXIT
	END

	select @SIN = [SINs] from tbl_Cntrcts_SINs where CntrctNum = @ContractNumber and Inactive = 0

	select @error = @@ERROR, @rowCount = @@ROWCOUNT

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error encountered when attempting to retrieve default SIN for contract ' + @ContractNumber
		goto ERROREXIT
	END

GOTO OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 ) 

	IF @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
	/* only rollback iff this the highest level */ 
		ROLLBACK TRANSACTION
	END

	RETURN (-1)

OKEXIT:

	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)

