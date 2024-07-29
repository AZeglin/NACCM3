using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using VA.NAC.NACCMBrowser.DBInterface;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class ItemDiscontinueManager
    {

        public ItemDiscontinueManager()
        {
        }

        // select function
        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public ItemDiscontinueContent GetItemDiscontinueContent()
        {
            ItemDiscontinueContent itemDiscontinueContent = null;

            if( HttpContext.Current.Session[ "ItemDiscontinueContent" ] != null )
                itemDiscontinueContent = ( ItemDiscontinueContent )HttpContext.Current.Session[ "ItemDiscontinueContent" ];
            else
                itemDiscontinueContent = new ItemDiscontinueContent();

            return ( itemDiscontinueContent );
        }


        // update function
        [DataObjectMethod( DataObjectMethodType.Update, true )]
        public static int DiscontinueItem( ItemDiscontinueContent itemDiscontinueContent )
        {
            // save the values to the session
            HttpContext.Current.Session[ "ItemDiscontinueContent" ] = itemDiscontinueContent;

            DrugItemDB drugItemDB = ( DrugItemDB )HttpContext.Current.Session[ "DrugItemDB" ];

            bool bSuccess = drugItemDB.DiscontinueItem( itemDiscontinueContent.SourceContractNumber, itemDiscontinueContent.SelectedDrugItemId, itemDiscontinueContent.DiscontinuationDate, itemDiscontinueContent.DiscontinuationReasonString, itemDiscontinueContent.ModificationStatusId );
            if( bSuccess == false )
            {
                throw new Exception( string.Format( "The following error was encountered when attempting to discontinue item with id {0} : {1}", itemDiscontinueContent.SelectedDrugItemId, drugItemDB.ErrorMessage ) );
            }

            return ( 1 );
        }


        [DataObjectMethod( DataObjectMethodType.Delete, true )]
        public static int Delete( ItemDiscontinueContent itemDiscontinueContent )
        {
            return ( 1 );
        }

        [DataObjectMethod( DataObjectMethodType.Insert, true )]
        public static int Insert( ItemDiscontinueContent itemDiscontinueContent )
        {
            return ( 1 );
        }


    }
}
