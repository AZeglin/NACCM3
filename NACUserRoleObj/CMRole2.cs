using System;
using System.Runtime.Serialization;
using System.Collections;
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

namespace VA.NAC.Security.UserRoleObj
{
    [Serializable]
    public class CMRole2 
    {

        private bool _bIsActive = false;
        private bool _bDoesNotExist = false;
        private bool _bIsUser = false;

        private int _userDivision = 0;

        public enum Division
        {
            Undefined = 0,
            FSS = 1,
            National = 2
        }

        public bool IsUser
        {
            get
            {
                return ( _bIsUser );
            }
        }

        public bool Active
        {
            get
            {
                return ( _bIsActive );
            }
            set
            {
                _bIsActive = value;
            }
        }

        // if the user is in our user table but does not have a database account
        public bool DoesNotExist
        {
            get
            {
                return ( _bDoesNotExist );
            }
            set
            {
                _bDoesNotExist = value;
            }
        }

        public Division GetUserDivision()
        {
            return ( ( Division )Enum.Parse( typeof( Division ), Enum.GetName( typeof( Division ), _userDivision ) ) );
        }

        public CMRole2()
        {
        }

        public void SetRoleDetails( bool bIsUser, string status, int userDivision )
        {
            _bIsUser = bIsUser;

            if( status.ToUpper().CompareTo( "ACTIVE" ) == 0 )
                _bIsActive = true;

            if( status.ToUpper().CompareTo( "DOESNOTEXIST" ) == 0 )
                _bDoesNotExist = true;

            _userDivision = userDivision;

        }
    }
}
