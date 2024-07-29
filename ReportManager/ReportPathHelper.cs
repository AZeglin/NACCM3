using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Microsoft.ReportingServices.Common;
using Microsoft.ReportingServices.Interfaces;
using Microsoft.Reporting.Common;
using Microsoft.ReportingServices.WebServer;
using Microsoft.ReportingServices.Library;

using VA.NAC.Application.SharedObj;

namespace VA.NAC.ReportManager
{
    public class ReportPathHelper
    {
        private string _reportPath = "";

        public string ReportPath
        {
            get
            {
                return _reportPath;
            }

            set
            {
                _reportPath = value;
            }
        }

        private string _reportingServicesPath = "";

        public string ReportingServicesPath
        {
            get
            {
                return _reportingServicesPath;
            }

            set
            {
                _reportingServicesPath =  value ;
            }
        }

        public ReportPathHelper()
        {
            CMGlobals cmGlobals = ( CMGlobals )HttpContext.Current.Session[ "CMGlobals" ];

            if( cmGlobals.WasFullPathUsedToAccessNACCM() == false )
            {
                // non default instance name provided
                if( Config.ReportingServicesInstanceName.Length > 0 && Config.ReportingServicesInstanceName.CompareTo( "MSSQLSERVER" ) != 0 )
                {
                    _reportPath = string.Format( "http://{0}/Reports_{1}", Config.ReportingServicesServerName, Config.ReportingServicesInstanceName );
                    _reportingServicesPath = string.Format( "http://{0}/ReportServer_{1}", Config.ReportingServicesServerName, Config.ReportingServicesInstanceName );

                }
                else
                {
                    _reportPath = string.Format( "http://{0}/Reports", Config.ReportingServicesServerName );
                    _reportingServicesPath = string.Format( "http://{0}/ReportServer", Config.ReportingServicesServerName );
                }

            }
            else
            {
                // non default instance name provided
                if( Config.ReportingServicesInstanceName.Length > 0 && Config.ReportingServicesInstanceName.CompareTo( "MSSQLSERVER" ) != 0 )
                {
                    _reportPath = string.Format( "http://{0}.vha.med.va.gov/Reports_{1}", Config.ReportingServicesServerName, Config.ReportingServicesInstanceName );
                    _reportingServicesPath = string.Format( "http://{0}.vha.med.va.gov/ReportServer_{1}", Config.ReportingServicesServerName, Config.ReportingServicesInstanceName );

                }
                else
                {
                    _reportPath = string.Format( "http://{0}.vha.med.va.gov/Reports", Config.ReportingServicesServerName );
                    _reportingServicesPath = string.Format( "http://{0}.vha.med.va.gov/ReportServer", Config.ReportingServicesServerName );
                }
            }

        }
    }
}