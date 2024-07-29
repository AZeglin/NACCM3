IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[InsertMedSurgItemPriceHistory]') AND type in (N'P', N'PC'))
DROP PROCEDURE [InsertMedSurgItemPriceHistory]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [InsertMedSurgItemPriceHistory]  
(
@loginName  nvarchar(120),
@ContractNumber nvarchar(20),
@ItemId int,
@ItemPriceId int,
@PriceStartDate datetime,         
@PriceEndDate datetime,       
@Price decimal(9,2),      
@IsBPA bit,                                           	                  
@IsTemporary bit,
@TrackingCustomerPrice decimal(10,2),
@TrackingCustomerRatio nvarchar(100),
@TrackingCustomerName nvarchar(100),
@TrackingCustomerFOBTerms nvarchar(40),
@LastModificationType nchar(1),
@ModificationStatusId int ,
@CreatedBy nvarchar(120),
@CreationDate datetime,
@LastModifiedBy nvarchar(120),
@LastModificationDate datetime,
@Notes nvarchar(2000)                         	                  
)

AS

DECLARE @ContractId int,
		@error int,
		@rowcount int,
		@errorMsg nvarchar(250),
		@retVal int,
		@maxPriceId int,
		@ItemPriceHistoryId	int,
		@IsRemoved int

BEGIN TRANSACTION	

	If cast(Convert(Varchar(10),@PriceStartDate ,20) as datetime) > cast(Convert(Varchar(10),@PriceEndDate ,20) as datetime)
	Begin
		select @errorMsg = 'Price start date : ' + cast(@PriceStartDate as varchar(12)) + 'cannot be greater than stop date: ' + cast(@PriceEndDate as varchar(12))
		GOTO ERROREXIT
	End


	If len(@Notes) > 0
	Begin
		If @Notes = 'DeleteFSSPriceForItemPriceId;' or @Notes = 'DeleteFSSPriceIdFromSpreadsheet;'
		Begin
			Set @IsRemoved = 1
		End
		Else
		Begin
			Set @IsRemoved = 0				
		End
	End
	Else
	Begin
		Set @IsRemoved = 0		
	End

	Select @Notes = @Notes + 'InsertMedSurgItemPriceHistory;'
	

	insert into CM_ItemPriceHistory
	(   ItemPriceId, ItemId, PriceId, 
		PriceStartDate, PriceStopDate, Price, IsBPA,
		
		IsTemporary,
		TrackingCustomerPrice,
		TrackingCustomerRatio,
		TrackingCustomerName,
		TrackingCustomerFOBTerms,
				
		Removed,                                         	                  
		
		LastModificationType, ModificationStatusId, CreatedBy,
		CreationDate, LastModifiedBy, LastModificationDate, Notes, MovedToHistoryBy, DateMovedToHistory 
	)
	values
	(   @ItemPriceId, @ItemId, 0, 
	    @PriceStartDate, CONVERT(VARCHAR(10),GETDATE(),111), @Price, @IsBPA, 
		
		@IsTemporary,
		@TrackingCustomerPrice,
		@TrackingCustomerRatio,
		@TrackingCustomerName,
		@TrackingCustomerFOBTerms,

		@IsRemoved,	    
		
		@LastModificationType,
		@ModificationStatusId,
		@CreatedBy,
		@CreationDate,
		@LastModifiedBy,
		@LastModificationDate,
		@Notes,
		@loginName,
		getdate()
	)


	select @error = @@error, @ItemPriceHistoryId = @@identity
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting drug item price history for contract ' + @ContractNumber
		GOTO ERROREXIT
	END


	
GOTO OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 ) 

	IF @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
	/* only rollback iff this the highest level */ 
		ROLLBACK TRANSACTION
	END

	RETURN (-1)

OKEXIT:
	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)


	

