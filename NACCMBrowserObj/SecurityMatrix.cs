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
    // A matrix of schedules, managers, directors and divisions taken directly from sched/cat table
    public class SecurityMatrix
    {
        private DataSet _dsSecurityMatrix = null;

        public SecurityMatrix( DataSet dsSecurityMatrix )
        {
            _dsSecurityMatrix = dsSecurityMatrix;
        }

        public enum ScheduleRoles
        {
            Undefined,
            Director,
            AssistantDirector,
            ScheduleManager,
            DataEntryFull,
            DataEntrySBA,
            DataEntrySales
        }

        public ScheduleRoles GetScheduleRoleFromString( string roleString )
        {
            ScheduleRoles returnRole = ScheduleRoles.Undefined;

            if( roleString.CompareTo( "D" ) == 0 )
                returnRole = ScheduleRoles.Director;
            else if( roleString.CompareTo( "A" ) == 0 )
                returnRole = ScheduleRoles.AssistantDirector;
            else if( roleString.CompareTo( "M" ) == 0 )
                returnRole = ScheduleRoles.ScheduleManager;
            else if( roleString.CompareTo( "F" ) == 0 )
                returnRole = ScheduleRoles.DataEntryFull;
            else if( roleString.CompareTo( "B" ) == 0 )
                returnRole = ScheduleRoles.DataEntrySBA;
            else if( roleString.CompareTo( "S" ) == 0 )
                returnRole = ScheduleRoles.DataEntrySales;
            else 
                returnRole = ScheduleRoles.Undefined;

            return( returnRole );
        }

        public bool CanUserEditScheduleRole( int scheduleNumber, ScheduleRoles testRole, int oldUserId )
        {
            bool bCanEdit = false;

            string filter = string.Format( "ScheduleNumber = {0} and OldUserId = {1}", scheduleNumber, oldUserId );
            DataView dv = new DataView( _dsSecurityMatrix.Tables[ 0 ], filter, "", DataViewRowState.CurrentRows );

            if( dv.Count > 0 )
            {
                // collect the users roles
                foreach( DataRowView drv in dv )
                {
                    ScheduleRoles role = GetScheduleRoleFromString( ( string )drv[ "Role" ] );

                    if( role == ScheduleRoles.Director )
                        bCanEdit = true;
                    if( role == ScheduleRoles.AssistantDirector )
                        bCanEdit = true;
                    if( role == ScheduleRoles.ScheduleManager )
                        bCanEdit = true;
                    if( role == ScheduleRoles.DataEntryFull )
                        bCanEdit = true;
                    if( role == ScheduleRoles.DataEntrySBA && testRole == ScheduleRoles.DataEntrySBA )
                        bCanEdit = true;
                    if( role == ScheduleRoles.DataEntrySales && testRole == ScheduleRoles.DataEntrySales )
                        bCanEdit = true;
                }
            }

            return( bCanEdit );
        }
    }
}
