IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessDuplicateNDCItems]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessDuplicateNDCItems]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[ProcessDuplicateNDCItems]
As

Declare @error int,
		@errorMsg nvarchar(512),
		@contractId int,
		@drugItemIdWithNIdNull int,
		@drugItemIdWithNIdNotNull int,
		@drugItemNDCId int,
		@NDCWithNIdNull int,
		@NDCWithNIdNotNull int,
		@contractNumber nvarchar(20),
		@ndc1 char(5),
		@ndc2 char(4),
		@ndc3 char(2),
		@n	char(1)

	Declare Items_Cursor CURSOR For
/*		Select a.DrugitemID
		From di_Drugitems a
		join 
		( 
			Select contractId,DrugitemNDCID From di_Drugitems
			group by contractId,DrugitemNDCID
			having count(*) > 1
		) b
		on a.contractid = b.contractid
		and a.drugitemndcid = b.drugitemndcid
*/

		Select ContractNumber,FdaAssignedLabelerCode,ProductCode,PackageCode,N 
		From LogDuplicateNDC

	Open Items_Cursor
	FETCH NEXT FROM Items_Cursor
	INTO @contractNumber,@ndc1,@ndc2,@ndc3,@n

	WHILE @@FETCH_STATUS = 0
	BEGIN		
		/*If Exists (Select top 1 1 From Di_DrugItemPrice Where DrugItemId = @drugItemId
					And pricestopdate >= getdate()-1
				   )
		Begin
			Update di_Drugitems
			Set Active = 1
			Where DrugItemId = @drugItemId
		End
*/
		Select @drugItemIdWithNIdNull = Null,@contractid = Null, @drugItemIdWithNIdNotNull = Null,
				@drugItemNDCId = Null,@NDCWithNIdNull = Null,@NDCWithNIdNotNull = Null

		Select @contractId = ContractId
		From DI_Contracts
		Where ContractNumber = @contractNumber

		Select @drugItemNDCId = DrugItemNDCId
		From Di_DrugItemNDC
		Where FDAAssignedLabelerCode = @ndc1
		And ProductCode = @ndc2
		And PackageCode = @ndc3

		Select @NDCWithNIdNull = NDCWithNId
		From Di_DrugItemNDCWithN
		Where DrugItemNDCId = @drugItemNDCId
		And n is null

		Select @NDCWithNIdNotNull = NDCWithNId
		From Di_DrugItemNDCWithN
		Where DrugItemNDCId = @drugItemNDCId
		And N = @n

		Select @drugItemIdWithNIdNull = DrugItemId
		From Di_DrugItems
		Where ContractId = @contractId
		And DrugItemNDCId = @drugItemNDCId
		And NDCWithNId = @NDCWithNIdNull

		Select @drugItemIdWithNIdNotNull = DrugItemId
		From Di_DrugItems
		Where ContractId = @contractId
		And DrugItemNDCId = @drugItemNDCId
		And NDCWithNId = @NDCWithNIdNotNull

		Update Di_DrugItemPrice
		Set DrugItemId = @drugItemIdWithNIdNull
		Where DrugItemId = @drugItemIdWithNIdNotNull

		FETCH NEXT FROM Items_Cursor
		INTO @contractNumber,@ndc1,@ndc2,@ndc3,@n
	End
	Close Items_Cursor
	DeAllocate Items_Cursor


