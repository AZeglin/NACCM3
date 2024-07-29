IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'NightlyBatchProcessForFSSWithoutSubItem')
	BEGIN
		DROP  Procedure  NightlyBatchProcessForFSSWithoutSubItem
	END

GO

CREATE Proc [dbo].[NightlyBatchProcessForFSSWithoutSubItem]
As

Declare @AdjustPriceTableId int,
		@CurrentDrugItemPriceId int,
		@CurrentPricestartDate datetime,
		@CurrentPriceStopDate datetime,
		@error int,
		@errorMsg varchar(1000),
		@i int,
		@count int,
		@rowsAffected int,
		@drugitemId int,
		@PreviousDrugItemPriceId int, 
		@PreviousPricestartDate datetime,
		@PreviousPriceStopDate datetime,
		@PreviousPrice decimal(18,2),
		@count1 int,
		@count3 int,
		@DrugItemPriceHistoryId int
		
   
BEGIN TRANSACTION

Declare Price_History cursor For 
	Select Distinct DrugitemID
	From Di_DrugItempricehistory
	where Priceid = 0 
	And IsFSS = 1
	And (DrugItemSubItemId is null or len(DrugItemSubItemId) = 0 
		 or DrugItemSubItemId = -1)
	order by 1
	
Open Price_History
FETCH NEXT FROM Price_History
INTO @drugitemid	

