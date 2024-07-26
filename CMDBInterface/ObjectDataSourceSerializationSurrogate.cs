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

namespace VA.NAC.NACCMBrowser.DBInterface
{
    sealed class ObjectDataSourceSerializationSurrogate :  ISerializationSurrogate
    {
        // copy members from s to d
        public static void Copy( ObjectDataSource s, ObjectDataSource d )
        {
            d.DeleteMethod = s.DeleteMethod;
            d.InsertMethod = s.InsertMethod;
            d.SelectMethod = s.SelectMethod;
            d.UpdateMethod = s.UpdateMethod;
           
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

            d.ID = s.ID;
            d.TypeName = s.TypeName;
            d.DataObjectTypeName = s.DataObjectTypeName;
        }

        private static IEnumerable<string> GetEventKeysList( Component issuer )
        {
            return from key in issuer.GetType().GetFields( BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy )
                   where key.Name.StartsWith( "Event" )
                   select key.Name;

        }

        void ISerializationSurrogate.GetObjectData( object obj, SerializationInfo info, StreamingContext context )
        {
            ObjectDataSource d = ( ObjectDataSource )obj;
            info.AddValue( "DeleteMethod", d.DeleteMethod );
            info.AddValue( "InsertMethod", d.InsertMethod );
            info.AddValue( "SelectMethod", d.SelectMethod );
            info.AddValue( "UpdateMethod", d.UpdateMethod );

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

            info.AddValue( "ID", d.ID );
            info.AddValue( "TypeName", d.TypeName );
            info.AddValue( "DataObjectTypeName", d.DataObjectTypeName );

        }

        private string GetKey( string prefix, int counter )
        {
            return ( string.Format( "{0}{1}", prefix, counter ) );
        }

        object ISerializationSurrogate.SetObjectData( object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector )
        {
            ObjectDataSource d = ( ObjectDataSource )obj;
            d.DeleteMethod = info.GetString( "DeleteMethod" );
            d.InsertMethod = info.GetString( "InsertMethod" );
            d.SelectMethod = info.GetString( "SelectMethod" );
            d.UpdateMethod = info.GetString( "UpdateMethod" );

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

            d.ID = info.GetString( "ID" );
            d.TypeName = info.GetString( "TypeName" );
            d.DataObjectTypeName = info.GetString( "DataObjectTypeName" );

            return ( null );        
        }
    }
}
