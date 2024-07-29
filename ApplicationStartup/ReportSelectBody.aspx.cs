using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using VA.NAC.ReportManager;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ReportSelectBody : BaseSearchPage
    {

        private ReportCollection.ReportCategories _selectedReportCategory = ReportCollection.ReportCategories.Undefined;
        private ReportCollection _reportCollection = new ReportCollection();
        private ArrayList _reportList = new ArrayList();

        public ReportSelectBody() 
            : base( SearchPageTypes.Reports )
        {

        }

        protected void Page_Load( object sender, EventArgs e )
        {
            if( Session[ "NACCMStartedProperly" ] == null || Request.UrlReferrer == null )
            {
                Response.Redirect( "~/Start.aspx" );
            }
            
            if( _reportCollection != null )
            {
                _reportCollection.Init();
            }

            if( Session[ "CurrentSelectedLevel2MenuItemValue" ] != null )
            {
                string menuItem = Session[ "CurrentSelectedLevel2MenuItemValue" ].ToString();

                if( menuItem.Length > 0 )
                {
                    _selectedReportCategory = ReportCollection.GetReportCategoryEnumFromName( menuItem );                   
                }

                LoadAndBindReportList();
            }            
        }

        private void LoadAndBindReportList()
        {
            LoadReports();

            if( Page.IsPostBack == false )
            {

                BindReportList();
            }
        }

        private void LoadReports()
        {
            try
            {
                _reportList = _reportCollection.GetReportList( this.Page, _selectedReportCategory );
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        private void BindReportList()
        {
            try
            {
                if( ReportListView != null )
                {
                    ReportListView.DataSource = _reportList;
                    ReportListView.DataKeyNames = new string[] { "ReportName" };
                    ReportListView.DataBind();
                }
                else
                {
                    ShowException( new Exception( "ReportListView was null" ) );
                }
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        protected void ReportListView_OnItemDataBound( object sender, ListViewItemEventArgs e )
        {
            try
            {                
                if( e.Item.ItemType == ListViewItemType.DataItem )
                {
                    Button reportSelectButton = null;
                    reportSelectButton = ( Button )e.Item.FindControl( "ReportSelectButton" );
                    if( reportSelectButton != null )
                    {
                        string cursorChangeToHand = string.Format( "reportNameMouseChange(this, 'over');" );
                        string cursorChangeToNormal = string.Format( "reportNameMouseChange(this, 'out');" );

                        reportSelectButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        reportSelectButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }
                }
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        protected void ReportSelectButton_OnCommand( Object sender, CommandEventArgs e )
        {
            string commandName = e.CommandName;

            string commandArguments = e.CommandArgument.ToString();
            string[] commandArgumentList = commandArguments.Split( new char[] { ',' } );

            int rowIndex = int.Parse( commandArgumentList[ 0 ].ToString() );
            int displayIndex = int.Parse( commandArgumentList[ 1 ].ToString() );
            string reportName = commandArgumentList[ 2 ].ToString();
            string reportExecutionPath = commandArgumentList[ 3 ].ToString();
           

            switch( commandName )
            {
                case "RunReport":
                    try
                    {
                        //Response.Redirect( reportExecutionPath );  // opens in same tab
                        // opens report in different tab
                        string reportWindowScript = string.Format( "window.open( '{0}', '_newtab' );", reportExecutionPath );
                        ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ReportWindowScript", reportWindowScript, true );
                    }
                    catch( Exception ex )
                    {
                        ShowException( ex );
                    }
                    break;
            }
        }
    }
}