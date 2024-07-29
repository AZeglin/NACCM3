IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetDrugItemContractInfo')
	BEGIN
		DROP  Procedure  GetDrugItemContractInfo
	END

GO

CREATE Procedure GetDrugItemContractInfo
(
@ContractNumber nvarchar(20),
@ScheduleNumber int OUTPUT,
@IsBPA bit OUTPUT
)

As

	/* to run with NACCM for DrugItem reports remotely called from DrugItem database SP */
	/* not currently used */


DECLARE 	@error int,
		@errorMsg nvarchar(250)

BEGIN

	select @ScheduleNumber = c.Schedule_Number,
		@IsBPA = case when ( s.Type = 'BPA' ) then 1 else 0 end
	from tbl_Cntrcts c join [tlkup_Sched/Cat] s
		on c.Schedule_Number = s.Schedule_Number
	where c.CntrctNum = @ContractNumber

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving contract info from contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

END