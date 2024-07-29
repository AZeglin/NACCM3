IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectActiveFSSContractsForBPAParent' )
BEGIN
	DROP PROCEDURE SelectActiveFSSContractsForBPAParent
END
GO

CREATE PROCEDURE SelectActiveFSSContractsForBPAParent
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier
)

AS

Declare 	@error int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	select c.CntrctNum
	from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
	where s.Division = 1 or s.Division = 4
	and dbo.IsContractActiveFunction( c.CntrctNum, GETDATE() ) = 1
	order by c.CntrctNum

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting active contracts for BPA parent list.'
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


