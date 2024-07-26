using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Security;

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;

namespace VA.NAC.NACCMBrowser.DBInterface
{
    // provides ado.net access to security releated stored procedures 
    // in the NAC CM security database
    [Serializable]
    public class UserSecurityDB2 : DBCommon, ISerializable
    {
      //  private NACLog _log = new NACLog();

        private string _userName = string.Empty;

        // constructor for retrieving cache monitor commands
        public UserSecurityDB2()
            : base()
        {
            //_log.SetCategory( LogBase.Category.DB );
            //_log.SetContext( "UserSecurityDB2", this.GetType() );
            //_log.WriteLog( "Calling UserSecurityDB2() ctor 1", LogBase.Severity.InformLowLevel );
        }

        public UserSecurityDB2( string userName )
            : base( DBCommon.TargetDatabases.SecurityCommonUser )
        {
            //_log.SetCategory( LogBase.Category.DB );
            //_log.SetContext( "UserSecurityDB2", this.GetType() );
            //_log.WriteLog( "Calling UserSecurityDB2() ctor 2", LogBase.Severity.InformLowLevel );

            _userName = userName;
        }



        public bool SelectContractingOfficersForDivision( int divisionId, bool bIncludeInitialValueInResults, ref DataSet dsUsers )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daUsers = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractingOfficersForDivision2
                //(
                //@DivisionId int,   note this SP considers contract related roles for inclusion
                //@SelectFlag int   -- -1 = include initial value of '--select--'
                //select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, UserId

                //SelectContractingOfficers3
                //(
                //@DivisionId as int = -1,   -- -1 = all ( used for reports )
                //@SelectFlag as int = 0,   -- -1 = include initial value of '--select--';  -2 = include initial value of 'All' for reports;  0 = dont add any extra values
                //@OrderByLastNameFullName as nchar(1) = 'L' -- 'L' = order by last name ( use when select flag is not 0 ); 'F' = order by full name
                //)
                //select distinct u.CO_ID, u.FirstName, u.LastName, u.FullName, u.UserName, u.User_Email, u.User_Phone, u.UserId

                SqlCommand cmdSelectContractingOfficers = new SqlCommand( "SelectContractingOfficers3", dbConnection );
                cmdSelectContractingOfficers.CommandType = CommandType.StoredProcedure;
                cmdSelectContractingOfficers.CommandTimeout = 30;

                SqlParameter divisionIdParm = new SqlParameter( "@DivisionId", SqlDbType.Int );
                SqlParameter selectFlagParm = new SqlParameter( "@SelectFlag", SqlDbType.Int );
                SqlParameter lastNameFullNameParm = new SqlParameter( "@OrderByLastNameFullName", SqlDbType.NChar, 1 );

                cmdSelectContractingOfficers.Parameters.Add( divisionIdParm );
                cmdSelectContractingOfficers.Parameters.Add( selectFlagParm );
                cmdSelectContractingOfficers.Parameters.Add( lastNameFullNameParm );

                divisionIdParm.Value = divisionId;
                selectFlagParm.Value = ( bIncludeInitialValueInResults == true ) ? -1 : 0;
                lastNameFullNameParm.Value = "L";   // sort order

                // create a data adapter and dataset to 
                // run the query and hold the results
                daUsers = new SqlDataAdapter();
                daUsers.SelectCommand = cmdSelectContractingOfficers;

                dsUsers = new DataSet( "Users" );
                DataTable dtUsers = dsUsers.Tables.Add( "UserTable" );

                DataColumn userIdColumn = new DataColumn( "CO_ID", typeof( int ) );
                dtUsers.Columns.Add( userIdColumn );
                dtUsers.Columns.Add( "FirstName", typeof( string ) );
                dtUsers.Columns.Add( "LastName", typeof( string ) );
                dtUsers.Columns.Add( "FullName", typeof( string ) );
                dtUsers.Columns.Add( "UserName", typeof( string ) );
                dtUsers.Columns.Add( "User_Email", typeof( string ) );
                dtUsers.Columns.Add( "User_Phone", typeof( string ) );
                dtUsers.Columns.Add( "UserId", typeof( Guid ) ); 

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = userIdColumn;

                // add the keys to the table
                dtUsers.PrimaryKey = primaryKeyColumns;

                dtUsers.Clear();

                // connect
                dbConnection.Open();

                // run
                daUsers.Fill( dsUsers, "UserTable" );

