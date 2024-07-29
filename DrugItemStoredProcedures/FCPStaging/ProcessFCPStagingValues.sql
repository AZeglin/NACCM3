IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessFSSItemsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessFSSItemsData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[ProcessFCPStagingValues]
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




	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Nfamp_STG]') AND type in (N'U'))
	Begin
		DROP TABLE [dbo].[Nfamp_STG]
	End

	CREATE TABLE [dbo].[Nfamp_STG](
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
		[ErrorMsg] [nvarchar](1000) NULL--,
	 CONSTRAINT [PK_Nfamp_STG] PRIMARY KEY CLUSTERED 
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

		Insert into NFAMP_STG
		(ndc_1,ndc_2,ndc_3,n,cnt_no,FCP,[Year])
		Select @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year

		Select  @stgId = @@identity

		Select @contractId = ContractId
		From DI_Contracts
		Where ContractNumber = @cnt_no or NACCMContractNumber = @cnt_no

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

			Update NFAMP_STG
				Set ContractId = @contractId,
					DrugItemId = @drugitemid,
					DrugItemNDCId = @drugItemNDCId
			Where STGId= @stgId				
				
		End
		Else
		Begin
			select @errorMsg = 'NDC Price Item not found'

			Update NFAMP_STG
				Set ContractId = @contractId,
					DrugItemId = 0,
					DrugItemNDCId = @drugItemNDCId,
					ErrorMsg = @errorMsg
			Where STGId= @stgId		
		End



		FETCH NEXT FROM NFAMP_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year
	End
	Close NFAMP_Cursor
	DeAllocate NFAMP_Cursor



