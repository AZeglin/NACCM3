using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Threading;
using System.Security.Principal;
using System.Runtime.Serialization;
using System.Security;

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;
using VA.NAC.Security.UserRoleObj;
using VA.NAC.NACCMBrowser.DBInterface;


namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class BrowserSecurity2 : ISerializable
    {
        private CMRole2 _cmRole = null;      
        private NACUser2 _nacUser = null;

        private DataSet _accessPoints = null;
        private DataSet _operationalStatuses = null;

        private DataSet _activeRoles = null;

        private DataSet _reverseLookupScheduleGroups = null;

        private DataSet _reverseLookupOperationalStatusGroups = null;

        private DataSet _operationalStatusHierarchy = null;
        

        private string _permissionDeniedMessage = String.Empty;

    //    private NACLog _log = new NACLog();

        private SecurityMatrix3 _securityMatrix = null;

        public SecurityMatrix3 SecurityMatrix
        {
            get
            {
                return ( _securityMatrix );
            }
            set
            {
                _securityMatrix = value;
            }
        }

        public string PermissionDeniedMessage
        {
            get
            {
                return ( _permissionDeniedMessage );
            }
        }

        public NACUser2 UserInfo
        {
            get
            {
                return ( _nacUser );
            }
        }

        public CMRole2 CmRole
        {
            get 
            { 
                return _cmRole; 
            }           
        }

        public BrowserSecurity2()
        {
            //_log.SetCategory( LogBase.Category.GUI );
            //_log.SetContext( "BrowserSecurity2", this.GetType() );

            _nacUser = new NACUser2();
            _cmRole = new CMRole2();

            SetUserDetailsAndRoles( ref _nacUser, ref _cmRole );

            // not in table
            if( _cmRole.IsUser == false )
                throw new Exception( "User is not authorized to use this application." );

            // in table but not active
            if( _cmRole.Active == false )
                throw new Exception( "User status is not active" );

            // in table but no database account
            // note: this is blocked as the SP will no longer return it
            // due to newly mandated use of generic account
            if( _cmRole.DoesNotExist == true )
                throw new Exception( "User does not have a database account" );

            _securityMatrix = new SecurityMatrix3( GetSecurityMatrix3() );
            _accessPoints = GetAccessPoints();
            _operationalStatuses = GetOperationalStatuses();
            _activeRoles = GetActiveRoles();
            _reverseLookupScheduleGroups = GetReverseLookupScheduleGroups();
            _reverseLookupOperationalStatusGroups = GetReverseLookupOperationalStatusGroups();
            _operationalStatusHierarchy = GetOperationalStatusHierarchy();
        }

        //// returns true if allowed, false if not allowed
        //public bool CheckPermissions( ArrayList scheduleGroupIds, int accessPointId )
        //{
        //    bool bAllow = false;

        //    if( _securityMatrix == null )
        //        throw new Exception( "BrowserSecurity2.CheckPermissions() called prior to security matrix availability" );

        //    for( int i = 0; i < scheduleGroupIds.Count; i++ )
        //    {
        //        int scheduleGroupId = int.Parse( scheduleGroupIds[ i ].ToString() );
        //        bAllow = _securityMatrix.CheckPermissions( scheduleGroupId, accessPointId );
        //        if( bAllow == true )
        //            break;
        //    }
        //    return ( bAllow );
        //}

        // returns true if allowed, false if not allowed
        public bool CheckPermissionsAgainstSchedule( ArrayList scheduleGroupIds, int accessPointId )
        {
            bool bAllow = false;

            if( _securityMatrix == null )
                throw new Exception( "BrowserSecurity2.CheckPermissionsAgainstSchedule() called prior to security matrix availability" );

            for( int i = 0; i < scheduleGroupIds.Count; i++ )
            {
                int scheduleGroupId = int.Parse( scheduleGroupIds[ i ].ToString() );
                bAllow = _securityMatrix.CheckPermissionsAgainstSchedule( scheduleGroupId, accessPointId );
                if( bAllow == true )
                    break;
            }

            return ( bAllow );
        }

        // returns true if allowed, false if not allowed
        public bool CheckPermissionsAgainstOperationalStatusGroupIdList( ArrayList ownerOperationalStatusGroupIds, int accessPointId )
        {
            bool bAllow = false;

            if( _securityMatrix == null )
                throw new Exception( "BrowserSecurity2.CheckPermissionsAgainstOperationalStatusGroupIdList() called prior to security matrix availability" );

            for( int i = 0; i < ownerOperationalStatusGroupIds.Count; i++ )
            {
                int operationalStatusGroupId = int.Parse( ownerOperationalStatusGroupIds[ i ].ToString() );
                bAllow = _securityMatrix.CheckPermissionsAgainstOperationalStatusGroup( operationalStatusGroupId, accessPointId );

                if( bAllow == true )
                    break;
            }

            return ( bAllow );
        }

        // returns true if allowed, false if not allowed
        public bool CheckPermissionsAgainstScheduleAndOperationalStatusGroups( ArrayList scheduleGroupIds, ArrayList ownerOperationalStatusGroupIds, int accessPointId )
        {
            bool bAllow = false;

            if( _securityMatrix == null )
                throw new Exception( "BrowserSecurity2.CheckPermissionsAgainstScheduleAndOperationalStatusGroups() called prior to security matrix availability" );

            for( int i = 0; i < scheduleGroupIds.Count; i++ )
            {
                int scheduleGroupId = int.Parse( scheduleGroupIds[ i ].ToString() );

                for( int j = 0; j < ownerOperationalStatusGroupIds.Count; j++ )
                {
                    int operationalStatusGroupId = int.Parse( ownerOperationalStatusGroupIds[ j ].ToString() );
                    bAllow = _securityMatrix.CheckPermissionsAgainstScheduleAndOperationalStatusGroup( scheduleGroupId, operationalStatusGroupId, accessPointId );

                    if( bAllow == true )
                        break;
                }
                if( bAllow == true )
                    break;
            }
            return ( bAllow );
        }


        // checks if allowed for ANY schedule group or operational status group
        // returns true if allowed, false if not allowed
        public bool CheckPermissions( AccessPoints accessPoint )
        {
            bool bAllow = false;

            if( _securityMatrix == null )
                throw new Exception( "BrowserSecurity2.CheckPermissions() called prior to security matrix availability" );

            int accessPointId = GetAccessPointId( accessPoint );

            bAllow = _securityMatrix.CheckPermissions( accessPointId );

            return ( bAllow );
        }

        // enum version of Access Points stored in the SEC_AccessPoints table
        // these names must match AccessPointDescription field and thus cannot contain spaces
        public enum AccessPoints
        {
            Undefined,
            Sales,
            Checks,
            Payments,
            SBA,
            MedSurgItems,
            Contract,
            PharmItems,
  //          ByOwner,  // allows ownership to cause access
            Offers,
            CreateContract,
            CreateOffer,
            PharmPrices,  // splitting PBM prices and items to be mutually exclusive 
            ContractAssignment,
  //          SalesByOwner,
  //          SBAByOwner,
  //          MedSurgItemsByOwner,
  //          PharmItemsByOwner,
  //          PharmPricesByOwner,
  //          ContractAssignmentByOwner,
            PharmItemRemove,    // added the following 8 Pharm for 2011
            PharmItemDiscontinue,
            PharmChangeNDC,
            PharmCopyItem,
            PharmCopyItemToContract,
            PharmBPAItems,
            PharmItemDetailsPBM,  // changed behaviour of PBM and CO details to be mutex 3/2013
            PharmItemDetailsCO,
            ContractAwardDate,
            ContractEffectiveDate,
            ContractExpirationDate,
            ContractCompletionDate,
            PharmContractExpirationDate,
            PharmContractCompletionDate,
            AdministerOfferDates,   // depricated  4/2017 - restored 8/2017 for administrative use
            RebateRequired,
            RebateTerms,
  //          RebateTermsByOwner
            OfferReceivedDate,
            OfferAssignment,
            OfferAssignmentDate,            
            OfferReassignmentDate,
            OfferEstimatedCompletionDate,
            OfferActionDate,
            OfferAuditDate,
            OfferReturnDate,
            OfferUnlimitedDateRange,    //  to be used by admin role
            ContractUnlimitedDateRange,  // replacement to be used by admin role
            InsuranceUnlimitedDateRange, // to be used by admin role
            InsurancePolicyDates,
            MedSurgItemExport,   // these 4 are used by the upload application, they are evaluated prior to opening the upload window
            MedSurgItemUpload,
            PharmItemExport,
            PharmItemUpload,
            MedSurgItemDetails,  // re-added with item redesign 2 
            MedSurgPriceDetails,
            MedSurgPrices            
        }

        public int GetAccessPointId( AccessPoints accessPoint )
        {
            int accessPointId = -1;
            string accessPointDescription = Enum.GetName( typeof( AccessPoints ), accessPoint );

            if( _accessPoints != null )
            {
                if( _accessPoints.Tables[ "AccessPointsTable" ] != null )
                {
                    if( _accessPoints.Tables[ "AccessPointsTable" ].Rows.Count > 0 )
                    {
                        DataRow row = _accessPoints.Tables[ "AccessPointsTable" ].Rows.Find( accessPointDescription );
                        if( row != null )
                        {
                            accessPointId = int.Parse( row[ "AccessPointId" ].ToString() );
                        }
                    }
                }
            }

            return ( accessPointId );
        }

        // enum version of OperationalStatuses stored in the SEC_OperationalStatuses table
        // these names must match the OperationalStatusDescription field and thus cannot contain spaces
        public enum OperationalStatuses
        {
            Undefined,
            FSSDirector,
            FSSAlphaAssistantDirector,
            FSSAlphaSeniorContractSpecialist,
            FSSAlphaContractSpecialist,
            FSSLambdaAssistantDirector,
            FSSLambdaSeniorContractSpecialist,
            FSSLambdaContractSpecialist,
            FSSPMRS,
            NationalDirector,
            NationalAssistantDirector,
            NationalSeniorContractSpecialist,
            NationalContractSpecialist,
            FSSOmegaAssistantDirector,
            FSSOmegaSeniorContractSpecialist,
            FSSOmegaContractSpecialist,
            NACCMAdministrator,
            DALCDirector,
            DALCDataEntry,
            DALCContractSpecialist,
            CMOPDirector,
            FrontOffice
        }

        public int GetOperationalStatusId( OperationalStatuses operationalStatus )
        {
            int operationalStatusId = -1;
            string operationalStatusDescription = Enum.GetName( typeof( OperationalStatuses ), operationalStatus );

            if( _operationalStatuses != null )
            {
                if( _operationalStatuses.Tables[ "ActiveOperationalStatusesTable" ] != null )
                {
                    if( _operationalStatuses.Tables[ "ActiveOperationalStatusesTable" ].Rows.Count > 0 )
                    {
                        DataRow row = _operationalStatuses.Tables[ "ActiveOperationalStatusesTable" ].Rows.Find( operationalStatusDescription );
                        if( row != null )
                        {
                            operationalStatusId = int.Parse( row[ "OperationalStatusId" ].ToString() );
                        }
                    }
                }
            }

            return ( operationalStatusId );
        }

        public ArrayList GetScheduleGroupIdsForScheduleNumber( int scheduleNumber )
        {
            ArrayList scheduleGroupIds = new ArrayList();
            string scheduleGroupIdList = "";

            if( _reverseLookupScheduleGroups != null )
            {
                if( _reverseLookupScheduleGroups.Tables[ "ReverseLookupScheduleGroupsTable" ] != null )
                {
                    if( _reverseLookupScheduleGroups.Tables[ "ReverseLookupScheduleGroupsTable" ].Rows.Count > 0 )
                    {
                        DataRow row = _reverseLookupScheduleGroups.Tables[ "ReverseLookupScheduleGroupsTable" ].Rows.Find( scheduleNumber );
                        if( row != null )
                        {
                            scheduleGroupIdList = row[ "ScheduleGroupIdList" ].ToString();

                            if( scheduleGroupIdList.Length > 0 )
                            {
                                string[] idList = scheduleGroupIdList.Split( new char[] { ',' } );
                                scheduleGroupIds.AddRange( ( ICollection )idList );
                            }
                        }
                    }
                }
            }

            return ( scheduleGroupIds );

        }

        // not tested yet $$$ returns a list of ids
        private ArrayList GetOperationalStatusIdsForDocumentOwner( int currentDocumentOwnerId )
        {
            ArrayList operationalStatusIds = new ArrayList();
            string operationalStatusIdList = "";
            bool bSuccess = false;
            DataSet dsOperationalStatusIdLists = null;
            string msg = "";

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( _nacUser.LoginName );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.GetOperationalStatusesForDocumentOwner( ref dsOperationalStatusIdLists, currentDocumentOwnerId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetOperationalStatusIdsForDocumentOwner() GetOperationalStatusesForDocumentOwner() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetOperationalStatusIdsForDocumentOwner() GetOperationalStatusesForDocumentOwner() returned success." );

                }
            }
            catch( Exception ex )
            {
                throw new Exception( "BrowserSecurity.GetOperationalStatusIdsForDocumentOwner() encountered an exception.", ex );
            }

            if( dsOperationalStatusIdLists != null )
            {
                if( dsOperationalStatusIdLists.Tables[ "OperationalStatusIdListTable" ] != null )
                {
                    if( dsOperationalStatusIdLists.Tables[ "OperationalStatusIdListTable" ].Rows.Count > 0 )
                    {
                        DataRowCollection rows = dsOperationalStatusIdLists.Tables[ "OperationalStatusIdListTable" ].Rows;

                        foreach( DataRow row in rows )
                        {
                            if( row != null )
                            {
                                operationalStatusIdList = row[ "OperationalStatusIdList" ].ToString();

                                if( operationalStatusIdList.Length > 0 )
                                {
                                    string[] idList = operationalStatusIdList.Split( new char[] { ',' } );
                                    foreach( string id in idList )
                                    {
                                        if( operationalStatusIds.Contains( id.Trim() ) == false )
                                        {
                                            operationalStatusIds.Add( id.Trim() );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ( operationalStatusIds );
        }

        // not tested $$$ will it fail because its in a loop - returns a list of ids
        private ArrayList GetAllowedOperationalStatusIdsForOwner( ArrayList ownerOperationalStatusIds )
        {
            ArrayList allowedOperationalStatusIds = new ArrayList();

            if( _operationalStatusHierarchy != null )
            {
                if( _operationalStatusHierarchy.Tables[ "OperationalStatusHierarchyTable" ] != null )
                {
                    if( _operationalStatusHierarchy.Tables[ "OperationalStatusHierarchyTable" ].Rows.Count > 0 )
                    {
                        DataTable dtHierarchy = _operationalStatusHierarchy.Tables[ "OperationalStatusHierarchyTable" ];

                        for( int i = 0; i < ownerOperationalStatusIds.Count; i++ )
                        {
                            var idCheckQuery = from id in dtHierarchy.AsEnumerable()
                                               where id.Field<int>( "OperationalStatusId" ) == int.Parse( ownerOperationalStatusIds[ i ].ToString() )
                                               select new
                                               {
                                                   allowedIdList = id.Field<string>( "AllowedOperationalStatusIdList" )
                                               };
                            // should be at most one
                            foreach( var row in idCheckQuery )
                            {
                                if( row.allowedIdList.Length > 0 )
                                {
                                    string[] idList = row.allowedIdList.Split( new char[] { ',' } );
                                    foreach( string allowedId in idList )
                                    {
                                        if( allowedOperationalStatusIds.Contains( allowedId.Trim() ) == false )
                                        {
                                            allowedOperationalStatusIds.Add( allowedId.Trim() );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ( allowedOperationalStatusIds );
        }


        public ArrayList GetOperationalStatusGroupIdsForOperationalStatusId( int operationalStatusId )
        {
            ArrayList operationalStatusGroupIds = new ArrayList();
            string operationalStatusGroupIdList = "";

            if( _reverseLookupOperationalStatusGroups != null )
            {
                if( _reverseLookupOperationalStatusGroups.Tables[ "ReverseLookupOperationalStatusGroupsTable" ] != null )
                {
                    if( _reverseLookupOperationalStatusGroups.Tables[ "ReverseLookupOperationalStatusGroupsTable" ].Rows.Count > 0 )
                    {
                        DataRow row = _reverseLookupOperationalStatusGroups.Tables[ "ReverseLookupOperationalStatusGroupsTable" ].Rows.Find( operationalStatusId );
                        if( row != null )
                        {
                            operationalStatusGroupIdList = row[ "OperationalStatusGroupIdList" ].ToString();

                            if( operationalStatusGroupIdList.Length > 0 )
                            {
                                string[] idList = operationalStatusGroupIdList.Split( new char[] { ',' } );
                                operationalStatusGroupIds.AddRange( ( ICollection )idList );
                            }
                        }
                    }
                }
            }

            return ( operationalStatusGroupIds );

        }

        // this instance resolves many to many
        public ArrayList GetOperationalStatusGroupIdsForOperationalStatusIdList( ArrayList operationalStatusIdList )
        {
            ArrayList operationalStatusGroupIds = new ArrayList();
            string operationalStatusGroupIdList = "";

            if( _reverseLookupOperationalStatusGroups != null )
            {
                if( _reverseLookupOperationalStatusGroups.Tables[ "ReverseLookupOperationalStatusGroupsTable" ] != null )
                {
                    if( _reverseLookupOperationalStatusGroups.Tables[ "ReverseLookupOperationalStatusGroupsTable" ].Rows.Count > 0 )
                    {

                        for( int i = 0; i < operationalStatusIdList.Count; i++ )
                        {
                            int id = int.Parse( operationalStatusIdList[ i ].ToString() );
                        
                            DataRow row = _reverseLookupOperationalStatusGroups.Tables[ "ReverseLookupOperationalStatusGroupsTable" ].Rows.Find( id );
                        
                            if( row != null )
                            {
                                operationalStatusGroupIdList = row[ "OperationalStatusGroupIdList" ].ToString();
                            
                                string[] idList = operationalStatusGroupIdList.Split( new char[] { ',' } );

                                foreach( string groupId in idList )
                                {
                                    if( operationalStatusGroupIds.Contains( groupId ) == false )
                                    {
                                        operationalStatusGroupIds.Add( groupId );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ( operationalStatusGroupIds );

        }
        // inform the selected document of security access for this user
        public void SetDocumentEditStatus( CurrentDocument currentDocument )
        {
            if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.Offer )
            {
                SetDocumentEditStatusForOffer( currentDocument );
            }
            else
            {
                SetDocumentEditStatusForContract( currentDocument );
            }
        }

        public void SetDocumentEditStatusForContract( CurrentDocument currentDocument )
        {
            currentDocument.EditStatus = CurrentDocument.EditStatuses.Denied; // default
            currentDocument.AllowedAccessPoints.Clear();

            ArrayList scheduleGroupIds = GetScheduleGroupIdsForScheduleNumber( currentDocument.ScheduleNumber );

            // get everything the owner is involved in
            ArrayList ownerOperationalStatusIds = GetOperationalStatusIdsForDocumentOwner( currentDocument.OwnerId );

            // get related operational status id's from hierarchy table
            ArrayList allowedOperationalStatusIdsForOwner = GetAllowedOperationalStatusIdsForOwner( ownerOperationalStatusIds );

            // convert to group ids since that is what the matrix requires
            ArrayList allowedOperationalStatusGroupIds = GetOperationalStatusGroupIdsForOperationalStatusIdList( allowedOperationalStatusIdsForOwner );

            if( _cmRole.DoesNotExist == false )
            {
                if( _cmRole.IsUser == true )
                {
                    if( _cmRole.Active == true )
                    {
                        // allow editing if this is the document owner
                        if( _nacUser.OldUserId == currentDocument.OwnerId )
                        {
                            currentDocument.IsCurrentUserTheOwner = true;
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEdit;
                            SetAdditionalEditStatusesForContractIfOwner( currentDocument, scheduleGroupIds );
                        }
                        else
                        {
                            currentDocument.IsCurrentUserTheOwner = false;

                            // check general editing permissions ( main contract body )
                            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.Contract ) ) == true )
                            {
                                currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEdit;
                            }

                            // all active users can view
                            if( currentDocument.EditStatus == CurrentDocument.EditStatuses.Denied )
                                currentDocument.EditStatus = CurrentDocument.EditStatuses.CanView;

                            SetAdditionalEditStatusesForContractIfNotOwner( currentDocument, scheduleGroupIds, allowedOperationalStatusGroupIds );
                        }
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

        // set additional access beyond main contract body in the case where the current user is the owner
        public void SetAdditionalEditStatusesForContractIfOwner( CurrentDocument currentDocument, ArrayList scheduleGroupIds )
        {
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.Checks ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.Checks );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.Payments ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.Payments );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.MedSurgItems ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgItems );
            }       
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.MedSurgItemDetails ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgItemDetails );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.MedSurgPrices ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgPrices );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.MedSurgPriceDetails ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgPriceDetails );
            }            
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmItems ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItems );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmBPAItems ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmBPAItems );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmItemRemove ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemRemove );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmItemDiscontinue ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemDiscontinue );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmChangeNDC ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmChangeNDC );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmCopyItem ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmCopyItem );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmCopyItemToContract ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmCopyItemToContract );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmItemDetailsPBM ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemDetailsPBM );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmItemDetailsCO ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemDetailsCO );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.PharmPrices ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmPrices );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.Sales ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.Sales );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.SBA ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.SBA );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.ContractAssignment ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractAssignment );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.ContractAwardDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractAwardDate );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.ContractEffectiveDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractEffectiveDate );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.ContractExpirationDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractExpirationDate );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.ContractCompletionDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractCompletionDate );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.PharmContractExpirationDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmContractExpirationDate );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.PharmContractCompletionDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmContractCompletionDate );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.RebateRequired ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.RebateRequired );
            }
            if ( CheckPermissionsAgainstSchedule( scheduleGroupIds,  GetAccessPointId( AccessPoints.RebateTerms ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.RebateTerms );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.InsurancePolicyDates ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.InsurancePolicyDates );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.InsuranceUnlimitedDateRange ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.InsuranceUnlimitedDateRange );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.MedSurgItemExport ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgItemExport );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.MedSurgItemUpload ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgItemUpload );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.PharmItemExport ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemExport );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.PharmItemUpload ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemUpload );
            }
        }

        // set additional access beyond main contract body in the case where the current user is not the owner
        public void SetAdditionalEditStatusesForContractIfNotOwner( CurrentDocument currentDocument, ArrayList scheduleGroupIds, ArrayList allowedOperationalStatusGroupIds )
        {
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.Checks ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.Checks );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.Payments ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.Payments );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.MedSurgItems ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgItems );
            }   
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.MedSurgItemDetails ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgItemDetails );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.MedSurgPrices ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgPrices );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.MedSurgPriceDetails ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgPriceDetails );
            }   
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmItems ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItems );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmBPAItems ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmBPAItems );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmItemRemove ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemRemove );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmItemDiscontinue ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemDiscontinue );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmChangeNDC ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmChangeNDC );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmCopyItem ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmCopyItem );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmCopyItemToContract ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmCopyItemToContract );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmItemDetailsPBM ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemDetailsPBM );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmItemDetailsCO ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemDetailsCO );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmPrices ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmPrices );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.Sales ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.Sales );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.SBA ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.SBA );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.ContractAssignment ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractAssignment );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.ContractAwardDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractAwardDate );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.ContractEffectiveDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractEffectiveDate );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.ContractExpirationDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractExpirationDate );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.ContractCompletionDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractCompletionDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmContractExpirationDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmContractExpirationDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmContractCompletionDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmContractCompletionDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.ContractUnlimitedDateRange ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.ContractUnlimitedDateRange );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.RebateRequired ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.RebateRequired );
            }
            if ( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.RebateTerms ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.RebateTerms );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.InsurancePolicyDates ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.InsurancePolicyDates );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.InsuranceUnlimitedDateRange ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.InsuranceUnlimitedDateRange );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.MedSurgItemExport ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgItemExport );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.MedSurgItemUpload ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.MedSurgItemUpload );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmItemExport ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemExport );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.PharmItemUpload ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.PharmItemUpload );
            }
        }


        public void SetDocumentEditStatusForOffer( CurrentDocument currentDocument )
        {
            currentDocument.EditStatus = CurrentDocument.EditStatuses.Denied; // default
            currentDocument.AllowedAccessPoints.Clear();

            ArrayList scheduleGroupIds = GetScheduleGroupIdsForScheduleNumber( currentDocument.ScheduleNumber );

            // get everything the owner is involved in
            ArrayList ownerOperationalStatusIds = GetOperationalStatusIdsForDocumentOwner( currentDocument.OwnerId );

            // get related operational status id's from hierarchy table
            ArrayList allowedOperationalStatusIdsForOwner = GetAllowedOperationalStatusIdsForOwner( ownerOperationalStatusIds );

            // convert to group ids since that is what the matrix requires
            ArrayList allowedOperationalStatusGroupIds = GetOperationalStatusGroupIdsForOperationalStatusIdList( allowedOperationalStatusIdsForOwner );

            if( _cmRole.DoesNotExist == false )
            {
                if( _cmRole.IsUser == true )
                {
                    if( _cmRole.Active == true )
                    {
                        // allow editing if this is the document owner
                        if( _nacUser.OldUserId == currentDocument.OwnerId )
                        {
                            currentDocument.IsCurrentUserTheOwner = true;
                            currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEdit;
                            SetAdditionalEditStatusesForOfferIfOwner( currentDocument, scheduleGroupIds );
                        }
                        else
                        {
                            currentDocument.IsCurrentUserTheOwner = false;

                            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.Offers ) ) == true )
                            {
                                currentDocument.EditStatus = CurrentDocument.EditStatuses.CanEdit;
                            }

                            // all active users can view
                            if( currentDocument.EditStatus == CurrentDocument.EditStatuses.Denied )
                                currentDocument.EditStatus = CurrentDocument.EditStatuses.CanView;

                            SetAdditionalEditStatusesForOfferIfNotOwner( currentDocument, scheduleGroupIds, allowedOperationalStatusGroupIds );
                        }
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

       // set additional access beyond main contract body in the case where the current user is the owner
        public void SetAdditionalEditStatusesForOfferIfOwner( CurrentDocument currentDocument, ArrayList scheduleGroupIds )
        {
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.OfferReceivedDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferReceivedDate );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.OfferAssignment ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferAssignment );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.OfferAssignmentDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferAssignmentDate );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.OfferReassignmentDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferReassignmentDate );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.OfferActionDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferActionDate );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.OfferEstimatedCompletionDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferEstimatedCompletionDate );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.OfferAuditDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferAuditDate );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.OfferReturnDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferReturnDate );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.OfferUnlimitedDateRange ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferUnlimitedDateRange );
            }
            if( CheckPermissionsAgainstSchedule( scheduleGroupIds, GetAccessPointId( AccessPoints.AdministerOfferDates ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.AdministerOfferDates );
            }
        }
        
        // set additional access beyond main offer body in the case where the current user is not the owner
        public void SetAdditionalEditStatusesForOfferIfNotOwner( CurrentDocument currentDocument, ArrayList scheduleGroupIds, ArrayList allowedOperationalStatusGroupIds )
        {
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.OfferReceivedDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferReceivedDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.OfferAssignment ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferAssignment );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.OfferAssignmentDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferAssignmentDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.OfferReassignmentDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferReassignmentDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.OfferActionDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferActionDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.OfferEstimatedCompletionDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferEstimatedCompletionDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.OfferAuditDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferAuditDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.OfferReturnDate ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferReturnDate );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.OfferUnlimitedDateRange ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.OfferUnlimitedDateRange );
            }
            if( CheckPermissionsAgainstScheduleAndOperationalStatusGroups( scheduleGroupIds, allowedOperationalStatusGroupIds, GetAccessPointId( AccessPoints.AdministerOfferDates ) ) == true )
            {
                currentDocument.AllowedAccessPoints.Add( AccessPoints.AdministerOfferDates );
            }
        }


        // get the user schedule access points
        private DataSet GetSecurityMatrix()
        {
            bool bSuccess = false;
            string msg = "Called GetSecurityMatrix()";

            DataSet dsSecurityMatrix = null;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( _nacUser.LoginName );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.SelectSecurityMatrix( ref dsSecurityMatrix, _nacUser.UserId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetSecurityMatrix() SelectSecurityMatrix() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
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

        // get the user schedule access points operational status groups
        private DataSet GetSecurityMatrix3()
        {
            bool bSuccess = false;
            string msg = "Called GetSecurityMatrix3()";

            DataSet dsSecurityMatrix = null;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( _nacUser.LoginName );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.SelectSecurityMatrix3( ref dsSecurityMatrix, _nacUser.UserId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetSecurityMatrix3() SelectSecurityMatrix3() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetSecurityMatrix3() SelectSecurityMatrix3() returned success." );

                }
            }
            catch( Exception ex )
            {
                //  msg = String.Format( "MTSecurity GetUserSecurityMatrix encountered the following exception: {0}", ex.ToString() );
                throw new Exception( "BrowserSecurity.GetSecurityMatrix3() encountered an exception.", ex );
            }

            return ( dsSecurityMatrix );
        }


        // get access points
        private DataSet GetAccessPoints()
        {
            bool bSuccess = false;
            string msg = "Called GetAccessPoints()";

            DataSet dsAccessPoints = null;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( _nacUser.LoginName );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.SelectAccessPoints( ref dsAccessPoints, _nacUser.UserId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetAccessPoints() SelectAccessPoints() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetAccessPoints() SelectAccessPoints() returned success." );

                }
            }
            catch( Exception ex )
            {
                //  msg = String.Format( "MTSecurity GetUserSecurityMatrix encountered the following exception: {0}", ex.ToString() );
                throw new Exception( "BrowserSecurity.GetAccessPoints() encountered an exception.", ex );
            }

            return ( dsAccessPoints );
        }

        // get operational statuses
        private DataSet GetOperationalStatuses()
        {
            bool bSuccess = false;
            string msg = "Called GetOperationalStatuses()";

            DataSet dsActiveOperationalStatuses = null;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( _nacUser.LoginName );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.SelectActiveOperationalStatuses( ref dsActiveOperationalStatuses, _nacUser.UserId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetOperationalStatuses() SelectActiveOperationalStatuses() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetOperationalStatuses() SelectActiveOperationalStatuses() returned success." );

                }
            }
            catch( Exception ex )
            {
                //  msg = String.Format( "MTSecurity GetUserSecurityMatrix encountered the following exception: {0}", ex.ToString() );
                throw new Exception( "BrowserSecurity.GetOperationalStatuses() encountered an exception.", ex );
            }

            return ( dsActiveOperationalStatuses );
        }

        // get active roles defined in the security database
        private DataSet GetActiveRoles()
        {
            bool bSuccess = false;
            string msg = "Called GetActiveRoles()";

            DataSet dsActiveRoles = null;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( _nacUser.LoginName );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.SelectRoles( ref dsActiveRoles, _nacUser.UserId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetActiveRoles() SelectRoles() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetActiveRoles() SelectRoles() returned success." );

                }
            }
            catch( Exception ex )
            {
                throw new Exception( "BrowserSecurity.GetActiveRoles() encountered an exception.", ex );
            }

            return ( dsActiveRoles );
        }

        // get list of schedules with list of groups to which each belongs
        private DataSet GetReverseLookupScheduleGroups()
        {
            bool bSuccess = false;
            string msg = "Called GetReverseLookupScheduleGroups()";

            DataSet dsReverseLookupScheduleGroups = null;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( _nacUser.LoginName );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.SelectReverseLookupScheduleGroups( ref dsReverseLookupScheduleGroups, _nacUser.UserId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetReverseLookupScheduleGroups() SelectReverseLookupScheduleGroups() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetReverseLookupScheduleGroups() SelectReverseLookupScheduleGroups() returned success." );

                }
            }
            catch( Exception ex )
            {
                //  msg = String.Format( "MTSecurity GetUserSecurityMatrix encountered the following exception: {0}", ex.ToString() );
                throw new Exception( "BrowserSecurity.GetReverseLookupScheduleGroups() encountered an exception.", ex );
            }

            return ( dsReverseLookupScheduleGroups );
        }

        // get list of operational status ids with list of groups to which each belongs
        private DataSet GetReverseLookupOperationalStatusGroups()
        {
            bool bSuccess = false;
            string msg = "Called GetReverseLookupOperationalStatusGroups()";

            DataSet dsReverseLookupOperationalStatusGroups = null;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( _nacUser.LoginName );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.SelectReverseLookupOperationalStatusGroups( ref dsReverseLookupOperationalStatusGroups, _nacUser.UserId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetReverseLookupOperationalStatusGroups() SelectReverseLookupOperationalStatusGroups() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetReverseLookupOperationalStatusGroups() SelectReverseLookupOperationalStatusGroups() returned success." );

                }
            }
            catch( Exception ex )
            {
                //  msg = String.Format( "MTSecurity GetUserSecurityMatrix encountered the following exception: {0}", ex.ToString() );
                throw new Exception( "BrowserSecurity.GetReverseLookupOperationalStatusGroups() encountered an exception.", ex );
            }

            return ( dsReverseLookupOperationalStatusGroups );
        }

        // get list of operational status ids with their associated allowed operational status ids
        private DataSet GetOperationalStatusHierarchy()
        {
            bool bSuccess = false;
            string msg = "Called GeOperationalStatusHierarchy()";

            DataSet dsOperationalStatusHierarchy = null;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( _nacUser.LoginName );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.SelectOperationalStatusHierarchy( ref dsOperationalStatusHierarchy, _nacUser.UserId );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetOperationalStatusHierarchy() SelectOperationalStatusHierarchy() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetOperationalStatusHierarchy() SelectOperationalStatusHierarchy() returned success." );

                }
            }
            catch( Exception ex )
            {
                //  msg = String.Format( "MTSecurity GetUserSecurityMatrix encountered the following exception: {0}", ex.ToString() );
                throw new Exception( "BrowserSecurity.SelectOperationalStatusHierarchy() encountered an exception.", ex );
            }

            return ( dsOperationalStatusHierarchy );
        }

        private void SetUserDetailsAndRoles( ref NACUser2 nacUser, ref CMRole2 cmRole )
        {
            bool bSuccess = true;
            string msg = "Called SetUserDetailsAndRoles()";
 //           _log.WriteLog( msg, LogBase.Severity.InformMediumLevel );

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( NACUser.GetLoginName() );
                userSecurityDB2.MakeConnectionString();

                Guid userGuid = Guid.Empty;
                string firstName = "";
                string lastName = "";
                string fullName = "";
                string email = "";
                string phone = "";
                string status = "";
                int oldUserId = -1;
                int userDivision = -1;

                bSuccess = userSecurityDB2.GetUserInfo( NACUser.GetLoginName(), ref userGuid, ref firstName, ref lastName, ref fullName, ref email, ref phone, ref status, ref oldUserId, ref userDivision );
                if( bSuccess != true )
                {
                    msg = String.Format( "SetUserDetailsAndRoles() GetUserInfo() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
          //          _log.WriteLog( msg, LogBase.Severity.Exception );

                    if( userSecurityDB2.ErrorMessage.Contains( "does not exist" ) == true )
                    {
                        cmRole.SetRoleDetails( false, status, userDivision );
                        nacUser.SetUserDetails( userGuid, firstName, lastName, fullName, email, phone, oldUserId );
                    }
                    else
                    {
                        throw new Exception( msg );
                    }
                }
                else
                {
                    msg = String.Format( "SetUserDetailsAndRoles() GetUserInfo() returned success." );
           //         _log.WriteLog( msg, LogBase.Severity.InformLowLevel );

                    cmRole.SetRoleDetails( true, status, userDivision );
                    nacUser.SetUserDetails( userGuid, firstName, lastName, fullName, email, phone, oldUserId );
                }
            }
            catch( Exception ex )
            {
                msg = String.Format( "Returning the following exception to the client {0}", ex.ToString() );
     //           _log.WriteLog( msg, LogBase.Severity.Exception );
                throw new Exception( "GetUserDetails() encountered an exception.", ex );
            }

        }

        public NACUserList2 GetContractingOfficersForDivision( int divisionId )
        {
            bool bSuccess = true;
            string msg = "Called GetUserDetails()";
     //       _log.WriteLog( msg, LogBase.Severity.InformMediumLevel );

            NACUserList2 nacUserList = null;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( NACUser.GetLoginName() );
                userSecurityDB2.MakeConnectionString();

                DataSet dsUsers = null;
                bSuccess = userSecurityDB2.SelectContractingOfficersForDivision( divisionId, false, ref dsUsers );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetContractingOfficersForDivision() SelectContractingOfficersForDivision() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
           //         _log.WriteLog( msg, LogBase.Severity.Exception );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetContractingOfficersForDivision() SelectContractingOfficersForDivision() returned success." );
          //          _log.WriteLog( msg, LogBase.Severity.InformLowLevel );
                }

                nacUserList = NACUserList2.FromDataSet( dsUsers, "UserTable" );

                if( nacUserList == null )
                {
                    msg = String.Format( "GetContractingOfficersForDivision() NACUserList2.FromDataSet() returned null" );
             //       _log.WriteLog( msg, LogBase.Severity.Exception );
                    throw new Exception( msg );
                }
                else
                {
                    msg = String.Format( "GetContractingOfficersForDivision() NACUserList2.FromDataSet() returned success" );
         //           _log.WriteLog( msg, LogBase.Severity.InformLowLevel );
                }
            }
            catch( Exception ex )
            {
                msg = String.Format( "Returning the following exception to the client {0}", ex.ToString() );
        //        _log.WriteLog( msg, LogBase.Severity.Exception );
                throw new Exception( "GetContractingOfficersForDivision() encountered an exception.", ex );
            }

            return ( nacUserList );
        }

        public bool GetContractingOfficersForDivision( int divisionId, ref DataSet dsContractingOfficersForDivision )
        {
            bool bSuccess = true;
            string msg = "";

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( NACUser.GetLoginName() );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.SelectContractingOfficersForDivision( divisionId, true, ref dsContractingOfficersForDivision );
                if( bSuccess != true )
                {
                    msg = String.Format( "GetContractingOfficersForDivision() SelectContractingOfficersForDivision() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }

            }
            catch( Exception ex )
            {
                bSuccess = false;
                msg = String.Format( "Returning the following exception to the client {0}", ex.ToString() );
                throw new Exception( "GetContractingOfficersForDivision() encountered an exception.", ex );
            }

            return ( bSuccess );
        }

        public bool IsInRole( int scheduleNumber, string roleDescription )
        {
            bool bAllow = false;
            DataTable dtRoles = _activeRoles.Tables[ "ActiveRolesTable" ];
            int roleId = -1;

            // check existence
            int count = dtRoles.AsEnumerable().Where( r => r.Field<string>( "RoleDescription" ) == roleDescription ).Count();

            if( count > 0 )
            {
                var roleCheckQuery = from role in dtRoles.AsEnumerable()
                                     where role.Field<string>( "RoleDescription" ) == roleDescription
                                     select new
                                     {
                                         roleId = role.Field<int>( "RoleId" )
                                     };
                foreach( var r in roleCheckQuery )
                {
                    roleId = r.roleId;
                    break;
                }

                ArrayList scheduleGroupIds = GetScheduleGroupIdsForScheduleNumber( scheduleNumber );

                if( _securityMatrix == null )
                    throw new Exception( "BrowserSecurity2.IsInRole() called prior to security matrix availability" );

                for( int i = 0; i < scheduleGroupIds.Count; i++ )
                {
                    int scheduleGroupId = int.Parse( scheduleGroupIds[ i ].ToString() );
                    bAllow = _securityMatrix.CheckRoleMembership( scheduleGroupId, roleId );
                    if( bAllow == true )
                        break;
                }
            }
            else
            {
                throw new Exception( string.Format( "The application could not find a defined role with the following description: {0}", roleDescription ));
            }

            return ( bAllow );
        }

        // version for any schedule
        public bool IsInRole( string roleDescription )
        {
            bool bAllow = false;
            DataTable dtRoles = _activeRoles.Tables[ "ActiveRolesTable" ];
            int roleId = -1;

            // check existence
            int count = dtRoles.AsEnumerable().Where( r => r.Field<string>( "RoleDescription" ) == roleDescription ).Count();

            if( count > 0 )
            {
                var roleCheckQuery = from role in dtRoles.AsEnumerable()
                                     where role.Field<string>( "RoleDescription" ) == roleDescription
                                     select new
                                     {
                                         roleId = role.Field<int>( "RoleId" )
                                     };
                foreach( var r in roleCheckQuery )
                {
                    roleId = r.roleId;
                    break;
                }

                if( _securityMatrix == null )
                    throw new Exception( "BrowserSecurity2.IsInRole() called prior to security matrix availability" );

                 bAllow = _securityMatrix.CheckRoleMembership( roleId );
            }
            else
            {
                throw new Exception( string.Format( "The application could not find a defined role with the following description: {0}", roleDescription ) );
            }

            return ( bAllow );
        }

        public bool IsUserInBossRole(int COID, out bool bIsUserInBossRole)
        {
            bool bSuccess = true;
            string msg = "";
            bIsUserInBossRole = false;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2(NACUser.GetLoginName());
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.IsUserInBossRole( COID, out bIsUserInBossRole );
                if (bSuccess != true)
                {
                    msg = String.Format("IsUserInBossRole() IsUserInBossRole() encountered the following exception : {0}", userSecurityDB2.ErrorMessage);
                    throw new Exception(msg);
                }

            }
            catch (Exception ex)
            {
                bSuccess = false;
                msg = String.Format("Returning the following exception to the client {0}", ex.ToString());
                throw new Exception("IsUserInBossRole() encountered an exception.", ex);
            }

            return (bSuccess);
        }

        public bool IsUserInFSSChiefRole( int COID, out bool bIsUserInFSSChiefRole )
        {
            bool bSuccess = true;
            string msg = "";
            bIsUserInFSSChiefRole = false;

            try
            {
                UserSecurityDB2 userSecurityDB2 = new UserSecurityDB2( NACUser.GetLoginName() );
                userSecurityDB2.MakeConnectionString();

                bSuccess = userSecurityDB2.IsUserInFSSChiefRole( COID, out bIsUserInFSSChiefRole );
                if( bSuccess != true )
                {
                    msg = String.Format( "IsUserInFSSChiefRole() IsUserInFSSChiefRole() encountered the following exception : {0}", userSecurityDB2.ErrorMessage );
                    throw new Exception( msg );
                }

            }
            catch( Exception ex )
            {
                bSuccess = false;
                msg = String.Format( "Returning the following exception to the client {0}", ex.ToString() );
                throw new Exception( "IsUserInFSSChiefRole() encountered an exception.", ex );
            }

            return ( bSuccess );
        }

        #region ISerializable Members

        [SecurityCritical]
        public void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "CMRole2", _cmRole );
            info.AddValue( "NACUser2", _nacUser );
            info.AddValue( "AccessPoints", _accessPoints );
            info.AddValue( "ActiveRoles", _activeRoles );
            info.AddValue( "ReverseLookupScheduleGroups", _reverseLookupScheduleGroups );
            info.AddValue( "PermissionDeniedMessage", _permissionDeniedMessage );
            info.AddValue( "SecurityMatrix3", _securityMatrix );
            info.AddValue( "OperationalStatuses", _operationalStatuses );
            info.AddValue( "ReverseLookupOperationalStatusGroups", _reverseLookupOperationalStatusGroups );
            info.AddValue( "OperationalStatusHierarchy", _operationalStatusHierarchy );
        }

        public BrowserSecurity2( SerializationInfo info, StreamingContext context )
        {
            _cmRole = ( CMRole2 )info.GetValue( "CMRole2", typeof( CMRole2 ) );
            _nacUser = ( NACUser2 )info.GetValue( "NACUser2", typeof( NACUser2 ) );
            _accessPoints = ( DataSet )info.GetValue( "AccessPoints", typeof( DataSet ) );
            _activeRoles = ( DataSet )info.GetValue( "ActiveRoles", typeof( DataSet ) );
            _reverseLookupScheduleGroups = ( DataSet )info.GetValue( "ReverseLookupScheduleGroups", typeof( DataSet ) );
            _permissionDeniedMessage = info.GetString( "PermissionDeniedMessage" );
            _securityMatrix = ( SecurityMatrix3 )info.GetValue( "SecurityMatrix3", typeof( SecurityMatrix3 ) );
            _operationalStatuses = ( DataSet )info.GetValue( "OperationalStatuses", typeof( DataSet ) );
            _reverseLookupOperationalStatusGroups = ( DataSet )info.GetValue( "ReverseLookupOperationalStatusGroups", typeof( DataSet ) );
            _operationalStatusHierarchy = ( DataSet )info.GetValue( "OperationalStatusHierarchy", typeof( DataSet ) );
        }

        #endregion
    }
}
