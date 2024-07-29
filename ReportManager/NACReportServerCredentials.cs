using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Net;
using System.Web.Security;
using Microsoft.Reporting.WebForms;

namespace VA.NAC.ReportManager
{
    [Serializable]
    public sealed class NACReportServerCredentials :  IReportServerCredentials 
    {
        private string _userName = "";
        private string _password = "";
        private string _domain = "";

        public NACReportServerCredentials( string userName, string password, string domain )
        {
            _userName = userName;
            _password = password;
            _domain = domain;
        }

        public NACReportServerCredentials()
        {
        }

        #region IReportServerCredentials Members

        public bool GetFormsCredentials( out System.Net.Cookie authCookie, out string userName, out string password, out string authority )
        {
            authCookie = null;
            userName = "";
            password = "";
            authority = "";
            return( false );
        }

        public System.Security.Principal.WindowsIdentity ImpersonationUser
        {
            get
            {
                return ( null );
            }
        }

        public System.Net.ICredentials NetworkCredentials
        {
            get
            {
                //return ( new NetworkCredential( _userName, _password, _domain ) );
                return ( null );
            }
        }

        #endregion
    }
}
