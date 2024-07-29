IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetParentDrugItemDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetParentDrugItemDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure GetParentDrugItemDetails
(
@CurrentUser uniqueidentifier,
@ParentDrugItemId int
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
	
BEGIN
	
	select i.DrugItemNDCId,
		n.FdaAssignedLabelerCode,    
		n.ProductCode,       
		n.PackageCode,          
		i.PackageDescription,        
		i.DispensingUnit,
		i.Generic ,   
		i.TradeName ,     
		i.Covered,         
		p.Price as CurrentFSSPrice,
		p.PriceStartDate as PriceStartDate,
		p.PriceStopDate as PriceEndDate
	from DI_DrugItems i left outer join DI_DrugItemPrice p
		on i.DrugItemId = p.DrugItemId
		and p.IsFSS = 1
		and getdate() between p.PriceStartDate and p.PriceStopDate 
	  join DI_DrugItemNDC n
		on i.DrugItemNDCId = n.DrugItemNDCId
	where i.DrugItemId = @ParentDrugItemId    
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving parent drug items for parent item id = ' + convert( nvarchar(20), @ParentDrugItemId )
		raiserror( @errorMsg, 16, 1 )
	END


END

