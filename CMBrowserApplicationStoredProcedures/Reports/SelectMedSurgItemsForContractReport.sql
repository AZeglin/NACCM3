IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectMedSurgItemsForContractReport')
BEGIN
	DROP  Procedure  SelectMedSurgItemsForContractReport
END

GO

CREATE Procedure SelectMedSurgItemsForContractReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@ContractId int,
@IncludeHistory bit = 0
)
AS

DECLARE @ContractNumber nvarchar(50),
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200)

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'MedSurg Items For Contract Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	select @ContractNumber = CntrctNum from tbl_Cntrcts where Contract_Record_ID = @ContractId

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting contract number for report.'
		goto ERROREXIT
	END

	if @IncludeHistory = 0
	BEGIN
		select  @ContractNumber as ContractNumber, 0 as IsFromHistory, ItemId, CatalogNumber, ItemDescription, [SIN], PackageAsPriced, LastModifiedBy, LastModificationDate
		from CM_Items
		where ContractId = @ContractId

		order by CatalogNumber
	END
	else
	BEGIN
		select  @ContractNumber as ContractNumber, 0 as IsFromHistory, ItemId, CatalogNumber, ItemDescription, [SIN], PackageAsPriced, LastModifiedBy, LastModificationDate
		from CM_Items
		where ContractId = @ContractId

		union

		select  @ContractNumber as ContractNumber, 1 as IsFromHistory, ItemId, CatalogNumber, ItemDescription, [SIN], PackageAsPriced, LastModifiedBy, LastModificationDate
		from CM_ItemsHistory
		where ContractId = @ContractId

		order by CatalogNumber

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


