IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CreateContract')
	BEGIN
		DROP  Procedure  CreateContract
	END

GO

CREATE Procedure CreateContract
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@DrugItemServerName nvarchar(255),
@DrugItemDatabaseName nvarchar(255),
@ScheduleNumber int,
@ManuallyAssignedContractNumber nvarchar(50),
@AwardDate datetime,
@EffectiveDate datetime,
@ExpirationDate datetime,
@AssignedCOID int,
@VendorName nvarchar(75),
@OptionYears int = null,
@VendorContactName  nvarchar(30) = null,
@VendorContactPhone nvarchar(15) = null,
@VendorContactPhoneExtension  nvarchar(5) = null,
@VendorContactFax   nvarchar(15) = null,
@VendorContactEmail    nvarchar(50) = null,
@VendorAddress1    nvarchar(100) = null,
@VendorAddress2    nvarchar(100) = null,
@VendorAddressCity   nvarchar(20) = null,
@VendorAddressState    nvarchar(2) = null,
@VendorZipCode   nvarchar(10) = null,
@VendorUrl     nvarchar(50) = null,
@OfferId int = null, -- available when created from offer record
@IsRebateRequired bit = null,
@NewContractId int OUTPUT,
@NewPharmaceuticalContractId int OUTPUT,
@GeographicCoverageId int OUTPUT
)

AS

DECLARE @userName nvarchar(30),
		@error int,
		@rowcount int,
		@retVal int,
		@errorMsg nvarchar(250),
		@SQL nvarchar(2400),
		@SQLParms nvarchar(1000)

