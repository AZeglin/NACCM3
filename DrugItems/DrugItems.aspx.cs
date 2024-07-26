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
using TextBox = System.Web.UI.WebControls.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.DrugItems
{
    public partial class DrugItems : System.Web.UI.Page
    {
        // item
        private const int HasBPAFieldNumber = 15;    // $$$+
        private const int DrugItemNDCIdFieldNumber = 20;    //$$$+
        private const int ParentDrugItemIdFieldNumber = 21; //$$$+
        private const int DualPriceDesignationFieldNumber = 22; //$$$+

        private const int ParentDrugItemDropDownListFieldNumber = 3;
        private const int FdaAssignedLabelerCodeFieldNumber = 4;
        private const int ProductCodeFieldNumber = 5;
        private const int PackageCodeFieldNumber = 6;
        private const int CoveredIndicatorFieldNumber = 7;
        private const int GenericFieldNumber = 8;
        private const int TradeNameFieldNumber = 9;
        private const int DispensingUnitFieldNumber = 10;
        private const int PackageSizeFieldNumber = 11;

        // price
        private const int PriceStartDateFieldNumber = 2; //$$$+
        private const int PriceFieldNumber = 4; //$$$+
        private const int PriceMatrixFieldOffset = 6; //$$$+
        private const int SubItemDropDownFieldNumber = 25; // $$$+
        private const int RemoveButtonFieldNumber = 27; // $$$+
        private const int VAIFFFieldNumber = 29;  //$$$+
        private const int DualPriceDesignationInPriceFieldNumber = 30; //$$$+
        private const int IsFromHistoryFieldNumber = 31; //$$$+
        private const int IsHistoryFromArchiveFieldNumber = 32; //$$$+
        private const int TPRAlwaysHasBasePriceFieldNumber = 33; //$$$+

        private const string SavePriceValidationUserOverrideKeyName = "SavePriceValidationUserOverride";

        private DocumentDataSource _drugItemsDataSource = null;
        private DocumentDataSource _drugItemPricesDataSource = null;

        private DocumentDataSource _parentDrugItemsDataSource = null;

        private DocumentDataSource _subItemIdentifierDataSource = null;

        private Parameter _modificationStatusIdParameter = null;

        // item parameters
        private Parameter _contractNumberParameter = null;
        private Parameter _drugItemIdForItemsParameter = null;
        private Parameter _drugItemNDCIdParameter = null;
        private Parameter _drugItemNDCIdForItemInsertParameter = null;
        private Parameter _discontinuationDateParameter = null;
        private Parameter _drugItemIdForItemInsertParameter = null;
        private Parameter _drugItemIdForPricesParameter = null;
        private Parameter _fdaAssignedLabelerCodeParameter = null;
        private Parameter _productCodeParameter = null;
        private Parameter _packageCodeParameter = null;
        private Parameter _coveredParameter = null;
        private Parameter _genericParameter = null;
        private Parameter _tradeNameParameter = null;
        private Parameter _dispensingUnitParameter = null;
        private Parameter _packageDescriptionParameter = null;
        private Parameter _withAddParameter = null;
        private Parameter _coveredSelectionCriteriaParameter = null;
        private Parameter _isBPAParameter = null;

        // bpa
        private Parameter _parentDrugItemIdParameter = null;

        // price parameters
        private Parameter _futureHistoricalSelectionCriteriaParameter = null;

        private Parameter _drugItemPriceIdParameter = null;
        private Parameter _withAddPriceParameter = null;
      //  private Parameter _priceIdParameter = null;

        private Parameter _drugItemPriceIdForPriceInsertParameter = null;
      //  private Parameter _priceIdForPriceInsertParameter = null;

        private Parameter _priceStartDateParameter = null;
        private Parameter _priceEndDateParameter = null;
        private Parameter _priceParameter = null;
        private Parameter _isTemporaryParameter = null;
        private Parameter _isFSSParameter = null;
        private Parameter _isBIG4Parameter = null;
        private Parameter _isVAParameter = null;
        private Parameter _isBOPParameter = null;
        private Parameter _isCMOPParameter = null;
        private Parameter _isDODParameter = null;
        private Parameter _isHHSParameter = null;
        private Parameter _isIHSParameter = null;
        private Parameter _isIHS2Parameter = null;
        private Parameter _isDIHSParameter = null;
        private Parameter _isNIHParameter = null;
        private Parameter _isPHSParameter = null;
        private Parameter _isSVHParameter = null;
        private Parameter _isSVH1Parameter = null;
        private Parameter _isSVH2Parameter = null;
        private Parameter _isTMOPParameter = null;
        private Parameter _isUSCGParameter = null;
        private Parameter _isFHCCParameter = null;
        private Parameter _drugItemSubItemIdParameter = null;

        //private Parameter _awardedFSSTrackingCustomerRatio = null;
        //private Parameter _proposedTrackingCustomerPriceParameter = null;
        //private Parameter _currentTrackingCustomerPriceParameter = null;
        //private Parameter _primeVendorChangedDateParameter = null;

        // command line parameters
        private DrugItemStartupParameters _startupParameters = null;

        // context which affects item grid
        private ItemGridContext _itemGridContext = null;

        private bool[] _documentTypePriceMatrix = null;

        private const int NUMBEROFITEMGRIDVIEWLEADINGCOLS = 2;

        private const int ITEMGRIDVIEWROWHEIGHTESTIMATE = 48;
        private const int PRICEGRIDVIEWROWHEIGHTESTIMATE = 47;

#region LoadFunctions

        [Serializable]
        public class DrugItemStartupParameters
        {
            public string ContractNumber = "";
            public bool IsItemEditable = false;
            public bool IsPriceEditable = false;
            public bool IsItemDetailsCOEditable = false;
            public bool IsItemDetailsPBMEditable = false;
            public bool IsNational = false;
            public string VendorName = "";
            public bool IsBPA = false;
        }

        [Serializable]
        public class ItemGridContext
        {
            public bool IsDiscontinued = false;
        }

        protected void Page_Load( object sender, EventArgs e )
        {
         
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }

     //       CMGlobals.AddKeepAlive( this.Page, 12000 );

            LoadStartupParms();
            InitItemGridContext();
            InitSharedParms();
            LoadDrugItems();

            if( Page.IsPostBack == false )
            {
                SetDrugItemsGridViewSelectedItem( 0, true );
                BindDrugItemHeaderAndGrid();
            }
            else
            {
                RestoreDrugItemsGridViewSelectedItem();
            }

            DisablePriceMatrixForDocumentType();
            LoadDrugItemPrices();

            if( Page.IsPostBack == false )
            {
                SetDrugItemPricesGridViewSelectedItem( 0 );
                BindDrugItemPricesHeaderAndGrid();
                DrugItemPricesGridViewUpdatePanel.Update();
            }
            else
            {
                RestoreDrugItemPricesGridViewSelectedItem();
                RestoreDrugItemPricesGridViewInterimValues();
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
                HiddenField ServerConfirmationDialogResultsHiddenField = ( HiddenField )drugItemForm.FindControl( "ServerConfirmationDialogResults" );

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
                HiddenField RefreshItemHeaderCountOnSubmitHiddenField = ( HiddenField )drugItemForm.FindControl( "RefreshItemHeaderCountOnSubmit" );

                if( RefreshItemHeaderCountOnSubmitHiddenField != null )
                {
                    refreshItemHeaderCountOnSubmit = RefreshItemHeaderCountOnSubmitHiddenField.Value;
                    if( refreshItemHeaderCountOnSubmit.Contains( "refresh" ) == true )
                    {
                        bRefreshItemHeaderCount = true;
                        RefreshItemHeaderCountOnSubmitHiddenField.Value = "false";
                    }
                }

                
                string refreshDrugItemScreenOnSubmit = "";
                HiddenField RefreshDrugItemScreenOnSubmitHiddenField = ( HiddenField )drugItemForm.FindControl( "RefreshDrugItemScreenOnSubmit" );

                if( RefreshDrugItemScreenOnSubmitHiddenField != null )
                {
                    refreshDrugItemScreenOnSubmit = RefreshDrugItemScreenOnSubmitHiddenField.Value;
                    if( refreshDrugItemScreenOnSubmit.Contains( "true" ) == true )
                    {

                        string rebindDrugItemScreenOnRefreshOnSubmit = "";
                        HiddenField RebindDrugItemScreenOnRefreshOnSubmitHiddenField = ( HiddenField )drugItemForm.FindControl( "RebindDrugItemScreenOnRefreshOnSubmit" );

                        if( RebindDrugItemScreenOnRefreshOnSubmitHiddenField != null )
                        {
                            rebindDrugItemScreenOnRefreshOnSubmit = RebindDrugItemScreenOnRefreshOnSubmitHiddenField.Value;
                            if( rebindDrugItemScreenOnRefreshOnSubmit.Contains( "true" ) == true )
                            {
                                bRebindItems = true;
                            }

                            RebindDrugItemScreenOnRefreshOnSubmitHiddenField.Value = "false";
                        }

                        RefreshDrugItemScreenOnSubmitHiddenField.Value = "false";
                        RefreshDrugItemScreenDueToDetailsChanged( bRebindItems, bRefreshItemHeaderCount );
                        bItemGridWasRefreshed = true;
                    }
                }


                string refreshDrugItemPriceScreenOnSubmit = "";
                HiddenField RefreshDrugItemPriceScreenOnSubmitHiddenField = ( HiddenField )drugItemForm.FindControl( "RefreshDrugItemPriceScreenOnSubmit" );

                if( RefreshDrugItemPriceScreenOnSubmitHiddenField != null )
                {
                    refreshDrugItemPriceScreenOnSubmit = RefreshDrugItemPriceScreenOnSubmitHiddenField.Value;
                    if( refreshDrugItemPriceScreenOnSubmit.Contains( "true" ) == true )
                    {
                        string rebindDrugItemPriceScreenOnRefreshOnSubmit = "";
                        HiddenField RebindDrugItemPriceScreenOnRefreshOnSubmitHiddenField = ( HiddenField )drugItemForm.FindControl( "RebindDrugItemPriceScreenOnRefreshOnSubmit" );

                        if( RebindDrugItemPriceScreenOnRefreshOnSubmitHiddenField != null )
                        {
                            rebindDrugItemPriceScreenOnRefreshOnSubmit = RebindDrugItemPriceScreenOnRefreshOnSubmitHiddenField.Value;
                            if( rebindDrugItemPriceScreenOnRefreshOnSubmit.Contains( "true" ) == true )
                            {
                                bRebindItemPrices = true;
                            }

                            RebindDrugItemPriceScreenOnRefreshOnSubmitHiddenField.Value = "false";
                        }

                        RefreshDrugItemPriceScreenOnSubmitHiddenField.Value = "false";
                        RefreshDrugItemPriceScreenDueToDetailsChanged( bRebindItemPrices );
                        // since doing complete postback ( for now ), need to restore item as well
                        if( bItemGridWasRefreshed == false )
                            RefreshDrugItemScreenDueToDetailsChanged( bRebindItems, bRefreshItemHeaderCount ); 
                    }
                }
            }
        }

        private void DisableControlsForReadOnlyOrDocumentTypeOrItemGridContext()
        {
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                     if( currentDocument.IsBPA( currentDocument.ScheduleNumber ) == false )
                     {
                         if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmChangeNDC ) == true && _itemGridContext.IsDiscontinued == false )
                         {
                             ItemContextMenu.Enable( "NDCChangeCommand", true );
                         }
                         else
                         {
                             ItemContextMenu.Enable( "NDCChangeCommand", false );
                         }

                         if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmCopyItem ) == true && _itemGridContext.IsDiscontinued == false )
                         {
                             ItemContextMenu.Enable( "ItemCopyCommand", true );
                         }
                         else
                         {
                             ItemContextMenu.Enable( "ItemCopyCommand", false );
                         }

                         if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmCopyItemToContract ) == true )
                         {
                             ItemContextMenu.Enable( "ItemCopyToContractCommand", true );
                         }
                         else
                         {
                             ItemContextMenu.Enable( "ItemCopyToContractCommand", false );
                         }

                         if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemDiscontinue ) == true && _itemGridContext.IsDiscontinued == false )
                         {
                             ItemContextMenu.Enable( "ItemDiscontinueCommand", true );
                         }
                         else
                         {
                             ItemContextMenu.Enable( "ItemDiscontinueCommand", false );
                         }
                
                         if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemDiscontinue ) == true )
                         {
                             ItemContextMenu.Enable( "RestoreDiscontinuedItemCommand", true );
                         }
                         else
                         {
                             ItemContextMenu.Enable( "RestoreDiscontinuedItemCommand", false );
                         }
                     }
                     else
                     {
                         ItemContextMenu.Enable( "NDCChangeCommand", false );
                         ItemContextMenu.Enable( "ItemCopyCommand", false );
                         ItemContextMenu.Enable( "ItemCopyToContractCommand", false );
                         ItemContextMenu.Enable( "ItemDiscontinueCommand", false );
                         ItemContextMenu.Enable( "RestoreDiscontinuedItemCommand", false );
                     }
         
            if( _startupParameters.IsItemEditable == false || _itemGridContext.IsDiscontinued == true )
            {
                Button addNewItemButton = ( Button )drugItemForm.FindControl( "AddNewItemButton" );
                if( addNewItemButton != null )
                {
                    addNewItemButton.Enabled = false;
                }
            }
            else
            {
                Button addNewItemButton = ( Button )drugItemForm.FindControl( "AddNewItemButton" );
                if( addNewItemButton != null )
                {
                    addNewItemButton.Enabled = true;
                }
            }

            if( _startupParameters.IsPriceEditable == false )
            {
                Button addNewPriceButton = ( Button )drugItemForm.FindControl( "AddNewPriceButton" );
                if( addNewPriceButton != null )
                {
                    addNewPriceButton.Enabled = false;
                }
            }
        }

        private enum PriceMatrixFields
        {
            IsTemporary=0,
            IsFSS=1,
            IsBIG4=2,
            IsVA=3,
            IsBOP=4,
            IsCMOP=5,
            IsDOD=6,
            IsHHS=7,
            IsIHS=8,
            IsIHS2=9,
            IsDIHS=10,
            IsNIH=11,
            IsPHS=12,
            IsSVH=13,
            IsSVH1=14,
            IsSVH2=15,
            IsTMOP=16,
            IsUSCG=17,
            IsFHCC=18,
            MaxPriceMatrixFields = 18
        }

        private void DisablePriceMatrixForDocumentType()
        {
            if( Session[ "DocumentTypePriceMatrix" ] == null )
            {
                _documentTypePriceMatrix = new bool[ ((( int )PriceMatrixFields.MaxPriceMatrixFields ) + 1 ) ];

                if( _startupParameters.IsNational == true )
                {
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTemporary ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFSS ] = false;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBIG4 ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsVA ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBOP ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsCMOP ] = false;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDOD ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsHHS ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS2 ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDIHS ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsNIH ] = false;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsPHS ] = false;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH1 ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH2 ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTMOP ] = false;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsUSCG ] = false;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFHCC ] = true;

                    // national BPA ( more like FSS but without BIG4 )
                    if( _startupParameters.IsBPA == true )
                    {
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTemporary ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFSS ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBIG4 ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsVA ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBOP ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsCMOP ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDOD ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsHHS ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS2 ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDIHS ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsNIH ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsPHS ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH1 ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH2 ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTMOP ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsUSCG ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFHCC ] = true;
                    }
                }
                else // fss
                {
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTemporary ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFSS ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBIG4 ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsVA ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBOP ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsCMOP ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDOD ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsHHS ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS2 ] = false;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDIHS ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsNIH ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsPHS ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH ] = false;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH1 ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH2 ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTMOP ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsUSCG ] = true;
                    _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFHCC ] = true;

                    // FSS BPA
                    if( _startupParameters.IsBPA == true )
                    {
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTemporary ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFSS ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBIG4 ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsVA ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBOP ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsCMOP ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDOD ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsHHS ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS2 ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDIHS ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsNIH ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsPHS ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH ] = false;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH1 ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH2 ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTMOP ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsUSCG ] = true;
                        _documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFHCC ] = true;
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
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsFSS ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFSS ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsBIG4 ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBIG4 ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsVA ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsVA ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsBOP ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsBOP ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsCMOP ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsCMOP ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsDOD ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDOD ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsHHS ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsHHS ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsIHS ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsIHS2 ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsIHS2 ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsDIHS ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsDIHS ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsNIH ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsNIH ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsPHS ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsPHS ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsSVH ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsSVH1 ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH1 ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsSVH2 ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsSVH2 ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsTMOP ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsTMOP ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsUSCG ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsUSCG ];
                priceGridViewRow.Cells[ ( ( int )PriceMatrixFields.IsFHCC ) + PriceMatrixFieldOffset ].Enabled = ( bool )_documentTypePriceMatrix[ ( int )PriceMatrixFields.IsFHCC ];
            }
            else
            {
                throw new Exception( "Unexpected null value for _documentTypePriceMatrix." );
            }
        }

 

        // disable the search and other non-edit controls before going into edit mode
        private void EnableControlsForItemEditMode( bool bEnabled )
        {
            Button searcbButton = ( Button )drugItemForm.FindControl( "SearchButton" );
            if( searcbButton != null )
            {
                searcbButton.Enabled = bEnabled;
            }

            Button clearSearchButton = ( Button )drugItemForm.FindControl( "ClearSearchButton" );
            if( clearSearchButton != null )
            {
                clearSearchButton.Enabled = bEnabled;
            }

            VA.NAC.NACCMBrowser.BrowserObj.TextBox itemSearchTextBox = ( VA.NAC.NACCMBrowser.BrowserObj.TextBox )drugItemForm.FindControl( "ItemSearchTextBox" );
            if( itemSearchTextBox != null )
            {
                itemSearchTextBox.Enabled = bEnabled;
            }

            Label itemSearchLabel = ( Label )drugItemForm.FindControl( "ItemSearchLabel" );
            if( itemSearchLabel != null )
            {
                itemSearchLabel.Enabled = bEnabled;
            }

            Button addNewItemButton = ( Button )drugItemForm.FindControl( "AddNewItemButton" );
            if( addNewItemButton != null )
            {
                addNewItemButton.Enabled = bEnabled;
            }

            RadioButtonList itemFilterRadioButtonList = ( RadioButtonList )drugItemForm.FindControl( "ItemFilterRadioButtonList" );
            if( itemFilterRadioButtonList != null )
            {
                itemFilterRadioButtonList.Enabled = bEnabled;
            }
        }

        private void ClearSessionVariables()
        {
            Session[ "DrugItemStartupParameters" ] = null;
            Session[ "DrugItemGridContext" ] = null;
            Session[ "DrugItemsDataSource" ] = null;
            Session[ "DrugItemPricesDataSource" ] = null;
            Session[ "DrugItemsGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedItemId" ] = null;
            Session[ "LastInsertedItemId" ] = null;
            Session[ "DrugItemPricesGridViewSelectedIndex" ] = null;
            Session[ "LastUpdatedItemPriceId" ] = null;
            Session[ "LastInsertedDrugItemPriceId" ] = null;
            Session[ "ModificationStatusIdParameter" ] = null;
            Session[ "ParentDrugItemsDataSource" ] = null;
            Session[ "IsSelectedDrugItemDualPricer" ] = null;
            Session[ "IsInSearchMode" ] = null;
            Session[ "DestinationDocumentUsedInCopyItem" ] = null;
            Session[ "DocumentTypePriceMatrix" ] = null;
            Session[ "DrugItemPriceVATandemSelectionRowIndex" ] = null;
            Session[ "SelectedDrugItemIncludedFETAmount" ] = null;

        }

        private void AddClientCloseEvent()
        {
            string closeFunctionText = "";

                closeFunctionText = " RefreshParent2();";
           
            // old R1 verion of close no longer used:  closeFunctionText = " RefreshParent(); CloseWindow();";            

            formCloseButton.Attributes.Add( "onclick", closeFunctionText );
        }

        private void InitContextMenus()
        {
            if( Page.IsPostBack != true )
                DrugItemsGridView.ContextMenuItemCommand += new ContextMenuCommandEventHandler( DrugItemsGridView_ContextMenuItemCommand );
        }

        private void InitItemGridContext()
        {
            if( Page.Session[ "DrugItemGridContext" ] == null )
            {
                _itemGridContext = new ItemGridContext();

                _itemGridContext.IsDiscontinued = false;

                Session[ "DrugItemGridContext" ] = _itemGridContext;
            }
            else
            {
                _itemGridContext = ( ItemGridContext )Session[ "DrugItemGridContext" ];
            }

        }

        private void LoadStartupParms()
        {
          //  this.ClientScript.RegisterStartupScript( this.GetType(), "scrollDiv", "scrollTo( 'DrugItemsGridViewDiv', '" + scrollPos.ClientID + "');", true );
         //   RegisterScrollScript(); 

            if( Page.Session[ "DrugItemStartupParameters" ] == null )
            {
                _startupParameters = new DrugItemStartupParameters();

                if( Page.Session[ "CurrentDocument" ] != null )
                {
                    CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
                    _startupParameters.ContractNumber = currentDocument.ContractNumber;
                    
                    if( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Active )
                    {
                        if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItems ) == true ||
                            ( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmBPAItems ) == true &&
                            currentDocument.IsBPA( currentDocument.ScheduleNumber ) == true ) )
                        {
                            _startupParameters.IsItemEditable = true;
                        }
                        else
                        {
                            _startupParameters.IsItemEditable = false;
                        }

                        if ( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemDetailsCO ) == true )
                        {
                            _startupParameters.IsItemDetailsCOEditable = true;
                        }
                        else
                        {
                            _startupParameters.IsItemDetailsCOEditable = false;
                        }

                        if ( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemDetailsPBM ) == true )
                        {
                            _startupParameters.IsItemDetailsPBMEditable = true;
                        }
                        else
                        {
                            _startupParameters.IsItemDetailsPBMEditable = false;
                        }

                        _startupParameters.IsPriceEditable = currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmPrices ) == true ? true : false;
                    }
                    else // block editing of expired contracts
                    {
                        _startupParameters.IsItemEditable = false;
                        _startupParameters.IsItemDetailsCOEditable = false;
                        _startupParameters.IsItemDetailsPBMEditable = false;
                        _startupParameters.IsPriceEditable = false;
                    }

                    _startupParameters.IsNational = ( currentDocument.Division == CurrentDocument.Divisions.National ) ? true : false;  //DocumentType == CurrentDocument.DocumentTypes.BPA ) ? true : false;
                    _startupParameters.VendorName = currentDocument.VendorName;
                    _startupParameters.IsBPA = currentDocument.IsBPA( currentDocument.ScheduleNumber ); // includes FSS BPA

                }
                else // should not occur
                {
                    _startupParameters.ContractNumber = Request.QueryString.Get( "ContractNumber" );
                    _startupParameters.IsItemEditable = ( Request.QueryString.Get( "Edit" ).ToString() == "Y" ) ? true : false;
                    _startupParameters.IsPriceEditable = ( Request.QueryString.Get( "Edit" ).ToString() == "Y" ) ? true : false;
                    _startupParameters.IsNational = ( Request.QueryString.Get( "National" ).ToString() == "Y" ) ? true : false;
                    _startupParameters.VendorName = CMGlobals.RestoreQuote( Request.QueryString.Get( "VendorName" ), "^" );
                }

                Session[ "DrugItemStartupParameters" ] = _startupParameters;
            }
            else
            {
                _startupParameters = ( DrugItemStartupParameters )Page.Session[ "DrugItemStartupParameters" ];
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
        }

        //private void AddDrugItemsDataSourceEvents( DocumentDataSource ds )
        //{
        //    ds.Inserted += new SqlDataSourceStatusEventHandler( _drugItemsDataSource_Inserted );
        //    ds.Updated += new SqlDataSourceStatusEventHandler( _drugItemsDataSource_Updated );
        //}

        private void LoadDrugItems()
        {
            if( Page.Session[ "DrugItemsDataSource" ] == null )
            {
                _drugItemsDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, true );
                _drugItemsDataSource.ID = "DrugItemsDataSource";
                _drugItemsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _drugItemsDataSource.SelectCommand = "SelectDrugItemsForFSSContract";

                _drugItemsDataSource.UpdateCommand = "UpdateFSSDrugItem"; // "UpdateDrugItemForFSSContract";

                _drugItemsDataSource.InsertCommand = "InsertFSSDrugItem"; // "InsertDrugItemForFSSContract";

                _drugItemsDataSource.SetEventOwnerName( "DrugItems" );
                _drugItemsDataSource.Inserted += new SqlDataSourceStatusEventHandler( _drugItemsDataSource_Inserted );
                _drugItemsDataSource.Updated += new SqlDataSourceStatusEventHandler( _drugItemsDataSource_Updated );
                
                _drugItemsDataSource.DeleteCommand = "DeleteFSSItemAndPrices"; // "DeleteDrugItemFromContract";
        //        _drugItemDataSource.Deleted += new SqlDataSourceStatusEventHandler( _addressDataSource_Deleted );

                CreateDrugItemDataSourceParameters();

                _drugItemsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _drugItemsDataSource.SelectParameters.Add( _contractNumberParameter );
                _contractNumberParameter.DefaultValue = _startupParameters.ContractNumber;
                _drugItemsDataSource.SelectParameters.Add( _withAddParameter );
                _withAddParameter.DefaultValue = "false"; // not adding
                _drugItemsDataSource.SelectParameters.Add( _coveredSelectionCriteriaParameter );
                _coveredSelectionCriteriaParameter.DefaultValue = "B"; // Both
                _isBPAParameter.DefaultValue = _startupParameters.IsBPA.ToString();
                _drugItemsDataSource.SelectParameters.Add( _isBPAParameter );

                _drugItemsDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _drugItemsDataSource.UpdateParameters.Add( _contractNumberParameter );
                _drugItemsDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
                _drugItemsDataSource.UpdateParameters.Add( _drugItemIdForItemsParameter );
                _drugItemsDataSource.UpdateParameters.Add( _drugItemNDCIdParameter );
                _drugItemsDataSource.UpdateParameters.Add( _fdaAssignedLabelerCodeParameter );
                _drugItemsDataSource.UpdateParameters.Add( _productCodeParameter );
                _drugItemsDataSource.UpdateParameters.Add( _packageCodeParameter );
                _drugItemsDataSource.UpdateParameters.Add( _coveredParameter );
                _drugItemsDataSource.UpdateParameters.Add( _genericParameter );
                _drugItemsDataSource.UpdateParameters.Add( _tradeNameParameter );
                _drugItemsDataSource.UpdateParameters.Add( _dispensingUnitParameter );
                _drugItemsDataSource.UpdateParameters.Add( _packageDescriptionParameter );
                _drugItemsDataSource.UpdateParameters.Add( _parentDrugItemIdParameter );
                _drugItemsDataSource.UpdateParameters.Add( _isBPAParameter );

                _drugItemsDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
                _drugItemsDataSource.InsertParameters.Add( _contractNumberParameter );
                _drugItemsDataSource.InsertParameters.Add( _fdaAssignedLabelerCodeParameter );
                _drugItemsDataSource.InsertParameters.Add( _productCodeParameter );
                _drugItemsDataSource.InsertParameters.Add( _packageCodeParameter );
                _drugItemsDataSource.InsertParameters.Add( _coveredParameter );
                _drugItemsDataSource.InsertParameters.Add( _genericParameter );
                _drugItemsDataSource.InsertParameters.Add( _tradeNameParameter );
                _drugItemsDataSource.InsertParameters.Add( _dispensingUnitParameter );
                _drugItemsDataSource.InsertParameters.Add( _packageDescriptionParameter );
                _drugItemsDataSource.InsertParameters.Add( _drugItemIdForItemInsertParameter );
                _drugItemsDataSource.InsertParameters.Add( _drugItemNDCIdForItemInsertParameter );
                _drugItemsDataSource.InsertParameters.Add( _modificationStatusIdParameter );
                _drugItemsDataSource.InsertParameters.Add( _parentDrugItemIdParameter );
                _drugItemsDataSource.InsertParameters.Add( _isBPAParameter );

                _drugItemsDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
                _drugItemsDataSource.DeleteParameters.Add( _contractNumberParameter );
                _drugItemsDataSource.DeleteParameters.Add( _drugItemIdForItemsParameter );
                _drugItemsDataSource.DeleteParameters.Add( _modificationStatusIdParameter );

                // save to session
                Page.Session[ "DrugItemsDataSource" ] = _drugItemsDataSource;
            }
            else
            {
                _drugItemsDataSource = ( DocumentDataSource )Page.Session[ "DrugItemsDataSource" ];
                _drugItemsDataSource.RestoreDelegatesAfterDeserialization( this, "DrugItems" );

                RestoreDrugItemDataSourceParameters( _drugItemsDataSource );
            }

            DrugItemsGridView.DataSource = _drugItemsDataSource;

            if( _startupParameters.IsBPA == true )
            {
                if( Page.Session[ "ParentDrugItemsDataSource" ] == null )
                {
                    _parentDrugItemsDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, false );
                    _parentDrugItemsDataSource.ID = "ParentDrugItemsDataSource";
                    _parentDrugItemsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                    _parentDrugItemsDataSource.SelectCommand = "GetParentDrugItems";
                    _parentDrugItemsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                    _parentDrugItemsDataSource.SetEventOwnerName( "ParentDrugItems" );

                    _parentDrugItemsDataSource.SelectParameters.Add( _contractNumberParameter );
                    _contractNumberParameter.DefaultValue = _startupParameters.ContractNumber;

                    Page.Session[ "ParentDrugItemsDataSource" ] = _parentDrugItemsDataSource;
                }
                else
                {
                    _parentDrugItemsDataSource = ( DocumentDataSource )Page.Session[ "ParentDrugItemsDataSource" ];
                    _parentDrugItemsDataSource.RestoreDelegatesAfterDeserialization( this, "ParentDrugItems" );
                }

                // bound during grid row binding
            }
        }

        private void LoadDrugItemPrices()
        {
            if( Page.Session[ "DrugItemPricesDataSource" ] == null )
            {
                _drugItemPricesDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, true );
                _drugItemPricesDataSource.ID = "DrugItemPricesDataSource";
                _drugItemPricesDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _drugItemPricesDataSource.SelectCommand = "SelectDrugItemPricesForFSSItem";
                
                CreateDrugItemPricesDataSourceParameters();

                _drugItemPricesDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _drugItemPricesDataSource.SelectParameters.Add( _drugItemIdForPricesParameter );
                _drugItemPricesDataSource.SelectParameters.Add( _contractNumberParameter );
                _contractNumberParameter.DefaultValue = _startupParameters.ContractNumber;
                _drugItemPricesDataSource.SelectParameters.Add( _withAddPriceParameter );
                _withAddPriceParameter.DefaultValue = "false"; // not adding
                _drugItemPricesDataSource.SelectParameters.Add( _futureHistoricalSelectionCriteriaParameter );
                _futureHistoricalSelectionCriteriaParameter.DefaultValue = "A"; // Active

                _drugItemPricesDataSource.UpdateCommand = "UpdateFSSDrugItemPrice";
                _drugItemPricesDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _drugItemPricesDataSource.SetEventOwnerName( "DrugItemPrices" );
                _drugItemPricesDataSource.Updated += new SqlDataSourceStatusEventHandler( _drugItemPricesDataSource_Updated );

                _drugItemPricesDataSource.UpdateParameters.Add( _modificationStatusIdParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _contractNumberParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _drugItemPriceIdParameter );
            //    _drugItemPricesDataSource.UpdateParameters.Add( _priceIdParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _priceStartDateParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _priceEndDateParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _priceParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isTemporaryParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isFSSParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isBIG4Parameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isVAParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isBOPParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isCMOPParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isDODParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isHHSParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isIHSParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isIHS2Parameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isDIHSParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isNIHParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isPHSParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isSVHParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isSVH1Parameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isSVH2Parameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isTMOPParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isUSCGParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _isFHCCParameter );
                _drugItemPricesDataSource.UpdateParameters.Add( _drugItemSubItemIdParameter );


                _drugItemPricesDataSource.DeleteCommand = "DeleteFSSPriceForItemPriceId";  // "DeletePriceForItemPriceId"; 
                _drugItemPricesDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
                _drugItemPricesDataSource.DeleteParameters.Add( _drugItemPriceIdParameter );
                _drugItemPricesDataSource.DeleteParameters.Add( _contractNumberParameter );
                _drugItemPricesDataSource.DeleteParameters.Add( _modificationStatusIdParameter );

                _drugItemPricesDataSource.InsertCommand = "InsertFSSDrugItemPrice";
                _drugItemPricesDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
                _drugItemPricesDataSource.Inserted += new SqlDataSourceStatusEventHandler( _drugItemPricesDataSource_Inserted );

                _drugItemPricesDataSource.InsertParameters.Add( _contractNumberParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _drugItemIdForPricesParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _priceStartDateParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _priceEndDateParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _priceParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isTemporaryParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isFSSParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isBIG4Parameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isVAParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isBOPParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isCMOPParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isDODParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isHHSParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isIHSParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isIHS2Parameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isDIHSParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isNIHParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isPHSParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isSVHParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isSVH1Parameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isSVH2Parameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isTMOPParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isUSCGParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _isFHCCParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _modificationStatusIdParameter );
                _drugItemPricesDataSource.InsertParameters.Add( _drugItemSubItemIdParameter );

                _drugItemPricesDataSource.InsertParameters.Add( _drugItemPriceIdForPriceInsertParameter );
        //        _drugItemPricesDataSource.InsertParameters.Add( _priceIdForPriceInsertParameter );

                // save to session
                Page.Session[ "DrugItemPricesDataSource" ] = _drugItemPricesDataSource;
            }
            else
            {
                _drugItemPricesDataSource = ( DocumentDataSource )Page.Session[ "DrugItemPricesDataSource" ];

                _drugItemPricesDataSource.RestoreDelegatesAfterDeserialization( this, "DrugItemPrices" );

                RestoreDrugItemPricesDataSourceParameters( _drugItemPricesDataSource );
            }

            DrugItemPricesGridView.DataSource = _drugItemPricesDataSource;


            if( Session[ "SubItemIdentifierDataSource" ] == null )
            {
                _subItemIdentifierDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, false );
                _subItemIdentifierDataSource.ID = "SubItemIdentifierDataSource";
                _subItemIdentifierDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _subItemIdentifierDataSource.SelectCommand = "SelectFSSDrugItemSubItemsForPrice";
                _subItemIdentifierDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _subItemIdentifierDataSource.SetEventOwnerName( "SubItemIdentifier" );
                _subItemIdentifierDataSource.SelectParameters.Add( _drugItemIdForPricesParameter ); // value was set above

         // do not save       Session[ "SubItemIdentifierDataSource" ] = _subItemIdentifierDataSource;
            }
            else
            {
                _subItemIdentifierDataSource = ( DocumentDataSource )Session[ "SubItemIdentifierDataSource" ];
                _subItemIdentifierDataSource.RestoreDelegatesAfterDeserialization( this, "SubItemIdentifier" );
                // id parameter is shared and was restored above
            }

        }
 
        private void BindDrugItemHeaderAndGrid()
        {
            RefreshMainHeader( false );

            try
            {
                // bind
                DrugItemsGridView.DataBind();

               
//                SetDrugItemsGridViewSelectedItem( 0 );
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }

        private void BindDrugItemPricesHeaderAndGrid()
        {

            SetDrugItemPriceDataSourceParameterValues();

            SaveSelectedItemInfoIntoSession( DrugItemsGridView.SelectedIndex );  //$$$ 2/5/2016 moved this line up one so session can be set for header display

            SetItemPriceHeaderInfo( DrugItemsGridView.SelectedIndex );
           
            try
            {
                // bind
                DrugItemPricesGridView.DataBind();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

        }

        //private void RestoreStartupParms()
        //{
        //    _startupParameters = ( DrugItemStartupParameters )Page.Session[ "DrugItemStartupParameters" ];

        //}

        //private void RestoreDrugItemPrices()
        //{
//            _drugItemPricesDataSource = ( DocumentDataSource )Page.Session[ "DrugItemPricesDataSource" ];

 //           RestoreDrugItemPricesDataSourceParameters( _drugItemPricesDataSource );

  //          DrugItemPricesGridView.DataSource = _drugItemPricesDataSource;

  //          SetDrugItemPriceDataSourceParameterValues();

 //           SetItemPriceHeaderInfo( DrugItemsGridView.SelectedIndex );

   //     }

        private void CreateDrugItemDataSourceParameters()
        {
            _contractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );

            _drugItemIdForItemsParameter = new Parameter( "DrugItemId", TypeCode.Int32 );
            _drugItemNDCIdParameter = new Parameter( "DrugItemNDCId", TypeCode.Int32 );
            _fdaAssignedLabelerCodeParameter = new Parameter( "FdaAssignedLabelerCode", TypeCode.String );
            _fdaAssignedLabelerCodeParameter.Size = 5;
            _productCodeParameter = new Parameter( "ProductCode", TypeCode.String );
            _productCodeParameter.Size = 4;
            _packageCodeParameter = new Parameter( "PackageCode", TypeCode.String );
            _packageCodeParameter.Size = 2;
            _coveredParameter = new Parameter( "Covered", TypeCode.String );
            _genericParameter = new Parameter( "Generic", TypeCode.String );
            _tradeNameParameter = new Parameter( "TradeName", TypeCode.String );
            _dispensingUnitParameter = new Parameter( "DispensingUnit", TypeCode.String );
            _packageDescriptionParameter = new Parameter( "PackageDescription", TypeCode.String );
            _withAddParameter = new Parameter( "WithAdd", TypeCode.Boolean );
            _coveredSelectionCriteriaParameter = new Parameter( "CoveredSelectionCriteria", TypeCode.String );
            _isBPAParameter = new Parameter( "IsBPA", TypeCode.Boolean );

            _drugItemIdForItemInsertParameter = new Parameter( "DrugItemId", TypeCode.Int32 );
            _drugItemIdForItemInsertParameter.Direction = ParameterDirection.Output;
            _drugItemNDCIdForItemInsertParameter = new Parameter( "DrugItemNDCId", TypeCode.Int32 );
            _drugItemNDCIdForItemInsertParameter.Direction = ParameterDirection.Output;

            _discontinuationDateParameter = new Parameter( "DiscontinuationDate", TypeCode.DateTime );
            _parentDrugItemIdParameter = new Parameter( "ParentDrugItemId", TypeCode.Int32 );
        }

        private void RestoreDrugItemDataSourceParameters( DocumentDataSource drugItemDataSource )
        {
            // select
            _withAddParameter = drugItemDataSource.SelectParameters[ "WithAdd" ];
            _coveredSelectionCriteriaParameter = drugItemDataSource.SelectParameters[ "CoveredSelectionCriteria" ];
            _isBPAParameter = drugItemDataSource.SelectParameters[ "IsBPA" ];

            // select, update and insert
            _contractNumberParameter = drugItemDataSource.SelectParameters[ "ContractNumber" ];

            // update and insert
            _drugItemIdForItemsParameter = drugItemDataSource.UpdateParameters[ "DrugItemId" ];
            _drugItemNDCIdParameter = drugItemDataSource.UpdateParameters[ "DrugItemNDCId" ];
            _fdaAssignedLabelerCodeParameter = drugItemDataSource.UpdateParameters[ "FdaAssignedLabelerCode" ];
            _productCodeParameter = drugItemDataSource.UpdateParameters[ "ProductCode" ];
            _packageCodeParameter = drugItemDataSource.UpdateParameters[ "PackageCode" ];
            _coveredParameter = drugItemDataSource.UpdateParameters[ "Covered" ];
            _genericParameter = drugItemDataSource.UpdateParameters[ "Generic" ];
            _tradeNameParameter = drugItemDataSource.UpdateParameters[ "TradeName" ];
            _dispensingUnitParameter = drugItemDataSource.UpdateParameters[ "DispensingUnit" ];
            _packageDescriptionParameter = drugItemDataSource.UpdateParameters[ "PackageDescription" ];
            _parentDrugItemIdParameter = drugItemDataSource.UpdateParameters[ "ParentDrugItemId" ];

            // insert
            _drugItemIdForItemInsertParameter = drugItemDataSource.InsertParameters[ "DrugItemId" ];
            _drugItemNDCIdForItemInsertParameter = drugItemDataSource.InsertParameters[ "DrugItemNDCId" ];

        }


        private void RefreshMainHeader( bool bWithRefresh )
        {
            MainHeaderTitleLabel2.Text = string.Format( "For Contract {0}", _startupParameters.ContractNumber );
            MainHeaderTitleLabel3.Text = string.Format( "{0}", _startupParameters.VendorName );

            MainHeaderItemCount.Text = string.Format( "{0} Total Items", GetCurrentItemCount( bWithRefresh ));
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
                totalItems = currentDocument.DrugItemCount;
            }
            return ( totalItems );
        }

        protected void DrugItemsGridView_Init( object sender, EventArgs e )
        {
            GridView drugItemsGridView = ( GridView )sender;
            drugItemsGridView.SetContextMenu( ItemContextMenu );

            drugItemsGridView.ContextMenuItemCommand += new ContextMenuCommandEventHandler( DrugItemsGridView_ContextMenuItemCommand );

            ItemContextMenu.ContextMenuItems.Add( new ContextMenuItem( "Change NDC", "NDCChangeCommand" ) );
            ItemContextMenu.ContextMenuItems.Add( new ContextMenuItem( "Copy Item", "ItemCopyCommand" ) );
            ItemContextMenu.ContextMenuItems.Add( new ContextMenuItem( "Copy Item To Contract", "ItemCopyToContractCommand" ) );
            ItemContextMenu.ContextMenuItems.Add( new ContextMenuItem( "Discontinue", "ItemDiscontinueCommand" ) );

            ContextMenuItem restoreMenuItem = new ContextMenuItem( "Restore", "RestoreDiscontinuedItemCommand" );
            restoreMenuItem.ClientOnClick = "presentConfirmationMessage('Restore the selected item as an active item on this contract?')";
            ItemContextMenu.ContextMenuItems.Add( restoreMenuItem );
        }
        
        protected void DrugItemsGridView_PreRender( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
                HighlightDrugItemRow( 0 );

            GridView drugItemsGridView = ( GridView )sender;
            GridViewRow headerRow = drugItemsGridView.HeaderRow;

            // no drug items
            if( headerRow == null )
                return;

            if( _startupParameters != null )
            {
                if( _startupParameters.IsBPA == true )
                {
                    // hide hasBPA indicator
                    headerRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( HasBPAFieldNumber ) ].Visible = false;
                }
            }

            if( ShowParentItemCombo() == true )
            {
                // hide DrugItemNDCId
                headerRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DrugItemNDCIdFieldNumber ) ].Visible = false;

                // hide ParentDrugItemId
                headerRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemIdFieldNumber ) ].Visible = false;

                // hide dual price designation
                headerRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DualPriceDesignationFieldNumber ) ].Visible = false;

                foreach( GridViewRow gridViewRow in drugItemsGridView.Rows )
                {
                    // hide DrugItemNDCId
                    gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DrugItemNDCIdFieldNumber ) ].Visible = false;

                    // hide ParentDrugItemId
                    gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemIdFieldNumber ) ].Visible = false;

                    // hide dual price designation
                    gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DualPriceDesignationFieldNumber ) ].Visible = false;

                    // hide HasBPA indicator
                    gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( HasBPAFieldNumber ) ].Visible = false;
                }
            }
            else
            {
                headerRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemDropDownListFieldNumber ) ].Visible = false;

                foreach( GridViewRow gridViewRow in drugItemsGridView.Rows )
                {
                    gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemDropDownListFieldNumber ) ].Visible = false;

                    if( _startupParameters != null )
                    {
                        if( _startupParameters.IsBPA == true )
                        {
                            // hide hasBPA indicator
                            headerRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( HasBPAFieldNumber ) ].Visible = false;
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

        protected void DiscontinueItemButton_DataBinding( object sender, EventArgs e )
        {
            Button discontinueItemButton = ( Button )sender;
            if( discontinueItemButton != null )
                MultiLineButtonText( discontinueItemButton, new string[] { "Discontinue", "Item" } );
        }

        protected void DrugItemsGridView_RowDataBound( object sender, GridViewRowEventArgs e )
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
                        Label coveredLabel = ( Label )e.Row.FindControl( "coveredLabel" );
                        if( coveredLabel.Text.CompareTo( "T" ) == 0 )
                        {
                            coveredLabel.ForeColor = Color.Crimson;
                            coveredLabel.Text = "Covered";
                        }
                        else
                        {
                            coveredLabel.ForeColor = Color.Coral;
                            coveredLabel.Text = "Non-covered";
                        }

                        Button removeItemAndPricesButton = null;
                        removeItemAndPricesButton = ( Button )e.Row.FindControl( "RemoveItemAndPricesButton" );
                        if( removeItemAndPricesButton != null )
                        {
                            removeItemAndPricesButton.Attributes.Add( "onclick", "presentConfirmationMessage('Permanently delete the selected item and its associated prices?')" );
                        }

                        Button discontinueItemButton = null;
                        discontinueItemButton = ( Button )e.Row.FindControl( "DiscontinueItemButton" );
                        if( discontinueItemButton != null )
                        {
                            discontinueItemButton.Attributes.Add( "onclick", "presentPromptMessage('Please enter the date to discontinue the item (mm/dd/yyyy):')" );
                        }

                        if( currentDocument != null )
                        {
                            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemRemove ) != true || _itemGridContext.IsDiscontinued == true )
                            {
                                if( removeItemAndPricesButton != null )
                                {
                                    removeItemAndPricesButton.Enabled = false;
                                }
                            }
                            if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.PharmItemDiscontinue ) != true )
                            {
                                if( discontinueItemButton != null )
                                {
                                    discontinueItemButton.Enabled = false;
                                }
                            }
                        }

                        if( _startupParameters.IsItemEditable == false || _itemGridContext.IsDiscontinued == true )
                        {
                            //if( removeItemAndPricesButton != null )
                            //{
                            //    removeItemAndPricesButton.Enabled = false;
                            //}
                            //if( discontinueItemButton != null )
                            //{
                            //    discontinueItemButton.Enabled = false;
                            //}

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

                        // go into edit mode for the new row ( if any )
                        //if( e.Row.DataItem != null )
                        //{
                        //    string isNewBlankRow = ( ( DataRowView )e.Row.DataItem )[ "IsNewBlankRow" ].ToString();
                        //    if( isNewBlankRow.CompareTo( "1" ) == 0 )
                        //    {
                        //        gv.EditIndex = e.Row.RowIndex;
                        //    }
                        //}
                    }
                    else // bind parent item ddl during edit
                    {
                        DropDownList parentItemDropDownList = ( DropDownList )e.Row.FindControl( "parentItemDropDownList" );
                        parentItemDropDownList.DataSource = _parentDrugItemsDataSource;
                        parentItemDropDownList.DataBind();
                    }
                }

                //// hide DrugItemNDCId
                //e.Row.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DrugItemNDCIdFieldNumber ) ].Visible = false;

                //// hide ParentDrugItemId
                //e.Row.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemIdFieldNumber ) ].Visible = false;

            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void parentItemLabel_OnDataBinding( object sender, EventArgs e )
        {
            Label parentItemLabel = ( Label )sender;
            GridViewRow gridViewRow = ( GridViewRow )parentItemLabel.NamingContainer;
            int parentDrugItemId = -1;
            string parentDrugItemOverallDescription = "";
            bool bSuccess = false;

            DrugItemDB drugItemDB = ( DrugItemDB )Page.Session[ "DrugItemDB" ];
            if( drugItemDB != null )
            {
                if( gridViewRow.DataItem != null )
                {
                    string parentDrugItemIdString = ( ( DataRowView )gridViewRow.DataItem )[ "ParentDrugItemId" ].ToString();
                    if( parentDrugItemIdString != null )
                    {
                        if( int.TryParse( parentDrugItemIdString, out parentDrugItemId ) )
                        {
                            bSuccess = drugItemDB.GetParentDrugItemDescription( parentDrugItemId, out parentDrugItemOverallDescription );
                            if( bSuccess == true )
                            {
                                parentItemLabel.Text = parentDrugItemOverallDescription;
                            }
                        }
                    }
                }
            }
        }

        protected void ParentItemDropDownList_DataBound( object sender, EventArgs e )
        {
            DropDownList parentItemDropDownList = ( DropDownList )sender;
            GridViewRow gridViewRow = ( GridViewRow )parentItemDropDownList.NamingContainer;
            int parentDrugItemId = -1;

            if( gridViewRow.DataItem != null )
            {
                string parentDrugItemIdString = ( ( DataRowView )gridViewRow.DataItem )[ "ParentDrugItemId" ].ToString();
                if( parentDrugItemIdString != null )
                {
                    if( int.TryParse( parentDrugItemIdString, out parentDrugItemId ) )
                    {
                        ListItem listItem = parentItemDropDownList.Items.FindByValue( parentDrugItemId.ToString() );
                        if( listItem != null )
                            listItem.Selected = true;
                    }
                }
            }

        }

        protected void DrugItemsGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;

                // upon first entry after document creation, 
                // e.Row.Cells.Count is 1, make sure count is ok before proceeding
                if( e.Row.Cells.Count > ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemDropDownListFieldNumber ) &&
                    e.Row.Cells.Count > ApplyRightHandColumnOffsetBasedOnDocumentType( DrugItemNDCIdFieldNumber ) &&
                    e.Row.Cells.Count > ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemIdFieldNumber ) &&
                    e.Row.Cells.Count > ApplyRightHandColumnOffsetBasedOnDocumentType( DualPriceDesignationFieldNumber ) &&
                    e.Row.Cells.Count > ApplyRightHandColumnOffsetBasedOnDocumentType( HasBPAFieldNumber ))
                {

                    if( ShowParentItemCombo() == false )
                    {
                        // hide ParentDrugItem drop down list
                        e.Row.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemDropDownListFieldNumber ) ].Visible = false;
                    }

                    if( _startupParameters != null )
                    {
                        if( _startupParameters.IsBPA == true )
                        {
                            // hide hasBPA indicator
                            e.Row.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( HasBPAFieldNumber ) ].Visible = false;

                            // disable most item cells from editing, since parent should be re-selected
                            DisableItemFieldsForDocumentTypeForRow( e.Row );
                        }
                    }
                    // hide DrugItemNDCId
                    e.Row.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DrugItemNDCIdFieldNumber ) ].Visible = false;

                    // hide ParentDrugItemId
                    e.Row.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemIdFieldNumber ) ].Visible = false;

                    // hide DualPriceDesignation
                    e.Row.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DualPriceDesignationFieldNumber ) ].Visible = false;
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        // called on row creation
        private void DisableItemFieldsForDocumentTypeForRow( GridViewRow itemGridViewRow )
        {
            if( itemGridViewRow.RowType == DataControlRowType.DataRow )
            {
                if(( itemGridViewRow.RowState & DataControlRowState.Edit ) == DataControlRowState.Edit )
                {
                    itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( FdaAssignedLabelerCodeFieldNumber ) ].Enabled = false;
                    itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ProductCodeFieldNumber ) ].Enabled = false;
                    itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( PackageCodeFieldNumber ) ].Enabled = false;
                    itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( CoveredIndicatorFieldNumber ) ].Enabled = false;
                    itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( GenericFieldNumber ) ].Enabled = false;
                    itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( TradeNameFieldNumber ) ].Enabled = false;
                    itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DispensingUnitFieldNumber ) ].Enabled = false;
                    itemGridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( PackageSizeFieldNumber ) ].Enabled = false;
                }
            }
        }

        private bool ShowParentItemCombo()
        {
            if( _startupParameters == null )
                return ( true ); // masking an error that happens when onrowcreated is called prior to load

            if( DrugItemsGridView.EditIndex >= 0 && _startupParameters.IsBPA == true )
                return ( true );
            else
                return ( false );
        }

        // select covered from drop down during edit
        protected void CoveredDropDownList_DataBound( object sender, EventArgs e )
        {
            DropDownList coveredDropDownList = ( DropDownList )sender;
            GridViewRow gridViewRow = ( GridViewRow )coveredDropDownList.NamingContainer;

            if( gridViewRow.DataItem != null )
            {
                string covered = ( ( DataRowView )gridViewRow.DataItem )[ "Covered" ].ToString();
                ListItem listItem = coveredDropDownList.Items.FindByValue( covered );
                if( listItem != null )
                    listItem.Selected = true;
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

        protected void DrugItemsGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            int i = 1;

        }

        void DrugItemsGridView_ContextMenuItemCommand( ContextMenu theMenu, ContextMenuCommandEventArgs args )
        {
            //$$$
            string commandName = args.CommandName;
            int itemIndex = args.GridViewRowId;
            int drugItemId = DrugItemsGridView.GetRowIdFromSelectedIndex( itemIndex, 0 );

            if( commandName.CompareTo( "NDCChangeCommand" ) == 0 )
            {
                // gather NDC Change parameters
                string fdaAssignedLabelerCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ), 0, false, "fdaAssignedLabelerCodeLabel" );
                string productCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ), 0, false, "productCodeLabel" );
                string packageCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ), 0, false, "packageCodeLabel" );
                string genericName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ), 0, false, "genericLabel" );
                string tradeName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ), 0, false, "tradeNameLabel" );
                int modificationStatusId = -1;

                HighlightDrugItemRow( itemIndex );

                // refresh the price list
                RefreshPricelist( itemIndex );

                OpenNDCChangeWindow( itemIndex, drugItemId, _startupParameters.ContractNumber, fdaAssignedLabelerCode, productCode, packageCode, genericName, tradeName, modificationStatusId  );

                RefreshMainHeader( true );

                // allow the update postback to occur
                InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else if( commandName.CompareTo( "ItemCopyCommand" ) == 0 )
            {
                // gather Item Copy parameters
                string fdaAssignedLabelerCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ), 0, false, "fdaAssignedLabelerCodeLabel" );
                string productCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ), 0, false, "productCodeLabel" );
                string packageCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ), 0, false, "packageCodeLabel" );
                string genericName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ), 0, false, "genericLabel" );
                string tradeName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ), 0, false, "tradeNameLabel" );
                string dispensingUnit = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 10 ), 0, false, "dispensingUnitLabel" );
                string packageDescription = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 11 ), 0, false, "packageSizeLabel" );
                int modificationStatusId = -1;

                HighlightDrugItemRow( itemIndex );

                // refresh the price list
                RefreshPricelist( itemIndex );

                OpenItemCopyWindow( itemIndex, drugItemId, _startupParameters.ContractNumber, fdaAssignedLabelerCode, productCode, packageCode, genericName, tradeName, modificationStatusId, dispensingUnit, packageDescription, ItemCopyWindowParms.CopyTypes.CopyLocal );

                RefreshMainHeader( true );

                // allow the update postback to occur
                InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else if( commandName.CompareTo( "ItemCopyToContractCommand" ) == 0 )
            {
                // gather Item Copy parameters
                string fdaAssignedLabelerCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ), 0, false, "fdaAssignedLabelerCodeLabel" );
                string productCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ), 0, false, "productCodeLabel" );
                string packageCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ), 0, false, "packageCodeLabel" );
                string genericName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ), 0, false, "genericLabel" );
                string tradeName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ), 0, false, "tradeNameLabel" );
                string dispensingUnit = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 10 ), 0, false, "dispensingUnitLabel" );
                string packageDescription = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 11 ), 0, false, "packageSizeLabel" );
                int modificationStatusId = -1;

                HighlightDrugItemRow( itemIndex );

                // refresh the price list
                RefreshPricelist( itemIndex );

                OpenItemCopyWindow( itemIndex, drugItemId, _startupParameters.ContractNumber, fdaAssignedLabelerCode, productCode, packageCode, genericName, tradeName, modificationStatusId, dispensingUnit, packageDescription, ItemCopyWindowParms.CopyTypes.CopyToContract );
            }
            else if( commandName.CompareTo( "ItemDiscontinueCommand" ) == 0 ) 
            {
                // gather Item Discontinue parameters
                string fdaAssignedLabelerCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ), 0, false, "fdaAssignedLabelerCodeLabel" );
                string productCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ), 0, false, "productCodeLabel" );
                string packageCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ), 0, false, "packageCodeLabel" );
                string genericName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ), 0, false, "genericLabel" );
                string tradeName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ), 0, false, "tradeNameLabel" );
                string dispensingUnit = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 10 ), 0, false, "dispensingUnitLabel" );
                string packageDescription = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 11 ), 0, false, "packageSizeLabel" );
                string discontinuationDateString = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 18 ), 0, false, "discontinuationDateLabel" );
                string discontinuationReasonString = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 19 ), 0, false, "discontinuationReasonLabel" );
                int modificationStatusId = -1;

                HighlightDrugItemRow( itemIndex );

                // refresh the price list
                RefreshPricelist( itemIndex );

                OpenItemDiscontinueWindow( itemIndex, drugItemId, _startupParameters.ContractNumber, fdaAssignedLabelerCode, productCode, packageCode, genericName, tradeName, modificationStatusId, dispensingUnit, packageDescription, discontinuationDateString, discontinuationReasonString, ItemDiscontinueWindowParms.DiscontinuationReasonCategories.FSS );

                RefreshMainHeader( true );

                // allow the update postback to occur
                InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else if( commandName.CompareTo( "RestoreDiscontinuedItemCommand" ) == 0 ) // $$$
            {
                // gather Item Discontinue parameters
                string fdaAssignedLabelerCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ), 0, false, "fdaAssignedLabelerCodeLabel" );
                string productCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ), 0, false, "productCodeLabel" );
                string packageCode = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ), 0, false, "packageCodeLabel" );
                string genericName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ), 0, false, "genericLabel" );
                string tradeName = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ), 0, false, "tradeNameLabel" );
                string dispensingUnit = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 10 ), 0, false, "dispensingUnitLabel" );
                string packageDescription = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 11 ), 0, false, "packageSizeLabel" );
                string discontinuationDateString = DrugItemsGridView.GetStringValueFromSelectedControl( itemIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 18 ), 0, false, "discontinuationDateLabel" );
                          
                int modificationStatusId = -1;

                bool bContinueWithRestore = false;
                bContinueWithRestore = GetConfirmationMessageResults();

                if( bContinueWithRestore == true )
                {
                    if( discontinuationDateString.Trim().Length > 0 )
                    {
 
                        int newRowIndex = RestoreDiscontinuedItem( DrugItemsGridView, itemIndex, drugItemId, _startupParameters.ContractNumber, fdaAssignedLabelerCode, productCode, packageCode, genericName, tradeName, modificationStatusId, dispensingUnit, packageDescription );

                        HighlightDrugItemRow( newRowIndex );
                        RefreshMainHeader( true );
                        RefreshPricelist( newRowIndex );
                    }
                    else
                    {
                        MsgBox.AlertFromUpdatePanel( Page, "Cannot restore an item that is not discontinued." );
                    }
               }
          
                // allow the update postback to occur
                InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        protected void DrugItemsGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedDrugItemId = -1;
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
                        selectedDrugItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                        HighlightDrugItemRow( itemIndex );
                        
                        // refresh the price list
                        RefreshPricelist( itemIndex );

                        OpenItemDetailsWindow( itemIndex, selectedDrugItemId, _startupParameters.ContractNumber );
                    }
                }
            }
            else if( e.CommandName.CompareTo( "RemoveItemAndItemPrices" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                bool bContinueWithDelete = false;
                bContinueWithDelete = GetConfirmationMessageResults();

                if( bContinueWithDelete == true )
                {
                    int newRowIndex = DeleteDrugItem( DrugItemsGridView, itemIndex, selectedDrugItemId );

                    HighlightDrugItemRow( newRowIndex );
                    RefreshMainHeader( true );
                    RefreshPricelist( newRowIndex );
                }
            }
            //else if( e.CommandName.CompareTo( "DiscontinueItem" ) == 0 )
            //{
            //    // argument is row index and row id
            //    string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
            //    itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
            //    selectedDrugItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

            //    string discontinuationDateString = "";
            //    DateTime discontinuationDate = DateTime.MinValue;
            //    discontinuationDateString = GetPromptMessageResults();

            //    int modificationStatusId = -1;

            //    if( discontinuationDateString.Trim().Length > 0 && discontinuationDateString.Contains( "null" ) != true )
            //    {
            //        if( DateTime.TryParse( discontinuationDateString, out discontinuationDate ) == true )
            //        {
            //            int newRowIndex = DiscontinueDrugItem( DrugItemsGridView, itemIndex, selectedDrugItemId, discontinuationDate, modificationStatusId );

            //            HighlightDrugItemRow( newRowIndex );
            //            RefreshMainHeader( true );
            //            RefreshPricelist( newRowIndex );
            //        }
            //        else
            //        {
            //            MsgBox.AlertFromUpdatePanel( Page, "Invalid date format." );
            //        }
            //    }
            //}
            else if( e.CommandName.CompareTo( "RefreshPriceList" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                HighlightDrugItemRow( itemIndex );

                // refresh the price list
                RefreshPricelist( itemIndex );
            }
            else if( e.CommandName.CompareTo( "EditItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                int revisedItemIndex = ClearSearchWithBind( selectedDrugItemId );

                HighlightDrugItemRow( revisedItemIndex );

                RefreshPricelist( revisedItemIndex );

                InitiateEditModeForItem( revisedItemIndex );

            }
            // save update or save insert
            else if( e.CommandName.CompareTo( "SaveItem" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                itemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

                string validationMessage = "";

                // validate the item before saving
                bool bIsItemOk = ValidateItemBeforeUpdate( DrugItemsGridView, itemIndex, selectedDrugItemId, ref validationMessage );

                if( bIsItemOk == true )
                {
                    // is this an insert or an update
                    int newOrUpdatedRowIndex = -1;
                    if( DrugItemsGridView.InsertRowActive == true )
                    {
                        newOrUpdatedRowIndex = InsertDrugItem( DrugItemsGridView, itemIndex );
                    }
                    else
                    {
                        newOrUpdatedRowIndex = UpdateDrugItem( DrugItemsGridView, itemIndex );
                    }

                    HighlightDrugItemRow( newOrUpdatedRowIndex );
                    RefreshMainHeader( true );
                    RefreshPricelist( newOrUpdatedRowIndex );
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
                selectedDrugItemId = Int32.Parse( commandArgs[ 1 ].ToString() );

            }
            // can trap editing commands here if needed, prior to individual editing events
        }

        private bool GetConfirmationMessageResults()
        {
            bool bConfirmationResults = false;
            string confirmationResultsString = "";

            HtmlInputHidden confirmationMessageResultsHiddenField = ( HtmlInputHidden )drugItemForm.FindControl( "confirmationMessageResults" );

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

            HtmlInputHidden promptMessageResultsHiddenField = ( HtmlInputHidden )drugItemForm.FindControl( "promptMessageResults" );

            if( promptMessageResultsHiddenField != null )
            {
                promptResultsString = promptMessageResultsHiddenField.Value;
                promptMessageResultsHiddenField.Value = "";
            }

            return ( promptResultsString );
        }

        protected void AddNewItemButton_OnClick( object sender, EventArgs e )
        {
            ClearSearch();

            DrugItemsGridView.Insert();

            _withAddParameter.DefaultValue = "true";

            DrugItemsGridView.DataBind();

            InitiateEditModeForItem( 0 );

            // allow the update postback to occur
            InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            HighlightDrugItemRow( 0 );

            RefreshPricelist( 0 );
        }

        protected void ParentItemDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList parentItemDropDownList = ( DropDownList )sender;

            GridViewRow gridViewRow = ( GridViewRow )parentItemDropDownList.NamingContainer;
            int parentDrugItemId = -1;

            ListItem selectedItem = parentItemDropDownList.SelectedItem;
            parentDrugItemId = int.Parse( selectedItem.Value );

            if( parentDrugItemId != -1 )
            {

                DrugItemDB drugItemDB = ( DrugItemDB )Session[ "DrugItemDB" ];
                if( drugItemDB != null )
                {
                    DataSet drugItemDetailsDataSet = null;
                    try
                    {
                        bool bSuccess = drugItemDB.GetDrugItemDetails( ref drugItemDetailsDataSet, _startupParameters.ContractNumber, parentDrugItemId );
                        if( bSuccess == false )
                        {
                            throw new Exception( drugItemDB.ErrorMessage );
                        }
                        else
                        {
                            // show the data
                            int count = drugItemDB.RowsReturned;

                            if( count == 1 )
                            {
                                DataRow currentRow = drugItemDetailsDataSet.Tables[ "DrugItemDetailsTable" ].Rows[ 0 ];

                                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ) ].Controls[ 1 ] ).Text = currentRow[ "FdaAssignedLabelerCode" ].ToString();
                                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ) ].Controls[ 1 ] ).Text = currentRow[ "ProductCode" ].ToString();
                                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ) ].Controls[ 1 ] ).Text = currentRow[ "PackageCode" ].ToString();
                                ( ( DropDownList )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 7 ) ].Controls[ 1 ] ).SelectedValue = currentRow[ "Covered" ].ToString();
                                ( ( VA.NAC.NACCMBrowser.BrowserObj.TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ) ].Controls[ 1 ] ).Text = currentRow[ "Generic" ].ToString();
                                ( ( VA.NAC.NACCMBrowser.BrowserObj.TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ) ].Controls[ 1 ] ).Text = currentRow[ "TradeName" ].ToString();
                                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 10 ) ].Controls[ 1 ] ).Text = currentRow[ "DispensingUnit" ].ToString();
                                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 11 ) ].Controls[ 1 ] ).Text = currentRow[ "PackageDescription" ].ToString();
                              //  ( ( Label )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DrugItemNDCIdFieldNumber ) ].Controls[ 1 ] ).Text = currentRow[ "DrugItemNDCId" ].ToString();
                                ( ( Label )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemIdFieldNumber ) ].Controls[ 1 ] ).Text = parentDrugItemId.ToString();

                                if( _startupParameters != null )
                                {
                                    if( _startupParameters.IsBPA == true )
                                    {
                                        gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( FdaAssignedLabelerCodeFieldNumber ) ].Enabled = false;
                                        gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ProductCodeFieldNumber ) ].Enabled = false;
                                        gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( PackageCodeFieldNumber ) ].Enabled = false;
                                        gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( CoveredIndicatorFieldNumber ) ].Enabled = false;
                                        gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( GenericFieldNumber ) ].Enabled = false;
                                        gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( TradeNameFieldNumber ) ].Enabled = false;
                                        gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DispensingUnitFieldNumber ) ].Enabled = false;
                                        gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( PackageSizeFieldNumber ) ].Enabled = false;
                                    }
                                }

                                gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( HasBPAFieldNumber ) ].Visible = false;

                                // hide DrugItemNDCId
                                gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DrugItemNDCIdFieldNumber ) ].Visible = false;

                                // hide ParentDrugItemId
                                gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemIdFieldNumber ) ].Visible = false;

                                // hide DualPriceDesignation
                                gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DualPriceDesignationFieldNumber ) ].Visible = false;

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
                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ) ].Controls[ 1 ] ).Text = "";
                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ) ].Controls[ 1 ] ).Text = "";
                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ) ].Controls[ 1 ] ).Text = "";
                ( ( DropDownList )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 7 ) ].Controls[ 1 ] ).SelectedValue = "F";
                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ) ].Controls[ 1 ] ).Text = "";
                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ) ].Controls[ 1 ] ).Text = "";
                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 10 ) ].Controls[ 1 ] ).Text = "";
                ( ( TextBox )gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( 11 ) ].Controls[ 1 ] ).Text = "";

                if( _startupParameters.IsBPA == true )
                {
                    // hide hasBPA indicator
                    gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( HasBPAFieldNumber ) ].Visible = false;

                }


                // hide DrugItemNDCId
                gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DrugItemNDCIdFieldNumber ) ].Visible = false;

                // hide ParentDrugItemId
                gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemIdFieldNumber ) ].Visible = false;

                // hide DualPriceDesignation
                gridViewRow.Cells[ ApplyRightHandColumnOffsetBasedOnDocumentType( DualPriceDesignationFieldNumber ) ].Visible = false;

                // allow the update postback to occur
                SelectParentItemForBPAUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

