IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ValidateOfferNumber')
	BEGIN
		DROP  Procedure  ValidateOfferNumber
	END

GO

CREATE Procedure ValidateOfferNumber
(
@OfferNumber nvarchar(30),
@ScheduleNumber int,
@OfferId int,		/* -1 = creating new offer */
@IsValidated bit OUTPUT,
@ValidationMessage nvarchar(300) OUTPUT
)

AS

DECLARE
	@Prefix nvarchar(8),		
	@NumericSuffix nchar(4),
	@FullSuffix nchar(6),
	@YearOnly nchar(2)


BEGIN
	select @IsValidated = 1
	select @ValidationMessage = ''

	/* check existence */
	/* new offer */
	if @OfferId = -1
	BEGIN
		if exists ( select OfferNumber from tbl_Offers
					where OfferNumber = LTRIM(RTRIM(@OfferNumber)) )
		BEGIN
			select @IsValidated = 0
			select @ValidationMessage = 'The entered offer number already exists in the database.'
			goto ERROREXIT	
		END	
	END
	else /* editing existing offer */
	BEGIN
		if exists ( select OfferNumber from tbl_Offers
					where OfferNumber = LTRIM(RTRIM(@OfferNumber))
					and Offer_ID <> @OfferId )
		BEGIN
			select @IsValidated = 0
			select @ValidationMessage = 'The entered offer number already exists in the database.'
			goto ERROREXIT	
		END	
	END

	select @Prefix = LEFT( LTRIM(RTRIM( @OfferNumber )), 8 )
	if @Prefix <> 'OFF-FSS-' and @Prefix <> 'EXT-FSS-'
	BEGIN
			select @IsValidated = 0
			select @ValidationMessage = 'The offer number entered does not have the correct prefix. The expected prefix format is OFF-FSS- or EXT-FSS- .'
			goto ERROREXIT					
	END
	
	
	if LEN( LTRIM(RTRIM( @OfferNumber ))) <> 12  and LEN( LTRIM(RTRIM( @OfferNumber ))) <> 14
	BEGIN
		select @IsValidated = 0
		select @ValidationMessage = 'The offer number entered does not have the correct length of 12 or 14 characters. The expected format is OFF-FSS-nnnn or OFF-FSS-YY-nnn or EXT-FSS-nnnn or EXT-FSS-YY-nnn where YY represents the year and n represents a numeric digit.'
		goto ERROREXIT					
	END
		
	/* 123456789012    12345678901234 */
	/* OFF-FSS-nnnn or OFF-FSS-YY-nnn */	

	if LEN( LTRIM(RTRIM( @OfferNumber ))) = 12 
	BEGIN
		select @NumericSuffix = RIGHT( LTRIM(RTRIM( @OfferNumber )), 4 )
	
		if ISNUMERIC( @NumericSuffix ) <> 1
		BEGIN
			select @IsValidated = 0
			select @ValidationMessage = 'The offer number entered does not have a number in the last 4 character positions. The expected format is OFF-FSS-nnnn or EXT-FSS-nnnn where n represents a numeric digit.'
			goto ERROREXIT								
		END
	END
	else
	BEGIN
		if LEN( LTRIM(RTRIM( @OfferNumber ))) = 14 
		BEGIN
			select @NumericSuffix = RIGHT( LTRIM(RTRIM( @OfferNumber )), 3 )
	
			if ISNUMERIC( @NumericSuffix ) <> 1
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The offer number entered does not have a number in the last 3 character positions. The expected format is OFF-FSS-YY-nnn or EXT-FSS-YY-nnn where n represents a numeric digit.'
				goto ERROREXIT								
			END

			select @FullSuffix = RIGHT( LTRIM(RTRIM( @OfferNumber )), 6 )
			select @YearOnly = LEFT( @FullSuffix, 2 )

			if ISNUMERIC( @YearOnly ) <> 1
			BEGIN
				select @IsValidated = 0
				select @ValidationMessage = 'The offer number entered does not have a number in the 9th and 10th character positions. The expected format is OFF-FSS-YY-nnn or EXT-FSS-YY-nnn where YY represents the year.'
				goto ERROREXIT								
			END

		END
	END
END

ERROREXIT: