using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ContractSearch : BaseMasterPage
    {

        public global::VA.NAC.NACCMBrowser.BrowserObj.UpdatePanelEventProxy SearchMasterUpdatePanelEventProxy;
 
        private ContractListFilterParms _contractListFilterParms = null;

        private DataSet _contractHeaderDataSet = null;

        private DataView _contractHeaderDataView = null;

        public UpdatePanelEventProxy SearchMasterEventProxy
        {
            get
            {
                return ( this.SearchMasterUpdatePanelEventProxy );
            }
        }

        public ContractListFilterParms ContractListFilterParms
        {
            get { return _contractListFilterParms; }
            set { _contractListFilterParms = value; }
        }

        public DataSet ContractHeaderDataSet
        {
            get { return _contractHeaderDataSet; }
            set { _contractHeaderDataSet = value; }
        }

        public DataView ContractHeaderDataView
        {
            get { return _contractHeaderDataView; }
            set { _contractHeaderDataView = value; }
        }


        protected override void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }

            CreateFilterParameters();
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            CMGlobals.AddKeepAlive( this.Page, 12000 );

            AddProgressIndicatorToCheckboxClickEvent( ExpiredContractsCheckBox );
            AddProgressIndicatorToCheckboxClickEvent( ActiveContractsCheckBox );
            AddProgressIndicatorToCheckboxClickEvent( MyContractsCheckBox );

            if( Page.IsPostBack == true )
            {
                string eTarget = Request.Params[ "__EVENTTARGET" ].ToString();
                string eArgument = Request.Params[ "__EVENTARGUMENT" ].ToString();
                if( eTarget.ToString().Contains( "Expired" ) == true )
                {
                    ExpiredContractsCheckBox_OnCheckedChanged( ExpiredContractsCheckBox, new EventArgs() );
                }
                else if( eTarget.ToString().Contains( "Active" ) == true )
                {
                    ActiveContractsCheckBox_OnCheckedChanged( ActiveContractsCheckBox, new EventArgs() );
                }
                //else if( eTarget.ToString().Contains( "MyContracts" ) == true )
                //{

                //}


            }
        }

        // setting onclick for progress indicator blows away calls to server side click handler in rendered html, so compensate here...
        protected void AddProgressIndicatorToCheckboxClickEvent( CheckBox cb )
        {
            // $$$ delay was removed
            string cbUniqueID = cb.UniqueID;
            string showProgressIndicator = string.Format( "__doPostBack('{0}','');EnableProgressIndicator(true);", cbUniqueID );
            cb.Attributes.Add( "onclick", showProgressIndicator );
        }

        private void ClearSessionVariables()
        {
            
        }

        private void CreateFilterParameters()
        {
            if( Session[ "ContractListFilterParms" ] == null )
            {
                _contractListFilterParms = new ContractListFilterParms( 2 );
                Session[ "ContractListFilterParms" ] = _contractListFilterParms;

                _contractListFilterParms.SortExpression = "CntrctNum"; // first load is sorted by this field
                _contractListFilterParms.SortDirection = "Ascending";
                _contractListFilterParms.IsSecondaryFilterDirty = true;
            }
            else
            {
                _contractListFilterParms = ( ContractListFilterParms )Session[ "ContractListFilterParms" ];
            }
        }

        private ContractListFilterParms.FilterTypes GetSelectedFilterType()
        {
            ContractListFilterParms.FilterTypes selectedFilterType = ContractListFilterParms.FilterTypes.None;

            ListItem selectedItem = SearchTypeDropDownList.SelectedItem;
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

            return ( selectedFilterType );
        }

        private string GetSelectedFilterValue()
        {
            string selectedFilterValue = "";

            selectedFilterValue = SearchFilterValueTextBox.Text;

            return ( selectedFilterValue );
        }

        private void ClearFilterValues()
        {
            // default
            SearchTypeDropDownList.SelectedValue = Enum.GetName( typeof( ContractListFilterParms.FilterTypes ), ContractListFilterParms.FilterTypes.ContractNumber );

            SearchFilterValueTextBox.Text = "";

            ActiveContractsCheckBox.Checked = false;
            ExpiredContractsCheckBox.Checked = false;
            MyContractsCheckBox.Checked = false;
        }

        // if filtering with no checkboxes selected, then force selection of both
        private void AdjustCheckBoxesForFilter()
        {
            if( ActiveContractsCheckBox.Checked == false && ExpiredContractsCheckBox.Checked == false )
            {
                ActiveContractsCheckBox.Checked = true;
                ExpiredContractsCheckBox.Checked = true;
            }
        }

        private ContractListFilterParms.ContractStatusFilters GetContractStatusFilter()
        {
            if( ActiveContractsCheckBox.Checked == true && ExpiredContractsCheckBox.Checked == true )
            {
                return( ContractListFilterParms.ContractStatusFilters.All );
            }
            else if( ActiveContractsCheckBox.Checked == true )
            {
                return( ContractListFilterParms.ContractStatusFilters.Active );
            }
            else if( ExpiredContractsCheckBox.Checked == true )
            {
                return ( ContractListFilterParms.ContractStatusFilters.Closed );
            }
            else
            {
                return ( ContractListFilterParms.ContractStatusFilters.None );
            }
        }

        private ContractListFilterParms.ContractOwnerFilters GetContractOwnerFilter()
        {
            if( MyContractsCheckBox.Checked == true )
            {
                return ( ContractListFilterParms.ContractOwnerFilters.Mine );
            }
            else
            {
                return ( ContractListFilterParms.ContractOwnerFilters.All );
            }

        }

        public void RestoreSearchFromFilterParms()
        {
            // even on redirect, may be first time visiting this page
            if( _contractListFilterParms != null )
            {
                if( _contractListFilterParms.ContractOwnerFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractOwnerFilters.Mine )
                {
                    MyContractsCheckBox.Checked = true;
                }
                else
                {
                    MyContractsCheckBox.Checked = false;
                }

                if( _contractListFilterParms.ContractStatusFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.Active )
                {
                    ActiveContractsCheckBox.Checked = true;
                }
                else if( _contractListFilterParms.ContractStatusFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.Closed )
                {
                    ExpiredContractsCheckBox.Checked = true;
                }

                SearchFilterValueTextBox.Text = _contractListFilterParms.FilterValue;

                SearchTypeDropDownList.SelectedValue = Enum.GetName( typeof( ContractListFilterParms.FilterTypes ), _contractListFilterParms.FilterType );

                // mark as dirty to force load after restore
                _contractListFilterParms.IsPrimaryFilterDirty = true;
                _contractListFilterParms.IsSecondaryFilterDirty = true;
            }
        }

        // if active/expired are both clear when the filter button is clicked, then check them both before running the filter,
        // otherwise, go with what's there.
        public void GatherFilterValues( ContractListFilterParms contractListFilterParms )
        {
            ContractListFilterParms.FilterTypes selectedFilterType = ContractListFilterParms.FilterTypes.None;
            ContractListFilterParms.ContractStatusFilters selectedContractStatusFilter = ContractListFilterParms.ContractStatusFilters.None;
            ContractListFilterParms.ContractOwnerFilters selectedContractOwnerFilter = ContractListFilterParms.ContractOwnerFilters.None;

            string selectedFilterValue = "";

            selectedFilterType = GetSelectedFilterType();
            selectedFilterValue = GetSelectedFilterValue();
            selectedContractStatusFilter = GetContractStatusFilter();
            selectedContractOwnerFilter = GetContractOwnerFilter();
 
            // if filter button was just clicked
            if( contractListFilterParms.EventTargetControlName.CompareTo( "SearchFilterButton" ) == 0 )
            {
                // treat none ( or all ) as all
                if( selectedContractStatusFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.None )
                        selectedContractStatusFilter = NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.All;
            }
            else // either first load or checkbox was clicked
            {
                // fix for both checkboxes being selected - alternative is to turn off autopostback and require filter button click
                if( selectedContractStatusFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.All &&
                    contractListFilterParms.EventTargetControlName.Contains( "ActiveContractsCheckBox" ) == true )
                {
                    selectedContractStatusFilter = NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.Active;
                }
                else if( selectedContractStatusFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.All &&
                    contractListFilterParms.EventTargetControlName.Contains( "ExpiredContractsCheckBox" ) == true )
                {
                    selectedContractStatusFilter = NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.Closed;
                }
            }

            _contractListFilterParms.IsPrimaryFilterDirty = false;
            _contractListFilterParms.IsSecondaryFilterDirty = false;

            if( _contractListFilterParms.FilterType != selectedFilterType ||
                _contractListFilterParms.FilterValue != selectedFilterValue )
            {
                _contractListFilterParms.IsSecondaryFilterDirty = true;  // the parameters have changed from the last load
                _contractListFilterParms.FilterType = selectedFilterType;
                _contractListFilterParms.FilterValue = selectedFilterValue;
            }
            
            if( _contractListFilterParms.ContractStatusFilter != selectedContractStatusFilter )
            {
                _contractListFilterParms.IsPrimaryFilterDirty = true;
                _contractListFilterParms.ContractStatusFilter = selectedContractStatusFilter;
            }
            
            if( _contractListFilterParms.ContractOwnerFilter != selectedContractOwnerFilter )
            {
                _contractListFilterParms.IsPrimaryFilterDirty = true;
                _contractListFilterParms.ContractOwnerFilter = selectedContractOwnerFilter;
            }
        }

        protected ContentPlaceHolder GetContractSearchContentPlaceHolder()
        {
             ContentPlaceHolder contractSearchContentPlaceHolder = null;

             MasterPage master = Page.Master;
             if( master != null )
             {
                 MasterPage topMaster = master.Master;

                 if( topMaster != null )
                 {
                     contractSearchContentPlaceHolder = ( ContentPlaceHolder )CMGlobals.FindControlRecursive( topMaster, "ContractSearchContentPlaceHolder" );
                 }
             }   
             return( contractSearchContentPlaceHolder );
        }

        protected void SetSearchGridViewSelectedItem( int index, bool bIncludeScroll )
        {
            ContentPlaceHolder contractSelectBodyPlaceHolder = GetContractSearchContentPlaceHolder();
            ( contractSelectBodyPlaceHolder.Page as ContractSelectBody ).SetSearchGridViewSelectedItem( index, bIncludeScroll );
        }

        protected void HighlightContractHeaderRow( int index )
        {
            ContentPlaceHolder contractSelectBodyPlaceHolder = GetContractSearchContentPlaceHolder();
            ( contractSelectBodyPlaceHolder.Page as ContractSelectBody ).HighlightContractHeaderRow( index );
        }

        protected void SearchFilterButton_OnClick( object sender, EventArgs e )
        {
            _contractListFilterParms.EventTargetControlName = "SearchFilterButton";

            AdjustCheckBoxesForFilter();

            GatherFilterValues( _contractListFilterParms );

            if( _contractListFilterParms.FilterValue.Contains( ";" ) == true )
            {
                MsgBox.AlertFromUpdatePanel( Page, "Semi Colons are not allowed as a filter criteria." );

                _contractListFilterParms.FilterValue.Replace( ";", "" );
            }

            LoadContractHeaders2( _contractListFilterParms );

            BindSearchGrid();

            SetSearchGridViewSelectedItem( 0, true );

            HighlightContractHeaderRow( 0 );

            SearchMasterEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ClearFilterButton_OnClick( object sender, EventArgs e )
        {
            ClearFilterValues();

            GatherFilterValues( _contractListFilterParms );

            LoadContractHeaders2( _contractListFilterParms );

            BindSearchGrid();

            SetSearchGridViewSelectedItem( 0, true );

            HighlightContractHeaderRow( 0 );

            SearchMasterEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void SearchHeaderPanel_OnPreRender( object sender, EventArgs e )
        {
 
        }

        private void FilterContractHeaders( ContractListFilterParms contractListFilterParms )
        {
            if( contractListFilterParms.SortExpression.Trim().Length > 0 )
            {
                if( contractListFilterParms.SortDirection.CompareTo( "Descending" ) == 0 )
                {
                    _contractHeaderDataView.Sort = string.Format( "{0} {1}", contractListFilterParms.SortExpression.Trim(), "DESC" );
                }
                else
                {
                    _contractHeaderDataView.Sort = string.Format( "{0} {1}", contractListFilterParms.SortExpression.Trim(), "ASC" );
                }
            }

            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            
            StringBuilder rowFilter = new StringBuilder( 300 ); // filter value can be 200
            if( contractListFilterParms.ContractOwnerFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractOwnerFilters.Mine || _contractListFilterParms.FilterValue.Trim().Length > 0 )
            {
                if( contractListFilterParms.ContractOwnerFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractOwnerFilters.Mine )
                {
                    rowFilter.AppendFormat( "CO_ID = {0}", bs.UserInfo.OldUserId );
                }

                if( contractListFilterParms.ContractOwnerFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractOwnerFilters.Mine && _contractListFilterParms.FilterValue.Trim().Length > 0 )
                {
                    rowFilter.Append( " AND " );
                }

                if( _contractListFilterParms.FilterValue.Replace( "'", " " ).Trim().Length > 0 )
                {
                    rowFilter.AppendFormat( "{0} like '%{1}%'", ContractListFilterParms.GetFieldNameFromFilterType( contractListFilterParms.FilterType ), _contractListFilterParms.FilterValue.Replace( "'", " " ).Trim() );
                }

                _contractHeaderDataView.RowFilter = rowFilter.ToString();
            }
        }

        // this session variable is also set when a contract's changes are saved
        public void SetContractSearchDataDirtyFlag( bool bIsDirty )
        {
            Session[ "ContractSearchDataDirtyFlag" ] = bIsDirty;
        }

        public bool GetContractSearchDataDirtyFlag()
        {
            if( Session[ "ContractSearchDataDirtyFlag" ] == null )
                return false;
            else
                return ( ( bool )Session[ "ContractSearchDataDirtyFlag" ] );
        }

        // loads the cache for the contract search screen
        public void LoadContractHeaders( ContractListFilterParms contractListFilterParms )
        {
            bool bSuccess = false;

            string cacheName = "";

            if( contractListFilterParms.ContractStatusFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.Active )
            {
                cacheName = "ActiveContractHeaderDataSet";
            }
            else if( contractListFilterParms.ContractStatusFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.Closed )
            {
                cacheName = "ExpiredContractHeaderDataSet";
            }
            else if( contractListFilterParms.ContractStatusFilter == NACCMBrowser.BrowserObj.ContractListFilterParms.ContractStatusFilters.All )
            {
                cacheName = "AllContractHeaderDataSet";
            }
            else
            {
                cacheName = "EmptyContractHeaderDataSet";
            }

            _contractHeaderDataSet = null;
            _contractHeaderDataView = null;

            if( Cache[ cacheName ] != null )
            {
                _contractHeaderDataSet = ( DataSet )Cache[ cacheName ];
            }
            else
            {
                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                if( contractDB != null )
                {
                    contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    contractDB.MakeConnectionString();

                    // note that owner, filter and sort info are considered in dataview
                    // thus, related parms are defaulted here to include all rows
                    // the SP is not changed to maintain backward compatibility with release 1
                    bSuccess = contractDB.SelectContractHeaders( ref _contractHeaderDataSet, ContractListFilterParms.GetStringFromContractStatusFilter( _contractListFilterParms.ContractStatusFilter ), ContractListFilterParms.GetStringFromContractOwnerFilter( ContractListFilterParms.ContractOwnerFilters.All ), ContractListFilterParms.GetStringFromFilterType( ContractListFilterParms.FilterTypes.None ), "", "", "" );
                    if( bSuccess == false )
                    {
                        MsgBox.AlertFromUpdatePanel( Page, contractDB.ErrorMessage );
                    }
                    else
                    {
                        CMGlobals.CreateDataSetCache( this.Page, cacheName, _contractHeaderDataSet );

                        SetContractSearchDataDirtyFlag( false );
                    }            
                }
            }

            _contractHeaderDataView = new DataView( _contractHeaderDataSet.Tables[ 0 ] );

            // avoid filtering for no reason to improve performance
            if( cacheName.CompareTo( "EmptyContractHeaderDataSet" ) != 0 )
            {
                FilterContractHeaders( _contractListFilterParms );
            }

            GetSearchGridView().DataSource = _contractHeaderDataView;
        }

        // loads the contract search screen, all parms go to SP, no cache
        public void LoadContractHeaders2( ContractListFilterParms contractListFilterParms )
        {
            bool bSuccess = false;
          
            _contractHeaderDataSet = null;
            _contractHeaderDataView = null;
           
            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            if( contractDB != null )
            {
                contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                contractDB.MakeConnectionString();

                bSuccess = contractDB.SelectContractHeaders( ref _contractHeaderDataSet, ContractListFilterParms.GetStringFromContractStatusFilter( contractListFilterParms.ContractStatusFilter ), ContractListFilterParms.GetStringFromContractOwnerFilter( contractListFilterParms.ContractOwnerFilter ), ContractListFilterParms.GetStringFromFilterType( contractListFilterParms.FilterType ), contractListFilterParms.FilterValue, contractListFilterParms.SortExpression, contractListFilterParms.SortDirection ); 
                if( bSuccess == false )
                {
                    MsgBox.AlertFromUpdatePanel( Page, contractDB.ErrorMessage );
                }
                else
                {                        
                    SetContractSearchDataDirtyFlag( false );     // $$$ dirty flag may not be required TBD                 
                }
            }

            _contractHeaderDataView = new DataView( _contractHeaderDataSet.Tables[ 0 ] );  // $$$ this is throwing ( null dataset or null tables )
          
            GetSearchGridView().DataSource = _contractHeaderDataView;
        }

        private GridView GetSearchGridView()
        {
            ContentPlaceHolder contractSearchContentPlaceHolder = GetContractSearchContentPlaceHolder();
            return ( ( GridView )contractSearchContentPlaceHolder.FindControl( "SearchGridView" ) );
        }

        private void BindSearchGrid()
        {
            try
            {
                ContentPlaceHolder contractSelectBodyPlaceHolder = GetContractSearchContentPlaceHolder();
                // bind
                ( contractSelectBodyPlaceHolder.Page as ContractSelectBody ).BindSearchGrid();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }

        protected void ActiveContractsCheckBox_OnCheckedChanged( object sender, EventArgs e )
        {
            CheckBox activeContractsCheckBox = ( CheckBox )sender;
            CheckBox expiredContractsCheckBox;

            ContentPlaceHolder contentPlaceHolderMain;

            if( activeContractsCheckBox != null )
            {

                contentPlaceHolderMain = ( ContentPlaceHolder )activeContractsCheckBox.NamingContainer;
                if( contentPlaceHolderMain != null )
                {
                    expiredContractsCheckBox = ( CheckBox )contentPlaceHolderMain.FindControl( "ExpiredContractsCheckBox" );

                    if( expiredContractsCheckBox != null )
                    {
                        if( activeContractsCheckBox.Checked == true )
                        {
                            expiredContractsCheckBox.Checked = false;                            
                        }

                        SearchMasterEventProxy.InvokeEvent( new EventArgs() );   
                    }
                }
            }
        }

        protected void ExpiredContractsCheckBox_OnCheckedChanged( object sender, EventArgs e )
        {
            CheckBox expiredContractsCheckBox = ( CheckBox )sender;
            CheckBox activeContractsCheckBox;

            ContentPlaceHolder contentPlaceHolderMain;

            if( expiredContractsCheckBox != null )
            {
                contentPlaceHolderMain = ( ContentPlaceHolder )expiredContractsCheckBox.NamingContainer;
                if( contentPlaceHolderMain != null )
                {
                    activeContractsCheckBox = ( CheckBox )contentPlaceHolderMain.FindControl( "ActiveContractsCheckBox" );

                    if( activeContractsCheckBox != null )
                    {
                        if( expiredContractsCheckBox.Checked == true )
                        {
                            activeContractsCheckBox.Checked = false;                            
                        }

                        SearchMasterEventProxy.InvokeEvent( new EventArgs() );   
                    }
                }
            }
        }
    }
}