IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateSBAPlanInfoInContract')
	BEGIN
		DROP  Procedure  UpdateSBAPlanInfoInContract
	END

GO

CREATE Procedure UpdateSBAPlanInfoInContract
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@SBAPlanId int,
@SBAPlanExemptStatus bit
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	if @SBAPlanId <> -1
	BEGIN
		UPDATE tbl_Cntrcts 
		SET SBAPlanID = @SBAPlanId, 
		SBA_Plan_Exempt = @SBAPlanExemptStatus 
		where CntrctNum = @ContractNumber

		select @error = @@error, @rowcount = @@rowcount
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error updating sba plan id and exempt status for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else
		BEGIN
		UPDATE tbl_Cntrcts 
		SET SBA_Plan_Exempt = @SBAPlanExemptStatus 
		where CntrctNum = @ContractNumber

		select @error = @@error, @rowcount = @@rowcount
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Error updating sba exempt status for contract ' + @ContractNumber
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