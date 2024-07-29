IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSBAAssociatedContracts' )
BEGIN
	DROP PROCEDURE SelectSBAAssociatedContracts
END
GO

CREATE PROCEDURE SelectSBAAssociatedContracts
(
@CurrentUser uniqueidentifier,
@UserLogin nvarchar(120),
@ContractNumber nvarchar(20),
@SBAPlanId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@ContractResponsible nvarchar(50)

BEGIN TRANSACTION

	select @ContractResponsible = dbo.GetContractResponsibleForSBAPlanFunction( @SBAPlanId, getdate() )

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error selecting SBA plan responsible contract.'
		goto ERROREXIT
	END

	select c.Contract_Record_ID as ContractId, c.CntrctNum as ContractNumber, c.Dates_CntrctExp as ExpirationDate, c.Dates_Completion as CompletionDate, c.Contractor_Name as ContractorName, 
		c.Estimated_Contract_Value as EstimatedContractValue, u.FullName as ContractingOfficerName, s.Schedule_Name as ScheduleName, s.Schedule_Number as ScheduleNumber,
		dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) as IsActive,
		case when ( c.CntrctNum = @ContractResponsible ) then 1 else 0 end as IsResponsible

	from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number 
	join [NACSEC].[dbo].SEC_UserProfile u on c.CO_ID = u.CO_ID
	where c.SBAPlanID = @SBAPlanId
	order by IsResponsible desc, IsActive desc, c.CntrctNum asc
	
	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting SBA plan associated contracts.'
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


