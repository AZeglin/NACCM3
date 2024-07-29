IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetAllActiveNDCForContract]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetAllActiveNDCForContract]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[GetAllActiveNDCForContract]
@contractId int 
As

	Declare @drugitemId int
		
	Declare @NDCTempTable table
	(FdaAssignedLabelerCode Char(5),
	 ProductCode Char(4),
	 PackageCode Char(2)
	)

	Declare NDC_Cursor Cursor For
		Select Distinct a.drugitemId from di_Drugitems a
		join di_drugitemprice b
		on a.drugitemid = b.drugitemid
		where contractid = @contractId

	Open NDC_Cursor
	FETCH NEXT FROM NDC_Cursor
	INTO @drugItemId

	WHILE @@FETCH_STATUS = 0
	BEGIN	

		Insert into	@NDCTempTable
		Select b.FdaAssignedLabelerCode,b.ProductCode,b.PackageCode
		from di_drugitems a
		join di_drugitemndc b
		on a.drugitemndcid = b.drugitemndcid
		where a.drugitemid = @drugItemId

		FETCH NEXT FROM NDC_Cursor
		INTO @drugItemId	
	End
	Close NDC_Cursor
	DeAllocate NDC_Cursor

	Select * From @NDCTempTable
	Order by 1,2,3

