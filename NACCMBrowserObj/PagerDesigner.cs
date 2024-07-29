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
    public class PagerDesigner : ControlDesigner
    {
        private Pager _pagerInstance;

        public PagerDesigner()
            : base()
        {
        }

        // initialize the control to render at design-time
        public override void Initialize( System.ComponentModel.IComponent component )
        {
            _pagerInstance = ( Pager )component;
            base.Initialize( component );
        }

        // return the HTML to display in the VS IDE
        public override string GetDesignTimeHtml()
        {
            // pseudo-rendering
            StringWriter swTemp = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter( swTemp );
            _pagerInstance.RenderControl( writer );
            writer.Close();
            swTemp.Close();

            // modify the xxx of the pager table
            Table pagerTable = ( Table )( _pagerInstance.Controls[ 0 ] ).Controls[ 0 ];
            TableRow row = pagerTable.Rows[ 0 ];
            

            StringWriter sw = new StringWriter();
            writer.InnerWriter = sw;
      //      pagerTable.RenderControl( writer );           

            return( sw.ToString() );
        }
    }
}
