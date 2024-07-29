using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class GridViewContextMenuEventArgs : CommandEventArgs
    {
        public GridViewContextMenuEventArgs( CommandEventArgs args ) 
            : base( args )
        {
        }
    }
}
