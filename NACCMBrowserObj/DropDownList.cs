using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;

using AjaxControlToolkit;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public class DropDownList : System.Web.UI.WebControls.DropDownList
    {

        public DropDownList()
        {

        }

        protected override void Render( HtmlTextWriter writer )
        {

            //int itemCount = ( ( DropDownList )this ).Items.Count;

            //for( int i = 0; i < itemCount; i++ )
            //{
            //    this.Items[ i ].Attributes.CssStyle.Add( HtmlTextWriterStyle.BorderWidth, "0px" );
            //    this.Items[ i ].Attributes.CssStyle.Add( HtmlTextWriterStyle.Margin, "0px" );
            //    this.Items[ i ].Attributes.CssStyle.Add( HtmlTextWriterStyle.Padding, "0 0 0px" );
            //    this.Items[ i ].Attributes.CssStyle.Add( HtmlTextWriterStyle.Height, "18px" );
           
            //    //myItem.Attributes.Add("style","background-color:#111111");
            //}

            base.Render( writer );
        }
    }
}
