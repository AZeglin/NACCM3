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

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;

namespace VA.NAC.NACCMBrowser.DBInterface
{
    // provides ado.net access to security releated stored procedures in the NAC CM database
    public class UserSecurityDB : DBCommon
    {
        private NACLog _log = new NACLog();

        private string _userName = string.Empty;

        // constructor for retrieving cache monitor commands
        public UserSecurityDB()
            : base()
        {
            _log.SetCategory( LogBase.Category.DB );
            _log.SetContext( "UserSecurityDB", this.GetType() );
            _log.WriteLog( "Calling UserSecurityDB() ctor 1", LogBase.Severity.InformLowLevel );
        }

        public UserSecurityDB( string userName )
            : base()
        {
            _log.SetCategory( LogBase.Category.DB );
            _log.SetContext( "UserSecurityDB", this.GetType() );
            _log.WriteLog( "Calling UserSecurityDB() ctor 2", LogBase.Severity.InformLowLevel );

            _userName = userName;
        }


        // returns a dataset
        //public bool SelectUserSecurityMatrix( ref DataSet dsUserSecurityMatrix )
        //{
        //    bool bSuccess = true;
        //    SqlConnection dbConnection = null;
        //    SqlDataAdapter daUserSecurityMatrix = null;

        //    try
        //    {
        //        dbConnection = new SqlConnection( ConnectionString );

        //        // set up the call to the stored procedure
        //        //GetUserPermissionsForDocument
        //        //@userGuid
        //        //Select documentId, subdivision, canview, canupdate, candelete, canassign

        //        SqlCommand cmdSelectUserPermissionsForDocumentQuery = new SqlCommand( "GetUserPermissionsForDocument", dbConnection );
        //        cmdSelectUserPermissionsForDocumentQuery.CommandType = CommandType.StoredProcedure;
        //        cmdSelectUserPermissionsForDocumentQuery.CommandTimeout = 30;

        //        // create a data adapter and dataset to 
        //        // run the query and hold the results
        //        daUserSecurityMatrix = new SqlDataAdapter();
        //        daUserSecurityMatrix.SelectCommand = cmdSelectUserPermissionsForDocumentQuery;

        //        dsUserSecurityMatrix = new DataSet( "UserSecurityMatrix" );
        //        DataTable dtUserSecurityMatrix = dsUserSecurityMatrix.Tables.Add( "UserSecurityMatrixTable" );

        //        DataColumn documentIdColumn = new DataColumn( "DocumentId", typeof( int ) );

        //        dtUserSecurityMatrix.Columns.Add( documentIdColumn );
        //        dtUserSecurityMatrix.Columns.Add( "Subdivision", typeof( string ) );
        //        dtUserSecurityMatrix.Columns.Add( "CanView", typeof( bool ) );
        //        dtUserSecurityMatrix.Columns.Add( "CanUpdate", typeof( bool ) );
        //        dtUserSecurityMatrix.Columns.Add( "CanDelete", typeof( bool ) );
        //        dtUserSecurityMatrix.Columns.Add( "CanAssign", typeof( bool ) );

        //        // create array of primary key columns
        //        DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
        //        primaryKeyColumns[ 0 ] = documentIdColumn;

        //        // add the keys to the table
        //        dtUserSecurityMatrix.PrimaryKey = primaryKeyColumns;

        //        dtUserSecurityMatrix.Clear();

        //        // connect
        //        dbConnection.Open();

        //        // run
        //        daUserSecurityMatrix.Fill( dsUserSecurityMatrix, "UserSecurityMatrixTable" );

        //        int rowCount = dsUserSecurityMatrix.Tables[ "UserSecurityMatrixTable" ].Rows.Count;


        //    }
        //    catch( Exception ex )
        //    {
        //        ErrorMessage = String.Format( "The following exception was encountered in UserSecurityDB.SelectUserSecurityMatrix(): {0}", ex.Message );
        //        bSuccess = false;
        //    }
        //    finally
        //    {
        //        if( dbConnection != null )
        //            dbConnection.Close();
        //    }

        //    return ( bSuccess );
        //}

        //// returns a dataset
        //public bool SelectUserSecurityMatrix( ref DataSet dsUserSecurityMatrix, Guid userId )
        //{
        //    bool bSuccess = true;
        //    SqlConnection dbConnection = null;
        //    SqlDataAdapter daUserSecurityMatrix = null;

        //    try
        //    {
        //        dbConnection = new SqlConnection( ConnectionString );

