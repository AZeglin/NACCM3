IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetOfferInfo')
	BEGIN
		DROP  Procedure  GetOfferInfo
	END

GO

CREATE Procedure GetOfferInfo
(
@CurrentUser uniqueidentifier,
@OfferId int
)

AS
-- this version no longer used
DECLARE @error int,
		@rowcount int,
		@errorMsg nvarchar(200),
		@rebateRequired bit

BEGIN

	select @rebateRequired = 0

	select o.Offer_ID as OfferId,
			o.CO_ID as AssignedCOID,
			s.Division as DivisionId,
			o.Schedule_Number as ScheduleNumber,
			@rebateRequired as RebateRequired,
			o.Contractor_Name as VendorName,
			o.Primary_Address_1 as VendorAddress1,
			o.Primary_Address_2 as VendorAddress2,
			o.Primary_City as VendorAddressCity,
			o.Primary_State as VendorAddressState,
			o.Primary_Zip as VendorZipCode,
			o.POC_VendorWeb as VendorUrl,
			o.POC_Primary_Name as VendorContactName,
			o.POC_Primary_Phone as VendorContactPhone,
			o.POC_Primary_Ext as VendorContactPhoneExtension,
			o.POC_Primary_Fax as VendorContactFax,
			o.POC_Primary_Email as VendorContactEmail,
			u.FullName as OwnerName,
			s.Schedule_Name as ScheduleName,
			o.OfferNumber, 
			o.Action_ID,
			a.Complete as IsOfferComplete,
			o.Proposal_Type_ID as ProposalTypeId,
			o.Dates_Received as DateOfferReceived,
			o.Dates_Assigned as DateOfferAssigned,
			o.Dates_Reassigned,
			o.Dates_Action,
			o.ContractNumber,
			o.ExtendsContractNumber
	from tbl_Offers o join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number
	join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
	join NACSEC.dbo.SEC_UserProfile u on o.CO_ID = u.CO_ID
	where Offer_ID = @OfferId			

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0 or @rowcount <> 1
	BEGIN
			Select @errorMsg = 'Error encountered obtaining information for offer : ' + @OfferId
			raiserror( @errorMsg, 16, 1 )
	END
END