#endregion CommandsAndClicks

#region ItemInsertEditCancelFunctionsAndEvents


        protected void DrugItemsGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            ClearSearch();
            InitiateEditModeForItem( e.NewEditIndex );
        }

        private void InitiateEditModeForItem( int editIndex )
        {
            DrugItemsGridView.EditIndex = editIndex;

            // select the edited item also
            if( DrugItemsGridView.InsertRowActive == true )
            {
                SetDrugItemsGridViewSelectedItem( editIndex, true );  // scroll to new row
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
                    SetDrugItemsGridViewSelectedItem( editIndex, true ); // scroll to new row required
                    Session[ "IsInSearchMode" ] = false;
                }
                else
                {
                    SetDrugItemsGridViewSelectedItem( editIndex, false );
                }
            }

            DrugItemsGridView.DataBind();

            // disable appropriate buttons for the selected row
            SetEnabledItemControlsDuringEdit( DrugItemsGridView, editIndex, false );

            // disable the search and other non-edit controls before going into edit mode
            EnableControlsForItemEditMode( false );
        }

        protected void DrugItemsGridView_RowInserting( object sender,  GridViewInsertEventArgs e )
        {
      //      InitiateEditMode( e.Row.RowIndex );

        }

        private void SetEnabledItemControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )
        {
            gv.SetEnabledControlsForCell( rowIndex, 0, bEnabled ); // refresh
            gv.SetEnabledControlsForCell( rowIndex, 1, bEnabled ); // details
            gv.SetEnabledControlsForCell( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 19 ), bEnabled ); // remove //$$$+
       //     gv.SetEnabledControlsForCell( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 20 ), bEnabled ); // discontinue //$$$+

            gv.SetVisibleControlsForCell( rowIndex, 2, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 2, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 2, "CancelButton", !bEnabled );
        }

        private int ApplyRightHandColumnOffsetBasedOnDocumentType( int columnNumber )
        {
            if( ShowParentItemCombo() == true )
                return( columnNumber );
            else
                if( columnNumber > ParentDrugItemDropDownListFieldNumber )
                    return ( columnNumber );
                else
                    return ( columnNumber );
        }

        private void SetDrugItemsGridViewSelectedItem( int selectedItemIndex, bool bIncludeScroll )
        {
            // save for postback
            Session[ "DrugItemsGridViewSelectedIndex" ] = selectedItemIndex;

            // set the row as selected
            DrugItemsGridView.SelectedIndex = selectedItemIndex;

            // tell the client
            if( bIncludeScroll == true )
                ScrollToSelectedItem();

            // allow the update postback to occur
            InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedItem()
        {
            int rowIndex = DrugItemsGridView.SelectedIndex;
          //  TableItemStyle rowStyle = DrugItemsGridView.RowStyle;
          //  int rowHeight = ( int )rowStyle.Height.Value;  // this value is always zero
            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( ITEMGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( ITEMGRIDVIEWROWHEIGHTESTIMATE * fudge );
            
            string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
       //     ScriptManager.RegisterClientScriptBlock( this.Page, this.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true );  // runs prior to control rendering
        }

        protected void HighlightDrugItemRow( int itemIndex )
        {
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            if( DrugItemsGridView.HasData() == true )
            {
                GridViewRow row = DrugItemsGridView.Rows[ itemIndex ];
                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = DrugItemsGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = DrugItemsGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setDrugItemHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveItemHighlightingScript", preserveHighlightingScript, true );

                // allow the highlight postback to occur 
                ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        private void RestoreDrugItemsGridViewSelectedItem()
        {
            DrugItemsGridView.SelectedIndex = ( int )Session[ "DrugItemsGridViewSelectedIndex" ];
        }
       
        protected void DrugItemsGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            int cancelIndex = e.RowIndex;
            bool bInserting = DrugItemsGridView.InsertRowActive;

            // if inserting
            if( bInserting == true )
            {
                DrugItemsGridView.InsertRowActive = false; // cancels insert ( if inserting )
                _withAddParameter.DefaultValue = "false";
                DrugItemsGridView.EditIndex = -1; // cancels the edit
                DrugItemsGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledItemControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                // re-enable the search and filter
                EnableControlsForItemEditMode( true );

                HighlightDrugItemRow( 0 );
                RefreshPricelist( 0 ); // revert to item zero's pricelist
            }
            else // editing existing row
            {
                DrugItemsGridView.EditIndex = -1; // cancels the edit
                DrugItemsGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledItemControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

                // re-enable the search and filter
                EnableControlsForItemEditMode( true );

                HighlightDrugItemRow( cancelIndex );
                RefreshPricelist( cancelIndex ); // revert to same item 
            }
        }

        protected void DrugItemsGridView_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {

        }



        // command name changed to saveedit so this not firing anymore
        protected void DrugItemsGridView_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {

        }

#endregion ItemInsertEditCancelFunctionsAndEvents

#region DrugItemDatabaseCommandsAndEvents

        private int UpdateDrugItem( GridView gv, int rowIndex )
        {
            int updatedRowIndex = -1;

            _drugItemIdForItemsParameter.DefaultValue = gv.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();

            _fdaAssignedLabelerCodeParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ), 0, false, "fdaAssignedLabelerCodeTextBox" );
            _productCodeParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ), 0, false, "productCodeTextBox" );
            _packageCodeParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ), 0, false, "packageCodeTextBox" );

            _coveredParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 7 ), 0, false, "coveredDropDownList" );

            string genericName = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ), 0, false, "genericTextBox" );
            _genericParameter.DefaultValue = CMGlobals.ReplaceNonPrintableCharacters( genericName ).Trim();
            
            string tradeName = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ), 0, false, "tradeNameTextBox" );
            _tradeNameParameter.DefaultValue = CMGlobals.ReplaceNonPrintableCharacters( tradeName ).Trim();
            
            _dispensingUnitParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 10 ), 0, false, "dispensingUnitTextBox" );
            _packageDescriptionParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 11 ), 0, false, "packageSizeTextBox" );


            _drugItemNDCIdParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( DrugItemNDCIdFieldNumber ), 0, false, "NDCIdLabel" );
            _parentDrugItemIdParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( ParentDrugItemIdFieldNumber ), 0, false, "ParentDrugItemIdLabel" );

            try
            {
                _drugItemsDataSource.Update();
            }
            catch( Exception ex )
            {
               MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.EditIndex = -1; // done with edit
   //         SetDrugItemsGridViewSelectedItem( rowIndex );
            gv.DataBind();

            if( Session[ "LastUpdatedItemId" ] != null )
            {
                int updatedDrugItemId = ( int )Session[ "LastUpdatedItemId" ];
                updatedRowIndex = DrugItemsGridView.GetRowIndexFromId( updatedDrugItemId, 0 );

                SetDrugItemsGridViewSelectedItem( updatedRowIndex, false );

                // bind to select
                gv.DataBind();
            }


            // enable appropriate buttons for the selected row
            SetEnabledItemControlsDuringEdit( gv, updatedRowIndex, true );

            // re-enable the search and filter
            EnableControlsForItemEditMode( true );

            return ( updatedRowIndex );
        }



        private int InsertDrugItem( GridView gv, int rowIndex )
        {
            int insertedRowIndex = 0;

            string fdaAssignedLabelerCode = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ), 0, false, "fdaAssignedLabelerCodeTextBox" );
            _fdaAssignedLabelerCodeParameter.DefaultValue = fdaAssignedLabelerCode.Trim();

            string productCode = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ), 0, false, "productCodeTextBox" );
            _productCodeParameter.DefaultValue = productCode.Trim();

            string packageCode = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ), 0, false, "packageCodeTextBox" );
            _packageCodeParameter.DefaultValue = packageCode.Trim();

            _coveredParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 7 ), 0, false, "coveredDropDownList" );
            
            string genericName = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ), 0, false, "genericTextBox" );
            _genericParameter.DefaultValue = CMGlobals.ReplaceNonPrintableCharacters( genericName ).Trim();
            
            string tradeName = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ), 0, false, "tradeNameTextBox" );
            _tradeNameParameter.DefaultValue = CMGlobals.ReplaceNonPrintableCharacters( tradeName ).Trim();
            
            _dispensingUnitParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 10 ), 0, false, "dispensingUnitTextBox" );
            _packageDescriptionParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 11 ), 0, false, "packageSizeTextBox" );

            _parentDrugItemIdParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "parentItemDropDownList" );

            try
            {
                _drugItemsDataSource.Insert();
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

            if( Session[ "LastInsertedItemId" ] != null )
            {
                int newDrugItemId = ( int )Session[ "LastInsertedItemId" ];
                insertedRowIndex = DrugItemsGridView.GetRowIndexFromId( newDrugItemId, 0 );

                SetDrugItemsGridViewSelectedItem( insertedRowIndex, true );

                // bind to select
                gv.DataBind();
            }

            // enable appropriate buttons for the selected row
            SetEnabledItemControlsDuringEdit( gv, insertedRowIndex, true );

            // re-enable the search and filter
            EnableControlsForItemEditMode( true );

            return ( insertedRowIndex );
        }

        public void _drugItemsDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@DrugItemId" ].Value != null )
            {
                string drugItemIdString = e.Command.Parameters[ "@DrugItemId" ].Value.ToString();

                if( drugItemIdString.Length > 0 )
                {
                    int drugItemId = int.Parse( drugItemIdString );
                    Session[ "LastInsertedItemId" ] = drugItemId;
                }
            }
            else
            {
                Exception insertException = e.Exception;
                if( insertException != null )
                    throw new Exception( String.Format( "DrugItemId returned from insert was null. Insert failed. {0}", insertException.Message ));
                else
                    throw new Exception( "DrugItemId returned from insert was null. Insert failed." );

            }
        }

        // probably wont happen - id changing during update
        public void _drugItemsDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemIdString = e.Command.Parameters[ "@DrugItemId" ].Value.ToString();

            if( drugItemIdString.Length > 0 )
            {
                int drugItemId = int.Parse( drugItemIdString );
                Session[ "LastUpdatedItemId" ] = drugItemId;
            }
        }

        private int DeleteDrugItem( GridView gv, int rowIndex, int selectedDrugItemId )
        {
            // id of row to delete
            _drugItemIdForItemsParameter.DefaultValue = selectedDrugItemId.ToString();

            try
            {
                _drugItemsDataSource.Delete();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            // previous row gets focus
            if( rowIndex >= 1 )
                rowIndex--;

            SetDrugItemsGridViewSelectedItem( rowIndex, false );

            gv.DataBind();

            return ( rowIndex );
        }

        private int RestoreDiscontinuedItem( GridView gv, int itemIndex, int drugItemId, string contractNumber, string fdaAssignedLabelerCode, string productCode, string packageCode, string genericName, string tradeName, int modificationStatusId, string dispensingUnit, string packageDescription )
        {
            bool bSuccess = false;

             DrugItemDB drugItemDB = ( DrugItemDB )Page.Session[ "DrugItemDB" ];
             if( drugItemDB != null )
             {
                 bSuccess = drugItemDB.RestoreDiscontinuedItem( contractNumber, drugItemId, modificationStatusId );
                 if( bSuccess == true )
                 {
                     // if currently viewing discontinued
                     if( _itemGridContext.IsDiscontinued == true )
                     {
                         // previous row gets focus
                         if( itemIndex >= 1 )
                             itemIndex--;
                     }
                     // else restoring an active item so keep current focus

                     SetDrugItemsGridViewSelectedItem( itemIndex, false );

                     gv.DataBind();
                 }
             }
            return ( itemIndex );
        }

        //// old
        //private int DiscontinueDrugItem( GridView gv, int rowIndex, int selectedDrugItemId, DateTime discontinuationDate, int modificationStatusId )
        //{
        //    // id of row to delete
        //    _drugItemIdForItemsParameter.DefaultValue = selectedDrugItemId.ToString();

        //    try
        //    {
        //        DrugItemDB drugItemDB = ( DrugItemDB )Session[ "DrugItemDB" ];
        //        bool bSuccess = drugItemDB.DiscontinueItem( _startupParameters.ContractNumber, selectedDrugItemId, discontinuationDate, modificationStatusId );
        //        if( bSuccess == false )
        //        {
        //            throw new Exception( string.Format( "The following error was encountered when attempting to discontinue item with id {0} : {1}", selectedDrugItemId, drugItemDB.ErrorMessage ));
        //        }
        //    }
        //    catch( Exception ex )
        //    {
        //        MsgBox.ShowErrorFromUpdatePanel( Page, ex );
        //    }

        //    // previous row gets focus
        //    if( rowIndex >= 1 )
        //        rowIndex--;

        //    SetDrugItemsGridViewSelectedItem( rowIndex, false );

        //    gv.DataBind();

        //    return ( rowIndex );
        //}

        // could get called as a result of details or ndc change or similar item operation
        // rebind is true if caller has altered the item list
        private void RefreshDrugItemScreenDueToDetailsChanged( bool bRebindItems, bool bRefreshItemHeaderCount )
        {
            int currentItemIndex = DrugItemsGridView.SelectedIndex;
            int adjustedCurrentItemIndex = currentItemIndex;

            if( bRebindItems == true )
            {
                // adjust the current item index $$$
                int offset = 0;
                if( Session[ "NDCChangeCurrentRowOffset" ] != null )
                {
                    offset = ( int )Session[ "NDCChangeCurrentRowOffset" ];
                    adjustedCurrentItemIndex += offset;

                    // if discontinuing first item
                    if( adjustedCurrentItemIndex < 0 )
                        adjustedCurrentItemIndex = 0;
                }
                else if( Session[ "ItemDiscontinueCurrentRowOffset" ] != null )
                {
                    offset = ( int )Session[ "ItemDiscontinueCurrentRowOffset" ];
                    adjustedCurrentItemIndex += offset;

                    // if discontinuing first item
                    if( adjustedCurrentItemIndex < 0 )
                        adjustedCurrentItemIndex = 0;
                }

                SetDrugItemsGridViewSelectedItem( adjustedCurrentItemIndex, false );

                if( bRefreshItemHeaderCount == true )
                    RefreshMainHeader( true );

                DrugItemsGridView.DataBind();
            }

            ScrollToSelectedItem();
            HighlightDrugItemRow( adjustedCurrentItemIndex );

        }

#endregion DrugItemDatabaseCommandsAndEvents

#region ItemFilterAndSearch

        protected void ItemFilterRadioButtonList_OnSelectedIndexChanged( Object sender, EventArgs e )
        {
            // save current state
            bool bCurrentStateIsDiscontinued = false;
            if( _itemGridContext.IsDiscontinued == true )
            {
                bCurrentStateIsDiscontinued = true;
            }

            string filterCriteria = ItemFilterRadioButtonList.SelectedValue;
            if( filterCriteria.CompareTo( "C" ) == 0 )
            {
                _coveredSelectionCriteriaParameter.DefaultValue = "C"; // Covered
                _itemGridContext.IsDiscontinued = false;
            }
            else if( filterCriteria.CompareTo( "N" ) == 0 )
            {
                _coveredSelectionCriteriaParameter.DefaultValue = "N"; // NonCovered
                _itemGridContext.IsDiscontinued = false;
            }
            else if( filterCriteria.CompareTo( "D" ) == 0 )
            {
                _coveredSelectionCriteriaParameter.DefaultValue = "D"; // Discontinued
                _itemGridContext.IsDiscontinued = true;
            }
            else // all
            {
                _coveredSelectionCriteriaParameter.DefaultValue = "B"; // Both
                _itemGridContext.IsDiscontinued = false;
            }

            DisableControlsForReadOnlyOrDocumentTypeOrItemGridContext(); 

            DrugItemsGridView.DataBind();

            SetDrugItemsGridViewSelectedItem( 0, true );

            HighlightDrugItemRow( 0 );

            RefreshPricelist( 0 );

            // if the selected viewing state of discontinued items is changing
            if( bCurrentStateIsDiscontinued != _itemGridContext.IsDiscontinued )
            {
                if( bCurrentStateIsDiscontinued == true )
                {
                    // changing from discontinued, so select active prices
                    SimulateActivePriceFilterClick();
                }
                else
                {
                    // changing to discontinued, so select historical prices
                    SimulateHistoricalPriceFilterClick();
                }
            }
        }

        protected void PrintItemsAndPricesButton_OnClick( Object sender, EventArgs e )
        {
            Report drugItemsAndPricesForContractReport = new Report( "/Pharmaceutical/Reports/SelectItemsAndPricesForContract" );
            drugItemsAndPricesForContractReport.AddParameter( "ContractNumber", _startupParameters.ContractNumber );
            drugItemsAndPricesForContractReport.AddParameter( "FutureHistoricalSelectionCriteria", "B" );
            drugItemsAndPricesForContractReport.AddStandardParameters();

            Session[ "ReportToShow" ] = drugItemsAndPricesForContractReport;

            string windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=1,top=200,left=240,width=1020,height=800');";
            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "ReportViewerWindowOpenScript", windowOpenScript, true );

        }

        protected void SearchButton_OnClick( Object sender, EventArgs e )
        {
            string searchString = ItemSearchTextBox.Text.Trim();
            if( searchString.Length <= 0 )
                return;

            Session[ "IsInSearchMode" ] = true;

            //string lastSearchString = "";
            //if( Session[ "LastDrugItemSearchString" ] != null )
            //{
            //    lastSearchString = ( String )Session[ "LastDrugItemSearchString" ];
            //}

            //// same search
            //int lastDrugItemSearchRowIndex = 0;
            //if( searchString.CompareTo( lastSearchString ) == 0 )
            //{
            //    if( Session[ "LastDrugItemSearchRowIndex" ] != null )
            //    {
            //        lastDrugItemSearchRowIndex = ( int )Session[ "LastDrugItemSearchRowIndex" ];
            //    }
            //}
            //else
            //{
            //    // new search
            //    Session[ "LastDrugItemSearchString" ] = searchString;
            //}

            // fields to search
            //FdaAssignedLabelerCode
            //ProductCode
            //PackageCode
            //Generic
            //TradeName 
            
            string filterExpression = "";
            _drugItemsDataSource.FilterParameters.Clear();
            DrugItemsGridView.EmptyDataText = "There were no matches for the selected search.";
           
            // number only, include number fields in search
            int testResultInt = 0;
            if( int.TryParse( searchString, out testResultInt ) == true )
            {
                filterExpression = "FdaAssignedLabelerCode like '%{0}%' OR ProductCode like '%{1}%' OR PackageCode like '%{2}%' OR Generic like '%{3}%' OR TradeName like '%{4}%'";
                Parameter fdaAssignedLabelerCodeParameter = new Parameter( "FdaAssignedLabelerCode", TypeCode.String );
                Parameter productCodeParameter = new Parameter( "ProductCode", TypeCode.String );
                Parameter packageCodeParameter = new Parameter( "PackageCode", TypeCode.String );
                Parameter genericNameParameter = new Parameter( "GenericName", TypeCode.String );
                Parameter tradeNameParameter = new Parameter( "TradeName", TypeCode.String );

                fdaAssignedLabelerCodeParameter.DefaultValue = searchString;
                productCodeParameter.DefaultValue = searchString;
                packageCodeParameter.DefaultValue = searchString;
                genericNameParameter.DefaultValue = searchString;
                tradeNameParameter.DefaultValue = searchString;

                _drugItemsDataSource.FilterExpression = filterExpression;
                _drugItemsDataSource.FilterParameters.Add( fdaAssignedLabelerCodeParameter );
                _drugItemsDataSource.FilterParameters.Add( productCodeParameter );
                _drugItemsDataSource.FilterParameters.Add( packageCodeParameter );
                _drugItemsDataSource.FilterParameters.Add( genericNameParameter );
                _drugItemsDataSource.FilterParameters.Add( tradeNameParameter );

            }
            else
            {
                filterExpression = "Generic like '%{0}%' OR TradeName like '%{1}%'";
                Parameter genericNameParameter = new Parameter( "GenericName", TypeCode.String );
                Parameter tradeNameParameter = new Parameter( "TradeName", TypeCode.String );

                genericNameParameter.DefaultValue = searchString;
                tradeNameParameter.DefaultValue = searchString;

                _drugItemsDataSource.FilterExpression = filterExpression;
                _drugItemsDataSource.FilterParameters.Add( genericNameParameter );
                _drugItemsDataSource.FilterParameters.Add( tradeNameParameter );
            }

            DrugItemsGridView.DataBind();

            SetDrugItemsGridViewSelectedItem( 0, true );
        }

        protected void ClearSearchButton_OnClick( Object sender, EventArgs e )
        {
            ClearSearch();
            DrugItemsGridView.DataBind();
            SetDrugItemsGridViewSelectedItem( 0, true );
            Session[ "IsInSearchMode" ] = false;
        }

        private void ClearSearch()
        {
            ItemSearchTextBox.Text = "";
            _drugItemsDataSource.FilterExpression = "";
        }

        // returns a revised row index
        private int ClearSearchWithBind( int drugItemId )
        {
            int revisedRowIndex = 0;
            
            ItemSearchTextBox.Text = "";
            _drugItemsDataSource.FilterExpression = "";

            DrugItemsGridView.DataBind();

            revisedRowIndex = DrugItemsGridView.GetRowIndexFromId( drugItemId, 0 );

            return ( revisedRowIndex );
        }

