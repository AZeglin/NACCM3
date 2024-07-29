IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectItemsWithSpecialtyDistributorReport' )
BEGIN
	DROP PROCEDURE SelectItemsWithSpecialtyDistributorReport
END
GO

CREATE PROCEDURE SelectItemsWithSpecialtyDistributorReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@Active nchar(1) /* 'A' active , 'D' discontinued, 'B' both */
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	if @Active = 'B'
	BEGIN
		select d.DrugItemDistributorId, d.DrugItemId,  c.ContractNumber, z.Dates_CntrctExp, x.ContractNumber as BPAContractNumber, n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode, i.Generic, i.TradeName, d.DistributorName, d.ContactPerson, d.Phone, d.Notes, i.DiscontinuationDate
		from DI_DrugItemDistributors d join DI_DrugItems i on d.DrugItemId = i.DrugItemId
		join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
		join DI_Contracts c on i.ContractId = c.ContractId
		join NAC_CM.dbo.tbl_Cntrcts z on z.CntrctNum = c.NACCMContractNumber
		left outer join DI_DrugItems p on p.ParentDrugItemId = i.DrugItemId
		left outer join DI_Contracts x on p.ContractId = x.ContractId

		order by c.ContractNumber, n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode
	END
	else
	BEGIN
		if @Active = 'A'
		BEGIN
			select d.DrugItemDistributorId, d.DrugItemId,  c.ContractNumber, z.Dates_CntrctExp,  x.ContractNumber as BPAContractNumber, n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode, i.Generic, i.TradeName, d.DistributorName, d.ContactPerson, d.Phone, d.Notes, i.DiscontinuationDate
			from DI_DrugItemDistributors d join DI_DrugItems i on d.DrugItemId = i.DrugItemId
			join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
			join DI_Contracts c on i.ContractId = c.ContractId
			join NAC_CM.dbo.tbl_Cntrcts z on z.CntrctNum = c.NACCMContractNumber
			left outer join DI_DrugItems p on p.ParentDrugItemId = i.DrugItemId
			left outer join DI_Contracts x on p.ContractId = x.ContractId
			where i.DiscontinuationDate is null

			order by c.ContractNumber, n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode
		END
		else
		BEGIN
			select d.DrugItemDistributorId, d.DrugItemId,  c.ContractNumber, z.Dates_CntrctExp,  x.ContractNumber as BPAContractNumber, n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode, i.Generic, i.TradeName, d.DistributorName, d.ContactPerson, d.Phone, d.Notes, i.DiscontinuationDate
			from DI_DrugItemDistributors d join DI_DrugItems i on d.DrugItemId = i.DrugItemId
			join DI_DrugItemNDC n on i.DrugItemNDCId = n.DrugItemNDCId
			join DI_Contracts c on i.ContractId = c.ContractId
			join NAC_CM.dbo.tbl_Cntrcts z on z.CntrctNum = c.NACCMContractNumber
			left outer join DI_DrugItems p on p.ParentDrugItemId = i.DrugItemId
			left outer join DI_Contracts x on p.ContractId = x.ContractId
			where i.DiscontinuationDate is not null

			order by c.ContractNumber, n.FdaAssignedLabelerCode, n.ProductCode, n.PackageCode
		END
	END


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting distributors for report.'
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


