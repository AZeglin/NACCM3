using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;
using System.Runtime.InteropServices;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security;

namespace VA.NAC.NACCMBrowser.DBInterface
{
    sealed class SqlDataSourceSerializationSurrogate :  ISerializationSurrogate
    {
        // copy members from s to d
        public static void Copy( SqlDataSource s, SqlDataSource d )
        {
            d.DeleteCommand = s.DeleteCommand;
            d.InsertCommand = s.InsertCommand;
            d.SelectCommand = s.SelectCommand;
            d.UpdateCommand = s.UpdateCommand;
            d.DeleteCommandType = s.DeleteCommandType;
            d.InsertCommandType = s.InsertCommandType;
            d.SelectCommandType = s.SelectCommandType;
            d.UpdateCommandType = s.UpdateCommandType;
            for( int i = 0; i < s.DeleteParameters.Count; i++ )
            {
                d.DeleteParameters.Add( s.DeleteParameters[ i ] );
            }
            for( int i = 0; i < s.InsertParameters.Count; i++ )
            {
                d.InsertParameters.Add( s.InsertParameters[ i ] );
            }
            for( int i = 0; i < s.SelectParameters.Count; i++ )
            {
                d.SelectParameters.Add( s.SelectParameters[ i ] );
            }
            for( int i = 0; i < s.UpdateParameters.Count; i++ )
            {
                d.UpdateParameters.Add( s.UpdateParameters[ i ] );
            }

            d.CancelSelectOnNullParameter = s.CancelSelectOnNullParameter;

       //     Delegate del = ( Delegate )GetDelegate( ( IComponent )d, "Updating" );

            
            //foreach( SqlDataSourceCommandEventHandler h in d.Updating.GetInvocationList() )
            //{
            //    IntPtr p = Marshal.GetFunctionPointerForDelegate( h );
            //}

         //   IntPtr p = Marshal.GetFunctionPointerForDelegate( del );
            
        }

        private static IEnumerable<string> GetEventKeysList( Component issuer )
        {
            return from key in issuer.GetType().GetFields( BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy )
                   where key.Name.StartsWith( "Event" )
                   select key.Name;

        }

        //private static object GetDelegate( IComponent issuer, string keyName )
        //{
        //    FieldInfo field = issuer.GetType().GetField( keyName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy );

        //    object key = field.GetValue( null );

        //    object events = typeof( Component ).GetField( "events", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( issuer );

        //    object listEntry = typeof( EventHandlerList ).GetMethod( "Find", BindingFlags.Instance | BindingFlags.NonPublic ).Invoke( events, new object[] { key } );

        //    object handler = listEntry.GetType().GetField( "handler", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( listEntry );

        //    return ( handler );
        //}

        [SecurityCritical]
        void ISerializationSurrogate.GetObjectData( object obj, SerializationInfo info, StreamingContext context )
        {
            SqlDataSource d = ( SqlDataSource )obj;
            info.AddValue( "DeleteCommand", d.DeleteCommand );
            info.AddValue( "InsertCommand", d.InsertCommand );
            info.AddValue( "SelectCommand", d.SelectCommand );
            info.AddValue( "UpdateCommand", d.UpdateCommand );
            info.AddValue( "DeleteCommandType", Enum.GetName( typeof( SqlDataSourceCommandType ), d.DeleteCommandType ));
            info.AddValue( "InsertCommandType", Enum.GetName( typeof( SqlDataSourceCommandType ), d.InsertCommandType ));
            info.AddValue( "SelectCommandType", Enum.GetName( typeof( SqlDataSourceCommandType ), d.SelectCommandType ));
            info.AddValue( "UpdateCommandType", Enum.GetName( typeof( SqlDataSourceCommandType ), d.UpdateCommandType ) );

            info.AddValue( "DeleteParameterCount", d.DeleteParameters.Count );
            info.AddValue( "InsertParameterCount", d.InsertParameters.Count );
            info.AddValue( "SelectParameterCount", d.SelectParameters.Count );
            info.AddValue( "UpdateParameterCount", d.UpdateParameters.Count );

            for( int i = 0; i < d.DeleteParameters.Count; i++ )
            {
                info.AddValue( GetKey( "DeleteParameters", i ), d.DeleteParameters[ i ] );
            }

            for( int i = 0; i < d.InsertParameters.Count; i++ )
            {
                info.AddValue( GetKey( "InsertParameters", i ), d.InsertParameters[ i ] );
            }

            for( int i = 0; i < d.SelectParameters.Count; i++ )
            {
                info.AddValue( GetKey( "SelectParameters", i ), d.SelectParameters[ i ] );
            }

            for( int i = 0; i < d.UpdateParameters.Count; i++ )
            {
                info.AddValue( GetKey( "UpdateParameters", i ), d.UpdateParameters[ i ] );
            }

            info.AddValue( "CancelSelectOnNullParameter", d.CancelSelectOnNullParameter );

        }

        private string GetKey( string prefix, int counter )
        {
            return ( string.Format( "{0}{1}", prefix, counter ) );
        }

        [SecurityCritical]
        object ISerializationSurrogate.SetObjectData( object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector )
        {
            SqlDataSource d = ( SqlDataSource )obj;
            d.DeleteCommand = info.GetString( "DeleteCommand" );
            d.InsertCommand = info.GetString( "InsertCommand" );
            d.SelectCommand = info.GetString( "SelectCommand" );
            d.UpdateCommand = info.GetString( "UpdateCommand" );
            d.DeleteCommandType = ( SqlDataSourceCommandType )Enum.Parse( typeof( SqlDataSourceCommandType ), info.GetString( "DeleteCommandType" ), false );
            d.InsertCommandType = ( SqlDataSourceCommandType )Enum.Parse( typeof( SqlDataSourceCommandType ), info.GetString( "InsertCommandType" ), false );
            d.SelectCommandType = ( SqlDataSourceCommandType )Enum.Parse( typeof( SqlDataSourceCommandType ), info.GetString( "SelectCommandType" ), false );
            d.UpdateCommandType = ( SqlDataSourceCommandType )Enum.Parse( typeof( SqlDataSourceCommandType ), info.GetString( "UpdateCommandType" ), false );

            int deleteParametersCount = info.GetInt32( "DeleteParameterCount" );
            int insertParametersCount = info.GetInt32( "InsertParameterCount" );
            int selectParametersCount = info.GetInt32( "SelectParameterCount" );
            int updateParametersCount = info.GetInt32( "UpdateParameterCount" );


            for( int i = 0; i < deleteParametersCount; i++ )
            {
                Parameter p = ( Parameter )info.GetValue( GetKey( "DeleteParameters", i ), typeof( Parameter ) );
                d.DeleteParameters.Add( p );
            }
            for( int i = 0; i < insertParametersCount; i++ )
            {
                Parameter p = ( Parameter )info.GetValue( GetKey( "InsertParameters", i ), typeof( Parameter ) );
                d.InsertParameters.Add( p );
            }
            for( int i = 0; i < selectParametersCount; i++ )
            {
                Parameter p = ( Parameter )info.GetValue( GetKey( "SelectParameters", i ), typeof( Parameter ) );
                d.SelectParameters.Add( p );
            }
            for( int i = 0; i < updateParametersCount; i++ )
            {
                Parameter p = ( Parameter )info.GetValue( GetKey( "UpdateParameters", i ), typeof( Parameter ) );
                d.UpdateParameters.Add( p );
            }

            d.CancelSelectOnNullParameter = info.GetBoolean( "CancelSelectOnNullParameter" );

            return ( null );        
        }
    }
}
