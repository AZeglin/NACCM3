using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.ComponentModel;
using System.ComponentModel.Design;

//using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;

namespace VA.NAC.NACCMBrowser.DBInterface
{
	/// <summary>
	/// Collection of common db related functions and properties
	/// </summary>
    [Serializable]
    public class DBCommon : SqlDataSource, ISerializable, IDisposable
	{
		private string _connectionString = "";
		private string _errorMessage = "";
        private int _commandTimeout = 30;
        private int _rowsReturned = -1;
        private int _bytesReturned = -1;
       
        [NonSerialized]
        private SerializationHelpers _sh = new SerializationHelpers();

        private TargetDatabases _targetDatabase = TargetDatabases.Undefined;

        public TargetDatabases TargetDatabase
        {
            get { return _targetDatabase; }
            set { _targetDatabase = value; }
        }

        
		public enum XmlReturnFormat
		{
			DataOnly=0,
			DataWithSchema=1,
			SchemaInSeparateStream=2,
			SchemaOnly=3,
            NoXmlOnlyDataSet=4
		}

        public enum TargetDatabases
        {
            NACCM,
            NACCMCommonUser, // adds common user and password to string
            DrugItem,
            ItemExport,
            Security,
            SecurityCommonUser, // adds common user and password to string
            Item,
            Undefined
        }

        public int CommandTimeout
        {
            get
            {
                return _commandTimeout;
            }
            set
            {
                _commandTimeout = value;
            }
        }

        public int RowsReturned
        {
            get
            {
                return _rowsReturned;
            }
            set
            {
                _rowsReturned = value;
            }
        }

        public int BytesReturned
        {
            get
            {
                return _bytesReturned;
            }
            set
            {
                _bytesReturned = value;
            }
        }

		public string ErrorMessage
		{
			get
			{
				return( _errorMessage );
			}
			set
			{
				_errorMessage = value;
			}
		}

		public override string ConnectionString
		{
			get
			{
				return( _connectionString );
			}
			set
			{
				_connectionString = value;
			}
		}

		public DBCommon()
		{
        }

        public DBCommon( TargetDatabases targetDatabase )
        {
            _targetDatabase = targetDatabase;
        }

        private const string _connectionStringFormatString = "Server={0}; Integrated Security=SSPI; Database={1}";
        private const string _commonUserConnectionStringFormatString = "Data Source={0};Initial Catalog={1};UID={2};PWD={3};";

		// presumes parms have already been validated
		private string FormatConnectionString( string dbServerName, string dbName )
		{
            string returnString = "";
            if( _targetDatabase == TargetDatabases.NACCMCommonUser )
                returnString = String.Format( _commonUserConnectionStringFormatString, dbServerName, dbName, Config.ContractManagerDatabaseCommonUserName, Config.ContractManagerDatabaseCommonUserPassword );
            else if( _targetDatabase == TargetDatabases.SecurityCommonUser )
                returnString = String.Format( _commonUserConnectionStringFormatString, dbServerName, dbName, Config.SecurityCommonUserName, Config.SecurityCommonUserPassword );
            else // or undefined
                returnString = String.Format( _connectionStringFormatString, dbServerName, dbName );

            return ( returnString );
		}

        // only for use with standard connection string
        public static string CreateConnectionString( string dbServerName, string dbName )
        {
            return ( String.Format( _connectionStringFormatString, dbServerName, dbName ) );
        }

        // set the db connection string using values from Config object
        public void MakeConnectionString()
        {
            if( _targetDatabase == TargetDatabases.NACCM ) 
                _connectionString = FormatConnectionString( Config.ContractManagerDatabaseServer, Config.ContractManagerDatabase );
            else if( _targetDatabase == TargetDatabases.NACCMCommonUser )
            {
                _connectionString = FormatConnectionString( Config.ContractManagerDatabaseServer, Config.ContractManagerDatabase );
       // $$$ TBD  base.ConnectionString = _connectionString;
            }
            else if( _targetDatabase == TargetDatabases.Item )
            {
                _connectionString = FormatConnectionString( Config.ContractManagerDatabaseServer, Config.ContractManagerDatabase );
            }
            else if( _targetDatabase == TargetDatabases.DrugItem )
                _connectionString = FormatConnectionString( Config.DrugItemDatabaseServer, Config.DrugItemDatabase );
            else if( _targetDatabase == TargetDatabases.ItemExport )
                _connectionString = FormatConnectionString( Config.ExportDatabaseServer, Config.ExportDatabase );
            else if( _targetDatabase == TargetDatabases.Security )
                _connectionString = FormatConnectionString( Config.SecurityDatabaseServer, Config.SecurityDatabase );
            else if( _targetDatabase == TargetDatabases.SecurityCommonUser )
                _connectionString = FormatConnectionString( Config.SecurityDatabaseServer, Config.SecurityDatabase );
            else
                throw new Exception( "Cannot instantiate DocumentDataSource. Target database was not specified." );
        }

