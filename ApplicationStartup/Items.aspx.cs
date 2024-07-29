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
using System.Threading;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.ReportManager;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using TextBox = System.Web.UI.WebControls.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class Items : System.Web.UI.Page
    {
        // item                                                                                                                 FSS     National    Service         BPA
        private const int RefreshPricesButtonFieldNumber = 0;    //                                                             0           0           0           0
        // for BPA can still view details of parent - fields na to national or service 
        private const int ItemDetailsButtonFieldNumber = 1;                                                     //              1                                   1
        // 2 is edit                                                                                                            2           2           2           2
        private const int ParentItemDropDownListFieldNumber = 3;   // only if BPA                                                                                   3
        private const int ServiceCategoryFieldNumber = 4;   // only if service                                                                          3
        private const int CatalogNumberFieldNumber = 5;     // only if FSS or National                                          3           3
                                   
        private const int ItemDescriptionFieldNumber = 6;   // if FSS, National or Service, BPA displays parent desc            4           4           4           4
        private const int SINFieldNumber = 7;               // only if FSS ( for service, it's included with description )      5
        private const int PackageAsPricedFieldNumber = 8;   // only if FSS or National                                          6           5                       5
        private const int CurrentPriceFieldNumber = 9;          // if BPA, then parent price info                               7           6           5           6
        private const int ItemPriceStartDateFieldNumber = 10;        //                                                         8           7           6           7
        private const int ItemPriceEndDateFieldNumber = 11;         //                                                          9           8           7           8
        private const int HasBPAFieldNumber = 12;           // only if FSS or service                                           10                      8
        private const int ItemLastModifiedByFieldNumber = 13;       //                                                          11          9           9           9
        private const int ReasonMovedToHistoryFieldNumber = 14; 
        private const int MovedToHistoryByFieldNumber = 15;
        private const int DateMovedToHistoryFieldNumber = 16; 
        private const int RemoveItemAndPricesButtonFieldNumber = 17;    //                                                      12          10          10          10 
       
        // hidden
        private const int ParentItemIdFieldNumber = 18;     //                                                                  13          11          11          11   
        private const int ServiceCategoryIdFieldNumber = 19; //                                                                 14          12          12          12
        private const int ServiceCategorySINFieldNumber = 20;
        private const int ParentActiveFieldNumber = 21;
        private const int ParentHistoricalFieldNumber = 22;
        private const int ItemHistoryIdFieldNumber = 23;
        private const int RestorableFieldNumber = 24;

        // price
        private const int PriceDetailsButtonFieldNumber = 0;
        private const int PriceStartDateFieldNumber = 2;
        private const int PriceEndDateFieldNumber = 3;
        private const int PriceFieldNumber = 4;
        private const int PriceMatrixFieldOffset = 5; //$$$+
        private const int IsTemporaryFieldNumber = 5;
        private const int PriceLastModifiedByFieldNumber = 6;
        private const int PriceLastModificationDateFieldNumber = 7;
        private const int PriceReasonMovedToHistoryFieldNumber = 8;
        private const int PriceMovedToHistoryByFieldNumber = 9;
        private const int PriceDateMovedToHistoryFieldNumber = 10;
        private const int RemovePriceButtonFieldNumber = 11;
        private const int IsFromHistoryFieldNumber = 12;
        private const int TPRAlwaysHasBasePriceFieldNumber = 13; 

        private const string SavePriceValidationUserOverrideKeyName = "SavePriceValidationUserOverride";

        private DocumentDataSource _medSurgItemsDataSource = null;
        private DocumentDataSource _medSurgItemPricesDataSource = null;

        private DocumentDataSource _parentItemsDataSource = null;
        private DocumentDataSource _medSurgItemPackagingDataSource = null;
        private DocumentDataSource _itemSINsDataSource = null;
        private DocumentDataSource _serviceCategoryDataSource = null;

        // shared parameter
        private Parameter _modificationStatusIdParameter = null;

        private Parameter _userLoginParameter;

        public string UserLoginParameterValue
        {
            get { return _userLoginParameter.DefaultValue; }
            set { _userLoginParameter.DefaultValue = value; }
        }

        // item parameters
        private Parameter _itemSelectionCriteriaParameter = null;
        private Parameter _searchTextParameter = null;

        private Parameter _contractNumberParameter = null;
        private Parameter _contractIdParameter = null;
        private Parameter _itemIdForItemsParameter = null;           
        private Parameter _itemIdForItemInsertParameter = null;

        private Parameter _catalogNumberParameter = null;
        private Parameter _manufacturersCatalogNumberParameter = null;
        private Parameter _manufacturersNameParameter = null;
        private Parameter _letterOfCommitmentDateParameter = null;
        private Parameter _commericalListPriceParameter = null;
        private Parameter _commercialPricelistDateParameter = null;
        private Parameter _commercialPricelistFOBTermsParameter = null;
        private Parameter _manufacturersCommercialListPriceParameter = null;
        private Parameter _trackingMechanismParameter = null;
        private Parameter _acquisitionCostParameter = null;
        private Parameter _typeOfContractorParameter = null;
        private Parameter _countryOfOriginParameter = null;
        private Parameter _itemDescriptionParameter = null;   

        private Parameter _SINParameter = null;
        private Parameter _serviceCategoryIdParameter = null;
        private Parameter _isServiceParameter = null;
        private Parameter _packageAsPricedParameter = null;

        private Parameter _withAddParameter = null;
        private Parameter _startRowParameter = null;
        private Parameter _pageSizeParameter = null;
        private Parameter _insertedRowNumberParameter = null;
        private Parameter _updatedRowNumberParameter = null;

        // bpa items
        private Parameter _parentItemIdParameter = null;

        // price parameters
        private Parameter _futureHistoricalSelectionCriteriaParameter = null;

        private Parameter _itemIdForPricesParameter = null;

        private Parameter _itemPriceIdParameter = null;
        private Parameter _withAddPriceParameter = null;
        private Parameter _contractExpirationDateParameter = null;
    
        private Parameter _itemPriceIdForPriceInsertParameter = null;

        private Parameter _priceStartDateParameter = null;
        private Parameter _priceEndDateParameter = null;
        private Parameter _isTemporaryParameter = null;
        private Parameter _priceParameter = null;

        // shared 
        private Parameter _isBPAParameter = null;
        private Parameter _parentContractIdParameter = null;

        // itemSINsDataSource parameters
        private Parameter _withAddSINParameter = null;

        // medSurgItemPackagingDataSource parameters
        private Parameter _includeInactivePackagingParameter = null;

        // command line parameters
        private ItemStartupParameters _startupParameters = null;

        // context which affects item grid
        private ItemGridContext _itemGridContext = null;

        private bool[] _documentTypePriceMatrix = null;

        // context which affects price grid
        private PriceGridContext _priceGridContext = null;

        private const int ITEMGRIDVIEWROWHEIGHTESTIMATE = 54;
        private const int PRICEGRIDVIEWROWHEIGHTESTIMATE = 47;


        [Serializable]
        public class ItemStartupParameters
        {
            public string ContractNumber = "";
            public int ContractId = -1;
            public bool IsExpired = false;
            public bool IsItemEditable = false;
            public bool IsPriceEditable = false;
            public bool IsItemDetailsEditable = false;
            public bool IsPriceDetailsEditable = false;

            public bool IsNational = false;
            public string VendorName = "";
            public bool IsBPA = false;
            public bool IsBOA = false;
            public int ParentContractId = -1;
            public bool IsService = false;

            public string DefaultSIN = "";
            public DateTime ContractExpirationDate = DateTime.Today;
        }

        [Serializable]
        public class ItemGridContext
        {
            public bool IsHistorical = false;
        }

        [Serializable]
        public class PriceGridContext
        {
            public bool IsHistorical = false;
        }

#region LoadFunctions

        protected void Page_Load( object sender, EventArgs e )
        {
         
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }

       //     CMGlobals.AddKeepAlive( this.Page, 30000 );

            LoadStartupParms();
            InitItemGridContext();
            InitSharedParms();            
            LoadItems();
            LoadLookupInformation();   // load contract SINs, packaging, parent and service category lookup information

            if( Page.IsPostBack == false )
            {
                InitItemPager( 1, 5 );
                SetItemsGridViewSelectedItem( 0, true );
                BindItemHeaderAndGrid();
            }
            else
            {
                // $$$ first attempt to get row highlighted after insert after postback
                //if( Session[ "LastInsertedItemId" ] != null )
                //{
                //    SelectRowOnPostbackAfterInsert();
                //}
                //else
                //{
                    RestoreItemsGridViewSelectedItem();
                //}
            }

            DisablePriceMatrixForDocumentType();
            InitPriceGridContext();
            LoadItemPrices();

            if( Page.IsPostBack == false )
            {
                SetItemPricesGridViewSelectedItem( 0 );
                BindItemPricesHeaderAndGrid();
                ItemPricesGridViewUpdatePanel.Update();
            }
            else
            {
                RestoreItemPricesGridViewSelectedItem();
              
            }

            AddClientCloseEvent();

            DisableControlsForReadOnlyOrDocumentTypeOrItemGridContext();

            bool bItemGridWasRefreshed = false;
            bool bRebindItems = false;
            bool bRebindItemPrices = false;
            bool bRefreshItemHeaderCount = false;
            if( Page.IsPostBack == true )
            {
                string serverConfirmationDialogResults = "";
                HiddenField ServerConfirmationDialogResultsHiddenField = ( HiddenField )medSurgItemForm.FindControl( "ServerConfirmationDialogResults" );

                if( ServerConfirmationDialogResultsHiddenField != null )
                {
                    serverConfirmationDialogResults = ServerConfirmationDialogResultsHiddenField.Value;
                    MsgBox.ConfirmationResults currentResult = MsgBox.ConfirmationResults.NotDefined;
                    
                    currentResult = MsgBox.GetConfirmationResult( SavePriceValidationUserOverrideKeyName, serverConfirmationDialogResults );
                    if( currentResult != MsgBox.ConfirmationResults.NotSubmitted )
                    {
                        HandleSavePriceValidationUserOverrideResult( currentResult );
                        ServerConfirmationDialogResultsHiddenField.Value = "";
                    }
                    // else currentResult = ...someotherkey
                }


                string refreshItemHeaderCountOnSubmit = "";
                HiddenField RefreshItemHeaderCountOnSubmitHiddenField = ( HiddenField )medSurgItemForm.FindControl( "RefreshItemHeaderCountOnSubmit" );

                if( RefreshItemHeaderCountOnSubmitHiddenField != null )
                {
                    refreshItemHeaderCountOnSubmit = RefreshItemHeaderCountOnSubmitHiddenField.Value;
                    if( refreshItemHeaderCountOnSubmit.Contains( "refresh" ) == true )
                    {
                        bRefreshItemHeaderCount = true;
                        RefreshItemHeaderCountOnSubmitHiddenField.Value = "false";
                    }
                }

                
                string refreshItemScreenOnSubmit = "";
                HiddenField RefreshItemScreenOnSubmitHiddenField = ( HiddenField )medSurgItemForm.FindControl( "RefreshItemScreenOnSubmit" );

                if( RefreshItemScreenOnSubmitHiddenField != null )
                {
                    refreshItemScreenOnSubmit = RefreshItemScreenOnSubmitHiddenField.Value;
                    if( refreshItemScreenOnSubmit.Contains( "true" ) == true )
                    {

                        string rebindItemScreenOnRefreshOnSubmit = "";
                        HiddenField RebindItemScreenOnRefreshOnSubmitHiddenField = ( HiddenField )medSurgItemForm.FindControl( "RebindItemScreenOnRefreshOnSubmit" );

                        if( RebindItemScreenOnRefreshOnSubmitHiddenField != null )
                        {
                            rebindItemScreenOnRefreshOnSubmit = RebindItemScreenOnRefreshOnSubmitHiddenField.Value;
                            if( rebindItemScreenOnRefreshOnSubmit.Contains( "true" ) == true )
                            {
                                bRebindItems = true;
                            }

                            RebindItemScreenOnRefreshOnSubmitHiddenField.Value = "false";
                        }

                        RefreshItemScreenOnSubmitHiddenField.Value = "false";
                        RefreshItemScreenDueToDetailsChanged( bRebindItems, bRefreshItemHeaderCount );
                        bItemGridWasRefreshed = true;
                    }
                }


                string refreshItemPriceScreenOnSubmit = "";
                HiddenField RefreshItemPriceScreenOnSubmitHiddenField = ( HiddenField )medSurgItemForm.FindControl( "RefreshItemPriceScreenOnSubmit" );

                if( RefreshItemPriceScreenOnSubmitHiddenField != null )
                {
                    refreshItemPriceScreenOnSubmit = RefreshItemPriceScreenOnSubmitHiddenField.Value;
                    if( refreshItemPriceScreenOnSubmit.Contains( "true" ) == true )
                    {
                        string rebindItemPriceScreenOnRefreshOnSubmit = "";
                        HiddenField RebindItemPriceScreenOnRefreshOnSubmitHiddenField = ( HiddenField )medSurgItemForm.FindControl( "RebindItemPriceScreenOnRefreshOnSubmit" );

                        if( RebindItemPriceScreenOnRefreshOnSubmitHiddenField != null )
                        {
                            rebindItemPriceScreenOnRefreshOnSubmit = RebindItemPriceScreenOnRefreshOnSubmitHiddenField.Value;
                            if( rebindItemPriceScreenOnRefreshOnSubmit.Contains( "true" ) == true )
                            {
                                bRebindItemPrices = true;
                            }

                            RebindItemPriceScreenOnRefreshOnSubmitHiddenField.Value = "false";
                        }

                        RefreshItemPriceScreenOnSubmitHiddenField.Value = "false";
                        RefreshItemPriceScreenDueToDetailsChanged( bRebindItemPrices );
                        // since doing complete postback ( for now ), need to restore item as well
                        if( bItemGridWasRefreshed == false )
                            RefreshItemScreenDueToDetailsChanged( bRebindItems, bRefreshItemHeaderCount ); 
                    }
                }
            }
        }

        protected void ItemsGridViewUpdatePanel_OnLoad( object sender, EventArgs e )        
        {
            GetAverageItemGridViewRowHeight();
        }

        private void DisableControlsForReadOnlyOrDocumentTypeOrItemGridContext()
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( IsHistorical() == true )
            {
                ItemContextMenu.Enable( "RestoreHistoricalItemCommand", true );
            }
            else
            {
                ItemContextMenu.Enable( "RestoreHistoricalItemCommand", false );
            }
         
         
            if( _startupParameters.IsItemEditable == true && IsHistorical() == false && _startupParameters.IsExpired == false )
            {
                Button addNewItemButton = ( Button )medSurgItemForm.FindControl( "AddNewItemButton" );
                if( addNewItemButton != null )
                {
                    addNewItemButton.Enabled = true;
                }
            }
            else
            {
                Button addNewItemButton = ( Button )medSurgItemForm.FindControl( "AddNewItemButton" );
                if( addNewItemButton != null )
                {
                    addNewItemButton.Enabled = false;
                }
            }

            if( _startupParameters.IsPriceEditable == false || _startupParameters.IsExpired == true )
            {
                Button addNewPriceButton = ( Button )medSurgItemForm.FindControl( "AddNewPriceButton" );
                if( addNewPriceButton != null )
                {
                    addNewPriceButton.Enabled = false;
                }
            }
        }

        private enum PriceMatrixFields
        {
            IsTemporary = 0,           
            MaxPriceMatrixFields = 0
        }

        private void DisablePriceMatrixForDocumentType()
        {
            if( Session[ "DocumentTypePriceMatrix" ] == null )
            {
                _documentTypePriceMatrix = new bool[ ( ( ( int )PriceMatrixFields.MaxPriceMatrixFields ) + 1 ) ];

                if( _startupParameters.IsNational == true )
                {
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTemporary ] = true;
                    

                    // national BPA 
                    if( _startupParameters.IsBPA == true )
                    {
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTemporary ] = false;
                     
                    }
                }
                else // fss
                {
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTemporary ] = true;
                   

                    // FSS BPA
                    if( _startupParameters.IsBPA == true )
                    {
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTemporary ] = false;
                      
                    }

                }

                Session[ "DocumentTypePriceMatrix" ] = _documentTypePriceMatrix;
            }
            else
            {
                _documentTypePriceMatrix = ( bool[] )Session[ "DocumentTypePriceMatrix" ];
            }
        }

        // called on row creation
        private void DisablePriceMatrixForRow( GridViewRow priceGridViewRow )
        {
            if( _documentTypePriceMatrix != null )
            {
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsTemporary ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTemporary ];
            }
            else
            {
                throw new Exception( "Unexpected null value for _documentTypePriceMatrix." );
            }
        }

        // disable the search and other non-edit controls before going into edit mode
        private void EnableControlsForItemEditMode( bool bEnabled )
        {
            Button searcbButton = ( Button )medSurgItemForm.FindControl( "SearchButton" );
            if( searcbButton != null )
            {
                searcbButton.Enabled = bEnabled;
            }

            Button clearSearchButton = ( Button )medSurgItemForm.FindControl( "ClearSearchButton" );
            if( clearSearchButton != null )
            {
                clearSearchButton.Enabled = bEnabled;
            }

            VA.NAC.NACCMBrowser.BrowserObj.TextBox itemSearchTextBox = ( VA.NAC.NACCMBrowser.BrowserObj.TextBox )medSurgItemForm.FindControl( "ItemSearchTextBox" );
            if( itemSearchTextBox != null )
            {
                itemSearchTextBox.Enabled = bEnabled;
            }

            Label itemSearchLabel = ( Label )medSurgItemForm.FindControl( "ItemSearchLabel" );
            if( itemSearchLabel != null )
            {
                itemSearchLabel.Enabled = bEnabled;
            }

            Button addNewItemButton = ( Button )medSurgItemForm.FindControl( "AddNewItemButton" );
            if( addNewItemButton != null )
            {
                addNewItemButton.Enabled = bEnabled;
            }

            RadioButtonList itemFilterRadioButtonList = ( RadioButtonList )medSurgItemForm.FindControl( "ItemFilterRadioButtonList" );
            if( itemFilterRadioButtonList != null )
            {
                itemFilterRadioButtonList.Enabled = bEnabled;
            }
        }

        private void ClearSessionVariables()
        {
            Session[ "ItemGridContext" ] = null;
            Session[ "ItemsDataSource" ] = null;
            Session[ "ItemPricesDataSource" ] = null;
            Session[ "ItemsGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedItemId" ] = null;
            Session[ "LastInsertedItemId" ] = null;
            Session[ "ItemPricesGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedItemPriceId" ] = null;
            Session[ "LastInsertedItemPriceId" ] = null;
            Session[ "LastInsertedItemRowNumber" ] = null;
            Session[ "LastUpdatedItemRowNumber" ] = null;
            Session[ "ModificationStatusIdParameter" ] = null;
            Session[ "ContractNumberParameter" ] = null;
            Session[ "ContractIdParameter" ] = null;
            Session[ "IsBPAParameter" ] = null;
            Session[ "ParentContractIdParameter" ] = null;
            Session[ "MedSurgParentItemsDataSource" ] = null;
            Session[ "ServiceCategoryDataSource" ] = null;
            Session[ "ItemSINsDataSource" ] = null;
            Session[ "ItemPackagingDataSource" ] = null;
            Session[ "IsInSearchMode" ] = null;     
            Session[ "ItemRemovalCurrentRowOffset" ] = null;
            Session[ "AverageItemRowHeight" ] = null;

        }

        private void AddClientCloseEvent()
        {
            string closeFunctionText = "";
  
            closeFunctionText = " RefreshParent2();";
           
            formCloseButton.Attributes.Add( "onclick", closeFunctionText );
        }

        private void InitContextMenus()
        {
            if( Page.IsPostBack != true )
                ItemsGridView.ContextMenuItemCommand += new ContextMenuCommandEventHandler( ItemsGridView_ContextMenuItemCommand );
        }

        private void InitItemGridContext()
        {
            if( Page.Session[ "ItemGridContext" ] == null )
            {
                _itemGridContext = new ItemGridContext();

                _itemGridContext.IsHistorical = false;

                Session[ "ItemGridContext" ] = _itemGridContext;
            }
            else
            {
                _itemGridContext = ( ItemGridContext )Session[ "ItemGridContext" ];
            }

        }

        private bool IsHistorical()
        {
            if( _itemGridContext != null )
                return ( _itemGridContext.IsHistorical );
            else
            {
                _itemGridContext = ( ItemGridContext )Session[ "ItemGridContext" ];
                if( _itemGridContext != null )
                    return ( _itemGridContext.IsHistorical );
                else
                    return ( false );
            }
        }

        private void InitPriceGridContext()
        {
            if( Page.Session[ "PriceGridContext" ] == null )
            {
                _priceGridContext = new PriceGridContext();

                _priceGridContext.IsHistorical = false;

                Session[ "PriceGridContext" ] = _priceGridContext;
            }
            else
            {
                _priceGridContext = ( PriceGridContext )Session[ "PriceGridContext" ];
            }

        }

        private bool IsPriceHistorical()
        {
            if( _priceGridContext != null )
                return ( _priceGridContext.IsHistorical );
            else
            {
                _priceGridContext = ( PriceGridContext )Session[ "PriceGridContext" ];
                if( _priceGridContext != null )
                    return ( _priceGridContext.IsHistorical );
                else
                    return ( false );
            }
        }

        private void LoadStartupParms()
        {
            if( Page.Session[ "ItemStartupParameters" ] != null )
            {
                _startupParameters = ( ItemStartupParameters )Page.Session[ "ItemStartupParameters" ];
            }
            else
            {
                throw new Exception( "Startup parameters not found for med/surg item editor." );
            }
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

            if( Session[ "ContractNumberParameter" ] == null )
            {
                _contractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
                _contractNumberParameter.DefaultValue = _startupParameters.ContractNumber;
            }
            else
            {
                _contractNumberParameter = ( Parameter )Session[ "ContractNumberParameter" ];
            }

            if( Session[ "ContractIdParameter" ] == null )
            {
                _contractIdParameter = new Parameter( "ContractId", TypeCode.Int32 );
                _contractIdParameter.DefaultValue = _startupParameters.ContractId.ToString();
            }
            else
            {
                _contractIdParameter = ( Parameter )Session[ "ContractIdParameter" ];
            }

            if( Session[ "IsBPAParameter" ] == null )
            {
                _isBPAParameter = new Parameter( "IsBPA", TypeCode.Boolean );
                _isBPAParameter.DefaultValue = _startupParameters.IsBPA.ToString();
            }
            else
            {
                _isBPAParameter = ( Parameter )Session[ "IsBPAParameter" ];
            }

            if( Session[ "ParentContractIdParameter" ] == null )
            {
                _parentContractIdParameter = new Parameter( "ParentContractId", TypeCode.Int32 );
                _parentContractIdParameter.DefaultValue = _startupParameters.ParentContractId.ToString();
            }
            else
            {
                _parentContractIdParameter = ( Parameter )Session[ "ParentContractIdParameter" ];
            }

            if( Session[ "IsServiceParameter" ] == null )
            {
                _isServiceParameter = new Parameter( "IsService", TypeCode.Boolean );
                _isServiceParameter.DefaultValue = _startupParameters.IsService.ToString();
            }
            else
            {
                _isServiceParameter = ( Parameter )Session[ "IsServiceParameter" ];
            }
        }

        private void InitItemPager( int startRowNumber, int pageSize )
        {
            if( Page.IsPostBack != true )
            {
                SetPagingParameters( startRowNumber, pageSize );
            }
        }

        private void SetPagingParameters( int startRowNumber, int pageSize )
        {
            if( _pageSizeParameter != null && _startRowParameter != null )
            {
                _pageSizeParameter.DefaultValue = pageSize.ToString();
                _startRowParameter.DefaultValue = startRowNumber.ToString();
            }
        }

        private void SetPagerForSearch()
        {
            _startRowParameter.DefaultValue = "1";

            ItemPager.PageToRowNumber = 1;
        }

        private void SetPagerForItemContextChange()
        {
            _startRowParameter.DefaultValue = "1";

            ItemPager.PageToRowNumber = 1;
        }

        private void LoadItems()
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ];

            if( Page.Session[ "ItemsDataSource" ] == null )
            {
                _medSurgItemsDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.Item, true );
                _medSurgItemsDataSource.ID = "ItemsDataSource";
                _medSurgItemsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _medSurgItemsDataSource.SelectCommand = "SelectMedSurgItemsForContract";

                _medSurgItemsDataSource.UpdateCommand = "UpdateMedSurgItem";

                _medSurgItemsDataSource.InsertCommand = "InsertMedSurgItem";

                _medSurgItemsDataSource.SetEventOwnerName( "MedSurgItems" );
                _medSurgItemsDataSource.Inserted += new SqlDataSourceStatusEventHandler( _medSurgItemsDataSource_Inserted );
                _medSurgItemsDataSource.Updated += new SqlDataSourceStatusEventHandler( _medSurgItemsDataSource_Updated );

                _medSurgItemsDataSource.DeleteCommand = "DeleteMedSurgItemAndPrices";

                CreateItemDataSourceParameters();

                _medSurgItemsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _medSurgItemsDataSource.SelectParameters.Add( _contractNumberParameter );
                _medSurgItemsDataSource.SelectParameters.Add( _contractIdParameter );

                _medSurgItemsDataSource.SelectParameters.Add( _withAddParameter );
                _medSurgItemsDataSource.SelectParameters.Add( _startRowParameter );
                _medSurgItemsDataSource.SelectParameters.Add( _pageSizeParameter );

                _withAddParameter.DefaultValue = "false"; // not adding
                _medSurgItemsDataSource.SelectParameters.Add( _itemSelectionCriteriaParameter );
                _itemSelectionCriteriaParameter.DefaultValue = "A";  // Active
                _medSurgItemsDataSource.SelectParameters.Add( _searchTextParameter );
                _medSurgItemsDataSource.SelectParameters.Add( _isBPAParameter );
                _medSurgItemsDataSource.SelectParameters.Add( _isServiceParameter );
                _medSurgItemsDataSource.SelectParameters.Add( _parentContractIdParameter );

                _medSurgItemsDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _medSurgItemsDataSource.UpdateParameters.Add( _contractNumberParameter );
                _medSurgItemsDataSource.UpdateParameters.Add( _contractIdParameter );
                _medSurgItemsDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
                _medSurgItemsDataSource.UpdateParameters.Add( _itemIdForItemsParameter );

                _medSurgItemsDataSource.UpdateParameters.Add( _parentItemIdParameter );

                _medSurgItemsDataSource.UpdateParameters.Add( _catalogNumberParameter );
               
                _medSurgItemsDataSource.UpdateParameters.Add( _itemDescriptionParameter );
                _medSurgItemsDataSource.UpdateParameters.Add( _SINParameter );
                _medSurgItemsDataSource.UpdateParameters.Add( _serviceCategoryIdParameter );
                _medSurgItemsDataSource.UpdateParameters.Add( _packageAsPricedParameter );
                _medSurgItemsDataSource.UpdateParameters.Add( _searchTextParameter );               
                _medSurgItemsDataSource.UpdateParameters.Add( _updatedRowNumberParameter );

                _medSurgItemsDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
                _medSurgItemsDataSource.InsertParameters.Add( _contractNumberParameter );
                _medSurgItemsDataSource.InsertParameters.Add( _contractIdParameter );
                _medSurgItemsDataSource.InsertParameters.Add( _modificationStatusIdParameter );

                _medSurgItemsDataSource.InsertParameters.Add( _parentItemIdParameter );

                _medSurgItemsDataSource.InsertParameters.Add( _catalogNumberParameter );
               
                _medSurgItemsDataSource.InsertParameters.Add( _itemDescriptionParameter );
                _medSurgItemsDataSource.InsertParameters.Add( _SINParameter );
                _medSurgItemsDataSource.InsertParameters.Add( _serviceCategoryIdParameter );
                _medSurgItemsDataSource.InsertParameters.Add( _packageAsPricedParameter );
                _medSurgItemsDataSource.InsertParameters.Add( _searchTextParameter );

                _medSurgItemsDataSource.InsertParameters.Add( _itemIdForItemInsertParameter );
                _medSurgItemsDataSource.InsertParameters.Add( _insertedRowNumberParameter );

                _medSurgItemsDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
                _medSurgItemsDataSource.DeleteParameters.Add( _contractNumberParameter );
                _medSurgItemsDataSource.DeleteParameters.Add( _contractIdParameter );
                _medSurgItemsDataSource.DeleteParameters.Add( _itemIdForItemsParameter );
                _medSurgItemsDataSource.DeleteParameters.Add( _modificationStatusIdParameter );

                // save to session
                Page.Session[ "ItemsDataSource" ] = _medSurgItemsDataSource;
            }
            else
            {
                _medSurgItemsDataSource = ( DocumentDataSource )Page.Session[ "ItemsDataSource" ];
                _medSurgItemsDataSource.RestoreDelegatesAfterDeserialization( this, "MedSurgItems" );

                RestoreItemDataSourceParameters( _medSurgItemsDataSource );
            }

            ItemsGridView.DataSource = _medSurgItemsDataSource;
        }

        private void LoadLookupInformation()
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ];

            if( _startupParameters.IsBPA == true )
            {
                if( Page.Session[ "MedSurgParentItemsDataSource" ] == null )
                {
                    _parentItemsDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.Item, false );
                    _parentItemsDataSource.ID = "MedSurgParentItemsDataSource";
                    _parentItemsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                    _parentItemsDataSource.SelectCommand = "GetMedSurgParentItems";
                    _parentItemsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                    _parentItemsDataSource.SetEventOwnerName( "MedSurgParentItems" );

                    _parentItemsDataSource.SelectParameters.Add( _contractNumberParameter );
                   _parentItemsDataSource.SelectParameters.Add( _contractIdParameter );

                    Page.Session[ "MedSurgParentItemsDataSource" ] = _parentItemsDataSource;
                }
                else
                {
                    _parentItemsDataSource = ( DocumentDataSource )Page.Session[ "MedSurgParentItemsDataSource" ];
                    _parentItemsDataSource.RestoreDelegatesAfterDeserialization( this, "MedSurgParentItems" );
                }

                // bound during grid row binding
            }
            
            if( _startupParameters.IsService == true )
            {
                if( Page.Session[ "ServiceCategoryDataSource" ] == null )
                {
                    _serviceCategoryDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.Item, false );
                    _serviceCategoryDataSource.ID = "ServiceCategoryDataSource";
                    _serviceCategoryDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                    _serviceCategoryDataSource.SelectCommand = "GetServiceCategoriesForItems";
                    _serviceCategoryDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                    _serviceCategoryDataSource.SetEventOwnerName( "ServiceCategories" );

                    _serviceCategoryDataSource.SelectParameters.Add( _contractNumberParameter );
                    _serviceCategoryDataSource.SelectParameters.Add( _contractIdParameter );
 
                    Page.Session[ "ServiceCategoryDataSource" ] = _serviceCategoryDataSource;
                }
                else
                {
                    _serviceCategoryDataSource = ( DocumentDataSource )Page.Session[ "ServiceCategoryDataSource" ];
                    _serviceCategoryDataSource.RestoreDelegatesAfterDeserialization( this, "ServiceCategories" );
                }

                // bound during grid row binding
            }

            // provides a list of SINs associated with the contract, to be assigned to an item
            if( Page.Session[ "ItemSINsDataSource" ] == null )
            {
                _itemSINsDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.Item, false );
                _itemSINsDataSource.ID = "MedSurgParentItemsDataSource";
                _itemSINsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _itemSINsDataSource.SelectCommand = "SelectSINSForContract2";
                _itemSINsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _itemSINsDataSource.SetEventOwnerName( "ItemSINs" );

                _itemSINsDataSource.SelectParameters.Add( _contractNumberParameter );
 
                _withAddSINParameter = new Parameter( "WithAdd", DbType.Boolean );
                _itemSINsDataSource.SelectParameters.Add( _withAddSINParameter );
                _withAddSINParameter.DefaultValue = "false"; // not adding

                Page.Session[ "ItemSINsDataSource" ] = _itemSINsDataSource;
            }
            else
            {
                _itemSINsDataSource = ( DocumentDataSource )Page.Session[ "ItemSINsDataSource" ];

                _withAddSINParameter = ( Parameter )_itemSINsDataSource.SelectParameters[ "WithAdd" ];

                _itemSINsDataSource.RestoreDelegatesAfterDeserialization( this, "ItemSINs" );
            }

            // bound during grid row binding

            
            // provides a list of packaging to assign to an item
            if( Page.Session[ "ItemPackagingDataSource" ] == null )
            {
                _medSurgItemPackagingDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.Item, false );
                _medSurgItemPackagingDataSource.ID = "ItemPackagingDataSource";
                _medSurgItemPackagingDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _medSurgItemPackagingDataSource.SelectCommand = "SelectItemPackaging";
                _medSurgItemPackagingDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _medSurgItemPackagingDataSource.SetEventOwnerName( "ItemPackaging" );

                _includeInactivePackagingParameter = new Parameter( "IncludeInactive", DbType.Int32 );
                _includeInactivePackagingParameter.DefaultValue = "0"; // false

                Page.Session[ "ItemPackagingDataSource" ] = _medSurgItemPackagingDataSource;
            }
            else
            {
                _medSurgItemPackagingDataSource = ( DocumentDataSource )Page.Session[ "ItemPackagingDataSource" ];

                _includeInactivePackagingParameter = ( Parameter )_medSurgItemPackagingDataSource.SelectParameters[ "IncludeInactive" ];

                _medSurgItemPackagingDataSource.RestoreDelegatesAfterDeserialization( this, "ItemPackaging" );
            }

            // bound during grid row binding
        }

      

        private void LoadItemPrices()
        {
            if( Page.Session[ "ItemPricesDataSource" ] == null )
            {
                _medSurgItemPricesDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.Item, true );
                _medSurgItemPricesDataSource.ID = "ItemPricesDataSource";
                _medSurgItemPricesDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _medSurgItemPricesDataSource.SelectCommand = "SelectMedSurgItemPricesForItem";

                _medSurgItemPricesDataSource.UpdateCommand = "UpdateMedSurgItemPrice";

                _medSurgItemPricesDataSource.SetEventOwnerName( "MedSurgItemPrices" );
                _medSurgItemPricesDataSource.Updated += new SqlDataSourceStatusEventHandler( _medSurgItemPricesDataSource_Updated );

                _medSurgItemPricesDataSource.InsertCommand = "InsertMedSurgItemPrice";
                _medSurgItemPricesDataSource.Inserted += new SqlDataSourceStatusEventHandler( _medSurgItemPricesDataSource_Inserted );

                _medSurgItemPricesDataSource.DeleteCommand = "DeleteMedSurgPriceForItemPriceId";  // "DeletePriceForItemPriceId"; 

                CreateItemPricesDataSourceParameters();

                _medSurgItemPricesDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _medSurgItemPricesDataSource.SelectParameters.Add( _futureHistoricalSelectionCriteriaParameter );
                _futureHistoricalSelectionCriteriaParameter.DefaultValue = "B"; // Both Active and Future
                _medSurgItemPricesDataSource.SelectParameters.Add( _contractNumberParameter );
                _medSurgItemPricesDataSource.SelectParameters.Add( _contractIdParameter );
                _medSurgItemPricesDataSource.SelectParameters.Add( _itemIdForPricesParameter );
                _medSurgItemPricesDataSource.SelectParameters.Add( _withAddPriceParameter );
                _withAddPriceParameter.DefaultValue = "false"; // not adding
                _medSurgItemPricesDataSource.SelectParameters.Add( _contractExpirationDateParameter );
                _contractExpirationDateParameter.DefaultValue = _startupParameters.ContractExpirationDate.ToShortDateString(); 

                _medSurgItemPricesDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _medSurgItemPricesDataSource.UpdateParameters.Add( _contractNumberParameter );
                _medSurgItemPricesDataSource.UpdateParameters.Add( _contractIdParameter );
                _medSurgItemPricesDataSource.UpdateParameters.Add( _modificationStatusIdParameter );                
                _medSurgItemPricesDataSource.UpdateParameters.Add( _itemPriceIdParameter );

                _medSurgItemPricesDataSource.UpdateParameters.Add( _priceStartDateParameter );
                _medSurgItemPricesDataSource.UpdateParameters.Add( _priceEndDateParameter );
                _medSurgItemPricesDataSource.UpdateParameters.Add( _isTemporaryParameter );
                _medSurgItemPricesDataSource.UpdateParameters.Add( _priceParameter );
                
                _medSurgItemPricesDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;

                _medSurgItemPricesDataSource.InsertParameters.Add( _modificationStatusIdParameter );
                _medSurgItemPricesDataSource.InsertParameters.Add( _itemIdForPricesParameter );

                _medSurgItemPricesDataSource.InsertParameters.Add( _priceStartDateParameter );
                _medSurgItemPricesDataSource.InsertParameters.Add( _priceEndDateParameter );
                _medSurgItemPricesDataSource.InsertParameters.Add( _priceParameter );
                _medSurgItemPricesDataSource.InsertParameters.Add( _isBPAParameter );
                _medSurgItemPricesDataSource.InsertParameters.Add( _isTemporaryParameter );

                _medSurgItemPricesDataSource.InsertParameters.Add( _itemPriceIdForPriceInsertParameter );

                _medSurgItemPricesDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
                _medSurgItemPricesDataSource.DeleteParameters.Add( _itemPriceIdParameter );
                _medSurgItemPricesDataSource.DeleteParameters.Add( _modificationStatusIdParameter );

                // save to session
                Page.Session[ "ItemPricesDataSource" ] = _medSurgItemPricesDataSource;
            }
            else
            {
                _medSurgItemPricesDataSource = ( DocumentDataSource )Page.Session[ "ItemPricesDataSource" ];

                _medSurgItemPricesDataSource.RestoreDelegatesAfterDeserialization( this, "MedSurgItemPrices" );

                RestoreItemPricesDataSourceParameters( _medSurgItemPricesDataSource );
            }

            ItemPricesGridView.DataSource = _medSurgItemPricesDataSource;

        }
 
        private void BindItemHeaderAndGrid()
        {
            RefreshMainHeader( false );

            try
            {
                // bind
                ItemsGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }

        private void BindItemPricesHeaderAndGrid()
        {

            SetItemPriceDataSourceParameterValues();

            SaveSelectedItemInfoIntoSession( ItemsGridView.SelectedIndex );  // header depends on session

            SetItemPriceHeaderInfo( ItemsGridView.SelectedIndex );
           
            try
            {
                // bind
                ItemPricesGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }

  
        private void CreateItemDataSourceParameters()
        {
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );

            _withAddParameter = new Parameter( "WithAdd", TypeCode.Boolean );
            _startRowParameter = new Parameter( "StartRow", TypeCode.Int32 );
            _pageSizeParameter = new Parameter( "PageSize", TypeCode.Int32 );

            _itemSelectionCriteriaParameter = new Parameter( "ItemSelectionCriteria", TypeCode.String );
            _searchTextParameter = new Parameter( "SearchText", TypeCode.String );

            _itemIdForItemsParameter = new Parameter( "ItemId", TypeCode.Int32 );

            _parentItemIdParameter = new Parameter( "ParentItemId", TypeCode.Int32 );

            _catalogNumberParameter = new Parameter( "CatalogNumber", TypeCode.String );
            _catalogNumberParameter.Size = 70;

            _manufacturersCatalogNumberParameter = new Parameter( "ManufacturersCatalogNumber", TypeCode.String );
            _manufacturersCatalogNumberParameter.Size = 100;

            _manufacturersNameParameter = new Parameter( "ManufacturersName", TypeCode.String );
            _manufacturersNameParameter.Size = 100;

            _manufacturersCommercialListPriceParameter = new Parameter( "ManufacturersCommercialListPrice", TypeCode.Double );
            _letterOfCommitmentDateParameter = new Parameter( "LetterOfCommitmentDate", TypeCode.DateTime );
            _commericalListPriceParameter = new Parameter( "CommercialListPrice", TypeCode.Double );
            _commercialPricelistDateParameter = new Parameter( "CommercialPricelistDate", TypeCode.DateTime );
            _commercialPricelistFOBTermsParameter = new Parameter( "CommercialPricelistFOBTerms", TypeCode.String );
            _commercialPricelistFOBTermsParameter.Size = 40;


            _trackingMechanismParameter = new Parameter( "TrackingMechanism", TypeCode.String );
            _trackingMechanismParameter.Size = 100;
            _acquisitionCostParameter = new Parameter( "AcquisitionCost", TypeCode.Double );
            
            _typeOfContractorParameter = new Parameter( "TypeOfContractor", TypeCode.String );
            _typeOfContractorParameter.Size = 100;

            _countryOfOriginParameter = new Parameter( "CountryOfOrigin", TypeCode.Int32 );


            _itemDescriptionParameter = new Parameter( "ItemDescription", TypeCode.String );
            _itemDescriptionParameter.Size = 800;
 
            _SINParameter = new Parameter( "SIN", TypeCode.String );
            _SINParameter.Size = 50;

            _serviceCategoryIdParameter = new Parameter( "ServiceCategoryId", TypeCode.Int32 );

            _packageAsPricedParameter = new Parameter( "PackageAsPriced", TypeCode.String );
            _packageAsPricedParameter.Size = 2;

            _itemIdForItemInsertParameter = new Parameter( "ItemId", TypeCode.Int32 );
            _itemIdForItemInsertParameter.Direction = ParameterDirection.Output;

            _insertedRowNumberParameter = new Parameter( "InsertedRowNumber", TypeCode.Int32 );
            _insertedRowNumberParameter.Direction = ParameterDirection.Output;

            _updatedRowNumberParameter = new Parameter( "UpdatedRowNumber", TypeCode.Int32 );
            _updatedRowNumberParameter.Direction = ParameterDirection.Output;
        }

        private void RestoreItemDataSourceParameters( DocumentDataSource medSurgItemDataSource )
        {
            // select
            _withAddParameter = medSurgItemDataSource.SelectParameters[ "WithAdd" ];
            _itemSelectionCriteriaParameter = medSurgItemDataSource.SelectParameters[ "ItemSelectionCriteria" ];
            _searchTextParameter = medSurgItemDataSource.SelectParameters[ "SearchText" ];
            _startRowParameter = medSurgItemDataSource.SelectParameters[ "StartRow" ];
            _pageSizeParameter = medSurgItemDataSource.SelectParameters[ "PageSize" ];

            // select, update and insert            
            _contractNumberParameter = medSurgItemDataSource.SelectParameters[ "ContractNumber" ];
            _contractIdParameter = medSurgItemDataSource.SelectParameters[ "ContractId" ];

            // update and insert
            _itemIdForItemsParameter = medSurgItemDataSource.UpdateParameters[ "ItemId" ];
            _parentItemIdParameter = medSurgItemDataSource.UpdateParameters[ "ParentItemId" ];
   
            _catalogNumberParameter = medSurgItemDataSource.UpdateParameters[ "CatalogNumber" ];
          
            //_manufacturersCatalogNumberParameter = medSurgItemDataSource.UpdateParameters[ "ManufacturersCatalogNumber" ];         
            //_manufacturersNameParameter = medSurgItemDataSource.UpdateParameters[ "ManufacturersName" ];
            //_manufacturersCommercialListPriceParameter = medSurgItemDataSource.UpdateParameters[ "ManufacturersCommercialListPrice" ];
            //_letterOfCommitmentDateParameter = medSurgItemDataSource.UpdateParameters[ "LetterOfCommitmentDate" ];
            //_commericalListPriceParameter = medSurgItemDataSource.UpdateParameters[ "CommercialListPrice" ];
            //_commercialPricelistDateParameter = medSurgItemDataSource.UpdateParameters[ "CommercialPricelistDate" ];
            //_commercialPricelistFOBTermsParameter = medSurgItemDataSource.UpdateParameters[ "CommercialPricelistFOBTerms" ];

            //_trackingMechanismParameter = medSurgItemDataSource.UpdateParameters[ "TrackingMechanism" ];
            //_acquisitionCostParameter = medSurgItemDataSource.UpdateParameters[ "AcquisitionCost" ];
            //_typeOfContractorParameter = medSurgItemDataSource.UpdateParameters[ "TypeOfContractor" ];

            //_countryOfOriginParameter = medSurgItemDataSource.UpdateParameters[ "CountryOfOrigin" ];

            _itemDescriptionParameter = medSurgItemDataSource.UpdateParameters[ "ItemDescription" ];     
            _SINParameter = medSurgItemDataSource.UpdateParameters[ "SIN" ];
            _serviceCategoryIdParameter = medSurgItemDataSource.UpdateParameters[ "ServiceCategoryId" ];
            _packageAsPricedParameter = medSurgItemDataSource.UpdateParameters[ "PackageAsPriced" ];
            _updatedRowNumberParameter = medSurgItemDataSource.UpdateParameters[ "UpdatedRowNumber" ];

            // insert
            _itemIdForItemInsertParameter = medSurgItemDataSource.InsertParameters[ "ItemId" ];
            _insertedRowNumberParameter = medSurgItemDataSource.InsertParameters[ "InsertedRowNumber" ];

            // update, insert and delete
            _userLoginParameter = medSurgItemDataSource.UpdateParameters[ "UserLogin" ];
        }


        private void RefreshMainHeader( bool bWithRefresh )
        {
            MainHeaderTitleLabel2.Text = string.Format( "For Contract {0}", _startupParameters.ContractNumber );
            MainHeaderTitleLabel3.Text = string.Format( "{0}", _startupParameters.VendorName );

            if( _itemGridContext.IsHistorical == false )
            {
                MainHeaderItemCount.Text = string.Format( "{0} Total Items", GetCurrentItemCount( bWithRefresh ) );
            }
            else
            {
                MainHeaderItemCount.Text = "";
            }
        }

        private int GetCurrentItemCount( bool bWithRefresh )
        {
            int totalItems = 0;

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
            if( currentDocument != null )
            {
                if( bWithRefresh == true )
                {
                    try
                    {
                        currentDocument.UpdateItemCounts();
                    }
                    catch( Exception ex )
                    {
                        MsgBox.ShowErrorFromUpdatePanel( Page, ex );
                    }
                }
                totalItems = currentDocument.ActiveMedSurgItemCount + currentDocument.FutureMedSurgItemCount + currentDocument.PricelessMedSurgItemCount;
            }
            return ( totalItems );
        }

        protected void ItemsGridView_Init( object sender, EventArgs e )
        {
            GridView medSurgItemsGridView = ( GridView )sender;
            medSurgItemsGridView.SetContextMenu( ItemContextMenu );

            medSurgItemsGridView.ContextMenuItemCommand += new ContextMenuCommandEventHandler( ItemsGridView_ContextMenuItemCommand );

            ContextMenuItem restoreMenuItem = new ContextMenuItem( "Restore", "RestoreHistoricalItemCommand" );
            restoreMenuItem.ClientOnClick = "presentConfirmationMessage('Restore the selected item as an active item on this contract?')";
            ItemContextMenu.ContextMenuItems.Add( restoreMenuItem );

            ItemPager.SetGridView( medSurgItemsGridView );
            medSurgItemsGridView.SetPager( ItemPager );
            ItemPager.PagerCommand += new PagerCommandEventHandler( ItemsGridView_PagerCommand );
        }

        protected void ItemPager_OnInit( object sender, EventArgs e )
        {
            Pager itemPager = ( Pager )sender;
            itemPager.RowsPerPage = 5;
            itemPager.CurrentPage = 1;
        }
        
        protected void ItemsGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightItemRow( 0 );

            GridView medSurgItemsGridView = ( GridView )sender;
            GridViewRow headerRow = medSurgItemsGridView.HeaderRow;

            // no medSurg items
            if( headerRow == null )
                return;

            // hide id fields
            headerRow.Cells[ ParentItemIdFieldNumber ].Visible = false;
            headerRow.Cells[ ServiceCategoryIdFieldNumber ].Visible = false;
            headerRow.Cells[ ServiceCategorySINFieldNumber ].Visible = false;
            headerRow.Cells[ ParentActiveFieldNumber ].Visible = false;
            headerRow.Cells[ ParentHistoricalFieldNumber ].Visible = false;
            headerRow.Cells[ ItemHistoryIdFieldNumber ].Visible = false;
            headerRow.Cells[ RestorableFieldNumber ].Visible = false;

            if( _startupParameters.IsBPA == true )
            {
                headerRow.Cells[ ParentItemDropDownListFieldNumber ].Visible = true;
            }
            else
            {
                headerRow.Cells[ ParentItemDropDownListFieldNumber ].Visible = false;
            }

            if( _startupParameters.IsService == true )
            {
                headerRow.Cells[ ServiceCategoryFieldNumber ].Visible = true;
            }
            else
            {
                headerRow.Cells[ ServiceCategoryFieldNumber ].Visible = false;
            }

            if( _startupParameters.IsBPA == false && _startupParameters.IsService == false )
            {
                headerRow.Cells[ CatalogNumberFieldNumber ].Visible = true;
            }
            else
            {
                headerRow.Cells[ CatalogNumberFieldNumber ].Visible = false;                  
            }

            if( _startupParameters.IsBPA == false )
            {
                headerRow.Cells[ ItemDescriptionFieldNumber ].Visible = true;              
            }
            else
            {
                headerRow.Cells[ ItemDescriptionFieldNumber ].Visible = true;    // $$$ was false 3/10/22
              
                // change the header text to reflect parent description and price for BPA
                ChangeHeaderTextForColumn( headerRow, CurrentPriceFieldNumber, "Parent Price" );
                ChangeHeaderTextForColumn( headerRow, ItemDescriptionFieldNumber, "Parent Description" );  // $$$ added 3/10/22
            }

            if( _startupParameters.IsBPA == false && _startupParameters.IsService == false && _startupParameters.IsNational == false )
            {
                headerRow.Cells[ SINFieldNumber ].Visible = true;
            }
            else
            {
                headerRow.Cells[ SINFieldNumber ].Visible = false;
            }

            if( _startupParameters.IsService == false && _startupParameters.IsBPA == false )
            {
                headerRow.Cells[ PackageAsPricedFieldNumber ].Visible = true;
            }
            else
            {
                headerRow.Cells[ PackageAsPricedFieldNumber ].Visible = false;
            }        
            
            if( _startupParameters.IsBPA == true || _startupParameters.IsNational == true )
            {
                headerRow.Cells[ HasBPAFieldNumber ].Visible = false;
            }
            else
            {
                headerRow.Cells[ HasBPAFieldNumber ].Visible = true;
            }

            // historical filter is selected
            if( IsHistorical() == true )
            {
                // no active price to display with items for historical
                headerRow.Cells[ CurrentPriceFieldNumber ].Visible = false;
                headerRow.Cells[ ItemPriceStartDateFieldNumber ].Visible = false;
                headerRow.Cells[ ItemPriceEndDateFieldNumber ].Visible = false;
                headerRow.Cells[ ItemLastModifiedByFieldNumber ].Visible = false;

                headerRow.Cells[ ReasonMovedToHistoryFieldNumber ].Visible = true;
                headerRow.Cells[ MovedToHistoryByFieldNumber ].Visible = true;
                headerRow.Cells[ DateMovedToHistoryFieldNumber ].Visible = true;
            }
            else
            {
                headerRow.Cells[ ReasonMovedToHistoryFieldNumber ].Visible = false;
                headerRow.Cells[ MovedToHistoryByFieldNumber ].Visible = false;
                headerRow.Cells[ DateMovedToHistoryFieldNumber ].Visible = false;
            }
         
            foreach( GridViewRow gridViewRow in medSurgItemsGridView.Rows )
            {

                // hide id fields
                gridViewRow.Cells[ ParentItemIdFieldNumber ].Visible = false;
                gridViewRow.Cells[ ServiceCategoryIdFieldNumber ].Visible = false;
                gridViewRow.Cells[ ServiceCategorySINFieldNumber ].Visible = false;
                gridViewRow.Cells[ ParentActiveFieldNumber ].Visible = false;
                gridViewRow.Cells[ ParentHistoricalFieldNumber ].Visible = false;
                gridViewRow.Cells[ ItemHistoryIdFieldNumber ].Visible = false;
                gridViewRow.Cells[ RestorableFieldNumber ].Visible = false;

                if( _startupParameters.IsBPA == true )
                {
                    gridViewRow.Cells[ ParentItemDropDownListFieldNumber ].Visible = true;
                }
                else
                {
                    gridViewRow.Cells[ ParentItemDropDownListFieldNumber ].Visible = false;
                }

                if( _startupParameters.IsService == true )
                {
                    gridViewRow.Cells[ ServiceCategoryFieldNumber ].Visible = true;

                    // disable details button
                    DisableItemDetailsButtonForRow( gridViewRow );
                }
                else
                {
                    gridViewRow.Cells[ ServiceCategoryFieldNumber ].Visible = false;
                }

                if( _startupParameters.IsBPA == false && _startupParameters.IsService == false )
                {
                    gridViewRow.Cells[ CatalogNumberFieldNumber ].Visible = true;
                }
                else
                {
                    gridViewRow.Cells[ CatalogNumberFieldNumber ].Visible = false;      
                }

                if( _startupParameters.IsBPA == false )
                {
                    gridViewRow.Cells[ ItemDescriptionFieldNumber ].Visible = true;
                 
                }
                else
                {
                    gridViewRow.Cells[ ItemDescriptionFieldNumber ].Visible = true;  // $$$ changed from false to true 3/10/22
                   

                    // disable details button -- long term need the details to just show parent
                    DisableItemDetailsButtonForRow( gridViewRow );
                }

                if( _startupParameters.IsNational == true )
                {
                    DisableItemDetailsButtonForRow( gridViewRow );
                }

                if( _startupParameters.IsBPA == false && _startupParameters.IsService == false && _startupParameters.IsNational == false )
                {
                    gridViewRow.Cells[ SINFieldNumber ].Visible = true;
                }
                else
                {
                    gridViewRow.Cells[ SINFieldNumber ].Visible = false;
                }

                if( _startupParameters.IsService == false && _startupParameters.IsBPA == false )
                {
                    gridViewRow.Cells[ PackageAsPricedFieldNumber ].Visible = true;
                }
                else
                {
                    gridViewRow.Cells[ PackageAsPricedFieldNumber ].Visible = false;
                }

            

                if( _startupParameters.IsBPA == true || _startupParameters.IsNational == true )
                {
                    gridViewRow.Cells[ HasBPAFieldNumber ].Visible = false;
                }
                else
                {
                    gridViewRow.Cells[ HasBPAFieldNumber ].Visible = true;
                }

                // historical filter is selected
                if( IsHistorical() == true )
                {
                    // no active price to display with items for historical
                    gridViewRow.Cells[ CurrentPriceFieldNumber ].Visible = false;
                    gridViewRow.Cells[ ItemPriceStartDateFieldNumber ].Visible = false;
                    gridViewRow.Cells[ ItemPriceEndDateFieldNumber ].Visible = false;
                    gridViewRow.Cells[ ItemLastModifiedByFieldNumber ].Visible = false;

                    gridViewRow.Cells[ ReasonMovedToHistoryFieldNumber ].Visible = true;
                    gridViewRow.Cells[ MovedToHistoryByFieldNumber ].Visible = true;
                    gridViewRow.Cells[ DateMovedToHistoryFieldNumber ].Visible = true;
                }
                else
                {
                    gridViewRow.Cells[ ReasonMovedToHistoryFieldNumber ].Visible = false;
                    gridViewRow.Cells[ MovedToHistoryByFieldNumber ].Visible = false;
                    gridViewRow.Cells[ DateMovedToHistoryFieldNumber ].Visible = false;
                }

                // adjust for edit mode            
                if( medSurgItemsGridView.EditIndex != -1 )
                {
                    if( _startupParameters.IsBPA == false && _startupParameters.IsService == false )
                    {
                        Label catalogNumberLabel2 = null;

                        catalogNumberLabel2 = ( Label )gridViewRow.Cells[ CatalogNumberFieldNumber ].FindControl( "catalogNumberLabel2" );
                        if( catalogNumberLabel2 != null )
                            catalogNumberLabel2.Visible = false;

                    }
                    if( _startupParameters.IsBPA == false || _startupParameters.IsService == true )
                    {
                        Label itemDescriptionLabel2 = null;

                        itemDescriptionLabel2 = ( Label )gridViewRow.Cells[ ItemDescriptionFieldNumber ].FindControl( "itemDescriptionLabel2" );
                        if( itemDescriptionLabel2 != null )
                            itemDescriptionLabel2.Visible = false;
                    }
                }              
            }


            string saveGridViewRowHeightScript = String.Format( "SaveItemGridViewRowHeight();" );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SaveGridViewRowHeightScript", saveGridViewRowHeightScript, true ); // runs after controls established

            HideProgressIndicator();
        }

        protected void ChangeHeaderTextForColumn( GridViewRow headerRow, int columnNumber, string headerText )
        {
            if( headerRow != null )
            {
                if( headerRow.Cells[ columnNumber ] != null )
                {
                    headerRow.Cells[ columnNumber ].Text = headerText;
                }
            }                                     
        }

        protected void DisableItemDetailsButtonForRow( GridViewRow gridViewRow )
        {
            ControlCollection controlsInCell = null;

            if( gridViewRow != null )
            {
                controlsInCell = gridViewRow.Cells[ ItemDetailsButtonFieldNumber ].Controls;

                if( controlsInCell != null )
                {
                    foreach( Control control in controlsInCell )
                    {
                        if( control != null )
                        {
                            Type controlType = control.GetType();
                            string typeName = controlType.Name;
                            if( typeName.CompareTo( "Button" ) == 0 )
                            {
                                ( ( Button )control ).Enabled = false;
                            }
                        }
                    }
                }
            }
        }

