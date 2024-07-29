IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectParentContractsForSchedule' )
BEGIN
	DROP PROCEDURE SelectParentContractsForSchedule
END
GO

CREATE PROCEDURE SelectParentContractsForSchedule
(
@CurrentUser uniqueidentifier,
@COID int,
@LoginId nvarchar(120),
@BPAScheduleNumber int
)

AS

Declare 	@parentScheduleNumber int,
		@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	-- select all active fss contracts that are in the corresponding parent schedule
	if @BPAScheduleNumber = 53 
	BEGIN

		select c.CntrctNum, c.Schedule_Number, s.Schedule_Name, p.FullName, c.CO_ID, c.Contractor_Name, v.SAMUEI, c.DUNS, c.Drug_Covered, c.Dates_CntrctAward,
			c.Dates_Effective, c.Dates_CntrctExp, c.Dates_Completion
		from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
			join NACSEC.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
			left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId
		where c.Schedule_Number = 34 
		and dbo.IsContractActiveFunction( c.CntrctNum, GETDATE() ) = 1

		union
	
		select '-- select --' as CntrctNum, -1 as Schedule_Number, '' as Schedule_Name, '' as FullName, -1 as CO_ID, '' as Contractor_Name, '' as SAMUEI, '' as DUNS, '' as Drug_Covered, GETDATE() as Dates_CntrctAward,
			GETDATE() as Dates_Effective, GETDATE() as Dates_CntrctExp, GETDATE() as Dates_Completion

		order by CntrctNum

	END
	else if @BPAScheduleNumber = 39 or @BPAScheduleNumber = 48 or @BPAScheduleNumber = 52
	BEGIN

		select c.CntrctNum, c.Schedule_Number, s.Schedule_Name, p.FullName, c.CO_ID, c.Contractor_Name, v.SAMUEI, c.DUNS, c.Drug_Covered, c.Dates_CntrctAward,
			c.Dates_Effective, c.Dates_CntrctExp, c.Dates_Completion
		from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
			join NACSEC.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
			left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId
		where c.Schedule_Number = 1
		and dbo.IsContractActiveFunction( c.CntrctNum, GETDATE() ) = 1

		union
	
		select '-- select --' as CntrctNum, -1 as Schedule_Number, '' as Schedule_Name, '' as FullName, -1 as CO_ID, '' as Contractor_Name, '' as SAMUEI, '' as DUNS, '' as Drug_Covered, GETDATE() as Dates_CntrctAward,
			GETDATE() as Dates_Effective, GETDATE() as Dates_CntrctExp, GETDATE() as Dates_Completion

		order by CntrctNum

	END
	-- parent usually 34 but sometimes 1
	else if @BPAScheduleNumber = 15
	BEGIN

		select c.CntrctNum, c.Schedule_Number, s.Schedule_Name, p.FullName, c.CO_ID, c.Contractor_Name, v.SAMUEI, c.DUNS, c.Drug_Covered, c.Dates_CntrctAward,
			c.Dates_Effective, c.Dates_CntrctExp, c.Dates_Completion
		from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
			join NACSEC.dbo.SEC_UserProfile p on c.CO_ID = p.CO_ID
			left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId
		where c.Schedule_Number in ( 1, 34 )
		and dbo.IsContractActiveFunction( c.CntrctNum, GETDATE() ) = 1

		union
	
		select '-- select --' as CntrctNum, 100 as Schedule_Number, '' as Schedule_Name, '' as FullName, -1 as CO_ID, '' as Contractor_Name, '' as SAMUEI, '' as DUNS, '' as Drug_Covered, GETDATE() as Dates_CntrctAward,
			GETDATE() as Dates_Effective, GETDATE() as Dates_CntrctExp, GETDATE() as Dates_Completion

		order by Schedule_Number desc, CntrctNum asc

	END
	else
	BEGIN
		select '-- select --' as CntrctNum, -1 as Schedule_Number, '' as Schedule_Name, '' as FullName, -1 as CO_ID, '' as Contractor_Name, '' as SAMUEI, '' as DUNS, '' as Drug_Covered, GETDATE() as Dates_CntrctAward,
			GETDATE() as Dates_Effective, GETDATE() as Dates_CntrctExp, GETDATE() as Dates_Completion

	END


	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting parent contracts for selected BPA schedule.'
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


