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

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class CreateContract : System.Web.UI.Page
    {
        [Serializable]
        public class ContractCreationInfo
        {
            public int OfferId;
            public int AssignedCOID;
            public int DivisionId;
            public int ScheduleNumber;
            public bool IsRebateRequired;
            public string VendorName;
            public string VendorAddress1;
            public string VendorAddress2;
            public string VendorAddressCity;
            public string VendorAddressState;
            public string VendorZipCode;
            public string VendorUrl;
            public string VendorContactName;
            public string VendorContactPhone;
            public string VendorContactPhoneExtension;
            public string VendorContactFax;
            public string VendorContactEmail;
        }

        protected void Page_Load( object sender, EventArgs e )
        {

            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );

            if( Page.IsPostBack == false )
            {
                // if loading from offer screen, will have the following parameters:
                // ?ScheduleNum=" & mySchNum & "&OfferID=" & myOfferID)
                string offerIdString = "";
                string scheduleNumberString = "";
                if( Request.QueryString.Count > 0 )
                {
                    offerIdString = Request.QueryString.Get( "OfferID" );
                    scheduleNumberString = Request.QueryString.Get( "ScheduleNum" );
                }

                if( offerIdString.Length > 0 && scheduleNumberString.Length > 0 )
                {
                    int offerId = int.Parse( offerIdString );
                    int scheduleNumber = int.Parse( scheduleNumberString );

                    LoadScreenBasedOnOffer( offerId, scheduleNumber );
                    Session[ "CreateFromOffer" ] = offerIdString;
                }
                else
                {
                    LoadDivisions( -1 );
                    SelectScheduleAndCOBasedOnDivision( -1, -1, -1 ); // defaults to --select--
                    LoadStates( "--" ); // defaults to -- ( no selection )
                    Session[ "CreateFromOffer" ] = "";
                }

                LoadScreenDefaultDates();
            }


            if( Page.IsPostBack == true )
            {
                string refreshDateType = "";
                bool refreshOrNot = false;
                HiddenField refreshDateValueOnSubmitHiddenField = ( HiddenField )CreateContractForm.FindControl( "RefreshDateValueOnSubmit" );
                HiddenField refreshOrNotOnSubmitHiddenField = ( HiddenField )CreateContractForm.FindControl( "RefreshOrNotOnSubmit" );

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

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "AwardDateButtonOnClickScript", GetDateButtonScript( "Award" ), true );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "EffectiveDateButtonOnClickScript", GetDateButtonScript( "Effective" ), true );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ExpirationDateButtonOnClickScript", GetDateButtonScript( "Expiration" ), true );


        }


        private void LoadDivisions( int divisionId )
        {
            bool bSuccess = true;

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];

            DataSet dsDivisions = null;
            bSuccess = contractDB.SelectDivisions( ref dsDivisions );
            if( bSuccess == true )
            {
                DivisionDropDownList.DataSource = dsDivisions;
                DivisionDropDownList.DataMember = "DivisionsTable";
                DivisionDropDownList.DataTextField = "DivisionName";
                DivisionDropDownList.DataValueField = "DivisionId";
                DivisionDropDownList.DataBind();

                DivisionDropDownList.SelectedValue = divisionId.ToString();
            }
            else
            {
                MsgBox.Alert( contractDB.ErrorMessage );
            }
        }

        private void LoadStates( string selectedAbbreviation )
        {
            bool bSuccess = true;
            DataSet dsStateCodes = null;
            ContractDB contractDB = null;

            if( Session[ "StateCodeDataSet" ] != null )
            {
                dsStateCodes = ( DataSet )Session[ "StateCodeDataSet" ];
            }
            else
            {
                contractDB = ( ContractDB )Session[ "ContractDB" ];

                bSuccess = contractDB.SelectStateCodes( ref dsStateCodes, 239 );  // temp $$$ while debugging
            }

            if( bSuccess == true )
            {
                StateDropDownList.ClearSelection();
                StateDropDownList.Items.Clear();
                StateDropDownList.DataSource = dsStateCodes;
                StateDropDownList.DataMember = "StateCodesTable";
                StateDropDownList.DataTextField = "StateAbbreviation";
                StateDropDownList.DataValueField = "StateName";
                StateDropDownList.DataBind();

                StateDropDownList.SelectedItem.Text = selectedAbbreviation;

                Session[ "StateCodeDataSet" ] = dsStateCodes;
            }
            else
            {
                MsgBox.Alert( contractDB.ErrorMessage );
            }
        }

        private void LoadScreenBasedOnOffer( int offerId, int scheduleNumber )
        {
            ContractCreationInfo contractCreationInfo = new ContractCreationInfo();
            contractCreationInfo.OfferId = offerId;
            contractCreationInfo.ScheduleNumber = scheduleNumber;

            bool bSuccess = true;

            OfferDB offerDB = ( OfferDB )Session[ "OfferDB" ];

            DataSet dsOneOfferRow = null;
            bSuccess = offerDB.GetOfferInfo2( ref dsOneOfferRow, offerId );
            if( bSuccess == true )
            {
                if( offerDB.RowsReturned > 0 )
                {
                    DataTable dtOneOfferRow = dsOneOfferRow.Tables[ "OneOfferRowTable" ];
                    DataRow row = dtOneOfferRow.Rows[ 0 ];
                    if( row.GetType() != typeof( DBNull ) )
                    {
                        if( row.IsNull( "AssignedCOID" ) == false )
                            contractCreationInfo.AssignedCOID = ( int )row[ "AssignedCOID" ];
                        if( row.IsNull( "DivisionId" ) == false )
                            contractCreationInfo.DivisionId = ( int )row[ "DivisionId" ];
                        if( row.IsNull( "ScheduleNumber" ) == false )
                            // overwriting scheduleNumber passed on query string
                            contractCreationInfo.ScheduleNumber = ( int )row[ "ScheduleNumber" ];
                        if( row.IsNull( "RebateRequired" ) == false )
                            contractCreationInfo.IsRebateRequired = ( bool )row[ "RebateRequired" ];
                        if( row.IsNull( "VendorName" ) == false )
                            contractCreationInfo.VendorName = ( string )row[ "VendorName" ];
                        if( row.IsNull( "VendorAddress1" ) == false )
                            contractCreationInfo.VendorAddress1 = ( string )row[ "VendorAddress1" ];
                        if( row.IsNull( "VendorAddress2" ) == false )
                            contractCreationInfo.VendorAddress2 = ( string )row[ "VendorAddress2" ];
                        if( row.IsNull( "VendorAddressCity" ) == false )
                            contractCreationInfo.VendorAddressCity = ( string )row[ "VendorAddressCity" ];
                        if( row.IsNull( "VendorAddressState" ) == false )
                            contractCreationInfo.VendorAddressState = ( string )row[ "VendorAddressState" ];
                        if( row.IsNull( "VendorZipCode" ) == false )
                            contractCreationInfo.VendorZipCode = ( string )row[ "VendorZipCode" ];
                        if( row.IsNull( "VendorUrl" ) == false )
                            contractCreationInfo.VendorUrl = ( string )row[ "VendorUrl" ];
                        if( row.IsNull( "VendorContactName" ) == false )
                            contractCreationInfo.VendorContactName = ( string )row[ "VendorContactName" ];
                        if( row.IsNull( "VendorContactPhone" ) == false )
                            contractCreationInfo.VendorContactPhone = ( string )row[ "VendorContactPhone" ];
                        if( row.IsNull( "VendorContactPhoneExtension" ) == false )
                            contractCreationInfo.VendorContactPhoneExtension = ( string )row[ "VendorContactPhoneExtension" ];
                        if( row.IsNull( "VendorContactFax" ) == false )
                            contractCreationInfo.VendorContactFax = ( string )row[ "VendorContactFax" ];
                        if( row.IsNull( "VendorContactEmail" ) == false )
                            contractCreationInfo.VendorContactEmail = ( string )row[ "VendorContactEmail" ];

                        Session[ "ContractCreationInfo" ] = contractCreationInfo;

                        LoadDivisions( contractCreationInfo.DivisionId );
                        SelectScheduleAndCOBasedOnDivision( contractCreationInfo.DivisionId, contractCreationInfo.ScheduleNumber, contractCreationInfo.AssignedCOID ); 
                        LoadStates( contractCreationInfo.VendorAddressState );

                        PrepopulateContractCreationInfoFromOffer( contractCreationInfo );

                    }

                }
            }
            else
            {
                MsgBox.Alert( offerDB.ErrorMessage );
            }

        }

        private void PrepopulateContractCreationInfoFromOffer( ContractCreationInfo contractCreationInfo )
        {
            VendorNameTextBox.Text = contractCreationInfo.VendorName;
            PointOfContactNameTextBox.Text = contractCreationInfo.VendorContactName;
            PointOfContactPhoneTextBox.Text = contractCreationInfo.VendorContactPhone;
            PointOfContactPhoneExtensionTextBox.Text = contractCreationInfo.VendorContactPhoneExtension;
            PointOfContactFaxTextBox.Text = contractCreationInfo.VendorContactFax;
            PointOfContactEmailTextBox.Text = contractCreationInfo.VendorContactEmail;

            Address1TextBox.Text = contractCreationInfo.VendorAddress1;
            Address2TextBox.Text = contractCreationInfo.VendorAddress2;
            CityTextBox.Text = contractCreationInfo.VendorAddressCity;
            ZipTextBox.Text = contractCreationInfo.VendorZipCode;
            CompanyUrlTextBox.Text = contractCreationInfo.VendorUrl;
        }

        protected void ContractNumberCustomValidator_OnServerValidate( object sender, ServerValidateEventArgs args )
        {

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            int selectedScheduleNumber = int.Parse( ScheduleDropDownList.SelectedValue.ToString() );
            bool bIsValidated = false;
            string validationMessage = "";
            bool bSuccess = contractDB.ValidateContractNumber( args.Value, selectedScheduleNumber, ref bIsValidated, ref validationMessage );

            if( bSuccess == true )
            {
                args.IsValid = bIsValidated;

                if( bIsValidated == false )
                    Session[ "CreateContractValidationErrorMessage" ] = validationMessage;
            }
            else
            {
                MsgBox.Alert( contractDB.ErrorMessage );
            }

        }

        protected void ContractAdditionSaveButton_OnClick( object sender, EventArgs e )
        {
            bool bSuccess = true;

            if( Page.IsValid == false )
            {
                DisplayValidationErrors();
                return;
            }

            int selectedDivisionId = int.Parse( DivisionDropDownList.SelectedValue.ToString() );
            int selectedScheduleNumber = int.Parse( ScheduleDropDownList.SelectedValue.ToString() );
            int selectedContractingOfficer = int.Parse( ContractingOfficerDropDownList.SelectedValue.ToString() );

            string contractNumber = ContractNumberTextBox.Text;
            
            string awardDateString = AwardDateTextBox.Text;
            DateTime awardDate;
            if( DateTime.TryParse( awardDateString, out awardDate ) != true )
            {
                MsgBox.Alert( "Award Date is not a valid date." );
                return;
            }

            string effectiveDateString = EffectiveDateTextBox.Text;
            DateTime effectiveDate;
            if( DateTime.TryParse( effectiveDateString, out effectiveDate ) != true )
            {
                MsgBox.Alert( "Effective Date is not a valid date." );
                return;
            }

            string expirationDateString = ExpirationDateTextBox.Text;
            DateTime expirationDate;
            if( DateTime.TryParse( expirationDateString, out expirationDate ) != true )
            {
                MsgBox.Alert( "Expiration Date is not a valid date." );
                return;
            }

            bool bIsRebateRequired = ( RebateRequiredRadioButtonList.SelectedValue == "1" ) ? true : false;  
            string vendorName = VendorNameTextBox.Text;
            string vendorContactName = PointOfContactNameTextBox.Text;
            string vendorContactPhone = PointOfContactPhoneTextBox.Text;
            string vendorContactPhoneExtension = PointOfContactPhoneExtensionTextBox.Text;
            string vendorContactFax = PointOfContactFaxTextBox.Text;
            string vendorContactEmail = PointOfContactEmailTextBox.Text;

            string vendorAddress1 = Address1TextBox.Text;
            string vendorAddress2 = Address2TextBox.Text;
            string vendorCity = CityTextBox.Text;
            string vendorState = StateDropDownList.SelectedItem.Text;
            string vendorZip = ZipTextBox.Text;
            string vendorWebPage = CompanyUrlTextBox.Text;

            int newContractId = -1;
            int newPharmaceuticalContractId = -1;

            // if created from offer, then offerId should be available
            int sourceOfferId = -1;
            if( Session[ "ContractCreationInfo" ] != null )
            {
                ContractCreationInfo contractCreationInfo = ( ContractCreationInfo )Session[ "ContractCreationInfo" ];
                if( contractCreationInfo != null )
                {
                    sourceOfferId = contractCreationInfo.OfferId;
                }
            }

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            bSuccess = contractDB.CreateContract( selectedScheduleNumber, contractNumber, awardDate, effectiveDate, expirationDate,
                                0, selectedContractingOfficer, vendorName, vendorContactName, vendorContactPhone, vendorContactPhoneExtension,
                                vendorContactFax, vendorContactEmail, vendorAddress1, vendorAddress2, vendorCity, vendorState, vendorZip, vendorWebPage,
                                sourceOfferId, bIsRebateRequired, ref newContractId, ref newPharmaceuticalContractId );
            if( bSuccess != true )
            {
                MsgBox.Alert( contractDB.ErrorMessage );
            }
            else
            {
                // create a current document record and open up the contract editor screen
                // this code is depricated in R2.2
                CurrentDocument currentDocument = new CurrentDocument( -1, contractNumber, selectedScheduleNumber, contractDB, ( DrugItemDB )Session[ "DrugItemDB" ], ( ItemDB )Session[ "ItemDB" ], true );
                Session[ "CurrentDocument" ] = currentDocument;
                currentDocument.ActiveStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.ActiveStatuses.Active;
                currentDocument.OwnerId = selectedContractingOfficer;

                currentDocument.VendorName = vendorName;
                currentDocument.Description = "";
                currentDocument.AwardDate = awardDate;
                currentDocument.EffectiveDate = effectiveDate;
                currentDocument.ExpirationDate = expirationDate;

                BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
                bs.SetDocumentEditStatus( currentDocument );

                //Dim strWindow As String = "<script>window.open('NAC_CM_Contracts.aspx?CntrctNum=" & cleansedContractNumber & "&SchNum=" & dlSchedule.SelectedValue & "','Details','toolbar=no,menubar=no,resizable=yes,scrollbars=yes,width=900,height=700,left=170,top=110')</script>"
                Response.Redirect( string.Format( "NAC_CM_Contracts.aspx?CntrctNum={0}&SchNum={1}", contractNumber, selectedScheduleNumber ));
            }
        }

        private void DisplayValidationErrors()
        {
            StringBuilder sb = new StringBuilder( 400 );
            sb.AppendFormat( "Please correct the following: \n\n" );

            CustomValidator fv = ContractNumberCustomValidator;
            string validationMessage = "";
            if( fv.IsValid == false )
            {
                if( Session[ "CreateContractValidationErrorMessage" ] != null )
                    validationMessage = ( string )Session[ "CreateContractValidationErrorMessage" ];
                sb.AppendFormat( "{0} \n", validationMessage );
            }
         
            MsgBox.Alert( sb.ToString() );
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

            SelectScheduleAndCOBasedOnDivision( selectedDivisionId, -1, -1 );

            // disable rebate for non-fss
            RadioButtonList rebateRequiredRadioButtonList = ( RadioButtonList )CreateContractForm.FindControl( "RebateRequiredRadioButtonList" );
            if( selectedDivisionId != 1 )
            {
                rebateRequiredRadioButtonList.Enabled = false;
            }
            else
            {
                rebateRequiredRadioButtonList.Enabled = true;
            }
             
        }

        private void SelectScheduleAndCOBasedOnDivision( int selectedDivisionId, int selectedScheduleNumber, int selectedCOId )
        {
            bool bSuccess = true;

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            DataSet dsSchedulesForDivision = null;
            bSuccess = contractDB.SelectSchedulesForDivision( ref dsSchedulesForDivision, selectedDivisionId );
            if( bSuccess == true )
            {
                ScheduleDropDownList.DataSource = dsSchedulesForDivision;
                ScheduleDropDownList.DataMember = "SchedulesTable";
                ScheduleDropDownList.DataTextField = "Schedule_Name";
                ScheduleDropDownList.DataValueField = "Schedule_Number";
                ScheduleDropDownList.DataBind();

                ScheduleDropDownList.SelectedValue = selectedScheduleNumber.ToString(); ;
            }
            else
            {
                MsgBox.Alert( contractDB.ErrorMessage );
            }

            try
            {
                BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
                DataSet dsContractingOfficersForDivision = null;
                bSuccess = bs.GetContractingOfficersForDivision( selectedDivisionId, ref dsContractingOfficersForDivision );

                if( bSuccess == true )
                {
                    ContractingOfficerDropDownList.DataSource = dsContractingOfficersForDivision;
                    ContractingOfficerDropDownList.DataMember = "UserTable";
                    ContractingOfficerDropDownList.DataTextField = "FullName";
                    ContractingOfficerDropDownList.DataValueField = "CO_ID";
                    ContractingOfficerDropDownList.DataBind();

                    ContractingOfficerDropDownList.SelectedValue = selectedCOId.ToString();
                }
            }
            catch( Exception ex )
            {
                MsgBox.ShowError( ex );
            }
        }

        public string GetDateButtonScript( string dateTypeString )
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
            string script = string.Format( "function On{0}DateButtonClick() {{ window.open('Calendar.aspx?InitialDate={1}&DateType={2}','Calendar','toolbar=no,menubar=no,resizable=no,scrollbars=no,statusbar=0,location=0,width=250,height=340,left=660,top=300'); return false;}}", dateTypeString, defaultDateString, dateTypeString );
            return ( script );
        }

        private void RefreshDate( string dateTypeString )
        {
            DateTime displayDate;

            if( dateTypeString.Contains( "Award" ) == true )
            {
                if( Session[ "Award" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "Award" ].ToString());
                    AwardDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    AwardDateTextBox.Text = "x";
                }
            }
            if( dateTypeString.Contains( "Effective" ) == true )
            {
                if( Session[ "Effective" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "Effective" ].ToString() );
                    EffectiveDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    EffectiveDateTextBox.Text = "x";
                }
            }
            if( dateTypeString.Contains( "Expiration" ) == true )
            {
                if( Session[ "Expiration" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "Expiration" ].ToString() );
                    ExpirationDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    ExpirationDateTextBox.Text = "x";
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
            Session[ "Award" ] = awardDate.ToShortDateString();
            Session[ "Effective" ] = effectiveDate.ToShortDateString();
            Session[ "Expiration" ] = expirationDate.ToShortDateString();

            AwardDateTextBox.Text = awardDate.ToShortDateString();
            EffectiveDateTextBox.Text = effectiveDate.ToShortDateString();
            ExpirationDateTextBox.Text = expirationDate.ToShortDateString();
        }

        private void ClearSessionVariables()
        {
            Session[ "Award" ] = null;
            Session[ "Effective" ] = null;
            Session[ "Expiration" ] = null;
            Session[ "CreateContractValidationErrorMessage" ] = null;
            Session[ "ContractCreationInfo" ] = null; 
        }

 

        //protected void CreateContractForm_OnPreRender( object sender, EventArgs e )
        //{
        //   // CreateContractPanel.Attributes[ "class" ] = ContractCreatePanelStyle;
        //}

        public string ContractCreatePanelStyle
        {
            get
            {
                CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
                if( cmGlobals.ClientScreenHeight == 900 && cmGlobals.ClientScreenWidth == 1440 )
                {
                    return ( "ContractCreatePanelHighRes" );
                }
                else if( cmGlobals.ClientScreenHeight == 768 && cmGlobals.ClientScreenWidth == 1024 )
                {
                    return ( "ContractCreatePanelMedRes" );
                }
                else
                {
                    return ( "ContractCreatePanelLowRes" ); // low res default
                }
            }
        }


        protected void ScheduleDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            bool bSuccess = true;
            DropDownList scheduleDropDownList = ( DropDownList )sender;
            ListItem selectedItem = scheduleDropDownList.SelectedItem;
            int selectedScheduleNumber = int.Parse( selectedItem.Value );

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            string prefix = "";
            bSuccess = contractDB.GetNewContractPrefix( selectedScheduleNumber, ref prefix );
            if( bSuccess == true )
            {
                ContractNumberTextBox.Text = prefix;
            }

            // disable rebate for service schedule
            RadioButtonList rebateRequiredRadioButtonList = ( RadioButtonList )CreateContractForm.FindControl( "RebateRequiredRadioButtonList" );
            if ( selectedScheduleNumber == 36 )
            {
                rebateRequiredRadioButtonList.Enabled = false;
            }
            else
            {
                rebateRequiredRadioButtonList.Enabled = true;
            }
        }
        //*********************************** shared functions **************************

        private void MultiLineButtonText( Button button, string[] buttonTextArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < buttonTextArray.Count(); i++ )
            {
                sb.AppendLine( buttonTextArray[ i ] );
            }

            button.Text = sb.ToString();
        }

        protected void CreateContractScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "CreateContractErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "CreateContractErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            ScriptManager1.AsyncPostBackErrorMessage = errorMsg;
        }
    }
}
