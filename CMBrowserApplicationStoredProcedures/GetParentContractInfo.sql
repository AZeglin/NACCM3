IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetParentContractInfo')
	BEGIN
		DROP  Procedure  GetParentContractInfo
	END

GO

CREATE Procedure GetParentContractInfo
(
@CurrentUser uniqueidentifier,
@BPAContractNumber nvarchar(50),
@ParentContractNumber nvarchar(20) OUTPUT,
@ParentScheduleNumber int OUTPUT,
@ParentOwnerId int OUTPUT
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	/* caller expects an error if the contract does not exist */
	select @ParentContractNumber = BPA_FSS_Counterpart
	from tbl_Cntrcts
	where CntrctNum = @BPAContractNumber
	
	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting parent contract number for contract ' + @BPAContractNumber
		goto ERROREXIT
	END

	select @ParentScheduleNumber = Schedule_Number,
		@ParentOwnerId = CO_ID
	from tbl_Cntrcts
	where CntrctNum = @ParentContractNumber

	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting schedule number for parent contract ' + @ParentContractNumber
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

