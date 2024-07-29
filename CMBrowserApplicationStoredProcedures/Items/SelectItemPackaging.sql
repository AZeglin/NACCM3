IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectItemPackaging' )
BEGIN
	DROP PROCEDURE SelectItemPackaging
END
GO

CREATE PROCEDURE SelectItemPackaging
(
@CurrentUser uniqueidentifier,
@IncludeInactive int = 0
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	SELECT PackageAbbreviation, PackageDescription
	from CM_ItemPackagingTypes
	where Inactive = @IncludeInactive

	union

	select '--' as PackageAbbreviation, '' as PackageDescription

	order by PackageAbbreviation


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount = 0
	BEGIN
		select @errorMsg = 'Error selecting Item Packaging Types.'
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


