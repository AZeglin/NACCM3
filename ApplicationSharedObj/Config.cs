using System;
using System.Data;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using VA.NAC.Logging;

namespace VA.NAC.Application.SharedObj
{
    // contains config information read in from IIS config at startup, stored in the cache,
    public static class Config
    {
        private static string _logFilePath = "";
        private static string _logFileName = "";
        private static int _maxLogFileSize = 20000000;
        private static int _maxLogFiles = 20;
        private static VA.NAC.Logging.LogBase.Severity _loggingLevel = LogBase.Severity.Debug;
        
        private static string _contractManagerDatabaseServer = "";
        private static string _contractManagerDatabase = "";

        private static string _drugItemDatabaseServer = "";
        private static string _drugItemDatabase = "";

        private static string _medSurgItemExportPath = "";
        private static string _drugItemExportPath = "";

        // these two no longer used
        private static string _medSurgItemExportDatabaseServer = ""; // root item export SP to call DTS
        private static string _medSurgItemExportDatabase = "";

        private static string _exportDatabaseServer = ""; // contains root item export SP to call DTS
        private static string _exportDatabase = "";

        private static string _contractManagerDatabaseCommonUserName = "";
        private static string _contractManagerDatabaseCommonUserPassword = "";

        private static string _reportingServicesServerName = "";
        private static string _reportingServicesInstanceName = "";      

        private static string _securityDatabaseServer = "";
        private static string _securityDatabase = "";

        private static string _securityCommonUserName = "";
        private static string _securityCommonUserPassword = "";

        private static string _itemVersion = "";
        private static string _salesVersion = "";

        public static string ItemVersion
        {
            get { return Config._itemVersion; }
            set { Config._itemVersion = value; }
        }

        public static string SecurityCommonUserName
        {
            get { return Config._securityCommonUserName; }
            set { Config._securityCommonUserName = value; }
        }

        public static string SecurityCommonUserPassword
        {
            get { return Config._securityCommonUserPassword; }
            set { Config._securityCommonUserPassword = value; }
        }
        public static string SecurityDatabase
        {
            get { return Config._securityDatabase; }
            set { Config._securityDatabase = value; }
        }

        public static string SecurityDatabaseServer
        {
            get { return Config._securityDatabaseServer; }
            set { Config._securityDatabaseServer = value; }
        }

        public static string ReportingServicesServerName
        {
            get { return Config._reportingServicesServerName; }
            set { Config._reportingServicesServerName = value; }
        }

        public static string ReportingServicesInstanceName
        {
            get { return Config._reportingServicesInstanceName; }
            set { Config._reportingServicesInstanceName = value; }
        }
            
        public static string ContractManagerDatabaseCommonUserName
        {
            get { return Config._contractManagerDatabaseCommonUserName; }
            set { Config._contractManagerDatabaseCommonUserName = value; }
        }

        public static string ContractManagerDatabaseCommonUserPassword
        {
            get { return Config._contractManagerDatabaseCommonUserPassword; }
            set { Config._contractManagerDatabaseCommonUserPassword = value; }
        }

        public static string DrugItemDatabaseServer
        {
            get 
            { 
                return( _drugItemDatabaseServer ); 
            }
        }

        public static string DrugItemDatabase
        {
            get 
            { 
                return( _drugItemDatabase ); 
            }
        }

        public static bool WasLoaded()
        {
            if(( _contractManagerDatabaseServer.Length > 0 && _contractManagerDatabase.Length > 0 )
                || ( _drugItemDatabaseServer.Length > 0 && _drugItemDatabase.Length > 0 ) )
                return true;
            else
                return false;
        }