#endregion LoadFunctions

        #region ItemGridBinding

        protected void RefreshPricesButton_DataBinding( object sender, EventArgs e )
        {
            Button refreshPricesButton = ( Button )sender;
            if( refreshPricesButton != null )
                MultiLineButtonText( refreshPricesButton, new string[] { "Refresh", "Prices" } );
        }

        protected void RemoveItemAndPricesButton_DataBinding( object sender, EventArgs e )
        {
            Button removeItemAndPricesButton = ( Button )sender;
            if( removeItemAndPricesButton != null )
                MultiLineButtonText( removeItemAndPricesButton, new string[] { "Remove Item", "And Prices" } );
        }

  
        protected void ItemsGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                if( e.Row.RowType == DataControlRowType.DataRow )
                {
                    if( ( ( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                        ( ( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert ) )
                    {                     
                        Button removeItemAndPricesButton = null;
                        removeItemAndPricesButton = ( Button )e.Row.FindControl( "RemoveItemAndPricesButton" );
                        if( removeItemAndPricesButton != null )
                        {
                            removeItemAndPricesButton.Attributes.Add( "onclick", "presentConfirmationMessage('Permanently delete the selected item and its associated prices?')" );
                        }
                                                            
                        if( _startupParameters.IsItemEditable == false || IsHistorical() == true || _startupParameters.IsExpired == true )
                        {
                            if( removeItemAndPricesButton != null )
                            {
                                removeItemAndPricesButton.Enabled = false;
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
                    else // bind ddls used during edit
                    {
                        if( _startupParameters.IsBPA == true )
                        {
                            DropDownList parentItemDropDownList = ( DropDownList )e.Row.FindControl( "parentItemDropDownList" );
                            parentItemDropDownList.DataSource = _parentItemsDataSource;
                            parentItemDropDownList.DataBind();
                        }

                        if( _startupParameters.IsBPA == false && _startupParameters.IsService == false && _startupParameters.IsNational == false )
                        {
                            DropDownList itemSINDropDownList = ( DropDownList )e.Row.FindControl( "ItemSINDropDownList" );
                            itemSINDropDownList.DataSource = _itemSINsDataSource;
                            itemSINDropDownList.DataBind();
                        }

                        if( _startupParameters.IsService == false )
                        {
                            DropDownList packageAsPricedDropDownList = ( DropDownList )e.Row.FindControl( "packageAsPricedDropDownList" );
                            packageAsPricedDropDownList.DataSource = _medSurgItemPackagingDataSource;
                            packageAsPricedDropDownList.DataBind();
                        }
                        else
                        {
                            DropDownList serviceCategoryDropDownList = ( DropDownList )e.Row.FindControl( "serviceCategoryDropDownList" );
                            serviceCategoryDropDownList.DataSource = _serviceCategoryDataSource;
                            serviceCategoryDropDownList.DataBind();
                        }

                    }

                }

            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }


        protected void ParentItemDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList parentItemDropDownList = ( DropDownList )sender;
            GridViewRow gridViewRow = ( GridViewRow )parentItemDropDownList.NamingContainer;
            int parentItemId = -1;

            if( gridViewRow.DataItem != null )
            {
                string parentItemIdString = ( ( DataRowView )gridViewRow.DataItem )[ "ParentItemId" ].ToString();
                if( parentItemIdString != null )
                {
                    if( int.TryParse( parentItemIdString, out parentItemId ) )
                    {
                        ListItem listItem = parentItemDropDownList.Items.FindByValue( parentItemId.ToString() );
                        if( listItem != null )
                        {
                            listItem.Selected = true;
                        }
                        else // bind to the "select" item presented as "  --  "
                        {
                            int selectItemId = -1;
                            string selectItemIdString = selectItemId.ToString();
                            listItem = parentItemDropDownList.Items.FindByValue( selectItemIdString );
                            if( listItem != null )
                            {
                                listItem.Selected = true;
                            }
                        }
                    }
                }
            }

        }

        protected void serviceCategoryLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label serviceCategoryLabel = ( Label )sender;
            GridViewRow gridViewRow = ( GridViewRow )serviceCategoryLabel.NamingContainer;
            int serviceCategoryId = -1;
            bool bSuccess = false;

            DataSet serviceCategoryDetailsDataSet = null;

            ItemDB itemDB = ( ItemDB )Page.Session[ "ItemDB" ];
            if( itemDB != null )
            {
                if( gridViewRow.DataItem != null )
                {
                    string categoryIdString = ( ( DataRowView )gridViewRow.DataItem )[ "ServiceCategoryId" ].ToString();
                    if( categoryIdString != null )
                    {
                        if( int.TryParse( categoryIdString, out serviceCategoryId ) )
                        {
                            // service category is only visible for service contracts ( with service category id ) but the binding call is being made all the time (?)
                            if( serviceCategoryId != -1 )
                            {
                                bSuccess = itemDB.GetServiceCategoryDetails( ref serviceCategoryDetailsDataSet, _startupParameters.ContractNumber, _startupParameters.ContractId, serviceCategoryId );
                                if( bSuccess == false )
                                {
                                    throw new Exception( itemDB.ErrorMessage );
                                }
                                else
                                {
                                    // show the data
                                    int count = itemDB.RowsReturned;

                                    if( count == 1 )
                                    {
                                        DataRow currentRow = serviceCategoryDetailsDataSet.Tables[ ItemDB.ServiceCategoryDetailsTableName ].Rows[ 0 ];

                                        serviceCategoryLabel.Text = currentRow[ "ServiceCategoryDescription" ].ToString();

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void ServiceCategoryDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList serviceCategoryDropDownList = ( DropDownList )sender;
            GridViewRow gridViewRow = ( GridViewRow )serviceCategoryDropDownList.NamingContainer;
            int serviceCategoryId = -1;

            if( gridViewRow.DataItem != null )
            {
                string serviceCategoryIdString = ( ( DataRowView )gridViewRow.DataItem )[ "ServiceCategoryId" ].ToString();
                if( serviceCategoryIdString != null )
                {
                    if( int.TryParse( serviceCategoryIdString, out serviceCategoryId ) )
                    {
                        ListItem listItem = serviceCategoryDropDownList.Items.FindByValue( serviceCategoryId.ToString() );
                        if( listItem != null )
                        {
                            listItem.Selected = true;
                        }                       
                    }
                }
            }
        }

        // select SIN from drop down during edit
        protected void ItemSINDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList itemSINDropDownList = ( DropDownList )sender;
            GridViewRow gridViewRow = ( GridViewRow )itemSINDropDownList.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string itemSIN = ( ( DataRowView )gridViewRow.DataItem )[ "SIN" ].ToString();
                ListItem listItem = itemSINDropDownList.Items.FindByValue( itemSIN );
                if( listItem != null )
                    listItem.Selected = true;
            }
        }

        protected void packageAsPricedDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList packageAsPricedDropDownList = ( DropDownList )sender;
            GridViewRow gridViewRow = ( GridViewRow )packageAsPricedDropDownList.NamingContainer;
            string packageAsPriced = "";

            if( gridViewRow.DataItem != null )
            {
                packageAsPriced = ( ( DataRowView )gridViewRow.DataItem )[ "PackageAsPriced" ].ToString();
                if( packageAsPriced != null )
                {
                    ListItem listItem = packageAsPricedDropDownList.Items.FindByValue( packageAsPriced.ToString() );
                    if( listItem != null )
                        listItem.Selected = true;                  
                }
            }
        }

        protected void ItemsGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;

                // upon first entry after document creation, 
                // e.Row.Cells.Count is 1, make sure count is ok before proceeding
                if( e.Row.Cells.Count > ParentItemDropDownListFieldNumber &&
                    e.Row.Cells.Count > CatalogNumberFieldNumber &&                 
                    e.Row.Cells.Count > ServiceCategoryFieldNumber &&
                    e.Row.Cells.Count > SINFieldNumber &&
                    e.Row.Cells.Count > HasBPAFieldNumber &&
                    e.Row.Cells.Count > ParentItemIdFieldNumber )
                {
                    // not BPA
                    if( ShowParentItemCombo() == false )
                    {
                        // hide ParentItem drop down list
             //           e.Row.Cells[  ParentItemDropDownListFieldNumber ].Visible = false;
                    }

                    if( _startupParameters != null )
                    {
                        if( _startupParameters.IsService == true )
                        {
                            // hide CatalogNumber
                            e.Row.Cells[ CatalogNumberFieldNumber ].Visible = false;      

                            // hide SIN
                            e.Row.Cells[ SINFieldNumber ].Visible = false;

                            // hide packaging
                            e.Row.Cells[ PackageAsPricedFieldNumber ].Visible = false;
                        }
                        else
                        {
                            // hide service category
                            e.Row.Cells[ ServiceCategoryFieldNumber ].Visible = false;
                        }

                        if( _startupParameters.IsNational == true )
                        {
                            // hide SIN
                            e.Row.Cells[ SINFieldNumber ].Visible = false;

                            // hide hasBPA indicator
                            e.Row.Cells[ HasBPAFieldNumber ].Visible = false;
                        }

                        if( _startupParameters.IsBPA == true )
                        {
                            // hide SIN
                            e.Row.Cells[ SINFieldNumber ].Visible = false;

                            // hide hasBPA indicator
                            e.Row.Cells[ HasBPAFieldNumber ].Visible = false;
                        }
                    }

                    // viewing historical items
                    if( IsHistorical() == true )
                    {                        
                        // no active price to display with historical items
                        e.Row.Cells[ CurrentPriceFieldNumber ].Visible = false;
                        e.Row.Cells[ ItemPriceStartDateFieldNumber ].Visible = false;
                        e.Row.Cells[ ItemPriceEndDateFieldNumber ].Visible = false;
                        e.Row.Cells[ ItemLastModifiedByFieldNumber ].Visible = false;

                        e.Row.Cells[ ReasonMovedToHistoryFieldNumber ].Visible = true;
                        e.Row.Cells[ MovedToHistoryByFieldNumber ].Visible = true;
                        e.Row.Cells[ DateMovedToHistoryFieldNumber ].Visible = true;
                    }
                    else
                    {
                        e.Row.Cells[ ReasonMovedToHistoryFieldNumber ].Visible = false;
                        e.Row.Cells[ MovedToHistoryByFieldNumber ].Visible = false;
                        e.Row.Cells[ DateMovedToHistoryFieldNumber ].Visible = false;
                    }

                    // hide id fields
                    e.Row.Cells[ ParentItemIdFieldNumber ].Visible = false;
                    e.Row.Cells[ ServiceCategoryIdFieldNumber ].Visible = false;
                    e.Row.Cells[ ServiceCategorySINFieldNumber ].Visible = false;
                    e.Row.Cells[ ParentActiveFieldNumber ].Visible = false;
                    e.Row.Cells[ ParentHistoricalFieldNumber ].Visible = false;
                    e.Row.Cells[ ItemHistoryIdFieldNumber ].Visible = false;
                    e.Row.Cells[ RestorableFieldNumber ].Visible = false;
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        //// called on row creation
        //private void DisableItemFieldsForDocumentTypeForRow( GridViewRow itemGridViewRow )
        //{
        //    if( itemGridViewRow.RowType == DataControlRowType.DataRow )
        //    {
        //        if(( itemGridViewRow.RowState & DataControlRowState.Edit ) == DataControlRowState.Edit )
        //        {
        //            if( _startupParameters.IsBPA == true )
        //            {

        //            }
        //            else if( _startupParameters.IsService == true )
        //            {


        //            }
        //            itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( CatalogNumberFieldNumber ) ].Enabled = false;
                 
        //            itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ItemDescriptionFieldNumber ) ].Enabled = false;
                
        //            itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( PackageAbbreviationFieldNumber ) ].Enabled = false;
        //        }
        //    }
        //}

        // this is not really used $$$
        private bool ShowParentItemCombo()
        {
            if( _startupParameters == null )
                return ( true ); // masking an error that happens when onrowcreated is called prior to load

            if( ItemsGridView.EditIndex >= 0 && _startupParameters.IsBPA == true )
                return ( true );
            else
                return ( false );
        }

        protected void CatalogNumberLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label catalogNumberLabel = ( Label )sender;
            GridViewRow gridViewRow = ( GridViewRow )catalogNumberLabel.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string catalogNumber = "";

                if( _startupParameters.IsBPA == false )
                {
                    catalogNumber = ( ( DataRowView )gridViewRow.DataItem )[ "CatalogNumber" ].ToString();
                }
                else
                {
                    catalogNumber = ( ( DataRowView )gridViewRow.DataItem )[ "ParentCatalogNumber" ].ToString();
                }

                catalogNumberLabel.Text = catalogNumber;  
            }

        }

        //protected void ManufacturersPartNumberLabel_OnDataBinding( object sender, EventArgs e )
        //{
        //    Label manufacturersPartNumberLabel = ( Label )sender;
        //    GridViewRow gridViewRow = ( GridViewRow )manufacturersPartNumberLabel.NamingContainer;

        //    if( gridViewRow.DataItem != null )
        //    {
        //        string manufacturersPartNumber = "";

        //        if( _startupParameters.IsBPA == false )
        //        {
        //            manufacturersPartNumber = ( ( DataRowView )gridViewRow.DataItem )[ "ManufacturersPartNumber" ].ToString();
        //        }
        //        else
        //        {
        //            manufacturersPartNumber = ( ( DataRowView )gridViewRow.DataItem )[ "ParentManufacturersPartNumber" ].ToString();
        //        }

        //        manufacturersPartNumberLabel.Text = manufacturersPartNumber;  
        //    }

        //}

        protected void CatalogNumberTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox catalogNumberTextBox = ( TextBox )sender;
            GridViewRow gridViewRow = ( GridViewRow )catalogNumberTextBox.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string catalogNumber = "";

                if( _startupParameters.IsBPA == false )
                {
                    catalogNumber = ( ( DataRowView )gridViewRow.DataItem )[ "CatalogNumber" ].ToString();
                }
                else
                {
                    catalogNumber = ( ( DataRowView )gridViewRow.DataItem )[ "ParentCatalogNumber" ].ToString();
                    catalogNumberTextBox.ReadOnly = true;
                }

                catalogNumberTextBox.Text = catalogNumber;
            }


        }

        protected void ItemDescriptionLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label itemDescriptionLabel = ( Label )sender;
            GridViewRow gridViewRow = ( GridViewRow )itemDescriptionLabel.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string itemDescription = "";

                if( _startupParameters.IsBPA == false )
                {
                    itemDescription = ( ( DataRowView )gridViewRow.DataItem )[ "ItemDescription" ].ToString();
                }
                else
                {
                    itemDescription = ( ( DataRowView )gridViewRow.DataItem )[ "ParentItemDescription" ].ToString();
                }

                itemDescriptionLabel.Text = itemDescription;
            }

        }

        protected void ItemDescriptionTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox itemDescriptionTextBox = ( TextBox )sender;
            GridViewRow gridViewRow = ( GridViewRow )itemDescriptionTextBox.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string itemDescription = "";

                if( _startupParameters.IsBPA == false )
                {
                    itemDescription = ( ( DataRowView )gridViewRow.DataItem )[ "ItemDescription" ].ToString();
                }
                else
                {
                    itemDescription = ( ( DataRowView )gridViewRow.DataItem )[ "ParentItemDescription" ].ToString();
                    itemDescriptionTextBox.ReadOnly = true;
                }

                itemDescriptionTextBox.Text = itemDescription;
            }
        }

        protected void ItemPriceLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label itemPriceLabel = ( Label )sender;
            GridViewRow gridViewRow = ( GridViewRow )itemPriceLabel.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string itemPrice = ( ( DataRowView )gridViewRow.DataItem )[ "CurrentPrice" ].ToString();

                itemPriceLabel.Text = itemPrice;  // "{0:c}"
            }
        }

        // Text='<%# DataBinder.Eval( Container.DataItem, "PriceStartDate", "{0:d}" )%>' >
        protected void ItemPriceStartDateLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label itemPriceStartDateLabel = ( Label )sender;
            GridViewRow gridViewRow = ( GridViewRow )itemPriceStartDateLabel.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string itemPriceStartDate = ( ( DataRowView )gridViewRow.DataItem )[ "PriceStartDate" ].ToString();

                itemPriceStartDateLabel.Text = itemPriceStartDate;  // "{0:d}"
            }
        }

        // Text='<%# DataBinder.Eval( Container.DataItem, "PriceStopDate", "{0:d}" )%>'
        protected void ItemPriceEndDateLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label itemPriceEndDateLabel = ( Label )sender;
            GridViewRow gridViewRow = ( GridViewRow )itemPriceEndDateLabel.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string itemPriceEndDate = ( ( DataRowView )gridViewRow.DataItem )[ "PriceStopDate" ].ToString();

                itemPriceEndDateLabel.Text = itemPriceEndDate;  // "{0:d}"
            }
        }

        protected void HasBPALabel_OnDataBinding( object sender, EventArgs e ) 
        {
            Label hasBPALabel = ( Label )sender;
            GridViewRow gridViewRow = ( GridViewRow )hasBPALabel.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string hasBPA = ( ( DataRowView )gridViewRow.DataItem )[ "HasBPA" ].ToString();
                if( hasBPA.CompareTo( "1" ) == 0 )
                    hasBPALabel.Text = "Yes";
                else
                    hasBPALabel.Text = "No";
            }
        }

