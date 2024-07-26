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

using VA.NAC.Security.UserRoleObj;
using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.ReportManager;

using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class CreateContract2 : BaseDocumentEditorPage
    {

        public CreateContract2()
            : base( DocumentEditorTypes.NewContract )
        {
  
        }

        protected new void Page_Load( object sender, EventArgs e ) 
        {
            if( Session[ "NACCMStartedProperly" ] == null || Request.UrlReferrer == null )
            {
                Response.Redirect( "~/Start.aspx" );
            }
            
            if( Page.IsPostBack == false ) // && ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
            {
                ClearSessionVariables();
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );

            int offerId = -1;
            int scheduleNumber = -1;
            if( Page.IsPostBack == false )
            {
                // if loading from offer screen, will have the following parameters:
                // ?ScheduleNumber=" & mySchNum & "&OfferId=" & myOfferID & IsCreatedFromOffer=Y/N
                string offerIdString = "";
                string scheduleNumberString = "";
                string isCreatedFromOffer = "";
                if( Request.QueryString.Count > 0 )
                {
                    offerIdString = Request.QueryString.Get( "OfferId" );
                    scheduleNumberString = Request.QueryString.Get( "ScheduleNumber" );
                    isCreatedFromOffer = Request.QueryString.Get( "IsCreatedFromOffer" );
                }

                if( offerIdString.Length > 0 && scheduleNumberString.Length > 0 )
                {
                    offerId = int.Parse( offerIdString );
                    scheduleNumber = int.Parse( scheduleNumberString );
                }

                // override default value of documentEditorType to indicate that it's from an offer
                if( isCreatedFromOffer.CompareTo( "Y" ) == 0 )
                {
                    DocumentEditorType = DocumentEditorTypes.NewContractFromOffer;
                }

            }

            AssignDataSourceToFormViews();

            if( Page.IsPostBack == false )
            {
                if( CurrentDocumentIsChanging == true )
                {
                    DataRelay.Clear();  // prepare for document creation
                }

                // prepopulate the front object with offer data
                if( offerId != -1 )
                {
                    InitContractFromOffer( offerId, scheduleNumber );

                    LoadContractNumberPrefix();
                }

                BindFormViews();

                LoadScreenDefaultDates(); 
            }

            if( Page.IsPostBack == true )
            {
                string refreshDateType = "";
                bool refreshOrNot = false;
                HiddenField refreshDateValueOnSubmitHiddenField = ( HiddenField )CreateContractFormView.FindControl( "RefreshDateValueOnSubmit" );
                HiddenField refreshOrNotOnSubmitHiddenField = ( HiddenField )CreateContractFormView.FindControl( "RefreshOrNotOnSubmit" );

                if( refreshDateValueOnSubmitHiddenField != null )
                {
                    refreshDateType = refreshDateValueOnSubmitHiddenField.Value;
                    if( refreshOrNotOnSubmitHiddenField != null )
                    {
                        refreshOrNot = Boolean.Parse( refreshOrNotOnSubmitHiddenField.Value );

                        if( refreshDateType.Contains( "Undefined" ) == false )
                        {
                            if( refreshOrNot == true )
                            {
                                RefreshDate( refreshDateType );
                            }
                            else
                            {
                                // reset date
                                Session[ refreshDateType ] = Session[ "CalendarInitialDate" ];
                            }
                            refreshDateValueOnSubmitHiddenField.Value = "Undefined";
                            refreshOrNotOnSubmitHiddenField.Value = "False";

                        }
                    }
                }
            }

            bool bUnlimited = false;
            DateTime awardMinDate = DateTime.Now.AddDays( -60 );
            DateTime effectiveMinDate = DateTime.Now.AddDays( -14 );
            DateTime expirationMinDate = DateTime.Now;
            DateTime maxAllowedDate = DateTime.Now.AddYears( 10 );

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "AwardDateButtonOnClickScript", GetDateButtonScript( "X2Award", bUnlimited, awardMinDate, maxAllowedDate ), true );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "EffectiveDateButtonOnClickScript", GetDateButtonScript( "X2Effective", bUnlimited, effectiveMinDate, maxAllowedDate ), true );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ExpirationDateButtonOnClickScript", GetDateButtonScript( "X2Expiration", bUnlimited, expirationMinDate, maxAllowedDate ), true );
        }

        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            CreateContractFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            CreateContractFormView.DataKeyNames = new string[] { "ContractId" };


            VendorPOCFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorPOCFormView.DataKeyNames = new string[] { "ContractId" };


            VendorAddressFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorAddressFormView.DataKeyNames = new string[] { "ContractId" };
        }
  
        protected void BindFormViews()
        {
            BindHeader();

            CreateContractFormView.DataBind();
            VendorPOCFormView.DataBind();
            VendorAddressFormView.DataBind();
        }

        protected void CreateContractFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "CreateContractFormView" );

            CreateContractFormView.Visible = bVisible;
            CreateContractFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "CreateContractFormView" );

            if( bVisible == true )
            {
                documentControlPresentation.UpdatePresentationFromFront( ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ] );
                
                RadioButtonList rebateRequiredRadioButtonList = ( RadioButtonList )CreateContractFormView.FindControl( "RebateRequiredRadioButtonList" );

                rebateRequiredRadioButtonList.Visible = documentControlPresentation.IsControlVisible( "RebateRequiredRadioButtonList" );
                rebateRequiredRadioButtonList.Enabled = documentControlPresentation.IsControlEnabled( "RebateRequiredRadioButtonList" );

                Label parentContractNumberLabel = ( Label )CreateContractFormView.FindControl( "ParentContractNumberLabel" );
                DropDownList parentContractsDropDownList = ( DropDownList )CreateContractFormView.FindControl( "ParentContractsDropDownList" );

                parentContractNumberLabel.Visible = documentControlPresentation.IsControlVisible( "ParentContractNumberLabel" );
                parentContractsDropDownList.Visible = documentControlPresentation.IsControlVisible( "ParentContractsDropDownList" );
                parentContractNumberLabel.Enabled = documentControlPresentation.IsControlEnabled( "ParentContractNumberLabel" );
                parentContractsDropDownList.Enabled = documentControlPresentation.IsControlEnabled( "ParentContractsDropDownList" );

            }
        }

        protected void CreateContractFormView_OnDataBound( object sender, EventArgs e )
        {
       
            FormView createContractFormView = ( FormView )sender;

            EditedDocumentContent formViewContent = ( EditedDocumentContent )createContractFormView.DataItem;

            int selectedDivisionId = -1;
            DropDownList divisionDropDownList = null;

            if( formViewContent != null )
            {
                selectedDivisionId = formViewContent.DivisionId;
            }

            LoadDivisions();

            if( createContractFormView != null )
            {
                divisionDropDownList = ( DropDownList )createContractFormView.FindControl( "DivisionDropDownList" );

                if( divisionDropDownList != null )
                {
                    divisionDropDownList.ClearSelection();
                    divisionDropDownList.Items.Clear();

                    foreach( DataRow row in DivisionsDataSet.Tables[ ContractDB.DivisionsTableName ].Rows )
                    {
                        string divisionIdString = row[ "DivisionId" ].ToString();
                        string divisionName = row[ "DivisionName" ].ToString();

                        divisionDropDownList.Items.Add( new ListItem( divisionName, divisionIdString ) );
                    }

                    if( divisionDropDownList.Items.FindByValue( selectedDivisionId.ToString() ) != null )
                    {
                        //  -1 matches -- select -- choice
                        divisionDropDownList.Items.FindByValue( selectedDivisionId.ToString() ).Selected = true;
                    }
                }
            }


            LoadContractingOfficersForDivision( selectedDivisionId );

            DropDownList contractingOfficerDropDownList = null;
            int selectedCOID = -1;

            if( formViewContent != null )
            {
                selectedCOID = formViewContent.COID;
            }

            if( createContractFormView != null )
            {
                contractingOfficerDropDownList = ( DropDownList )createContractFormView.FindControl( "ContractingOfficerDropDownList" );

                if( contractingOfficerDropDownList != null )
                {
                    contractingOfficerDropDownList.ClearSelection();
                    contractingOfficerDropDownList.Items.Clear();

                    foreach( DataRow row in ContractingOfficersDataSet.Tables[ "UserTable" ].Rows )
                    {
                        string COIDString = row[ "CO_ID" ].ToString();
                        string COFullName = row[ "FullName" ].ToString();

                        contractingOfficerDropDownList.Items.Add( new ListItem( COFullName, COIDString ) );
                    }

                    if( contractingOfficerDropDownList.Items.FindByValue( selectedCOID.ToString() ) != null )
                    {
                        //  -1 matches -- select -- choice
                        contractingOfficerDropDownList.Items.FindByValue( selectedCOID.ToString() ).Selected = true;
                    }
                }
            }

            LoadSchedulesForDivision( selectedDivisionId );

            DropDownList scheduleDropDownList = null;
            int selectedScheduleNumber = -1;

            if( formViewContent != null )
            {
                selectedScheduleNumber = formViewContent.ScheduleNumber;

            }

            if( createContractFormView != null )
            {
                scheduleDropDownList = ( DropDownList )createContractFormView.FindControl( "ScheduleDropDownList" );

                if( scheduleDropDownList != null )
                {
                    scheduleDropDownList.ClearSelection();
                    scheduleDropDownList.Items.Clear();

                    foreach( DataRow row in SchedulesDataSet.Tables[ ContractDB.SchedulesTableName ].Rows )
                    {
                        string scheduleNumberString = row[ "Schedule_Number" ].ToString();
                        string scheduleName = row[ "Schedule_Name" ].ToString();

                        scheduleDropDownList.Items.Add( new ListItem( scheduleName, scheduleNumberString ) );
                    }

                    if( scheduleDropDownList.Items.FindByValue( selectedScheduleNumber.ToString() ) != null )
                    {
                        //  -1 matches -- select -- choice
                        scheduleDropDownList.Items.FindByValue( selectedScheduleNumber.ToString() ).Selected = true;
                    }
                }
            }

            LoadParentContractsForSchedule( selectedScheduleNumber );

            DropDownList parentContractsDropDownList = null;
            string selectedParentContractNumber = "-- select --";
            string contractorName = "";

            if( formViewContent != null )
            {
                selectedParentContractNumber = formViewContent.ParentFSSContractNumber;
            }

            if( createContractFormView != null )
            {
                parentContractsDropDownList = ( DropDownList )createContractFormView.FindControl( "ParentContractsDropDownList" );

                if( parentContractsDropDownList != null )
                {
                    parentContractsDropDownList.ClearSelection();
                    parentContractsDropDownList.Items.Clear();

                    foreach( DataRow row in ParentContractsForScheduleDataSet.Tables[ ContractDB.ParentContractsTableName ].Rows )
                    {
                        string contractNumber = row[ "CntrctNum" ].ToString();
                        contractorName = row[ "Contractor_Name" ].ToString();

                        parentContractsDropDownList.Items.Add( new ListItem( string.Format( "{0}   {1}", contractNumber, contractorName ), contractNumber ) );
                    }

                    if( parentContractsDropDownList.Items.FindByValue( selectedParentContractNumber ) != null )
                    {
                        //  "-- select --" matches -- select -- choice
                        parentContractsDropDownList.Items.FindByValue( selectedParentContractNumber ).Selected = true;                      
                    }
                }
            }

        }

        protected void VendorPOCFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "VendorPOCFormView" );

            VendorPOCFormView.Visible = bVisible;
            VendorPOCFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorPOCFormView" );
        }

        protected void VendorPOCFormView_OnDataBound( object sender, EventArgs e )
        {
   
        }

        protected void VendorAddressFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "VendorAddressFormView" );

            VendorAddressFormView.Visible = bVisible;
            VendorAddressFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorAddressFormView" );
        }

        protected void VendorAddressFormView_OnDataBound( object sender, EventArgs e )
        {
            FormView vendorAddressFormView = ( FormView )sender;

            EditedDocumentContent formViewContent = ( EditedDocumentContent )vendorAddressFormView.DataItem;

            int selectedVendorCountryId = -1;

            base.LoadCountries();
            DataSet dsCountries = ( DataSet )Cache[ PointsOfContactVendorCountriesCacheName ];
            DropDownList countryDropDownList;

            if( formViewContent != null )
            {
                selectedVendorCountryId = formViewContent.VendorCountryId;
            }

            if( vendorAddressFormView != null )
            {
                countryDropDownList = ( DropDownList )vendorAddressFormView.FindControl( "CountryDropDownList" );

                if( countryDropDownList != null )
                {
                    countryDropDownList.ClearSelection();
                    countryDropDownList.Items.Clear();

                    foreach( DataRow row in dsCountries.Tables[ ContractDB.VendorCountriesTableName ].Rows )
                    {
                        string countryName = row[ "CountryName" ].ToString();
                        string countryIdString = row[ "CountryId" ].ToString();
                        countryDropDownList.Items.Add( new ListItem( countryName, countryIdString ) );
                    }

                    if( countryDropDownList.Items.FindByValue( selectedVendorCountryId.ToString() ) != null )
                    {
                        //  -1 matches -- select -- choice
                        countryDropDownList.Items.FindByValue( selectedVendorCountryId.ToString() ).Selected = true;
                    }
                }
            }

            LoadStates( selectedVendorCountryId );

            DropDownList stateDropDownList = null;
            string selectedStateAbbreviation = "--";

            if( formViewContent != null )
            {
                selectedStateAbbreviation = formViewContent.VendorState;
            }

            if( vendorAddressFormView != null )
            {
                stateDropDownList = ( DropDownList )vendorAddressFormView.FindControl( "StateDropDownList" );

                if( stateDropDownList != null )
                {
                    stateDropDownList.ClearSelection();
                    stateDropDownList.Items.Clear();

                    foreach( DataRow row in PointsOfContactStateCodeDataSet.Tables[ ContractDB.StateCodesTableName ].Rows )
                    {
                        string stateAbbreviation = row[ "StateAbbreviation" ].ToString();
                        string stateName = row[ "StateName" ].ToString();
                        string stateAbbreviationValue = ( stateAbbreviation.CompareTo( "--" ) != 0 ) ? stateAbbreviation : "";
                        stateDropDownList.Items.Add( new ListItem( stateAbbreviation, stateAbbreviationValue ) );
                    }

                    if( stateDropDownList.Items.FindByValue( selectedStateAbbreviation ) != null )
                    {
                        // -- defaults to -- = no selection
                        stateDropDownList.Items.FindByValue( selectedStateAbbreviation ).Selected = true;
                    }
                }
            }

        }

        protected void CountryDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList businessCountryDropDownList = ( DropDownList )sender;
            ListItem selectedItem = businessCountryDropDownList.SelectedItem;
            int selectedBusinessCountryId = int.Parse( selectedItem.Value );

            // save the selection
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            if( editedDocumentContentFront != null )
            {
                editedDocumentContentFront.VendorCountryId = selectedBusinessCountryId;
            }

            // reset associated state drop down list  
            DropDownList stateDropDownList = ( DropDownList )VendorAddressFormView.FindControl( "StateDropDownList" );
            if( stateDropDownList != null )
            {
                stateDropDownList.DataBind();
            }

            // allow the postback to occur 
            CreateContract2UpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }
        protected void BusinessSizeRadioButtonList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            Session[ "BusinessSizeRadioButtonChanged" ] = true;
        }

        protected void BusinessSizeRadioButtonList_OnDataBinding( object sender, EventArgs e )
        {
            RadioButtonList businessSizeRadioButtonList = ( RadioButtonList )sender;

            FormView createContractFormView;

            if( businessSizeRadioButtonList != null )
            {
                createContractFormView = ( FormView )businessSizeRadioButtonList.NamingContainer;

                if( createContractFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )createContractFormView.DataItem;

                    int socioBusinessSizeId = editedDocumentContent.SocioBusinessSizeId;

                    if( Session[ "BusinessSizeRadioButtonChanged" ] == null )
                    {
                        businessSizeRadioButtonList.SelectedIndex = -1; 
                    }
                    else
                    {
                        businessSizeRadioButtonList.SelectedValue = ( socioBusinessSizeId == 1 ) ? "1" : "2";
                    }
                }
            }
        }

        protected void RebateRequiredRadioButtonList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            Session[ "RebateRequiredRadioButtonChanged" ] = true;
        }

        protected void RebateRequiredRadioButtonList_OnDataBinding( object sender, EventArgs e )
        {
            RadioButtonList rebateRequiredRadioButtonList = ( RadioButtonList )sender;

            FormView createContractFormView;

            if( rebateRequiredRadioButtonList != null )
            {
                createContractFormView = ( FormView )rebateRequiredRadioButtonList.NamingContainer;

                if( createContractFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )createContractFormView.DataItem;

                    bool bRebateRequired = editedDocumentContent.RebateRequired;

                    if( Session[ "RebateRequiredRadioButtonChanged" ] == null )
                    {
                        rebateRequiredRadioButtonList.SelectedIndex = -1;
                    }
                    else
                    {
                        rebateRequiredRadioButtonList.SelectedValue = ( bRebateRequired == true ) ? "1" : "0";
                    }
                }
            }
        }

        protected void AwardDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox awardDateTextBox = ( TextBox )sender;

            FormView contractGeneralContractDatesFormView;

            if( awardDateTextBox != null )
            {
                contractGeneralContractDatesFormView = ( FormView )awardDateTextBox.NamingContainer;

                if( contractGeneralContractDatesFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )contractGeneralContractDatesFormView.DataItem;

                    DateTime contractAwardDate = editedDocumentContent.ContractAwardDate;

                    if( contractAwardDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        awardDateTextBox.Text = "";
                    }
                    else
                    {
                        awardDateTextBox.Text = contractAwardDate.ToString( "MM/dd/yyyy" );
                        Session[ "X2Award" ] = contractAwardDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

 
        protected void EffectiveDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox effectiveDateTextBox = ( TextBox )sender;

            FormView contractGeneralContractDatesFormView;

            if( effectiveDateTextBox != null )
            {
                contractGeneralContractDatesFormView = ( FormView )effectiveDateTextBox.NamingContainer;

                if( contractGeneralContractDatesFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )contractGeneralContractDatesFormView.DataItem;

                    DateTime contractEffectiveDate = editedDocumentContent.ContractEffectiveDate;

                    if( contractEffectiveDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        effectiveDateTextBox.Text = "";
                    }
                    else
                    {
                        effectiveDateTextBox.Text = contractEffectiveDate.ToString( "MM/dd/yyyy" );
                        Session[ "X2Effective" ] = contractEffectiveDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

        protected void ExpirationDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox expirationDateTextBox = ( TextBox )sender;

            FormView contractGeneralContractDatesFormView;

            if( expirationDateTextBox != null )
            {
                contractGeneralContractDatesFormView = ( FormView )expirationDateTextBox.NamingContainer;

                if( contractGeneralContractDatesFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )contractGeneralContractDatesFormView.DataItem;

                    DateTime contractExpirationDate = editedDocumentContent.ContractExpirationDate;

                    if( contractExpirationDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        expirationDateTextBox.Text = "";
                    }
                    else
                    {
                        expirationDateTextBox.Text = contractExpirationDate.ToString( "MM/dd/yyyy" );
                        Session[ "X2Expiration" ] = contractExpirationDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

  
        protected bool ValidateContractNumberFormat( string contractNumber, int selectedScheduleNumber, ref string formatError )
        {
            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];

            bool bIsValidated = false;
            string validationMessage = "";
            bool bSuccess = contractDB.ValidateContractNumber( contractNumber, selectedScheduleNumber, ref bIsValidated, ref validationMessage );

            if( bSuccess == true )
            {
                if( bIsValidated == false )
                    formatError = validationMessage;
            }
            else
            {
               ShowException( new Exception( string.Format( "The following database error was encountered when validating the contract number format: {0}", contractDB.ErrorMessage )));
            }

            return ( bIsValidated );
        }

       // currently, the create screen is a single screen and short save via tabbing will not be used 
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
            return ( "CreateContract2" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;
            
            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            if( dataRelay.EditedDocumentContentFront.DivisionId == -1 )
            {
                AppendValidationError( "Division selection is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.ScheduleNumber == -1 )
            {
                AppendValidationError( "Schedule selection is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.COID == -1 )
            {
                AppendValidationError( "Contracting officer selection is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.ContractNumber.Length == 0 )
            {
                AppendValidationError( "Contract number is required.", bIsShortSave );
                bSuccess = false;
            }

            string formatErrorMessage = "";
            if( ValidateContractNumberFormat( dataRelay.EditedDocumentContentFront.ContractNumber, dataRelay.EditedDocumentContentFront.ScheduleNumber, ref formatErrorMessage ) == false )
            {
                AppendValidationError( formatErrorMessage, bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.ContractAwardDate == null || DateTime.MinValue.CompareTo( dataRelay.EditedDocumentContentFront.ContractAwardDate ) == 0 )
            {              
                AppendValidationError( "Award date is required.", bIsShortSave );
                bSuccess = false;               
            }

            if( dataRelay.EditedDocumentContentFront.ContractEffectiveDate == null || DateTime.MinValue.CompareTo( dataRelay.EditedDocumentContentFront.ContractEffectiveDate ) == 0 )
            {
                AppendValidationError( "Effective date is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.ContractExpirationDate == null || DateTime.MinValue.CompareTo( dataRelay.EditedDocumentContentFront.ContractExpirationDate ) == 0 )
            {
                AppendValidationError( "Expiration date is required.", bIsShortSave );
                bSuccess = false;
            }

            if( Session[ "BusinessSizeRadioButtonChanged" ] == null )
            {
                AppendValidationError( "Business size selection is required.", bIsShortSave );
                bSuccess = false;
            }

            if( CurrentDocument.CanHaveRebates( dataRelay.EditedDocumentContentFront.ScheduleNumber, 0 ) == true )
            {
                if( Session[ "RebateRequiredRadioButtonChanged" ] == null )
                {
                    AppendValidationError( "Rebate requirement selection is required.", bIsShortSave );
                    bSuccess = false;
                }
            }
      
            if( CurrentDocument.IsBPA( dataRelay.EditedDocumentContentFront.ScheduleNumber, 0 ) == true )
            {
                if( dataRelay.EditedDocumentContentFront.ParentFSSContractNumber.Trim().Length == 0 )
                {
                    AppendValidationError( "Parent contract selection is required.", bIsShortSave );
                    bSuccess = false;
                }
            }

            if( dataRelay.EditedDocumentContentFront.VendorName.Trim().Length == 0 )
            {
                AppendValidationError( "Vendor name is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.VendorPrimaryContactName.Trim().Length == 0 )
            {
                AppendValidationError( "Vendor primary contact name is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.VendorAddress1.Trim().Length == 0 )
            {
                AppendValidationError( "Vendor addresss is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.VendorCity.Trim().Length == 0 )
            {
                AppendValidationError( "Vendor city is required.", bIsShortSave );
                bSuccess = false;
            }
            
            int vendorCountryId = dataRelay.EditedDocumentContentFront.VendorCountryId;
            if( vendorCountryId == -1 )
            {
                AppendValidationError( "Vendor country is required.", bIsShortSave );
                bSuccess = false;
            }

            if( vendorCountryId == CMGlobals.COUNTRYIDUSA || vendorCountryId == CMGlobals.COUNTRYIDCANADA )
            {
                if( dataRelay.EditedDocumentContentFront.VendorState.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor state is required.", bIsShortSave );
                    bSuccess = false;
                }

                if( dataRelay.EditedDocumentContentFront.VendorZip.Trim().Length == 0 )
                {
                    AppendValidationError( "Vendor zip is required.", bIsShortSave );
                    bSuccess = false;
                }
            }

            // zip 5 or 10
            if( vendorCountryId == CMGlobals.COUNTRYIDUSA )
            {
                if( dataRelay.EditedDocumentContentFront.VendorZip.Trim().Length > 0 )
                {
                    if( dataRelay.EditedDocumentContentFront.VendorZip.Trim().Length == 5 )
                    {
                        int tempZip = 0;
                        if( int.TryParse( dataRelay.EditedDocumentContentFront.VendorZip.Trim(), out tempZip ) == false )
                        {
                            AppendValidationError( "Vendor zip is not formatted correctly.", bIsShortSave );
                            bSuccess = false;
                        }
                    }
                    else if( dataRelay.EditedDocumentContentFront.VendorZip.Trim().Length == 10 )
                    {
                        string leftZip = "";
                        string dash = "";
                        string rightZip = "";

                        leftZip = dataRelay.EditedDocumentContentFront.VendorZip.Trim().Substring( 0, 5 );
                        dash = dataRelay.EditedDocumentContentFront.VendorZip.Trim().Substring( 5, 1 );
                        rightZip = dataRelay.EditedDocumentContentFront.VendorZip.Trim().Substring( 6, 4 );

                        int tempZip = 0;
                        if( dash.CompareTo( "-" ) != 0 || int.TryParse( leftZip, out tempZip ) == false || int.TryParse( rightZip, out tempZip ) == false )
                        {
                            AppendValidationError( "Vendor zip is not formatted correctly.", bIsShortSave );
                            bSuccess = false;
                        }
                    }
                    else
                    {
                        AppendValidationError( "Vendor zip is not formatted correctly.", bIsShortSave );
                        bSuccess = false;
                    }
                }
            }
       

            if( dataRelay.EditedDocumentContentFront.VendorPrimaryContactEmail.Trim().Length == 0 )
            {
                AppendValidationError( "Vendor email is required.", bIsShortSave );
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
               // CreateContractFormView
                DropDownList divisionDropDownList = ( DropDownList )CreateContractFormView.FindControl( "DivisionDropDownList" );
               if( divisionDropDownList != null )
               {
                   if( divisionDropDownList.SelectedItem != null )
                   {
                       dataRelay.EditedDocumentContentFront.DivisionId = Int32.Parse( divisionDropDownList.SelectedItem.Value.ToString() );
                   }
               }

               DropDownList scheduleDropDownList = ( DropDownList )CreateContractFormView.FindControl( "ScheduleDropDownList" );
               if( scheduleDropDownList != null )
               {
                   if( scheduleDropDownList.SelectedItem != null )
                   {
                       dataRelay.EditedDocumentContentFront.ScheduleNumber = Int32.Parse( scheduleDropDownList.SelectedItem.Value.ToString() );
                   }
               }

               DropDownList contractingOfficerDropDownList = ( DropDownList )CreateContractFormView.FindControl( "ContractingOfficerDropDownList" );
               if( contractingOfficerDropDownList != null )
               {
                   if( contractingOfficerDropDownList.SelectedItem != null )
                   {
                       dataRelay.EditedDocumentContentFront.COID = Int32.Parse( contractingOfficerDropDownList.SelectedItem.Value.ToString() );
                   }
               }

               TextBox contractNumberTextBox = ( TextBox )CreateContractFormView.FindControl( "ContractNumberTextBox" );
               if( contractNumberTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.ContractNumber = contractNumberTextBox.Text;
               }

               DropDownList parentContractsDropDownList = ( DropDownList )CreateContractFormView.FindControl( "ParentContractsDropDownList" );
               if( parentContractsDropDownList != null )
               {
                   if( parentContractsDropDownList.SelectedItem != null )
                   {
                       if( parentContractsDropDownList.SelectedItem.Value.ToString().Trim().Length > 0 )
                       {
                           if( parentContractsDropDownList.SelectedItem.Value.ToString().Trim().CompareTo( "-- select --" ) != 0 )
                           {
                               dataRelay.EditedDocumentContentFront.ParentFSSContractNumber = parentContractsDropDownList.SelectedItem.Value.ToString();
                           }
                       }
                   }
               }

               TextBox awardDateTextBox = ( TextBox )CreateContractFormView.FindControl( "AwardDateTextBox" );
               if( awardDateTextBox != null )
               {
                   string awardDateString = awardDateTextBox.Text;
                   DateTime awardDate;
                   if( DateTime.TryParse( awardDateString, out awardDate ) != true )
                   {
                       // throw new Exception( "Award Date is not a valid date." );
                       ErrorMessage = "Award Date is not a valid date.";
                       bSuccess = false;
                   }
                   dataRelay.EditedDocumentContentFront.ContractAwardDate = awardDate;
               }

               TextBox effectiveDateTextBox = ( TextBox )CreateContractFormView.FindControl( "EffectiveDateTextBox" );
               if( effectiveDateTextBox != null )
               {
                   string effectiveDateString = effectiveDateTextBox.Text;
                   DateTime effectiveDate;
                   if( DateTime.TryParse( effectiveDateString, out effectiveDate ) != true )
                   {
                       // throw new Exception( "Effective Date is not a valid date." );
                       ErrorMessage = "Effective Date is not a valid date.";
                       bSuccess = false;
                   }
                   dataRelay.EditedDocumentContentFront.ContractEffectiveDate = effectiveDate;
               }

               TextBox expirationDateTextBox = ( TextBox )CreateContractFormView.FindControl( "ExpirationDateTextBox" );
               if( expirationDateTextBox != null )
               {
                   string expirationDateString = expirationDateTextBox.Text;
                   DateTime expirationDate;
                   if( DateTime.TryParse( expirationDateString, out expirationDate ) != true )
                   {
                       // throw new Exception( "Expiration Date is not a valid date." );
                       ErrorMessage = "Expiration Date is not a valid date.";
                       bSuccess = false;
                   }
                   dataRelay.EditedDocumentContentFront.ContractExpirationDate = expirationDate;
               }

               TextBox vendorNameTextBox = ( TextBox )CreateContractFormView.FindControl( "VendorNameTextBox" );
               if( vendorNameTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorName = vendorNameTextBox.Text;
               }

               RadioButtonList businessSizeRadioButtonList = ( RadioButtonList )CreateContractFormView.FindControl( "BusinessSizeRadioButtonList" );
               int socioBusinessSizeId = -1;
               if( businessSizeRadioButtonList != null )
               {
                   socioBusinessSizeId = ( businessSizeRadioButtonList.SelectedValue == "1" ) ? 1 : 2;
               }
               dataRelay.EditedDocumentContentFront.SocioBusinessSizeId = socioBusinessSizeId;
             
               RadioButtonList rebateRequiredRadioButtonList = ( RadioButtonList )CreateContractFormView.FindControl( "RebateRequiredRadioButtonList" );
               bool bIsRebateRequired = false;
               if( rebateRequiredRadioButtonList != null )
               {
                   bIsRebateRequired = ( rebateRequiredRadioButtonList.SelectedValue == "1" ) ? true : false;
               }
               dataRelay.EditedDocumentContentFront.RebateRequired = bIsRebateRequired;

               // VendorPOCFormView
               TextBox pointOfContactNameTextBox = ( TextBox )VendorPOCFormView.FindControl( "PointOfContactNameTextBox" );
               if( pointOfContactNameTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactName = pointOfContactNameTextBox.Text;
               }

               TextBox pointOfContactPhoneTextBox = ( TextBox )VendorPOCFormView.FindControl( "PointOfContactPhoneTextBox" );
               if( pointOfContactPhoneTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactPhone = pointOfContactPhoneTextBox.Text;
               }

               TextBox pointOfContactPhoneExtensionTextBox = ( TextBox )VendorPOCFormView.FindControl( "PointOfContactPhoneExtensionTextBox" );
               if( pointOfContactPhoneExtensionTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactExtension = pointOfContactPhoneExtensionTextBox.Text;
               }

               TextBox pointOfContactFaxTextBox = ( TextBox )VendorPOCFormView.FindControl( "PointOfContactFaxTextBox" );
               if( pointOfContactFaxTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactFax = pointOfContactFaxTextBox.Text;
               }

               TextBox pointOfContactEmailTextBox = ( TextBox )VendorPOCFormView.FindControl( "PointOfContactEmailTextBox" );
               if( pointOfContactEmailTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorPrimaryContactEmail = pointOfContactEmailTextBox.Text;
               }

               // VendorAddressFormView
               TextBox address1TextBox = ( TextBox )VendorAddressFormView.FindControl( "Address1TextBox" );
               if( address1TextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorAddress1 = address1TextBox.Text;
               }

               TextBox address2TextBox = ( TextBox )VendorAddressFormView.FindControl( "Address2TextBox" );
               if( address2TextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorAddress2 = address2TextBox.Text;
               }

               TextBox cityTextBox = ( TextBox )VendorAddressFormView.FindControl( "CityTextBox" );
               if( cityTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorCity = cityTextBox.Text;
               }

                DropDownList countryDropDownList = ( DropDownList )VendorAddressFormView.FindControl( "CountryDropDownList" );
                if( countryDropDownList != null )
                {
                    if( countryDropDownList.SelectedItem != null )
                    {
                        dataRelay.EditedDocumentContentFront.VendorCountryId = int.Parse( countryDropDownList.SelectedItem.Value.ToString() );
                        dataRelay.EditedDocumentContentFront.VendorCountryName = countryDropDownList.SelectedItem.Text;
                    }
                }

                DropDownList stateDropDownList = ( DropDownList )VendorAddressFormView.FindControl( "StateDropDownList" );
               if( stateDropDownList != null )
               {
                   if( stateDropDownList.SelectedItem != null )
                   {
                       dataRelay.EditedDocumentContentFront.VendorState = stateDropDownList.SelectedItem.Value.ToString();
                   }
               }

               TextBox zipTextBox = ( TextBox )VendorAddressFormView.FindControl( "ZipTextBox" );
               if( zipTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorZip = zipTextBox.Text;
               }

               TextBox companyUrlTextBox = ( TextBox )VendorAddressFormView.FindControl( "CompanyUrlTextBox" );
               if( companyUrlTextBox != null )
               {
                   dataRelay.EditedDocumentContentFront.VendorWebAddress = companyUrlTextBox.Text;
               }

               // no longer collecting this, so default it here
               dataRelay.EditedDocumentContentFront.IffEmbedded = true;
 
            }
            catch( Exception ex )
            {
                bSuccess = false;
                ErrorMessage = string.Format( "The following exception was encountered validating the contract information: {0}", ex.Message );
            }

            return ( bSuccess );
       }

       protected void CreateContractFormView_OnChange( object sender, EventArgs e )
       {
    //       SetDirtyFlag( "CreateContractFormView" );
       }


    
        protected void ContractAdditionCancelButton_OnClick( object sender, EventArgs e )
        {
            if( Session[ "CreateFromOffer" ] != null )
            {
                string offerIdString = Session[ "CreateFromOffer" ].ToString();
                if( offerIdString.Length > 0 )
                {
                    Response.Write( "<script>window.close();</script>" );
                }
                else
                {
                    Response.Redirect( "CM_Splash.htm" );
                }
            }
            else
            {
                Response.Redirect( "CM_Splash.htm" );
            }
        }


        protected void DivisionDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {

            DropDownList divisionDropDownList = ( DropDownList )sender;
            ListItem selectedItem = divisionDropDownList.SelectedItem;
            int selectedDivisionId = int.Parse( selectedItem.Value );

            // save the selection
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            if( editedDocumentContentFront != null )
            {
                editedDocumentContentFront.DivisionId = selectedDivisionId;
            }

            // clear other related selections
            editedDocumentContentFront.ScheduleNumber = -1;
            editedDocumentContentFront.COID = -1;
            editedDocumentContentFront.ParentFSSContractNumber = "";

            // reset other associated drop down lists
            CreateContractFormView.DataBind();

            // trying this for update panel refresh since there is no nested update panel - may have to add one $$$
            TriggerViewMasterUpdatePanel();
        }


        protected void ScheduleDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList scheduleDropDownList = ( DropDownList )sender;
            ListItem selectedItem = scheduleDropDownList.SelectedItem;
            int selectedScheduleNumber = int.Parse( selectedItem.Value );

            // save the selection
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            if( editedDocumentContentFront != null )
            {
                editedDocumentContentFront.ScheduleNumber = selectedScheduleNumber;
            }

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            string prefix = "";
            bool bSuccess = contractDB.GetNewContractPrefix( selectedScheduleNumber, ref prefix );
            if( bSuccess == true )
            {
                editedDocumentContentFront.ContractNumber = prefix;
            }

            // clear other related selections 
            editedDocumentContentFront.COID = -1;
            editedDocumentContentFront.ParentFSSContractNumber = "";

            // reset other associated drop down lists
            CreateContractFormView.DataBind();

            // trying this for update panel refresh since there is no nested update panel - may have to add one $$$
            TriggerViewMasterUpdatePanel();
        }

        protected void ContractingOfficerDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList contractingOfficerDropDownList = ( DropDownList )sender;
            ListItem selectedItem = contractingOfficerDropDownList.SelectedItem;
            int selectedCOID = int.Parse( selectedItem.Value );

            // save the selection
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            if( editedDocumentContentFront != null )
            {
                editedDocumentContentFront.COID = selectedCOID;
            }

            // trying this for update panel refresh since there is no nested update panel - may have to add one $$$
            //TriggerViewMasterUpdatePanel();
        }

        protected void ParentContractsDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList parentContractsDropDownList = ( DropDownList )sender;
            ListItem selectedItem = parentContractsDropDownList.SelectedItem;
            string selectedParentContractNumber = selectedItem.Value;

            // save the selection
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            if( editedDocumentContentFront != null )
            {
                editedDocumentContentFront.ParentFSSContractNumber = selectedParentContractNumber;

                // grab the contents of the text box before bind
                TextBox contractNumberTextBox = ( TextBox )CreateContractFormView.FindControl( "ContractNumberTextBox" );
                if( contractNumberTextBox != null )
                {
                    editedDocumentContentFront.ContractNumber = contractNumberTextBox.Text;
                }
            }

            // pre-populates vendor info into BPA
            GetParentContractAndVendorInfoForBPA( selectedParentContractNumber );

            // binds other associated textboxes
            CreateContractFormView.DataBind();
            VendorPOCFormView.DataBind();
            VendorAddressFormView.DataBind();

            // trying this for update panel refresh since there is no nested update panel - may have to add one $$$
            TriggerViewMasterUpdatePanel();
        }

        public string GetDateButtonScript( string dateTypeString, bool bUnlimitedDateRange, DateTime minAllowedDate, DateTime maxAllowedDate )
        {
            string defaultDateString = "";

            if( Session[ dateTypeString ] != null )
            {
                defaultDateString = Session[ dateTypeString ].ToString();
            }
            else
            {
                defaultDateString = DateTime.Today.ToShortDateString();
            }

            string minAllowedDateString = minAllowedDate.ToShortDateString();
            string maxAllowedDateString = maxAllowedDate.ToShortDateString();

            string script = String.Format( "function On{0}DateButtonClick() {{ window.open('Calendar.aspx?InitialDate={1}&DateType={2}&Unlimited={3}&MinAllowedDate={4}&MaxAllowedDate={5}','Calendar','toolbar=no,menubar=no,resizable=no,scrollbars=no,statusbar=0,location=0,width=250,height=340,left=660,top=300'); return false;}}", dateTypeString, defaultDateString, dateTypeString, ( ( bUnlimitedDateRange == true ) ? 1 : 0 ), minAllowedDateString, maxAllowedDateString );

            return ( script );
        }

        private void RefreshDate( string dateTypeString )
        {
            DateTime displayDate;

            if( dateTypeString.Contains( "X2Award" ) == true )
            {
                TextBox awardDateTextBox = ( TextBox )CreateContractFormView.FindControl( "AwardDateTextBox" );
                if( Session[ "X2Award" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "X2Award" ].ToString() );
                    awardDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    awardDateTextBox.Text = "x";
                }
            }
            if( dateTypeString.Contains( "X2Effective" ) == true )
            {
                TextBox effectiveDateTextBox = ( TextBox )CreateContractFormView.FindControl( "EffectiveDateTextBox" );
                if( Session[ "X2Effective" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "X2Effective" ].ToString() );
                    effectiveDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    effectiveDateTextBox.Text = "x";
                }
            }
            if( dateTypeString.Contains( "X2Expiration" ) == true )
            {
                TextBox expirationDateTextBox = ( TextBox )CreateContractFormView.FindControl( "ExpirationDateTextBox" );
                if( Session[ "X2Expiration" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "X2Expiration" ].ToString() );
                    expirationDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    expirationDateTextBox.Text = "x";
                }
            }
        }

        // needed when creating from offer
        private void LoadContractNumberPrefix()
        {
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            
            if( editedDocumentContentFront != null )
            {
                int selectedScheduleNumber = editedDocumentContentFront.ScheduleNumber;

                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                string prefix = "";
                bool bSuccess = contractDB.GetNewContractPrefix( selectedScheduleNumber, ref prefix );
                if( bSuccess == true )
                {
                    editedDocumentContentFront.ContractNumber = prefix;
                }
            }
        }

        private void LoadScreenDefaultDates()
        {
            DateTime awardDate = DateTime.Today;
            DateTime effectiveDate;
            DateTime expirationDate;

            effectiveDate = CMGlobals.GetNextEffectiveDate( DateTime.Today );
            expirationDate = CMGlobals.GetExpirationDate( effectiveDate );

            // for calendar control InitialDate
            Session[ "X2Award" ] = awardDate.ToShortDateString();
            Session[ "X2Effective" ] = effectiveDate.ToShortDateString();
            Session[ "X2Expiration" ] = expirationDate.ToShortDateString();

            // save the defaults
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            if( editedDocumentContentFront != null )
            {
                editedDocumentContentFront.ContractAwardDate = awardDate;
                editedDocumentContentFront.ContractEffectiveDate = effectiveDate;
                editedDocumentContentFront.ContractExpirationDate = expirationDate;
            }

            TextBox awardDateTextBox = ( TextBox )CreateContractFormView.FindControl( "AwardDateTextBox" );
            TextBox effectiveDateTextBox = ( TextBox )CreateContractFormView.FindControl( "EffectiveDateTextBox" );
            TextBox expirationDateTextBox = ( TextBox )CreateContractFormView.FindControl( "ExpirationDateTextBox" );
            
            awardDateTextBox.Text = awardDate.ToShortDateString();
            effectiveDateTextBox.Text = effectiveDate.ToShortDateString();
            expirationDateTextBox.Text = expirationDate.ToShortDateString();
        }

        private void ClearSessionVariables()
        {
            Session[ "X2Award" ] = null;
            Session[ "X2Effective" ] = null;
            Session[ "X2Expiration" ] = null;
            Session[ "CreateContractValidationErrorMessage" ] = null;
            Session[ "RebateRequiredRadioButtonChanged" ] = null;
            Session[ "BusinessSizeRadioButtonChanged" ] = null;
        }

        protected void VendorPOCFormView_OnChange( object sender, EventArgs e )
        {
   //         SetDirtyFlag( "VendorPOCFormView" );
        }

        protected void VendorAddressFormView_OnChange( object sender, EventArgs e )
        {
    //        SetDirtyFlag( "VendorAddressFormView" );
        }

        protected void StateDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList stateDropDownList = ( DropDownList )sender;

            int selectedVendorCountryId;
            string selectedStateAbbreviation = "--";

            if( stateDropDownList != null )
            {
                
                EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];

                if( editedDocumentContentFront != null )
                {
                    selectedVendorCountryId = editedDocumentContentFront.VendorCountryId;
                    selectedStateAbbreviation = editedDocumentContentFront.VendorState;

                    LoadStates( selectedVendorCountryId );

                    stateDropDownList.ClearSelection();
                    stateDropDownList.Items.Clear();

                    foreach( DataRow row in PointsOfContactStateCodeDataSet.Tables[ ContractDB.StateCodesTableName ].Rows )
                    {
                        string stateAbbreviation = row[ "StateAbbreviation" ].ToString();
                        string stateName = row[ "StateName" ].ToString();
                        string stateAbbreviationValue = ( stateAbbreviation.CompareTo( "--" ) != 0 ) ? stateAbbreviation : "";
                        stateDropDownList.Items.Add( new ListItem( stateAbbreviation, stateAbbreviationValue ) );
                    }

                    if( stateDropDownList.Items.FindByText( selectedStateAbbreviation ) != null )
                    {
                        // -- defaults to -- = no selection
                        stateDropDownList.Items.FindByText( selectedStateAbbreviation ).Selected = true;
                    }
                }                
            }
        }
    }
}
