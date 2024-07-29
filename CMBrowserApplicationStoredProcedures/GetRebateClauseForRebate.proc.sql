IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetRebateClauseForRebate' )
BEGIN
	DROP PROCEDURE GetRebateClauseForRebate
END
GO

CREATE PROCEDURE GetRebateClauseForRebate
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@RebateId int,
@IsCustom bit OUTPUT,
@RebateClause nvarchar(4000) OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	select @IsCustom = IsCustom
	from tbl_Rebates
	where RebateId = @RebateId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error getting rebate clause for rebate id=' + CONVERT( nvarchar(14), @RebateId )
		goto ERROREXIT
	END

	if @IsCustom = 1
	BEGIN
		select @RebateClause = c.RebateClause
		from tbl_Rebates r join tbl_CustomRebateTerms c on c.RebateId = r.RebateId
		where r.RebateId = @RebateId
	END
	else
	BEGIN
		select @RebateClause = s.RebateClause
		from tbl_Rebates r join tbl_RebatesStandardRebateTerms t on t.RebateId = r.RebateId
		join tbl_StandardRebateTerms s on t.StandardRebateTermId = s.StandardRebateTermId
		where r.RebateId = @RebateId
	END

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error getting rebate clause (2) for rebate id=' + CONVERT( nvarchar(14), @RebateId )
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


