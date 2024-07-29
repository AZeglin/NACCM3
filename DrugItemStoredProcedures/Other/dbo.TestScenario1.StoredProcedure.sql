IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TestScenario1]') AND type in (N'P', N'PC'))
DROP PROCEDURE [TestScenario1]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[TestScenario1]
(
@IsFSS bit,
@IsBig4 bit,
@IsRestricted bit,
@OriginalTemporary bit,
@IsTemporary bit,
@IsRemoval bit,
@IsInsert bit,
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
	@date0 datetime,
	@date1 datetime,
	@date2 datetime,
	@date3 datetime,
	@date4 datetime,
	@date5 datetime,
	@date6 datetime,
	@date7 datetime,
	@drugItemPriceId int,
	@currentStartDateNumber int,
	@currentEndDateNumber int,
	@nextStartDateNumber int,
	@nextEndDateNumber int,
	@newStartDate datetime,
	@newEndDate datetime,
	@msg nvarchar(800)
	
	/* dates of current price */
	select @date2 = '2010-10-10'
	select @date5 = '2010-10-20'
	
	/* dates for new price scenarios */
	select @date0 = '2010-10-2'
	select @date1 = '2010-10-4'
	select @date3 = '2010-10-12'
	select @date4 = '2010-10-14'
	select @date6 = '2010-10-22'
	select @date7 = '2010-10-24'
	

	/* generate and run test scenario against AdjustDrugItemPriceHistorySequence */
	/* as if it is being done as a spread sheet upload */
	
insert into DI_Contracts
	( ContractNumber, NACCMContractNumber, NACCMContractId, ParentFSSContractId,
	ModificationStatusID, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	values
	( @SimulatedContractNumber, @SimulatedContractNumber, NULL, NULL,
	-1, 'TestScenario1', getdate(), 'TestScenario1', getdate() )
		
	select @contractId = @@IDENTITY
	
	print 'Created new contract with id= ' + convert( nvarchar(20), @contractId )
	
	exec CreateDrugItemPriceSpreadsheetModification  '{54134FF3-B915-488c-AE12-EFA8EB3AEA19}', @SimulatedContractNumber, @ModificationNumber, 'TestScenario1',
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
		'11111', '1111', '11', @modificationStatusId, 'TestScenario4', getdate(), 'TestScenario4', getdate() 
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
		'TestScenario1', getdate(), 'TestScenario1', getdate()
	)
	
	select @drugItemId = @@IDENTITY	
	
   	print 'Created new covered drug item with id= ' + convert( nvarchar(20), @drugItemId )
    
    /* for this scenario, removal is trivial */	         
	if @IsRemoval = 1
	BEGIN
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
			@date2,
			@date5,
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
			'TestScenario1',
			getdate(),
			'TestScenario1',
			getdate()		
		)

		select @drugItemPriceId = @@IDENTITY	

		print 'Selecting snapshot at start of test scenario'
		
		select * from DI_DrugItemPrice where DrugItemId = @drugItemId order by PriceId
		
		select * from DI_DrugItemPriceHistory  where DrugItemId = @drugItemId order by PriceId
		
		print 'Testing delete'
		
		exec DeleteFSSPriceForItemPriceId '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @modificationStatusId, @drugItemPriceId

	/*	exec DeleteDrugItemPrice @drugItemPriceId, @SimulatedContractNumber, @modificationStatusId, 'TestScenario1' */

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
		
			print 'creating original item price'	
		    	         
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
				@date2,
				@date5,
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
			'TestScenario1',
			getdate(),
			'TestScenario1',
			getdate()		
			)

			select @drugItemPriceId = @@IDENTITY	

			print 'Selecting snapshot at start of test scenario'
			
			select 'starting' as 'scenario', * from DI_DrugItemPrice where DrugItemId = @drugItemId order by PriceId
			
			select 'starting' as 'scenario', * from DI_DrugItemPriceHistory  where DrugItemId = @drugItemId order by PriceId

			print 'Setting up new price date range'
			/* start date */
			if @currentStartDateNumber = 0
			BEGIN
				select @newStartDate = @date0
			END
			else if @currentStartDateNumber = 1
			BEGIN
				select @newStartDate = @date1
			END
			else if @currentStartDateNumber = 2
			BEGIN
				select @newStartDate = @date2
			END
			else if @currentStartDateNumber = 3
			BEGIN
				select @newStartDate = @date3
			END
			else if @currentStartDateNumber = 4
			BEGIN
				select @newStartDate = @date4
			END
			else if @currentStartDateNumber = 5
			BEGIN
				select @newStartDate = @date5
			END
			else if @currentStartDateNumber = 6
			BEGIN
				select @newStartDate = @date6
			END
			/* end date */
			if @currentEndDateNumber = 0
			BEGIN
				select @newEndDate = @date0
			END
			else if @currentEndDateNumber = 1
			BEGIN
				select @newEndDate = @date1
			END
			else if @currentEndDateNumber = 2
			BEGIN
				select @newEndDate = @date2
			END
			else if @currentEndDateNumber = 3
			BEGIN
				select @newEndDate = @date3
			END
			else if @currentEndDateNumber = 4
			BEGIN
				select @newEndDate = @date4
			END
			else if @currentEndDateNumber = 5
			BEGIN
				select @newEndDate = @date5
			END
			else if @currentEndDateNumber = 6
			BEGIN
				select @newEndDate = @date6
			END
			else if @currentEndDateNumber = 7
			BEGIN
				select @newEndDate = @date7
			END

			if @IsInsert = 1
			BEGIN
				print 'Testing new price insert with IsTemporary=' + convert(nvarchar(2), @IsTemporary )

				exec InsertFSSDrugItemPrice '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @drugItemId,
				@newStartDate, @newEndDate, 9999, @IsTemporary, @IsFSS, @IsBig4,
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
			
				print 'Testing price update with IsTemporary=' + convert(nvarchar(2), @IsTemporary )

				exec UpdateFSSDrugItemPrice '3275CD05-385A-4131-A81F-E77DE3373076', @SimulatedContractNumber, @drugItemPriceId,
				@newStartDate, @newEndDate, 10000, @IsTemporary, @IsFSS, @IsBig4,
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

			exec GetNextDateScenario 1, @currentStartDateNumber, @currentEndDateNumber, @nextStartDateNumber = @nextStartDateNumber OUTPUT, @nextEndDateNumber = @nextEndDateNumber OUTPUT

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
