IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessNDCData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessNDCData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[ProcessNDCData]
As 

Declare 
	@ndc_1 char(5),
	@ndc_2 char(4),
	@ndc_3 char(2),
	@n char(1),
	@identity int,
	@drugItemNDCId int,
	@error int,
	@errorMsg nvarchar(512)

	Declare NDC_Cursor CURSOR For
	Select Distinct a.ndc_1,a.ndc_2,a.ndc_3--,a.n 
	From
	(Select distinct ndc_1,ndc_2,ndc_3--,n 
	from fssdata
	union
	Select distinct ndc1_new as ndc_1,ndc2_new as ndc_2,ndc3_new as ndc_3--,n_new as n
	from ndclink
	union
	Select distinct ndc1_old as ndc_1,ndc2_old as ndc_2,ndc3_old as ndc_3--,n_old as n
	from ndclink
	union
	Select distinct ndc_1,ndc_2,ndc_3--,n 
	from ncfprice
	) a
	Order by a.ndc_1,a.ndc_2,a.ndc_3--,a.n

	Open NDC_Cursor
	FETCH NEXT FROM NDC_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3--,@n

	WHILE @@FETCH_STATUS = 0
	BEGIN
		
		Select @drugItemNDCId = DrugitemNDCId from Di_DrugItemNDC 
		Where FdaAssignedLabelerCode = @ndc_1
			And	  ProductCode = @ndc_2
			And	  PackageCode = @ndc_3 

		
		IF @drugItemNDCId is null
		Begin
			Insert into Di_DrugItemNDC
			(FdaAssignedLabelerCode,ProductCode,PackageCode,ModificationStatusId,CreatedBy,
			 CreationDate,LastModifiedBy,LastModificationDate)
			Select	@ndc_1,@ndc_2,@ndc_3,0,'Re Extract',getdate(),user_name(),getdate()
		End
		Else
		Begin
			Select @errorMsg = 'NDC Already Exists'	
		End


		Select @drugItemNDCId = null

		FETCH NEXT FROM NDC_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3--,@n
	End
	CLose NDC_Cursor
	DeAllocate NDC_Cursor
