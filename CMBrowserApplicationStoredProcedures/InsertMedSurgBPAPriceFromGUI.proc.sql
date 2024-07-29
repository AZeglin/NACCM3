IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertMedSurgBPAPriceFromGUI' )
BEGIN
	DROP PROCEDURE InsertMedSurgBPAPriceFromGUI
END
GO

CREATE PROCEDURE InsertMedSurgBPAPriceFromGUI
(
@UserLogin nvarchar(120),
@ContractNumber nvarchar(50),
@FSSLogNumber int,
@BPADescription nvarchar(255), 
@BPAPrice money, 
@DateEffective datetime, 
@ExpirationDate datetime, 
@CreatedBy nvarchar(120), 
@CreationDate datetime, 
@LastModifiedBy nvarchar(120), 
@LastModificationDate datetime,
@BPALogNumber int OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@PriceExists bit



BEGIN TRANSACTION

	if DATEDIFF( dd, @DateEffective, @ExpirationDate ) < 0
	BEGIN
		select @errorMsg = 'Effective date must be before expiration date.' 
		goto ERROREXIT
	END

	select @PriceExists = dbo.CheckForDuplicateMedSurgBPAPriceFunction( @ContractNumber, @FSSLogNumber, @BPADescription, @BPAPrice, @DateEffective, @ExpirationDate, null, 0 )

	if @PriceExists = 0
	BEGIN

		insert into tbl_BPA_pricelist
		 ( [CntrctNum], [FSSLogNumber], [Description], [BPA/BOA Price], [DateEffective], [ExpirationDate], [CreatedBy], [CreationDate], [LastModifiedBy], [LastModificationDate] ) 
		 VALUES 
		 ( @ContractNumber, @FSSLogNumber, @BPADescription, @BPAPrice, @DateEffective, @ExpirationDate, @CreatedBy, @CreationDate, @LastModifiedBy, @LastModificationDate )
 

		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @BPALogNumber = SCOPE_IDENTITY() 
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting med/surg BPA item into pricelist.'
			goto ERROREXIT
		END
	END
	else
	BEGIN
		select @errorMsg = 'The BPA item/price being inserted already exists in the database. Either update the existing price, or choose an effective date range that does not overlap the date range of existing item/price.'
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


