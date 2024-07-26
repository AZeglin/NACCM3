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
using VA.NAC.NACCMBrowser.DBInterface;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using TextBox = System.Web.UI.WebControls.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.DrugItems
{
    public partial class ItemCopy : System.Web.UI.Page
    {
        private ItemCopyWindowParms _itemCopyWindowParms = null;

        private SerializableObjectDataSource _itemCopyDataSource = null;

        private DocumentDataSource _UnitOfSaleDataSource = null;
        private DocumentDataSource _UnitPackageDataSource = null;
        private DocumentDataSource _UnitOfMeasureDataSource = null;

        ItemCopyContent _itemCopyContent = null;

        CurrentDocument _destinationDocumentUsedInCopyItem = null;

        protected void Page_Load( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }

            if( _itemCopyWindowParms != null )
            {
                InitObjectDataSource();
                LoadSalePackagingMeasureUnits();

                if( Page.IsPostBack == false )
                {
                    BindItemCopy();
                    BindItemPackagingLists();
                }
            }

            RetrieveLastUsedDetinationContract();

            AddClientCloseEvent();
       }

        protected void ItemCopyForm_OnInit( object sender, EventArgs e )
        {
            if( Session[ "ItemCopyWindowParms" ] != null )
            {
                _itemCopyWindowParms = ( ItemCopyWindowParms )Session[ "ItemCopyWindowParms" ];
            }
            else
            {
                throw new Exception( "Error: ItemCopyWindowParms not available for change window presentation." );
            }

            if( _itemCopyWindowParms.CopyType == ItemCopyWindowParms.CopyTypes.CopyLocal )
            {
                ItemCopyMultiView.ActiveViewIndex = 0;
            }
            else
            {
                ItemCopyMultiView.ActiveViewIndex = 1; // copy to another contract
            }
        }

        private void BindItemPackagingLists()
        {
            if( _itemCopyWindowParms.CopyType == ItemCopyWindowParms.CopyTypes.CopyLocal )
            {
                DropDownList UnitOfSaleDropDownList = ( DropDownList )ItemCopyToSameContractFormView.FindControl( "UnitOfSaleDropDownList" );
                UnitOfSaleDropDownList.DataSource = _UnitOfSaleDataSource;
                UnitOfSaleDropDownList.DataTextField = "SelectableUnit";
                UnitOfSaleDropDownList.DataValueField = "SelectableUnit";
                UnitOfSaleDropDownList.DataBind();

                DropDownList UnitPackageDropDownList = ( DropDownList )ItemCopyToSameContractFormView.FindControl( "UnitPackageDropDownList" );
                UnitPackageDropDownList.DataSource = _UnitPackageDataSource;
                UnitPackageDropDownList.DataTextField = "SelectableUnit";
                UnitPackageDropDownList.DataValueField = "SelectableUnit";
                UnitPackageDropDownList.DataBind();

                DropDownList UnitOfMeasureDropDownList = ( DropDownList )ItemCopyToSameContractFormView.FindControl( "UnitOfMeasureDropDownList" );
                UnitOfMeasureDropDownList.DataSource = _UnitOfMeasureDataSource;
                UnitOfMeasureDropDownList.DataTextField = "SelectableUnit";
                UnitOfMeasureDropDownList.DataValueField = "SelectableUnit";
                UnitOfMeasureDropDownList.DataBind();
            }
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

        private void RetrieveLastUsedDetinationContract()
        {
            if( Session[ "DestinationDocumentUsedInCopyItem" ] != null )
            {
                _destinationDocumentUsedInCopyItem = ( CurrentDocument )Session[ "DestinationDocumentUsedInCopyItem" ];
            }
        }

        private bool CreateLastUsedDetinationContract( string contractNumber, ref string destinationValidationMessage )
        {
            bool bSuccess = true;

            _destinationDocumentUsedInCopyItem = new CurrentDocument();

            _destinationDocumentUsedInCopyItem.ContractNumber = contractNumber;

            if( Session[ "ContractDB" ] != null )
            {
                _destinationDocumentUsedInCopyItem.ContractDatabase = ( ContractDB )Session[ "ContractDB" ];

                try
                {
                    bSuccess = _destinationDocumentUsedInCopyItem.LookupCurrentDocument();
                    if( bSuccess == false )
                    {
                        destinationValidationMessage = _destinationDocumentUsedInCopyItem.GetLastContractDBError();
                    }
                }
                catch( Exception ex )
                {
                    bSuccess = false;
                    destinationValidationMessage = string.Format( "Error looking up destination contract: {0}", ex.Message );
                }
            }

            // set security on destination
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            bs.SetDocumentEditStatus( _destinationDocumentUsedInCopyItem );

            // save destination for future copies
            Session[ "DestinationDocumentUsedInCopyItem" ] = _destinationDocumentUsedInCopyItem;

            return ( bSuccess );
        }

        private void ClearSessionVariables()
        {
            Session[ "ItemCopyContent" ] = null;
            Session[ "ItemCopyDataSource" ] = null;
            Session[ "ItemCopyCurrentRowOffset" ] = null;
            Session[ "LastPackageIdForSelectedItemDetails" ] = null;
            Session[ "UnitOfSaleDataSource" ] = null;
            Session[ "UnitPackageDataSource" ] = null;
            Session[ "UnitOfMeasureDataSource" ] = null;
        }

        private void InitObjectDataSource()
        {
            InitItemCopyContent();

            if( Session[ "ItemCopyDataSource" ] == null )
            {
                _itemCopyDataSource = new SerializableObjectDataSource();
                _itemCopyDataSource.ID = "ItemCopyDataSource";
                _itemCopyDataSource.DataObjectTypeName = "VA.NAC.NACCMBrowser.BrowserObj.ItemCopyContent";
                _itemCopyDataSource.TypeName = "VA.NAC.NACCMBrowser.BrowserObj.ItemCopyManager";
                _itemCopyDataSource.SelectMethod = "GetItemCopyContent";
                _itemCopyDataSource.UpdateMethod = "CopyItem";
                _itemCopyDataSource.SetEventOwnerName( "ItemCopy" );
                _itemCopyDataSource.Selecting += new ObjectDataSourceSelectingEventHandler( _ItemCopyDataSource_Selecting );
                _itemCopyDataSource.Updating += new ObjectDataSourceMethodEventHandler( _ItemCopyDataSource_Updating );
          //      _itemCopyDataSource.ObjectCreated += new ObjectDataSourceObjectEventHandler( _itemCopyDataSource_ObjectCreated );

                Session[ "ItemCopyDataSource" ] = _itemCopyDataSource;
            }
            else
            {
                _itemCopyDataSource = ( SerializableObjectDataSource )Session[ "ItemCopyDataSource" ];
                _itemCopyDataSource.RestoreDelegatesAfterDeserialization( this, "ItemCopy" );
            }

            ItemCopyToSameContractFormView.DataSource = _itemCopyDataSource;
            ItemCopyToDifferentContractFormView.DataSource = _itemCopyDataSource;
        }

        //void _itemCopyDataSource_ObjectCreated( object sender, ObjectDataSourceEventArgs e )
        //{
        //    ItemCopyManager itemCopyManager = null;
        //    ItemCopyContent itemCopyContent = null;
        //    ItemCopyContent itemCopyContentSaved = null;

        //    itemCopyManager = ( VA.NAC.NACCMBrowser.BrowserObj.ItemCopyManager )e.ObjectInstance;
        //    itemCopyContent = itemCopyManager.GetItemCopyContent();

        //    itemCopyContentSaved = ( VA.NAC.NACCMBrowser.BrowserObj.ItemCopyContent )Session[ "ItemCopyContent" ];
        //    itemCopyContent.FillItemCopyContent( itemCopyContentSaved );
        //}


        private void InitItemCopyContent()
        {
            if( Session[ "ItemCopyContent" ] == null )
            {
                _itemCopyContent = new ItemCopyContent( _itemCopyWindowParms );
                Session[ "ItemCopyContent" ] = _itemCopyContent;
            }
            else
            {
                _itemCopyContent = ( ItemCopyContent )Session[ "ItemCopyContent" ];
            }
        }

        private void BindItemCopy()
        {
            if( _itemCopyWindowParms.CopyType == ItemCopyWindowParms.CopyTypes.CopyLocal )
            {
                ItemCopyToSameContractFormView.DataBind();
                BindItemPackagingLists();
            }
            else
                ItemCopyToDifferentContractFormView.DataBind();
        }

        void _ItemCopyDataSource_Selecting( object sender, ObjectDataSourceSelectingEventArgs e )
        {
        }

        void _ItemCopyDataSource_Updating( object sender, ObjectDataSourceMethodEventArgs e )
        {
            ItemCopyContent itemCopyContentForUpdate = ( ItemCopyContent )e.InputParameters[ "ItemCopyContent" ];
            ItemCopyContent itemCopyContent = ( ItemCopyContent )Session[ "ItemCopyContent" ];

            itemCopyContentForUpdate.FillItemCopyContent( itemCopyContent );

            SaveScreenValuesToObject( itemCopyContentForUpdate );

            Session[ "ItemCopyContent" ] = itemCopyContentForUpdate;

            SaveCurrentRowOffset( itemCopyContentForUpdate );
        }

        private void SaveScreenValuesToObject( ItemCopyContent itemCopyContent )
        {
            string fdaAssignedLabelerCode = "";
            string productCode = "";
            string packageCode = "";
            string destinationContractNumber = "";
            string tradeName = "";
            string tradeNameCleansed = "";
            string genericName = "";
            string genericNameCleansed = "";
            string dispensingUnit = "";
            string packageDescription = "";

            string unitOfSale = "";
            string quantityInUnitOfSale = "";
            string unitPackage = "";
            string quantityInUnitPackage = "";
            string unitOfMeasure = "";

            bool bCopyPricing = true;
            bool bCopySubItems = true;

            if( _itemCopyWindowParms.CopyType == ItemCopyWindowParms.CopyTypes.CopyLocal )
            {
                FormView itemCopyToSameContractFormView = ( FormView )ItemCopyForm.FindControl( "ItemCopyToSameContractFormView" );
                if( itemCopyToSameContractFormView != null )
                {
                    if( itemCopyToSameContractFormView.CurrentMode == FormViewMode.Edit )
                    {
                        TextBox FdaAssignedLabelerCodeTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "FdaAssignedLabelerCodeTextBox" );
                        if( FdaAssignedLabelerCodeTextBox != null )
                            fdaAssignedLabelerCode = FdaAssignedLabelerCodeTextBox.Text;

                        TextBox ProductCodeTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "ProductCodeTextBox" );
                        if( ProductCodeTextBox != null )
                            productCode = ProductCodeTextBox.Text;

                        TextBox PackageCodeTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "PackageCodeTextBox" );
                        if( PackageCodeTextBox != null )
                            packageCode = PackageCodeTextBox.Text;

                        TextBox TradeNameTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "TradeNameTextBox" );
                        if( TradeNameTextBox != null )
                        {
                            tradeName = TradeNameTextBox.Text;
                            tradeNameCleansed = CMGlobals.ReplaceNonPrintableCharacters( tradeName ).Trim();
                        }

                        TextBox GenericNameTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "GenericNameTextBox" );
                        if( GenericNameTextBox != null )
                        {
                            genericName = GenericNameTextBox.Text;
                            genericNameCleansed = CMGlobals.ReplaceNonPrintableCharacters( genericName ).Trim();
                        }

                        TextBox DispensingUnitTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "DispensingUnitTextBox" );
                        if( DispensingUnitTextBox != null )
                            dispensingUnit = DispensingUnitTextBox.Text;

                        TextBox PackageDescriptionTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "PackageDescriptionTextBox" );
                        if( PackageDescriptionTextBox != null )
                            packageDescription = PackageDescriptionTextBox.Text;

                        DropDownList UnitOfSaleDropDownList = ( DropDownList )itemCopyToSameContractFormView.FindControl( "UnitOfSaleDropDownList" );
                        if( UnitOfSaleDropDownList != null )
                            unitOfSale = UnitOfSaleDropDownList.SelectedValue;

                        TextBox QuantityInUnitOfSaleTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "QuantityInUnitOfSaleTextBox" );
                        if( QuantityInUnitOfSaleTextBox != null )
                            quantityInUnitOfSale = QuantityInUnitOfSaleTextBox.Text;

                        DropDownList UnitPackageDropDownList = ( DropDownList )itemCopyToSameContractFormView.FindControl( "UnitPackageDropDownList" );
                        if( UnitPackageDropDownList != null )
                            unitPackage = UnitPackageDropDownList.SelectedValue;

                        TextBox QuantityInUnitPackageTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "QuantityInUnitPackageTextBox" );
                        if( QuantityInUnitPackageTextBox != null )
                            quantityInUnitPackage = QuantityInUnitPackageTextBox.Text;

                        DropDownList UnitOfMeasureDropDownList = ( DropDownList )itemCopyToSameContractFormView.FindControl( "UnitOfMeasureDropDownList" );
                        if( UnitOfMeasureDropDownList != null )
                            unitOfMeasure = UnitOfMeasureDropDownList.SelectedValue;

                        CheckBox CopyPricingCheckBox = ( CheckBox )itemCopyToSameContractFormView.FindControl( "CopyPricingCheckBox" );
                        if( CopyPricingCheckBox != null )
                            bCopyPricing = CopyPricingCheckBox.Checked;

                        CheckBox CopySubItemsCheckBox = ( CheckBox )itemCopyToSameContractFormView.FindControl( "CopySubItemsCheckBox" );
                        if( CopySubItemsCheckBox != null )
                            bCopySubItems = CopySubItemsCheckBox.Checked;

                        itemCopyContent.FdaAssignedLabelerCode = fdaAssignedLabelerCode.Trim();
                        itemCopyContent.ProductCode = productCode.Trim();
                        itemCopyContent.PackageCode = packageCode.Trim();
                        itemCopyContent.TradeName = tradeNameCleansed.Trim();
                        itemCopyContent.GenericName = genericNameCleansed.Trim();
                        itemCopyContent.DispensingUnit = dispensingUnit.Trim();
                        itemCopyContent.PackageDescription = packageDescription.Trim();

                        itemCopyContent.UnitOfSale = unitOfSale;
                        itemCopyContent.QuantityInUnitOfSale = decimal.Parse( quantityInUnitOfSale );
                        itemCopyContent.UnitPackage = unitPackage;
                        itemCopyContent.QuantityInUnitPackage = decimal.Parse( quantityInUnitPackage );
                        itemCopyContent.UnitOfMeasure = unitOfMeasure;

                        itemCopyContent.CopyPricing = bCopyPricing;
                        itemCopyContent.CopySubItems = bCopySubItems;
                    }
                }
            }
            else
            {
                FormView itemCopyToDifferentContractFormView = ( FormView )ItemCopyForm.FindControl( "ItemCopyToDifferentContractFormView" );
                if( itemCopyToDifferentContractFormView != null )
                {
                    if( itemCopyToDifferentContractFormView.CurrentMode == FormViewMode.Edit )
                    {
                        TextBox FdaAssignedLabelerCodeTextBox = ( TextBox )itemCopyToDifferentContractFormView.FindControl( "FdaAssignedLabelerCodeTextBox" );
                        if( FdaAssignedLabelerCodeTextBox != null )
                            fdaAssignedLabelerCode = FdaAssignedLabelerCodeTextBox.Text;

                        TextBox ProductCodeTextBox = ( TextBox )itemCopyToDifferentContractFormView.FindControl( "ProductCodeTextBox" );
                        if( ProductCodeTextBox != null )
                            productCode = ProductCodeTextBox.Text;

                        TextBox PackageCodeTextBox = ( TextBox )itemCopyToDifferentContractFormView.FindControl( "PackageCodeTextBox" );
                        if( PackageCodeTextBox != null )
                            packageCode = PackageCodeTextBox.Text;

                        TextBox DestinationContractNumberTextBox = ( TextBox )itemCopyToDifferentContractFormView.FindControl( "DestinationContractNumberTextBox" );
                        if( DestinationContractNumberTextBox != null )
                            destinationContractNumber = DestinationContractNumberTextBox.Text;

                        CheckBox CopyPricingCheckBox = ( CheckBox )itemCopyToDifferentContractFormView.FindControl( "CopyPricingCheckBox" );
                        if( CopyPricingCheckBox != null )
                            bCopyPricing = CopyPricingCheckBox.Checked;

                        CheckBox CopySubItemsCheckBox = ( CheckBox )itemCopyToDifferentContractFormView.FindControl( "CopySubItemsCheckBox" );
                        if( CopySubItemsCheckBox != null )
                            bCopySubItems = CopySubItemsCheckBox.Checked;

                        itemCopyContent.FdaAssignedLabelerCode = fdaAssignedLabelerCode.Trim();
                        itemCopyContent.ProductCode = productCode.Trim();
                        itemCopyContent.PackageCode = packageCode.Trim();
                        itemCopyContent.DestinationContractNumber = destinationContractNumber.Trim();
                        itemCopyContent.CopyPricing = bCopyPricing;
                        itemCopyContent.CopySubItems = bCopySubItems;
                    }
                }
            }
        }

        private void AddClientCloseEvent()
        {
            string closeFunctionText = "CloseWindow( \"true\", \"true\", \"false\" );";
            CancelItemCopyButton.Attributes.Add( "onclick", closeFunctionText );
        }

        private void CloseWindow()
        {
            string closeWindowScript = "CloseWindow( \"true\", \"true\", \"refresh\" );";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "CloseWindowScript", closeWindowScript, true );

            // allow the postback to occur 
            UpdatePanelEventProxy itemCopyUpdateUpdatePanelEventProxy = ( UpdatePanelEventProxy )ItemCopyForm.FindControl( "ItemCopyUpdateUpdatePanelEventProxy" );
            itemCopyUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        // affects highlighting of current item in item grid
        private void SaveCurrentRowOffset( ItemCopyContent itemCopyContent )
        {
            int offset = 0;

            string oldNDC = string.Format( "{0}{1}{2}", _itemCopyWindowParms.FdaAssignedLabelerCode, _itemCopyWindowParms.ProductCode, _itemCopyWindowParms.PackageCode );
            string newNDC = string.Format( "{0}{1}{2}", itemCopyContent.FdaAssignedLabelerCode, itemCopyContent.ProductCode, itemCopyContent.PackageCode );

            if( newNDC.CompareTo( oldNDC ) < 0 )
                offset += 1;

            Session[ "ItemCopyCurrentRowOffset" ] = offset;
        }

        private void SetItemCopyHeaderInfo()
        {
            if( _itemCopyWindowParms.CopyType == ItemCopyWindowParms.CopyTypes.CopyToContract )
            {
                SelectedDrugItemHeader.HeaderTitle = "Copy Item To Contract";
            }
            else if( _itemCopyWindowParms.CopyType == ItemCopyWindowParms.CopyTypes.CopyLocal )
            {
                SelectedDrugItemHeader.HeaderTitle = "Copy Item";
            }
            SelectedDrugItemHeader.FdaAssignedLabelerCode = _itemCopyWindowParms.FdaAssignedLabelerCode;
            SelectedDrugItemHeader.ProductCode = _itemCopyWindowParms.ProductCode;
            SelectedDrugItemHeader.PackageCode = _itemCopyWindowParms.PackageCode;
            SelectedDrugItemHeader.Generic = _itemCopyWindowParms.GenericName;
            SelectedDrugItemHeader.TradeName = _itemCopyWindowParms.TradeName;
        }

   


        protected void ItemCopyToSameOrDifferentContractFormView_OnDataBound( Object sender, EventArgs e )
        {
            SetItemCopyHeaderInfo();

            ItemCopyUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        private bool ValidateDestinationContract( ref string destinationValidationMessage )
        {
            bool bSuccess = false;
            string destinationContractNumber = "";
            string tmpValidationMessage = "";
            
            FormView itemCopyToDifferentContractFormView = ( FormView )ItemCopyForm.FindControl( "ItemCopyToDifferentContractFormView" );
            TextBox DestinationContractNumberTextBox = ( TextBox )itemCopyToDifferentContractFormView.FindControl( "DestinationContractNumberTextBox" );
            if( DestinationContractNumberTextBox != null )
                destinationContractNumber = DestinationContractNumberTextBox.Text;

            if( _destinationDocumentUsedInCopyItem == null )
            {
                bSuccess = CreateLastUsedDetinationContract( destinationContractNumber, ref tmpValidationMessage );

                if( bSuccess == true )
                {
                    // intentionally only checking PBMItems per FSS email 4/27/2010
                    // changed again to specific CopyItemToContract access point 3/29/2011
                    bSuccess = _destinationDocumentUsedInCopyItem.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmCopyItemToContract );
                    if( bSuccess == false )
                    {
                        destinationValidationMessage = "Access Denied";
                    }
                }
                else
                {
                    destinationValidationMessage = tmpValidationMessage;
                    _destinationDocumentUsedInCopyItem = null;
                    Session[ "DestinationDocumentUsedInCopyItem" ] = null;
                }
            }
            else
            {
                if( _destinationDocumentUsedInCopyItem.ContractNumber.CompareTo( destinationContractNumber ) == 0 )
                {
                    // intentionally only checking PBMItems per FSS email 4/27/2010
                    // changed again to specific CopyItemToContract access point 3/29/2011
                    bSuccess = _destinationDocumentUsedInCopyItem.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmCopyItemToContract );
                    if( bSuccess == false )
                    {
                        destinationValidationMessage = "Access Denied";
                    }
                }
                else // current cached document is not the correct one
                {
                    // need to look up new contract
                    bSuccess = CreateLastUsedDetinationContract( destinationContractNumber, ref tmpValidationMessage );

                    if( bSuccess == true )
                    {
                        // intentionally only checking PBMItems per FSS email 4/27/2010
                        // changed again to specific CopyItemToContract access point 3/29/2011
                        bSuccess = _destinationDocumentUsedInCopyItem.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmCopyItemToContract );
                        if( bSuccess == false )
                        {
                            destinationValidationMessage = "Access Denied";
                        }
                    }
                    else
                    {
                        destinationValidationMessage = tmpValidationMessage;
                        _destinationDocumentUsedInCopyItem = null;
                        Session[ "DestinationDocumentUsedInCopyItem" ] = null;
                    }
                }
            }
            return ( bSuccess );
        }


        protected void UpdateItemCopyButton_OnClick( object sender, EventArgs e )
        {
            string validationMessage = "";
            string destinationValidationMessage = "";

            try
            {

                if( ValidateItemCopy( ref validationMessage ) == true )
                {
                    // check security on destination contract
                    if( _itemCopyWindowParms.CopyType == ItemCopyWindowParms.CopyTypes.CopyToContract )
                    {
                        if( ValidateDestinationContract( ref destinationValidationMessage ) == true )
                        {
                            _itemCopyDataSource.Update();
                            ItemCopyToDifferentContractFormView.DataBind();
                        }
                        else
                        {
                            MsgBox.Alert( destinationValidationMessage );
                            return;
                        }
                    }
                    else
                    {
                        _itemCopyDataSource.Update();
                        ItemCopyToSameContractFormView.DataBind();
                        BindItemPackagingLists();
                    }

                    CloseWindow();
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

        protected void UpdateAndContinueItemCopyButton_OnClick( object sender, EventArgs e )
        {
            string validationMessage = "";
            string destinationValidationMessage = "";

            try
            {

                if( ValidateItemCopy( ref validationMessage ) == true )
                {
                    // check security on destination contract
                    if( _itemCopyWindowParms.CopyType == ItemCopyWindowParms.CopyTypes.CopyToContract )
                    {
                        if( ValidateDestinationContract( ref destinationValidationMessage ) == true )
                        {
                            _itemCopyDataSource.Update();
                            ItemCopyToDifferentContractFormView.DataBind();
                        }
                        else
                        {
                            MsgBox.Alert( destinationValidationMessage );
                            return;
                        }
                    }
                    else
                    {
                        _itemCopyDataSource.Update();
                        ItemCopyToSameContractFormView.DataBind();
                        BindItemPackagingLists();
                    }

                    MsgBox.Alert( "Save Completed" );
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

        private void DisableUpdateButton()
        {
            Button updateItemCopyButton = ( Button )ItemCopyForm.FindControl( "UpdateItemCopyButton" );
            if( updateItemCopyButton != null )
            {
                updateItemCopyButton.Enabled = false;
            }
        }

        private bool ValidateItemCopy( ref string validationMessage )
        {
            bool bSuccess = true;

            string fdaAssignedLabelerCode = "";
            string destinationContractNumber = "";
            string tradeName = "";
            string tradeNameCleansed = "";
            string genericName = "";
            string genericNameCleansed = "";
            string quantityInUnitOfSale = "";
            string quantityInUnitPackage = "";

            try
            {
                if( _itemCopyWindowParms.CopyType == ItemCopyWindowParms.CopyTypes.CopyLocal )
                {
                    FormView itemCopyToSameContractFormView = ( FormView )ItemCopyForm.FindControl( "ItemCopyToSameContractFormView" );
                    if( itemCopyToSameContractFormView != null )
                    {
                        if( itemCopyToSameContractFormView.CurrentMode == FormViewMode.Edit )
                        {
                            TextBox FdaAssignedLabelerCodeTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "FdaAssignedLabelerCodeTextBox" );
                            if( FdaAssignedLabelerCodeTextBox != null )
                                fdaAssignedLabelerCode = FdaAssignedLabelerCodeTextBox.Text;

                            TextBox TradeNameTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "TradeNameTextBox" );
                            if( TradeNameTextBox != null )
                            {
                                tradeName = TradeNameTextBox.Text;
                                tradeNameCleansed = CMGlobals.ReplaceNonPrintableCharacters( tradeName ).Trim();
                            }

                            TextBox GenericNameTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "GenericNameTextBox" );
                            if( GenericNameTextBox != null )
                            {
                                genericName = GenericNameTextBox.Text;
                                genericNameCleansed = CMGlobals.ReplaceNonPrintableCharacters( genericName ).Trim();
                            }

                            TextBox QuantityInUnitOfSaleTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "QuantityInUnitOfSaleTextBox" );
                            if( QuantityInUnitOfSaleTextBox != null )
                                quantityInUnitOfSale = QuantityInUnitOfSaleTextBox.Text;

                            TextBox QuantityInUnitPackageTextBox = ( TextBox )itemCopyToSameContractFormView.FindControl( "QuantityInUnitPackageTextBox" );
                            if( QuantityInUnitPackageTextBox != null )
                                quantityInUnitPackage = QuantityInUnitPackageTextBox.Text;

                            int parseResult = 0;
                            if( int.TryParse( fdaAssignedLabelerCode, out  parseResult ) == false )
                            {
                                throw new Exception( "Fda Assigned Labeler Code is not valid." );
                            }

                            if( tradeNameCleansed.Trim().Length <= 0 )
                            {
                                throw new Exception( "Trade Name is required." );
                            }

                            if( genericNameCleansed.Trim().Length <= 0 )
                            {
                                throw new Exception( "Generic Name is required." );
                            }

                            decimal parseDecimal;

                            if( Decimal.TryParse( quantityInUnitOfSale, out parseDecimal ) == false )
                            {
                                throw new Exception( "Quantity In Unit Of Sale is not a valid number." );
                            }

                            if( Decimal.TryParse( quantityInUnitPackage, out parseDecimal ) == false )
                            {
                                throw new Exception( "Quantity In Unit Package is not a valid number." );
                            }

                        }
                    }
                }
                else
                {
                    FormView itemCopyToDifferentContractFormView = ( FormView )ItemCopyForm.FindControl( "ItemCopyToDifferentContractFormView" );
                    if( itemCopyToDifferentContractFormView != null )
                    {
                        if( itemCopyToDifferentContractFormView.CurrentMode == FormViewMode.Edit )
                        {
                            TextBox FdaAssignedLabelerCodeTextBox = ( TextBox )itemCopyToDifferentContractFormView.FindControl( "FdaAssignedLabelerCodeTextBox" );
                            if( FdaAssignedLabelerCodeTextBox != null )
                                fdaAssignedLabelerCode = FdaAssignedLabelerCodeTextBox.Text;

                            TextBox DestinationContractNumberTextBox = ( TextBox )itemCopyToDifferentContractFormView.FindControl( "DestinationContractNumberTextBox" );
                            if( DestinationContractNumberTextBox != null )
                                destinationContractNumber = DestinationContractNumberTextBox.Text;

                            int parseResult = 0;
                            if( int.TryParse( fdaAssignedLabelerCode, out  parseResult ) == false )
                            {
                                throw new Exception( "Fda Assigned Labeler Code is not valid." );
                            }

                            if( destinationContractNumber.Trim().Length <= 0 )
                            {
                                throw new Exception( "Destination contract number is required." );
                            }
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                bSuccess = false;
                validationMessage = string.Format( "The following exception was encountered validating the item destination information: {0}", ex.Message );
            }

            return ( bSuccess );
        }

 

        protected void CancelItemCopyButton_OnClick( object sender, EventArgs e )
        {

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

 

        protected void CopySubItemsCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox copySubItemsCheckBox = ( CheckBox )sender;

            FormView containingFormView = ( FormView )copySubItemsCheckBox.NamingContainer;

            ItemCopyContent ItemCopyContent = ( ItemCopyContent )containingFormView.DataItem;

            bool bIsChecked = false;

            if( ItemCopyContent != null )
            {
                bIsChecked = ItemCopyContent.CopySubItems;
            }

            copySubItemsCheckBox.Checked = bIsChecked;
        }

        protected void CopyPricingCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox copyPricingCheckBox = ( CheckBox )sender;

            FormView containingFormView = ( FormView )copyPricingCheckBox.NamingContainer;

            ItemCopyContent ItemCopyContent = ( ItemCopyContent )containingFormView.DataItem;

            bool bIsChecked = false;

            if( ItemCopyContent != null )
            {
                bIsChecked = ItemCopyContent.CopyPricing;
            }

            copyPricingCheckBox.Checked = bIsChecked;
        }

        // select unit of sale from drop down during edit
        protected void UnitOfSaleDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList unitOfSaleDropDownList = ( DropDownList )sender;
            FormView containingFormView = ( FormView )unitOfSaleDropDownList.NamingContainer;

            ItemCopyContent ItemCopyContent = ( ItemCopyContent )containingFormView.DataItem;

            string selectedUnitOfSale = "EA";

            if( ItemCopyContent != null )
            {
                selectedUnitOfSale = ItemCopyContent.UnitOfSale.ToString();
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

            ItemCopyContent ItemCopyContent = ( ItemCopyContent )containingFormView.DataItem;

            string selectedUnitPackage = "EA";

            if( ItemCopyContent != null )
            {
                selectedUnitPackage = ItemCopyContent.UnitPackage.ToString();
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

            ItemCopyContent ItemCopyContent = ( ItemCopyContent )containingFormView.DataItem;

            string selectedUnitOfMeasure = "EA";

            if( ItemCopyContent != null )
            {
                selectedUnitOfMeasure = ItemCopyContent.UnitOfMeasure.ToString();
            }

            ListItem listItem = unitOfMeasureDropDownList.Items.FindByValue( selectedUnitOfMeasure );
            if( listItem != null )
                listItem.Selected = true;

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

        protected void ItemCopyScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "ItemCopyErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "ItemCopyErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            ItemCopyScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

        protected void ItemCopyToSameContractFormView_OnPreRender( object sender, EventArgs e )
        {
            UpdateAndContinueItemCopyButton.Visible = true;
            UpdateAndContinueItemCopyButton.Enabled = true;
        }

        protected void ItemCopyToDifferentContractFormView_OnPreRender( object sender, EventArgs e )
        {
            UpdateAndContinueItemCopyButton.Visible = false;
            UpdateAndContinueItemCopyButton.Enabled = false;
        }
    }
}
