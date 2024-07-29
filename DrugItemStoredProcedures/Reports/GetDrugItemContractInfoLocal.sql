IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetDrugItemContractInfoLocal')
	BEGIN
		DROP  Procedure  GetDrugItemContractInfoLocal
	END

GO

CREATE Procedure GetDrugItemContractInfoLocal
(
@ContractNumber nvarchar(20),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@ScheduleNumber int OUTPUT,
@IsBPA bit OUTPUT,
@Division int OUTPUT
)

As


DECLARE 	@error int,
			@errorMsg nvarchar(250),
			@SQL nvarchar(2400),
			@SQLParms nvarchar(1000)


BEGIN

	select @SQL = N'select @ScheduleNumber_parm = c.Schedule_Number,
					@IsBPA_parm = case when ( s.Type = ''BPA'' ) then 1 else 0 end,
					@Division_parm = s.Division
				from [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].dbo.tbl_Cntrcts c 
					join [' + @NACCMServerName + '].[' + @NACCMDatabaseName + '].dbo.[tlkup_Sched/Cat] s
						on c.Schedule_Number = s.Schedule_Number
				where c.CntrctNum = @ContractNumber_parm'
	
	select @SQLParms = N'@ContractNumber_parm nvarchar(50), @ScheduleNumber_parm int OUTPUT, @IsBPA_parm bit OUTPUT, @Division_parm int OUTPUT'
	
	Exec SP_executeSQL @SQL, @SQLParms, @ContractNumber_parm = @ContractNumber, @ScheduleNumber_parm = @ScheduleNumber OUTPUT, @IsBPA_parm = @IsBPA OUTPUT, @Division_parm = @Division OUTPUT

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving contract info from contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
	
END