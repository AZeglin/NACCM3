IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TestScenario4]') AND type in (N'P', N'PC'))
DROP PROCEDURE [TestScenario4]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[TestScenario4]
(
@IsExistingFSS bit,   -- can specify 0,0,0 through 1,1,1 in any combination unless testing removal
@IsExistingBig4 bit,
@IsExistingRestricted bit,
@IsNewFSS bit,   -- can only select one of the three
@IsNewBig4 bit,
@IsNewRestricted bit,
@NewUsesThisId int, -- 1,2 or 3 to select which one of the original records to update
@IsExistingTemporary bit,
@IsNewTemporary bit,
@IsRemoval bit,
@IsInsert bit,
@UseSpreadsheetVersion bit, -- 1 = UpdateFSSDrugItemPriceFromSpreadsheet; 0 = UpdateFSSDrugItemPrice
@AddSecondRecord bit, -- adds a second record while history from previous insert is left in-tact
@SimulatedContractNumber nvarchar(20),
@SimulatedDrugItemName nvarchar(45),
@ModificationNumber nvarchar(20) 
)

AS

BEGIN

DECLARE
	@contractId int,
	@modificationStatusId int,
	@drugItemId int,
	@drugItemNDCId int,
	@NewDrugItemPriceId int,
	@drugItemPriceId1 int,
	@drugItemPriceId2 int,
	@drugItemPriceId3 int,
	@drugItemPriceIdToUse int, -- affected by @NewUsesThisId

	@existingStartDate1 datetime,
	@existingEndDate1 datetime,
	@existingStartDate2 datetime,
	@existingEndDate2 datetime,
	@existingStartDate3 datetime,
	@existingEndDate3 datetime,
	
	@currentStartDateNumber int,
	@currentEndDateNumber int,
	@nextStartDateNumber int,
	@nextEndDateNumber int,
	@newStartDate datetime,
	@newEndDate datetime,
	@msg nvarchar(800),
	@restrictedChange int,
	@deletionTestIterations int,
	@idToDelete int,
	@priceTypesSelected int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@retVal int,
	@currentUserLogin nvarchar(120)
	
	
	if @IsInsert = 1 and @UseSpreadsheetVersion  = 1
	BEGIN
		select @errorMsg = '@IsInsert = 1 and @UseSpreadsheetVersion  = 1 is not valid for 2010 since spreadsheet does not support insert'
		raiserror( @errorMsg, 16, 1 )
		return 

	END

	
	EXEC dbo.GetLoginNameFromUserId '3275CD05-385A-4131-A81F-E77DE3373076', 'AMMHINTESTAPP', 'NACSEC', @currentUserLogin OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during test scenario for user id 3275CD05-385A-4131-A81F-E77DE3373076'
		raiserror( @errorMsg, 16, 1 )
		return 
	END
	
	-- for removal testing, only specify one of the existing price types ( for now )
	if @IsRemoval = 1
	BEGIN
		select @priceTypesSelected = 0
		if @IsExistingFSS = 1
		BEGIN
			select @priceTypesSelected = @priceTypesSelected + 1
		END
		if @IsExistingBig4 = 1
		BEGIN
			select @priceTypesSelected = @priceTypesSelected + 1
		END
		if @IsExistingRestricted = 1
		BEGIN
			select @priceTypesSelected = @priceTypesSelected + 1
		END
		
		if @priceTypesSelected > 1
		BEGIN
			raiserror( 'For testing removal, please select only one price type = 1, the remaining types = 0', 16, 1 )
			return
		END
	
	END
	
	-- can only select one of the three for the NEW price
	select @priceTypesSelected = 0
	if @IsNewFSS = 1
	BEGIN
		select @priceTypesSelected = @priceTypesSelected + 1
	END
	if @IsNewBig4 = 1
	BEGIN
		select @priceTypesSelected = @priceTypesSelected + 1
	END
	if @IsNewRestricted = 1
	BEGIN
		select @priceTypesSelected = @priceTypesSelected + 1
	END
	
	if @priceTypesSelected > 1
	BEGIN
		raiserror( 'Please select only one NEW price type = 1, the remaining types = 0', 16, 1 )
		return
	END
	
	/* generate and run test scenario against AdjustDrugItemPriceHistorySequence */
	/* as if it is being done as a spread sheet upload */
	/* 4 is same as 3 but existing 3 dates as well as new dates temp/permanent are parameterized separately */
	
	if exists ( select NACCMContractNumber from DI_Contracts where NACCMContractNumber = @SimulatedContractNumber )
	BEGIN
		delete DI_Contracts where NACCMContractNumber = @SimulatedContractNumber 
	END
	
	insert into DI_Contracts
	( ContractNumber, NACCMContractNumber, NACCMContractId, ParentFSSContractId,
	ModificationStatusID, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	values
	( @SimulatedContractNumber, @SimulatedContractNumber, NULL, NULL,
	-1, 'TestScenario4', getdate(), 'TestScenario4', getdate() )
	
	select @contractId = @@IDENTITY
	
	print 'Created new contract with id= ' + convert( nvarchar(20), @contractId )
	
	exec CreateDrugItemPriceSpreadsheetModification  '{54134FF3-B915-488c-AE12-EFA8EB3AEA19}', @SimulatedContractNumber, @ModificationNumber, 'TestScenario4',
	'no spreadsheet', @modificationStatusId OUTPUT

	print 'Created new modification with id= ' + convert( nvarchar(20), @modificationStatusId )
	
	insert into DI_DrugItemNDC
	( FdaAssignedLabelerCode ,
		ProductCode ,
		PackageCode ,
		ModificationStatusId,
		CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	values
	(
		'12233', '3221', '31', @modificationStatusId, 'TestScenario4', getdate(), 'TestScenario4', getdate() 
	)
	
	select @drugItemNDCId = @@identity
	
	insert into DI_DrugItems
	(	
		ContractId ,
		DrugItemNDCId,
		PackageDescription  ,       
		Generic     ,
		TradeName       ,              
			DiscontinuationDate,
			DiscontinuationEnteredDate,
			DateEnteredMarket,                      	                  
		Covered            ,  
			PrimeVendor,                        
			PrimeVendorChangedDate ,                         	             
			PassThrough             ,            
			DispensingUnit          ,       
			VAClass                 ,                        	          
			DualPriceDesignation,
		ExcludeFromExport,
			ParentDrugItemId,
		LastModificationType,
		ModificationStatusId    ,                        	                  
		CreatedBy                , 
		CreationDate             ,                       	         
		LastModifiedBy           ,     
		LastModificationDate      
	)
	values
	(
		@contractId,
		@drugItemNDCId, 'Test', @SimulatedDrugItemName, @SimulatedDrugItemName, null, null,
		null, 'T', null, null, null, null, null, null, 0, null, 'T', @modificationStatusId, 
		'TestScenario4', getdate(), 'TestScenario4', getdate()
	)
	
	select @drugItemId = @@IDENTITY	
	
   	print 'Created new covered drug item with id= ' + convert( nvarchar(20), @drugItemId )
    
	if @IsRemoval = 1
	BEGIN
		exec GetDatesFromScenarioNumbers 
			@scenarioNumber = 42,  -- special dates for deletion test
			@scenarioStartDateNumber = 0,
			@scenarioEndDateNumber = 1,
			@scenarioStartDate = @newStartDate OUTPUT,
			@scenarioEndDate = @newEndDate OUTPUT,
			
			@existingStartDate1 = @existingStartDate1 OUTPUT, 
			@existingEndDate1 = @existingEndDate1 OUTPUT,
			@existingStartDate2 = @existingStartDate2 OUTPUT,
			@existingEndDate2 = @existingEndDate2 OUTPUT,
			@existingStartDate3 = @existingStartDate3 OUTPUT,
			@existingEndDate3 = @existingEndDate3 OUTPUT
				
		select @deletionTestIterations = 3
				
		while @deletionTestIterations > 0
		BEGIN
			print 'creating 3 original item prices ( for deletion )'
		    	         
			exec InsertRawPriceForTest @drugItemId, @existingStartDate1, @existingEndDate1, 101,
						@IsExistingTemporary, @IsExistingFSS, @IsExistingBig4, @IsExistingRestricted, 'T', @modificationStatusId, 'TestScenario42', @drugItemPriceId = @drugItemPriceId1 OUTPUT
						

			exec InsertRawPriceForTest @drugItemId, @existingStartDate2, @existingEndDate2, 102,
						@IsExistingTemporary, @IsExistingFSS, @IsExistingBig4, @IsExistingRestricted, 'T', @modificationStatusId, 'TestScenario42', @drugItemPriceId = @drugItemPriceId2 OUTPUT


			exec InsertRawPriceForTest @drugItemId, @existingStartDate3, @existingEndDate3, 103,
						@IsExistingTemporary, @IsExistingFSS, @IsExistingBig4, @IsExistingRestricted, 'T', @modificationStatusId, 'TestScenario42', @drugItemPriceId = @drugItemPriceId3 OUTPUT


			print 'Selecting snapshot at start of test scenario'
			
			select * from DI_DrugItemPrice where DrugItemId = @drugItemId order by PriceId
			
			select * from DI_DrugItemPriceHistory  where DrugItemId = @drugItemId order by PriceId
			
			print 'Testing delete for iteration ' + convert( nvarchar(10), @deletionTestIterations )
			
			if @deletionTestIterations = 3
			BEGIN
				select @idToDelete = @drugItemPriceId3
			END
			else if @deletionTestIterations = 2
			BEGIN
				select @idToDelete = @drugItemPriceId2		
			END
			else
			BEGIN
				select @idToDelete = @drugItemPriceId1
			END

			exec DeleteFSSPriceForItemPriceId '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @modificationStatusId, @idToDelete

			/* exec DeleteDrugItemPrice @idToDelete, @SimulatedContractNumber, @modificationStatusId, 'TestScenario42' */

			print 'Selecting snapshot at end of test scenario'
			
			select * from DI_DrugItemPrice where DrugItemId = @drugItemId order by PriceId
			
			select * from DI_DrugItemPriceHistory  where DrugItemId = @drugItemId order by PriceId
			
			select @deletionTestIterations = @deletionTestIterations - 1
			
			print 'Deleting test price history'
			
			delete DI_DrugItemPriceHistory
			where DrugItemId = @drugItemId

			print 'Deleting test prices'
			
			delete DI_DrugItemPrice
			where DrugItemId = @drugItemId
		END
	END
    else
    BEGIN -- not a removal
    	         
		print 'Entering test loop'
		
		select @currentStartDateNumber = 0
		select @currentEndDateNumber = 1
		
		select @msg = 'First date scenario: start date number = ' + convert( nvarchar(10), @currentStartDateNumber ) + ' end date number = ' + convert( nvarchar(10), @currentEndDateNumber )
		print @msg

		while( @currentStartDateNumber <> -1 )
		BEGIN
			/* date reversal ( error ) scenario */
			if @currentStartDateNumber = 2 AND @currentEndDateNumber = 11
			BEGIN
				exec GetDatesFromScenarioNumbers 
					@scenarioNumber = 4,
					@scenarioStartDateNumber = @currentEndDateNumber,
					@scenarioEndDateNumber = @currentStartDateNumber,
					@scenarioStartDate = @newStartDate OUTPUT,
					@scenarioEndDate = @newEndDate OUTPUT,
					
					@existingStartDate1 = @existingStartDate1 OUTPUT,
					@existingEndDate1 = @existingEndDate1 OUTPUT,
					@existingStartDate2 = @existingStartDate2 OUTPUT,
					@existingEndDate2 = @existingEndDate2 OUTPUT,
					@existingStartDate3 = @existingStartDate3 OUTPUT,
					@existingEndDate3 = @existingEndDate3 OUTPUT			
			END
			else
			BEGIN
				exec GetDatesFromScenarioNumbers 
					@scenarioNumber = 4,
					@scenarioStartDateNumber = @currentStartDateNumber,
					@scenarioEndDateNumber = @currentEndDateNumber,
					@scenarioStartDate = @newStartDate OUTPUT,
					@scenarioEndDate = @newEndDate OUTPUT,
					
					@existingStartDate1 = @existingStartDate1 OUTPUT,
					@existingEndDate1 = @existingEndDate1 OUTPUT,
					@existingStartDate2 = @existingStartDate2 OUTPUT,
					@existingEndDate2 = @existingEndDate2 OUTPUT,
					@existingStartDate3 = @existingStartDate3 OUTPUT,
					@existingEndDate3 = @existingEndDate3 OUTPUT
			
			END

		
			print 'creating 3 to 9 original item prices for selected price types'
		    	         
		    if @IsExistingFSS = 1
		    BEGIN
				exec InsertRawPriceForTest @drugItemId, @existingStartDate1, @existingEndDate1, 101,
							@IsExistingTemporary, 1, 0, 0, 'T', @modificationStatusId, 'TestScenario4', @drugItemPriceId = @drugItemPriceId1 OUTPUT
							

				exec InsertRawPriceForTest @drugItemId, @existingStartDate2, @existingEndDate2, 102,
							@IsExistingTemporary, 1, 0, 0, 'T', @modificationStatusId, 'TestScenario4', @drugItemPriceId = @drugItemPriceId2 OUTPUT


				exec InsertRawPriceForTest @drugItemId, @existingStartDate3, @existingEndDate3, 103,
							@IsExistingTemporary, 1, 0, 0, 'T', @modificationStatusId, 'TestScenario4', @drugItemPriceId = @drugItemPriceId3 OUTPUT
			END
		    
		    if @IsExistingBig4 = 1
		    BEGIN
				exec InsertRawPriceForTest @drugItemId, @existingStartDate1, @existingEndDate1, 11,
							@IsExistingTemporary, 0, 1, 0, 'T', @modificationStatusId, 'TestScenario4', @drugItemPriceId = @drugItemPriceId1 OUTPUT
							

				exec InsertRawPriceForTest @drugItemId, @existingStartDate2, @existingEndDate2, 12,
							@IsExistingTemporary, 0, 1, 0, 'T', @modificationStatusId, 'TestScenario4', @drugItemPriceId = @drugItemPriceId2 OUTPUT


				exec InsertRawPriceForTest @drugItemId, @existingStartDate3, @existingEndDate3, 13,
							@IsExistingTemporary, 0, 1, 0, 'T', @modificationStatusId, 'TestScenario4', @drugItemPriceId = @drugItemPriceId3 OUTPUT
			END
			
			if @IsExistingRestricted = 1
		    BEGIN
				exec InsertRawPriceForTest @drugItemId, @existingStartDate1, @existingEndDate1, 7,
							@IsExistingTemporary, 0, 0, 1, 'T', @modificationStatusId, 'TestScenario4', @drugItemPriceId = @drugItemPriceId1 OUTPUT
							

				exec InsertRawPriceForTest @drugItemId, @existingStartDate2, @existingEndDate2, 8,
							@IsExistingTemporary, 0, 0, 1, 'T', @modificationStatusId, 'TestScenario4', @drugItemPriceId = @drugItemPriceId2 OUTPUT


				exec InsertRawPriceForTest @drugItemId, @existingStartDate3, @existingEndDate3, 9,
							@IsExistingTemporary, 0, 0, 1, 'T', @modificationStatusId, 'TestScenario4', @drugItemPriceId = @drugItemPriceId3 OUTPUT
			END


			print 'Selecting snapshot at start of test scenario'
			
			select 'starting' as 'scenario', * from DI_DrugItemPrice where DrugItemId = @drugItemId order by PriceId
			
			select 'starting' as 'scenario', * from DI_DrugItemPriceHistory  where DrugItemId = @drugItemId order by PriceId

			if @currentStartDateNumber = 2 AND @currentEndDateNumber = 8 AND @IsNewRestricted = 1
			BEGIN
				select @restrictedChange = 1			
			END
			else
			BEGIN
				select @restrictedChange = 0
			END

			if @NewUsesThisId = 1
			BEGIN
				select @drugItemPriceIdToUse = @drugItemPriceId1
			END
			else if @NewUsesThisId = 2
			BEGIN
				select @drugItemPriceIdToUse = @drugItemPriceId2		
			END
			else
			BEGIN
				select @drugItemPriceIdToUse = @drugItemPriceId3		
			END

			-- insert, from the user's perspective, can only be done through the GUI, so this is a GUI only test
			if @IsInsert = 1
			BEGIN
				print 'Testing new price insert with IsNewTemporary=' + convert(nvarchar(2), @IsNewTemporary )

				exec InsertFSSDrugItemPrice '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @drugItemId,
				@newStartDate, @newEndDate, 8484, @IsNewTemporary, @IsNewFSS, @IsNewBig4,
					@IsNewRestricted, -- VA
					0,
					0,
					0,
					0,
					@IsNewRestricted,  -- IHS
					@IsNewRestricted,  -- IHS2
					0,
					0,
					@restrictedChange,
					0,
					0,
					0,
					0,
					0,
					@modificationStatusId,
					null,
					@NewDrugItemPriceId OUTPUT

			END
			else
			BEGIN

				-- update can be done via the spreadsheet or the GUI, as determined by the flag
				--@UseSpreadsheetVersion bit, -- 1 = UpdateFSSDrugItemPriceFromSpreadsheet; 0 = UpdateFSSDrugItemPrice

				print 'Testing price update with IsNewTemporary=' + convert(nvarchar(2), @IsNewTemporary ) + ' and @UseSpreadsheetVersion =' + convert( nvarchar(2), @UseSpreadsheetVersion )

				if @UseSpreadsheetVersion = 1
				BEGIN
					if @IsNewTemporary = 1
					BEGIN
						Exec  @retVal = UpdateFSSDrugItemPriceFromSpreadsheet @currentUserLogin,@SimulatedContractNumber,@drugItemPriceIdToUse, null,
										null,null,@newStartDate, @newEndDate,
										4848,
										@IsNewFSS, @IsNewBig4,@IsNewRestricted,0,0,0,0,@IsNewRestricted,
										@IsNewRestricted,0,0,@restrictedChange,0,0,0,0,0,
										1,1.99,
										@modificationStatusId,'E',0.005,'TestScenario4'

					--	IF @retVal = -1 OR @error > 0
					--	BEGIN
					--		select @errorMsg = 'Error returned when Updating price  for @DrugItemPriceId=' + convert( nvarchar(20), @drugItemPriceIdToUse )
					--		raiserror( @errorMsg, 16, 1 )
					--		return 
					--	END					
					END
					else
					BEGIN
						Exec  @retVal = UpdateFSSDrugItemPriceFromSpreadsheet @currentUserLogin,@SimulatedContractNumber,@drugItemPriceIdToUse,@newStartDate, @newEndDate,
										4848,null,null,
										null,
										@IsNewFSS, @IsNewBig4,@IsNewRestricted,0,0,0,0,@IsNewRestricted,
										@IsNewRestricted,0,0,@restrictedChange,0,0,0,0,0,
										1,1.99,
										@modificationStatusId,'E',0.005,'TestScenario4'

					--	IF @retVal = -1 OR @error > 0
					--	BEGIN
					--		select @errorMsg = 'Error returned when Updating price  for @DrugItemPriceId=' + convert( nvarchar(20), @drugItemPriceIdToUse )
					--		raiserror( @errorMsg, 16, 1 )
					--		return 
					--	END
					END
				END
				else
				BEGIN

					exec UpdateFSSDrugItemPrice '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @drugItemPriceIdToUse,
					@newStartDate, @newEndDate, 8484, @IsNewTemporary, @IsNewFSS, @IsNewBig4,
						@IsNewRestricted, -- VA
						0,
						0,
						0,
						0,
						@IsNewRestricted,  -- IHS
						@IsNewRestricted,  -- IHS2
						0,
						0,
						@restrictedChange,
						0,
						0,
						0,
						0,
						0,
						@modificationStatusId,
						null

				END
			END
		
			
			print 'Selecting snapshot at end of test scenario'
			
			print 'IsFSS=1'
			select  'IsFSS=1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPrice 
			where DrugItemId = @drugItemId and
			IsFSS = 1 
			order by PriceId
			
			print 'IsBig4=1'
			select 'IsBig4=1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPrice 
			where DrugItemId = @drugItemId and
			IsBig4 = 1 
			order by PriceId
			
			print 'IsRestricted = 1'
			select 'IsRestricted = 1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPrice 
			where DrugItemId = @drugItemId and
			IsFSS = 0 and
			IsBig4 = 0
			order by PriceId

			print 'History.IsFSS=1'
			select 'History.IsFSS=1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPriceHistory 
			where DrugItemId = @drugItemId and
			IsFSS = 1 
			order by PriceId
			
			print 'History.IsBig4=1'
			select 'History.IsBig4=1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPriceHistory 
			where DrugItemId = @drugItemId and
			IsBig4 = 1 
			order by PriceId
			
			print 'History.IsRestricted = 1'
			select 'History.IsRestricted = 1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPriceHistory 
			where DrugItemId = @drugItemId and
			IsFSS = 0 and
			IsBig4 = 0
			order by PriceId
				
			if @AddSecondRecord = 1
			BEGIN

				print 'Testing 2nd temporary new price insert'
				
				exec InsertFSSDrugItemPrice '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @drugItemId,
				@newStartDate, @newEndDate, 8484, @IsNewTemporary, @IsNewFSS, @IsNewBig4,
					@IsNewRestricted, -- VA
					0,
					0,
					0,
					0,
					@IsNewRestricted,  -- IHS
					@IsNewRestricted,  -- IHS2
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					@modificationStatusId,
					null,
					@NewDrugItemPriceId OUTPUT
			
				print 'Selecting snapshot AGAIN at end of 2nd insert'
				
				print 'IsFSS=1'
				select 'IsFSS=1 scenario' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPrice 
				where DrugItemId = @drugItemId and
				IsFSS = 1 
				order by PriceId
				
				print 'IsBig4=1'
				select 'IsBig4=1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPrice 
				where DrugItemId = @drugItemId and
				IsBig4 = 1 
				order by PriceId
				
				print 'IsRestricted = 1'
				select 'IsRestricted = 1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPrice 
				where DrugItemId = @drugItemId and
				IsFSS = 0 and
				IsBig4 = 0
				order by PriceId

				print 'History.IsFSS=1'
				select 'History.IsFSS=1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPriceHistory 
				where DrugItemId = @drugItemId and
				IsFSS = 1 
				order by PriceId
				
				print 'History.IsBig4=1'
				select 'History.IsBig4=1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPriceHistory 
				where DrugItemId = @drugItemId and
				IsBig4 = 1 
				order by PriceId
				
				print 'History.IsRestricted = 1'
				select 'History.IsRestricted = 1 scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPriceHistory 
				where DrugItemId = @drugItemId and
				IsFSS = 0 and
				IsBig4 = 0
				order by PriceId
				
			END
			
			
			print 'Deleting test price history'
			
			delete DI_DrugItemPriceHistory
			where DrugItemId = @drugItemId

			print 'Deleting test prices'
			
			delete DI_DrugItemPrice
			where DrugItemId = @drugItemId
			print 'Getting next date scenario'

			exec GetNextDateScenario 4, @currentStartDateNumber, @currentEndDateNumber, @nextStartDateNumber = @nextStartDateNumber OUTPUT, @nextEndDateNumber = @nextEndDateNumber OUTPUT

			select @currentStartDateNumber = @nextStartDateNumber,
				@currentEndDateNumber = @nextEndDateNumber
							
			select @msg = '================================Next date scenario: start date number = ' + convert( nvarchar(10), @currentStartDateNumber ) + ' end date number = ' + convert( nvarchar(10), @currentEndDateNumber )
			print @msg
		END
	END

	print 'Deleting test item'
	
	delete DI_DrugItems
	where DrugItemId = @drugItemId
	
	print 'Deleting test contract'
	
	delete DI_Contracts
	where ContractId = @contractId


END
GO
