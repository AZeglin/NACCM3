using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using VA.NAC.NACCMBrowser.BrowserObj;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class Old1NCM : System.Web.UI.Page
    {
        private BrowserSecurity2 _browserSecurity = null;
        
        protected void Page_Load( object sender, EventArgs e )
        {
            if( Page.IsPostBack != true )
            {
                _browserSecurity = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
                // retro
                Session[ "UserName" ] = _browserSecurity.UserInfo.LoginName;
                // in lieu of context
                Session["NACCMStartedProperly"] = "yes";
                Session[ "NACCMVersion" ] = "1";
                Response.Redirect( "CM_Home.aspx" );  // "ContractSelect.aspx?Status=Active&Owner=All" ); //
            }
        }

    }
}
