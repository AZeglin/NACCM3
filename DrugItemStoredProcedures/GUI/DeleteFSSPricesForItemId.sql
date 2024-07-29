IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DeleteFSSPricesForItemId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [DeleteFSSPricesForItemId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create Procedure [dbo].[DeleteFSSPricesForItemId]
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@ModificationStatusId int,
@DrugItemId int
)

As
	Declare @error int,@errorMsg nvarchar(1000), @UserName nvarchar(120),@drugItemPriceId int,
			@retval int

	Begin Tran
	
		EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @UserName OUTPUT 

		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error getting current user login during delete of all item prices for @DrugItemId=' + convert( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END


	Declare Item_Cursor CURSOR For
		Select DrugItemPriceId
		from DI_DrugItemPrice
		Where drugItemid = 	@DrugItemId

	Open Item_Cursor
	FETCH NEXT FROM Item_Cursor
	INTO @drugItemPriceId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Exec @retVal = dbo.DeleteFSSPriceForItemPriceId 
						@CurrentUser, @SecurityServerName, @SecurityDatabaseName, @ContractNumber, @ModificationStatusId, @DrugItemPriceId
					
		SELECT @error = @@ERROR
		IF @retVal = -1 OR @error > 0
		BEGIN
			select @errorMsg = 'Error returned when Inserting price history for contract ' + @ContractNumber
			Close Item_Cursor
			DeAllocate Item_Cursor
			GOTO ERROREXIT
		END		
		
		FETCH NEXT FROM Item_Cursor
		INTO @drugItemPriceId				
	End
	Close Item_Cursor
	DeAllocate Item_Cursor 


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

