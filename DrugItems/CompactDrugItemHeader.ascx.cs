using System;
using System.Drawing;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace VA.NAC.CM.DrugItems
{
    public partial class CompactDrugItemHeader : System.Web.UI.UserControl
    {
        [Serializable]
        public class SelectedCompactDrugItemHeaderInfo
        {
            public string FDAAssignedLabelerCode = String.Empty;
            public string ProductCode = String.Empty;
            public string PackageCode = String.Empty;

            public static SelectedCompactDrugItemHeaderInfo Create()
            {
                SelectedCompactDrugItemHeaderInfo selectedCompactDrugItemHeaderInfo = null;

                if( HttpContext.Current.Session[ "SelectedCompactDrugItemHeaderInfo" ] == null )
                {
                    selectedCompactDrugItemHeaderInfo = new SelectedCompactDrugItemHeaderInfo();
                    HttpContext.Current.Session[ "SelectedCompactDrugItemHeaderInfo" ] = selectedCompactDrugItemHeaderInfo;
                }
                else
                {
                    selectedCompactDrugItemHeaderInfo = ( SelectedCompactDrugItemHeaderInfo )HttpContext.Current.Session[ "SelectedCompactDrugItemHeaderInfo" ];
                }

                return ( selectedCompactDrugItemHeaderInfo );
            }
        }

        protected void Page_Load( object sender, EventArgs e )
        {
 
        }



        public string FdaAssignedLabelerCode
        {
            get
            {
                SelectedCompactDrugItemHeaderInfo selectedCompactDrugItemHeaderInfo = SelectedCompactDrugItemHeaderInfo.Create();
                return ( selectedCompactDrugItemHeaderInfo.FDAAssignedLabelerCode );
            }
            set
            {
                SelectedCompactDrugItemHeaderInfo selectedCompactDrugItemHeaderInfo = SelectedCompactDrugItemHeaderInfo.Create();
                selectedCompactDrugItemHeaderInfo.FDAAssignedLabelerCode = value;
                UpdateNDCDisplay();
            }
        }

        public string ProductCode
        {
            get
            {
                SelectedCompactDrugItemHeaderInfo selectedCompactDrugItemHeaderInfo = SelectedCompactDrugItemHeaderInfo.Create();
                return ( selectedCompactDrugItemHeaderInfo.ProductCode );
            }
            set
            {
                SelectedCompactDrugItemHeaderInfo selectedCompactDrugItemHeaderInfo = SelectedCompactDrugItemHeaderInfo.Create();
                selectedCompactDrugItemHeaderInfo.ProductCode = value;
                UpdateNDCDisplay();
            }
        }

        public string PackageCode
        {
            get
            {
                SelectedCompactDrugItemHeaderInfo selectedCompactDrugItemHeaderInfo = SelectedCompactDrugItemHeaderInfo.Create();
                return ( selectedCompactDrugItemHeaderInfo.PackageCode );
            }
            set
            {
                SelectedCompactDrugItemHeaderInfo selectedCompactDrugItemHeaderInfo = SelectedCompactDrugItemHeaderInfo.Create();
                selectedCompactDrugItemHeaderInfo.PackageCode = value;
                UpdateNDCDisplay();
            }
        }

        private void UpdateNDCDisplay()
        {
            SelectedCompactDrugItemHeaderInfo selectedCompactDrugItemHeaderInfo = SelectedCompactDrugItemHeaderInfo.Create();
            CombinedNDCLabel.Text = String.Format( "{0} {1} {2}", selectedCompactDrugItemHeaderInfo.FDAAssignedLabelerCode, selectedCompactDrugItemHeaderInfo.ProductCode, selectedCompactDrugItemHeaderInfo.PackageCode );
        }

 
        public string Generic
        {
            get
            {
                return ( GenericNameLabelData.Text );
            }
            set
            {
                GenericNameLabelData.Text = value;
            }
        }

        public string TradeName
        {
            get
            {
                return ( TradeNameLabelData.Text );
            }
            set
            {
                TradeNameLabelData.Text = value;
            }
        }

        public string HeaderTitle
        {
            get
            {
                return ( HeaderTitleLabel.Text );
            }
            set
            {
                HeaderTitleLabel.Text = value;
            }
        }
    }
}