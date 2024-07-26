using System;
using System.Collections;
using System.Collections.Generic;
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
    public partial class ContractPaymentsNational : BaseDocumentEditorPage
    {
       
        private const  int PaymentEditButtonFieldNumber = 0;  // $$$+
        private const  int PaymentYearQuarterFieldNumber = 1; //$$$+
        private const  int PaymentAmountFieldNumber = 2; //$$$+

        private const  int SubmissionDateFieldNumber = 3; //$$$+
        private const  int SubmittedByFieldNumber = 4; //$$$+
        private const  int PaymentMethodFieldNumber = 5; //$$$+
        private const  int PaymentSourceFieldNumber = 6; //$$$+
        private const  int TransationIdFieldNumber = 7; //$$$+
        private const  int TrackingIdFieldNumber = 8; //$$$+
        private const  int DepositNumberFieldNumber = 9; //$$$+ 
        private const  int DebitVoucherNumberFieldNumber = 10; //$$$+
        private const  int CheckNumberFieldNumber = 11; //$$$+
        private const  int SettlementDateFieldNumber = 12; //$$$+
        private const  int PaymentCommentsFieldNumber = 13; //$$$+
  
        private const   int PaymentRemoveButtonFieldNumber = 14;  // $$$+
        private const   int SRPActivityIdFieldNumber = 15;  // $$$+
  

        private const int PAYMENTGRIDVIEWROWHEIGHTESTIMATE = 48;

        private bool _bBlockGridBindingForGridControlPostback = false;

        public ContractPaymentsNational()
            : base( DocumentEditorTypes.Contract )
        {
        }

        // this fixes issues with gridview edit save or cancel not redrawing out of edit mode
        protected new void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( Page.IsPostBack != true )
            {
                BindPaymentGridView();
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

            PaymentEventTarget();

            LoadNonFormViewControls();

            if( Page.IsPostBack == false )
            {
                SetPaymentGridViewSelectedItem( 0, true );
                BindPaymentGridView();
            }
            else
            {
                RestorePaymentGridViewSelectedItem();
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

        private void PaymentEventTarget()
        {
            _bBlockGridBindingForGridControlPostback = false;

            string controlName = Request.Form[ "__EVENTTARGET" ];

            if( controlName != null )
            {
                // set up to block binding if postback is from grid control
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$PaymentGridView$ctl02$ )
                if( controlName.Contains( "yearQuarterDropDownList" ) == true ||
                    controlName.Contains( "paymentAmountTextBox" ) == true ||
                    controlName.Contains( "submissionDateTextBox" ) == true ||
                    controlName.Contains( "submittedByTextBox" ) == true ||
                    controlName.Contains( "paymentMethodDropDownList" ) == true ||
                    controlName.Contains( "paymentSourceTextBox" ) == true ||
                    controlName.Contains( "transactionIdTextBox" ) == true ||
                    controlName.Contains( "trackingIdTextBox" ) == true ||
                    controlName.Contains( "depositNumberTextBox" ) == true ||
                    controlName.Contains( "debitVoucherNumberTextBox" ) == true ||
                    controlName.Contains( "checkNumberTextBox" ) == true ||
                    controlName.Contains( "settlementDateTextBox" ) == true ||
                    controlName.Contains( "paymentCommentsTextBox" ) == true ||
                    controlName.Contains( "RemovePaymentButton" ) == true )
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

            PaymentsNationalHeaderFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            PaymentsNationalHeaderFormView.DataKeyNames = new string[] { "ContractId" };
        }

        
        protected void ClearSessionVariables()
        {
            Session[ "PaymentGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedSRPActivityId" ] = null;
            Session[ "LastInsertedSRPActivityId" ] = null;
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

            PaymentsNationalHeaderFormView.DataBind();
 
            // note form view controls are not yet created here
        }

        protected void LoadNonFormViewControls()
        {
            LoadContractNationalPayments();

            PaymentGridView.DataSource = NationalPaymentDataSource;
        }


        protected void BindPaymentGridView()
        {
            try
            {
                // bind if postback not due to grid edit
                if( BlockGridBindingForGridControlPostback == false )
                    PaymentGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        //protected void PaymentsNationalHeaderFormView_OnChange( object sender, EventArgs e )
        //{
        //    SetDirtyFlag( "PaymentsNationalHeaderFormView" );
        //}

        protected void PaymentGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedPaymentId = -1;
            int itemIndex = -1;

            if( e.CommandName.CompareTo( "EditPayment" ) == 0 )
            {

                string commandArgument = e.CommandArgument.ToString();
                string[] argumentList = commandArgument.Split( new char[] { ',' } );

                itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                selectedPaymentId = int.Parse( argumentList[ 1 ].ToString() );

                HighlightPaymentRow( itemIndex );

                InitiateEditModeForPayment( itemIndex );

                // allow the postback to occur 
                PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else
            {
                if( e.CommandName.CompareTo( "SavePayment" ) == 0 )
                {

                    string commandArgument = e.CommandArgument.ToString();
                    string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                    itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                    selectedPaymentId = int.Parse( argumentList[ 1 ].ToString() );

                    // validate the item before saving
                    bool bIsItemOk = ValidatePaymentBeforeUpdate( PaymentGridView, itemIndex, selectedPaymentId, false );

                    if( bIsItemOk == true )
                    {
                        // is this an insert or an update
                        int newOrUpdatedRowIndex = -1;

                        if( PaymentGridView.InsertRowActive == true )
                        {
                            newOrUpdatedRowIndex = InsertPayment( PaymentGridView, itemIndex );
                        }
                        else
                        {
                            newOrUpdatedRowIndex = UpdatePayment( PaymentGridView, itemIndex );
                        }

                        HighlightPaymentRow( newOrUpdatedRowIndex );

                        // allow the postback to occur 
                        PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
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
                        selectedPaymentId = int.Parse( argumentList[ 1 ].ToString() );

                        CancelEdit( itemIndex );
                    }
                    else
                    {
                        if( e.CommandName.CompareTo( "RemovePayment" ) == 0 )
                        {
                            string commandArgument = e.CommandArgument.ToString();
                            string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                            itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                            selectedPaymentId = int.Parse( argumentList[ 1 ].ToString() );

                            bool bContinueWithDelete = false;

                            bContinueWithDelete = GetConfirmationMessageResults();

                            if( bContinueWithDelete == true )
                            {
                                int newRowIndex = DeletePayment( PaymentGridView, itemIndex );

                                HighlightPaymentRow( newRowIndex );

                                // allow the postback to occur 
                                PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                            }
                        }
                    }
                }
            }
        }

        public override string GetValidationGroupName()
        {
            return ( "ContractPaymentsNational" );
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


        protected void PaymentsNationalHeaderFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "PaymentsNationalHeaderFormView" );
            bool bEnabled = documentControlPresentation.IsFormViewEnabled( "PaymentsNationalHeaderFormView" );

            PaymentsNationalHeaderFormView.Visible = bVisible;
            PaymentsNationalHeaderFormView.Enabled = bEnabled;

            if( bVisible == true && bEnabled == true )
            {
                EnableDisablePaymentEditControls();
            }
        }

        protected void PaymentNationalGridPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "PaymentNationalGridPanel" );

            PaymentNationalGridPanel.Visible = bVisible;
            PaymentNationalGridPanel.Enabled = documentControlPresentation.IsControlEnabled( "PaymentNationalGridPanel" );
        }

        // disable add button and Payment box
        protected void EnableDisablePaymentEditControls()
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument != null )
            {
                // allow Payments to be added even if document not active
            //    bool bActive = ( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Active ) ? true : false;
                
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.Checks ) != true )  // $$$ currently, only checks are in use as an access point
                {
                    Button addPaymentButton = ( Button )PaymentsNationalHeaderFormView.FindControl( "AddPaymentButton" );

                    if( addPaymentButton != null )
                    { 
                        addPaymentButton.Enabled = false;
                    }
                } 
            } 
        }

        protected void PaymentGridView_RowDataBound( object sender,  GridViewRowEventArgs e )
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
                        Button removePaymentButton = null;
                        removePaymentButton = ( Button )e.Row.FindControl( "RemovePaymentButton" );

                        if( removePaymentButton != null )
                        { 
                            removePaymentButton.OnClientClick = "presentConfirmationMessage( 'Permanently delete the selected Payment from this contract?' );";
                        }

                        // currently allowing edit of Payments even if document has expired
                        if( currentDocument != null )
                        { 
                            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.Checks ) != true ) 
                            { 
                                if( removePaymentButton != null )
                                { 
                                    removePaymentButton.Enabled = false;
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
                        yearQuarterDropDownList.DataSource = NationalPaymentDateDataSource;
                        yearQuarterDropDownList.DataBind();

                        List<string> paymentMethodList = new List<string>();
                        paymentMethodList.Add( "EFT" );
                        paymentMethodList.Add( "ACH" );
                        paymentMethodList.Add( "CHECK" );

                        DropDownList paymentMethodDropDownList = ( DropDownList )e.Row.FindControl( "paymentMethodDropDownList" );
                        paymentMethodDropDownList.DataSource = paymentMethodList;
                        paymentMethodDropDownList.DataBind();

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

        protected void PaymentGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {
        }

        protected void PaymentGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForPayment( e.NewEditIndex );  // added this to match drug item grid handling 
        }

        protected void PaymentGridView_RowInserting( object sender, GridViewInsertEventArgs e )
        {
        }

        protected void PaymentGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            CancelEdit( e.RowIndex );
        }

        protected void CancelEdit( int rowIndex )
        {
            int cancelIndex = rowIndex;
            bool bInserting = PaymentGridView.InsertRowActive;
            if( bInserting == true ) 
            { 
                PaymentGridView.InsertRowActive = false; // cancels insert ( if inserting )

                AddingPaymentRecord = false; //_withAddPaymentParameter.DefaultValue = "false";
                PaymentGridView.EditIndex = -1;
                BindPaymentGridView();

                // enable appropriate buttons for the selected row
                SetEnabledPaymentControlsDuringEdit( PaymentGridView, cancelIndex, true );

                EnableControlsForPaymentEditMode( true );

                HighlightPaymentRow( 0 );   // $$$ should this be cancelIndex instead of 0 like other cancel functions

                // allow the postback to occur 
                PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            } 
            else  // editing existing row
            {

                PaymentGridView.EditIndex = -1; // cancels the edit
                BindPaymentGridView();

                // enable appropriate buttons for the selected row
                SetEnabledPaymentControlsDuringEdit( PaymentGridView, cancelIndex, true );

                EnableControlsForPaymentEditMode( true );

                HighlightPaymentRow( cancelIndex );

                // allow the postback to occur 
                PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            } 

        }

        protected void PaymentGridView_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {
        }
        protected void PaymentGridView_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {
        }

        protected void PaymentGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id field
            if( e.Row.Cells.Count > SRPActivityIdFieldNumber ) 
            { 
                e.Row.Cells[ SRPActivityIdFieldNumber ].Visible = false;
            }
        }

        protected void PaymentGridView_OnSelectedIndexChanged( object sender, EventArgs e )
        {
        }
        protected void PaymentGridView_PreRender( object sender, EventArgs e )
        {
        }

        protected void PaymentGridView_Init( object sender, EventArgs e )
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

        protected void AddNewPaymentButton_OnClick( object sender, EventArgs e )
        {
            PaymentGridView.Insert();

            AddingPaymentRecord = true; // _withAddPaymentParameter.DefaultValue = "true";

            BindPaymentGridView();

            InitiateEditModeForPayment( 0 );

            PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() ); 
        }

        protected void InitiateEditModeForPayment( int editIndex )
        {
            PaymentGridView.EditIndex = editIndex;

            // select the edited item also
            if( PaymentGridView.InsertRowActive == true ) 
            { 
                SetPaymentGridViewSelectedItem( editIndex, true ); // scroll to new row
            } 
            else 
            {
                SetPaymentGridViewSelectedItem( editIndex, false );
            }

            PaymentGridView.DataBind(); 

            // disable appropriate buttons for the selected row
            SetEnabledPaymentControlsDuringEdit( PaymentGridView, editIndex, false );

            // disable the non-edit controls before going into edit mode
            EnableControlsForPaymentEditMode( false );

        }

