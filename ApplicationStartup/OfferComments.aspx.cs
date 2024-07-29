using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Threading;
using AjaxControlToolkit;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.ReportManager;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using ListView = VA.NAC.NACCMBrowser.BrowserObj.ListView;
using Menu = System.Web.UI.WebControls.Menu;
using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class OfferComments : BaseDocumentEditorPage
    {
        public OfferComments()
            : base()
        {
        }


        public void Page_PreInit( object sender, EventArgs e )
        {
            SetCreatingNewOffer();

            if( CreatingNewOffer() == true )
            {
            //    this.MasterPageFile = "~/DocumentCreation.Master";
                base.DocumentEditorType = DocumentEditorTypes.NewOffer;
            }
            else
            {
            //    this.MasterPageFile = "~/OfferView.Master";
                base.DocumentEditorType = DocumentEditorTypes.Offer;
            }
        }

        CurrentDocument _currentDocument = null;
        private bool _bCreatingNewOffer = false;

        protected new void Page_Load( object sender, EventArgs e )
        {
            if( Session[ "NACCMStartedProperly" ] == null || Request.UrlReferrer == null )
            {
                Response.Redirect( "~/Start.aspx" );
            }
            
            if( Page.IsPostBack == false && ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
            {
                ClearSessionVariables();
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );

            LoadAndBindNonFormViewControls();

            AssignDataSourceToFormViews();

            if( Page.IsPostBack == false )
            {
                if( CurrentDocumentIsChanging == true )
                {
                    if( CreatingNewOffer() == true )
                    {
                        OfferDataRelay.Clear();  // prepare for offer creation
                    }
                    else
                    {
                        OfferDataRelay.Load();
                    }
                }
                BindFormViews();
            }
        }

        private void SetCreatingNewOffer()
        {
            _bCreatingNewOffer = false;

            // current document is cleared to null if create offer is selected from the main menu
            CurrentDocument currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];

            if( currentDocument == null )
                _bCreatingNewOffer = true;
        }

        private bool CreatingNewOffer()
        {
            return ( _bCreatingNewOffer );
        }

        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            OfferCommentsFormView.DataSource = OfferDataRelay.EditedOfferDataSourceFront;
            OfferCommentsFormView.DataKeyNames = new string[] { "OfferId" };
        }


        protected void ClearSessionVariables()
        {

        }

        protected void BindFormViews()
        {
            BindHeader();

            OfferCommentsFormView.DataBind();
        }

        protected void LoadAndBindNonFormViewControls()
        {
        }

        public override void BindAfterShortSave()
        {
            BindFormViews();
        }

        public override void RebindHeader()
        {
            BindHeader();
        }

        public override string GetValidationGroupName()
        {
            return ( "OfferComments" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            OfferDataRelay offerDataRelay = ( OfferDataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            if( offerDataRelay.EditedOfferContentFront.OfferComment != null )
            {
                if( offerDataRelay.EditedOfferContentFront.OfferComment.Length > 4000 )
                {
                    AppendValidationError( "Offer comment exceeds the maximum length of 4000 characters.", bIsShortSave );
                    bSuccess = false;                  
                }
            }

            return ( bSuccess );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            OfferDataRelay offerDataRelay = ( OfferDataRelay )dataRelayInterface;

            bool bSuccess = true;

            try
            {
                // vendor comments
                string offerComments = "";
                TextBox offerCommentsTextBox = ( TextBox )OfferCommentsFormView.FindControl( "OfferCommentsTextBox" );
                if( offerCommentsTextBox != null )
                {
                    offerComments = offerCommentsTextBox.Text.Replace( "\n", "\r\n" );                    
                    offerDataRelay.EditedOfferContentFront.OfferComment = offerComments;
                }

            }
            catch( Exception ex )
            {
                bSuccess = false;
                ErrorMessage = string.Format( "The following exception was encountered saving the offer information: {0}", ex.Message );
            }

            return ( bSuccess );
        }

        protected void OfferCommentsFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            OfferCommentsFormView.Visible = documentControlPresentation.IsFormViewVisible( "OfferCommentsFormView" );
            OfferCommentsFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "OfferCommentsFormView" );
        }

        protected void OfferCommentsFormView_OnDataBound( object sender, EventArgs e )
        {
        }

        protected void OfferCommentsFormView_OnChange( object sender, EventArgs e )
        {
    //        SetDirtyFlag( "OfferCommentsFormView" );
        }

    }
}