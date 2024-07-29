IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectRebatesForContract' )
BEGIN
	DROP PROCEDURE SelectRebatesForContract
END
GO

CREATE PROCEDURE SelectRebatesForContract
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@WithAdd bit = 0
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),

		@rebateId int,
		@startQuarterId int,
		@endQuarterId int,
		@rebatePercentOfSales numeric(8,3),
		@rebateThreshold money,
		@amountReceived money,
		@isCustom bit,
		@createdBy nvarchar(120),
		@creationDate datetime,
		@lastModifiedBy nvarchar(120),
		@lastModificationDate datetime,
		@rebateTermId int,
		@rebatesStandardRebateTermId int,
		@standardClauseName nvarchar(255),
		@rebateClause nvarchar(4000),
		@isActive bit,
		@isNewBlankRow bit,
		@customStartDate datetime

BEGIN TRANSACTION

	if @WithAdd = 0
	BEGIN

		select r.RebateId, r.StartQuarterId, r.EndQuarterId, r.CustomStartDate, r.RebatePercentOfSales, r.RebateThreshold, r.AmountReceived, 0 as IsCustom, 
			r.CreatedBy, r.CreationDate, r.LastModifiedBy, r.LastModificationDate, s.StandardRebateTermId as RebateTermId, t.RebatesStandardRebateTermId, s.StandardClauseName, s.RebateClause, s.IsActive, 0 as IsNewBlankRow
		from tbl_Rebates r join tbl_RebatesStandardRebateTerms t on t.RebateId = r.RebateId
		join tbl_StandardRebateTerms s on t.StandardRebateTermId = s.StandardRebateTermId
		where r.ContractNumber = @ContractNumber

		union

		select r.RebateId, r.StartQuarterId, r.EndQuarterId, r.CustomStartDate, r.RebatePercentOfSales, r.RebateThreshold, r.AmountReceived, r.IsCustom, 
			r.CreatedBy, r.CreationDate, r.LastModifiedBy, r.LastModificationDate, c.CustomRebateTermId as RebateTermId, 0 as RebatesStandardRebateTermId, 'Custom' as StandardClauseName, c.RebateClause, 1 as IsActive, 0 as IsNewBlankRow
		from tbl_Rebates r join tbl_CustomRebateTerms c on c.RebateId = r.RebateId
		where r.ContractNumber = @ContractNumber

		order by r.StartQuarterId desc

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting rebate terms for contract'
			goto ERROREXIT
		END

	END
	else
	BEGIN
		/* blank row definition */
		select @rebateId = 0,
		@startQuarterId = 0,
		@endQuarterId = 0,
		@customStartDate = getdate(),
		@rebatePercentOfSales = 0,
		@rebateThreshold = 0,
		@amountReceived = 0,
		@isCustom = 0,
		@createdBy = '',
		@creationDate = getdate(),
		@lastModifiedBy = '',
		@lastModificationDate = getdate(),
		@rebateTermId = 0,
		@rebatesStandardRebateTermId = 0,
		@standardClauseName = '',
		@rebateClause = '',
		@isActive = 1,
		@isNewBlankRow = 1

		select r.RebateId, r.StartQuarterId, r.EndQuarterId, r.CustomStartDate, r.RebatePercentOfSales, r.RebateThreshold, r.AmountReceived, 0 as IsCustom, 
			r.CreatedBy, r.CreationDate, r.LastModifiedBy, r.LastModificationDate, s.StandardRebateTermId as RebateTermId, t.RebatesStandardRebateTermId, s.StandardClauseName, s.RebateClause, s.IsActive, 0 as IsNewBlankRow
		from tbl_Rebates r join tbl_RebatesStandardRebateTerms t on t.RebateId = r.RebateId
		join tbl_StandardRebateTerms s on t.StandardRebateTermId = s.StandardRebateTermId
		where r.ContractNumber = @ContractNumber

		union

		select r.RebateId, r.StartQuarterId, r.EndQuarterId, r.CustomStartDate, r.RebatePercentOfSales, r.RebateThreshold, r.AmountReceived, r.IsCustom, 
			r.CreatedBy, r.CreationDate, r.LastModifiedBy, r.LastModificationDate, c.CustomRebateTermId as RebateTermId, 0 as RebatesStandardRebateTermId, 'Custom' as StandardClauseName, c.RebateClause, 1 as IsActive, 0 as IsNewBlankRow
		from tbl_Rebates r join tbl_CustomRebateTerms c on c.RebateId = r.RebateId
		where r.ContractNumber = @ContractNumber

		union

		select @rebateId as RebateId,
			@startQuarterId as StartQuarterId,
			@endQuarterId as EndQuarterId,
			@customStartDate as CustomStartDate,
			@rebatePercentOfSales as RebatePercentOfSales,
			@rebateThreshold as RebateThreshold,
			@amountReceived as AmountReceived,
			@isCustom as IsCustom,
			@createdBy as CreatedBy,
			@creationDate as CreationDate,
			@lastModifiedBy as LastModifiedBy,
			@lastModificationDate as LastModificationDate,
			@rebateTermId as RebateTermId,
			@rebatesStandardRebateTermId as RebatesStandardRebateTermId,
			@standardClauseName as StandardClauseName,
			@rebateClause as RebateClause,
			@isActive as IsActive,
			@isNewBlankRow as IsNewBlankRow

		order by IsNewBlankRow desc, r.StartQuarterId desc

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting rebate terms for contract'
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


