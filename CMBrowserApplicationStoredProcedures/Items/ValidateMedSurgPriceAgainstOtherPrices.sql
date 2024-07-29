IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ValidateMedSurgPriceAgainstOtherPrices]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [ValidateMedSurgPriceAgainstOtherPrices]
GO
CREATE PROCEDURE ValidateMedSurgPriceAgainstOtherPrices
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ContractId int,
@ItemPriceId int,
@ItemId int,
@PriceStartDate datetime,         
@PriceStopDate datetime,      
@IsTemporary bit,
@IsPriceOk bit OUTPUT,
@UserCanOverride bit OUTPUT,
@ValidationMessage nvarchar(1250) OUTPUT      
)

AS

DECLARE 	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@TPRAlwaysHasBasePrice bit


BEGIN TRANSACTION

	select @IsPriceOk = 0
	select @UserCanOverride = 0
	select @ValidationMessage = ''
	select @TPRAlwaysHasBasePrice = 0

	/* the only validation is for date overlap for temporary with temporary and non-temp with non-temp */

	-- if price is being updated
	if @ItemPriceId <> -1
	BEGIN
		if exists ( select Price from CM_ItemPrice 
			where (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
				PriceStartDate between @PriceStartDate and @PriceStopDate or
				PriceStopDate between @PriceStartDate and @PriceStopDate or
				( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
				( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
				PriceStartDate = @PriceStopDate  or
				PriceStopDate = @PriceStartDate )
			and ItemId = @ItemId
			and IsTemporary = @IsTemporary
			and ItemPriceId <> @ItemPriceId )
		BEGIN
			select @IsPriceOk = 0
			select @ValidationMessage = 'The price dates overlap the dates of an existing price of the same type.';
		END	
		else
		BEGIN

			/* if TPR, check for base price */
			if @IsTemporary = 1
			BEGIN
				select @TPRAlwaysHasBasePrice = dbo.GetMedSurgTPRAlwaysHasBasePriceFunction( @ItemId, @PriceStartDate, @PriceStopDate, @IsTemporary )							
			
				if @TPRAlwaysHasBasePrice = 0
				BEGIN
					select @IsPriceOk = 1
					select @UserCanOverride = 1
					/* code keys off of 'base price' being in this message */
					select @ValidationMessage = 'An active base price was not defined for the current TPR. Please add a corresponding base price.' 
				END
				else
				BEGIN
					select @IsPriceOk = 1
				END
			END
			else
			BEGIN
				select @IsPriceOk = 1
			END
		END
		
	END
	else -- a new price
	BEGIN
		if exists ( select Price from CM_ItemPrice 
			where (( PriceStartDate = @PriceStartDate and PriceStopDate = @PriceStopDate ) or
				PriceStartDate between @PriceStartDate and @PriceStopDate or
				PriceStopDate between @PriceStartDate and @PriceStopDate or
				( PriceStartDate < @PriceStartDate and PriceStopDate > @PriceStopDate ) or
				( PriceStartDate > @PriceStartDate and PriceStopDate < @PriceStopDate ) or
				PriceStartDate = @PriceStopDate  or
				PriceStopDate = @PriceStartDate )
			and ItemId = @ItemId
			and IsTemporary = @IsTemporary )
		BEGIN
			select @IsPriceOk = 0
			select @ValidationMessage = 'The price dates overlap the dates of an existing price of the same type.';
		END	
		else
		BEGIN
			/* if TPR, check for base price */
			if @IsTemporary = 1
			BEGIN
				select @TPRAlwaysHasBasePrice = dbo.GetMedSurgTPRAlwaysHasBasePriceFunction( @ItemId, @PriceStartDate, @PriceStopDate, @IsTemporary )							
			
				if @TPRAlwaysHasBasePrice = 0
				BEGIN
					select @IsPriceOk = 1
					select @UserCanOverride = 1
					/* code keys off of 'base price' being in this message */
					select @ValidationMessage = 'An active base price was not defined for the current TPR. Please add a corresponding base price.' 
				END
				else
				BEGIN
					select @IsPriceOk = 1
				END
			END
			else
			BEGIN
				select @IsPriceOk = 1
			END
		END
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
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 

ENDEXIT:






