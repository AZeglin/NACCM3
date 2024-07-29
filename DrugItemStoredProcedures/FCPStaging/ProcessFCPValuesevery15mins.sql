IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ProcessFCPValuesevery15mins')
	BEGIN
		DROP  Procedure  ProcessFCPValuesevery15mins
	END

GO

CREATE Procedure ProcessFCPValuesevery15mins
AS


Declare @cnt_no varchar(20),
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
	@fcp decimal(9,2),
	@Year int,
	@YearId int,
	@stgid int,
	@discDate datetime,
	@qa_exempt char(1),
	@status varchar(11),
	@fcpold decimal(9,2),
	@contractNumber nvarchar(20),
	@fcplogid int,
	@creationdate datetime




	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DI_FCPEvery15Mins]') AND type in (N'U'))
	Begin
		DROP TABLE [dbo].[DI_FCPEvery15Mins]
	End

	CREATE TABLE [dbo].[DI_FCPEvery15Mins](
		[FCP_Id] [int] IDENTITY(1,1) NOT NULL,
		[ndc_1] [char](5) NOT NULL,
		[ndc_2] [char](4) NOT NULL,
		[ndc_3] [char](2) NOT NULL,
		[n] [char](1) NULL,
		[cnt_no] [varchar](20) NOT NULL,
		[FCP] [decimal](9, 2) NULL,
		[YearId] [int] NOT NULL,
		[Disc_Date] [datetime] NULL,
		[QA_Exempt] Char(1) NULL,
		[Status]  varchar(11) null,
		[ContractId] [int] NULL,
		[DrugItemId] [int] NULL,
		[DrugItemNDCId] [int] NULL,
		[ErrorMsg] [nvarchar](1000) NULL,
		[ModificationStatusId] [int]  NULL,
		[CreatedBy] [nvarchar](120) NOT NULL,
		[CreationDate] [datetime] NOT NULL,
		[LastModifiedBy] [nvarchar](120) NOT NULL,
		[LastModificationDate] [datetime] NOT NULL,
	 CONSTRAINT [PK_DI_FCPEvery15Mins] PRIMARY KEY CLUSTERED 
	(
		[FCP_Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]



If exists (Select top 1 1 from nfamp_Stg)
Begin

-- For years less than 2011
-- as of 10/2020, the NFAMP_STG does not contain any data with year < 2017 due to selection criteria at the staging table job step
-- so this method of handling older data is commented out
	
--	Declare NFAMP_Cursor_YearLessThan2010 CURSOR For
--		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,FCP,YearID,
--		Case
--			When disc_Date is not null and isdate(cast(disc_date as varchar(12))) = 1
--				then DISC_Date
--			else Null
--		End	,
--		Case
--			When len(QA_Exempt) > 0 then QA_Exempt
--			else Null
--		End,status
--		from NFAMP_STG
--		where yearid < 2011
----		Where (len(A) = 0 or A is null)
					
--	Open NFAMP_Cursor_YearLessThan2010
--	FETCH NEXT FROM NFAMP_Cursor_YearLessThan2010
--	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status


--	WHILE @@FETCH_STATUS = 0
--	BEGIN
		
--		If @ndc_1 is not null And @ndc_2 is not null or @ndc_3 is not null
--		Begin
--			Select @contractId = ContractId
--			From DI_Contracts
--			Where ContractNumber = @cnt_no or NACCMContractNumber = @cnt_no 
			
--			Select @drugItemNDCId = DrugItemNDCId
--			From DI_DrugItemNDC
--			Where FDAAssignedLabelerCode = @ndc_1
--			And ProductCode = @ndc_2
--			And PackageCode = @ndc_3	

--			If @contractId is null 
--			Begin
--				Insert into [DI_FCPEvery15Mins]
--				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
--				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
--				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
--				)
--				Select
--					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,0,0,
--					0,@year,@discDate,@fcp,@qa_exempt,@status,'ContractId not found',0,
--					user_name(),
--					getdate(),
--					user_name(),
--					getdate()		
--			End
--			Else If @drugItemNDCId is null
--			Begin
--				Insert into [DI_FCPEvery15Mins]
--				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
--				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
--				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
--				)
--				Select
--					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,0,
--					0,@year,@discDate,@fcp,@qa_exempt,@status,'NDCId not found',0,
--					user_name(),
--					getdate(),
--					user_name(),
--					getdate()				
--			End
--			Else
--			Begin
--				Select @drugitemid = Drugitemid
--				from DI_DrugItems where contractid = @contractId 
--				and drugitemndcid = @drugItemNDCId 

