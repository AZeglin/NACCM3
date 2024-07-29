IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DrugItemTPRWithoutBasePriceReport' )
BEGIN
	DROP PROCEDURE DrugItemTPRWithoutBasePriceReport
END
GO

CREATE PROCEDURE DrugItemTPRWithoutBasePriceReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@ActiveFutureSelectionCriteria nchar(1),  -- F future, A active, B both future and active
@CoveredSelectionCriteria nchar(1) -- C Covered, N Non-covered, B both covered and non-covered
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertDrugItemUserActivity @ReportUserLoginId, 'R', 'TPRs Without Base Price Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END
	
		
	IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = '#DrugItemTPRWithoutBasePriceReport' ) 
	BEGIN
		DROP TABLE #DrugItemTPRWithoutBasePriceReport
	END
	
	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error dropping #DrugItemTPRWithoutBasePriceReport temp table'
		goto ERROREXIT
	END
	
	create table #DrugItemTPRWithoutBasePriceReport
	(
		ContractId int,
		ContractNumber nvarchar(50),
		DrugItemNDCId int,
		DrugItemId int,
		FdaAssignedLabelerCode char(5),    
		ProductCode char(4),       
		PackageCode char(2),          
		PackageDescription  nvarchar(14),        
		Generic nvarchar(64),   
		TradeName nvarchar(45),     
		DiscontinuationDate  datetime,                             	             
		DiscontinuationEnteredDate datetime,
		Covered nchar(1),        
		PriceId int,
		Price decimal(18,2), 
		PriceStartDate datetime,   
		PriceStopDate datetime,      
		IsTemporary bit,
		IsFSS bit,                                           	                  
		IsBIG4 bit,                                          	                  
		IsVA bit,                                            	                  
		IsBOP bit,                                           	                  
		IsCMOP bit,                                          	                  
		IsDOD bit,                                           	                  
		IsHHS bit,                                           	                  
		IsIHS bit,                                           	                  
		IsIHS2 bit,                                          	                  
		IsDIHS bit,                                          	                  
		IsNIH bit,                                           	                  
		IsPHS bit,                                           	                  
		IsSVH bit,                                           	                  
		IsSVH1 bit,                                          	                  
		IsSVH2 bit,                                          	                  
		IsTMOP bit,                                          	                  
		IsUSCG bit,
		IsFHCC bit,
		CreatedBy nvarchar(120),     
		CreationDate datetime,        
		LastModifiedBy nvarchar(120),             
		LastModificationDate datetime,
		TPRAlwaysHasBasePrice bit
	)

	select @error = @@error

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating #DrugItemTPRWithoutBasePriceReport temp table'
		goto ERROREXIT
	END

	/* active */
	if @ActiveFutureSelectionCriteria = 'A'
	BEGIN

		insert into #DrugItemTPRWithoutBasePriceReport
		(
			ContractId,
			ContractNumber,
			DrugItemNDCId,
			DrugItemId,
			FdaAssignedLabelerCode,    
			ProductCode,       
			PackageCode,          
			PackageDescription,        
			Generic,   
			TradeName,     
			DiscontinuationDate,                             	             
			DiscontinuationEnteredDate,
			Covered,        
			PriceId,
			Price, 
			PriceStartDate,   
			PriceStopDate,      
			IsTemporary,
			IsFSS,                                           	                  
			IsBIG4,                                          	                  
			IsVA,                                            	                  
			IsBOP,                                           	                  
			IsCMOP,                                          	                  
			IsDOD,                                           	                  
			IsHHS,                                           	                  
			IsIHS,                                           	                  
			IsIHS2,                                          	                  
			IsDIHS,                                          	                  
			IsNIH,                                           	                  
			IsPHS,                                           	                  
			IsSVH,                                           	                  
			IsSVH1,                                          	                  
			IsSVH2,                                          	                  
			IsTMOP,                                          	                  
			IsUSCG,
			IsFHCC,
			CreatedBy,     
			CreationDate,        
			LastModifiedBy,             
			LastModificationDate,
			TPRAlwaysHasBasePrice
		)
		select 
			c.ContractId,
			c.ContractNumber,
			n.DrugItemNDCId,
			i.DrugItemId,
			n.FdaAssignedLabelerCode,    
			n.ProductCode,       
			n.PackageCode,          
			i.PackageDescription,        
			i.Generic,   
			i.TradeName,     
			i.DiscontinuationDate,                             	             
			i.DiscontinuationEnteredDate,
			i.Covered,        
			p.PriceId,
			p.Price, 
			p.PriceStartDate,   
			p.PriceStopDate,      
			p.IsTemporary,
			p.IsFSS,                                           	                  
			p.IsBIG4,                                          	                  
			p.IsVA,                                            	                  
			p.IsBOP,                                           	                  
			p.IsCMOP,                                          	                  
			p.IsDOD,                                           	                  
			p.IsHHS,                                           	                  
			p.IsIHS,                                           	                  
			p.IsIHS2,                                          	                  
			p.IsDIHS,                                          	                  
			p.IsNIH,                                           	                  
			p.IsPHS,                                           	                  
			p.IsSVH,                                           	                  
			p.IsSVH1,                                          	                  
			p.IsSVH2,                                          	                  
			p.IsTMOP,                                          	                  
			p.IsUSCG,
			p.IsFHCC,
			p.CreatedBy,     
			p.CreationDate,        
			p.LastModifiedBy,             
			p.LastModificationDate,
			dbo.GetTPRAlwaysHasBasePriceFunction( p.DrugItemId,          
										p.PriceStartDate,         
										p.PriceStopDate,       
										p.IsTemporary,                                     	                  
										p.IsFSS,                                           	                  
										p.IsBIG4,                                          	                  
										p.IsVA,                                            	                  
										p.IsBOP,                                           	                  
										p.IsCMOP,                                          	                  
										p.IsDOD,                                           	                  
										p.IsHHS,                                           	                  
										p.IsIHS,                                           	                  
										p.IsIHS2,                                          	                  
										p.IsDIHS,                                          	                  
										p.IsNIH,                                           	                  
										p.IsPHS,                                           	                  
										p.IsSVH,                                           	                  
										p.IsSVH1,                                          	                  
										p.IsSVH2,                                          	                  
										p.IsTMOP,                                          	                  
										p.IsUSCG,
										p.IsFHCC ) as TPRAlwaysHasBasePrice
		from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
			join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
			join DI_Contracts c on i.ContractId = c.ContractId
		where getdate() between p.PriceStartDate and p.PriceStopDate
			and p.IsTemporary = 1

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting active TPRs into temp table.'
			goto ERROREXIT
		END

	END
	/* both */
	else if @ActiveFutureSelectionCriteria = 'B'
	BEGIN

		insert into #DrugItemTPRWithoutBasePriceReport
		(
			ContractId,
			ContractNumber,
			DrugItemNDCId,
			DrugItemId,
			FdaAssignedLabelerCode,    
			ProductCode,       
			PackageCode,          
			PackageDescription,        
			Generic,   
			TradeName,     
			DiscontinuationDate,                             	             
			DiscontinuationEnteredDate,
			Covered,        
			PriceId,
			Price, 
			PriceStartDate,   
			PriceStopDate,      
			IsTemporary,
			IsFSS,                                           	                  
			IsBIG4,                                          	                  
			IsVA,                                            	                  
			IsBOP,                                           	                  
			IsCMOP,                                          	                  
			IsDOD,                                           	                  
			IsHHS,                                           	                  
			IsIHS,                                           	                  
			IsIHS2,                                          	                  
			IsDIHS,                                          	                  
			IsNIH,                                           	                  
			IsPHS,                                           	                  
			IsSVH,                                           	                  
			IsSVH1,                                          	                  
			IsSVH2,                                          	                  
			IsTMOP,                                          	                  
			IsUSCG,
			IsFHCC,
			CreatedBy,     
			CreationDate,        
			LastModifiedBy,             
			LastModificationDate,
			TPRAlwaysHasBasePrice
		)
		select 
			c.ContractId,
			c.ContractNumber,
			n.DrugItemNDCId,
			i.DrugItemId,
			n.FdaAssignedLabelerCode,    
			n.ProductCode,       
			n.PackageCode,          
			i.PackageDescription,        
			i.Generic,   
			i.TradeName,     
			i.DiscontinuationDate,                             	             
			i.DiscontinuationEnteredDate,
			i.Covered,        
			p.PriceId,
			p.Price, 
			p.PriceStartDate,   
			p.PriceStopDate,      
			p.IsTemporary,
			p.IsFSS,                                           	                  
			p.IsBIG4,                                          	                  
			p.IsVA,                                            	                  
			p.IsBOP,                                           	                  
			p.IsCMOP,                                          	                  
			p.IsDOD,                                           	                  
			p.IsHHS,                                           	                  
			p.IsIHS,                                           	                  
			p.IsIHS2,                                          	                  
			p.IsDIHS,                                          	                  
			p.IsNIH,                                           	                  
			p.IsPHS,                                           	                  
			p.IsSVH,                                           	                  
			p.IsSVH1,                                          	                  
			p.IsSVH2,                                          	                  
			p.IsTMOP,                                          	                  
			p.IsUSCG,
			p.IsFHCC,
			p.CreatedBy,     
			p.CreationDate,        
			p.LastModifiedBy,             
			p.LastModificationDate,
			dbo.GetTPRAlwaysHasBasePriceFunction( p.DrugItemId,          
										p.PriceStartDate,         
										p.PriceStopDate,       
										p.IsTemporary,                                     	                  
										p.IsFSS,                                           	                  
										p.IsBIG4,                                          	                  
										p.IsVA,                                            	                  
										p.IsBOP,                                           	                  
										p.IsCMOP,                                          	                  
										p.IsDOD,                                           	                  
										p.IsHHS,                                           	                  
										p.IsIHS,                                           	                  
										p.IsIHS2,                                          	                  
										p.IsDIHS,                                          	                  
										p.IsNIH,                                           	                  
										p.IsPHS,                                           	                  
										p.IsSVH,                                           	                  
										p.IsSVH1,                                          	                  
										p.IsSVH2,                                          	                  
										p.IsTMOP,                                          	                  
										p.IsUSCG,
										p.IsFHCC ) as TPRAlwaysHasBasePrice
		from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
			join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
			join DI_Contracts c on i.ContractId = c.ContractId
		where ( getdate() between p.PriceStartDate and p.PriceStopDate
				or ( p.PriceStartDate > GETDATE() and p.PriceStopDate > GETDATE() ))
			and p.IsTemporary = 1

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting both active and future TPRs into temp table.'
			goto ERROREXIT
		END
	END
	/* future only */
	else
	BEGIN
		insert into #DrugItemTPRWithoutBasePriceReport
		(
			ContractId,
			ContractNumber,
			DrugItemNDCId,
			DrugItemId,
			FdaAssignedLabelerCode,    
			ProductCode,       
			PackageCode,          
			PackageDescription,        
			Generic,   
			TradeName,     
			DiscontinuationDate,                             	             
			DiscontinuationEnteredDate,
			Covered,        
			PriceId,
			Price, 
			PriceStartDate,   
			PriceStopDate,      
			IsTemporary,
			IsFSS,                                           	                  
			IsBIG4,                                          	                  
			IsVA,                                            	                  
			IsBOP,                                           	                  
			IsCMOP,                                          	                  
			IsDOD,                                           	                  
			IsHHS,                                           	                  
			IsIHS,                                           	                  
			IsIHS2,                                          	                  
			IsDIHS,                                          	                  
			IsNIH,                                           	                  
			IsPHS,                                           	                  
			IsSVH,                                           	                  
			IsSVH1,                                          	                  
			IsSVH2,                                          	                  
			IsTMOP,                                          	                  
			IsUSCG,
			IsFHCC,
			CreatedBy,     
			CreationDate,        
			LastModifiedBy,             
			LastModificationDate,
			TPRAlwaysHasBasePrice
		)
		select 
			c.ContractId,
			c.ContractNumber,
			n.DrugItemNDCId,
			i.DrugItemId,
			n.FdaAssignedLabelerCode,    
			n.ProductCode,       
			n.PackageCode,          
			i.PackageDescription,        
			i.Generic,   
			i.TradeName,     
			i.DiscontinuationDate,                             	             
			i.DiscontinuationEnteredDate,
			i.Covered,        
			p.PriceId,
			p.Price, 
			p.PriceStartDate,   
			p.PriceStopDate,      
			p.IsTemporary,
			p.IsFSS,                                           	                  
			p.IsBIG4,                                          	                  
			p.IsVA,                                            	                  
			p.IsBOP,                                           	                  
			p.IsCMOP,                                          	                  
			p.IsDOD,                                           	                  
			p.IsHHS,                                           	                  
			p.IsIHS,                                           	                  
			p.IsIHS2,                                          	                  
			p.IsDIHS,                                          	                  
			p.IsNIH,                                           	                  
			p.IsPHS,                                           	                  
			p.IsSVH,                                           	                  
			p.IsSVH1,                                          	                  
			p.IsSVH2,                                          	                  
			p.IsTMOP,                                          	                  
			p.IsUSCG,
			p.IsFHCC,
			p.CreatedBy,     
			p.CreationDate,        
			p.LastModifiedBy,             
			p.LastModificationDate,
			dbo.GetTPRAlwaysHasBasePriceFunction( p.DrugItemId,          
										p.PriceStartDate,         
										p.PriceStopDate,       
										p.IsTemporary,                                     	                  
										p.IsFSS,                                           	                  
										p.IsBIG4,                                          	                  
										p.IsVA,                                            	                  
										p.IsBOP,                                           	                  
										p.IsCMOP,                                          	                  
										p.IsDOD,                                           	                  
										p.IsHHS,                                           	                  
										p.IsIHS,                                           	                  
										p.IsIHS2,                                          	                  
										p.IsDIHS,                                          	                  
										p.IsNIH,                                           	                  
										p.IsPHS,                                           	                  
										p.IsSVH,                                           	                  
										p.IsSVH1,                                          	                  
										p.IsSVH2,                                          	                  
										p.IsTMOP,                                          	                  
										p.IsUSCG,
										p.IsFHCC ) as TPRAlwaysHasBasePrice
		from DI_DrugItemPrice p join DI_DrugItems i on p.DrugItemId = i.DrugItemId
			join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
			join DI_Contracts c on i.ContractId = c.ContractId
		where  p.PriceStartDate > GETDATE() and p.PriceStopDate > GETDATE() 
			and p.IsTemporary = 1

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting future TPRs into temp table.'
			goto ERROREXIT
		END

	END

	-- filter covered/non-covered as requested
	if @CoveredSelectionCriteria = 'C'
	BEGIN
		delete #DrugItemTPRWithoutBasePriceReport where Covered = 'F'
		
		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error filtering for covered only.'
			goto ERROREXIT
		END

	END
	else if @CoveredSelectionCriteria = 'N'
	BEGIN
		delete #DrugItemTPRWithoutBasePriceReport where Covered = 'T'

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error filtering for non-covered only.'
			goto ERROREXIT
		END
	END

	select 	ContractId,
		ContractNumber,
		DrugItemNDCId,
		DrugItemId,
		FdaAssignedLabelerCode,    
		ProductCode,       
		PackageCode,          
		PackageDescription,        
		Generic,   
		TradeName,     
		DiscontinuationDate,                             	             
		DiscontinuationEnteredDate,
		Covered,        
		PriceId,
		Price, 
		PriceStartDate,   
		PriceStopDate,      
		IsTemporary,
		IsFSS,                                           	                  
		IsBIG4,                                          	                  
		IsVA,                                            	                  
		IsBOP,                                           	                  
		IsCMOP,                                          	                  
		IsDOD,                                           	                  
		IsHHS,                                           	                  
		IsIHS,                                           	                  
		IsIHS2,                                          	                  
		IsDIHS,                                          	                  
		IsNIH,                                           	                  
		IsPHS,                                           	                  
		IsSVH,                                           	                  
		IsSVH1,                                          	                  
		IsSVH2,                                          	                  
		IsTMOP,                                          	                  
		IsUSCG,
		IsFHCC,
		CreatedBy,     
		CreationDate,        
		LastModifiedBy,             
		LastModificationDate,
		TPRAlwaysHasBasePrice
	from #DrugItemTPRWithoutBasePriceReport
	where TPRAlwaysHasBasePrice = 0
	order by ContractNumber, FdaAssignedLabelerCode, ProductCode, PackageCode, PriceStartDate
	
goto OKEXIT

ERROREXIT:

	raiserror( @errorMsg, 16, 1 )
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )


