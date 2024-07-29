IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectMedSurgItemsForContract' )
BEGIN
	DROP PROCEDURE SelectMedSurgItemsForContract
END
GO

CREATE PROCEDURE SelectMedSurgItemsForContract
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@ContractId int,
@WithAdd bit = 0,
@ItemSelectionCriteria nchar(1) = 'A',    -- A = Active,  H = Historical
@SearchText nvarchar(50),
@IsBPA bit = 0,
@IsService bit = 0,
@ParentContractId int = -1,
@StartRow int = 1,   -- these are required for paging
@PageSize int = 10
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),	
		@query nvarchar(max),	
		@query1 nvarchar(max),
		@query1a nvarchar(max),
		@query1b nvarchar(max),
		@query2 nvarchar(max),
		@SQLParms nvarchar(2200),
		@orderBy nvarchar(200),
		@ItemId int, 
		@CatalogNumber nvarchar(70), 
	
		@ManufacturersCatalogNumber nvarchar(100),
		@ManufacturersName nvarchar(100),
		@LetterOfCommitmentDate datetime,
		@CommercialListPrice decimal(10,2),
		@CommercialPricelistDate datetime,
		@CommercialPricelistFOBTerms nvarchar(40),
		@ManufacturersCommercialListPrice decimal(10,2),
		@TrackingMechanism	nvarchar(100),
		@AcquisitionCost decimal(10,2),
		@TypeOfContractor nvarchar(100),


		@ItemDescription nvarchar(800), 
		@SIN nvarchar(50), 
		@ServiceCategoryId int,
		@PackageAsPriced nvarchar(2), 
		@CurrentPrice nvarchar(20), -- decimal(18,2),
		@PriceStartDate nvarchar(10),
		@PriceStopDate nvarchar(10),
		@HasBPA bit,
		@ParentItemId int, 
		@ParentCatalogNumber nvarchar(70),

		@ParentManufacturersCatalogNumber nvarchar(100),
		@ParentManufacturersName nvarchar(100),
		@ParentLetterOfCommitmentDate datetime,
		@ParentCommercialListPrice decimal(10,2),
		@ParentCommercialPricelistDate datetime,
		@ParentCommercialPricelistFOBTerms nvarchar(40),
		@ParentManufacturersCommercialListPrice decimal(10,2),		
		@ParentTrackingMechanism	nvarchar(100),
		@ParentAcquisitionCost decimal(10,2),
		@ParentTypeOfContractor nvarchar(100),

		@ParentItemDescription nvarchar(800), 
		@ParentActive bit,
		@ParentHistorical bit,
		@ItemHistoryId int,
		@LastModificationType nchar(1), 
		@ModificationStatusId int, 
		@LastModifiedBy nvarchar(120), 
		@LastModificationDate datetime,
		@ReasonMovedToHistory nvarchar(30),
		@MovedToHistoryBy nvarchar(120),
		@DateMovedToHistory datetime,
		@RowNumber int,
		@ReverseRowNumber int,
		@EndRow int,
		@searchWhere nvarchar(600),
		@Restorable bit


BEGIN TRANSACTION

-- init
select @query = '', @query1 = '', @query1a = '', @query1b = '', @query2 = ''

-- set up for paging
if @WithAdd = 0
BEGIN
	select @EndRow = @StartRow + ( @PageSize - 1 )
END
else
BEGIN
	select @EndRow = @StartRow + ( @PageSize - 2 ) 
END

-- note this must match order by of the item insert SP	
if @IsBPA = 1
BEGIN
	select @orderBy = ' order by x.ParentCatalogNumber asc '  
END
else if @IsService = 1
BEGIN
	select @orderBy = ' order by x.ItemDescription asc '
END
else -- fss or national 
BEGIN
	select @orderBy = ' order by CatalogNumber asc '
END

if @SearchText is not null
BEGIN
	if LEN(LTRIM(RTRIM(@SearchText))) > 0
	BEGIN
		-- searching should ignore page number
		--select @StartRow = 1


		if @ItemSelectionCriteria = 'A'
		BEGIN
			if @IsBPA = 0
			BEGIN
			-- manuf pt no moving to details
			--	select @searchWhere = ' and ( i.CatalogNumber like ''%' + @SearchText + '%'' or i.ManufacturersPartNumber like ''%' + @SearchText + '%'' or i.ItemDescription like ''%' + @SearchText + '%'' ) '
			select @searchWhere = ' and ( i.CatalogNumber like ''%' + @SearchText + '%'' or i.ItemDescription like ''%' + @SearchText + '%'' ) '
			END
			else
			BEGIN
			--	select @searchWhere = ' and (( d.ItemId is not null and ( d.CatalogNumber like ''%' + @SearchText + '%'' or d.ManufacturersPartNumber like ''%' + @SearchText + '%'' or d.ItemDescription like ''%' + @SearchText + '%'' ))
			--							or ( e.ItemId is not null and  ( e.CatalogNumber like ''%' + @SearchText + '%''or e.ManufacturersPartNumber like ''%' + @SearchText + '%''  or e.ItemDescription like ''%' + @SearchText + '%'' ))) '
				select @searchWhere = ' and (( d.ItemId is not null and ( d.CatalogNumber like ''%' + @SearchText + '%'' or d.ItemDescription like ''%' + @SearchText + '%'' ))
										or ( e.ItemId is not null and  ( e.CatalogNumber like ''%' + @SearchText + '%'' or e.ItemDescription like ''%' + @SearchText + '%'' ))) '


			END

		END
		else
		BEGIN
			--select @searchWhere = ' and ( h.CatalogNumber like ''%' + @SearchText + '%'' or h.ManufacturersPartNumber like ''%' + @SearchText + '%'' or h.ItemDescription like ''%' + @SearchText + '%'' ) '
			select @searchWhere = ' and ( h.CatalogNumber like ''%' + @SearchText + '%'' or h.ItemDescription like ''%' + @SearchText + '%'' ) '
		END
	END
	else
	BEGIN
		select @searchWhere = ''
	END
END
else
BEGIN
	select @searchWhere = ''
END