BEGIN TRANSACTION

		select @NewPharmaceuticalContractId = -1

		select @ManuallyAssignedContractNumber = LTRIM(RTRIM(@ManuallyAssignedContractNumber)) -- $$$ bug fix adding to R2

		if exists (Select top 1 1 from tbl_Cntrcts Where CntrctNum = @ManuallyAssignedContractNumber )
		BEGIN
			select @errorMsg = 'Contract number already exists: ' + @ManuallyAssignedContractNumber
			GOTO ERROREXIT			
		END

		-- this works on 2005+ but not on 2000 when server is same server
		--select @SQL = N'Select @userName_parm = UserName 
		--From [' + @SecurityServerName + '].[' + @SecurityDatabaseName + '].[dbo].[SEC_UserProfile]
		--Where UserId = @CurrentUser_parm'

		select @SQL = N'Select @userName_parm = UserName 
		From [' + @SecurityDatabaseName + '].[dbo].[SEC_UserProfile]
		Where UserId = @CurrentUser_parm'

		select @SQLParms = N'@CurrentUser_parm uniqueidentifier, @userName_parm nvarchar(30) OUTPUT'

		Exec SP_executeSQL @SQL, @SQLParms, @CurrentUser_parm = @CurrentUser, @userName_parm = @userName OUTPUT

		IF @userName is null
		Begin
			select @errorMsg = 'Unable to get User Information for UserId: ' + cast(@CurrentUser as nvarchar(120))
			GOTO ERROREXIT			
		End
			
		if @OfferId = -1
		BEGIN
			select @OfferId = null
		END	
			
		insert Into tbl_Cntrcts
			( CntrctNum, Schedule_Number, CO_ID, Contractor_Name, 
 			Primary_Address_1, Primary_Address_2, Primary_City, Primary_State, Primary_Zip,
 			POC_Primary_Name, POC_Primary_Phone, POC_Primary_Ext, POC_Primary_Fax, POC_Primary_Email, POC_VendorWeb,				
			Dates_CntrctAward, Dates_Effective, Dates_CntrctExp, Offer_ID, RebateRequired, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate ) -- Dates_TotOptYrs, Drug_Covered,
		values
			( @ManuallyAssignedContractNumber, @ScheduleNumber, @AssignedCOID, @VendorName,
			@VendorAddress1, @VendorAddress2, @VendorAddressCity, @VendorAddressState, @VendorZipCode,
			@VendorContactName, @VendorContactPhone, @VendorContactPhoneExtension, @VendorContactFax, @VendorContactEmail, @VendorUrl,
			@AwardDate, @EffectiveDate, @ExpirationDate, @OfferId, @IsRebateRequired, @userName, getdate(), @userName, getdate() ) -- @OptionYears

		select @error = @@error, @rowcount = @@rowcount, @NewContractId = SCOPE_IDENTITY()  -- $$$ change to go with v2.
		
		if @error <> 0 or @rowcount <> 1
		BEGIN
			select @errorMsg = 'Unable to insert new contract for contract number: ' + @ManuallyAssignedContractNumber
			GOTO ERROREXIT					
		END

		exec InsertGeographicCoverageForContract @CurrentUser = @CurrentUser, @SecurityServerName = @SecurityServerName, @SecurityDatabaseName = @SecurityDatabaseName, @ContractNumber = @ManuallyAssignedContractNumber, @GeographicCoverageId = @GeographicCoverageId OUTPUT

		select @error = @@error
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Unable to insert geographic coverage for contract number: ' + @ManuallyAssignedContractNumber
			GOTO ERROREXIT					
		END

		/* if pharmaceutical contract, then create row in DrugItem database */
		/* revised latently ( bug fix ) 10/30/2013	if @ScheduleNumber = 1 OR @ScheduleNumber = 18 OR @ScheduleNumber = 28 OR @ScheduleNumber = 29 OR 
                @ScheduleNumber = 30 OR @ScheduleNumber = 31 OR @ScheduleNumber = 32 OR @ScheduleNumber = 37 OR 
                @ScheduleNumber = 39 OR @ScheduleNumber = 43 OR @ScheduleNumber = 47 OR @ScheduleNumber = 48  */
		if @ScheduleNumber = 1 OR @ScheduleNumber = 8 OR @ScheduleNumber = 18 OR  
                @ScheduleNumber = 30 OR @ScheduleNumber = 31 OR @ScheduleNumber = 32 OR @ScheduleNumber = 37 OR 
                @ScheduleNumber = 39 OR @ScheduleNumber = 47 OR @ScheduleNumber = 48 OR @ScheduleNumber = 52  
		BEGIN
		
			select @SQL = N'exec @retVal_parm = [' + @DrugItemServerName + '].[' + @DrugItemDatabaseName + '].dbo.CreateDrugItemContract @CurrentUser  = @CurrentUser_parm,
				@SecurityServerName = @SecurityServerName_parm,
				@SecurityDatabaseName = @SecurityDatabaseName_parm,
                @ContractNumber = @ManuallyAssignedContractNumber_parm,
                @ContractRecordId = @NewContractId_parm,
                @ModificationStatusId = 0,
                @ScheduleNumber = @ScheduleNumber_parm,
                @ContractId = @NewPharmaceuticalContractId_parm OUTPUT'
                
 			select @SQLParms = N'@CurrentUser_parm uniqueidentifier,  
 								@SecurityServerName_parm nvarchar(255),
 								@SecurityDatabaseName_parm nvarchar(255),
 								@ManuallyAssignedContractNumber_parm nvarchar(50),
 								@NewContractId_parm int,
 								@ScheduleNumber_parm int,
 								@NewPharmaceuticalContractId_parm int OUTPUT,
 								@retVal_parm int OUTPUT'
         
       		Exec SP_executeSQL @SQL, @SQLParms, @CurrentUser_parm = @CurrentUser, 
       											@SecurityServerName_parm = @SecurityServerName,
       											@SecurityDatabaseName_parm = @SecurityDatabaseName,
       											@ManuallyAssignedContractNumber_parm = @ManuallyAssignedContractNumber,
       											@NewContractId_parm = @NewContractId,
       											@ScheduleNumber_parm = @ScheduleNumber,
       											@NewPharmaceuticalContractId_parm = @NewPharmaceuticalContractId OUTPUT,       											
       											@retVal_parm = @retVal OUTPUT
      
		
			select @error = @@error
			
			IF @retVal = -1 OR @error <> 0					
			BEGIN
				select @errorMsg = 'Error returned when creating row for new contract in drug item database for contract: ' + @ManuallyAssignedContractNumber
  				GOTO ERROREXIT
			END	
			
		END               

		-- insert the default SIN for National non-BPA contracts
		if exists ( select Schedule_Number from [tlkup_Sched/Cat] where Division = 2 
															and Inactive = 0
															and Type = 'National' 
															and Schedule_Number = @ScheduleNumber )
		BEGIN
			-- note there should only be one defined for each of this type of schedule
			-- however, if there was more than one it wouldn't matter
			insert into tbl_Cntrcts_SINs 
			( CntrctNum, SINs, Recoverable, Inactive, LexicalSIN, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
			select @ManuallyAssignedContractNumber, s.[SIN], 0, 0, s.[SIN], @userName, getdate(), @userName, getdate() 
			from tbl_SINs s 
			where s.[Schedule_ Number] = @ScheduleNumber
			and s.Inactive = 0
		END

		-- also backfill contract number into offer record and set the status of the offer
		if @OfferId is not null and @OfferId <> -1
		BEGIN
			update tbl_Offers
				set ContractNumber = @ManuallyAssignedContractNumber,
				Action_ID = 10 
			where Offer_ID = @OfferId
		
		
			select @error = @@error, @rowcount = @@rowcount
			
			IF @error <> 0 or @rowcount <> 1
			BEGIN
				select @errorMsg = 'Error returned when updating offer status during new contract creation for contract: ' + @ManuallyAssignedContractNumber
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