#region "currentselectedPaymentrow"


        protected void SetPaymentGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {

            if( selectedItemIndex < PaymentGridView.Rows.Count ) 
            { 

                // save for postback
                Session[ "PaymentGridViewSelectedIndex" ] = selectedItemIndex;

                // set the row selected
                PaymentGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true ) 
                { 
                    ScrollToSelectedItem();
                }

                PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            } 
        }

        protected void ScrollToSelectedItem()
        {

            int rowIndex = PaymentGridView.SelectedIndex;   // took out + 1 here to make header visible on add

            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( PAYMENTGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( PAYMENTGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string restorePaymentGridSelectionScript = String.Format( "RestorePaymentGridSelection( {0},{1} );", rowPosition, rowIndex );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestorePaymentGridSelectionScript", restorePaymentGridSelectionScript, true ); // runs after controls established
        }

        protected void HighlightPaymentRow( int itemIndex )
        { 
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1; 

            if( PaymentGridView.HasData() == true ) 
            {
                GridViewRow row = PaymentGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate ) 
                {
                    highlightedRowOriginalColor = PaymentGridView.AlternatingRowStyle.BackColor.ToString();
                } 
                else 
                {
                    highlightedRowOriginalColor = PaymentGridView.RowStyle.BackColor.ToString();
                } 
                
                string preserveHighlightingScript = String.Format( "setPaymentHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreservePaymentHighlightingScript", preserveHighlightingScript, true );
            } 
        }

        protected void RestorePaymentGridViewSelectedItem()
        {
            if( Session[ "PaymentGridViewSelectedIndex" ] == null )
                return;
            
            PaymentGridView.SelectedIndex = int.Parse( Session[ "PaymentGridViewSelectedIndex" ].ToString() );

            //ScrollToSelectedItem();

            //PaymentUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void SetEnabledPaymentControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )   
        {
            gv.SetEnabledControlsForCell( rowIndex, PaymentRemoveButtonFieldNumber, bEnabled ); // remove button

            gv.SetVisibleControlsForCell( rowIndex, PaymentEditButtonFieldNumber, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, PaymentEditButtonFieldNumber, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, PaymentEditButtonFieldNumber, "CancelButton", !bEnabled );
        }


        // disable non-edit controls before going into edit mode
        protected void EnableControlsForPaymentEditMode( bool bEnabled )
        {
            Button addPaymentButton  = ( Button )PaymentsNationalHeaderFormView.FindControl( "AddPaymentButton" );

            if( addPaymentButton != null )
            { 
                addPaymentButton.Enabled = bEnabled;
            } 
        }

#endregion 
  
        private bool ValidatePaymentBeforeUpdate( GridView paymentGridView, int itemIndex, int selectedPaymentId, bool bIsShortSave )
        {
            bool bIsValid = true;   

            int quarterId = -1;
            decimal paymentAmount = 0;

    
            string yearQuarterIdString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, PaymentYearQuarterFieldNumber, 0, false, "yearQuarterDropDownList" );
            string paymentAmountString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, PaymentAmountFieldNumber, 0, false, "PaymentAmountTextBox" );

            string submissionDateString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, SubmissionDateFieldNumber, 0, false, "submissionDateTextBox" );
            string submittedByUserNameString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, SubmittedByFieldNumber, 0, false, "submittedByTextBox" );
            string paymentMethodString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, PaymentMethodFieldNumber, 0, false, "paymentMethodDropDownList" );
            string paymentSourceString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, PaymentSourceFieldNumber, 0, false, "paymentSourceTextBox" );
            string transactionIdString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, TransationIdFieldNumber, 0, false, "transactionIdTextBox" );
            string payGovTrackingIdString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, TrackingIdFieldNumber, 0, false, "trackingIdTextBox" );
            string paymentDepositTicketNumberString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, DepositNumberFieldNumber, 0, false, "depositNumberTextBox" );
            string paymentDebitVoucherNumbeString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, DebitVoucherNumberFieldNumber, 0, false, "debitVoucherNumberTextBox" );
            string checkNumberString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, CheckNumberFieldNumber, 0, false, "checkNumberTextBox" );
            string paymentSettlementDateString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, SettlementDateFieldNumber, 0, false, "settlementDateTextBox" );
            string paymentCommentsString = paymentGridView.GetStringValueFromSelectedControl( itemIndex, PaymentCommentsFieldNumber, 0, false, "PaymentCommentsTextBox" );


            if( int.TryParse( yearQuarterIdString, out quarterId ) == false )
            {
                bIsValid = false;
                AppendValidationError( "A year/quarter must be selected.", bIsShortSave );
            }

            if( Decimal.TryParse( paymentAmountString, out paymentAmount ) == false ) 
            {
                bIsValid = false;
                AppendValidationError( "A valid payment amount is required.", bIsShortSave );
            }

            DateTime submissionDate;
            if( submissionDateString.Trim().Length > 0 )
            {
                if( DateTime.TryParseExact( submissionDateString, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out submissionDate ) == false )
                {                
                    bIsValid = false;
                    AppendValidationError( "Submission date is not a valid date format.", bIsShortSave );
                }
                else 
                {
                    try
                    {
                        System.Data.SqlTypes.SqlDateTime sqlDateTimeObj = new System.Data.SqlTypes.SqlDateTime( submissionDate );
                    }
                    catch( System.Data.SqlTypes.SqlTypeException ex )
                    {
                        bIsValid = false;
                        AppendValidationError( "Submission date is not a valid date format.", bIsShortSave );
                    }
                }
            }

            DateTime settlementDate;
            if( paymentSettlementDateString.Trim().Length > 0 )
            {
                if( DateTime.TryParseExact( paymentSettlementDateString, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out settlementDate ) == false )
                {                
                    bIsValid = false;
                    AppendValidationError( "Settlement date is not a valid date format.", bIsShortSave );
                }
                else 
                {
                    try
                    {
                        System.Data.SqlTypes.SqlDateTime sqlDateTimeObj = new System.Data.SqlTypes.SqlDateTime( settlementDate );
                    }
                    catch( System.Data.SqlTypes.SqlTypeException ex )
                    {
                        bIsValid = false;
                        AppendValidationError( "Settlement date is not a valid date format.", bIsShortSave );
                    }
                }
            }
           
            if( bIsValid == true )
            {
                if( paymentAmount == 0 && paymentCommentsString.Length == 0 )
                {
                    bIsValid = false;
                    AppendValidationError( "If Payment amount is zero then comment entry is required.", bIsShortSave );
                }
            }
 
            return( bIsValid );
        }
    

        protected void RemovePaymentButton_DataBinding( object sender,  EventArgs e )
        {
            Button removePaymentButton  = ( Button )sender;
            if( removePaymentButton != null )
            { 
                CMGlobals.MultilineButtonText( removePaymentButton, new String[] { "Remove", "Payment" } );
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

        private int InsertPayment( GridView paymentGridView, int rowIndex ) 
        {
            int insertedRowIndex = 0;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs  = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            NationalPaymentContractNumberParameterValue = currentDocument.ContractNumber;
            PaymentContractIdParameterValue = currentDocument.ContractId.ToString();
            UserLoginParameterValue = bs.UserInfo.LoginName;

            PaymentQuarterIdParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentYearQuarterFieldNumber, 0, false, "yearQuarterDropDownList" );
            PaymentAmountParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentAmountFieldNumber, 0, false, "paymentAmountTextBox" );

            SubmissionDateParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, SubmissionDateFieldNumber, 0, false, "submissionDateTextBox" );
            SubmittedByUserNameParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, SubmittedByFieldNumber, 0, false, "submittedByTextBox" );
            PaymentMethodParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentMethodFieldNumber, 0, false, "paymentMethodDropDownList" );
            PaymentSourceParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentSourceFieldNumber, 0, false, "paymentSourceTextBox" );
            TransactionIdParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, TransationIdFieldNumber, 0, false, "transactionIdTextBox" );
            PayGovTrackingIdParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, TrackingIdFieldNumber, 0, false, "trackingIdTextBox" );
            PaymentDepositTicketNumberParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, DepositNumberFieldNumber, 0, false, "depositNumberTextBox" );
            PaymentDebitVoucherNumberParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, DebitVoucherNumberFieldNumber, 0, false, "debitVoucherNumberTextBox" );
            PaymentCheckNumberParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, CheckNumberFieldNumber, 0, false, "checkNumberTextBox" );
            PaymentSettlementDateParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, SettlementDateFieldNumber, 0, false, "settlementDateTextBox" );
            PaymentCommentsParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentCommentsFieldNumber, 0, false, "PaymentCommentsTextBox" );
            SRPActivityIdParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, SRPActivityIdFieldNumber, 0, false, "SRPActivityIdLabel" );

            try
            {
                NationalPaymentDataSource.Insert();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }


            paymentGridView.InsertRowActive = false; // done with insert
            paymentGridView.EditIndex = -1; // done with edit
            AddingPaymentRecord = false; // _withAddPaymentParameter.DefaultValue = "false";   // no extra row
            paymentGridView.DataBind(); //  bind with new row

            if( Session[ "LastInsertedSRPActivityId" ] != null )
            { 
                int newPaymentId = ( int )Session[ "LastInsertedSRPActivityId" ];
                insertedRowIndex = paymentGridView.GetRowIndexFromId( newPaymentId, 0 );

                SetPaymentGridViewSelectedItem( insertedRowIndex, true );

                // bind to select
                paymentGridView.DataBind();
            } 

            // enable appropriate buttons for the selected row
            SetEnabledPaymentControlsDuringEdit( paymentGridView, insertedRowIndex, true );

            EnableControlsForPaymentEditMode( true );

            return( insertedRowIndex );
        }
    


        private int UpdatePayment( GridView paymentGridView, int rowIndex )
        {
            int updatedRowIndex = -1;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs  = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            NationalPaymentContractNumberParameterValue = currentDocument.ContractNumber;
            PaymentContractIdParameterValue = currentDocument.ContractId.ToString();
            UserLoginParameterValue = bs.UserInfo.LoginName;

            SRPActivityIdParameterValue = paymentGridView.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

            PaymentQuarterIdParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentYearQuarterFieldNumber, 0, false, "yearQuarterDropDownList" );
            PaymentAmountParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentAmountFieldNumber, 0, false, "PaymentAmountTextBox" );

            SubmissionDateParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, SubmissionDateFieldNumber, 0, false, "submissionDateTextBox" );
            SubmittedByUserNameParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, SubmittedByFieldNumber, 0, false, "submittedByTextBox" );
            PaymentMethodParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentMethodFieldNumber, 0, false, "paymentMethodDropDownList" );
            PaymentSourceParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentSourceFieldNumber, 0, false, "paymentSourceTextBox" );
            TransactionIdParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, TransationIdFieldNumber, 0, false, "transactionIdTextBox" );
            PayGovTrackingIdParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, TrackingIdFieldNumber, 0, false, "trackingIdTextBox" );
            PaymentDepositTicketNumberParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, DepositNumberFieldNumber, 0, false, "depositNumberTextBox" );
            PaymentDebitVoucherNumberParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, DebitVoucherNumberFieldNumber, 0, false, "debitVoucherNumberTextBox" );
            PaymentCheckNumberParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, CheckNumberFieldNumber, 0, false, "checkNumberTextBox" );
            PaymentSettlementDateParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, SettlementDateFieldNumber, 0, false, "settlementDateTextBox" );
            PaymentCommentsParameterValue = paymentGridView.GetStringValueFromSelectedControl( rowIndex, PaymentCommentsFieldNumber, 0, false, "PaymentCommentsTextBox" );
  
            try

            {
                NationalPaymentDataSource.Update();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }

            paymentGridView.EditIndex = -1; // done with edit
            paymentGridView.DataBind();

            if( Session[ "LastUpdatedSRPActivityId" ] != null )
            { 
                int lastUpdatedPaymentId = int.Parse( Session[ "LastUpdatedSRPActivityId" ].ToString() );
                updatedRowIndex = paymentGridView.GetRowIndexFromId( lastUpdatedPaymentId, 0 );

                SetPaymentGridViewSelectedItem( updatedRowIndex, false );

                // bind to select
                paymentGridView.DataBind();
            } 

            // enable appropriate buttons for the selected row
            SetEnabledPaymentControlsDuringEdit( paymentGridView, updatedRowIndex, true );

            EnableControlsForPaymentEditMode( true );

            return( updatedRowIndex );

        }

        private int DeletePayment( GridView paymentGridView, int rowIndex )
        {
            int updatedRowIndex = -1;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            NationalPaymentContractNumberParameterValue = currentDocument.ContractNumber;
            PaymentContractIdParameterValue = currentDocument.ContractId.ToString();
            UserLoginParameterValue = bs.UserInfo.LoginName;   // $$$ this may be redundant. the guid is added in DocumentDataSource event and the SP only takes the guid

            SRPActivityIdParameterValue = paymentGridView.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();


            try
            {
                NationalPaymentDataSource.Delete();
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

            SetPaymentGridViewSelectedItem( updatedRowIndex, false );

            // bind to select
            paymentGridView.DataBind();

            return( updatedRowIndex );
        }
    


 

#endregion

#region "PaymentDateSelection"


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




        #endregion "PaymentDateSelection"


        #region "PaymentMethod"






        protected void paymentMethodDropDownList_DataBound( object sender, EventArgs e )
        {
            DropDownList paymentMethodDropDownList  = ( DropDownList )sender;
            GridViewRow gridViewRow  = ( GridViewRow )paymentMethodDropDownList.NamingContainer;
            string selectedPaymentMethod = "";
          
            ListItem customListItem  = new ListItem();

            if( gridViewRow != null )
            { 
                DataRowView dataRowView  = ( DataRowView )gridViewRow.DataItem;
                selectedPaymentMethod = dataRowView[ "PaymentMethod" ].ToString();

                if( selectedPaymentMethod != null )
                {                     
                    ListItem listItem  = paymentMethodDropDownList.Items.FindByValue( selectedPaymentMethod );
                    if( listItem != null )
                    {
                        listItem.Selected = true;
                    }
                    
                } 
            } 
        }

        protected void paymentMethodDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList paymentMethodDropDownList  = ( DropDownList )sender;
            ListItem selectedItem;

            selectedItem = paymentMethodDropDownList.SelectedItem;

            string selectedPaymentMethod;

            selectedPaymentMethod = selectedItem.Value;
        }










        #endregion "PaymentMethod"


    }
}