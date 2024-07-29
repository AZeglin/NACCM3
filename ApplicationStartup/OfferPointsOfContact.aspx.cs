using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Threading;
using AjaxControlToolkit;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.ReportManager;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using ListView = VA.NAC.NACCMBrowser.BrowserObj.ListView;
using Menu = System.Web.UI.WebControls.Menu;
using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class OfferPointsOfContact : BaseDocumentEditorPage
    {
        public OfferPointsOfContact()
            : base()
        {
        }


        public void Page_PreInit( object sender, EventArgs e )
        {
            SetCreatingNewOffer();   

            if( CreatingNewOffer() == true )
            {
           //     this.MasterPageFile = "~/DocumentCreation.Master";
                base.DocumentEditorType = DocumentEditorTypes.NewOffer;
            }
            else
            {
           //     this.MasterPageFile = "~/OfferView.Master";
                base.DocumentEditorType = DocumentEditorTypes.Offer;
            }
        }

        CurrentDocument _currentDocument = null;
        private bool _bCreatingNewOffer = false;

        protected new void Page_Load( object sender, EventArgs e )
        {
            if( Session[ "NACCMStartedProperly" ] == null || Request.UrlReferrer == null )
            {
                Response.Redirect( "~/Start.aspx" );
            }
            
            if( Page.IsPostBack == false && ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
            {
                ClearSessionVariables();
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );

            LoadAndBindNonFormViewControls();

            AssignDataSourceToFormViews();

            if( Page.IsPostBack == false )
            {
                if( CurrentDocumentIsChanging == true )
                {
                    if( CreatingNewOffer() == true )
                    {
                        OfferDataRelay.Clear();  // prepare for offer creation
                    }
                    else
                    {
                        OfferDataRelay.Load();
                    }
                }
                BindFormViews();
            }
        }

        private void SetCreatingNewOffer()
        {
            _bCreatingNewOffer = false;

            // current document is cleared to null if create offer is selected from the main menu
            CurrentDocument currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];

            if( currentDocument == null )
                _bCreatingNewOffer = true;
        }

        private bool CreatingNewOffer()
        {
            return ( _bCreatingNewOffer );
        }

        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            OfferVendorPOCFormView.DataSource = OfferDataRelay.EditedOfferDataSourceFront;
            OfferVendorPOCFormView.DataKeyNames = new string[] { "OfferId" };

            OfferVendorAddressFormView.DataSource = OfferDataRelay.EditedOfferDataSourceFront;
            OfferVendorAddressFormView.DataKeyNames = new string[] { "OfferId" };
        }


        protected void ClearSessionVariables()
        {

        }

        protected void BindFormViews()
        {
            try
            {
                BindHeader();

                OfferVendorPOCFormView.DataBind();
                OfferVendorAddressFormView.DataBind();
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void LoadAndBindNonFormViewControls()
        {
        }

        public override void BindAfterShortSave()
        {
            BindFormViews();
        }

        public override void RebindHeader()
        {
            BindHeader();
        }

        public override string GetValidationGroupName()
        {
            return ( "OfferPointsOfContact" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            OfferDataRelay offerDataRelay = ( OfferDataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            // for now, only validate during offer creation, not just an edit  // $$$ taking this out temporarily for testing purposes
     //       if( CreatingNewOffer() == true ) 
      //      {
                // Vendor POC             
                if( offerDataRelay.EditedOfferContentFront.VendorPrimaryContactName.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor primary contact name is required.", bIsShortSave );
                    bSuccess = false;
                }

                if( offerDataRelay.EditedOfferContentFront.VendorPrimaryContactPhone.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor primary contact phone is required.", bIsShortSave );
                    bSuccess = false;
                }

                if( offerDataRelay.EditedOfferContentFront.VendorPrimaryContactEmail.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor primary contact email is required.", bIsShortSave );
                    bSuccess = false;
                }
                else if( CMGlobals.IsValidEmailAddress( offerDataRelay.EditedOfferContentFront.VendorPrimaryContactEmail.Trim() ) != true )
                {
                    AppendValidationError( "Vendor email is not a correctly formatted email address.", bIsShortSave );
                    bSuccess = false;
                }
               
                // Vendor Address
                if( offerDataRelay.EditedOfferContentFront.VendorAddress1.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor address is required.", bIsShortSave );
                    bSuccess = false;
                }

                int vendorCountryId = offerDataRelay.EditedOfferContentFront.VendorCountryId;
                if( vendorCountryId == -1 )
                {
                    AppendValidationError( "Vendor country is required.", bIsShortSave );
                    bSuccess = false;
                }

                if( offerDataRelay.EditedOfferContentFront.VendorCity.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor city is required.", bIsShortSave );
                    bSuccess = false;
                }

                if( vendorCountryId == CMGlobals.COUNTRYIDUSA || vendorCountryId == CMGlobals.COUNTRYIDCANADA )
                {
                    if( offerDataRelay.EditedOfferContentFront.VendorState.Trim().Length == 0 )
                    {
                        AppendValidationError( "Vendor state is required.", bIsShortSave );
                        bSuccess = false;
                    }

                    if( offerDataRelay.EditedOfferContentFront.VendorZip.Trim().Length == 0 )
                    {
                        AppendValidationError( "Vendor zip is required.", bIsShortSave );
                        bSuccess = false;
                    }
                }

            // zip 5 or 10
            if( vendorCountryId == CMGlobals.COUNTRYIDUSA )
            {
                if( offerDataRelay.EditedOfferContentFront.VendorZip.Trim().Length > 0 )
                {
                    if( offerDataRelay.EditedOfferContentFront.VendorZip.Trim().Length == 5 )
                    {
                        int tempZip = 0;
                        if( int.TryParse( offerDataRelay.EditedOfferContentFront.VendorZip.Trim(), out tempZip ) == false )
                        {
                            AppendValidationError( "Vendor zip is not formatted correctly.", bIsShortSave );
                            bSuccess = false;
                        }
                    }
                    else if( offerDataRelay.EditedOfferContentFront.VendorZip.Trim().Length == 10 )
                    {
                        string leftZip = "";
                        string dash = "";
                        string rightZip = "";

                        leftZip = offerDataRelay.EditedOfferContentFront.VendorZip.Trim().Substring( 0, 5 );
                        dash = offerDataRelay.EditedOfferContentFront.VendorZip.Trim().Substring( 5, 1 );
                        rightZip = offerDataRelay.EditedOfferContentFront.VendorZip.Trim().Substring( 6, 4 );

                        int tempZip = 0;
                        if( dash.CompareTo( "-" ) != 0 || int.TryParse( leftZip, out tempZip ) == false || int.TryParse( rightZip, out tempZip ) == false )
                        {
                            AppendValidationError( "Vendor zip is not formatted correctly.", bIsShortSave );
                            bSuccess = false;
                        }
                    }
                    else
                    {
                        AppendValidationError( "Vendor zip is not formatted correctly.", bIsShortSave );
                        bSuccess = false;
                    }
                }
            }
            //    }
            //  else 

            if( offerDataRelay.EditedOfferContentFront.VendorPrimaryContactEmail.Trim().Length > 0 )
            {
                if( CMGlobals.IsValidEmailAddress( offerDataRelay.EditedOfferContentFront.VendorPrimaryContactEmail.Trim() ) != true )
                {
                    AppendValidationError( "Vendor email is not a correctly formatted email address.", bIsShortSave );
                    bSuccess = false;
                }
            }

            return ( bSuccess );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            OfferDataRelay offerDataRelay = ( OfferDataRelay )dataRelayInterface;

            bool bSuccess = true;

            try
            {
                // Offer POC
                TextBox pointOfContactNameTextBox = ( TextBox )OfferVendorPOCFormView.FindControl( "PointOfContactNameTextBox" );
                if( pointOfContactNameTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorPrimaryContactName = pointOfContactNameTextBox.Text;
                }

                TextBox pointOfContactPhoneTextBox = ( TextBox )OfferVendorPOCFormView.FindControl( "PointOfContactPhoneTextBox" );
                if( pointOfContactPhoneTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorPrimaryContactPhone = pointOfContactPhoneTextBox.Text;
                }

                TextBox pointOfContactPhoneExtensionTextBox = ( TextBox )OfferVendorPOCFormView.FindControl( "PointOfContactPhoneExtensionTextBox" );
                if( pointOfContactPhoneExtensionTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorPrimaryContactExtension = pointOfContactPhoneExtensionTextBox.Text;
                }

                TextBox pointOfContactFaxTextBox = ( TextBox )OfferVendorPOCFormView.FindControl( "PointOfContactFaxTextBox" );
                if( pointOfContactFaxTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorPrimaryContactFax = pointOfContactFaxTextBox.Text;
                }

                TextBox pointOfContactEmailTextBox = ( TextBox )OfferVendorPOCFormView.FindControl( "PointOfContactEmailTextBox" );
                if( pointOfContactEmailTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorPrimaryContactEmail = pointOfContactEmailTextBox.Text;
                }

                // Vendor Address
                TextBox address1TextBox = ( TextBox )OfferVendorAddressFormView.FindControl( "Address1TextBox" );
                if( address1TextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorAddress1 = address1TextBox.Text;
                }

                TextBox address2TextBox = ( TextBox )OfferVendorAddressFormView.FindControl( "Address2TextBox" );
                if( address2TextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorAddress2 = address2TextBox.Text;
                }

                TextBox cityTextBox = ( TextBox )OfferVendorAddressFormView.FindControl( "CityTextBox" );
                if( cityTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorCity = cityTextBox.Text;
                }

                DropDownList stateDropDownList = ( DropDownList )OfferVendorAddressFormView.FindControl( "StateDropDownList" );
                if( stateDropDownList != null )
                {
                    if( stateDropDownList.SelectedItem != null )
                    {
                        offerDataRelay.EditedOfferContentFront.VendorState = stateDropDownList.SelectedItem.Value.ToString();
                    }
                }

                TextBox zipTextBox = ( TextBox )OfferVendorAddressFormView.FindControl( "ZipTextBox" );
                if( zipTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorZip = zipTextBox.Text;
                }

                //TextBox countryTextBox = ( TextBox )OfferVendorAddressFormView.FindControl( "CountryTextBox" );
                //if( countryTextBox != null )
                //{
                //    offerDataRelay.EditedOfferContentFront.VendorCountry = countryTextBox.Text;
                //}

                DropDownList countryDropDownList = ( DropDownList )OfferVendorAddressFormView.FindControl( "CountryDropDownList" );
                if( countryDropDownList != null )
                {
                    if( countryDropDownList.SelectedItem != null )
                    {
                        offerDataRelay.EditedOfferContentFront.VendorCountryId = int.Parse( countryDropDownList.SelectedItem.Value.ToString() );
                    }
                }

                TextBox companyUrlTextBox = ( TextBox )OfferVendorAddressFormView.FindControl( "CompanyUrlTextBox" );
                if( companyUrlTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorWebAddress = companyUrlTextBox.Text;
                }

            }
            catch( Exception ex )
            {
                bSuccess = false;
                ErrorMessage = string.Format( "The following exception was encountered validating the offer information: {0}", ex.Message );
            }

            return ( bSuccess );
        }

        protected void OfferVendorPOCFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            OfferVendorPOCFormView.Visible = documentControlPresentation.IsFormViewVisible( "OfferVendorPOCFormView" );
            OfferVendorPOCFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "OfferVendorPOCFormView" );
        }

        protected void OfferVendorAddressFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            OfferVendorAddressFormView.Visible = documentControlPresentation.IsFormViewVisible( "OfferVendorAddressFormView" );
            OfferVendorAddressFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "OfferVendorAddressFormView" );
        }

        protected void OfferVendorPOCFormView_OnDataBound( object sender, EventArgs e )
        {
        }
        protected void OfferVendorAddressFormView_OnDataBound( object sender, EventArgs e )
        {
            FormView offerVendorAddressFormView = ( FormView )sender;
            EditedOfferContent formViewContent = ( EditedOfferContent )offerVendorAddressFormView.DataItem;

            base.LoadCountries();
            DataSet dsCountries = ( DataSet )Cache[ PointsOfContactVendorCountriesCacheName ];
            DropDownList countryDropDownList;
            int selectedVendorCountryId = -1;

            if( formViewContent != null )
            {
                selectedVendorCountryId = formViewContent.VendorCountryId;
            }

            if( offerVendorAddressFormView != null )
            {
                countryDropDownList = ( DropDownList )offerVendorAddressFormView.FindControl( "CountryDropDownList" );

                if( countryDropDownList != null )
                {
                    countryDropDownList.ClearSelection();
                    countryDropDownList.Items.Clear();

                    foreach( DataRow row in dsCountries.Tables[ ContractDB.VendorCountriesTableName ].Rows )
                    {
                        string countryName = row[ "CountryName" ].ToString();
                        string countryIdString = row[ "CountryId" ].ToString();
                        countryDropDownList.Items.Add( new ListItem( countryName, countryIdString ) );
                    }

                    if( countryDropDownList.Items.FindByValue( selectedVendorCountryId.ToString() ) != null )
                    {
                        //  -1 matches -- select -- choice
                        countryDropDownList.Items.FindByValue( selectedVendorCountryId.ToString() ).Selected = true;
                    }
                }
            }

            base.LoadStates( selectedVendorCountryId );
            DataSet dsStateCodes = base.PointsOfContactStateCodeDataSet;
            DropDownList stateDropDownList;
            string vendorState = "";
          
            if( formViewContent != null )
            {
                vendorState = formViewContent.VendorState;
            }

            if( offerVendorAddressFormView != null )
            {
                stateDropDownList = ( DropDownList )offerVendorAddressFormView.FindControl( "StateDropDownList" );

                if( stateDropDownList != null )
                {
                    stateDropDownList.ClearSelection();
                    stateDropDownList.Items.Clear();

                    foreach( DataRow row in dsStateCodes.Tables[ "StateCodesTable" ].Rows )
                    {
                        string stateAbbreviation = row[ "StateAbbreviation" ].ToString();
                        string stateAbbreviationValue = ( stateAbbreviation.CompareTo( "--" ) != 0 ) ? stateAbbreviation : "";
                        stateDropDownList.Items.Add( new ListItem( stateAbbreviation, stateAbbreviationValue ) );
                    }

                    if( stateDropDownList.Items.FindByValue( vendorState ) != null )
                    {
                        stateDropDownList.Items.FindByValue( vendorState ).Selected = true;
                    }
                }
            }

         
        }



        protected void CountryDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList countryDropDownList = ( DropDownList )sender;
            ListItem selectedItem = countryDropDownList.SelectedItem;
            int selectedVendorCountryId = int.Parse( selectedItem.Value );

            // save the selection
            EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];
            if( editedOfferContentFront != null )
            {
                editedOfferContentFront.VendorCountryId = selectedVendorCountryId;
            }

            // reset other associated drop down list
            DropDownList stateDropDownList = ( DropDownList )OfferVendorAddressFormView.FindControl( "StateDropDownList" );
            if( stateDropDownList != null )
            {
                stateDropDownList.DataBind();
            }

            // allow the postback to occur 
            OfferPointsOfContactUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }


        protected void OfferVendorPOCFormView_OnChange( object sender, EventArgs e )
        {
     //       SetDirtyFlag( "OfferVendorPOCFormView" );
        }

        protected void OfferVendorAddressFormView_OnChange( object sender, EventArgs e )
        {
     //       SetDirtyFlag( "OfferVendorAddressFormView" );
        }

        protected void StateDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList stateDropDownList = ( DropDownList )sender;

            int selectedVendorCountryId;
            string vendorState = "";

            if( stateDropDownList != null )
            {
                EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];

                if( editedOfferContentFront != null )
                {
                    selectedVendorCountryId = editedOfferContentFront.VendorCountryId;
                    vendorState = editedOfferContentFront.VendorState;

                    base.LoadStates( selectedVendorCountryId );
                    DataSet dsStateCodes = base.PointsOfContactStateCodeDataSet;

                    stateDropDownList.ClearSelection();
                    stateDropDownList.Items.Clear();

                    foreach( DataRow row in dsStateCodes.Tables[ "StateCodesTable" ].Rows )
                    {
                        string stateAbbreviation = row[ "StateAbbreviation" ].ToString();
                        string stateAbbreviationValue = ( stateAbbreviation.CompareTo( "--" ) != 0 ) ? stateAbbreviation : "";
                        stateDropDownList.Items.Add( new ListItem( stateAbbreviation, stateAbbreviationValue ) );
                    }

                    if( stateDropDownList.Items.FindByText( vendorState ) != null )
                    {
                        stateDropDownList.Items.FindByText( vendorState ).Selected = true;
                    }                    
                }
            }
        }
    }
}