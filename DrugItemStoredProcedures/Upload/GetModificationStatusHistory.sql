IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetModificationStatusHistory]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetModificationStatusHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [GetModificationStatusHistory]
(
@ContractNumber nvarchar(20),
@StartDate datetime,
@EndDate datetime
)

AS

BEGIN
	
	select ModificationStatusId, ChangeId, ModificationNumber, ModifiedBy, ModificationDate, SpreadsheetFileName,
	ModificationType, ModificationStatus, UploadedBy, UploadDate
	from DI_ModificationStatus
	where ContractNumber = @ContractNumber
	and ModificationDate between @StartDate and @EndDate

END


