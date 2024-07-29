IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ExtractDataFromPBM]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ExtractDataFromPBM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create Proc [ExtractDataFromPBM]
As


Drop table fcpprice
Drop table fssprice
Drop table fssrpric
Drop table fssdata
Drop table ncfprice
Drop table bpaprice

--run the dts pacakge


update FSSData
set disc_date = Case 
					When Year(disc_date) < 1900 then null 
					else disc_date
				End ,
	disc_edat = Case 
					When Year(disc_edat) < 1900 then null 
					else disc_edat
				End ,
	pv_date = Case 
					When Year(pv_date) < 1900 then null 
					else pv_date
				End 

update FSSPrice
set Cnt_Start = Case 
					When Year(Cnt_Start) < 1900 then null 
					else Cnt_Start
				End ,
	Cnt_Stop = Case 
					When Year(Cnt_Stop) < 1900 then null 
					else Cnt_Stop
				End ,
	edate = Case 
					When Year(edate) < 1900 then null 
					else edate
				End,
	chg_date = Case 
					When Year(chg_date) < 1900 then null 
					else chg_date
				End, 
	pv_chg_dat = Case 
					When Year(pv_chg_dat) < 1900 then null 
					else pv_chg_dat
				End 

update FSSrPric
	set Cnt_Start = Case 
					When Year(Cnt_Start) < 1900 then null 
					else Cnt_Start
				End ,
	Cnt_Stop = Case 
					When Year(Cnt_Stop) < 1900 then null 
					else Cnt_Stop
				End ,
	edate = Case 
					When Year(edate) < 1900 then null 
					else edate
				End,
	chg_date = Case 
					When Year(chg_date) < 1900 then null 
					else chg_date
				End, 
	pv_chg_dat = Case 
					When Year(pv_chg_dat) < 1900 then null 
					else pv_chg_dat
				End 

update FCPPrice
set Cnt_Start = Case 
					When Year(Cnt_Start) < 1900 then null 
					else Cnt_Start
				End ,
	Cnt_Stop = Case 
					When Year(Cnt_Stop) < 1900 then null 
					else Cnt_Stop
				End ,
	edate = Case 
					When Year(edate) < 1900 then null 
					else edate
				End,
	chg_date = Case 
					When Year(chg_date) < 1900 then null 
					else chg_date
				End, 
	pv_chg_dat = Case 
					When Year(pv_chg_dat) < 1900 then null 
					else pv_chg_dat
				End 


update bpaprice
set Cnt_Start = Case 
					When Year(Cnt_Start) < 1900 then null 
					else Cnt_Start
				End ,
	Cnt_Stop = Case 
					When Year(Cnt_Stop) < 1900 then null 
					else Cnt_Stop
				End ,
	edate = Case 
					When Year(edate) < 1900 then null 
					else edate
				End,
	chg_date = Case 
					When Year(chg_date) < 1900 then null 
					else chg_date
				End, 
	pv_chg_dat = Case 
					When Year(pv_chg_dat) < 1900 then null 
					else pv_chg_dat
				End 


update bpaprice
set Cnt_Start = Case 
					When Year(Cnt_Start) < 1900 then null 
					else Cnt_Start
				End ,
	Cnt_Stop = Case 
					When Year(Cnt_Stop) < 1900 then null 
					else Cnt_Stop
				End ,
	edate = Case 
					When Year(edate) < 1900 then null 
					else edate
				End,
	chg_date = Case 
					When Year(chg_date) < 1900 then null 
					else chg_date
				End, 
	pv_chg_dat = Case 
					When Year(pv_chg_dat) < 1900 then null 
					else pv_chg_dat
				End 


CREATE NONCLUSTERED INDEX [IX_fcpprice_ndc_1] ON [dbo].[fcpprice] 
(
	[ndc_1] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_fcpprice_ndc_2] ON [dbo].[fcpprice] 
(
	[ndc_2] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_fcpprice_ndc_3] ON [dbo].[fcpprice] 
(
	[ndc_3] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_fssdata_NDC_1] ON [dbo].[fssdata] 
(
	[ndc_1] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_fssdata_NDC_2] ON [dbo].[fssdata] 
(
	[ndc_2] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_fssdata_NDC_3] ON [dbo].[fssdata] 
(
	[ndc_3] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_fssprice_ndc_1] ON [dbo].[fssprice] 
(
	[ndc_1] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_fssprice_ndc_2] ON [dbo].[fssprice] 
(
	[ndc_2] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_fssprice_ndc_3] ON [dbo].[fssprice] 
(
	[ndc_3] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_ncfprice_ndc_1] ON [dbo].[ncfprice] 
(
	[ndc_1] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_ncfprice_ndc_2] ON [dbo].[ncfprice] 
(
	[ndc_2] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_ncfprice_ndc_3] ON [dbo].[ncfprice] 
(
	[ndc_3] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

