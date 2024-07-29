using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Linq;

using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class DocumentControlPresentation
    {
        CurrentDocument _currentDocument = null;
        EditedDocumentContent _editedDocumentContentFront = null;

        [Serializable]
        public class ControlInfo
        {
            private string _controlName = "";

            public string ControlName
            {
                get { return _controlName; }
                set { _controlName = value; }
            }

            private bool _bVisible = false;

            public bool IsVisible
            {
                get { return _bVisible; }
                set { _bVisible = value; }
            }

            private bool _bEnabled = false;

            public bool IsEnabled
            {
                get { return _bEnabled; }
                set { _bEnabled = value; }
            }

            public ControlInfo( string controlName, bool bVisible, bool bEnabled )
            {
                _controlName = controlName;
                _bVisible = bVisible;
                _bEnabled = bEnabled;
            }

        }

        private ArrayList _formViewList = null;

        public ArrayList FormViewList
        {
            get { return _formViewList; }
            set { _formViewList = value; }
        }

        private ArrayList _controlList = null;

        public ArrayList ControlList
        {
            get { return _controlList; }
            set { _controlList = value; }
        }

        public DocumentControlPresentation( CurrentDocument currentDocument )
        {
            _currentDocument = currentDocument;  // $$$ can this member be removed ?
            _formViewList = new ArrayList();
            _controlList = new ArrayList();
            InitControlLists( currentDocument );
        }

        // used during document creation
        public DocumentControlPresentation( bool bIsContract )
        {
            _currentDocument = null;
            _formViewList = new ArrayList();
            _controlList = new ArrayList();
            InitControlLists( bIsContract );
        }

        public bool IsFormViewVisible( string formViewName )
        {
            bool bVisible = false;

            for( int i = 0; i < _formViewList.Count; i++ )
            {
                ControlInfo f = ( ControlInfo )_formViewList[ i ];
                if( f.ControlName.CompareTo( formViewName ) == 0 )
                {
                    bVisible = f.IsVisible;
                    break;
                }
            }

            return ( bVisible );
        }

        public bool IsFormViewEnabled( string formViewName )
        {
            bool bEnabled = false;

            for( int i = 0; i < _formViewList.Count; i++ )
            {
                ControlInfo f = ( ControlInfo )_formViewList[ i ];
                if( f.ControlName.CompareTo( formViewName ) == 0 )
                {
                    bEnabled = f.IsEnabled;
                    break;
                }
            }

            return ( bEnabled );
        }

        public bool IsFormViewVisibleAndEnabled( string formViewName )
        {
            bool bVisibleAndEnabled = false;

            for( int i = 0; i < _formViewList.Count; i++ )
            {
                ControlInfo f = ( ControlInfo )_formViewList[ i ];
                if( f.ControlName.CompareTo( formViewName ) == 0 )
                {
                    bVisibleAndEnabled = f.IsVisible & f.IsEnabled;
                    break;
                }
            }

            return ( bVisibleAndEnabled );
        }
        public bool IsControlVisible( string controlName )
        {
            bool bVisible = false;

            for( int i = 0; i < _controlList.Count; i++ )
            {
                ControlInfo c = ( ControlInfo )_controlList[ i ];
                if( c.ControlName.CompareTo( controlName ) == 0 )
                {
                    bVisible = c.IsVisible;
                    break;
                }
            }

            return ( bVisible );
        }

        public bool IsControlEnabled( string controlName )
        {
            bool bEnabled = false;

            for( int i = 0; i < _controlList.Count; i++ )
            {
                ControlInfo c = ( ControlInfo )_controlList[ i ];
                if( c.ControlName.CompareTo( controlName ) == 0 )
                {
                    bEnabled = c.IsEnabled;
                    break;
                }
            }

            return ( bEnabled );
        }

        public bool IsControlVisibleAndEnabled( string controlName )
        {
            bool bVisibleAndEnabled = false;

            for( int i = 0; i < _controlList.Count; i++ )
            {
                ControlInfo c = ( ControlInfo )_controlList[ i ];
                if( c.ControlName.CompareTo( controlName ) == 0 )
                {
                    bVisibleAndEnabled = c.IsVisible & c.IsEnabled;
                    break;
                }
            }

            return ( bVisibleAndEnabled );
        }

        public void UpdateControlVisibility( string controlName, bool bVisible )
        {
            for( int i = 0; i < _controlList.Count; i++ )
            {
                ControlInfo c = ( ControlInfo )_controlList[ i ];
                if( c.ControlName.CompareTo( controlName ) == 0 )
                {
                    c.IsVisible = bVisible;
                    break;
                }
            }
        }

        public void UpdateControlEnabled( string controlName, bool bEnabled )
        {
            for( int i = 0; i < _controlList.Count; i++ )
            {
                ControlInfo c = ( ControlInfo )_controlList[ i ];
                if( c.ControlName.CompareTo( controlName ) == 0 )
                {
                    c.IsEnabled = bEnabled;
                    break;
                }
            }
        }

        // set up visibility and enabled for document controls
        private void InitControlLists( CurrentDocument currentDocument )
        {
            // generally, controls are enabled if the contract is active.  However, individual controls may be disabled by security constraints
            bool bIsActive = ( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Active ) ? true : false;  // will be false for completed offers
            bool bMayEdit = ( currentDocument.EditStatus == CurrentDocument.EditStatuses.CanEdit ) ? true : false;
            bool bMayOnlyView = ( currentDocument.EditStatus == CurrentDocument.EditStatuses.CanView ) ? true : false;  // EditStatus = CanView only if cannot edit, so do not use this exclusively
            bool bAllowEdit = bIsActive & bMayEdit;
            bool bAllowView = bMayOnlyView | bMayEdit;  // combine with canedit 

            // contract
            if( currentDocument.DocumentType != CurrentDocument.DocumentTypes.Offer )
            {
                // general tab
                // dates always visible
                _formViewList.Add( new ControlInfo( "ContractGeneralContractDatesFormView", true, bAllowEdit ) );
                // attributes always visible
                _formViewList.Add( new ControlInfo( "ContractGeneralContractAttributesFormView", true, bAllowEdit ) );
                
                _formViewList.Add( new ControlInfo( "ContractGeneralContractAssignmentFormView", bAllowEdit, bAllowEdit ) );

                if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSS || currentDocument.DocumentType == CurrentDocument.DocumentTypes.National )
                {
                    _formViewList.Add( new ControlInfo( "ContractGeneralParentContractFormView", false, false ) );
                                                        
                    if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSS )
                    {
                        _controlList.Add( new ControlInfo( "AssociatedBPAContractsGridPanel", true, true ) );
                    }
                    else
                    {                          
                        _controlList.Add( new ControlInfo( "AssociatedBPAContractsGridPanel", false, false ) );
                    }
                }
                else if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.BPA || currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSSBPA )
                {
                    _formViewList.Add( new ControlInfo( "ContractGeneralParentContractFormView", true, true ) );  // allow jump even when not active

                    _controlList.Add( new ControlInfo( "AssociatedBPAContractsGridPanel", false, false ) );
                }

                if( currentDocument.CanHaveSINs( currentDocument.ScheduleNumber ) == true )
                {
                    _controlList.Add( new ControlInfo( "SINPanel", true, bAllowEdit ) );
                }
                else
                {
                    // default SINs are provided for National or SAC Contracts according to tbl_SINs on the backend ( SP )
                    _controlList.Add( new ControlInfo( "SINPanel", false, false ) );
                }

                // vendor tab
                if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSS || currentDocument.DocumentType == CurrentDocument.DocumentTypes.National )
                {
                    _formViewList.Add( new ControlInfo( "ContractVendorSocioFormView", true, bAllowEdit ) );
                    _formViewList.Add( new ControlInfo( "VendorAttributesFormView", true, bAllowEdit ) );
                    _formViewList.Add( new ControlInfo( "WarrantyInformationFormView", true, bAllowEdit ) );
                    _formViewList.Add( new ControlInfo( "ReturnedGoodsPolicyFormView", true, bAllowEdit ) );
                    _formViewList.Add( new ControlInfo( "StateFormView", true, bAllowEdit ) );

                    if( currentDocument.CanHaveInsurance( currentDocument.ScheduleNumber ) == true )
                    {
                        _formViewList.Add( new ControlInfo( "ContractVendorInsuranceDatesFormView", true, bAllowEdit ) );
                    }
                    else
                    {
                        _formViewList.Add( new ControlInfo( "ContractVendorInsuranceDatesFormView", false, false ) );
                    }
                }
                else if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.BPA || currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSSBPA )
                {
                    _formViewList.Add( new ControlInfo( "ContractVendorSocioFormView", false, false ) );
                    _formViewList.Add( new ControlInfo( "VendorAttributesFormView", false, false ) );
                    _formViewList.Add( new ControlInfo( "WarrantyInformationFormView", false, false ) );
                    _formViewList.Add( new ControlInfo( "ReturnedGoodsPolicyFormView", false, false ) );
                    _formViewList.Add( new ControlInfo( "StateFormView", true, bAllowEdit ) );

                    if( currentDocument.CanHaveInsurance( currentDocument.ScheduleNumber ) == true )
                    {
                        _formViewList.Add( new ControlInfo( "ContractVendorInsuranceDatesFormView", true, bAllowEdit ) );
                    }
                    else
                    {
                        _formViewList.Add( new ControlInfo( "ContractVendorInsuranceDatesFormView", false, false ) );
                    }
                }

                // contract details tab
                if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSS || currentDocument.DocumentType == CurrentDocument.DocumentTypes.National )
                {
                    _formViewList.Add( new ControlInfo( "ContractDetailsAttributesFormView", true, bAllowEdit ) );

                    _formViewList.Add( new ControlInfo( "DiscountFormView", true, bAllowEdit ) );
                    _formViewList.Add( new ControlInfo( "DeliveryFormView", true, bAllowEdit ) );
               }
                else if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.BPA || currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSSBPA )
                {
                    _formViewList.Add( new ControlInfo( "ContractDetailsAttributesFormView", true, bAllowEdit ) );

                    _formViewList.Add( new ControlInfo( "DiscountFormView", false, false ) );
                    _formViewList.Add( new ControlInfo( "DeliveryFormView", true, bAllowEdit ) );
                }

                // iff checkbox only visible for historical ( expired ) contracts.  All new contracts select embedded by default.
                if(( currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSS || currentDocument.DocumentType == CurrentDocument.DocumentTypes.National )
                    && currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Expired )
                {
                    _controlList.Add( new ControlInfo( "IffHeaderLabel", true, false ) );
                    _controlList.Add( new ControlInfo( "IffAbsorbedCheckBox", true, false ) );
                    _controlList.Add( new ControlInfo( "IffEmbeddedCheckBox", true, false ) );
                }
                else //if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.BPA || currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSSBPA )
                {
                    _controlList.Add( new ControlInfo( "IffHeaderLabel", false, false ) );
                    _controlList.Add( new ControlInfo( "IffAbsorbedCheckBox", false, false ) );
                    _controlList.Add( new ControlInfo( "IffEmbeddedCheckBox", false, false ) );
                }

                if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.BPA || currentDocument.DocumentType == CurrentDocument.DocumentTypes.National )
                {
                    _controlList.Add( new ControlInfo( "SolicitationDropDownList", false, false ) );
                    _controlList.Add( new ControlInfo( "SolicitationNumberTextBox", true, true ) );                                           
                }
                else if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSS || currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSSBPA )
                {
                    _controlList.Add( new ControlInfo( "SolicitationDropDownList", true, true ) );
                    _controlList.Add( new ControlInfo( "SolicitationNumberTextBox", false, false ) );                                            
                }

                 // points of contact tab
                 if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSS || currentDocument.DocumentType == CurrentDocument.DocumentTypes.National )
                 {
                     _formViewList.Add( new ControlInfo( "VendorContractAdministratorFormView", true, bAllowEdit ) );
                     _formViewList.Add( new ControlInfo( "VendorAlternateContactFormView", true, bAllowEdit ) );
                     _formViewList.Add( new ControlInfo( "VendorTechnicalContactFormView", true, bAllowEdit ) );
                     _formViewList.Add( new ControlInfo( "VendorEmergencyContactFormView", true, bAllowEdit ) );
                     _formViewList.Add( new ControlInfo( "VendorOrderingContactFormView", true, bAllowEdit ) );
                     _formViewList.Add( new ControlInfo( "VendorSalesContactFormView", true, bAllowEdit ) );
                     _formViewList.Add( new ControlInfo( "VendorBusinessAddressFormView", true, bAllowEdit ) );
                 }
                 else if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.BPA || currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSSBPA )
                 {
                     _formViewList.Add( new ControlInfo( "VendorContractAdministratorFormView", true, bAllowEdit ) );
                     _formViewList.Add( new ControlInfo( "VendorAlternateContactFormView", false, false ) );
                     _formViewList.Add( new ControlInfo( "VendorTechnicalContactFormView", false, false ) );
                     _formViewList.Add( new ControlInfo( "VendorEmergencyContactFormView", false, false ) );
                     _formViewList.Add( new ControlInfo( "VendorOrderingContactFormView", false, false ) );
                     _formViewList.Add( new ControlInfo( "VendorSalesContactFormView", false, false ) );
                     _formViewList.Add( new ControlInfo( "VendorBusinessAddressFormView", false, false ) );
                 }

                 // items tab
                 _formViewList.Add( new ControlInfo( "PricelistVerificationFormView", true, bAllowEdit ) );
                 _formViewList.Add( new ControlInfo( "ItemPriceCountsFormView", true, bAllowEdit ) );
                 _formViewList.Add( new ControlInfo( "PricelistFormView", true, bAllowView ) );
                 _formViewList.Add( new ControlInfo( "PricelistNotesFormView", true, bAllowEdit ) );

                // rebates tab
                if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSS || currentDocument.DocumentType == CurrentDocument.DocumentTypes.National )
                {
                    _formViewList.Add( new ControlInfo( "RebatesHeaderFormView", true, bAllowEdit ) );
                    _controlList.Add( new ControlInfo( "RebateGridPanel", true, bAllowView ) );
                    _formViewList.Add( new ControlInfo( "RebatesFooterDateFormView", true, bAllowEdit ) );
                    _formViewList.Add( new ControlInfo( "RebatesFooterClauseFormView", true, bAllowEdit ) );
                }
                else if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.BPA || currentDocument.DocumentType == CurrentDocument.DocumentTypes.FSSBPA )
                {
                    _formViewList.Add( new ControlInfo( "RebatesHeaderFormView", false, false ) );
                    _controlList.Add( new ControlInfo( "RebateGridPanel", false, false ) );
                    _formViewList.Add( new ControlInfo( "RebatesFooterDateFormView", false, false ) );
                    _formViewList.Add( new ControlInfo( "RebatesFooterClauseFormView", false, false ) );
                }

                // sales tab
                _formViewList.Add( new ControlInfo( "SalesHeaderFormView", true, true ) );  // keeping sales and checks formviews enabled
                _controlList.Add( new ControlInfo( "SalesGridPanel", true, true ) );        // to edit values after end of contract
                                                                                            // also to allow detailed browsing

                if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) == false )
                {
                    _controlList.Add( new ControlInfo( "ViewIFFSalesComparisonButton", true, true ) );
                }
                else
                {
                    _controlList.Add( new ControlInfo( "ViewIFFSalesComparisonButton", true, false ) );
                }

                // edit sales window
                _formViewList.Add( new ControlInfo( "EditSalesHeaderFormView", true, true ) );
                _controlList.Add( new ControlInfo( "EditSalesGridPanel", true, true ) );        

                // payments tab
                if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) == false )
                {
                    _formViewList.Add( new ControlInfo( "PaymentsHeaderFormView", true, true ) );                    
                }
                else
                {
                    _formViewList.Add( new ControlInfo( "PaymentsHeaderFormView", true, false ) );                   
                }
                _controlList.Add( new ControlInfo( "PaymentGridPanel", true, true ) );

                 // payments national tab
                if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) == false )
                {
                    _formViewList.Add( new ControlInfo( "PaymentsNationalHeaderFormView", true, true ) );                    
                }
                else
                {
                    _formViewList.Add( new ControlInfo( "PaymentsNationalHeaderFormView", true, false ) );                   
                }
                _controlList.Add( new ControlInfo( "PaymentNationalGridPanel", true, true ) );

                // checks tab
                if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) == false )
                {
                    _formViewList.Add( new ControlInfo( "ChecksHeaderFormView", true, true ) );                    
                }
                else
                {
                    _formViewList.Add( new ControlInfo( "ChecksHeaderFormView", true, false ) );                   
                }
                _controlList.Add( new ControlInfo( "CheckGridPanel", true, true ) );

                // sba tab
                _formViewList.Add( new ControlInfo( "SBAHeaderFormView", true, bIsActive ) );
                _formViewList.Add( new ControlInfo( "SBAPlanDetailsFormView", true, bIsActive ) );
                _controlList.Add( new ControlInfo( "ProjectionGridPanel", true, bIsActive ) );
                _controlList.Add( new ControlInfo( "AssociatedContractsGridPanel", true, true ) ); // allow clicks to assoc contracts even if expired
                
                // sba popup
                _controlList.Add( new ControlInfo( "EditSBAPlanDetailsPanel", true, bIsActive ) );

                // comment tab
                _formViewList.Add( new ControlInfo( "ContractCommentsFormView", true, bAllowEdit ) );

                //$$$FormViewAddition
            }
            else // offer
            {
                // general tab
                _formViewList.Add( new ControlInfo( "OfferAttributesFormView", true, bIsActive ) );
                _formViewList.Add( new ControlInfo( "OfferActionFormView", true, bIsActive ) );
                _formViewList.Add( new ControlInfo( "OfferAuditFormView", true, bIsActive ) );
                // award tab
                _formViewList.Add( new ControlInfo( "OfferAwardFormView", true, bMayEdit ) );
                // poc tab
                _formViewList.Add( new ControlInfo( "OfferVendorPOCFormView", true, bAllowEdit ) );
                _formViewList.Add( new ControlInfo( "OfferVendorAddressFormView", true, bAllowEdit ) );
                // comments tab
                _formViewList.Add( new ControlInfo( "OfferCommentsFormView", true, bAllowEdit ) );
                // offer headers
                _formViewList.Add( new ControlInfo( "OfferViewMasterVendorFormView", true, true ) );
                _formViewList.Add( new ControlInfo( "OfferViewMasterVendorAdministratorFormView", true, true ) );
                _formViewList.Add( new ControlInfo( "OfferViewMasterContractingOfficerFormView", true, true ) );
                _formViewList.Add( new ControlInfo( "CreateOfferFormView", false, false ) );
            }
        }

        public void UpdatePresentationFromFront( EditedDocumentContent editedDocumentContentFront )
        {
            _editedDocumentContentFront = editedDocumentContentFront;

            int selectedDivisionId = -1;
            int selectedScheduleNumber = -1;
 
            if( editedDocumentContentFront != null )
            {
                selectedDivisionId = editedDocumentContentFront.DivisionId;
                selectedScheduleNumber = editedDocumentContentFront.ScheduleNumber;

                // disable rebate for non-fss or for fss service schedule
                if( CurrentDocument.CanHaveRebates( selectedScheduleNumber, 0 ) == true )
                {
                    UpdateControlVisibility( "RebateRequiredRadioButtonList", true );
                    UpdateControlEnabled( "RebateRequiredRadioButtonList", true );
                } 
                else
                {
                    UpdateControlVisibility( "RebateRequiredRadioButtonList", false );
                    UpdateControlEnabled( "RebateRequiredRadioButtonList", false );
                }

                //  parent contract number list only enabled for BPA
                if( CurrentDocument.IsBPA( selectedScheduleNumber, 0 ) == true )
                {
                    UpdateControlVisibility( "ParentContractNumberLabel", true );
                    UpdateControlVisibility( "ParentContractsDropDownList", true );
                    UpdateControlEnabled( "ParentContractNumberLabel", true );
                    UpdateControlEnabled( "ParentContractsDropDownList", true );
                }
                else
                {
                    UpdateControlVisibility( "ParentContractNumberLabel", false );
                    UpdateControlVisibility( "ParentContractsDropDownList", false );
                    UpdateControlEnabled( "ParentContractNumberLabel", false );
                    UpdateControlEnabled( "ParentContractsDropDownList", false );
                }

            }
        }

        // set up visibility and enabled for document controls for the create screens ( no current document )
        private void InitControlLists( bool bIsContract )
        {
            if( bIsContract == true )
            {
                // create contract
                _formViewList.Add( new ControlInfo( "CreateContractFormView", true, true ) );
                _formViewList.Add( new ControlInfo( "VendorPOCFormView", true, true ) );
                _formViewList.Add( new ControlInfo( "VendorAddressFormView", true, true ) );

                _controlList.Add( new ControlInfo( "RebateRequiredRadioButtonList", true, true ) );
       //         _controlList.Add( new ControlInfo( "RebateRequiredRadioButtonList", true, bActive ) );
                _controlList.Add( new ControlInfo( "ParentContractNumberLabel", false, false ) );
                _controlList.Add( new ControlInfo( "ParentContractsDropDownList", false, false ) );
            }
            else
            {
                // create offer
                // general tab
                _formViewList.Add( new ControlInfo( "OfferAttributesFormView", true, true ) );
                _formViewList.Add( new ControlInfo( "OfferActionFormView", true, true ) );
                _formViewList.Add( new ControlInfo( "OfferAuditFormView", true, true ) );
                // award tab
                _formViewList.Add( new ControlInfo( "OfferAwardFormView", true, true ) );
                // poc tab
                _formViewList.Add( new ControlInfo( "OfferVendorPOCFormView", true, true ) );
                _formViewList.Add( new ControlInfo( "OfferVendorAddressFormView", true, true ) );
                // comments tab
                _formViewList.Add( new ControlInfo( "OfferCommentsFormView", true, true ) );
                // offer headers
                _formViewList.Add( new ControlInfo( "OfferViewMasterVendorFormView", false, false ) );
                _formViewList.Add( new ControlInfo( "OfferViewMasterVendorAdministratorFormView", false, false ) );
                _formViewList.Add( new ControlInfo( "OfferViewMasterContractingOfficerFormView", false, false ) );
                _formViewList.Add( new ControlInfo( "CreateOfferFormView", true, true ) );
            }
        }
    }
}
