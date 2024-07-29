IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'QuarterlySalesReport')
	BEGIN
		DROP  Procedure  QuarterlySalesReport
	END

GO

CREATE Procedure QuarterlySalesReport
(
@DivisionId int,
@ScheduleNumber int,
@COId int,
@QuarterId int
)

AS

BEGIN

	create table #QuarterlySalesReportCurrent
	(
		ContractNumber nvarchar(50),
		COId int,
		ScheduleNumber int,
		DivisionId int,
		VASales money,
		OGASales money,
		SLGSales money,
		TotalSales money,
		VAPercentComparedToLastQuarter nvarchar(20), /* percent as a string, or N/A if no past data available */
		OGAPercentComparedToLastQuarter nvarchar(20),		
		SLGPercentComparedToLastQuarter nvarchar(20)
	)
	
	create table #QuarterlySalesReportLastQuarter
	(
		ContractNumber nvarchar(50),
		COId int,
		ScheduleNumber int,
		DivisionId int,
		VASales money,
		OGASales money,
		SLGSales money
	)
	
	create table #QuarterlySalesReportLastYear
	(
		ContractNumber nvarchar(50),
		COId int,
		ScheduleNumber int,
		DivisionId int,
		VASales money,
		OGASales money,
		SLGSales money
	)
	
	insert into #QuarterlySalesReportCurrent
	( ContractNumber, VASales, OGASales, SLGSales )
	select CntrctNum, VA_Sales, OGA_Sales, SLG_Sales 
	from tbl_Cntrcts_Sales
	where Quarter_ID = @QuarterId
	
	update #QuarterlySalesReportCurrent 
	set COId = CO_ID,
	ScheduleNumber = Schedule_Number
	from tbl_Cntrcts
	where CntrctNum = #QuarterlySalesReportCurrent.ContractNumber
	
	update #QuarterlySalesReportCurrent 
	set DivisionId = Division
	from [tlkup_Sched/Cat]
	where Schedule_Number = #QuarterlySalesReportCurrent.ScheduleNumber
	
	insert into #QuarterlySalesReportLastQuarter
	( ContractNumber, COId, ScheduleNumber, DivisionId )
	select ContractNumber, COId, ScheduleNumber, DivisionId 
	from #QuarterlySalesReportCurrent
	
	insert into #QuarterlySalesReportLastYear
	( ContractNumber, COId, ScheduleNumber, DivisionId )
	select ContractNumber, COId, ScheduleNumber, DivisionId 
	from #QuarterlySalesReportCurrent
	
	update #QuarterlySalesReportLastQuarter
	set VASales = VA_Sales, 
		OGASales = OGA_Sales, 
		SLGSales = SLG_Sales
	from tbl_Cntrcts_Sales
	where Quarter_ID = @QuarterId - 1

	update #QuarterlySalesReportLastYear
	set VASales = VA_Sales, 
		OGASales = OGA_Sales, 
		SLGSales = SLG_Sales
	from tbl_Cntrcts_Sales
	where Quarter_ID = @QuarterId - 4

	/* comparison calculations */
	update #QuarterlySalesReportCurrent
	set TotalSales = #QuarterlySalesReportCurrent.VASales + #QuarterlySalesReportCurrent.OGASales + #QuarterlySalesReportCurrent.SLGSales,
		VAPercentComparedToLastQuarter = dbo.CalculatePercentAsStringFunction( #QuarterlySalesReportCurrent.VASales, #QuarterlySalesReportLastQuarter.VASales, 'N/A' ),
		OGAPercentComparedToLastQuarter = dbo.CalculatePercentAsStringFunction( #QuarterlySalesReportCurrent.OGASales, #QuarterlySalesReportLastQuarter.OGASales, 'N/A' ),
		SLGPercentComparedToLastQuarter = dbo.CalculatePercentAsStringFunction( #QuarterlySalesReportCurrent.SLGSales, #QuarterlySalesReportLastQuarter.SLGSales, 'N/A' )
	from #QuarterlySalesReportLastQuarter
	where #QuarterlySalesReportCurrent.ContractNumber = #QuarterlySalesReportLastQuarter.ContractNumber
	

	select * from #QuarterlySalesReportCurrent


END

