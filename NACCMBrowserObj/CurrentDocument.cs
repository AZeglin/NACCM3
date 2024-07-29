using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.NACCMBrowser.BrowserObj;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public delegate void CurrentDocumentUpdateEventHandler( CurrentDocumentUpdateEventArgs args );

    [Serializable]
    public class CurrentDocument
    {
        private int _scheduleNumber;       // also applies to offers
        private string _scheduleName = "";

        private EditStatuses _editStatus;
        private ArrayList _allowedAccessPoints = null;
        private ArrayList _ownersOperationalStatusIds = null;

        private DocumentTypes _documentType;
        private string _contractNumber;     // also applies to awarded offers
        private string _longContractNumber;
        private int _contractId;   // as of 10/2016 changing this to represent the contractId of the awarded contract off of an offer ( as well as the expected contractId of a current contract )

        public int ContractId
        {
            get { return _contractId; }
            set { _contractId = value; }
        }

        // offer
        private int _offerId = -1;
        private DateTime _dateOfferAssigned;
        private DateTime _dateOfferReceived;
        private bool _bIsOfferCompleted;
        private string _offerNumber = "";
        private int _proposalTypeId = -1;
        private string _extendsContractNumber = "";
        private int _extendsContractId = -1;

        private int _ownerId = -1;  // CO_ID

        public int OwnerId
        {
            get { return _ownerId; }
            set { _ownerId = value; }
        }

        private string _ownerName = "";  // CO FullName

        public string OwnerName
        {
            get { return _ownerName; }
            set { _ownerName = value; }
        }

        private bool _bIsCurrentUserTheOwner = false;

        // currently only used to help accurately dirty the personalized pane
        public bool IsCurrentUserTheOwner
        {
            get { return _bIsCurrentUserTheOwner; }
            // must call after backfill Load of currentdocument
            // currently not called for parent document         
            set { _bIsCurrentUserTheOwner = value; }
        }

        private static event CurrentDocumentUpdateEventHandler CurrentDocumentUpdateEvent;

        // only allows the handler to be set once for the life time of this object ( per session )
        public static void SetCurrentDocumentUpdateEvent( CurrentDocumentUpdateEventHandler theHandler )
        {
            if( CurrentDocumentUpdateEvent == null )
            {
                CurrentDocumentUpdateEvent = theHandler;
            }
        }

        public static void SetCurrentDocument( CurrentDocument newCurrentDocument, CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes currentDocumentUpdateType )
        {
            if( newCurrentDocument != null )
            {
                if( HttpContext.Current.Session[ "CurrentDocument" ] != null )
                {
                    CurrentDocument existingCurrentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];

                    // changing document type
                    if( existingCurrentDocument.DocumentType == DocumentTypes.Offer && currentDocumentUpdateType == CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.SelectedContractFromSearch ||
                        existingCurrentDocument.DocumentType != DocumentTypes.Offer && currentDocumentUpdateType == CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.SelectedOfferFromSearch )
                    {
                        HttpContext.Current.Session[ "CurrentDocument" ] = newCurrentDocument;
                        existingCurrentDocument = null;
                        HttpContext.Current.Session[ "CurrentDocumentIsChanging" ] = true.ToString();
                        // signal that the current document has changed
                        OnCurrentDocumentUpdateEvent( currentDocumentUpdateType );
                    }
                    // changing from one contract to another
                    else if( existingCurrentDocument.DocumentType != DocumentTypes.Offer && existingCurrentDocument.ContractNumber.CompareTo( newCurrentDocument.ContractNumber ) != 0 )
                    {
                        HttpContext.Current.Session[ "CurrentDocument" ] = newCurrentDocument;
                        existingCurrentDocument = null;
                        HttpContext.Current.Session[ "CurrentDocumentIsChanging" ] = true.ToString();
                        // signal that the current document has changed
                        OnCurrentDocumentUpdateEvent( currentDocumentUpdateType );
                    }
                    // changing from one offer to another
                    else if( existingCurrentDocument.DocumentType == DocumentTypes.Offer && existingCurrentDocument.OfferId != newCurrentDocument.OfferId )
                    {
                        HttpContext.Current.Session[ "CurrentDocument" ] = newCurrentDocument;
                        existingCurrentDocument = null;
                        HttpContext.Current.Session[ "CurrentDocumentIsChanging" ] = true.ToString();
                        // signal that the current document has changed
                        OnCurrentDocumentUpdateEvent( currentDocumentUpdateType );
                    }
                }
                else // first document 
                {
                    HttpContext.Current.Session[ "CurrentDocument" ] = newCurrentDocument;
                    HttpContext.Current.Session[ "CurrentDocumentIsChanging" ] = true.ToString();
                    // signal that the current document has changed
                    OnCurrentDocumentUpdateEvent( currentDocumentUpdateType );
                }
            }
            else // clearing current document ( create operation )
            {
                HttpContext.Current.Session[ "CurrentDocument" ] = null;
                
                HttpContext.Current.Session[ "CurrentDocumentIsChanging" ] = true.ToString();  // even if already null

                // signal that the current document has changed
                OnCurrentDocumentUpdateEvent( currentDocumentUpdateType );
            }
        }

        public static void OnCurrentDocumentUpdateEvent( CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes currentDocumentUpdateType )
        {
            if( CurrentDocumentUpdateEvent != null )
            {
                CurrentDocumentUpdateEventArgs args = new CurrentDocumentUpdateEventArgs( currentDocumentUpdateType );
                CurrentDocumentUpdateEvent( args );
            }
        }

        private string _vendorName = "";    // also applies to offers
        private string _description = "";
        private DateTime _awardDate;
        private DateTime _effectiveDate;
        private DateTime _expirationDate; // will be max value if null
        private DateTime _completionDate; // will be max value if null

        // these should be treated as read only as they are only for display of parent header info in the event of a BPA
        private string _vendorWebAddress = "";
        private string _vendorAddress1 = "";
        private string _vendorAddress2 = "";
        private string _vendorCity = "";
        private string _vendorState = "";
        private string _vendorCountryName = "";
        private int _vendorCountryId = -1;
        private string _vendorZip = "";


        private ActiveStatuses _activeStatus;
        private Divisions _division;

        private int _activeMedSurgItemCount = 0;
        private int _futureMedSurgItemCount = 0;
        private int _pricelessMedSurgItemCount = 0;

        private int _drugItemCount = 0;

        private int _primeVendorDrugItemCount = 0;
        private int _coveredDrugItemCount = 0;
        private int _withFCPDrugItemCount = 0;
        private int _ppvDrugItemCount = 0;

        private int _changeRequestCount = 0;

        public int ChangeRequestCount
        {
            get { return _changeRequestCount; }
            set { _changeRequestCount = value; }
        }

        // for BPA's
        private CurrentDocument _parentDocument = null;

        private DrugItemDB _drugItemDB = null;
        private ContractDB _contractDB = null;
        private OfferDB _offerDB = null;
        private ItemDB _itemDB = null;

        public ContractDB ContractDatabase
        {
            get { return _contractDB; }
            set { _contractDB = value; }
        }


        public CurrentDocument()
        {
            _allowedAccessPoints = new ArrayList();
            _ownersOperationalStatusIds = new ArrayList();
        }

        // note: ownerId will initialize to -1 when drilling down from an offer or offer list, 
        // this is followed by a lookup which backfills the value
        public CurrentDocument( int contractId, string contractNumber, int scheduleNumber, ContractDB contractDB, DrugItemDB drugItemDB, ItemDB itemDB )
        {
            ScheduleNumber = scheduleNumber;
            ContractNumber = contractNumber;
            _contractId = contractId;

            _contractDB = contractDB;
            _drugItemDB = drugItemDB;
            _itemDB = itemDB;

            GetItemCounts( scheduleNumber );

            if( _documentType == DocumentTypes.BPA || _documentType == DocumentTypes.FSSBPA )
                CheckParentDocument();

            _allowedAccessPoints = new ArrayList();
            _ownersOperationalStatusIds = new ArrayList();
        }

        public CurrentDocument( int contractId, string contractNumber, int scheduleNumber, ContractDB contractDB, DrugItemDB drugItemDB, ItemDB itemDB, bool bIsUserCreatingNewContract )
        {
            ScheduleNumber = scheduleNumber;
            ContractNumber = contractNumber;
            _contractId = contractId;

            _contractDB = contractDB;
            _drugItemDB = drugItemDB;
            _itemDB = itemDB;

            if( bIsUserCreatingNewContract == false )
            {
                GetItemCounts( scheduleNumber );
            }
            else // this would have been done as a side effect in GetItemCounts
            {
                _contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                _contractDB.MakeConnectionString();

                _drugItemDB.MakeConnectionString();
            }

            if( _documentType == DocumentTypes.BPA || _documentType == DocumentTypes.FSSBPA )
                CheckParentDocument();

            _allowedAccessPoints = new ArrayList();
            _ownersOperationalStatusIds = new ArrayList();
        }

        // offer
        public CurrentDocument( int offerId, int scheduleNumber, string vendorName, DateTime dateReceived, DateTime dateAssigned, int ownerId, string contractNumber, bool bIsOfferCompleted, OfferDB offerDB, ContractDB contractDB, DrugItemDB drugItemDB, ItemDB itemDB )
        {
            _offerId = offerId;
            ScheduleNumber = scheduleNumber;
            _vendorName = vendorName;
            _dateOfferReceived = dateReceived;
            _dateOfferAssigned = dateAssigned;
            _ownerId = ownerId;
            
            _bIsOfferCompleted = bIsOfferCompleted;
            if( bIsOfferCompleted == true )
                this.ActiveStatus = ActiveStatuses.Expired;
            else
                this.ActiveStatus = ActiveStatuses.Active;

            _contractNumber = contractNumber;          
            _contractDB = contractDB;
            _drugItemDB = drugItemDB;
            _offerDB = offerDB;
            _itemDB = itemDB;

            _offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _offerDB.MakeConnectionString();

            _contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _contractDB.MakeConnectionString();

            _itemDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _itemDB.MakeConnectionString();

            _drugItemDB.MakeConnectionString();

            _documentType = DocumentTypes.Offer;

            _allowedAccessPoints = new ArrayList();
            _ownersOperationalStatusIds = new ArrayList();
        }

        // offer release 2
        public CurrentDocument( int offerId, int scheduleNumber, string scheduleName, string vendorName, DateTime dateReceived, DateTime dateAssigned, int ownerId, string ownerName, string contractNumber, int contractId, bool bIsOfferCompleted, string offerNumber, int proposalTypeId, string extendsContractNumber, int extendsContractId, OfferDB offerDB, ContractDB contractDB, DrugItemDB drugItemDB, ItemDB itemDB )
        {
            _offerId = offerId;
            ScheduleNumber = scheduleNumber;
            _scheduleName = scheduleName;
            _vendorName = vendorName;
            _dateOfferReceived = dateReceived;
            _dateOfferAssigned = dateAssigned;
            _ownerId = ownerId;
            _ownerName = ownerName;

            _bIsOfferCompleted = bIsOfferCompleted;
            if( bIsOfferCompleted == true )
                this.ActiveStatus = ActiveStatuses.Expired;
            else
                this.ActiveStatus = ActiveStatuses.Active;

            _offerNumber = offerNumber;
            _proposalTypeId = proposalTypeId;

            _contractNumber = contractNumber;
            _contractId = contractId;
            _extendsContractNumber = extendsContractNumber;
            _extendsContractId = extendsContractId;
            _contractDB = contractDB;
            _drugItemDB = drugItemDB;
            _offerDB = offerDB;
            _itemDB = itemDB;

            _offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _offerDB.MakeConnectionString();

            _contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _contractDB.MakeConnectionString();

            _itemDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _itemDB.MakeConnectionString();

            _drugItemDB.MakeConnectionString();

            _documentType = DocumentTypes.Offer;

            _allowedAccessPoints = new ArrayList();
            _ownersOperationalStatusIds = new ArrayList();
        }

        // offer release 2 - this version provides less initial info and requires a lookup to backfill
        public CurrentDocument( int offerId, int scheduleNumber, OfferDB offerDB, ContractDB contractDB, DrugItemDB drugItemDB, ItemDB itemDB )
        {
            _offerId = offerId;
            ScheduleNumber = scheduleNumber;
           
            _contractDB = contractDB;
            _drugItemDB = drugItemDB;
            _offerDB = offerDB;
            _itemDB = itemDB;

            _offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _offerDB.MakeConnectionString();

            _contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _contractDB.MakeConnectionString();

            _itemDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _itemDB.MakeConnectionString();

            _drugItemDB.MakeConnectionString();

            _documentType = DocumentTypes.Offer;

            _allowedAccessPoints = new ArrayList();
            _ownersOperationalStatusIds = new ArrayList();
        }

        public int OfferId
        {
            get { return _offerId; }
            set { _offerId = value; }
        }

        public DateTime DateOfferAssigned
        {
            get { return _dateOfferAssigned; }
            set { _dateOfferAssigned = value; }
        }

        public DateTime DateOfferReceived
        {
            get { return _dateOfferReceived; }
            set { _dateOfferReceived = value; }
        }

        public bool IsOfferCompleted
        {
            get { return _bIsOfferCompleted; }
            set 
            {
                // added 3/17/2015 for R2 development
                if( _bIsOfferCompleted != value )
                {
                    if( value == true )
                        ActiveStatus = ActiveStatuses.Expired;
                    else
                        ActiveStatus = ActiveStatuses.Active;
                }

                _bIsOfferCompleted = value; 
            }
        }

        public string OfferNumber
        {
            get { return _offerNumber; }
            set { _offerNumber = value; }
        }

        public int ProposalTypeId
        {
            get { return _proposalTypeId; }
            set { _proposalTypeId = value; }
        }

        public string ExtendsContractNumber
        {
            get { return _extendsContractNumber; }
            set { _extendsContractNumber = value; }
        }

        public int ExtendsContractId
        {
            get { return _extendsContractId; }
            set { _extendsContractId = value; }
        }

        public int ScheduleNumber
        {
            get
            { 
                return( _scheduleNumber ); 
            }
            
            set
            { 
                _scheduleNumber = value;
                _division = GetDivisionFromSchedule( value );

                // offer documenttype does not change when schedule is set ( on this convenience update )
                if( _documentType != DocumentTypes.Offer )
                {

                    if( IsBPA( value ) == true )
                    {
                        if( IsFSSBPA( value ) == true )
                            _documentType = DocumentTypes.FSSBPA;
                        else
                            _documentType = DocumentTypes.BPA;
                    }
                    else
                    {
                        if( _division == Divisions.National || _division == Divisions.SAC )
                            _documentType = DocumentTypes.National;   // SAC treated like National
                        else
                            _documentType = DocumentTypes.FSS;  // "Other" divisions get treated like FSS
                    }
                }
            }
        }

        public string ScheduleName
        {
            get { return _scheduleName; }
            set { _scheduleName = value; }
        }

        public DocumentTypes DocumentType
        {
            get
            { 
                return( _documentType ); 
            }
            set
            { 
                _documentType = value; 
            }
        }

        public string GetDocumentTypeString( DocumentTypes documentType )
        {
            string documentTypeString = "F"; // default to FSS

            if( documentType == DocumentTypes.FSSBPA )
            {
                documentTypeString = "P";
            }
            else if( documentType == DocumentTypes.BPA )
            {
                documentTypeString = "B";
            }
            else if( documentType == DocumentTypes.National )
            {
                documentTypeString = "N";
            }
            else if( documentType == DocumentTypes.Offer )
            {
                documentTypeString = "O";
            }
            else if( documentType == DocumentTypes.FSS )
            {
                if( IsService( _scheduleNumber ) == true )
                {
                    documentTypeString = "S";
                }
                else
                {
                    documentTypeString = "F";
                }
            }

            return ( documentTypeString );
        }

        public string ContractNumber
        {
            get
            { 
                return( _contractNumber );
            }
            set
            {
                _contractNumber = value;
                //if( value.ToUpper().StartsWith( "V" ) == true || value.ToUpper().Contains( "FSSBPA" ) == true )
                //{
                //    _longContractNumber = value;
                //}
                //else
                //{
                //    _longContractNumber = String.Format( "V797P-{0}", value );
                //}
                _longContractNumber = value; // made them the same on 9/7/2010
            }
        }

        public string LongContractNumber
        {
            get
            { 
                return _longContractNumber; 
            }
            set
            { 
                _longContractNumber = value; 
            }
        }
 
        public ActiveStatuses ActiveStatus
        {
            get
            { 
                return( _activeStatus ); 
            }
            set
            { 
                _activeStatus = value; 
            }
        }

        public EditStatuses EditStatus
        {
            get
            {
                return ( _editStatus );
            }
            set
            { 
                _editStatus = value; 
            }
        }

        public ArrayList AllowedAccessPoints
        {
            get
            {
                return ( _allowedAccessPoints );
            }
        }

        // not used, it is included in BrowserSecurity2 object
        //public ArrayList OwnersOperationalStatusIds
        //{
        //    get { return _ownersOperationalStatusIds; }
        //    set { _ownersOperationalStatusIds = value; }
        //}

        public bool IsAccessAllowed( BrowserSecurity2.AccessPoints accessPoint )
        {
            bool bAllowed = false;

            for( int i = 0; i < _allowedAccessPoints.Count; i++ )
            {
                BrowserSecurity2.AccessPoints allowedAccessPoint = ( BrowserSecurity2.AccessPoints )_allowedAccessPoints[ i ];
                if( accessPoint == allowedAccessPoint )
                {
                    bAllowed = true;
                    break;
                }
            }

            return ( bAllowed );
        }

        public Divisions Division
        {
            get
            { 
                return _division; 
            }
            set
            { 
                _division = value; 
            }
        }

        public int ActiveMedSurgItemCount
        {
            get { return _activeMedSurgItemCount; }
            set { _activeMedSurgItemCount = value; }
        }

        public int FutureMedSurgItemCount
        {
            get { return _futureMedSurgItemCount; }
            set { _futureMedSurgItemCount = value; }
        }

        public int PricelessMedSurgItemCount
        {
            get { return _pricelessMedSurgItemCount; }
            set { _pricelessMedSurgItemCount = value; }
        }

        public int DrugItemCount
        {
            get { return _drugItemCount; }
            set { _drugItemCount = value; }
        }


        public int PrimeVendorDrugItemCount
        {
            get { return _primeVendorDrugItemCount; }
            set { _primeVendorDrugItemCount = value; }
        }

        public int CoveredDrugItemCount
        {
            get { return _coveredDrugItemCount; }
            set { _coveredDrugItemCount = value; }
        }

        public int WithFCPDrugItemCount
        {
            get { return _withFCPDrugItemCount; }
            set { _withFCPDrugItemCount = value; }
        }

        public int PPVDrugItemCount
        {
            get { return _ppvDrugItemCount; }
            set { _ppvDrugItemCount = value; }
        }

        public enum DocumentTypes
        {
            FSS,  // other divisions, besides FSS, National or SAC also receive documentType = FSS
            BPA,
            National,   // for now, added only to use with pharmaceutical item screen + SAC receives documentType = National
            FSSBPA,      // for now, added only to use with pharmaceutical item screen           
            Offer
        }

        public enum EditStatuses
        {
            Denied,
            CanView,
            CanEdit,  // anything
            CanEditFiscal, // these are exclusive - these edit statuses are no longer used
            CanEditSales,
            CanEditSBA,
            CanEditPBM
        }

        public enum ActiveStatuses
        {
            Active,
            Expired
        }

        public enum Divisions
        {
            Other=0,
            FSS=1,
            National=2,
            SAC=6
        }

        public bool IsBOA( int scheduleNumber )
        {
            bool bBOA = false;

            // 43, 47  are pharm BOAs  55 is SAC BOA
            if( scheduleNumber == 14 || scheduleNumber == 43 || scheduleNumber == 47 || scheduleNumber == 55 )
                bBOA = true;

            return ( bBOA );
        }

        public bool IsBPA( int scheduleNumber )
        {
            return ( CurrentDocument.IsBPA( scheduleNumber, 0 ) );
        }

        // nothing parm added so compiler can distinguish between static and non-static overload
        public static bool IsBPA( int scheduleNumber, int nothing )
        {
            bool bBPA = false;

            // omitting 41 and 58 because they have non-VA parents, they cannot reference a NACCM internal parent
            if( scheduleNumber == 15 || scheduleNumber == 39 || scheduleNumber == 44 || scheduleNumber == 48 || scheduleNumber == 52 || scheduleNumber == 53 )
                bBPA = true;

            return ( bBPA );
        }

        public bool IsFSSBPA( int scheduleNumber )
        {
            bool bBPA = false;

            if( scheduleNumber == 48 )
                bBPA = true;

            return ( bBPA );
        }

        public bool IsService( int scheduleNumber )
        {
            return ( CurrentDocument.IsService( scheduleNumber, 0 ) );
        }

        public static bool IsService( int scheduleNumber, int nothing )
        {
            bool bService = false;

            if( scheduleNumber == 36 )
                bService = true;

            return ( bService );
        }

        public bool CanHaveInsurance( int scheduleNumber )
        {
            bool bInsurance = false;

            if( scheduleNumber == 10 || scheduleNumber == 36 || scheduleNumber == 42 )  // cost per test, services, and laboratory testing
                bInsurance = true;

            return ( bInsurance );
        }

        public bool CanHaveTradeAgreement( int scheduleNumber )
        {
            bool bTradeAgreement = false;

            if( scheduleNumber != 42 && scheduleNumber != 36 )  // lab testing, and services
                bTradeAgreement = true;

            return ( bTradeAgreement );
        }

        public bool CanHaveRebates( int scheduleNumber )
        {
            return ( CurrentDocument.CanHaveRebates( scheduleNumber, 0 ) );
        }

        public static bool CanHaveRebates( int scheduleNumber, int nothing )
        {
            bool bRebates = true;

            Divisions division = GetDivisionFromSchedule( scheduleNumber );
            
            // disable rebate for non-fss or for fss service schedule
            if( division != Divisions.FSS || IsService( scheduleNumber, 0 ) == true )
            {
                bRebates = false;
            }

            return ( bRebates );
        }

        public bool CanHaveSINs( int scheduleNumber )
        {
            bool bSINs = true;

            Divisions division = GetDivisionFromSchedule( scheduleNumber );
            
            // disable for all National and all SAC 
            if( division == Divisions.National || division == Divisions.SAC )
            {
                bSINs = false;
            }

            return ( bSINs );
        }

        public bool IsPharmaceutical( int scheduleNumber )
        {
            bool bIsPharmaceutical = false;

            if ( scheduleNumber == 1 || scheduleNumber == 8 || scheduleNumber == 18 || // retired 9/25/2013: 28, 29, 43, 50
                scheduleNumber == 30 || scheduleNumber == 31 || scheduleNumber == 32 || scheduleNumber == 37 ||
                scheduleNumber == 39 || scheduleNumber == 47 || scheduleNumber == 48 || scheduleNumber == 49 || scheduleNumber == 52 ) // added CMOP blue 1/13/2010 // Invitro added 6/9/2011 // addded specialty BPA's 9/24/2013 // added subsistence 12/21/2016 to support dietary which is depricated
                bIsPharmaceutical = true;

            return ( bIsPharmaceutical );
        }

        public bool CanHaveStandardizedItems( int scheduleNumber )
        {
            bool bCanHaveStandardizedItems = false;

            if( scheduleNumber == 14 || scheduleNumber == 15 || scheduleNumber == 17 || scheduleNumber == 19 || scheduleNumber == 38 || scheduleNumber == 41 || scheduleNumber == 53 || scheduleNumber == 54 || scheduleNumber == 55 || scheduleNumber == 57 || scheduleNumber == 58 || scheduleNumber == 63 )
                bCanHaveStandardizedItems = true;

            return ( bCanHaveStandardizedItems );
        }

        private static Divisions GetDivisionFromSchedule( int scheduleNumber )
        {
            if( ( scheduleNumber >= 1 && scheduleNumber <= 10 ) || scheduleNumber == 20
                || scheduleNumber == 34 || scheduleNumber == 36 || scheduleNumber == 42 || scheduleNumber == 48 )
                return ( Divisions.FSS );
            else if( scheduleNumber == 53 || scheduleNumber == 54 || scheduleNumber == 55 || scheduleNumber == 57 || scheduleNumber == 58 )
                return ( Divisions.SAC );
            // 44,45 front office, 46 = DALC, 
            else if( scheduleNumber == 44 || scheduleNumber == 45 || scheduleNumber == 46 )
                return ( Divisions.Other );
            else
                return ( Divisions.National );
        }

        public static int GetDivisionIdFromDivision( Divisions division )
        {
            int divisionId = -1;

            if( division == Divisions.FSS )
                divisionId = 1;
            else if( division == Divisions.National )
                divisionId = 2;
            else if( division == Divisions.SAC )
                divisionId = 6;
            
            return( divisionId );                                 
        }

        private void SetActiveStatus()
        {
            _activeStatus = ActiveStatuses.Active;
            if( _completionDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) != 0 )
            {
                if( _completionDate.CompareTo( DateTime.Now ) < 0 )
                    _activeStatus = ActiveStatuses.Expired;
            }
            if( _expirationDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) != 0 )
            {
                if( _expirationDate.Date.CompareTo( DateTime.Now.Date ) < 0 )
                    _activeStatus = ActiveStatuses.Expired;
            }
        }

        // called to refresh the screen's item counts after an edit
        public void UpdateItemCounts()
        {
            GetItemCounts( _scheduleNumber );
        }

        protected void GetItemCounts()
        {
            this.GetItemCounts( _scheduleNumber );
        }

        private void GetItemCounts( int scheduleNumber )
        {
            int activeMedSurgItemCount = 0;
            int futureMedSurgItemCount = 0;
            int pricelessMedSurgItemCount = 0;
            int drugItemCount = 0;
            int coveredDrugItemCount = 0;
            int withFCPDrugItemCount = 0;
            int ppvDrugItemCount = 0;

            bool bSuccess = false;

            _itemDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _itemDB.MakeConnectionString();

            if( Config.ItemVersion.CompareTo( "I2" ) == 0 )
            {
                bSuccess = _itemDB.GetMedSurgItemCount2( _contractId, _contractNumber, ref activeMedSurgItemCount, ref futureMedSurgItemCount, ref pricelessMedSurgItemCount );

            }
            else
            {
                if( IsBPA( scheduleNumber ) == true )
                {
                    bSuccess = _itemDB.GetMedSurgBPAItemCount( _contractId, _contractNumber, ref activeMedSurgItemCount, ref futureMedSurgItemCount );
                }
                else
                {
                    pricelessMedSurgItemCount = 0; 
                    bSuccess = _itemDB.GetMedSurgItemCount( _contractId, _contractNumber, ref activeMedSurgItemCount, ref futureMedSurgItemCount );
                }
            }
            if( bSuccess != true )
            {
                //if( MsgBox != null )
                //    MsgBox.Alert( _contractDB.ErrorMessage );
            }
            else
            {
                _activeMedSurgItemCount = activeMedSurgItemCount;
                _futureMedSurgItemCount = futureMedSurgItemCount;
                _pricelessMedSurgItemCount = pricelessMedSurgItemCount;

                if( IsPharmaceutical( scheduleNumber ) == true )
                {
                    _drugItemDB.MakeConnectionString();
                    bSuccess = _drugItemDB.GetDrugItemCount( _contractNumber, ref drugItemCount );
                    if( bSuccess != true )
                    {
                        //if( MsgBox != null )
                        //    MsgBox.Alert( _drugItemDB.ErrorMessage );
                    }
                    else
                    {
                        _drugItemCount = drugItemCount;
                    }

                    // gets covered, FCP and PPV counts
                    bSuccess = _drugItemDB.GetCoveredFCPCountForDrugContract( _contractNumber, ref coveredDrugItemCount, ref withFCPDrugItemCount, ref ppvDrugItemCount );
                    if( bSuccess != true )
                    {
                        //if( MsgBox != null )
                        //    MsgBox.Alert( _drugItemDB.ErrorMessage );
                    }
                    else
                    {
                        _coveredDrugItemCount = coveredDrugItemCount;
                        _withFCPDrugItemCount = withFCPDrugItemCount;
                        _ppvDrugItemCount = ppvDrugItemCount;
                    }
                }
            }
        }

        public DateTime ExpirationDate
        {
            get { return _expirationDate; }
            set 
            { 
                _expirationDate = value;
                SetActiveStatus();  // added 1/7/2015
            }
        }

        public DateTime AwardDate
        {
            get { return _awardDate; }
            set 
            { 
                _awardDate = value;
                SetActiveStatus();  // added 1/7/2015
            }
        }

        public DateTime CompletionDate
        {
            get { return _completionDate; }
            set 
            { 
                _completionDate = value;
                SetActiveStatus();  // added 1/7/2015
            }
        }

        public DateTime EffectiveDate
        {
            get { return _effectiveDate; }
            set 
            { 
                _effectiveDate = value;
                SetActiveStatus();  // added 1/7/2015
            }
        } 

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public string VendorName
        {
            get { return _vendorName; }
            set { _vendorName = value; }
        }

        public string VendorWebAddress
        {
            get { return _vendorWebAddress; }
            set { _vendorWebAddress = value; }
        }

        public string VendorAddress1
        {
            get { return _vendorAddress1; }
            set { _vendorAddress1 = value; }
        }

        public string VendorAddress2
        {
            get { return _vendorAddress2; }
            set { _vendorAddress2 = value; }
        }

        public string VendorCity
        {
            get { return _vendorCity; }
            set { _vendorCity = value; }
        }

        public string VendorState
        {
            get { return _vendorState; }
            set { _vendorState = value; }
        }

        public string VendorCountryName
        {
            get
            {
                return _vendorCountryName;
            }
            set
            {
                _vendorCountryName = value;
            }
        }

        public int VendorCountryId
        {
            get
            {
                return _vendorCountryId;
            }
            set
            {
                _vendorCountryId = value;
            }
        }

        public string VendorZip
        {
            get { return _vendorZip; }
            set { _vendorZip = value; }
        }

        public bool HasParent
        {
            get
            {
                if( _parentDocument != null )
                    return ( true );
                else
                    return ( false );
            }
        }

        public CurrentDocument ParentDocument
        {
            get
            {
                return ( _parentDocument );
            }
        }

        private void CheckParentDocument()
        {
            int parentScheduleNumber = -1;
            string parentContractNumber = "";
            int parentContractId = -1;
            int parentOwnerId = -1;

            bool bSuccess = false;

            _contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _contractDB.MakeConnectionString();

            if( Config.ItemVersion.CompareTo( "I2" ) == 0 )
            {
                bSuccess = _contractDB.GetParentContractInfo2( _contractNumber, ref parentContractId, ref parentContractNumber, ref parentScheduleNumber, ref parentOwnerId );
            }
            else
            {
            bSuccess = _contractDB.GetParentContractInfo( _contractNumber, ref parentContractNumber, ref parentScheduleNumber, ref parentOwnerId );
            }


            if( bSuccess != true )
            {
                _parentDocument = null;
                //if( MsgBox != null )
                //    MsgBox.Alert( _contractDB.ErrorMessage );
            }
            else
            {
                CurrentDocument parentDocument = new CurrentDocument( parentContractId, parentContractNumber, parentScheduleNumber, _contractDB, _drugItemDB, _itemDB );
                parentDocument.OwnerId = parentOwnerId;
                _parentDocument = parentDocument;
                // backfill a full parent
                _parentDocument.LookupCurrentDocument();
            }
        }

        // presumes getting called after contract record has been updated
        public void UpdateParentDocument()
        {
            int parentScheduleNumber = -1;
            string parentContractNumber = "";
            int parentContractId = -1;
            int parentOwnerId = -1;

            bool bSuccess = false;

            _contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
            _contractDB.MakeConnectionString();

            if( Config.ItemVersion.CompareTo( "I2" ) == 0 )
            {
                bSuccess = _contractDB.GetParentContractInfo2( _contractNumber, ref parentContractId, ref parentContractNumber, ref parentScheduleNumber, ref parentOwnerId );
            }
            else
            {
            bSuccess = _contractDB.GetParentContractInfo( _contractNumber, ref parentContractNumber, ref parentScheduleNumber, ref parentOwnerId );
            }

            if( bSuccess != true )
            {
                _parentDocument = null;
                //if( MsgBox != null )
                //    MsgBox.Alert( _contractDB.ErrorMessage );
            }
            else
            {
                CurrentDocument parentDocument = new CurrentDocument( parentContractId, parentContractNumber, parentScheduleNumber, _contractDB, _drugItemDB, _itemDB );
                parentDocument.OwnerId = parentOwnerId;
                _parentDocument = parentDocument;
                // backfill a full parent
                _parentDocument.LookupCurrentDocument();
            }

            // handle FSS Pharmaceutical BPAs
            if( bSuccess == true && IsPharmaceutical( ScheduleNumber ) == true )
            {
                bSuccess = UpdatePharmaceuticalParentDocument( ContractNumber, parentContractNumber );
               // if( bSuccess == false )
                //if( MsgBox != null )
                //    MsgBox.Alert( _drugItemDB.ErrorMessage );

            }
        }

        private bool UpdatePharmaceuticalParentDocument( string contractNumber, string parentContractNumber )
        {
            bool bSuccess = false;
            bSuccess = _drugItemDB.UpdateParentDrugItemContract( contractNumber, parentContractNumber );
            return ( bSuccess );
        }

        public bool LookupCurrentDocument()
        {
            int contractId = -1;
            int scheduleNumber = -1;
            string scheduleName = "";
            int ownerId = -1;
            string ownerName = "";
            string vendorName = "";
            string description = "";
            DateTime awardDate = DateTime.MinValue;   
            DateTime effectiveDate = DateTime.MinValue;
            DateTime expirationDate = DateTime.MinValue;
            DateTime completionDate = DateTime.MinValue;

            string vendorWebAddress = "";
            string vendorAddress1 = "";
            string vendorAddress2 = ""; 
            string vendorCity = "";
            string vendorState = "";
            string vendorCountryName = "";
            int vendorCountryId = -1;
            string vendorZip = "";

            bool bSuccess = false;

            if( _documentType == DocumentTypes.Offer )
            {
                _offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                _offerDB.MakeConnectionString();

                DataSet dsOneOfferRow = null;
                bSuccess = _offerDB.GetOfferInfo2( ref dsOneOfferRow, _offerId );     // calling new version of SP to retrieve extra 2 ids  5/2017

                if( bSuccess == true )
                {
                    if( _offerDB.RowsReturned > 0 )
                    {
                        DataTable dtOneOfferRow = dsOneOfferRow.Tables[ "OneOfferRowTable" ];
                        DataRow row = dtOneOfferRow.Rows[ 0 ];
                        if( row.GetType() != typeof( DBNull ) )
                        {
                            if( row.IsNull( "AssignedCOID" ) == false )
                                _ownerId = ( int )row[ "AssignedCOID" ];
                           
                            if( row.IsNull( "VendorName" ) == false )
                                _vendorName = ( string )row[ "VendorName" ];
                            if( row.IsNull( "OwnerName" ) == false )
                                _ownerName = ( string )row[ "OwnerName" ];
                            if( row.IsNull( "ScheduleName" ) == false )
                                _scheduleName = ( string )row[ "ScheduleName" ];
                            if( row.IsNull( "OfferNumber" ) == false )
                                _offerNumber = ( string )row[ "OfferNumber" ];                                                                                  

                            if( row.IsNull( "IsOfferComplete" ) == false )
                                _bIsOfferCompleted = bool.Parse( row[ "IsOfferComplete" ].ToString() );

                            if( _bIsOfferCompleted == true )
                                this.ActiveStatus = ActiveStatuses.Expired;
                            else
                                this.ActiveStatus = ActiveStatuses.Active;
                            
                            if( row.IsNull( "ProposalTypeId" ) == false )
                                _proposalTypeId = ( int )row[ "ProposalTypeId" ];
                            if( row.IsNull( "DateOfferReceived" ) == false )
                                _dateOfferReceived = DateTime.Parse( row[ "DateOfferReceived" ].ToString() );
                            if( row.IsNull( "DateOfferAssigned" ) == false )
                                _dateOfferAssigned = DateTime.Parse( row[ "DateOfferAssigned" ].ToString() );
                            if( row.IsNull( "ContractId" ) == false )
                                _contractId = ( int )row[ "ContractId" ];  // added 10/12/2016 to support offer ability to pass contractId of awarded contract for item selection
                            if( row.IsNull( "ContractNumber" ) == false )
                                _contractNumber = ( string )row[ "ContractNumber" ];
                            if( row.IsNull( "ExtendsContractNumber" ) == false )
                                _extendsContractNumber = ( string )row[ "ExtendsContractNumber" ];
                            if( row.IsNull( "ExtendsContractId" ) == false )
                                _extendsContractId = ( int )row[ "ExtendsContractId" ];  // added 10/12/2016 to support offer ability to pass contractId of awarded contract for item selection
                        }
                    }
                }
            }
            else
            {
                _contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                _contractDB.MakeConnectionString();

                bSuccess = _contractDB.GetContractInfo( _contractNumber, ref contractId, ref scheduleNumber, ref ownerId, ref vendorName, ref description, ref awardDate, ref expirationDate, ref completionDate, ref effectiveDate,
                    ref scheduleName, ref ownerName, ref vendorWebAddress, ref vendorAddress1, ref vendorAddress2, ref vendorCity, ref vendorState, ref vendorCountryName, ref vendorCountryId, ref vendorZip );

                if( bSuccess == true )
                {
                    ContractId = contractId;    // this is only used for logging user preferences
                    ScheduleNumber = scheduleNumber;
                    _scheduleName = scheduleName;
                    _ownerId = ownerId;
                    _ownerName = ownerName;
                    _vendorName = vendorName;
                    _description = description;
                    _awardDate = awardDate;
                    _effectiveDate = effectiveDate;
                    _expirationDate = expirationDate;
                    _completionDate = completionDate;
                    // the below are only displayed if this is the parent document, they are read-only and do not require update ( since the parent cannot be edited )
                    _vendorWebAddress = vendorWebAddress;
                    _vendorAddress1 = vendorAddress1;
                    _vendorAddress2 = vendorAddress2; 
                    _vendorCity = vendorCity;
                    _vendorState = vendorState;
                    _vendorCountryName = vendorCountryName;
                    _vendorCountryId = vendorCountryId;
                    _vendorZip = vendorZip;

                    SetActiveStatus();
                }
            }
            return ( bSuccess );
        }

        public string GetLastContractDBError()
        {
            string lastError = "";

            if( _contractDB != null )
            {
                lastError = _contractDB.ErrorMessage;
            }

            return( lastError );
        }

 
    }
}
