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
    public partial class ContractSelectBody : BaseSearchPage
    {
        private const int CONTRACTHEADERGRIDVIEWROWHEIGHTESTIMATE = 36; // 38 scrolls way too far, 37 scrolls about 2 pgs too far, 36 scrolls 1 page too far

        private const int EXPIRATIONDATECOLUMN = 7;

        private bool _bBlockGridBindingForGridControlPostback = false;

        public ContractSelectBody()
            : base( SearchPageTypes.Contract )
        { 
        }

        protected void Page_Load( object sender, EventArgs e )
        {

            string controlName = CheckEventTarget();
            Master.ContractListFilterParms.EventTargetControlName = controlName;
            
            if( Page.IsPostBack == false )
            {
                if( ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
                {
                    ClearSessionVariables();
                    Master.GatherFilterValues( Master.ContractListFilterParms );
                }
                else
                {
                    ( ( ContractSearch )Page.Master ).RestoreSearchFromFilterParms();
                }
            }
            else
            {
                Master.GatherFilterValues( Master.ContractListFilterParms );
            }


            if( Page.IsPostBack == false && ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
            {
                SetSearchGridViewSelectedItem( 0, true );
            }
            else if( Page.IsPostBack == false )
            {
                RestoreSearchGridViewSelectedItem();
            }
            else if( Page.IsPostBack == true )
            {
                //  copy contract has multiple hidden vars set on close, since in release 2 always loading and binding, only one of the flags is relevant
                string refreshContractSelectScreenOnSubmit = "";
                HiddenField refreshContractSelectScreenOnSubmitHiddenField = ( HiddenField )SearchGridPanel.FindControl( "RefreshContractSelectScreenOnSubmit" );

                if( refreshContractSelectScreenOnSubmitHiddenField != null )
                {
                    refreshContractSelectScreenOnSubmit = refreshContractSelectScreenOnSubmitHiddenField.Value;
                    if( refreshContractSelectScreenOnSubmit.Contains( "true" ) == true )
                    {
                        // cache was already reset by successful copy operation, leaving this here for other future purposes
                        Master.SetContractSearchDataDirtyFlag( true );

                        AdjustContractSearchScreenSelectedIndexDueToDetailsChanged();
                    }
                }
            }
        
            Master.LoadContractHeaders2( Master.ContractListFilterParms );

            // highlight the selected item
            if( Session[ "SearchGridViewSelectedIndex" ] != null )   // added 2 if-checks 8/30/2016 $$$ still to test
            {
                int selectedItemIndex = 0;
                if( int.TryParse( Session[ "SearchGridViewSelectedIndex" ].ToString(), out selectedItemIndex ) == true )
                {
                    HighlightContractHeaderRow( selectedItemIndex );
                }
            }

            BindSearchGrid();
        }

 
        private void ClearSessionVariables()
        {
            Session[ "SearchGridViewSelectedIndex" ] = null;
            Session[ "CopyContractCurrentRowOffset" ] = null;
        }

        private string CheckEventTarget()
        {
            _bBlockGridBindingForGridControlPostback = false;

            string controlName = Request.Form[ "__EVENTTARGET" ];

            if( controlName != null )
            {
                // set up to block binding if postback is from grid control
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$SearchGridView$ctl??$ )
                if( controlName.Contains( "SelectContractButton" ) == true )
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
                // bind if postback not due to grid edit
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
            GridView SearchGridView = ( GridView )sender;
            SearchGridView.SetContextMenu( SearchContextMenu );

            SearchGridView.ContextMenuItemCommand += new ContextMenuCommandEventHandler( SearchGridView_ContextMenuItemCommand );

            SearchContextMenu.ContextMenuItems.Add( new ContextMenuItem( "Copy Contract", "CopyContractCommand" ) );
        }

        protected void SearchGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightContractHeaderRow( 0 );
            ( ( BaseSearchPage )this ).HideProgressIndicator();
        }

   

        void SearchGridView_ContextMenuItemCommand( ContextMenu theMenu, ContextMenuCommandEventArgs args )
        {
            string commandName = args.CommandName;
            int itemIndex = args.GridViewRowId;
            int contractId = SearchGridView.GetRowIdFromSelectedIndex( itemIndex, 0 );
            string contractNumber = SearchGridView.GetStringValueFromSelectedIndexForTemplateField( itemIndex, "SelectContractButton" );
            string contractorName = SearchGridView.GetStringValueFromSelectedIndexForBoundField( itemIndex, "Contractor_Name" );
            string commodityCovered = SearchGridView.GetStringValueFromSelectedIndexForBoundField( itemIndex, "Drug_Covered" );
            int scheduleNumber = SearchGridView.GetRowIdFromSelectedIndex( itemIndex, 1 );

            if( commandName.CompareTo( "CopyContractCommand" ) == 0 )
            {
                SetSearchGridViewSelectedItem( itemIndex, true );

                HighlightContractHeaderRow( itemIndex );

                OpenCopyContractWindow( itemIndex, contractId, contractNumber, contractorName, commodityCovered, scheduleNumber );
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
            if( e.CommandName.CompareTo( "EditSearchContract" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                int selectedItemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                int contractRecordId = Int32.Parse( commandArgs[ 1 ].ToString() );
                string contractNumber = commandArgs[ 2 ].ToString();
                int scheduleNumber = Int32.Parse( commandArgs[ 3 ].ToString() );

                if( selectedItemIndex < SearchGridView.Rows.Count )
                {
                    SetSearchGridViewSelectedItem( selectedItemIndex, false );
                }

                ( ( NACCM )Page.Master.Master ).ViewSelectedContract( contractRecordId, contractNumber, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.ContractSearchGridView );

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

 
        private void RestoreSearchGridViewSelectedItem()
        {
            int selectedIndex = 0;
            if( Session[ "SearchGridViewSelectedIndex" ] != null )
            {
                selectedIndex = ( int )Session[ "SearchGridViewSelectedIndex" ];
            }

            SetSearchGridViewSelectedItem( selectedIndex, true );
        }

        private void AdjustContractSearchScreenSelectedIndexDueToDetailsChanged()
        {
            int currentItemIndex = 0;
            if( Session[ "SearchGridViewSelectedIndex" ] != null )
            {
                currentItemIndex = ( int )Session[ "SearchGridViewSelectedIndex" ];
            }
            int adjustedCurrentItemIndex = currentItemIndex;

            // adjust the current item index
            int offset = 0;
            if( Session[ "CopyContractCurrentRowOffset" ] != null )
            {
                offset = ( int )Session[ "CopyContractCurrentRowOffset" ];  // could be 1 or 0
                adjustedCurrentItemIndex += offset;

                // if removing first item ( remove not currently implemented )
                if( adjustedCurrentItemIndex < 0 )
                    adjustedCurrentItemIndex = 0;
            }

            Session[ "SearchGridViewSelectedIndex" ] = adjustedCurrentItemIndex;

        }

        public void SetSearchGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {
            // save for postback
            Session[ "SearchGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            if( SearchGridView.HasData() == true )  // added 8/30/2016 $$$ still to test
            {
                SearchGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true )
                    ScrollToSelectedItem();
            }
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

            int rowPosition = ( CONTRACTHEADERGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( CONTRACTHEADERGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
        }

        public void HighlightContractHeaderRow( int itemIndex )
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

  
        private void OpenCopyContractWindow( int itemIndex, int selectedContractId, string contractNumber, string contractorName, string commodityCovered, int scheduleNumber )
        {
            Session[ "CopyContractWindowParms" ] = null;
            Session[ "CopyContractWindowParms" ] = new CopyContractWindowParms( itemIndex, selectedContractId, contractNumber, contractorName, commodityCovered, scheduleNumber );
 
            string windowOpenScript = "window.open('CopyContract.aspx','CopyContract','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=200,left=240,width=500,height=436, resizable=0');"; 
            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "CopyContractWindowOpenScript", windowOpenScript, true );
        }

        protected void SearchGridView_OnSorting( object sender, GridViewSortEventArgs e )
        {
            Sort( e.SortExpression );
        }

        private void Sort( string sortExpression )
        {
            Master.ContractListFilterParms.SortExpression = sortExpression;

            SetSortDirectionInFilterParms( sortExpression );

            Master.GatherFilterValues( Master.ContractListFilterParms );

            if( Master.ContractListFilterParms.FilterValue.Contains( ";" ) == true )
            {
                //            MsgBox.AlertFromUpdatePanel( Page, "Semi Colons are not allow as a filter criteria." );
                //            return;
            }

            Master.ContractListFilterParms.IsSecondaryFilterDirty = true;

            SortContractHeaders( Master.ContractListFilterParms );

            BindSearchGrid();

            SetSearchGridViewSelectedItem( 0, true );


        }

        private void SortContractHeaders( ContractListFilterParms contractListFilterParms )
        {
            Master.ContractHeaderDataView.Sort = string.Format( "{0} {1}", contractListFilterParms.SortExpression, ( Master.ContractListFilterParms.SortDirection.CompareTo( "Ascending" ) == 0 ) ? "ASC" : "DESC" );
        }



        private void SetSortDirectionInFilterParms( string requestedSortExpression )
        {
            if( requestedSortExpression.CompareTo( Master.ContractListFilterParms.SortExpression ) == 0 )
            {
                if( Master.ContractListFilterParms.SortDirection.CompareTo( "Ascending" ) == 0 )
                {
                    Master.ContractListFilterParms.SortDirection = "Descending";
                }
                else
                {
                    Master.ContractListFilterParms.SortDirection = "Ascending";
                }
            }
            else
            {
                Master.ContractListFilterParms.SortDirection = "Ascending";
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
                    Button selectContractButton = null;
                    selectContractButton = ( Button )e.Row.FindControl( "SelectContractButton" );
                    if( selectContractButton != null )
                    {
                        string contractNumber = ( string )dataRowView[ "CntrctNum" ];
                        int scheduleNumber = Int32.Parse( dataRowView[ "Schedule_Number" ].ToString() );

                        // colors match .SearchGridItems and  .SearchGridAltItems
                        string rowColor = "alt";
                        int odd = 0;
                        Math.DivRem( rowIndex, 2, out odd );
                        if( odd > 0 )
                        {
                            rowColor = "norm";
                        }

                        string windowHighlightCommand = string.Format( "resetSearchHighlighting( 'SearchGridView', {0}, '{1}' );", rowIndex, rowColor );
                        string cursorChangeToHand = string.Format( "contractNumberMouseChange(this, 'over', {0}, '{1}');", rowIndex, rowColor );
                        string cursorChangeToNormal = string.Format( "contractNumberMouseChange(this, 'out', {0}, '{1}');", rowIndex, rowColor );

                        selectContractButton.Attributes.Add( "onclick", windowHighlightCommand );
                        selectContractButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        selectContractButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }
                }
                else if( e.Row.RowType == DataControlRowType.Header )
                {
                    LinkButton headerButton = null;
                    string showProgressIndicator = string.Format( "EnableProgressIndicator(true);" );

                    // ContractNumberColumnHeaderButton                  
                    // ExpirationDateColumnHeaderButton

                    headerButton = ( LinkButton )e.Row.FindControl( "ContractNumberColumnHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }

                    headerButton = ( LinkButton )e.Row.FindControl( "ExpirationDateColumnHeaderButton" );
                    if( headerButton != null )
                    {
                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                    }

                    for( int i = 0; i < e.Row.Controls.Count; i++ )
                    {
                        if( e.Row.Controls[ i ].GetType().Name.CompareTo( "DataControlFieldHeaderCell" ) == 0 )
                        {
                            if( e.Row.Controls[ i ].Controls.Count > 0 )
                            {
                                if( e.Row.Controls[ i ].Controls[ 0 ].GetType().Name.CompareTo( "DataControlLinkButton" ) == 0 )
                                {
                                    // DataControlLinkButton base class is LinkButton
                                    string columnHeaderName = ( ( LinkButton )e.Row.Controls[ i ].Controls[ 0 ] ).Text;

                                    // must match the header text on the grid, these controls do not have a simple clientid
                                    if( columnHeaderName.CompareTo( "Schedule" ) == 0 ||
                                        columnHeaderName.CompareTo( "CO" ) == 0 ||
                                        columnHeaderName.CompareTo( "Contractor Name" ) == 0 ||
                                        columnHeaderName.CompareTo( "Commodity Covered" ) == 0 ||
                                        columnHeaderName.CompareTo( "Dates Awarded" ) == 0 )
                                    {
                                        headerButton = ( LinkButton )e.Row.Controls[ i ].Controls[ 0 ];
                                        headerButton.Attributes.Add( "onclick", showProgressIndicator );
                                    }
                                }
                            }
                        }                       
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

        //  format ContractExpirationDate label with "expired:"  if expired
        public void ContractExpirationDate_OnDataBinding( object sender, EventArgs e )
        {
            Label expirationDateLabel = ( Label )sender;
            
            GridViewRow gridViewRow = ( GridViewRow )expirationDateLabel.NamingContainer;            

            if( gridViewRow.DataItem != null )
            {
                if( ( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_CntrctExp" ] ) != null )
                {
                    DateTime expirationDate;

                    if( DateTime.TryParse( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_CntrctExp" ].ToString(), out expirationDate ) == true )
                    {
                        // if expired
                        if( DateTime.Compare( expirationDate, DateTime.Today ) < 0 )
                        {

                            if( ( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_Completion" ] ) != null )
                            {
                                DateTime completionDate;

                                if( DateTime.TryParse( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_Completion" ].ToString(), out completionDate ) == true )
                                {
                                    // if completed and completion is less than expiration, then don't show expired, becauase it completed first
                                    if( DateTime.Compare( completionDate, expirationDate ) < 0 )
                                    {
                                        // dont show expired
                                        expirationDateLabel.Text = expirationDate.ToString( "d" );
                                        expirationDateLabel.ForeColor = Color.Red;
                                    }
                                    else
                                    {
                                        // show expired
                                        expirationDateLabel.Text = "Expired: " + expirationDate.ToString( "d" );
                                        expirationDateLabel.ForeColor = Color.Red;
                                    }
                                }
                                else // no completion, show expired
                                {
                                    expirationDateLabel.Text = "Expired: " + expirationDate.ToString( "d" );
                                    expirationDateLabel.ForeColor = Color.Red;
                                }
                            }
                            else // show expired
                            {
                                expirationDateLabel.Text = "Expired: " + expirationDate.ToString( "d" );
                                expirationDateLabel.ForeColor = Color.Red;
                            }
                        }
                        else
                        {
                            // just show the exp date without any indicator
                            expirationDateLabel.Text = expirationDate.ToString( "d" );
                        }
                    }
                }
            }                    
        }


        public void CompletionDate_OnDataBinding( object sender, EventArgs e )
        {
            Label completionDateLabel = ( Label )sender;

            //  hide or format ContractCancellationDate label
            if( Master.ContractListFilterParms.ContractStatusFilter != ContractListFilterParms.ContractStatusFilters.Active )
            {
                GridViewRow gridViewRow = ( GridViewRow )completionDateLabel.NamingContainer;

                if( gridViewRow.DataItem != null )
                {
                    if( ( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_Completion" ] ) != null )
                    {
                        DateTime completionDate;

                        if( DateTime.TryParse( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_Completion" ].ToString(), out completionDate ) == true )
                        {
                            DateTime expirationDate = DateTime.Parse( ( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_CntrctExp" ] ).ToString() );
                            if( completionDate.CompareTo( expirationDate ) != 0 )
                            {
                                completionDateLabel.Text = "Completed: " + completionDate.ToString( "d" );
                            }
                            else
                            {
                                completionDateLabel.Visible = false;
                            }
                        }
                        else
                        {
                            completionDateLabel.Visible = false;
                        }
                    }
                    else
                    {
                        completionDateLabel.Visible = false;
                    }
                }
                else
                {
                    completionDateLabel.Visible = false;
                }
            }
            else
            {
                completionDateLabel.Visible = false;
            }
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