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

namespace VA.NAC.CM.DrugItems
{
    public partial class DrugItemPriceDetails : System.Web.UI.Page
    {
        private const int TIEREDPRICEGRIDVIEWROWHEIGHTESTIMATE = 47;

        private DrugItemPriceDetailsWindowParms _DrugItemPriceDetailsWindowParms = null;

        private DocumentDataSource _DrugItemPriceDetailsDataSource = null;
        private DocumentDataSource _tieredPriceDataSource = null;

        private Parameter _modificationStatusIdParameter = null;

        // select and update parameters
        private Parameter _contractNumberParameter = null;
        private Parameter _drugItemIdParameter = null;
        private Parameter _drugItemPriceIdParameter = null;
        private Parameter _drugItemPriceHistoryIdParameter = null;
        private Parameter _isFromHistoryParameter = null;
        private Parameter _isHistoryFromArchiveParameter = null;

        // update parameters
   //     private Parameter _priceIdParameter = null;

        private Parameter _trackingCustomerRatioParameter = null;
        private Parameter _trackingCustomerPriceParameter = null;
        private Parameter _trackingCustomerNameParameter = null;
        private Parameter _excludeFromExportParameter = null;
   //     private Parameter _historicalNValueParameter = null;

        private Parameter _drugItemTieredPriceIdParameter = null;
        private Parameter _drugItemTieredPriceIdForInsertParameter = null;
        private Parameter _tieredPriceStartDateParameter = null;
        private Parameter _tieredPriceStopDateParameter = null;
        private Parameter _tieredPriceParameter = null;   // decimal(9,2)
        private Parameter _tierMinimumParameter = null; // nvarchar(200) 
        private Parameter _tierMinimumValueParameter = null; // int
        private Parameter _withAddParameter = null;
        private Parameter _withAddSubItemParameter = null;
        private Parameter _isTieredPriceFromHistoryParameter = null;



        protected void Page_Load( object sender, EventArgs e )
        {
            
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }
            
            if( Session[ "DrugItemPriceDetailsWindowParms" ] != null )
            {
                _DrugItemPriceDetailsWindowParms = ( DrugItemPriceDetailsWindowParms )Session[ "DrugItemPriceDetailsWindowParms" ];
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

            //if( Session[ "CurrentDocument" ] != null )
            //{
            //    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
            //    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PBMPrices ) == true )
            //    {
            //        DrugItemPriceDetailsFormView.DefaultMode = FormViewMode.Edit;
            //    }
            //    else
            //    {
            //        DrugItemPriceDetailsFormView.DefaultMode = FormViewMode.ReadOnly;
            //    }
            //}

            LoadTieredPrices();
            SetTieredPriceDataSource();

            if( Page.IsPostBack == false )
            {
                BindTieredPriceGrid();
            }
        }

        // also disables if item is from history
        private void DisableControlsForReadOnly()
        {
            if( Session[ "CurrentDocument" ] != null )
            {
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmPrices ) == false || _DrugItemPriceDetailsWindowParms.IsFromHistory == true )
                {
                    Button updateItemPriceDetailsButton = null;
                    updateItemPriceDetailsButton = ( Button )DrugItemPriceDetailsForm.FindControl( "UpdateItemPriceDetailsButton" );
                    if( updateItemPriceDetailsButton != null )
                    {
                        updateItemPriceDetailsButton.Enabled = false;
                    }

                    Button addTieredPriceButton = null;
                    addTieredPriceButton = ( Button )DrugItemPriceDetailsFormView.FindControl( "AddTieredPriceButton" );
                    if( addTieredPriceButton != null )
                    {
                        addTieredPriceButton.Enabled = false;
                    }

                    DisableTextBox( "TrackingCustomerRatioTextBox" );
                    DisableTextBox( "TrackingCustomerPriceTextBox" );
                    DisableTextBox( "TrackingCustomerNameTextBox" );

                    CheckBox excludeFromExportCheckBox = null;
                    excludeFromExportCheckBox = ( CheckBox )DrugItemPriceDetailsFormView.FindControl( "ExcludeFromExportCheckBox" );
                    if( excludeFromExportCheckBox != null )
                        excludeFromExportCheckBox.Enabled = false;
                }

