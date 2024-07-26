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
using System.Web.Caching;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Security;

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;

namespace VA.NAC.NACCMBrowser.DBInterface
{
    // provides ado.net access to contract releated stored procedures in the NAC CM database
    [Serializable]
    public class OfferDB : DBCommon, ISerializable
    {
        private Guid _currentUserId = Guid.Empty;
        private string _userName = string.Empty;
        private int _oldUserId = -1;

        public OfferDB()
            : base()
        {
        }

        public OfferDB( Guid currentUserId, string userName, int oldUserId )
            : base()
        {
            _currentUserId = currentUserId;
            _userName = userName;
            _oldUserId = oldUserId;
        }

        public bool SelectOfferHeaders( ref DataSet dsOfferHeaders, string offerStatusFilter, string offerOwnerFilter, string filterType, string filterValue, string sortExpression, string sortDirection )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOfferHeaders = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                // SelectOfferHeaders
                //(
                //@CurrentUser uniqueidentifier,
                //@COID int,
                //@LoginId nvarchar(120),
                //@OfferStatusFilter nchar(1),  /* A - All, O - Open, C - Completed, new for release 2 N - none */
                //@OfferOwnerFilter nchar(1), /* A - All, M - Mine */
                //@FilterType nchar(1), /* O - CO Name, V - Vendor, T - Status, S - Schedule, X = none */
                //@FilterValue nvarchar(200),
                //@SortExpression nvarchar(100),
                //@SortDirection nvarchar(20)

                SqlCommand cmdSelectContractHeadersQuery = new SqlCommand( "SelectOfferHeaders", dbConnection );
                cmdSelectContractHeadersQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractHeadersQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                SqlParameter parmLoginId = new SqlParameter( "@LoginId", SqlDbType.NVarChar, 120 );
                SqlParameter parmOfferStatusFilter = new SqlParameter( "@OfferStatusFilter", SqlDbType.NChar, 1 );
                SqlParameter parmOfferOwnerFilter = new SqlParameter( "@OfferOwnerFilter", SqlDbType.NChar, 1 );
                SqlParameter parmFilterType = new SqlParameter( "@FilterType", SqlDbType.NChar, 1 );
                SqlParameter parmFilterValue = new SqlParameter( "@FilterValue", SqlDbType.NVarChar, 200 );
                SqlParameter parmSortExpression = new SqlParameter( "@SortExpression", SqlDbType.NVarChar, 100 );
                SqlParameter parmSortDirection = new SqlParameter( "@SortDirection", SqlDbType.NVarChar, 20 );

                parmCurrentUser.Value = _currentUserId;
                parmCOID.Value = _oldUserId;
                parmLoginId.Value = _userName;
                parmOfferStatusFilter.Value = offerStatusFilter;
                parmOfferOwnerFilter.Value = offerOwnerFilter;
                parmFilterType.Value = filterType;
                string cleansedFilterValue = filterValue.Replace( "'", "''" );
                parmFilterValue.Value = cleansedFilterValue;
                parmSortExpression.Value = sortExpression;
                parmSortDirection.Value = sortDirection;

                cmdSelectContractHeadersQuery.Parameters.Add( parmCurrentUser );
                cmdSelectContractHeadersQuery.Parameters.Add( parmCOID );
                cmdSelectContractHeadersQuery.Parameters.Add( parmLoginId );
                cmdSelectContractHeadersQuery.Parameters.Add( parmOfferStatusFilter );
                cmdSelectContractHeadersQuery.Parameters.Add( parmOfferOwnerFilter );
                cmdSelectContractHeadersQuery.Parameters.Add( parmFilterType );
                cmdSelectContractHeadersQuery.Parameters.Add( parmFilterValue );
                cmdSelectContractHeadersQuery.Parameters.Add( parmSortExpression );
                cmdSelectContractHeadersQuery.Parameters.Add( parmSortDirection );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferHeaders = new SqlDataAdapter();
                daOfferHeaders.SelectCommand = cmdSelectContractHeadersQuery;

                dsOfferHeaders = new DataSet( "OfferHeaders" );
                DataTable dtOfferHeaders = dsOfferHeaders.Tables.Add( "OfferHeadersTable" );

                // add the common elements to the table
                DataColumn offerIdColumn = new DataColumn( "Offer_ID", typeof( int ) );

                dtOfferHeaders.Columns.Add( offerIdColumn );

                dtOfferHeaders.Columns.Add( "Solicitation_Number", typeof( string ) );
                dtOfferHeaders.Columns.Add( "CO_ID", typeof( int ) );
                dtOfferHeaders.Columns.Add( "FullName", typeof( string ) );
                dtOfferHeaders.Columns.Add( "Schedule_Number", typeof( int ) );
                dtOfferHeaders.Columns.Add( "Schedule_Name", typeof( string ) );
                dtOfferHeaders.Columns.Add( "OfferNumber", typeof( string ) );  // added for R2

                dtOfferHeaders.Columns.Add( "ProposalTypeId", typeof( int ) );   // added for R2
                dtOfferHeaders.Columns.Add( "Proposal_Type_Description", typeof( string ) );
                dtOfferHeaders.Columns.Add( "Action_Description", typeof( string ) );
                dtOfferHeaders.Columns.Add( "Complete", typeof( bool ) );

                dtOfferHeaders.Columns.Add( "Contractor_Name", typeof( string ) );

                dtOfferHeaders.Columns.Add( "Dates_Received", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Assigned", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Reassigned", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Action", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Expected_Completion", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Sent_for_Preaward", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Returned_to_Office", typeof( DateTime ) );

