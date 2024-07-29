using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Web.UI;
using System.ComponentModel.Design;
using System.Reflection;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [TypeConverter( typeof( ExpandableObjectConverter ) )]
    [Serializable]
    public class ContextMenuItem
    {

        private string _displayText;
        private string _commandName;
        private bool _bEnabled = true;
        private string _tooltip;
        private object _commandEventArgs = null;
        private string _clientOnClick = "";

  
        public ContextMenuItem()
        {
        }

        public ContextMenuItem( string displayText, string commandName )
        {
            _displayText = displayText;
            _commandName = commandName;
        }

        public ContextMenuItem( string displayText, string commandName, string toolTip )
        {
            _displayText = displayText;
            _commandName = commandName;
            _tooltip = toolTip;
        }

        public ContextMenuItem( string displayText, string commandName, object commandEventArgs )
        {
            _displayText = displayText;
            _commandName = commandName;
            _commandEventArgs = commandEventArgs;
        }

        public ContextMenuItem( string displayText, string commandName, string toolTip, object commandEventArgs )
        {
            _displayText = displayText;
            _commandName = commandName;
            _tooltip = toolTip;
            _commandEventArgs = commandEventArgs;
        }

        public object CommandEventArgs
        {
            get
            {
                return ( _commandEventArgs );
            }
            set
            {
                _commandEventArgs = value;
            }
        }

        [Category( "Behavior" ), DefaultValue( "" ), Description( "Text of the menu item" ), NotifyParentProperty( true )]
        public string DisplayText
        {
            get { return _displayText; }
            set { _displayText = value; }
        }


        [Category( "Behavior" ), DefaultValue( "" ), Description( "Command name associated with the menu item" ), NotifyParentProperty( true )]
        public string CommandName
        {
            get { return _commandName; }
            set { _commandName = value; }
        }


        [Category( "Behavior" ), DefaultValue( "" ), Description( "The tooltip for the menu item" ), NotifyParentProperty( true )]
        public string Tooltip
        {
            get { return _tooltip; }
            set { _tooltip = value; }
        }

        [Category( "Behavior" ), DefaultValue( true ), Description( "Enable the menu item" ), NotifyParentProperty( true )]
        public bool Enabled
        {
            get { return _bEnabled; }
            set { _bEnabled = value; }
        }

        [Category( "Behavior" ), DefaultValue( "" ), Description( "Set a client on click function" ), NotifyParentProperty( true )]
        public string ClientOnClick
        {
            get { return _clientOnClick; }
            set { _clientOnClick = value; }
        }


    }
}
