IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSalesYearlyTotalsForContractReport' )
BEGIN
	DROP PROCEDURE SelectSalesYearlyTotalsForContractReport
END
GO

CREATE PROCEDURE SelectSalesYearlyTotalsForContractReport
(
@UserLogin nvarchar(120),
@ContractNumber nvarchar(20)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	/* copied from view view_Sales_Yearly_Totals */

	SELECT dbo.tbl_Cntrcts_Sales.CntrctNum, 
			dbo.tlkup_year_qtr.Year, 
			SUM(dbo.tbl_Cntrcts_Sales.VA_Sales) AS VA_Sales, 
            SUM(dbo.tbl_Cntrcts_Sales.OGA_Sales) AS OGA_Sales, 
			SUM(dbo.tbl_Cntrcts_Sales.SLG_Sales) AS SLG_Sales, 
            SUM(dbo.tbl_Cntrcts_Sales.VA_Sales + dbo.tbl_Cntrcts_Sales.OGA_Sales + dbo.tbl_Cntrcts_Sales.SLG_Sales) AS [Total Sales], 
            SUM(CAST(dbo.tbl_Cntrcts_Sales.VA_Sales * dbo.tbl_IFF.VA_IFF AS decimal(19, 2))) AS [VA IFF Amount], 
            SUM(CAST(dbo.tbl_Cntrcts_Sales.OGA_Sales * dbo.tbl_IFF.OGA_IFF AS decimal(19, 2))) AS [OGA IFF Amount], 
            SUM(CAST(dbo.tbl_Cntrcts_Sales.SLG_Sales * dbo.tbl_IFF.SLG_IFF AS decimal(19, 2))) AS [SLG IFF Amount], 
            SUM(CAST((dbo.tbl_Cntrcts_Sales.VA_Sales * dbo.tbl_IFF.VA_IFF + dbo.tbl_Cntrcts_Sales.OGA_Sales * dbo.tbl_IFF.OGA_IFF) 
                      + dbo.tbl_Cntrcts_Sales.SLG_Sales * dbo.tbl_IFF.SLG_IFF AS decimal(19, 2))) AS [Total IFF Amount], 
			dbo.tbl_IFF.VA_IFF, 
			dbo.tbl_IFF.OGA_IFF, 
			dbo.tbl_IFF.SLG_IFF
	FROM dbo.tbl_Cntrcts_Sales INNER JOIN dbo.tlkup_year_qtr ON dbo.tbl_Cntrcts_Sales.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID 
			INNER JOIN dbo.tbl_Cntrcts ON dbo.tbl_Cntrcts_Sales.CntrctNum = dbo.tbl_Cntrcts.CntrctNum 
			INNER JOIN dbo.[tlkup_Sched/Cat] ON dbo.tbl_Cntrcts.Schedule_Number = dbo.[tlkup_Sched/Cat].Schedule_Number 
			INNER JOIN dbo.tbl_IFF ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.tbl_IFF.Schedule_Number 
			AND dbo.tlkup_year_qtr.start_quarter_id = dbo.tbl_IFF.Start_Quarter_Id
	WHERE dbo.tbl_Cntrcts_Sales.CntrctNum = @ContractNumber
	GROUP BY dbo.tbl_Cntrcts_Sales.CntrctNum, 
			dbo.tlkup_year_qtr.Year, 
			dbo.tbl_IFF.VA_IFF, 
			dbo.tbl_IFF.OGA_IFF, 
			dbo.tbl_IFF.SLG_IFF
	ORDER BY dbo.tlkup_year_qtr.Year DESC



	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting sales for contract.'
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


