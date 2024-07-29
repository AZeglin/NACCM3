IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[InsertFSSDrugItemPriceHistory]') AND type in (N'P', N'PC'))
DROP PROCEDURE [InsertFSSDrugItemPriceHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [InsertFSSDrugItemPriceHistory]
(
@loginName  nvarchar(120),
@ContractNumber nvarchar(20),
@DrugItemId int,
@DrugItemPriceId int,
@PriceStartDate datetime,         
@PriceEndDate datetime,       
@Price decimal(9,2),      
@IsTemporary bit,
@IsFSS bit,                                           	                  
@IsBIG4 bit,                                          	                  
@IsVA bit,                                            	                  
@IsBOP bit,                                           	                  
@IsCMOP bit,                                          	                  
@IsDOD bit,                                           	                  
@IsHHS bit,                                           	                  
@IsIHS bit,                                           	                  
@IsIHS2 bit,                                          	                  
@IsDIHS bit,                                          	                  
@IsNIH bit,                                           	                  
@IsPHS bit,                                           	                  
@IsSVH bit,                                           	                  
@IsSVH1 bit,                                          	                  
@IsSVH2 bit,                                          	                  
@IsTMOP bit,                                          	                  
@IsUSCG bit,
@IsFHCC bit,
@AwardedFSSTrackingCustomerRatio decimal(9,2),
@TrackingCustomerName nvarchar(120),
@CurrentTrackingCustomerPrice decimal(9,2),
@ExcludeFromExport bit,
@LastModificationType nchar(1),
@ModificationStatusId int ,
@Notes nvarchar(2000)                         	                  
)

AS

DECLARE @ContractId int,
		@error int,
		@rowcount int,
		@errorMsg nvarchar(250),
		@retVal int,
		@maxPriceId int,
		@DrugItemPriceHistoryId	int,
		@IsDeleted int,
		@drugitemsubitemid int,
		@HistoricalNValue char(1)
	


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
			Set @IsDeleted = 1
		End
		Else
		Begin
			Set @IsDeleted = 0				
		End
	End
	Else
	Begin
		Set @IsDeleted = 0		
	End


	Select @drugitemsubitemid = DrugItemSubItemId,
			@HistoricalNValue = HistoricalNValue
	From Di_DrugItemprice
	Where DrugItemPriceId = @DrugItemPriceId


	Select @Notes = @Notes + 'InsertFSSDrugItemPriceHistory'
	

	insert into DI_DrugItemPriceHistory
	(   DrugItemPriceId, DrugItemId, DrugItemSubItemId,HistoricalNValue, PriceId, 
		PriceStartDate, PriceStopDate, Price, IsDeleted,IsTemporary, IsFSS, IsBIG4,                                          	                  
		IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, IsNIH, IsPHS, 
		IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG, IsFHCC,AwardedFSSTrackingCustomerRatio,
		TrackingCustomerName, CurrentTrackingCustomerPrice, ExcludeFromExport,
		LastModificationType, ModificationStatusId, Notes, CreatedBy,
		CreationDate, LastModifiedBy, LastModificationDate 
	)
	values
	(   @DrugItemPriceId, @DrugItemId,@drugitemsubitemid,@HistoricalNValue, 0, 
	    @PriceStartDate, CONVERT(VARCHAR(10),GETDATE(),111), @Price, @IsDeleted,@IsTemporary, @IsFSS, 
	    @IsBIG4,@IsVA, @IsBOP, @IsCMOP, @IsDOD, @IsHHS, @IsIHS, @IsIHS2, @IsDIHS, 
		@IsNIH, @IsPHS, @IsSVH, @IsSVH1, @IsSVH2, @IsTMOP, @IsUSCG,@IsFHCC,
		@AwardedFSSTrackingCustomerRatio,
		@TrackingCustomerName,
		@CurrentTrackingCustomerPrice,
		@ExcludeFromExport,
		@LastModificationType,
		@ModificationStatusId,
		@Notes,
		@loginName,
		getdate(),
		@loginName, 
		getdate()
	)


	select @error = @@error, @DrugItemPriceHistoryId = @@identity
	
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



