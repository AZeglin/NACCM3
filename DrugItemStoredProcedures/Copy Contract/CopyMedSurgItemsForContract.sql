IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopyMedSurgItemsForContract')
	BEGIN
		DROP  Procedure  CopyMedSurgItemsForContract
	END

GO

CREATE Procedure CopyMedSurgItemsForContract
(
	@CopyContractLogId int,
	@OldContractNumber nvarchar(50),
	@NewContractNumber nvarchar(50),
	@EffectiveDate datetime,
	@ExpirationDate datetime,
	@UserLogin nvarchar(120) 
)
As

	Declare @count int,
			@error int,
			@rowcount int,
			@errorMsg nvarchar(250),
			@retVal int	

	BEGIN TRANSACTION
	
		Select @count = Count(*) 
		From tbl_PriceList
		Where CntrctNum = @OldContractNumber
		and removed = 0

		IF @count = 0
		BEGIN
			Update tbl_CopyContractsLog
				Set TotalPriceListItems = 0
			Where CopyContractLogId = @CopyContractLogId
			
			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error updating tbl_CopyContractsLog for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END			
		END
		ELSE
		BEGIN
			Insert Into tbl_PriceList
				(CntrctNum,[Contractor Catalog Number],[Product Long Description],[FSS Price],
				 [Package Size Priced on Contract],[SIN],[Removed],[Outer Pack UOM],
				 [Outer Pack Unit of Conversion Factor],[Outer Pack Unit Shippable],[Outer Pack UPN],
				 [Intermediate Pack UOM],[Intermediate Pack Unit of Conversion Factor],[Intermediate Pack Shippable],
				 [Intermediate Pack UPN],[Base Packaging UOM],[Base Packaging Unit of Conversion Factor],
				 [Base Packaging Unit Shippable],[Base Packaging UPN],[Tier 1 Price],[Tier 2 Price],[Tier 3 Price],
				 [Tier 4 Price],[Tier 5 Price],[Tier 1 Note],[Tier 2 Note],[Tier 3 Note],[Tier 4 Note],[Tier 5 Note],
				 [621I_Category_ID],[Date_Entered],[Date_Modified],[DateEffective],[ExpirationDate],[CreatedBy],[LastModifiedBy]
				)
			Select 
				 @NewContractNumber,[Contractor Catalog Number],[Product Long Description],[FSS Price],
				 [Package Size Priced on Contract],[SIN],[Removed],[Outer Pack UOM],
				 [Outer Pack Unit of Conversion Factor],[Outer Pack Unit Shippable],[Outer Pack UPN],
				 [Intermediate Pack UOM],[Intermediate Pack Unit of Conversion Factor],[Intermediate Pack Shippable],
				 [Intermediate Pack UPN],[Base Packaging UOM],[Base Packaging Unit of Conversion Factor],
				 [Base Packaging Unit Shippable],[Base Packaging UPN],[Tier 1 Price],[Tier 2 Price],[Tier 3 Price],
				 [Tier 4 Price],[Tier 5 Price],[Tier 1 Note],[Tier 2 Note],[Tier 3 Note],[Tier 4 Note],[Tier 5 Note],
				 [621I_Category_ID],GETDATE(),GETDATE(),@EffectiveDate,@ExpirationDate,@UserLogin,@UserLogin
			From tbl_PriceList
			Where CntrctNum = @OldContractNumber
			And Removed = 0

			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting tbl_PriceList for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END	

			Update tbl_CopyContractsLog
				Set TotalPriceListItems = @count
			Where CopyContractLogId = @CopyContractLogId
			
			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error updating tbl_CopyContractsLog for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END	
			
							
		END


GOTO OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 ) 

	IF @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
	/* only rollback iff this the highest level */ 
		ROLLBACK TRANSACTION
	END

	RETURN (-1)

OKEXIT:
	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)


	