        //        // set up the call to the stored procedure
        //        //GetUserPermissionsForDocument
        //        //@userGuid
        //        //Select documenttype, documentId, subdivision, canview, canupdate, candelete, canassign
        //        // note the sp also returns contractcategoryid which is ignored ( for now )

        //        SqlCommand cmdSelectUserPermissionsForDocumentQuery = new SqlCommand( "GetUserPermissionsForDocument", dbConnection );
        //        cmdSelectUserPermissionsForDocumentQuery.CommandType = CommandType.StoredProcedure;
        //        cmdSelectUserPermissionsForDocumentQuery.CommandTimeout = 30;

        //        SqlParameter userIdParm = new SqlParameter( "@userGuid", SqlDbType.UniqueIdentifier );
        //        cmdSelectUserPermissionsForDocumentQuery.Parameters.Add( userIdParm );
        //        userIdParm.Value = userId;

        //        // create a data adapter and dataset to 
        //        // run the query and hold the results
        //        daUserSecurityMatrix = new SqlDataAdapter();
        //        daUserSecurityMatrix.SelectCommand = cmdSelectUserPermissionsForDocumentQuery;

        //        dsUserSecurityMatrix = new DataSet( "UserSecurityMatrix" );
        //        DataTable dtUserSecurityMatrix = dsUserSecurityMatrix.Tables.Add( "UserSecurityMatrixTable" );

        //        // this was added to suppress an erroneous constraint error
        //        // there are no ado implemented constraints and the error was on a simple select
        //        dsUserSecurityMatrix.EnforceConstraints = false;

        //        DataColumn documentIdColumn = new DataColumn( "DocumentId", typeof( int ) );
        //        DataColumn documentTypeColumn = new DataColumn( "DocumentType", typeof( string ) );
        //        DataColumn subdivisionColumn = new DataColumn( "Subdivision", typeof( string ) );

        //        dtUserSecurityMatrix.Columns.Add( documentTypeColumn );
        //        dtUserSecurityMatrix.Columns.Add( documentIdColumn );
        //        dtUserSecurityMatrix.Columns.Add( subdivisionColumn );
        //        dtUserSecurityMatrix.Columns.Add( "CanView", typeof( bool ) );
        //        dtUserSecurityMatrix.Columns.Add( "CanUpdate", typeof( bool ) );
        //        dtUserSecurityMatrix.Columns.Add( "CanDelete", typeof( bool ) );
        //        dtUserSecurityMatrix.Columns.Add( "CanAssign", typeof( bool ) );

        //        // create array of primary key columns
        //        DataColumn[] primaryKeyColumns = new DataColumn[ 3 ];
        //        primaryKeyColumns[ 0 ] = documentTypeColumn;
        //        primaryKeyColumns[ 1 ] = documentIdColumn;
        //        primaryKeyColumns[ 2 ] = subdivisionColumn;

        //        // add the keys to the table
        //        dtUserSecurityMatrix.PrimaryKey = primaryKeyColumns;

        //        dtUserSecurityMatrix.Clear();

        //        // connect
        //        dbConnection.Open();

        //        // run
        //        daUserSecurityMatrix.Fill( dsUserSecurityMatrix, "UserSecurityMatrixTable" );

        //        int rowCount = dsUserSecurityMatrix.Tables[ "UserSecurityMatrixTable" ].Rows.Count;


        //    }
        //    catch( Exception ex )
        //    {
        //        ErrorMessage = String.Format( "The following exception was encountered in UserSecurityDB.SelectUserSecurityMatrix(): {0}", ex.Message );
        //        bSuccess = false;
        //    }
        //    finally
        //    {
        //        if( dbConnection != null )
        //            dbConnection.Close();
        //    }

        //    return ( bSuccess );
        //}

        //public bool SelectUsers( ref DataSet dsUsers )
        //{
        //    bool bSuccess = true;
        //    SqlConnection dbConnection = null;
        //    SqlDataAdapter daUsers = null;

        //    try
        //    {
        //        dbConnection = new SqlConnection( ConnectionString );

        //        // set up the call to the stored procedure
        //        //SelectUsers
        //        //select UserGuid, FirstName, LastName, LoginId, Email, Phone, Status
        //        //from CM_NacUsers

        //        SqlCommand cmdSelectUsers = new SqlCommand( "SelectUsers", dbConnection );
        //        cmdSelectUsers.CommandType = CommandType.StoredProcedure;
        //        cmdSelectUsers.CommandTimeout = 30;

        //        // create a data adapter and dataset to 
        //        // run the query and hold the results
        //        daUsers = new SqlDataAdapter();
        //        daUsers.SelectCommand = cmdSelectUsers;

