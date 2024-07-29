using System;
using System.Collections;
using System.ComponentModel;
using System.Web.UI;
using System.ComponentModel.Design;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class ContextMenuItemCollection : CollectionBase
    {
        public ContextMenuItemCollection()
        {
        }

        public ContextMenuItem this[ int index ]
        {
            get 
            { 
                return ( ContextMenuItem )InnerList[ index ]; 
            }
            set 
            { 
                InnerList[ index ] = value; 
            }
        }


        // add an object to the end of the collection
        public void Add( ContextMenuItem item )
        {
            InnerList.Add( item );
        }

        // create and add an item to the end of the collection
        public void Add( string displayText, string commandName, string toolTip, object commandEventArgs )
        {
            ContextMenuItem item = new ContextMenuItem( displayText, commandName, toolTip, commandEventArgs );
            InnerList.Add( item );
        }

        // create and add an item to the end of the collection
        public void Add( string displayText, string commandName, bool bEnabled, string toolTip )
        {
            ContextMenuItem item = new ContextMenuItem( displayText, commandName, toolTip );
            InnerList.Add( item );
        }

        // create and add an item to the end of the collection
        public void Add( string displayText, string commandName, bool bEnabled )
        {
            ContextMenuItem item = new ContextMenuItem( displayText, commandName, bEnabled );
            InnerList.Add( item );
        }

        // add an object at the specified position in the collection
        public void AddAt( int index, ContextMenuItem item )
        {
            InnerList.Insert( index, item );
        }

        // set enabled attribute of an item in the collection
        public void Enable( int index, bool bEnabled )
        {
            ContextMenuItem item = ( ContextMenuItem )InnerList[ index ];
            item.Enabled = bEnabled;
        }

        // set enabled attribute of an item in the collection, by command name
        public void Enable( string commandName, bool bEnabled )
        {
            foreach( ContextMenuItem item in InnerList )
            {
                if( item.CommandName.CompareTo( commandName ) == 0 )
                {
                    item.Enabled = bEnabled;
                    break;
                }
            }
        }
    }
}
