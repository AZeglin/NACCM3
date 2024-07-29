IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSINsForSchedule' )
BEGIN
	DROP PROCEDURE SelectSINsForSchedule
END
GO

CREATE PROCEDURE SelectSINsForSchedule
(
@CurrentUser uniqueidentifier,
@ScheduleNumber int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	select SIN, Description
	from tbl_SINs
	where [Schedule_ Number] = @ScheduleNumber
	and Inactive = 0
	order by LexicalSIN

	select @error = @@ERROR, @rowCount = @@ROWCOUNT

	if @error <> 0 or @rowCount = 0
	BEGIN
		select @errorMsg = 'Error retrieving SINs for schedule number ' + CONVERT( nvarchar(5), @ScheduleNumber )
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