#endregion ItemFilterAndSearch


#region WillMoveToGridControl

        //private int GetRowIndexFromId( GridView gv, int id, int indexOfGridKeyToMatch )
        //{
        //    int rowIndex = -1;
        //    int keyValue = 0;

        //    for( int i = 0; i < gv.Rows.Count; i++ )
        //    {
        //        // flag to indicate the new row
        //        keyValue = int.Parse( gv.DataKeys[ i ].Values[ indexOfGridKeyToMatch ].ToString() );
        //        if( keyValue == id )
        //        {
        //            rowIndex = i;
        //            break;
        //        }
        //    }

        //    return ( rowIndex );
        //}

        //private int GetRowIdFromSelectedIndex( GridView gv, int selectedIndex )
        //{
        //    int selectedItemId = -1;
        //    if( selectedIndex >= 0 )
        //    {
        //        GridViewRow selectedRow = gv.Rows[ selectedIndex ];
        //        //TableCell aCell = selectedRow.Cells[ 0 ];
        //        selectedItemId = Int32.Parse( gv.DataKeys[ selectedIndex ].Value.ToString() );
        //    }
        //    return ( selectedItemId );
        //}

        //// use only during or after the databound event of the gridview
        //private string GetStringValueFromSelectedIndexOnDatabind( GridView gv, int selectedIndex, string databaseResultsetFieldName )
        //{
        //    string cellValue = string.Empty; 
        //    if( selectedIndex >= 0 )
        //    {
        //        GridViewRow selectedRow = gv.Rows[ selectedIndex ];
        //        if( selectedRow != null )
        //        {
        //            DataRowView dataRowView = ( DataRowView )selectedRow.DataItem;
        //            if( dataRowView != null )
        //            {
        //                object cellValueObj = dataRowView[ databaseResultsetFieldName ];
        //                if( cellValueObj != null )
        //                    cellValue = cellValueObj.ToString();
        //                else
        //                    throw new Exception( string.Format( "Null cellValueObj returned for index {0} field name {1}", selectedIndex, databaseResultsetFieldName ) );
        //            }
        //        }
        //    }
        //    return ( cellValue );
        //}

        //private string GetStringValueFromSelectedIndexForBoundField( GridView gv, int selectedIndex, string databaseResultsetFieldName )
        //{
        //    string cellValue = string.Empty;
        //    int cellIndex = -1;

        //    if( selectedIndex >= 0 )
        //    {
        //        GridViewRow selectedRow = gv.Rows[ selectedIndex ];
        //        if( selectedRow != null )
        //        {
        //            cellIndex = GetIndexFromFieldNameForBoundField( gv, databaseResultsetFieldName );
        //            cellValue = selectedRow.Cells[ cellIndex ].Text;
        //        }
        //    }
        //    return ( cellValue );
        //}

        // control index will usually be zero as there will usually be only one control in a cell
        // if a cell is read only, it gets accessed differently
        //private string GetStringValueFromSelectedControl( GridView gv, int selectedIndex, int columnIndex, int controlIndex, bool bIsReadOnly, string nestedTemplatedControlName )
        //{
        //    string cellValue = string.Empty;

        //    if( selectedIndex >= 0 )
        //    {
        //        GridViewRow selectedRow = gv.Rows[ selectedIndex ];
        //        if( selectedRow != null )
        //        {
        //            if( bIsReadOnly == false )
        //            {
        //                Control control = selectedRow.Cells[ columnIndex ].Controls[ controlIndex ];
        //                if( control != null )
        //                {
        //                    Type controlType = control.GetType();
        //                    string typeName = controlType.Name;
        //                    if( typeName.CompareTo( "LiteralControl" ) == 0 )
        //                    {
        //                        Control nestedControl = selectedRow.FindControl( nestedTemplatedControlName );
        //                        if( nestedControl != null )
        //                        {
        //                            cellValue = GetStringValueFromControl( nestedControl );
        //                        }
        //                    }
        //                    else
        //                    {
        //                        cellValue = GetStringValueFromControl( control );
        //                    }
        //                }
        //            }
        //            else // read only has data directly in cell
        //            {
        //                cellValue = selectedRow.Cells[ columnIndex ].Text;
        //            }
        //        }
        //    }
        //    return ( cellValue );
        //}

        //private string GetStringValueFromControl( Control control )
        //{
        //    string cellValue = string.Empty;

        //    if( control != null )
        //    {
        //        Type controlType = control.GetType();
        //        string typeName = controlType.Name;

        //        if( typeName.CompareTo( "Label" ) == 0 )
        //        {
        //            cellValue = ( ( Label )control ).Text;
        //        }
        //        else if( typeName.CompareTo( "TextBox" ) == 0 )
        //        {
        //            cellValue = ( ( TextBox )control ).Text;
        //        }
        //        else if( typeName.CompareTo( "CheckBox" ) == 0 )
        //        {
        //            cellValue = ( ( CheckBox )control ).Checked.ToString();
        //        }
        //        else if( typeName.CompareTo( "DropDownList" ) == 0 )
        //        {
        //            cellValue = ( ( DropDownList )control ).SelectedValue.ToString();
        //        }
        //    }

        //    return ( cellValue );
        //}

        //private string GetStringValueFromSelectedIndexForTemplateField( GridView gv, int selectedIndex, string containedControlId )
        //{
        //    string cellValue = string.Empty;

        //    if( selectedIndex >= 0 )
        //    {
        //        GridViewRow selectedRow = gv.Rows[ selectedIndex ];
        //        if( selectedRow != null )
        //        {
        //            Label containedLabel = ( Label )selectedRow.FindControl( containedControlId );
        //            if( containedLabel != null )
        //                cellValue = containedLabel.Text;
        //        }
        //    }
        //    return ( cellValue );
        //}

        //private void SetEnabledControlsForCell( GridView gv, int rowIndex, int columnIndex, bool bEnabled )
        //{
        //    GridViewRow selectedRow = gv.Rows[ rowIndex ];
        //    ControlCollection controlsInCell = null;

        //    if( selectedRow != null )
        //    {
        //        controlsInCell = selectedRow.Cells[ columnIndex ].Controls;

        //        if( controlsInCell != null )
        //        {
        //            foreach( Control control in controlsInCell )
        //            {
        //                if( control != null )
        //                {
        //                    Type controlType = control.GetType();
        //                    string typeName = controlType.Name;
        //                    if( typeName.CompareTo( "Button" ) == 0 )
        //                    {
        //                        ( ( Button )control ).Enabled = bEnabled;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}



        //private void SetVisibleControlsForCell( GridView gv, int rowIndex, int columnIndex, string controlId, bool bVisible )
        //{
        //    GridViewRow selectedRow = gv.Rows[ rowIndex ];
        //    ControlCollection controlsInCell = null;
        //    string tempId = "";

        //    if( selectedRow != null )
        //    {
        //        controlsInCell = selectedRow.Cells[ columnIndex ].Controls;

        //        if( controlsInCell != null )
        //        {
        //            foreach( Control control in controlsInCell )
        //            {
        //                if( control != null )
        //                {
        //                    Type controlType = control.GetType();
        //                    string typeName = controlType.Name;
        //                    if( typeName.CompareTo( "Button" ) == 0 )
        //                    {
        //                        if( control.ID.CompareTo( controlId ) == 0 )
        //                        {
        //                            ( ( Button )control ).Visible = bVisible;
        //                        }
        //                    }
        //                    else // debug
        //                    {
        //                        tempId = control.UniqueID;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private int GetIndexFromFieldNameForBoundField( GridView gv, string databaseResultsetFieldName )
        //{
        //    int columnIndex = 0;

        //    DataControlFieldCollection gvFields = gv.Columns;
            
        //    foreach( DataControlField field in gvFields )
        //    {
        //        if( field.GetType().Name.CompareTo( "BoundField" ) == 0 )
        //        {
        //            BoundField boundField = ( BoundField )field;

        //            if( boundField.DataField.CompareTo( databaseResultsetFieldName ) == 0 )
        //            {
        //                columnIndex = gvFields.IndexOf( field );
        //                break;
        //            }
        //        }
        //        //else if( field.GetType().Name.CompareTo( "TemplateField" ) == 0 )
        //        //{
        //        //    TemplateField templateField = ( TemplateField )field;

        //        //    templateField.n
                    
                    
                   

        //        //}
        //    }

        //    return ( columnIndex );
        //}

        //private string GetStringValueFromSelectedIndex( GridView gv, int selectedIndex, SqlDataSource dataSource, string databaseResultsetFieldName )
        //{
        //    string cellValue = string.Empty;
        //    int selectedRowId = -1;

        //    selectedRowId = GetRowIdFromSelectedIndex( gv, selectedIndex );

        //    if( selectedRowId >= 0 )
        //    {
        //        ControlBindingsCollection 


        //    }


        //}

