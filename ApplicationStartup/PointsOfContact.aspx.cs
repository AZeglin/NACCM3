using System;
using System.Collections;
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
using AjaxControlToolkit;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.ReportManager;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using ListView = VA.NAC.NACCMBrowser.BrowserObj.ListView;
using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class PointsOfContact : BaseDocumentEditorPage
    {
        public PointsOfContact()
            : base( DocumentEditorTypes.Contract )
        {
        }

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
                    DataRelay.Load();
                }
                BindFormViews();
            }
        }

      


        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            VendorContractAdministratorFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorContractAdministratorFormView.DataKeyNames = new string[] { "ContractId" };

            VendorAlternateContactFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorAlternateContactFormView.DataKeyNames = new string[] { "ContractId" };

            VendorTechnicalContactFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorTechnicalContactFormView.DataKeyNames = new string[] { "ContractId" };

            VendorEmergencyContactFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorEmergencyContactFormView.DataKeyNames = new string[] { "ContractId" };

            VendorOrderingContactFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorOrderingContactFormView.DataKeyNames = new string[] { "ContractId" };

            VendorSalesContactFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorSalesContactFormView.DataKeyNames = new string[] { "ContractId" };

            VendorBusinessAddressFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorBusinessAddressFormView.DataKeyNames = new string[] { "ContractId" };

            
        }

        protected void ClearSessionVariables()
        {

        }

        protected void BindFormViews()
        {
            BindHeader();

            VendorContractAdministratorFormView.DataBind();
            VendorAlternateContactFormView.DataBind();
            VendorTechnicalContactFormView.DataBind();
            VendorEmergencyContactFormView.DataBind();
            VendorOrderingContactFormView.DataBind();
            VendorSalesContactFormView.DataBind();
            VendorBusinessAddressFormView.DataBind();

            // note form view controls are not yet created here

            //$$$FormViewAddition
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
            return ( "ContractPointsOfContact" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;
            
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorOrderingContactFormView" ) == true )
            {
                if( dataRelay.EditedDocumentContentFront.OrderingAddress1.Trim().Length == 0 )
                {
                    bSuccess = false;
                    AppendValidationError( "Ordering contact address is required.", bIsShortSave );
                }

                if( dataRelay.EditedDocumentContentFront.OrderingCity.Trim().Length == 0 )
                {
                    bSuccess = false;
                    AppendValidationError( "Ordering contact address city is required.", bIsShortSave );
                }

                int orderingCountryId = dataRelay.EditedDocumentContentFront.OrderingCountryId;
                if( orderingCountryId == -1 )
                {
                    bSuccess = false;
                    AppendValidationError( "Ordering contact address country is required.", bIsShortSave );
                }

                if( orderingCountryId == CMGlobals.COUNTRYIDUSA || orderingCountryId == CMGlobals.COUNTRYIDCANADA )
                {
                    if( dataRelay.EditedDocumentContentFront.OrderingState.Trim().Length == 0 )
                    {
                        bSuccess = false;
                        AppendValidationError( "Ordering contact address state is required.", bIsShortSave );
                    }

                    if( dataRelay.EditedDocumentContentFront.OrderingZip.Trim().Length == 0 )
                    {
                        bSuccess = false;
                        AppendValidationError( "Ordering contact address zip is required.", bIsShortSave );
                    }
                }

                // zip 5 or 10
                if( orderingCountryId == CMGlobals.COUNTRYIDUSA )
                {
                    if( dataRelay.EditedDocumentContentFront.OrderingZip.Trim().Length > 0 )
                    {
                        if( dataRelay.EditedDocumentContentFront.OrderingZip.Trim().Length == 5 )
                        {
                            int tempZip = 0;
                            if( int.TryParse( dataRelay.EditedDocumentContentFront.OrderingZip.Trim(), out tempZip ) == false )
                            {
                                AppendValidationError( "Ordering zip is not formatted correctly.", bIsShortSave );
                                bSuccess = false;
                            }
                        }
                        else if( dataRelay.EditedDocumentContentFront.OrderingZip.Trim().Length == 10 )
                        {
                            string leftZip = "";
                            string dash = "";
                            string rightZip = "";

                            leftZip = dataRelay.EditedDocumentContentFront.OrderingZip.Trim().Substring( 0, 5 );
                            dash = dataRelay.EditedDocumentContentFront.OrderingZip.Trim().Substring( 5, 1 );
                            rightZip = dataRelay.EditedDocumentContentFront.OrderingZip.Trim().Substring( 6, 4 );

                            int tempZip = 0;
                            if( dash.CompareTo( "-" ) != 0 || int.TryParse( leftZip, out tempZip ) == false || int.TryParse( rightZip, out tempZip ) == false )
                            {
                                AppendValidationError( "Ordering zip is not formatted correctly.", bIsShortSave );
                                bSuccess = false;
                            }
                        }
                        else
                        {
                            AppendValidationError( "Ordering zip is not formatted correctly.", bIsShortSave );
                            bSuccess = false;
                        }
                    }
                }

                if( dataRelay.EditedDocumentContentFront.OrderingTelephone.Trim().Length == 0 )
                {
                    bSuccess = false;
                    AppendValidationError( "Ordering contact telephone is required.", bIsShortSave );
                }

                if( dataRelay.EditedDocumentContentFront.OrderingEmail.Trim().Length == 0 )
                {
                    bSuccess = false;
                    AppendValidationError( "Ordering contact email is required.", bIsShortSave );
                }
                else
                {
                    if( CMGlobals.IsValidEmailAddress( dataRelay.EditedDocumentContentFront.OrderingEmail.Trim() ) != true )
                    {
                        bSuccess = false;
                        AppendValidationError( "Ordering contact email is not a correctly formatted email address.", bIsShortSave );
                    }
                }               
            }

            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorContractAdministratorFormView" ) == true )
            {
                if( dataRelay.EditedDocumentContentFront.VendorPrimaryContactName.Trim().Length == 0 )
                {
                    bSuccess = false;
                    AppendValidationError( "Contract administrator name is required.", bIsShortSave );
                }
                else if( CMGlobals.ContainsNonBasicLetter( dataRelay.EditedDocumentContentFront.VendorPrimaryContactName.Trim() ))
                {
                    bSuccess = false;
                    AppendValidationError( "Contract administrator name contains non-basic characters.", bIsShortSave );
                }

                if( dataRelay.EditedDocumentContentFront.VendorPrimaryContactPhone.Trim().Length == 0 )
                {
                    bSuccess = false;
                    AppendValidationError( "Contract administrator phone is required.", bIsShortSave );
                }

                if( dataRelay.EditedDocumentContentFront.VendorPrimaryContactEmail.Trim().Length > 0 )
                {

                    if( CMGlobals.IsValidEmailAddress( dataRelay.EditedDocumentContentFront.VendorPrimaryContactEmail.Trim() ) != true )
                    {
                        bSuccess = false;
                        AppendValidationError( "Contract administrator email is not a correctly formatted email address.", bIsShortSave );
                    }
                }
            }

            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorSalesContactFormView" ) == true )
            {
                if( dataRelay.EditedDocumentContentFront.VendorSalesContactName.Trim().Length == 0 )
                {
                    bSuccess = false;
                    AppendValidationError( "Sales contact name is required.", bIsShortSave );
                }
                else if( CMGlobals.ContainsNonBasicLetter( dataRelay.EditedDocumentContentFront.VendorSalesContactName.Trim() ))
                {
                    bSuccess = false;
                    AppendValidationError( "Sales contact name contains non-basic characters.", bIsShortSave );
                }

                if( dataRelay.EditedDocumentContentFront.VendorSalesContactPhone.Trim().Length == 0 )
                {
                    bSuccess = false;
                    AppendValidationError( "Sales contact telephone is required.", bIsShortSave );
                }

                if( dataRelay.EditedDocumentContentFront.VendorSalesContactEmail.Trim().Length == 0 )
                {
                    bSuccess = false;
                    AppendValidationError( "Sales contact email is required.", bIsShortSave );
                }
                else
                {
                    if( CMGlobals.IsValidEmailAddress( dataRelay.EditedDocumentContentFront.VendorSalesContactEmail.Trim() ) != true )
                    {
                        bSuccess = false;
                        AppendValidationError( "Sales contact email is not a correctly formatted email address.", bIsShortSave );
                    }
                }
            }

            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorBusinessAddressFormView" ) == true )
            {
                if( dataRelay.EditedDocumentContentFront.VendorName.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor name is required.", bIsShortSave );
                    bSuccess = false;
                }
                else if( CMGlobals.ContainsNonBasicLetter( dataRelay.EditedDocumentContentFront.VendorName.Trim() ))
                {
                    bSuccess = false;
                    AppendValidationError( "Vendor name contains non-basic characters.", bIsShortSave );
                }

                if( dataRelay.EditedDocumentContentFront.VendorAddress1.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor address is required.", bIsShortSave );
                    bSuccess = false;
                }

                if( dataRelay.EditedDocumentContentFront.VendorCity.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor city is required.", bIsShortSave );
                    bSuccess = false;
                }

                int vendorCountryId = dataRelay.EditedDocumentContentFront.VendorCountryId;
                if( vendorCountryId == -1 )
                {
                    AppendValidationError( "Vendor country is required.", bIsShortSave );
                    bSuccess = false;
                }

                if( vendorCountryId == CMGlobals.COUNTRYIDUSA || vendorCountryId == CMGlobals.COUNTRYIDCANADA )
                {
                    if( dataRelay.EditedDocumentContentFront.VendorState.Trim().Length == 0 )
                    {
                        AppendValidationError( "Vendor state is required.", bIsShortSave );
                        bSuccess = false;
                    }

                    if( dataRelay.EditedDocumentContentFront.VendorZip.Trim().Length == 0 )
                    {
                        AppendValidationError( "Vendor zip is required.", bIsShortSave );
                        bSuccess = false;
                    }
                }

                // zip 5 or 10
                if( vendorCountryId == CMGlobals.COUNTRYIDUSA )
                {
                    if( dataRelay.EditedDocumentContentFront.VendorZip.Trim().Length > 0 )
                    {
                        if( dataRelay.EditedDocumentContentFront.VendorZip.Trim().Length == 5 )
                        {
                            int tempZip = 0;
                            if( int.TryParse( dataRelay.EditedDocumentContentFront.VendorZip.Trim(), out tempZip ) == false )
                            {
                                AppendValidationError( "Vendor zip is not formatted correctly.", bIsShortSave );
                                bSuccess = false;
                            }
                        }
                        else if( dataRelay.EditedDocumentContentFront.VendorZip.Trim().Length == 10 )
                        {
                            string leftZip = "";
                            string dash = "";
                            string rightZip = "";

                            leftZip = dataRelay.EditedDocumentContentFront.VendorZip.Trim().Substring( 0, 5 );
                            dash = dataRelay.EditedDocumentContentFront.VendorZip.Trim().Substring( 5, 1 );
                            rightZip = dataRelay.EditedDocumentContentFront.VendorZip.Trim().Substring( 6, 4 );

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
            }

            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorAlternateContactFormView" ) == true )
            {
                if( dataRelay.EditedDocumentContentFront.VendorAlternateContactName.Trim().Length == 0 && dataRelay.EditedDocumentContentFront.DivisionId == 1 )  // FSS Required Field
                {
                    AppendValidationError( "Alternate contact name is required.", bIsShortSave );
                    bSuccess = false;
                }
                else if( CMGlobals.ContainsNonBasicLetter( dataRelay.EditedDocumentContentFront.VendorAlternateContactName.Trim() ))
                {
                    bSuccess = false;
                    AppendValidationError( "Alternate contact name contains non-basic characters.", bIsShortSave );
                }

                if( dataRelay.EditedDocumentContentFront.VendorAlternateContactPhone.Trim().Length == 0 && dataRelay.EditedDocumentContentFront.DivisionId == 1 )  // FSS Required Field
                {
                    bSuccess = false;
                    AppendValidationError( "Alternate contact telephone is required.", bIsShortSave );
                }

                if( dataRelay.EditedDocumentContentFront.VendorAlternateContactEmail.Trim().Length == 0 && dataRelay.EditedDocumentContentFront.DivisionId == 1 )  // FSS Required Field
                {
                    bSuccess = false;
                    AppendValidationError( "Alternate contact email is required.", bIsShortSave );
                }
                else
                {             
                    if( dataRelay.EditedDocumentContentFront.VendorAlternateContactEmail.Trim().Length > 0 && CMGlobals.IsValidEmailAddress( dataRelay.EditedDocumentContentFront.VendorAlternateContactEmail.Trim() ) != true )
                    {
                        bSuccess = false;
                        AppendValidationError( "Alternate contact email is not a correctly formatted email address.", bIsShortSave );
                    }
                }
            }

            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorTechnicalContactFormView" ) == true )
            {
                if( dataRelay.EditedDocumentContentFront.VendorTechnicalContactEmail.Trim().Length > 0 )
                {
                    if( CMGlobals.IsValidEmailAddress( dataRelay.EditedDocumentContentFront.VendorTechnicalContactEmail.Trim() ) != true )
                    {
                        bSuccess = false;
                        AppendValidationError( "Technical contact email is not a correctly formatted email address.", bIsShortSave );
                    }
                }
            }

            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorEmergencyContactFormView" ) == true )
            {
                if( dataRelay.EditedDocumentContentFront.VendorEmergencyContactEmail.Trim().Length > 0 )
                {
                    if( CMGlobals.IsValidEmailAddress( dataRelay.EditedDocumentContentFront.VendorEmergencyContactEmail.Trim() ) != true )
                    {
                        bSuccess = false;
                        AppendValidationError( "Emergency contact email is not a correctly formatted email address.", bIsShortSave );
                    }
                }
            }
            return ( bSuccess );
        }

        protected void OrderingCountryDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList orderingCountryDropDownList = ( DropDownList )sender;
            ListItem selectedItem = orderingCountryDropDownList.SelectedItem;
            int selectedOrderingCountryId = int.Parse( selectedItem.Value );

            // save the selection
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            if( editedDocumentContentFront != null )
            {
                editedDocumentContentFront.OrderingCountryId = selectedOrderingCountryId;
            }

            // reset associated state drop down list         
            DropDownList orderingStateDropDownList = ( DropDownList )VendorOrderingContactFormView.FindControl( "OrderingStateDropDownList" );
            if( orderingStateDropDownList != null )
            {
                orderingStateDropDownList.DataBind();
            }
                
            // allow the postback to occur 
            PointsOfContactUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void BusinessCountryDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList businessCountryDropDownList = ( DropDownList )sender;
            ListItem selectedItem = businessCountryDropDownList.SelectedItem;
            int selectedBusinessCountryId = int.Parse( selectedItem.Value );

            // save the selection
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            if( editedDocumentContentFront != null )
            {
                editedDocumentContentFront.VendorCountryId = selectedBusinessCountryId;
            }

            // reset associated state drop down list  -- note just doing databind here doesn't work - it clears out the other formview fields       
            DropDownList businessStateDropDownList = ( DropDownList )VendorBusinessAddressFormView.FindControl( "BusinessStateDropDownList" );
            if( businessStateDropDownList != null )
            {
                businessStateDropDownList.DataBind();
            }

            // allow the postback to occur 
            PointsOfContactUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bSuccess = true;

            // VendorContractAdministratorFormView
            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorContractAdministratorFormView" ) == true )
            {
               TextBox administratorNameTextBox = ( TextBox )VendorContractAdministratorFormView.FindControl( "AdministratorNameTextBox" );
               if( administratorNameTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactName = administratorNameTextBox.Text;
               }

               TextBox administratorPhoneTextBox = ( TextBox )VendorContractAdministratorFormView.FindControl( "AdministratorPhoneTextBox" );
               if( administratorPhoneTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactPhone = administratorPhoneTextBox.Text;
               }

               TextBox administratorPhoneExtTextBox = ( TextBox )VendorContractAdministratorFormView.FindControl( "AdministratorPhoneExtTextBox" );
               if( administratorPhoneExtTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactExtension = administratorPhoneExtTextBox.Text;
               }

               TextBox administratorFaxTextBox = ( TextBox )VendorContractAdministratorFormView.FindControl( "AdministratorFaxTextBox" );
               if( administratorFaxTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactFax = administratorFaxTextBox.Text;
               }

               TextBox administratorEmailTextBox = ( TextBox )VendorContractAdministratorFormView.FindControl( "AdministratorEmailTextBox" );
               if( administratorEmailTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactEmail = administratorEmailTextBox.Text;
               }
           }
           // VendorAlternateContactFormView
            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorAlternateContactFormView" ) == true )
            {

                TextBox alternateContactNameTextBox = ( TextBox )VendorAlternateContactFormView.FindControl( "AlternateContactNameTextBox" );
                if( alternateContactNameTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorAlternateContactName = alternateContactNameTextBox.Text;
                }

                TextBox alternateContactPhoneTextBox = ( TextBox )VendorAlternateContactFormView.FindControl( "AlternateContactPhoneTextBox" );
                if( alternateContactPhoneTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorAlternateContactPhone = alternateContactPhoneTextBox.Text;
                }

                TextBox alternateContactPhoneExtTextBox = ( TextBox )VendorAlternateContactFormView.FindControl( "AlternateContactPhoneExtTextBox" );
                if( alternateContactPhoneExtTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorAlternateContactExtension = alternateContactPhoneExtTextBox.Text;
                }

                TextBox alternateContactFaxTextBox = ( TextBox )VendorAlternateContactFormView.FindControl( "AlternateContactFaxTextBox" );
                if( alternateContactFaxTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorAlternateContactFax = alternateContactFaxTextBox.Text;
                }

                TextBox alternateContactEmailTextBox = ( TextBox )VendorAlternateContactFormView.FindControl( "AlternateContactEmailTextBox" );
                if( alternateContactEmailTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorAlternateContactEmail = alternateContactEmailTextBox.Text;
                }
            }
            // VendorTechnicalContactFormView
            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorTechnicalContactFormView" ) == true )
            {
                TextBox technicalContactNameTextBox = ( TextBox )VendorTechnicalContactFormView.FindControl( "TechnicalContactNameTextBox" );
                if( technicalContactNameTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorTechnicalContactName = technicalContactNameTextBox.Text;
                }

                TextBox technicalContactPhoneTextBox = ( TextBox )VendorTechnicalContactFormView.FindControl( "TechnicalContactPhoneTextBox" );
                if( technicalContactPhoneTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorTechnicalContactPhone = technicalContactPhoneTextBox.Text;
                }

                TextBox technicalContactPhoneExtTextBox = ( TextBox )VendorTechnicalContactFormView.FindControl( "TechnicalContactPhoneExtTextBox" );
                if( technicalContactPhoneExtTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorTechnicalContactExtension = technicalContactPhoneExtTextBox.Text;
                }

                TextBox technicalContactFaxTextBox = ( TextBox )VendorTechnicalContactFormView.FindControl( "TechnicalContactFaxTextBox" );
                if( technicalContactFaxTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorTechnicalContactFax = technicalContactFaxTextBox.Text;
                }

                TextBox technicalContactEmailTextBox = ( TextBox )VendorTechnicalContactFormView.FindControl( "TechnicalContactEmailTextBox" );
                if( technicalContactEmailTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorTechnicalContactEmail = technicalContactEmailTextBox.Text;
                }
            }
           // VendorEmergencyContactFormView
            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorEmergencyContactFormView" ) == true )
            {
                TextBox emergencyContactNameTextBox = ( TextBox )VendorEmergencyContactFormView.FindControl( "EmergencyContactNameTextBox" );
                if( emergencyContactNameTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorEmergencyContactName = emergencyContactNameTextBox.Text;
                }

                TextBox emergencyContactPhoneTextBox = ( TextBox )VendorEmergencyContactFormView.FindControl( "EmergencyContactPhoneTextBox" );
                if( emergencyContactPhoneTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorEmergencyContactPhone = emergencyContactPhoneTextBox.Text;
                }

                TextBox emergencyContactPhoneExtTextBox = ( TextBox )VendorEmergencyContactFormView.FindControl( "EmergencyContactPhoneExtTextBox" );
                if( emergencyContactPhoneExtTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorEmergencyContactExtension = emergencyContactPhoneExtTextBox.Text;
                }

                TextBox emergencyContactFaxTextBox = ( TextBox )VendorEmergencyContactFormView.FindControl( "EmergencyContactFaxTextBox" );
                if( emergencyContactFaxTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorEmergencyContactFax = emergencyContactFaxTextBox.Text;
                }

                TextBox emergencyContactEmailTextBox = ( TextBox )VendorEmergencyContactFormView.FindControl( "EmergencyContactEmailTextBox" );
                if( emergencyContactEmailTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorEmergencyContactEmail = emergencyContactEmailTextBox.Text;
                }
            }
            
            // VendorOrderingContactFormView
            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorOrderingContactFormView" ) == true )
            {
                TextBox orderingAddress1TextBox = ( TextBox )VendorOrderingContactFormView.FindControl( "OrderingAddress1TextBox" );
                if( orderingAddress1TextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.OrderingAddress1 = orderingAddress1TextBox.Text;
                }

                TextBox orderingAddress2TextBox = ( TextBox )VendorOrderingContactFormView.FindControl( "OrderingAddress2TextBox" );
                if( orderingAddress2TextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.OrderingAddress2 = orderingAddress2TextBox.Text;
                }

                TextBox orderingCityTextBox = ( TextBox )VendorOrderingContactFormView.FindControl( "OrderingCityTextBox" );
                if( orderingCityTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.OrderingCity = orderingCityTextBox.Text;
                }

                DropDownList orderingCountryDropDownList = ( DropDownList )VendorOrderingContactFormView.FindControl( "OrderingCountryDropDownList" );
                if( orderingCountryDropDownList != null )
                {
                    if( orderingCountryDropDownList.SelectedItem != null )
                    {
                        dataRelay.EditedDocumentContentFront.OrderingCountryId = int.Parse( orderingCountryDropDownList.SelectedItem.Value.ToString() );
                    }
                }

                DropDownList orderingStateDropDownList = ( DropDownList )VendorOrderingContactFormView.FindControl( "OrderingStateDropDownList" );
                if( orderingStateDropDownList != null )
                {
                    if( orderingStateDropDownList.SelectedItem != null )
                    {
                        dataRelay.EditedDocumentContentFront.OrderingState = orderingStateDropDownList.SelectedItem.Value.ToString();
                    }
                }

                TextBox orderingStateTextBox = ( TextBox )VendorOrderingContactFormView.FindControl( "OrderingStateTextBox" );
                if( orderingStateTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.OrderingState = orderingStateTextBox.Text;
                }

                TextBox orderingZipTextBox = ( TextBox )VendorOrderingContactFormView.FindControl( "OrderingZipTextBox" );
                if( orderingZipTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.OrderingZip = orderingZipTextBox.Text;
                }

                TextBox orderingContactPhoneTextBox = ( TextBox )VendorOrderingContactFormView.FindControl( "OrderingContactPhoneTextBox" );
                if( orderingContactPhoneTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.OrderingTelephone = orderingContactPhoneTextBox.Text;
                }

                TextBox orderingContactPhoneExtTextBox = ( TextBox )VendorOrderingContactFormView.FindControl( "OrderingContactPhoneExtTextBox" );
                if( orderingContactPhoneExtTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.OrderingExtension = orderingContactPhoneExtTextBox.Text;
                }

                TextBox orderingContactFaxTextBox = ( TextBox )VendorOrderingContactFormView.FindControl( "OrderingContactFaxTextBox" );
                if( orderingContactFaxTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.OrderingFax = orderingContactFaxTextBox.Text;
                }

                TextBox orderingContactEmailTextBox = ( TextBox )VendorOrderingContactFormView.FindControl( "OrderingContactEmailTextBox" );
                if( orderingContactEmailTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.OrderingEmail = orderingContactEmailTextBox.Text;
                }
            }
            // VendorSalesContactFormView
            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorSalesContactFormView" ) == true )
            {
                TextBox salesContactNameTextBox = ( TextBox )VendorSalesContactFormView.FindControl( "SalesContactNameTextBox" );
                if( salesContactNameTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorSalesContactName = salesContactNameTextBox.Text;
                }

                TextBox salesContactPhoneTextBox = ( TextBox )VendorSalesContactFormView.FindControl( "SalesContactPhoneTextBox" );
                if( salesContactPhoneTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorSalesContactPhone = salesContactPhoneTextBox.Text;
                }

                TextBox salesContactPhoneExtTextBox = ( TextBox )VendorSalesContactFormView.FindControl( "SalesContactPhoneExtTextBox" );
                if( salesContactPhoneExtTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorSalesContactExtension = salesContactPhoneExtTextBox.Text;
                }

                TextBox salesContactFaxTextBox = ( TextBox )VendorSalesContactFormView.FindControl( "SalesContactFaxTextBox" );
                if( salesContactFaxTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorSalesContactFax = salesContactFaxTextBox.Text;
                }

                TextBox salesContactEmailTextBox = ( TextBox )VendorSalesContactFormView.FindControl( "SalesContactEmailTextBox" );
                if( salesContactEmailTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorSalesContactEmail = salesContactEmailTextBox.Text;
                }
            }

            // VendorBusinessAddressFormView
            if( documentControlPresentation.IsFormViewVisibleAndEnabled( "VendorBusinessAddressFormView" ) == true )
            {
                TextBox vendorNameTextBox = ( TextBox )VendorBusinessAddressFormView.FindControl( "VendorNameTextBox" );
                if( vendorNameTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorName = vendorNameTextBox.Text;
                }

                TextBox businessAddress1TextBox = ( TextBox )VendorBusinessAddressFormView.FindControl( "BusinessAddress1TextBox" );
                if( businessAddress1TextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorAddress1 = businessAddress1TextBox.Text;
                }

                TextBox businessAddress2TextBox = ( TextBox )VendorBusinessAddressFormView.FindControl( "BusinessAddress2TextBox" );
                if( businessAddress2TextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorAddress2 = businessAddress2TextBox.Text;
                }

                TextBox businessCityTextBox = ( TextBox )VendorBusinessAddressFormView.FindControl( "BusinessCityTextBox" );
                if( businessCityTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorCity = businessCityTextBox.Text;
                }

                DropDownList businessCountryDropDownList = ( DropDownList )VendorBusinessAddressFormView.FindControl( "BusinessCountryDropDownList" );
                if( businessCountryDropDownList != null )
                {
                    if( businessCountryDropDownList.SelectedItem != null )
                    {
                        dataRelay.EditedDocumentContentFront.VendorCountryId = int.Parse( businessCountryDropDownList.SelectedItem.Value.ToString() );
                        dataRelay.EditedDocumentContentFront.VendorCountryName = businessCountryDropDownList.SelectedItem.Text;
                    }
                }

                DropDownList businessStateDropDownList = ( DropDownList )VendorBusinessAddressFormView.FindControl( "BusinessStateDropDownList" );
                if( businessStateDropDownList != null )
                {
                    if( businessStateDropDownList.SelectedItem != null )
                    {
                        dataRelay.EditedDocumentContentFront.VendorState = businessStateDropDownList.SelectedItem.Value.ToString();
                    }
                }

                TextBox businessZipTextBox = ( TextBox )VendorBusinessAddressFormView.FindControl( "BusinessZipTextBox" );
                if( businessZipTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorZip = businessZipTextBox.Text;
                }

                TextBox businessWebAddressTextBox = ( TextBox )VendorBusinessAddressFormView.FindControl( "BusinessWebAddressTextBox" );
                if( businessWebAddressTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.VendorWebAddress = businessWebAddressTextBox.Text;
                }
            }

            return ( bSuccess );
       }

        //VendorContractAdministratorFormView
        //VendorAlternateContactFormView
        //VendorTechnicalContactFormView
        //VendorEmergencyContactFormView
        //VendorOrderingContactFormView
        //VendorSalesContactFormView
        //VendorBusinessAddressFormView


        protected void VendorContractAdministratorFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            VendorContractAdministratorFormView.Visible = documentControlPresentation.IsFormViewVisible( "VendorContractAdministratorFormView" );
            VendorContractAdministratorFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorContractAdministratorFormView" );
        }

        protected void VendorAlternateContactFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            VendorAlternateContactFormView.Visible = documentControlPresentation.IsFormViewVisible( "VendorAlternateContactFormView" );
            VendorAlternateContactFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorAlternateContactFormView" );
        }

        protected void VendorTechnicalContactFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            VendorTechnicalContactFormView.Visible = documentControlPresentation.IsFormViewVisible( "VendorTechnicalContactFormView" );
            VendorTechnicalContactFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorTechnicalContactFormView" );
        }

        protected void VendorEmergencyContactFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            VendorEmergencyContactFormView.Visible = documentControlPresentation.IsFormViewVisible( "VendorEmergencyContactFormView" );
            VendorEmergencyContactFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorEmergencyContactFormView" );
        }

        protected void VendorOrderingContactFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            VendorOrderingContactFormView.Visible = documentControlPresentation.IsFormViewVisible( "VendorOrderingContactFormView" );
            VendorOrderingContactFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorOrderingContactFormView" );
        }

        protected void VendorSalesContactFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            VendorSalesContactFormView.Visible = documentControlPresentation.IsFormViewVisible( "VendorSalesContactFormView" );
            VendorSalesContactFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorSalesContactFormView" );
        }

        protected void VendorBusinessAddressFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            VendorBusinessAddressFormView.Visible = documentControlPresentation.IsFormViewVisible( "VendorBusinessAddressFormView" );
            VendorBusinessAddressFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorBusinessAddressFormView" );
        }


        protected void VendorBusinessAddressFormView_OnDataBound( object sender, EventArgs e )
        {
            FormView vendorBusinessAddressFormView = ( FormView )sender;

            EditedDocumentContent formViewContent = ( EditedDocumentContent )VendorBusinessAddressFormView.DataItem;

            int selectedVendorCountryId = -1;

            base.LoadCountries();
            DataSet dsCountries = ( DataSet )Cache[ PointsOfContactVendorCountriesCacheName ];
            DropDownList businessCountryDropDownList;

            if( formViewContent != null )
            {
                selectedVendorCountryId = formViewContent.VendorCountryId;
            }

            if( vendorBusinessAddressFormView != null )
            {
                businessCountryDropDownList = ( DropDownList )vendorBusinessAddressFormView.FindControl( "BusinessCountryDropDownList" );

                if( businessCountryDropDownList != null )
                {
                    businessCountryDropDownList.ClearSelection();
                    businessCountryDropDownList.Items.Clear();

                    foreach( DataRow row in dsCountries.Tables[ ContractDB.VendorCountriesTableName ].Rows )
                    {
                        string countryName = row[ "CountryName" ].ToString();
                        string countryIdString = row[ "CountryId" ].ToString();
                        businessCountryDropDownList.Items.Add( new ListItem( countryName, countryIdString ) );
                    }

                    if( businessCountryDropDownList.Items.FindByValue( selectedVendorCountryId.ToString() ) != null )
                    {
                        //  -1 matches -- select -- choice
                        businessCountryDropDownList.Items.FindByValue( selectedVendorCountryId.ToString() ).Selected = true;
                    }
                }
            }

            base.LoadStates( selectedVendorCountryId );
            DataSet dsStateCodes = base.PointsOfContactStateCodeDataSet;  // was:  ( DataSet )Cache[ PointsOfContactStateCodeCacheName ];  4/18/2022
            DropDownList businessStateDropDownList;
            string vendorState = "";

            

            if( formViewContent != null )
            {
                vendorState = formViewContent.VendorState;
            }

            if( vendorBusinessAddressFormView != null )
            {
                businessStateDropDownList = ( DropDownList )vendorBusinessAddressFormView.FindControl( "BusinessStateDropDownList" );

                if( businessStateDropDownList != null )
                {
                    businessStateDropDownList.ClearSelection();
                    businessStateDropDownList.Items.Clear();

                    foreach( DataRow row in dsStateCodes.Tables[ "StateCodesTable" ].Rows )
                    {
                        string stateAbbreviation = row[ "StateAbbreviation" ].ToString();
                        string stateAbbreviationValue = ( stateAbbreviation.CompareTo( "--" ) != 0 ) ? stateAbbreviation : "";
                        businessStateDropDownList.Items.Add( new ListItem( stateAbbreviation, stateAbbreviationValue ) );
                    }

                    if( businessStateDropDownList.Items.FindByText( vendorState ) != null )
                    {
                        businessStateDropDownList.Items.FindByText( vendorState ).Selected = true;
                    }
                }
            }
        }

       
        // "points of contact"
        // The NACCM was originally developed by Greg Gor-ski in Microsoft Access; 
        // Eric Dav-is continued the development and converted the backend into SqlServer;
        // Billy Eyt-el converted the front end from Access into Microsoft asp.net and Visual Basic; ( 2005-2006 )
        // The VB version was maintained and expanded to handle pharmaceutical items under the Zolon contract;
        // by Adrian Zeg-lin, Ram Ven-kata and Santosh And-em ( 2005-2015 );
        // The VB version was rewritten in C# under the Zolon contract by Adrian Zeg-lin and Gaurav Sah-ni ( 2015-2019 );
        // The remaining VB portions were removed under the TTS contract by Adrian Zeg-lin and Peter Lee ( 2019-20xx );

        protected void VendorOrderingContactFormView_OnDataBound( object sender, EventArgs e )
        {
            FormView vendorOrderingContactFormView = ( FormView )sender;

            EditedDocumentContent formViewContent = ( EditedDocumentContent )vendorOrderingContactFormView.DataItem;

            int selectedOrderingCountryId = -1;

            base.LoadCountries();
            DataSet dsCountries = ( DataSet )Cache[ PointsOfContactVendorCountriesCacheName ];
            DropDownList orderingCountryDropDownList;

            if( formViewContent != null )
            {
                selectedOrderingCountryId = formViewContent.OrderingCountryId;
            }

            if( vendorOrderingContactFormView != null )
            {
                orderingCountryDropDownList = ( DropDownList )vendorOrderingContactFormView.FindControl( "OrderingCountryDropDownList" );

                if( orderingCountryDropDownList != null )
                {
                    orderingCountryDropDownList.ClearSelection();
                    orderingCountryDropDownList.Items.Clear();

                    foreach( DataRow row in dsCountries.Tables[ ContractDB.VendorCountriesTableName ].Rows )
                    {
                        string countryName = row[ "CountryName" ].ToString();
                        string countryIdString = row[ "CountryId" ].ToString();
                        orderingCountryDropDownList.Items.Add( new ListItem( countryName, countryIdString ) );
                    }

                    if( orderingCountryDropDownList.Items.FindByValue( selectedOrderingCountryId.ToString() ) != null )
                    {
                        //  -1 matches -- select -- choice
                        orderingCountryDropDownList.Items.FindByValue( selectedOrderingCountryId.ToString() ).Selected = true;
                    }
                }
            }

            base.LoadStates( selectedOrderingCountryId );
            DataSet dsStateCodes = base.PointsOfContactStateCodeDataSet;  // was:  ( DataSet )Cache[ PointsOfContactStateCodeCacheName ];  4/18/2022
            DropDownList orderingStateDropDownList;
            string vendorOrderingContactState = "";
          
            if( formViewContent != null )
            {
                vendorOrderingContactState = formViewContent.OrderingState;
            }

            if( vendorOrderingContactFormView != null )
            {
                orderingStateDropDownList = ( DropDownList )vendorOrderingContactFormView.FindControl( "OrderingStateDropDownList" );

                if( orderingStateDropDownList != null )
                {
                    orderingStateDropDownList.ClearSelection();
                    orderingStateDropDownList.Items.Clear();

                    foreach( DataRow row in dsStateCodes.Tables[ "StateCodesTable" ].Rows )
                    {
                        string stateAbbreviation = row[ "StateAbbreviation" ].ToString();
                        string stateAbbreviationValue = ( stateAbbreviation.CompareTo( "--" ) != 0 ) ? stateAbbreviation : "";
                        orderingStateDropDownList.Items.Add( new ListItem( stateAbbreviation, stateAbbreviationValue ) );
                    }

                    if( orderingStateDropDownList.Items.FindByText( vendorOrderingContactState ) != null )
                    {
                        orderingStateDropDownList.Items.FindByText( vendorOrderingContactState ).Selected = true;
                    }
                }
            }
        }

        protected void OrderingStateDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList orderingStateDropDownList = ( DropDownList )sender;

            int selectedOrderingCountryId;
            string vendorOrderingContactState = "";

            if( orderingStateDropDownList != null )
            {
                FormView vendorOrderingContactFormView = ( FormView )orderingStateDropDownList.NamingContainer;

                if( vendorOrderingContactFormView != null )
                {
                    EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];

                    if( editedDocumentContentFront != null )
                    {
                        selectedOrderingCountryId = editedDocumentContentFront.OrderingCountryId;
                        vendorOrderingContactState = editedDocumentContentFront.OrderingState;

                        base.LoadStates( selectedOrderingCountryId );
                        DataSet dsStateCodes = base.PointsOfContactStateCodeDataSet;

                        orderingStateDropDownList.ClearSelection();
                        orderingStateDropDownList.Items.Clear();

                        foreach( DataRow row in dsStateCodes.Tables[ "StateCodesTable" ].Rows )
                        {
                            string stateAbbreviation = row[ "StateAbbreviation" ].ToString();
                            string stateAbbreviationValue = ( stateAbbreviation.CompareTo( "--" ) != 0 ) ? stateAbbreviation : "";
                            orderingStateDropDownList.Items.Add( new ListItem( stateAbbreviation, stateAbbreviationValue ) );
                        }

                        if( orderingStateDropDownList.Items.FindByText( vendorOrderingContactState ) != null )
                        {
                            orderingStateDropDownList.Items.FindByText( vendorOrderingContactState ).Selected = true;
                        }
                    }
                }
            }
        }

        protected void BusinessStateDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList businessStateDropDownList = ( DropDownList )sender;

            int selectedVendorCountryId;
            string vendorBusinessContactState = "";

            if( businessStateDropDownList != null )
            {               
                EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];

                if( editedDocumentContentFront != null )
                {
                    selectedVendorCountryId = editedDocumentContentFront.VendorCountryId;
                    vendorBusinessContactState = editedDocumentContentFront.VendorState;

                    base.LoadStates( selectedVendorCountryId );
                    DataSet dsStateCodes = base.PointsOfContactStateCodeDataSet;

                    businessStateDropDownList.ClearSelection();
                    businessStateDropDownList.Items.Clear();

                    foreach( DataRow row in dsStateCodes.Tables[ "StateCodesTable" ].Rows )
                    {
                        string stateAbbreviation = row[ "StateAbbreviation" ].ToString();
                        string stateAbbreviationValue = ( stateAbbreviation.CompareTo( "--" ) != 0 ) ? stateAbbreviation : "";
                        businessStateDropDownList.Items.Add( new ListItem( stateAbbreviation, stateAbbreviationValue ) );
                    }

                    if( businessStateDropDownList.Items.FindByText( vendorBusinessContactState ) != null )
                    {
                        businessStateDropDownList.Items.FindByText( vendorBusinessContactState ).Selected = true;
                    }
                }                
            }
        }
    }
}