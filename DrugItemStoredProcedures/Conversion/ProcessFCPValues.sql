IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessFCPValues]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessFCPValues]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[ProcessFCPValues]
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
	@fcp decimal(9,2),
	@Year int,
	@YearId int,
	@stgid int
	
	

	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DI_FCP]') AND type in (N'U'))
	Begin
		DROP TABLE [dbo].[DI_FCP]
	End

	CREATE TABLE [dbo].[DI_FCP](
		[FCP_Id] [int] IDENTITY(1,1) NOT NULL,
		[ndc_1] [char](5) NOT NULL,
		[ndc_2] [char](4) NOT NULL,
		[ndc_3] [char](2) NOT NULL,
		[n] [char](1) NULL,
		[cnt_no] [char](11) NOT NULL,
		[FCP] [decimal](9, 2) NOT NULL,
		[YearId] [int] NOT NULL,
		[ContractId] [int] NULL,
		[DrugItemId] [int] NULL,
		[DrugItemNDCId] [int] NULL,
		[ErrorMsg] [nvarchar](1000) NULL,
		[ModificationStatusId] [int] NOT NULL,
		[CreatedBy] [nvarchar](120) NOT NULL,
		[CreationDate] [datetime] NOT NULL,
		[LastModifiedBy] [nvarchar](120) NOT NULL,
		[LastModificationDate] [datetime] NOT NULL,
	 CONSTRAINT [PK_DI_FCP] PRIMARY KEY CLUSTERED 
	(
		[FCP_Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	

	Declare NFAMP_Cursor CURSOR For
		Select ndc_1,ndc_2,ndc_3,n,cnt_no,fcp,[Year],ContractId,DrugItemId,DrugItemNDCId,ErrorMsg
		from Nfamp_STG	

	Open NFAMP_Cursor
	FETCH NEXT FROM NFAMP_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@Year,@contractid,@drugitemid,@drugItemNDCId,@errorMsg

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Select @YearId = null

		Select @YearId = YearId
		From DI_yearLookUp
		Where YearValue  = @Year
		
		If @YearId is null
		Begin
			Insert into DI_fcpstatus
			(cnt_no,ndc_1,ndc_2,ndc_3,n,ErrorMessage,CreatedBy,CreationDate)
			select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,'YearId is null',USER_NAME(),GETDATE()
		End
		Else
		begin
		
			Insert into DI_FCP
			(NDC_1,NDC_2,NDC_3,N,CNT_NO,ContractId,DrugItemNDCId,DrugItemId,YearId,FCP,ErrorMsg,ModificationStatusId,CreatedBy,CreationDate,
				LastModifiedBy,LastModificationDate
			)
			Select
				@ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@contractid,@drugItemNDCId,@drugitemid,@YearId,@fcp,@errorMsg,0,
				user_name(),
				getdate(),
				user_name(),
				getdate()
		end
				
		FETCH NEXT FROM NFAMP_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,@fcp,@Year,@contractid,@drugitemid,@drugItemNDCId,@errorMsg
	End
	Close NFAMP_Cursor
	DeAllocate NFAMP_Cursor




