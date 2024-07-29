IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteSBAPlanDetails' )
BEGIN
	DROP PROCEDURE DeleteSBAPlanDetails
END
GO

CREATE PROCEDURE DeleteSBAPlanDetails
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@SBAPlanId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	delete tbl_sba_sbaPlan
	where SBAPlanID = @SBAPlanId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting SBA Plan.'
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


