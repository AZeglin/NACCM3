IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ForwardTraverseChangeTableForNDCReport')
	BEGIN
		DROP  Procedure  ForwardTraverseChangeTableForNDCReport
	END

GO

CREATE Procedure ForwardTraverseChangeTableForNDCReport
(
@StartingDrugItemNDCId int
)

AS

	Declare @NextDrugItemNDCId int,
		@CurrentDrugItemNDCId int,
		@CurrentInnerContractNDCNumberChangeId int,
		@rowcount int,
		@error int,
		@errorMsg nvarchar(240)
	
BEGIN TRANSACTION

	if exists( select NewDrugItemNDCId
			from DI_ContractNDCNumberChange
			where OldDrugItemNDCId = @StartingDrugItemNDCId ) 
	BEGIN
		
		insert into #NDCInnerTraversalList
		( 
		  ContractNDCNumberChangeId,
		  OldContractId,
		  OldDrugItemNDCId,
		  NewContractId,
		  NewDrugItemNDCId,
		  ChangeStatus,
		  ModificationId,
		  EffectiveDate,
		  EndDate,
		  LastModifiedBy,
		  LastModificationDate,
		  Processed
		)				
		select a.ContractNDCNumberChangeId, 
				a.OldContractId, 
				a.OldDrugItemNDCId, 
				a.NewContractId, 
				a.NewDrugItemNDCId, 
				a.ChangeStatus,
				a.ModificationId,
				a.EffectiveDate,
				a.EndDate,
				a.LastModifiedBy,
				a.LastModificationDate,
				0
		from DI_ContractNDCNumberChange a
		where a.OldDrugItemNDCId = @StartingDrugItemNDCId
		and not exists( select ContractNDCNumberChangeId
						from #NDCInnerTraversalList b
						where a.OldContractId = b.OldContractId
							and a.OldDrugItemNDCId = b.OldDrugItemNDCId
							and a.NewContractId = b.NewContractId
							and a.NewDrugItemNDCId = b.NewDrugItemNDCId )
	
		select @error = @@error, @rowcount = @@rowcount
		
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting NDC into inner temp table (1) during forward traversal.'
			goto ERROREXIT
		END
	
	/*	print 'rowcount at initial insert = ' + convert( nvarchar(20), @rowcount ) */
	
		while( exists( select ContractNDCNumberChangeId
						from #NDCInnerTraversalList
						where Processed = 0 ))						
		BEGIN			
			select	@CurrentDrugItemNDCId = OldDrugItemNDCId,
					@NextDrugItemNDCId = NewDrugItemNDCId,
					@CurrentInnerContractNDCNumberChangeId = ContractNDCNumberChangeId
						from #NDCInnerTraversalList
						where Processed = 0
						and ContractNDCNumberChangeId = ( select min( ContractNDCNumberChangeId )
															from #NDCInnerTraversalList
															where Processed = 0 )
	
			select @error = @@error, @rowcount = @@rowcount
		
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting next NDC from inner temp table during forward traversal.'
				goto ERROREXIT
			END
	
	
			insert into #NDCTraversalList
			( 
			  ContractNDCNumberChangeId,
			  ContractId,
			  DrugItemNDCId,
			  SourceColumn,
			  ChangeStatus,
			  ModificationId,
			  EffectiveDate,
			  EndDate,
			  LastModifiedBy,
			  LastModificationDate,
			  TraversalDirection
			 )
	         select ContractNDCNumberChangeId,
				  OldContractId,
				  OldDrugItemNDCId,
				  'O',      /* old */
				  ChangeStatus,
				  ModificationId,
				  EffectiveDate,
				  EndDate,
				  LastModifiedBy,
				  LastModificationDate,
				  'F' as 'TraversalDirection'
			 FROM #NDCInnerTraversalList c
			 where c.ContractNDCNumberChangeId = @CurrentInnerContractNDCNumberChangeId
			 and not exists ( select t.ContractNDCNumberChangeId
								from #NDCTraversalList t
								where t.ContractNDCNumberChangeId = c.ContractNDCNumberChangeId
								and t.DrugItemNDCId = c.OldDrugItemNDCId
								and t.ContractId = c.OldContractId )				

			select @error = @@error, @rowcount = @@rowcount
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting NDC into outer temp table (1) during forward traversal.'
				goto ERROREXIT
			END
			
	/*		print 'rowcount inserted into outer temp table = ' + convert( nvarchar(20), @rowcount )

			select * from #NDCInnerTraversalList
			select * from #NDCTraversalList    */
			
			update #NDCInnerTraversalList
			set Processed = 1
			where ContractNDCNumberChangeId = @CurrentInnerContractNDCNumberChangeId
			
			select @error = @@error, @rowcount = @@rowcount
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error updating processed status of inner temp table during forward traversal.'
				goto ERROREXIT
			END
			
			insert into #NDCInnerTraversalList
			( 
			  ContractNDCNumberChangeId,
			  OldContractId,
			  OldDrugItemNDCId,
			  NewContractId,
			  NewDrugItemNDCId,
			  ChangeStatus,
			  ModificationId,
			  EffectiveDate,
			  EndDate,
			  LastModifiedBy,
			  LastModificationDate,
			  Processed
			)				
			select a.ContractNDCNumberChangeId, 
					a.OldContractId, 
					a.OldDrugItemNDCId, 
					a.NewContractId, 
					a.NewDrugItemNDCId, 
					a.ChangeStatus,
					a.ModificationId,
					a.EffectiveDate,
					a.EndDate,
					a.LastModifiedBy,
					a.LastModificationDate,
					0
			from DI_ContractNDCNumberChange a
			where a.OldDrugItemNDCId = @NextDrugItemNDCId
			and not exists( select ContractNDCNumberChangeId
							from #NDCInnerTraversalList b
							where a.OldContractId = b.OldContractId
								and a.OldDrugItemNDCId = b.OldDrugItemNDCId
								and a.NewContractId = b.NewContractId
								and a.NewDrugItemNDCId = b.NewDrugItemNDCId )
	
			select @error = @@error, @rowcount = @@rowcount
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error inserting NDC into inner temp table (2) during forward traversal.'
				goto ERROREXIT
			END
			
		/*	print 'rowcount at traveral point = ' + convert( nvarchar(20), @rowcount )  */

			/* end of the traversal */
			if @rowcount = 0
			BEGIN
			
				/* if final node contains useful information, save it */
				if @CurrentDrugItemNDCId <> @NextDrugItemNDCId
				BEGIN
				
					/* make an entry for the old Id */
					insert into #NDCTraversalList
					( 
						ContractNDCNumberChangeId,
						ContractId,
						DrugItemNDCId,
						SourceColumn,
						ChangeStatus,
						ModificationId,
						EffectiveDate,
						EndDate,
						LastModifiedBy,
						LastModificationDate,
						TraversalDirection
					)
      
					select ContractNDCNumberChangeId,
						NewContractId,
						NewDrugItemNDCId,
						'N',      /* new */
						ChangeStatus,
						ModificationId,
						EffectiveDate,
						EndDate,
						LastModifiedBy,
						LastModificationDate,
						'F' as 'TraversalDirection'
					 FROM #NDCInnerTraversalList c
					 where c.ContractNDCNumberChangeId = @CurrentInnerContractNDCNumberChangeId
					 and not exists ( select t.ContractNDCNumberChangeId
										from #NDCTraversalList t
										where t.ContractNDCNumberChangeId = c.ContractNDCNumberChangeId
										and t.DrugItemNDCId = c.NewDrugItemNDCId
										and t.ContractId = c.NewContractId )				

		
					select @error = @@error, @rowcount = @@rowcount
		
					if @error <> 0
					BEGIN
						select @errorMsg = 'Error inserting NDC into outer temp table (2) during forward traversal.'
						goto ERROREXIT
					END
					
			/*		print 'rowcount inserted into outer temp table at end of traversal = ' + convert( nvarchar(20), @rowcount )  */

				END
			END
		END /* while */
	END /* if exists */
	
	delete #NDCInnerTraversalList
	
	goto OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 )
	
  	if @@TRANCOUNT > 1
  	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 




