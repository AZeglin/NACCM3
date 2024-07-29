IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractRebate' )
BEGIN
	DROP PROCEDURE UpdateContractRebate
END
GO

CREATE PROCEDURE UpdateContractRebate
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@StandardRebateTermId int,   
@RebateId int,
@StartQuarterId int, 
@EndQuarterId int, 
@RebatePercentOfSales numeric(8,3), 
@RebateThreshold money, 
@AmountReceived money, 
@IsCustom bit,
@RebateClause nvarchar(4000),
@CustomStartDate datetime = null,
@CustomRebateId int output, -- when updating from standard to custom
@RebateTermId int output  -- when updating from custom to standard
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@WasCustom bit,
		@overlap bit



BEGIN TRANSACTION

	/* clear custom date if a quarter was selected */
	if @StartQuarterId <> -1
	BEGIN
		select @CustomStartDate = null
	END

	select @overlap = 0
	select @overlap = dbo.CheckForOverlappingRebateDateFunction( @ContractNumber, @RebateId, @StartQuarterId, @EndQuarterId, @CustomStartDate )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error checking for date overlap with existing rebates.'
		goto ERROREXIT
	END

	if @overlap > 0
	BEGIN
		select @errorMsg = 'Could not update rebate because the specified date range overlaps with an existing rebate'
		goto ERROREXIT
	END

	select @WasCustom = IsCustom
	from tbl_Rebates
	where RebateId = @RebateId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving existing rebate for update'
		goto ERROREXIT
	END


	-- updating standard rebate selected for the particular quarter
	if @WasCustom = 0 and @IsCustom = 0
	BEGIN
		update tbl_Rebates
		set StartQuarterId = @StartQuarterId, 
			EndQuarterId = @EndQuarterId, 
			CustomStartDate = @CustomStartDate,
			RebatePercentOfSales = @RebatePercentOfSales, 
			RebateThreshold = @RebateThreshold, 
			AmountReceived = @AmountReceived, 
			LastModifiedBy = @UserLogin, 
			LastModificationDate = getdate()
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating existing standard rebate terms (1)'
			goto ERROREXIT
		END
		
		-- update rebate details
		update tbl_RebatesStandardRebateTerms
		set StandardRebateTermId = @StandardRebateTermId
		from tbl_RebatesStandardRebateTerms
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating existing standard rebate terms (2)'
			goto ERROREXIT
		END
	END
	-- updating custom rebate
	else if  @WasCustom = 1 and @IsCustom = 1
	BEGIN
		update tbl_Rebates
		set StartQuarterId = @StartQuarterId, 
			EndQuarterId = @EndQuarterId, 
			CustomStartDate = @CustomStartDate,
			RebatePercentOfSales = @RebatePercentOfSales, 
			RebateThreshold = @RebateThreshold, 
			AmountReceived = @AmountReceived, 
			LastModifiedBy = @UserLogin, 
			LastModificationDate = getdate()
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating existing custom rebate terms (1)'
			goto ERROREXIT
		END
		
		-- update rebate details
		update tbl_CustomRebateTerms
		set RebateClause = @RebateClause
		from tbl_CustomRebateTerms
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating existing custom rebate terms (2)'
			goto ERROREXIT
		END
	END
	-- replacing custom rebate with standard rebate
	else if @WasCustom = 1 and @IsCustom = 0
	BEGIN
		update tbl_Rebates
		set StartQuarterId = @StartQuarterId, 
			EndQuarterId = @EndQuarterId, 
			CustomStartDate = @CustomStartDate,
			RebatePercentOfSales = @RebatePercentOfSales, 
			RebateThreshold = @RebateThreshold, 
			AmountReceived = @AmountReceived, 
			IsCustom = @IsCustom,
			LastModifiedBy = @UserLogin, 
			LastModificationDate = getdate()
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating existing custom rebate terms with standard terms (1)'
			goto ERROREXIT
		END

		-- update rebate details
		-- remove custom rebate terms
		delete tbl_CustomRebateTerms
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating existing custom rebate terms with standard terms (2)'
			goto ERROREXIT
		END

		-- replace with standard terms
		insert into tbl_RebatesStandardRebateTerms
		(
			RebateId, StandardRebateTermId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate  
		)
		values
		(
			@RebateId, @StandardRebateTermId, @UserLogin, getdate(), @UserLogin, getdate() 
		)

		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @RebateTermId = @@IDENTITY
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error updating existing custom rebate terms with standard terms (3)'
			goto ERROREXIT
		END

	END
	-- replacing standard rebate with custom rebate
	else if @WasCustom = 0 and @IsCustom = 1
	BEGIN
		update tbl_Rebates
		set StartQuarterId = @StartQuarterId, 
			EndQuarterId = @EndQuarterId, 
			CustomStartDate = @CustomStartDate,
			RebatePercentOfSales = @RebatePercentOfSales, 
			RebateThreshold = @RebateThreshold, 
			AmountReceived = @AmountReceived, 
			IsCustom = @IsCustom,
			LastModifiedBy = @UserLogin, 
			LastModificationDate = getdate()
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating existing standard rebate terms with custom terms (1)'
			goto ERROREXIT
		END

		-- remove standard rebate
		delete tbl_RebatesStandardRebateTerms
		where RebateId = @RebateId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error updating existing standard rebate terms with custom terms (2)'
			goto ERROREXIT
		END

		-- replace with custom rebate
		insert into tbl_CustomRebateTerms 
		(
			RebateId, RebateClause, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate 
		)
		values
		(
			@rebateId, @RebateClause, @UserLogin, getdate(), @UserLogin, getdate() 
		)


		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @CustomRebateId = @@IDENTITY
		if @error <> 0 
		BEGIN
			select @errorMsg =  'Error updating existing standard rebate terms with custom terms (3)'
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


