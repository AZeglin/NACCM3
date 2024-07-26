using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using AjaxControlToolkit;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.ReportManager;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using ListView = VA.NAC.NACCMBrowser.BrowserObj.ListView;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
 
    public class BaseDocumentEditorPage : System.Web.UI.Page
    {

        private DocumentEditorTypes _documentEditorType = DocumentEditorTypes.Undefined;

        public DocumentEditorTypes DocumentEditorType
        {
            get { return _documentEditorType; }
            set { _documentEditorType = value; }
        }

        public BaseDocumentEditorPage()
        {
        }

        public BaseDocumentEditorPage( DocumentEditorTypes documentEditorType )
        {
            _documentEditorType = documentEditorType;
        }

        public enum DocumentEditorTypes
        {
            Contract,
            Offer,
            NewContract,
            NewContractFromOffer,
            NewOffer,
            Undefined
        }

        private CurrentDocument _currentDocument = null;

        private string _errorMessage = "";

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
 
        private DataRelay _dataRelay = null;

        public DataRelay DataRelay
        {
            get { return _dataRelay; }
            set { _dataRelay = value; }
        }

        private OfferDataRelay _offerDataRelay = null;

        public OfferDataRelay OfferDataRelay
        {
            get { return _offerDataRelay; }
            set { _offerDataRelay = value; }
        }

        private ValidationErrorManager _validationErrorManager = null;

        public ValidationErrorManager ValidationErrorManager
        {
            get
            {
                return _validationErrorManager;
            }

            set
            {
                _validationErrorManager = value;
            }
        }


        public bool CurrentDocumentIsChanging
        {
            get 
            {
                if( Session[ "CurrentDocumentIsChanging" ] != null )
                {
                    return ( bool.Parse( Session[ "CurrentDocumentIsChanging" ].ToString() ) );
                }
                else
                {
                    return ( false );
                }
            }
            set 
            {
                Session[ "CurrentDocumentIsChanging" ] = value; 
            }
        }


 
        private DocumentDataSource _scheduleSINDataSource = null;

        public DocumentDataSource ScheduleSINDataSource
        {
            get { return _scheduleSINDataSource; }
            set { _scheduleSINDataSource = value; }
        }
        private DocumentDataSource _contractSINDataSource = null;

        public DocumentDataSource ContractSINDataSource
        {
            get { return _contractSINDataSource; }
            set { _contractSINDataSource = value; }
        }

        // ScheduleSIN parameters
        private Parameter _scheduleNumberParameter = null;

        // ContractSIN parameters
        private Parameter _contractNumberParameter = null;
        private Parameter _SINParameter = null;
        private Parameter _withAddParameter = null;
        private Parameter _contractSINIDParameter = null;

        public string SINParameterValue
        {
            get
            {
                return ( _SINParameter.DefaultValue );
            }
            set
            {
                _SINParameter.DefaultValue = value;
            }
        }

        private Parameter _recoverableParameter = null;

        public string RecoverableParameterValue
        {
            get
            {
                return ( _recoverableParameter.DefaultValue );
            }
            set
            {
                _recoverableParameter.DefaultValue = value;
            }
        }

        public bool AddingRecord
        {
            get
            {
                return ( bool.Parse( _withAddParameter.DefaultValue.ToString() ) );
            }
            set
            {
                _withAddParameter.DefaultValue = ( value == true ) ? "true" : "false";
            }
        }

        private DocumentDataSource _solicitationDataSource = null;

        public DocumentDataSource SolicitationDataSource
        {
            get { return _solicitationDataSource; }
            set { _solicitationDataSource = value; }
        }

        // solicitation data source parameter
        private Parameter _solicitationActiveParameter = null;

        public string SolicitationActiveParameterValue
        {
            get
            {
                return ( _solicitationActiveParameter.DefaultValue );
            }
            set
            {
                _solicitationActiveParameter.DefaultValue = value;
            }
        }

        // general

        private Parameter _associatedBPAContractsContractNumberParameter;

        public string AssociatedBPAContractsContractNumberParameterValue
        {
            get { return _associatedBPAContractsContractNumberParameter.DefaultValue; }
            set { _associatedBPAContractsContractNumberParameter.DefaultValue = value; }
        }

        // rebates

        private DocumentDataSource _rebateDataSource;

        public DocumentDataSource RebateDataSource
        {
            get { return _rebateDataSource; }
            set { _rebateDataSource = value; }
        }
        private DocumentDataSource _rebateDateDataSource;

        public DocumentDataSource RebateDateDataSource
        {
            get { return _rebateDateDataSource; }
            set { _rebateDateDataSource = value; }
        }
        private DocumentDataSource _rebateStandardClauseNameDataSource;

        public DocumentDataSource RebateStandardClauseNameDataSource
        {
            get { return _rebateStandardClauseNameDataSource; }
            set { _rebateStandardClauseNameDataSource = value; }
        }

        private DataSet _dsRebateStandardClauses;

        public DataSet RebateStandardClausesDataSet
        {
            get { return _dsRebateStandardClauses; }
            set { _dsRebateStandardClauses = value; }
        }

        private Parameter _withAddRebateParameter;

        public bool AddingRebateRecord
        {
            get
            {
                return ( bool.Parse( _withAddRebateParameter.DefaultValue.ToString() ) );
            }
            set
            {
                _withAddRebateParameter.DefaultValue = ( value == true ) ? "true" : "false";
            }
        }

        private Parameter _rebateContractNumberParameter;

        public string RebateContractNumberParameterValue
        {
            get { return _rebateContractNumberParameter.DefaultValue; }
            set { _rebateContractNumberParameter.DefaultValue = value; }
        }

        // shared
        private Parameter _userLoginParameter;

        public string UserLoginParameterValue
        {
            get { return _userLoginParameter.DefaultValue; }
            set { _userLoginParameter.DefaultValue = value; }
        }

        private Parameter _currentUserParameter;

        public string CurrentUserParameterValue
        {
            get { return _currentUserParameter.DefaultValue; }
            set { _currentUserParameter.DefaultValue = value; }
        }

        private Parameter _clauseTypeParameter;
        private Parameter _rebateDateSelectStartDate;
        private Parameter _rebateDateSelectEndDate;

        private Parameter _standardRebateTermIdParameter;

        public string StandardRebateTermIdParameterValue
        {
            get { return _standardRebateTermIdParameter.DefaultValue; }
            set { _standardRebateTermIdParameter.DefaultValue = value; }
        }
        private Parameter _rebateIdParameter;

        public string RebateIdParameterValue
        {
            get { return _rebateIdParameter.DefaultValue; }
            set { _rebateIdParameter.DefaultValue = value; }
        }
        private Parameter _startQuarterIdParameter;

        public string StartQuarterIdParameterValue
        {
            get { return _startQuarterIdParameter.DefaultValue; }
            set { _startQuarterIdParameter.DefaultValue = value; }
        }

        private Parameter _endQuarterIdParameter;

        public string EndQuarterIdParameterValue
        {
            get { return _endQuarterIdParameter.DefaultValue; }
            set { _endQuarterIdParameter.DefaultValue = value; }
        }

        private Parameter _rebatePercentOfSalesParameter;

        public string RebatePercentOfSalesParameterValue
        {
            get { return _rebatePercentOfSalesParameter.DefaultValue; }
            set { _rebatePercentOfSalesParameter.DefaultValue = value; }
        }
        private Parameter _rebateThresholdParameter;

        public string RebateThresholdParameterValue
        {
            get { return _rebateThresholdParameter.DefaultValue; }
            set { _rebateThresholdParameter.DefaultValue = value; }
        }
        private Parameter _amountReceivedParameter;

        public string AmountReceivedParameterValue 
        {
            get { return _amountReceivedParameter.DefaultValue; }
            set { _amountReceivedParameter.DefaultValue = value; }
        }
        private Parameter _isCustomParameter;

        public bool IsCustomParameterValue
        {
            get 
            { 
                return( bool.Parse( _isCustomParameter.DefaultValue.ToString() ));
            }
            set 
            { 
                _isCustomParameter.DefaultValue = ( value == true ) ? "true" : "false"; 
            }
        }

        private Parameter _rebateClauseParameter;

        public string RebateClauseParameterValue
        {
            get { return _rebateClauseParameter.DefaultValue; }
            set { _rebateClauseParameter.DefaultValue = value; }
        }

        private Parameter _customRebateIdParameter;
        private Parameter _rebateTermIdParameter;
        private Parameter _rebateIdForInsertParameter;
        private Parameter _rebateCustomStartDateParameter;

        public string RebateCustomStartDateParameterValue
        {
            get { return _rebateCustomStartDateParameter.DefaultValue; }
            set { _rebateCustomStartDateParameter.DefaultValue = value; }
        }

        // sales

        private Parameter _salesContractNumberParameter;

        public string SalesContractNumberParameterValue
        {
            get { return _salesContractNumberParameter.DefaultValue; }
            set { _salesContractNumberParameter.DefaultValue = value; }
        }

        private DocumentDataSource _salesSummaryDataSource;

        public DocumentDataSource SalesSummaryDataSource
        {
            get { return _salesSummaryDataSource; }
            set { _salesSummaryDataSource = value; }
        }
        private DocumentDataSource _yearQuarterDataSource;

        public DocumentDataSource YearQuarterDataSource
        {
            get { return _yearQuarterDataSource; }
            set { _yearQuarterDataSource = value; }
        }

        // checks

        private DocumentDataSource _checkDataSource;

        public DocumentDataSource CheckDataSource
        {
            get { return _checkDataSource; }
            set { _checkDataSource = value; }
        }

        private DocumentDataSource _checkDateDataSource;

        public DocumentDataSource CheckDateDataSource
        {
            get { return _checkDateDataSource; }
            set { _checkDateDataSource = value; }
        }

        private Parameter _withAddCheckParameter;

        public bool AddingCheckRecord
        {
            get
            {
                return ( bool.Parse( _withAddCheckParameter.DefaultValue.ToString() ) );
            }
            set
            {
                _withAddCheckParameter.DefaultValue = ( value == true ) ? "true" : "false";
            }
        }

        private Parameter _checkContractNumberParameter;

        public string CheckContractNumberParameterValue
        {
            get { return _checkContractNumberParameter.DefaultValue; }
            set { _checkContractNumberParameter.DefaultValue = value; }
        }

        private Parameter _checkContractIdParameter;

        public string CheckContractIdParameterValue
        {
            get { return _checkContractIdParameter.DefaultValue; }
            set { _checkContractIdParameter.DefaultValue = value; }
        }

        private Parameter _checkIdParameter;

        public string CheckIdParameterValue
        {
            get { return _checkIdParameter.DefaultValue; }
            set { _checkIdParameter.DefaultValue = value; }
        }

        private Parameter _checkIdForInsertParameter;

        public string CheckIdForInsertParameterValue
        {
            get { return _checkIdForInsertParameter.DefaultValue; }
            set { _checkIdForInsertParameter.DefaultValue = value; }
        }
        

        private Parameter _checkQuarterIdParameter;

        public string CheckQuarterIdParameterValue
        {
            get { return _checkQuarterIdParameter.DefaultValue; }
            set { _checkQuarterIdParameter.DefaultValue = value; }
        }

        private Parameter _checkAmountParameter;

        public string CheckAmountParameterValue
        {
            get { return _checkAmountParameter.DefaultValue; }
            set { _checkAmountParameter.DefaultValue = value; }
        }

        private Parameter _checkNumberParameter;

        public string CheckNumberParameterValue
        {
            get { return _checkNumberParameter.DefaultValue; }
            set { _checkNumberParameter.DefaultValue = value; }
        }

        private Parameter _checkDepositNumberParameter;

        public string CheckDepositNumberParameterValue
        {
            get { return _checkDepositNumberParameter.DefaultValue; }
            set { _checkDepositNumberParameter.DefaultValue = value; }
        }

         private Parameter _checkDepositTicketNumberParameter;

        public string CheckDepositTicketNumberParameterValue
        {
            get { return _checkDepositTicketNumberParameter.DefaultValue; }
            set { _checkDepositTicketNumberParameter.DefaultValue = value; }
        }

        private Parameter _checkDateReceivedParameter;

        public string CheckDateReceivedParameterValue
        {
            get { return _checkDateReceivedParameter.DefaultValue; }
            set { _checkDateReceivedParameter.DefaultValue = value; }
        }

        private Parameter _checkSettlementDateParameter;

        public string CheckSettlementDateParameterValue
        {
            get { return _checkSettlementDateParameter.DefaultValue; }
            set { _checkSettlementDateParameter.DefaultValue = value; }
        }

        private Parameter _checkCommentsParameter;

        public string CheckCommentsParameterValue
        {
            get { return _checkCommentsParameter.DefaultValue; }
            set { _checkCommentsParameter.DefaultValue = value; }
        }

        
        // payments

        private DocumentDataSource _paymentDataSource;

        public DocumentDataSource PaymentDataSource
        {
            get { return _paymentDataSource; }
            set { _paymentDataSource = value; }
        }

            
        private Parameter _paymentContractNumberParameter;

        public string PaymentContractNumberParameterValue
        {
            get { return _paymentContractNumberParameter.DefaultValue; }
            set { _paymentContractNumberParameter.DefaultValue = value; }
        }

       
        //  National payments

        private DocumentDataSource _nationalPaymentDataSource;

        public DocumentDataSource NationalPaymentDataSource
        {
            get { return _nationalPaymentDataSource; }
            set { _nationalPaymentDataSource = value; }
        }

        private DocumentDataSource _nationalPaymentDateDataSource;

        public DocumentDataSource NationalPaymentDateDataSource
        {
            get { return _nationalPaymentDateDataSource; }
            set { _nationalPaymentDateDataSource = value; }
        }
        
        private Parameter _nationalPaymentContractNumberParameter;

        public string NationalPaymentContractNumberParameterValue
        {
            get { return _nationalPaymentContractNumberParameter.DefaultValue; }
            set { _nationalPaymentContractNumberParameter.DefaultValue = value; }
        }

        private Parameter _withAddPaymentParameter;

        public bool AddingPaymentRecord
        {
            get
            {
                return ( bool.Parse( _withAddPaymentParameter.DefaultValue.ToString() ) );
            }
            set
            {
                _withAddPaymentParameter.DefaultValue = ( value == true ) ? "true" : "false";
            }
        }


        private Parameter _paymentContractIdParameter;

        public string PaymentContractIdParameterValue
        {
            get { return _paymentContractIdParameter.DefaultValue; }
            set { _paymentContractIdParameter.DefaultValue = value; }
        }

        private Parameter _SRPActivityIdParameter;

        public string SRPActivityIdParameterValue
        {
            get { return _SRPActivityIdParameter.DefaultValue; }
            set { _SRPActivityIdParameter.DefaultValue = value; }
        }

        private Parameter _SRPActivityIdForInsertParameter;

        public string _SRPActivityIdForInsertParameterValue
        {
            get { return _SRPActivityIdForInsertParameter.DefaultValue; }
            set { _SRPActivityIdForInsertParameter.DefaultValue = value; }
        }
        

        private Parameter _paymentQuarterIdParameter;

        public string PaymentQuarterIdParameterValue
        {
            get { return _paymentQuarterIdParameter.DefaultValue; }
            set { _paymentQuarterIdParameter.DefaultValue = value; }
        }

        private Parameter _paymentAmountParameter;

        public string PaymentAmountParameterValue
        {
            get { return _paymentAmountParameter.DefaultValue; }
            set { _paymentAmountParameter.DefaultValue = value; }
        }

        private Parameter _submissionDateParameter;
        public string SubmissionDateParameterValue
        {
            get { return _submissionDateParameter.DefaultValue; }
            set { _submissionDateParameter.DefaultValue = value; }
        }

        private Parameter _submittedByUserNameParameter;
        public string SubmittedByUserNameParameterValue
        {
            get { return _submittedByUserNameParameter.DefaultValue; }
            set { _submittedByUserNameParameter.DefaultValue = value; }
        }

        private Parameter _paymentMethodParameter;        
        public string PaymentMethodParameterValue
        {
            get { return _paymentMethodParameter.DefaultValue; }
            set { _paymentMethodParameter.DefaultValue = value; }
        }

        private Parameter _paymentSourceParameter;        
        public string PaymentSourceParameterValue
        {
            get { return _paymentSourceParameter.DefaultValue; }
            set { _paymentSourceParameter.DefaultValue = value; }
        }

        private Parameter _transactionIdParameter;        
        public string TransactionIdParameterValue
        {
            get { return _transactionIdParameter.DefaultValue; }
            set { _transactionIdParameter.DefaultValue = value; }
        }

        private Parameter _payGovTrackingIdParameter;
        public string PayGovTrackingIdParameterValue
        {
            get { return _payGovTrackingIdParameter.DefaultValue; }
            set { _payGovTrackingIdParameter.DefaultValue = value; }
        }

        private Parameter _paymentDepositTicketNumberParameter;
        public string PaymentDepositTicketNumberParameterValue
        {
            get { return _paymentDepositTicketNumberParameter.DefaultValue; }
            set { _paymentDepositTicketNumberParameter.DefaultValue = value; }
        }

        private Parameter _paymentDebitVoucherNumberParameter;
        public string PaymentDebitVoucherNumberParameterValue
        {
            get { return _paymentDebitVoucherNumberParameter.DefaultValue; }
            set { _paymentDebitVoucherNumberParameter.DefaultValue = value; }
        }

        private Parameter _paymentCheckNumberParameter;

        public string PaymentCheckNumberParameterValue
        {
            get { return _paymentCheckNumberParameter.DefaultValue; }
            set { _paymentCheckNumberParameter.DefaultValue = value; }
        }


        private Parameter _paymentSettlementDateParameter;

        public string PaymentSettlementDateParameterValue
        {
            get { return _paymentSettlementDateParameter.DefaultValue; }
            set { _paymentSettlementDateParameter.DefaultValue = value; }
        }

        private Parameter _paymentCommentsParameter;

        public string PaymentCommentsParameterValue
        {
            get { return _paymentCommentsParameter.DefaultValue; }
            set { _paymentCommentsParameter.DefaultValue = value; }
        }


        // sba projections

        private DocumentDataSource _projectionDataSource;

        public DocumentDataSource ProjectionDataSource
        {
            get { return _projectionDataSource; }
            set { _projectionDataSource = value; }
        }

        // sba associated contracts

        private DocumentDataSource _sbaAssociatedContractsDataSource;

        public DocumentDataSource SBAAssociatedContractsDataSource
        {
            get { return _sbaAssociatedContractsDataSource; }
            set { _sbaAssociatedContractsDataSource = value; }
        }


        // sba associated contracts

        private DocumentDataSource _bpaAssociatedContractsDataSource;

        public DocumentDataSource AssociatedBPAContractsDataSource
        {
            get { return _bpaAssociatedContractsDataSource; }
            set { _bpaAssociatedContractsDataSource = value; }
        }

        //private DocumentDataSource _projectionDateDataSource;

        //public DocumentDataSource ProjectionDateDataSource
        //{
        //    get { return _projectionDateDataSource; }
        //    set { _projectionDateDataSource = value; }
        //}

        private Parameter _withAddProjectionParameter;

        public bool AddingProjectionRecord
        {
            get
            {
                return ( bool.Parse( _withAddProjectionParameter.DefaultValue.ToString() ) );
            }
            set
            {
                _withAddProjectionParameter.DefaultValue = ( value == true ) ? "true" : "false";
            }
        }

        private Parameter _projectionContractNumberParameter;

        public string ProjectionContractNumberParameterValue
        {
            get { return _projectionContractNumberParameter.DefaultValue; }
            set { _projectionContractNumberParameter.DefaultValue = value; }
        }

        private Parameter _sbaPlanIdParameter;

        public string SBAPlanIdParameterValue
        {
            get { return _sbaPlanIdParameter.DefaultValue; }
            set { _sbaPlanIdParameter.DefaultValue = value; }
        }

        private Parameter _projectionIdParameter;

        public string ProjectionIdParameterValue
        {
            get { return _projectionIdParameter.DefaultValue; }
            set { _projectionIdParameter.DefaultValue = value; }
        }

        private Parameter _projectionIdForInsertParameter;

        public string ProjectionIdForInsertParameterValue
        {
            get { return _projectionIdForInsertParameter.DefaultValue; }
            set { _projectionIdForInsertParameter.DefaultValue = value; }
        }

        private Parameter _projectionStartDateParameter;

        public string ProjectionStartDateParameterValue
        {
            get { return _projectionStartDateParameter.DefaultValue; }
            set { _projectionStartDateParameter.DefaultValue = value; }
        }

        private Parameter _projectionEndDateParameter;

        public string ProjectionEndDateParameterValue
        {
            get { return _projectionEndDateParameter.DefaultValue; }
            set { _projectionEndDateParameter.DefaultValue = value; }
        }

        private Parameter _totalSubContractingDollarsParameter;

        public string TotalSubContractingDollarsParameterValue
        {
            get { return _totalSubContractingDollarsParameter.DefaultValue; }
            set { _totalSubContractingDollarsParameter.DefaultValue = value; }
        }

        private Parameter _sbDollarsParameter;

        public string SBDollarsParameterValue
        {
            get { return _sbDollarsParameter.DefaultValue; }
            set { _sbDollarsParameter.DefaultValue = value; }
        }

        private Parameter _veteranOwnedDollarsParameter;

        public string VeteranOwnedDollarsParameterValue
        {
            get { return _veteranOwnedDollarsParameter.DefaultValue; }
            set { _veteranOwnedDollarsParameter.DefaultValue = value; }
        }
        private Parameter _disabledVeteranDollarsParameter;

        public string DisabledVeteranDollarsParameterValue
        {
            get { return _disabledVeteranDollarsParameter.DefaultValue; }
            set { _disabledVeteranDollarsParameter.DefaultValue = value; }
        }
        private Parameter _sdbDollarsParameter;

        public string SDBDollarsParameterValue
        {
            get { return _sdbDollarsParameter.DefaultValue; }
            set { _sdbDollarsParameter.DefaultValue = value; }
        }

        private Parameter _womenOwnedDollarsParameter;

        public string WomenOwnedDollarsParameterValue
        {
            get { return _womenOwnedDollarsParameter.DefaultValue; }
            set { _womenOwnedDollarsParameter.DefaultValue = value; }
        }

        private Parameter _hubZoneDollarsParameter;

        public string HubZoneDollarsParameterValue
        {
            get { return _hubZoneDollarsParameter.DefaultValue; }
            set { _hubZoneDollarsParameter.DefaultValue = value; }
        }

        private Parameter _hbcuDollarsParameter;

        public string HBCUDollarsParameterValue
        {
            get { return _hbcuDollarsParameter.DefaultValue; }
            set { _hbcuDollarsParameter.DefaultValue = value; }
        }

        private Parameter _projectionCommentsParameter;

        public string ProjectionCommentsParameterValue
        {
            get { return _projectionCommentsParameter.DefaultValue; }
            set { _projectionCommentsParameter.DefaultValue = value; }
        }

        // SBA Plan Details ( Plan contact information and plan type )
        private DocumentDataSource _sbaPlanDetailsDataSource = null;

        public DocumentDataSource SBAPlanDetailsDataSource
        {
            get { return _sbaPlanDetailsDataSource; }
            set { _sbaPlanDetailsDataSource = value; }
        }

        private Parameter _sbaPlanDetailsContractNumberParameter = null;

        private Parameter _sbaPlanIdForDetailsParameter;

        public string SBAPlanIdForDetailsParameterValue
        {
            get { return _sbaPlanIdForDetailsParameter.DefaultValue; }
            set { _sbaPlanIdForDetailsParameter.DefaultValue = value; }
        }

        private Parameter _associatedContractsContractNumberParameter;

        public string AssociatedContractsContractNumberParameterValue
        {
            get { return _associatedContractsContractNumberParameter.DefaultValue; }
            set { _associatedContractsContractNumberParameter.DefaultValue = value; }
        }

        private Parameter _associatedContractsSBAPlanIdParameter;

        public string AssociatedContractsSBAPlanIdParameterValue
        {
            get { return _associatedContractsSBAPlanIdParameter.DefaultValue; }
            set { _associatedContractsSBAPlanIdParameter.DefaultValue = value; }
        }

        private DataSet _contractingOfficersDataSet;

        public DataSet ContractingOfficersDataSet
        {
            get { return _contractingOfficersDataSet; }
            set { _contractingOfficersDataSet = value; }
        }

        // selection of potential parent contracts for new BPA
        private DataSet _parentContractsForScheduleDataSet;

        public DataSet ParentContractsForScheduleDataSet
        {
            get { return _parentContractsForScheduleDataSet; }
            set { _parentContractsForScheduleDataSet = value; }
        }

        private DataSet _schedulesDataSet;

        public DataSet SchedulesDataSet        
        {
            get { return _schedulesDataSet; }
            set { _schedulesDataSet = value; }
        }

        private DataSet _extendableContractsDataSet;

        public DataSet ExtendableContractsDataSet
        {
            get { return _extendableContractsDataSet; }
            set { _extendableContractsDataSet = value; }
        }

        private DataSet _divisionsDataSet;

        public DataSet DivisionsDataSet
        {
            get { return _divisionsDataSet; }
            set { _divisionsDataSet = value; }
        }

        private DataSet _pointsOfContactStateCodeDataSet;

        public DataSet PointsOfContactStateCodeDataSet
        {
            get { return _pointsOfContactStateCodeDataSet; }
            set { _pointsOfContactStateCodeDataSet = value; }
        }

        private DataSet _pointsOfContactVendorCountriesDataSet;

        public DataSet PointsOfContactVendorCountriesDataSet
        {
            get
            {
                return _pointsOfContactVendorCountriesDataSet;
            }

            set
            {
                _pointsOfContactVendorCountriesDataSet = value;
            }
        }

        private DataSet _offerActionTypesDataSet;

        public DataSet OfferActionTypesDataSet
        {
            get { return _offerActionTypesDataSet; }
            set { _offerActionTypesDataSet = value; }
        }

        private DataSet _activeSolicitationsDataSet;

        public DataSet ActiveSolicitationsDataSet
        {
            get { return _activeSolicitationsDataSet; }
            set { _activeSolicitationsDataSet = value; }
        }

        public void Page_Init( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }

            _currentDocument = ( CurrentDocument )Page.Session[ "CurrentDocument" ];

            if( _documentEditorType == DocumentEditorTypes.Contract || _documentEditorType == DocumentEditorTypes.NewContract || _documentEditorType == DocumentEditorTypes.NewContractFromOffer )
            {
                if( Session[ "DataRelay" ] == null )
                {
                    _dataRelay = new DataRelay( DocumentTypes.Contract );
                    Session[ "DataRelay" ] = _dataRelay;
                }
                else
                {
                    if( CurrentDocumentIsChanging == true )
                    {
                        _dataRelay = new DataRelay( DocumentTypes.Contract );
                        Session[ "DataRelay" ] = _dataRelay;
                        ClearContractSessionVariables();
                    }
                    else
                    {
                        _dataRelay = ( DataRelay )Session[ "DataRelay" ];
                        _dataRelay.RestoreDelegatesAfterDeserialization();
                    }
                }

                _dataRelay.SetInternalEventHandlers();
                _dataRelay.SetCurrentPage( Page );
                _dataRelay.UpdateSuccessEvent += new UpdateSuccessEventHandler( _dataRelay_UpdateSuccessEvent );
                _dataRelay.InsertSuccessEvent += new InsertSuccessEventHandler( _dataRelay_InsertSuccessEvent );

                if( Session[ "ValidationErrorManager" ] == null )
                {
                    _validationErrorManager = new ValidationErrorManager( DocumentTypes.Contract );
                    Session[ "ValidationErrorManager" ] = _validationErrorManager;
                }
                else
                {
                    if( CurrentDocumentIsChanging == true )
                    {
                        _validationErrorManager = new ValidationErrorManager( DocumentTypes.Contract );
                        Session[ "ValidationErrorManager" ] = _validationErrorManager;
                    }
                    else
                    {
                        _validationErrorManager = ( ValidationErrorManager )Session[ "ValidationErrorManager" ];
                    }
                }
            }
            else if( _documentEditorType == DocumentEditorTypes.Offer || _documentEditorType == DocumentEditorTypes.NewOffer )
            {
                if( Session[ "OfferDataRelay" ] == null )
                {
                    _offerDataRelay = new OfferDataRelay( DocumentTypes.Offer );
                    Session[ "OfferDataRelay" ] = _offerDataRelay;
                }
                else
                {
                    if( CurrentDocumentIsChanging == true )
                    {
                        _offerDataRelay = new OfferDataRelay( DocumentTypes.Offer );
                        Session[ "OfferDataRelay" ] = _offerDataRelay;
                        ClearOfferSessionVariables();
                    }
                    else
                    {
                        _offerDataRelay = ( OfferDataRelay )Session[ "OfferDataRelay" ];
                        _offerDataRelay.RestoreDelegatesAfterDeserialization();
                    }
                }

                _offerDataRelay.SetInternalEventHandlers();
                _offerDataRelay.SetCurrentPage( Page );
                _offerDataRelay.UpdateSuccessEvent += new OfferUpdateSuccessEventHandler( _offerDataRelay_UpdateSuccessEvent );
                _offerDataRelay.InsertSuccessEvent += new OfferInsertSuccessEventHandler( _offerDataRelay_InsertSuccessEvent );

                if( Session[ "ValidationErrorManager" ] == null )
                {
                    _validationErrorManager = new ValidationErrorManager( DocumentTypes.Offer );
                    Session[ "ValidationErrorManager" ] = _validationErrorManager;
                }
                else
                {
                    if( CurrentDocumentIsChanging == true )
                    {
                        _validationErrorManager = new ValidationErrorManager( DocumentTypes.Offer );
                        Session[ "ValidationErrorManager" ] = _validationErrorManager;
                    }
                    else
                    {
                        _validationErrorManager = ( ValidationErrorManager )Session[ "ValidationErrorManager" ];
                    }
                }
            }

           
        }

        // not called 
        public void Page_Load( object sender, EventArgs e )
        {          
        }
    
        void _dataRelay_UpdateSuccessEvent( DataRelayUpdateSuccessEventArgs args )
        {
            Session[ "ContractSearchDataDirtyFlag" ] = true; // invalidates any session search data $$$ may remove this if not needed due to caching

            if( _documentEditorType == DocumentEditorTypes.Contract )
            {

                bool bRefreshHeader = _dataRelay.EditedDocumentContentFront.DirtyFlags.IsContractHeaderInfoDirty;
                bool bRefreshPersonalizedNotificationPane = _dataRelay.EditedDocumentContentFront.DirtyFlags.IsPersonalizedNotificationPaneDirty;
       
       
                _dataRelay.EditedDocumentContentFront.ClearDirtyFlags();

                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

                // current document may have changed owners ( added 4/11/17 )
                Session[ "DocumentControlPresentation" ] = null;
                DocumentControlPresentation documentControlPresentation = new DocumentControlPresentation( currentDocument );
                Session[ "DocumentControlPresentation" ] = documentControlPresentation;

                if( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Active )
                {
                    CMGlobals.ExpireCache( args.CurrentPage, "ActiveContractHeaderDataSet" );
                    CMGlobals.ExpireCache( args.CurrentPage, "AllContractHeaderDataSet" );
                }
                else
                {
                    CMGlobals.ExpireCache( args.CurrentPage, "ExpiredContractHeaderDataSet" );
                    CMGlobals.ExpireCache( args.CurrentPage, "AllContractHeaderDataSet" );
                }

                if( bRefreshHeader == true )
                {
                    // copy lookup data from back to front
                    RefreshHeaderLookupDataAfterUpdate();
                    // rebind header
                    BindHeader();
                }

                if( bRefreshPersonalizedNotificationPane == true )
                {            
                    LoadPersonalizedContractList();
                    LoadPersonalizedNotifications();
                }

                if( this.GetType().Name.CompareTo( "contractsbaplan_aspx" ) == 0 )
                {
                    ( ( ContractSBAPlan )this ).ReloadAfterSave();
                }
                     
            }

            HideProgressIndicator( args.CurrentPage );
        }

        void _offerDataRelay_UpdateSuccessEvent( DataRelayUpdateSuccessEventArgs args )
        {
            Session[ "OfferSearchDataDirtyFlag" ] = true; // invalidates any session search data $$$ may remove this if not needed due to caching

            bool bImpactsExpiredOfferHeaderCache = _offerDataRelay.EditedOfferContentFront.DirtyFlags.ImpactsExpiredOfferHeaderCache;  // this refers to the list of header on  the search screen

            bool bRefreshHeader = _offerDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferHeaderInfoDirty; // note this is the editor header not the list of headers on the search screen

            _offerDataRelay.EditedOfferContentFront.ClearDirtyFlags();

            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            // current document may have changed owners ( added 4/11/17 )
            Session[ "DocumentControlPresentation" ] = null;
            DocumentControlPresentation documentControlPresentation = new DocumentControlPresentation( currentDocument );
            Session[ "DocumentControlPresentation" ] = documentControlPresentation;

            if( currentDocument.ActiveStatus == CurrentDocument.ActiveStatuses.Active )
            {
                CMGlobals.ExpireCache( args.CurrentPage, "ActiveOfferHeaderDataSet" );
            }
            else
            {
                // since the expired offer header data set is large, only expire the cache if a new row was added
                if( bImpactsExpiredOfferHeaderCache == true )
                    CMGlobals.ExpireCache( args.CurrentPage, "ExpiredOfferHeaderDataSet" );
            }

            if( bRefreshHeader == true )
            {
                // copy lookup data from back to front
                RefreshHeaderLookupDataAfterUpdate();
                RebindHeader();
            }

            HideProgressIndicator( args.CurrentPage );
        }

        void _dataRelay_InsertSuccessEvent( DataRelayInsertSuccessEventArgs args )
        {
            Session[ "ContractSearchDataDirtyFlag" ] = true; // invalidates any session search data $$$ may remove this if not needed due to caching

            if( _documentEditorType == DocumentEditorTypes.NewContract || _documentEditorType == DocumentEditorTypes.NewContractFromOffer )
            {
                // new contract, so always reload the personalized pane
                LoadPersonalizedContractList();
                LoadPersonalizedNotifications();                

                _dataRelay.EditedDocumentContentFront.ClearDirtyFlags();
                CMGlobals.ExpireCache( args.CurrentPage, "ActiveContractHeaderDataSet" );
                CMGlobals.ExpireCache( args.CurrentPage, "AllContractHeaderDataSet" );

                // create the CurrentDocument object

                EditedDocumentContent newContract = DataRelay.EditedDocumentDataSourceFront.EditedDocumentContentFront;
                CurrentDocument currentDocument = new CurrentDocument( newContract.ContractId, newContract.ContractNumber, newContract.ScheduleNumber, ( ContractDB )Session[ "ContractDB" ], ( DrugItemDB )Session[ "DrugItemDB" ], ( ItemDB )Session[ "ItemDB" ], true );

                CurrentDocument.SetCurrentDocument( currentDocument, CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.CreateContract );

                currentDocument.ContractId = newContract.ContractId;
                currentDocument.ActiveStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.ActiveStatuses.Active;
                currentDocument.OwnerId = newContract.COID;
                currentDocument.OwnerName = newContract.ContractingOfficerFullName;
                currentDocument.ScheduleName = newContract.ScheduleName;

                currentDocument.ContractNumber = newContract.ContractNumber;
                currentDocument.VendorName = newContract.VendorName;
                currentDocument.Description = newContract.ContractDescription;
                currentDocument.AwardDate = newContract.ContractAwardDate;
                currentDocument.EffectiveDate = newContract.ContractEffectiveDate;
                currentDocument.ExpirationDate = newContract.ContractExpirationDate;
                currentDocument.CompletionDate = newContract.ContractCompletionDate;
                currentDocument.VendorCountryId = newContract.VendorCountryId;

                BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
                bs.SetDocumentEditStatus( currentDocument );

                DocumentControlPresentation documentControlPresentation = new DocumentControlPresentation( currentDocument );
                Session[ "DocumentControlPresentation" ] = documentControlPresentation;

            }

            HideProgressIndicator( args.CurrentPage );
        }

        void _offerDataRelay_InsertSuccessEvent( DataRelayInsertSuccessEventArgs args )
        {
            Session[ "OfferSearchDataDirtyFlag" ] = true; // invalidates any session search data $$$ may remove this if not needed due to caching

            if( _documentEditorType == DocumentEditorTypes.NewOffer )
            {                
                _offerDataRelay.EditedOfferContentFront.ClearDirtyFlags();
                CMGlobals.ExpireCache( args.CurrentPage, "ActiveOfferHeaderDataSet" );

                // create the CurrentDocument object

                EditedOfferContent newOffer = OfferDataRelay.EditedOfferDataSourceFront.EditedOfferContentFront;

                CurrentDocument currentDocument = new CurrentDocument( newOffer.OfferId, newOffer.ScheduleNumber, newOffer.ScheduleName, newOffer.VendorName, newOffer.DateReceived, newOffer.DateAssigned, newOffer.COID, newOffer.ContractingOfficerFullName,
                    newOffer.ContractNumber, newOffer.ContractId, newOffer.IsOfferCompleted, newOffer.OfferNumber, newOffer.ProposalTypeId, newOffer.ExtendsContractNumber, newOffer.ExtendsContractId, ( OfferDB )Session[ "OfferDB" ], ( ContractDB )Session[ "ContractDB" ], ( DrugItemDB )Session[ "DrugItemDB" ], ( ItemDB )Session[ "ItemDB" ] );

                CurrentDocument.SetCurrentDocument( currentDocument, CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.CreateOffer );

                currentDocument.ActiveStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.ActiveStatuses.Active;
  
                BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
                bs.SetDocumentEditStatus( currentDocument );

                DocumentControlPresentation documentControlPresentation = new DocumentControlPresentation( currentDocument );
                Session[ "DocumentControlPresentation" ] = documentControlPresentation;
            }

            RebindHeader();

            HideProgressIndicator( args.CurrentPage );
        }

        private void ClearSessionVariables()
        {
          //  Session[ "ContractingOfficersDataSet" ] = null; taking these out 12/6/14 - added to clear for offer
          //  Session[ "SchedulesDataSet" ] = null;
        }

        private void ClearOfferSessionVariables()
        {
            ClearOfferGeneralSessionVariables();
            ClearOfferPointsOfContactSessionVariables();
            ClearOfferCommentsSalesSessionVariables();
            ClearOfferAwardSessionVariables();
            Session[ "ValidationErrorManager" ] = null;

        }

        private void ClearContractSessionVariables()
        {
            ClearContractGeneralSessionVariables();
            ClearContractRebateSessionVariables();
            ClearContractSalesSessionVariables();
            ClearContractCheckSessionVariables();
            ClearContractPaymentSessionVariables();
            ClearContractNationalPaymentSessionVariables();
            ClearContractSBAProjectionSessionVariables();
            ClearSBAPlanDetailsSessionVariables();
            ClearSBAAssociatedContractsSessionVariables();
            Session[ "ValidationErrorManager" ] = null;
        }

        private void ClearContractGeneralSessionVariables()
        {
            Page.Session[ "ScheduleSINDataSource" ] = null;
            Page.Session[ "ContractSINDataSource" ] = null;
            Page.Session[ "LastInsertedContractSINId" ] = null;
            Page.Session[ "AssociatedBPAContractsGridViewSelectedIndex" ] = null;
            Page.Session[ "AssociatedBPAContractsDataSource" ] = null;
        }

        private void ClearContractRebateSessionVariables()
        {
            Page.Session[ "RebateDataSource" ] = null;
            Page.Session[ "RebateDateDataSource" ] = null;
            Page.Session[ "LastUpdatedRebateId" ] = null;
            Page.Session[ "LastInsertedRebateId" ] = null;
        }

        private void ClearContractSalesSessionVariables()
        {
            Page.Session[ "SalesSummaryDataSource" ] = null;
        }

        private void ClearContractCheckSessionVariables()
        {
            Page.Session[ "CheckDataSource" ] = null;
            Page.Session[ "CheckDateDataSource" ] = null;

        }

        private void ClearContractPaymentSessionVariables()
        {
            Page.Session[ "PaymentDataSource" ] = null;           
        }

        private void ClearContractNationalPaymentSessionVariables()
        {
            Page.Session[ "NationalPaymentDataSource" ] = null;
            Page.Session[ "NationalPaymentDateDataSource" ] = null;
        }
       
        private void ClearContractSBAProjectionSessionVariables()
        {
            Page.Session[ "ProjectionDataSource" ] = null;
        }

        private void ClearSBAPlanDetailsSessionVariables()
        {
            Page.Session[ "SBAPlanDetailsDataSource" ] = null;
        }

        private void ClearSBAAssociatedContractsSessionVariables()
        {
            Page.Session[ "SBAAssociatedContractsDataSource" ] = null;
        }

        private void ClearOfferGeneralSessionVariables()
        {
      //      Session[ "ContractingOfficersDataSet" ] = null;
      //      Session[ "SchedulesDataSet" ] = null;
        //    Session[ "OfferActionTypesDataSet" ] = null;
         //   Session[ "SolicitationsDataSet" ] = null;
        }

        private void ClearOfferPointsOfContactSessionVariables()
        {
        }

        private void ClearOfferCommentsSalesSessionVariables()
        {
        }

        private void ClearOfferAwardSessionVariables()
        {
        }

        public void ResetValidationGroup( string validationGroupName )
        {
            _validationErrorManager.ResetValidationGroup( validationGroupName );
        }

        public void AppendValidationError( string message, bool bIsShortSave )
        {
            _validationErrorManager.AppendValidationError( message, bIsShortSave );
        }

        public void ShowValidationErrors()
        {
            if( _validationErrorManager != null )
                ShowException( new Exception( _validationErrorManager.ToString() ) );   
        }

        protected void ShowException( Exception ex )
        {
            MasterPage master = Page.Master;
            if( master != null )
            {
                MasterPage topMaster = master.Master;

                if( topMaster != null )
                {
                    MsgBox msgBox = ( MsgBox )topMaster.FindControl( "MsgBox" );
 
                    if( msgBox != null )
                    {
                        msgBox.ShowErrorFromUpdatePanelAsync( Page, ex );
                    }
                }
            }
        }

        protected void AssignDataSourceToHeader()
        {
            MasterPage master = Page.Master;
            if( master != null )
            {
                if( _documentEditorType == DocumentEditorTypes.Contract )
                {
                    ( ( ContractView )master ).AssignObjectDataSourceToHeader( DataRelay.EditedDocumentDataSourceFront );
                }
                else if( _documentEditorType == DocumentEditorTypes.Offer )
                {
                    ( ( OfferView )master ).AssignObjectDataSourceToHeader( OfferDataRelay.EditedOfferDataSourceFront );
                }
                else if( _documentEditorType == DocumentEditorTypes.NewContract || _documentEditorType == DocumentEditorTypes.NewContractFromOffer )
                {
                    ( ( DocumentCreation )master ).AssignObjectDataSourceToHeader( DataRelay.EditedDocumentDataSourceFront );
                }
                else if( _documentEditorType == DocumentEditorTypes.NewOffer )
                {
                    ( ( OfferView )master ).AssignObjectDataSourceToHeader( OfferDataRelay.EditedOfferDataSourceFront );
                }
            }
        }

        protected void BindHeader()
        {
            MasterPage master = Page.Master;
            if( master != null )
            {
                if( _documentEditorType == DocumentEditorTypes.Contract )
                {
                    ( ( ContractView )master ).BindHeader();
                }
                else if( _documentEditorType == DocumentEditorTypes.Offer || _documentEditorType == DocumentEditorTypes.NewOffer )
                {
                    ( ( OfferView )master ).BindHeader();
                }
                else if( _documentEditorType == DocumentEditorTypes.NewContract || _documentEditorType == DocumentEditorTypes.NewContractFromOffer )
                {
                    ( ( DocumentCreation )master ).BindHeader();
                }
            }
        }

        protected void LoadPersonalizedContractList()
        {
            MasterPage master = Page.Master;
            
            if( master != null )
            {
                MasterPage topMaster = master.Master;

                if( topMaster != null )
                {
                    ( ( NACCM )topMaster ).LoadPersonalizedContractList( true );
                }
            }
        }

        protected void LoadPersonalizedNotifications()
        {
            MasterPage master = Page.Master;

            if( master != null )
            {
                MasterPage topMaster = master.Master;

                if( topMaster != null )
                {
                    ( ( NACCM )topMaster ).LoadPersonalizedNotifications( true );
                }
            }
        }

        // must be overridden by document editor
        public virtual bool SaveScreenValuesToObject( IDataRelay dataRelay, bool bIsShortSave ) 
        {
            return ( false );
        }

        // validate before back-end save to database
        // must be overridden by document editor
        public virtual bool ValidateBeforeSave( IDataRelay dataRelay, bool bIsShortSave, string validationGroupName )
        {
            return ( false );
        }

        public virtual string GetValidationGroupName()
        {
            return ( "" );
        }

        // must be overridden by document editor
        public virtual void BindAfterShortSave()
        {
        }

        public virtual void RebindHeader()
        {

        }

        protected void RefreshHeaderLookupDataAfterUpdate()
        {
            if( _documentEditorType == DocumentEditorTypes.Contract )
            {

                EditedDocumentContent editedDocumentContentFront = DataRelay.EditedDocumentDataSourceFront.EditedDocumentContentFront;

                EditedDocumentContent editedDocumentContentBack =  ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentBack" ];

                if( editedDocumentContentFront != null && editedDocumentContentBack != null )
                {
                    editedDocumentContentFront.ContractingOfficerFullName = editedDocumentContentBack.ContractingOfficerFullName;
                    editedDocumentContentFront.ContractingOfficerPhone = editedDocumentContentBack.ContractingOfficerPhone;

                    editedDocumentContentFront.SeniorContractSpecialistCOID = editedDocumentContentBack.SeniorContractSpecialistCOID;
                    editedDocumentContentFront.SeniorContractSpecialistName = editedDocumentContentFront.SeniorContractSpecialistName;

                    editedDocumentContentFront.AssistantDirectorCOID = editedDocumentContentBack.AssistantDirectorCOID;
                    editedDocumentContentFront.AssistantDirectorName = editedDocumentContentBack.AssistantDirectorName;

                    editedDocumentContentFront.VendorCountryId = editedDocumentContentBack.VendorCountryId;
                    editedDocumentContentFront.VendorCountryName = editedDocumentContentBack.VendorCountryName;
                }
            }
            else if( _documentEditorType == DocumentEditorTypes.Offer )
            {
                EditedOfferContent editedOfferContentFront = OfferDataRelay.EditedOfferDataSourceFront.EditedOfferContentFront;

                EditedOfferContent editedOfferContentBack = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentBack" ];

                if( editedOfferContentFront != null && editedOfferContentBack != null )
                {
                    editedOfferContentFront.ContractingOfficerPhone = editedOfferContentBack.ContractingOfficerPhone;
                }
            }
        }

        // save values on current tab before going to another tab
        // or save values before jumping to another contract if user responds yes to dirty save
        public void ShortSave()
        {
            // note that the contract creation screen currently only has one tab and so current there are no opportunities to execute a short save for those scenarios
            if( _documentEditorType == DocumentEditorTypes.Contract || _documentEditorType == DocumentEditorTypes.NewContract || _documentEditorType == DocumentEditorTypes.NewContractFromOffer )
            {
                if( SaveScreenValuesToObject( _dataRelay, true ) != true )
                {
                    HideProgressIndicator();
                    throw new Exception( _errorMessage );  // validation error
                }
                else // success
                {
                    // bank validation errors on current tab
                    ValidateBeforeSave( _dataRelay, true, GetValidationGroupName() );     // $$$ added 4/13/2022
                    _dataRelay.UpdateFront( _dataRelay.EditedDocumentContentFront );
                    HideProgressIndicator();
                }                
            }
            else if( _documentEditorType == DocumentEditorTypes.Offer || _documentEditorType == DocumentEditorTypes.NewOffer )
            {
                if( SaveScreenValuesToObject( _offerDataRelay, true ) != true )
                {
                    HideProgressIndicator();
                    throw new Exception( _errorMessage );  // validation error
                }
                else // success
                {
                    // bank validation errors on current tab
                    ValidateBeforeSave( _offerDataRelay, true, GetValidationGroupName() );  // $$$ added 4/13/2022
                    _offerDataRelay.UpdateFront( _offerDataRelay.EditedOfferContentFront );
                    HideProgressIndicator();
                }          
            }
        }

        // do this instead of regular short save when dirty screen is not visible
        public void ShortSaveWithoutScreen()
        {
            _dataRelay.UpdateFront( _dataRelay.EditedDocumentContentFront );
        }

        public void UpdateDocument()
        {
            if( _documentEditorType == DocumentEditorTypes.Contract )
            {
                if( SaveScreenValuesToObject( _dataRelay, false ) == true )
                {
                    // if anything has changed
                    if( _dataRelay.EditedDocumentContentFront.IsDocumentDirty() == true )
                    {
                        // validate the current tab
                        ValidateBeforeSave( _dataRelay, false, GetValidationGroupName() );

                        // check cumulative errors
                        if( _validationErrorManager.HasErrors() == false )
                        {
                            try
                            {
                                _dataRelay.Update();
                            }
                            catch( Exception ex )
                            {
                                HideProgressIndicator();
                                throw new Exception( "Exception encountered during DataRelay.Update()", ex );
                            }
                        }
                        else
                        {
                            HideProgressIndicator();
                            ShowValidationErrors();                            
                        }
                    }
                }
                else
                {
                    HideProgressIndicator();
                    throw new Exception( _errorMessage );  // validation error
                }
            }
            else if( _documentEditorType == DocumentEditorTypes.Offer )
            {
                if( SaveScreenValuesToObject( _offerDataRelay, false ) == true )
                {
                    // if anything has changed
                    if( _offerDataRelay.EditedOfferContentFront.IsDocumentDirty() == true )
                    {
                        // validate the current tab
                        ValidateBeforeSave( _offerDataRelay, false, GetValidationGroupName() );

                        // check cumulative errors
                        if( _validationErrorManager.HasErrors() == false )
                        {
                            try
                            {
                                _offerDataRelay.Update();
                            }
                            catch( Exception ex )
                            {
                                HideProgressIndicator();
                                throw new Exception( "Exception encountered during OfferDataRelay.Update()", ex );
                            }
                         }
                         else
                         {
                             HideProgressIndicator();
                             ShowValidationErrors();
                         }
                    }
                }
                else
                {
                    HideProgressIndicator();
                    throw new Exception( _errorMessage );  // validation error
                }
            }
            else if( _documentEditorType == DocumentEditorTypes.NewContract || _documentEditorType == DocumentEditorTypes.NewContractFromOffer )
            {
                if( SaveScreenValuesToObject( _dataRelay, false ) == true )
                {
                    // if anything has changed
                    if( _dataRelay.EditedDocumentContentFront.IsDocumentDirty() == true )
                    {
                        // validate the current tab
                        ValidateBeforeSave( _dataRelay, false, GetValidationGroupName() );

                        // check cumulative errors
                        if( _validationErrorManager.HasErrors() == false )
                        {
                            try
                            {
                                _dataRelay.Insert();
                            }
                            catch( Exception ex )
                            {
                                HideProgressIndicator();
                                throw new Exception( "Exception encountered during DataRelay.Insert()", ex );
                            }
                        }
                        else
                        {
                            HideProgressIndicator();
                            ShowValidationErrors();
                        }
                    }
                }
                else
                {
                    HideProgressIndicator();
                    throw new Exception( _errorMessage );  // validation error
                }
            }
            else if( _documentEditorType == DocumentEditorTypes.NewOffer )
            {
                if( SaveScreenValuesToObject( _offerDataRelay, false ) == true )
                {
                    // if anything has changed
                    if( _offerDataRelay.EditedOfferContentFront.IsDocumentDirty() == true )
                    {
                        // validate the current tab
                        ValidateBeforeSave( _offerDataRelay, false, GetValidationGroupName() );

                        // check cumulative errors
                        if( _validationErrorManager.HasErrors() == false )
                        {
                            try
                            {
                                _offerDataRelay.Insert();
                            }
                            catch( Exception ex )
                            {
                                HideProgressIndicator();
                                throw new Exception( "Exception encountered during OfferDataRelay.Insert()", ex );
                            }
                        }
                        else
                        {
                            HideProgressIndicator();
                            ShowValidationErrors();
                        }
                    }
                }
                else
                {
                    HideProgressIndicator();
                    throw new Exception( _errorMessage );  // validation error
                }
            }
        }
     
        public void HideProgressIndicator()
        {
            string hideProgressIndicatorScript = "HideProgressIndicator();";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "HideProgressIndicatorScript", hideProgressIndicatorScript, true ); // runs after controls established

            TriggerViewMasterUpdatePanel();
        }

        public void HideProgressIndicator( Page containingPage )
        {
            string hideProgressIndicatorScript = "HideProgressIndicator();";
            ScriptManager.RegisterStartupScript( containingPage, containingPage.GetType(), "HideProgressIndicatorScript", hideProgressIndicatorScript, true ); // runs after controls established

            TriggerViewMasterUpdatePanel();
        }

   

        public void ClearErrors()
        {
            // validation errors
            _errorMessage = "";
            // database errors
            if( _documentEditorType == DocumentEditorTypes.Contract || _documentEditorType == DocumentEditorTypes.NewContract || _documentEditorType == DocumentEditorTypes.NewContractFromOffer )
            {
                _dataRelay.ClearError();
            }
            else if( _documentEditorType == DocumentEditorTypes.Offer || _documentEditorType == DocumentEditorTypes.NewOffer )
            {
                _offerDataRelay.ClearError();
            }
        }

        protected void SetDirtyFlag( string formViewName )
        {            
            if( formViewName.CompareTo( "ContractGeneralContractDatesFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractGeneralContractDatesDirty = true;
            }
            else if( formViewName.CompareTo( "ContractGeneralContractAttributesFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractGeneralContractAttributesDirty = true;
            }
            else if( formViewName.CompareTo( "ContractGeneralContractAssignmentFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractGeneralContractAssignmentDirty = true;
            }
            else if( formViewName.CompareTo( "ContractGeneralParentContractFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractGeneralParentContractDirty = true;
            }
            else if( formViewName.CompareTo( "ContractVendorSocioFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractVendorSocioDirty = true;
            }
            else if( formViewName.CompareTo( "VendorAttributesFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractVendorAttributesDirty = true;
            }
            else if( formViewName.CompareTo( "WarrantyInformationFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsWarrantyInformationDirty = true;
            }
            else if( formViewName.CompareTo( "ReturnedGoodsPolicyFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsReturnedGoodsPolicyInformationDirty = true;
            }
            else if( formViewName.CompareTo( "StateFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsGeographicCoverageDirty = true;
            }
            else if( formViewName.CompareTo( "ContractDetailsAttributesFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractDetailsAttributesDirty = true;
            }
            else if( formViewName.CompareTo( "DiscountFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractDetailsDiscountDirty = true;
            }
            else if( formViewName.CompareTo( "DeliveryFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractDetailsDeliveryDirty = true;
            }
            else if( formViewName.CompareTo( "VendorContractAdministratorFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsVendorContractAdministratorDirty = true;
            }
            else if( formViewName.CompareTo( "VendorAlternateContactFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsVendorAlternateContactDirty = true;
            }
            else if( formViewName.CompareTo( "VendorTechnicalContactFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsVendorTechnicalContactDirty = true;
            }
            else if( formViewName.CompareTo( "VendorEmergencyContactFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsVendorEmergencyContactDirty = true;
            }
            else if( formViewName.CompareTo( "VendorOrderingContactFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsVendorOrderingContactDirty = true;
            }
            else if( formViewName.CompareTo( "VendorSalesContactFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsVendorSalesContactDirty = true;
            }
            else if( formViewName.CompareTo( "VendorBusinessAddressFormView" ) == 0 )  // Contract vendor tab
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsVendorBusinessAddressDirty = true;
            }
            else if( formViewName.CompareTo( "PricelistVerificationFormView" ) == 0 )  // Contract vendor tab
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsPricelistVerificationDirty = true;
            }
            else if( formViewName.CompareTo( "PricelistNotesFormView" ) == 0 )  // Contract vendor tab
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsPricelistNotesDirty = true;
            }
            else if( formViewName.CompareTo( "RebatesHeaderFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsRebateDirty = true;
            }
 
            else if( formViewName.CompareTo( "SBAHeaderFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsSBAHeaderDirty = true;
            }
            else if( formViewName.CompareTo( "SBAPlanDetailsFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsSBAPlanDetailsDirty = true;
            }
            else if( formViewName.CompareTo( "ContractCommentsFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsContractCommentDirty = true;
            }

            else if( formViewName.CompareTo( "CreateContractFormView" ) == 0 )
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsCreateContractFormViewDirty = true;
            }

            else if( formViewName.CompareTo( "VendorPOCFormView" ) == 0 )  // create contract screen
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsVendorPOCFormViewDirty = true;
            }
            else if( formViewName.CompareTo( "VendorAddressFormView" ) == 0 )   // create contract screen
            {
                DataRelay.EditedDocumentContentFront.DirtyFlags.IsVendorAddressFormViewDirty = true;
            }
            else if( formViewName.CompareTo( "OfferAttributesFormView" ) == 0 )
            {
                OfferDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferGeneralOfferAttributesDirty = true;
            }
            else if( formViewName.CompareTo( "OfferAttributesDatesFormView" ) == 0 )
            {
                OfferDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferGeneralOfferDatesDirty = true;
            }
            else if( formViewName.CompareTo( "OfferActionFormView" ) == 0 )   
            {
                OfferDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferActionDirty = true;
            }
            else if( formViewName.CompareTo( "OfferAuditFormView" ) == 0 )
            {
                OfferDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferAuditInformationDirty= true;
            }
            else if( formViewName.CompareTo( "OfferVendorPOCFormView" ) == 0 )
            {
                OfferDataRelay.EditedOfferContentFront.DirtyFlags.IsVendorOfferContactDirty = true;
            }
            else if( formViewName.CompareTo( "OfferVendorAddressFormView" ) == 0 )
            {
                OfferDataRelay.EditedOfferContentFront.DirtyFlags.IsVendorBusinessAddressDirty = true;
            }
            else if( formViewName.CompareTo( "OfferCommentsFormView" ) == 0 )
            {
                OfferDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferCommentDirty = true;
            }
            //$$$ FormViewAddition
        }
        

#region ContractGeneralSINListViewSupport

        protected void LoadScheduleSINs()
        {
            if( Page.Session[ "ScheduleSINDataSource" ] == null )
            {
                _scheduleSINDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.NACCMCommonUser, false );
                _scheduleSINDataSource.ID = "ScheduleSINDataSource";
                _scheduleSINDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _scheduleSINDataSource.SelectCommand = "SelectSINsForSchedule";
                _scheduleSINDataSource.SetEventOwnerName( "BaseDocumentEditorPage" );
                _scheduleSINDataSource.Selected += new SqlDataSourceStatusEventHandler( _scheduleSINDataSource_Selected );

                CreateScheduleSINDataSourceParameters();

                _scheduleSINDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _scheduleSINDataSource.SelectParameters.Add( _scheduleNumberParameter );
                _scheduleNumberParameter.DefaultValue = _currentDocument.ScheduleNumber.ToString();

                Page.Session[ "ScheduleSINDataSource" ] = _scheduleSINDataSource;
            }
            else
            {
                _scheduleSINDataSource = ( DocumentDataSource )Page.Session[ "ScheduleSINDataSource" ];
                _scheduleSINDataSource.RestoreDelegatesAfterDeserialization( this, "BaseDocumentEditorPage" );

                RestoreScheduleSINDataSourceParameters( _scheduleSINDataSource );
            }
        }

        private void CreateScheduleSINDataSourceParameters()
        {
            _scheduleNumberParameter = new Parameter( "ScheduleNumber", TypeCode.Int32 );
        }

        private void RestoreScheduleSINDataSourceParameters( DocumentDataSource scheduleSINDataSource )
        {
            _scheduleNumberParameter = _scheduleSINDataSource.SelectParameters[ "ScheduleNumber" ];
        }


        void _scheduleSINDataSource_Selected( object sender, SqlDataSourceStatusEventArgs e )
        {
            _scheduleSINDataSource.RowsReturned = e.AffectedRows;  // used as quality check when saving changes to contract
        }

        public int ScheduleSINCount
        {
            get
            {
                return ( _scheduleSINDataSource.RowsReturned );
            }
        }

        protected void LoadContractSINs()
        {
            if( Page.Session[ "ContractSINDataSource" ] == null )
            {
                _contractSINDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.NACCMCommonUser, true );
                _contractSINDataSource.ID = "ContractSINDataSource";
                _contractSINDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _contractSINDataSource.SelectCommand = "SelectSINsForContract2";
                _contractSINDataSource.InsertCommand = "InsertSINForContract";
                _contractSINDataSource.UpdateCommand = "UpdateSINForContract";
                _contractSINDataSource.DeleteCommand = "DeleteSINFromContract2";

                _contractSINDataSource.SetEventOwnerName( "BaseDocumentEditorPage" );
                _contractSINDataSource.Inserted += new SqlDataSourceStatusEventHandler( _contractSINDataSource_Inserted );
                _contractSINDataSource.Updated += new SqlDataSourceStatusEventHandler( _contractSINDataSource_Updated );
                _contractSINDataSource.Updating += new SqlDataSourceCommandEventHandler( _contractSINDataSource_Updating );
                _contractSINDataSource.Selected += new SqlDataSourceStatusEventHandler( _contractSINDataSource_Selected );
                CreateContractSINDataSourceParameters();

                _contractSINDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _contractSINDataSource.SelectParameters.Add( _contractNumberParameter );
                _contractNumberParameter.DefaultValue = _currentDocument.ContractNumber;   // "V797P-4681A"
                _contractSINDataSource.SelectParameters.Add( _withAddParameter );
                _withAddParameter.DefaultValue = "false"; // not adding

                _contractSINDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
                _contractSINDataSource.InsertParameters.Add( _contractNumberParameter );
                _contractSINDataSource.InsertParameters.Add( _SINParameter );
                _contractSINDataSource.InsertParameters.Add( _recoverableParameter );
                _contractSINDataSource.InsertParameters.Add( _contractSINIDParameter );

                _contractSINDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
                _contractSINDataSource.UpdateParameters.Add( _contractNumberParameter );
                _contractSINDataSource.UpdateParameters.Add( _SINParameter );
                _contractSINDataSource.UpdateParameters.Add( _recoverableParameter );

                _contractSINDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
                _contractSINDataSource.DeleteParameters.Add( _contractNumberParameter );
                _contractSINDataSource.DeleteParameters.Add( _SINParameter );

                Page.Session[ "ContractSINDataSource" ] = _contractSINDataSource;
            }
            else
            {
                _contractSINDataSource = ( DocumentDataSource )Page.Session[ "ContractSINDataSource" ];
                _contractSINDataSource.RestoreDelegatesAfterDeserialization( this, "BaseDocumentEditorPage" );  // must use same name given in SetEventOwnerName() call prior to setting event handlers
                _contractSINDataSource.RestoreDelegatesAfterDeserialization( this, "ContractGeneral" );   // adding second line since derived class has also created an event handler

                RestoreContractSINDataSourceParameters( _contractSINDataSource );
            }
        }

   
        private void CreateContractSINDataSourceParameters()
        {
            _contractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _SINParameter = new Parameter( "SIN", TypeCode.String );
            _recoverableParameter = new Parameter( "Recoverable", TypeCode.Boolean );
            _withAddParameter = new Parameter( "WithAdd", TypeCode.Boolean );
            _contractSINIDParameter = new Parameter( "ID", TypeCode.Int32 );
            _contractSINIDParameter.Direction = ParameterDirection.Output;
        }

        private void RestoreContractSINDataSourceParameters( DocumentDataSource scheduleSINDataSource )
        {
            // select
            _contractNumberParameter = _contractSINDataSource.SelectParameters[ "ContractNumber" ];
            _withAddParameter = _contractSINDataSource.SelectParameters[ "WithAdd" ];

            // update
            _SINParameter = _contractSINDataSource.UpdateParameters[ "SIN" ];
            _recoverableParameter = _contractSINDataSource.UpdateParameters[ "Recoverable" ];

            // insert
            _contractSINIDParameter = _contractSINDataSource.InsertParameters[ "ID" ];
        }

        protected void _contractSINDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@ID" ].Value != null )
            {
                string contractSINIdString = e.Command.Parameters[ "@ID" ].Value.ToString();

                if( contractSINIdString.Length > 0 )
                {
                    int contractSINId = int.Parse( contractSINIdString );
                    Session[ "LastInsertedContractSINId" ] = contractSINId;
                }
            }
            else
            {
                throw new Exception( "( contract SIN ) ID returned from insert was null. Insert failed." );
            }
        }

        void _contractSINDataSource_Updating( object sender, SqlDataSourceCommandEventArgs e )
        {
            string currentContractNumber = _currentDocument.ContractNumber; // 4681A

            SqlParameter contractNumberParameter = ( SqlParameter )e.Command.Parameters[ "@ContractNumber" ];
            contractNumberParameter.SqlDbType = SqlDbType.NVarChar;
            contractNumberParameter.Size = 50;
            contractNumberParameter.SqlValue = currentContractNumber;   
        }

        protected void _contractSINDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {

        }

        void _contractSINDataSource_Selected( object sender, SqlDataSourceStatusEventArgs e )
        {
            _contractSINDataSource.RowsReturned = e.AffectedRows;  // used as quality check when saving changes to contract
        }



        public int ContractSINCount
        {
            get
            {
                return( _contractSINDataSource.RowsReturned );
            }
        }

       



        #endregion ContractGeneralSINListViewSupport

        #region ContractGeneralAssociatedBPAContractsSupport

        protected void LoadAssociatedBPAContracts()
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            string contractNumber = currentDocument.ContractNumber;

            if( Session[ "AssociatedBPAContractsDataSource" ] == null )
            {

                AssociatedBPAContractsDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, true );

                AssociatedBPAContractsDataSource.ID = "AssociatedBPAContractsDataSource";
                AssociatedBPAContractsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                AssociatedBPAContractsDataSource.SelectCommand = "SelectAssociatedBPAContracts";

                CreateAssociatedBPAContractsDataSourceParameters();

                AssociatedBPAContractsDataSource.SetEventOwnerName( "BaseDocumentEditorPage" );

                AssociatedBPAContractsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                AssociatedBPAContractsDataSource.SelectParameters.Add( _userLoginParameter );
                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;

                AssociatedBPAContractsDataSource.SelectParameters.Add( _associatedBPAContractsContractNumberParameter );
                _associatedBPAContractsContractNumberParameter.DefaultValue = contractNumber;

                // save to session
                Session[ "AssociatedBPAContractsDataSource" ] = AssociatedBPAContractsDataSource;

            }
            else
            {
                AssociatedBPAContractsDataSource = ( DocumentDataSource )Session[ "AssociatedBPAContractsDataSource" ];
                AssociatedBPAContractsDataSource.RestoreDelegatesAfterDeserialization( this, "BaseDocumentEditorPage" );
                RestoreAssociatedBPAContractsDataSourceParameters( AssociatedBPAContractsDataSource );               
            }
        }

        protected void CreateAssociatedBPAContractsDataSourceParameters()
        {
            // select parms
            _associatedBPAContractsContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );          
        }

        protected void RestoreAssociatedBPAContractsDataSourceParameters( DocumentDataSource associatedBPAContractsDataSource )
        {
            // select parms
            _associatedBPAContractsContractNumberParameter = associatedBPAContractsDataSource.SelectParameters[ "ContractNumber" ];
            _userLoginParameter = associatedBPAContractsDataSource.SelectParameters[ "UserLogin" ];
        }
        
#endregion ContractGeneralAssociatedBPAContractsSupport

        #region ContractDetailSolicitationSupport

        protected void LoadSolicitations()
        {
            if( Page.Session[ "SolicitationDataSource" ] == null )
            {
                _solicitationDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.NACCMCommonUser, false );
                _solicitationDataSource.ID = "SolicitationDataSource";
                _solicitationDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _solicitationDataSource.SelectCommand = "SelectSolicitations";

                CreateSolicitationDataSourceParameters();

                _solicitationDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _solicitationDataSource.SelectParameters.Add( _solicitationActiveParameter );
                _solicitationActiveParameter.DefaultValue = "A";

                Page.Session[ "SolicitationDataSource" ] = _solicitationDataSource;
            }
            else
            {
                _solicitationDataSource = ( DocumentDataSource )Page.Session[ "SolicitationDataSource" ];
                _solicitationDataSource.RestoreDelegatesAfterDeserialization( this, "BaseDocumentEditorPage" );

                RestoreSolicitationDataSourceParameters( _solicitationDataSource );
            }
        }

        private void CreateSolicitationDataSourceParameters()
        {
            _solicitationActiveParameter = new Parameter( "Active", TypeCode.Char );
        }

        private void RestoreSolicitationDataSourceParameters( DocumentDataSource scheduleSINDataSource )
        {
            _solicitationActiveParameter = _solicitationDataSource.SelectParameters[ "Active" ];
        }

#endregion ContractDetailSolicitationSupport


#region PointsOfContactStateSupport

        //private void LoadStates( DropDownList stateDropDownList, string selectedAbbreviation )
        //{
        //    bool bSuccess = true;
        //    DataSet dsStateCodes = null;
        //    ContractDB contractDB = null;

        //    if( stateDropDownList != null )
        //    {
        //        if( Session[ "PointsOfContactStateCodeDataSet" ] != null )
        //        {
        //            dsStateCodes = ( DataSet )Session[ "PointsOfContactStateCodeDataSet" ];
        //        }
        //        else
        //        {
        //            contractDB = ( ContractDB )Session[ "ContractDB" ];

        //            bSuccess = contractDB.SelectStateCodes( ref dsStateCodes );
        //        }

        //        if( bSuccess == true )
        //        {
        //            stateDropDownList.ClearSelection();
        //            stateDropDownList.Items.Clear();
        //            stateDropDownList.DataSource = dsStateCodes;
        //            stateDropDownList.DataMember = "StateCodesTable";
        //            stateDropDownList.DataTextField = "StateAbbreviation";
        //            stateDropDownList.DataValueField = "StateName";
        //            stateDropDownList.DataBind();

        //            stateDropDownList.SelectedItem.Text = selectedAbbreviation;

        //            Session[ "PointsOfContactStateCodeDataSet" ] = dsStateCodes;
        //        }
        //        else
        //        {
        //            ShowException( new Exception( contractDB.ErrorMessage ) );
        //        }
        //    }
        //}

        public static string PointsOfContactUSAStateCodeCacheName = "PointsOfContactUSAStateCodeDataSet";
        public static string PointsOfContactCanadaStateCodeCacheName = "PointsOfContactCanadaStateCodeDataSet";

        // also used by offers and contract creation
        public void LoadStates( int vendorCountryId )
        {
            bool bSuccess = false;
            DataSet dsStateCodes = null;
            ContractDB contractDB = null;

            if( vendorCountryId == CMGlobals.COUNTRYIDUSA )
            {
                if( Cache[ PointsOfContactUSAStateCodeCacheName ] != null )
                {
                    dsStateCodes = ( DataSet )Cache[ PointsOfContactUSAStateCodeCacheName ];

                    PointsOfContactStateCodeDataSet = dsStateCodes;
                }
                else
                {
                    contractDB = ( ContractDB )Session[ "ContractDB" ];

                    bSuccess = contractDB.SelectStateCodes( ref dsStateCodes, vendorCountryId );

                    if( bSuccess == true )
                    {
                        CMGlobals.CreateDataSetCache( this.Page, PointsOfContactUSAStateCodeCacheName, dsStateCodes );

                        PointsOfContactStateCodeDataSet = dsStateCodes;
                    }
                    else
                    {
                        ShowException( new Exception( contractDB.ErrorMessage ) );
                    }
                }
            }
            else if( vendorCountryId == CMGlobals.COUNTRYIDCANADA )
            {
                if( Cache[ PointsOfContactCanadaStateCodeCacheName ] != null )
                {
                    dsStateCodes = ( DataSet )Cache[ PointsOfContactCanadaStateCodeCacheName ];

                    PointsOfContactStateCodeDataSet = dsStateCodes;
                }
                else
                {
                    contractDB = ( ContractDB )Session[ "ContractDB" ];

                    bSuccess = contractDB.SelectStateCodes( ref dsStateCodes, vendorCountryId );

                    if( bSuccess == true )
                    {
                        CMGlobals.CreateDataSetCache( this.Page, PointsOfContactCanadaStateCodeCacheName, dsStateCodes );

                        PointsOfContactStateCodeDataSet = dsStateCodes;
                    }
                    else
                    {
                        ShowException( new Exception( contractDB.ErrorMessage ) );
                    }
                }

            }
            else // some other country
            {
                contractDB = ( ContractDB )Session[ "ContractDB" ];

                bSuccess = contractDB.SelectStateCodes( ref dsStateCodes, vendorCountryId );

                if( bSuccess == true )
                {
                    PointsOfContactStateCodeDataSet = dsStateCodes;
                }
                else
                {
                    ShowException( new Exception( contractDB.ErrorMessage ) );
                }
            }    
        }

        #endregion PointsOfContactStateSupport

        #region PointsOfContactCountrySupport

        public static string PointsOfContactVendorCountriesCacheName = "PointsOfContactVendorCountriesDataSet";

        // also used by offers and contract creation
        public void LoadCountries()
        {
            bool bSuccess = false;
            DataSet dsCountries = null;
            ContractDB contractDB = null;

            if( Cache[ PointsOfContactVendorCountriesCacheName ] != null )
            {
                dsCountries = ( DataSet )Cache[ PointsOfContactVendorCountriesCacheName ];

                PointsOfContactVendorCountriesDataSet = dsCountries;
            }
            else
            {
                contractDB = ( ContractDB )Session[ "ContractDB" ];

                bSuccess = contractDB.SelectCountries( ref dsCountries );

                if( bSuccess == true )
                {
                    CMGlobals.CreateDataSetCache( this.Page, PointsOfContactVendorCountriesCacheName, dsCountries );

                    PointsOfContactVendorCountriesDataSet = dsCountries;
                }
                else
                {
                    ShowException( new Exception( contractDB.ErrorMessage ) );
                }
            }
        }

        #endregion PointsOfContactCountrySupport

        #region RebateSupport



        protected void LoadContractRebates()
        {

            bool bSuccess;
            ContractDB contractDB;

            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            string contractNumber = currentDocument.ContractNumber;

            if( Session[ "RebateDataSource" ] == null )
            {

                RebateDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, false );

                RebateDataSource.ID = "RebateDataSource";
                RebateDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                RebateDataSource.SelectCommand = "SelectRebatesForContract";

                RebateDataSource.UpdateCommand = "UpdateContractRebate";

                RebateDataSource.InsertCommand = "InsertContractRebate";

                RebateDataSource.DeleteCommand = "DeleteContractRebate";

                RebateDataSource.SetEventOwnerName( "RebateDataSource" );              

                RebateDataSource.Updated += new SqlDataSourceStatusEventHandler( RebateDataSource_Updated );
                RebateDataSource.Inserted += new SqlDataSourceStatusEventHandler( RebateDataSource_Inserted );

                CreateRebateDataSourceParameters();

                RebateDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                RebateDataSource.SelectParameters.Add( _rebateContractNumberParameter );
                _rebateContractNumberParameter.DefaultValue = contractNumber;
                RebateDataSource.SelectParameters.Add( _userLoginParameter );
                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;
                _withAddRebateParameter.DefaultValue = "false"; // not adding
                RebateDataSource.SelectParameters.Add( _withAddRebateParameter );

                RebateDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;

                RebateDataSource.UpdateParameters.Add( _rebateContractNumberParameter );
                RebateDataSource.UpdateParameters.Add( _userLoginParameter );

                RebateDataSource.UpdateParameters.Add( _standardRebateTermIdParameter );
                RebateDataSource.UpdateParameters.Add( _rebateIdParameter );
                RebateDataSource.UpdateParameters.Add( _startQuarterIdParameter );
                RebateDataSource.UpdateParameters.Add( _endQuarterIdParameter );
                RebateDataSource.UpdateParameters.Add( _rebatePercentOfSalesParameter );
                RebateDataSource.UpdateParameters.Add( _rebateThresholdParameter );
                RebateDataSource.UpdateParameters.Add( _amountReceivedParameter );
                RebateDataSource.UpdateParameters.Add( _isCustomParameter );
                RebateDataSource.UpdateParameters.Add( _rebateClauseParameter );
                RebateDataSource.UpdateParameters.Add( _customRebateIdParameter );
                RebateDataSource.UpdateParameters.Add( _rebateTermIdParameter );
                RebateDataSource.UpdateParameters.Add( _rebateCustomStartDateParameter );

                RebateDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;

                RebateDataSource.InsertParameters.Add( _userLoginParameter );
                RebateDataSource.InsertParameters.Add( _rebateContractNumberParameter );
                RebateDataSource.InsertParameters.Add( _startQuarterIdParameter );
                RebateDataSource.InsertParameters.Add( _endQuarterIdParameter );
                RebateDataSource.InsertParameters.Add( _rebatePercentOfSalesParameter );
                RebateDataSource.InsertParameters.Add( _rebateThresholdParameter );
                RebateDataSource.InsertParameters.Add( _amountReceivedParameter );
                RebateDataSource.InsertParameters.Add( _isCustomParameter );
                RebateDataSource.InsertParameters.Add( _rebateClauseParameter );
                RebateDataSource.InsertParameters.Add( _standardRebateTermIdParameter );
                RebateDataSource.InsertParameters.Add( _rebateCustomStartDateParameter );

                RebateDataSource.InsertParameters.Add( _rebateIdForInsertParameter );

                RebateDataSource.InsertParameters.Add( _customRebateIdParameter );
                RebateDataSource.InsertParameters.Add( _rebateTermIdParameter );

                RebateDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;

                RebateDataSource.DeleteParameters.Add( _userLoginParameter );
                RebateDataSource.DeleteParameters.Add( _rebateContractNumberParameter );
                RebateDataSource.DeleteParameters.Add( _rebateIdParameter );

                // save to session
                Session[ "RebateDataSource" ] = RebateDataSource;

            }
            else
            {
                RebateDataSource = ( DocumentDataSource )Session[ "RebateDataSource" ];
                RebateDataSource.RestoreDelegatesAfterDeserialization( this, "RebateDataSource" );
                RestoreRebateDataSourceParameters( RebateDataSource );                
            }

            if( Session[ "RebateDateDataSource" ] == null )
            {
                RebateDateDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, false );

                RebateDateDataSource.ID = "RebateDateDataSource";
                RebateDateDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                RebateDateDataSource.SelectCommand = "SelectYearQuartersForDateRange";
                RebateDateDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;


                _rebateDateSelectStartDate = new Parameter( "StartDate", TypeCode.DateTime );
                _rebateDateSelectEndDate = new Parameter( "EndDate", TypeCode.DateTime );
                RebateDateDataSource.SelectParameters.Add( _userLoginParameter );
                RebateDateDataSource.SelectParameters.Add( _rebateDateSelectStartDate );
                RebateDateDataSource.SelectParameters.Add( _rebateDateSelectEndDate );

                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;
                _rebateDateSelectStartDate.DefaultValue = currentDocument.AwardDate.ToString();
                _rebateDateSelectEndDate.DefaultValue = currentDocument.ExpirationDate.ToString();


                // save to session
                Session[ "RebateDateDataSource" ] = RebateDateDataSource;

            }
            else
            {
                RebateDateDataSource = ( DocumentDataSource )Session[ "RebateDateDataSource" ];

                _userLoginParameter = RebateDateDataSource.SelectParameters[ "UserLogin" ];
                _rebateDateSelectStartDate = RebateDateDataSource.SelectParameters[ "StartDate" ];
                _rebateDateSelectEndDate = RebateDateDataSource.SelectParameters[ "EndDate" ];

            }

            // dates are bound during grid row binding

            if( Session[ "StandardClauseNameDataSource" ] == null )
            {
                RebateStandardClauseNameDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, false );

                RebateStandardClauseNameDataSource.ID = "StandardClauseNameDataSource";
                RebateStandardClauseNameDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                RebateStandardClauseNameDataSource.SelectCommand = "SelectStandardRebateTerms";
                RebateStandardClauseNameDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                // values set above in rebate selection
                RebateStandardClauseNameDataSource.SelectParameters.Add( _userLoginParameter );

                _clauseTypeParameter = new Parameter( "ClauseType", TypeCode.String );
                RebateStandardClauseNameDataSource.SelectParameters.Add( _clauseTypeParameter );
                _clauseTypeParameter.DefaultValue = "A";  // All

                // save to session
                Session[ "StandardClauseNameDataSource" ] = RebateStandardClauseNameDataSource;

            }
            else
            {
                RebateStandardClauseNameDataSource = ( DocumentDataSource )Session[ "StandardClauseNameDataSource" ];

                // restore parameters ( not already restored )
                _clauseTypeParameter = RebateStandardClauseNameDataSource.SelectParameters[ "ClauseType" ];

            }

            // clause names are bound during grid row binding

            // retrieve the standard clauses in a dataset
            if( Session[ "StandardClauseNameDataSet" ] == null )
            {
                contractDB = ( ContractDB )Session[ "ContractDB" ];

                bSuccess = contractDB.SelectStandardRebateTerms( ref _dsRebateStandardClauses, "A" );

                if( bSuccess == true )
                {
                    Session[ "StandardClauseNameDataSet" ] = _dsRebateStandardClauses;
                }
            }
            else
            {
                _dsRebateStandardClauses = ( DataSet )Session[ "StandardClauseNameDataSet" ];
            }

        }


        protected void CreateRebateDataSourceParameters()
        {
            // select parms
            _rebateContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );
            _withAddRebateParameter = new Parameter( "WithAdd", TypeCode.Boolean );

            // update parms
            _standardRebateTermIdParameter = new Parameter( "StandardRebateTermId", TypeCode.Int32 );
            _rebateIdParameter = new Parameter( "RebateId", TypeCode.Int32 );
            _startQuarterIdParameter = new Parameter( "StartQuarterId", TypeCode.Int32 );
            _endQuarterIdParameter = new Parameter( "EndQuarterId", TypeCode.Int32 );
            _rebatePercentOfSalesParameter = new Parameter( "RebatePercentOfSales", TypeCode.Decimal );
            _rebateThresholdParameter = new Parameter( "RebateThreshold", TypeCode.Decimal );
            _amountReceivedParameter = new Parameter( "AmountReceived", TypeCode.Decimal );
            _isCustomParameter = new Parameter( "IsCustom", TypeCode.Boolean );
            _rebateClauseParameter = new Parameter( "RebateClause", TypeCode.String );
            _rebateCustomStartDateParameter = new Parameter( "CustomStartDate", TypeCode.String );

            _customRebateIdParameter = new Parameter( "CustomRebateId", TypeCode.Int32 );
            _customRebateIdParameter.Direction = ParameterDirection.Output;
            _rebateTermIdParameter = new Parameter( "RebateTermId", TypeCode.Int32 );
            _rebateTermIdParameter.Direction = ParameterDirection.Output;

            // insert parameters
            _rebateIdForInsertParameter = new Parameter( "RebateId", TypeCode.Int32 );
            _rebateIdForInsertParameter.Direction = ParameterDirection.Output;


        }

        protected void RestoreRebateDataSourceParameters( DocumentDataSource rebateDataSource )
        {
            // select parms
            _rebateContractNumberParameter = rebateDataSource.SelectParameters[ "ContractNumber" ];
            _userLoginParameter = rebateDataSource.SelectParameters[ "UserLogin" ];
            _withAddRebateParameter = rebateDataSource.SelectParameters[ "WithAdd" ];

            // update parms
            _standardRebateTermIdParameter = rebateDataSource.UpdateParameters[ "StandardRebateTermId" ];
            _rebateIdParameter = rebateDataSource.UpdateParameters[ "RebateId" ];
            _startQuarterIdParameter = rebateDataSource.UpdateParameters[ "StartQuarterId" ];
            _endQuarterIdParameter = rebateDataSource.UpdateParameters[ "EndQuarterId" ];
            _rebatePercentOfSalesParameter = rebateDataSource.UpdateParameters[ "RebatePercentOfSales" ];
            _rebateThresholdParameter = rebateDataSource.UpdateParameters[ "RebateThreshold" ];
            _amountReceivedParameter = rebateDataSource.UpdateParameters[ "AmountReceived" ];
            _isCustomParameter = rebateDataSource.UpdateParameters[ "IsCustom" ];
            _rebateClauseParameter = rebateDataSource.UpdateParameters[ "RebateClause" ];
            _customRebateIdParameter = rebateDataSource.UpdateParameters[ "CustomRebateId" ];
            _rebateTermIdParameter = rebateDataSource.UpdateParameters[ "RebateTermId" ];
            _rebateCustomStartDateParameter = rebateDataSource.UpdateParameters[ "CustomStartDate" ];

            // insert parameters
            _rebateIdForInsertParameter = rebateDataSource.InsertParameters[ "RebateId" ];

        }
        public void RebateDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string rebateIdString = e.Command.Parameters[ "@RebateId" ].Value.ToString();

            if( rebateIdString.Length > 0 )
            {
                int rebateId;
                rebateId = int.Parse( rebateIdString );
                Session[ "LastUpdatedRebateId" ] = rebateId;
            }
        }

        public void RebateDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@RebateId" ].Value != null )
            {
                string rebateIdString = e.Command.Parameters[ "@RebateId" ].Value.ToString();

                if( rebateIdString.Length > 0 )
                {
                    int rebateId;
                    rebateId = int.Parse( rebateIdString );
                    Session[ "LastInsertedRebateId" ] = rebateId;
                }
            }
            else
            {
                Exception insertException;
                insertException = e.Exception;

                if( insertException != null )
                {
                    throw new Exception( String.Format( "RebateId returned from insert was null. Insert failed. {0}", insertException.Message ) );
                }
                else
                {
                    throw new Exception( "RebateId returned from insert was null. Insert failed." );
                }
            }

        }




#endregion RebateSupport


#region SalesSupport

        protected void LoadContractSalesSummary()
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            string contractNumber = currentDocument.ContractNumber;

            if( Session[ "SalesSummaryDataSource" ] == null )
            {

                SalesSummaryDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, false );

                SalesSummaryDataSource.ID = "SalesSummaryDataSource";
                SalesSummaryDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                SalesSummaryDataSource.SelectCommand = "SelectSalesVarianceSummaryForContract";

                CreateSalesSummaryDataSourceParameters();

                SalesSummaryDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                SalesSummaryDataSource.SelectParameters.Add( _salesContractNumberParameter );
                _salesContractNumberParameter.DefaultValue = contractNumber;
                SalesSummaryDataSource.SelectParameters.Add( _userLoginParameter );
                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;
 
                // save to session
                Session[ "SalesSummaryDataSource" ] = SalesSummaryDataSource;

            }
            else
            {
                SalesSummaryDataSource = ( DocumentDataSource )Session[ "SalesSummaryDataSource" ];
                SalesSummaryDataSource.RestoreDelegatesAfterDeserialization( this, "BaseDocumentEditorPage" );
                RestoreSalesSummaryDataSourceParameters( SalesSummaryDataSource );
                
            }

            //if( Session[ "YearQuarterDataSource" ] == null )
            //{
            //    YearQuarterDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, false );

            //    YearQuarterDataSource.ID = "YearQuarterDataSource";
            //    YearQuarterDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

            //    YearQuarterDataSource.SelectCommand = "SelectYearQuartersForDateRange";
            //    YearQuarterDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;


            //    _rebateDateSelectStartDate = new Parameter( "StartDate", TypeCode.DateTime );
            //    _rebateDateSelectEndDate = new Parameter( "EndDate", TypeCode.DateTime );
            //    YearQuarterDataSource.SelectParameters.Add( _userLoginParameter );
            //    YearQuarterDataSource.SelectParameters.Add( _rebateDateSelectStartDate );
            //    YearQuarterDataSource.SelectParameters.Add( _rebateDateSelectEndDate );

            //    _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;
            //    _rebateDateSelectStartDate.DefaultValue = currentDocument.AwardDate.ToString();
            //    _rebateDateSelectEndDate.DefaultValue = currentDocument.ExpirationDate.ToString();


            //     save to session
            //    Session[ "YearQuarterDataSource" ] = YearQuarterDataSource;

            //}
            //else
            //{
            //    YearQuarterDataSource = ( DocumentDataSource )Session[ "YearQuarterDataSource" ];

            //    _userLoginParameter = YearQuarterDataSource.SelectParameters[ "UserLogin" ];
            //    _rebateDateSelectStartDate = YearQuarterDataSource.SelectParameters[ "StartDate" ];
            //    _rebateDateSelectEndDate = YearQuarterDataSource.SelectParameters[ "EndDate" ];

            //}
        }


        protected void CreateSalesSummaryDataSourceParameters()
        {
            // select parms
            _salesContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );
        }

        protected void RestoreSalesSummaryDataSourceParameters( DocumentDataSource salesSummaryDataSource )
        {
            // select parms
            _salesContractNumberParameter = salesSummaryDataSource.SelectParameters[ "ContractNumber" ];
            _userLoginParameter = salesSummaryDataSource.SelectParameters[ "UserLogin" ];
        }

#endregion SalesSupport


#region CheckSupport



        protected void LoadContractChecks()
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            string contractNumber = currentDocument.ContractNumber;

            if( Session[ "CheckDataSource" ] == null )
            {

                CheckDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, true );

                CheckDataSource.ID = "CheckDataSource";
                CheckDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                CheckDataSource.SelectCommand = "SelectContractChecks";

                CheckDataSource.UpdateCommand = "UpdateContractCheck";

                CheckDataSource.InsertCommand = "InsertContractCheck";

                CheckDataSource.DeleteCommand = "DeleteContractCheck";

                CheckDataSource.SetEventOwnerName( "CheckDataSource" );

                CheckDataSource.Updated += new SqlDataSourceStatusEventHandler( CheckDataSource_Updated );
                CheckDataSource.Inserted += new SqlDataSourceStatusEventHandler( CheckDataSource_Inserted );

                CreateCheckDataSourceParameters();

                CheckDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                CheckDataSource.SelectParameters.Add( _checkContractNumberParameter );
                _checkContractNumberParameter.DefaultValue = contractNumber;
                CheckDataSource.SelectParameters.Add( _userLoginParameter );
                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;
                _withAddCheckParameter.DefaultValue = "false"; // not adding
                CheckDataSource.SelectParameters.Add( _withAddCheckParameter );

                CheckDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;

                CheckDataSource.UpdateParameters.Add( _checkContractNumberParameter );
                CheckDataSource.UpdateParameters.Add( _checkIdParameter );
                CheckDataSource.UpdateParameters.Add( _checkQuarterIdParameter );
                CheckDataSource.UpdateParameters.Add( _checkAmountParameter );
                CheckDataSource.UpdateParameters.Add( _checkNumberParameter );
                CheckDataSource.UpdateParameters.Add( _checkDepositNumberParameter );
                CheckDataSource.UpdateParameters.Add( _checkDateReceivedParameter );
                CheckDataSource.UpdateParameters.Add( _checkCommentsParameter );

                CheckDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;

                CheckDataSource.InsertParameters.Add( _checkContractNumberParameter );
                CheckDataSource.InsertParameters.Add( _checkQuarterIdParameter );
                CheckDataSource.InsertParameters.Add( _checkAmountParameter );
                CheckDataSource.InsertParameters.Add( _checkNumberParameter );
                CheckDataSource.InsertParameters.Add( _checkDepositNumberParameter );
                CheckDataSource.InsertParameters.Add( _checkDateReceivedParameter );
                CheckDataSource.InsertParameters.Add( _checkCommentsParameter );

                CheckDataSource.InsertParameters.Add( _checkIdForInsertParameter );

                CheckDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;

                CheckDataSource.DeleteParameters.Add( _checkContractNumberParameter );
                CheckDataSource.DeleteParameters.Add( _checkIdParameter );

                // save to session
                Session[ "CheckDataSource" ] = CheckDataSource;

            }
            else
            {
                CheckDataSource = ( DocumentDataSource )Session[ "CheckDataSource" ];
                CheckDataSource.RestoreDelegatesAfterDeserialization( this, "CheckDataSource" );
                RestoreCheckDataSourceParameters( CheckDataSource );                
            }

            if( Session[ "CheckDateDataSource" ] == null )
            {
                CheckDateDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, false );

                CheckDateDataSource.ID = "CheckDateDataSource";
                CheckDateDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                CheckDateDataSource.SelectCommand = "SelectYearQuartersForEditContractChecks";
                CheckDateDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
           
                CheckDateDataSource.SelectParameters.Add( _checkContractNumberParameter ); // value set above

                // save to session
                Session[ "CheckDateDataSource" ] = CheckDateDataSource;

            }
            else
            {
                CheckDateDataSource = ( DocumentDataSource )Session[ "CheckDateDataSource" ];
                // note contract number parameter restored above
            }

            // dates are bound during grid row binding
        }


         protected void CreateCheckDataSourceParameters()
        {
            // select parms
            _checkContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );
            _withAddCheckParameter = new Parameter( "WithAdd", TypeCode.Boolean );

            // update parms
            _checkIdParameter = new Parameter( "CheckId", TypeCode.Int32 );
            _checkQuarterIdParameter = new Parameter( "QuarterId", TypeCode.Int32 );
            _checkAmountParameter = new Parameter( "CheckAmount", TypeCode.Decimal );
            _checkNumberParameter = new Parameter( "CheckNumber", TypeCode.String );
            _checkDepositNumberParameter = new Parameter( "DepositNumber", TypeCode.String ); 
          
            _checkDateReceivedParameter = new Parameter( "DateReceived", TypeCode.String );
            _checkCommentsParameter = new Parameter( "CheckComments", TypeCode.String );

            // insert parameters
            _checkIdForInsertParameter = new Parameter( "CheckId", TypeCode.Int32 );
            _checkIdForInsertParameter.Direction = ParameterDirection.Output;
        }

        protected void RestoreCheckDataSourceParameters( DocumentDataSource checkDataSource )
        {
            // select parms
            _checkContractNumberParameter = checkDataSource.SelectParameters[ "ContractNumber" ];
            _userLoginParameter = checkDataSource.SelectParameters[ "UserLogin" ];
            _withAddCheckParameter = checkDataSource.SelectParameters[ "WithAdd" ];

            // update parms
            _checkIdParameter = checkDataSource.UpdateParameters[ "CheckId" ];
            _checkQuarterIdParameter = checkDataSource.UpdateParameters[ "QuarterId" ];
            _checkAmountParameter = checkDataSource.UpdateParameters[ "CheckAmount" ];
            _checkNumberParameter = checkDataSource.UpdateParameters[ "CheckNumber" ];
            _checkDepositNumberParameter = checkDataSource.UpdateParameters[ "DepositNumber" ];

            _checkDateReceivedParameter = checkDataSource.UpdateParameters[ "DateReceived" ];
            _checkCommentsParameter = checkDataSource.UpdateParameters[ "CheckComments" ];           

            // insert parameters
            _checkIdForInsertParameter = checkDataSource.InsertParameters[ "CheckId" ];

        }

        public void CheckDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string checkIdString = e.Command.Parameters[ "@CheckId" ].Value.ToString();

            if( checkIdString.Length > 0 )
            {
                int checkId;
                checkId = int.Parse( checkIdString );
                Session[ "LastUpdatedCheckId" ] = checkId;
            }
        }

        public void CheckDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@CheckId" ].Value != null )
            {
                string checkIdString = e.Command.Parameters[ "@CheckId" ].Value.ToString();

                if( checkIdString.Length > 0 )
                {
                    int checkId;
                    checkId = int.Parse( checkIdString );
                    Session[ "LastInsertedCheckId" ] = checkId;
                }
            }
            else
            {
                Exception insertException;
                insertException = e.Exception;

                if( insertException != null )
                {
                    throw new Exception( String.Format( "CheckId returned from insert was null. Insert failed. {0}", insertException.Message ) );
                }
                else
                {
                    throw new Exception( "CheckId returned from insert was null. Insert failed." );
                }
            }

        }

        #endregion CheckSupport


        
