using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using VA.NAC.NACCMBrowser.DBInterface;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class EditedDocumentManagerBack
    {
        CurrentDocument _currentDocument = null;

        private DrugItemDB _drugItemDB = null;
        private ContractDB _contractDB = null;
        private OfferDB _offerDB = null;

        public ContractDB ContractDatabase
        {
            get { return _contractDB; }
            set { _contractDB = value; }
        }

        public static string ErrorIndicator = "HasError";

        public EditedDocumentManagerBack()
        {
            _currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];
            _contractDB = ( ContractDB )HttpContext.Current.Session[ "ContractDB" ];
            _drugItemDB = ( DrugItemDB )HttpContext.Current.Session[ "DrugItemDB" ];
            _offerDB = ( OfferDB )HttpContext.Current.Session[ "OfferDB" ];
        }

        // select function
        [DataObjectMethod( DataObjectMethodType.Select, true )]
        public EditedDocumentContent GetEditedDocumentContent()
        {
            EditedDocumentContent editedDocumentContentBack = null;

            if( HttpContext.Current.Session[ "EditedDocumentContentBack" ] != null )
            {
                editedDocumentContentBack = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentBack" ];

                editedDocumentContentBack = GetDocumentContent( editedDocumentContentBack );               
            }
            else  // on initial load
            {
                editedDocumentContentBack = GetDocumentContent( new EditedDocumentContent() );

                HttpContext.Current.Session[ "EditedDocumentContentBack" ] = editedDocumentContentBack;
            }

            return ( editedDocumentContentBack );
        }

        private EditedDocumentContent GetDocumentContent( EditedDocumentContent editedDocumentContentBack )
        {

            bool bSuccess = false;

            DataSet dsContractDetails = null;

            _contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _contractDB.MakeConnectionString();

            bSuccess = _contractDB.GetContractInfoDetails( _currentDocument.ContractNumber, ref dsContractDetails );

            if( bSuccess == true )
            {
                bSuccess = editedDocumentContentBack.CompleteContractDetails( dsContractDetails ); // note that all dirty flags are cleared on success
                if( bSuccess == false )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to parse the contract details for contract {0} : {1}", editedDocumentContentBack.ContractNumber, editedDocumentContentBack.ErrorMessage );
                }
            }
            else
            {
                HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to retrieve the contract details for contract {0} : {1}", editedDocumentContentBack.ContractNumber, _contractDB.ErrorMessage );
            }

            // get geographic coverage row
            if( bSuccess == true )
            {
                DataSet dsGeographicCoverage = null;

                bSuccess = _contractDB.GetGeographicCoverage( _currentDocument.ContractNumber, ref dsGeographicCoverage );

                if( bSuccess == true )
                {
                    GeographicCoverage geographicCoverage = editedDocumentContentBack.GeographicCoverage;

                    bSuccess = geographicCoverage.CompleteGeographicCoverage( dsGeographicCoverage );
                    if( bSuccess == false )
                    {
                        HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                        HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to parse the geographic coverage for contract {0} : {1}", editedDocumentContentBack.ContractNumber, geographicCoverage.ErrorMessage );
                    }

                }
                else
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to retrieve the geographic coverage for contract {0} : {1}", editedDocumentContentBack.ContractNumber, _contractDB.ErrorMessage );
                }

            }

            return ( editedDocumentContentBack );
        }

        // update function
        [DataObjectMethod( DataObjectMethodType.Update, true )]
        public static int SaveChanges( EditedDocumentContent editedDocumentContentBack )
        {
            bool bSuccess = true;

            // save the values to the session
            HttpContext.Current.Session[ "EditedDocumentContentBack" ] = editedDocumentContentBack;

            ContractDB contractDB = ( ContractDB )HttpContext.Current.Session[ "ContractDB" ];

            contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            contractDB.MakeConnectionString();

            // Contract General

            if( editedDocumentContentBack.DirtyFlags.IsContractGeneralContractDatesDirty == true )
            {
                bSuccess = contractDB.UpdateContractGeneralContractDates( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber, editedDocumentContentBack.ContractAwardDate, editedDocumentContentBack.ContractEffectiveDate, editedDocumentContentBack.ContractExpirationDate, editedDocumentContentBack.ContractCompletionDate, editedDocumentContentBack.TerminatedByConvenience, editedDocumentContentBack.TerminatedByDefault, editedDocumentContentBack.TotalOptionYears );
                if( bSuccess == true )
                {
                    editedDocumentContentBack.RefreshCurrentDocumentDatesFromEditedDocument();
                }
                else
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract dates for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsContractGeneralContractAttributesDirty == true )
            {
                bSuccess = contractDB.UpdateContractGeneralContractAttributes( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber, editedDocumentContentBack.ContractDescription, editedDocumentContentBack.VADOD, editedDocumentContentBack.PrimeVendorParticipation, editedDocumentContentBack.TradeAgreementActCompliance, editedDocumentContentBack.StimulusAct, editedDocumentContentBack.Standardized );
                if( bSuccess == true )
                {
                    editedDocumentContentBack.RefreshCurrentDocumentAttributesFromEditedDocument();
                }
                else
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract attributes for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }


            if( editedDocumentContentBack.DirtyFlags.IsContractGeneralContractAssignmentDirty == true )
            {
                bSuccess = contractDB.UpdateContractGeneralContractAssignment( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber, editedDocumentContentBack.COID );
                if( bSuccess == true )
                {
                    // backfill CO assignment related data
                    int COID = -1;
                    string contractingOfficerFullName = "";
                    string contractingOfficerPhone = "";
                    Guid contractingOfficerUserId = System.Guid.Empty;   // not used
                    int seniorContractSpecialistCOID = -1;
                    string seniorContractSpecialistName = "";
                    int assistantDirectorCOID = -1;
                    string assistantDirectorName = "";

                    bSuccess = contractDB.GetContractOwnerRelatedInfo( editedDocumentContentBack.ContractNumber, ref COID, ref contractingOfficerFullName, ref contractingOfficerPhone, ref contractingOfficerUserId, ref seniorContractSpecialistCOID, ref seniorContractSpecialistName, ref assistantDirectorCOID, ref assistantDirectorName );
                    if( bSuccess == true )
                    {
                        editedDocumentContentBack.ContractingOfficerFullName = contractingOfficerFullName;
                        editedDocumentContentBack.ContractingOfficerPhone = contractingOfficerPhone;
                        editedDocumentContentBack.SeniorContractSpecialistCOID = seniorContractSpecialistCOID;
                        editedDocumentContentBack.SeniorContractSpecialistName = seniorContractSpecialistName;
                        editedDocumentContentBack.AssistantDirectorCOID = assistantDirectorCOID;
                        editedDocumentContentBack.AssistantDirectorName = assistantDirectorName;

                        editedDocumentContentBack.RefreshCurrentDocumentAssignmentFromEditedDocument();
                    }
                    else
                    {
                        HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                        HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to retrieve contract assignment related data for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                    }
                }
                else
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract assignment for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            // Contract Vendor
            if( editedDocumentContentBack.DirtyFlags.IsContractVendorSocioDirty == true )
            {
                bSuccess = contractDB.UpdateContractVendorSocio( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                                    editedDocumentContentBack.SocioBusinessSizeId, editedDocumentContentBack.SocioVetStatusId,
                                    editedDocumentContentBack.SocioWomanOwned, editedDocumentContentBack.SocioSDB, editedDocumentContentBack.Socio8a,
                                    editedDocumentContentBack.HubZone );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the vendor socioeconomic status for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsContractVendorAttributesDirty == true )
            {
                bSuccess = contractDB.UpdateContractVendorAttributes( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber, editedDocumentContentBack.SAMUEI,
                                    editedDocumentContentBack.DUNS, editedDocumentContentBack.TIN, editedDocumentContentBack.VendorTypeId,
                                    editedDocumentContentBack.CreditCardAccepted, editedDocumentContentBack.HazardousMaterial );
                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the vendor attributes for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsContractVendorInsurancePolicyDirty == true )
            {
                bSuccess = contractDB.UpdateContractVendorInsurancePolicy( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                                    editedDocumentContentBack.InsurancePolicyEffectiveDate, editedDocumentContentBack.InsurancePolicyExpirationDate );
                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the vendor insurance policy dates for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsWarrantyInformationDirty == true )
            {
                bSuccess = contractDB.UpdateContractVendorWarrantyInformation( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.WarrantyDuration, editedDocumentContentBack.WarrantyNotes );
                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the vendor warranty information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsReturnedGoodsPolicyInformationDirty == true )
            {
                bSuccess = contractDB.UpdateContractVendorReturnedGoodsPolicy( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.ReturnedGoodsPolicyTypeId, editedDocumentContentBack.ReturnedGoodsPolicyNotes );
                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the vendor returned goods policy for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsGeographicCoverageDirty == true )
            {
                bSuccess = contractDB.UpdateContractVendorGeographicCoverage( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber, editedDocumentContentBack.GeographicCoverage.GeographicCoverageId,
                    		editedDocumentContentBack.GeographicCoverage.Group52, 
			                editedDocumentContentBack.GeographicCoverage.Group51,
			                editedDocumentContentBack.GeographicCoverage.Group50,
			                editedDocumentContentBack.GeographicCoverage.Group49,
                            editedDocumentContentBack.GeographicCoverage.AL,
                            editedDocumentContentBack.GeographicCoverage.AK,
                            editedDocumentContentBack.GeographicCoverage.AZ,
                            editedDocumentContentBack.GeographicCoverage.AR,
                            editedDocumentContentBack.GeographicCoverage.CA,
                            editedDocumentContentBack.GeographicCoverage.CO,
                            editedDocumentContentBack.GeographicCoverage.CT,
                            editedDocumentContentBack.GeographicCoverage.DE,
                            editedDocumentContentBack.GeographicCoverage.DC,
                            editedDocumentContentBack.GeographicCoverage.FL,
                            editedDocumentContentBack.GeographicCoverage.GA,
                            editedDocumentContentBack.GeographicCoverage.HI,
                            editedDocumentContentBack.GeographicCoverage.ID,
                            editedDocumentContentBack.GeographicCoverage.IL,
                            editedDocumentContentBack.GeographicCoverage.IN,
                            editedDocumentContentBack.GeographicCoverage.IA,
                            editedDocumentContentBack.GeographicCoverage.KS,
                            editedDocumentContentBack.GeographicCoverage.KY,
                            editedDocumentContentBack.GeographicCoverage.LA,
                            editedDocumentContentBack.GeographicCoverage.ME,
                            editedDocumentContentBack.GeographicCoverage.MD,
                            editedDocumentContentBack.GeographicCoverage.MA,
                            editedDocumentContentBack.GeographicCoverage.MI,
                            editedDocumentContentBack.GeographicCoverage.MN,
                            editedDocumentContentBack.GeographicCoverage.MS,
                            editedDocumentContentBack.GeographicCoverage.MO,
                            editedDocumentContentBack.GeographicCoverage.MT,
                            editedDocumentContentBack.GeographicCoverage.NE,
                            editedDocumentContentBack.GeographicCoverage.NV,
                            editedDocumentContentBack.GeographicCoverage.NH,
                            editedDocumentContentBack.GeographicCoverage.NJ,
                            editedDocumentContentBack.GeographicCoverage.NM,
                            editedDocumentContentBack.GeographicCoverage.NY,
                            editedDocumentContentBack.GeographicCoverage.NC,
                            editedDocumentContentBack.GeographicCoverage.ND,
                            editedDocumentContentBack.GeographicCoverage.OH,
                            editedDocumentContentBack.GeographicCoverage.OK,
                            editedDocumentContentBack.GeographicCoverage.OR,
                            editedDocumentContentBack.GeographicCoverage.PA,
                            editedDocumentContentBack.GeographicCoverage.RI,
                            editedDocumentContentBack.GeographicCoverage.SC,
                            editedDocumentContentBack.GeographicCoverage.SD,
                            editedDocumentContentBack.GeographicCoverage.TN,
                            editedDocumentContentBack.GeographicCoverage.TX,
                            editedDocumentContentBack.GeographicCoverage.UT,
                            editedDocumentContentBack.GeographicCoverage.VT,
                            editedDocumentContentBack.GeographicCoverage.VA,
                            editedDocumentContentBack.GeographicCoverage.WA,
                            editedDocumentContentBack.GeographicCoverage.WV,
                            editedDocumentContentBack.GeographicCoverage.WI,
                            editedDocumentContentBack.GeographicCoverage.WY,
                            editedDocumentContentBack.GeographicCoverage.PR,
                            editedDocumentContentBack.GeographicCoverage.AB,
                            editedDocumentContentBack.GeographicCoverage.BC,
                            editedDocumentContentBack.GeographicCoverage.MB,
                            editedDocumentContentBack.GeographicCoverage.NB,
                            editedDocumentContentBack.GeographicCoverage.NF,
                            editedDocumentContentBack.GeographicCoverage.NT,
                            editedDocumentContentBack.GeographicCoverage.NS,
                            editedDocumentContentBack.GeographicCoverage.ON,
                            editedDocumentContentBack.GeographicCoverage.PE,
                            editedDocumentContentBack.GeographicCoverage.QC,
                            editedDocumentContentBack.GeographicCoverage.SK,
                            editedDocumentContentBack.GeographicCoverage.YT );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the geographic coverage for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsContractDetailsAttributesDirty == true )
            {
                bSuccess = contractDB.UpdateContractDetailsAttributes( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.EstimatedContractValue, editedDocumentContentBack.FPRFreeFormatDateString, editedDocumentContentBack.IffTypeId, editedDocumentContentBack.SolicitationNumber, 
                    editedDocumentContentBack.TrackingCustomerName, editedDocumentContentBack.Ratio, editedDocumentContentBack.MinimumOrder);
                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract detailed attributes for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsContractDetailsDeliveryDirty == true )
            {
                bSuccess = contractDB.UpdateContractDetailsDelivery( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.DeliveryTerms, editedDocumentContentBack.ExpeditedDeliveryTerms );
                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract detailed delivery information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsContractDetailsDiscountDirty == true )
            {
                bSuccess = contractDB.UpdateContractDetailsDiscount( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.BasicDiscount, editedDocumentContentBack.AdditionalDiscount, editedDocumentContentBack.EndOfYearDiscount,
                    editedDocumentContentBack.PromptPayDiscount, editedDocumentContentBack.QuantityDiscount, editedDocumentContentBack.CreditCardDiscount );
                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract detailed discount information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            //VendorContractAdministratorFormView
            //VendorAlternateContactFormView
            //VendorTechnicalContactFormView
            //VendorEmergencyContactFormView
            //VendorOrderingContactFormView
            //VendorSalesContactFormView
            //VendorBusinessAddressFormView

            if( editedDocumentContentBack.DirtyFlags.IsVendorContractAdministratorDirty == true )
            {
                bSuccess = contractDB.UpdateContractContact( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    Enum.GetName( typeof( EditedDocumentContent.ContactTypes ), EditedDocumentContent.ContactTypes.ADM ),
                    editedDocumentContentBack.VendorPrimaryContactName,
                    editedDocumentContentBack.VendorPrimaryContactPhone,
                    editedDocumentContentBack.VendorPrimaryContactExtension,
                    editedDocumentContentBack.VendorPrimaryContactFax,
                    editedDocumentContentBack.VendorPrimaryContactEmail,
                    "", "", "", -1, "", "", "" );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract contact information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsVendorAlternateContactDirty == true )
            {
                bSuccess = contractDB.UpdateContractContact( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    Enum.GetName( typeof( EditedDocumentContent.ContactTypes ), EditedDocumentContent.ContactTypes.ALT ),
                    editedDocumentContentBack.VendorAlternateContactName,
                    editedDocumentContentBack.VendorAlternateContactPhone,
                    editedDocumentContentBack.VendorAlternateContactExtension,
                    editedDocumentContentBack.VendorAlternateContactFax,
                    editedDocumentContentBack.VendorAlternateContactEmail,
                    "", "", "", -1, "", "", "" );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract contact information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsVendorTechnicalContactDirty == true )
            {
                bSuccess = contractDB.UpdateContractContact( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    Enum.GetName( typeof( EditedDocumentContent.ContactTypes ), EditedDocumentContent.ContactTypes.TECH ),
                    editedDocumentContentBack.VendorTechnicalContactName,
                    editedDocumentContentBack.VendorTechnicalContactPhone,
                    editedDocumentContentBack.VendorTechnicalContactExtension,
                    editedDocumentContentBack.VendorTechnicalContactFax,
                    editedDocumentContentBack.VendorTechnicalContactEmail,
                    "", "", "", -1, "", "", "" );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract contact information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsVendorEmergencyContactDirty == true )
            {
                bSuccess = contractDB.UpdateContractContact( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    Enum.GetName( typeof( EditedDocumentContent.ContactTypes ), EditedDocumentContent.ContactTypes.EMER ),
                    editedDocumentContentBack.VendorEmergencyContactName,
                    editedDocumentContentBack.VendorEmergencyContactPhone,
                    editedDocumentContentBack.VendorEmergencyContactExtension,
                    editedDocumentContentBack.VendorEmergencyContactFax,
                    editedDocumentContentBack.VendorEmergencyContactEmail,
                    "", "", "", -1, "", "", "" );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract contact information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsVendorSalesContactDirty == true )
            {
                bSuccess = contractDB.UpdateContractContact( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    Enum.GetName( typeof( EditedDocumentContent.ContactTypes ), EditedDocumentContent.ContactTypes.SALE ),
                    editedDocumentContentBack.VendorSalesContactName,
                    editedDocumentContentBack.VendorSalesContactPhone,
                    editedDocumentContentBack.VendorSalesContactExtension,
                    editedDocumentContentBack.VendorSalesContactFax,
                    editedDocumentContentBack.VendorSalesContactEmail,
                    "", "", "", -1, "", "", "" );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract contact information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsVendorOrderingContactDirty == true )
            {
                bSuccess = contractDB.UpdateContractContact( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    Enum.GetName( typeof( EditedDocumentContent.ContactTypes ), EditedDocumentContent.ContactTypes.ORD ),
                    "",
                    editedDocumentContentBack.OrderingTelephone,
                    editedDocumentContentBack.OrderingExtension,
                    editedDocumentContentBack.OrderingFax,
                    editedDocumentContentBack.OrderingEmail,
                    editedDocumentContentBack.OrderingAddress1,
                    editedDocumentContentBack.OrderingAddress2,
                    editedDocumentContentBack.OrderingCity,
                    editedDocumentContentBack.OrderingCountryId,
                    editedDocumentContentBack.OrderingState,
                    editedDocumentContentBack.OrderingZip,
                    "" );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract contact information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsVendorBusinessAddressDirty == true )
            {
                bSuccess = contractDB.UpdateContractContact( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    Enum.GetName( typeof( EditedDocumentContent.ContactTypes ), EditedDocumentContent.ContactTypes.BUS ),
                    editedDocumentContentBack.VendorName,
                    "",
                    "",
                    "",
                    "",
                    editedDocumentContentBack.VendorAddress1,
                    editedDocumentContentBack.VendorAddress2,
                    editedDocumentContentBack.VendorCity,
                    editedDocumentContentBack.VendorCountryId,
                    editedDocumentContentBack.VendorState,
                    editedDocumentContentBack.VendorZip,
                    editedDocumentContentBack.VendorWebAddress);
                
                if( bSuccess == true )
                {
                    // backfill contract country related data                    
                    string countryName = "";

                    bSuccess = contractDB.GetContractCountryInfo( editedDocumentContentBack.ContractNumber, ref countryName );
                    if( bSuccess == true )
                    {
                        editedDocumentContentBack.VendorCountryName = countryName;

                        editedDocumentContentBack.RefreshCurrentDocumentCountryInfoFromEditedDocument();
                    }
                    else
                    {
                        HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                        HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to retrieve contract country related data for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                    }                   
                }
                else
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract business address information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsPricelistVerificationDirty == true )
            {
                bSuccess = contractDB.UpdateContractPricelistVerification( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.PricelistVerificationDate, editedDocumentContentBack.PricelistVerifiedBy, editedDocumentContentBack.CurrentModNumber, editedDocumentContentBack.PricelistVerified );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract pricelist verification information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsPricelistNotesDirty == true )
            {
                bSuccess = contractDB.UpdateContractPricelistNotes( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.PricelistNotes );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract pricelist notes for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsRebateDirty == true )
            {
                bSuccess = contractDB.UpdateContractRebateHeader( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.RebateRequired );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract rebate information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsSBAHeaderDirty == true )
            {
                bSuccess = contractDB.UpdateContractSBAHeader( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.SBAPlanExempt, editedDocumentContentBack.SBAPlanId );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract sba plan information for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            if( editedDocumentContentBack.DirtyFlags.IsContractCommentDirty == true )
            {
                bSuccess = contractDB.UpdateContractComments( editedDocumentContentBack.ContractId, editedDocumentContentBack.ContractNumber,
                    editedDocumentContentBack.GeneralContractNotes );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the contract comments for contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
            }

            return ( 1 );
        }

        [DataObjectMethod( DataObjectMethodType.Delete, true )]
        public static int Delete( EditedDocumentContent editedDocumentContent )
        {
            return ( 1 );
        }

        [DataObjectMethod( DataObjectMethodType.Insert, true )]
        public static int CreateDocument( EditedDocumentContent editedDocumentContentBack, CustomDocumentInsertingEventHandler CustomInsertingEvent )
        {
            bool bSuccess = false;

            // save the values to the session
            HttpContext.Current.Session[ "EditedDocumentContentBack" ] = editedDocumentContentBack;

            ContractDB contractDB = ( ContractDB )HttpContext.Current.Session[ "ContractDB" ];

            contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            contractDB.MakeConnectionString();

            int newContractId = -1;
            int newPharmaceuticalContractId = -1;

            // same create statement for any of the three creation formviews
            if( editedDocumentContentBack.DirtyFlags.IsCreateContractFormViewDirty == true ||
                editedDocumentContentBack.DirtyFlags.IsVendorAddressFormViewDirty == true ||
                editedDocumentContentBack.DirtyFlags.IsVendorPOCFormViewDirty == true )
            {
                // note that the default SIN for National and BPA contracts will be created along with the contract
                bSuccess = contractDB.CreateContract2( editedDocumentContentBack.ScheduleNumber, editedDocumentContentBack.ContractNumber, editedDocumentContentBack.ContractAwardDate, editedDocumentContentBack.ContractEffectiveDate, editedDocumentContentBack.ContractExpirationDate, editedDocumentContentBack.TotalOptionYears, editedDocumentContentBack.ParentFSSContractNumber, editedDocumentContentBack.COID,
                                        editedDocumentContentBack.VendorName, editedDocumentContentBack.VendorPrimaryContactName, editedDocumentContentBack.VendorPrimaryContactPhone, editedDocumentContentBack.VendorPrimaryContactExtension, editedDocumentContentBack.VendorPrimaryContactFax, editedDocumentContentBack.VendorPrimaryContactEmail,
                                        editedDocumentContentBack.VendorAddress1, editedDocumentContentBack.VendorAddress2, editedDocumentContentBack.VendorCity, editedDocumentContentBack.VendorState, editedDocumentContentBack.VendorZip, editedDocumentContentBack.VendorCountryId, editedDocumentContentBack.VendorWebAddress, editedDocumentContentBack.OfferId, editedDocumentContentBack.RebateRequired, editedDocumentContentBack.SocioBusinessSizeId, editedDocumentContentBack.SolicitationNumber, ref newContractId, ref newPharmaceuticalContractId );
                if( bSuccess == true )
                {
                    editedDocumentContentBack.ContractId = newContractId;
                    editedDocumentContentBack.PharmaceuticalContractId = newPharmaceuticalContractId;
                }
                else
                {
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to create contract {0} : {1}", editedDocumentContentBack.ContractNumber, contractDB.ErrorMessage );
                }
     
                if( CustomInsertingEvent != null )
                {
                    CustomDocumentInsertingEventArgs CustomInsertingEventArgs = ( CustomDocumentInsertingEventArgs )new CustomDocumentInsertingEventArgs( editedDocumentContentBack );
                    CustomInsertingEvent( CustomInsertingEventArgs );
                }

            }
            return ( 1 );
        }
    }
}
