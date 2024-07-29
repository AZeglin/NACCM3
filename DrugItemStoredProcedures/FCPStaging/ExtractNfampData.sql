 select  ndc_1,ndc_2,ndc_3,
		Case 
			When len(n)=0 or n is null then null
			Else N
		End as n,
		cnt_no,fcp,YearID,
		disc_date,
		qa_exempt,
		A	
 From nfamp
where 
(YEAR(getdate()) = YearID or YEAR(GETDATE())+1 = yearid or YEAR(GETDATE())-1 = yearid)
order by yearid
