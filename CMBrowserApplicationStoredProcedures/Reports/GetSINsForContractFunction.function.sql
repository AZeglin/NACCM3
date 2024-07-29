IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetSINsForContractFunction')
	BEGIN
		DROP  Function  GetSINsForContractFunction
	END

GO

CREATE FUNCTION GetSINsForContractFunction
(
@ContractNumber nvarchar(50)
)

RETURNS nvarchar(500)

AS

BEGIN

	DECLARE  @sinList as nvarchar(500)
	set @sinList = null

	select @sinList = COALESCE( @sinList + ', ', '') + [SINs] 
	from tbl_Cntrcts_SINs 
	where CntrctNum = @ContractNumber
	and Inactive = 0

	RETURN @sinList
END