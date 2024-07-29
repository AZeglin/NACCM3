using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public class UpdatePanelEventProxy : Control, IPostBackEventHandler
    {
        public UpdatePanelEventProxy()
        {
        }

        public void RaisePostBackEvent( string eventArgument )
        {
        }

        public event EventHandler<EventArgs> ProxiedEvent;

        protected virtual void OnProxiedEvent( EventArgs e )
        {
            if( ProxiedEvent != null )
            {
                ProxiedEvent( this, e );
            }
        }

        public void InvokeEvent( EventArgs e )
        {
            OnProxiedEvent( e );
        }
    }
}