        public static void Init( NameValueCollection appSettings )
        {
            try
            {
                _logFilePath = ( string )appSettings[ "logFilePath" ];
                _logFileName = String.Format( @"{0}\{1}", _logFilePath, VA.NAC.Logging.LogBase.CreateTodaysFileName( "LogFile" ) );

                _maxLogFileSize = Int32.Parse( appSettings[ "maxLogFileSize" ] );
                _maxLogFiles = Int32.Parse( appSettings[ "maxLogFiles" ] );

                string loggingLevel = ( string )appSettings[ "loggingLevel" ];
                _loggingLevel = ( VA.NAC.Logging.LogBase.Severity )Int32.Parse( loggingLevel );
                
                _contractManagerDatabaseServer = ( string )appSettings[ "contractManagerDatabaseServerName" ];
                _contractManagerDatabase = ( string )appSettings[ "contractManagerDatabaseName" ];

                _drugItemDatabaseServer = ( string )appSettings[ "drugItemDatabaseServerName" ];
                _drugItemDatabase = ( string )appSettings[ "drugItemDatabaseName" ];

                _medSurgItemExportPath = ( string )appSettings[ "medSurgItemExportDirectoryPath" ];
                _drugItemExportPath = ( string )appSettings[ "drugItemExportDirectoryPath" ];

                _medSurgItemExportDatabaseServer = ( string )appSettings[ "medSurgItemExportDatabaseServerName" ];
                _medSurgItemExportDatabase = ( string )appSettings[ "medSurgItemExportDatabaseName" ];
                _exportDatabaseServer = ( string )appSettings[ "exportDatabaseServerName" ];
                _exportDatabase = ( string )appSettings[ "exportDatabaseName" ];

                _contractManagerDatabaseCommonUserName = ( string )appSettings[ "contractManagerDatabaseCommonUserName" ];
                _contractManagerDatabaseCommonUserPassword = ( string )appSettings[ "contractManagerDatabaseCommonUserPassword" ];

                _reportingServicesServerName = ( string )appSettings[ "reportingServicesServerName" ];
                _reportingServicesInstanceName = ( string )appSettings[ "reportingServicesInstanceName" ];               

                _securityDatabaseServer = ( string )appSettings[ "securityDatabaseServer" ];
                _securityDatabase = ( string )appSettings[ "securityDatabase" ];

                _securityCommonUserName = ( string )appSettings[ "securityCommonUserName" ];
                _securityCommonUserPassword = ( string )appSettings[ "securityCommonPassword" ];

                _itemVersion = ( string )appSettings[ "ItemVersion" ];
                _salesVersion = ( string )appSettings[ "SalesVersion" ];

            }
            catch // mask exceptions at application start up
            {
            }
        }

        public static string LogFilePath
        {
            get
            {
                return ( _logFilePath );
            }
        }

        public static string LogFileName
        {
            get
            {
                return ( _logFileName );
            }
        }

        public static int MaxLogFileSize
        {
            get
            {
                return ( _maxLogFileSize );
            }
        }

        public static int MaxLogFiles
        {
            get
            {
                return ( _maxLogFiles );
            }
        }

        public static VA.NAC.Logging.LogBase.Severity LoggingLevel
        {
            get
            {
                return (_loggingLevel);
            }
        }

        public static string ContractManagerDatabaseServer
        {
            get
            {
                return ( _contractManagerDatabaseServer );
            }
        }

        public static string ContractManagerDatabase
        {
            get
            {
                return ( _contractManagerDatabase );
            }
        }

        public static string MedSurgItemExportPath
        {
            get
            {
                return ( _medSurgItemExportPath );
            }
        }

        public static string DrugItemExportPath
        {
            get
            {
                return ( _drugItemExportPath );
            }
        }

        public static string MedSurgItemExportDatabaseServer
        {
            get
            { 
                return Config._medSurgItemExportDatabaseServer; 
            }
            set
            { 
                Config._medSurgItemExportDatabaseServer = value; 
            }
        }

        public static string MedSurgItemExportDatabase
        {
            get
            { 
                return Config._medSurgItemExportDatabase; 
            }
            set
            { 
                Config._medSurgItemExportDatabase = value; 
            }
        }

        public static string ExportDatabaseServer
        {
            get
            { 
                return Config._exportDatabaseServer; 
            }
            set 
            { 
                Config._exportDatabaseServer = value; 
            }
        }

        public static string ExportDatabase
        {
            get
            { 
                return Config._exportDatabase; 
            }
            set
            { 
                Config._exportDatabase = value; 
            }
        }

        public static string SalesVersion
        {
            get
            {
                return _salesVersion;
            }

            set
            {
                _salesVersion =  value ;
            }
        }
    }
}
