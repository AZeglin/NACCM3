using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class NACCMStartupErrorPage : System.Web.UI.Page
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            StringBuilder sbErrorMessage = new StringBuilder( 1000, 2000 );

            if( Session[ "StartupException" ] != null )
                sbErrorMessage.AppendFormat( "The following error was encountered during application startup: {0}", Session[ "StartupException" ].ToString() );
            
            if( Session[ "UnhandledException" ] != null )
                sbErrorMessage.AppendFormat( "The following error was encountered during request processing: {0}", Session[ "UnhandledException" ].ToString() );
            
            StartupErrorTextBox.Text = sbErrorMessage.ToString();
        }
    }
}