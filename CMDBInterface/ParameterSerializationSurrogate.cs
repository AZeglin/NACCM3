using System;
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
using System.Security;

namespace VA.NAC.NACCMBrowser.DBInterface
{
    public class ParameterSerializationSurrogate : ISerializationSurrogate
    {
        #region ISerializationSurrogate Members

        [SecurityCritical]
        void ISerializationSurrogate.GetObjectData( object obj, SerializationInfo info, StreamingContext context )
        {
            Parameter p = ( Parameter )obj;
            info.AddValue( "DbType", p.DbType );
            info.AddValue( "ConvertEmptyStringToNull", p.ConvertEmptyStringToNull );
            info.AddValue( "DefaultValue", p.DefaultValue );
            info.AddValue( "Direction", Enum.GetName( typeof( ParameterDirection ), p.Direction ));
            info.AddValue( "Name", p.Name );
            info.AddValue( "Size", p.Size );
            info.AddValue( "Type", Enum.GetName( typeof( TypeCode ), p.Type ) );
        }

        object ISerializationSurrogate.SetObjectData( object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector )
        {
            Parameter p = ( Parameter )obj;
            p.DbType = ( DbType )info.GetValue( "DbType", typeof( DbType ));
            p.ConvertEmptyStringToNull = info.GetBoolean( "ConvertEmptyStringToNull" );
            p.DefaultValue = info.GetString( "DefaultValue" );
            p.Direction = ( ParameterDirection )Enum.Parse( typeof( ParameterDirection ), info.GetString( "Direction" ), false );
            p.Name = info.GetString( "Name" );
            p.Size = info.GetInt32( "Size" );
            p.Type = ( TypeCode )Enum.Parse( typeof( TypeCode ), info.GetString( "Type" ), false );

            return ( null );
        }

        #endregion
    }
}
