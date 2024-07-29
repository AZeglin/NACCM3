IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertUserRecentDocument' )
BEGIN
	DROP PROCEDURE InsertUserRecentDocument
END
GO

CREATE PROCEDURE InsertUserRecentDocument
(
@CurrentUser uniqueidentifier,
@DocumentType nchar(1),  -- 'C' = contract or BPA, 'O' = offer
@DocumentNumber nvarchar(50), -- contract number or offer number
@DocumentId int -- contractId or offerId
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@PreferenceKey nvarchar(28)



BEGIN TRANSACTION

	if @DocumentType = 'C'
	BEGIN
		select @PreferenceKey = 'RecentContract'
	END
	else if @DocumentType = 'O'
	BEGIN
		select @PreferenceKey = 'RecentOffer'
	END
	
	if exists ( select UserPreferenceId from CM_UserPreferences
				where UserId = @CurrentUser
				and PreferenceKey = @PreferenceKey
				and PreferenceNumericValue = @DocumentId
				and Removed = 0 )
	BEGIN
		update CM_UserPreferences
			set LastModificationDate = GETDATE(),
				Ordinality = Ordinality + 1,
				PreferenceStringValue = @DocumentNumber  -- support update of offer number
		where UserId = @CurrentUser
		and PreferenceKey = @PreferenceKey
		and PreferenceNumericValue = @DocumentId
		and Removed = 0

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating recent document list for user.'
			goto ERROREXIT
		END
	END
	else
	BEGIN
		-- 'D' indicates recent document
		insert into CM_UserPreferences
		( UserId, Ordinality, PreferenceType, PreferenceKey, PreferenceStringValue, PreferenceNumericValue, LastModificationDate )
		values
		( @CurrentUser, 0, 'D', @PreferenceKey, @DocumentNumber, @DocumentId, GETDATE() )

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error inserting recent document for user.'
			goto ERROREXIT
		END
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


