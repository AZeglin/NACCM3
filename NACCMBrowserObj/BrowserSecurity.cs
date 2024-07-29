using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Threading;
using System.Security.Principal;

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;
using VA.NAC.Security.UserRoleObj;
using VA.NAC.NACCMBrowser.DBInterface;


namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public class BrowserSecurity
    {
        private CMRole _cmRole = null;
        private NACUser _nacUser = null;

        private string _permissionDeniedMessage = String.Empty;

        private NACLog _log = new NACLog();

        private SecurityMatrix _securityMatrix = null;



        public string PermissionDeniedMessage
        {
            get
            {
                return ( _permissionDeniedMessage );
            }
        }

        public NACUser UserInfo
        {
            get
            {
                return( _nacUser );
            }
        }

        public BrowserSecurity()
        {
            _log.SetCategory( LogBase.Category.GUI );
            _log.SetContext( "BrowserSecurity", this.GetType() );
            
            _nacUser = new NACUser();
            _cmRole = new CMRole();

            SetUserDetailsAndRoles( ref _nacUser, ref _cmRole );

            // not in table
            if( _cmRole.IsUser == false )
                throw new Exception( "User is not authorized to use this application." );

            // in table but not active
            if( _cmRole.Active == false )
                throw new Exception( "User status is not active" );

            // in table but no database account
            if( _cmRole.DoesNotExist == true )
                throw new Exception( "User does not have a database account" );

            _securityMatrix = new SecurityMatrix( GetSecurityMatrix() );

        }

        public SecurityMatrix SecurityMatrix
        {
            get
            { 
                return( _securityMatrix ); 
            }
        }

        public void SetDocumentEditStatus( CurrentDocument currentDocument )
        {
            if( _cmRole.DoesNotExist == false )
            {
                if( _cmRole.IsUser == true )
                {
                    if( _cmRole.Active == true )
                    {
                        if( _cmRole.IsAdministrator == true )
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEdit;
                        else if( _nacUser.OldUserId == currentDocument.OwnerId )
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEdit;
                        else if( ( _securityMatrix.CanUserEditScheduleRole( currentDocument.ScheduleNumber, SecurityMatrix.ScheduleRoles.Director, _nacUser.OldUserId ) == true ) ||
                            ( _securityMatrix.CanUserEditScheduleRole( currentDocument.ScheduleNumber, SecurityMatrix.ScheduleRoles.AssistantDirector, _nacUser.OldUserId ) == true ) ||
                            ( _securityMatrix.CanUserEditScheduleRole( currentDocument.ScheduleNumber, SecurityMatrix.ScheduleRoles.ScheduleManager, _nacUser.OldUserId ) == true ) ||
                            ( _securityMatrix.CanUserEditScheduleRole( currentDocument.ScheduleNumber, SecurityMatrix.ScheduleRoles.DataEntryFull, _nacUser.OldUserId ) == true ))
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEdit;
                        else if( _cmRole.GetUserType() == CMRole.UserType.Fiscal )
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEditFiscal;
                        else if( _cmRole.GetUserType() == CMRole.UserType.PBM )
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEditPBM;
                        else if( _securityMatrix.CanUserEditScheduleRole( currentDocument.ScheduleNumber, SecurityMatrix.ScheduleRoles.DataEntrySBA, _nacUser.OldUserId ) == true )
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEditSBA;
                        else if( _securityMatrix.CanUserEditScheduleRole( currentDocument.ScheduleNumber, SecurityMatrix.ScheduleRoles.DataEntrySales, _nacUser.OldUserId ) == true )
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEditSales;

                        else
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanView;
                    }
                    else
                    {
                        currentDocument.EditStatus = CurrentDocument.EditStatuses.Denied;
                        _permissionDeniedMessage = "Not an active user.";
                    }
                }
                else
                {
                    currentDocument.EditStatus = CurrentDocument.EditStatuses.Denied;
                    _permissionDeniedMessage = "Not a valid user.";
                }
            }
            else
            {
                currentDocument.EditStatus = CurrentDocument.EditStatuses.Denied;
                _permissionDeniedMessage = "User has no database account.";
            }
        }

        //// returns true if allowed, false if not allowed
        //public bool CheckPermissions( CMRole.DocumentTypes documentType, int documentId, CMRole.DataEntrySubdivisions subdivision, CMRole.PermissionTypes permission )
        //{
        //    bool bAllow = false;
        //    CMRole cmRole = null;

        //    string subdivisionName = Enum.GetName( typeof( CMRole.DataEntrySubdivisions ), subdivision );
        //    string msg = String.Format( "Calling CheckPermissions() for document {0} subdivision {1}.", documentId, subdivisionName );

        //    try
        //    {
        //        bAllow = _cmRole.CheckPermissions( documentType, documentId, subdivision, permission );
        //    }
        //    catch( Exception ex )
        //    {
        //        msg = String.Format( "CheckPermissions() encountered the following exception: {0}", ex.ToString() );
        //        throw new Exception( msg );
        //    }

        //    // set up standard denied message
        //    _permissionDeniedMessage = "";
        //    if( bAllow == false )
        //    {
        //        // "Permission denied" is a key used by the client to handle this particular exception gracefully.
        //        _permissionDeniedMessage = String.Format( "Permission denied for document {0} subdivision {1} access {2}.", documentId, subdivisionName, cmRole.GetPermissionName( permission ));
        //    }

        //    return ( bAllow );
        //}

        // get the security matrix
        private DataSet GetSecurityMatrix()
        {
            bool bSuccess = false;
            string msg = "Called GetSecurityMatrix()";

            DataSet dsSecurityMatrix = null;

            try
            {
                // get db config info
                string contractManagerDatabaseServer = Config.ContractManagerDatabaseServer;
                string contractManagerDatabase = Config.ContractManagerDatabase;

                UserSecurityDB userSecurityDB = new UserSecurityDB( _nacUser.LoginName );
                userSecurityDB.SetConnectionInfo( contractManagerDatabaseServer, contractManagerDatabase );

                bSuccess = userSecurityDB.SelectSecurityMatrix( ref dsSecurityMatrix, _nacUser.UserId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetSecurityMatrix() SelectSecurityMatrix() encountered the following exception : {0}", userSecurityDB.ErrorMessage );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetSecurityMatrix() SelectSecurityMatrix() returned success." );

                }
            }
            catch( Exception ex )
            {
              //  msg = String.Format( "MTSecurity GetUserSecurityMatrix encountered the following exception: {0}", ex.ToString() );
                throw new Exception( "BrowserSecurity.GetSecurityMatrix() encountered an exception.", ex );
            }

            return ( dsSecurityMatrix );
        }

       private void SetUserDetailsAndRoles( ref NACUser nacUser, ref CMRole cmRole )
        {
            bool bSuccess = true;
            string msg = "Called SetUserDetailsAndRoles()";
            _log.WriteLog( msg, LogBase.Severity.InformMediumLevel );

            try
            {
                // get db config info
                string contractManagerDatabaseServer = Config.ContractManagerDatabaseServer;
                string contractManagerDatabase = Config.ContractManagerDatabase;

                UserSecurityDB userSecurityDB = new UserSecurityDB( NACUser.GetLoginName() );
                userSecurityDB.SetConnectionInfo( contractManagerDatabaseServer, contractManagerDatabase );

                Guid userGuid = Guid.Empty;
                string firstName = "";
                string lastName = "";
                string email = "";
                string phone = "";
                string status = "";

                // $$$ added items for use with old database
                int oldUserId = -1;
                int userTitle = -1;
                int userDivision = -1;
                bool isAdmin = false;

                bSuccess = userSecurityDB.GetUserInfo( NACUser.GetLoginName(), ref userGuid, ref firstName, ref lastName, ref email, ref phone, ref status, ref oldUserId, ref userTitle, ref userDivision, ref isAdmin );
                if( bSuccess != true )
                {
                    msg = String.Format( "SetUserDetailsAndRoles() GetUserInfo() encountered the following exception : {0}", userSecurityDB.ErrorMessage );
                    _log.WriteLog( msg, LogBase.Severity.Exception );

                    if( userSecurityDB.ErrorMessage.Contains( "does not exist" ) == true )
                    {
                        cmRole.SetRoleDetails( false, status, isAdmin, userTitle, userDivision );
                        nacUser.SetUserDetails( userGuid, firstName, lastName, email, phone, oldUserId );
                    }
                    else
                    {
                        throw new Exception( msg );
                    }
                }
                else
                {
                    msg = String.Format( "SetUserDetailsAndRoles() GetUserInfo() returned success." );
                    _log.WriteLog( msg, LogBase.Severity.InformLowLevel );

                    cmRole.SetRoleDetails( true, status, isAdmin, userTitle, userDivision );
                    nacUser.SetUserDetails( userGuid, firstName, lastName, email, phone, oldUserId );
                }
            }
            catch( Exception ex )
            {
                msg = String.Format( "Returning the following exception to the client {0}", ex.ToString() );
                _log.WriteLog( msg, LogBase.Severity.Exception );
                throw new Exception( "GetUserDetails() encountered an exception.", ex );
            }

        }

        public NACUserList GetContractingOfficersForDivision( int divisionId )
        {
            bool bSuccess = true;
            string msg = "Called GetUserDetails()";
            _log.WriteLog( msg, LogBase.Severity.InformMediumLevel );

            NACUserList nacUserList = null;

            try
            {
                // get db config info
                string contractManagerDatabaseServer = Config.ContractManagerDatabaseServer;
                string contractManagerDatabase = Config.ContractManagerDatabase;

                UserSecurityDB userSecurityDB = new UserSecurityDB( NACUser.GetLoginName() );
                userSecurityDB.SetConnectionInfo( contractManagerDatabaseServer, contractManagerDatabase );

                DataSet dsUsers = null;
                bSuccess = userSecurityDB.SelectContractingOfficersForDivision( divisionId, ref dsUsers );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetContractingOfficersForDivision() SelectContractingOfficersForDivision() encountered the following exception : {0}", userSecurityDB.ErrorMessage );
                    _log.WriteLog( msg, LogBase.Severity.Exception );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetContractingOfficersForDivision() SelectContractingOfficersForDivision() returned success." );
                    _log.WriteLog( msg, LogBase.Severity.InformLowLevel );
                }

                nacUserList = NACUserList.FromDataSet( dsUsers, "UserTable" );

                if( nacUserList == null )
                {
                    msg = String.Format( "GetContractingOfficersForDivision() NACUserList.FromDataSet() returned null" );
                    _log.WriteLog( msg, LogBase.Severity.Exception );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetContractingOfficersForDivision() NACUserList.FromDataSet() returned success" );
                    _log.WriteLog( msg, LogBase.Severity.InformLowLevel );
                }
            }
            catch( Exception ex )
            {
                msg = String.Format( "Returning the following exception to the client {0}", ex.ToString() );
                _log.WriteLog( msg, LogBase.Severity.Exception );
                throw new Exception( "GetContractingOfficersForDivision() encountered an exception.", ex );
            }

            return ( nacUserList );
        }
    }
}

