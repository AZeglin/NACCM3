IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetDivisionId')
	BEGIN
		DROP  Function  GetDivisionId
	END

GO

CREATE function GetDivisionId
(
@ContractId int
)

returns int

as

BEGIN


DECLARE @ContractRecordId int, 
		@division int, @type nvarchar(20),
		@shortScheduleName nvarchar(100)

	Select @ContractRecordId = NACCMContractId
	From DI_Contracts
	Where ContractId = @ContractId

	If @ContractRecordId is null
	Begin
		Select @division =  -1
	End
	Else
	Begin
		Select @type = b.[Type], @shortScheduleName = Short_Sched_Name
		from nac_cm.dbo.tbl_cntrcts a
		join nac_cm.dbo.[tlkup_Sched/Cat] b
		on a.schedule_number = b.schedule_number
		where a.contract_record_id = @ContractRecordId
	
		If @type = 'FSS'
		Begin
			Set @division = 1
		End
		Else If @type = 'National'
		Begin
			Set @division = 2
		End
		Else If @type = 'BPA'
		Begin
			If @shortScheduleName = 'FSS BPA'
			Begin
				Set @division = 3
			End
			Else If @shortScheduleName = 'BPA Pharm'
			Begin
				Set @division = 4
			End
			Else
			Begin
				Set @division = -1
			End
		End
		Else
		Begin
			Set @division = -1
		End

	End

	return @division
END

GO

GRANT EXECUTE ON [dbo].[GetDivisionId] TO [VHAMASTER\VHAPBHConeR] as [dbo]
GO
