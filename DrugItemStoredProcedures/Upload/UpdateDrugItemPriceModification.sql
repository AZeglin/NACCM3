IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UpdateDrugItemPriceModification]') AND type in (N'P', N'PC'))
DROP PROCEDURE [UpdateDrugItemPriceModification]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [UpdateDrugItemPriceModification]
(
@ModificationStatusId int,
@ModificationStatus nchar(2),
@ContractNumber nvarchar(20)
)

AS

BEGIN

	DECLARE @error int,
	@errorMsg nvarchar( 128 )

	update DI_ModificationStatus
	set ModificationStatus = @ModificationStatus,
	ModificationDate = getdate()
	where ModificationStatusId = @ModificationStatusId
	
	select @error = @@ERROR

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error updating DI_ModificationStatus for contract ' + @ContractNumber
		RAISERROR( @errorMsg, 16, 1 )
	END

END


