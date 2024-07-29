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
    public class EditedOfferManagerBack
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

        public EditedOfferManagerBack()
        {
            _currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];
            _contractDB = ( ContractDB )HttpContext.Current.Session[ "ContractDB" ];
            _drugItemDB = ( DrugItemDB )HttpContext.Current.Session[ "DrugItemDB" ];
            _offerDB = ( OfferDB )HttpContext.Current.Session[ "OfferDB" ];
        }

        // select function
        [DataObjectMethod( DataObjectMethodType.Select, true )]
        public EditedOfferContent GetEditedOfferContent()
        {
            EditedOfferContent editedOfferContentBack = null;

            if( HttpContext.Current.Session[ "EditedOfferContentBack" ] != null )
            {
                editedOfferContentBack = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentBack" ];

                editedOfferContentBack = GetOfferContent( editedOfferContentBack );               
            }
            else  // on initial load
            {
                editedOfferContentBack = GetOfferContent( new EditedOfferContent() );

                HttpContext.Current.Session[ "EditedOfferContentBack" ] = editedOfferContentBack;
            }

            return ( editedOfferContentBack );
        }

        private EditedOfferContent GetOfferContent( EditedOfferContent editedOfferContentBack )
        {

            bool bSuccess = false;

            DataSet dsOfferDetails = null;

            _offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _offerDB.MakeConnectionString();

            bSuccess = _offerDB.GetOfferInfoDetails( ref dsOfferDetails, _currentDocument.OfferId );

            if( bSuccess == true )
            {
                bSuccess = editedOfferContentBack.CompleteOfferDetails( dsOfferDetails ); // note that all dirty flags are cleared on success
                if( bSuccess == false )
                {
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to parse the offer details for offerId {0} : {1}", editedOfferContentBack.OfferId, editedOfferContentBack.ErrorMessage );
                }
            }
            else
            {
                HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to retrieve the offer details for offerId {0} : {1}", editedOfferContentBack.OfferId, _offerDB.ErrorMessage );
            }

            return ( editedOfferContentBack );
        }

        // update function
        [DataObjectMethod( DataObjectMethodType.Update, true )]
        public static int SaveChanges( EditedOfferContent editedOfferContentBack )
        {
            bool bSuccess = true;

            // save the values to the session
            HttpContext.Current.Session[ "EditedOfferContentBack" ] = editedOfferContentBack;

            OfferDB offerDB = ( OfferDB )HttpContext.Current.Session[ "OfferDB" ];

            offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            offerDB.MakeConnectionString();

            // Offer General
            if( editedOfferContentBack.DirtyFlags.IsOfferGeneralOfferAttributesDirty == true )
            {
                bSuccess = offerDB.UpdateOfferGeneralOfferAttributes( editedOfferContentBack.OfferId, editedOfferContentBack.SolicitationId, editedOfferContentBack.OfferNumber, editedOfferContentBack.ScheduleNumber, editedOfferContentBack.COID, editedOfferContentBack.ProposalTypeId, editedOfferContentBack.VendorName, editedOfferContentBack.ExtendsContractNumber );
                if( bSuccess == true )
                {
                    int COID = -1;
                    string contractingOfficerFullName = "";
                    string contractingOfficerPhone = "";
                    Guid contractingOfficerUserId = System.Guid.Empty;   // not used             
                    int assistantDirectorCOID = -1;
                    string assistantDirectorName = "";

                    bSuccess = offerDB.GetOfferOwnerRelatedInfo( editedOfferContentBack.OfferId, ref COID, ref contractingOfficerFullName, ref contractingOfficerPhone, ref contractingOfficerUserId, ref assistantDirectorCOID, ref assistantDirectorName );
                    if( bSuccess == true )
                    {
                        editedOfferContentBack.ContractingOfficerFullName = contractingOfficerFullName;
                        editedOfferContentBack.ContractingOfficerPhone = contractingOfficerPhone;
                        editedOfferContentBack.AssistantDirectorCOID = assistantDirectorCOID;
                        editedOfferContentBack.AssistantDirectorName = assistantDirectorName;

                        editedOfferContentBack.RefreshCurrentDocumentAttributesFromEditedDocument();
                    }
                    else
                    {
                        HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                        HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to retrieve offer assignment related data for offer id {0} : {1}", editedOfferContentBack.OfferId, offerDB.ErrorMessage );
                    }                    
                }
                else
                {
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the offer attributes for offerId {0} : {1}", editedOfferContentBack.OfferId, offerDB.ErrorMessage );
                }
            }

            // Offer Dates
            if( editedOfferContentBack.DirtyFlags.IsOfferGeneralOfferDatesDirty == true )
            {
                bSuccess = offerDB.UpdateOfferGeneralOfferDates( editedOfferContentBack.OfferId, editedOfferContentBack.DateReceived, editedOfferContentBack.DateAssigned, editedOfferContentBack.DateReassigned );
                if( bSuccess == true )
                {
                    editedOfferContentBack.RefreshCurrentDocumentDatesFromEditedDocument();
                }
                else
                {
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the offer dates for offerId {0} : {1}", editedOfferContentBack.OfferId, offerDB.ErrorMessage );
                }
            }

 

            // Offer Action Status
            if( editedOfferContentBack.DirtyFlags.IsOfferActionDirty == true )
            {
                bool bIsOfferCompleted = false;

                bSuccess = offerDB.UpdateOfferGeneralActionStatus( editedOfferContentBack.OfferId, editedOfferContentBack.ActionId,
                                    editedOfferContentBack.ActionDate, editedOfferContentBack.ExpectedCompletionDate,
                                    editedOfferContentBack.ExpirationDate, ref bIsOfferCompleted );

                if( bSuccess == true )
                {
                    editedOfferContentBack.IsOfferCompleted = bIsOfferCompleted;   

                    editedOfferContentBack.RefreshCurrentDocumentCompleteStatusFromEditedDocument();
                }
                else
                {
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the offer action status for offerId {0} : {1}", editedOfferContentBack.OfferId, offerDB.ErrorMessage );
                }
            }

            // Offer Audit Dates
            if( editedOfferContentBack.DirtyFlags.IsOfferAuditInformationDirty == true )
            {
                bSuccess = offerDB.UpdateOfferGeneralAuditInformation( editedOfferContentBack.OfferId, editedOfferContentBack.AuditIndicator,
                                    editedOfferContentBack.DateSentForPreaward, editedOfferContentBack.DateReturnedToOffice );
                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the offer audit information for offerId {0} : {1}", editedOfferContentBack.OfferId, offerDB.ErrorMessage );
                }
            }

            // offer primary contact
            if( editedOfferContentBack.DirtyFlags.IsVendorOfferContactDirty == true )
            {
                bSuccess = offerDB.UpdateOfferPrimaryContact( editedOfferContentBack.OfferId, 
                    editedOfferContentBack.VendorPrimaryContactName,
                    editedOfferContentBack.VendorPrimaryContactPhone,
                    editedOfferContentBack.VendorPrimaryContactExtension,
                    editedOfferContentBack.VendorPrimaryContactFax,
                    editedOfferContentBack.VendorPrimaryContactEmail );

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the offer primary contact information for offerId {0} : {1}", editedOfferContentBack.OfferId, offerDB.ErrorMessage );
                }
            }

            if( editedOfferContentBack.DirtyFlags.IsVendorBusinessAddressDirty == true )
            {
                bSuccess = offerDB.UpdateOfferVendorBusinessAddress( editedOfferContentBack.OfferId,
                    editedOfferContentBack.VendorAddress1,
                    editedOfferContentBack.VendorAddress2,
                    editedOfferContentBack.VendorCity,
                    editedOfferContentBack.VendorState,
                    editedOfferContentBack.VendorZip,
                    editedOfferContentBack.VendorCountry,
                    editedOfferContentBack.VendorCountryId,
                    editedOfferContentBack.VendorWebAddress);

                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the offer vendor business address for offerId {0} : {1}", editedOfferContentBack.OfferId, offerDB.ErrorMessage );
                }
            }
         
            // Offer comments
            if( editedOfferContentBack.DirtyFlags.IsOfferCommentDirty == true )
            {
                bSuccess = offerDB.UpdateOfferComment( editedOfferContentBack.OfferId, editedOfferContentBack.OfferComment );
                if( bSuccess != true )
                {
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to save the offer comment for offerId {0} : {1}", editedOfferContentBack.OfferId, offerDB.ErrorMessage );
                }
            }

            return ( 1 );
        }

        [DataObjectMethod( DataObjectMethodType.Delete, true )]
        public static int Delete( EditedOfferContent editedOfferContent )
        {
            return ( 1 );
        }

        [DataObjectMethod( DataObjectMethodType.Insert, true )]
        public static int CreateOffer( EditedOfferContent editedOfferContentBack, CustomOfferInsertingEventHandler CustomInsertingEvent )
        {
            bool bSuccess = false;

            // save the values to the session
            HttpContext.Current.Session[ "EditedOfferContentBack" ] = editedOfferContentBack;

            OfferDB offerDB = ( OfferDB )HttpContext.Current.Session[ "OfferDB" ];

            offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            offerDB.MakeConnectionString();

            int newOfferId = -1;
            string scheduleName = "";

            if( editedOfferContentBack.DirtyFlags.IsOfferActionDirty == true ||
                editedOfferContentBack.DirtyFlags.IsOfferAuditInformationDirty == true ||
                editedOfferContentBack.DirtyFlags.IsOfferCommentDirty == true ||
                editedOfferContentBack.DirtyFlags.IsOfferGeneralOfferAttributesDirty == true ||
                editedOfferContentBack.DirtyFlags.IsOfferGeneralOfferDatesDirty == true ||
                editedOfferContentBack.DirtyFlags.IsVendorBusinessAddressDirty == true ||
                editedOfferContentBack.DirtyFlags.IsVendorOfferContactDirty == true )
            {

                bSuccess = offerDB.CreateOffer( editedOfferContentBack.SolicitationId, editedOfferContentBack.OfferNumber, editedOfferContentBack.ScheduleNumber, editedOfferContentBack.COID, editedOfferContentBack.ProposalTypeId, editedOfferContentBack.VendorName, editedOfferContentBack.ExtendsContractNumber,
                                            editedOfferContentBack.DateReceived, editedOfferContentBack.DateAssigned, editedOfferContentBack.DateReassigned,
                                            editedOfferContentBack.ActionId, editedOfferContentBack.ActionDate, editedOfferContentBack.ExpectedCompletionDate, editedOfferContentBack.ExpirationDate,
                                            editedOfferContentBack.AuditIndicator, editedOfferContentBack.DateSentForPreaward, editedOfferContentBack.DateReturnedToOffice,
                                            editedOfferContentBack.VendorPrimaryContactName, editedOfferContentBack.VendorPrimaryContactPhone, editedOfferContentBack.VendorPrimaryContactExtension, editedOfferContentBack.VendorPrimaryContactFax, editedOfferContentBack.VendorPrimaryContactEmail,
                                            editedOfferContentBack.VendorAddress1, editedOfferContentBack.VendorAddress2, editedOfferContentBack.VendorCity, editedOfferContentBack.VendorState, editedOfferContentBack.VendorZip, editedOfferContentBack.VendorCountry, editedOfferContentBack.VendorCountryId, editedOfferContentBack.VendorWebAddress,
                                            editedOfferContentBack.OfferComment, ref newOfferId, ref scheduleName );
                if( bSuccess == true )
                {
                    editedOfferContentBack.OfferId = newOfferId;
                    editedOfferContentBack.ScheduleName = scheduleName;
                }
                else
                {
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = ErrorIndicator;
                    HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = string.Format( "The following error was encountered when attempting to create offer {0} : {1}", editedOfferContentBack.OfferNumber, offerDB.ErrorMessage );
                }

                if( CustomInsertingEvent != null )
                {
                    CustomOfferInsertingEventArgs customOfferInsertingEventArgs = ( CustomOfferInsertingEventArgs )new CustomOfferInsertingEventArgs( editedOfferContentBack );
                    CustomInsertingEvent( customOfferInsertingEventArgs );
                }
            }

            return ( 1 );
        }
    }
}
