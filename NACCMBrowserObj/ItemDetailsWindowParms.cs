using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // parms set when opening ItemDetails window
    [Serializable]
    public class ItemDetailsWindowParms
    {
        private int _itemIndex = -1; // id of the row in the item grid that opened the window

        public ItemDetailsWindowParms( int itemIndex, int selectedItemId, string contractNumber, int contractId, bool bIsEditable, bool bIsBPA, bool bIsService )
        {
            _itemIndex = itemIndex;
            _selectedItemId = selectedItemId;
            _contractNumber = contractNumber;
            _contractId = contractId;
            _bIsEditable = bIsEditable;

            _bIsBPA = bIsBPA;
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

        private string _contractNumber = "";

        public string ContractNumber
        {
            get { return _contractNumber; }
            set { _contractNumber = value; }
        }

        private int _contractId = -1;

        public int ContractId
        {
            get
            {
                return _contractId;
            }
            set
            {
                _contractId = value;
            }
        }

        private bool _bIsEditable = false;

        public bool IsEditable
        {
            get { return _bIsEditable; }
            set { _bIsEditable = value; }
        }
       
        private bool _bIsBPA = false;

        public bool IsBPA
        {
            get { return _bIsBPA; }
            set { _bIsBPA = value; }
        }

        private bool _bIsService = false;
        
        public bool IsService
        {
            get { return _bIsService; }
            set { _bIsService = value; }
        }        
    }
}
