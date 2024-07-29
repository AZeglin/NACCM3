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

namespace VA.NAC.Security.UserRoleObj
{
    public class CMRole
    {

        private bool _bIsActive = false;
        private bool _bDoesNotExist = false;
        private bool _bIsUser = false;
        private bool _bIsAdministrator = false;
        private NACUser _nacUser = null;

        private int _userTitle = -1; // role from old database = UserType
        private int _userDivision = 0;

        public enum UserType
        {
            Undefined=0,
            Fiscal=1,
            Management=2,
            ContractSpecialist=3,
            PurchasingAgent=4,
            ProcurementClerk=5,
            EquipmentClerk=6,
            EquipmentSpecialist=7,
            PBM=8
        }

        public enum Division
        {
            Undefined=0,
            FSS=1,
            National=2
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

        public bool IsAdministrator
        {
            get
            {
                return ( _bIsAdministrator );
            }
        }

        public bool IsManager( int currentDivision )
        {
            if( GetUserType() == UserType.Management &&
                _userDivision == currentDivision )
                return ( true );
            else
                return ( false );
        }

        public UserType GetUserType()
        {
            return ( ( UserType )Enum.Parse( typeof( UserType ), Enum.GetName( typeof( UserType ), _userTitle ) ) );
        }

        public Division GetUserDivision()
        {
            return ( ( Division )Enum.Parse( typeof( Division ), Enum.GetName( typeof( Division ), _userDivision ) ) );
        }

        public CMRole()
        {
            //_log.SetCategory( LogBase.Category.GUI );
            //_log.SetContext( "CMRole", this.GetType() );
        }

        public void SetRoleDetails( bool bIsUser, string status, bool bIsAdministrator, int userTitle, int userDivision )
        {
            _bIsUser = bIsUser;

            if( status.ToUpper().CompareTo( "ACTIVE" ) == 0 )
                _bIsActive = true;

            if( status.ToUpper().CompareTo( "DOESNOTEXIST" ) == 0 )
                _bDoesNotExist = true;

            _bIsAdministrator = bIsAdministrator;
            _userTitle = userTitle;
            _userDivision = userDivision;

        }

        // returns true if allowed, false if not allowed
        //public bool CheckPermissions( CMRole.DocumentTypes documentType, int documentId, CMRole.DataEntrySubdivisions subdivision, CMRole.PermissionTypes permission )
        //{
        //    bool bAllow = false;

        //    TimeSpan queryDuration = new TimeSpan();
        //    DateTime queryStartTime = DateTime.Now;

        //    string subdivisionName = Enum.GetName( typeof( CMRole.DataEntrySubdivisions ), subdivision );
        //    string documentTypeName = GetDocumentTypeString( documentType );
        //    string documentFilter = String.Format( "DocumentType='{0}' and DocumentId={1} and Subdivision='{2}'", documentTypeName, documentId, subdivisionName );

        //    DataTable dtUserSecurityMatrix = _userSecurityMatrix.Tables[ "UserSecurityMatrixTable" ];
        //    DataRow[] documentRows = dtUserSecurityMatrix.Select( documentFilter );

        //    DateTime queryEndTime = DateTime.Now;
        //    queryDuration = queryEndTime - queryStartTime;
        //    _log.WriteLog( String.Format( "CheckPermissions() query ellapsed time = {0} ms", queryDuration.TotalMilliseconds ), LogBase.Severity.InformLowLevel );

        //    if( documentRows == null )
        //    {
        //        if( permission == PermissionTypes.CanView )
        //        {
        //            bAllow = true;
        //        }
        //        else
        //        {
        //            bAllow = false;
        //        }
        //    }
        //    else if( documentRows.Length == 0 && permission == PermissionTypes.CanView )
        //    {
        //        bAllow = true;
        //    }
        //    else if( documentRows.Length != 1 )
        //    {
        //        string msg = String.Format( "CheckPermissions for document {0} subdivision {1} failed due to incorrect row count returned from filter.", documentId, subdivisionName );
        //        throw new Exception( msg );
        //    }
        //    else
        //    {
        //        DataRow row = documentRows[ 0 ];
        //        bool bCanView = false;
        //        bool bCanUpdate = false;
        //        bool bCanDelete = false;
        //        bool bCanAssign = false;

        //        try
        //        {
        //            bCanView = ( bool )row[ "CanView" ];
        //            bCanUpdate = ( bool )row[ "CanUpdate" ];
        //            bCanDelete = ( bool )row[ "CanDelete" ];
        //            bCanAssign = ( bool )row[ "CanAssign" ];
        //        }
        //        catch( Exception ex )
        //        {
        //            string msg = String.Format( "CheckPermissions for document {0} subdivision {1} failed due to the following exception when reading the matrix row: {2}", documentId, subdivisionName, ex.ToString() );
        //            throw new Exception( msg );
        //        }

        //        // permission sought
        //        switch( permission )
        //        {
        //            case PermissionTypes.CanView:
        //                if( bCanView == true )
        //                    bAllow = true;
        //                break;
        //            case PermissionTypes.CanUpdate:
        //                if( bCanUpdate == true )
        //                    bAllow = true;
        //                break;
        //            case PermissionTypes.CanDelete:
        //                if( bCanDelete == true )
        //                    bAllow = true;
        //                break;
        //            case PermissionTypes.CanAssign:
        //                if( bCanAssign == true )
        //                    bAllow = true;
        //                break;
        //            default:
        //                string msg = String.Format( "CheckPermissions for document {0} subdivision {1} failed due to invalid permission sought.", documentId, subdivisionName );
        //                throw new Exception( msg );
        //                break;

        //        }
        //    }

        //    return ( bAllow );
        //}
    }
}
