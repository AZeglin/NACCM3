using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.Design;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public class EdgeMenuDesigner : ControlDesigner
    {
        private EdgeMenu _menuInstance;

        public EdgeMenuDesigner()
            : base()
        {
        }

        // initialize the control to render at design-time
        public override void Initialize( System.ComponentModel.IComponent component )
        {
            _menuInstance = ( EdgeMenu )component;
            base.Initialize( component );
        }

        // return the HTML to display in the VS IDE
        public override string GetDesignTimeHtml()
        {
            // pseudo-rendering
            StringWriter swTemp = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter( swTemp );
            _menuInstance.RenderControl( writer );
            writer.Close();
            swTemp.Close();

            // modify the xxx of the menu table
            Table menuTable = ( Table )( _menuInstance.Controls[ 0 ] ).Controls[ 0 ];
            TableRow row = menuTable.Rows[ 0 ];
            

            StringWriter sw = new StringWriter();
            writer.InnerWriter = sw;
            //      menuTable.RenderControl( writer );           

            return ( sw.ToString() );
        }
    }
}
