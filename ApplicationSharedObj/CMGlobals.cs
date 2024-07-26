using System;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.SessionState;

using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.Application.SharedObj
{
 
    [Serializable]
    public class CMGlobals
    {
        // SBA Plan related constants
        public const int NOPLANID = 511;
        public const int PLANTYPEFORNOPLAN = 4;
        public const int PLANTYPEFORCOMPANY = 1;  // NAC requested that instead of initially displaying "no plan" as the default, that the first choice of "company" should be displayed.

        // record ID's for countries that have states required
        public const int COUNTRYIDUSA = 239;
        public const int COUNTRYIDCANADA = 42;

        private int _clientScreenHeight = 0;

        public int ClientScreenHeight
        {
            get { return _clientScreenHeight; }
            set { _clientScreenHeight = value; }
        }
        private int _clientScreenWidth = 0;

        public int ClientScreenWidth
        {
            get { return _clientScreenWidth; }
            set { _clientScreenWidth = value; }
        }

        public static string Trim( object inString )
        {
            if( inString != System.DBNull.Value )
            {
                string outString = ( string )inString;
                return ( outString.Trim() );
            }
            else
            {
                return ( string.Empty );
            }
        }

        private string _requestUrl = "";

        public string RequestUrl
        {
            get { return _requestUrl; }
            set { _requestUrl = value; }
        }

        public CMGlobals()
        {
            if( HttpContext.Current.Request != null )
            {
                _requestUrl = HttpContext.Current.Request.Url.AbsoluteUri;
            }
        }

        // presuming that the full path contains va.gov
        // this is presumed:
        //http://ammhinprdapp1.vha.med.va.gov/NCM.aspx OR
        //http://ammhinprdapp1/NCM.aspx
        // may be used to access the app
        public bool WasFullPathUsedToAccessNACCM()
        {
            if( _requestUrl.Contains( "va.gov" ) == true )
                return ( true );
            else
                return ( false );
        }

        public static decimal GetMoneyFromString( string moneyString, string fieldName )
        {
            string[] moneyParts;
            string moneyPart = "";
            decimal moneyNumber = 0;

            if( moneyString.Contains( "$" ) == true )
            {
                moneyParts = moneyString.Split( '$' );
                moneyPart = moneyParts[ 1 ];
            }
            else
            {
                moneyPart = moneyString;
            }

            if( decimal.TryParse( moneyPart, out moneyNumber ) == true )
            {
                return( moneyNumber );
            }
            else
            {
                throw new Exception( string.Format( "{0} is not in a correct format.", fieldName ));
            }
        }

        public static string GetLastModificationTypeDescription( object lastModificationTypeString )
        {
            string lastModificationTypeDescription = "";
            string lastModificationType = "";

            if( lastModificationTypeString != System.DBNull.Value )
            {
                lastModificationType = ( string )lastModificationTypeString;

                if( lastModificationType.CompareTo( "U" ) == 0 )
                    lastModificationTypeDescription = "Upload";
                else if( lastModificationType.CompareTo( "I" ) == 0 )
                    lastModificationTypeDescription = "Initial Extract From PBM";
                else if( lastModificationType.CompareTo( "C" ) == 0 )
                    lastModificationTypeDescription = "Contracting Officer Edit";
                else if( lastModificationType.CompareTo( "P" ) == 0 )
                    lastModificationTypeDescription = "PBM Edit";
                else if( lastModificationType.CompareTo( "V" ) == 0 )
                    lastModificationTypeDescription = "Adjust Sequence";
                else if( lastModificationType.CompareTo( "O" ) == 0 )
                    lastModificationTypeDescription = "Copy Item";
                else if( lastModificationType.CompareTo( "Y" ) == 0 )
                    lastModificationTypeDescription = "Copy Contract";
                else if( lastModificationType.CompareTo( "N" ) == 0 )
                    lastModificationTypeDescription = "NDC Change";
                else if( lastModificationType.CompareTo( "R" ) == 0 )
                    lastModificationTypeDescription = "Restored";
                else if( lastModificationType.CompareTo( "E" ) == 0 )
                    lastModificationTypeDescription = "Contract Extension";
                else if( lastModificationType.CompareTo( "T" ) == 0 )
                    lastModificationTypeDescription = "Contract Termination";  // added 10/19/2017 includes editing prices for expiration or cancellation date via contract date change
                else
                    lastModificationTypeDescription = "Unknown";
            }
            
            return ( lastModificationTypeDescription );
        }

        public static string GetMedSurgLastModificationTypeDescription( object lastModificationTypeString )
        {
            string lastModificationTypeDescription = "";
            string lastModificationType = "";

            if( lastModificationTypeString != System.DBNull.Value )
            {
                lastModificationType = ( string )lastModificationTypeString;

                if( lastModificationType.CompareTo( "U" ) == 0 )
                    lastModificationTypeDescription = "Upload";
                else if( lastModificationType.CompareTo( "I" ) == 0 )
                    lastModificationTypeDescription = "Initial Conversion From Active";
                else if( lastModificationType.CompareTo( "H" ) == 0 )
                    lastModificationTypeDescription = "Initial Conversion From History";
                else if( lastModificationType.CompareTo( "C" ) == 0 )
                    lastModificationTypeDescription = "Contracting Officer Edit";              
                else if( lastModificationType.CompareTo( "V" ) == 0 )
                    lastModificationTypeDescription = "Adjust Sequence";
                else if( lastModificationType.CompareTo( "O" ) == 0 )
                    lastModificationTypeDescription = "Copy Item";
                else if( lastModificationType.CompareTo( "Y" ) == 0 )
                    lastModificationTypeDescription = "Copy Contract";         
                else if( lastModificationType.CompareTo( "R" ) == 0 )
                    lastModificationTypeDescription = "Restored";
                else if( lastModificationType.CompareTo( "E" ) == 0 )
                    lastModificationTypeDescription = "Contract Extension";
                else
                    lastModificationTypeDescription = "Unknown";
            }

            return ( lastModificationTypeDescription );
        }


        public static void AddKeepAlive( Page currentPage, int keepAliveIntervalMilliseconds )
        {           
            string script = @"
                <script type='text/javascript'>

                      function Reconnect() {
                          var currentdate = new Date();      
                          var hours = ('0' + currentdate.getHours()).slice(-2);                                          
                          var minutes = ('0' + currentdate.getMinutes()).slice(-2);      
                          var seconds = ('0' + currentdate.getSeconds()).slice(-2);
                          var milliseconds = ('00' + currentdate.getMilliseconds()).slice(-3);          
                          var strTime = hours + ':' + minutes + ':' + seconds + '.' + milliseconds + '';
            
                          var monthPlusOne = currentdate.getMonth() + 1;
                          var month = ('0' + monthPlusOne).slice(-2);     
                          var day = ('0' + currentdate.getDate()).slice(-2);  
                          var year = currentdate.getFullYear();
      
 			              var dateTimeString = month + '/' + day + '/' + year + ' ' + strTime
 			                    
                          var img = new Image(1, 1);
                          img.src = 'KeepAlive.aspx?q=' + dateTimeString + ' ';
                        }; " +
                
                @"window.setInterval( Reconnect, " + 
                    keepAliveIntervalMilliseconds.ToString()+ @"); " + 

                @"</script> ";  // call function after every interval

            ClientScriptManager clientScriptManager = currentPage.ClientScript;
            clientScriptManager.RegisterStartupScript( currentPage.GetType(), "Reconnect", script, false );   //RegisterClientScriptBlock

        }

        public static bool IsAccessPointImplemented( Page thePage, string accessPointName )
        {
            bool bSuccess = false;

            if( thePage.Session[ accessPointName ] != null )
            {
                if( ( ( bool )thePage.Session[ accessPointName ] ) == true )
                    bSuccess = true;
            }

            return ( bSuccess );
        }

        public static DateTime GetExpirationDate( DateTime effectiveDate )
        {
            DateTime expirationDate;

            System.Globalization.Calendar cal = CultureInfo.CurrentCulture.Calendar;

            expirationDate = cal.AddYears( effectiveDate, 5 );
            expirationDate = cal.AddDays( expirationDate, -1 );

            return ( expirationDate );
        }

        public static DateTime GetNextEffectiveDate( DateTime awardDate )
        {
            DateTime effectiveDate;
            int monthNumber = 0;
            int dayNumber = 0;
            int yearNumber = 0;

            if( awardDate.Day >= 1 && awardDate.Day <= 14 )
            {
                dayNumber = 15;
                monthNumber = awardDate.Month;
                yearNumber = awardDate.Year;
            }
            else
            {
                dayNumber = 1;
                if( awardDate.Month == 12 )
                {
                    yearNumber = awardDate.Year + 1;
                    monthNumber = 1;
                }
                else
                {
                    yearNumber = awardDate.Year;
                    monthNumber = awardDate.Month + 1;
                }
            }

            effectiveDate = DateTime.Parse( string.Format( "{0}/{1}/{2}", monthNumber, dayNumber, yearNumber ) );

            return ( effectiveDate );
        }

        public static void SelectTextInDropDownList( ref DropDownList dropDownList, String selectedText )
        {
            ListItem selectedItem = dropDownList.Items.FindByText( selectedText );
            if( selectedItem != null )
            {
                dropDownList.SelectedIndex = dropDownList.Items.IndexOf( selectedItem );
            }
        }

        public static void SelectTextInDropDownListWithAdd( ref DropDownList dropDownList, String selectedText )
        {
            ListItem selectedItem = dropDownList.Items.FindByText( selectedText );
            if( selectedItem != null )
            {
                dropDownList.SelectedIndex = dropDownList.Items.IndexOf( selectedItem );
            }
            else
            {
                ListItem newItem = new ListItem( selectedText );
                dropDownList.Items.Add( newItem );
                dropDownList.SelectedIndex = dropDownList.Items.IndexOf( newItem );
            }
        }

        public static string GetSelectedTextFromDropDownList( ref DropDownList dropDownList )
        {
            int selectedIndex;
            ListItem selectedItem;
            string selectedText = "";

            if( dropDownList != null )
            {
                selectedIndex = dropDownList.SelectedIndex;
                if( selectedIndex >= 0 )
                {
                    selectedItem = dropDownList.Items[ selectedIndex ];
                    if( selectedItem != null )
                    {
                        selectedText = selectedItem.Text;
                    }
                }
            }
            return ( selectedText );
        }

        public static Control FindControlRecursive( Control root, string id )
        {
            if( ( root.ID != null ) && ( root.ID == id ) ) return root;

            foreach( Control ctrl in root.Controls )
            {
                Control found = FindControlRecursive( ctrl, id );
                if( found != null ) return found;
            }

            return null;
        }

        public static Regex NonPrintableRegex = new Regex( "[\x00-\x1F]", RegexOptions.None );

        public static string ReplaceNonPrintableCharacters( string inString, string replacementString )
        {
            return ( NonPrintableRegex.Replace( inString, replacementString ) );
        }

        public static string ReplaceNonPrintableCharacters( string inString )
        {
            return ( NonPrintableRegex.Replace( inString, " " ) );
        }

        public static Regex QuoteRegex = new Regex( "[\']", RegexOptions.None );

        public static string ReplaceQuote( string inString, string replacementString )
        {
            return ( QuoteRegex.Replace( inString, replacementString ) );
        }

        public static string ReplaceQuote( string inString )
        {
            return ( QuoteRegex.Replace( inString, " " ) );
        }

        public static string RestoreQuote( string inString, string replacementString  )
        {
            return ( inString.Replace( replacementString, "'" ));
        }

        // replace latin chars with their regular ascii lookalike 
        public static string ReplaceLatinWithAscii( string inString )
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( inString.Normalize( NormalizationForm.FormKD ).Where( x => x < 128 ).ToArray() );
            return(  sb.ToString() );
        }

        public static bool IsBasicLetter( char c ) 
        {
            return( c >= 'a' && c <= 'z' ) || ( c >= 'A' && c <= 'Z' );
        }

        public static bool ContainsNonBasicLetter( string inString )
        {
            return( Regex.IsMatch( inString, @"[^\u0000-\u007F]+" ));    
        }

        public static string MultilineText( string[] textArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < textArray.Count(); i++ )
            {
                sb.AppendLine( textArray[ i ] );
                if( i < textArray.Count() - 1 )
                    sb.Append( "<br>" );
            }

            return ( sb.ToString() );
        }

        public static string MultilineButtonText( string[] textArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < textArray.Count(); i++ )
            {
                sb.AppendLine( textArray[ i ] );
            }

            return ( sb.ToString() );
        }

        public static string MultilineButtonText2( string[] textArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < textArray.Count(); i++ )
            {
                sb.AppendLine( textArray[ i ] );

                if( i < textArray.Count() - 1 )
                    sb.Append( Environment.NewLine );
            }

            return ( sb.ToString() );
        }

        public static void MultilineButtonText( Button button, string[] buttonTextArray ) 
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < buttonTextArray.Count(); i++ )
            {
                sb.AppendLine( buttonTextArray[ i ] );
            }

            button.Text = sb.ToString();
        }

        public static string[] ValidGUIDateFormats()
        {
            return( new string[4] { "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", "M/d/yyyy" } );
        }

        // returns the status, also changes the color of the status control
        public static string GetContractStatus( object expirationDateObj, object completionDateObj, Label statusLabel )
        {
            string theStatus = "";
            DateTime expirationDate;
            DateTime completionDate;
            DateTime nullDate = DateTime.MinValue;

            if( expirationDateObj == null )
            {
                expirationDate = nullDate;
            }
            else if( DateTime.TryParse( expirationDateObj.ToString(), out expirationDate ) != true )
            {
                expirationDate = nullDate;
            }

            if( completionDateObj == null )
            {
                completionDate = nullDate;
            }
            else if( DateTime.TryParse( completionDateObj.ToString(), out completionDate ) != true )
            {
                completionDate = nullDate;
            }

            if( expirationDate.CompareTo( nullDate ) == 0 && completionDate.CompareTo( nullDate ) == 0 )
            {
                theStatus = "Undefined";
            }
            else if( completionDate.CompareTo( DateTime.Now.Date ) < 0 && completionDate.CompareTo( nullDate ) != 0 )
            {
                theStatus = "Canceled";
                if( statusLabel != null )
                {
                    statusLabel.ForeColor = Color.Red;
                }
            }
            else if( expirationDate.CompareTo( DateTime.Now.Date ) < 0 )
            {
                theStatus = "Expired";
                if( statusLabel != null )
                {
                    statusLabel.ForeColor = Color.Red;
                }
            }
            else
            {
                theStatus = "Active";
                if( statusLabel != null )
                {
                    statusLabel.ForeColor = Color.Green;
                }
            }

            return ( theStatus );
        }

        public static void CreateDataSetCache( Page thePage, string name, DataSet ds )
        {
            thePage.Cache.Insert( name, ds, null, Cache.NoAbsoluteExpiration, new TimeSpan( 4, 0, 0 ) ); // expires 4 hours after last use
        }

        public static void ExpireCache( Page thePage, string name )
        {
            thePage.Cache.Remove( name );
        }

        public static void CreateArrayListCache( Page thePage, string name, ArrayList al )
        {
            thePage.Cache.Insert( name, al, null, Cache.NoAbsoluteExpiration, new TimeSpan( 4, 0, 0 ) ); // expires 4 hours after last use
            SaveCacheTimestamp( thePage, name );
        }

        public static void SaveCacheTimestamp( Page thePage, string cacheName )
        {
            // remove existing before insert
            if( thePage.Cache[ GetCacheTimeStampName( cacheName ) ] != null )
            {
                thePage.Cache.Remove( GetCacheTimeStampName( cacheName ) );                
            }

            thePage.Cache.Insert( GetCacheTimeStampName( cacheName ), DateTime.Now.ToString() );
        }

        public static DateTime GetCacheTimestamp( Page thePage, string cacheName )
        {
            DateTime cacheTimeStamp = DateTime.Now;

            if( thePage.Cache[ GetCacheTimeStampName( cacheName ) ] != null )
            {
                cacheTimeStamp = DateTime.Parse( thePage.Cache[ GetCacheTimeStampName( cacheName ) ].ToString() );
            }

            return ( cacheTimeStamp );
        }

        public static string GetCacheTimeStampName( string cacheName )
        {
            return ( string.Format( "CacheTimeStamp{0}", cacheName ) );
        }

        public static void SaveLastModifiedReportDate( Page thePage, DateTime reportLastModifiedDate )
        {
            thePage.Session[ "LastModifiedReportDate" ] = reportLastModifiedDate.ToString();
        }

        public static DateTime GetLastModifiedReportDate( Page thePage )
        {
            DateTime lastModifiedReportDate = DateTime.Now;

            if( thePage != null )
            {
                if( thePage.Session[ "LastModifiedReportDate" ] != null )
                {
                    lastModifiedReportDate = DateTime.Parse( thePage.Session[ "LastModifiedReportDate" ].ToString() );
                }
            }

            return ( lastModifiedReportDate );
        }

        // compare the dated cache with a reference date to determine if the cache should be expired now
        public static bool IsCacheDateExpired( Page thePage, DateTime referenceDateTime, string cacheName )
        {
            bool bIsExpired = true;

            DateTime cacheDateTime = GetCacheTimestamp( thePage, cacheName );

            // if the cache is fresher than the reference, then no need to expire
            if( DateTime.Compare( referenceDateTime, cacheDateTime ) <= 0 )
                bIsExpired = false;

            return ( bIsExpired );
        }

        // the three values that indicate a "complete" status
        public const int AwardedOfferActionId = 10;
        public const int WithdrawnOfferActionId = 7;
        public const int NoAwardOfferActionId = 8;

        public static void CheckIfStartedProperly( Page thePage )
        {
            if( thePage.Session[ "NACCMStartedProperly" ] == null )
            {
                thePage.Response.StatusCode = ( int )System.Net.HttpStatusCode.Forbidden;
                thePage.Response.BufferOutput = true;
                thePage.Response.Redirect( "403A.htm" ); // forbidden error
            }
        }

        // email address validation regex
        // Pattern = name part AND ( ip address OR 
        //			( textual sub domain ( may include additional dots ) AND top level domain ( e.g., com, net ) up to 23 chars long )
        //  ! # $ % * / ? | ^ { } ` ~ & ' + - = _.  chars, which are allowed by the standard ( RFC 2822 ) in the local part, are not allowed by the above expression.

        public const string MatchEmailAddressPattern =
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
             + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				        [0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
             + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				        [0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
             + @"([a-zA-Z0-9]+[\w-]+\.)+[a-zA-Z]{1}[a-zA-Z0-9-]{1,23})$";

        public static bool IsValidEmailAddress( string emailAddress )
        {
            bool bIsValid = false;

            if( emailAddress != null )
            {
                bIsValid = Regex.IsMatch( emailAddress, MatchEmailAddressPattern );
            }

            return ( bIsValid );
        }


    }
}
