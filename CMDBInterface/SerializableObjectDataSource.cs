using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security;
using System.Security.Permissions;

//using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;

namespace VA.NAC.NACCMBrowser.DBInterface
{
    [Serializable]
    public class SerializableObjectDataSource : ObjectDataSource, ISerializable, IDataSource, IDisposable
    {
        public ArrayList _savedDelegates = new ArrayList();

        private string _eventOwnerName = null;

        public string EventOwnerName
        {
            get { return _eventOwnerName; }
            set { _eventOwnerName = value; }
        }

        public SerializableObjectDataSource() : base()
        {
        }

        public SerializableObjectDataSource( string typeName, string selectMethod )
            : base( typeName, selectMethod )
        {
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
                ObjectCreating,
                Selected,
                Inserted,
                Updated,
                Deleted,
                ObjectCreated,
                Undefined
            }
 
            // see DBCommon.cs for other examples of delegate deserialization that were attempted prior to using RuntimeMethodHandle
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

            public SavedDelegate( Delegate del, DelegateTypes delegateType, string eventOwnerName )
            {
                MethodInfo mi = del.Method;
                Type declaringType = mi.DeclaringType;
 
                _targetTypeName = declaringType.Name;
                _eventOwnerName = eventOwnerName;
                _runtimeMethodHandle = mi.MethodHandle;
                _delegateType = delegateType;           
             }

            public ObjectDataSourceSelectingEventHandler DeserializeSelectingDelegate( object eventOwner, string eventOwnerName )
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

                return ( ( ObjectDataSourceSelectingEventHandler )newDel );
            }

            public ObjectDataSourceMethodEventHandler DeserializeMethodDelegate( object eventOwner, string eventOwnerName )
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

