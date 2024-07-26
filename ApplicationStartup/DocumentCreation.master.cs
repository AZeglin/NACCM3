using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class DocumentCreation : BaseMasterPage
    {
        public ContentPlaceHolder DocumentCreationBody
        {
            get
            {
                return ( this.CommonContentPlaceHolder );  
            }
        }

        public UpdatePanelEventProxy DocumentCreationMasterEventProxy
        {
            get
            {
                return ( this.DocumentCreationMasterUpdatePanelEventProxy );
            }
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false && ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
            {
                
            }
        }

        protected void SetRedirected( bool bIsRedirected )
        {
            MasterPage master = Page.Master;
            if( master != null )
            {
                MasterPage topMaster = master.Master;

                if( topMaster != null )
                {
                    ( ( NACCM )topMaster ).SetRedirected( bIsRedirected );
                }
            }
        }

        // not used
        public void AssignObjectDataSourceToHeader( EditedOfferDataSource editedOfferDataSource )
        {
            DocumentCreationMasterFormView.DataSource = editedOfferDataSource;
            DocumentCreationMasterFormView.DataKeyNames = new string[] { "OfferId" };
        }

        public void AssignObjectDataSourceToHeader( EditedDocumentDataSource editedDocumentDataSource )
        {
            DocumentCreationMasterFormView.DataSource = editedDocumentDataSource;
            DocumentCreationMasterFormView.DataKeyNames = new string[] { "ContractId" };
        }

        public void BindHeader()
        {
            DocumentCreationMasterFormView.DataBind();
        }

        protected void DocumentCreationMasterFormView_OnPreRender( object sender, EventArgs e )
        {
            FormView documentCreationMasterFormView = ( FormView )sender;
            Label createDocumentHeaderLabel = ( Label )documentCreationMasterFormView.FindControl( "CreateDocumentHeaderLabel" );

            if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewContract ||
                ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewContractFromOffer )
            {
                createDocumentHeaderLabel.Text = "Contract Addition";
            } // not used
            else if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewOffer )
            {
                createDocumentHeaderLabel.Text = "Offer Addition";
            }
        }

        protected void ShowException( Exception ex )
        {
            MasterPage master = Page.Master;
            if( master != null )
            {
                MasterPage topMaster = master.Master;

                if( topMaster != null )
                {
                    MsgBox msgBox = ( MsgBox )topMaster.FindControl( "MsgBox" );

                    if( msgBox != null )
                    {
                        msgBox.ShowErrorFromUpdatePanelAsync( Page, ex );
                    }

                }
            }
        }

        // createcontract2 has only one screen and thus a shortsave is not used
        //private void ShortSave()
        //{
        //    ( ( BaseDocumentEditorPage )this.Page ).ClearErrors();

        //    if( Page.IsValid == true )
        //    {
        //        try
        //        {
        //            ( ( BaseDocumentEditorPage )this.Page ).ShortSave();  // synchronous
        //        }
        //        catch( Exception ex )
        //        {
        //            ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
        //            ShowException( ex );
        //        }
        //    }
        //    else
        //    {
        //        ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
        //        ( ( CreateContract2 )this.Page ).DisplayValidationErrors();
        //    }
        //}

        protected void CancelCreationButton_OnClick( object sender, EventArgs e )
        {
            ( ( BaseDocumentEditorPage )this.Page ).ClearErrors();
            ( ( BaseDocumentEditorPage )this.Page ).CancelCreation();  // currently doesn't do anything

            // $$$ if a contract, if creation came from offer, then go back to offer, else go to start screen.
            ( ( NACCM )Page.Master.Master ).ViewStartScreen();

        }

        protected void DocumentSaveButton_OnClick( object sender, EventArgs e )
        {
            ( ( BaseDocumentEditorPage )this.Page ).ClearErrors();

            try
            {
                ( ( BaseDocumentEditorPage )this.Page ).UpdateDocument();

                // if contract creation, then open full document editor
                if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewContract ||
                        ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewContractFromOffer )
                {
                    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
                    if( currentDocument != null )
                    {
                        ( ( NACCM )Page.Master.Master ).ViewSelectedContract( currentDocument.ContractId, currentDocument.ContractNumber, currentDocument.ScheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.NewContractAfterSave );
                    }
                    else
                    {
                        throw new Exception( "CurrentDocument was null after update." );
                    }
                }

            }
            catch( Exception ex )
            {
                ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
                ShowException( ex );
            }

            ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
            
        }
    }
}