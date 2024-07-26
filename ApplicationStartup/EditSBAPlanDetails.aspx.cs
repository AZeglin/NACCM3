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

using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class EditSBAPlanDetails : System.Web.UI.Page
    {
        private EditSBAPlanDetailsWindowParms _editSBAPlanDetailsWindowParms = null;

        public bool PlanDetailsWereModified
        {
            get
            {
                bool bDirty = false;
                if( Session[ "PlanDetailsWereModified" ] != null )
                {
                    bDirty = bool.Parse( Session[ "PlanDetailsWereModified" ].ToString() );
                }
                return ( bDirty );
            }
            set
            {
                Session[ "PlanDetailsWereModified" ] = value;
            }
        }

        public bool SelectedPlanWasModified
        {
            get
            {
                bool bDirty = false;
                if( Session[ "SelectedPlanWasModified" ] != null )
                {
                    bDirty = bool.Parse( Session[ "SelectedPlanWasModified" ].ToString() );
                }
                return ( bDirty );
            }
            set
            {
                Session[ "SelectedPlanWasModified" ] = value;
            }
        }



        [Serializable]
        public class PlanDetails
        {
            public int SBAPlanId;
            public string PlanName;
            public int PlanTypeId;
            public string PlanTypeDescription;
            public string AdministratorName;
            
            public string AdministratorAddress;
            public string AdministratorCity;
            public int AdministratorCountryId;
            public string AdministratorState;
            public string AdministratorZip;
            public string AdministratorPhone;
            public string AdministratorFax;
            public string AdministratorEmail;
            public string PlanNotes;
            public string PlanComments;
        }

        private StringBuilder _validationMessageStringBuilder = null;

        protected void Page_Load( object sender, EventArgs e )
        {
            //if( Session[ "NACCMStartedProperly" ] == null || Request.UrlReferrer == null )
            //{
            //    Response.Redirect( "~/Start.aspx" );
            //}
            
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }

      //      CMGlobals.AddKeepAlive( this.Page, 12000 );

            int selectedSBAPlanId = -1;

            //bool bSelectedPlanWasModified = false;
            //if( Session[ "SelectedPlanWasModified" ] != null )
            //    bSelectedPlanWasModified = ( bool )Session[ "SelectedPlanWasModified" ];

            if( Session[ "EditSBAPlanDetailsWindowParms" ] != null )
            {
                _editSBAPlanDetailsWindowParms = ( EditSBAPlanDetailsWindowParms )Session[ "EditSBAPlanDetailsWindowParms" ];
                if( _editSBAPlanDetailsWindowParms.EditSBAPlanAction != EditSBAPlanDetailsWindowParms.EditSBAPlanActions.CreateNewPlan )
                {
                    if( Page.IsPostBack == false  )
                    {
                        // edit the existing associated plan
                        if( _editSBAPlanDetailsWindowParms.SBAPlanId != -1 )
                        {
                            LoadScreen( _editSBAPlanDetailsWindowParms.ContractNumber, _editSBAPlanDetailsWindowParms.SBAPlanId, true, _editSBAPlanDetailsWindowParms.IsContractExpired );
                            Session[ "SelectedSBAPlanId" ] = _editSBAPlanDetailsWindowParms.SBAPlanId;
                        }
                        else  // assign an existing plan, where there was none before - possibly editing it
                        {                
                            ClearScreen();
                            LoadAndBindActivePlans( -1, true, false );
                            LoadAndBindPlanTypes( CMGlobals.PLANTYPEFORCOMPANY ); // defaults to "Company"
                            LoadAndBindCountries( -1 );
                            LoadAndBindStates( -1, "--" ); // defaults to -- ( no selection )
                        }
                    }
                    // different plan was selected
                    //else if( bSelectedPlanWasModified == true )
                    //{
                    //    selectedSBAPlanId = int.Parse( Session[ "SelectedSBAPlanId" ].ToString() );
                    //    LoadScreen( _editSBAPlanDetailsWindowParms.ContractNumber, selectedSBAPlanId );
                    //}
                  
                }
                else  // new plan
                {
                    if( Page.IsPostBack == false )
                    {
                        ClearScreen();
                        LoadAndBindPlanTypes( CMGlobals.PLANTYPEFORCOMPANY ); // defaults to "Company"
                        LoadAndBindCountries( -1 );
                        LoadAndBindStates( -1, "--" ); // defaults to -- ( no selection )
                    }
                }        
            }
        }

        private void ClearSessionVariables()
        {
            Session[ "SBAPlanDetails" ] = null; 
            Session[ "PlanDetailsWereModified" ] = null;
            Session[ "SelectedPlanWasModified" ] = null;
            Session[ "LastInsertedSBAPlanId" ] = null;
            Session[ "ActiveSBAPlansDataSet" ] = null;
            Session[ "SBAPlanTypeDataSet" ] = null;
            Session[ "CountryDataSet" ] = null;
            Session[ "StateCodeDataSet" ] = null;
            Session[ "SelectedSBAPlanId" ] = null;
        }

        private void LoadAndBindActivePlans( int selectedSBAPlanId, bool bIncludeSelect, bool bIncludeInactive )
        {
            bool bSuccess = true;
            DataSet dsSBAPlans = null;
            ContractDB contractDB = null;

            DropDownList ActivePlansDropDownList = ( DropDownList )EditSBAPlanDetailsPanel.FindControl( "ActivePlansDropDownList" );

            //if( Session[ "ActiveSBAPlansDataSet" ] != null )
            //{
            //    dsSBAPlans = ( DataSet )Session[ "ActiveSBAPlansDataSet" ];
            //}
            //else
            //{
                contractDB = ( ContractDB )Session[ "ContractDB" ];
            
                bSuccess = contractDB.SelectActiveSBAPlans2( bIncludeSelect, bIncludeInactive, ref dsSBAPlans );
            //}
            
            if( bSuccess == true )
            {
                ActivePlansDropDownList.ClearSelection();
                ActivePlansDropDownList.Items.Clear();
                ActivePlansDropDownList.DataSource = dsSBAPlans;
                ActivePlansDropDownList.DataMember = ContractDB.ActiveSBAPlansTableName;
                ActivePlansDropDownList.DataTextField = "PlanName";
                ActivePlansDropDownList.DataValueField = "SBAPlanId";
                ActivePlansDropDownList.DataBind();

              //  if( selectedSBAPlanId != -1 )  // allow -1 as it is returned with "--select--"
                    ActivePlansDropDownList.SelectedValue = selectedSBAPlanId.ToString();

              //  Session[ "ActiveSBAPlansDataSet" ] = dsSBAPlans;
            }
            else
            {
                MsgBox.Alert( contractDB.ErrorMessage );
            }
            
        }

        private void LoadAndBindPlanTypes( int selectedPlanTypeId )
        {
            bool bSuccess = true;
            DataSet dsPlanTypes = null;
            ContractDB contractDB = null;

            DropDownList PlanTypeDropDownList = ( DropDownList )EditSBAPlanDetailsPanel.FindControl( "PlanTypeDropDownList" );

            if( Session[ "SBAPlanTypeDataSet" ] != null )
            {
                dsPlanTypes = ( DataSet )Session[ "SBAPlanTypeDataSet" ];
            }
            else
            {
                contractDB = ( ContractDB )Session[ "ContractDB" ];

                bSuccess = contractDB.SelectSBAPlanTypes( ref dsPlanTypes );
            }

            if( bSuccess == true )
            {
                PlanTypeDropDownList.ClearSelection();
                PlanTypeDropDownList.Items.Clear();
                PlanTypeDropDownList.DataSource = dsPlanTypes;
                PlanTypeDropDownList.DataMember = "SBAPlanTypesTable";
                PlanTypeDropDownList.DataTextField = "PlanTypeDescription";
                PlanTypeDropDownList.DataValueField = "PlanTypeId";
                PlanTypeDropDownList.DataBind();

                PlanTypeDropDownList.SelectedValue = selectedPlanTypeId.ToString();

                Session[ "SBAPlanTypeDataSet" ] = dsPlanTypes;
            }
            else
            {
                MsgBox.Alert( contractDB.ErrorMessage );
            }
            
        }

        private void LoadAndBindCountries( int selectedCountryId )
        {
            bool bSuccess = true;
            DataSet dsCountries = null;
            ContractDB contractDB = null;

            DropDownList AdministratorCountryDropDownList = ( DropDownList )EditSBAPlanDetailsPanel.FindControl( "AdministratorCountryDropDownList" );

            if( Session[ "CountryDataSet" ] != null )
            {
                dsCountries = ( DataSet )Session[ "CountryDataSet" ];
            }
            else
            {
                contractDB = ( ContractDB )Session[ "ContractDB" ];

                // int vendorCountryId = 239; // $$$ placeholder 

                bSuccess = contractDB.SelectCountries( ref dsCountries );
            }

            if( bSuccess == true )
            {
                AdministratorCountryDropDownList.ClearSelection();
                AdministratorCountryDropDownList.Items.Clear();
                AdministratorCountryDropDownList.DataSource = dsCountries;
                AdministratorCountryDropDownList.DataMember = ContractDB.VendorCountriesTableName;
                AdministratorCountryDropDownList.DataTextField = "CountryName";
                AdministratorCountryDropDownList.DataValueField = "CountryId";
                AdministratorCountryDropDownList.DataBind();

                AdministratorCountryDropDownList.SelectedValue = selectedCountryId.ToString();

                Session[ "CountryDataSet" ] = dsCountries;
            }
            else
            {
                MsgBox.Alert( contractDB.ErrorMessage );
            }
        }

        private void LoadAndBindStates( int selectedCountryId, string selectedAbbreviation )
        {
            bool bSuccess = true;
            DataSet dsStateCodes = null;
            ContractDB contractDB = null;

            DropDownList StateDropDownList = ( DropDownList )EditSBAPlanDetailsPanel.FindControl( "StateDropDownList" );

          
            contractDB = ( ContractDB )Session[ "ContractDB" ];
            
            // int vendorCountryId = 239; // $$$ placeholder 

            bSuccess = contractDB.SelectStateCodes( ref dsStateCodes, selectedCountryId );
            
            if( bSuccess == true )
            {
                StateDropDownList.ClearSelection();
                StateDropDownList.Items.Clear();
                StateDropDownList.DataSource = dsStateCodes;
                StateDropDownList.DataMember = ContractDB.StateCodesTableName;
                StateDropDownList.DataTextField = "StateAbbreviation";
                StateDropDownList.DataValueField = "StateAbbreviation";
                StateDropDownList.DataBind();

                StateDropDownList.SelectedValue = selectedAbbreviation;

                Session[ "StateCodeDataSet" ] = dsStateCodes;
            }
            else
            {
                MsgBox.Alert( contractDB.ErrorMessage );
            }
        }

        private void LoadScreen( string contractNumber, int sbaPlanId, bool bReloadActivePlans, bool bIsContractExpired )
        {
            PlanDetails planDetails = new PlanDetails();
            planDetails.SBAPlanId = sbaPlanId;

            bool bSuccess = true;

            if( sbaPlanId != -1 )
            {
                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];

                DataSet dsOneSBAPlanRow = null;
                bSuccess = contractDB.GetSBAPlanInfo( ref dsOneSBAPlanRow, contractNumber, sbaPlanId );
                if( bSuccess == true )
                {
                    if( contractDB.RowsReturned > 0 )
                    {
                        DataTable dtOneSBAPlanRow = dsOneSBAPlanRow.Tables[ ContractDB.SBAPlanDetailsTableName ];
                        DataRow row = dtOneSBAPlanRow.Rows[ 0 ];
                        if( row.GetType() != typeof( DBNull ) )
                        {
                            if( row.IsNull( "PlanName" ) == false )
                                planDetails.PlanName = ( string )row[ "PlanName" ];
                            if( row.IsNull( "PlanTypeId" ) == false )
                                planDetails.PlanTypeId = ( int )row[ "PlanTypeId" ];
                            if( row.IsNull( "PlanTypeDescription" ) == false )
                                planDetails.PlanTypeDescription = ( string )row[ "PlanTypeDescription" ];
                            if( row.IsNull( "PlanAdministratorName" ) == false )
                                planDetails.AdministratorName = ( string )row[ "PlanAdministratorName" ];
                            if( row.IsNull( "PlanAdministratorAddress" ) == false )
                                planDetails.AdministratorAddress = ( string )row[ "PlanAdministratorAddress" ];

                            if( row.IsNull( "PlanAdministratorCity" ) == false )
                                planDetails.AdministratorCity = ( string )row[ "PlanAdministratorCity" ];

                            if( row.IsNull( "PlanAdministratorCountryId" ) == false )
                                planDetails.AdministratorCountryId = ( int )row[ "PlanAdministratorCountryId" ];

                            if( row.IsNull( "PlanAdministratorState" ) == false )
                                planDetails.AdministratorState = ( string )row[ "PlanAdministratorState" ];

                            if( row.IsNull( "PlanAdministratorZip" ) == false )
                                planDetails.AdministratorZip = ( string )row[ "PlanAdministratorZip" ];

                            if( row.IsNull( "PlanAdministratorPhone" ) == false )
                                planDetails.AdministratorPhone = ( string )row[ "PlanAdministratorPhone" ];
                            if( row.IsNull( "PlanAdministratorFax" ) == false )
                                planDetails.AdministratorFax = ( string )row[ "PlanAdministratorFax" ];
                            if( row.IsNull( "PlanAdministratorEmail" ) == false )
                                planDetails.AdministratorEmail = ( string )row[ "PlanAdministratorEmail" ];
                            if( row.IsNull( "PlanNotes" ) == false )
                                planDetails.PlanNotes = ( string )row[ "PlanNotes" ];
                            if( row.IsNull( "Comments" ) == false )
                                planDetails.PlanComments = ( string )row[ "Comments" ];

                            if( bReloadActivePlans == true )
                            {
                                if( bIsContractExpired == false )
                                {
                                    LoadAndBindActivePlans( planDetails.SBAPlanId, false, false );
                                }
                                else
                                {
                                    LoadAndBindActivePlans( planDetails.SBAPlanId, false, true );
                                }
                            }

                            LoadAndBindPlanTypes( planDetails.PlanTypeId );
                            LoadAndBindCountries( planDetails.AdministratorCountryId );
                            LoadAndBindStates( planDetails.AdministratorCountryId, planDetails.AdministratorState );
                        }
                    }
                    else // zero rows returned
                    {
                        ClearScreen();
                        planDetails.PlanTypeId = CMGlobals.PLANTYPEFORNOPLAN;
                        LoadAndBindPlanTypes( CMGlobals.PLANTYPEFORNOPLAN );
                        planDetails.AdministratorState = "--";
                        LoadAndBindCountries( -1 );
                        LoadAndBindStates( -1, "--" ); // defaults to -- ( no selection )
                    }
                }
                else
                {
                    MsgBox.Alert( contractDB.ErrorMessage );
                }
            }
            else  // no plan available
            {
                ClearScreen();
                planDetails.PlanTypeId = CMGlobals.PLANTYPEFORNOPLAN;
                LoadAndBindPlanTypes( CMGlobals.PLANTYPEFORNOPLAN );
                planDetails.AdministratorState = "--";
                LoadAndBindCountries( -1 );
                LoadAndBindStates( -1, "--" ); // defaults to -- ( no selection )
            }
                
            Session[ "SBAPlanDetails" ] = planDetails;     

            PopulatePlanDetailFields( planDetails );
                                        
        }

        protected void EditSBAPlanDetailsPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "EditSBAPlanDetailsPanel" );

            EditSBAPlanDetailsPanel.Visible = bVisible;
            EditSBAPlanDetailsPanel.Enabled = documentControlPresentation.IsControlEnabled( "EditSBAPlanDetailsPanel" );

            Label SBAPlanDetailsFormViewHeaderLabel = ( Label )EditSBAPlanDetailsPanel.FindControl( "SBAPlanDetailsFormViewHeaderLabel" );
            Label PlanNameLabel = ( Label )EditSBAPlanDetailsPanel.FindControl( "PlanNameLabel" );
            TextBox NewPlanNameTextBox = ( TextBox )EditSBAPlanDetailsPanel.FindControl( "NewPlanNameTextBox" );
            DropDownList ActivePlansDropDownList = ( DropDownList )EditSBAPlanDetailsPanel.FindControl( "ActivePlansDropDownList" );
            Button EditSBAPlanDetailsSaveButton = ( Button )EditSBAPlanDetailsPanel.FindControl( "EditSBAPlanDetailsSaveButton" );


            if( _editSBAPlanDetailsWindowParms.EditSBAPlanAction == EditSBAPlanDetailsWindowParms.EditSBAPlanActions.CreateNewPlan )
            {
                if( SBAPlanDetailsFormViewHeaderLabel != null )
                {
                    SBAPlanDetailsFormViewHeaderLabel.Text = "Add New SBA Plan";
                }

                if( PlanNameLabel != null )
                {
                    PlanNameLabel.Text = "New Plan Name";
                }
              
                if( ActivePlansDropDownList != null )
                {
                    ActivePlansDropDownList.Visible = false;
                }

                if( NewPlanNameTextBox != null )
                {
                    NewPlanNameTextBox.Visible = true;
                }

                if( EditSBAPlanDetailsSaveButton != null )
                {
                    EditSBAPlanDetailsSaveButton.Text = "Done";
                }
            }
            else if( _editSBAPlanDetailsWindowParms.EditSBAPlanAction == EditSBAPlanDetailsWindowParms.EditSBAPlanActions.SelectDifferentPlan )
            {
                if( SBAPlanDetailsFormViewHeaderLabel != null )
                {
                    SBAPlanDetailsFormViewHeaderLabel.Text = "Select Different SBA Plan";
                } 
                
                if( PlanNameLabel != null )
                {
                    PlanNameLabel.Text = "Select Plan";
                }

                if( ActivePlansDropDownList != null )
                {
                    ActivePlansDropDownList.Visible = true;
                }

                if( NewPlanNameTextBox != null )
                {
                    NewPlanNameTextBox.Visible = false;
                }

                if( EditSBAPlanDetailsSaveButton != null )
                {
                    EditSBAPlanDetailsSaveButton.Text = "Done";
                }
            }
            else // edit existing plan
            {
                if( SBAPlanDetailsFormViewHeaderLabel != null )
                {
                    SBAPlanDetailsFormViewHeaderLabel.Text = "Edit Current SBA Plan";
                }

                if( PlanNameLabel != null )
                {
                    PlanNameLabel.Text = "Plan Name";
                }

                if( ActivePlansDropDownList != null )
                {
                    ActivePlansDropDownList.Visible = false;
                }

                if( NewPlanNameTextBox != null )
                {
                    NewPlanNameTextBox.Visible = true;
                }

                if( EditSBAPlanDetailsSaveButton != null )
                {
                    EditSBAPlanDetailsSaveButton.Text = "Save";
                }
            }

        }

        private void PopulatePlanDetailFields( PlanDetails planDetails )
        {
            ActivePlansDropDownList.SelectedValue = planDetails.SBAPlanId.ToString();
            NewPlanNameTextBox.Text = planDetails.PlanName;
            PlanTypeDropDownList.SelectedValue = planDetails.PlanTypeId.ToString();

            StateDropDownList.SelectedValue = planDetails.AdministratorState;

            AdministratorNameTextBox.Text = planDetails.AdministratorName;

            AdministratorEmailTextBox.Text = planDetails.AdministratorEmail;
            AdministratorAddressTextBox.Text = planDetails.AdministratorAddress;
            AdministratorCityTextBox.Text = planDetails.AdministratorCity;

            AdministratorCountryDropDownList.SelectedValue = planDetails.AdministratorCountryId.ToString();

            AdministratorZipTextBox.Text = planDetails.AdministratorZip;
            AdministratorPhoneTextBox.Text = planDetails.AdministratorPhone;
            AdministratorFaxTextBox.Text = planDetails.AdministratorFax;
        }

        private void ClearScreen()
        {
            if( NewPlanNameTextBox.Visible == true )
                NewPlanNameTextBox.Text = "";

            if( ActivePlansDropDownList.Visible == true )
                ActivePlansDropDownList.ClearSelection();

            if( PlanTypeDropDownList.Visible == true )
                PlanTypeDropDownList.ClearSelection();

            if( AdministratorCountryDropDownList.Visible == true )
                AdministratorCountryDropDownList.ClearSelection();

            AdministratorNameTextBox.Text = "";

            AdministratorEmailTextBox.Text = "";
            AdministratorAddressTextBox.Text = "";
            AdministratorCityTextBox.Text = "";
            AdministratorZipTextBox.Text = "";
            AdministratorPhoneTextBox.Text = "";
            AdministratorFaxTextBox.Text = "";

            StateDropDownList.SelectedValue = "--";  // no selection
        }

        protected void EditSBAPlanDetailsSaveButton_OnClick( object sender, EventArgs e )
        {
            if( SavePlanDetails() == true )
            {
                bool bSBAPlanIdChanged = ( ( SelectedPlanWasModified == true ) || ( _editSBAPlanDetailsWindowParms.EditSBAPlanAction == EditSBAPlanDetailsWindowParms.EditSBAPlanActions.CreateNewPlan ) ) ? true : false;
                CloseWindow( true, bSBAPlanIdChanged );
            }
        }

        private void CloseWindow( bool bWithRefresh, bool bSBAPlanIdChanged )
        {
            string closeWindowScript = "CloseWindow( \"false\", \"false\" );";

            if( bWithRefresh == true && bSBAPlanIdChanged == true )
                closeWindowScript = "CloseWindow( \"true\",\"true\" );";
            else if( bWithRefresh == true && bSBAPlanIdChanged == false )
                closeWindowScript = "CloseWindow( \"true\",\"false\" );";
         
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "CloseWindowScript", closeWindowScript, true );
        }

        private bool SavePlanDetails()
        {
            bool bSuccess = true;
     
            PlanDetails planDetails = null;
            if( Session[ "SBAPlanDetails" ] != null )
            {
                planDetails = ( PlanDetails )Session[ "SBAPlanDetails" ];
            }
            else
            {
                planDetails = new PlanDetails();
                Session[ "SBAPlanDetails" ] = planDetails;
            }

            if( _editSBAPlanDetailsWindowParms.EditSBAPlanAction == EditSBAPlanDetailsWindowParms.EditSBAPlanActions.CreateNewPlan )
            {
                planDetails.SBAPlanId = -1;
                planDetails.PlanName = NewPlanNameTextBox.Text;
            }
            else if( _editSBAPlanDetailsWindowParms.EditSBAPlanAction == EditSBAPlanDetailsWindowParms.EditSBAPlanActions.SelectDifferentPlan )
            {
                ListItem selectedPlanItem = null;
                selectedPlanItem = ActivePlansDropDownList.SelectedItem;
                int selectedSBAPlanId = -1;
                string selectedPlanName = "";
                if( selectedPlanItem != null )
                {
                    selectedSBAPlanId = int.Parse( selectedPlanItem.Value.ToString() );     
                    selectedPlanName = selectedPlanItem.Text;
                }
                planDetails.SBAPlanId = selectedSBAPlanId;
                planDetails.PlanName = selectedPlanName;
            }
            else // edit existing ( note plan id doesn't change )
            {
                planDetails.PlanName = NewPlanNameTextBox.Text;
            }


            planDetails.PlanTypeId = int.Parse( PlanTypeDropDownList.SelectedValue.ToString() );

            planDetails.AdministratorState = StateDropDownList.SelectedValue;

            planDetails.AdministratorCountryId = int.Parse( AdministratorCountryDropDownList.SelectedValue.ToString() );
            
            planDetails.AdministratorName = AdministratorNameTextBox.Text;
            planDetails.AdministratorEmail = AdministratorEmailTextBox.Text;
            planDetails.AdministratorAddress = AdministratorAddressTextBox.Text;
            planDetails.AdministratorCity = AdministratorCityTextBox.Text;
            planDetails.AdministratorZip = AdministratorZipTextBox.Text;
            planDetails.AdministratorPhone = AdministratorPhoneTextBox.Text;
            planDetails.AdministratorFax = AdministratorFaxTextBox.Text;

                                                                                            
            string validationMessage = "";
            if( bSuccess = ValidateBeforeSave( planDetails, ref validationMessage, "EditSBAPlanDetails" ) == true )
            {
                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                if( _editSBAPlanDetailsWindowParms.EditSBAPlanAction == EditSBAPlanDetailsWindowParms.EditSBAPlanActions.CreateNewPlan )
                {
                    int newSBAPlanId = -1;

                    // note: this does not save the new id to the parent contract record
                    bSuccess = contractDB.CreateSBAPlan( _editSBAPlanDetailsWindowParms.ContractNumber,
                        planDetails.PlanName,
                        planDetails.PlanTypeId,
                        planDetails.AdministratorName,
                        planDetails.AdministratorAddress,
                        planDetails.AdministratorCity,
                        planDetails.AdministratorCountryId,
                        planDetails.AdministratorState,
                        planDetails.AdministratorZip,
                        planDetails.AdministratorPhone,
                        planDetails.AdministratorFax,
                        planDetails.AdministratorEmail,
                        planDetails.PlanNotes,
                        planDetails.PlanComments,
                        ref newSBAPlanId );

                    if( bSuccess == true )
                    {
                        Session[ "LastInsertedSBAPlanId" ] = newSBAPlanId;
                        planDetails.SBAPlanId = newSBAPlanId;

                        // save the new planId to the front object only
                        DataRelay dataRelay = ( DataRelay )Session[ "DataRelay" ];
                        dataRelay.RestoreDelegatesAfterDeserialization();

                        if( dataRelay != null )
                        {
                            if( dataRelay.EditedDocumentContentFront != null )
                            {
                                if( dataRelay.EditedDocumentContentFront.SBAPlanId != planDetails.SBAPlanId )
                                    Session[ "SelectedPlanWasModified" ] = true;
                                dataRelay.EditedDocumentContentFront.SBAPlanId = newSBAPlanId;
                            }
                        }
                    }
                    else
                    {
                        MsgBox.Alert( contractDB.ErrorMessage );
                    }
                }
                else
                {
                    // note: if sbaId has changed, this does not select the new id to the parent contract
                    bSuccess = contractDB.UpdateSBAPlan( _editSBAPlanDetailsWindowParms.ContractNumber,
                         planDetails.SBAPlanId,
                         planDetails.PlanName,
                         planDetails.PlanTypeId,
                         planDetails.AdministratorName,
                         planDetails.AdministratorAddress,
                         planDetails.AdministratorCity,
                         planDetails.AdministratorCountryId,
                         planDetails.AdministratorState,
                         planDetails.AdministratorZip,
                         planDetails.AdministratorPhone,
                         planDetails.AdministratorFax,
                         planDetails.AdministratorEmail,
                         planDetails.PlanNotes,
                         planDetails.PlanComments );


                    if( bSuccess == true )
                    {
                        if( _editSBAPlanDetailsWindowParms.EditSBAPlanAction == EditSBAPlanDetailsWindowParms.EditSBAPlanActions.SelectDifferentPlan )
                        {
                            // save the newly selected planId to the front object only
                            DataRelay dataRelay = ( DataRelay )Session[ "DataRelay" ];
                            dataRelay.RestoreDelegatesAfterDeserialization();

                            if( dataRelay != null )
                            {
                                if( dataRelay.EditedDocumentContentFront != null )
                                {
                                    if( dataRelay.EditedDocumentContentFront.SBAPlanId != planDetails.SBAPlanId )
                                        Session[ "SelectedPlanWasModified" ] = true;
                                    dataRelay.EditedDocumentContentFront.SBAPlanId = planDetails.SBAPlanId;
                                }
                            }
                        }
                    }
                    else
                    {
                        MsgBox.Alert( contractDB.ErrorMessage );
                    }
                }
            }
            else
            {
                ShowValidationErrors();
            }

            return ( bSuccess );
        }
        
        protected bool ValidateBeforeSave( PlanDetails planDetails, ref string validationMessage, string validationGroupName )
        {
            bool bIsValid = true;

            // no validation if 'no plan' plan is selected
            if( planDetails.SBAPlanId != CMGlobals.NOPLANID ) 
            {
                if( _editSBAPlanDetailsWindowParms.EditSBAPlanAction == EditSBAPlanDetailsWindowParms.EditSBAPlanActions.SelectDifferentPlan )
                {
                    if( planDetails.SBAPlanId == -1 )  
                    {
                        bIsValid = false;
                        AppendValidationError( "Plan selection is required." );
                    }
                }

                if( planDetails.PlanName.Trim().Length == 0 )
                {
                    bIsValid = false;
                    AppendValidationError( "Plan name is required." );
                }

                if( planDetails.PlanTypeId != 1 && planDetails.PlanTypeId != 2 && planDetails.PlanTypeId != 3 )
                {
                    bIsValid = false;
                    AppendValidationError( "Plan type is required." );
                }

                if( planDetails.AdministratorName.Trim().Length == 0 )
                {
                    bIsValid = false;
                    AppendValidationError( "Plan administrator name is required." );
                }

                if( planDetails.AdministratorEmail.Trim().Length == 0 )
                {
                    bIsValid = false;
                    AppendValidationError( "Plan administrator email is required." );
                }
                else if( CMGlobals.IsValidEmailAddress( planDetails.AdministratorEmail.Trim() ) != true )
                {
                    bIsValid = false;
                    AppendValidationError( "Plan administrator email is not a correctly formatted email address." );
                }

                if( planDetails.AdministratorPhone.Trim().Length == 0 )
                {
                    bIsValid = false;
                    AppendValidationError( "Plan administrator phone is required." );
                }

                
                if( planDetails.AdministratorCountryId == -1 )
                {
                    bIsValid = false;
                    AppendValidationError( "Plan administrator country is required." );
                }

                if( planDetails.AdministratorCountryId == CMGlobals.COUNTRYIDUSA || planDetails.AdministratorCountryId == CMGlobals.COUNTRYIDCANADA )
                {
                    if( planDetails.AdministratorState.CompareTo( "--" ) == 0 )
                    {
                        bIsValid = false;
                        AppendValidationError( "Plan administrator state is required." );
                    }

                    if( planDetails.AdministratorZip.Trim().Length == 0 )
                    {
                        bIsValid = false;
                        AppendValidationError( "Plan administrator zip is required." );
                    }
                }

                // zip 5 or 10
                if( planDetails.AdministratorCountryId == CMGlobals.COUNTRYIDUSA )
                {
                    if( planDetails.AdministratorZip.Trim().Length > 0 )
                    {
                        if( planDetails.AdministratorZip.Trim().Length == 5 )
                        {
                            int tempZip = 0;
                            if( int.TryParse( planDetails.AdministratorZip.Trim(), out tempZip ) == false )
                            {
                                bIsValid = false;
                                AppendValidationError( "Plan administrator zip is not formatted correctly." );
                            }
                        }
                        else if( planDetails.AdministratorZip.Trim().Length == 10 )
                        {
                            string leftZip = "";
                            string dash = "";
                            string rightZip = "";

                            leftZip = planDetails.AdministratorZip.Trim().Substring( 0, 5 );
                            dash = planDetails.AdministratorZip.Trim().Substring( 5, 1 );
                            rightZip = planDetails.AdministratorZip.Trim().Substring( 6, 4 );

                            int tempZip = 0;
                            if( dash.CompareTo( "-" ) != 0 || int.TryParse( leftZip, out tempZip ) == false || int.TryParse( rightZip, out tempZip ) == false )
                            {
                                bIsValid = false;
                                AppendValidationError( "Plan administrator zip is not formatted correctly." );
                            }
                        }
                        else
                        {
                            bIsValid = false;
                            AppendValidationError( "Plan administrator zip is not formatted correctly." );
                        }
                    }
                }

            }

            return ( bIsValid );
        }

        public void AppendValidationError( string message )
        {
            if( _validationMessageStringBuilder == null )
            {
                _validationMessageStringBuilder = new StringBuilder( 400 );
                _validationMessageStringBuilder.AppendFormat( "Please correct the following: \n\n" );
            }

            _validationMessageStringBuilder.AppendFormat( "{0} \n", message );
        }

        public void ShowValidationErrors()
        {
            if( _validationMessageStringBuilder != null )
                MsgBox.Alert( _validationMessageStringBuilder.ToString() );
        }

        protected void EditSBAPlanDetailsCancelButton_OnClick( object sender, EventArgs e )
        {
            CloseWindow( false, false );  
        }

        protected void ActivePlansDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )      // $$$ must capture the fact the "--select--" was selected
        {
            DropDownList activePlansDropDownList = ( DropDownList )sender;
            ListItem selectedItem = activePlansDropDownList.SelectedItem;
            if( selectedItem != null )
            {
                int selectedSBAPlanId = int.Parse( selectedItem.Value );

                Session[ "SelectedSBAPlanId" ] = selectedSBAPlanId;
                Session[ "SelectedPlanWasModified" ] = true;

                selectedSBAPlanId = int.Parse( Session[ "SelectedSBAPlanId" ].ToString() );
                LoadScreen( _editSBAPlanDetailsWindowParms.ContractNumber, selectedSBAPlanId, false, false );   // false = do not reload this list while calling from a list related event added 9/9/2016 (dev) to potentially fix an exception in log
            }

            // fixes tab order problem
            //if( EditSBAPlanDetailsScriptManager != null )
            //    EditSBAPlanDetailsScriptManager.SetFocus( activePlansDropDownList );  
        }

        // fixes tab order problem
        protected void PlanTypeDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
        //    DropDownList planTypeDropDownList = ( DropDownList )sender;

        //    if( EditSBAPlanDetailsScriptManager != null )
        //        EditSBAPlanDetailsScriptManager.SetFocus( planTypeDropDownList );  
        }

        // fixes tab order problem
        protected void StateDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            //VA.NAC.NACCMBrowser.BrowserObj.DropDownList stateDropDownList = ( VA.NAC.NACCMBrowser.BrowserObj.DropDownList )sender;

            //if( EditSBAPlanDetailsScriptManager != null )
            //    EditSBAPlanDetailsScriptManager.SetFocus( stateDropDownList );  
        }

        protected void AdministratorCountryDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList administratorCountryDropDownList = ( DropDownList )sender;
            ListItem selectedItem = administratorCountryDropDownList.SelectedItem;
            int selectedAdministratorCountryId = int.Parse( selectedItem.Value );

            //PlanDetails planDetails = ( PlanDetails )Session[ "PlanDetails" ];
            //planDetails.AdministratorCountryId = selectedAdministratorCountryId;

            LoadAndBindStates( selectedAdministratorCountryId, "--" );
        }


        // ScriptManager required only for tab order problem fix
        protected void EditSBAPlanDetailsScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            String errorMsg = "";

            if( e.Exception.Data[ "EditSBAPlanDetailsErrorMessage" ] != null )
            {
                errorMsg = String.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "EditSBAPlanDetailsErrorMessage" ] );
            }
            else
            {
                errorMsg = String.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            EditSBAPlanDetailsScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }
    }
}