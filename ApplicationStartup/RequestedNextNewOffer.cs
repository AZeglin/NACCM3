using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VA.NAC.CM.ApplicationStartup
{
    // used to contain the parameters captured when an offer creation request is being created
    [Serializable]
    public class RequestedNextNewOffer : RequestedNextDocument
    {
        public RequestedNextNewOffer()
            : base( NextDocumentTypes.NewOffer )
        {

        }
    }
}