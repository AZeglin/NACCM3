using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // parms set when opening DrugItemDetails window
    [Serializable]
    public class DrugItemDetailsWindowParms
    {
        private int _itemIndex = -1; // id of the row in the item grid that opened the window

        public DrugItemDetailsWindowParms( int itemIndex, int selectedDrugItemId, string contractNumber, bool bIsCOEditable, bool bIsPBMEditable, bool bIsBPA )
        {
            _itemIndex = itemIndex;
            _selectedDrugItemId = selectedDrugItemId;
            _contractNumber = contractNumber;
            _bIsCOEditable = bIsCOEditable;
            _bIsPBMEditable = bIsPBMEditable;

            _bIsBPA = bIsBPA;
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

        private string _contractNumber = "";

        public string ContractNumber
        {
            get { return _contractNumber; }
            set { _contractNumber = value; }
        }

        private bool _bIsCOEditable = false;

        public bool IsCOEditable
        {
            get { return _bIsCOEditable; }
            set { _bIsCOEditable = value; }
        }

        private bool _bIsPBMEditable = false;

        public bool IsPBMEditable
        {
            get { return _bIsPBMEditable; }
            set { _bIsPBMEditable = value; }
        }

        private bool _bIsBPA = false;

        public bool IsBPA
        {
            get { return _bIsBPA; }
            set { _bIsBPA = value; }
        }
    }
}
