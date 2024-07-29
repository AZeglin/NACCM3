IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TestScenario2]') AND type in (N'P', N'PC'))
DROP PROCEDURE [TestScenario2]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[TestScenario2]
(
@IsFSS bit,
@IsBig4 bit,
@IsRestricted bit,
@OriginalTemporary bit,
@IsTemporary bit,
@IsRemoval bit,
@IsInsert bit,
@UseSpreadsheetVersion bit, -- 1 = UpdateFSSDrugItemPriceFromSpreadsheet; 0 = UpdateFSSDrugItemPrice
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
	@NewDrugItemPriceId int,
	@drugItemNDCId int,
	@drugItemPriceId1 int,
	@drugItemPriceId2 int,

	@existingStartDate1 datetime,
	@existingEndDate1 datetime,
	@existingStartDate2 datetime,
	@existingEndDate2 datetime,
	
	@currentStartDateNumber int,
	@currentEndDateNumber int,
	@nextStartDateNumber int,
	@nextEndDateNumber int,
	@newStartDate datetime,
	@newEndDate datetime,
	@msg nvarchar(800),
	
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@retVal int,
	@currentUserLogin nvarchar(120)

	
	/* generate and run test scenario against AdjustDrugItemPriceHistorySequence */
	/* as if it is being done as a spread sheet upload */
	
	if @IsInsert = 1 and @UseSpreadsheetVersion  = 1
	BEGIN
		select @errorMsg = '@IsInsert = 1 and @UseSpreadsheetVersion  = 1 is not valid for 2010 since spreadsheet does not support insert'
		raiserror( @errorMsg, 16, 1 )
		return 

	END
	
--	exec dbo.GetLoginNameFromUserIdLocalProc '3275CD05-385A-4131-A81F-E77DE3373076', @currentUserLogin OUTPUT
	EXEC dbo.GetLoginNameFromUserId '3275CD05-385A-4131-A81F-E77DE3373076', 'AMMHINTESTAPP', 'NACSEC', @currentUserLogin OUTPUT 

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error getting current user login during test scenario for user id 3275CD05-385A-4131-A81F-E77DE3373076'
		raiserror( @errorMsg, 16, 1 )
		return 
	END
	
	insert into DI_Contracts
	( ContractNumber, NACCMContractNumber, NACCMContractId, ParentFSSContractId,
	ModificationStatusID, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	values
	( @SimulatedContractNumber, @SimulatedContractNumber, NULL, NULL,
	-1, 'TestScenario2', getdate(), 'TestScenario2', getdate() )
		
	select @contractId = @@IDENTITY
	
	print 'Created new contract with id= ' + convert( nvarchar(20), @contractId )
	
	exec CreateDrugItemPriceSpreadsheetModification  '{54134FF3-B915-488c-AE12-EFA8EB3AEA19}', @SimulatedContractNumber, @ModificationNumber, 'TestScenario2',
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
		'22222', '2222', '22', @modificationStatusId, 'TestScenario2', getdate(), 'TestScenario2', getdate() 
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
		'TestScenario2', getdate(), 'TestScenario2', getdate()
	)
	
	select @drugItemId = @@IDENTITY	
	
   	print 'Created new covered drug item with id= ' + convert( nvarchar(20), @drugItemId )
    
    /* for this scenario, removal is trivial */	         
	if @IsRemoval = 1
	BEGIN
		exec GetDatesFromScenarioNumbers 
			@scenarioNumber = 2,
			@scenarioStartDateNumber = 0,
			@scenarioEndDateNumber = 1,
			@scenarioStartDate = @newStartDate OUTPUT,
			@scenarioEndDate = @newEndDate OUTPUT,
			
			@existingStartDate1 = @existingStartDate1 OUTPUT, -- only interested in this for removal test
			@existingEndDate1 = @existingEndDate1 OUTPUT,
			@existingStartDate2 = @existingStartDate2 OUTPUT,
			@existingEndDate2 = @existingEndDate2 OUTPUT
					
		print 'creating original item price ( for deletion )'	
	    	         
		insert into DI_DrugItemPrice
		(
			DrugItemId ,                                     	              
			PriceId    ,                                     	                  
			PriceStartDate ,                                 	             
			PriceStopDate   ,                                	             
			Price        ,                      
			IsTemporary  ,                                   	                  
			IsFSS        ,                                   	                  
			IsBIG4       ,                                   	                  
			IsVA         ,                                   	                  
			IsBOP        ,                                   	                  
			IsCMOP       ,                                   	                  
			IsDOD        ,                                   	                  
			IsHHS        ,                                   	                  
			IsIHS        ,                                   	                  
			IsIHS2       ,                                   	                  
			IsDIHS        ,                                   	                  
			IsNIH        ,                                   	                  
			IsPHS        ,                                   	                  
			IsSVH        ,                                   	                  
			IsSVH1       ,                                   	                  
			IsSVH2       ,                                   	                  
			IsTMOP       ,                                   	                  
			IsUSCG       ,                                   	                  
			AwardedFSSTrackingCustomerRatio    , 
			CurrentTrackingCustomerPrice       ,             
			ExcludeFromExport,
			LastModificationType,		             
			ModificationStatusId               ,             	                  
			CreatedBy                          ,             	
			CreationDate                       ,             	         
			LastModifiedBy                     ,             
			LastModificationDate            
		)             	         
		values
		(
			@drugItemId,
			1,
			@existingStartDate1,
			@existingEndDate1,
			100,
			@OriginalTemporary,
			@IsFSS,
			@IsBig4,
			@IsRestricted, /* VA */
			0,
			0,
			0,
			0,
			@IsRestricted, /* IHS */
			@IsRestricted, /* IHS2 */
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			null,
			null,
			0,
			'T',
			@ModificationStatusId,
			'TestScenario2',
			getdate(),
			'TestScenario2',
			getdate()		
		)

		select @drugItemPriceId1 = @@IDENTITY	

		print 'Selecting snapshot at start of test scenario'
		
		select * from DI_DrugItemPrice where DrugItemId = @drugItemId order by PriceId
		
		select * from DI_DrugItemPriceHistory  where DrugItemId = @drugItemId order by PriceId
		
		print 'Testing delete'
		
		exec DeleteFSSPriceForItemPriceId '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @modificationStatusId, @drugItemPriceId1

		/*	exec DeleteDrugItemPrice @drugItemPriceId1, @SimulatedContractNumber, @modificationStatusId, 'TestScenario2' */

		print 'Selecting snapshot at end of test scenario'
		
		select * from DI_DrugItemPrice where DrugItemId = @drugItemId order by PriceId
		
		select * from DI_DrugItemPriceHistory  where DrugItemId = @drugItemId order by PriceId

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
			exec GetDatesFromScenarioNumbers 
					@scenarioNumber = 2,
					@scenarioStartDateNumber = @currentStartDateNumber,
					@scenarioEndDateNumber = @currentEndDateNumber,
					@scenarioStartDate = @newStartDate OUTPUT,
					@scenarioEndDate = @newEndDate OUTPUT,
					
					@existingStartDate1 = @existingStartDate1 OUTPUT,
					@existingEndDate1 = @existingEndDate1 OUTPUT,
					@existingStartDate2 = @existingStartDate2 OUTPUT,
					@existingEndDate2 = @existingEndDate2 OUTPUT
		
			print 'creating 2 original item prices'	
		    	         
		    exec  InsertRawPriceForTest @drugItemId, @existingStartDate1, @existingEndDate1, 101,
						@IsTemporary, @IsFSS, @IsBig4, @IsRestricted, 'T', @modificationStatusId, 'TestScenario2', @drugItemPriceId = @drugItemPriceId1 OUTPUT

		    exec  InsertRawPriceForTest @drugItemId, @existingStartDate2, @existingEndDate2, 102,
						@IsTemporary, @IsFSS, @IsBig4, @IsRestricted, 'T', @modificationStatusId, 'TestScenario2', @drugItemPriceId = @drugItemPriceId2 OUTPUT

			print 'Selecting snapshot at start of test scenario'
			
			select 'starting' as 'scenario', * from DI_DrugItemPrice where DrugItemId = @drugItemId order by PriceId
			
			select 'starting' as 'scenario', * from DI_DrugItemPriceHistory  where DrugItemId = @drugItemId order by PriceId

			print 'Setting up new price date range'

			if @IsInsert = 1
			BEGIN
				print 'Testing new price insert with IsTemporary=' + convert(nvarchar(2), @IsTemporary )

				exec InsertFSSDrugItemPrice '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @drugItemId,
				@newStartDate, @newEndDate, 8888, @IsTemporary, @IsFSS, @IsBig4,
					@IsRestricted, -- VA
					0,
					0,
					0,
					0,
					@IsRestricted,  -- IHS
					@IsRestricted,  -- IHS2
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

			END
			else
			BEGIN
				-- update can be done via the spreadsheet or the GUI, as determined by the flag
				--@UseSpreadsheetVersion bit, -- 1 = UpdateFSSDrugItemPriceFromSpreadsheet; 0 = UpdateFSSDrugItemPrice

				print 'Testing price update with IsNewTemporary=' + convert(nvarchar(2), @IsTemporary ) + ' and @UseSpreadsheetVersion =' + convert( nvarchar(2), @UseSpreadsheetVersion )

				if @UseSpreadsheetVersion = 1
				BEGIN
					if @IsTemporary = 1
					BEGIN
						Exec  @retVal = UpdateFSSDrugItemPriceFromSpreadsheet @currentUserLogin,@SimulatedContractNumber,@drugItemPriceId1, null,
										null,null,@newStartDate, @newEndDate,
										7777,
										@IsFSS, @IsBig4,@IsRestricted,0,0,0,0,@IsRestricted,
										@IsRestricted,0,0,0,0,0,0,0,0,
										1,1.99,
										@modificationStatusId,'E',0.005,'TestScenario2'

					END
					else
					BEGIN
						Exec  @retVal = UpdateFSSDrugItemPriceFromSpreadsheet @currentUserLogin,@SimulatedContractNumber,@drugItemPriceId1,@newStartDate, @newEndDate,
										7777,null,null,
										null,
										@IsFSS, @IsBig4,@IsRestricted,0,0,0,0,@IsRestricted,
										@IsRestricted,0,0,0,0,0,0,0,0,
										1,1.99,
										@modificationStatusId,'E',0.005,'TestScenario2'

					END
				END
				else
				BEGIN
				
					exec UpdateFSSDrugItemPrice '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @drugItemPriceId1,
					@newStartDate, @newEndDate, 7777, @IsTemporary, @IsFSS, @IsBig4,
						@IsRestricted, -- VA
						0,
						0,
						0,
						0,
						@IsRestricted,  -- IHS
						@IsRestricted,  -- IHS2
						0,
						0,
						0,
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
			
			select 'scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPrice where DrugItemId = @drugItemId order by PriceId
			
			select 'scenario ' + convert( nvarchar(10), @currentStartDateNumber ) + ',' + convert( nvarchar(10), @currentEndDateNumber ) as 'scenario', * from DI_DrugItemPriceHistory  where DrugItemId = @drugItemId order by PriceId
				
				
			print 'Deleting test price history'
			
			delete DI_DrugItemPriceHistory
			where DrugItemId = @drugItemId

			print 'Deleting test prices'
			
			delete DI_DrugItemPrice
			where DrugItemId = @drugItemId

			print 'Getting next date scenario'

			exec GetNextDateScenario 2, @currentStartDateNumber, @currentEndDateNumber, @nextStartDateNumber = @nextStartDateNumber OUTPUT, @nextEndDateNumber = @nextEndDateNumber OUTPUT

			select @currentStartDateNumber = @nextStartDateNumber,
				@currentEndDateNumber = @nextEndDateNumber
				
			select @msg = 'Next date scenario: start date number = ' + convert( nvarchar(10), @currentStartDateNumber ) + ' end date number = ' + convert( nvarchar(10), @currentEndDateNumber )
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
