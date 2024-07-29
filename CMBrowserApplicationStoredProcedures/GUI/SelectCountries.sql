IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectCountries')
	BEGIN
		DROP  Procedure  SelectCountries
	END

GO

CREATE Procedure SelectCountries
(
@CurrentUser uniqueidentifier,
@CountryListType char(1)  -- 'V' = vendor addresses;  'I' = item countries of origin
)

AS

DECLARE @error int,
		@rowcount int,
		@errorMsg nvarchar(200),
		@countryId int,
		@countryName nvarchar(100)
	
		
BEGIN
	select @countryId = -1,
		@countryName = '-- select --'
	
	if @CountryListType = 'V'
	BEGIN

		select CountryId, CountryName 
		from CM_Countries
		where ActiveForVendorAddress = 1

		union
	
		select @countryId as CountryId, @countryName as CountryName

		order by CountryName

		select @error = @@error, @rowcount = @@rowcount
	
		if @error <> 0 or @rowcount < 1
		BEGIN
			select @errorMsg = 'Error retrieving list of countries'
			raiserror( @errorMsg, 16, 1 )
		END
	END
	else if @CountryListType = 'I'
	BEGIN

		select CountryId, CountryName 
		from CM_Countries
		where ActiveForItemOrigin = 1

		union
	
		select @countryId as CountryId, @countryName as CountryName

		order by CountryName

		select @error = @@error, @rowcount = @@rowcount
	
		if @error <> 0 or @rowcount < 1
		BEGIN
			select @errorMsg = 'Error retrieving list of countries'
			raiserror( @errorMsg, 16, 1 )
		END
	END

END
