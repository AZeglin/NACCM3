using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Collections;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class EdgeMenuCommandEventArgs : EventArgs
    {
        private string _commandName = "";
       
        public string CommandName
        {
            get 
            { 
                return _commandName; 
            }
            set 
            { 
                _commandName = value; 
            }
        }

        private string _selectedMenuItemValue = "";

        public string SelectedMenuItemValue
        {
            get
            {
                return _selectedMenuItemValue;
            }

            set
            {
                _selectedMenuItemValue =  value ;
            }
        }


        public EdgeMenuCommandEventArgs( string commandName, string selectedMenuItemValue )
            : base()
        {
            _commandName = commandName;
            _selectedMenuItemValue = selectedMenuItemValue;
        }
    }
}
