IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdjustPriceSequence]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdjustPriceSequence]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE proc [dbo].[AdjustPriceSequence]
(
@DrugItemPriceId int,
@ModificationStatusId int,
@loginName nvarchar(120),
@ContractNumber nvarchar(20),
@UpdateSource char(1)
)
As

Set nocount on

Declare @maxPriceId int,
		@AdjustPriceTableId int,
		@NextDrugItemPriceId int,
		@nextPricestartDate datetime,
		@nextPriceStopDate datetime,
		@NextPrice decimal(18,2),
		@CurrentPricestartDate datetime,
		@CurrentPriceStopDate datetime,
		@error int,
		@errorMsg varchar(1000),
		@i int,
		@count int,
		@rowsAffected int,
		@DrugitemId int,
		@IsTemp bit,
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
		@count3 int,
		@AdjustPriceTableId1 int,
		@AdjustPriceTableId2 int,
		@CurrentTemporary bit,
		@PreviousTemporary bit,
		@check2 int,
		@check1 int,
		@modification1 char(1),
		@modification2 char(1),
		@CurrentPrice decimal(18,2),
		@DLALoggingDrugItemPriceId int,
		@rowCount int,
		@ExistingPriceStartDate datetime,
		@NewPriceStartDate datetime,
		@retVal int
		