        //        dsUsers = new DataSet( "Users" );
        //        DataTable dtUsers = dsUsers.Tables.Add( "UserTable" );

        //        DataColumn userGuidColumn = new DataColumn( "UserGuid", typeof( Guid ) );

        //        dtUsers.Columns.Add( userGuidColumn );
        //        dtUsers.Columns.Add( "FirstName", typeof( string ) );
        //        dtUsers.Columns.Add( "LastName", typeof( string ) );
        //        dtUsers.Columns.Add( "LoginId", typeof( string ) ); // with domain
        //        dtUsers.Columns.Add( "Email", typeof( string ) );
        //        dtUsers.Columns.Add( "Phone", typeof( string ) );
        //        dtUsers.Columns.Add( "Status", typeof( string ) );

        //        // create array of primary key columns
        //        DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
        //        primaryKeyColumns[ 0 ] = userGuidColumn;

        //        // add the keys to the table
        //        dtUsers.PrimaryKey = primaryKeyColumns;

        //        dtUsers.Clear();

        //        // connect
        //        dbConnection.Open();

        //        // run
        //        daUsers.Fill( dsUsers, "UserTable" );

        //        int rowCount = dsUsers.Tables[ "UserTable" ].Rows.Count;
        //    }
        //    catch( Exception ex )
        //    {
        //        ErrorMessage = String.Format( "The following exception was encountered in UserSecurityDB.SelectUsers(): {0}", ex.Message );
        //        bSuccess = false;
        //    }
        //    finally
        //    {
        //        if( dbConnection != null )
        //            dbConnection.Close();
        //    }

        //    return ( bSuccess );
        //}

        public bool SelectContractingOfficersForDivision( int divisionId, ref DataSet dsUsers )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daUsers = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractingOfficersForDivision
                //@divisionId int
                //select UserGuid, FirstName, LastName, LoginId, Email, Phone, Status

                SqlCommand cmdSelectUsers = new SqlCommand( "SelectContractingOfficersForDivision", dbConnection );
                cmdSelectUsers.CommandType = CommandType.StoredProcedure;
                cmdSelectUsers.CommandTimeout = 30;

                SqlParameter divisionIdParm = new SqlParameter( "@divisionId", SqlDbType.Int );
                cmdSelectUsers.Parameters.Add( divisionIdParm );
                divisionIdParm.Value = divisionId;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daUsers = new SqlDataAdapter();
                daUsers.SelectCommand = cmdSelectUsers;

                dsUsers = new DataSet( "Users" );
                DataTable dtUsers = dsUsers.Tables.Add( "UserTable" );

                DataColumn userGuidColumn = new DataColumn( "UserGuid", typeof( Guid ) );

