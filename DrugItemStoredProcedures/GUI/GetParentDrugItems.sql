IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetParentDrugItems]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetParentDrugItems]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure GetParentDrugItems
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20)
)

AS

DECLARE @contractId int,
	@parentContractId int,
	@parentContractNumber nvarchar(20),
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	
	@DrugItemId int,
	@DrugItemNDCId int,   
	@FdaAssignedLabelerCode char(5),
	@ProductCode char(4),
	@PackageCode char(2),
	@PackageDescription nvarchar(14),
	@Generic nvarchar(64),
	@TradeName nvarchar(45),
	@DiscontinuationDate  datetime,
	@DiscontinuationEnteredDate  datetime,
	@DiscontinuationReasonId int,
	@Covered nchar(1),

	@CurrentFSSPrice decimal(9,2),
	@PriceStartDate datetime,
	@PriceEndDate datetime,
	@OverallDescription nvarchar(200)

BEGIN
		
	select @contractId = ContractId,
		@parentContractId = ParentFSSContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1 
	BEGIN
		select @errorMsg = 'Error getting contractId from contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
	
	if @parentContractId is null
	BEGIN
		select @errorMsg = 'Parent contract was null for contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
	
	
	select @parentContractNumber = NACCMContractNumber
	from DI_Contracts
	where ContractId = @parentContractId
	
	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0 or @rowcount <> 1 
	BEGIN
		select @errorMsg = 'Error getting contract number for parentContractId ' + convert( nvarchar(20), @parentContractId )
		raiserror( @errorMsg, 16, 1 )
	END
	
	
	-- blank row
	select @DrugItemId = -1,
		@DrugItemNDCId = -1,   
		@FdaAssignedLabelerCode = '',
		@ProductCode = '',
		@PackageCode = '',
		@PackageDescription = '',
		@Generic = '',
		@TradeName = '',
		@DiscontinuationDate = getdate(),
		@DiscontinuationReasonId = -1,
		@Covered = 'F',
		@CurrentFSSPrice = 0,
		@PriceStartDate = getdate(),
		@PriceEndDate = getdate(),
		@OverallDescription = 'None Selected'
		
	select i.DrugItemId as ParentDrugItemId,
		i.DrugItemNDCId,
		n.FdaAssignedLabelerCode,    
		n.ProductCode,       
		n.PackageCode,          
		i.PackageDescription  ,        
		i.Generic ,   
		i.TradeName ,     
		i.DiscontinuationDate,                             	             
		i.DiscontinuationReasonId,
		i.Covered,         
		p.Price as CurrentFSSPrice,
		p.PriceStartDate as PriceStartDate,
		p.PriceStopDate as PriceEndDate,
		n.FdaAssignedLabelerCode + ' ' + n.ProductCode + ' ' + n.PackageCode + ' ' + i.Generic as OverallDescription
	from DI_DrugItems i left outer join DI_DrugItemPrice p
		on i.DrugItemId = p.DrugItemId
		and p.IsFSS = 1
		and getdate() between p.PriceStartDate and p.PriceStopDate 
	  join DI_DrugItemNDC n
		on i.DrugItemNDCId = n.DrugItemNDCId
	where i.ContractId = @parentContractId    
	and i.DiscontinuationDate is null	
	
	union
	
	select @DrugItemId as ParentDrugItemId,
		@DrugItemNDCId as DrugItemNDCId,
		@FdaAssignedLabelerCode as FdaAssignedLabelerCode,
		@ProductCode as ProductCode,
		@PackageCode as PackageCode,
		@PackageDescription as PackageDescription,
		@Generic as Generic,
		@TradeName as TradeName,
		@DiscontinuationDate as DiscontinuationDate,
		@DiscontinuationReasonId as DiscontinuationReasonId,
		@Covered as Covered,
		@CurrentFSSPrice as CurrentFSSPrice,
		@PriceStartDate as PriceStartDate,
		@PriceEndDate as PriceEndDate,
		@OverallDescription as OverallDescription
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving parent drug items for parent contract ' + @parentContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

	if @rowcount = 0
	BEGIN
		select @errorMsg = 'There are no drug items available for parent contract ' + @parentContractNumber
		raiserror( @errorMsg, 16, 1 )
	END

END


