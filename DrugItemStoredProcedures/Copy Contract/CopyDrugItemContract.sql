IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopyDrugItemContract')
	BEGIN
		DROP  Procedure  CopyDrugItemContract
	END

GO

CREATE Procedure CopyDrugItemContract
(
	@CopyContractLogId int,
	@OldContractNumber nvarchar(50),
	@NewContractNumber nvarchar(50),
	@EffectiveDate datetime,
	@ExpirationDate datetime,
	@oldContractRecordId int,
	@userName nvarchar(120),
	@NewContractId int
)
As

	Declare @count int,
			@countDrugItems int,
			@countPackage int,			
			@countPriceItems int,			
			@error int,
			@rowcount int,
			@errorMsg nvarchar(250),
			@retVal int,
			@contractId int,
			@NewDrugItemId int,
			@drugItemId int,
			@newDrugItemContractId int
			

	BEGIN TRANSACTION
	
		-- as a precaution, since this happened
		if exists( select ContractId 
					from DI_Contracts
					where NACCMContractNumber = @NewContractNumber )
		BEGIN
			select @errorMsg = 'Error inserting into DI_Contracts for contract: ' + @NewContractNumber + ' the new contract number already exists'
			GOTO ERROREXIT
		END
		
		if exists( select ContractId 
			from DI_Contracts
			where NACCMContractId = @NewContractId )
		BEGIN
			select @errorMsg = 'Error inserting into DI_Contracts for contract: ' + @NewContractNumber + ' the new contract id already exists'
			GOTO ERROREXIT
		END
	
		Select @contractId = ContractId 
		From DI_Contracts
		Where NACCMContractId = @oldContractRecordId
		
		IF @contractId > 0 
		BEGIN
			Insert into DI_Contracts
			(ContractNumber,NACCMContractNumber,NACCMContractId,ParentFSSContractId,
			 ModificationStatusID,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
			)
			Select @NewContractNumber,@NewContractNumber,@NewContractId,ParentFSSContractId,
					ModificationStatusID,@userName,GETDATE(),@userName,GETDATE()
			From DI_Contracts
			Where ContractId = @contractId

			Select @newDrugItemContractId = @@identity, @error = @@ERROR			
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting DI_Contracts for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END	

			Insert into DI_ContractNDCNumberChange
			(
				NewContractId,OldContractId,ChangeStatus,ModificationId,EffectiveDate,EndDate,
				LastModifiedBy,LastModificationDate
			)
			Select 
				@newDrugItemContractId,@contractId,'C',-1,@EffectiveDate,@ExpirationDate,
				@userName, getdate()

			Select @error = @@ERROR			
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting DI_ContractNDCNumberChange for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END					

			Select 	@countDrugItems = COUNT(*)
			From DI_DrugItems 
			Where ContractId = @contractId
			And (DateDiff("d",GETDATE(),DiscontinuationDate) > 0 or DiscontinuationDate is null)
			

			IF @countDrugItems is not null and @countDrugItems > 0
			Begin
				Declare Items_Cursor cursor For 
					Select DrugItemId						
					From DI_DrugItems 
					Where ContractId = @contractId
					And (DateDiff("d",GETDATE(),DiscontinuationDate) > 0	or DiscontinuationDate is null)				 
					
				Open Items_Cursor
				FETCH NEXT FROM Items_Cursor
				INTO @drugItemId

				WHILE @@FETCH_STATUS = 0
				BEGIN			
					Insert into DI_DrugItems 
					(
					 ContractId,DrugItemNDCId,HistoricalNValue,PackageDescription,Generic,
					 TradeName,DiscontinuationDate,DiscontinuationEnteredDate,
					 DateEnteredMarket,Covered,PrimeVendor,PrimeVendorChangedDate,
					 PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
					 ExcludeFromExport, NonTAA, IncludedFETAmount,ParentDrugItemId,LastModificationType,
					 ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,
					 LastModificationDate
					)
					Select 
					 @newDrugItemContractId,DrugItemNDCId,HistoricalNValue,PackageDescription,Generic,
					 TradeName,null,GETDATE(),GETDATE(),Covered,PrimeVendor,
					 GETDATE(),PassThrough,DispensingUnit,VAClass,DualPriceDesignation,
					 ExcludeFromExport,NonTAA, IncludedFETAmount,ParentDrugItemId,'Y',0,@userName,GETDATE(),
					 @userName,GETDATE()
					 From DI_DrugItems 
					 Where DrugItemId = @drugItemId
					 
					Select @NewDrugItemId = @@identity, @error = @@ERROR		
					IF @error <> 0
					BEGIN
						select @errorMsg = 'Error inserting DI_DrugItems for contract: ' + @NewContractNumber
						GOTO ERROREXIT
					END					 

					Insert into DI_DrugItemPackage 			
					(DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure,
					 ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					Select 
					@NewDrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,UnitOfMeasure,
					0,@userName,GETDATE(),@userName,GETDATE()
					From DI_DrugItemPackage 
					Where DrugItemId = @drugItemId

					Select @error = @@ERROR		
					IF @error <> 0
					BEGIN
						select @errorMsg = 'Error inserting DI_DrugItemPackage for contract: ' + @NewContractNumber
						GOTO ERROREXIT
					END						

					insert into DI_DrugItemDistributors
					( DrugItemId, DistributorName, Address1, Address2, City, State, Zip, Phone, Extension, Fax, Email, ContactPerson,
						WebPage, Notes, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
					select
					@NewDrugItemId, DistributorName, Address1, Address2, City, State, Zip, Phone, Extension, Fax, Email, ContactPerson,
						WebPage, Notes, @userName,GETDATE(),@userName,GETDATE()
					from DI_DrugItemDistributors
					where DrugItemId = @drugItemId

					Select @error = @@ERROR		
					IF @error <> 0
					BEGIN
						select @errorMsg = 'Error inserting DI_DrugItemDistributors for contract: ' + @NewContractNumber
						GOTO ERROREXIT
					END		


					Insert into DI_DrugItemPrice 			
					(DrugItemId,DrugItemSubItemId,HistoricalNValue,PriceId,PriceStartDate,PriceStopDate,Price,
					 IsTemporary,IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,
					 IsSVH1,IsSVH2,IsTMOP,IsUSCG,IsFHCC,AwardedFSSTrackingCustomerRatio,TrackingCustomerName,
					 CurrentTrackingCustomerPrice,ExcludeFromExport,LastModificationType,ModificationStatusId,
					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					select
					@NewDrugItemId,DrugItemSubItemId,HistoricalNValue,PriceId,@EffectiveDate,@ExpirationDate,Price,
					 IsTemporary,IsFSS,IsBIG4,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,
					 IsSVH1,IsSVH2,IsTMOP,IsUSCG,IsFHCC,AwardedFSSTrackingCustomerRatio,TrackingCustomerName,
					 CurrentTrackingCustomerPrice,ExcludeFromExport,'Y',0,
					 @userName,GETDATE(),@userName,GETDATE()
					From DI_DrugItemPrice 
					Where DrugItemId = @drugItemId
					and IsTemporary = 0    /* fix requested by Lydia on 10/2/2012 */
					--and DATEDIFF(dd,GETDATE(), PriceStopDate) >= 0
					-- should copy only active base prices 
					and	datediff( d, PriceStopDate, GETDATE() ) <= 0 
					and datediff( d, PriceStartDate, GETDATE() ) >= 0  

					Select @error = @@ERROR		
					IF @error <> 0
					BEGIN
						select @errorMsg = 'Error inserting DI_DrugItemPrice for contract: ' + @NewContractNumber
						GOTO ERROREXIT
					END				
		
					FETCH NEXT FROM Items_Cursor
					INTO @drugItemId
				END
				Close Items_Cursor
				DeAllocate Items_Cursor
			End
			
			Select @countPackage = COUNT(*)
			From DI_DrugItemPackage
			Where DrugItemId in 
			(
				Select DrugItemId 
				From DI_DrugItems 
				Where ContractId = @newDrugItemContractId
			)
			
			Select @countPriceItems = COUNT(*)
			From DI_DrugItemPrice
			Where DrugItemId in 						
			(
				Select DrugItemId 
				From DI_DrugItems 
				Where ContractId = @newDrugItemContractId
			)
			
		
			Insert into DI_CopyContractsLog
			(LogIdFromSource,NewDrugItemContractId,OldDrugItemContractID,
			 TotalDrugItemContracts,TotalDrugItems,TotalDrugItemPackage,TotalDrugItemPrice,
			 CreatedBy,CreationDate
			 )
			Select @CopyContractLogId,@newDrugItemContractId,
					@ContractId,1,@countDrugItems,
					@countPackage,@countPriceItems,@userName,GETDATE()
		
	
			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting di_CopyContractsLog for contract: ' + @NewContractNumber
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
