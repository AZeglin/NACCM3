IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertMedSurgServicePriceFromGUI' )
BEGIN
	DROP PROCEDURE InsertMedSurgServicePriceFromGUI
END
GO

CREATE PROCEDURE InsertMedSurgServicePriceFromGUI
(
@UserLogin nvarchar(120),
@ContractNumber nvarchar(20),
@ContractorCatalogNumber nvarchar(50), 
@ProductLongDescription nvarchar(800), 
@FSSPrice decimal(18,2), 
@PackageSizePricedOnContract nvarchar(2), 
@SIN nvarchar(50), 
@ServiceDescriptionId int,
@DateEffective datetime, 
@ExpirationDate datetime, 
@CreatedBy nvarchar(120), 
@DateEntered datetime, 
@LastModifiedBy nvarchar(120), 
@DateModified datetime,
@LogNumber int OUTPUT
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

	select @PriceExists = dbo.CheckForDuplicateMedSurgFSSPriceFunction( @ContractNumber, @ContractorCatalogNumber, @ProductLongDescription, @FSSPrice, @PackageSizePricedOnContract, @SIN, @ServiceDescriptionId, @DateEffective, @ExpirationDate, null, 0, 1 )

	if @PriceExists = 0
	BEGIN

		insert into tbl_pricelist
		 ( CntrctNum, [Contractor Catalog Number], [Product Long Description], [FSS Price], [Package Size Priced on Contract], [SIN], [621I_Category_ID], [DateEffective], [ExpirationDate], [CreatedBy], [Date_Entered], [LastModifiedBy], [Date_Modified] ) 
		VALUES 
		 ( @ContractNumber, @ContractorCatalogNumber, @ProductLongDescription, @FSSPrice, @PackageSizePricedOnContract, @SIN, @ServiceDescriptionId, @DateEffective, @ExpirationDate, @CreatedBy, @DateEntered, @LastModifiedBy, @DateModified )

		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @LogNumber = SCOPE_IDENTITY() 
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting med/surg item into pricelist.'
			goto ERROREXIT
		END

	END
	else
	BEGIN
		select @errorMsg = 'The item/price being inserted already exists in the database. Either update the existing price, or choose an effective date range that does not overlap the date range of existing item/price.'
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


