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
using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ContractSBAPlan : BaseDocumentEditorPage
    {

        public ContractSBAPlan()
            : base( DocumentEditorTypes.Contract )
        {
        }

      //  private const int ProjectionViewPercentagesButtonFieldNumber = 0;   // $$$+
        private const int ProjectionEditButtonFieldNumber = 0;  // $$$+
        private const int ProjectionStartDateFieldNumber = 1; //$$$+
        private const int ProjectionEndDateFieldNumber = 2; //$$$+
        private const int ProjectionTotalSubcontractingDollarsFieldNumber = 3; //$$$+
        private const int ProjectionSBDollarsFieldNumber = 4; //$$$+
        private const int ProjectionVeteranOwnedDollarsFieldNumber = 5; //$$$+
        private const int ProjectionDisabledVeteranDollarsFieldNumber = 6; //$$$+
        private const int ProjectionSDBDollarsFieldNumber = 7; //$$$+
        private const int ProjectionWomanOwnedDollarsFieldNumber = 8; //$$$+
        private const int ProjectionHubZoneDollarsFieldNumber = 9; //$$$+
      //  private const int ProjectionHBCUDollarsFieldNumber = 11; //$$$+
        private const int ProjectionCommentsFieldNumber = 10; // 12; //$$$+
  
        private const   int ProjectionRemoveButtonFieldNumber = 11;  // $$$+
        private const   int ProjectionIdFieldNumber = 12;  // $$$+

        private const int AssociatedContractsSelectAssociatedContractButtonFieldNumber = 0;
        private const int AssociatedContractsContractIdFieldNumber = 1;
        private const int AssociatedContractsIsResponsibleFieldNumber = 2;

        private const int PROJECTIONGRIDVIEWROWHEIGHTESTIMATE = 48;
        private const int ASSOCIATEDCONTRACTGRIDVIEWROWHEIGHTESTIMATE = 48;

        private bool _bBlockProjectionGridBindingForGridControlPostback = false;
        private bool _bBlockAssociatedContractsGridBindingForGridControlPostback = false;

        // this fixes issues with gridview edit save or cancel not redrawing out of edit mode
        protected new void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( Page.IsPostBack != true )
            {
                BindProjectionGridView();
                BindAssociatedContractsGridView();
            }
        }

        protected new void Page_Load( object sender, EventArgs e )
        {
            //if( Session[ "NACCMStartedProperly" ] == null || Request.UrlReferrer == null )
            //{
            //    Response.Redirect( "~/Start.aspx" );
            //}
            
            if( Page.IsPostBack == false && ( ( NACCM )Page.Master.Master ).IsRedirected() == false )
            {
                ClearSessionVariables(); 
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );

            CheckEventTarget();

            LoadNonFormViewControls( false );
            LoadSBAPlanDetails( false );

            if( Page.IsPostBack == false )
            {
                SetProjectionGridViewSelectedItem( 0, true );
                BindProjectionGridView();
                SetAssociatedContractsGridViewSelectedItem( 0, true );
                BindAssociatedContractsGridView();
            }
            else
            {
                RestoreProjectionGridViewSelectedItem();
                RestoreAssociatedContractsGridViewSelectedItem();
            }      

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
                string refreshSBADetailsOnSubmit = "";
                string sbaIdHasChanged = "";
                HtmlInputHidden refreshSBADetailsOnSubmitHiddenField = ( HtmlInputHidden )ContractFindControl( "RefreshSBADetailsOnSubmit" );
                HtmlInputHidden changeSelectedSBAIdOnSubmitHiddenField = ( HtmlInputHidden )ContractFindControl( "ChangeSelectedSBAIdOnSubmit" );
                
                if( refreshSBADetailsOnSubmitHiddenField != null )
                {
                    refreshSBADetailsOnSubmit = refreshSBADetailsOnSubmitHiddenField.Value;

                    if( changeSelectedSBAIdOnSubmitHiddenField != null )
                    {
                        sbaIdHasChanged = changeSelectedSBAIdOnSubmitHiddenField.Value;

                        if( refreshSBADetailsOnSubmit.Contains( "true" ) == true )
                        {
                            // for restore of personalized grid on popup close
                            ( ( NACCM )this.Master.Master ).SetPostbackFromPopupClose( true );

                            // if the selected plan has changed
                            if( sbaIdHasChanged.Contains( "true" ) == true )        // $$$ handle case where "--select--" was selected to clear the current plan
                            {
                                LoadNonFormViewControls( true );
                                LoadSBAPlanDetails( true );

                                SetProjectionGridViewSelectedItem( 0, true );
                                BindProjectionGridView();

                                SetAssociatedContractsGridViewSelectedItem( 0, true );
                                BindAssociatedContractsGridView();

                                BindFormViews();   
                            }
                            else // plan details were edited
                            {
                                LoadSBAPlanDetails( true );
                                BindFormViews();
                            }

                            refreshSBADetailsOnSubmitHiddenField.Value = "false";
                            changeSelectedSBAIdOnSubmitHiddenField.Value = "false";
                        }
                    }
                }
            }  
        }

        // called from update success event if the screen is visible
        public void ReloadAfterSave()
        {                               
            if( Session[ "SelectedPlanWasModified" ] != null )
            {                            
                if( (( bool )Session[ "SelectedPlanWasModified" ]) == true )
                {
                    LoadNonFormViewControls( true );
                    LoadSBAPlanDetails( true );

                    SetProjectionGridViewSelectedItem( 0, true );
                    BindProjectionGridView();

                    SetAssociatedContractsGridViewSelectedItem( 0, true );
                    BindAssociatedContractsGridView();

                    BindFormViews();

                    Session[ "SelectedPlanWasModified" ] = false;
                }
            }
        }

        private void CheckEventTarget()
        {
            _bBlockProjectionGridBindingForGridControlPostback = false;

            string controlName = Request.Form[ "__EVENTTARGET" ];

            if( controlName != null )
            {
                // set up to block binding if postback is from grid control
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$ProjectionGridView$ctl02$ )
                if( controlName.Contains( "startDateTextBox" ) == true ||
                    controlName.Contains( "endDateTextBox" ) == true ||
                    controlName.Contains( "totalSubContractingDollarsTextBox" ) == true ||
                    controlName.Contains( "sbDollarsTextBox" ) == true ||
                    controlName.Contains( "veteranOwnedDollarsTextBox" ) == true ||
                    controlName.Contains( "disabledVetDollarsTextBox" ) == true ||
                    controlName.Contains( "sdbDollarsTextBox" ) == true ||
                    controlName.Contains( "womenOwnedDollarsTextBox" ) == true ||
                    controlName.Contains( "hbcuDollarsTextBox" ) == true ||
                    controlName.Contains( "projectionCommentsTextBox" ) == true ||
                    controlName.Contains( "RemoveProjectionButton" ) == true )
                {
                    _bBlockProjectionGridBindingForGridControlPostback = true;
                }
                else if( controlName.Contains( "SelectAssociatedContractButton" ) == true )
                {
                    _bBlockAssociatedContractsGridBindingForGridControlPostback = true;
                }
            }
        }

        public bool BlockGridBindingForProjectionGridControlPostback
        {
            get
            { 
                return _bBlockProjectionGridBindingForGridControlPostback; 
            }
            set 
            { 
                _bBlockProjectionGridBindingForGridControlPostback = value; 
            }
        }

        public bool BlockGridBindingForAssociatedContractsGridControlPostback
        {
            get
            {
                return _bBlockAssociatedContractsGridBindingForGridControlPostback;
            }
            set
            {
                _bBlockAssociatedContractsGridBindingForGridControlPostback = value;
            }
        }

        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            SBAHeaderFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            SBAHeaderFormView.DataKeyNames = new string[] { "ContractId" };

            SBAPlanDetailsFormView.DataSource = SBAPlanDetailsDataSource;
            SBAPlanDetailsFormView.DataKeyNames = new string[] { "SBAPlanId" };
        }

        
        protected void ClearSessionVariables()
        {
            Session[ "ProjectionGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedProjectionId" ] = null;
            Session[ "LastInsertedProjectionId" ] = null;
            Session[ "AssociatedContractsGridViewSelectedIndex" ] = null;
            Session[ "SelectedPlanWasModified" ] = null;
            Session[ "LastAssociatedSBAPlanId" ] = null;
  //          Session[ "ChartWindowParms" ] = null;
        }

        public override void BindAfterShortSave()
        {
            BindFormViews();
        }

        public override void RebindHeader()
        {
            BindHeader();
        }

        protected void BindFormViews()
        {
            try
            {
                BindHeader();

                SBAHeaderFormView.DataBind();
                SBAPlanDetailsFormView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
 
            // note form view controls are not yet created here
        }


        protected void LoadNonFormViewControls( bool bSBAIdHasChanged )
        {
            LoadContractProjections( bSBAIdHasChanged );
            LoadContractSBAAssociatedContracts( bSBAIdHasChanged );

            ProjectionGridView.DataSource = ProjectionDataSource;
            AssociatedContractsGridView.DataSource = SBAAssociatedContractsDataSource;
        }


        protected void BindProjectionGridView()
        {
            try
            {
                // bind if postback not due to grid edit
                if( BlockGridBindingForProjectionGridControlPostback == false )
                    ProjectionGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        // not used
        //protected void SBAHeaderFormView_OnChange( object sender, EventArgs e )
        //{
        //    SetDirtyFlag( "SBAHeaderFormView" );
        //}

        // form is read-only
        //protected void SBAPlanDetailsFormView_OnChange( object sender, EventArgs e )
        //{
        //    SetDirtyFlag( "SBAPlanDetailsFormView" );
        //}

        public override string GetValidationGroupName()
        {
            return ( "ContractSBAPlan" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            // no fields to validate outside of grid

            return ( bSuccess );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            CheckBox SBAPlanExemptCheckBox = ( CheckBox )SBAHeaderFormView.FindControl( "SBAPlanExemptCheckBox" );

            if( SBAPlanExemptCheckBox != null )
            {
                if( SBAPlanExemptCheckBox.Checked == true )
                {
                    dataRelay.EditedDocumentContentFront.SBAPlanExempt = true;
                }
                else
                {
                    dataRelay.EditedDocumentContentFront.SBAPlanExempt = false;
                }
            }

            return ( bSuccess );
        }

        protected void SBAHeaderFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "SBAHeaderFormView" );
            bool bEnabled = documentControlPresentation.IsFormViewEnabled( "SBAHeaderFormView" );
            SBAHeaderFormView.Visible = bVisible;
            SBAHeaderFormView.Enabled = bEnabled;

            if( bVisible == true && bEnabled == true )
            {
                if( IsProjectionGridViewGoingIntoEditMode() == false )
                {
                    EnableDisableSBAActions( true );
                    EnableDisableExemptCheckBox( true );
                }
            }   
            else
            {
                EnableDisableSBAActions( false );              
                EnableDisableExemptCheckBox( false );
            }
        }

        protected void SBAPlanDetailsFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "SBAPlanDetailsFormView" );

            SBAPlanDetailsFormView.Visible = bVisible;
            SBAPlanDetailsFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "SBAPlanDetailsFormView" );

        }


        protected void AddNewSBAPlanButton_OnClick( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            EditSBAPlanDetailsWindowParms editSBAPlanDetailsWindowParms = new EditSBAPlanDetailsWindowParms( currentDocument.ContractNumber );

            Session[ "EditSBAPlanDetailsWindowParms" ] = editSBAPlanDetailsWindowParms;

            string windowOpenScript = "";

            windowOpenScript = "window.open('EditSBAPlanDetails.aspx','EditSBAPlanDetails','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=120,left=120,width=460,height=322');";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "EditSBAPlanDetailsWindowOpenScript", windowOpenScript, true );

            // allow the postback to occur 
            SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );  
        }

        protected void SelectDifferentSBAPlanButton_OnClick( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
            int currentSBAPlanId = DataRelay.EditedDocumentDataSourceFront.EditedDocumentContentFront.SBAPlanId;

            EditSBAPlanDetailsWindowParms editSBAPlanDetailsWindowParms = new EditSBAPlanDetailsWindowParms( currentSBAPlanId, currentDocument.ContractNumber, false, ( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Expired ) ? true : false ); 

            Session[ "EditSBAPlanDetailsWindowParms" ] = editSBAPlanDetailsWindowParms;

            string windowOpenScript = "";

            windowOpenScript = "window.open('EditSBAPlanDetails.aspx','EditSBAPlanDetails','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=120,left=120,width=460,height=322');";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "EditSBAPlanDetailsWindowOpenScript", windowOpenScript, true );

            // allow the postback to occur 
            SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );            
        }

      
        protected void EditExistingSBAPlanButton_OnClick( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
            int currentSBAPlanId = DataRelay.EditedDocumentDataSourceFront.EditedDocumentContentFront.SBAPlanId;

            EditSBAPlanDetailsWindowParms editSBAPlanDetailsWindowParms = new EditSBAPlanDetailsWindowParms( currentSBAPlanId, currentDocument.ContractNumber, true, ( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Expired ) ? true : false ); 

            Session[ "EditSBAPlanDetailsWindowParms" ] = editSBAPlanDetailsWindowParms;

            string windowOpenScript = "";

            windowOpenScript = "window.open('EditSBAPlanDetails.aspx','EditSBAPlanDetails','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=120,left=120,width=460,height=322');";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "EditSBAPlanDetailsWindowOpenScript", windowOpenScript, true );

            // allow the postback to occur 
            SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );            
        }

        protected void SBAPlanExemptCheckBox_OnCheckedChanged( object obj, EventArgs e )
        {
            CheckBox SBAPlanExemptCheckBox = ( CheckBox )obj;
            bool bSBAPlanExempt = SBAPlanExemptCheckBox.Checked;

            DataRelay.EditedDocumentDataSourceFront.EditedDocumentContentFront.SBAPlanExempt = bSBAPlanExempt;

            if( bSBAPlanExempt == true )
            {
                EnableDisableSBAActions( false );
            }
            else
            {
                EnableDisableSBAActions( true );
            }
            
            if( bSBAPlanExempt == true )
            {
                // save the sba plan information, in case the user changes their minds during this session
                Session[ "LastAssociatedSBAPlanId" ] = DataRelay.EditedDocumentDataSourceFront.EditedDocumentContentFront.SBAPlanId;

                DataRelay.EditedDocumentDataSourceFront.EditedDocumentContentFront.SBAPlanId = -1; // none

                LoadNonFormViewControls( true );
                LoadSBAPlanDetails( true );

                LoadContractSBAAssociatedContracts( true );

                SetProjectionGridViewSelectedItem( 0, true );
                BindProjectionGridView();  // rebind grid to clear selected sba plan

                BindFormViews();
            }
            else
            {
                // see if there was an associated plan when this session started
                // if so, then restore it
                if( Session[ "LastAssociatedSBAPlanId" ] != null )
                {
                    int lastAssociatedSBAPlanId = ( int )Session[ "LastAssociatedSBAPlanId" ];
                    Session[ "LastAssociatedSBAPlanId" ] = null; // reset

                    DataRelay.EditedDocumentDataSourceFront.EditedDocumentContentFront.SBAPlanId = lastAssociatedSBAPlanId;

                    LoadNonFormViewControls( true );
                    LoadSBAPlanDetails( true );

                    LoadContractSBAAssociatedContracts( true );

                    SetProjectionGridViewSelectedItem( 0, true );
                    BindProjectionGridView();  // rebind grid to clear selected sba plan

                    BindFormViews();
                }
            }        
        }

        protected void EnableDisableExemptCheckBox( bool bEnableExempt )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
            CheckBox SBAPlanExemptCheckBox = ( CheckBox )SBAHeaderFormView.FindControl( "SBAPlanExemptCheckBox" );

            if( currentDocument != null && SBAPlanExemptCheckBox != null )
            {
               if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.SBA ) == true &&
                    currentDocument.EditStatus == CurrentDocument.EditStatuses.CanEdit ) 
                {
                    SBAPlanExemptCheckBox.Enabled = bEnableExempt;
                }
                else
                {
                    SBAPlanExemptCheckBox.Enabled = false;
                }
            }
        }

        protected void EnableDisableSBAActions( bool bEnableActions )
        {
            bool bSBAPlanExempt = false;

            CheckBox SBAPlanExemptCheckBox = ( CheckBox )SBAHeaderFormView.FindControl( "SBAPlanExemptCheckBox" );
            if( SBAPlanExemptCheckBox != null )
                bSBAPlanExempt = SBAPlanExemptCheckBox.Checked;

            Button addSBAPlanButton = ( Button )SBAHeaderFormView.FindControl( "AddSBAPlanButton" );
            Button selectDifferentSBAPlanButton = ( Button )SBAHeaderFormView.FindControl( "SelectDifferentSBAPlanButton" );
            Button editExistingSBAPlanButton = ( Button )SBAHeaderFormView.FindControl( "EditExistingSBAPlanButton" );
            Button addSBAProjectionButton = ( Button )SBAHeaderFormView.FindControl( "AddSBAProjectionButton" );

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument != null )
            {
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.SBA ) == true &&
                    currentDocument.EditStatus == CurrentDocument.EditStatuses.CanEdit && bSBAPlanExempt == false )
                {
                    if( addSBAPlanButton != null )
                    {
                        addSBAPlanButton.Enabled = bEnableActions;
                    }

                    if( selectDifferentSBAPlanButton != null )
                    {
                        selectDifferentSBAPlanButton.Enabled = bEnableActions;
                    }

                    // some actions require a current plan
                    if( DataRelay.EditedDocumentContentFront.SBAPlanId != -1 )
                    {
                        if( editExistingSBAPlanButton != null )
                        {
                            editExistingSBAPlanButton.Enabled = bEnableActions;
                        }
                        else
                        {
                            editExistingSBAPlanButton.Enabled = false;
                        }
                    }

                    if( addSBAProjectionButton != null )
                    {
                        // enable only if a plan exists and the plan is saved
                        EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];                      

                        if( editedDocumentContentFront != null )
                        {
                            if( editedDocumentContentFront.SBAPlanId != -1 && editedDocumentContentFront.SBAPlanId != CMGlobals.NOPLANID )
                            {
                                if( editedDocumentContentFront.DirtyFlags.IsSBAHeaderDirty == false )
                                {
                                    addSBAProjectionButton.Enabled = bEnableActions;
                                }
                                else
                                {
                                    addSBAProjectionButton.Enabled = false;
                                }
                            }
                            else
                            {
                                addSBAProjectionButton.Enabled = false;
                            }
                        }
                    }
                }
                else
                {
                    if( addSBAPlanButton != null )
                    {
                        addSBAPlanButton.Enabled = false;
                    }

                    if( selectDifferentSBAPlanButton != null )
                    {
                        selectDifferentSBAPlanButton.Enabled = false;
                    }
               
                    if( editExistingSBAPlanButton != null )
                    {
                        editExistingSBAPlanButton.Enabled = false;
                    }

                    if( addSBAProjectionButton != null )
                    {
                        addSBAProjectionButton.Enabled = false;
                    }   
                }
            }
        }
       
        protected string FormatEmailAddress( object emailAddressObj )
        {
            string emailAddress = "";
            if( emailAddressObj != null )
            {
                emailAddress = emailAddressObj.ToString();
            }

            if( emailAddress.Length >= 19 )
            {
                return ( emailAddress ?? "" ).Replace( "@", "@<br/>" );
            }
            else
            {
                return ( emailAddress );
            }
        }

