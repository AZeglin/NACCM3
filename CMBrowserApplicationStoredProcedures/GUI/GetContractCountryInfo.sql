IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetContractCountryInfo')
	BEGIN
		DROP  Procedure  GetContractCountryInfo
	END

GO

CREATE PROCEDURE GetContractCountryInfo
(
@CurrentUser uniqueidentifier,
@UserLogin nvarchar(120),
@ContractNumber nvarchar(20),
@CountryName nvarchar(100) OUTPUT
)

AS

Declare @countryId int,	
		@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)
				
BEGIN TRANSACTION

	select @countryId = Primary_CountryId
	from tbl_Cntrcts
	where CntrctNum = @ContractNumber
		
	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving country id for country info lookup.'
		goto ERROREXIT
	END

	select @CountryName = CountryName
	from CM_Countries
	where CountryId = @countryId


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving country name for country info lookup.'
		goto ERROREXIT
	END

	
goto OKEXIT

ERROREXIT:

	raiserror( @errorMsg, 16, 1 )
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )



