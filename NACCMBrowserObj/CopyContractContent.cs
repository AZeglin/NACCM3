using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // fields for CopyContractFormView to bind to
    [Serializable]
    public class CopyContractContent
    {

        private string _oldContractNumber;

        public string OldContractNumber
        {
            get { return _oldContractNumber; }
            set { _oldContractNumber = value; }
        }
        private string _newContractNumber;

        public string NewContractNumber
        {
            get { return _newContractNumber; }
            set { _newContractNumber = value; }
        }
        private DateTime _awardDate;

        public DateTime AwardDate
        {
            get { return _awardDate; }
            set { _awardDate = value; }
        }
        private DateTime _effectiveDate;

        public DateTime EffectiveDate
        {
            get { return _effectiveDate; }
            set { _effectiveDate = value; }
        }
        private DateTime _expirationDate;

        public DateTime ExpirationDate
        {
            get { return _expirationDate; }
            set { _expirationDate = value; }
        }
        private int _optionYears;

        public int OptionYears
        {
            get { return _optionYears; }
            set { _optionYears = value; }
        }
        private int _newContractId;

        public int NewContractId
        {
            get { return _newContractId; }
            set { _newContractId = value; }
        }
        

        public CopyContractContent()
        {
        }

        // window parms provide most of the change parameters
        public CopyContractContent( CopyContractWindowParms parms )
        {
            _oldContractNumber = parms.ContractNumber;
        }

    }
}
