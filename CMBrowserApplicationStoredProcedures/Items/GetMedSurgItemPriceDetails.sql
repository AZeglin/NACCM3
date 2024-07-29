IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetMedSurgItemPriceDetails' )
BEGIN
	DROP PROCEDURE GetMedSurgItemPriceDetails
END
GO

CREATE PROCEDURE GetMedSurgItemPriceDetails
(
@CurrentUser uniqueidentifier,
@ItemPriceId int,
@ItemPriceHistoryId int = 0,
@IsFromHistory bit = 0
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserName nvarchar(120)
		
BEGIN TRANSACTION

	if @IsFromHistory = 0
	BEGIN
		select	TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms, LastModificationType, LastModifiedBy, LastModificationDate
		from CM_ItemPrice
		where ItemPriceId = @ItemPriceId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT

		if @error <> 0 OR @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error encountered when attempting to retrieve item price details'
			goto ERROREXIT
		END
	END
	else
	BEGIN
		select	TrackingCustomerPrice, TrackingCustomerRatio, TrackingCustomerName, TrackingCustomerFOBTerms, LastModificationType, LastModifiedBy, LastModificationDate
		from CM_ItemPriceHistory
		where ItemPriceHistoryId = @ItemPriceHistoryId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT

		if @error <> 0 OR @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error encountered when attempting to retrieve item price details from history'
			goto ERROREXIT
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