#region CheckSupport2



        protected void LoadContractChecks2()
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            string contractNumber = currentDocument.ContractNumber;
            int contractId = currentDocument.ContractId;

            if( Session[ "CheckDataSource" ] == null )
            {

                CheckDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, true );

                CheckDataSource.ID = "CheckDataSource";
                CheckDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                CheckDataSource.SelectCommand = "SelectContractChecks2";

                CheckDataSource.UpdateCommand = "UpdateContractCheck2";

                CheckDataSource.InsertCommand = "InsertContractCheck2";

                CheckDataSource.DeleteCommand = "DeleteContractCheck2";

                CheckDataSource.SetEventOwnerName( "CheckDataSource" );

                CheckDataSource.Updated += new SqlDataSourceStatusEventHandler( CheckDataSource_Updated2 );
                CheckDataSource.Inserted += new SqlDataSourceStatusEventHandler( CheckDataSource_Inserted2 );

                CreateCheckDataSourceParameters2();

                CheckDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                CheckDataSource.SelectParameters.Add( _checkContractNumberParameter );
                _checkContractNumberParameter.DefaultValue = contractNumber;
                CheckDataSource.SelectParameters.Add( _checkContractIdParameter );
                _checkContractIdParameter.DefaultValue = contractId.ToString();
                CheckDataSource.SelectParameters.Add( _userLoginParameter );
                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;
                _withAddCheckParameter.DefaultValue = "false"; // not adding
                CheckDataSource.SelectParameters.Add( _withAddCheckParameter );

                CheckDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;

                CheckDataSource.UpdateParameters.Add( _checkContractNumberParameter );
                CheckDataSource.UpdateParameters.Add( _checkContractIdParameter );
                CheckDataSource.UpdateParameters.Add( _checkIdParameter );
                CheckDataSource.UpdateParameters.Add( _checkQuarterIdParameter );
                CheckDataSource.UpdateParameters.Add( _checkAmountParameter );
                CheckDataSource.UpdateParameters.Add( _checkNumberParameter );
                CheckDataSource.UpdateParameters.Add( _checkDepositTicketNumberParameter );
                CheckDataSource.UpdateParameters.Add( _checkDateReceivedParameter );
                CheckDataSource.UpdateParameters.Add( _checkSettlementDateParameter );
                CheckDataSource.UpdateParameters.Add( _checkCommentsParameter );

                CheckDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;

                CheckDataSource.InsertParameters.Add( _checkContractNumberParameter );
                CheckDataSource.InsertParameters.Add( _checkContractIdParameter );
                CheckDataSource.InsertParameters.Add( _checkQuarterIdParameter );
                CheckDataSource.InsertParameters.Add( _checkAmountParameter );
                CheckDataSource.InsertParameters.Add( _checkNumberParameter );
                CheckDataSource.InsertParameters.Add( _checkDepositTicketNumberParameter );
                CheckDataSource.InsertParameters.Add( _checkDateReceivedParameter );
                CheckDataSource.InsertParameters.Add( _checkSettlementDateParameter );
                CheckDataSource.InsertParameters.Add( _checkCommentsParameter );

                CheckDataSource.InsertParameters.Add( _checkIdForInsertParameter );

                CheckDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;

                CheckDataSource.DeleteParameters.Add( _checkContractNumberParameter );
                CheckDataSource.DeleteParameters.Add( _checkContractIdParameter );
                CheckDataSource.DeleteParameters.Add( _checkIdParameter );

                // save to session
                Session[ "CheckDataSource" ] = CheckDataSource;

            }
            else
            {
                CheckDataSource = ( DocumentDataSource )Session[ "CheckDataSource" ];
                CheckDataSource.RestoreDelegatesAfterDeserialization( this, "CheckDataSource" );
                RestoreCheckDataSourceParameters2( CheckDataSource );                
            }

            if( Session[ "CheckDateDataSource" ] == null )
            {
                CheckDateDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, false );

                CheckDateDataSource.ID = "CheckDateDataSource";
                CheckDateDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                CheckDateDataSource.SelectCommand = "SelectYearQuartersForEditContractChecks";
                CheckDateDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
           
                CheckDateDataSource.SelectParameters.Add( _checkContractNumberParameter ); // value set above

                // save to session
                Session[ "CheckDateDataSource" ] = CheckDateDataSource;

            }
            else
            {
                CheckDateDataSource = ( DocumentDataSource )Session[ "CheckDateDataSource" ];
                // note contract number parameter restored above
            }

            // dates are bound during grid row binding
        }

        
        protected void CreateCheckDataSourceParameters2()
        {
            // select parms
            _checkContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _checkContractIdParameter = new Parameter( "ContractId", TypeCode.Int32 );
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );
            _withAddCheckParameter = new Parameter( "WithAdd", TypeCode.Boolean );

            // update parms
            _checkIdParameter = new Parameter( "CheckId", TypeCode.Int32 );
            _checkQuarterIdParameter = new Parameter( "QuarterId", TypeCode.Int32 );
            _checkAmountParameter = new Parameter( "CheckAmount", TypeCode.Decimal );
            _checkNumberParameter = new Parameter( "CheckNumber", TypeCode.String );
            _checkDepositTicketNumberParameter = new Parameter( "DepositTicketNumber", TypeCode.String ); 
          
            _checkDateReceivedParameter = new Parameter( "DateReceived", TypeCode.String );
            _checkSettlementDateParameter = new Parameter( "SettlementDate", TypeCode.String );
            _checkCommentsParameter = new Parameter( "CheckComments", TypeCode.String );

            // insert parameters
            _checkIdForInsertParameter = new Parameter( "CheckId", TypeCode.Int32 );
            _checkIdForInsertParameter.Direction = ParameterDirection.Output;
        }

        protected void RestoreCheckDataSourceParameters2( DocumentDataSource checkDataSource )
        {
            // select parms
            _checkContractNumberParameter = checkDataSource.SelectParameters[ "ContractNumber" ];
            _checkContractIdParameter =  checkDataSource.SelectParameters[ "ContractId" ];
            _userLoginParameter = checkDataSource.SelectParameters[ "UserLogin" ];
            _withAddCheckParameter = checkDataSource.SelectParameters[ "WithAdd" ];

            // update parms
            _checkIdParameter = checkDataSource.UpdateParameters[ "CheckId" ];
            _checkQuarterIdParameter = checkDataSource.UpdateParameters[ "QuarterId" ];
            _checkAmountParameter = checkDataSource.UpdateParameters[ "CheckAmount" ];
            _checkNumberParameter = checkDataSource.UpdateParameters[ "CheckNumber" ];
            _checkDepositTicketNumberParameter = checkDataSource.UpdateParameters[ "DepositTicketNumber" ];

            _checkDateReceivedParameter = checkDataSource.UpdateParameters[ "DateReceived" ];
            _checkSettlementDateParameter = checkDataSource.UpdateParameters[ "SettlementDate" ];
            _checkCommentsParameter = checkDataSource.UpdateParameters[ "CheckComments" ];           

            // insert parameters
            _checkIdForInsertParameter = checkDataSource.InsertParameters[ "CheckId" ];

        }

       
        public void CheckDataSource_Updated2( object sender, SqlDataSourceStatusEventArgs e )
        {
            string checkIdString = e.Command.Parameters[ "@CheckId" ].Value.ToString();

            if( checkIdString.Length > 0 )
            {
                int checkId;
                checkId = int.Parse( checkIdString );
                Session[ "LastUpdatedCheckId" ] = checkId;
            }
        }

        public void CheckDataSource_Inserted2( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@CheckId" ].Value != null )
            {
                string checkIdString = e.Command.Parameters[ "@CheckId" ].Value.ToString();

                if( checkIdString.Length > 0 )
                {
                    int checkId;
                    checkId = int.Parse( checkIdString );
                    Session[ "LastInsertedCheckId" ] = checkId;
                }
            }
            else
            {
                Exception insertException;
                insertException = e.Exception;

                if( insertException != null )
                {
                    throw new Exception( String.Format( "CheckId returned from insert was null. Insert failed. {0}", insertException.Message ) );
                }
                else
                {
                    throw new Exception( "CheckId returned from insert was null. Insert failed." );
                }
            }

        }

        public bool ValidateCheck( string contractNumber, int contractId, int quarterId, string checkNumber, decimal checkAmount, ref string validationMessage  )
        {
            bool bSuccess = false;      
            bool bIsValidated = false;

            ContractDB contractDB = null;

            contractDB = ( ContractDB )Session[ "ContractDB" ];

            bSuccess = contractDB.ValidateCheck(  contractNumber,  contractId,  quarterId,  checkNumber,  checkAmount, ref bIsValidated, ref validationMessage );

            if( bSuccess == false )
            {              
                ShowException( new Exception( contractDB.ErrorMessage ) );
            }

            return( bIsValidated );               
        }


        #endregion CheckSupport2



        
        #region PaymentSupport



        protected void LoadContractPayments()
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            string contractNumber = currentDocument.ContractNumber;

            if( Session[ "PaymentDataSource" ] == null )
            {

                PaymentDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, true );

                PaymentDataSource.ID = "PaymentDataSource";
                PaymentDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                PaymentDataSource.SelectCommand = "SelectContractPayments";

              
                PaymentDataSource.SetEventOwnerName( "PaymentDataSource" );

                CreatePaymentDataSourceParameters();

                PaymentDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                PaymentDataSource.SelectParameters.Add( _paymentContractNumberParameter );
                _paymentContractNumberParameter.DefaultValue = contractNumber;
                PaymentDataSource.SelectParameters.Add( _userLoginParameter );
                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;
              
                // save to session
                Session[ "PaymentDataSource" ] = PaymentDataSource;
            }
            else
            {
                PaymentDataSource = ( DocumentDataSource )Session[ "PaymentDataSource" ];
                PaymentDataSource.RestoreDelegatesAfterDeserialization( this, "PaymentDataSource" );
                RestorePaymentDataSourceParameters( PaymentDataSource );                
            }
           
            // dates are bound during grid row binding
        }


        protected void CreatePaymentDataSourceParameters()
        {
            // select parms
            _paymentContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );          
        }

        protected void RestorePaymentDataSourceParameters( DocumentDataSource paymentDataSource )
        {
            // select parms
            _paymentContractNumberParameter = paymentDataSource.SelectParameters[ "ContractNumber" ];
            _userLoginParameter = paymentDataSource.SelectParameters[ "UserLogin" ];          
        }
        

        #endregion PaymentSupport

        //$$$



        

        #region NationalPaymentSupport



        protected void LoadContractNationalPayments()
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            string contractNumber = currentDocument.ContractNumber;
            int contractId = currentDocument.ContractId;

            if( Session[ "NationalPaymentDataSource" ] == null )
            {

                NationalPaymentDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, true );

                NationalPaymentDataSource.ID = "NationalPaymentDataSource";
                NationalPaymentDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                NationalPaymentDataSource.SelectCommand = "SelectContractNationalPayments";

                NationalPaymentDataSource.UpdateCommand = "UpdateContractNationalPayment";

                NationalPaymentDataSource.InsertCommand = "InsertContractNationalPayment";

                NationalPaymentDataSource.DeleteCommand = "DeleteContractNationalPayment";

                NationalPaymentDataSource.SetEventOwnerName( "NationalPaymentDataSource" );

                NationalPaymentDataSource.Updated += new SqlDataSourceStatusEventHandler( NationalPaymentDataSource_Updated );
                NationalPaymentDataSource.Inserted += new SqlDataSourceStatusEventHandler( NationalPaymentDataSource_Inserted );

                CreateNationalPaymentDataSourceParameters();

                NationalPaymentDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                NationalPaymentDataSource.SelectParameters.Add( _nationalPaymentContractNumberParameter );
                _nationalPaymentContractNumberParameter.DefaultValue = contractNumber;
                NationalPaymentDataSource.SelectParameters.Add( _paymentContractIdParameter );
                _paymentContractIdParameter.DefaultValue = contractId.ToString();
                NationalPaymentDataSource.SelectParameters.Add( _userLoginParameter );
                
                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;
                _withAddPaymentParameter.DefaultValue = "false"; // not adding
                NationalPaymentDataSource.SelectParameters.Add( _withAddPaymentParameter );

                NationalPaymentDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;

                NationalPaymentDataSource.UpdateParameters.Add( _SRPActivityIdParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _nationalPaymentContractNumberParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _paymentContractIdParameter );                
                NationalPaymentDataSource.UpdateParameters.Add( _paymentQuarterIdParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _paymentAmountParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _submissionDateParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _submittedByUserNameParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _paymentMethodParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _paymentSourceParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _transactionIdParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _payGovTrackingIdParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _paymentCheckNumberParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _paymentDepositTicketNumberParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _paymentDebitVoucherNumberParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _paymentSettlementDateParameter );
                NationalPaymentDataSource.UpdateParameters.Add( _paymentCommentsParameter );

                NationalPaymentDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;

                NationalPaymentDataSource.InsertParameters.Add( _nationalPaymentContractNumberParameter );
                NationalPaymentDataSource.InsertParameters.Add( _paymentContractIdParameter );               
                NationalPaymentDataSource.InsertParameters.Add( _paymentQuarterIdParameter );
                NationalPaymentDataSource.InsertParameters.Add( _paymentAmountParameter );
                NationalPaymentDataSource.InsertParameters.Add( _submissionDateParameter );
                NationalPaymentDataSource.InsertParameters.Add( _submittedByUserNameParameter );
                NationalPaymentDataSource.InsertParameters.Add( _paymentMethodParameter );
                NationalPaymentDataSource.InsertParameters.Add( _paymentSourceParameter );
                NationalPaymentDataSource.InsertParameters.Add( _transactionIdParameter );
                NationalPaymentDataSource.InsertParameters.Add( _payGovTrackingIdParameter );
                NationalPaymentDataSource.InsertParameters.Add( _paymentCheckNumberParameter );
                NationalPaymentDataSource.InsertParameters.Add( _paymentDepositTicketNumberParameter );
                NationalPaymentDataSource.InsertParameters.Add( _paymentDebitVoucherNumberParameter );
                NationalPaymentDataSource.InsertParameters.Add( _paymentSettlementDateParameter );
                NationalPaymentDataSource.InsertParameters.Add( _paymentCommentsParameter );

                NationalPaymentDataSource.InsertParameters.Add( _SRPActivityIdForInsertParameter );

                NationalPaymentDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;

                NationalPaymentDataSource.DeleteParameters.Add( _nationalPaymentContractNumberParameter );
                NationalPaymentDataSource.DeleteParameters.Add( _SRPActivityIdParameter );

                // save to session
                Session[ "NationalPaymentDataSource" ] = NationalPaymentDataSource;

            }
            else
            {
                NationalPaymentDataSource = ( DocumentDataSource )Session[ "NationalPaymentDataSource" ];
                NationalPaymentDataSource.RestoreDelegatesAfterDeserialization( this, "NationalPaymentDataSource" );
                RestoreNationalPaymentDataSourceParameters( NationalPaymentDataSource );                
            }

            if( Session[ "NationalPaymentDateDataSource" ] == null )
            {
                NationalPaymentDateDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, false );

                NationalPaymentDateDataSource.ID = "NationalPaymentDateDataSource";
                NationalPaymentDateDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                NationalPaymentDateDataSource.SelectCommand = "SelectYearQuartersForEditContractNationalPayments";
                NationalPaymentDateDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
           
                NationalPaymentDateDataSource.SelectParameters.Add( _nationalPaymentContractNumberParameter ); // value set above

                // save to session
                Session[ "NationalPaymentDateDataSource" ] = NationalPaymentDateDataSource;

            }
            else
            {
                NationalPaymentDateDataSource = ( DocumentDataSource )Session[ "NationalPaymentDateDataSource" ];
                // note contract number parameter restored above
            }

            // dates are bound during grid row binding
        }


         protected void CreateNationalPaymentDataSourceParameters()
        {
            // select parms
            _nationalPaymentContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _paymentContractIdParameter = new Parameter( "ContractId", TypeCode.Int32 );
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );
            _withAddPaymentParameter = new Parameter( "WithAdd", TypeCode.Boolean );

            // update parms
            _SRPActivityIdParameter = new Parameter( "SRPActivityId", TypeCode.Int32 );
            _paymentQuarterIdParameter = new Parameter( "QuarterId", TypeCode.Int32 );
            _paymentAmountParameter = new Parameter( "PaymentAmount", TypeCode.Decimal );

            _submissionDateParameter = new Parameter( "SubmissionDate", TypeCode.DateTime );
            _submittedByUserNameParameter = new Parameter( "SubmittedByUserName", TypeCode.String );
            _paymentMethodParameter = new Parameter( "PaymentMethod", TypeCode.String );
            _paymentSourceParameter = new Parameter( "PaymentSource", TypeCode.String );
            _transactionIdParameter = new Parameter( "TransactionId", TypeCode.String );
            _payGovTrackingIdParameter = new Parameter( "PayGovTrackingId", TypeCode.String );
            _paymentCheckNumberParameter = new Parameter( "CheckNumber", TypeCode.String );
            _paymentDepositTicketNumberParameter = new Parameter( "DepositTicketNumber", TypeCode.String );
            _paymentDebitVoucherNumberParameter = new Parameter( "DebitVoucherNumber", TypeCode.String );
            _paymentSettlementDateParameter = new Parameter( "SettlementDate", TypeCode.DateTime ); 
          
            _paymentCommentsParameter = new Parameter( "Comments", TypeCode.String );

            // insert parameters
            _SRPActivityIdForInsertParameter = new Parameter( "SRPActivityId", TypeCode.Int32 );
            _SRPActivityIdForInsertParameter.Direction = ParameterDirection.Output;
        }

        protected void RestoreNationalPaymentDataSourceParameters( DocumentDataSource paymentDataSource )
        {
            // select parms
            _nationalPaymentContractNumberParameter = paymentDataSource.SelectParameters[ "ContractNumber" ];
            _paymentContractIdParameter = paymentDataSource.SelectParameters[ "ContractId" ];
            _userLoginParameter = paymentDataSource.SelectParameters[ "UserLogin" ];
            _withAddPaymentParameter = paymentDataSource.SelectParameters[ "WithAdd" ];

            // update parms
            _SRPActivityIdParameter = paymentDataSource.UpdateParameters[ "SRPActivityId" ];
            _paymentQuarterIdParameter = paymentDataSource.UpdateParameters[ "QuarterId" ];
            _paymentAmountParameter = paymentDataSource.UpdateParameters[ "PaymentAmount" ];

            _submissionDateParameter = paymentDataSource.UpdateParameters[ "SubmissionDate" ];
            _submittedByUserNameParameter = paymentDataSource.UpdateParameters[ "SubmittedByUserName" ];
            _paymentMethodParameter = paymentDataSource.UpdateParameters[ "PaymentMethod" ];
            _paymentSourceParameter = paymentDataSource.UpdateParameters[ "PaymentSource" ];
            _transactionIdParameter = paymentDataSource.UpdateParameters[ "TransactionId" ];
            _payGovTrackingIdParameter = paymentDataSource.UpdateParameters[ "PayGovTrackingId" ];
            _paymentCheckNumberParameter = paymentDataSource.UpdateParameters[ "CheckNumber" ];
            _paymentDepositTicketNumberParameter = paymentDataSource.UpdateParameters[ "DepositTicketNumber" ];
            _paymentDebitVoucherNumberParameter = paymentDataSource.UpdateParameters[ "DebitVoucherNumber" ];
            _paymentSettlementDateParameter = paymentDataSource.UpdateParameters[ "SettlementDate" ];


            _paymentCommentsParameter = paymentDataSource.UpdateParameters[ "Comments" ];           

            // insert parameters
            _SRPActivityIdForInsertParameter = paymentDataSource.InsertParameters[ "SRPActivityId" ];

        }

        public void NationalPaymentDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string SRPActivityIdString = e.Command.Parameters[ "@SRPActivityId" ].Value.ToString();

            if( SRPActivityIdString.Length > 0 )
            {
                int SRPActivityId;
                SRPActivityId = int.Parse( SRPActivityIdString );
                Session[ "LastUpdatedSRPActivityId" ] = SRPActivityId;
            }
        }

        public void NationalPaymentDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@SRPActivityId" ].Value != null )
            {
                string SRPActivityIdString = e.Command.Parameters[ "@SRPActivityId" ].Value.ToString();

                if( SRPActivityIdString.Length > 0 )
                {
                    int SRPActivityId;
                    SRPActivityId = int.Parse( SRPActivityIdString );
                    Session[ "LastInsertedSRPActivityId" ] = SRPActivityId;
                }
            }
            else
            {
                Exception insertException;
                insertException = e.Exception;

                if( insertException != null )
                {
                    throw new Exception( String.Format( "SRPActivityId returned from insert was null. Insert failed. {0}", insertException.Message ) );
                }
                else
                {
                    throw new Exception( "SRPActivityId returned from insert was null. Insert failed." );
                }
            }

        }

        #endregion NationalPaymentSupport





        //$$$

        #region SBASupport

        protected void LoadContractProjections( bool bSBAIdHasChanged )
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            string contractNumber = currentDocument.ContractNumber;

            if( Session[ "ProjectionDataSource" ] == null )
            {

                ProjectionDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, true );

                ProjectionDataSource.ID = "ProjectionDataSource";
                ProjectionDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                ProjectionDataSource.SelectCommand = "SelectContractSBAProjections";

                ProjectionDataSource.UpdateCommand = "UpdateContractSBAProjection";

                ProjectionDataSource.InsertCommand = "InsertContractSBAProjection";

                ProjectionDataSource.DeleteCommand = "DeleteContractSBAProjection";

                ProjectionDataSource.SetEventOwnerName( "ProjectionDataSource" );
                ProjectionDataSource.Updated += new SqlDataSourceStatusEventHandler( ProjectionDataSource_Updated );
                ProjectionDataSource.Inserted += new SqlDataSourceStatusEventHandler( ProjectionDataSource_Inserted );

                CreateProjectionDataSourceParameters();

                ProjectionDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                ProjectionDataSource.SelectParameters.Add( _projectionContractNumberParameter );
                _projectionContractNumberParameter.DefaultValue = contractNumber;
                ProjectionDataSource.SelectParameters.Add( _userLoginParameter );
                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;
                _withAddProjectionParameter.DefaultValue = "false"; // not adding
                ProjectionDataSource.SelectParameters.Add( _withAddProjectionParameter );

                // note: may be -1 = no plan selected
                _sbaPlanIdParameter.DefaultValue = DataRelay.EditedDocumentContentFront.SBAPlanId.ToString();

                ProjectionDataSource.SelectParameters.Add( _sbaPlanIdParameter );

                ProjectionDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;

                ProjectionDataSource.UpdateParameters.Add( _projectionContractNumberParameter );
                ProjectionDataSource.UpdateParameters.Add( _userLoginParameter );
                ProjectionDataSource.UpdateParameters.Add( _sbaPlanIdParameter );
                ProjectionDataSource.UpdateParameters.Add( _projectionIdParameter );
                ProjectionDataSource.UpdateParameters.Add( _projectionStartDateParameter );
                ProjectionDataSource.UpdateParameters.Add( _projectionEndDateParameter );
                ProjectionDataSource.UpdateParameters.Add( _totalSubContractingDollarsParameter );
                ProjectionDataSource.UpdateParameters.Add( _sbDollarsParameter );
                ProjectionDataSource.UpdateParameters.Add( _veteranOwnedDollarsParameter );
                ProjectionDataSource.UpdateParameters.Add( _disabledVeteranDollarsParameter );
                ProjectionDataSource.UpdateParameters.Add( _sdbDollarsParameter );
                ProjectionDataSource.UpdateParameters.Add( _womenOwnedDollarsParameter );
                ProjectionDataSource.UpdateParameters.Add( _hubZoneDollarsParameter );
                ProjectionDataSource.UpdateParameters.Add( _hbcuDollarsParameter );
                ProjectionDataSource.UpdateParameters.Add( _projectionCommentsParameter );

                ProjectionDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;

                ProjectionDataSource.InsertParameters.Add( _projectionContractNumberParameter );
                ProjectionDataSource.InsertParameters.Add( _userLoginParameter );
                ProjectionDataSource.InsertParameters.Add( _sbaPlanIdParameter );
                
                ProjectionDataSource.InsertParameters.Add( _projectionStartDateParameter );
                ProjectionDataSource.InsertParameters.Add( _projectionEndDateParameter );
                ProjectionDataSource.InsertParameters.Add( _totalSubContractingDollarsParameter );
                ProjectionDataSource.InsertParameters.Add( _sbDollarsParameter );
                ProjectionDataSource.InsertParameters.Add( _veteranOwnedDollarsParameter );
                ProjectionDataSource.InsertParameters.Add( _disabledVeteranDollarsParameter );
                ProjectionDataSource.InsertParameters.Add( _sdbDollarsParameter );
                ProjectionDataSource.InsertParameters.Add( _womenOwnedDollarsParameter );
                ProjectionDataSource.InsertParameters.Add( _hubZoneDollarsParameter );
                ProjectionDataSource.InsertParameters.Add( _hbcuDollarsParameter );
                ProjectionDataSource.InsertParameters.Add( _projectionCommentsParameter );

                ProjectionDataSource.InsertParameters.Add( _projectionIdForInsertParameter );

                ProjectionDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;

                ProjectionDataSource.DeleteParameters.Add( _projectionContractNumberParameter );
                ProjectionDataSource.DeleteParameters.Add( _projectionIdParameter );

                // save to session
                Session[ "ProjectionDataSource" ] = ProjectionDataSource;

            }
            else
            {
                ProjectionDataSource = ( DocumentDataSource )Session[ "ProjectionDataSource" ];
                ProjectionDataSource.RestoreDelegatesAfterDeserialization( this, "ProjectionDataSource" );
                RestoreProjectionDataSourceParameters( ProjectionDataSource );
               
                if( bSBAIdHasChanged == true )
                {
                    // note: may be -1 = no plan selected
                    _sbaPlanIdParameter.DefaultValue = DataRelay.EditedDocumentContentFront.SBAPlanId.ToString();
                }
            }

        }


        protected void CreateProjectionDataSourceParameters()
        {
            // select parms
            _projectionContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );
            _withAddProjectionParameter = new Parameter( "WithAdd", TypeCode.Boolean );
            _sbaPlanIdParameter = new Parameter( "SBAPlanId", TypeCode.Int32 );

            // update parms
            _projectionIdParameter = new Parameter( "ProjectionId", TypeCode.Int32 );
            _projectionStartDateParameter = new Parameter( "ProjectionStartDate", TypeCode.DateTime );
            _projectionEndDateParameter = new Parameter( "ProjectionEndDate", TypeCode.DateTime );
            _totalSubContractingDollarsParameter = new Parameter( "TotalSubcontractingDollars", TypeCode.Decimal );
            _sbDollarsParameter = new Parameter( "SBDollars", TypeCode.Decimal );
            _veteranOwnedDollarsParameter = new Parameter( "VeteranOwnedDollars", TypeCode.Decimal );
            _disabledVeteranDollarsParameter = new Parameter( "DisabledVeteranOwnedDollars", TypeCode.Decimal );
            _sdbDollarsParameter = new Parameter( "SDBDollars", TypeCode.Decimal );
            _womenOwnedDollarsParameter = new Parameter( "WomenOwnedDollars", TypeCode.Decimal );
            _hubZoneDollarsParameter = new Parameter( "HubZoneDollars", TypeCode.Decimal );
            _hbcuDollarsParameter = new Parameter( "HBCUDollars", TypeCode.Decimal );
            _projectionCommentsParameter = new Parameter( "ProjectionComments", TypeCode.String );

            // insert parameters
            _projectionIdForInsertParameter = new Parameter( "ProjectionId", TypeCode.Int32 );
            _projectionIdForInsertParameter.Direction = ParameterDirection.Output;
        }

        protected void RestoreProjectionDataSourceParameters( DocumentDataSource projectionDataSource )
        {
            // select parms
            _projectionContractNumberParameter = projectionDataSource.SelectParameters[ "ContractNumber" ];
            _userLoginParameter = projectionDataSource.SelectParameters[ "UserLogin" ];
            _withAddProjectionParameter = projectionDataSource.SelectParameters[ "WithAdd" ];
            _sbaPlanIdParameter = projectionDataSource.SelectParameters[ "SBAPlanId" ];

            // update parms
            _projectionIdParameter = projectionDataSource.UpdateParameters[ "ProjectionId" ];
            _projectionStartDateParameter = projectionDataSource.UpdateParameters[ "ProjectionStartDate" ];
            _projectionEndDateParameter = projectionDataSource.UpdateParameters[ "ProjectionEndDate" ];
            _totalSubContractingDollarsParameter = projectionDataSource.UpdateParameters[ "TotalSubcontractingDollars" ];
            _sbDollarsParameter = projectionDataSource.UpdateParameters[ "SBDollars" ];
            _veteranOwnedDollarsParameter = projectionDataSource.UpdateParameters[ "VeteranOwnedDollars" ];
            _disabledVeteranDollarsParameter = projectionDataSource.UpdateParameters[ "DisabledVeteranOwnedDollars" ];
            _sdbDollarsParameter = projectionDataSource.UpdateParameters[ "SDBDollars" ];
            _womenOwnedDollarsParameter = projectionDataSource.UpdateParameters[ "WomenOwnedDollars" ];
            _hubZoneDollarsParameter = projectionDataSource.UpdateParameters[ "HubZoneDollars" ];
            _hbcuDollarsParameter = projectionDataSource.UpdateParameters[ "HBCUDollars" ];
            _projectionCommentsParameter = projectionDataSource.UpdateParameters[ "ProjectionComments" ];

            // insert parameters
            _projectionIdForInsertParameter = projectionDataSource.InsertParameters[ "ProjectionId" ];
        }
        public void ProjectionDataSource_Updated( object sender, SqlDataSourceStatusEventArgs e )
        {
            string projectionIdString = e.Command.Parameters[ "@ProjectionId" ].Value.ToString();

            if( projectionIdString.Length > 0 )
            {
                int projectionId;
                projectionId = int.Parse( projectionIdString );
                Session[ "LastUpdatedProjectionId" ] = projectionId;
            }
        }

        public void ProjectionDataSource_Inserted( object sender, SqlDataSourceStatusEventArgs e )
        {
            if( e.Command.Parameters[ "@ProjectionId" ].Value != null )
            {
                string projectionIdString = e.Command.Parameters[ "@ProjectionId" ].Value.ToString();

                if( projectionIdString.Length > 0 )
                {
                    int projectionId;
                    projectionId = int.Parse( projectionIdString );
                    Session[ "LastInsertedProjectionId" ] = projectionId;
                }
            }
            else
            {
                Exception insertException;
                insertException = e.Exception;

                if( insertException != null )
                {
                    throw new Exception( String.Format( "ProjectionId returned from insert was null. Insert failed. {0}", insertException.Message ) );
                }
                else
                {
                    throw new Exception( "ProjectionId returned from insert was null. Insert failed." );
                }
            }

        }

        protected void LoadSBAPlanDetails( bool bSBAIdHasChanged )
        {
            if( Page.Session[ "SBAPlanDetailsDataSource" ] == null )
            {
                _sbaPlanDetailsDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.NACCMCommonUser, true );
                _sbaPlanDetailsDataSource.ID = "SBAPlanDetailsDataSource";
                _sbaPlanDetailsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _sbaPlanDetailsDataSource.SelectCommand = "SelectSBAPlanDetails";

                _sbaPlanDetailsDataSource.SetEventOwnerName( "SBAPlanDetailsDataSource" );

                CreateSBAPlanDetailsDataSourceParameters();

                _sbaPlanDetailsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
                _sbaPlanDetailsDataSource.SelectParameters.Add( _sbaPlanDetailsContractNumberParameter );
                _sbaPlanDetailsContractNumberParameter.DefaultValue = _currentDocument.ContractNumber;

                // note: may be -1 = no plan selected
                _sbaPlanIdForDetailsParameter.DefaultValue = DataRelay.EditedDocumentContentFront.SBAPlanId.ToString();
                
                _sbaPlanDetailsDataSource.SelectParameters.Add( _sbaPlanIdForDetailsParameter );

                Page.Session[ "SBAPlanDetailsDataSource" ] = _sbaPlanDetailsDataSource;
            }
            else
            {
                _sbaPlanDetailsDataSource = ( DocumentDataSource )Page.Session[ "SBAPlanDetailsDataSource" ];
                _sbaPlanDetailsDataSource.RestoreDelegatesAfterDeserialization( this, "SBAPlanDetailsDataSource" );

                RestoreSBAPlanDetailsDataSourceParameters( _sbaPlanDetailsDataSource );

                if( bSBAIdHasChanged == true )
                {
                    _sbaPlanIdForDetailsParameter.DefaultValue = DataRelay.EditedDocumentContentFront.SBAPlanId.ToString();
                }
            }
        }

        private void CreateSBAPlanDetailsDataSourceParameters()
        {
            _sbaPlanDetailsContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _sbaPlanIdForDetailsParameter = new Parameter( "SBAPlanId", TypeCode.Int32 );
        }

        private void RestoreSBAPlanDetailsDataSourceParameters( DocumentDataSource scheduleSINDataSource )
        {
            // select
            _sbaPlanDetailsContractNumberParameter = _sbaPlanDetailsDataSource.SelectParameters[ "ContractNumber" ];
            _sbaPlanIdForDetailsParameter = _sbaPlanDetailsDataSource.SelectParameters[ "SBAPlanId" ]; 
        }

        protected void LoadContractSBAAssociatedContracts( bool bSBAIdHasChanged )
        {
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            string contractNumber = currentDocument.ContractNumber;

            if( Session[ "SBAAssociatedContractsDataSource" ] == null )
            {

                SBAAssociatedContractsDataSource = new DocumentDataSource( bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, true );

                SBAAssociatedContractsDataSource.ID = "SBAAssociatedContractsDataSource";
                SBAAssociatedContractsDataSource.DataSourceMode = SqlDataSourceMode.DataSet;

                SBAAssociatedContractsDataSource.SelectCommand = "SelectSBAAssociatedContracts";

                SBAAssociatedContractsDataSource.SetEventOwnerName( "SBAAssociatedContractsDataSource" );

                CreateSBAAssociatedContractsDataSourceParameters();

                SBAAssociatedContractsDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                SBAAssociatedContractsDataSource.SelectParameters.Add( _userLoginParameter );
                _userLoginParameter.DefaultValue = bs.UserInfo.LoginName;

                SBAAssociatedContractsDataSource.SelectParameters.Add( _associatedContractsContractNumberParameter );
                _associatedContractsContractNumberParameter.DefaultValue = contractNumber;
       
                // note: may be -1 = no plan selected
                _associatedContractsSBAPlanIdParameter.DefaultValue = DataRelay.EditedDocumentContentFront.SBAPlanId.ToString();

                SBAAssociatedContractsDataSource.SelectParameters.Add( _associatedContractsSBAPlanIdParameter );

                // save to session
                Session[ "SBAAssociatedContractsDataSource" ] = SBAAssociatedContractsDataSource;

            }
            else
            {
                SBAAssociatedContractsDataSource = ( DocumentDataSource )Session[ "SBAAssociatedContractsDataSource" ];
                SBAAssociatedContractsDataSource.RestoreDelegatesAfterDeserialization( this, "SBAAssociatedContractsDataSource" );
                RestoreSBAAssociatedContractsDataSourceParameters( SBAAssociatedContractsDataSource );
                

                if( bSBAIdHasChanged == true )
                {
                    // note: may be -1 = no plan selected
                    _associatedContractsSBAPlanIdParameter.DefaultValue = DataRelay.EditedDocumentContentFront.SBAPlanId.ToString();
                }
            }
        }

        protected void CreateSBAAssociatedContractsDataSourceParameters()
        {
            // select parms
            _associatedContractsContractNumberParameter = new Parameter( "ContractNumber", TypeCode.String );
            _userLoginParameter = new Parameter( "UserLogin", TypeCode.String );     
            _associatedContractsSBAPlanIdParameter = new Parameter( "SBAPlanId", TypeCode.Int32 );
        }

        protected void RestoreSBAAssociatedContractsDataSourceParameters( DocumentDataSource sbaAssociatedContractsDataSource )
        {
            // select parms
            _associatedContractsContractNumberParameter = sbaAssociatedContractsDataSource.SelectParameters[ "ContractNumber" ];
            _userLoginParameter = sbaAssociatedContractsDataSource.SelectParameters[ "UserLogin" ];
            _associatedContractsSBAPlanIdParameter = sbaAssociatedContractsDataSource.SelectParameters[ "SBAPlanId" ];
        }
        
        #endregion SBASupport


        // update the main update panel
        //protected void UpdateMasterUpdatePanelFromContract()
        //{
        //     ContractView contractView = ( ContractView )Page.Master;
        //     if( contractView != null )
        //     {
        //         NACCM naccm = ( NACCM )contractView.Master;
        //         if( naccm != null )
        //         {
        //             UpdatePanel masterUpdatePanel = naccm.MasterUpdatePanel;
        //             if( masterUpdatePanel != null )
        //             {
        //                 masterUpdatePanel.Update();
        //             }
        //         }
        //     }
        //}

        public void TriggerViewMasterUpdatePanel()
        {
            if( _documentEditorType == DocumentEditorTypes.Contract )
            {
                TriggerContractViewMasterUpdatePanelFromContract();
            }
            else if( _documentEditorType == DocumentEditorTypes.Offer || _documentEditorType == DocumentEditorTypes.NewOffer )
            {
                TriggerOfferViewMasterUpdatePanelFromOffer();
            }
            else if( _documentEditorType == DocumentEditorTypes.NewContract || _documentEditorType == DocumentEditorTypes.NewContractFromOffer )
            {
                TriggerDocumentCreationMasterUpdatePanelFromCreate();
            }
        }

        public void TriggerContractViewMasterUpdatePanelFromContract()
        {
            ContractView contractView = ( ContractView )Page.Master;
            if( contractView != null )
            {
                UpdatePanelEventProxy contractViewMasterEventProxy = contractView.ContractViewMasterEventProxy;
                if( contractViewMasterEventProxy != null )
                {
                    contractViewMasterEventProxy.InvokeEvent( new EventArgs() );
                }              
            }
        }

        public void TriggerOfferViewMasterUpdatePanelFromOffer()
        {
            OfferView offerView = ( OfferView )Page.Master;
            if( offerView != null )
            {
                UpdatePanelEventProxy offerViewMasterEventProxy = offerView.OfferViewMasterEventProxy;
                if( offerViewMasterEventProxy != null )
                {
                    offerViewMasterEventProxy.InvokeEvent( new EventArgs() );
                }
            }
        }

        public void TriggerDocumentCreationMasterUpdatePanelFromCreate()
        {
            DocumentCreation documentCreation = ( DocumentCreation )Page.Master;
            if( documentCreation != null )
            {
                UpdatePanelEventProxy documentCreationMasterEventProxy = documentCreation.DocumentCreationMasterEventProxy;
                if( documentCreationMasterEventProxy != null )
                {
                    documentCreationMasterEventProxy.InvokeEvent( new EventArgs() );
                }
            }
        }

        protected Control ContractFindControl( string controlName )
        {
            Control returnControl = null;
            ContractView contractView = ( ContractView )Page.Master;
            if( contractView != null )
            {
                ContentPlaceHolder contractTabs = ( ContentPlaceHolder )contractView.ContractTabs;

                if( contractTabs != null )
                {
                    returnControl = contractTabs.FindControl( controlName );
                }
            }

            return ( returnControl );
        }

        #region OfferSupport

        protected Control OfferFindControl( string controlName )
        {
            Control returnControl = null;
            OfferView offerView = ( OfferView )Page.Master;
            if( offerView != null )
            {
                ContentPlaceHolder offerTabs = ( ContentPlaceHolder )offerView.OfferTabs;

                if( offerTabs != null )
                {
                    returnControl = offerTabs.FindControl( controlName );
                }
            }

            return ( returnControl );
        }

        protected void LoadParentContractsForSchedule( int scheduleNumber )
        {
            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            DataSet dsParentContracts = null;
            bool bSuccess = false;

            try
            {
                if( Cache[ GetParentContractsCacheName( scheduleNumber ) ] != null )
                {
                    dsParentContracts = ( DataSet )Cache[ GetParentContractsCacheName( scheduleNumber ) ];
                }
                else
                {
                    contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    contractDB.MakeConnectionString();

                    bSuccess = contractDB.GetParentContractsForSchedule( scheduleNumber, ref dsParentContracts );

                    if( bSuccess == true )
                    {
                        CMGlobals.CreateDataSetCache( this.Page, GetParentContractsCacheName( scheduleNumber ), dsParentContracts );
                    }
                    else
                    {
                        throw new Exception( contractDB.ErrorMessage );
                    }                    
                }

                ParentContractsForScheduleDataSet = dsParentContracts;
            }
            catch( Exception ex )
            {
                throw new Exception( ex.Message );
            }
        }


        protected void GetParentContractAndVendorInfoForBPA( string parentContractNumber )
        {
            bool bSuccess = false;

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
            string primaryName = "";
            string primaryPhone = "";
            string primaryExt = "";
            string primaryFax = "";
            string primaryEmail = "";
            string vendorWebUrl = "";
            string primaryAddress1 = "";
            string primaryAddress2 = "";
            string primaryCity = "";
            int primaryCountryId = -1;
            string primaryState = "";
            string primaryZip = "";

            try
            {
                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];

                contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                contractDB.MakeConnectionString();

                bSuccess = contractDB.GetContractAndVendorInfo( parentContractNumber, ref scheduleNumber, ref ownerId, ref vendorName, ref description, ref awardDate, ref expirationDate, ref completionDate, ref effectiveDate, ref scheduleName, ref ownerName,
                    ref primaryName, ref primaryPhone, ref primaryExt, ref primaryFax, ref primaryEmail, ref vendorWebUrl,
                ref primaryAddress1, ref primaryAddress2, ref primaryCity, ref primaryCountryId, ref primaryState, ref primaryZip );

                if( bSuccess == true )
                {
                    // save the returned data from the parent into the current BPA
                    EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
                    if( editedDocumentContentFront != null )
                    {
                        editedDocumentContentFront.VendorName = vendorName;
                        editedDocumentContentFront.VendorPrimaryContactName = primaryName;
                        editedDocumentContentFront.VendorPrimaryContactPhone = primaryPhone;
                        editedDocumentContentFront.VendorPrimaryContactExtension = primaryExt;
                        editedDocumentContentFront.VendorPrimaryContactFax = primaryFax;
                        editedDocumentContentFront.VendorPrimaryContactEmail = primaryEmail;
                        editedDocumentContentFront.VendorAddress1 = primaryAddress1;
                        editedDocumentContentFront.VendorAddress2 = primaryAddress2;
                        editedDocumentContentFront.VendorCity = primaryCity;
                        editedDocumentContentFront.VendorCountryId = primaryCountryId;
                        editedDocumentContentFront.VendorState = primaryState;
                        editedDocumentContentFront.VendorZip = primaryZip;
                        editedDocumentContentFront.VendorWebAddress = vendorWebUrl;
                    }
                }
            }
            catch( Exception ex )
            {
                throw new Exception( ex.Message );
            }
        }

        protected void LoadContractingOfficersForDivision( int divisionId )
        { 
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            DataSet dsContractingOfficers = null;

            try
            {
                bs.GetContractingOfficersForDivision( divisionId, ref dsContractingOfficers );

                ContractingOfficersDataSet = dsContractingOfficers;

                // save to session
        //        Session[ "ContractingOfficersDataSet" ] = ContractingOfficersDataSet;
            }
            catch( Exception ex )
            {
                throw new Exception( ex.Message );
            }
        }

        public static string GetSchedulesCacheName( int divisionId )
        {
            return ( string.Format( "SchedulesDataSetForDivision{0}", divisionId.ToString() ) );
        }

        public static string GetParentContractsCacheName( int scheduleNumber )
        {
            return ( string.Format( "ParentContractsDataSetForSchedule{0}", scheduleNumber.ToString() ) );
        }

        protected void LoadSchedulesForDivision( int divisionId )
        {
            bool bSuccess = true;

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            DataSet dsSchedulesForDivision = null;

            try
            {  
                if( Cache[ GetSchedulesCacheName( divisionId ) ] != null )
                {
                    dsSchedulesForDivision = ( DataSet )Cache[ GetSchedulesCacheName( divisionId ) ];
                }
                else
                {
                    contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    contractDB.MakeConnectionString();

                    bSuccess = contractDB.SelectSchedulesForDivision( ref dsSchedulesForDivision, divisionId );
                    if( bSuccess == true )
                    {
                        CMGlobals.CreateDataSetCache( this.Page, GetSchedulesCacheName( divisionId ), dsSchedulesForDivision );
                    }
                    else
                    {
                        throw new Exception( contractDB.ErrorMessage );
                    }
                }
             
                SchedulesDataSet = dsSchedulesForDivision;
            }
            catch( Exception ex )
            {
                throw new Exception( ex.Message );
            }
    
        }

        // used for contract creation process
        protected void LoadDivisions()
        {
            bool bSuccess = true;

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            DataSet dsDivisions = null;

            if( Session[ "DivisionsDataSet" ] == null )
            {
                try
                {
                    contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    contractDB.MakeConnectionString();

                    bSuccess = contractDB.SelectDivisions( ref dsDivisions );

                    DivisionsDataSet = dsDivisions;

                    // save to session
                    Session[ "DivisionsDataSet" ] = DivisionsDataSet;
                }
                catch( Exception ex )
                {
                    throw new Exception( ex.Message );
                }
            }
            else
            {
                DivisionsDataSet = ( DataSet )Session[ "DivisionsDataSet" ];
            }
        }

        public bool IsOfferActionCompleted( int actionId )
        {
            bool bComplete = false;

            if( OfferActionTypesDataSet == null )
            {
                LoadOfferActionTypes();
            }

            foreach( DataRow row in OfferActionTypesDataSet.Tables[ OfferDB.OfferActionTypesTableName ].Rows )
            {
                string actionIdString = row[ "ActionId" ].ToString();

                if( int.Parse( actionIdString ) == actionId )
                {
                    bComplete = bool.Parse( row[ "IsOfferComplete" ].ToString() );
                    break;
                }
            }

            return ( bComplete );
        }

        public static string OfferActionTypesCacheName = "OfferActionTypesDataSet";

        protected void LoadOfferActionTypes()
        {
            bool bSuccess = true;

            OfferDB offerDB = ( OfferDB )Session[ "OfferDB" ];
            DataSet dsOfferActionTypes = null;

            if( Cache[ OfferActionTypesCacheName ] == null )
            {
                try
                {
                    offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    offerDB.MakeConnectionString();

                    bSuccess = offerDB.SelectOfferActionTypes( ref dsOfferActionTypes );

                    if( bSuccess == true )
                    {
                        OfferActionTypesDataSet = dsOfferActionTypes;

                        CMGlobals.CreateDataSetCache( this.Page, OfferActionTypesCacheName, dsOfferActionTypes ); ;
                    }
                    else
                    {
                        throw new Exception( offerDB.ErrorMessage );
                    }
                }
                catch( Exception ex )
                {
                    throw new Exception( ex.Message );
                }
            }
            else
            {
                OfferActionTypesDataSet = ( DataSet )Cache[ OfferActionTypesCacheName ];
            }
        }

        public static string SolicitationsCacheName = "SolicitationsDataSet";

        // used by offers and contracts
        protected void LoadActiveSolicitations()
        {
            bool bSuccess = true;

            OfferDB offerDB = ( OfferDB )Session[ "OfferDB" ];
            DataSet dsSolicitations = null;

            if( Cache[ SolicitationsCacheName ] == null )
            {
                try
                {
                    offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    offerDB.MakeConnectionString();

                    bSuccess = offerDB.SelectSolicitations( ref dsSolicitations, "A" );

                    if( bSuccess == true && offerDB.RowsReturned > 0 )
                    {
                        ActiveSolicitationsDataSet = dsSolicitations;

                        // save to cache
                        CMGlobals.CreateDataSetCache( this.Page, SolicitationsCacheName, ActiveSolicitationsDataSet );
                    }
                }
                catch( Exception ex )
                {
                    throw new Exception( ex.Message );
                }
            }
            else
            {
                ActiveSolicitationsDataSet = ( DataSet )Cache[ SolicitationsCacheName ];
            }
        }


        protected void LoadExtendableContracts( int proposalTypeId, int scheduleNumber )
        {
            bool bSuccess = true;

            ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
            DataSet dsExtendableContracts = null;

            try
            {               
                contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                contractDB.MakeConnectionString();

                bSuccess = contractDB.SelectExtendableContracts( ref dsExtendableContracts, proposalTypeId, scheduleNumber );
                if( bSuccess != true )
                {                        
                    throw new Exception( contractDB.ErrorMessage );
                }

                ExtendableContractsDataSet = dsExtendableContracts;
            }
            catch( Exception ex )
            {
                throw new Exception( ex.Message );
            }
        }

        #endregion OfferSupport

        #region CreateContractSupport

        protected void InitContractFromOffer( int offerId, int scheduleNumber )
        {
            EditedDocumentContent editedDocumentContentFront = DataRelay.EditedDocumentContentFront;

            DataSet dsOneOfferRow = null;

            bool bSuccess = true;

            OfferDB offerDB = ( OfferDB )Session[ "OfferDB" ];

            bSuccess = offerDB.GetOfferInfo2( ref dsOneOfferRow, offerId );
            if( bSuccess == true )
            {
                editedDocumentContentFront.InitContractFromOffer( dsOneOfferRow );
            }
            else
            {
                ShowException( new Exception( offerDB.ErrorMessage ) );
            }
        }

        internal void CancelCreation()
        {
            // CreateContract2 constructor is called upon cancel by the runtime, so it resets document editor type to NewContract from NewContractFromOffer
            if( _documentEditorType == DocumentEditorTypes.NewContract || _documentEditorType == DocumentEditorTypes.NewContractFromOffer )
            {

                if( DataRelay != null )
                {
                    if( DataRelay.EditedDocumentContentFront != null )
                    {
                        DataRelay.EditedDocumentContentFront.ClearDirtyFlags();
                    }
                }
            }
            else if( _documentEditorType == DocumentEditorTypes.NewOffer )
            {
                if( OfferDataRelay != null )
                {
                    if( OfferDataRelay.EditedOfferContentFront != null )
                    {
                        OfferDataRelay.EditedOfferContentFront.ClearDirtyFlags();
                    }
                }
            }
        }

        #endregion CreateContractSupport

    }
}