#region SBAProjections

        protected void ProjectionGridPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "ProjectionGridPanel" );

            ProjectionGridPanel.Visible = bVisible;
            ProjectionGridPanel.Enabled = documentControlPresentation.IsControlEnabled( "ProjectionGridPanel" );

            if( bVisible == true )
            {
                EnableProjectionDateEditing( ( Panel )sender );
         //       EnableProjectionChart( ( Panel )sender );
            }
        }

        //private void OpenChartWindow( int itemIndex, DateTime startDate, DateTime endDate, double totalSubConDollars = 0, double sbDollars = 0, double veteranOwnedDollars = 0, double disabledVetDollars = 0, double SDBDollars = 0, double womenOwnedDollars = 0, double hubZoneDollars = 0, double HBCUDollars = 0 )
        //{
        //    Session[ "ChartWindowParms" ] = null;
        //    Session[ "ChartWindowParms" ] = new ChartWindowParms( itemIndex, ChartWindowParms.ChartNames.SBAPercentages, ChartWindowParms.SupportedChartTypes.PieChart, totalSubConDollars, sbDollars, veteranOwnedDollars, disabledVetDollars, SDBDollars, womenOwnedDollars, hubZoneDollars, HBCUDollars, startDate, endDate );

        //    string windowOpenScript = "window.open('Chart.aspx','Chart','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=500,left=1300,width=400,height=500, resizable=0');";
        //    ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "ChartWindowOpenScript", windowOpenScript, true );
        //}

        protected void ProjectionGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedProjectionId = -1;
            int itemIndex = -1;
            double totalSubConDollars = 0;
            double sbDollars = 0;
            double veteranOwnedDollars = 0;
            double disabledVetDollars = 0;
            double SDBDollars = 0;
            double womenOwnedDollars = 0;
            double hubZoneDollars = 0;
            double HBCUDollars = 0;
            DateTime startDate;
            DateTime endDate;

            if( e.CommandName.CompareTo( "ViewPercentages" ) == 0 )
            {

                //string commandArgument = e.CommandArgument.ToString();
                //string[] argumentList = commandArgument.Split( new char[] { ',' } );

                //itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                //selectedProjectionId = int.Parse( argumentList[ 1 ].ToString() );

                //if( double.TryParse( argumentList[ 2 ].ToString(), out totalSubConDollars ) == false )
                //    totalSubConDollars = 0;
                //if( double.TryParse( argumentList[ 3 ].ToString(), out sbDollars ) == false )
                //    sbDollars = 0;
                //if( double.TryParse( argumentList[ 4 ].ToString(), out veteranOwnedDollars ) == false )
                //    veteranOwnedDollars = 0;
                //if( double.TryParse( argumentList[ 5 ].ToString(), out disabledVetDollars ) == false )
                //    disabledVetDollars = 0;
                //if( double.TryParse( argumentList[ 6 ].ToString(), out SDBDollars ) == false )
                //    SDBDollars = 0;
                //if( double.TryParse( argumentList[ 7 ].ToString(), out womenOwnedDollars ) == false )
                //    womenOwnedDollars = 0;
                //if( double.TryParse( argumentList[ 8 ].ToString(), out hubZoneDollars ) == false )
                //    hubZoneDollars = 0;
                //if( double.TryParse( argumentList[ 9 ].ToString(), out HBCUDollars ) == false )
                //    HBCUDollars = 0;               
                
                //startDate = DateTime.Parse( argumentList[ 10 ].ToString() );
                //endDate = DateTime.Parse( argumentList[ 11 ].ToString() );

                //HighlightProjectionRow( itemIndex );

                //OpenChartWindow( itemIndex, startDate, endDate, totalSubConDollars, sbDollars, veteranOwnedDollars, disabledVetDollars, SDBDollars, womenOwnedDollars, hubZoneDollars, HBCUDollars );

                //// allow the postback to occur 
                //SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else
            {
                if( e.CommandName.CompareTo( "EditProjection" ) == 0 )
                {

                    string commandArgument = e.CommandArgument.ToString();
                    string[] argumentList = commandArgument.Split( new char[] { ',' } );

                    itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                    selectedProjectionId = int.Parse( argumentList[ 1 ].ToString() );

                    HighlightProjectionRow( itemIndex );

                    InitiateEditModeForProjection( itemIndex );

                    // allow the postback to occur 
                    SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                }
                else
                {
                    if( e.CommandName.CompareTo( "SaveProjection" ) == 0 )
                    {

                        string commandArgument = e.CommandArgument.ToString();
                        string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                        itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                        selectedProjectionId = int.Parse( argumentList[ 1 ].ToString() );

                        string validationMessage = "";

                        // validate the item before saving
                        bool bIsItemOk = ValidateProjectionBeforeUpdate( ProjectionGridView, itemIndex, selectedProjectionId, ref validationMessage );

                        if( bIsItemOk == true )
                        {
                            // is this an insert or an update
                            int newOrUpdatedRowIndex = -1;

                            if( ProjectionGridView.InsertRowActive == true )
                            {
                                newOrUpdatedRowIndex = InsertProjection( ProjectionGridView, itemIndex );
                            }
                            else
                            {
                                newOrUpdatedRowIndex = UpdateProjection( ProjectionGridView, itemIndex );
                            }

                            HighlightProjectionRow( newOrUpdatedRowIndex );

                            // allow the postback to occur 
                            SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                        }
                        else
                        {
                            ShowException( new Exception( validationMessage ) );
                        }
                    }
                    else
                    {                      
                        if( e.CommandName.CompareTo( "Cancel" ) == 0 )
                        {
                            string commandArgument = e.CommandArgument.ToString();
                            string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                            itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                            selectedProjectionId = int.Parse( argumentList[ 1 ].ToString() );

                            CancelEdit( itemIndex );
                        }
                        else
                        {
                            if( e.CommandName.CompareTo( "RemoveProjection" ) == 0 )
                            {
                                string commandArgument = e.CommandArgument.ToString();
                                string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                                itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                                selectedProjectionId = int.Parse( argumentList[ 1 ].ToString() );

                                bool bContinueWithDelete = false;

                                bContinueWithDelete = GetConfirmationMessageResults();

                                if( bContinueWithDelete == true )
                                {
                                    int newRowIndex = DeleteProjection( ProjectionGridView, itemIndex );

                                    HighlightProjectionRow( newRowIndex );

                                    // allow the postback to occur 
                                    SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                                }
                            }
                        }                           
                    }
                }
            }
        }

        protected void ProjectionGridView_RowDataBound( object sender,  GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;
                CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];

                bool bUnlimited = true;
                DateTime projectionMinDate = DateTime.Now.AddDays( -120 );
                DateTime projectionMaxDate = DateTime.Now.AddYears( 10 );

                if( e.Row.RowType == DataControlRowType.DataRow ) 
                { 

                    if( ((( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                        (( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert )) )
                    {                           
                        Button removeProjectionButton = null;
                        removeProjectionButton = ( Button )e.Row.FindControl( "RemoveProjectionButton" );

                        if( removeProjectionButton != null )
                        { 
                            removeProjectionButton.OnClientClick = "presentConfirmationMessage( 'Permanently delete the selected check from this contract?' );";
                        }

                        //Button viewPercentagesButton = null;
                        //viewPercentagesButton = ( Button )e.Row.FindControl( "ViewPercentagesButton" );

                        //if( viewPercentagesButton != null )
                        //{
                        //    viewPercentagesButton.OnClientClick = GetGridViewChartButtonScript();
                        //}

                        if( currentDocument != null )
                        { 
                            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.SBA ) != true &&
                                currentDocument.EditStatus != CurrentDocument.EditStatuses.CanEdit ) 
                            { 
                                if( removeProjectionButton != null )
                                { 
                                    removeProjectionButton.Enabled = false;
                                } 

                                Button editButton = null;
                                editButton = ( Button )e.Row.FindControl( "EditButton" );
                                if( editButton != null )  
                                { 
                                    editButton.Enabled = false;
                                } 

                                Button saveButton = null;
                                saveButton = ( Button )e.Row.FindControl( "SaveButton" );
                                if( saveButton != null )
                                { 
                                    saveButton.Enabled = false;
                                } 
                            } 
                        } 
                    } 
                    else 
                    {                     
                        // image buttons visible during edit
                        ImageButton startDateImageButton = null;
                        startDateImageButton = ( ImageButton )e.Row.FindControl( "startDateImageButton" );
                        TextBox startDateTextBox = null;
                        startDateTextBox = ( TextBox )e.Row.FindControl( "startDateTextBox" );


                        if( startDateImageButton != null )
                        {
                            startDateImageButton.OnClientClick = GetGridViewDateButtonScript( "ProjectionStart", startDateTextBox.UniqueID );
                        }

                        ImageButton endDateImageButton = null;
                        endDateImageButton = ( ImageButton )e.Row.FindControl( "endDateImageButton" );
                        TextBox endDateTextBox = null;
                        endDateTextBox = ( TextBox )e.Row.FindControl( "endDateTextBox" );

                        if( endDateImageButton != null )
                        {
                            endDateImageButton.OnClientClick = GetGridViewDateButtonScript( "ProjectionEnd", endDateTextBox.UniqueID );
                        }
                    } 
                }
                // set header text for dollar columns to percent, when in edit mode
                else if( e.Row.RowType == DataControlRowType.Header )
                {
                    if( ( ( ( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                      ( ( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert ) ) )
                    {
                        if( IsProjectionGridViewGoingIntoEditMode() == true )
                        {
                            e.Row.Cells[ ProjectionSBDollarsFieldNumber ].Text = "SB $";
                            e.Row.Cells[ ProjectionVeteranOwnedDollarsFieldNumber ].Text = "VO $";
                            e.Row.Cells[ ProjectionDisabledVeteranDollarsFieldNumber ].Text = "SDVO $";
                            e.Row.Cells[ ProjectionSDBDollarsFieldNumber ].Text = "SDB $";
                            e.Row.Cells[ ProjectionWomanOwnedDollarsFieldNumber ].Text = "WO $";
                            e.Row.Cells[ ProjectionHubZoneDollarsFieldNumber ].Text = "Hub $";
                        }
                        else
                        {
                            e.Row.Cells[ ProjectionSBDollarsFieldNumber ].Text = "SB %";
                            e.Row.Cells[ ProjectionVeteranOwnedDollarsFieldNumber ].Text = "VO %";
                            e.Row.Cells[ ProjectionDisabledVeteranDollarsFieldNumber ].Text = "SDVO %";
                            e.Row.Cells[ ProjectionSDBDollarsFieldNumber ].Text = "SDB %";
                            e.Row.Cells[ ProjectionWomanOwnedDollarsFieldNumber ].Text = "WO %";
                            e.Row.Cells[ ProjectionHubZoneDollarsFieldNumber ].Text = "Hub %";
                        }
                    }
                    else
                    {
                        e.Row.Cells[ ProjectionSBDollarsFieldNumber ].Text = "SB $";
                        e.Row.Cells[ ProjectionVeteranOwnedDollarsFieldNumber ].Text = "VO $";
                        e.Row.Cells[ ProjectionDisabledVeteranDollarsFieldNumber ].Text = "SDVO $";
                        e.Row.Cells[ ProjectionSDBDollarsFieldNumber ].Text = "SDB $";
                        e.Row.Cells[ ProjectionWomanOwnedDollarsFieldNumber ].Text = "WO $";
                        e.Row.Cells[ ProjectionHubZoneDollarsFieldNumber ].Text = "Hub $";
                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }
       
        // called from panel_onprerender
        protected void EnableProjectionDateEditing( Panel projectionGridPanel )
        {
            Page currentPage = ( Page )projectionGridPanel.Page;
            bool bUnlimited = true;
            DateTime projectionMinDate = DateTime.Now.AddDays( -120 );
            DateTime projectionMaxDate = DateTime.Now.AddYears( 10 );

            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "ProjectionStartDateButtonOnClickScript", GetDateButtonScript( "ProjectionStart", bUnlimited, projectionMinDate, projectionMaxDate ), true );
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "ProjectionEndDateButtonOnClickScript", GetDateButtonScript( "ProjectionEnd", bUnlimited, projectionMinDate, projectionMaxDate ), true );
        }

        //protected void EnableProjectionChart( Panel projectionGridPanel )
        //{
        //    Page currentPage = ( Page )projectionGridPanel.Page;
          
        //    ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "ProjectionChartButtonOnClickScript", GetChartButtonScript( "standard" ), true );
        //}

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

            string script = String.Format( "function On{0}DateButtonClick( receivingControlClientId ) {{ window.open('Calendar.aspx?InitialDate={1}&DateType={2}&Unlimited={3}&MinAllowedDate={4}&MaxAllowedDate={5}&ReceivingControlClientId=' + receivingControlClientId,'Calendar','toolbar=no,menubar=no,resizable=no,scrollbars=no,statusbar=0,location=0,width=250,height=340,left=660,top=300'); return false;}}", dateTypeString, defaultDateString, dateTypeString, ( ( bUnlimitedDateRange == true ) ? 1 : 0 ), minAllowedDateString, maxAllowedDateString );

            return ( script );
        }

        public string GetGridViewDateButtonScript( string dateTypeString, string receivingControlClientId )
        {
            string script = String.Format( "On{0}DateButtonClick( '{1}' );", dateTypeString, receivingControlClientId );
            return ( script );
        }

        //public string GetChartButtonScript( string chartTypeString )
        //{           
        //    string script = String.Format( "function OnChartButtonClick() {{ window.open('Chart.aspx?ChartType={0}','Chart','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=160,height=310'); return false;}}", chartTypeString );

        //    return ( script );
        //}

        //public string GetGridViewChartButtonScript( )
        //{
        //    string script = String.Format( "OnChartButtonClick()" );
        //    return ( script );
        //}

        protected void Save_ButtonClick( object sender, EventArgs e )
        {

        }

        protected void ProjectionGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {
        }

        protected void ProjectionGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForProjection( e.NewEditIndex );  // added this to match drug item grid handling 
        }

        protected void ProjectionGridView_RowInserting( object sender, GridViewInsertEventArgs e )
        {
        }

        protected void ProjectionGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            CancelEdit( e.RowIndex );
        }

        protected void CancelEdit( int rowIndex )
        {
            int cancelIndex = rowIndex;
            bool bInserting = ProjectionGridView.InsertRowActive;
            if( bInserting == true ) 
            { 
                ProjectionGridView.InsertRowActive = false; // cancels insert ( if inserting )

                AddingProjectionRecord = false; //_withAddProjectionParameter.DefaultValue = "false";
                ProjectionGridView.EditIndex = -1;
                BindProjectionGridView();

                // enable appropriate buttons for the selected row
                SetEnabledProjectionControlsDuringEdit( ProjectionGridView, cancelIndex, true );

                EnableDisableSBAActions( true );
                EnableDisableExemptCheckBox( true );

                HighlightProjectionRow( 0 );   // $$$ should this be cancelIndex instead of 0 like other cancel functions

                // allow the postback to occur 
                SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            } 
            else  // editing existing row
            {

                ProjectionGridView.EditIndex = -1; // cancels the edit
                BindProjectionGridView();

                // enable appropriate buttons for the selected row
                SetEnabledProjectionControlsDuringEdit( ProjectionGridView, cancelIndex, true );

                EnableDisableSBAActions( true );
                EnableDisableExemptCheckBox( true );

                HighlightProjectionRow( cancelIndex );

                // allow the postback to occur 
                SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            } 

        }

        protected void ProjectionGridView_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {
        }
        protected void ProjectionGridView_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {
        }

        protected void ProjectionGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id field
            if( e.Row.Cells.Count > ProjectionIdFieldNumber ) 
            { 
                e.Row.Cells[ ProjectionIdFieldNumber ].Visible = false;
            }
        }

        protected void ProjectionGridView_OnSelectedIndexChanged( object sender, EventArgs e )
        {
        }
        protected void ProjectionGridView_PreRender( object sender, EventArgs e )
        {
        }

        protected void ProjectionGridView_Init( object sender, EventArgs e )
        {
        }



        private bool GetConfirmationMessageResults()
        {
            bool bConfirmationResults = false;
            string confirmationResultsString = "";

            HtmlInputHidden confirmationMessageResultsHiddenField = ( HtmlInputHidden )ContractFindControl( "confirmationMessageResults" );
            if( confirmationMessageResultsHiddenField != null )
            { 
                confirmationResultsString = confirmationMessageResultsHiddenField.Value;
                if( confirmationResultsString.Contains( "true" ) == true )
                { 
                    bConfirmationResults = true;
                    confirmationMessageResultsHiddenField.Value = "false";
                } 
            } 

            return( bConfirmationResults );
        }

        protected void AddNewSBAProjectionButton_OnClick( object sender, EventArgs e )
        {
            ProjectionGridView.Insert();

            AddingProjectionRecord = true; // _withAddProjectionParameter.DefaultValue = "true";

            BindProjectionGridView();

            InitiateEditModeForProjection( 0 );

            SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() ); 
        }

        protected void InitiateEditModeForProjection( int editIndex )
        {
            ProjectionGridView.EditIndex = editIndex;

            // select the edited item also
            if( ProjectionGridView.InsertRowActive == true ) 
            { 
                SetProjectionGridViewSelectedItem( editIndex, true ); // scroll to new row
            } 
            else 
            {
                SetProjectionGridViewSelectedItem( editIndex, false );
            }

            ProjectionGridView.DataBind(); 

            // disable appropriate buttons for the selected row
            SetEnabledProjectionControlsDuringEdit( ProjectionGridView, editIndex, false );

            // disable the non-edit controls before going into edit mode
            EnableDisableSBAActions( false );
            EnableDisableExemptCheckBox( false );

            // allow the postback to occur 
            SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected bool IsProjectionGridViewGoingIntoEditMode()
        {
            bool bGoingIntoEditMode = false;
            if( ProjectionGridView.EditIndex >= 0 || ProjectionGridView.InsertRowActive == true )
            {
                bGoingIntoEditMode = true;
            }
            return ( bGoingIntoEditMode );
        }

        protected void SetProjectionGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {

            if( selectedItemIndex < ProjectionGridView.Rows.Count ) 
            { 

                // save for postback
                Session[ "ProjectionGridViewSelectedIndex" ] = selectedItemIndex;

                // set the row selected
                ProjectionGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true ) 
                { 
                    ScrollToSelectedProjectionItem();
                }

                SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            } 
        }

        protected void ScrollToSelectedProjectionItem()
        {

            int rowIndex = ProjectionGridView.SelectedIndex;   // took out + 1 here to make header visible on add

            int fudge = ( int )Math.Floor( ( double )rowIndex / ( double )100.0 );
            int rowPosition = ( PROJECTIONGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( PROJECTIONGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string restoreProjectionGridSelectionScript = String.Format( "RestoreProjectionGridSelection( {0},{1} );", rowPosition, rowIndex );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestoreProjectionGridSelectionScript", restoreProjectionGridSelectionScript, true ); // runs after controls established
        }

        protected void HighlightProjectionRow( int itemIndex )
        { 
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1; 

            if( ProjectionGridView.HasData() == true ) 
            {
                GridViewRow row = ProjectionGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate ) 
                {
                    highlightedRowOriginalColor = ProjectionGridView.AlternatingRowStyle.BackColor.ToString();
                } 
                else 
                {
                    highlightedRowOriginalColor = ProjectionGridView.RowStyle.BackColor.ToString();
                } 
                
                string preserveHighlightingScript = String.Format( "setProjectionHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveProjectionHighlightingScript", preserveHighlightingScript, true );
            } 
        }

        protected void RestoreProjectionGridViewSelectedItem()
        {
            if( Session[ "ProjectionGridViewSelectedIndex" ] == null )
                return;
            
            ProjectionGridView.SelectedIndex = int.Parse( Session[ "ProjectionGridViewSelectedIndex" ].ToString() );

            //ScrollToSelectedItem();

            //SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void SetEnabledProjectionControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )   
        {
        //    gv.SetEnabledControlsForCell( rowIndex, ProjectionViewPercentagesButtonFieldNumber, bEnabled ); // view percentages button
            gv.SetEnabledControlsForCell( rowIndex, ProjectionRemoveButtonFieldNumber, bEnabled ); // remove button

            gv.SetVisibleControlsForCell( rowIndex, ProjectionEditButtonFieldNumber, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, ProjectionEditButtonFieldNumber, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, ProjectionEditButtonFieldNumber, "CancelButton", !bEnabled );
        }

    
        private bool ValidateProjectionBeforeUpdate( GridView projectionGridView, int itemIndex, int selectedProjectionId, ref string validationMessage )
        {
            bool bIsValid = true;
            validationMessage = "";

            DateTime startDate;
            DateTime endDate;
            Decimal dollarAmount = 0;

            string startDateString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionStartDateFieldNumber, 0, false, "startDateTextBox" );
            string endDateString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionEndDateFieldNumber, 0, false, "endDateTextBox" );
            string totalSubcontractingDollarsString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionTotalSubcontractingDollarsFieldNumber, 0, false, "totalSubContractingDollarsTextBox" );
            string sbDollarsString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionSBDollarsFieldNumber, 0, false, "sbDollarsTextBox" );
            string veteranOwnedDollarsString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionVeteranOwnedDollarsFieldNumber, 0, false, "veteranOwnedDollarsTextBox" );
            string disabledVetDollarsString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionDisabledVeteranDollarsFieldNumber, 0, false, "disabledVetDollarsTextBox" );
            string sdbDollarsString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionSDBDollarsFieldNumber, 0, false, "sdbDollarsTextBox" );
            string womenOwnedDollarsString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionWomanOwnedDollarsFieldNumber, 0, false, "womenOwnedDollarsTextBox" );
            string hubZoneDollarsString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionHubZoneDollarsFieldNumber, 0, false, "hubZoneDollarsTextBox" );
            string hbcuDollarsString = "0";
            string projectionCommentsString = projectionGridView.GetStringValueFromSelectedControl( itemIndex, ProjectionCommentsFieldNumber, 0, false, "projectionCommentsTextBox" );

            if( DateTime.TryParseExact( startDateString, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out startDate ) == false )
            {
                bIsValid = false;
                validationMessage = "A valid start date is required.";
            }
            else
            {
                try
                {
                    System.Data.SqlTypes.SqlDateTime sqlDateTimeObj = new System.Data.SqlTypes.SqlDateTime( startDate );
                }
                catch( System.Data.SqlTypes.SqlTypeException ex )
                {
                    bIsValid = false;
                    validationMessage = "A valid start date is required.";
                }
            }

            if( DateTime.TryParseExact( endDateString, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out endDate ) == false )
            {
                bIsValid = false;
                validationMessage = "A valid end date is required.";
            }
            else
            {
                try
                {
                    System.Data.SqlTypes.SqlDateTime sqlDateTimeObj = new System.Data.SqlTypes.SqlDateTime( endDate );
                }
                catch( System.Data.SqlTypes.SqlTypeException ex )
                {
                    bIsValid = false;
                    validationMessage = "A valid end date is required.";
                }
            }

            if( Decimal.TryParse( totalSubcontractingDollarsString, out dollarAmount ) == false )
            {
                bIsValid = false;
                validationMessage = "Total subcontracting dollars is not a valid number.";
            }
            else
            {
                if( dollarAmount == 0 )
                {
                    bIsValid = false;
                    validationMessage = "Total subcontracting dollars must be greater than zero.";
                }
            }

            if( Decimal.TryParse( sbDollarsString, out dollarAmount ) == false )
            {
                bIsValid = false;
                validationMessage = "SB dollars is not a valid number.";
            }

            if( Decimal.TryParse( veteranOwnedDollarsString, out dollarAmount ) == false )
            {
                bIsValid = false;
                validationMessage = "Veteran Owned dollars is not a valid number.";
            }

            if( Decimal.TryParse( disabledVetDollarsString, out dollarAmount ) == false )
            {
                bIsValid = false;
                validationMessage = "Disabled Veteran dollars is not a valid number.";
            }

            if( Decimal.TryParse( sdbDollarsString, out dollarAmount ) == false )
            {
                bIsValid = false;
                validationMessage = "SDB dollars is not a valid number.";
            }

            if( Decimal.TryParse( womenOwnedDollarsString, out dollarAmount ) == false )
            {
                bIsValid = false;
                validationMessage = "Women owned dollars is not a valid number.";
            }

            if( Decimal.TryParse( hubZoneDollarsString, out dollarAmount ) == false )
            {
                bIsValid = false;
                validationMessage = "Hub zone dollars is not a valid number.";
            }

            //if( Decimal.TryParse( hbcuDollarsString, out dollarAmount ) == false )
            //{
            //    bIsValid = false;
            //    validationMessage = "HBCU dollars is not a valid number.";
            //}

            return( bIsValid );
        }
    

        //protected void RemoveProjectionButton_DataBinding( object sender,  EventArgs e )
        //{
        //    Button removeProjectionButton  = ( Button )sender;
        //    if( removeProjectionButton != null )
        //    { 
        //        CMGlobals.MultilineButtonText( removeProjectionButton, new String[] { "Remove", "Projection" } );
        //    } 
        //}

      

