IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectMedSurgItemsForExport')
	BEGIN
		DROP  Procedure  SelectMedSurgItemsForExport
	END

GO

CREATE Procedure SelectMedSurgItemsForExport
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20)
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

    select [Index Do Not Alter], [Contract Number], [SIN], [Catalog Number], [Product Description], 
		[FSS Price], [UOM Priced], [Unit of Sale UOM], [Number of Inner Packs], [Inner Pack UOM], 
		[Number of Base Packs], [Base Pack UOM], [Qty Within Base Pack], '', DateEffective2, ExpirationDate2 
    FROM [view_Pricelist_Export_Equipment] 
    WHERE [view_Pricelist_Export_Equipment].[Contract Number] = @ContractNumber

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving MedSurg items for export, for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
	
END