--				If @drugitemid is null 
--				Begin
--					Insert into [DI_FCPEvery15Mins]
--					(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
--					 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
--					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
--					)
--					Select
--						@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
--						0,@year,@discDate,@fcp,@qa_exempt,@status,'DrugItemId not found',0,
--						user_name(),
--						getdate(),
--						user_name(),
--						getdate()
--				End
--				Else 
--				Begin
--					Insert into [DI_FCPEvery15Mins]
--					(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
--					 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
--					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
--					)
--					Select
--						@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
--						@drugitemid,@year,@discDate,@fcp,@qa_exempt,@status,null,0,
--						user_name(),
--						getdate(),
--						user_name(),
--						getdate()			
--				End
--			End
--		End
				
--		Select 	@drugitemid = null,@drugItemNDCId = null, @contractid = null							 
		
--		FETCH NEXT FROM NFAMP_Cursor_YearLessThan2010
--		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status
--	End
--	Close NFAMP_Cursor_YearLessThan2010
--	DeAllocate NFAMP_Cursor_YearLessThan2010



-- For years greater than 2010 with PERM

	Declare NFAMP_Cursor_PERM CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,FCP,YearID,
		Case
			When disc_Date is not null and isdate(cast(disc_date as varchar(12))) = 1
				then DISC_Date
			else Null
		End	,
		Case
			When len(QA_Exempt) > 0 then QA_Exempt
			else Null
		End,status
		from NFAMP_STG
		where yearid > 2010
		and status = 'PERM'
