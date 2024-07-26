using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VA.NAC.CM.ApplicationStartup
{
    // a base class to unify the offerview master and the documentcreation master
    // however, note that document creation applies to contracts also and not just offers
    public class BaseMasterPage : System.Web.UI.MasterPage
    {
        protected virtual void Page_Init( object sender, EventArgs e )
        {
            // detect IE11
            string strAgent = Request.UserAgent;
            HttpBrowserCapabilities myBrowserCapabilities = Request.Browser;
            Int32 majorVersion = myBrowserCapabilities.MajorVersion;
            if( strAgent.Contains( "like Gecko" ) && strAgent.Contains( "Trident" ) && majorVersion == 11 )
            {
                Page.ClientTarget = "uplevel";
            }
        }
 
    }
}