IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ProcessPriceStopDateForBig4')
	BEGIN
		DROP  Procedure  ProcessPriceStopDateForBig4
	END

GO

CREATE Procedure ProcessPriceStopDateForBig4
As
	Declare @drugitemid int,@contractExpDate datetime, @discontinuationDate datetime,@count int,
		@drugitempriceid int,@pricestopdate datetime, @pricestartdate datetime,@errorMsg nvarchar(255)

	Declare Drugitem_Cursor CURSOR For
		Select distinct Drugitemid
		From di_drugitemprice
		where IsBig4 =1 

	Open Drugitem_Cursor
	FETCH NEXT FROM Drugitem_Cursor
	INTO @drugitemid
	
	WHILE @@FETCH_STATUS = 0
	BEGIN
		Select @contractExpDate = 
								Case 
									When c.Dates_Completion is not null then c.Dates_Completion
									else c.Dates_CntrctExp
								End ,
				@discontinuationDate = a.discontinuationdate
		From DI_DrugItems a
		join DI_Contracts b
		on a.ContractId = b.ContractId
		join NAC_CM.dbo.tbl_cntrcts c
		on b.NACCMContractId = c.Contract_Record_ID
		where a.DrugItemId = @drugitemid
				
		Select @count = count(*) 
		From di_drugitemprice
		where drugitemid = @drugitemid
		and IsBig4 =1 
		and (istemporary is null or istemporary = 0)

		If @count = 0
		Begin
			Set @errorMsg = 'Do Nothing'
		End
		Else If @count = 1
		Begin
			Select  @drugitempriceid =DrugitemPriceId,
					@pricestopdate = Pricestopdate,
					@pricestartdate = Pricestartdate
			From Di_DrugItemPrice
			Where drugitemid = @drugitemid
			and IsBig4 =1 
			and (istemporary is null or istemporary = 0)				

			IF @discontinuationDate is null
			Begin
				Insert into testNFAMPProcess
				(DrugitemPriceId,Drugitemid,ContractStopDate,DiscontinuationDate,PriceStopDate,
					ExpectedStopDate,Stat,ItemType)
				Select
					@drugitempriceid,@drugitemid,@contractExpDate,@discontinuationDate,@pricestopdate,
					@contractExpDate,'Active','BIG4'				
			End
			Else
			Begin
				Insert into testNFAMPProcess
				(DrugitemPriceId,Drugitemid,ContractStopDate,DiscontinuationDate,PriceStopDate,
					ExpectedStopDate,Stat,ItemType)
				Select
					@drugitempriceid,@drugitemid,@contractExpDate,@discontinuationDate,@pricestopdate,
					@discontinuationDate,'Expired','BIG4'			
			End
		End
		Else
		Begin
			Select @pricestartdate = max(pricestartdate)
			From Di_DrugItemPrice
			Where drugitemid = @drugitemid
			and IsBig4 =1 
			and (istemporary is null or istemporary = 0)				

			Select  @drugitempriceid =DrugitemPriceId,
					@pricestopdate = Pricestopdate,
					@pricestartdate = Pricestartdate
			From Di_DrugItemPrice
			Where drugitemid = @drugitemid
			and IsBig4 =1 
			and Pricestartdate = @pricestartdate
			and (istemporary is null or istemporary = 0)	

			IF @discontinuationDate is null
			Begin
				Insert into testNFAMPProcess
				(DrugitemPriceId,Drugitemid,ContractStopDate,DiscontinuationDate,PriceStopDate,
					ExpectedStopDate,Stat,ItemType)
				Select
					@drugitempriceid,@drugitemid,@contractExpDate,@discontinuationDate,@pricestopdate,
					@contractExpDate,'Active','BIG4'				
			End
			Else
			Begin
				Insert into testNFAMPProcess
				(DrugitemPriceId,Drugitemid,ContractStopDate,DiscontinuationDate,PriceStopDate,
					ExpectedStopDate,Stat,ItemType)
				Select
					@drugitempriceid,@drugitemid,@contractExpDate,@discontinuationDate,@pricestopdate,
					@discontinuationDate,'Expired','BIG4'			
			End		
		End
	

		FETCH NEXT FROM Drugitem_Cursor
		INTO @drugitemid
	End	
	Close Drugitem_Cursor
	DeAllocate Drugitem_Cursor
