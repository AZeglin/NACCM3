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
    public partial class OfferSelectBody : BaseSearchPage
    {
        private const int OFFERHEADERGRIDVIEWROWHEIGHTESTIMATE = 36; // 38 scrolls way too far, 37 scrolls about 2 pgs too far, 36 scrolls 1 page too far

        private const int CONTRACTNUMBERCOLUMNNUMBER = 8;

        bool _bBlockGridBindingForGridControlPostback = false;

        public OfferSelectBody()
            : base( SearchPageTypes.Offer )
        { 
        }

        protected void Page_Load( object sender, EventArgs e )
        {

            string controlName = CheckEventTarget();
            Master.OfferListFilterParms.EventTargetControlName = controlName;
            
            if( Page.IsPostBack == false )
            {
                if( ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
                {
                    ClearSessionVariables();
                    Master.GatherFilterValues( Master.OfferListFilterParms );
                }
                else
                {
                    ( ( OfferSearch )Page.Master ).RestoreSearchFromFilterParms();
                }
            }
            else
            {
                Master.GatherFilterValues( Master.OfferListFilterParms );
            }

            Master.LoadOfferHeaders2( Master.OfferListFilterParms );

            if( Page.IsPostBack == false )
                RestoreSearchGridViewSelectedItem();
            else
                SetSearchGridViewSelectedItem( 0, true );

            int selectedItemIndex = ( int )Session[ "SearchGridViewSelectedIndex" ];
            HighlightOfferHeaderRow( selectedItemIndex );

            BindSearchGrid();
        }

 
        private void ClearSessionVariables()
        {
            Session[ "SearchGridViewSelectedIndex" ] = null;
        }

        private string CheckEventTarget()
        {
            _bBlockGridBindingForGridControlPostback = false;

            string controlName = Request.Form[ "__EVENTTARGET" ];

            if( controlName != null )
            {
                // set up to block binding if postback is from grid control
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$CommonContentPlaceHolder$SearchGridView$ctl??$ )
                if( controlName.Contains( "SelectOfferButton" ) == true )
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

 
        public void BindSearchGrid()
        {
            try
            {
                // bind if postback not due to grid 
                if( BlockGridBindingForGridControlPostback == false )
                    SearchGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }


        protected void SearchGridView_Init( object sender, EventArgs e )
        {

        }

        protected void SearchGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightOfferHeaderRow( 0 );
            ( ( BaseSearchPage )this ).HideProgressIndicator();

            GridView searchGridView = ( GridView )sender;
            GridViewRow headerRow = searchGridView.HeaderRow;

            // no offers
            if( headerRow == null )
                return;

            OfferListFilterParms offerListFilterParms = ( ( OfferSearch )Page.Master ).OfferListFilterParms;

            if( offerListFilterParms != null )
            {
                if( offerListFilterParms.OfferStatusFilter == OfferListFilterParms.OfferStatusFilters.Open )
                {
                    // hide contract number column
                    headerRow.Cells[ CONTRACTNUMBERCOLUMNNUMBER ].Visible = false;

                    foreach( GridViewRow gridViewRow in searchGridView.Rows )
                    {
                        gridViewRow.Cells[ CONTRACTNUMBERCOLUMNNUMBER ].Visible = false;
                    }
                }
            }
        }

        protected void SearchGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            string sortColumn = "";

            if( e.CommandName.CompareTo( "Sort" ) == 0 )
            {
                sortColumn = e.CommandArgument.ToString();
                Sort( sortColumn );
            }

            // can trap editing commands here if needed, prior to individual editing events
        }

        protected void SearchGridView_OnRowCommand( object sender, GridViewCommandEventArgs e )
        {
            char commandSeparator = ( ( GridView )sender ).CommandSeparator;

            if( e.CommandName.CompareTo( "EditSearchOffer" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { commandSeparator } );
                int selectedItemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                int offerId = Int32.Parse( commandArgs[ 1 ].ToString() );
                int scheduleNumber = Int32.Parse( commandArgs[ 2 ].ToString() );
                string scheduleName = commandArgs[ 3 ].ToString();
                string vendorName = commandArgs[ 4 ].ToString();
                DateTime dateReceived = DateTime.Parse( commandArgs[ 5 ].ToString() );
                string dateAssignedString = commandArgs[ 6 ].ToString().Trim();
                DateTime dateAssigned = DateTime.MinValue;
                if( dateAssignedString.Length > 0 )
                {
                    dateAssigned = DateTime.Parse( commandArgs[ 6 ].ToString() );
                }
                int ownerId = Int32.Parse( commandArgs[ 7 ].ToString() );
                string ownerName = commandArgs[ 8 ].ToString();     // CO FullName
                string contractNumber = commandArgs[ 9 ].ToString();
                string contractIdString = commandArgs[ 10 ].ToString();
                int contractId = -1;
                if( contractIdString.Length > 0 )
                {
                    contractId = Int32.Parse( contractIdString );
                }
                bool bIsOfferCompleted = bool.Parse( commandArgs[ 11 ].ToString() );
                string offerNumber = commandArgs[ 12 ].ToString();
                int proposalTypeId = Int32.Parse( commandArgs[ 13 ].ToString() );
                string extendsContractNumber = commandArgs[ 14 ].ToString();
                string extendsContractIdString = commandArgs[ 15 ].ToString();
                int extendsContractId = -1;
                if( extendsContractIdString.Length > 0 )
                {
                    extendsContractId = Int32.Parse( extendsContractIdString );
                }
                if( selectedItemIndex < SearchGridView.Rows.Count )
                {
                    SetSearchGridViewSelectedItem( selectedItemIndex, false );
                }

                ( ( NACCM )Page.Master.Master ).ViewSelectedOffer( offerId, scheduleNumber, scheduleName, vendorName, dateReceived, dateAssigned, ownerId, ownerName, contractNumber, contractId, bIsOfferCompleted, offerNumber, proposalTypeId, extendsContractNumber, extendsContractId, RequestedNextDocument.DocumentChangeRequestSources.OfferSearchGridView );
            }

            else if( e.CommandName.CompareTo( "EditSearchContract" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { commandSeparator } );
                int selectedItemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                int contractId = Int32.Parse( commandArgs[ 1 ].ToString() );
                string contractNumber = commandArgs[ 2 ].ToString();     
                int scheduleNumber = Int32.Parse( commandArgs[ 3 ].ToString() );

                if( selectedItemIndex < SearchGridView.Rows.Count )
                {
                    SetSearchGridViewSelectedItem( selectedItemIndex, false );
                }

                ( ( NACCM )Page.Master.Master ).ViewSelectedContract( contractId, contractNumber, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.OfferSearchGridView );

            }
            else if( e.CommandName.CompareTo( "EditExtendsContract" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { commandSeparator } );
                int selectedItemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                string extendsContractIdString = commandArgs[ 1 ].ToString();
                int extendsContractId = -1;
                if( extendsContractIdString.Length > 0 )
                {
                    extendsContractId = Int32.Parse( extendsContractIdString );
                }
               
                string extendsContractNumber = commandArgs[ 2 ].ToString();      
                int scheduleNumber = Int32.Parse( commandArgs[ 3 ].ToString() );

                if( selectedItemIndex < SearchGridView.Rows.Count )
                {
                    SetSearchGridViewSelectedItem( selectedItemIndex, false );
                }

                ( ( NACCM )Page.Master.Master ).ViewSelectedContract( extendsContractId, extendsContractNumber, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.OfferSearchGridView );

            }

        }
 
        private void RestoreSearchGridViewSelectedItem()
        {
            int selectedIndex = 0;
            if( Session[ "SearchGridViewSelectedIndex" ] != null )
            {
                selectedIndex = ( int )Session[ "SearchGridViewSelectedIndex" ];
            }

            SetSearchGridViewSelectedItem( selectedIndex, true );
        }

        public void SetSearchGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {
            // save for postback
            Session[ "SearchGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            SearchGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            if( bIncludeScroll == true )
                ScrollToSelectedItem();

            // allow the update postback to occur
            ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedItem()
        {
            int rowIndex = SearchGridView.SelectedIndex;
            int fudge;

            //if( rowIndex > 2000 )
            fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )80.0 );   // 36 row height plus 50 scrolls about a pg too far
            //if( rowIndex > 1000 )
            //    fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )96.0 );
            //else
            //    fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )97.0 );

            int rowPosition = ( OFFERHEADERGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( OFFERHEADERGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
        }

        public void HighlightOfferHeaderRow( int itemIndex )
        {
            int highlightedRowIndex = itemIndex + 1;

            if( SearchGridView.HasData() == true )
            {
                if( itemIndex < SearchGridView.Rows.Count )
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

        protected void SearchGridView_OnSorting( object sender, GridViewSortEventArgs e )
        {
            Sort( e.SortExpression );
        }

        private void Sort( string sortExpression )
        {
            Master.OfferListFilterParms.SortExpression = sortExpression;

            SetSortDirectionInFilterParms( sortExpression );

            Master.GatherFilterValues( Master.OfferListFilterParms );

            if( Master.OfferListFilterParms.FilterValue.Contains( ";" ) == true )
            {
                //            MsgBox.AlertFromUpdatePanel( Page, "Semi Colons are not allow as a filter criteria." );
                //            return;
            }

            Master.OfferListFilterParms.IsSecondaryFilterDirty = true;

            SortOfferHeaders( Master.OfferListFilterParms );

            BindSearchGrid();

            SetSearchGridViewSelectedItem( 0, true );


        }

        private void SortOfferHeaders( OfferListFilterParms offerListFilterParms )
        {
            Master.OfferHeaderDataView.Sort = string.Format( "{0} {1}", offerListFilterParms.SortExpression, ( Master.OfferListFilterParms.SortDirection.CompareTo( "Ascending" ) == 0 ) ? "ASC" : "DESC" );
        }



        private void SetSortDirectionInFilterParms( string requestedSortExpression )
        {
            if( requestedSortExpression.CompareTo( Master.OfferListFilterParms.SortExpression ) == 0 )
            {
                if( Master.OfferListFilterParms.SortDirection.CompareTo( "Ascending" ) == 0 )
                {
                    Master.OfferListFilterParms.SortDirection = "Descending";
                }
                else
                {
                    Master.OfferListFilterParms.SortDirection = "Ascending";
                }
            }
            else
            {
                Master.OfferListFilterParms.SortDirection = "Ascending";
            }
        }

        protected void SearchGridView_OnSorted( object sender, EventArgs e )
        {


        }

        protected void SearchGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;

                if( e.Row.RowType == DataControlRowType.DataRow )
                {
          //        int offerId = Int32.Parse( dataRowView[ "Offer_ID" ].ToString() );
          //        int scheduleNumber = Int32.Parse( dataRowView[ "Schedule_Number" ].ToString() );                    
          //        string contractNumber = "";
                    bool bAddContractClick = false;
                    if( dataRowView[ "ContractNumber" ] != System.DBNull.Value )
                    {
          //            contractNumber = ( string )dataRowView[ "ContractNumber" ];
                        bAddContractClick = true;
                    }

                    // colors match .SearchGridItems and  .SearchGridAltItems
                    string rowColor = "alt";
                    int odd = 0;
                    Math.DivRem( rowIndex, 2, out odd );
                    if( odd > 0 )
                    {
                        rowColor = "norm";
                    }

                    Button selectOfferButton = null;
                    selectOfferButton = ( Button )e.Row.FindControl( "SelectOfferButton" );
                    if( selectOfferButton != null )
                    {
                        string windowHighlightCommand = string.Format( "resetSearchHighlighting( 'SearchGridView', {0}, '{1}' );", rowIndex, rowColor );
                        string cursorChangeToHand = string.Format( "offerNumberMouseChange(this, 'over', {0}, '{1}');", rowIndex, rowColor );
                        string cursorChangeToNormal = string.Format( "offerNumberMouseChange(this, 'out', {0}, '{1}');", rowIndex, rowColor );

                        selectOfferButton.Attributes.Add( "onclick", windowHighlightCommand );
                        selectOfferButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        selectOfferButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }

                    Button selectContractButton = null;
                    selectContractButton = ( Button )e.Row.FindControl( "SelectContractButton" );
                    if( selectContractButton != null )
                    {
                        if( bAddContractClick == true )
                        {
                            string windowHighlightCommand = string.Format( "resetSearchHighlighting( 'SearchGridView', {0}, '{1}' );", rowIndex, rowColor );
                            string cursorChangeToHand = string.Format( "contractNumberMouseChange(this, 'over', {0}, '{1}');", rowIndex, rowColor );
                            string cursorChangeToNormal = string.Format( "contractNumberMouseChange(this, 'out', {0}, '{1}');", rowIndex, rowColor );

                            selectContractButton.Attributes.Add( "onclick", windowHighlightCommand );
                            selectContractButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                            selectContractButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                        }
                    }
                }
                else if( e.Row.RowType == DataControlRowType.Header )
                {
                    LinkButton headerButton = null;
                    string showProgressIndicator = string.Format( "EnableProgressIndicator(true);" );
                    
                    // ContractorNameColumnHeaderButton                  
                    // OfferNumberColumnHeaderButton
                    // ReceivedDateColumnHeaderButton
                    // AssignmentDateColumnHeaderButton
                    // ScheduleNameColumnHeaderButton
                    // ContractingOfficerNameColumnHeaderButton
                    // OfferStatusColumnHeaderButton
                    // ContractNumberHeaderButton

                    headerButton = ( LinkButton )e.Row.FindControl( "ContractorNameColumnHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }

                    headerButton = ( LinkButton )e.Row.FindControl( "OfferNumberColumnHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }

                    headerButton = ( LinkButton )e.Row.FindControl( "ReceivedDateColumnHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }

                    headerButton = ( LinkButton )e.Row.FindControl( "AssignmentDateColumnHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }

                    headerButton = ( LinkButton )e.Row.FindControl( "ScheduleNameColumnHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }

                    headerButton = ( LinkButton )e.Row.FindControl( "ContractingOfficerNameColumnHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }

                    headerButton = ( LinkButton )e.Row.FindControl( "OfferStatusColumnHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }

                    headerButton = ( LinkButton )e.Row.FindControl( "ContractNumberHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void SearchGridView_OnDataBound( object sender, EventArgs e )
        {
            //( ( BaseSearchPage )this ).HideProgressIndicator();  moving to prerender
        }

        protected void SearchForm_OnInit( object sender, EventArgs e )
        {
            CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
            string screenSizeInfo = string.Format( "H:{0} W:{1}", cmGlobals.ClientScreenHeight, cmGlobals.ClientScreenWidth );
            UserActivity log = new UserActivity( UserActivity.ActionTypes.ScreenResolution, screenSizeInfo, UserActivity.ActionDetailsTypes.ScreenResolutionValues );
            log.LogUserActivity();
        }

        protected void SearchGridPanel_OnPreRender( object sender, EventArgs e )
        {
            SearchGridViewDiv.Attributes[ "class" ] = SearchGridDivStyle;
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