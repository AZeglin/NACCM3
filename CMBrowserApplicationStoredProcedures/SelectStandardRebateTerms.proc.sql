IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectStandardRebateTerms' )
BEGIN
	DROP PROCEDURE SelectStandardRebateTerms
END
GO

CREATE PROCEDURE SelectStandardRebateTerms
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ClauseType nchar(1)   -- 'A' = All
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	if @ClauseType = 'A'
	BEGIN
		select StandardRebateTermId, StandardClauseName, RebateClause, ClauseType, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate
		from tbl_StandardRebateTerms
		where IsActive = 1
	END
	else
	BEGIN
		select StandardRebateTermId, StandardClauseName, RebateClause, ClauseType, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate
		from tbl_StandardRebateTerms
		where ClauseType = @ClauseType
		and IsActive = 1
	END

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <= 0
	BEGIN
		select @errorMsg = 'Error retrieving standard rebates'
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


