IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ValidateNewContractNumber')
	BEGIN
		DROP  Procedure  ValidateNewContractNumber
	END

GO

CREATE Procedure ValidateNewContractNumber
(
@ContractNumber nvarchar(50),
@ScheduleNumber int,
@IsValidated bit OUTPUT,
@ValidationMessage nvarchar(300) OUTPUT
)

AS

DECLARE
	@Prefix nvarchar(50),
	@AsciiValue int,
	@DivisionId int,
	@Position6 nchar(1),
	@Position7 nchar(1),
	@Position78 nchar(2),
	@Position89 nchar(2),
	@Position9 nchar(1),
	@Position10 nchar(1),
	@Position11 nchar(1),
	@Position12 nchar(1),
	@NumericSuffix nchar(4), -- also interim agreements which still have 4 digits.
	@FSSNumericSuffix nchar(5),
	@CurrentYear int,
	@NumericValue int,

	/* new format elements */
	@FirstChar nchar(1),
	@NewPrefix nchar(6),
	@Year nchar(2),
	@Position13 nchar(1),
	@Position9Expected nchar(1)
	-- also used @Position9


BEGIN
	select @IsValidated = 1
	select @ValidationMessage = ''

	/* check existence */
	if exists ( select CntrctNum from tbl_Cntrcts
				where CntrctNum = LTRIM(RTRIM(@ContractNumber)) )
	BEGIN
		select @IsValidated = 0
		select @ValidationMessage = 'The entered contract number already exists in the database.'
		goto ERROREXIT	
	END	

	-- exemption for contract numbers already allocated added 10/2/2012
	if @ContractNumber = 'V797P-2259D' or
		@ContractNumber = 'V797P-2260D' or
		@ContractNumber = 'V797P-2263D' or
		@ContractNumber = 'V797P-2264D' or
		@ContractNumber = 'V797P-2278D' or
		@ContractNumber = 'V797P-2281D' or
		@ContractNumber = 'V797P-2282D' or
		@ContractNumber = 'V797P-2285D' or
		@ContractNumber = 'V797P-2287D' or
		@ContractNumber = 'V797P-2289D' or
		@ContractNumber = 'V797P-2290D' or
		@ContractNumber = 'V797P-2293D' or
		@ContractNumber = 'V797P-2302D' or
		@ContractNumber = 'V797P-2305D' or
		@ContractNumber = 'V797P-2312D' or
		@ContractNumber = 'V797P-2314D' or
		@ContractNumber = 'V797P-2317D' or
		@ContractNumber = 'V797P-2320D' or
		@ContractNumber = 'V797P-2323D' 
	BEGIN
		goto ERROREXIT
	END

	select @DivisionId = Division from [tlkup_Sched/Cat] where Schedule_Number = @ScheduleNumber

	/* base the validation path on the 1st character:  V indicates old format, which will continue to be supported for contracts in-process.  3 indicates new format introduced 10/2017 */
	select @FirstChar = LEFT( LTRIM(RTRIM( @ContractNumber )), 1 )

	/* old format */
	if @FirstChar = 'V'
	BEGIN
		/* FSS expects prefix of V797D as in V797D-ynnn */
		if @DivisionId = 1 
		BEGIN

			select @Prefix = LEFT( LTRIM(RTRIM( @ContractNumber )), 5 )
			if @Prefix <> 'V797D'
			BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have the correct prefix. The expected format is V797D-ynnnn or V797D-ynnnE where y represents the year and n represents a numeric digit'
					goto ERROREXIT					
			END
		END
		else if @DivisionId = 2
		BEGIN
			if @Prefix <> 'VA797'
			BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have the correct prefix. The expected format is VA797a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT					
			END
	
		END
		/* DALC expects prefix of VA791 */
		else if @DivisionId = 3
		BEGIN
			select @Prefix = LEFT( LTRIM(RTRIM( @ContractNumber )), 5 )
			if @Prefix <> 'VA791'
			BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have the correct prefix. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT					
			END
		END
		/* SAC expects prefix of VA119 */
		else if @DivisionId = 6
		BEGIN
			select @Prefix = LEFT( LTRIM(RTRIM( @ContractNumber )), 5 )
			if @Prefix <> 'VA119'
			BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have the correct prefix. The expected format is VA119a-nn-a-nnnn where a represents a letter ( optional ) and n represents a numeric digit'
					goto ERROREXIT					
			END
		END
		else /* default division */
		BEGIN
			if @Prefix <> 'VA797'
			BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have the correct prefix. The expected format is VA797a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT					
			END
	
		END	
	
		-- FSS
		if @DivisionId = 1 
		BEGIN

			if LEN( LTRIM(RTRIM( @ContractNumber ))) <> 11
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have the correct length of 11 characters. The expected format is V797D-ynnnn or V797D-ynnnE where y represents the year and n represents a numeric digit'
				goto ERROREXIT					
			END
		
			/* 1234567890123456 */
			/* V797P-ynnnn */
		
			select @Position6 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 6 ), 1 )
			select @NumericSuffix = LEFT( RIGHT( LTRIM(RTRIM( @ContractNumber )), 5 ), 4 )
			select @FSSNumericSuffix = RIGHT( LTRIM(RTRIM( @ContractNumber )), 5 )
			select @Position11 = RIGHT( LTRIM(RTRIM( @ContractNumber )), 1 )

			if @Position6 <> '-'
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have a dash in position 6. The expected format is V797D-ynnnn or V797D-ynnnE where y represents the year and n represents a numeric digit'
				goto ERROREXIT						
			END
		
			if( @Position11 = 'E' and ISNUMERIC( @NumericSuffix )  <> 1 ) or ( @Position11 <> 'E' and ISNUMERIC( @FSSNumericSuffix ) <> 1 )
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have a numeric suffix in the correct position. The expected format is V797D-ynnnn or V797D-ynnnE where y represents the year and n represents a numeric digit'
				goto ERROREXIT	
			END		

		END
		/* SAC VA119a-nn-a-nnnn where the first a is optional */
		else if @DivisionId = 6 
		BEGIN
			if LEN( LTRIM(RTRIM( @ContractNumber ))) <> 16 and LEN( LTRIM(RTRIM( @ContractNumber ))) <> 15
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have the correct length of 15 or 16 characters. The expected format is VA119a-nn-a-nnnn where a represents a letter ( optional ) and n represents a numeric digit'
				goto ERROREXIT					
			END
		
			if LEN( LTRIM(RTRIM( @ContractNumber ))) = 16
			BEGIN

				/* 1234567890123456 */
				/* VA119a-nn-a-nnnn */
		
				select @Position6 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 6 ), 1 )
				select @Position7 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 7 ), 1 )
				select @Position89 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 9 ), 2 )
				select @Position10 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 10 ), 1 )
				select @Position11 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 11 ), 1 )
				select @Position12 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 12 ), 1 )
				select @NumericSuffix = RIGHT( LTRIM(RTRIM( @ContractNumber )), 4 )

				select @AsciiValue = ASCII( @Position6 )
				if @AsciiValue < 65 or @AsciiValue > 90
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a letter in position 6. The expected format is VA119a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT					
				END
		
				if @Position7 <> '-'
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a dash in position 7. The expected format is VA119a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT						
				END
		
				if ISNUMERIC( @Position89 ) <> 1
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a number (representing the last 2 digits of the year) in position 8 and 9. The expected format is VA119a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT							
				END
				else -- check for a reasonable year
				BEGIN
					select @CurrentYear = convert( int, RIGHT( convert( nchar(4), YEAR( getdate() )), 2 ))
					select @NumericValue = convert( int, @Position89 )
					if @NumericValue < @CurrentYear - 10 or @NumericValue > @CurrentYear + 10
					BEGIN
						select @IsValidated = 0
						select @ValidationMessage = 'The contract number entered does not have a reasonable value for the last 2 digits of a year in position 8. The expected format is VA119a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT								
					END
				END
		
				if @Position10 <> '-'
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a dash in position 10. The expected format isVA119a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT							
				END
		
				select @AsciiValue = ASCII( @Position11 )
				if @AsciiValue < 65 or @AsciiValue > 90
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a letter in position 11. The expected format is VA119a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT						
				END
		
				if @Position12 <> '-'
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a dash in position 12. The expected format is VA119a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT								
				END
		
				if ISNUMERIC( @NumericSuffix ) <> 1
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a number in the last 4 character positions. The expected format is VA119a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT								
				END
			END
			else if LEN( LTRIM(RTRIM( @ContractNumber ))) = 15
			BEGIN

				/* 123456789012345 */
				/* VA119-nn-a-nnnn */
		
				select @Position6 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 6 ), 1 )
				select @Position78 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 8 ), 2 )
				select @Position9 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 9 ), 1 )
				select @Position10 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 10 ), 1 )
				select @Position11 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 11 ), 1 )
				select @NumericSuffix = RIGHT( LTRIM(RTRIM( @ContractNumber )), 4 )

		
		
				if @Position6 <> '-'
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a dash in position 6. The expected format is VA119-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT						
				END
		
				if ISNUMERIC( @Position78 ) <> 1
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a number (representing the last 2 digits of the year) in position 7 and 8. The expected format is VA119-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT							
				END
				else -- check for a reasonable year
				BEGIN
					select @CurrentYear = convert( int, RIGHT( convert( nchar(4), YEAR( getdate() )), 2 ))
					select @NumericValue = convert( int, @Position78 )
					if @NumericValue < @CurrentYear - 10 or @NumericValue > @CurrentYear + 10
					BEGIN
						select @IsValidated = 0
						select @ValidationMessage = 'The contract number entered does not have a reasonable value for the last 2 digits of a year in position 7. The expected format is VA119-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT								
					END
				END
		
				if @Position9 <> '-'
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a dash in position 9. The expected format is VA119-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT							
				END
		
				select @AsciiValue = ASCII( @Position10 )
				if @AsciiValue < 65 or @AsciiValue > 90
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a letter in position 10. The expected format is VA119-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT						
				END
		
				if @Position11 <> '-'
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a dash in position 11. The expected format is VA119-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT								
				END
		
				if ISNUMERIC( @NumericSuffix ) <> 1
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a number in the last 4 character positions. The expected format is VA119-nn-a-nnnn where a represents a letter and n represents a numeric digit'
					goto ERROREXIT								
				END
			END
		
		END
		/* same format for remainder for NC, DALC and default division */
		else if @DivisionId = 2  or @DivisionId = 3 or @DivisionId = 4 or @DivisionId = 5 or @DivisionId > 6
		BEGIN
			if LEN( LTRIM(RTRIM( @ContractNumber ))) <> 16
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have the correct length of 16 characters. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
				goto ERROREXIT					
			END
		
			/* 1234567890123456 */
			/* VA79na-nn-a-nnnn */
		
			select @Position6 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 6 ), 1 )
			select @Position7 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 7 ), 1 )
			select @Position89 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 9 ), 2 )
			select @Position10 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 10 ), 1 )
			select @Position11 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 11 ), 1 )
			select @Position12 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 12 ), 1 )
			select @NumericSuffix = RIGHT( LTRIM(RTRIM( @ContractNumber )), 4 )

			select @AsciiValue = ASCII( @Position6 )
			if @AsciiValue < 65 or @AsciiValue > 90
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have a letter in position 6. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
				goto ERROREXIT					
			END
		
			if @Position7 <> '-'
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have a dash in position 7. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
				goto ERROREXIT						
			END
		
			if ISNUMERIC( @Position89 ) <> 1
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have a number (representing the last 2 digits of the year) in position 8 and 9. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
				goto ERROREXIT							
			END
			else -- check for a reasonable year
			BEGIN
				select @CurrentYear = convert( int, RIGHT( convert( nchar(4), YEAR( getdate() )), 2 ))
				select @NumericValue = convert( int, @Position89 )
				if @NumericValue < @CurrentYear - 10 or @NumericValue > @CurrentYear + 10
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a reasonable value for the last 2 digits of a year in position 8. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
				goto ERROREXIT								
				END
			END
		
			if @Position10 <> '-'
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have a dash in position 10. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
				goto ERROREXIT							
			END
		
			select @AsciiValue = ASCII( @Position11 )
			if @AsciiValue < 65 or @AsciiValue > 90
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have a letter in position 11. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
				goto ERROREXIT						
			END
		
			if @Position12 <> '-'
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have a dash in position 12. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
				goto ERROREXIT								
			END
		
			if ISNUMERIC( @NumericSuffix ) <> 1
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have a number in the last 4 character positions. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
				goto ERROREXIT								
			END
		
		END
	END
	else 
	BEGIN
		/* new format */
		if @FirstChar = '3'
		BEGIN

			if LEN( LTRIM(RTRIM( @ContractNumber ))) >= 13
			BEGIN
				select @NewPrefix = LEFT( LTRIM(RTRIM( @ContractNumber )), 6 )
				select @Year = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 8 ), 2 )
				select @Position9 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 9 ), 1 )
				select @Position10 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 10 ), 1 )
				select @Position11 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 11 ), 1 )
				select @Position12 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 12 ), 1 )
				select @Position13 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 13 ), 1 )

				/* FSS is D, BPA is A, BOA is G */
				select @Position9Expected = ( case when @ScheduleNumber = 36 or @ScheduleNumber = 42 or @ScheduleNumber = 1 or @ScheduleNumber = 34 or @ScheduleNumber = 4 or @ScheduleNumber = 7 or @ScheduleNumber = 9 or @ScheduleNumber = 8 or @ScheduleNumber = 10  then 'D' else
								case when @ScheduleNumber = 48 or @ScheduleNumber = 44 or @ScheduleNumber = 39 or @ScheduleNumber = 52 or @ScheduleNumber = 53 or @ScheduleNumber = 58 then 'A' else
								case when @ScheduleNumber = 55 then 'G' else 'X'
								end end end );
				/* other schedules are as follows:  46 DALC may be either D or C, 45 NAC Front Office may be D or C,  all National which are not BPA may be D or C, all SAC which are not BPA or BOA may be D or C; */


				if @DivisionId = 1 
				BEGIN
					if @NewPrefix <> '36F797'
					BEGIN
						select @IsValidated = 0
						select @ValidationMessage = 'The contract number entered does not have the expected prefix of 36F797 for FSS'
						goto ERROREXIT		
					END				
					
					if @Position9 <> @Position9Expected
					BEGIN
						select @IsValidated = 0
						select @ValidationMessage = 'The contract number entered does not have the character in position 9 for the selected schedule: ' + @Position9Expected
						goto ERROREXIT
					END

				END
				else if @DivisionId = 3
				BEGIN
					if @NewPrefix <> '36C791'
					BEGIN
						select @IsValidated = 0
						select @ValidationMessage = 'The contract number entered does not have the expected prefix of 36C791 for DALC'
						goto ERROREXIT	
					END		
					
					if @Position9 <> @Position9Expected
					BEGIN
						select @IsValidated = 0
						select @ValidationMessage = 'The contract number entered does not have the character in position 9 for the selected schedule: ' + @Position9Expected
						goto ERROREXIT
					END	
				END
				else if @DivisionId = 4
				BEGIN
					if @NewPrefix <> '36S797'
					BEGIN
						select @IsValidated = 0
						select @ValidationMessage = 'The contract number entered does not have the expected prefix of 36S797 for NAC Front Office'
						goto ERROREXIT	
					END			

					if @Position9Expected <> 'X'
					BEGIN
						if @Position9 <> @Position9Expected
						BEGIN
							select @IsValidated = 0
							select @ValidationMessage = 'The contract number entered does not have the character in position 9 for the selected schedule: ' + @Position9Expected
							goto ERROREXIT
						END	
					END
					else if @Position9 <> 'D' and @Position9 <> 'C'
					BEGIN
							select @IsValidated = 0
							select @ValidationMessage = 'The contract number entered does not have the expected character of D or C in position 9 for the selected schedule'
							goto ERROREXIT
					END
				END
				else if @DivisionId = 6   /* example from email dated 6/11/2018:  36C10G18D0056 */
				BEGIN
					if @NewPrefix <> '36C119' and @NewPrefix <> '36C10G'
					BEGIN
						select @IsValidated = 0
						select @ValidationMessage = 'The contract number entered does not have the expected prefix of 36C119 or 36C10G for SAC'
						goto ERROREXIT	
					END		

					if @Position9Expected <> 'X'
					BEGIN
						if @Position9 <> @Position9Expected
						BEGIN
							select @IsValidated = 0
							select @ValidationMessage = 'The contract number entered does not have the character in position 9 for the selected schedule: ' + @Position9Expected
							goto ERROREXIT
						END	
					END
					else if @Position9 <> 'D' and @Position9 <> 'C'
					BEGIN
							select @IsValidated = 0
							select @ValidationMessage = 'The contract number entered does not have the expected character of D or C in position 9 for the selected schedule'
							goto ERROREXIT
					END
				END
				else if @DivisionId = 2
				BEGIN
					if @ScheduleNumber = 62 or @ScheduleNumber = 59 or @ScheduleNumber = 60 or @ScheduleNumber = 40
					BEGIN
						if @NewPrefix <> '36H797'
						BEGIN
							select @IsValidated = 0
							select @ValidationMessage = 'The contract number entered does not have the expected prefix of 36H797 for the selected National Contracts schedule'
							goto ERROREXIT		
						END		 
						if @Position9 <> 'D' and @Position9 <> 'C'
						BEGIN
								select @IsValidated = 0
								select @ValidationMessage = 'The contract number entered does not have the expected character of D or C in position 9 for the selected schedule'
								goto ERROREXIT
						END
					END
					else if @ScheduleNumber = 33
					BEGIN
						if @NewPrefix <> '36L797'
						BEGIN
							select @IsValidated = 0
							select @ValidationMessage = 'The contract number entered does not have the expected prefix of 36L797 for the selected National Contracts schedule'
							goto ERROREXIT		
						END		 
						if @Position9 <> 'D' and @Position9 <> 'C'
						BEGIN
								select @IsValidated = 0
								select @ValidationMessage = 'The contract number entered does not have the expected character of D or C in position 9 for the selected schedule'
								goto ERROREXIT
						END
					END
					else if @ScheduleNumber = 30 
					BEGIN
						if @NewPrefix <> '36E797'
						BEGIN
							select @IsValidated = 0
							select @ValidationMessage = 'The contract number entered does not have the expected prefix of 36E797 for the selected National Contracts schedule'
							goto ERROREXIT		
						END		 
						if @Position9 <> 'D' and @Position9 <> 'C'
						BEGIN
								select @IsValidated = 0
								select @ValidationMessage = 'The contract number entered does not have the expected character of D or C in position 9 for the selected schedule'
								goto ERROREXIT
						END
					END
					else if @ScheduleNumber = 39 or @ScheduleNumber = 52
					BEGIN
						if @NewPrefix <> '36E797'
						BEGIN
							select @IsValidated = 0
							select @ValidationMessage = 'The contract number entered does not have the expected prefix of 36E797 for the selected National Contracts schedule'
							goto ERROREXIT		
						END		 
						if @Position9 <> 'A' 
						BEGIN
								select @IsValidated = 0
								select @ValidationMessage = 'The contract number entered does not have the expected character of A in position 9 for the selected schedule'
								goto ERROREXIT
						END
					END
					else if @ScheduleNumber = 18 or @ScheduleNumber = 61 or @ScheduleNumber = 49
					BEGIN
						if @NewPrefix <> '36W797'
						BEGIN
							select @IsValidated = 0
							select @ValidationMessage = 'The contract number entered does not have the expected prefix of 36W797 for the selected National Contracts schedule'
							goto ERROREXIT		
						END		 
						if @Position9 <> 'D' and @Position9 <> 'C'
						BEGIN
								select @IsValidated = 0
								select @ValidationMessage = 'The contract number entered does not have the expected character of D or C in position 9 for the selected schedule'
								goto ERROREXIT
						END
					END
				END

				if ISNUMERIC( @Year ) <> 1
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a number (representing the last 2 digits of the year) in position 7 and 8. The expected format is 36annnYYannnn where YY represents the last 2 digits of a year, a represents a letter and n represents a numeric digit'
					goto ERROREXIT							
				END
				else -- check for a reasonable year
				BEGIN
					select @CurrentYear = convert( int, RIGHT( convert( nchar(4), YEAR( getdate() )), 2 ))
					select @NumericValue = convert( int, @Year )
					if @NumericValue < @CurrentYear - 10 or @NumericValue > @CurrentYear + 10
					BEGIN
						select @IsValidated = 0
						select @ValidationMessage = 'The contract number entered does not have a reasonable value for the last 2 digits of a year in positions 7 and 8. The expected format is 36annnYYannnn where YY represents the last 2 digits of a year, a represents a letter and n represents a numeric digit'
					goto ERROREXIT								
					END
				END

				if ISNUMERIC( @Position10 ) <> 1 or ISNUMERIC( @Position11 ) <> 1 or ISNUMERIC( @Position12 ) <> 1
				BEGIN
					select @IsValidated = 0
					select @ValidationMessage = 'The contract number entered does not have a number in positions 10-12. The expected format is 36annnYYannnn where YY represents the last 2 digits of a year, a represents a letter and n represents a numeric digit'
					goto ERROREXIT	
				END
			END
			else
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The contract number entered does not have the expected length. The expected formats are VA79na-nn-a-nnnn or 36annnYYannnn where a represents a letter and n represents a numeric digit and YY represents the current year'
				goto ERROREXIT		
			END
		END
		else
		BEGIN
			select @IsValidated = 0
			select @ValidationMessage = 'The contract number entered does not have the expected V or 3 in the first position. The expected formats are VA79na-nn-a-nnnn or 36a797YYannnn where a represents a letter and n represents a numeric digit and YY represents the current year'
			goto ERROREXIT		
		END

	END