#endregion WillMoveToGridControl

#region ItemEffectOnPrice

        protected void DrugItemsGridView_OnSelectedIndexChanged( object sender, EventArgs e )
        {

        }

 
        
        private void SetDrugItemPriceDataSourceParameterValues()
        {
            int selectedIndex = DrugItemsGridView.SelectedIndex;
            int drugItemId = DrugItemsGridView.GetRowIdFromSelectedIndex( selectedIndex, 0 );

            _drugItemIdForPricesParameter.DefaultValue = drugItemId.ToString();
            _contractNumberParameter.DefaultValue = _startupParameters.ContractNumber;
        }

        private void SetDrugItemPriceDataSourceParameterValues( int drugItemIndex )
        {
            int drugItemId = DrugItemsGridView.GetRowIdFromSelectedIndex( drugItemIndex, 0 );

            _drugItemIdForPricesParameter.DefaultValue = drugItemId.ToString();
            _contractNumberParameter.DefaultValue = _startupParameters.ContractNumber; //"V797P-5624x";
        }

        private void SetItemPriceHeaderInfo( int selectedIndex )
        {
            if( DrugItemsGridView.HasData() == true )
            {
                SelectedDrugItemHeader.HeaderTitle = "Prices For Selected Item";
                SelectedDrugItemHeader.FdaAssignedLabelerCode = DrugItemsGridView.GetStringValueFromSelectedControl( selectedIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ), 0, false, "fdaAssignedLabelerCodeLabel" );
                SelectedDrugItemHeader.ProductCode = DrugItemsGridView.GetStringValueFromSelectedControl( selectedIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ), 0, false, "productCodeLabel" );// "ProductCode" );
                SelectedDrugItemHeader.PackageCode = DrugItemsGridView.GetStringValueFromSelectedControl( selectedIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ), 0, false, "packageCodeLabel" );//  "PackageCode" );
                SelectedDrugItemHeader.SetCovered( DrugItemsGridView.GetStringValueFromSelectedControl( selectedIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 7 ), 0, false, "coveredLabel" ) );
           //     SelectedDrugItemHeader.SetSingleDual( AlterSingleDualString( DrugItemsGridView.GetStringValueFromSelectedControl( selectedIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( DualPriceDesignationFieldNumber ), 0, false, "DualPriceDesignationLabel" ) ) );
                SelectedDrugItemHeader.SetSingleDual( GetSingleDualStringForDisplay() );
                SelectedDrugItemHeader.SetFETAmount( GetFETAmountStringForDisplay() );
                SelectedDrugItemHeader.Generic = DrugItemsGridView.GetStringValueFromSelectedControl( selectedIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ), 0, false, "genericLabel" );
                SelectedDrugItemHeader.TradeName = DrugItemsGridView.GetStringValueFromSelectedControl( selectedIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ), 0, false, "tradeNameLabel" );
            }
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
                        FETAmountString = IncludedFETAmount.ToString("0.00");
                }
            }
            return ( FETAmountString );
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

        private void SaveSelectedItemInfoIntoSession( int selectedDrugItemIndex )
        {
            int drugItemId = DrugItemsGridView.GetRowIdFromSelectedIndex( selectedDrugItemIndex, 0 );
            bool bIsItemDualPricer = false;
            Decimal IncludedFETAmount = 0;

            try
            {
                if( Session[ "DrugItemDB" ] != null )
                {
                    DrugItemDB drugItemDB = ( DrugItemDB )Session[ "DrugItemDB" ];

                    if( drugItemDB != null )
                    {
                        drugItemDB.IsItemDualPricer( drugItemId, out bIsItemDualPricer );
                        drugItemDB.GetItemIncludedFETAmount( drugItemId, out IncludedFETAmount );
                    }
                }
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            Session[ "IsSelectedDrugItemDualPricer" ] = bIsItemDualPricer;   
            Session[ "SelectedDrugItemIncludedFETAmount" ] = IncludedFETAmount.ToString( "0.00" );
        }

        private void RefreshPricelist( int itemIndex )
        {
            SetDrugItemPriceDataSourceParameterValues( itemIndex );
            SqlDataSource drugItemPricesDataSource = ( SqlDataSource )DrugItemPricesGridView.DataSource;
            drugItemPricesDataSource.Select( DataSourceSelectArguments.Empty );
            SetDrugItemsGridViewSelectedItem( itemIndex, false );
            
            SaveSelectedItemInfoIntoSession( itemIndex );  //$$$ 3/10/2016 moved this line up one more so session can be set for header display and before binding where formatting happens

            DrugItemPricesGridView.DataBind();

            
            // refresh the header info
            SetItemPriceHeaderInfo( itemIndex );
 
            // allow the update postback to occur
            RefreshPricesButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

#endregion ItemEffectOnPrice

        private void OpenItemDetailsWindow( int itemIndex, int selectedDrugItemId, string contractNumber )
        {
            Session[ "DrugItemDetailsWindowParms" ] = null;
            Session[ "DrugItemDetailsWindowParms" ] = new DrugItemDetailsWindowParms( itemIndex, selectedDrugItemId, contractNumber, ( _startupParameters.IsItemDetailsCOEditable && _itemGridContext.IsDiscontinued == false ), ( _startupParameters.IsItemDetailsPBMEditable && _itemGridContext.IsDiscontinued == false ), _startupParameters.IsBPA );

            string windowOpenScript = "window.open('DrugItemDetails.aspx','DrugItemDetails','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=100,left=240,width=900,height=748, resizable=0');";
            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "ItemDetailsWindowOpenScript", windowOpenScript, true );
        }

        private void OpenNDCChangeWindow( int itemIndex, int selectedDrugItemId, string contractNumber, string fdaAssignedLabelerCode, string productCode, string packageCode, string genericName, string tradeName, int modificationStatusId )
        {
            Session[ "NDCChangeWindowParms" ] = null;
            Session[ "NDCChangeWindowParms" ] = new NDCChangeWindowParms( itemIndex, selectedDrugItemId, contractNumber, ( _startupParameters.IsItemEditable && _itemGridContext.IsDiscontinued == false ), fdaAssignedLabelerCode, productCode, packageCode, genericName, tradeName, modificationStatusId );

            string windowOpenScript = "window.open('NDCChange.aspx','NDCChange','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=200,left=240,width=550,height=436, resizable=0');";
            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "NDCChangeWindowOpenScript", windowOpenScript, true );
        }

        private void OpenItemCopyWindow( int itemIndex, int selectedDrugItemId, string contractNumber, string fdaAssignedLabelerCode, string productCode, string packageCode, string genericName, string tradeName, int modificationStatusId, string dispensingUnit, string packageDescription, ItemCopyWindowParms.CopyTypes copyType )
        {
            string unitOfSale = "";
            decimal quantityInUnitOfSale = 0;
            string unitPackage = "";
            decimal quantityInUnitPackage = 0;
            string unitOfMeasure = "";

            // look up package info for copy local
            if( copyType == ItemCopyWindowParms.CopyTypes.CopyLocal )
            {
                try
                {
                    if( Session[ "DrugItemDB" ] != null )
                    {
                        DrugItemDB drugItemDB = ( DrugItemDB )Session[ "DrugItemDB" ];

                        if( drugItemDB != null )
                        {
                            drugItemDB.GetPackageInfoForItemCopy( selectedDrugItemId, out unitOfSale, out quantityInUnitOfSale, out unitPackage, out quantityInUnitPackage, out unitOfMeasure );
                        }
                    }
                }
                catch( Exception ex )
                {
                    MsgBox.ShowErrorFromUpdatePanel( Page, ex );
                }

            }

            Session[ "ItemCopyWindowParms" ] = null;
            Session[ "ItemCopyWindowParms" ] = new ItemCopyWindowParms( itemIndex, selectedDrugItemId, contractNumber, _startupParameters.IsItemEditable, fdaAssignedLabelerCode, productCode, packageCode, genericName, tradeName, modificationStatusId, dispensingUnit, packageDescription, copyType, unitOfSale, quantityInUnitOfSale, unitPackage, quantityInUnitPackage, unitOfMeasure, _itemGridContext.IsDiscontinued );

            string windowOpenScript;
            windowOpenScript = "window.open('ItemCopy.aspx','ItemCopy','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=200,left=240,width=550,height=534, resizable=0');";

            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "ItemCopyWindowOpenScript", windowOpenScript, true );
        }

        private void OpenItemDiscontinueWindow( int itemIndex, int selectedDrugItemId, string contractNumber, string fdaAssignedLabelerCode, string productCode, string packageCode, string genericName, string tradeName, int modificationStatusId, string dispensingUnit, string packageDescription, string discontinuationDateString, string discontinuationReasonString, ItemDiscontinueWindowParms.DiscontinuationReasonCategories discontinuationReasonCategory )
        {
            Session[ "ItemDiscontinueWindowParms" ] = null;
            Session[ "ItemDiscontinueWindowParms" ] = new ItemDiscontinueWindowParms( itemIndex, selectedDrugItemId, contractNumber, _startupParameters.IsItemEditable, fdaAssignedLabelerCode, productCode, packageCode, genericName, tradeName, modificationStatusId, dispensingUnit, packageDescription, discontinuationDateString, discontinuationReasonString, discontinuationReasonCategory, _itemGridContext.IsDiscontinued );

            string windowOpenScript;
            windowOpenScript = "window.open('ItemDiscontinue.aspx','ItemDiscontinue','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=200,left=240,width=550,height=534, resizable=0');";

            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "ItemDiscontinueWindowOpenScript", windowOpenScript, true );
        }

        

        //************************** Prices *******************************



        private void CreateDrugItemPricesDataSourceParameters()
        {
            // select
            _drugItemIdForPricesParameter = new Parameter( "DrugItemId", TypeCode.Int32 );
            _withAddPriceParameter = new Parameter( "WithAddPrice", TypeCode.Boolean );
            _futureHistoricalSelectionCriteriaParameter = new Parameter( "FutureHistoricalSelectionCriteria", TypeCode.String );

            // select and update
            _contractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );

            // update and delete
            _drugItemPriceIdParameter = new Parameter( "DrugItemPriceId", TypeCode.Int32 );

            // update
      //      _priceIdParameter = new Parameter( "PriceId", TypeCode.Int32 );   
            _priceStartDateParameter = new Parameter( "PriceStartDate", TypeCode.DateTime );
            _priceEndDateParameter = new Parameter( "PriceEndDate", TypeCode.DateTime );   
            _priceParameter = new Parameter( "Price", TypeCode.Decimal );
            _isTemporaryParameter = new Parameter( "IsTemporary", TypeCode.Boolean );
            _isFSSParameter = new Parameter( "IsFSS", TypeCode.Boolean );
            _isBIG4Parameter = new Parameter( "IsBIG4", TypeCode.Boolean );   
            _isVAParameter = new Parameter( "IsVA", TypeCode.Boolean );   
            _isBOPParameter = new Parameter( "IsBOP", TypeCode.Boolean );   
            _isCMOPParameter = new Parameter( "IsCMOP", TypeCode.Boolean );   
            _isDODParameter = new Parameter( "IsDOD", TypeCode.Boolean );   
            _isHHSParameter = new Parameter( "IsHHS", TypeCode.Boolean );   
            _isIHSParameter = new Parameter( "IsIHS", TypeCode.Boolean );   
            _isIHS2Parameter = new Parameter( "IsIHS2", TypeCode.Boolean );   
            _isDIHSParameter = new Parameter( "IsDIHS", TypeCode.Boolean );   
            _isNIHParameter = new Parameter( "IsNIH", TypeCode.Boolean );   
            _isPHSParameter = new Parameter( "IsPHS", TypeCode.Boolean );   
            _isSVHParameter = new Parameter( "IsSVH", TypeCode.Boolean );   
            _isSVH1Parameter = new Parameter( "IsSVH1", TypeCode.Boolean );   
            _isSVH2Parameter = new Parameter( "IsSVH2", TypeCode.Boolean );   
            _isTMOPParameter = new Parameter( "IsTMOP", TypeCode.Boolean );   
            _isUSCGParameter = new Parameter( "IsUSCG", TypeCode.Boolean );
            _isFHCCParameter = new Parameter( "IsFHCC", TypeCode.Boolean );
            _drugItemSubItemIdParameter = new Parameter( "DrugItemSubItemId", TypeCode.String );

            // insert
            _drugItemPriceIdForPriceInsertParameter = new Parameter( "DrugItemPriceId", TypeCode.Int32 );
            _drugItemPriceIdForPriceInsertParameter.Direction = ParameterDirection.Output;

       //     _priceIdForPriceInsertParameter = new Parameter( "PriceId", TypeCode.Int32 );
       //     _priceIdForPriceInsertParameter.Direction = ParameterDirection.Output;
        }


        private void RestoreDrugItemPricesDataSourceParameters( DocumentDataSource _drugItemPricesDataSource )
        {
            // select
            _contractNumberParameter = _drugItemPricesDataSource.SelectParameters[ "ContractNumber" ];
            _drugItemIdForPricesParameter = _drugItemPricesDataSource.SelectParameters[ "DrugItemId" ];
            _withAddPriceParameter = _drugItemPricesDataSource.SelectParameters[ "WithAddPrice" ];
            _futureHistoricalSelectionCriteriaParameter = _drugItemPricesDataSource.SelectParameters[ "FutureHistoricalSelectionCriteria" ];

            // update and delete
            _drugItemPriceIdParameter = _drugItemPricesDataSource.UpdateParameters[ "DrugItemPriceId" ];
            _modificationStatusIdParameter = _drugItemPricesDataSource.UpdateParameters[ "ModificationStatuId" ];

            // update
      //      _priceIdParameter = _drugItemPricesDataSource.UpdateParameters[ "PriceId" ];
            _priceStartDateParameter = _drugItemPricesDataSource.UpdateParameters[ "PriceStartDate" ];
            _priceEndDateParameter = _drugItemPricesDataSource.UpdateParameters[ "PriceEndDate" ];
            _priceParameter = _drugItemPricesDataSource.UpdateParameters[ "Price" ];
            _isTemporaryParameter = _drugItemPricesDataSource.UpdateParameters[ "IsTemporary" ];
            _isFSSParameter = _drugItemPricesDataSource.UpdateParameters[ "IsFSS" ];
            _isBIG4Parameter = _drugItemPricesDataSource.UpdateParameters[ "IsBIG4" ];
            _isVAParameter = _drugItemPricesDataSource.UpdateParameters[ "IsVA" ];
            _isBOPParameter = _drugItemPricesDataSource.UpdateParameters[ "IsBOP" ];
            _isCMOPParameter = _drugItemPricesDataSource.UpdateParameters[ "IsCMOP" ];
            _isDODParameter = _drugItemPricesDataSource.UpdateParameters[ "IsDOD" ];
            _isHHSParameter = _drugItemPricesDataSource.UpdateParameters[ "IsHHS" ];
            _isIHSParameter = _drugItemPricesDataSource.UpdateParameters[ "IsIHS" ];
            _isIHS2Parameter = _drugItemPricesDataSource.UpdateParameters[ "IsIHS2" ];
            _isDIHSParameter = _drugItemPricesDataSource.UpdateParameters[ "IsDIHS" ];
            _isNIHParameter = _drugItemPricesDataSource.UpdateParameters[ "IsNIH" ];
            _isPHSParameter = _drugItemPricesDataSource.UpdateParameters[ "IsPHS" ];
            _isSVHParameter = _drugItemPricesDataSource.UpdateParameters[ "IsSVH" ];
            _isSVH1Parameter = _drugItemPricesDataSource.UpdateParameters[ "IsSVH1" ];
            _isSVH2Parameter = _drugItemPricesDataSource.UpdateParameters[ "IsSVH2" ];
            _isTMOPParameter = _drugItemPricesDataSource.UpdateParameters[ "IsTMOP" ];
            _isUSCGParameter = _drugItemPricesDataSource.UpdateParameters[ "IsUSCG" ];
            _isFHCCParameter = _drugItemPricesDataSource.UpdateParameters[ "IsFHCC" ];
            _drugItemSubItemIdParameter = _drugItemPricesDataSource.UpdateParameters[ "DrugItemSubItemId" ];

            // insert
            _drugItemPriceIdForPriceInsertParameter = _drugItemPricesDataSource.InsertParameters[ "DrugItemPriceId" ];
    //        _priceIdForPriceInsertParameter = _drugItemPricesDataSource.InsertParameters[ "PriceId" ];

        }

 
        protected void DrugItemPricesGridView_RowCommand( object sender, GridViewCommandEventArgs e )
        {

        }

        protected void DrugItemPricesGridView_ButtonCommand( object sender, CommandEventArgs e )
        {
            int selectedDrugItemPriceId = -1;
            int selectedDrugItemPriceHistoryId = -1;
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
                        selectedDrugItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );
                        selectedDrugItemPriceHistoryId = Int32.Parse( commandArgs[ 2 ].ToString() );

                        HighlightDrugItemPriceRow( priceIndex );

                        SetDrugItemPricesGridViewSelectedItem( priceIndex );

                        // get parent item id information
                        int itemIndex = DrugItemsGridView.SelectedIndex;

                        // save for postback
                        //   SetDrugItemsGridViewSelectedItem( itemIndex );  
                        Session[ "DrugItemsGridViewSelectedIndex" ] = itemIndex;

                        int selectedDrugItemId = DrugItemsGridView.GetRowIdFromSelectedIndex( itemIndex, 0 );
                        string IsFromHistoryString = DrugItemPricesGridView.GetStringValueFromSelectedIndexForTemplateField( priceIndex, "IsFromHistoryLabel" );
                        bool bIsFromHistory = ( IsFromHistoryString == "1" ) ? true : false;
                        string IsHistoryFromArchiveString = DrugItemPricesGridView.GetStringValueFromSelectedIndexForTemplateField( priceIndex, "IsHistoryFromArchiveLabel" );
                        bool bIsHistoryFromArchive = ( IsHistoryFromArchiveString == "1" ) ? true : false;
                        OpenPriceDetailsWindow( itemIndex, selectedDrugItemId, priceIndex, selectedDrugItemPriceId, selectedDrugItemPriceHistoryId, bIsFromHistory, bIsHistoryFromArchive );
                    }
                }
            }
            else if( e.CommandName.CompareTo( "RemovePrice" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                priceIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                int newRowIndex = DeleteDrugItemPrice( DrugItemPricesGridView, priceIndex, selectedDrugItemPriceId );

                HighlightDrugItemPriceRow( newRowIndex );

            }
            else if( e.CommandName.CompareTo( "EditPrice" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                priceIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                HighlightDrugItemPriceRow( priceIndex );

                InitiateEditModeForPrice( priceIndex );
            }
            // save update or save insert
            else if( e.CommandName.CompareTo( "SavePrice" ) == 0 )
            {
                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                priceIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                selectedDrugItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );

                int selectedDrugItemId = ( int )DrugItemsGridView.SelectedDataKey.Value;
                string validationMessage = "";
                string warningMessage = "";
                bool bUserCanOverride = false;
                bool bWarningOnly = false;

                // validate the price before saving
                bool bIsPriceOk = ValidatePriceBeforeUpdate( DrugItemPricesGridView, priceIndex, selectedDrugItemPriceId, selectedDrugItemId, ref bUserCanOverride, ref bWarningOnly, ref validationMessage, ref warningMessage );

                if( bIsPriceOk == true )
                {
                    if( bWarningOnly == true )
                    {
                        MsgBox.AlertFromUpdatePanel( Page, warningMessage );
                    }

                    // is this an insert or an update
                    int newOrUpdatedRowIndex = -1;
                    if( DrugItemPricesGridView.InsertRowActive == true )
                    {
                        newOrUpdatedRowIndex = InsertDrugItemPrice( DrugItemPricesGridView, priceIndex, selectedDrugItemId );
                    }
                    else
                    {
                        newOrUpdatedRowIndex = UpdateDrugItemPrice( DrugItemPricesGridView, priceIndex );
                    }

                    HighlightDrugItemPriceRow( newOrUpdatedRowIndex );
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
                selectedDrugItemPriceId = Int32.Parse( commandArgs[ 1 ].ToString() );
            }
            // can trap editing commands here if needed, prior to individual editing events
        }


        private void HandleSavePriceValidationUserOverrideResult( MsgBox.ConfirmationResults currentResult )
        {
            int priceIndex = DrugItemPricesGridView.EditIndex;
            int selectedDrugItemId = ( int )DrugItemsGridView.SelectedDataKey.Value;

            // user requested save anyway
            if( currentResult == MsgBox.ConfirmationResults.TrueResult )
            {       
                // is this an insert or an update
                int newOrUpdatedRowIndex = -1;
                if( DrugItemPricesGridView.InsertRowActive == true )
                {

                    newOrUpdatedRowIndex = InsertDrugItemPrice( DrugItemPricesGridView, priceIndex, selectedDrugItemId );
                }
                else
                {
                    newOrUpdatedRowIndex = UpdateDrugItemPrice( DrugItemPricesGridView, priceIndex );
                }

                HighlightDrugItemPriceRow( newOrUpdatedRowIndex );
            }
            else // user requested no save, therefore cancel
            {
                CancelPriceEdit( priceIndex );
            }
        }

        protected void AddNewPriceButton_OnClick( object sender, EventArgs e )
        {
            DrugItemPricesGridView.Insert();

            _withAddPriceParameter.DefaultValue = "true";

            DrugItemPricesGridView.DataBind();

            InitiateEditModeForPrice( 0 );

            // allow the update postback to occur
            InsertPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }
            
        protected void DrugItemPricesGridView_DataBound( object sender, EventArgs e )
        {
            // update the header dual pricer status
            GridView gv = ( GridView )sender;
            if( gv != null )
            {
                if( gv.Rows.Count > 0 )
                {
                    // get the status from the 1st row
                    GridViewRow row = gv.Rows[ 0 ];
                    string dualPriceDesignation = gv.GetStringValueFromSelectedIndexForTemplateField( 0, "DualPriceDesignationLabel" );
                        
                    SelectedDrugItemHeader.SetSingleDual( AlterSingleDualString( dualPriceDesignation ) );
                }
                else
                {
                    SelectedDrugItemHeader.SetSingleDual( "Undefined" );
                }

                PriceListChangeUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        private void UpdateDualPricerDesignationInDrugItemGridFromPriceGrid( string dualPriceDesignation )
        {
            if( DrugItemsGridView.SelectedIndex >= 0 )
            {
                DrugItemsGridView.SetStringValueInControlInCell( DrugItemsGridView.SelectedIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( DualPriceDesignationFieldNumber ), "DualPriceDesignationLabel", dualPriceDesignation );
            }
        }

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
            DrugItemPricesGridView.DataBind();
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
            DrugItemPricesGridView.DataBind();
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
            DrugItemPricesGridView.DataBind();
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
                SetEnabledPriceControlsForHistorical( DrugItemPricesGridView, false );
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
                SetEnabledPriceControlsForHistorical( DrugItemPricesGridView, true );
            }


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
            CheckBoxList priceFilterCheckBoxList = ( CheckBoxList )drugItemForm.FindControl( "PriceFilterCheckBoxList" );
            if( priceFilterCheckBoxList != null )
            {
                priceFilterCheckBoxList.Enabled = bEnabled;
            }

            Button addNewPriceButton = ( Button )drugItemForm.FindControl( "AddNewPriceButton" );
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

        protected void FCP_OnDataBinding( object sender, EventArgs e )
        {
            Label fcpLabel = ( Label )sender;

            if( fcpLabel != null )
            {
                 GridViewRow gridViewRow = ( GridViewRow )fcpLabel.NamingContainer;

                 if( gridViewRow != null )
                 {
                     if( gridViewRow.DataItem != null )
                     {
                         TableCell targetCell = gridViewRow.Cells[ 5 ];
                         
                         string fcp = ( ( DataRowView )gridViewRow.DataItem )[ "FCP" ].ToString();
                         fcpLabel.Text = string.Format( "{0:c}", fcp );

                         string price = ( ( DataRowView )gridViewRow.DataItem )[ "Price" ].ToString();

                         string covered = ( ( DataRowView )gridViewRow.DataItem )[ "Covered" ].ToString();

                         targetCell.CssClass = GetFCPFormat( covered, fcp, price );
                     }
                 }
             }
        }

        private string GetFCPFormat( string covered, string fcpString, string priceString )
        {
            //HighlightedMissingData
            //HighlightedOutOfRangeData
            //HighlightedTPRWithoutBasePrice
            //UnhighlightedData
            string fcpFormatClassName = "";
            if( covered.CompareTo( "T" ) == 0 )
            {
                decimal price = 0;
                decimal fcp = 0;

                if( decimal.TryParse( fcpString, out fcp ) == false )
                {
                    fcpFormatClassName = "HighlightedMissingData";
                }
            }

            return ( fcpFormatClassName );
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
                        TableCell targetCell = gridViewRow.Cells[ 4 ];

                        bool bIsItemDualPricer = false;
                        if( Session[ "IsSelectedDrugItemDualPricer" ] != null )
                            bIsItemDualPricer = ( bool )Session[ "IsSelectedDrugItemDualPricer" ];

                        Decimal selectedDrugItemIncludedFETAmount = 0;
                        if( Session[ "SelectedDrugItemIncludedFETAmount" ] != null )
                            selectedDrugItemIncludedFETAmount = Decimal.Parse( Session[ "SelectedDrugItemIncludedFETAmount" ].ToString() );

                        string isTemporary = ( ( DataRowView )gridViewRow.DataItem )[ "IsTemporary" ].ToString();
                        bool bIsTemporary = bool.Parse( isTemporary );

                        string isFSS = ( ( DataRowView )gridViewRow.DataItem )[ "IsFSS" ].ToString();
                        bool bIsFSS = bool.Parse( isFSS );

                        string isBIG4 = ( ( DataRowView )gridViewRow.DataItem )[ "IsBIG4" ].ToString();
                        bool bIsBIG4 = bool.Parse( isBIG4 );

                        string price = ( ( DataRowView )gridViewRow.DataItem )[ "Price" ].ToString();
                        priceLabel.Text = string.Format( "{0:c}", price );

                        string fcp = ( ( DataRowView )gridViewRow.DataItem )[ "FCP" ].ToString();

                        string covered = ( ( DataRowView )gridViewRow.DataItem )[ "Covered" ].ToString();

                        string VAIFF = ( ( DataRowView )gridViewRow.DataItem )[ "VAIFF" ].ToString();

                        string TPRAlwaysHasBasePrice = ( ( DataRowView )gridViewRow.DataItem )[ "TPRAlwaysHasBasePrice" ].ToString();
                        TPRAlwaysHasBasePrice = ( TPRAlwaysHasBasePrice.CompareTo( "0" ) == 0 ) ? "False" : ( ( TPRAlwaysHasBasePrice.CompareTo( "1" ) == 0 ) ? "True" : TPRAlwaysHasBasePrice );
                        bool bTPRAlwaysHasBasePrice = bool.Parse( TPRAlwaysHasBasePrice );

                        targetCell.CssClass = GetPriceFormat( targetCell.CssClass, covered, fcp, price, VAIFF, bIsBIG4, bIsFSS, bIsItemDualPricer, bIsTemporary, bTPRAlwaysHasBasePrice, selectedDrugItemIncludedFETAmount );
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
                        TableCell targetCell = gridViewRow.Cells[ 4 ];

                        bool bIsItemDualPricer = false;
                        if( Session[ "IsSelectedDrugItemDualPricer" ] != null )
                            bIsItemDualPricer = ( bool )Session[ "IsSelectedDrugItemDualPricer" ];

                        Decimal selectedDrugItemIncludedFETAmount = 0;
                        if( Session[ "SelectedDrugItemIncludedFETAmount" ] != null )
                            selectedDrugItemIncludedFETAmount = Decimal.Parse( Session[ "SelectedDrugItemIncludedFETAmount" ].ToString() );

                        string isTemporary = ( ( DataRowView )gridViewRow.DataItem )[ "IsTemporary" ].ToString();
                        bool bIsTemporary = bool.Parse( isTemporary );

                        string isFSS = ( ( DataRowView )gridViewRow.DataItem )[ "IsFSS" ].ToString();
                        bool bIsFSS = bool.Parse( isFSS );

                        string isBIG4 = ( ( DataRowView )gridViewRow.DataItem )[ "IsBIG4" ].ToString();
                        bool bIsBIG4 = bool.Parse( isBIG4 );

                        string price = ( ( DataRowView )gridViewRow.DataItem )[ "Price" ].ToString();
                        priceTextBox.Text = string.Format( "{0:c}", price );

                        string fcp = ( ( DataRowView )gridViewRow.DataItem )[ "FCP" ].ToString();

                        string covered = ( ( DataRowView )gridViewRow.DataItem )[ "Covered" ].ToString();

                        string VAIFF = ( ( DataRowView )gridViewRow.DataItem )[ "VAIFF" ].ToString();

                        string TPRAlwaysHasBasePrice = ( ( DataRowView )gridViewRow.DataItem )[ "TPRAlwaysHasBasePrice" ].ToString();
                        TPRAlwaysHasBasePrice = ( TPRAlwaysHasBasePrice.CompareTo( "0" ) == 0 ) ? "False" : ( ( TPRAlwaysHasBasePrice.CompareTo( "1" ) == 0 ) ? "True" : TPRAlwaysHasBasePrice );
                        bool bTPRAlwaysHasBasePrice = bool.Parse( TPRAlwaysHasBasePrice );

                        targetCell.CssClass = GetPriceFormat( targetCell.CssClass, covered, fcp, price, VAIFF, bIsBIG4, bIsFSS, bIsItemDualPricer, bIsTemporary, bTPRAlwaysHasBasePrice, selectedDrugItemIncludedFETAmount );                
                    }
                }
            }
        }

        private string GetPriceFormat( string defaultClass, string covered, string fcpString, string priceString, string VAIFFString, bool bIsBIG4, bool bIsFSS, bool bIsItemDualPricer, bool bIsTemporary, bool bTPRAlwaysHasBasePrice, Decimal selectedDrugItemFETAmount )
        {
            //HighlightedMissingData
            //HighlightedOutOfRangeData
            //HighlightedTPRWithoutBasePrice
            //UnhighlightedData
            
            string priceFormatClassName = defaultClass;
            decimal price = 0;
            decimal fcp = 0;
            decimal VAIFF = 0;
            decimal calculatedPriceWithoutFET = 0;
            decimal calculatedPriceWithoutIFF = 0;
       
            if( decimal.TryParse( priceString, out price ) == true )
            {
                calculatedPriceWithoutFET = price - selectedDrugItemFETAmount;

                if( calculatedPriceWithoutFET <= 0 )  
                {
                    priceFormatClassName = "HighlightedOutOfRangeData";
                }
                else
                {
            	    if( bIsBIG4 == true || ( bIsFSS == true && bIsItemDualPricer == false ) )
            	    {
                        if( covered.CompareTo( "T" ) == 0 )
                        {
                            if( decimal.TryParse( fcpString, out fcp ) == true )
                            {
                                if( decimal.TryParse( VAIFFString, out VAIFF ) == true )
                                {
                                    calculatedPriceWithoutIFF = Math.Round( calculatedPriceWithoutFET - ( calculatedPriceWithoutFET * VAIFF ), 2 ); //$$$IFF

                                    if( calculatedPriceWithoutIFF > fcp )
                                    {
                                        priceFormatClassName = "HighlightedOutOfRangeData";                         
                                    }
                                }
                            }
                        }
                    }
                }
            

                if( bIsTemporary == true && bTPRAlwaysHasBasePrice == false )
                {
                    priceFormatClassName = "HighlightedTPRWithoutBasePrice";
                }
	    }           
            return ( priceFormatClassName );
        }

        // selectedDrugItemPriceHistoryId = -1 = not from history
        private void OpenPriceDetailsWindow( int itemIndex, int selectedDrugItemId, int priceIndex, int selectedDrugItemPriceId, int selectedDrugItemPriceHistoryId, bool bIsFromHistory, bool bIsHistoryFromArchive )
        {
            Session[ "DrugItemPriceDetailsWindowParms" ] = null;
            Session[ "DrugItemPriceDetailsWindowParms" ] = new DrugItemPriceDetailsWindowParms( itemIndex, selectedDrugItemId, priceIndex, selectedDrugItemPriceId, selectedDrugItemPriceHistoryId, _startupParameters.ContractNumber, _startupParameters.IsPriceEditable, bIsFromHistory, bIsHistoryFromArchive );

            string windowOpenScript = "window.open('DrugItemPriceDetails.aspx','DrugItemPriceDetails','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=200,left=240,width=900,height=412, resizable=0');";
            ScriptManager.RegisterStartupScript( this.Page, typeof( Page ), "ItemPriceDetailsWindowOpenScript", windowOpenScript, true );
        }

        private void InitiateEditModeForPrice( int editIndex )
        {
            DrugItemPricesGridView.EditIndex = editIndex;

            // select the edited item also
            SetDrugItemPricesGridViewSelectedItem( editIndex );

            DrugItemPricesGridView.DataBind();

            // disable appropriate buttons for the selected row
            SetEnabledPriceControlsDuringEdit( DrugItemPricesGridView, editIndex, false );

            SetGridCellFocusDuringEdit( DrugItemPricesGridView, editIndex );
        }

        private void SetDrugItemPricesGridViewSelectedItem( int selectedPriceIndex )
        {
            // save for postback
            Session[ "DrugItemPricesGridViewSelectedIndex" ] = selectedPriceIndex;

            // set the row as selected
            DrugItemPricesGridView.SelectedIndex = selectedPriceIndex;

            // tell the client
            ScrollToSelectedPrice();

            // allow the update postback to occur
            InsertPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScrollToSelectedPrice()
        {
            int rowIndex = DrugItemPricesGridView.SelectedIndex;
            //  TableItemStyle rowStyle = DrugItemsGridView.RowStyle;
            //  int rowHeight = ( int )rowStyle.Height.Value;  // this value is always zero
            int rowPosition = PRICEGRIDVIEWROWHEIGHTESTIMATE * rowIndex;

            string scrollToRowScript = String.Format( "setPriceScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedPriceIndexChangedScript", scrollToRowScript, true ); // runs after controls established
            //     ScriptManager.RegisterClientScriptBlock( this.Page, this.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true );  // runs prior to control rendering
        }

        private void RestoreDrugItemPricesGridViewSelectedItem()
        {
            DrugItemPricesGridView.SelectedIndex = ( int )Session[ "DrugItemPricesGridViewSelectedIndex" ];
        }

        // restores automated check box selection made during validation 
        private void RestoreDrugItemPricesGridViewInterimValues()
        {
            if( Session[ "DrugItemPriceVATandemSelectionRowIndex" ] != null )
            {
                int rowIndex = int.Parse( Session[ "DrugItemPriceVATandemSelectionRowIndex" ].ToString() );
                if( rowIndex != -1 )
                {
                    DrugItemPricesGridView.SetCheckBoxValueInControlInCell( rowIndex, "IsVA", true );
                    DrugItemPricesGridView.SetCheckBoxValueInControlInCell( rowIndex, "IsSVH2", true );
                    DrugItemPricesGridView.SetCheckBoxValueInControlInCell( rowIndex, "IsCMOP", true );

                    // reset
                    Session[ "DrugItemPriceVATandemSelectionRowIndex" ] = -1;
                }
            }
        }

        private void SetEnabledPriceControlsDuringEdit( GridView gv, int rowIndex, bool bEnabled )
        {
            gv.SetEnabledControlsForCell( rowIndex, 0, bEnabled ); // details
            gv.SetEnabledControlsForCell( rowIndex, RemoveButtonFieldNumber, bEnabled ); // remove

            gv.SetVisibleControlsForCell( rowIndex, 1, "EditButton", bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 1, "SaveButton", !bEnabled );
            gv.SetVisibleControlsForCell( rowIndex, 1, "CancelButton", !bEnabled );

            gv.AddCRTrapToCheckBoxes( rowIndex, PriceMatrixFieldOffset, SubItemDropDownFieldNumber - 1 );

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

        private void SetGridCellFocusDuringEdit( GridView gv, int rowIndex )
        {
            gv.SetCellFocusInPanelInLiteral( rowIndex, PriceStartDateFieldNumber, "priceStartDateTextBox" );
        }

        private void SetEnabledPriceControlsForHistorical( GridView gv, bool bEnabled )
        {
            //gv.SetEnabledControlsForCell( rowIndex, 0, bEnabled ); // details
            //gv.SetEnabledControlsForCell( rowIndex, RemoveButtonFieldNumber, bEnabled ); // remove

            //gv.SetVisibleControlsForCell( rowIndex, 1, "EditButton", bEnabled );

            Button addNewPriceButton = ( Button )drugItemForm.FindControl( "AddNewPriceButton" );
            if( addNewPriceButton != null )
            {
                // apply security to add button
                if( _startupParameters.IsPriceEditable == false )
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

        private int DeleteDrugItemPrice( GridView gv, int rowIndex, int selectedDrugItemPriceId )
        {
            // id of row to delete
            _drugItemPriceIdParameter.DefaultValue = selectedDrugItemPriceId.ToString();

            try
            {
                _drugItemPricesDataSource.Delete();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            // previous row gets focus
            if( rowIndex >= 1 )
                rowIndex--;

            SetDrugItemPricesGridViewSelectedItem( rowIndex );

            gv.DataBind();

            return ( rowIndex );
        }

        private int UpdateDrugItemPrice( GridView gv, int rowIndex )
        {
            int updatedRowIndex = -1;

            _drugItemPriceIdParameter.DefaultValue = gv.GetRowIdFromSelectedIndex( rowIndex, 0 ).ToString();
       //     _priceIdParameter.DefaultValue = gv.GetRowIdFromSelectedIndex( rowIndex, 1 ).ToString();
            
            _priceStartDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "priceStartDateTextBox" );
            _priceEndDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "priceEndDateTextBox" );
            _priceParameter.DefaultValue = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "priceTextBox" ), "Price" ).ToString();

            _isTemporaryParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsTemporary" ).ToString();
            _isFSSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsFSS" ).ToString();
            _isBIG4Parameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsBIG4" ).ToString();
            _isVAParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsVA" ).ToString();
            _isBOPParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsBOP" ).ToString();
            _isCMOPParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsCMOP" ).ToString();
            _isDODParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsDOD" ).ToString();
            _isHHSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsHHS" ).ToString();
            _isIHSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsIHS" ).ToString();
            _isIHS2Parameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsIHS2" ).ToString();
            _isDIHSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsDIHS" ).ToString();
            _isNIHParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsNIH" ).ToString();
            _isPHSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsPHS" ).ToString();
            _isSVHParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsSVH" ).ToString();
            _isSVH1Parameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsSVH1" ).ToString();
            _isSVH2Parameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsSVH2" ).ToString();
            _isTMOPParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsTMOP" ).ToString();
            _isUSCGParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsUSCG" ).ToString();
            _isFHCCParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsFHCC" ).ToString();

            int drugItemSubItemId = int.Parse( gv.GetStringValueFromSelectedControl( rowIndex, SubItemDropDownFieldNumber, 0, false, "subItemIdentifierDropDownList" ) );
            if( drugItemSubItemId != -1 )
                _drugItemSubItemIdParameter.DefaultValue = drugItemSubItemId.ToString();
            else
            {
                _drugItemSubItemIdParameter.ConvertEmptyStringToNull = true;
                _drugItemSubItemIdParameter.DefaultValue = String.Empty;
            }
                        
            try
            {
                _drugItemPricesDataSource.Update();
            }
            catch( Exception ex )
            {
                MsgBox.ShowErrorFromUpdatePanel( Page, ex );
            }

            gv.EditIndex = -1; // done with edit
            //         SetDrugItemsGridViewSelectedItem( rowIndex );
            gv.DataBind();

            if( Session[ "LastUpdatedItemPriceId" ] != null )
            {
                int updatedDrugItemPriceId = ( int )Session[ "LastUpdatedItemPriceId" ];
                updatedRowIndex = DrugItemPricesGridView.GetRowIndexFromId( updatedDrugItemPriceId, 0 );

                SetDrugItemPricesGridViewSelectedItem( updatedRowIndex );

                // bind to select
                gv.DataBind();
            }


            // enable appropriate buttons for the selected row
            SetEnabledPriceControlsDuringEdit( gv, updatedRowIndex, true );

            return ( updatedRowIndex );
        }

        private bool ValidateItemBeforeUpdate( GridView gv, int rowIndex, int selectedDrugItemId, ref string validationMessage )
        {
            bool bIsItemOk = true;

            validationMessage = "";

            try
            {
                string fdaAssignedLabelerCode = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 4 ), 0, false, "fdaAssignedLabelerCodeTextBox" );
                string productCode = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 5 ), 0, false, "productCodeTextBox" );
                string packageCode = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 6 ), 0, false, "packageCodeTextBox" );

                string genericName = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 8 ), 0, false, "genericTextBox" );
                string genericNameCleansed = CMGlobals.ReplaceNonPrintableCharacters( genericName ).Trim();

                string tradeName = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 9 ), 0, false, "tradeNameTextBox" );
                string tradeNameCleansed = CMGlobals.ReplaceNonPrintableCharacters( tradeName ).Trim();

                string packageSize = gv.GetStringValueFromSelectedControl( rowIndex, ApplyRightHandColumnOffsetBasedOnDocumentType( 11 ), 0, false, "packageSizeTextBox" );

                int parseResult = 0;
                if( int.TryParse( fdaAssignedLabelerCode, out  parseResult ) == false )
                {
                    throw new Exception( "Fda Assigned Labeler Code is not valid." );
                }

                // radio pharmaceuticals may contain alpha chars in the product and package code fields
                if( fdaAssignedLabelerCode.Length != 5 )
                {
                    throw new Exception( "Fda Assigned Labeler Code must be exactly 5 characters in length." );
                }

                if( productCode.Length != 4 )
                {
                    throw new Exception( "Product Code must be exactly 4 characters in length." );
                }

                if( packageCode.Length != 2 )
                {
                    throw new Exception( "Package Code must be exactly 2 characters in length." );
                }

                if( genericNameCleansed.Trim().Length == 0 )
                {
                    throw new Exception( "Generic name is required." );
                }

                if( tradeNameCleansed.Trim().Length == 0 )
                {
                    throw new Exception( "Trade name is required." );
                }

                if( packageSize.Trim().Length == 0 )
                {
                    throw new Exception( "Package size is required." );
                }

            }
            catch( Exception ex )
            {
                bIsItemOk = false;
                validationMessage = string.Format( "The following exception was encountered validating the item {0}", ex.Message );
            }

            return ( bIsItemOk );
        }

        private bool ValidatePriceBeforeUpdate( GridView gv, int rowIndex, int drugItemPriceId, int drugItemId, ref bool bUserCanOverride, ref bool bWarningOnly, ref string validationMessage, ref string warningMessage )
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
                if( DateTime.TryParse( gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "priceStartDateTextBox" ), out priceStartDate ) != true )
                {
                    throw new Exception( "Price start date is not correctly formatted." );
                }
                
                DateTime priceEndDate;
                if( DateTime.TryParse( gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "priceEndDateTextBox" ), out priceEndDate ) != true )
                {
                    throw new Exception( "Price end date is not correctly formatted." );
                }

                if( DateTime.Compare( priceStartDate, priceEndDate ) >= 0 )
                {
                    throw new Exception( "Price start date must precede price end date." );
                }

                decimal price = 0;
                if( Decimal.TryParse( CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "priceTextBox" ), "Price" ).ToString(), out price ) != true )
                {
                    throw new Exception( "Price is not correctly formatted." );
                }

                bool bIsTemporary = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsTemporary" );
                bool bIsFSS = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsFSS" );
                bool bIsBIG4 = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsBIG4" );
                bool bIsVA = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsVA" );
                bool bIsBOP = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsBOP" );
                bool bIsCMOP = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsCMOP" );
                bool bIsDOD = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsDOD" );
                bool bIsHHS = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsHHS" );
                bool bIsIHS = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsIHS" );
                bool bIsIHS2 = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsIHS2" );
                bool bIsDIHS = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsDIHS" );
                bool bIsNIH = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsNIH" );
                bool bIsPHS = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsPHS" );
                bool bIsSVH = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsSVH" );
                bool bIsSVH1 = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsSVH1" );
                bool bIsSVH2 = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsSVH2" );
                bool bIsTMOP = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsTMOP" );
                bool bIsUSCG = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsUSCG" );
                bool bIsFHCC = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsFHCC" );
                string drugItemSubItemIdString = gv.GetStringValueFromSelectedControl( rowIndex, SubItemDropDownFieldNumber, 0, false, "subItemIdentifierDropDownList" );
                int drugItemSubItemId = int.Parse( drugItemSubItemIdString );

                if( bIsTemporary == false && bIsFSS == false && bIsBIG4 == false && bIsVA == false && bIsBOP == false && bIsCMOP == false
                   && bIsDOD == false && bIsHHS == false && bIsIHS == false && bIsIHS2 == false && bIsDIHS == false
                   && bIsNIH == false && bIsPHS == false && bIsSVH == false && bIsSVH1 == false && bIsSVH2 == false
                   && bIsTMOP == false && bIsUSCG == false && bIsFHCC == false )
                {
                    throw new Exception( "Please select at least one check box to indicate price scope." );
                }

                if( bIsTemporary == true )
                {
                    if( bIsFSS == false && bIsBIG4 == false && bIsVA == false && bIsBOP == false && bIsCMOP == false
                        && bIsDOD == false && bIsHHS == false && bIsIHS == false && bIsIHS2 == false && bIsDIHS == false
                        && bIsNIH == false && bIsPHS == false && bIsSVH == false && bIsSVH1 == false && bIsSVH2 == false
                        && bIsTMOP == false && bIsUSCG == false && bIsFHCC == false )
                    {
                        throw new Exception( "Temporary price must also select FSS or organizations." );
                    }
                }

                if( bIsFSS == true )
                {
                    if(  bIsBIG4 == true || bIsVA == true || bIsBOP == true || bIsCMOP == true
                        || bIsDOD == true || bIsHHS == true || bIsIHS == true || bIsIHS2 == true || bIsDIHS == true
                        || bIsNIH == true || bIsPHS == true || bIsSVH == true || bIsSVH1 == true || bIsSVH2 == true
                        || bIsTMOP == true || bIsUSCG == true || bIsFHCC == true )
                    {
                        throw new Exception( "FSS price requires an entry on it's own row apart from BIG4 or restricted prices." );
                    }
                }

                if( bIsBIG4 == true )
                {
                    if( bIsFSS == true || bIsVA == true || bIsBOP == true || bIsCMOP == true
                        || bIsDOD == true || bIsHHS == true || bIsIHS == true || bIsIHS2 == true || bIsDIHS == true
                        || bIsNIH == true || bIsPHS == true || bIsSVH == true || bIsSVH1 == true || bIsSVH2 == true
                        || bIsTMOP == true || bIsUSCG == true || bIsFHCC == true )
                    {
                        throw new Exception( "BIG4 price requires an entry on it's own row apart from FSS or restricted prices." );
                    }
                }

                // VA, SVH2 and CMOP must always be selected in tandem
                if( bIsVA == true || bIsSVH2 == true || bIsCMOP == true )
                {
                    if( bIsVA == false || bIsSVH2 == false || bIsCMOP == false )
                    {
                        bIsPriceOk = true;
                        bWarningOnly = true;
                        warningMessage = "VA, SVH2 and CMOP must be selected in tandem. Price restriction will be updated to reflect selection of all three organizations.";
                        // must still validate for price pattern overlap
                        bIsVA = true;
                        bIsSVH2 = true;
                        bIsCMOP = true;
                        gv.SetCheckBoxValueInControlInCell( rowIndex, "IsVA", true );
                        gv.SetCheckBoxValueInControlInCell( rowIndex, "IsSVH2", true );
                        gv.SetCheckBoxValueInControlInCell( rowIndex, "IsCMOP", true );
                        // save for restore on postback
                        Session[ "DrugItemPriceVATandemSelectionRowIndex" ] = rowIndex;
                    }
                    else
                    {
                        // reset
                        Session[ "DrugItemPriceVATandemSelectionRowIndex" ] = -1;
                    }
                }
               
                DrugItemDB drugItemDB = ( DrugItemDB )Session[ "DrugItemDB" ];
                drugItemDB.MakeConnectionString();  // added 11/21/2016 - if theres a bug in item count skips this init
                bSuccess = drugItemDB.ValidatePriceAgainstOtherPrices( _startupParameters.ContractNumber,
                    drugItemPriceId, drugItemId, priceStartDate, priceEndDate, bIsTemporary, bIsFSS, bIsBIG4, bIsVA,
                    bIsBOP, bIsCMOP, bIsDOD, bIsHHS, bIsIHS, bIsIHS2, bIsDIHS, bIsNIH, bIsPHS, bIsSVH,
                    bIsSVH1, bIsSVH2, bIsTMOP, bIsUSCG, bIsFHCC, drugItemSubItemId, ref bIsPriceOk, ref bUserCanOverride, ref validationMessage );
                if( bSuccess == false )
                {
                    throw new Exception( string.Format( "Exception encountered validating price pattern {0}", drugItemDB.ErrorMessage ) );
                }
                else
                {
                    if( bIsPriceOk == true )
                    {
                        if( validationMessage.Contains( "base price" ) == true )
                        {
                            bWarningOnly = true;
                            warningMessage = validationMessage;
                        }

                    }
                }
            }
            catch( Exception ex )
            {
                bIsPriceOk = false;
                validationMessage = string.Format( "The following exception was encountered validating the price: {0}", ex.Message );
            }

            return ( bIsPriceOk );
        }

        private int InsertDrugItemPrice( GridView gv, int rowIndex, int selectedDrugItemId )
        {
            int insertedRowIndex = 0;

            _drugItemIdForPricesParameter.DefaultValue = selectedDrugItemId.ToString();
            _priceStartDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 1, 0, false, "priceStartDateTextBox" );
            _priceEndDateParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 2, 0, false, "priceEndDateTextBox" );
            _priceParameter.DefaultValue = CMGlobals.GetMoneyFromString( gv.GetStringValueFromSelectedControl( rowIndex, 3, 0, false, "priceTextBox" ), "Price" ).ToString();

            _isTemporaryParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsTemporary" ).ToString();
            _isFSSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsFSS" ).ToString();
            _isBIG4Parameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsBIG4" ).ToString();
            _isVAParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsVA" ).ToString();
            _isBOPParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsBOP" ).ToString();
            _isCMOPParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsCMOP" ).ToString();
            _isDODParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsDOD" ).ToString();
            _isHHSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsHHS" ).ToString();
            _isIHSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsIHS" ).ToString();
            _isIHS2Parameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsIHS2" ).ToString();
            _isDIHSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsDIHS" ).ToString();
            _isNIHParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsNIH" ).ToString();
            _isPHSParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsPHS" ).ToString();
            _isSVHParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsSVH" ).ToString();
            _isSVH1Parameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsSVH1" ).ToString();
            _isSVH2Parameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsSVH2" ).ToString();
            _isTMOPParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsTMOP" ).ToString();
            _isUSCGParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsUSCG" ).ToString();
            _isFHCCParameter.DefaultValue = gv.GetValueFromSelectedIndexForCheckBoxField( rowIndex, "IsFHCC" ).ToString();
            //     _drugItemSubItemIdParameter.DefaultValue = gv.GetStringValueFromSelectedControl( rowIndex, 24, 0, false, "subItemIdentifierDropDownList" );


            int drugItemSubItemId = int.Parse( gv.GetStringValueFromSelectedControl( rowIndex, SubItemDropDownFieldNumber, 0, false, "subItemIdentifierDropDownList" ) );
            if( drugItemSubItemId != -1 )
                _drugItemSubItemIdParameter.DefaultValue = drugItemSubItemId.ToString();
            else
            {
                _drugItemSubItemIdParameter.ConvertEmptyStringToNull = true;
                _drugItemSubItemIdParameter.DefaultValue = String.Empty;
            }

            try
            {
                _drugItemPricesDataSource.Insert();
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

            if( Session[ "LastInsertedDrugItemPriceId" ] != null )
            {
                int newDrugItemPriceId = ( int )Session[ "LastInsertedDrugItemPriceId" ];
                insertedRowIndex = DrugItemPricesGridView.GetRowIndexFromId( newDrugItemPriceId, 0 );

                // new item falls out of range of current filter, so go to item 0 instead
                if( insertedRowIndex == -1 )
                {
                    if( gv.HasData() == true )
                        insertedRowIndex = 0;
                }

                if( insertedRowIndex != -1 )
                {
                    SetDrugItemPricesGridViewSelectedItem( insertedRowIndex );

                    // bind to select
                    gv.DataBind();
                }
            }

            // enable appropriate buttons for the selected row
            SetEnabledPriceControlsDuringEdit( gv, insertedRowIndex, true );

            return ( insertedRowIndex );
        }

        protected void _drugItemPricesDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@DrugItemPriceId" ].Value != null )
            {
                string drugItemPriceIdString = e.Command.Parameters[ "@DrugItemPriceId" ].Value.ToString();
                //    string priceIdString = e.Command.Parameters[ "@PriceId" ].Value.ToString();

                if( drugItemPriceIdString.Length > 0 )
                {
                    int drugItemPriceId = int.Parse( drugItemPriceIdString );
                    Session[ "LastInsertedDrugItemPriceId" ] = drugItemPriceId;
                }

                //if( priceIdString.Length > 0 )
                //{
                //    int priceId = int.Parse( priceIdString );
                //    Session[ "LastInsertedPriceId" ] = priceId;
                //}
            }
            else
            {
                throw new Exception( "DrugItemPriceId returned from insert was null. Insert failed." );
            }
        }

        protected void DrugItemPricesGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;

                if( e.Row.RowType == DataControlRowType.DataRow )
                {
                    if( ( ( e.Row.RowState & DataControlRowState.Edit ) != DataControlRowState.Edit ) &&
                        ( ( e.Row.RowState & DataControlRowState.Insert ) != DataControlRowState.Insert ) )
                    {
                        //string lastModificationType = "";
                        //string lastModificationTypeDescription = "";
                        //int cellIndex = gv.GetIndexFromFieldNameForBoundField( "LastModificationType" );

                        //lastModificationType = e.Row.Cells[ cellIndex ].Text;  // gv.GetStringValueFromSelectedIndexForBoundField( e.Row.RowIndex, "LastModificationType" );
                        //lastModificationTypeDescription = CMGlobals.GetLastModificationTypeDescription( lastModificationType );
                        //e.Row.Cells[ cellIndex ].Text = lastModificationTypeDescription;
                    }
                    else
                    {
                        DropDownList subItemIdentifierDropDownList = ( DropDownList )e.Row.FindControl( "subItemIdentifierDropDownList" );
                        if( subItemIdentifierDropDownList != null )
                        {
                            subItemIdentifierDropDownList.DataSource = _subItemIdentifierDataSource;
                            subItemIdentifierDropDownList.DataBind();
                        }
                    }

                    if( _startupParameters.IsPriceEditable == false )
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

        protected void DrugItemPricesGridView_RowEditing( object sender, GridViewEditEventArgs e )
        {
            InitiateEditModeForPrice( e.NewEditIndex );
        }

        protected void DrugItemPricesGridView_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            CancelPriceEdit( e.RowIndex );

            //int cancelIndex = e.RowIndex;
            //bool bInserting = DrugItemPricesGridView.InsertRowActive;

            //// if inserting
            //if( bInserting == true )
            //{
            //    DrugItemPricesGridView.InsertRowActive = false; // cancels insert ( if inserting )
            //    _withAddPriceParameter.DefaultValue = "false";
            //    DrugItemPricesGridView.EditIndex = -1; // cancels the edit
            //    DrugItemPricesGridView.DataBind();

            //    // enable appropriate buttons for the selected row
            //    SetEnabledPriceControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

            //    HighlightDrugItemPriceRow( 0 );

            //    // allow the cancel postback to occur 
            //    EditCancelPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            //}
            //else
            //{
            //    DrugItemPricesGridView.EditIndex = -1; // cancels the edit
            //    DrugItemPricesGridView.DataBind();

            //    // enable appropriate buttons for the selected row
            //    SetEnabledPriceControlsDuringEdit( ( ( GridView )sender ), e.RowIndex, true );

            //    HighlightDrugItemPriceRow( cancelIndex );

            //    // allow the cancel postback to occur 
            //    EditCancelPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            //}
        }

        protected void CancelPriceEdit( int rowIndex )
        {
            bool bInserting = DrugItemPricesGridView.InsertRowActive;

            // if inserting
            if( bInserting == true )
            {
                DrugItemPricesGridView.InsertRowActive = false; // cancels insert ( if inserting )
                _withAddPriceParameter.DefaultValue = "false";
                DrugItemPricesGridView.EditIndex = -1; // cancels the edit
                DrugItemPricesGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledPriceControlsDuringEdit( DrugItemPricesGridView, rowIndex, true );

                HighlightDrugItemPriceRow( 0 );

                // allow the cancel postback to occur 
                EditCancelPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
            else
            {
                DrugItemPricesGridView.EditIndex = -1; // cancels the edit
                DrugItemPricesGridView.DataBind();

                // enable appropriate buttons for the selected row
                SetEnabledPriceControlsDuringEdit( DrugItemPricesGridView, rowIndex, true );

                HighlightDrugItemPriceRow( rowIndex );

                // allow the cancel postback to occur 
                EditCancelPriceButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }

        }

        // probably wont happen - id changing during update
        protected void _drugItemPricesDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string drugItemPriceIdString = e.Command.Parameters[ "@DrugItemPriceId" ].Value.ToString();

            if( drugItemPriceIdString.Length > 0 )
            {
                int drugItemPriceId = int.Parse( drugItemPriceIdString );
                Session[ "LastUpdatedItemPriceId" ] = drugItemPriceId;
            }
        }

        protected void HighlightDrugItemPriceRow( int itemIndex )
        {
            if( itemIndex >= 0 && DrugItemPricesGridView.Rows.Count > 0 )
            {

                string highlightedRowOriginalColor = "";
                int highlightedRowIndex = itemIndex + 1;

                try
                {
                    GridViewRow row = DrugItemPricesGridView.Rows[ itemIndex ]; // $$$ this threw an index exception when saving drug item changes, redid the same and then it did not throw, added try catch to mask since still happens even with range check
                    if( row.RowState == DataControlRowState.Alternate )
                    {
                        highlightedRowOriginalColor = DrugItemPricesGridView.AlternatingRowStyle.BackColor.ToString();
                    }
                    else
                    {
                        highlightedRowOriginalColor = DrugItemPricesGridView.RowStyle.BackColor.ToString();
                    }

                    string preserveHighlightingScript = String.Format( "setDrugItemPriceHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
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

        private void RefreshDrugItemPriceScreenDueToDetailsChanged( bool bRebindItemPrices )
        {
           // DrugItemPricesGridView.DataBind(); // if sub item id is changed in item, reflect to pricelist
            BindDrugItemPricesHeaderAndGrid(); // $$$ changed simple bind to this call which also updates the header 2/24/2016

            int currentPriceIndex = DrugItemPricesGridView.SelectedIndex;
            ScrollToSelectedPrice();
            HighlightDrugItemPriceRow( currentPriceIndex );
        }

        protected void DrugItemPricesGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;

                 // upon first entry after document creation, 
                // e.Row.Cells.Count is 1 or -1, make sure count is ok before proceeding
                if( e.Row.Cells.Count > VAIFFFieldNumber &&
                    e.Row.Cells.Count > DualPriceDesignationInPriceFieldNumber &&
                    e.Row.Cells.Count > IsFromHistoryFieldNumber &&
                    e.Row.Cells.Count > IsHistoryFromArchiveFieldNumber &&
                    e.Row.Cells.Count > TPRAlwaysHasBasePriceFieldNumber )
                {
                    // disable price matrix checkboxes based on document type
                    DisablePriceMatrixForRow( e.Row );

                    // hide VAIFF
                    e.Row.Cells[ VAIFFFieldNumber ].Visible = false;

                    // hide dual price designation
                    e.Row.Cells[ DualPriceDesignationInPriceFieldNumber ].Visible = false;

                    // hide is history
                    e.Row.Cells[ IsFromHistoryFieldNumber ].Visible = false;

                    // hide is history from archive
                    e.Row.Cells[ IsHistoryFromArchiveFieldNumber ].Visible = false;

                    // hide always has base price
                    e.Row.Cells[ TPRAlwaysHasBasePriceFieldNumber ].Visible = false;
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        protected void DrugItemPricesGridView_OnPreRender( object sender, EventArgs e )
        {

            GridView drugItemPricesGridView = ( GridView )sender;
            GridViewRow headerRow = drugItemPricesGridView.HeaderRow;

            // no prices
            if( headerRow == null )
                return;

            // hide VAIFF
            headerRow.Cells[ VAIFFFieldNumber ].Visible = false;

            // hide Dual Price Designation
            headerRow.Cells[ DualPriceDesignationInPriceFieldNumber ].Visible = false;

            // hide is history
            headerRow.Cells[ IsFromHistoryFieldNumber ].Visible = false;

            // hide is history from archive
            headerRow.Cells[ IsHistoryFromArchiveFieldNumber ].Visible = false;

            // hide always has base price
            headerRow.Cells[ TPRAlwaysHasBasePriceFieldNumber ].Visible = false;

            foreach( GridViewRow gridViewRow in drugItemPricesGridView.Rows )
            {
                // hide VAIFF
                gridViewRow.Cells[ VAIFFFieldNumber ].Visible = false;
                // hide Dual Price Designation
                gridViewRow.Cells[ DualPriceDesignationInPriceFieldNumber ].Visible = false;
                // hide is history
                gridViewRow.Cells[ IsFromHistoryFieldNumber ].Visible = false;
                // hide is history from archive
                gridViewRow.Cells[ IsHistoryFromArchiveFieldNumber ].Visible = false;
                // hide always has base price
                gridViewRow.Cells[ TPRAlwaysHasBasePriceFieldNumber ].Visible = false;
            }

        }

        //protected void SubItemIdentifierLabel_OnDataBinding( object sender, EventArgs e )
        //{
        //    Label subItemIdentifierLabel = ( Label )sender;
        //    GridViewRow gridViewRow = ( GridViewRow )subItemIdentifierLabel.NamingContainer;
            
        //    int drugItemSubItemId = -1;
        //    string subItemIdentifier = "";
        //    bool bSuccess = false;

        //    DrugItemDB drugItemDB = ( DrugItemDB )Page.Session[ "DrugItemDB" ];
        //    if( drugItemDB != null )
        //    {
        //        if( gridViewRow.DataItem != null )
        //        {
        //            subItemIdentifier = ( ( DataRowView )gridViewRow.DataItem )[ "HistoricalNValue" ].ToString();
        //            subItemIdentifierLabel.Text = subItemIdentifier;

        //            //string drugItemSubItemIdString = ( ( DataRowView )gridViewRow.DataItem )[ "DrugItemSubItemId" ].ToString();
        //            //if( drugItemSubItemIdString != null )
        //            //{
        //            //    if( int.TryParse( drugItemSubItemIdString, out drugItemSubItemId ) )
        //            //    {
        //            //        bSuccess = drugItemDB.GetSubItemIdentifier( drugItemSubItemId, out subItemIdentifier );
        //            //        if( bSuccess == true )
        //            //        {
        //            //            subItemIdentifierLabel.Text = subItemIdentifier;
        //            //        }
        //            //    }
        //            //}
        //        }
        //    }
        //}

        protected void SubItemIdentifierDropDownList_DataBound( object sender, EventArgs e )
        {
            DropDownList subItemIdentifierDropDownList = ( DropDownList )sender;
            GridViewRow gridViewRow = ( GridViewRow )subItemIdentifierDropDownList.NamingContainer;
            int drugItemSubItemId = -1;

            if( gridViewRow.DataItem != null )
            {
                string drugItemSubItemIdString = ( ( DataRowView )gridViewRow.DataItem )[ "DrugItemSubItemId" ].ToString();
                if( drugItemSubItemIdString != null )
                {
                    if( int.TryParse( drugItemSubItemIdString, out drugItemSubItemId ) )
                    {
                        ListItem listItem = subItemIdentifierDropDownList.Items.FindByValue( drugItemSubItemId.ToString() );
                        if( listItem != null )
                            listItem.Selected = true;
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

        protected void DrugItemsScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "DrugItemsErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "DrugItemsErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            ScriptManager1.AsyncPostBackErrorMessage = errorMsg;
        }


    }
}
