IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertChangeRequest')
	BEGIN
		DROP  Procedure  InsertChangeRequest
	END

GO

CREATE Procedure InsertChangeRequest
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@ChangeRequestStatus nchar(1),   -- D Draft, S Submitted, R Reviewed, A Accepted, X Rejected, I Implemented
@ChangeRequestType nchar(1),  -- U Update, D Discontinuation
@DrugItemId int,
@ProposedDiscontinuationDate datetime,
@PackageDescription nvarchar(14),
@DispensingUnit nvarchar(10),
@ChangeRequestDescription nvarchar(1200),
@ChangeRequestId int OUTPUT
)

AS 

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@loginName  nvarchar(120),
	@errorMsg nvarchar(250)
	
BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @loginName OUTPUT 

	if @error <> 0 or @loginName is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END

	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId for contract ' + @ContractNumber
		GOTO ERROREXIT
	END

	insert into DI_ChangeRequests
	( DrugItemId, ContractId, DrugItemNDCId, ChangeRequestType, ChangeRequestStatus,
		ProposedDiscontinuationDate, PackageDescription, DispensingUnit, ChangeRequestDescription,
		ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	select  @DrugItemId, @ContractId, i.DrugItemNDCId, @ChangeRequestType, @ChangeRequestStatus,
		@ProposedDiscontinuationDate, @PackageDescription, @DispensingUnit, @ChangeRequestDescription,
		-1, @loginName, getdate(), @loginName, getdate() 
	from DI_DrugItems i
	where i.DrugItemId = @DrugItemId
	
	select @error = @@error, @rowcount = @@rowcount, @ChangeRequestId = @@Identity

	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error inserting new change request for contract ' + @ContractNumber
		GOTO ERROREXIT
	END

GOTO OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 ) 

	IF @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
	/* only rollback iff this the highest level */ 
		ROLLBACK TRANSACTION
	END

	RETURN (-1)

OKEXIT:
	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)



