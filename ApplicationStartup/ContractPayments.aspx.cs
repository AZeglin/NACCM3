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
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ContractPayments : BaseDocumentEditorPage
    {
                        
        private const   int PaymentIdFieldNumber = 13;  // $$$+
  

        private const int PAYMENTGRIDVIEWROWHEIGHTESTIMATE = 48;

        private bool _bBlockGridBindingForGridControlPostback = false;

        public ContractPayments()
            : base( DocumentEditorTypes.Contract )
        {
        }

        // this fixes issues with gridview edit save or cancel not redrawing out of edit mode
        protected new void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( Page.IsPostBack != true )
            {
                BindPaymentGridView();
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

            PaymentEventTarget();

            LoadNonFormViewControls();

            if( Page.IsPostBack == false )
            {
                SetPaymentGridViewSelectedItem( 0, true );
                BindPaymentGridView();
            }
            else
            {
                RestorePaymentGridViewSelectedItem();
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
        }

        private void PaymentEventTarget()
        {
            _bBlockGridBindingForGridControlPostback = false;

            string controlName = Request.Form[ "__EVENTTARGET" ];

            if( controlName != null )
            {
                // set up to block binding if postback is from grid control
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$PaymentGridView$ctl02$ )
                if( controlName.Contains( "yearQuarterDropDownList" ) == true ||
                    controlName.Contains( "paymentAmountTextBox" ) == true ||
                    controlName.Contains( "paymentNumberTextBox" ) == true ||
                    controlName.Contains( "depositNumberTextBox" ) == true ||
                    controlName.Contains( "dateReceivedTextBox" ) == true ||
                    controlName.Contains( "paymentCommentsTextBox" ) == true ||
                    controlName.Contains( "RemovePaymentButton" ) == true )
                {
                    _bBlockGridBindingForGridControlPostback = true;
                }
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

            PaymentsHeaderFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            PaymentsHeaderFormView.DataKeyNames = new string[] { "ContractId" };
        }

        
        protected void ClearSessionVariables()
        {
            Session[ "PaymentGridViewSelectedIndex" ] = null;           
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

            PaymentsHeaderFormView.DataBind();
 
            // note form view controls are not yet created here
        }

        protected void LoadNonFormViewControls()
        {
            LoadContractPayments();

            PaymentGridView.DataSource = PaymentDataSource;
        }


        protected void BindPaymentGridView()
        {
            try
            {
                // bind if postback not due to grid edit
                if( BlockGridBindingForGridControlPostback == false )
                    PaymentGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        public override string GetValidationGroupName()
        {
            return ( "ContractPayments" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            // currently no fields outside of the grid

            return ( bSuccess );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            // currently no fields outside of the grid

            return ( bSuccess );
        }


        protected void PaymentsHeaderFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "PaymentsHeaderFormView" );
            bool bEnabled = documentControlPresentation.IsFormViewEnabled( "PaymentsHeaderFormView" );

            PaymentsHeaderFormView.Visible = bVisible;
            PaymentsHeaderFormView.Enabled = bEnabled;

            if( bVisible == true && bEnabled == true )
            {
                EnableDisablePaymentEditControls();
            }
        }

        protected void PaymentGridPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "PaymentGridPanel" );

            PaymentGridPanel.Visible = bVisible;
            PaymentGridPanel.Enabled = documentControlPresentation.IsControlEnabled( "PaymentGridPanel" );
        }

        // disable add button and check box
        protected void EnableDisablePaymentEditControls()
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument != null )
            {
                // allow payments to be added even if document not active
            //    bool bActive = ( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Active ) ? true : false;
                
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.Checks ) != true )  
                {
                    
                } 
            } 
        }

        protected void PaymentGridView_RowDataBound( object sender,  GridViewRowEventArgs e )
        {
            try
            {

                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;
                CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];

                if( e.Row.RowType == DataControlRowType.DataRow ) 
                { 
                    if( ((( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                        (( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert )) )
                    { 
                       
                        // currently allowing edit of payments even if document has expired
                        if( currentDocument != null )
                        { 
                            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.Checks ) != true ) 
                            { 
                                
                            } 
                        } 
                    } 
                    else 
                    {

                       
                    } 
                } 
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void Save_ButtonClick( object sender, EventArgs e )
        {

        }

        protected void PaymentGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {
        }

            

        protected void PaymentGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id field
            if( e.Row.Cells.Count > PaymentIdFieldNumber ) 
            { 
                e.Row.Cells[ PaymentIdFieldNumber ].Visible = false;
            }
        }

        protected void PaymentGridView_OnSelectedIndexChanged( object sender, EventArgs e )
        {
        }
        protected void PaymentGridView_PreRender( object sender, EventArgs e )
        {
        }

        protected void PaymentGridView_Init( object sender, EventArgs e )
        {
        }




#region "currentselectedpaymentrow"


        protected void SetPaymentGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {

            if( selectedItemIndex < PaymentGridView.Rows.Count ) 
            { 

                // save for postback
                Session[ "PaymentGridViewSelectedIndex" ] = selectedItemIndex;

                // set the row selected
                PaymentGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true ) 
                { 
                    ScrollToSelectedItem();
                }

                PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            } 
        }

        protected void ScrollToSelectedItem()
        {

            int rowIndex = PaymentGridView.SelectedIndex;   // took out + 1 here to make header visible on add

            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( PAYMENTGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( PAYMENTGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string restorePaymentGridSelectionScript = String.Format( "RestorePaymentGridSelection( {0},{1} );", rowPosition, rowIndex );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestorePaymentGridSelectionScript", restorePaymentGridSelectionScript, true ); // runs after controls established
        }

        protected void HighlightPaymentRow( int itemIndex )
        { 
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1; 

            if( PaymentGridView.HasData() == true ) 
            {
                GridViewRow row = PaymentGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate ) 
                {
                    highlightedRowOriginalColor = PaymentGridView.AlternatingRowStyle.BackColor.ToString();
                } 
                else 
                {
                    highlightedRowOriginalColor = PaymentGridView.RowStyle.BackColor.ToString();
                } 
                
                string preserveHighlightingScript = String.Format( "setPaymentHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreservePaymentHighlightingScript", preserveHighlightingScript, true );
            } 
        }

        protected void RestorePaymentGridViewSelectedItem()
        {
            if( Session[ "PaymentGridViewSelectedIndex" ] == null )
                return;
            
            PaymentGridView.SelectedIndex = int.Parse( Session[ "PaymentGridViewSelectedIndex" ].ToString() );

            //ScrollToSelectedItem();

            //PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }


      
#endregion 
  
        private bool ValidatePaymentBeforeUpdate( GridView paymentGridView, int itemIndex, int selectedPaymentId, bool bIsShortSave )
        {
            bool bIsValid = true;   

            return( bIsValid );
        }
    


        protected void ViewIFFSalesComparisonButton_OnClick( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            Report salesReport = new Report( "/Sales/Reports/IFFCheckComparisonByContract" );

            salesReport.AddParameter( "ContractNumber", currentDocument.ContractNumber );
            salesReport.AddReportUserLoginIdParameter();

            Session[ "ReportToShow" ] = salesReport;

            string windowOpenScript = "";

            windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=1,resizable=1,top=20,left=24,width=640,height=840');";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ReportViewerWindowOpenScript", windowOpenScript, true );
        }

#region "updateinsertparameters"

      
 

#endregion

#region "paymentDateSelection"


        protected void yearQuarterDropDownList_DataBound( object sender, EventArgs e )
        {
            DropDownList yearQuarterDropDownList  = ( DropDownList )sender;
            GridViewRow gridViewRow  = ( GridViewRow )yearQuarterDropDownList.NamingContainer;
            int quarterId = -1;
          
            ListItem customListItem  = new ListItem();

            if( gridViewRow != null )
            { 
                DataRowView dataRowView  = ( DataRowView )gridViewRow.DataItem;
                string quarterIdString = dataRowView[ "QuarterId" ].ToString();

                if( quarterIdString != null )
                { 
                    if( int.TryParse( quarterIdString, out quarterId )) 
                    { 
                        ListItem listItem  = yearQuarterDropDownList.Items.FindByValue( quarterId.ToString() );
                        if( listItem != null )
                        {
                            listItem.Selected = true;
                        }
                    } 
                } 
            } 
        }

        protected void yearQuarterDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList yearQuarterDropDownList  = ( DropDownList )sender;
            ListItem selectedItem;

            selectedItem = yearQuarterDropDownList.SelectedItem;

            int selectedQuarterId;

            selectedQuarterId = int.Parse( selectedItem.Value );
        }

 
    

#endregion "paymentDateSelection"

    }
}