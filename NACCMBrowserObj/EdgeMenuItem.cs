using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    /*
       IE MenuItem Rendering - works with JAWS - used as a reference

           <div class="staticMenuItemStyle" id="MainMenu" style="float: left;">
               <ul tabindex="0" class="level1 static" role="menubar" style="width: auto; float: left; position: relative;">
                   <li class="static" role="menuitem" style="float: left; position: relative;">
                       <a tabindex="-1" class="level1 staticMenuItemStyle static" onclick="__doPostBack('ctl00$MainMenu','FirstMenuItem')" href="#">First Menu Item</a>
                   </li>
               </ul>
           </div>

        * 
        */

    [TypeConverter( typeof( ExpandableObjectConverter ) )]
    [Serializable]
    public class EdgeMenuItem
    {
        private string _displayText = "";
        private string _itemValue = "";

        private string _itemUrl = "";
        private bool _bHasUrl = false;

        private string _target = "";
        private bool _bHasTarget = false;

        private bool _bEnabled = true;
        private bool _bHighlighted = false;

        public EdgeMenuItem()
        {

        }

        public EdgeMenuItem( string displayText, string itemValue )
        {
            _displayText = displayText;
            _itemValue = itemValue;        
        }

        public EdgeMenuItem( string displayText, string itemValue, string target, string itemUrl )
        {
            _displayText = displayText;
            _itemValue = itemValue;
            Target = target;
            ItemUrl = itemUrl;
        }

        public string ItemText
        {
            get
            {              
                return _displayText;
            }

            set
            {
                _displayText = value;
            }
        }

        public string ItemValue
        {
            get
            {
                return _itemValue;
            }

            set
            {
                _itemValue = value;
            }
        }

        public string ItemUrl
        {
            get
            {
                return _itemUrl;
            }

            set
            {
                if( value.Length > 0 )
                {
                    _bHasUrl = true;
                }

                _itemUrl = value;
            }
        }

        public string Target
        {
            get
            {
                return _target;
            }

            set
            {
                if( value.Length > 0 )
                {
                    _bHasTarget = true;
                }

                _target = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return _bEnabled;
            }

            set
            {
                _bEnabled = value;
            }
        }

        public bool Highlighted
        {
            get
            {
                return _bHighlighted;
            }

            set
            {
                _bHighlighted =  value ;
            }
        }

        public bool HasUrl
        {
            get
            {
                return _bHasUrl;
            }

            set
            {          
                _bHasUrl = value;
            }
        }

        public bool HasTarget
        {
            get
            {
                return _bHasTarget;
            }

            set
            {
                _bHasTarget = value;
            }
        }
    }
}
