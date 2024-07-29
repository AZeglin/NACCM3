using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public class CurrentDocumentUpdateEventArgs : EventArgs
    {
        private CurrentDocumentUpdateTypes _currentDocumentUpdateType = CurrentDocumentUpdateTypes.Undefined;

        public CurrentDocumentUpdateTypes CurrentDocumentUpdateType
        {
            get { return _currentDocumentUpdateType; }
            set { _currentDocumentUpdateType = value; }
        }

        public CurrentDocumentUpdateEventArgs( CurrentDocumentUpdateTypes currentDocumentUpdateType )
        {
            _currentDocumentUpdateType = currentDocumentUpdateType;
        }

        public enum CurrentDocumentUpdateTypes
        {
            Undefined,
            SelectedContractFromSearch,
            SelectedOfferFromSearch,
            CreateContract,
            CreateOffer
        }
    }
}
