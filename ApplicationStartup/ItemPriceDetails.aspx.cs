using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using TextBox = System.Web.UI.WebControls.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ItemPriceDetails : System.Web.UI.Page
    {
        private const int TIEREDPRICEGRIDVIEWROWHEIGHTESTIMATE = 47;

        private ItemPriceDetailsWindowParms _itemPriceDetailsWindowParms = null;

        private DocumentDataSource _itemPriceDetailsDataSource = null;
        private DocumentDataSource _tieredPriceDataSource = null;

        private Parameter _modificationStatusIdParameter = null;

        // select and update parameters
        private Parameter _itemPriceIdParameter = null;
        private Parameter _itemPriceHistoryIdParameter = null;
        private Parameter _isPriceFromHistoryParameter = null;

        // update parameters

        private Parameter _trackingCustomerRatioParameter = null;
        private Parameter _trackingCustomerPriceParameter = null;
        private Parameter _trackingCustomerNameParameter = null;
        private Parameter _trackingCustomerFOBTermsParameter = null;

        private Parameter _itemTieredPriceIdParameter = null;
        private Parameter _itemTieredPriceIdForInsertParameter = null;

        private Parameter _tieredPriceStartDateParameter = null;
        private Parameter _tieredPriceStopDateParameter = null;
        private Parameter _tieredPriceParameter = null;   // decimal(18,2)
              
        private Parameter _tierSequenceParameter = null;
        private Parameter _tierCriteriaParameter = null; // nvarchar(255) 
        private Parameter _tierMinimumValueParameter = null; // int
        private Parameter _withAddParameter = null;
        private Parameter _contractExpirationDateParameter = null;

        private ValidationErrorManager _validationErrorManager = null;

        protected void Page_Load( object sender, EventArgs e )
        {
            
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }
            
            if( Session[ "ItemPriceDetailsWindowParms" ] != null )
            {
                _itemPriceDetailsWindowParms = ( ItemPriceDetailsWindowParms )Session[ "ItemPriceDetailsWindowParms" ];
            }
            else
            {
                throw new Exception( "Error: ItemId not available for details presentation." );
            }

            InitSharedParms();
            LoadItemPriceDetails();

            if( Page.IsPostBack == false )
            {
                BindItemPriceDetails();
            }

            AddClientCloseEvent();

            _validationErrorManager = new ValidationErrorManager( DocumentTypes.Contract );

            //if( Session[ "CurrentDocument" ] != null )
            //{
            //    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
            //    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PBMPrices ) == true )
            //    {
            //        ItemPriceDetailsFormView.DefaultMode = FormViewMode.Edit;
            //    }
            //    else
            //    {
            //        ItemPriceDetailsFormView.DefaultMode = FormViewMode.ReadOnly;
            //    }
            //}

            LoadTieredPrices();
            SetTieredPriceDataSource();

            if( Page.IsPostBack == false )
            {
                BindTieredPriceGrid();
            }
        }

  

        private void DisableTextBox( string textBoxName )
        {
            System.Web.UI.WebControls.TextBox tb = null;
            tb = ( System.Web.UI.WebControls.TextBox )ItemPriceDetailsFormView.FindControl( textBoxName );

            if( tb != null )
            {
                tb.Enabled = false;
            }
        }

        private void ClearSessionVariables()
        {
            Session[ "ItemPriceDetailsDataSource" ] = null;
            Session[ "TieredPriceDataSource" ] = null;
            Session[ "TieredPriceGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedItemTieredPriceId" ] = null;
            Session[ "LastInsertedItemTieredPriceId" ] = null;
        }

        private void InitSharedParms()
        {
            if( Session[ "ModificationStatusIdParameter" ] == null )
            {
                _modificationStatusIdParameter = new Parameter( "ModificationStatusId", TypeCode.Int32 );
                _modificationStatusIdParameter.DefaultValue = "-1"; // $$$
            }
            else
            {
                _modificationStatusIdParameter = ( Parameter )Session[ "ModificationStatusIdParameter" ];
            }
        }

        private void AddClientCloseEvent()
        {
            string closeFunctionText = "CloseWindow( \"true\" );";
            CancelItemPriceDetailsButton.Attributes.Add( "onclick", closeFunctionText );
        }

        protected void ItemPriceDetailsScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "ItemPriceDetailsErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "ItemPriceDetailsErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            ItemPriceDetailsScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

        private void SetItemPriceDetailsHeaderInfo()
        {
            DataRowView currentRow = ( DataRowView )ItemPriceDetailsFormView.DataItem;

            SelectedItemHeader.HeaderTitle = "Price Details";
            SelectedItemHeader.ItemDescription = _itemPriceDetailsWindowParms.ItemDescription;     
        }

     
        // capture Id's which are needed as future parameter values on save
        private void CaptureExtraIds()
        {
            DataRowView currentRow = ( DataRowView )ItemPriceDetailsFormView.DataItem;
   //         Session[ "LastPriceIdForSelectedPriceDetails" ] = int.Parse( currentRow[ "PriceId" ].ToString() );
        }

     
        private void LoadItemPriceDetails()
        {
            if( Session[ "ItemPriceDetailsDataSource" ] == null )
            {
                _itemPriceDetailsDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.Item, true );
                _itemPriceDetailsDataSource.ID = "ItemPriceDetailsDataSource";
                _itemPriceDetailsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _itemPriceDetailsDataSource.SelectCommand = "GetMedSurgItemPriceDetails";
                _itemPriceDetailsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                
                _itemPriceDetailsDataSource.UpdateCommand = "UpdateMedSurgItemPriceDetails";
                _itemPriceDetailsDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;

                _itemPriceDetailsDataSource.SetEventOwnerName( "ItemPriceDetails" );

                SetUpItemPriceDetailsParameters();  

                _itemPriceIdParameter.DefaultValue = _itemPriceDetailsWindowParms.SelectedPriceId.ToString();
                _itemPriceHistoryIdParameter.DefaultValue = _itemPriceDetailsWindowParms.SelectedPriceHistoryId.ToString();
                _isPriceFromHistoryParameter.DefaultValue = _itemPriceDetailsWindowParms.IsFromHistory.ToString();

                AddParametersToDataSource( _itemPriceDetailsDataSource );

            }
            else
            {
                _itemPriceDetailsDataSource = ( DocumentDataSource )Session[ "ItemPriceDetailsDataSource" ];

                _itemPriceDetailsDataSource.RestoreDelegatesAfterDeserialization( this, "ItemPriceDetails" );

                ReloadItemPriceDetailsParameters( _itemPriceDetailsDataSource );
            }

            ItemPriceDetailsFormView.DataSource = _itemPriceDetailsDataSource;

        }

        private void BindItemPriceDetails()
        {
            try
            {
                ItemPriceDetailsFormView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }
        }

        private void BindTieredPriceGrid()
        {
            try
            {
                GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );
                tieredPriceGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }
        }

        //private void UpdateTieredPriceGrid()
        //{
        //    UpdatePanel tieredPriceGridViewUpdatePanel = ( UpdatePanel )ItemPriceDetailsFormView.FindControl( "TieredPriceGridViewUpdatePanel" );
        //    tieredPriceGridViewUpdatePanel.Update();
        //}

        protected void ItemPriceDetailsFormView_OnDataBound( Object sender, EventArgs e )
        {
            SetItemPriceDetailsHeaderInfo();

            CaptureExtraIds();

            ItemPriceDetailsUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        private void SetUpItemPriceDetailsParameters()
        {
            // select
            _itemPriceIdParameter = new Parameter( "ItemPriceId", TypeCode.Int32 );
            _itemPriceHistoryIdParameter = new Parameter( "ItemPriceHistoryId", TypeCode.Int32 );
            _isPriceFromHistoryParameter = new Parameter( "IsFromHistory", TypeCode.Boolean );

            // update parameters
            _trackingCustomerPriceParameter = new Parameter( "TrackingCustomerPrice", TypeCode.Decimal );
            _trackingCustomerRatioParameter = new Parameter( "TrackingCustomerRatio", TypeCode.String );           
            _trackingCustomerNameParameter = new Parameter( "TrackingCustomerName", TypeCode.String );
            _trackingCustomerFOBTermsParameter = new Parameter( "TrackingCustomerFOBTerms", TypeCode.String );

        }

        private void AddParametersToDataSource( DocumentDataSource itemPriceDetailsDataSource )
        {
            itemPriceDetailsDataSource.SelectParameters.Add( _itemPriceIdParameter );
            itemPriceDetailsDataSource.SelectParameters.Add( _itemPriceHistoryIdParameter );
            itemPriceDetailsDataSource.SelectParameters.Add( _isPriceFromHistoryParameter );

            itemPriceDetailsDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
            itemPriceDetailsDataSource.UpdateParameters.Add( _itemPriceIdParameter );
            
            itemPriceDetailsDataSource.UpdateParameters.Add( _trackingCustomerPriceParameter );
            itemPriceDetailsDataSource.UpdateParameters.Add( _trackingCustomerRatioParameter );
            itemPriceDetailsDataSource.UpdateParameters.Add( _trackingCustomerNameParameter );
            itemPriceDetailsDataSource.UpdateParameters.Add( _trackingCustomerFOBTermsParameter );

        }

        private void ReloadItemPriceDetailsParameters( DocumentDataSource itemPriceDetailsDataSource )
        {
            _itemPriceIdParameter = itemPriceDetailsDataSource.SelectParameters[ "ItemPriceId" ];
            _itemPriceHistoryIdParameter = itemPriceDetailsDataSource.SelectParameters[ "ItemPriceHistoryId" ];
            _isPriceFromHistoryParameter = itemPriceDetailsDataSource.SelectParameters[ "IsFromHistory" ];

            _modificationStatusIdParameter = itemPriceDetailsDataSource.UpdateParameters[ "ModificationStatusId" ];
            _trackingCustomerPriceParameter = itemPriceDetailsDataSource.UpdateParameters[ "TrackingCustomerPrice" ];
            _trackingCustomerRatioParameter = itemPriceDetailsDataSource.UpdateParameters[ "TrackingCustomerRatio" ];            
            _trackingCustomerNameParameter = itemPriceDetailsDataSource.UpdateParameters[ "TrackingCustomerName" ];
            _trackingCustomerFOBTermsParameter = itemPriceDetailsDataSource.UpdateParameters[ "TrackingCustomerFOBTerms" ];
        }



        protected void UpdateItemPriceDetailsButton_OnClick( object sender, EventArgs e )
        {
            string validationMessage = "";

            try
            {
                if( ValidatePriceDetails( ref validationMessage ) == true )
                {
                    GetUpdateParameterValues();
                    _itemPriceDetailsDataSource.Update();
                    ItemPriceDetailsFormView.DataBind();

                    SetTieredPriceDataSource();
                    BindTieredPriceGrid();

                    // allow the update postback to occur
                    UpdatePanelEventProxy outerFormViewWasSavedUpdatePanelEventProxy = ( UpdatePanelEventProxy )ItemPriceDetailsFormView.FindControl( "OuterFormViewWasSavedUpdatePanelEventProxy" );
                    outerFormViewWasSavedUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                }
                else
                {
                    MsgBox.Alert( validationMessage );
                }
            }
            catch( Exception ex )
            {
                MsgBox.ShowError( ex );
            }
        }

        private bool ValidatePriceDetails( ref string validationMessage )
        {
            bool bSuccess = true;
           
            string trackingCustomerPrice = "";
            string trackingCustomerRatio = "";
            string trackingCustomerName = "";
            string trackingCustomerFOBTerms = "";

            try
            {
                FormView ItemPriceDetailsFormView = ( FormView )ItemPriceDetailsForm.FindControl( "ItemPriceDetailsFormView" );
                if( ItemPriceDetailsFormView != null )
                {
                    if( ItemPriceDetailsFormView.CurrentMode == FormViewMode.Edit )
                    {
                        // TrackingCustomerRatioTextBox
                        // TrackingCustomerPriceTextBox
                        // TrackingCustomerNameTextBox
                        // TrackingCustomerFOBTermsTextBox

                        TextBox TrackingCustomerRatioTextBox = ( TextBox )ItemPriceDetailsFormView.FindControl( "TrackingCustomerRatioTextBox" );
                        if( TrackingCustomerRatioTextBox != null )
                        {
                            trackingCustomerRatio = TrackingCustomerRatioTextBox.Text;

                            if( trackingCustomerRatio.Length == 0 )
                            {
                                if( _itemPriceDetailsWindowParms.IsService == false )
                                {
                         //       _validationErrorManager.AppendValidationError( "Tracking Customer Ratio is required.", false );
                                }
                            }
                        }
                        
                        TextBox TrackingCustomerPriceTextBox = ( TextBox )ItemPriceDetailsFormView.FindControl( "TrackingCustomerPriceTextBox" );
                        if( TrackingCustomerPriceTextBox != null )
                        {
                            trackingCustomerPrice = TrackingCustomerPriceTextBox.Text;

                            decimal parseDecimal;

                            if( trackingCustomerPrice.Length > 0 )
                            {
                                if( decimal.TryParse( trackingCustomerPrice, out parseDecimal ) == false )
                                {
                                    _validationErrorManager.AppendValidationError( "The Tracking Customer Price is not a valid number.", false );
                                }
                            }
                            else
                            {
                                if( _itemPriceDetailsWindowParms.IsService == false )
                                {
                           //       _validationErrorManager.AppendValidationError( "Tracking Customer Price is required.", false );
                                }
                            }
                        }

                        TextBox TrackingCustomerNameTextBox = ( TextBox )ItemPriceDetailsFormView.FindControl( "TrackingCustomerNameTextBox" );
                        if( TrackingCustomerNameTextBox != null )
                        {
                            trackingCustomerName = TrackingCustomerNameTextBox.Text;

                            if( trackingCustomerName.Length == 0 )
                            {
                                if( _itemPriceDetailsWindowParms.IsService == false )
                                {
                                    //       _validationErrorManager.AppendValidationError( "Tracking Customer Name is required.", false );
                                }
                            }
                        }

                        TextBox TrackingCustomerFOBTermsTextBox = ( TextBox )ItemPriceDetailsFormView.FindControl( "TrackingCustomerFOBTermsTextBox" );
                        if( TrackingCustomerFOBTermsTextBox != null )
                        {
                            trackingCustomerFOBTerms = TrackingCustomerFOBTermsTextBox.Text;

                            if( trackingCustomerFOBTerms.Length == 0 )
                            {
                                if( _itemPriceDetailsWindowParms.IsService == false )
                                {
                                    //       _validationErrorManager.AppendValidationError( "Tracking Customer FOB Terms are required.", false );
                                }
                            }
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                bSuccess = false;
                validationMessage = string.Format( "The following exception was encountered validating the price details: {0}", ex.Message );
            }

            if( _validationErrorManager.HasErrors() == true )
            {
                bSuccess = false;
                _validationErrorManager.OutputSpacing = 2;
                validationMessage = _validationErrorManager.ToString();
            }

            return ( bSuccess );
        }

        private void GetUpdateParameterValues()
        {
            FormView ItemPriceDetailsFormView = ( FormView )ItemPriceDetailsForm.FindControl( "ItemPriceDetailsFormView" );
            if( ItemPriceDetailsFormView != null )
            {
                if( ItemPriceDetailsFormView.CurrentMode == FormViewMode.Edit )
                {
                    // get extra id's which were saved during last bind event
     //               int priceId = ( int )Session[ "LastPriceIdForSelectedPriceDetails" ];

                    // save the id's parameters for the update
     //               _priceIdParameter.DefaultValue = priceId.ToString();

                    TextBox TrackingCustomerRatioTextBox = ( TextBox )ItemPriceDetailsFormView.FindControl( "TrackingCustomerRatioTextBox" );
                    if( TrackingCustomerRatioTextBox != null )
                        _trackingCustomerRatioParameter.DefaultValue = TrackingCustomerRatioTextBox.Text;

                    TextBox TrackingCustomerPriceTextBox = ( TextBox )ItemPriceDetailsFormView.FindControl( "TrackingCustomerPriceTextBox" );
                    if( TrackingCustomerPriceTextBox != null )
                        if( TrackingCustomerPriceTextBox.Text.Length > 0 )
                            _trackingCustomerPriceParameter.DefaultValue = CMGlobals.GetMoneyFromString( TrackingCustomerPriceTextBox.Text, "Tracking Customer Price" ).ToString();
                        else
                            _trackingCustomerPriceParameter.DefaultValue = "";

                    TextBox TrackingCustomerNameTextBox = ( TextBox )ItemPriceDetailsFormView.FindControl( "TrackingCustomerNameTextBox" );
                    if( TrackingCustomerNameTextBox != null )
                        _trackingCustomerNameParameter.DefaultValue = TrackingCustomerNameTextBox.Text;

                    TextBox TrackingCustomerFOBTermsTextBox = ( TextBox )ItemPriceDetailsFormView.FindControl( "TrackingCustomerFOBTermsTextBox" );
                    if( TrackingCustomerFOBTermsTextBox != null )
                        _trackingCustomerFOBTermsParameter.DefaultValue = TrackingCustomerFOBTermsTextBox.Text;

                }
            }
        }

        protected void CancelItemPriceDetailsButton_OnClick( object sender, EventArgs e )
        {

        }

        protected void LastModificationDateLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label LastModificationDateLabel = ( Label )sender;

            FormView containingFormView = ( FormView )LastModificationDateLabel.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            string lastModificationDate = "null";

            if( currentRow[ "LastModificationDate" ] != DBNull.Value )
            {
                lastModificationDate = currentRow[ "LastModificationDate" ].ToString();
            }

            LastModificationDateLabel.Text = String.Format( "Last Modified On: {0:d}", lastModificationDate );
        }

        protected void LastModifedByLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label LastModifedByLabel = ( Label )sender;

            FormView containingFormView = ( FormView )LastModifedByLabel.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            string lastModifiedBy = "null";

            if( currentRow[ "LastModifiedBy" ] != DBNull.Value )
            {
                lastModifiedBy = currentRow[ "LastModifiedBy" ].ToString();
            }

            LastModifedByLabel.Text = String.Format( "Last Modified By: {0}", lastModifiedBy );
        }

        protected void LastModificationTypeLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label LastModificationTypeLabel = ( Label )sender;
            FormView containingFormView = ( FormView )LastModificationTypeLabel.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            string lastModificationType = "";
            string lastModificationTypeDescription = "null";

            if( currentRow[ "LastModificationType" ] != DBNull.Value )
            {
                lastModificationType = currentRow[ "LastModificationType" ].ToString();

                lastModificationTypeDescription = CMGlobals.GetMedSurgLastModificationTypeDescription( lastModificationType );
            }

            LastModificationTypeLabel.Text = String.Format( "Last Modification Type: {0}", lastModificationTypeDescription );
        }

        protected string MultilineText( string[] textArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < textArray.Count(); i++ )
            {
                sb.AppendLine( textArray[ i ] );
                if( i < textArray.Count() - 1 )
                    sb.Append( "<br>" );
            }

            return ( sb.ToString() );
        }


        //protected void ExcludeFromExportCheckBox_OnDataBinding( object sender, EventArgs e )
        //{
        //    CheckBox excludeFromExportCheckBox = ( CheckBox )sender;

        //    FormView containingFormView = ( FormView )excludeFromExportCheckBox.NamingContainer;

        //    DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

        //    bool bIsChecked = false;

        //    if( currentRow[ "ExcludeFromExport" ] != DBNull.Value )
        //    {
        //        bIsChecked = bool.Parse( currentRow[ "ExcludeFromExport" ].ToString() );
        //    }

        //    excludeFromExportCheckBox.Checked = bIsChecked;
        //}

        //protected void SubItemIdentifierDropDownList_OnDataBound( object sender, EventArgs e )
        //{
        //    DropDownList subItemIdentifierDropDownList = ( DropDownList )sender;

        //    FormView containingFormView = ( FormView )subItemIdentifierDropDownList.NamingContainer;

        //    DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

        //    string selectedHistoricalNValue = "";

        //    if( currentRow[ "HistoricalNValue" ] != DBNull.Value )
        //    {
        //        selectedHistoricalNValue = currentRow[ "HistoricalNValue" ].ToString();

        //        ListItem listItem = subItemIdentifierDropDownList.Items.FindByValue( selectedHistoricalNValue );
        //        if( listItem != null )
        //            listItem.Selected = true;
        //    }

        //}

        // the details form view elements are only applicable to FSS
        protected void ItemPriceDetailsFormView_OnPreRender( object sender, EventArgs e )
        {
            FormView itemPriceDetailsFormView = ( FormView )sender;

            if( itemPriceDetailsFormView != null )
            {
                TableHeaderCell blankHeaderCell0 = ( TableHeaderCell )itemPriceDetailsFormView.FindControl( "BlankHeaderCell0" );
                TableHeaderCell trackingCustomerRatioCell = ( TableHeaderCell )itemPriceDetailsFormView.FindControl( "TrackingCustomerRatioCell" );
                TableHeaderCell trackingCustomerPriceCell = ( TableHeaderCell )itemPriceDetailsFormView.FindControl( "TrackingCustomerPriceCell" );
                TableHeaderCell trackingCustomerNameCell = ( TableHeaderCell )itemPriceDetailsFormView.FindControl( "TrackingCustomerNameCell" );
                TableHeaderCell trackingCustomerFOBTermsCell = ( TableHeaderCell )itemPriceDetailsFormView.FindControl( "TrackingCustomerFOBTermsCell" );
                TableCell blankDataCell0 = ( TableCell )itemPriceDetailsFormView.FindControl( "BlankDataCell0" );
                TableCell trackingCustomerRatioDataCell = ( TableCell )itemPriceDetailsFormView.FindControl( "TrackingCustomerRatioDataCell" );
                TableCell trackingCustomerPriceDataCell = ( TableCell )itemPriceDetailsFormView.FindControl( "TrackingCustomerPriceDataCell" );
                TableCell trackingCustomerNameDataCell = ( TableCell )itemPriceDetailsFormView.FindControl( "TrackingCustomerNameDataCell" );
                TableCell trackingCustomerFOBTermsDataCell = ( TableCell )itemPriceDetailsFormView.FindControl( "TrackingCustomerFOBTermsDataCell" );

                if( _itemPriceDetailsWindowParms.IsPriceDetailsEditable == false || _itemPriceDetailsWindowParms.IsFromHistory == true ||
                            _itemPriceDetailsWindowParms.IsBPA == true || _itemPriceDetailsWindowParms.IsNational == true || _itemPriceDetailsWindowParms.IsBOA == true )
                {
                    Button updateItemPriceDetailsButton = null;
                    updateItemPriceDetailsButton = ( Button )ItemPriceDetailsForm.FindControl( "UpdateItemPriceDetailsButton" );
                    if( updateItemPriceDetailsButton != null )
                    {
                        updateItemPriceDetailsButton.Enabled = false;
                    }

                    Button addTieredPriceButton = null;
                    addTieredPriceButton = ( Button )ItemPriceDetailsFormView.FindControl( "AddTieredPriceButton" );
                    if( addTieredPriceButton != null )
                    {
                        addTieredPriceButton.Enabled = false;
                    }

                    DisableTextBox( "TrackingCustomerRatioTextBox" );
                    DisableTextBox( "TrackingCustomerPriceTextBox" );
                    DisableTextBox( "TrackingCustomerNameTextBox" );
                    DisableTextBox( "TrackingCustomerFOBTermsTextBox" );

                    //CheckBox excludeFromExportCheckBox = null;
                    //excludeFromExportCheckBox = ( CheckBox )ItemPriceDetailsFormView.FindControl( "ExcludeFromExportCheckBox" );
                    //if( excludeFromExportCheckBox != null )
                    //    excludeFromExportCheckBox.Enabled = false;



                    if( _itemPriceDetailsWindowParms.IsBPA == true || _itemPriceDetailsWindowParms.IsNational == true || _itemPriceDetailsWindowParms.IsBOA == true )
                    {
                        if( trackingCustomerRatioCell != null )
                        {
                            trackingCustomerRatioCell.Visible = false;
                        }
                        if( trackingCustomerPriceCell != null )
                        {
                            trackingCustomerPriceCell.Visible = false;
                        }
                        if( trackingCustomerNameCell != null )
                        {
                            trackingCustomerNameCell.Visible = false;
                        }
                        if( trackingCustomerFOBTermsCell != null )
                        {
                            trackingCustomerFOBTermsCell.Visible = false;
                        }
                        if( trackingCustomerRatioDataCell != null )
                        {
                            trackingCustomerRatioDataCell.Visible = false;
                        }
                        if( trackingCustomerPriceDataCell != null )
                        {
                            trackingCustomerPriceDataCell.Visible = false;
                        }
                        if( trackingCustomerNameDataCell != null )
                        {
                            trackingCustomerNameDataCell.Visible = false;
                        }
                        if( trackingCustomerFOBTermsDataCell != null )
                        {
                            trackingCustomerFOBTermsDataCell.Visible = false;
                        }

                        // expand the blank cell to center the grid
                        if( blankHeaderCell0 != null )
                        {
                            blankHeaderCell0.Width = new Unit( "15%" );
                        }

                        if( blankDataCell0 != null )
                        {
                            blankDataCell0.Width = new Unit( "15%" );
                        }
                    }
                    else
                    {
                        if( blankHeaderCell0 != null )
                        {
                            blankHeaderCell0.Width = new Unit( "40px" );
                        }

                        if( blankDataCell0 != null )
                        {
                            blankDataCell0.Width = new Unit( "40px" );
                        }
                    }
                }
                else
                {
                    if( blankHeaderCell0 != null )
                    {
                        blankHeaderCell0.Width = new Unit( "40px" );
                    }

                    if( blankDataCell0 != null )
                    {
                        blankDataCell0.Width = new Unit( "40px" );
                    }
                }
            }
        }

