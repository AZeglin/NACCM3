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
    public class EditContractSalesHeaderManager
    {

        public EditContractSalesHeaderManager()
        {
        }

        // select function
        [DataObjectMethod( DataObjectMethodType.Select, true )]
        public EditContractSalesHeaderContent GetEditContractSalesHeaderContent()
        {
            EditContractSalesHeaderContent editContractSalesHeaderContent = null;

            if( HttpContext.Current.Session[ "EditContractSalesHeaderContent" ] != null )
                editContractSalesHeaderContent = ( EditContractSalesHeaderContent )HttpContext.Current.Session[ "EditContractSalesHeaderContent" ];
            else
                editContractSalesHeaderContent = new EditContractSalesHeaderContent();

            return ( editContractSalesHeaderContent );
        }

   
        // update function
        [DataObjectMethod( DataObjectMethodType.Update, true )]
        public static int UpdateContractSalesHeader( EditContractSalesHeaderContent editContractSalesHeaderContent )
        {
            // save the values to the session
            HttpContext.Current.Session[ "EditContractSalesHeaderContent" ] = editContractSalesHeaderContent;

            // note: header information is not saved separately. Only the selected quarterId is saved with the individual SIN's sales.

            return ( 1 );
        }


        [DataObjectMethod( DataObjectMethodType.Delete, true )]
        public static int Delete( CopyContractContent copyContractContent )
        {
            return ( 1 );
        }

        [DataObjectMethod( DataObjectMethodType.Insert, true )]
        public static int Insert( CopyContractContent copyContractContent )
        {
            return ( 1 );
        }


    }
}


