IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetOfferInfoDetails' )
BEGIN
	DROP PROCEDURE GetOfferInfoDetails
END
GO

CREATE PROCEDURE GetOfferInfoDetails
(
@CurrentUser uniqueidentifier,
@OfferId int
)

AS

DECLARE @error int,
		@rowcount int,
		@errorMsg nvarchar(200)

BEGIN

	select o.Offer_ID as OfferId,
			o.Solicitation_ID as SolicitationId,
			o.Action_ID as ActionId,
			d.Action_Description as OfferActionDescription,
			d.Complete as IsOfferComplete,

			o.Contractor_Name as VendorName,
			o.Primary_Address_1 as VendorAddress1,
			o.Primary_Address_2 as VendorAddress2,
			o.Primary_City as VendorAddressCity,
			o.Primary_State as VendorAddressState,
			o.Primary_Zip as VendorZipCode,
			o.Country as VendorCountry,
			o.Primary_CountryId as VendorCountryId,

			o.POC_Primary_Name as VendorContactName,
			o.POC_Primary_Phone as VendorContactPhone,
			o.POC_Primary_Ext as VendorContactPhoneExtension,
			o.POC_Primary_Fax as VendorContactFax,
			o.POC_Primary_Email as VendorContactEmail,
			
			o.POC_VendorWeb as VendorUrl,
			
			o.Dates_Received as DateReceived,
			o.Dates_Assigned as DateAssigned,
			o.Dates_Reassigned as DateReassigned,
			o.Dates_Action as ActionDate,
			o.Dates_Expected_Completion as ExpectedCompletionDate,
			o.Dates_Expiration as ExpirationDate,
			o.Dates_Sent_for_Preaward as DateSentForPreAward,
			o.Dates_Returned_to_Office as DateReturnedToOffice,
			
			o.Comments as OfferComment,
			o.Audit_Indicator as AuditIndicator,
			
			o.CreatedBy as CreatedBy,
		    o.Date_Entered as CreationDate,
			o.LastModifiedBy as LastModifiedBy,
			o.Date_Modified as LastModificationDate,
			 
			t.Solicitation_Number as SolicitationNumber,
			x.User_Phone as ContractingOfficerPhone,  -- CO of the offer
			s.Schedule_Manager,
			s.Asst_Director,
			m.FullName as ScheduleManagerName,  
			a.FullName as AssistantDirectorName,

			c.Dates_CntrctAward as ContractAwardDate,
			c.Dates_CntrctExp as ContractExpirationDate,
			c.Dates_Completion as ContractCompletionDate,
			u.FullName as AwardedContractingOfficerFullName
		                   	
	from tbl_Offers o join tlkup_Solicitation_Numbers t on o.Solicitation_ID = t.Solicitation_ID
		join tlkup_Offers_Action_Type d on o.Action_ID = d.Action_ID
		join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number
		left outer join tbl_Cntrcts c on o.ContractNumber = c.CntrctNum
		left outer join NACSEC.dbo.SEC_UserProfile u on c.CO_ID = u.CO_ID
		left outer join NACSEC.dbo.SEC_UserProfile x on o.CO_ID = x.CO_ID
		left outer join NACSEC.dbo.SEC_UserProfile m on s.Schedule_Manager = m.CO_ID
		left outer join NACSEC.dbo.SEC_UserProfile a on s.Asst_Director = a.CO_ID
	where o.Offer_ID = @OfferId			

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0 or @rowcount <> 1
	BEGIN
			Select @errorMsg = 'Error encountered obtaining information for offer : ' + @OfferId
			raiserror( @errorMsg, 16, 1 )
	END
END
