IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetListOfSinsForContractFunction')
	BEGIN
		DROP  Function  GetListOfSinsForContractFunction
	END

GO


CREATE FUNCTION [dbo].[GetListOfSinsForContractFunction]
(
	@ContractNumber nvarchar(50)
)


RETURNS nvarchar(3000)

AS
BEGIN
	DECLARE @ListOfSinsForContract nvarchar(max)

	select @ListOfSinsForContract = COALESCE( @ListOfSinsForContract + ', ', '' ) + COALESCE( s.SINs, '' )
	from tbl_Cntrcts_SINs s
	where s.CntrctNum = @ContractNumber 
	and s.SINs is not null
	and s.Inactive = 0

	RETURN @ListOfSinsForContract
END