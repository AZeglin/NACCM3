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
    public partial class EditContractSales : System.Web.UI.Page
    {
        EditContractSalesWindowParms _editContractSalesWindowParms = null;

        private ObjectDataSource _editContractSalesHeaderDataSource = null;

        private EditContractSalesHeaderContent _editContractSalesHeaderContent = null; 

        private const int EditSalesEditButtonFieldNumber = 0;  // $$$+
        private const int EditSalesSINFieldNumber = 1; //$$$+
        private const int EditSalesVASalesFieldNumber = 2; //$$$+
        private const int EditSalesOGASalesFieldNumber = 3; //$$$+
        private const int EditSalesSLGSalesFieldNumber = 4; //$$$+
        private const int EditSalesCommentsFieldNumber = 5; //$$$+
        private const int EditSalesModifiedByFieldNumber = 6; //$$$+
        private const int EditSalesLastModificationDateFieldNumber = 7; //$$$+

        private const int EditSalesExternalSalesIdFieldNumber = 8;  // $$$+
        private const int EditSalesSalesIdFieldNumber = 9;  // $$$+
        private const int EditSalesIsNewFieldNumber = 10;  // $$$+


        private const int EDITSALESGRIDVIEWROWHEIGHTESTIMATE = 48;

        private DocumentDataSource _editSalesDataSource = null;

        public DocumentDataSource EditSalesDataSource
        {
            get { return _editSalesDataSource; }
            set { _editSalesDataSource = value; }
        }

        // EditSales parameters
        private Parameter _contractNumberParameter = null;
        private Parameter _quarterIdParameter = null;
        private Parameter _salesIdParameter = null;
        private Parameter _isNewParameter = null;
        private Parameter _SINParameter = null;
        private Parameter _VASalesParameter = null;
        private Parameter _OGASalesParameter = null;
        private Parameter _SLGSalesParameter = null;
        private Parameter _commentsParameter = null;
        private Parameter _newSalesIdParameter = null;

   
        public bool SalesWereModified
        {
            get
            {
                bool bDirty = false;
                if( Session[ "SalesWereModified" ] != null )
                {
                    bDirty = bool.Parse( Session[ "SalesWereModified" ].ToString() );
                }
                return ( bDirty );
            }
            set
            {
                Session[ "SalesWereModified" ] = value;
            }
        }

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

       //     CMGlobals.AddKeepAlive( this.Page, 12000 );

            if( Session[ "EditContractSalesWindowParms" ] != null )
            {
                _editContractSalesWindowParms = ( EditContractSalesWindowParms )Session[ "EditContractSalesWindowParms" ];
                InitObjectDataSource();
            }

            
            // if Add New then user must decide current quarter
            if( _editContractSalesHeaderContent.IsNewQuarter == false )
            {
                LoadEditSalesGrid();

                if( Page.IsPostBack == false )
                {
                    SetEditSalesGridViewSelectedItem( 0, true );
                    BindEditSalesGridView();
                }
                else
                {
                    RestoreEditSalesGridViewSelectedItem();
                }

                BindFormViews();
            }
            else
            {
                if( Page.IsPostBack == false )
                {
                    LoadYearQuarters( _editContractSalesHeaderContent.ContractNumber );

                    if( EditSalesYearQuarterDropDownList != null )
                    {
                        DataSet dsNewSalesQuarters = ( DataSet )Session[ "NewSalesQuartersDataSet" ];
                        EditSalesYearQuarterDropDownList.DataSource = dsNewSalesQuarters;
                        EditSalesYearQuarterDropDownList.DataValueField = "Quarter_ID";
                        EditSalesYearQuarterDropDownList.DataTextField = "Title";
                    }

                    BindFormViews();
                    BindYearQuarterDropDownList();
                }
                else  // in add mode with postback
                {
                    if( _editContractSalesHeaderContent.HasNewQuarterBeenSelected == true )
                    {
                        LoadEditSalesGrid();
                    }
                }
            }
        }

        protected void BindYearQuarterDropDownList()
        {
            try
            {
                // bind
                EditSalesYearQuarterDropDownList.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( this.Page, ex );
            }
        }

        protected void ClearSessionVariables()
        {
            Session[ "EditSalesDataSource" ] = null;
            Session[ "EditSalesGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedEditSalesId" ] = null;
            Session[ "EditContractSalesHeaderDataSource" ] = null;
            Session[ "NewSalesQuartersDataSet" ] = null;
            Session[ "EditContractSalesHeaderContent" ] = null;
            Session[ "SalesWereModified" ] = null;
        }

        private void InitObjectDataSource()
        {
            InitSalesHeaderContent();

            if( Session[ "EditContractSalesHeaderDataSource" ] == null )
            {
                _editContractSalesHeaderDataSource = new ObjectDataSource();
                _editContractSalesHeaderDataSource.ID = "EditContractSalesHeaderDataSource";
                _editContractSalesHeaderDataSource.DataObjectTypeName = "VA.NAC.NACCMBrowser.BrowserObj.EditContractSalesHeaderContent";
                _editContractSalesHeaderDataSource.TypeName = "VA.NAC.NACCMBrowser.BrowserObj.EditContractSalesHeaderManager";
                _editContractSalesHeaderDataSource.SelectMethod = "GetEditContractSalesHeaderContent";
                _editContractSalesHeaderDataSource.UpdateMethod = "UpdateContractSalesHeader";

            }
            else
            {
                _editContractSalesHeaderDataSource = ( ObjectDataSource )Session[ "EditContractSalesHeaderDataSource" ];
            }

            EditSalesHeaderFormView.DataSource = _editContractSalesHeaderDataSource;
        }


        private void InitSalesHeaderContent()
        {
            if( Session[ "EditContractSalesHeaderContent" ] == null )
            {
                _editContractSalesHeaderContent = new EditContractSalesHeaderContent( _editContractSalesWindowParms );
                Session[ "EditContractSalesHeaderContent" ] = _editContractSalesHeaderContent;
            }
            else
            {
                _editContractSalesHeaderContent = ( EditContractSalesHeaderContent )Session[ "EditContractSalesHeaderContent" ];
            }
        }

        private void UpdateSalesHeaderContent( int quarterId, int quarter, int year )
        {
            _editContractSalesHeaderContent = ( EditContractSalesHeaderContent )Session[ "EditContractSalesHeaderContent" ];
            _editContractSalesHeaderContent.QuarterId = quarterId;
            _editContractSalesHeaderContent.Quarter = quarter;
            _editContractSalesHeaderContent.Year = year;
            _editContractSalesHeaderContent.HasNewQuarterBeenSelected = true;
        }

        protected void BindFormViews()
        {
            EditSalesHeaderFormView.DataBind();

            // note form view controls are not yet created here
        }

        public void LoadYearQuarters( string contractNumber )
        {
            bool bSuccess = false;
            bool bNoUnreportedQuarters = false;
            DataSet dsNewSalesQuarters = null;
            ContractDB contractDB = null;

            contractDB = ( ContractDB )Session[ "ContractDB" ];

            bSuccess = contractDB.SelectNewSalesQuartersForContract( ref dsNewSalesQuarters, ref bNoUnreportedQuarters, contractNumber );

            if( bSuccess == true )
            {
                Session[ "NewSalesQuartersDataSet" ] = dsNewSalesQuarters;

                if( bNoUnreportedQuarters == true )
                {
                    MsgBox.ShowErrorFromUpdatePanel( this.Page, new Exception( "All quarters have been reported. Use 'Edit' next to desired quarter to update existing sales." ) );
                }
            }
            else
            { 
                MsgBox.ShowErrorFromUpdatePanel( this.Page, new Exception( contractDB.ErrorMessage ) );
            }
        }

        protected void EditSalesYearQuarterDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList editSalesYearQuarterDropDownList = ( DropDownList )sender;

            ListItem selectedItem;

            selectedItem = editSalesYearQuarterDropDownList.SelectedItem;

            int selectedQuarterId;
            int selectedQuarter;
            int selectedYear;
            string[] yearQuarterStrings = selectedItem.Text.Split( new Char[] { ' ' } );  // Text = Title field from table

            selectedQuarterId = int.Parse( selectedItem.Value );
            selectedYear = int.Parse( yearQuarterStrings[ 0 ] );
            selectedQuarter = int.Parse( yearQuarterStrings[ 2 ] );

            // synch content object with selection
            UpdateSalesHeaderContent( selectedQuarterId, selectedQuarter, selectedYear );

            LoadGridForNewQuarter();

        }

        protected void LoadGridForNewQuarter()
        {
            LoadEditSalesGrid();

            SetEditSalesGridViewSelectedItem( 0, true );

            BindEditSalesGridView();

            // allow the postback to occur 
            ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }


        protected string FormatHeaderLabel1()
        {
            string headerLabel1 = "";
            if( _editContractSalesHeaderContent.IsNewQuarter == true )
            {
                headerLabel1 = "Add Sales";
            }   
            else
            {
                headerLabel1 = "Edit Sales";
            }
            return( headerLabel1 );
        }

        protected string FormatContractNumber( object contractNumberObj )
        {
            string contractNumber = "";
            if( contractNumberObj != null )
            {
                contractNumber = contractNumberObj.ToString();
            }
            return ( String.Format( "For Contract {0}", contractNumber ) );
        }

        protected string FormatYearQuarter( object yearObj, object quarterObj )
        {
            string year = "";
            string quarter = "";
            if( yearObj != null && quarterObj != null )
            {
                year = yearObj.ToString();
                quarter = quarterObj.ToString();
            }
            return ( String.Format( "{0}/Quarter {1}", year, quarter ) );
        }

        private void LoadEditSalesGrid()
        {

            if( Page.Session[ "EditSalesDataSource" ] == null )
            {
                _editSalesDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.NACCMCommonUser, true );
                _editSalesDataSource.ID = "EditSalesDataSource";
                _editSalesDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _editSalesDataSource.SelectCommand = "SelectContractSalesForEdit";
                _editSalesDataSource.UpdateCommand = "UpdateContractSalesForEdit";
                _editSalesDataSource.SetEventOwnerName( "EditContractSales" );
                _editSalesDataSource.Updated += new SqlDataSourceStatusEventHandler( _editSalesDataSource_Updated );

                CreateEditSalesDataSourceParameters();

                _editSalesDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _editSalesDataSource.SelectParameters.Add( _contractNumberParameter );
                _editSalesDataSource.SelectParameters.Add( _quarterIdParameter );
 
                _editSalesDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _editSalesDataSource.UpdateParameters.Add( _contractNumberParameter );
                _editSalesDataSource.UpdateParameters.Add( _quarterIdParameter );
                
                _editSalesDataSource.UpdateParameters.Add( _salesIdParameter );
                _editSalesDataSource.UpdateParameters.Add( _isNewParameter );
                _editSalesDataSource.UpdateParameters.Add( _SINParameter );
                _editSalesDataSource.UpdateParameters.Add( _VASalesParameter );
                _editSalesDataSource.UpdateParameters.Add( _OGASalesParameter );
                _editSalesDataSource.UpdateParameters.Add( _SLGSalesParameter );
                _editSalesDataSource.UpdateParameters.Add( _commentsParameter );
                _editSalesDataSource.UpdateParameters.Add( _newSalesIdParameter );

                Page.Session[ "EditSalesDataSource" ] = _editSalesDataSource;
            }
            else
            {
                _editSalesDataSource = ( DocumentDataSource )Page.Session[ "EditSalesDataSource" ];
                _editSalesDataSource.RestoreDelegatesAfterDeserialization( this, "EditContractSales" );

                RestoreEditSalesDataSourceParameters( _editSalesDataSource );
            }

            
            EditSalesGridView.DataSource = _editSalesDataSource;
        }

 

        private void CreateEditSalesDataSourceParameters()
        {
            _contractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _quarterIdParameter = new Parameter( "QuarterId", TypeCode.Int32 );
            _salesIdParameter =  new Parameter( "SalesId", TypeCode.Int32 );
            _isNewParameter = new Parameter( "IsNew", TypeCode.Boolean );
            _SINParameter = new Parameter( "SIN", TypeCode.String );
            _VASalesParameter = new Parameter( "VASales", TypeCode.Decimal );
            _OGASalesParameter = new Parameter( "OGASales", TypeCode.Decimal );
            _SLGSalesParameter = new Parameter( "SLGSales", TypeCode.Decimal );
            _commentsParameter = new Parameter( "Comments", TypeCode.String );
            _newSalesIdParameter = new Parameter( "NewSalesId", TypeCode.Int32 );
            _newSalesIdParameter.Direction = ParameterDirection.Output;
        }

        private void RestoreEditSalesDataSourceParameters( DocumentDataSource editSalesDataSource )
        {
            // select 
            _contractNumberParameter = editSalesDataSource.SelectParameters[ "ContractNumber" ];
            _quarterIdParameter = editSalesDataSource.SelectParameters[ "QuarterId" ];

            // update
            _salesIdParameter = editSalesDataSource.UpdateParameters[ "SalesId" ];
            _isNewParameter = editSalesDataSource.UpdateParameters[ "IsNew" ];
            _SINParameter = editSalesDataSource.UpdateParameters[ "SIN" ];
            _VASalesParameter = editSalesDataSource.UpdateParameters[ "VASales" ];
            _OGASalesParameter = editSalesDataSource.UpdateParameters[ "OGASales" ];
            _SLGSalesParameter = editSalesDataSource.UpdateParameters[ "SLGSales" ];
            _commentsParameter = editSalesDataSource.UpdateParameters[ "Comments" ];
            _newSalesIdParameter = editSalesDataSource.UpdateParameters[ "NewSalesId" ];
        }

        protected void BindEditSalesGridView()
        {
            try
            {
                _contractNumberParameter.DefaultValue = _editContractSalesHeaderContent.ContractNumber;
                _quarterIdParameter.DefaultValue = _editContractSalesHeaderContent.QuarterId.ToString();

                EditSalesGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( this.Page, ex );
            }
        }

  
        protected void EditSalesGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            string externalSalesId = "";
            int itemIndex = -1;

            if( e.CommandName.CompareTo( "EditSales" ) == 0 )
            {
                string commandArgument = e.CommandArgument.ToString();
                string[] argumentList = commandArgument.Split( new char[] { ',' } );

                itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                externalSalesId = argumentList[ 1 ].ToString();

                HighlightEditSalesRow( itemIndex );

                InitiateEditModeForEditSales( itemIndex );

                // allow the postback to occur 
                ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else
            {
                if( e.CommandName.CompareTo( "SaveSales" ) == 0 )
                {
                    string commandArgument = e.CommandArgument.ToString();
                    string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                    itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                    externalSalesId = argumentList[ 1 ].ToString();

                    string validationMessage = "";

                    // validate the item before saving
                    bool bIsItemOk = ValidateSalesBeforeUpdate( EditSalesGridView, itemIndex, ref validationMessage );

                    if( bIsItemOk == true )
                    {
                        int newOrUpdatedRowIndex = -1;

                        newOrUpdatedRowIndex = UpdateInsertSales( EditSalesGridView, itemIndex );

                        HighlightEditSalesRow( newOrUpdatedRowIndex );

                        // allow the postback to occur 
                        ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

                    }
                    else
                    {
                        MsgBox.ShowErrorFromUpdatePanel( this.Page, new Exception( validationMessage ) );                        
                    }

                    BindFormViews();

                    // allow the postback to occur 
                    UpdateHeaderUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

                }
                else if( e.CommandName.CompareTo( "Cancel" ) == 0 )
                {
                    string commandArgument = e.CommandArgument.ToString();
                    string[] argumentList = commandArgument.Split( new Char[] { ',' } );

                    itemIndex = int.Parse( argumentList[ 0 ].ToString() );
                    externalSalesId = argumentList[ 1 ].ToString();

                    CancelEdit( itemIndex );
                }
            }
        }

 
        protected void EditSalesGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {

                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

 
                if( e.Row.RowType == DataControlRowType.DataRow )
                {

                    if( ( ( ( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                        ( ( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert ) ) )
                    {

                        Button viewEditSalesTextButton = null;
                        viewEditSalesTextButton = ( Button )e.Row.FindControl( "ViewEditSalesTextButton" );
                        if( viewEditSalesTextButton != null )
                        {
                            // colors match .PersonalizedNotificationGridItems and  .PersonalizedNotificationGridAltItems
                            string rowColor = "alt";
                            int odd = 0;
                            Math.DivRem( rowIndex, 2, out odd );
                            if( odd > 0 )
                            {
                                rowColor = "norm";
                            }
                            string windowHighlightCommand = string.Format( "resetEditSalesHighlighting( 'EditSalesGridView', {0}, '{1}' );", rowIndex, rowColor );
                            viewEditSalesTextButton.Attributes.Add( "onclick", windowHighlightCommand );
                        }

                        if( currentDocument != null )
                        {
                            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.Sales ) != true )
                            {
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

                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }


        protected void EditSalesGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForEditSales( e.NewEditIndex );  // added this to match drug item grid handling 
        }

        protected void EditSalesGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            CancelEdit( e.RowIndex );
        }

        protected void CancelEdit( int rowIndex )
        {
            int cancelIndex = rowIndex;

            EditSalesGridView.EditIndex = -1; // cancels the edit
            BindEditSalesGridView();

            // enable appropriate buttons for the selected row
            SetEnabledEditSalesControlsDuringEdit( EditSalesGridView, cancelIndex, true );

            EnableControlsForEditSalesEditMode( true );

            HighlightEditSalesRow( cancelIndex );
   
            // allow the postback to occur 
            ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }


        protected void EditSalesGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id fields
            if( e.Row.Cells.Count > EditSalesIsNewFieldNumber )
            {
                e.Row.Cells[ EditSalesModifiedByFieldNumber ].Visible = false;
                e.Row.Cells[ EditSalesLastModificationDateFieldNumber ].Visible = false;
                e.Row.Cells[ EditSalesExternalSalesIdFieldNumber ].Visible = false;
                e.Row.Cells[ EditSalesSalesIdFieldNumber ].Visible = false;
                e.Row.Cells[ EditSalesIsNewFieldNumber ].Visible = false;
            }
        }

        private bool GetConfirmationMessageResults()
        {
            bool bConfirmationResults = false;
            //string confirmationResultsString = "";

            //HtmlInputHidden confirmationMessageResultsHiddenField = ( HtmlInputHidden )ContractFindControl( "confirmationMessageResults" );
            //if( confirmationMessageResultsHiddenField != null )
            //{
            //    confirmationResultsString = confirmationMessageResultsHiddenField.Value;
            //    if( confirmationResultsString.Contains( "true" ) == true )
            //    {
            //        bConfirmationResults = true;
            //        confirmationMessageResultsHiddenField.Value = "false";
            //    }
            //}

            return ( bConfirmationResults );
        }

  

        protected void InitiateEditModeForEditSales( int editIndex )
        {
            EditSalesGridView.EditIndex = editIndex;

            // select the edited item also
            if( EditSalesGridView.InsertRowActive == true )
            {
                SetEditSalesGridViewSelectedItem( editIndex, true ); // scroll to new row
            }
            else
            {
                SetEditSalesGridViewSelectedItem( editIndex, false );
            }

            EditSalesGridView.DataBind(); 

            // disable appropriate buttons for the selected row
            SetEnabledEditSalesControlsDuringEdit( EditSalesGridView, editIndex, false );

            // disable the non-edit controls before going into edit mode
            EnableControlsForEditSalesEditMode( false );

        }

        protected void SetEditSalesGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {

            if( selectedItemIndex < EditSalesGridView.Rows.Count )
            {

                // save for postback
                Session[ "EditSalesGridViewSelectedIndex" ] = selectedItemIndex;

                // set the row selected
                EditSalesGridView.SelectedIndex = selectedItemIndex;

                // tell the client
                if( bIncludeScroll == true )
                {
                    ScrollToSelectedItem();
                }

                ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            }
        }

        protected void ScrollToSelectedItem()
        {

            int rowIndex = EditSalesGridView.SelectedIndex + 1;

            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( EDITSALESGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( EDITSALESGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string restoreEditSalesGridSelectionScript = String.Format( "RestoreEditSalesGridSelection( {0},{1} );", rowPosition, rowIndex );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestoreEditSalesGridSelectionScript", restoreEditSalesGridSelectionScript, true ); // runs after controls established
        }

        protected void HighlightEditSalesRow( int itemIndex )
        {
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            if( EditSalesGridView.HasData() == true )
            {
                GridViewRow row = EditSalesGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = EditSalesGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = EditSalesGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setEditSalesHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveEditSalesHighlightingScript", preserveHighlightingScript, true );
            }
        }

        protected void RestoreEditSalesGridViewSelectedItem()
        {
            if( Session[ "EditSalesGridViewSelectedIndex" ] == null )
                return;

            EditSalesGridView.SelectedIndex = int.Parse( Session[ "EditSalesGridViewSelectedIndex" ].ToString() );
        }

        protected void SetEnabledEditSalesControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )
        {
            gv.SetVisibleControlsForCell( rowIndex, EditSalesEditButtonFieldNumber, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, EditSalesEditButtonFieldNumber, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, EditSalesEditButtonFieldNumber, "CancelButton", !bEnabled );
        }

        // disable non-edit controls before going into edit mode
        protected void EnableControlsForEditSalesEditMode( bool bEnabled )
        {
  
        }

 
        private bool ValidateSalesBeforeUpdate( GridView editSalesGridView, int rowIndex, ref string validationMessage )
        {
            bool bIsValid = true;
            validationMessage = "";

            string VASalesString = editSalesGridView.GetStringValueFromSelectedControl( rowIndex, EditSalesVASalesFieldNumber, 0, false, "VASalesTextBox" );
            string OGASalesString = editSalesGridView.GetStringValueFromSelectedControl( rowIndex, EditSalesOGASalesFieldNumber, 0, false, "OGASalesTextBox" );
            string SLGSalesString = editSalesGridView.GetStringValueFromSelectedControl( rowIndex, EditSalesSLGSalesFieldNumber, 0, false, "SLGSalesTextBox" );
 
            decimal VASales = 0;
            decimal OGASales = 0;
            decimal SLGSales = 0;

            try
            {
                VASales = CMGlobals.GetMoneyFromString( VASalesString, "VA Sales" );
                OGASales = CMGlobals.GetMoneyFromString( OGASalesString, "OGA Sales" );
                SLGSales = CMGlobals.GetMoneyFromString( OGASalesString, "S/L/C Sales" );
            }
            catch( Exception ex )
            {
                validationMessage = ex.Message;
                bIsValid = false;
            }

            return ( bIsValid );
        }

        // records to update have valid Id's and records to insert are flagged
        private int UpdateInsertSales( GridView editSalesGridView, int rowIndex )
        {
            int updatedRowIndex = -1;

            _salesIdParameter.DefaultValue = editSalesGridView.GetRowIdFromSelectedIndex( rowIndex, 1 ).ToString();

            string externalSalesId = editSalesGridView.GetRowIdStringFromSelectedIndex( rowIndex, 0 ).ToString();
            bool bIsNew = false;
            if( externalSalesId.Substring( 0, 1 ).CompareTo( "T" ) == 0 )
                bIsNew = true;
            _isNewParameter.DefaultValue = bIsNew.ToString();
            _SINParameter.DefaultValue = editSalesGridView.GetStringValueFromSelectedControl( rowIndex, EditSalesSINFieldNumber, 0, false, "SINLabel" );
            _commentsParameter.DefaultValue = editSalesGridView.GetStringValueFromSelectedControl( rowIndex, EditSalesCommentsFieldNumber, 0, false, "salesCommentsTextBox" );

            string VASalesString = editSalesGridView.GetStringValueFromSelectedControl( rowIndex, EditSalesVASalesFieldNumber, 0, false, "VASalesTextBox" );
            string OGASalesString = editSalesGridView.GetStringValueFromSelectedControl( rowIndex, EditSalesOGASalesFieldNumber, 0, false, "OGASalesTextBox" );
            string SLGSalesString = editSalesGridView.GetStringValueFromSelectedControl( rowIndex, EditSalesSLGSalesFieldNumber, 0, false, "SLGSalesTextBox" );
 
            _VASalesParameter.DefaultValue = CMGlobals.GetMoneyFromString( VASalesString, "VA Sales" ).ToString();
            _OGASalesParameter.DefaultValue = CMGlobals.GetMoneyFromString( OGASalesString, "OGA Sales" ).ToString();
            _SLGSalesParameter.DefaultValue = CMGlobals.GetMoneyFromString( SLGSalesString, "S/L/C Sales" ).ToString();

            try
            {
                _editSalesDataSource.Update();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( this.Page, ex );
            }

            editSalesGridView.EditIndex = -1; // done with edit
            editSalesGridView.DataBind();
            SalesWereModified = true;  // for update of parent window
            _editContractSalesHeaderContent.IsNewQuarter = false;  // after first save, switch from new to edit mode

            if( Session[ "LastUpdatedEditSalesId" ] != null )
            {
                int lastUpdatedEditSalesId = int.Parse( Session[ "LastUpdatedEditSalesId" ].ToString() );
                updatedRowIndex = editSalesGridView.GetRowIndexFromId( lastUpdatedEditSalesId, 1 );

                SetEditSalesGridViewSelectedItem( updatedRowIndex, false );

                // bind to select
                editSalesGridView.DataBind();
            }

            // enable appropriate buttons for the selected row
            SetEnabledEditSalesControlsDuringEdit( editSalesGridView, updatedRowIndex, true );

            EnableControlsForEditSalesEditMode( true );

            return ( updatedRowIndex );

        }

        void _editSalesDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string salesIdString = "";

            if( e.Exception == null )
            {
                if( e.Command.Parameters[ "@NewSalesId" ].Value != null )
                {
                    salesIdString = e.Command.Parameters[ "@NewSalesId" ].Value.ToString();

                    if( salesIdString.Length > 0 )
                    {
                        int salesId;
                        salesId = int.Parse( salesIdString );
                        Session[ "LastUpdatedEditSalesId" ] = salesId;
                    }
                }
            }
        }

        protected void EditSalesHeaderFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "EditSalesHeaderFormView" );

            EditSalesHeaderFormView.Visible = bVisible;
            EditSalesHeaderFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "EditSalesHeaderFormView" );

            if( _editContractSalesHeaderContent.IsNewQuarter == true )
            {
                Label EditSalesHeaderFormViewHeaderLabel3 = ( Label )EditSalesHeaderFormView.FindControl( "EditSalesHeaderFormViewHeaderLabel3" );
                if( EditSalesHeaderFormViewHeaderLabel3 != null )
                {
                    EditSalesHeaderFormViewHeaderLabel3.Visible = false;
                }

                if( EditSalesYearQuarterDropDownList != null )
                {
                    EditSalesYearQuarterDropDownList.Visible = true;
                    EditSalesYearQuarterLabel.Visible = true;
                }
            }
            else
            {
                Label EditSalesHeaderFormViewHeaderLabel3 = ( Label )EditSalesHeaderFormView.FindControl( "EditSalesHeaderFormViewHeaderLabel3" );
                if( EditSalesHeaderFormViewHeaderLabel3 != null )
                {
                    EditSalesHeaderFormViewHeaderLabel3.Visible = true;
                }

                if( EditSalesYearQuarterDropDownList != null )
                {
                    EditSalesYearQuarterDropDownList.Visible = false;
                    EditSalesYearQuarterLabel.Visible = false;
                }
                
            }
        }

        protected void EditSalesGridPanel_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsControlVisible( "EditSalesGridPanel" );

            EditSalesGridPanel.Visible = bVisible;
            EditSalesGridPanel.Enabled = documentControlPresentation.IsControlEnabled( "EditSalesGridPanel" );
        }

        protected void EditSalesScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "EditSalesErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "EditSalesErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            EditSalesScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

        protected void CloseEditSalesButton_OnClick( object sender, EventArgs e )
        {
            CloseWindow( SalesWereModified );
        }

        private void CloseWindow( bool bWithRefresh )
        {
            string closeWindowScript = "CloseWindow( \"false\" );";

            if( bWithRefresh == true )
                closeWindowScript = "CloseWindow( \"true\" );";
            else
                closeWindowScript = "CloseWindow( \"false\" );";

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "CloseWindowScript", closeWindowScript, true );
        }
    }
}