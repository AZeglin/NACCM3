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
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ContractChecks : BaseDocumentEditorPage
    {
       
        private const  int CheckEditButtonFieldNumber = 0;  // $$$+
        private const  int CheckYearQuarterFieldNumber = 1; //$$$+
        private const  int CheckAmountFieldNumber = 2; //$$$+
        private const  int CheckNumberFieldNumber = 3; //$$$+
        private const  int CheckDepositNumberFieldNumber = 4; //$$$+
        private const  int CheckDateReceivedFieldNumber = 5; //$$$+
        private const  int CheckCommentsFieldNumber = 6; //$$$+
  
        private const   int CheckRemoveButtonFieldNumber = 7;  // $$$+
        private const   int CheckIdFieldNumber = 8;  // $$$+
  

        private const int CHECKGRIDVIEWROWHEIGHTESTIMATE = 48;

        private bool _bBlockGridBindingForGridControlPostback = false;

        public ContractChecks()
            : base( DocumentEditorTypes.Contract )
        {
        }

        // this fixes issues with gridview edit save or cancel not redrawing out of edit mode
        protected new void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( Page.IsPostBack != true )
            {
                BindCheckGridView();
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

            CheckEventTarget();

            LoadNonFormViewControls();

            if( Page.IsPostBack == false )
            {
                SetCheckGridViewSelectedItem( 0, true );
                BindCheckGridView();
            }
            else
            {
                RestoreCheckGridViewSelectedItem();
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
        }

        private void CheckEventTarget()
        {
            _bBlockGridBindingForGridControlPostback = false;

            string controlName = Request.Form[ "__EVENTTARGET" ];

            if( controlName != null )
            {
                // set up to block binding if postback is from grid control
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$CheckGridView$ctl02$ )
                if( controlName.Contains( "yearQuarterDropDownList" ) == true ||
                    controlName.Contains( "checkAmountTextBox" ) == true ||
                    controlName.Contains( "checkNumberTextBox" ) == true ||
                    controlName.Contains( "depositNumberTextBox" ) == true ||
                    controlName.Contains( "dateReceivedTextBox" ) == true ||
                    controlName.Contains( "checkCommentsTextBox" ) == true ||
                    controlName.Contains( "RemoveCheckButton" ) == true )
                {
                    _bBlockGridBindingForGridControlPostback = true;
                }
            }
        }

        public bool BlockGridBindingForGridControlPostback
        {
            get
            { 
                return _bBlockGridBindingForGridControlPostback; 
            }
            set 
            { 
                _bBlockGridBindingForGridControlPostback = value; 
            }
        }

 

        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            ChecksHeaderFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            ChecksHeaderFormView.DataKeyNames = new string[] { "ContractId" };
        }

        
        protected void ClearSessionVariables()
        {
            Session[ "CheckGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedCheckId" ] = null;
            Session[ "LastInsertedCheckId" ] = null;
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
            BindHeader();

            ChecksHeaderFormView.DataBind();
 
            // note form view controls are not yet created here
        }

        protected void LoadNonFormViewControls()
        {
            LoadContractChecks();

            CheckGridView.DataSource = CheckDataSource;
        }


        protected void BindCheckGridView()
        {
            try
            {
                // bind if postback not due to grid edit
                if( BlockGridBindingForGridControlPostback == false )
                    CheckGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        //protected void ChecksHeaderFormView_OnChange( object sender, EventArgs e )
        //{
        //    SetDirtyFlag( "ChecksHeaderFormView" );
        //}

        protected void CheckGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedCheckId = -1;
            int itemIndex = -1;

            if( e.CommandName.CompareTo( "EditCheck" ) == 0 )
            {

                string commandArgument = e.CommandArgument.ToString();
                string[] argumentList = commandArgument.Split( new char[] { ',' } );

                itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                selectedCheckId = int.Parse( argumentList[ 1 ].ToString() );

                HighlightCheckRow( itemIndex );

                InitiateEditModeForCheck( itemIndex );

                // allow the postback to occur 
                CheckUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else
            {
                if( e.CommandName.CompareTo( "SaveCheck" ) == 0 )
                {

                    string commandArgument = e.CommandArgument.ToString();
                    string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                    itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                    selectedCheckId = int.Parse( argumentList[ 1 ].ToString() );

                    // validate the item before saving
                    bool bIsItemOk = ValidateCheckBeforeUpdate( CheckGridView, itemIndex, selectedCheckId, false );

                    if( bIsItemOk == true )
                    {
                        // is this an insert or an update
                        int newOrUpdatedRowIndex = -1;

                        if( CheckGridView.InsertRowActive == true )
                        {
                            newOrUpdatedRowIndex = InsertCheck( CheckGridView, itemIndex );
                        }
                        else
                        {
                            newOrUpdatedRowIndex = UpdateCheck( CheckGridView, itemIndex );
                        }

                        HighlightCheckRow( newOrUpdatedRowIndex );

                        // allow the postback to occur 
                        CheckUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                    }
                    else
                    {
                        ShowValidationErrors();
                    }
                }
                else
                {
                    if( e.CommandName.CompareTo( "Cancel" ) == 0 )
                    {
                        string commandArgument = e.CommandArgument.ToString();
                        string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                        itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                        selectedCheckId = int.Parse( argumentList[ 1 ].ToString() );

                        CancelEdit( itemIndex );
                    }
                    else
                    {
                        if( e.CommandName.CompareTo( "RemoveCheck" ) == 0 )
                        {
                            string commandArgument = e.CommandArgument.ToString();
                            string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                            itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                            selectedCheckId = int.Parse( argumentList[ 1 ].ToString() );

                            bool bContinueWithDelete = false;

                            bContinueWithDelete = GetConfirmationMessageResults();

                            if( bContinueWithDelete == true )
                            {
                                int newRowIndex = DeleteCheck( CheckGridView, itemIndex );

                                HighlightCheckRow( newRowIndex );

                                // allow the postback to occur 
                                CheckUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                            }
                        }
                    }
                }
            }
        }

        public override string GetValidationGroupName()
        {
            return ( "ContractChecks" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            // currently no fields outside of the grid

            return ( bSuccess );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            // currently no fields outside of the grid

            return ( bSuccess );
        }


        protected void ChecksHeaderFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "ChecksHeaderFormView" );
            bool bEnabled = documentControlPresentation.IsFormViewEnabled( "ChecksHeaderFormView" );

            ChecksHeaderFormView.Visible = bVisible;
            ChecksHeaderFormView.Enabled = bEnabled;

            if( bVisible == true && bEnabled == true )
            {
                EnableDisableCheckEditControls();
            }
        }

        protected void CheckGridPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "CheckGridPanel" );

            CheckGridPanel.Visible = bVisible;
            CheckGridPanel.Enabled = documentControlPresentation.IsControlEnabled( "CheckGridPanel" );
        }

        // disable add button and check box
        protected void EnableDisableCheckEditControls()
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument != null )
            {
                // allow checks to be added even if document not active
            //    bool bActive = ( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Active ) ? true : false;
                
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.Checks ) != true )  
                {
                    Button addCheckButton = ( Button )ChecksHeaderFormView.FindControl( "AddCheckButton" );

                    if( addCheckButton != null )
                    { 
                        addCheckButton.Enabled = false;
                    }
                } 
            } 
        }

        protected void CheckGridView_RowDataBound( object sender,  GridViewRowEventArgs e )
        {
            try
            {

                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;
                CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];

                if( e.Row.RowType == DataControlRowType.DataRow ) 
                { 
                    if( ((( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                        (( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert )) )
                    { 
                        Button removeCheckButton = null;
                        removeCheckButton = ( Button )e.Row.FindControl( "RemoveCheckButton" );

                        if( removeCheckButton != null )
                        { 
                            removeCheckButton.OnClientClick = "presentConfirmationMessage( 'Permanently delete the selected check from this contract?' );";
                        }

                        // currently allowing edit of checks even if document has expired
                        if( currentDocument != null )
                        { 
                            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.Checks ) != true ) 
                            { 
                                if( removeCheckButton != null )
                                { 
                                    removeCheckButton.Enabled = false;
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

                        // bind ddls during edit mode
                        DropDownList yearQuarterDropDownList = ( DropDownList )e.Row.FindControl( "yearQuarterDropDownList" );
                        yearQuarterDropDownList.DataSource = CheckDateDataSource;
                        yearQuarterDropDownList.DataBind();
                    } 
                } 
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void Save_ButtonClick( object sender, EventArgs e )
        {

        }

        protected void CheckGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {
        }

        protected void CheckGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForCheck( e.NewEditIndex );  // added this to match drug item grid handling 
        }

        protected void CheckGridView_RowInserting( object sender, GridViewInsertEventArgs e )
        {
        }

        protected void CheckGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            CancelEdit( e.RowIndex );
        }

        protected void CancelEdit( int rowIndex )
        {
            int cancelIndex = rowIndex;
            bool bInserting = CheckGridView.InsertRowActive;
            if( bInserting == true ) 
            { 
                CheckGridView.InsertRowActive = false; // cancels insert ( if inserting )

                AddingCheckRecord = false; //_withAddCheckParameter.DefaultValue = "false";
                CheckGridView.EditIndex = -1;
                BindCheckGridView();

                // enable appropriate buttons for the selected row
                SetEnabledCheckControlsDuringEdit( CheckGridView, cancelIndex, true );

                EnableControlsForCheckEditMode( true );

                HighlightCheckRow( 0 );   // $$$ should this be cancelIndex instead of 0 like other cancel functions

                // allow the postback to occur 
                CheckUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            } 
            else  // editing existing row
            {

                CheckGridView.EditIndex = -1; // cancels the edit
                BindCheckGridView();

                // enable appropriate buttons for the selected row
                SetEnabledCheckControlsDuringEdit( CheckGridView, cancelIndex, true );

                EnableControlsForCheckEditMode( true );

                HighlightCheckRow( cancelIndex );

                // allow the postback to occur 
                CheckUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            } 

        }

        protected void CheckGridView_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {
        }
        protected void CheckGridView_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {
        }

        protected void CheckGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id field
            if( e.Row.Cells.Count > CheckIdFieldNumber ) 
            { 
                e.Row.Cells[ CheckIdFieldNumber ].Visible = false;
            }
        }

        protected void CheckGridView_OnSelectedIndexChanged( object sender, EventArgs e )
        {
        }
        protected void CheckGridView_PreRender( object sender, EventArgs e )
        {
        }

        protected void CheckGridView_Init( object sender, EventArgs e )
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

        protected void AddNewCheckButton_OnClick( object sender, EventArgs e )
        {
            CheckGridView.Insert();

            AddingCheckRecord = true; // _withAddCheckParameter.DefaultValue = "true";

            BindCheckGridView();

            InitiateEditModeForCheck( 0 );

            CheckUpdatePanelEventProxy.InvokeEvent( new EventArgs() ); 
        }

        protected void InitiateEditModeForCheck( int editIndex )
        {
            CheckGridView.EditIndex = editIndex;

            // select the edited item also
            if( CheckGridView.InsertRowActive == true ) 
            { 
                SetCheckGridViewSelectedItem( editIndex, true ); // scroll to new row
            } 
            else 
            {
                SetCheckGridViewSelectedItem( editIndex, false );
            }

            CheckGridView.DataBind(); 

            // disable appropriate buttons for the selected row
            SetEnabledCheckControlsDuringEdit( CheckGridView, editIndex, false );

            // disable the non-edit controls before going into edit mode
            EnableControlsForCheckEditMode( false );

        }

#region "currentselectedcheckrow"


        protected void SetCheckGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {

            if( selectedItemIndex < CheckGridView.Rows.Count ) 
            { 

                // save for postback
                Session[ "CheckGridViewSelectedIndex" ] = selectedItemIndex;

                // set the row selected
                CheckGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true ) 
                { 
                    ScrollToSelectedItem();
                }

                CheckUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            } 
        }

        protected void ScrollToSelectedItem()
        {

            int rowIndex = CheckGridView.SelectedIndex;   // took out + 1 here to make header visible on add

            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( CHECKGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( CHECKGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string restoreCheckGridSelectionScript = String.Format( "RestoreCheckGridSelection( {0},{1} );", rowPosition, rowIndex );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestoreCheckGridSelectionScript", restoreCheckGridSelectionScript, true ); // runs after controls established
        }

        protected void HighlightCheckRow( int itemIndex )
        { 
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1; 

            if( CheckGridView.HasData() == true ) 
            {
                GridViewRow row = CheckGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate ) 
                {
                    highlightedRowOriginalColor = CheckGridView.AlternatingRowStyle.BackColor.ToString();
                } 
                else 
                {
                    highlightedRowOriginalColor = CheckGridView.RowStyle.BackColor.ToString();
                } 
                
                string preserveHighlightingScript = String.Format( "setCheckHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveCheckHighlightingScript", preserveHighlightingScript, true );
            } 
        }

        protected void RestoreCheckGridViewSelectedItem()
        {
            if( Session[ "CheckGridViewSelectedIndex" ] == null )
                return;
            
            CheckGridView.SelectedIndex = int.Parse( Session[ "CheckGridViewSelectedIndex" ].ToString() );

            //ScrollToSelectedItem();

            //CheckUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void SetEnabledCheckControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )   
        {
            gv.SetEnabledControlsForCell( rowIndex, CheckRemoveButtonFieldNumber, bEnabled ); // remove button

            gv.SetVisibleControlsForCell( rowIndex, CheckEditButtonFieldNumber, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, CheckEditButtonFieldNumber, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, CheckEditButtonFieldNumber, "CancelButton", !bEnabled );
        }


        // disable non-edit controls before going into edit mode
        protected void EnableControlsForCheckEditMode( bool bEnabled )
        {
            Button addCheckButton  = ( Button )ChecksHeaderFormView.FindControl( "AddCheckButton" );

            if( addCheckButton != null )
            { 
                addCheckButton.Enabled = bEnabled;
            } 
        }

#endregion 
  
        private bool ValidateCheckBeforeUpdate( GridView checkGridView, int itemIndex, int selectedCheckId, bool bIsShortSave )
        {
            bool bIsValid = true;   

            int quarterId = -1;
            decimal checkAmount = 0;
            DateTime dateReceived;
    
            string yearQuarterIdString = checkGridView.GetStringValueFromSelectedControl( itemIndex, CheckYearQuarterFieldNumber, 0, false, "yearQuarterDropDownList" );
            string checkAmountString = checkGridView.GetStringValueFromSelectedControl( itemIndex, CheckAmountFieldNumber, 0, false, "checkAmountTextBox" );
            string checkNumberString = checkGridView.GetStringValueFromSelectedControl( itemIndex, CheckNumberFieldNumber, 0, false, "checkNumberTextBox" );
            string checkDepositNumberString = checkGridView.GetStringValueFromSelectedControl( itemIndex, CheckDepositNumberFieldNumber, 0, false, "depositNumberTextBox" );
            string dateReceivedString = checkGridView.GetStringValueFromSelectedControl( itemIndex, CheckDateReceivedFieldNumber, 0, false, "dateReceivedTextBox" );
            string checkCommentsString = checkGridView.GetStringValueFromSelectedControl( itemIndex, CheckCommentsFieldNumber, 0, false, "checkCommentsTextBox" );

            if( int.TryParse( yearQuarterIdString, out quarterId ) == false )
            {
                bIsValid = false;
                AppendValidationError( "A year/quarter must be selected.", bIsShortSave );
            }

            if( Decimal.TryParse( checkAmountString, out checkAmount ) == false ) 
            {
                bIsValid = false;
                AppendValidationError( "A valid check amount is required.", bIsShortSave );
            }

            if( checkNumberString.Trim().Length == 0 )
            {
                bIsValid = false;
                AppendValidationError( "A check number is required.", bIsShortSave );
            }

            if( checkDepositNumberString.Trim().Length == 0 )
            {
                bIsValid = false;
                AppendValidationError( "A deposit number is required.", bIsShortSave );
            }

            if( DateTime.TryParseExact( dateReceivedString, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out dateReceived ) == false )
            {
                bIsValid = false;
                AppendValidationError( "A valid received date is required.", bIsShortSave );
            }
            else 
            {
                try
                {
                    System.Data.SqlTypes.SqlDateTime sqlDateTimeObj = new System.Data.SqlTypes.SqlDateTime( dateReceived );
                }
                catch( System.Data.SqlTypes.SqlTypeException ex )
                {
                    bIsValid = false;
                    AppendValidationError( "A valid received date is required.", bIsShortSave );
                }
            }

            if( bIsValid == true )
            {
                if( checkAmount == 0 && checkCommentsString.Length == 0 )
                {
                    bIsValid = false;
                    AppendValidationError( "If check amount is zero then comment entry is required.", bIsShortSave );
                }
            }
 
            return( bIsValid );
        }
    

        protected void RemoveCheckButton_DataBinding( object sender,  EventArgs e )
        {
            Button removeCheckButton  = ( Button )sender;
            if( removeCheckButton != null )
            { 
                CMGlobals.MultilineButtonText( removeCheckButton, new String[] { "Remove", "Check" } );
            } 
        }

        protected void ViewIFFSalesComparisonButton_OnClick( object sender, EventArgs e )
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            Report salesReport = new Report( "/Sales/Reports/IFFCheckComparisonByContract" );

            salesReport.AddParameter( "ContractNumber", currentDocument.ContractNumber );
            salesReport.AddReportUserLoginIdParameter();

            Session[ "ReportToShow" ] = salesReport;

            string windowOpenScript = "";

            windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=1,resizable=1,top=20,left=24,width=640,height=840');";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ReportViewerWindowOpenScript", windowOpenScript, true );
        }

#region "updateinsertparameters"

        private int InsertCheck( GridView checkGridView, int rowIndex ) 
        {
            int insertedRowIndex = 0;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs  = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            CheckContractNumberParameterValue = currentDocument.ContractNumber;
            UserLoginParameterValue = bs.UserInfo.LoginName;

            CheckQuarterIdParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckYearQuarterFieldNumber, 0, false, "yearQuarterDropDownList" );
            CheckAmountParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckAmountFieldNumber, 0, false, "checkAmountTextBox" );
            CheckNumberParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckNumberFieldNumber, 0, false, "checkNumberTextBox" );
            CheckDepositNumberParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckDepositNumberFieldNumber, 0, false, "depositNumberTextBox" );
            CheckDateReceivedParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckDateReceivedFieldNumber, 0, false, "dateReceivedTextBox" );
            CheckCommentsParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckCommentsFieldNumber, 0, false, "checkCommentsTextBox" );

            try
            {
                CheckDataSource.Insert();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }


            checkGridView.InsertRowActive = false; // done with insert
            checkGridView.EditIndex = -1; // done with edit
            AddingCheckRecord = false; // _withAddCheckParameter.DefaultValue = "false";   // no extra row
            checkGridView.DataBind(); //  bind with new row

            if( Session[ "LastInsertedCheckId" ] != null )
            { 
                int newCheckId = ( int )Session[ "LastInsertedCheckId" ];
                insertedRowIndex = checkGridView.GetRowIndexFromId( newCheckId, 0 );

                SetCheckGridViewSelectedItem( insertedRowIndex, true );

                // bind to select
                checkGridView.DataBind();
            } 

            // enable appropriate buttons for the selected row
            SetEnabledCheckControlsDuringEdit( checkGridView, insertedRowIndex, true );

            EnableControlsForCheckEditMode( true );

            return( insertedRowIndex );
        }
    


        private int UpdateCheck( GridView checkGridView, int rowIndex )
        {
            int updatedRowIndex = -1;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs  = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            CheckContractNumberParameterValue = currentDocument.ContractNumber;
            UserLoginParameterValue = bs.UserInfo.LoginName;

            CheckIdParameterValue = checkGridView.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

            CheckQuarterIdParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckYearQuarterFieldNumber, 0, false, "yearQuarterDropDownList" );
            CheckAmountParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckAmountFieldNumber, 0, false, "checkAmountTextBox" );
            CheckNumberParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckNumberFieldNumber, 0, false, "checkNumberTextBox" );
            CheckDepositNumberParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckDepositNumberFieldNumber, 0, false, "depositNumberTextBox" );
            CheckDateReceivedParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckDateReceivedFieldNumber, 0, false, "dateReceivedTextBox" );
            CheckCommentsParameterValue = checkGridView.GetStringValueFromSelectedControl( rowIndex, CheckCommentsFieldNumber, 0, false, "checkCommentsTextBox" );


            try
            {
                CheckDataSource.Update();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }

            checkGridView.EditIndex = -1; // done with edit
            checkGridView.DataBind();

            if( Session[ "LastUpdatedCheckId" ] != null )
            { 
                int lastUpdatedCheckId = int.Parse( Session[ "LastUpdatedCheckId" ].ToString() );
                updatedRowIndex = checkGridView.GetRowIndexFromId( lastUpdatedCheckId, 0 );

                SetCheckGridViewSelectedItem( updatedRowIndex, false );

                // bind to select
                checkGridView.DataBind();
            } 

            // enable appropriate buttons for the selected row
            SetEnabledCheckControlsDuringEdit( checkGridView, updatedRowIndex, true );

            EnableControlsForCheckEditMode( true );

            return( updatedRowIndex );

        }

        private int DeleteCheck( GridView checkGridView, int rowIndex )
        {
            int updatedRowIndex = -1;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            CheckContractNumberParameterValue = currentDocument.ContractNumber;
            UserLoginParameterValue = bs.UserInfo.LoginName;   // $$$ this may be redundant. the guid is added in DocumentDataSource event and the SP only takes the guid

            CheckIdParameterValue = checkGridView.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();


            try
            {
                CheckDataSource.Delete();
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

            SetCheckGridViewSelectedItem( updatedRowIndex, false );

            // bind to select
            checkGridView.DataBind();

            return( updatedRowIndex );
        }
    


 

#endregion

#region "checkDateSelection"


        protected void yearQuarterDropDownList_DataBound( object sender, EventArgs e )
        {
            DropDownList yearQuarterDropDownList  = ( DropDownList )sender;
            GridViewRow gridViewRow  = ( GridViewRow )yearQuarterDropDownList.NamingContainer;
            int quarterId = -1;
          
            ListItem customListItem  = new ListItem();

            if( gridViewRow != null )
            { 
                DataRowView dataRowView  = ( DataRowView )gridViewRow.DataItem;
                string quarterIdString = dataRowView[ "QuarterId" ].ToString();

                if( quarterIdString != null )
                { 
                    if( int.TryParse( quarterIdString, out quarterId )) 
                    { 
                        ListItem listItem  = yearQuarterDropDownList.Items.FindByValue( quarterId.ToString() );
                        if( listItem != null )
                        {
                            listItem.Selected = true;
                        }
                    } 
                } 
            } 
        }

        protected void yearQuarterDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList yearQuarterDropDownList  = ( DropDownList )sender;
            ListItem selectedItem;

            selectedItem = yearQuarterDropDownList.SelectedItem;

            int selectedQuarterId;

            selectedQuarterId = int.Parse( selectedItem.Value );
        }

 
    

#endregion "checkDateSelection"

    }
}