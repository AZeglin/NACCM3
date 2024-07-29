IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[CreateDrugItemPriceSpreadsheetModification]') AND type in (N'P', N'PC'))
DROP PROCEDURE [CreateDrugItemPriceSpreadsheetModification]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [CreateDrugItemPriceSpreadsheetModification]
(
@ChangeId uniqueidentifier,
@ContractNumber nvarchar(20),
@ModificationNumber nvarchar(20),
@UserName nvarchar(120),
@SpreadsheetFileName nvarchar(256),
@AllowNDCChanges bit, /* 4 flags added for 2010 */
@AllowRemoval bit,
@AllowItemChanges bit,
@ErrorOnUnallowedChanges bit,
@ModificationStatusId int OUTPUT
)

AS

BEGIN

	DECLARE @error int,
	@errorMsg nvarchar( 128 )

	insert into DI_ModificationStatus
	( ChangeId, ContractNumber, ModificationNumber, ModifiedBy, ModificationDate, SpreadsheetFileName, ModificationType, ModificationStatus, AllowNDCChanges, AllowRemoval, AllowItemChanges, ErrorOnUnallowedChanges )
	values
	( @ChangeId, @ContractNumber, @ModificationNumber, @UserName, getdate(), @SpreadsheetFileName, 'SS', 'IN', @AllowNDCChanges, @AllowRemoval, @AllowItemChanges, @ErrorOnUnallowedChanges )

	select @ModificationStatusId = @@IDENTITY, @error = @@ERROR

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting into DI_ModificationStatus for contract ' + @ContractNumber
		RAISERROR( @errorMsg, 16, 1 )
	END

END

