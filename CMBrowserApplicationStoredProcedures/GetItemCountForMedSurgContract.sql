IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetItemCountForMedSurgContract')
	BEGIN
		DROP  Procedure  GetItemCountForMedSurgContract
	END

GO

CREATE Procedure GetItemCountForMedSurgContract
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
    FROM tbl_pricelist
    where CntrctNum = @ContractNumber
    and Removed = 0
	and datediff(dd,[DateEffective],GETDATE())>=0  
	and datediff(dd,GETDATE(), [ExpirationDate])>=0
                                       
	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting active MedSurg items for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

    select @FutureItemCount = count(*)
    FROM tbl_pricelist
    where CntrctNum = @ContractNumber
    and Removed = 0
	and datediff(dd,GETDATE(), [DateEffective])>0

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting future MedSurg items for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
			
END
