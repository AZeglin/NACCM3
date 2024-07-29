IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertContractRebate' )
BEGIN
	DROP PROCEDURE InsertContractRebate
END
GO

CREATE PROCEDURE InsertContractRebate
(
@UserLogin nvarchar(120),
@currentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@StartQuarterId int, 
@EndQuarterId int, 
@RebatePercentOfSales numeric(8,3), 
@RebateThreshold money, 
@AmountReceived money, 
@IsCustom bit,
@RebateClause nvarchar(4000),
@StandardRebateTermId int,
@CustomStartDate datetime = null,
@RebateId int OUTPUT,
@CustomRebateId int OUTPUT,
@RebateTermId int OUTPUT
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@overlap bit



BEGIN TRANSACTION

	/* clear custom date if a quarter was selected */
	if @StartQuarterId <> -1
	BEGIN
		select @CustomStartDate = null
	END

	select @overlap = 0
	select @overlap = dbo.CheckForOverlappingRebateDateFunction( @ContractNumber, null, @StartQuarterId, @EndQuarterId, @CustomStartDate )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error checking for date overlap with existing rebates.'
		goto ERROREXIT
	END

	if @overlap > 0
	BEGIN
		select @errorMsg = 'Could not insert rebate because the specified date range overlaps with an existing rebate.'
		goto ERROREXIT
	END

	insert into tbl_Rebates
	( 
		ContractNumber, StartQuarterId, EndQuarterId, CustomStartDate, RebatePercentOfSales, RebateThreshold, AmountReceived, IsCustom, 
		CreatedBy, CreationDate, LastModifiedBy, LastModificationDate 
	)
	values
	(
		@ContractNumber, @StartQuarterId, @EndQuarterId, @CustomStartDate, @RebatePercentOfSales, @RebateThreshold, @AmountReceived, @IsCustom,
		@UserLogin, getdate(), @UserLogin, getdate() 
	)



	select @error = @@ERROR, @rowCount = @@ROWCOUNT, @RebateId = SCOPE_IDENTITY() 
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting new rebate'
		goto ERROREXIT
	END

	if @IsCustom = 1
	BEGIN
		insert into tbl_CustomRebateTerms 
		(
			RebateId, RebateClause, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate 
		)
		values
		(
			@RebateId, @RebateClause, @UserLogin, getdate(), @UserLogin, getdate() 
		)


		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @CustomRebateId = SCOPE_IDENTITY() 
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting new custom rebate terms'
			goto ERROREXIT
		END

	END
	else
	BEGIN
		insert into tbl_RebatesStandardRebateTerms
		(
			RebateId, StandardRebateTermId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate  
		)
		values
		(
			@RebateId, @StandardRebateTermId, @UserLogin, getdate(), @UserLogin, getdate() 
		)


		select @error = @@ERROR, @rowCount = @@ROWCOUNT, @RebateTermId = SCOPE_IDENTITY() 
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting new standard rebate terms'
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


