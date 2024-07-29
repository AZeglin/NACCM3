using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;

namespace VA.NAC.Security.UserRoleObj
{
    // contains a list of NACUser2 objects
    [Serializable]
    [XmlInclude( typeof( NACUser2 ) )]
    [XmlInclude( typeof( VA.NAC.Application.SharedObj.KeyValueList<NACUserList2> ) )]
    public class NACUserList2 : KeyValueList<NACUserList2>, ISerializable
    {
        public NACUserList2()
            : base( "NACUser2" )
        {

        }

        public new int Count
        {
            get
            {
                return ( base.Count() );
            }
        }

        // copy ctor
        public NACUserList2( NACUserList l )
            : base( "NACUser2" )
        {
            this.Keys = new ArrayList( l.Keys );
            this.Values = new ArrayList( l.Values.Count );

            for( int i = 0; i < l.Values.Count; i++ )
            {
                NACUser2 u = new NACUser2( ( NACUser2 )l.Values[ i ] );
                this.Values.Insert( i, u );
            }
        }

        // creates a list from the dataset returned from the database
        public static NACUserList2 FromDataSet( DataSet dsUsers, string tableName )
        {
            NACUserList2 userList = new NACUserList2();

            if( dsUsers != null )
            {
                if( dsUsers.Tables.Count > 0 )
                {
                    DataTable dtUsers = dsUsers.Tables[ tableName ];
                    foreach( DataRow row in dtUsers.Rows )
                    {
                        if( row.GetType() != typeof( DBNull ) )
                        {
                            // transform a row into a NACUser2 object
                            NACUser2 nacUser = new NACUser2( row );
                            userList.Add( nacUser.LoginName, nacUser );
                        }
                    }
                }
            }

            return ( userList );
        }

        #region ISerializable Members

        public new void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );
        }

        public NACUserList2( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

        #endregion
    }
}

