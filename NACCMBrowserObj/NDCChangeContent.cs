using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // fields for NDCChangeFormView to bind to
    [Serializable]
    public class NDCChangeContent
    {
        private int _selectedDrugItemId = -1; // id of the corresponding record
        private string _fdaAssignedLabelerCode = "";
        private string _productCode = "";
        private string _packageCode = "";
        private int _modificationStatusId = -1;
        private bool _bCopyPricing = true;
        private bool _bCopySubItems = true;
        private DateTime _discontinuationDateForOldNDC;
        private DateTime _effectiveDateForNewNDC;
        private string _contractNumber = "";

        // results of insert
        private int _newDrugItemId = -1;
        private int _newDrugItemNDCId = -1;
        private int _newDrugItemPackageId = -1;


        public NDCChangeContent()
        {
        }

        // window parms provide most of the change parameters
        public NDCChangeContent( NDCChangeWindowParms parms )
        {
            _fdaAssignedLabelerCode = parms.FdaAssignedLabelerCode;
            _productCode = parms.ProductCode;
            _packageCode = parms.PackageCode;
            _modificationStatusId = parms.ModificationStatusId;
            _selectedDrugItemId = parms.SelectedDrugItemId;
            _contractNumber = parms.ContractNumber;
        }

        public NDCChangeContent( string fdaAssignedLabelerCode, string productCode, string packageCode, DateTime discontinuationDateForOldNDC, DateTime effectiveDateForNewNDC, bool bCopyPricing, bool bCopySubItems )
        {
            _fdaAssignedLabelerCode = fdaAssignedLabelerCode;
            _productCode = productCode;
            _packageCode = packageCode;
            _discontinuationDateForOldNDC = discontinuationDateForOldNDC;
            _effectiveDateForNewNDC = effectiveDateForNewNDC;
            _bCopyPricing = bCopyPricing;
            _bCopySubItems = bCopySubItems;
        }

        public int SelectedDrugItemId
        {
            get { return _selectedDrugItemId; }
            set { _selectedDrugItemId = value; }
        }

        public string FdaAssignedLabelerCode
        {
            get { return _fdaAssignedLabelerCode; }
            set { _fdaAssignedLabelerCode = value; }
        }

        public string ProductCode
        {
            get { return _productCode; }
            set { _productCode = value; }
        }

        public string PackageCode
        {
            get { return _packageCode; }
            set { _packageCode = value; }
        }

        public int ModificationStatusId
        {
            get { return _modificationStatusId; }
            set { _modificationStatusId = value; }
        }

        public bool CopyPricing
        {
            get { return _bCopyPricing; }
            set { _bCopyPricing = value; }
        }

        public bool CopySubItems
        {
            get { return _bCopySubItems; }
            set { _bCopySubItems = value; }
        }

        public DateTime DiscontinuationDateForOldNDC
        {
            get { return _discontinuationDateForOldNDC; }
            set { _discontinuationDateForOldNDC = value; }
        }

        public DateTime EffectiveDateForNewNDC
        {
            get { return _effectiveDateForNewNDC; }
            set { _effectiveDateForNewNDC = value; }
        }

        public string ContractNumber
        {
          get { return _contractNumber; }
          set { _contractNumber = value; }
        }

        public int NewDrugItemId
        {
            get { return _newDrugItemId; }
            set { _newDrugItemId = value; }
        }

        public int NewDrugItemNDCId
        {
            get { return _newDrugItemNDCId; }
            set { _newDrugItemNDCId = value; }
        }

        public int NewDrugItemPackageId
        {
            get { return _newDrugItemPackageId; }
            set { _newDrugItemPackageId = value; }
        }
 
    }
}