--		Where (len(A) = 0 or A is null)
					
	Open NFAMP_Cursor_PERM
	FETCH NEXT FROM NFAMP_Cursor_PERM
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status


	WHILE @@FETCH_STATUS = 0
	BEGIN
		If @ndc_1 is not null And @ndc_2 is not null or @ndc_3 is not null
		Begin
			Select @contractId = ContractId
			From DI_Contracts
			Where ContractNumber = @cnt_no or NACCMContractNumber = @cnt_no 
			
			Select @drugItemNDCId = DrugItemNDCId
			From DI_DrugItemNDC
			Where FDAAssignedLabelerCode = @ndc_1
			And ProductCode = @ndc_2
			And PackageCode = @ndc_3	

			If @contractId is null 
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,0,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'ContractId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()		
			End
			Else If @drugItemNDCId is null
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'NDCId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()				
			End
			Else
			Begin
				Select @drugitemid = Drugitemid
				from DI_DrugItems where contractid = @contractId 
				and drugitemndcid = @drugItemNDCId 

				If @drugitemid is null 
				Begin
					Insert into [DI_FCPEvery15Mins]
					(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
					 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					Select
						@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
						0,@year,@discDate,@fcp,@qa_exempt,@status,'DrugItemId not found',0,
						user_name(),
						getdate(),
						user_name(),
						getdate()
				End
				Else 
				Begin
					Insert into [DI_FCPEvery15Mins]
					(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
					 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					Select
						@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
						@drugitemid,@year,@discDate,@fcp,@qa_exempt,@status,null,0,
						user_name(),
						getdate(),
						user_name(),
						getdate()			
				End
			End
		End
				
		Select 	@drugitemid = null,@drugItemNDCId = null, @contractid = null							 
		
		FETCH NEXT FROM NFAMP_Cursor_PERM
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status
	End
	Close NFAMP_Cursor_PERM
	DeAllocate NFAMP_Cursor_PERM
	
-- For years greater than 2010 with 1PRM -- added this new status code handling on 10/22/2020 per email from CK of PBM

	Declare NFAMP_Cursor_1PRM CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,FCP,YearID,
		Case
			When disc_Date is not null and isdate(cast(disc_date as varchar(12))) = 1
				then DISC_Date
			else Null
		End	,
		Case
			When len(QA_Exempt) > 0 then QA_Exempt
			else Null
		End,status
		from NFAMP_STG
		where yearid > 2010
		and status = '1PRM'
--		Where (len(A) = 0 or A is null)
					
	Open NFAMP_Cursor_1PRM
	FETCH NEXT FROM NFAMP_Cursor_1PRM
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status


	WHILE @@FETCH_STATUS = 0
	BEGIN
		If @ndc_1 is not null And @ndc_2 is not null or @ndc_3 is not null
		Begin
			Select @contractId = ContractId
			From DI_Contracts
			Where ContractNumber = @cnt_no or NACCMContractNumber = @cnt_no 
			
			Select @drugItemNDCId = DrugItemNDCId
			From DI_DrugItemNDC
			Where FDAAssignedLabelerCode = @ndc_1
			And ProductCode = @ndc_2
			And PackageCode = @ndc_3	

			If @contractId is null 
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,0,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'ContractId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()		
			End
			Else If @drugItemNDCId is null
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'NDCId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()				
			End
			Else
			Begin
				Select @drugitemid = Drugitemid
				from DI_DrugItems where contractid = @contractId 
				and drugitemndcid = @drugItemNDCId 

				If @drugitemid is null 
				Begin
					Insert into [DI_FCPEvery15Mins]
					(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
					 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					Select
						@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
						0,@year,@discDate,@fcp,@qa_exempt,@status,'DrugItemId not found',0,
						user_name(),
						getdate(),
						user_name(),
						getdate()
				End
				Else 
				Begin
					If exists (select top 1 1 from [DI_FCPEvery15Mins] where DrugItemId = @drugitemid and Status = 'PERM')
					Begin
						Select @errorMsg = 'Do Nothing'
					End
					Else
					Begin
						Insert into [DI_FCPEvery15Mins]
						(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
						 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
						 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
						)
						Select
							@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
							@drugitemid,@year,@discDate,@fcp,@qa_exempt,@status,null,0,
							user_name(),
							getdate(),
							user_name(),
							getdate()
					End			
				End
			End
		End
				
		Select 	@drugitemid = null,@drugItemNDCId = null, @contractid = null							 
		
		FETCH NEXT FROM NFAMP_Cursor_1PRM
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status
	End
	Close NFAMP_Cursor_1PRM
	DeAllocate NFAMP_Cursor_1PRM



-- For years greater than 2010 with NPS

	Declare NFAMP_Cursor_NPS CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,FCP,YearID,
		Case
			When disc_Date is not null and isdate(cast(disc_date as varchar(12))) = 1
				then DISC_Date
			else Null
		End	,
		Case
			When len(QA_Exempt) > 0 then QA_Exempt
			else Null
		End,status
		from NFAMP_STG
		where yearid > 2010
		and status = 'NPS'
--		Where (len(A) = 0 or A is null)
					
	Open NFAMP_Cursor_NPS
	FETCH NEXT FROM NFAMP_Cursor_NPS
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status


	WHILE @@FETCH_STATUS = 0
	BEGIN
		If @ndc_1 is not null And @ndc_2 is not null or @ndc_3 is not null
		Begin
			Select @contractId = ContractId
			From DI_Contracts
			Where ContractNumber = @cnt_no or NACCMContractNumber = @cnt_no 
			
			Select @drugItemNDCId = DrugItemNDCId
			From DI_DrugItemNDC
			Where FDAAssignedLabelerCode = @ndc_1
			And ProductCode = @ndc_2
			And PackageCode = @ndc_3	

			If @contractId is null 
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,0,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'ContractId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()		
			End
			Else If @drugItemNDCId is null
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'NDCId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()				
			End
			Else
			Begin
				Select @drugitemid = Drugitemid
				from DI_DrugItems where contractid = @contractId 
				and drugitemndcid = @drugItemNDCId 

				If @drugitemid is null 
				Begin
					Insert into [DI_FCPEvery15Mins]
					(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
					 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					Select
						@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
						0,@year,@discDate,@fcp,@qa_exempt,@status,'DrugItemId not found',0,
						user_name(),
						getdate(),
						user_name(),
						getdate()
				End
				Else 
				Begin
					If exists (select top 1 1 from [DI_FCPEvery15Mins] where DrugItemId = @drugitemid and Status in ('PERM', '1PRM' ))
					Begin
						Select @errorMsg = 'Do Nothing'
					End
					Else
					Begin
						Insert into [DI_FCPEvery15Mins]
						(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
						 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
						 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
						)
						Select
							@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
							@drugitemid,@year,@discDate,@fcp,@qa_exempt,@status,null,0,
							user_name(),
							getdate(),
							user_name(),
							getdate()
					End			
				End
			End
		End
				
		Select 	@drugitemid = null,@drugItemNDCId = null, @contractid = null							 
		
		FETCH NEXT FROM NFAMP_Cursor_NPS
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status
	End
	Close NFAMP_Cursor_NPS
	DeAllocate NFAMP_Cursor_NPS



-- For years greater than 2010 with TEMP

	Declare NFAMP_Cursor_TEMP CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,FCP,YearID,
		Case
			When disc_Date is not null and isdate(cast(disc_date as varchar(12))) = 1
				then DISC_Date
			else Null
		End	,
		Case
			When len(QA_Exempt) > 0 then QA_Exempt
			else Null
		End,status
		from NFAMP_STG
		where yearid > 2010
		and status = 'TEMP'
--		Where (len(A) = 0 or A is null)
					
	Open NFAMP_Cursor_TEMP
	FETCH NEXT FROM NFAMP_Cursor_TEMP
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status


	WHILE @@FETCH_STATUS = 0
	BEGIN
		If @ndc_1 is not null And @ndc_2 is not null or @ndc_3 is not null
		Begin
			Select @contractId = ContractId
			From DI_Contracts
			Where ContractNumber = @cnt_no or NACCMContractNumber = @cnt_no 
			
			Select @drugItemNDCId = DrugItemNDCId
			From DI_DrugItemNDC
			Where FDAAssignedLabelerCode = @ndc_1
			And ProductCode = @ndc_2
			And PackageCode = @ndc_3	

			If @contractId is null 
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,0,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'ContractId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()		
			End
			Else If @drugItemNDCId is null
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'NDCId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()				
			End
			Else
			Begin
				Select @drugitemid = Drugitemid
				from DI_DrugItems where contractid = @contractId 
				and drugitemndcid = @drugItemNDCId 

				If @drugitemid is null 
				Begin
					Insert into [DI_FCPEvery15Mins]
					(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
					 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					Select
						@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
						0,@year,@discDate,@fcp,@qa_exempt,@status,'DrugItemId not found',0,
						user_name(),
						getdate(),
						user_name(),
						getdate()
				End
				Else 
				Begin
					If exists (select top 1 1 from [DI_FCPEvery15Mins] where DrugItemId = @drugitemid 
								and Status in ('PERM', '1PRM', 'NPS') )
					Begin
						Select @errorMsg = 'Do Nothing'
					End
					Else
					Begin
						Insert into [DI_FCPEvery15Mins]
						(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
						 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
						 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
						)
						Select
							@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
							@drugitemid,@year,@discDate,@fcp,@qa_exempt,@status,null,0,
							user_name(),
							getdate(),
							user_name(),
							getdate()
					End			
				End
			End
		End
				
		Select 	@drugitemid = null,@drugItemNDCId = null, @contractid = null							 
		
		FETCH NEXT FROM NFAMP_Cursor_TEMP
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status
	End
	Close NFAMP_Cursor_TEMP
	DeAllocate NFAMP_Cursor_TEMP


-- For years greater than 2010 with PROV

	Declare NFAMP_Cursor_PROV CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,FCP,YearID,
		Case
			When disc_Date is not null and isdate(cast(disc_date as varchar(12))) = 1
				then DISC_Date
			else Null
		End	,
		Case
			When len(QA_Exempt) > 0 then QA_Exempt
			else Null
		End,status
		from NFAMP_STG
		where yearid > 2010
		and status = 'PROV'
--		Where (len(A) = 0 or A is null)
					
	Open NFAMP_Cursor_PROV
	FETCH NEXT FROM NFAMP_Cursor_PROV
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status


	WHILE @@FETCH_STATUS = 0
	BEGIN
		If @ndc_1 is not null And @ndc_2 is not null or @ndc_3 is not null
		Begin
			Select @contractId = ContractId
			From DI_Contracts
			Where ContractNumber = @cnt_no or NACCMContractNumber = @cnt_no 
			
			Select @drugItemNDCId = DrugItemNDCId
			From DI_DrugItemNDC
			Where FDAAssignedLabelerCode = @ndc_1
			And ProductCode = @ndc_2
			And PackageCode = @ndc_3	

			If @contractId is null 
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,0,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'ContractId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()		
			End
			Else If @drugItemNDCId is null
			Begin
				Insert into [DI_FCPEvery15Mins]
				(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
				 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
				 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
				)
				Select
					@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,0,
					0,@year,@discDate,@fcp,@qa_exempt,@status,'NDCId not found',0,
					user_name(),
					getdate(),
					user_name(),
					getdate()				
			End
			Else
			Begin
				Select @drugitemid = Drugitemid
				from DI_DrugItems where contractid = @contractId 
				and drugitemndcid = @drugItemNDCId 

				If @drugitemid is null 
				Begin
					Insert into [DI_FCPEvery15Mins]
					(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
					 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
					 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
					)
					Select
						@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
						0,@year,@discDate,@fcp,@qa_exempt,@status,'DrugItemId not found',0,
						user_name(),
						getdate(),
						user_name(),
						getdate()
				End
				Else 
				Begin
					If exists (select top 1 1 from [DI_FCPEvery15Mins] where DrugItemId = @drugitemid 
								and Status in ('PERM', '1PRM', 'NPS', 'TEMP') )
					Begin
						Select @errorMsg = 'Do Nothing'
					End
					Else
					Begin
						Insert into [DI_FCPEvery15Mins]
						(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
						 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
						 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
						)
						Select
							@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,
							@drugitemid,@year,@discDate,@fcp,@qa_exempt,@status,null,0,
							user_name(),
							getdate(),
							user_name(),
							getdate()
					End			
				End
			End
		End
				
		Select 	@drugitemid = null,@drugItemNDCId = null, @contractid = null							 
		
		FETCH NEXT FROM NFAMP_Cursor_PROV
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status
	End
	Close NFAMP_Cursor_PROV
	DeAllocate NFAMP_Cursor_PROV

	
-- For years greater than 2010 with NUll Status

	Declare NFAMP_Cursor_NULL CURSOR For
		Select NDC_1,NDC_2,NDC_3,N,CNT_NO,FCP,YearID,
		Case
			When disc_Date is not null and isdate(cast(disc_date as varchar(12))) = 1
				then DISC_Date
			else Null
		End	,
		Case
			When len(QA_Exempt) > 0 then QA_Exempt
			else Null
		End,status
		from NFAMP_STG
		where yearid > 2010
		and status is null
					
	Open NFAMP_Cursor_NULL
	FETCH NEXT FROM NFAMP_Cursor_NULL
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status


	WHILE @@FETCH_STATUS = 0
	BEGIN
		Insert into [DI_FCPEvery15Mins]
		(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,
		 YearId,Disc_Date,FCP,QA_Exempt,[Status],ErrorMsg,ModificationStatusId,
		 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
		)
		Select
			@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,0,0,
			0,@year,@discDate,@fcp,@qa_exempt,@status,'Status is Null',0,
			user_name(),
			getdate(),
			user_name(),
			getdate()
							 
		
		FETCH NEXT FROM NFAMP_Cursor_NULL
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@year,@discdate,@qa_exempt,@status
	End
	Close NFAMP_Cursor_NULL
	DeAllocate NFAMP_Cursor_NULL


-- Drop and rename FCP table

	If ((Select COUNT(*) from [DI_FCPEvery15Mins]) > 0) 
	Begin
		Drop table DI_FCP
		EXEC sp_rename 'DI_FCPEvery15Mins', 'DI_FCP'
		Exec sp_rename 'PK_DI_FCPEvery15Mins','PK_DI_FCP'
		CREATE NONCLUSTERED INDEX [IX_DI_FCP_DrugItemId] ON [dbo].[DI_FCP] 
		(
			[DrugItemId] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]


	End



-- Log FCP changes
	If exists(Select top 1 1 From DI_FCP 
				where yearid = YEAR(getdate())+1 
				and fcp > 0 
				and errormsg is null 
				and drugitemid > 0)
	Begin
		Declare FCPLOG_Cursor CURSOR For
			Select DrugItemId,FCP,YearId,ContractId
			from DI_FCP
			where yearid = YEAR(getdate())+1 
			and fcp > 0 
			and errormsg is null 
			and drugitemid > 0
					
		Open FCPLOG_Cursor
		FETCH NEXT FROM FCPLOG_Cursor
		INTO @drugitemid,@fcp,@yearid,@contractid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			Select @contractNumber = NACCMContractNumber 
			From DI_Contracts 
			Where ContractId = @contractid
		
		
			If exists (Select top 1 1 from DI_FCPLog 
						where DrugItemId = @drugitemid 
						and yearid = @yearid)
			Begin
				Select @creationdate = MAX(creationdate)
				From DI_FCPLog
				Where DrugItemId = @drugitemid 
				and yearid = @yearid
				
				Select @fcplogid = FCPLogId
				From DI_FCPLog
				Where DrugItemId = @drugitemid 
				and yearid = @yearid
				and creationdate = @creationdate
				
				Select @fcpold = FCP
				From  DI_FCPLog 
				Where FCPLogId = @fcplogid
				

				IF @fcpold <> @fcp
				Begin
					Insert into DI_FCPLog
					(ContractNumber,DrugItemId,FCP,YearId)			
					Select
						@contractNumber,@drugitemid,@fcp,@yearid					
				End
			End
			Else
			Begin
				Insert into DI_FCPLog
				(ContractNumber,DrugItemId,FCP,YearId)			
				Select
					@contractNumber,@drugitemid,@fcp,@yearid
			End
			
		
		
			FETCH NEXT FROM FCPLOG_Cursor
			INTO @drugitemid,@fcp,@yearid,@contractid
		End
		Close FCPLOG_Cursor
		DeAllocate FCPLOG_Cursor		
	End

End


