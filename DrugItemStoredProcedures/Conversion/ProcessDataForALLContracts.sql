IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessDataForALLContracts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessDataForALLContracts]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [ProcessDataForALLContracts]
As 

Set nocount on

	Exec ProcessContractsData
	Exec ProcessItemsData
	Exec ProcessItemPacakgeData
	Exec ProcessItemPriceData