#endregion ItemGridBinding

#region CommandsAndClicks

        protected void Save_ButtonClick( object sender, EventArgs e )
        {

            //AddClientCloseEvent()
        }

        protected void ItemsGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            int i = 1;

        }

        protected void ItemsGridView_PagerCommand( Pager thePager, PagerCommandEventArgs args )
        {
            string commandName = args.CommandName;
            bool bItemGridWasRefreshed = false;
            
       //     Thread.Sleep( 4000 );

            if( commandName.CompareTo( Pager.CurrentPageUpdateCommand ) == 0 )
            {
                if( args.TotalRowsInDataSet >= 0 )
                {
                    SetPagingParameters( thePager.CurrentStartingRow, args.RowsPerPage );   //tried and reverted thePager.CurrentStartingRow to PageToRowNumber 3/31/22 $$$

                    if( ItemsGridView.DataSource != null )
                    {
                        
                        ItemsGridView.DataBind();

                        // allow the update postback to occur
                        PageGridViewUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

                        bItemGridWasRefreshed = true;

                        thePager.RefreshStatus();
                        thePager.RefreshHiddenValues();
                    }
                }
            }
            else if( commandName.CompareTo( Pager.RowsPerPageUpdateCommand ) == 0 )
            {
                if( args.TotalRowsInDataSet >= 0 )
                {
                    SetPagingParameters( thePager.CurrentStartingRow, args.RowsPerPage );

                    if( ItemsGridView.DataSource != null )
                    {
                        ItemsGridView.DataBind();

                        // allow the update postback to occur
                        PageGridViewUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

                        bItemGridWasRefreshed = true;

                        thePager.RefreshStatus();
                        thePager.RefreshHiddenValues();
                    }
                }
            }

            if( bItemGridWasRefreshed == true )
            {
                GetAverageItemGridViewRowHeight();

                HighlightItemRow( 0 );

                RefreshPricelist( 0 );

                HideProgressIndicator();
            }
        }

        protected void PageToInsertedRow( int lastInsertedItemRowNumber )
        {
            Pager thePager = ItemsGridView.GetPager();

            thePager.PageToRowNumber = lastInsertedItemRowNumber;

            // simulate a page change event
            PagerCommandEventArgs args = new PagerCommandEventArgs( Pager.CurrentPageUpdateCommand, thePager.CurrentPage, thePager.RowsPerPage, thePager.TotalRowsForPaging );
            ItemsGridView_PagerCommand( thePager, args );
        }

        protected void ItemsGridView_ContextMenuItemCommand( ContextMenu theMenu, ContextMenuCommandEventArgs args )
        {
            string commandName = args.CommandName;
            int itemIndex = args.GridViewRowId;
            int itemId = ItemsGridView.GetRowIdFromSelectedIndex( itemIndex, 0 );


            // for med/surg only historical items may be restored ( no discontinuation date as in pharm )
            if( commandName.CompareTo( "RestoreHistoricalItemCommand" ) == 0 ) 
            {

                bool bIsRestorable = false;
               
                string bitRestorable = ItemsGridView.GetStringValueFromSelectedControl( itemIndex, RestorableFieldNumber, 0, false, "RestorableLabel" );
                if( bitRestorable.CompareTo( "1" ) == 0 )
                    bIsRestorable = true;

                if( bIsRestorable == false )
                {
                    MsgBox.AlertFromUpdatePanel( ItemsGridView, "Only deleted or expired items may be restored. The item selected represents an update to an existing item. To affect a change, edit the existing, active item." );
                }
                else
                {
                    int modificationStatusId = -1;

                    bool bContinueWithRestore = false;
                    bContinueWithRestore = GetConfirmationMessageResults();

                    if( bContinueWithRestore == true )
                    {
                        int itemHistoryId = -1;
                        int.TryParse( ItemsGridView.GetStringValueFromSelectedControl( itemIndex, ItemHistoryIdFieldNumber, 0, false, "ItemHistoryIdLabel" ), out itemHistoryId );

                        int newRowIndex = RestoreRemovedItem( ItemsGridView, itemIndex, itemHistoryId, itemId, _startupParameters.ContractId, modificationStatusId );

                        GetAverageItemGridViewRowHeight();
                        HighlightItemRow( newRowIndex );
                        RefreshMainHeader( true );
                        RefreshPricelist( newRowIndex );
                    }

                    // allow the update postback to occur
                    InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                }
            }           
        }

        protected void ItemsGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedItemId = -1;
            int itemIndex = -1;

            if( e.CommandName.CompareTo( "OpenItemDetailsForItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                if( commandArgs[ 0 ].ToString().Length > 0 )
                {
                    itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );

                    if( commandArgs[ 1 ].ToString().Length > 0 )
                    {
                        selectedItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                        HighlightItemRow( itemIndex );

                        // refresh the price list
                        RefreshPricelist( itemIndex );

                        OpenItemDetailsWindow( itemIndex, selectedItemId, _startupParameters.ContractNumber, _startupParameters.ContractId );
                    }
                }
            }
            if( e.CommandName.CompareTo( "RemoveItemAndItemPrices" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                bool bContinueWithDelete = false;
                bContinueWithDelete = GetConfirmationMessageResults();

                if( bContinueWithDelete == true )
                {
                    int newRowIndex = DeleteItem( ItemsGridView, itemIndex, selectedItemId );

                    GetAverageItemGridViewRowHeight();
                    HighlightItemRow( newRowIndex );
                    RefreshMainHeader( true );
                    RefreshPricelist( newRowIndex );
                }
            }
           
            else if( e.CommandName.CompareTo( "RefreshPriceList" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                GetAverageItemGridViewRowHeight();
                HighlightItemRow( itemIndex );

                // refresh the price list
                RefreshPricelist( itemIndex );
            }
            else if( e.CommandName.CompareTo( "EditItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

          //      int revisedItemIndex = ClearSearchWithBind( selectedItemId );  // $$$ removed 3/18/22 for debug

                GetAverageItemGridViewRowHeight();

                HighlightItemRow( itemIndex );

                RefreshPricelist( itemIndex );

                InitiateEditModeForItem( itemIndex );

            }
            // save update or save insert
            else if( e.CommandName.CompareTo( "SaveItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                string validationMessage = "";

                // validate the item before saving
                bool bIsItemOk = ValidateItemBeforeUpdate( ItemsGridView, itemIndex, selectedItemId, ref validationMessage );

                if( bIsItemOk == true )
                {
                    // is this an insert or an update
                    int newOrUpdatedRowIndex = -1;
                    if( ItemsGridView.InsertRowActive == true )
                    {
                        newOrUpdatedRowIndex = InsertItem( ItemsGridView, itemIndex );
                    }
                    else
                    {
                        newOrUpdatedRowIndex = UpdateItem( ItemsGridView, itemIndex );
                    }

                    GetAverageItemGridViewRowHeight();

                    RefreshMainHeader( true );                    
                    // allow the header postback to occur 
                    InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

                    HighlightItemRow( newOrUpdatedRowIndex );                    
                    RefreshPricelist( newOrUpdatedRowIndex );
                }
                else
                {
                    MsgBox.AlertFromUpdatePanel( Page, validationMessage );
                }

                HideProgressIndicator();
            }
            // cancel update or cancel insert
            else if( e.CommandName.CompareTo( "Cancel" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

            }
            // can trap editing commands here if needed, prior to individual editing events
        }

        private bool GetConfirmationMessageResults()
        {
            bool bConfirmationResults = false;
            string confirmationResultsString = "";

            HtmlInputHidden confirmationMessageResultsHiddenField = ( HtmlInputHidden )medSurgItemForm.FindControl( "confirmationMessageResults" );

            if( confirmationMessageResultsHiddenField != null )
            {
                confirmationResultsString = confirmationMessageResultsHiddenField.Value;
                if( confirmationResultsString.Contains( "true" ) == true )
                {
                    bConfirmationResults = true;
                    confirmationMessageResultsHiddenField.Value = "false";
                }
            }

            return ( bConfirmationResults );
        }

        private string GetPromptMessageResults()
        {
            string promptResultsString = "";

            HtmlInputHidden promptMessageResultsHiddenField = ( HtmlInputHidden )medSurgItemForm.FindControl( "promptMessageResults" );

            if( promptMessageResultsHiddenField != null )
            {
                promptResultsString = promptMessageResultsHiddenField.Value;
                promptMessageResultsHiddenField.Value = "";
            }

            return ( promptResultsString );
        }

        // copy value from hidden var into session prior to use
        private void GetAverageItemGridViewRowHeight()
        {
            string averageValue = "";
            decimal averageRowHeight = 0;

            HtmlInputHidden avgRowHeightHiddenField = ( HtmlInputHidden )medSurgItemForm.FindControl( "avgRowHeight" );

            if( avgRowHeightHiddenField != null )
            {
                averageValue = avgRowHeightHiddenField.Value;
            }

            if( decimal.TryParse( averageValue, out averageRowHeight ) == false )
            {
                averageRowHeight = ITEMGRIDVIEWROWHEIGHTESTIMATE;
            }

            Session[ "AverageItemRowHeight" ] = averageRowHeight.ToString();
          
        }

        protected void AddNewItemButton_OnClick( object sender, EventArgs e )
        {
 //           ClearSearch();   trying add without clearing search 4/1/22 $$$

            ItemsGridView.Insert();

            _withAddParameter.DefaultValue = "true";

            try
            {
                ItemsGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            InitiateEditModeForItem( 0 );

            // allow the update postback to occur
            InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            GetAverageItemGridViewRowHeight();

            HighlightItemRow( 0 );

            RefreshPricelist( 0 );
        }

        protected void ParentItemDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList parentItemDropDownList = ( DropDownList )sender;

            GridViewRow gridViewRow = ( GridViewRow )parentItemDropDownList.NamingContainer;
            int parentItemId = -1;

            ListItem selectedItem = parentItemDropDownList.SelectedItem;
            parentItemId = int.Parse( selectedItem.Value );

            if( parentItemId != -1 )
            {
                ItemDB itemDB = ( ItemDB )Session[ "ItemDB" ];
                if( itemDB != null )
                {
                    DataSet medSurgItemDetailsDataSet = null;
                    try
                    {
                        bool bSuccess = itemDB.GetParentItemDetails( ref medSurgItemDetailsDataSet, _startupParameters.ContractNumber, parentItemId );
                        if( bSuccess == false )
                        {
                            throw new Exception( itemDB.ErrorMessage );
                        }
                        else
                        {
                            // show the data
                            int count = itemDB.RowsReturned;

                            if( count == 1 )
                            {
                                DataRow currentRow = medSurgItemDetailsDataSet.Tables[ ItemDB.MedSurgItemDetailsTableName ].Rows[ 0 ];
                                ( ( Label )gridViewRow.Cells[ CatalogNumberFieldNumber ].Controls[ 3 ] ).Text = currentRow[ "CatalogNumber" ].ToString();
                                //           ( ( TextBox )gridViewRow.Cells[ ItemDescriptionFieldNumber ].Controls[ 1 ] ).Text = currentRow[ "ItemDescription" ].ToString();  // $$$ changing on 3/10/22
                                ( ( Label )gridViewRow.Cells[ ItemDescriptionFieldNumber ].Controls[ 3 ] ).Text = currentRow[ "ItemDescription" ].ToString();  // $$$ added on 3/10/22
                                ( ( Label )gridViewRow.Cells[ ParentItemIdFieldNumber ].Controls[ 1 ] ).Text = parentItemId.ToString();
                                ( ( Label )gridViewRow.Cells[ ParentActiveFieldNumber ].Controls[ 1 ] ).Text = currentRow[ "ParentActive" ].ToString();
                                ( ( Label )gridViewRow.Cells[ ParentHistoricalFieldNumber ].Controls[ 1 ] ).Text = currentRow[ "ParentHistorical" ].ToString();
                                ( ( Label )gridViewRow.Cells[ ItemHistoryIdFieldNumber ].Controls[ 1 ] ).Text = currentRow[ "ItemHistoryId" ].ToString();
                                ( ( Label )gridViewRow.Cells[ RestorableFieldNumber ].Controls[ 1 ] ).Text = currentRow[ "Restorable" ].ToString();  

                                // allow the update postback to occur
                                SelectParentItemForBPAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                            }
                        }
                    }
                    catch( Exception ex )
                    {
                        MsgBox.ShowErrorFromUpdatePanel( Page, ex );
                    }
                }
            }
            else  // the user has selected the "none selected" row
            {
                // $$$ ok as long as it doesn't blank out upon entering edit mode
                ( ( Label )gridViewRow.Cells[ CatalogNumberFieldNumber ].Controls[ 3 ] ).Text = "";
           //     ( ( TextBox )gridViewRow.Cells[ ItemDescriptionFieldNumber ].Controls[ 1 ] ).Text = "";
                ( ( Label )gridViewRow.Cells[ ItemDescriptionFieldNumber ].Controls[ 3 ] ).Text = "";

                // allow the update postback to occur
                SelectParentItemForBPAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        protected void ServiceCategoryDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList serviceCategoryDropDownList = ( DropDownList )sender;

            GridViewRow gridViewRow = ( GridViewRow )serviceCategoryDropDownList.NamingContainer;
            int serviceCategoryId = -1;

            ListItem selectedItem = serviceCategoryDropDownList.SelectedItem;
            serviceCategoryId = int.Parse( selectedItem.Value );
            bool bSuccess = false;

            if( serviceCategoryId != -1 )
            {
                ItemDB itemDB = ( ItemDB )Session[ "ItemDB" ];
                if( itemDB != null )
                {
                    DataSet serviceCategoryDetailsDataSet = null;
                    try
                    {
                        bSuccess = itemDB.GetServiceCategoryDetails( ref serviceCategoryDetailsDataSet, _startupParameters.ContractNumber, _startupParameters.ContractId, serviceCategoryId );
                        if( bSuccess == false )
                        {
                            throw new Exception( itemDB.ErrorMessage );
                        }
                        else
                        {
                            // show the data
                            int count = itemDB.RowsReturned;

                            if( count == 1 )
                            {
                                DataRow currentRow = serviceCategoryDetailsDataSet.Tables[ ItemDB.ServiceCategoryDetailsTableName ].Rows[ 0 ];

                                //string itemDescription = string.Format( "{0} : {1}", currentRow[ "SIN" ].ToString(), currentRow[ "ServiceCategoryDescription" ].ToString() );
                                string itemDescription = string.Format( "{0}", currentRow[ "ServiceCategoryDescription" ].ToString() );

                                ( ( TextBox )gridViewRow.Cells[ ItemDescriptionFieldNumber ].Controls[ 1 ] ).Text = itemDescription;
                                ( ( Label )gridViewRow.Cells[ ServiceCategoryIdFieldNumber ].Controls[ 1 ] ).Text = serviceCategoryId.ToString();
                                ( ( Label )gridViewRow.Cells[ ServiceCategorySINFieldNumber ].Controls[ 1 ] ).Text = currentRow[ "SIN" ].ToString();

                                // allow the update postback to occur
                                SelectServiceCategoryUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                            }
                        }
                    }
                    catch( Exception ex )
                    {
                        MsgBox.ShowErrorFromUpdatePanel( Page, ex );
                    }
                }
            }
            else  // the user has selected the "none selected" row
            {
                ( ( TextBox )gridViewRow.Cells[ ItemDescriptionFieldNumber ].Controls[ 1 ] ).Text = "";
                ( ( Label )gridViewRow.Cells[ ServiceCategoryIdFieldNumber ].Controls[ 1 ] ).Text = "-1";
                ( ( Label )gridViewRow.Cells[ ServiceCategorySINFieldNumber ].Controls[ 1 ] ).Text = "";

                // allow the update postback to occur
                SelectServiceCategoryUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        protected void ItemSINDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList itemSINDropDownList = ( DropDownList )sender;

            GridViewRow gridViewRow = ( GridViewRow )itemSINDropDownList.NamingContainer;
            string itemSIN = "";

            ListItem selectedItem = itemSINDropDownList.SelectedItem;
            itemSIN = selectedItem.Value.ToString();

            if( itemSIN.Length > 0 )
            {
                // $$$
            }
        }

        protected void packageAsPricedDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList packageAsPricedDropDownList = ( DropDownList )sender;

            GridViewRow gridViewRow = ( GridViewRow )packageAsPricedDropDownList.NamingContainer;
            string packageAbbreviation = "";

            ListItem selectedItem = packageAsPricedDropDownList.SelectedItem;
            packageAbbreviation = selectedItem.Value.ToString();

            if( packageAbbreviation.Length > 0 )
            {
                // $$$
            }
        }

#endregion CommandsAndClicks

#region ItemInsertEditCancelFunctionsAndEvents


        protected void ItemsGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            //      ClearSearch();   trying add without clearing search 4/1/22 $$$
            InitiateEditModeForItem( e.NewEditIndex );
        }

        private void InitiateEditModeForItem( int editIndex )
        {
            ItemsGridView.EditIndex = editIndex;

            // select the edited item also
            if( ItemsGridView.InsertRowActive == true )
            {
                SetItemsGridViewSelectedItem( editIndex, true );  // scroll to new row
            }
            else
            {
                bool bIsInSearchMode = false;
                if( Session[ "IsInSearchMode" ] != null )
                {
                    bIsInSearchMode = ( bool )Session[ "IsInSearchMode" ];
                }

                if( bIsInSearchMode == true )
                {
                    SetItemsGridViewSelectedItem( editIndex, true ); // scroll to new row required
          //          Session[ "IsInSearchMode" ] = false;    took this out 4/1/2022 $$$
                }
                else
                {
                    SetItemsGridViewSelectedItem( editIndex, false );
                }
            }

            ItemsGridView.DataBind();

            // disable appropriate buttons for the selected row
            SetEnabledItemControlsDuringEdit( ItemsGridView, editIndex, false );

            // disable the search and other non-edit controls before going into edit mode
            EnableControlsForItemEditMode( false );
        }

        protected void ItemsGridView_RowInserting( object sender,  GridViewInsertEventArgs e )
        {
      //      InitiateEditMode( e.Row.RowIndex );

        }

        private void SetEnabledItemControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )
        {
            gv.SetEnabledControlsForCell( rowIndex, 0, bEnabled ); // refresh
            gv.SetEnabledControlsForCell( rowIndex, ItemDetailsButtonFieldNumber, bEnabled ); // details
            gv.SetEnabledControlsForCell( rowIndex, RemoveItemAndPricesButtonFieldNumber, bEnabled ); // remove //$$$+

            gv.SetVisibleControlsForCell( rowIndex, 2, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 2, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 2, "CancelButton", !bEnabled );

            // if bpa, then edit mode displays a label instead of a textbox
            if( _startupParameters.IsBPA == true )
            {
                gv.SetVisibleControlsForCellInTemplateField( rowIndex, "catalogNumberTextBox", bEnabled );
           
           //     gv.SetVisibleControlsForCellInTemplateField( rowIndex, "manufacturersPartNumberTextBox", bEnabled );
                gv.SetVisibleControlsForCellInTemplateField( rowIndex, "itemDescriptionTextBox", bEnabled ); 
            }

            // if service or national, then details screen fields do not apply
            if( _startupParameters.IsService == true || _startupParameters.IsNational == true )
            {
                gv.SetEnabledControlsForCell( rowIndex, ItemDetailsButtonFieldNumber, false ); // details
            }
        }

                
        private int ApplyRightHandColumnOffsetBasedOnDocumentType( int columnNumber )
        {
            int retVal = columnNumber;
            // BPA
            if( ShowParentItemCombo() == true )
            {
                switch( columnNumber )
                {
                      case 0:
                        retVal = 0;
                        break;
                      case 1:
                        retVal = 1;
                        break;
                      case 2:
                        retVal = 2;
                        break;     
                      case 5:
                        retVal = 3;
                        break;
                      case 7:
                        retVal = 4;
                        break;
                      case 8:
                        retVal = 5;
                        break;
                      case 9:
                        retVal = 6;
                        break;
                      case 10:
                        retVal = 7;
                        break;                     
                      case 12:
                        retVal = 8;
                        break;
                      case 13:
                        retVal = 9;
                        break;
                      case 14:
                        retVal = 10;
                        break;
                      case 15:
                        retVal = 11;
                        break;     
                }

            }
            else if( _startupParameters.IsNational == true )
            {
                switch( columnNumber )
                {
                      case 0:
                        retVal = 0;
                        break;
                      case 1:
                        retVal = 1;
                        break;
                      case 4:
                        retVal = 2;
                        break;       
                      case 5:
                        retVal = 3;
                        break;
                      case 7:
                        retVal = 4;
                        break;
                      case 8:
                        retVal = 5;
                        break;
                      case 9:
                        retVal = 6;
                        break;
                      case 10:
                        retVal = 7;
                        break;                     
                      case 12:
                        retVal = 8;
                        break;
                      case 13:
                        retVal = 9;
                        break;
                      case 14:
                        retVal = 10;
                        break;
                      case 15:
                        retVal = 11;
                        break;                                 
                }

            }
            else if( _startupParameters.IsService == true )
            {
                switch( columnNumber )
                {
                      case 0:
                        retVal = 0;
                        break;
                      case 1:
                        retVal = 1;
                        break;
                      case 3:
                        retVal = 2;
                        break;                    
                      case 5:
                        retVal = 3;
                        break;
                      case 8:
                        retVal = 4;
                        break;
                      case 9:
                        retVal = 5;
                        break;
                      case 10:
                        retVal = 6;
                        break;
                      case 11:
                        retVal = 7;
                        break;
                      case 12:
                        retVal = 8;
                        break;
                      case 13:
                        retVal = 9;
                        break;
                      case 14:
                        retVal = 10;
                        break;
                      case 15:
                        retVal = 11;
                        break;             
                }

            }
            else // fss
            {
                switch( columnNumber )
                {
                      case 0:
                        retVal = 0;
                        break;
                      case 1:
                        retVal = 1;
                        break;
                      case 4:
                        retVal = 2;
                        break;
                      case 5:
                        retVal = 3;
                        break;
                      case 6:
                        retVal = 4;
                        break;
                      case 7:
                        retVal = 5;
                        break;
                      case 8:
                        retVal = 6;
                        break;
                      case 9:
                        retVal = 7;
                        break;
                      case 10:
                        retVal = 8;
                        break;
                      case 11:
                        retVal = 9;
                        break;
                      case 12:
                        retVal = 10;
                        break;
                      case 13:
                        retVal = 11;
                        break;
                      case 14:
                        retVal = 12;
                        break;
                      case 15:
                        retVal = 13;
                        break;                           
                }
            }

            if( IsHistorical() == true )   
            {
                   switch( columnNumber )
                   {
                      case 14:
                        retVal = retVal + 1;
                        break;
                      case 15:
                        retVal = retVal + 1;
                        break;
                      case 16:
                        retVal = retVal + 1;
                        break;
                   }
            }

            return( retVal );
        }

        private void SetItemsGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {
            // save for postback
            Session[ "ItemsGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            ItemsGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            if( bIncludeScroll == true )
                ScrollToSelectedItem();

            // allow the update postback to occur
            ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedItem()
        {
            int rowIndex = ItemsGridView.SelectedIndex;
            int rowsPerPage = ItemsGridView.RowsPerPage;
            decimal averageRowHeight = 0;
            int rowPosition = 0;

            //  TableItemStyle rowStyle = ItemsGridView.RowStyle;
          //  int rowHeight = ( int )rowStyle.Height.Value;  // this value is always zero
            if( Session[ "AverageItemRowHeight" ] != null )
            {
                averageRowHeight = decimal.Parse( Session[ "AverageItemRowHeight" ].ToString() );
             
                int fudge = ( rowIndex == 0 ) ? 0 : ( int )Math.Floor( ( decimal )rowsPerPage * ( decimal )0.004 );
                rowPosition = ( int )(( averageRowHeight * rowIndex ) - ( averageRowHeight * fudge ));
            }
            else
            {
                averageRowHeight = ITEMGRIDVIEWROWHEIGHTESTIMATE;
                int fudge = ( rowIndex == 0 ) ? 0 : ( int )Math.Floor( ( decimal )rowsPerPage / ( decimal )7.8 );   // for 1000 rowsPerPage 8 wasn't enough and 7 was too much
                rowPosition = ( int )(( averageRowHeight * rowIndex ) + ( averageRowHeight * fudge ));
            }
                     
            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
       //     ScriptManager.RegisterClientScriptBlock( this.Page, this.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true );  // runs prior to control rendering
        }

        protected void HighlightItemRow( int itemIndex )
        {
            string highlightedRowOriginalColor = "";          
            int highlightedRowIndex = itemIndex + 1;

            if( ItemsGridView.HasData() == true )
            {
                GridViewRow row = ItemsGridView.Rows[ itemIndex ];
                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = ItemsGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = ItemsGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setItemHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveItemHighlightingScript", preserveHighlightingScript, true );

                // allow the highlight postback to occur 
                ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        private void RestoreItemsGridViewSelectedItem()
        {
            ItemsGridView.SelectedIndex = ( int )Session[ "ItemsGridViewSelectedIndex" ];
        }


        public void HideProgressIndicator()
        {
            string hideProgressIndicatorScript = "HideProgressIndicator();";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "HideProgressIndicatorScript", hideProgressIndicatorScript, true ); // runs after controls established

            PageGridViewUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

        }

        public void HideProgressIndicator( Page containingPage )
        {
            string hideProgressIndicatorScript = "HideProgressIndicator();";
            ScriptManager.RegisterStartupScript( containingPage, containingPage.GetType(), "HideProgressIndicatorScript", hideProgressIndicatorScript, true ); // runs after controls established

            PageGridViewUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

        }




        protected void ItemsGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            int cancelIndex = e.RowIndex;
            bool bInserting = ItemsGridView.InsertRowActive;

            // if inserting
            if( bInserting == true )
            {
                ItemsGridView.InsertRowActive = false; // cancels insert ( if inserting )
                _withAddParameter.DefaultValue = "false";
                ItemsGridView.EditIndex = -1; // cancels the edit
                ItemsGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledItemControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                // re-enable the search and filter
                EnableControlsForItemEditMode( true );

                HighlightItemRow( 0 );
                RefreshPricelist( 0 ); // revert to item zero's pricelist
            }
            else // editing existing row
            {
                ItemsGridView.EditIndex = -1; // cancels the edit
                ItemsGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledItemControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                // re-enable the search and filter
                EnableControlsForItemEditMode( true );

                HighlightItemRow( cancelIndex );
                RefreshPricelist( cancelIndex ); // revert to same item 
            }
        }

        protected void ItemsGridView_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {

        }



        // command name changed to saveedit so this not firing anymore
        protected void ItemsGridView_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {
            int i = 0;
        }

#endregion ItemInsertEditCancelFunctionsAndEvents

#region ItemDatabaseCommandsAndEvents

        private int UpdateItem( GridView gv, int rowIndex )
        {
            int updatedRowIndex = -1;
            bool bSuccess = true;

            _itemIdForItemsParameter.DefaultValue = gv.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

            // parent item id
            if( _startupParameters.IsBPA == true )
            {
                _parentItemIdParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ParentItemDropDownListFieldNumber, 0, false, "parentItemDropDownList" );
            }
            // else null

            // catalog number and description
            if( _startupParameters.IsBPA == true )
            {
                _catalogNumberParameter.DefaultValue = "Reference parent part number";
                _itemDescriptionParameter.DefaultValue = "Reference parent item description";
            }
            else if( _startupParameters.IsService == true )
            {
                _catalogNumberParameter.DefaultValue = "Services";
                
                string itemDescription = gv.GetStringValueFromSelectedControl( rowIndex, ItemDescriptionFieldNumber, 0, false, "itemDescriptionTextBox" );
                _itemDescriptionParameter.DefaultValue = CMGlobals.ReplaceNonPrintableCharacters( itemDescription ).Trim();
            }
            else // FSS or National
            {
                string catalogNumber = gv.GetStringValueFromSelectedControl( rowIndex, CatalogNumberFieldNumber, 0, false, "catalogNumberTextBox" );
                _catalogNumberParameter.DefaultValue = CMGlobals.ReplaceNonPrintableCharacters( catalogNumber ).Trim();
            
                string itemDescription = gv.GetStringValueFromSelectedControl( rowIndex, ItemDescriptionFieldNumber, 0, false, "itemDescriptionTextBox" );
                _itemDescriptionParameter.DefaultValue = CMGlobals.ReplaceNonPrintableCharacters( itemDescription ).Trim();
            }

            // SIN
            if( _startupParameters.IsNational == true || _startupParameters.IsBPA == true )
            {
                // NC or BPA SINs get defaulted based on schedule
                _SINParameter.DefaultValue = _startupParameters.DefaultSIN;
            }
            // service SIN comes from category
            else if( _startupParameters.IsService == true )
            {
                _SINParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ServiceCategorySINFieldNumber, 0, false, "ServiceCategorySINLabel" );
            }
            else  // FSS item
            {
                _SINParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, SINFieldNumber, 0, false, "ItemSINDropDownList" );
            }

            // Packaging
            if( _startupParameters.IsBPA == true )
            {
                _packageAsPricedParameter.DefaultValue = "BP";   // refer to parent for actual packaging
            }
            else if( _startupParameters.IsService == true )
            {
                _packageAsPricedParameter.DefaultValue = "HR";  // hourly rate is default for service item measure
            }
            else  // FSS or National packaging is specified
            {
                _packageAsPricedParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, PackageAsPricedFieldNumber, 0, false, "packageAsPricedDropDownList" );
            }

            if( _startupParameters.IsService == true )
            {
                string serviceCategoryIdString = gv.GetStringValueFromSelectedControl( rowIndex, ServiceCategoryIdFieldNumber, 0, false, "ServiceCategoryIdLabel" );
                _serviceCategoryIdParameter.DefaultValue = serviceCategoryIdString;
            }
          
            try
            {
                _medSurgItemsDataSource.Update();
            }
            catch( Exception ex )
            {
               bSuccess = false;
               MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.EditIndex = -1; // done with edit
   //         SetItemsGridViewSelectedItem( rowIndex );
            gv.DataBind();

            if( bSuccess == true )
            {
                // $$$ changing to match insert on 3/7/2022
                if( Session[ "LastUpdatedItemRowNumber" ] != null )
                {
                    int lastUpdatedItemRowNumber = ( int )Session[ "LastUpdatedItemRowNumber" ];

                    // page down to newly updated item's page
                    PageToInsertedRow( lastUpdatedItemRowNumber );

                    if( Session[ "LastUpdatedItemId" ] != null )
                    {
                        int updatedItemId = ( int )Session[ "LastUpdatedItemId" ];
                        updatedRowIndex = ItemsGridView.GetRowIndexFromId( updatedItemId, 0 );

                        // $$$ if paging, then new row index might not be in the current display -- see insert for additional comments
                        if( updatedRowIndex == -1 )
                            updatedRowIndex = rowIndex;

                        SetItemsGridViewSelectedItem( updatedRowIndex, true );

                        // bind to select
                        gv.DataBind();
                    }
                }

            }
            else
            {
                updatedRowIndex = rowIndex; 
            }

            // enable appropriate buttons for the selected row
            SetEnabledItemControlsDuringEdit( gv, updatedRowIndex, true );

            // re-enable the search and filter
            EnableControlsForItemEditMode( true );

            return ( updatedRowIndex );
        }

        
        private int InsertItem( GridView gv, int rowIndex )
        {
            int insertedRowIndex = 0;

            // item description
            if( _startupParameters.IsBPA == true )
            {
                _itemDescriptionParameter.DefaultValue = "Reference parent item description";
            }
            else
            {
                string itemDescription = gv.GetStringValueFromSelectedControl( rowIndex, ItemDescriptionFieldNumber, 0, false, "itemDescriptionTextBox" );
                _itemDescriptionParameter.DefaultValue = CMGlobals.ReplaceNonPrintableCharacters( itemDescription ).Trim();
            }

            // parent item id
            if( _startupParameters.IsBPA == true )
            {
                _parentItemIdParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ParentItemDropDownListFieldNumber, 0, false, "parentItemDropDownList" );
            }
            // else null

            // catalog number
            if( _startupParameters.IsBPA == true )
            {
                _catalogNumberParameter.DefaultValue = "Reference parent part number";               
            }
            else if( _startupParameters.IsService == true )
            {
                _catalogNumberParameter.DefaultValue = "Services";
            }
            else // FSS or National
            {
                string catalogNumber = gv.GetStringValueFromSelectedControl( rowIndex, CatalogNumberFieldNumber, 0, false, "catalogNumberTextBox" );
                _catalogNumberParameter.DefaultValue = CMGlobals.ReplaceNonPrintableCharacters( catalogNumber ).Trim();
            }

            // SIN
            if( _startupParameters.IsNational == true || _startupParameters.IsBPA == true )
            {
                // NC or BPA SINs get defaulted based on schedule
                _SINParameter.DefaultValue = _startupParameters.DefaultSIN;
            }
            // service SIN comes from category
            else if( _startupParameters.IsService == true )
            {
                _SINParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ServiceCategorySINFieldNumber, 0, false, "ServiceCategorySINLabel" );
            }
            else  // FSS item
            {
                _SINParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, SINFieldNumber, 0, false, "ItemSINDropDownList" );
            }

            // Packaging
            if( _startupParameters.IsBPA == true )
            {
                _packageAsPricedParameter.DefaultValue = "BP";   // refer to parent for actual packaging
            }
            else if( _startupParameters.IsService == true )
            {
                _packageAsPricedParameter.DefaultValue = "HR";  // hourly rate is default for service item measure
            }
            else  // FSS or National packaging is specified
            {
                _packageAsPricedParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, PackageAsPricedFieldNumber, 0, false, "packageAsPricedDropDownList" );
            }

            if( _startupParameters.IsService == true )
            {
                string serviceCategoryIdString = gv.GetStringValueFromSelectedControl( rowIndex, ServiceCategoryIdFieldNumber, 0, false, "ServiceCategoryIdLabel" );
                _serviceCategoryIdParameter.DefaultValue = serviceCategoryIdString;
            }

            try
            {
                _medSurgItemsDataSource.Insert();
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

            if( Session[ "LastInsertedItemRowNumber" ] != null )
            {
                int lastInsertedItemRowNumber = ( int )Session[ "LastInsertedItemRowNumber" ];

                // page down to newly inserted item's page
                PageToInsertedRow( lastInsertedItemRowNumber );

                if( Session[ "LastInsertedItemId" ] != null )
                {
                    // scroll to specific item after postback didn't work -- see $$$ in page load
                    // so, if the new item isn't on the current page, just going to scroll to row 0
                    int newItemId = ( int )Session[ "LastInsertedItemId" ];

                    insertedRowIndex = ItemsGridView.GetRowIndexFromId( newItemId, 0 );

                    if( insertedRowIndex < 0 )
                        insertedRowIndex = 0;

                    SetItemsGridViewSelectedItem( insertedRowIndex, true );

                    // bind to select
                    gv.DataBind();
                }
            }

            // enable appropriate buttons for the selected row
            SetEnabledItemControlsDuringEdit( gv, insertedRowIndex, true );

            // re-enable the search and filter
            EnableControlsForItemEditMode( true );

            return ( insertedRowIndex );
        }

        // for insert, exact row may be selected on postback
        protected void SelectRowOnPostbackAfterInsert()
        {                        
            int newItemId = ( int )Session[ "LastInsertedItemId" ];

            int insertedRowIndex = ItemsGridView.GetRowIndexFromId( newItemId, 0 );
           
            // clear the id
            Session[ "LastInsertedItemId" ] = null;

            SetItemsGridViewSelectedItem( insertedRowIndex, true );

            // bind to select
            ItemsGridView.DataBind();        
        }

        public void _medSurgItemsDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@ItemId" ].Value != null )
            {
                string itemIdString = e.Command.Parameters[ "@ItemId" ].Value.ToString();

                if( itemIdString.Length > 0 )
                {
                    int itemId = int.Parse( itemIdString );
                    Session[ "LastInsertedItemId" ] = itemId;
                }
            }
            else
            {
                Exception insertException = e.Exception;
                if( insertException != null )
                    throw new Exception( String.Format( "ItemId returned from insert was null. Insert failed. {0}", insertException.Message ));
                else
                    throw new Exception( "ItemId returned from insert was null. Insert failed." );

            }

            if( e.Command.Parameters[ "@InsertedRowNumber" ].Value != null )
            {
                string lastInsertedItemRowNumberString = e.Command.Parameters[ "@InsertedRowNumber" ].Value.ToString();

                if( lastInsertedItemRowNumberString.Length > 0 )
                {
                    int lastInsertedItemRowNumber = int.Parse( lastInsertedItemRowNumberString );
                    Session[ "LastInsertedItemRowNumber" ] = lastInsertedItemRowNumber;
                }
            }
            else
            {
                Exception insertException = e.Exception;
                if( insertException != null )
                    throw new Exception( String.Format( "InsertedRowNumber returned from insert was null. Insert failed. {0}", insertException.Message ) );
                else
                    throw new Exception( "InsertedRowNumber returned from insert was null. Insert failed." );
            }          
        }

        // probably wont happen - id changing during update
        public void _medSurgItemsDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@ItemId" ].Value != null )
            {
                string itemIdString = e.Command.Parameters[ "@ItemId" ].Value.ToString();

                if( itemIdString.Length > 0 )
                {
                    int itemId = int.Parse( itemIdString );
                    Session[ "LastUpdatedItemId" ] = itemId;
                }
            }
            else
            {
                Exception updateException = e.Exception;
                if( updateException != null )
                    throw new Exception( String.Format( "ItemId returned from update was null. Update failed. {0}", updateException.Message ) );
                else
                    throw new Exception( "ItemId returned from update was null. Update failed." );

            }

            if( e.Command.Parameters[ "@UpdatedRowNumber" ].Value != null )
            {
                string lastUpdatedItemRowNumberString = e.Command.Parameters[ "@UpdatedRowNumber" ].Value.ToString();

                if( lastUpdatedItemRowNumberString.Length > 0 )
                {
                    int lastUpdatedItemRowNumber = int.Parse( lastUpdatedItemRowNumberString );
                    Session[ "LastUpdatedItemRowNumber" ] = lastUpdatedItemRowNumber;
                }
            }
            else
            {
                Exception updateException = e.Exception;
                if( updateException != null )
                    throw new Exception( String.Format( "UpdatedRowNumber returned from update was null. Insert failed. {0}", updateException.Message ) );
                else
                    throw new Exception( "UpdatedRowNumber returned from update was null. Update failed." );
            }
        }

        private int DeleteItem( GridView gv, int rowIndex, int selectedItemId )
        {
            // id of row to delete
            _itemIdForItemsParameter.DefaultValue = selectedItemId.ToString();

            try
            {
                _medSurgItemsDataSource.Delete();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            // previous row gets focus
            if( rowIndex >= 1 )
                rowIndex--;

            SetItemsGridViewSelectedItem( rowIndex, false );

            gv.DataBind();

            return ( rowIndex );
        }

        private int RestoreRemovedItem( GridView gv, int itemIndex, int itemHistoryId, int itemId, int contractId, int modificationStatusId )
        {
            bool bSuccess = false;

            ItemDB itemDB = ( ItemDB )Page.Session[ "ItemDB" ];
            if( itemDB != null )
            {
                bSuccess = itemDB.RestoreHistoricalItem( itemHistoryId, contractId, itemId, modificationStatusId );
                if( bSuccess == true )
                {
                    // currently viewing historical
                    if( IsHistorical() == true )
                    {
                        // previous row gets focus
                        if( itemIndex >= 1 )
                            itemIndex--;
                    }

                    SetItemsGridViewSelectedItem( itemIndex, false );

                    gv.DataBind();
                }
                else
                {
                    MsgBox.ShowErrorFromUpdatePanel( this.Page, new Exception( itemDB.ErrorMessage ));
                }
            }
            return ( itemIndex );
        }

        // could get called as a result of details change or similar item operation
        // rebind is true if caller has altered the item list
        private void RefreshItemScreenDueToDetailsChanged( bool bRebindItems, bool bRefreshItemHeaderCount )
        {
            int currentItemIndex = ItemsGridView.SelectedIndex;
            int adjustedCurrentItemIndex = currentItemIndex;

            if( bRebindItems == true )
            {
                // adjust the current item index $$$
                int offset = 0;
                if( Session[ "ItemRemovalCurrentRowOffset" ] != null )
                {
                    offset = ( int )Session[ "ItemRemovalCurrentRowOffset" ];
                    adjustedCurrentItemIndex += offset;

                    // if removing first item
                    if( adjustedCurrentItemIndex < 0 )
                        adjustedCurrentItemIndex = 0;
                }

                SetItemsGridViewSelectedItem( adjustedCurrentItemIndex, false );

                if( bRefreshItemHeaderCount == true )
                    RefreshMainHeader( true );

                ItemsGridView.DataBind();
            }

            ScrollToSelectedItem();
            HighlightItemRow( adjustedCurrentItemIndex );

        }


#endregion ItemDatabaseCommandsAndEvents

#region ItemFilterAndSearch

        protected void ItemFilterRadioButtonList_OnSelectedIndexChanged( Object sender, EventArgs e )
        {
            // save current state
            bool bCurrentStateIsHistorical = false;
            if( IsHistorical() == true )
            {
                bCurrentStateIsHistorical = true;
            }

            string filterCriteria = ItemFilterRadioButtonList.SelectedValue;
        
            if( filterCriteria.CompareTo( "H" ) == 0 )
            {
                _itemSelectionCriteriaParameter.DefaultValue = "H"; // Historical
                _itemGridContext.IsHistorical = true;               
            }
            else // active
            {
                _itemSelectionCriteriaParameter.DefaultValue = "A"; // Active
                _itemGridContext.IsHistorical = false;
            }

            // if the selected viewing state of historical items is changing
            if( bCurrentStateIsHistorical != IsHistorical() )
            {
                // reset pager and start row 
                SetPagerForItemContextChange();
            }

            DisableControlsForReadOnlyOrDocumentTypeOrItemGridContext();

            BindItemHeaderAndGrid();

            SetItemsGridViewSelectedItem( 0, true );

            HighlightItemRow( 0 );

            RefreshPricelist( 0 );

            // if the selected viewing state of historical items is changing
            if( bCurrentStateIsHistorical != IsHistorical() )
            {
                if( bCurrentStateIsHistorical == true )
                {
                    // changing from historical, so select active prices
                    SimulateActivePriceFilterClick();
                }
                else
                {
                    // changing to historical, so select historical prices
                    SimulateHistoricalPriceFilterClick();
                }
            }
        }

        protected void PrintItemsAndPricesButton_OnClick( Object sender, EventArgs e )
        {
            Report medSurgItemsAndPricesForContractReport = new Report( "/Contracts/Reports/MedSurgItemsForContractReport" );
            string contractIdString = string.Format( "{0}", _startupParameters.ContractId );
            medSurgItemsAndPricesForContractReport.AddParameter( "ContractId", contractIdString );
            medSurgItemsAndPricesForContractReport.AddParameter( "IncludeHistory", false.ToString() );
            medSurgItemsAndPricesForContractReport.AddReportUserLoginIdParameter();

            Session[ "ReportToShow" ] = medSurgItemsAndPricesForContractReport;

            string windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=1,resizable=1,top=200,left=240,width=920,height=800');";
            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "ReportViewerWindowOpenScript", windowOpenScript, true );

        }

        protected void SearchButton_OnClick( Object sender, EventArgs e )
        {
            string searchString = ItemSearchTextBox.Text.Trim();
            if( searchString.Length <= 0 )
                return;

            Session[ "IsInSearchMode" ] = true;

            SetPagerForSearch();
    
            // fields to search
            //CatalogNumber
            //ItemDescription    
            
            //string filterExpression = "";
            //_medSurgItemsDataSource.FilterParameters.Clear();
            //ItemsGridView.EmptyDataText = "There were no matches for the selected search.";
          
            //filterExpression = "CatalogNumber like '%{0}%' OR ItemDescription like '%{1}%'";
            //Parameter catalogNumberParameter = new Parameter( "CatalogNumber", TypeCode.String );
            //Parameter itemDescriptionParameter = new Parameter( "ItemDescription", TypeCode.String );

            //catalogNumberParameter.DefaultValue = searchString;
            //itemDescriptionParameter.DefaultValue = searchString;

            //_medSurgItemsDataSource.FilterExpression = filterExpression;
            //_medSurgItemsDataSource.FilterParameters.Add( catalogNumberParameter );
            //_medSurgItemsDataSource.FilterParameters.Add( itemDescriptionParameter );

            _searchTextParameter.DefaultValue = searchString;
            
            ItemsGridView.DataBind();

            SetItemsGridViewSelectedItem( 0, true );

            RefreshPricelist( 0 );
        }

        protected void ClearSearchButton_OnClick( Object sender, EventArgs e )
        {
            ClearSearch();

            SetPagerForSearch();

            ItemsGridView.DataBind();
            SetItemsGridViewSelectedItem( 0, true );
            Session[ "IsInSearchMode" ] = false;

            RefreshPricelist( 0 );
        }

        private void ClearSearch()
        {
            ItemSearchTextBox.Text = "";
           // _medSurgItemsDataSource.FilterExpression = "";
            _searchTextParameter.DefaultValue = "";
        }

        // returns a revised row index
        private int ClearSearchWithBind( int itemId )
        {
            int revisedRowIndex = 0;
            
            ItemSearchTextBox.Text = "";
            // _medSurgItemsDataSource.FilterExpression = "";
            _searchTextParameter.DefaultValue = "";  // added 3/4/2022

            // SetPagerForSearch();  // removed 3/4/2022

            ItemsGridView.DataBind();

            revisedRowIndex = ItemsGridView.GetRowIndexFromId( itemId, 0 );

            return ( revisedRowIndex );
        }

