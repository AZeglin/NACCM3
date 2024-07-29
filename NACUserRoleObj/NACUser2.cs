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
    [Serializable]
    public class NACUser2 : IListObject, ISerializable
    {
        private System.Guid _userId = Guid.Empty;
        private string _firstName = System.String.Empty;
        private string _lastName = System.String.Empty;
        private string _fullName = System.String.Empty;
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

        public string FullName
        {
            get
            {
                return ( _fullName );
            }
            set
            {
                _fullName = value;
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

                if( loginName.Length == 0 )
                {
                    throw new Exception( "Login name from current windows prinicpal was blank." );
                }
            }
            catch( Exception ex )
            {
                string msg = string.Format( "Exception encountered in NACUser GetLoginName() for name {0}: {1}", loginName, ex.Message );
                throw new Exception( msg, ex );
            }

            return ( loginName );
        }

        public NACUser2()
        {
            _loginName = GetLoginName();
        }

        public NACUser2( NACUser2 u )
        {
            _userId = u.UserId;
            _firstName = u.FirstName;
            _lastName = u.LastName;
            _fullName = u.FullName;
            _loginName = u.LoginName;
            _email = u.Email;
            _phone = u.Phone;
            _oldUserId = u.OldUserId;
        }

        public NACUser2( DataRow row )
        {
            if( row.IsNull( "UserGuid" ) == false )
                _userId = ( Guid )row[ "UserGuid" ];
            if( row.IsNull( "FirstName" ) == false )
                _firstName = ( string )row[ "FirstName" ];
            if( row.IsNull( "LastName" ) == false )
                _lastName = ( string )row[ "LastName" ];
            if( row.IsNull( "FullName" ) == false )
                _fullName = ( string )row[ "FullName" ];
            if( row.IsNull( "LoginId" ) == false )
                _loginName = ( string )row[ "LoginId" ];
            if( row.IsNull( "Email" ) == false )
                _email = ( string )row[ "Email" ];
            if( row.IsNull( "Phone" ) == false )
                _phone = ( string )row[ "Phone" ];
            if( row.IsNull( "OldUserId" ) == false )
                _oldUserId = ( int )row[ "OldUserId" ];

        }
        public void SetUserDetails( Guid userId, string firstName, string lastName, string fullName, string email, string phone, int oldUserId )
        {
            _userId = userId;
            _firstName = firstName;
            _lastName = lastName;
            _fullName = fullName;
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
            info.AddValue( "FullName", _fullName );
            info.AddValue( "LoginName", _loginName );
            info.AddValue( "Email", _email );
            info.AddValue( "Phone", _phone );
            info.AddValue( "OldUserId", _oldUserId );
        }

        public NACUser2( SerializationInfo info, StreamingContext context )
        {
            _userId = ( Guid )info.GetValue( "UserId", typeof( Guid ) );
            _firstName = info.GetString( "FirstName" );
            _lastName = info.GetString( "LastName" );
            _fullName = info.GetString( "FullName" );
            _loginName = info.GetString( "LoginName" );
            _email = info.GetString( "Email" );
            _phone = info.GetString( "Phone" );
            _oldUserId = info.GetInt32( "OldUserId" );
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
