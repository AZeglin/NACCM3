IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetMedSurgParentItems' )
BEGIN
	DROP PROCEDURE GetMedSurgParentItems
END
GO

CREATE PROCEDURE GetMedSurgParentItems
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),   /* this contract is the child contract */
@ContractId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@parentContractNumber nvarchar(20),
		@parentContractId int,
		@selectCatalogNumber nvarchar(70),
		@selectDescription nvarchar( 100 ),
		@selectItemId int


BEGIN TRANSACTION

	select @parentContractNumber = BPA_FSS_Counterpart 
	from tbl_Cntrcts 
	where Contract_Record_ID = @ContractId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error looking up parent contract number for contract ' + @ContractNumber
		goto ERROREXIT
	END

	select @parentContractId = Contract_Record_ID 
	from tbl_Cntrcts
	where CntrctNum = @parentContractNumber

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error looking up contract id for parent contract ' + @parentContractNumber
		goto ERROREXIT
	END

	select @selectCatalogNumber = '  --  ', @selectDescription = 'select parent item', @selectItemId = -1

	select @selectItemId as ItemId, @selectCatalogNumber as CatalogNumber, @selectCatalogNumber + convert( nvarchar(70), REPLICATE ( ' ', 64 )) + @selectDescription as OverallDescription

	union

	select i.ItemId, i.CatalogNumber, LTRIM(RTRIM( i.CatalogNumber )) + convert( nvarchar(70), REPLICATE ( ' ', 70 - LEN(LTRIM(RTRIM( i.CatalogNumber ))) )) + i.ItemDescription as OverallDescription 
		from CM_Items i join CM_ItemPrice p on i.ItemId = p.ItemId
		where i.ContractId = @parentContractId
		and datediff( dd, p.PriceStopDate, getdate() ) <= 0

	order by CatalogNumber

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting items from parent contract ' + @parentContractNumber
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



