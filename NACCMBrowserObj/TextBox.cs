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
    // allow the maxlength property to work for multiline text boxes in NACCM
    [Themeable( true )]
    public class TextBox : System.Web.UI.WebControls.TextBox
    {
        public TextBox()
        {
           
        }

        protected override void Render( HtmlTextWriter writer )
        {
            if( this.TextMode == TextBoxMode.MultiLine
                && this.MaxLength > 0 )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Maxlength, this.MaxLength.ToString() );
            }

            base.Render( writer );
        }

        //[DefaultValue(false)]
        //[Category("Appearance")]
        //[Description("Read Only.")]
        //public override bool ReadOnly
        //{
        //    get
        //    {
        //        if( this.ViewState[ "BReadOnly" ] == null )
        //        {
        //            return ( false ); // default
        //        }
        //        else
        //        {
        //            return (( bool )this.ViewState[ "BReadOnly" ]);
        //        }
        //    }
        //    set
        //    {
        //        if( this.ReadOnly != value )
        //        {
        //            this.ViewState[ "BReadOnly" ] = value;
        //        }

        //        if( value == true )
        //        {
        //            //this.Attributes.Add( "ReadOnly", "true" );
        //            this.Enabled = true;
        //        }
        //        else
        //        {
        //            //this.Attributes.Remove( "ReadOnly" );
        //            this.Enabled = false;
        //        }
        //    }
        //}       
    }
}
