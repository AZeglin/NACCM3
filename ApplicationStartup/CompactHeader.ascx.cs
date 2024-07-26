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

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class CompactHeader : System.Web.UI.UserControl
    {
        [Serializable]
        public class SelectedCompactHeaderInfo
        {
            public string ContractNumber = String.Empty;
            public string ContractorName = String.Empty;
            public string CommodityCovered = String.Empty;

            public static SelectedCompactHeaderInfo Create()
            {
                SelectedCompactHeaderInfo selectedCompactHeaderInfo = null;

                if( HttpContext.Current.Session[ "SelectedCompactHeaderInfo" ] == null )
                {
                    selectedCompactHeaderInfo = new SelectedCompactHeaderInfo();
                    HttpContext.Current.Session[ "SelectedCompactHeaderInfo" ] = selectedCompactHeaderInfo;
                }
                else
                {
                    selectedCompactHeaderInfo = ( SelectedCompactHeaderInfo )HttpContext.Current.Session[ "SelectedCompactHeaderInfo" ];
                }

                return ( selectedCompactHeaderInfo );
            }
        }

        protected void Page_Load( object sender, EventArgs e )
        {
 
        }

        public string ContractNumber
        {
            get
            {
                return ( ContractNumberLabelData.Text );
            }
            set
            {
                 ContractNumberLabelData.Text = value;
            }
        }
 
        public string ContractorName
        {
            get
            {
                return ( ContractorNameLabelData.Text );
            }
            set
            {
                ContractorNameLabelData.Text = value;
            }
        }

        public string CommodityCovered
        {
            get
            {
                return ( CommodityCoveredLabelData.Text );
            }
            set
            {
                CommodityCoveredLabelData.Text = value;
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