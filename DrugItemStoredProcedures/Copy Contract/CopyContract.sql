IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopyContract')
	BEGIN
		DROP  Procedure  CopyContract
	END

GO

CREATE Procedure [dbo].[CopyContract]
(
@CurrentUser uniqueidentifier,
@DrugItemServerName nvarchar(255),
@DrugItemDatabaseName nvarchar(255),
@OldContractNumber nvarchar(50),
@NewContractNumber nvarchar(50),
@AwardDate datetime,
@EffectiveDate datetime,
@ExpirationDate datetime,
@OptionYears int,
@IsItemVersion2 bit,
@NewContractId int OUTPUT
)
As
	Declare @oldContractRecordId int,
		@CopyContractLogId int,
		@userName nvarchar(120),
		@error int,
		@rowcount int,
		@errorMsg nvarchar(250),
		@retVal int,
		@SQL nvarchar(2400),
		@SQLParms nvarchar(1000)


	BEGIN TRANSACTION
		IF exists (Select top 1 1 from tbl_Cntrcts Where CntrctNum = @NewContractNumber)
		BEGIN
			select @errorMsg = 'New Contract number already exists: ' + @NewContractNumber
			GOTO ERROREXIT			
		End	
	
		IF exists (Select top 1 1 from tbl_Cntrcts Where CntrctNum = @OldContractNumber)
		BEGIN
			Select @oldContractRecordId = Contract_Record_Id
			From tbl_Cntrcts Where CntrctNum = @OldContractNumber
			
			Select @userName = UserName 
			From tlkup_userprofile
			Where UserId = @CurrentUser

			IF @userName is null
			Begin
				select @errorMsg = 'Unable to get User Information for UserId: ' + cast(@CurrentUser as nvarchar(120))
				GOTO ERROREXIT			
			End
		
			Insert Into tbl_Cntrcts
				(CntrctNum,Schedule_Number,CO_ID,Contractor_Name,DUNS,TIN,PV_Participation,Solicitation_Number,
				Primary_Address_1,Primary_Address_2,Primary_City,Primary_State, Primary_CountryId, Primary_Zip,POC_Primary_Name,
				POC_Primary_Phone,POC_Primary_Ext,POC_Primary_Fax,POC_Primary_Email,POC_VendorWeb,POC_Notes,
				POC_Alternate_Name,POC_Alternate_Phone,POC_Alternate_Ext,POC_Alternate_Fax,POC_Alternate_Email,
				POC_Emergency_Name,POC_Emergency_Phone,POC_Emergency_Ext,POC_Emergency_Fax,POC_Emergency_Email,
				POC_Tech_Name,POC_Tech_Phone,POC_Tech_Ext,POC_Tech_Fax,POC_Tech_Email,Socio_VetStatus_ID,
				Socio_Business_Size_ID,Socio_SDB,Socio_8a,Socio_Woman,Socio_HubZone,Discount_Basic,Discount_Credit_Card,
				Discount_Prompt_Pay,Discount_Quantity,Geographic_Coverage_ID,Tracking_Customer,Mininum_Order,
				Delivery_Terms,Expedited_Delivery_Terms,Annual_Rebate,BF_Offer,Credit_Card_Accepted,Hazard,
				Warranty_Duration,Warranty_Notes,IFF_Type_ID,Ratio,Returned_Goods_Policy_Type,Returned_Goods_Policy_Notes,
				Incentive_Description,Dist_Manf_ID,Ord_Address_1,Ord_Address_2,Ord_City, Ord_CountryId, Ord_State,Ord_Zip,Ord_Telephone,
				Ord_Ext,Ord_Fax,Ord_EMail,Estimated_Contract_Value,Dates_CntrctAward,Dates_Effective,Dates_CntrctExp,
				Dates_Completion,Dates_TotOptYrs,Pricelist_Verified,Verification_Date,Verified_By,Current_Mod_Number,
				Pricelist_Notes,SBAPlanID,VA_DOD,Terminated_Convenience,Terminated_Default,Drug_Covered,
				BPA_FSS_Counterpart,VA_IFF,OGA_IFF,Cost_Avoidance,ICD_Exempt,On_GSA_Advantage,SBA_Plan_Exempt,
				Insurance_Policy_Effective_Date,Insurance_Policy_Expiration_Date,Solicitation_ID,Offer_ID,
				[65IB_Contract_Type],POC_Sales_Name,POC_Sales_Phone,POC_Sales_Ext,POC_Sales_Fax,POC_Sales_Email,
				TradeAgreementActCompliance,VietnamVetOwned,StimulusAct,CreatedBy, CreationDate, LastModifiedBy,LastModificationDate)
			Select 
				@NewContractNumber,Schedule_Number,CO_ID,Contractor_Name,DUNS,TIN,PV_Participation,Solicitation_Number,
				Primary_Address_1,Primary_Address_2,Primary_City,Primary_State, Primary_CountryId, Primary_Zip,POC_Primary_Name,
				POC_Primary_Phone,POC_Primary_Ext,POC_Primary_Fax,POC_Primary_Email,POC_VendorWeb,POC_Notes,
				POC_Alternate_Name,POC_Alternate_Phone,POC_Alternate_Ext,POC_Alternate_Fax,POC_Alternate_Email,
				POC_Emergency_Name,POC_Emergency_Phone,POC_Emergency_Ext,POC_Emergency_Fax,POC_Emergency_Email,
				POC_Tech_Name,POC_Tech_Phone,POC_Tech_Ext,POC_Tech_Fax,POC_Tech_Email,Socio_VetStatus_ID,
				Socio_Business_Size_ID,Socio_SDB,Socio_8a,Socio_Woman,Socio_HubZone,Discount_Basic,Discount_Credit_Card,
				Discount_Prompt_Pay,Discount_Quantity,Geographic_Coverage_ID,Tracking_Customer,Mininum_Order,
				Delivery_Terms,Expedited_Delivery_Terms,Annual_Rebate,BF_Offer,Credit_Card_Accepted,Hazard,
				Warranty_Duration,Warranty_Notes,IFF_Type_ID,Ratio,Returned_Goods_Policy_Type,Returned_Goods_Policy_Notes,
				Incentive_Description,Dist_Manf_ID,Ord_Address_1,Ord_Address_2,Ord_City, Ord_CountryId, Ord_State,Ord_Zip,Ord_Telephone,
				Ord_Ext,Ord_Fax,Ord_EMail,Estimated_Contract_Value,@AwardDate,@EffectiveDate,@ExpirationDate,
/*				
				Case 
					When Dates_CntrctExp is not null Then Null --and DateDiff("d",getdate(),Dates_CntrctExp)  < 0 
					Else Dates_CntrctExp
				End,
*/				
				Null,				
				@OptionYears,Pricelist_Verified,Verification_Date,Verified_By,Current_Mod_Number,
				Pricelist_Notes,SBAPlanID,VA_DOD,Terminated_Convenience,Terminated_Default,Drug_Covered,
				BPA_FSS_Counterpart,VA_IFF,OGA_IFF,Cost_Avoidance,ICD_Exempt,On_GSA_Advantage,SBA_Plan_Exempt,
				Insurance_Policy_Effective_Date,Insurance_Policy_Expiration_Date,Solicitation_ID,Offer_ID,
				65IB_Contract_Type,POC_Sales_Name,POC_Sales_Phone,POC_Sales_Ext,POC_Sales_Fax,POC_Sales_Email,
				TradeAgreementActCompliance,VietnamVetOwned,StimulusAct, @userName, getdate(), @userName, getdate()
			From tbl_Cntrcts
			Where CntrctNum = @OldContractNumber

			Select @NewContractId = SCOPE_IDENTITY(), @error = @@error
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting tbl_cntrcts for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END	
			
			Insert Into tbl_CopyContractsLog
			(
				OldContractNum,OldContractRecordId,NewContractNum,NewContractRecordId,TotalStateCoveragesCopied,	
				TotalSINSCopied,TotalPriceListItems,TotalBPAPriceListItems,
				UserGuid,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
			)
			Select 
				@OldContractNumber,@oldContractRecordId,@NewContractNumber,@NewContractId,
				0,0,0,0,@CurrentUser,@userName,
				GetDate(),@userName,GetDate()

			Select @CopyContractLogId = SCOPE_IDENTITY(), @error = @@error
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting tbl_CopyContractsLog for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END	
			
			Exec @retVal = CopyBPALookupForContract @CopyContractLogId,@OldContractNumber,@NewContractNumber,@userName
			SELECT @error = @@ERROR
			IF @retVal = -1 OR @error > 0					
			BEGIN
				select @errorMsg = 'Error returned when copying BPALookup for contract: ' + @NewContractNumber
  				GOTO ERROREXIT
			END			

			Exec @retVal = CopySINSForContract @CopyContractLogId,@OldContractNumber,@NewContractNumber,@userName
			SELECT @error = @@ERROR
			IF @retVal = -1 OR @error > 0					
			BEGIN
				select @errorMsg = 'Error returned when copying sins for contract: ' + @NewContractNumber
  				GOTO ERROREXIT
			END			
			
			Exec @retVal = CopyStateCoverageForContract2 @CopyContractLogId,@OldContractNumber,@NewContractNumber,@userName
			SELECT @error = @@ERROR
			IF @retVal = -1 OR @error > 0					
			BEGIN
				select @errorMsg = 'Error returned when copying state coverage (2) for contract: ' + @NewContractNumber
  				GOTO ERROREXIT
			END	
			
			Exec @retVal = CopySAMVendorInfoForContract @CopyContractLogId, @OldContractNumber, @NewContractNumber, @oldContractRecordId, @NewContractId, @userName
			SELECT @error = @@ERROR
			IF @retVal = -1 OR @error > 0					
			BEGIN
				select @errorMsg = 'Error returned when copying SAM vendor info for contract: ' + @NewContractNumber
  				GOTO ERROREXIT
			END	

			if @IsItemVersion2 = 1
			BEGIN
				Exec @retVal = CopyMedSurgItemsForContract2 @CopyContractLogId, @OldContractNumber, @oldContractRecordId, @NewContractNumber, @NewContractId, @EffectiveDate, @ExpirationDate, @userName
				SELECT @error = @@ERROR
				IF @retVal = -1 OR @error > 0					
				BEGIN
					select @errorMsg = 'Error returned when copying Medsurg items (2) for contract: ' + @NewContractNumber
  					GOTO ERROREXIT
				END
			END
			else
			BEGIN
				Exec @retVal = CopyMedSurgItemsForContract @CopyContractLogId,@OldContractNumber,@NewContractNumber,@EffectiveDate,@ExpirationDate,@userName
				SELECT @error = @@ERROR
				IF @retVal = -1 OR @error > 0					
				BEGIN
					select @errorMsg = 'Error returned when copying Medsurg items (1) for contract: ' + @NewContractNumber
  					GOTO ERROREXIT
				END
			

				Exec @retVal = CopyBPAItemsForContract @CopyContractLogId,@OldContractNumber,@NewContractNumber,@EffectiveDate,@ExpirationDate,@userName
				SELECT @error = @@ERROR
				IF @retVal = -1 OR @error > 0					
				BEGIN
					select @errorMsg = 'Error returned when copying BPA items for contract: ' + @NewContractNumber
  					GOTO ERROREXIT
				END

			END
			select @SQL = N'exec @retVal_parm = [' + @DrugItemServerName + '].[' + @DrugItemDatabaseName + '].dbo.CopyDrugItemContract @CopyContractLogId  = @CopyContractLogId_parm,
                @OldContractNumber = @OldContractNumber_parm,
                @NewContractNumber = @NewContractNumber_parm,
                @EffectiveDate = @EffectiveDate_parm,
                @ExpirationDate = @ExpirationDate_parm,
                @oldContractRecordId = @oldContractRecordId_parm,
                @userName = @userName_parm,
                @NewContractId = @NewContractId_parm'

 			select @SQLParms = N'@CopyContractLogId_parm int,  
 								@OldContractNumber_parm nvarchar(50),
 								@NewContractNumber_parm nvarchar(50),
 								@EffectiveDate_parm datetime,
 								@ExpirationDate_parm datetime,
 								@oldContractRecordId_parm int,
 								@userName_parm nvarchar(120),
 								@NewContractId_parm int,
 								@retVal_parm int OUTPUT'

       		Exec SP_executeSQL @SQL, @SQLParms, @CopyContractLogId_parm = @CopyContractLogId, 
       											@OldContractNumber_parm = @OldContractNumber,
       											@NewContractNumber_parm = @NewContractNumber,
       											@EffectiveDate_parm = @EffectiveDate,
       											@ExpirationDate_parm = @ExpirationDate,
       											@oldContractRecordId_parm = @oldContractRecordId,
       											@userName_parm = @userName,
       											@NewContractId_parm = @NewContractId,     											
       											@retVal_parm = @retVal OUTPUT

		/* Exec @retVal = AMMHINDEVDB.DrugItem.dbo.CopyDrugItemContract 
							@CopyContractLogId,@OldContractNumber,@NewContractNumber,
							@EffectiveDate,@ExpirationDate,@oldContractRecordId,@userName,
							@NewContractId  */
							
			SELECT @error = @@ERROR
			
			IF @retVal = -1 OR @error > 0					
			BEGIN
				select @errorMsg = 'Error returned when copying  drug item contract for contract: ' + @NewContractNumber
  				GOTO ERROREXIT
			END	
			
		END
		ELSE
		BEGIN
			select @errorMsg = 'Contract: ' + @NewContractNumber + ' cannot be found'
			GOTO ERROREXIT
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


	
