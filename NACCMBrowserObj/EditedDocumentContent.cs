using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.NACCMBrowser.BrowserObj;


namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // content for the current document as it is edited
    [Serializable]
    public class EditedDocumentContent
    {
        CurrentDocument _currentDocument = null;
        bool _bContractCreationInProgress = false;

        public EditedDocumentContent()
        {
            _currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];

            // current document does not exist during contract creation
            if( _currentDocument != null )
            {
                _contractNumber = _currentDocument.ContractNumber;
                _contractDescription = _currentDocument.Description;

                _scheduleNumber = _currentDocument.ScheduleNumber;
                _divisionId = CurrentDocument.GetDivisionIdFromDivision( _currentDocument.Division );

                _COID = _currentDocument.OwnerId;

                if( _currentDocument.HasParent == true )
                {
                    _parentFSSContractNumber = _currentDocument.ParentDocument.ContractNumber;
                    _parentContractId = _currentDocument.ParentDocument.ContractId;
                }

                _vendorName = _currentDocument.VendorName;

                _contractAwardDate = _currentDocument.AwardDate;
                _contractEffectiveDate = _currentDocument.EffectiveDate;
                _contractExpirationDate = _currentDocument.ExpirationDate;
                _contractCompletionDate = _currentDocument.CompletionDate;
            }
            else
            {
                _bContractCreationInProgress = true;
            }

            _geographicCoverage = new GeographicCoverage();

            _returnedGoodsPolicyTypes = new ArrayList();
            _returnedGoodsPolicyTypes.Add( new ReturnedGoodsPolicy( 3, "None Selected" ) );
            _returnedGoodsPolicyTypes.Add( new ReturnedGoodsPolicy( 1, "Government" ) );
            _returnedGoodsPolicyTypes.Add( new ReturnedGoodsPolicy( 2, "Commercial" ) );
            
            _dirtyFlags = new DocumentDirtyFlags();
            ClearDirtyFlags();
        }

        public void RefreshCurrentDocumentDatesFromEditedDocument()
        {
            // setting a date sets ActiveStatus
             _currentDocument.AwardDate = _contractAwardDate;
             _currentDocument.EffectiveDate = _contractEffectiveDate;
             _currentDocument.ExpirationDate = _contractExpirationDate;
             _currentDocument.CompletionDate = _contractCompletionDate;
        }

        public void RefreshCurrentDocumentAttributesFromEditedDocument()
        {
            _currentDocument.Description = _contractDescription;
            _currentDocument.ContractNumber = _contractNumber;   // used during insert
        }

        public void RefreshCurrentDocumentAssignmentFromEditedDocument()
        {
            _currentDocument.OwnerId = _COID;
            _currentDocument.OwnerName = _contractingOfficerFullName;

            // update the current document edit status due to the new owner
            BrowserSecurity2 browserSecurity = ( BrowserSecurity2 )HttpContext.Current.Session[ "BrowserSecurity" ];
            if( browserSecurity != null )
            {
                browserSecurity.SetDocumentEditStatus( _currentDocument );
            }
        }

        public void RefreshCurrentDocumentCountryInfoFromEditedDocument()
        {
            // note only the id is refreshed. the name in the current document is only used for bpa parent and will not be edited when viewing the child
            // the name for display comes from this object
            _currentDocument.VendorCountryId = _vendorCountryId;
        }

        public bool CompleteContractDetails( DataSet dsContractDetails )
        {
            bool bSuccess = false;
            if( dsContractDetails != null )
            {
                if( dsContractDetails.Tables[ "ContractDetailsTable" ] != null )
                {
                    if( dsContractDetails.Tables[ "ContractDetailsTable" ].Rows.Count > 0 )
                    {
                        DataRow row = dsContractDetails.Tables[ "ContractDetailsTable" ].Rows[ 0 ];
                        
                        if( row != null )
                        {
                            try
                            {
                                CopyRowToObjectMembers( row );
 
                                bSuccess = true;

                                ClearDirtyFlags();
                            }
                            catch( Exception ex )
                            {
                                _errorMessage = string.Format( "Exception encountered when copying row to object members: {0}", ex.Message );
                            }
                        }
                        else
                        {
                            _errorMessage = "Row returned from contract details table was null.";
                        }
                    }
                    else
                    {
                        _errorMessage = "Contract details table had zero rows.";
                    }
                }
                else
                {
                    _errorMessage = "Contract details table was not found in the returned recordset.";
                }
            }
            else
            {
                _errorMessage = "The recordset returned was null.";
            }

            return( bSuccess );
        }

        public bool InitContractFromOffer( DataSet dsOneOfferRow )
        {
            bool bSuccess = false;
            if( dsOneOfferRow != null )
            {
                if( dsOneOfferRow.Tables[ "OneOfferRowTable" ] != null )
                {
                    if( dsOneOfferRow.Tables[ "OneOfferRowTable" ].Rows.Count > 0 )
                    {
                        DataRow row = dsOneOfferRow.Tables[ "OneOfferRowTable" ].Rows[ 0 ];

                        if( row.GetType() != typeof( DBNull ) )     // old way was if row != null
                        {                
                            try
                            {
                                CopyOfferRowToObjectMembers( row );

                                bSuccess = true;

                                ClearDirtyFlags();
                            }
                            catch( Exception ex )
                            {
                                _errorMessage = string.Format( "Exception encountered when copying offer row to object members: {0}", ex.Message );
                            }
                        }
                        else
                        {
                            _errorMessage = "Row returned from contract details table was null.";
                        }
                    }
                    else
                    {
                        _errorMessage = "Contract details table had zero rows.";
                    }
                }
                else
                {
                    _errorMessage = "Contract details table was not found in the returned recordset.";
                }
            }
            else
            {
                _errorMessage = "The recordset returned was null.";
            }

            return ( bSuccess );
        }

        // copy from sourceObj to this
        public void CopyFrom( EditedDocumentContent sourceObj )
        {

            _contractNumber = sourceObj.ContractNumber;
            _contractDescription = sourceObj.ContractDescription;
            _scheduleNumber = sourceObj.ScheduleNumber;
            _COID = sourceObj.COID;

            _parentFSSContractNumber = sourceObj.ParentFSSContractNumber;
            _parentContractId = sourceObj.ParentContractId;
          
            _vendorName = sourceObj.VendorName;

            _contractAwardDate = sourceObj.ContractAwardDate;
            _contractEffectiveDate = sourceObj.ContractEffectiveDate;
            _contractExpirationDate = sourceObj.ContractExpirationDate;
            _contractCompletionDate = sourceObj.ContractCompletionDate;

            _contractId = sourceObj.ContractId;
            _pharmaceuticalContractId = sourceObj.PharmaceuticalContractId;
            _SAMUEI = sourceObj.SAMUEI;
            _DUNS = sourceObj.DUNS;
            _TIN = sourceObj.TIN;
            _bPrimeVendorParticipation = sourceObj.PrimeVendorParticipation;

            _solicitationNumber = sourceObj.SolicitationNumber;
            _vendorAddress1 = sourceObj.VendorAddress1;

            _vendorAddress2 = sourceObj.VendorAddress2;
            _vendorCity = sourceObj.VendorCity;
            _vendorCountryId = sourceObj.VendorCountryId;
            _vendorCountryName = sourceObj.VendorCountryName;
            _vendorState = sourceObj.VendorState;

            _vendorZip = sourceObj.VendorZip;
            _vendorWebAddress = sourceObj.VendorWebAddress;

            _vendorPrimaryContactName = sourceObj.VendorPrimaryContactName;
            _vendorPrimaryContactPhone = sourceObj.VendorPrimaryContactPhone;
            _vendorPrimaryContactExtension = sourceObj.VendorPrimaryContactExtension;
            _vendorPrimaryContactFax = sourceObj.VendorPrimaryContactFax;
            _vendorPrimaryContactEmail = sourceObj.VendorPrimaryContactEmail;
            _generalContractNotes = sourceObj.GeneralContractNotes;

            _vendorAlternateContactName = sourceObj.VendorAlternateContactName;
            _vendorAlternateContactPhone = sourceObj.VendorAlternateContactPhone;
            _vendorAlternateContactExtension = sourceObj.VendorAlternateContactExtension;
            _vendorAlternateContactFax = sourceObj.VendorAlternateContactFax;
            _vendorAlternateContactEmail = sourceObj.VendorAlternateContactEmail;
            _vendorEmergencyContactName = sourceObj.VendorEmergencyContactName;
            _vendorEmergencyContactPhone = sourceObj.VendorEmergencyContactPhone;

            _vendorEmergencyContactExtension = sourceObj.VendorEmergencyContactExtension;
            _vendorEmergencyContactFax = sourceObj.VendorEmergencyContactFax;
            _vendorEmergencyContactEmail = sourceObj.VendorEmergencyContactEmail;
            _vendorTechnicalContactName = sourceObj.VendorTechnicalContactName;
            _vendorTechnicalContactPhone = sourceObj.VendorTechnicalContactPhone;
            _vendorTechnicalContactExtension = sourceObj.VendorTechnicalContactExtension;
            _vendorTechnicalContactFax = sourceObj.VendorTechnicalContactFax;
            _vendorTechnicalContactEmail = sourceObj.VendorTechnicalContactEmail;


            _socioVetStatusId = sourceObj.SocioVetStatusId;
            _socioBusinessSizeId = sourceObj.SocioBusinessSizeId;
            _bSocioSDB = sourceObj.SocioSDB;
            _bSocio8a = sourceObj.Socio8a;
            _bSocioWomanOwned = sourceObj.SocioWomanOwned;
            _bHubZone = sourceObj.HubZone;

            _basicDiscount = sourceObj.BasicDiscount;
            _creditCardDiscount = sourceObj.CreditCardDiscount;
            _promptPayDiscount = sourceObj.PromptPayDiscount;
            _quantityDiscount = sourceObj.QuantityDiscount;

 
            _geographicCoverageId = sourceObj.GeographicCoverageId;
    

            _trackingCustomerName = sourceObj.TrackingCustomerName;
            _minimumOrder = sourceObj.MinimumOrder;
            _deliveryTerms = sourceObj.DeliveryTerms;
            _expeditedDeliveryTerms = sourceObj.ExpeditedDeliveryTerms;
            _endOfYearDiscount = sourceObj.EndOfYearDiscount;
            _FPRFreeFormatDateString = sourceObj.FPRFreeFormatDateString;
            _bCreditCardAccepted = sourceObj.CreditCardAccepted;
            _bHazardousMaterial = sourceObj.HazardousMaterial;
            _warrantyDuration = sourceObj.WarrantyDuration;
            _warrantyNotes = sourceObj.WarrantyNotes;

            _iffTypeId = sourceObj.IffTypeId;
    

            _ratio = sourceObj.Ratio;

            _returnedGoodsPolicyTypeId = sourceObj.ReturnedGoodsPolicyTypeId;


            _returnedGoodsPolicyNotes = sourceObj.ReturnedGoodsPolicyNotes;
            _additionalDiscount = sourceObj.AdditionalDiscount;

            _vendorTypeId = sourceObj.VendorTypeId;


            _orderingAddress1 = sourceObj.OrderingAddress1;
            _orderingAddress2 = sourceObj.OrderingAddress2;
            _orderingCity = sourceObj.OrderingCity;
            _orderingCountryId = sourceObj.OrderingCountryId;
            _orderingState = sourceObj.OrderingState;
            _orderingZip = sourceObj.OrderingZip;
            _orderingTelephone = sourceObj.OrderingTelephone;
            _orderingExtension = sourceObj.OrderingExtension;
            _orderingFax = sourceObj.OrderingFax;
            _orderingEmail = sourceObj.OrderingEmail;

            _estimatedContractValue = sourceObj.EstimatedContractValue;

            _totalOptionYears = sourceObj.TotalOptionYears;


            _bPricelistVerified = sourceObj.PricelistVerified;

            _pricelistVerificationDate = sourceObj.PricelistVerificationDate;


            _pricelistVerifiedBy = sourceObj.PricelistVerifiedBy;
            _currentModNumber = sourceObj.CurrentModNumber;
            _pricelistNotes = sourceObj.PricelistNotes;

            _SBAPlanId = sourceObj.SBAPlanId;

            _bVADOD = sourceObj.VADOD;

            _bTerminatedByConvenience = sourceObj.TerminatedByConvenience;
            _bTerminatedByDefault = sourceObj.TerminatedByDefault;

            _bSBAPlanExempt = sourceObj.SBAPlanExempt;

            _insurancePolicyEffectiveDate = sourceObj.InsurancePolicyEffectiveDate;
 
            _insurancePolicyExpirationDate = sourceObj.InsurancePolicyExpirationDate;

            _solicitationId = sourceObj.SolicitationId;
 
            _offerId = sourceObj.OfferId;
   
            _65IBContractType = sourceObj.ServiceContractType;
    

            _vendorSalesContactName = sourceObj.VendorSalesContactName;
            _vendorSalesContactPhone = sourceObj.VendorSalesContactPhone;
            _vendorSalesContactExtension = sourceObj.VendorSalesContactExtension;
            _vendorSalesContactFax = sourceObj.VendorSalesContactFax;
            _vendorSalesContactEmail = sourceObj.VendorSalesContactEmail;

            _tradeAgreementActCompliance = sourceObj.TradeAgreementActCompliance;
            _stimulusActId = ( sourceObj.StimulusAct == true ) ? 1 : 0; 
            _bRebateRequired = sourceObj.RebateRequired;
            _bStandardized = sourceObj.Standardized;

            _createdBy = sourceObj.CreatedBy;
            _creationDate = sourceObj.CreationDate;
            _lastModifiedBy = sourceObj.LastModifiedBy;
            _lastModificationDate = sourceObj.LastModificationDate;

            _contractingOfficerFullName = sourceObj.ContractingOfficerFullName;
            _contractingOfficerPhone = sourceObj.ContractingOfficerPhone;
            _scheduleName = sourceObj.ScheduleName;
            _assistantDirectorCOID = sourceObj.AssistantDirectorCOID;
            _seniorContractSpecialistCOID = sourceObj.SeniorContractSpecialistCOID;
            _assistantDirectorName = sourceObj.AssistantDirectorName;
            _seniorContractSpecialistName = sourceObj.SeniorContractSpecialistName;

            _dirtyFlags = null;
            _dirtyFlags = new DocumentDirtyFlags( sourceObj.DirtyFlags );

            _geographicCoverage = null;
            _geographicCoverage = new GeographicCoverage( sourceObj.GeographicCoverage );

        }

        private void CopyRowToObjectMembers( DataRow row )
        {
            _contractId = int.Parse( row[ "Contract_Record_ID" ].ToString() );
            _SAMUEI = row[ "SAMUEI" ].ToString();
            _DUNS = row[ "DUNS" ].ToString();
            _TIN = row[ "TIN" ].ToString();
            _bPrimeVendorParticipation = bool.Parse( row[ "PV_Participation" ].ToString() );

            if( row[ "Solicitation_Number" ] != DBNull.Value )
            {
                _solicitationNumber = row[ "Solicitation_Number" ].ToString();
            }
            _vendorAddress1 = row[ "Primary_Address_1" ].ToString();

            _vendorAddress2 = row[ "Primary_Address_2" ].ToString();
            _vendorCity = row[ "Primary_City" ].ToString();
            _vendorState = row[ "Primary_State" ].ToString();
            if( row[ "Primary_CountryId" ] != DBNull.Value )
            {
                _vendorCountryId = int.Parse( row[ "Primary_CountryId" ].ToString() );
            }
            if( row[ "CountryName" ] != DBNull.Value )
            {
                _vendorCountryName = row[ "CountryName" ].ToString();
            }

            _vendorZip = row[ "Primary_Zip" ].ToString();
            _vendorWebAddress = row[ "POC_VendorWeb" ].ToString();

            _vendorPrimaryContactName = row[ "POC_Primary_Name" ].ToString();
            _vendorPrimaryContactPhone = row[ "POC_Primary_Phone" ].ToString();
            _vendorPrimaryContactExtension = row[ "POC_Primary_Ext" ].ToString();
            _vendorPrimaryContactFax = row[ "POC_Primary_Fax" ].ToString();
            _vendorPrimaryContactEmail = row[ "POC_Primary_Email" ].ToString();
            _generalContractNotes = row[ "POC_Notes" ].ToString();

            _vendorAlternateContactName = row[ "POC_Alternate_Name" ].ToString();
            _vendorAlternateContactPhone = row[ "POC_Alternate_Phone" ].ToString();
            _vendorAlternateContactExtension = row[ "POC_Alternate_Ext" ].ToString();
            _vendorAlternateContactFax = row[ "POC_Alternate_Fax" ].ToString();
            _vendorAlternateContactEmail = row[ "POC_Alternate_Email" ].ToString();
            _vendorEmergencyContactName = row[ "POC_Emergency_Name" ].ToString();
            _vendorEmergencyContactPhone = row[ "POC_Emergency_Phone" ].ToString();

            _vendorEmergencyContactExtension = row[ "POC_Emergency_Ext" ].ToString();
            _vendorEmergencyContactFax = row[ "POC_Emergency_Fax" ].ToString();
            _vendorEmergencyContactEmail = row[ "POC_Emergency_Email" ].ToString();
            _vendorTechnicalContactName = row[ "POC_Tech_Name" ].ToString();
            _vendorTechnicalContactPhone = row[ "POC_Tech_Phone" ].ToString();
            _vendorTechnicalContactExtension = row[ "POC_Tech_Ext" ].ToString();
            _vendorTechnicalContactFax = row[ "POC_Tech_Fax" ].ToString();
            _vendorTechnicalContactEmail = row[ "POC_Tech_Email" ].ToString();


            _socioVetStatusId = int.Parse( row[ "Socio_VetStatus_ID" ].ToString() );
            _socioBusinessSizeId = int.Parse( row[ "Socio_Business_Size_ID" ].ToString() );
            _bSocioSDB = bool.Parse( row[ "Socio_SDB" ].ToString() );
            _bSocio8a = bool.Parse( row[ "Socio_8a" ].ToString() );
            _bSocioWomanOwned = bool.Parse( row[ "Socio_Woman" ].ToString() );
            _bHubZone = bool.Parse( row[ "Socio_HubZone" ].ToString() );

            _basicDiscount = row[ "Discount_Basic" ].ToString();
            _creditCardDiscount = row[ "Discount_Credit_Card" ].ToString();
            _promptPayDiscount = row[ "Discount_Prompt_Pay" ].ToString();
            _quantityDiscount = row[ "Discount_Quantity" ].ToString();

            if( row[ "Geographic_Coverage_ID" ] != DBNull.Value )
            {
                _geographicCoverageId = int.Parse( row[ "Geographic_Coverage_ID" ].ToString() );
            }
            
            _trackingCustomerName = row[ "Tracking_Customer" ].ToString();
            _minimumOrder = row[ "Mininum_Order" ].ToString();
            _deliveryTerms = row[ "Delivery_Terms" ].ToString();
            _expeditedDeliveryTerms = row[ "Expedited_Delivery_Terms" ].ToString();
            _endOfYearDiscount = row[ "Annual_Rebate" ].ToString();
            _FPRFreeFormatDateString = row[ "BF_Offer" ].ToString();
            _bCreditCardAccepted = bool.Parse( row[ "Credit_Card_Accepted" ].ToString() );
            _bHazardousMaterial = bool.Parse( row[ "Hazard" ].ToString() );
            _warrantyDuration = row[ "Warranty_Duration" ].ToString();
            _warrantyNotes = row[ "Warranty_Notes" ].ToString();
            
            if( row[ "IFF_Type_ID" ] != DBNull.Value )
            {
                _iffTypeId = int.Parse( row[ "IFF_Type_ID" ].ToString() );
            }

            _ratio = row[ "Ratio" ].ToString();

            if( row[ "Returned_Goods_Policy_Type" ] != DBNull.Value )
            {
                _returnedGoodsPolicyTypeId = int.Parse( row[ "Returned_Goods_Policy_Type" ].ToString() );
            }

            _returnedGoodsPolicyNotes = row[ "Returned_Goods_Policy_Notes" ].ToString();
            _additionalDiscount = row[ "Incentive_Description" ].ToString();

            if( row[ "Dist_Manf_ID" ] != DBNull.Value )
            {
                _vendorTypeId = int.Parse( row[ "Dist_Manf_ID" ].ToString() );
            }

            _orderingAddress1 = row[ "Ord_Address_1" ].ToString();
            _orderingAddress2 = row[ "Ord_Address_2" ].ToString();
            _orderingCity = row[ "Ord_City" ].ToString();
            if( row[ "Ord_CountryId" ] != DBNull.Value )
            {
                _orderingCountryId = int.Parse( row[ "Ord_CountryId" ].ToString() );
            }
            _orderingState = row[ "Ord_State" ].ToString();
            _orderingZip = row[ "Ord_Zip" ].ToString();
            _orderingTelephone = row[ "Ord_Telephone" ].ToString();
            _orderingExtension = row[ "Ord_Ext" ].ToString();
            _orderingFax = row[ "Ord_Fax" ].ToString();
            _orderingEmail = row[ "Ord_EMail" ].ToString();

            if( row[ "Estimated_Contract_Value" ] != DBNull.Value )
            {
                _estimatedContractValue = decimal.Parse( row[ "Estimated_Contract_Value" ].ToString() );
            }

            if( row[ "Dates_TotOptYrs" ] != DBNull.Value )
            {
                _totalOptionYears = int.Parse( row[ "Dates_TotOptYrs" ].ToString() );
            }

            _bPricelistVerified = bool.Parse( row[ "Pricelist_Verified" ].ToString() );

            if( row[ "Verification_Date" ] != DBNull.Value )
            {
                _pricelistVerificationDate = DateTime.Parse( row[ "Verification_Date" ].ToString() );
            }

            _pricelistVerifiedBy = row[ "Verified_By" ].ToString();
            _currentModNumber = row[ "Current_Mod_Number" ].ToString();
            _pricelistNotes = row[ "Pricelist_Notes" ].ToString();

            if( row[ "SBAPlanID" ] != DBNull.Value )
            {
                _SBAPlanId = int.Parse( row[ "SBAPlanID" ].ToString() );
            }

            _bVADOD = bool.Parse( row[ "VA_DOD" ].ToString() );

            _bTerminatedByConvenience = bool.Parse( row[ "Terminated_Convenience" ].ToString() );
            _bTerminatedByDefault = bool.Parse( row[ "Terminated_Default" ].ToString() );

            _bSBAPlanExempt = bool.Parse( row[ "SBA_Plan_Exempt" ].ToString() );

            if( row[ "Insurance_Policy_Effective_Date" ] != DBNull.Value )
            {
                _insurancePolicyEffectiveDate = DateTime.Parse( row[ "Insurance_Policy_Effective_Date" ].ToString() );
            }

            if( row[ "Insurance_Policy_Expiration_Date" ] != DBNull.Value )
            {
                _insurancePolicyExpirationDate = DateTime.Parse( row[ "Insurance_Policy_Expiration_Date" ].ToString() );
            }

            if( row[ "Solicitation_ID" ] != DBNull.Value )
            {
                _solicitationId = int.Parse( row[ "Solicitation_ID" ].ToString() );
            }

            if( row[ "Offer_ID" ] != DBNull.Value )
            {
                _offerId = int.Parse( row[ "Offer_ID" ].ToString() );
            }

            if( row[ "65IB_Contract_Type" ] != DBNull.Value )
            {
                _65IBContractType = int.Parse( row[ "65IB_Contract_Type" ].ToString() );
            }

            _vendorSalesContactName = row[ "POC_Sales_Name" ].ToString();
            _vendorSalesContactPhone = row[ "POC_Sales_Phone" ].ToString();
            _vendorSalesContactExtension = row[ "POC_Sales_Ext" ].ToString();
            _vendorSalesContactFax = row[ "POC_Sales_Fax" ].ToString();
            _vendorSalesContactEmail = row[ "POC_Sales_Email" ].ToString();

            _tradeAgreementActCompliance = row[ "TradeAgreementActCompliance" ].ToString();
            _stimulusActId = int.Parse( row[ "StimulusAct" ].ToString() );
            _bRebateRequired = bool.Parse( row[ "RebateRequired" ].ToString() );
            _bStandardized = bool.Parse( row[ "Standardized" ].ToString() );

            _createdBy = row[ "CreatedBy" ].ToString();
            _creationDate = DateTime.Parse( row[ "CreationDate" ].ToString() );
            _lastModifiedBy = row[ "LastModifiedBy" ].ToString();
            _lastModificationDate = DateTime.Parse( row[ "LastModificationDate" ].ToString() );

            _contractingOfficerFullName = row[ "FullName" ].ToString();
            _contractingOfficerPhone = row[ "User_Phone" ].ToString();
            _scheduleName = row[ "Schedule_Name" ].ToString();

            if( row[ "Asst_Director" ] != DBNull.Value )
            {
                _assistantDirectorCOID = int.Parse( row[ "Asst_Director" ].ToString() );
            }

            if( row[ "Schedule_Manager" ] != DBNull.Value )
            {
                _seniorContractSpecialistCOID = int.Parse( row[ "Schedule_Manager" ].ToString() );
            }

            _assistantDirectorName = row[ "AssistantDirectorName" ].ToString();  
            _seniorContractSpecialistName = row[ "ScheduleManagerName" ].ToString();  
        }

        // used during contract creation from offer
        private void CopyOfferRowToObjectMembers( DataRow row )
        {
            if( row[ "AssignedCOID" ] != DBNull.Value )
                _COID = int.Parse( row[ "AssignedCOID" ].ToString());
            if( row[ "DivisionId" ] != DBNull.Value )
                _divisionId = int.Parse( row[ "DivisionId" ].ToString());
            if( row[ "ScheduleNumber" ] != DBNull.Value )
                // overwriting scheduleNumber and offerId passed on query string
                _scheduleNumber = int.Parse( row[ "ScheduleNumber" ].ToString());
            if( row[ "OfferId" ] != DBNull.Value )
                _offerId = int.Parse( row[ "OfferId" ].ToString() );
            //  adding solicitation 3/5/21
            if( row[ "SolicitationId" ] != DBNull.Value )
                _solicitationId = int.Parse( row[ "SolicitationId" ].ToString() );
            if( row[ "SolicitationNumber" ] != DBNull.Value )
                _solicitationNumber = ( string )row[ "SolicitationNumber" ].ToString();
            if( row[ "RebateRequired" ] != DBNull.Value )
                _bRebateRequired = bool.Parse( row[ "RebateRequired" ].ToString());
            if( row[ "VendorName" ] != DBNull.Value )
                _vendorName = ( string )row[ "VendorName" ].ToString();
            if( row[ "VendorAddress1" ] != DBNull.Value )
                _vendorAddress1 = ( string )row[ "VendorAddress1" ].ToString();
            if( row[ "VendorAddress2" ] != DBNull.Value )
                _vendorAddress2 = ( string )row[ "VendorAddress2" ].ToString();
            if( row[ "VendorAddressCity" ] != DBNull.Value )
                _vendorCity = ( string )row[ "VendorAddressCity" ].ToString();
            if( row[ "VendorAddressCountryId" ] != DBNull.Value )
                _vendorCountryId = int.Parse( row[ "VendorAddressCountryId" ].ToString() );
            if( row[ "VendorAddressCountryName" ] != DBNull.Value )
                _vendorCountryName = ( string )row[ "VendorAddressCountryName" ].ToString();
            if( row[ "VendorAddressState" ] != DBNull.Value )
                _vendorState = ( string )row[ "VendorAddressState" ].ToString();
            if( row[ "VendorZipCode" ] != DBNull.Value )
                _vendorZip = ( string )row[ "VendorZipCode" ].ToString();
            if( row[ "VendorUrl"  ] != DBNull.Value )
                _vendorWebAddress = ( string )row[ "VendorUrl" ].ToString();
            if( row[ "VendorContactName" ] != DBNull.Value )
                _vendorPrimaryContactName = ( string )row[ "VendorContactName" ].ToString();
            if( row[ "VendorContactPhone" ] != DBNull.Value )
                _vendorPrimaryContactPhone = ( string )row[ "VendorContactPhone" ].ToString();
            if( row[ "VendorContactPhoneExtension" ] != DBNull.Value )
                _vendorPrimaryContactExtension = ( string )row[ "VendorContactPhoneExtension" ].ToString();
            if( row[ "VendorContactFax" ] != DBNull.Value )
                _vendorPrimaryContactFax = ( string )row[ "VendorContactFax" ].ToString();
            if( row[ "VendorContactEmail" ] != DBNull.Value )
                _vendorPrimaryContactEmail = ( string )row[ "VendorContactEmail" ].ToString();
            
        }

        private string _errorMessage = "";

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        [Serializable]
        public class DocumentDirtyFlags
        {

            // set this to true when data stored in the current document, including parent contract information, has changed.
            // use it to resync the current document object
            private bool _bIsContractGeneralContractDatesDirty = false;

            public bool IsContractGeneralContractDatesDirty
            {
                get { return _bIsContractGeneralContractDatesDirty; }
                set { _bIsContractGeneralContractDatesDirty = value; }
            }

            private bool _bIsContractGeneralContractAssignmentDirty = false;

            public bool IsContractGeneralContractAssignmentDirty
            {
                get { return _bIsContractGeneralContractAssignmentDirty; }
                set { _bIsContractGeneralContractAssignmentDirty = value; }
            }

            private bool _bIsContractGeneralContractAttributesDirty = false;

            public bool IsContractGeneralContractAttributesDirty
            {
                get { return _bIsContractGeneralContractAttributesDirty; }
                set { _bIsContractGeneralContractAttributesDirty = value; }
            }

            // currently not editable
            private bool _bIsContractGeneralParentContractDirty = false;

            public bool IsContractGeneralParentContractDirty
            {
                get { return _bIsContractGeneralParentContractDirty; }
                set { _bIsContractGeneralParentContractDirty = value; }
            }

            // special flag for all contract header info, associated with various fields spread across all contract tabs
            private bool _bIsContractHeaderInfoDirty = false;

            public bool IsContractHeaderInfoDirty
            {
                get { return _bIsContractHeaderInfoDirty; }
                set { _bIsContractHeaderInfoDirty = value; }
            }

            // special flag for info in personal pane, associated with contract dates, contract creation and sales addition
            private bool _bIsPersonalizedNotificationPaneDirty = false;

            public bool IsPersonalizedNotificationPaneDirty
            {
                get { return _bIsPersonalizedNotificationPaneDirty; }
                set { _bIsPersonalizedNotificationPaneDirty = value; }
            }

            private bool _bIsContractVendorSocioDirty = false;

            public bool IsContractVendorSocioDirty
            {
                get { return _bIsContractVendorSocioDirty; }
                set { _bIsContractVendorSocioDirty = value; }
            }

            private bool _bIsContractVendorAttributesDirty = false;

            public bool IsContractVendorAttributesDirty
            {
                get { return _bIsContractVendorAttributesDirty; }
                set { _bIsContractVendorAttributesDirty = value; }
            }

            private bool _bIsContractVendorInsurancePolicyDirty = false;

            public bool IsContractVendorInsurancePolicyDirty
            {
                get { return _bIsContractVendorInsurancePolicyDirty; }
                set { _bIsContractVendorInsurancePolicyDirty = value; }
            }

            private bool _bIsWarrantyInformationDirty = false;

            public bool IsWarrantyInformationDirty
            {
                get { return _bIsWarrantyInformationDirty; }
                set { _bIsWarrantyInformationDirty = value; }
            }

            private bool _bIsReturnedGoodsPolicyInformationDirty = false;

            public bool IsReturnedGoodsPolicyInformationDirty
            {
                get { return _bIsReturnedGoodsPolicyInformationDirty; }
                set { _bIsReturnedGoodsPolicyInformationDirty = value; }
            }

            private bool _bIsGeographicCoverageDirty = false;

            public bool IsGeographicCoverageDirty
            {
                get { return _bIsGeographicCoverageDirty; }
                set { _bIsGeographicCoverageDirty = value; }
            }

            private bool _bIsContractDetailsAttributesDirty = false;

            public bool IsContractDetailsAttributesDirty
            {
                get { return _bIsContractDetailsAttributesDirty; }
                set { _bIsContractDetailsAttributesDirty = value; }
            }

            private bool _bIsContractDetailsDiscountDirty = false;

            public bool IsContractDetailsDiscountDirty
            {
                get { return _bIsContractDetailsDiscountDirty; }
                set { _bIsContractDetailsDiscountDirty = value; }
            }

            private bool _bIsContractDetailsDeliveryDirty = false;

            public bool IsContractDetailsDeliveryDirty
            {
                get { return _bIsContractDetailsDeliveryDirty; }
                set { _bIsContractDetailsDeliveryDirty = value; }
            }

            private bool _bIsVendorContractAdministratorDirty = false;

            public bool IsVendorContractAdministratorDirty
            {
                get { return _bIsVendorContractAdministratorDirty; }
                set { _bIsVendorContractAdministratorDirty = value; }
            }

            private bool _bIsVendorAlternateContactDirty = false;

            public bool IsVendorAlternateContactDirty
            {
                get { return _bIsVendorAlternateContactDirty; }
                set { _bIsVendorAlternateContactDirty = value; }
            }

            private bool _bIsVendorTechnicalContactDirty = false;

            public bool IsVendorTechnicalContactDirty
            {
                get { return _bIsVendorTechnicalContactDirty; }
                set { _bIsVendorTechnicalContactDirty = value; }
            }

            private bool _bIsVendorEmergencyContactDirty = false;

            public bool IsVendorEmergencyContactDirty
            {
                get { return _bIsVendorEmergencyContactDirty; }
                set { _bIsVendorEmergencyContactDirty = value; }
            }

            private bool _bIsVendorOrderingContactDirty = false;

            public bool IsVendorOrderingContactDirty
            {
                get { return _bIsVendorOrderingContactDirty; }
                set { _bIsVendorOrderingContactDirty = value; }
            }

            private bool _bIsVendorSalesContactDirty = false;

            public bool IsVendorSalesContactDirty
            {
                get { return _bIsVendorSalesContactDirty; }
                set { _bIsVendorSalesContactDirty = value; }
            }

            private bool _bIsVendorBusinessAddressDirty = false;

            public bool IsVendorBusinessAddressDirty
            {
                get { return _bIsVendorBusinessAddressDirty; }
                set { _bIsVendorBusinessAddressDirty = value; }
            }

            // currently not editable
            private bool _bIsItemPriceCountsDirty = false;

            public bool IsItemPriceCountsDirty
            {
                get { return _bIsItemPriceCountsDirty; }
                set { _bIsItemPriceCountsDirty = value; }
            }

            private bool _bIsPricelistVerificationDirty = false;

            public bool IsPricelistVerificationDirty
            {
                get { return _bIsPricelistVerificationDirty; }
                set { _bIsPricelistVerificationDirty = value; }
            }

            // this is the form view with the edit/export buttons
            // currently not editable
            private bool _bIsPricelistFormViewDirty = false;

            public bool IsPricelistFormViewDirty
            {
                get { return _bIsPricelistFormViewDirty; }
                set { _bIsPricelistFormViewDirty = value; }
            }

            private bool _bIsPricelistNotesDirty = false;

            public bool IsPricelistNotesDirty
            {
                get { return _bIsPricelistNotesDirty; }
                set { _bIsPricelistNotesDirty = value; }
            }

            private bool _bIsRebateDirty = false;

            public bool IsRebateDirty
            {
                get { return _bIsRebateDirty; }
                set { _bIsRebateDirty = value; }
            }

            // this includes the exempt check box and the current selected plan
            private bool _bIsSBAHeaderDirty = false;

            public bool IsSBAHeaderDirty
            {
                get { return _bIsSBAHeaderDirty; }
                set { _bIsSBAHeaderDirty = value; }
            }

            private bool _bIsSBAPlanDetailsDirty = false;

            public bool IsSBAPlanDetailsDirty
            {
                get { return _bIsSBAPlanDetailsDirty; }
                set { _bIsSBAPlanDetailsDirty = value; }
            }

            private bool _bIsContractCommentDirty = false;

            public bool IsContractCommentDirty
            {
                get { return _bIsContractCommentDirty; }
                set { _bIsContractCommentDirty = value; }
            }

            // CreateContract2 screen

            private bool _bIsCreateContractFormViewDirty = false;

            public bool IsCreateContractFormViewDirty
            {
                get { return _bIsCreateContractFormViewDirty; }
                set { _bIsCreateContractFormViewDirty = value; }
            }

            private bool _bIsVendorPOCFormViewDirty = false;

            public bool IsVendorPOCFormViewDirty
            {
                get { return _bIsVendorPOCFormViewDirty; }
                set { _bIsVendorPOCFormViewDirty = value; }
            }

            private bool _bIsVendorAddressFormViewDirty = false;

            public bool IsVendorAddressFormViewDirty
            {
                get { return _bIsVendorAddressFormViewDirty; }
                set { _bIsVendorAddressFormViewDirty = value; }
            }


            //$$$FormViewAddition

            public void CopyFrom( DocumentDirtyFlags sourceObj )
            {
                _bIsContractGeneralContractDatesDirty = sourceObj.IsContractGeneralContractDatesDirty;
                _bIsContractGeneralContractAttributesDirty = sourceObj.IsContractGeneralContractAttributesDirty;
                _bIsContractGeneralContractAssignmentDirty = sourceObj.IsContractGeneralContractAssignmentDirty;
                _bIsContractGeneralParentContractDirty = sourceObj.IsContractGeneralParentContractDirty;
                _bIsContractHeaderInfoDirty = sourceObj.IsContractHeaderInfoDirty;

                _bIsContractVendorSocioDirty = sourceObj.IsContractVendorSocioDirty;
                _bIsContractVendorAttributesDirty = sourceObj.IsContractVendorAttributesDirty;
                _bIsContractVendorInsurancePolicyDirty = sourceObj.IsContractVendorInsurancePolicyDirty;
                _bIsWarrantyInformationDirty = sourceObj.IsWarrantyInformationDirty;
                _bIsReturnedGoodsPolicyInformationDirty = sourceObj.IsReturnedGoodsPolicyInformationDirty;
                _bIsGeographicCoverageDirty = sourceObj.IsGeographicCoverageDirty;

                _bIsContractDetailsAttributesDirty = sourceObj.IsContractDetailsAttributesDirty;
                _bIsContractDetailsDiscountDirty = sourceObj.IsContractDetailsDiscountDirty;
                _bIsContractDetailsDeliveryDirty = sourceObj.IsContractDetailsDeliveryDirty;

                _bIsVendorContractAdministratorDirty = sourceObj.IsVendorContractAdministratorDirty;
                _bIsVendorAlternateContactDirty = sourceObj.IsVendorAlternateContactDirty;
                _bIsVendorTechnicalContactDirty = sourceObj.IsVendorTechnicalContactDirty;
                _bIsVendorEmergencyContactDirty = sourceObj.IsVendorEmergencyContactDirty;
                _bIsVendorOrderingContactDirty = sourceObj.IsVendorOrderingContactDirty;
                _bIsVendorSalesContactDirty = sourceObj.IsVendorSalesContactDirty;
                _bIsVendorBusinessAddressDirty = sourceObj.IsVendorBusinessAddressDirty;

                _bIsItemPriceCountsDirty = sourceObj.IsItemPriceCountsDirty;
                _bIsPricelistVerificationDirty = sourceObj.IsPricelistVerificationDirty;
                _bIsPricelistFormViewDirty = sourceObj.IsPricelistFormViewDirty;
                _bIsPricelistNotesDirty = sourceObj.IsPricelistNotesDirty;

                _bIsRebateDirty = sourceObj.IsRebateDirty;

                _bIsSBAHeaderDirty = sourceObj.IsSBAHeaderDirty;
                _bIsSBAPlanDetailsDirty = sourceObj.IsSBAPlanDetailsDirty;
                _bIsContractCommentDirty = sourceObj.IsContractCommentDirty;

                _bIsCreateContractFormViewDirty = sourceObj.IsCreateContractFormViewDirty;
                _bIsVendorPOCFormViewDirty = sourceObj.IsVendorPOCFormViewDirty;
                _bIsVendorAddressFormViewDirty = sourceObj.IsVendorAddressFormViewDirty;
            }

            public DocumentDirtyFlags()
            {
            }

            public DocumentDirtyFlags( DocumentDirtyFlags sourceObj )
            {
                _bIsContractGeneralContractDatesDirty = sourceObj.IsContractGeneralContractDatesDirty;
                _bIsContractGeneralContractAttributesDirty = sourceObj.IsContractGeneralContractAttributesDirty;
                _bIsContractGeneralContractAssignmentDirty = sourceObj.IsContractGeneralContractAssignmentDirty;
                _bIsContractGeneralParentContractDirty = sourceObj.IsContractGeneralParentContractDirty;
                _bIsContractHeaderInfoDirty = sourceObj.IsContractHeaderInfoDirty;

                _bIsContractVendorSocioDirty = sourceObj.IsContractVendorSocioDirty;
                _bIsContractVendorAttributesDirty = sourceObj.IsContractVendorAttributesDirty;
                _bIsContractVendorInsurancePolicyDirty = sourceObj.IsContractVendorInsurancePolicyDirty;
                _bIsWarrantyInformationDirty = sourceObj.IsWarrantyInformationDirty;
                _bIsReturnedGoodsPolicyInformationDirty = sourceObj.IsReturnedGoodsPolicyInformationDirty;
                _bIsGeographicCoverageDirty = sourceObj.IsGeographicCoverageDirty;

                _bIsContractDetailsAttributesDirty = sourceObj.IsContractDetailsAttributesDirty;
                _bIsContractDetailsDiscountDirty = sourceObj.IsContractDetailsDiscountDirty;
                _bIsContractDetailsDeliveryDirty = sourceObj.IsContractDetailsDeliveryDirty;

                _bIsVendorContractAdministratorDirty = sourceObj.IsVendorContractAdministratorDirty;
                _bIsVendorAlternateContactDirty = sourceObj.IsVendorAlternateContactDirty;
                _bIsVendorTechnicalContactDirty = sourceObj.IsVendorTechnicalContactDirty;
                _bIsVendorEmergencyContactDirty = sourceObj.IsVendorEmergencyContactDirty;
                _bIsVendorOrderingContactDirty = sourceObj.IsVendorOrderingContactDirty;
                _bIsVendorSalesContactDirty = sourceObj.IsVendorSalesContactDirty;
                _bIsVendorBusinessAddressDirty = sourceObj.IsVendorBusinessAddressDirty;

                _bIsItemPriceCountsDirty = sourceObj.IsItemPriceCountsDirty;
                _bIsPricelistVerificationDirty = sourceObj.IsPricelistVerificationDirty;
                _bIsPricelistFormViewDirty = sourceObj.IsPricelistFormViewDirty;
                _bIsPricelistNotesDirty = sourceObj.IsPricelistNotesDirty;

                _bIsRebateDirty = sourceObj.IsRebateDirty;

                _bIsSBAHeaderDirty = sourceObj.IsSBAHeaderDirty;
                _bIsSBAPlanDetailsDirty = sourceObj.IsSBAPlanDetailsDirty;
                _bIsContractCommentDirty = sourceObj.IsContractCommentDirty;

                _bIsCreateContractFormViewDirty = sourceObj.IsCreateContractFormViewDirty;
                _bIsVendorPOCFormViewDirty = sourceObj.IsVendorPOCFormViewDirty;
                _bIsVendorAddressFormViewDirty = sourceObj.IsVendorAddressFormViewDirty;
            }
        }

        private DocumentDirtyFlags _dirtyFlags = null;

        public DocumentDirtyFlags DirtyFlags
        {
            get { return _dirtyFlags; }
            set { _dirtyFlags = value; }
        }

        public void ClearDirtyFlags()
        {
            _dirtyFlags.IsContractGeneralContractAttributesDirty = false;
            _dirtyFlags.IsContractGeneralContractAssignmentDirty = false;
            _dirtyFlags.IsContractGeneralContractDatesDirty = false;
            _dirtyFlags.IsContractGeneralParentContractDirty = false;
            _dirtyFlags.IsContractHeaderInfoDirty = false;
            _dirtyFlags.IsPersonalizedNotificationPaneDirty = false;       

            _dirtyFlags.IsContractVendorSocioDirty = false;
            _dirtyFlags.IsContractVendorAttributesDirty = false;
            _dirtyFlags.IsContractVendorInsurancePolicyDirty = false;
            _dirtyFlags.IsWarrantyInformationDirty = false;
            _dirtyFlags.IsReturnedGoodsPolicyInformationDirty = false;
            _dirtyFlags.IsGeographicCoverageDirty = false;

            _dirtyFlags.IsContractDetailsAttributesDirty = false;
            _dirtyFlags.IsContractDetailsDiscountDirty = false;
            _dirtyFlags.IsContractDetailsDeliveryDirty = false;

            _dirtyFlags.IsVendorContractAdministratorDirty = false;
            _dirtyFlags.IsVendorAlternateContactDirty = false;
            _dirtyFlags.IsVendorTechnicalContactDirty = false;
            _dirtyFlags.IsVendorEmergencyContactDirty = false;
            _dirtyFlags.IsVendorOrderingContactDirty = false;
            _dirtyFlags.IsVendorSalesContactDirty = false;
            _dirtyFlags.IsVendorBusinessAddressDirty = false;

            _dirtyFlags.IsItemPriceCountsDirty = false;
            _dirtyFlags.IsPricelistNotesDirty = false;
            _dirtyFlags.IsPricelistFormViewDirty = false;
            _dirtyFlags.IsPricelistVerificationDirty = false;

            _dirtyFlags.IsRebateDirty = false;

            _dirtyFlags.IsSBAHeaderDirty = false;
            _dirtyFlags.IsSBAPlanDetailsDirty = false;
            _dirtyFlags.IsContractCommentDirty = false;

            _dirtyFlags.IsCreateContractFormViewDirty = false;
            _dirtyFlags.IsVendorPOCFormViewDirty = false;
            _dirtyFlags.IsVendorAddressFormViewDirty = false;
        }

        // summary of all flags
        public bool IsDocumentDirty()
        {
            return ( _dirtyFlags.IsContractGeneralContractAttributesDirty || _dirtyFlags.IsContractGeneralContractDatesDirty ||
                _dirtyFlags.IsContractGeneralContractAssignmentDirty || // _dirtyFlags.IsContractHeaderInfoDirty ||   _dirtyFlags.IsContractGeneralParentContractDirty ||
                _dirtyFlags.IsContractVendorSocioDirty ||
                _dirtyFlags.IsContractVendorAttributesDirty || _dirtyFlags.IsWarrantyInformationDirty ||
                _dirtyFlags.IsContractVendorInsurancePolicyDirty || 
                _dirtyFlags.IsReturnedGoodsPolicyInformationDirty || _dirtyFlags.IsGeographicCoverageDirty ||
                _dirtyFlags.IsContractDetailsAttributesDirty || _dirtyFlags.IsContractDetailsDiscountDirty ||
                _dirtyFlags.IsContractDetailsDeliveryDirty  || _dirtyFlags.IsVendorContractAdministratorDirty ||
                _dirtyFlags.IsVendorAlternateContactDirty || _dirtyFlags.IsVendorTechnicalContactDirty ||
                _dirtyFlags.IsVendorEmergencyContactDirty || _dirtyFlags.IsVendorOrderingContactDirty ||
                _dirtyFlags.IsVendorSalesContactDirty || _dirtyFlags.IsVendorBusinessAddressDirty ||  // _dirtyFlags.IsItemPriceCountsDirty || 
                _dirtyFlags.IsPricelistNotesDirty ||
                _dirtyFlags.IsPricelistVerificationDirty ||   // _dirtyFlags.IsPricelistFormViewDirty || 
                _dirtyFlags.IsRebateDirty || _dirtyFlags.IsSBAHeaderDirty || _dirtyFlags.IsSBAPlanDetailsDirty ||
                _dirtyFlags.IsContractCommentDirty || _dirtyFlags.IsCreateContractFormViewDirty || _dirtyFlags.IsVendorPOCFormViewDirty || 
                _dirtyFlags.IsVendorAddressFormViewDirty 
                );


        }

        public string ParentContractNumber
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.ContractNumber );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }       
        }

        public string ParentAwardDate
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.AwardDate.ToShortDateString() );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentEffectiveDate
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.EffectiveDate.ToShortDateString() );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentExpirationDate
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.ExpirationDate.ToShortDateString() );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentCompletionDate
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.CompletionDate.ToShortDateString() );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentScheduleName
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.ScheduleName );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public int ParentScheduleNumber
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.ScheduleNumber );
                        }
                        else
                        {
                            return ( -1 );
                        }
                    }
                    else
                    {
                        return ( -1 );
                    }
                }
                else
                {
                    return ( -1 );
                }
            }
        }

        public string ParentContractingOfficerFullName
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.OwnerName );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentVendorName
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.VendorName );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentVendorWebAddress
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.VendorWebAddress );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentVendorAddress1
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.VendorAddress1 );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentVendorAddress2
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.VendorAddress2 );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentVendorCity
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.VendorCity );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }
        public string ParentVendorState
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.VendorState );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }

        public string ParentVendorCountryName
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.VendorCountryName );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }
        public string ParentVendorZip
        {
            get
            {
                if( _currentDocument != null )
                {
                    if( _currentDocument.HasParent == true )
                    {
                        if( _currentDocument.ParentDocument != null )
                        {
                            return ( _currentDocument.ParentDocument.VendorZip );
                        }
                        else
                        {
                            return ( "" );
                        }
                    }
                    else
                    {
                        return ( "" );
                    }
                }
                else
                {
                    return ( "" );
                }
            }
        }
        // * = items originate from CurrentDocument
        private int    _contractId = -1;                   //[Contract_Record_ID]  int              NOT NULL IDENTITY (1, 1),
        private string _contractNumber = "";               // * [CntrctNum]           nvarchar(50)     NOT NULL,
        private int    _scheduleNumber = -1;               // * [Schedule_Number]     int              NOT NULL,
        private int    _COID = -1;                         // * [CO_ID]               int              NOT NULL,
        private string _vendorName = "";                   // * [Contractor_Name]     nvarchar(75)     NOT NULL,
        private string _SAMUEI = "";                       //[SAMUEI]              nvarchar(12)         NULL,
        private string _DUNS = "";                         //[DUNS]                nvarchar(9)          NULL,
        private string _TIN = "";                          //[TIN]                 nvarchar(9)          NULL,
        private bool   _bPrimeVendorParticipation;         //[PV_Participation]    bit                  NOT NULL DEFAULT ((0)),
        private string _solicitationNumber = "";           //[Solicitation_Number] nvarchar(40)         NULL,
        private string _vendorAddress1 = "";               //[Primary_Address_1]   nvarchar(100)        NULL,
        private string _vendorAddress2 = "";               //[Primary_Address_2]   nvarchar(100)        NULL,
        private string _vendorCity = "";                   //[Primary_City]        nvarchar(20)         NULL,
        private int _vendorCountryId = -1;                  //[Primary_Vendor_CountryId] int NULL,
        private string _vendorCountryName = "";             //CountryName nvarchar(100) NOT NULL 
        private string _vendorState = "";                  //[Primary_State]       nvarchar(2)          NULL,
        private string _vendorZip = "";                    //[Primary_Zip]         nvarchar(10)         NULL,
        private string _vendorWebAddress = "";             //[POC_VendorWeb]       nvarchar(50)         NULL,

        private string _vendorPrimaryContactName = "";          //[POC_Primary_Name]   	nvarchar(30)         NULL,
        private string _vendorPrimaryContactPhone = "";         //[POC_Primary_Phone]   nvarchar(15)         NULL,
        private string _vendorPrimaryContactExtension = "";     //[POC_Primary_Ext]    	nvarchar(5)          NULL,
        private string _vendorPrimaryContactFax = "";           //[POC_Primary_Fax]     nvarchar(15)         NULL,
        private string _vendorPrimaryContactEmail = "";         //[POC_Primary_Email]   nvarchar(50)         NULL,

        private string _generalContractNotes = "";              //[POC_Notes]           nvarchar(800)        NULL,

        private string _vendorAlternateContactName = "";           //[POC_Alternate_Name]  	nvarchar(30)         NULL,
        private string _vendorAlternateContactPhone = "";          //[POC_Alternate_Phone]  	nvarchar(15)         NULL,
        private string _vendorAlternateContactExtension = "";      //[POC_Alternate_Ext]     	nvarchar(5)          NULL,
        private string _vendorAlternateContactFax = "";            //[POC_Alternate_Fax]     	nvarchar(15)         NULL,
        private string _vendorAlternateContactEmail = "";          //[POC_Alternate_Email]   	nvarchar(50)         NULL,
        private string _vendorEmergencyContactName = "";           //[POC_Emergency_Name]   	nvarchar(30)         NULL,
        private string _vendorEmergencyContactPhone = "";          //[POC_Emergency_Phone]   	nvarchar(15)         NULL,
        private string _vendorEmergencyContactExtension = "";      //[POC_Emergency_Ext]    	nvarchar(5)          NULL,
        private string _vendorEmergencyContactFax = "";            //[POC_Emergency_Fax]    	nvarchar(15)         NULL,
        private string _vendorEmergencyContactEmail = "";          //[POC_Emergency_Email]  	nvarchar(50)         NULL,
        private string _vendorTechnicalContactName = "";           //[POC_Tech_Name]      	    nvarchar(30)         NULL,
        private string _vendorTechnicalContactPhone = "";          //[POC_Tech_Phone]     	    nvarchar(15)         NULL,
        private string _vendorTechnicalContactExtension = "";      //[POC_Tech_Ext]        	nvarchar(5)          NULL,
        private string _vendorTechnicalContactFax = "";            //[POC_Tech_Fax]        	nvarchar(15)         NULL,
        private string _vendorTechnicalContactEmail = "";          //[POC_Tech_Email]      	nvarchar(50)         NULL,


        private int _socioVetStatusId = -1;                 //[Socio_VetStatus_ID]   	int              NOT NULL DEFAULT ((0)),
        private int _socioBusinessSizeId = -1;              //[Socio_Business_Size_ID]  int              NOT NULL DEFAULT ((2)),
        private bool _bSocioSDB;                            //[Socio_SDB]        	    bit              NOT NULL DEFAULT ((0)),
        private bool _bSocio8a;                             //[Socio_8a]            	bit              NOT NULL DEFAULT ((0)),
        private bool _bSocioWomanOwned;                     //[Socio_Woman]         	bit              NOT NULL DEFAULT ((0)),
        private bool _bHubZone;                             //[Socio_HubZone]       	bit              NOT NULL DEFAULT ((0)),

        private string _basicDiscount = "";                 //[Discount_Basic]        	nvarchar(255)        NULL,
        private string _creditCardDiscount = "";            //[Discount_Credit_Card]    nvarchar(255)        NULL,
        private string _promptPayDiscount = "";             //[Discount_Prompt_Pay]   	nvarchar(255)        NULL,
        private string _quantityDiscount = "";              //[Discount_Quantity]      	nvarchar(255)        NULL,

        private int _geographicCoverageId = -1;             //[Geographic_Coverage_ID]  int                  NULL,
        private string _trackingCustomerName = "";          //[Tracking_Customer]      	nvarchar(255)        NULL,
        private string _minimumOrder = "";                  //[Mininum_Order]           nvarchar(255)        NULL,
        private string _deliveryTerms = "";                 //[Delivery_Terms]           	nvarchar(255)        NULL,
        private string _expeditedDeliveryTerms = "";        //[Expedited_Delivery_Terms]   	nvarchar(255)        NULL,
        private string _endOfYearDiscount ="";              //[Annual_Rebate]               nvarchar(255)        NULL,
        private string _FPRFreeFormatDateString = "";       //[BF_Offer]                    nvarchar(255)        NULL,
        private bool _bCreditCardAccepted;                  //[Credit_Card_Accepted]        bit              NOT NULL DEFAULT ((0)),
        private bool _bHazardousMaterial;                   //[Hazard]             	        bit              NOT NULL DEFAULT ((0)),
        private string _warrantyDuration = "";              //[Warranty_Duration]        	nvarchar(20)         NULL,
        private string _warrantyNotes = "";                 //[Warranty_Notes]          	nvarchar(1000)       NULL,
        private int _iffTypeId = -1;                        //[IFF_Type_ID]           	    int                  NULL,
        private string _ratio = "";                         //[Ratio]                    	nvarchar(255)        NULL,
        private int _returnedGoodsPolicyTypeId = 3;         //[Returned_Goods_Policy_Type]  int                  NULL,
        private string _returnedGoodsPolicyNotes = "";      //[Returned_Goods_Policy_Notes] nvarchar(1000)       NULL,
        private string _additionalDiscount = "";            //[Incentive_Description]     	nvarchar(255)        NULL,
        private int _vendorTypeId = -1;                     //[Dist_Manf_ID]         	    int                  NULL,

        private string _orderingAddress1 = "";              //[Ord_Address_1]        	    nvarchar(100)        NULL,
        private string _orderingAddress2 = "";              //[Ord_Address_2]      	        nvarchar(100)        NULL,
        private string _orderingCity = "";                  //[Ord_City]          	        nvarchar(20)         NULL,
        private int _orderingCountryId = 239;                //[Ord_CountryId]               int                 NULL, default to USA
        private string _orderingState = "";                 //[Ord_State]              	    nvarchar(2)          NULL,
        private string _orderingZip = "";                   //[Ord_Zip]            	        nvarchar(10)         NULL,
        private string _orderingTelephone = "";             //[Ord_Telephone]      	        nvarchar(15)         NULL,
        private string _orderingExtension = "";             //[Ord_Ext]             	    nvarchar(5)          NULL,
        private string _orderingFax = "";                   //[Ord_Fax]            	        nvarchar(15)         NULL,
        private string _orderingEmail = "";                 //[Ord_EMail]                 	nvarchar(50)         NULL,
                                    
        private decimal _estimatedContractValue = 0;        //[Estimated_Contract_Value]    money                NULL,
                                    
        private DateTime _contractAwardDate;                // * [Dates_CntrctAward]          	datetime         NOT NULL,
        private DateTime _contractEffectiveDate;            // * [Dates_Effective]         	datetime         NOT NULL,
        private DateTime _contractExpirationDate;           // * [Dates_CntrctExp]            	datetime         NOT NULL,
        private DateTime _contractCompletionDate;           // * [Dates_Completion]         	datetime             NULL,
        private int     _totalOptionYears = 0;             //[Dates_TotOptYrs]        	    int                  NULL,

        private bool _bPricelistVerified;                   //[Pricelist_Verified]      	bit              NOT NULL DEFAULT ((0)),
        private DateTime _pricelistVerificationDate;        //[Verification_Date]           datetime             NULL,
        private string _pricelistVerifiedBy = "";           //[Verified_By]                 nvarchar(25)         NULL,
        private string _currentModNumber = "";              //[Current_Mod_Number]      	nvarchar(20)         NULL,
        private string _pricelistNotes = "";                //[Pricelist_Notes]        	    nvarchar(255)        NULL,
          
        private int _SBAPlanId = -1;                        //[SBAPlanID]         	        int                  NULL,

        private bool _bVADOD;                               //[VA_DOD]                   	bit              NOT NULL DEFAULT ((0)),
        
        private bool _bTerminatedByConvenience;             //[Terminated_Convenience]    	bit              NOT NULL DEFAULT ((0)),
        private bool _bTerminatedByDefault;                 //[Terminated_Default]       	bit              NOT NULL DEFAULT ((0)),
        
        private string _contractDescription = "";           // * [Drug_Covered]          	    nvarchar(50)         NULL,
        private string _parentFSSContractNumber = "";       // * [BPA_FSS_Counterpart]       	nvarchar(20)         NULL,
        private int _parentContractId = -1;                 // * only from current document via lookup                                    
                                                            // unused [VA_IFF]              numeric(18,0)      NULL,
                                                            // unused [OGA_IFF]          	numeric(18,0)        NULL,
                                                            // unused [Cost_Avoidance]    	numeric(18,0)        NULL,
                                                            // unused [ICD_Exempt]     	    bit              NOT NULL DEFAULT ((0)),
                                                            // not on GUI [On_GSA_Advantage] bit              NOT NULL DEFAULT ((0)),
        private bool _bSBAPlanExempt;                       //[SBA_Plan_Exempt]                     bit              NOT NULL DEFAULT ((0)),
        private DateTime _insurancePolicyEffectiveDate;     //[Insurance_Policy_Effective_Date]     datetime             NULL,
        private DateTime _insurancePolicyExpirationDate;    //[Insurance_Policy_Expiration_Date]    datetime             NULL,
        private int _solicitationId = -1;                   //[Solicitation_ID]           	int                  NULL,
        private int _offerId = -1;                          //[Offer_ID]                  	int                  NULL,
        private int _65IBContractType = -1;                 //[65IB_Contract_Type]          int                  NULL DEFAULT ((1)),

        private string _vendorSalesContactName = "";        //[POC_Sales_Name]            	nvarchar(30)         NULL,
        private string _vendorSalesContactPhone = "";       //[POC_Sales_Phone]  	        nvarchar(15)         NULL,
        private string _vendorSalesContactExtension = "";   //[POC_Sales_Ext]      	        nvarchar(5)          NULL,
        private string _vendorSalesContactFax = "";         //[POC_Sales_Fax]    	        nvarchar(15)         NULL,
        private string _vendorSalesContactEmail = "";       //[POC_Sales_Email]          	nvarchar(50)         NULL,

        private string _tradeAgreementActCompliance = "";   //[TradeAgreementActCompliance]   	nchar(1)         NOT NULL DEFAULT ('C'),
                                             // not on GUI [VietnamVetOwned]     bit                  NULL DEFAULT ('0'),
        private int _stimulusActId = -1;                      //[StimulusAct]           	int     NOT NULL DEFAULT ((0)),
        private bool _bRebateRequired;                      //[RebateRequired]       	bit     NOT NULL DEFAULT ((0)),
        private bool _bStandardized;                        //[Standardized]          	bit     NOT NULL DEFAULT ((0)),

        private string _createdBy = "";                     //[CreatedBy]            	nvarchar(120)    NOT NULL DEFAULT (suser_sname()),
        private DateTime _creationDate;                     //[CreationDate]        	datetime         NOT NULL DEFAULT (getdate()),
        private string _lastModifiedBy = "";                //[LastModifiedBy]          nvarchar(120)    NOT NULL DEFAULT (suser_sname()),
        private DateTime _lastModificationDate;             //[LastModificationDate]   	datetime         NOT NULL DEFAULT (getdate()),

        // the below items are lookup values that are not updated
        private int _pharmaceuticalContractId = -1;  // from DI_Contracts populated on contract creation
        private string _contractingOfficerFullName = "";  // [FullName]
        private string _contractingOfficerPhone = ""; // [User_Phone]
        private string _scheduleName = "";                  // [Schedule_Name]
        private int _assistantDirectorCOID = -1;  // NC and National BPA's only tlkup_Sched/Cat [Asst_Director]
        private string _assistantDirectorName = "";  // NC and National BPA's only  [AssistantDirectorName]
        private int _seniorContractSpecialistCOID = -1; // NC and National BPA's only tlkup_Sched/Cat [Schedule_Manager]
        private string _seniorContractSpecialistName = "";  // NC and National BPA's only  [ScheduleManagerName]
        private string _rebateClause = "";
        private string _customRebateStartDate = "";

        private string[] _businessSizeDescription = new string[] { "", "Small", "Large" }; // 1,2 _socioBusinessSizeId
        private string[] _vendorType = new string[] { "", "Distributor", "Manufacturer", "Both", "", "Services" };  // 1,2,3,5 _vendorTypeId
        private string[] _geographicCoverageDescription = new string[] { "", "48 Cont States, DC", "50 States, DC", "50 States, DC, PR", "Specific States" };  // 1,2,3,4  _geographicCoverageId
        private string[] _iffTypeDescription = new string[] { "", "Embedded", "Absorbed" }; // 1,2 _iffTypeId
        private ArrayList _returnedGoodsPolicyTypes = null; // { new ReturnedGoodsPolicy( 0, "" ), new ReturnedGoodsPolicy( 1, "Government" ), new ReturnedGoodsPolicy( 2, "Commercial" ), new ReturnedGoodsPolicy( 3, "None Selected" ) }; // 1,2,3 _returnedGoodsPolicyTypeId
        private string[] _socioVetStatus = new string[] { "None", "Veteran", "Disabled Veteran" }; // 0,1,2 _socioVetStatusId

        [Serializable]
        public class ReturnedGoodsPolicy
        {

            private int _policyTypeId = 0;

            public int PolicyTypeId
            {
                get { return _policyTypeId; }
                set { _policyTypeId = value; }
            }
            private string _policyTypeDescription = "";

            public string PolicyTypeDescription
            {
                get { return _policyTypeDescription; }
                set { _policyTypeDescription = value; }
            }

            public ReturnedGoodsPolicy( int policyTypeId, string policyTypeDescription )
            {
                _policyTypeId = policyTypeId;
                _policyTypeDescription = policyTypeDescription;
            }
        }

        // the 7 types of contact information stored in tbl_Cntrcts used during update
        public enum ContactTypes
        {
            ADM, // administrator 
            ALT, // alternate 
            TECH,// technical 
            EMER, // emergency 
            ORD, // ordering
            SALE, // sales
            BUS // business address            
        }

