IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetContractResponsibleForSBAPlanFunction')
	BEGIN
		DROP  Function  GetContractResponsibleForSBAPlanFunction
	END

GO

CREATE Function GetContractResponsibleForSBAPlanFunction
(
@SBAPlanId int,
@ActiveAsOfDate datetime
)

RETURNS nvarchar(50)

AS

BEGIN

	DECLARE @ContractNumber nvarchar(50),
	@error int

	if exists( select c.CntrctNum
				from tbl_Cntrcts c
				where SBAPlanID = @SBAPlanId
				and dbo.IsContractActiveFunction( c.CntrctNum, @ActiveAsOfDate ) = 1 )
	BEGIN				

		if exists( select c.CntrctNum
					from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
					where SBAPlanID = @SBAPlanId
					and s.Division = 2 
					and PATINDEX('National',s.Type) > 0
					and dbo.IsContractActiveFunction( c.CntrctNum, @ActiveAsOfDate ) = 1
					and Estimated_Contract_Value = ( select MAX( Estimated_Contract_Value )
														from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
														where SBAPlanID = @SBAPlanId
														and s.Division = 2 
														and PATINDEX('National',s.Type) > 0 
														and dbo.IsContractActiveFunction( c.CntrctNum, @ActiveAsOfDate ) = 1 ))
		BEGIN
			select @ContractNumber = c.CntrctNum
					from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
					join NACSEC.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
					where SBAPlanID = @SBAPlanId
					and s.Division = 2 
					and PATINDEX('National',s.Type) > 0
					and dbo.IsContractActiveFunction( c.CntrctNum, @ActiveAsOfDate ) = 1
					and Estimated_Contract_Value = ( select MAX( Estimated_Contract_Value )
														from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
														where SBAPlanID = @SBAPlanId
														and s.Division = 2 
														and PATINDEX('National',s.Type) > 0 
														and dbo.IsContractActiveFunction( c.CntrctNum, @ActiveAsOfDate ) = 1 )
					select @error = @@error
					
					if @error <> 0
					BEGIN
						Select @ContractNumber = ''
					END
		
		END
		else
		BEGIN
			select @ContractNumber = c.CntrctNum
			from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
			join NACSEC.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
			where SBAPlanID = @SBAPlanId
			and s.Division = 1 
			and PATINDEX('FSS',s.Type) > 0
			and dbo.IsContractActiveFunction( c.CntrctNum, @ActiveAsOfDate ) = 1
			and Estimated_Contract_Value = ( select MAX( Estimated_Contract_Value )
												from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
												where SBAPlanID = @SBAPlanId
												and s.Division = 1 
												and PATINDEX('FSS',s.Type) > 0 
												and dbo.IsContractActiveFunction( c.CntrctNum, @ActiveAsOfDate ) = 1 )			

					select @error = @@error
					
					if @error <> 0
					BEGIN
						Select @ContractNumber = ''
					END
		
		END
	END
	else
	BEGIN
			if exists( select c.CntrctNum
					from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
					where SBAPlanID = @SBAPlanId
					and s.Division = 2 
					and PATINDEX('National',s.Type) > 0
					and Estimated_Contract_Value = ( select MAX( Estimated_Contract_Value )
														from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
														where SBAPlanID = @SBAPlanId
														and s.Division = 2 
														and PATINDEX('National',s.Type) > 0 ))
		BEGIN
			select @ContractNumber = c.CntrctNum
					from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
					join NACSEC.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
					where SBAPlanID = @SBAPlanId
					and s.Division = 2 
					and PATINDEX('National',s.Type) > 0
					and Estimated_Contract_Value = ( select MAX( Estimated_Contract_Value )
														from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
														where SBAPlanID = @SBAPlanId
														and s.Division = 2 
														and PATINDEX('National',s.Type) > 0  )
					select @error = @@error
					
					if @error <> 0
					BEGIN
						Select @ContractNumber = ''
					END
		
		END
		else
		BEGIN
			select @ContractNumber = c.CntrctNum
			from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
			join NACSEC.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
			where SBAPlanID = @SBAPlanId
			and s.Division = 1 
			and PATINDEX('FSS',s.Type) > 0
			and Estimated_Contract_Value = ( select MAX( Estimated_Contract_Value )
												from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
												where SBAPlanID = @SBAPlanId
												and s.Division = 1 
												and PATINDEX('FSS',s.Type) > 0 )

					select @error = @@error
					
					if @error <> 0
					BEGIN
						Select @ContractNumber = ''
					END
		
		END
	
	END

	return @ContractNumber

END