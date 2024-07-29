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
    // contains a list of NACUser objects
    [Serializable]
    [XmlInclude( typeof( NACUser ) )]
    [XmlInclude( typeof( VA.NAC.Application.SharedObj.KeyValueList<NACUserList> ) )]
    public class NACUserList : KeyValueList<NACUserList>, ISerializable
    {
        public NACUserList()
            : base( "NACUser" )
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
        public NACUserList( NACUserList l )
            : base( "NACUser" )
        {
            this.Keys = new ArrayList( l.Keys );
            this.Values = new ArrayList( l.Values.Count );

            for( int i = 0; i < l.Values.Count; i++ )
            {
                NACUser u = new NACUser( ( NACUser )l.Values[ i ] );
                this.Values.Insert( i, u );
            }
        }

        // creates a OfferList from the dataset returned from the database
        // ( used on the application server )
        public static NACUserList FromDataSet( DataSet dsUsers, string tableName )
        {
            NACUserList userList = new NACUserList();

            if( dsUsers != null )
            {
                if( dsUsers.Tables.Count > 0 )
                {
                    DataTable dtUsers = dsUsers.Tables[ tableName ];
                    foreach( DataRow row in dtUsers.Rows )
                    {
                        if( row.GetType() != typeof( DBNull ) )
                        {
                            // transform a row into a NACUser object
                            NACUser nacUser = new NACUser( row );
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

        public NACUserList( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

        #endregion
    }
}

