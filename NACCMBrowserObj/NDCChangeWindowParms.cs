using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // parms set when opening NDCChange window
    [Serializable]
    public class NDCChangeWindowParms
    {
        private int _itemIndex = -1; // id of the row in the item grid that opened the window


        public NDCChangeWindowParms( int itemIndex, int selectedDrugItemId, string contractNumber, bool bIsEditable,
                                    string fdaAssignedLabelerCode, string productCode, string packageCode, string genericName, string tradeName, int modificationStatusId )
        {
            _itemIndex = itemIndex;
            _selectedDrugItemId = selectedDrugItemId;
            _contractNumber = contractNumber;
            _bIsEditable = bIsEditable;
            _fdaAssignedLabelerCode = fdaAssignedLabelerCode;
            _productCode = productCode;
            _packageCode = packageCode;
            _genericName = genericName;
            _tradeName = tradeName;
            _modificationStatusId = modificationStatusId;
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

        private string _fdaAssignedLabelerCode = "";

        public string FdaAssignedLabelerCode
        {
            get { return _fdaAssignedLabelerCode; }
            set { _fdaAssignedLabelerCode = value; }
        }
        
        private string _productCode = "";

        public string ProductCode
        {
            get { return _productCode; }
            set { _productCode = value; }
        }
        
        private string _packageCode = "";

        public string PackageCode
        {
            get { return _packageCode; }
            set { _packageCode = value; }
        }
        
        private string _genericName = "";

        public string GenericName
        {
            get { return _genericName; }
            set { _genericName = value; }
        }
        
        private string _tradeName = "";

        public string TradeName
        {
            get { return _tradeName; }
            set { _tradeName = value; }
        }

        private int _modificationStatusId = -1;

        public int ModificationStatusId
        {
            get { return _modificationStatusId; }
            set { _modificationStatusId = value; }
        }

    }
}
