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
    // content for the current offer as it is edited
    [Serializable]
    public class EditedOfferContent
    {
        CurrentDocument _currentDocument = null;
        bool _bOfferCreationInProgress = false;

        public EditedOfferContent()
        {
            _currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];

            // current document does not exist during offer creation
            if( _currentDocument != null )
            {
                _offerId = _currentDocument.OfferId;
                _offerNumber = _currentDocument.OfferNumber;
                _scheduleNumber = _currentDocument.ScheduleNumber;
                _scheduleName = _currentDocument.ScheduleName;
                _COID = _currentDocument.OwnerId;
                _contractingOfficerFullName = _currentDocument.OwnerName;
                _proposalTypeId = _currentDocument.ProposalTypeId;
                _bIsOfferCompleted = _currentDocument.IsOfferCompleted;
                _dateAssigned = _currentDocument.DateOfferAssigned;  // could be min value which is null
                _dateReceived = _currentDocument.DateOfferReceived;
                _contractNumber = _currentDocument.ContractNumber;
                _contractId = _currentDocument.ContractId;
                _extendsContractNumber = _currentDocument.ExtendsContractNumber;
                _extendsContractId = _currentDocument.ExtendsContractId;
                
                _vendorName = _currentDocument.VendorName;
            }
            else
            {
                _bOfferCreationInProgress = true;
            }

            _proposalTypeDescriptions = new ArrayList();
            _proposalTypeDescriptions.Add( new ProposalType( 3, "None Selected" ) );
            _proposalTypeDescriptions.Add( new ProposalType( 1, "Offer Proposal" ) );
            _proposalTypeDescriptions.Add( new ProposalType( 2, "Contract Extension Proposal" ) );

            if( _contractNumber != null )
            {
                if( _contractNumber.Trim().Length > 0 )
                {
                    _awardedContractDetails = new AwardedContract();
                }
            }

            _dirtyFlags = new DocumentDirtyFlags();
            ClearDirtyFlags();
        }

        public void RefreshCurrentDocumentDatesFromEditedDocument()
        {
            _currentDocument.DateOfferAssigned = _dateAssigned;
            _currentDocument.DateOfferReceived = _dateReceived;
        }

        public void RefreshCurrentDocumentAttributesFromEditedDocument()
        {
            _currentDocument.OfferNumber = _offerNumber;
            _currentDocument.ScheduleNumber = _scheduleNumber;
            _currentDocument.ScheduleName = _scheduleName;
            _currentDocument.OwnerId = _COID;
            _currentDocument.OwnerName = _contractingOfficerFullName;
            _currentDocument.ProposalTypeId = _proposalTypeId;           
            _currentDocument.VendorName = _vendorName;
            _currentDocument.OfferId = _offerId;   // used during insert
            _currentDocument.ExtendsContractNumber = _extendsContractNumber;

            // update the current document edit status due to the new owner
            BrowserSecurity2 browserSecurity = ( BrowserSecurity2 )HttpContext.Current.Session[ "BrowserSecurity" ];
            if( browserSecurity != null )
            {
                browserSecurity.SetDocumentEditStatus( _currentDocument );
            }
        }

        public void RefreshCurrentDocumentCompleteStatusFromEditedDocument()
        {
            _currentDocument.IsOfferCompleted = _bIsOfferCompleted;
        }

        public bool CompleteOfferDetails( DataSet dsOfferDetails )
        {
            bool bSuccess = false;
            if( dsOfferDetails != null )
            {
                if( dsOfferDetails.Tables[ OfferDB.OFFERINFODETAILSTABLENAME ] != null )
                {
                    if( dsOfferDetails.Tables[ OfferDB.OFFERINFODETAILSTABLENAME ].Rows.Count > 0 )
                    {
                        DataRow row = dsOfferDetails.Tables[ OfferDB.OFFERINFODETAILSTABLENAME ].Rows[ 0 ];

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
                            _errorMessage = "Row returned from offer details table was null.";
                        }
                    }
                    else
                    {
                        _errorMessage = "Offer details table had zero rows.";
                    }
                }
                else
                {
                    _errorMessage = "Offer details table was not found in the returned recordset.";
                }
            }
            else
            {
                _errorMessage = "The recordset returned was null.";
            }

            return ( bSuccess );
        }

        // copy from sourceObj to this
        public void CopyFrom( EditedOfferContent sourceObj )
        {

            _offerId = sourceObj.OfferId;
            _offerNumber = sourceObj.OfferNumber;
            _scheduleNumber = sourceObj.ScheduleNumber;
            _COID = sourceObj.COID;
            _contractingOfficerFullName = sourceObj.ContractingOfficerFullName;
            _proposalTypeId = sourceObj.ProposalTypeId;
            _bIsOfferCompleted = sourceObj.IsOfferCompleted;
            _dateAssigned = sourceObj.DateAssigned;
            _dateReassigned = sourceObj.DateReassigned;
            _dateReceived = sourceObj.DateReceived;
            _contractNumber = sourceObj.ContractNumber;
            _contractId = sourceObj.ContractId;
            _extendsContractNumber = sourceObj.ExtendsContractNumber;
            _vendorName = sourceObj.VendorName; 
            
            _solicitationNumber = sourceObj.SolicitationNumber;
            _solicitationId = sourceObj.SolicitationId;
            _actionDate = sourceObj.ActionDate;
            _actionId = sourceObj.ActionId;
            _offerActionDescription = sourceObj.OfferActionDescription;
            _expectedCompletionDate = sourceObj.ExpectedCompletionDate;
            _expirationDate = sourceObj.ExpirationDate;
            _dateSentForPreaward = sourceObj.DateSentForPreaward;
            _dateReturnedToOffice = sourceObj.DateReturnedToOffice;
            _offerComment = sourceObj.OfferComment;
            _bAuditIndicator = sourceObj.AuditIndicator;

            _vendorAddress1 = sourceObj.VendorAddress1;
            _vendorAddress2 = sourceObj.VendorAddress2;
            _vendorCity = sourceObj.VendorCity;
            _vendorState = sourceObj.VendorState;
            _vendorZip = sourceObj.VendorZip;
            _vendorCountry = sourceObj.VendorCountry;
            _vendorCountryId = sourceObj.VendorCountryId;
            _vendorWebAddress = sourceObj.VendorWebAddress;

            _vendorPrimaryContactName = sourceObj.VendorPrimaryContactName;
            _vendorPrimaryContactPhone = sourceObj.VendorPrimaryContactPhone;
            _vendorPrimaryContactExtension = sourceObj.VendorPrimaryContactExtension;
            _vendorPrimaryContactFax = sourceObj.VendorPrimaryContactFax;
            _vendorPrimaryContactEmail = sourceObj.VendorPrimaryContactEmail;

            _createdBy = sourceObj.CreatedBy;
            _creationDate = sourceObj.CreationDate;
            _lastModifiedBy = sourceObj.LastModifiedBy;
            _lastModificationDate = sourceObj.LastModificationDate;

            _contractingOfficerPhone = sourceObj.ContractingOfficerPhone;
            _scheduleName = sourceObj.ScheduleName;
            _assistantDirectorCOID = sourceObj.AssistantDirectorCOID;
            _assistantDirectorName = sourceObj.AssistantDirectorName;

            if( _contractNumber != null )
            {
                if( _contractNumber.Trim().Length > 0 )
                {
                    _awardedContractDetails = new AwardedContract();

                    _awardedContractDetails.ContractAwardDate = sourceObj.AwardedContractDetails.ContractAwardDate;
                    _awardedContractDetails.ContractExpirationDate = sourceObj.AwardedContractDetails.ContractExpirationDate;
                    _awardedContractDetails.ContractCompletionDate = sourceObj.AwardedContractDetails.ContractCompletionDate;
                    _awardedContractDetails.AwardedContractingOfficerFullName = sourceObj.AwardedContractDetails.AwardedContractingOfficerFullName;
                }
            }

            _dirtyFlags = null;
            _dirtyFlags = new DocumentDirtyFlags( sourceObj.DirtyFlags );
        }

        private void CopyRowToObjectMembers( DataRow row )
        {
            if( row[ "SolicitationId" ] != DBNull.Value )
            {
                _solicitationId = int.Parse( row[ "SolicitationId" ].ToString() );
            }

            if( row[ "ActionId" ] != DBNull.Value )
            {
                _actionId = int.Parse( row[ "ActionId" ].ToString() );
            }

            if( row[ "OfferActionDescription" ] != DBNull.Value )
            {
                _offerActionDescription = row[ "OfferActionDescription" ].ToString();
            }

            if( row[ "IsOfferComplete" ] != DBNull.Value )
            {
                _bIsOfferCompleted = bool.Parse( row[ "IsOfferComplete" ].ToString() );
            }

            if( row[ "VendorName" ] != DBNull.Value )
            {
                _vendorName = row[ "VendorName" ].ToString();
            }

            if( row[ "VendorAddress1" ] != DBNull.Value )
            {
                _vendorAddress1 = row[ "VendorAddress1" ].ToString();
            }

            if( row[ "VendorAddress2" ] != DBNull.Value )
            {
                _vendorAddress2 = row[ "VendorAddress2" ].ToString();
            }

            if( row[ "VendorAddressCity" ] != DBNull.Value )
            {
                _vendorCity = row[ "VendorAddressCity" ].ToString();
            }

            if( row[ "VendorAddressState" ] != DBNull.Value )
            {
                _vendorState = row[ "VendorAddressState" ].ToString();
            }

            if( row[ "VendorZipCode" ] != DBNull.Value )
            {
                _vendorZip = row[ "VendorZipCode" ].ToString();
            }

            if( row[ "VendorCountry" ] != DBNull.Value )
            {
                _vendorCountry = row[ "VendorCountry" ].ToString();
            }

            if( row[ "VendorCountryId" ] != DBNull.Value )
            {
                _vendorCountryId = int.Parse( row[ "VendorCountryId" ].ToString() );
            }

            if( row[ "VendorContactName" ] != DBNull.Value )
            {
                _vendorPrimaryContactName = row[ "VendorContactName" ].ToString();
            }

            if( row[ "VendorContactPhone" ] != DBNull.Value )
            {
                _vendorPrimaryContactPhone = row[ "VendorContactPhone" ].ToString();
            }

            if( row[ "VendorContactPhoneExtension" ] != DBNull.Value )
            {
                _vendorPrimaryContactExtension = row[ "VendorContactPhoneExtension" ].ToString();
            }

            if( row[ "VendorContactFax" ] != DBNull.Value )
            {
                _vendorPrimaryContactFax = row[ "VendorContactFax" ].ToString();
            }

            if( row[ "VendorContactEmail" ] != DBNull.Value )
            {
                _vendorPrimaryContactEmail = row[ "VendorContactEmail" ].ToString();
            }

            if( row[ "VendorUrl" ] != DBNull.Value )
            {
                _vendorWebAddress = row[ "VendorUrl" ].ToString();
            }

            if( row[ "DateReceived" ] != DBNull.Value )
            {
                _dateReceived = DateTime.Parse( row[ "DateReceived" ].ToString() );
            }

            if( row[ "DateAssigned" ] != DBNull.Value )
            {
                _dateAssigned = DateTime.Parse( row[ "DateAssigned" ].ToString() );
            }

            if( row[ "DateReassigned" ] != DBNull.Value )
            {
                _dateReassigned = DateTime.Parse( row[ "DateReassigned" ].ToString() );
            }

            if( row[ "ActionDate" ] != DBNull.Value )
            {
                _actionDate = DateTime.Parse( row[ "ActionDate" ].ToString() );
            }

            if( row[ "ExpectedCompletionDate" ] != DBNull.Value )
            {
                _expectedCompletionDate = DateTime.Parse( row[ "ExpectedCompletionDate" ].ToString() );
            }

            if( row[ "ExpirationDate" ] != DBNull.Value )
            {
                _expirationDate = DateTime.Parse( row[ "ExpirationDate" ].ToString() );
            }

            if( row[ "DateSentForPreAward" ] != DBNull.Value )
            {
                _dateSentForPreaward = DateTime.Parse( row[ "DateSentForPreAward" ].ToString() );
            }

            if( row[ "DateReturnedToOffice" ] != DBNull.Value )
            {
                _dateReturnedToOffice = DateTime.Parse( row[ "DateReturnedToOffice" ].ToString() );
            }

            if( row[ "OfferComment" ] != DBNull.Value )
            {
                _offerComment = row[ "OfferComment" ].ToString();
            }

            if( row[ "AuditIndicator" ] != DBNull.Value )
            {
                _bAuditIndicator = bool.Parse( row[ "AuditIndicator" ].ToString() );
            }

            if( row[ "CreatedBy" ] != DBNull.Value )
            {
                _createdBy = row[ "CreatedBy" ].ToString();
            }

            if( row[ "CreationDate" ] != DBNull.Value )
            {
                _creationDate = DateTime.Parse( row[ "CreationDate" ].ToString() );
            }

            if( row[ "LastModifiedBy" ] != DBNull.Value )
            {
                _lastModifiedBy = row[ "LastModifiedBy" ].ToString();
            }

            if( row[ "LastModificationDate" ] != DBNull.Value )
            {
                _lastModificationDate = DateTime.Parse( row[ "LastModificationDate" ].ToString() );
            }

            if( row[ "SolicitationNumber" ] != DBNull.Value )
            {
                _solicitationNumber = row[ "SolicitationNumber" ].ToString();
            }

            // this is the phone of the offer CO
            if( row[ "ContractingOfficerPhone" ] != DBNull.Value )
            {
                _contractingOfficerPhone = row[ "ContractingOfficerPhone" ].ToString();
            }

            if( row[ "Asst_Director" ] != DBNull.Value )
            {
                _assistantDirectorCOID = int.Parse( row[ "Asst_Director" ].ToString() );
            }

            if( row[ "AssistantDirectorName" ] != DBNull.Value )
            {
                _assistantDirectorName = row[ "AssistantDirectorName" ].ToString();
            }

            if( _contractNumber != null )
            {
                if( _contractNumber.Trim().Length > 0 )
                {
                    if( _awardedContractDetails == null )
                        _awardedContractDetails = new AwardedContract();

                    if( row[ "ContractAwardDate" ] != DBNull.Value )
                    {
                        _awardedContractDetails.ContractAwardDate = DateTime.Parse( row[ "ContractAwardDate" ].ToString() );
                    }

                    if( row[ "ContractExpirationDate" ] != DBNull.Value )
                    {
                        _awardedContractDetails.ContractExpirationDate = DateTime.Parse( row[ "ContractExpirationDate" ].ToString() );
                    }

                    if( row[ "ContractCompletionDate" ] != DBNull.Value )
                    {
                        _awardedContractDetails.ContractCompletionDate = DateTime.Parse( row[ "ContractCompletionDate" ].ToString() );
                    }

                    if( row[ "AwardedContractingOfficerFullName" ] != DBNull.Value )
                    {
                        _awardedContractDetails.AwardedContractingOfficerFullName = row[ "AwardedContractingOfficerFullName" ].ToString();
                    }
                }
            }
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
            private bool _bImpactsExpiredOfferHeaderCache = false;

            public bool ImpactsExpiredOfferHeaderCache
            {
                get { return _bImpactsExpiredOfferHeaderCache; }
                set { _bImpactsExpiredOfferHeaderCache = value; }
            }

            private bool _bIsOfferHeaderInfoDirty = false;

            public bool IsOfferHeaderInfoDirty  // this is the header on the editor screen
            {
                get { return _bIsOfferHeaderInfoDirty; }
                set { _bIsOfferHeaderInfoDirty = value; }
            }

            private bool _bIsOfferGeneralOfferAttributesDirty = false;

            public bool IsOfferGeneralOfferAttributesDirty
            {
                get { return _bIsOfferGeneralOfferAttributesDirty; }
                set { _bIsOfferGeneralOfferAttributesDirty = value; }
            }
            
            private bool _bIsOfferGeneralOfferDatesDirty = false;

            public bool IsOfferGeneralOfferDatesDirty
            {
                get { return _bIsOfferGeneralOfferDatesDirty; }
                set { _bIsOfferGeneralOfferDatesDirty = value; }
            }

            // the following 7 offer date field changes plus 1 assigned CO are tracked to assist with validation
            private bool _bIsOfferCOIDDirty = false;

            public bool IsOfferCOIDDirty
            {
                get { return _bIsOfferCOIDDirty; }
                set { _bIsOfferCOIDDirty = value; }
            }

            private bool _bIsOfferGeneralOfferReceivedDateDirty = false;

            public bool IsOfferGeneralOfferReceivedDateDirty
            {
                get { return _bIsOfferGeneralOfferReceivedDateDirty; }
                set { _bIsOfferGeneralOfferReceivedDateDirty = value; }
            }
            
            private bool _bIsOfferGeneralOfferAssignmentDateDirty = false;

            public bool IsOfferGeneralOfferAssignmentDateDirty
            {
                get { return _bIsOfferGeneralOfferAssignmentDateDirty; }
                set { _bIsOfferGeneralOfferAssignmentDateDirty = value; }
            }
            
            private bool _bIsOfferGeneralOfferReassignmentDateDirty = false;

            public bool IsOfferGeneralOfferReassignmentDateDirty
            {
                get { return _bIsOfferGeneralOfferReassignmentDateDirty; }
                set { _bIsOfferGeneralOfferReassignmentDateDirty = value; }
            }
            
            private bool _bIsOfferGeneralOfferEstimatedCompletionDateDirty = false;

            public bool IsOfferGeneralOfferEstimatedCompletionDateDirty
            {
                get { return _bIsOfferGeneralOfferEstimatedCompletionDateDirty; }
                set { _bIsOfferGeneralOfferEstimatedCompletionDateDirty = value; }
            }
            
            private bool _bIsOfferGeneralOfferActionDateDirty = false;

            public bool IsOfferGeneralOfferActionDateDirty
            {
                get { return _bIsOfferGeneralOfferActionDateDirty; }
                set { _bIsOfferGeneralOfferActionDateDirty = value; }
            }
            
            private bool _bIsOfferGeneralOfferPreawardDateDirty = false;

            public bool IsOfferGeneralOfferPreawardDateDirty
            {
                get { return _bIsOfferGeneralOfferPreawardDateDirty; }
                set { _bIsOfferGeneralOfferPreawardDateDirty = value; }
            }
            
            private bool _bIsOfferGeneralOfferReturnedDateDirty = false;

            public bool IsOfferGeneralOfferReturnedDateDirty
            {
                get { return _bIsOfferGeneralOfferReturnedDateDirty; }
                set { _bIsOfferGeneralOfferReturnedDateDirty = value; }
            }

            private bool _bIsOfferActionDirty = false;

            public bool IsOfferActionDirty
            {
                get { return _bIsOfferActionDirty; }
                set { _bIsOfferActionDirty = value; }
            }

            private bool _bOfferAuditInformationDirty = false;

            public bool IsOfferAuditInformationDirty
            {
                get { return _bOfferAuditInformationDirty; }
                set { _bOfferAuditInformationDirty = value; }
            }

            private bool _bIsVendorOfferContactDirty = false;

            public bool IsVendorOfferContactDirty
            {
                get { return _bIsVendorOfferContactDirty; }
                set { _bIsVendorOfferContactDirty = value; }
            }

            private bool _bIsVendorBusinessAddressDirty = false;

            public bool IsVendorBusinessAddressDirty
            {
                get { return _bIsVendorBusinessAddressDirty; }
                set { _bIsVendorBusinessAddressDirty = value; }
            }

            private bool _bIsOfferCommentDirty = false;

            public bool IsOfferCommentDirty
            {
                get { return _bIsOfferCommentDirty; }
                set { _bIsOfferCommentDirty = value; }
            }

            //$$$FormViewAddition

            public void CopyFrom( DocumentDirtyFlags sourceObj )
            {
                _bIsOfferHeaderInfoDirty = sourceObj.IsOfferHeaderInfoDirty;

                _bIsOfferGeneralOfferAttributesDirty = sourceObj.IsOfferGeneralOfferAttributesDirty;
                _bIsOfferGeneralOfferDatesDirty = sourceObj.IsOfferGeneralOfferDatesDirty;
                _bIsOfferActionDirty = sourceObj.IsOfferActionDirty;

                _bOfferAuditInformationDirty = sourceObj.IsOfferAuditInformationDirty;
                _bIsVendorOfferContactDirty = sourceObj.IsVendorOfferContactDirty;
                _bIsVendorBusinessAddressDirty = sourceObj.IsVendorBusinessAddressDirty;
                _bIsOfferCommentDirty = sourceObj.IsOfferCommentDirty;

                _bIsOfferCOIDDirty = sourceObj.IsOfferCOIDDirty;
                _bIsOfferGeneralOfferReceivedDateDirty = sourceObj.IsOfferGeneralOfferReceivedDateDirty;
                _bIsOfferGeneralOfferAssignmentDateDirty = sourceObj.IsOfferGeneralOfferAssignmentDateDirty;
                _bIsOfferGeneralOfferReassignmentDateDirty = sourceObj.IsOfferGeneralOfferReassignmentDateDirty;
                _bIsOfferGeneralOfferEstimatedCompletionDateDirty = sourceObj.IsOfferGeneralOfferEstimatedCompletionDateDirty;
                _bIsOfferGeneralOfferActionDateDirty = sourceObj.IsOfferGeneralOfferActionDateDirty;
                _bIsOfferGeneralOfferPreawardDateDirty = sourceObj.IsOfferGeneralOfferPreawardDateDirty;
                _bIsOfferGeneralOfferReturnedDateDirty = sourceObj.IsOfferGeneralOfferReturnedDateDirty;
            }

            public DocumentDirtyFlags()
            {
            }

            public DocumentDirtyFlags( DocumentDirtyFlags sourceObj )
            {
                _bIsOfferHeaderInfoDirty = sourceObj.IsOfferHeaderInfoDirty;

                _bIsOfferGeneralOfferAttributesDirty = sourceObj.IsOfferGeneralOfferAttributesDirty;
                _bIsOfferGeneralOfferDatesDirty = sourceObj.IsOfferGeneralOfferDatesDirty;
                _bIsOfferActionDirty = sourceObj.IsOfferActionDirty;

                _bOfferAuditInformationDirty = sourceObj.IsOfferAuditInformationDirty;
                _bIsVendorOfferContactDirty = sourceObj.IsVendorOfferContactDirty;
                _bIsVendorBusinessAddressDirty = sourceObj.IsVendorBusinessAddressDirty;
                _bIsOfferCommentDirty = sourceObj.IsOfferCommentDirty;

                _bIsOfferCOIDDirty = sourceObj.IsOfferCOIDDirty;
                _bIsOfferGeneralOfferReceivedDateDirty = sourceObj.IsOfferGeneralOfferReceivedDateDirty;
                _bIsOfferGeneralOfferAssignmentDateDirty = sourceObj.IsOfferGeneralOfferAssignmentDateDirty;
                _bIsOfferGeneralOfferReassignmentDateDirty = sourceObj.IsOfferGeneralOfferReassignmentDateDirty;
                _bIsOfferGeneralOfferEstimatedCompletionDateDirty = sourceObj.IsOfferGeneralOfferEstimatedCompletionDateDirty;
                _bIsOfferGeneralOfferActionDateDirty = sourceObj.IsOfferGeneralOfferActionDateDirty;
                _bIsOfferGeneralOfferPreawardDateDirty = sourceObj.IsOfferGeneralOfferPreawardDateDirty;
                _bIsOfferGeneralOfferReturnedDateDirty = sourceObj.IsOfferGeneralOfferReturnedDateDirty;
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
            _dirtyFlags.ImpactsExpiredOfferHeaderCache = false;
            _dirtyFlags.IsOfferHeaderInfoDirty = false;

            _dirtyFlags.IsOfferGeneralOfferAttributesDirty = false;
            _dirtyFlags.IsOfferGeneralOfferDatesDirty = false;
            _dirtyFlags.IsOfferActionDirty = false;

            _dirtyFlags.IsOfferAuditInformationDirty = false;
            _dirtyFlags.IsVendorOfferContactDirty = false;
            _dirtyFlags.IsVendorBusinessAddressDirty = false;
            _dirtyFlags.IsOfferCommentDirty = false;

            _dirtyFlags.IsOfferCOIDDirty = false;
            _dirtyFlags.IsOfferGeneralOfferReceivedDateDirty = false;
            _dirtyFlags.IsOfferGeneralOfferAssignmentDateDirty = false;
            _dirtyFlags.IsOfferGeneralOfferReassignmentDateDirty = false;
            _dirtyFlags.IsOfferGeneralOfferEstimatedCompletionDateDirty = false;
            _dirtyFlags.IsOfferGeneralOfferActionDateDirty = false;
            _dirtyFlags.IsOfferGeneralOfferPreawardDateDirty = false;
            _dirtyFlags.IsOfferGeneralOfferReturnedDateDirty = false;
        }

        // summary of all flags
        public bool IsDocumentDirty()
        {
            return ( _dirtyFlags.IsOfferGeneralOfferAttributesDirty || _dirtyFlags.IsOfferGeneralOfferDatesDirty ||
                _dirtyFlags.IsOfferActionDirty || _dirtyFlags.IsOfferAuditInformationDirty ||
                _dirtyFlags.IsVendorOfferContactDirty || _dirtyFlags.IsVendorBusinessAddressDirty ||
                _dirtyFlags.IsOfferCommentDirty
                );
        }

        // * = items originate from CurrentDocument
        private int _offerId = -1;                   // *   [Offer_ID]                                        	int              NOT NULL IDENTITY (1, 1),
        private int _solicitationId = -1;            //[Solicitation_ID]                                 	int              NOT NULL,
        private int _COID = -1;                       // * [CO_ID]                                           	int              NOT NULL,
        private int _scheduleNumber = -1;                        // * [Schedule_Number]                                 	int              NOT NULL,
        private string _offerNumber = "";                    // * [OfferNumber]                                     	nvarchar(30)         NULL,
        private int _proposalTypeId = -1;                    // * [Proposal_Type_ID]                                	int              NOT NULL DEFAULT ((1)),
        private string _extendsContractNumber = "";           // * ExtendsContractNumber nvarchar(20)  added 6/20/2016
        private int _extendsContractId = -1;                    //*
        private int _actionId = -1;                    //[Action_ID]                                       	int              NOT NULL DEFAULT ((1)),
        
        private string _vendorName = "";                        // * [Contractor_Name]                                 	nvarchar(75)     NOT NULL,
        private string _vendorAddress1 = "";                       //[Primary_Address_1]                               	nvarchar(100)        NULL,
        private string _vendorAddress2 = "";               //[Primary_Address_2]   nvarchar(100)        NULL,
        private string _vendorCity = "";                   //[Primary_City]        nvarchar(20)         NULL,
        private string _vendorState = "";                  //[Primary_State]       nvarchar(2)          NULL,
        private string _vendorZip = "";                    //[Primary_Zip]         nvarchar(10)         NULL,

        private string _vendorCountry = "";                    //[Country]                                         	nvarchar(50)         NULL,
        private int _vendorCountryId = -1;                      //Primary_CountryId    int    NULL,

        private string _vendorPrimaryContactName = "";          //[POC_Primary_Name]   	nvarchar(30)         NULL,
        private string _vendorPrimaryContactPhone = "";         //[POC_Primary_Phone]   nvarchar(15)         NULL,
        private string _vendorPrimaryContactExtension = "";     //[POC_Primary_Ext]    	nvarchar(5)          NULL,
        private string _vendorPrimaryContactFax = "";           //[POC_Primary_Fax]     nvarchar(15)         NULL,
        private string _vendorPrimaryContactEmail = "";         //[POC_Primary_Email]   nvarchar(50)         NULL,
        private string _vendorWebAddress = "";                     //[POC_VendorWeb]                                   	nvarchar(50)         NULL,

        private DateTime _dateReceived;                     // * [Dates_Received]                                  	datetime         NOT NULL,
        private DateTime _dateAssigned;                     // * [Dates_Assigned]                                  	datetime             NULL,
        private DateTime _dateReassigned;                   //[Dates_Reassigned]                                	datetime             NULL,
        private DateTime _actionDate;                       //[Dates_Action]                                    	datetime         NOT NULL DEFAULT (getdate()),
        private DateTime _expectedCompletionDate;           //[Dates_Expected_Completion]                       	datetime             NULL,
        private DateTime _expirationDate;                   //[Dates_Expiration]                                	datetime             NULL,
        private DateTime _dateSentForPreaward;              //[Dates_Sent_for_Preaward]                         	datetime             NULL,
        private DateTime _dateReturnedToOffice;             //[Dates_Returned_to_Office]                        	datetime             NULL,
        private string _offerComment = "";                  //[Comments]                                        	nvarchar(4000)       NULL,
        private bool _bAuditIndicator = false;              //[Audit_Indicator]                                 	bit              NOT NULL DEFAULT ((0)),
        private string _contractNumber = "";                // * [ContractNumber]                                  	nvarchar(20)         NULL,
        private int _contractId = -1;                       // *
        private DateTime _creationDate;                     //[Date_Entered]                                    	datetime         NOT NULL DEFAULT (getdate()),
        private DateTime _lastModificationDate;             //[Date_Modified]                                   	datetime         NOT NULL DEFAULT (getdate()),
        private string _createdBy = "";                     //[CreatedBy]                                       	nvarchar(120)    NOT NULL DEFAULT (suser_sname()),
        private string _lastModifiedBy = "";                //[LastModifiedBy]                                  	nvarchar(120)    NOT NULL DEFAULT (suser_sname()),


        // the below items are lookup values that are not updated
        private string _solicitationNumber = "";        //    Solicitation_Number from tlkup_Solicitations
        private string _offerActionDescription = "";
        private bool _bIsOfferCompleted = false;      // * ( from currentdocument ) and represents the current status of the saved offer

        // the edited complete status is retrieved from a cached lookup table, based on the most recent selected actionId, as an edited offer is being saved ( this impacts cache of completed offers )
        private bool _bIsEditedOfferCompleted = false;

        public bool IsEditedOfferCompleted
        {
            get { return _bIsEditedOfferCompleted; }
            set { _bIsEditedOfferCompleted = value; }
        }

        private ArrayList _proposalTypeDescriptions = null;

        public ArrayList ProposalTypeDescriptions
        {
            get { return _proposalTypeDescriptions; }
        }

        private string _proposalTypeDescription = "";
   
        private string _contractingOfficerFullName = "";  // [FullName]
        private string _contractingOfficerPhone = ""; // [User_Phone]
        private string _scheduleName = "";                  // [Schedule_Name]
        private int _assistantDirectorCOID = -1;   
        private string _assistantDirectorName = "";



        [Serializable]
        public class ProposalType
        {

            private int _proposalTypeId = 0;

            public int ProposalTypeId
            {
                get { return _proposalTypeId; }
                set { _proposalTypeId = value; }
            }
            private string _proposalTypeDescription = "";

            public string ProposalTypeDescription
            {
                get { return _proposalTypeDescription; }
                set { _proposalTypeDescription = value; }
            }

            public ProposalType( int proposalTypeId, string proposalTypeDescription )
            {
                _proposalTypeId = proposalTypeId;
                _proposalTypeDescription = proposalTypeDescription;
            }
        }

        [Serializable]
        public class AwardedContract
        {
            // the contract number is stored with the offer

            private string _awardedContractingOfficerFullName = "";

            public string AwardedContractingOfficerFullName
            {
                get { return _awardedContractingOfficerFullName; }
                set { _awardedContractingOfficerFullName = value; }
            }

            private DateTime _contractAwardDate = DateTime.MinValue;

            public DateTime ContractAwardDate
            {
                get { return _contractAwardDate; }
                set { _contractAwardDate = value; }
            }

            private DateTime _contractExpirationDate = DateTime.MinValue;

            public DateTime ContractExpirationDate
            {
                get { return _contractExpirationDate; }
                set { _contractExpirationDate = value; }
            }

            private DateTime _contractCompletionDate = DateTime.MinValue;

            public DateTime ContractCompletionDate
            {
                get { return _contractCompletionDate; }
                set { _contractCompletionDate = value; }
            }

            public AwardedContract()
            {
            }

            public AwardedContract( AwardedContract sourceObj )
            {
                _awardedContractingOfficerFullName = sourceObj.AwardedContractingOfficerFullName;
                _contractAwardDate = sourceObj.ContractAwardDate;
                _contractExpirationDate = sourceObj.ContractExpirationDate;
                _contractCompletionDate = sourceObj.ContractCompletionDate;
            }

            public void CopyFrom( AwardedContract sourceObj )
            {
                _awardedContractingOfficerFullName = sourceObj.AwardedContractingOfficerFullName;
                _contractAwardDate = sourceObj.ContractAwardDate;
                _contractExpirationDate = sourceObj.ContractExpirationDate;
                _contractCompletionDate = sourceObj.ContractCompletionDate;
            }
        }

        private AwardedContract _awardedContractDetails = null;

        public AwardedContract AwardedContractDetails
        {
            get { return _awardedContractDetails; }
            set { _awardedContractDetails = value; }
        }


        #region StandardAccessors

        public int OfferId
        {
            get { return _offerId; }
            set { _offerId = value; }
        }

        public int SolicitationId
        {
            get { return _solicitationId; }
            set
            {
                if( _solicitationId.CompareTo( value ) != 0 )
                    _dirtyFlags.IsOfferGeneralOfferAttributesDirty = true; 
                _solicitationId = value;
            }
        }

        public int COID
        {
            get { return _COID; }
            set
            {
                if( _COID.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferAttributesDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
                    _dirtyFlags.IsOfferCOIDDirty = true;
                }
                _COID = value;
            }
        }

        public int ScheduleNumber
        {
            get { return _scheduleNumber; }
            set
            {
                if( _scheduleNumber.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferAttributesDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
                }
                _scheduleNumber = value;
            }
        }

        public string OfferNumber
        {
            get { return _offerNumber; }
            set
            {
                if( _offerNumber.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferAttributesDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
                }
                _offerNumber = value;
            }
        }

        public int ProposalTypeId
        {
            get { return _proposalTypeId; }
            set
            {
                if( _proposalTypeId.CompareTo( value ) != 0 )
                    _dirtyFlags.IsOfferGeneralOfferAttributesDirty = true; 
                _proposalTypeId = value;
            }
        }

        public string ExtendsContractNumber
        {
            get { return _extendsContractNumber; }
            set
            {
                if( _extendsContractNumber.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferAttributesDirty = true;                   
                }
                _extendsContractNumber = value;
            }
        }


        public int ExtendsContractId
        {
            get { return _extendsContractId; }
            set
            {
                if( _extendsContractId.CompareTo( value ) != 0 )
                    _dirtyFlags.IsOfferGeneralOfferAttributesDirty = true;
                _extendsContractId = value;
            }
        }

        public int ActionId
        {
            get { return _actionId; }
            set
            {
                if( _actionId.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferActionDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
                    if( IsEditedOfferCompleted == true )
                    {
                        _dirtyFlags.ImpactsExpiredOfferHeaderCache = true;
                    }
                }
                _actionId = value;
            }
        }

        public string VendorName
        {
            get { return _vendorName; }
            set
            {
                if( _vendorName.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferAttributesDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
                }
                _vendorName = value;
            }
        }

        public string VendorAddress1
        {
            get { return _vendorAddress1; }
            set
            {
                if( _vendorAddress1.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsVendorBusinessAddressDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
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
                    _dirtyFlags.IsVendorBusinessAddressDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
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
                    _dirtyFlags.IsVendorBusinessAddressDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
                }
                _vendorCity = value;
            }
        }

        public string VendorState
        {
            get { return _vendorState; }
            set
            {
                if( _vendorState.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsVendorBusinessAddressDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
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
                    _dirtyFlags.IsVendorBusinessAddressDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
                }
                _vendorZip = value;
            }
        }

        public string VendorCountry
        {
            get { return _vendorCountry; }
            set
            {
                if( _vendorCountry.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorBusinessAddressDirty = true;
                _vendorCountry = value;
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
                    _dirtyFlags.IsVendorBusinessAddressDirty = true;                    
                }
                _vendorCountryId = value;
            }
        }

        public string VendorPrimaryContactName
        {
            get { return _vendorPrimaryContactName; }
            set
            {
                if( _vendorPrimaryContactName.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsVendorOfferContactDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
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
                    _dirtyFlags.IsVendorOfferContactDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
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
                    _dirtyFlags.IsVendorOfferContactDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
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
                    _dirtyFlags.IsVendorOfferContactDirty = true;

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
                    _dirtyFlags.IsVendorOfferContactDirty = true;
                    _dirtyFlags.IsOfferHeaderInfoDirty = true;
                }

                _vendorPrimaryContactEmail = value;
            }
        }

        public string VendorWebAddress
        {
            get { return _vendorWebAddress; }
            set
            {
                if( _vendorWebAddress.CompareTo( value ) != 0 )
                    _dirtyFlags.IsVendorBusinessAddressDirty = true;
                _vendorWebAddress = value;
            }
        }

        public DateTime DateReceived
        {
            get { return _dateReceived; }
            set
            {
                if( _dateReceived.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferReceivedDateDirty = true;
                    _dirtyFlags.IsOfferGeneralOfferDatesDirty = true;
                }
                _dateReceived = value;
            }
        }

        public DateTime DateAssigned
        {
            get { return _dateAssigned; }
            set
            {
                if( _dateAssigned.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferAssignmentDateDirty = true;
                    _dirtyFlags.IsOfferGeneralOfferDatesDirty = true;
                }
                _dateAssigned = value;
            }
        }


        public DateTime DateReassigned
        {
            get { return _dateReassigned; }
            set
            {
                if( _dateReassigned.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferReassignmentDateDirty = true;
                    _dirtyFlags.IsOfferGeneralOfferDatesDirty = true;
                }
                _dateReassigned = value;
            }
        }

        public DateTime ActionDate
        {
            get { return _actionDate; }
            set
            {
                if( _actionDate.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferActionDateDirty = true;
                    _dirtyFlags.IsOfferActionDirty = true;
                }
                _actionDate = value;
            }
        }

        public DateTime ExpectedCompletionDate
        {
            get { return _expectedCompletionDate; }
            set
            {
                if( _expectedCompletionDate.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferEstimatedCompletionDateDirty = true;
                    _dirtyFlags.IsOfferActionDirty = true;
                }
                _expectedCompletionDate = value;
            }
        }

        public DateTime ExpirationDate
        {
            get { return _expirationDate; }
            set
            {
                if( _expirationDate.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferDatesDirty = true;
                }
                _expirationDate = value;
            }
        }

        public DateTime DateSentForPreaward
        {
            get { return _dateSentForPreaward; }
            set
            {
                if( _dateSentForPreaward.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferPreawardDateDirty = true;
                    _dirtyFlags.IsOfferAuditInformationDirty = true;
                }
                _dateSentForPreaward = value;
            }
        }

        public DateTime DateReturnedToOffice
        {
            get { return _dateReturnedToOffice; }
            set
            {
                if( _dateReturnedToOffice.CompareTo( value ) != 0 )
                {
                    _dirtyFlags.IsOfferGeneralOfferReturnedDateDirty = true;
                    _dirtyFlags.IsOfferAuditInformationDirty = true;
                }
                _dateReturnedToOffice = value;
            }
        }

        public string OfferComment
        {
            get { return _offerComment; }
            set
            {
                if( _offerComment.CompareTo( value ) != 0 )
                    _dirtyFlags.IsOfferCommentDirty = true; 
                _offerComment = value;
            }
        }

        public bool AuditIndicator
        {
            get { return _bAuditIndicator; }
            set
            {
                if( _bAuditIndicator.CompareTo( value ) != 0 )
                    _dirtyFlags.IsOfferAuditInformationDirty = true; 
                _bAuditIndicator = value;
            }
        }

        public string ContractNumber
        {
            get { return _contractNumber; }
            set { _contractNumber = value; }
        }

        public int ContractId
        {
            get { return _contractId; }
            set { _contractId = value; }
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

        // the below items are lookup values that are not updated

        public string SolicitationNumber
        {
            get { return _solicitationNumber; }
            set { _solicitationNumber = value; }
        }


        public string ProposalTypeDescription
        {
            get { return _proposalTypeDescription; }
            set { _proposalTypeDescription = value; }
        }

        public string OfferActionDescription
        {
            get { return _offerActionDescription; }
            set { _offerActionDescription = value; }
        }


        public bool IsOfferCompleted
        {
            get { return _bIsOfferCompleted; }
            set { _bIsOfferCompleted = value; }
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
            set { _scheduleName = value; } 
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

    }
}
