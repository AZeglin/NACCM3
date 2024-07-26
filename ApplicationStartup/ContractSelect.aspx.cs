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
    public partial class ContractSelect : System.Web.UI.Page
    {
        private const int NUMBEROFITEMGRIDVIEWLEADINGCOLS = 2;
        private const int CONTRACTHEADERGRIDVIEWROWHEIGHTESTIMATE = 42;

        private const int EXPIRATIONDATECOLUMN = 7;

        private DataSet _contractHeaderDataSet = null;
        private DataView _contractHeaderDataView = null;
        private ContractListFilterParms _contractListFilterParms = null;
        private ContractSelectStartupParameters _contractSelectStartupParameters = null;

        [Serializable]
        public class ContractSelectStartupParameters
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
            GatherFilterValues( _contractListFilterParms, false );

        //    if( _contractListFilterParms.FilterValue.Contains( ";" ) == true )
        //    {
        ////        MsgBox.AlertFromUpdatePanel( Page, "Semi Colons are not allowed as a filter criteria." );

        //        _contractListFilterParms.FilterValue = _contractListFilterParms.FilterValue.Replace( ";", "" );
        //    }

            SetGridTitle( _contractListFilterParms );

            bool bClearOnly = false;

            if( Page.IsPostBack == false )
            {
                // search screen starts blank
                if( _contractListFilterParms.ContractOwnerFilter == ContractListFilterParms.ContractOwnerFilters.All &&
                    _contractListFilterParms.ContractStatusFilter == ContractListFilterParms.ContractStatusFilters.All )
                {
                    bClearOnly = true;
                }
            }

            LoadContractHeaders( _contractListFilterParms, bClearOnly );

            if( Page.IsPostBack == false )
            {
                SetContractSelectGridViewSelectedItem( 0, true );
                BindContractSelectGrid();
            }
            else
            {
                RestoreContractSelectGridViewSelectedItem();
            }


            bool bRebindItems = false;
            if( Page.IsPostBack == true )
            {
                string refreshContractSelectScreenOnSubmit = "";
                HiddenField RefreshContractSelectScreenOnSubmitHiddenField = ( HiddenField )ContractSelectForm.FindControl( "RefreshContractSelectScreenOnSubmit" );

                if( RefreshContractSelectScreenOnSubmitHiddenField != null )
                {
                    refreshContractSelectScreenOnSubmit = RefreshContractSelectScreenOnSubmitHiddenField.Value;
                    if( refreshContractSelectScreenOnSubmit.Contains( "true" ) == true )
                    {

                        string rebindContractSelectScreenOnRefreshOnSubmit = "";
                        HiddenField RebindContractSelectScreenOnRefreshOnSubmitHiddenField = ( HiddenField )ContractSelectForm.FindControl( "RebindContractSelectScreenOnRefreshOnSubmit" );

                        if( RebindContractSelectScreenOnRefreshOnSubmitHiddenField != null )
                        {
                            rebindContractSelectScreenOnRefreshOnSubmit = RebindContractSelectScreenOnRefreshOnSubmitHiddenField.Value;
                            if( rebindContractSelectScreenOnRefreshOnSubmit.Contains( "true" ) == true )
                            {
                                bRebindItems = true;
                                SetDataDirtyFlag( true );
                            }

                            RebindContractSelectScreenOnRefreshOnSubmitHiddenField.Value = "false";
                        }

                        RefreshContractSelectScreenOnSubmitHiddenField.Value = "false";

                        LoadContractHeaders( _contractListFilterParms, false );

                        RefreshContractSelectScreenDueToDetailsChanged( bRebindItems );
                    }
                    else
                    {
                        string reselectPreviouslySelectedItem = "";
                        HiddenField ReselectPreviouslySelectedItemHiddenField = ( HiddenField )ContractSelectForm.FindControl( "ReselectPreviouslySelectedItem" );

                        if( ReselectPreviouslySelectedItemHiddenField != null )
                        {
                            reselectPreviouslySelectedItem = ReselectPreviouslySelectedItemHiddenField.Value;
                            if( reselectPreviouslySelectedItem.Contains( "true" ) == true )
                            {
                                ReselectPreviouslySelectedItemHiddenField.Value = "false";

                                int currentItemIndex = ContractSelectGridView.SelectedIndex;
                                ScrollToSelectedItem();
                                HighlightContractHeaderRow( currentItemIndex );
                            }
                        }
                    }
                }
            }
        }

        private void GatherStartupParameters()
        {
            if( Session[ "ContractSelectStartupParameters" ] == null )
            {
                _contractSelectStartupParameters = new ContractSelectStartupParameters();

                _contractSelectStartupParameters.Status = Request.QueryString.Get( "Status" );
                _contractSelectStartupParameters.Owner = Request.QueryString.Get( "Owner" );

                Session[ "ContractSelectStartupParameters" ] = _contractSelectStartupParameters;
            }
            else
            {
                _contractSelectStartupParameters = ( ContractSelectStartupParameters )Session[ "ContractSelectStartupParameters" ];
            }
        }
  
        private void ClearSessionVariables()
        {
            Session[ "ContractHeaderDataSet" ] = null;
            Session[ "ContractSelectGridViewSelectedIndex" ] = null;
            Session[ "ContractListFilterParms" ] = null;
            Session[ "CopyContractCurrentRowOffset" ] = null;
            Session[ "ContractSelectStartupParameters" ] = null;
            Session[ "IsDataDirtyFlag" ] = null;
        }

        private void CreateFilterParameters()
        {
            if( Session[ "ContractListFilterParms" ] == null )
            {
                _contractListFilterParms = new ContractListFilterParms();
                Session[ "ContractListFilterParms" ] = _contractListFilterParms;

                _contractListFilterParms.SortExpression = "CntrctNum"; // first load is sorted by this field
                _contractListFilterParms.SortDirection = "Ascending";
                _contractListFilterParms.IsDirty = true;
            }
            else
            {
                _contractListFilterParms = ( ContractListFilterParms )Session[ "ContractListFilterParms" ];
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

        private void GatherFilterValues( ContractListFilterParms contractListFilterParms, bool bClearFilter )
        {
            ContractListFilterParms.FilterTypes selectedFilterType = ContractListFilterParms.FilterTypes.None;
            string selectedFilterValue = "";

            if( bClearFilter == false )
            {
                selectedFilterType = GetSelectedFilterType();
                selectedFilterValue = GetSelectedFilterValue();
            }

            if( _contractListFilterParms.FilterType != selectedFilterType ||
                _contractListFilterParms.FilterValue != selectedFilterValue )
            {
                _contractListFilterParms.IsDirty = true;  // the parameters have changed from the last load
                _contractListFilterParms.FilterType = selectedFilterType;
                _contractListFilterParms.FilterValue = selectedFilterValue;

                _contractListFilterParms.ContractOwnerFilter = ContractListFilterParms.GetOwnerFilterFromString( _contractSelectStartupParameters.Owner );
                _contractListFilterParms.ContractStatusFilter = ContractListFilterParms.GetStatusFilterFromString( _contractSelectStartupParameters.Status );
            }
            else
            {
                _contractListFilterParms.IsDirty = false;
            }
        }

        private void SetGridTitle( ContractListFilterParms contractListFilterParms )
        {
            string gridTitle = "";

            Label contractSelectFormTitleLabel = ( Label )ContractSelectForm.FindControl( "ContractSelectFormTitle" );
            
            if( contractSelectFormTitleLabel != null )
            {
                if( contractListFilterParms.ContractOwnerFilter == ContractListFilterParms.ContractOwnerFilters.Mine )
                {
                    if( contractListFilterParms.ContractStatusFilter == ContractListFilterParms.ContractStatusFilters.Active )
                    {
                        gridTitle = "My Active Contracts";
                    }
                    else if( contractListFilterParms.ContractStatusFilter == ContractListFilterParms.ContractStatusFilters.Closed )
                    {
                        gridTitle = "My Expired Contracts";
                    }
                    else
                    {
                        gridTitle = "My Contracts";
                    }
                }
                else
                {
                    if( contractListFilterParms.ContractStatusFilter == ContractListFilterParms.ContractStatusFilters.Active )
                    {
                        gridTitle = "Active Contracts";
                    }
                    else if( contractListFilterParms.ContractStatusFilter == ContractListFilterParms.ContractStatusFilters.Closed )
                    {
                        gridTitle = "Expired Contracts";
                    }
                    else
                    {
                        gridTitle = "Contract Search";
                    }
                }

                contractSelectFormTitleLabel.Text = gridTitle;
            }
        }

        private void LoadContractHeaders( ContractListFilterParms contractListFilterParms, bool bClearOnly )
        {
            bool bSuccess = false;

            if( contractListFilterParms.IsDirty == true || GetDataDirtyFlag() == true )
            {
                Session[ "ContractHeaderDataSet" ] = null;
                _contractHeaderDataSet = null;

                if( bClearOnly == false )
                {

                    ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                    if( contractDB != null )
                    {
                        contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                        contractDB.MakeConnectionString();

                        bSuccess = contractDB.SelectContractHeaders( ref _contractHeaderDataSet, ContractListFilterParms.GetStringFromContractStatusFilter( _contractListFilterParms.ContractStatusFilter ), ContractListFilterParms.GetStringFromContractOwnerFilter( _contractListFilterParms.ContractOwnerFilter ), ContractListFilterParms.GetStringFromFilterType( _contractListFilterParms.FilterType ), _contractListFilterParms.FilterValue, _contractListFilterParms.SortExpression, _contractListFilterParms.SortDirection );
                        if( bSuccess == false )
                        {
                            MsgBox.AlertFromUpdatePanel( Page, contractDB.ErrorMessage );
                        }
                        else
                        {
                            _contractHeaderDataView = new DataView( _contractHeaderDataSet.Tables[ 0 ] );
                            Session[ "ContractHeaderDataSet" ] = _contractHeaderDataSet;
                            SetDataDirtyFlag( false );
                        }
                    }
                }

                contractListFilterParms.IsDirty = false;
            }
            else
            {
                if( Session[ "ContractHeaderDataSet" ] != null )
                {
                    _contractHeaderDataSet = ( DataSet )Session[ "ContractHeaderDataSet" ];
                    _contractHeaderDataView = new DataView( _contractHeaderDataSet.Tables[ 0 ] );
                }
            }

            ContractSelectGridView.DataSource = _contractHeaderDataView;

        }


        private void BindContractSelectGrid()
        {
            try
            {
                // bind
                ContractSelectGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }


        protected void ContractSelectGridView_Init( object sender, EventArgs e )
        {
            GridView ContractSelectGridView = ( GridView )sender;
            ContractSelectGridView.SetContextMenu( ContractSelectContextMenu );

            ContractSelectGridView.ContextMenuItemCommand += new ContextMenuCommandEventHandler( ContractSelectGridView_ContextMenuItemCommand );

            ContractSelectContextMenu.ContextMenuItems.Add( new ContextMenuItem( "Copy Contract", "CopyContractCommand" ) );
        }

        protected void ContractSelectGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightContractHeaderRow( 0 );
        }


        void ContractSelectGridView_ContextMenuItemCommand( ContextMenu theMenu, ContextMenuCommandEventArgs args )
        {
            string commandName = args.CommandName;
            int itemIndex = args.GridViewRowId;
            int contractId = ContractSelectGridView.GetRowIdFromSelectedIndex( itemIndex, 0 );
            string contractNumber = ContractSelectGridView.GetStringValueFromSelectedIndexForTemplateField( itemIndex, "SelectContractButton" );
            string contractorName = ContractSelectGridView.GetStringValueFromSelectedIndexForBoundField( itemIndex, "Contractor_Name" );
            string commodityCovered = ContractSelectGridView.GetStringValueFromSelectedIndexForBoundField( itemIndex, "Drug_Covered" );
            int scheduleNumber = ContractSelectGridView.GetRowIdFromSelectedIndex( itemIndex, 1 );

            if( commandName.CompareTo( "CopyContractCommand" ) == 0 )
            {
                SetContractSelectGridViewSelectedItem( itemIndex, true );

                HighlightContractHeaderRow( itemIndex );

                OpenCopyContractWindow( itemIndex, contractId, contractNumber, contractorName, commodityCovered, scheduleNumber );
            }
        }

 

        private ContractListFilterParms.FilterTypes GetSelectedFilterType()
        {
            ContractListFilterParms.FilterTypes selectedFilterType = ContractListFilterParms.FilterTypes.None;

            ListItem selectedItem = SearchDropDownList.SelectedItem;
            if( selectedItem != null )
            {
                if( selectedItem.Value.CompareTo( Enum.GetName( typeof( ContractListFilterParms.FilterTypes ), ContractListFilterParms.FilterTypes.ContractingOfficer ) ) == 0 )
                {
                    selectedFilterType = ContractListFilterParms.FilterTypes.ContractingOfficer;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( ContractListFilterParms.FilterTypes ), ContractListFilterParms.FilterTypes.ContractNumber ) ) == 0 )
                {
                    selectedFilterType = ContractListFilterParms.FilterTypes.ContractNumber;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( ContractListFilterParms.FilterTypes ), ContractListFilterParms.FilterTypes.Description ) ) == 0 )
                {
                    selectedFilterType = ContractListFilterParms.FilterTypes.Description;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( ContractListFilterParms.FilterTypes ), ContractListFilterParms.FilterTypes.Schedule ) ) == 0 )
                {
                    selectedFilterType = ContractListFilterParms.FilterTypes.Schedule;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( ContractListFilterParms.FilterTypes ), ContractListFilterParms.FilterTypes.Vendor ) ) == 0 )
                {
                    selectedFilterType = ContractListFilterParms.FilterTypes.Vendor;
                }
            }

            return( selectedFilterType );
        }

        private string GetSelectedFilterValue()
        {
            string selectedFilterValue = "";

            selectedFilterValue = ContractSelectFilterValueTextBox.Text;

            return ( selectedFilterValue );
        }

        private void ClearFilterValue()
        {
            ContractSelectFilterValueTextBox.Text = "";
            _contractListFilterParms.FilterValue = "";
        }

        protected void ContractSelectFilterButton_OnClick( object sender, EventArgs e )
        {
            GatherFilterValues( _contractListFilterParms, false );

            if( _contractListFilterParms.FilterValue.Contains( ";" ) == true )
            {
                MsgBox.AlertFromUpdatePanel( Page, "Semi Colons are not allowed as a filter criteria." );

                _contractListFilterParms.FilterValue.Replace( ";", "" );
            }

            if( _contractListFilterParms.ContractOwnerFilter == ContractListFilterParms.ContractOwnerFilters.All &&
                _contractListFilterParms.ContractStatusFilter == ContractListFilterParms.ContractStatusFilters.All )
            {
                LoadContractHeaders( _contractListFilterParms, false );
            }
            else
            {
                FilterContractHeaders( _contractListFilterParms );
            }

            BindContractSelectGrid();

            SetContractSelectGridViewSelectedItem( 0, true );

            HighlightContractHeaderRow( 0 );
        }

        protected void ContractSelectViewAllButton_OnClick( object sender, EventArgs e )
        {
            ClearFilterValue();

            GatherFilterValues( _contractListFilterParms, true );

            LoadContractHeaders( _contractListFilterParms, false );

            BindContractSelectGrid();

            SetContractSelectGridViewSelectedItem( 0, true );

            HighlightContractHeaderRow( 0 );
        }

        protected void ContractSelectGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            string sortColumn = "";

            if( e.CommandName.CompareTo( "Sort" ) == 0 )
            {                                                           
                sortColumn = e.CommandArgument.ToString();
                Sort( sortColumn );
            }

            // can trap editing commands here if needed, prior to individual editing events
        }

        private void SetCurrentDocument( int selectedContractId, string selectedContractNumber, int scheduleNumber )
        {
            CurrentDocument currentDocument = null;
            currentDocument = new CurrentDocument( selectedContractId, selectedContractNumber, scheduleNumber, ( ContractDB )Session[ "ContractDB" ], ( DrugItemDB )Session[ "DrugItemDB" ], ( ItemDB )Session[ "ItemDB" ] );
            Session[ "CurrentDocument" ] = currentDocument;
            currentDocument.LookupCurrentDocument();

            BrowserSecurity2 browserSecurity = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            browserSecurity.SetDocumentEditStatus( currentDocument );
        }

        private void SetContractSelectGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {
            // save for postback
            Session[ "ContractSelectGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            ContractSelectGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            if( bIncludeScroll == true )
                ScrollToSelectedItem();

            // allow the update postback to occur
            ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );  
        }

        protected void ScrollToSelectedItem()
        {
            int rowIndex = ContractSelectGridView.SelectedIndex;
            int fudge;

            //if( rowIndex > 2000 )
                fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )50.0 );
            //if( rowIndex > 1000 )
            //    fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )96.0 );
            //else
            //    fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )97.0 );

            int rowPosition = ( CONTRACTHEADERGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( CONTRACTHEADERGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
        }

        protected void HighlightContractHeaderRow( int itemIndex )
        {
            int highlightedRowIndex = itemIndex + 1;

            if( ContractSelectGridView.HasData() == true )
            {
                if( itemIndex < ContractSelectGridView.Rows.Count )
                {
                    string rowColor = "alt";
                    int odd = 0;
                    Math.DivRem( highlightedRowIndex, 2, out odd );
                    if( odd > 0 )
                    {
                        rowColor = "norm";
                    }

                    string setContractHighlightedRowIndexAndOriginalColorScript = String.Format( "setContractHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, rowColor );
                    ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveItemHighlightingScript", setContractHighlightedRowIndexAndOriginalColorScript, true );

                    // allow the highlight postback to occur 
                    ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );  
                }
            }
        }

        private void RestoreContractSelectGridViewSelectedItem()
        {
            if( Session[ "ContractSelectGridViewSelectedIndex" ] != null )
            {
                int selectedIndex = ( int )Session[ "ContractSelectGridViewSelectedIndex" ];
                ContractSelectGridView.SelectedIndex = selectedIndex;
            }
        }

        // could get called as a result of copy contract or similar contract header operation
        // rebind is true if caller has altered the contract list
        private void RefreshContractSelectScreenDueToDetailsChanged( bool bRebindItems )
        {
            int currentItemIndex = ContractSelectGridView.SelectedIndex;
            int adjustedCurrentItemIndex = currentItemIndex;

            if( bRebindItems == true )
            {
                // adjust the current item index $$$
                int offset = 0;
                if( Session[ "CopyContractCurrentRowOffset" ] != null )
                {
                    offset = ( int )Session[ "CopyContractCurrentRowOffset" ];
                    adjustedCurrentItemIndex += offset;

                    // if removing first item ( remove not currently implemented )
                    if( adjustedCurrentItemIndex < 0 )
                        adjustedCurrentItemIndex = 0;
                }

                SetContractSelectGridViewSelectedItem( adjustedCurrentItemIndex, false );

                BindContractSelectGrid();

            }

            ScrollToSelectedItem();
            HighlightContractHeaderRow( adjustedCurrentItemIndex );

        }

        private void OpenCopyContractWindow( int itemIndex, int selectedContractId, string contractNumber, string contractorName, string commodityCovered, int scheduleNumber )
        {
            Session[ "CopyContractWindowParms" ] = null;
            Session[ "CopyContractWindowParms" ] = new CopyContractWindowParms( itemIndex, selectedContractId, contractNumber, contractorName, commodityCovered, scheduleNumber );

            string windowOpenScript = "window.open('CopyContract.aspx','CopyContract','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=200,left=240,width=500,height=436, resizable=0');";
            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "CopyContractWindowOpenScript", windowOpenScript, true );
        }

        protected void ContractSelectGridView_OnSorting( object sender, GridViewSortEventArgs e )
        {
            Sort( e.SortExpression );
        }

        private void Sort( string sortExpression )
        {
            _contractListFilterParms.SortExpression = sortExpression;

            SetSortDirectionInFilterParms( sortExpression );

            GatherFilterValues( _contractListFilterParms, false );

            if( _contractListFilterParms.FilterValue.Contains( ";" ) == true )
            {
    //            MsgBox.AlertFromUpdatePanel( Page, "Semi Colons are not allow as a filter criteria." );
    //            return;
            }

            _contractListFilterParms.IsDirty = true;

            SortContractHeaders( _contractListFilterParms );

            BindContractSelectGrid();

            SetContractSelectGridViewSelectedItem( 0, true );


        }

        private void SortContractHeaders( ContractListFilterParms contractListFilterParms )
        {
            _contractHeaderDataView.Sort = string.Format( "{0} {1}", contractListFilterParms.SortExpression, ( _contractListFilterParms.SortDirection.CompareTo( "Ascending" ) == 0 ) ? "ASC" : "DESC" );
        }

        private void FilterContractHeaders( ContractListFilterParms contractListFilterParms )
        {
            _contractHeaderDataView.RowFilter = string.Format( "{0} like '%{1}%'", ContractListFilterParms.GetFieldNameFromFilterType( contractListFilterParms.FilterType ), _contractListFilterParms.FilterValue.Trim() );
        }

        private void SetSortDirectionInFilterParms( string requestedSortExpression )
        {
            if( requestedSortExpression.CompareTo( _contractListFilterParms.SortExpression ) == 0 )
            {
                if( _contractListFilterParms.SortDirection.CompareTo( "Ascending" ) == 0 )
                {
                    _contractListFilterParms.SortDirection = "Descending";
                }
                else
                {
                    _contractListFilterParms.SortDirection = "Ascending";
                }
            }
            else
            {
                _contractListFilterParms.SortDirection = "Ascending";
            }
        }

        protected void ContractSelectGridView_OnSorted( object sender, EventArgs e )
        {


        }

        protected void ContractSelectGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;

                if( e.Row.RowType == DataControlRowType.DataRow )
                {
                    Label selectContractButton = null;
                    selectContractButton = ( Label )e.Row.FindControl( "SelectContractButton" );
                    if( selectContractButton != null )
                    {
                        string contractNumber = ( string )dataRowView[ "CntrctNum" ];
                        int scheduleNumber = Int32.Parse( dataRowView[ "Schedule_Number" ].ToString() );

                        // colors match .DocumentSelectGridItems and  .DocumentSelectGridAltItems
                        string rowColor = "alt";
                        int odd = 0;
                        Math.DivRem( rowIndex, 2, out odd );
                        if( odd > 0 )
                        {
                            rowColor = "norm";
                        }

                        string windowOpenCommand = string.Format( "openContractDetailsWindow(this, {0}, '{1}', {2}, '{3}');", rowIndex, contractNumber, scheduleNumber, rowColor );
                        string cursorChangeToHand = string.Format( "contractNumberMouseChange(this, 'over', {0}, '{1}');", rowIndex, rowColor );
                        string cursorChangeToNormal = string.Format( "contractNumberMouseChange(this, 'out', {0}, '{1}');", rowIndex, rowColor );

                        selectContractButton.Attributes.Add( "onclick", windowOpenCommand );
                        selectContractButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        selectContractButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }


        public void CompletionDate_OnDataBinding( object sender, EventArgs e )
        {
            Label completionDateLabel = ( Label )sender;

            //  hide or format ContractCancellationDate label
            if( _contractListFilterParms.ContractStatusFilter != ContractListFilterParms.ContractStatusFilters.Active )
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

        protected void ContractSelectForm_OnInit( object sender, EventArgs e )
        {
            CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
            string screenSizeInfo = string.Format( "H:{0} W:{1}", cmGlobals.ClientScreenHeight, cmGlobals.ClientScreenWidth );
            UserActivity log = new UserActivity( UserActivity.ActionTypes.ScreenResolution, screenSizeInfo, UserActivity.ActionDetailsTypes.ScreenResolutionValues );
            log.LogUserActivity();
        }

        protected void ContractSelectForm_OnPreRender( object sender, EventArgs e )
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

        protected void ContractSelectScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "ContractSelectErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "ContractSelectErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            ScriptManager1.AsyncPostBackErrorMessage = errorMsg;
        }

    }
}

