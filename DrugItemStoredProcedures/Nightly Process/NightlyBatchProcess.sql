IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'NightlyBatchProcess')
	BEGIN
		DROP  Procedure  NightlyBatchProcess
	END

GO

CREATE Procedure [dbo].[NightlyBatchProcess]
As
	Declare @val int

	insert into DI_DrugItemPriceHistory
	(DrugItemPriceId, DrugItemId, DrugItemSubItemId,HistoricalNValue, PriceId, PriceStartDate, PriceStopDate, Price, IsTemporary, IsFSS, IsBIG4,                                          	                  
		IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, 
		IsNIH, IsPHS, IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG,IsFHCC,
		AwardedFSSTrackingCustomerRatio,
		TrackingCustomerName,
		CurrentTrackingCustomerPrice,
		ExcludeFromExport,
		LastModificationType,
		ModificationStatusId,	
		Notes,
		CreatedBy,
		CreationDate,
		LastModifiedBy, 
		LastModificationDate 
	)
	Select 
		DrugItemPriceId, a.DrugItemId, DrugItemSubItemId,a.HistoricalNValue, 0, 
		PriceStartDate, GETDATE(), Price, IsTemporary, IsFSS, IsBIG4,                                          	                  
		IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, 
		IsNIH, IsPHS, IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG,IsFHCC,
		AwardedFSSTrackingCustomerRatio,
		TrackingCustomerName,
		CurrentTrackingCustomerPrice,
		a.ExcludeFromExport,
		a.LastModificationType,
		a.ModificationStatusId,	
		'Nightly batch process',
		a.CreatedBy,
		a.CreationDate,
		'dbo', 
		GETDATE()
	From DI_DrugItemPrice a
--	join DI_DrugItems b
--	on a.DrugItemId = b.drugitemid
	where datediff(day,getdate(),PriceStopDate)< 0
	union
	Select 
		DrugItemPriceId, b.DrugItemId, DrugItemSubItemId,a.HistoricalNValue, 0, PriceStartDate, 
		b.DiscontinuationDate-1 as pricestopdate, 
		Price, IsTemporary, IsFSS, IsBIG4,                                          	                  
		IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, 
		IsNIH, IsPHS, IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG,IsFHCC,
		AwardedFSSTrackingCustomerRatio,
		TrackingCustomerName,
		CurrentTrackingCustomerPrice,
		a.ExcludeFromExport,
		a.LastModificationType,
		a.ModificationStatusId,	
		'Nightly batch process',
		a.CreatedBy,
		a.CreationDate,
		'dbo', 
		GETDATE()
	From DI_DrugItemPrice a
	join DI_DrugItems b
	on a.DrugItemId = b.drugitemid
	where datediff(day,getdate(),DiscontinuationDate)<= 0	
	
	
	Delete a 
	From DI_DrugItemPrice a
	join DI_DrugItems b
	on a.DrugItemId = b.drugitemid
	where datediff(day,getdate(),DiscontinuationDate)<= 0
	
	Delete  a
	From DI_DrugItemPrice a
	where datediff(day,getdate(),a.PriceStopDate)< 0
	
	Exec @val = NightlyBatchProcessForFSSWithoutSubItem 
	If @val <> 0
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForFSSWithoutSubItem',
			'Failed',
			GETDATE()
	End
	Else
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForFSSWithoutSubItem',
			'Success',
			GETDATE()	
	End


	Exec @val = NightlyBatchProcessForFSSWithSubItem 
	If @val <> 0
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForFSSWithSubItem',
			'Failed',
			GETDATE()
	End
	Else
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForFSSWithSubItem',
			'Success',
			GETDATE()	
	End
	
	
	Exec @val = NightlyBatchProcessForBig4WithoutSubItem 
	If @val <> 0
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForBig4WithoutSubItem',
			'Failed',
			GETDATE()
	End
	Else
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForBig4WithoutSubItem',
			'Success',
			GETDATE()	
	End
	

	Exec @val = NightlyBatchProcessForBig4WithSubItem 
	If @val <> 0
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForBig4WithSubItem',
			'Failed',
			GETDATE()
	End
	Else
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForBig4WithSubItem',
			'Success',
			GETDATE()	
	End
	
	
	Exec @val = NightlyBatchProcessForRestrictedWithoutSubItem 
	If @val <> 0
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForRestrictedWithoutSubItem',
			'Failed',
			GETDATE()
	End
	Else
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForRestrictedWithoutSubItem',
			'Success',
			GETDATE()	
	End
	
	
	Exec @val = NightlyBatchProcessForRestrictedWithSubItem 
	If @val <> 0
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForRestrictedWithSubItem',
			'Failed',
			GETDATE()
	End
	Else
	Begin
		Insert into NightlyBatchProcessLog
		(StorecProc, Status, LastModificationDate)
		Select
			'NightlyBatchProcessForRestrictedWithSubItem',
			'Success',
			GETDATE()	
	End					