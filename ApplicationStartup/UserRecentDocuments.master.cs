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

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class UserRecentDocuments : BaseMasterPage
    {
        public UpdatePanelEventProxy UserRecentDocumentsMasterEventProxy
        {
            get
            {
                return ( this.UserRecentDocumentsMasterUpdatePanelEventProxy );
            }
        }

        private DataSet _userRecentDocumentsDataSet = null;

        public DataSet UserRecentDocumentDataSet
        {
            get { return _userRecentDocumentsDataSet; }
            set { _userRecentDocumentsDataSet = value; }
        }

        private DataView _userRecentDocumentDataView = null;

        public DataView UserRecentDocumentDataView
        {
          get { return _userRecentDocumentDataView; }
          set { _userRecentDocumentDataView = value; }
        }

        protected override void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            CMGlobals.AddKeepAlive( this.Page, 12000 );

            LoadUserRecentDocuments( 20 );

            AssignObjectDataSourceToHeader();

            BindHeader();

            BindUserRecentDocumentsGrid();

            SetUserRecentDocumentsGridViewSelectedItem( 0, true );

            HighlightUserRecentDocumentRow( 0 );
        }

        public void LoadUserRecentDocuments( int count )
        {
            bool bSuccess = false;

            _userRecentDocumentsDataSet = null;

 
            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            if( contractDB != null )
            {
                contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                contractDB.MakeConnectionString();

                bSuccess = contractDB.SelectUserRecentDocuments( ref _userRecentDocumentsDataSet, count );
                if( bSuccess == false )
                {
                    MsgBox.AlertFromUpdatePanel( Page, contractDB.ErrorMessage );
                }
            }

            _userRecentDocumentDataView = new DataView( _userRecentDocumentsDataSet.Tables[ 0 ] );
            GetUserRecentDocumentsGridView().DataSource = _userRecentDocumentDataView;
        }

  
        private void ClearSessionVariables()
        {

        }

        public void AssignObjectDataSourceToHeader()
        {
            UserRecentDocumentsMasterFormView.DataSource = _userRecentDocumentDataView;
            UserRecentDocumentsMasterFormView.DataKeyNames = new string[] { "UserPreferenceId" };
        }

        public void BindHeader()
        {
            UserRecentDocumentsMasterFormView.DataBind();
        }

        protected ContentPlaceHolder GetUserRecentDocumentsPlaceHolder()
        {
            ContentPlaceHolder userRecentDocumentsPlaceHolder = null;

            MasterPage master = Page.Master;
            if( master != null )
            {
                MasterPage topMaster = master.Master;

                if( topMaster != null )
                {
                    userRecentDocumentsPlaceHolder = ( ContentPlaceHolder )CMGlobals.FindControlRecursive( topMaster, "UserRecentDocumentsPlaceHolder" );
                }
            }
            return ( userRecentDocumentsPlaceHolder );
        }

        protected void SetUserRecentDocumentsGridViewSelectedItem( int index, bool bIncludeScroll )
        {
            ContentPlaceHolder userRecentDocumentsPlaceHolder = GetUserRecentDocumentsPlaceHolder();
            ( userRecentDocumentsPlaceHolder.Page as UserRecentDocumentsBody ).SetUserRecentDocumentsGridViewSelectedItem( index, bIncludeScroll );
        }

        protected void HighlightUserRecentDocumentRow( int index )
        {
            ContentPlaceHolder userRecentDocumentsPlaceHolder = GetUserRecentDocumentsPlaceHolder();
            ( userRecentDocumentsPlaceHolder.Page as UserRecentDocumentsBody ).HighlightUserRecentDocumentRow( index );
        }

        private GridView GetUserRecentDocumentsGridView()
        {
            ContentPlaceHolder userRecentDocumentsPlaceHolder = GetUserRecentDocumentsPlaceHolder();
            return ( ( GridView )userRecentDocumentsPlaceHolder.FindControl( "UserRecentDocumentsGridView" ) );
        }

        private void BindUserRecentDocumentsGrid()
        {
            try
            {
                ContentPlaceHolder userRecentDocumentsPlaceHolder = GetUserRecentDocumentsPlaceHolder();
                // bind
                ( userRecentDocumentsPlaceHolder.Page as UserRecentDocumentsBody ).BindUserRecentDocumentsGrid();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }

    }
}