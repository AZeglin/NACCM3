IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectOfferActionTypes' )
BEGIN
	DROP PROCEDURE SelectOfferActionTypes
END
GO

CREATE PROCEDURE SelectOfferActionTypes
(
@CurrentUser uniqueidentifier
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	SELECT Action_ID as ActionId, 
		Action_Description as ActionDescription, 
		Complete as IsOfferComplete
	FROM tlkup_Offers_Action_Type 
	ORDER BY Action_Description

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount = 0
	BEGIN
		select @errorMsg = 'Error selecting Offer Action Types.'
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


