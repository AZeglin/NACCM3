IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetItemCountForDrugContract]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetItemCountForDrugContract]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure GetItemCountForDrugContract
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@ItemCount int OUTPUT
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId from contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
	
    select @itemCount = count(*)
	from DI_DrugItems i
	where i.ContractId = @ContractId    
	and ( i.DiscontinuationDate >= CONVERT( datetime, CONVERT( varchar(50), getdate(), 101 )) or i.DiscontinuationDate is null ) 

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting drug items for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END		
END

