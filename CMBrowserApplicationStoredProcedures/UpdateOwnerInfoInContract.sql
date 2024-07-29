IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateOwnerInfoInContract')
	BEGIN
		DROP  Procedure  UpdateOwnerInfoInContract
	END

GO

CREATE Procedure UpdateOwnerInfoInContract
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@NewContractOwnerId int
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	UPDATE tbl_Cntrcts 
	SET CO_ID = @NewContractOwnerId
	where CntrctNum = @ContractNumber

	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error updating the selected contract owner for contract ' + @ContractNumber
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
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 

ENDEXIT:







END

