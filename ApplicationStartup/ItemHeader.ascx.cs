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

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ItemHeader : System.Web.UI.UserControl
    {
        [Serializable]
        public class SelectedItemHeaderInfo
        {
            public string ItemDescription = String.Empty;
            public string CatalogNumber = String.Empty;
            public bool HideCatalogNumber = false;
            public string CatalogNumberTitle = String.Empty;
            
            public static SelectedItemHeaderInfo Create()
            {
                SelectedItemHeaderInfo selectedItemHeaderInfo = null;

                if( HttpContext.Current.Session[ "SelectedItemHeaderInfo" ] == null )
                {
                    selectedItemHeaderInfo = new SelectedItemHeaderInfo();
                    HttpContext.Current.Session[ "SelectedItemHeaderInfo" ] = selectedItemHeaderInfo;
                }
                else
                {
                    selectedItemHeaderInfo = ( SelectedItemHeaderInfo )HttpContext.Current.Session[ "SelectedItemHeaderInfo" ];
                }

                return ( selectedItemHeaderInfo );
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
                if( this.ViewState[ "ItemHeaderWidth" ] == null )
                {
                    return ( System.Web.UI.WebControls.Unit.Parse( "100%" )); // default
                }
                else
                {
                    return ( ( System.Web.UI.WebControls.Unit )this.ViewState[ "ItemHeaderWidth" ] );
                }
            }
            set
            {
                if( this.HeaderWidth != value )
                {
                    this.ViewState[ "ItemHeaderWidth" ] = value;                 
                }
            }
        }

        public bool HideCatalogNumber
        {
            get
            {
                SelectedItemHeaderInfo selectedItemHeaderInfo = SelectedItemHeaderInfo.Create();
                return ( selectedItemHeaderInfo.HideCatalogNumber );
            }
            set
            {
                SelectedItemHeaderInfo selectedItemHeaderInfo = SelectedItemHeaderInfo.Create();
                selectedItemHeaderInfo.HideCatalogNumber = value;
                UpdateDisplay();
            }
        }

        public string CatalogNumber
        {
            get
            {
                SelectedItemHeaderInfo selectedItemHeaderInfo = SelectedItemHeaderInfo.Create();
                return ( selectedItemHeaderInfo.CatalogNumber );
            }
            set
            {
                SelectedItemHeaderInfo selectedItemHeaderInfo = SelectedItemHeaderInfo.Create();
                selectedItemHeaderInfo.CatalogNumber = value;
                UpdateDisplay();
            }
        }

        public string CatalogNumberTitle
        {
            get
            {
                SelectedItemHeaderInfo selectedItemHeaderInfo = SelectedItemHeaderInfo.Create();
                return ( selectedItemHeaderInfo.CatalogNumberTitle );
            }
            set
            {
                SelectedItemHeaderInfo selectedItemHeaderInfo = SelectedItemHeaderInfo.Create();
                selectedItemHeaderInfo.CatalogNumberTitle = value;
                UpdateDisplay();
            }
        }

        public string ItemDescription
        {
            get
            {
                SelectedItemHeaderInfo selectedItemHeaderInfo = SelectedItemHeaderInfo.Create();
                return ( selectedItemHeaderInfo.ItemDescription );
            }
            set
            {
                SelectedItemHeaderInfo selectedItemHeaderInfo = SelectedItemHeaderInfo.Create();
                selectedItemHeaderInfo.ItemDescription = value;
                UpdateDisplay();
            }
        }

      
        private void UpdateDisplay()
        {
          SelectedItemHeaderInfo selectedItemHeaderInfo = SelectedItemHeaderInfo.Create();
          if( selectedItemHeaderInfo.ItemDescription.Length <= 300 )
          {
              ItemDescriptionLabelData.Text = String.Format( "{0}", selectedItemHeaderInfo.ItemDescription );
          }
          else
          {
              ItemDescriptionLabelData.Text = String.Format( "{0}...", selectedItemHeaderInfo.ItemDescription.Substring( 0, 300 ));
          }

          if( selectedItemHeaderInfo.HideCatalogNumber == true )
          {
              CatalogNumberLabel.Visible = false;
              CatalogNumberLabelData.Visible = false;
          }
          else
          {
              CatalogNumberLabel.Visible = true;
              CatalogNumberLabelData.Visible = true;
              CatalogNumberLabelData.Text = String.Format( "{0}", selectedItemHeaderInfo.CatalogNumber );
              CatalogNumberLabel.Text = String.Format( "{0}", selectedItemHeaderInfo.CatalogNumberTitle );
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