-- mod for first release: take out ManufacturersPartNumber  and add new fields
/*
    [ManufacturersCatalogNumber]					nvarchar(100)		NULL,
	[ManufacturersName]								nvarchar(100)		NULL,
	[LetterOfCommitmentDate]						datetime			 NULL,
	[CommercialListPrice]							decimal(10,2)        NULL,
	[CommercialPricelistDate]						datetime			NULL,
	[CommercialPricelistFOBTerms]					nvarchar(40)		NULL,
	[ManufacturersCommercialListPrice]				decimal(10,2)        NULL,
	[TrackingMechanism]								nvarchar(100)		NULL,
	[AcquisitionCost]								decimal(10,2)		NULL,
	[TypeOfContractor]								nvarchar(100)		NULL,


	ManufacturersCatalogNumber, ManufacturersName, LetterOfCommitmentDate, CommercialListPrice, CommercialPricelistDate, CommercialPricelistFOBTerms, ManufacturersCommercialListPrice, TrackingMechanism, AcquisitionCost, TypeOfContractor, 

	ParentManufacturersCatalogNumber, ParentManufacturersName, ParentLetterOfCommitmentDate, ParentCommercialListPrice, ParentCommercialPricelistDate, ParentCommercialPricelistFOBTerms, ParentManufacturersCommercialListPrice, ParentTrackingMechanism, ParentAcquisitionCost, ParentTypeOfContractor, 

	i.ManufacturersCatalogNumber, 
	i.ManufacturersName, 
	i.LetterOfCommitmentDate, 
	i.CommercialListPrice, 
	i.CommercialPricelistDate, 
	i.CommercialPricelistFOBTerms, 
	i.ManufacturersCommercialListPrice, 
	i.TrackingMechanism, 
	i.AcquisitionCost, 
	i.TypeOfContractor, 


	'''' as ParentManufacturersCatalogNumber, 
	'''' as ParentManufacturersName, 
	'''' as ParentLetterOfCommitmentDate, 
	0 as ParentCommercialListPrice, 
	'''' as ParentCommercialPricelistDate, 
	'''' as ParentCommercialPricelistFOBTerms, 
	0 as ParentManufacturersCommercialListPrice, 
	'''' as ParentTrackingMechanism, 
	0 as ParentAcquisitionCost, 
	'''' as ParentTypeOfContractor
*/

		
/* Active */
if @ItemSelectionCriteria = 'A'
BEGIN
	if @WithAdd = 0
	BEGIN
		if @IsBPA = 0
		BEGIN
			if @IsService = 0
			BEGIN
				
				select @query1 = 'select x.ItemId, x.CatalogNumber, x.ManufacturersCatalogNumber, x.ManufacturersName, x.LetterOfCommitmentDate, x.CommercialListPrice, x.CommercialPricelistDate, x.CommercialPricelistFOBTerms, x.ManufacturersCommercialListPrice, x.TrackingMechanism, x.AcquisitionCost, x.TypeOfContractor, 
					x.ItemDescription, x.[SIN], x.ServiceCategoryId, x.PackageAsPriced, x.CurrentPrice, x.PriceStartDate, x.PriceStopDate, x.HasBPA,
					x.ParentItemId, x.ParentCatalogNumber, 	x.ParentManufacturersCatalogNumber, x.ParentManufacturersName, x.ParentLetterOfCommitmentDate, x.ParentCommercialListPrice, x.ParentCommercialPricelistDate, x.ParentCommercialPricelistFOBTerms, x.ParentManufacturersCommercialListPrice, x.ParentTrackingMechanism, x.ParentAcquisitionCost, x.ParentTypeOfContractor, 
					x.ParentItemDescription, x.ParentActive,  x.ParentHistorical, x.ItemHistoryId,  x.LastModificationType,  x.ModificationStatusId, x.LastModifiedBy, x.LastModificationDate,
					x.ReasonMovedToHistory, x.MovedToHistoryBy, x.DateMovedToHistory, x.Restorable,
					RowNumber + ReverseRowNumber - 1 as TotalRows
					from (
						select i.ItemId, 
						i.CatalogNumber, 
						i.ManufacturersCatalogNumber, 
						i.ManufacturersName, 
						i.LetterOfCommitmentDate, 
						i.CommercialListPrice, 
						i.CommercialPricelistDate, 
						i.CommercialPricelistFOBTerms, 
						i.ManufacturersCommercialListPrice, 
						i.TrackingMechanism, 
						i.AcquisitionCost, 
						i.TypeOfContractor, 
						i.ItemDescription, 
						i.[SIN], 
						isnull( i.ServiceCategoryId, -1 ) as ServiceCategoryId,
						i.PackageAsPriced, 
						( select convert( nvarchar(20), isnull( p.Price, '''' )) from CM_ItemPrice p where p.ItemId = i.ItemId 
																					and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																					and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																					and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as CurrentPrice,
						( select isnull( convert( nvarchar(10),  p.PriceStartDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ItemId 
																					and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																					and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																					and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as PriceStartDate,
						( select isnull( convert( nvarchar(10),  p.PriceStopDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ItemId 
																					and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																					and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																					and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as PriceStopDate,
						case when exists ( select b.ItemId from CM_Items b join tbl_Cntrcts c on b.ContractId = c.Contract_Record_ID
											join CM_BPALookup u on u.BPAContractNumber = c.CntrctNum 
											where b.ParentItemId = i.ItemId ) then 1 else 0 end as HasBPA,	'
			select @query1a = '																	
						-1 as ParentItemId, 
						'''' as ParentCatalogNumber,						
						'''' as ParentManufacturersCatalogNumber, 
						'''' as ParentManufacturersName, 
						'''' as ParentLetterOfCommitmentDate, 
						0 as ParentCommercialListPrice, 
						'''' as ParentCommercialPricelistDate, 
						'''' as ParentCommercialPricelistFOBTerms, 
						0 as ParentManufacturersCommercialListPrice, 
						'''' as ParentTrackingMechanism, 
						0 as ParentAcquisitionCost, 
						'''' as ParentTypeOfContractor, 
						'''' as ParentItemDescription,
						0 as ParentActive,
						0 as ParentHistorical,
						-1 as ItemHistoryId,
						i.LastModificationType, 
						i.ModificationStatusId, 
						i.LastModifiedBy, 
						i.LastModificationDate,
						'''' as ReasonMovedToHistory,
						'''' as MovedToHistoryBy,
						null as DateMovedToHistory,
						0 as Restorable,
						ROW_NUMBER() OVER ( ORDER BY CatalogNumber ) as RowNumber,
						ROW_NUMBER() OVER ( ORDER BY CatalogNumber desc ) as ReverseRowNumber 
					from CM_Items i 
					where ContractId = @ContractId_parm ' + @searchWhere +									
				'	) x
				where x.RowNumber between @StartRow_parm and @EndRow_parm '
						
			
				select @error = @@error
	
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error assigning query string when retrieving FSS/National items for contract ' + @ContractNumber
					goto ERROREXIT
				END
			END
			else -- service
			BEGIN
				select @query1 = 'select x.ItemId, x.CatalogNumber, x.ManufacturersCatalogNumber, x.ManufacturersName, x.LetterOfCommitmentDate, x.CommercialListPrice, x.CommercialPricelistDate, x.CommercialPricelistFOBTerms, x.ManufacturersCommercialListPrice, x.TrackingMechanism, x.AcquisitionCost, x.TypeOfContractor,
					x.ItemDescription, x.[SIN], x.ServiceCategoryId, x.PackageAsPriced, x.CurrentPrice, x.PriceStartDate, x.PriceStopDate, x.HasBPA,
					x.ParentItemId, x.ParentCatalogNumber, x.ParentManufacturersCatalogNumber, x.ParentManufacturersName, x.ParentLetterOfCommitmentDate, x.ParentCommercialListPrice, x.ParentCommercialPricelistDate, x.ParentCommercialPricelistFOBTerms, x.ParentManufacturersCommercialListPrice, x.ParentTrackingMechanism, x.ParentAcquisitionCost, x.ParentTypeOfContractor, 
					x.ParentItemDescription, x.ParentActive,  x.ParentHistorical,  x.ItemHistoryId,  x.LastModificationType,  x.ModificationStatusId, x.LastModifiedBy, x.LastModificationDate,
					x.ReasonMovedToHistory, x.MovedToHistoryBy, x.DateMovedToHistory, x.Restorable,
					RowNumber + ReverseRowNumber - 1 as TotalRows
					from (
					select i.ItemId, 
					i.CatalogNumber,  
					i.ManufacturersCatalogNumber, 
					i.ManufacturersName, 
					i.LetterOfCommitmentDate, 
					i.CommercialListPrice, 
					i.CommercialPricelistDate, 
					i.CommercialPricelistFOBTerms, 
					i.ManufacturersCommercialListPrice, 
					i.TrackingMechanism, 
					i.AcquisitionCost, 
					i.TypeOfContractor, 
					i.ItemDescription, 
					i.[SIN], 
					isnull( i.ServiceCategoryId, -1 ) as ServiceCategoryId,
					i.PackageAsPriced, 
					( select convert( nvarchar(20), isnull( p.Price, '''' )) from CM_ItemPrice p where p.ItemId = i.ItemId 
															and datediff( DD, p.PriceStartDate, getdate() ) >= 0
															and datediff( DD, getdate(), p.PriceStopDate ) >= 0
															and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as CurrentPrice,
					( select isnull( convert( nvarchar(10),  p.PriceStartDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ItemId 
																				and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																				and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																				and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as PriceStartDate,
					( select isnull( convert( nvarchar(10),  p.PriceStopDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ItemId 
																				and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																				and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																				and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as PriceStopDate,	
					case when exists ( select b.ItemId from CM_Items b join tbl_Cntrcts c on b.ContractId = c.Contract_Record_ID
									join CM_BPALookup u on u.BPAContractNumber = c.CntrctNum 
									where b.ParentItemId = i.ItemId ) then 1 else 0 end as HasBPA,	'
			select @query1a = '															
					-1 as ParentItemId, 
					'''' as ParentCatalogNumber,
					'''' as ParentManufacturersCatalogNumber, 
					'''' as ParentManufacturersName, 
					'''' as ParentLetterOfCommitmentDate, 
					0 as ParentCommercialListPrice, 
					'''' as ParentCommercialPricelistDate, 
					'''' as ParentCommercialPricelistFOBTerms, 
					0 as ParentManufacturersCommercialListPrice, 
					'''' as ParentTrackingMechanism, 
					0 as ParentAcquisitionCost, 
					'''' as ParentTypeOfContractor,
					'''' as ParentItemDescription,
					0 as ParentActive,
					0 as ParentHistorical,
					-1 as ItemHistoryId,
					i.LastModificationType, 
					i.ModificationStatusId, 
					i.LastModifiedBy, 
					i.LastModificationDate,
					'''' as ReasonMovedToHistory,
					'''' as MovedToHistoryBy,
					null as DateMovedToHistory,
					0 as Restorable,
					ROW_NUMBER() OVER ( ORDER BY ServiceCategoryId   ) as RowNumber,
					ROW_NUMBER() OVER ( ORDER BY ServiceCategoryId desc   ) as ReverseRowNumber 
				from CM_Items i
				where ContractId = @ContractId_parm ' + @searchWhere +									
				'	) x
				where x.RowNumber between @StartRow_parm and @EndRow_parm '							

			select @error = @@error
	
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error assigning query string when retrieving service items for contract ' + @ContractNumber
				goto ERROREXIT
			END

			END
		END
		else -- for BPA the price is the parent price
		BEGIN
			select @query1 = 'select x.ItemId, x.CatalogNumber, x.ManufacturersCatalogNumber, x.ManufacturersName, x.LetterOfCommitmentDate, x.CommercialListPrice, x.CommercialPricelistDate, x.CommercialPricelistFOBTerms, x.ManufacturersCommercialListPrice, x.TrackingMechanism, x.AcquisitionCost, x.TypeOfContractor, 
				x.ItemDescription, x.[SIN], x.ServiceCategoryId, x.PackageAsPriced, x.CurrentPrice, x.PriceStartDate, x.PriceStopDate, x.HasBPA,
				x.ParentItemId, x.ParentCatalogNumber, x.ParentManufacturersCatalogNumber, x.ParentManufacturersName, x.ParentLetterOfCommitmentDate, x.ParentCommercialListPrice, x.ParentCommercialPricelistDate, x.ParentCommercialPricelistFOBTerms, x.ParentManufacturersCommercialListPrice, x.ParentTrackingMechanism, x.ParentAcquisitionCost, x.ParentTypeOfContractor, 
				x.ParentItemDescription, x.ParentActive,  x.ParentHistorical, x.ItemHistoryId,  x.LastModificationType,  x.ModificationStatusId, x.LastModifiedBy, x.LastModificationDate,
				x.ReasonMovedToHistory, x.MovedToHistoryBy, x.DateMovedToHistory, x.Restorable,
				RowNumber + ReverseRowNumber - 1 as TotalRows
				from (
					select i.ItemId, 
					i.CatalogNumber,  
					i.ManufacturersCatalogNumber, 
					i.ManufacturersName, 
					i.LetterOfCommitmentDate, 
					i.CommercialListPrice, 
					i.CommercialPricelistDate, 
					i.CommercialPricelistFOBTerms, 
					i.ManufacturersCommercialListPrice, 
					i.TrackingMechanism, 
					i.AcquisitionCost, 
					i.TypeOfContractor, 
					i.ItemDescription, 
					i.[SIN], 
					isnull( i.ServiceCategoryId, -1 ) as ServiceCategoryId,
					i.PackageAsPriced, 
					( select convert( nvarchar(20), isnull( p.Price, '''' )) from CM_ItemPrice p where p.ItemId = i.ParentItemId 
																and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ParentItemId ) ) as CurrentPrice,
					( select isnull( convert( nvarchar(10),  p.PriceStartDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ParentItemId 
																				and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																				and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																				and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ParentItemId ) ) as PriceStartDate,
					( select isnull( convert( nvarchar(10),  p.PriceStopDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ParentItemId 
																				and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																				and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																				and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ParentItemId ) ) as PriceStopDate,			
					0 as HasBPA,	'

			select @query1a = '	case when d.ItemId is not null then d.ItemId else
						case when e.ItemId is not null then e.ItemId else
						-1 end end as ParentItemId,
					
					case when d.CatalogNumber is not null then d.CatalogNumber else
						case when e.CatalogNumber is not null then e.CatalogNumber else
						'''' end end as ParentCatalogNumber,

					case when d.ManufacturersCatalogNumber is not null then d.ManufacturersCatalogNumber else
						case when e.ManufacturersCatalogNumber is not null then e.ManufacturersCatalogNumber else
						'''' end end as ParentManufacturersCatalogNumber,

					case when d.ManufacturersName is not null then d.ManufacturersName else
						case when e.ManufacturersName is not null then e.ManufacturersName else
						'''' end end as ParentManufacturersName,

					case when d.LetterOfCommitmentDate is not null then d.LetterOfCommitmentDate else
						case when e.LetterOfCommitmentDate is not null then e.LetterOfCommitmentDate else
						'''' end end as ParentLetterOfCommitmentDate,

					case when d.CommercialListPrice is not null then d.CommercialListPrice else
						case when e.CommercialListPrice is not null then e.CommercialListPrice else
						0 end end as ParentCommercialListPrice,

					case when d.CommercialPricelistDate is not null then d.CommercialPricelistDate else
						case when e.CommercialPricelistDate is not null then e.CommercialPricelistDate else
						'''' end end as ParentCommercialPricelistDate,

					case when d.CommercialPricelistFOBTerms is not null then d.CommercialPricelistFOBTerms else
						case when e.CommercialPricelistFOBTerms is not null then e.CommercialPricelistFOBTerms else
						'''' end end as ParentCommercialPricelistFOBTerms,

					case when d.ManufacturersCommercialListPrice is not null then d.ManufacturersCommercialListPrice else
						case when e.ManufacturersCommercialListPrice is not null then e.ManufacturersCommercialListPrice else
						0 end end as ParentManufacturersCommercialListPrice,
						
					case when d.TrackingMechanism is not null then d.TrackingMechanism else
						case when e.TrackingMechanism is not null then e.TrackingMechanism else
						'''' end end as ParentTrackingMechanism,

					case when d.AcquisitionCost is not null then d.AcquisitionCost else
						case when e.AcquisitionCost is not null then e.AcquisitionCost else
						0 end end as ParentAcquisitionCost,

					case when d.TypeOfContractor is not null then d.TypeOfContractor else
						case when e.TypeOfContractor is not null then e.TypeOfContractor else
						'''' end end as ParentTypeOfContractor,

					case when d.ItemDescription is not null then d.ItemDescription else
						case when e.ItemDescription is not null then e.ItemDescription else
						'''' end end as ParentItemDescription,

					case when d.ItemId is not null then 1 else 0 end as ParentActive,
					case when e.ItemId is not null then 1 else 0 end as ParentHistorical, '

			
					select @query1b = '	-1 as ItemHistoryId,
					i.LastModificationType, 
					i.ModificationStatusId, 
					i.LastModifiedBy, 
					i.LastModificationDate,
					'''' as ReasonMovedToHistory,
					'''' as MovedToHistoryBy,
					null as DateMovedToHistory,
					0 as Restorable,
					ROW_NUMBER() OVER ( ORDER BY d.CatalogNumber, e.CatalogNumber   ) as RowNumber,
					ROW_NUMBER() OVER ( ORDER BY d.CatalogNumber desc, e.CatalogNumber desc   ) as ReverseRowNumber 				
				from CM_Items i left outer join
				( select z.ItemId, z.CatalogNumber, z.ManufacturersCatalogNumber, z.ManufacturersName, z.LetterOfCommitmentDate, 
					z.CommercialListPrice, z.CommercialPricelistDate, z.CommercialPricelistFOBTerms, 
					z.ManufacturersCommercialListPrice, z.TrackingMechanism, z.AcquisitionCost, z.TypeOfContractor, z.ItemDescription
					from CM_Items z where z.ContractId = @ParentContractId_parm ) d on i.ParentItemId = d.ItemId
				left outer join
				( select y.ItemId, y.CatalogNumber, y.ManufacturersCatalogNumber, y.ManufacturersName, y.LetterOfCommitmentDate, 
					y.CommercialListPrice, y.CommercialPricelistDate, y.CommercialPricelistFOBTerms, 
					y.ManufacturersCommercialListPrice, y.TrackingMechanism, y.AcquisitionCost, y.TypeOfContractor, y.ItemDescription
					from CM_ItemsHistory y where y.ContractId = @ParentContractId_parm
									and y.Ordinality = ( select max( j.Ordinality )
															from CM_ItemsHistory j
															where j.ItemId = y.ItemId )) e on i.ParentItemId = e.ItemId

				where ContractId = @ContractId_parm ' + @searchWhere +									
				'	) x
			where x.RowNumber between @StartRow_parm and @EndRow_parm '

			select @error = @@error
	
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error assigning query string when retrieving BPA items for contract ' + @ContractNumber
				goto ERROREXIT
			END
		END

		select @SQLParms = N'@ContractId_parm int, @ParentContractId_parm int, @ContractNumber_parm nvarchar(20), @StartRow_parm int, @EndRow_parm int'

		set @query = @query1 + @query1a + @query1b + @orderBy
		
		exec SP_EXECUTESQL  @query, @SQLParms, @ContractId_parm = @ContractId, @ParentContractId_parm = @ParentContractId, @ContractNumber_parm = @ContractNumber, @StartRow_parm = @StartRow, @EndRow_parm = @EndRow
																					
		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting items for contract (1)'
			goto ERROREXIT
		END
	END
	else  -- adding
	BEGIN
		select	@ItemId  = -1,
			@CatalogNumber  = '',
			@ManufacturersCatalogNumber = '',
			@ManufacturersName = '',
			@LetterOfCommitmentDate = '', 
			@CommercialListPrice = 0,
			@CommercialPricelistDate = '',
			@CommercialPricelistFOBTerms = '',
			@ManufacturersCommercialListPrice = 0,
			@TrackingMechanism = '', 
			@AcquisitionCost = 0,
			@TypeOfContractor = '',  
			@ItemDescription  = '',
			@SIN  = '',
			@ServiceCategoryId = -1,
			@PackageAsPriced  = '',
			@CurrentPrice   = '',
			@PriceStartDate  = '',
			@PriceStopDate  = '',
			@HasBPA = 0,
			@ParentItemId = -1,
			@ParentCatalogNumber = '',
			@ParentManufacturersCatalogNumber = '',
			@ParentManufacturersName = '',
			@ParentLetterOfCommitmentDate = '', 
			@ParentCommercialListPrice = 0,
			@ParentCommercialPricelistDate = '',
			@ParentCommercialPricelistFOBTerms = '',
			@ParentManufacturersCommercialListPrice = 0,
			@ParentTrackingMechanism = '', 
			@ParentAcquisitionCost = 0,
			@ParentTypeOfContractor = '',  
			@ParentItemDescription = '',
			@ParentActive = 0,
			@ParentHistorical = 0,
			@ItemHistoryId = -1,
			@LastModificationType  = '',
			@ModificationStatusId = -1,
			@LastModifiedBy  = '',
			@LastModificationDate = getdate(),
			@ReasonMovedToHistory = '',
			@MovedToHistoryBy = '',
			@DateMovedToHistory = getdate(),
			@Restorable = 0,
			@RowNumber = 0,
			@ReverseRowNumber = 0

		if @IsBPA = 0
		BEGIN
			if @IsService = 0
			BEGIN
				

				select @query1 = 'select x.ItemId, x.CatalogNumber, x.ManufacturersCatalogNumber, x.ManufacturersName, x.LetterOfCommitmentDate, x.CommercialListPrice, x.CommercialPricelistDate, x.CommercialPricelistFOBTerms, x.ManufacturersCommercialListPrice, x.TrackingMechanism, x.AcquisitionCost, x.TypeOfContractor, 
					x.ItemDescription, x.[SIN], x.ServiceCategoryId, x.PackageAsPriced, x.CurrentPrice, x.PriceStartDate, x.PriceStopDate, x.HasBPA,
					x.ParentItemId, x.ParentCatalogNumber, x.ParentManufacturersCatalogNumber, x.ParentManufacturersName, x.ParentLetterOfCommitmentDate, x.ParentCommercialListPrice, x.ParentCommercialPricelistDate, x.ParentCommercialPricelistFOBTerms, x.ParentManufacturersCommercialListPrice, x.ParentTrackingMechanism, x.ParentAcquisitionCost, x.ParentTypeOfContractor, 
					x.ParentItemDescription, x.ParentActive,  x.ParentHistorical, x.ItemHistoryId,  x.LastModificationType,  x.ModificationStatusId, x.LastModifiedBy, x.LastModificationDate,
					x.ReasonMovedToHistory, x.MovedToHistoryBy, x.DateMovedToHistory, x.Restorable,
					RowNumber + ReverseRowNumber - 1 as TotalRows
					from ( select i.ItemId, 
					i.CatalogNumber,  
					i.ManufacturersCatalogNumber, 
					i.ManufacturersName, 
					i.LetterOfCommitmentDate, 
					i.CommercialListPrice, 
					i.CommercialPricelistDate, 
					i.CommercialPricelistFOBTerms, 
					i.ManufacturersCommercialListPrice, 
					i.TrackingMechanism, 
					i.AcquisitionCost, 
					i.TypeOfContractor, 
					i.ItemDescription, 
					i.[SIN], 
					isnull( i.ServiceCategoryId, -1 ) as ServiceCategoryId,
					i.PackageAsPriced, 
					( select convert( nvarchar(20), isnull( p.Price, '''' )) from CM_ItemPrice p where p.ItemId = i.ItemId 
																				and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																				and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																				and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as CurrentPrice,
					( select isnull( convert( nvarchar(10),  p.PriceStartDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ItemId 
																				and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																				and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																				and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as PriceStartDate,
					( select isnull( convert( nvarchar(10),  p.PriceStopDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ItemId 
																				and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																				and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																				and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as PriceStopDate,
					case when exists ( select b.ItemId from CM_Items b join tbl_Cntrcts c on b.ContractId = c.Contract_Record_ID
										join CM_BPALookup u on u.BPAContractNumber = c.CntrctNum 
										where b.ParentItemId = i.ItemId ) then 1 else 0 end as HasBPA,	'
										

					select @query1a = '	-1 as ParentItemId, 
					'''' as ParentCatalogNumber,
					'''' as ParentManufacturersCatalogNumber, 
					'''' as ParentManufacturersName, 
					'''' as ParentLetterOfCommitmentDate, 
					0 as ParentCommercialListPrice, 
					'''' as ParentCommercialPricelistDate, 
					'''' as ParentCommercialPricelistFOBTerms, 
					0 as ParentManufacturersCommercialListPrice, 
					'''' as ParentTrackingMechanism, 
					0 as ParentAcquisitionCost, 
					'''' as ParentTypeOfContractor, 
					'''' as ParentItemDescription,
					0 as ParentActive,
					0 as ParentHistorical,
					-1 as ItemHistoryId,
					i.LastModificationType, 
					i.ModificationStatusId, 
					i.LastModifiedBy, 
					i.LastModificationDate,
					'''' as ReasonMovedToHistory,
					'''' as MovedToHistoryBy,
					null as DateMovedToHistory,
					0 as Restorable,
					ROW_NUMBER() OVER ( ORDER BY CatalogNumber  ) as RowNumber,
					ROW_NUMBER() OVER ( ORDER BY CatalogNumber desc  ) as ReverseRowNumber
				from CM_Items i 
				where ContractId = @ContractId_parm  ' + @searchWhere +	' ' 
			
				select @query2 = ' union

				select	@ItemId_parm as ItemId,
					@CatalogNumber_parm as CatalogNumber,
					@ManufacturersCatalogNumber_parm as ManufacturersCatalogNumber,
					@ManufacturersName_parm as ManufacturersName,
					@LetterOfCommitmentDate_parm as LetterOfCommitmentDate,
					@CommercialListPrice_parm as CommercialListPrice,
					@CommercialPricelistDate_parm as  CommercialPricelistDate,
					@CommercialPricelistFOBTerms_parm as  CommercialPricelistFOBTerms,
					@ManufacturersCommercialListPrice_parm as  ManufacturersCommercialListPrice,
					@TrackingMechanism_parm as TrackingMechanism, 
					@AcquisitionCost_parm as AcquisitionCost, 
					@TypeOfContractor_parm as TypeOfContractor, 
					@ItemDescription_parm as ItemDescription,
					@SIN_parm  as [SIN],
					@ServiceCategoryId_parm as ServiceCategoryId,
					@PackageAsPriced_parm as PackageAsPriced, 
					@CurrentPrice_parm as CurrentPrice,
					@PriceStartDate_parm as PriceStartDate,
					@PriceStopDate_parm as PriceStopDate,
					@HasBPA_parm as HasBPA,
					@ParentItemId_parm as ParentItemId,
					@ParentCatalogNumber_parm as ParentCatalogNumber,

					@ParentManufacturersCatalogNumber_parm as ParentManufacturersCatalogNumber,
					@ParentManufacturersName_parm as ParentManufacturersName,
					@ParentLetterOfCommitmentDate_parm as ParentLetterOfCommitmentDate,
					@ParentCommercialListPrice_parm as ParentCommercialListPrice,
					@ParentCommercialPricelistDate_parm as  ParentCommercialPricelistDate,
					@ParentCommercialPricelistFOBTerms_parm as  ParentCommercialPricelistFOBTerms,
					@ParentManufacturersCommercialListPrice_parm as  ParentManufacturersCommercialListPrice,
					@ParentTrackingMechanism_parm as ParentTrackingMechanism, 
					@ParentAcquisitionCost_parm as ParentAcquisitionCost, 
					@ParentTypeOfContractor_parm as ParentTypeOfContractor, 

					@ParentItemDescription_parm as ParentItemDescription,
					@ParentActive_parm as ParentActive,
					@ParentHistorical_parm as ParentHistorical,
					@ItemHistoryId_parm as ItemHistoryId,
					@LastModificationType_parm as LastModificationType,
					@ModificationStatusId_parm as ModificationStatusId,
					@LastModifiedBy_parm as LastModifiedBy,
					@LastModificationDate_parm as LastModificationDate,
					@ReasonMovedToHistory_parm as ReasonMovedToHistory,
					@MovedToHistoryBy_parm as MovedToHistoryBy,
					@DateMovedToHistory_parm as DateMovedToHistory,
					@Restorable_parm as Restorable,
					@RowNumber_parm as RowNumber,
					@ReverseRowNumber_parm as ReverseRowNumber			
			
				) x
				where x.RowNumber between @StartRow_parm and @EndRow_parm
				or x.RowNumber = 0 '
				
				select @error = @@error
	
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error assigning query string when retrieving non-BPA items ( with add ) for contract ' + @ContractNumber
					goto ERROREXIT
				END
			END
			else  -- service
			BEGIN
				select @query1 = 'select x.ItemId, x.CatalogNumber, x.ManufacturersCatalogNumber, x.ManufacturersName, x.LetterOfCommitmentDate, x.CommercialListPrice, x.CommercialPricelistDate, x.CommercialPricelistFOBTerms, x.ManufacturersCommercialListPrice, x.TrackingMechanism, x.AcquisitionCost, x.TypeOfContractor, 
					x.ItemDescription, x.[SIN], x.ServiceCategoryId, x.PackageAsPriced, x.CurrentPrice, x.PriceStartDate, x.PriceStopDate, x.HasBPA,
					x.ParentItemId, x.ParentCatalogNumber, x.ParentManufacturersCatalogNumber, x.ParentManufacturersName, x.ParentLetterOfCommitmentDate, x.ParentCommercialListPrice, x.ParentCommercialPricelistDate, x.ParentCommercialPricelistFOBTerms, x.ParentManufacturersCommercialListPrice, x.ParentTrackingMechanism, x.ParentAcquisitionCost, x.ParentTypeOfContractor, 
					x.ParentItemDescription, x.ParentActive,  x.ParentHistorical, x.ItemHistoryId,  x.LastModificationType,  x.ModificationStatusId, x.LastModifiedBy, x.LastModificationDate,
					x.ReasonMovedToHistory, x.MovedToHistoryBy, x.DateMovedToHistory, x.Restorable,
					RowNumber + ReverseRowNumber - 1 as TotalRows
					from ( select i.ItemId, 
					i.CatalogNumber,  
					
					i.ManufacturersCatalogNumber, 
					i.ManufacturersName, 
					i.LetterOfCommitmentDate, 
					i.CommercialListPrice, 
					i.CommercialPricelistDate, 
					i.CommercialPricelistFOBTerms, 
					i.ManufacturersCommercialListPrice, 					
					i.TrackingMechanism, 
					i.AcquisitionCost, 
					i.TypeOfContractor,

					i.ItemDescription, 
					i.[SIN], 
					isnull( i.ServiceCategoryId, -1 ) as ServiceCategoryId,
					i.PackageAsPriced, 
					( select convert( nvarchar(20), isnull( p.Price, '''' )) from CM_ItemPrice p where p.ItemId = i.ItemId 
															and datediff( DD, p.PriceStartDate, getdate() ) >= 0
															and datediff( DD, getdate(), p.PriceStopDate ) >= 0
															and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as CurrentPrice,
					( select isnull( convert( nvarchar(10),  p.PriceStartDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ItemId 
																				and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																				and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																				and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as PriceStartDate,
					( select isnull( convert( nvarchar(10),  p.PriceStopDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ItemId 
																				and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																				and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																				and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ItemId ) ) as PriceStopDate,	
					case when exists ( select b.ItemId from CM_Items b join tbl_Cntrcts c on b.ContractId = c.Contract_Record_ID
										join CM_BPALookup u on u.BPAContractNumber = c.CntrctNum 
										where b.ParentItemId = i.ItemId ) then 1 else 0 end as HasBPA,	'
										
					select @query1a = '	-1 as ParentItemId, 
					'''' as ParentCatalogNumber,
					'''' as ParentManufacturersCatalogNumber, 
					'''' as ParentManufacturersName, 
					'''' as ParentLetterOfCommitmentDate, 
					0 as ParentCommercialListPrice, 
					'''' as ParentCommercialPricelistDate, 
					'''' as ParentCommercialPricelistFOBTerms, 
					0 as ParentManufacturersCommercialListPrice, 
					'''' as ParentTrackingMechanism, 
					0 as ParentAcquisitionCost, 
					'''' as ParentTypeOfContractor, 
					'''' as ParentItemDescription,
					0 as ParentActive,
					0 as ParentHistorical,
					-1 as ItemHistoryId,
					i.LastModificationType, 
					i.ModificationStatusId, 
					i.LastModifiedBy, 
					i.LastModificationDate,
					'''' as ReasonMovedToHistory,
					'''' as MovedToHistoryBy,
					null as DateMovedToHistory,
					0 as Restorable,
					ROW_NUMBER() OVER ( ORDER BY CatalogNumber   ) as RowNumber,
					ROW_NUMBER() OVER ( ORDER BY CatalogNumber desc   ) as ReverseRowNumber
				from CM_Items i 
				where ContractId = @ContractId_parm  ' + @searchWhere + ' '
							
				select @query2 = '	union

				select	@ItemId_parm as ItemId,
					@CatalogNumber_parm as CatalogNumber,					
					@ManufacturersCatalogNumber_parm as ManufacturersCatalogNumber,
					@ManufacturersName_parm as ManufacturersName,
					@LetterOfCommitmentDate_parm as LetterOfCommitmentDate,
					@CommercialListPrice_parm as CommercialListPrice,
					@CommercialPricelistDate_parm as  CommercialPricelistDate,
					@CommercialPricelistFOBTerms_parm as  CommercialPricelistFOBTerms,
					@ManufacturersCommercialListPrice_parm as  ManufacturersCommercialListPrice,
					@TrackingMechanism_parm as TrackingMechanism, 
					@AcquisitionCost_parm as AcquisitionCost, 
					@TypeOfContractor_parm as TypeOfContractor, 
					@ItemDescription_parm as ItemDescription,
					@SIN_parm  as [SIN],
					@ServiceCategoryId_parm as ServiceCategoryId,
					@PackageAsPriced_parm as PackageAsPriced, 
					@CurrentPrice_parm as CurrentPrice,
					@PriceStartDate_parm as PriceStartDate,
					@PriceStopDate_parm as PriceStopDate,
					@HasBPA_parm as HasBPA,
					@ParentItemId_parm as ParentItemId,
					@ParentCatalogNumber_parm as ParentCatalogNumber,
					
					@ParentManufacturersCatalogNumber_parm as ParentManufacturersCatalogNumber,
					@ParentManufacturersName_parm as ParentManufacturersName,
					@ParentLetterOfCommitmentDate_parm as ParentLetterOfCommitmentDate,
					@ParentCommercialListPrice_parm as ParentCommercialListPrice,
					@ParentCommercialPricelistDate_parm as  ParentCommercialPricelistDate,
					@ParentCommercialPricelistFOBTerms_parm as  ParentCommercialPricelistFOBTerms,
					@ParentManufacturersCommercialListPrice_parm as  ParentManufacturersCommercialListPrice,
					@ParentTrackingMechanism_parm as ParentTrackingMechanism, 
					@ParentAcquisitionCost_parm as ParentAcquisitionCost, 
					@ParentTypeOfContractor_parm as ParentTypeOfContractor, 

					@ParentItemDescription_parm as ParentItemDescription,
					@ParentActive_parm as ParentActive,
					@ParentHistorical_parm as ParentHistorical,
					@ItemHistoryId_parm as ItemHistoryId,
					@LastModificationType_parm as LastModificationType,
					@ModificationStatusId_parm as ModificationStatusId,
					@LastModifiedBy_parm as LastModifiedBy,
					@LastModificationDate_parm as LastModificationDate,
					@ReasonMovedToHistory_parm as ReasonMovedToHistory,
					@MovedToHistoryBy_parm as MovedToHistoryBy,
					@DateMovedToHistory_parm as DateMovedToHistory,
					@Restorable_parm as Restorable,
					@RowNumber_parm as RowNumber,
					@ReverseRowNumber_parm as ReverseRowNumber			
			
				) x
				where x.RowNumber between @StartRow_parm and @EndRow_parm
				or x.RowNumber = 0 '

				select @error = @@error
	
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error assigning query string when retrieving service items ( with add ) for contract ' + @ContractNumber
					goto ERROREXIT
				END

			END
		END
		else -- for BPA the price is the parent price
		BEGIN
			select @query1 = 'select x.ItemId, x.CatalogNumber, x.ManufacturersCatalogNumber, x.ManufacturersName, x.LetterOfCommitmentDate, x.CommercialListPrice, x.CommercialPricelistDate, x.CommercialPricelistFOBTerms, x.ManufacturersCommercialListPrice, x.TrackingMechanism, x.AcquisitionCost, x.TypeOfContractor,
				x.ItemDescription, x.[SIN], x.ServiceCategoryId, x.PackageAsPriced, x.CurrentPrice, x.PriceStartDate, x.PriceStopDate, x.HasBPA,
				x.ParentItemId, x.ParentCatalogNumber, x.ParentManufacturersCatalogNumber, x.ParentManufacturersName, x.ParentLetterOfCommitmentDate, x.ParentCommercialListPrice, x.ParentCommercialPricelistDate, x.ParentCommercialPricelistFOBTerms, x.ParentManufacturersCommercialListPrice, x.ParentTrackingMechanism, x.ParentAcquisitionCost, x.ParentTypeOfContractor,
				x.ParentItemDescription, x.ParentActive,  x.ParentHistorical, x.ItemHistoryId, x.LastModificationType,  x.ModificationStatusId, x.LastModifiedBy, x.LastModificationDate,
				x.ReasonMovedToHistory, x.MovedToHistoryBy, x.DateMovedToHistory, x.Restorable,
				RowNumber + ReverseRowNumber - 1 as TotalRows
				from ( select i.ItemId, 
				i.CatalogNumber,  
				i.ManufacturersCatalogNumber, 
				i.ManufacturersName, 
				i.LetterOfCommitmentDate, 
				i.CommercialListPrice, 
				i.CommercialPricelistDate, 
				i.CommercialPricelistFOBTerms, 
				i.ManufacturersCommercialListPrice, 
				i.TrackingMechanism, 
				i.AcquisitionCost, 
				i.TypeOfContractor, 
				i.ItemDescription, 
				i.[SIN], 
				isnull( i.ServiceCategoryId, -1 ) as ServiceCategoryId,
				i.PackageAsPriced, 
				( select convert( nvarchar(20), isnull( p.Price, '''' )) from CM_ItemPrice p where p.ItemId = i.ParentItemId 
															and datediff( DD, p.PriceStartDate, getdate() ) >= 0
															and datediff( DD, getdate(), p.PriceStopDate ) >= 0
															and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ParentItemId ) ) as CurrentPrice,
				( select isnull( convert( nvarchar(10),  p.PriceStartDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ParentItemId 
																			and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																			and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																			and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ParentItemId ) ) as PriceStartDate,
				( select isnull( convert( nvarchar(10),  p.PriceStopDate, 101 ), '''' )  from CM_ItemPrice p where p.ItemId = i.ParentItemId 
																			and datediff( DD, p.PriceStartDate, getdate() ) >= 0
																			and datediff( DD, getdate(), p.PriceStopDate ) >= 0
																			and p.ItemPriceId = ( select min( ItemPriceId ) from CM_ItemPrice where ItemId = i.ParentItemId ) ) as PriceStopDate,	
				0 as HasBPA,  '
							
				select @query1a = '	case when d.ItemId is not null then d.ItemId else
						case when e.ItemId is not null then e.ItemId else
						-1 end end as ParentItemId,
											
				case when d.CatalogNumber is not null then d.CatalogNumber else
					case when e.CatalogNumber is not null then e.CatalogNumber else
					'''' end end as ParentCatalogNumber,

				case when d.ManufacturersCatalogNumber is not null then d.ManufacturersCatalogNumber else
					case when e.ManufacturersCatalogNumber is not null then e.ManufacturersCatalogNumber else
					'''' end end as ParentManufacturersCatalogNumber,

				case when d.ManufacturersName is not null then d.ManufacturersName else
					case when e.ManufacturersName is not null then e.ManufacturersName else
					'''' end end as ParentManufacturersName,

				case when d.LetterOfCommitmentDate is not null then d.LetterOfCommitmentDate else
					case when e.LetterOfCommitmentDate is not null then e.LetterOfCommitmentDate else
					'''' end end as ParentLetterOfCommitmentDate,

				case when d.CommercialListPrice is not null then d.CommercialListPrice else
					case when e.CommercialListPrice is not null then e.CommercialListPrice else
					0 end end as ParentCommercialListPrice,

				case when d.CommercialPricelistDate is not null then d.CommercialPricelistDate else
					case when e.CommercialPricelistDate is not null then e.CommercialPricelistDate else
					'''' end end as ParentCommercialPricelistDate,

				case when d.CommercialPricelistFOBTerms is not null then d.CommercialPricelistFOBTerms else
					case when e.CommercialPricelistFOBTerms is not null then e.CommercialPricelistFOBTerms else
					'''' end end as ParentCommercialPricelistFOBTerms,

				case when d.ManufacturersCommercialListPrice is not null then d.ManufacturersCommercialListPrice else
					case when e.ManufacturersCommercialListPrice is not null then e.ManufacturersCommercialListPrice else
					0 end end as ParentManufacturersCommercialListPrice,

				case when d.TrackingMechanism is not null then d.TrackingMechanism else
						case when e.TrackingMechanism is not null then e.TrackingMechanism else
						'''' end end as ParentTrackingMechanism,

				case when d.AcquisitionCost is not null then d.AcquisitionCost else
						case when e.AcquisitionCost is not null then e.AcquisitionCost else
						0 end end as ParentAcquisitionCost,

				case when d.TypeOfContractor is not null then d.TypeOfContractor else
						case when e.TypeOfContractor is not null then e.TypeOfContractor else
						'''' end end as ParentTypeOfContractor,

				case when d.ItemDescription is not null then d.ItemDescription else
					case when e.ItemDescription is not null then e.ItemDescription else
					'''' end end as ParentItemDescription,

				case when d.ItemId is not null then 1 else 0 end as ParentActive,
				case when e.ItemId is not null then 1 else 0 end as ParentHistorical,  '

				select @query1b = '	-1 as ItemHistoryId,
				i.LastModificationType, 
				i.ModificationStatusId, 
				i.LastModifiedBy, 
				i.LastModificationDate,
				'''' as ReasonMovedToHistory,
				'''' as MovedToHistoryBy,
				null as DateMovedToHistory,
				0 as Restorable,
				
				ROW_NUMBER() OVER ( ORDER BY d.CatalogNumber, e.CatalogNumber   ) as RowNumber,
				ROW_NUMBER() OVER ( ORDER BY d.CatalogNumber desc, e.CatalogNumber desc   ) as ReverseRowNumber 				
				
				from CM_Items i left outer join
				( select z.ItemId, z.CatalogNumber, z.ManufacturersCatalogNumber, z.ManufacturersName, z.LetterOfCommitmentDate, 
					z.CommercialListPrice, z.CommercialPricelistDate, z.CommercialPricelistFOBTerms, 
					z.ManufacturersCommercialListPrice, z.TrackingMechanism, z.AcquisitionCost, z.TypeOfContractor, z.ItemDescription
					from CM_Items z where z.ContractId = @ParentContractId_parm ) d on i.ParentItemId = d.ItemId
				left outer join
				( select y.ItemId, y.CatalogNumber, y.ManufacturersCatalogNumber, y.ManufacturersName, y.LetterOfCommitmentDate, 
					y.CommercialListPrice, y.CommercialPricelistDate, y.CommercialPricelistFOBTerms, 
					y.ManufacturersCommercialListPrice, y.TrackingMechanism, y.AcquisitionCost, y.TypeOfContractor, y.ItemDescription 
					from CM_ItemsHistory y where y.ContractId = @ParentContractId_parm
									and y.Ordinality = ( select max( j.Ordinality )
															from CM_ItemsHistory j
															where j.ItemId = y.ItemId )) e on i.ParentItemId = e.ItemId

				where ContractId = @ContractId_parm  ' + @searchWhere + ' '							
				
			select @query2 = ' union

			select	@ItemId_parm as ItemId,
				@CatalogNumber_parm as CatalogNumber,
				@ManufacturersCatalogNumber_parm as ManufacturersCatalogNumber,
				@ManufacturersName_parm as ManufacturersName,
				@LetterOfCommitmentDate_parm as LetterOfCommitmentDate,
				@CommercialListPrice_parm as CommercialListPrice,
				@CommercialPricelistDate_parm as  CommercialPricelistDate,
				@CommercialPricelistFOBTerms_parm as  CommercialPricelistFOBTerms,
				@ManufacturersCommercialListPrice_parm as  ManufacturersCommercialListPrice,
				@TrackingMechanism_parm as TrackingMechanism, 
				@AcquisitionCost_parm as AcquisitionCost, 
				@TypeOfContractor_parm as TypeOfContractor, 
				@ItemDescription_parm as ItemDescription,
				@SIN_parm  as [SIN],
				@ServiceCategoryId_parm as ServiceCategoryId,
				@PackageAsPriced_parm as PackageAsPriced, 
				@CurrentPrice_parm as CurrentPrice,
				@PriceStartDate_parm as PriceStartDate,
				@PriceStopDate_parm as PriceStopDate,
				@HasBPA_parm as HasBPA,
				@ParentItemId_parm as ParentItemId,
				@ParentCatalogNumber_parm as ParentCatalogNumber,
				@ParentManufacturersCatalogNumber_parm as ParentManufacturersCatalogNumber,
				@ParentManufacturersName_parm as ParentManufacturersName,
				@ParentLetterOfCommitmentDate_parm as ParentLetterOfCommitmentDate,
				@ParentCommercialListPrice_parm as ParentCommercialListPrice,
				@ParentCommercialPricelistDate_parm as  ParentCommercialPricelistDate,
				@ParentCommercialPricelistFOBTerms_parm as  ParentCommercialPricelistFOBTerms,
				@ParentManufacturersCommercialListPrice_parm as  ParentManufacturersCommercialListPrice,
				@ParentTrackingMechanism_parm as ParentTrackingMechanism, 
				@ParentAcquisitionCost_parm as ParentAcquisitionCost, 
				@ParentTypeOfContractor_parm as ParentTypeOfContractor, 
				@ParentItemDescription_parm as ParentItemDescription,
				@ParentActive_parm as ParentActive,
				@ParentHistorical_parm as ParentHistorical,
				@ItemHistoryId_parm as ItemHistoryId,
				@LastModificationType_parm as LastModificationType,
				@ModificationStatusId_parm as ModificationStatusId,
				@LastModifiedBy_parm as LastModifiedBy,
				@LastModificationDate_parm as LastModificationDate,
				@ReasonMovedToHistory_parm as ReasonMovedToHistory,
				@MovedToHistoryBy_parm as MovedToHistoryBy,
				@DateMovedToHistory_parm as DateMovedToHistory,
				@Restorable_parm as Restorable,
				@RowNumber_parm as RowNumber,
				@ReverseRowNumber_parm as ReverseRowNumber			
			
			) x
			where x.RowNumber between @StartRow_parm and @EndRow_parm
			or x.RowNumber = 0 '

			select @error = @@error
	
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error assigning query string when retrieving BPA items ( with add ) for contract ' + @ContractNumber
				goto ERROREXIT
			END

		END

		select @SQLParms = N'@ParentContractId_parm int,
			@ContractId_parm int,
			@ItemId_parm int,
			@CatalogNumber_parm nvarchar(70), 

			@ManufacturersCatalogNumber_parm nvarchar(100),
			@ManufacturersName_parm nvarchar(100),
			@LetterOfCommitmentDate_parm datetime,
			@CommercialListPrice_parm decimal(10,2),
			@CommercialPricelistDate_parm datetime,
			@CommercialPricelistFOBTerms_parm nvarchar(40),
			@ManufacturersCommercialListPrice_parm decimal(10,2),
			@TrackingMechanism_parm	nvarchar(100),
			@AcquisitionCost_parm decimal(10,2),
			@TypeOfContractor_parm nvarchar(100),

			@ItemDescription_parm nvarchar(800), 
			@SIN_parm nvarchar(50), 
			@ServiceCategoryId_parm int,
			@PackageAsPriced_parm nvarchar(2), 
			@CurrentPrice_parm nvarchar(20), 
			@PriceStartDate_parm nvarchar(10),
			@PriceStopDate_parm nvarchar(10),
			@HasBPA_parm bit,
			@ParentItemId_parm int, 
			@ParentCatalogNumber_parm nvarchar(70),

			@ParentManufacturersCatalogNumber_parm nvarchar(100),
			@ParentManufacturersName_parm nvarchar(100),
			@ParentLetterOfCommitmentDate_parm datetime,
			@ParentCommercialListPrice_parm decimal(10,2),
			@ParentCommercialPricelistDate_parm datetime,
			@ParentCommercialPricelistFOBTerms_parm nvarchar(40),
			@ParentManufacturersCommercialListPrice_parm decimal(10,2),
			@ParentTrackingMechanism_parm	nvarchar(100),
			@ParentAcquisitionCost_parm decimal(10,2),
			@ParentTypeOfContractor_parm nvarchar(100),

			@ParentItemDescription_parm nvarchar(800),
			@ParentActive_parm bit,
			@ParentHistorical_parm bit,
			@ItemHistoryId_parm int,
			@LastModificationType_parm nchar(1), 
			@ModificationStatusId_parm int, 
			@LastModifiedBy_parm nvarchar(120), 
			@LastModificationDate_parm datetime,
			@ReasonMovedToHistory_parm nvarchar(30),
			@MovedToHistoryBy_parm nvarchar(120),
			@DateMovedToHistory_parm datetime,
			@Restorable_parm bit,
			@RowNumber_parm int,
			@ReverseRowNumber_parm int,		
			@StartRow_parm int,
			@EndRow_parm int '


		select @query = @query1 + @query1a + @query1b + @query2 + @orderBy

		exec SP_EXECUTESQL @query, @SQLParms, @ParentContractId_parm = @ParentContractId, 
												@ContractId_parm = @ContractId, 
												@ItemId_parm = @ItemId,
												@CatalogNumber_parm  = @CatalogNumber,

												@ManufacturersCatalogNumber_parm = @ManufacturersCatalogNumber,
												@ManufacturersName_parm = @ManufacturersName,
												@LetterOfCommitmentDate_parm = @LetterOfCommitmentDate,
												@CommercialListPrice_parm = @CommercialListPrice,
												@CommercialPricelistDate_parm = @CommercialPricelistDate,
												@CommercialPricelistFOBTerms_parm = @CommercialPricelistFOBTerms,
												@ManufacturersCommercialListPrice_parm = @ManufacturersCommercialListPrice,
												@TrackingMechanism_parm = @TrackingMechanism, 
												@AcquisitionCost_parm = @AcquisitionCost, 
												@TypeOfContractor_parm = @TypeOfContractor, 

												@ItemDescription_parm  = @ItemDescription,
												@SIN_parm = @SIN,
												@ServiceCategoryId_parm = @ServiceCategoryId,
												@PackageAsPriced_parm  = @PackageAsPriced,
												@CurrentPrice_parm   = @CurrentPrice,
												@PriceStartDate_parm  = @PriceStartDate,
												@PriceStopDate_parm  = @PriceStopDate,												
												@HasBPA_parm = @HasBPA,
												@ParentItemId_parm = @ParentItemId,
												@ParentCatalogNumber_parm = @ParentCatalogNumber,

												@ParentManufacturersCatalogNumber_parm = @ParentManufacturersCatalogNumber,
												@ParentManufacturersName_parm = @ParentManufacturersName,
												@ParentLetterOfCommitmentDate_parm = @ParentLetterOfCommitmentDate,
												@ParentCommercialListPrice_parm = @ParentCommercialListPrice,
												@ParentCommercialPricelistDate_parm = @ParentCommercialPricelistDate,
												@ParentCommercialPricelistFOBTerms_parm = @ParentCommercialPricelistFOBTerms,
												@ParentManufacturersCommercialListPrice_parm = @ParentManufacturersCommercialListPrice,
												@ParentTrackingMechanism_parm = @ParentTrackingMechanism, 
												@ParentAcquisitionCost_parm = @ParentAcquisitionCost, 
												@ParentTypeOfContractor_parm = @ParentTypeOfContractor, 

												@ParentItemDescription_parm = @ParentItemDescription,
												@ParentActive_parm = @ParentActive,
												@ParentHistorical_parm = @ParentHistorical,
												@ItemHistoryId_parm = @ItemHistoryId,
												@LastModificationType_parm  = @LastModificationType,
												@ModificationStatusId_parm = @ModificationStatusId,
												@LastModifiedBy_parm  = @LastModifiedBy,
												@LastModificationDate_parm = @LastModificationDate,
												@ReasonMovedToHistory_parm = @ReasonMovedToHistory,
												@MovedToHistoryBy_parm = @MovedToHistoryBy,
												@DateMovedToHistory_parm = @DateMovedToHistory,
												@Restorable_parm = @Restorable,
												@RowNumber_parm = @RowNumber,
												@ReverseRowNumber_parm = @ReverseRowNumber,											
												@StartRow_parm = @StartRow,
												@EndRow_parm = @EndRow

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting items for contract (2)'
			goto ERROREXIT
		END
	END
END
else  -- Historical, including removed
BEGIN
	if @IsBPA = 0
	BEGIN
		if @IsService = 0
		BEGIN
			select @query1 = 'select x.ItemId, x.Ordinality, x.CatalogNumber, x.ManufacturersCatalogNumber, x.ManufacturersName, x.LetterOfCommitmentDate, x.CommercialListPrice, x.CommercialPricelistDate, x.CommercialPricelistFOBTerms, x.ManufacturersCommercialListPrice, x.TrackingMechanism, x.AcquisitionCost, x.TypeOfContractor, 
				x.ItemDescription, x.[SIN], x.ServiceCategoryId, x.PackageAsPriced, x.CurrentPrice, x.PriceStartDate, x.PriceStopDate, x.HasBPA,
				x.ParentItemId, x.ParentCatalogNumber, x.ParentManufacturersCatalogNumber, x.ParentManufacturersName, x.ParentLetterOfCommitmentDate, x.ParentCommercialListPrice, x.ParentCommercialPricelistDate, x.ParentCommercialPricelistFOBTerms, x.ParentManufacturersCommercialListPrice, x.ParentTrackingMechanism, x.ParentAcquisitionCost, x.ParentTypeOfContractor, 
				x.ParentItemDescription, x.ParentActive,  x.ParentHistorical, x.ItemHistoryId,  x.LastModificationType,  x.ModificationStatusId, x.LastModifiedBy, x.LastModificationDate,
				x.ReasonMovedToHistory, x.MovedToHistoryBy, x.DateMovedToHistory, x.Restorable,
				RowNumber + ReverseRowNumber - 1 as TotalRows
				from (
						select h.ItemId, 
						h.Ordinality,
						h.CatalogNumber,  

						h.ManufacturersCatalogNumber, 
						h.ManufacturersName, 
						h.LetterOfCommitmentDate, 
						h.CommercialListPrice, 
						h.CommercialPricelistDate, 
						h.CommercialPricelistFOBTerms, 
						h.ManufacturersCommercialListPrice, 
						h.TrackingMechanism, 
						h.AcquisitionCost, 
						h.TypeOfContractor,	

						h.ItemDescription, 
						h.[SIN], 
						isnull( h.ServiceCategoryId, -1 ) as ServiceCategoryId,
						h.PackageAsPriced, 
						'''' as CurrentPrice,    -- current price is not available for historical items
						'''' as PriceStartDate,
						'''' as PriceStopDate,
						case when exists ( select b.ItemId from CM_Items b join tbl_Cntrcts c on b.ContractId = c.Contract_Record_ID
											join CM_BPALookup u on u.BPAContractNumber = c.CntrctNum 
											where b.ParentItemId = h.ItemId ) then 1 else 0 end as HasBPA,							
						-1 as ParentItemId, 
						'''' as ParentCatalogNumber,
						'''' as ParentManufacturersCatalogNumber, 
						'''' as ParentManufacturersName, 
						'''' as ParentLetterOfCommitmentDate, 
						0 as ParentCommercialListPrice, 
						'''' as ParentCommercialPricelistDate, 
						'''' as ParentCommercialPricelistFOBTerms, 
						0 as ParentManufacturersCommercialListPrice, 
						'''' as ParentTrackingMechanism, 
						0 as ParentAcquisitionCost, 
						'''' as ParentTypeOfContractor, 
						'''' as ParentItemDescription,
						0 as ParentActive,
						0 as ParentHistorical,
						h.ItemHistoryId,
						h.LastModificationType, 
						h.ModificationStatusId, 
						h.LastModifiedBy, 
						h.LastModificationDate,  '

select @query2 = '		case when ( PATINDEX( ''%DeleteMedSurgItemAndPrices%'', h.Notes ) > 0 ) then ''Deleted'' else 
							case when ( PATINDEX( ''%UpdateMedSurgItem%'', h.Notes ) > 0 ) then ''Updated'' else 
							case when ( PATINDEX( ''%DeleteItemForMedSurgItemUpload2%'', h.Notes ) > 0 ) then ''Deleted via Upload''  else 
							case when ( PATINDEX( ''%UpdateItemForMedSurgItemUpload2%'', h.Notes ) > 0 ) then ''Updated via Upload'' else 
							case when ( PATINDEX( ''%Initial%'', h.Notes ) > 0 ) then ''Initial Conversion'' else 
							case when ( PATINDEX( ''%NightlyBatchProcess%'', h.Notes ) > 0 ) then ''All Prices Expired'' else 
							''Unknown'' end end end end end end as ReasonMovedToHistory,
						case when ( PATINDEX( ''%Initial%'', h.MovedToHistoryBy ) > 0 ) then ''Unknown'' else h.MovedToHistoryBy end as MovedToHistoryBy,
						h.DateMovedToHistory,
						case when ( PATINDEX( ''%DeleteMedSurgItemAndPrices%'', h.Notes ) > 0 or PATINDEX( ''%DeleteItemForMedSurgItemUpload2%'', h.Notes ) > 0 or PATINDEX( ''%NightlyBatchProcess%'', h.Notes ) > 0 ) then 1 else 0 end as Restorable,
						ROW_NUMBER() OVER ( ORDER BY CatalogNumber   ) as RowNumber,
						ROW_NUMBER() OVER ( ORDER BY CatalogNumber desc   ) as ReverseRowNumber 
					from CM_ItemsHistory h 
					where ContractId = @ContractId_parm 
					and h.Ordinality = ( select max(Ordinality) from CM_ItemsHistory where ContractId = @ContractId_parm and ItemId = h.ItemId ) ' + @searchWhere +		
				'	) x
				where x.RowNumber between @StartRow_parm and @EndRow_parm '
									
			select @error = @@error
	
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error assigning query string when retrieving historical FSS/National items for contract ' + @ContractNumber
				goto ERROREXIT
			END
		END
		else -- service
		BEGIN
			select @query1 = 'select x.ItemId, x.Ordinality, x.CatalogNumber, x.ManufacturersCatalogNumber, x.ManufacturersName, x.LetterOfCommitmentDate, x.CommercialListPrice, x.CommercialPricelistDate, x.CommercialPricelistFOBTerms, x.ManufacturersCommercialListPrice, x.TrackingMechanism, x.AcquisitionCost, x.TypeOfContractor, 
				x.ItemDescription, x.[SIN], x.ServiceCategoryId, x.PackageAsPriced, x.CurrentPrice, x.PriceStartDate, x.PriceStopDate, x.HasBPA,
				x.ParentItemId, x.ParentCatalogNumber, x.ParentManufacturersCatalogNumber, x.ParentManufacturersName, x.ParentLetterOfCommitmentDate, x.ParentCommercialListPrice, x.ParentCommercialPricelistDate, x.ParentCommercialPricelistFOBTerms, x.ParentManufacturersCommercialListPrice, x.ParentTrackingMechanism, x.ParentAcquisitionCost, x.ParentTypeOfContractor, 
				x.ParentItemDescription, x.ParentActive,  x.ParentHistorical, x.ItemHistoryId,  x.LastModificationType,  x.ModificationStatusId, x.LastModifiedBy, x.LastModificationDate,
				x.ReasonMovedToHistory, x.MovedToHistoryBy, x.DateMovedToHistory, x.Restorable,
				RowNumber + ReverseRowNumber - 1 as TotalRows
				from (
				select h.ItemId, 
				h.Ordinality,
				h.CatalogNumber,  

				h.ManufacturersCatalogNumber, 
				h.ManufacturersName, 
				h.LetterOfCommitmentDate, 
				h.CommercialListPrice, 
				h.CommercialPricelistDate, 
				h.CommercialPricelistFOBTerms, 
				h.ManufacturersCommercialListPrice, 
				h.TrackingMechanism, 
				h.AcquisitionCost, 
				h.TypeOfContractor, 

				h.ItemDescription, 
				h.[SIN], 
				isnull( h.ServiceCategoryId, -1 ) as ServiceCategoryId,
				h.PackageAsPriced, 
				'''' as CurrentPrice,    -- current price is not available for historical items
				'''' as PriceStartDate,
				'''' as PriceStopDate,
				case when exists ( select b.ItemId from CM_Items b join tbl_Cntrcts c on b.ContractId = c.Contract_Record_ID
								join CM_BPALookup u on u.BPAContractNumber = c.CntrctNum 
								where b.ParentItemId = h.ItemId ) then 1 else 0 end as HasBPA,							
				-1 as ParentItemId, 
				'''' as ParentCatalogNumber,

				'''' as ParentManufacturersCatalogNumber, 
				'''' as ParentManufacturersName, 
				'''' as ParentLetterOfCommitmentDate, 
				0 as ParentCommercialListPrice, 
				'''' as ParentCommercialPricelistDate, 
				'''' as ParentCommercialPricelistFOBTerms, 
				0 as ParentManufacturersCommercialListPrice, 
				'''' as ParentTrackingMechanism, 
				0 as ParentAcquisitionCost, 
				'''' as ParentTypeOfContractor, 

				'''' as ParentItemDescription,
				0 as ParentActive,
				0 as ParentHistorical,
				h.ItemHistoryId,
				h.LastModificationType, 
				h.ModificationStatusId, 
				h.LastModifiedBy, 
				h.LastModificationDate,  '
				
select @query2 = '	case when ( PATINDEX( ''%DeleteMedSurgItemAndPrices%'', h.Notes ) > 0 ) then ''Deleted'' else 
					case when ( PATINDEX( ''%UpdateMedSurgItem%'', h.Notes ) > 0 ) then ''Updated'' else 
					case when ( PATINDEX( ''%DeleteItemForMedSurgItemUpload2%'', h.Notes ) > 0 ) then ''Deleted via Upload''  else 
					case when ( PATINDEX( ''%UpdateItemForMedSurgItemUpload2%'', h.Notes ) > 0 ) then ''Updated via Upload'' else 
					case when ( PATINDEX( ''%Initial%'', h.Notes ) > 0 ) then ''Initial Conversion'' else 
					case when ( PATINDEX( ''%NightlyBatchProcess%'', h.Notes ) > 0 ) then ''All Prices Expired'' else 
					''Unknown'' end end end end end end as ReasonMovedToHistory,
				case when ( PATINDEX( ''%Initial%'', h.MovedToHistoryBy ) > 0 ) then ''Unknown'' else h.MovedToHistoryBy end as MovedToHistoryBy,
				h.DateMovedToHistory,
				case when ( PATINDEX( ''%DeleteMedSurgItemAndPrices%'', h.Notes ) > 0 or PATINDEX( ''%DeleteItemForMedSurgItemUpload2%'', h.Notes ) > 0 or PATINDEX( ''%NightlyBatchProcess%'', h.Notes ) > 0 ) then 1 else 0 end as Restorable,
				ROW_NUMBER() OVER ( ORDER BY ServiceCategoryId   ) as RowNumber,
				ROW_NUMBER() OVER ( ORDER BY ServiceCategoryId desc   ) as ReverseRowNumber 
			from CM_ItemsHistory h
			where ContractId = @ContractId_parm 
			and h.Ordinality = ( select max(Ordinality) from CM_ItemsHistory where ContractId = @ContractId_parm and ItemId = h.ItemId ) ' + @searchWhere +									
			'	) x
			where x.RowNumber between @StartRow_parm and @EndRow_parm '

			select @error = @@error
	
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error assigning query string when retrieving historical service items for contract ' + @ContractNumber
				goto ERROREXIT
			END
		END
	END
	else -- BPA
	BEGIN
		select @query1 = 'select x.ItemId, x.Ordinality, x.CatalogNumber, x.ManufacturersCatalogNumber, x.ManufacturersName, x.LetterOfCommitmentDate, x.CommercialListPrice, x.CommercialPricelistDate, x.CommercialPricelistFOBTerms, x.ManufacturersCommercialListPrice, x.TrackingMechanism, x.AcquisitionCost, x.TypeOfContractor, 
			x.ItemDescription, x.[SIN], x.ServiceCategoryId, x.PackageAsPriced, x.CurrentPrice, x.PriceStartDate, x.PriceStopDate, x.HasBPA,
			x.ParentItemId, x.ParentCatalogNumber, x.ParentManufacturersCatalogNumber, x.ParentManufacturersName, x.ParentLetterOfCommitmentDate, x.ParentCommercialListPrice, x.ParentCommercialPricelistDate, x.ParentCommercialPricelistFOBTerms, x.ParentManufacturersCommercialListPrice, x.ParentTrackingMechanism, x.ParentAcquisitionCost, x.ParentTypeOfContractor, 
			x.ParentItemDescription, x.ParentActive,  x.ParentHistorical, x.ItemHistoryId, x.LastModificationType,  x.ModificationStatusId, x.LastModifiedBy, x.LastModificationDate,
			x.ReasonMovedToHistory, x.MovedToHistoryBy, x.DateMovedToHistory, x.Restorable,
			RowNumber + ReverseRowNumber - 1 as TotalRows
			from (
					select h.ItemId, 
					h.Ordinality,
					h.CatalogNumber, 
					h.ManufacturersCatalogNumber, 
					h.ManufacturersName, 
					h.LetterOfCommitmentDate, 
					h.CommercialListPrice, 
					h.CommercialPricelistDate, 
					h.CommercialPricelistFOBTerms, 
					h.ManufacturersCommercialListPrice, 					
					h.TrackingMechanism, 
					h.AcquisitionCost, 
					h.TypeOfContractor, 
					h.ItemDescription, 
					h.[SIN], 
					isnull( h.ServiceCategoryId, -1 ) as ServiceCategoryId,
					h.PackageAsPriced, 
					'''' as CurrentPrice,    -- price is not available for historical items
					'''' as PriceStartDate,
					'''' as PriceStopDate,			
					0 as HasBPA,	'
					
			select @query1b = 'case when d.ItemId is not null then d.ItemId else
						case when e.ItemId is not null then e.ItemId else
						-1 end end as ParentItemId,
					
					case when d.CatalogNumber is not null then d.CatalogNumber else
						case when e.CatalogNumber is not null then e.CatalogNumber else
						'''' end end as ParentCatalogNumber,

					case when d.ManufacturersCatalogNumber is not null then d.ManufacturersCatalogNumber else
						case when e.ManufacturersCatalogNumber is not null then e.ManufacturersCatalogNumber else
						'''' end end as ParentManufacturersCatalogNumber,

					case when d.ManufacturersName is not null then d.ManufacturersName else
						case when e.ManufacturersName is not null then e.ManufacturersName else
						'''' end end as ParentManufacturersName,

					case when d.LetterOfCommitmentDate is not null then d.LetterOfCommitmentDate else
						case when e.LetterOfCommitmentDate is not null then e.LetterOfCommitmentDate else
						'''' end end as ParentLetterOfCommitmentDate,

					case when d.CommercialListPrice is not null then d.CommercialListPrice else
						case when e.CommercialListPrice is not null then e.CommercialListPrice else
						0 end end as ParentCommercialListPrice,

					case when d.CommercialPricelistDate is not null then d.CommercialPricelistDate else
						case when e.CommercialPricelistDate is not null then e.CommercialPricelistDate else
						'''' end end as ParentCommercialPricelistDate,

					case when d.CommercialPricelistFOBTerms is not null then d.CommercialPricelistFOBTerms else
						case when e.CommercialPricelistFOBTerms is not null then e.CommercialPricelistFOBTerms else
						'''' end end as ParentCommercialPricelistFOBTerms,

					case when d.ManufacturersCommercialListPrice is not null then d.ManufacturersCommercialListPrice else
						case when e.ManufacturersCommercialListPrice is not null then e.ManufacturersCommercialListPrice else
						0 end end as ParentManufacturersCommercialListPrice,
						
					case when d.TrackingMechanism is not null then d.TrackingMechanism else
						case when e.TrackingMechanism is not null then e.TrackingMechanism else
						'''' end end as ParentTrackingMechanism,

					case when d.AcquisitionCost is not null then d.AcquisitionCost else
						case when e.AcquisitionCost is not null then e.AcquisitionCost else
						0 end end as ParentAcquisitionCost,

					case when d.TypeOfContractor is not null then d.TypeOfContractor else
						case when e.TypeOfContractor is not null then e.TypeOfContractor else
						'''' end end as ParentTypeOfContractor,

					case when d.ItemDescription is not null then d.ItemDescription else
						case when e.ItemDescription is not null then e.ItemDescription else
						'''' end end as ParentItemDescription,

					case when d.ItemId is not null then 1 else 0 end as ParentActive,
					case when e.ItemId is not null then 1 else 0 end as ParentHistorical,
														
					h.ItemHistoryId,
					h.LastModificationType, 
					h.ModificationStatusId, 
					h.LastModifiedBy, 
					h.LastModificationDate,   '

select @query2 = '		case when ( PATINDEX( ''%DeleteMedSurgItemAndPrices%'', h.Notes ) > 0 ) then ''Deleted'' else 
						case when ( PATINDEX( ''%UpdateMedSurgItem%'', h.Notes ) > 0 ) then ''Updated'' else 
						case when ( PATINDEX( ''%DeleteItemForMedSurgItemUpload2%'', h.Notes ) > 0 ) then ''Deleted via Upload''  else 
						case when ( PATINDEX( ''%UpdateItemForMedSurgItemUpload2%'', h.Notes ) > 0 ) then ''Updated via Upload'' else 
						case when ( PATINDEX( ''%Initial%'', h.Notes ) > 0 ) then ''Initial Conversion'' else 
						case when ( PATINDEX( ''%NightlyBatchProcess%'', h.Notes ) > 0 ) then ''All Prices Expired'' else 
						''Unknown'' end end end end end end as ReasonMovedToHistory,
					case when ( PATINDEX( ''%Initial%'', h.MovedToHistoryBy ) > 0 ) then ''Unknown'' else h.MovedToHistoryBy end as MovedToHistoryBy,
					h.DateMovedToHistory,
					case when ( PATINDEX( ''%DeleteMedSurgItemAndPrices%'', h.Notes ) > 0 or PATINDEX( ''%DeleteItemForMedSurgItemUpload2%'', h.Notes ) > 0 or PATINDEX( ''%NightlyBatchProcess%'', h.Notes ) > 0 ) then 1 else 0 end as Restorable,
					ROW_NUMBER() OVER ( ORDER BY d.CatalogNumber, e.CatalogNumber   ) as RowNumber,
					ROW_NUMBER() OVER ( ORDER BY d.CatalogNumber desc, e.CatalogNumber desc   ) as ReverseRowNumber 				
				from CM_ItemsHistory h left outer join
				( select z.ItemId, z.CatalogNumber, z.ManufacturersCatalogNumber, z.ManufacturersName, z.LetterOfCommitmentDate, 
					z.CommercialListPrice, z.CommercialPricelistDate, z.CommercialPricelistFOBTerms, 
					z.ManufacturersCommercialListPrice, z.TrackingMechanism, z.AcquisitionCost, z.TypeOfContractor, z.ItemDescription 
				 from CM_Items z where z.ContractId = @ParentContractId_parm ) d on h.ParentItemId = d.ItemId
				left outer join
				( select y.ItemId, y.CatalogNumber, y.ManufacturersCatalogNumber, y.ManufacturersName, y.LetterOfCommitmentDate, 
					y.CommercialListPrice, y.CommercialPricelistDate, y.CommercialPricelistFOBTerms, 
					y.ManufacturersCommercialListPrice, y.TrackingMechanism, y.AcquisitionCost, y.TypeOfContractor, y.ItemDescription 
				 from CM_ItemsHistory y where y.ContractId = @ParentContractId_parm
									and y.Ordinality = ( select max( j.Ordinality )
															from CM_ItemsHistory j
															where j.ItemId = y.ItemId )) e on h.ParentItemId = e.ItemId			
						
				where ContractId = @ContractId_parm 
				and h.Ordinality = ( select max(Ordinality) from CM_ItemsHistory where ContractId = @ContractId_parm and ItemId = h.ItemId ) ' + @searchWhere +									
			'	) x
			where x.RowNumber between @StartRow_parm and @EndRow_parm '

		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error assigning query string when retrieving historical BPA items for contract ' + @ContractNumber
			goto ERROREXIT
		END
	END

	select @SQLParms = N'@ContractId_parm int, @ParentContractId_parm int, @ContractNumber_parm nvarchar(20), @StartRow_parm int, @EndRow_parm int'

	select @orderBy = ' '

	select @query = @query1 + @query1b + @query2 + @orderBy

	exec SP_EXECUTESQL @query, @SQLParms, @ContractId_parm = @ContractId, @ParentContractId_parm = @ParentContractId, @ContractNumber_parm = @ContractNumber, @StartRow_parm = @StartRow, @EndRow_parm = @EndRow
																					
	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting historical items for contract.'
		goto ERROREXIT
	END
END

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



