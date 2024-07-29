IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'RestoreDiscontinuedItem' )
BEGIN
	DROP PROCEDURE RestoreDiscontinuedItem
END
GO

CREATE PROCEDURE RestoreDiscontinuedItem
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemId int,
@ModificationStatusId int,
@LastModificationType nchar(1)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),		@loginName nvarchar(120)


BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @loginName OUTPUT 
	
	if @error <> 0 or @loginName is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END

	Insert into Di_DrugItemsHistory
	(DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
	 DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
	 Covered,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
	 ExcludeFromExport,NonTAA,IncludedFETAmount,ParentDrugItemId,LastModificationType,ModificationStatusId,Notes,CreatedBy,CreationDate,
	 LastModifiedBy,LastModificationDate 
	)
	Select 
		DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
		DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
		Covered,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
		ExcludeFromExport,NonTAA,IncludedFETAmount,ParentDrugItemId,LastModificationType,ModificationStatusId,'RestoreDiscontinuedItem',
		CreatedBy,CreationDate,@loginName,getdate() 
	From Di_DrugItems
	Where DrugItemId = @drugitemId

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting item history for drugitem Id: '+ cast(@drugitemId as varchar)
		GOTO ERROREXIT
	END

	update DI_DrugItems
	set DiscontinuationDate = null,
		DiscontinuationEnteredDate = null,
		DiscontinuationReasonId = null,
		ModificationStatusId = @modificationStatusId,
		LastModifiedBy = @loginName,
		LastModificationDate = getdate(),
		LastModificationType = @LastModificationType
	where 
	DrugItemId = @DrugItemId
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error restoring drug item to contract ' + @ContractNumber
		GOTO ERROREXIT
	END

	GOTO OKEXIT

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error'
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


