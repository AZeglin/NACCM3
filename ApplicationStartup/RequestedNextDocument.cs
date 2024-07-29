using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VA.NAC.CM.ApplicationStartup
{
    [Serializable]
    public class RequestedNextDocument
    {
        private NextDocumentTypes _nextDocumentType = NextDocumentTypes.Undefined;

        public NextDocumentTypes NextDocumentType
        {
          get { return _nextDocumentType; }
          set { _nextDocumentType = value; }
        }

        public enum DocumentChangeRequestSources
        {
            ContractSearchGridView,
            OfferSearchGridView,
            UserRecentDocumentsGridView,
            PersonalizedNotificationGridView,
            PersonalizedContractListGridView,
            CreateContract,
            CreateOffer,
            AssociatedBPAContractsGridView,
            ContractGeneralParentContractFormView,
            OfferAwardFormViewViewContract,
            OfferAwardFormViewCreateContract,
            AssociatedContractsGridView,        // SBA
            NewOfferAfterSave,
            NewContractAfterSave,
            Undefined
        }

        private DocumentChangeRequestSources _sourceOfDocumentChangeRequest = DocumentChangeRequestSources.Undefined;

        public DocumentChangeRequestSources SourceOfDocumentChangeRequest
        {
            get { return _sourceOfDocumentChangeRequest; }
            set { _sourceOfDocumentChangeRequest = value; }
        }


        public enum NextDocumentTypes
        {
            Contract,
            Offer,
            ShortOffer, // less initial info
            NewContract,
            NewOffer,
            Undefined
        }

        public RequestedNextDocument( NextDocumentTypes nextDocumentType )
        {
            _nextDocumentType = nextDocumentType;
        }

    }
}