                dtOfferHeaders.Columns.Add( "ContractNumber", typeof( string ) );
                dtOfferHeaders.Columns.Add( "ExtendsContractNumber", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerIdColumn;

                // add the keys to the table
                dtOfferHeaders.PrimaryKey = primaryKeyColumns;

                dtOfferHeaders.Clear();

                // connect
                dbConnection.Open();

                // run
                daOfferHeaders.Fill( dsOfferHeaders, "OfferHeadersTable" );

                RowsReturned = dsOfferHeaders.Tables[ "OfferHeadersTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.SelectOfferHeaders(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool SelectOfferHeaders2( ref DataSet dsOfferHeaders, string offerStatusFilter, string offerOwnerFilter, string filterType, string filterValue, string sortExpression, string sortDirection )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOfferHeaders = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                // SelectOfferHeaders2
                //(
                //@CurrentUser uniqueidentifier,
                //@COID int,
                //@LoginId nvarchar(120),
                //@OfferStatusFilter nchar(1),  /* A - All, O - Open, C - Completed, new for release 2 N - none */
                //@OfferOwnerFilter nchar(1), /* A - All, M - Mine */
                //@FilterType nchar(1), /* O - CO Name, V - Vendor, T - Status, S - Schedule, X = none */
                //@FilterValue nvarchar(200),
                //@SortExpression nvarchar(100),
                //@SortDirection nvarchar(20)

                SqlCommand cmdSelectContractHeadersQuery = new SqlCommand( "SelectOfferHeaders2", dbConnection );
                cmdSelectContractHeadersQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractHeadersQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                SqlParameter parmLoginId = new SqlParameter( "@LoginId", SqlDbType.NVarChar, 120 );
                SqlParameter parmOfferStatusFilter = new SqlParameter( "@OfferStatusFilter", SqlDbType.NChar, 1 );
                SqlParameter parmOfferOwnerFilter = new SqlParameter( "@OfferOwnerFilter", SqlDbType.NChar, 1 );
                SqlParameter parmFilterType = new SqlParameter( "@FilterType", SqlDbType.NChar, 1 );
                SqlParameter parmFilterValue = new SqlParameter( "@FilterValue", SqlDbType.NVarChar, 200 );
                SqlParameter parmSortExpression = new SqlParameter( "@SortExpression", SqlDbType.NVarChar, 100 );
                SqlParameter parmSortDirection = new SqlParameter( "@SortDirection", SqlDbType.NVarChar, 20 );

                parmCurrentUser.Value = _currentUserId;
                parmCOID.Value = _oldUserId;
                parmLoginId.Value = _userName;
                parmOfferStatusFilter.Value = offerStatusFilter;
                parmOfferOwnerFilter.Value = offerOwnerFilter;
                parmFilterType.Value = filterType;
                parmFilterValue.Value = filterValue;
                parmSortExpression.Value = sortExpression;
                parmSortDirection.Value = sortDirection;

                cmdSelectContractHeadersQuery.Parameters.Add( parmCurrentUser );
                cmdSelectContractHeadersQuery.Parameters.Add( parmCOID );
                cmdSelectContractHeadersQuery.Parameters.Add( parmLoginId );
                cmdSelectContractHeadersQuery.Parameters.Add( parmOfferStatusFilter );
                cmdSelectContractHeadersQuery.Parameters.Add( parmOfferOwnerFilter );
                cmdSelectContractHeadersQuery.Parameters.Add( parmFilterType );
                cmdSelectContractHeadersQuery.Parameters.Add( parmFilterValue );
                cmdSelectContractHeadersQuery.Parameters.Add( parmSortExpression );
                cmdSelectContractHeadersQuery.Parameters.Add( parmSortDirection );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferHeaders = new SqlDataAdapter();
                daOfferHeaders.SelectCommand = cmdSelectContractHeadersQuery;

                dsOfferHeaders = new DataSet( "OfferHeaders" );
                DataTable dtOfferHeaders = dsOfferHeaders.Tables.Add( "OfferHeadersTable" );

                // add the common elements to the table
                DataColumn offerIdColumn = new DataColumn( "Offer_ID", typeof( int ) );

                dtOfferHeaders.Columns.Add( offerIdColumn );

                dtOfferHeaders.Columns.Add( "Solicitation_Number", typeof( string ) );
                dtOfferHeaders.Columns.Add( "CO_ID", typeof( int ) );
                dtOfferHeaders.Columns.Add( "FullName", typeof( string ) );
                dtOfferHeaders.Columns.Add( "Schedule_Number", typeof( int ) );
                dtOfferHeaders.Columns.Add( "Schedule_Name", typeof( string ) );
                dtOfferHeaders.Columns.Add( "OfferNumber", typeof( string ) );  // added for R2

                dtOfferHeaders.Columns.Add( "ProposalTypeId", typeof( int ) );   // added for R2
                dtOfferHeaders.Columns.Add( "Proposal_Type_Description", typeof( string ) );
                dtOfferHeaders.Columns.Add( "Action_Description", typeof( string ) );
                dtOfferHeaders.Columns.Add( "Complete", typeof( bool ) );

                dtOfferHeaders.Columns.Add( "Contractor_Name", typeof( string ) );

                dtOfferHeaders.Columns.Add( "Dates_Received", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Assigned", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Reassigned", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Action", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Expected_Completion", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Sent_for_Preaward", typeof( DateTime ) );
                dtOfferHeaders.Columns.Add( "Dates_Returned_to_Office", typeof( DateTime ) );

                dtOfferHeaders.Columns.Add( "ContractNumber", typeof( string ) );
                dtOfferHeaders.Columns.Add( "ContractId", typeof( int ) );
                dtOfferHeaders.Columns.Add( "ExtendsContractNumber", typeof( string ) );
                dtOfferHeaders.Columns.Add( "ExtendsContractId", typeof( int ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerIdColumn;

                // add the keys to the table
                dtOfferHeaders.PrimaryKey = primaryKeyColumns;

                dtOfferHeaders.Clear();

                // connect
                dbConnection.Open();

                // run
                daOfferHeaders.Fill( dsOfferHeaders, "OfferHeadersTable" );

                RowsReturned = dsOfferHeaders.Tables[ "OfferHeadersTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.SelectOfferHeaders2(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // get offer info to be used for pre-populating the contract creation screen when a contract is created from an offer
        // this version no longer used
        public bool GetOfferInfo( ref DataSet dsOneOfferRow, int offerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOneOfferRow = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetOfferInfo
                //(
                //@CurrentUser uniqueidentifier,
                //@OfferId int
                //)


                SqlCommand cmdSelectOfferQuery = new SqlCommand( "GetOfferInfo", dbConnection );
                cmdSelectOfferQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                parmCurrentUser.Value = _currentUserId;
                parmOfferId.Value = offerId;

                cmdSelectOfferQuery.Parameters.Add( parmCurrentUser );
                cmdSelectOfferQuery.Parameters.Add( parmOfferId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOneOfferRow = new SqlDataAdapter();
                daOneOfferRow.SelectCommand = cmdSelectOfferQuery;

                dsOneOfferRow = new DataSet( "OneOfferRow" );
                DataTable dtOneOfferRow = dsOneOfferRow.Tables.Add( "OneOfferRowTable" );

                // add the common elements to the table
                DataColumn offerIdColumn = new DataColumn( "OfferId", typeof( int ) );

                dtOneOfferRow.Columns.Add( offerIdColumn );

                dtOneOfferRow.Columns.Add( "AssignedCOID", typeof( int ) );
                dtOneOfferRow.Columns.Add( "DivisionId", typeof( int ) );
                dtOneOfferRow.Columns.Add( "ScheduleNumber", typeof( int ) );
                dtOneOfferRow.Columns.Add( "RebateRequired", typeof( bool ) );
                dtOneOfferRow.Columns.Add( "VendorName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddress1", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddress2", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddressCity", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddressState", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorZipCode", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorUrl", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactPhone", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactPhoneExtension", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactFax", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactEmail", typeof( string ) );
                dtOneOfferRow.Columns.Add( "OwnerName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "ScheduleName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "OfferNumber", typeof( string ) );
                dtOneOfferRow.Columns.Add( "IsOfferComplete", typeof( bool ) );
                dtOneOfferRow.Columns.Add( "ProposalTypeId", typeof( int ) );
                dtOneOfferRow.Columns.Add( "DateOfferReceived", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "DateOfferAssigned", typeof( DateTime ) );                              
                dtOneOfferRow.Columns.Add( "ContractNumber", typeof( string ) );
                dtOneOfferRow.Columns.Add( "ExtendsContractNumber", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerIdColumn;

                // add the keys to the table
                dtOneOfferRow.PrimaryKey = primaryKeyColumns;

                dtOneOfferRow.Clear();

                // connect
                dbConnection.Open();

                // run
                daOneOfferRow.Fill( dsOneOfferRow, "OneOfferRowTable" );

                RowsReturned = dsOneOfferRow.Tables[ "OneOfferRowTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetOfferInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // get offer info to be used for pre-populating the contract creation screen when a contract is created from an offer
        public bool GetOfferInfo2( ref DataSet dsOneOfferRow, int offerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOneOfferRow = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetOfferInfo2
                //(
                //@CurrentUser uniqueidentifier,
                //@OfferId int
                //)


                SqlCommand cmdSelectOfferQuery = new SqlCommand( "GetOfferInfo2", dbConnection );
                cmdSelectOfferQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                parmCurrentUser.Value = _currentUserId;
                parmOfferId.Value = offerId;

                cmdSelectOfferQuery.Parameters.Add( parmCurrentUser );
                cmdSelectOfferQuery.Parameters.Add( parmOfferId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOneOfferRow = new SqlDataAdapter();
                daOneOfferRow.SelectCommand = cmdSelectOfferQuery;

                dsOneOfferRow = new DataSet( "OneOfferRow" );
                DataTable dtOneOfferRow = dsOneOfferRow.Tables.Add( "OneOfferRowTable" );

                // add the common elements to the table
                DataColumn offerIdColumn = new DataColumn( "OfferId", typeof( int ) );

                dtOneOfferRow.Columns.Add( offerIdColumn );

                dtOneOfferRow.Columns.Add( "AssignedCOID", typeof( int ) );
                dtOneOfferRow.Columns.Add( "DivisionId", typeof( int ) );
                dtOneOfferRow.Columns.Add( "ScheduleNumber", typeof( int ) );
                dtOneOfferRow.Columns.Add( "SolicitationId", typeof( int ) );
                dtOneOfferRow.Columns.Add( "SolicitationNumber", typeof( string ) );
                dtOneOfferRow.Columns.Add( "RebateRequired", typeof( bool ) );
                dtOneOfferRow.Columns.Add( "VendorName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddress1", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddress2", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddressCity", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddressCountryId", typeof( int ) );
                dtOneOfferRow.Columns.Add( "VendorAddressCountryName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddressState", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorZipCode", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorUrl", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactPhone", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactPhoneExtension", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactFax", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactEmail", typeof( string ) );
                dtOneOfferRow.Columns.Add( "OwnerName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "ScheduleName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "OfferNumber", typeof( string ) );
                dtOneOfferRow.Columns.Add( "IsOfferComplete", typeof( bool ) );
                dtOneOfferRow.Columns.Add( "ProposalTypeId", typeof( int ) );
                dtOneOfferRow.Columns.Add( "DateOfferReceived", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "DateOfferAssigned", typeof( DateTime ) );

                dtOneOfferRow.Columns.Add( "ContractNumber", typeof( string ) );
                dtOneOfferRow.Columns.Add( "ContractId", typeof( int ) );
                dtOneOfferRow.Columns.Add( "ExtendsContractNumber", typeof( string ) );
                dtOneOfferRow.Columns.Add( "ExtendsContractId", typeof( int ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerIdColumn;

                // add the keys to the table
                dtOneOfferRow.PrimaryKey = primaryKeyColumns;

                dtOneOfferRow.Clear();

                // connect
                dbConnection.Open();

                // run
                daOneOfferRow.Fill( dsOneOfferRow, "OneOfferRowTable" );

                RowsReturned = dsOneOfferRow.Tables[ "OneOfferRowTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetOfferInfo2(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public static string OFFERINFODETAILSTABLENAME = "OneOfferRowTable";

        // release 2 to populate relay object
        public bool GetOfferInfoDetails( ref DataSet dsOneOfferRow, int offerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOneOfferRow = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetOfferInfoDetails
                //(
                //@CurrentUser uniqueidentifier,
                //@OfferId int
                //)


                SqlCommand cmdSelectOfferQuery = new SqlCommand( "GetOfferInfoDetails", dbConnection );
                cmdSelectOfferQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                parmCurrentUser.Value = _currentUserId;
                parmOfferId.Value = offerId;

                cmdSelectOfferQuery.Parameters.Add( parmCurrentUser );
                cmdSelectOfferQuery.Parameters.Add( parmOfferId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOneOfferRow = new SqlDataAdapter();
                daOneOfferRow.SelectCommand = cmdSelectOfferQuery;

                dsOneOfferRow = new DataSet( "OneOfferRow" );
                DataTable dtOneOfferRow = dsOneOfferRow.Tables.Add( OFFERINFODETAILSTABLENAME );

                // add the common elements to the table
                DataColumn offerIdColumn = new DataColumn( "OfferId", typeof( int ) );

                dtOneOfferRow.Columns.Add( offerIdColumn );

                dtOneOfferRow.Columns.Add( "SolicitationId", typeof( int ) );
                dtOneOfferRow.Columns.Add( "ActionId", typeof( int ) );
                dtOneOfferRow.Columns.Add( "IsOfferComplete", typeof( bool ) );
                dtOneOfferRow.Columns.Add( "OfferActionDescription", typeof( string ) );
         
                dtOneOfferRow.Columns.Add( "VendorName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddress1", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddress2", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddressCity", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorAddressState", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorZipCode", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorCountry", typeof( string ) );  // this is the old string version of country in an offer which is no longer used
                dtOneOfferRow.Columns.Add( "VendorCountryId", typeof( int ) );

                dtOneOfferRow.Columns.Add( "VendorContactName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactPhone", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactPhoneExtension", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactFax", typeof( string ) );
                dtOneOfferRow.Columns.Add( "VendorContactEmail", typeof( string ) );

                dtOneOfferRow.Columns.Add( "VendorUrl", typeof( string ) );

                dtOneOfferRow.Columns.Add( "DateReceived", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "DateAssigned", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "DateReassigned", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "ActionDate", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "ExpectedCompletionDate", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "ExpirationDate", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "DateSentForPreAward", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "DateReturnedToOffice", typeof( DateTime ) );

                dtOneOfferRow.Columns.Add( "OfferComment", typeof( string ) );
                dtOneOfferRow.Columns.Add( "AuditIndicator", typeof( bool ) );

                dtOneOfferRow.Columns.Add( "CreatedBy", typeof( string ) );
                dtOneOfferRow.Columns.Add( "CreationDate", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "LastModifiedBy", typeof( string ) );            
                dtOneOfferRow.Columns.Add( "LastModificationDate", typeof( DateTime ) );

                dtOneOfferRow.Columns.Add( "SolicitationNumber", typeof( string ) );
                dtOneOfferRow.Columns.Add( "ContractingOfficerPhone", typeof( string ) );
                dtOneOfferRow.Columns.Add( "Schedule_Manager", typeof( int ) );      // these 4 from sched/cat table
                dtOneOfferRow.Columns.Add( "Asst_Director", typeof( int ) );
                dtOneOfferRow.Columns.Add( "ScheduleManagerName", typeof( string ) );
                dtOneOfferRow.Columns.Add( "AssistantDirectorName", typeof( string ) );  

                dtOneOfferRow.Columns.Add( "ContractAwardDate", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "ContractExpirationDate", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "ContractCompletionDate", typeof( DateTime ) );
                dtOneOfferRow.Columns.Add( "AwardedContractingOfficerFullName", typeof( string ) );
          
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerIdColumn;

                // add the keys to the table
                dtOneOfferRow.PrimaryKey = primaryKeyColumns;

                dtOneOfferRow.Clear();

                // connect
                dbConnection.Open();

                // run
                daOneOfferRow.Fill( dsOneOfferRow, OFFERINFODETAILSTABLENAME );

                RowsReturned = dsOneOfferRow.Tables[ OFFERINFODETAILSTABLENAME ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetOfferInfoDetails(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateOfferGeneralOfferAttributes( int offerId, int solicitationId, string offerNumber, int scheduleNumber, int COID, int proposalTypeId, string vendorName, string extendsContractNumber )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateOfferGeneralOfferAttributes 
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@OfferId int,
                //@OfferNumber nvarchar(30),
                //@ScheduleNumber int,
                //@COID int,
                //@ProposalTypeId int,
                //@VendorName nvarchar(75),
                //@SolicitationId int,
                //@ExtendsContractNumber nvarchar(20)

                SqlCommand cmdUpdateOfferAttributesQuery = new SqlCommand( "UpdateOfferGeneralOfferAttributes", dbConnection );
                cmdUpdateOfferAttributesQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateOfferAttributesQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateOfferAttributesQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateOfferAttributesQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                SqlParameter parmOfferNumber = new SqlParameter( "@OfferNumber", SqlDbType.NVarChar, 30 );
                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                SqlParameter parmProposalTypeId = new SqlParameter( "@ProposalTypeId", SqlDbType.Int );
                SqlParameter parmVendorName = new SqlParameter( "@VendorName", SqlDbType.NVarChar, 75 );
                SqlParameter parmSolicitationId = new SqlParameter( "@SolicitationId", SqlDbType.Int );
                SqlParameter parmExtendsContractNumber = new SqlParameter( "@ExtendsContractNumber", SqlDbType.NVarChar, 20 );

                parmOfferId.Value = offerId;

                parmOfferNumber.Value = offerNumber;
                parmScheduleNumber.Value = scheduleNumber;
                parmCOID.Value = COID;
                parmProposalTypeId.Value = proposalTypeId;
                parmVendorName.Value = vendorName;
                parmSolicitationId.Value = solicitationId;
                parmExtendsContractNumber.Value = extendsContractNumber;

                cmdUpdateOfferAttributesQuery.Parameters.Add( parmOfferId );

                cmdUpdateOfferAttributesQuery.Parameters.Add( parmOfferNumber );
                cmdUpdateOfferAttributesQuery.Parameters.Add( parmScheduleNumber );
                cmdUpdateOfferAttributesQuery.Parameters.Add( parmCOID );
                cmdUpdateOfferAttributesQuery.Parameters.Add( parmProposalTypeId );
                cmdUpdateOfferAttributesQuery.Parameters.Add( parmVendorName );
                cmdUpdateOfferAttributesQuery.Parameters.Add( parmSolicitationId );
                cmdUpdateOfferAttributesQuery.Parameters.Add( parmExtendsContractNumber );

                // connect
                dbConnection.Open();

                cmdUpdateOfferAttributesQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.UpdateOfferGeneralOfferAttributes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetOfferOwnerRelatedInfo( int offerId, ref int COID, ref string contractingOfficerFullName, ref string contractingOfficerPhone, ref Guid contractingOfficerUserId, ref int assistantDirectorCOID, ref string assistantDirectorName )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetOfferOwnerInfo
                //(
                //@CurrentUser uniqueidentifier,
                //@UserLogin nvarchar(120),
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@OfferId int,
                //@COID int OUTPUT,
                //@ContractingOfficerFullName nvarchar(80) OUTPUT,
                //@ContractingOfficerPhone nvarchar(20) OUTPUT,
                //@ContractingOfficerUserId uniqueidentifier OUTPUT,            
                //@AssistantDirectorCOID int OUTPUT,
                //@AssistantDirectorName nvarchar(80) OUTPUT

                SqlCommand cmdGetOfferOwnerInfoQuery = new SqlCommand( "GetOfferOwnerInfo", dbConnection );
                cmdGetOfferOwnerInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetOfferOwnerInfoQuery.CommandTimeout = 30;

                AddStandardParameter( cmdGetOfferOwnerInfoQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdGetOfferOwnerInfoQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
   
                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                parmCOID.Direction = ParameterDirection.Output;

                SqlParameter parmContractingOfficerFullName = new SqlParameter( "@ContractingOfficerFullName", SqlDbType.NVarChar, 80 );
                parmContractingOfficerFullName.Direction = ParameterDirection.Output;

                SqlParameter parmContractingOfficerPhone = new SqlParameter( "@ContractingOfficerPhone", SqlDbType.NVarChar, 20 );
                parmContractingOfficerPhone.Direction = ParameterDirection.Output;

                SqlParameter parmContractingOfficerUserId = new SqlParameter( "@ContractingOfficerUserId", SqlDbType.UniqueIdentifier );
                parmContractingOfficerUserId.Direction = ParameterDirection.Output;

                SqlParameter parmAssistantDirectorCOID = new SqlParameter( "@AssistantDirectorCOID", SqlDbType.Int );
                parmAssistantDirectorCOID.Direction = ParameterDirection.Output;

                SqlParameter parmAssistantDirectorName = new SqlParameter( "@AssistantDirectorName", SqlDbType.NVarChar, 80 );
                parmAssistantDirectorName.Direction = ParameterDirection.Output;

                parmUserLogin.Value = _userName;
                parmOfferId.Value = offerId;

                cmdGetOfferOwnerInfoQuery.Parameters.Add( parmUserLogin );
                cmdGetOfferOwnerInfoQuery.Parameters.Add( parmOfferId );

                cmdGetOfferOwnerInfoQuery.Parameters.Add( parmCOID );
                cmdGetOfferOwnerInfoQuery.Parameters.Add( parmContractingOfficerFullName );
                cmdGetOfferOwnerInfoQuery.Parameters.Add( parmContractingOfficerPhone );
                cmdGetOfferOwnerInfoQuery.Parameters.Add( parmContractingOfficerUserId );

                cmdGetOfferOwnerInfoQuery.Parameters.Add( parmAssistantDirectorCOID );
                cmdGetOfferOwnerInfoQuery.Parameters.Add( parmAssistantDirectorName );

                // connect
                dbConnection.Open();

                cmdGetOfferOwnerInfoQuery.ExecuteNonQuery();

                COID = int.Parse( cmdGetOfferOwnerInfoQuery.Parameters[ "@COID" ].Value.ToString() );
                contractingOfficerFullName = cmdGetOfferOwnerInfoQuery.Parameters[ "@ContractingOfficerFullName" ].Value.ToString();
                contractingOfficerPhone = cmdGetOfferOwnerInfoQuery.Parameters[ "@ContractingOfficerPhone" ].Value.ToString();
                contractingOfficerUserId = new Guid( cmdGetOfferOwnerInfoQuery.Parameters[ "@ContractingOfficerUserId" ].Value.ToString() );
                assistantDirectorCOID = int.Parse( cmdGetOfferOwnerInfoQuery.Parameters[ "@AssistantDirectorCOID" ].Value.ToString() );
                assistantDirectorName = cmdGetOfferOwnerInfoQuery.Parameters[ "@AssistantDirectorName" ].Value.ToString();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.GetOfferOwnerRelatedInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );

        }


        public bool UpdateOfferGeneralOfferDates( int offerId, DateTime dateReceived, DateTime dateAssigned, DateTime dateReassigned )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateOfferGeneralOfferDates 
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@OfferId int,
                //@DateReceived datetime,
                //@DateAssigned datetime,
                //@DateReassigned datetime

                SqlCommand cmdUpdateOfferDatesQuery = new SqlCommand( "UpdateOfferGeneralOfferDates", dbConnection );
                cmdUpdateOfferDatesQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateOfferDatesQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateOfferDatesQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateOfferDatesQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                SqlParameter parmDateReceived = new SqlParameter( "@DateReceived", SqlDbType.DateTime );
                SqlParameter parmDateAssigned = new SqlParameter( "@DateAssigned", SqlDbType.DateTime );
                SqlParameter parmDateReassigned = new SqlParameter( "@DateReassigned", SqlDbType.DateTime );
             
                parmOfferId.Value = offerId;

                parmDateReceived.Value = GetDBDate( dateReceived );
                parmDateAssigned.Value = GetDBDate( dateAssigned );  
                parmDateReassigned.Value = GetDBDate( dateReassigned );
                
                cmdUpdateOfferDatesQuery.Parameters.Add( parmOfferId );

                cmdUpdateOfferDatesQuery.Parameters.Add( parmDateReceived );
                cmdUpdateOfferDatesQuery.Parameters.Add( parmDateAssigned );
                cmdUpdateOfferDatesQuery.Parameters.Add( parmDateReassigned );
 
                // connect
                dbConnection.Open();

                cmdUpdateOfferDatesQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.UpdateOfferGeneralOfferDates(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateOfferGeneralActionStatus( int offerId, int actionId, DateTime actionDate, DateTime expectedCompletionDate, DateTime expirationDate, ref bool bIsOfferCompleted )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            bIsOfferCompleted = false;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateOfferGeneralActionStatus
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@OfferId int,
                //@ActionId int,
                //@ActionDate datetime,
                //@ExpectedCompletionDate datetime = null,
                //@ExpirationDate datetime = null,
                //@IsOfferCompleted bit OUTPUT

                SqlCommand cmdUpdateOfferActionStatusQuery = new SqlCommand( "UpdateOfferGeneralActionStatus", dbConnection );
                cmdUpdateOfferActionStatusQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateOfferActionStatusQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateOfferActionStatusQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateOfferActionStatusQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                SqlParameter parmActionId = new SqlParameter( "@ActionId", SqlDbType.Int );
                SqlParameter parmActionDate = new SqlParameter( "@ActionDate", SqlDbType.DateTime );
                SqlParameter parmExpectedCompletionDate = new SqlParameter( "@ExpectedCompletionDate", SqlDbType.DateTime );
                SqlParameter parmExpirationDate = new SqlParameter( "@ExpirationDate", SqlDbType.DateTime );
                SqlParameter parmIsOfferCompleted = new SqlParameter( "@IsOfferCompleted", SqlDbType.Bit );
                parmIsOfferCompleted.Direction = ParameterDirection.Output;

                parmOfferId.Value = offerId;

                parmActionId.Value = actionId;
                parmActionDate.Value = GetDBDate( actionDate );
                parmExpectedCompletionDate.Value = GetDBDate( expectedCompletionDate );
                parmExpirationDate.Value = GetDBDate( expirationDate );

                cmdUpdateOfferActionStatusQuery.Parameters.Add( parmOfferId );

                cmdUpdateOfferActionStatusQuery.Parameters.Add( parmActionId );
                cmdUpdateOfferActionStatusQuery.Parameters.Add( parmActionDate );
                cmdUpdateOfferActionStatusQuery.Parameters.Add( parmExpectedCompletionDate );
                cmdUpdateOfferActionStatusQuery.Parameters.Add( parmExpirationDate );
                cmdUpdateOfferActionStatusQuery.Parameters.Add( parmIsOfferCompleted );

                // connect
                dbConnection.Open();

                cmdUpdateOfferActionStatusQuery.ExecuteNonQuery();

                bIsOfferCompleted = bool.Parse( cmdUpdateOfferActionStatusQuery.Parameters[ "@IsOfferCompleted" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.UpdateOfferGeneralActionStatus(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateOfferGeneralAuditInformation( int offerId, bool bAuditIndicator, DateTime dateSentForPreaward, DateTime dateReturnedToOffice )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateOfferGeneralAuditInformation
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@OfferId int,
                //@AuditIndicator bit,
                //@DateSentForPreaward datetime,
                //@DateReturnedToOffice datetime

                SqlCommand cmdUpdateOfferAuditInformationQuery = new SqlCommand( "UpdateOfferGeneralAuditInformation", dbConnection );
                cmdUpdateOfferAuditInformationQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateOfferAuditInformationQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateOfferAuditInformationQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateOfferAuditInformationQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                SqlParameter parmAuditIndicator = new SqlParameter( "@AuditIndicator", SqlDbType.Bit );
                SqlParameter parmDateSentForPreaward = new SqlParameter( "@DateSentForPreaward", SqlDbType.DateTime );
                SqlParameter parmDateReturnedToOffice = new SqlParameter( "@DateReturnedToOffice", SqlDbType.DateTime );
 
                parmOfferId.Value = offerId;

                parmAuditIndicator.Value = bAuditIndicator;
                parmDateSentForPreaward.Value = GetDBDate( dateSentForPreaward );
                parmDateReturnedToOffice.Value = GetDBDate( dateReturnedToOffice );
 

                cmdUpdateOfferAuditInformationQuery.Parameters.Add( parmOfferId );

                cmdUpdateOfferAuditInformationQuery.Parameters.Add( parmAuditIndicator );
                cmdUpdateOfferAuditInformationQuery.Parameters.Add( parmDateSentForPreaward );
                cmdUpdateOfferAuditInformationQuery.Parameters.Add( parmDateReturnedToOffice );

                // connect
                dbConnection.Open();

                cmdUpdateOfferAuditInformationQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.UpdateOfferGeneralAuditInformation(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateOfferPrimaryContact( int offerId, string vendorPrimaryContactName, string vendorPrimaryContactPhone, string vendorPrimaryContactExtension, string vendorPrimaryContactFax, string vendorPrimaryContactEmail )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateOfferPrimaryContact  // was UpdateContractContact
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@OfferId int,
                //@VendorPrimaryContactName nvarchar(30),
                //@VendorPrimaryContactPhone nvarchar(15),
                //@VendorPrimaryContactExtension nvarchar(5),
                //@VendorPrimaryContactFax nvarchar(15),
                //@VendorPrimaryContactEmail nvarchar(50)

                SqlCommand cmdUpdateOfferContactQuery = new SqlCommand( "UpdateOfferPrimaryContact", dbConnection );
                cmdUpdateOfferContactQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateOfferContactQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateOfferContactQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateOfferContactQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                SqlParameter parmVendorPrimaryContactName = new SqlParameter( "@VendorPrimaryContactName", SqlDbType.NVarChar, 30 );
                SqlParameter parmVendorPrimaryContactPhone = new SqlParameter( "@VendorPrimaryContactPhone", SqlDbType.NVarChar, 15 );
                SqlParameter parmVendorPrimaryContactExtension = new SqlParameter( "@VendorPrimaryContactExtension", SqlDbType.NVarChar, 5 );
                SqlParameter parmVendorPrimaryContactFax = new SqlParameter( "@VendorPrimaryContactFax", SqlDbType.NVarChar, 15 );
                SqlParameter parmVendorPrimaryContactEmail = new SqlParameter( "@VendorPrimaryContactEmail", SqlDbType.NVarChar, 50 );

                parmOfferId.Value = offerId;

                parmVendorPrimaryContactName.Value = vendorPrimaryContactName;
                parmVendorPrimaryContactPhone.Value = vendorPrimaryContactPhone;
                parmVendorPrimaryContactExtension.Value = vendorPrimaryContactExtension;
                parmVendorPrimaryContactFax.Value = vendorPrimaryContactFax;
                parmVendorPrimaryContactEmail.Value = vendorPrimaryContactEmail;
              
                cmdUpdateOfferContactQuery.Parameters.Add( parmOfferId );

                cmdUpdateOfferContactQuery.Parameters.Add( parmVendorPrimaryContactName );
                cmdUpdateOfferContactQuery.Parameters.Add( parmVendorPrimaryContactPhone );
                cmdUpdateOfferContactQuery.Parameters.Add( parmVendorPrimaryContactExtension );
                cmdUpdateOfferContactQuery.Parameters.Add( parmVendorPrimaryContactFax );
                cmdUpdateOfferContactQuery.Parameters.Add( parmVendorPrimaryContactEmail );
          
                // connect
                dbConnection.Open();

                cmdUpdateOfferContactQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.UpdateOfferPrimaryContact(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateOfferVendorBusinessAddress( int offerId, string vendorAddress1, string vendorAddress2,
                                        string vendorCity, string vendorState,
                                        string vendorZip, string vendorCountry, int vendorCountryId, string vendorWebAddress )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateOfferVendorBusinessAddress  
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@OfferId int,

                //@VendorAddress1 nvarchar(100),
                //@VendorAddress2 nvarchar(100),
                //@VendorCity nvarchar(20),
                //@VendorState nvarchar(2),
                //@VendorZip nvarchar(10),
                //@VendorCountry nvarchar(50),
                //@VendorCountryId int,
                //@VendorWebAddress nvarchar(50)

                SqlCommand cmdUpdateOfferVendorAddressQuery = new SqlCommand( "UpdateOfferVendorBusinessAddress", dbConnection );
                cmdUpdateOfferVendorAddressQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateOfferVendorAddressQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateOfferVendorAddressQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateOfferVendorAddressQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                SqlParameter parmVendorAddress1 = new SqlParameter( "@VendorAddress1", SqlDbType.NVarChar, 100 );
                SqlParameter parmVendorAddress2 = new SqlParameter( "@VendorAddress2", SqlDbType.NVarChar, 100 );
                SqlParameter parmVendorCity = new SqlParameter( "@VendorCity", SqlDbType.NVarChar, 20 );
                SqlParameter parmVendorState = new SqlParameter( "@VendorState", SqlDbType.NVarChar, 2 );
                SqlParameter parmVendorZip = new SqlParameter( "@VendorZip", SqlDbType.NVarChar, 10 );
                SqlParameter parmVendorCountry = new SqlParameter( "@VendorCountry", SqlDbType.NVarChar, 50 );
                SqlParameter parmVendorCountryId = new SqlParameter( "@VendorCountryId", SqlDbType.Int );
                SqlParameter parmVendorWebAddress = new SqlParameter( "@VendorWebAddress", SqlDbType.NVarChar, 50 );


                parmOfferId.Value = offerId;

                parmVendorAddress1.Value = vendorAddress1;
                parmVendorAddress2.Value = vendorAddress2;
                parmVendorCity.Value = vendorCity;
                parmVendorState.Value = vendorState;
                parmVendorZip.Value = vendorZip;
                parmVendorCountry.Value = vendorCountry;
                parmVendorCountryId.Value = vendorCountryId;
                parmVendorWebAddress.Value = vendorWebAddress;

                cmdUpdateOfferVendorAddressQuery.Parameters.Add( parmOfferId );
                cmdUpdateOfferVendorAddressQuery.Parameters.Add( parmVendorAddress1 );
                cmdUpdateOfferVendorAddressQuery.Parameters.Add( parmVendorAddress2 );
                cmdUpdateOfferVendorAddressQuery.Parameters.Add( parmVendorCity );
                cmdUpdateOfferVendorAddressQuery.Parameters.Add( parmVendorState );
                cmdUpdateOfferVendorAddressQuery.Parameters.Add( parmVendorZip );
                cmdUpdateOfferVendorAddressQuery.Parameters.Add( parmVendorCountry );
                cmdUpdateOfferVendorAddressQuery.Parameters.Add( parmVendorCountryId );
                cmdUpdateOfferVendorAddressQuery.Parameters.Add( parmVendorWebAddress );

                // connect
                dbConnection.Open();

                cmdUpdateOfferVendorAddressQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.UpdateOfferVendorBusinessAddress(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateOfferComment( int offerId, string offerComment )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateOfferComment  
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@OfferId int,
                //@OfferComment nvarchar(4000)


                SqlCommand cmdUpdateOfferCommentQuery = new SqlCommand( "UpdateOfferComment", dbConnection );
                cmdUpdateOfferCommentQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateOfferCommentQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateOfferCommentQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateOfferCommentQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );

                SqlParameter parmOfferComment = new SqlParameter( "@OfferComment", SqlDbType.NVarChar, 4000 );
               
                parmOfferId.Value = offerId;
                parmOfferComment.Value = offerComment;
              
                cmdUpdateOfferCommentQuery.Parameters.Add( parmOfferId );
                cmdUpdateOfferCommentQuery.Parameters.Add( parmOfferComment );
           
                // connect
                dbConnection.Open();

                cmdUpdateOfferCommentQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.UpdateOfferComment(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetNewOfferPrefix( int proposalTypeId, ref string prefix )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            prefix = "";

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //CREATE PROCEDURE GetOfferPrefix
                //(
                //@CurrentUser uniqueidentifier,
                //@ProposalTypeId int,          /* 1 = Offer Proposal; 2 = Contract Extension Proposal */
                //@OfferPrefix nvarchar(30) OUTPUT
                //)

                SqlCommand cmdGetOfferPrefixQuery = new SqlCommand( "GetOfferPrefix", dbConnection );
                cmdGetOfferPrefixQuery.CommandType = CommandType.StoredProcedure;
                cmdGetOfferPrefixQuery.CommandTimeout = 30;

                AddStandardParameter( cmdGetOfferPrefixQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                SqlParameter parmProposalTypeId = new SqlParameter( "@ProposalTypeId", SqlDbType.Int );
                SqlParameter parmOfferPrefix = new SqlParameter( "@OfferPrefix", SqlDbType.NVarChar, 30 );
                parmOfferPrefix.Direction = ParameterDirection.Output;

                parmProposalTypeId.Value = proposalTypeId;

                cmdGetOfferPrefixQuery.Parameters.Add( parmProposalTypeId );
                cmdGetOfferPrefixQuery.Parameters.Add( parmOfferPrefix );               

                // connect
                dbConnection.Open();

                cmdGetOfferPrefixQuery.ExecuteNonQuery();

                prefix = cmdGetOfferPrefixQuery.Parameters[ "@OfferPrefix" ].Value.ToString();              
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.GetNewOfferPrefix(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }
      
        public bool ValidateOfferNumber( string offerNumber, int scheduleNumber, int offerId, ref bool bIsValidated, ref string validationMessage )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //ValidateOfferNumber
                //(
                //@OfferNumber nvarchar(30),
                //@ScheduleNumber int,
                //@OfferId int,		/* -1 = creating new offer */
                //@IsValidated bit OUTPUT,
                //@ValidationMessage nvarchar(300) OUTPUT

                SqlCommand cmdValidateOfferNumberQuery = new SqlCommand( "ValidateOfferNumber", dbConnection );
                cmdValidateOfferNumberQuery.CommandType = CommandType.StoredProcedure;
                cmdValidateOfferNumberQuery.CommandTimeout = 30;


                //        SqlParameter parmUserId = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                SqlParameter parmOfferNumber = new SqlParameter( "@OfferNumber", SqlDbType.NVarChar, 30 );
                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );
                SqlParameter parmIsValidated = new SqlParameter( "@IsValidated", SqlDbType.Bit );
                parmIsValidated.Direction = ParameterDirection.Output;
                SqlParameter parmValidationMessage = new SqlParameter( "@ValidationMessage", SqlDbType.NVarChar, 300 );
                parmValidationMessage.Direction = ParameterDirection.Output;

                //         parmUserId.Value = _currentUserId;
                parmOfferNumber.Value = offerNumber;
                parmScheduleNumber.Value = scheduleNumber;
                parmOfferId.Value = offerId;

                //         cmdValidateOfferNumberQuery.Parameters.Add( parmUserId );
                cmdValidateOfferNumberQuery.Parameters.Add( parmOfferNumber );
                cmdValidateOfferNumberQuery.Parameters.Add( parmScheduleNumber );
                cmdValidateOfferNumberQuery.Parameters.Add( parmOfferId );
                cmdValidateOfferNumberQuery.Parameters.Add( parmIsValidated );
                cmdValidateOfferNumberQuery.Parameters.Add( parmValidationMessage );

                // connect
                dbConnection.Open();

                cmdValidateOfferNumberQuery.ExecuteNonQuery();

                bIsValidated = bool.Parse( cmdValidateOfferNumberQuery.Parameters[ "@IsValidated" ].Value.ToString() );
                validationMessage = cmdValidateOfferNumberQuery.Parameters[ "@ValidationMessage" ].Value.ToString();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.ValidateOfferNumber(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool CreateOffer( int solicitationId, string offerNumber, int scheduleNumber, int COID, int proposalTypeId, string vendorName, string extendsContractNumber, 
            DateTime dateReceived, DateTime dateAssigned, DateTime dateReassigned,
            int actionId, DateTime actionDate, DateTime expectedCompletionDate, DateTime expirationDate,
            bool bAuditIndicator, DateTime dateSentForPreaward, DateTime dateReturnedToOffice,
            string vendorPrimaryContactName, string vendorPrimaryContactPhone, string vendorPrimaryContactExtension, string vendorPrimaryContactFax, string vendorPrimaryContactEmail,
            string vendorAddress1, string vendorAddress2, string vendorCity, string vendorState, string vendorZip, string vendorCountry, int vendorCountryId, string vendorWebAddress,
            string offerComment, ref int offerId, ref string scheduleName )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //CreateOffer
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@OfferNumber nvarchar(30),
                //@ScheduleNumber int,
                //@COID int,
                //@ProposalTypeId int,
                //@ExtendsContractNumber nvarchar(20),
                //@VendorName nvarchar(75),
                //@SolicitationId int,
                //@DateReceived datetime,
                //@DateAssigned datetime,
                //@DateReassigned datetime,
                //@ActionId int,
                //@ActionDate datetime,
                //@ExpectedCompletionDate datetime = null,
                //@ExpirationDate datetime = null ,
                //@AuditIndicator bit,
                //@DateSentForPreaward datetime,
                //@DateReturnedToOffice datetime,
                //@VendorPrimaryContactName nvarchar(30),
                //@VendorPrimaryContactPhone nvarchar(15),
                //@VendorPrimaryContactExtension nvarchar(5),
                //@VendorPrimaryContactFax nvarchar(15),
                //@VendorPrimaryContactEmail nvarchar(50),
                //@VendorAddress1 nvarchar(100),
                //@VendorAddress2 nvarchar(100),
                //@VendorCity nvarchar(20),
                //@VendorState nvarchar(2),
                //@VendorZip nvarchar(10),
                //@VendorCountry nvarchar(50),
                //@VendorCountryId int,
                //@VendorWebAddress nvarchar(50),
                //@OfferComment nvarchar(4000),
                //@OfferId int OUTPUT,
                //@ScheduleName nvarchar(75) OUTPUT

                SqlCommand cmdCreateOfferQuery = new SqlCommand( "CreateOffer", dbConnection );
                cmdCreateOfferQuery.CommandType = CommandType.StoredProcedure;
                cmdCreateOfferQuery.CommandTimeout = 30;

                AddStandardParameter( cmdCreateOfferQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdCreateOfferQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmOfferNumber = new SqlParameter( "@OfferNumber", SqlDbType.NVarChar, 30 );
                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                SqlParameter parmProposalTypeId = new SqlParameter( "@ProposalTypeId", SqlDbType.Int );
                SqlParameter parmExtendsContractNumber = new SqlParameter( "@ExtendsContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmVendorName = new SqlParameter( "@VendorName", SqlDbType.NVarChar, 75 );
                SqlParameter parmSolicitationId = new SqlParameter( "@SolicitationId", SqlDbType.Int );

                SqlParameter parmDateReceived = new SqlParameter( "@DateReceived", SqlDbType.DateTime );
                SqlParameter parmDateAssigned = new SqlParameter( "@DateAssigned", SqlDbType.DateTime );
                SqlParameter parmDateReassigned = new SqlParameter( "@DateReassigned", SqlDbType.DateTime );

                SqlParameter parmActionId = new SqlParameter( "@ActionId", SqlDbType.Int );
                SqlParameter parmActionDate = new SqlParameter( "@ActionDate", SqlDbType.DateTime );
                SqlParameter parmExpectedCompletionDate = new SqlParameter( "@ExpectedCompletionDate", SqlDbType.DateTime );
                SqlParameter parmExpirationDate = new SqlParameter( "@ExpirationDate", SqlDbType.DateTime );

                SqlParameter parmAuditIndicator = new SqlParameter( "@AuditIndicator", SqlDbType.Bit );
                SqlParameter parmDateSentForPreaward = new SqlParameter( "@DateSentForPreaward", SqlDbType.DateTime );
                SqlParameter parmDateReturnedToOffice = new SqlParameter( "@DateReturnedToOffice", SqlDbType.DateTime );

                SqlParameter parmVendorPrimaryContactName = new SqlParameter( "@VendorPrimaryContactName", SqlDbType.NVarChar, 30 );
                SqlParameter parmVendorPrimaryContactPhone = new SqlParameter( "@VendorPrimaryContactPhone", SqlDbType.NVarChar, 15 );
                SqlParameter parmVendorPrimaryContactExtension = new SqlParameter( "@VendorPrimaryContactExtension", SqlDbType.NVarChar, 5 );
                SqlParameter parmVendorPrimaryContactFax = new SqlParameter( "@VendorPrimaryContactFax", SqlDbType.NVarChar, 15 );
                SqlParameter parmVendorPrimaryContactEmail = new SqlParameter( "@VendorPrimaryContactEmail", SqlDbType.NVarChar, 50 );

                SqlParameter parmVendorAddress1 = new SqlParameter( "@VendorAddress1", SqlDbType.NVarChar, 100 );
                SqlParameter parmVendorAddress2 = new SqlParameter( "@VendorAddress2", SqlDbType.NVarChar, 100 );
                SqlParameter parmVendorCity = new SqlParameter( "@VendorCity", SqlDbType.NVarChar, 20 );
                SqlParameter parmVendorState = new SqlParameter( "@VendorState", SqlDbType.NVarChar, 2 );
                SqlParameter parmVendorZip = new SqlParameter( "@VendorZip", SqlDbType.NVarChar, 10 );
                SqlParameter parmVendorCountry = new SqlParameter( "@VendorCountry", SqlDbType.NVarChar, 50 );
                SqlParameter parmVendorCountryId = new SqlParameter( "@VendorCountryId", SqlDbType.Int );
                SqlParameter parmVendorWebAddress = new SqlParameter( "@VendorWebAddress", SqlDbType.NVarChar, 50 );

                SqlParameter parmOfferComment = new SqlParameter( "@OfferComment", SqlDbType.NVarChar, 4000 );

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );
                parmOfferId.Direction = ParameterDirection.Output;

                SqlParameter parmScheduleName = new SqlParameter( "@ScheduleName", SqlDbType.NVarChar, 75 );
                parmScheduleName.Direction = ParameterDirection.Output;

                parmOfferNumber.Value = offerNumber;
                parmScheduleNumber.Value = scheduleNumber;
                parmCOID.Value = COID;
                parmProposalTypeId.Value = proposalTypeId;
                parmExtendsContractNumber.Value = extendsContractNumber;
                parmVendorName.Value = vendorName;
                parmSolicitationId.Value = solicitationId;

                parmDateReceived.Value = GetDBDate( dateReceived );
                parmDateAssigned.Value = GetDBDate( dateAssigned );
                parmDateReassigned.Value = GetDBDate( dateReassigned );

                parmActionId.Value = actionId;

                parmActionDate.Value = GetDBDate( actionDate );
                parmExpectedCompletionDate.Value = GetDBDate( expectedCompletionDate );
                parmExpirationDate.Value = GetDBDate( expirationDate );
               
                parmAuditIndicator.Value = bAuditIndicator;

                parmDateSentForPreaward.Value = GetDBDate( dateSentForPreaward );
                parmDateReturnedToOffice.Value = GetDBDate( dateReturnedToOffice );

                parmVendorPrimaryContactName.Value = vendorPrimaryContactName;
                parmVendorPrimaryContactPhone.Value = vendorPrimaryContactPhone;
                parmVendorPrimaryContactExtension.Value = vendorPrimaryContactExtension;
                parmVendorPrimaryContactFax.Value = vendorPrimaryContactFax;
                parmVendorPrimaryContactEmail.Value = vendorPrimaryContactEmail;

                parmVendorAddress1.Value = vendorAddress1;
                parmVendorAddress2.Value = vendorAddress2;
                parmVendorCity.Value = vendorCity;
                parmVendorState.Value = vendorState;
                parmVendorZip.Value = vendorZip;
                parmVendorCountry.Value = vendorCountry;
                parmVendorCountryId.Value = vendorCountryId;
                parmVendorWebAddress.Value = vendorWebAddress;

                parmOfferComment.Value = offerComment;

                cmdCreateOfferQuery.Parameters.Add( parmOfferNumber );
                cmdCreateOfferQuery.Parameters.Add( parmScheduleNumber );
                cmdCreateOfferQuery.Parameters.Add( parmCOID );
                cmdCreateOfferQuery.Parameters.Add( parmProposalTypeId );
                cmdCreateOfferQuery.Parameters.Add( parmExtendsContractNumber );
                cmdCreateOfferQuery.Parameters.Add( parmVendorName );
                cmdCreateOfferQuery.Parameters.Add( parmSolicitationId );
                cmdCreateOfferQuery.Parameters.Add( parmDateReceived );
                cmdCreateOfferQuery.Parameters.Add( parmDateAssigned );
                cmdCreateOfferQuery.Parameters.Add( parmDateReassigned );
                cmdCreateOfferQuery.Parameters.Add( parmActionId );
                cmdCreateOfferQuery.Parameters.Add( parmActionDate );
                cmdCreateOfferQuery.Parameters.Add( parmExpectedCompletionDate );
                cmdCreateOfferQuery.Parameters.Add( parmExpirationDate );
                cmdCreateOfferQuery.Parameters.Add( parmAuditIndicator );
                cmdCreateOfferQuery.Parameters.Add( parmDateSentForPreaward );
                cmdCreateOfferQuery.Parameters.Add( parmDateReturnedToOffice );

                cmdCreateOfferQuery.Parameters.Add( parmVendorPrimaryContactName );
                cmdCreateOfferQuery.Parameters.Add( parmVendorPrimaryContactPhone );
                cmdCreateOfferQuery.Parameters.Add( parmVendorPrimaryContactExtension );
                cmdCreateOfferQuery.Parameters.Add( parmVendorPrimaryContactFax );
                cmdCreateOfferQuery.Parameters.Add( parmVendorPrimaryContactEmail );

                cmdCreateOfferQuery.Parameters.Add( parmVendorAddress1 );
                cmdCreateOfferQuery.Parameters.Add( parmVendorAddress2 );
                cmdCreateOfferQuery.Parameters.Add( parmVendorCity );
                cmdCreateOfferQuery.Parameters.Add( parmVendorState );
                cmdCreateOfferQuery.Parameters.Add( parmVendorZip );
                cmdCreateOfferQuery.Parameters.Add( parmVendorCountry );
                cmdCreateOfferQuery.Parameters.Add( parmVendorCountryId );
                cmdCreateOfferQuery.Parameters.Add( parmVendorWebAddress );
                cmdCreateOfferQuery.Parameters.Add( parmOfferComment );

                cmdCreateOfferQuery.Parameters.Add( parmOfferId );
                cmdCreateOfferQuery.Parameters.Add( parmScheduleName );

                // connect
                dbConnection.Open();

                cmdCreateOfferQuery.ExecuteNonQuery();

                offerId = int.Parse( cmdCreateOfferQuery.Parameters[ "@OfferId" ].Value.ToString() );
                scheduleName = cmdCreateOfferQuery.Parameters[ "@ScheduleName" ].Value.ToString();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.CreateOffer(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string OfferActionTypesTableName = "OfferActionTypesTable";

        public bool SelectOfferActionTypes( ref DataSet dsOfferActionTypes )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOfferActionTypes = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectOfferActionTypes
                //(
                //@CurrentUser uniqueidentifier
                //)
                SqlCommand cmdSelectOfferActionTypes = new SqlCommand( "SelectOfferActionTypes", dbConnection );
                cmdSelectOfferActionTypes.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferActionTypes.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );

                parmCurrentUser.Value = _currentUserId;

                cmdSelectOfferActionTypes.Parameters.Add( parmCurrentUser );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferActionTypes = new SqlDataAdapter();
                daOfferActionTypes.SelectCommand = cmdSelectOfferActionTypes;

                dsOfferActionTypes = new DataSet( "OfferActionTypes" );
                DataTable dtOfferActionTypes = dsOfferActionTypes.Tables.Add( "OfferActionTypesTable" );

                // add the common elements to the table
                DataColumn actionIdColumn = new DataColumn( "ActionId", typeof( int ) );

                dtOfferActionTypes.Columns.Add( actionIdColumn );
                dtOfferActionTypes.Columns.Add( "ActionDescription", typeof( string ) );
                dtOfferActionTypes.Columns.Add( "IsOfferComplete", typeof( bool ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = actionIdColumn;

                // add the keys to the table
                dtOfferActionTypes.PrimaryKey = primaryKeyColumns;

                dtOfferActionTypes.Clear();

                // connect
                dbConnection.Open();

                // run
                daOfferActionTypes.Fill( dsOfferActionTypes, "OfferActionTypesTable" );

                RowsReturned = dsOfferActionTypes.Tables[ "OfferActionTypesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectOfferActionTypes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public static string SolicitationsTableName = "SolicitationsTable";

        // A = active, B = both active and inactive
        public bool SelectSolicitations( ref DataSet dsSolicitations, string activeStatus )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSolicitations = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectSolicitations
                //(
                //@CurrentUser uniqueidentifier,
                //@Active nchar(1)             --  'A' = Active Only  'B' = Both Active and Inactive
                //SELECT Solicitation_ID, Solicitation_Number 

                SqlCommand cmdSelectSolicitationsQuery = new SqlCommand( "SelectSolicitations", dbConnection );
                cmdSelectSolicitationsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectSolicitationsQuery.CommandTimeout = 30;

                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmActive = new SqlParameter( "@Active", SqlDbType.NChar, 1 );

                parmCurrentUser.Value = _currentUserId;
                parmActive.Value = activeStatus;

                cmdSelectSolicitationsQuery.Parameters.Add( parmCurrentUser );
                cmdSelectSolicitationsQuery.Parameters.Add( parmActive );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSolicitations = new SqlDataAdapter();
                daSolicitations.SelectCommand = cmdSelectSolicitationsQuery;

                dsSolicitations = new DataSet( "Solicitations" );
                DataTable dtSolicitations = dsSolicitations.Tables.Add( SolicitationsTableName );

                DataColumn solicitationIdColumn = new DataColumn( "Solicitation_ID", typeof( int ) );

                dtSolicitations.Columns.Add( solicitationIdColumn );

                dtSolicitations.Columns.Add( "Solicitation_Number", typeof( string ) );
               
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = solicitationIdColumn;

                // add the keys to the table
                dtSolicitations.PrimaryKey = primaryKeyColumns;

                dtSolicitations.Clear();

                // connect
                dbConnection.Open();

                // run
                daSolicitations.Fill( dsSolicitations, SolicitationsTableName );

                RowsReturned = dsSolicitations.Tables[ SolicitationsTableName ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in OfferDB.SelectSolicitations(): {0}", ex.Message );
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

            info.AddValue( "CurrentUserId", _currentUserId );
            info.AddValue( "UserName", _userName );
            info.AddValue( "OldUserId", _oldUserId );
        }

        protected OfferDB( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            _currentUserId = ( Guid )info.GetValue( "CurrentUserId", typeof( Guid ) );
            _userName = info.GetString( "UserName" );
            _oldUserId = info.GetInt32( "OldUserId" );
            RestoreDelegatesAfterDeserialization( this, "OfferDB" );
        }
        #endregion


    }
}
