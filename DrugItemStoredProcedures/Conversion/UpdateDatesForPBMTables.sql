IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateDatesForPBMTables')
	BEGIN
		DROP  Procedure  UpdateDatesForPBMTables
	END

GO

CREATE Proc [dbo].[UpdateDatesForPBMTables]
As

	Update FSSData
		Set disc_date = null
	Where Year(disc_date) = 1899

	Update FSSData
		Set disc_edat = null
	Where Year(disc_edat) = 1899

	Update FSSData
		Set pv_date = null
	Where Year(pv_date) = 1899	

		
	Update FSSPrice
		Set cnt_start = null
	Where Year(cnt_start) = 1899	

	Update FSSPrice
		Set cnt_stop = null
	Where Year(cnt_stop) = 1899

	Update FSSPrice
		Set edate = null
	Where Year(edate) = 1899		
		
	Update FSSPrice
		Set chg_date = null
	Where Year(chg_date) = 1899			

	Update FSSPrice
		Set pv_chg_dat = null
	Where Year(pv_chg_dat) = 1899		


	Update FcpPrice
		Set cnt_start = null
	Where Year(cnt_start) = 1899		

	Update FcpPrice
		Set cnt_stop = null
	Where Year(cnt_stop) = 1899		
		
	Update FcpPrice
		Set edate = null
	Where Year(edate) = 1899	

	Update FcpPrice
		Set chg_date = null
	Where Year(chg_date) = 1899	
		
	Update FcpPrice
		Set pv_chg_dat = null
	Where Year(pv_chg_dat) = 1899



	Update FSSRPric
		Set cnt_start = null
	Where Year(cnt_start) = 1899		

	Update FSSRPric
		Set cnt_stop = null
	Where Year(cnt_stop) = 1899		
		
	Update FSSRPric
		Set edate = null
	Where Year(edate) = 1899	

	Update FSSRPric
		Set chg_date = null
	Where Year(chg_date) = 1899	
		
	Update FSSRPric
		Set pv_chg_dat = null
	Where Year(pv_chg_dat) = 1899		



	Update ncfprice
		Set cnt_start = null
	Where Year(cnt_start) = 1899	

	Update ncfprice
		Set cnt_stop = null
	Where Year(cnt_stop) = 1899		
		
	Update ncfprice
		Set edate = null
	Where Year(edate) = 1899	

	Update ncfprice
		Set chg_date = null
	Where Year(chg_date) = 1899	
		
	Update ncfprice
		Set pv_chg_dat = null
	Where Year(pv_chg_dat) = 1899	


	Update ndclink
		Set eff_Date = null
	Where Year(eff_Date) = 1899

	Update ndclink
		Set edate = null
	Where Year(edate) = 1899	

	
/*	Update nc
	Set cnt_stop = null
	Where Year(cnt_stop) = 1899		
		
	Update nc
		Set cnt_start = null
	Where Year(cnt_start) = 1899
*/
