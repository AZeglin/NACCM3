using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // parms set when opening DrugItemPriceDetails window
    [Serializable]
    public class DrugItemPriceDetailsWindowParms
    {
        private int _itemIndex = -1; // id of the row in the item grid that opened the window

        public DrugItemPriceDetailsWindowParms( int itemIndex, int selectedDrugItemId, int priceIndex, int selectedPriceId, int selectedPriceHistoryId, string contractNumber, bool bIsEditable, bool bIsFromHistory, bool bIsHistoryFromArchive )
        {
            _itemIndex = itemIndex;
            _selectedDrugItemId = selectedDrugItemId;
            _priceIndex = priceIndex;
            _selectedPriceId = selectedPriceId;
            _contractNumber = contractNumber;
            _bIsEditable = bIsEditable;
            _selectedPriceHistoryId = selectedPriceHistoryId; // this is only populated if the user has selected a row from price history
            _bIsFromHistory = bIsFromHistory;
            _bIsHistoryFromArchive = bIsHistoryFromArchive;
        }

        public int ItemIndex
        {
            get { return _itemIndex; }
            set { _itemIndex = value; }
        }
        private int _selectedDrugItemId = -1; // id of the corresponding record

        public int SelectedDrugItemId
        {
            get { return _selectedDrugItemId; }
            set { _selectedDrugItemId = value; }
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

        private bool _bIsEditable = false;

        public bool IsEditable
        {
            get { return _bIsEditable; }
            set { _bIsEditable = value; }
        }

        private bool _bIsFromHistory = false;

        public bool IsFromHistory
        {
            get { return _bIsFromHistory; }
            set { _bIsFromHistory = value; }
        }

        private bool _bIsHistoryFromArchive = false;

        public bool IsHistoryFromArchive
        {
            get { return _bIsHistoryFromArchive; }
            set { _bIsHistoryFromArchive = value; }
        }

    }
}
