IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdjustPriceHistorySequence]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdjustPriceHistorySequence]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AdjustPriceHistorySequence]
(
@DrugItemPriceHistoryId int,
@ModificationStatusId int,
@loginName nvarchar(120),
@Notes nvarchar(2000)
)
As

Set nocount on

Declare @maxPriceId int,
		@AdjustPriceTableId int,
		@NextDrugItemPriceId int,
		@nextPricestartDate datetime,
		@nextPriceStopDate datetime,
		@NextPrice decimal(18,2),
		@CurrentDrugItemPriceId int,
		@CurrentPricestartDate datetime,
		@CurrentPriceStopDate datetime,
		@error int,
		@errorMsg varchar(1000),
		@i int,
		@count int,
		@rowsAffected int,
		@drugitemId int,
		@IsFSS bit,
		@IsBIG4 bit,
		@IsVA bit,
		@IsBOP bit,
		@IsCMOP bit,
		@IsDOD bit,
		@IsHHS bit,
		@IsIHS bit,
		@IsIHS2 bit,
		@IsDIHS bit,
		@IsNIH bit,
		@IsPHS bit,
		@IsSVH bit,
		@IsSVH1 bit,
		@IsSVH2 bit,
		@IsTMOP bit,
		@IsUSCG bit,
		@keepThePrice int,
		@HistoricalNValue char(1),
		@PreviousDrugItemPriceId int, 
		@PreviousPricestartDate datetime,
		@PreviousPriceStopDate datetime,
		@PreviousPrice decimal(18,2),
		@SubItemId int,
		@count2 int,
		@count1 int,
		@count3 int
		
		
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[#AdjustPriceHistoryTable]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
Begin
	 WAITFOR DELAY '000:00:05'
End

    
BEGIN TRANSACTION

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


	Select  @drugItemId = DrugItemId,
			@CurrentDrugItemPriceId = DrugItemPriceId,
			@SubItemId = DrugItemSubItemId,
			@HistoricalNValue= HistoricalNValue,
		    @IsFss = IsFss,
			@IsBig4 = IsBig4,
			@IsVA = IsVA,
			@IsBOP = IsBOP,
			@IsCMOP = IsCMOP,
			@IsDOD = IsDOD,
			@IsHHS = IsHHS, 
			@IsIHS = IsIHS,
			@IsIHS2 = IsIHS2,
			@IsDIHS = IsDIHS, 
			@IsNIH = IsNIH, 
			@IsPHS = IsPHS, 
			@IsSVH = IsSVH,
			@IsSVH1 = IsSVH1,
			@IsSVH2 = IsSVH2, 
			@IsTMOP = IsTMOP, 
			@IsUSCG = IsUSCG 
	From DI_DrugItemPriceHistory
	Where DrugItemPriceHistoryId = @DrugItemPriceHistoryId


	If @SubItemId is null
	Begin
		Insert into #AdjustPriceHistoryTable
		(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
		Select DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price, 'U'
		From DI_DrugItemPricehistory
		Where DrugItemId = @DrugItemId 
		And DrugItemSubItemId is null
		And IsFss = @IsFss
		And IsBig4 = @IsBig4
		And	IsVA = @IsVA
		And	IsBOP = @IsBOP
		And	IsCMOP = @IsCMOP 
		And	IsDOD = @IsDOD 
		And	IsHHS = @IsHHS 
		And	IsIHS = @IsIHS
		And IsIHS2 = @IsIHS2
		And IsDIHS = @IsDIHS 
		And IsNIH =	@IsNIH 
		And IsPHS =	@IsPHS 
		And	IsSVH = @IsSVH
		And	IsSVH1 = @IsSVH1
		And IsSVH2 = @IsSVH2 
		And	IsTMOP = @IsTMOP 
		And	IsUSCG = @IsUSCG 
		order by PriceStartDate,PriceStopDate

		select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
		if @error <> 0 
		BEGIN
			select @errorMsg = '001: Error processing AdjustPriceHistory table'
			goto ERROREXIT
		END  
	End
	Else
	Begin
		Insert into #AdjustPriceHistoryTable
		(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
		Select DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,'U'
		From DI_DrugItemPricehistory
		Where DrugItemId = @DrugItemId 
		And DrugItemSubItemId = @SubItemId
		And IsFss = @IsFss
		And IsBig4 = @IsBig4
		And	IsVA = @IsVA
		And	IsBOP = @IsBOP
		And	IsCMOP = @IsCMOP 
		And	IsDOD = @IsDOD 
		And	IsHHS = @IsHHS 
		And	IsIHS = @IsIHS
		And IsIHS2 = @IsIHS2
		And IsDIHS = @IsDIHS 
		And IsNIH =	@IsNIH 
		And IsPHS =	@IsPHS 
		And	IsSVH = @IsSVH
		And	IsSVH1 = @IsSVH1
		And IsSVH2 = @IsSVH2 
		And	IsTMOP = @IsTMOP 
		And	IsUSCG = @IsUSCG 
		order by PriceStartDate,PriceStopDate

		select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
		if @error <> 0 
		BEGIN
			select @errorMsg = '002: Error processing AdjustPriceHistory table'
			goto ERROREXIT
		END 
	End



    Select @maxPriceId = max(AdjustPriceTableId)
    from #AdjustPriceHistoryTable

    If (@maxPriceId = 1)
    Begin
        Goto OKEXIT 
    End

    Select @AdjustPriceTableId = AdjustPriceTableId
    From #AdjustPriceHistoryTable
    where DrugItemPriceHistoryId = @DrugItemPriceHistoryId

	Insert into #AdjustPriceHistoryTable1
	(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
	Select DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification
	From #AdjustPriceHistoryTable
	Where AdjustPriceTableId <= @AdjustPriceTableId
	Order by AdjustPriceTableId -- PriceStartDate,PriceStopDate

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = '003: Error processing AdjustPriceHistory table'
		goto ERROREXIT
	END 


	Insert into #AdjustPriceHistoryTable2
	(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
	Select DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification
	From #AdjustPriceHistoryTable
	Where AdjustPriceTableId >= @AdjustPriceTableId
	Order by AdjustPriceTableId --PriceStartDate,PriceStopDate

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = '004: Error processing AdjustPriceHistory table'
		goto ERROREXIT
	END 
 

	Select @count1 = count(*) From #AdjustPriceHistoryTable1
	Select @i = @count1

    Select  @CurrentPricestartDate = PriceStartDate,
			@CurrentPriceStopDate = PriceStopdate,
			@CurrentDrugItemPriceId = DrugItemPriceId
    from #AdjustPriceHistoryTable
    Where AdjustPriceTableid = @i   

	While (@i >1)
	Begin
        Select  @PreviousDrugItemPriceId = DrugItemPriceHistoryId, 
				@PreviousPricestartDate = PriceStartDate,
				@PreviousPriceStopDate = PriceStopdate,
				@PreviousPrice = Price
        from #AdjustPriceHistoryTable
        Where AdjustPriceTableid = @i-1		


		IF 	(Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))= 0 
		Begin
			If (Select DateDiff("d",@PreviousPriceStartDate,@CurrentPricestartDate))= 0 
			Begin
				Update  #AdjustPriceHistoryTable1
				Set --PriceStopdate =   cast( Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
					Modification = 'D'
				Where DrugItemPriceHistoryId = @PreviousDrugItemPriceId 			

				select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
				if @error <> 0 or @rowsAffected <> 1
				BEGIN
					select @errorMsg = 'Error code: 001, Error returned when Updating AdjustPriceHistory table'
					goto ERROREXIT
				END  
			End
			Else If (Select DateDiff("d",@PreviousPriceStartDate,@CurrentPricestartDate))> 0 
			Begin
				Update  #AdjustPriceHistoryTable1
				Set PriceStopdate =   cast( Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
					Modification = 'U'
				Where DrugItemPriceHistoryId = @PreviousDrugItemPriceId 			

				select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
				if @error <> 0 or @rowsAffected <> 1
				BEGIN
					select @errorMsg = 'Error code: 002, Error returned when Updating AdjustPriceHistory table'
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

			select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
			if @error <> 0 or @rowsAffected <> 1
			BEGIN
				select @errorMsg = 'Error code: 003, Error returned when inserting dummy date into AdjustPriceHistory table'
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

					select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
					if @error <> 0 or @rowsAffected <> 1
					BEGIN
						select @errorMsg = 'Error code: 004, Error returned when Updating AdjustPricehistory table'
						goto ERROREXIT
					END 
				End
				Else
				Begin
					Update  #AdjustPriceHistoryTable1
					Set PriceStopdate =   cast( Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
						Modification = 'U'
					Where DrugItemPriceHistoryId = @PreviousDrugItemPriceId 			

					select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
					if @error <> 0 or @rowsAffected <> 1
					BEGIN
						select @errorMsg = 'Error code: 005, Error returned when Updating AdjustPricehistory table'
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

				select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
				if @error <> 0 or @rowsAffected <> 1
				BEGIN
					select @errorMsg = 'Error code: 006, Error returned when Updating AdjustPricehistory table'
					goto ERROREXIT
				END

				Insert into #AdjustPriceHistoryTable1
				(DrugItemPriceHistoryId,DrugItemPriceId, DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
				Select null,@CurrentDrugItemPriceId,@DrugItemId,cast( Convert(Varchar(10),@CurrentPricestopDate+1 ,20) as datetime),
						cast(Convert(Varchar (10), @PreviousPriceStopDate ,20) as datetime ),@PreviousPrice, 'I'

				select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
				if @error <> 0 or @rowsAffected <> 1
				BEGIN
					select @errorMsg = 'Error code: 007, Error returned when inserting dummy date into AdjustPricehistory table'
					goto ERROREXIT
				END
			End
			Set @i = -1
		End
	End

	
	Select @count2 = count(*) From #AdjustPriceHistoryTable1
	If @count1 = @count2
	Begin
		set @i = 1
		select @count3 = count(*) From #AdjustPriceHistoryTable2

		While (@i < @count3)
		Begin
			Select  @NextDrugItemPriceId = DrugItemPriceHistoryId, 
					@NextPricestartDate = PriceStartDate,
					@NextPriceStopDate = PriceStopdate,
					@NextPrice = Price
			from #AdjustPriceHistoryTable2
			Where AdjustPriceTableid = @i+1		
		
			If (Select DateDiff("d",@CurrentPricestopDate,@NextPricestartDate)) <= 0
			Begin
				If (Select DateDiff("d",@CurrentPricestopDate,@NextPricestopDate)) <= 0
				Begin

					Update  #AdjustPriceHistoryTable2
					Set --PriceStopdate =   cast( Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
						Modification = 'D'
					Where DrugitemPriceHistoryid = @NextDrugItemPriceId 			

					select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
					if @error <> 0 or @rowsAffected <> 1
					BEGIN
						select @errorMsg = 'Error code: 008, Error returned when Updating AdjustPriceHistory table'
						goto ERROREXIT
					END 
				End
				Else If (Select DateDiff("d",@CurrentPricestopDate,@NextPricestopDate)) >=1
				Begin

					Update  #AdjustPriceHistoryTable2
					Set PriceStartdate =   cast( Convert(Varchar(10),@CurrentPricestopDate+1 ,20) as datetime),
						Modification = 'U'
					Where DrugitemPriceHistoryid = @NextDrugItemPriceId 			

					select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
					if @error <> 0 or @rowsAffected <> 1
					BEGIN
						select @errorMsg = 'Error code: 009, Error returned when Updating AdjustPriceHistory table'
						goto ERROREXIT
					END 
					Set @i = 100000
				End
			End
			Else If (Select DateDiff("d",@CurrentPricestopDate,@NextPricestartDate)) = 1
			Begin

				Set @i = 100000		
			End
			Else If (Select DateDiff("d",@CurrentPricestopDate,@NextPricestartDate)) > 1
			Begin

				Insert into #AdjustPriceHistoryTable2
				(DrugitemPriceHistoryid,DrugItemPriceId, DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
				Select null,@CurrentDrugItemPriceId,@DrugItemId,cast( Convert(Varchar(10),@CurrentPricestopDate+1 ,20) as datetime),
						cast(Convert(Varchar (10), @NextPricestartDate-1 ,20) as datetime ),-1, 'I'

				select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
				if @error <> 0 or @rowsAffected <> 1
				BEGIN
					select @errorMsg = 'Error code: 010, Error returned when inserting dummy date into AdjustPriceHistory table'
					goto ERROREXIT
				END
				Set @i = 100000
			End	
			Set @i = @i + 1
		End
	End



	
	Insert into #AdjustPriceHistoryTable4
	(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
	Select DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification
	From #AdjustPriceHistoryTable1
	order by  AdjustPriceTableId 	

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = '005: Error processing AdjustPricehistory table'
		goto ERROREXIT
	END 


	Insert into #AdjustPriceHistoryTable4
	(DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification)
	Select a.DrugItemPriceHistoryId,a.DrugItemPriceId,a.DrugItemId,a.PriceStartDate,a.PriceStopDate,a.Price,a.Modification
	From 
	(
	Select AdjustPriceTableId,DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification
	From #AdjustPriceHistoryTable2
	Where DrugItemPriceHistoryId <> @DrugItemPriceHistoryId
	Union
	Select AdjustPriceTableId,DrugItemPriceHistoryId,DrugItemPriceId,DrugItemId,PriceStartDate,PriceStopDate,Price,Modification
	From #AdjustPriceHistoryTable2
	Where DrugItemPriceId = @CurrentDrugItemPriceId
	and price = -1
	)a
	order by  a.AdjustPriceTableId 	

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = '006: Error processing AdjustPricehistory table'
		goto ERROREXIT
	END 



	
	Insert into #PriceId
	(AdjustPricetableId )
	Select AdjustPriceTableId From #AdjustPriceHistoryTable4
	Where Modification <> 'D'
	Order by PriceStartDate,PriceStopdate

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = '007: Error processing AdjustPricehistory table'
		goto ERROREXIT
	END 
	

	
	Update a
	Set a.PriceId = b.PriceId
	From #AdjustPriceHistoryTable4 a
	Join #PriceId b
	on a.AdjustPricetableId = b.AdjustPricetableId

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = '008: Error processing AdjustPricehistory table'
		goto ERROREXIT
	END 

	
	Update a
	Set a.PriceId = b.PriceId,
		a.PriceStartDate = b.PriceStartDate,
		a.PriceStopDate = b.PriceStopDate,
		a.Price = b.Price,
		a.ModificationStatusId =@ModificationStatusId,
		a.LastModifiedBy = @loginName,
		a.LastModificationDate = getdate()
	From DI_DrugItemPriceHistory a
	Join #AdjustPriceHistoryTable4 b
	on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId 
	and Modification <> 'D'

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = '009: Error processing AdjustPricehistory table'
		goto ERROREXIT
	END 

		
	Delete a
	From DI_DrugItemPriceHistory a
	Join #AdjustPriceHistoryTable4 b
	on a.DrugItemPriceHistoryId = b.DrugItemPriceHistoryId 
	and b.Modification = 'D'

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = '010: Error processing AdjustPricehistory table'
		goto ERROREXIT
	END 
 
	Select @Notes = @Notes + ';AdjustPriceHistorySequenceInsert'

	Insert into DI_DrugItemPriceHistory
	(DrugItemPriceId,DrugItemId,DrugItemSubItemId,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,IsFSS,IsBIG4,IsVA,
	IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,
	AwardedFSSTrackingCustomerRatio,TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,
	LastModificationType,ModificationStatusId,Notes,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
	)
	Select DrugItemPriceId,@drugItemId,@SubItemId,PriceId,PriceStartDate,PriceStopDate,Price,0,@IsFss,@IsBig4,
	@IsVA,@IsBOP,@IsCMOP,@IsDOD,@IsHHS,@IsIHS,@IsIHS2,@IsDIHS,@IsNIH,@IsPHS,@IsSVH,@IsSVH1,@IsSVH2,
	@IsTMOP,@IsUSCG,null,null,null,0,'V',@ModificationStatusId,@Notes,@loginName,getdate(),
	@loginName,getdate()
	From #AdjustPriceHistoryTable4 a
	Where DrugItemPricehistoryId is null

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = '011: Error processing AdjustPricehistory table'
		goto ERROREXIT
	END 

Set nocount OFF
GOTO OKEXIT

ERROREXIT:
	Drop table #AdjustPriceHistoryTable
	Drop table #AdjustPriceHistoryTable1
	Drop table #AdjustPriceHistoryTable2
	Drop table #AdjustPriceHistoryTable4
	Drop table #PriceId

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
	Drop table #AdjustPriceHistoryTable
	Drop table #AdjustPriceHistoryTable1
	Drop table #AdjustPriceHistoryTable2
	Drop table #AdjustPriceHistoryTable4
	Drop table #PriceId

	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)
	
	
	
