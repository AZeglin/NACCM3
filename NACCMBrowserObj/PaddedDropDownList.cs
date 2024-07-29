using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;

using AjaxControlToolkit;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public class PaddedDropDownList : System.Web.UI.WebControls.DropDownList
    {
        protected override void Render( HtmlTextWriter writer )
        {
            foreach ( ListItem item in Items )
            {
                item.Text = item.Text.Replace( " ", HttpUtility.HtmlDecode( "&nbsp;" ));
            }
            base.Render( writer );
        }
    }
}
