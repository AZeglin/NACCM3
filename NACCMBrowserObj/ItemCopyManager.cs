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
    public class ItemCopyManager
    {

        public ItemCopyManager()
        {
        }

        // select function
        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public ItemCopyContent GetItemCopyContent()
        {
            ItemCopyContent itemCopyContent = null;

            if( HttpContext.Current.Session[ "ItemCopyContent" ] != null )
                itemCopyContent = ( ItemCopyContent )HttpContext.Current.Session[ "ItemCopyContent" ];
            else
                itemCopyContent = new ItemCopyContent();

            return ( itemCopyContent );
        }


        // update function
        [DataObjectMethod( DataObjectMethodType.Update, true )]
        public static int CopyItem( ItemCopyContent itemCopyContent )
        {
            // save the values to the session
            HttpContext.Current.Session[ "ItemCopyContent" ] = itemCopyContent;

            DrugItemDB drugItemDB = ( DrugItemDB )HttpContext.Current.Session[ "DrugItemDB" ];

            int newDrugItemId;
            int newDrugItemNDCId;
            int newDrugItemPacakgeId;
            int destinationContractId;

            bool bSuccess = drugItemDB.CopyItem( itemCopyContent.CopyType, itemCopyContent.SourceContractNumber, itemCopyContent.SelectedDrugItemId, itemCopyContent.FdaAssignedLabelerCode, itemCopyContent.ProductCode, itemCopyContent.PackageCode, itemCopyContent.TradeName, itemCopyContent.GenericName, itemCopyContent.DestinationContractNumber, itemCopyContent.CopyPricing, itemCopyContent.CopySubItems, itemCopyContent.ModificationStatusId, itemCopyContent.DispensingUnit, itemCopyContent.PackageDescription, 
                 itemCopyContent.UnitOfSale, itemCopyContent.QuantityInUnitOfSale, itemCopyContent.UnitPackage, itemCopyContent.QuantityInUnitPackage, itemCopyContent.UnitOfMeasure, out newDrugItemId, out newDrugItemNDCId, out newDrugItemPacakgeId, out destinationContractId );
            if( bSuccess == false )
            {
                throw new Exception( string.Format( "The following error was encountered when attempting to copy item with id {0} : {1}", itemCopyContent.SelectedDrugItemId, drugItemDB.ErrorMessage ) );
            }

            itemCopyContent.NewDrugItemId = newDrugItemId;
            itemCopyContent.NewDrugItemNDCId = newDrugItemNDCId;
            itemCopyContent.NewDrugItemPackageId = newDrugItemPacakgeId;
            itemCopyContent.DestinationContractId = destinationContractId;

            return ( 1 );
        }


        [DataObjectMethod( DataObjectMethodType.Delete, true )]
        public static int Delete( ItemCopyContent itemCopyContent )
        {
            return ( 1 );
        }

        [DataObjectMethod( DataObjectMethodType.Insert, true )]
        public static int Insert( ItemCopyContent itemCopyContent )
        {
            return ( 1 );
        }


    }
}
