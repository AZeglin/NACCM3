IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetCoveredFCPCountForDrugContract]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetCoveredFCPCountForDrugContract]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[GetCoveredFCPCountForDrugContract]
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@CoveredCount int OUTPUT,
@FCPCount int OUTPUT,
@PPVCount int OUTPUT
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@month int,
	@year int
	
BEGIN

	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId from contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
	
	create table #CoveredCount
	(
		CoveredDrugItemId int not null,
		FCP decimal( 18, 2 ) null
	)
	
	insert into #CoveredCount
	( CoveredDrugItemId )
	select i.DrugItemId
	from DI_DrugItems i
	where i.ContractId = @ContractId    
	and ( i.DiscontinuationDate >= CONVERT( datetime, CONVERT( varchar(50), getdate(), 101 )) or i.DiscontinuationDate is null ) 
	and i.Covered = 'T'
	and i.ExcludeFromExport = 0
	-- adding this clause for 2012 PL due to change in process for discontinuing items, need to remove this when disc date is used again
	-- and exists ( select p.DrugItemPriceId from DI_DrugItemPrice p where p.DrugItemId = i.DrugItemId ) -- removing 5/23/2017

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting covered drug items for contract ' + @ContractNumber
		Drop table #CoveredCount
		raiserror( @errorMsg, 16, 1 )
	END		
	
	-- works differently during public law period
	select @month = MONTH(getdate())
	if @month = 10 or @month = 11 or @month = 12
	BEGIN
		select @year = YEAR(getdate()) + 1
	END
	else
	BEGIN
		select @year = YEAR(getdate()) 
	END
	
	update #CoveredCount 
	set FCP = dbo.GetFCPValueForDrugItem( c.CoveredDrugItemId, @year )
	from #CoveredCount c
	
	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error counting fcp values for contract ' + @ContractNumber
		Drop table #CoveredCount
		raiserror( @errorMsg, 16, 1 )
	END		
	
	select @CoveredCount = count(*) 
	from #CoveredCount

	select @FCPCount = count(*) 
	from #CoveredCount
	where FCP is not null
	and FCP <> 0
	
	Drop table #CoveredCount
	
	select @PPVCount = count(*)
	from DI_DrugItems i
	where i.ContractId = @ContractId
	and ( i.DiscontinuationDate >= CONVERT( datetime, CONVERT( varchar(50), getdate(), 101 )) or i.DiscontinuationDate is null ) 
	and i.PrimeVendor = 'T'
	 
END
