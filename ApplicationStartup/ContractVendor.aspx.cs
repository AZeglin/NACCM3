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
    public partial class ContractVendor : BaseDocumentEditorPage
    {
        public ContractVendor()
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
            if( Page.IsPostBack == true )
            {
                string refreshDateType = "";
                bool bRefreshOrNot = false;

                HiddenField refreshDateValueOnSubmitHiddenField = ( HiddenField )ContractVendorInsuranceDatesFormView.FindControl( "RefreshDateValueOnSubmit" );
                HiddenField refreshOrNotOnSubmitHiddenField = ( HiddenField )ContractVendorInsuranceDatesFormView.FindControl( "RefreshOrNotOnSubmit" );

                if( refreshDateValueOnSubmitHiddenField != null )
                {
                    refreshDateType = refreshDateValueOnSubmitHiddenField.Value.ToString();

                    if( refreshOrNotOnSubmitHiddenField != null )
                    {
                        bRefreshOrNot = Boolean.Parse( refreshOrNotOnSubmitHiddenField.Value );

                        if( refreshDateType.Contains( "Undefined" ) == false )
                        {
                            if( bRefreshOrNot == true )
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
        }


        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            ContractVendorSocioFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            ContractVendorSocioFormView.DataKeyNames = new string[] { "ContractId" };

            VendorAttributesFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            VendorAttributesFormView.DataKeyNames = new string[] { "ContractId" };

            ContractVendorInsuranceDatesFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            ContractVendorInsuranceDatesFormView.DataKeyNames = new string[] { "ContractId" };

            WarrantyInformationFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            WarrantyInformationFormView.DataKeyNames = new string[] { "ContractId" };

            ReturnedGoodsPolicyFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            ReturnedGoodsPolicyFormView.DataKeyNames = new string[] { "ContractId" };

            StateFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            StateFormView.DataKeyNames = new string[] { "ContractId" };

            // note form view controls are not yet created here
        }

        protected void ClearSessionVariables()
        {
            Session[ "InsurancePolicyEffective" ] = null;
            Session[ "InsurancePolicyExpiration" ] = null;
        }

       protected void BindFormViews()
       {
            BindHeader();

            ContractVendorSocioFormView.DataBind();
            VendorAttributesFormView.DataBind();
            ContractVendorInsuranceDatesFormView.DataBind();
            WarrantyInformationFormView.DataBind();
            ReturnedGoodsPolicyFormView.DataBind();
            StateFormView.DataBind();

           // note form view controls are not yet created here
       }

  
       protected void ReturnedGoodsPolicyFormView_OnDataBound( object sender, EventArgs e )
       {
           FormView returnedGoodsPolicyFormView = ( FormView )sender;

           DropDownList returnedGoodsPolicyTypeDropDownList;
           int selectedReturnedGoodsPolicyTypeId = -1;

           EditedDocumentContent formViewContent = ( EditedDocumentContent )returnedGoodsPolicyFormView.DataItem;

           if( formViewContent != null )
           {
               selectedReturnedGoodsPolicyTypeId = formViewContent.ReturnedGoodsPolicyTypeId;
           }

           if( returnedGoodsPolicyFormView != null )
           {
               returnedGoodsPolicyTypeDropDownList = ( DropDownList )returnedGoodsPolicyFormView.FindControl( "ReturnedGoodsPolicyTypeDropDownList" );

               if( returnedGoodsPolicyTypeDropDownList != null )
               {
                   returnedGoodsPolicyTypeDropDownList.ClearSelection();
                   returnedGoodsPolicyTypeDropDownList.Items.Clear();

                   foreach( object obj in formViewContent.ReturnedGoodsPolicyTypes )
                   {
                       EditedDocumentContent.ReturnedGoodsPolicy returnedGoodsPolicy = ( EditedDocumentContent.ReturnedGoodsPolicy )obj;

                       returnedGoodsPolicyTypeDropDownList.Items.Add( new ListItem( returnedGoodsPolicy.PolicyTypeDescription, returnedGoodsPolicy.PolicyTypeId.ToString() ) );
                   }

                   if( returnedGoodsPolicyTypeDropDownList.Items.FindByValue( selectedReturnedGoodsPolicyTypeId.ToString() ) != null )
                   {
                       returnedGoodsPolicyTypeDropDownList.Items.FindByValue( selectedReturnedGoodsPolicyTypeId.ToString() ).Selected = true;
                   }
               }
           }
       }

       protected void InsurancePolicyEffectiveDateTextBox_OnDataBinding( object sender, EventArgs e )
       {
           TextBox insurancePolicyEffectiveDateTextBox = ( TextBox )sender;

           FormView contractVendorInsuranceDatesFormView;

           if( insurancePolicyEffectiveDateTextBox != null )
           {
               contractVendorInsuranceDatesFormView = ( FormView )insurancePolicyEffectiveDateTextBox.NamingContainer;

               if( contractVendorInsuranceDatesFormView != null )
               {
                   EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )contractVendorInsuranceDatesFormView.DataItem;

                   DateTime insurancePolicyEffectiveDate = editedDocumentContent.InsurancePolicyEffectiveDate;

                   if( insurancePolicyEffectiveDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                   {
                       insurancePolicyEffectiveDateTextBox.Text = "";
                   }
                   else
                   {
                       insurancePolicyEffectiveDateTextBox.Text = insurancePolicyEffectiveDate.ToString( "MM/dd/yyyy" );
                       Session[ "InsurancePolicyEffective" ] = insurancePolicyEffectiveDate.ToString( "MM/dd/yyyy" );
                   }
               }
           }
       }

  
       protected void InsurancePolicyExpirationDateTextBox_OnDataBinding( object sender, EventArgs e )
       {
           TextBox insurancePolicyExpirationDateTextBox = ( TextBox )sender;

           FormView contractVendorInsuranceDatesFormView;

           if( insurancePolicyExpirationDateTextBox != null )
           {
               contractVendorInsuranceDatesFormView = ( FormView )insurancePolicyExpirationDateTextBox.NamingContainer;

               if( contractVendorInsuranceDatesFormView != null )
               {
                   EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )contractVendorInsuranceDatesFormView.DataItem;

                   DateTime insurancePolicyExpirationDate = editedDocumentContent.InsurancePolicyExpirationDate;

                   if( insurancePolicyExpirationDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                   {
                       insurancePolicyExpirationDateTextBox.Text = "";
                   }
                   else
                   {
                       insurancePolicyExpirationDateTextBox.Text = insurancePolicyExpirationDate.ToString( "MM/dd/yyyy" );
                       Session[ "InsurancePolicyExpiration" ] = insurancePolicyExpirationDate.ToString( "MM/dd/yyyy" );
                   }
               }
           }
       }


       // each contract date is authorized separately
       protected void EnableContractVendorDateEditing( FormView sender )
       {
           Page currentPage;
           FormView contractVendorInsuranceDatesFormView = sender;

           currentPage = contractVendorInsuranceDatesFormView.Page;

           CurrentDocument currentDocument = null;
           currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

           bool bUnlimited = false;
           if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.InsuranceUnlimitedDateRange ) == true )
           {
               bUnlimited = true;
           }

           DateTime insurancePolicyEffectiveMinDate = DateTime.Now.AddDays( -60 );
           DateTime insurancePolicyExpirationMinDate = DateTime.Now;

           DateTime maxAllowedDate = DateTime.Now.AddYears( 10 );

           // create image button scripts - new way with parameterized limits
           ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "InsurancePolicyEffectiveDateButtonOnClickScript", GetDateButtonScript( "InsurancePolicyEffective", bUnlimited, insurancePolicyEffectiveMinDate, maxAllowedDate ), true );
           ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "InsurancePolicyExpirationDateButtonOnClickScript", GetDateButtonScript( "InsurancePolicyExpiration", bUnlimited, insurancePolicyExpirationMinDate, maxAllowedDate ), true );

           ImageButton insurancePolicyEffectiveDateImageButton = ( ImageButton )contractVendorInsuranceDatesFormView.FindControl( "InsurancePolicyEffectiveDateImageButton" );
           TextBox insurancePolicyEffectiveDateTextBox = ( TextBox )contractVendorInsuranceDatesFormView.FindControl( "InsurancePolicyEffectiveDateTextBox" );

           if( insurancePolicyEffectiveDateImageButton != null )
           {
               if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.InsurancePolicyDates ) == true )
               {
                   insurancePolicyEffectiveDateImageButton.Visible = true;
                   insurancePolicyEffectiveDateTextBox.Enabled = true;
               }
               else
               {
                   insurancePolicyEffectiveDateImageButton.Visible = false;
                   insurancePolicyEffectiveDateTextBox.Enabled = false;
               }
           }

           ImageButton insurancePolicyExpirationDateImageButton = ( ImageButton )contractVendorInsuranceDatesFormView.FindControl( "InsurancePolicyExpirationDateImageButton" );
           TextBox insurancePolicyExpirationDateTextBox = ( TextBox )contractVendorInsuranceDatesFormView.FindControl( "InsurancePolicyExpirationDateTextBox" );

           if( insurancePolicyExpirationDateImageButton != null )
           {
               if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.InsurancePolicyDates ) == true )
               {
                   insurancePolicyExpirationDateImageButton.Visible = true;
                   insurancePolicyExpirationDateTextBox.Enabled = true;
               }
               else
               {
                   insurancePolicyExpirationDateImageButton.Visible = false;
                   insurancePolicyExpirationDateTextBox.Enabled = false;
               }
           }
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

       public void RefreshDate( string dateTypeString )
       {
           DateTime displayDate;

           if( dateTypeString.Contains( "InsurancePolicyEffective" ) == true )
           {
               TextBox insurancePolicyEffectiveDateTextBox = ( TextBox )ContractVendorInsuranceDatesFormView.FindControl( "InsurancePolicyEffectiveDateTextBox" );
               if( Session[ "InsurancePolicyEffective" ] != null )
               {
                   displayDate = DateTime.Parse( Session[ "InsurancePolicyEffective" ].ToString() );
                   insurancePolicyEffectiveDateTextBox.Text = displayDate.ToShortDateString();
               }
               else
               {
                   insurancePolicyEffectiveDateTextBox.Text = "";
               }
           }


           if( dateTypeString.Contains( "InsurancePolicyExpiration" ) == true )
           {
               TextBox insurancePolicyExpirationDateTextBox = ( TextBox )ContractVendorInsuranceDatesFormView.FindControl( "InsurancePolicyExpirationDateTextBox" );
               if( Session[ "InsurancePolicyExpiration" ] != null )
               {
                   displayDate = DateTime.Parse( Session[ "InsurancePolicyExpiration" ].ToString() );
                   insurancePolicyExpirationDateTextBox.Text = displayDate.ToShortDateString();
               }
               else
               {
                   insurancePolicyExpirationDateTextBox.Text = "";
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
            return ( "ContractVendor" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
           DataRelay dataRelay = ( DataRelay )dataRelayInterface;

           bool bSuccess = true;

           ResetValidationGroup( validationGroupName );

           CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

           if( currentDocument != null )
           {
               if( currentDocument.CanHaveInsurance( currentDocument.ScheduleNumber ) == true )
               {
                   if( dataRelay.EditedDocumentContentFront.InsurancePolicyEffectiveDate.CompareTo( DateTime.MinValue ) == 0 )
                   {
                       AppendValidationError( "Insurance policy effective date is required.", bIsShortSave );
                       bSuccess = false;
                   }

                   if( dataRelay.EditedDocumentContentFront.InsurancePolicyExpirationDate.CompareTo( DateTime.MinValue ) == 0 )
                   {
                       AppendValidationError( "Insurance policy expiration date is required.", bIsShortSave );
                       bSuccess = false;
                   }

                   if( dataRelay.EditedDocumentContentFront.InsurancePolicyExpirationDate.CompareTo( dataRelay.EditedDocumentContentFront.InsurancePolicyEffectiveDate ) <= 0 )
                   {
                       AppendValidationError( "Insurance policy expiration date must be after effective date.", bIsShortSave );
                       bSuccess = false;
                   }
               }

               if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) == false )
               {
                   if( dataRelay.EditedDocumentContentFront.SAMUEI.Trim().Length < 12 )
                   {
                       AppendValidationError( "A 12 character UEI is required.", bIsShortSave );
                       bSuccess = false;
                   }

                   if( dataRelay.EditedDocumentContentFront.DUNS.Trim().Length < 9 )
                   {
                       AppendValidationError( "A 9 digit numeric DUNS is required.", bIsShortSave );
                       bSuccess = false;
                   }


                   if( dataRelay.EditedDocumentContentFront.TIN.Trim().Length > 0 )
                   {
                       if( dataRelay.EditedDocumentContentFront.TIN.Trim().Length < 9 )
                       {
                           AppendValidationError( "TIN must be 9 digits in length.", bIsShortSave );
                           bSuccess = false;
                       }
                   }

                   int vendorTypeId = -1;
                   vendorTypeId = dataRelay.EditedDocumentContentFront.VendorTypeId;

                   if( vendorTypeId != 1 && vendorTypeId != 2 && vendorTypeId != 3 && vendorTypeId != 5 )
                   {
                       AppendValidationError( "Please specify a vendor type.", bIsShortSave );
                       bSuccess = false;
                   }

               }
           }

           int socioBusinessSizeId = -1;
           socioBusinessSizeId = dataRelay.EditedDocumentContentFront.SocioBusinessSizeId;

           if( socioBusinessSizeId != 1 && socioBusinessSizeId != 2  )
           {
               AppendValidationError( "Please specify a business size.", bIsShortSave );
               bSuccess = false;
           }

           if( socioBusinessSizeId == 2 )
           {
               if( dataRelay.EditedDocumentContentFront.SocioSDB == true || dataRelay.EditedDocumentContentFront.Socio8a == true )
               {
                   AppendValidationError( "SDB or 8a are not allowed for Large Businesses.", bIsShortSave );
                   bSuccess = false;
               }
           }

           return ( bSuccess );
       }

       public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
       {
           DataRelay dataRelay = ( DataRelay )dataRelayInterface;

           bool bSuccess = true;

           CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

           CheckBox largeBusinessCheckBox = ( CheckBox )ContractVendorSocioFormView.FindControl( "LargeBusinessCheckBox" );
           CheckBox smallBusinessCheckBox = ( CheckBox )ContractVendorSocioFormView.FindControl( "SmallBusinessCheckBox" );

           if( largeBusinessCheckBox != null && smallBusinessCheckBox != null )
           {
               if( largeBusinessCheckBox.Checked == true )
               {
                   dataRelay.EditedDocumentContentFront.SocioBusinessSizeId = 2;
               }
               else if( smallBusinessCheckBox.Checked == true )
               {
                   dataRelay.EditedDocumentContentFront.SocioBusinessSizeId = 1;
               }
               else
               {
                   dataRelay.EditedDocumentContentFront.SocioBusinessSizeId = -1;  // no selection
               }
           }

           CheckBox veteranOwnedCheckBox = ( CheckBox )ContractVendorSocioFormView.FindControl( "VeteranOwnedCheckBox" );
           CheckBox disabledVeteranOwnedCheckBox = ( CheckBox )ContractVendorSocioFormView.FindControl( "DisabledVeteranOwnedCheckBox" );
           if( veteranOwnedCheckBox != null && disabledVeteranOwnedCheckBox != null )
           {
               if( veteranOwnedCheckBox.Checked == true || disabledVeteranOwnedCheckBox.Checked == true )
               {
                   if( veteranOwnedCheckBox.Checked == true )
                   {
                       dataRelay.EditedDocumentContentFront.SocioVetStatusId = 1;
                   }
                   if( disabledVeteranOwnedCheckBox.Checked == true )
                   {
                       dataRelay.EditedDocumentContentFront.SocioVetStatusId = 3;   // these must match tlkup_VetStatus
                   }
               }
               else
               {
                   dataRelay.EditedDocumentContentFront.SocioVetStatusId = 0;
               }
           }

           CheckBox womanOwnedCheckBox = ( CheckBox )ContractVendorSocioFormView.FindControl( "WomanOwnedCheckBox" );
           if( womanOwnedCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.SocioWomanOwned = womanOwnedCheckBox.Checked;
           }

           CheckBox smallDisadvantagedCheckBox = ( CheckBox )ContractVendorSocioFormView.FindControl( "SmallDisadvantagedCheckBox" );
           if( smallDisadvantagedCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.SocioSDB = smallDisadvantagedCheckBox.Checked;
           }

           CheckBox eightACheckBox = ( CheckBox )ContractVendorSocioFormView.FindControl( "EightACheckBox" );
           if( eightACheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.Socio8a = eightACheckBox.Checked;
           }

           CheckBox hubZoneCheckBox = ( CheckBox )ContractVendorSocioFormView.FindControl( "HubZoneCheckBox" );
           if( hubZoneCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.HubZone = hubZoneCheckBox.Checked;
           }

           // VendorAttributesFormView
           TextBox SAMUEITextBox = ( TextBox )VendorAttributesFormView.FindControl( "SAMUEITextBox" );
           if( SAMUEITextBox != null )
           {
               dataRelay.EditedDocumentContentFront.SAMUEI = SAMUEITextBox.Text;
           }

           TextBox dunsTextBox = ( TextBox )VendorAttributesFormView.FindControl( "DunsTextBox" );
           if( dunsTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.DUNS = dunsTextBox.Text;
           }

           TextBox tinTextBox = ( TextBox )VendorAttributesFormView.FindControl( "TinTextBox" );
           if( tinTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.TIN = tinTextBox.Text;
           }

           CheckBox distributorCheckBox = ( CheckBox )VendorAttributesFormView.FindControl( "DistributorCheckBox" );
           CheckBox manufacturerCheckBox = ( CheckBox )VendorAttributesFormView.FindControl( "ManufacturerCheckBox" );
           CheckBox servicesCheckBox = ( CheckBox )VendorAttributesFormView.FindControl( "ServicesCheckBox" );
           
           if( distributorCheckBox != null &&  manufacturerCheckBox != null && servicesCheckBox != null )
           {
               if( servicesCheckBox.Checked == true )
                   dataRelay.EditedDocumentContentFront.VendorTypeId = 5;
               else if( manufacturerCheckBox.Checked == true && distributorCheckBox.Checked == true )
               {
                   dataRelay.EditedDocumentContentFront.VendorTypeId = 3; // both
               }
               else if( manufacturerCheckBox.Checked == true )
               {
                   dataRelay.EditedDocumentContentFront.VendorTypeId = 2;
               }
               else if (distributorCheckBox.Checked == true)
               {
                   dataRelay.EditedDocumentContentFront.VendorTypeId = 1;
               }
               else
               {
                   dataRelay.EditedDocumentContentFront.VendorTypeId = -1;
               }
           }

           CheckBox creditCardCheckBox = ( CheckBox )VendorAttributesFormView.FindControl( "CreditCardCheckBox" );
           if( creditCardCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.CreditCardAccepted = creditCardCheckBox.Checked;
           }

           CheckBox hazardousCheckBox = ( CheckBox )VendorAttributesFormView.FindControl( "HazardousCheckBox" );
           if( hazardousCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.HazardousMaterial = hazardousCheckBox.Checked;
           }

           #region GeographicCoverage

           CheckBox group52CheckBox = ( CheckBox )StateFormView.FindControl( "Group52" );
           if( group52CheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.Group52 = group52CheckBox.Checked;
           }

           CheckBox group51CheckBox = ( CheckBox )StateFormView.FindControl( "Group51" );
           if( group52CheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.Group52 = group52CheckBox.Checked;
           }

           CheckBox group50CheckBox = ( CheckBox )StateFormView.FindControl( "Group50" );
           if( group52CheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.Group52 = group52CheckBox.Checked;
           }
           
           CheckBox group49CheckBox = ( CheckBox )StateFormView.FindControl( "Group49" );
           if( group52CheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.Group52 = group52CheckBox.Checked;
           }

           CheckBox ALCheckBox = ( CheckBox )StateFormView.FindControl( "AL" );
           if( ALCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.AL = ALCheckBox.Checked;
           }

           CheckBox AKCheckBox = ( CheckBox )StateFormView.FindControl( "AK" );
           if( AKCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.AK = AKCheckBox.Checked;
           }

           CheckBox AZCheckBox = ( CheckBox )StateFormView.FindControl( "AZ" );
           if( AZCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.AZ = AZCheckBox.Checked;
           }

//AR

           CheckBox ARCheckBox = ( CheckBox )StateFormView.FindControl( "AR" );
           if( ARCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.AR = ARCheckBox.Checked;
           }
//CA

           CheckBox CACheckBox = ( CheckBox )StateFormView.FindControl( "CA" );
           if( CACheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.CA = CACheckBox.Checked;
           }
//CO

           CheckBox COCheckBox = ( CheckBox )StateFormView.FindControl( "CO" );
           if( COCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.CO = COCheckBox.Checked;
           }
//CT

           CheckBox CTCheckBox = ( CheckBox )StateFormView.FindControl( "CT" );
           if( CTCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.CT = CTCheckBox.Checked;
           }
//DE

           CheckBox DECheckBox = ( CheckBox )StateFormView.FindControl( "DE" );
           if( DECheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.DE = DECheckBox.Checked;
           }
//DC

           CheckBox DCCheckBox = ( CheckBox )StateFormView.FindControl( "DC" );
           if( DCCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.DC = DCCheckBox.Checked;
           }
//FL

           CheckBox FLCheckBox = ( CheckBox )StateFormView.FindControl( "FL" );
           if( FLCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.FL = FLCheckBox.Checked;
           }
//GA

           CheckBox GACheckBox = ( CheckBox )StateFormView.FindControl( "GA" );
           if( GACheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.GA = GACheckBox.Checked;
           }
//HI

           CheckBox HICheckBox = ( CheckBox )StateFormView.FindControl( "HI" );
           if( HICheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.HI = HICheckBox.Checked;
           }
//ID

           CheckBox IDCheckBox = ( CheckBox )StateFormView.FindControl( "ID" );
           if( IDCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.ID = IDCheckBox.Checked;
           }
//IL

           CheckBox ILCheckBox = ( CheckBox )StateFormView.FindControl( "IL" );
           if( ILCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.IL = ILCheckBox.Checked;
           }
//IN

           CheckBox INCheckBox = ( CheckBox )StateFormView.FindControl( "IN" );
           if( INCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.IN = INCheckBox.Checked;
           }
//IA

           CheckBox IACheckBox = ( CheckBox )StateFormView.FindControl( "IA" );
           if( IACheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.IA = IACheckBox.Checked;
           }
//KS

           CheckBox KSCheckBox = ( CheckBox )StateFormView.FindControl( "KS" );
           if( KSCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.KS = KSCheckBox.Checked;
           }
//KY

           CheckBox KYCheckBox = ( CheckBox )StateFormView.FindControl( "KY" );
           if( KYCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.KY = KYCheckBox.Checked;
           }
//LA

           CheckBox LACheckBox = ( CheckBox )StateFormView.FindControl( "LA" );
           if( LACheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.LA = LACheckBox.Checked;
           }
//ME

           CheckBox MECheckBox = ( CheckBox )StateFormView.FindControl( "ME" );
           if( MECheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.ME = MECheckBox.Checked;
           }
//MD

           CheckBox MDCheckBox = ( CheckBox )StateFormView.FindControl( "MD" );
           if( MDCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.MD = MDCheckBox.Checked;
           }
//MA

           CheckBox MACheckBox = ( CheckBox )StateFormView.FindControl( "MA" );
           if( MACheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.MA = MACheckBox.Checked;
           }
//MI

           CheckBox MICheckBox = ( CheckBox )StateFormView.FindControl( "MI" );
           if( MICheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.MI = MICheckBox.Checked;
           }
//MN

           CheckBox MNCheckBox = ( CheckBox )StateFormView.FindControl( "MN" );
           if( MNCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.MN = MNCheckBox.Checked;
           }
//MS

           CheckBox MSCheckBox = ( CheckBox )StateFormView.FindControl( "MS" );
           if( MSCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.MS = MSCheckBox.Checked;
           }
//MO

           CheckBox MOCheckBox = ( CheckBox )StateFormView.FindControl( "MO" );
           if( MOCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.MO = MOCheckBox.Checked;
           }


//MT
           CheckBox MTCheckBox = ( CheckBox )StateFormView.FindControl( "MT" );
           if( MTCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.MT = MTCheckBox.Checked;
           }
//NE

           CheckBox NECheckBox = ( CheckBox )StateFormView.FindControl( "NE" );
           if( NECheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.NE = NECheckBox.Checked;
           }
//NV
           CheckBox NVCheckBox = ( CheckBox )StateFormView.FindControl( "NV" );
           if( NVCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.NV = NVCheckBox.Checked;
           }
//NH
           CheckBox NHCheckBox = ( CheckBox )StateFormView.FindControl( "NH" );
           if( NHCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.NH = NHCheckBox.Checked;
           }
//NJ
           CheckBox NJCheckBox = ( CheckBox )StateFormView.FindControl( "NJ" );
           if( NJCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.NJ = NJCheckBox.Checked;
           }

//NM
           CheckBox NMCheckBox = ( CheckBox )StateFormView.FindControl( "NM" );
           if( NMCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.NM = NMCheckBox.Checked;
           }
//NY
           CheckBox NYCheckBox = ( CheckBox )StateFormView.FindControl( "NY" );
           if( NYCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.NY = NYCheckBox.Checked;
           }
//NC

           CheckBox NCCheckBox = ( CheckBox )StateFormView.FindControl( "NC" );
           if( NCCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.NC = NCCheckBox.Checked;
           }
//ND

           CheckBox NDCheckBox = ( CheckBox )StateFormView.FindControl( "ND" );
           if( NDCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.ND = NDCheckBox.Checked;
           }
//OH

           CheckBox OHCheckBox = ( CheckBox )StateFormView.FindControl( "OH" );
           if( OHCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.OH = OHCheckBox.Checked;
           }
//OK

           CheckBox OKCheckBox = ( CheckBox )StateFormView.FindControl( "OK" );
           if( OKCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.OK = OKCheckBox.Checked;
           }
//OR

           CheckBox ORCheckBox = ( CheckBox )StateFormView.FindControl( "OR" );
           if( ORCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.OR = ORCheckBox.Checked;
           }
//PA

           CheckBox PACheckBox = ( CheckBox )StateFormView.FindControl( "PA" );
           if( PACheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.PA = PACheckBox.Checked;
           }
//RI

           CheckBox RICheckBox = ( CheckBox )StateFormView.FindControl( "RI" );
           if( RICheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.RI = RICheckBox.Checked;
           }
//SC

           CheckBox SCCheckBox = ( CheckBox )StateFormView.FindControl( "SC" );
           if( SCCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.SC = SCCheckBox.Checked;
           }
//SD

           CheckBox SDCheckBox = ( CheckBox )StateFormView.FindControl( "SD" );
           if( SDCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.SD = SDCheckBox.Checked;
           }
//TN

           CheckBox TNCheckBox = ( CheckBox )StateFormView.FindControl( "TN" );
           if( TNCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.TN = TNCheckBox.Checked;
           }
//TX

           CheckBox TXCheckBox = ( CheckBox )StateFormView.FindControl( "TX" );
           if( TXCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.TX = TXCheckBox.Checked;
           }
//UT

           CheckBox UTCheckBox = ( CheckBox )StateFormView.FindControl( "UT" );
           if( UTCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.UT = UTCheckBox.Checked;
           }
//VT

           CheckBox VTCheckBox = ( CheckBox )StateFormView.FindControl( "VT" );
           if( VTCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.VT = VTCheckBox.Checked;
           }
//VA

           CheckBox VACheckBox = ( CheckBox )StateFormView.FindControl( "VA" );
           if( VACheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.VA = VACheckBox.Checked;
           }
//WA

           CheckBox WACheckBox = ( CheckBox )StateFormView.FindControl( "WA" );
           if( WACheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.WA = WACheckBox.Checked;
           }
//WV

           CheckBox WVCheckBox = ( CheckBox )StateFormView.FindControl( "WV" );
           if( WVCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.WV = WVCheckBox.Checked;
           }
//WI

           CheckBox WICheckBox = ( CheckBox )StateFormView.FindControl( "WI" );
           if( WICheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.WI = WICheckBox.Checked;
           }
//WY

           CheckBox WYCheckBox = ( CheckBox )StateFormView.FindControl( "WY" );
           if( WYCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.WY = WYCheckBox.Checked;
           }
//PR

           CheckBox PRCheckBox = ( CheckBox )StateFormView.FindControl( "PR" );
           if( PRCheckBox != null )
           {
               dataRelay.EditedDocumentContentFront.GeographicCoverage.PR = PRCheckBox.Checked;
           }

////AB
//           CheckBox ABCheckBox = ( CheckBox )StateFormView.FindControl( "AB" );
//           if( ABCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.AB = ABCheckBox.Checked;
//           }
////BC

//           CheckBox BCCheckBox = ( CheckBox )StateFormView.FindControl( "BC" );
//           if( BCCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.BC = BCCheckBox.Checked;
//           }
////MB

//           CheckBox MBCheckBox = ( CheckBox )StateFormView.FindControl( "MB" );
//           if( MBCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.MB = MBCheckBox.Checked;
//           }
////NB

//           CheckBox NBCheckBox = ( CheckBox )StateFormView.FindControl( "NB" );
//           if( NBCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.NB = NBCheckBox.Checked;
//           }
////NF

//           CheckBox NFCheckBox = ( CheckBox )StateFormView.FindControl( "NF" );
//           if( NFCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.NF = NFCheckBox.Checked;
//           }
////NT

//           CheckBox NTCheckBox = ( CheckBox )StateFormView.FindControl( "NT" );
//           if( NTCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.NT = NTCheckBox.Checked;
//           }
////NS

//           CheckBox NSCheckBox = ( CheckBox )StateFormView.FindControl( "NS" );
//           if( NSCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.NS = NSCheckBox.Checked;
//           }
////ON

//           CheckBox ONCheckBox = ( CheckBox )StateFormView.FindControl( "ON" );
//           if( ONCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.ON = ONCheckBox.Checked;
//           }
////PE

//           CheckBox PECheckBox = ( CheckBox )StateFormView.FindControl( "PE" );
//           if( PECheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.PE = PECheckBox.Checked;
//           }


////QC
//           CheckBox QCCheckBox = ( CheckBox )StateFormView.FindControl( "QC" );
//           if( QCCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.QC = QCCheckBox.Checked;
//           }
////SK
//           CheckBox SKCheckBox = ( CheckBox )StateFormView.FindControl( "SK" );
//           if( SKCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.SK = SKCheckBox.Checked;
//           }
////YT

//           CheckBox YTCheckBox = ( CheckBox )StateFormView.FindControl( "YT" );
//           if( YTCheckBox != null )
//           {
//               dataRelay.EditedDocumentContentFront.GeographicCoverage.YT = YTCheckBox.Checked;
//           }

           #endregion GeographicCoverage

           if( currentDocument != null )
           {
               if( currentDocument.CanHaveInsurance( currentDocument.ScheduleNumber ) == true )
               {
                   TextBox insurancePolicyEffectiveDateTextBox = ( TextBox )ContractVendorInsuranceDatesFormView.FindControl( "InsurancePolicyEffectiveDateTextBox" );
                   string insurancePolicyEffectiveDateString = "";
                   DateTime parseDate;

                   if( insurancePolicyEffectiveDateTextBox != null )
                   {
                       insurancePolicyEffectiveDateString = insurancePolicyEffectiveDateTextBox.Text;
                   }

                   if( insurancePolicyEffectiveDateString.Length > 0 )
                   {
                       if( DateTime.TryParseExact( insurancePolicyEffectiveDateString, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                       {
                           ErrorMessage = "Insurance policy effective date is not a valid date.";
                           bSuccess = false;
                       }
                       else
                       {
                           dataRelay.EditedDocumentContentFront.InsurancePolicyEffectiveDate = parseDate;
                           Session[ "InsurancePolicyEffective" ] = parseDate.ToString( "MM/dd/yyyy" );
                       }
                   }

                   TextBox insurancePolicyExpirationDateTextBox = ( TextBox )ContractVendorInsuranceDatesFormView.FindControl( "InsurancePolicyExpirationDateTextBox" );
                   string insurancePolicyExpirationDateString = "";

                   if( insurancePolicyExpirationDateTextBox != null )
                   {
                       insurancePolicyExpirationDateString = insurancePolicyExpirationDateTextBox.Text;
                   }

                   if( insurancePolicyExpirationDateString.Length > 0 )
                   {
                       if( DateTime.TryParseExact( insurancePolicyExpirationDateString, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                       {
                           ErrorMessage = "Insurance policy expiration date is not a valid date.";
                           bSuccess = false;
                       }
                       else
                       {
                           dataRelay.EditedDocumentContentFront.InsurancePolicyExpirationDate = parseDate;
                           Session[ "InsurancePolicyExpiration" ] = parseDate.ToString( "MM/dd/yyyy" );
                       }
                   }
               }
           }

           TextBox warrantyDurationTextBox = ( TextBox )WarrantyInformationFormView.FindControl( "WarrantyDurationTextBox" );
           if( warrantyDurationTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.WarrantyDuration = warrantyDurationTextBox.Text;
           }

           TextBox warrantyDescriptionTextBox = ( TextBox )WarrantyInformationFormView.FindControl( "WarrantyDescriptionTextBox" );
           if( warrantyDescriptionTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.WarrantyNotes = warrantyDescriptionTextBox.Text;
           }

           DropDownList returnedGoodsPolicyTypeDropDownList = ( DropDownList )ReturnedGoodsPolicyFormView.FindControl( "ReturnedGoodsPolicyTypeDropDownList" );

           if( returnedGoodsPolicyTypeDropDownList != null )
           {
               if( returnedGoodsPolicyTypeDropDownList.SelectedItem != null )
               {
                   dataRelay.EditedDocumentContentFront.ReturnedGoodsPolicyTypeId = int.Parse( returnedGoodsPolicyTypeDropDownList.SelectedItem.Value );
                   dataRelay.EditedDocumentContentFront.ReturnedGoodsPolicyType = returnedGoodsPolicyTypeDropDownList.SelectedItem.Text;
               }
               else
               {
                   dataRelay.EditedDocumentContentFront.ReturnedGoodsPolicyTypeId = -1;
                   dataRelay.EditedDocumentContentFront.ReturnedGoodsPolicyType = "";
               }
           }

           TextBox returnedGoodsPolicyNotesTextBox = ( TextBox )ReturnedGoodsPolicyFormView.FindControl( "ReturnedGoodsPolicyNotesTextBox" );
           if( returnedGoodsPolicyNotesTextBox != null )
           {
               dataRelay.EditedDocumentContentFront.ReturnedGoodsPolicyNotes = returnedGoodsPolicyNotesTextBox.Text;
           }

           return ( bSuccess );
       }

          

       protected void LargeBusinessCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox largeBusinessCheckBox = ( CheckBox )sender;
           CheckBox smallBusinessCheckBox;
           CheckBox smallDisadvantagedCheckBox;
           CheckBox eightACheckBox;
           CheckBox disabledVeteranOwnedCheckBox;
           CheckBox womanOwnedCheckBox;
           CheckBox hubZoneCheckBox;

           FormView contractVendorSocioFormView;

           if( largeBusinessCheckBox != null )
           {
       //        SetDirtyFlag( "ContractVendorSocioFormView" );

               contractVendorSocioFormView = ( FormView )largeBusinessCheckBox.NamingContainer;
               if( contractVendorSocioFormView != null )
               {
                   smallBusinessCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "SmallBusinessCheckBox" );
                   smallDisadvantagedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "SmallDisadvantagedCheckBox" );
                   eightACheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "EightACheckBox" );
                   disabledVeteranOwnedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "DisabledVeteranOwnedCheckBox" );
                   womanOwnedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "WomanOwnedCheckBox" );
                   hubZoneCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "HubZoneCheckBox" );

                   if( smallBusinessCheckBox != null )
                   {
                       if( largeBusinessCheckBox.Checked == true )
                       {
                           smallBusinessCheckBox.Checked = false;

                           // large trumps these
                           smallDisadvantagedCheckBox.Checked = false;
                           smallDisadvantagedCheckBox.Enabled = false;
                           eightACheckBox.Checked = false;
                           eightACheckBox.Enabled = false;
                           disabledVeteranOwnedCheckBox.Checked = false;
                           disabledVeteranOwnedCheckBox.Enabled = false;
                           womanOwnedCheckBox.Checked = false;
                           womanOwnedCheckBox.Enabled = false;
                           hubZoneCheckBox.Checked = false;
                           hubZoneCheckBox.Enabled = false;

                           TriggerContractViewMasterUpdatePanelFromContract();
                       }
                       else  // clearing large
                       {
                           smallDisadvantagedCheckBox.Enabled = true;
                           eightACheckBox.Enabled = true;

                           TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }

       protected void LargeBusinessCheckBox_OnDataBinding( object sender, EventArgs e )
       {
           CheckBox largeBusinessCheckBox = ( CheckBox )sender;

           FormView contractVendorSocioFormView; ;

           if( largeBusinessCheckBox != null )
           {
               contractVendorSocioFormView = ( FormView )largeBusinessCheckBox.NamingContainer;

               if( contractVendorSocioFormView != null )
               {
                   EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )contractVendorSocioFormView.DataItem;

                   int socioBusinessSizeId = editedDocumentContent.SocioBusinessSizeId;

                   if( socioBusinessSizeId == 2  )
                   {
                       largeBusinessCheckBox.Checked = true;
                   }
                   else
                   {
                       largeBusinessCheckBox.Checked = false;
                   }
               }
           }
       }

       protected void SmallBusinessCheckBox_OnDataBinding( object sender, EventArgs e )
       {
           CheckBox smallBusinessCheckBox = ( CheckBox )sender;

           FormView contractVendorSocioFormView; ;

           if( smallBusinessCheckBox != null )
           {
               contractVendorSocioFormView = ( FormView )smallBusinessCheckBox.NamingContainer;

               if( contractVendorSocioFormView != null )
               {
                   EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )contractVendorSocioFormView.DataItem;

                   int socioBusinessSizeId = editedDocumentContent.SocioBusinessSizeId;

                   if( socioBusinessSizeId == 1 )
                   {
                       smallBusinessCheckBox.Checked = true;
                   }
                   else
                   {
                       smallBusinessCheckBox.Checked = false;
                   }
               }
           }
       }

       protected void SmallBusinessCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox smallBusinessCheckBox = ( CheckBox )sender;
           CheckBox largeBusinessCheckBox;
           CheckBox smallDisadvantagedCheckBox;
           CheckBox eightACheckBox;
           CheckBox disabledVeteranOwnedCheckBox;
           CheckBox womanOwnedCheckBox;
           CheckBox hubZoneCheckBox;

           FormView contractVendorSocioFormView;

           if( smallBusinessCheckBox != null )
           {
        //       SetDirtyFlag( "ContractVendorSocioFormView" );

               contractVendorSocioFormView = ( FormView )smallBusinessCheckBox.NamingContainer;
               if( contractVendorSocioFormView != null )
               {
                   largeBusinessCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "LargeBusinessCheckBox" );
                   smallDisadvantagedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "SmallDisadvantagedCheckBox" );
                   eightACheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "EightACheckBox" );
                   disabledVeteranOwnedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "DisabledVeteranOwnedCheckBox" );
                   womanOwnedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "WomanOwnedCheckBox" );
                   hubZoneCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "HubZoneCheckBox" );

                   if( largeBusinessCheckBox != null )
                   {
                       if( smallBusinessCheckBox.Checked == true )
                       {
                           largeBusinessCheckBox.Checked = false;

                           // small allows these
                           smallDisadvantagedCheckBox.Enabled = true;
                           eightACheckBox.Enabled = true;
                           disabledVeteranOwnedCheckBox.Enabled = true;
                           womanOwnedCheckBox.Enabled = true;
                           hubZoneCheckBox.Enabled = true;

                           TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }

       protected bool GetVeteranStatusCheckBoxValue( int socioVetStatusId, string checkBoxType )
       {
           bool bChecked = false;

           if( checkBoxType.CompareTo( "Veteran" ) == 0 )
           {
               if( socioVetStatusId == 1 )
               {
                   bChecked = true;
               }
           }
           else if( checkBoxType.CompareTo( "Disabled Veteran" ) == 0 )
           {
               if( socioVetStatusId == 3 )
               {
                   bChecked = true;
               }
           }
           return ( bChecked );
       }

       protected bool GetBusinessSizeCheckBoxValue( int socioBusinessSizeId, string checkBoxType )
       {
           bool bChecked = false;

           if( checkBoxType.CompareTo( "Large" ) == 0 )
           {
               if( socioBusinessSizeId == 2 )
               {
                   bChecked = true;
               }
           }
           else if( checkBoxType.CompareTo( "Small" ) == 0 )
           {
               if( socioBusinessSizeId == 1 )
               {
                   bChecked = true;
               }
           }
           return ( bChecked );
       }

       protected void VeteranOwnedCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox veteranOwnedCheckBox = ( CheckBox )sender;
           CheckBox disabledVeteranOwnedCheckBox;
 
           FormView contractVendorSocioFormView;

           if( veteranOwnedCheckBox != null )
           {

               contractVendorSocioFormView = ( FormView )veteranOwnedCheckBox.NamingContainer;
               if( contractVendorSocioFormView != null )
               {
                   disabledVeteranOwnedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "DisabledVeteranOwnedCheckBox" );

                   if( disabledVeteranOwnedCheckBox != null )
                   {
                       if( veteranOwnedCheckBox.Checked == true )
                       {
                           disabledVeteranOwnedCheckBox.Checked = false;

                           TriggerContractViewMasterUpdatePanelFromContract();

                       }
                   }
               }
           }
       }

       protected void DisabledVeteranOwnedCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox disabledVeteranOwnedCheckBox = ( CheckBox )sender;
           CheckBox veteranOwnedCheckBox;

           FormView contractVendorSocioFormView;

           if( disabledVeteranOwnedCheckBox != null )
           {

               contractVendorSocioFormView = ( FormView )disabledVeteranOwnedCheckBox.NamingContainer;
               if( contractVendorSocioFormView != null )
               {
                   veteranOwnedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "VeteranOwnedCheckBox" );

                   if( veteranOwnedCheckBox != null )
                   {
                       if( disabledVeteranOwnedCheckBox.Checked == true )
                       {
                           veteranOwnedCheckBox.Checked = false;

                           TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }

       protected void VendorAttributesFormView_OnChange( object sender, EventArgs e )
       {
    //       SetDirtyFlag( "VendorAttributesFormView" );
       }

       protected void WarrantyInformationFormView_OnChange( object sender, EventArgs e )
       {
     //      SetDirtyFlag( "WarrantyInformationFormView" );
       }

       protected void ReturnedGoodsPolicyFormView_OnChange( object sender, EventArgs e )
       {
     //      SetDirtyFlag( "ReturnedGoodsPolicyFormView" );
       }

       protected void ContractVendorInsuranceDatesFormView_OnChange( object sender, EventArgs e )
       {
     //      SetDirtyFlag( "ContractVendorInsuranceDatesFormView" );
       }

       protected void ContractVendorSocioFormView_OnPreRender( object sender, EventArgs e )
       {
           FormView contractVendorSocioFormView = ( FormView )sender;

           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];
           bool bVisible = documentControlPresentation.IsFormViewVisible( "ContractVendorSocioFormView" );
           ContractVendorSocioFormView.Visible = bVisible;
           ContractVendorSocioFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "ContractVendorSocioFormView" );

           if( bVisible )
           {
               CheckBox largeBusinessCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "LargeBusinessCheckBox" );

               if( largeBusinessCheckBox != null )
               {
                   bool bChecked = largeBusinessCheckBox.Checked;
                   // if large
                   if( bChecked == true )
                   {
                       // disable SDB and 8A
                       CheckBox smallDisadvantagedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "SmallDisadvantagedCheckBox" );
                       CheckBox eightACheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "EightACheckBox" );
                       CheckBox disabledVeteranOwnedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "DisabledVeteranOwnedCheckBox" );
                       CheckBox womanOwnedCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "WomanOwnedCheckBox" );
                       CheckBox hubZoneCheckBox = ( CheckBox )contractVendorSocioFormView.FindControl( "HubZoneCheckBox" );

                       if( smallDisadvantagedCheckBox != null )
                       {
                           smallDisadvantagedCheckBox.Enabled = false;
                       }
                       if( eightACheckBox != null )
                       {
                           eightACheckBox.Enabled = false;
                       }
                       if( disabledVeteranOwnedCheckBox != null )
                       {
                           disabledVeteranOwnedCheckBox.Enabled = false;
                       }
                       if( womanOwnedCheckBox != null )
                       {
                           womanOwnedCheckBox.Enabled = false;
                       }
                       if( hubZoneCheckBox != null )
                       {
                           hubZoneCheckBox.Enabled = false;
                       }
                   }
               }
           }
       }

       protected void VendorAttributesFormView_OnPreRender( object sender, EventArgs e )
       {
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           VendorAttributesFormView.Visible = documentControlPresentation.IsFormViewVisible( "VendorAttributesFormView" );
           VendorAttributesFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "VendorAttributesFormView" );
       }

       protected void WarrantyInformationFormView_OnPreRender( object sender, EventArgs e )
       {
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           WarrantyInformationFormView.Visible = documentControlPresentation.IsFormViewVisible( "WarrantyInformationFormView" );
           WarrantyInformationFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "WarrantyInformationFormView" );
       }

       protected void ReturnedGoodsPolicyFormView_OnPreRender( object sender, EventArgs e )
       {
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           ReturnedGoodsPolicyFormView.Visible = documentControlPresentation.IsFormViewVisible( "ReturnedGoodsPolicyFormView" );
           ReturnedGoodsPolicyFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "ReturnedGoodsPolicyFormView" );

       }

       protected void StateFormView_OnPreRender( object sender, EventArgs e )
       {
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           StateFormView.Visible = documentControlPresentation.IsFormViewVisible( "StateFormView" );
           StateFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "StateFormView" );
       }

       protected void ContractVendorInsuranceDatesFormView_OnPreRender( object sender, EventArgs e )
       {
           DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

           bool bVisible = documentControlPresentation.IsFormViewVisible( "ContractVendorInsuranceDatesFormView" );

           ContractVendorInsuranceDatesFormView.Visible = bVisible;
           ContractVendorInsuranceDatesFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "ContractVendorInsuranceDatesFormView" );

           if( bVisible == true )
           {
               EnableContractVendorDateEditing( ( FormView )sender );
           }
       }

       public static Control FindControlRecursive( Control root, string id )
       {
           if( ( root.ID != null ) && ( root.ID == id ) ) return root;

           foreach( Control ctrl in root.Controls )
           {
               Control found = FindControlRecursive( ctrl, id );
               if( found != null ) return found;
           }

           return null;
       }

       protected void DistributorCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox distributorCheckBox = ( CheckBox )sender;
           CheckBox servicesCheckBox;

           FormView vendorAttributesFormView;

           if( distributorCheckBox != null )
           {
               vendorAttributesFormView = ( FormView )distributorCheckBox.NamingContainer;

               if( vendorAttributesFormView != null )
               {
                   servicesCheckBox = ( CheckBox )vendorAttributesFormView.FindControl( "ServicesCheckBox" );

                   if( servicesCheckBox != null )
                   {
                       if( distributorCheckBox.Checked == true )
                       {
                           // manufacturer or distributor trumps service
                           servicesCheckBox.Checked = false;

                           TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }

       protected void ManufacturerCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox manufacturerCheckBox = ( CheckBox )sender;
           CheckBox servicesCheckBox;

           FormView vendorAttributesFormView;

           if( manufacturerCheckBox != null )
           {
               vendorAttributesFormView = ( FormView )manufacturerCheckBox.NamingContainer;

               if( vendorAttributesFormView != null )
               {
                   servicesCheckBox = ( CheckBox )vendorAttributesFormView.FindControl( "ServicesCheckBox" );

                   if( servicesCheckBox != null )
                   {
                       if( manufacturerCheckBox.Checked == true )
                       {
                           // manufacturer or distributor trumps service
                           servicesCheckBox.Checked = false;

                           TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }

       protected void ServicesCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox servicesCheckBox = ( CheckBox )sender;
           CheckBox distributorCheckBox;
           CheckBox manufacturerCheckBox;

           FormView vendorAttributesFormView;

           if( servicesCheckBox != null )
           {
               vendorAttributesFormView = ( FormView )servicesCheckBox.NamingContainer;

               if( vendorAttributesFormView != null )
               {
                   distributorCheckBox = ( CheckBox )vendorAttributesFormView.FindControl( "DistributorCheckBox" );
                   manufacturerCheckBox = ( CheckBox )vendorAttributesFormView.FindControl( "ManufacturerCheckBox" );

                   if( distributorCheckBox != null && manufacturerCheckBox != null )
                   {
                       if( servicesCheckBox.Checked == true )
                       {
                           // service trumps manufacturer and distributor
                           manufacturerCheckBox.Checked = false;
                           distributorCheckBox.Checked = false;

                           TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }

        //  _vendorType = new string[] { "", "Distributor", "Manufacturer", "Both", "", "Services" };  // 1,2,3,5 _vendorTypeId
        protected void DistributorCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox distributorCheckBox = ( CheckBox )sender;

            FormView vendorAttributesFormView;;

            if( distributorCheckBox != null )
            {
                vendorAttributesFormView = ( FormView )distributorCheckBox.NamingContainer;

                if( vendorAttributesFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )vendorAttributesFormView.DataItem;

                    int vendorTypeId = editedDocumentContent.VendorTypeId;
 
                   if( vendorTypeId == 1 || vendorTypeId == 3 )
                   {
                       distributorCheckBox.Checked = true;
                   }
                   else
                   {
                       distributorCheckBox.Checked = false;
                   }

                }
            }
       }

        //  _vendorType = new string[] { "", "Distributor", "Manufacturer", "Both", "", "Services" };  // 1,2,3,5 _vendorTypeId
        protected void ManufacturerCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox manufacturerCheckBox = ( CheckBox )sender;

            FormView vendorAttributesFormView; ;

            if( manufacturerCheckBox != null )
            {
                vendorAttributesFormView = ( FormView )manufacturerCheckBox.NamingContainer;

                if( vendorAttributesFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )vendorAttributesFormView.DataItem;

                    int vendorTypeId = editedDocumentContent.VendorTypeId;

                    if( vendorTypeId == 2 || vendorTypeId == 3 )
                    {
                        manufacturerCheckBox.Checked = true;
                    }
                    else
                    {
                        manufacturerCheckBox.Checked = false;
                    }

                }
            }
        }


        //  _vendorType = new string[] { "", "Distributor", "Manufacturer", "Both", "", "Services" };  // 1,2,3,5 _vendorTypeId
        protected void ServicesCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox servicesCheckBox = ( CheckBox )sender;

            FormView vendorAttributesFormView; ;

            if( servicesCheckBox != null )
            {
                vendorAttributesFormView = ( FormView )servicesCheckBox.NamingContainer;

                if( vendorAttributesFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )vendorAttributesFormView.DataItem;

                    int vendorTypeId = editedDocumentContent.VendorTypeId;

                    if( vendorTypeId == 5 )
                    {
                        servicesCheckBox.Checked = true;
                    }
                    else
                    {
                        servicesCheckBox.Checked = false;
                    }

                }
            }
        }

       public bool GetSelected( string stateName )
       {
           bool bIsSelected = false;

           GeographicCoverage currentValues = null;

           if( DataRelay != null )
           {
               if( DataRelay.EditedDocumentContentFront != null )
               {
                   if( DataRelay.EditedDocumentContentFront.GeographicCoverage != null )
                   {
                       currentValues = DataRelay.EditedDocumentContentFront.GeographicCoverage;
                   }
               }
           }

           if( currentValues != null )
           {
               bIsSelected = currentValues.GetValueByName( stateName );
           }

           return ( bIsSelected );
       }

       private const string _stateGroups = "Group52,Group51,Group50,Group49";
       private const string _lower48StateAbbreviations = "AL,AZ,AR,CA,CO,CT,DE,FL,GA,ID,IL,IN,IA,KS,KY,LA,ME,MD,MA,MI,MN,MS,MO,MT,NE,NV,NH,NJ,NM,NY,NC,ND,OH,OK,OR,PA,RI,SC,SD,TN,TX,UT,VT,VA,WA,WV,WI,WY";
       private const string _specialStateAbbreviations = "DC,PR,AK,HI";
       // per Sandee not supporting Canada 7/6/2016
       private const string _canadaAbbreviations = "AB,BC,MB,NB,NF,NT,NS,ON,PE,QC,SK,YT";


       protected void StateCheckBox_OnCheckChanged( object sender, EventArgs e )
       {      
            CheckBox stateCheckBox = ( CheckBox )sender;

            string changedName = stateCheckBox.ID;
            bool bNewValue = stateCheckBox.Checked;

            SetDirtyFlag( "StateFormView" );  // this is the only gui set dirty flag, all others should be set during the save

            GeographicCoverage currentValues = null;

            if( DataRelay != null )
            {
                if( DataRelay.EditedDocumentContentFront != null )
                {
                    if( DataRelay.EditedDocumentContentFront.GeographicCoverage != null )
                    {
                        currentValues = DataRelay.EditedDocumentContentFront.GeographicCoverage;
                    }
                }
            }

            if( currentValues != null )
            {
                if( _stateGroups.IndexOf( changedName ) >= 0 )
                {
                    if( changedName.CompareTo( "Group52" ) == 0 )
                    {
                        currentValues.Group52 = bNewValue;

                        if( bNewValue == true )
                        {
                            currentValues.ClearStateGroup( "Group51" );
                            currentValues.ClearStateGroup( "Group50" );
                            currentValues.ClearStateGroup( "Group49" );
                        }
                        currentValues.CheckLower48StatesPlus( bNewValue, true, true, true );
                    }
                    else if( changedName.CompareTo( "Group51" ) == 0 )
                    {
                        currentValues.Group51 = bNewValue;

                        if( bNewValue == true )
                        {
                            currentValues.ClearStateGroup( "Group52" );
                            currentValues.ClearStateGroup( "Group50" );
                            currentValues.ClearStateGroup( "Group49" );
                        }
                        currentValues.CheckLower48StatesPlus( bNewValue, false, true, true );
                    }
                    else if( changedName.CompareTo( "Group50" ) == 0 )
                    {
                        currentValues.Group50 = bNewValue;

                        if( bNewValue == true )
                        {
                            currentValues.ClearStateGroup( "Group52" );
                            currentValues.ClearStateGroup( "Group51" );
                            currentValues.ClearStateGroup( "Group49" );
                        }
                        currentValues.CheckLower48StatesPlus( bNewValue, false, false, true );
                    }
                    else if( changedName.CompareTo( "Group49" ) == 0 )
                    {
                        currentValues.Group49 = bNewValue;
                        
                        if( bNewValue == true )
                        {
                            currentValues.ClearStateGroup( "Group52" );
                            currentValues.ClearStateGroup( "Group51" );
                            currentValues.ClearStateGroup( "Group50" );
                        }
                        currentValues.CheckLower48StatesPlus( bNewValue, false, true, false );
                    }
                }
                else if( _lower48StateAbbreviations.IndexOf( changedName ) >= 0 || _specialStateAbbreviations.IndexOf( changedName ) >= 0 )
                {
                    currentValues.SetValueByName( changedName, bNewValue );

                    // look at all states and see if the one just set/cleared completes a set 
                    // and then also check the appropriate group
                    currentValues.SetGroupFromStates();
                }
                else if( _canadaAbbreviations.IndexOf( changedName ) >= 0 )
                {
                    currentValues.SetValueByName( changedName, bNewValue );
                }
      
                StateFormView.DataBind();

                TriggerContractViewMasterUpdatePanelFromContract();
            }
       }

    }
}