		// set the db connection string directly
		public bool SetConnectionInfo( string contractManagerDatabaseConnectionString )
		{
			bool bSuccess = true;

            if( contractManagerDatabaseConnectionString.Length > 0 )
                _connectionString = contractManagerDatabaseConnectionString;
			else
				bSuccess = false;
	
			return( bSuccess );
		}

		// set the db connection info from parms
		public bool SetConnectionInfo( string contractManagerDatabaseServer, string contractManagerDatabase )
		{
			bool bSuccess = true;

            if( contractManagerDatabase.Length > 0 && contractManagerDatabaseServer.Length > 0 )
                _connectionString = FormatConnectionString( contractManagerDatabaseServer, contractManagerDatabase );
			else
				bSuccess = false;
	
			return( bSuccess );
		}

		// set the db connection info 
		// expected keys: "contractManagerDatabase=NACCM;contractManagerDatabaseServer=AMMHINxxx"
		public bool SetConnectionInfo( Hashtable dbConfig )
		{
			bool bSuccess = true;

			if( dbConfig.ContainsKey( "contractManagerDatabase" ) == true &&
				dbConfig.ContainsKey( "contractManagerDatabaseServer" ) == true )
			{
				string contractManagerDatabase = ( string )dbConfig[ "contractManagerDatabase" ];
				string contractManagerDatabaseServer = ( string )dbConfig[ "contractManagerDatabaseServer" ];

				if( contractManagerDatabase.Length > 0 && contractManagerDatabaseServer.Length > 0 )
                    _connectionString = FormatConnectionString( contractManagerDatabaseServer, contractManagerDatabase );
				else
					bSuccess = false;
			}
			else
			{
				bSuccess = false;
			}

			return( bSuccess );
		}

        public int GetByteCount( DataSet ds )
        {
            MemoryStream ms = new MemoryStream();
            ds.WriteXmlSchema( ms );
            ms.Seek( 0, 0 );
            string tmp = _sh.ConvertByteToString( ms.GetBuffer() );
            ms.Close();
            ms = null;
            return ( tmp.Length );
        }

        // internally using MinValue date to represent null date
        public Object GetDBDate( DateTime inDate )
        {
            if( inDate.CompareTo( DateTime.MinValue ) == 0 )
                return( DBNull.Value );
            else
                return ( inDate );
        }

        public enum StandardParameterTypes
        {
            CurrentUser,
            SecurityDatabase,    // each of these indicate both server name and database name parameters
            DrugItemDatabase,
            NACCMDatabase
        }

        public void AddStandardParameter( SqlCommand command, StandardParameterTypes standardParameterType, Guid guidParameter )
        {
            switch( standardParameterType )
            {
                case StandardParameterTypes.CurrentUser:
                    SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                    command.Parameters.Add( parmCurrentUser );
                    parmCurrentUser.Value = guidParameter;
                    break; 
            }
        }

        public void AddStandardParameter( SqlCommand command, StandardParameterTypes standardParameterType, string serverName, string databaseName )
        {
            SqlParameter parmServerName;
            SqlParameter parmDatabaseName;
 
            switch( standardParameterType )
            {
                case StandardParameterTypes.DrugItemDatabase:
                    parmServerName = new SqlParameter( "@DrugItemServerName", SqlDbType.NVarChar, 255 );
                    parmDatabaseName = new SqlParameter( "@DrugItemDatabaseName", SqlDbType.NVarChar, 255 );
                    break;
                case StandardParameterTypes.SecurityDatabase:
                    parmServerName = new SqlParameter( "@SecurityServerName", SqlDbType.NVarChar, 255 );
                    parmDatabaseName = new SqlParameter( "@SecurityDatabaseName", SqlDbType.NVarChar, 255 );
                    break;
                case StandardParameterTypes.NACCMDatabase:
                    parmServerName = new SqlParameter( "@NACCMServerName", SqlDbType.NVarChar, 255 );
                    parmDatabaseName = new SqlParameter( "@NACCMDatabaseName", SqlDbType.NVarChar, 255 );
                    break;
                default:
                    parmServerName = new SqlParameter();
                    parmDatabaseName = new SqlParameter();
                    break;
            }
            command.Parameters.Add( parmServerName );
            command.Parameters.Add( parmDatabaseName );
            parmServerName.Value = serverName;
            parmDatabaseName.Value = databaseName;

        }

