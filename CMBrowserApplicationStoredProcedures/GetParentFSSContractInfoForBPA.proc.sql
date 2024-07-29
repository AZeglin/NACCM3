IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetParentFSSContractInfoForBPA' )
BEGIN
	DROP PROCEDURE GetParentFSSContractInfoForBPA
END
GO

CREATE PROCEDURE GetParentFSSContractInfoForBPA
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	select c.CntrctNum,
		c.Drug_Covered, 
		c.Contractor_Name, 
		c.Dates_CntrctAward, 
		c.Dates_Effective, 
		c.Dates_CntrctExp, 
		c.Dates_Completion, 
		s.Schedule_Name, 
		p.FullName as CO_Name

	from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
	join NACSEC.dbo.SEC_UserProfile p on p.CO_ID = c.CO_ID
	where c.CntrctNum = @ContractNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error selecting parent contract info for BPA.'
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


