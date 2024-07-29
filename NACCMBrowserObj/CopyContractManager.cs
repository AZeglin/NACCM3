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
    public class CopyContractManager
    {

        public CopyContractManager()
        {
        }

        // select function
        [DataObjectMethod( DataObjectMethodType.Select, true )]
        public CopyContractContent GetCopyContractContent()
        {
            CopyContractContent copyContractContent = null;

            if( HttpContext.Current.Session[ "CopyContractContent" ] != null )
                copyContractContent = ( CopyContractContent )HttpContext.Current.Session[ "CopyContractContent" ];
            else
                copyContractContent = new CopyContractContent();

            return ( copyContractContent );
        }

   
        // update function
        [DataObjectMethod( DataObjectMethodType.Update, true )]
        public static int CopyContract( CopyContractContent copyContractContent )
        {
            // save the values to the session
            HttpContext.Current.Session[ "CopyContractContent" ] = copyContractContent;

            ContractDB contractDB = ( ContractDB )HttpContext.Current.Session[ "ContractDB" ];

            int newContractId = -1;

            bool bSuccess = contractDB.CopyContract( copyContractContent.OldContractNumber, copyContractContent.NewContractNumber, copyContractContent.AwardDate, copyContractContent.EffectiveDate, copyContractContent.ExpirationDate, copyContractContent.OptionYears, ref newContractId );
            if( bSuccess == false )
            {
                throw new Exception( string.Format( "The following error was encountered when attempting to copy the contract from {0} to {1} : {2}", copyContractContent.OldContractNumber, copyContractContent.NewContractNumber, contractDB.ErrorMessage ) );
            }

            copyContractContent.NewContractId = newContractId;

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
