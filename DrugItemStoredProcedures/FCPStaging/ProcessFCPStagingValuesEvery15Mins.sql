IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ProcessFCPStagingValuesEvery15Mins')
	BEGIN
		DROP  Procedure  ProcessFCPStagingValuesEvery15Mins
	END

GO

CREATE procedure [dbo].[ProcessFCPStagingValuesEvery15Mins]
As

Declare @cnt_no nvarchar(20),
	@contractId int,
	@drugItemNDCId int,
	@error int,
	@errorMsg nvarchar(512),
	@ndc_1 char(5),
	@ndc_2 char(4),
	@ndc_3 char(2),
	@n char(1),
	@drugitemid int,
	@priceId int,
	@NDCWithNId int,
	@fcp decimal(9,2),
	@year int,
	@stgId int


	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Nfamp_STGEvery15Mins]') AND type in (N'U'))
	Begin
		DROP TABLE [dbo].[Nfamp_STGEvery15Mins]
	End

	CREATE TABLE [dbo].[Nfamp_STGEvery15Mins](
		[STGId] [int] IDENTITY(1,1) NOT NULL,
		[ndc_1] [char](5) NOT NULL,
		[ndc_2] [char](4) NOT NULL,
		[ndc_3] [char](2) NOT NULL,
		[n] [char](1) NULL,
		[cnt_no] [char](11) NOT NULL,
		[FCP] [decimal](9, 2) NULL,
		[Year] [int] NOT NULL,
		[ContractId] [int] NULL,
		[DrugItemId] [int] NULL,
		[DrugItemNDCId] [int] NULL,
		[ErrorMsg] [nvarchar](1000) NULL
	 CONSTRAINT [PK_Nfamp_STGEvery15Mins] PRIMARY KEY CLUSTERED 
	(
		[STGId] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]




	Declare NFAMP_Cursor CURSOR For
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n,
				cnt_no,fcp_2000,2000
		from nfamp2000
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n				
				,cnt_no,fcp_2001,2001
		from nfamp2001
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n		
				,cnt_no,fcp_2002,2002
		from nfamp2002
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n			
				,cnt_no,fcp_2003,2003
		from nfamp2003
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n		
				,cnt_no,fcp_2004,2004
		from nfamp2004
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n		
				,cnt_no,fcp_2005,2005
		from nfamp2005
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n		
				,cnt_no,fcp_2006,2006
		from nfamp2006
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n		
			,cnt_no,fcp_2007,2007
		from nfamp2007
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n		
				,cnt_no,fcp_2008,2008
		from nfamp2008
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n		
				,cnt_no,fcp_2009,2009
		from nfamp2009
		Union
		Select ndc_1,ndc_2,ndc_3,
				Case 
					When len(n)=0 or n is null then null
					Else N
				End as n		
				,cnt_no,fcp_2010,2010
		from nfamp2010

	Open NFAMP_Cursor
	FETCH NEXT FROM NFAMP_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Select @drugitemid = Null,@contractid = Null, @drugItemNDCId = Null, @stgId = Null

		Insert into [Nfamp_STGEvery15Mins]
		(ndc_1,ndc_2,ndc_3,n,cnt_no,FCP,[Year])
		Select @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year

		Select  @stgId = @@identity

		Select @contractId = ContractId
		From DI_Contracts
		Where ContractNumber = @cnt_no or NACCMContractNumber = @cnt_no

		If @contractId is null
		Begin
			If (LEFT(@cnt_no,5) = 'V797P')
			Begin
				Select @new_cntno = RIGHT(@cnt_no,5)
			End
		
			Select @contractId = ContractId
			From DI_Contracts
			Where ContractNumber = @new_cntno or NACCMContractNumber = @new_cntno 
		End


/*		Select @drugItemNDCId = DrugItemNDCId
		From Di_DrugItemNDC
		Where FDAAssignedLabelerCode = @ndc_1
		And ProductCode = @ndc_2
		And PackageCode = @ndc_3


		If exists (Select top 1 1 from DI_DrugItems where contractid = @contractId 
					and drugitemndcid = @drugItemNDCId 
				   )
		Begin
			Select @drugitemid = Drugitemid
			from DI_DrugItems where contractid = @contractId 
			and drugitemndcid = @drugItemNDCId and HistoricalNValue is null

			Update [Nfamp_STGEvery15Mins]
				Set ContractId = @contractId,
					DrugItemId = @drugitemid,
					DrugItemNDCId = @drugItemNDCId
			Where STGId= @stgId				
				
		End
		Else
		Begin
			select @errorMsg = 'NDC Price Item not found'

			Update [Nfamp_STGEvery15Mins]
				Set ContractId = @contractId,
					DrugItemId = 0,
					DrugItemNDCId = @drugItemNDCId,
					ErrorMsg = @errorMsg
			Where STGId= @stgId		
		End

*/

		Select @count = count(*) 
		From Di_DrugItemNDC
		Where FDAAssignedLabelerCode = @ndc_1
		And ProductCode = @ndc_2
		And PackageCode = @ndc_3

		If @count = 0
		Begin
			Select @drugItemNDCId = 0

			select @errorMsg = 'NDC Price Item not found'
		
			Update [Nfamp_STGEvery15Mins]
				Set ContractId = @contractId,
					DrugItemId = 0,
					DrugItemNDCId = @drugItemNDCId,
					ErrorMsg = @errorMsg
			Where STGId= @stgId	

		End
		Else If @count = 1
		Begin
			Select @drugItemNDCId = DrugItemNDCId
			From Di_DrugItemNDC
			Where FDAAssignedLabelerCode = @ndc_1
			And ProductCode = @ndc_2
			And PackageCode = @ndc_3

			If exists (Select top 1 1 from DI_DrugItems where contractid = @contractId 
								and drugitemndcid = @drugItemNDCId 
							   )
			Begin
				Select @drugitemid = Drugitemid
				from DI_DrugItems where contractid = @contractId 
				and drugitemndcid = @drugItemNDCId and HistoricalNValue is null

				Update [Nfamp_STGEvery15Mins]
					Set ContractId = @contractId,
						DrugItemId = @drugitemid,
						DrugItemNDCId = @drugItemNDCId
				Where STGId= @stgId				
					
			End
			Else
			Begin
				select @errorMsg = 'NDC Price Item not found'
			
				Update [Nfamp_STGEvery15Mins]
					Set ContractId = @contractId,
						DrugItemId = 0,
						DrugItemNDCId = @drugItemNDCId,
						ErrorMsg = @errorMsg
				Where STGId= @stgId	
	
			End
		End
		Else If @count > 1
		Begin
			Select @drugitemid = DrugItemId
			From DI_DrugItems where contractid = @contractId 
			and drugitemndcid in (Select  DrugItemNDCId
									From Di_DrugItemNDC
									Where FDAAssignedLabelerCode = @ndc_1
									And ProductCode = @ndc_2
									And PackageCode = @ndc_3  
								  )
			and HistoricalNValue is null

			IF @drugitemid is null
			Begin
				select @errorMsg = 'NDC Price Item not found'

				Update [Nfamp_STGEvery15Mins]
					Set ContractId = @contractId,
						DrugItemId = 0,
						DrugItemNDCId = 0,
						ErrorMsg = @errorMsg
				Where STGId= @stgId	
			End
			Else
			Begin
				Select @drugItemNDCId = DrugItemNDCId
				From Di_DrugItems
				where drugitemid = @drugitemid

				Update [Nfamp_STGEvery15Mins]
					Set ContractId = @contractId,
						DrugItemId = @drugitemid,
						DrugItemNDCId = @drugItemNDCId
				Where STGId= @stgId		

			End
		End




		FETCH NEXT FROM NFAMP_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year
	End
	Close NFAMP_Cursor
	DeAllocate NFAMP_Cursor