                // also disable tiered price addition if National or BPA
                if( currentDocument.DocumentType != CurrentDocument.DocumentTypes.FSS )
                {
                    Button addTieredPriceButton = null;
                    addTieredPriceButton = ( Button )DrugItemPriceDetailsFormView.FindControl( "AddTieredPriceButton" );
                    if( addTieredPriceButton != null )
                    {
                        addTieredPriceButton.Enabled = false;
                    }
                }
            }
        }

        private void DisableTextBox( string textBoxName )
        {
            System.Web.UI.WebControls.TextBox tb = null;
            tb = ( System.Web.UI.WebControls.TextBox )DrugItemPriceDetailsFormView.FindControl( textBoxName );

            if( tb != null )
            {
                tb.Enabled = false;
            }
        }

        private void ClearSessionVariables()
        {
            Session[ "DrugItemPriceDetailsDataSource" ] = null;
            Session[ "TieredPriceDataSource" ] = null;
            Session[ "TieredPriceGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedDrugItemTieredPriceId" ] = null;
            Session[ "LastInsertedDrugItemTieredPriceId" ] = null;
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

        protected void DrugItemPriceDetailsScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "DrugItemPriceDetailsErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "DrugItemPriceDetailsErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            DrugItemPriceDetailsScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

        private void SetItemPriceDetailsHeaderInfo()
        {
            DataRowView currentRow = ( DataRowView )DrugItemPriceDetailsFormView.DataItem;

            SelectedDrugItemHeader.HeaderTitle = "Price Details";
            SelectedDrugItemHeader.FdaAssignedLabelerCode = currentRow[ "FdaAssignedLabelerCode" ].ToString();
            SelectedDrugItemHeader.ProductCode = currentRow[ "ProductCode" ].ToString();
            SelectedDrugItemHeader.PackageCode = currentRow[ "PackageCode" ].ToString();
            SelectedDrugItemHeader.SetCovered( AlterCoveredString( currentRow[ "Covered" ].ToString() ) );
            SelectedDrugItemHeader.SetSingleDual( GetSingleDualStringForDisplay() ); // AlterSingleDualString( currentRow[ "DualPriceDesignation" ].ToString() ) );
            SelectedDrugItemHeader.SetFETAmount( GetFETAmountStringForDisplay() );
            SelectedDrugItemHeader.Generic = currentRow[ "Generic" ].ToString();
            SelectedDrugItemHeader.TradeName = currentRow[ "TradeName" ].ToString();
        }

        private string GetSingleDualStringForDisplay()
        {
            string singleDualString = "";
            bool bIsItemDualPricer = false;

            if( Session[ "IsSelectedDrugItemDualPricer" ] != null )
            {
                if( bool.TryParse( Session[ "IsSelectedDrugItemDualPricer" ].ToString(), out bIsItemDualPricer ) == true )
                {
                    if( bIsItemDualPricer == true )
                        singleDualString = "Dual";
                    else
                        singleDualString = "Single";
                }
            }
            return ( singleDualString );
        }

        private string GetFETAmountStringForDisplay()
        {
            string FETAmountString = "";
            Decimal IncludedFETAmount = 0;

            if( Session[ "SelectedDrugItemIncludedFETAmount" ] != null )
            {
                if( Decimal.TryParse( Session[ "SelectedDrugItemIncludedFETAmount" ].ToString(), out IncludedFETAmount ) == true )
                {
                    if( IncludedFETAmount == 0 )
                        FETAmountString = "";
                    else
                        FETAmountString = IncludedFETAmount.ToString( "0.00" );
                }
            }
            return ( FETAmountString );
        }

        // capture Id's which are needed as future parameter values on save
        private void CaptureExtraIds()
        {
            DataRowView currentRow = ( DataRowView )DrugItemPriceDetailsFormView.DataItem;
   //         Session[ "LastPriceIdForSelectedPriceDetails" ] = int.Parse( currentRow[ "PriceId" ].ToString() );
        }

        private string AlterCoveredString( string coveredShortString )
        {
            string coveredLongString = "";

            if( coveredShortString.CompareTo( "T" ) == 0 )
            {
                coveredLongString = "Covered";
            }
            else
            {
                coveredLongString = "Non-covered";
            }

            return ( coveredLongString );
        }

        private string AlterSingleDualString( string singleDualShortString )
        {
            string singleDualLongString = "";

            if( singleDualShortString.CompareTo( "T" ) == 0 )
            {
                singleDualLongString = "Dual";
            }
            else
            {
                singleDualLongString = "Single";
            }

            return ( singleDualLongString );
        }

        private void LoadItemPriceDetails()
        {
            if( Session[ "DrugItemPriceDetailsDataSource" ] == null )
            {
                _DrugItemPriceDetailsDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, true );
                _DrugItemPriceDetailsDataSource.ID = "DrugItemPriceDetailsDataSource";
                _DrugItemPriceDetailsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _DrugItemPriceDetailsDataSource.SelectCommand = "GetFSSDrugItemPriceDetails";
                _DrugItemPriceDetailsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                
                _DrugItemPriceDetailsDataSource.UpdateCommand = "UpdateFSSDrugItemPriceDetails";
                _DrugItemPriceDetailsDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;

                _DrugItemPriceDetailsDataSource.SetEventOwnerName( "DrugItemPriceDetails" );

                SetUpDrugItemPriceDetailsParameters();

                _contractNumberParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.ContractNumber;
                _drugItemIdParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.SelectedDrugItemId.ToString();
                _drugItemPriceIdParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.SelectedPriceId.ToString();
                _drugItemPriceHistoryIdParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.SelectedPriceHistoryId.ToString();
                _isFromHistoryParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.IsFromHistory.ToString();
                _isHistoryFromArchiveParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.IsHistoryFromArchive.ToString();

                AddParametersToDataSource( _DrugItemPriceDetailsDataSource );

            }
            else
            {
                _DrugItemPriceDetailsDataSource = ( DocumentDataSource )Session[ "DrugItemPriceDetailsDataSource" ];

                _DrugItemPriceDetailsDataSource.RestoreDelegatesAfterDeserialization( this, "DrugItemPriceDetails" );

                ReloadDrugItemPriceDetailsParameters( _DrugItemPriceDetailsDataSource );
            }

            DrugItemPriceDetailsFormView.DataSource = _DrugItemPriceDetailsDataSource;

        }

        private void BindItemPriceDetails()
        {
            DrugItemPriceDetailsFormView.DataBind();
        }

        private void BindTieredPriceGrid()
        {
            try
            {
                GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );
                tieredPriceGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }
        }

        //private void UpdateTieredPriceGrid()
        //{
        //    UpdatePanel tieredPriceGridViewUpdatePanel = ( UpdatePanel )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridViewUpdatePanel" );
        //    tieredPriceGridViewUpdatePanel.Update();
        //}

        protected void DrugItemPriceDetailsFormView_OnDataBound( Object sender, EventArgs e )
        {
            SetItemPriceDetailsHeaderInfo();

            CaptureExtraIds();

            ItemPriceDetailsUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        private void SetUpDrugItemPriceDetailsParameters()
        {
            // select and update
            _contractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _drugItemIdParameter = new Parameter( "DrugItemId", TypeCode.Int32 );
            _drugItemPriceIdParameter = new Parameter( "DrugItemPriceId", TypeCode.Int32 );
    //        _priceIdParameter = new Parameter( "PriceId", TypeCode.Int32 );
            _drugItemPriceHistoryIdParameter = new Parameter( "DrugItemPriceHistoryId", TypeCode.Int32 );
            _isFromHistoryParameter = new Parameter( "IsFromHistory", TypeCode.Boolean );
            _isHistoryFromArchiveParameter = new Parameter( "IsHistoryFromArchive", TypeCode.Boolean );

            // update parameters
            _trackingCustomerRatioParameter = new Parameter( "TrackingCustomerRatio", TypeCode.String );
            _trackingCustomerPriceParameter = new Parameter( "TrackingCustomerPrice", TypeCode.Decimal );
            _trackingCustomerNameParameter = new Parameter( "TrackingCustomerName", TypeCode.String );
            _excludeFromExportParameter = new Parameter( "ExcludeFromExport", TypeCode.Boolean );
     //       _historicalNValueParameter = new Parameter( "HistoricalNValue", TypeCode.String );

            _tieredPriceStartDateParameter = new Parameter( "TieredPriceStartDate", TypeCode.DateTime );
            _tieredPriceStopDateParameter = new Parameter( "TieredPriceStopDate", TypeCode.DateTime );
            _tieredPriceParameter = new Parameter( "TierPrice", TypeCode.Decimal );
            _tierMinimumParameter = new Parameter( "TierMinimum", TypeCode.String );
            _tierMinimumValueParameter = new Parameter( "TierMinimumValue", TypeCode.Int32 );
        }

        private void AddParametersToDataSource( DocumentDataSource drugItemDataSource )
        {
            drugItemDataSource.SelectParameters.Add( _contractNumberParameter );
            drugItemDataSource.SelectParameters.Add( _drugItemIdParameter );
            drugItemDataSource.SelectParameters.Add( _drugItemPriceIdParameter );
            drugItemDataSource.SelectParameters.Add( _drugItemPriceHistoryIdParameter );
            drugItemDataSource.SelectParameters.Add( _isFromHistoryParameter );
            drugItemDataSource.SelectParameters.Add( _isHistoryFromArchiveParameter );

            drugItemDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
            drugItemDataSource.UpdateParameters.Add( _contractNumberParameter );
            drugItemDataSource.UpdateParameters.Add( _drugItemIdParameter );
            drugItemDataSource.UpdateParameters.Add( _drugItemPriceIdParameter );
   //         drugItemDataSource.UpdateParameters.Add( _priceIdParameter );

            drugItemDataSource.UpdateParameters.Add( _trackingCustomerRatioParameter );
            drugItemDataSource.UpdateParameters.Add( _trackingCustomerPriceParameter );
            drugItemDataSource.UpdateParameters.Add( _trackingCustomerNameParameter );
            drugItemDataSource.UpdateParameters.Add( _excludeFromExportParameter );
   //         drugItemDataSource.UpdateParameters.Add( _historicalNValueParameter );
        }

        private void ReloadDrugItemPriceDetailsParameters( DocumentDataSource drugItemDataSource )
        {
            _contractNumberParameter = drugItemDataSource.SelectParameters[ "ContractNumber" ];
            _drugItemIdParameter = drugItemDataSource.SelectParameters[ "DrugItemId" ];
            _drugItemPriceIdParameter = drugItemDataSource.SelectParameters[ "DrugItemPriceId" ];
            _drugItemPriceHistoryIdParameter = drugItemDataSource.SelectParameters[ "DrugItemPriceHistoryId" ];
            _isFromHistoryParameter = drugItemDataSource.SelectParameters[ "IsFromHistory" ];
            _isHistoryFromArchiveParameter = drugItemDataSource.SelectParameters[ "IsHistoryFromArchive" ];

   //         _priceIdParameter = drugItemDataSource.SelectParameters[ "PriceId" ];

            _trackingCustomerRatioParameter = drugItemDataSource.UpdateParameters[ "TrackingCustomerRatio" ];
            _trackingCustomerPriceParameter = drugItemDataSource.UpdateParameters[ "TrackingCustomerPrice" ];
            _trackingCustomerNameParameter = drugItemDataSource.UpdateParameters[ "TrackingCustomerName" ];
            _excludeFromExportParameter = drugItemDataSource.UpdateParameters[ "ExcludeFromExport" ];
    //        _historicalNValueParameter = drugItemDataSource.UpdateParameters[ "HistoricalNValue" ];
        }



        protected void UpdateItemPriceDetailsButton_OnClick( object sender, EventArgs e )
        {
            string validationMessage = "";

            try
            {
                if( ValidatePriceDetails( ref validationMessage ) == true )
                {
                    GetUpdateParameterValues();
                    _DrugItemPriceDetailsDataSource.Update();
                    DrugItemPriceDetailsFormView.DataBind();

                    SetTieredPriceDataSource();
                    BindTieredPriceGrid();

                    // allow the update postback to occur
                    UpdatePanelEventProxy outerFormViewWasSavedUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemPriceDetailsFormView.FindControl( "OuterFormViewWasSavedUpdatePanelEventProxy" );
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
            string trackingCustomerRatio = "";
            string trackingCustomerPrice = "";

            try
            {
                FormView DrugItemPriceDetailsFormView = ( FormView )DrugItemPriceDetailsForm.FindControl( "DrugItemPriceDetailsFormView" );
                if( DrugItemPriceDetailsFormView != null )
                {
                    if( DrugItemPriceDetailsFormView.CurrentMode == FormViewMode.Edit )
                    {

                        TextBox TrackingCustomerRatioTextBox = ( TextBox )DrugItemPriceDetailsFormView.FindControl( "TrackingCustomerRatioTextBox" );
                        if( TrackingCustomerRatioTextBox != null )
                            trackingCustomerRatio = TrackingCustomerRatioTextBox.Text;

                        TextBox TrackingCustomerPriceTextBox = ( TextBox )DrugItemPriceDetailsFormView.FindControl( "TrackingCustomerPriceTextBox" );
                        if( TrackingCustomerPriceTextBox != null )
                        {
                            string tmpTrackingCustomerPrice = TrackingCustomerPriceTextBox.Text;
                            if( tmpTrackingCustomerPrice.Length > 0 )
                                trackingCustomerPrice = CMGlobals.GetMoneyFromString( TrackingCustomerPriceTextBox.Text, "Tracking Customer Price" ).ToString();
                        }

                        decimal parseDecimal;

                        if( trackingCustomerRatio.Length > 0 )
                        {
                            if( decimal.TryParse( trackingCustomerRatio, out parseDecimal ) == false )
                            {
                                throw new Exception( "The Tracking Customer Ratio must be a decimal value less than 10 characters in length, or blank." );
                            }
                        }

                        if( trackingCustomerPrice.Length > 0 )
                        {
                            if( decimal.TryParse( trackingCustomerPrice, out parseDecimal ) == false )
                            {
                                throw new Exception( "The Tracking Customer Price must be a decimal value less than 10 characters in length, or blank." );
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

            return ( bSuccess );
        }

        private void GetUpdateParameterValues()
        {
            FormView DrugItemPriceDetailsFormView = ( FormView )DrugItemPriceDetailsForm.FindControl( "DrugItemPriceDetailsFormView" );
            if( DrugItemPriceDetailsFormView != null )
            {
                if( DrugItemPriceDetailsFormView.CurrentMode == FormViewMode.Edit )
                {
                    // get extra id's which were saved during last bind event
     //               int priceId = ( int )Session[ "LastPriceIdForSelectedPriceDetails" ];

                    // save the id's parameters for the update
     //               _priceIdParameter.DefaultValue = priceId.ToString();

                    TextBox TrackingCustomerRatioTextBox = ( TextBox )DrugItemPriceDetailsFormView.FindControl( "TrackingCustomerRatioTextBox" );
                    if( TrackingCustomerRatioTextBox != null )
                        _trackingCustomerRatioParameter.DefaultValue = TrackingCustomerRatioTextBox.Text;

                    TextBox TrackingCustomerPriceTextBox = ( TextBox )DrugItemPriceDetailsFormView.FindControl( "TrackingCustomerPriceTextBox" );
                    if( TrackingCustomerPriceTextBox != null )
                        if( TrackingCustomerPriceTextBox.Text.Length > 0 )
                            _trackingCustomerPriceParameter.DefaultValue = CMGlobals.GetMoneyFromString( TrackingCustomerPriceTextBox.Text, "Tracking Customer Price" ).ToString();
                        else
                            _trackingCustomerPriceParameter.DefaultValue = "";

                    TextBox TrackingCustomerNameTextBox = ( TextBox )DrugItemPriceDetailsFormView.FindControl( "TrackingCustomerNameTextBox" );
                    if( TrackingCustomerNameTextBox != null )
                        _trackingCustomerNameParameter.DefaultValue = TrackingCustomerNameTextBox.Text;

                    CheckBox ExcludeFromExportCheckBox = ( CheckBox )DrugItemPriceDetailsFormView.FindControl( "ExcludeFromExportCheckBox" );
                    if( ExcludeFromExportCheckBox != null )
                        _excludeFromExportParameter.DefaultValue = ExcludeFromExportCheckBox.Checked.ToString();

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

                lastModificationTypeDescription = CMGlobals.GetLastModificationTypeDescription( lastModificationType );
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


        protected void ExcludeFromExportCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox excludeFromExportCheckBox = ( CheckBox )sender;

            FormView containingFormView = ( FormView )excludeFromExportCheckBox.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            bool bIsChecked = false;

            if( currentRow[ "ExcludeFromExport" ] != DBNull.Value )
            {
                bIsChecked = bool.Parse( currentRow[ "ExcludeFromExport" ].ToString() );
            }

            excludeFromExportCheckBox.Checked = bIsChecked;
        }

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

        protected void DrugItemPriceDetailsFormView_OnPreRender( object sender, EventArgs e )
        {
            DisableControlsForReadOnly();

            //FormView drugItemPriceDetailsFormView = ( FormView )sender;
            //GridView tieredPriceGridView = ( GridView )drugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );
            //tieredPriceGridView.DataSource = _tieredPriceDataSource;

            //// save to session
            //Page.Session[ "TieredPriceGridView" ] = tieredPriceGridView;
        }

#region TieredPriceGrid

        private void LoadTieredPrices()
        {
            if( Page.Session[ "TieredPriceDataSource" ] == null )
            {
                _tieredPriceDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, true );
                _tieredPriceDataSource.ID = "TieredPriceDataSource";
                _tieredPriceDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _tieredPriceDataSource.SelectCommand = "SelectTieredPricesForDrugItemPrice";

                _tieredPriceDataSource.UpdateCommand = "UpdateTieredPriceForDrugItemPrice";

                _tieredPriceDataSource.InsertCommand = "InsertTieredPriceForDrugItemPrice";
                _tieredPriceDataSource.SetEventOwnerName( "TieredPrice" );
                _tieredPriceDataSource.Inserted += new SqlDataSourceStatusEventHandler( _tieredPriceDataSource_Inserted );

                _tieredPriceDataSource.Updated += new SqlDataSourceStatusEventHandler( _tieredPriceDataSource_Updated );

                _tieredPriceDataSource.DeleteCommand = "DeleteTieredPriceForDrugItemPrice";

                CreateTieredPriceDataSourceParameters();

                _tieredPriceDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _tieredPriceDataSource.SelectParameters.Add( _contractNumberParameter );
                _contractNumberParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.ContractNumber;
                _tieredPriceDataSource.SelectParameters.Add( _withAddParameter );
                _withAddParameter.DefaultValue = "false"; // not adding
                _tieredPriceDataSource.SelectParameters.Add( _drugItemPriceIdParameter );
                _tieredPriceDataSource.SelectParameters.Add( _drugItemPriceHistoryIdParameter );
                _tieredPriceDataSource.SelectParameters.Add( _isTieredPriceFromHistoryParameter );
                _isTieredPriceFromHistoryParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.IsFromHistory.ToString();

                _tieredPriceDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _tieredPriceDataSource.UpdateParameters.Add( _contractNumberParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _drugItemPriceIdParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _drugItemTieredPriceIdParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tieredPriceStartDateParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tieredPriceStopDateParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tieredPriceParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tierMinimumParameter );
                _tieredPriceDataSource.UpdateParameters.Add( _tierMinimumValueParameter );

                _tieredPriceDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
                _tieredPriceDataSource.InsertParameters.Add( _contractNumberParameter );
                _tieredPriceDataSource.InsertParameters.Add( _modificationStatusIdParameter );
                _tieredPriceDataSource.InsertParameters.Add( _drugItemPriceIdParameter );
                _tieredPriceDataSource.InsertParameters.Add( _drugItemTieredPriceIdForInsertParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tieredPriceStartDateParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tieredPriceStopDateParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tieredPriceParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tierMinimumParameter );
                _tieredPriceDataSource.InsertParameters.Add( _tierMinimumValueParameter );

                _tieredPriceDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
                _tieredPriceDataSource.DeleteParameters.Add( _contractNumberParameter );
                _tieredPriceDataSource.DeleteParameters.Add( _modificationStatusIdParameter );
                _tieredPriceDataSource.DeleteParameters.Add( _drugItemTieredPriceIdParameter );

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
            GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );
            tieredPriceGridView.DataSource = _tieredPriceDataSource;
        }


        private void CreateTieredPriceDataSourceParameters()
        {
            _tieredPriceStartDateParameter = new Parameter( "TieredPriceStartDate", TypeCode.DateTime );
            _tieredPriceStopDateParameter = new Parameter( "TieredPriceStopDate", TypeCode.DateTime );
            _tieredPriceParameter = new Parameter( "Price", TypeCode.Decimal );
            _tierMinimumParameter = new Parameter( "Minimum", TypeCode.String );
            _tierMinimumValueParameter = new Parameter( "MinimumValue", TypeCode.Int32 );
            _withAddParameter = new Parameter( "WithAdd", TypeCode.Boolean );
            _isTieredPriceFromHistoryParameter = new Parameter( "IsFromHistory", TypeCode.Boolean );
            _drugItemTieredPriceIdForInsertParameter = new Parameter( "DrugItemTieredPriceId", TypeCode.Int32 );
            _drugItemTieredPriceIdForInsertParameter.Direction = ParameterDirection.Output;
            _drugItemTieredPriceIdParameter = new Parameter( "DrugItemTieredPriceId", TypeCode.Int32 );

        }

        private void RestoreTieredPriceDataSourceParameters( DocumentDataSource tieredPriceDataSource )
        {
            _tieredPriceStartDateParameter = tieredPriceDataSource.UpdateParameters[ "TieredPriceStartDate" ];
            _tieredPriceStopDateParameter = tieredPriceDataSource.UpdateParameters[ "TieredPriceStopDate" ];
            _tieredPriceParameter = tieredPriceDataSource.UpdateParameters[ "Price" ];
            _tierMinimumParameter = tieredPriceDataSource.UpdateParameters[ "Minimum" ];
            _tierMinimumValueParameter = tieredPriceDataSource.UpdateParameters[ "MinimumValue" ];
            _drugItemTieredPriceIdParameter = tieredPriceDataSource.UpdateParameters[ "DrugItemTieredPriceId" ];

            _withAddParameter = tieredPriceDataSource.SelectParameters[ "WithAdd" ];
            _isTieredPriceFromHistoryParameter = tieredPriceDataSource.SelectParameters[ "IsFromHistory" ];

            _drugItemTieredPriceIdForInsertParameter = tieredPriceDataSource.InsertParameters[ "DrugItemTieredPriceId" ];
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

            GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

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
                UpdatePanelEventProxy changeTieredPriceHighlightUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemPriceDetailsFormView.FindControl( "ChangeTieredPriceHighlightUpdatePanelEventProxy" );
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
                    if( Session[ "CurrentDocument" ] != null )
                    {
                        CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                        if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmPrices ) == false ||
                            _DrugItemPriceDetailsWindowParms.IsFromHistory == true ||
                            currentDocument.DocumentType != CurrentDocument.DocumentTypes.FSS )
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
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }



        protected void TieredPriceGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedDrugItemTieredPriceId = -1;
            int itemIndex = -1;

            GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            if( e.CommandName.CompareTo( "RemovePrice" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemTieredPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                int newRowIndex = DeleteTieredPrice( tieredPriceGridView, itemIndex, selectedDrugItemTieredPriceId );

                HighlightTieredPriceRow( newRowIndex );
            }

            else if( e.CommandName.CompareTo( "EditItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemTieredPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                HighlightTieredPriceRow( itemIndex );

                InitiateEditModeForItem( itemIndex );

            }
            // save update or save insert
            else if( e.CommandName.CompareTo( "SaveItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemTieredPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

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
                selectedDrugItemTieredPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

            }
            // can trap editing commands here if needed, prior to individual editing events
        }

        protected void AddNewTieredPriceButton_OnClick( object sender, EventArgs e )
        {
            GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            tieredPriceGridView.Insert();

            _withAddParameter.DefaultValue = "true";

            tieredPriceGridView.DataBind();

            InitiateEditModeForItem( 0 );

            // allow the update postback to occur
            UpdatePanelEventProxy insertTieredPriceButtonClickUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemPriceDetailsFormView.FindControl( "InsertTieredPriceButtonClickUpdatePanelEventProxy" );
            insertTieredPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            HighlightTieredPriceRow( 0 );

        }


        protected void TieredPriceGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForItem( e.NewEditIndex );
        }

        private void InitiateEditModeForItem( int editIndex )
        {
            GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            tieredPriceGridView.EditIndex = editIndex;

            // select the edited item also
            SetTieredPriceGridViewSelectedItem( editIndex );

            tieredPriceGridView.DataBind();

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
            GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );
            
            // save for postback
            Session[ "TieredPriceGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            tieredPriceGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            ScrollToSelectedItem();

            // allow the update postback to occur
            UpdatePanelEventProxy insertTieredPriceButtonClickUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemPriceDetailsFormView.FindControl( "InsertTieredPriceButtonClickUpdatePanelEventProxy" );
            insertTieredPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedItem()
        {
            GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            int rowIndex = tieredPriceGridView.SelectedIndex;
            //  TableItemStyle rowStyle = DrugItemsGridView.RowStyle;
            //  int rowHeight = ( int )rowStyle.Height.Value;  // this value is always zero
            int rowPosition = TIEREDPRICEGRIDVIEWROWHEIGHTESTIMATE * rowIndex;

            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
            //     ScriptManager.RegisterClientScriptBlock( this.Page, this.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true );  // runs prior to control rendering
        }

        private void RestoreTieredPriceGridViewSelectedItem()
        {
            GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );

            tieredPriceGridView.SelectedIndex = ( int )Session[ "TieredPriceGridViewSelectedIndex" ];
        }

        protected void TieredPriceGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            GridView tieredPriceGridView = ( GridView )DrugItemPriceDetailsFormView.FindControl( "TieredPriceGridView" );
            
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

            string priceStartDate = "";
            string priceStopDate = "";
            string price = "";
            string minimum = "";
            string minimumValueString = "";
            int minimumValue = -1;


            try
            {
                priceStartDate = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "priceStartDateTextBox" );
                priceStopDate = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "priceEndDateTextBox" );
                price = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "priceTextBox" ), "Tiered Price" ).ToString();
                minimum = gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "minimumTextBox" );
                minimumValueString = gv.GetStringValueFromSelectedControl( rowIndex, 5, 0, false, "minimumValueTextBox" );

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
                _drugItemPriceIdParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.SelectedPriceId.ToString();
                _drugItemTieredPriceIdParameter.DefaultValue = gv.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();
                _tieredPriceStartDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "priceStartDateTextBox" );
                _tieredPriceStopDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "priceEndDateTextBox" );
                _tieredPriceParameter.DefaultValue = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "priceTextBox" ), "Tiered Price" ).ToString();
                _tierMinimumParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "minimumTextBox" );
                _tierMinimumValueParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 5, 0, false, "minimumValueTextBox" );

                _tieredPriceDataSource.Update();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.EditIndex = -1; // done with edit
            //         SetDrugItemsGridViewSelectedItem( rowIndex );
            gv.DataBind();

            if( Session[ "LastUpdatedDrugItemTieredPriceId" ] != null )
            {
                int updatedTieredPriceId = ( int )Session[ "LastUpdatedDrugItemTieredPriceId" ];
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

            _drugItemPriceIdParameter.DefaultValue = _DrugItemPriceDetailsWindowParms.SelectedPriceId.ToString();
            _tieredPriceStartDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "priceStartDateTextBox" );
            _tieredPriceStopDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "priceEndDateTextBox" );
            _tieredPriceParameter.DefaultValue = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "priceTextBox" ), "Tiered Price" ).ToString();
            _tierMinimumParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "minimumTextBox" );
            _tierMinimumValueParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 5, 0, false, "minimumValueTextBox" );

            try
            {
                _tieredPriceDataSource.Insert();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.InsertRowActive = false; // done with insert
            gv.EditIndex = -1; // done with edit of new row
            _withAddParameter.DefaultValue = "false"; // no extra row

            // bind with new row
            gv.DataBind();

            if( Session[ "LastInsertedDrugItemTieredPriceId" ] != null )
            {
                int newTieredPriceId = ( int )Session[ "LastInsertedDrugItemTieredPriceId" ];
                insertedRowIndex = gv.GetRowIndexFromId( newTieredPriceId, 0 );

                SetTieredPriceGridViewSelectedItem( insertedRowIndex );

                // bind to select
                gv.DataBind();
            }

            return ( insertedRowIndex );
        }

        protected void _tieredPriceDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemTieredPriceIdString = e.Command.Parameters[ "@DrugItemTieredPriceId" ].Value.ToString();

            if( drugItemTieredPriceIdString.Length > 0 )
            {
                int drugItemTieredPriceId = int.Parse( drugItemTieredPriceIdString );
                Session[ "LastInsertedDrugItemTieredPriceId" ] = drugItemTieredPriceId;
            }
        }

        // probably wont happen - id changing during update
        protected void _tieredPriceDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemTieredPriceIdString = e.Command.Parameters[ "@DrugItemTieredPriceId" ].Value.ToString();

            if( drugItemTieredPriceIdString.Length > 0 )
            {
                int drugItemTieredPriceId = int.Parse( drugItemTieredPriceIdString );
                Session[ "LastUpdatedDrugItemTieredPriceId" ] = drugItemTieredPriceId;
            }
        }

        private int DeleteTieredPrice( GridView gv, int rowIndex, int selectedDrugItemTieredPriceId )
        {
            // id of row to delete
            _drugItemTieredPriceIdParameter.DefaultValue = selectedDrugItemTieredPriceId.ToString();

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
