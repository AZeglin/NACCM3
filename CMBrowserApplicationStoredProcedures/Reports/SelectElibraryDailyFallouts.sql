IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectElibraryDailyFallouts')
	BEGIN
		DROP  Procedure  SelectElibraryDailyFallouts
	END

GO

CREATE Procedure SelectElibraryDailyFallouts
AS

BEGIN

	select Contract, Julian, Contractor, Elibrary_Schedule, Address1, Address2, City, State, Zip, POC_Primary_Phone
	from ELibraryDailyFallouts
	order by Contract

END
