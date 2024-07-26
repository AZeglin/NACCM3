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

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ContractItems : BaseDocumentEditorPage
    {
        public ContractItems()
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

            // temporary support for medsurt pricelist to allow old version to coexist
            Session[ "Requested" ] = "Contract2";

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

            LoadItemCounts();

            Guid exportUploadPermissions = Guid.Empty;
       
            if( Session[ "ExportUploadPermissions" ] == null )
            {
                exportUploadPermissions = Guid.NewGuid();
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
                SetExportUploadPermissions( currentDocument, ref exportUploadPermissions );
                Session[ "ExportUploadPermissions" ] = exportUploadPermissions.ToString();
            }
            else
            {
                exportUploadPermissions = Guid.Parse( Session[ "ExportUploadPermissions" ].ToString() );
            }
          
            AssignPricelistButtonActions( exportUploadPermissions );

            if( Page.IsPostBack == true )
            {
                string refreshDateType = "";
                bool bRefreshOrNot = false;

                HiddenField refreshDateValueOnSubmitHiddenField = ( HiddenField )PricelistVerificationFormView.FindControl( "RefreshDateValueOnSubmit" );
                HiddenField refreshOrNotOnSubmitHiddenField = ( HiddenField )PricelistVerificationFormView.FindControl( "RefreshOrNotOnSubmit" );

                if( refreshDateValueOnSubmitHiddenField != null )
                {
                    refreshDateType = refreshDateValueOnSubmitHiddenField.Value.ToString();

                    if( refreshOrNotOnSubmitHiddenField != null )
                    {
                        bRefreshOrNot = Boolean.Parse( refreshOrNotOnSubmitHiddenField.Value );

                        if( refreshDateType.Contains( "Undefined" ) == false )
                        {
                            if( bRefreshOrNot == true )
                            {
                                RefreshDate( refreshDateType );
                            }
                            else
                            {
                                // reset date
                                Session[ refreshDateType ] = Session[ "CalendarInitialDate" ];
                            }

                            refreshDateValueOnSubmitHiddenField.Value = "Undefined";
                            refreshOrNotOnSubmitHiddenField.Value = "False";
                        }
                    }
                }

                // support for R1 style refresh from med/surg screen
                string refreshPricelist;
                HiddenField refreshPricelistScreenOnSubmitHiddenField = ( HiddenField )ItemPriceCountsFormView.FindControl( "RefreshPricelistScreenOnSubmit" );

                if( refreshPricelistScreenOnSubmitHiddenField != null )
                {
                    refreshPricelist = refreshPricelistScreenOnSubmitHiddenField.Value;
                    if( refreshPricelist.Contains( "true" ) == true )
                    {
                        refreshPricelistScreenOnSubmitHiddenField.Value = "false";
                        UpdateItemCounts();
                    }
                }

                // support for R2 refresh from pharm
                string refreshPricelistCounts = "";
                HiddenField refreshPricelistCountsOnSubmitHiddenField = ( HiddenField )ItemPriceCountsFormView.FindControl( "RefreshPricelistCountsOnSubmit" );

                if( refreshPricelistCountsOnSubmitHiddenField != null )
                {
                    refreshPricelistCounts = refreshPricelistCountsOnSubmitHiddenField.Value;
                    if( refreshPricelistCounts.Contains( "true" ) == true )
                    {
                        refreshPricelistCountsOnSubmitHiddenField.Value = "false";
                        UpdateItemCounts();
                    }
                }
 
            }
        }

   

        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            PricelistVerificationFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            PricelistVerificationFormView.DataKeyNames = new string[] { "ContractId" };
            ItemPriceCountsFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            ItemPriceCountsFormView.DataKeyNames = new string[] { "ContractId" };
            PricelistFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            PricelistFormView.DataKeyNames = new string[] { "ContractId" };
            PricelistNotesFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            PricelistNotesFormView.DataKeyNames = new string[] { "ContractId" };
        }

        protected void ClearSessionVariables()
        {
            Session[ "C2PricelistModificationEffective" ] = null;
            Session[ "ItemStartupParameters" ] = null;
        }

        public override void BindAfterShortSave()
        {
            BindFormViews();
        }

        public override void RebindHeader()
        {
            BindHeader();
        }

        protected void BindFormViews()
        {
            BindHeader();

            PricelistVerificationFormView.DataBind();
            ItemPriceCountsFormView.DataBind();
            PricelistFormView.DataBind();
            PricelistNotesFormView.DataBind();

            // note form view controls are not yet created here
        }

        protected void LoadAndBindNonFormViewControls()
        {
        }

        public override string GetValidationGroupName()
        {
            return ( "ContractItems" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            // nothing significant to validate

            return ( bSuccess );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            string modificationEffectiveDate = "";
           
            DateTime parseDate;
            string dateFormat = "M/d/yyyy";

            try
            {
                // PricelistVerificationFormView

                TextBox modificationEffectiveDateTextBox = ( TextBox )PricelistVerificationFormView.FindControl( "ModificationEffectiveDateTextBox" );
                if( modificationEffectiveDateTextBox != null )
                    modificationEffectiveDate = modificationEffectiveDateTextBox.Text;

                if( modificationEffectiveDate.Length > 0 )
                {
                    if( DateTime.TryParseExact( modificationEffectiveDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                       // throw new Exception( "Modification effective date is not a valid date." );
                        ErrorMessage = "Modification effective date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        dataRelay.EditedDocumentContentFront.PricelistVerificationDate = parseDate;
                    }
                }

                CheckBox pricelistVerifiedCheckBox = ( CheckBox )PricelistVerificationFormView.FindControl( "PricelistVerifiedCheckBox" );
                if( pricelistVerifiedCheckBox != null )
                {
                    dataRelay.EditedDocumentContentFront.PricelistVerified = pricelistVerifiedCheckBox.Checked;
                }

                TextBox modificationNumberTextBox = ( TextBox )PricelistVerificationFormView.FindControl( "ModificationNumberTextBox" );
                if( modificationNumberTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.CurrentModNumber = modificationNumberTextBox.Text;
                }

                TextBox verifiedByTextBox = ( TextBox )PricelistVerificationFormView.FindControl( "VerifiedByTextBox" );
                if( verifiedByTextBox != null )
                {
                    dataRelay.EditedDocumentContentFront.PricelistVerifiedBy = verifiedByTextBox.Text;
                }

                TextBox pricelistNotesTextBox = ( TextBox )PricelistNotesFormView.FindControl( "PricelistNotesTextBox" );
                string pricelistNotes = "";
                if( pricelistNotesTextBox != null )
                {
                    pricelistNotes = pricelistNotesTextBox.Text.Replace( "\n", "\r\n" );
                    dataRelay.EditedDocumentContentFront.PricelistNotes = pricelistNotes;
                }
           }
            catch( Exception ex )
            {
                bSuccess = false;
                ErrorMessage = string.Format( "The following exception was encountered validating the pricelist verification information {0}", ex.Message );
            }
            return ( bSuccess );
        }

        protected void PricelistVerificationFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "PricelistVerificationFormView" );

            PricelistVerificationFormView.Visible = bVisible;
            PricelistVerificationFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "PricelistVerificationFormView" );

            if( bVisible == true )
            {
                EnableContractDateEditing( ( FormView )sender );
            }
        }

        protected void ItemPriceCountsFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            ItemPriceCountsFormView.Visible = documentControlPresentation.IsFormViewVisible( "ItemPriceCountsFormView" );
            ItemPriceCountsFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "ItemPriceCountsFormView" );

            if( documentControlPresentation.IsFormViewVisible( "ItemPriceCountsFormView" ) == true )
            {
              //  Label medSurgActivePriceCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "MedSurgActivePriceCountLabel" );
              //  Label medSurgFutureItemCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "MedSurgFutureItemCountLabel" );
                Label pharmaceuticalItemCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "PharmaceuticalItemCountLabel" );
                Label pharmaceuticalCoveredFCPItemCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "PharmaceuticalCoveredFCPItemCountLabel" );
                Label pharmaceuticalPPVTotalItemCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "PharmaceuticalPPVTotalItemCountLabel" );

                Label pharmaceuticalItemCountHeaderLabel = ( Label )ItemPriceCountsFormView.FindControl( "PharmaceuticalItemCountHeaderLabel" );
                Label pharmaceuticalCoveredFCPItemCountHeaderLabel = ( Label )ItemPriceCountsFormView.FindControl( "PharmaceuticalCoveredFCPItemCountHeaderLabel" );
                Label pharmaceuticalPPVTotalItemCountHeaderLabel = ( Label )ItemPriceCountsFormView.FindControl( "PharmaceuticalPPVTotalItemCountHeaderLabel" );

                // medsurg
                TableHeaderCell medSurgActivePriceCountHeaderCell = ( TableHeaderCell )ItemPriceCountsFormView.FindControl( "MedSurgActivePriceCountHeaderCell" );
                TableHeaderCell medSurgFuturePriceCountHeaderCell = ( TableHeaderCell )ItemPriceCountsFormView.FindControl( "MedSurgFuturePriceCountHeaderCell" );
                TableCell medSurgActivePriceCountDataCell = ( TableCell )ItemPriceCountsFormView.FindControl( "MedSurgActivePriceCountDataCell" );
                TableCell medSurgFuturePriceCountDataCell = ( TableCell )ItemPriceCountsFormView.FindControl( "MedSurgFuturePriceCountDataCell" );

                // pharm
                TableHeaderCell pharmaceuticalItemCountHeaderCell = ( TableHeaderCell )ItemPriceCountsFormView.FindControl( "PharmaceuticalItemCountHeaderCell" );
                TableHeaderCell pharmaceuticalCoveredFCPItemCountHeaderCell = ( TableHeaderCell )ItemPriceCountsFormView.FindControl( "PharmaceuticalCoveredFCPItemCountHeaderCell" );
                TableHeaderCell pharmaceuticalPPVTotalItemCountHeaderCell = ( TableHeaderCell )ItemPriceCountsFormView.FindControl( "PharmaceuticalPPVTotalItemCountHeaderCell" );

                TableCell pharmaceuticalItemCountDataCell = ( TableCell )ItemPriceCountsFormView.FindControl( "PharmaceuticalItemCountDataCell" );
                TableCell pharmaceuticalCoveredFCPItemCountDataCell = ( TableCell )ItemPriceCountsFormView.FindControl( "PharmaceuticalCoveredFCPItemCountDataCell" );
                TableCell pharmaceuticalPPVTotalItemCountDataCell = ( TableCell )ItemPriceCountsFormView.FindControl( "PharmaceuticalPPVTotalItemCountDataCell" );

                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                if( currentDocument.IsPharmaceutical( currentDocument.ScheduleNumber ) == true )
                {
                    if( pharmaceuticalItemCountLabel != null )
                        pharmaceuticalItemCountLabel.Visible = true;
                    if( pharmaceuticalCoveredFCPItemCountLabel != null )
                        pharmaceuticalCoveredFCPItemCountLabel.Visible = true;
                    if( pharmaceuticalPPVTotalItemCountLabel != null )
                        pharmaceuticalPPVTotalItemCountLabel.Visible = true;

                    if( pharmaceuticalItemCountHeaderLabel != null )
                        pharmaceuticalItemCountHeaderLabel.Visible = true;
                    if( pharmaceuticalCoveredFCPItemCountHeaderLabel != null )
                        pharmaceuticalCoveredFCPItemCountHeaderLabel.Visible = true;
                    if( pharmaceuticalPPVTotalItemCountHeaderLabel != null )
                        pharmaceuticalPPVTotalItemCountHeaderLabel.Visible = true;

                    if( pharmaceuticalItemCountHeaderCell != null )
                        pharmaceuticalItemCountHeaderCell.Visible = true;
                    if( pharmaceuticalCoveredFCPItemCountHeaderCell != null )
                        pharmaceuticalCoveredFCPItemCountHeaderCell.Visible = true;
                    if( pharmaceuticalPPVTotalItemCountHeaderCell != null )
                        pharmaceuticalPPVTotalItemCountHeaderCell.Visible = true;

                    if( pharmaceuticalItemCountDataCell != null )
                        pharmaceuticalItemCountDataCell.Visible = true;
                    if( pharmaceuticalCoveredFCPItemCountDataCell != null )
                        pharmaceuticalCoveredFCPItemCountDataCell.Visible = true;
                    if( pharmaceuticalPPVTotalItemCountDataCell != null )
                        pharmaceuticalPPVTotalItemCountDataCell.Visible = true;

                }
                else
                {
                    if( pharmaceuticalItemCountLabel != null )
                        pharmaceuticalItemCountLabel.Visible = false;
                    if( pharmaceuticalCoveredFCPItemCountLabel != null )
                        pharmaceuticalCoveredFCPItemCountLabel.Visible = false;
                    if( pharmaceuticalPPVTotalItemCountLabel != null )
                        pharmaceuticalPPVTotalItemCountLabel.Visible = false;

                    if( pharmaceuticalItemCountHeaderLabel != null )
                        pharmaceuticalItemCountHeaderLabel.Visible = false;
                    if( pharmaceuticalCoveredFCPItemCountHeaderLabel != null )
                        pharmaceuticalCoveredFCPItemCountHeaderLabel.Visible = false;
                    if( pharmaceuticalPPVTotalItemCountHeaderLabel != null )
                        pharmaceuticalPPVTotalItemCountHeaderLabel.Visible = false;

                    if( pharmaceuticalItemCountHeaderCell != null )
                        pharmaceuticalItemCountHeaderCell.Visible = false;
                    if( pharmaceuticalCoveredFCPItemCountHeaderCell != null )
                        pharmaceuticalCoveredFCPItemCountHeaderCell.Visible = false;
                    if( pharmaceuticalPPVTotalItemCountHeaderCell != null )
                        pharmaceuticalPPVTotalItemCountHeaderCell.Visible = false;

                    if( pharmaceuticalItemCountDataCell != null )
                        pharmaceuticalItemCountDataCell.Visible = false;
                    if( pharmaceuticalCoveredFCPItemCountDataCell != null )
                        pharmaceuticalCoveredFCPItemCountDataCell.Visible = false;
                    if( pharmaceuticalPPVTotalItemCountDataCell != null )
                        pharmaceuticalPPVTotalItemCountDataCell.Visible = false;

                    medSurgActivePriceCountHeaderCell.Width = new System.Web.UI.WebControls.Unit( "50%" );
                    medSurgFuturePriceCountHeaderCell.Width = new System.Web.UI.WebControls.Unit( "50%" );
                    medSurgActivePriceCountDataCell.Width = new System.Web.UI.WebControls.Unit( "50%" );
                    medSurgFuturePriceCountDataCell.Width = new System.Web.UI.WebControls.Unit( "50%" );
                }
            }
        }

        protected void PricelistFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            PricelistFormView.Visible = documentControlPresentation.IsFormViewVisible( "PricelistFormView" );
            PricelistFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "PricelistFormView" );

            if( documentControlPresentation.IsFormViewVisible( "PricelistFormView" ) == true )
            {
                Button viewEditPharmaceuticalPricelistButton = ( Button )PricelistFormView.FindControl( "ViewEditPharmaceuticalPricelistButton" );
                Button exportUploadPharmaceuticalPricelistButton = ( Button )PricelistFormView.FindControl( "ExportUploadPharmaceuticalPricelistButton" );

                Button viewEditMedSurgPricelistButton = ( Button )PricelistFormView.FindControl( "ViewEditMedSurgPricelistButton" );
                Button exportUploadMedSurgPricelistButton = ( Button )PricelistFormView.FindControl( "ExportUploadMedSurgPricelistButton" );

                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                if( currentDocument.IsPharmaceutical( currentDocument.ScheduleNumber ) == true )
                {
                    viewEditPharmaceuticalPricelistButton.Visible = true;
                    viewEditPharmaceuticalPricelistButton.Enabled = true;
                    exportUploadPharmaceuticalPricelistButton.Visible = true;
                    exportUploadPharmaceuticalPricelistButton.Enabled = true;
                    
                    if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) == true )
                    {
                        exportUploadPharmaceuticalPricelistButton.Enabled = false;
                    }
                }
                else
                {
                    viewEditPharmaceuticalPricelistButton.Visible = false;
                    viewEditPharmaceuticalPricelistButton.Enabled = false;
                    exportUploadPharmaceuticalPricelistButton.Visible = false;
                    exportUploadPharmaceuticalPricelistButton.Enabled = false;

                    if( currentDocument.IsService( currentDocument.ScheduleNumber ) == true ||
                        currentDocument.IsBPA( currentDocument.ScheduleNumber ) == true )
                    {
                        exportUploadMedSurgPricelistButton.Enabled = false;
                        exportUploadMedSurgPricelistButton.Visible = false;
                    }
                }

                // an expired pharm contract will not have any active prices, thus, the export would be blank anyway
                if( currentDocument.ActiveStatus != CurrentDocument.ActiveStatuses.Active )
                {
                     exportUploadPharmaceuticalPricelistButton.Enabled = false;
                }
     
            }
        }

        protected void PricelistNotesFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            PricelistNotesFormView.Visible = documentControlPresentation.IsFormViewVisible( "PricelistNotesFormView" );
            PricelistNotesFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "PricelistNotesFormView" );
        }

        protected void ModificationEffectiveDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox modificationEffectiveDateTextBox = ( TextBox )sender;

            FormView pricelistVerificationFormView;

            if( modificationEffectiveDateTextBox != null )
            {
                pricelistVerificationFormView = ( FormView )modificationEffectiveDateTextBox.NamingContainer;

                if( pricelistVerificationFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )pricelistVerificationFormView.DataItem;

                    DateTime modificationEffectiveDate = editedDocumentContent.PricelistVerificationDate;

                    if( modificationEffectiveDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        modificationEffectiveDateTextBox.Text = "";
                    }
                    else
                    {
                        modificationEffectiveDateTextBox.Text = modificationEffectiveDate.ToString( "MM/dd/yyyy" );
                        Session[ "C2PricelistModificationEffective" ] = modificationEffectiveDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

        protected void PricelistVerificationFormView_OnChange( object sender, EventArgs e )
        {
   //         SetDirtyFlag( "PricelistVerificationFormView" );
        }

        protected void PricelistNotesFormView_OnChange( object sender, EventArgs e )
        {
    //        SetDirtyFlag( "PricelistNotesFormView" );
        }

        // currently no need to restrict this date separately from general contract editing priviledge
        protected void EnableContractDateEditing( FormView sender )
        {
            Page currentPage;
            FormView pricelistVerificationFormView = sender;

            currentPage = pricelistVerificationFormView.Page;

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            bool bUnlimited = false;
            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.ContractUnlimitedDateRange ) == true )
            {
                bUnlimited = true;
            }

            DateTime modificationEffectiveMinDate = DateTime.Now.AddDays( -60 );           
            DateTime maxAllowedDate = DateTime.Now.AddYears( 10 );

            // create image button script
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "C2PricelistModificationEffectiveDateButtonOnClickScript", GetDateButtonScript( "C2PricelistModificationEffective", bUnlimited, modificationEffectiveMinDate, maxAllowedDate ), true );

           
            ImageButton modificationEffectiveDateImageButton = ( ImageButton )pricelistVerificationFormView.FindControl( "ModificationEffectiveDateImageButton" );
            TextBox modificationEffectiveDateTextBox = ( TextBox )pricelistVerificationFormView.FindControl( "ModificationEffectiveDateTextBox" );

            if( modificationEffectiveDateImageButton != null )
            {
                if( modificationEffectiveDateTextBox != null )
                {
                    modificationEffectiveDateImageButton.Visible = true;
                    modificationEffectiveDateTextBox.Enabled = true;
                }                
            }
        }

        public string GetDateButtonScript( string dateTypeString, bool bUnlimitedDateRange, DateTime minAllowedDate, DateTime maxAllowedDate )
        {
            string defaultDateString = "";

            if( Session[ dateTypeString ] != null )
            {
                defaultDateString = Session[ dateTypeString ].ToString();
            }
            else
            {
                defaultDateString = DateTime.Today.ToShortDateString();
            }

            string minAllowedDateString = minAllowedDate.ToShortDateString();
            string maxAllowedDateString = maxAllowedDate.ToShortDateString();

            string script = String.Format( "function On{0}DateButtonClick() {{ window.open('Calendar.aspx?InitialDate={1}&DateType={2}&Unlimited={3}&MinAllowedDate={4}&MaxAllowedDate={5}','Calendar','toolbar=no,menubar=no,resizable=no,scrollbars=no,statusbar=0,location=0,width=250,height=340,left=660,top=300'); return false;}}", dateTypeString, defaultDateString, dateTypeString, ( ( bUnlimitedDateRange == true ) ? 1 : 0 ), minAllowedDateString, maxAllowedDateString );

            return ( script );
        }

        public void RefreshDate( string dateTypeString )
        {
            DateTime displayDate;

            if( dateTypeString.Contains( "C2PricelistModificationEffective" ) == true )
            {
                TextBox modificationEffectiveDateTextBox = ( TextBox )PricelistVerificationFormView.FindControl( "ModificationEffectiveDateTextBox" );
                if( Session[ "C2PricelistModificationEffective" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "C2PricelistModificationEffective" ].ToString() );
                    modificationEffectiveDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    modificationEffectiveDateTextBox.Text = "";
                }
            }
        }

        private void UpdateItemCounts()
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
            currentDocument.UpdateItemCounts();  // reload from database

            LoadItemCounts(); // display the new values

            ContractItemUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        private void LoadItemCounts()
        {
            Label medSurgActivePriceCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "MedSurgActivePriceCountLabel" );
            Label medSurgFutureItemCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "MedSurgFutureItemCountLabel" );
            Label pharmaceuticalItemCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "PharmaceuticalItemCountLabel" );
            Label pharmaceuticalCoveredFCPItemCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "PharmaceuticalCoveredFCPItemCountLabel" );
            Label pharmaceuticalPPVTotalItemCountLabel = ( Label )ItemPriceCountsFormView.FindControl( "PharmaceuticalPPVTotalItemCountLabel" );

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument.IsPharmaceutical( currentDocument.ScheduleNumber ) == true )
            {
                if( pharmaceuticalItemCountLabel != null )
                    pharmaceuticalItemCountLabel.Text = currentDocument.DrugItemCount.ToString();
                if( pharmaceuticalCoveredFCPItemCountLabel != null )
                    pharmaceuticalCoveredFCPItemCountLabel.Text = String.Format( "{0}/{1}", currentDocument.WithFCPDrugItemCount, currentDocument.CoveredDrugItemCount );
                if( pharmaceuticalPPVTotalItemCountLabel != null )
                    pharmaceuticalPPVTotalItemCountLabel.Text = String.Format( "{0}/{1}", currentDocument.PPVDrugItemCount, currentDocument.DrugItemCount );
            }

            if( medSurgActivePriceCountLabel != null )
                medSurgActivePriceCountLabel.Text = String.Format( "{0}", currentDocument.ActiveMedSurgItemCount );
            if( medSurgFutureItemCountLabel != null )
                medSurgFutureItemCountLabel.Text = String.Format( "{0}", currentDocument.FutureMedSurgItemCount );
        }

        // open the correct pricelist based on the document type
        private void AssignPricelistButtonActions( Guid exportUploadPermissions )
        {
            string msPricelistEditWindowOpenCommand = "";
            string msPricelistExportWindowOpenCommand = "";
            string pharmPricelistEditWindowOpenCommand = "";
            string pharmPricelistExportWindowOpenCommand = "";
            string isContractActive = "";
            string pricelistType = "";
            string isEditable = "N";
            string isNational = "N";
            string vendorName = "";
            string applicationVersion = "";
            int priceListWindowWidth = 0;
            string applicationDirectory = "";

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument.ActiveStatus == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.ActiveStatuses.Expired )
            {
                isContractActive = "F";
            }
            else
            {
                isContractActive = "T";
            }

            if( currentDocument.EditStatus == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.EditStatuses.CanEdit )
            {
                isEditable = "Y";
            }

            if( currentDocument.Division == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.Divisions.National )
            {
                isNational = "Y";
            }

            vendorName = CMGlobals.ReplaceQuote( currentDocument.VendorName, "^" );

            applicationVersion = ( ( Config.ItemVersion.CompareTo( "I1" ) != 0 ) &&  ( Config.ItemVersion.CompareTo( "I2" ) != 0 ) ) ? "I2" : Config.ItemVersion;

            applicationDirectory = "NACCMExportUpload";     
            
            Button viewEditPharmaceuticalPricelistButton = ( Button )PricelistFormView.FindControl( "ViewEditPharmaceuticalPricelistButton" );
            Button exportUploadPharmaceuticalPricelistButton = ( Button )PricelistFormView.FindControl( "ExportUploadPharmaceuticalPricelistButton" );

            Button viewEditMedSurgPricelistButton = ( Button )PricelistFormView.FindControl( "ViewEditMedSurgPricelistButton" );
            Button exportUploadMedSurgPricelistButton = ( Button )PricelistFormView.FindControl( "ExportUploadMedSurgPricelistButton" );

            // $$$ future - modify pharm to use ItemStartupParameters object like new med/surg screen
            if( currentDocument.IsPharmaceutical(currentDocument.ScheduleNumber) == true )
            {               
                if( viewEditPharmaceuticalPricelistButton != null )
                {
                    Session[ "LoadComplete" ] = false; // initializes loader flag used by DrugItems.aspx
                    pharmPricelistEditWindowOpenCommand = "window.open('DrugItems.aspx?ContractNumber=" + currentDocument.ContractNumber + "&VendorName=" + vendorName + "&Edit=" + isEditable + "&National=" + isNational + "','Pricelist','resizable=1,scrollbars=0,top=100,left=150,width=1220,height=810,modal=1')";
                    viewEditPharmaceuticalPricelistButton.Attributes.Add( "onclick", pharmPricelistEditWindowOpenCommand );
                }

                if( exportUploadPharmaceuticalPricelistButton != null )
                {
                    pharmPricelistExportWindowOpenCommand = string.Format( "window.open('/{0}/ItemExportUpload.aspx?ContractNumber={1}&ScheduleNumber={2}&ExportUploadType=P&Id={3}&ContractId={4}&IsBPA={5}&Ver={6}','ExportUpload','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=394,height=680,left=170,top=90,status=no');", applicationDirectory, currentDocument.ContractNumber, currentDocument.ScheduleNumber.ToString(), exportUploadPermissions.ToString(), currentDocument.ContractId.ToString(), ( ( currentDocument.DocumentType == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.DocumentTypes.BPA ) ? "true" : "false" ), applicationVersion );
                    exportUploadPharmaceuticalPricelistButton.Attributes.Add( "onclick", pharmPricelistExportWindowOpenCommand );
                }
            }

            if( currentDocument.DocumentType == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.DocumentTypes.BPA )
            {
                pricelistType = "B";
                priceListWindowWidth = 800;
            }
            else
            {
                if( currentDocument.IsService( currentDocument.ScheduleNumber ) == true )
                {
                    pricelistType = "6";
                    priceListWindowWidth = 680;
                }
                else
                {
                    pricelistType = "F";
                    priceListWindowWidth = 770;
                }
            }

            Items.ItemStartupParameters itemStartupParameters = new Items.ItemStartupParameters();
            itemStartupParameters.ContractId = currentDocument.ContractId;
            itemStartupParameters.ContractNumber = currentDocument.ContractNumber;
            itemStartupParameters.IsItemEditable = currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.MedSurgItems );
            itemStartupParameters.IsItemDetailsEditable = currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.MedSurgItemDetails );
            itemStartupParameters.IsPriceEditable = currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.MedSurgPrices );
            itemStartupParameters.IsPriceDetailsEditable = currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.MedSurgPriceDetails );
            itemStartupParameters.VendorName = currentDocument.VendorName;
            itemStartupParameters.IsNational = ( currentDocument.Division == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.Divisions.National ) ? true : false;
            itemStartupParameters.IsBOA = ( currentDocument.IsBOA( currentDocument.ScheduleNumber )) ? true : false;
            itemStartupParameters.IsBPA = ( currentDocument.DocumentType == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.DocumentTypes.BPA ) ? true : false;
            itemStartupParameters.ParentContractId = ( currentDocument.DocumentType == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.DocumentTypes.BPA ) ? currentDocument.ParentDocument.ContractId : -1;
            itemStartupParameters.IsService = currentDocument.IsService( currentDocument.ScheduleNumber );
            itemStartupParameters.IsExpired = ( currentDocument.ActiveStatus == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.ActiveStatuses.Expired )  ? true : false;
            itemStartupParameters.ContractExpirationDate = currentDocument.ExpirationDate;

            if( itemStartupParameters.IsBPA == true || itemStartupParameters.IsNational == true )
            {
                string defaultSIN = "";
                bool bSuccess = false;
                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                if( contractDB != null )
                {
                    bSuccess = contractDB.GetContractDefaultSIN( currentDocument.ContractNumber, ref defaultSIN );

                    if( bSuccess == false )
                    {
                        throw new Exception( string.Format( "Exception encountered when retrieving default SIN for contract {0}", contractDB.ErrorMessage ) );
                    }
                }

                itemStartupParameters.DefaultSIN = defaultSIN;
            }
            else
            {
                itemStartupParameters.DefaultSIN = "";
            }
             
            Session[ "ItemStartupParameters" ] = itemStartupParameters;

            if( Config.ItemVersion.CompareTo( "I2" ) != 0 )
            {
            msPricelistEditWindowOpenCommand = "window.open('MedSurg_Pricelist.aspx?CntrctNum=" + currentDocument.ContractNumber + "&Edit=" + isEditable + "&IsContractActive=" + isContractActive + "&PricelistType=" + pricelistType + "','Pricelist','resizable=0,scrollbars=1,width=" + priceListWindowWidth.ToString() + ",height=500,left=280,top=140' )";
            }
            else
            {
                msPricelistEditWindowOpenCommand = "window.open('Items.aspx','MSPricelist','resizable=1,scrollbars=0,top=100,left=150,width=1220,height=810,modal=1')";
            }


            if( viewEditMedSurgPricelistButton != null )
            {
                viewEditMedSurgPricelistButton.Attributes.Add( "onclick", msPricelistEditWindowOpenCommand );
            }
                       
            if( exportUploadMedSurgPricelistButton != null )
            {
                msPricelistExportWindowOpenCommand = string.Format( "window.open('/{0}/ItemExportUpload.aspx?ContractNumber={1}&ScheduleNumber={2}&ExportUploadType=M&Id={3}&ContractId={4}&IsBPA={5}&Ver={6}','ExportUpload','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=394,height=680,left=170,top=90,status=no');", applicationDirectory, currentDocument.ContractNumber, currentDocument.ScheduleNumber, exportUploadPermissions.ToString(), currentDocument.ContractId.ToString(), ( ( currentDocument.DocumentType == VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.DocumentTypes.BPA ) ? "true" : "false" ), applicationVersion );

                exportUploadMedSurgPricelistButton.Attributes.Add( "onclick", msPricelistExportWindowOpenCommand );
            }
      
        }

        // any positions between  1 and 128
        private const int MSExport = 13;
        private const int MSUpload = 22;
        private const int PharmExport = 24;
        private const int PharmUpload = 37;
        private const int CanEdit = 87;
        private const int CanView = 88;

        protected void SetExportUploadPermissions( CurrentDocument currentDocument, ref Guid exportUploadPermissions )
        {
            bool bIsMedSurgExportAuthorized = false;
            bool bIsMedSurgUploadAuthorized = false;
            bool bIsPharmExportAuthorized = false;
            bool bIsPharmUploadAuthorized = false;
            bool bCanEdit = false;
            bool bCanView = false;
            
            bIsMedSurgExportAuthorized = currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.MedSurgItemExport );
            bIsMedSurgUploadAuthorized = currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.MedSurgItemUpload );
            bIsPharmExportAuthorized = currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemExport );
            bIsPharmUploadAuthorized = currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemUpload );

            if( currentDocument.EditStatus == CurrentDocument.EditStatuses.CanEdit )
                bCanEdit = true;

            if( currentDocument.EditStatus == CurrentDocument.EditStatuses.CanView )
                bCanView = true;
                 
            // encode the permissions in the guid
            // a 16 element byte array
            // e.g., C9 8B 91 35 6D 19 EA 40 97 79 88 9D 79 B7 53 F0
            Byte[] bytes = exportUploadPermissions.ToByteArray();
            // 16 * 8 = 128 bits
            BitArray bits = new BitArray( bytes );

            string bitString1 = GetStringFromBitArray( bits );

            // set the permissions
            bits[ MSExport ] = bIsMedSurgExportAuthorized;
            bits[ MSUpload ] = bIsMedSurgUploadAuthorized;
            bits[ PharmExport ] = bIsPharmExportAuthorized;
            bits[ PharmUpload ] = bIsPharmUploadAuthorized;
            bits[ CanEdit ] = bCanEdit;
            bits[ CanView ] = bCanView;

            string bitString2 = GetStringFromBitArray( bits );

            // back to bytes     
            Byte[] newBytes = new byte[ 16 ];
            bits.CopyTo( newBytes, 0 );

            exportUploadPermissions = new Guid( newBytes );
        }

        protected string GetStringFromBitArray( BitArray bits )
        {
            StringBuilder sb = new StringBuilder( 128 );

            for( int i = 0; i < bits.Count; i++ )
            {
                if( bits[ i ] == true )
                    sb.Append( "1" );
                else
                    sb.Append( "0" );
            }

            return ( sb.ToString() );
        }
    }
}