#endregion SBAProjections


#region UpdateInsertDeleteProjection

        private int InsertProjection( GridView projectionGridView, int rowIndex ) 
        {
            int insertedRowIndex = 0;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs  = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            ProjectionContractNumberParameterValue = currentDocument.ContractNumber;
            UserLoginParameterValue = bs.UserInfo.LoginName;

            ProjectionStartDateParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionStartDateFieldNumber, 0, false, "startDateTextBox" );
            ProjectionEndDateParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionEndDateFieldNumber, 0, false, "endDateTextBox" );
            TotalSubContractingDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionTotalSubcontractingDollarsFieldNumber, 0, false, "totalSubContractingDollarsTextBox" );
            SBDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionSBDollarsFieldNumber, 0, false, "sbDollarsTextBox" );
            VeteranOwnedDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionVeteranOwnedDollarsFieldNumber, 0, false, "veteranOwnedDollarsTextBox" );
            DisabledVeteranDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionDisabledVeteranDollarsFieldNumber, 0, false, "disabledVetDollarsTextBox" );
            SDBDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionSDBDollarsFieldNumber, 0, false, "sdbDollarsTextBox" );
            WomenOwnedDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionWomanOwnedDollarsFieldNumber, 0, false, "womenOwnedDollarsTextBox" );
            HubZoneDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionHubZoneDollarsFieldNumber, 0, false, "hubZoneDollarsTextBox" );
            HBCUDollarsParameterValue = "0";
            ProjectionCommentsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionCommentsFieldNumber, 0, false, "projectionCommentsTextBox" );


            try
            {
                ProjectionDataSource.Insert();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }


            projectionGridView.InsertRowActive = false; // done with insert
            projectionGridView.EditIndex = -1; // done with edit
            AddingProjectionRecord = false; // _withAddProjectionParameter.DefaultValue = "false";   // no extra row
            projectionGridView.DataBind(); //  bind with new row

            if( Session[ "LastInsertedProjectionId" ] != null )
            { 
                int newProjectionId = ( int )Session[ "LastInsertedProjectionId" ];
                insertedRowIndex = projectionGridView.GetRowIndexFromId( newProjectionId, 0 );

                SetProjectionGridViewSelectedItem( insertedRowIndex, true );

                // bind to select
                projectionGridView.DataBind();
            } 

            // enable appropriate buttons for the selected row
            SetEnabledProjectionControlsDuringEdit( projectionGridView, insertedRowIndex, true );

            EnableDisableSBAActions( false );

            return( insertedRowIndex );
        }
    


        private int UpdateProjection( GridView projectionGridView, int rowIndex )
        {
            int updatedRowIndex = -1;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs  = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            ProjectionContractNumberParameterValue = currentDocument.ContractNumber;
            UserLoginParameterValue = bs.UserInfo.LoginName;

            ProjectionIdParameterValue = projectionGridView.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

            ProjectionStartDateParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionStartDateFieldNumber, 0, false, "startDateTextBox" );
            ProjectionEndDateParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionEndDateFieldNumber, 0, false, "endDateTextBox" );
            TotalSubContractingDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionTotalSubcontractingDollarsFieldNumber, 0, false, "totalSubContractingDollarsTextBox" );
            SBDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionSBDollarsFieldNumber, 0, false, "sbDollarsTextBox" );
            VeteranOwnedDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionVeteranOwnedDollarsFieldNumber, 0, false, "veteranOwnedDollarsTextBox" );
            DisabledVeteranDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionDisabledVeteranDollarsFieldNumber, 0, false, "disabledVetDollarsTextBox" );
            SDBDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionSDBDollarsFieldNumber, 0, false, "sdbDollarsTextBox" );
            WomenOwnedDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionWomanOwnedDollarsFieldNumber, 0, false, "womenOwnedDollarsTextBox" );
            HubZoneDollarsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionHubZoneDollarsFieldNumber, 0, false, "hubZoneDollarsTextBox" );
            HBCUDollarsParameterValue = "0";
            ProjectionCommentsParameterValue = projectionGridView.GetStringValueFromSelectedControl( rowIndex, ProjectionCommentsFieldNumber, 0, false, "projectionCommentsTextBox" );


            try
            {
                ProjectionDataSource.Update();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }

            projectionGridView.EditIndex = -1; // done with edit
            projectionGridView.DataBind();

            if( Session[ "LastUpdatedProjectionId" ] != null )
            { 
                int lastUpdatedProjectionId = int.Parse( Session[ "LastUpdatedProjectionId" ].ToString() );
                updatedRowIndex = projectionGridView.GetRowIndexFromId( lastUpdatedProjectionId, 0 );

                SetProjectionGridViewSelectedItem( updatedRowIndex, false );

                // bind to select
                projectionGridView.DataBind();
            } 

            // enable appropriate buttons for the selected row
            SetEnabledProjectionControlsDuringEdit( projectionGridView, updatedRowIndex, true );

            EnableDisableSBAActions( true );

            return( updatedRowIndex );

        }

        private int DeleteProjection( GridView projectionGridView, int rowIndex )
        {
            int updatedRowIndex = -1;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            ProjectionContractNumberParameterValue = currentDocument.ContractNumber;
            UserLoginParameterValue = bs.UserInfo.LoginName;

            ProjectionIdParameterValue = projectionGridView.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

            try
            {
                ProjectionDataSource.Delete();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }

            // previous row gets focus
            if( rowIndex >= 1 ) 
            { 
                updatedRowIndex = rowIndex - 1;
            } 
            else 
            {
                updatedRowIndex = rowIndex;
            } 

            SetProjectionGridViewSelectedItem( updatedRowIndex, false );

            // bind to select
            projectionGridView.DataBind();

            return( updatedRowIndex );
        }

