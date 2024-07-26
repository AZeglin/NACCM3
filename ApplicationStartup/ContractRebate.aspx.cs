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
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
   
    /* this was on the 2 textboxes onkeydown="trapBackspace(event); */
    public partial class ContractRebate : BaseDocumentEditorPage
    {
        public ContractRebate()
            : base( DocumentEditorTypes.Contract )
        {
        }

        private const int RebateViewTextButtonFieldNumber = 0; // $$$+
        private const  int RebateEditButtonFieldNumber = 1;  // $$$+
        private const  int RebateStartYearQuarterFieldNumber = 2; //$$$+
        private const  int RebateEndYearQuarterFieldNumber = 3; //$$$+
        private const  int RebatePercentOfSalesFieldNumber = 4; //$$$+
        private const  int RebateThresholdFieldNumber = 5; //$$$+
        private const  int RebateClauseNameFieldNumber = 6; //$$$+
        private const  int RebateModifiedByFieldNumber = 7; //$$$+
        private const  int RebateLastModificationDateFieldNumber = 8; //$$$+

        private const   int RebateRemoveButtonFieldNumber = 9;  // $$$+
        private const   int RebateTermIdFieldNumber = 10;  // $$$+
        private const   int RebatesStandardRebateTermIdFieldNumber = 11;  // $$$+

        private const  string  RebatePercentTagString = "{percent}";
        private const  string RebateThresholdTagString = "{threshold}";

        private const int REBATEGRIDVIEWROWHEIGHTESTIMATE = 48;

        private bool _bBlockGridBindingForGridControlPostback = false;

        // this fixes issues with gridview edit save or cancel not redrawing out of edit mode
        protected new void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            if( Page.IsPostBack != true )
            {
                BindRebateGridView();
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
                SetRebateClauseReadOnly( true );   // initially read-only
                SetCustomStartDateReadOnly( true );
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );

            CheckEventTarget();

            LoadNonFormViewControls();

            if( Page.IsPostBack == false )
            {
                SetRebateGridViewSelectedItem( 0, true );
                BindRebateGridView();
      //          RefreshRebateFooterData( 0, false, false );
            }
            else
            {
                RestoreRebateGridViewSelectedItem();
      //          BindRebateGridView();
      //          RefreshRebateFooterData( 0, false, false );
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
                // ( prefix = ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$RebateGridView$ctl02$ )
                if( controlName.Contains( "percentOfSalesTextBox" ) == true ||
                    controlName.Contains( "rebateThresholdTextBox" ) == true ||
                    controlName.Contains( "startYearQuarterDropDownList" ) == true ||
                    controlName.Contains( "endYearQuarterDropDownList" ) == true ||
                    controlName.Contains( "rebateClauseNameDropDownList" ) == true )
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

            RebatesHeaderFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            RebatesHeaderFormView.DataKeyNames = new string[] { "ContractId" };
            
            // this source is not used, but must bind to something
            RebatesFooterDateFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            RebatesFooterDateFormView.DataKeyNames = new string[] { "ContractId" };

            RebatesFooterClauseFormView.DataSource = DataRelay.EditedDocumentDataSourceFront;
            RebatesFooterClauseFormView.DataKeyNames = new string[] { "ContractId" };

        }

        
        protected void ClearSessionVariables()
        {
            Session[ "RebateDataSource" ] = null;
            Session[ "RebateDateDataSource" ] = null;
            Session[ "StandardClauseNameDataSource" ] = null;
            Session[ "RebateGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedRebateId" ] = null;
            Session[ "LastInsertedRebateId" ] = null;
            Session[ "CustomRebateTextForCurrentRow" ] = null;
            Session[ "StandardClauseNameDataSet" ] = null;
            Session[ "CustomStartDateForCurrentRow" ] = null;
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

            RebatesHeaderFormView.DataBind();

            BindRebateClause();
 
            // note form view controls are not yet created here
        }

        protected void BindRebateClause()
        {
            RebatesFooterDateFormView.DataBind();
            RebatesFooterClauseFormView.DataBind();
        }

        protected void LoadNonFormViewControls()
        {
            LoadContractRebates();

            RebateGridView.DataSource = RebateDataSource;
        }


        protected void BindRebateGridView()
        {
            try
            {
                // bind if postback not due to grid edit
                if( BlockGridBindingForGridControlPostback == false )
                    RebateGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        protected void RebatesHeaderFormView_OnChange( object sender, EventArgs e )
        {
   //         SetDirtyFlag( "RebatesHeaderFormView" );
        }

        protected void RebateGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedRebateId = -1;
            int itemIndex = -1;

            if( e.CommandName.CompareTo( "ViewRebateText" ) == 0 )
            {
                string commandArgument = e.CommandArgument.ToString();
                string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                itemIndex = int.Parse( argumentList[ 0 ] );
                selectedRebateId = int.Parse( argumentList[ 1 ] );

                HighlightRebateRow( itemIndex );

                RefreshRebateFooterData( itemIndex, false, false );

                // allow the postback to occur 
                RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else
            {
                if( e.CommandName.CompareTo( "EditRebate" ) == 0 )
                {

                    string commandArgument = e.CommandArgument.ToString();
                    string[] argumentList = commandArgument.Split( new char[] { ',' } );

                    itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                    selectedRebateId = int.Parse( argumentList[ 1 ].ToString() );

                    HighlightRebateRow( itemIndex );

                    RefreshRebateFooterData( itemIndex, true, true );
  
                    InitiateEditModeForRebate( itemIndex );

                    // allow the postback to occur 
                    RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                }
                else
                {
                    if( e.CommandName.CompareTo( "SaveRebate" ) == 0 )
                    {

                        string commandArgument = e.CommandArgument.ToString();
                        string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                        itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                        selectedRebateId = int.Parse( argumentList[ 1 ].ToString() );

                        // validate the item before saving
                        bool bIsItemOk = ValidateRebateBeforeUpdate( RebateGridView, itemIndex, selectedRebateId, false );

                        if( bIsItemOk == true )
                        {
                            // is this an insert or an update
                            int newOrUpdatedRowIndex = -1;

                            if( RebateGridView.InsertRowActive == true )
                            {
                                newOrUpdatedRowIndex = InsertRebate( RebateGridView, itemIndex );
                            }
                            else
                            {
                                newOrUpdatedRowIndex = UpdateRebate( RebateGridView, itemIndex );
                            }

                            HighlightRebateRow( newOrUpdatedRowIndex );

                            RefreshRebateFooterData( newOrUpdatedRowIndex, true, false ); 

                            // allow the postback to occur 
                            RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
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
                            selectedRebateId = int.Parse( argumentList[ 1 ].ToString() );

                            CancelEdit( itemIndex );
                        }
                        else
                        {
                            if( e.CommandName.CompareTo( "RemoveRebate" ) == 0 )
                            {
                                string commandArgument = e.CommandArgument.ToString();
                                string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                                itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                                selectedRebateId = int.Parse( argumentList[ 1 ].ToString() );

                                bool bContinueWithDelete = false;

                                bContinueWithDelete = GetConfirmationMessageResults();

                                if( bContinueWithDelete == true )
                                {
                                    int newRowIndex = DeleteRebate( RebateGridView, itemIndex );

                                    HighlightRebateRow( newRowIndex );

                                    RefreshRebateFooterData( newRowIndex, true, false );

                                    // allow the postback to occur 
                                    RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                                }
                            }
                        }
                    }
                }
            }
        }

        public override string GetValidationGroupName()
        {
            return ( "ContractRebate" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            //if( offerDataRelay.EditedOfferContentFront.VendorName.Length == 0 )
            //{
            //    ErrorMessage = "Vendor name is required.";
            //    bSuccess = false;
            //}

            return ( bSuccess );
        }


        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            DataRelay dataRelay = ( DataRelay )dataRelayInterface;

            bool bSuccess = true;

            // Rebates Tab
            CheckBox RebateRequiredCheckBox = ( CheckBox )RebatesHeaderFormView.FindControl( "RebateRequiredCheckBox" );
            if( RebateRequiredCheckBox != null )
            {
                if( RebateRequiredCheckBox.Checked == true )
                {
                    dataRelay.EditedDocumentContentFront.RebateRequired = true;
                }
                else
                {
                    dataRelay.EditedDocumentContentFront.RebateRequired = false;
                }
            }

            return ( bSuccess );
        }


        protected void RebatesHeaderFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "RebatesHeaderFormView" );

            RebatesHeaderFormView.Visible = bVisible;
            RebatesHeaderFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "RebatesHeaderFormView" );

            if( bVisible == true )
            {
                EnableDisableRebateEditControls();
            }
        }

        protected void RebateGridPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "RebateGridPanel" );

            RebateGridPanel.Visible = bVisible;
            RebateGridPanel.Enabled = documentControlPresentation.IsControlEnabled( "RebateGridPanel" );
        }

        protected void RebatesFooterDateFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "RebatesFooterDateFormView" );

            RebatesFooterDateFormView.Visible = bVisible;
            RebatesFooterDateFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "RebatesFooterDateFormView" );

            if( bVisible == true )
            {
                EnableDisableRebateFooterDateControls();   // this is req to enable editing by default after add
            }
        }

        protected void RebatesFooterClauseFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "RebatesFooterClauseFormView" );

            RebatesFooterClauseFormView.Visible = bVisible;
            RebatesFooterClauseFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "RebatesFooterClauseFormView" );

            if( bVisible == true )
            {
                EnableDisableRebateFooterClauseControls();   // this is req to enable editing by default after add
            }
        }

        protected void RebatesFooterDateFormView_OnDataBinding( object sender, EventArgs e )
        {
            
        }

        protected void RebatesFooterClauseFormView_OnDataBinding( object sender, EventArgs e )
        {

        }

        // disable add button and check box
        protected void EnableDisableRebateEditControls()
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            CheckBox rebateRequiredCheckBox  = ( CheckBox )RebatesHeaderFormView.FindControl( "RebateRequiredCheckBox" );
            bool bRebateRequired = rebateRequiredCheckBox.Checked;

            if( currentDocument != null )
            { 
                if( ( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.RebateTerms ) != true ) || bRebateRequired == false ) 
                {

                    Button addRebateButton = ( Button )RebatesHeaderFormView.FindControl( "AddRebateButton" );

                    if( addRebateButton != null )
                    { 
                        addRebateButton.Enabled = false;
                    } 

                } 

                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.RebateRequired ) != true ) 
                { 
                    if( rebateRequiredCheckBox != null )
                    { 
                        rebateRequiredCheckBox.Enabled = false;
                    } 
                } 
            } 
        }

        protected void EnableDisableRebateFooterDateControls()
        {
            if( AddingRebateRecord == true )
            {
                // init the rebate text
                ClearCustomDate( false );
            }
        }

        protected void EnableDisableRebateFooterClauseControls()
        {
            if( AddingRebateRecord == true )
            {
                // init the rebate text
                ClearRebateText( false, true );  // init with "Enter custom rebate text." since initial clause selected is custom
               
            }
        }
 
        protected void RebateRequiredCheckBox_OnCheckedChanged( object obj, EventArgs e )
        {
            CheckBox rebateRequiredCheckBox = ( CheckBox )obj; 
            bool bRebateRequired = rebateRequiredCheckBox.Checked;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument != null )
            {

                Button addRebateButton = ( Button )RebatesHeaderFormView.FindControl( "AddRebateButton" );

                if( ( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.RebateTerms ) != true ) || bRebateRequired == false ) 
                { 
                    if( addRebateButton != null )
                    { 
                        addRebateButton.Enabled = false;
                    } 
                } 
                else 
                {
                    if( addRebateButton != null )
                    { 
                        addRebateButton.Enabled = true;
                    } 
                } 
            }

            // save the rebate required checkbox to the front object before binding
            ShortSave();

            // rebind grid to enable/disable edit controls
            BindRebateGridView();
            TriggerContractViewMasterUpdatePanelFromContract();
            
        }

        protected void RebateGridView_RowDataBound( object sender,  GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;
                CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];

                bool bRebateRequired = DataRelay.EditedDocumentDataSourceFront.EditedDocumentContentFront.RebateRequired;

                if( e.Row.RowType == DataControlRowType.DataRow ) 
                { 

                    if( ((( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                        (( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert )) )
                    { 
                        Button removeRebateButton = null;
                        removeRebateButton = ( Button )e.Row.FindControl( "RemoveRebateButton" );

                        if( removeRebateButton != null )
                        { 
                            removeRebateButton.OnClientClick = "presentConfirmationMessage( 'Permanently delete the selected rebate from this contract?' );";
                        }


                        Button viewRebateTextButton = null;
                        viewRebateTextButton = ( Button )e.Row.FindControl( "ViewRebateTextButton" );
                        if( viewRebateTextButton != null )
                        {
                            // colors match .PersonalizedNotificationGridItems and  .PersonalizedNotificationGridAltItems
                            string rowColor = "alt";
                            int odd = 0;
                            Math.DivRem( rowIndex, 2, out odd );
                            if( odd > 0 )
                            {
                                rowColor = "norm";
                            }
                            viewRebateTextButton.Enabled = true;
                            string windowHighlightCommand = string.Format( "resetRebateHighlighting( 'RebateGridView', {0}, '{1}' );", rowIndex, rowColor );
                            viewRebateTextButton.Attributes.Add( "onclick", windowHighlightCommand );
                        }

                        if( currentDocument != null )
                        {
                            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.RebateTerms ) != true || bRebateRequired == false )
                            {
                                if( removeRebateButton != null )
                                {
                                    removeRebateButton.Enabled = false;
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
                        DropDownList startYearQuarterDropDownList  = ( DropDownList )e.Row.FindControl( "startYearQuarterDropDownList" );
                        startYearQuarterDropDownList.DataSource = RebateDateDataSource;
                        startYearQuarterDropDownList.DataBind();

                        DropDownList endYearQuarterDropDownList  = ( DropDownList )e.Row.FindControl( "endYearQuarterDropDownList" );
                        endYearQuarterDropDownList.DataSource = RebateDateDataSource;
                        endYearQuarterDropDownList.DataBind();

                        DropDownList rebateClauseNameDropDownList  = ( DropDownList )e.Row.FindControl( "rebateClauseNameDropDownList" );
                        rebateClauseNameDropDownList.DataSource = RebateStandardClauseNameDataSource;
                        rebateClauseNameDropDownList.DataBind();

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

        protected void RebateGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {
        }

        protected void RebateGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForRebate( e.NewEditIndex );  // added this to match drug item grid handling 
        }

        protected void RebateGridView_RowInserting( object sender, GridViewInsertEventArgs e )
        {
        }

        protected void RebateGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            CancelEdit( e.RowIndex );
        }

        protected void CancelEdit( int rowIndex )
        {
            int cancelIndex = rowIndex;
            bool bInserting = RebateGridView.InsertRowActive;
            if( bInserting == true ) 
            { 
                RebateGridView.InsertRowActive = false; // cancels insert ( if inserting )

                AddingRebateRecord = false; //_withAddRebateParameter.DefaultValue = "false";
                RebateGridView.EditIndex = -1;
                BindRebateGridView();

                // enable appropriate buttons for the selected row
                SetEnabledRebateControlsDuringEdit( RebateGridView, cancelIndex, true );    

                EnableControlsForRebateEditMode( true );  

                HighlightRebateRow( 0 );
                RefreshRebateFooterData( 0, true, false ); // revert to item zero's clause

                // allow the postback to occur 
                RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            } 
            else  // editing existing row
            {

                RebateGridView.EditIndex = -1; // cancels the edit
                BindRebateGridView();

                // enable appropriate buttons for the selected row
                SetEnabledRebateControlsDuringEdit( RebateGridView, cancelIndex, true );

                EnableControlsForRebateEditMode( true );

                HighlightRebateRow( cancelIndex );
                RefreshRebateFooterData( cancelIndex, false, false );

                // allow the postback to occur 
                RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            } 

        }

        protected void RebateGridView_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {
        }
        protected void RebateGridView_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {
        }

        protected void RebateGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id fields
            if( ( e.Row.Cells.Count > RebateTermIdFieldNumber ) && ( e.Row.Cells.Count > RebatesStandardRebateTermIdFieldNumber ) )
            { 
                e.Row.Cells[ RebateTermIdFieldNumber ].Visible = false;
                e.Row.Cells[ RebatesStandardRebateTermIdFieldNumber ].Visible = false;
            }
        }

        protected void RebateGridView_OnSelectedIndexChanged( object sender, EventArgs e )
        {
        }
        protected void RebateGridView_PreRender( object sender, EventArgs e )
        {
        }

        protected void RebateGridView_Init( object sender, EventArgs e )
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

        protected void AddNewRebateButton_OnClick( object sender, EventArgs e )
        {
            RebateGridView.Insert();

            AddingRebateRecord = true; // _withAddRebateParameter.DefaultValue = "true";

            BindRebateGridView();

            InitiateEditModeForRebate( 0 );

            // save the rebate required checkbox to the front object before binding
            ShortSave();

            // init the rebate text
            ClearRebateText( false, true );
            ClearCustomDate( false );

            BindRebateClause(); 

            RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
 
        }

        protected void InitiateEditModeForRebate( int editIndex )
        {
            RebateGridView.EditIndex = editIndex;

            // clear the most recent custom rebate text
            Session[ "CustomRebateTextForCurrentRow" ] = null;
            Session[ "CustomStartDateForCurrentRow" ] = null;

            // select the edited item also
            if( RebateGridView.InsertRowActive == true ) 
            { 
                SetRebateGridViewSelectedItem( editIndex, true ); // scroll to new row
            } 
            else 
            {
                SetRebateGridViewSelectedItem( editIndex, false );
            }

            RebateGridView.DataBind(); 

            // disable appropriate buttons for the selected row
            SetEnabledRebateControlsDuringEdit( RebateGridView, editIndex, false );

            // disable the non-edit controls before going into edit mode
            EnableControlsForRebateEditMode( false );
        }

#region "currentselectedrebaterow"


        protected void SetRebateGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {

            if( selectedItemIndex < RebateGridView.Rows.Count ) 
            { 

                // save for postback
                Session[ "RebateGridViewSelectedIndex" ] = selectedItemIndex;

                // set the row selected
                RebateGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true ) 
                { 
                    ScrollToSelectedItem();
                }

                RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            } 
        }

        protected void ScrollToSelectedItem()
        {

            int rowIndex = RebateGridView.SelectedIndex + 1;

            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( REBATEGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( REBATEGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string restoreRebateGridSelectionScript = String.Format( "RestoreRebateGridSelection( {0},{1} );", rowPosition, rowIndex );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestoreRebateGridSelectionScript", restoreRebateGridSelectionScript, true ); // runs after controls established
        }

        protected void HighlightRebateRow( int itemIndex )
        { 
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1; 

            if( RebateGridView.HasData() == true ) 
            {
                GridViewRow row = RebateGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate ) 
                {
                    highlightedRowOriginalColor = RebateGridView.AlternatingRowStyle.BackColor.ToString();
                } 
                else 
                {
                    highlightedRowOriginalColor = RebateGridView.RowStyle.BackColor.ToString();
                } 
                
                string preserveHighlightingScript = String.Format( "setRebateHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveRebateHighlightingScript", preserveHighlightingScript, true );
            } 
        }

        protected void RestoreRebateGridViewSelectedItem()
        {
            if( Session[ "RebateGridViewSelectedIndex" ] == null )
                return;
            
            RebateGridView.SelectedIndex = int.Parse( Session[ "RebateGridViewSelectedIndex" ].ToString() );
        }

        protected void SetEnabledRebateControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )   
        {

            gv.SetEnabledControlsForCell( rowIndex, RebateViewTextButtonFieldNumber, bEnabled ); // view button
            gv.SetEnabledControlsForCell( rowIndex, RebateRemoveButtonFieldNumber, bEnabled ); // remove button

            gv.SetVisibleControlsForCell( rowIndex, RebateEditButtonFieldNumber, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, RebateEditButtonFieldNumber, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, RebateEditButtonFieldNumber, "CancelButton", !bEnabled );

            //  SetRebateClauseReadOnly(bEnabled)
        }

        protected void SetRebateClauseReadOnly( bool bReadOnly )
        {
            TextBox rebateClauseTextBox = ( TextBox )RebatesFooterClauseFormView.FindControl( "RebateClauseTextBox" );
            if( rebateClauseTextBox != null )
            {
                if( bReadOnly == true )
                {
                    //rebateClauseTextBox.ReadOnly = true;
                    RebatesFooterClauseFormView.ChangeMode( FormViewMode.ReadOnly );   // this works but need to be able to address the 2 textboxes separately
                    // note: the following were not effective in making the footer fields read only
                    //       RebatesFooterFormView.DefaultMode = FormViewMode.ReadOnly;
                    //       rebateClauseTextBox.Attributes.Add( "ReadOnly", "ReadOnly" );
                    //       rebateClauseTextBox.Enabled = false;
                }
                else
                {
                    RebatesFooterClauseFormView.ChangeMode( FormViewMode.Edit );
                    //rebateClauseTextBox.ReadOnly = false;
                }
            }

            RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() ); 
        }

        protected void SetCustomStartDateReadOnly( bool bReadOnly )
        {
            TextBox customStartDateTextBox  = ( TextBox )RebatesFooterDateFormView.FindControl( "CustomStartDateTextBox" );
            if( customStartDateTextBox != null )
            {
                if( bReadOnly == true )
                {
                   
                    RebatesFooterDateFormView.ChangeMode( FormViewMode.ReadOnly );          
                }
                else
                {
                
                    RebatesFooterDateFormView.ChangeMode( FormViewMode.Edit );            
                }
            }

            RebateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        // disable non-edit controls before going into edit mode
        protected void EnableControlsForRebateEditMode( bool bEnabled )
        {
            Button addRebateButton  = ( Button )RebatesHeaderFormView.FindControl( "AddRebateButton" );

            if( addRebateButton != null )
            { 
                addRebateButton.Enabled = bEnabled;
            } 

           CheckBox rebateRequiredCheckBox = ( CheckBox )RebatesHeaderFormView.FindControl( "RebateRequiredCheckBox" );
           
           if( rebateRequiredCheckBox != null )
           {
               rebateRequiredCheckBox.Enabled = bEnabled;
           }            
        }

#endregion 
        

        protected void RefreshRebateFooterData( int rowIndex, bool bIncludeScroll, bool bIsInEditMode )
        {
            int rebateId;
            decimal rebatePercentOfSales = -1;
            decimal rebateThreshold = -1; 
            
            SetRebateGridViewSelectedItem( rowIndex, bIncludeScroll );

            if( rowIndex < RebateGridView.Rows.Count )
            {

                rebateId = RebateGridView.GetRowIdFromSelectedIndex( rowIndex, 0 );
                string rebatePercentOfSalesString = RebateGridView.GetStringValueFromSelectedIndexForTemplateField( rowIndex, "percentOfSalesLabel" );
                string rebateThresholdString = RebateGridView.GetStringValueFromSelectedIndexForTemplateField( rowIndex, "rebateThresholdLabel" );

                if( decimal.TryParse( rebatePercentOfSalesString, out rebatePercentOfSales ) == false )
                {
                    rebatePercentOfSales = -1;
                }

                if( decimal.TryParse( rebateThresholdString, out rebateThreshold ) == false )
                {
                    rebateThreshold = -1;
                }

                // hit the database for clause and substitute values if required
                ViewRebateClauseForSelectedRebate( rebateId, rebatePercentOfSales, rebateThreshold, bIsInEditMode );
                ViewCustomDateForSelectedRebate( rebateId, bIsInEditMode );
            }
            else // unexpected error
            {
                ClearRebateText( true, false );
                ClearCustomDate( true );
            }

            BindRebateClause();
        }

        // not used
        // called from InitRebateControls 
        protected void UpdateRebateTextFromSelectedItem()
        {
            int rebateId;
            decimal rebatePercentOfSales = -1;
            decimal rebateThreshold = -1;
            int rowIndex;

            if( Session[ "RebateGridViewSelectedIndex" ] == null )
                return;

            rowIndex = ( int )Session[ "RebateGridViewSelectedIndex" ];

            if( rowIndex < RebateGridView.Rows.Count ) 
            {

                rebateId = RebateGridView.GetRowIdFromSelectedIndex( rowIndex, 0 );
                string rebatePercentOfSalesString = RebateGridView.GetStringValueFromSelectedIndexForTemplateField( rowIndex, "percentOfSalesLabel" );
                string rebateThresholdString = RebateGridView.GetStringValueFromSelectedIndexForTemplateField( rowIndex, "rebateThresholdLabel" );

                if( decimal.TryParse( rebatePercentOfSalesString, out rebatePercentOfSales ) == false ) 
                { 
                    rebatePercentOfSales = -1;
                } 

                if( decimal.TryParse( rebateThresholdString, out rebateThreshold ) == false ) 
                { 
                    rebateThreshold = -1;
                } 

                // hit the database for clause and substitute values if( required
                ViewRebateClauseForSelectedRebate( rebateId, rebatePercentOfSales, rebateThreshold, false );
                ViewCustomDateForSelectedRebate( rebateId, false );
            } 
            else 
            {
                ClearRebateText( true, false );
                ClearCustomDate( true );
            }

            BindRebateClause();
            TriggerContractViewMasterUpdatePanelFromContract();

        }

        protected void ViewRebateClauseForSelectedRebate( int rebateId, decimal rebatePercentOfSales, decimal rebateThreshold, bool bIsEditMode )
        {
            ContractDB contractDB;
            bool bSuccess;
            bool bIsCustom = false;
            string rebateClause = "";

            contractDB = ( ContractDB )Session[ "ContractDB" ];

            bSuccess = contractDB.GetRebateClauseForRebate( rebateId, ref bIsCustom, ref rebateClause );


            if( bSuccess == true )
            {
                if( bIsCustom == true )
                {
                    DataRelay.EditedDocumentContentFront.RebateClause = rebateClause;

                    // preserve the most recently selected custom clause
                    Session[ "CustomRebateTextForCurrentRow" ] = rebateClause;
                    if( bIsEditMode == true )
                    {
                        SetRebateClauseReadOnly( false );
                    }
                    else
                    {
                        SetRebateClauseReadOnly( true );
                    }
                }
                else
                {
                    DataRelay.EditedDocumentContentFront.RebateClause = FormatRebateClauseWithData( rebateClause, rebatePercentOfSales, rebateThreshold );
                    SetRebateClauseReadOnly( true );
                }
            }
        }

        protected void ViewCustomDateForSelectedRebate( int rebateId, bool bIsEditMode )
        {
            bool bSuccess = false;
            bool bIsCustom = false;

            DateTime customStartDate = DateTime.MinValue;

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];

            // returns min date if null date
            bSuccess = contractDB.GetCustomDateForRebate( rebateId, ref customStartDate );

            if( bSuccess == true )
            {
                // not really a custom date
                if( customStartDate == DateTime.MinValue )
                {
                    bIsCustom = false;
                    DataRelay.EditedDocumentContentFront.CustomRebateStartDate = "";
                    Session[ "CustomStartDateForCurrentRow" ] = null;
                }
                else
                {
                    bIsCustom = true;
                    DataRelay.EditedDocumentContentFront.CustomRebateStartDate = customStartDate.ToString( "d" );
                    Session[ "CustomStartDateForCurrentRow" ] = customStartDate.ToString();
                }

            }

            if( bIsCustom == true )
            {
                if( bIsEditMode == true )
                {
                    SetCustomStartDateReadOnly( false );
                }
                else
                {
                    SetCustomStartDateReadOnly( true );
                }
            }
            else
            {
                SetCustomStartDateReadOnly( true );
            }
        }

        protected void ClearRebateText( bool bReadOnly, bool bInitWithText )
        {
            if( bInitWithText == true )
            {
                DataRelay.EditedDocumentContentFront.RebateClause = "Enter custom rebate text.";
            }
            else
            {
                DataRelay.EditedDocumentContentFront.RebateClause = "";
            }
               
            SetRebateClauseReadOnly( bReadOnly );
        }

        protected void ClearCustomDate( bool bReadOnly )
        {
            DataRelay.EditedDocumentContentFront.CustomRebateStartDate = "";
            SetCustomStartDateReadOnly( bReadOnly );
        }

        // format the text to substitute the numeric values for any located placeholders
        // -1 indicates a value has not been provided by the user
        private string FormatRebateClauseWithData( string rebateClause, decimal rebatePercentOfSales, decimal rebateThreshold ) 
        {
            string formattedRebateClause = "";

            int percentPosition = -1;
            int thresholdPosition = -1;
            int endOfPercent = -1;
            int endOfThreshold = -1;

            string s1 = "";
            string s2 = "";
            string s3 = "";

            percentPosition = GetRebateTagPosition( rebateClause, RebatePercentTagString );

            // percent is required
            if( percentPosition >= 0 ) 
            { 
                endOfPercent = percentPosition + ( RebatePercentTagString.Length - 1 );
                s1 = rebateClause.Substring( 0, percentPosition );
                s2 = rebatePercentOfSales.ToString();
                s3 = rebateClause.Substring( endOfPercent + 1, ( rebateClause.Length - endOfPercent ) - 1 );
                formattedRebateClause = s1 + s2 + s3;
            } 
            else 
            {
                formattedRebateClause = rebateClause;
            } 

            thresholdPosition = GetRebateTagPosition( formattedRebateClause, RebateThresholdTagString );

            // threshold is required
            if( thresholdPosition >= 0 ) 
            { 
                endOfThreshold = thresholdPosition + ( RebateThresholdTagString.Length - 1 );
                s1 = formattedRebateClause.Substring( 0, thresholdPosition );
                s2 = rebateThreshold.ToString();
                s3 = formattedRebateClause.Substring( endOfThreshold + 1, ( formattedRebateClause.Length - endOfThreshold ) - 1 );
                formattedRebateClause = s1 + s2 + s3;
            } 

            return( formattedRebateClause );
        }

        private int GetRebateTagPosition( string rebateClause, string tag )
        {
            return( rebateClause.IndexOf( tag ));
        }

        private bool ValidateRebateBeforeUpdate( GridView rebateGridView, int itemIndex, int selectedRebateId, bool bIsShortSave )
        {
            bool bIsValid = true;

            bool bSuccess;

            int startQuarterId;
            int endQuarterId;
            decimal rebatePercentOfSales = -1;
            decimal rebateThreshold = -1;
            string rebateClause = "";
            bool bIsCustom = false;

            TextBox rebateClauseTextBox = ( TextBox )RebatesFooterClauseFormView.FindControl( "RebateClauseTextBox" );

            string startQuarterIdString  = rebateGridView.GetStringValueFromSelectedControl( itemIndex, RebateStartYearQuarterFieldNumber, 0, false, "startYearQuarterDropDownList" );
            string endQuarterIdString  = rebateGridView.GetStringValueFromSelectedControl( itemIndex, RebateEndYearQuarterFieldNumber, 0, false, "endYearQuarterDropDownList" );
            string rebatePercentOfSalesString  = rebateGridView.GetStringValueFromSelectedControl( itemIndex, RebatePercentOfSalesFieldNumber, 0, false, "percentOfSalesTextBox" );
            string rebateThresholdString  = rebateGridView.GetStringValueFromSelectedControl( itemIndex, RebateThresholdFieldNumber, 0, false, "rebateThresholdTextBox" );

            // rebate clause
            int selectedClauseId  = int.Parse( rebateGridView.GetStringValueFromSelectedControl( itemIndex, RebatesStandardRebateTermIdFieldNumber, 0, false, "rebateClauseNameDropDownList" ));

            // custom date
            TextBox customDateTextBox = ( TextBox )RebatesFooterDateFormView.FindControl( "CustomStartDateTextBox" );
            string customDateString  = customDateTextBox.Text;
            DateTime customDate;

            // standard
            if( selectedClauseId != -1 )
            { 
                bIsCustom = false;
                rebateClause = GetStandardRebateTermFromId( selectedClauseId );
            } 
            else 
            { 
                // custom
                bIsCustom = true;
                if( rebateClauseTextBox != null )
                { 
                    rebateClause = rebateClauseTextBox.Text;
                } 
            } 

            // standard
            if( bIsCustom == false )
            { 
                // determine which, if( any numeric values are required by the standard clause
                bool bIsPercentRequired  = false;
                bool bIsThresholdRequired  = false;

                if( GetRebateTagPosition( rebateClause, RebatePercentTagString ) >= 0 ) 
                { 
                    bIsPercentRequired = true;
                } 

                if( GetRebateTagPosition( rebateClause, RebateThresholdTagString ) >= 0 ) 
                { 
                    bIsThresholdRequired = true;
                } 

                // validate that the required numeric value(s) are present
                if( Decimal.TryParse( rebatePercentOfSalesString, out rebatePercentOfSales ) == false ) 
                { 
                    rebatePercentOfSales = -1;
                } 

                if( Decimal.TryParse( rebateThresholdString, out rebateThreshold ) == false) 
                { 
                    rebateThreshold = -1;
                } 

                if( bIsPercentRequired == true && rebatePercentOfSales == -1 ) 
                { 
                    bIsValid = false;
                    AppendValidationError( "A value for percent is required for the selected standard rebate clause.", bIsShortSave );
                } 

                if( bIsThresholdRequired == true && rebateThreshold == -1 ) 
                { 
                    bIsValid = false;
                    AppendValidationError( "A value for threshold is required for the selected standard rebate clause.", bIsShortSave );
                } 

            } 
            else 
            { 
                // custom
                // validate that some custom text is present
                if( rebateClause.Length <= 0 ) 
                { 
                    bIsValid = false;
                    AppendValidationError( "A custom rebate clause must be entered when 'custom' has been selected as the rebate type.", bIsShortSave );
                } 
            } 

            // date validation
            if( bIsValid == true ) 
            { 

                CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
                ContractDB contractDB  = ( ContractDB )Session[ "ContractDB" ];

                DateTime startQuarterStartDate = DateTime.MinValue;
                DateTime startQuarterEndDate = DateTime.MinValue;
                DateTime endQuarterStartDate = DateTime.MinValue;
                DateTime endQuarterEndDate = DateTime.MinValue;
                string yearQuarterDescription = ""; 
                int fiscalYear = -1; 
                int quarter = -1; 
                int calendarYear = -1; 
                DateTime startDateOfContractAwardQuarter = DateTime.MinValue;
                DateTime endDateOfContractExpirationQuarter = DateTime.MinValue; 
                int testQuarterId = -1;
                DateTime testStartQuarterStartDate = DateTime.MinValue;
                DateTime testStartQuarterEndDate = DateTime.MinValue;
                DateTime testEndQuarterStartDate = DateTime.MinValue;
                DateTime testEndQuarterEndDate = DateTime.MinValue; 

                startQuarterId = int.Parse( startQuarterIdString );
                endQuarterId = int.Parse( endQuarterIdString );

                // custom date
                if( startQuarterId == -1 ) 
                { 
                    if( DateTime.TryParse(customDateString, out customDate) != true )
                    { 
                        bIsValid = false;
                        AppendValidationError( "A valid custom date must be entered when custom has been selected as the start date.", bIsShortSave );
                    } 
                    else 
                    {
                        startQuarterStartDate = customDate;
                        endQuarterEndDate = customDate.AddYears( 1 ); // one year later
                        endQuarterEndDate = endQuarterEndDate.AddDays( -1 ); // minus one day
                    } 
                } 
                else 
                { 
                    // standard date
                    bSuccess = contractDB.GetYearQuarterInfo( startQuarterId, ref yearQuarterDescription, ref fiscalYear, ref quarter, ref startQuarterStartDate, ref startQuarterEndDate, ref calendarYear );
                    bSuccess = contractDB.GetYearQuarterInfo( endQuarterId, ref yearQuarterDescription, ref fiscalYear, ref quarter, ref endQuarterStartDate, ref endQuarterEndDate, ref calendarYear );
                } 

                // compare standard dates to each other
                if( startQuarterId != -1 ) 
                { 
                    if( startQuarterId > endQuarterId ) 
                    { 
                        bIsValid = false;
                        AppendValidationError( "Start Year Quarter must precede End Year Quarter.", bIsShortSave );
                    } 
                } 

                // validate against contract dates
                if( bIsValid == true ) 
                { 

                    // note not caring about most of the return parms in this case
                    bSuccess = contractDB.GetYearQuarterInfo( currentDocument.AwardDate, ref yearQuarterDescription, ref fiscalYear, ref quarter, ref testStartQuarterStartDate, ref testStartQuarterEndDate, ref calendarYear, ref testQuarterId );
                    bSuccess = contractDB.GetYearQuarterInfo( currentDocument.ExpirationDate, ref yearQuarterDescription, ref fiscalYear, ref quarter, ref testEndQuarterStartDate, ref testEndQuarterEndDate, ref calendarYear, ref testQuarterId );

                    if( DateTime.Compare( startQuarterStartDate, testStartQuarterStartDate ) < 0 ||
                       DateTime.Compare( endQuarterEndDate, testStartQuarterStartDate ) < 0  ||
                        DateTime.Compare( startQuarterStartDate, testEndQuarterEndDate ) > 0  ||
                       DateTime.Compare( endQuarterEndDate, testEndQuarterEndDate ) > 0 ) 
                    { 

                        bIsValid = false;

                        if( startQuarterId == -1 ) 
                        { 
                            AppendValidationError( "Custom dates must fall within the contract dates.", bIsShortSave );
                        } 
                        else 
                        {
                            AppendValidationError( "Selected Year Quarters must fall within the contract dates.", bIsShortSave );
                        } 

                    } 

                } 

            } 

            return( bIsValid );
        }
    


        protected void rebateClauseNameDropDownList_OnSelectedIndexChanged( object sender,  EventArgs e )
        {
            DropDownList rebateClauseNameDropDownList  = ( DropDownList )sender;

            ListItem selectedItem;

            selectedItem = rebateClauseNameDropDownList.SelectedItem;

            int standardRebateTermId;

            standardRebateTermId = int.Parse( selectedItem.Value );

            TextBox rebateClauseTextBox = ( TextBox )RebatesFooterClauseFormView.FindControl( "RebateClauseTextBox" );

            string rebateClause; 

            // if custom
            if( standardRebateTermId == -1 ) 
            { 
                if( Session[ "CustomRebateTextForCurrentRow" ] != null )  
                { 
                    rebateClauseTextBox.Text = Session[ "CustomRebateTextForCurrentRow" ].ToString();
                    SetRebateClauseReadOnly( false );
                } 
                else 
                {
                    rebateClauseTextBox.Text = "Enter custom rebate text.";
                    SetRebateClauseReadOnly( false );
                } 

            } 
            else 
            { 
                // standard
                rebateClause = GetStandardRebateTermFromId( standardRebateTermId );

                decimal rebatePercentOfSales = -1;
                decimal rebateThreshold = -1;

                if( Session[ "RebateGridViewSelectedIndex" ] == null )
                    return;

                int rowIndex = int.Parse( Session[ "RebateGridViewSelectedIndex" ].ToString() );
             
                string rebatePercentOfSalesString = RebateGridView.GetStringValueFromSelectedIndexForTemplateField( rowIndex, "percentOfSalesTextBox" );
                string rebateThresholdString  = RebateGridView.GetStringValueFromSelectedIndexForTemplateField( rowIndex, "rebateThresholdTextBox" );

                if( Decimal.TryParse( rebatePercentOfSalesString, out rebatePercentOfSales ) == false ) 
                { 
                    rebatePercentOfSales = -1;
                } 

                if( Decimal.TryParse( rebateThresholdString, out rebateThreshold ) == false ) 
                { 
                    rebateThreshold = -1;
                } 

                rebateClauseTextBox.Text = FormatRebateClauseWithData( rebateClause, rebatePercentOfSales, rebateThreshold );

                SetRebateClauseReadOnly( true );
            } 
        }

        private string GetStandardRebateTermFromId( int standardRebateTermId )
        {
            string rebateClause = "";

            DataTable t  = RebateStandardClausesDataSet.Tables[ ContractDB.StandardRebateTermsTableName ];
            Object keyObject  = ( Object )standardRebateTermId;
            DataRow row  = t.Rows.Find (keyObject );

            if( row != null )
            { 
                rebateClause = row[ "RebateClause" ].ToString();
            } 

            return( rebateClause );
        }

        protected void rebateClauseNameDropDownList_DataBound( object sender,  EventArgs e )
        {
            DropDownList rebateClauseNameDropDownList  = ( DropDownList )sender;
            GridViewRow gridViewRow  = ( GridViewRow )rebateClauseNameDropDownList.NamingContainer;
            int rebateTermId = -1;
            bool bFoundStandardClause = false;
            ListItem customListItem = new ListItem();

            if( gridViewRow != null )
            { 
                DataRowView dataRowView  = ( DataRowView )gridViewRow.DataItem;
                string rebateTermIdString = dataRowView[ "RebateTermId" ].ToString();

                if( rebateTermIdString != null )
                { 
                    if( int.TryParse( rebateTermIdString, out rebateTermId ) ) 
                    { 
                        ListItem listItem  = rebateClauseNameDropDownList.Items.FindByValue( rebateTermId.ToString() );
                        if( listItem != null )
                        { 
                            listItem.Selected = true;
                            bFoundStandardClause = true;
                   //         SetRebateClauseReadOnly( true );
                        } 

                        // add a row for custom
                        customListItem.Text = "Custom";
                        customListItem.Value = "-1";
                        rebateClauseNameDropDownList.Items.Add( customListItem );

                        if( bFoundStandardClause == false ) 
                        { 
                            customListItem.Selected = true;
                   //         SetRebateClauseReadOnly( false );
                        } 

                    } 
                } 
            } 
        }

   

        protected void RemoveRebateButton_DataBinding( object sender,  EventArgs e )
        {
            Button removeRebateButton  = ( Button )sender;
            if( removeRebateButton != null )
            { 
                CMGlobals.MultilineButtonText( removeRebateButton, new String[] { "Remove", "Rebate" } );
            } 
        }
    
        protected void ViewRebateTextButton_DataBinding( object sender,  EventArgs e )
        {
            Button viewRebateButton  = ( Button )sender;
            if( viewRebateButton != null )
            {
                CMGlobals.MultilineButtonText( viewRebateButton, new String[] { "View", "Rebate Clause" } );
            } 
        }

#region "updateinsertparameters"

        private int InsertRebate( GridView rebateGridView, int rowIndex ) 
        {
            int insertedRowIndex = 0;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs  = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            RebateContractNumberParameterValue = currentDocument.ContractNumber;
            UserLoginParameterValue = bs.UserInfo.LoginName;

            // may be -1 for custom
            int startQuarterId;
            startQuarterId = int.Parse( rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebateStartYearQuarterFieldNumber, 0, false, "startYearQuarterDropDownList" ) );

            StartQuarterIdParameterValue = startQuarterId.ToString();
            EndQuarterIdParameterValue = rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebateEndYearQuarterFieldNumber, 0, false, "endYearQuarterDropDownList" );
            RebatePercentOfSalesParameterValue = rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebatePercentOfSalesFieldNumber, 0, false, "percentOfSalesTextBox" );
            RebateThresholdParameterValue = rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebateThresholdFieldNumber, 0, false, "rebateThresholdTextBox" );

            // rebate clause
            int selectedClauseId = int.Parse( rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebatesStandardRebateTermIdFieldNumber, 0, false, "rebateClauseNameDropDownList" ));

            // standard
            if( selectedClauseId != -1 )
            { 
                IsCustomParameterValue = false;
                StandardRebateTermIdParameterValue = selectedClauseId.ToString();
                RebateClauseParameterValue = "custom"; // placeholder value not saved

            } 
            else 
            { 
                // custom
                IsCustomParameterValue = true;
                StandardRebateTermIdParameterValue = "-1"; // placeholder value not saved

                // must save custom text from textbox
                TextBox rebateClauseTextBox = ( TextBox )RebatesFooterClauseFormView.FindControl( "RebateClauseTextBox" );
                RebateClauseParameterValue = rebateClauseTextBox.Text;
            } 

            // custom date
            if( startQuarterId == -1 ) 
            {
                TextBox customStartDateTextBox = ( TextBox )RebatesFooterDateFormView.FindControl( "CustomStartDateTextBox" );
                RebateCustomStartDateParameterValue = customStartDateTextBox.Text;
            } 


            // not used
            AmountReceivedParameterValue = "0";

            try
            {
                RebateDataSource.Insert();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }


            rebateGridView.InsertRowActive = false; // done with insert
            rebateGridView.EditIndex = -1; // done with edit
            AddingRebateRecord = false; // _withAddRebateParameter.DefaultValue = "false";   // no extra row
            rebateGridView.DataBind(); //  bind with new row

            if( Session[ "LastInsertedRebateId" ] != null )
            { 
                int newRebateId = ( int )Session[ "LastInsertedRebateId" ];
                insertedRowIndex = rebateGridView.GetRowIndexFromId( newRebateId, 0 );

                SetRebateGridViewSelectedItem( insertedRowIndex, true );

                // bind to select
                rebateGridView.DataBind();
            } 

            // enable appropriate buttons for the selected row
            SetEnabledRebateControlsDuringEdit( rebateGridView, insertedRowIndex, true );

            EnableControlsForRebateEditMode( true );

            return( insertedRowIndex );
        }
    


        private int UpdateRebate( GridView rebateGridView, int rowIndex )
        {
            int updatedRowIndex = -1;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs  = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            RebateContractNumberParameterValue = currentDocument.ContractNumber;
            UserLoginParameterValue = bs.UserInfo.LoginName;

            RebateIdParameterValue = rebateGridView.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

            // may be -1 for custom
            int startQuarterId;
            startQuarterId = int.Parse( rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebateStartYearQuarterFieldNumber, 0, false, "startYearQuarterDropDownList" ));

            StartQuarterIdParameterValue = startQuarterId.ToString();
            EndQuarterIdParameterValue = rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebateEndYearQuarterFieldNumber, 0, false, "endYearQuarterDropDownList" );
            RebatePercentOfSalesParameterValue = rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebatePercentOfSalesFieldNumber, 0, false, "percentOfSalesTextBox"  );
            RebateThresholdParameterValue = rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebateThresholdFieldNumber, 0, false, "rebateThresholdTextBox"  );

            // rebate clause
            int selectedClauseId = int.Parse( rebateGridView.GetStringValueFromSelectedControl( rowIndex, RebatesStandardRebateTermIdFieldNumber, 0, false, "rebateClauseNameDropDownList"  ));

            // standard
            if( selectedClauseId != -1 )
            { 
                IsCustomParameterValue = false ;
                StandardRebateTermIdParameterValue = selectedClauseId.ToString();
                RebateClauseParameterValue = "custom"; // placeholder value not saved

            } 
            else 
            { 
                // custom
                IsCustomParameterValue = true;
                StandardRebateTermIdParameterValue = "-1"; // placeholder value not saved

                // must save custom text from textbox
                TextBox rebateClauseTextBox = ( TextBox )RebatesFooterClauseFormView.FindControl( "RebateClauseTextBox" );
                RebateClauseParameterValue = rebateClauseTextBox.Text;
            } 

            // custom date
            if( startQuarterId == -1 ) 
            {
                TextBox customStartDateTextBox = ( TextBox )RebatesFooterDateFormView.FindControl( "CustomStartDateTextBox" );
                RebateCustomStartDateParameterValue = customStartDateTextBox.Text;
            } 

            // not used
            AmountReceivedParameterValue = "0";

            try
            {
                RebateDataSource.Update();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }

            rebateGridView.EditIndex = -1; // done with edit
            rebateGridView.DataBind();

            if( Session[ "LastUpdatedRebateId" ] != null )
            { 
                int lastUpdatedRebateId = int.Parse( Session[ "LastUpdatedRebateId" ].ToString() );
                updatedRowIndex = rebateGridView.GetRowIndexFromId( lastUpdatedRebateId, 0 );

                SetRebateGridViewSelectedItem( updatedRowIndex, false );

                // bind to select
                rebateGridView.DataBind();
            } 

            // enable appropriate buttons for the selected row
            SetEnabledRebateControlsDuringEdit( rebateGridView, updatedRowIndex, true );

            EnableControlsForRebateEditMode( true );

            return( updatedRowIndex );

        }

        private int DeleteRebate( GridView rebateGridView, int rowIndex )
        {
            int updatedRowIndex = -1;

            CurrentDocument currentDocument  = ( CurrentDocument )Session[ "CurrentDocument" ];
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

            RebateContractNumberParameterValue = currentDocument.ContractNumber;
            UserLoginParameterValue = bs.UserInfo.LoginName;

            RebateIdParameterValue = rebateGridView.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();


            try
            {
                RebateDataSource.Delete();
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

            SetRebateGridViewSelectedItem( updatedRowIndex, false );

            // bind to select
            rebateGridView.DataBind();

            return( updatedRowIndex );
        }
    


 

#endregion

#region "rebateDateSelection"

        protected void startYearQuarterLabel_OnDataBinding( object sender,  EventArgs e )
        {
            Label startYearQuarterLabel = ( Label )sender;

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];

            string yearQuarterDescription = "";
            int fiscalYear = -1;
            int quarter = -1;
            DateTime quarterStartDate = DateTime.MinValue;
            DateTime quarterEndDate = DateTime.MinValue;
            int calendarYear = -1;

            if( startYearQuarterLabel != null )
            { 
                GridViewRow gridViewRow  = ( GridViewRow )startYearQuarterLabel.NamingContainer;
                if( gridViewRow != null )
                { 
                    if( gridViewRow.DataItem != null )
                    {
                        DataRowView dataRowView = ( DataRowView )gridViewRow.DataItem;

                        // use targetCell for formatting
                        // targetCell TableCell = gridViewRow.Cells(5)
                        string startQuarterIdString = dataRowView[ "StartQuarterId" ].ToString();
                        int startQuarterId = int.Parse( startQuarterIdString );

                        if( startQuarterId > 0 ) // was != -1
                        {
                            if( contractDB.GetYearQuarterInfo( int.Parse( startQuarterIdString ), ref yearQuarterDescription, ref fiscalYear, ref quarter, ref quarterStartDate, ref quarterEndDate, ref calendarYear ) == true )
                            {
                                startYearQuarterLabel.Text = yearQuarterDescription;
                            }
                            else
                            {
                                // $$$ show database error here
                            }
                        } 
                        else 
                        { 
                            // custom
                            startYearQuarterLabel.Text = "Custom";
                        } 
                    } 

                } 
            } 

        }

        protected void startYearQuarterDropDownList_DataBound( object sender,  EventArgs e)
        {
            DropDownList startYearQuarterDropDownList  = ( DropDownList )sender;
            GridViewRow gridViewRow  = ( GridViewRow )startYearQuarterDropDownList.NamingContainer;
            int startQuarterId = -1;
            bool bFoundStandardQuarter = false;
            ListItem customListItem  = new ListItem();

            if( gridViewRow != null )
            { 
                DataRowView dataRowView  = ( DataRowView )gridViewRow.DataItem;
                string startQuarterIdString = dataRowView[ "StartQuarterId" ].ToString();

                if( startQuarterIdString != null )
                { 
                    if( int.TryParse( startQuarterIdString, out startQuarterId )) 
                    { 
                        ListItem listItem  = startYearQuarterDropDownList.Items.FindByValue( startQuarterId.ToString() );
                        if( listItem != null )
                        { 
                            listItem.Selected = true;
                            bFoundStandardQuarter = true;
                        } 

                        // add a row for custom
                        customListItem.Text = "Custom";
                        customListItem.Value = "-1";
                        startYearQuarterDropDownList.Items.Add( customListItem );

                        if( bFoundStandardQuarter == false )
                        { 
                            customListItem.Selected = true;
            //                SetCustomStartDateReadOnly( false );
                        } 

                    } 
                } 
            } 
        }

        protected void startYearQuarterDropDownList_OnSelectedIndexChanged( object sender,  EventArgs e )
        {
            DropDownList startYearQuarterDropDownList  = ( DropDownList )sender;
            ListItem selectedItem;

            selectedItem = startYearQuarterDropDownList.SelectedItem;

            int selectedQuarterId;

            selectedQuarterId = int.Parse( selectedItem.Value );

            TextBox customStartDateTextBox = ( TextBox )RebatesFooterDateFormView.FindControl( "CustomStartDateTextBox" );

            // if custom
            if( selectedQuarterId == -1 )
            { 
                if( Session[ "CustomStartDateForCurrentRow" ] != null )
                { 
                    customStartDateTextBox.Text = Session[ "CustomStartDateForCurrentRow" ].ToString();
                    SetCustomStartDateReadOnly( false );
                } 
                else 
                {
                    customStartDateTextBox.Text = "Enter date.";
                    SetCustomStartDateReadOnly( false );
                } 

            } 
            else 
            { // standard
                // push the custom date
                Session[ "CustomStartDateForCurrentRow" ] = customStartDateTextBox.Text;
                // clear 
                ClearCustomDate( true );
            } 

            GridViewRow gridViewRow  = ( GridViewRow )startYearQuarterDropDownList.NamingContainer;
            DropDownList endYearQuarterDropDownList  = ( DropDownList )gridViewRow.FindControl( "endYearQuarterDropDownList" );
            selectedItem = endYearQuarterDropDownList.SelectedItem;

            int selectedEndQuarterId;

            selectedEndQuarterId = int.Parse( selectedItem.Value );

            // if NOT alredy custom 
            if( selectedEndQuarterId != -1 ) 
            { 
                // unselect in other drop down
                ListItem currentItem  = endYearQuarterDropDownList.SelectedItem;
                if( currentItem != null )
                { 
                    currentItem.Selected = false;
                } 

                // select custom in the other drop down also
                ListItem listItem  = endYearQuarterDropDownList.Items.FindByValue( "-1" );
                if( listItem != null )
                { 
                    listItem.Selected = true;
                } 
            } 

        }

        protected void endYearQuarterLabel_OnDataBinding( object sender,  EventArgs e )
        {
            Label endYearQuarterLabel = ( Label )sender;

            ContractDB contractDB  = ( ContractDB )Session[ "ContractDB" ];

            string yearQuarterDescription = "";
            int fiscalYear = -1;
            int quarter = -1;
            DateTime quarterStartDate = DateTime.MinValue;
            DateTime quarterEndDate = DateTime.MinValue;
            int calendarYear = -1;

            if( endYearQuarterLabel != null )
            { 
                GridViewRow gridViewRow  = ( GridViewRow )endYearQuarterLabel.NamingContainer;
                if( gridViewRow != null )
                { 
                    if( gridViewRow.DataItem != null )
                    { 
                        DataRowView dataRowView = ( DataRowView )gridViewRow.DataItem;

                        // use targetCell for formatting
                        // targetCell TableCell = gridViewRow.Cells(5)
                        string endQuarterIdString = dataRowView[ "endQuarterId" ].ToString();
                        int endQuarterId = int.Parse( endQuarterIdString );

                        if( endQuarterId > 0 ) // was != -1
                        { 
                            if( contractDB.GetYearQuarterInfo( endQuarterId, ref yearQuarterDescription, ref fiscalYear, ref quarter, ref quarterStartDate, ref quarterEndDate, ref calendarYear ) == true ) 
                            { 
                                endYearQuarterLabel.Text = yearQuarterDescription;
                            } 
                        } 
                        else 
                        { // custom
                            endYearQuarterLabel.Text = "Custom";
                        } 

                    } 
                } 
            } 
        }

        protected void endYearQuarterDropDownList_DataBound( object sender,  EventArgs e)
        {
            DropDownList endYearQuarterDropDownList = ( DropDownList )sender;
            GridViewRow gridViewRow  = ( GridViewRow )endYearQuarterDropDownList.NamingContainer;
            int endQuarterId = -1;
            bool bFoundStandardQuarter = false;
            ListItem customListItem  = new ListItem();

            if( gridViewRow != null )
            { 
                DataRowView dataRowView  = ( DataRowView )gridViewRow.DataItem;
                string endQuarterIdString = dataRowView[ "endQuarterId" ].ToString();

                if( endQuarterIdString != null )
                { 
                    if( int.TryParse( endQuarterIdString, out endQuarterId )) 
                    { 
                        ListItem listItem  = endYearQuarterDropDownList.Items.FindByValue( endQuarterId.ToString() );
                        if( listItem != null )
                        { 
                            listItem.Selected = true;
                            bFoundStandardQuarter = true;
                        } 

                        // add a row for custom
                        customListItem.Text = "Custom";
                        customListItem.Value = "-1";
                        endYearQuarterDropDownList.Items.Add( customListItem );

                        if( bFoundStandardQuarter == false )
                        { 
                            customListItem.Selected = true;
               //             SetCustomStartDateReadOnly( false );
                        } 

                    } 
                } 
            } 
        }

        protected void endYearQuarterDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList endYearQuarterDropDownList  = ( DropDownList )sender;
            ListItem selectedItem;

            GridViewRow gridViewRow = ( GridViewRow )endYearQuarterDropDownList.NamingContainer;
            DropDownList startYearQuarterDropDownList  = ( DropDownList )gridViewRow.FindControl( "startYearQuarterDropDownList" );

            selectedItem = endYearQuarterDropDownList.SelectedItem;

            int selectedQuarterId;

            selectedQuarterId = int.Parse( selectedItem.Value );

            // if custom 
            if( selectedQuarterId == -1 ) 
            { 
                // unselect in other drop down
                ListItem currentItem  = startYearQuarterDropDownList.SelectedItem;
                if( currentItem != null )
                { 
                    currentItem.Selected = false;
                } 

                // select custom in the other drop down also
                ListItem listItem  = startYearQuarterDropDownList.Items.FindByValue( selectedQuarterId.ToString() );
                if( listItem != null )
                { 
                    listItem.Selected = true;
                } 
            } 
        }

#endregion "rebateDateSelection"

      
    }
}