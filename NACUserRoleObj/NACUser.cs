using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using System.DirectoryServices;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;

namespace VA.NAC.Security.UserRoleObj
{
    [ Serializable ]
    public class NACUser : IListObject, ISerializable
    {
        private System.Guid _userId = Guid.Empty;
        private string _firstName = System.String.Empty;
        private string _lastName = System.String.Empty;
        private string _loginName = System.String.Empty; // with domain
        private string _email = System.String.Empty;
        private string _phone = System.String.Empty;
        private int _oldUserId = -1;

        public int OldUserId
        {
            get { return _oldUserId; }
            set { _oldUserId = value; }
        }

        public System.Guid UserId
        {
            get
            {
                return ( _userId );
            }
            set
            {
                _userId = value;
            }
        }

        public string GetUserIdString()
        {
            return ( _userId.ToString() );
        }

        public string FirstName
        {
            get
            {
                return ( _firstName );
            }
            set
            {
                _firstName = value;
            }
        }

        public string LastName
        {
            get
            {
                return ( _lastName );
            }
            set
            {
                _lastName = value;
            }
        }

        public string LoginName
        {
            get
            {
                return ( _loginName );
            }
            set
            {
                _loginName = value;
            }
        }

        public string Email
        {
            get
            {
                return ( _email );
            }
            set
            {
                _email = value;
            }
        }

        public string Phone
        {
            get
            {
                return ( _phone );
            }
            set
            {
                _phone = value;
            }
        }

 

        public static string GetLoginName()
        {
            string loginName = "";

            try
            {
                AppDomain.CurrentDomain.SetPrincipalPolicy( PrincipalPolicy.WindowsPrincipal );
                WindowsPrincipal currentPrincipal = ( WindowsPrincipal )Thread.CurrentPrincipal;
                loginName = currentPrincipal.Identity.Name;

            }
            catch( Exception ex )
            {
                string msg = "Exception encountered in NACUser GetLoginName()";
                throw new Exception( msg, ex );
            }

            return ( loginName );
        }

        public NACUser()
        {
            _loginName = GetLoginName();
        }

        public NACUser( NACUser u )
        {
            _userId = u.UserId;
            _firstName = u.FirstName;
            _lastName = u.LastName;
            _loginName = u.LoginName;
            _email = u.Email;
            _phone = u.Phone;
            _oldUserId = u.OldUserId;
        }

        public NACUser( DataRow row )
        {
            if( row.IsNull( "UserGuid" ) == false )
                _userId = ( Guid )row[ "UserGuid" ];
            if( row.IsNull( "FirstName" ) == false )
                _firstName = ( string )row[ "FirstName" ];
            if( row.IsNull( "LastName" ) == false )
                _lastName = ( string )row[ "LastName" ];
            if( row.IsNull( "LoginId" ) == false )
                _loginName = ( string )row[ "LoginId" ];
            if( row.IsNull( "Email" ) == false )
                _email = ( string )row[ "Email" ];
            if( row.IsNull( "Phone" ) == false )
                _phone = ( string )row[ "Phone" ];
            if( row.IsNull( "OldUserId" ) == false )
                _oldUserId = ( int )row[ "OldUserId" ];

        }
        public void SetUserDetails( Guid userId, string firstName, string lastName, string email, string phone, int oldUserId )
        {
            _userId = userId;
            _firstName = firstName;
            _lastName = lastName;
            _email = email;
            _phone = phone;
            _oldUserId = oldUserId;
        }


        #region ISerializable Members

        public void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "UserId", _userId );
            info.AddValue( "FirstName", _firstName );
            info.AddValue( "LastName", _lastName );
            info.AddValue( "LoginName", _loginName );
            info.AddValue( "Email", _email );
            info.AddValue( "Phone", _phone );
        }

        public NACUser( SerializationInfo info, StreamingContext context )
        {
            _userId = ( Guid )info.GetValue( "UserId", typeof( Guid ));
            _firstName = info.GetString( "FirstName" );
            _lastName = info.GetString( "LastName" );
            _loginName = info.GetString( "LoginName" );
            _email = info.GetString( "Email" );
            _phone = info.GetString( "Phone" );
        }

        #endregion

        #region IListObject Members

        public object GetKey()
        {
            return ( UserId );
        }

        public string GetKeyName()
        {
            return ( "UserId" );
        }

        public Type GetKeyType()
        {
            return ( typeof( Guid ) );
        }

        public override string ToString()
        {
            return ( string.Format( "{0}, {1}", _lastName, _firstName ) );
        }

        #endregion
    }
}
