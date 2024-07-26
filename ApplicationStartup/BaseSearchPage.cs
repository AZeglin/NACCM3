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
using ListView = VA.NAC.NACCMBrowser.BrowserObj.ListView;

namespace VA.NAC.CM.ApplicationStartup
{
    public class BaseSearchPage : System.Web.UI.Page
    {

        public BaseSearchPage( SearchPageTypes searchPageType )
        {
            _searchPageType = searchPageType;
        }

        public enum SearchPageTypes
        {
            Contract,
            Offer,
            UserRecentDocuments,
            Reports,
            Undefined
        }

        private SearchPageTypes _searchPageType = SearchPageTypes.Undefined;


        //public UpdatePanelEventProxy SearchMasterEventProxy
        //{
        //    get
        //    {
        //        if( _searchPageType == SearchPageTypes.Contract )
        //            return ( ((( ContractSelectBody )this ).Master ).SearchMasterUpdatePanelEventProxy );
        //        else
        //            return ( null ); // $$$ replace with OfferSearch when ready
        //    }
        //}

        private string _errorMessage = "";

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
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

        public void HideProgressIndicator()
        {
            string hideProgressIndicatorScript = "HideProgressIndicator();";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "HideProgressIndicatorScript", hideProgressIndicatorScript, true ); // runs after controls established

            TriggerSearchMasterUpdatePanel();

        }

        public void HideProgressIndicator( Page containingPage )
        {
            string hideProgressIndicatorScript = "HideProgressIndicator();";
            ScriptManager.RegisterStartupScript( containingPage, containingPage.GetType(), "HideProgressIndicatorScript", hideProgressIndicatorScript, true ); // runs after controls established

            TriggerSearchMasterUpdatePanel();

        }

        public void TriggerSearchMasterUpdatePanel()
        {
            if( _searchPageType == SearchPageTypes.Contract )
            {
                ( ( ContractSearch )( this.Master ) ).SearchMasterEventProxy.InvokeEvent( new EventArgs() );
            }
            else if( _searchPageType == SearchPageTypes.Offer )
            {
                ( ( OfferSearch )( this.Master ) ).SearchMasterEventProxy.InvokeEvent( new EventArgs() );
            }
            else if( _searchPageType == SearchPageTypes.UserRecentDocuments )
            {
                ( ( UserRecentDocuments )( this.Master ) ).UserRecentDocumentsMasterEventProxy.InvokeEvent( new EventArgs() );
            }
            else if( _searchPageType == SearchPageTypes.Reports )
            {
                ( ( ReportSearch )( this.Master ) ).ReportSearchMasterEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        public void ClearErrors()
        {
            // validation errors
            _errorMessage = "";
        }


    }
}