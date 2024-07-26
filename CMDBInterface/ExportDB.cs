using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Web.Caching;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Security;

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;

namespace VA.NAC.NACCMBrowser.DBInterface
{
    // provides ado.net access to export releated stored procedures in the logical "export" database
    [Serializable]
    public class ExportDB : DBCommon, ISerializable
    {
    //    private NACLog _log = new NACLog();

        private Guid _currentUserId = Guid.Empty;
        private string _userName = string.Empty;
        private int _oldUserId = -1;

        public ExportDB()
            : base( DBCommon.TargetDatabases.ItemExport )
        {
            //_log.SetCategory( LogBase.Category.DB );
            //_log.SetContext( "ExportDB", this.GetType() );
            //_log.WriteLog( "Calling ExportDB() ctor 1", LogBase.Severity.InformLowLevel );
        }

        public ExportDB( Guid currentUserId, string userName, int oldUserId )
            : base( DBCommon.TargetDatabases.ItemExport )
        {
            //_log.SetCategory( LogBase.Category.DB );
            //_log.SetContext( "ExportDB", this.GetType() );
            //_log.WriteLog( "Calling ExportDB() ctor 2", LogBase.Severity.InformLowLevel );

            _currentUserId = currentUserId;
            _userName = userName;
            _oldUserId = oldUserId;
        }

        public bool ExportItems( string contractNumber, string destinationPath, string exportType, string startDateString, string endDateString, string sourceDatabaseName, string sourceServerName, ref string actualDestinationFilePathAndName )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            actualDestinationFilePathAndName = "";

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //CreateNACCMPriceListExportSpreadsheet
                //(
                //@currentUser uniqueidentifier ,
                //@ContractNumber nvarchar(20)  ,
                //@DestinationPath nvarchar(500),
                //@ExportType nchar(1), 
                //@StartDate nvarchar(10),
                //@EndDate nvarchar(10),
                //@DatabaseName nvarchar(30),
                //@ServerName nvarchar(30),
                //@filepath nvarchar(1000) output
                //)

                SqlCommand cmdCreateSpreadsheetQuery = new SqlCommand( "CreateNACCMPriceListExportSpreadsheet", dbConnection );
                cmdCreateSpreadsheetQuery.CommandType = CommandType.StoredProcedure;
                cmdCreateSpreadsheetQuery.CommandTimeout = 0; /* refresh for drug item export can take a while */


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmDestinationPath = new SqlParameter( "@DestinationPath", SqlDbType.NVarChar, 500 );
                SqlParameter parmExportType = new SqlParameter( "@ExportType", SqlDbType.NVarChar, 1 );
                SqlParameter parmStartDateString = new SqlParameter( "@StartDate", SqlDbType.NVarChar, 10 );
                SqlParameter parmEndDateString = new SqlParameter( "@EndDate", SqlDbType.NVarChar, 10 );
                SqlParameter parmDatabaseName = new SqlParameter( "@DrugItemDatabaseName", SqlDbType.NVarChar, 30 );
                SqlParameter parmServerName = new SqlParameter( "@DrugItemServerName", SqlDbType.NVarChar, 30 );
                
                SqlParameter parmOutputFileAndPath = new SqlParameter( "@filepath", SqlDbType.NVarChar, 1000 );
                parmOutputFileAndPath.Direction = ParameterDirection.Output;

                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;
                parmDestinationPath.Value = destinationPath;
                parmExportType.Value = exportType;
                parmStartDateString.Value = startDateString;
                parmEndDateString.Value = endDateString;
                parmDatabaseName.Value = sourceDatabaseName;
                parmServerName.Value = sourceServerName;

                cmdCreateSpreadsheetQuery.Parameters.Add( parmCurrentUser );
                cmdCreateSpreadsheetQuery.Parameters.Add( parmContractNumber );
                cmdCreateSpreadsheetQuery.Parameters.Add( parmDestinationPath );
                cmdCreateSpreadsheetQuery.Parameters.Add( parmExportType );
                cmdCreateSpreadsheetQuery.Parameters.Add( parmStartDateString );
                cmdCreateSpreadsheetQuery.Parameters.Add( parmEndDateString );
                cmdCreateSpreadsheetQuery.Parameters.Add( parmDatabaseName );
                cmdCreateSpreadsheetQuery.Parameters.Add( parmServerName );
                cmdCreateSpreadsheetQuery.Parameters.Add( parmOutputFileAndPath );

                // connect
                dbConnection.Open();

                cmdCreateSpreadsheetQuery.ExecuteNonQuery();

                if( cmdCreateSpreadsheetQuery.Parameters[ "@filepath" ].Value != DBNull.Value )
                {
                    actualDestinationFilePathAndName = cmdCreateSpreadsheetQuery.Parameters[ "@filepath" ].Value.ToString();
                }
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ExportDB.ExportItems(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        #region ISerializable Members

        [SecurityCritical]
        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );

            info.AddValue( "CurrentUserId", _currentUserId );
            info.AddValue( "UserName", _userName );
            info.AddValue( "OldUserId", _oldUserId );
        }

        protected ExportDB( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            _currentUserId = ( Guid )info.GetValue( "CurrentUserId", typeof( Guid ) );
            _userName = info.GetString( "UserName" );
            _oldUserId = info.GetInt32( "OldUserId" );
            RestoreDelegatesAfterDeserialization( this, "ExportDB" );
        }

        #endregion
    }
}
