IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetCustomDateForRebate' )
BEGIN
	DROP PROCEDURE GetCustomDateForRebate
END
GO

CREATE PROCEDURE GetCustomDateForRebate
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@RebateId int,
@CustomStartDate datetime OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	select @CustomStartDate = CustomStartDate
	from tbl_Rebates
	where RebateId = @RebateId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error getting custom start date for rebate id=' + CONVERT( nvarchar(14), @RebateId )
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


