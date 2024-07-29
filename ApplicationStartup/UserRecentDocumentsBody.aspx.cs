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

using Menu = System.Web.UI.WebControls.Menu;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class UserRecentDocumentsBody : BaseSearchPage
    {

        private const int CONTRACTHEADERGRIDVIEWROWHEIGHTESTIMATE = 36; // 38 scrolls way too far, 37 scrolls about 2 pgs too far, 36 scrolls 1 page too far

        private const int EXPIRATIONDATECOLUMN = 7;

        private bool _bBlockGridBindingForGridControlPostback = false;

        public UserRecentDocumentsBody()
            : base( SearchPageTypes.UserRecentDocuments )
        { 
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            if( Session[ "NACCMStartedProperly" ] == null || Request.UrlReferrer == null )
            {
                Response.Redirect( "~/Start.aspx" );
            }
            
            if( Page.IsPostBack == false )
            {
                if( ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
                {
                    ClearSessionVariables();
                }        
            }

            Master.LoadUserRecentDocuments( 20 );

            // since recent documents are sorted, always scroll to first item
            //if( Page.IsPostBack == false )
            //    RestoreUserRecentDocumentsGridViewSelectedItem();
            //else
                SetUserRecentDocumentsGridViewSelectedItem( 0, true );

            int selectedItemIndex = ( int )Session[ "UserRecentDocumentsGridViewSelectedIndex" ];
            HighlightUserRecentDocumentRow( selectedItemIndex );

            BindUserRecentDocumentsGrid();
        }

        public string GetActiveStatus( string documentType, bool bIsActive, bool bIsCompleted )
        {
            string status = "";

            if( documentType.CompareTo( "Offer" ) == 0 )
            {
                if( bIsActive == true )
                    status = "Active";
                else
                    status = "Completed";
            }
            else if( documentType.CompareTo( "Contract" ) == 0 )
            {
                if( bIsActive == true )
                    status = "Active";
                else
                {
                    if( bIsCompleted == true )
                        status = "Cancelled";
                    else
                        status = "Expired";
                }
            }

            return ( status );
        }

        private void ClearSessionVariables()
        {
            Session[ "UserRecentDocumentsGridViewSelectedIndex" ] = null;
        }

        private string CheckEventTarget()
        {
            _bBlockGridBindingForGridControlPostback = false;

            string controlName = Request.Form[ "__EVENTTARGET" ];

            if( controlName != null )
            {
                // set up to block binding if postback is from grid control
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$UserRecentDocumentsGridView$ctl??$ )
                if( controlName.Contains( "SelectDocumentButton" ) == true )
                {
                    _bBlockGridBindingForGridControlPostback = true;
                }
            }
            else
            {
                controlName = "";
            }

            return ( controlName );
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


        public void BindUserRecentDocumentsGrid()
        {
            try
            {
                // bind if postback not due to grid edit
                if( BlockGridBindingForGridControlPostback == false )
                    UserRecentDocumentsGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }

        protected void UserRecentDocumentsGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightUserRecentDocumentRow( 0 );
            ( ( BaseSearchPage )this ).HideProgressIndicator();
        }

        protected void UserRecentDocumentsGridView_OnRowCommand( object sender, GridViewCommandEventArgs e )
        {
            if( e.CommandName.CompareTo( "EditSelectedDocument" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                int selectedItemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                int documentId = Int32.Parse( commandArgs[ 1 ].ToString() );
                string documentNumber = commandArgs[ 2 ].ToString();
                int scheduleNumber = Int32.Parse( commandArgs[ 3 ].ToString() );
                string documentType = commandArgs[ 4 ].ToString();

                if( selectedItemIndex < UserRecentDocumentsGridView.Rows.Count )
                {
                    SetUserRecentDocumentsGridViewSelectedItem( selectedItemIndex, false );
                }

                if( documentType.CompareTo( "Contract" ) == 0 )
                {
                    ( ( NACCM )Page.Master.Master ).ViewSelectedContract( documentId, documentNumber, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.UserRecentDocumentsGridView );
                }
                else if( documentType.CompareTo( "Offer" ) == 0 )
                {
                    ( ( NACCM )Page.Master.Master ).ViewSelectedOffer( documentId, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.UserRecentDocumentsGridView );
                }

            }

        }


        //private void SetCurrentDocument( string selectedContractNumber, int scheduleNumber )
        //{
        //    CurrentDocument currentDocument = null;
        //    currentDocument = new CurrentDocument( selectedContractNumber, scheduleNumber, ( ContractDB )Session[ "ContractDB" ], ( DrugItemDB )Session[ "DrugItemDB" ] );
        //    Session[ "CurrentDocument" ] = currentDocument;
        //    currentDocument.LookupCurrentDocument();

        //    BrowserSecurity2 browserSecurity = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
        //    browserSecurity.SetDocumentEditStatus( currentDocument );
        //}


        private void RestoreUserRecentDocumentsGridViewSelectedItem()
        {
            int selectedIndex = 0;
            if( Session[ "UserRecentDocumentsGridViewSelectedIndex" ] != null )
            {
                selectedIndex = ( int )Session[ "UserRecentDocumentsGridViewSelectedIndex" ];
            }

            SetUserRecentDocumentsGridViewSelectedItem( selectedIndex, true );
        }

        // copy not supported on recent documents screen
        //private void AdjustContractSearchScreenSelectedIndexDueToDetailsChanged()
        //{
        //    int currentItemIndex = 0;
        //    if( Session[ "UserRecentDocumentsGridViewSelectedIndex" ] != null )
        //    {
        //        currentItemIndex = ( int )Session[ "UserRecentDocumentsGridViewSelectedIndex" ];
        //    }
        //    int adjustedCurrentItemIndex = currentItemIndex;

        //    // adjust the current item index
        //    int offset = 0;
        //    if( Session[ "CopyContractCurrentRowOffset" ] != null )
        //    {
        //        offset = ( int )Session[ "CopyContractCurrentRowOffset" ];
        //        adjustedCurrentItemIndex += offset;

        //        // if removing first item ( remove not currently implemented )
        //        if( adjustedCurrentItemIndex < 0 )
        //            adjustedCurrentItemIndex = 0;
        //    }

        //    Session[ "UserRecentDocumentsGridViewSelectedIndex" ] = adjustedCurrentItemIndex;

        //}

        public void SetUserRecentDocumentsGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {
            // save for postback
            Session[ "UserRecentDocumentsGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            UserRecentDocumentsGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            if( bIncludeScroll == true )
                ScrollToSelectedItem();

            // allow the update postback to occur
            ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedItem()
        {
            int rowIndex = UserRecentDocumentsGridView.SelectedIndex;
            int fudge;

            //if( rowIndex > 2000 )
            fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )80.0 );   // 36 row height plus 50 scrolls about a pg too far
            //if( rowIndex > 1000 )
            //    fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )96.0 );
            //else
            //    fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )97.0 );

            int rowPosition = ( CONTRACTHEADERGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( CONTRACTHEADERGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
        }

        public void HighlightUserRecentDocumentRow( int itemIndex )
        {
            int highlightedRowIndex = itemIndex + 1;

            if( UserRecentDocumentsGridView.HasData() == true )
            {
                if( itemIndex < UserRecentDocumentsGridView.Rows.Count )
                {
                    string rowColor = "alt";
                    int odd = 0;
                    Math.DivRem( highlightedRowIndex, 2, out odd );
                    if( odd > 0 )
                    {
                        rowColor = "norm";
                    }

                    string setSearchHighlightedRowIndexAndOriginalColorScript = String.Format( "setSearchHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, rowColor );
                    ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveItemHighlightingScript", setSearchHighlightedRowIndexAndOriginalColorScript, true );

                    // allow the highlight postback to occur 
                    ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                }
            }
        }

        protected void UserRecentDocumentsGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;

                if( e.Row.RowType == DataControlRowType.DataRow )
                {
                    Button selectDocumentButton = null;
                    selectDocumentButton = ( Button )e.Row.FindControl( "SelectDocumentButton" );
                    if( selectDocumentButton != null )
                    {
                        string documentNumber = ( string )dataRowView[ "DocumentNumber" ];
                        int scheduleNumber = Int32.Parse( dataRowView[ "Schedule_Number" ].ToString() );

                        // colors match .SearchGridItems and  .SearchGridAltItems
                        string rowColor = "alt";
                        int odd = 0;
                        Math.DivRem( rowIndex, 2, out odd );
                        if( odd > 0 )
                        {
                            rowColor = "norm";
                        }

                        string windowHighlightCommand = string.Format( "resetSearchHighlighting( 'UserRecentDocumentsGridView', {0}, '{1}' );", rowIndex, rowColor );
                        string cursorChangeToHand = string.Format( "contractNumberMouseChange(this, 'over', {0}, '{1}');", rowIndex, rowColor );
                        string cursorChangeToNormal = string.Format( "contractNumberMouseChange(this, 'out', {0}, '{1}');", rowIndex, rowColor );

                        selectDocumentButton.Attributes.Add( "onclick", windowHighlightCommand );
                        selectDocumentButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        selectDocumentButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }

                    string documentType = ( string )dataRowView[ "DocumentType" ];
                    bool bIsActive =  bool.Parse( dataRowView[ "ActiveStatus" ].ToString() );
                    bool bIsCompleted = bool.Parse( dataRowView[ "CompletionStatus" ].ToString() );

                    Label activeStatusDataLabel = null;
                    activeStatusDataLabel = ( Label )e.Row.FindControl( "ActiveStatusDataLabel" );

                    if( activeStatusDataLabel != null )
                    {
                        activeStatusDataLabel.Text = GetActiveStatus( documentType, bIsActive, bIsCompleted );
                        if( bIsActive == false )
                        {
                            activeStatusDataLabel.ForeColor = Color.Red;
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void UserRecentDocumentsGridView_OnDataBound( object sender, EventArgs e )
        {
            
        }



        protected void SearchForm_OnInit( object sender, EventArgs e )
        {
            CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
            string screenSizeInfo = string.Format( "H:{0} W:{1}", cmGlobals.ClientScreenHeight, cmGlobals.ClientScreenWidth );
            UserActivity log = new UserActivity( UserActivity.ActionTypes.ScreenResolution, screenSizeInfo, UserActivity.ActionDetailsTypes.ScreenResolutionValues );
            log.LogUserActivity();
        }

        protected void UserRecentDocumentsGridPanel_OnPreRender( object sender, EventArgs e )
        {
            UserRecentDocumentsGridViewDiv.Attributes[ "class" ] = SearchGridDivStyle;
        }

        public string SearchGridDivStyle
        {
            get
            {
                CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
                if( cmGlobals.ClientScreenHeight == 900 && cmGlobals.ClientScreenWidth == 1440 )
                {
                    return ( "SearchGridDivHighRes" );
                }
                else if( cmGlobals.ClientScreenHeight == 768 && cmGlobals.ClientScreenWidth == 1024 )
                {
                    return ( "SearchGridDivMedRes" );
                }
                else
                {
                    return ( "SearchGridDivLowRes" ); // low res default
                }
            }
        }

        public string SearchHeaderDivStyle
        {
            get
            {
                CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
                if( cmGlobals.ClientScreenHeight == 900 && cmGlobals.ClientScreenWidth == 1440 )
                {
                    return ( "SearchHeaderDivHighRes" );
                }
                else if( cmGlobals.ClientScreenHeight == 768 && cmGlobals.ClientScreenWidth == 1024 )
                {
                    return ( "SearchHeaderDivMedRes" );
                }
                else
                {
                    return ( "SearchHeaderDivLowRes" ); // low res default
                }
            }
        }

        //*********************************** shared functions **************************

        private void MultiLineButtonText( Button button, string[] buttonTextArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < buttonTextArray.Count(); i++ )
            {
                sb.AppendLine( buttonTextArray[ i ] );
            }

            button.Text = sb.ToString();
        }
    }
}