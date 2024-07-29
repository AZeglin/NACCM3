using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // parms set when opening ItemPriceDetails window
    [Serializable]
    public class ItemPriceDetailsWindowParms
    {
        private int _itemIndex = -1; // id of the row in the item grid that opened the window

        public ItemPriceDetailsWindowParms( int itemIndex, int selectedItemId, string itemCatalogNumber, string itemDescription, bool bIsBPA, bool bIsBOA, bool bIsNational, int priceIndex, int selectedPriceId, int selectedPriceHistoryId, string contractNumber, int contractId, bool bIsPriceEditable, bool bIsPriceDetailsEditable, bool bIsFromHistory, DateTime contractExpirationDate, bool bIsService )
        {
            _itemIndex = itemIndex;
            _selectedItemId = selectedItemId;
            _itemCatalogNumber = itemCatalogNumber;
            _itemDescription = itemDescription;
            _bIsBPA = bIsBPA;
            _bIsBOA = bIsBOA;
            _bIsNational = bIsNational;
            _priceIndex = priceIndex;
            _selectedPriceId = selectedPriceId;
            _contractNumber = contractNumber;
            _contractId = contractId;
            _bIsPriceEditable = bIsPriceEditable;
            _bIsPriceDetailsEditable = bIsPriceDetailsEditable;
            _selectedPriceHistoryId = selectedPriceHistoryId; // this is only populated if the user has selected a row from price history
            _bIsFromHistory = bIsFromHistory;
            _contractExpirationDate = contractExpirationDate;
            _bIsService = bIsService;
        }

        public int ItemIndex
        {
            get { return _itemIndex; }
            set { _itemIndex = value; }
        }
        private int _selectedItemId = -1; // id of the corresponding record

        public int SelectedItemId
        {
            get { return _selectedItemId; }
            set { _selectedItemId = value; }
        }

        private string _itemCatalogNumber = "";

        public string ItemCatalogNumber
        {
            get { return _itemCatalogNumber; }
            set { _itemCatalogNumber = value; }
        }

        private string _itemDescription = "";

        public string ItemDescription
        {
            get { return _itemDescription; }
            set { _itemDescription = value; }
        }

        private bool _bIsBPA = false;

        public bool IsBPA
        {
            get { return _bIsBPA; }
            set { _bIsBPA = value; }
        }

        private bool _bIsBOA = false;

        public bool IsBOA
        {
            get { return _bIsBOA; }
            set { _bIsBOA = value; }
        }

        private bool _bIsNational = false;

        public bool IsNational
        {
            get { return _bIsNational; }
            set { _bIsNational = value; }
        }

        private int _selectedPriceHistoryId = -1;

        public int SelectedPriceHistoryId
        {
            get { return _selectedPriceHistoryId; }
            set { _selectedPriceHistoryId = value; }
        } 

        private string _contractNumber = "";

        public string ContractNumber
        {
            get { return _contractNumber; }
            set { _contractNumber = value; }
        }

        private int _contractId = -1;

        public int ContractId
        {
            get { return _contractId; }
            set { _contractId = value; }
        }

        private int _priceIndex = -1;

        public int PriceIndex
        {
            get { return _priceIndex; }
            set { _priceIndex = value; }
        }

        private int _selectedPriceId = -1;

        public int SelectedPriceId
        {
            get { return _selectedPriceId; }
            set { _selectedPriceId = value; }
        }

        private bool _bIsPriceEditable = false;

        public bool IsPriceEditable
        {
            get { return _bIsPriceEditable; }
            set { _bIsPriceEditable = value; }
        }

        private bool _bIsPriceDetailsEditable = false;

        public bool IsPriceDetailsEditable
        {
            get { return _bIsPriceDetailsEditable; }
            set { _bIsPriceDetailsEditable = value; }
        }

        private bool _bIsFromHistory = false;

        public bool IsFromHistory
        {
            get { return _bIsFromHistory; }
            set { _bIsFromHistory = value; }
        }

        private DateTime _contractExpirationDate = DateTime.Today;

        public DateTime ContractExpirationDate
        {
            get { return _contractExpirationDate; }
            set { _contractExpirationDate = value; }
        }

        private bool _bIsService = false;

        public bool IsService
        {
            get
            {
                return _bIsService;
            }
            set
            {
                _bIsService = value;
            }
        }
    }
}
