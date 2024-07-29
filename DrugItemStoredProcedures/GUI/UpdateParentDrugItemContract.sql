IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateParentDrugItemContract')
	BEGIN
		DROP  Procedure  UpdateParentDrugItemContract
	END

GO

CREATE Procedure UpdateParentDrugItemContract
(
@ContractNumber nvarchar(20),
@ParentContractNumber nvarchar(20)
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	update DI_Contracts
	set ParentFSSContractId = ( select ContractId from DI_Contracts where NACCMContractNumber = @ParentContractNumber )
	where NACCMContractNumber = @ContractNumber

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0 or @rowcount <> 1 
	BEGIN
		select @errorMsg = 'Error updating parent contractId for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

END
