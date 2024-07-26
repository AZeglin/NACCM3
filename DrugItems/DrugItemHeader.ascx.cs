using System;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ComponentModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace VA.NAC.CM.DrugItems
{
    public partial class DrugItemHeader : System.Web.UI.UserControl
    {
        [Serializable]
        public class SelectedDrugItemHeaderInfo
        {
            public string FDAAssignedLabelerCode = String.Empty;
            public string ProductCode = String.Empty;
            public string PackageCode = String.Empty;

            public static SelectedDrugItemHeaderInfo Create()
            {
                SelectedDrugItemHeaderInfo selectedDrugItemHeaderInfo = null;

                if( HttpContext.Current.Session[ "SelectedDrugItemHeaderInfo" ] == null )
                {
                    selectedDrugItemHeaderInfo = new SelectedDrugItemHeaderInfo();
                    HttpContext.Current.Session[ "SelectedDrugItemHeaderInfo" ] = selectedDrugItemHeaderInfo;
                }
                else
                {
                    selectedDrugItemHeaderInfo = ( SelectedDrugItemHeaderInfo )HttpContext.Current.Session[ "SelectedDrugItemHeaderInfo" ];
                }

                return ( selectedDrugItemHeaderInfo );
            }
        }

        protected void Page_Load( object sender, EventArgs e )
        {
 
        }

        [DefaultValue( "100%" )]
        [Category( "Appearance" )]
        [Description( "Width of the header control." )]
        public System.Web.UI.WebControls.Unit HeaderWidth
        {
            get
            {
                if( this.ViewState[ "HeaderWidth" ] == null )
                {
                    return ( System.Web.UI.WebControls.Unit.Parse( "100%" )); // default
                }
                else
                {
                    return ( ( System.Web.UI.WebControls.Unit )this.ViewState[ "HeaderWidth" ] );
                }
            }
            set
            {
                if( this.HeaderWidth != value )
                {
                    this.ViewState[ "HeaderWidth" ] = value;
                    //ItemDetailsHeaderPanel.Width = value;
                    //HtmlControl ItemDetailsHeaderDiv = ( HtmlControl )ItemDetailsHeaderPanel.FindControl( "ItemDetailsHeaderDiv" );
                    //if( ItemDetailsHeaderDiv != null )
                    //{
                    //    ItemDetailsHeaderDiv.Attributes.Add( "style", string.Format( "width:{0}; height:76px; top:0px;  left:0px; border:solid 1px black; background-color:White; margin:1px;", value )); 
                    //}
                }
            }
        }

        public string FdaAssignedLabelerCode
        {
            get
            {
                SelectedDrugItemHeaderInfo selectedDrugItemHeaderInfo = SelectedDrugItemHeaderInfo.Create();
                return ( selectedDrugItemHeaderInfo.FDAAssignedLabelerCode );
            }
            set
            {
                SelectedDrugItemHeaderInfo selectedDrugItemHeaderInfo = SelectedDrugItemHeaderInfo.Create();
                selectedDrugItemHeaderInfo.FDAAssignedLabelerCode = value;
                UpdateNDCDisplay();
            }
        }

        public string ProductCode
        {
            get
            {
                SelectedDrugItemHeaderInfo selectedDrugItemHeaderInfo = SelectedDrugItemHeaderInfo.Create();
                return ( selectedDrugItemHeaderInfo.ProductCode );
            }
            set
            {
                SelectedDrugItemHeaderInfo selectedDrugItemHeaderInfo = SelectedDrugItemHeaderInfo.Create();
                selectedDrugItemHeaderInfo.ProductCode = value;
                UpdateNDCDisplay();
            }
        }

        public string PackageCode
        {
            get
            {
                SelectedDrugItemHeaderInfo selectedDrugItemHeaderInfo = SelectedDrugItemHeaderInfo.Create();
                return ( selectedDrugItemHeaderInfo.PackageCode );
            }
            set
            {
                SelectedDrugItemHeaderInfo selectedDrugItemHeaderInfo = SelectedDrugItemHeaderInfo.Create();
                selectedDrugItemHeaderInfo.PackageCode = value;
                UpdateNDCDisplay();
            }
        }

        private void UpdateNDCDisplay()
        {
          SelectedDrugItemHeaderInfo selectedDrugItemHeaderInfo = SelectedDrugItemHeaderInfo.Create();
          CombinedNDCLabel.Text = String.Format( "{0} {1} {2}", selectedDrugItemHeaderInfo.FDAAssignedLabelerCode, selectedDrugItemHeaderInfo.ProductCode, selectedDrugItemHeaderInfo.PackageCode );
        }

        public void SetCovered( string coveredYN )
        {
            CoveredLabel.Text = coveredYN;

            if( coveredYN.CompareTo( "Covered" ) == 0 )
            {
                CoveredLabel.ForeColor = Color.Crimson;
            }
            else
            {
                CoveredLabel.ForeColor = Color.Coral;
            }
        }

        public string GetCovered()
        {
            if( CoveredLabel.Text.CompareTo( "Covered" ) == 0 )
            {
                return ( "T" );
            }
            else
            {
                return ( "F" );
            }
        }

        public void SetFETAmount( string FETAmountString )
        {
            Decimal FETAmount = 0;

            if( FETAmountString.Length > 0 )
            {
                if( Decimal.TryParse( FETAmountString, out FETAmount ) == true )
                {
                    FETAmountLabel.Text = String.Format( "Includes FET of {0}", FETAmountString );
                }
                else
                {
                    FETAmountLabel.Text = "";
                }
            }
            else
            {
                FETAmountLabel.Text = "";
            }
        }

        public void SetSingleDual( string singleDualUndefined )
        {
            if( singleDualUndefined.CompareTo( "Undefined" ) != 0 )
                SingleDualLabel.Text = string.Format( "{0} Pricer", singleDualUndefined );
            else
                SingleDualLabel.Text = "";
        }

        public bool IsDual()
        {
            if( SingleDualLabel.Text.CompareTo( "Dual Pricer" ) == 0 )
            {
                return( true );
            }
            else
            {
                return( false );
            }
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