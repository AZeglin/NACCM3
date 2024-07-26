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
    public partial class ContractView : BaseMasterPage
    {
        public ContentPlaceHolder ContractTabs
        {
            get
            {
                return ( this.ContractViewContentPlaceHolder ); 
            }
        }

        public UpdatePanelEventProxy ContractViewMasterEventProxy
        {
            get
            {
                return ( this.ContractViewMasterUpdatePanelEventProxy );
            }
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false && ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
            {
                Session[ "CurrentSelectedLevel2MenuItemValue" ] = null; 
            }
           
            ///$$$ add hide progress indicator
            if( Page.IsPostBack == true )
            {
                ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
            }

            //MasterPage master = Page.Master;
            //if( master != null )
            //{
            //    MasterPage topMaster = master.Master;

            //    if( topMaster != null )
            //    {
            //        Panel theMainRightPanel = ( ( NACCM )topMaster ).TheMainRightPanel;
            //        UpdatePanelEventProxy theMainRightPanelUpdatePanelEventProxy = ( ( NACCM )topMaster ).TheMainRightPanelUpdatePanelEventProxy;
            //        if( theMainRightPanel != null )
            //        {
            //            theMainRightPanel.Attributes.Remove( "display" );
            //            theMainRightPanel.Attributes.Add( "display", "inline-block" );
            //            theMainRightPanelUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            //        }
            //    }
            //}
        }

        // used here and from "Current Document" in main menu
        public static string GetUrlFromMenuItemValue( string menuItemValue, bool bIsNational )
        {
            string destinationUrl = "~/ContractGeneral.aspx"; // default
            
            switch( menuItemValue )
            {
                case "General":
                    destinationUrl = "~/ContractGeneral.aspx";
                    break;
                case "Vendor":                   
                    destinationUrl = "~/ContractVendor.aspx";
                    break;
                case "Contract":                   
                    destinationUrl = "~/ContractDetails.aspx";
                    break;
                case "PointsOfContact":                  
                    destinationUrl = "~/PointsOfContact.aspx";
                    break;
                case "Items":                  
                    destinationUrl = "~/ContractItems.aspx";
                    break;
                case "Rebates":                  
                    destinationUrl = "~/ContractRebate.aspx";
                    break;
                case "Sales":      
                    if( Config.SalesVersion.CompareTo( "S2" ) == 0  &&  bIsNational == false )
                    {                    
                        destinationUrl = "~/ContractSales2.aspx";
                    }
                    else if( Config.SalesVersion.CompareTo( "S2" ) == 0  &&  bIsNational == true )
                    {
                        destinationUrl = "~/ContractSales.aspx";
                    }
                    else  // S1
                    {
                        destinationUrl = "~/ContractSales.aspx";
                    }

                    break;
                case "Payments":     // S2 only
                    if( bIsNational == false )
                    {
                        destinationUrl = "~/ContractPayments.aspx";
                    }
                    else 
                    {
                        destinationUrl = "~/ContractPaymentsNational.aspx";
                    }
                    
                    break;
                case "Checks":      
                    if( Config.SalesVersion.CompareTo( "S2" ) == 0  &&  bIsNational == false )
                    {                                         
                        destinationUrl = "~/ContractChecks2.aspx";                     
                    }
                    else  // S1
                    { 
                        destinationUrl = "~/ContractChecks.aspx";
                    }
                    break;
                case "SBAPlans":                 
                    destinationUrl = "~/ContractSBAPlan.aspx";
                    break;
                case "Comments":                
                    destinationUrl = "~/ContractComments.aspx";
                    break;
            }

            return ( destinationUrl );
        }

        // args.SelectedMenuItemValue is item.ItemValue
        private void ContractMenu_EdgeMenuCommand( EdgeMenu theMenu, EdgeMenuCommandEventArgs args )
        {
            bool bSuccess = true;
            bool bIsNational = false;

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

            if( Session[ "CurrentDocument" ] != null )
            {
                CurrentDocument currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];
                bIsNational = ( currentDocument.Division == CurrentDocument.Divisions.National ) ? true : false;
            }
          
            switch( args.SelectedMenuItemValue )
            {
                case "General":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ));
                    break;
                case "Vendor":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
                    break;
                case "Contract":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
                    break;
                case "PointsOfContact":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
                    break;
                case "Items":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
                    break;
                case "Rebates":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
                    break;
                case "Sales":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
                    break;
                case "Payments":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
                    break;
                case "Checks":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
                    break;
                case "SBAPlans":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
                    break;
                case "Comments":
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = args.SelectedMenuItemValue;
                    SetRedirected( true );
                    Response.Redirect( GetUrlFromMenuItemValue( args.SelectedMenuItemValue, bIsNational ) );
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

        protected void ContractMenu_OnInit( object sender, EventArgs e )
        {
            EdgeMenu theMenu = ( EdgeMenu )sender;
            theMenu.EdgeMenuCommand += ContractMenu_EdgeMenuCommand;

            theMenu.AddItem( "General", "General" );
            theMenu.AddItem( "Comments", "Comments" );
            theMenu.AddItem( "Vendor", "Vendor" );
            theMenu.AddItem( "Contract", "Contract" );
            theMenu.AddItem( "Points of Contact", "PointsOfContact" );
            theMenu.AddItem( "Pricelist", "Items" );

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            // hide rebates if BPA
            if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) != true )
            {
                theMenu.AddItem( "Rebates", "Rebates" );
            }
            
            theMenu.AddItem( "Sales", "Sales" );

            if( Config.SalesVersion.CompareTo( "S2" ) == 0 )
            {
                theMenu.AddItem( "Payments", "Payments" );
            }

             if( Config.SalesVersion.CompareTo( "S1" ) == 0 || ( Config.SalesVersion.CompareTo( "S2" ) == 0 && ( currentDocument.Division == CurrentDocument.Divisions.FSS )))
             {
                theMenu.AddItem( "Checks", "Checks" );
             }
                
             theMenu.AddItem( "SBA Plans", "SBAPlans" );
        }

        protected void ContractMenu_OnPreRender( object sender, EventArgs e )
        {
            EdgeMenu contractMenu = ( EdgeMenu )sender;
                     
            // re-highlight the selected menu item
            string menuItemValue = "";

            if( Session[ "CurrentSelectedLevel2MenuItemValue" ] != null )
            {
                menuItemValue = ( string )Session[ "CurrentSelectedLevel2MenuItemValue" ];
                foreach( EdgeMenuItem mi in contractMenu.MenuItems )
                {                   
                    if( mi.ItemValue.Contains( menuItemValue ) == true )
                    {
                        mi.Highlighted = true;
                        break;
                    }                   
                }
            }
        }

        protected void ContractViewMasterVendorFormView_OnPreRender( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            Label vendorWebPageLabel = ( Label )ContractViewMasterVendorFormView.FindControl( "VendorWebPage" );
            Label parentVendorWebPageLabel = ( Label )ContractViewMasterVendorFormView.FindControl( "ParentVendorWebPage" );

            if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) == true )
            {
                vendorWebPageLabel.Visible = false;
                parentVendorWebPageLabel.Visible = true;       
            }
            else
            {
                vendorWebPageLabel.Visible = true;
                parentVendorWebPageLabel.Visible = false;
            }
        }

        protected void ContractViewMasterVendorAdministratorFormView_OnPreRender( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            Label vendorAddress1Label = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "VendorAddress1Label" );
            Label parentVendorAddress1Label = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "ParentVendorAddress1Label" );

            Label vendorAddress2Label = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "VendorAddress2Label" );
            Label parentVendorAddress2Label = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "ParentVendorAddress2Label" );

            Label vendorCityLabel = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "VendorCityLabel" );
            Label parentVendorCityLabel = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "ParentVendorCityLabel" );

            Label vendorStateLabel = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "VendorStateLabel" );
            Label parentVendorStateLabel = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "ParentVendorStateLabel" );
            Label vendorCountryLabel = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "VendorCountryLabel" );
            Label parentVendorCountryLabel = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "ParentVendorCountryLabel" );

            Label vendorZipLabel = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "VendorZipLabel" );
            Label parentVendorZipLabel = ( Label )ContractViewMasterVendorAdministratorFormView.FindControl( "ParentVendorZipLabel" );


            if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) == true )
            {
                vendorAddress1Label.Visible = false;
                parentVendorAddress1Label.Visible = true;

                vendorAddress2Label.Visible = false;
                parentVendorAddress2Label.Visible = true;

                vendorCityLabel.Visible = false;
                parentVendorCityLabel.Visible = true;

                if( currentDocument.VendorCountryId == CMGlobals.COUNTRYIDUSA || currentDocument.VendorCountryId == CMGlobals.COUNTRYIDCANADA )
                {
                    vendorStateLabel.Visible = false;
                    parentVendorStateLabel.Visible = true;
                    vendorCountryLabel.Visible = false;
                    parentVendorCountryLabel.Visible = false;
                }
                else
                {
                    vendorStateLabel.Visible = false;
                    parentVendorStateLabel.Visible = false;
                    vendorCountryLabel.Visible = false;
                    parentVendorCountryLabel.Visible = true;
                }

                vendorZipLabel.Visible = false;
                parentVendorZipLabel.Visible = true;
            }
            else
            {
                vendorAddress1Label.Visible = true;
                parentVendorAddress1Label.Visible = false;

                vendorAddress2Label.Visible = true;
                parentVendorAddress2Label.Visible = false;

                vendorCityLabel.Visible = true;
                parentVendorCityLabel.Visible = false;

                if( currentDocument.VendorCountryId == CMGlobals.COUNTRYIDUSA || currentDocument.VendorCountryId == CMGlobals.COUNTRYIDCANADA )
                {
                    vendorStateLabel.Visible = true;
                    parentVendorStateLabel.Visible = false;
                    vendorCountryLabel.Visible = false;
                    parentVendorCountryLabel.Visible = false;
                }
                else
                {
                    vendorStateLabel.Visible = false;
                    parentVendorStateLabel.Visible = false;
                    vendorCountryLabel.Visible = true;
                    parentVendorCountryLabel.Visible = false;
                }

                vendorZipLabel.Visible = true;
                parentVendorZipLabel.Visible = false;
            }
        }

        // markup removed for senior
        //<asp:TableRow SkinID="TableRow"  runat="server" >
        //    <asp:TableHeaderCell  ID="SeniorContractSpecialistHeaderCell" runat="server"  align="right" SkinId="TableHeaderCell" >
        //        <asp:Label ID="SeniorContractSpecialistHeaderLabel" runat="server" Text="Senior Contract Specialist:" SkinID="HeaderLabel"/>
        //    </asp:TableHeaderCell>
        //    <asp:TableCell  ID="SeniorContractSpecialistDataCell"  runat="server" align="left">
        //        <asp:Label ID="SeniorContractSpecialistLabel" runat="server" Text='<%# Eval("SeniorContractSpecialistName") %>' SkinID="HeaderLabel"/>
        //    </asp:TableCell>
        //</asp:TableRow>


        protected void ContractViewMasterContractingOfficerFormView_OnPreRender( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            //TableHeaderCell seniorContractSpecialistHeaderCell = ( TableHeaderCell )ContractViewMasterContractingOfficerFormView.FindControl( "SeniorContractSpecialistHeaderCell" );
            //TableCell seniorContractSpecialistDataCell = ( TableCell )ContractViewMasterContractingOfficerFormView.FindControl( "SeniorContractSpecialistDataCell" );
            //Label seniorContractSpecialistHeaderLabel = ( Label )ContractViewMasterContractingOfficerFormView.FindControl( "SeniorContractSpecialistHeaderLabel" );
        //    Label seniorContractSpecialistLabel = ( Label )ContractViewMasterContractingOfficerFormView.FindControl( "SeniorContractSpecialistLabel" );

            
            //if( currentDocument.Division == CurrentDocument.Divisions.National )
            //{
            //    seniorContractSpecialistHeaderCell.Visible = true;
            //    seniorContractSpecialistDataCell.Visible = true;
            //    seniorContractSpecialistHeaderLabel.Visible = true;
            //    seniorContractSpecialistLabel.Visible = true;
            //}
            //else
            //{
                //seniorContractSpecialistHeaderCell.Visible = false;
                //seniorContractSpecialistDataCell.Visible = false;
                //seniorContractSpecialistHeaderLabel.Visible = false;
                //seniorContractSpecialistLabel.Visible = false;
            //}

            // assistant director is shown as "Division Chief"
            TableHeaderCell assistantDirectorHeaderCell = ( TableHeaderCell )ContractViewMasterContractingOfficerFormView.FindControl( "AssistantDirectorHeaderCell" );
            TableCell assistantDirectorDataCell = ( TableCell )ContractViewMasterContractingOfficerFormView.FindControl( "AssistantDirectorDataCell" );
            Label assistantDirectorHeaderLabel = ( Label )ContractViewMasterContractingOfficerFormView.FindControl( "AssistantDirectorHeaderLabel" );
            Label assistantDirectorLabel = ( Label )ContractViewMasterContractingOfficerFormView.FindControl( "AssistantDirectorLabel" );

            
            if( currentDocument.Division == CurrentDocument.Divisions.National )
            {
                assistantDirectorHeaderCell.Visible = true;
                assistantDirectorDataCell.Visible = true;
                assistantDirectorHeaderLabel.Visible = true;
                assistantDirectorLabel.Visible = true;
            }
            else
            {
                assistantDirectorHeaderCell.Visible = false;
                assistantDirectorDataCell.Visible = false;
                assistantDirectorHeaderLabel.Visible = false;
                assistantDirectorLabel.Visible = false;
            }

        }

        // returns the status, also changes the color of the status control in the parent form view
        protected string ContractStatus( object expirationDateObj, object completionDateObj, string labelControlName )
        {
            Label statusLabel = ( Label )ContractViewMasterVendorFormView.FindControl( labelControlName );
            string status = "";

            status = CMGlobals.GetContractStatus( expirationDateObj, completionDateObj, statusLabel );
            
            return ( status );
        }

        public void AssignObjectDataSourceToHeader( EditedDocumentDataSource editedDocumentDataSource )
        {
            ContractViewMasterVendorFormView.DataSource = editedDocumentDataSource;
            ContractViewMasterVendorFormView.DataKeyNames = new string[] { "ContractId" };

            ContractViewMasterVendorAdministratorFormView.DataSource = editedDocumentDataSource;
            ContractViewMasterVendorAdministratorFormView.DataKeyNames = new string[] { "ContractId" };

            ContractViewMasterContractingOfficerFormView.DataSource = editedDocumentDataSource;
            ContractViewMasterContractingOfficerFormView.DataKeyNames = new string[] { "ContractId" };
        }

        public void BindHeader()
        {
            ContractViewMasterVendorFormView.DataBind();
            ContractViewMasterVendorAdministratorFormView.DataBind();
            ContractViewMasterContractingOfficerFormView.DataBind();
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
                ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
                bSuccess = false;
                ShowException( ex );
            }
            finally
            {
                ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
            }
           

            return ( bSuccess );
        }

        protected void ContractSaveButton_OnClick( object sender, EventArgs e )
        {
            ( ( BaseDocumentEditorPage )this.Page ).ClearErrors();

            try
            {
                ( ( BaseDocumentEditorPage )this.Page ).UpdateDocument();
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
    }
}