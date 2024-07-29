IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetOfferInfo2')
	BEGIN
		DROP  Procedure  GetOfferInfo2
	END

GO

CREATE Procedure GetOfferInfo2
(
@CurrentUser uniqueidentifier,
@OfferId int
)

AS

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
			o.Solicitation_ID as SolicitationId,
			x.Solicitation_Number as SolicitationNumber,
			@rebateRequired as RebateRequired,
			o.Contractor_Name as VendorName,
			o.Primary_Address_1 as VendorAddress1,
			o.Primary_Address_2 as VendorAddress2,
			o.Primary_City as VendorAddressCity,
			o.Primary_CountryId as VendorAddressCountryId,
			z.CountryName as VendorAddressCountryName,
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
			c.Contract_Record_ID as ContractId,
			o.ExtendsContractNumber,
			d.Contract_Record_ID as ExtendsContractId
	from tbl_Offers o join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number
	join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
	join tlkup_Solicitation_Numbers x on o.Solicitation_ID = x.Solicitation_ID
	join NACSEC.dbo.SEC_UserProfile u on o.CO_ID = u.CO_ID
	left outer join tbl_Cntrcts c on c.CntrctNum = o.ContractNumber
	left outer join tbl_Cntrcts d on d.CntrctNum = o.ExtendsContractNumber
	left outer join CM_Countries z on o.Primary_CountryId = z.CountryId
	where o.Offer_ID = @OfferId			

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0 or @rowcount <> 1
	BEGIN
			Select @errorMsg = 'Error encountered obtaining information for offer (2) : ' + @OfferId
			raiserror( @errorMsg, 16, 1 )
	END
END
