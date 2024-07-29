IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DeleteTieredPriceForDrugItemPrice]') AND type in (N'P', N'PC'))
DROP PROCEDURE [DeleteTieredPriceForDrugItemPrice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[DeleteTieredPriceForDrugItemPrice]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ModificationStatusId int,
@DrugItemTieredPriceId int
)

As
	Declare @error int,@errorMsg nvarchar(1000), @UserName nvarchar(120)

	Begin Tran
	
		EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @UserName OUTPUT 

		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error getting current user login during delete of tiered price for @DrugItemTieredPriceId=' + convert( nvarchar(20), @DrugItemTieredPriceId )
			goto ERROREXIT
		END
	
	
		Insert into DI_DrugItemTieredPriceHistory
		(DrugItemTieredPriceId,DrugItemPriceId,TieredPriceStartDate,TieredPriceStopDate,Price,Minimum,MinimumValue,
		 ModificationStatusId,IsDeleted,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate)
		Select DrugItemTieredPriceId,DrugItemPriceId,TieredPriceStartDate,TieredPriceStopDate,Price,Minimum,MinimumValue,
				@ModificationStatusId,1,CreatedBy,CreationDate,@UserName,getdate()
		From DI_DrugItemTieredPrice
		Where DrugItemTieredPriceId = @DrugItemTieredPriceId

		SELECT @error = @@ERROR
		IF  @error <> 0
		BEGIN
			select @errorMsg = 'Error inserting into DI_DrugItemTieredPriceHistory  for @DrugItemTieredPriceId=' + convert( nvarchar(20), @DrugItemTieredPriceId )
    		GOTO ERROREXIT
		END

		Delete From DI_DrugItemTieredPrice
		Where DrugItemTieredPriceId = @DrugItemTieredPriceId

		SELECT @error = @@ERROR
		IF  @error <> 0
		BEGIN
			select @errorMsg = 'Error deleting from DI_DrugItemTieredPrice  for @DrugItemTieredPriceId=' + convert( nvarchar(20), @DrugItemTieredPriceId )
    		GOTO ERROREXIT
		END

	goto OKEXIT

 
	ERROREXIT:

		  raiserror( @errorMsg, 16, 1 )
		  if @@TRANCOUNT > 1
		  BEGIN
				COMMIT TRANSACTION
		  END
		  Else if @@TRANCOUNT = 1
		  BEGIN
				/* only rollback iff this the highest level */
				ROLLBACK TRANSACTION
		  END

		  RETURN( -1 )

	OKEXIT:
		If @@TRANCOUNT > 0
		BEGIN
			COMMIT TRANSACTION
		END
		RETURN( 0 )
