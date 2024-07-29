IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'OffersByCreationDateReport' )
BEGIN
	DROP PROCEDURE OffersByCreationDateReport
END
GO

CREATE PROCEDURE OffersByCreationDateReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@StartingCreationDate datetime
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	select o.Contractor_Name, u.FullName as COName, s.Schedule_Name, a.Action_Description, p.Proposal_Type_Description, 
		o.POC_Primary_Name, o.POC_Primary_Phone, POC_Primary_Email, Dates_Received, Dates_Assigned, Dates_Reassigned, Dates_Action, Dates_Expected_Completion, Dates_Expiration,
		o.Dates_Sent_for_Preaward, o.Dates_Returned_to_Office, o.Audit_Indicator, o.ContractNumber, o.Date_Entered, o.Date_Modified, o.CreatedBy, o.LastModifiedBy, o.Comments
		  from tbl_Offers o join tlkup_UserProfile u on o.CO_ID = u.CO_ID
	join tlkup_Offers_Action_Type a on a.Action_ID = o.Action_ID
	join tlkup_Offers_Proposal_Type p on p.Proposal_Type_ID = o.Proposal_Type_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number
	where  Date_Entered >= @StartingCreationDate
	order by Date_Entered

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting offers for report.'
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