        #region BaseEvents
        
        public ArrayList _savedDelegates = new ArrayList();

        private string _eventOwnerName = null;

        public string EventOwnerName
        {
            get { return _eventOwnerName; }
            set { _eventOwnerName = value; }
        }

        [Serializable]
        public class SavedDelegate : ISerializable
        {
            public enum DelegateTypes
            {
                Selecting,
                Inserting,
                Updating,
                Deleting,
                Selected,
                Inserted,
                Updated,
                Deleted,
                Undefined
            }

            // these methods were attempted prior to using RuntimeMethodHandle which is serializable
            //  public Delegate TheDelegate = null;
            //  public GCHandle TheDelegatePointerAsGCHandle;
            //  public IntPtr TheDelegatePointer = IntPtr.Zero;

            private RuntimeMethodHandle _runtimeMethodHandle;

            public RuntimeMethodHandle RMH
            {
                get { return _runtimeMethodHandle; }
                set { _runtimeMethodHandle = value; }
            }

            private DelegateTypes _delegateType = DelegateTypes.Undefined;

            public DelegateTypes DelegateType
            {
                get { return _delegateType; }
                set { _delegateType = value; }
            }

            private string _targetTypeName = "";

            public string TargetTypeName
            {
                get { return _targetTypeName; }
                set { _targetTypeName = value; }
            }

            private string _errorMessage = "";

            public string ErrorMessage
            {
                get { return _errorMessage; }
                set { _errorMessage = value; }
            }

            private string _eventOwnerName = "";

            public string EventOwnerName
            {
                get { return _eventOwnerName; }
                set { _eventOwnerName = value; }
            }

            // collection of old code removed from this object
            //   TheDelegatePointer = Marshal.GetFunctionPointerForDelegate( del );
            //   RuntimeMethodHandle mh = mi.MethodHandle;
            //   TheDelegatePointer = mh.GetFunctionPointer();
            //      TheDelegatePointerAsGCHandle = GCHandle.FromIntPtr( mh.GetFunctionPointer() );
            //   return ( ( SqlDataSourceSelectingEventHandler )Marshal.GetDelegateForFunctionPointer( TheDelegatePointer, typeof( SqlDataSourceSelectingEventHandler ) ) );
            //Type[] constructorParameterTypes = new Type[] { typeof( object ), typeof( IntPtr ) };
            // deserialize example from old way
            //ConstructorInfo ci = delegateType.GetConstructor( constructorParameterTypes );
            //if( TheDelegatePointer != IntPtr.Zero )
            //{
            //    obj = ci.Invoke( new object[] { eventOwner, TheDelegatePointer } );
            //}

            public SavedDelegate( Delegate del, DelegateTypes delegateType, string eventOwnerName )
            {                
                MethodInfo mi = del.Method;
                Type declaringType = mi.DeclaringType;
        
                _targetTypeName = declaringType.Name;   
                _eventOwnerName = eventOwnerName;
                _runtimeMethodHandle = mi.MethodHandle;
                _delegateType = delegateType;               
            }

            public SqlDataSourceSelectingEventHandler DeserializeSelectingDelegate( object eventOwner, string eventOwnerName )
            {                
                Delegate newDel = null;

                try
                {       
                    if( _runtimeMethodHandle.Value != IntPtr.Zero )
                    {
                        MethodBase mb = MethodInfo.GetMethodFromHandle( _runtimeMethodHandle );
                      
                        newDel = Delegate.CreateDelegate( GetDelegateType(), eventOwner, mb.Name );
                    }
                }                
                catch( Exception ex )
                {
                    ErrorMessage = ex.Message;
                }

                return ( ( SqlDataSourceSelectingEventHandler )newDel );
            }