#region TieredPriceGrid

        private void LoadTieredPrices()
        {
            if( Page.Session[ "TieredPriceDataSource" ] == null )
            {
                _tieredPriceDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.Item, true );
                _tieredPriceDataSource.ID = "TieredPriceDataSource";
                _tieredPriceDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _tieredPriceDataSource.SelectCommand = "SelectTieredPricesForItemPrice";

                _tieredPriceDataSource.UpdateCommand = "UpdateTieredPriceForItemPrice";

                _tieredPriceDataSource.InsertCommand = "InsertTieredPriceForItemPrice";
                _tieredPriceDataSource.SetEventOwnerName( "TieredPrice" );
                _tieredPriceDataSource.Inserted += new SqlDataSourceStatusEventHandler( _tieredPriceDataSource_Inserted );

                _tieredPriceDataSource.Updated += new SqlDataSourceStatusEventHandler( _tieredPriceDataSource_Updated );

                _tieredPriceDataSource.DeleteCommand = "DeleteTieredPriceForItemPrice";

                CreateTieredPriceDataSourceParameters();

                _tieredPriceDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _tieredPriceDataSource.SelectParameters.Add( _withAddParameter );
                _withAddParameter.DefaultValue = "false"; // not adding
                _tieredPriceDataSource.SelectParameters.Add( _contractExpirationDateParameter );
                _contractExpirationDateParameter.DefaultValue = _itemPriceDetailsWindowParms.ContractExpirationDate.ToShortDateString(); 

                _tieredPriceDataSource.SelectParameters.Add( _itemPriceIdParameter );
                _tieredPriceDataSource.SelectParameters.Add( _itemPriceHistoryIdParameter );
                _tieredPriceDataSource.SelectParameters.Add( _isPriceFromHistoryParameter );
                _isPriceFromHistoryParameter.DefaultValue = _itemPriceDetailsWindowParms.IsFromHistory.ToString();

                _tieredPriceDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _tieredPriceDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _itemPriceIdParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _itemTieredPriceIdParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tieredPriceStartDateParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tieredPriceStopDateParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tieredPriceParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tierSequenceParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tierCriteriaParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tierMinimumValueParameter );

                _tieredPriceDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
                _tieredPriceDataSource.InsertParameters.Add( _modificationStatusIdParameter );
                _tieredPriceDataSource.InsertParameters.Add( _itemPriceIdParameter );               
                _tieredPriceDataSource.InsertParameters.Add( _tieredPriceStartDateParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tieredPriceStopDateParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tieredPriceParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tierSequenceParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tierCriteriaParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tierMinimumValueParameter );
                _tieredPriceDataSource.InsertParameters.Add( _itemTieredPriceIdForInsertParameter );

                _tieredPriceDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
                _tieredPriceDataSource.DeleteParameters.Add( _modificationStatusIdParameter );
                _tieredPriceDataSource.DeleteParameters.Add( _itemTieredPriceIdParameter );

                // save to session
                Page.Session[ "TieredPriceDataSource" ] = _tieredPriceDataSource;
            }
            else
            {
                _tieredPriceDataSource = ( DocumentDataSource )Page.Session[ "TieredPriceDataSource" ];

                _tieredPriceDataSource.RestoreDelegatesAfterDeserialization( this, "TieredPrice" );

                RestoreTieredPriceDataSourceParameters( _tieredPriceDataSource );
            }


        }

        private void SetTieredPriceDataSource()
        {
            GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );
            tieredPriceGridView.DataSource = _tieredPriceDataSource;
        }


        private void CreateTieredPriceDataSourceParameters()
        {

            _itemTieredPriceIdParameter = new Parameter( "ItemTieredPriceId", TypeCode.Int32 );
            _itemTieredPriceIdForInsertParameter = new Parameter( "ItemTieredPriceId", TypeCode.Int32 );
            _itemTieredPriceIdForInsertParameter.Direction = ParameterDirection.Output;

            _tieredPriceStartDateParameter = new Parameter( "TieredPriceStartDate", TypeCode.DateTime );
            _tieredPriceStopDateParameter = new Parameter( "TieredPriceStopDate", TypeCode.DateTime );
            _tieredPriceParameter = new Parameter( "Price", TypeCode.Decimal );

            _tierSequenceParameter = new Parameter( "TierSequence", TypeCode.Int32 );
            _tierCriteriaParameter = new Parameter( "TierCriteria", TypeCode.String );
            _tierMinimumValueParameter = new Parameter( "TierMinimumValue", TypeCode.Int32 );

            _withAddParameter = new Parameter( "WithAdd", TypeCode.Boolean );
            _contractExpirationDateParameter = new Parameter( "ContractExpirationDate", TypeCode.DateTime );
        }

        private void RestoreTieredPriceDataSourceParameters( DocumentDataSource tieredPriceDataSource )
        {
            _itemTieredPriceIdParameter = tieredPriceDataSource.UpdateParameters[ "ItemTieredPriceId" ];
            _itemTieredPriceIdForInsertParameter = tieredPriceDataSource.InsertParameters[ "ItemTieredPriceId" ];

            _tieredPriceStartDateParameter = tieredPriceDataSource.UpdateParameters[ "TieredPriceStartDate" ];
            _tieredPriceStopDateParameter = tieredPriceDataSource.UpdateParameters[ "TieredPriceStopDate" ];
            _tieredPriceParameter = tieredPriceDataSource.UpdateParameters[ "Price" ];

            _tierSequenceParameter = tieredPriceDataSource.UpdateParameters[ "TierSequence" ];
            _tierCriteriaParameter = tieredPriceDataSource.UpdateParameters[ "TierCriteria" ];
            _tierMinimumValueParameter = tieredPriceDataSource.UpdateParameters[ "TierMinimumValue" ];
            
            _withAddParameter = tieredPriceDataSource.SelectParameters[ "WithAdd" ];
            _contractExpirationDateParameter = tieredPriceDataSource.SelectParameters[ "ContractExpirationDate" ];
        }

        protected void TieredPriceGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightTieredPriceRow( 0 );
        }

        protected void HighlightTieredPriceRow( int itemIndex )
        {
            // uninitialized index
            if( itemIndex < 0 )
                return;

            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            if( tieredPriceGridView.HasData() == true )
            {
                GridViewRow row = tieredPriceGridView.Rows[ itemIndex ];
                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = tieredPriceGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = tieredPriceGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setTieredPriceHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveHighlightingScript", preserveHighlightingScript, true );

                // allow the highlight postback to occur 
                UpdatePanelEventProxy changeTieredPriceHighlightUpdatePanelEventProxy = ( UpdatePanelEventProxy )ItemPriceDetailsFormView.FindControl( "ChangeTieredPriceHighlightUpdatePanelEventProxy" );
                changeTieredPriceHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        protected void TieredPriceGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;

                if( e.Row.RowType == DataControlRowType.DataRow )
                {
                    if( ( ( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                        ( ( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert ) )
                    {


                    }

                    // block tiered price editing             
                    if( _itemPriceDetailsWindowParms.IsPriceDetailsEditable == false ||
                        _itemPriceDetailsWindowParms.IsFromHistory == true ||
                        _itemPriceDetailsWindowParms.IsBPA == true ||
                        _itemPriceDetailsWindowParms.IsNational == true )
                    {
                        Button editButton = ( Button )e.Row.FindControl( "EditButton" );
                        if( editButton != null )
                            editButton.Enabled = false;

                        Button removePriceButton = ( Button )e.Row.FindControl( "RemovePriceButton" );
                        if( removePriceButton != null )
                            removePriceButton.Enabled = false;
                    }                    
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void TieredPriceGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedItemTieredPriceId = -1;
            int itemIndex = -1;

            GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            if( e.CommandName.CompareTo( "RemovePrice" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemTieredPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                int newRowIndex = DeleteTieredPrice( tieredPriceGridView, itemIndex, selectedItemTieredPriceId );

                HighlightTieredPriceRow( newRowIndex );
            }

            else if( e.CommandName.CompareTo( "EditItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemTieredPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                HighlightTieredPriceRow( itemIndex );

                InitiateEditModeForTieredPrice( itemIndex );

            }
            // save update or save insert
            else if( e.CommandName.CompareTo( "SaveItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemTieredPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                string validationMessage = "";

                bool bTierOk = ValidateTieredPrice( tieredPriceGridView, itemIndex, ref validationMessage );

                if( bTierOk == true )
                {
                    // is this an insert or an update
                    int newOrUpdatedRowIndex = -1;
                    if( tieredPriceGridView.InsertRowActive == true )
                    {
                        newOrUpdatedRowIndex = InsertTieredPrice( tieredPriceGridView, itemIndex );
                    }
                    else
                    {
                        newOrUpdatedRowIndex = UpdateTieredPrice( tieredPriceGridView, itemIndex );
                    }

                    HighlightTieredPriceRow( newOrUpdatedRowIndex );
                }
                else
                {
                    MsgBox.AlertFromUpdatePanel( Page, validationMessage );
                }
            }
            // cancel update or cancel insert
            else if( e.CommandName.CompareTo( "Cancel" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemTieredPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

            }
            // can trap editing commands here if needed, prior to individual editing events
        }

        protected void AddNewTieredPriceButton_OnClick( object sender, EventArgs e )
        {
            GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            tieredPriceGridView.Insert();

            _withAddParameter.DefaultValue = "true";

            tieredPriceGridView.DataBind();

            InitiateEditModeForTieredPrice( 0 );

            // allow the update postback to occur
            UpdatePanelEventProxy insertTieredPriceButtonClickUpdatePanelEventProxy = ( UpdatePanelEventProxy )ItemPriceDetailsFormView.FindControl( "InsertTieredPriceButtonClickUpdatePanelEventProxy" );
            insertTieredPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            HighlightTieredPriceRow( 0 );

        }


        protected void TieredPriceGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForTieredPrice( e.NewEditIndex );
        }

        private void InitiateEditModeForTieredPrice( int editIndex )
        {
            GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            tieredPriceGridView.EditIndex = editIndex;

            // select the edited item also
            SetTieredPriceGridViewSelectedItem( editIndex );

            try
            {
                tieredPriceGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            // disable appropriate buttons for the selected row
            SetEnabledGridControlsDuringEdit( tieredPriceGridView, editIndex, false );
        }

        private void SetEnabledGridControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )
        {
            gv.SetEnabledControlsForCell( rowIndex, 6, bEnabled ); // remove price

            gv.SetVisibleControlsForCell( rowIndex, 0, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 0, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 0, "CancelButton", !bEnabled );
        }

        private void SetTieredPriceGridViewSelectedItem( int selectedItemIndex )
        {
            GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );
            
            // save for postback
            Session[ "TieredPriceGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            tieredPriceGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            ScrollToSelectedItem();

            // allow the update postback to occur
            UpdatePanelEventProxy insertTieredPriceButtonClickUpdatePanelEventProxy = ( UpdatePanelEventProxy )ItemPriceDetailsFormView.FindControl( "InsertTieredPriceButtonClickUpdatePanelEventProxy" );
            insertTieredPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedItem()
        {
            GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            int rowIndex = tieredPriceGridView.SelectedIndex;
            //  TableItemStyle rowStyle = ItemsGridView.RowStyle;
            //  int rowHeight = ( int )rowStyle.Height.Value;  // this value is always zero
            int rowPosition = TIEREDPRICEGRIDVIEWROWHEIGHTESTIMATE * rowIndex;

            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
            //     ScriptManager.RegisterClientScriptBlock( this.Page, this.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true );  // runs prior to control rendering
        }

        private void RestoreTieredPriceGridViewSelectedItem()
        {
            GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            tieredPriceGridView.SelectedIndex = ( int )Session[ "TieredPriceGridViewSelectedIndex" ];
        }

        protected void TieredPriceGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            GridView tieredPriceGridView = ( GridView )ItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );
            
            int cancelIndex = e.RowIndex;
            bool bInserting = tieredPriceGridView.InsertRowActive;

            // if inserting
            if( bInserting == true )
            {
                tieredPriceGridView.InsertRowActive = false; // cancels insert ( if inserting )
                _withAddParameter.DefaultValue = "false";
                tieredPriceGridView.EditIndex = -1; // cancels the edit
                tieredPriceGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledGridControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                HighlightTieredPriceRow( 0 );
            }
            else // editing existing row
            {
                tieredPriceGridView.EditIndex = -1; // cancels the edit
                tieredPriceGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledGridControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                HighlightTieredPriceRow( cancelIndex );
            }
        }

        private bool ValidateTieredPrice( GridView gv, int rowIndex, ref string validationMessage )
        {
            bool bSuccess = true;

            string tierSequenceString = "";
            string priceStartDate = "";
            string priceStopDate = "";
            string price = "";
            string minimum = "";
            string minimumValueString = "";
            int minimumValue = -1;


            try
            {
                tierSequenceString = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "tierSequenceTextBox" );                
                priceStartDate = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "priceStartDateTextBox" );
                priceStopDate = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "priceEndDateTextBox" );
                price = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "priceTextBox" ), "Tiered Price" ).ToString();
                minimum = gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "minimumTextBox" );
                minimumValueString = gv.GetStringValueFromSelectedControl( rowIndex, 5, 0, false, "minimumValueTextBox" );

                int tierSequence = -1;
                if( int.TryParse( tierSequenceString, out tierSequence ) == false )
                {
                    throw new Exception( "Tier sequence is not in the correct format. It must be numeric." );
                }

                DateTime startDate;

                if( DateTime.TryParse( priceStartDate, out startDate ) == false )
                {
                    throw new Exception( "Price Start Date is invalid." );
                }

                DateTime stopDate;

                if( DateTime.TryParse( priceStopDate, out stopDate ) == false )
                {
                    throw new Exception( "Price Stop Date is invalid." );
                }

                if( DateTime.Compare( startDate, stopDate ) >= 0 )
                {
                    throw new Exception( "Price start date must be before end date." );
                }

                decimal decimalParse;

                if( Decimal.TryParse( price, out decimalParse ) == false )
                {
                    throw new Exception( "Price must be a valid decimal value with a maximum length of 9 characters." );
                }

                if( minimum.Length > 200 )
                {
                    throw new Exception( "The decription of 'minimum' tier must be less than 200 characters in length." );
                }

                if( Int32.TryParse( minimumValueString, out minimumValue ) == false )
                {
                    throw new Exception( "The 'minimum value' tier must be numeric only. Use the minimum description field for additional text." );
                }
            }
            catch( Exception ex )
            {
                bSuccess = false;
                validationMessage = string.Format( "The following exception was encountered validating the tiered price {0}", ex.Message );
            }

            return ( bSuccess );
        }


        private int UpdateTieredPrice( GridView gv, int rowIndex )
        {
            int updatedRowIndex = -1;

            try
            {
                _itemPriceIdParameter.DefaultValue = _itemPriceDetailsWindowParms.SelectedPriceId.ToString();
                _itemTieredPriceIdParameter.DefaultValue = gv.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

                _tierSequenceParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "tierSequenceTextBox" );                
                _tieredPriceStartDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "priceStartDateTextBox" );
                _tieredPriceStopDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "priceEndDateTextBox" );
                _tieredPriceParameter.DefaultValue = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "priceTextBox" ), "Tiered Price" ).ToString();
                _tierCriteriaParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 5, 0, false, "tierCriteriaTextBox" );
                _tierMinimumValueParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 6, 0, false, "minimumValueTextBox" );

             
                _tieredPriceDataSource.Update();
              
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.EditIndex = -1; // done with edit
            //         SetItemsGridViewSelectedItem( rowIndex );
            gv.DataBind();

            if( Session[ "LastUpdatedItemTieredPriceId" ] != null )
            {
                int updatedTieredPriceId = ( int )Session[ "LastUpdatedItemTieredPriceId" ];
                updatedRowIndex = gv.GetRowIndexFromId( updatedTieredPriceId, 0 );

                SetTieredPriceGridViewSelectedItem( updatedRowIndex );

                // bind to select
                gv.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledGridControlsDuringEdit( gv, updatedRowIndex, true );
            }

            return ( updatedRowIndex );
        }



        private int InsertTieredPrice( GridView gv, int rowIndex )
        {
            int insertedRowIndex = 0;

            try
            {
                _itemPriceIdParameter.DefaultValue = _itemPriceDetailsWindowParms.SelectedPriceId.ToString();
                _tierSequenceParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "tierSequenceTextBox" );
                _tieredPriceStartDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "priceStartDateTextBox" );
                _tieredPriceStopDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "priceEndDateTextBox" );
                _tieredPriceParameter.DefaultValue = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "priceTextBox" ), "Tiered Price" ).ToString();
                _tierCriteriaParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 5, 0, false, "tierCriteriaTextBox" );
                _tierMinimumValueParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 6, 0, false, "minimumValueTextBox" );
          

                _tieredPriceDataSource.Insert();


            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.InsertRowActive = false; // done with insert
            gv.EditIndex = -1; // done with edit of new row
            _withAddParameter.DefaultValue = "false"; // done with insert

            // bind with new row
            gv.DataBind();

            if( Session[ "LastInsertedItemTieredPriceId" ] != null )
            {
                int newTieredPriceId = ( int )Session[ "LastInsertedItemTieredPriceId" ];
                insertedRowIndex = gv.GetRowIndexFromId( newTieredPriceId, 0 );

                SetTieredPriceGridViewSelectedItem( insertedRowIndex );

                // bind to select
                gv.DataBind();
            }

            return ( insertedRowIndex );
        }

        protected void _tieredPriceDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemTieredPriceIdString = e.Command.Parameters[ "@ItemTieredPriceId" ].Value.ToString();

            if( drugItemTieredPriceIdString.Length > 0 )
            {
                int drugItemTieredPriceId = int.Parse( drugItemTieredPriceIdString );
                Session[ "LastInsertedItemTieredPriceId" ] = drugItemTieredPriceId;
            }
        }

        // probably wont happen - id changing during update
        protected void _tieredPriceDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemTieredPriceIdString = e.Command.Parameters[ "@ItemTieredPriceId" ].Value.ToString();

            if( drugItemTieredPriceIdString.Length > 0 )
            {
                int drugItemTieredPriceId = int.Parse( drugItemTieredPriceIdString );
                Session[ "LastUpdatedItemTieredPriceId" ] = drugItemTieredPriceId;
            }
        }

        private int DeleteTieredPrice( GridView gv, int rowIndex, int selectedItemTieredPriceId )
        {
            // id of row to delete
            _itemTieredPriceIdParameter.DefaultValue = selectedItemTieredPriceId.ToString();

            try
            {
                _tieredPriceDataSource.Delete();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            // previous row gets focus
            if( rowIndex >= 1 )
                rowIndex--;

            SetTieredPriceGridViewSelectedItem( rowIndex );

            gv.DataBind();

            return ( rowIndex );
        }

        //protected void RemovePriceButton_DataBinding( object sender, EventArgs e )
        //{
        //    Button removePriceButton = ( Button )sender;
        //    if( removePriceButton != null )
        //        MultiLineButtonText( removePriceButton, new string[] { "Remove", "Price" } );
        //}

        private void MultiLineButtonText( Button button, string[] buttonTextArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < buttonTextArray.Count(); i++ )
            {
                sb.AppendLine( buttonTextArray[ i ] );
            }

            button.Text = sb.ToString();
        }

#endregion TieredPriceGrid

    }
}
