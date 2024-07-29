IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetItemCountForMedSurgContract2')
	BEGIN
		DROP  Procedure  GetItemCountForMedSurgContract2
	END

GO

CREATE Procedure GetItemCountForMedSurgContract2
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(50),
@ContractId int,
@ActiveItemCount int OUTPUT,
@FutureItemCount int OUTPUT,
@PricelessItemCount int OUTPUT
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

    select @ActiveItemCount = count(*)
    FROM CM_Items i 
    where ContractId = @ContractId
	and exists ( select p.ItemPriceId from CM_ItemPrice p where p.ItemId = i.ItemId and getdate() between PriceStartDate and PriceStopDate )
                                       
	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting active MedSurg items for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

    select @FutureItemCount = count(*)
    FROM CM_Items i 
    where ContractId = @ContractId
	and exists ( select p.ItemPriceId from CM_ItemPrice p where p.ItemId = i.ItemId and datediff( dd, GETDATE(), PriceStartDate ) > 0 )
	
	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting future MedSurg items for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
			
	select @PricelessItemCount = count(*)
    FROM CM_Items i 
    where ContractId = @ContractId
	and not exists ( select p.ItemPriceId from CM_ItemPrice p where p.ItemId = i.ItemId and ( datediff( dd, GETDATE(), PriceStartDate ) > 0  or getdate() between PriceStartDate and PriceStopDate ))
	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting items without prices MedSurg items for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

END
