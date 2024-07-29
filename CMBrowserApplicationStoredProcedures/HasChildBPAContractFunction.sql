IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'HasChildBPAContractFunction')
	BEGIN
		DROP  Function  HasChildBPAContractFunction
	END

GO

CREATE Function HasChildBPAContractFunction
(
@ContractNumber nvarchar(20)
)

returns bit

AS

BEGIN

	declare @HasChild as bit

	if exists ( select Contract_Record_ID from tbl_Cntrcts where BPA_FSS_Counterpart = @ContractNumber )
	BEGIN
		select @HasChild = 1
	END
	else
	BEGIN
		select @HasChild = 0
	END
	

	return @HasChild
END