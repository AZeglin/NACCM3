IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'TestScenario2009-1')
	BEGIN
		DROP  Procedure  TestScenario2009-1
	END

GO

CREATE Procedure TestScenario2009-1
(
@SimulatedContractNumber nvarchar(20),
@NDC1 char(5),
@NDC2 char(4),
@NDC3 char(2),
@Covered nchar(1),
)

AS

	DECLARE @ContractId int,
	@CurrentUser uniqueidentifier,
	@DrugItemId int OUTPUT,
	@DrugItemNDCId int OUTPUT 


BEGIN

	insert into DI_Contracts
	( ContractNumber, ModificationStatusId, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	values
	( @SimulatedContractNumber, -1, 'history test', getdate(), 'history test', getdate() )
	
	select @ContractId = @@IDENTITY
	
	print 'Created new contract with id= ' + convert( nvarchar(20), @ContractId )

	select @CurrentUser = ''

	print 'Selecting baseline drug item and history content for new contract'
	
	select * from DI_DrugItems where ContractId = @ContractId

	select * from DI_DrugItemsHistory where ContractId = @ContractId

	print 'Calling InsertFSSDrugItem for new test NDC'
	
	exec dbo.InsertFSSDrugItem  @CurrentUser, @ContractNumber, @NDC1, @NDC2, @NDC3, @Covered, 'Generic', 'Trade Name', 'Unit',
	'package', -1, -1, @DrugItemId  OUTPUT, @DrugItemNDCId OUTPUT 

	print 'InsertFSSDrugItem returned drug item id= ' + convert( nvarchar(20), @DrugItemId ) + ' and NDC id = ' + convert( nvarchar(20), @DrugItemNDCId ) 

	



	delete DI_Contracts
	where ContractId = @contractId
	
	print 'Deleted simulated contract'


END
