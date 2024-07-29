IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectDetailedSalesTotalsForContractReport' )
BEGIN
	DROP PROCEDURE SelectDetailedSalesTotalsForContractReport
END
GO

CREATE PROCEDURE SelectDetailedSalesTotalsForContractReport
(
@UserLogin nvarchar(120),
@ContractNumber nvarchar(20)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

/* copied from view_sales_full_totals */

BEGIN TRANSACTION

	SELECT dbo.tbl_Cntrcts_Sales.CntrctNum, 
			dbo.tlkup_year_qtr.Year, 
			dbo.tlkup_year_qtr.Qtr, 
			dbo.tbl_Cntrcts_Sales.SIN, 
            SUM(dbo.tbl_Cntrcts_Sales.VA_Sales) AS [VA Sales], 
			SUM(dbo.tbl_Cntrcts_Sales.OGA_Sales) AS [OGA Sales], 
			SUM(dbo.tbl_Cntrcts_Sales.SLG_Sales) AS [SLG Sales], 
            SUM(dbo.tbl_Cntrcts_Sales.VA_Sales + dbo.tbl_Cntrcts_Sales.OGA_Sales + dbo.tbl_Cntrcts_Sales.SLG_Sales) AS [Total Sales], 
            SUM(dbo.tbl_Cntrcts_Sales.VA_Sales * dbo.tbl_IFF.VA_IFF) AS [VA IFF Amount], 
			SUM(dbo.tbl_Cntrcts_Sales.OGA_Sales * dbo.tbl_IFF.OGA_IFF) AS [OGA IFF Amount], 
            SUM(dbo.tbl_Cntrcts_Sales.SLG_Sales * dbo.tbl_IFF.SLG_IFF) AS [SLG IFF Amount], 
            SUM(dbo.tbl_Cntrcts_Sales.VA_Sales * dbo.tbl_IFF.VA_IFF + dbo.tbl_Cntrcts_Sales.OGA_Sales * dbo.tbl_IFF.OGA_IFF + dbo.tbl_Cntrcts_Sales.SLG_Sales * dbo.tbl_IFF.SLG_IFF) AS [Total IFF Amount], 
			dbo.tbl_IFF.VA_IFF, 
			dbo.tbl_IFF.OGA_IFF, 
			dbo.tbl_IFF.SLG_IFF
	FROM dbo.tbl_Cntrcts_Sales INNER JOIN dbo.tlkup_year_qtr ON dbo.tbl_Cntrcts_Sales.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID 
			INNER JOIN dbo.tbl_Cntrcts ON dbo.tbl_Cntrcts_Sales.CntrctNum = dbo.tbl_Cntrcts.CntrctNum 
			INNER JOIN dbo.[tlkup_Sched/Cat] ON dbo.tbl_Cntrcts.Schedule_Number = dbo.[tlkup_Sched/Cat].Schedule_Number 
			INNER JOIN dbo.tbl_IFF ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.tbl_IFF.Schedule_Number AND dbo.tlkup_year_qtr.start_quarter_id = dbo.tbl_IFF.Start_Quarter_Id
	where dbo.tbl_Cntrcts_Sales.CntrctNum = @ContractNumber
	GROUP BY dbo.tbl_Cntrcts_Sales.CntrctNum, 
				dbo.tlkup_year_qtr.Year, 
				dbo.tlkup_year_qtr.Qtr, 
				dbo.tbl_IFF.VA_IFF, 
				dbo.tbl_IFF.OGA_IFF, 
				dbo.tbl_IFF.SLG_IFF, 
                dbo.tbl_Cntrcts_Sales.SIN
	ORDER BY dbo.tlkup_year_qtr.Year DESC, dbo.tlkup_year_qtr.Qtr DESC, dbo.tbl_Cntrcts_Sales.SIN DESC


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting detailed sales for report.'
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