            public SqlDataSourceCommandEventHandler DeserializeCommandDelegate( object eventOwner, string eventOwnerName )
            {
                Delegate newDel = null;

                try
                {
                    if( _runtimeMethodHandle.Value != IntPtr.Zero )
                    {
                        MethodBase mb = MethodInfo.GetMethodFromHandle( _runtimeMethodHandle );
                        newDel = Delegate.CreateDelegate( GetDelegateType(), eventOwner, mb.Name );
                    }
                }
                catch( Exception ex )
                {
                    ErrorMessage = ex.Message;
                }

                return ( ( SqlDataSourceCommandEventHandler )newDel );
            }

            public SqlDataSourceStatusEventHandler DeserializeStatusDelegate( object eventOwner, string eventOwnerName )
            {
                Delegate newDel = null;

                try
                {
                    if( _runtimeMethodHandle.Value != IntPtr.Zero )
                    {
                        MethodBase mb = MethodInfo.GetMethodFromHandle( _runtimeMethodHandle );
                        newDel = Delegate.CreateDelegate( GetDelegateType(), eventOwner, mb.Name );
                    }
                }
                catch( Exception ex )
                {
                    ErrorMessage = ex.Message;
                }

                return ( ( SqlDataSourceStatusEventHandler )newDel );
            }


            public Type GetDelegateType()
            {
                Type returnType = typeof( SqlDataSourceCommandEventHandler ); // default
                switch( DelegateType )
                {
                    case DelegateTypes.Selecting:
                        returnType = typeof( SqlDataSourceSelectingEventHandler );
                        break;
                    case DelegateTypes.Inserting:
                        returnType = typeof( SqlDataSourceCommandEventHandler );
                        break;
                    case DelegateTypes.Updating:
                        returnType = typeof( SqlDataSourceCommandEventHandler );
                        break;
                    case DelegateTypes.Deleting:
                        returnType = typeof( SqlDataSourceCommandEventHandler );
                        break;
                    case DelegateTypes.Selected:
                        returnType = typeof( SqlDataSourceStatusEventHandler );
                        break;
                    case DelegateTypes.Inserted:
                        returnType = typeof( SqlDataSourceStatusEventHandler );
                        break;
                    case DelegateTypes.Updated:
                        returnType = typeof( SqlDataSourceStatusEventHandler );
                        break;
                    case DelegateTypes.Deleted:
                        returnType = typeof( SqlDataSourceStatusEventHandler );
                        break;
                }
                return ( returnType );
            }


            [SecurityCritical]
            void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
            {
                info.AddValue( "RMH", _runtimeMethodHandle );
                info.AddValue( "DelegateType", Enum.GetName( typeof( SavedDelegate.DelegateTypes ), _delegateType ) );
                info.AddValue( "TTN", _targetTypeName );
                info.AddValue( "EM", _errorMessage );
                info.AddValue( "EventOwnerName", _eventOwnerName );
            }

            protected SavedDelegate( SerializationInfo info, StreamingContext context )
            {
                _runtimeMethodHandle = ( RuntimeMethodHandle )info.GetValue( "RMH", typeof( RuntimeMethodHandle ) );
                _delegateType = ( DelegateTypes )Enum.Parse( typeof( DelegateTypes ), info.GetString( "DelegateType" ) );
                _targetTypeName = info.GetString( "TTN" );
                _errorMessage = info.GetString( "EM" );
                _eventOwnerName = info.GetString( "EventOwnerName" );
            }
        }



        private void SaveDelegate( Delegate del, SavedDelegate.DelegateTypes delegateType, string eventOwnerName )
        {
            _savedDelegates.Add( new SavedDelegate( del, delegateType, eventOwnerName ) );
        }

