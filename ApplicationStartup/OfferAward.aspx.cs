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
    public partial class OfferAward : BaseDocumentEditorPage
    {
        public OfferAward()
            : base()
        {
        }

        private bool _bCreatingNewOffer = false;

        public void Page_PreInit( object sender, EventArgs e )
        {
            SetCreatingNewOffer();

            if( CreatingNewOffer() == true )
            {
               // this.MasterPageFile = "~/DocumentCreation.Master";
                base.DocumentEditorType = DocumentEditorTypes.NewOffer;
            }
            else
            {
              //  this.MasterPageFile = "~/OfferView.Master";
                base.DocumentEditorType = DocumentEditorTypes.Offer;
            }
        }

        public new void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( CreatingNewOffer() != true )
            {
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                if( currentDocument.IsOfferCompleted == true )
                {
                    if( OfferDataRelay != null )
                    {
                        if( OfferDataRelay.EditedOfferContentFront != null )
                        {
                            if( OfferDataRelay.EditedOfferContentFront.ActionId == CMGlobals.AwardedOfferActionId )
                            {
                                // offer is awarded
                                if( IsAwardedContractAvailable() == true )
                                {
                                    OfferAwardFormView.DefaultMode = FormViewMode.ReadOnly;
                                }
                                else
                                {
                                    // show create award button
                                    OfferAwardFormView.DefaultMode = FormViewMode.Edit;
                                }
                            }
                            else  // no award or withdrawn
                            {
                                // not used: empty data template  = OfferAwardFormView.DefaultMode = FormViewMode.Insert;  
                                // show create award button
                                OfferAwardFormView.DefaultMode = FormViewMode.Edit;     
                            }
                        }
                    }
                }
                else  // not a completion status
                {
                    // show create award button
                    OfferAwardFormView.DefaultMode = FormViewMode.Edit;
                }
            }
            else  // in creation mode
            {
                // show create award button
                OfferAwardFormView.DefaultMode = FormViewMode.Edit;
            }
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

        private bool IsAwardedContractAvailable()
        {
            if( CreatingNewOffer() == true )
                return ( false );

            bool bIsAwardedContractAvailable = false;

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument.IsOfferCompleted == true )
            {
                if( OfferDataRelay != null )
                {
                    if( OfferDataRelay.EditedOfferContentFront != null )
                    {
                        if( OfferDataRelay.EditedOfferContentFront.ContractNumber != null )
                        {
                            if( OfferDataRelay.EditedOfferContentFront.ContractNumber.Length > 0 )
                            {
                                bIsAwardedContractAvailable = true;
                            }
                        }
                    }
                }
            }

            return ( bIsAwardedContractAvailable );
        }

        private bool IsOfferAwarded()
        {
            if( CreatingNewOffer() == true )
                return ( false );
            
            bool bIsOfferAwarded = false;

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument.IsOfferCompleted == true )
            {
                if( OfferDataRelay != null )
                {
                    if( OfferDataRelay.EditedOfferContentFront != null )
                    {
                        if( OfferDataRelay.EditedOfferContentFront.ActionId == CMGlobals.AwardedOfferActionId )
                        {
                            bIsOfferAwarded = true;
                        }
                    }
                }
            }

            return ( bIsOfferAwarded );
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

            OfferAwardFormView.DataSource = OfferDataRelay.EditedOfferDataSourceFront;
            OfferAwardFormView.DataKeyNames = new string[] { "OfferId" };
        }


        protected void ClearSessionVariables()
        {

        }

        protected void BindFormViews()
        {
            BindHeader();

            OfferAwardFormView.DataBind();
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

        // returns the status, also changes the color of the status control in the parent form view
        protected string ContractStatus( object expirationDateObj, object completionDateObj, string labelControlName )
        {
            Label statusLabel = ( Label )OfferAwardFormView.FindControl( labelControlName );
            string status = "";

            status = CMGlobals.GetContractStatus( expirationDateObj, completionDateObj, statusLabel );

            return ( status );
        }

        public override string GetValidationGroupName()
        {
            return ( "OfferAward" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            OfferDataRelay offerDataRelay = ( OfferDataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            // nothing to save or validate

            return ( bSuccess );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            OfferDataRelay offerDataRelay = ( OfferDataRelay )dataRelayInterface;

            bool bSuccess = true;

            // nothing to save or validate

            return ( bSuccess );
        }

        private void EnableAwardButton( bool bEnable )
        {
            Button awardContractButton = ( Button )OfferAwardFormView.FindControl( "AwardContractButton" );
            if( awardContractButton != null )
            {
                awardContractButton.Enabled = bEnable;
            }
        }

        protected void OfferAwardFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bIsFormViewVisible = documentControlPresentation.IsFormViewVisible( "OfferAwardFormView" );
            bool bIsFormViewEnabled = documentControlPresentation.IsFormViewEnabled( "OfferAwardFormView" );
            OfferAwardFormView.Visible = bIsFormViewVisible;
            OfferAwardFormView.Enabled = bIsFormViewEnabled;

            if( bIsFormViewEnabled == true )
            {
                if( CreatingNewOffer() != true )
                {
                    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                    if( currentDocument.IsOfferCompleted == true )
                    {
                        if( OfferDataRelay != null )
                        {
                            if( OfferDataRelay.EditedOfferContentFront != null )
                            {
                                if( OfferDataRelay.EditedOfferContentFront.ActionId == CMGlobals.AwardedOfferActionId )
                                {
                                    // offer is awarded
                                    if( IsAwardedContractAvailable() == false )
                                    {
                                        // enable
                                        EnableAwardButton( true );
                                    }
                                    else
                                    {
                                        // disable
                                        EnableAwardButton( false );
                                    }
                                }
                                else
                                {
                                    // disable
                                    EnableAwardButton( false );
                                }
                            }
                        }
                    }
                    else
                    {
                        // disable
                        EnableAwardButton( false );
                    }
                }
                else
                {
                    // disable
                    EnableAwardButton( false );
                }
            }
        }
        
        protected void OfferAwardFormView_OnDataBound( object sender, EventArgs e )
        {
        }

        // jump to awarded contract from award screen
        protected void SelectAwardedContractButton_OnCommand( object sender, CommandEventArgs e )
        {
            int contractId = -1;
            string contractNumber = "";
            int scheduleNumber = -1;

            if( e.CommandName.CompareTo( "JumpToAwardedContract" ) == 0 )
            {

                string commandArgument = e.CommandArgument.ToString();
                string[] argumentList = commandArgument.Split( new char[] { ',' } );

                contractId = int.Parse( argumentList[ 0 ].ToString() );
                contractNumber = argumentList[ 1 ].ToString();
                scheduleNumber = int.Parse( argumentList[ 2 ].ToString() );

                ( ( NACCM )Page.Master.Master ).ViewSelectedContract( contractId, contractNumber, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.OfferAwardFormViewViewContract );

            }
        }

        protected void AwardContractButton_OnClick( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            ( ( NACCM )Page.Master.Master ).CreateContract( currentDocument.OfferId, currentDocument.ScheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.OfferAwardFormViewCreateContract );
        }
    }
}