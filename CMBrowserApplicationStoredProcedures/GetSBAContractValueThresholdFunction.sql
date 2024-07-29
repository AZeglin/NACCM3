IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetSBAContractValueThresholdFunction')
	BEGIN
		DROP  Function  GetSBAContractValueThresholdFunction
	END

GO

CREATE FUNCTION [dbo].[GetSBAContractValueThresholdFunction]
(
	@ContractAwardDate datetime
)
RETURNS INT
AS

BEGIN

	/* on 9/2015 when the new threshold of 700000 was adopted, this function was created to correctly reflect the threshold for older contracts and was substituted in */
	/* active reports that used the hardcoded value; the values as specified below were taken from an email from Ray dated 9/10/2015 */
	/* Oct 2005 - $500K */
	/* Oct 1 2006 - $500K to $550K */
	/* Oct 1 2011 - Increased from $550K to $650K */
	/* Oct 1 2015 - Increased from $650K to $700K */
	/* Oct 1 2020 - Increased from $700k to $750k */

	Declare @ContractValueThreshold int

	select @ContractValueThreshold = ( case when @ContractAwardDate between '1/1/2000' and '9/30/2006' then 500000 
	else ( case when @ContractAwardDate between '10/1/2006' and '9/30/2011' then 550000 
	else ( case when @ContractAwardDate between '10/1/2011' and '9/30/2015' then 650000 
	else ( case when @ContractAwardDate between '10/1/2015' and '9/30/2020' then 700000 
	else ( case when @ContractAwardDate between '10/1/2020' and '9/30/2099' then 750000 
	else 499999 end ) end ) end ) end ) end )

	RETURN @ContractValueThreshold
END
