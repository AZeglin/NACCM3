using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // parms set when opening ChangeRequestDetails window
    [Serializable]
    public class ChangeRequestDetailsWindowParms
    {
        private int _itemIndex = -1; // id of the row in the item grid that opened the window

        public ChangeRequestDetailsWindowParms( int itemIndex, int selectedDrugItemId, string contractNumber, bool bIsEditable )
        {
            _itemIndex = itemIndex;
            _selectedDrugItemId = selectedDrugItemId;
            _contractNumber = contractNumber;
            _bIsEditable = bIsEditable;
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

        private bool _bIsEditable = false;

        public bool IsEditable
        {
            get { return _bIsEditable; }
            set { _bIsEditable = value; }
        }
        
    }
}
