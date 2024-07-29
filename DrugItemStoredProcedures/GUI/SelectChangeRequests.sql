IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectChangeRequests')
	BEGIN
		DROP  Procedure  SelectChangeRequests
	END

GO

CREATE Procedure SelectChangeRequests
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@ChangeRequestStatusCriteria nchar(1),   -- D Draft, S Submitted, R Reviewed, A Accepted, X Rejected, I Implemented, L All
@ChangeRequestTypeCriteria nchar(1),  -- U Update, D Discontinuation, B Both
@StartDate datetime,
@EndDate datetime
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

	select
		c.ChangeRequestId,
		n.FdaAssignedLabelerCode,    
		n.ProductCode,       
		n.PackageCode,
		c.DrugItemId,
		c.ContractId,
		c.DrugItemNDCId,
		c.ChangeRequestType,
		c.ChangeRequestStatus,
		c.ProposedDiscontinuationDate,
		c.PackageDescription,
		c.DispensingUnit,
		c.ChangeRequestDescription,
		c.ReviewedBy,
		c.ReviewDate,
		c.ModificationStatusId,
		c.CreatedBy,
		c.CreationDate,
		c.LastModifiedBy,
		c.LastModificationDate
	from  DI_ChangeRequests c join DI_DrugItemNDC n
		on c.DrugItemNDCId = n.DrugItemNDCId
	where c.ContractId = @ContractId
	and c.LastModificationDate between @StartDate and @EndDate 

	order by n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode


END