using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VA.NAC.CM.ApplicationStartup
{
    // used to contain the parameters captured when a document is selected from search results
    [Serializable]
    public class RequestedNextContract : RequestedNextDocument
    {
        private int _contractId = -1;

        public int ContractId
        {
            get { return _contractId; }
            set { _contractId = value; }
        }

        private string _contractNumber = "";

        public string ContractNumber
        {
            get { return _contractNumber; }
            set { _contractNumber = value; }
        }

        private int _scheduleNumber = -1;

        public int ScheduleNumber
        {
            get { return _scheduleNumber; }
            set { _scheduleNumber = value; }
        }

        public RequestedNextContract()
            : base( NextDocumentTypes.Contract )
        {

        }
    }
}