END

ERROREXIT:

-- /* old validation code abandoned 11/4/2011 */
--
--	/* FSS & NC expect prefix of VA797 */
--	if @DivisionId = 1 or @DivisionId = 2
--	BEGIN
--
--		select @Prefix = LEFT( LTRIM(RTRIM( @ContractNumber )), 5 )
--		if @Prefix <> 'VA797'
--		BEGIN
--				select @IsValidated = 0
--				select @ValidationMessage = 'The contract number entered does not have the correct prefix. The expected format is VA797a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--				goto ERROREXIT					
--		END
--	END
--	/* DALC expects prefix of VA791 */
--	else if @DivisionId = 3
--	BEGIN
--		select @Prefix = LEFT( LTRIM(RTRIM( @ContractNumber )), 5 )
--		if @Prefix <> 'VA791'
--		BEGIN
--				select @IsValidated = 0
--				select @ValidationMessage = 'The contract number entered does not have the correct prefix. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--				goto ERROREXIT					
--		END
--	END
--	
--	if LEN( LTRIM(RTRIM( @ContractNumber ))) <> 16
--	BEGIN
--		select @IsValidated = 0
--		select @ValidationMessage = 'The contract number entered does not have the correct length of 16 characters. The expected format is VA79na-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--		goto ERROREXIT					
--	END
--	
--	/* 1234567890123456 */
--	/* VA79na-nn-a-nnnn */
--	
--	select @Position6 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 6 ), 1 )
--	select @Position7 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 7 ), 1 )
--	select @Position89 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 9 ), 2 )
--	select @Position10 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 10 ), 1 )
--	select @Position11 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 11 ), 1 )
--	select @Position12 = RIGHT( LEFT( LTRIM(RTRIM( @ContractNumber )), 12 ), 1 )
--	select @NumericSuffix = RIGHT( LTRIM(RTRIM( @ContractNumber )), 4 )
--
--	select @AsciiValue = ASCII( @Position6 )
--	if @AsciiValue < 65 or @AsciiValue > 90
--	BEGIN
--		select @IsValidated = 0
--		select @ValidationMessage = 'The contract number entered does not have a letter in position 6. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--		goto ERROREXIT					
--	END
--	
--	if @Position7 <> '-'
--	BEGIN
--		select @IsValidated = 0
--		select @ValidationMessage = 'The contract number entered does not have a dash in position 7. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--		goto ERROREXIT						
--	END
--	
--	if ISNUMERIC( @Position89 ) <> 1
--	BEGIN
--		select @IsValidated = 0
--		select @ValidationMessage = 'The contract number entered does not have a number (representing the last 2 digits of the year) in position 8 and 9. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--		goto ERROREXIT							
--	END
--	else -- check for a reasonable year
--	BEGIN
--		select @CurrentYear = convert( int, RIGHT( convert( nchar(4), YEAR( getdate() )), 2 ))
--		select @NumericValue = convert( int, @Position89 )
--		if @NumericValue < @CurrentYear - 10 or @NumericValue > @CurrentYear + 10
--		BEGIN
--			select @IsValidated = 0
--			select @ValidationMessage = 'The contract number entered does not have a reasonable value for the last 2 digits of a year in position 8. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--			goto ERROREXIT								
--		END
--	END
--	
--	if @Position10 <> '-'
--	BEGIN
--		select @IsValidated = 0
--		select @ValidationMessage = 'The contract number entered does not have a dash in position 10. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--		goto ERROREXIT							
--	END
--	
--	select @AsciiValue = ASCII( @Position11 )
--	if @AsciiValue < 65 or @AsciiValue > 90
--	BEGIN
--		select @IsValidated = 0
--		select @ValidationMessage = 'The contract number entered does not have a letter in position 11. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--		goto ERROREXIT						
--	END
--	
--	if @Position12 <> '-'
--	BEGIN
--		select @IsValidated = 0
--		select @ValidationMessage = 'The contract number entered does not have a dash in position 12. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--		goto ERROREXIT								
--	END
--	
--	if ISNUMERIC( @NumericSuffix ) <> 1
--	BEGIN
--		select @IsValidated = 0
--		select @ValidationMessage = 'The contract number entered does not have a number in the last 4 character positions. The expected format is VA791a-nn-a-nnnn where a represents a letter and n represents a numeric digit'
--		goto ERROREXIT								
--	END
--	
--END
--
--ERROREXIT:

/* old validation code abandoned 10/1/2011 */

--	/* FSS expects prefix of V797P- with nnnna suffix, except for FSSBPA schedule = 48 
--	if @DivisionId = 1
--	BEGIN
--		select @Prefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 6 )
---		if @Prefix = 'V797P-'
--		BEGIN
--			if @ScheduleNumber = 48
--			BEGIN
--				select @IsValidated = 0
--				select @ValidationMessage = 'The contract number entered for the FSS BPA Contract does not match the expected format of 797-FSSBPA-yy-nnnn where yy represents the last 2 digits of the year and n represents a numeric digit'
--				goto ERROREXIT					
--			END
--		
--			if LEN(LTRIM(RTRIM(@ContractNumber))) = 11
--			BEGIN
--				select @Suffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 7, 5 )
--				select @AsciiValue = ASCII( RIGHT( LTRIM(RTRIM(@ContractNumber)), 1 ))
--				select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 7, 4 )
--				
--				if ISNUMERIC( @NumericSuffix ) <> 1 or @AsciiValue < 65 or @AsciiValue > 90
--				BEGIN
--					select @IsValidated = 0
--					select @ValidationMessage = 'The contract number entered for the FSS Contract does not match the expected format of V797P-nnnnA where n represents a numeric digit and A represents an upper case letter'
--					goto ERROREXIT		
--				END
--				
--			END
--			else
--			BEGIN
--				select @IsValidated = 0
--				select @ValidationMessage = 'The contract number entered for the FSS Contract does not have the correct length e.g., V797P-nnnnA.'
--				goto ERROREXIT		
--		
--			END
--		END
--		else
--		BEGIN
--			select @AltPrefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 11 )
--			if @AltPrefix = '797-FSSBPA-'
--			BEGIN
--				if @ScheduleNumber <> 48
--				BEGIN
--					select @IsValidated = 0
--					select @ValidationMessage = 'The contract number entered for the FSS Contract does not match the expected format of V797P-nnnnA where n represents a numeric digit and A represents an upper case letter'
--					goto ERROREXIT					
--				END
--				-- valid prefix for FSSBPA schedule
--				-- now validate the suffix yy-nnnn
--				if LEN(LTRIM(RTRIM(@ContractNumber))) = 18
--				BEGIN
--					select @YearSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 12, 2 )
--					select @AsciiValue = ASCII( SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 14, 1 ))
--					select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 15, 4 )
--					
--					if ISNUMERIC( @NumericSuffix ) <> 1 or ISNUMERIC( @YearSuffix ) <> 1 or @AsciiValue <> 45 -- dash -
--					BEGIN
--						select @IsValidated = 0
--						select @ValidationMessage = 'The contract number entered for the FSS BPA Contract does not match the expected format of 797-FSSBPA-yy-nnnn where yy represents the last 2 digits of the year and n represents a numeric digit'
--						goto ERROREXIT		
--					END
--					
--				END
--				else
--				BEGIN
--					select @IsValidated = 0
--					select @ValidationMessage = 'The contract number entered for the FSS BPA Contract does not have the correct length e.g., 797-FSSBPA-yy-nnnn.'
--					goto ERROREXIT		
--			
--				END			
--				
--		
--			END
--			else
--			BEGIN
--				select @IsValidated = 0
--				select @ValidationMessage = 'The contract number entered for the FSS Contract does not match the expected prefix of V797P- or 797-FSSBPA-'
--				goto ERROREXIT		
--			END
--		END
--		
--	END	
--	/* NC expects prefix of VA797-P- with nnnn suffix, except for HTME and 3 CMOP schedules and pharma BPA 
--	else if @DivisionId = 2
--	BEGIN
--		/* regular NC 
--		if @ScheduleNumber <> 40 and @ScheduleNumber <> 37 and @ScheduleNumber <> 47 and @ScheduleNumber <> 50 and @ScheduleNumber <> 14 and @ScheduleNumber <> 15 and @ScheduleNumber <> 39 
--		BEGIN
--			select @Prefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 8 )
--			if @Prefix <> 'VA797-P-'
--			BEGIN
--				select @IsValidated = 0
--				select @ValidationMessage = 'The contract number entered for the National Contract does not match the expected prefix of VA797-P-'
--				goto ERROREXIT		
--			END
--			
--			if LEN(LTRIM(RTRIM(@ContractNumber))) = 12
--			BEGIN
--				select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 9, 4 )
--				
--				if ISNUMERIC( @NumericSuffix ) <> 1 
--				BEGIN
----					select @IsValidated = 0
--					select @ValidationMessage = 'The contract number entered for the National Contract does not match the expected format of VA797-P-nnnn where n represents a numeric digit'
--					goto ERROREXIT		
--				END
--				
--			END
--			else
--			BEGIN
--				select @IsValidated = 0
--				select @ValidationMessage = 'The contract number entered for the National Contract does not have the correct length e.g., VA797-P-nnnn.'
--				goto ERROREXIT		
--		
--			END
--		END
--		else 
----		BEGIN
--			/* HTME 
--			if @ScheduleNumber = 40 
--			BEGIN
--				select @Prefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 6 )
--				if @Prefix <> 'V797P-'
--				BEGIN
--					select @IsValidated = 0
--					select @ValidationMessage = 'The contract number entered for the National Contract ( HTME ) does not match the expected prefix of V797P-'
--					goto ERROREXIT		
--				END
--		
----				if LEN(LTRIM(RTRIM(@ContractNumber))) = 11
--				BEGIN
--					select @Suffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 7, 5 )
--					select @AsciiValue = ASCII( RIGHT( LTRIM(RTRIM(@ContractNumber)), 1 ))
--					select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 7, 4 )
--					
--					if ISNUMERIC( @NumericSuffix ) <> 1 or @AsciiValue < 65 or @AsciiValue > 90
--					BEGIN
--						select @IsValidated = 0
--						select @ValidationMessage = 'The contract number entered for the National Contract ( HTME ) does not match the expected format of V797P-nnnnA where n represents a numeric digit and A represents an upper case letter'
--						goto ERROREXIT		
--					END
--					
--				END
--				else
--				BEGIN
--					select @IsValidated = 0
--					select @ValidationMessage = 'The contract number entered for the National Contract ( HTME ) does not have the correct length e.g., V797P-nnnnA.'
--					goto ERROREXIT		
--			
----				END	
--			END
--			else 
--			BEGIN
--				/* BPA 'VA797-BP-nnnn' 
--				if @ScheduleNumber = 15 or @ScheduleNumber = 39
--				BEGIN
--					select @Prefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 9 )
--					if @Prefix <> 'VA797-BP-'
--					BEGIN
--						select @IsValidated = 0
--						select @ValidationMessage = 'The contract number entered for the National Contract ( BPA ) does not match the expected prefix of VA797-BP-'
--						goto ERROREXIT		
--					END
--			
--					if LEN(LTRIM(RTRIM(@ContractNumber))) = 13
--					BEGIN
--						select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 10, 4 )
--						
--						if ISNUMERIC( @NumericSuffix ) <> 1 
--						BEGIN
--							select @IsValidated = 0
--							select @ValidationMessage = 'The contract number entered for the National Contract ( BPA ) does not match the expected format of VA797-BP-nnnn where n represents a numeric digit'
--							goto ERROREXIT		
--						END
--						
--					END
--					else
--					BEGIN
--						select @IsValidated = 0
--						select @ValidationMessage = 'The contract number entered for the National Contract ( BPA ) does not have the correct length e.g., VA797-BP-nnnn.'
--						goto ERROREXIT		
--				
----					END	
--				END
--				else
--				BEGIN
--					/* BOA 
--					if @ScheduleNumber = 14
--					BEGIN
--						select @Prefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 9 )
--						if @Prefix <> 'VA797-BO-'
--						BEGIN
--							select @IsValidated = 0
--							select @ValidationMessage = 'The contract number entered for the National Contract ( BOA ) does not match the expected prefix of VA797-BO-'
--							goto ERROREXIT		
--						END
----				
--						if LEN(LTRIM(RTRIM(@ContractNumber))) = 13
--						BEGIN
--							select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 10, 4 )
--							
--							if ISNUMERIC( @NumericSuffix ) <> 1 
--							BEGIN
--								select @IsValidated = 0
--								select @ValidationMessage = 'The contract number entered for the National Contract ( BOA ) does not match the expected format of VA797-BO-nnnn where n represents a numeric digit'
--								goto ERROREXIT		
--							END
--							
--						END
--						else
----						BEGIN
--						select @IsValidated = 0
----							select @ValidationMessage = 'The contract number entered for the National Contract ( BOA ) does not have the correct length e.g., VA797-BO-nnnn.'
--							goto ERROREXIT		
--					
--						END	
--					END
--					else
--					BEGIN
--						/* CMOP BOA VA797M-BO-nnnn 
--						if @ScheduleNumber = 47
--						BEGIN
--							select @Prefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 10 )
--							if @Prefix <> 'VA797M-BO-'
--							BEGIN
--								select @IsValidated = 0
--								select @ValidationMessage = 'The contract number entered for the CMOP BOA Contract does not match the expected prefix of VA797M-BO-'
--								goto ERROREXIT		
--							END
--
----							if LEN(LTRIM(RTRIM(@ContractNumber))) = 14
--							BEGIN
--								select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 11, 4 )
--								
--								if ISNUMERIC( @NumericSuffix ) <> 1 
--								BEGIN
--									select @IsValidated = 0
--									select @ValidationMessage = 'The contract number entered for the CMOP BOA Contract does not match the expected format of VA797M-BO-nnnn where n represents a numeric digit'
--									goto ERROREXIT		
--								END
--								
--							END
--							else
--							BEGIN
--								select @IsValidated = 0
--								select @ValidationMessage = 'The contract number entered for the CMOP BOA Contract does not have the correct length e.g., VA797M-BO-nnnn.'
--------								goto ERROREXIT		
--							END	
--						END
--						else
--						-- CMOP VA797M-P-nnnn   OR VA797M-BP-nnnn
----						BEGIN						
--							--select @Prefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 9 )
----							select @AltPrefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 10 )
--							if @Prefix <> 'VA797M-P-' AND @AltPrefix <> 'VA797M-BP-'
--							BEGIN
--								select @IsValidated = 0
--								select @ValidationMessage = 'The contract number entered for the CMOP Contract does not match the expected prefix of VA797M-P- or VA797M-BP-'
--								goto ERROREXIT		
--							END
--
--							if LEN(LTRIM(RTRIM(@ContractNumber))) = 13 
--							BEGIN
--								select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 10, 4 )
--								
--								if ISNUMERIC( @NumericSuffix ) <> 1 
--								BEGIN
--									select @IsValidated = 0
--									select @ValidationMessage = 'The contract number entered for the CMOP Contract does not match the expected format of VA797M-P-nnnn or VA797M-BP-nnnn where n represents a numeric digit'
--									goto ERROREXIT		
--								END
--								
--							END
--							else
----							BEGIN
--								if LEN(LTRIM(RTRIM(@ContractNumber))) = 14 
--								BEGIN
----									select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 11, 4 )
--									
--									if ISNUMERIC( @NumericSuffix ) <> 1 
--									BEGIN
--										select @IsValidated = 0
--										select @ValidationMessage = 'The contract number entered for the CMOP Contract does not match the expected format of VA797M-P-nnnn or VA797M-BP-nnnn where n represents a numeric digit'
--										goto ERROREXIT		
--									END							
--								END
----								else
--								BEGIN
----									select @IsValidated = 0
--									--select @ValidationMessage = 'The contract number entered for the CMOP Contract does not have the correct length e.g., VA797M-P-nnnn or VA797M-BP-nnnn.'
--									goto ERROREXIT		
--								END
--							END
--						END
--					END
--				END
----			END
--		END
--	END
--	/* DALC expects prefix of VA791-P- with nnnn suffix 
--	else if @DivisionId = 3
--	BEGIN
--		select @Prefix = LEFT( LTRIM(RTRIM(@ContractNumber)), 8 )
--		if @Prefix <> 'VA791-P-'
----		BEGIN
--			select @IsValidated = 0
--			select @ValidationMessage = 'The contract number entered for the DALC Contract does not match the expected prefix of VA791-P-'
--			goto ERROREXIT		
--		END
--
--		if LEN(LTRIM(RTRIM(@ContractNumber))) = 12
--		BEGIN
--			select @NumericSuffix = SUBSTRING( LTRIM(RTRIM(@ContractNumber)), 9, 4 )
--			
--			if ISNUMERIC( @NumericSuffix ) <> 1 
--			BEGIN
--				select @IsValidated = 0
--				select @ValidationMessage = 'The contract number entered for the DALC Contract does not match the expected format of VA791-P-nnnn where n represents a numeric digit'
--				goto ERROREXIT		
--			END
--			
--		END
------		else
--		BEGIN
----			select @IsValidated = 0
--			--select @ValidationMessage = 'The contract number entered for the DALC Contract does not have the correct length e.g., VA791-P-nnnn.'
--			goto ERROREXIT		
--		END
--	
--	END
--	