WHILE @@FETCH_STATUS = 0
BEGIN

	Create table #AdjustPriceHistoryTable
	(
	AdjustPriceTableId int identity(1,1), 
	DrugItemPriceHistoryId int,
	DrugItemPriceId int,
	DrugItemId int,
	PriceId int ,
	PriceStartDate datetime,
	PriceStopDate datetime,
	Price decimal(18,2) ,
	Modification char(1)
	)

	Create table #AdjustPriceHistoryTable1
	(
	AdjustPriceTableId int identity(1,1), 
	DrugItemPriceHistoryId int,
	DrugItemPriceId int,
	DrugItemId int,
	PriceId int ,
	PriceStartDate datetime,
	PriceStopDate datetime,
	Price decimal(18,2) ,
	Modification char(1)
	)

	Create table #AdjustPriceHistoryTable2
	(
	AdjustPriceTableId int identity(1,1), 
	DrugItemPriceHistoryId int,
	DrugItemPriceId int,
	DrugItemId int,
	PriceId int ,
	PriceStartDate datetime,
	PriceStopDate datetime,
	Price decimal(18,2) ,
	Modification char(1)
	)


	Create table #AdjustPriceHistoryTable4
	(
	AdjustPriceTableId int identity(1,1), 
	DrugItemPriceHistoryId int,
	DrugItemPriceId int,
	DrugItemId int,
	PriceId int ,
	PriceStartDate datetime,
	PriceStopDate datetime,
	Price decimal(18,2) ,
	Modification char(1)
	)

	Create table #PriceId
	(PriceId int identity(1,1),AdjustPricetableId int)


	Insert into #AdjustPriceHistoryTable
	(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
	Select DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price, 'Z'
	From DI_DrugItemPricehistory
	Where DrugItemId = @DrugItemId 
	And IsFSS = 1
	And Priceid >= 0
	And (DrugItemSubItemId is null or len(DrugItemSubItemId) = 0 or DrugItemSubItemId = -1)
	order by PriceStartDate,PriceStopDate

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 

    Select @DrugItemPriceHistoryId = DrugItemPriceHistoryId
    From DI_DrugItemPricehistory
	Where DrugItemId = @DrugItemId 
	And IsFSS = 1
	And Priceid = 0
	And (DrugItemSubItemId is null or len(DrugItemSubItemId) = 0 or DrugItemSubItemId = -1)
    
    Select @AdjustPriceTableId = AdjustPriceTableId
    From #AdjustPriceHistoryTable
    where DrugItemPriceHistoryId = @DrugItemPriceHistoryId 

	Insert into #AdjustPriceHistoryTable1
	(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
	Select DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification
	From #AdjustPriceHistoryTable
	Where AdjustPriceTableId <= @AdjustPriceTableId
	Order by AdjustPriceTableId

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 

	Insert into #AdjustPriceHistoryTable2
	(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
	Select DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification
	From #AdjustPriceHistoryTable
	Where AdjustPriceTableId > @AdjustPriceTableId
	Order by AdjustPriceTableId 

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 
 

	Select @count1 = count(*) From #AdjustPriceHistoryTable1
	Select @i = @count1

	While (@i >1)
	Begin
		Select  @CurrentPricestartDate = PriceStartDate,
				@CurrentPriceStopDate = PriceStopdate,
				@CurrentDrugItemPriceId = DrugItemPriceId
		from #AdjustPriceHistoryTable1
		Where AdjustPriceTableid = @i  

        Select  @PreviousDrugItemPriceId = DrugItemPriceHistoryId, 
				@PreviousPricestartDate = PriceStartDate,
				@PreviousPriceStopDate = PriceStopdate,
				@PreviousPrice = Price
        from #AdjustPriceHistoryTable1
        Where AdjustPriceTableid = @i-1		


		IF 	(Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))= 0 
		Begin
			If (Select DateDiff("d",@PreviousPriceStartDate,@CurrentPricestartDate))= 0 
			Begin
				Update  #AdjustPriceHistoryTable1
				Set --PriceStopdate =   cast( Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
					Modification = 'D'
				Where DrugItemPriceHistoryId = @PreviousDrugItemPriceId 			

				select @error = @@ERROR
				if @error <> 0 or @rowsAffected <> 1
				BEGIN
					goto ERROREXIT
				END  
			End
			Else If (Select DateDiff("d",@PreviousPriceStartDate,@CurrentPricestartDate))> 0 
			Begin
				Update  #AdjustPriceHistoryTable1
				Set PriceStopdate =   cast( Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
					Modification = 'U'
				Where DrugItemPriceHistoryId = @PreviousDrugItemPriceId 			

				select @error = @@ERROR 
				if @error <> 0 or @rowsAffected <> 1
				BEGIN
					goto ERROREXIT
				END  
			End
			Set @i = -1
		End
		Else IF (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))= 1
		Begin
			Set @i = -1
		End 
		Else If	(Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))> 1
		Begin
			Insert into #AdjustPriceHistoryTable1
			(DrugItemPriceHistoryId,DrugItemPriceId, DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
			Select null,@CurrentDrugItemPriceId,@DrugItemId,cast( Convert(Varchar(10),@PreviousPriceStopDate+1 ,20) as datetime),
					cast(Convert(Varchar (10), @CurrentPriceStartDate-1 ,20) as datetime ),-1, 'I'

			select @error = @@ERROR 
			if @error <> 0 or @rowsAffected <> 1
			BEGIN
				goto ERROREXIT
			END
			Set @i = -1  
		End
		Else If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))< 1
		Begin
			If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestopDate))>=0 
			Begin
				If (Select DateDiff("d",@PreviousPriceStartDate,@CurrentPricestartDate)) = 0
				Begin
					Update  #AdjustPriceHistoryTable1
					Set --PriceStopdate =   cast( Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
						Modification = 'D'
					Where DrugItemPriceHistoryId = @PreviousDrugItemPriceId 			

					select @error = @@ERROR
					if @error <> 0 or @rowsAffected <> 1
					BEGIN
						goto ERROREXIT
					END 
				End
				Else
				Begin
					Update  #AdjustPriceHistoryTable1
					Set PriceStopdate =   cast( Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
						Modification = 'U'
					Where DrugItemPriceHistoryId = @PreviousDrugItemPriceId 			

					select @error = @@ERROR 
					if @error <> 0 or @rowsAffected <> 1
					BEGIN
						goto ERROREXIT
					END 
				End
			End
			Else 
			Begin
				Update  #AdjustPriceHistoryTable1
				Set PriceStopdate =   cast( Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
					Modification = 'U'
				Where DrugItemPriceHistoryId = @PreviousDrugItemPriceId 			

				select @error = @@ERROR 
				if @error <> 0 or @rowsAffected <> 1
				BEGIN
					goto ERROREXIT
				END

				Insert into #AdjustPriceHistoryTable1
				(DrugItemPriceHistoryId,DrugItemPriceId, DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
				Select null,@CurrentDrugItemPriceId,@DrugItemId,cast( Convert(Varchar(10),@CurrentPricestopDate+1 ,20) as datetime),
						cast(Convert(Varchar (10), @PreviousPriceStopDate ,20) as datetime ),@PreviousPrice, 'I'

				select @error = @@ERROR
				if @error <> 0 or @rowsAffected <> 1
				BEGIN
					goto ERROREXIT
				END
			End
			Set @i = -1
		End
	End


	Insert into #AdjustPriceHistoryTable4
	(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
	Select DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification
	From #AdjustPriceHistoryTable1
	order by  AdjustPriceTableId 	

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 

	Insert into #PriceId
	(AdjustPricetableId )
	Select AdjustPriceTableId From #AdjustPriceHistoryTable4
	Where Modification <> 'D'
	Order by PriceStartDate,PriceStopdate

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 
	
	Update a
	Set a.PriceId = b.PriceId
	From #AdjustPriceHistoryTable4 a
	Join #PriceId b
	on a.AdjustPricetableId = b.AdjustPricetableId

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 

	
	Update a
	Set --a.PriceId = b.PriceId,
		a.PriceStartDate = b.PriceStartDate,
		a.PriceStopDate = b.PriceStopDate,
		a.Price = b.Price,
		a.notes = 'Nightly batch process',
		a.ModificationStatusId = - 1,
		a.LastModifiedBy = 'dbo',
		a.LastModificationDate = getdate()
	From DI_DrugItemPriceHistory a
	Join #AdjustPriceHistoryTable4 b
	on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId 
	and Modification = 'U'

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 

	Update a
	Set a.PriceId = b.PriceId
	From DI_DrugItemPriceHistory a
	Join #AdjustPriceHistoryTable4 b
	on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId 
	and Modification <> 'D'

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 	

	Update a
	Set a.PriceId = -1,
		a.Notes = 'Nightly batch process',
		a.LastModifiedBy = 'dbo',
		a.LastModificationDate = getdate()
	From DI_DrugItemPriceHistory a
	Join #AdjustPriceHistoryTable4 b
	on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId 
	and Modification = 'D'

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 
 
 	Update a
	Set a.PriceId = -1,
		a.Notes = 'Nightly batch process',
		a.LastModifiedBy = 'dbo',
		a.LastModificationDate = getdate()
	From DI_DrugItemPriceHistory a
	Join #AdjustPriceHistoryTable2 b
	on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId 


	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 
  

	Insert into DI_DrugItemPriceHistory
	(DrugItemPriceId,DrugItemId,DrugItemSubItemId,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,IsFSS,IsBIG4,IsVA,
	IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,
	AwardedFSSTrackingCustomerRatio,TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,
	LastModificationType,ModificationStatusId,Notes,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
	)
	Select DrugItemPriceId,@drugItemId,null,PriceId,PriceStartDate,PriceStopDate,Price,0,1,0,
	0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
	null,null,null,0,'V',-1,'Nightly batch process','dbo',getdate(),
	'dbo',getdate()
	From #AdjustPriceHistoryTable4 
	Where DrugItemPricehistoryId is null

	select @error = @@ERROR 
	if @error <> 0 
	BEGIN
		goto ERROREXIT
	END 

	Drop table #AdjustPriceHistoryTable
	Drop table #AdjustPriceHistoryTable1
	Drop table #AdjustPriceHistoryTable2
	Drop table #AdjustPriceHistoryTable4
	Drop table #PriceId

	Select  @DrugItemPriceHistoryId = null,@AdjustPriceTableId = null, @count1 = null, @i = null,  
			@CurrentPricestartDate = null, @CurrentPriceStopDate = null, @CurrentDrugItemPriceId = null,
			@PreviousDrugItemPriceId = null, @PreviousPricestartDate = null, @PreviousPriceStopDate = null,
			@PreviousPrice = null
			
	FETCH NEXT FROM Price_History
	INTO @drugitemid	
End
Close Price_History
DeAllocate Price_History

GOTO OKEXIT

ERROREXIT:
	Drop table #AdjustPriceHistoryTable
	Drop table #AdjustPriceHistoryTable1
	Drop table #AdjustPriceHistoryTable2
	Drop table #AdjustPriceHistoryTable4
	Drop table #PriceId

	ROLLBACK TRANSACTION
	RETURN (-1)
	
OKEXIT:
	COMMIT TRANSACTION
	RETURN (0)
	
	


