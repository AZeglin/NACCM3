IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectAssociatedBPAContracts' )
BEGIN
	DROP PROCEDURE SelectAssociatedBPAContracts
END
GO

CREATE PROCEDURE SelectAssociatedBPAContracts
(
@CurrentUser uniqueidentifier,
@UserLogin nvarchar(120),
@ContractNumber nvarchar(20)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION


	select c.Contract_Record_ID as ContractId, c.CntrctNum as ContractNumber, c.Dates_CntrctAward as AwardDate, c.Dates_Effective as EffectiveDate, c.Dates_CntrctExp as ExpirationDate, c.Dates_Completion as CompletionDate, 
	c.Contractor_Name as ContractorName, u.FullName as ContractingOfficerName, s.Schedule_Name as ScheduleName, s.Schedule_Number as ScheduleNumber,
	dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) as IsActive

	from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number 
	join [NACSEC].[dbo].SEC_UserProfile u on c.CO_ID = u.CO_ID
	where c.BPA_FSS_Counterpart = @ContractNumber
	order by IsActive desc, c.CntrctNum asc
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting associated BPA contracts.'
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


