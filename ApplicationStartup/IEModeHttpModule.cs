using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace VA.NAC.CM.ApplicationStartup
{
    public class IEModeHttpModule : IHttpModule
    {
        public void Init( HttpApplication context )
        {
            context.PreSendRequestHeaders += (sender, e) => DisableCompatibilityModeIfApplicable();
        }

        private void DisableCompatibilityModeIfApplicable()
        {
            if( IsIE && IsPage )
                DisableCompatibilityMode();
        }

        // comment said to exclude responses that are redirects
        private void DisableCompatibilityMode()
        {
            var response = Context.Response;
            response.AddHeader( "X-UA-Compatible", "IE=edge" );
            //     response.AddHeader( "p3p", "CP=\"CAO PSA OUR\"" );
            //      response.AddHeader( "P3P", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"" );


        }

        private bool IsIE 
        { 
            get 
            { 
                return Context.Request.Browser.IsBrowser( "InternetExplorer" ); 
            } 
        }

        private bool IsPage 
        { 
            get 
            { 
                return Context.Handler is Page; 
            } 
        }

        private HttpContext Context 
        { 
            get 
            { 
                return HttpContext.Current; 
            } 
        }

        public void Dispose() { }
    }
}