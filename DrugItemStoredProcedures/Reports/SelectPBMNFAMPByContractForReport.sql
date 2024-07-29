IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectPBMNFAMPByContractForReport')
	BEGIN
		DROP  Procedure  SelectPBMNFAMPByContractForReport
	END

GO

CREATE Procedure SelectPBMNFAMPByContractForReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@FutureHistoricalSelectionCriteria nchar(1)  -- F future, A active
)

AS

BEGIN TRANSACTION

DECLARE 	@error int,
			@errorMsg nvarchar(250),
			@currentYear int	

	/* log the request for the report */
	exec InsertDrugItemUserActivity @ReportUserLoginId, 'R', 'PBM NFAMP By Contract Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	select @currentYear = year(getdate())
	if( @FutureHistoricalSelectionCriteria = 'F' )
	BEGIN
		select @currentYear = @currentYear + 1
	END
	
	select 	f.ndc_1, 
			f.ndc_2, 
			f.ndc_3,
			f.n,
			f.QA_Exempt,
			f.Disc_Date, 
			f.FCP,
			f.CreationDate,
			case f.ErrorMsg when 'ContractId not found' then 'The contract was not found in the NACCM.'
				when 'NDCId not found' then 'The NDC was not found in the NACCM.'
				when 'DrugItemId not found' then 'No items with this NDC are active on this contract.' 
				else f.ErrorMsg
				end as ErrorMessage
		from DI_FCP f 
		where f.cnt_no = @ContractNumber
		and f.YearId = @currentYear
		order by f.ndc_1, f.ndc_2, f.ndc_3 

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting NFAMP items for contract report'
		goto ERROREXIT
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
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 

ENDEXIT:






	