                return ( ( ObjectDataSourceMethodEventHandler )newDel );
            }

            public ObjectDataSourceStatusEventHandler DeserializeStatusDelegate( object eventOwner, string eventOwnerName )
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

                return ( ( ObjectDataSourceStatusEventHandler )newDel );
            }

            public ObjectDataSourceObjectEventHandler DeserializeObjectCreationDelegate( object eventOwner, string eventOwnerName )
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

                return ( ( ObjectDataSourceObjectEventHandler )newDel );
            }

            // can add these later, if required...

            // public ObjectDataSourceFilteringEventHandler

            // public ObjectDataSourceDisposingEventHandler

            public Type GetDelegateType()
            {
                Type returnType = typeof( ObjectDataSourceMethodEventHandler ); // default
                switch( DelegateType )
                {
                    case DelegateTypes.Selecting:
                        returnType = typeof( ObjectDataSourceSelectingEventHandler );
                        break;
                    case DelegateTypes.Inserting:
                        returnType = typeof( ObjectDataSourceMethodEventHandler );
                        break;
                    case DelegateTypes.Updating:
                        returnType = typeof( ObjectDataSourceMethodEventHandler );
                        break;
                    case DelegateTypes.Deleting:
                        returnType = typeof( ObjectDataSourceMethodEventHandler );
                        break;
                    case DelegateTypes.ObjectCreating:
                        returnType = typeof( ObjectDataSourceObjectEventHandler );
                        break;
                    case DelegateTypes.Selected:
                        returnType = typeof( ObjectDataSourceStatusEventHandler );
                        break;
                    case DelegateTypes.Inserted:
                        returnType = typeof( ObjectDataSourceStatusEventHandler );
                        break;
                    case DelegateTypes.Updated:
                        returnType = typeof( ObjectDataSourceStatusEventHandler );
                        break;
                    case DelegateTypes.Deleted:
                        returnType = typeof( ObjectDataSourceStatusEventHandler );
                        break;
                    case DelegateTypes.ObjectCreated:
                        returnType = typeof( ObjectDataSourceObjectEventHandler );
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
        /// call this function from the deserialization ctor for classes that descend from ObjectDataSource. This will
        /// handle all events for those derived classes. For proper deserialization of event handlers contained in objects that do not
        /// descend from ObjectDataSource, call this when the object is retrieved from the session.
        /// </summary>
        /// <param name="eventOwner"></param>
        public void RestoreDelegatesAfterDeserialization( object eventOwner, string eventOwnerName )
        {
            lock( this )
            {
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
                                    base.Selecting += ( ObjectDataSourceSelectingEventHandler )del.DeserializeSelectingDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Inserting:
                                    base.Inserting += ( ObjectDataSourceMethodEventHandler )del.DeserializeMethodDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Updating:
                                    base.Updating += ( ObjectDataSourceMethodEventHandler )del.DeserializeMethodDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Deleting:
                                    base.Deleting += ( ObjectDataSourceMethodEventHandler )del.DeserializeMethodDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.ObjectCreating:
                                    base.ObjectCreating += ( ObjectDataSourceObjectEventHandler )del.DeserializeObjectCreationDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Selected:
                                    base.Selected += ( ObjectDataSourceStatusEventHandler )del.DeserializeStatusDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Inserted:
                                    base.Inserted += ( ObjectDataSourceStatusEventHandler )del.DeserializeStatusDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Updated:
                                    base.Updated += ( ObjectDataSourceStatusEventHandler )del.DeserializeStatusDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.Deleted:
                                    base.Deleted += ( ObjectDataSourceStatusEventHandler )del.DeserializeStatusDelegate( eventOwner, eventOwnerName );
                                    break;
                                case SavedDelegate.DelegateTypes.ObjectCreated:
                                    base.ObjectCreated += ( ObjectDataSourceObjectEventHandler )del.DeserializeObjectCreationDelegate( eventOwner, eventOwnerName );
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

        public new event ObjectDataSourceSelectingEventHandler Selecting
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


        public new event ObjectDataSourceMethodEventHandler Inserting
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

        public new event ObjectDataSourceMethodEventHandler Updating
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

        public new event ObjectDataSourceMethodEventHandler Deleting
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

        public new event ObjectDataSourceObjectEventHandler ObjectCreating
        {
            add
            {
                base.ObjectCreating += value;
                SaveDelegate( value, SavedDelegate.DelegateTypes.ObjectCreating, _eventOwnerName );
            }
            remove
            {
                base.ObjectCreating -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.ObjectCreating );
            }
        }

        public new event ObjectDataSourceStatusEventHandler Selected
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


        public new event ObjectDataSourceStatusEventHandler Inserted
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

        public new event ObjectDataSourceStatusEventHandler Updated
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

        public new event ObjectDataSourceStatusEventHandler Deleted
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

        public new event ObjectDataSourceObjectEventHandler ObjectCreated
        {
            add
            {
                base.ObjectCreated += value;
                SaveDelegate( value, SavedDelegate.DelegateTypes.ObjectCreated, _eventOwnerName );
            }
            remove
            {
                base.ObjectCreated -= value;
                RemoveDelegate( value, SavedDelegate.DelegateTypes.ObjectCreated );
            }
        }
        #region ISerializable Members
        

        // see DBCommon for old code versions regarding surrogate for delegates
        public static BinaryFormatter GetFormatterWithSurrogates()
        {
            SurrogateSelector ss = new SurrogateSelector();

            StreamingContext context = new StreamingContext( StreamingContextStates.All );

            ObjectDataSourceSerializationSurrogate objectDataSourceSurrogate = new ObjectDataSourceSerializationSurrogate();
            ParameterSerializationSurrogate parmSurrogate = new ParameterSerializationSurrogate();
 
            ss.AddSurrogate( typeof( ObjectDataSource ), context, objectDataSourceSurrogate );
            ss.AddSurrogate( typeof( Parameter ), context, parmSurrogate );

            BinaryFormatter serializationFormatter = new BinaryFormatter( ss, context );

            return ( serializationFormatter );
        }

        [SecurityCritical]
        public void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            BinaryFormatter serializationFormatter = GetFormatterWithSurrogates();
 
            if( serializationFormatter != null )
            {
                MemoryStream ms = new MemoryStream();
                ObjectDataSource destinationObjectDataSource = new ObjectDataSource();
                ObjectDataSourceSerializationSurrogate.Copy( ( ObjectDataSource )this, destinationObjectDataSource );
                serializationFormatter.Serialize( ms, destinationObjectDataSource );
                info.AddValue( "BaseClass", ms.ToArray() );
            }

            info.AddValue( "EventOwnerName", _eventOwnerName ); 
            info.AddValue( "SavedDelegates", _savedDelegates );  
        }

        protected SerializableObjectDataSource( SerializationInfo info, StreamingContext context )
            : base()
        {
            BinaryFormatter serializationFormatter = GetFormatterWithSurrogates();

            if( serializationFormatter != null )
            {
                MemoryStream ms = new MemoryStream();
                byte[] byteArray = ( byte[] )info.GetValue( "BaseClass", typeof( Byte[] ));
                ms.Write( byteArray, 0, byteArray.Length );
                ms.Seek( 0, SeekOrigin.Begin );
                ObjectDataSource o = ( ObjectDataSource )serializationFormatter.Deserialize( ms );
                ObjectDataSourceSerializationSurrogate.Copy( o, ( ObjectDataSource )this );
            }

            _eventOwnerName = info.GetString( "EventOwnerName" ); 
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
