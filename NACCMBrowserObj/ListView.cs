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
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public class ListView : System.Web.UI.WebControls.ListView
    {
        // control index will usually be zero as there will usually be only one control in a cell
        // if a cell is read only, it gets accessed differently
        public string GetStringValueFromSelectedControl( int selectedIndex, int controlIndex, string nestedTemplatedControlName )
        {
            string cellValue = string.Empty;

            if( selectedIndex >= 0 )
            {
               ListViewItem selectedItem = this.Items[ selectedIndex ];

               if( selectedItem != null )
                {
                    Control control = selectedItem.Controls[ controlIndex ];
                    if( control != null )
                    {
                        Type controlType = control.GetType();
                        string typeName = controlType.Name;
                        if( typeName.CompareTo( "LiteralControl" ) == 0 )
                        {
                            Control nestedControl = selectedItem.FindControl( nestedTemplatedControlName );
                            if( nestedControl != null )
                            {
                                cellValue = GetStringValueFromControl( nestedControl );
                            }
                        }
                        else
                        {
                            cellValue = GetStringValueFromControl( control );
                        }
                        
                    }
                }
            }
            return ( cellValue );
        }

        private string GetStringValueFromControl( Control control )
        {
            string cellValue = string.Empty;

            if( control != null )
            {
                Type controlType = control.GetType();
                string typeName = controlType.Name;

                if( typeName.CompareTo( "Label" ) == 0 )
                {
                    cellValue = ( ( Label )control ).Text;
                }
                else if( typeName.CompareTo( "TextBox" ) == 0 )
                {
                    cellValue = ( ( TextBox )control ).Text;
                }
                else if( typeName.CompareTo( "CheckBox" ) == 0 )
                {
                    cellValue = ( ( CheckBox )control ).Checked.ToString();
                }
                else if( typeName.CompareTo( "DropDownList" ) == 0 )
                {
                    cellValue = ( ( DropDownList )control ).SelectedValue.ToString();
                }
                else if( typeName.CompareTo( "ComboBox" ) == 0 )
                {
                    cellValue = ( ( ComboBox )control ).Text;    //SelectedValue.ToString();
                }
            }

            return ( cellValue );
        }
    }
}
