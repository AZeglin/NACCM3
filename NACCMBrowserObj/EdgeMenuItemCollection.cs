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
    public class EdgeMenuItemCollection : CollectionBase
    {
        public EdgeMenuItemCollection()
        {
        }

        public EdgeMenuItem this[ int index ]
        {
            get 
            { 
                return ( EdgeMenuItem )InnerList[ index ]; 
            }
            set 
            { 
                InnerList[ index ] = value; 
            }
        }

        // all modifications need to be completed before prerender
        // add an object to the end of the collection
        public void Add( EdgeMenuItem item )
        {
            InnerList.Add( item );
        }
    
        // create and add an item to the end of the collection
        public void Add( string displayText, string itemValue )
        {
            EdgeMenuItem item = new EdgeMenuItem( displayText, itemValue );
            InnerList.Add( item );
        }

        // create and add an item to the end of the collection
        public void Add( string displayText, string itemValue, string target, string itemUrl )
        {
            EdgeMenuItem item = new EdgeMenuItem( displayText, itemValue, target, itemUrl );
            InnerList.Add( item );
        }

        // add an object at the specified position in the collection
        public void AddAt( int index, EdgeMenuItem item )
        {
            InnerList.Insert( index, item );
        }

        // remove an object from the specified position in the collection
        public void Remove( int index )
        {
            InnerList.RemoveAt( index );
        }

        // set enabled attribute of an item in the collection
        public void Enable( int index, bool bEnabled )
        {
            EdgeMenuItem item = ( EdgeMenuItem )InnerList[ index ];
            item.Enabled = bEnabled;
        }

        // set enabled attribute of an item in the collection
        public void Enable( string itemValue, bool bEnabled )
        {
            foreach( EdgeMenuItem i in InnerList )
            {
                if( i.ItemValue.CompareTo( itemValue ) == 0 )
                {
                    i.Enabled = bEnabled;
                }
            }          
        }

        // set highlighted attribute of an item in the collection
        public void Highlight( int index, bool bHighlighted )
        {
            EdgeMenuItem item = ( EdgeMenuItem )InnerList[ index ];
            item.Highlighted = bHighlighted;
        }

        // set highlighted attribute of an item in the collection and un-highlight the others
        public void Highlight( string itemValue )
        {
            foreach( EdgeMenuItem i in InnerList )
            {
                if( i.ItemValue.CompareTo( itemValue ) == 0 )
                {
                    i.Highlighted = true;
                }
                else
                {
                    i.Highlighted = false;
                }
            }
        }

        public EdgeMenuItem GetEdgeMenuItem( string itemValue )
        {
            EdgeMenuItem returnValue = null;

            foreach( EdgeMenuItem i in InnerList )
            {
                if( i.ItemValue.CompareTo( itemValue ) == 0 )
                {
                    returnValue = i;
                    break;
                }
            }
            return ( returnValue );
        }
    
        public int GetEdgeMenuItemIndexFromValue( string itemValue )
        {
            int returnIndex = -1;

            for( int i = 0; i < InnerList.Count; i++ ) 
            {
                EdgeMenuItem item = ( EdgeMenuItem )InnerList[ i ];
                if( item.ItemValue.CompareTo( itemValue ) == 0 )
                {
                    returnIndex = i;
                    break;
                }
            }
            return ( returnIndex );
        }

    }
}
