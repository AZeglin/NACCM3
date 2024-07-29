using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class NCM : System.Web.UI.Page
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            Session[ "NACCMVersion" ] = "2";
            Response.Redirect( "~/Start.aspx" );
        }
    }
}