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
    public partial class ContractGeneral : BaseDocumentEditorPage
    {
        public ContractGeneral()
            : base( DocumentEditorTypes.Contract )
        {
        }


        private const int AssociatedBPAContractsSelectBPAAssociatedContractButtonFieldNumber = 0;
        private const int AssociatedBPAContractsContractIdFieldNumber = 1;     

        private const int ASSOCIATEDCONTRACTGRIDVIEWROWHEIGHTESTIMATE = 48;

        private bool _bBlockAssociatedBPAContractsGridBindingForGridControlPostback = false;

        // this fixes issues with gridview edit save or cancel not redrawing out of edit mode
        protected new void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( Page.IsPostBack != true )
            {              
                BindAssociatedBPAContractsGridView();
            }
        }

        protected new void Page_Load( object sender, EventArgs e )
        {
            if( Session[ "NACCMStartedProperly" ] == null || Request.UrlReferrer == null )
            {
                Response.Redirect( "~/Start.aspx" );
            }
            
            if( Page.IsPostBack == false && ( ( NACCM )Page.Master.Master ).IsRedirected() == true )   // chgd to true 4/3/2017
            {
                ClearSessionVariables();
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );

            LoadAndBindNonFormViewControls();

            if( Page.IsPostBack == false )
            {  
                SetAssociatedBPAContractsGridViewSelectedItem( 0, true );
                BindAssociatedBPAContractsGridView();
            }
            else
            {
                RestoreAssociatedBPAContractsGridViewSelectedItem();
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
                string refreshDateType = "";
                bool bRefreshOrNot = false;

                HiddenField refreshDateValueOnSubmitHiddenField = ( HiddenField )ContractGeneralContractDatesFormView.FindControl( "RefreshDateValueOnSubmit" );
                HiddenField refreshOrNotOnSubmitHiddenField = ( HiddenField )ContractGeneralContractDatesFormView.FindControl( "RefreshOrNotOnSubmit" );
   
                if( refreshDateValueOnSubmitHiddenField != null )
                {
                     refreshDateType = refreshDateValueOnSubmitHiddenField.Value.ToString();

                     if( refreshOrNotOnSubmitHiddenField != null )
                     {
                         bRefreshOrNot = Boolean.Parse( refreshOrNotOnSubmitHiddenField.Value );

                         if( refreshDateType.Contains("Undefined") == false )
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

        private void CheckEventTarget()
        {
            _bBlockAssociatedBPAContractsGridBindingForGridControlPostback = false;

            string controlName = Request.Form[ "__EVENTTARGET" ];

            if( controlName != null )
            {
                // set up to block binding if postback is from grid control
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$ProjectionGridView$ctl02$ )
                if( controlName.Contains( "SelectBPAAssociatedContractButton" ) == true )
                {
                    _bBlockAssociatedBPAContractsGridBindingForGridControlPostback = true;
                }
            }
        }


        public bool BlockGridBindingForAssociatedBPAContractsGridControlPostback
        {
            get
            {
                return _bBlockAssociatedBPAContractsGridBindingForGridControlPostback;
            }
            set
            {
                _bBlockAssociatedBPAContractsGridBindingForGridControlPostback = value;
            }
        }

        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            ContractGeneralContractDatesFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;    
            ContractGeneralContractDatesFormView.DataKeyNames = new string[] { "ContractId" };

            ContractGeneralContractAttributesFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;   
            ContractGeneralContractAttributesFormView.DataKeyNames = new string[] { "ContractId" };

            ContractGeneralParentContractFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;       
            ContractGeneralParentContractFormView.DataKeyNames = new string[] { "ContractId" };

            ContractGeneralContractAssignmentFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            ContractGeneralContractAssignmentFormView.DataKeyNames = new string[] { "ContractId" };
        }

        protected void ClearSessionVariables()
        {
            Session[ "C2Award" ] = null;
            Session[ "C2Effective" ] = null;
            Session[ "C2Expiration" ] = null;
            Session[ "C2Completion" ] = null;
        }

        protected void BindFormViews()
        {
            BindHeader();

            ContractGeneralContractDatesFormView.DataBind();
            ContractGeneralContractAttributesFormView.DataBind();
            ContractGeneralParentContractFormView.DataBind();
            ContractGeneralContractAssignmentFormView.DataBind();
        }

        public override void BindAfterShortSave()
        {
            BindFormViews();
        }

        public override void RebindHeader()
        {
            BindHeader();
        }


#region SINControl

        protected void LoadAndBindNonFormViewControls()
        {
            LoadScheduleSINs();
            LoadContractSINs();
            LoadAssociatedBPAContracts();

            SINListView.DataSource = base.ContractSINDataSource;
            SINListView.DataKeyNames = new string[] { "SIN" };

            AssociatedBPAContractsGridView.DataSource = AssociatedBPAContractsDataSource;

            if( Page.IsPostBack == false )
            {
                if( ( ( NACCM )Page.Master.Master ).IsRedirected() == true )
                {
                    base.ContractSINDataSource.SetEventOwnerName( "ContractGeneral" );
                    base.ContractSINDataSource.Inserted += new SqlDataSourceStatusEventHandler( ContractSINDataSource_Inserted );   

                    BindSINListView();
                }
            }
        }

        protected void BindSINListView()
        {
            try
            {
                // bind
                SINListView.DataBind();

                int i = ContractSINCount;
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        protected void BindSINDropDownList( DropDownList SINDropDownList )
        {
            try
            {
                // bind
                SINDropDownList.DataBind();

          //      int i = ScheduleSINCount;  $$$ this causes an null reference exception because the _scheduleSINDataSource has not been created at this point
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

 
        protected void SINDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList sinDropDownList = ( DropDownList )sender;
            ListItem selectedItem = sinDropDownList.SelectedItem;
            Session[ "AddContractSIN" ] = selectedItem.Value;
        }

 
        protected void RecoverableCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox RecoverableCheckBox = ( CheckBox )sender;
            ListViewDataItem sinItem = ( ListViewDataItem )RecoverableCheckBox.NamingContainer;

            if( sinItem != null )
            {
                DataRowView dataRowView = ( DataRowView )sinItem.DataItem;

                CheckBox recoverableCheckBox = ( CheckBox )sinItem.FindControl( "RecoverableCheckBox" );
                if( recoverableCheckBox != null )
                {
                    recoverableCheckBox.Checked = bool.Parse( dataRowView[ "Recoverable" ].ToString() );
                }
            }
        }
    
        protected void SINListView_OnItemCommand( Object sender, ListViewCommandEventArgs e )
        {
            ListView sinListView = ( ListView )sender;
            string commandName = e.CommandName;

            string commandArguments = e.CommandArgument.ToString();
            string[] commandArgumentList = commandArguments.Split(new char[] {','});

            int rowIndex = int.Parse( commandArgumentList[0].ToString() );
            int displayIndex = int.Parse( commandArgumentList[ 1 ].ToString() );
            string SIN = "";
            Boolean bRecoverable = false;

            switch( commandName )
            {
                case "SINListViewSaveInsert":
                    try
                    {
                        DropDownList SINDropDownList = ( DropDownList )sinListView.InsertItem.FindControl( "SINDropDownList" );
                        SIN = SINDropDownList.SelectedItem.Text;
                        CheckBox recoverableCheckBox = ( CheckBox )sinListView.InsertItem.FindControl( "RecoverableCheckBox" );
                        bRecoverable = recoverableCheckBox.Checked;

                        // clicking save without selecting -- may happen if there is only one SIN available or the first sin is desired
                        if( Session[ "AddContractSIN" ] == null )
                        {
                            Session[ "AddContractSIN" ] = SIN;                         
                        }

                        base.SINParameterValue = SIN;
                        base.RecoverableParameterValue = bRecoverable.ToString();

                        base.ContractSINDataSource.Insert();

                        BindSINListView();  // $$$ added

                        ContractGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );  // $$$ added 10/28/2015
                    }
                    catch( Exception ex )
                    {
                        // user friendly common exception
                        if( ex.Message.Contains( "duplicate key" ) == true )
                        {
                            AppendValidationError( string.Format( "The selected SIN {0} is already included in this contract.", SIN ), false );
                            ShowValidationErrors();
                        }
                        else
                        {
                            ShowException( ex );
                        }
                    }
                    break;
                case "SINListViewDelete":
                    try
                    {
                        // either of these works:
                        //if( commandArgumentList.Length == 3 )
                        //    SIN = commandArgumentList[ 2 ].ToString(); 
                        SIN = sinListView.GetStringValueFromSelectedControl( displayIndex, 0, "SINLabel" );

                        base.SINParameterValue = SIN;

                        base.ContractSINDataSource.Delete();

                        BindSINListView();

                        ContractGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );  // $$$ added 10/28/2015
                    }
                    catch( Exception ex )
                    {
                        ShowException( ex );
                    }
                    break;
                case "SINListViewCancelInsert":
                    try
                    {
                        sinListView.InsertItemPosition = InsertItemPosition.None;

                        sinListView.EditIndex = -1;

                        AddingRecord = false;

                        LoadContractSINs();
                        BindSINListView();

                        ContractGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );  // $$$ added 10/28/2015
                    }
                    catch( Exception ex )
                    {
                        ShowException( ex );
                    }
                    break;
            }
        }

        protected void RecoverableCheckBox_OnCheckChanged( Object sender, EventArgs e )
        {
            CheckBox recoverableCheckBox = ( CheckBox )sender;
            bool bChecked = recoverableCheckBox.Checked;
            ListViewItem selectedItem = ( ListViewItem )recoverableCheckBox.NamingContainer;
            ListViewDataItem selectedDataItem = ( ListViewDataItem )selectedItem;

            string selectedSIN = SINListView.DataKeys[ selectedDataItem.DisplayIndex ].Value.ToString();

            SINParameterValue = selectedSIN;
            RecoverableParameterValue = bChecked.ToString();

            try
            {
                ContractSINDataSource.Update();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        protected void AddContractSIN_OnClick( Object sender, EventArgs e )
        {
            SINListView.InsertItemPosition = InsertItemPosition.FirstItem;

            AddingRecord = true;

            SINListView.EditIndex = 0;

            BindSINListView();

            ContractGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        //private void InitiateEditModeForContractSIN( int editIndex )
        //{
        //    // select the edited item
        //    if( AddingRecord == true )
        //    {
        //        // scroll to new row
        //  //      SINListViewDataPager.SetPageProperties( 0, SINListViewDataPager.MaximumRows, false );
        //    }
  
        //    BindSINListView();

        //    ContractGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        //}

        protected void SINListView_OnItemCreated( Object sender, ListViewItemEventArgs e )
        {
            if( e.Item.ItemType == ListViewItemType.InsertItem )
            {
                DropDownList SINDropDownList = ( DropDownList )e.Item.FindControl( "SINDropDownList" ); // change InsertItem to HiddenTemplate not to e.Item
                SINDropDownList.DataSource = base.ScheduleSINDataSource;
                if( Session[ "AddContractSIN" ] != null )
                {
                    string selectedSIN = ( string )Session[ "AddContractSIN" ];
                    CMGlobals.SelectTextInDropDownList( ref SINDropDownList, selectedSIN );
                    
                }
                BindSINDropDownList( SINDropDownList );
            }
        }

        // note SINPanel also has OnPreRender
        protected void SINListViewDataPager_OnPreRender( Object sender, EventArgs e )
        {
            if( Page.IsPostBack == true )
            {
                if( AddingRecord == false )
                {
                    LoadContractSINs(); // new $$$ - restored the prerender body on 9/25

                    BindSINListView();

                    ContractGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );  // took out on 10/30/15 $$$
                }
            }
        }

        protected void SINPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            SINPanel.Visible = documentControlPresentation.IsControlVisible( "SINPanel" );
            SINPanel.Enabled = documentControlPresentation.IsControlEnabled( "SINPanel" );
        }

        protected void ContractSINDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            SINListView.InsertItemPosition = InsertItemPosition.None;

            AddingRecord = false;

            SINListView.EditIndex = -1;

            LoadContractSINs();

            BindSINListView();

            ContractGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

#endregion SINControl

        protected void SelectParentFSSContractNumberButton_OnClick( object sender, EventArgs e )
        {
            int parentContractId = base.DataRelay.EditedDocumentContentFront.ParentContractId;
            string parentContractNumber = base.DataRelay.EditedDocumentContentFront.ParentFSSContractNumber;
            int parentScheduleNumber = base.DataRelay.EditedDocumentContentFront.ParentScheduleNumber;

            ( ( NACCM )Page.Master.Master ).ViewSelectedContract( parentContractId, parentContractNumber, parentScheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.ContractGeneralParentContractFormView );               
        }

        public override string GetValidationGroupName()
        {
            return ( "ContractGeneral" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( dataRelay.EditedDocumentContentFront.ContractAwardDate.CompareTo( DateTime.MinValue ) == 0 )
            {
                AppendValidationError( "Award date is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.ContractEffectiveDate.CompareTo( DateTime.MinValue ) == 0 )
            {
                AppendValidationError( "Effective date is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.ContractExpirationDate.CompareTo( DateTime.MinValue ) == 0 )
            {
                AppendValidationError( "Expiration date is required.", bIsShortSave );
                bSuccess = false;
            }
            else
            {            
                if( currentDocument != null )
                {            
                    if( currentDocument.ExpirationDate != null && currentDocument.EffectiveDate != null )
                    {                
                        if( dataRelay.EditedDocumentContentFront.ContractExpirationDate.CompareTo( currentDocument.ExpirationDate) < 0 && currentDocument.EffectiveDate.CompareTo( DateTime.Today ) <= 0 )
                        {
                            AppendValidationError( "Edited expiration date may not be less than current expiration date.", bIsShortSave );
                            bSuccess = false;
                        }
                    }
                }
            }

            if( dataRelay.EditedDocumentContentFront.ContractAwardDate.CompareTo( dataRelay.EditedDocumentContentFront.ContractEffectiveDate ) > 0 )
            {
                // rule grandfathered in for certain contracts
                if( currentDocument != null )
                {
                    if( currentDocument.ContractNumber.CompareTo( "VA797-BO-0310" ) != 0 &&
                        currentDocument.ContractNumber.CompareTo( "VA797-BO-0361" ) != 0 &&
                        currentDocument.ContractNumber.CompareTo( "V797P-4108b" ) != 0 &&
                        currentDocument.ContractNumber.CompareTo( "V797P-3153m" ) != 0 )
                    {
                        AppendValidationError( "Effective date must be after or equal to award date.", bIsShortSave );
                        bSuccess = false;
                    }
                }
                else
                {
                    AppendValidationError( "Effective date must be after or equal to award date.", bIsShortSave );
                    bSuccess = false;
                }  
            }

            if( dataRelay.EditedDocumentContentFront.ContractEffectiveDate.CompareTo( dataRelay.EditedDocumentContentFront.ContractExpirationDate ) > 0 )
            {
                AppendValidationError( "Expiration date must be after or equal to effective date.", bIsShortSave );
                bSuccess = false;
            }

            bool bCompletionDateProvided = false;
            if( dataRelay.EditedDocumentContentFront.ContractCompletionDate != null )
            {
                if( dataRelay.EditedDocumentContentFront.ContractCompletionDate.CompareTo( DateTime.MinValue ) != 0 )
                {
                    bCompletionDateProvided = true;

                    if( dataRelay.EditedDocumentContentFront.ContractAwardDate.CompareTo( dataRelay.EditedDocumentContentFront.ContractCompletionDate ) > 0 )
                    {
                        AppendValidationError( "Completion date must be after or equal to award date.", bIsShortSave );
                        bSuccess = false;
                    }
                }
            }

            //if( bCompletionDateProvided == true )
            //{
            //    if( dataRelay.EditedDocumentContentFront.TerminatedByConvenience != true && dataRelay.EditedDocumentContentFront.TerminatedByDefault != true )
            //    {
            //        AppendValidationError( "Termination reason is required if termination date is provided." );
            //        bSuccess = false;
            //    }
            //}

            if( dataRelay.EditedDocumentContentFront.TotalOptionYears == -1 )
            {
                AppendValidationError( "Selection of total option years is required.", bIsShortSave );
                bSuccess = false;
            }

            if( dataRelay.EditedDocumentContentFront.COID == -1 )
            {
                AppendValidationError( "Selection of assigned CO is required.", bIsShortSave );
                bSuccess = false;
            }

            // only validate SINs if visible
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];
            if( documentControlPresentation.IsControlVisible( "SINPanel" ) == true )
            {
                if( ContractSINCount == 0 )
                {
                    AppendValidationError( "Addition of at least one SIN is required.", bIsShortSave );
                    bSuccess = false;
                }
            }

            return ( bSuccess );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;
            
            string awardDate = "";
            string effectiveDate = "";
            string expirationDate = "";
            string completionDate = "";
            string optionYears = "";
            DateTime parseDate;
            int parseInt;

            try
            {
                TextBox awardDateTextBox = ( TextBox )ContractGeneralContractDatesFormView.FindControl( "AwardDateTextBox" );
                if( awardDateTextBox != null )
                    awardDate = awardDateTextBox.Text;

                if( awardDate.Trim().Length > 0 )
                {
                    if( DateTime.TryParseExact( awardDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                      //  throw new Exception( "Award date is not a valid date." );
                        ErrorMessage = "Award date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        dataRelay.EditedDocumentContentFront.ContractAwardDate = parseDate;
                        Session[ "C2Award" ] = parseDate.ToString( "MM/dd/yyyy" );
       
                    }
                }
 

                TextBox effectiveDateTextBox = ( TextBox )ContractGeneralContractDatesFormView.FindControl( "EffectiveDateTextBox" );
                if( effectiveDateTextBox != null )
                    effectiveDate = effectiveDateTextBox.Text;

                if( effectiveDate.Trim().Length > 0 )
                {
                    if( DateTime.TryParseExact( effectiveDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                       // throw new Exception( "Effective date is not a valid date." );
                        ErrorMessage = "Effective date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        dataRelay.EditedDocumentContentFront.ContractEffectiveDate = parseDate;
                        Session[ "C2Effective" ] = parseDate.ToString( "MM/dd/yyyy" );
                    }
                }
         

                TextBox expirationDateTextBox = ( TextBox )ContractGeneralContractDatesFormView.FindControl( "ExpirationDateTextBox" );
                if( expirationDateTextBox != null )
                    expirationDate = expirationDateTextBox.Text;

                if( expirationDate.Trim().Length > 0 )
                {
                    if( DateTime.TryParseExact( expirationDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                        // throw new Exception( "Expiration date is not a valid date." );
                        ErrorMessage = "Expiration date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        dataRelay.EditedDocumentContentFront.ContractExpirationDate = parseDate;
                        Session[ "C2Expiration" ] = parseDate.ToString( "MM/dd/yyyy" );
                    }
                }
            
                bool bCompletionDateProvided = false;

                TextBox completionDateTextBox = ( TextBox )ContractGeneralContractDatesFormView.FindControl( "CompletionDateTextBox" );
                if( completionDateTextBox != null )
                    completionDate = completionDateTextBox.Text;

                if( completionDate.Trim().Length > 0 )
                {
                    if( DateTime.TryParseExact( completionDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                        // throw new Exception( "Completion date is not a valid date." );
                        ErrorMessage = "Completion date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        bCompletionDateProvided = true;
                        dataRelay.EditedDocumentContentFront.ContractCompletionDate = parseDate;
                        Session[ "C2Completion" ] = parseDate.ToString( "MM/dd/yyyy" );
                    }
                }

                DropDownList optionYearsDropDownList = ( DropDownList )ContractGeneralContractDatesFormView.FindControl( "OptionYearsDropDownList" );
                if( optionYearsDropDownList != null )
                    optionYears = optionYearsDropDownList.SelectedItem.Text;

                if( optionYears.Trim().Length > 0 )
                {
                    if( Int32.TryParse( optionYears, out parseInt ) == false )
                    {
                        // throw new Exception( "Option years is not a valid number." );
                        ErrorMessage = "Option years is not a valid number.";
                        bSuccess = false;
                    }
                    else
                    {
                        dataRelay.EditedDocumentContentFront.TotalOptionYears = parseInt;
                    }
                }

                CheckBox TerminatedByConvenienceCheckBox = ( CheckBox )ContractGeneralContractDatesFormView.FindControl( "TerminatedByConvenienceCheckBox" );
                if( TerminatedByConvenienceCheckBox != null )
                    dataRelay.EditedDocumentContentFront.TerminatedByConvenience = TerminatedByConvenienceCheckBox.Checked;

                CheckBox TerminatedByDefaultCheckBox = ( CheckBox )ContractGeneralContractDatesFormView.FindControl( "TerminatedByDefaultCheckBox" );
                if( TerminatedByDefaultCheckBox != null )
                    dataRelay.EditedDocumentContentFront.TerminatedByDefault = TerminatedByDefaultCheckBox.Checked;

                //if( bCompletionDateProvided == true )
                //{
                //    if( dataRelay.EditedDocumentContentFront.TerminatedByConvenience != true && dataRelay.EditedDocumentContentFront.TerminatedByDefault != true )
                //    {
                //        ErrorMessage = "Termination reason is required if termination date is provided.";
                //        bSuccess = false;
                //    }
                //}

                TextBox ContractDescriptionTextBox = ( TextBox )ContractGeneralContractAttributesFormView.FindControl( "ContractDescriptionTextBox" );
                if( ContractDescriptionTextBox != null )
                    dataRelay.EditedDocumentContentFront.ContractDescription = ContractDescriptionTextBox.Text;

                CheckBox PrimeVendorCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "PrimeVendorCheckBox" );
                if( PrimeVendorCheckBox != null )
                    dataRelay.EditedDocumentContentFront.PrimeVendorParticipation = PrimeVendorCheckBox.Checked;

                CheckBox VADODContractCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "VADODContractCheckBox" );
                if( VADODContractCheckBox != null )
                    dataRelay.EditedDocumentContentFront.VADOD = VADODContractCheckBox.Checked;
 
                CheckBox TradeAgreementYesCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "TradeAgreementYesCheckBox" );
                CheckBox TradeAgreementOtherCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "TradeAgreementOtherCheckBox" );
                if( TradeAgreementYesCheckBox != null && TradeAgreementOtherCheckBox != null )
                {
                    if( TradeAgreementYesCheckBox.Checked == true && TradeAgreementOtherCheckBox.Checked == false )
                    {
                        dataRelay.EditedDocumentContentFront.TradeAgreementActCompliance = "C";
                    }
                    else if( TradeAgreementYesCheckBox.Checked == false && TradeAgreementOtherCheckBox.Checked == true )
                    {
                        dataRelay.EditedDocumentContentFront.TradeAgreementActCompliance = "O";
                    }
                    else
                    {
                        dataRelay.EditedDocumentContentFront.TradeAgreementActCompliance = " ";  // neither checked is an option, which is indicated by one space
                    }
                }

                CheckBox StimulusActCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "StimulusActCheckBox" );
                if( StimulusActCheckBox != null )
                    dataRelay.EditedDocumentContentFront.StimulusAct = StimulusActCheckBox.Checked;

                CheckBox StandardizedCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "StandardizedCheckBox" );
                if( StandardizedCheckBox != null )
                    dataRelay.EditedDocumentContentFront.Standardized = StandardizedCheckBox.Checked;

                DropDownList contractingOfficerDropDownList = ( DropDownList )ContractGeneralContractAssignmentFormView.FindControl( "ContractingOfficerDropDownList" );
                if( contractingOfficerDropDownList != null )
                {
                    if( contractingOfficerDropDownList.SelectedItem != null )
                    {
                        dataRelay.EditedDocumentContentFront.COID = Int32.Parse( contractingOfficerDropDownList.SelectedItem.Value.ToString() );
                        dataRelay.EditedDocumentContentFront.ContractingOfficerFullName = contractingOfficerDropDownList.SelectedItem.Text;
                    }
                }
            }
            catch( Exception ex )
            {
                bSuccess = false;
                ErrorMessage = string.Format( "The following exception was encountered validating the general contract information: {0}", ex.Message );
            }
            return ( bSuccess );
        }

        protected void ContractGeneralContractDatesFormView_OnChange( object sender, EventArgs e )
        {
     //       SetDirtyFlag( "ContractGeneralContractDatesFormView" );
        }
                           
        protected void AwardDateTextBox_OnTextChanged( object sender, EventArgs e )
        {
            TextBox awardDateTextBox = ( TextBox )sender;
            DateTime date = DateTime.MinValue;

            if( awardDateTextBox != null )
            {
                string dateString = awardDateTextBox.Text;
                if( DateTime.TryParse( dateString, out date ) == true )
                {
                    if( DataRelay.EditedDocumentContentFront != null )
                    {
                        DataRelay.EditedDocumentContentFront.ContractAwardDate = date;
                        Session[ "C2Award" ] = date.ToString( "MM/dd/yyyy" );
                    }
                }
                else if( dateString.Trim().Length == 0 )
                {
                    DataRelay.EditedDocumentContentFront.ContractAwardDate = DateTime.MinValue;
                    Session[ "C2Award" ] = null;
                }
                // ignoring dates that are not valid
            }
        }

        protected void EffectiveDateTextBox_OnTextChanged( object sender, EventArgs e )
        {
            TextBox effectiveDateTextBox = ( TextBox )sender;
            DateTime date = DateTime.MinValue;

            if( effectiveDateTextBox != null )
            {
                string dateString = effectiveDateTextBox.Text;
                if( DateTime.TryParse( dateString, out date ) == true )
                {
                    if( DataRelay.EditedDocumentContentFront != null )
                    {
                        DataRelay.EditedDocumentContentFront.ContractEffectiveDate = date;
                        Session[ "C2Effective" ] = date.ToString( "MM/dd/yyyy" );
                    }
                }
                else if( dateString.Trim().Length == 0 )
                {
                    DataRelay.EditedDocumentContentFront.ContractEffectiveDate = DateTime.MinValue;
                    Session[ "C2Effective" ] = null;
                }
                // ignoring dates that are not valid
            }
        }

        protected void ExpirationDateTextBox_OnTextChanged( object sender, EventArgs e )
        {
            TextBox expirationDateTextBox = ( TextBox )sender;
            DateTime date = DateTime.MinValue;

            if( expirationDateTextBox != null )
            {
                string dateString = expirationDateTextBox.Text;
                if( DateTime.TryParse( dateString, out date ) == true )
                {
                    if( DataRelay.EditedDocumentContentFront != null )
                    {
                        DataRelay.EditedDocumentContentFront.ContractExpirationDate = date;
                        Session[ "C2Expiration" ] = date.ToString( "MM/dd/yyyy" );
                    }
                }
                else if( dateString.Trim().Length == 0 )
                {
                    DataRelay.EditedDocumentContentFront.ContractExpirationDate = DateTime.MinValue;
                    Session[ "C2Expiration" ] = null;
                }
                // ignoring dates that are not valid
            }
        }

        protected void CompletionDateTextBox_OnTextChanged( object sender, EventArgs e )
        {
            TextBox completionDateTextBox = ( TextBox )sender;
            DateTime date = DateTime.MinValue;

            if( completionDateTextBox != null )
            {
                string dateString = completionDateTextBox.Text;
                if( DateTime.TryParse( dateString, out date ) == true )
                {
                    if( DataRelay.EditedDocumentContentFront != null )
                    {
                        DataRelay.EditedDocumentContentFront.ContractCompletionDate = date;
                        Session[ "C2Completion" ] = date.ToString( "MM/dd/yyyy" );
                    }
                }
                else if( dateString.Trim().Length == 0 )
                {
                    DataRelay.EditedDocumentContentFront.ContractCompletionDate = DateTime.MinValue;
                    Session[ "C2Completion" ] = null;
                }
                // ignoring dates that are not valid
            }

        }

        protected void ContractGeneralContractDatesFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "ContractGeneralContractDatesFormView" );
            
            ContractGeneralContractDatesFormView.Visible = bVisible;
            ContractGeneralContractDatesFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "ContractGeneralContractDatesFormView" );

            if( bVisible == true )
            {
                EnableContractDateEditing( ( FormView )sender );

                UpdateCurrentOptionYearDisplay( ( FormView )sender );
            }
        }

        protected void ContractGeneralContractAttributesFormView_OnChange( object sender, EventArgs e )
        {
     //       SetDirtyFlag( "ContractGeneralContractAttributesFormView" );
        }

        protected void ContractGeneralContractAssignmentFormView_OnChange( object sender, EventArgs e )
        {
      //      SetDirtyFlag( "ContractGeneralContractAssignmentFormView" );
        }

        protected void ContractGeneralContractAssignmentFormView_OnPreRender( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bEnabled = documentControlPresentation.IsFormViewEnabled( "ContractGeneralContractAssignmentFormView" );
            ContractGeneralContractAssignmentFormView.Visible = documentControlPresentation.IsFormViewVisible( "ContractGeneralContractAssignmentFormView" );
            ContractGeneralContractAssignmentFormView.Enabled = bEnabled;

            if( bEnabled == true )
            {
                DropDownList ContractingOfficerDropDownList = ( DropDownList )ContractGeneralContractAssignmentFormView.FindControl( "ContractingOfficerDropDownList" );

                if( ContractingOfficerDropDownList != null )
                {
                    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.ContractAssignment ) != true )
                    {
                        ContractingOfficerDropDownList.Enabled = false;
                    }
                }
            }
        }

        protected void ContractGeneralContractAttributesFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "ContractGeneralContractAttributesFormView" );

            ContractGeneralContractAttributesFormView.Visible = bVisible;
            ContractGeneralContractAttributesFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "ContractGeneralContractAttributesFormView" );

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];


            if( bVisible == true )
            {
                TableCell VADODDataCell = ( TableCell )ContractGeneralContractAttributesFormView.FindControl( "VADODDataCell" );
                CheckBox VADODContractCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "VADODContractCheckBox" );

                if( currentDocument.IsService( currentDocument.ScheduleNumber ) == true )
                {
                    if( VADODContractCheckBox != null )
                    {
                        VADODContractCheckBox.Visible = false;
                        VADODContractCheckBox.Enabled = false;
                    }

                    if( VADODDataCell != null )
                    {
                        VADODDataCell.Visible = false;
                    }
                }
                else
                {
                    if( VADODContractCheckBox != null )
                    {
                        VADODContractCheckBox.Visible = true;
                        VADODContractCheckBox.Enabled = true;
                    }

                    if( VADODDataCell != null )
                    {
                        VADODDataCell.Visible = true;
                    }
                }

                CheckBox StandardizedCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "StandardizedCheckBox" );
                if( StandardizedCheckBox != null )
                {
                    if( currentDocument.CanHaveStandardizedItems( currentDocument.ScheduleNumber ) == false )
                    {
                        StandardizedCheckBox.Visible = false;
                        StandardizedCheckBox.Enabled = false;
                    }
                    else
                    {
                        StandardizedCheckBox.Visible = true;
                        StandardizedCheckBox.Enabled = true;
                    }
                }

                TableCell StimulusDataCell = ( TableCell )ContractGeneralContractAttributesFormView.FindControl( "StimulusDataCell" );
                CheckBox StimulusActCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "StimulusActCheckBox" );

                // only showing stimulus act for historical reference
                if( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Active )
                {
                    if( StimulusActCheckBox != null )
                    {
                        StimulusActCheckBox.Visible = false;
                        StimulusActCheckBox.Enabled = false;
                    }

                    if( StimulusDataCell != null )
                    {
                        StimulusDataCell.Visible = false;
                    }
                }

                // the prime vendor at the contract level is not applicable to pharm items, but may be applicable to medsurg items on the pharm contract
                CheckBox PrimeVendorCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "PrimeVendorCheckBox" );
                TableCell PrimeVendorDataCell = ( TableCell )ContractGeneralContractAttributesFormView.FindControl( "PrimeVendorDataCell" );
                TableRow PrimeVendorRow = ( TableRow )ContractGeneralContractAttributesFormView.FindControl( "PrimeVendorRow" );

                // pv checkbox should be invisible if service;  vadod checkbox should be invisible if service
                if( currentDocument.IsService( currentDocument.ScheduleNumber ) == true )
                {
                    if( PrimeVendorCheckBox != null )
                    {
                        PrimeVendorCheckBox.Visible = false;
                        PrimeVendorCheckBox.Enabled = false;
                    }
                    
                    if( PrimeVendorDataCell != null  )
                    {
                        PrimeVendorDataCell.Visible = false;
                    }

                    if( PrimeVendorRow != null )
                    {
                        PrimeVendorRow.Visible = false;
                }
                }
                else
                {
                    if( PrimeVendorCheckBox != null )
                    {
                        PrimeVendorCheckBox.Visible = true;
                        PrimeVendorCheckBox.Enabled = true;                       
                    }

                    if( PrimeVendorDataCell != null )
                    {
                        PrimeVendorDataCell.Visible = true;
                    }

                    if( PrimeVendorRow != null )
                {
                        PrimeVendorRow.Visible = true;
                    }
                }
          
                // add a spacer cell and then expand or contract via attributes
                // BlankSpacerCell1.Attributes.Clear();
                // BlankSpacerCell1.Attributes.Add( "colspan", "2" );

                CheckBox TradeAgreementYesCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "TradeAgreementYesCheckBox" );
                CheckBox TradeAgreementOtherCheckBox = ( CheckBox )ContractGeneralContractAttributesFormView.FindControl( "TradeAgreementOtherCheckBox" );
                Label TradeAgreementHeaderLabel = ( Label )ContractGeneralContractAttributesFormView.FindControl( "TradeAgreementHeaderLabel" );
                TableRow TradeAgreementRow = ( TableRow )ContractGeneralContractAttributesFormView.FindControl( "TradeAgreementRow" );

                // non-services
                if( currentDocument.CanHaveTradeAgreement( currentDocument.ScheduleNumber ) == true )
                {
                    if( TradeAgreementYesCheckBox != null && TradeAgreementOtherCheckBox != null && TradeAgreementHeaderLabel != null )
                    {
                        TradeAgreementYesCheckBox.Visible = true;
                        TradeAgreementYesCheckBox.Enabled = true;

                        TradeAgreementOtherCheckBox.Visible = true;
                        TradeAgreementOtherCheckBox.Enabled = true;

                        TradeAgreementHeaderLabel.Visible = true;
                        TradeAgreementRow.Visible = true;
                    }
                }
                else
                {
                    if( TradeAgreementYesCheckBox != null && TradeAgreementOtherCheckBox != null && TradeAgreementHeaderLabel != null && TradeAgreementRow != null )
                    {
                        TradeAgreementYesCheckBox.Visible = false;
                        TradeAgreementYesCheckBox.Enabled = false;

                        TradeAgreementOtherCheckBox.Visible = false;
                        TradeAgreementOtherCheckBox.Enabled = false;

                        TradeAgreementHeaderLabel.Visible = false;
                        TradeAgreementRow.Visible = false;
                    }
                }

                TableRow MedSurgRow = ( TableRow )ContractGeneralContractAttributesFormView.FindControl( "MedSurgRow" );

                if( PrimeVendorRow != null && TradeAgreementRow != null && MedSurgRow != null )
                {
                    if( PrimeVendorRow.Visible == false && TradeAgreementRow.Visible == false )
                    {
                        MedSurgRow.Visible = false;
                    }
                }
            }
        }

        protected void ContractGeneralParentContractFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "ContractGeneralParentContractFormView" );
            ContractGeneralParentContractFormView.Visible = bVisible;
            ContractGeneralParentContractFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "ContractGeneralParentContractFormView" );

            if( bVisible == true )
            {
                Button selectParentFSSContractNumberButton = ( Button )ContractGeneralParentContractFormView.FindControl( "SelectParentFSSContractNumberButton" );

                if( selectParentFSSContractNumberButton != null )
                {
                    string cursorChangeToHand = string.Format( "contractNumberMouseChange(this, 'over', {0}, '{1}');", 1, "alt");
                    string cursorChangeToNormal = string.Format( "contractNumberMouseChange(this, 'out', {0}, '{1}');", 1, "alt" );

                    selectParentFSSContractNumberButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                    selectParentFSSContractNumberButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                }
            }
        }

        protected bool GetTradeAgreementCheckBoxValue( string tradeAgreementActCompliance, string checkBoxType )
        {
            bool bChecked = false;
      
            if( checkBoxType.CompareTo( "Yes" ) == 0 )
            {
                if( tradeAgreementActCompliance.CompareTo( "C" ) == 0 )
                {
                    bChecked = true;
                }
            }
            else if( checkBoxType.CompareTo( "Other" ) == 0 )
            {
                if( tradeAgreementActCompliance.CompareTo( "O" ) == 0 )
                {
                    bChecked = true;
                }
            }
            return( bChecked );
        }

        protected void TradeAgreementYesCheckBox_OnCheckedChanged( object sender, EventArgs e )
        {
            CheckBox tradeAgreementYesCheckBox = ( CheckBox )sender;
            CheckBox tradeAgreementOtherCheckBox;

            FormView contractGeneralContractAttributesFormView;

            if( tradeAgreementYesCheckBox != null )
            {

                contractGeneralContractAttributesFormView = ( FormView )tradeAgreementYesCheckBox.NamingContainer;
                if( contractGeneralContractAttributesFormView != null )
                {
                    tradeAgreementOtherCheckBox = ( CheckBox )contractGeneralContractAttributesFormView.FindControl( "TradeAgreementOtherCheckBox" );
       
                    if( tradeAgreementOtherCheckBox != null )
                    {
                        if( tradeAgreementYesCheckBox.Checked == true )
                        {
                            tradeAgreementOtherCheckBox.Checked = false;
              //              SetDirtyFlag( "ContractGeneralContractAttributesFormView" );
             //               TriggerContractViewMasterUpdatePanelFromContract();       these 4 trigger statements were removed when debugging BPA refresh problem.  they were left out because there was no apparent side effect of leaving them out TBD
                        }
                    }
                }
            }
        }

       protected void TradeAgreementOtherCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
            CheckBox tradeAgreementOtherCheckBox = ( CheckBox )sender;
            CheckBox tradeAgreementYesCheckBox;

            FormView contractGeneralContractAttributesFormView;

            if( tradeAgreementOtherCheckBox != null )
            {

                contractGeneralContractAttributesFormView = ( FormView )tradeAgreementOtherCheckBox.NamingContainer;
                if( contractGeneralContractAttributesFormView != null )
                {
                    tradeAgreementYesCheckBox = ( CheckBox )contractGeneralContractAttributesFormView.FindControl( "TradeAgreementYesCheckBox" );
       
                    if( tradeAgreementYesCheckBox != null )
                    {
                        if( tradeAgreementOtherCheckBox.Checked == true )
                        {
                            tradeAgreementYesCheckBox.Checked = false;
              //              SetDirtyFlag( "ContractGeneralContractAttributesFormView" );
             //               TriggerContractViewMasterUpdatePanelFromContract();
                        }
                    }
                }
            }
        }

       protected void TerminatedByConvenienceCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox terminatedByConvenienceCheckBox = ( CheckBox )sender;
           CheckBox terminatedByDefaultCheckBox;

           FormView contractGeneralContractDatesFormView;

           if( terminatedByConvenienceCheckBox != null )
           {

               contractGeneralContractDatesFormView = ( FormView )terminatedByConvenienceCheckBox.NamingContainer;
               if( contractGeneralContractDatesFormView != null )
               {
                   terminatedByDefaultCheckBox = ( CheckBox )contractGeneralContractDatesFormView.FindControl( "TerminatedByDefaultCheckBox" );

                   if( terminatedByDefaultCheckBox != null )
                   {
                       if( terminatedByConvenienceCheckBox.Checked == true )
                       {
                           terminatedByDefaultCheckBox.Checked = false;
             //              SetDirtyFlag( "ContractGeneralContractDatesFormView" );
            //               TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }

       protected void TerminatedByDefaultCheckBox_OnCheckedChanged( object sender, EventArgs e )
       {
           CheckBox terminatedByDefaultCheckBox = ( CheckBox )sender;
           CheckBox terminatedByConvenienceCheckBox;

           FormView contractGeneralContractDatesFormView;

           if( terminatedByDefaultCheckBox != null )
           {
               contractGeneralContractDatesFormView = ( FormView )terminatedByDefaultCheckBox.NamingContainer;
               if( contractGeneralContractDatesFormView != null )
               {
                   terminatedByConvenienceCheckBox = ( CheckBox )contractGeneralContractDatesFormView.FindControl( "TerminatedByConvenienceCheckBox" );

                   if( terminatedByConvenienceCheckBox != null )
                   {
                       if( terminatedByDefaultCheckBox.Checked == true )
                       {
                           terminatedByConvenienceCheckBox.Checked = false;
            //               SetDirtyFlag( "ContractGeneralContractDatesFormView" );
           //                TriggerContractViewMasterUpdatePanelFromContract();
                       }
                   }
               }
           }
       }

        // returns the status, also changes the color of the status control in the parent form view
        protected string ParentContractStatus( object expirationDateObj, object completionDateObj, string labelControlName )
        {
            Label statusLabel = ( Label )ContractGeneralParentContractFormView.FindControl( labelControlName );
            string status = "";

            MasterPage master = Page.Master;
            if( master != null )
            {
                status = CMGlobals.GetContractStatus( expirationDateObj, completionDateObj, statusLabel );
            }
            return( status );
        }

        protected string GetCurrentOptionYear( object totalOptionYearsObj, object effectiveDateObj, object expirationDateObj, object completionDateObj )
        {
            string currentOptionYearString = "";
            int totalOptionYears = -1;
            DateTime effectiveDateAsDate;
            DateTime expirationDateAsDate;
            DateTime completionDateAsDate;

            DateTime currentDateTime = DateTime.Now;
            
            int currentOptionYear = -1;
            bool bIsExpired = false;
            DateTime testDate;

            if( totalOptionYearsObj != DBNull.Value )
            {
                totalOptionYears = int.Parse( totalOptionYearsObj.ToString() );

                effectiveDateAsDate = DateTime.Parse( effectiveDateObj.ToString() );
                expirationDateAsDate = DateTime.Parse( expirationDateObj.ToString() );
                completionDateAsDate = DateTime.Parse( completionDateObj.ToString() );

                if( expirationDateAsDate.Date.CompareTo( currentDateTime.Date ) < 0 )
                {
                    bIsExpired = true;
                }

                if( completionDateAsDate.Date.CompareTo( currentDateTime.Date ) < 0 )
                {
                    bIsExpired = true;
                }

                if( bIsExpired == false )
                {
                    int effectiveDateYear = effectiveDateAsDate.Year;
                    int currentDateYear = currentDateTime.Year;

                    currentOptionYear = currentDateYear - effectiveDateYear;

                    // adjust option year for current month/day
                    // handle leap day

                    if( currentDateTime.Day == 29 && currentDateTime.Month == 2 ) 
                    {
                        testDate = new DateTime(effectiveDateYear, 2, 28);
                    }
                    else
                    {
                        testDate = new DateTime(effectiveDateYear, currentDateTime.Month, currentDateTime.Day);
                    }

                    if( testDate.CompareTo( effectiveDateAsDate ) < 0)
                    {
                        currentOptionYear = currentOptionYear - 1;
                    }

                    currentOptionYearString = currentOptionYear.ToString();
                }
                else
                {
                    currentOptionYearString = "N/A";
                }
            }
            else
            {
                currentOptionYearString = "N/A";
            }
     
            return( currentOptionYearString );
        }

        protected string GetCurrentOptionYear( int totalOptionYears, DateTime effectiveDate, DateTime expirationDate, DateTime completionDate )
        {
            string currentOptionYearString = "";
                
            DateTime currentDateTime = DateTime.Now;

            int currentOptionYear = -1;
            bool bIsExpired = false;
            DateTime testDate;

            if( totalOptionYears > 0 )
            {
                if( effectiveDate.CompareTo( DateTime.MinValue ) != 0 )
                {
                    if( expirationDate.CompareTo( DateTime.MinValue ) != 0 )
                    {
                        if( expirationDate.Date.CompareTo( currentDateTime.Date ) < 0 )
                        {
                            bIsExpired = true;
                        }

                        if( completionDate.CompareTo( DateTime.MinValue ) != 0 )
                        {
                            if( completionDate.Date.CompareTo( currentDateTime.Date ) < 0 )
                            {
                                bIsExpired = true;
                            }
                        }

                        if( bIsExpired == false )
                        {
                            int effectiveDateYear = effectiveDate.Year;
                            int currentDateYear = currentDateTime.Year;

                            currentOptionYear = currentDateYear - effectiveDateYear;

                            // adjust option year for current month/day
                            // handle leap day

                            if( currentDateTime.Day == 29 && currentDateTime.Month == 2 )
                            {
                                testDate = new DateTime( effectiveDateYear, 2, 28 );
                            }
                            else
                            {
                                testDate = new DateTime( effectiveDateYear, currentDateTime.Month, currentDateTime.Day );
                            }

                            if( testDate.CompareTo( effectiveDate ) < 0 )
                            {
                                currentOptionYear = currentOptionYear - 1;
                            }

                            // display 0 instead of -1 for first year
                            if( currentOptionYear == -1 )
                                currentOptionYear = 0;

                            currentOptionYearString = currentOptionYear.ToString();
                        }
                        else
                        {
                            currentOptionYearString = "N/A";
                        }
                    }
                    else
                    {
                        currentOptionYearString = "N/A";
                    }
                }
                else
                {
                    currentOptionYearString = "N/A";
                }
            }
            else
            {
                currentOptionYearString = "N/A";
            }

            return ( currentOptionYearString );
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
                        Session[ "C2Award" ] = contractAwardDate.ToString( "MM/dd/yyyy" );
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
                        Session[ "C2Effective" ] = contractEffectiveDate.ToString( "MM/dd/yyyy" );
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
                        Session[ "C2Expiration" ] = contractExpirationDate.ToString( "MM/dd/yyyy" );
                   }
                }
            }
        }

        protected void CompletionDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox completionDateTextBox = ( TextBox )sender;

            FormView contractGeneralContractDatesFormView;

            if( completionDateTextBox != null )
            {
                contractGeneralContractDatesFormView = ( FormView )completionDateTextBox.NamingContainer;

                if( contractGeneralContractDatesFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )contractGeneralContractDatesFormView.DataItem;

                    DateTime contractCompletionDate = editedDocumentContent.ContractCompletionDate;

                    if( contractCompletionDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        completionDateTextBox.Text = "";
                    }
                    else
                    {
                        completionDateTextBox.Text = contractCompletionDate.ToString( "MM/dd/yyyy" );
                        Session[ "C2Completion" ] = contractCompletionDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }
        
        protected void ParentFSSCompletionDateLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label parentFSSCompletionDateLabel = ( Label )sender;
 
            FormView contractGeneralParentContractFormView;

            if( parentFSSCompletionDateLabel != null )
            {
                contractGeneralParentContractFormView = ( FormView )parentFSSCompletionDateLabel.NamingContainer;

                if( contractGeneralParentContractFormView != null )
                {
                    EditedDocumentContent editedDocumentContent = ( EditedDocumentContent )contractGeneralParentContractFormView.DataItem;

                    String parentCompletionDateString = editedDocumentContent.ParentCompletionDate;

                    if( parentCompletionDateString.Length > 0 )
                    {
                        DateTime parentCompletionDate = DateTime.Parse( parentCompletionDateString );

                        if( parentCompletionDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                        {
                            parentFSSCompletionDateLabel.Text = " ";
                        }
                        else
                        {
                            parentFSSCompletionDateLabel.Text = parentCompletionDate.ToString( "MM/dd/yyyy" );
                        }
                    }
                }
            }
        }

        protected void UpdateCurrentOptionYearDisplay( FormView sender )
        {
            Page currentPage;
            FormView contractGeneralContractDatesFormView = sender;

            currentPage = contractGeneralContractDatesFormView.Page;
                   
            TextBox currentOptionYearLabel = ( TextBox )contractGeneralContractDatesFormView.FindControl( "CurrentOptionYearLabel" );

            string optionYearsStr = "0";
            int totalOptionYears = 0;

            DropDownList optionYearsDropDownList = ( DropDownList )ContractGeneralContractDatesFormView.FindControl( "OptionYearsDropDownList" );
            if( optionYearsDropDownList != null )
            {
                optionYearsStr = optionYearsDropDownList.SelectedItem.Text;

                if( optionYearsStr.Length > 0 )
                {
                    totalOptionYears = Int32.Parse( optionYearsStr );
                }
            }

            
            TextBox effectiveDateTextBox = ( TextBox )contractGeneralContractDatesFormView.FindControl( "EffectiveDateTextBox" );
            TextBox expirationDateTextBox = ( TextBox )contractGeneralContractDatesFormView.FindControl( "ExpirationDateTextBox" );
            TextBox completionDateTextBox = ( TextBox )contractGeneralContractDatesFormView.FindControl( "CompletionDateTextBox" );

            DateTime effectiveDate = DateTime.MinValue;
            DateTime expirationDate = DateTime.MinValue;
            DateTime completionDate = DateTime.MinValue;

            if( effectiveDateTextBox != null )
            {
                if( effectiveDateTextBox.Text.Length > 0 )
                {
                    DateTime.TryParse( effectiveDateTextBox.Text, out effectiveDate );
                }
            }
            
            if( expirationDateTextBox != null )
            {
                if( expirationDateTextBox.Text.Length > 0 )
                {
                    DateTime.TryParse( expirationDateTextBox.Text, out expirationDate );
                }
            }

            if( completionDateTextBox != null )
            {
                if( completionDateTextBox.Text.Length > 0 )
                {
                    DateTime.TryParse( completionDateTextBox.Text, out completionDate );
                }
            }



            if( currentOptionYearLabel != null )
            {
                currentOptionYearLabel.Text = GetCurrentOptionYear( totalOptionYears, effectiveDate, expirationDate, completionDate );
            }
        }

        // each contract date is authorized separately
        protected void EnableContractDateEditing( FormView sender )
        {
            Page currentPage;
            FormView contractGeneralContractDatesFormView = sender;

            currentPage = contractGeneralContractDatesFormView.Page;

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            bool bUnlimited = false;
            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.ContractUnlimitedDateRange ) == true )
            {
                bUnlimited = true;
            }

            DateTime awardMinDate = DateTime.Now.AddDays( -60 );
            DateTime effectiveMinDate = DateTime.Now.AddDays( -14 );
            DateTime expirationMinDate = DateTime.Now;
            DateTime completionMinDate = DateTime.Now;
            DateTime maxAllowedDate = DateTime.Now.AddYears( 10 );

            // create image button scripts - old way
            //ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "C2AwardDateButtonOnClickScript", GetDateButtonScript( "C2Award" ), true );
            //ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "C2EffectiveDateButtonOnClickScript", GetDateButtonScript( "C2Effective" ), true );
            //ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "C2ExpirationDateButtonOnClickScript", GetDateButtonScript( "C2Expiration" ), true );
            //ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "C2CompletionDateButtonOnClickScript", GetDateButtonScript( "C2Completion" ), true );

            // create image button scripts - new way with parameterized limits
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "C2AwardDateButtonOnClickScript", GetDateButtonScript( "C2Award", bUnlimited, awardMinDate, maxAllowedDate ), true );
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "C2EffectiveDateButtonOnClickScript", GetDateButtonScript( "C2Effective", bUnlimited, effectiveMinDate, maxAllowedDate ), true );
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "C2ExpirationDateButtonOnClickScript", GetDateButtonScript( "C2Expiration", bUnlimited, expirationMinDate, maxAllowedDate ), true );
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "C2CompletionDateButtonOnClickScript", GetDateButtonScript( "C2Completion", bUnlimited, completionMinDate, maxAllowedDate ), true );
 

            ImageButton awardDateImageButton = ( ImageButton )contractGeneralContractDatesFormView.FindControl( "AwardDateImageButton" );
            TextBox awardDateTextBox = ( TextBox )contractGeneralContractDatesFormView.FindControl( "AwardDateTextBox" );

            if( awardDateImageButton != null )
            {
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.ContractAwardDate ) == true )
                {
                    awardDateImageButton.Visible = true;
                    awardDateTextBox.Enabled = true;
                }
                else
                {
                    awardDateImageButton.Visible = false;
                    awardDateTextBox.Enabled = false;
                }
            }
        
            ImageButton effectiveDateImageButton = ( ImageButton )contractGeneralContractDatesFormView.FindControl( "EffectiveDateImageButton" );
            TextBox effectiveDateTextBox = ( TextBox )contractGeneralContractDatesFormView.FindControl( "EffectiveDateTextBox" );

            if( effectiveDateImageButton != null )
            {
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.ContractEffectiveDate ) == true )
                {
                    effectiveDateImageButton.Visible = true;
                    effectiveDateTextBox.Enabled = true;
                }
                else
                {
                    effectiveDateImageButton.Visible = false;
                    effectiveDateTextBox.Enabled = false;
                }
            }

            ImageButton expirationDateImageButton = ( ImageButton )contractGeneralContractDatesFormView.FindControl( "ExpirationDateImageButton" );
            TextBox expirationDateTextBox = ( TextBox )contractGeneralContractDatesFormView.FindControl( "ExpirationDateTextBox" );

            if( expirationDateImageButton != null )
            {
                if((( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.ContractExpirationDate ) == true || currentDocument.EditStatus == CurrentDocument.EditStatuses.CanEdit ) && currentDocument.ScheduleNumber != 1 ) ||
                    ( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmContractExpirationDate ) == true && currentDocument.ScheduleNumber == 1 ))
                {
                    expirationDateImageButton.Visible = true;
                    expirationDateTextBox.Enabled = true;
                }
                else
                {
                    expirationDateImageButton.Visible = false;
                    expirationDateTextBox.Enabled = false;
                }
            }

            ImageButton completionDateImageButton = ( ImageButton )contractGeneralContractDatesFormView.FindControl( "CompletionDateImageButton" );
            TextBox completionDateTextBox = ( TextBox )contractGeneralContractDatesFormView.FindControl( "CompletionDateTextBox" );

            if( completionDateImageButton != null )
            {
                if( ( ( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.ContractCompletionDate ) == true || currentDocument.EditStatus == CurrentDocument.EditStatuses.CanEdit ) && currentDocument.IsPharmaceutical( currentDocument.ScheduleNumber ) == false ) ||
                    ( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmContractCompletionDate ) == true && currentDocument.IsPharmaceutical( currentDocument.ScheduleNumber ) == true ) )
                {
                    completionDateImageButton.Visible = true;
                    completionDateTextBox.Enabled = true;
                }
                else
                {
                    completionDateImageButton.Visible = false;
                    completionDateTextBox.Enabled = false;
                }
            }
        }

        // old way without parameterized limits to be retired $$$
        //public string GetDateButtonScript( string dateTypeString )
        //{
        //    string defaultDateString = "";

        //    if( Session[ dateTypeString ] != null )
        //    {
        //        defaultDateString = Session[ dateTypeString ].ToString();
        //    }
        //    else
        //    {
        //        defaultDateString = DateTime.Today.ToShortDateString();
        //    }

        //    string script = String.Format("function On{0}DateButtonClick() {{ window.open('Calendar.aspx?InitialDate={1}&DateType={2}','Calendar','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=160,height=310'); return false;}}", dateTypeString, defaultDateString, dateTypeString );
            
        //    return( script );
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

           
            //string script = String.Format( "function On{0}DateButtonClick() {{ window.open('Calendar.aspx?InitialDate={1}&DateType={2}&Unlimited={3}&MinAllowedDate={4}&MaxAllowedDate={5}','Calendar','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=160,height=310'); return false;}}", dateTypeString, defaultDateString, dateTypeString, ( ( bUnlimitedDateRange == true ) ? 1 : 0 ), minAllowedDateString, maxAllowedDateString );
            // changes for chrome
            string script = String.Format( "function On{0}DateButtonClick() {{ window.open('Calendar.aspx?InitialDate={1}&DateType={2}&Unlimited={3}&MinAllowedDate={4}&MaxAllowedDate={5}','Calendar','toolbar=0,menubar=0,resizable=0,scrollbars=0,statusbar=0,location=0,width=250,height=340,left=660,top=300'); return false;}}", dateTypeString, defaultDateString, dateTypeString, ( ( bUnlimitedDateRange == true ) ? 1 : 0 ), minAllowedDateString, maxAllowedDateString );

            return ( script );
        }

        public void RefreshDate( string dateTypeString )
        {
            DateTime displayDate;

            if( dateTypeString.Contains( "C2Award" ) == true )
            {
                TextBox awardDateTextBox = ( TextBox )ContractGeneralContractDatesFormView.FindControl( "AwardDateTextBox" );
                if( Session[ "C2Award" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "C2Award" ].ToString() );
                    awardDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    awardDateTextBox.Text = "";
                }
            }

  
            if( dateTypeString.Contains( "C2Effective" ) == true )
            {
                TextBox effectiveDateTextBox = ( TextBox )ContractGeneralContractDatesFormView.FindControl( "EffectiveDateTextBox" );
                if( Session[ "C2Effective" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "C2Effective" ].ToString() );
                    effectiveDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    effectiveDateTextBox.Text = "";
                }
            }

            if( dateTypeString.Contains( "C2Expiration" ) == true )
            {
                TextBox expirationDateTextBox = ( TextBox )ContractGeneralContractDatesFormView.FindControl( "ExpirationDateTextBox" );
                if( Session[ "C2Expiration" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "C2Expiration" ].ToString() );
                    expirationDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    expirationDateTextBox.Text = "";
                }
            }

            if( dateTypeString.Contains( "C2Completion" ) == true )
            {
                TextBox completionDateTextBox = ( TextBox )ContractGeneralContractDatesFormView.FindControl( "CompletionDateTextBox" );
                if( Session[ "C2Completion" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "C2Completion" ].ToString() );
                    completionDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    completionDateTextBox.Text = "";
                }
            }
        }

        protected void ContractGeneralContractAssignmentFormView_OnDataBound( object sender, EventArgs e )
        {
            FormView contractGeneralContractAssignmentFormView = ( FormView )sender;

            EditedDocumentContent formViewContent = ( EditedDocumentContent )contractGeneralContractAssignmentFormView.DataItem;

            int selectedDivisionId = -1;
            int selectedCOID = -1;
            string selectedCOFullName = "";

            if( formViewContent != null )
            {
                selectedDivisionId = formViewContent.DivisionId;
                selectedCOID = formViewContent.COID;
                selectedCOFullName = formViewContent.ContractingOfficerFullName;
            }

            LoadContractingOfficersForDivision( selectedDivisionId );

            DropDownList contractingOfficerDropDownList = null;
    
            if( contractGeneralContractAssignmentFormView != null )
            {
                contractingOfficerDropDownList = ( DropDownList )contractGeneralContractAssignmentFormView.FindControl( "ContractingOfficerDropDownList" );

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
                    else // add it for display purposes on historical records
                    {
                        contractingOfficerDropDownList.Items.Add( new ListItem( selectedCOFullName, selectedCOID.ToString() ) );
                        contractingOfficerDropDownList.Items.FindByValue( selectedCOID.ToString() ).Selected = true;
                    }
                }
            }
        }


