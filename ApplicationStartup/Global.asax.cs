using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using System.Threading;
using System.Security.Principal;

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.NACCMBrowser.DBInterface;

namespace VA.NAC.CM.ApplicationStartup
{
    public class Global : System.Web.HttpApplication
    {
        private NameValueCollection _appSettings = null;
 //       private static IEModeHttpModule IEMode;
 //       private static bool _bAddedHeader = false;

        private VA.NAC.Logging.NACLog _log = null;

        private static IEModeHttpModule IEMode;
        private static bool _bAddedHeader = false;

        protected void Application_Start( object sender, EventArgs e )
        {
            IEMode = new IEModeHttpModule();

            _appSettings = ( NameValueCollection )System.Web.Configuration.WebConfigurationManager.AppSettings;
            Config.Init( _appSettings );

       //     ScriptManager.ScriptResourceMapping.AddDefinition( "jquery", new ScriptResourceDefinition
       //     {
       //         Path = "~/Jquery/jquery-2.1.3.min.js"
       //     } );

            // init the log for the first time in the application with the file type and information
            _log = new VA.NAC.Logging.NACLog( VA.NAC.Logging.LogBase.LoggingType.File );
            _log.InitFileLog( Config.LogFileName, System.Text.Encoding.Default, false, Config.MaxLogFileSize, Config.MaxLogFiles );
            _log.SetDisplayLevel( Config.LoggingLevel );
            _log.SetCategory( VA.NAC.Logging.LogBase.Category.GUI );
            _log.SetContext( "ApplicationStartup Global", this.GetType() );
            string msg = "Called Application_Start()";

            _log.WriteLog( msg, VA.NAC.Logging.LogBase.Severity.InformMediumLevel );

            ScriptManager.ScriptResourceMapping.AddDefinition( "jquery", new ScriptResourceDefinition
            {
                Path = "~/Scripts/JQuery/jquery-3.7.1.min.js"
            } );

        }

        public override void Init()
        {
            base.Init();

            if( _bAddedHeader == false )
            {
                if( IEMode != null )
                {
                    IEMode.Init( this );
                    _bAddedHeader = true;
                }
            }
        }

        protected void Session_Start( object sender, EventArgs e )
        {
            Session.Timeout = 180; // confer with IIS website timeout as well
            Session[ "FailsafeNoUser" ] = "false";  // app will not work anyway, this only assists with graceful exit

            try
            {
            BrowserSecurity2 bs = new BrowserSecurity2();
            Session[ "BrowserSecurity" ] = bs;
            
            Session[ "ContractDB" ] = new ContractDB( bs.UserInfo.UserId, bs.UserInfo.LoginName, bs.UserInfo.OldUserId );
            Session[ "DrugItemDB" ] = new DrugItemDB( bs.UserInfo.UserId, bs.UserInfo.LoginName, bs.UserInfo.OldUserId );
            Session[ "ItemDB" ] = new ItemDB( bs.UserInfo.UserId, bs.UserInfo.LoginName, bs.UserInfo.OldUserId );
            Session[ "ExportDB" ] = new ExportDB( bs.UserInfo.UserId, bs.UserInfo.LoginName, bs.UserInfo.OldUserId );
            Session[ "OfferDB" ] = new OfferDB( bs.UserInfo.UserId, bs.UserInfo.LoginName, bs.UserInfo.OldUserId );

            Session[ "CMGlobals" ] = new CMGlobals();
            
        }
            catch( Exception ex )
            {
                Session[ "StartupException" ] = ex.Message;

                if( ex.Message != null )
                {
                    if( ex.Message.Contains( "not authorized" ) == true || ex.Message.Contains( "not active" ) == true )
                    {
                        Session[ "FailsafeNoUser" ] = "true";
                    }
                }

                HandleStartupError( sender, e );
            }
        
        }

        protected void Application_Error( object sender, EventArgs e )
        {
            if (HttpContext.Current != null)
            {
                Session["UnhandledException"] = Server.GetLastError();
            }
            else
            {
                Exception ex = Server.GetLastError();
                if (ex != null)
        {
                    string tmp = ex.Message;
                }
            }

            HandleStartupError( sender, e );
        }

        protected void HandleStartupError( object sender, EventArgs e )
        {
            Server.Transfer( "NACCMStartupErrorPage.aspx" );           
        }

        protected void Application_BeginRequest( object sender, EventArgs e )
        {
           // HttpContext.Current.Response.AddHeader( "p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"" ); took out 3/29/2021 
        }

        protected void Application_AuthenticateRequest( object sender, EventArgs e )
        {

        }

        protected void Session_End( object sender, EventArgs e )
        {
            Session[ "BrowserSecurity" ] = null;
            Session[ "ContractDB" ] = null;
            Session[ "DrugItemDB" ] = null;
            Session[ "ItemDB" ] = null;
            Session[ "ExportDB" ] = null;
            Session[ "OfferDB" ] = null;
            Session[ "CMGlobals" ] = null;

         //  Server.Transfer( "NCM.aspx", true); // Response.Redirect( "NCM.aspx" );
        }

        protected void Application_End( object sender, EventArgs e )
        {

        }
    }
}