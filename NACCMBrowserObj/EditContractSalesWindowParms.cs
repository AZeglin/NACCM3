using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // parms set when opening the edit contract sales window
    [Serializable]
    public class EditContractSalesWindowParms
    {
        // editing existing quarter
        public EditContractSalesWindowParms( int itemIndex, string contractNumber, int quarterId, int year, int quarter )
        {
            _bIsNewQuarter = false;
            _itemIndex = itemIndex;
            _contractNumber = contractNumber;
            _quarterId = quarterId;
            _year = year;
            _quarter = quarter;
        }

        // adding new quarter
        public EditContractSalesWindowParms( string contractNumber )
        {
            _bIsNewQuarter = true;
            _contractNumber = contractNumber;
        }

        private bool _bIsNewQuarter = false;

        public bool IsNewQuarter
        {
            get { return _bIsNewQuarter; }
            set { _bIsNewQuarter = value; }
        }

        public bool HasNewQuarterBeenSelected
        {
            get
            {
                return ( ( _quarterId != -1 ) ? true : false );
            }
        }

        private int _itemIndex = -1; // id of the row in the item grid that opened the window

        public int ItemIndex
        {
            get { return _itemIndex; }
            set { _itemIndex = value; }
        }

        private int _quarterId = -1;

        public int QuarterId
        {
            get { return _quarterId; }
            set { _quarterId = value; }
        }

        private string _contractNumber = "";

        public string ContractNumber
        {
            get { return _contractNumber; }
            set { _contractNumber = value; }
        }

        private int _year = DateTime.MinValue.Year;

        public int Year
        {
            get { return _year; }
            set { _year = value; }
        }

        private int _quarter = -1;

        public int Quarter
        {
            get { return _quarter; }
            set { _quarter = value; }
        }
    }
}
