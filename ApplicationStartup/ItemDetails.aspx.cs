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
using VA.NAC.NACCMBrowser.DBInterface;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using TextBox = System.Web.UI.WebControls.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class ItemDetails : System.Web.UI.Page
    {

        //ManufacturersCatalogNumber
        //ManufacturersName
        //ManufacturersCommercialListPrice
        //LetterOfCommitmentDate
        //CommercialListPrice
        //CommercialPricelistDate
        //CommercialPricelistFOBTerms
        //TrackingMechanism
        //AcquisitionCost
        //TypeOfContractor
        //CountryOfOrigin - list
        
        private ItemDetailsWindowParms _itemDetailsWindowParms = null;

        private DocumentDataSource _itemDetailsDataSource = null;

        private DataSet _countriesOfOriginDataSet = null;
       // private int[] _selectedCountries = new int[ 250 ];

        private Parameter _modificationStatusIdParameter = null;

        // select parameters
        private Parameter _contractNumberParameter = null;
        private Parameter _contractIdParameter = null;
        private Parameter _itemIdParameter = null;

        // update parameters
        private Parameter _manufacturersCatalogNumberParameter = null;
        private Parameter _manufacturersNameParameter = null;
        private Parameter _manufacturersCommercialListPriceParameter = null;
        private Parameter _letterOfCommitmentDateParameter = null;
        private Parameter _commercialListPriceParameter = null;
        private Parameter _commercialPricelistDateParameter = null;
        private Parameter _commercialPricelistFOBTermsParameter = null;

        private Parameter _trackingMechanismParameter = null;
        private Parameter _acquisitionCostParameter = null;
        private Parameter _typeOfContractorParameter = null;

        public DataSet CountriesOfOriginDataSet         
        { 
            get
            {
                return ( _countriesOfOriginDataSet );
            }
            set
            {
                _countriesOfOriginDataSet = value;
            }        
        }

        private ValidationErrorManager _validationErrorManager = null;

        protected void Page_Load( object sender, EventArgs e )
        {           
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }
            
            if( Session[ "ItemDetailsWindowParms" ] != null )
            {
                _itemDetailsWindowParms = ( ItemDetailsWindowParms )Session[ "ItemDetailsWindowParms" ];
            }
            else
            {
                throw new Exception( "Error: ItemId not available for details presentation." );
            }

            InitSharedParms();
            
            LoadItemDetails();

            LoadItemCountriesOfOrigin( _itemDetailsWindowParms.SelectedItemId );

            if( Page.IsPostBack == false )
            {
                BindItemDetails();
                BindCountryOfOriginCheckBoxList();
            }

            AddClientCloseEvent();

            _validationErrorManager = new ValidationErrorManager( DocumentTypes.Contract );
        }

        private void DisableItemDetailsControlsForReadOnly()
        {
            if( Session[ "CurrentDocument" ] != null )
            {
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                // items may not be editable at all if, for example, contract was expired or item is discontinued
                // also, the values for all of the detail fields are inherited from the parent FSS item
                if ( _itemDetailsWindowParms.IsEditable == false || _itemDetailsWindowParms.IsBPA == true )
                {
                    DisableAllControlsForReadOnly();
                }               
            }
        }

        private void DisableAllControlsForReadOnly()
        {
            Button updateItemDetailsButton = null;
            updateItemDetailsButton = ( Button )ItemDetailsForm.FindControl( "UpdateItemDetailsButton" );
            if( updateItemDetailsButton != null )
            {
                updateItemDetailsButton.Enabled = false;
            }

            DisableTextBox( "ManufacturersCatalogNumberTextBox" );
            DisableTextBox( "ManufacturersNameTextBox" );
            DisableTextBox( "ManufacturersCommercialListPriceTextBox" );
            DisableTextBox( "LetterOfCommitmentDateTextBox" );
            DisableTextBox( "TrackingMechanismTextBox" );
            
            DisableTextBox( "AcquisitionCostTextBox" );
            DisableTextBox( "CommercialListPriceTextBox" );
            DisableTextBox( "CommercialPricelistDateTextBox" );
            DisableTextBox( "CommercialPricelistFOBTermsTextBox" );
            DisableTextBox( "TypeOfContractorTextBox" );

            CheckBoxList countriesOfOriginCheckBoxList = ( CheckBoxList )ItemDetailsFormView.FindControl( "CountriesOfOriginCheckBoxList" );
            if( countriesOfOriginCheckBoxList != null )
                countriesOfOriginCheckBoxList.Enabled = false;            
        }



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
        //            excludeFromExportCheckBox = ( CheckBox )ItemDetailsFormView.FindControl( "ExcludeFromExportCheckBox" );
        //            if( excludeFromExportCheckBox != null )
        //                excludeFromExportCheckBox.Enabled = false;

        //            CheckBox primeVendorFlagCheckBox = null;
        //            primeVendorFlagCheckBox = ( CheckBox )ItemDetailsFormView.FindControl( "PrimeVendorFlagCheckBox" );
        //            if( primeVendorFlagCheckBox != null )
        //                primeVendorFlagCheckBox.Enabled = false;

        //            DropDownList unitOfSaleDropDownList = null;
        //            unitOfSaleDropDownList = ( DropDownList )ItemDetailsFormView.FindControl( "UnitOfSaleDropDownList" );
        //            if( unitOfSaleDropDownList != null )
        //                unitOfSaleDropDownList.Enabled = false;

        //            DisableTextBox( "QuantityInUnitOfSaleTextBox" );

        //            DropDownList unitPackageDropDownList = null;
        //            unitPackageDropDownList = ( DropDownList )ItemDetailsFormView.FindControl( "UnitPackageDropDownList" );
        //            if( unitPackageDropDownList != null )
        //                unitPackageDropDownList.Enabled = false;

        //            DisableTextBox( "QuantityInUnitPackageTextBox" );

        //            DropDownList unitOfMeasureDropDownList = null;
        //            unitOfMeasureDropDownList = ( DropDownList )ItemDetailsFormView.FindControl( "UnitOfMeasureDropDownList" );
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
            tb = ( System.Web.UI.WebControls.TextBox )ItemDetailsFormView.FindControl( textBoxName );

            if( tb != null )
            {
                tb.Enabled = false;
            }
        }

        private void ClearSessionVariables()
        {
            Session[ "ItemDetailsDataSource" ] = null;
            Session[ "LastCountryOfOriginForSelectedItemDetails" ] = null;
            Session[ "CountriesOfOriginDataSource" ] = null; 
        }

        protected void ItemDetailsFormView_OnPreRender( object sender, EventArgs e )
        {            
            DisableItemDetailsControlsForReadOnly();            
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

        protected void ItemDetailsScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "ItemDetailsErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "ItemDetailsErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            ItemDetailsScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }
        
        private void SetItemDetailsHeaderInfo()
        {
            DataRowView currentRow = ( DataRowView )ItemDetailsFormView.DataItem;

            SelectedItemHeader.HeaderTitle = "Item Details";
            SelectedItemHeader.ItemDescription = currentRow[ "ItemDescription" ].ToString();
            SelectedItemHeader.CatalogNumberTitle = "Part Number";
            SelectedItemHeader.CatalogNumber = currentRow[ "CatalogNumber" ].ToString();
        }

        private void LoadItemDetails()
        {
            if( Session[ "ItemDetailsDataSource" ] == null )
            {
                _itemDetailsDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.Item, true );
                _itemDetailsDataSource.ID = "ItemDetailsDataSource";
                _itemDetailsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _itemDetailsDataSource.SelectCommand = "GetMedSurgItemDetails";
                _itemDetailsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                _itemDetailsDataSource.UpdateCommand = "UpdateMedSurgItemDetails";
                _itemDetailsDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _itemDetailsDataSource.SetEventOwnerName( "ItemDetails" );

                SetUpItemDetailsParameters();

                _contractNumberParameter.DefaultValue = _itemDetailsWindowParms.ContractNumber;
                _contractIdParameter.DefaultValue = _itemDetailsWindowParms.ContractId.ToString();
                _itemIdParameter.DefaultValue = _itemDetailsWindowParms.SelectedItemId.ToString();

                AddParametersToDataSource( _itemDetailsDataSource );

            }
            else
            {
                _itemDetailsDataSource = ( DocumentDataSource )Session[ "ItemDetailsDataSource" ];
                _itemDetailsDataSource.RestoreDelegatesAfterDeserialization( this, "ItemDetails" );
                ReloadItemDetailsParameters( _itemDetailsDataSource );
            }

            ItemDetailsFormView.DataSource = _itemDetailsDataSource;
            ItemDetailsFormView.DataKeyNames = new string[] { "ItemId" };
        }

        private void BindItemDetails()
        {
            ItemDetailsFormView.DataBind();
        }



        private void BindCountryOfOriginCheckBoxList()
        {
            CheckBoxList countriesOfOriginCheckBoxList = ( CheckBoxList )ItemDetailsFormView.FindControl( "CountriesOfOriginCheckBoxList" );          
            countriesOfOriginCheckBoxList.DataBind();
        }

        protected void ItemDetailsFormView_OnDataBound( Object sender, EventArgs e )
        {
            SetItemDetailsHeaderInfo();

            SetScreenDefaults();

            ItemDetailsUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        
        private void SetScreenDefaults()
        {
            DataRowView currentRow = ( DataRowView )ItemDetailsFormView.DataItem;
                      
        }

       

        private void SetUpItemDetailsParameters()
        {
            // select and update
            _contractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _contractIdParameter = new Parameter( "ContractId", TypeCode.Int32 );
            _itemIdParameter = new Parameter( "ItemId", TypeCode.Int32 );

            // update     
            _manufacturersCatalogNumberParameter = new Parameter( "ManufacturersCatalogNumber", TypeCode.String );
            _manufacturersNameParameter = new Parameter( "ManufacturersName", TypeCode.String );
            _manufacturersCommercialListPriceParameter = new Parameter( "ManufacturersCommercialListPrice", TypeCode.Double );
            _letterOfCommitmentDateParameter = new Parameter( "LetterOfCommitmentDate", TypeCode.DateTime );
            _commercialListPriceParameter = new Parameter( "CommercialListPrice", TypeCode.Double );
            _commercialPricelistDateParameter = new Parameter( "CommercialPricelistDate", TypeCode.DateTime );
            _commercialPricelistFOBTermsParameter = new Parameter( "CommercialPricelistFOBTerms", TypeCode.String );
            _trackingMechanismParameter = new Parameter( "TrackingMechanism", TypeCode.String );
            _acquisitionCostParameter = new Parameter( "AcquisitionCost", TypeCode.Double );
            _typeOfContractorParameter = new Parameter( "TypeOfContractor", TypeCode.String );

        }

        


        private void AddParametersToDataSource( DocumentDataSource itemDetailsDataSource )
        {
            itemDetailsDataSource.SelectParameters.Add( _contractNumberParameter );
            itemDetailsDataSource.SelectParameters.Add( _contractIdParameter );
            itemDetailsDataSource.SelectParameters.Add( _itemIdParameter );

            itemDetailsDataSource.UpdateParameters.Add( _contractNumberParameter );
            itemDetailsDataSource.UpdateParameters.Add( _contractIdParameter );
            itemDetailsDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
            itemDetailsDataSource.UpdateParameters.Add( _itemIdParameter );

            itemDetailsDataSource.UpdateParameters.Add( _manufacturersCatalogNumberParameter );
            itemDetailsDataSource.UpdateParameters.Add( _manufacturersNameParameter );
            itemDetailsDataSource.UpdateParameters.Add( _manufacturersCommercialListPriceParameter );
            itemDetailsDataSource.UpdateParameters.Add( _letterOfCommitmentDateParameter );
            itemDetailsDataSource.UpdateParameters.Add( _commercialListPriceParameter );
            itemDetailsDataSource.UpdateParameters.Add( _commercialPricelistDateParameter );
            itemDetailsDataSource.UpdateParameters.Add( _commercialPricelistFOBTermsParameter );
            itemDetailsDataSource.UpdateParameters.Add( _trackingMechanismParameter );
            itemDetailsDataSource.UpdateParameters.Add( _acquisitionCostParameter );
            itemDetailsDataSource.UpdateParameters.Add( _typeOfContractorParameter );
        }

        private void ReloadItemDetailsParameters( DocumentDataSource itemDetailsDataSource )
        {
            _contractNumberParameter = itemDetailsDataSource.SelectParameters[ "ContractNumber" ];
            _contractIdParameter = itemDetailsDataSource.SelectParameters[ "ContractId" ];
            _itemIdParameter = itemDetailsDataSource.SelectParameters[ "ItemId" ];

            // update     
            _manufacturersCatalogNumberParameter = itemDetailsDataSource.UpdateParameters[ "ManufacturersCatalogNumber" ];
            _manufacturersNameParameter = itemDetailsDataSource.UpdateParameters[ "ManufacturersName" ];
            _manufacturersCommercialListPriceParameter = itemDetailsDataSource.UpdateParameters[ "ManufacturersCommercialListPrice" ];
            _letterOfCommitmentDateParameter = itemDetailsDataSource.UpdateParameters[ "LetterOfCommitmentDate" ];
            _commercialListPriceParameter = itemDetailsDataSource.UpdateParameters[ "CommercialListPrice" ];
            _commercialPricelistDateParameter = itemDetailsDataSource.UpdateParameters[ "CommercialPricelistDate" ];
            _commercialPricelistFOBTermsParameter = itemDetailsDataSource.UpdateParameters[ "CommercialPricelistFOBTerms" ];
            _trackingMechanismParameter = itemDetailsDataSource.UpdateParameters[ "TrackingMechanism" ];
            _acquisitionCostParameter = itemDetailsDataSource.UpdateParameters[ "AcquisitionCost" ];
            _typeOfContractorParameter = itemDetailsDataSource.UpdateParameters[ "TypeOfContractor" ];
        }
          
        protected void UpdateItemDetailsButton_OnClick( object sender, EventArgs e )
        {
            string validationMessage = "";

            try
            {
                if( ValidateItemDetails( ref validationMessage ) == true )
                {
                    GetUpdateParameterValues();
                    _itemDetailsDataSource.Update();

                    SaveCountriesOfOrigin( _itemDetailsWindowParms.SelectedItemId );
                    LoadItemCountriesOfOrigin( _itemDetailsWindowParms.SelectedItemId );

                    ItemDetailsFormView.DataBind();                   
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
          
            string manufacturersCatalogNumber = "";
            string manufacturersName = "";
            string manufacturersCommercialListPrice = "";
            string letterOfCommitmentDate = "";
            string commercialListPrice = "";
            string commercialPricelistDate = "";
            string commercialPricelistFOBTerms = "";
            string trackingMechanism = "";
            string acquisitionCost = "";
            string typeOfContractor = "";

            try
            {
                FormView itemDetailsFormView = ( FormView )ItemDetailsForm.FindControl( "ItemDetailsFormView" );
                if( itemDetailsFormView != null )
                {
                    if( itemDetailsFormView.CurrentMode == FormViewMode.Edit )
                    {
                        TextBox manufacturersCatalogNumberTextBox = ( TextBox )itemDetailsFormView.FindControl( "ManufacturersCatalogNumberTextBox" );
                        if( manufacturersCatalogNumberTextBox != null )
                            manufacturersCatalogNumber = manufacturersCatalogNumberTextBox.Text;

                        TextBox ManufacturersNameTextBox = ( TextBox )itemDetailsFormView.FindControl( "ManufacturersNameTextBox" );
                        if( ManufacturersNameTextBox != null )
                            manufacturersName = ManufacturersNameTextBox.Text;

                        TextBox ManufacturersCommercialListPriceTextBox = ( TextBox )itemDetailsFormView.FindControl( "ManufacturersCommercialListPriceTextBox" );
                        if( ManufacturersCommercialListPriceTextBox != null )
                            manufacturersCommercialListPrice = ManufacturersCommercialListPriceTextBox.Text;

                        TextBox LetterOfCommitmentDateTextBox = ( TextBox )itemDetailsFormView.FindControl( "LetterOfCommitmentDateTextBox" );
                        if( LetterOfCommitmentDateTextBox != null )
                            letterOfCommitmentDate = LetterOfCommitmentDateTextBox.Text;

                        TextBox CommercialListPriceTextBox = ( TextBox )itemDetailsFormView.FindControl( "CommercialListPriceTextBox" );
                        if( CommercialListPriceTextBox != null )
                            commercialListPrice = CommercialListPriceTextBox.Text;

                        TextBox CommercialPricelistDate = ( TextBox )ItemDetailsFormView.FindControl( "CommercialPricelistDateTextBox" );
                        if( CommercialPricelistDate != null )
                            commercialPricelistDate = CommercialPricelistDate.Text;

                        TextBox CommercialPricelistFOBTermsTextBox = ( TextBox )ItemDetailsFormView.FindControl( "CommercialPricelistFOBTermsTextBox" );
                        if( CommercialPricelistFOBTermsTextBox != null )
                            commercialPricelistFOBTerms = CommercialPricelistFOBTermsTextBox.Text;

                        TextBox TrackingMechanismTextBox = ( TextBox )ItemDetailsFormView.FindControl( "TrackingMechanismTextBox" );
                        if( TrackingMechanismTextBox != null )
                            trackingMechanism = TrackingMechanismTextBox.Text;

                        TextBox AcquisitionCostTextBox = ( TextBox )ItemDetailsFormView.FindControl( "AcquisitionCostTextBox" );
                        if( AcquisitionCostTextBox != null )
                            acquisitionCost = AcquisitionCostTextBox.Text;

                        TextBox TypeOfContractorTextBox = ( TextBox )ItemDetailsFormView.FindControl( "TypeOfContractorTextBox" );
                        if( TypeOfContractorTextBox != null )
                            typeOfContractor = TypeOfContractorTextBox.Text;

                        // holding mandatory fields for a future release
                        if( manufacturersCatalogNumber.Length == 0 )
                        {                           
                            if( _itemDetailsWindowParms.IsService == false )
                            {
                        //        _validationErrorManager.AppendValidationError( "Manufacturers Part Number is required.", false );
                            }                           
                        }

                        if( manufacturersName.Length == 0 )
                        {
                            if( _itemDetailsWindowParms.IsService == false )
                            {
                      //          _validationErrorManager.AppendValidationError( "Manufacturers Name is required.", false );
                            }
                        }

                        decimal parseDecimal;

                        if( manufacturersCommercialListPrice.Length > 0 )
                        {
                            if( Decimal.TryParse( manufacturersCommercialListPrice, out parseDecimal ) == false )
                            {
                                _validationErrorManager.AppendValidationError( "Manufacturers Commercial List Price is not a valid number.", false );
                            }
                        }
                        
                        DateTime parseDate;

                        if( letterOfCommitmentDate.Length > 0 )
                        {
                            if( DateTime.TryParse( letterOfCommitmentDate, out parseDate ) == false )
                            {
                                _validationErrorManager.AppendValidationError( "Letter Of Commitment Date is not a valid date.", false );
                            }
                        }

                        if( commercialListPrice.Length > 0 )
                        {
                            if( Decimal.TryParse( commercialListPrice, out parseDecimal ) == false )
                            {
                                _validationErrorManager.AppendValidationError( "Commercial List Price is not a valid number.", false );
                            }
                        }
                        else
                        {
                            if( _itemDetailsWindowParms.IsService == false )
                            {
                    //            _validationErrorManager.AppendValidationError( "Commercial List Price is required.", false );
                            }
                        }


                        if( commercialPricelistDate.Length > 0 )
                        {
                            if( DateTime.TryParse( commercialPricelistDate, out parseDate ) == false )
                            {
                                _validationErrorManager.AppendValidationError( "Commercial Pricelist Date is not a valid date.", false );
                            }
                        }
                        else
                        {
                            if( _itemDetailsWindowParms.IsService == false )
                            {
                      //          _validationErrorManager.AppendValidationError( "Commercial Pricelist Date is required.", false );
                            }
                        }

                        if( commercialPricelistFOBTerms.Length == 0 )
                        {
                            if( _itemDetailsWindowParms.IsService == false )
                            {
                      //          _validationErrorManager.AppendValidationError( "Commercial Pricelist FOB Terms is required.", false );
                            }
                        }

                        if( trackingMechanism.Length == 0 )
                        {
                            if( _itemDetailsWindowParms.IsService == false )
                            {
                       //         _validationErrorManager.AppendValidationError( "Tracking Mechanism is required.", false );
                            }
                        }

                        if( acquisitionCost.Length > 0 )
                        {
                            if( Decimal.TryParse( acquisitionCost, out parseDecimal ) == false )
                            {
                                _validationErrorManager.AppendValidationError( "Acquisition Cost is not a valid number.", false );
                            }
                        }
                        else
                        {
                            if( _itemDetailsWindowParms.IsService == false )
                            {
                      //          _validationErrorManager.AppendValidationError( "Acquisition Cost is required.", false );
                            }
                        }

                        if( typeOfContractor.Length == 0 )
                        {
                            if( _itemDetailsWindowParms.IsService == false )
                            {
                       //         _validationErrorManager.AppendValidationError( "Type Of Contractor is required.", false );
                            }
                        }


                        if( ValidateCountriesOfOrigin() == false )
                        {
                      //      _validationErrorManager.AppendValidationError( "At least one country of origin must be selected.", false );
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                bSuccess = false;
                validationMessage = string.Format( "The following exception was encountered validating the item details: {0}", ex.Message );
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
            FormView itemDetailsFormView = ( FormView )ItemDetailsForm.FindControl( "ItemDetailsFormView" );
            if( itemDetailsFormView != null )
            {
                if( itemDetailsFormView.CurrentMode == FormViewMode.Edit )
                {

                    TextBox manufacturersCatalogNumberTextBox = ( TextBox )itemDetailsFormView.FindControl( "ManufacturersCatalogNumberTextBox" );
                    if( manufacturersCatalogNumberTextBox != null )
                        _manufacturersCatalogNumberParameter.DefaultValue = manufacturersCatalogNumberTextBox.Text;

                    TextBox ManufacturersNameTextBox = ( TextBox )itemDetailsFormView.FindControl( "ManufacturersNameTextBox" );
                    if( ManufacturersNameTextBox != null )
                        _manufacturersNameParameter.DefaultValue = ManufacturersNameTextBox.Text;

                    TextBox ManufacturersCommercialListPriceTextBox = ( TextBox )itemDetailsFormView.FindControl( "ManufacturersCommercialListPriceTextBox" );
                    if( ManufacturersCommercialListPriceTextBox != null )
                        _manufacturersCommercialListPriceParameter.DefaultValue = ManufacturersCommercialListPriceTextBox.Text;

                    TextBox LetterOfCommitmentDateTextBox = ( TextBox )itemDetailsFormView.FindControl( "LetterOfCommitmentDateTextBox" );
                    if( LetterOfCommitmentDateTextBox != null )
                        _letterOfCommitmentDateParameter.DefaultValue = LetterOfCommitmentDateTextBox.Text;

                    TextBox CommercialListPriceTextBox = ( TextBox )itemDetailsFormView.FindControl( "CommercialListPriceTextBox" );
                    if( CommercialListPriceTextBox != null )
                        _commercialListPriceParameter.DefaultValue = CommercialListPriceTextBox.Text;

                    TextBox CommercialPricelistDateTextBox = ( TextBox )ItemDetailsFormView.FindControl( "CommercialPricelistDateTextBox" );
                    if( CommercialPricelistDateTextBox != null )
                        _commercialPricelistDateParameter.DefaultValue = CommercialPricelistDateTextBox.Text;

                    TextBox CommercialPricelistFOBTermsTextBox = ( TextBox )ItemDetailsFormView.FindControl( "CommercialPricelistFOBTermsTextBox" );
                    if( CommercialPricelistFOBTermsTextBox != null )
                        _commercialPricelistFOBTermsParameter.DefaultValue = CommercialPricelistFOBTermsTextBox.Text;

                    TextBox TrackingMechanismTextBox = ( TextBox )ItemDetailsFormView.FindControl( "TrackingMechanismTextBox" );
                    if( TrackingMechanismTextBox != null )
                        _trackingMechanismParameter.DefaultValue = TrackingMechanismTextBox.Text;

                    TextBox AcquisitionCostTextBox = ( TextBox )ItemDetailsFormView.FindControl( "AcquisitionCostTextBox" );
                    if( AcquisitionCostTextBox != null )
                        _acquisitionCostParameter.DefaultValue = AcquisitionCostTextBox.Text;

                    TextBox TypeOfContractorTextBox = ( TextBox )ItemDetailsFormView.FindControl( "TypeOfContractorTextBox" );
                    if( TypeOfContractorTextBox != null )
                        _typeOfContractorParameter.DefaultValue = TypeOfContractorTextBox.Text;
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
          
        // select country of origin from drop down during edit
        //protected void CountryOfOriginDropDownList_OnDataBound( object sender, EventArgs e )
        //{
        //    DropDownList countryOfOriginDropDownList = ( DropDownList )sender;
        //    FormView containingFormView = ( FormView )countryOfOriginDropDownList.NamingContainer;

        //    DataRowView currentRow = ( DataRowView )containingFormView.DataItem;

        //    string selectedCountryOfOriginString = currentRow[ "CountryOfOrigin" ].ToString();
        //    int selectedCountryOfOrigin;

        //    if( selectedCountryOfOriginString != null )
        //    {
        //        if( int.TryParse( selectedCountryOfOriginString, out selectedCountryOfOrigin ) )
        //        {
        //            ListItem listItem = countryOfOriginDropDownList.Items.FindByValue( selectedCountryOfOrigin.ToString() );
        //            if( listItem != null )
        //            {
        //                listItem.Selected = true;
        //            }
        //        }
        //    }
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

        private void LoadItemCountriesOfOrigin( int itemId )
        {
            bool bSuccess = true;

            ItemDB itemDB = ( ItemDB )Session[ "ItemDB" ];
            DataSet dsItemCountries = null;

            try
            {
                itemDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                itemDB.MakeConnectionString();

                bSuccess = itemDB.GetMedSurgItemCountriesOfOrigin( ref dsItemCountries, itemId );

                if( bSuccess == true && itemDB.RowsReturned > 0 )
                {
                    _countriesOfOriginDataSet = dsItemCountries;


                }
            }
            catch( Exception ex )
            {
                throw new Exception( ex.Message );
            }

        }

        protected void CountriesOfOriginCheckBoxList_OnDataBound( object sender, EventArgs e )
        {
            CheckBoxList countriesOfOriginCheckBoxList = ( CheckBoxList )sender;
           
            if( countriesOfOriginCheckBoxList != null )
            {
                countriesOfOriginCheckBoxList.ClearSelection();
                countriesOfOriginCheckBoxList.Items.Clear();

                foreach( DataRow row in _countriesOfOriginDataSet.Tables[ ItemDB.MedSurgItemCountriesOfOriginTableName ].Rows )
                {
                    int countryId = int.Parse( row[ "CountryId" ].ToString() );
                    bool bIsSelected = bool.Parse( row[ "IsSelected" ].ToString() );
                    string countryName = row[ "CountryName" ].ToString();

                    ListItem listItem = new ListItem( countryName, countryId.ToString() );
                    listItem.Selected = bIsSelected;

                    countriesOfOriginCheckBoxList.Items.Add( listItem );
                }               
            }
        }

        // ensure that at least one is selected
        protected bool ValidateCountriesOfOrigin()
        {
            bool bSuccess = false;

            CheckBoxList countriesOfOriginCheckBoxList = ( CheckBoxList )ItemDetailsFormView.FindControl( "CountriesOfOriginCheckBoxList" );

            for( int i = 0; i < countriesOfOriginCheckBoxList.Items.Count; i++ )
            {
                bool bSelected = countriesOfOriginCheckBoxList.Items[ i ].Selected;

                if( bSelected == true )
                {
                    bSuccess = true;
                    break;
                }
            }
            
            return ( bSuccess );
        }

        protected void SaveCountriesOfOrigin( int itemId )
        {
            CheckBoxList countriesOfOriginCheckBoxList = ( CheckBoxList )ItemDetailsFormView.FindControl( "CountriesOfOriginCheckBoxList" );

            bool bSuccess = true;
            bool bSelected = false;
            int countryId = -1;
            int itemCountryId = -1;

            ItemDB itemDB = ( ItemDB )Session[ "ItemDB" ];
            itemDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            itemDB.MakeConnectionString();
           
            if( countriesOfOriginCheckBoxList != null )
            {
                for( int i = 0; i < countriesOfOriginCheckBoxList.Items.Count; i++ )
                {
                    bSelected = countriesOfOriginCheckBoxList.Items[ i ].Selected;
                    countryId = int.Parse( countriesOfOriginCheckBoxList.Items[ i ].Value.ToString()); 

                    {
                        try
                        {
                            if( bSelected == true )
                            {
                                bSuccess = itemDB.InsertCountryOfOriginForItem( itemId, countryId, ref itemCountryId );
                            }
                            else
                            {
                                bSuccess = itemDB.DeleteCountryOfOriginForItem( itemId, countryId );
                            }

                            if( bSuccess == false )
                            {
                                throw new Exception( itemDB.ErrorMessage );
                            }
                        }
                        catch( Exception ex )
                        {
                            throw new Exception( ex.Message );
                        }
                    }                   
                }
            }
        }

        protected void CountriesOfOriginCheckBoxList_OnPreRender( object sender, EventArgs e )
        {
            CheckBoxList countriesOfOriginCheckBoxList = ( CheckBoxList )sender;
        }

        protected void CountriesOfOriginCheckBoxList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            //CheckBoxList countriesOfOriginCheckBoxList = ( CheckBoxList )sender;
            //ListItem selectedItem = countriesOfOriginCheckBoxList.SelectedItem;  // this is not the recently selected item.  will have to maintain a sep list and each time scan all to det which was selected or cleared.

            //int selectedCountryId = int.Parse( selectedItem.Value );  

            //// update the selected list


            // allow the postback to occur 
            ItemDetailsUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

        }
    }
}
