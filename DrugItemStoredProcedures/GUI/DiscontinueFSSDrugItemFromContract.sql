IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DiscontinueFSSDrugItemFromContract]') AND type in (N'P', N'PC'))
DROP PROCEDURE [DiscontinueFSSDrugItemFromContract]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[DiscontinueFSSDrugItemFromContract]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemId int,
@DiscontinuationDate datetime,
@DiscontinuationReason nvarchar(512),
@ModificationStatusId int,
@LastModificationType nchar(1)
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@loginName nvarchar(120),
	@DiscontinuationReasonId int

BEGIN Tran

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @loginName OUTPUT 
	
	if @error <> 0 or @loginName is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END
-- Contract Id not required as DrugItemId is passed as parameter
/*	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId during discontinue item for contract ' + @ContractNumber
		GOTO ERROREXIT
	END
*/

	select @DiscontinuationReasonId = DiscontinuationReasonId
	from DI_ItemDiscontinuationReasons
	where DiscontinuationReason = @DiscontinuationReason

	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting DiscontinuationReasonId during discontinue item for reason: ' + @DiscontinuationReason
		GOTO ERROREXIT
	END

	Insert into Di_DrugItemsHistory
	(DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
	 DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
	 Covered,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
	 ExcludeFromExport,NonTAA, IncludedFETAmount,ParentDrugItemId,LastModificationType,ModificationStatusId,Notes,CreatedBy,CreationDate,
	 LastModifiedBy,LastModificationDate 
	)
	Select 
		DrugItemId,ContractId,DrugItemNDCId,PackageDescription,Generic,TradeName,DiscontinuationDate,
		DiscontinuationEnteredDate,DiscontinuationReasonId,DateEnteredMarket,
		Covered,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
		ExcludeFromExport,NonTAA, IncludedFETAmount,ParentDrugItemId,LastModificationType,ModificationStatusId,'DiscontinueDrugItemFromContract',
		CreatedBy,CreationDate,@loginName,getdate() 
	From Di_DrugItems
	Where DrugItemId = @drugitemId

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting items history for drugitem Id: '+ cast(@drugitemId as varchar)
		GOTO ERROREXIT
	END
	
	update DI_DrugItems
	set DiscontinuationDate = @DiscontinuationDate,
		DiscontinuationEnteredDate = getdate(),
		DiscontinuationReasonId = @DiscontinuationReasonId,
		ModificationStatusId = @modificationStatusId,
		LastModifiedBy = @loginName,
		LastModificationDate = getdate(),
		LastModificationType = @LastModificationType
	where --ContractId = @ContractId
	DrugItemId = @DrugItemId
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error discontinuing drug item from contract ' + @ContractNumber
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

