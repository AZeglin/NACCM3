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

using Menu = System.Web.UI.WebControls.Menu;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class OfferView : BaseMasterPage
    {
        public ContentPlaceHolder OfferTabs
        {
            get
            {
                return ( this.CommonContentPlaceHolder ); 
            }
        }

        public UpdatePanelEventProxy OfferViewMasterEventProxy
        {
            get
            {
                return ( this.OfferViewMasterUpdatePanelEventProxy );
            }
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false && ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
            {
      
            }
        }

        // used here and from "Current Document" in main menu
        public static string GetUrlFromMenuItemValue( string menuItemValue )
        {
            string destinationUrl = "~/OfferGeneral.aspx"; // default

            switch( menuItemValue )
            {
                case "General":
                    destinationUrl = "~/OfferGeneral.aspx";
                    break;               
                case "PointsOfContact":                   
                    destinationUrl = "~/OfferPointsOfContact.aspx";
                    break;
                case "Comments":                
                    destinationUrl = "~/OfferComments.aspx";
                    break;
                case "Award":                 
                    destinationUrl = "~/OfferAward.aspx";
                    break;
            }

            return ( destinationUrl );
        }

        protected void OfferMenu_EdgeMenuCommand( EdgeMenu theMenu, EdgeMenuCommandEventArgs args )
        {
            bool bSuccess = true;

            // if menu choice is changing, save the current tab to the front object
            if( Session[ "CurrentSelectedLevel2MenuItemValue" ] != null )
            {
                string currentMenuValue = Session[ "CurrentSelectedLevel2MenuItemValue" ].ToString();
                if( currentMenuValue.CompareTo( args.SelectedMenuItemValue ) != 0 )
                {
                    bSuccess = ShortSave();
                }
            }

            // cancel until the error on the current page is fixed
            if( bSuccess == false )
                return;

            switch( args.SelectedMenuItemValue )
            {
                case "General":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue ) );
                    break;
                case "PointsOfContact":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue ) );
                    break;
                case "Comments":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue ) );
                    break;
                case "Award":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue ) );
                    break;
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

        protected void OfferMenu_OnInit( object sender, EventArgs e )
        {
            EdgeMenu theMenu = ( EdgeMenu )sender;
            theMenu.EdgeMenuCommand += OfferMenu_EdgeMenuCommand;

            theMenu.AddItem( "General", "General" );
            theMenu.AddItem( "Points Of Contact", "PointsOfContact" );
            theMenu.AddItem( "Comments", "Comments" );
            theMenu.AddItem( "Award", "Award" );        
        }

        protected void OfferMenu_OnPreRender( object sender, EventArgs e )
        {
            EdgeMenu offerMenu = ( EdgeMenu )sender;

            // re-highlight the selected menu item
            string menuItemValue = "";

            if( Session[ "CurrentSelectedLevel2MenuItemValue" ] != null )
            {
                menuItemValue = ( string )Session[ "CurrentSelectedLevel2MenuItemValue" ];
                foreach( EdgeMenuItem mi in offerMenu.MenuItems )
                {
                    if( mi.ItemValue.Contains( menuItemValue ) == true )
                    {
                        mi.Highlighted = true;
                        break;
                    }
                }
            }
        }

        public void AssignObjectDataSourceToHeader( EditedOfferDataSource editedOfferDataSource )
        {
            OfferViewMasterVendorFormView.DataSource = editedOfferDataSource;
            OfferViewMasterVendorFormView.DataKeyNames = new string[] { "OfferId" };

            OfferViewMasterVendorAdministratorFormView.DataSource = editedOfferDataSource;
            OfferViewMasterVendorAdministratorFormView.DataKeyNames = new string[] { "OfferId" };

            OfferViewMasterContractingOfficerFormView.DataSource = editedOfferDataSource;
            OfferViewMasterContractingOfficerFormView.DataKeyNames = new string[] { "OfferId" };

            CreateOfferFormView.DataSource = editedOfferDataSource;
            CreateOfferFormView.DataKeyNames = new string[] { "OfferId" };
        }

        public void BindHeader()
        {
            OfferViewMasterVendorFormView.DataBind();
            OfferViewMasterVendorAdministratorFormView.DataBind();
            OfferViewMasterContractingOfficerFormView.DataBind();
            CreateOfferFormView.DataBind();
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

        // save current tab before switching to another tab
        private bool ShortSave()
        {
            bool bSuccess = true;

            ( ( BaseDocumentEditorPage )this.Page ).ClearErrors();

            try
            {
                ( ( BaseDocumentEditorPage )this.Page ).ShortSave();  // synchronous
                //    ( ( BaseDocumentEditorPage )this.Page ).BindAfterShortSave();  if youre leaving the page, you don't really need to rebind
            }
            catch( Exception ex )
            {
                bSuccess = false;
                ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
                ShowException( ex );
            }
   
            return ( bSuccess );
        }

        protected void CancelCreationButton_OnClick( object sender, EventArgs e )
        {
            ( ( BaseDocumentEditorPage )this.Page ).ClearErrors();
            ( ( BaseDocumentEditorPage )this.Page ).CancelCreation();  // currently doesn't do anything

            // go to start screen.
            ( ( NACCM )Page.Master.Master ).ViewStartScreen();
        }

        protected void OfferSaveButton_OnClick( object sender, EventArgs e )
        {
            ( ( BaseDocumentEditorPage )this.Page ).ClearErrors();

            try
            {
                ( ( BaseDocumentEditorPage )this.Page ).UpdateDocument();

                // log the new offer in the recent documents list
                if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewOffer )
                {
                    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                    UserActivity log = ( UserActivity )new UserActivity( VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionTypes.ViewDocument, currentDocument.OfferNumber, VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionDetailsTypes.ViewOffer );
                    log.LogDocumentAccess();

                    // reenter from the top as an existing offer
                    ( ( NACCM )Page.Master.Master ).ViewSelectedOffer( currentDocument.OfferId, currentDocument.ScheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.NewOfferAfterSave );
                }
            }
            catch( Exception ex )
            {
                ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
                ShowException( ex );
            }
            finally
            {
                ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
            }
        }

        protected void OfferViewMasterVendorFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "OfferViewMasterVendorFormView" );

            OfferViewMasterVendorFormView.Visible = bVisible;
            OfferViewMasterVendorFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "OfferViewMasterVendorFormView" );

            if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewOffer )
            {
                //OfferViewMasterVendorFormView.Visible = false;
                //OfferViewMasterVendorFormView.Enabled = false;

                OfferViewVendorCell.Width = new System.Web.UI.WebControls.Unit( "0%" );
            }
            else
            {
                OfferViewVendorCell.Width = new System.Web.UI.WebControls.Unit( "30%" );            
            }
        }

        protected void OfferViewMasterVendorAdministratorFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "OfferViewMasterVendorAdministratorFormView" );

            OfferViewMasterVendorAdministratorFormView.Visible = bVisible;
            OfferViewMasterVendorAdministratorFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "OfferViewMasterVendorAdministratorFormView" );

            if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewOffer )
            {
                //OfferViewMasterVendorAdministratorFormView.Visible = false;
                //OfferViewMasterVendorAdministratorFormView.Enabled = false;
                OfferViewVendorAdministratorCell.Width = new System.Web.UI.WebControls.Unit( "0%" );        
            }
            else
            {
                OfferViewVendorAdministratorCell.Width = new System.Web.UI.WebControls.Unit( "30%" );             
            }
        }


        protected void OfferViewMasterContractingOfficerFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "OfferViewMasterContractingOfficerFormView" );

            OfferViewMasterContractingOfficerFormView.Visible = bVisible;
            OfferViewMasterContractingOfficerFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "OfferViewMasterContractingOfficerFormView" );

            if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewOffer )
            {
                //OfferViewMasterContractingOfficerFormView.Visible = false;
                //OfferViewMasterContractingOfficerFormView.Enabled = false;
                OfferViewContractingOfficerCell.Width = new System.Web.UI.WebControls.Unit( "0%" );
            }
            else
            {
                OfferViewContractingOfficerCell.Width = new System.Web.UI.WebControls.Unit( "30%" );
            }

            // until assistant director is moved from sched/cat to NACSEC, this designation is inaccurate for FSS
            Label assistantDirectorHeaderLabel = ( Label )OfferViewMasterContractingOfficerFormView.FindControl( "AssistantDirectorHeaderLabel" );
            Label assistantDirectorLabel = ( Label )OfferViewMasterContractingOfficerFormView.FindControl( "AssistantDirectorLabel" );       
            assistantDirectorHeaderLabel.Visible = false;
            assistantDirectorLabel.Visible = false;     
        }

        protected void CreateOfferFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "CreateOfferFormView" );

            CreateOfferFormView.Visible = bVisible;
            CreateOfferFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "CreateOfferFormView" );

            OfferViewSaveButtonCell.Width = new System.Web.UI.WebControls.Unit( "10%" );
            Button cancelCreationButton = ( Button )OfferViewSaveButtonCell.FindControl( "CancelCreationButton" );
 
            if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.NewOffer )
            {
                OfferViewCreateOfferCell.Width = new System.Web.UI.WebControls.Unit( "90%" );

                if( cancelCreationButton != null )
                {
                    cancelCreationButton.Visible = true;
                }
            }
            else
            {
                OfferViewCreateOfferCell.Width = new System.Web.UI.WebControls.Unit( "0%" );

                if( cancelCreationButton != null )
                {
                    cancelCreationButton.Visible = false; // no cancel for offer edit
                }
            }

 
        }
    }
}