#endregion UpdateInsertDeleteProjection


        #region SBAAssociatedContracts

        protected void BindAssociatedContractsGridView()
        {
            try
            {
                // bind if postback not due to grid edit
                if( BlockGridBindingForAssociatedContractsGridControlPostback == false )  
                    AssociatedContractsGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        //// returns the status, also changes the color of the status control in the parent form view
        //protected string ContractStatus( object expirationDateObj, object completionDateObj, string labelControlName )
        //{
        //    Label statusLabel = ( Label )AssociatedContractsGridPanel.FindControl( labelControlName );
        //    string status = "";

        //    status = CMGlobals.GetContractStatus( expirationDateObj, completionDateObj, statusLabel );

        //    return ( status );
        //}

        protected void AssociatedContractsGridPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "AssociatedContractsGridPanel" );

            AssociatedContractsGridPanel.Visible = bVisible;
            AssociatedContractsGridPanel.Enabled = documentControlPresentation.IsControlEnabled( "AssociatedContractsGridPanel" );

        }

        protected void AssociatedContractsGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int contractId = -1;
            string contractNumber = "";
            int scheduleNumber = -1;
            int itemIndex = -1;

            if( e.CommandName.CompareTo( "JumpToAssociatedContract" ) == 0 )
            {

                string commandArgument = e.CommandArgument.ToString();
                string[] argumentList = commandArgument.Split( new char[] { ',' } );

                itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                contractId = int.Parse( argumentList[ 1 ].ToString() );
                contractNumber = argumentList[ 2 ].ToString();
                scheduleNumber = int.Parse( argumentList[ 3 ].ToString() );

                AssociatedContractsGridView.SelectedIndex = itemIndex;

                HighlightAssociatedContractsRow( itemIndex );

                ( ( NACCM )Page.Master.Master ).ViewSelectedContract( contractId, contractNumber, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.AssociatedContractsGridView );

                // allow the postback to occur 
              //  SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        protected void AssociatedContractsGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                if( e.Row.RowType == DataControlRowType.DataRow )
                {

                    Label contractStatusLabel = null;
                    contractStatusLabel = ( Label )e.Row.FindControl( "ContractStatusLabel" );

                    string expirationDateString = dataRowView[ "ExpirationDate" ].ToString();
                    string completionDateString = dataRowView[ "CompletionDate" ].ToString();

                    if( contractStatusLabel != null )
                    { 
                        contractStatusLabel.Text = CMGlobals.GetContractStatus( ( object )expirationDateString, ( object )completionDateString, contractStatusLabel );
                    }

                        
                    Label responsibleContractLabel = null;
                    responsibleContractLabel = ( Label )e.Row.FindControl( "ResponsibleContractLabel" );
                    bool bIsResponsibleContract = ( dataRowView[ "IsResponsible" ].ToString().CompareTo( "1" ) == 0 ) ? true : false;

                    if( responsibleContractLabel != null )
                    {
                        if( bIsResponsibleContract == true )
                        {
                            responsibleContractLabel.Text = "(Responsible)";
                        }
                    }

                    Button selectAssociatedContractButton = null;
                    selectAssociatedContractButton = ( Button )e.Row.FindControl( "SelectAssociatedContractButton" );
                    if( selectAssociatedContractButton != null )
                    {
                        string cursorChangeToHand = string.Format( "contractNumberMouseChange(this, 'over', {0}, '{1}');", 1, "alt" );
                        string cursorChangeToNormal = string.Format( "contractNumberMouseChange(this, 'out', {0}, '{1}');", 1, "alt" );

                        selectAssociatedContractButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        selectAssociatedContractButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void AssociatedContractsGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id field
            if( e.Row.Cells.Count > AssociatedContractsContractIdFieldNumber )
            {
                e.Row.Cells[ AssociatedContractsContractIdFieldNumber ].Visible = false;
            }

            if( e.Row.Cells.Count > AssociatedContractsIsResponsibleFieldNumber )
            {
                e.Row.Cells[ AssociatedContractsIsResponsibleFieldNumber ].Visible = false;
            }
        }


        protected void SetAssociatedContractsGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {

            if( selectedItemIndex < AssociatedContractsGridView.Rows.Count )
            {

                // save for postback
                Session[ "AssociatedContractsGridViewSelectedIndex" ] = selectedItemIndex;

                // set the row selected
                AssociatedContractsGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true )
                {
                    ScrollToSelectedAssociatedContractsItem();
                }

                SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }


        protected void ScrollToSelectedAssociatedContractsItem()
        {

            int rowIndex = AssociatedContractsGridView.SelectedIndex;   // took out + 1 here to make header visible on add

            int fudge = ( int )Math.Floor( ( double )rowIndex / ( double )100.0 );
            int rowPosition = ( ASSOCIATEDCONTRACTGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( ASSOCIATEDCONTRACTGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string restoreAssociatedContractGridSelectionScript = String.Format( "RestoreAssociatedContractGridSelection( {0},{1} );", rowPosition, rowIndex );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestoreAssociatedContractGridSelectionScript", restoreAssociatedContractGridSelectionScript, true ); // runs after controls established
        }

        protected void HighlightAssociatedContractsRow( int itemIndex )
        {
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            if( AssociatedContractsGridView.HasData() == true )
            {
                GridViewRow row = AssociatedContractsGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = AssociatedContractsGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = AssociatedContractsGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setAssociatedContractsHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveAssociatedContractsHighlightingScript", preserveHighlightingScript, true );
            }
        }

        protected void RestoreAssociatedContractsGridViewSelectedItem()
        {
            if( Session[ "AssociatedContractsGridViewSelectedIndex" ] == null )
                return;

            AssociatedContractsGridView.SelectedIndex = int.Parse( Session[ "AssociatedContractsGridViewSelectedIndex" ].ToString() );

            //ScrollToSelectedItem();

            //SBAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }



        #endregion SBAAssociatedContracts

    }
}