using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;

using Menu = System.Web.UI.WebControls.Menu;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ReportSearch : BaseMasterPage
    {
        public ContentPlaceHolder ReportTabs
        {
            get                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
            {
                return ( this.ReportSearchContentPlaceHolder ); 
            }
        }

        public UpdatePanelEventProxy ReportSearchMasterEventProxy
        {
            get
            {
                return ( this.ReportSearchMasterUpdatePanelEventProxy );
            }
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

            string showProgressIndicator = string.Format( "EnableProgressIndicator(true);" );
            ReportSearchMenu.OnClientClickScript = showProgressIndicator;      // Attributes.Add( "onclick", showProgressIndicator );
        }

        private void ClearSessionVariables()
        {

        }

      
        protected void ReportSearchMenu_EdgeMenuCommand( EdgeMenu theMenu, EdgeMenuCommandEventArgs args )
        {
            Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
            SetRedirected( true );
            ( ( BaseSearchPage )this.Page ).HideProgressIndicator();
            Response.Redirect( "~/ReportSelectBody.aspx" );
        }

        protected void SetRedirected( bool bIsRedirected )
        {
            MasterPage master = Page.Master;
            if( master != null )
            {
                MasterPage topMaster = master.Master;

                if( topMaster != null )
                {
                    ( ( NACCM )topMaster ).SetRedirected( bIsRedirected );
                }
            }
        }

        protected void ReportSearchMenu_OnInit( object sender, EventArgs e )
        {
            EdgeMenu theMenu = ( EdgeMenu )sender;
            theMenu.EdgeMenuCommand += ReportSearchMenu_EdgeMenuCommand;

            theMenu.AddItem( "Contracts", "Contracts" );
            theMenu.AddItem( "Fiscal", "Fiscal" );
            theMenu.AddItem( "Pharmaceutical", "Pharmaceutical" );
            theMenu.AddItem( "Sales", "Sales" );
            theMenu.AddItem( "SBA", "SBA" );           
        }

        protected void ReportSearchMenu_OnPreRender( object sender, EventArgs e )
        {
            EdgeMenu reportSearchMenu = ( EdgeMenu )sender;

            // re-highlight the selected menu item
            string menuItemValue = "";

            if( Session[ "CurrentSelectedLevel2MenuItemValue" ] != null )
            {
                menuItemValue = ( string )Session[ "CurrentSelectedLevel2MenuItemValue" ];
                foreach( EdgeMenuItem mi in reportSearchMenu.MenuItems )
                {
                    if( mi.ItemValue.Contains( menuItemValue ) == true )
                    {
                        mi.Highlighted = true;
                        break;
                    }
                }
            }
        }


        protected void ShowException( Exception ex )
        {
            MasterPage master = Page.Master;
            if( master != null )
            {
                MasterPage topMaster = master.Master;

                if( topMaster != null )
                {
                    MsgBox msgBox = ( MsgBox )topMaster.FindControl( "MsgBox" );

                    if( msgBox != null )
                    {
                        msgBox.ShowErrorFromUpdatePanelAsync( Page, ex );
                    }

                }
            }
        }
    }
}