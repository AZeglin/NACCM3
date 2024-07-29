IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetOfferPrefix' )
BEGIN
	DROP PROCEDURE GetOfferPrefix
END
GO

CREATE PROCEDURE GetOfferPrefix
(
@CurrentUser uniqueidentifier,
@ProposalTypeId int,          /* 1 = Offer Proposal; 2 = Contract Extension Proposal */
@OfferPrefix nvarchar(30) OUTPUT
)

AS

Declare @error int,
		@rowCount int,
		@errorMsg nvarchar(1000)


BEGIN TRANSACTION

	if @ProposalTypeId = 1
	BEGIN
		select @OfferPrefix = 'OFF-FSS-'
	END
	else if @ProposalTypeId = 2
	BEGIN
		select @OfferPrefix = 'EXT-FSS-'
	END
	else
	BEGIN
		select @OfferPrefix = ''
	END

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting offer prefix based on offer proposal id.'
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