#endregion ItemFilterAndSearch 


#region ItemEffectOnPrice

        protected void ItemsGridView_OnSelectedIndexChanged( object sender, EventArgs e )
        {

        }

 
        
        private void SetItemPriceDataSourceParameterValues()
        {
            int selectedIndex = ItemsGridView.SelectedIndex;
            int itemId = ItemsGridView.GetRowIdFromSelectedIndex( selectedIndex, 0 );

            _itemIdForPricesParameter.DefaultValue = itemId.ToString();            
        }

        private void SetItemPriceDataSourceParameterValues( int itemIndex )
        {
            int itemId = ItemsGridView.GetRowIdFromSelectedIndex( itemIndex, 0 );

            _itemIdForPricesParameter.DefaultValue = itemId.ToString();         
        }

        private void SetItemPriceHeaderInfo( int selectedIndex )
        {
            if( ItemsGridView.HasData() == true )
            {
                SelectedItemHeader.HeaderTitle = "Prices For Selected Item";
                SelectedItemHeader.ItemDescription = ItemsGridView.GetStringValueFromSelectedControl( selectedIndex, ItemDescriptionFieldNumber, 0, false, "itemDescriptionLabel1" );
                if( _startupParameters.IsService == true )
                {
                    SelectedItemHeader.HideCatalogNumber = true;
                }
                else
                {
                    SelectedItemHeader.HideCatalogNumber = false;
                    SelectedItemHeader.CatalogNumber = ItemsGridView.GetStringValueFromSelectedControl( selectedIndex, CatalogNumberFieldNumber, 0, false, "catalogNumberLabel1" );

                    if( _startupParameters.IsBPA == true )
                    {
                        SelectedItemHeader.CatalogNumberTitle = "Parent Part Number";
                    }
                    else
                    {
                        SelectedItemHeader.CatalogNumberTitle = "Part Number";
                    }
                }                
            }
        }

     

        private void SaveSelectedItemInfoIntoSession( int selectedItemIndex )
        {
            //int itemId = ItemsGridView.GetRowIdFromSelectedIndex( selectedItemIndex, 0 );
         
            //try
            //{
            //    if( Session[ "ItemDB" ] != null )
            //    {
            //        ItemDB itemDB = ( ItemDB )Session[ "ItemDB" ];

            //        if( itemDB != null )
            //        {
                       
            //        }
            //    }
            //}
            //catch( Exception ex )
            //{
            //    MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            //}
      
        }

        private void RefreshPricelist( int itemIndex )
        {
            SetItemPriceDataSourceParameterValues( itemIndex );
            SqlDataSource itemPriceDataSource = ( SqlDataSource )ItemPricesGridView.DataSource;
            itemPriceDataSource.Select( DataSourceSelectArguments.Empty );
            SetItemsGridViewSelectedItem( itemIndex, false );
            
            SaveSelectedItemInfoIntoSession( itemIndex ); 

            ItemPricesGridView.DataBind();

            
            // refresh the header info
            SetItemPriceHeaderInfo( itemIndex );
 
            // allow the update postback to occur
            RefreshPricesButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        #endregion ItemEffectOnPrice

        private void OpenItemDetailsWindow( int itemIndex, int selectedItemId, string contractNumber, int contractId )
        {
            bool bIsEditable = false;

            if( _startupParameters.IsItemDetailsEditable && IsHistorical() == false && _startupParameters.IsExpired == false )
            {
                bIsEditable = true;
            }

            Session[ "ItemDetailsWindowParms" ] = null;
            Session[ "ItemDetailsWindowParms" ] = new ItemDetailsWindowParms( itemIndex, selectedItemId, contractNumber, contractId, bIsEditable, _startupParameters.IsBPA, _startupParameters.IsService );

            string windowOpenScript = "window.open('ItemDetails.aspx','ItemDetails','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=100,left=240,width=884,height=534, resizable=0');";
            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "ItemDetailsWindowOpenScript", windowOpenScript, true );
        }



        //************************** Prices *******************************



        private void CreateItemPricesDataSourceParameters()
        {
            // select
            _futureHistoricalSelectionCriteriaParameter = new Parameter( "FutureHistoricalSelectionCriteria", TypeCode.String );
            _withAddPriceParameter = new Parameter( "WithAddPrice", TypeCode.Boolean );
            _contractExpirationDateParameter = new Parameter( "ContractExpirationDate", TypeCode.DateTime );

            // select and insert
            _itemIdForPricesParameter = new Parameter( "ItemId", TypeCode.Int32 );
 
            // update and delete
            _itemPriceIdParameter = new Parameter( "ItemPriceId", TypeCode.Int32 );

            // update and insert
            _priceStartDateParameter = new Parameter( "PriceStartDate", TypeCode.DateTime );
            _priceEndDateParameter = new Parameter( "PriceEndDate", TypeCode.DateTime );
            _isTemporaryParameter = new Parameter( "IsTemporary", TypeCode.Boolean );
            _priceParameter = new Parameter( "Price", TypeCode.Decimal );
            
            // insert
            _itemPriceIdForPriceInsertParameter = new Parameter( "ItemPriceId", TypeCode.Int32 );
            _itemPriceIdForPriceInsertParameter.Direction = ParameterDirection.Output;
    
        }


        private void RestoreItemPricesDataSourceParameters( DocumentDataSource medSurgItemPricesDataSource )
        {
            // select
            _futureHistoricalSelectionCriteriaParameter = medSurgItemPricesDataSource.SelectParameters[ "FutureHistoricalSelectionCriteria" ];
            _itemIdForPricesParameter = medSurgItemPricesDataSource.SelectParameters[ "ItemId" ];
            _withAddPriceParameter = medSurgItemPricesDataSource.SelectParameters[ "WithAddPrice" ];
            _contractExpirationDateParameter = medSurgItemPricesDataSource.SelectParameters[ "ContractExpirationDate" ];

            // update and delete
            _itemPriceIdParameter = medSurgItemPricesDataSource.UpdateParameters[ "ItemPriceId" ];

            // update
            _priceStartDateParameter = medSurgItemPricesDataSource.UpdateParameters[ "PriceStartDate" ];
            _priceEndDateParameter = medSurgItemPricesDataSource.UpdateParameters[ "PriceEndDate" ];
            _priceParameter = medSurgItemPricesDataSource.UpdateParameters[ "Price" ];
            _isTemporaryParameter = medSurgItemPricesDataSource.UpdateParameters[ "IsTemporary" ];
 
            // insert
            _itemPriceIdForPriceInsertParameter = medSurgItemPricesDataSource.InsertParameters[ "ItemPriceId" ];

        }

 
        protected void ItemPricesGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {

        }

        protected void ItemPricesGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedItemPriceId = -1;
            int selectedItemPriceHistoryId = -1;
            int priceIndex = -1;

            if( e.CommandName.CompareTo( "OpenItemPriceDetailsForItemPrice" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                if( commandArgs[ 0 ].ToString().Length > 0 )
                {
                    priceIndex = Int32.Parse( commandArgs[ 0 ].ToString() );

                    if( commandArgs[ 1 ].ToString().Length > 0 )
                    {
                        selectedItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );
                        selectedItemPriceHistoryId = Int32.Parse( commandArgs[ 2 ].ToString() );

                        HighlightItemPriceRow( priceIndex );

                        SetItemPricesGridViewSelectedItem( priceIndex );

                        // get parent item id information
                        int itemIndex = ItemsGridView.SelectedIndex;

                        // save for postback
                        //   SetItemsGridViewSelectedItem( itemIndex );  
                        Session[ "ItemsGridViewSelectedIndex" ] = itemIndex;

                        int selectedItemId = ItemsGridView.GetRowIdFromSelectedIndex( itemIndex, 0 );
                        string catalogNumber = ItemsGridView.GetStringValueFromSelectedIndexForTemplateField( itemIndex, "catalogNumberLabel1" );
                     //   string manufacturersPartNumber = ItemsGridView.GetStringValueFromSelectedIndexForTemplateField( itemIndex, "manufacturersPartNumberLabel1" );
                        string itemDescription = ItemsGridView.GetStringValueFromSelectedIndexForTemplateField( itemIndex, "itemDescriptionLabel1" );
                        string IsFromHistoryString = ItemPricesGridView.GetStringValueFromSelectedIndexForTemplateField( priceIndex, "IsFromHistoryLabel" );
                        bool bIsFromHistory = ( IsFromHistoryString == "1" ) ? true : false;
                        bool bIsBPA = _startupParameters.IsBPA;
                        bool bIsBOA = _startupParameters.IsBOA;
                        bool bIsNational = _startupParameters.IsNational;
                        DateTime contractExpirationDate = _startupParameters.ContractExpirationDate;
                        bool bIsService = _startupParameters.IsService;

                        OpenPriceDetailsWindow( itemIndex, selectedItemId, catalogNumber, itemDescription, bIsBPA, bIsBOA, bIsNational, priceIndex, selectedItemPriceId, selectedItemPriceHistoryId, bIsFromHistory, contractExpirationDate, bIsService );
                    }
                }
            }
            else if( e.CommandName.CompareTo( "RemovePrice" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                priceIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                bool bContinueWithDelete = false;
                bContinueWithDelete = GetConfirmationMessageResults();

                if( bContinueWithDelete == true )
                {
                    int newRowIndex = DeleteItemPrice( ItemPricesGridView, priceIndex, selectedItemPriceId );

                    HighlightItemPriceRow( newRowIndex );

                    ItemsGridView.DataBind(); // needed to update "summary" price info
                }

                // allow the update postback to occur
                ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else if( e.CommandName.CompareTo( "EditPrice" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                priceIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                HighlightItemPriceRow( priceIndex );

                InitiateEditModeForPrice( priceIndex );
            }
            // save update or save insert
            else if( e.CommandName.CompareTo( "SavePrice" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                priceIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                int selectedItemId = ( int )ItemsGridView.SelectedDataKey.Value;
                string validationMessage = "";
                string warningMessage = "";
                bool bUserCanOverride = false;
                bool bWarningOnly = false;

                // validate the price before saving
                bool bIsPriceOk = ValidatePriceBeforeUpdate( ItemPricesGridView, priceIndex, selectedItemPriceId, selectedItemId, ref bUserCanOverride, ref bWarningOnly, ref validationMessage, ref warningMessage );

                if( bIsPriceOk == true )
                {
                    if( bWarningOnly == true )
                    {
                        MsgBox.AlertFromUpdatePanel( Page, warningMessage );
                    }

                    // is this an insert or an update
                    int newOrUpdatedRowIndex = -1;
                    if( ItemPricesGridView.InsertRowActive == true )
                    {
                        newOrUpdatedRowIndex = InsertItemPrice( ItemPricesGridView, priceIndex, selectedItemId );
                    }
                    else
                    {
                        newOrUpdatedRowIndex = UpdateItemPrice( ItemPricesGridView, priceIndex );
                    }

                    HighlightItemPriceRow( newOrUpdatedRowIndex );

                    ItemsGridView.DataBind(); // needed to update "summary" price info
                    
                    // allow the update postback to occur
                    ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                }
                else
                {   
                    if( bUserCanOverride == true )
                    {

                        MsgBox.ConfirmFromUpdatePanel( Page, "The dates for the new price overlap an existing price of the same type. Proceed anyway?", "ServerConfirmationDialogResults", SavePriceValidationUserOverrideKeyName );
                    }
                    else
                    {
                        MsgBox.AlertFromUpdatePanel( Page, validationMessage );
                    }
                }

            }
            // cancel update or cancel insert
            else if( e.CommandName.CompareTo( "Cancel" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                priceIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );
            }
            // can trap editing commands here if needed, prior to individual editing events
        }


        private void HandleSavePriceValidationUserOverrideResult( MsgBox.ConfirmationResults currentResult )
        {
            int priceIndex = ItemPricesGridView.EditIndex;
            int selectedItemId = ( int )ItemsGridView.SelectedDataKey.Value;

            // user requested save anyway
            if( currentResult == MsgBox.ConfirmationResults.TrueResult )
            {       
                // is this an insert or an update
                int newOrUpdatedRowIndex = -1;
                if( ItemPricesGridView.InsertRowActive == true )
                {

                    newOrUpdatedRowIndex = InsertItemPrice( ItemPricesGridView, priceIndex, selectedItemId );
                }
                else
                {
                    newOrUpdatedRowIndex = UpdateItemPrice( ItemPricesGridView, priceIndex );
                }

                HighlightItemPriceRow( newOrUpdatedRowIndex );
            }
            else // user requested no save, therefore cancel
            {
                CancelPriceEdit( priceIndex );
            }
        }

        protected void AddNewPriceButton_OnClick( object sender, EventArgs e )
        {
            ItemPricesGridView.Insert();

            _withAddPriceParameter.DefaultValue = "true";

            ItemPricesGridView.DataBind();

            InitiateEditModeForPrice( 0 );

            // allow the update postback to occur
            InsertPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }
            
        protected void ItemPricesGridView_OnDataBound( object sender, EventArgs e )
        {
            // update the header dual pricer status
            GridView gv = ( GridView )sender;
            if( gv != null )
            {
                if( gv.Rows.Count > 0 )
                {
                    // get the status from the 1st row
                    GridViewRow row = gv.Rows[ 0 ];
                    //string dualPriceDesignation = gv.GetStringValueFromSelectedIndexForTemplateField( 0, "DualPriceDesignationLabel" );
                        
                    //SelectedItemHeader.SetSingleDual( AlterSingleDualString( dualPriceDesignation ) );
                }
                else
                {
                    //SelectedItemHeader.SetSingleDual( "Undefined" );
                }

                PriceListChangeUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        //private void UpdateDualPricerDesignationInItemGridFromPriceGrid( string dualPriceDesignation )
        //{
        //    //if( ItemsGridView.SelectedIndex >= 0 )
        //    //{
        //    //    ItemsGridView.SetStringValueInControlInCell( ItemsGridView.SelectedIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( DualPriceDesignationFieldNumber ), "DualPriceDesignationLabel", dualPriceDesignation );
        //    //}
        //}

        protected void PriceFilterCheckBoxList_OnSelectedIndexChanged( Object sender, EventArgs e )
        {
            string boxThatWasChecked = Request.Form[ "__EVENTTARGET" ];
            int indexOfBoxThatWasClicked = int.Parse( boxThatWasChecked.Substring( boxThatWasChecked.Trim().Length - 1 ) );

            // gather values after click
            bool bIncludeActive = false;
            bool bIncludeFuture = false;
            bool bHistoricalOnly = false;

            for( int i = 0; i < PriceFilterCheckBoxList.Items.Count; i++ )
            {
                ListItem item = PriceFilterCheckBoxList.Items[ i ];

                if( item.Value.CompareTo( "Active" ) == 0 )
                {
                    bIncludeActive = item.Selected;
                }
                else if( item.Value.CompareTo( "Future" ) == 0 )
                {
                    bIncludeFuture = item.Selected;
                }
                else if( item.Value.CompareTo( "Historical" ) == 0 )
                {
                    bHistoricalOnly = item.Selected;
                }
            }

            string parmValue = AdjustPriceControlsDueToFilterClick( ( CheckBoxList )sender, indexOfBoxThatWasClicked, bIncludeActive, bIncludeFuture, bHistoricalOnly );

            _futureHistoricalSelectionCriteriaParameter.DefaultValue = parmValue;
            
            // bind
            ItemPricesGridView.DataBind();
            PriceFilterChangeUpdatePanelEventProxy.InvokeEvent( new EventArgs() ); 
        }

        private void SimulateHistoricalPriceFilterClick()
        {
            SetFilterItem( PriceFilterCheckBoxList, 2, true );

            // gather values after click
            bool bIncludeActive = false;
            bool bIncludeFuture = false;
            bool bHistoricalOnly = false;

            for( int i = 0; i < PriceFilterCheckBoxList.Items.Count; i++ )
            {
                ListItem item = PriceFilterCheckBoxList.Items[ i ];

                if( item.Value.CompareTo( "Active" ) == 0 )
                {
                    bIncludeActive = item.Selected;
                }
                else if( item.Value.CompareTo( "Future" ) == 0 )
                {
                    bIncludeFuture = item.Selected;
                }
                else if( item.Value.CompareTo( "Historical" ) == 0 )
                {
                    bHistoricalOnly = item.Selected;
                }
            }

            string parmValue = AdjustPriceControlsDueToFilterClick( PriceFilterCheckBoxList, 2, bIncludeActive, bIncludeFuture, bHistoricalOnly ); // false, false, true );

            _futureHistoricalSelectionCriteriaParameter.DefaultValue = parmValue;

            // bind
            ItemPricesGridView.DataBind();
            PriceFilterChangeUpdatePanelEventProxy.InvokeEvent( new EventArgs() ); 

        }

        private void SimulateActivePriceFilterClick()
        {
            SetFilterItem( PriceFilterCheckBoxList, 0, true );

            // gather values after click
            bool bIncludeActive = false;
            bool bIncludeFuture = false;
            bool bHistoricalOnly = false;

            for( int i = 0; i < PriceFilterCheckBoxList.Items.Count; i++ )
            {
                ListItem item = PriceFilterCheckBoxList.Items[ i ];

                if( item.Value.CompareTo( "Active" ) == 0 )
                {
                    bIncludeActive = item.Selected;
                }
                else if( item.Value.CompareTo( "Future" ) == 0 )
                {
                    bIncludeFuture = item.Selected;
                }
                else if( item.Value.CompareTo( "Historical" ) == 0 )
                {
                    bHistoricalOnly = item.Selected;
                }
            }

            string parmValue = AdjustPriceControlsDueToFilterClick( PriceFilterCheckBoxList, 0, bIncludeActive, bIncludeFuture, bHistoricalOnly );

            _futureHistoricalSelectionCriteriaParameter.DefaultValue = parmValue;

            // bind
            ItemPricesGridView.DataBind();
            PriceFilterChangeUpdatePanelEventProxy.InvokeEvent( new EventArgs() ); 

        }

        private string AdjustPriceControlsDueToFilterClick( CheckBoxList priceFilterCheckBoxList, int indexOfBoxThatWasClicked, bool bIncludeActive, bool bIncludeFuture, bool bHistoricalOnly )
        {
            string parmValue = "A";

            // active 
            if( indexOfBoxThatWasClicked == 0 )
            {
                // if set
                if( bIncludeActive == true )
                {
                    // if historical already selected then must clear historical
                    if( bHistoricalOnly == true )
                    {
                        bHistoricalOnly = false;
                        SetFilterItem( priceFilterCheckBoxList, 2, false );
                    }
                }
                else // if cleared, must ensure at least one is selected
                {
                    EnsureAtLeastOneFilterIsSelected( priceFilterCheckBoxList, ref bIncludeActive, ref bIncludeFuture, ref bHistoricalOnly );
                }
            }
            // future
            else if( indexOfBoxThatWasClicked == 1 )
            {
                // if set
                if( bIncludeFuture == true )
                {
                    // if historical already selected then must clear historical
                    if( bHistoricalOnly == true )
                    {
                        bHistoricalOnly = false;
                        SetFilterItem( priceFilterCheckBoxList, 2, false );
                    }
                }
                else // if cleared, must ensure at least one is selected
                {
                    EnsureAtLeastOneFilterIsSelected( priceFilterCheckBoxList, ref bIncludeActive, ref bIncludeFuture, ref bHistoricalOnly );
                }
            }
            // historical
            else if( indexOfBoxThatWasClicked == 2 )
            {
                // if set
                if( bHistoricalOnly == true )
                {
                    // clear active and future
                    bIncludeActive = false;
                    bIncludeFuture = false;

                    SetFilterItem( priceFilterCheckBoxList, 0, false );
                    SetFilterItem( priceFilterCheckBoxList, 1, false );
                }
                else // if cleared, must ensure at least one is selected
                {
                    EnsureAtLeastOneFilterIsSelected( priceFilterCheckBoxList, ref bIncludeActive, ref bIncludeFuture, ref bHistoricalOnly );
                }
            }

            if( bHistoricalOnly == true )
            {
                parmValue = "H";
                SetEnabledPriceControlsForHistorical( ItemPricesGridView, false );
            }
            else
            {
                if( bIncludeActive == true && bIncludeFuture == true )
                {
                    parmValue = "B"; // both
                }
                else if( bIncludeFuture == true )
                {
                    parmValue = "F";
                }
                else
                {
                    parmValue = "A";
                }
                SetEnabledPriceControlsForHistorical( ItemPricesGridView, true );
            }

            // sets context whether actual or simulated click
            _priceGridContext.IsHistorical = bHistoricalOnly;

            return ( parmValue );
        }

        private void EnsureAtLeastOneFilterIsSelected( CheckBoxList priceFilterCheckBoxList, ref bool bIncludeActive, ref bool bIncludeFuture, ref bool bHistoricalOnly )
        {
            if( bIncludeActive == false && bIncludeFuture == false && bHistoricalOnly == false )
            {
                bIncludeActive = true;

                SetFilterItem( priceFilterCheckBoxList, 0, true );
            }
        }

        private void SetFilterItem( CheckBoxList priceFilterCheckBoxList, int index, bool bSet )
        {
            ListItem item = priceFilterCheckBoxList.Items[ index ];
            item.Selected = bSet;
        }

        private void EnablePriceFilterAndAddControls( bool bEnabled )
        {
            CheckBoxList priceFilterCheckBoxList = ( CheckBoxList )medSurgItemForm.FindControl( "PriceFilterCheckBoxList" );
            if( priceFilterCheckBoxList != null )
            {
                priceFilterCheckBoxList.Enabled = bEnabled;
            }

            Button addNewPriceButton = ( Button )medSurgItemForm.FindControl( "AddNewPriceButton" );
            if( addNewPriceButton != null )
            {
                addNewPriceButton.Enabled = bEnabled;
            }

            // update the controls
            PriceFilterChangeUpdatePanelEventProxy.InvokeEvent( new EventArgs() ); 
        }

        protected void RemovePriceButton_DataBinding( object sender, EventArgs e )
        {
            Button removePriceButton = ( Button )sender;
            if( removePriceButton != null )
                MultiLineButtonText( removePriceButton, new string[] { "Remove", "Price" } );
        }
     

        protected void PriceLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label priceLabel = ( Label )sender;

            if( priceLabel != null )
            {
                GridViewRow gridViewRow = ( GridViewRow )priceLabel.NamingContainer;

                if( gridViewRow != null )
                {
                    if( gridViewRow.DataItem != null )
                    {
                        TableCell targetCell = gridViewRow.Cells[ PriceFieldNumber ];

                        string price = ( ( DataRowView )gridViewRow.DataItem )[ "Price" ].ToString();
                        priceLabel.Text = string.Format( "{0:c}", price );

                        string isTemporary = ( ( DataRowView )gridViewRow.DataItem )[ "IsTemporary" ].ToString();
                        bool bIsTemporary = bool.Parse( isTemporary );

                        string TPRAlwaysHasBasePrice = ( ( DataRowView )gridViewRow.DataItem )[ "TPRAlwaysHasBasePrice" ].ToString();
                        TPRAlwaysHasBasePrice = ( TPRAlwaysHasBasePrice.CompareTo( "0" ) == 0 ) ? "False" : ( ( TPRAlwaysHasBasePrice.CompareTo( "1" ) == 0 ) ? "True" : TPRAlwaysHasBasePrice );
                        bool bTPRAlwaysHasBasePrice = bool.Parse( TPRAlwaysHasBasePrice );

                        targetCell.CssClass = GetPriceFormat( targetCell.CssClass, price, bIsTemporary, bTPRAlwaysHasBasePrice, IsPriceHistorical() );
                    }
                }
            }
        }

        protected void PriceTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox priceTextBox = ( TextBox )sender;

            if( priceTextBox != null )
            {
                GridViewRow gridViewRow = ( GridViewRow )priceTextBox.NamingContainer;

                if( gridViewRow != null )
                {
                    if( gridViewRow.DataItem != null )
                    {
                        TableCell targetCell = gridViewRow.Cells[ PriceFieldNumber ];
                     
                        string price = ( ( DataRowView )gridViewRow.DataItem )[ "Price" ].ToString();
                        priceTextBox.Text = string.Format( "{0:c}", price );

                        string isTemporary = ( ( DataRowView )gridViewRow.DataItem )[ "IsTemporary" ].ToString();
                        bool bIsTemporary = bool.Parse( isTemporary );

                        string TPRAlwaysHasBasePrice = ( ( DataRowView )gridViewRow.DataItem )[ "TPRAlwaysHasBasePrice" ].ToString();
                        TPRAlwaysHasBasePrice = ( TPRAlwaysHasBasePrice.CompareTo( "0" ) == 0 ) ? "False" : ( ( TPRAlwaysHasBasePrice.CompareTo( "1" ) == 0 ) ? "True" : TPRAlwaysHasBasePrice );
                        bool bTPRAlwaysHasBasePrice = bool.Parse( TPRAlwaysHasBasePrice );

                        targetCell.CssClass = GetPriceFormat( targetCell.CssClass, price, bIsTemporary, bTPRAlwaysHasBasePrice, IsPriceHistorical() );
                    }
                }
            }
        }

        private string GetPriceFormat( string defaultClass, string priceString, bool bIsTemporary, bool bTPRAlwaysHasBasePrice, bool bIsPriceHistorical )
        {
            //HighlightedMissingData
            //HighlightedOutOfRangeData
            //HighlightedTPRWithoutBasePrice
            //UnhighlightedData

            string priceFormatClassName = defaultClass;   // or  "HighlightedOutOfRangeData";      or "HighlightedTPRWithoutBasePrice";
        
            if( bIsTemporary == true && bTPRAlwaysHasBasePrice == false && bIsPriceHistorical == false )
            {
                priceFormatClassName = "HighlightedTPRWithoutBasePrice";
            }

            return ( priceFormatClassName );
        }

        // selectedItemPriceHistoryId = -1 = not from history
        private void OpenPriceDetailsWindow( int itemIndex, int selectedItemId, string itemCatalogNumber, string itemDescription, bool bIsBPA, bool bIsBOA, bool bIsNational, int priceIndex, int selectedItemPriceId, int selectedItemPriceHistoryId, bool bIsFromHistory, DateTime contractExpirationDate, bool bIsService )
        {
            Session[ "ItemPriceDetailsWindowParms" ] = null;
            Session[ "ItemPriceDetailsWindowParms" ] = new ItemPriceDetailsWindowParms( itemIndex, selectedItemId, itemCatalogNumber, itemDescription, bIsBPA, bIsBOA, bIsNational, priceIndex, selectedItemPriceId, selectedItemPriceHistoryId, _startupParameters.ContractNumber, _startupParameters.ContractId, _startupParameters.IsPriceEditable, _startupParameters.IsPriceDetailsEditable, bIsFromHistory, contractExpirationDate, bIsService );

            string windowOpenScript = "window.open('ItemPriceDetails.aspx','ItemPriceDetails','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=200,left=240,width=1102,height=434, resizable=0');";
            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "ItemPriceDetailsWindowOpenScript", windowOpenScript, true );
        }

        private void InitiateEditModeForPrice( int editIndex )
        {
            ItemPricesGridView.EditIndex = editIndex;

            // select the edited item also
            SetItemPricesGridViewSelectedItem( editIndex );

            ItemPricesGridView.DataBind();

            // disable appropriate buttons for the selected row
            SetEnabledPriceControlsDuringEdit( ItemPricesGridView, editIndex, false );
        }

        private void SetItemPricesGridViewSelectedItem( int selectedPriceIndex )
        {
            // save for postback
            Session[ "ItemPricesGridViewSelectedIndex" ] = selectedPriceIndex;

            // set the row as selected
            ItemPricesGridView.SelectedIndex = selectedPriceIndex;

            // tell the client
            ScrollToSelectedPrice();

            // allow the update postback to occur
            ChangeItemPriceHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedPrice()
        {
            int rowIndex = ItemPricesGridView.SelectedIndex;
            //  TableItemStyle rowStyle = ItemsGridView.RowStyle;
            //  int rowHeight = ( int )rowStyle.Height.Value;  // this value is always zero
            int rowPosition = PRICEGRIDVIEWROWHEIGHTESTIMATE * rowIndex;

            string scrollToRowScript = String.Format( "setPriceScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedPriceIndexChangedScript", scrollToRowScript, true ); // runs after controls established
            //     ScriptManager.RegisterClientScriptBlock( this.Page, this.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true );  // runs prior to control rendering
        }

        private void RestoreItemPricesGridViewSelectedItem()
        {
            ItemPricesGridView.SelectedIndex = ( int )Session[ "ItemPricesGridViewSelectedIndex" ];
        }   

        private void SetEnabledPriceControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )
        {
            gv.SetEnabledControlsForCell( rowIndex, 0, bEnabled ); // details
            gv.SetEnabledControlsForCell( rowIndex, RemovePriceButtonFieldNumber, bEnabled ); // remove

            gv.SetVisibleControlsForCell( rowIndex, 1, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 1, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 1, "CancelButton", !bEnabled );

            EnablePriceFilterAndAddControls( bEnabled );

            // added 11/21/22 
            foreach( GridViewRow row in gv.Rows )
            {
                if( row.RowIndex != rowIndex )
                {
                    row.Enabled = bEnabled;
                }
            }
        }

        private void SetEnabledPriceControlsForHistorical( GridView gv, bool bEnabled )
        {
            Button addNewPriceButton = ( Button )medSurgItemForm.FindControl( "AddNewPriceButton" );
            if( addNewPriceButton != null )
            {
                // apply security to add button
                if( _startupParameters.IsPriceEditable == false || _startupParameters.IsExpired == true )
                {
                    addNewPriceButton.Enabled = false;
                }
                else
                {
                    addNewPriceButton.Enabled = bEnabled;
                }
            }
            // update the controls
            PriceFilterChangeUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        private int DeleteItemPrice( GridView gv, int rowIndex, int selectedItemPriceId )
        {
            // id of row to delete
            _itemPriceIdParameter.DefaultValue = selectedItemPriceId.ToString();

            try
            {
                _medSurgItemPricesDataSource.Delete();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            // previous row gets focus
            if( rowIndex >= 1 )
                rowIndex--;

            SetItemPricesGridViewSelectedItem( rowIndex );

            gv.DataBind();

            return ( rowIndex );
        }

        private int UpdateItemPrice( GridView gv, int rowIndex )
        {
            int updatedRowIndex = -1;

            _itemPriceIdParameter.DefaultValue = gv.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();
            
            _priceStartDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, PriceStartDateFieldNumber, 0, false, "priceStartDateTextBox" );
            _priceEndDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, PriceEndDateFieldNumber, 0, false, "priceEndDateTextBox" );
            _priceParameter.DefaultValue = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, PriceFieldNumber, 0, false, "priceTextBox" ), "Price" ).ToString();
            _isTemporaryParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsTemporary" ).ToString();

            try
            {
                _medSurgItemPricesDataSource.Update();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.EditIndex = -1; // done with edit
            //         SetItemsGridViewSelectedItem( rowIndex );
            gv.DataBind();

            if( Session[ "LastUpdatedItemPriceId" ] != null )
            {
                int updatedItemPriceId = ( int )Session[ "LastUpdatedItemPriceId" ];
                updatedRowIndex = ItemPricesGridView.GetRowIndexFromId( updatedItemPriceId, 0 );

                SetItemPricesGridViewSelectedItem( updatedRowIndex );

                // bind to select
                gv.DataBind();
            }


            // enable appropriate buttons for the selected row
            SetEnabledPriceControlsDuringEdit( gv, updatedRowIndex, true );

            return ( updatedRowIndex );
        }

        private bool ValidateItemBeforeUpdate( GridView gv, int rowIndex, int selectedItemId, ref string validationMessage )
        {
            bool bIsItemOk = true;

            validationMessage = "";

            try
            {
                // parent description
                string itemDescriptionCleansed = "";
                if( _startupParameters.IsBPA == true )
                {
                    itemDescriptionCleansed = "Reference parent item description";
                }
                else
                {
                    string itemDescription = gv.GetStringValueFromSelectedControl( rowIndex, ItemDescriptionFieldNumber, 0, false, "itemDescriptionTextBox" );
                    itemDescriptionCleansed = CMGlobals.ReplaceNonPrintableCharacters( itemDescription ).Trim();                    
                }
                if( itemDescriptionCleansed.Trim().Length == 0 )
                {
                    throw new Exception( "Item description is required." );
                }

                // parent item id
                if( _startupParameters.IsBPA == true )
                {
                    string parentItemId = gv.GetStringValueFromSelectedControl( rowIndex, ParentItemDropDownListFieldNumber, 0, false, "parentItemDropDownList" );
                    if( parentItemId == "-1" )
                    {
                        throw new Exception( "Parent selection is required." );
                    }
                }
                // else null

                // catalog number
                string catalogNumber = "";
                string catalogNumberCleansed = "";
                if( _startupParameters.IsBPA == true )
                {
                    catalogNumberCleansed = "Reference parent part number";
                }
                else if( _startupParameters.IsService == true )
                {
                    catalogNumberCleansed = "Services";
                }
                else // FSS or National
                {
                    catalogNumber = gv.GetStringValueFromSelectedControl( rowIndex, CatalogNumberFieldNumber, 0, false, "catalogNumberTextBox" );
                    catalogNumberCleansed = CMGlobals.ReplaceNonPrintableCharacters( catalogNumber ).Trim();
                }

                if( catalogNumberCleansed.Trim().Length == 0 )
                {
                    throw new Exception( "Part number is required." );
                } 

                 // manufacturers part number
                //string manufacturersPartNumber = "";
                //string manufacturersPartNumberCleansed = "";
                //if( _startupParameters.IsBPA == true )
                //{
                //    manufacturersPartNumberCleansed = "Reference parent manuf part number";
                //}
                //else if( _startupParameters.IsService == true )
                //{
                //    manufacturersPartNumberCleansed = "Services";
                //}              
                //else // FSS or National
                //{
                //    manufacturersPartNumber = gv.GetStringValueFromSelectedControl( rowIndex, ManufacturersPartNumberFieldNumber, 0, false, "manufacturersPartNumberTextBox" );
                //    manufacturersPartNumberCleansed = CMGlobals.ReplaceNonPrintableCharacters( manufacturersPartNumber ).Trim();
                //}

                // initially the new value will not be mandatory
                //if( manufacturersPartNumberCleansed.Trim().Length == 0 )
                //{
                //    throw new Exception( "Manufacturers part number is required." );
                //} 

                // SIN
                string SIN = "";
                if( _startupParameters.IsNational == true || _startupParameters.IsBPA == true )
                {
                    // NC or BPA SINs get defaulted based on schedule
                    SIN = _startupParameters.DefaultSIN;
                }
                // service SIN comes from category
                else if( _startupParameters.IsService == true )
                {
                    SIN = gv.GetStringValueFromSelectedControl( rowIndex, ServiceCategorySINFieldNumber, 0, false, "ServiceCategorySINLabel" );
                }
                else  // FSS item
                {
                    SIN = gv.GetStringValueFromSelectedControl( rowIndex, SINFieldNumber, 0, false, "ItemSINDropDownList" );
                }

                if( SIN.Trim().Length == 0 )
                {
                    throw new Exception( "SIN is required." );
                } 

                // Packaging
                string packaging = "";
                if( _startupParameters.IsBPA == true )
                {
                    packaging = "BP";   // refer to parent for actual packaging
                }
                else if( _startupParameters.IsService == true )
                {
                    packaging = "HR";  // hourly rate is default for service item measure
                }
                else  // FSS or National packaging is specified
                {
                    packaging = gv.GetStringValueFromSelectedControl( rowIndex, PackageAsPricedFieldNumber, 0, false, "packageAsPricedDropDownList" );
                }

                if( packaging.Trim().Length == 0 || packaging.CompareTo( "--" ) == 0 )
                {
                    throw new Exception( "Package as priced is required." );
                }

                if( _startupParameters.IsService == true )
                {
                    string serviceCategoryIdString = gv.GetStringValueFromSelectedControl( rowIndex, ServiceCategoryIdFieldNumber, 0, false, "ServiceCategoryIdLabel" );
                    if( serviceCategoryIdString == "-1" )
                    {
                        throw new Exception( "Service category selection is required." );
                    }
                }
            }
            catch( Exception ex )
            {
                bIsItemOk = false;
                validationMessage = string.Format( "The following exception was encountered validating the item: {0}", ex.Message );
            }

            return ( bIsItemOk );
        }

        private bool ValidatePriceBeforeUpdate( GridView gv, int rowIndex, int itemPriceId, int itemId, ref bool bUserCanOverride, ref bool bWarningOnly, ref string validationMessage, ref string warningMessage )
        {
            bool bIsPriceOk = true;           
            bool bSuccess = false;

            bUserCanOverride = false;
            bWarningOnly = false;
            validationMessage = "";
            warningMessage = "";

            try
            {
                DateTime priceStartDate;
                if( DateTime.TryParse( gv.GetStringValueFromSelectedControl( rowIndex, PriceStartDateFieldNumber, 0, false, "priceStartDateTextBox" ), out priceStartDate ) != true )
                {
                    throw new Exception( "Price start date is not correctly formatted." );
                }
                
                DateTime priceEndDate;
                if( DateTime.TryParse( gv.GetStringValueFromSelectedControl( rowIndex, PriceEndDateFieldNumber, 0, false, "priceEndDateTextBox" ), out priceEndDate ) != true )
                {
                    throw new Exception( "Price end date is not correctly formatted." );
                }

                if( DateTime.Compare( priceStartDate, priceEndDate ) >= 0 )
                {
                    throw new Exception( "Price start date must precede price end date." );
                }

                decimal price = 0;
                if( Decimal.TryParse( CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, PriceFieldNumber, 0, false, "priceTextBox" ), "Price" ).ToString(), out price ) != true )
                {
                    throw new Exception( "Price is not correctly formatted." );
                }
              
                bool bIsTemporary = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsTemporary" );

                ItemDB itemDB = ( ItemDB )Session[ "ItemDB" ];
                bSuccess = itemDB.ValidateMedSurgPriceAgainstOtherPrices( _startupParameters.ContractNumber, _startupParameters.ContractId,
                    itemPriceId, itemId, priceStartDate, priceEndDate, bIsTemporary, ref bIsPriceOk, ref bUserCanOverride, ref validationMessage );
                if( bSuccess == false )
                {
                    throw new Exception( string.Format( "Exception encountered validating price: {0}", itemDB.ErrorMessage ) );
                }
                else if( bIsPriceOk == true )
                {
                    if( validationMessage.Contains( "base price" ) == true )
                    {
                        bWarningOnly = true;
                        warningMessage = validationMessage;
                    }

                }
                else if( bIsPriceOk == false )
                {
                    throw new Exception( string.Format( "Error encountered validating price: {0}", validationMessage ) );
                }
   
            }
            catch( Exception ex )
            {
                bIsPriceOk = false;
                validationMessage = string.Format( "The following exception was encountered validating the price: {0}", ex.Message );
            }

            return ( bIsPriceOk );
        }

        private int InsertItemPrice( GridView gv, int rowIndex, int selectedItemId )
        {
            int insertedRowIndex = 0;

            _itemIdForPricesParameter.DefaultValue = selectedItemId.ToString();
            _priceStartDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, PriceStartDateFieldNumber, 0, false, "priceStartDateTextBox" );
            _priceEndDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, PriceEndDateFieldNumber, 0, false, "priceEndDateTextBox" );
            _priceParameter.DefaultValue = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, PriceFieldNumber, 0, false, "priceTextBox" ), "Price" ).ToString();
            _isTemporaryParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsTemporary" ).ToString();

            try
            {
                _medSurgItemPricesDataSource.Insert();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.InsertRowActive = false; // done with insert
            gv.EditIndex = -1; // done with edit of new row
            _withAddPriceParameter.DefaultValue = "false"; // no extra row

            // bind with new row
            gv.DataBind();
            
            if( Session[ "LastInsertedItemPriceId" ] != null )
            {
                int newItemPriceId = ( int )Session[ "LastInsertedItemPriceId" ];
                insertedRowIndex = ItemPricesGridView.GetRowIndexFromId( newItemPriceId, 0 );

                // new item falls out of range of current filter, so go to item 0 instead
                if( insertedRowIndex == -1 )
                {
                    if( gv.HasData() == true )
                        insertedRowIndex = 0;
                }

                if( insertedRowIndex != -1 )
                {
                    SetItemPricesGridViewSelectedItem( insertedRowIndex );

                    // bind to select
                    gv.DataBind();
                }
            }

            // enable appropriate buttons for the selected row
            SetEnabledPriceControlsDuringEdit( gv, insertedRowIndex, true );

            return ( insertedRowIndex );
        }

        protected void _medSurgItemPricesDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@ItemPriceId" ].Value != null )
            {
                string medSurgItemPriceIdString = e.Command.Parameters[ "@ItemPriceId" ].Value.ToString();

                if( medSurgItemPriceIdString.Length > 0 )
                {
                    int itemPriceId = int.Parse( medSurgItemPriceIdString );
                    Session[ "LastInsertedItemPriceId" ] = itemPriceId;
                }
            }
            else
            {
                throw new Exception( "ItemPriceId returned from insert was null. Insert failed." );
            }
          
        }

        protected void ItemPricesGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;

                if( e.Row.RowType == DataControlRowType.DataRow )
                {
                    if( ( ( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                       ( ( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert ) )
                    {
                        Button removePriceButton = null;
                        removePriceButton = ( Button )e.Row.FindControl( "RemovePriceButton" );
                        if( removePriceButton != null )
                        {
                            removePriceButton.Attributes.Add( "onclick", "presentConfirmationMessage('Permanently delete the selected price?')" );
                        }
                    }

                    if( _startupParameters.IsPriceEditable == false || _startupParameters.IsExpired == true )
                    {
                        Button removePriceButton = null;
                        removePriceButton = ( Button )e.Row.FindControl( "RemovePriceButton" );
                        if( removePriceButton != null )
                        {
                            removePriceButton.Enabled = false;
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

                    Label IsFromHistoryLabel = ( Label )e.Row.Cells[ IsFromHistoryFieldNumber ].FindControl( "IsFromHistoryLabel" );
                    if( IsFromHistoryLabel != null )
                    {
                        if( IsFromHistoryLabel.Text.CompareTo( "1" ) == 0 )
                        {
                            Button removePriceButton = null;
                            removePriceButton = ( Button )e.Row.FindControl( "RemovePriceButton" );
                            if( removePriceButton != null )
                            {
                                removePriceButton.Enabled = false;
                            }

                            Button editButton = null;
                            editButton = ( Button )e.Row.FindControl( "EditButton" );
                            if( editButton != null )
                            {
                                editButton.Enabled = false;
                            }
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }
        }

        protected void ItemPricesGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForPrice( e.NewEditIndex );
        }

        protected void ItemPricesGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            CancelPriceEdit( e.RowIndex );
        }

        protected void CancelPriceEdit( int rowIndex )
        {
            bool bInserting = ItemPricesGridView.InsertRowActive;

            // if inserting
            if( bInserting == true )
            {
                ItemPricesGridView.InsertRowActive = false; // cancels insert ( if inserting )
                _withAddPriceParameter.DefaultValue = "false";
                ItemPricesGridView.EditIndex = -1; // cancels the edit
                ItemPricesGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledPriceControlsDuringEdit( ItemPricesGridView, rowIndex, true );

                HighlightItemPriceRow( 0 );

                // allow the cancel postback to occur 
                EditCancelPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else
            {
                ItemPricesGridView.EditIndex = -1; // cancels the edit
                ItemPricesGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledPriceControlsDuringEdit( ItemPricesGridView, rowIndex, true );

                HighlightItemPriceRow( rowIndex );

                // allow the cancel postback to occur 
                EditCancelPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }

        }

        // probably wont happen - id changing during update
        protected void _medSurgItemPricesDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string medSurgItemPriceIdString = e.Command.Parameters[ "@ItemPriceId" ].Value.ToString();

            if( medSurgItemPriceIdString.Length > 0 )
            {
                int medSurgItemPriceId = int.Parse( medSurgItemPriceIdString );
                Session[ "LastUpdatedItemPriceId" ] = medSurgItemPriceId;
            }
        }

        protected void HighlightItemPriceRow( int itemIndex )
        {
            if( itemIndex >= 0 && ItemPricesGridView.Rows.Count > 0 )
            {

                string highlightedRowOriginalColor = "";
                int highlightedRowIndex = itemIndex + 1;

                try
                {
                    GridViewRow row = ItemPricesGridView.Rows[ itemIndex ]; // $$$ this throws for the drug item screen. keep a lookout for medSurg.
                    if( row.RowState == DataControlRowState.Alternate )
                    {
                        highlightedRowOriginalColor = ItemPricesGridView.AlternatingRowStyle.BackColor.ToString();
                    }
                    else
                    {
                        highlightedRowOriginalColor = ItemPricesGridView.RowStyle.BackColor.ToString();
                    }

                    string preserveHighlightingScript = String.Format( "setItemPriceHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                    ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreservePriceHighlightingScript", preserveHighlightingScript, true );
                }
                catch( Exception ex )
                {
                }
            }
            // else allow postback anyway to handle index out of range case where price row should fall off filter criteria

            // allow the highlight postback to occur 
            ChangeItemPriceHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        private void RefreshItemPriceScreenDueToDetailsChanged( bool bRebindItemPrices )
        {
           // ItemPricesGridView.DataBind(); // if sub item id is changed in item, reflect to pricelist
            BindItemPricesHeaderAndGrid(); // $$$ changed simple bind to this call which also updates the header 2/24/2016

            int currentPriceIndex = ItemPricesGridView.SelectedIndex;
            ScrollToSelectedPrice();
            HighlightItemPriceRow( currentPriceIndex );
        }

        protected void ItemPricesGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;

                 // upon first entry after document creation, 
                // e.Row.Cells.Count is 1 or -1, make sure count is ok before proceeding
                if( e.Row.Cells.Count > TPRAlwaysHasBasePriceFieldNumber )
                {
                    // disable price matrix checkboxes based on document type
                    DisablePriceMatrixForRow( e.Row );
              
                    // hide is history
                    e.Row.Cells[ IsFromHistoryFieldNumber ].Visible = false;

                    if( IsPriceHistorical() == true )
                    {
                        e.Row.Cells[ PriceLastModifiedByFieldNumber ].Visible = false;
                        e.Row.Cells[ PriceLastModificationDateFieldNumber ].Visible = false;

                        e.Row.Cells[ PriceReasonMovedToHistoryFieldNumber ].Visible = true;
                        e.Row.Cells[ PriceMovedToHistoryByFieldNumber ].Visible = true;
                        e.Row.Cells[ PriceDateMovedToHistoryFieldNumber ].Visible = true;
                    }
                    else
                    {
                        //e.Row.Cells[ PriceLastModifiedByFieldNumber ].Visible = true;
                        //e.Row.Cells[ PriceLastModificationDateFieldNumber ].Visible = true;

                        e.Row.Cells[ PriceReasonMovedToHistoryFieldNumber ].Visible = false;
                        e.Row.Cells[ PriceMovedToHistoryByFieldNumber ].Visible = false;
                        e.Row.Cells[ PriceDateMovedToHistoryFieldNumber ].Visible = false;
                    }

                    // hide always has base price
                    e.Row.Cells[ TPRAlwaysHasBasePriceFieldNumber ].Visible = false;
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void ItemPricesGridView_OnPreRender( object sender, EventArgs e )
        {

            GridView itemPricesGridView = ( GridView )sender;
            GridViewRow headerRow = itemPricesGridView.HeaderRow;

            // no prices
            if( headerRow == null )
                return;

            // hide is history
            headerRow.Cells[ IsFromHistoryFieldNumber ].Visible = false;

            // if viewing price history
            if( IsPriceHistorical() == true )
            {
                headerRow.Cells[ PriceLastModifiedByFieldNumber ].Visible = false;
                headerRow.Cells[ PriceLastModificationDateFieldNumber ].Visible = false;

                headerRow.Cells[ PriceReasonMovedToHistoryFieldNumber ].Visible = true;
                headerRow.Cells[ PriceMovedToHistoryByFieldNumber ].Visible = true;
                headerRow.Cells[ PriceDateMovedToHistoryFieldNumber ].Visible = true;
            }
            else
            {
                //headerRow.Cells[ PriceLastModifiedByFieldNumber ].Visible = true;
                //headerRow.Cells[ PriceLastModificationDateFieldNumber ].Visible = true;

                headerRow.Cells[ PriceReasonMovedToHistoryFieldNumber ].Visible = false;
                headerRow.Cells[ PriceMovedToHistoryByFieldNumber ].Visible = false;
                headerRow.Cells[ PriceDateMovedToHistoryFieldNumber ].Visible = false;
            }

            foreach( GridViewRow gridViewRow in itemPricesGridView.Rows )
            {
                // hide is history
                gridViewRow.Cells[ IsFromHistoryFieldNumber ].Visible = false;          
   
                 // if viewing price history
                if( IsPriceHistorical() == true )
                {
                    gridViewRow.Cells[ PriceLastModifiedByFieldNumber ].Visible = false;
                    gridViewRow.Cells[ PriceLastModificationDateFieldNumber ].Visible = false;

                    gridViewRow.Cells[ PriceReasonMovedToHistoryFieldNumber ].Visible = true;
                    gridViewRow.Cells[ PriceMovedToHistoryByFieldNumber ].Visible = true;
                    gridViewRow.Cells[ PriceDateMovedToHistoryFieldNumber ].Visible = true;
                }
                else
                {
                    //gridViewRow.Cells[ PriceLastModifiedByFieldNumber ].Visible = true;
                    //gridViewRow.Cells[ PriceLastModificationDateFieldNumber ].Visible = true;
                    
                    gridViewRow.Cells[ PriceReasonMovedToHistoryFieldNumber ].Visible = false;
                    gridViewRow.Cells[ PriceMovedToHistoryByFieldNumber ].Visible = false;
                    gridViewRow.Cells[ PriceDateMovedToHistoryFieldNumber ].Visible = false;
                }

                // disable price details button
                if( _startupParameters.IsBPA == true || _startupParameters.IsService == true || _startupParameters.IsNational == true )
                {
                    DisablePriceDetailsButtonForRow( gridViewRow );
                }
            }
        }

        protected void DisablePriceDetailsButtonForRow( GridViewRow gridViewRow )
        {
            ControlCollection controlsInCell = null;

            if( gridViewRow != null )
            {
                controlsInCell = gridViewRow.Cells[ PriceDetailsButtonFieldNumber ].Controls;

                if( controlsInCell != null )
                {
                    foreach( Control control in controlsInCell )
                    {
                        if( control != null )
                        {
                            Type controlType = control.GetType();
                            string typeName = controlType.Name;
                            if( typeName.CompareTo( "Button" ) == 0 )
                            {
                                ( ( Button )control ).Enabled = false;
                            }
                        }
                    }
                }
            }
        }
        //*********************************** shared functions **************************

        private void MultiLineButtonText( Button button, string[] buttonTextArray )
        {

            StringBuilder sb = new StringBuilder();

            for( int i = 0; i < buttonTextArray.Count(); i++ )
            {
                sb.AppendLine( buttonTextArray[ i ] );
            }
            
            button.Text = sb.ToString(); 
        }

        protected void ItemsScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "ItemsErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "ItemsErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            ItemsScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

       

    }
}
