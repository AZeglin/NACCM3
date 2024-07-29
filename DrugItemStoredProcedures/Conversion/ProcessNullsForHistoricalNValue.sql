IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessNullsForHistoricalNValue]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessNullsForHistoricalNValue]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create Proc [ProcessNullsForHistoricalNValue]
As
BEGIN
	Update DI_DrugItemPrice
	Set HistoricalNValue = Null
	Where len(HistoricalNValue) = 0' 
END

