IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetContractResponsibleForSBAPlan')
	BEGIN
		DROP  Procedure  GetContractResponsibleForSBAPlan
	END

GO

CREATE Procedure GetContractResponsibleForSBAPlan
(
@SBAPlanId int,
@ContractNumber nvarchar(50) OUTPUT,
@COID int OUTPUT,
@COName nvarchar(50) OUTPUT
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	if exists( select c.CntrctNum
				from tbl_Cntrcts c
				where SBAPlanID = @SBAPlanId
				and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 )
	BEGIN				

		if exists( select c.CntrctNum
					from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
					where SBAPlanID = @SBAPlanId
					and s.Division = 2 
					and PATINDEX('National',s.Type) > 0
					and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
					and Estimated_Contract_Value = ( select MAX( Estimated_Contract_Value )
														from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
														where SBAPlanID = @SBAPlanId
														and s.Division = 2 
														and PATINDEX('National',s.Type) > 0 
														and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 ))
		BEGIN
			select @ContractNumber = c.CntrctNum,
					@COID = p.CO_ID,
					@COName = p.FullName
					from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
					join NACSEC.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
					where SBAPlanID = @SBAPlanId
					and s.Division = 2 
					and PATINDEX('National',s.Type) > 0
					and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
					and Estimated_Contract_Value = ( select MAX( Estimated_Contract_Value )
														from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
														where SBAPlanID = @SBAPlanId
														and s.Division = 2 
														and PATINDEX('National',s.Type) > 0 
														and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 )
					select @error = @@error
					
					if @error <> 0
					BEGIN
						select @errorMsg = 'Error finding ( national ) contract responsible for SBAPlanId ' + convert( nvarchar( 20 ), @SBAPlanId )
						raiserror( @errorMsg, 16, 1 )
					END
		
		END
		else
		BEGIN
			select @ContractNumber = c.CntrctNum,
					@COID = p.CO_ID,
					@COName = p.FullName
			from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
			join NACSEC.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
			where SBAPlanID = @SBAPlanId
			and s.Division = 1 
			and PATINDEX('FSS',s.Type) > 0
			and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1
			and Estimated_Contract_Value = ( select MAX( Estimated_Contract_Value )
												from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
												where SBAPlanID = @SBAPlanId
												and s.Division = 1 
												and PATINDEX('FSS',s.Type) > 0 
												and dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) = 1 )			

					select @error = @@error
					
					if @error <> 0
					BEGIN
						select @errorMsg = 'Error finding ( fss ) contract responsible for SBAPlanId ' + convert( nvarchar( 20 ), @SBAPlanId )
						raiserror( @errorMsg, 16, 1 )
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
			select @ContractNumber = c.CntrctNum,
					@COID = p.CO_ID,
					@COName = p.FullName
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
						select @errorMsg = 'Error finding ( national ) contract responsible for SBAPlanId ' + convert( nvarchar( 20 ), @SBAPlanId )
						raiserror( @errorMsg, 16, 1 )
					END
		
		END
		else
		BEGIN
			select @ContractNumber = c.CntrctNum,
					@COID = p.CO_ID,
					@COName = p.FullName
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
						select @errorMsg = 'Error finding ( fss ) contract responsible for SBAPlanId ' + convert( nvarchar( 20 ), @SBAPlanId )
						raiserror( @errorMsg, 16, 1 )
					END
		
		END
	
	END
END