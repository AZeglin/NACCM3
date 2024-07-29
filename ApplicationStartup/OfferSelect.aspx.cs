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
    public partial class OfferSelect : System.Web.UI.Page
    {
        private const int OFFERHEADERGRIDVIEWROWHEIGHTESTIMATE = 42;
        private const int CONTRACTNUMBERCOLUMNNUMBER = 6;

        private DataSet _offerHeaderDataSet = null;
        private DataView _offerHeaderDataView = null;
        private OfferListFilterParms _offerListFilterParms = null;
        private OfferSelectStartupParameters _offerSelectStartupParameters = null;

        [Serializable]
        public class OfferSelectStartupParameters
        {
            public string Status = "";
            public string Owner = "";
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );

            GatherStartupParameters();
            CreateFilterParameters();
            GatherFilterValues( _offerListFilterParms, false );

            SetGridTitle( _offerListFilterParms );

            bool bClearOnly = false;

            if( Page.IsPostBack == false )
            {
                // search screen starts blank
                if( _offerListFilterParms.OfferOwnerFilter == OfferListFilterParms.OfferOwnerFilters.All &&
                    _offerListFilterParms.OfferStatusFilter == OfferListFilterParms.OfferStatusFilters.All )
                {
                    bClearOnly = true;
                }
            }

            LoadOfferHeaders( _offerListFilterParms, bClearOnly );

            if( Page.IsPostBack == false )
            {
                SetOfferSelectGridViewSelectedItem( 0, true );
                BindOfferSelectGrid();
            }
            else
            {
                RestoreOfferSelectGridViewSelectedItem();
            }


            bool bRebindItems = false;
            if( Page.IsPostBack == true )
            {
                string refreshOfferSelectScreenOnSubmit = "";
                HiddenField RefreshOfferSelectScreenOnSubmitHiddenField = ( HiddenField )OfferSelectForm.FindControl( "RefreshOfferSelectScreenOnSubmit" );

                if( RefreshOfferSelectScreenOnSubmitHiddenField != null )
                {
                    refreshOfferSelectScreenOnSubmit = RefreshOfferSelectScreenOnSubmitHiddenField.Value;
                    if( refreshOfferSelectScreenOnSubmit.Contains( "true" ) == true )
                    {

                        string rebindOfferSelectScreenOnRefreshOnSubmit = "";
                        HiddenField RebindOfferSelectScreenOnRefreshOnSubmitHiddenField = ( HiddenField )OfferSelectForm.FindControl( "RebindOfferSelectScreenOnRefreshOnSubmit" );

                        if( RebindOfferSelectScreenOnRefreshOnSubmitHiddenField != null )
                        {
                            rebindOfferSelectScreenOnRefreshOnSubmit = RebindOfferSelectScreenOnRefreshOnSubmitHiddenField.Value;
                            if( rebindOfferSelectScreenOnRefreshOnSubmit.Contains( "true" ) == true )
                            {
                                bRebindItems = true;
                                SetDataDirtyFlag( true );
                            }

                            RebindOfferSelectScreenOnRefreshOnSubmitHiddenField.Value = "false";
                        }

                        RefreshOfferSelectScreenOnSubmitHiddenField.Value = "false";

                        LoadOfferHeaders( _offerListFilterParms, false );

                        RefreshOfferSelectScreenDueToDetailsChanged( bRebindItems );
                    }
                    else
                    {
                        string reselectPreviouslySelectedItem = "";
                        HiddenField ReselectPreviouslySelectedItemHiddenField = ( HiddenField )OfferSelectForm.FindControl( "ReselectPreviouslySelectedItem" );

                        if( ReselectPreviouslySelectedItemHiddenField != null )
                        {
                            reselectPreviouslySelectedItem = ReselectPreviouslySelectedItemHiddenField.Value;
                            if( reselectPreviouslySelectedItem.Contains( "true" ) == true )
                            {
                                ReselectPreviouslySelectedItemHiddenField.Value = "false";

                                int currentItemIndex = OfferSelectGridView.SelectedIndex;
                                ScrollToSelectedItem();
                                HighlightOfferHeaderRow( currentItemIndex );
                            }
                        }
                    }
                }
            }
        }

        private void GatherStartupParameters()
        {
            if( Session[ "OfferSelectStartupParameters" ] == null )
            {
                _offerSelectStartupParameters = new OfferSelectStartupParameters();

                _offerSelectStartupParameters.Status = Request.QueryString.Get( "Status" );
                _offerSelectStartupParameters.Owner = Request.QueryString.Get( "Owner" );

                Session[ "OfferSelectStartupParameters" ] = _offerSelectStartupParameters;
            }
            else
            {
                _offerSelectStartupParameters = ( OfferSelectStartupParameters )Session[ "OfferSelectStartupParameters" ];
            }
        }
  
        private void ClearSessionVariables()
        {
            Session[ "OfferHeaderDataSet" ] = null;
            Session[ "OfferSelectGridViewSelectedIndex" ] = null;
            Session[ "OfferListFilterParms" ] = null;
         //   Session[ "CopyOfferCurrentRowOffset" ] = null;
            Session[ "OfferSelectStartupParameters" ] = null;
            Session[ "IsDataDirtyFlag" ] = null;
        }

        private void CreateFilterParameters()
        {
            if( Session[ "OfferListFilterParms" ] == null )
            {
                _offerListFilterParms = new OfferListFilterParms();
                Session[ "OfferListFilterParms" ] = _offerListFilterParms;

                _offerListFilterParms.SortExpression = "Contractor_Name"; // first load is sorted by this field
                _offerListFilterParms.SortDirection = "Ascending";
                _offerListFilterParms.IsDirty = true;
            }
            else
            {
                _offerListFilterParms = ( OfferListFilterParms )Session[ "OfferListFilterParms" ];
            }
        }

        private void SetDataDirtyFlag( bool bIsDirty )
        {
            Session[ "IsDataDirtyFlag" ] = bIsDirty;
        }

        private bool GetDataDirtyFlag()
        {
            if( Session[ "IsDataDirtyFlag" ] == null )
                return false;
            else
                return ( ( bool )Session[ "IsDataDirtyFlag" ] );
        }

        private void GatherFilterValues( OfferListFilterParms offerListFilterParms, bool bClearFilter )
        {
            OfferListFilterParms.FilterTypes selectedFilterType = OfferListFilterParms.FilterTypes.None;
            string selectedFilterValue = "";

            if( bClearFilter == false )
            {
                selectedFilterType = GetSelectedFilterType();
                selectedFilterValue = GetSelectedFilterValue();
            }

            if( _offerListFilterParms.FilterType != selectedFilterType ||
                _offerListFilterParms.FilterValue != selectedFilterValue )
            {
                _offerListFilterParms.IsDirty = true;  // the parameters have changed from the last load
                _offerListFilterParms.FilterType = selectedFilterType;
                _offerListFilterParms.FilterValue = selectedFilterValue;

                _offerListFilterParms.OfferOwnerFilter = OfferListFilterParms.GetOwnerFilterFromString( _offerSelectStartupParameters.Owner );
                _offerListFilterParms.OfferStatusFilter = OfferListFilterParms.GetStatusFilterFromString( _offerSelectStartupParameters.Status );
            }
            else
            {
                _offerListFilterParms.IsDirty = false;
            }
        }

        private void SetGridTitle( OfferListFilterParms offerListFilterParms )
        {
            string gridTitle = "";

            Label offerSelectFormTitleLabel = ( Label )OfferSelectForm.FindControl( "OfferSelectFormTitle" );
            
            if( offerSelectFormTitleLabel != null )
            {
                if( offerListFilterParms.OfferOwnerFilter == OfferListFilterParms.OfferOwnerFilters.Mine )
                {
                    if( offerListFilterParms.OfferStatusFilter == OfferListFilterParms.OfferStatusFilters.Open )
                    {
                        gridTitle = "My Open Offers";
                    }
                    else if( offerListFilterParms.OfferStatusFilter == OfferListFilterParms.OfferStatusFilters.Completed )
                    {
                        gridTitle = "My Closed Offers";
                    }
                    else
                    {
                        gridTitle = "My Offers";
                    }
                }
                else
                {
                    if( offerListFilterParms.OfferStatusFilter == OfferListFilterParms.OfferStatusFilters.Open )
                    {
                        gridTitle = "Open Offers";
                    }
                    else if( offerListFilterParms.OfferStatusFilter == OfferListFilterParms.OfferStatusFilters.Completed )
                    {
                        gridTitle = "Closed Offers";
                    }
                    else
                    {
                        gridTitle = "Offer Search";
                    }
                }

                offerSelectFormTitleLabel.Text = gridTitle;
            }
        }

        private void LoadOfferHeaders( OfferListFilterParms offerListFilterParms, bool bClearOnly )
        {
            bool bSuccess = false;

            if( offerListFilterParms.IsDirty == true || GetDataDirtyFlag() == true )
            {
                Session[ "OfferHeaderDataSet" ] = null;
                _offerHeaderDataSet = null;

                if( bClearOnly == false )
                {

                    OfferDB offerDB = ( OfferDB )Session[ "OfferDB" ];
                    if( offerDB != null )
                    {
                        offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                        offerDB.MakeConnectionString();

                        //if( Config.ItemVersion.CompareTo( "I2" ) == 0 )
                        //{  $$$$$
                            bSuccess = offerDB.SelectOfferHeaders2( ref _offerHeaderDataSet, OfferListFilterParms.GetStringFromOfferStatusFilter( _offerListFilterParms.OfferStatusFilter ), OfferListFilterParms.GetStringFromOfferOwnerFilter( _offerListFilterParms.OfferOwnerFilter ), OfferListFilterParms.GetStringFromFilterType( _offerListFilterParms.FilterType ), _offerListFilterParms.FilterValue, _offerListFilterParms.SortExpression, _offerListFilterParms.SortDirection );
                        //}
                        //else
                        //{
                        //    bSuccess = offerDB.SelectOfferHeaders( ref _offerHeaderDataSet, OfferListFilterParms.GetStringFromOfferStatusFilter( _offerListFilterParms.OfferStatusFilter ), OfferListFilterParms.GetStringFromOfferOwnerFilter( _offerListFilterParms.OfferOwnerFilter ), OfferListFilterParms.GetStringFromFilterType( _offerListFilterParms.FilterType ), _offerListFilterParms.FilterValue, _offerListFilterParms.SortExpression, _offerListFilterParms.SortDirection );
                        //}

                        if( bSuccess == false )
                        {
                            MsgBox.AlertFromUpdatePanel( Page, offerDB.ErrorMessage );
                        }
                        else
                        {
                            _offerHeaderDataView = new DataView( _offerHeaderDataSet.Tables[ 0 ] );
                            Session[ "OfferHeaderDataSet" ] = _offerHeaderDataSet;
                            SetDataDirtyFlag( false );
                        }
                    }
                }

                offerListFilterParms.IsDirty = false;
            }
            else
            {
                if( Session[ "OfferHeaderDataSet" ] != null )
                {
                    _offerHeaderDataSet = ( DataSet )Session[ "OfferHeaderDataSet" ];
                    _offerHeaderDataView = new DataView( _offerHeaderDataSet.Tables[ 0 ] );
                }
            }

            OfferSelectGridView.DataSource = _offerHeaderDataView;

        }


        private void BindOfferSelectGrid()
        {
            try
            {
                // bind
                OfferSelectGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }


        protected void OfferSelectGridView_Init( object sender, EventArgs e )
        {
            //GridView OfferSelectGridView = ( GridView )sender;
            //OfferSelectGridView.SetContextMenu( OfferSelectContextMenu );

            //OfferSelectGridView.ContextMenuItemCommand += new ContextMenuCommandEventHandler( OfferSelectGridView_ContextMenuItemCommand );

            //OfferSelectContextMenu.ContextMenuItems.Add( new ContextMenuItem( "Copy Offer", "CopyOfferCommand" ) );
        }

        protected void OfferSelectGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightOfferHeaderRow( 0 );

            GridView offerSelectGridView = ( GridView )sender;
            GridViewRow headerRow = offerSelectGridView.HeaderRow;

            // no offers
            if( headerRow == null )
                return;

            if( _offerListFilterParms != null )
            {
                if( _offerListFilterParms.OfferStatusFilter == OfferListFilterParms.OfferStatusFilters.Open )
                {
                    // hide contract number column
                    headerRow.Cells[ CONTRACTNUMBERCOLUMNNUMBER ].Visible = false;

                    foreach( GridViewRow gridViewRow in offerSelectGridView.Rows )
                    {
                        gridViewRow.Cells[ CONTRACTNUMBERCOLUMNNUMBER ].Visible = false;
                    }
                }
            }
        }


        void OfferSelectGridView_ContextMenuItemCommand( ContextMenu theMenu, ContextMenuCommandEventArgs args )
        {
            //string commandName = args.CommandName;
            //int itemIndex = args.GridViewRowId;
            //int offerId = OfferSelectGridView.GetRowIdFromSelectedIndex( itemIndex, 0 );
            //string contractNumber = OfferSelectGridView.GetStringValueFromSelectedIndexForTemplateField( itemIndex, "SelectOfferButton" );
            //string contractorName = OfferSelectGridView.GetStringValueFromSelectedIndexForBoundField( itemIndex, "Contractor_Name" );
            //string commodityCovered = OfferSelectGridView.GetStringValueFromSelectedIndexForBoundField( itemIndex, "Drug_Covered" );
            //int scheduleNumber = OfferSelectGridView.GetRowIdFromSelectedIndex( itemIndex, 1 );

            //if( commandName.CompareTo( "CopyOfferCommand" ) == 0 )
            //{
            //    SetOfferSelectGridViewSelectedItem( itemIndex, true );

            //    HighlightOfferHeaderRow( itemIndex );

            //    OpenCopyOfferWindow( itemIndex, offerId, contractNumber, contractorName, commodityCovered, scheduleNumber );
            //}
        }

 

        private OfferListFilterParms.FilterTypes GetSelectedFilterType()
        {
            OfferListFilterParms.FilterTypes selectedFilterType = OfferListFilterParms.FilterTypes.None;

            ListItem selectedItem = SearchDropDownList.SelectedItem;
            if( selectedItem != null )
            {
                if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.ContractingOfficer ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.ContractingOfficer;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.Vendor ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.Vendor;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.Status ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.Status;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.Schedule ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.Schedule;
                }
            }

            return( selectedFilterType );
        }

        private string GetSelectedFilterValue()
        {
            string selectedFilterValue = "";

            selectedFilterValue = OfferSelectFilterValueTextBox.Text;

            return ( selectedFilterValue );
        }

        private void ClearFilterValue()
        {
            OfferSelectFilterValueTextBox.Text = "";
            _offerListFilterParms.FilterValue = "";
        }

        protected void OfferSelectFilterButton_OnClick( object sender, EventArgs e )
        {
            GatherFilterValues( _offerListFilterParms, false );

            if( _offerListFilterParms.FilterValue.Contains( ";" ) == true )
            {
                MsgBox.AlertFromUpdatePanel( Page, "Semi Colons are not allowed as a filter criteria." );

                _offerListFilterParms.FilterValue.Replace( ";", "" );
            }

            if( _offerListFilterParms.OfferOwnerFilter == OfferListFilterParms.OfferOwnerFilters.All &&
                _offerListFilterParms.OfferStatusFilter == OfferListFilterParms.OfferStatusFilters.All )
            {
                LoadOfferHeaders( _offerListFilterParms, false );
            }
            else
            {
                FilterOfferHeaders( _offerListFilterParms );
            }

            BindOfferSelectGrid();

            SetOfferSelectGridViewSelectedItem( 0, true );

            HighlightOfferHeaderRow( 0 );
        }

        protected void OfferSelectViewAllButton_OnClick( object sender, EventArgs e )
        {
            ClearFilterValue();

            GatherFilterValues( _offerListFilterParms, true );

            LoadOfferHeaders( _offerListFilterParms, false );

            BindOfferSelectGrid();

            SetOfferSelectGridViewSelectedItem( 0, true );

            HighlightOfferHeaderRow( 0 );
        }

        protected void OfferSelectGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            string sortColumn = "";

            if( e.CommandName.CompareTo( "Sort" ) == 0 )
            {                                                           
                sortColumn = e.CommandArgument.ToString();
                Sort( sortColumn );
            }

            // can trap editing commands here if needed, prior to individual editing events
        }

        //private void SetCurrentDocument( int offerId, int scheduleNumber, string vendorName, DateTime dateReceived, DateTime dateAssigned, int ownerId, bool bIsCompleted )
        //{
        //    CurrentDocument currentDocument = null;
        //    currentDocument = new CurrentDocument( offerId, scheduleNumber, vendorName, dateReceived, dateAssigned, ownerId, bIsCompleted, ( OfferDB )Session[ "OfferDB" ], ( ContractDB )Session[ "ContractDB" ], ( DrugItemDB )Session[ "DrugItemDB" ] );
        //    Session[ "CurrentDocument" ] = currentDocument;
 
        //    BrowserSecurity2 browserSecurity = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
        //    browserSecurity.SetDocumentEditStatus( currentDocument );
        //}

        private void SetOfferSelectGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {
            // save for postback
            Session[ "OfferSelectGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            OfferSelectGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            if( bIncludeScroll == true )
                ScrollToSelectedItem();

            // allow the update postback to occur
            ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );  
        }

        protected void ScrollToSelectedItem()
        {
            int rowIndex = OfferSelectGridView.SelectedIndex;
            int fudge;

            //if( rowIndex > 2000 )
                fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )50.0 );
            //if( rowIndex > 1000 )
            //    fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )96.0 );
            //else
            //    fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )97.0 );

            int rowPosition = ( OFFERHEADERGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( OFFERHEADERGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
        }

        protected void HighlightOfferHeaderRow( int itemIndex )
        {
            int highlightedRowIndex = itemIndex + 1;

            if( OfferSelectGridView.HasData() == true )
            {
                if( itemIndex < OfferSelectGridView.Rows.Count )
                {
                    string rowColor = "alt";
                    int odd = 0;
                    Math.DivRem( highlightedRowIndex, 2, out odd );
                    if( odd > 0 )
                    {
                        rowColor = "norm";
                    }

                    string setOfferHighlightedRowIndexAndOriginalColorScript = String.Format( "setOfferHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, rowColor );
                    ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveItemHighlightingScript", setOfferHighlightedRowIndexAndOriginalColorScript, true );

                    // allow the highlight postback to occur 
                    ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );  
                }
            }
        }

        private void RestoreOfferSelectGridViewSelectedItem()
        {
            if( Session[ "OfferSelectGridViewSelectedIndex" ] != null )
            {
                int selectedIndex = ( int )Session[ "OfferSelectGridViewSelectedIndex" ];
                OfferSelectGridView.SelectedIndex = selectedIndex;
            }
        }

        // could get called as a result of copy offer or similar offer header operation
        // rebind is true if caller has altered the offer list
        private void RefreshOfferSelectScreenDueToDetailsChanged( bool bRebindItems )
        {
            int currentItemIndex = OfferSelectGridView.SelectedIndex;
            int adjustedCurrentItemIndex = currentItemIndex;

            if( bRebindItems == true )
            {
                // adjust the current item index $$$
                //int offset = 0;
                //if( Session[ "CopyOfferCurrentRowOffset" ] != null )
                //{
                //    offset = ( int )Session[ "CopyOfferCurrentRowOffset" ];
                //    adjustedCurrentItemIndex += offset;

                //    // if removing first item ( remove not currently implemented )
                //    if( adjustedCurrentItemIndex < 0 )
                //        adjustedCurrentItemIndex = 0;
                //}

                SetOfferSelectGridViewSelectedItem( adjustedCurrentItemIndex, false );

                BindOfferSelectGrid();

            }

            ScrollToSelectedItem();
            HighlightOfferHeaderRow( adjustedCurrentItemIndex );

        }

        //private void OpenCopyOfferWindow( int itemIndex, int selectedContractId, string contractNumber, string contractorName, string commodityCovered, int scheduleNumber )
        //{
        //    Session[ "CopyOfferWindowParms" ] = null;
        //    Session[ "CopyOfferWindowParms" ] = new CopyOfferWindowParms( itemIndex, selectedContractId, contractNumber, contractorName, commodityCovered, scheduleNumber );

        //    string windowOpenScript = "window.open('CopyOffer.aspx','CopyOffer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=200,left=240,width=500,height=436, resizable=0');";
        //    ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "CopyOfferWindowOpenScript", windowOpenScript, true );
        //}

        protected void OfferSelectGridView_OnSorting( object sender, GridViewSortEventArgs e )
        {
            Sort( e.SortExpression );
        }

        private void Sort( string sortExpression )
        {
            _offerListFilterParms.SortExpression = sortExpression;

            SetSortDirectionInFilterParms( sortExpression );

            GatherFilterValues( _offerListFilterParms, false );

            if( _offerListFilterParms.FilterValue.Contains( ";" ) == true )
            {
    //            MsgBox.AlertFromUpdatePanel( Page, "Semi Colons are not allow as a filter criteria." );
    //            return;
            }

            _offerListFilterParms.IsDirty = true;

            SortOfferHeaders( _offerListFilterParms );

            BindOfferSelectGrid();

            SetOfferSelectGridViewSelectedItem( 0, true );


        }

        private void SortOfferHeaders( OfferListFilterParms offerListFilterParms )
        {
            _offerHeaderDataView.Sort = string.Format( "{0} {1}", offerListFilterParms.SortExpression, ( _offerListFilterParms.SortDirection.CompareTo( "Ascending" ) == 0 ) ? "ASC" : "DESC" );
        }

        private void FilterOfferHeaders( OfferListFilterParms offerListFilterParms )
        {
            _offerHeaderDataView.RowFilter = string.Format( "{0} like '%{1}%'", OfferListFilterParms.GetFieldNameFromFilterType( offerListFilterParms.FilterType ), _offerListFilterParms.FilterValue.Trim() );
        }

        private void SetSortDirectionInFilterParms( string requestedSortExpression )
        {
            if( requestedSortExpression.CompareTo( _offerListFilterParms.SortExpression ) == 0 )
            {
                if( _offerListFilterParms.SortDirection.CompareTo( "Ascending" ) == 0 )
                {
                    _offerListFilterParms.SortDirection = "Descending";
                }
                else
                {
                    _offerListFilterParms.SortDirection = "Ascending";
                }
            }
            else
            {
                _offerListFilterParms.SortDirection = "Ascending";
            }
        }

        protected void OfferSelectGridView_OnSorted( object sender, EventArgs e )
        {


        }

        protected void OfferSelectGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;

                if( e.Row.RowType == DataControlRowType.DataRow )
                {
                    int scheduleNumber = Int32.Parse( dataRowView[ "Schedule_Number" ].ToString() );
                    string contractNumber = "";
                    bool bAddContractClick = false; 
                    if( dataRowView[ "ContractNumber" ] != System.DBNull.Value )
                    {
                        contractNumber = ( string )dataRowView[ "ContractNumber" ];
                        bAddContractClick = true;
                    }

                    // colors match .DocumentSelectGridItems and  .DocumentSelectGridAltItems
                    string rowColor = "alt";
                    int odd = 0;
                    Math.DivRem( rowIndex, 2, out odd );
                    if( odd > 0 )
                    {
                        rowColor = "norm";
                    }

                    Label selectOfferButton = null;
                    selectOfferButton = ( Label )e.Row.FindControl( "SelectOfferButton" );
                    if( selectOfferButton != null )
                    {
                        int offerId = Int32.Parse( dataRowView[ "Offer_ID" ].ToString() ); 
                        int COID = Int32.Parse( dataRowView[ "CO_ID" ].ToString() );
                        string vendor = ( string )dataRowView[ "Contractor_Name" ];
                        
                        string completeString = dataRowView[ "Complete" ].ToString();
                        
                        DateTime receivedDate;
                        if( dataRowView[ "Dates_Received" ] != System.DBNull.Value )
                        {
                            receivedDate = DateTime.Parse( dataRowView[ "Dates_Received" ].ToString() );
                        }
                        else
                        {
                            receivedDate = DateTime.MinValue;
                        }

                        DateTime assignedDate;
                        if( dataRowView[ "Dates_Assigned" ] != System.DBNull.Value )
                        {
                            assignedDate = DateTime.Parse( dataRowView[ "Dates_Assigned" ].ToString() );
                        }
                        else
                        {
                            assignedDate = DateTime.MinValue;
                        }                

                        // $$$ temporary fix
                        string cleansedVendorName = CMGlobals.ReplaceQuote( vendor, "^" );

                        string windowOpenCommand = string.Format( "openOfferDetailsWindow(this, {0}, {1}, {2}, {3}, '{4}', '{5}', '{6}', '{7}', '{8}', '{9}');", rowIndex, offerId, scheduleNumber, COID, cleansedVendorName, contractNumber, receivedDate, assignedDate, completeString, rowColor );
                        string cursorChangeToHand = string.Format( "vendorMouseChange(this, 'over', {0}, '{1}');", rowIndex, rowColor );
                        string cursorChangeToNormal = string.Format( "vendorMouseChange(this, 'out', {0}, '{1}');", rowIndex, rowColor );

                        selectOfferButton.Attributes.Add( "onclick", windowOpenCommand );
                        selectOfferButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        selectOfferButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }


                    Label selectContractButton = null;
                    selectContractButton = ( Label )e.Row.FindControl( "SelectContractButton" );
                    if( selectContractButton != null )
                    {
                        if( bAddContractClick == true )
                        {
                            string windowOpenCommand = string.Format( "openContractWindowFromOffer(this, {0}, '{1}', {2}, '{3}' );", rowIndex, contractNumber, scheduleNumber, rowColor );
                            string cursorChangeToHand = string.Format( "contractNumberMouseChange(this, 'over', {0}, '{1}');", rowIndex, rowColor );
                            string cursorChangeToNormal = string.Format( "contractNumberMouseChange(this, 'out', {0}, '{1}');", rowIndex, rowColor );

                            selectContractButton.Attributes.Add( "onclick", windowOpenCommand );
                            selectContractButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                            selectContractButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void OfferSelectForm_OnInit( object sender, EventArgs e )
        {
            CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
            string screenSizeInfo = string.Format( "H:{0} W:{1}", cmGlobals.ClientScreenHeight, cmGlobals.ClientScreenWidth );
            UserActivity log = new UserActivity( UserActivity.ActionTypes.ScreenResolution, screenSizeInfo, UserActivity.ActionDetailsTypes.ScreenResolutionValues );
            log.LogUserActivity();
        }

        protected void OfferSelectForm_OnPreRender( object sender, EventArgs e )
        {
            DocumentSelectGridViewDiv.Attributes[ "class" ] = DocumentSelectGridDivStyle;
            DocumentSelectHeaderDiv.Attributes[ "class" ] = DocumentSelectHeaderDivStyle;

        }

        public string DocumentSelectGridDivStyle
        {
            get
            {
                CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
                if( cmGlobals.ClientScreenHeight == 900 && cmGlobals.ClientScreenWidth == 1440 )
                {
                    return ( "DocumentSelectGridDivHighRes" );
                }
                else if( cmGlobals.ClientScreenHeight == 768 && cmGlobals.ClientScreenWidth == 1024 )
                {
                    return ( "DocumentSelectGridDivMedRes" );
                }
                else
                {
                    return ( "DocumentSelectGridDivLowRes" ); // low res default
                }
            }
        }

        public string DocumentSelectHeaderDivStyle
        {
            get
            {
                CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
                if( cmGlobals.ClientScreenHeight == 900 && cmGlobals.ClientScreenWidth == 1440 )
                {
                    return ( "DocumentSelectHeaderDivHighRes" );
                }
                else if( cmGlobals.ClientScreenHeight == 768 && cmGlobals.ClientScreenWidth == 1024 )
                {
                    return ( "DocumentSelectHeaderDivMedRes" );
                }
                else
                {
                    return ( "DocumentSelectHeaderDivLowRes" ); // low res default
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

        protected void OfferSelectScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "OfferSelectErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "OfferSelectErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            ScriptManager1.AsyncPostBackErrorMessage = errorMsg;
        }

    }
}

