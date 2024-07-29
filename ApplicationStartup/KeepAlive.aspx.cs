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
using System.Globalization;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class KeepAlive : System.Web.UI.Page
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            //Response.AddHeader( "Refresh", Convert.ToString( ( Session.Timeout - 1 ) * 60 ));
            //Response.AddHeader( "cache-control", "private" );
            //Response.AddHeader( "Pragma", "No-Cache" );
            //Response.Buffer = true;
            //Response.Expires = 0;
            //Response.ExpiresAbsolute = DateTime.Now;
            DateTime currentDate;
            DateTime previousDate;
            DateTime lastPingTimeOfLastIncrementDate;
            int increment = 0;
            int lastPingTooSoonCount = 0;
            
            CultureInfo enUS = new CultureInfo( "en-US" );
            string dateFormatString = "MM/dd/yyyy HH:mm:ss.fff";

            if( Request.QueryString[ "q" ] != null )
            {
                string currentDateTimeString = Request.QueryString[ "q" ].ToString();
                if( Session[ "LastPing" ] != null )
                {
                    string previousDateTimeString = Session[ "LastPing" ].ToString();

                    if( DateTime.TryParseExact( currentDateTimeString, dateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDate ) == true )
                    {
                        if( DateTime.TryParseExact( previousDateTimeString, dateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out previousDate ) == true )
                        {
                            TimeSpan difference = currentDate.Subtract( previousDate );

                            // possible second instance detected
                            if( difference.TotalMilliseconds < 10000 )   // value must be less than the polling rate -- was running fine at 11000 until Chromium Edge
                            {
                                increment = 1;
                                Session[ "LastPingTimeOfLastIncrement" ] = currentDateTimeString;
                            }
                        }

                        Session[ "LastPing" ] = currentDateTimeString;
                    }
                }
                else
                {
                    Session[ "LastPing" ] = currentDateTimeString;
                }

                // safety to reset all counters if a spurious increment was recorded
                if( Session[ "LastPingTimeOfLastIncrement" ] != null )
                {
                    string lastPingTimeOfLastIncrement = Session[ "LastPingTimeOfLastIncrement" ].ToString();

                    if( DateTime.TryParseExact( lastPingTimeOfLastIncrement, dateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out lastPingTimeOfLastIncrementDate ) == true )
                    {
                        if( DateTime.TryParseExact( currentDateTimeString, dateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDate ) == true )
                        {
                            TimeSpan differenceFromLastIncrement = currentDate.Subtract( lastPingTimeOfLastIncrementDate );

                            // last increment was a while ago, so start count from zero
                            if( differenceFromLastIncrement.TotalSeconds >= 48 )    // value must be greater than twice the polling rate
                            {
                                Session[ "LastPingTooSoonCount" ] = null;
                            }
                        }
                    }
                    Session[ "LastPingTimeOfLastIncrement" ] = currentDateTimeString;
                }
      
                   
                if( Session[ "LastPingTooSoonCount" ] != null )
                {
                    lastPingTooSoonCount = int.Parse( Session[ "LastPingTooSoonCount" ].ToString() );
                    lastPingTooSoonCount = lastPingTooSoonCount + increment;

                    if( lastPingTooSoonCount >= 2 )
                    {
                        // user has 2 windows open, so redirect to error page, upon next request, via master page
                        Session[ "MultipleInstances" ] = "true";   
                    }
                    else
                    {
                    
                        Session[ "LastPingTooSoonCount" ] = lastPingTooSoonCount;
                    }
                }
                else
                {
                    if( increment > 0 )
                        Session[ "LastPingTooSoonCount" ] = increment;
                }
            }
        }
    }
}
