IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectCountriesOfOriginForItem' )
BEGIN
	DROP PROCEDURE SelectCountriesOfOriginForItem
END
GO

CREATE PROCEDURE SelectCountriesOfOriginForItem
(
@CurrentUser uniqueidentifier,
@ItemId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)
		
BEGIN TRANSACTION

	select c.CountryId, c.CountryName, case when ( j.CountryId is not null ) then 1 else 0 end as IsSelected, j.ItemCountryId
	from CM_Countries c left outer join CM_ItemCountries j on c.CountryId = j.CountryId and j.ItemId = @ItemId
	where ActiveForItemOrigin = 1
	

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting active countries of origin for item' 
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



