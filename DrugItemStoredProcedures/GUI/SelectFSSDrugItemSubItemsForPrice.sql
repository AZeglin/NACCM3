IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectFSSDrugItemSubItemsForPrice')
	BEGIN
		DROP  Procedure  SelectFSSDrugItemSubItemsForPrice
	END

GO

CREATE Procedure SelectFSSDrugItemSubItemsForPrice
(
@CurrentUser uniqueidentifier,
@DrugItemId int
)

AS

DECLARE
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	
	@DrugItemSubItemId int,
	@SubItemIdentifier as nchar(1)

BEGIN


		/* 'non-selected' blank row definition */
		select @DrugItemSubItemId = -1,
		@SubItemIdentifier = ' '
	
	
		select  DrugItemSubItemId, DrugItemId, SubItemIdentifier
		from DI_DrugItemSubItems
		where DrugItemId = @DrugItemId
		
		union
		
		select @DrugItemSubItemId as DrugItemSubItemId,
			@DrugItemId as DrugItemId,
			@SubItemIdentifier as SubItemIdentifier
			
	
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving subitems for DrugItemId=' + convert( nvarchar(20), @DrugItemId )
			raiserror( @errorMsg, 16, 1 )
		END
	
END