BEGIN TRANSACTION


	if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[#AdjustPriceTableForSpreadsheet]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	Begin
		 WAITFOR DELAY '000:00:05'
	End


	Create table #AdjustPriceTableForSpreadsheet
	(
	AdjustPriceTableId int identity(1,1), 
	DrugItemPriceId int,
	DrugItemId int,
	PriceId int ,
	IsTemporary bit,
	PriceStartDate datetime,
	PriceStopDate datetime,
	Price decimal(18,2) ,
	Modification char(1)
	)


	Create table #AdjustPriceTableForSpreadsheet1
	(
	AdjustPriceTableId int identity(1,1), 
	DrugItemPriceId int,
	DrugItemId int,
	PriceId int ,
	IsTemporary bit,	
	PriceStartDate datetime,
	PriceStopDate datetime,
	Price decimal(18,2) ,
	Modification char(1)
	)

	Create table #AdjustPriceTableForSpreadsheet2
	(
	AdjustPriceTableId int identity(1,1), 
	DrugItemPriceId int,
	DrugItemId int,
	PriceId int ,
	IsTemporary bit,	
	PriceStartDate datetime,
	PriceStopDate datetime,
	Price decimal(18,2) ,
	Modification char(1)
	)
	
	Create table #AdjustPriceTableForSpreadsheet3
	(
	AdjustPriceTableId int identity(1,1), 
	DrugItemPriceId int,
	DrugItemId int,
	PriceId int ,
	IsTemporary bit,	
	PriceStartDate datetime,
	PriceStopDate datetime,
	Price decimal(18,2) ,
	Modification char(1)
	)	




	Select  @DrugitemId = DrugItemId,
			@SubItemId= DrugItemSubItemId,
			@HistoricalNValue = HistoricalNValue,
			@IsTemp = IsTemporary,
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
	From DI_DrugItemPrice
	Where DrugItemPriceId = @DrugItemPriceId


	IF @SubItemId is not null
	Begin
		Insert into #AdjustPriceTableForSpreadsheet
		(DrugItemPriceId,DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
		Select DrugItemPriceId,DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,'I'
		From DI_DrugItemPrice
		Where DrugItemId = @DrugitemId 
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
			select @errorMsg = '001: Error processing AdjustPrice table'
			goto ERROREXIT
		END  
	End
	Else
	Begin
		Insert into #AdjustPriceTableForSpreadsheet
		(DrugItemPriceId,DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
		Select DrugItemPriceId,DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,'I'
		From DI_DrugItemPrice
		Where DrugItemId = @DrugitemId 
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
			select @errorMsg = '002: Error processing AdjustPrice table'
			goto ERROREXIT
		END 

	End
	

    Select @maxPriceId = max(AdjustPriceTableId)
    from #AdjustPriceTableForSpreadsheet

    If (@maxPriceId = 1)
    Begin
        Goto OKEXIT 
    End
	
	Select @AdjustPriceTableId = AdjustPriceTableId
    From #AdjustPriceTableForSpreadsheet
    where DrugItemPriceId = @DrugItemPriceId

	Insert into #AdjustPriceTableForSpreadsheet1
	(DrugItemPriceId,DrugItemId,Istemporary,PriceStartDate,PriceStopDate,Price,Modification)
	Select DrugItemPriceId,DrugItemId,@IsTemp,PriceStartDate,PriceStopDate,Price,Modification
	From #AdjustPriceTableForSpreadsheet
	Where AdjustPriceTableId = @AdjustPriceTableId

	select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
	if @error <> 0 
	BEGIN
		select @errorMsg = '003: Error processing AdjustPrice table'
		goto ERROREXIT
	END 

	Select @i = @AdjustPriceTableId, @check1 = @AdjustPriceTableId1
	While @i >1
	Begin
--		print @i
--		Select * from #AdjustPriceTableForSpreadsheet1
			
		Select  @CurrentPricestartDate = PriceStartDate,
				@CurrentPriceStopDate = PriceStopdate,
				@CurrentTemporary = Istemporary
		from #AdjustPriceTableForSpreadsheet1
		Where AdjustPriceTableid = @AdjustPriceTableId1  		
	
	
	    Select  @PreviousDrugItemPriceId = DrugItemPriceId, 
				@PreviousPricestartDate = PriceStartDate,
				@PreviousPriceStopDate = PriceStopdate,
				@PreviousPrice = Price,
				@PreviousTemporary = Istemporary
        from #AdjustPriceTableForSpreadsheet
        Where AdjustPriceTableid = @i-1	
        
        
        If (@CurrentTemporary = 1 and @PreviousTemporary = 1)
			or (@CurrentTemporary = 1 and @PreviousTemporary = 0)
			or (@CurrentTemporary = 0 and @PreviousTemporary = 0)
        Begin
			If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))> 0
			Begin
				If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))= 1
				Begin
					Insert into #AdjustPriceTableForSpreadsheet1
					(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
					Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
							@PreviousPricestartDate,
							@PreviousPriceStopDate,
							@PreviousPrice, 'I'

					select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
					If @error <> 0 
					Begin
						select @errorMsg = '004: Error processing AdjustPrice table1'
						goto ERROREXIT
					End 				
				End
				Else
				Begin
					Insert into #AdjustPriceTableForSpreadsheet1
					(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
					Select null,@DrugitemId,0,
							cast(Convert(Varchar(10),@PreviousPriceStopDate+1 ,20) as datetime),
							cast(Convert(Varchar(10),@CurrentPriceStartDate-1 ,20) as datetime ),
							-1, 'I'

					select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
					If @error <> 0 
					Begin
						select @errorMsg = '0041: Error processing AdjustPrice table1'
						goto ERROREXIT
					End
					
					Select @i = @i+1
					
				End
			End
			Else If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))= 0 
			Begin
				If (Select DateDiff("d",@PreviousPriceStartDate,@CurrentPricestartDate))> 0 
				Begin
					Insert into #AdjustPriceTableForSpreadsheet1
					(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
					Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
							@PreviousPricestartDate,
							cast(Convert(Varchar(10),@CurrentPriceStartDate-1 ,20) as datetime),
							@PreviousPrice, 'I'

					select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
					if @error <> 0 
					Begin
						select @errorMsg = '005: Error processing AdjustPrice table1'
						goto ERROREXIT
					End 				
				End
				Else 
				Begin
					Update #AdjustPriceTableForSpreadsheet
						Set Modification = 'D'
					Where AdjustPriceTableid = @i-1	

					select @error = @@ERROR 
					if @error <> 0 
					Begin
						select @errorMsg = '0051: Error processing AdjustPrice table1'
						goto ERROREXIT
					End 									
				End
			End
			Else If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))< 0 
			Begin
				If (Select DateDiff("d",@PreviousPriceStartDate,@CurrentPricestartDate))> 0 
				Begin
					If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestopDate))>= 0 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet1
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@PreviousPricestartDate,
								cast(Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
						if @error <> 0 
						Begin
							select @errorMsg = '006: Error processing AdjustPrice table1'
							goto ERROREXIT
						End 					
					End
					Else
					Begin
						Insert into #AdjustPriceTableForSpreadsheet1
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@PreviousPricestartDate,
								cast(Convert(Varchar(10),@CurrentPricestartDate-1 ,20) as datetime),
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
						if @error <> 0 
						Begin
							select @errorMsg = '006: Error processing AdjustPrice table1'
							goto ERROREXIT
						End 						
					
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select null,@DrugitemId,@PreviousTemporary,
								@CurrentPricestopDate+1,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR 
						if @error <> 0 
						Begin
							select @errorMsg = '006: Error processing AdjustPrice table1'
							goto ERROREXIT
						End 					
					End
				End
				Else
				Begin
					Update #AdjustPriceTableForSpreadsheet
						Set Modification = 'D'
					Where AdjustPriceTableid = @i-1	

					select @error = @@ERROR 
					if @error <> 0 
					Begin
						select @errorMsg = '0061: Error processing AdjustPrice table1'
						goto ERROREXIT
					End 					
				End	
			End
			
	    End
	    Else If(@CurrentTemporary = 0 and @PreviousTemporary = 1)
	    Begin
			If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))> 0
			Begin
				If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))= 1
				Begin
					Insert into #AdjustPriceTableForSpreadsheet1
					(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
					Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
							@PreviousPricestartDate,
							@PreviousPriceStopDate,
							@PreviousPrice, 'I'

					select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
					If @error <> 0 
					Begin
						select @errorMsg = '004: Error processing AdjustPrice table1'
						goto ERROREXIT
					End 				
				End
				Else
				Begin
					Insert into #AdjustPriceTableForSpreadsheet1
					(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
					Select null,@DrugitemId,0,
							cast(Convert(Varchar(10),@PreviousPriceStopDate+1 ,20) as datetime),
							cast(Convert(Varchar(10),@CurrentPriceStartDate-1 ,20) as datetime ),
							-1, 'I'

					select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
					If @error <> 0 
					Begin
						select @errorMsg = '0041: Error processing AdjustPrice table1'
						goto ERROREXIT
					End 
					
					Select @i = @i+1					
				End
			End			
			Else If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))= 0 
			Begin
				If (Select DateDiff("d",@PreviousPriceStartDate,@CurrentPricestartDate))> 0 
				Begin
					Insert into #AdjustPriceTableForSpreadsheet1
					(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
					Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
							@PreviousPricestartDate,
							@PreviousPriceStopDate,
							@PreviousPrice, 'I'

					select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
					if @error <> 0 
					Begin
						select @errorMsg = '008: Error processing AdjustPrice table1'
						goto ERROREXIT
					End
					
					Update  #AdjustPriceTableForSpreadsheet1
						Set PriceStartDate = @CurrentPricestartDate + 1,
						    Modification = 'U'
					Where AdjustPriceTableid = @AdjustPriceTableId1-1	
					
					select @error = @@ERROR  
					if @error <> 0 
					Begin
						select @errorMsg = '009: Error processing AdjustPrice table1'
						goto ERROREXIT
					End								
				End
				Else 
				Begin
					Insert into #AdjustPriceTableForSpreadsheet1
					(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
					Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
							@PreviousPricestartDate,
							@PreviousPriceStopDate,
							@PreviousPrice, 'I'

					select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
					if @error <> 0 
					Begin
						select @errorMsg = '010: Error processing AdjustPrice table1'
						goto ERROREXIT
					End
					
					Update  #AdjustPriceTableForSpreadsheet1
						Set Modification = 'D'
					Where AdjustPriceTableid = @AdjustPriceTableId1-1	
					
					select @error = @@ERROR 
					if @error <> 0 
					Begin
						select @errorMsg = '011: Error processing AdjustPrice table1'
						goto ERROREXIT
					End						
				End
			End
			Else If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestartDate))< 0 
			Begin
				If (Select DateDiff("d",@PreviousPriceStartDate,@CurrentPricestartDate))>= 0 
				Begin
					If (Select DateDiff("d",@PreviousPriceStopDate,@CurrentPricestopDate))> 0 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet1
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@PreviousPricestartDate,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
						if @error <> 0 
						Begin
							select @errorMsg = '012: Error processing AdjustPrice table1'
							goto ERROREXIT
						End 
						
						Update  #AdjustPriceTableForSpreadsheet1
							Set PriceStartDate = @PreviousPriceStopDate + 1,
								Modification = 'U'
						Where AdjustPriceTableid = @AdjustPriceTableId1-1	
						
						select @error = @@ERROR 
						if @error <> 0 
						Begin
							select @errorMsg = '013: Error processing AdjustPrice table1'
							goto ERROREXIT
						End						
					End
					Else
					Begin
						Insert into #AdjustPriceTableForSpreadsheet1
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@PreviousPricestartDate,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId1 = @@identity 
						if @error <> 0 
						Begin
							select @errorMsg = '014: Error processing AdjustPrice table1'
							goto ERROREXIT
						End 
						
						Update  #AdjustPriceTableForSpreadsheet1
							Set Modification = 'D'
						Where AdjustPriceTableid = @AdjustPriceTableId1-1	
						
						select @error = @@ERROR 
						if @error <> 0 
						Begin
							select @errorMsg = '015: Error processing AdjustPrice table1'
							goto ERROREXIT
						End							
					End
				End
			End								    
	    End
	    Set @i = @i-1
	End


	If exists (Select top 1 1 From #AdjustPriceTableForSpreadsheet2)
	Begin
		Select @AdjustPriceTableId2 = max(AdjustPricetableId)
		From #AdjustPriceTableForSpreadsheet2
		
		Select @check2 = -1
	End 
	Else
	Begin
		Insert into #AdjustPriceTableForSpreadsheet2
		(DrugItemPriceId,DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
		Select DrugItemPriceId,DrugItemId,@IsTemp,PriceStartDate,PriceStopDate,Price,Modification
		From #AdjustPriceTableForSpreadsheet
		Where AdjustPriceTableId = @AdjustPriceTableId

		select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
		If @error <> 0 
		Begin
			select @errorMsg = '016: Error processing AdjustPrice2 table'
			goto ERROREXIT
		End 
		
		Select @check2 = @AdjustPriceTableId2
	End
 
	Select @i = @AdjustPriceTableId
	
	While @i < @maxPriceId
	Begin
--		print @i
--		Print @maxPriceId
--		Select * from #AdjustPriceTableForSpreadsheet2
	
		Select  @CurrentPricestartDate = PriceStartDate,
				@CurrentPriceStopDate = PriceStopdate,
				@CurrentTemporary = Istemporary,
				@CurrentPrice = Price
		from #AdjustPriceTableForSpreadsheet2
		Where AdjustPriceTableid = @AdjustPriceTableId2
		
	    Select  @PreviousDrugItemPriceId = DrugItemPriceId, 
				@PreviousPricestartDate = PriceStartDate,
				@PreviousPriceStopDate = PriceStopdate,
				@PreviousPrice = Price,
				@PreviousTemporary = Istemporary
        from #AdjustPriceTableForSpreadsheet
        Where AdjustPriceTableid = @i+1			
        

        If (@CurrentTemporary = 1 and @PreviousTemporary = 1)
			or (@CurrentTemporary = 1 and @PreviousTemporary = 0)
			or (@CurrentTemporary = 0 and @PreviousTemporary = 0)
        Begin
			If (Select DateDiff("d",@CurrentPricestartDate,@PreviousPriceStartDate))> 0 
			Begin
				If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))>= 0 
				Begin
					If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))= 0 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@CurrentPricestopDate + 1,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '100: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 					
					End
					Else If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))= 1 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@PreviousPriceStartDate,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '101: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 					
					End	
					Else If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))> 1 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select null,@DrugitemId,0,
								@CurrentPricestopDate+1,
								@PreviousPriceStartDate-1,
								-1, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '102: Error processing AdjustPrice table2'
							goto ERROREXIT
						END
						
						Select @i = @i-1 					
					End														
				End
				Else If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))< 0 
				Begin
					If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStopDate))<= 0 
					Begin
						Update #AdjustPriceTableForSpreadsheet
							Set Modification = 'D'
						Where AdjustPriceTableid = @i+1	
						
						select @error = @@ERROR 
						if @error <> 0 
						BEGIN
							select @errorMsg = '103: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 
					End
					Else 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@CurrentPricestopDate + 1,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '104: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 					
					End											
				End
			End        		 	
			Else If	(Select DateDiff("d",@CurrentPricestartDate,@PreviousPriceStartDate))= 0		
			Begin
				If	(Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStopDate))= 0		
				Begin
					Update #AdjustPriceTableForSpreadsheet
						Set Modification = 'D'
					Where AdjustPriceTableid = @i+1	
					
					select @error = @@ERROR
					if @error <> 0 
					BEGIN
						select @errorMsg = '105: Error processing AdjustPrice table2'
						goto ERROREXIT
					END 					
				End	
				Else If	(Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStopDate))> 0		
				Begin
					Insert into #AdjustPriceTableForSpreadsheet2
					(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
					Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
							@CurrentPricestopDate + 1,
							@PreviousPriceStopDate,
							@PreviousPrice, 'I'

					select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
					if @error <> 0 
					BEGIN
						select @errorMsg = '106: Error processing AdjustPrice table2'
						goto ERROREXIT
					END 					
				End								
			End
		End
		Else If (@CurrentTemporary = 0 and @PreviousTemporary = 1)
        Begin
			If (Select DateDiff("d",@CurrentPricestartDate,@PreviousPriceStartDate))> 0 
			Begin
				If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))>= 0 
				Begin
					If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))= 0 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@PreviousPriceStartDate,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '107: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 
						
						Update 	#AdjustPriceTableForSpreadsheet2
							Set Pricestopdate = @PreviousPriceStartDate -1,
								Modification = 'U'
						where AdjustPriceTableId = @AdjustPriceTableId2-1	

						select @error = @@ERROR
						if @error <> 0 
						BEGIN
							select @errorMsg = '1071: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 								
					End
					Else If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))= 1 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@PreviousPriceStartDate,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '108: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 
	
					End	
					Else If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))> 1 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select null,@DrugitemId,0,
								@CurrentPricestopDate+1,
								@PreviousPriceStartDate-1,
								-1, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '109: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 

						Select @i = @i-1						
					End														
				End
				Else If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStartDate))< 0 
				Begin
					If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStopDate))< 0 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@PreviousPriceStartDate ,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '110: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 					
					
						Update 	#AdjustPriceTableForSpreadsheet2
							Set Pricestopdate = @PreviousPriceStartDate -1,
								Modification = 'U'
						where AdjustPriceTableId = @AdjustPriceTableId2-1	
						
						select @error = @@ERROR 
						if @error <> 0 
						BEGIN
							select @errorMsg = '1101: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 
						
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select null,@DrugitemId,@CurrentTemporary,
								@PreviousPriceStopDate + 1 ,
								@CurrentPricestopDate,
								@CurrentPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '1102: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 
								
					End
					Else If (Select DateDiff("d",@CurrentPricestopDate,@PreviousPriceStopDate))>= 0 
					Begin
						Insert into #AdjustPriceTableForSpreadsheet2
						(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
						Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
								@PreviousPriceStartDate ,
								@PreviousPriceStopDate,
								@PreviousPrice, 'I'

						select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
						if @error <> 0 
						BEGIN
							select @errorMsg = '110: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 					
					
						Update 	#AdjustPriceTableForSpreadsheet2
							Set Pricestopdate = @PreviousPriceStartDate -1,
								Modification = 'U'
						where AdjustPriceTableId = @AdjustPriceTableId2-1	
						
						select @error = @@ERROR 
						if @error <> 0 
						BEGIN
							select @errorMsg = '1101: Error processing AdjustPrice table2'
							goto ERROREXIT
						END 
					
					End	
				End
			End        		 	
			Else If	(Select DateDiff("d",@CurrentPricestartDate,@PreviousPriceStartDate))= 0		
			Begin
				Insert into #AdjustPriceTableForSpreadsheet2
				(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)
				Select @PreviousDrugItemPriceId,@DrugitemId,@PreviousTemporary,
						@PreviousPriceStartDate,
						@PreviousPriceStopDate,
						@PreviousPrice, 'I'

				select @error = @@ERROR , @AdjustPriceTableId2 = @@identity 
				if @error <> 0 
				BEGIN
					select @errorMsg = '113: Error processing AdjustPrice table2'
					goto ERROREXIT
				END 

			
				Update #AdjustPriceTableForSpreadsheet
					Set Modification = 'D'
				Where AdjustPriceTableid = @i
				
				select @error = @@ERROR
				if @error <> 0 
				BEGIN
					select @errorMsg = '112: Error processing AdjustPrice table2'
					goto ERROREXIT
				END 					
			End
		End		
		Select @i = @i + 1
	End 

--	Select * from #AdjustPriceTableForSpreadsheet
--	Select * from #AdjustPriceTableForSpreadsheet1
--	Select * from #AdjustPriceTableForSpreadsheet2


	Select @modification1 = Modification 
	From #AdjustPriceTableForSpreadsheet1
	Where AdjustPriceTableId = @check1
	
	Select @modification2 = Modification 
	From #AdjustPriceTableForSpreadsheet2
	Where AdjustPriceTableId = @check2
	
	If @modification2 is null
	Begin
		Select @modification2 = 'I'
	End
	
	If ((@modification1 = 'I' or @modification1 = 'U') and @modification2 = 'I')  
	Begin
		Insert into #AdjustPriceTableForSpreadsheet3
		(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)	
		Select DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification
		From #AdjustPriceTableForSpreadsheet1
		Where Modification <> 'D'
		Order by AdjustPricetableId Desc
		
		Insert into #AdjustPriceTableForSpreadsheet3
		(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)	
		Select DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification
		From #AdjustPriceTableForSpreadsheet2
		Where Modification <> 'D'
		And AdjustPricetableId <> @check2
		Order by AdjustPricetableId 	
	End
	Else If ((@modification1 = 'I' or @modification1 = 'U') and @modification2 = 'U')
	Begin
		Insert into #AdjustPriceTableForSpreadsheet3
		(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)	
		Select DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification
		From #AdjustPriceTableForSpreadsheet1
		Where Modification <> 'D'
		And AdjustPricetableId <> @check1
		Order by AdjustPricetableId Desc
		
		Insert into #AdjustPriceTableForSpreadsheet3
		(DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification)	
		Select DrugItemPriceId, DrugItemId,IsTemporary,PriceStartDate,PriceStopDate,Price,Modification
		From #AdjustPriceTableForSpreadsheet2
		Where Modification <> 'D'
--		And AdjustPricetableId = @check2
		Order by AdjustPricetableId 
	End
	   
	-- compare dates for DLA logging
	if exists( select a.Price from 
				DI_DrugItemPrice a Join #AdjustPriceTableForSpreadsheet3 b on a.DrugItemPriceId = b.DrugItemPriceId 
				where a.PriceStartDate <> b.PriceStartDate )
	BEGIN
		create table #TempDateChanges
		(
			ChangeId int identity(1,1), 
			ExistingPriceStartDate datetime,
			NewPriceStartDate datetime,
			DrugItemPriceId int
		)

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = '1201: Error creating #TempDateChanges'
			goto ERROREXIT
		END 				

		insert into #TempDateChanges
		( ExistingPriceStartDate, NewPriceStartDate, DrugItemPriceId )
		select
		a.PriceStartDate, b.PriceStartDate, a.DrugItemPriceId
		from DI_DrugItemPrice a Join #AdjustPriceTableForSpreadsheet3 b on a.DrugItemPriceId = b.DrugItemPriceId 
		where a.PriceStartDate <> b.PriceStartDate 

		select  @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = '1202: Error inserting into #TempDateChanges'
			goto ERROREXIT
		END 


		while @rowCount > 0
		BEGIN
			select @DLALoggingDrugItemPriceId = isnull( DrugItemPriceId, -1 ),
				@ExistingPriceStartDate = ExistingPriceStartDate,
				@NewPriceStartDate = NewPriceStartDate
			from #TempDateChanges 
			where ChangeId = @rowCount

			exec @retVal = InsertDLAPriceChangeLog @loginName, @ContractNumber, @UpdateSource, 'D', @DrugitemId, @DLALoggingDrugItemPriceId, @DLALoggingDrugItemPriceId, @NewPriceStartDate, @ExistingPriceStartDate, null, null, null, null

			select @error = @@ERROR

			if @error <> 0 or @retVal = -1
			BEGIN
				select @errorMsg = '1201: Error from InsertDLAPriceChangeLog'
				goto ERROREXIT
			END 		

			select @rowCount = @rowCount - 1
		END
	END


	Update a
	Set a.PriceId = b.AdjustPricetableId,
		a.PriceStartDate = b.PriceStartDate,
		a.PriceStopDate = b.PriceStopDate,
		a.Price = b.Price,
		a.ModificationStatusId =@ModificationStatusId,
--		a.LastModifiedBy = @loginName,
		a.LastModificationDate = getdate()
	From DI_DrugItemPrice a
	Join #AdjustPriceTableForSpreadsheet3 b
	on a.DrugItemPriceId = b.DrugItemPriceId 

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating DI_DrugItemPrice table'
		goto ERROREXIT
	END 


	Delete a
	From DI_DrugItemPrice a
	Join #AdjustPriceTableForSpreadsheet1 b
	on a.DrugItemPriceId = b.DrugItemPriceId 
	and b.Modification = 'D'

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting AdjustPrice1 table'
		goto ERROREXIT
	END 
 
	Delete a
	From DI_DrugItemPrice a
	Join #AdjustPriceTableForSpreadsheet2 b
	on a.DrugItemPriceId = b.DrugItemPriceId 
	and b.Modification = 'D'

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting AdjustPrice2 table'
		goto ERROREXIT
	END 


	Delete a
	From DI_DrugItemPrice a
	Join #AdjustPriceTableForSpreadsheet b
	on a.DrugItemPriceId = b.DrugItemPriceId 
	and b.Modification = 'D'

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error deleting AdjustPrice2 table'
		goto ERROREXIT
	END 


	Insert into DI_DrugItemPrice
	(DrugItemId,DrugItemSubItemId,HistoricalNValue,PriceId,PriceStartDate,PriceStopDate,Price,IsTemporary,IsFSS,IsBIG4,IsVA,
	IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,IsSVH1,IsSVH2,IsTMOP,IsUSCG,
	AwardedFSSTrackingCustomerRatio,TrackingCustomerName,CurrentTrackingCustomerPrice,ExcludeFromExport,
	LastModificationType,ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
	)
	Select @DrugitemId,@SubItemId,@HistoricalNValue,AdjustPricetableId,PriceStartDate,PriceStopDate,Price,0,@IsFss,@IsBig4,
	@IsVA,@IsBOP,@IsCMOP,@IsDOD,@IsHHS,@IsIHS,@IsIHS2,@IsDIHS,@IsNIH,@IsPHS,@IsSVH,@IsSVH1,@IsSVH2,
	@IsTMOP,@IsUSCG,null,null,null,0,'V',@ModificationStatusId,@loginName,getdate(),
	@loginName,getdate()
	From #AdjustPriceTableForSpreadsheet3 a
	Where DrugItemPriceId is null
	

	select @error = @@ERROR , @rowsAffected = @@ROWCOUNT 
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting DI_DrugItemPrice table'
		goto ERROREXIT
	END 
	


Set nocount OFF
GOTO OKEXIT

ERROREXIT:
	Drop table #AdjustPriceTableForSpreadsheet
	Drop table #AdjustPriceTableForSpreadsheet1
	Drop table #AdjustPriceTableForSpreadsheet2
	Drop table #AdjustPriceTableForSpreadsheet3


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
	Drop table #AdjustPriceTableForSpreadsheet
	Drop table #AdjustPriceTableForSpreadsheet1
	Drop table #AdjustPriceTableForSpreadsheet2
	Drop table #AdjustPriceTableForSpreadsheet3


	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)