#region AssociatedBPAContracts

        protected void BindAssociatedBPAContractsGridView()
        {
            try
            {
                // bind if postback not due to grid edit
                if( BlockGridBindingForAssociatedBPAContractsGridControlPostback == false )
                    AssociatedBPAContractsGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        //// returns the status, also changes the color of the status control in the parent form view
        //protected string ContractStatus( object expirationDateObj, object completionDateObj, string labelControlName )
        //{
        //    Label statusLabel = ( Label )AssociatedBPAContractsGridPanel.FindControl( labelControlName );
        //    string status = "";

        //    status = CMGlobals.GetContractStatus( expirationDateObj, completionDateObj, statusLabel );

        //    return ( status );
        //}

        protected void AssociatedBPAContractsGridPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "AssociatedBPAContractsGridPanel" );

            AssociatedBPAContractsGridPanel.Visible = bVisible;
            AssociatedBPAContractsGridPanel.Enabled = documentControlPresentation.IsControlEnabled( "AssociatedBPAContractsGridPanel" );
        }

        protected void AssociatedBPAContractsGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int contractId = -1;
            string contractNumber = "";
            int scheduleNumber = -1;
            int itemIndex = -1;

            if( e.CommandName.CompareTo( "JumpToAssociatedBPAContract" ) == 0 )
            {

                string commandArgument = e.CommandArgument.ToString();
                string[] argumentList = commandArgument.Split( new char[] { ',' } );

                itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                contractId = int.Parse( argumentList[ 1 ].ToString() );
                contractNumber = argumentList[ 2 ].ToString();
                scheduleNumber = int.Parse( argumentList[ 3 ].ToString() );

                AssociatedBPAContractsGridView.SelectedIndex = itemIndex;

                HighlightAssociatedBPAContractsRow( itemIndex );

                ( ( NACCM )Page.Master.Master ).ViewSelectedContract( contractId, contractNumber, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.AssociatedBPAContractsGridView );

            }
        }

        protected void AssociatedBPAContractsGridView_RowDataBound( object sender, GridViewRowEventArgs e )
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

                    Button selectBPAAssociatedContractButton = null;
                    selectBPAAssociatedContractButton = ( Button )e.Row.FindControl( "SelectBPAAssociatedContractButton" );

                    if( selectBPAAssociatedContractButton != null )
                    {
                        string cursorChangeToHand = string.Format( "contractNumberMouseChange(this, 'over', {0}, '{1}');", 1, "alt" );
                        string cursorChangeToNormal = string.Format( "contractNumberMouseChange(this, 'out', {0}, '{1}');", 1, "alt" );

                        selectBPAAssociatedContractButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        selectBPAAssociatedContractButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }                                   
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void AssociatedBPAContractsGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id field
            if( e.Row.Cells.Count > AssociatedBPAContractsContractIdFieldNumber )
            {
                e.Row.Cells[ AssociatedBPAContractsContractIdFieldNumber ].Visible = false;
            }
        }


        protected void SetAssociatedBPAContractsGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {

            if( selectedItemIndex < AssociatedBPAContractsGridView.Rows.Count )
            {

                // save for postback
                Session[ "AssociatedBPAContractsGridViewSelectedIndex" ] = selectedItemIndex;

                // set the row selected
                AssociatedBPAContractsGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true )
                {
                    ScrollToSelectedAssociatedBPAContractsItem();
                }
            }
        }


        protected void ScrollToSelectedAssociatedBPAContractsItem()
        {

            int rowIndex = AssociatedBPAContractsGridView.SelectedIndex;   // took out + 1 here to make header visible on add

            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( ASSOCIATEDCONTRACTGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( ASSOCIATEDCONTRACTGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string restoreAssociatedBPAContractGridSelectionScript = String.Format( "RestoreAssociatedBPAContractsGridSelection( {0},{1} );", rowPosition, rowIndex );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestoreAssociatedBPAContractsGridSelectionScript", restoreAssociatedBPAContractGridSelectionScript, true ); // runs after controls established
        }

        protected void HighlightAssociatedBPAContractsRow( int itemIndex )
        {
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            if( AssociatedBPAContractsGridView.HasData() == true )
            {
                GridViewRow row = AssociatedBPAContractsGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = AssociatedBPAContractsGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = AssociatedBPAContractsGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setAssociatedBPAContractsHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveAssociatedBPAContractsHighlightingScript", preserveHighlightingScript, true );
            }
        }

        protected void RestoreAssociatedBPAContractsGridViewSelectedItem()
        {
            if( Session[ "AssociatedBPAContractsGridViewSelectedIndex" ] == null )
                return;

            AssociatedBPAContractsGridView.SelectedIndex = int.Parse( Session[ "AssociatedBPAContractsGridViewSelectedIndex" ].ToString() );
        }



#endregion AssociatedBPAContracts


    }
}