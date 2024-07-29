using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
     // parms set when opening the copy contract window
    [Serializable]
    public class CopyContractWindowParms
    {
        private int _itemIndex = -1; // id of the row in the item grid that opened the window


        public CopyContractWindowParms( int itemIndex, int selectedContractId, string contractNumber, string contractorName, string commodityCovered, int scheduleNumber )
        {
            _itemIndex = itemIndex;
            _selectedContractId = selectedContractId;
            _contractNumber = contractNumber;
            _contractorName = contractorName;
            _commodityCovered = commodityCovered;
            _scheduleNumber = scheduleNumber;
        }

        private int _scheduleNumber = -1;

        public int ScheduleNumber
        {
            get { return _scheduleNumber; }
            set { _scheduleNumber = value; }
        }

        private string _contractorName = "";

        public string ContractorName
        {
            get { return _contractorName; }
            set { _contractorName = value; }
        }

        private string _commodityCovered = "";

        public string CommodityCovered
        {
            get { return _commodityCovered; }
            set { _commodityCovered = value; }
        }

        public int ItemIndex
        {
            get { return _itemIndex; }
            set { _itemIndex = value; }
        }
        
        private int _selectedContractId = -1; // id of the corresponding record

        public int SelectedContractId
        {
            get { return _selectedContractId; }
            set { _selectedContractId = value; }
        }

        private string _contractNumber = "";

        public string ContractNumber
        {
            get { return _contractNumber; }
            set { _contractNumber = value; }
        }

    }
}
