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
    public class NDCChangeManager
    {

        public NDCChangeManager()
        {
        }

        // select function
        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public NDCChangeContent GetNDCChangeContent()
        {
            NDCChangeContent ndcChangeContent = null;

            if( HttpContext.Current.Session[ "NDCChangeContent" ] != null )
                ndcChangeContent = ( NDCChangeContent )HttpContext.Current.Session[ "NDCChangeContent" ];
            else
                ndcChangeContent = new NDCChangeContent();

            return ( ndcChangeContent );
        }

        //// update function
        //public void ChangeNDC()
        //{
        //    NDCChangeContent ndcChangeContent = null;

        //    if( HttpContext.Current.Session[ "NDCChangeContent" ] != null )
        //        ndcChangeContent = ( NDCChangeContent )HttpContext.Current.Session[ "NDCChangeContent" ];
        //    else
        //        return;

        //    DrugItemDB drugItemDB = ( DrugItemDB )HttpContext.Current.Session[ "DrugItemDB" ];
            
        //    bool bSuccess = drugItemDB.ChangeNDC( ndcChangeContent.ContractNumber, ndcChangeContent.SelectedDrugItemId, ndcChangeContent.FdaAssignedLabelerCode, ndcChangeContent.ProductCode, ndcChangeContent.PackageCode, ndcChangeContent.DiscontinuationDateForOldNDC, ndcChangeContent.CopyPricing, ndcChangeContent.CopySubItems, ndcChangeContent.ModificationStatusId );
        //    if( bSuccess == false )
        //    {
        //        throw new Exception( string.Format( "The following error was encountered when attempting to change NDC for item with id {0} : {1}", ndcChangeContent.SelectedDrugItemId, drugItemDB.ErrorMessage ) );
        //    }
        //}


        // update function
        [DataObjectMethod( DataObjectMethodType.Update, true )]
        public static int ChangeNDC( NDCChangeContent ndcChangeContent )
        {
            // save the values to the session
            HttpContext.Current.Session[ "NDCChangeContent" ] = ndcChangeContent;

            DrugItemDB drugItemDB = ( DrugItemDB )HttpContext.Current.Session[ "DrugItemDB" ];

            int newDrugItemId;
            int newDrugItemNDCId;
            int newDrugItemPacakgeId;

            bool bSuccess = drugItemDB.ChangeNDC( ndcChangeContent.ContractNumber, ndcChangeContent.SelectedDrugItemId, ndcChangeContent.FdaAssignedLabelerCode, ndcChangeContent.ProductCode, ndcChangeContent.PackageCode, ndcChangeContent.DiscontinuationDateForOldNDC, ndcChangeContent.EffectiveDateForNewNDC, ndcChangeContent.CopyPricing, ndcChangeContent.CopySubItems, ndcChangeContent.ModificationStatusId, out newDrugItemId, out newDrugItemNDCId, out newDrugItemPacakgeId );
            if( bSuccess == false )
            {
                throw new Exception( string.Format( "The following error was encountered when attempting to change the NDC for item with id {0} : {1}", ndcChangeContent.SelectedDrugItemId, drugItemDB.ErrorMessage ) );
            }

            ndcChangeContent.NewDrugItemId = newDrugItemId;
            ndcChangeContent.NewDrugItemNDCId = newDrugItemNDCId;
            ndcChangeContent.NewDrugItemPackageId = newDrugItemPacakgeId;

            return ( 1 );
        }


        [DataObjectMethod( DataObjectMethodType.Delete, true )]
        public static int Delete( NDCChangeContent ndcChangeContent )
        {
            return ( 1 );
        }

        [DataObjectMethod( DataObjectMethodType.Insert, true )]
        public static int Insert( NDCChangeContent ndcChangeContent )
        {
            return ( 1 );
        }


    }
}
