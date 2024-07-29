IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectStateCodes')
	BEGIN
		DROP  Procedure  SelectStateCodes
	END

GO

CREATE Procedure SelectStateCodes
(
@CurrentUser uniqueidentifier,
@CountryId int
)

AS

DECLARE @error int,
		@rowcount int,
		@errorMsg nvarchar(200),
		@stateAbbreviation nvarchar(2),
		@stateName nvarchar(40), 
		@country nvarchar(50)
		
BEGIN
	select @stateAbbreviation = '--',
		@stateName = '',
		@country = 'Z'

	select [Abbr] as StateAbbreviation, [State/Province] as StateName, Country 
	from tlkup_StateAbbr 
	where CountryId = @CountryId  -- added 4/18/2022

	union
	
	select @stateAbbreviation as StateAbbreviation, @stateName as StateName, @country as Country 

	order by Country desc, StateAbbreviation asc

	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount < 1
	BEGIN
		select @errorMsg = 'Error retrieving list of state codes'
		raiserror( @errorMsg, 16, 1 )
	END

END
