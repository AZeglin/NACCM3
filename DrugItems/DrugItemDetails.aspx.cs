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
using AjaxControlToolkit;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using TextBox = System.Web.UI.WebControls.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.DrugItems
{
    public partial class DrugItemDetails : System.Web.UI.Page
    {
        private const int SUBITEMGRIDVIEWROWHEIGHTESTIMATE = 47;
        private const int DISTRIBUTORGRIDVIEWROWHEIGHTESTIMATE = 47;

        private DrugItemDetailsWindowParms _drugItemDetailsWindowParms = null;

        private DocumentDataSource _drugItemDetailsDataSource = null;

        private DocumentDataSource _UnitOfSaleDataSource = null;
        private DocumentDataSource _UnitPackageDataSource = null;
        private DocumentDataSource _UnitOfMeasureDataSource = null;
        private DocumentDataSource _specialtyDistributorDataSource = null;
        private DocumentDataSource _distributorNameDataSource = null;

        private DocumentDataSource _subItemDataSource = null;

        private Parameter _modificationStatusIdParameter = null;

        // select parameters
        private Parameter _contractNumberParameter = null;
        private Parameter _drugItemIdParameter = null;
        private Parameter _drugItemPackageIdParameter = null;
        private Parameter _drugItemPackageIdForInsertParameter = null;

        private Parameter _DateEnteredMarketParameter = null;
  //      private Parameter _DualPriceDesignationParameter = null;
        private Parameter _PrimeVendorParameter = null;
        private Parameter _PrimeVendorChangedDateParameter = null;
        private Parameter _PassThroughFlagParameter = null;
        private Parameter _VAClassParameter = null;
        private Parameter _ExcludeFromExportParameter = null;
        private Parameter _NonTAAParameter = null;
        private Parameter _IncludedFETAmountParameter = null;

        private Parameter _UnitOfSaleParameter = null;
        private Parameter _QuantityInUnitOfSaleParameter = null;
        private Parameter _UnitPackageParameter = null;
        private Parameter _QuantityInUnitPackageParameter = null;
        private Parameter _UnitOfMeasureParameter = null;
        private Parameter _PriceMultiplierParameter = null;
        private Parameter _PriceDividerParameter = null;

        // distributor parameters
        private Parameter _withAddDistributorParameter = null;
        private Parameter _selectParentItemDistributors = null;
 
        private Parameter _distributorNameParameter = null;
        private Parameter _distributorContactPersonParameter = null;
        private Parameter _distributorPhoneParameter = null;
        private Parameter _distributorNotesParameter = null;
        private Parameter _drugItemIdForDistributorInsertParameter = null;
        private Parameter _drugItemDistributorIdParameter = null;
        private Parameter _drugItemDistributorIdForInsertParameter = null;

        // sub item parameters
        private Parameter _withAddSubItemParameter = null;
        private Parameter _dispensingUnitParameter = null;
        private Parameter _tradeNameParameter = null;
        private Parameter _genericParameter = null;
        private Parameter _packageDescriptionParameter = null;
        private Parameter _subItemIdentifierParameter = null;
        private Parameter _drugItemSubItemIdForInsertParameter = null;
        private Parameter _drugItemSubItemIdParameter = null;

        protected void Page_Load( object sender, EventArgs e )
        {           
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }
            
            if( Session[ "DrugItemDetailsWindowParms" ] != null )
            {
                _drugItemDetailsWindowParms = ( DrugItemDetailsWindowParms )Session[ "DrugItemDetailsWindowParms" ];
            }
            else
            {
                throw new Exception( "Error: ItemId not available for details presentation." );
            }

            InitSharedParms();
            LoadSalePackagingMeasureUnits();
            LoadItemDetails();
  
            if( Page.IsPostBack == false )
            {
                BindItemDetails();
                BindItemPackagingLists();
            }

            AddClientCloseEvent();

            //if( Session[ "CurrentDocument" ] != null )
            //{
            //    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
            //    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PBMItems ) == true )
            //    {
            //        DrugItemDetailsFormView.DefaultMode = FormViewMode.Edit;
            //    }
            //    else
            //    {
            //        DrugItemDetailsFormView.DefaultMode = FormViewMode.ReadOnly;
            //    }
            //}

            LoadSpecialtyDistributors();
            SetDistributorDataSource();

            if( Page.IsPostBack == false )
            {
                BindDistributorGrid();
            }

            LoadSubItems();
            SetSubItemDataSource();

            if( Page.IsPostBack == false )
            {
                BindSubItemGrid();
            }
        }

        private void DisableItemDetailsControlsForReadOnly()
        {
            if( Session[ "CurrentDocument" ] != null )
            {
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                // items may not be editable at all if, for example, contract was expired or item is discontinued
                if ( _drugItemDetailsWindowParms.IsCOEditable == false && _drugItemDetailsWindowParms.IsPBMEditable == false )
                {
                    DisableAllControlsForReadOnly();
                }
                else
                {
                    if ( _drugItemDetailsWindowParms.IsCOEditable == false )
                    {
                        DisableCOControlsForReadOnly();
                    }

                    if ( _drugItemDetailsWindowParms.IsPBMEditable == false )
                    {
                        DisablePBMControlsForReadOnly();
                    }

                    // bpas inherit the specialty distributors and cannot edit them
                    if ( _drugItemDetailsWindowParms.IsBPA == true )
                    {
                        DisableSpecialtyDistributorGrid();
                    }
                }
            }
        }

        private void DisableAllControlsForReadOnly()
        {
            Button updateItemDetailsButton = null;
            updateItemDetailsButton = ( Button )DrugItemDetailsForm.FindControl( "UpdateItemDetailsButton" );
            if ( updateItemDetailsButton != null )
            {
                updateItemDetailsButton.Enabled = false;
            }

            DisablePBMControlsForReadOnly();
            DisableCOControlsForReadOnly();

            DisableSpecialtyDistributorGrid();

        }

        // disable PBM specific controls (for CO user)
        private void DisablePBMControlsForReadOnly()
        {
            DisableTextBox( "VAClassTextBox" );

            DropDownList unitOfSaleDropDownList = null;
            unitOfSaleDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitOfSaleDropDownList" );
            if( unitOfSaleDropDownList != null )
                unitOfSaleDropDownList.Enabled = false;

            DisableTextBox( "QuantityInUnitOfSaleTextBox" );

            DropDownList unitPackageDropDownList = null;
            unitPackageDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitPackageDropDownList" );
            if( unitPackageDropDownList != null )
                unitPackageDropDownList.Enabled = false;

            DisableTextBox( "QuantityInUnitPackageTextBox" );

            DropDownList unitOfMeasureDropDownList = null;
            unitOfMeasureDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitOfMeasureDropDownList" );
            if( unitOfMeasureDropDownList != null )
                unitOfMeasureDropDownList.Enabled = false;

            DisableTextBox( "PriceMultiplierTextBox" );
            DisableTextBox( "PriceDividerTextBox" );

            DisableSubItemGrid();
        }

        // disable CO specific controls ( should be mutex with other disable function )
        private void DisableCOControlsForReadOnly()
        {
            DisableTextBox( "DateEnteredMarketTextBox" );
            DisableTextBox( "PrimeVendorChangedDateTextBox" );
            DisableTextBox( "PassThroughFlagTextBox" );

            CheckBox excludeFromExportCheckBox = null;
            excludeFromExportCheckBox = ( CheckBox )DrugItemDetailsFormView.FindControl( "ExcludeFromExportCheckBox" );
            if ( excludeFromExportCheckBox != null )
                excludeFromExportCheckBox.Enabled = false;

            CheckBox primeVendorFlagCheckBox = null;
            primeVendorFlagCheckBox = ( CheckBox )DrugItemDetailsFormView.FindControl( "PrimeVendorFlagCheckBox" );
            if ( primeVendorFlagCheckBox != null )
                primeVendorFlagCheckBox.Enabled = false;

            CheckBox nonTAACheckBox = null;
            nonTAACheckBox = ( CheckBox )DrugItemDetailsFormView.FindControl( "NonTAACheckBox" );
            if( nonTAACheckBox != null )
                nonTAACheckBox.Enabled = false;

            DisableTextBox( "IncludedFETAmountTextBox" );
        }

        private void DisableSpecialtyDistributorGrid()
        {
            Button addNewDistributorButton = null;
            addNewDistributorButton = ( Button )DrugItemDetailsFormView.FindControl( "AddNewDistributorButton" );
            if( addNewDistributorButton != null )
            {
                addNewDistributorButton.Enabled = false;
            } 

            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );
            if( distributorGridView != null )
            {
                distributorGridView.Enabled = false;
            }
        }

        private void DisableSubItemGrid()
        {
            Button addNewSubItemButton = null;
            addNewSubItemButton = ( Button )DrugItemDetailsFormView.FindControl( "AddNewSubItemButton" );
            if( addNewSubItemButton != null )
            {
                addNewSubItemButton.Enabled = false;
            } 
            
            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );
            if( subItemGridView != null )
            {
                subItemGridView.Enabled = false;
            }
        }

        //private void DisableControlsForReadOnly()
        //{
        //    if( Session[ "CurrentDocument" ] != null )
        //    {
        //        CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

        //        if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItems ) == false )
        //        {
        //            Button updateItemDetailsButton = null;
        //            updateItemDetailsButton = ( Button )DrugItemDetailsForm.FindControl( "UpdateItemDetailsButton" );
        //            if( updateItemDetailsButton != null )
        //            {
        //                updateItemDetailsButton.Enabled = false;
        //            }

        //            Button addNewSubItemButton = null;
        //            addNewSubItemButton = ( Button )DrugItemDetailsFormView.FindControl( "AddNewSubItemButton" );
        //            if( addNewSubItemButton != null )
        //            {
        //                addNewSubItemButton.Enabled = false;
        //            }
        //        }
        //    }
        //}

        //private void DisableFormViewForReadOnly()
        //{
        //    if( Session[ "CurrentDocument" ] != null )
        //    {
        //        CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

        //        if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItems ) == false )
        //        {
        //            DisableTextBox( "DateEnteredMarketTextBox" );
        //            DisableTextBox( "PrimeVendorChangedDateTextBox" );
        //            DisableTextBox( "PassThroughFlagTextBox" );
        //            DisableTextBox( "VAClassTextBox" );


        //            CheckBox excludeFromExportCheckBox = null;
        //            excludeFromExportCheckBox = ( CheckBox )DrugItemDetailsFormView.FindControl( "ExcludeFromExportCheckBox" );
        //            if( excludeFromExportCheckBox != null )
        //                excludeFromExportCheckBox.Enabled = false;

        //            CheckBox primeVendorFlagCheckBox = null;
        //            primeVendorFlagCheckBox = ( CheckBox )DrugItemDetailsFormView.FindControl( "PrimeVendorFlagCheckBox" );
        //            if( primeVendorFlagCheckBox != null )
        //                primeVendorFlagCheckBox.Enabled = false;

        //            DropDownList unitOfSaleDropDownList = null;
        //            unitOfSaleDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitOfSaleDropDownList" );
        //            if( unitOfSaleDropDownList != null )
        //                unitOfSaleDropDownList.Enabled = false;

        //            DisableTextBox( "QuantityInUnitOfSaleTextBox" );

        //            DropDownList unitPackageDropDownList = null;
        //            unitPackageDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitPackageDropDownList" );
        //            if( unitPackageDropDownList != null )
        //                unitPackageDropDownList.Enabled = false;

        //            DisableTextBox( "QuantityInUnitPackageTextBox" );

        //            DropDownList unitOfMeasureDropDownList = null;
        //            unitOfMeasureDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitOfMeasureDropDownList" );
        //            if( unitOfMeasureDropDownList != null )
        //                unitOfMeasureDropDownList.Enabled = false;

        //            DisableTextBox( "PriceMultiplierTextBox" );
        //            DisableTextBox( "PriceDividerTextBox" );
        //        }
        //    }
        //}

        private void DisableTextBox( string textBoxName )
        {
            System.Web.UI.WebControls.TextBox tb = null;
            tb = ( System.Web.UI.WebControls.TextBox )DrugItemDetailsFormView.FindControl( textBoxName );

            if( tb != null )
            {
                tb.Enabled = false;
            }
        }

        private void ClearSessionVariables()
        {
            Session[ "DrugItemDetailsDataSource" ] = null;
            Session[ "LastPackageIdForSelectedItemDetails" ] = null;
            Session[ "UnitOfSaleDataSource" ] = null;
            Session[ "UnitPackageDataSource" ] = null;
            Session[ "UnitOfMeasureDataSource" ] = null;
            Session[ "SpecialtyDistributorDataSource" ] = null;
            Session[ "SubItemDataSource" ] = null;
            Session[ "SubItemGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedDrugItemSubItemId" ] = null;
            Session[ "LastInsertedDrugItemSubItemId" ] = null;
        }

        protected void DrugItemDetailsFormView_OnPreRender( object sender, EventArgs e )
        {
            // dual price designation will be determined by the presence of a current BIG4 price
            DropDownList dualPriceDesignationDropDownList = null;
            dualPriceDesignationDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "DualPriceDesignationDropDownList" );
            if( dualPriceDesignationDropDownList != null )
                dualPriceDesignationDropDownList.Enabled = false;

            DisableItemDetailsControlsForReadOnly();
            //DisableControlsForReadOnly();
            //DisableFormViewForReadOnly();
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
            string closeFunctionText = "CloseWindow( \"true\", \"true\" );";
            CancelItemDetailsButton.Attributes.Add( "onclick", closeFunctionText );
        }

        protected void DrugItemDetailsScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "DrugItemDetailsErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "DrugItemDetailsErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            DrugItemDetailsScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }
        
        private void SetItemDetailsHeaderInfo()
        {
            DataRowView currentRow = ( DataRowView )DrugItemDetailsFormView.DataItem;

            SelectedDrugItemHeader.HeaderTitle = "Item Details";
            SelectedDrugItemHeader.FdaAssignedLabelerCode = currentRow[ "FdaAssignedLabelerCode" ].ToString();
            SelectedDrugItemHeader.ProductCode = currentRow[ "ProductCode" ].ToString();
            SelectedDrugItemHeader.PackageCode = currentRow[ "PackageCode" ].ToString();
            SelectedDrugItemHeader.SetCovered( AlterCoveredString( currentRow[ "Covered" ].ToString()) );
            SelectedDrugItemHeader.SetSingleDual( GetSingleDualStringForDisplay() );
            SelectedDrugItemHeader.SetFETAmount( "" ); // not displaying fet on details header
            SelectedDrugItemHeader.Generic = currentRow[ "Generic" ].ToString();
            SelectedDrugItemHeader.TradeName = currentRow[ "TradeName" ].ToString();
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

        private void LoadItemDetails()
        {
            if( Session[ "DrugItemDetailsDataSource" ] == null )
            {
                _drugItemDetailsDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, true );
                _drugItemDetailsDataSource.ID = "DrugItemDetailsDataSource";
                _drugItemDetailsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _drugItemDetailsDataSource.SelectCommand = "GetFSSDrugItemDetails";
                _drugItemDetailsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                _drugItemDetailsDataSource.UpdateCommand = "UpdateFSSDrugItemDetails";
                _drugItemDetailsDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _drugItemDetailsDataSource.SetEventOwnerName( "DrugItemDetails" );

                SetUpDrugItemDetailsParameters();

                _contractNumberParameter.DefaultValue = _drugItemDetailsWindowParms.ContractNumber;
                _drugItemIdParameter.DefaultValue = _drugItemDetailsWindowParms.SelectedDrugItemId.ToString();

                AddParametersToDataSource( _drugItemDetailsDataSource );

            }
            else
            {
                _drugItemDetailsDataSource = ( DocumentDataSource )Session[ "DrugItemDetailsDataSource" ];
                _drugItemDetailsDataSource.RestoreDelegatesAfterDeserialization( this, "DrugItemDetails" );
                ReloadDrugItemDetailsParameters( _drugItemDetailsDataSource );
            }

            DrugItemDetailsFormView.DataSource = _drugItemDetailsDataSource;
        }

        private void BindItemDetails()
        {
            DrugItemDetailsFormView.DataBind();
        }

        private void BindItemPackagingLists()
        {
            DropDownList UnitOfSaleDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitOfSaleDropDownList" );
            UnitOfSaleDropDownList.DataSource = _UnitOfSaleDataSource;
            UnitOfSaleDropDownList.DataTextField = "SelectableUnit";
            UnitOfSaleDropDownList.DataValueField = "SelectableUnit";
            UnitOfSaleDropDownList.DataBind();

            DropDownList UnitPackageDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitPackageDropDownList" );
            UnitPackageDropDownList.DataSource = _UnitPackageDataSource;
            UnitPackageDropDownList.DataTextField = "SelectableUnit";
            UnitPackageDropDownList.DataValueField = "SelectableUnit";
            UnitPackageDropDownList.DataBind();

            DropDownList UnitOfMeasureDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitOfMeasureDropDownList" );
            UnitOfMeasureDropDownList.DataSource = _UnitOfMeasureDataSource;
            UnitOfMeasureDropDownList.DataTextField = "SelectableUnit";
            UnitOfMeasureDropDownList.DataValueField = "SelectableUnit";
            UnitOfMeasureDropDownList.DataBind();
        }

        protected void DrugItemDetailsFormView_OnDataBound( Object sender, EventArgs e )
        {
            SetItemDetailsHeaderInfo();

            CaptureExtraIds();

            SetScreenDefaults();

            ItemDetailsUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        // capture Id's which are needed as future parameter values on save
        private void CaptureExtraIds()
        {
            DataRowView currentRow = ( DataRowView )DrugItemDetailsFormView.DataItem;
            Session[ "LastPackageIdForSelectedItemDetails" ] = int.Parse( currentRow[ "DrugItemPackageId" ].ToString() );
        }

        private void SetScreenDefaults()
        {
            DataRowView currentRow = ( DataRowView )DrugItemDetailsFormView.DataItem;
            int priceMultiplier;
            int priceDivider;

            if( int.TryParse( currentRow[ "PriceMultiplier" ].ToString(), out priceMultiplier ) != true )
            {
                TextBox PriceMultiplierTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "PriceMultiplierTextBox" );
                if( PriceMultiplierTextBox != null )
                    PriceMultiplierTextBox.Text = "1";
            }

            if( int.TryParse( currentRow[ "PriceDivider" ].ToString(), out priceDivider ) != true )
            {
                TextBox PriceDividerTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "PriceDividerTextBox" );
                if( PriceDividerTextBox != null )
                    PriceDividerTextBox.Text = "1";
            }

            TextBox PassThroughFlagTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "PassThroughFlagTextBox" );
            if( currentRow[ "PassThrough" ] != DBNull.Value )
            {
                if( currentRow[ "PassThrough" ].ToString().CompareTo( "T" ) != 0 && currentRow[ "PassThrough" ].ToString().CompareTo( "F" ) != 0 )
                {
                    if( PassThroughFlagTextBox != null )
                        PassThroughFlagTextBox.Text = "F";
                }
            }
            else
            {
                if( PassThroughFlagTextBox != null )
                    PassThroughFlagTextBox.Text = "F";
            }
        }

        private void SetUpDrugItemDetailsParameters()
        {
            // select and update
            _contractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _drugItemIdParameter = new Parameter( "DrugItemId", TypeCode.Int32 );
            _drugItemPackageIdParameter = new Parameter( "DrugItemPackageId", TypeCode.Int32 );

            // update                      
            _DateEnteredMarketParameter = new Parameter( "DateEnteredMarket", TypeCode.DateTime );
     //       _DualPriceDesignationParameter = new Parameter( "DualPriceDesignation", TypeCode.String );
            _PrimeVendorParameter = new Parameter( "PrimeVendor", TypeCode.Char );
            _PrimeVendorChangedDateParameter = new Parameter( "PrimeVendorChangedDate", TypeCode.DateTime );
            _PassThroughFlagParameter = new Parameter( "PassThrough", TypeCode.Char );
            _VAClassParameter = new Parameter( "VAClass", TypeCode.String );
            _ExcludeFromExportParameter = new Parameter( "ExcludeFromExport", TypeCode.Boolean );
            _NonTAAParameter = new Parameter( "NonTAA", TypeCode.Boolean );
            _IncludedFETAmountParameter = new Parameter( "IncludedFETAmount", TypeCode.Double );

            _UnitOfSaleParameter = new Parameter( "UnitOfSale", TypeCode.String );
            _QuantityInUnitOfSaleParameter = new Parameter( "QuantityInUnitOfSale", TypeCode.Decimal );
            _UnitPackageParameter = new Parameter( "UnitPackage", TypeCode.String );
            _QuantityInUnitPackageParameter = new Parameter( "QuantityInUnitPackage", TypeCode.Decimal );
            _UnitOfMeasureParameter = new Parameter( "UnitOfMeasure", TypeCode.String );
            _PriceMultiplierParameter = new Parameter( "PriceMultiplier", TypeCode.Int32 );
            _PriceDividerParameter = new Parameter( "PriceDivider", TypeCode.Int32 );

            // insert
            _drugItemPackageIdForInsertParameter = new Parameter( "NewDrugItemPackageId", TypeCode.Int32 );
            _drugItemPackageIdForInsertParameter.Direction = ParameterDirection.Output;

        }

        private void AddParametersToDataSource( DocumentDataSource drugItemDataSource )
        {
            drugItemDataSource.SelectParameters.Add( _contractNumberParameter );
            drugItemDataSource.SelectParameters.Add( _drugItemIdParameter );

            drugItemDataSource.UpdateParameters.Add( _contractNumberParameter );
            drugItemDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
            drugItemDataSource.UpdateParameters.Add( _drugItemIdParameter );
            drugItemDataSource.UpdateParameters.Add( _DateEnteredMarketParameter );
      //      drugItemDataSource.UpdateParameters.Add( _DualPriceDesignationParameter );
            drugItemDataSource.UpdateParameters.Add( _PrimeVendorParameter );
            drugItemDataSource.UpdateParameters.Add( _PrimeVendorChangedDateParameter );
            drugItemDataSource.UpdateParameters.Add( _PassThroughFlagParameter );
            drugItemDataSource.UpdateParameters.Add( _VAClassParameter );
            drugItemDataSource.UpdateParameters.Add( _ExcludeFromExportParameter );
            drugItemDataSource.UpdateParameters.Add( _NonTAAParameter );
            drugItemDataSource.UpdateParameters.Add( _IncludedFETAmountParameter );
         
            drugItemDataSource.UpdateParameters.Add( _drugItemPackageIdParameter );
            drugItemDataSource.UpdateParameters.Add( _drugItemPackageIdForInsertParameter );

            drugItemDataSource.UpdateParameters.Add( _UnitOfSaleParameter );
            drugItemDataSource.UpdateParameters.Add( _QuantityInUnitOfSaleParameter );
            drugItemDataSource.UpdateParameters.Add( _UnitPackageParameter );
            drugItemDataSource.UpdateParameters.Add( _QuantityInUnitPackageParameter );
            drugItemDataSource.UpdateParameters.Add( _UnitOfMeasureParameter );
            drugItemDataSource.UpdateParameters.Add( _PriceMultiplierParameter );
            drugItemDataSource.UpdateParameters.Add( _PriceDividerParameter );

        }

        private void ReloadDrugItemDetailsParameters( DocumentDataSource drugItemDataSource )
        {
            _contractNumberParameter = drugItemDataSource.SelectParameters[ "ContractNumber" ];
            _drugItemIdParameter = drugItemDataSource.SelectParameters[ "DrugItemId" ];

            _DateEnteredMarketParameter = drugItemDataSource.UpdateParameters[ "DateEnteredMarket" ];
    //        _DualPriceDesignationParameter = drugItemDataSource.UpdateParameters[ "DualPriceDesignation" ];
            _PrimeVendorParameter = drugItemDataSource.UpdateParameters[ "PrimeVendorParameter" ];
            _PrimeVendorChangedDateParameter = drugItemDataSource.UpdateParameters[ "PrimeVendorChangedDate" ];
            _PassThroughFlagParameter = drugItemDataSource.UpdateParameters[ "PassThrough" ];
            _VAClassParameter = drugItemDataSource.UpdateParameters[ "VAClass" ];
            _ExcludeFromExportParameter = drugItemDataSource.UpdateParameters[ "ExcludeFromExport" ];
            _NonTAAParameter = drugItemDataSource.UpdateParameters[ "NonTAA" ];
            _IncludedFETAmountParameter = drugItemDataSource.UpdateParameters[ "IncludedFETAmount" ];

            _drugItemPackageIdParameter = drugItemDataSource.UpdateParameters[ "DrugItemPackageId" ];
            _drugItemPackageIdForInsertParameter = drugItemDataSource.UpdateParameters[ "NewDrugItemPackageId" ];

            _UnitOfSaleParameter = drugItemDataSource.UpdateParameters[ "UnitOfSale" ];
            _QuantityInUnitOfSaleParameter = drugItemDataSource.UpdateParameters[ "QuantityInUnitOfSale" ];
            _UnitPackageParameter = drugItemDataSource.UpdateParameters[ "UnitPackage" ];
            _QuantityInUnitPackageParameter = drugItemDataSource.UpdateParameters[ "QuantityInUnitPackage" ];
            _UnitOfMeasureParameter = drugItemDataSource.UpdateParameters[ "UnitOfMeasure" ];
            _PriceMultiplierParameter = drugItemDataSource.UpdateParameters[ "PriceMultiplier" ];
            _PriceDividerParameter = drugItemDataSource.UpdateParameters[ "PriceDivider" ];

        }

 

        private void LoadSalePackagingMeasureUnits()
        {
            if( Session[ "UnitOfSaleDataSource" ] == null )
            {
                _UnitOfSaleDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, false );
                _UnitOfSaleDataSource.ID = "UnitOfSaleDataSource";
                _UnitOfSaleDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _UnitOfSaleDataSource.SelectCommand = "SelectSalePackagingMeasureUnits";
                _UnitOfSaleDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                Parameter unitOfSaleParameter = new Parameter( "UnitType", TypeCode.String );
                unitOfSaleParameter.DefaultValue = "S";
                _UnitOfSaleDataSource.SelectParameters.Add( unitOfSaleParameter );

                _UnitOfSaleDataSource.SetEventOwnerName( "UnitOfSale" );

                Session[ "UnitOfSaleDataSource" ] = _UnitOfSaleDataSource;
            }
            else
            {
                _UnitOfSaleDataSource = ( DocumentDataSource )Session[ "UnitOfSaleDataSource" ];
                _UnitOfSaleDataSource.RestoreDelegatesAfterDeserialization( this, "UnitOfSale" );
            }

            if( Session[ "UnitPackageDataSource" ] == null )
            {
                _UnitPackageDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, false );
                _UnitPackageDataSource.ID = "UnitPackageDataSource";
                _UnitPackageDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _UnitPackageDataSource.SelectCommand = "SelectSalePackagingMeasureUnits";
                _UnitPackageDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                Parameter unitPackageParameter = new Parameter( "UnitType", TypeCode.String );
                unitPackageParameter.DefaultValue = "P";
                _UnitPackageDataSource.SelectParameters.Add( unitPackageParameter );

                _UnitPackageDataSource.SetEventOwnerName( "UnitPackage" );

                Session[ "UnitPackageDataSource" ] = _UnitPackageDataSource;
            }
            else
            {
                _UnitPackageDataSource = ( DocumentDataSource )Session[ "UnitPackageDataSource" ];
                _UnitPackageDataSource.RestoreDelegatesAfterDeserialization( this, "UnitPackage" );
            }

            if( Session[ "UnitOfMeasureDataSource" ] == null )
            {
                _UnitOfMeasureDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, false );
                _UnitOfMeasureDataSource.ID = "UnitOfMeasureDataSource";
                _UnitOfMeasureDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _UnitOfMeasureDataSource.SelectCommand = "SelectSalePackagingMeasureUnits";
                _UnitOfMeasureDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                Parameter unitOfMeasureParameter = new Parameter( "UnitType", TypeCode.String );
                unitOfMeasureParameter.DefaultValue = "M";
                _UnitOfMeasureDataSource.SelectParameters.Add( unitOfMeasureParameter );

                _UnitOfMeasureDataSource.SetEventOwnerName( "UnitOfMeasure" );

                Session[ "UnitOfMeasureDataSource" ] = _UnitOfMeasureDataSource;
            }
            else
            {
                _UnitOfMeasureDataSource = ( DocumentDataSource )Session[ "UnitOfMeasureDataSource" ];
                _UnitOfMeasureDataSource.RestoreDelegatesAfterDeserialization( this, "UnitOfMeasure" );
            }

        }

        protected void UpdateItemDetailsButton_OnClick( object sender, EventArgs e )
        {
            string validationMessage = "";

            try
            {
                if( ValidateItemDetails( ref validationMessage ) == true )
                {
                    GetUpdateParameterValues();
                    _drugItemDetailsDataSource.Update();
                    DrugItemDetailsFormView.DataBind();
                    BindItemPackagingLists();

                    SetDistributorDataSource();
                    BindDistributorGrid();

                    SetSubItemDataSource();
                    BindSubItemGrid();

                    // allow the update postback to occur : added 4/14/2010
                    UpdatePanelEventProxy outerFormViewWasSavedUpdatePanelEventProxy = ( UpdatePanelEventProxy ) DrugItemDetailsFormView.FindControl( "OuterFormViewWasSavedUpdatePanelEventProxy" );
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

        private bool ValidateItemDetails( ref string validationMessage )
        {
            bool bSuccess = true;

            string dateEnteredMarket = "";
            string primeVendorChangedDate = "";
            string passThroughFlag = "";
            string vaClass = "";
            string includedFETAmountString = "";
            string quantityInUnitOfSaleString = "";
            string quantityInUnitPackage = "";
            string priceMultiplier = "";
            string priceDivider = "";
            string unitOfSale = "";
            string unitPackage = "";

            try
            {
                FormView drugItemDetailsFormView = ( FormView )DrugItemDetailsForm.FindControl( "DrugItemDetailsFormView" );
                if( drugItemDetailsFormView != null )
                {
                    if( drugItemDetailsFormView.CurrentMode == FormViewMode.Edit )
                    {
                        TextBox DateEnteredMarketTextBox = ( TextBox )drugItemDetailsFormView.FindControl( "DateEnteredMarketTextBox" );
                        if( DateEnteredMarketTextBox != null )
                            dateEnteredMarket = DateEnteredMarketTextBox.Text;

                        TextBox PrimeVendorChangedDateTextBox = ( TextBox )drugItemDetailsFormView.FindControl( "PrimeVendorChangedDateTextBox" );
                        if( PrimeVendorChangedDateTextBox != null )
                            primeVendorChangedDate = PrimeVendorChangedDateTextBox.Text;

                        TextBox PassThroughFlagTextBox = ( TextBox )drugItemDetailsFormView.FindControl( "PassThroughFlagTextBox" );
                        if( PassThroughFlagTextBox != null )
                            passThroughFlag = PassThroughFlagTextBox.Text;

                        TextBox VAClassTextBox = ( TextBox )drugItemDetailsFormView.FindControl( "VAClassTextBox" );
                        if( VAClassTextBox != null )
                            vaClass = VAClassTextBox.Text;

                        TextBox IncludedFETAmountTextBox = ( TextBox )drugItemDetailsFormView.FindControl( "IncludedFETAmountTextBox" );
                        if( IncludedFETAmountTextBox != null )
                            includedFETAmountString = IncludedFETAmountTextBox.Text;

                        TextBox QuantityInUnitOfSaleTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "QuantityInUnitOfSaleTextBox" );
                        if( QuantityInUnitOfSaleTextBox != null )
                            quantityInUnitOfSaleString = QuantityInUnitOfSaleTextBox.Text;

                        TextBox QuantityInUnitPackageTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "QuantityInUnitPackageTextBox" );
                        if( QuantityInUnitPackageTextBox != null )
                            quantityInUnitPackage = QuantityInUnitPackageTextBox.Text;

                        TextBox PriceMultiplierTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "PriceMultiplierTextBox" );
                        if( PriceMultiplierTextBox != null )
                            priceMultiplier = PriceMultiplierTextBox.Text;

                        TextBox PriceDividerTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "PriceDividerTextBox" );
                        if( PriceDividerTextBox != null )
                            priceDivider = PriceDividerTextBox.Text;

                        DropDownList UnitOfSaleDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitOfSaleDropDownList" );
                        if ( UnitOfSaleDropDownList != null )
                            unitOfSale = UnitOfSaleDropDownList.SelectedValue;

                        DropDownList UnitPackageDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitPackageDropDownList" );
                        if ( UnitPackageDropDownList != null )
                            unitPackage = UnitPackageDropDownList.SelectedValue;


                        DateTime parseDate;

                        if( dateEnteredMarket.Length > 0 )
                        {
                            if( DateTime.TryParse( dateEnteredMarket, out parseDate ) == false )
                            {
                                throw new Exception( "Date Entered Market is not a valid date." );
                            }
                        }

                        if( primeVendorChangedDate.Length > 0 )
                        {
                            if( DateTime.TryParse( primeVendorChangedDate, out parseDate ) == false )
                            {
                                throw new Exception( "Prime Vendor Changed Date is not a valid date." );
                            }
                        }

                        if( passThroughFlag.Trim().Length == 0 && _drugItemDetailsWindowParms.IsCOEditable == true ) 
                        {
                            throw new Exception( "Pass Through is a required field." );
                        }
                        else if( passThroughFlag.CompareTo( "T" ) != 0 && passThroughFlag.CompareTo( "F" ) != 0 && _drugItemDetailsWindowParms.IsCOEditable == true )
                        {
                            throw new Exception( "Pass Through must be T or F." );
                        }

                        if( vaClass.Trim().Length == 0 && _drugItemDetailsWindowParms.IsPBMEditable == true )
                        {
                            throw new Exception( "VA Class is a required field." );
                        }

                        decimal includedFETAmount;

                        if( Decimal.TryParse( includedFETAmountString, out includedFETAmount ) == false )
                        {
                            throw new Exception( "FET Amount is not a valid number." );
                        }

                        decimal quantityInUnitOfSale;

                        if ( Decimal.TryParse( quantityInUnitOfSaleString, out quantityInUnitOfSale ) == false )
                        {
                            throw new Exception( "Quantity In Unit Of Sale is not a valid number." );
                        }

                        decimal parseDecimal;

                        if( Decimal.TryParse( quantityInUnitPackage, out parseDecimal ) == false )
                        {
                            throw new Exception( "Quantity In Unit Package is not a valid number." );
                        }

                        int parseInt;

                        if( Int32.TryParse( priceMultiplier, out parseInt ) == false )
                        {
                            throw new Exception( "Quantity in Price Multiplier is not a valid number." );
                        }

                        if( Int32.TryParse( priceDivider, out parseInt ) == false )
                        {
                            throw new Exception( "Quantity in Price Divider is not a valid number." );
                        }

                        if( quantityInUnitOfSale == 1 )
                        {
                            if ( unitOfSale.Trim().CompareTo( unitPackage.Trim() ) != 0 )
                            {
                                throw new Exception( "If quantity in unit of sale is 1, then unit of sale and unit package must be the same." );
                            }
                        }

                        if( unitOfSale.Trim().CompareTo( unitPackage.Trim() ) == 0 )
                        {
                            if( quantityInUnitOfSale != 1 )
                            {
                                throw new Exception( "If unit of sale and unit package are the same, then quantity in unit of sale must be 1." );
                            }
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                bSuccess = false;
                validationMessage = string.Format( "The following exception was encountered validating the item details: {0}", ex.Message );
            }

            return ( bSuccess );
        }

        private void GetUpdateParameterValues()
        {
            FormView drugItemDetailsFormView = ( FormView )DrugItemDetailsForm.FindControl( "DrugItemDetailsFormView" );
            if( drugItemDetailsFormView != null )
            {
                if( drugItemDetailsFormView.CurrentMode == FormViewMode.Edit )
                {
                    // get extra id's which were saved during last bind event
                    int drugItemPackageId = ( int )Session[ "LastPackageIdForSelectedItemDetails" ];

                    // save the id's parameters for the update
                    _drugItemPackageIdParameter.DefaultValue = drugItemPackageId.ToString();

                    TextBox DateEnteredMarketTextBox = ( TextBox )drugItemDetailsFormView.FindControl( "DateEnteredMarketTextBox" );
                    if( DateEnteredMarketTextBox != null )
                        _DateEnteredMarketParameter.DefaultValue = DateEnteredMarketTextBox.Text;

                    //DropDownList DualPriceDesignationDropDownList = ( DropDownList )drugItemDetailsFormView.FindControl( "DualPriceDesignationDropDownList" );
                    //if( DualPriceDesignationDropDownList != null )
                    //    _DualPriceDesignationParameter.DefaultValue = DualPriceDesignationDropDownList.SelectedValue;

                    CheckBox PrimeVendorCheckBox = ( CheckBox )drugItemDetailsFormView.FindControl( "PrimeVendorFlagCheckBox" );
                    if( PrimeVendorCheckBox != null )
                    {
                        _PrimeVendorParameter.DefaultValue = ( PrimeVendorCheckBox.Checked == true ) ? "T" : "F";
                    }

                    TextBox PrimeVendorChangedDateTextBox = ( TextBox )drugItemDetailsFormView.FindControl( "PrimeVendorChangedDateTextBox" );
                    if( PrimeVendorChangedDateTextBox != null )
                        _PrimeVendorChangedDateParameter.DefaultValue = PrimeVendorChangedDateTextBox.Text;
                    
                    TextBox PassThroughFlagTextBox = ( TextBox )drugItemDetailsFormView.FindControl( "PassThroughFlagTextBox" );
                    if( PassThroughFlagTextBox != null )
                        _PassThroughFlagParameter.DefaultValue = PassThroughFlagTextBox.Text;
                    
                    TextBox VAClassTextBox = ( TextBox )drugItemDetailsFormView.FindControl( "VAClassTextBox" );
                    if( VAClassTextBox != null )
                        _VAClassParameter.DefaultValue = VAClassTextBox.Text;

                     DropDownList UnitOfSaleDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitOfSaleDropDownList" );
                    if( UnitOfSaleDropDownList != null )
                        _UnitOfSaleParameter.DefaultValue = UnitOfSaleDropDownList.SelectedValue;

                    TextBox QuantityInUnitOfSaleTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "QuantityInUnitOfSaleTextBox" );
                    if( QuantityInUnitOfSaleTextBox != null )
                        _QuantityInUnitOfSaleParameter.DefaultValue = QuantityInUnitOfSaleTextBox.Text;

                    DropDownList UnitPackageDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitPackageDropDownList" );
                    if( UnitPackageDropDownList != null )
                        _UnitPackageParameter.DefaultValue = UnitPackageDropDownList.SelectedValue;

                    TextBox QuantityInUnitPackageTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "QuantityInUnitPackageTextBox" );
                    if( QuantityInUnitPackageTextBox != null )
                        _QuantityInUnitPackageParameter.DefaultValue = QuantityInUnitPackageTextBox.Text;

                    DropDownList UnitOfMeasureDropDownList = ( DropDownList )DrugItemDetailsFormView.FindControl( "UnitOfMeasureDropDownList" );
                    if( UnitOfMeasureDropDownList != null )
                        _UnitOfMeasureParameter.DefaultValue = UnitOfMeasureDropDownList.SelectedValue;


                    CheckBox ExcludeFromExportCheckBox = ( CheckBox )drugItemDetailsFormView.FindControl( "ExcludeFromExportCheckBox" );
                    if( ExcludeFromExportCheckBox != null )
                        _ExcludeFromExportParameter.DefaultValue = ExcludeFromExportCheckBox.Checked.ToString();

                    CheckBox NonTAACheckBox = ( CheckBox )drugItemDetailsFormView.FindControl( "NonTAACheckBox" );
                    if( NonTAACheckBox != null )
                        _NonTAAParameter.DefaultValue = NonTAACheckBox.Checked.ToString();

                    TextBox IncludedFETAmountTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "IncludedFETAmountTextBox" );
                    if( IncludedFETAmountTextBox != null )
                        _IncludedFETAmountParameter.DefaultValue = IncludedFETAmountTextBox.Text;

                    TextBox PriceMultiplierTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "PriceMultiplierTextBox" );
                    if( PriceMultiplierTextBox != null )
                        _PriceMultiplierParameter.DefaultValue = PriceMultiplierTextBox.Text;

                    TextBox PriceDividerTextBox = ( TextBox )DrugItemDetailsFormView.FindControl( "PriceDividerTextBox" );
                    if( PriceDividerTextBox != null )
                        _PriceDividerParameter.DefaultValue = PriceDividerTextBox.Text;

                }
            }
        }

        protected void CancelItemDetailsButton_OnClick( object sender, EventArgs e )
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

        //protected void DateEnteredMarketLabel_OnInit( object sender, EventArgs e )
        //{
        //    Label DateEnteredMarketLabel = ( Label )sender;
        //    if( DateEnteredMarketLabel != null )
        //        MultiLineLabelText( DateEnteredMarketLabel, new string[] { "Date Entered", "Market" } );
        //}

        //protected void DualPriceDesignationLabel_OnInit( object sender, EventArgs e )
        //{
        //    Label DualPriceDesignationLabel = ( Label )sender;
        //    if( DualPriceDesignationLabel != null )
        //        MultiLineLabelText( DualPriceDesignationLabel, new string[] { "Dual Price", "Designation" } );
        //}

        //protected void PrimeVendorChangedDateLabel_OnInit( object sender, EventArgs e )
        //{
        //    Label PrimeVendorChangedDateLabel = ( Label )sender;
        //    if( PrimeVendorChangedDateLabel != null )
        //        MultiLineLabelText( PrimeVendorChangedDateLabel, new string[] { "Prime Vendor", "Changed Date" } );
        //}

        //protected void PassThroughFlagLabel_OnInit( object sender, EventArgs e )
        //{
        //    Label PassThroughFlagLabel = ( Label )sender;
        //    if( PassThroughFlagLabel != null )
        //        MultiLineLabelText( PassThroughFlagLabel, new string[] { "Pass", "Through", "Flag" } );
        //}

        //protected void VAClassLabel_OnInit( object sender, EventArgs e )
        //{
        //    Label VAClassLabel = ( Label )sender;
        //    if( VAClassLabel != null )
        //        MultiLineLabelText( VAClassLabel, new string[] { "VA", "Class" } );
        //}

        //protected void ExcludeFromExportLabel_OnInit( object sender, EventArgs e )
        //{
        //    Label ExcludeFromExportLabel = ( Label )sender;
        //    if( ExcludeFromExportLabel != null )
        //        MultiLineLabelText( ExcludeFromExportLabel, new string[] { "Exclude From", "Export" } );
        //}


        protected string MultilineText( string[] textArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < textArray.Count(); i++ )
            {
                sb.AppendLine( textArray[ i ] );
                if( i < textArray.Count() - 1 )
                    sb.Append( "<br>" );
            }

            return( sb.ToString() );
        }

        //// select dual price designation from drop down during edit
        //protected void DualPriceDesignationDropDownList_DataBound( object sender, EventArgs e )
        //{
        //    DropDownList dualPriceDesignationDropDownList = ( DropDownList )sender;
        //    FormView containingFormView = ( FormView )dualPriceDesignationDropDownList.NamingContainer;

        //    DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

        //    string dualPriceDesignation = "F";

        //    if( currentRow[ "DualPriceDesignation" ] != DBNull.Value )
        //    {
        //        dualPriceDesignation = currentRow[ "DualPriceDesignation" ].ToString();
        //    }

        //    ListItem listItem = dualPriceDesignationDropDownList.Items.FindByValue( dualPriceDesignation );
        //    if( listItem != null )
        //        listItem.Selected = true;
           
        //}

        // select unit of sale from drop down during edit
        protected void UnitOfSaleDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList unitOfSaleDropDownList = ( DropDownList )sender;
            FormView containingFormView = ( FormView )unitOfSaleDropDownList.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            string selectedUnitOfSale = "EA";

            if( currentRow[ "UnitOfSale" ] != DBNull.Value )
            {
                selectedUnitOfSale = currentRow[ "UnitOfSale" ].ToString();
            }

            ListItem listItem = unitOfSaleDropDownList.Items.FindByValue( selectedUnitOfSale );
            if( listItem != null )
                listItem.Selected = true;

        }

        // select unit package from drop down during edit
        protected void UnitPackageDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList unitPackageDropDownList = ( DropDownList )sender;
            FormView containingFormView = ( FormView )unitPackageDropDownList.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            string selectedUnitPackage = "EA";

            if( currentRow[ "UnitPackage" ] != DBNull.Value )
            {
                selectedUnitPackage = currentRow[ "UnitPackage" ].ToString();
            }

            ListItem listItem = unitPackageDropDownList.Items.FindByValue( selectedUnitPackage );
            if( listItem != null )
                listItem.Selected = true;

        }

        // select unit of measure from drop down during edit
        protected void UnitOfMeasureDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList unitOfMeasureDropDownList = ( DropDownList )sender;
            FormView containingFormView = ( FormView )unitOfMeasureDropDownList.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            string selectedUnitOfMeasure = "EA";

            if( currentRow[ "UnitOfMeasure" ] != DBNull.Value )
            {
                selectedUnitOfMeasure = currentRow[ "UnitOfMeasure" ].ToString();
            }

            ListItem listItem = unitOfMeasureDropDownList.Items.FindByValue( selectedUnitOfMeasure );
            if( listItem != null )
                listItem.Selected = true;

        }

        protected void ExcludeFromExportCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox excludeFromExportCheckBox = ( CheckBox )sender;

            FormView containingFormView = ( FormView )excludeFromExportCheckBox.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            bool bIsChecked = false;

            if( currentRow[ "ExcludeFromExport" ] != DBNull.Value )
            {
                bIsChecked =  bool.Parse( currentRow[ "ExcludeFromExport" ].ToString() );
            }

            excludeFromExportCheckBox.Checked = bIsChecked;
        }


        protected void NonTAACheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox nonTAACheckBox = ( CheckBox )sender;

            FormView containingFormView = ( FormView )nonTAACheckBox.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            bool bIsChecked = false;

            if( currentRow[ "NonTAA" ] != DBNull.Value )
            {
                bIsChecked = bool.Parse( currentRow[ "NonTAA" ].ToString() );
            }

            nonTAACheckBox.Checked = bIsChecked;
        }

        protected void PrimeVendorFlagCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox primeVendorFlagCheckBox = ( CheckBox )sender;

            FormView containingFormView = ( FormView )primeVendorFlagCheckBox.NamingContainer;

            DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

            bool bIsChecked = false;

            if( currentRow[ "PrimeVendor" ] != DBNull.Value )
            {
                string primeVendorString = currentRow[ "PrimeVendor" ].ToString();
                bIsChecked = ( bool )(( primeVendorString == "T" ) ? true : false );
            }

            primeVendorFlagCheckBox.Checked = bIsChecked;
        }

