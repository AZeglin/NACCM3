using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // A matrix of roles, schedules, access points and operational statuses
    [Serializable]
    public class SecurityMatrix3
    {
        private DataSet _dsSecurityMatrix = null;

        public SecurityMatrix3( DataSet dsSecurityMatrix )
        {
            _dsSecurityMatrix = dsSecurityMatrix;
        }

        // returns true if allowed, false if not allowed
        public bool CheckPermissionsAgainstSchedule( int scheduleGroupId, int accessPointId )
        {
            bool bAllow = false;

            if( _dsSecurityMatrix == null )
                throw new Exception( "SecurityMatrix3.CheckPermissionsAgainstSchedule() called prior to security matrix availability" );

            string permissionFilter = String.Format( "ScheduleGroupId={0} and AccessPointId={1}", scheduleGroupId, accessPointId );
            DataTable dtSecurityMatrix = _dsSecurityMatrix.Tables[ "SecurityMatrixTable" ];
            DataRow[] permissionRows = dtSecurityMatrix.Select( permissionFilter );

            if( permissionRows != null )
            {
                if( permissionRows.Length > 0 )
                {
                    bAllow = true;
                    // can capture roles responsible for admission here if required in future
                }
            }

            return ( bAllow );
        }

        // returns true if allowed, false if not allowed
        public bool CheckPermissionsAgainstOperationalStatusGroup( int operationalStatusGroupId, int accessPointId )
        {
            bool bAllow = false;

            if( _dsSecurityMatrix == null )
                throw new Exception( "SecurityMatrix3.CheckPermissionsAgainstOperationalStatusGroup() called prior to security matrix availability" );

            string permissionFilter = String.Format( "OperationalStatusGroupId={0} and AccessPointId={1}", operationalStatusGroupId, accessPointId );
            DataTable dtSecurityMatrix = _dsSecurityMatrix.Tables[ "SecurityMatrixTable" ];
            DataRow[] permissionRows = dtSecurityMatrix.Select( permissionFilter );

            if( permissionRows != null )
            {
                if( permissionRows.Length > 0 )
                {
                    bAllow = true;
                    // can capture roles responsible for admission here if required in future
                }
            }

            return ( bAllow );
        }


        // returns true if allowed, false if not allowed
        public bool CheckPermissionsAgainstScheduleAndOperationalStatusGroup( int scheduleGroupId, int operationalStatusGroupId, int accessPointId )
        {
            bool bAllow = false;

            if( _dsSecurityMatrix == null )
                throw new Exception( "SecurityMatrix3.CheckPermissionsAgainstScheduleAndOperationalStatusGroup() called prior to security matrix availability" );

            string permissionFilter = String.Format( "ScheduleGroupId={0} and OperationalStatusGroupId={1} and AccessPointId={2}", scheduleGroupId, operationalStatusGroupId, accessPointId );
            DataTable dtSecurityMatrix = _dsSecurityMatrix.Tables[ "SecurityMatrixTable" ];
            DataRow[] permissionRows = dtSecurityMatrix.Select( permissionFilter );

            if( permissionRows != null )
            {
                if( permissionRows.Length > 0 )
                {
                    bAllow = true;
                    // can capture roles responsible for admission here if required in future
                }
            }

            return ( bAllow );
        }

        public bool CheckRoleMembership( int scheduleGroupId, int roleId )
        {
            bool bAllow = false;

            DataTable dtSecurityMatrix = _dsSecurityMatrix.Tables[ "SecurityMatrixTable" ];

            // check existence
            int count = dtSecurityMatrix.AsEnumerable().Where( r => r.Field<int>( "RoleId" ) == roleId && r.Field<int>( "ScheduleGroupId" ) == scheduleGroupId ).Count();

            if( count > 0 )
                bAllow = true;

            return ( bAllow );
        }

        // for any schedule
        public bool CheckRoleMembership( int roleId )
        {
            bool bAllow = false;

            DataTable dtSecurityMatrix = _dsSecurityMatrix.Tables[ "SecurityMatrixTable" ];

            // check existence
            int count = dtSecurityMatrix.AsEnumerable().Where( r => r.Field<int>( "RoleId" ) == roleId ).Count();

            if( count > 0 )
                bAllow = true;

            return ( bAllow );
        }

        // checks permissions for ANY schedule group Id
        // returns true if allowed, false if not allowed
        public bool CheckPermissions( int accessPointId )
        {
            bool bAllow = false;

            if( _dsSecurityMatrix == null )
                throw new Exception( "SecurityMatrix3.CheckPermissions() called prior to security matrix availability" );

            string permissionFilter = String.Format( "AccessPointId={0}", accessPointId );
            DataTable dtSecurityMatrix = _dsSecurityMatrix.Tables[ "SecurityMatrixTable" ];
            DataRow[] permissionRows = dtSecurityMatrix.Select( permissionFilter );

            if( permissionRows != null )
            {
                if( permissionRows.Length > 0 )
                {
                    bAllow = true;
                    // can capture roles responsible for admission here if required in future
                }
            }

            return ( bAllow );
        }
    }
}