                dtUsers.Columns.Add( userGuidColumn );
                dtUsers.Columns.Add( "FirstName", typeof( string ) );
                dtUsers.Columns.Add( "LastName", typeof( string ) );
                dtUsers.Columns.Add( "LoginId", typeof( string ) ); // with domain
                dtUsers.Columns.Add( "Email", typeof( string ) );
                dtUsers.Columns.Add( "Phone", typeof( string ) );
                dtUsers.Columns.Add( "Status", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = userGuidColumn;

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
                ErrorMessage = String.Format( "The following exception was encountered in UserSecurityDB.SelectUsers(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetUserInfo( string userName, ref Guid userGuid, ref string firstName, ref string lastName, ref string email, ref string phone, ref string status, ref int oldUserId, ref int userTitle, ref int userDivision, ref bool isAdmin )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daUserInfo = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetUserInfo
                //( @userId )
                //select UserId as 'UserGuid',
                //FirstName, LastName, UserName as 'LoginId', User_Email as 'Email', User_Phone as 'Phone', 
                //'DOESNOTEXIST' as 'status', CO_ID as 'oldUserId', UserTitle, Division, Admin
                
                SqlCommand cmdSelectUser = new SqlCommand( "GetUserInfo", dbConnection );
                cmdSelectUser.CommandType = CommandType.StoredProcedure;
                cmdSelectUser.CommandTimeout = 30;

                SqlParameter loginIdParm = new SqlParameter( "@loginId", SqlDbType.NVarChar, 120 );
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
                dtUsers.Columns.Add( "LoginId", typeof( string ) ); // with domain
                dtUsers.Columns.Add( "Email", typeof( string ) );
                dtUsers.Columns.Add( "Phone", typeof( string ) );
                dtUsers.Columns.Add( "Status", typeof( string ) );
                dtUsers.Columns.Add( "OldUserId", typeof( int ) );
                dtUsers.Columns.Add( "UserTitle", typeof( int ) );
                dtUsers.Columns.Add( "Division", typeof( int ) );
                dtUsers.Columns.Add( "Admin", typeof( bool ) );

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

                int rowCount = dsUsers.Tables[ "UserTable" ].Rows.Count;

                userGuid = Guid.Empty;
                firstName = "";
                lastName = "";
                email = "";
                phone = "";
                status = "";
                oldUserId = -1;
                userTitle = -1;
                userDivision = -1;
                isAdmin = false;

                if( rowCount == 1 )
                {
                    DataRow userInfoRow = dsUsers.Tables[ "UserTable" ].Rows[ 0 ];
                    userGuid = ( Guid )userInfoRow[ "UserGuid" ];
                    firstName = ( string )userInfoRow[ "FirstName" ];
                    lastName = ( string )userInfoRow[ "LastName" ];
                    email = ( string )userInfoRow[ "Email" ];
                    phone = ( string )userInfoRow[ "Phone" ];
                    status = ( string )userInfoRow[ "Status" ];
                    oldUserId = int.Parse( userInfoRow[ "OldUserId" ].ToString() );
                    userTitle = int.Parse( userInfoRow[ "UserTitle" ].ToString() );
                    userDivision = int.Parse( userInfoRow[ "Division" ].ToString() );
                    isAdmin = ( bool )userInfoRow[ "Admin" ];
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


        // returns a dataset
        public bool SelectSecurityMatrix( ref DataSet dsSecurityMatrix, Guid userId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSecurityMatrix = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //SelectSecurityMatrixFromSchedCat
                //(
                //@CurrentUser uniqueidentifier
                //select Schedule_Number as ScheduleNumber,
                //Schedule_Name as ScheduleName,
                //Short_Sched_Name as ShortScheduleName,
                //Division,
                //~Inactive as Active,
                //'M' as Role,   /* schedule manager */
                //Schedule_Manager as OldUserId
                //	1 as Ordinality
                //union
                SqlCommand cmdSelectUserPermissionsForScheduleQuery = new SqlCommand( "SelectSecurityMatrixFromSchedCat", dbConnection );
                cmdSelectUserPermissionsForScheduleQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectUserPermissionsForScheduleQuery.CommandTimeout = 30;

                SqlParameter userIdParm = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                cmdSelectUserPermissionsForScheduleQuery.Parameters.Add( userIdParm );
                userIdParm.Value = userId;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSecurityMatrix = new SqlDataAdapter();
                daSecurityMatrix.SelectCommand = cmdSelectUserPermissionsForScheduleQuery;

                dsSecurityMatrix = new DataSet( "SecurityMatrix" );
                DataTable dtSecurityMatrix = dsSecurityMatrix.Tables.Add( "SecurityMatrixTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsSecurityMatrix.EnforceConstraints = false;

                DataColumn scheduleNumberColumn = new DataColumn( "ScheduleNumber", typeof( int ) );
                DataColumn scheduleNameColumn = new DataColumn( "ScheduleName", typeof( string ) );
                DataColumn shortScheduleNameColumn = new DataColumn( "ShortScheduleName", typeof( string ) );
                DataColumn divisionColumn = new DataColumn( "Division", typeof( int ) );
                DataColumn activeColumn = new DataColumn( "Active", typeof( bool ) );
                DataColumn roleColumn = new DataColumn( "Role", typeof( string ) );
                DataColumn oldUserIdColumn = new DataColumn( "OldUserId", typeof( int ) );
                DataColumn ordinalityColumn = new DataColumn( "Ordinality", typeof( int ) );

                dtSecurityMatrix.Columns.Add( scheduleNumberColumn );
                dtSecurityMatrix.Columns.Add( scheduleNameColumn );
                dtSecurityMatrix.Columns.Add( shortScheduleNameColumn );
                dtSecurityMatrix.Columns.Add( divisionColumn );
                dtSecurityMatrix.Columns.Add( activeColumn );
                dtSecurityMatrix.Columns.Add( roleColumn );
                dtSecurityMatrix.Columns.Add( oldUserIdColumn );
                dtSecurityMatrix.Columns.Add( ordinalityColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 3 ];
                primaryKeyColumns[ 0 ] = scheduleNumberColumn;
                primaryKeyColumns[ 1 ] = roleColumn;
                primaryKeyColumns[ 2 ] = ordinalityColumn;

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
                ErrorMessage = String.Format( "The following exception was encountered in SecurityDB.SelectSecurityMatrix(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }
    }
}
