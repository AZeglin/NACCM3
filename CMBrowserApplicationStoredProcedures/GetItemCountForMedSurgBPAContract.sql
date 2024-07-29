IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetItemCountForMedSurgBPAContract')
	BEGIN
		DROP  Procedure  GetItemCountForMedSurgBPAContract
	END

GO

CREATE Procedure GetItemCountForMedSurgBPAContract
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@ActiveItemCount int OUTPUT,
@FutureItemCount int OUTPUT
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

    select @ActiveItemCount = count(*)
    FROM tbl_BPA_Pricelist
    where CntrctNum = @ContractNumber
    and Removed = 0
    and datediff(dd,[DateEffective],GETDATE())>=0  
	and datediff(dd,GETDATE(), [ExpirationDate])>=0

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting active MedSurg BPA items for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

	select @FutureItemCount = count(*)
    FROM tbl_BPA_Pricelist
    where CntrctNum = @ContractNumber
    and Removed = 0
	and datediff(dd,GETDATE(), [DateEffective])>0

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting future MedSurg BPA items for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
END

