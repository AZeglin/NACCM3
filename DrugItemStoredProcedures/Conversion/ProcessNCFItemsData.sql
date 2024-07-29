IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProcessNCFItemsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ProcessNCFItemsData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[ProcessNCFItemsData]
As

Declare @cnt_no nvarchar(20),
	@contractId int,
	@drugItemNDCId int,
	@identity int,
	@error int,
	@errorMsg nvarchar(512),
	@ndc_1 char(5),
	@ndc_2 char(4),
	@ndc_3 char(2),
	@n char(1),
	@drugitemid int,
	@pkg nvarchar(14),
	@generic nvarchar(64),
	@trade_name nvarchar(45),
	@disc_date datetime,
	@disc_edat datetime,
	@ndc_link int,
	@cov nchar(1),
	@pv nchar(1),
	@pv_date datetime,
	@pt nchar(1),
	@dsp_unt nvarchar(10),
	@va_class nvarchar(5),
	@unit_sale nchar(2),
	@qty_u_sale decimal(5,0),
	@unit_pkg nchar(2),
	@qty_u_pkg decimal(13,5),
	@unit_meas nchar(2),
	@NDCWithNId int,
	@discontinuationDate datetime,
	@discontinuationEnteredDate datetime,
	@subitemid int,
	@pr_mult int,@pr_divide int
	

	Declare FSSData_Cursor CURSOR For
		Select a.NDC_1,a.NDC_2,a.NDC_3,a.n,a.ncf_cnt,
				pkg,generic,trade_name,disc_date,disc_edat,ndc_link,cov,pv,pv_date,
				pt,dsp_unt,va_class,unit_sale,qty_u_sale,unit_pkg,qty_u_pkg,unit_meas,
				cast(pr_mult as int),cast(pr_divide as int)
		From Ncfprice a join fssdata b
		on a.cnt_no = b.cnt_no
		and a.ndc_1 = b.ndc_1
		and a.ndc_2 = b.ndc_2
		and a.ndc_3 = b.ndc_3
		where (len(a.N) = 0 or a.N is null) and (len(b.N) = 0 or b.N is null)
		Order by a.ncf_cnt,a.NDC_1,a.NDC_2,a.NDC_3

	Open FSSData_Cursor
	FETCH NEXT FROM FSSData_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,
		 @pkg,@generic,@trade_name,@disc_date,@disc_edat,@ndc_link,@cov,@pv,@pv_date,
		 @pt,@dsp_unt,@va_class,@unit_sale,@qty_u_sale,@unit_pkg,@qty_u_pkg,@unit_meas,
		 @pr_mult,@pr_divide

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Select @drugitemid = Null,@contractid = Null, @drugItemNDCId = Null,@NDCWithNId = Null
		
		Select @contractId = ContractId
		From DI_Contracts
		Where ContractNumber = @cnt_no

		Select @drugItemNDCId = DrugItemNDCId
		From Di_DrugItemNDC
		Where FDAAssignedLabelerCode = @ndc_1
		And ProductCode = @ndc_2
		And PackageCode = @ndc_3

		IF @contractId is null or @drugItemNDCId is null 
		Begin
			Select @errorMsg = 'National Contract or DrugItemNDC Cannot be found'

			Insert into DI_ItemStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,ErrorMessage,CreatedBy,CreationDate)
			Select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@errorMsg,user_name(),getdate()
		End
		Else If exists (Select top 1 1 from DI_DrugItems where contractid = @contractId 
							and drugitemndcid = @drugItemNDCId 
						)
		Begin
				Select @errorMsg = 'Do Nothing'
		End
		Else
		Begin
			Insert into DI_DrugItems
			(ContractID,DrugItemNDCId,
			PackageDescription,Generic,TradeName,DiscontinuationDate,DiscontinuationEnteredDate,
			 Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,
			 DualPriceDesignation,ExcludeFromExport,LastModificationType,ModificationStatusId,
			 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
			)
			Select @contractId,	@drugItemNDCId,
				   @pkg,@generic,@trade_name,null,null,--,@disc_date,@disc_edat,
				   @cov,@pv,@pv_date,@pt,@dsp_unt,@va_class,
				   'F',0,'I',0,
				   'Re Extract',getdate(),user_name(),getdate()
					
			select @drugitemid = @@identity,@error = @@ERROR
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error when inserting into Items table for National Contracts'
				Insert into DI_ItemStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,discontinuationdate,ErrorMessage,CreatedBy,CreationDate)
				Select
					@cnt_no,@ndc_1,@ndc_2,@ndc_3,@disc_date,@errorMsg,user_name(),getdate()
			End
			Else
			Begin
				Insert into DI_ItemStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,n,discontinuationdate,drugitemid,
				 contractid,DrugitemNdcid,ErrorMessage,CreatedBy,CreationDate)
				Select
					@cnt_no,@ndc_1,@ndc_2,@ndc_3,null,@disc_date,@drugitemid,
					@contractId,@drugItemNDCId,'Inserted',user_name(),getdate()			
			
				Insert into DI_drugItemPackage
				(DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,
				 UnitOfMeasure,ModificationStatusId,PriceMultiplier,PriceDivider,CreatedBy,
				 CreationDate,LastModifiedBy,
				 LastModificationDate)
				Select 
					@drugitemid,@unit_sale,@qty_u_sale,@unit_pkg,@qty_u_pkg,
					@unit_meas,0,@pr_mult,@pr_divide,'Re Extract',getdate(),user_name(),getdate()

				select @error = @@ERROR
				if @error <> 0 
				Begin
					select @errorMsg = 'Error when inserting into Item Package table for National contracts'
					Insert into DI_ItemPackageStatus
					(ContractNumber,NDC_1,NDC_2,NDC_3,ErrorMessage,CreatedBy,CreationDate)
					Select
						@cnt_no,@ndc_1,@ndc_2,@ndc_3,@errorMsg,user_name(),getdate()
				End
			End
		End
		
		FETCH NEXT FROM FSSData_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,
			 @pkg,@generic,@trade_name,@disc_date,@disc_edat,@ndc_link,@cov,@pv,@pv_date,
			 @pt,@dsp_unt,@va_class,@unit_sale,@qty_u_sale,@unit_pkg,@qty_u_pkg,@unit_meas,
			 @pr_mult,@pr_divide
	End
	Close FSSData_Cursor
	DeAllocate FSSData_Cursor



	Declare FSSData_Cursor CURSOR For
		Select a.NDC_1,a.NDC_2,a.NDC_3,a.n,a.ncf_cnt,
				pkg,generic,trade_name,disc_date,disc_edat,ndc_link,cov,pv,pv_date,
				pt,dsp_unt,va_class,unit_sale,qty_u_sale,unit_pkg,qty_u_pkg,unit_meas,
				cast(pr_mult as int),cast(pr_divide as int)
		From Ncfprice a join fssdata b
		on a.cnt_no = b.cnt_no
		and a.ndc_1 = b.ndc_1
		and a.ndc_2 = b.ndc_2
		and a.ndc_3 = b.ndc_3
		and a.n = b.n
		where (len(a.N) > 0) and (len(b.N) > 0)
		Order by a.ncf_cnt,a.NDC_1,a.NDC_2,a.NDC_3

	Open FSSData_Cursor
	FETCH NEXT FROM FSSData_Cursor
	INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,
		 @pkg,@generic,@trade_name,@disc_date,@disc_edat,@ndc_link,@cov,@pv,@pv_date,
		 @pt,@dsp_unt,@va_class,@unit_sale,@qty_u_sale,@unit_pkg,@qty_u_pkg,@unit_meas,
		 @pr_mult,@pr_divide

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Select @drugitemid = Null,@contractid = Null, @drugItemNDCId = Null,
				@NDCWithNId = Null,@subitemid = null
		
		Select @contractId = ContractId
		From DI_Contracts
		Where ContractNumber = @cnt_no

		Select @drugItemNDCId = DrugItemNDCId
		From Di_DrugItemNDC
		Where FDAAssignedLabelerCode = @ndc_1
		And ProductCode = @ndc_2
		And PackageCode = @ndc_3

		IF @contractId is null or @drugItemNDCId is null 
		Begin
			Select @errorMsg = 'National Contract or DrugItemNDC Cannot be found'

			Insert into DI_ItemStatus
			(ContractNumber,NDC_1,NDC_2,NDC_3,n,ErrorMessage,CreatedBy,CreationDate)
			Select @cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@errorMsg,user_name(),getdate()
			
		End
		Else If exists (Select top 1 1 from DI_DrugItems where contractid = @contractId 
					and drugitemndcid = @drugItemNDCId 
				   )
		Begin
			Select @drugitemid = DrugItemID From DI_DrugItems 
			where contractid = @contractId 
			and drugitemndcid = @drugItemNDCId 

			If exists (Select top 1 1 from DI_DrugItemSubItems 
						where drugitemid = @drugitemid 
						and SubItemIdentifier = @n 
					  )
			Begin
				Select @errorMsg = 'Sub Item exists do nothing'
			End
			Else
			Begin			
				Insert into DI_DrugItemSubItems
					(DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,
					 LastModificationType,ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,
					 LastModificationDate)
				Select 
					@drugitemid,@n,@pkg,@generic,@trade_name,@dsp_unt,'I',0,'Re Extract',getdate(),user_name(),getdate()
					
				Select @subitemid = @@identity					
				Insert into DI_ItemStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,n,discontinuationdate,drugitemid,drugitemsubitemid,
				 contractid,DrugitemNdcid,ErrorMessage,CreatedBy,CreationDate)
				Select
					@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@disc_date,@drugitemid,@subitemid,
					@contractId,@drugItemNDCId,'Inserted',user_name(),getdate()					
			End				
		End
		Else
		Begin
			Insert into DI_DrugItems
			(ContractID,DrugItemNDCId,
			PackageDescription,Generic,TradeName,DiscontinuationDate,DiscontinuationEnteredDate,
			 Covered,PrimeVendor,PrimeVendorChangedDate,PassThrough,DispensingUnit,VAClass,
			 DualPriceDesignation,ExcludeFromExport,LastModificationType,ModificationStatusId,
			 CreatedBy,CreationDate,LastModifiedBy,LastModificationDate
			)
			Select @contractId,	@drugItemNDCId,					
				   @pkg,@generic,@trade_name,null,null,--,@disc_date,@disc_edat,
				   @cov,@pv,@pv_date,@pt,@dsp_unt,@va_class,
				   'F',0,'I',0,
				   'Re Extract',getdate(),user_name(),getdate()
					
			select @drugitemid = @@identity,@error = @@ERROR
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error when inserting into Items table for National Contracts'
				Insert into DI_ItemStatus
				(ContractNumber,NDC_1,NDC_2,NDC_3,n,discontinuationdate,ErrorMessage,CreatedBy,CreationDate)
				Select
					@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@disc_date,@errorMsg,user_name(),getdate()
			End
			Else
			Begin
				If exists (Select top 1 1 from DI_DrugItemSubItems 
							where drugitemid = @drugitemid 
							and SubItemIdentifier = @n 
						  )
				Begin
					Select @errorMsg = 'Sub Item exists do nothing'
				End
				Else
				Begin			
					Insert into DI_DrugItemSubItems
						(DrugItemId,SubItemIdentifier,PackageDescription,Generic,TradeName,DispensingUnit,
						 LastModificationType,ModificationStatusId,CreatedBy,CreationDate,LastModifiedBy,
						 LastModificationDate)
					Select 
						@drugitemid,@n,@pkg,@generic,@trade_name,@dsp_unt,'I',0,'Re Extract',getdate(),user_name(),getdate()
						
					Select @subitemid = @@identity					
					Insert into DI_ItemStatus
					(ContractNumber,NDC_1,NDC_2,NDC_3,n,discontinuationdate,drugitemid,drugitemsubitemid,
					 contractid,DrugitemNdcid,ErrorMessage,CreatedBy,CreationDate)
					Select
						@cnt_no,@ndc_1,@ndc_2,@ndc_3,@n,@disc_date,@drugitemid,@subitemid,
						@contractId,@drugItemNDCId,'Inserted',user_name(),getdate()						
				End	
			
				Insert into DI_drugItemPackage
				(DrugItemId,UnitOfSale,QuantityInUnitOfSale,UnitPackage,QuantityInUnitPackage,
				 UnitOfMeasure,ModificationStatusId,PriceMultiplier,PriceDivider,
				 CreatedBy,CreationDate,LastModifiedBy,
				 LastModificationDate)
				Select 
					@drugitemid,@unit_sale,@qty_u_sale,@unit_pkg,@qty_u_pkg,
					@unit_meas,0,@pr_mult,@pr_divide,'Re Extract',getdate(),user_name(),getdate()

				select @error = @@ERROR
				if @error <> 0 
				Begin
					select @errorMsg = 'Error when inserting into Item Package table for National Contracts'
					Insert into DI_ItemPackageStatus
					(ContractNumber,NDC_1,NDC_2,NDC_3,ErrorMessage,CreatedBy,CreationDate)
					Select
						@cnt_no,@ndc_1,@ndc_2,@ndc_3,@errorMsg,user_name(),getdate()
				End
			End
		End
		
		FETCH NEXT FROM FSSData_Cursor
		INTO @ndc_1,@ndc_2,@ndc_3,@n,@cnt_no,
			 @pkg,@generic,@trade_name,@disc_date,@disc_edat,@ndc_link,@cov,@pv,@pv_date,
			 @pt,@dsp_unt,@va_class,@unit_sale,@qty_u_sale,@unit_pkg,@qty_u_pkg,@unit_meas,
			 @pr_mult,@pr_divide
	End
	Close FSSData_Cursor
	DeAllocate FSSData_Cursor
