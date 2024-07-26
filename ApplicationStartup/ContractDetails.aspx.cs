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
using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ContractDetails : BaseDocumentEditorPage
    {
        public ContractDetails()
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

            ContractDetailsAttributesFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            ContractDetailsAttributesFormView.DataKeyNames = new string[] { "ContractId" };

            DiscountFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            DiscountFormView.DataKeyNames = new string[] { "ContractId" };

            DeliveryFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            DeliveryFormView.DataKeyNames = new string[] { "ContractId" };
        }

        protected void ClearSessionVariables()
        {

        }

        protected void BindFormViews()
        {
            BindHeader();

            ContractDetailsAttributesFormView.DataBind();
            DiscountFormView.DataBind();
            DeliveryFormView.DataBind();
          
            // note form view controls are not yet created here
        }

        protected void ContractDetailsAttributesFormView_OnDataBound( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
            bool bIsContractActive = ( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Active ) ? true : false;

            FormView contractDetailsAttributesFormView = ( FormView )sender;

            base.LoadActiveSolicitations();
           
            DropDownList solicitationDropDownList;
            TextBox solicitationNumberTextBox;
            string selectedSolicitationNumber = "";

            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            EditedDocumentContent formViewContent = ( EditedDocumentContent )contractDetailsAttributesFormView.DataItem;

            if( formViewContent != null )
            {
                selectedSolicitationNumber = formViewContent.SolicitationNumber;
            }

            if( contractDetailsAttributesFormView != null )
            {

                if( documentControlPresentation.IsControlVisibleAndEnabled( "SolicitationDropDownList" ) )
                {

                    solicitationDropDownList = ( DropDownList )contractDetailsAttributesFormView.FindControl( "SolicitationDropDownList" );

                    if( solicitationDropDownList != null )
                    {
                        solicitationDropDownList.ClearSelection();
                        solicitationDropDownList.Items.Clear();

                        foreach( DataRow row in base.ActiveSolicitationsDataSet.Tables[ OfferDB.SolicitationsTableName ].Rows )
                        {
                            string solicitationIdString = row[ "Solicitation_ID" ].ToString();
                            string solicitationNumber = row[ "Solicitation_Number" ].ToString();

                            solicitationDropDownList.Items.Add( new ListItem( solicitationNumber, solicitationIdString ) );
                        }

                        if( solicitationDropDownList.Items.FindByText( selectedSolicitationNumber ) != null )
                        {
                            solicitationDropDownList.Items.FindByText( selectedSolicitationNumber ).Selected = true;
                        }
                        else if( selectedSolicitationNumber.Trim().Length == 0 && bIsContractActive == true ) // null
                        {
                            solicitationDropDownList.Items.FindByText( "--select--" ).Selected = true;   // string must match stored procedure
                        }
                        else if( selectedSolicitationNumber.Trim().Length == 0 && bIsContractActive == false ) // null
                        {
                            solicitationDropDownList.Items.Add( new ListItem( "-- none selected --", "-2" ) );
                            solicitationDropDownList.Items.FindByText( "-- none selected --" ).Selected = true;
                        }
                        else
                        {
                            solicitationDropDownList.Items.Add( new ListItem( selectedSolicitationNumber, "-3" ) );
                            solicitationDropDownList.Items.FindByText( selectedSolicitationNumber ).Selected = true;
                        }
                    }
                }
                else
                {
                    if( documentControlPresentation.IsControlVisibleAndEnabled( "SolicitationNumberTextBox" ) )
                    {

                        solicitationNumberTextBox = ( TextBox )contractDetailsAttributesFormView.FindControl( "SolicitationNumberTextBox" );

                        if( solicitationNumberTextBox != null )
                        {
                            solicitationNumberTextBox.Text = selectedSolicitationNumber;
                        }
                    }
                }
            }
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
            return ( "ContractDetails" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
           DataRelay dataRelay = ( DataRelay )dataRelayInterface;
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           bool bSuccess = true;

           ResetValidationGroup( validationGroupName );

           if( documentControlPresentation.IsFormViewVisibleAndEnabled( "ContractDetailsAttributesFormView" ) == true )
           {

               if( dataRelay.EditedDocumentContentFront.SolicitationId == -1 && dataRelay.EditedDocumentContentFront.SolicitationNumber.Trim().Length == 0 ) 
               {
                   bSuccess = false;
                   AppendValidationError( "A solicitation number is required.", bIsShortSave );
               }

               // no longer collecting this 2/2015
               //if( documentControlPresentation.IsControlVisibleAndEnabled( "IffAbsorbedCheckBox" ) == true )
               //{
               //    if( dataRelay.EditedDocumentContentFront.IffTypeId == -1 )
               //    {
               //        bSuccess = false;
               //        AppendValidationError( "Selection of IFF type is required." );
               //    }
               //}

               if( dataRelay.EditedDocumentContentFront.EstimatedContractValue <= 0 )
               {
                   bSuccess = false;
                   AppendValidationError( "An estimated contract value is required.", bIsShortSave );
               }
           }
           return ( bSuccess );
       }


       public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
       {
           DataRelay dataRelay = ( DataRelay )dataRelayInterface;
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           bool bSuccess = true;

           // Contract Details Tab
           if( documentControlPresentation.IsFormViewVisibleAndEnabled( "ContractDetailsAttributesFormView" ) == true )
           {
               if( documentControlPresentation.IsControlVisibleAndEnabled( "SolicitationDropDownList" ) )
               {
                   DropDownList solicitationDropDownList = ( DropDownList )ContractDetailsAttributesFormView.FindControl( "SolicitationDropDownList" );
                   if( solicitationDropDownList != null )
                   {
                       if( solicitationDropDownList.SelectedItem != null )
                       {
                           dataRelay.EditedDocumentContentFront.SolicitationId = Int32.Parse( solicitationDropDownList.SelectedItem.Value.ToString() );
                           dataRelay.EditedDocumentContentFront.SolicitationNumber = solicitationDropDownList.SelectedItem.Text;
                       }
                       else
                       {
                           dataRelay.EditedDocumentContentFront.SolicitationId = -1;
                           dataRelay.EditedDocumentContentFront.SolicitationNumber = "";
                       }
                   }
               }
               else
               {
                   if( documentControlPresentation.IsControlVisibleAndEnabled( "SolicitationNumberTextBox" ) == true )
                   {
                       TextBox solicitationNumberTextBox = ( TextBox )ContractDetailsAttributesFormView.FindControl( "SolicitationNumberTextBox" );
                       if( solicitationNumberTextBox != null )
                       {
                           dataRelay.EditedDocumentContentFront.SolicitationId = -1;
                           dataRelay.EditedDocumentContentFront.SolicitationNumber = solicitationNumberTextBox.Text;
                       }
                       else
                       {
                           dataRelay.EditedDocumentContentFront.SolicitationId = -1;
                           dataRelay.EditedDocumentContentFront.SolicitationNumber = "";
                       }
                   }
               }

               TextBox trackingCustomerTextBox = ( TextBox )ContractDetailsAttributesFormView.FindControl( "TrackingCustomerTextBox" );
               if( trackingCustomerTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.TrackingCustomerName = trackingCustomerTextBox.Text;
               }

               // no longer collecting this 2/2015
               //if( documentControlPresentation.IsControlVisibleAndEnabled( "IffAbsorbedCheckBox" ) == true )
               //{
               //    CheckBox IFFAbsorbedCheckBox = ( CheckBox )ContractDetailsAttributesFormView.FindControl( "IFFAbsorbedCheckBox" );
               //    if( IFFAbsorbedCheckBox != null )
               //    {
               //        if( IFFAbsorbedCheckBox.Checked == true )
               //        {
               //            dataRelay.EditedDocumentContentFront.IffTypeId = 2;
               //        }
               //    }

               //    CheckBox IffEmbeddedCheckBox = ( CheckBox )ContractDetailsAttributesFormView.FindControl( "IffEmbeddedCheckBox" );
               //    if( IffEmbeddedCheckBox != null )
               //    {
               //        if( IffEmbeddedCheckBox.Checked == true )
               //        {
               //            dataRelay.EditedDocumentContentFront.IffTypeId = 1;
               //        }
               //    }
               //}

               TextBox EstimatedValueTextBox = ( TextBox )ContractDetailsAttributesFormView.FindControl( "EstimatedValueTextBox" );
               if( EstimatedValueTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.EstimatedContractValue = CMGlobals.GetMoneyFromString( EstimatedValueTextBox.Text, "Estimated Contract Value" );
               }

               TextBox MinimumOrderTextBox = ( TextBox )ContractDetailsAttributesFormView.FindControl( "MinimumOrderTextBox" );
               if( MinimumOrderTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.MinimumOrder = MinimumOrderTextBox.Text;
               }

               TextBox FPRDateTextBox = ( TextBox )ContractDetailsAttributesFormView.FindControl( "FPRDateTextBox" );
               if( FPRDateTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.FPRFreeFormatDateString = FPRDateTextBox.Text;
               }

               TextBox RatioTextBox = ( TextBox )ContractDetailsAttributesFormView.FindControl( "RatioTextBox" );
               if( RatioTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.Ratio = RatioTextBox.Text;
               }

           }

           TextBox BasicDiscountTextBox = ( TextBox )DiscountFormView.FindControl( "BasicDiscountTextBox" );
           if( BasicDiscountTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.BasicDiscount = BasicDiscountTextBox.Text;
           }

           TextBox endOfYearDiscountTextBox = ( TextBox )DiscountFormView.FindControl( "EndOfYearDiscountTextBox" );
           if( endOfYearDiscountTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.EndOfYearDiscount = endOfYearDiscountTextBox.Text;
           }

           TextBox QuantityDiscountTextBox = ( TextBox )DiscountFormView.FindControl( "QuantityDiscountTextBox" );
           if( QuantityDiscountTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.QuantityDiscount = QuantityDiscountTextBox.Text;
           }

           TextBox PromptPayTextBox = ( TextBox )DiscountFormView.FindControl( "PromptPayTextBox" );
           if( PromptPayTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.PromptPayDiscount = PromptPayTextBox.Text;
           }

           TextBox CreditCardDiscountTextBox = ( TextBox )DiscountFormView.FindControl( "CreditCardDiscountTextBox" );
           if( CreditCardDiscountTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.CreditCardDiscount = CreditCardDiscountTextBox.Text;
           }

           TextBox AdditionalDiscountTextBox = ( TextBox )DiscountFormView.FindControl( "AdditionalDiscountTextBox" );
           if( AdditionalDiscountTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.AdditionalDiscount = AdditionalDiscountTextBox.Text;
           }

           TextBox StandardDeliveryTextBox = ( TextBox )DeliveryFormView.FindControl( "StandardDeliveryTextBox" );
           if( StandardDeliveryTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.DeliveryTerms = StandardDeliveryTextBox.Text;
           }

           TextBox ExpeditedDeliveryTextBox = ( TextBox )DeliveryFormView.FindControl( "ExpeditedDeliveryTextBox" );
           if( ExpeditedDeliveryTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.ExpeditedDeliveryTerms = ExpeditedDeliveryTextBox.Text;
           }
           return ( bSuccess );
       }

       //protected void ContractDetailsAttributesFormView_OnChange( object sender, EventArgs e )
       //{
       //    SetDirtyFlag( "ContractDetailsAttributesFormView" );
       //}

       //protected void DiscountFormView_OnChange( object sender, EventArgs e )
       //{
       //    SetDirtyFlag( "DiscountFormView" );
       //}

       //protected void DeliveryFormView_OnChange( object sender, EventArgs e )
       //{
       //    SetDirtyFlag( "DiscountFormView" );
       //}

       protected void ContractDetailsAttributesFormView_OnPreRender( object sender, EventArgs e )
       {
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           ContractDetailsAttributesFormView.Visible = documentControlPresentation.IsFormViewVisible( "ContractDetailsAttributesFormView" );
           ContractDetailsAttributesFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "ContractDetailsAttributesFormView" );

           if( documentControlPresentation.IsFormViewVisible( "ContractDetailsAttributesFormView" ) == true )
           {
               Label IffHeaderLabel = ( Label )ContractDetailsAttributesFormView.FindControl( "IffHeaderLabel" );
               if( IffHeaderLabel != null )
               {
                   IffHeaderLabel.Visible = documentControlPresentation.IsControlVisible( "IffHeaderLabel" );
                   IffHeaderLabel.Enabled = documentControlPresentation.IsControlEnabled( "IffHeaderLabel" );
               }

               CheckBox IffAbsorbedCheckBox = ( CheckBox )ContractDetailsAttributesFormView.FindControl( "IffAbsorbedCheckBox" );
               if( IffAbsorbedCheckBox != null )
               {
                   IffAbsorbedCheckBox.Visible = documentControlPresentation.IsControlVisible( "IffAbsorbedCheckBox" );
                   IffAbsorbedCheckBox.Enabled = documentControlPresentation.IsControlEnabled( "IffAbsorbedCheckBox" );                  
               }

               CheckBox IffEmbeddedCheckBox = ( CheckBox )ContractDetailsAttributesFormView.FindControl( "IffEmbeddedCheckBox" );
               if( IffEmbeddedCheckBox != null )
               {
                   IffEmbeddedCheckBox.Visible = documentControlPresentation.IsControlVisible( "IffEmbeddedCheckBox" );
                   IffEmbeddedCheckBox.Enabled = documentControlPresentation.IsControlEnabled( "IffEmbeddedCheckBox" );                
               }
           }

           if( documentControlPresentation.IsFormViewVisible( "ContractDetailsAttributesFormView" ) == true )
           {
               DropDownList solicitationDropDownList = ( DropDownList )ContractDetailsAttributesFormView.FindControl( "SolicitationDropDownList" );
               if( solicitationDropDownList != null )
               {
                   solicitationDropDownList.Visible = documentControlPresentation.IsControlVisible( "SolicitationDropDownList" );
                   solicitationDropDownList.Enabled = documentControlPresentation.IsControlEnabled( "SolicitationDropDownList" );         
               }

               TextBox solicitationNumberTextBox = ( TextBox )ContractDetailsAttributesFormView.FindControl( "SolicitationNumberTextBox" );
               if( solicitationNumberTextBox != null )
               {
                   solicitationNumberTextBox.Visible = documentControlPresentation.IsControlVisible( "SolicitationNumberTextBox" );
                   solicitationNumberTextBox.Enabled = documentControlPresentation.IsControlEnabled( "SolicitationNumberTextBox" );
               }
           }
       }

       protected void DiscountFormView_OnPreRender( object sender, EventArgs e )
       {
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           DiscountFormView.Visible = documentControlPresentation.IsFormViewVisible( "DiscountFormView" );
           DiscountFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "DiscountFormView" );
       }

       protected void DeliveryFormView_OnPreRender( object sender, EventArgs e )
       {
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           DeliveryFormView.Visible = documentControlPresentation.IsFormViewVisible( "DeliveryFormView" );
           DeliveryFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "DeliveryFormView" );
       }

       protected void IffAbsorbedCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox IffAbsorbedCheckBox = ( CheckBox )sender;
           CheckBox IffEmbeddedCheckBox;

           FormView ContractDetailsAttributesFormView;

           if( IffAbsorbedCheckBox != null )
           {
               ContractDetailsAttributesFormView = ( FormView )IffAbsorbedCheckBox.NamingContainer;
               if( ContractDetailsAttributesFormView != null )
               {
                   IffEmbeddedCheckBox = ( CheckBox )ContractDetailsAttributesFormView.FindControl( "IffEmbeddedCheckBox" );

                   if( IffEmbeddedCheckBox != null )
                   {
                       if( IffAbsorbedCheckBox.Checked == true )
                       {
                           IffEmbeddedCheckBox.Checked = false;
                  //         SetDirtyFlag( "ContractDetailsAttributesFormView" );
                           TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }

       protected void IffEmbeddedCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox IffEmbeddedCheckBox = ( CheckBox )sender;
           CheckBox IffAbsorbedCheckBox;

           FormView ContractDetailsAttributesFormView;

           if( IffEmbeddedCheckBox != null )
           {
               ContractDetailsAttributesFormView = ( FormView )IffEmbeddedCheckBox.NamingContainer;
               if( ContractDetailsAttributesFormView != null )
               {
                   IffAbsorbedCheckBox = ( CheckBox )ContractDetailsAttributesFormView.FindControl( "IffAbsorbedCheckBox" );

                   if( IffAbsorbedCheckBox != null )
                   {
                       if( IffEmbeddedCheckBox.Checked == true )
                       {
                           IffAbsorbedCheckBox.Checked = false;
                 //          SetDirtyFlag( "ContractDetailsAttributesFormView" );
                           TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }
    }
}