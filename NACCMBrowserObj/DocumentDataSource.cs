using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using System.Security;

using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // sql data source with common events, connection and sp parameters
    [Serializable]
    public class DocumentDataSource : DBCommon, ISerializable
    {
        [NonSerialized]
        private BrowserSecurity2 _browserSecurity = null;

        public DocumentDataSource( BrowserSecurity2 browserSecurity, TargetDatabases targetDatabase, bool bAddSecurityDatabaseParms )
            :base( targetDatabase )
        {
            if( Config.WasLoaded() == true )
            {
                MakeConnectionString();
            }
            else
            {
                throw new Exception( "Cannot instantiate DocumentDataSource. Config was not loaded." );
            }
           
            _browserSecurity = browserSecurity;

            base.SetEventOwnerName( "DocumentDataSource" );
            base.Selecting += new SqlDataSourceSelectingEventHandler( DocumentDataSource_Selecting );
            base.Inserting += new SqlDataSourceCommandEventHandler( DocumentDataSource_Inserting );
            base.Updating += new SqlDataSourceCommandEventHandler( DocumentDataSource_Updating );
            base.Deleting += new SqlDataSourceCommandEventHandler( DocumentDataSource_Deleting );

            Parameter guidParameter = new Parameter( "currentUser" );
            guidParameter.DefaultValue = _browserSecurity.UserInfo.UserId.ToString();
            base.SelectParameters.Add( guidParameter );
            base.UpdateParameters.Add( guidParameter );
            base.InsertParameters.Add( guidParameter );
            base.DeleteParameters.Add( guidParameter );

            if( bAddSecurityDatabaseParms == true )
            {
                Parameter securityServerNameParameter = new Parameter( "SecurityServerName" );
                securityServerNameParameter.DefaultValue = Config.SecurityDatabaseServer;
                Parameter securityDatabaseNameParameter = new Parameter( "SecurityDatabaseName" );
                securityDatabaseNameParameter.DefaultValue = Config.SecurityDatabase;

                base.UpdateParameters.Add( securityServerNameParameter );
                base.InsertParameters.Add( securityServerNameParameter );
                base.DeleteParameters.Add( securityServerNameParameter );

                base.UpdateParameters.Add( securityDatabaseNameParameter );
                base.InsertParameters.Add( securityDatabaseNameParameter );
                base.DeleteParameters.Add( securityDatabaseNameParameter );
            }

            //allow a select even with a null parameter - I think we can handle it, don't you?
            base.CancelSelectOnNullParameter = false;
        }
 
        void DocumentDataSource_Deleting( object sender, SqlDataSourceCommandEventArgs e )
        {
            SqlParameter guidParameter = ( SqlParameter )e.Command.Parameters[ "@currentUser" ];
            string guidString = guidParameter.Value.ToString();
            guidParameter.SqlDbType = SqlDbType.UniqueIdentifier;
            guidParameter.SqlValue = new Guid( guidString );
        }

        void DocumentDataSource_Updating( object sender, SqlDataSourceCommandEventArgs e )
        {
            SqlParameter guidParameter = ( SqlParameter )e.Command.Parameters[ "@currentUser" ];
            string guidString = guidParameter.Value.ToString();
            guidParameter.SqlDbType = SqlDbType.UniqueIdentifier;
            guidParameter.SqlValue = new Guid( guidString );
        }

        void DocumentDataSource_Inserting( object sender, SqlDataSourceCommandEventArgs e )
        {
            SqlParameter guidParameter = ( SqlParameter )e.Command.Parameters[ "@currentUser" ];
            string guidString = guidParameter.Value.ToString();
            guidParameter.SqlDbType = SqlDbType.UniqueIdentifier;
            guidParameter.SqlValue = new Guid( guidString );
        }

        void DocumentDataSource_Selecting( object sender, SqlDataSourceSelectingEventArgs e )
        {
            SqlParameter guidParameter = ( SqlParameter )e.Command.Parameters[ "@currentUser" ];
            string guidString = guidParameter.Value.ToString();
            guidParameter.SqlDbType = SqlDbType.UniqueIdentifier;
            guidParameter.SqlValue = new Guid( guidString );
        }

        #region ISerializable Members

        [SecurityCritical]
        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );

            _browserSecurity.GetObjectData( info, context );
        }

        public DocumentDataSource( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
            _browserSecurity = new BrowserSecurity2( info, context );
      //      RestoreDelegatesAfterDeserialization( this );  this should be done by the caller after deserialization, not here. commented out 10/16/2015 $$$ 
        }

        #endregion
    }
}
