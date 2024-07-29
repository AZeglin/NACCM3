IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetSubItemIdentifierForHistoricalPriceFunction')
	BEGIN
		DROP  Function  GetSubItemIdentifierForHistoricalPriceFunction
	END

GO

CREATE function GetSubItemIdentifierForHistoricalPriceFunction
( 
@DrugItemSubItemId int
)

returns nchar(1)

AS

BEGIN

	DECLARE @SubItemIdentifier nchar(1)
	select @SubItemIdentifier = ''

	if @DrugItemSubItemId is not null
	BEGIN

		if exists( select SubItemIdentifier
			from DI_DrugItemSubItems
			where DrugItemSubItemId = @DrugItemSubItemId )
		BEGIN
			select @SubItemIdentifier = SubItemIdentifier
			from DI_DrugItemSubItems
			where DrugItemSubItemId = @DrugItemSubItemId
		END
		ELSE
		BEGIN
			select @SubItemIdentifier = isnull( SubItemIdentifier, '' )
			from DI_DrugItemSubItemsHistory
			where DrugItemSubItemId = @DrugItemSubItemId	
		END
	END
	
	return @SubItemIdentifier

END