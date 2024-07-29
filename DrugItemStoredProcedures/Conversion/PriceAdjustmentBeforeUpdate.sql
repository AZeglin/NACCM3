IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PriceAdjustmentBeforeUpdate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [PriceAdjustmentBeforeUpdate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[PriceAdjustmentBeforeUpdate]
(
@drugitempriceid int,
@drugitemid int,
@Isfss bit,
@IsBig4 bit,
@username nvarchar(120),
@modificationstatusid int
)

As 

Declare	@newdrugItemPriceId int,
	@i int,
	@error int,
	@rowsAffected int,
	@errorMsg nvarchar(128),
	@currentPriceStopDate datetime,
	@currentPriceid int,
	@nextPriceStartDate datetime,
	@count int,
	@AdjustPriceId int,
	@AdjustPriceStartdate datetime,
	@AdjustDrugItemPriceId int,
	@AdjustPriceStopDate datetime,
	@PriceStartDate datetime,
	@PriceStopDate datetime,
	@retval int,
	@maxpriceid int
  




If (@drugitemid is null)
Begin
	select @errorMsg = 'Error Code: 001, DrugItemID cannot be null'
	GOTO ERROREXIT
End


Begin transaction 

	Declare DrugItemPriceId_Cursor CURSOR For
	Select DrugItemPriceId 
	From DI_DrugItemPrice
	Where DrugItemId = @drugItemId
	And cast ( Convert(Varchar(10 ),PriceStopDate,20 ) as datetime) < cast ( Convert(Varchar(10 ),getdate(),20 ) as datetime)

	Open DrugItemPriceId_Cursor

	FETCH NEXT FROM DrugItemPriceId_Cursor
	INTO @newdrugItemPriceId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Select  @IsFss = IsFSS,
			@IsBIG4 = IsBig4,
			@PriceStartDate = PriceStartDate,
			@PriceStopDate = PriceStopDate
		From DI_DrugItemPrice
		Where DrugItemPriceId = @newdrugItemPriceId


/*		EXEC @retVal = AdjustDrugItemPriceHistorySequence @newdrugItemPriceId,@drugItemId,@IsFSS,@IsBIG4 ,@PriceStartDate,@PriceStopdate,@UserName ,@ModificationStatusId

		SELECT @error = @@ERROR
		IF @retVal = -1 OR @error > 0
		BEGIN
			select @errorMsg = 'Error Code: 002, Error in Adjusting Price History Sequence for DrugitemPriceId: ' + cast( @newdrugItemPriceId as varchar)
--			Drop Table #AdjustDatesTable
			GOTO ERROREXIT
		END
*/
		Delete from DI_DrugItemPrice
		Where DrugItemPriceId = @newdrugItemPriceId
	
		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error returned when deleting from price table for DrugitemPriceid: ' + cast(@newdrugItemPriceId as varchar)
--			Drop Table #AdjustDatesTable
			goto ERROREXIT
		END	

		FETCH NEXT FROM DrugItemPriceId_Cursor
		INTO @newdrugItemPriceId
	End
	Close DrugItemPriceId_Cursor
	DeAllocate DrugItemPriceId_Cursor

	Create table #AdjustDatesTable
	(	AdjustDatesTableId Int Identity (1,1),
		DrugItemPriceId int,
		PriceId int,
		PriceStartDate datetime,
		PriceStopDate datetime,
		Price Decimal(10,2)
	)


	If (@IsFss = 1 )
	Begin
		Insert into #AdjustDatesTable
			(DrugItemPriceId,PriceStartDate ,PriceStopDate,Price)
		Select
			DrugItemPriceId,PriceStartDate, PriceStopDate,Price
		From DI_DrugItemPrice
		Where DrugItemId = @DrugItemId
		And IsFss = 1
		order by PriceStartDate,PriceStopdate

		select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error code 003, Error returned when Inserting FSS Prices into Adjust Dates Table for DrugItemID: ' + convert( nvarchar(20), @DrugItemId )
			Drop Table #AdjustDatesTable
			goto ERROREXIT
		END

	End
	Else If (@IsBig4 = 1)
	Begin
		Insert into #AdjustDatesTable
			(DrugItemPriceId,PriceStartDate ,PriceStopDate,Price)
		Select
			DrugItemPriceId,PriceStartDate, PriceStopDate,Price
		From DI_DrugItemPrice
		Where DrugItemId = @DrugItemId
		And IsBig4 = 1
		order by PriceStartDate,PriceStopdate

		select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error code 003, Error returned when Inserting Big4 Prices into Adjust Dates Table for DrugItemID: ' + convert( nvarchar(20), @DrugItemId )
			Drop Table #AdjustDatesTable
			goto ERROREXIT
		END
	End
	Else If(@Isfss = 0 and @Isbig4 = 0 )
	Begin
		Insert into #AdjustDatesTable
			(DrugItemPriceId,PriceStartDate ,PriceStopDate,Price)
		Select
			DrugItemPriceId,PriceStartDate, PriceStopDate,Price
		From DI_DrugItemPrice
		Where DrugItemId = @DrugItemId
		And IsBig4 = 0
		And IsFSS = 0
		order by PriceStartDate,PriceStopdate

		select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error code 003, Error returned when Inserting Restricted Prices into Adjust Dates Table for DrugItemID: ' + convert( nvarchar(20), @DrugItemId )
			Drop Table #AdjustDatesTable
			goto ERROREXIT
		END
	End
	


	Set @i = 1
		
	Select @count = Count(*) from #AdjustDatesTable

	While (@i < @count)
	Begin
		Select @maxpriceid = max(priceid) from #AdjustDatesTable
	
		If (@maxpriceid is null)
		Begin
			Update #AdjustDatesTable
			Set PriceId = 1
			Where AdjustDatesTableId = @i

			select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
			if @error <> 0 or @rowsAffected <> 1
			BEGIN
				select @errorMsg = 'Error code: 004, Error returned when Updating Adjust Dates Table for PriceId for DrugItemID: ' + convert( nvarchar(20), @DrugItemId )
				Drop table #AdjustDatesTable
				goto ERROREXIT
			END
		End
		Else
		begin
			Update #AdjustDatesTable
			Set PriceId = @maxpriceid + 1
			Where AdjustDatesTableId = @i

			select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
			if @error <> 0 or @rowsAffected <> 1
			BEGIN
				select @errorMsg = 'Error code: 004, Error returned when Updating Adjust Dates Table for PriceId for DrugItemID: ' + convert( nvarchar(20), @DrugItemId )
				Drop table #AdjustDatesTable
				goto ERROREXIT
			END
		End

		Select 	@currentPriceStopDate = PriceStopDate,
			@currentPriceid = PriceId
		From #AdjustDatesTable
		Where AdjustDatesTableId = @i	

		Select  @nextPriceStartDate = PriceStartDate 
		From #AdjustDatesTable
		Where AdjustDatesTableId = @i+1

		if (cast ( Convert(Varchar (10),@CurrentPriceStopDate ,20) as datetime) <
			cast(Convert(Varchar (10), @nextpricestartdate- 1 ,20) as datetime ))
		Begin
			Insert into #AdjustDatesTable
				(DrugItemPriceId, Priceid,PriceStartDate,PriceStopDate,Price )
			Select
				null,@currentPriceid+1,cast( Convert( Varchar(10),@CurrentPriceStopDate+1 ,20) as datetime ),cast(Convert( Varchar (10), @nextpricestartdate- 1 ,20) as datetime), -1

			select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
			if @error <> 0 or @rowsAffected <> 1
			BEGIN
				select @errorMsg = 'Error code: 003, Error returned when inserting dummy Price into Adjust dates Table table for DrugItemPriceID: ' + convert( nvarchar(20), @DrugItemId )
				Drop table #AdjustDatesTable
				goto ERROREXIT
			END	
		End
		Set @i = @i + 1
	End		


	set @i = 1

	Select @count = count (*) from #AdjustDatesTable

	While (@i < = @count)
	Begin
		Select @AdjustPriceId = PriceId,
			@AdjustDrugItemPriceId = DrugItemPriceId,
			@AdjustPriceStartdate = PriceStartDate,
			@AdjustPriceStopDate = PriceStopDate
		From #AdjustDatesTable
		Where AdjustDatesTableId = @i

 		If @AdjustDrugItemPriceId is null
		Begin


			Insert into DI_DrugItemPrice
				(DrugItemId,PriceId ,PriceStartDate ,PriceStopDate,Price ,IsTemporary ,IsFSS,IsBIG4 ,IsVA, IsBOP,IsCMOP, IsDOD,IsHHS ,IsIHS,IsIHS2 ,
				IsDIHS,IsNIH,IsPHS,IsSVH ,IsSVH1,IsSVH2,IsTMOP, IsUSCG, ModificationStatusId,
				CreatedBy,CreationDate, LastModifiedBy , LastModificationDate)
			Select
				DrugItemId,@AdjustPriceId ,@AdjustPriceStartdate, @AdjustPriceStopDate,-1 ,IsTemporary, IsFSS,IsBIG4 ,IsVA,IsBOP ,IsCMOP, IsDOD,IsHHS, IsIHS,IsIHS2 ,
				IsDIHS,IsNIH,IsPHS,IsSVH ,IsSVH1,IsSVH2,IsTMOP, IsUSCG, @ModificationStatusId,
				@username,Getdate (), @username ,Getdate ()
			From Di_DrugItemPrice
			Where Drugitempriceid = @Drugitempriceid

			select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
			if @error <> 0 or @rowsAffected <> 1
			BEGIN
				select @errorMsg = 'Error code: 004, Error returned when Inserting into History table for DrugItemPriceID: ' + convert( nvarchar(20), @DrugItemPriceId )
				Drop table #AdjustDatesTable
				goto ERROREXIT
			End
		End

		Else
		Begin
			Update DI_DrugItemPrice
			Set 	PriceId = @AdjustPriceId
			Where DrugItemPriceId = @AdjustDrugItemPriceId

 			select @error = @@ERROR , @rowsAffected = @@ROWCOUNT
			if @error <> 0 or @rowsAffected <> 1
			BEGIN
				select @errorMsg = 'Error code: 004, Error returned when Updating PriceId for History table for DrugItemPriceID: ' + convert( nvarchar(20), @DrugItemPriceId )
				Drop table #AdjustDatesTable
				goto ERROREXIT
			End

		End
		Set @i = @i + 1
	End

Drop table #AdjustDatesTable


goto OKEXIT

ERROREXIT:

raiserror( @errorMsg, 16 , 1 )

if @@TRANCOUNT > 1

BEGIN

COMMIT TRANSACTION

END

Else if @@TRANCOUNT = 1

BEGIN

/* only rollback iff this the highest level */

ROLLBACK TRANSACTION

END

RETURN( -1 )

OKEXIT:

If
@@TRANCOUNT > 0

BEGIN

COMMIT
TRANSACTION

END

RETURN
( 0 )


