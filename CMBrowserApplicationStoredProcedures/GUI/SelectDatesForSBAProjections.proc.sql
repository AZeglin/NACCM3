IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectDatesForSBAProjections' )
BEGIN
	DROP PROCEDURE SelectDatesForSBAProjections
END
GO

CREATE PROCEDURE SelectDatesForSBAProjections
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION





	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error'
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


