using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;

using VA.NAC.Application.SharedObj;

namespace VA.NAC.ReportManager
{
    [Serializable]
    public class Report
    {
        private string _reportName = "";
        private string _reportPath = "";
        private string _reportExecutionPath = "";

        private ArrayList _parameterList = null;

        public string ReportPath
        {
            get { return _reportPath; }
            set { _reportPath = value; }
        }

        public ArrayList ParameterList
        {
            get { return _parameterList; }
            set { _parameterList = value; }
        }

        public string ReportName
        {
            get { return _reportName; }
            set { _reportName = value; }
        }

        public string ReportExecutionPath
        {
            get { return _reportExecutionPath; }
            set { _reportExecutionPath = value; }
        }

        public Report( string reportPath )
        {
            _reportPath = reportPath;
        }

        public Report( string reportName, string reportPath, string reportExecutionPath )
        {
            _reportName = reportName;
            _reportPath = reportPath;
            _reportExecutionPath = reportExecutionPath;
        }

        public void AddParameter( string key, string value )
        {
            CMReportParameter newParm = new CMReportParameter( key, value );

            if( _parameterList == null )
            {
                _parameterList = new ArrayList();
            }

            _parameterList.Add( newParm );

        }

        public void AddStandardParameters()
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy( PrincipalPolicy.WindowsPrincipal );
            WindowsPrincipal currentPrincipal = ( WindowsPrincipal )Thread.CurrentPrincipal;

            AddParameter( "ReportUserLoginId", currentPrincipal.Identity.Name );

            AddParameter( "NACCMServerName", Config.ContractManagerDatabaseServer );
            AddParameter( "NACCMDatabaseName", Config.ContractManagerDatabase );

            AddParameter( "SecurityServerName", Config.SecurityDatabaseServer );
            AddParameter( "SecurityDatabaseName", Config.SecurityDatabase );

        }

        public void AddReportUserLoginIdParameter()
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy( PrincipalPolicy.WindowsPrincipal );
            WindowsPrincipal currentPrincipal = ( WindowsPrincipal )Thread.CurrentPrincipal;

            AddParameter( "ReportUserLoginId", currentPrincipal.Identity.Name );
        }
    }
}
