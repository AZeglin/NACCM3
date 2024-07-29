using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VA.NAC.CM.ApplicationStartup
{
    // used to contain the parameters captured when a contract creation request is being created
    [Serializable]
    public class RequestedNextNewContract : RequestedNextDocument
    {

        private int _offerId = -1;

        public int OfferId
        {
          get { return _offerId; }
          set { _offerId = value; }
        }

        private int _scheduleNumber = -1;

        public int ScheduleNumber
        {
            get { return _scheduleNumber; }
            set { _scheduleNumber = value; }
        }

        public RequestedNextNewContract()
            : base( NextDocumentTypes.NewContract )
        {

        }
    }
}