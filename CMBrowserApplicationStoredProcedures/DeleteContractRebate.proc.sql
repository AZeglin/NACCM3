IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteContractRebate' )
BEGIN
	DROP PROCEDURE DeleteContractRebate
END
GO

CREATE PROCEDURE DeleteContractRebate
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@RebateId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@WasCustom bit

BEGIN TRANSACTION

	select @WasCustom = IsCustom
	from tbl_Rebates
	where RebateId = @RebateId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving existing rebate for deletion for contract ' + @ContractNumber
		goto ERROREXIT
	END

	if @WasCustom = 0
	BEGIN
		delete tbl_RebatesStandardRebateTerms
		Output 'tbl_RebatesStandardRebateTerms', Deleted.RebatesStandardRebateTermId, @UserLogin, GETDATE() into Audit_Deleted_Data_By_User
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error deleting standard rebate for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END
	else -- custom
	BEGIN
		delete tbl_CustomRebateTerms
		Output 'tbl_CustomRebateTerms', Deleted.CustomRebateTermId, @UserLogin, GETDATE() into Audit_Deleted_Data_By_User
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error deleting custom rebate for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END

	delete tbl_Rebates
	Output 'tbl_Rebates', Deleted.RebateId, @UserLogin, GETDATE() into Audit_Deleted_Data_By_User
	where RebateId = @RebateId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting rebate for contract ' + @ContractNumber
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