        private void RemoveDelegate( Delegate delegateToRemove, SavedDelegate.DelegateTypes delegateType )
        {
            MethodInfo mi = delegateToRemove.Method;
            RuntimeMethodHandle mh = mi.MethodHandle;

            for( int i = 0; i < _savedDelegates.Count; i++ )
            {
                SavedDelegate savedDelegate = ( SavedDelegate )_savedDelegates[ i ];
                if( savedDelegate != null )
                {
                    if( savedDelegate.DelegateType == delegateType )
                    {
                        if( savedDelegate.RMH.Equals( mh ) == true )
                        {
                            _savedDelegates.Remove( savedDelegate );
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// To allow support of binary serialzation of the events of an object stored in the session, 
        /// call this function from the deserialization ctor for classes that descend from DBCommon. This will
        /// handle all events for those derived classes. For proper deserialization of event handlers contained in objects that do not
        /// descend from DBCommon, call this when the object is retrieved from the session.
        /// </summary>
        /// <param name="eventOwner"></param>
        public void RestoreDelegatesAfterDeserialization( object eventOwner, string eventOwnerName )
        {
            lock( this )
            {
                // Type targetType = eventOwner.GetType();    // this is contractgeneral_aspx ( ie, the derived type of BaseDocumentEditorPage )
                // string targetTypeName = targetType.Name.Replace( "_aspx", "" ).ToLower();

                for( int i = 0; i < _savedDelegates.Count; i++ )
                {
                    SavedDelegate del = ( SavedDelegate )_savedDelegates[ i ];

                    if( del != null ) 
                    {
                        // the same event type may be in multiple times, once for the base type ( since there are base events ), for the calling type, and for other derived types
                        if( eventOwnerName.CompareTo( del.EventOwnerName ) == 0 )  
                        {
                            switch( del.DelegateType )
                            {
                                case SavedDelegate.DelegateTypes.Selecting:
                                    base.Selecting += ( SqlDataSourceSelectingEventHandler )del.DeserializeSelectingDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Inserting:
                                    base.Inserting += ( SqlDataSourceCommandEventHandler )del.DeserializeCommandDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Updating:
                                    base.Updating += ( SqlDataSourceCommandEventHandler )del.DeserializeCommandDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Deleting:
                                    base.Deleting += ( SqlDataSourceCommandEventHandler )del.DeserializeCommandDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Selected:
                                    base.Selected += ( SqlDataSourceStatusEventHandler )del.DeserializeStatusDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Inserted:
                                    base.Inserted += ( SqlDataSourceStatusEventHandler )del.DeserializeStatusDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Updated:
                                    base.Updated += ( SqlDataSourceStatusEventHandler )del.DeserializeStatusDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Deleted:
                                    base.Deleted += ( SqlDataSourceStatusEventHandler )del.DeserializeStatusDelegate( eventOwner, eventOwnerName );
                                    break;
                            }
                        }
                    }
                }
            }
        }

        // call this before setting event handlers 
        // so that it can be saved with the SavedDelegates
        // use the same name when calling the restore after deserialization
        public void SetEventOwnerName( string eventOwnerName )
        {
            _eventOwnerName = eventOwnerName;
        }

        public new event SqlDataSourceSelectingEventHandler Selecting
        {
            add
            {
                base.Selecting += value; // ( EventHandler<EventArgs> )Delegate.Combine( Updating, value );
                SaveDelegate( value, SavedDelegate.DelegateTypes.Selecting, _eventOwnerName );
            }
            remove
            {
                base.Selecting -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.Selecting );
            }
        }


        public new event SqlDataSourceCommandEventHandler Inserting
        {
            add
            {
                base.Inserting += value;
                SaveDelegate( value, SavedDelegate.DelegateTypes.Inserting, _eventOwnerName );
            }
            remove
            {
                base.Inserting -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.Inserting );
            }
        }

        public new event SqlDataSourceCommandEventHandler Updating
        {
            add
            {
                base.Updating += value;
                SaveDelegate( value, SavedDelegate.DelegateTypes.Updating, _eventOwnerName );
            }
            remove
            {
                base.Updating -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.Updating );
            }
        }

        public new event SqlDataSourceCommandEventHandler Deleting
        {
            add
            {
                base.Deleting += value;
                SaveDelegate( value, SavedDelegate.DelegateTypes.Deleting, _eventOwnerName );
            }
            remove
            {
                base.Deleting -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.Deleting );
            }
        }

        public new event SqlDataSourceStatusEventHandler Selected
        {
            add
            {
                base.Selected += value;
                SaveDelegate( value, SavedDelegate.DelegateTypes.Selected, _eventOwnerName );
            }
            remove
            {
                base.Selected -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.Selected );
            }
        }


        public new event SqlDataSourceStatusEventHandler Inserted
        {
            add
            {
                base.Inserted += value;
                SaveDelegate( value, SavedDelegate.DelegateTypes.Inserted, _eventOwnerName );
            }
            remove
            {
                base.Inserted -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.Inserted );
            }
        }

