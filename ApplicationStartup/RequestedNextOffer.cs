using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VA.NAC.CM.ApplicationStartup
{
    // used to contain the parameters captured when a document is selected from search results
    [Serializable]
    public class RequestedNextOffer : RequestedNextDocument
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
        private string _scheduleName = "";

        public string ScheduleName
        {
            get { return _scheduleName; }
            set { _scheduleName = value; }
        }
        private string _vendorName = "";

        public string VendorName
        {
            get { return _vendorName; }
            set { _vendorName = value; }
        }
        private DateTime _dateReceived;

        public DateTime DateReceived
        {
            get { return _dateReceived; }
            set { _dateReceived = value; }
        }
        private DateTime _dateAssigned;

        public DateTime DateAssigned
        {
            get { return _dateAssigned; }
            set { _dateAssigned = value; }
        }
        private int _ownerId = -1;

        public int OwnerId
        {
            get { return _ownerId; }
            set { _ownerId = value; }
        }

        private string contractingOfficerName = "";

        public string ContractingOfficerName
        {
            get { return contractingOfficerName; }
            set { contractingOfficerName = value; }
        }

        private string _contractNumber = "";

        public string ContractNumber
        {
            get { return _contractNumber; }
            set { _contractNumber = value; }
        }

        private int _contractId = -1;

        public int ContractId
        {
            get { return _contractId; }
            set { _contractId = value; }
        }

        private bool _bIsOfferCompleted = false;

        public bool IsOfferCompleted
        {
            get { return _bIsOfferCompleted; }
            set { _bIsOfferCompleted = value; }
        }

        private string _offerNumber = "";

        public string OfferNumber
        {
            get { return _offerNumber; }
            set { _offerNumber = value; }
        }

        private int _proposalTypeId = -1;

        public int ProposalTypeId
        {
            get { return _proposalTypeId; }
            set { _proposalTypeId = value; }
        }

        private string _extendsContractNumber = "";

        public string ExtendsContractNumber
        {
            get { return _extendsContractNumber; }
            set { _extendsContractNumber = value; }
        }

        private int _extendsContractId = -1;

        public int ExtendsContractId
        {
            get { return _extendsContractId; }
            set { _extendsContractId = value; }
        }

        public RequestedNextOffer()
            : base( NextDocumentTypes.Offer )
        {

        }

        public RequestedNextOffer( bool bIsShort )
            : base( NextDocumentTypes.ShortOffer )
        {

        }
    }
}