IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'NormalizeContractNumberForDrugItemContractsFunction')
	BEGIN
		DROP  Function  NormalizeContractNumberForDrugItemContractsFunction
	END

GO

CREATE Function NormalizeContractNumberForDrugItemContractsFunction
(
@UserEnteredContractNumber nvarchar(20),
@ScheduleNumber int
)

returns nvarchar(20)

AS



BEGIN

	DECLARE @LeftPortion nvarchar(20),
		@HasV bit,
		@HasVA bit,
		@HasP bit,
		@HasDashP bit,
		@HasPDash bit,
		@HasDashPDash bit,
		@Division int,
		@NormalizedContractNumber nvarchar(20),
		@ShortScheduleName nvarchar(12),
		@Type nvarchar(50),
		@LastChar nvarchar(1),
		@RightPortion nvarchar(5)
		
	select @UserEnteredContractNumber = LTRIM( RTRIM( @UserEnteredContractNumber ))

	/* determine contract type from schedule */
	Select @Type = b.[Type], @ShortScheduleName = Short_Sched_Name
		from  AMMHINSQL2.nac_cm.dbo.[tlkup_Sched/Cat] b
		where b.schedule_number = @ScheduleNumber
	
	If @Type = 'FSS'
	Begin
		Set @Division = 1
	End
	Else If @Type = 'National'
	Begin
		Set @Division = 2
	End
	Else If @Type = 'BPA'
	Begin
		If @ShortScheduleName = 'FSS BPA'
		Begin
			Set @Division = 3
		End
		Else If @ShortScheduleName = 'BPA Pharm'
		Begin
			Set @Division = 4
		End
		Else
		Begin
			Set @Division = -1
		End
	End
	Else
	Begin
		Set @Division = -1
	End


	if LEN( @UserEnteredContractNumber ) > 5
	BEGIN
		if @Division = 1
		BEGIN
			select @LeftPortion = LEFT( @UserEnteredContractNumber, 6 )
			
		--	if CHARINDEX( @LeftPortion, 'V', 0 ) > 0
		--	BEGIN
		--		select @HasV = 1
		--	END
			
		--	if CHARINDEX( @LeftPortion, 'VA', 0 ) > 0
		--	BEGIN
		--		select @HasVA = 1
		--	END

			if CHARINDEX( @LeftPortion, 'P', 0 ) > 0
			BEGIN
				select @HasP = 1
			END
		
			if CHARINDEX( @LeftPortion, '-P', 0 ) > 0
			BEGIN
				select @HasDashP = 1
			END
			
			if CHARINDEX( @LeftPortion, 'P-', 0 ) > 0
			BEGIN
				select @HasPDash = 1
			END
			
			if CHARINDEX( @LeftPortion, '-P-', 0 ) > 0
			BEGIN
				select @HasDashPDash = 1
			END		
			
			/* get the right 4-5 chars */
			if @HasDashPDash = 1
			BEGIN
				select @RightPortion = substring( @UserEnteredContractNumber, charindex( '-P-', @UserEnteredContractNumber) + 3, len( @UserEnteredContractNumber ))
			END
			else
			BEGIN
				if @HasPDash = 1
				BEGIN
					select @RightPortion = substring( @UserEnteredContractNumber, charindex( 'P-', @UserEnteredContractNumber) + 2, len( @UserEnteredContractNumber ))	
				END
				else
				BEGIN
					if @HasDashP = 1 
					BEGIN
						select @RightPortion = substring( @UserEnteredContractNumber, charindex( '-P', @UserEnteredContractNumber) + 2, len( @UserEnteredContractNumber ))			
					END
					else
					BEGIN
						if @HasP = 1
						BEGIN
							select @RightPortion = substring( @UserEnteredContractNumber, charindex( 'P', @UserEnteredContractNumber) + 1, len( @UserEnteredContractNumber ))			
						END
						else /* esoteric scenarios */
						BEGIN 
							select @LastChar = RIGHT( @UserEnteredContractNumber, 1 )
							
							/* if last char is letter, then grab 5 else grab 4 */
							if PATINDEX( '%[^0-9]%', @LastChar ) > 0
							BEGIN
								select @RightPortion = UPPER(RIGHT( @UserEnteredContractNumber, 5 ))
							END
							else
							BEGIN
								select @RightPortion = RIGHT( @UserEnteredContractNumber, 4 )							
							END
						
						END
					END
				END
			END
			
			
			select @NormalizedContractNumber = 'V797P-' + @RightPortion
			

		END
		else
		BEGIN
			select @NormalizedContractNumber = @UserEnteredContractNumber		
		END
	END
	else /* <= 5 */
	BEGIN
		if @Division = 1
		BEGIN
			select @NormalizedContractNumber = 'V797P-' + UPPER(@UserEnteredContractNumber)
		END
		else
		BEGIN
			select @NormalizedContractNumber = @UserEnteredContractNumber
		END
		
	END

	return @NormalizedContractNumber

END