        public new event SqlDataSourceStatusEventHandler Updated
        {
            add
            {
                base.Updated += value;
                SaveDelegate( value, SavedDelegate.DelegateTypes.Updated, _eventOwnerName );
            }
            remove
            {
                base.Updated -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.Updated );
            }
        }

        public new event SqlDataSourceStatusEventHandler Deleted
        {
            add
            {
                base.Deleted += value;
                SaveDelegate( value, SavedDelegate.DelegateTypes.Deleted, _eventOwnerName );
            }
            remove
            {
                base.Deleted -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.Deleted );
            }
        }

        #endregion BaseEvents

        #region ISerializable Members
        

        //[OnSerializing]
        //void OnSerializing( StreamingContext context )
        //{
        //}

        //[OnDeserializing]
        //void OnDeserializing( StreamingContext context )
        //{
        //}

        public static BinaryFormatter GetFormatterWithSurrogates()
        {
            SurrogateSelector ss = new SurrogateSelector();

            StreamingContext context = new StreamingContext( StreamingContextStates.All );

            SqlDataSourceSerializationSurrogate sqlSurrogate = new SqlDataSourceSerializationSurrogate();
            ParameterSerializationSurrogate parmSurrogate = new ParameterSerializationSurrogate();
            //    DelegateSerializationSurrogate delSurrogate = new DelegateSerializationSurrogate( typeof( DBCommon ) );

            ss.AddSurrogate( typeof( SqlDataSource ), context, sqlSurrogate );
            ss.AddSurrogate( typeof( Parameter ), context, parmSurrogate );
            //    ss.AddSurrogate( typeof( Delegate ), context, delSurrogate );   // I think this may have worked if I passed in MulticastDelegate instead of just Delegate, in its present state, the custom function never got called

            BinaryFormatter serializationFormatter = new BinaryFormatter( ss, context );

            return ( serializationFormatter );
        }

        [SecurityCritical]
        public void GetObjectData( SerializationInfo info, StreamingContext context )  // 10/30/2015 took out new keyword
        {
            info.AddValue( "TargetDatabase", Enum.GetName( typeof( TargetDatabases ), _targetDatabase ) );
            info.AddValue( "ConnectionString", _connectionString );
            info.AddValue( "ErrorMessage", _errorMessage );
            info.AddValue( "CommandTimeout", _commandTimeout );
            info.AddValue( "RowsReturned", _rowsReturned );
            info.AddValue( "BytesReturned", _bytesReturned );
            info.AddValue( "EventOwnerName", _eventOwnerName );

            BinaryFormatter serializationFormatter = GetFormatterWithSurrogates();
            if( serializationFormatter != null )
            {
                MemoryStream ms = new MemoryStream();
                SqlDataSource destinationSqlDataSource = new SqlDataSource();
                SqlDataSourceSerializationSurrogate.Copy( ( SqlDataSource )this, destinationSqlDataSource );
                serializationFormatter.Serialize( ms, destinationSqlDataSource );
                info.AddValue( "BaseClass", ms.ToArray() );
            }

            info.AddValue( "SavedDelegates", _savedDelegates );  
        }

        protected DBCommon( SerializationInfo info, StreamingContext context )
            : base()
        {
            _targetDatabase = ( TargetDatabases )Enum.Parse( typeof( TargetDatabases ), info.GetString( "TargetDatabase" ) );
            _connectionString = info.GetString( "ConnectionString" );
            _errorMessage = info.GetString( "ErrorMessage" );
            _commandTimeout = info.GetInt32( "CommandTimeout" );
            _rowsReturned = info.GetInt32( "RowsReturned" );
            _bytesReturned = info.GetInt32( "BytesReturned" );
            _eventOwnerName = info.GetString( "EventOwnerName" );

            BinaryFormatter serializationFormatter = GetFormatterWithSurrogates();

            if( serializationFormatter != null )
            {
                MemoryStream ms = new MemoryStream();
                byte[] byteArray = ( byte[] )info.GetValue( "BaseClass", typeof( Byte[] ));
                ms.Write( byteArray, 0, byteArray.Length );
                ms.Seek( 0, SeekOrigin.Begin );
                SqlDataSource s = ( SqlDataSource )serializationFormatter.Deserialize( ms );
                SqlDataSourceSerializationSurrogate.Copy( s, ( SqlDataSource )this );
            }

            _savedDelegates = ( ArrayList )info.GetValue( "SavedDelegates", typeof( ArrayList ) );  
        }

        #endregion



        void IDisposable.Dispose()
        {
            if( _savedDelegates != null )
            {
                for( int i = 0; i < _savedDelegates.Count; i++ )
                {
                    SavedDelegate savedDelegate = ( SavedDelegate )_savedDelegates[ i ];
                    if( savedDelegate != null )
                    {
                        _savedDelegates.Remove( savedDelegate );
                    }
                }
            }
        }
        
    }
}

