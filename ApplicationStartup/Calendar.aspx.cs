using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using VA.NAC.Application.SharedObj;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class Calendar : System.Web.UI.Page
    {
        public DateTime SelectedDate;
        public DateTypes DateType = DateTypes.Undefined;
        private ArrayList YearList = new ArrayList();
        private ArrayList MonthList = new ArrayList();
        public DateTime MinAllowedDate;
        public DateTime MaxAllowedDate;
        private bool _bUnlimited = false; // allow unlimited date selection ( based on the arrays )
        public string ReceivingControlClientId = "";

        public enum DateTypes
        {
            Award,      // used in contract creation
            Effective,
            Expiration,
            Completion,
            CAward,     // Cxxx used in contract general tab 
            CEffective,
            CExpiration,
            CCompletion,
            C2Award,     // C2xxx used in new contract general tab 
            C2Effective,
            C2Expiration,
            C2Completion,
            C2PricelistModificationEffective, // this one used in new pricelist tab
            OAssignment,  // Oxxx used in offer edit form
            OReceived,
            OReassignment,
            OEstimatedCompletion,
            OAction,
            OAudit,
            OReturn,

            O2Assignment,  // O2xxx used in release 2 offer general tab
            O2Received,
            O2Reassignment,
            O2EstimatedCompletion,
            O2Action,
            O2Audit,
            O2Return,

            X2Award,     // X2xxx used on new contract creation screen
            X2Effective,
            X2Expiration,
            X2Completion,

            InsurancePolicyEffective,
            InsurancePolicyExpiration,

            ProjectionStart,
            ProjectionEnd,

            Undefined   // 36th item so MAXENUMINDEX = 35 ( zero based )
        }

        private const int MAXENUMINDEX = 35;

        protected int GetDateTypeIndexFromDateType( DateTypes dateType )
        {
            string[] nameList = Enum.GetNames( typeof( DateTypes ) );
            string dateTypeName = Enum.GetName( typeof( DateTypes ), dateType );
            int retVal = MAXENUMINDEX;

            for( int i = 0; i < nameList.Length; i++ )
            {
                if( dateTypeName.CompareTo( nameList[ i ] ) == 0 )
                {
                    retVal = i;
                    break;
                }
            }

            return ( retVal );
        }

        // these correspond to the "unlimited" setting
        // these array values are back years for each of the enum DateTypes above
        protected int[] _backYears = { 1, 1, 1, 1, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 10, 10, 1 };

        // these array values are future years for each of the enum DateTypes above
        protected int[] _futureYears = { 1, 1, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 1 };


        protected string GetDateTypeStringFromDateType( DateTypes dateType )
        {

            string retVal = "Undefined";

            switch( dateType )
            {
                case  DateTypes.Award:
                    retVal = "Award";
                    break;
                case  DateTypes.Effective:
                    retVal = "Effective";
                    break;
                case  DateTypes.Expiration:
                    retVal = "Expiration";
                    break;               
                case  DateTypes.Completion:
                    retVal = "Completion";
                    break;
                case  DateTypes.CAward:
                    retVal = "Award";
                    break;
                case  DateTypes.CEffective:
                    retVal = "Effective";
                    break;
                case  DateTypes.CExpiration:
                    retVal = "Expiration";
                    break;
                case  DateTypes.CCompletion:
                    retVal = "Completion";
                    break;
                case  DateTypes.C2Award:
                    retVal = "Award";
                    break;
                case  DateTypes.C2Effective:
                    retVal = "Effective";
                    break;
                case  DateTypes.C2Expiration:
                    retVal = "Expiration";
                    break;
                case  DateTypes.C2Completion:
                    retVal = "Completion";
                    break;
                case  DateTypes.C2PricelistModificationEffective:
                    retVal = "Modification Effective";
                    break;
                case  DateTypes.OAssignment:
                    retVal = "Assignment";
                    break;
                case  DateTypes.OReceived:
                    retVal = "Received";
                    break;
                case  DateTypes.OReassignment:
                    retVal = "Reassignment";
                    break;
                case  DateTypes.OEstimatedCompletion:
                    retVal = "Estimated Completion";
                    break;
                case  DateTypes.OAction:
                    retVal = "Action";
                    break;
                case  DateTypes.OAudit:
                    retVal = "Audit";
                    break;
                case  DateTypes.OReturn:
                    retVal = "Return";
                    break;
                case DateTypes.O2Assignment:
                    retVal = "Assignment";
                    break;
                case DateTypes.O2Received:
                    retVal = "Received";
                    break;
                case DateTypes.O2Reassignment:
                    retVal = "Reassignment";
                    break;
                case DateTypes.O2EstimatedCompletion:
                    retVal = "Estimated Completion";
                    break;
                case DateTypes.O2Action:
                    retVal = "Action";
                    break;
                case DateTypes.O2Audit:
                    retVal = "Audit";
                    break;
                case DateTypes.O2Return:
                    retVal = "Return";
                    break;
                case  DateTypes.X2Award:
                    retVal = "Award";
                    break;
                case  DateTypes.X2Effective:
                    retVal = "Effective";
                    break;
                case  DateTypes.X2Expiration:
                    retVal = "Expiration";
                    break;
                case  DateTypes.X2Completion:
                    retVal = "Completion";
                    break;
                case  DateTypes.InsurancePolicyEffective:
                    retVal = "Insurance Policy Effective";
                    break;
                case  DateTypes.InsurancePolicyExpiration:
                    retVal = "Insurance Policy Expiration";
                    break;
                case DateTypes.ProjectionStart:
                    retVal = "Projection Start";
                    break;
                case DateTypes.ProjectionEnd:
                    retVal = "Projection End";
                    break;
                case  DateTypes.Undefined:
                    retVal = "Undefined";
                    break;
            }
            return( retVal );
        }

        public string GetCaption()
        {
            string caption = "";

            caption = string.Format( "{0} Date", GetDateTypeStringFromDateType( DateType ) );

            return ( caption );
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            DateTime initialDate = DateTime.Now;
            int currentYear = DateTime.Now.Year;

            CMGlobals.CheckIfStartedProperly( this.Page );

            if( Request.QueryString[ "DateType" ] != null )
            {
                DateType = ( DateTypes )Enum.Parse( typeof( DateTypes ), Request.QueryString[ "DateType" ] );
            }

            if( Request.QueryString[ "Unlimited" ] != null )
            {
                int unlimited = int.Parse( Request.QueryString[ "Unlimited" ].ToString() );
                _bUnlimited = ( unlimited == 1 ) ? true : false;
            }

            if( Request.QueryString[ "MinAllowedDate" ] != null )
            {
                string minAllowedDateString = Request.QueryString[ "MinAllowedDate" ].ToString();
                MinAllowedDate = DateTime.Parse( minAllowedDateString );
            }
            
            if( Request.QueryString[ "MaxAllowedDate" ] != null )
            {
                string maxAllowedDateString = Request.QueryString[ "MaxAllowedDate" ].ToString();
                MaxAllowedDate = DateTime.Parse( maxAllowedDateString );
            }

            if( Request.QueryString[ "ReceivingControlClientId" ] != null )
            {
                ReceivingControlClientId = Request.QueryString[ "ReceivingControlClientId" ].ToString();             
            }


            if( IsPostBack == false )
            {
                if( Request.QueryString[ "InitialDate" ] != null )
                {
                    string initialDateString = Request.QueryString[ "InitialDate" ].ToString();
                    initialDate = DateTime.Parse( initialDateString );
                    calendar1.VisibleDate = initialDate;
                    calendar1.SelectedDate = initialDate;
                    Session[ Enum.GetName( typeof( DateTypes ), DateType ) ] = initialDate;
                    Session[ "CalendarInitialDate" ] = initialDate;
                }

                if( DateType == DateTypes.CAward || DateType == DateTypes.CCompletion || DateType == DateTypes.CEffective || DateType == DateTypes.CExpiration )
                {
                    string closeFunctionCallStringForDateType = String.Format( "CloseWindowFromContractInfo('{0}', 'True');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormOkCloseButton.Attributes.Add( "onclick", closeFunctionCallStringForDateType );
                    string closeFunctionCallStringForCancel = String.Format( "CloseWindowFromContractInfo('{0}', 'False');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormCancelButton.Attributes.Add( "onclick", closeFunctionCallStringForCancel );
                }
                else if( DateType == DateTypes.OAssignment || DateType == DateTypes.OReassignment || DateType == DateTypes.OEstimatedCompletion || DateType == DateTypes.OAction || DateType == DateTypes.OAudit || DateType == DateTypes.OReturn || DateType == DateTypes.OReceived )
                {
                    string closeFunctionCallStringForDateType = String.Format( "CloseWindowFromOfferRecord('{0}', 'True');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormOkCloseButton.Attributes.Add( "onclick", closeFunctionCallStringForDateType );
                    string closeFunctionCallStringForCancel = String.Format( "CloseWindowFromOfferRecord('{0}', 'False');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormCancelButton.Attributes.Add( "onclick", closeFunctionCallStringForCancel );
                }
                else if( DateType == DateTypes.C2Award || DateType == DateTypes.C2Completion || DateType == DateTypes.C2Effective || DateType == DateTypes.C2Expiration )
                {
                    string closeFunctionCallStringForDateType = String.Format( "CloseWindowFromContractInfo2('{0}', 'True');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormOkCloseButton.Attributes.Add( "onclick", closeFunctionCallStringForDateType );
                    string closeFunctionCallStringForCancel = String.Format( "CloseWindowFromContractInfo2('{0}', 'False');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormCancelButton.Attributes.Add( "onclick", closeFunctionCallStringForCancel );
                }
                else if( DateType == DateTypes.X2Award || DateType == DateTypes.X2Completion || DateType == DateTypes.X2Effective || DateType == DateTypes.X2Expiration )
                {
                    string closeFunctionCallStringForDateType = String.Format( "CloseWindowFromContractCreation2('{0}', 'True');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormOkCloseButton.Attributes.Add( "onclick", closeFunctionCallStringForDateType );
                    string closeFunctionCallStringForCancel = String.Format( "CloseWindowFromContractCreation2('{0}', 'False');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormCancelButton.Attributes.Add( "onclick", closeFunctionCallStringForCancel );
                }
                else if( DateType == DateTypes.O2Assignment || DateType == DateTypes.O2Reassignment || DateType == DateTypes.O2EstimatedCompletion || DateType == DateTypes.O2Action || DateType == DateTypes.O2Audit || DateType == DateTypes.O2Return || DateType == DateTypes.O2Received )
                {
                    string closeFunctionCallStringForDateType = String.Format( "CloseWindowFromOfferRecord2('{0}', 'True');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormOkCloseButton.Attributes.Add( "onclick", closeFunctionCallStringForDateType );
                    string closeFunctionCallStringForCancel = String.Format( "CloseWindowFromOfferRecord2('{0}', 'False');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormCancelButton.Attributes.Add( "onclick", closeFunctionCallStringForCancel );
                }
                else if( DateType == DateTypes.C2PricelistModificationEffective )
                {
                    string closeFunctionCallStringForDateType = String.Format( "CloseWindowFromItemInfo2('{0}', 'True');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormOkCloseButton.Attributes.Add( "onclick", closeFunctionCallStringForDateType );
                    string closeFunctionCallStringForCancel = String.Format( "CloseWindowFromItemInfo2('{0}', 'False');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormCancelButton.Attributes.Add( "onclick", closeFunctionCallStringForCancel );
                }
                else if( DateType == DateTypes.InsurancePolicyEffective || DateType == DateTypes.InsurancePolicyExpiration )
                {
                    string closeFunctionCallStringForDateType = String.Format( "CloseWindowFromContractVendor('{0}', 'True');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormOkCloseButton.Attributes.Add( "onclick", closeFunctionCallStringForDateType );
                    string closeFunctionCallStringForCancel = String.Format( "CloseWindowFromContractVendor('{0}', 'False');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormCancelButton.Attributes.Add( "onclick", closeFunctionCallStringForCancel );
                }
                else if( DateType == DateTypes.ProjectionStart || DateType == DateTypes.ProjectionEnd )
                {
                    string closeFunctionCallStringForDateType = String.Format( "CloseWindowFromSBAProjection('{0}', '{1}', 'True');", Enum.GetName( typeof( DateTypes ), DateType ), ReceivingControlClientId );
                    FormOkCloseButton.Attributes.Add( "onclick", closeFunctionCallStringForDateType );
                    string closeFunctionCallStringForCancel = String.Format( "CloseWindowFromSBAProjection('{0}', '{1}', 'False');", Enum.GetName( typeof( DateTypes ), DateType ), ReceivingControlClientId );
                    FormCancelButton.Attributes.Add( "onclick", closeFunctionCallStringForCancel );
                }
                else
                {
                    string closeFunctionCallStringForDateType = String.Format( "CloseWindow('{0}', 'True');", Enum.GetName( typeof( DateTypes ), DateType ) );
                    FormOkCloseButton.Attributes.Add( "onclick", closeFunctionCallStringForDateType );
                    string closeFunctionCallStringForCancel = String.Format( "CloseWindow('{0}', 'False');", Enum.GetName( typeof( DateTypes ), DateType ) ); 
                    FormCancelButton.Attributes.Add( "onclick", closeFunctionCallStringForCancel );
                }
            }

            if( Session[ GetSessionYearListName() ] == null )
            {
                int futureYears = 1;
                int backYears = 1;

                if( _bUnlimited == true )
                {
                    int dateTypeIndex = GetDateTypeIndexFromDateType( DateType );
                    futureYears = _futureYears[ dateTypeIndex ];
                    backYears = _backYears[ dateTypeIndex ];
                }
                else
                {
                    int minYear = MinAllowedDate.Year;
                    int maxYear = MaxAllowedDate.Year;
                    backYears = Math.Max( 0, currentYear - minYear );
                    futureYears = Math.Max( 0, maxYear - currentYear );
                }


                for( int i = currentYear - backYears; i <= currentYear + futureYears; i++ )
                {
                    YearList.Add( i );
                }

                Session[ GetSessionYearListName() ] = YearList;
            }
            else
            {
                YearList = ( ArrayList )Session[ GetSessionYearListName() ];
            }


            if( Session[ GetSessionMonthListName() ] == null )
            {
                for( int m = 1; m <= 12; m++ )
                {
                    MonthList.Add( DateTime.Parse( String.Format( "{0}/1/{1}", m, currentYear ) ).ToString( "MMMM" ) );                        
                }
                Session[ GetSessionMonthListName() ] = MonthList;
            }
            else
            {
                MonthList = ( ArrayList )Session[ GetSessionMonthListName() ];
            }


            // bind and select initial year and month in combos
            if( IsPostBack == false )
            {
                // add years to combo
                cbYears.DataSource = YearList;
                cbYears.DataBind();
               
                if( cbYears.Items.Contains( new ListItem( initialDate.Year.ToString() ) ) )
                    cbYears.SelectedValue = initialDate.Year.ToString();
                else
                    cbYears.SelectedValue = currentYear.ToString();

                // add months to combo
                cbMonths.DataSource = MonthList;
                cbMonths.DataBind();

                cbMonths.SelectedValue = initialDate.ToString( "MMMM" );
            }

        }


        protected void calendar1_OnPreRender( object sender, EventArgs e )
        {
            Label calendarCaption = ( Label )FindControl( "CalendarCaption" );
            if( calendarCaption != null )
            {
                calendarCaption.Text = GetCaption();
            }
        }
   
        protected void calendar1_DayRender( object sender, DayRenderEventArgs e )
        {
            if( _bUnlimited == false )
            {
                if( e.Day.Date < MinAllowedDate || e.Day.Date > MaxAllowedDate )
                {
                    e.Day.IsSelectable = false;
                }
            }
        }

        protected void calendar1_SelectionChanged( object sender, EventArgs e )
        {
            Session[ Enum.GetName( typeof( DateTypes ), DateType ) ] = calendar1.SelectedDate.ToString();
            if( cbYears.Items.Contains( new ListItem( calendar1.SelectedDate.Year.ToString() )) != true )
            {
                ResetYearData( calendar1.SelectedDate.Year );
            }
            cbYears.SelectedValue = calendar1.SelectedDate.Year.ToString();
            cbMonths.SelectedValue = calendar1.SelectedDate.ToString( "MMMM" );
            
            // save for client side access
            HiddenField selectedDateHiddenField = ( HiddenField )form1.FindControl( "SelectedDate" );
            if( selectedDateHiddenField != null )
            {
                selectedDateHiddenField.Value = calendar1.SelectedDate.ToShortDateString();
            }           
        }

        protected void cbYears_TextChanged( object sender, EventArgs e )
        {
            DateTime newDate = new DateTime( Int32.Parse( cbYears.SelectedValue ), calendar1.SelectedDate.Month, calendar1.SelectedDate.Day );
            Session[ Enum.GetName( typeof( DateTypes ), DateType ) ] = newDate;
            calendar1.VisibleDate = newDate;
            calendar1.SelectedDate = newDate;

        }

        // user is scolling through months, keep combos updated
        protected void calendar1_VisibleMonthChanged( object sender, MonthChangedEventArgs e )
        {
            DateTime newDate = AdjustDay( calendar1.SelectedDate, e.NewDate );
            Session[ Enum.GetName( typeof( DateTypes ), DateType ) ] = newDate;
            if( cbYears.Items.Contains( new ListItem( e.NewDate.Year.ToString() ) ) != true )
            {
                ResetYearData( e.NewDate.Year );
            }
            cbYears.SelectedValue = e.NewDate.Year.ToString();
            cbMonths.SelectedValue = e.NewDate.ToString( "MMMM" );

            calendar1.SelectedDate = newDate;
        }

        protected void cbMonths_TextChanged( object sender, EventArgs e )
        {
            DateTime newDate = DateTime.Parse( String.Format( "{0} 1 {1}", cbMonths.SelectedValue, calendar1.SelectedDate.Year ) );
            DateTime adjustedDate = AdjustDay( calendar1.SelectedDate, newDate );
            Session[ Enum.GetName( typeof( DateTypes ), DateType ) ] = adjustedDate;
            calendar1.VisibleDate = adjustedDate;
            calendar1.SelectedDate = adjustedDate;
        }

        // change the selected day to 1 if the old date falls off the end of the month
        private DateTime AdjustDay( DateTime oldDate, DateTime newDate )
        {
            DateTime returnDate;

            if( DateTime.DaysInMonth( newDate.Year, newDate.Month ) < oldDate.Day  )
                returnDate = new DateTime( newDate.Year, newDate.Month, 1 );
            else
                returnDate = new DateTime( newDate.Year, newDate.Month, oldDate.Day );

            return ( returnDate );
        }


        private void ResetYearData( int newYear )
        {
            int minYear = ( int )YearList[ 0 ];
            int maxYear = ( int )YearList[ YearList.Count - 1 ];

            if( newYear < minYear )
            {
                YearList.Clear();

                for( int i = newYear; i <= maxYear; i++ )
                {
                    YearList.Add( i );
                }

            }
            else if( newYear > maxYear )
            {
                for( int i = maxYear + 1; i <= newYear; i++ )
                {
                    YearList.Add( i );
                }
            }
 
            cbYears.DataSource = null;
            cbYears.DataSource = YearList;
            cbYears.DataBind();
        }

        private string GetSessionYearListName()
        {
            return( String.Format( "{0}YearList", Enum.GetName( typeof( DateTypes ), DateType ) ));
        }

        private string GetSessionMonthListName()
        {
            return( String.Format( "{0}MonthList", Enum.GetName( typeof( DateTypes ), DateType ) ));
        }
    }
}
