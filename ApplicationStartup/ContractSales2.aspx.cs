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
using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ContractSales2 : BaseDocumentEditorPage
    {
        public ContractSales2()
            : base( DocumentEditorTypes.Contract )
        {
        }

        private const int SalesViewSalesVarianceBySINButtonFieldNumber = 0; // $$$+
        private const int SalesViewSalesBySINButtonFieldNumber = 1;  // $$$+
        private const int SalesYearQuarterLabelFieldNumber = 2; //$$$+
        private const int SalesVASalesLabelFieldNumber = 3; //$$$+
        private const int SalesVAQuarterlyVarianceLabelFieldNumber = 4; //$$$+
        private const int SalesVAYearlyVarianceLabelFieldNumber = 5; //$$$+
        private const int SalesOGASalesLabelFieldNumber = 6; //$$$+
        private const int SalesOGAQuarterlyVarianceLabelFieldNumber = 7; //$$$+
        private const int SalesOGAYearlyVarianceLabelFieldNumber = 8; //$$$+
        private const int SalesSLGSalesLabelFieldNumber = 9; //$$$+
        private const int SalesSLGQuarterlyVarianceLabelFieldNumber = 10; //$$$+
        private const int SalesSLGYearlyVarianceLabelFieldNumber = 11; //$$$+
        private const int SalesTotalSalesLabelFieldNumber = 12; //$$$+
        private const int SalesTotalQuarterlyVarianceLabelFieldNumber = 13; //$$$+
        private const int SalesTotalYearlyVarianceLabelFieldNumber = 14; //$$$+
        private const int SalesQuarterIdLabelFieldNumber = 15;  // $$$+
        private const int SalesYearLabelFieldNumber = 16;  // $$$+
        private const int SalesQuarterLabelFieldNumber = 17;  // $$$+

        private const int SALESSUMMARYGRIDVIEWROWHEIGHTESTIMATE = 56; 

        private bool _bBlockGridBindingForGridControlPostback = false;

        // this fixes issues with gridview edit save or cancel not redrawing out of edit mode
        protected new void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( Page.IsPostBack != true )
            {
                BindSalesGridView();
            }
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

            CheckEventTarget();

            LoadNonFormViewControls();

            if( Page.IsPostBack == false )
            {
                SetSalesGridViewSelectedItem( 0, true );
                BindSalesGridView();
            }
            else
            {
                RestoreSalesGridViewSelectedItem();
            }

            AssignDataSourceToFormViews();

            if( Page.IsPostBack == false )
            {
                if( CurrentDocumentIsChanging == true )
                {
                    DataRelay.Load();
                }
                BindFormViews();
            }

 
            if( Page.IsPostBack == true )
            {
                string refreshSalesSummaryScreenOnSubmit = "";
                HtmlInputHidden RefreshSalesSummaryScreenOnSubmitHiddenField = ( HtmlInputHidden )ContractFindControl( "RefreshSalesSummaryScreenOnSubmit" );

                if( RefreshSalesSummaryScreenOnSubmitHiddenField != null )
                {
                    refreshSalesSummaryScreenOnSubmit = RefreshSalesSummaryScreenOnSubmitHiddenField.Value;
                    if( refreshSalesSummaryScreenOnSubmit.Contains( "true" ) == true )
                    {
                        // for restore of personalized grid on popup close
                        (( NACCM )this.Master.Master ).SetPostbackFromPopupClose( true );

                        // for reload of personalized data since sales may affect notifications  
                        LoadPersonalizedNotificationsForSales();

                        BindSalesGridView();

                        RefreshSalesSummaryScreenOnSubmitHiddenField.Value = "false";
                    }
                }
            }

        }

        protected void LoadPersonalizedNotificationsForSales()
        {
            MasterPage master = Page.Master;

            if( master != null )
            {
                MasterPage topMaster = master.Master;

                if( topMaster != null )
                {
                    ( ( NACCM )topMaster ).LoadPersonalizedNotifications( true );
                }
            }
        }

        private void CheckEventTarget()
        {
            _bBlockGridBindingForGridControlPostback = false;

            string controlName = Request.Form[ "__EVENTTARGET" ];

            if( controlName != null )
            {
                // set up to block binding if postback is from grid control
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$SalesSummaryGridView$ctl02$ )
                //if( controlName.Contains( "percentOfSalesTextBox" ) == true ||
                //    controlName.Contains( "rebateThresholdTextBox" ) == true ||
                //    controlName.Contains( "startYearQuarterDropDownList" ) == true ||
                //    controlName.Contains( "endYearQuarterDropDownList" ) == true ||
                //    controlName.Contains( "rebateClauseNameDropDownList" ) == true )
                //{
                //    _bBlockGridBindingForGridControlPostback = true;
                //}
            }
        }

        public bool BlockGridBindingForGridControlPostback
        {
            get
            {
                return _bBlockGridBindingForGridControlPostback;
            }
            set
            {
                _bBlockGridBindingForGridControlPostback = value;
            }
        }



        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            SalesHeaderFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            SalesHeaderFormView.DataKeyNames = new string[] { "ContractId" };
        }


        protected void ClearSessionVariables()
        {
            Session[ "SalesSummaryDataSource" ] = null;
            Session[ "SalesSummaryGridViewSelectedIndex" ] = null;
            Session[ "EditContractSalesWindowParms2" ] = null;
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

            SalesHeaderFormView.DataBind();

            // note form view controls are not yet created here
        }

        protected void LoadNonFormViewControls()
        {
            LoadContractSalesSummary();

            SalesSummaryGridView.DataSource = SalesSummaryDataSource;
        }


        protected void BindSalesGridView()
        {
            try
            {
                // bind if postback not due to grid edit
                if( BlockGridBindingForGridControlPostback == false )
                    SalesSummaryGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }


        protected void SalesSummaryGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int quarterId = -1;
            int itemIndex = -1;
            int year = -1;
            int qtr = -1;

            if( e.CommandName.CompareTo( "ViewSalesVarianceBySIN" ) == 0 )
            {
                string commandArgument = e.CommandArgument.ToString();
                string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                itemIndex = int.Parse( argumentList[ 0 ] );
                quarterId = int.Parse( argumentList[ 1 ] );
                year = int.Parse( argumentList[ 2 ] );
                qtr = int.Parse( argumentList[ 3 ] );

                HighlightSalesRow( itemIndex );
                SetSalesGridViewSelectedItem( itemIndex, false );
             
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                Report salesReport = new Report( "/Sales/Reports/SalesVarianceBySIN" );

                salesReport.AddReportUserLoginIdParameter();
                salesReport.AddParameter( "ContractNumber", currentDocument.ContractNumber );
                salesReport.AddParameter( "Year", year.ToString() );
                salesReport.AddParameter( "Qtr", qtr.ToString() );

                Session[ "ReportToShow" ] = salesReport;

                string windowOpenScript = "";

                windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=1,top=20,left=24,width=1260,height=920');";
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ReportViewerWindowOpenScript", windowOpenScript, true );

                // allow the postback to occur 
                SalesSummaryUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else
            {
                if( e.CommandName.CompareTo( "ViewSalesBySINButton" ) == 0 )
                {
                    string commandArgument = e.CommandArgument.ToString();
                    string[] argumentList = commandArgument.Split( new char[] { ',' } );
                    itemIndex = int.Parse( argumentList[ 0 ] );
                    quarterId = int.Parse( argumentList[ 1 ] );
                    year = int.Parse( argumentList[ 2 ] );
                    qtr = int.Parse( argumentList[ 3 ] );

                    HighlightSalesRow( itemIndex );
                    SetSalesGridViewSelectedItem( itemIndex, true );
                    
                    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                    EditContractSalesWindowParms2 EditContractSalesWindowParms2 = new EditContractSalesWindowParms2( itemIndex, currentDocument.ContractNumber, quarterId, year, qtr );
                    Session[ "EditContractSalesWindowParms2" ] = EditContractSalesWindowParms2;

                    string windowOpenScript = "";

                    windowOpenScript = "window.open('EditContractSales2.aspx','EditContractSales2','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=120,left=120,width=1000,height=500');";
                    ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "EditContractSales2WindowOpenScript", windowOpenScript, true );

                    // allow the postback to occur 
                    SalesSummaryUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                }
            }
        }

        public override string GetValidationGroupName()
        {
            return ( "ContractSales2" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            // there are no fields outside of the grid(s) to validate

            return ( bSuccess );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            return ( bSuccess );
        }


        protected void SalesHeaderFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "SalesHeaderFormView" );

            SalesHeaderFormView.Visible = bVisible;
            SalesHeaderFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "SalesHeaderFormView" );


            Button viewIFFSalesComparisonButton = ( Button )SalesHeaderFormView.FindControl( "ViewIFFSalesComparisonButton" );

            if( viewIFFSalesComparisonButton != null )
            {
                viewIFFSalesComparisonButton.Enabled = documentControlPresentation.IsControlEnabled( "ViewIFFSalesComparisonButton" );
            }

            if( bVisible == true )
            {
                EnableDisableSalesEditControls();
               
            }
        }

        protected void SalesGridPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "SalesGridPanel" );

            SalesGridPanel.Visible = bVisible;
            SalesGridPanel.Enabled = documentControlPresentation.IsControlEnabled( "SalesGridPanel" );
        }

        // disable add button and check box
        protected void EnableDisableSalesEditControls()
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument != null )
            {
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.Sales ) != true &&
                    currentDocument.EditStatus != CurrentDocument.EditStatuses.CanEdit )
                {

                    
                }
            }
        }

  

        protected void SalesSummaryGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {

                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
                bool bAllowEdit = true;

                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.Sales ) != true &&
                    currentDocument.EditStatus != CurrentDocument.EditStatuses.CanEdit )
                {
                    bAllowEdit = false;
                }

                if( e.Row.RowType == DataControlRowType.DataRow )
                {

                    if( ( ( ( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                        ( ( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert ) ) )
                    {
                        int odd = 0;
                        Math.DivRem( rowIndex, 2, out odd );

                        Button viewSalesBySINButton = null;
                        viewSalesBySINButton = ( Button )e.Row.FindControl( "ViewSalesBySINButton" );

                        if( bAllowEdit == false )
                        { 
                            if( viewSalesBySINButton != null )
                            {
                                viewSalesBySINButton.Enabled = false;
                            }

                        }

                        if( viewSalesBySINButton != null )
                        {
                            // colors match .SalesSummaryGridItems and  .SalesSummaryGridAltItems
                            string rowColor = "alt";
 
                            if( odd > 0 )
                            {
                                rowColor = "norm";
                            }
                            string windowHighlightCommand = string.Format( "resetSalesSummaryHighlighting( 'SalesSummaryGridView', {0}, '{1}' );", rowIndex, rowColor );
                            viewSalesBySINButton.Attributes.Add( "onclick", windowHighlightCommand );
                        }
                    }
                }
 
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void SalesSummaryGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {
        }

        protected void SalesSummaryGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
 
        }

        protected void SalesSummaryGridView_RowInserting( object sender, GridViewInsertEventArgs e )
        {
        }

        protected void SalesSummaryGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
 
        }

 

        protected void SalesSummaryGridView_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {
        }
        protected void SalesSummaryGridView_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {
        }

        protected void SalesSummaryGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id fields
            if( e.Row.Cells.Count > SalesQuarterIdLabelFieldNumber ) 
            {
                e.Row.Cells[ SalesQuarterIdLabelFieldNumber ].Visible = false;
            }

            if( e.Row.Cells.Count > SalesYearLabelFieldNumber )
            {
                e.Row.Cells[ SalesYearLabelFieldNumber ].Visible = false;
            }

            if( e.Row.Cells.Count > SalesQuarterLabelFieldNumber )
            {
                e.Row.Cells[ SalesQuarterLabelFieldNumber ].Visible = false;
            }
        }

        protected void SalesSummaryGridView_OnSelectedIndexChanged( object sender, EventArgs e )
        {
        }
        protected void SalesSummaryGridView_PreRender( object sender, EventArgs e )
        {
        }

        protected void SalesSummaryGridView_Init( object sender, EventArgs e )
        {
        }



        private bool GetConfirmationMessageResults()
        {
            bool bConfirmationResults = false;
            string confirmationResultsString = "";

            HtmlInputHidden confirmationMessageResultsHiddenField = ( HtmlInputHidden )ContractFindControl( "confirmationMessageResults" );
            if( confirmationMessageResultsHiddenField != null )
            {
                confirmationResultsString = confirmationMessageResultsHiddenField.Value;
                if( confirmationResultsString.Contains( "true" ) == true )
                {
                    bConfirmationResults = true;
                    confirmationMessageResultsHiddenField.Value = "false";
                }
            }

            return ( bConfirmationResults );
        }


   
   
        protected void SetSalesGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {

            if( selectedItemIndex < SalesSummaryGridView.Rows.Count )
            {

                // save for postback
                Session[ "SalesSummaryGridViewSelectedIndex" ] = selectedItemIndex;

                // set the row selected
                SalesSummaryGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true )
                {
                    ScrollToSelectedItem();
                }

                SalesSummaryUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        protected void ScrollToSelectedItem()
        {

            int rowIndex = SalesSummaryGridView.SelectedIndex + 1;

            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( SALESSUMMARYGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( SALESSUMMARYGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string restoreSalesGridSelectionScript = String.Format( "RestoreSalesSummaryGridSelection( {0},{1} );", rowPosition, rowIndex );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestoreSalesGridSelectionScript", restoreSalesGridSelectionScript, true ); // runs after controls established
        }

        protected void HighlightSalesRow( int itemIndex )
        {
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            if( SalesSummaryGridView.HasData() == true )
            {
                GridViewRow row = SalesSummaryGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = SalesSummaryGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = SalesSummaryGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setSalesSummaryHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveSalesHighlightingScript", preserveHighlightingScript, true );
            }
        }

        protected void RestoreSalesGridViewSelectedItem()
        {
            if( Session[ "SalesSummaryGridViewSelectedIndex" ] == null )
                return;

            SalesSummaryGridView.SelectedIndex = int.Parse( Session[ "SalesSummaryGridViewSelectedIndex" ].ToString() );
        }

        protected void ViewSalesVarianceBySINButton_DataBinding( object sender, EventArgs e )
        {
            Button viewSalesVarianceBySINButton = ( Button )sender;
            if( viewSalesVarianceBySINButton != null )
            {
                CMGlobals.MultilineButtonText( viewSalesVarianceBySINButton, new String[] { "View", "Sales Variance", "By SIN" } );
            }
        }

        protected void ViewSalesBySINButton_DataBinding( object sender, EventArgs e )
        {
            Button viewSalesBySINButton = ( Button )sender;
            if( viewSalesBySINButton != null )
            {
                CMGlobals.MultilineButtonText( viewSalesBySINButton, new String[] { "View", "Sales", "By SIN" } );
            }
        }

        protected string FormatAsPercent( object numericObj, object decimalPlacesObj )
        {
            string percentString = "";
            
            if( numericObj != null )
            {
                string numericString = numericObj.ToString();

                if( numericString.Length > 0 )
                {
                    double numeric = double.Parse( numericString );

                    if( decimalPlacesObj != null )
                    {
                        string decimalPlacesString = decimalPlacesObj.ToString();

                        if( decimalPlacesString.Length > 0 )
                        {
                            int decimalPlaces = int.Parse( decimalPlacesString );

                            string formatSpecifier = string.Format( "F{0}", decimalPlacesString ); // ( decimalPlaces != 0 ) ? decimalPlacesString : "" );

                            percentString = string.Format( "{0}%", ( numeric * 100 ).ToString( formatSpecifier ) );
                        }
                    }
                }
            }

            return ( percentString );
        }


        // color the negative variance red
        protected void SalesVariance_OnDataBinding( object sender, EventArgs e )
        {
            Label varianceLabel = ( Label )sender;

            string varianceString = varianceLabel.Text;

            if( varianceString.Contains( '-' ) == true )
                varianceLabel.ForeColor = Color.Red;
            else
                varianceLabel.ForeColor = Color.Green;
        }

      
        protected void ViewIFFSalesComparisonButton_OnClick( object sender, EventArgs e )
        {
            CurrentDocument  currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            Report salesReport = new Report( "/Sales/Reports/IFFCheckComparisonByContract" );
      
            salesReport.AddParameter( "ContractNumber", currentDocument.ContractNumber );
            salesReport.AddReportUserLoginIdParameter();

            Session[ "ReportToShow" ] = salesReport;

            string windowOpenScript = "";

            windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=1,resizable=1,top=20,left=24,width=640,height=840');";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ReportViewerWindowOpenScript", windowOpenScript, true );
        }


        protected void ViewFullSalesHistoryButton_OnClick( object sender, EventArgs e )
        {
            CurrentDocument  currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            Report salesReport = new Report( "/Sales/Reports/DetailedSalesByContractReport" );
      
            salesReport.AddParameter( "ContractNumber", currentDocument.ContractNumber );
            salesReport.AddReportUserLoginIdParameter();

            Session[ "ReportToShow" ] = salesReport;

            string windowOpenScript = "";

            windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=1,top=20,left=24,width=1320,height=840');";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ReportViewerWindowOpenScript", windowOpenScript, true );
        }

        protected void ViewSalesHistoryByQuarterButton_OnClick( object sender, EventArgs e )
        {
             CurrentDocument  currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            Report salesReport = new Report( "/Sales/Reports/QuarterlySalesByContractReport" );
      
            salesReport.AddParameter( "ContractNumber", currentDocument.ContractNumber );
            salesReport.AddReportUserLoginIdParameter();

            Session[ "ReportToShow" ] = salesReport;

            string windowOpenScript = "";

            windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=1,top=20,left=24,width=1310,height=840');";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ReportViewerWindowOpenScript", windowOpenScript, true );  
        }

        protected void ViewSalesHistoryByYearButton_OnClick( object sender, EventArgs e )
        {
            CurrentDocument  currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            Report salesReport = new Report( "/Sales/Reports/YearlySalesByContractReport" );
      
            salesReport.AddParameter( "ContractNumber", currentDocument.ContractNumber );
            salesReport.AddReportUserLoginIdParameter();

            Session[ "ReportToShow" ] = salesReport;

            string windowOpenScript = "";

            windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=1,top=20,left=24,width=1360,height=840');";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ReportViewerWindowOpenScript", windowOpenScript, true );  
        }



    }
}