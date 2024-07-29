IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSalesCheckComparison')
	BEGIN
		DROP  Procedure  SelectSalesCheckComparison
	END

GO

CREATE Procedure SelectSalesCheckComparison
(
@UserLogin nvarchar(120) = null,
@ContractNumber nvarchar(50)
)

AS

BEGIN

	select	convert( nvarchar(4), q.Year ) + convert( nchar(1), q.Qtr ) as YearQtr,
			q.Year, 
			q.Qtr, 
			m.VA + m.OGA + m.SLG as TotalSales, 
			ROUND( ( m.VA * i.VA_IFF ) + ( m.OGA * i.OGA_IFF ) + ( m.SLG * i.SLG_IFF ), 2 ) as TotalIFF, 
			k.PaymentAmount as CheckAmt, 
			isnull( PaymentAmount, 0 ) - (ROUND(( m.VA * i.VA_IFF ) + ( m.OGA * i.OGA_IFF ) + ( m.SLG * i.SLG_IFF ),2)) as Difference 
			from tlkup_year_qtr q join
					(
					select s.Quarter_ID,  
					sum(s.VA_Sales) as VA, 
					sum(s.OGA_Sales) as OGA, 
					sum(s.SLG_Sales) as SLG 
					from  tbl_Cntrcts_Sales s
					where s.CntrctNum = @ContractNumber
					group by s.Quarter_ID
					) m
	on q.Quarter_ID = m.Quarter_ID
	join tbl_Cntrcts c on c.CntrctNum = @ContractNumber
	join tbl_IFF i on m.Quarter_ID between i.Start_Quarter_Id and i.End_Quarter_Id 
									and i.Schedule_Number = c.Schedule_Number
	left outer join CM_PaymentsReceived k on k.ContractNumber = @ContractNumber
					and k.QuarterId = m.Quarter_ID
	order by Year desc, Qtr desc

END