#region StandardAccessors

        public int ContractId
        {
          get { return _contractId; }
          set { _contractId = value; }
        }                   
        
        public string ContractNumber
        {
          get { return _contractNumber; }
          set 
          {
              if( _contractNumber.CompareTo( value ) != 0 )
              {
                  // can only change during creation
                  if( _bContractCreationInProgress == true )
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
              }

              _contractNumber = value; 
          }
        }               
    
        public int ScheduleNumber
        {
          get { return _scheduleNumber; }
          set 
          {
              if( _scheduleNumber != value )
              {
                  // sched number can only change during contract creation
                  if( _bContractCreationInProgress == true )
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
              }

              _scheduleNumber = value; 
          }
        }            
      
        public int COID
        {
          get { return _COID; }
          set 
          {
              if( _COID != value )
              {
                  if( _bContractCreationInProgress == false )
                  {
                      _dirtyFlags.IsContractGeneralContractAssignmentDirty = true;
                      _dirtyFlags.IsContractHeaderInfoDirty = true;
                  }
                  else
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
              }

              _COID = value; 
          }
        }                      
       
        public string VendorName
        {
          get { return _vendorName; }
          set 
          {
              if( _vendorName.CompareTo( value ) != 0 )
              {
                  if( _bContractCreationInProgress == true )
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
                  else
                  {
                      _dirtyFlags.IsVendorBusinessAddressDirty = true;
                      _dirtyFlags.IsContractHeaderInfoDirty = true;
                  }
              }
              _vendorName = value; 
          }
        }

        public string SAMUEI
        {
            get
            {
                return _SAMUEI;
            }
            set
            {
                if( _SAMUEI.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractVendorAttributesDirty = true;
                _SAMUEI = value;
            }
        }

        public string DUNS
        {
            get { return _DUNS; }
            set 
            {
                if( _DUNS.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractVendorAttributesDirty = true;             
                _DUNS = value; 
            }
        }  

        public string TIN
        {
            get { return _TIN; }
            set
            {
                if( _TIN.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractVendorAttributesDirty = true;
                _TIN = value;
            }
        }                        
      
        public bool PrimeVendorParticipation
        {
          get { return _bPrimeVendorParticipation; }
          set 
          {
              if( _bPrimeVendorParticipation != value )
                _dirtyFlags.IsContractGeneralContractAttributesDirty = true;
              _bPrimeVendorParticipation = value;
          }
        }        
       
        public string SolicitationNumber
        {
            get { return _solicitationNumber; }
            set
            {
                // block the overwriting of inactive but historically valid solicitation numbers ( which do not appear in the drop down ) with the --select-- null value
             //   int testId = _solicitationId;
                string testValue = value;
                string oldValue = _solicitationNumber;

                // note NC will have an id = -1
                if( oldValue.Trim().Length == 0 && testValue.Trim().Length > 0 && testValue.CompareTo( "--select--" ) != 0 ) // new value
                {
                    _dirtyFlags.IsContractDetailsAttributesDirty = true;
                    _solicitationNumber = value;
                }
                else if( oldValue.Trim().Length > 0 && testValue.Trim().Length == 0 )  // intentional blank out
                {
                    _dirtyFlags.IsContractDetailsAttributesDirty = true;
                    _solicitationNumber = "";
                }
                else if( oldValue.Trim().Length > 0 && testValue.Trim().Length > 0 && testValue.CompareTo( "--select--" ) == 0 ) // intentional blank out
                {
                    _dirtyFlags.IsContractDetailsAttributesDirty = true;
                    _solicitationNumber = "";
                }
                else if( oldValue.Trim().Length > 0 && testValue.Trim().Length > 0 && testValue.CompareTo( "--select--" ) != 0 ) // potential update
                {                   
                    if( _solicitationNumber.CompareTo( value ) != 0 )
                        _dirtyFlags.IsContractDetailsAttributesDirty = true;
                    _solicitationNumber = value;
                }
                // else both blank, so no update
            }
        }         
        
        public string VendorAddress1
        {
          get { return _vendorAddress1; }
            set
            {
                if( _vendorAddress1.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorBusinessAddressDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // address formview on creation screen
                        _dirtyFlags.IsVendorAddressFormViewDirty = true;
                    }
                }   
                _vendorAddress1 = value;
            }
        }               
       
        public string VendorAddress2
        {
          get { return _vendorAddress2; }
            set
            {
                if( _vendorAddress2.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorBusinessAddressDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // address formview on creation screen
                        _dirtyFlags.IsVendorAddressFormViewDirty = true;
                    }
                }   
                _vendorAddress2 = value;
            }
        }               
        
        public string VendorCity
        {
          get { return _vendorCity; }
            set
            {
                if( _vendorCity.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorBusinessAddressDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // address formview on creation screen
                        _dirtyFlags.IsVendorAddressFormViewDirty = true;
                    }
                }   
                _vendorCity = value;
            }
        }

        public int VendorCountryId
        {
            get
            {
                return _vendorCountryId;
            }
            set
            {
                if( _vendorCountryId.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorBusinessAddressDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // address formview on creation screen
                        _dirtyFlags.IsVendorAddressFormViewDirty = true;
                    }
                }
                _vendorCountryId = value;
            }
        }

        // read-only for header
        public string VendorCountryName
        {
            get
            {
                return _vendorCountryName;
            }
            set
            {
                if( _vendorCountryName.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorBusinessAddressDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // address formview on creation screen
                        _dirtyFlags.IsVendorAddressFormViewDirty = true;
                    }
                }
                _vendorCountryName = value;
            }
        }

        public string VendorState
        {
          get { return _vendorState; }
            set
            {
                if( _vendorState.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorBusinessAddressDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // address formview on creation screen
                        _dirtyFlags.IsVendorAddressFormViewDirty = true;
                    }
                }   
                _vendorState = value;
            }
        }                   

        public string VendorZip
        {
          get { return _vendorZip; }
            set
            {
                if( _vendorZip.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorBusinessAddressDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // address formview on creation screen
                        _dirtyFlags.IsVendorAddressFormViewDirty = true;
                    }
                }   
                _vendorZip = value;
            }
        }                   
     
        public string VendorWebAddress
        {
          get { return _vendorWebAddress; }
            set
            {
                if( _vendorWebAddress.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorBusinessAddressDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // address formview on creation screen
                        _dirtyFlags.IsVendorAddressFormViewDirty = true;
                    }
                }   
                _vendorWebAddress = value;
            }
        }            

        public string VendorPrimaryContactName
        {
          get { return _vendorPrimaryContactName; }
          set
          {
              if( _vendorPrimaryContactName.CompareTo( value ) != 0 )
              {
                  if( _bContractCreationInProgress == false )
                  {
                      _dirtyFlags.IsVendorContractAdministratorDirty = true;
                      _dirtyFlags.IsContractHeaderInfoDirty = true;
                  }
                  else
                  {
                      // POC formview on creation screen
                      _dirtyFlags.IsVendorPOCFormViewDirty = true;
                  }
              }   

              _vendorPrimaryContactName = value; 
          }
        }    

        public string VendorPrimaryContactPhone
        {
          get { return _vendorPrimaryContactPhone; }
            set
            {
                if( _vendorPrimaryContactPhone.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorContractAdministratorDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // POC formview on creation screen
                        _dirtyFlags.IsVendorPOCFormViewDirty = true;
                    }
                }   

                _vendorPrimaryContactPhone = value;
            }
        }        
      
        public string VendorPrimaryContactExtension
        {
          get { return _vendorPrimaryContactExtension; }
            set
            {
                if( _vendorPrimaryContactExtension.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorContractAdministratorDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // POC formview on creation screen
                        _dirtyFlags.IsVendorPOCFormViewDirty = true;
                    }
                }   

                _vendorPrimaryContactExtension = value;
            }
        }         
      
        public string VendorPrimaryContactFax
        {
          get { return _vendorPrimaryContactFax; }
            set
            {
                if( _vendorPrimaryContactFax.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorContractAdministratorDirty = true;
                    }
                    else
                    {
                        // POC formview on creation screen
                        _dirtyFlags.IsVendorPOCFormViewDirty = true;
                    }
                }   

                _vendorPrimaryContactFax = value;
            }
        }       
       
        public string VendorPrimaryContactEmail
        {
          get { return _vendorPrimaryContactEmail; }
            set
            {
                if( _vendorPrimaryContactEmail.CompareTo( value ) != 0 )
                {
                    if( _bContractCreationInProgress == false )
                    {
                        _dirtyFlags.IsVendorContractAdministratorDirty = true;
                        _dirtyFlags.IsContractHeaderInfoDirty = true;
                    }
                    else
                    {
                        // POC formview on creation screen
                        _dirtyFlags.IsVendorPOCFormViewDirty = true;
                    }
                }   

                _vendorPrimaryContactEmail = value;
            }
        }       

        public string GeneralContractNotes
        {
          get { return _generalContractNotes; }
          set 
          {
              if( _generalContractNotes.CompareTo( value ) != 0 )
                  _dirtyFlags.IsContractCommentDirty = true;

              _generalContractNotes = value; 
          }
        }  

        public string VendorAlternateContactName
        {
          get { return _vendorAlternateContactName; }
            set
            {
                if( _vendorAlternateContactName.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorAlternateContactDirty = true;

                _vendorAlternateContactName = value;
            }
        }  
     
        public string VendorAlternateContactPhone
        {
          get { return _vendorAlternateContactPhone; }
            set
            {
                if( _vendorAlternateContactPhone.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorAlternateContactDirty = true;

                _vendorAlternateContactPhone = value;
            }
        }   
      
        public string VendorAlternateContactExtension
        {
          get { return _vendorAlternateContactExtension; }
            set
            {
                if( _vendorAlternateContactExtension.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorAlternateContactDirty = true;

                _vendorAlternateContactExtension = value;
            }
        }   
     
        public string VendorAlternateContactFax
        {
          get { return _vendorAlternateContactFax; }
            set
            {
                if( _vendorAlternateContactFax.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorAlternateContactDirty = true;

                _vendorAlternateContactFax = value;
            }
        }      
       
        public string VendorAlternateContactEmail
        {
          get { return _vendorAlternateContactEmail; }
            set
            {
                if( _vendorAlternateContactEmail.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorAlternateContactDirty = true;

                _vendorAlternateContactEmail = value;
            }
        }      
        
        public string VendorEmergencyContactName
        {
          get { return _vendorEmergencyContactName; }
            set
            {
                if( _vendorEmergencyContactName.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorEmergencyContactDirty = true;

                _vendorEmergencyContactName = value;
            }
        }    
      
        public string VendorEmergencyContactPhone
        {
          get { return _vendorEmergencyContactPhone; }
            set
            {
                if( _vendorEmergencyContactPhone.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorEmergencyContactDirty = true;

                _vendorEmergencyContactPhone = value;
            }
        }      
       
        public string VendorEmergencyContactExtension
        {
          get { return _vendorEmergencyContactExtension; }
            set
            {
                if( _vendorEmergencyContactExtension.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorEmergencyContactDirty = true;

                _vendorEmergencyContactExtension = value;
            }
        } 
       
        public string VendorEmergencyContactFax
        {
          get { return _vendorEmergencyContactFax; }
            set
            {
                if( _vendorEmergencyContactFax.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorEmergencyContactDirty = true;

                _vendorEmergencyContactFax = value;
            }
        }     
       
        public string VendorEmergencyContactEmail
        {
          get { return _vendorEmergencyContactEmail; }
            set
            {
                if( _vendorEmergencyContactEmail.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorEmergencyContactDirty = true;

                _vendorEmergencyContactEmail = value;
            }
        }    
       
        public string VendorTechnicalContactName
        {
          get { return _vendorTechnicalContactName; }
            set
            {
                if( _vendorTechnicalContactName.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorTechnicalContactDirty = true;

                _vendorTechnicalContactName = value;
            }
        }     
     
        public string VendorTechnicalContactPhone
        {
          get { return _vendorTechnicalContactPhone; }
            set
            {
                if( _vendorTechnicalContactPhone.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorTechnicalContactDirty = true;

                _vendorTechnicalContactPhone = value;
            }
        }     
       
        public string VendorTechnicalContactExtension
        {
          get { return _vendorTechnicalContactExtension; }
            set
            {
                if( _vendorTechnicalContactExtension.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorTechnicalContactDirty = true;

                _vendorTechnicalContactExtension = value;
            }
        }  
       
        public string VendorTechnicalContactFax
        {
          get { return _vendorTechnicalContactFax; }
            set
            {
                if( _vendorTechnicalContactFax.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorTechnicalContactDirty = true;

                _vendorTechnicalContactFax = value;
            }
        }      
      
        public string VendorTechnicalContactEmail
        {
          get { return _vendorTechnicalContactEmail; }
            set
            {
                if( _vendorTechnicalContactEmail.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorTechnicalContactDirty = true;

                _vendorTechnicalContactEmail = value;
            }
        }     

        public int SocioVetStatusId
        {
          get { return _socioVetStatusId; }
          set 
          {
              if( _socioVetStatusId != value )
                  _dirtyFlags.IsContractVendorSocioDirty = true;
              _socioVetStatusId = value;     
          }
        }     
       
        public int SocioBusinessSizeId
        {
          get { return _socioBusinessSizeId; }
          set 
          {
              if( _socioBusinessSizeId != value )
              {
                  if( _bContractCreationInProgress == false )
                  {
                      _dirtyFlags.IsContractVendorSocioDirty = true;
                  }
                  else
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
                  _socioBusinessSizeId = value;
              }
          }
        }    
       
        public bool SocioSDB
        {
          get { return _bSocioSDB; }
          set
          {
              if( _bSocioSDB != value )
                  _dirtyFlags.IsContractVendorSocioDirty = true;
              _bSocioSDB = value;     
          }
        }               
       
        public bool Socio8a
        {
            get { return _bSocio8a; }
            set
            {
                if( _bSocio8a != value )
                    _dirtyFlags.IsContractVendorSocioDirty = true;
                _bSocio8a = value;
            }
        }            
        
        public bool SocioWomanOwned
        {
            get { return _bSocioWomanOwned; }
            set
            {
                if( _bSocioWomanOwned != value )
                    _dirtyFlags.IsContractVendorSocioDirty = true;
                _bSocioWomanOwned = value;
            }
        }      
       
        public bool HubZone
        {
            get { return _bHubZone; }
            set
            {
                if( _bHubZone != value )
                    _dirtyFlags.IsContractVendorSocioDirty = true;
                _bHubZone = value;
            }
        }                 

        public string BasicDiscount
        {
          get { return _basicDiscount; }
            set
            {
                if( _basicDiscount.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsDiscountDirty = true;
                _basicDiscount = value;
            }
        }        
        
        public string CreditCardDiscount
        {
          get { return _creditCardDiscount; }
            set
            {
                if( _creditCardDiscount.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsDiscountDirty = true;
                _creditCardDiscount = value;
            }
        }   
       
        public string PromptPayDiscount
        {
          get { return _promptPayDiscount; }
            set
            {
                if( _promptPayDiscount.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsDiscountDirty = true;
                _promptPayDiscount = value;
            }
        }      
       
        public string QuantityDiscount
        {
          get { return _quantityDiscount; }
            set
            {
                if( _quantityDiscount.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsDiscountDirty = true;
                _quantityDiscount = value;
            }
        }      
     
        // this is being retired in release 2
        public int GeographicCoverageId
        {
          get { return _geographicCoverageId; }
          set { _geographicCoverageId = value; }
        }        
    
        public string TrackingCustomerName
        {
          get { return _trackingCustomerName; }
            set
            {
                if( _trackingCustomerName.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsAttributesDirty = true;
                _trackingCustomerName = value;
            }
        }     
       
        public string MinimumOrder
        {
          get { return _minimumOrder; }
            set
            {
                if( _minimumOrder.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsAttributesDirty = true;
                _minimumOrder = value;
            }
        }              
      
        public string DeliveryTerms
        {
          get { return _deliveryTerms; }
            set
            {
                if( _deliveryTerms.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsDeliveryDirty = true;
                _deliveryTerms = value;
            }
        }            
     
        public string ExpeditedDeliveryTerms
        {
          get { return _expeditedDeliveryTerms; }
            set
            {
                if( _expeditedDeliveryTerms.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsDeliveryDirty = true;
                _expeditedDeliveryTerms = value;
            }
        }      
   
        public string EndOfYearDiscount
        {
          get { return _endOfYearDiscount; }
            set
            {
                if( _endOfYearDiscount.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsDiscountDirty = true;
                _endOfYearDiscount = value;
            }
        }        
      
        public string FPRFreeFormatDateString
        {
          get { return _FPRFreeFormatDateString; }
            set
            {
                if( _FPRFreeFormatDateString.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsAttributesDirty = true;
                _FPRFreeFormatDateString = value;
            }
        }    
       
        public bool CreditCardAccepted
        {
            get { return _bCreditCardAccepted; }
            set
            {
                if( _bCreditCardAccepted != value )
                    _dirtyFlags.IsContractVendorAttributesDirty = true;
                _bCreditCardAccepted = value;
            }
        }               
       
        public bool HazardousMaterial
        {
            get { return _bHazardousMaterial; }
            set
            {
                if( _bHazardousMaterial != value )
                    _dirtyFlags.IsContractVendorAttributesDirty = true;
                _bHazardousMaterial = value;
            }
        }             
       
        public string WarrantyDuration
        {
          get { return _warrantyDuration; }
          set 
          {
              if( _warrantyDuration != value )
                  _dirtyFlags.IsWarrantyInformationDirty = true;
              _warrantyDuration = value; 
          }
        }        
      
        public string WarrantyNotes
        {
          get { return _warrantyNotes; }
          set 
          {
              if( _warrantyNotes != value )
                  _dirtyFlags.IsWarrantyInformationDirty = true;                   
              _warrantyNotes = value; 
          }
        }

        public bool IffAbsorbed
        {
            get
            {
                return( ( _iffTypeId == 2 ) ? true : false );
            }
            // as of R2 can no longer be edited, only displayed for historical contracts
            set
            {
                //if( ( _iffTypeId == 2 && value == false ) || ( _iffTypeId == 1 && value == true ) )
                //    _dirtyFlags.IsContractDetailsAttributesDirty = true;

                if( value == true )
                    _iffTypeId = 2;
                else
                    _iffTypeId = 1;
            }
        }

        public bool IffEmbedded
        {
            get
            {
                return( ( _iffTypeId == 1 ) ? true : false );
            }
            // as of R2 can no longer be edited, only displayed for historical contracts
            set
            {
                //if( ( _iffTypeId == 1 && value == false ) || ( _iffTypeId == 2 && value == true ) )
                //    _dirtyFlags.IsContractDetailsAttributesDirty = true;

                if( value == true )
                    _iffTypeId = 1;
                else
                    _iffTypeId = 2;
            }
        }

        public int IffTypeId
        {
          get { return _iffTypeId; }
          set { _iffTypeId = value; }
        }            
       
        public string Ratio
        {
          get { return _ratio; }
            set
            {
                if( _ratio.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsAttributesDirty = true;
                _ratio = value;
            }
        }     

        public string ReturnedGoodsPolicyType
        {
            get 
            {
                return ( ( _returnedGoodsPolicyTypeId == 1 ) ? "Government" : ( _returnedGoodsPolicyTypeId == 2 ) ? "Commercial" : ( _returnedGoodsPolicyTypeId == 3 ) ? "None Selected" : "None Selected" ); 
            }
            set 
            {
                int oldValue = _returnedGoodsPolicyTypeId;
                if( "Government".CompareTo( value ) == 0 )
                    _returnedGoodsPolicyTypeId = 1;
                else if( "Commercial".CompareTo( value ) == 0 )
                    _returnedGoodsPolicyTypeId = 2;
                else if( "None Selected".CompareTo( value ) == 0 )
                    _returnedGoodsPolicyTypeId = 3;
                else if( ( ( string )value ).Length == 0 )
                    _returnedGoodsPolicyTypeId = 3;
                if( oldValue != _returnedGoodsPolicyTypeId )
                {
                    _dirtyFlags.IsReturnedGoodsPolicyInformationDirty = true;
                }
            }
        }      
        
        public int ReturnedGoodsPolicyTypeId
        {
            get { return _returnedGoodsPolicyTypeId; }
            set { _returnedGoodsPolicyTypeId = value; }
        }   

        public string ReturnedGoodsPolicyNotes
        {
          get { return _returnedGoodsPolicyNotes; }
          set 
          {
              if( _returnedGoodsPolicyNotes.CompareTo( value ) != 0 )
                  _dirtyFlags.IsReturnedGoodsPolicyInformationDirty = true;
              _returnedGoodsPolicyNotes = value; 
          }
        }     
      
        public string AdditionalDiscount
        {
          get { return _additionalDiscount; }
            set
            {
                if( _additionalDiscount.CompareTo( value ) != 0 )
                    _dirtyFlags.IsContractDetailsDiscountDirty = true;
                _additionalDiscount = value;
            }
        }    
      
        public int VendorTypeId
        {
             get { return _vendorTypeId; }
            set
            {
                if( _vendorTypeId != value )
                    _dirtyFlags.IsContractVendorAttributesDirty = true;
                _vendorTypeId = value;
            }
        }              

        public string OrderingAddress1
        {
          get { return _orderingAddress1; }
            set
            {
                if( _orderingAddress1.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsVendorOrderingContactDirty = true;
                }
                _orderingAddress1 = value;
            }
        }      
       
        public string OrderingAddress2
        {
          get { return _orderingAddress2; }
            set
            {
                if( _orderingAddress2.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsVendorOrderingContactDirty = true;
                }

                _orderingAddress2 = value;
            }
        }         
      
        public string OrderingCity
        {
          get { return _orderingCity; }
            set
            {
                if( _orderingCity.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsVendorOrderingContactDirty = true;
                }

                _orderingCity = value;
            }
        }

        public int OrderingCountryId
        {
            get
            {
                return _orderingCountryId;
            }
            set
            {
                if( _orderingCountryId.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsVendorOrderingContactDirty = true;
                }

                _orderingCountryId = value;
            }
        }

        public string OrderingState
        {
          get { return _orderingState; }
            set
            {
                if( _orderingState.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsVendorOrderingContactDirty = true;
                }

                _orderingState = value;
            }
        }         
      
        public string OrderingZip
        {
          get { return _orderingZip; }
            set
            {
                if( _orderingZip.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsVendorOrderingContactDirty = true;
                }

                _orderingZip = value;
            }
        }           
     
        public string OrderingTelephone
        {
          get { return _orderingTelephone; }
            set
            {
                if( _orderingTelephone.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorOrderingContactDirty = true;

                _orderingTelephone = value;
            }
        }     
     
        public string OrderingExtension
        {
          get { return _orderingExtension; }
            set
            {
                if( _orderingExtension.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorOrderingContactDirty = true;

                _orderingExtension = value;
            }
        }       
     
        public string OrderingFax
        {
          get { return _orderingFax; }
            set
            {
                if( _orderingFax.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorOrderingContactDirty = true;

                _orderingFax = value;
            }
        }                

        public string OrderingEmail
        {
          get { return _orderingEmail; }
            set
            {
                if( _orderingEmail.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorOrderingContactDirty = true;

                _orderingEmail = value;
            }
        }          
                                      
        public decimal EstimatedContractValue
        {
          get { return _estimatedContractValue; }
          set 
          { 
              if( _estimatedContractValue.CompareTo( value ) != 0 )
                _dirtyFlags.IsContractDetailsAttributesDirty = true;
              _estimatedContractValue = value;
          }

        }   
                                    
        public DateTime ContractAwardDate
        {
          get { return _contractAwardDate; }
          set 
          {
              if( _contractAwardDate.CompareTo( value ) != 0 )
              {
                  if( _bContractCreationInProgress == false )
                  {
                      _dirtyFlags.IsContractGeneralContractDatesDirty = true;
                      if( _currentDocument.IsCurrentUserTheOwner == true )
                          _dirtyFlags.IsPersonalizedNotificationPaneDirty = true;
                  }
                  else
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
              }
              _contractAwardDate = value;
          }
        }             
    
        public DateTime ContractEffectiveDate
        {
          get { return _contractEffectiveDate; }
          set 
          { 
              if( _contractEffectiveDate.CompareTo( value ) != 0 )
              {
                  if( _bContractCreationInProgress == false )
                  {
                      _dirtyFlags.IsContractGeneralContractDatesDirty = true;
                  }
                  else
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
              }
  
              _contractEffectiveDate = value;
          }
        }      
       
        public DateTime ContractExpirationDate
        {
          get { return _contractExpirationDate; }
          set 
          { 
              if( _contractExpirationDate.CompareTo( value ) != 0 )
              {
                  if( _bContractCreationInProgress == false )
                  {
                      _dirtyFlags.IsContractGeneralContractDatesDirty = true;
                      _dirtyFlags.IsContractHeaderInfoDirty = true;
                      if( _currentDocument.IsCurrentUserTheOwner == true )
                          _dirtyFlags.IsPersonalizedNotificationPaneDirty = true;
                  }
                  else
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
              }

              _contractExpirationDate = value;
          }
        }      
      
        public DateTime ContractCompletionDate
        {
          get { return _contractCompletionDate; }
          set 
          { 
              if( _contractCompletionDate.CompareTo( value ) != 0 )
              {
                  if( _bContractCreationInProgress == false )
                  {
                      _dirtyFlags.IsContractGeneralContractDatesDirty = true;
                      _dirtyFlags.IsContractHeaderInfoDirty = true;
                  }
                  else
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
              }

              _contractCompletionDate = value;
          }
        }    
     
        public int TotalOptionYears
        {
          get { return _totalOptionYears; }
          set 
          { 
              if( _totalOptionYears != value )
                _dirtyFlags.IsContractGeneralContractDatesDirty = true;
              _totalOptionYears = value;
          }
        }         

        public bool PricelistVerified
        {
          get { return _bPricelistVerified; }
            set
            {
                if( _bPricelistVerified != value )
                    _dirtyFlags.IsPricelistVerificationDirty = true;
                _bPricelistVerified = value;
            }
        }                
      
        public DateTime PricelistVerificationDate
        {
          get { return _pricelistVerificationDate; }
            set
            {
                if( _pricelistVerificationDate != value )
                    _dirtyFlags.IsPricelistVerificationDirty = true;
                _pricelistVerificationDate = value;
            }
        }    
       
        public string PricelistVerifiedBy
        {
          get { return _pricelistVerifiedBy; }
            set
            {
                if( _pricelistVerifiedBy != value )
                    _dirtyFlags.IsPricelistVerificationDirty = true;
                _pricelistVerifiedBy = value;
            }
        }     
     
        public string CurrentModNumber
        {
          get { return _currentModNumber; }
            set
            {
                if( _currentModNumber != value )
                    _dirtyFlags.IsPricelistVerificationDirty = true;
                _currentModNumber = value;
            }
        }       
       
        public string PricelistNotes
        {
          get { return _pricelistNotes; }
            set
            {
                if( _pricelistNotes != value )
                    _dirtyFlags.IsPricelistNotesDirty = true;
                _pricelistNotes = value;
            }
        }             
          
        public int SBAPlanId
        {
          get { return _SBAPlanId; }
          set 
          {
              if( _SBAPlanId != value )
                  _dirtyFlags.IsSBAHeaderDirty = true;
              _SBAPlanId = value; 
          }
        }        

        public bool VADOD
        {
          get { return _bVADOD; }
          set 
          { 
              if( _bVADOD != value )
                _dirtyFlags.IsContractGeneralContractAttributesDirty = true;
              _bVADOD = value;
          }
        }           
             
        public bool TerminatedByConvenience
        {
          get { return _bTerminatedByConvenience; }
          set 
          { 
              if( _bTerminatedByConvenience != value )
                _dirtyFlags.IsContractGeneralContractDatesDirty = true;
              _bTerminatedByConvenience = value;
          }
        }           
      
        public bool TerminatedByDefault
        {
          get { return _bTerminatedByDefault; }
          set 
          { 
              if( _bTerminatedByDefault != value )
                _dirtyFlags.IsContractGeneralContractDatesDirty = true;
              _bTerminatedByDefault = value;
          }
        }        
        
        public string ContractDescription
        {
          get { return _contractDescription; }
          set 
          { 
              if( _contractDescription.CompareTo( value ) != 0 )
                _dirtyFlags.IsContractGeneralContractAttributesDirty = true;
              _contractDescription = value;
          }
        }         
       
        // currently cannot be edited
        public string ParentFSSContractNumber
        {
          get { return _parentFSSContractNumber; }
          set { _parentFSSContractNumber = value; }
        }

        public int ParentContractId
        {
            get { return _parentContractId; }
            set { _parentContractId = value; }
        }
                                                                                         
        public bool SBAPlanExempt
        {
          get { return _bSBAPlanExempt; }
          set
          {
              if( _bSBAPlanExempt != value )
                  _dirtyFlags.IsSBAHeaderDirty = true;
              _bSBAPlanExempt = value; 
          }
        }                      
       
        public DateTime InsurancePolicyEffectiveDate
        {
          get { return _insurancePolicyEffectiveDate; }
          set 
          {
              if( _insurancePolicyEffectiveDate != value )
                  _dirtyFlags.IsContractVendorInsurancePolicyDirty = true;
              _insurancePolicyEffectiveDate = value; 
          }
        }     
        
        public DateTime InsurancePolicyExpirationDate
        {
          get { return _insurancePolicyExpirationDate; }
          set 
          {
              if( _insurancePolicyExpirationDate != value )
                  _dirtyFlags.IsContractVendorInsurancePolicyDirty = true;
              _insurancePolicyExpirationDate = value; 
          }
        }   
      
        // not directly editable
        public int SolicitationId
        {
          get { return _solicitationId; }
          set { _solicitationId = value; }
        }                  
       
        // not directly editable
        public int OfferId
        {
          get { return _offerId; }
          set { _offerId = value; }
        }                         
       
        // not used
        public int ServiceContractType
        {
          get { return _65IBContractType; }
          set { _65IBContractType = value; }
        }                

        public string VendorSalesContactName
        {
          get { return _vendorSalesContactName; }
            set
            {
                if( _vendorSalesContactName.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorSalesContactDirty = true;

                _vendorSalesContactName = value;
            }
        }       
       
        public string VendorSalesContactPhone
        {
          get { return _vendorSalesContactPhone; }
            set
            {
                if( _vendorSalesContactPhone.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorSalesContactDirty = true;

                _vendorSalesContactPhone = value;
            }
        }      
       
        public string VendorSalesContactExtension
        {
          get { return _vendorSalesContactExtension; }
            set
            {
                if( _vendorSalesContactExtension.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorSalesContactDirty = true;

                _vendorSalesContactExtension = value;
            }
        }  
       
        public string VendorSalesContactFax
        {
          get { return _vendorSalesContactFax; }
            set
            {
                if( _vendorSalesContactFax.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorSalesContactDirty = true;

                _vendorSalesContactFax = value;
            }
        }        
       
        public string VendorSalesContactEmail
        {
          get { return _vendorSalesContactEmail; }
            set
            {
                if( _vendorSalesContactEmail.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorSalesContactDirty = true;

                _vendorSalesContactEmail = value;
            }
        }

        public string TradeAgreementActCompliance
        {
            get { return _tradeAgreementActCompliance; }
            set 
            { 
                if( _tradeAgreementActCompliance != value )
                    _dirtyFlags.IsContractGeneralContractAttributesDirty = true;
                _tradeAgreementActCompliance = value;
            }
        }
                                    
        public bool StimulusAct
        {
            get { return ( ( _stimulusActId == 0 ) ? false : true ); }
            set 
            { 
                if( _stimulusActId != ( int )(( value == true ) ? 1 : 0 ))
                    _dirtyFlags.IsContractGeneralContractAttributesDirty = true;
                _stimulusActId = ( value == true ) ? 1 : 0;
            }
        }                     
       
        public bool RebateRequired
        {
          get { return _bRebateRequired; }
          set 
          {
              if( _bRebateRequired != value )
              {
                  if( _bContractCreationInProgress == false )
                  {
                      _dirtyFlags.IsRebateDirty = true;
                  }
                  else
                  {
                      _dirtyFlags.IsCreateContractFormViewDirty = true;
                  }
              }

              _bRebateRequired = value; 
          }
        }                    
       
        public bool Standardized
        {
          get { return _bStandardized; }
          set 
          { 
              if( _bStandardized != value )
                _dirtyFlags.IsContractGeneralContractAttributesDirty = true;
              _bStandardized = value;
          }
        }                      

        public string CreatedBy
        {
          get { return _createdBy; }
          set { _createdBy = value; }
        }                   
    
        public DateTime CreationDate
        {
          get { return _creationDate; }
          set { _creationDate = value; }
        }                    
       
        public string LastModifiedBy
        {
          get { return _lastModifiedBy; }
          set { _lastModifiedBy = value; }
        }               
      
        public DateTime LastModificationDate
        {
          get { return _lastModificationDate; }
          set { _lastModificationDate = value; }
        }

#endregion StandardAccessors
        // the below item is unique to the contract creation process
        private int _divisionId = -1;

        public int DivisionId
        {
            get { return _divisionId; }
            set 
            {
                if( _divisionId != value )
                {
                    // can only be changed during contract creation
                    if( _bContractCreationInProgress == true )
                    {
                        _dirtyFlags.IsCreateContractFormViewDirty = true;
                    }
                }

                _divisionId = value; 
            }
        }

        // the below items are lookup values that are not updated
        public int PharmaceuticalContractId
        {
            get { return _pharmaceuticalContractId; }
            set { _pharmaceuticalContractId = value; }
        }

        public string ContractingOfficerFullName
        {
            get { return _contractingOfficerFullName; }
            set { _contractingOfficerFullName = value; }
        }

        public string ContractingOfficerPhone
        {
            get { return _contractingOfficerPhone; }
            set { _contractingOfficerPhone = value; }
        }

        public string ScheduleName
        {
            get { return _scheduleName; }           
        }

        public int AssistantDirectorCOID
        {
            get { return _assistantDirectorCOID; }
            set { _assistantDirectorCOID = value; }
        }

        public string AssistantDirectorName
        {
            get { return _assistantDirectorName; }
            set { _assistantDirectorName = value; }
        }

        public int SeniorContractSpecialistCOID
        {
            get { return _seniorContractSpecialistCOID; }
            set { _seniorContractSpecialistCOID = value; }
        }

        public string SeniorContractSpecialistName
        {
            get { return _seniorContractSpecialistName; }
            set { _seniorContractSpecialistName = value; }
        }

        public string RebateClause
        {
            get { return _rebateClause; }
            set { _rebateClause = value; }
        }

        public string CustomRebateStartDate
        {
            get { return _customRebateStartDate; }
            set { _customRebateStartDate = value; }
        }

        public string[] BusinessSizeDescription
        {
            get { return _businessSizeDescription; }
        }

        public string[] VendorType
        {
            get { return _vendorType; }
        }

        public string[] GeographicCoverageDescription
        {
            get { return _geographicCoverageDescription; }
        }

        public string[] IffTypeDescription
        {
            get { return _iffTypeDescription; }
        }
 
        public ArrayList ReturnedGoodsPolicyTypes
        {
            get { return _returnedGoodsPolicyTypes; }
        }

        public string[] SocioVetStatus
        {
            get { return _socioVetStatus; }
        }

        private GeographicCoverage _geographicCoverage = null;

        public GeographicCoverage GeographicCoverage
        {
            get { return _geographicCoverage; }
            set { _geographicCoverage = value; }
        }
    }
}