                int rowCount = dsUsers.Tables[ "UserTable" ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in UserSecurityDB.SelectContractingOfficersForDivision(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool SelectContractingOfficersForDivisionOld( int divisionId, bool bIncludeInitialValueInResults, ref DataSet dsUsers )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daUsers = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractingOfficersForDivision2
                //(
                //@DivisionId int,   note this SP considers contract related roles for inclusion
                //@SelectFlag int   -- -1 = include initial value of '--select--'
                //select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, UserId

                SqlCommand cmdSelectUsers = new SqlCommand( "SelectContractingOfficersForDivision2", dbConnection );
                cmdSelectUsers.CommandType = CommandType.StoredProcedure;
                cmdSelectUsers.CommandTimeout = 30;

                SqlParameter divisionIdParm = new SqlParameter( "@DivisionId", SqlDbType.Int );
                SqlParameter selectFlagParm = new SqlParameter( "@SelectFlag", SqlDbType.Int );

                cmdSelectUsers.Parameters.Add( divisionIdParm );
                cmdSelectUsers.Parameters.Add( selectFlagParm );

                divisionIdParm.Value = divisionId;
                selectFlagParm.Value = ( bIncludeInitialValueInResults == true ) ? -1 : 0;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daUsers = new SqlDataAdapter();
                daUsers.SelectCommand = cmdSelectUsers;

                dsUsers = new DataSet( "Users" );
                DataTable dtUsers = dsUsers.Tables.Add( "UserTable" );

                DataColumn userIdColumn = new DataColumn( "CO_ID", typeof( int ) );
                dtUsers.Columns.Add( userIdColumn );
                dtUsers.Columns.Add( "FirstName", typeof( string ) );
                dtUsers.Columns.Add( "LastName", typeof( string ) );
                dtUsers.Columns.Add( "FullName", typeof( string ) );
                dtUsers.Columns.Add( "UserName", typeof( string ) );
                dtUsers.Columns.Add( "User_Email", typeof( string ) );
                dtUsers.Columns.Add( "User_Phone", typeof( string ) );
                dtUsers.Columns.Add( "UserId", typeof( Guid ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = userIdColumn;

                // add the keys to the table
                dtUsers.PrimaryKey = primaryKeyColumns;

                dtUsers.Clear();

                // connect
                dbConnection.Open();

                // run
                daUsers.Fill( dsUsers, "UserTable" );

                int rowCount = dsUsers.Tables[ "UserTable" ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in UserSecurityDB.SelectContractingOfficersForDivision(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetUserInfo( string userName, ref Guid userGuid, ref string firstName, ref string lastName, ref string fullName, ref string email, ref string phone, ref string status, ref int oldUserId, ref int userDivision )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daUserInfo = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetUserInfo2
                //(
                //@LoginId nvarchar(120)
                //select UserId as 'UserGuid',
                //FirstName, LastName, FullName, UserName as 'LoginId', User_Email as 'Email', User_Phone as 'Phone', 
                //case Inactive when 1 then 'INACTIVE' else 'ACTIVE' end as 'Status', CO_ID as 'oldUserId', Division

                SqlCommand cmdSelectUser = new SqlCommand( "GetUserInfo2", dbConnection );
                cmdSelectUser.CommandType = CommandType.StoredProcedure;
                cmdSelectUser.CommandTimeout = 30;

                SqlParameter loginIdParm = new SqlParameter( "@LoginId", SqlDbType.NVarChar, 120 );
                loginIdParm.Direction = ParameterDirection.Input;
                loginIdParm.Value = userName;
                cmdSelectUser.Parameters.Add( loginIdParm );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daUserInfo = new SqlDataAdapter();
                daUserInfo.SelectCommand = cmdSelectUser;

                DataSet dsUsers = new DataSet( "Users" );
                DataTable dtUsers = dsUsers.Tables.Add( "UserTable" );

                DataColumn userGuidColumn = new DataColumn( "UserGuid", typeof( Guid ) );

                dtUsers.Columns.Add( userGuidColumn );
                dtUsers.Columns.Add( "FirstName", typeof( string ) );
                dtUsers.Columns.Add( "LastName", typeof( string ) );
                dtUsers.Columns.Add( "FullName", typeof( string ) );
                dtUsers.Columns.Add( "LoginId", typeof( string ) ); // with domain
                dtUsers.Columns.Add( "Email", typeof( string ) );
                dtUsers.Columns.Add( "Phone", typeof( string ) );
                dtUsers.Columns.Add( "Status", typeof( string ) );
                dtUsers.Columns.Add( "OldUserId", typeof( int ) );
                dtUsers.Columns.Add( "Division", typeof( int ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = userGuidColumn;

                // add the keys to the table
                dtUsers.PrimaryKey = primaryKeyColumns;

                dtUsers.Clear();

                // connect
                dbConnection.Open();

                // run
                daUserInfo.Fill( dsUsers, "UserTable" );

                userGuid = Guid.Empty;
                firstName = "";
                lastName = "";
                fullName = "";
                email = "";
                phone = "";
                status = "";
                oldUserId = -1;
                userDivision = -1;

                int tableCount = dsUsers.Tables.Count;
                int rowCount;
                DataRow userInfoRow = null;

                // user had > 1 entry, so the second result set contains the active instance of the user
                if( tableCount > 1 )
                {
                    rowCount = dsUsers.Tables[ "UserTable1" ].Rows.Count;

                    if( rowCount == 1 )
                    {
                        userInfoRow = dsUsers.Tables[ "UserTable1" ].Rows[ 0 ];
                    }
                }
                else // use the first and only result set
                {
                    rowCount = dsUsers.Tables[ "UserTable" ].Rows.Count;

                    if( rowCount == 1 )
                    {
                        userInfoRow = dsUsers.Tables[ "UserTable" ].Rows[ 0 ];
                    }
                }

                if( userInfoRow != null )
                {
                    userGuid = ( Guid )userInfoRow[ "UserGuid" ];
                    firstName = ( string )userInfoRow[ "FirstName" ];
                    lastName = ( string )userInfoRow[ "LastName" ];
                    fullName = ( string )userInfoRow[ "FullName" ];
                    email = ( string )userInfoRow[ "Email" ];
                    phone = ( string )userInfoRow[ "Phone" ];
                    status = ( string )userInfoRow[ "Status" ];
                    oldUserId = int.Parse( userInfoRow[ "OldUserId" ].ToString() );
                    userDivision = int.Parse( userInfoRow[ "Division" ].ToString() );
                }
                
                // throws if not found
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in UserSecurityDB.GetUserInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool IsUserInBossRole( int COID, out bool bIsUserInBossRole )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataReader reader = null;
            bIsUserInBossRole = false;

            try
            {
                dbConnection = new SqlConnection( ConnectionString ); 

                // set up the call to the function
                //IsUserInBossRoleFunction
                //(
                //@COID int
                //)

                //RETURNS bit             

                SqlCommand cmdIsUserInBossRoleQuery = new SqlCommand( string.Format( "select dbo.[IsUserInBossRoleFunction]({0})", COID ), dbConnection);
                cmdIsUserInBossRoleQuery.CommandType = CommandType.Text;
                cmdIsUserInBossRoleQuery.CommandTimeout = 30;

                // connect
                dbConnection.Open();

                reader = cmdIsUserInBossRoleQuery.ExecuteReader();
                reader.Read();

                bIsUserInBossRole = ( bool )reader.GetValue(0);

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format("The following exception was encountered in SecurityDB.IsUserInBossRole(): {0}", ex.Message);
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return( bSuccess );
        }

        public bool IsUserInFSSChiefRole( int COID, out bool bIsUserInFSSChiefRole )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataReader reader = null;
            bIsUserInFSSChiefRole = false;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //IsUserInFSSChiefRoleFunction
                //(
                //@COID int
                //)

                //RETURNS bit             

                SqlCommand cmdIsUserInFSSChiefRoleQuery = new SqlCommand( string.Format( "select dbo.[IsUserInFSSChiefRoleFunction]({0})", COID ), dbConnection );
                cmdIsUserInFSSChiefRoleQuery.CommandType = CommandType.Text;
                cmdIsUserInFSSChiefRoleQuery.CommandTimeout = 30;

                // connect
                dbConnection.Open();

                reader = cmdIsUserInFSSChiefRoleQuery.ExecuteReader();
                reader.Read();

                bIsUserInFSSChiefRole = ( bool )reader.GetValue( 0 );

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.IsUserInFSSChiefRole(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // returns a dataset
        public bool SelectSecurityMatrix( ref DataSet dsSecurityMatrix, Guid userId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSecurityMatrix = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //SelectUserScheduleAccessPoints
                //(
                //@UserId uniqueidentifier
                //)
                //select u.UserProfileUserRoleId, u.CO_ID, r.RoleId, r.RoleDescription, s.RoleScheduleGroupId, s.ScheduleGroupId, g.ScheduleNumberList, g.ScheduleGroupDescription,
                //a.RoleAccessPointId, a.AccessPointId, p.AccessPointDescription


                SqlCommand cmdSelectUserScheduleAccessPointsQuery = new SqlCommand( "SelectUserScheduleAccessPoints", dbConnection );
                cmdSelectUserScheduleAccessPointsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectUserScheduleAccessPointsQuery.CommandTimeout = 30;

                SqlParameter userIdParm = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                cmdSelectUserScheduleAccessPointsQuery.Parameters.Add( userIdParm );
                userIdParm.Value = userId;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSecurityMatrix = new SqlDataAdapter();
                daSecurityMatrix.SelectCommand = cmdSelectUserScheduleAccessPointsQuery;

                dsSecurityMatrix = new DataSet( "SecurityMatrix" );
                DataTable dtSecurityMatrix = dsSecurityMatrix.Tables.Add( "SecurityMatrixTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsSecurityMatrix.EnforceConstraints = false;

                DataColumn userProfileUserRoleIdColumn = new DataColumn( "UserProfileUserRoleId", typeof( int ) );
                DataColumn COIDColumn = new DataColumn( "CO_ID", typeof( int ) );
                DataColumn roleIdColumn = new DataColumn( "RoleId", typeof( int ) );
                DataColumn roleDescriptionColumn = new DataColumn( "RoleDescription", typeof( string ) );
                DataColumn roleScheduleGroupIdColumn = new DataColumn( "RoleScheduleGroupId", typeof( int ) );
                DataColumn scheduleGroupIdColumn = new DataColumn( "ScheduleGroupId", typeof( int ) );
                DataColumn scheduleNumberListColumn = new DataColumn( "ScheduleNumberList", typeof( string ) );
                DataColumn scheduleGroupDescriptionColumn = new DataColumn( "ScheduleGroupDescription", typeof( string ) );
                DataColumn roleAccessPointIdColumn = new DataColumn( "RoleAccessPointId", typeof( int ) );
                DataColumn accessPointIdColumn = new DataColumn( "AccessPointId", typeof( int ) );
                DataColumn accessPointDescriptionColumn = new DataColumn( "AccessPointDescription", typeof( string ) );

                dtSecurityMatrix.Columns.Add( userProfileUserRoleIdColumn );
                dtSecurityMatrix.Columns.Add( COIDColumn );
                dtSecurityMatrix.Columns.Add( roleIdColumn );
                dtSecurityMatrix.Columns.Add( roleDescriptionColumn );
                dtSecurityMatrix.Columns.Add( roleScheduleGroupIdColumn );
                dtSecurityMatrix.Columns.Add( scheduleGroupIdColumn );
                dtSecurityMatrix.Columns.Add( scheduleNumberListColumn );
                dtSecurityMatrix.Columns.Add( scheduleGroupDescriptionColumn );
                dtSecurityMatrix.Columns.Add( roleAccessPointIdColumn );
                dtSecurityMatrix.Columns.Add( accessPointIdColumn );
                dtSecurityMatrix.Columns.Add( accessPointDescriptionColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 3 ];
                primaryKeyColumns[ 0 ] = roleIdColumn;
                primaryKeyColumns[ 1 ] = scheduleGroupIdColumn;
                primaryKeyColumns[ 2 ] = accessPointIdColumn;

                // add the keys to the table
                dtSecurityMatrix.PrimaryKey = primaryKeyColumns;

                dtSecurityMatrix.Clear();

                // connect
                dbConnection.Open();

                // run
                daSecurityMatrix.Fill( dsSecurityMatrix, "SecurityMatrixTable" );

                int rowCount = dsSecurityMatrix.Tables[ "SecurityMatrixTable" ].Rows.Count;


            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.SelectUserScheduleAccessPoints(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // returns a dataset - this version includes the new dimension of the operational status groups
        public bool SelectSecurityMatrix3( ref DataSet dsSecurityMatrix, Guid userId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSecurityMatrix = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //SelectUserScheduleOperationalStatusAccessPoints
                //(
                //@UserId uniqueidentifier
                //)
                //select u.UserProfileUserRoleId, u.CO_ID, r.RoleId, r.RoleDescription, s.RoleScheduleGroupId, s.ScheduleGroupId, g.ScheduleNumberList, g.ScheduleGroupDescription,
                //    a.RoleAccessPointId, a.AccessPointId, p.AccessPointDescription, o.RoleOperationalStatusGroupId, o.OperationalStatusGroupId, t.OperationalStatusIdList, t.OperationalStatusGroupDescription


                SqlCommand cmdSelectUserScheduleOperationalStatusAccessPoints = new SqlCommand( "SelectUserScheduleOperationalStatusAccessPoints", dbConnection );
                cmdSelectUserScheduleOperationalStatusAccessPoints.CommandType = CommandType.StoredProcedure;
                cmdSelectUserScheduleOperationalStatusAccessPoints.CommandTimeout = 30;

                SqlParameter userIdParm = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                cmdSelectUserScheduleOperationalStatusAccessPoints.Parameters.Add( userIdParm );
                userIdParm.Value = userId;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSecurityMatrix = new SqlDataAdapter();
                daSecurityMatrix.SelectCommand = cmdSelectUserScheduleOperationalStatusAccessPoints;

                dsSecurityMatrix = new DataSet( "SecurityMatrix" );
                DataTable dtSecurityMatrix = dsSecurityMatrix.Tables.Add( "SecurityMatrixTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsSecurityMatrix.EnforceConstraints = false;

                DataColumn userProfileUserRoleIdColumn = new DataColumn( "UserProfileUserRoleId", typeof( int ) );
                DataColumn COIDColumn = new DataColumn( "CO_ID", typeof( int ) );
                DataColumn roleIdColumn = new DataColumn( "RoleId", typeof( int ) );
                DataColumn roleDescriptionColumn = new DataColumn( "RoleDescription", typeof( string ) );
                DataColumn roleScheduleGroupIdColumn = new DataColumn( "RoleScheduleGroupId", typeof( int ) );
                DataColumn scheduleGroupIdColumn = new DataColumn( "ScheduleGroupId", typeof( int ) );
                DataColumn scheduleNumberListColumn = new DataColumn( "ScheduleNumberList", typeof( string ) );
                DataColumn scheduleGroupDescriptionColumn = new DataColumn( "ScheduleGroupDescription", typeof( string ) );
                DataColumn roleAccessPointIdColumn = new DataColumn( "RoleAccessPointId", typeof( int ) );
                DataColumn accessPointIdColumn = new DataColumn( "AccessPointId", typeof( int ) );
                DataColumn accessPointDescriptionColumn = new DataColumn( "AccessPointDescription", typeof( string ) );
                DataColumn roleOperationalStatusGroupIdColumn = new DataColumn( "RoleOperationalStatusGroupId", typeof( int ) );
                DataColumn operationalStatusGroupIdColumn = new DataColumn( "OperationalStatusGroupId", typeof( int ) );
                DataColumn operationalStatusIdListColumn = new DataColumn( "OperationalStatusIdList", typeof( string ) );
                DataColumn operationalStatusGroupDescriptionColumn = new DataColumn( "OperationalStatusGroupDescription", typeof( string ) );

                dtSecurityMatrix.Columns.Add( userProfileUserRoleIdColumn );
                dtSecurityMatrix.Columns.Add( COIDColumn );
                dtSecurityMatrix.Columns.Add( roleIdColumn );
                dtSecurityMatrix.Columns.Add( roleDescriptionColumn );
                dtSecurityMatrix.Columns.Add( roleScheduleGroupIdColumn );
                dtSecurityMatrix.Columns.Add( scheduleGroupIdColumn );
                dtSecurityMatrix.Columns.Add( scheduleNumberListColumn );
                dtSecurityMatrix.Columns.Add( scheduleGroupDescriptionColumn );
                dtSecurityMatrix.Columns.Add( roleAccessPointIdColumn );
                dtSecurityMatrix.Columns.Add( accessPointIdColumn );
                dtSecurityMatrix.Columns.Add( accessPointDescriptionColumn );
                dtSecurityMatrix.Columns.Add( roleOperationalStatusGroupIdColumn );
                dtSecurityMatrix.Columns.Add( operationalStatusGroupIdColumn );
                dtSecurityMatrix.Columns.Add( operationalStatusIdListColumn );
                dtSecurityMatrix.Columns.Add( operationalStatusGroupDescriptionColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 4 ];
                primaryKeyColumns[ 0 ] = roleIdColumn;
                primaryKeyColumns[ 1 ] = scheduleGroupIdColumn;
                primaryKeyColumns[ 2 ] = accessPointIdColumn;
                primaryKeyColumns[ 3 ] = operationalStatusGroupIdColumn;

                // add the keys to the table
                dtSecurityMatrix.PrimaryKey = primaryKeyColumns;

                dtSecurityMatrix.Clear();

                // connect
                dbConnection.Open();

                // run
                daSecurityMatrix.Fill( dsSecurityMatrix, "SecurityMatrixTable" );

                int rowCount = dsSecurityMatrix.Tables[ "SecurityMatrixTable" ].Rows.Count;


            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.SelectSecurityMatrix3(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // returns a dataset containing the list of valid access points
        public bool SelectAccessPoints( ref DataSet dsAccessPoints, Guid userId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daAccessPoints = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //SelectAccessPoints
                //@UserId uniqueidentifier
                //select AccessPointId, AccessPointDescription

                SqlCommand cmdSelectAccessPointsQuery = new SqlCommand( "SelectAccessPoints", dbConnection );
                cmdSelectAccessPointsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectAccessPointsQuery.CommandTimeout = 30;

                SqlParameter userIdParm = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                cmdSelectAccessPointsQuery.Parameters.Add( userIdParm );
                userIdParm.Value = userId;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daAccessPoints = new SqlDataAdapter();
                daAccessPoints.SelectCommand = cmdSelectAccessPointsQuery;

                dsAccessPoints = new DataSet( "AccessPoints" );
                DataTable dtAccessPoints = dsAccessPoints.Tables.Add( "AccessPointsTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsAccessPoints.EnforceConstraints = false;

                DataColumn accessPointIdColumn = new DataColumn( "AccessPointId", typeof( int ) );
                DataColumn accessPointDescriptionColumn = new DataColumn( "AccessPointDescription", typeof( string ) );

                dtAccessPoints.Columns.Add( accessPointIdColumn );
                dtAccessPoints.Columns.Add( accessPointDescriptionColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = accessPointDescriptionColumn;

                // add the keys to the table
                dtAccessPoints.PrimaryKey = primaryKeyColumns;

                dtAccessPoints.Clear();

                // connect
                dbConnection.Open();

                // run
                daAccessPoints.Fill( dsAccessPoints, "AccessPointsTable" );

                int rowCount = dsAccessPoints.Tables[ "AccessPointsTable" ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.SelectAccessPoints(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        // returns a dataset containing the list of valid operational statuses
        public bool SelectActiveOperationalStatuses( ref DataSet dsActiveOperationalStatuses, System.Guid userId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daActiveOperationalStatuses = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //stored procedure
                //SelectActiveOperationalStatuses
                //@UserId uniqueidentifier
                //select OperationalStatusId,
                //    OperationalStatusDescription
                //from SEC_OperationalStatuses
                //where Inactive = 0
                //order by OperationalStatusDescription

                SqlCommand cmdGetActiveOperationalStatusesQuery = new SqlCommand( "SelectActiveOperationalStatuses", dbConnection );
                cmdGetActiveOperationalStatusesQuery.CommandType = CommandType.StoredProcedure;
                cmdGetActiveOperationalStatusesQuery.CommandTimeout = 30;

                SqlParameter parmUserId = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                cmdGetActiveOperationalStatusesQuery.Parameters.Add( parmUserId );
                parmUserId.Value = userId;

                daActiveOperationalStatuses = new SqlDataAdapter();
                daActiveOperationalStatuses.SelectCommand = cmdGetActiveOperationalStatusesQuery;

                dsActiveOperationalStatuses = new DataSet( "ActiveOperationalStatuses" );
                DataTable dtActiveOperationalStatuses = dsActiveOperationalStatuses.Tables.Add( "ActiveOperationalStatusesTable" );

                DataColumn operationalStatusIdColumn = new DataColumn( "OperationalStatusId", typeof( int ) );

                dtActiveOperationalStatuses.Columns.Add( operationalStatusIdColumn );
                dtActiveOperationalStatuses.Columns.Add( "OperationalStatusDescription", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = operationalStatusIdColumn;

                // add the keys to the table
                dtActiveOperationalStatuses.PrimaryKey = primaryKeyColumns;

                dtActiveOperationalStatuses.Clear();

                dbConnection.Open();

                daActiveOperationalStatuses.Fill( dsActiveOperationalStatuses, "ActiveOperationalStatusesTable" );

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.SelectActiveOperationalStatuses(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // returns a dataset containing the list of valid roles
        public bool SelectRoles( ref DataSet dsRoles, Guid userId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daRoles = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //SelectActiveRoles
                //@UserId uniqueidentifier
                //select RoleId, RoleDescription

                SqlCommand cmdSelectRolesQuery = new SqlCommand( "SelectActiveRoles", dbConnection );
                cmdSelectRolesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectRolesQuery.CommandTimeout = 30;

                SqlParameter userIdParm = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                cmdSelectRolesQuery.Parameters.Add( userIdParm );
                userIdParm.Value = userId;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daRoles = new SqlDataAdapter();
                daRoles.SelectCommand = cmdSelectRolesQuery;

                dsRoles = new DataSet( "Roles" );
                DataTable dtRoles = dsRoles.Tables.Add( "ActiveRolesTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsRoles.EnforceConstraints = false;

                DataColumn roleIdColumn = new DataColumn( "RoleId", typeof( int ) );
                DataColumn roleDescriptionColumn = new DataColumn( "RoleDescription", typeof( string ) );

                dtRoles.Columns.Add( roleIdColumn );
                dtRoles.Columns.Add( roleDescriptionColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = roleDescriptionColumn;

                // add the keys to the table
                dtRoles.PrimaryKey = primaryKeyColumns;

                dtRoles.Clear();

                // connect
                dbConnection.Open();

                // run
                daRoles.Fill( dsRoles, "ActiveRolesTable" );

                int rowCount = dsRoles.Tables[ "ActiveRolesTable" ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.SelectRoles(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // returns a dataset containing the list of schedules, with a list of group ids for each schedule
        public bool SelectReverseLookupScheduleGroups( ref DataSet dsReverseLookupScheduleGroups, Guid userId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daReverseLookupScheduleGroups = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //SelectGeneratedGroupSchedules
                //select ScheduleNumber, ScheduleGroupIdList

                SqlCommand cmdSelectReverseLookupScheduleGroupsQuery = new SqlCommand( "SelectGeneratedGroupSchedules", dbConnection );
                cmdSelectReverseLookupScheduleGroupsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectReverseLookupScheduleGroupsQuery.CommandTimeout = 30;

                SqlParameter userIdParm = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                cmdSelectReverseLookupScheduleGroupsQuery.Parameters.Add( userIdParm );
                userIdParm.Value = userId;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daReverseLookupScheduleGroups = new SqlDataAdapter();
                daReverseLookupScheduleGroups.SelectCommand = cmdSelectReverseLookupScheduleGroupsQuery;

                dsReverseLookupScheduleGroups = new DataSet( "ReverseLookupScheduleGroupsTable" );
                DataTable dtReverseLookupScheduleGroups = dsReverseLookupScheduleGroups.Tables.Add( "ReverseLookupScheduleGroupsTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsReverseLookupScheduleGroups.EnforceConstraints = false;

                DataColumn scheduleNumberColumn = new DataColumn( "ScheduleNumber", typeof( int ) );
                DataColumn scheduleGroupIdListColumn = new DataColumn( "ScheduleGroupIdList", typeof( string ) );

                dtReverseLookupScheduleGroups.Columns.Add( scheduleNumberColumn );
                dtReverseLookupScheduleGroups.Columns.Add( scheduleGroupIdListColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = scheduleNumberColumn;

                // add the keys to the table
                dtReverseLookupScheduleGroups.PrimaryKey = primaryKeyColumns;

                dtReverseLookupScheduleGroups.Clear();

                // connect
                dbConnection.Open();

                // run
                daReverseLookupScheduleGroups.Fill( dsReverseLookupScheduleGroups, "ReverseLookupScheduleGroupsTable" );

                int rowCount = dsReverseLookupScheduleGroups.Tables[ "ReverseLookupScheduleGroupsTable" ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.SelectReverseLookupScheduleGroups(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // returns a dataset containing the list of operational statuses, with a list of group ids for each operational status
        public bool SelectReverseLookupOperationalStatusGroups( ref DataSet dsReverseLookupOperationalStatusGroups, Guid userId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daReverseLookupOperationalStatusGroups = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //SelectGeneratedGroupOperationalStatuses
                //select OperationalStatusId, OperationalStatusGroupIdList
                //from SEC_GeneratedGroupOperationalStatuses
                //order by OperationalStatusId

                SqlCommand cmdSelectReverseLookupOperationalStatusGroupsQuery = new SqlCommand( "SelectGeneratedGroupOperationalStatuses", dbConnection );
                cmdSelectReverseLookupOperationalStatusGroupsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectReverseLookupOperationalStatusGroupsQuery.CommandTimeout = 30;

                SqlParameter userIdParm = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                cmdSelectReverseLookupOperationalStatusGroupsQuery.Parameters.Add( userIdParm );
                userIdParm.Value = userId;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daReverseLookupOperationalStatusGroups = new SqlDataAdapter();
                daReverseLookupOperationalStatusGroups.SelectCommand = cmdSelectReverseLookupOperationalStatusGroupsQuery;

                dsReverseLookupOperationalStatusGroups = new DataSet( "ReverseLookupOperationalStatusGroups" );
                DataTable dtReverseLookupOperationalStatusGroups = dsReverseLookupOperationalStatusGroups.Tables.Add( "ReverseLookupOperationalStatusGroupsTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsReverseLookupOperationalStatusGroups.EnforceConstraints = false;

                DataColumn operationalStatusIdColumn = new DataColumn( "OperationalStatusId", typeof( int ) );
                DataColumn operationalStatusGroupIdListColumn = new DataColumn( "OperationalStatusGroupIdList", typeof( string ) );

                dtReverseLookupOperationalStatusGroups.Columns.Add( operationalStatusIdColumn );
                dtReverseLookupOperationalStatusGroups.Columns.Add( operationalStatusGroupIdListColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = operationalStatusIdColumn;

                // add the keys to the table
                dtReverseLookupOperationalStatusGroups.PrimaryKey = primaryKeyColumns;

                dtReverseLookupOperationalStatusGroups.Clear();

                // connect
                dbConnection.Open();

                // run
                daReverseLookupOperationalStatusGroups.Fill( dsReverseLookupOperationalStatusGroups, "ReverseLookupOperationalStatusGroupsTable" );

                int rowCount = dsReverseLookupOperationalStatusGroups.Tables[ "ReverseLookupOperationalStatusGroupsTable" ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.SelectReverseLookupOperationalStatusGroups(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // returns a dataset containing the list of operational statuses, with a list of group ids for each operational status
        public bool SelectOperationalStatusHierarchy( ref DataSet dsOperationalStatusHierarchy, Guid userId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOperationalStatusHierarchy = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //SelectOperationalStatusHierarchy
                //select OperationalStatusHierarchyId, OperationalStatusId, AllowedOperationalStatusIdList
                //from SEC_OperationalStatusHierarchy
                //order by OperationalStatusId

                SqlCommand cmdSelectOperationalStatusHierarchyQuery = new SqlCommand( "SelectOperationalStatusHierarchy", dbConnection );
                cmdSelectOperationalStatusHierarchyQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOperationalStatusHierarchyQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOperationalStatusHierarchy = new SqlDataAdapter();
                daOperationalStatusHierarchy.SelectCommand = cmdSelectOperationalStatusHierarchyQuery;

                dsOperationalStatusHierarchy = new DataSet( "OperationalStatusHierarchy" );
                DataTable dtOperationalStatusHierarchy = dsOperationalStatusHierarchy.Tables.Add( "OperationalStatusHierarchyTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsOperationalStatusHierarchy.EnforceConstraints = false;

                DataColumn operationalStatusHierarchyIdColumn = new DataColumn( "OperationalStatusHierarchyId", typeof( int ) );
                DataColumn operationalStatusIdColumn = new DataColumn( "OperationalStatusId", typeof( int ) );
                DataColumn allowedOperationalStatusIdListColumn = new DataColumn( "AllowedOperationalStatusIdList", typeof( string ) );

                dtOperationalStatusHierarchy.Columns.Add( operationalStatusHierarchyIdColumn );
                dtOperationalStatusHierarchy.Columns.Add( operationalStatusIdColumn );
                dtOperationalStatusHierarchy.Columns.Add( allowedOperationalStatusIdListColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = operationalStatusHierarchyIdColumn;

                // add the keys to the table
                dtOperationalStatusHierarchy.PrimaryKey = primaryKeyColumns;

                dtOperationalStatusHierarchy.Clear();

                // connect
                dbConnection.Open();

                // run
                daOperationalStatusHierarchy.Fill( dsOperationalStatusHierarchy, "OperationalStatusHierarchyTable" );

                int rowCount = dsOperationalStatusHierarchy.Tables[ "OperationalStatusHierarchyTable" ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.SelectOperationalStatusHierarchy(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // returns a dataset containing the list of operational status id lists to which the input user id belongs
        public bool GetOperationalStatusesForDocumentOwner( ref DataSet dsOperationalStatusIdLists, int COID )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOperationalStatusIdLists = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetOperationalStatusesForDocumentOwner
                // @COID int

                SqlCommand cmdSelectOperationalStatusIdListsQuery = new SqlCommand( "GetOperationalStatusesForDocumentOwner", dbConnection );
                cmdSelectOperationalStatusIdListsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOperationalStatusIdListsQuery.CommandTimeout = 30;

                SqlParameter COIDParm = new SqlParameter( "@COID", SqlDbType.Int );
                cmdSelectOperationalStatusIdListsQuery.Parameters.Add( COIDParm );
                COIDParm.Value = COID;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOperationalStatusIdLists = new SqlDataAdapter();
                daOperationalStatusIdLists.SelectCommand = cmdSelectOperationalStatusIdListsQuery;

                dsOperationalStatusIdLists = new DataSet( "OperationalStatusIdLists" );
                DataTable dtOperationalStatusIdList = dsOperationalStatusIdLists.Tables.Add( "OperationalStatusIdListTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsOperationalStatusIdLists.EnforceConstraints = false;

                DataColumn operationalStatusGroupIdColumn = new DataColumn( "OperationalStatusGroupId", typeof( int ) );
                DataColumn operationalStatusIdListColumn = new DataColumn( "OperationalStatusIdList", typeof( string ) );

                dtOperationalStatusIdList.Columns.Add( operationalStatusGroupIdColumn );
                dtOperationalStatusIdList.Columns.Add( operationalStatusIdListColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = operationalStatusGroupIdColumn;

                // add the keys to the table
                dtOperationalStatusIdList.PrimaryKey = primaryKeyColumns;

                dtOperationalStatusIdList.Clear();

                // connect
                dbConnection.Open();

                // run
                daOperationalStatusIdLists.Fill( dsOperationalStatusIdLists, "OperationalStatusIdListTable" );

                int rowCount = dsOperationalStatusIdLists.Tables[ "OperationalStatusIdListTable" ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.GetOperationalStatusesForDocumentOwner(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        #region ISerializable Members

        [SecurityCritical]
        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );
        
            info.AddValue( "UserName", _userName );
        }


        protected UserSecurityDB2( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            _userName = info.GetString( "UserName" );
            RestoreDelegatesAfterDeserialization( this, "UserSecurityDB2" );
        }

        #endregion
    }
}