#region DistributorGrid

        private void LoadSpecialtyDistributors()
        {
            if( Session[ "DistributorNameDataSource" ] == null )
            {
                _distributorNameDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, false );
                _distributorNameDataSource.ID = "DistributorNameDataSource";
                _distributorNameDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _distributorNameDataSource.SelectCommand = "SelectDistributorNamesForDrugItem";
                _distributorNameDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                _distributorNameDataSource.SetEventOwnerName( "DistributorName" );

                Session[ "DistributorNameDataSource" ] = _distributorNameDataSource;
            }
            else
            {
                _distributorNameDataSource = ( DocumentDataSource )Session[ "DistributorNameDataSource" ];

                _distributorNameDataSource.RestoreDelegatesAfterDeserialization( this, "DistributorName" );  
            }

            if( Session[ "SpecialtyDistributorDataSource" ] == null )
            {
                _specialtyDistributorDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, true );
                _specialtyDistributorDataSource.ID = "SpecialtyDistributorDataSource";
                _specialtyDistributorDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _specialtyDistributorDataSource.SelectCommand = "SelectDistributorsForDrugItem";
 
                _specialtyDistributorDataSource.UpdateCommand = "UpdateDistributorForDrugItem";

                _specialtyDistributorDataSource.InsertCommand = "InsertDistributorForDrugItem";
                _specialtyDistributorDataSource.SetEventOwnerName( "SpecialtyDistributor" );
                _specialtyDistributorDataSource.Inserted += new SqlDataSourceStatusEventHandler( _distributorDataSource_Inserted );

                _specialtyDistributorDataSource.Updated += new SqlDataSourceStatusEventHandler( _distributorDataSource_Updated );

                _specialtyDistributorDataSource.DeleteCommand = "DeleteDrugItemDistributor";

                CreateDistributorDataSourceParameters();

                _specialtyDistributorDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _specialtyDistributorDataSource.SelectParameters.Add( _withAddDistributorParameter );
                _withAddDistributorParameter.DefaultValue = "false"; // not adding
                _specialtyDistributorDataSource.SelectParameters.Add( _selectParentItemDistributors );
                _selectParentItemDistributors.DefaultValue = _drugItemDetailsWindowParms.IsBPA.ToString(); // use parent item if bpa
                
                _specialtyDistributorDataSource.SelectParameters.Add( _drugItemIdParameter );

                _specialtyDistributorDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _specialtyDistributorDataSource.UpdateParameters.Add( _drugItemIdParameter ); 
                _specialtyDistributorDataSource.UpdateParameters.Add( _drugItemDistributorIdParameter );
                _specialtyDistributorDataSource.UpdateParameters.Add( _distributorNameParameter );
                _specialtyDistributorDataSource.UpdateParameters.Add( _distributorPhoneParameter );
                _specialtyDistributorDataSource.UpdateParameters.Add( _distributorContactPersonParameter );
                _specialtyDistributorDataSource.UpdateParameters.Add( _distributorNotesParameter );            

                _specialtyDistributorDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
                _specialtyDistributorDataSource.InsertParameters.Add( _drugItemIdParameter );
                _specialtyDistributorDataSource.InsertParameters.Add( _distributorNameParameter );
                _specialtyDistributorDataSource.InsertParameters.Add( _distributorPhoneParameter );
                _specialtyDistributorDataSource.InsertParameters.Add( _distributorContactPersonParameter );
                _specialtyDistributorDataSource.InsertParameters.Add( _distributorNotesParameter ); 
                _specialtyDistributorDataSource.InsertParameters.Add( _drugItemDistributorIdForInsertParameter );

                _specialtyDistributorDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
                _specialtyDistributorDataSource.DeleteParameters.Add( _drugItemDistributorIdParameter );
               
                Session[ "SpecialtyDistributorDataSource" ] = _specialtyDistributorDataSource;
            }
            else
            {
                _specialtyDistributorDataSource = ( DocumentDataSource )Session[ "SpecialtyDistributorDataSource" ];

                _specialtyDistributorDataSource.RestoreDelegatesAfterDeserialization( this, "SpecialtyDistributor" ); // added 4/14/2010

                RestoreDistributorDataSourceParameters( _specialtyDistributorDataSource ); 
                
            }

        }


        private void SetDistributorDataSource()
        {
            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );
            distributorGridView.DataSource = _specialtyDistributorDataSource;
        }

        private void BindDistributorGrid()
        {
            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );
            distributorGridView.DataBind();
        }   
  
        private void CreateDistributorDataSourceParameters()
        {
            // select 
            _withAddDistributorParameter = new Parameter( "WithAdd", TypeCode.Boolean );
            _selectParentItemDistributors = new Parameter( "UseParentItem", TypeCode.Boolean );

            // insert        
            _drugItemDistributorIdForInsertParameter = new Parameter( "DrugItemDistributorId", TypeCode.Int32 );
            _drugItemDistributorIdForInsertParameter.Direction = ParameterDirection.Output;

            // insert and update
            _distributorNameParameter = new Parameter( "DistributorName", TypeCode.String );
            _distributorPhoneParameter = new Parameter( "Phone", TypeCode.String );
            _distributorNotesParameter = new Parameter( "Notes", TypeCode.String );
            _distributorContactPersonParameter = new Parameter( "ContactPerson", TypeCode.String );

            // update and delete 
            _drugItemDistributorIdParameter = new Parameter( "DrugItemDistributorId", TypeCode.Int32 );
        }

        private void RestoreDistributorDataSourceParameters( DocumentDataSource distributorDataSource )
        {
            _withAddDistributorParameter = distributorDataSource.SelectParameters[ "WithAdd" ];
            _selectParentItemDistributors = distributorDataSource.SelectParameters[ "UseParentItem" ];

            // insert
            _drugItemDistributorIdForInsertParameter = distributorDataSource.InsertParameters[ "DrugItemDistributorId" ];
  
            // insert and update
            _distributorNameParameter = distributorDataSource.UpdateParameters[ "DistributorName" ];
            _distributorPhoneParameter = distributorDataSource.UpdateParameters[ "Phone" ];
            _distributorNotesParameter = distributorDataSource.UpdateParameters[ "Notes" ];
            _distributorContactPersonParameter = distributorDataSource.UpdateParameters[  "ContactPerson" ];

            // delete 
            _drugItemDistributorIdParameter = distributorDataSource.DeleteParameters[ "DrugItemDistributorId" ];
        }

        protected void DistributorGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightDistributorRow( 0 );
        }

        protected void HighlightDistributorRow( int itemIndex )
        {
            // uninitialized index
            if( itemIndex < 0 )
                return;

            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );

            if( distributorGridView.HasData() == true )
            {
                GridViewRow row = distributorGridView.Rows[ itemIndex ];
                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = distributorGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = distributorGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setDistributorHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveDistributorHighlightingScript", preserveHighlightingScript, true );

                // allow the highlight postback to occur 
                UpdatePanelEventProxy changeDistributorHighlightUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemDetailsFormView.FindControl( "ChangeDistributorHighlightUpdatePanelEventProxy" );
                changeDistributorHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        protected void DistributorGridView_RowDataBound( object sender, GridViewRowEventArgs e )
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
                    else // bind list of names in ddl during edit
                    {
                        ComboBox distributorNameComboBox = ( ComboBox )e.Row.FindControl( "distributorNameComboBox" );
                        distributorNameComboBox.DataSource = _distributorNameDataSource;
                        distributorNameComboBox.DataBind();
                    }

                    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
                    if( currentDocument != null )
                    {
                        if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemDetailsPBM ) == false &&
                            currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemDetailsCO ) == false )
                        {
                            Button editButton = ( Button )e.Row.FindControl( "EditButton" );
                            if( editButton != null )
                                editButton.Enabled = false;

                            Button removeDistributorButton = ( Button )e.Row.FindControl( "RemoveDistributorButton" );
                            if( removeDistributorButton != null )
                                removeDistributorButton.Enabled = false;
                        }
                    }

  
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }


        protected void distributorNameComboBox_DataBound( object sender, EventArgs e )
        {
            ComboBox distributorNameComboBox = ( ComboBox )sender;
            GridViewRow gridViewRow = ( GridViewRow )distributorNameComboBox.NamingContainer;
            string currentDistributorName = "";

            if( gridViewRow.DataItem != null )
            {
                currentDistributorName = ( ( DataRowView )gridViewRow.DataItem )[ "DistributorName" ].ToString();
                if( currentDistributorName.Length > 0 )
                {
                    ListItem listItem = distributorNameComboBox.Items.FindByValue( currentDistributorName );
                    if( listItem != null )
                        listItem.Selected = true;
                }
            }
        }

        protected void DistributorGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedDrugItemDistributorId = -1;
            int itemIndex = -1;

            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );

            if( e.CommandName.CompareTo( "RemoveDistributor" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemDistributorId = Int32.Parse( commandArgs[ 1 ].ToString() );

                int newRowIndex = DeleteDistributor( distributorGridView, itemIndex, selectedDrugItemDistributorId );

                HighlightDistributorRow( newRowIndex );
            }

            else if( e.CommandName.CompareTo( "EditItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemDistributorId = Int32.Parse( commandArgs[ 1 ].ToString() );

                HighlightDistributorRow( itemIndex );

                InitiateEditModeForDistributorGridItem( itemIndex );

            }
            // save update or save insert
            else if( e.CommandName.CompareTo( "SaveItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemDistributorId = Int32.Parse( commandArgs[ 1 ].ToString() );

                // is this an insert or an update
                int newOrUpdatedRowIndex = -1;
                if( distributorGridView.InsertRowActive == true )
                {
                    newOrUpdatedRowIndex = InsertDistributor( distributorGridView, itemIndex );
                }
                else
                {
                    newOrUpdatedRowIndex = UpdateDistributor( distributorGridView, itemIndex );
                }

                HighlightDistributorRow( newOrUpdatedRowIndex );

                //    MsgBox.AlertFromUpdatePanel( Page, validationMessage );
            }
            // cancel update or cancel insert
            else if( e.CommandName.CompareTo( "Cancel" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemDistributorId = Int32.Parse( commandArgs[ 1 ].ToString() );

            }
            // can trap editing commands here if needed, prior to individual editing events
        }

        protected void AddNewDistributorButton_OnClick( object sender, EventArgs e )
        {
            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );

            distributorGridView.Insert();

            _withAddDistributorParameter.DefaultValue = "true";

            distributorGridView.DataBind();

            InitiateEditModeForDistributorGridItem( 0 );

            // allow the update postback to occur
            UpdatePanelEventProxy insertDistributorButtonClickUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemDetailsFormView.FindControl( "InsertDistributorButtonClickUpdatePanelEventProxy" );
            insertDistributorButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            HighlightDistributorRow( 0 );

        }


        protected void DistributorGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForDistributorGridItem( e.NewEditIndex );
        }

        private void InitiateEditModeForDistributorGridItem( int editIndex )
        {
            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );

            distributorGridView.EditIndex = editIndex;

            // select the edited item also
            SetDistributorGridViewSelectedItem( editIndex );

            distributorGridView.DataBind();

            // disable appropriate buttons for the selected row
            SetEnabledDistributorGridControlsDuringEdit( distributorGridView, editIndex, false );
        }

        private void SetEnabledDistributorGridControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )
        {
            gv.SetEnabledControlsForCell( rowIndex, 5, bEnabled ); // remove distributor

            gv.SetVisibleControlsForCell( rowIndex, 0, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 0, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 0, "CancelButton", !bEnabled );
        }

        private void SetDistributorGridViewSelectedItem( int selectedItemIndex )
        {
            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );

            // save for postback
            Session[ "DistributorGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            distributorGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            ScrollToSelectedDistributorItem();

            // allow the update postback to occur
            UpdatePanelEventProxy insertDistributorButtonClickUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemDetailsFormView.FindControl( "InsertDistributorButtonClickUpdatePanelEventProxy" );
            insertDistributorButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedDistributorItem()
        {
            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );

            int rowIndex = distributorGridView.SelectedIndex;
            //  TableItemStyle rowStyle = DrugItemsGridView.RowStyle;
            //  int rowHeight = ( int )rowStyle.Height.Value;  // this value is always zero
            int rowPosition = DISTRIBUTORGRIDVIEWROWHEIGHTESTIMATE * rowIndex;

            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
            //     ScriptManager.RegisterClientScriptBlock( this.Page, this.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true );  // runs prior to control rendering
        }

        private void RestoreDistributorGridViewSelectedItem()
        {
            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );

            distributorGridView.SelectedIndex = ( int )Session[ "DistributorGridViewSelectedIndex" ];
        }

        protected void DistributorGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            GridView distributorGridView = ( GridView )DrugItemDetailsFormView.FindControl( "DistributorGridView" );

            int cancelIndex = e.RowIndex;
            bool bInserting = distributorGridView.InsertRowActive;

            // if inserting
            if( bInserting == true )
            {
                distributorGridView.InsertRowActive = false; // cancels insert ( if inserting )
                _withAddDistributorParameter.DefaultValue = "false";
                distributorGridView.EditIndex = -1; // cancels the edit
                distributorGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledDistributorGridControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                HighlightDistributorRow( 0 );
            }
            else // editing existing row
            {
                distributorGridView.EditIndex = -1; // cancels the edit
                distributorGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledDistributorGridControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                HighlightDistributorRow( cancelIndex );
            }
        }




        private int UpdateDistributor( GridView gv, int rowIndex )
        {
            int updatedRowIndex = -1;

            try
            {
                _drugItemDistributorIdParameter.DefaultValue = gv.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

                string distributorName = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "distributorNameComboBox" );
                _distributorNameParameter.DefaultValue = distributorName.Trim();

                string contactName = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "contactNameTextBox" );
                _distributorContactPersonParameter.DefaultValue = contactName.Trim();
                
                string distributorPhoneNumber = gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "distributorPhoneTextBox" );
                _distributorPhoneParameter.DefaultValue = distributorPhoneNumber.Trim();
                
                string distributorNotes = gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "distributorNotesTextBox" );
                _distributorNotesParameter.DefaultValue = distributorNotes.Trim();
               
                _specialtyDistributorDataSource.Update();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.EditIndex = -1; // done with edit
            //         SetDrugItemsGridViewSelectedItem( rowIndex );
            gv.DataBind();

            if( Session[ "LastUpdatedDrugItemDistributorId" ] != null )
            {
                int updatedDistributorId = ( int )Session[ "LastUpdatedDrugItemDistributorId" ];
                updatedRowIndex = gv.GetRowIndexFromId( updatedDistributorId, 0 );

                SetDistributorGridViewSelectedItem( updatedRowIndex );

                // bind to select
                gv.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledDistributorGridControlsDuringEdit( gv, updatedRowIndex, true );
            }

            return ( updatedRowIndex );
        }



        private int InsertDistributor( GridView gv, int rowIndex )
        {
            int insertedRowIndex = 0;

            string distributorName = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "distributorNameComboBox" );
            _distributorNameParameter.DefaultValue = distributorName.Trim();

            string contactName = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "contactNameTextBox" );
            _distributorContactPersonParameter.DefaultValue = contactName.Trim();

            string distributorPhoneNumber = gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "distributorPhoneTextBox" );
            _distributorPhoneParameter.DefaultValue = distributorPhoneNumber.Trim();

            string distributorNotes = gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "distributorNotesTextBox" );
            _distributorNotesParameter.DefaultValue = distributorNotes.Trim();
           
            try
            {
                _specialtyDistributorDataSource.Insert();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.InsertRowActive = false; // done with insert
            gv.EditIndex = -1; // done with edit of new row
            _withAddDistributorParameter.DefaultValue = "false"; // no extra row

            // bind with new row
            gv.DataBind();

            if( Session[ "LastInsertedDrugItemDistributorId" ] != null )
            {
                int newDistributorId = ( int )Session[ "LastInsertedDrugItemDistributorId" ];
                insertedRowIndex = gv.GetRowIndexFromId( newDistributorId, 0 );

                SetDistributorGridViewSelectedItem( insertedRowIndex );

                // bind to select
                gv.DataBind();
            }

            return ( insertedRowIndex );
        }

        protected void _distributorDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemDistributorIdString = e.Command.Parameters[ "@DrugItemDistributorId" ].Value.ToString();

            if( drugItemDistributorIdString.Length > 0 )
            {
                int drugItemDistributorId = int.Parse( drugItemDistributorIdString );
                Session[ "LastInsertedDrugItemDistributorId" ] = drugItemDistributorId;
            }
        }

        // probably wont happen - id changing during update
        protected void _distributorDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemDistributorIdString = e.Command.Parameters[ "@DrugItemDistributorId" ].Value.ToString();

            if( drugItemDistributorIdString.Length > 0 )
            {
                int drugItemDistributorId = int.Parse( drugItemDistributorIdString );
                Session[ "LastUpdatedDrugItemDistributorId" ] = drugItemDistributorId;
            }
        }

        private int DeleteDistributor( GridView gv, int rowIndex, int selectedDrugItemDistributorId )
        {
            // id of row to delete
            _drugItemDistributorIdParameter.DefaultValue = selectedDrugItemDistributorId.ToString();

            try
            {
                _specialtyDistributorDataSource.Delete();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            // previous row gets focus
            if( rowIndex >= 1 )
                rowIndex--;

            SetDistributorGridViewSelectedItem( rowIndex );

            gv.DataBind();

            return ( rowIndex );
        }

        protected void RemoveDistributorButton_DataBinding( object sender, EventArgs e )
        {
            Button removeDistributorButton = ( Button )sender;
            if( removeDistributorButton != null )
                MultiLineButtonText( removeDistributorButton, new string[] { "Remove", "Distributor" } );
        }

    
#endregion DistributorGrid

#region SubItemGrid

        private void LoadSubItems()
        {
            if( Page.Session[ "SubItemDataSource" ] == null )
            {
                _subItemDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, true );
                _subItemDataSource.ID = "SubItemDataSource";
                _subItemDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _subItemDataSource.SelectCommand = "SelectFSSDrugItemSubItems";

                _subItemDataSource.UpdateCommand = "UpdateFSSDrugItemSubItem";

                _subItemDataSource.InsertCommand = "InsertFSSDrugItemSubItem";
                _subItemDataSource.SetEventOwnerName( "SubItem" );
                _subItemDataSource.Inserted += new SqlDataSourceStatusEventHandler( _subItemDataSource_Inserted );

                _subItemDataSource.Updated += new SqlDataSourceStatusEventHandler( _subItemDataSource_Updated );

                _subItemDataSource.DeleteCommand = "DeleteFSSDrugItemSubItem";

                CreateSubItemDataSourceParameters();

                _subItemDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
            //    _subItemDataSource.SelectParameters.Add( _contractNumberParameter );
                _subItemDataSource.SelectParameters.Add( _withAddSubItemParameter );
                _withAddSubItemParameter.DefaultValue = "false"; // not adding
                _subItemDataSource.SelectParameters.Add( _drugItemIdParameter );

                _subItemDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _subItemDataSource.UpdateParameters.Add( _contractNumberParameter );
                _subItemDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
                _subItemDataSource.UpdateParameters.Add( _drugItemIdParameter );
                _subItemDataSource.UpdateParameters.Add( _drugItemSubItemIdParameter );
                _subItemDataSource.UpdateParameters.Add( _dispensingUnitParameter );
                _subItemDataSource.UpdateParameters.Add( _tradeNameParameter );
                _subItemDataSource.UpdateParameters.Add( _genericParameter );
                _subItemDataSource.UpdateParameters.Add( _packageDescriptionParameter );
                _subItemDataSource.UpdateParameters.Add( _subItemIdentifierParameter );

                _subItemDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
                _subItemDataSource.InsertParameters.Add( _contractNumberParameter );
                _subItemDataSource.InsertParameters.Add( _modificationStatusIdParameter );
                _subItemDataSource.InsertParameters.Add( _drugItemIdParameter );
                _subItemDataSource.InsertParameters.Add( _drugItemSubItemIdForInsertParameter );
                _subItemDataSource.InsertParameters.Add( _dispensingUnitParameter );
                _subItemDataSource.InsertParameters.Add( _tradeNameParameter );
                _subItemDataSource.InsertParameters.Add( _genericParameter );
                _subItemDataSource.InsertParameters.Add( _packageDescriptionParameter );
                _subItemDataSource.InsertParameters.Add( _subItemIdentifierParameter );

                _subItemDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
                _subItemDataSource.DeleteParameters.Add( _contractNumberParameter );
                _subItemDataSource.DeleteParameters.Add( _modificationStatusIdParameter );
                _subItemDataSource.DeleteParameters.Add( _drugItemSubItemIdParameter );
                _subItemDataSource.DeleteParameters.Add( _drugItemIdParameter );
                _subItemDataSource.DeleteParameters.Add( _subItemIdentifierParameter );

                // save to session
                Page.Session[ "SubItemDataSource" ] = _subItemDataSource;
            }
            else
            {
                _subItemDataSource = ( DocumentDataSource )Page.Session[ "SubItemDataSource" ];

                _subItemDataSource.RestoreDelegatesAfterDeserialization( this, "SubItem" ); // added 4/14/2010

                RestoreSubItemDataSourceParameters( _subItemDataSource );
            }


        }

        private void SetSubItemDataSource()
        {
            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );
            subItemGridView.DataSource = _subItemDataSource;
        }

        private void BindSubItemGrid()
        {
            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );
            subItemGridView.DataBind();
        }

        private void CreateSubItemDataSourceParameters()
        {
            // select ( _drugItemIdParameter is shared with formview parms )
            _withAddSubItemParameter = new Parameter( "WithAdd", TypeCode.Boolean );

            // insert ( _contractNumberParameter is shared with formview parms )           
            _dispensingUnitParameter = new Parameter( "DispensingUnit", TypeCode.String );
            _tradeNameParameter  = new Parameter( "TradeName", TypeCode.String );
            _genericParameter = new Parameter( "Generic", TypeCode.String );
            _packageDescriptionParameter = new Parameter( "PackageDescription", TypeCode.String );
            _subItemIdentifierParameter = new Parameter( "SubItemIdentifier", TypeCode.String );
            _drugItemSubItemIdForInsertParameter = new Parameter( "DrugItemSubItemId", TypeCode.Int32 );
            _drugItemSubItemIdForInsertParameter.Direction = ParameterDirection.Output;

            // update ( _contractNumberParameter, _drugItemIdParameter are shared with formview parms )
            _drugItemSubItemIdParameter = new Parameter( "DrugItemSubItemId", TypeCode.Int32 );
           
        }

        private void RestoreSubItemDataSourceParameters( DocumentDataSource subItemDataSource )
        {
            _withAddSubItemParameter = subItemDataSource.SelectParameters[ "WithAdd" ];

            _drugItemSubItemIdForInsertParameter = subItemDataSource.InsertParameters[ "DrugItemSubItemId" ];

            _subItemIdentifierParameter = subItemDataSource.UpdateParameters[ "SubItemIdentifier" ];
            _tradeNameParameter = subItemDataSource.UpdateParameters[ "TradeName" ];
            _genericParameter = subItemDataSource.UpdateParameters[ "Generic" ];
            _packageDescriptionParameter = subItemDataSource.UpdateParameters[ "PackageDescription" ];
            _dispensingUnitParameter = subItemDataSource.UpdateParameters[ "DispensingUnit" ];
            _drugItemSubItemIdParameter = subItemDataSource.UpdateParameters[ "DrugItemSubItemId" ];
        }

        protected void SubItemGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightSubItemRow( 0 );
        }

        protected void HighlightSubItemRow( int itemIndex )
        {
            // uninitialized index
            if( itemIndex < 0 )
                return;

            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );

            if( subItemGridView.HasData() == true )
            {
                GridViewRow row = subItemGridView.Rows[ itemIndex ];
                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = subItemGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = subItemGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setSubItemHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveHighlightingScript", preserveHighlightingScript, true );

                // allow the highlight postback to occur 
                UpdatePanelEventProxy changeSubItemHighlightUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemDetailsFormView.FindControl( "ChangeSubItemHighlightUpdatePanelEventProxy" );
                changeSubItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        protected void SubItemGridView_RowDataBound( object sender, GridViewRowEventArgs e )
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

                    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
                    if( currentDocument != null )
                    {
                        if( currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.PharmItemDetailsPBM ) == false )
                        {
                            Button editButton = ( Button )e.Row.FindControl( "EditButton" );
                            if( editButton != null )
                                editButton.Enabled = false;

                            Button removeSubItemButton = ( Button )e.Row.FindControl( "RemoveSubItemButton" );
                            if( removeSubItemButton != null )
                                removeSubItemButton.Enabled = false;
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }



        protected void SubItemGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedDrugItemSubItemId = -1;
            int itemIndex = -1;

            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );

            if( e.CommandName.CompareTo( "RemoveSubItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemSubItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                int newRowIndex = DeleteSubItem( subItemGridView, itemIndex, selectedDrugItemSubItemId );

                HighlightSubItemRow( newRowIndex );
            }

            else if( e.CommandName.CompareTo( "EditItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemSubItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                HighlightSubItemRow( itemIndex );

                InitiateEditModeForSubItemGridItem( itemIndex );

            }
            // save update or save insert
            else if( e.CommandName.CompareTo( "SaveItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemSubItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                // is this an insert or an update
                int newOrUpdatedRowIndex = -1;
                if( subItemGridView.InsertRowActive == true )
                {
                    newOrUpdatedRowIndex = InsertSubItem( subItemGridView, itemIndex );
                }
                else
                {
                    newOrUpdatedRowIndex = UpdateSubItem( subItemGridView, itemIndex );
                }

                HighlightSubItemRow( newOrUpdatedRowIndex );

                //    MsgBox.AlertFromUpdatePanel( Page, validationMessage );
            }
            // cancel update or cancel insert
            else if( e.CommandName.CompareTo( "Cancel" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemSubItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

            }
            // can trap editing commands here if needed, prior to individual editing events
        }

        protected void AddNewSubItemButton_OnClick( object sender, EventArgs e )
        {
            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );

            subItemGridView.Insert();

            _withAddSubItemParameter.DefaultValue = "true";

            subItemGridView.DataBind();

            InitiateEditModeForSubItemGridItem( 0 );

            // allow the update postback to occur
            UpdatePanelEventProxy insertSubItemButtonClickUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemDetailsFormView.FindControl( "InsertSubItemButtonClickUpdatePanelEventProxy" );
            insertSubItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            HighlightSubItemRow( 0 );

        }


        protected void SubItemGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForSubItemGridItem( e.NewEditIndex );
        }

        private void InitiateEditModeForSubItemGridItem( int editIndex )
        {
            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );

            subItemGridView.EditIndex = editIndex;

            // select the edited item also
            SetSubItemGridViewSelectedItem( editIndex );

            subItemGridView.DataBind();

            // disable appropriate buttons for the selected row
            SetEnabledSubItemGridControlsDuringEdit( subItemGridView, editIndex, false );
        }

        private void SetEnabledSubItemGridControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )
        {
            gv.SetEnabledControlsForCell( rowIndex, 6, bEnabled ); // remove sub-item

            gv.SetVisibleControlsForCell( rowIndex, 0, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 0, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 0, "CancelButton", !bEnabled );
        }

        private void SetSubItemGridViewSelectedItem( int selectedItemIndex )
        {
            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );

            // save for postback
            Session[ "SubItemGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            subItemGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            ScrollToSelectedItem();

            // allow the update postback to occur
            UpdatePanelEventProxy insertSubItemButtonClickUpdatePanelEventProxy = ( UpdatePanelEventProxy )DrugItemDetailsFormView.FindControl( "InsertSubItemButtonClickUpdatePanelEventProxy" );
            insertSubItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedItem()
        {
            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );

            int rowIndex = subItemGridView.SelectedIndex;
            //  TableItemStyle rowStyle = DrugItemsGridView.RowStyle;
            //  int rowHeight = ( int )rowStyle.Height.Value;  // this value is always zero
            int rowPosition = SUBITEMGRIDVIEWROWHEIGHTESTIMATE * rowIndex;

            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
            //     ScriptManager.RegisterClientScriptBlock( this.Page, this.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true );  // runs prior to control rendering
        }

        private void RestoreSubItemGridViewSelectedItem()
        {
            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );

            subItemGridView.SelectedIndex = ( int )Session[ "SubItemGridViewSelectedIndex" ];
        }

        protected void SubItemGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            GridView subItemGridView = ( GridView )DrugItemDetailsFormView.FindControl( "SubItemGridView" );

            int cancelIndex = e.RowIndex;
            bool bInserting = subItemGridView.InsertRowActive;

            // if inserting
            if( bInserting == true )
            {
                subItemGridView.InsertRowActive = false; // cancels insert ( if inserting )
                _withAddSubItemParameter.DefaultValue = "false";
                subItemGridView.EditIndex = -1; // cancels the edit
                subItemGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledSubItemGridControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                HighlightSubItemRow( 0 );
            }
            else // editing existing row
            {
                subItemGridView.EditIndex = -1; // cancels the edit
                subItemGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledSubItemGridControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                HighlightSubItemRow( cancelIndex );
            }
        }

 


        private int UpdateSubItem( GridView gv, int rowIndex )
        {
            int updatedRowIndex = -1;

            try
            {
                _drugItemSubItemIdParameter.DefaultValue = gv.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

                string subItemIdentifier = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "subItemIdentifierTextBox" );
                _subItemIdentifierParameter.DefaultValue = subItemIdentifier.Trim();
                string generic = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "genericNameTextBox" );
                _genericParameter.DefaultValue = generic.Trim();
                string tradeName = gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "tradeNameTextBox" );
                _tradeNameParameter.DefaultValue = tradeName.Trim();
                string packageDescription = gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "packageDescriptionTextBox" );
                _packageDescriptionParameter.DefaultValue = packageDescription.Trim();
                string dispensingUnit = gv.GetStringValueFromSelectedControl( rowIndex, 5, 0, false, "dispensingUnitTextBox" );
                _dispensingUnitParameter.DefaultValue = dispensingUnit.Trim();
                
                _subItemDataSource.Update();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.EditIndex = -1; // done with edit
            //         SetDrugItemsGridViewSelectedItem( rowIndex );
            gv.DataBind();

            if( Session[ "LastUpdatedDrugItemSubItemId" ] != null )
            {
                int updatedSubItemId = ( int )Session[ "LastUpdatedDrugItemSubItemId" ];
                updatedRowIndex = gv.GetRowIndexFromId( updatedSubItemId, 0 );

                SetSubItemGridViewSelectedItem( updatedRowIndex );

                // bind to select
                gv.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledSubItemGridControlsDuringEdit( gv, updatedRowIndex, true );
            }

            return ( updatedRowIndex );
        }



        private int InsertSubItem( GridView gv, int rowIndex )
        {
            int insertedRowIndex = 0;

            string subItemIdentifier = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "subItemIdentifierTextBox" );
            _subItemIdentifierParameter.DefaultValue = subItemIdentifier.Trim();
            string generic = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "genericNameTextBox" );
            _genericParameter.DefaultValue = generic.Trim();
            string tradeName = gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "tradeNameTextBox" );
            _tradeNameParameter.DefaultValue = tradeName.Trim();
            string packageDescription = gv.GetStringValueFromSelectedControl( rowIndex, 4, 0, false, "packageDescriptionTextBox" );
            _packageDescriptionParameter.DefaultValue = packageDescription.Trim();
            string dispensingUnit = gv.GetStringValueFromSelectedControl( rowIndex, 5, 0, false, "dispensingUnitTextBox" );
            _dispensingUnitParameter.DefaultValue = dispensingUnit.Trim();

            try
            {
                _subItemDataSource.Insert();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.InsertRowActive = false; // done with insert
            gv.EditIndex = -1; // done with edit of new row
            _withAddSubItemParameter.DefaultValue = "false"; // no extra row

            // bind with new row
            gv.DataBind();

            if( Session[ "LastInsertedDrugItemSubItemId" ] != null )
            {
                int newSubItemId = ( int )Session[ "LastInsertedDrugItemSubItemId" ];
                insertedRowIndex = gv.GetRowIndexFromId( newSubItemId, 0 );

                SetSubItemGridViewSelectedItem( insertedRowIndex );

                // bind to select
                gv.DataBind();
            }

            return ( insertedRowIndex );
        }

        protected void _subItemDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemSubItemIdString = e.Command.Parameters[ "@DrugItemSubItemId" ].Value.ToString();

            if( drugItemSubItemIdString.Length > 0 )
            {
                int drugItemSubItemId = int.Parse( drugItemSubItemIdString );
                Session[ "LastInsertedDrugItemSubItemId" ] = drugItemSubItemId;
            }
        }

        // probably wont happen - id changing during update
        protected void _subItemDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemSubItemIdString = e.Command.Parameters[ "@DrugItemSubItemId" ].Value.ToString();

            if( drugItemSubItemIdString.Length > 0 )
            {
                int drugItemSubItemId = int.Parse( drugItemSubItemIdString );
                Session[ "LastUpdatedDrugItemSubItemId" ] = drugItemSubItemId;
            }
        }

        private int DeleteSubItem( GridView gv, int rowIndex, int selectedDrugItemSubItemId )
        {
            // id of row to delete
            _drugItemSubItemIdParameter.DefaultValue = selectedDrugItemSubItemId.ToString();
            _subItemIdentifierParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "subItemIdentifierLabel" );

            try
            {
                _subItemDataSource.Delete();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            // previous row gets focus
            if( rowIndex >= 1 )
                rowIndex--;

            SetSubItemGridViewSelectedItem( rowIndex );

            gv.DataBind();

            return ( rowIndex );
        }

        protected void RemoveSubItemButton_DataBinding( object sender, EventArgs e )
        {
            Button removeSubItemButton = ( Button )sender;
            if( removeSubItemButton != null )
                MultiLineButtonText( removeSubItemButton, new string[] { "Remove", "Sub-Item" } );
        }

        private void MultiLineButtonText( Button button, string[] buttonTextArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < buttonTextArray.Count(); i++ )
            {
                sb.AppendLine( buttonTextArray[ i ] );
            }

            button.Text = sb.ToString();
        }

#endregion SubItemGrid


    }
}
