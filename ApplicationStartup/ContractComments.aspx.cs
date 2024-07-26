using System;
using System.Collections;
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
    public partial class ContractComments : BaseDocumentEditorPage
    {
        public ContractComments()
            : base( DocumentEditorTypes.Contract )
        {
        }

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
                    DataRelay.Load();
                }
                BindFormViews();
            }
        }

        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            ContractCommentsFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            ContractCommentsFormView.DataKeyNames = new string[] { "ContractId" };
        }

        protected void ClearSessionVariables()
        {

        }

        protected void BindFormViews()
        {
            BindHeader();

            ContractCommentsFormView.DataBind();

            // note form view controls are not yet created here
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
            return ( "ContractComments" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
           DataRelay dataRelay = ( DataRelay )dataRelayInterface;

           bool bSuccess = true;

           ResetValidationGroup( validationGroupName );

           if( dataRelay.EditedDocumentContentFront.GeneralContractNotes.Length > 800 )
           {
               AppendValidationError( "Contract comments cannot exceed a length of 800 characters.", bIsShortSave );
               bSuccess = false;
           }

           return ( bSuccess );
       }

       public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
       {
           DataRelay dataRelay = ( DataRelay )dataRelayInterface;

           bool bSuccess = true;

           try
           {
               // contract comments
               string contractComments = "";
               TextBox contractCommentsTextBox = ( TextBox )ContractCommentsFormView.FindControl( "ContractCommentsTextBox" );
               if( contractCommentsTextBox != null )
               {
                   contractComments = contractCommentsTextBox.Text.Replace( "\n", "\r\n" );
                   dataRelay.EditedDocumentContentFront.GeneralContractNotes = contractComments;
               }
           }
           catch( Exception ex )
           {
               bSuccess = false;
               ErrorMessage = string.Format( "The following exception was encountered saving the contract information: {0}", ex.Message );
           }

           return ( bSuccess );
       }

       protected void ContractCommentsFormView_OnChange( object sender, EventArgs e )
       {
     //      SetDirtyFlag( "ContractCommentsFormView" );
       }

       protected void ContractCommentsFormView_OnPreRender( object sender, EventArgs e )
       {
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           ContractCommentsFormView.Visible = documentControlPresentation.IsFormViewVisible( "ContractCommentsFormView" );
           ContractCommentsFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "ContractCommentsFormView" );
       }



    }
}