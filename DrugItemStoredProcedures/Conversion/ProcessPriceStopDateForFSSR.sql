IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ProcessPriceStopDateForFSSR')
	BEGIN
		DROP  Procedure  ProcessPriceStopDateForFSSR
	END

GO

CREATE Procedure ProcessPriceStopDateForFSSR
As
	Declare @drugitemid int,@contractExpDate datetime, @discontinuationDate datetime,@count int,
		@drugitempriceid int,@pricestopdate datetime, @pricestartdate datetime,@errorMsg nvarchar(255),
		@IsVA bit,@IsBOP bit,@IsCMOP bit,@IsDOD bit,@IsHHS bit,@IsIHS bit,@IsIHS2 bit,@IsDIHS bit,
		@IsNIH bit,@IsPHS bit,@IsSVH bit,@IsSVH1 bit,@IsSVH2 bit,@IsTMOP bit,@IsUSCG bit,@IsFHCC bit		

	Declare Drugitem_Cursor CURSOR For
		Select distinct Drugitemid,IsVA,IsBOP,IsCMOP,IsDOD,IsHHS,IsIHS,IsIHS2,IsDIHS,IsNIH,IsPHS,IsSVH,
						IsSVH1,IsSVH2,IsTMOP,IsUSCG,IsFHCC
		From di_drugitemprice
		where IsFSS = 0 and IsBig4 = 0 

	Open Drugitem_Cursor
	FETCH NEXT FROM Drugitem_Cursor
	INTO @drugitemid,@IsVA,@IsBOP,@IsCMOP,@IsDOD,@IsHHS,@IsIHS,@IsIHS2,@IsDIHS,@IsNIH,@IsPHS,@IsSVH,
		 @IsSVH1,@IsSVH2,@IsTMOP,@IsUSCG,@IsFHCC
	
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
		and IsFSS = 0 and IsBig4 = 0 
		and IsVA = @IsVA and ISBOP = @IsBOP and ISCMOP = @IsCMOP
		and IsDOD = @IsDOD and IsHHS = @IsHHS and IsIHS = @IsIHS and IsIHS2 = @IsIHS2
		and IsDIHS = @IsDIHS and IsNIH = @IsNIH and IsPHS = @IsPHS and IsSVH = @IsSVH
		and IsSVH1 = @IsSVH1 and IsSVH2 = @IsSVH2 and IsTMOP = @IsTMOP and IsUSCG = @IsUSCG
		and IsFHCC = @IsFHCC
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
			and IsFSS = 0 and IsBig4 = 0
			and IsVA = @IsVA and ISBOP = @IsBOP and ISCMOP = @IsCMOP
			and IsDOD = @IsDOD and IsHHS = @IsHHS and IsIHS = @IsIHS and IsIHS2 = @IsIHS2
			and IsDIHS = @IsDIHS and IsNIH = @IsNIH and IsPHS = @IsPHS and IsSVH = @IsSVH
			and IsSVH1 = @IsSVH1 and IsSVH2 = @IsSVH2 and IsTMOP = @IsTMOP and IsUSCG = @IsUSCG
			and IsFHCC = @IsFHCC			 
			and (istemporary is null or istemporary = 0)				

			IF @discontinuationDate is null
			Begin
				Insert into testNFAMPProcess
				(DrugitemPriceId,Drugitemid,ContractStopDate,DiscontinuationDate,PriceStopDate,
					ExpectedStopDate,Stat,ItemType)
				Select
					@drugitempriceid,@drugitemid,@contractExpDate,@discontinuationDate,@pricestopdate,
					@contractExpDate,'Active','FSSR'				
			End
			Else
			Begin
				Insert into testNFAMPProcess
				(DrugitemPriceId,Drugitemid,ContractStopDate,DiscontinuationDate,PriceStopDate,
					ExpectedStopDate,Stat,ItemType)
				Select
					@drugitempriceid,@drugitemid,@contractExpDate,@discontinuationDate,@pricestopdate,
					@discontinuationDate,'Expired','FSSR'			
			End
		End
		Else
		Begin
			Select @pricestartdate = max(pricestartdate)
			From Di_DrugItemPrice
			Where drugitemid = @drugitemid
			and IsFSS = 0 and IsBig4 = 0
			and IsVA = @IsVA and ISBOP = @IsBOP and ISCMOP = @IsCMOP
			and IsDOD = @IsDOD and IsHHS = @IsHHS and IsIHS = @IsIHS and IsIHS2 = @IsIHS2
			and IsDIHS = @IsDIHS and IsNIH = @IsNIH and IsPHS = @IsPHS and IsSVH = @IsSVH
			and IsSVH1 = @IsSVH1 and IsSVH2 = @IsSVH2 and IsTMOP = @IsTMOP and IsUSCG = @IsUSCG
			and IsFHCC = @IsFHCC
			and (istemporary is null or istemporary = 0)				

			Select  @drugitempriceid =DrugitemPriceId,
					@pricestopdate = Pricestopdate,
					@pricestartdate = Pricestartdate
			From Di_DrugItemPrice
			Where drugitemid = @drugitemid
			and IsFSS = 0 and IsBig4 = 0
			and IsVA = @IsVA and ISBOP = @IsBOP and ISCMOP = @IsCMOP
			and IsDOD = @IsDOD and IsHHS = @IsHHS and IsIHS = @IsIHS and IsIHS2 = @IsIHS2
			and IsDIHS = @IsDIHS and IsNIH = @IsNIH and IsPHS = @IsPHS and IsSVH = @IsSVH
			and IsSVH1 = @IsSVH1 and IsSVH2 = @IsSVH2 and IsTMOP = @IsTMOP and IsUSCG = @IsUSCG
			and IsFHCC = @IsFHCC
			and Pricestartdate = @pricestartdate
			and (istemporary is null or istemporary = 0)	

			IF @discontinuationDate is null
			Begin
				Insert into testNFAMPProcess
				(DrugitemPriceId,Drugitemid,ContractStopDate,DiscontinuationDate,PriceStopDate,
					ExpectedStopDate,Stat,ItemType)
				Select
					@drugitempriceid,@drugitemid,@contractExpDate,@discontinuationDate,@pricestopdate,
					@contractExpDate,'Active','FSSR'				
			End
			Else
			Begin
				Insert into testNFAMPProcess
				(DrugitemPriceId,Drugitemid,ContractStopDate,DiscontinuationDate,PriceStopDate,
					ExpectedStopDate,Stat,ItemType)
				Select
					@drugitempriceid,@drugitemid,@contractExpDate,@discontinuationDate,@pricestopdate,
					@discontinuationDate,'Expired','FSSR'			
			End		
		End
	

		FETCH NEXT FROM Drugitem_Cursor
		INTO @drugitemid,@IsVA,@IsBOP,@IsCMOP,@IsDOD,@IsHHS,@IsIHS,@IsIHS2,@IsDIHS,@IsNIH,@IsPHS,@IsSVH,
		 @IsSVH1,@IsSVH2,@IsTMOP,@IsUSCG,@IsFHCC
	End	
	Close Drugitem_Cursor
	DeAllocate Drugitem_Cursor
