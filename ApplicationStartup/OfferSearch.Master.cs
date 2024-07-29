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
    public partial class OfferSearch : BaseMasterPage
    {

        public global::VA.NAC.NACCMBrowser.BrowserObj.UpdatePanelEventProxy SearchMasterUpdatePanelEventProxy;
 
        private OfferListFilterParms _offerListFilterParms = null;

        private DataSet _offerHeaderDataSet = null;

        private DataView _offerHeaderDataView = null;

        public UpdatePanelEventProxy SearchMasterEventProxy
        {
            get
            {
                return ( this.SearchMasterUpdatePanelEventProxy );
            }
        }

        public OfferListFilterParms OfferListFilterParms
        {
            get { return _offerListFilterParms; }
            set { _offerListFilterParms = value; }
        }

        public DataSet OfferHeaderDataSet
        {
            get { return _offerHeaderDataSet; }
            set { _offerHeaderDataSet = value; }
        }

        public DataView OfferHeaderDataView
        {
            get { return _offerHeaderDataView; }
            set { _offerHeaderDataView = value; }
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
        
            AddProgressIndicatorToCheckboxClickEvent( ActiveOffersCheckBox );
            AddProgressIndicatorToCheckboxClickEvent( ExpiredOffersCheckBox );
            AddProgressIndicatorToCheckboxClickEvent( MyOffersCheckBox );

            if( Page.IsPostBack == true )
            {
                string eTarget = Request.Params[ "__EVENTTARGET" ].ToString();
                string eArgument = Request.Params[ "__EVENTARGUMENT" ].ToString();
                if( eTarget.ToString().Contains( "Expired" ) == true )
                {
                    ExpiredOffersCheckBox_OnCheckedChanged( ExpiredOffersCheckBox, new EventArgs() );
                }
                else if( eTarget.ToString().Contains( "Active" ) == true )
                {                    
                    ActiveOffersCheckBox_OnCheckedChanged( ActiveOffersCheckBox, new EventArgs() );
                }
                //else if( eTarget.ToString().Contains( "MyOffers" ) == true )
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
            if( Session[ "OfferListFilterParms" ] == null )
            {
                _offerListFilterParms = new OfferListFilterParms( 2 );
                Session[ "OfferListFilterParms" ] = _offerListFilterParms;

                _offerListFilterParms.SortExpression = "Contractor_Name"; // first load is sorted by this field
                _offerListFilterParms.SortDirection = "Ascending";
                _offerListFilterParms.IsSecondaryFilterDirty = true;
            }
            else
            {
                _offerListFilterParms = ( OfferListFilterParms )Session[ "OfferListFilterParms" ];
            }
        }

        private OfferListFilterParms.FilterTypes GetSelectedFilterType()
        {
            OfferListFilterParms.FilterTypes selectedFilterType = OfferListFilterParms.FilterTypes.None;

            ListItem selectedItem = SearchTypeDropDownList.SelectedItem;
            if( selectedItem != null )
            {
                if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.ContractingOfficer ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.ContractingOfficer;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.OfferNumber ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.OfferNumber;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.Schedule ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.Schedule;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.Vendor ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.Vendor;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.Status ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.Status;
                }
                else if( selectedItem.Value.CompareTo( Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.ExtendsContractNumber ) ) == 0 )
                {
                    selectedFilterType = OfferListFilterParms.FilterTypes.ExtendsContractNumber;
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
            SearchTypeDropDownList.SelectedValue = Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), OfferListFilterParms.FilterTypes.OfferNumber );

            SearchFilterValueTextBox.Text = "";

            ActiveOffersCheckBox.Checked = false;
            ExpiredOffersCheckBox.Checked = false;
            MyOffersCheckBox.Checked = false;
        }

        // if filtering with no checkboxes selected, then force selection of active
        private void AdjustCheckBoxesForFilter()
        {
            if( ActiveOffersCheckBox.Checked == false && ExpiredOffersCheckBox.Checked == false )
            {
                ActiveOffersCheckBox.Checked = true;  
            }
        }

        private OfferListFilterParms.OfferStatusFilters GetOfferStatusFilter()
        {
            if( ActiveOffersCheckBox.Checked == true && ExpiredOffersCheckBox.Checked == true )
            {
                return( OfferListFilterParms.OfferStatusFilters.All );
            }
            else if( ActiveOffersCheckBox.Checked == true )
            {
                return( OfferListFilterParms.OfferStatusFilters.Open );
            }
            else if( ExpiredOffersCheckBox.Checked == true )
            {
                return ( OfferListFilterParms.OfferStatusFilters.Completed );
            }
            else
            {
                return ( OfferListFilterParms.OfferStatusFilters.None );
            }
        }

        private OfferListFilterParms.OfferOwnerFilters GetOfferOwnerFilter()
        {
            if( MyOffersCheckBox.Checked == true )
            {
                return ( OfferListFilterParms.OfferOwnerFilters.Mine );
            }
            else
            {
                return ( OfferListFilterParms.OfferOwnerFilters.All );
            }

        }

        public void RestoreSearchFromFilterParms()
        {
            // even on redirect, may be first time visiting this page
            if( _offerListFilterParms != null )
            {
                if( _offerListFilterParms.OfferOwnerFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferOwnerFilters.Mine )
                {
                    MyOffersCheckBox.Checked = true;
                }
                else
                {
                    MyOffersCheckBox.Checked = false;
                }

                if( _offerListFilterParms.OfferStatusFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferStatusFilters.Open )
                {
                    ActiveOffersCheckBox.Checked = true;
                }
                else if( _offerListFilterParms.OfferStatusFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferStatusFilters.Completed )
                {
                    ExpiredOffersCheckBox.Checked = true;
                }

                SearchFilterValueTextBox.Text = _offerListFilterParms.FilterValue;

                SearchTypeDropDownList.SelectedValue = Enum.GetName( typeof( OfferListFilterParms.FilterTypes ), _offerListFilterParms.FilterType );

                // mark as dirty to force load after restore
                _offerListFilterParms.IsPrimaryFilterDirty = true;
                _offerListFilterParms.IsSecondaryFilterDirty = true;
            }
        }

        public void GatherFilterValues( OfferListFilterParms offerListFilterParms )
        {
            OfferListFilterParms.FilterTypes selectedFilterType = OfferListFilterParms.FilterTypes.None;
            OfferListFilterParms.OfferStatusFilters selectedOfferStatusFilter = OfferListFilterParms.OfferStatusFilters.None;
            OfferListFilterParms.OfferOwnerFilters selectedOfferOwnerFilter = OfferListFilterParms.OfferOwnerFilters.None;

            string selectedFilterValue = "";

            selectedFilterType = GetSelectedFilterType();
            selectedFilterValue = GetSelectedFilterValue();
            selectedOfferStatusFilter = GetOfferStatusFilter();
            selectedOfferOwnerFilter = GetOfferOwnerFilter();
 
            // fix for both checkboxes being selected - alternative is to turn off autopostback and require filter button click
            if( selectedOfferStatusFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferStatusFilters.All &&
                offerListFilterParms.EventTargetControlName.Contains( "ActiveOffersCheckBox" ) == true )
            {
                selectedOfferStatusFilter = NACCMBrowser.BrowserObj.OfferListFilterParms.OfferStatusFilters.Open;
            }
            else if( selectedOfferStatusFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferStatusFilters.All &&
                offerListFilterParms.EventTargetControlName.Contains( "ExpiredOffersCheckBox" ) == true )
            {
                selectedOfferStatusFilter = NACCMBrowser.BrowserObj.OfferListFilterParms.OfferStatusFilters.Completed;
            }

            _offerListFilterParms.IsPrimaryFilterDirty = false;
            _offerListFilterParms.IsSecondaryFilterDirty = false;

            if( _offerListFilterParms.FilterType != selectedFilterType ||
                _offerListFilterParms.FilterValue != selectedFilterValue )
            {
                _offerListFilterParms.IsSecondaryFilterDirty = true;  // the parameters have changed from the last load
                _offerListFilterParms.FilterType = selectedFilterType;
                _offerListFilterParms.FilterValue = selectedFilterValue;
            }
            
            if( _offerListFilterParms.OfferStatusFilter != selectedOfferStatusFilter )
            {
                _offerListFilterParms.IsPrimaryFilterDirty = true;
                _offerListFilterParms.OfferStatusFilter = selectedOfferStatusFilter;
            }
            
            if( _offerListFilterParms.OfferOwnerFilter != selectedOfferOwnerFilter )
            {
                _offerListFilterParms.IsPrimaryFilterDirty = true;
                _offerListFilterParms.OfferOwnerFilter = selectedOfferOwnerFilter;
            }
        }

        protected ContentPlaceHolder GetOfferSearchContentPlaceHolder()
        {
             ContentPlaceHolder offerSearchContentPlaceHolder = null;

             MasterPage master = Page.Master;
             if( master != null )
             {
                 MasterPage topMaster = master.Master;

                 if( topMaster != null )
                 {
                     offerSearchContentPlaceHolder = ( ContentPlaceHolder )CMGlobals.FindControlRecursive( topMaster, "OfferSearchContentPlaceHolder" );
                 }
             }   
             return( offerSearchContentPlaceHolder );
        }

        protected void SetSearchGridViewSelectedItem( int index, bool bIncludeScroll )
        {
            ContentPlaceHolder offerSelectBodyPlaceHolder = GetOfferSearchContentPlaceHolder();
            ( offerSelectBodyPlaceHolder.Page as OfferSelectBody ).SetSearchGridViewSelectedItem( index, bIncludeScroll );
        }

        protected void HighlightOfferHeaderRow( int index )
        {
            ContentPlaceHolder offerSelectBodyPlaceHolder = GetOfferSearchContentPlaceHolder();
            ( offerSelectBodyPlaceHolder.Page as OfferSelectBody ).HighlightOfferHeaderRow( index );
        }

        protected void SearchFilterButton_OnClick( object sender, EventArgs e )
        {
            _offerListFilterParms.EventTargetControlName = "SearchFilterButton";

            AdjustCheckBoxesForFilter();

            GatherFilterValues( _offerListFilterParms );

            if( _offerListFilterParms.FilterValue.Contains( ";" ) == true )
            {
                MsgBox.AlertFromUpdatePanel( Page, "Semi Colons are not allowed as a filter criteria." );

                _offerListFilterParms.FilterValue.Replace( ";", "" );
            }

            LoadOfferHeaders2( _offerListFilterParms );

            BindSearchGrid();

            SetSearchGridViewSelectedItem( 0, true );

            HighlightOfferHeaderRow( 0 );

            SearchMasterEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ClearFilterButton_OnClick( object sender, EventArgs e )
        {
            ClearFilterValues();

            GatherFilterValues( _offerListFilterParms );

            LoadOfferHeaders2( _offerListFilterParms );

            BindSearchGrid();

            SetSearchGridViewSelectedItem( 0, true );

            HighlightOfferHeaderRow( 0 );

            SearchMasterEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void SearchHeaderPanel_OnPreRender( object sender, EventArgs e )
        {
 
        }

        private void FilterOfferHeaders( OfferListFilterParms offerListFilterParms )
        {
            if( offerListFilterParms.SortExpression.Trim().Length > 0 )
            {
                if( offerListFilterParms.SortDirection.CompareTo( "Descending" ) == 0 )
                {
                    _offerHeaderDataView.Sort = string.Format( "{0} {1}", offerListFilterParms.SortExpression.Trim(), "DESC" );
                }
                else
                {
                    _offerHeaderDataView.Sort = string.Format( "{0} {1}", offerListFilterParms.SortExpression.Trim(), "ASC" );
                }
            }

            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            
            StringBuilder rowFilter = new StringBuilder( 300 ); // filter value can be 200
            if( offerListFilterParms.OfferOwnerFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferOwnerFilters.Mine || _offerListFilterParms.FilterValue.Trim().Length > 0 )
            {
                if( offerListFilterParms.OfferOwnerFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferOwnerFilters.Mine )
                {
                    rowFilter.AppendFormat( "CO_ID = {0}", bs.UserInfo.OldUserId );
                }

                if( offerListFilterParms.OfferOwnerFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferOwnerFilters.Mine && _offerListFilterParms.FilterValue.Trim().Length > 0 )
                {
                    rowFilter.Append( " AND " );
                }

                if( _offerListFilterParms.FilterValue.Replace( "'", " " ).Trim().Length > 0 )
                {
                    rowFilter.AppendFormat( "{0} like '%{1}%'", OfferListFilterParms.GetFieldNameFromFilterType( offerListFilterParms.FilterType ), _offerListFilterParms.FilterValue.Replace( "'", " " ).Trim() );
                }

                _offerHeaderDataView.RowFilter = rowFilter.ToString();
            }
        }

        // this session variable is also set when a offer's changes are saved
        public void SetOfferSearchDataDirtyFlag( bool bIsDirty )
        {
            Session[ "OfferSearchDataDirtyFlag" ] = bIsDirty;
        }

        public bool GetOfferSearchDataDirtyFlag()
        {
            if( Session[ "OfferSearchDataDirtyFlag" ] == null )
                return false;
            else
                return ( ( bool )Session[ "OfferSearchDataDirtyFlag" ] );
        }

        // loads the cache for the offer search screen
        public void LoadOfferHeaders( OfferListFilterParms offerListFilterParms )
        {
            bool bSuccess = false;

            string cacheName = "";

            if( offerListFilterParms.OfferStatusFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferStatusFilters.Open )
            {
                cacheName = "ActiveOfferHeaderDataSet";
            }
            else if( offerListFilterParms.OfferStatusFilter == NACCMBrowser.BrowserObj.OfferListFilterParms.OfferStatusFilters.Completed )
            {
                cacheName = "ExpiredOfferHeaderDataSet";
            }
            else
            {
                cacheName = "EmptyOfferHeaderDataSet";
            }

            _offerHeaderDataSet = null;
            _offerHeaderDataView = null;

            if( Cache[ cacheName ] != null )
            {
                _offerHeaderDataSet = ( DataSet )Cache[ cacheName ];
            }
            else
            {
                OfferDB offerDB = ( OfferDB )Session[ "OfferDB" ];
                if( offerDB != null )
                {
                    offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    offerDB.MakeConnectionString();

                    // note that owner, filter and sort info are considered in dataview
                    // thus, related parms are defaulted here to include all rows
                    // the SP is not changed to maintain backward compatibility with release 1
                    //if( Config.ItemVersion.CompareTo( "I2" ) == 0 )
                    //{ $$$$$
                        bSuccess = offerDB.SelectOfferHeaders2( ref _offerHeaderDataSet, OfferListFilterParms.GetStringFromOfferStatusFilter( _offerListFilterParms.OfferStatusFilter ), OfferListFilterParms.GetStringFromOfferOwnerFilter( OfferListFilterParms.OfferOwnerFilters.All ), OfferListFilterParms.GetStringFromFilterType( OfferListFilterParms.FilterTypes.None ), "", "", "" );
                    //}
                    //else
                    //{
                    //    bSuccess = offerDB.SelectOfferHeaders( ref _offerHeaderDataSet, OfferListFilterParms.GetStringFromOfferStatusFilter( _offerListFilterParms.OfferStatusFilter ), OfferListFilterParms.GetStringFromOfferOwnerFilter( OfferListFilterParms.OfferOwnerFilters.All ), OfferListFilterParms.GetStringFromFilterType( OfferListFilterParms.FilterTypes.None ), "", "", "" );
                    //}

                    if( bSuccess == false )
                    {
                        MsgBox.AlertFromUpdatePanel( Page, offerDB.ErrorMessage );
                    }
                    else
                    {
                        CMGlobals.CreateDataSetCache( this.Page, cacheName, _offerHeaderDataSet );

                        SetOfferSearchDataDirtyFlag( false );
                    }            
                }
            }

            _offerHeaderDataView = new DataView( _offerHeaderDataSet.Tables[ 0 ] );

            // avoid filtering for no reason to improve performance
            if( cacheName.CompareTo( "EmptyOfferHeaderDataSet" ) != 0 )
            {
                FilterOfferHeaders( _offerListFilterParms );
            }

            GetSearchGridView().DataSource = _offerHeaderDataView;
        }

        // loads the offer search screen, passing all parms to SP, no cache
        public void LoadOfferHeaders2( OfferListFilterParms offerListFilterParms )
        {
            bool bSuccess = false;

            _offerHeaderDataSet = null;
            _offerHeaderDataView = null;

            OfferDB offerDB = ( OfferDB )Session[ "OfferDB" ];
            if( offerDB != null )
            {
                offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                offerDB.MakeConnectionString();

                //if( Config.ItemVersion.CompareTo( "I2" ) == 0 )
                //{  $$$$$
                    bSuccess = offerDB.SelectOfferHeaders2( ref _offerHeaderDataSet, OfferListFilterParms.GetStringFromOfferStatusFilter( offerListFilterParms.OfferStatusFilter ), OfferListFilterParms.GetStringFromOfferOwnerFilter( offerListFilterParms.OfferOwnerFilter ), OfferListFilterParms.GetStringFromFilterType( offerListFilterParms.FilterType ), offerListFilterParms.FilterValue, offerListFilterParms.SortExpression, offerListFilterParms.SortDirection );
                //}
                //else
                //{
                //    bSuccess = offerDB.SelectOfferHeaders( ref _offerHeaderDataSet, OfferListFilterParms.GetStringFromOfferStatusFilter( offerListFilterParms.OfferStatusFilter ), OfferListFilterParms.GetStringFromOfferOwnerFilter( offerListFilterParms.OfferOwnerFilter ), OfferListFilterParms.GetStringFromFilterType( offerListFilterParms.FilterType ), offerListFilterParms.FilterValue, offerListFilterParms.SortExpression, offerListFilterParms.SortDirection );
                //}

                if( bSuccess == false )
                {
                    MsgBox.AlertFromUpdatePanel( Page, offerDB.ErrorMessage );
                }
                else
                {
                    SetOfferSearchDataDirtyFlag( false );   //$$$ may not be needed if not caching
                }
            }
           
            _offerHeaderDataView = new DataView( _offerHeaderDataSet.Tables[ 0 ] );
          
            GetSearchGridView().DataSource = _offerHeaderDataView;
        }

        private GridView GetSearchGridView()
        {
            ContentPlaceHolder offerSearchContentPlaceHolder = GetOfferSearchContentPlaceHolder();
            return ( ( GridView )offerSearchContentPlaceHolder.FindControl( "SearchGridView" ) );
        }

        private void BindSearchGrid()
        {
            try
            {
                ContentPlaceHolder offerSelectBodyPlaceHolder = GetOfferSearchContentPlaceHolder();
                // bind
                ( offerSelectBodyPlaceHolder.Page as OfferSelectBody ).BindSearchGrid();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }

        protected void ActiveOffersCheckBox_OnCheckedChanged( object sender, EventArgs e )
        {
            CheckBox activeOffersCheckBox = ( CheckBox )sender;
            CheckBox expiredOffersCheckBox;

            ContentPlaceHolder contentPlaceHolderMain;

            if( activeOffersCheckBox != null )
            {

                contentPlaceHolderMain = ( ContentPlaceHolder )activeOffersCheckBox.NamingContainer;
                if( contentPlaceHolderMain != null )
                {
                    expiredOffersCheckBox = ( CheckBox )contentPlaceHolderMain.FindControl( "ExpiredOffersCheckBox" );

                    if( expiredOffersCheckBox != null )
                    {
                        if( activeOffersCheckBox.Checked == true )
                        {
                            expiredOffersCheckBox.Checked = false;
                            
                        }

                        SearchMasterEventProxy.InvokeEvent( new EventArgs() );
                    }
                }
            }
        }

        protected void ExpiredOffersCheckBox_OnCheckedChanged( object sender, EventArgs e )
        {
            CheckBox expiredOffersCheckBox = ( CheckBox )sender;
            CheckBox activeOffersCheckBox;

            ContentPlaceHolder contentPlaceHolderMain;

            if( expiredOffersCheckBox != null )
            {
                contentPlaceHolderMain = ( ContentPlaceHolder )expiredOffersCheckBox.NamingContainer;
                if( contentPlaceHolderMain != null )
                {
                    activeOffersCheckBox = ( CheckBox )contentPlaceHolderMain.FindControl( "ActiveOffersCheckBox" );

                    if( activeOffersCheckBox != null )
                    {
                        if( expiredOffersCheckBox.Checked == true )
                        {
                            activeOffersCheckBox.Checked = false;
                            
                        }

                        SearchMasterEventProxy.InvokeEvent( new EventArgs() );
                    }
                }
            }
        }
    }
}