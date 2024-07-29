IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdatePricelistDetailForMedSurgBPAItem')
	BEGIN
		DROP  Procedure  UpdatePricelistDetailForMedSurgBPAItem
	END

GO

CREATE Procedure UpdatePricelistDetailForMedSurgBPAItem
(
@CurrentUser uniqueidentifier,
@BPALogNumber as int,
@BPADescription as nvarchar(255),
@BPAPrice as decimal(14,2),
@FSSLogNumber as int,
@BPAEffectiveDate as datetime,
@BPAExpirationDate as datetime,
@LastModifiedBy as nvarchar(120)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@PriceExists bit,
		@ExistingContractNumber nvarchar(50)



BEGIN TRANSACTION

	select @ExistingContractNumber = CntrctNum
	from tbl_BPA_pricelist 
	where BPALogNumber = @BPALogNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating med/surg BPA item into pricelist. Could not retrieve contract number of existing BPA item.'
		goto ERROREXIT
	END

	if DATEDIFF( dd, @BPAEffectiveDate, @BPAExpirationDate ) < 0
	BEGIN
		select @errorMsg = 'Effective date must be before expiration date.' 
		goto ERROREXIT
	END

	select @PriceExists = dbo.CheckForDuplicateMedSurgBPAPriceFunction( @ExistingContractNumber, @FSSLogNumber, @BPADescription, @BPAPrice, @BPAEffectiveDate, @BPAExpirationDate, @BPALogNumber, 0 )

	if @PriceExists = 0
	BEGIN


		UPDATE tbl_BPA_Pricelist
		SET Description = @BPADescription,
			[BPA/BOA Price] = @BPAPrice,
			FSSLogNumber = @FSSLogNumber,
			DateEffective = @BPAEffectiveDate,
			ExpirationDate = @BPAExpirationDate,
			LastModifiedBy = @LastModifiedBy,
			LastModificationDate = GETDATE()
		WHERE BPALogNumber = @BPALogNumber

	END
	else
	BEGIN
		select @errorMsg = 'The BPA item/price being updated matches another item already existing in the database. If implementing a future BPA item/price, consider inserting a new price record and choosing an effective date range that does not overlap the date range of an existing BPA item/price.'
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




