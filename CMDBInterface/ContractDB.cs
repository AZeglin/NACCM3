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
    public class ContractDB : DBCommon, ISerializable
    {
        private Guid _currentUserId = Guid.Empty;
        private string _userName = string.Empty;
        private int _oldUserId = -1;

        public ContractDB()
            : base()
        {
        }

        public ContractDB( Guid currentUserId, string userName, int oldUserId )
            : base()
        {
            _currentUserId = currentUserId;
            _userName = userName;
            _oldUserId = oldUserId;
        }

        public bool SelectContractHeaders( ref DataSet dsContractHeaders, string contractStatusFilter, string contractOwnerFilter, string filterType, string filterValue, string sortExpression, string sortDirection )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractHeaders = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractHeaders2
                //(
                //@CurrentUser uniqueidentifier,
                //@COID int,
                //@LoginId nvarchar(120),
                //@ContractStatusFilter nchar(1),  /* A - All, T - Active, C - Closed, new for release 2 N - none */
                //@ContractOwnerFilter nchar(1), /* A - All, M - Mine */
                //@FilterType nchar(1), /* N - Number, O - CO Name, V - Vendor, D - Description, S - Schedule */
                //@FilterValue nvarchar(200),
                //@SortExpression nvarchar(100),
                //@SortDirection nvarchar(20)
                //)
                SqlCommand cmdSelectContractHeadersQuery = new SqlCommand( "SelectContractHeaders2", dbConnection );
                cmdSelectContractHeadersQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractHeadersQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                SqlParameter parmLoginId = new SqlParameter( "@LoginId", SqlDbType.NVarChar, 120 );
                SqlParameter parmContractStatusFilter = new SqlParameter( "@ContractStatusFilter", SqlDbType.NChar, 1 );
                SqlParameter parmContractOwnerFilter = new SqlParameter( "@ContractOwnerFilter", SqlDbType.NChar, 1 );
                SqlParameter parmFilterType = new SqlParameter( "@FilterType", SqlDbType.NChar, 1 );
                SqlParameter parmFilterValue = new SqlParameter( "@FilterValue", SqlDbType.NVarChar, 200 );
                SqlParameter parmSortExpression = new SqlParameter( "@SortExpression", SqlDbType.NVarChar, 100 );
                SqlParameter parmSortDirection = new SqlParameter( "@SortDirection", SqlDbType.NVarChar, 20 );

                parmCurrentUser.Value = _currentUserId;
                parmCOID.Value = _oldUserId;
                parmLoginId.Value = _userName;
                parmContractStatusFilter.Value = contractStatusFilter;
                parmContractOwnerFilter.Value = contractOwnerFilter;
                parmFilterType.Value = filterType;
                string cleansedFilterValue = filterValue.Replace( "'", "''" );
                parmFilterValue.Value = cleansedFilterValue;
                parmSortExpression.Value = sortExpression;
                parmSortDirection.Value = sortDirection;

                cmdSelectContractHeadersQuery.Parameters.Add( parmCurrentUser );
                cmdSelectContractHeadersQuery.Parameters.Add( parmCOID );
                cmdSelectContractHeadersQuery.Parameters.Add( parmLoginId );
                cmdSelectContractHeadersQuery.Parameters.Add( parmContractStatusFilter );
                cmdSelectContractHeadersQuery.Parameters.Add( parmContractOwnerFilter );
                cmdSelectContractHeadersQuery.Parameters.Add( parmFilterType );
                cmdSelectContractHeadersQuery.Parameters.Add( parmFilterValue );
                cmdSelectContractHeadersQuery.Parameters.Add( parmSortExpression );
                cmdSelectContractHeadersQuery.Parameters.Add( parmSortDirection );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractHeaders = new SqlDataAdapter();
                daContractHeaders.SelectCommand = cmdSelectContractHeadersQuery;

                dsContractHeaders = new DataSet( "ContractHeaders" );
                DataTable dtContractHeaders = dsContractHeaders.Tables.Add( "ContractHeadersTable" );

                // add the common elements to the table
                DataColumn contractIdColumn = new DataColumn( "Contract_Record_ID", typeof( int ) );

                dtContractHeaders.Columns.Add( contractIdColumn );
                
                dtContractHeaders.Columns.Add( "CntrctNum", typeof( string ) );
                dtContractHeaders.Columns.Add( "Schedule_Name", typeof( string ) );
                dtContractHeaders.Columns.Add( "Schedule_Number", typeof( int ) );

                dtContractHeaders.Columns.Add( "CO_Name", typeof( string ) );
                dtContractHeaders.Columns.Add( "CO_ID", typeof( int ) );
                dtContractHeaders.Columns.Add( "Contractor_Name", typeof( string ) );
                dtContractHeaders.Columns.Add( "SAMUEI", typeof( string ) );
                dtContractHeaders.Columns.Add( "DUNS", typeof( string ) );
                dtContractHeaders.Columns.Add( "Drug_Covered", typeof( string ) );

                dtContractHeaders.Columns.Add( "Dates_CntrctAward", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "Dates_Effective", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "Dates_CntrctExp", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "Dates_Completion", typeof( DateTime ) );

                dtContractHeaders.Columns.Add( "HasBPA", typeof( string ) );
                dtContractHeaders.Columns.Add( "BPA_FSS_Counterpart", typeof( string ) );
                dtContractHeaders.Columns.Add( "Offer_ID", typeof( int ) );
     
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractIdColumn;

                // add the keys to the table
                dtContractHeaders.PrimaryKey = primaryKeyColumns;

                dtContractHeaders.Clear();

                // connect
                dbConnection.Open();

                // run
                daContractHeaders.Fill( dsContractHeaders, "ContractHeadersTable" );

                RowsReturned = dsContractHeaders.Tables[ "ContractHeadersTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectContractHeaders(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public static string PersonalizedNotificationTableName = "PersonalizedNotificationTable";

        public bool SelectPersonalizedNotification( ref DataSet dsPersonalizedNotificationDataSet, bool bIncludeSubordinates )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daPersonalizedNotification = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectPersonalizedNotification
                //(
                //@CurrentUser uniqueidentifier, 
                //@IncludeSubordinates bit
                //)
                SqlCommand cmdSelectPersonalizedNotificationQuery = new SqlCommand( "SelectPersonalizedNotification", dbConnection );
                cmdSelectPersonalizedNotificationQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectPersonalizedNotificationQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmIncludeSubordinates = new SqlParameter( "@IncludeSubordinates", SqlDbType.Bit );
              
                parmCurrentUser.Value = _currentUserId;
                parmIncludeSubordinates.Value = bIncludeSubordinates;            

                cmdSelectPersonalizedNotificationQuery.Parameters.Add( parmCurrentUser );
                cmdSelectPersonalizedNotificationQuery.Parameters.Add( parmIncludeSubordinates );
                

                // create a data adapter and dataset to 
                // run the query and hold the results
                daPersonalizedNotification = new SqlDataAdapter();
                daPersonalizedNotification.SelectCommand = cmdSelectPersonalizedNotificationQuery;

                dsPersonalizedNotificationDataSet = new DataSet( "PersonalizedNotificationDataSet" );
                DataTable dtPersonalizedNotification = dsPersonalizedNotificationDataSet.Tables.Add( "PersonalizedNotificationTableName" );

                // add the common elements to the table
                DataColumn personalizedNotificationIdColumn = new DataColumn( "PersonalizedNotificationId", typeof( int ) );

                dtPersonalizedNotification.Columns.Add( personalizedNotificationIdColumn );
                dtPersonalizedNotification.Columns.Add( "CO_ID", typeof( int ) );
                dtPersonalizedNotification.Columns.Add( "LastName", typeof( string ) );
                dtPersonalizedNotification.Columns.Add( "FullName", typeof( string ) );

                dtPersonalizedNotification.Columns.Add( "ContractNumber", typeof( string ) );
                dtPersonalizedNotification.Columns.Add( "Contract_Record_ID", typeof( int ) );
                dtPersonalizedNotification.Columns.Add( "Schedule_Number", typeof( int ) );

                dtPersonalizedNotification.Columns.Add( "BPAContractNumber", typeof( string ) );
                dtPersonalizedNotification.Columns.Add( "VendorName", typeof( string ) );


                dtPersonalizedNotification.Columns.Add( "EffectiveDate", typeof( DateTime ) );
                dtPersonalizedNotification.Columns.Add( "ExpirationDate", typeof( DateTime ) );
                dtPersonalizedNotification.Columns.Add( "CompletionDate", typeof( DateTime ) );

                dtPersonalizedNotification.Columns.Add( "NotificationRank", typeof( int ) );
                dtPersonalizedNotification.Columns.Add( "Countdown", typeof( int ) );
                dtPersonalizedNotification.Columns.Add( "NotificationMessage", typeof( string ) );
                

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = personalizedNotificationIdColumn;

                // add the keys to the table
                dtPersonalizedNotification.PrimaryKey = primaryKeyColumns;

                dtPersonalizedNotification.Clear();

                // connect
                dbConnection.Open();

                // run
                daPersonalizedNotification.Fill( dsPersonalizedNotificationDataSet, "PersonalizedNotificationTableName" );

                RowsReturned = dsPersonalizedNotificationDataSet.Tables[ "PersonalizedNotificationTableName" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectPersonalizedNotification(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool SelectUserRecentDocuments( ref DataSet dsUserRecentDocuments, int count )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daUserRecentDocuments = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                // SelectUserRecentDocuments
                //(
                //@CurrentUser uniqueidentifier,
                //@MostRecentCount int

                SqlCommand cmdSelectUserRecentDocumentsQuery = new SqlCommand( "SelectUserRecentDocuments", dbConnection );
                cmdSelectUserRecentDocumentsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectUserRecentDocumentsQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmMostRecentCount = new SqlParameter( "@MostRecentCount", SqlDbType.Int );
                

                parmCurrentUser.Value = _currentUserId;
                parmMostRecentCount.Value = count;
               
                cmdSelectUserRecentDocumentsQuery.Parameters.Add( parmCurrentUser );
                cmdSelectUserRecentDocumentsQuery.Parameters.Add( parmMostRecentCount );
                
                // create a data adapter and dataset to 
                // run the query and hold the results
                daUserRecentDocuments = new SqlDataAdapter();
                daUserRecentDocuments.SelectCommand = cmdSelectUserRecentDocumentsQuery;

                dsUserRecentDocuments = new DataSet( "UserRecentDocuments" );
                DataTable dtUserRecentDocuments = dsUserRecentDocuments.Tables.Add( "UserRecentDocumentsTable" );

                // add the common elements to the table
                DataColumn userPreferenceIdColumn = new DataColumn( "UserPreferenceId", typeof( int ) );

                dtUserRecentDocuments.Columns.Add( userPreferenceIdColumn );

                dtUserRecentDocuments.Columns.Add( "DocumentType", typeof( string ) );
                dtUserRecentDocuments.Columns.Add( "DocumentNumber", typeof( string ) );
                dtUserRecentDocuments.Columns.Add( "DocumentId", typeof( int ) );
                dtUserRecentDocuments.Columns.Add( "LastModificationDate", typeof( DateTime ) );
                dtUserRecentDocuments.Columns.Add( "Schedule_Number", typeof( int ) );
                dtUserRecentDocuments.Columns.Add( "Schedule_Name", typeof( string ) );
                dtUserRecentDocuments.Columns.Add( "CO_Name", typeof( string ) );
                // CO_ID, Contractor_Name, DocumentDate, ActiveStatus

                dtUserRecentDocuments.Columns.Add( "CO_ID", typeof( int ) );
                dtUserRecentDocuments.Columns.Add( "Contractor_Name", typeof( string ) );

                dtUserRecentDocuments.Columns.Add( "DocumentDate", typeof( string ) ); // label + date
                dtUserRecentDocuments.Columns.Add( "ActiveStatus", typeof( bool ) );
                dtUserRecentDocuments.Columns.Add( "CompletionStatus", typeof( bool ) );  // for contracts, completion means cancellation
              

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = userPreferenceIdColumn;

                // add the keys to the table
                dtUserRecentDocuments.PrimaryKey = primaryKeyColumns;

                dtUserRecentDocuments.Clear();

                // connect
                dbConnection.Open();

                // run
                daUserRecentDocuments.Fill( dsUserRecentDocuments, "UserRecentDocumentsTable" );

                RowsReturned = dsUserRecentDocuments.Tables[ "UserRecentDocumentsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectUserRecentDocuments(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // item counts moved it ItemDB object
        // 

        public bool GetParentContractInfo( string bpaContractNumber, ref string parentContractNumber, ref int parentScheduleNumber, ref int parentOwnerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetParentContractInfo
                //(
                //@CurrentUser uniqueidentifier,
                //@BPAContractNumber nvarchar(50),
                //@ParentContractNumber nvarchar(20) OUTPUT,
                //@ParentScheduleNumber int OUTPUT,
                //@ParentOwnerId int OUTPUT

                SqlCommand cmdGetParentContractInfoQuery = new SqlCommand( "GetParentContractInfo", dbConnection );
                cmdGetParentContractInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetParentContractInfoQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmBPAContractNumber = new SqlParameter( "@BPAContractNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmParentContractNumber = new SqlParameter( "@ParentContractNumber", SqlDbType.NVarChar, 20 );
                parmParentContractNumber.Direction = ParameterDirection.Output;
                SqlParameter parmParentScheduleNumber = new SqlParameter( "@ParentScheduleNumber", SqlDbType.Int );
                parmParentScheduleNumber.Direction = ParameterDirection.Output;
                SqlParameter parmParentOwnerId = new SqlParameter( "@ParentOwnerId", SqlDbType.Int );
                parmParentOwnerId.Direction = ParameterDirection.Output;

                parmCurrentUser.Value = _currentUserId;
                parmBPAContractNumber.Value = bpaContractNumber;

                cmdGetParentContractInfoQuery.Parameters.Add( parmCurrentUser );
                cmdGetParentContractInfoQuery.Parameters.Add( parmBPAContractNumber );
                cmdGetParentContractInfoQuery.Parameters.Add( parmParentContractNumber );
                cmdGetParentContractInfoQuery.Parameters.Add( parmParentScheduleNumber );
                cmdGetParentContractInfoQuery.Parameters.Add( parmParentOwnerId );

                // connect
                dbConnection.Open();

                cmdGetParentContractInfoQuery.ExecuteNonQuery();

                parentContractNumber = cmdGetParentContractInfoQuery.Parameters[ "@ParentContractNumber" ].Value.ToString();
                parentScheduleNumber = int.Parse( cmdGetParentContractInfoQuery.Parameters[ "@ParentScheduleNumber" ].Value.ToString() );
                parentOwnerId = int.Parse( cmdGetParentContractInfoQuery.Parameters[ "@ParentOwnerId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetParentContractInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetParentContractInfo2( string bpaContractNumber, ref int parentContractId, ref string parentContractNumber, ref int parentScheduleNumber, ref int parentOwnerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetParentContractInfo2
                //(
                //@CurrentUser uniqueidentifier,
                //@BPAContractNumber nvarchar(50),
                //@ParentContractId int OUTPUT,
                //@ParentContractNumber nvarchar(20) OUTPUT,
                //@ParentScheduleNumber int OUTPUT,
                //@ParentOwnerId int OUTPUT

                SqlCommand cmdGetParentContractInfoQuery = new SqlCommand( "GetParentContractInfo2", dbConnection );
                cmdGetParentContractInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetParentContractInfoQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmBPAContractNumber = new SqlParameter( "@BPAContractNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmParentContractId = new SqlParameter( "@ParentContractId", SqlDbType.Int );
                parmParentContractId.Direction = ParameterDirection.Output;
                SqlParameter parmParentContractNumber = new SqlParameter( "@ParentContractNumber", SqlDbType.NVarChar, 20 );
                parmParentContractNumber.Direction = ParameterDirection.Output;
                SqlParameter parmParentScheduleNumber = new SqlParameter( "@ParentScheduleNumber", SqlDbType.Int );
                parmParentScheduleNumber.Direction = ParameterDirection.Output;
                SqlParameter parmParentOwnerId = new SqlParameter( "@ParentOwnerId", SqlDbType.Int );
                parmParentOwnerId.Direction = ParameterDirection.Output;

                parmCurrentUser.Value = _currentUserId;
                parmBPAContractNumber.Value = bpaContractNumber;

                cmdGetParentContractInfoQuery.Parameters.Add( parmCurrentUser );
                cmdGetParentContractInfoQuery.Parameters.Add( parmBPAContractNumber );
                cmdGetParentContractInfoQuery.Parameters.Add( parmParentContractId );
                cmdGetParentContractInfoQuery.Parameters.Add( parmParentContractNumber );
                cmdGetParentContractInfoQuery.Parameters.Add( parmParentScheduleNumber );
                cmdGetParentContractInfoQuery.Parameters.Add( parmParentOwnerId );

                // connect
                dbConnection.Open();

                cmdGetParentContractInfoQuery.ExecuteNonQuery();

                parentContractId = int.Parse( cmdGetParentContractInfoQuery.Parameters[ "@ParentContractId" ].Value.ToString() );
                parentContractNumber = cmdGetParentContractInfoQuery.Parameters[ "@ParentContractNumber" ].Value.ToString();
                parentScheduleNumber = int.Parse( cmdGetParentContractInfoQuery.Parameters[ "@ParentScheduleNumber" ].Value.ToString() );
                parentOwnerId = int.Parse( cmdGetParentContractInfoQuery.Parameters[ "@ParentOwnerId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetParentContractInfo2(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetContractInfoDetails( string contractNumber, ref DataSet dsContractDetails )
        {
            bool bSuccess = true;
            dsContractDetails = null;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractDetails = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetContractInfoDetails
                //(
                //@CurrentUser uniqueidentifier,
                //@UserLogin nvarchar(120),
                //@ContractNumber nvarchar(20)
                //)

                SqlCommand cmdSelectContractDetailsQuery = new SqlCommand( "GetContractInfoDetails", dbConnection );
                cmdSelectContractDetailsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractDetailsQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectContractDetailsQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdSelectContractDetailsQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmLoginId = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                parmLoginId.Value = _userName;
                parmContractNumber.Value = contractNumber;

                cmdSelectContractDetailsQuery.Parameters.Add( parmLoginId );
                cmdSelectContractDetailsQuery.Parameters.Add( parmContractNumber );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractDetails = new SqlDataAdapter();
                daContractDetails.SelectCommand = cmdSelectContractDetailsQuery;

                dsContractDetails = new DataSet( "ContractDetails" );
                DataTable dtContractDetails = dsContractDetails.Tables.Add( "ContractDetailsTable" );

                DataColumn contractRecordIdColumn = new DataColumn( "Contract_Record_ID", typeof( int ) );
                dtContractDetails.Columns.Add( contractRecordIdColumn );

                dtContractDetails.Columns.Add( "SAMUEI", typeof( string ) );
                dtContractDetails.Columns.Add( "DUNS", typeof( string ) );   
		        dtContractDetails.Columns.Add( "TIN", typeof( string ) );           
		        dtContractDetails.Columns.Add( "PV_Participation", typeof( bool ) );                         	           
		        dtContractDetails.Columns.Add( "Solicitation_Number", typeof( string ) );                      	  
		        dtContractDetails.Columns.Add( "Primary_Address_1", typeof( string ) );                        	 
		        dtContractDetails.Columns.Add( "Primary_Address_2", typeof( string ) );                        	 
		        dtContractDetails.Columns.Add( "Primary_City", typeof( string ) );                             	  
		        dtContractDetails.Columns.Add( "Primary_State", typeof( string ) );                            	   
		        dtContractDetails.Columns.Add( "Primary_Zip", typeof( string ) );
                dtContractDetails.Columns.Add( "Primary_CountryId", typeof( int ) );
                dtContractDetails.Columns.Add( "CountryName", typeof( string ) );
		        dtContractDetails.Columns.Add( "POC_Primary_Name", typeof( string ) );                         	  
		        dtContractDetails.Columns.Add( "POC_Primary_Phone", typeof( string ) );                        	  
		        dtContractDetails.Columns.Add( "POC_Primary_Ext", typeof( string ) );                          	   
		        dtContractDetails.Columns.Add( "POC_Primary_Fax", typeof( string ) );                          	  
		        dtContractDetails.Columns.Add( "POC_Primary_Email", typeof( string ) );                        	  
		        dtContractDetails.Columns.Add( "POC_VendorWeb", typeof( string ) );                            	  
		        dtContractDetails.Columns.Add( "POC_Notes", typeof( string ) );                                	 
		        dtContractDetails.Columns.Add( "POC_Alternate_Name", typeof( string ) );                       	  
		        dtContractDetails.Columns.Add( "POC_Alternate_Phone", typeof( string ) );                      	  
		        dtContractDetails.Columns.Add( "POC_Alternate_Ext", typeof( string ) );                        	   
		        dtContractDetails.Columns.Add( "POC_Alternate_Fax", typeof( string ) );                        	  
		        dtContractDetails.Columns.Add( "POC_Alternate_Email", typeof( string ) );                      	  
		        dtContractDetails.Columns.Add( "POC_Emergency_Name", typeof( string ) );                       	  
		        dtContractDetails.Columns.Add( "POC_Emergency_Phone", typeof( string ) );                      	  
		        dtContractDetails.Columns.Add( "POC_Emergency_Ext", typeof( string ) );                        	   
		        dtContractDetails.Columns.Add( "POC_Emergency_Fax", typeof( string ) );                        	  
		        dtContractDetails.Columns.Add( "POC_Emergency_Email", typeof( string ) );                      	  
		        dtContractDetails.Columns.Add( "POC_Tech_Name", typeof( string ) );                            	  
		        dtContractDetails.Columns.Add( "POC_Tech_Phone", typeof( string ) );                           	  
		        dtContractDetails.Columns.Add( "POC_Tech_Ext", typeof( string ) );                             	   
		        dtContractDetails.Columns.Add( "POC_Tech_Fax", typeof( string ) );                             	  
		        dtContractDetails.Columns.Add( "POC_Tech_Email", typeof( string ) );
                dtContractDetails.Columns.Add( "Socio_VetStatus_ID", typeof( int ) );
                dtContractDetails.Columns.Add( "Socio_Business_Size_ID", typeof( int ) );                   	  
		        dtContractDetails.Columns.Add( "Socio_SDB", typeof( string ) );                                	           
		        dtContractDetails.Columns.Add( "Socio_8a", typeof( string ) );                                 	           
		        dtContractDetails.Columns.Add( "Socio_Woman", typeof( string ) );                              	           
		        dtContractDetails.Columns.Add( "Socio_HubZone", typeof( string ) );                            	           
		        dtContractDetails.Columns.Add( "Discount_Basic", typeof( string ) );                           	 
		        dtContractDetails.Columns.Add( "Discount_Credit_Card", typeof( string ) );                     	 
		        dtContractDetails.Columns.Add( "Discount_Prompt_Pay", typeof( string ) );                      	 
		        dtContractDetails.Columns.Add( "Discount_Quantity", typeof( string ) );
                dtContractDetails.Columns.Add( "Geographic_Coverage_ID", typeof( int ) );                   	           
		        dtContractDetails.Columns.Add( "Tracking_Customer", typeof( string ) );                        	 
		        dtContractDetails.Columns.Add( "Mininum_Order", typeof( string ) );                            	 
		        dtContractDetails.Columns.Add( "Delivery_Terms", typeof( string ) );                           	 
		        dtContractDetails.Columns.Add( "Expedited_Delivery_Terms", typeof( string ) );                 	 
		        dtContractDetails.Columns.Add( "Annual_Rebate", typeof( string ) );                            	 
		        dtContractDetails.Columns.Add( "BF_Offer", typeof( string ) );
                dtContractDetails.Columns.Add( "Credit_Card_Accepted", typeof( bool ) );
                dtContractDetails.Columns.Add( "Hazard", typeof( bool ) );                                   	           
		        dtContractDetails.Columns.Add( "Warranty_Duration", typeof( string ) );                        	  
		        dtContractDetails.Columns.Add( "Warranty_Notes", typeof( string ) );
                dtContractDetails.Columns.Add( "IFF_Type_ID", typeof( int ) );                              	           
		        dtContractDetails.Columns.Add( "Ratio", typeof( string ) );
                dtContractDetails.Columns.Add( "Returned_Goods_Policy_Type", typeof( int ) );               	           
		        dtContractDetails.Columns.Add( "Returned_Goods_Policy_Notes", typeof( string ) );              	
		        dtContractDetails.Columns.Add( "Incentive_Description", typeof( string ) );
                dtContractDetails.Columns.Add( "Dist_Manf_ID", typeof( int ) );                             	           
		        dtContractDetails.Columns.Add( "Ord_Address_1", typeof( string ) );                            	 
		        dtContractDetails.Columns.Add( "Ord_Address_2", typeof( string ) );                            	 
		        dtContractDetails.Columns.Add( "Ord_City", typeof( string ) );
                dtContractDetails.Columns.Add( "Ord_CountryId", typeof( int ) );
		        dtContractDetails.Columns.Add( "Ord_State", typeof( string ) );                                	   
		        dtContractDetails.Columns.Add( "Ord_Zip", typeof( string ) );                                 	  
		        dtContractDetails.Columns.Add( "Ord_Telephone", typeof( string ) );                            	  
		        dtContractDetails.Columns.Add( "Ord_Ext", typeof( string ) );                                  	   
		        dtContractDetails.Columns.Add( "Ord_Fax", typeof( string ) );                                  	  
		        dtContractDetails.Columns.Add( "Ord_EMail", typeof( string ) );
                dtContractDetails.Columns.Add( "Estimated_Contract_Value", typeof( decimal ) );

                dtContractDetails.Columns.Add( "Dates_Effective", typeof( DateTime ) );

                dtContractDetails.Columns.Add( "Dates_TotOptYrs", typeof( int ) );
                dtContractDetails.Columns.Add( "Pricelist_Verified", typeof( bool ) );
                dtContractDetails.Columns.Add( "Verification_Date", typeof( DateTime ) );            
		        dtContractDetails.Columns.Add( "Verified_By", typeof( string ) );                              	  
		        dtContractDetails.Columns.Add( "Current_Mod_Number", typeof( string ) );                       	  
		        dtContractDetails.Columns.Add( "Pricelist_Notes", typeof( string ) );
                dtContractDetails.Columns.Add( "SBAPlanID", typeof( int ) );
                dtContractDetails.Columns.Add( "VA_DOD", typeof( bool ) );
                dtContractDetails.Columns.Add( "Terminated_Convenience", typeof( bool ) );
                dtContractDetails.Columns.Add( "Terminated_Default", typeof( bool ) );                       	           

                dtContractDetails.Columns.Add( "SBA_Plan_Exempt", typeof( bool ) );
                dtContractDetails.Columns.Add( "Insurance_Policy_Effective_Date", typeof( DateTime ) );
                dtContractDetails.Columns.Add( "Insurance_Policy_Expiration_Date", typeof( DateTime ) );
                dtContractDetails.Columns.Add( "Solicitation_ID", typeof( int ) );
                dtContractDetails.Columns.Add( "Offer_ID", typeof( int ) );
                dtContractDetails.Columns.Add( "65IB_Contract_Type", typeof( int ) );                       	   
		        dtContractDetails.Columns.Add( "POC_Sales_Name", typeof( string ) );                           	  
		        dtContractDetails.Columns.Add( "POC_Sales_Phone", typeof( string ) );                          	  
		        dtContractDetails.Columns.Add( "POC_Sales_Ext", typeof( string ) );                            	   
		        dtContractDetails.Columns.Add( "POC_Sales_Fax", typeof( string ) );                            	  
		        dtContractDetails.Columns.Add( "POC_Sales_Email", typeof( string ) );                          	  
		        dtContractDetails.Columns.Add( "TradeAgreementActCompliance", typeof( string ) );
    
                dtContractDetails.Columns.Add( "StimulusAct", typeof( int ) );
                dtContractDetails.Columns.Add( "RebateRequired", typeof( bool ) );
                dtContractDetails.Columns.Add( "Standardized", typeof( bool ) );                             	           
		        dtContractDetails.Columns.Add( "CreatedBy", typeof( string ) );                       
		        dtContractDetails.Columns.Add( "CreationDate", typeof( DateTime ) );                             	
		        dtContractDetails.Columns.Add( "LastModifiedBy", typeof( string ) );
                dtContractDetails.Columns.Add( "LastModificationDate", typeof( DateTime ) );                          


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractRecordIdColumn;

                // add the keys to the table
                dtContractDetails.PrimaryKey = primaryKeyColumns;

                dtContractDetails.Clear();

                // connect
                dbConnection.Open();

                // run
                daContractDetails.Fill( dsContractDetails, "ContractDetailsTable" );

                RowsReturned = dsContractDetails.Tables[ "ContractDetailsTable" ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetContractInfoDetails(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetContractDefaultSIN( string contractNumber, ref string defaultSIN )
        {
            bool bSuccess = true;
            defaultSIN = "";
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetContractDefaultSIN
                //(
                //@CurrentUser uniqueidentifier,
                //@UserLogin nvarchar(120),
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(20),
                //@SIN varchar(10) OUTPUT
                //)

                SqlCommand cmdGetContractDefaultSINQuery = new SqlCommand( "GetContractDefaultSIN", dbConnection );
                cmdGetContractDefaultSINQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContractDefaultSINQuery.CommandTimeout = 30;

                AddStandardParameter( cmdGetContractDefaultSINQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdGetContractDefaultSINQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmSIN = new SqlParameter( "@SIN", SqlDbType.VarChar, 10 );
                parmSIN.Direction = ParameterDirection.Output;
       
                parmContractNumber.Value = contractNumber;

                cmdGetContractDefaultSINQuery.Parameters.Add( parmContractNumber );
                cmdGetContractDefaultSINQuery.Parameters.Add( parmSIN );

                // connect
                dbConnection.Open();

                cmdGetContractDefaultSINQuery.ExecuteNonQuery();

                defaultSIN = cmdGetContractDefaultSINQuery.Parameters[ "@SIN" ].Value.ToString();                
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetContractDefaultSIN(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool UpdateContractGeneralContractDates( int contractId, string contractNumber, DateTime contractAwardDate, DateTime contractEffectiveDate, DateTime contractExpirationDate, DateTime contractCompletionDate, bool bTerminatedByConvenience, bool bTerminatedByDefault, int totalOptionYears )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractGeneralContractDates
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),          
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@ContractAwardDate as DateTime,
                //@ContractEffectiveDate as DateTime,
                //@ContractExpirationDate as DateTime,
                //@ContractCompletionDate as DateTime = null,
                //@TerminatedByConvenience as bit, 
                //@TerminatedByDefault as bit, 
                //@TotalOptionYears as int

                SqlCommand cmdUpdateContractGeneralContractDatesQuery = new SqlCommand( "UpdateContractGeneralContractDates", dbConnection );
                cmdUpdateContractGeneralContractDatesQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractGeneralContractDatesQuery.CommandTimeout = 60;   // $$$ this causes the trigger to fire. increasing to 2 minutes did not help. Increasing from 30 to 60 instead on 3/29/2024.

                AddStandardParameter( cmdUpdateContractGeneralContractDatesQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractGeneralContractDatesQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );
 
                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmContractAwardDate = new SqlParameter( "@ContractAwardDate", SqlDbType.DateTime );
                SqlParameter parmContractEffectiveDate = new SqlParameter( "@ContractEffectiveDate", SqlDbType.DateTime );
                SqlParameter parmContractExpirationDate = new SqlParameter( "@ContractExpirationDate", SqlDbType.DateTime );
             
                SqlParameter parmContractCompletionDate = new SqlParameter( "@ContractCompletionDate", SqlDbType.DateTime );
                parmContractCompletionDate.IsNullable = true;

                SqlParameter parmTerminatedByConvenience = new SqlParameter( "@TerminatedByConvenience", SqlDbType.Bit );
                SqlParameter parmTerminatedByDefault = new SqlParameter( "@TerminatedByDefault", SqlDbType.Bit );  
                SqlParameter parmTotalOptionYears = new SqlParameter( "@TotalOptionYears", SqlDbType.Int );

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmContractAwardDate.Value = contractAwardDate;
                parmContractEffectiveDate.Value = contractEffectiveDate;
                parmContractExpirationDate.Value = contractExpirationDate;

                if( contractCompletionDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    parmContractCompletionDate.Value = null;
                else
                    parmContractCompletionDate.Value = contractCompletionDate;

                parmTerminatedByConvenience.Value = bTerminatedByConvenience;
                parmTerminatedByDefault.Value = bTerminatedByDefault;
                parmTotalOptionYears.Value = totalOptionYears;

                cmdUpdateContractGeneralContractDatesQuery.Parameters.Add( parmContractId );
                cmdUpdateContractGeneralContractDatesQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractGeneralContractDatesQuery.Parameters.Add( parmContractAwardDate );
                cmdUpdateContractGeneralContractDatesQuery.Parameters.Add( parmContractEffectiveDate );
                cmdUpdateContractGeneralContractDatesQuery.Parameters.Add( parmContractExpirationDate );
                cmdUpdateContractGeneralContractDatesQuery.Parameters.Add( parmContractCompletionDate );
                cmdUpdateContractGeneralContractDatesQuery.Parameters.Add( parmTerminatedByConvenience );
                cmdUpdateContractGeneralContractDatesQuery.Parameters.Add( parmTerminatedByDefault );
                cmdUpdateContractGeneralContractDatesQuery.Parameters.Add( parmTotalOptionYears );

                // connect
                dbConnection.Open();

                cmdUpdateContractGeneralContractDatesQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractGeneralContractDates(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateContractGeneralContractAttributes( int contractId, string contractNumber, string contractDescription, bool bVADOD, bool bPrimeVendor, string tradeAgreementActCompliance, bool bStimulusAct, bool bStandardized )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractGeneralContractAttributes
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@ContractDescription as nvarchar(50), 
                //@VADOD as bit,
                //@PrimeVendor as bit, 
                //@TradeAgreementActCompliance as nchar(1), 
                //@StimulusAct as bit,
                //@Standardized  as bit

                SqlCommand cmdUpdateContractGeneralContractAttributesQuery = new SqlCommand( "UpdateContractGeneralContractAttributes", dbConnection );
                cmdUpdateContractGeneralContractAttributesQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractGeneralContractAttributesQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractGeneralContractAttributesQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractGeneralContractAttributesQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmContractDescription = new SqlParameter( "@ContractDescription", SqlDbType.NVarChar, 50 );
                SqlParameter parmVADOD = new SqlParameter( "@VADOD", SqlDbType.Bit );
                SqlParameter parmPrimeVendor = new SqlParameter( "@PrimeVendor", SqlDbType.Bit );
                SqlParameter parmTradeAgreementActCompliance = new SqlParameter( "@TradeAgreementActCompliance", SqlDbType.NChar, 1 );
                SqlParameter parmStimulusAct = new SqlParameter( "@StimulusAct", SqlDbType.Bit );
                SqlParameter parmStandardized = new SqlParameter( "@Standardized", SqlDbType.Bit );

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmContractDescription.Value = contractDescription;
                parmVADOD.Value = bVADOD;
                parmPrimeVendor.Value = bPrimeVendor;
                parmTradeAgreementActCompliance.Value = tradeAgreementActCompliance;
                parmStimulusAct.Value = bStimulusAct;
                parmStandardized.Value = bStandardized;

                cmdUpdateContractGeneralContractAttributesQuery.Parameters.Add( parmContractId );
                cmdUpdateContractGeneralContractAttributesQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractGeneralContractAttributesQuery.Parameters.Add( parmContractDescription );
                cmdUpdateContractGeneralContractAttributesQuery.Parameters.Add( parmVADOD );
                cmdUpdateContractGeneralContractAttributesQuery.Parameters.Add( parmPrimeVendor );
                cmdUpdateContractGeneralContractAttributesQuery.Parameters.Add( parmTradeAgreementActCompliance );
                cmdUpdateContractGeneralContractAttributesQuery.Parameters.Add( parmStimulusAct );
                cmdUpdateContractGeneralContractAttributesQuery.Parameters.Add( parmStandardized );

                // connect
                dbConnection.Open();

                cmdUpdateContractGeneralContractAttributesQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractGeneralContractAttributes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateContractGeneralContractAssignment( int contractId, string contractNumber, int COID )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractGeneralContractAssignment
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@AssignedCOID int

                SqlCommand cmdUpdateContractGeneralContractAssignmentQuery = new SqlCommand( "UpdateContractGeneralContractAssignment", dbConnection );
                cmdUpdateContractGeneralContractAssignmentQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractGeneralContractAssignmentQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractGeneralContractAssignmentQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractGeneralContractAssignmentQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmAssignedCOID = new SqlParameter( "@AssignedCOID", SqlDbType.Int );

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmAssignedCOID.Value = COID;

                cmdUpdateContractGeneralContractAssignmentQuery.Parameters.Add( parmContractId );
                cmdUpdateContractGeneralContractAssignmentQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractGeneralContractAssignmentQuery.Parameters.Add( parmAssignedCOID );

                // connect
                dbConnection.Open();

                cmdUpdateContractGeneralContractAssignmentQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractGeneralContractAssignment(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetContractOwnerRelatedInfo( string contractNumber, ref int COID, ref string contractingOfficerFullName, ref string contractingOfficerPhone, ref Guid contractingOfficerUserId, ref int seniorContractSpecialistCOID, ref string seniorContractSpecialistName,  ref int assistantDirectorCOID, ref string assistantDirectorName )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetContractOwnerInfo
                //(
                //@CurrentUser uniqueidentifier,
                //@UserLogin nvarchar(120),
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(20),
                //@COID int OUTPUT,
                //@ContractingOfficerFullName nvarchar(80) OUTPUT,
                //@ContractingOfficerPhone nvarchar(20) OUTPUT,
                //@ContractingOfficerUserId uniqueidentifier OUTPUT,
                //@SeniorContractSpecialistCOID int OUTPUT,
                //@SeniorContractSpecialistName nvarchar(80) OUTPUT,
                //@AssistantDirectorCOID int OUTPUT,
                //@AssistantDirectorName nvarchar(80) OUTPUT

                SqlCommand cmdGetContractOwnerInfoQuery = new SqlCommand( "GetContractOwnerInfo", dbConnection );
                cmdGetContractOwnerInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContractOwnerInfoQuery.CommandTimeout = 30;

                AddStandardParameter( cmdGetContractOwnerInfoQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdGetContractOwnerInfoQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                parmCOID.Direction = ParameterDirection.Output;

                SqlParameter parmContractingOfficerFullName = new SqlParameter( "@ContractingOfficerFullName", SqlDbType.NVarChar, 80 );
                parmContractingOfficerFullName.Direction = ParameterDirection.Output;

                SqlParameter parmContractingOfficerPhone = new SqlParameter( "@ContractingOfficerPhone", SqlDbType.NVarChar, 20 );
                parmContractingOfficerPhone.Direction = ParameterDirection.Output;

                SqlParameter parmContractingOfficerUserId = new SqlParameter( "@ContractingOfficerUserId", SqlDbType.UniqueIdentifier );
                parmContractingOfficerUserId.Direction = ParameterDirection.Output;

                SqlParameter parmSeniorContractSpecialistCOID = new SqlParameter( "@SeniorContractSpecialistCOID", SqlDbType.Int );
                parmSeniorContractSpecialistCOID.Direction = ParameterDirection.Output;
                
                SqlParameter parmSeniorContractSpecialistName = new SqlParameter( "@SeniorContractSpecialistName", SqlDbType.NVarChar, 80 );
                parmSeniorContractSpecialistName.Direction = ParameterDirection.Output;

                SqlParameter parmAssistantDirectorCOID = new SqlParameter( "@AssistantDirectorCOID", SqlDbType.Int );
                parmAssistantDirectorCOID.Direction = ParameterDirection.Output;

                SqlParameter parmAssistantDirectorName = new SqlParameter( "@AssistantDirectorName", SqlDbType.NVarChar, 80 );
                parmAssistantDirectorName.Direction = ParameterDirection.Output;

                parmUserLogin.Value = _userName;
                parmContractNumber.Value = contractNumber;

                cmdGetContractOwnerInfoQuery.Parameters.Add( parmUserLogin );
                cmdGetContractOwnerInfoQuery.Parameters.Add( parmContractNumber );
                cmdGetContractOwnerInfoQuery.Parameters.Add( parmCOID );
                cmdGetContractOwnerInfoQuery.Parameters.Add( parmContractingOfficerFullName );
                cmdGetContractOwnerInfoQuery.Parameters.Add( parmContractingOfficerPhone );
                cmdGetContractOwnerInfoQuery.Parameters.Add( parmContractingOfficerUserId );
                cmdGetContractOwnerInfoQuery.Parameters.Add( parmSeniorContractSpecialistCOID );
                cmdGetContractOwnerInfoQuery.Parameters.Add( parmSeniorContractSpecialistName );
                cmdGetContractOwnerInfoQuery.Parameters.Add( parmAssistantDirectorCOID );
                cmdGetContractOwnerInfoQuery.Parameters.Add( parmAssistantDirectorName );
                
                // connect
                dbConnection.Open();

                cmdGetContractOwnerInfoQuery.ExecuteNonQuery();

                COID = int.Parse( cmdGetContractOwnerInfoQuery.Parameters[ "@COID" ].Value.ToString() );
                contractingOfficerFullName = cmdGetContractOwnerInfoQuery.Parameters[ "@ContractingOfficerFullName" ].Value.ToString();
                contractingOfficerPhone = cmdGetContractOwnerInfoQuery.Parameters[ "@ContractingOfficerPhone" ].Value.ToString();
                contractingOfficerUserId = new Guid( cmdGetContractOwnerInfoQuery.Parameters[ "@ContractingOfficerUserId" ].Value.ToString() );
                seniorContractSpecialistCOID = int.Parse( cmdGetContractOwnerInfoQuery.Parameters[ "@SeniorContractSpecialistCOID" ].Value.ToString() );
                seniorContractSpecialistName = cmdGetContractOwnerInfoQuery.Parameters[ "@SeniorContractSpecialistName" ].Value.ToString();
                assistantDirectorCOID = int.Parse( cmdGetContractOwnerInfoQuery.Parameters[ "@AssistantDirectorCOID" ].Value.ToString() );
                assistantDirectorName = cmdGetContractOwnerInfoQuery.Parameters[ "@AssistantDirectorName" ].Value.ToString();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetContractOwnerRelatedInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetContractCountryInfo( string contractNumber, ref string countryName )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetContractCountryInfo
                //(
                //@CurrentUser uniqueidentifier,
                //@UserLogin nvarchar(120),
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(20),              
                //@CountryName nvarchar(100) OUTPUT

                SqlCommand cmdGetContractCountryInfoQuery = new SqlCommand( "GetContractCountryInfo", dbConnection );
                cmdGetContractCountryInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContractCountryInfoQuery.CommandTimeout = 30;

                AddStandardParameter( cmdGetContractCountryInfoQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                
                SqlParameter parmCountryName = new SqlParameter( "@CountryName", SqlDbType.NVarChar, 100 );
                parmCountryName.Direction = ParameterDirection.Output;

                parmUserLogin.Value = _userName;
                parmContractNumber.Value = contractNumber;

                cmdGetContractCountryInfoQuery.Parameters.Add( parmUserLogin );
                cmdGetContractCountryInfoQuery.Parameters.Add( parmContractNumber );
                
                cmdGetContractCountryInfoQuery.Parameters.Add( parmCountryName );

                // connect
                dbConnection.Open();

                cmdGetContractCountryInfoQuery.ExecuteNonQuery();
               
                countryName = cmdGetContractCountryInfoQuery.Parameters[ "@CountryName" ].Value.ToString();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetContractCountryInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetContractInfo( string contractNumber, ref int contractId, ref int scheduleNumber, ref int ownerId, ref string vendorName, ref string description, ref DateTime awardDate, ref DateTime expirationDate, ref DateTime completionDate, ref DateTime effectiveDate, 
            ref string scheduleName, ref string ownerName, ref string vendorWebAddress, ref string vendorAddress1, ref string vendorAddress2, ref string vendorCity, ref string vendorState, ref string vendorCountryName, ref int vendorCountryId, ref string vendorZip )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetContractInfo
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@ContractId int OUTPUT,
                //@ScheduleNumber int OUTPUT,
                //@OwnerId int OUTPUT,
                //@VendorName nvarchar(75) OUTPUT,
                //@Description nvarchar(50) OUTPUT,
                //@AwardDate datetime OUTPUT,
                //@ExpirationDate datetime OUTPUT,
                //@CompletionDate datetime OUTPUT,
                //@EffectiveDate datetime OUTPUT,
                //@ScheduleName nvarchar(75) OUTPUT,
                //@OwnerName nvarchar(80) OUTPUT,
                //@VendorWebAddress nvarchar(50) OUTPUT,  -- these are used to fill the parent for header display only
                //@VendorAddress1 nvarchar(100) OUTPUT,
                //@VendorAddress2 nvarchar(100) OUTPUT,
                //@VendorCity nvarchar(20) OUTPUT,
                //@VendorState nvarchar(2) OUTPUT,
                //@VendorCountryName nvarchar(100) OUTPUT,
                //@VendorCountryId int OUTPUT,
                //@VendorZip nvarchar(10) OUTPUT

                SqlCommand cmdGetContractInfoQuery = new SqlCommand( "GetContractInfo", dbConnection );
                cmdGetContractInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContractInfoQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                parmContractId.Direction = ParameterDirection.Output;
                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                parmScheduleNumber.Direction = ParameterDirection.Output;
                SqlParameter parmOwnerId = new SqlParameter( "@OwnerId", SqlDbType.Int );
                parmOwnerId.Direction = ParameterDirection.Output;
                SqlParameter parmVendorName = new SqlParameter( "@VendorName", SqlDbType.NVarChar, 75 );
                parmVendorName.Direction = ParameterDirection.Output;
                SqlParameter parmDescription = new SqlParameter( "@Description", SqlDbType.NVarChar, 50 );
                parmDescription.Direction = ParameterDirection.Output;
                SqlParameter parmAwardDate = new SqlParameter( "@AwardDate", SqlDbType.DateTime);
                parmAwardDate.Direction = ParameterDirection.Output;
                SqlParameter parmExpirationDate = new SqlParameter( "@ExpirationDate", SqlDbType.DateTime );
                parmExpirationDate.Direction = ParameterDirection.Output;
                SqlParameter parmCompletionDate = new SqlParameter( "@CompletionDate", SqlDbType.DateTime );
                parmCompletionDate.Direction = ParameterDirection.Output;
                SqlParameter parmEffectiveDate = new SqlParameter( "@EffectiveDate", SqlDbType.DateTime );
                parmEffectiveDate.Direction = ParameterDirection.Output;
                SqlParameter parmScheduleName = new SqlParameter( "@ScheduleName", SqlDbType.NVarChar, 75 );
                parmScheduleName.Direction = ParameterDirection.Output;
                SqlParameter parmOwnerName = new SqlParameter( "@OwnerName", SqlDbType.NVarChar, 80 );
                parmOwnerName.Direction = ParameterDirection.Output;

                SqlParameter parmVendorWebAddress = new SqlParameter( "@VendorWebAddress", SqlDbType.NVarChar, 50 );
                parmVendorWebAddress.Direction = ParameterDirection.Output;
                SqlParameter parmVendorAddress1 = new SqlParameter( "@VendorAddress1", SqlDbType.NVarChar, 100 );
                parmVendorAddress1.Direction = ParameterDirection.Output;
                SqlParameter parmVendorAddress2 = new SqlParameter( "@VendorAddress2", SqlDbType.NVarChar, 100 );
                parmVendorAddress2.Direction = ParameterDirection.Output;
                SqlParameter parmVendorCity = new SqlParameter( "@VendorCity", SqlDbType.NVarChar, 20 );
                parmVendorCity.Direction = ParameterDirection.Output;
                SqlParameter parmVendorState = new SqlParameter( "@VendorState", SqlDbType.NVarChar, 2 );
                parmVendorState.Direction = ParameterDirection.Output;
                SqlParameter parmVendorCountryName = new SqlParameter( "@VendorCountryName", SqlDbType.NVarChar, 100 );
                parmVendorCountryName.Direction = ParameterDirection.Output;
                SqlParameter parmVendorCountryId = new SqlParameter( "@VendorCountryId", SqlDbType.Int );
                parmVendorCountryId.Direction = ParameterDirection.Output;
                SqlParameter parmVendorZip = new SqlParameter( "@VendorZip", SqlDbType.NVarChar, 10 );
                parmVendorZip.Direction = ParameterDirection.Output;

                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;

                cmdGetContractInfoQuery.Parameters.Add( parmCurrentUser );
                cmdGetContractInfoQuery.Parameters.Add( parmContractNumber );
                cmdGetContractInfoQuery.Parameters.Add( parmContractId );
                cmdGetContractInfoQuery.Parameters.Add( parmScheduleNumber );
                cmdGetContractInfoQuery.Parameters.Add( parmOwnerId );
                cmdGetContractInfoQuery.Parameters.Add( parmVendorName );
                cmdGetContractInfoQuery.Parameters.Add( parmDescription );
                cmdGetContractInfoQuery.Parameters.Add( parmAwardDate );
                cmdGetContractInfoQuery.Parameters.Add( parmExpirationDate );
                cmdGetContractInfoQuery.Parameters.Add( parmCompletionDate );
                cmdGetContractInfoQuery.Parameters.Add( parmEffectiveDate );
                cmdGetContractInfoQuery.Parameters.Add( parmScheduleName );
                cmdGetContractInfoQuery.Parameters.Add( parmOwnerName );

                cmdGetContractInfoQuery.Parameters.Add( parmVendorWebAddress );
                cmdGetContractInfoQuery.Parameters.Add( parmVendorAddress1 );
                cmdGetContractInfoQuery.Parameters.Add( parmVendorAddress2 );
                cmdGetContractInfoQuery.Parameters.Add( parmVendorCity );
                cmdGetContractInfoQuery.Parameters.Add( parmVendorState );
                cmdGetContractInfoQuery.Parameters.Add( parmVendorCountryName );
                cmdGetContractInfoQuery.Parameters.Add( parmVendorCountryId );
                cmdGetContractInfoQuery.Parameters.Add( parmVendorZip );

                // connect
                dbConnection.Open();

                cmdGetContractInfoQuery.ExecuteNonQuery();

                contractId = int.Parse( cmdGetContractInfoQuery.Parameters[ "@ContractId" ].Value.ToString() );
                scheduleNumber = int.Parse( cmdGetContractInfoQuery.Parameters[ "@ScheduleNumber" ].Value.ToString() );
                ownerId = int.Parse( cmdGetContractInfoQuery.Parameters[ "@OwnerId" ].Value.ToString() );
                vendorName = cmdGetContractInfoQuery.Parameters[ "@VendorName" ].Value.ToString();
                description = cmdGetContractInfoQuery.Parameters[ "@Description" ].Value.ToString();
                if( cmdGetContractInfoQuery.Parameters[ "@AwardDate" ].Value == DBNull.Value )
                    awardDate = DateTime.MinValue;
                else
                    awardDate = DateTime.Parse( cmdGetContractInfoQuery.Parameters[ "@AwardDate" ].Value.ToString() );

                if( cmdGetContractInfoQuery.Parameters[ "@ExpirationDate" ].Value == DBNull.Value )
                    expirationDate = DateTime.MinValue;
                else
                    expirationDate = DateTime.Parse( cmdGetContractInfoQuery.Parameters[ "@ExpirationDate" ].Value.ToString() );

                if( cmdGetContractInfoQuery.Parameters[ "@CompletionDate" ].Value == DBNull.Value )
                    completionDate = DateTime.MinValue;
                else
                    completionDate = DateTime.Parse( cmdGetContractInfoQuery.Parameters[ "@CompletionDate" ].Value.ToString() );

                if( cmdGetContractInfoQuery.Parameters[ "@EffectiveDate" ].Value == DBNull.Value )
                    effectiveDate = DateTime.MinValue;
                else
                    effectiveDate = DateTime.Parse( cmdGetContractInfoQuery.Parameters[ "@EffectiveDate" ].Value.ToString() );

                scheduleName = cmdGetContractInfoQuery.Parameters[ "@ScheduleName" ].Value.ToString();
                ownerName = cmdGetContractInfoQuery.Parameters[ "@OwnerName" ].Value.ToString();

                vendorWebAddress = cmdGetContractInfoQuery.Parameters[ "@VendorWebAddress" ].Value.ToString();
                vendorAddress1 = cmdGetContractInfoQuery.Parameters[ "@VendorAddress1" ].Value.ToString();
                vendorAddress2 = cmdGetContractInfoQuery.Parameters[ "@VendorAddress2" ].Value.ToString();
                vendorCity = cmdGetContractInfoQuery.Parameters[ "@VendorCity" ].Value.ToString();
                vendorState = cmdGetContractInfoQuery.Parameters[ "@VendorState" ].Value.ToString();
                vendorCountryName = cmdGetContractInfoQuery.Parameters[ "@VendorCountryName" ].Value.ToString();
                
                if( cmdGetContractInfoQuery.Parameters[ "@VendorCountryId" ].Value == DBNull.Value )
                    vendorCountryId = -1;
                else
                    vendorCountryId = int.Parse( cmdGetContractInfoQuery.Parameters[ "@VendorCountryId" ].Value.ToString() );

                vendorZip = cmdGetContractInfoQuery.Parameters[ "@VendorZip" ].Value.ToString();
 
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetContractInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetParentContractInfoForBPA( string contractNumber, ref DataSet dsParentContractInfo )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            dsParentContractInfo = null;
            SqlDataAdapter daParentContractInfo = null;
            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetParentFSSContractInfoForBPA
                //(
                //@UserLogin nvarchar(120),
                //@UserId uniqueidentifier,
                //@ContractNumber nvarchar(20),
               

                SqlCommand cmdGetContractInfoQuery = new SqlCommand( "GetParentFSSContractInfoForBPA", dbConnection );
                cmdGetContractInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContractInfoQuery.CommandTimeout = 30;

                AddStandardParameter( cmdGetContractInfoQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                cmdGetContractInfoQuery.Parameters.Add( parmUserLogin );
                cmdGetContractInfoQuery.Parameters.Add( parmContractNumber );
          

                parmUserLogin.Value = _userName;
                parmContractNumber.Value = contractNumber;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daParentContractInfo = new SqlDataAdapter();
                daParentContractInfo.SelectCommand = cmdGetContractInfoQuery;

                dsParentContractInfo = new DataSet( "ParentContractInfo" );
                DataTable dtParentContractInfo = dsParentContractInfo.Tables.Add( "ParentContractInfoTable" );

                // add the common elements to the table
                DataColumn contractNumberColumn = new DataColumn( "CntrctNum", typeof( string ) );

                dtParentContractInfo.Columns.Add( contractNumberColumn );

                dtParentContractInfo.Columns.Add( new DataColumn( "Drug_Covered", typeof( string ) ));
                dtParentContractInfo.Columns.Add( new DataColumn( "Contractor_Name", typeof( string ) ));
                dtParentContractInfo.Columns.Add( new DataColumn( "Dates_CntrctAward", typeof( DateTime ) ));
                dtParentContractInfo.Columns.Add( new DataColumn( "Dates_Effective", typeof( DateTime ) ) );
                dtParentContractInfo.Columns.Add( new DataColumn( "Dates_CntrctExp", typeof( DateTime ) ) );
                dtParentContractInfo.Columns.Add( new DataColumn( "Dates_Completion", typeof( DateTime ) ) );
                dtParentContractInfo.Columns.Add( new DataColumn( "Schedule_Name", typeof( string ) ));
                dtParentContractInfo.Columns.Add( new DataColumn( "FullName", typeof( string ) ));

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractNumberColumn;

                // add the keys to the table
                dtParentContractInfo.PrimaryKey = primaryKeyColumns;

                dtParentContractInfo.Clear();

                // connect
                dbConnection.Open();

                // run
                daParentContractInfo.Fill( dsParentContractInfo, "ParentContractInfoTable" );

                RowsReturned = dsParentContractInfo.Tables[ "ParentContractInfoTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetParentFSSContractInfoForBPA(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // use parent contract info to backfill for a new BPA upon creation
        public bool GetContractAndVendorInfo( string contractNumber, ref int scheduleNumber, ref int ownerId, ref string vendorName, ref string description, 
            ref DateTime awardDate, ref DateTime expirationDate, ref DateTime completionDate, ref DateTime effectiveDate, ref string scheduleName, ref string ownerName,
            ref string primaryName, ref string primaryPhone, ref string primaryExt, ref string primaryFax, ref string primaryEmail, ref string vendorWebUrl, 
            ref string primaryAddress1, ref string primaryAddress2, ref string primaryCity, ref int primaryCountryId, ref string primaryState, ref string primaryZip )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetContractAndVendorInfo
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@ScheduleNumber int OUTPUT,
                //@OwnerId int OUTPUT,
                //@VendorName nvarchar(75) OUTPUT,
                //@Description nvarchar(50) OUTPUT,
                //@AwardDate datetime OUTPUT,
                //@ExpirationDate datetime OUTPUT,
                //@CompletionDate datetime OUTPUT,
                //@EffectiveDate datetime OUTPUT,
                //@ScheduleName nvarchar(75) OUTPUT,
                //@OwnerName nvarchar(80) OUTPUT,

                //@PrimaryName nvarchar(30) OUTPUT,
                //@PrimaryPhone nvarchar(15) OUTPUT,
                //@PrimaryExt nvarchar(5) OUTPUT,
                //@PrimaryFax nvarchar(15) OUTPUT,
                //@PrimaryEmail nvarchar(50) OUTPUT,
                //@VendorWebUrl nvarchar(50) OUTPUT,
                //@PrimaryAddress1 nvarchar(100) OUTPUT,
                //@PrimaryAddress2 nvarchar(100) OUTPUT,
                //@PrimaryCity nvarchar(20) OUTPUT,
                //@PrimaryCountryId int OUTPUT,
                //@PrimaryState nvarchar(2) OUTPUT,
                //@PrimaryZip nvarchar(10) OUTPUT

                SqlCommand cmdGetContractAndVendorInfoQuery = new SqlCommand( "GetContractAndVendorInfo", dbConnection );
                cmdGetContractAndVendorInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContractAndVendorInfoQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                parmScheduleNumber.Direction = ParameterDirection.Output;
                SqlParameter parmOwnerId = new SqlParameter( "@OwnerId", SqlDbType.Int );
                parmOwnerId.Direction = ParameterDirection.Output;
                SqlParameter parmVendorName = new SqlParameter( "@VendorName", SqlDbType.NVarChar, 75 );
                parmVendorName.Direction = ParameterDirection.Output;
                SqlParameter parmDescription = new SqlParameter( "@Description", SqlDbType.NVarChar, 50 );
                parmDescription.Direction = ParameterDirection.Output;
                SqlParameter parmAwardDate = new SqlParameter( "@AwardDate", SqlDbType.DateTime );
                parmAwardDate.Direction = ParameterDirection.Output;
                SqlParameter parmExpirationDate = new SqlParameter( "@ExpirationDate", SqlDbType.DateTime );
                parmExpirationDate.Direction = ParameterDirection.Output;
                SqlParameter parmCompletionDate = new SqlParameter( "@CompletionDate", SqlDbType.DateTime );
                parmCompletionDate.Direction = ParameterDirection.Output;
                SqlParameter parmEffectiveDate = new SqlParameter( "@EffectiveDate", SqlDbType.DateTime );
                parmEffectiveDate.Direction = ParameterDirection.Output;
                SqlParameter parmScheduleName = new SqlParameter( "@ScheduleName", SqlDbType.NVarChar, 75 );
                parmScheduleName.Direction = ParameterDirection.Output;
                SqlParameter parmOwnerName = new SqlParameter( "@OwnerName", SqlDbType.NVarChar, 80 );
                parmOwnerName.Direction = ParameterDirection.Output;
  
                SqlParameter parmPrimaryName = new SqlParameter( "@PrimaryName", SqlDbType.NVarChar, 30 );
                parmPrimaryName.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryPhone = new SqlParameter( "@PrimaryPhone", SqlDbType.NVarChar, 15 );
                parmPrimaryPhone.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryExt = new SqlParameter( "@PrimaryExt", SqlDbType.NVarChar, 5 );
                parmPrimaryExt.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryFax = new SqlParameter( "@PrimaryFax", SqlDbType.NVarChar, 15 );
                parmPrimaryFax.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryEmail = new SqlParameter( "@PrimaryEmail", SqlDbType.NVarChar, 50 );
                parmPrimaryEmail.Direction = ParameterDirection.Output;
                SqlParameter parmVendorWebUrl = new SqlParameter( "@VendorWebUrl", SqlDbType.NVarChar, 50 );
                parmVendorWebUrl.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryAddress1 = new SqlParameter( "@PrimaryAddress1", SqlDbType.NVarChar, 100 );
                parmPrimaryAddress1.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryAddress2 = new SqlParameter( "@PrimaryAddress2", SqlDbType.NVarChar, 100 );
                parmPrimaryAddress2.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryCity = new SqlParameter( "@PrimaryCity", SqlDbType.NVarChar, 20 );
                parmPrimaryCity.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryCountryId = new SqlParameter( "@PrimaryCountryId", SqlDbType.Int );
                parmPrimaryCountryId.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryState = new SqlParameter( "@PrimaryState", SqlDbType.NVarChar, 2 );
                parmPrimaryState.Direction = ParameterDirection.Output;
                SqlParameter parmPrimaryZip = new SqlParameter( "@PrimaryZip", SqlDbType.NVarChar, 10 );
                parmPrimaryZip.Direction = ParameterDirection.Output;
 

                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;

                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmCurrentUser );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmContractNumber );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmScheduleNumber );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmOwnerId );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmVendorName );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmDescription );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmAwardDate );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmExpirationDate );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmCompletionDate );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmEffectiveDate );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmScheduleName );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmOwnerName );

                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryName );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryPhone );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryExt );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryFax );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryEmail );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmVendorWebUrl );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryAddress1 );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryAddress2 );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryCity );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryCountryId );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryState );
                cmdGetContractAndVendorInfoQuery.Parameters.Add( parmPrimaryZip );

                // connect
                dbConnection.Open();

                cmdGetContractAndVendorInfoQuery.ExecuteNonQuery();

                scheduleNumber = int.Parse( cmdGetContractAndVendorInfoQuery.Parameters[ "@ScheduleNumber" ].Value.ToString() );
                ownerId = int.Parse( cmdGetContractAndVendorInfoQuery.Parameters[ "@OwnerId" ].Value.ToString() );
                vendorName = cmdGetContractAndVendorInfoQuery.Parameters[ "@VendorName" ].Value.ToString();
                description = cmdGetContractAndVendorInfoQuery.Parameters[ "@Description" ].Value.ToString();
                if( cmdGetContractAndVendorInfoQuery.Parameters[ "@AwardDate" ].Value == DBNull.Value )
                    awardDate = DateTime.MinValue;
                else
                    awardDate = DateTime.Parse( cmdGetContractAndVendorInfoQuery.Parameters[ "@AwardDate" ].Value.ToString() );

                if( cmdGetContractAndVendorInfoQuery.Parameters[ "@ExpirationDate" ].Value == DBNull.Value )
                    expirationDate = DateTime.MinValue;
                else
                    expirationDate = DateTime.Parse( cmdGetContractAndVendorInfoQuery.Parameters[ "@ExpirationDate" ].Value.ToString() );

                if( cmdGetContractAndVendorInfoQuery.Parameters[ "@CompletionDate" ].Value == DBNull.Value )
                    completionDate = DateTime.MinValue;
                else
                    completionDate = DateTime.Parse( cmdGetContractAndVendorInfoQuery.Parameters[ "@CompletionDate" ].Value.ToString() );

                if( cmdGetContractAndVendorInfoQuery.Parameters[ "@EffectiveDate" ].Value == DBNull.Value )
                    effectiveDate = DateTime.MinValue;
                else
                    effectiveDate = DateTime.Parse( cmdGetContractAndVendorInfoQuery.Parameters[ "@EffectiveDate" ].Value.ToString() );

                scheduleName = cmdGetContractAndVendorInfoQuery.Parameters[ "@ScheduleName" ].Value.ToString();
                ownerName = cmdGetContractAndVendorInfoQuery.Parameters[ "@OwnerName" ].Value.ToString();

                primaryName = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryName" ].Value.ToString();
                primaryPhone = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryPhone" ].Value.ToString();
                primaryExt = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryExt" ].Value.ToString();
                primaryFax = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryFax" ].Value.ToString();
                primaryEmail = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryEmail" ].Value.ToString();
                vendorWebUrl = cmdGetContractAndVendorInfoQuery.Parameters[ "@VendorWebUrl" ].Value.ToString();
                primaryAddress1 = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryAddress1" ].Value.ToString();
                primaryAddress2 = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryAddress2" ].Value.ToString();
                primaryCity = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryCity" ].Value.ToString();
                primaryCountryId = int.Parse( cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryCountryId" ].Value.ToString() );
                primaryState = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryState" ].Value.ToString();
                primaryZip = cmdGetContractAndVendorInfoQuery.Parameters[ "@PrimaryZip" ].Value.ToString();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetContractAndVendorInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateSBAPlanInfoInContract( string contractNumber, int sbaPlanId, bool sbaPlanExemptStatus )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //UpdateSBAPlanInfoInContract
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@SBAPlanId int,
                //@SBAPlanExemptStatus bit

                SqlCommand cmdUpdateSBAPlanInfoQuery = new SqlCommand( "UpdateSBAPlanInfoInContract", dbConnection );
                cmdUpdateSBAPlanInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateSBAPlanInfoQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmSBAPlanId = new SqlParameter( "@SBAPlanId", SqlDbType.Int );
                SqlParameter parmSBAPlanExemptStatus = new SqlParameter( "@SBAPlanExemptStatus", SqlDbType.Bit );

                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;
                parmSBAPlanId.Value = sbaPlanId;
                parmSBAPlanExemptStatus.Value = sbaPlanExemptStatus;

                cmdUpdateSBAPlanInfoQuery.Parameters.Add( parmCurrentUser );
                cmdUpdateSBAPlanInfoQuery.Parameters.Add( parmContractNumber );
                cmdUpdateSBAPlanInfoQuery.Parameters.Add( parmSBAPlanId );
                cmdUpdateSBAPlanInfoQuery.Parameters.Add( parmSBAPlanExemptStatus );

                // connect
                dbConnection.Open();

                cmdUpdateSBAPlanInfoQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateSBAPlanInfoInContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateOwnerInfoInContract( string contractNumber, int newContractOwnerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //UpdateOwnerInfoInContract
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@NewContractOwnerId int

                SqlCommand cmdUpdateOwnerInfoInContractQuery = new SqlCommand( "UpdateOwnerInfoInContract", dbConnection );
                cmdUpdateOwnerInfoInContractQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateOwnerInfoInContractQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmNewContractOwnerId = new SqlParameter( "@NewContractOwnerId", SqlDbType.Int );

                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;
                parmNewContractOwnerId.Value = newContractOwnerId;

                cmdUpdateOwnerInfoInContractQuery.Parameters.Add( parmCurrentUser );
                cmdUpdateOwnerInfoInContractQuery.Parameters.Add( parmContractNumber );
                cmdUpdateOwnerInfoInContractQuery.Parameters.Add( parmNewContractOwnerId );

                // connect
                dbConnection.Open();

                cmdUpdateOwnerInfoInContractQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateOwnerInfoInContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool CopyContract( string oldContractNumber, string newContractNumber, DateTime awardDate, DateTime effectiveDate, DateTime expirationDate, int optionYears, ref int newContractId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            newContractId = -1;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //CopyContract
                //(
                //@CurrentUser uniqueidentifier,
                //@DrugItemServerName nvarchar(255),
                //@DrugItemDatabaseName nvarchar(255),
                //@OldContractNumber nvarchar(50),
                //@NewContractNumber nvarchar(50),
                //@AwardDate datetime,
                //@EffectiveDate datetime,
                //@ExpirationDate datetime,
                //@OptionYears int,
                //@IsItemVersion2 bit,
                //@NewContractId int OUTPUT
                //)

                SqlCommand cmdCopyContractQuery = new SqlCommand( "CopyContract", dbConnection );
                cmdCopyContractQuery.CommandType = CommandType.StoredProcedure;
                cmdCopyContractQuery.CommandTimeout = 30;

                AddStandardParameter( cmdCopyContractQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdCopyContractQuery, StandardParameterTypes.DrugItemDatabase, Config.DrugItemDatabaseServer, Config.DrugItemDatabase );

            //    SqlParameter parmUserId = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                SqlParameter parmOldContractNumber = new SqlParameter( "@OldContractNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmNewContractNumber = new SqlParameter( "@NewContractNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmAwardDate = new SqlParameter( "@AwardDate", SqlDbType.DateTime );
                SqlParameter parmEffectiveDate = new SqlParameter( "@EffectiveDate", SqlDbType.DateTime );
                SqlParameter parmExpirationDate = new SqlParameter( "@ExpirationDate", SqlDbType.DateTime );
                SqlParameter parmOptionYears = new SqlParameter( "@OptionYears", SqlDbType.Int );
                SqlParameter parmIsItemVersion2 = new SqlParameter( "@IsItemVersion2", SqlDbType.Bit );

                SqlParameter parmNewContractId = new SqlParameter( "@NewContractId", SqlDbType.Int );
                parmNewContractId.Direction = ParameterDirection.Output;

        //        parmUserId.Value = _currentUserId;
                parmOldContractNumber.Value = oldContractNumber;
                parmNewContractNumber.Value = newContractNumber;
                parmAwardDate.Value = awardDate;
                parmEffectiveDate.Value = effectiveDate;
                parmExpirationDate.Value = expirationDate;
                parmOptionYears.Value = optionYears;
                parmIsItemVersion2.Value = ( Config.ItemVersion.CompareTo( "I2" ) == 0 ) ? true : false;

         //       cmdCopyContractQuery.Parameters.Add( parmUserId );
                cmdCopyContractQuery.Parameters.Add( parmOldContractNumber );
                cmdCopyContractQuery.Parameters.Add( parmNewContractNumber );
                cmdCopyContractQuery.Parameters.Add( parmAwardDate );
                cmdCopyContractQuery.Parameters.Add( parmEffectiveDate );
                cmdCopyContractQuery.Parameters.Add( parmExpirationDate );
                cmdCopyContractQuery.Parameters.Add( parmOptionYears );            
                cmdCopyContractQuery.Parameters.Add( parmIsItemVersion2 );
                cmdCopyContractQuery.Parameters.Add( parmNewContractId );

                // connect
                dbConnection.Open();

                cmdCopyContractQuery.ExecuteNonQuery();

                newContractId = int.Parse( cmdCopyContractQuery.Parameters[ "@NewContractId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.CopyContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool LogUserActivity( string actionTypeString, string actionDetails, string actionDetailsTypeString )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //InsertUserActivity
                //(
                //@UserName nvarchar(100),
                //@ActionType nchar(1),
                //@ActionDetails nvarchar(80),
                //@ActionDetailsType nchar(1)

                SqlCommand cmdLogUserActivityQuery = new SqlCommand( "InsertUserActivity", dbConnection );
                cmdLogUserActivityQuery.CommandType = CommandType.StoredProcedure;
                cmdLogUserActivityQuery.CommandTimeout = 30;


                SqlParameter parmUserName = new SqlParameter( "@UserName", SqlDbType.NVarChar, 100 );
                SqlParameter parmActionType = new SqlParameter( "@ActionType", SqlDbType.NChar, 1 );
                SqlParameter parmActionDetails = new SqlParameter( "@ActionDetails", SqlDbType.NVarChar, 80 );
                SqlParameter parmActionDetailsType = new SqlParameter( "@ActionDetailsType", SqlDbType.NChar, 1 );

                parmUserName.Value = _userName;
                parmActionType.Value = actionTypeString;
                parmActionDetails.Value = actionDetails;
                parmActionDetailsType.Value = actionDetailsTypeString;

                cmdLogUserActivityQuery.Parameters.Add( parmUserName );
                cmdLogUserActivityQuery.Parameters.Add( parmActionType );
                cmdLogUserActivityQuery.Parameters.Add( parmActionDetails );
                cmdLogUserActivityQuery.Parameters.Add( parmActionDetailsType );

                // connect
                dbConnection.Open();

                cmdLogUserActivityQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.LogUserActivity(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool LogUserContractAccess( string contractNumber, int contractId )
        {
            return ( LogUserDocumentAccess( "C", contractNumber, contractId ) );
        }

        public bool LogUserOfferAccess( string offerNumber, int offerId )
        {
            return ( LogUserDocumentAccess( "O", offerNumber, offerId ) );
        }

        public bool LogUserDocumentAccess( string documentTypeString, string documentNumber, int documentId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //InsertUserRecentDocument
                //(
                //@UserId uniqueidentifier,
                //@DocumentType nchar(1),  -- 'C' = contract or BPA, 'O' = offer
                //@DocumentNumber nvarchar(50), -- contract number or offer number
                //@DocumentId int -- contractId or offerId
                //)

                SqlCommand cmdInsertUserDocumentAccessQuery = new SqlCommand( "InsertUserRecentDocument", dbConnection );
                cmdInsertUserDocumentAccessQuery.CommandType = CommandType.StoredProcedure;
                cmdInsertUserDocumentAccessQuery.CommandTimeout = 30;

                AddStandardParameter( cmdInsertUserDocumentAccessQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmDocumentType = new SqlParameter( "@DocumentType", SqlDbType.NChar, 1 );
                SqlParameter parmDocumentNumber = new SqlParameter( "@DocumentNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmDocumentId = new SqlParameter( "@DocumentId", SqlDbType.Int );

                parmDocumentType.Value = documentTypeString;
                parmDocumentNumber.Value = documentNumber;
                parmDocumentId.Value = documentId;

                cmdInsertUserDocumentAccessQuery.Parameters.Add( parmDocumentType );
                cmdInsertUserDocumentAccessQuery.Parameters.Add( parmDocumentNumber );
                cmdInsertUserDocumentAccessQuery.Parameters.Add( parmDocumentId );

                // connect
                dbConnection.Open();

                cmdInsertUserDocumentAccessQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.LogUserDocumentAccess(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string SchedulesTableName = "SchedulesTable";

        public bool SelectSchedulesForDivision( ref DataSet dsSchedulesForDivision, int divisionId )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSchedulesForDivision = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectSchedulesForDivision
                //(
                //@CurrentUser uniqueidentifier,
                //@COID int,
                //@LoginId nvarchar(120),
                //@DivisionId int
                //)
                SqlCommand cmdSelectScheduleForDivisionQuery = new SqlCommand( "SelectSchedulesForDivision", dbConnection );
                cmdSelectScheduleForDivisionQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectScheduleForDivisionQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                SqlParameter parmLoginId = new SqlParameter( "@LoginId", SqlDbType.NVarChar, 120 );
                SqlParameter parmDivisionId = new SqlParameter( "@DivisionId", SqlDbType.Int );

                parmCurrentUser.Value = _currentUserId;
                parmCOID.Value = _oldUserId;
                parmLoginId.Value = _userName;
                parmDivisionId.Value = divisionId;

                cmdSelectScheduleForDivisionQuery.Parameters.Add( parmCurrentUser );
                cmdSelectScheduleForDivisionQuery.Parameters.Add( parmCOID );
                cmdSelectScheduleForDivisionQuery.Parameters.Add( parmLoginId );
                cmdSelectScheduleForDivisionQuery.Parameters.Add( parmDivisionId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSchedulesForDivision = new SqlDataAdapter();
                daSchedulesForDivision.SelectCommand = cmdSelectScheduleForDivisionQuery;

                dsSchedulesForDivision = new DataSet( "Schedules" );
                DataTable dtSchedules = dsSchedulesForDivision.Tables.Add( SchedulesTableName );

                // add the common elements to the table
                DataColumn scheduleNumberColumn = new DataColumn( "Schedule_Number", typeof( int ) );

                dtSchedules.Columns.Add( scheduleNumberColumn );

                dtSchedules.Columns.Add( "Schedule_Name", typeof( string ) );
                dtSchedules.Columns.Add( "Short_Sched_Name", typeof( string ) );
                dtSchedules.Columns.Add( "Schedule_Manager", typeof( int ) );
                dtSchedules.Columns.Add( "Asst_Director", typeof( int ) );
                dtSchedules.Columns.Add( "Section", typeof( int ) );
                dtSchedules.Columns.Add( "Section_Description", typeof( string ) );
                dtSchedules.Columns.Add( "Director", typeof( int ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = scheduleNumberColumn;

                // add the keys to the table
                dtSchedules.PrimaryKey = primaryKeyColumns;

                dtSchedules.Clear();

                // connect
                dbConnection.Open();

                // run
                daSchedulesForDivision.Fill( dsSchedulesForDivision, "SchedulesTable" );

                RowsReturned = dsSchedulesForDivision.Tables[ "SchedulesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectSchedulesForDivision(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string ExtendableContractsTableName = "ExtendableContractsTable";

        public bool SelectExtendableContracts( ref DataSet dsExtendableContracts, int proposalTypeId, int scheduleNumber )     
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daExtendableContracts = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectExtendableContracts
                //(
                //@CurrentUser uniqueidentifier,
                //@COID int,
                //@LoginId nvarchar(120),
                //@ProposalTypeId int,
                //@ScheduleNumber int
                //)
                SqlCommand cmdSelectExtendableContractsQuery = new SqlCommand( "SelectExtendableContracts", dbConnection );
                cmdSelectExtendableContractsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectExtendableContractsQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                SqlParameter parmLoginId = new SqlParameter( "@LoginId", SqlDbType.NVarChar, 120 );
                SqlParameter parmProposalTypeId = new SqlParameter( "@ProposalTypeId", SqlDbType.Int );
                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );

                parmCurrentUser.Value = _currentUserId;
                parmCOID.Value = _oldUserId;
                parmLoginId.Value = _userName;
                parmProposalTypeId.Value = proposalTypeId;
                parmScheduleNumber.Value = scheduleNumber;

                cmdSelectExtendableContractsQuery.Parameters.Add( parmCurrentUser );
                cmdSelectExtendableContractsQuery.Parameters.Add( parmCOID );
                cmdSelectExtendableContractsQuery.Parameters.Add( parmLoginId );
                cmdSelectExtendableContractsQuery.Parameters.Add( parmProposalTypeId );
                cmdSelectExtendableContractsQuery.Parameters.Add( parmScheduleNumber );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daExtendableContracts = new SqlDataAdapter();
                daExtendableContracts.SelectCommand = cmdSelectExtendableContractsQuery;

                dsExtendableContracts = new DataSet( "ExtendableContracts" );
                DataTable dtExtendableContracts = dsExtendableContracts.Tables.Add( ExtendableContractsTableName );

                // add the common elements to the table
                DataColumn contractNumberColumn = new DataColumn( "ContractNumber", typeof( string ) );

                dtExtendableContracts.Columns.Add( contractNumberColumn );

                dtExtendableContracts.Columns.Add( "VendorName", typeof( string ) );                
                dtExtendableContracts.Columns.Add( "ExpirationDate", typeof( DateTime ) );
                dtExtendableContracts.Columns.Add( "CO_ID", typeof( int ) );
                dtExtendableContracts.Columns.Add( "FullName", typeof( string ) );                

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractNumberColumn;

                // add the keys to the table
                dtExtendableContracts.PrimaryKey = primaryKeyColumns;

                dtExtendableContracts.Clear();

                // connect
                dbConnection.Open();

                // run
                daExtendableContracts.Fill( dsExtendableContracts, ExtendableContractsTableName );

                RowsReturned = dsExtendableContracts.Tables[ ExtendableContractsTableName ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectExtendableContracts(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public const string ParentContractsTableName = "ParentContractsTable";

        public bool GetParentContractsForSchedule( int scheduleNumber, ref DataSet dsParentContracts )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daParentContractsForSchedule = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectParentContractsForSchedule
                //(
                //@CurrentUser uniqueidentifier,
                //@COID int,
                //@LoginId nvarchar(120),
                //@BPAScheduleNumber int
                //)
                SqlCommand cmdSelectParentContractsForScheduleQuery = new SqlCommand( "SelectParentContractsForSchedule", dbConnection );
                cmdSelectParentContractsForScheduleQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectParentContractsForScheduleQuery.CommandTimeout = 30;

                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                SqlParameter parmLoginId = new SqlParameter( "@LoginId", SqlDbType.NVarChar, 120 );
                SqlParameter parmBPAScheduleNumber = new SqlParameter( "@BPAScheduleNumber", SqlDbType.Int );

                parmCurrentUser.Value = _currentUserId;
                parmCOID.Value = _oldUserId;
                parmLoginId.Value = _userName;
                parmBPAScheduleNumber.Value = scheduleNumber;

                cmdSelectParentContractsForScheduleQuery.Parameters.Add( parmCurrentUser );
                cmdSelectParentContractsForScheduleQuery.Parameters.Add( parmCOID );
                cmdSelectParentContractsForScheduleQuery.Parameters.Add( parmLoginId );
                cmdSelectParentContractsForScheduleQuery.Parameters.Add( parmBPAScheduleNumber );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daParentContractsForSchedule = new SqlDataAdapter();
                daParentContractsForSchedule.SelectCommand = cmdSelectParentContractsForScheduleQuery;

                dsParentContracts = new DataSet( "ParentContracts" );
                DataTable dtParentContracts = dsParentContracts.Tables.Add( ParentContractsTableName );

                // add the common elements to the table
                DataColumn contractNumberColumn = new DataColumn( "CntrctNum", typeof( string ) );

                dtParentContracts.Columns.Add( contractNumberColumn );

                dtParentContracts.Columns.Add( "Schedule_Number", typeof( int ) );
                dtParentContracts.Columns.Add( "Schedule_Name", typeof( string ) );
                dtParentContracts.Columns.Add( "FullName", typeof( string ) );
                dtParentContracts.Columns.Add( "CO_ID", typeof( int ) );
                dtParentContracts.Columns.Add( "Contractor_Name", typeof( string ) );
                dtParentContracts.Columns.Add( "SAMUEI", typeof( string ) );
                dtParentContracts.Columns.Add( "DUNS", typeof( string ) );
                dtParentContracts.Columns.Add( "Drug_Covered", typeof( string ) );

                dtParentContracts.Columns.Add( "Dates_CntrctAward", typeof( DateTime ) );
                dtParentContracts.Columns.Add( "Dates_Effective", typeof( DateTime ) );
                dtParentContracts.Columns.Add( "Dates_CntrctExp", typeof( DateTime ) );
                dtParentContracts.Columns.Add( "Dates_Completion", typeof( DateTime ) );
             
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractNumberColumn;

                // add the keys to the table
                dtParentContracts.PrimaryKey = primaryKeyColumns;

                dtParentContracts.Clear();

                // connect
                dbConnection.Open();

                // run
                daParentContractsForSchedule.Fill( dsParentContracts, ParentContractsTableName );

                RowsReturned = dsParentContracts.Tables[ ParentContractsTableName ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetParentContractsForSchedule(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string DivisionsTableName = "DivisionsTable";

        public bool SelectDivisions( ref DataSet dsDivisions )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daDivisions = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectDivisions
                //(
                //@CurrentUser uniqueidentifier,
                //@COID int,
                //@LoginId nvarchar(120),
                //)
                SqlCommand cmdSelectDivisions = new SqlCommand( "SelectDivisions", dbConnection );
                cmdSelectDivisions.CommandType = CommandType.StoredProcedure;
                cmdSelectDivisions.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmCOID = new SqlParameter( "@COID", SqlDbType.Int );
                SqlParameter parmLoginId = new SqlParameter( "@LoginId", SqlDbType.NVarChar, 120 );

                parmCurrentUser.Value = _currentUserId;
                parmCOID.Value = _oldUserId;
                parmLoginId.Value = _userName;

                cmdSelectDivisions.Parameters.Add( parmCurrentUser );
                cmdSelectDivisions.Parameters.Add( parmCOID );
                cmdSelectDivisions.Parameters.Add( parmLoginId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daDivisions = new SqlDataAdapter();
                daDivisions.SelectCommand = cmdSelectDivisions;

                dsDivisions = new DataSet( "Divisions" );
                DataTable dtDivisions = dsDivisions.Tables.Add( DivisionsTableName );

                // add the common elements to the table
                DataColumn divisionIdColumn = new DataColumn( "DivisionId", typeof( int ) );

                dtDivisions.Columns.Add( divisionIdColumn );

                dtDivisions.Columns.Add( "DivisionName", typeof( string ) );
                dtDivisions.Columns.Add( "Director", typeof( int ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = divisionIdColumn;

                // add the keys to the table
                dtDivisions.PrimaryKey = primaryKeyColumns;

                dtDivisions.Clear();

                // connect
                dbConnection.Open();

                // run
                daDivisions.Fill( dsDivisions, "DivisionsTable" );

                RowsReturned = dsDivisions.Tables[ "DivisionsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectDivisions(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string StateCodesTableName = "StateCodesTable";

        public bool SelectStateCodes( ref DataSet dsStateCodes, int countryId )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daStateCodes = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectStateCodes
                //(
                //@CurrentUser uniqueidentifier,
                //@CountryId int
                //)
                SqlCommand cmdSelectStateCodes = new SqlCommand( "SelectStateCodes", dbConnection );
                cmdSelectStateCodes.CommandType = CommandType.StoredProcedure;
                cmdSelectStateCodes.CommandTimeout = 30;

                AddStandardParameter( cmdSelectStateCodes, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmCountryId = new SqlParameter( "@CountryId", SqlDbType.Int );

                parmCountryId.Value = countryId;

                cmdSelectStateCodes.Parameters.Add( parmCountryId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daStateCodes = new SqlDataAdapter();
                daStateCodes.SelectCommand = cmdSelectStateCodes;

                dsStateCodes = new DataSet( "StateCodes" );
                DataTable dtStateCodes = dsStateCodes.Tables.Add( StateCodesTableName );

                // add the common elements to the table
                DataColumn stateAbbreviationColumn = new DataColumn( "StateAbbreviation", typeof( string ) );

                dtStateCodes.Columns.Add( stateAbbreviationColumn );

                dtStateCodes.Columns.Add( "StateName", typeof( string ) );
                dtStateCodes.Columns.Add( "Country", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = stateAbbreviationColumn;

                // add the keys to the table
                dtStateCodes.PrimaryKey = primaryKeyColumns;

                dtStateCodes.Clear();

                // connect
                dbConnection.Open();

                // run
                daStateCodes.Fill( dsStateCodes, StateCodesTableName );

                RowsReturned = dsStateCodes.Tables[ StateCodesTableName ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectStateCodes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string VendorCountriesTableName = "VendorCountriesTable";

        // select countries for the vendor address
        public bool SelectCountries( ref DataSet dsCountries )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daCountries = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectCountries
                //(
                //@CurrentUser uniqueidentifier
                //@CountryListType char(1)  -- 'V' = vendor addresses;  'I' = item countries of origin
                //)
                SqlCommand cmdSelectCountries = new SqlCommand( "SelectCountries", dbConnection );
                cmdSelectCountries.CommandType = CommandType.StoredProcedure;
                cmdSelectCountries.CommandTimeout = 30;

                AddStandardParameter( cmdSelectCountries, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmCountryListType = new SqlParameter( "@CountryListType", SqlDbType.Char, 1 );

                parmCountryListType.Value = "V";

                cmdSelectCountries.Parameters.Add( parmCountryListType );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daCountries = new SqlDataAdapter();
                daCountries.SelectCommand = cmdSelectCountries;

                dsCountries = new DataSet( "Countries" );
                DataTable dtCountries = dsCountries.Tables.Add( VendorCountriesTableName );

                // add the common elements to the table
                DataColumn countryIdColumn = new DataColumn( "CountryId", typeof( int ) );
                DataColumn countryNameColumn = new DataColumn( "CountryName", typeof( string ) );

                dtCountries.Columns.Add( countryIdColumn );
                dtCountries.Columns.Add( countryNameColumn );
              
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = countryIdColumn;

                // add the keys to the table
                dtCountries.PrimaryKey = primaryKeyColumns;

                dtCountries.Clear();

                // connect
                dbConnection.Open();

                // run
                daCountries.Fill( dsCountries, VendorCountriesTableName );

                RowsReturned = dsCountries.Tables[ VendorCountriesTableName ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectCountries(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool SelectSBAPlanTypes( ref DataSet dsPlanTypes )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daPlanTypes = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectSBAPlanTypes
                //(
                //@CurrentUser uniqueidentifier
                //)
                SqlCommand cmdSelectPlanTypes = new SqlCommand( "SelectSBAPlanTypes", dbConnection );
                cmdSelectPlanTypes.CommandType = CommandType.StoredProcedure;
                cmdSelectPlanTypes.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );

                parmCurrentUser.Value = _currentUserId;

                cmdSelectPlanTypes.Parameters.Add( parmCurrentUser );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daPlanTypes = new SqlDataAdapter();
                daPlanTypes.SelectCommand = cmdSelectPlanTypes;

                dsPlanTypes = new DataSet( "SBAPlanTypes" );
                DataTable dtPlanTypes = dsPlanTypes.Tables.Add( "SBAPlanTypesTable" );

                // add the common elements to the table
                DataColumn planTypeIdColumn = new DataColumn( "PlanTypeId", typeof( int ) );

                dtPlanTypes.Columns.Add( planTypeIdColumn );
                dtPlanTypes.Columns.Add( "PlanTypeDescription", typeof( string ) );
         
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = planTypeIdColumn;

                // add the keys to the table
                dtPlanTypes.PrimaryKey = primaryKeyColumns;

                dtPlanTypes.Clear();

                // connect
                dbConnection.Open();

                // run
                daPlanTypes.Fill( dsPlanTypes, "SBAPlanTypesTable" );

                RowsReturned = dsPlanTypes.Tables[ "SBAPlanTypesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectSBAPlanTypes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool CreateContract( int scheduleNumber, string manuallyAssignedContractNumber, DateTime awardDate, DateTime effectiveDate, DateTime expirationDate, int optionYears, int assignedCOID, 
           string vendorName, string vendorContactName, string vendorContactPhone, string vendorContactPhoneExtension, string vendorContactFax, string vendorContactEmail, 
            string vendorAddress1, string vendorAddress2, string vendorAddressCity, string vendorAddressState, string vendorAddressZipCode, string vendorUrl, int sourceOfferId, bool bIsRebateRequired, ref int newContractId, ref int newPharmaceuticalContractId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            newContractId = -1;
            int geographicCoverageId = -1; // output parameter new for v2, not passed back in current release

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //CreateContract
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@DrugItemServerName nvarchar(255),
                //@DrugItemDatabaseName nvarchar(255),
                //@ScheduleNumber int,
                //@ManuallyAssignedContractNumber nvarchar(50),
                //@AwardDate datetime,
                //@EffectiveDate datetime,
                //@ExpirationDate datetime,
                //@AssignedCOID int,
                //@VendorName nvarchar(75),
                //@OptionYears int = null,
                //@VendorContactName  nvarchar(30) = null,
                //@VendorContactPhone nvarchar(15) = null,
                //@VendorContactPhoneExtension  nvarchar(5) = null,
                //@VendorContactFax   nvarchar(15) = null,
                //@VendorContactEmail    nvarchar(50) = null,
                //@VendorAddress1    nvarchar(100) = null,
                //@VendorAddress2    nvarchar(100) = null,
                //@VendorAddressCity   nvarchar(20) = null,
                //@VendorAddressState    nvarchar(2) = null,
                //@VendorZipCode   nvarchar(10) = null,
                //@VendorUrl     nvarchar(50) = null,
                //@OfferId int = null, -- available when created from offer record,
                //@IsRebateRequired bit = null,
                //@NewContractId int OUTPUT,
                //@NewPharmaceuticalContractId int OUTPUT,
                //@GeographicCoverageId int OUTPUT
                //)

                SqlCommand cmdCreateContractQuery = new SqlCommand( "CreateContract", dbConnection );
                cmdCreateContractQuery.CommandType = CommandType.StoredProcedure;
                cmdCreateContractQuery.CommandTimeout = 30;

                AddStandardParameter( cmdCreateContractQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdCreateContractQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );
                AddStandardParameter( cmdCreateContractQuery, StandardParameterTypes.DrugItemDatabase, Config.DrugItemDatabaseServer, Config.DrugItemDatabase );

              //  SqlParameter parmUserId = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                SqlParameter parmManuallyAssignedContractNumber = new SqlParameter( "@ManuallyAssignedContractNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmAwardDate = new SqlParameter( "@AwardDate", SqlDbType.DateTime );
                SqlParameter parmEffectiveDate = new SqlParameter( "@EffectiveDate", SqlDbType.DateTime );
                SqlParameter parmExpirationDate = new SqlParameter( "@ExpirationDate", SqlDbType.DateTime );
                SqlParameter parmAssignedCOID = new SqlParameter( "@AssignedCOID", SqlDbType.Int );
                SqlParameter parmVendorName = new SqlParameter( "@VendorName", SqlDbType.NVarChar, 75 );
                SqlParameter parmOptionYears = new SqlParameter( "@OptionYears", SqlDbType.Int );
                SqlParameter parmVendorContactName = new SqlParameter( "@VendorContactName", SqlDbType.NVarChar, 30 );
                SqlParameter parmVendorContactPhone = new SqlParameter( "@VendorContactPhone", SqlDbType.NVarChar, 15 );
                SqlParameter parmVendorContactPhoneExtension = new SqlParameter( "@VendorContactPhoneExtension", SqlDbType.NVarChar, 5 );
                SqlParameter parmVendorContactFax = new SqlParameter( "@VendorContactFax", SqlDbType.NVarChar, 15 );
                SqlParameter parmVendorContactEmail = new SqlParameter( "@VendorContactEmail", SqlDbType.NVarChar, 50 );
                SqlParameter parmVendorAddress1 = new SqlParameter( "@VendorAddress1", SqlDbType.NVarChar, 100 );
                SqlParameter parmVendorAddress2 = new SqlParameter( "@VendorAddress2", SqlDbType.NVarChar, 100 );
                SqlParameter parmVendorAddressCity = new SqlParameter( "@VendorAddressCity", SqlDbType.NVarChar, 20 );
                SqlParameter parmVendorAddressState = new SqlParameter( "@VendorAddressState", SqlDbType.NVarChar, 2 );
                SqlParameter parmVendorZipCode = new SqlParameter( "@VendorZipCode", SqlDbType.NVarChar, 10 );
                SqlParameter parmVendorUrl = new SqlParameter( "@VendorUrl", SqlDbType.NVarChar, 50 );
                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );
                SqlParameter parmIsRebateRequired = new SqlParameter( "@IsRebateRequired", SqlDbType.Bit );

                SqlParameter parmNewContractId = new SqlParameter( "@NewContractId", SqlDbType.Int );
                parmNewContractId.Direction = ParameterDirection.Output;

                SqlParameter parmNewPharmaceuticalContractId = new SqlParameter( "@NewPharmaceuticalContractId", SqlDbType.Int );
                parmNewPharmaceuticalContractId.Direction = ParameterDirection.Output;

                SqlParameter parmGeographicCoverageId = new SqlParameter( "@GeographicCoverageId", SqlDbType.Int );
                parmGeographicCoverageId.Direction = ParameterDirection.Output;

      //          parmUserId.Value = _currentUserId;
                parmScheduleNumber.Value = scheduleNumber;
                parmManuallyAssignedContractNumber.Value = manuallyAssignedContractNumber;
                parmAwardDate.Value = awardDate;
                parmEffectiveDate.Value = effectiveDate;
                parmExpirationDate.Value = expirationDate;
                parmAssignedCOID.Value = assignedCOID;
                parmVendorName.Value = vendorName;
                parmOptionYears.Value = optionYears;
                parmVendorContactName.Value = vendorContactName;
                parmVendorContactPhone.Value = vendorContactPhone;
                parmVendorContactPhoneExtension.Value = vendorContactPhoneExtension;
                parmVendorContactFax.Value = vendorContactFax;
                parmVendorContactEmail.Value = vendorContactEmail;
                parmVendorAddress1.Value = vendorAddress1;
                parmVendorAddress2.Value = vendorAddress2;
                parmVendorAddressCity.Value = vendorAddressCity;
                parmVendorAddressState.Value = vendorAddressState;
                parmVendorZipCode.Value = vendorAddressZipCode;
                parmVendorUrl.Value = vendorUrl;
                parmOfferId.Value = sourceOfferId;
                parmIsRebateRequired.Value = bIsRebateRequired;

      //          cmdCreateContractQuery.Parameters.Add( parmUserId );
                cmdCreateContractQuery.Parameters.Add( parmScheduleNumber );
                cmdCreateContractQuery.Parameters.Add( parmManuallyAssignedContractNumber );
                cmdCreateContractQuery.Parameters.Add( parmAwardDate );
                cmdCreateContractQuery.Parameters.Add( parmEffectiveDate );
                cmdCreateContractQuery.Parameters.Add( parmExpirationDate );
                cmdCreateContractQuery.Parameters.Add( parmAssignedCOID );
                cmdCreateContractQuery.Parameters.Add( parmVendorName );
                cmdCreateContractQuery.Parameters.Add( parmOptionYears );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactName );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactPhone );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactPhoneExtension );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactFax );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactEmail );
                cmdCreateContractQuery.Parameters.Add( parmVendorAddress1 );
                cmdCreateContractQuery.Parameters.Add( parmVendorAddress2 );
                cmdCreateContractQuery.Parameters.Add( parmVendorAddressCity );
                cmdCreateContractQuery.Parameters.Add( parmVendorAddressState );
                cmdCreateContractQuery.Parameters.Add( parmVendorZipCode );
                cmdCreateContractQuery.Parameters.Add( parmVendorUrl );
                cmdCreateContractQuery.Parameters.Add( parmOfferId );
                cmdCreateContractQuery.Parameters.Add( parmIsRebateRequired ); 
                cmdCreateContractQuery.Parameters.Add( parmNewContractId );
                cmdCreateContractQuery.Parameters.Add( parmNewPharmaceuticalContractId );
                cmdCreateContractQuery.Parameters.Add( parmGeographicCoverageId );

                // connect
                dbConnection.Open();

                cmdCreateContractQuery.ExecuteNonQuery();

                newContractId = int.Parse( cmdCreateContractQuery.Parameters[ "@NewContractId" ].Value.ToString() );
                newPharmaceuticalContractId = int.Parse( cmdCreateContractQuery.Parameters[ "@NewPharmaceuticalContractId" ].Value.ToString() );
                geographicCoverageId = int.Parse( cmdCreateContractQuery.Parameters[ "@GeographicCoverageId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.CreateContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // branched this version for release 2 -- added parentContractNumber now collected for BPAs at creation    
        public bool CreateContract2( int scheduleNumber, string manuallyAssignedContractNumber, DateTime awardDate, DateTime effectiveDate, DateTime expirationDate, int optionYears, string parentContractNumber, int assignedCOID, 
           string vendorName, string vendorContactName, string vendorContactPhone, string vendorContactPhoneExtension, string vendorContactFax, string vendorContactEmail, 
            string vendorAddress1, string vendorAddress2, string vendorAddressCity, string vendorAddressState, string vendorAddressZipCode, int vendorCountryId, string vendorUrl, int sourceOfferId, bool bIsRebateRequired, int socioBusinessSizeId, string solicitationNumber, ref int newContractId, ref int newPharmaceuticalContractId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            newContractId = -1;
            int geographicCoverageId = -1; // output parameter new for v2, not passed back in current release
            int SAMVendorInfoId = -1;   // output parameter not passed back

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                // note that the default SIN for National and BPA contracts will be created
                //CreateContract2
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@DrugItemServerName nvarchar(255),
                //@DrugItemDatabaseName nvarchar(255),
                //@ScheduleNumber int,
                //@ManuallyAssignedContractNumber nvarchar(50),
                //@AwardDate datetime,
                //@EffectiveDate datetime,
                //@ExpirationDate datetime,
                //@AssignedCOID int,
                //@VendorName nvarchar(75),
                //@OptionYears int = null,
                //@ParentContractNumber nvarchar(50) = null,
                //@VendorContactName  nvarchar(30) = null,
                //@VendorContactPhone nvarchar(15) = null,
                //@VendorContactPhoneExtension  nvarchar(5) = null,
                //@VendorContactFax   nvarchar(15) = null,
                //@VendorContactEmail    nvarchar(50) = null,
                //@VendorAddress1    nvarchar(100) = null,
                //@VendorAddress2    nvarchar(100) = null,
                //@VendorAddressCity   nvarchar(20) = null,
                //@VendorAddressState    nvarchar(2) = null,
                //@VendorZipCode   nvarchar(10) = null,
                //@VendorCountryId int = null,
                //@VendorUrl     nvarchar(50) = null,
                //@OfferId int = null, -- available when created from offer record,
                //@IsRebateRequired bit = null,
                //@SocioBusinessSizeId int = null,
                //@SolicitationNumber nvarchar(40) = null,
                //@NewContractId int OUTPUT,
                //@NewPharmaceuticalContractId int OUTPUT,
                //@GeographicCoverageId int OUTPUT,
                //@SAMVendorInfoId int OUTPUT
                //)

                SqlCommand cmdCreateContractQuery = new SqlCommand( "CreateContract2", dbConnection );
                cmdCreateContractQuery.CommandType = CommandType.StoredProcedure;
                cmdCreateContractQuery.CommandTimeout = 30;

                AddStandardParameter( cmdCreateContractQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdCreateContractQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );
                AddStandardParameter( cmdCreateContractQuery, StandardParameterTypes.DrugItemDatabase, Config.DrugItemDatabaseServer, Config.DrugItemDatabase );

              //  SqlParameter parmUserId = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                SqlParameter parmManuallyAssignedContractNumber = new SqlParameter( "@ManuallyAssignedContractNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmAwardDate = new SqlParameter( "@AwardDate", SqlDbType.DateTime );
                SqlParameter parmEffectiveDate = new SqlParameter( "@EffectiveDate", SqlDbType.DateTime );
                SqlParameter parmExpirationDate = new SqlParameter( "@ExpirationDate", SqlDbType.DateTime );
                SqlParameter parmAssignedCOID = new SqlParameter( "@AssignedCOID", SqlDbType.Int );
                SqlParameter parmVendorName = new SqlParameter( "@VendorName", SqlDbType.NVarChar, 75 );
                SqlParameter parmOptionYears = new SqlParameter( "@OptionYears", SqlDbType.Int );
                SqlParameter parmParentContractNumber = new SqlParameter( "@ParentContractNumber", SqlDbType.NVarChar, 50 );
                parmParentContractNumber.IsNullable = true;

                SqlParameter parmVendorContactName = new SqlParameter( "@VendorContactName", SqlDbType.NVarChar, 30 );
                SqlParameter parmVendorContactPhone = new SqlParameter( "@VendorContactPhone", SqlDbType.NVarChar, 15 );
                SqlParameter parmVendorContactPhoneExtension = new SqlParameter( "@VendorContactPhoneExtension", SqlDbType.NVarChar, 5 );
                SqlParameter parmVendorContactFax = new SqlParameter( "@VendorContactFax", SqlDbType.NVarChar, 15 );
                SqlParameter parmVendorContactEmail = new SqlParameter( "@VendorContactEmail", SqlDbType.NVarChar, 50 );
                SqlParameter parmVendorAddress1 = new SqlParameter( "@VendorAddress1", SqlDbType.NVarChar, 100 );
                SqlParameter parmVendorAddress2 = new SqlParameter( "@VendorAddress2", SqlDbType.NVarChar, 100 );
                SqlParameter parmVendorAddressCity = new SqlParameter( "@VendorAddressCity", SqlDbType.NVarChar, 20 );
                SqlParameter parmVendorAddressState = new SqlParameter( "@VendorAddressState", SqlDbType.NVarChar, 2 );
                SqlParameter parmVendorZipCode = new SqlParameter( "@VendorZipCode", SqlDbType.NVarChar, 10 );
                SqlParameter parmVendorCountryId = new SqlParameter( "@VendorCountryId", SqlDbType.Int );
                SqlParameter parmVendorUrl = new SqlParameter( "@VendorUrl", SqlDbType.NVarChar, 50 );
                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );
                SqlParameter parmIsRebateRequired = new SqlParameter( "@IsRebateRequired", SqlDbType.Bit );
                SqlParameter parmSocioBusinessSizeId = new SqlParameter( "@SocioBusinessSizeId", SqlDbType.Int );
                SqlParameter parmSolicitationNumber = new SqlParameter( "@SolicitationNumber", SqlDbType.NVarChar, 40 );

                SqlParameter parmNewContractId = new SqlParameter( "@NewContractId", SqlDbType.Int );
                parmNewContractId.Direction = ParameterDirection.Output;

                SqlParameter parmNewPharmaceuticalContractId = new SqlParameter( "@NewPharmaceuticalContractId", SqlDbType.Int );
                parmNewPharmaceuticalContractId.Direction = ParameterDirection.Output;

                SqlParameter parmGeographicCoverageId = new SqlParameter( "@GeographicCoverageId", SqlDbType.Int );
                parmGeographicCoverageId.Direction = ParameterDirection.Output;

                SqlParameter parmSAMVendorInfoId = new SqlParameter( "@SAMVendorInfoId", SqlDbType.Int );
                parmSAMVendorInfoId.Direction = ParameterDirection.Output;

                //          parmUserId.Value = _currentUserId;
                parmScheduleNumber.Value = scheduleNumber;
                parmManuallyAssignedContractNumber.Value = manuallyAssignedContractNumber;
                parmAwardDate.Value = awardDate;
                parmEffectiveDate.Value = effectiveDate;
                parmExpirationDate.Value = expirationDate;
                parmAssignedCOID.Value = assignedCOID;
                parmVendorName.Value = vendorName;
                parmOptionYears.Value = optionYears;

                if( parentContractNumber.Trim().Length == 0 )
                {
                    parmParentContractNumber.SqlValue = DBNull.Value;
                }
                else
                {
                parmParentContractNumber.Value = parentContractNumber;
                }

                parmVendorContactName.Value = vendorContactName;
                parmVendorContactPhone.Value = vendorContactPhone;
                parmVendorContactPhoneExtension.Value = vendorContactPhoneExtension;
                parmVendorContactFax.Value = vendorContactFax;
                parmVendorContactEmail.Value = vendorContactEmail;
                parmVendorAddress1.Value = vendorAddress1;
                parmVendorAddress2.Value = vendorAddress2;
                parmVendorAddressCity.Value = vendorAddressCity;
                parmVendorAddressState.Value = vendorAddressState;
                parmVendorZipCode.Value = vendorAddressZipCode;
                parmVendorCountryId.Value = vendorCountryId;
                parmVendorUrl.Value = vendorUrl;
                parmOfferId.Value = sourceOfferId;
                parmIsRebateRequired.Value = bIsRebateRequired;
                parmSocioBusinessSizeId.Value = socioBusinessSizeId;
                parmSolicitationNumber.Value = solicitationNumber;

      //          cmdCreateContractQuery.Parameters.Add( parmUserId );
                cmdCreateContractQuery.Parameters.Add( parmScheduleNumber );
                cmdCreateContractQuery.Parameters.Add( parmManuallyAssignedContractNumber );
                cmdCreateContractQuery.Parameters.Add( parmAwardDate );
                cmdCreateContractQuery.Parameters.Add( parmEffectiveDate );
                cmdCreateContractQuery.Parameters.Add( parmExpirationDate );
                cmdCreateContractQuery.Parameters.Add( parmAssignedCOID );
                cmdCreateContractQuery.Parameters.Add( parmVendorName );
                cmdCreateContractQuery.Parameters.Add( parmOptionYears );
                cmdCreateContractQuery.Parameters.Add( parmParentContractNumber );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactName );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactPhone );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactPhoneExtension );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactFax );
                cmdCreateContractQuery.Parameters.Add( parmVendorContactEmail );
                cmdCreateContractQuery.Parameters.Add( parmVendorAddress1 );
                cmdCreateContractQuery.Parameters.Add( parmVendorAddress2 );
                cmdCreateContractQuery.Parameters.Add( parmVendorAddressCity );
                cmdCreateContractQuery.Parameters.Add( parmVendorAddressState );
                cmdCreateContractQuery.Parameters.Add( parmVendorZipCode );
                cmdCreateContractQuery.Parameters.Add( parmVendorCountryId );
                cmdCreateContractQuery.Parameters.Add( parmVendorUrl );
                cmdCreateContractQuery.Parameters.Add( parmOfferId );
                cmdCreateContractQuery.Parameters.Add( parmIsRebateRequired );
                cmdCreateContractQuery.Parameters.Add( parmSocioBusinessSizeId );
                cmdCreateContractQuery.Parameters.Add( parmSolicitationNumber );
                cmdCreateContractQuery.Parameters.Add( parmNewContractId );
                cmdCreateContractQuery.Parameters.Add( parmNewPharmaceuticalContractId );
                cmdCreateContractQuery.Parameters.Add( parmGeographicCoverageId );
                cmdCreateContractQuery.Parameters.Add( parmSAMVendorInfoId );

                // connect
                dbConnection.Open();

                cmdCreateContractQuery.ExecuteNonQuery();

                newContractId = int.Parse( cmdCreateContractQuery.Parameters[ "@NewContractId" ].Value.ToString() );
                newPharmaceuticalContractId = int.Parse( cmdCreateContractQuery.Parameters[ "@NewPharmaceuticalContractId" ].Value.ToString() );
                geographicCoverageId = int.Parse( cmdCreateContractQuery.Parameters[ "@GeographicCoverageId" ].Value.ToString() );
                SAMVendorInfoId = int.Parse( cmdCreateContractQuery.Parameters[ "@SAMVendorInfoId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.CreateContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool ValidateContractNumber( string contractNumber, int scheduleNumber, ref bool bIsValidated, ref string validationMessage )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //ValidateNewContractNumber
                //(
                //@ContractNumber nvarchar(50),
                //@ScheduleNumber int,
                //@IsValidated bit OUTPUT,
                //@ValidationMessage nvarchar(300) OUTPUT

                SqlCommand cmdValidateNewContractNumberQuery = new SqlCommand( "ValidateNewContractNumber", dbConnection );
                cmdValidateNewContractNumberQuery.CommandType = CommandType.StoredProcedure;
                cmdValidateNewContractNumberQuery.CommandTimeout = 30;


        //        SqlParameter parmUserId = new SqlParameter( "@UserId", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                SqlParameter parmIsValidated = new SqlParameter( "@IsValidated", SqlDbType.Bit );
                parmIsValidated.Direction = ParameterDirection.Output;
                SqlParameter parmValidationMessage = new SqlParameter( "@ValidationMessage", SqlDbType.NVarChar, 300 );
                parmValidationMessage.Direction = ParameterDirection.Output;

       //         parmUserId.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;
                parmScheduleNumber.Value = scheduleNumber;

       //         cmdValidateNewContractNumberQuery.Parameters.Add( parmUserId );
                cmdValidateNewContractNumberQuery.Parameters.Add( parmContractNumber );
                cmdValidateNewContractNumberQuery.Parameters.Add( parmScheduleNumber );
                cmdValidateNewContractNumberQuery.Parameters.Add( parmIsValidated );
                cmdValidateNewContractNumberQuery.Parameters.Add( parmValidationMessage );

                // connect
                dbConnection.Open();

                cmdValidateNewContractNumberQuery.ExecuteNonQuery();

                bIsValidated = bool.Parse( cmdValidateNewContractNumberQuery.Parameters[ "@IsValidated" ].Value.ToString() );
                validationMessage = cmdValidateNewContractNumberQuery.Parameters[ "@ValidationMessage" ].Value.ToString();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.ValidateContractNumber(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool GetNewContractPrefix( int scheduleNumber, ref string prefix )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            prefix = "";

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetNewContractPrefix
                //(
                //@ScheduleNumber int,
                //@Prefix nvarchar(50) OUTPUT
                //)

                SqlCommand cmdGetNewContractPrefixQuery = new SqlCommand( "GetNewContractPrefix", dbConnection );
                cmdGetNewContractPrefixQuery.CommandType = CommandType.StoredProcedure;
                cmdGetNewContractPrefixQuery.CommandTimeout = 30;


                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                SqlParameter parmPrefix = new SqlParameter( "@Prefix", SqlDbType.NVarChar, 50 );
                parmPrefix.Direction = ParameterDirection.Output;

                parmScheduleNumber.Value = scheduleNumber;

                cmdGetNewContractPrefixQuery.Parameters.Add( parmScheduleNumber );
                cmdGetNewContractPrefixQuery.Parameters.Add( parmPrefix );

                // connect
                dbConnection.Open();

                cmdGetNewContractPrefixQuery.ExecuteNonQuery();

                prefix = cmdGetNewContractPrefixQuery.Parameters[ "@Prefix" ].Value.ToString();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetNewContractPrefix(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetContractResponsibleForSBAPlan( int sbaPlanId, ref string contractNumberResponsibleForSBAPlan, ref int COID, ref string COName )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetContractResponsibleForSBAPlan
                //(
                //@SBAPlanId int,
                //@ContractNumber nvarchar(50) OUTPUT,
                //@COID int OUTPUT,
                //@COName nvarchar(50) OUTPUT
                //)

                SqlCommand cmdGetContractResponsibleForSBAPlanQuery = new SqlCommand( "GetContractResponsibleForSBAPlan", dbConnection );
                cmdGetContractResponsibleForSBAPlanQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContractResponsibleForSBAPlanQuery.CommandTimeout = 30;


                SqlParameter parmSBAPlanId = new SqlParameter( "@SBAPlanId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 50 );
                parmContractNumber.Direction = ParameterDirection.Output;

                SqlParameter parmCOId = new SqlParameter( "@COID", SqlDbType.Int );
                parmCOId.Direction = ParameterDirection.Output;

                SqlParameter parmCOName = new SqlParameter( "@COName", SqlDbType.NVarChar, 50 );
                parmCOName.Direction = ParameterDirection.Output;

                parmSBAPlanId.Value = sbaPlanId;

                cmdGetContractResponsibleForSBAPlanQuery.Parameters.Add( parmSBAPlanId );
                cmdGetContractResponsibleForSBAPlanQuery.Parameters.Add( parmContractNumber );
                cmdGetContractResponsibleForSBAPlanQuery.Parameters.Add( parmCOId );
                cmdGetContractResponsibleForSBAPlanQuery.Parameters.Add( parmCOName );

                // connect
                dbConnection.Open();

                cmdGetContractResponsibleForSBAPlanQuery.ExecuteNonQuery();

                contractNumberResponsibleForSBAPlan = cmdGetContractResponsibleForSBAPlanQuery.Parameters[ "@ContractNumber" ].Value.ToString();
                COID = int.Parse( cmdGetContractResponsibleForSBAPlanQuery.Parameters[ "@COID" ].Value.ToString() );
                COName = cmdGetContractResponsibleForSBAPlanQuery.Parameters[ "@COName" ].Value.ToString();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetContractResponsibleForSBAPlan(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string StandardRebateTermsTableName = "StandardRebateTermsTable";

        public bool SelectStandardRebateTerms( ref DataSet dsStandardRebateTerms, string clauseType )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daStandardRebateTerms = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectStandardRebateTerms
                //(
                //@UserLogin nvarchar(120),
                //@UserId uniqueidentifier,
                //@ClauseType nchar(1)   -- 'A' = All

                SqlCommand cmdSelectStandardRebateTermsQuery = new SqlCommand( "SelectStandardRebateTerms", dbConnection );
                cmdSelectStandardRebateTermsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectStandardRebateTermsQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectStandardRebateTermsQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmLoginId = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                SqlParameter parmClauseType = new SqlParameter( "@ClauseType", SqlDbType.NChar, 1 );

                parmLoginId.Value = _userName;
                parmClauseType.Value = clauseType;

                cmdSelectStandardRebateTermsQuery.Parameters.Add( parmLoginId );
                cmdSelectStandardRebateTermsQuery.Parameters.Add( parmClauseType );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daStandardRebateTerms = new SqlDataAdapter();
                daStandardRebateTerms.SelectCommand = cmdSelectStandardRebateTermsQuery;

                dsStandardRebateTerms = new DataSet( "StandardRebateTerms" );
                DataTable dtStandardRebateTerms = dsStandardRebateTerms.Tables.Add( "StandardRebateTermsTable" );

                // add the common elements to the table
                DataColumn standardRebateTermIdColumn = new DataColumn( "StandardRebateTermId", typeof( int ) );

                dtStandardRebateTerms.Columns.Add( standardRebateTermIdColumn );

                dtStandardRebateTerms.Columns.Add( "RebateClause", typeof( string ) );
                dtStandardRebateTerms.Columns.Add( "ClauseType", typeof( string ) );
                dtStandardRebateTerms.Columns.Add( "IsActive", typeof( int ) );

                dtStandardRebateTerms.Columns.Add( "CreatedBy", typeof( string ) );
                dtStandardRebateTerms.Columns.Add( "CreationDate", typeof( DateTime ) );
 
                dtStandardRebateTerms.Columns.Add( "LastModifiedBy", typeof( string ) );
                dtStandardRebateTerms.Columns.Add( "LastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = standardRebateTermIdColumn;

                // add the keys to the table
                dtStandardRebateTerms.PrimaryKey = primaryKeyColumns;

                dtStandardRebateTerms.Clear();

                // connect
                dbConnection.Open();

                // run
                daStandardRebateTerms.Fill( dsStandardRebateTerms, "StandardRebateTermsTable" );

                RowsReturned = dsStandardRebateTerms.Tables[ "StandardRebateTermsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectStandardRebateTerms(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool CreateContractRebate( string contractNumber, int startQuarterId, int endQuarterId, DateTime customStartDate, double rebatePercentOfSales, double rebateThreshold, 
            double amountReceived, bool bIsCustom, string rebateClause, int standardRebateTermId, ref int rebateId, ref int  customRebateId, ref int rebateTermId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            rebateId = -1;
            customRebateId = -1;
            rebateTermId = -1;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //InsertContractRebate
                //(
                //@UserLogin nvarchar(120),
                //@UserId uniqueidentifier,

                //@ContractNumber nvarchar(20),
                //@StartQuarterId int, 
                //@EndQuarterId int, 
                //@RebatePercentOfSales numeric(8,3), 
                //@RebateThreshold money, 
                //@AmountReceived money, 
                //@IsCustom bit,
                //@RebateClause nvarchar(4000),

                //@StandardRebateTermId int,
                //@CustomStartDate datetime = null,
                //@RebateId int OUTPUT,
                //@CustomRebateId int OUTPUT,
                //@RebateTermId int OUTPUT

                SqlCommand cmdCreateContractRebateQuery = new SqlCommand( "InsertContractRebate", dbConnection );
                cmdCreateContractRebateQuery.CommandType = CommandType.StoredProcedure;
                cmdCreateContractRebateQuery.CommandTimeout = 30;

                SqlParameter parmLoginId = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                AddStandardParameter( cmdCreateContractRebateQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmStartQuarterId = new SqlParameter( "@StartQuarterId", SqlDbType.Int );
                SqlParameter parmEndQuarterId = new SqlParameter( "@EndQuarterId", SqlDbType.Int );
                SqlParameter parmRebatePercentOfSales = new SqlParameter( "@RebatePercentOfSales", SqlDbType.Decimal, 12 );
                SqlParameter parmRebateThreshold = new SqlParameter( "@RebateThreshold", SqlDbType.Money );
                SqlParameter parmAmountReceived = new SqlParameter( "@AmountReceived", SqlDbType.Money );
                SqlParameter parmIsCustom = new SqlParameter( "@IsCustom", SqlDbType.Bit );
                SqlParameter parmRebateClause = new SqlParameter( "@RebateClause", SqlDbType.NVarChar, 4000 );
                SqlParameter parmStandardRebateTermId = new SqlParameter( "@StandardRebateTermId", SqlDbType.Int );
                SqlParameter parmCustomStartDate = new SqlParameter( "@CustomStartDate", SqlDbType.DateTime );
                parmCustomStartDate.IsNullable = true;

                SqlParameter parmRebateId = new SqlParameter( "@RebateId", SqlDbType.Int );
                parmRebateId.Direction = ParameterDirection.Output;

                SqlParameter parmCustomRebateId = new SqlParameter( "@CustomRebateId", SqlDbType.Int );
                parmCustomRebateId.Direction = ParameterDirection.Output;

                SqlParameter parmRebateTermId = new SqlParameter( "@RebateTermId", SqlDbType.Int );
                parmRebateTermId.Direction = ParameterDirection.Output;


                parmLoginId.Value = _userName;
                parmContractNumber.Value = contractNumber;
                parmStartQuarterId.Value = startQuarterId;
                parmEndQuarterId.Value = endQuarterId;
                parmRebatePercentOfSales.Value = rebatePercentOfSales;
                parmRebateThreshold.Value = rebateThreshold;
                parmAmountReceived.Value = amountReceived;
                parmIsCustom.Value = bIsCustom;
                parmRebateClause.Value = rebateClause;
                parmStandardRebateTermId.Value = standardRebateTermId;

                if( customStartDate.CompareTo( DateTime.MinValue ) == 0 )
                    parmCustomStartDate.Value = null;
                else
                    parmCustomStartDate.Value = customStartDate;

                cmdCreateContractRebateQuery.Parameters.Add( parmLoginId );
                cmdCreateContractRebateQuery.Parameters.Add( parmContractNumber );
                cmdCreateContractRebateQuery.Parameters.Add( parmStartQuarterId );
                cmdCreateContractRebateQuery.Parameters.Add( parmEndQuarterId );
                cmdCreateContractRebateQuery.Parameters.Add( parmRebatePercentOfSales );
                cmdCreateContractRebateQuery.Parameters.Add( parmRebateThreshold );
                cmdCreateContractRebateQuery.Parameters.Add( parmAmountReceived );
                cmdCreateContractRebateQuery.Parameters.Add( parmIsCustom );
                cmdCreateContractRebateQuery.Parameters.Add( parmRebateClause );
                cmdCreateContractRebateQuery.Parameters.Add( parmStandardRebateTermId );
                cmdCreateContractRebateQuery.Parameters.Add( parmCustomStartDate );
  
                // connect
                dbConnection.Open();

                cmdCreateContractRebateQuery.ExecuteNonQuery();

                rebateId = int.Parse( cmdCreateContractRebateQuery.Parameters[ "@RebateId" ].Value.ToString() );
                customRebateId = int.Parse( cmdCreateContractRebateQuery.Parameters[ "@CustomRebateId" ].Value.ToString() );
                rebateTermId = int.Parse( cmdCreateContractRebateQuery.Parameters[ "@RebateTermId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.CreateContractRebate(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateContractRebate( string contractNumber, int standardRebateTermId, int rebateId, int startQuarterId, int endQuarterId, DateTime customStartDate, double rebatePercentOfSales, double rebateThreshold,
                        double amountReceived, bool bIsCustom, string rebateClause, ref int customRebateId, ref int rebateTermId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            rebateId = -1;
            customRebateId = -1;
            rebateTermId = -1;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //UpdateContractRebate
                //(
                //@UserLogin nvarchar(120),
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@StandardRebateTermId int,
                //@RebateId int,
                //@StartQuarterId int, 
                //@EndQuarterId int, 
                //@RebatePercentOfSales numeric(8,3), 
                //@RebateThreshold money, 
                //@AmountReceived money, 
                //@IsCustom bit,
                //@RebateClause nvarchar(4000),
                //@CustomStartDate datetime = null,
                //@CustomRebateId int output, -- when updating from standard to custom
                //@RebateTermId int output  -- when updating from custom to standard

                SqlCommand cmdUpdateContractRebateQuery = new SqlCommand( "UpdateContractRebate", dbConnection );
                cmdUpdateContractRebateQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractRebateQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractRebateQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmLoginId = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmStandardRebateTermId = new SqlParameter( "@StandardRebateTermId", SqlDbType.Int );
                SqlParameter parmRebateId = new SqlParameter( "@RebateId", SqlDbType.Int );
                SqlParameter parmStartQuarterId = new SqlParameter( "@StartQuarterId", SqlDbType.Int );
                SqlParameter parmEndQuarterId = new SqlParameter( "@EndQuarterId", SqlDbType.Int );
                SqlParameter parmRebatePercentOfSales = new SqlParameter( "@RebatePercentOfSales", SqlDbType.Decimal, 12 );
                SqlParameter parmRebateThreshold = new SqlParameter( "@RebateThreshold", SqlDbType.Money );
                SqlParameter parmAmountReceived = new SqlParameter( "@AmountReceived", SqlDbType.Money );
                SqlParameter parmIsCustom = new SqlParameter( "@IsCustom", SqlDbType.Bit );
                SqlParameter parmRebateClause = new SqlParameter( "@RebateClause", SqlDbType.NVarChar, 4000 );
                SqlParameter parmCustomStartDate = new SqlParameter( "@CustomStartDate", SqlDbType.DateTime );
                parmCustomStartDate.IsNullable = true;

                SqlParameter parmCustomRebateId = new SqlParameter( "@CustomRebateId", SqlDbType.Int );
                parmCustomRebateId.Direction = ParameterDirection.Output;

                SqlParameter parmRebateTermId = new SqlParameter( "@RebateTermId", SqlDbType.Int );
                parmRebateTermId.Direction = ParameterDirection.Output;

                parmLoginId.Value = _userName;
                parmContractNumber.Value = contractNumber;
                parmStandardRebateTermId.Value = standardRebateTermId;
                parmRebateId.Value = rebateId;

                parmStartQuarterId.Value = startQuarterId;
                parmEndQuarterId.Value = endQuarterId;
                parmRebatePercentOfSales.Value = rebatePercentOfSales;
                parmRebateThreshold.Value = rebateThreshold;
                parmAmountReceived.Value = amountReceived;
                parmIsCustom.Value = bIsCustom;
                parmRebateClause.Value = rebateClause;

                if( customStartDate.CompareTo( DateTime.MinValue ) == 0 )
                    parmCustomStartDate.Value = null;
                else
                    parmCustomStartDate.Value = customStartDate;

                cmdUpdateContractRebateQuery.Parameters.Add( parmLoginId );
                cmdUpdateContractRebateQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractRebateQuery.Parameters.Add( parmStandardRebateTermId );
                cmdUpdateContractRebateQuery.Parameters.Add( parmRebateId );

                cmdUpdateContractRebateQuery.Parameters.Add( parmStartQuarterId );
                cmdUpdateContractRebateQuery.Parameters.Add( parmEndQuarterId );
                cmdUpdateContractRebateQuery.Parameters.Add( parmRebatePercentOfSales );
                cmdUpdateContractRebateQuery.Parameters.Add( parmRebateThreshold );
                cmdUpdateContractRebateQuery.Parameters.Add( parmAmountReceived );
                cmdUpdateContractRebateQuery.Parameters.Add( parmIsCustom );
                cmdUpdateContractRebateQuery.Parameters.Add( parmRebateClause );
                cmdUpdateContractRebateQuery.Parameters.Add( parmCustomStartDate );

                // connect
                dbConnection.Open();

                cmdUpdateContractRebateQuery.ExecuteNonQuery();

                customRebateId = int.Parse( cmdUpdateContractRebateQuery.Parameters[ "@CustomRebateId" ].Value.ToString() );
                rebateTermId = int.Parse( cmdUpdateContractRebateQuery.Parameters[ "@RebateTermId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractRebate(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool SelectRebatesForContract( ref DataSet dsContractRebates, string contractNumber )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daRebatesForContract = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectRebatesForContract
                //(
                //@UserLogin nvarchar(120),
                //@UserId uniqueidentifier,
                //@ContractNumber nvarchar(20)

                SqlCommand cmdSelectRebatesForContractQuery = new SqlCommand( "SelectRebatesForContract", dbConnection );
                cmdSelectRebatesForContractQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectRebatesForContractQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectRebatesForContractQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmLoginId = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
 
                parmLoginId.Value = _userName;
                parmContractNumber.Value = contractNumber;

                cmdSelectRebatesForContractQuery.Parameters.Add( parmLoginId );
                cmdSelectRebatesForContractQuery.Parameters.Add( parmContractNumber );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daRebatesForContract = new SqlDataAdapter();
                daRebatesForContract.SelectCommand = cmdSelectRebatesForContractQuery;

                dsContractRebates = new DataSet( "RebatesForContract" );
                DataTable dtRebatesForContract = dsContractRebates.Tables.Add( "RebatesForContractTable" );

                //r.RebateId, r.StartQuarterId, r.EndQuarterId, r.RebatePercentOfSales, r.RebateThreshold, 
                //r.AmountReceived, 0 as IsCustom, r.CreatedBy, r.CreationDate, r.LastModifiedBy, r.LastModificationDate, 
                //s.StandardRebateTermId as RebateTermId, t.RebatesStandardRebateTermId, s.RebateClause, s.IsActive

                // add the common elements to the table
                DataColumn rebateIdColumn = new DataColumn( "RebateId", typeof( int ) );
                dtRebatesForContract.Columns.Add( rebateIdColumn );

                dtRebatesForContract.Columns.Add( "StartQuarterId", typeof( string ) );
                dtRebatesForContract.Columns.Add( "EndQuarterId", typeof( string ) );
                dtRebatesForContract.Columns.Add( "RebatePercentOfSales", typeof( int ) );
                dtRebatesForContract.Columns.Add( "RebateThreshold", typeof( string ) );
                dtRebatesForContract.Columns.Add( "AmountReceived", typeof( string ) );
                dtRebatesForContract.Columns.Add( "IsCustom", typeof( int ) );

                dtRebatesForContract.Columns.Add( "CreatedBy", typeof( string ) );
                dtRebatesForContract.Columns.Add( "CreationDate", typeof( DateTime ) );
                dtRebatesForContract.Columns.Add( "LastModifiedBy", typeof( string ) );
                dtRebatesForContract.Columns.Add( "LastModificationDate", typeof( DateTime ) );


                dtRebatesForContract.Columns.Add( "RebateTermId", typeof( int ) );
                dtRebatesForContract.Columns.Add( "RebatesStandardRebateTermId", typeof( int ) );
                dtRebatesForContract.Columns.Add( "RebateClause", typeof( string ) );
                dtRebatesForContract.Columns.Add( "IsActive", typeof( int ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = rebateIdColumn;

                // add the keys to the table
                dtRebatesForContract.PrimaryKey = primaryKeyColumns;

                dtRebatesForContract.Clear();

                // connect
                dbConnection.Open();

                // run
                daRebatesForContract.Fill( dsContractRebates, "RebatesForContractTable" );

                RowsReturned = dsContractRebates.Tables[ "RebatesForContractTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectRebatesForContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool GetYearQuarterInfo( int yearQuarterId, ref string yearQuarterDescription, ref int fiscalYear, ref int quarter, ref DateTime quarterStartDate, ref DateTime quarterEndDate, ref int calendarYear )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // GetYearQuarterInfoFromId
                //(
                //@UserLogin nvarchar(120),
                //@QuarterId int,
                //@YearQuarterDescription nvarchar(20) OUTPUT,
                //@FiscalYear int OUTPUT,
                //@Quarter int OUTPUT,
                //@QuarterStartDate datetime OUTPUT,
                //@QuarterEndDate datetime OUTPUT,
                //@CalendarYear int OUTPUT

                SqlCommand cmdGetYearQuarterInfoQuery = new SqlCommand( "GetYearQuarterInfoFromId", dbConnection );
                cmdGetYearQuarterInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetYearQuarterInfoQuery.CommandTimeout = 30;

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 20 );
                SqlParameter parmQuarterId = new SqlParameter( "@QuarterId", SqlDbType.Int );
                SqlParameter parmYearQuarterDescription = new SqlParameter( "@YearQuarterDescription", SqlDbType.NVarChar, 20 );
                parmYearQuarterDescription.Direction = ParameterDirection.Output;
                SqlParameter parmFiscalYear = new SqlParameter( "@FiscalYear", SqlDbType.Int );
                parmFiscalYear.Direction = ParameterDirection.Output;

                SqlParameter parmQuarter = new SqlParameter( "@Quarter", SqlDbType.Int );
                parmQuarter.Direction = ParameterDirection.Output;

                SqlParameter parmQuarterStartDate = new SqlParameter( "@QuarterStartDate", SqlDbType.DateTime );
                parmQuarterStartDate.Direction = ParameterDirection.Output;

                SqlParameter parmQuarterEndDate = new SqlParameter( "@QuarterEndDate", SqlDbType.DateTime );
                parmQuarterEndDate.Direction = ParameterDirection.Output;

                SqlParameter parmCalendarYear = new SqlParameter( "@CalendarYear", SqlDbType.Int );
                parmCalendarYear.Direction = ParameterDirection.Output;

                parmUserLogin.Value = _userName;
                parmQuarterId.Value = yearQuarterId;

                cmdGetYearQuarterInfoQuery.Parameters.Add( parmUserLogin );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmQuarterId );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmYearQuarterDescription );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmFiscalYear );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmQuarter );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmQuarterStartDate );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmQuarterEndDate );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmCalendarYear );
 
                // connect
                dbConnection.Open();

                cmdGetYearQuarterInfoQuery.ExecuteNonQuery();

                yearQuarterDescription = cmdGetYearQuarterInfoQuery.Parameters[ "@YearQuarterDescription" ].Value.ToString();
                fiscalYear = int.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@FiscalYear" ].Value.ToString() );
                quarter = int.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@Quarter" ].Value.ToString() );
                quarterStartDate = DateTime.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@QuarterStartDate" ].Value.ToString() );
                quarterEndDate = DateTime.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@QuarterEndDate" ].Value.ToString() );
                calendarYear = int.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@CalendarYear" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetYearQuarterInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool GetYearQuarterInfo( DateTime testDate, ref string yearQuarterDescription, ref int fiscalYear, ref int quarter, ref DateTime quarterStartDate, ref DateTime quarterEndDate, ref int calendarYear, ref int testQuarterId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // GetYearQuarterInfoFromDate
                //(
                //@UserLogin nvarchar(120),
                //@TestDate datetime,
                //@YearQuarterDescription nvarchar(20) OUTPUT,
                //@FiscalYear int OUTPUT,
                //@Quarter int OUTPUT,
                //@QuarterStartDate datetime OUTPUT,
                //@QuarterEndDate datetime OUTPUT,
                //@CalendarYear int OUTPUT,
                //@TestQuarterId int OUTPUT

                SqlCommand cmdGetYearQuarterInfoQuery = new SqlCommand( "GetYearQuarterInfoFromDate", dbConnection );
                cmdGetYearQuarterInfoQuery.CommandType = CommandType.StoredProcedure;
                cmdGetYearQuarterInfoQuery.CommandTimeout = 30;

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 20 );
                SqlParameter parmTestDate = new SqlParameter( "@TestDate", SqlDbType.DateTime );
                SqlParameter parmYearQuarterDescription = new SqlParameter( "@YearQuarterDescription", SqlDbType.NVarChar, 20 );
                parmYearQuarterDescription.Direction = ParameterDirection.Output;
                SqlParameter parmFiscalYear = new SqlParameter( "@FiscalYear", SqlDbType.Int );
                parmFiscalYear.Direction = ParameterDirection.Output;

                SqlParameter parmQuarter = new SqlParameter( "@Quarter", SqlDbType.Int );
                parmQuarter.Direction = ParameterDirection.Output;

                SqlParameter parmQuarterStartDate = new SqlParameter( "@QuarterStartDate", SqlDbType.DateTime );
                parmQuarterStartDate.Direction = ParameterDirection.Output;

                SqlParameter parmQuarterEndDate = new SqlParameter( "@QuarterEndDate", SqlDbType.DateTime );
                parmQuarterEndDate.Direction = ParameterDirection.Output;

                SqlParameter parmCalendarYear = new SqlParameter( "@CalendarYear", SqlDbType.Int );
                parmCalendarYear.Direction = ParameterDirection.Output;

                SqlParameter parmTestQuarterId = new SqlParameter( "@TestQuarterId", SqlDbType.Int );
                parmTestQuarterId.Direction = ParameterDirection.Output;

                parmUserLogin.Value = _userName;
                parmTestDate.Value = testDate;

                cmdGetYearQuarterInfoQuery.Parameters.Add( parmUserLogin );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmTestDate );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmYearQuarterDescription );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmFiscalYear );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmQuarter );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmQuarterStartDate );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmQuarterEndDate );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmCalendarYear );
                cmdGetYearQuarterInfoQuery.Parameters.Add( parmTestQuarterId );

                // connect
                dbConnection.Open();

                cmdGetYearQuarterInfoQuery.ExecuteNonQuery();

                yearQuarterDescription = cmdGetYearQuarterInfoQuery.Parameters[ "@YearQuarterDescription" ].Value.ToString();
                fiscalYear = int.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@FiscalYear" ].Value.ToString() );
                quarter = int.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@Quarter" ].Value.ToString() );
                quarterStartDate = DateTime.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@QuarterStartDate" ].Value.ToString() );
                quarterEndDate = DateTime.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@QuarterEndDate" ].Value.ToString() );
                calendarYear = int.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@CalendarYear" ].Value.ToString() );
                testQuarterId = int.Parse( cmdGetYearQuarterInfoQuery.Parameters[ "@TestQuarterId" ].Value.ToString() );

            }
            catch ( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in (2) ContractDB.GetYearQuarterInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if ( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }



        public bool GetRebateClauseForRebate( int rebateId, ref bool bIsCustom, ref string rebateClause )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            bIsCustom = false;
            rebateClause = "";


            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //  GetRebateClauseForRebate
                //(
                //@UserLogin nvarchar(120),
                //@UserId uniqueidentifier,
                //@RebateId int,
                //@IsCustom bit OUTPUT,
                //@RebateClause nvarchar(4000) OUTPUT

                SqlCommand cmdGetRebateClauseForRebateQuery = new SqlCommand( "GetRebateClauseForRebate", dbConnection );
                cmdGetRebateClauseForRebateQuery.CommandType = CommandType.StoredProcedure;
                cmdGetRebateClauseForRebateQuery.CommandTimeout = 30;

                AddStandardParameter( cmdGetRebateClauseForRebateQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                SqlParameter parmRebateId = new SqlParameter( "@RebateId", SqlDbType.Int );
                SqlParameter parmIsCustom = new SqlParameter( "@IsCustom", SqlDbType.Bit );
                parmIsCustom.Direction = ParameterDirection.Output;
                SqlParameter parmRebateClause = new SqlParameter( "@RebateClause", SqlDbType.NVarChar, 4000 );
                parmRebateClause.Direction = ParameterDirection.Output;

                parmUserLogin.Value = _userName;
                parmRebateId.Value = rebateId;

                cmdGetRebateClauseForRebateQuery.Parameters.Add( parmUserLogin );
                cmdGetRebateClauseForRebateQuery.Parameters.Add( parmRebateId );
                cmdGetRebateClauseForRebateQuery.Parameters.Add( parmIsCustom );
                cmdGetRebateClauseForRebateQuery.Parameters.Add( parmRebateClause );

                // connect
                dbConnection.Open();

                cmdGetRebateClauseForRebateQuery.ExecuteNonQuery();

                bIsCustom = Boolean.Parse( cmdGetRebateClauseForRebateQuery.Parameters[ "@IsCustom" ].Value.ToString() );
                rebateClause = cmdGetRebateClauseForRebateQuery.Parameters[ "@RebateClause" ].Value.ToString();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetRebateClauseForRebate(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetCustomDateForRebate( int rebateId, ref DateTime customStartDate )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
   
            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                // GetCustomDateForRebate
                //(
                //@UserLogin nvarchar(120),
                //@UserId uniqueidentifier,
                //@RebateId int,
                //@CustomStartDate datetime OUTPUT

                SqlCommand cmdGetCustomDateForRebateQuery = new SqlCommand( "GetCustomDateForRebate", dbConnection );
                cmdGetCustomDateForRebateQuery.CommandType = CommandType.StoredProcedure;
                cmdGetCustomDateForRebateQuery.CommandTimeout = 30;

                AddStandardParameter( cmdGetCustomDateForRebateQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                SqlParameter parmRebateId = new SqlParameter( "@RebateId", SqlDbType.Int );
                SqlParameter parmCustomStartDate = new SqlParameter( "@CustomStartDate", SqlDbType.DateTime );
                parmCustomStartDate.Direction = ParameterDirection.Output;
   
                parmUserLogin.Value = _userName;
                parmRebateId.Value = rebateId;

                cmdGetCustomDateForRebateQuery.Parameters.Add( parmUserLogin );
                cmdGetCustomDateForRebateQuery.Parameters.Add( parmRebateId );
                cmdGetCustomDateForRebateQuery.Parameters.Add( parmCustomStartDate );

                // connect
                dbConnection.Open();

                cmdGetCustomDateForRebateQuery.ExecuteNonQuery();

                if( cmdGetCustomDateForRebateQuery.Parameters[ "@CustomStartDate" ].Value == DBNull.Value ) 
                {
                    customStartDate = DateTime.MinValue;
                }
                else
                {
                    customStartDate = DateTime.Parse( cmdGetCustomDateForRebateQuery.Parameters[ "@CustomStartDate" ].Value.ToString() );
                }

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetCustomDateForRebate(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool SelectActiveFSSContractsForBPAParent( ref DataSet dsActiveFSSContractsForBPAParent )
        {

            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daActiveFSSContractsForBPAParent = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectActiveFSSContractsForBPAParent
                //(
                //@UserLogin nvarchar(120),
                //@UserId uniqueidentifier
                //)
                SqlCommand cmdSelectActiveFSSContracts = new SqlCommand( "SelectActiveFSSContractsForBPAParent", dbConnection );
                cmdSelectActiveFSSContracts.CommandType = CommandType.StoredProcedure;
                cmdSelectActiveFSSContracts.CommandTimeout = 30;

                AddStandardParameter( cmdSelectActiveFSSContracts, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );

                cmdSelectActiveFSSContracts.Parameters.Add( parmUserLogin );

                parmUserLogin.Value = _userName;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daActiveFSSContractsForBPAParent = new SqlDataAdapter();
                daActiveFSSContractsForBPAParent.SelectCommand = cmdSelectActiveFSSContracts;

                dsActiveFSSContractsForBPAParent = new DataSet( "ActiveFSSContractsForBPAParent" );
                DataTable dtActiveFSSContractsForBPAParent = dsActiveFSSContractsForBPAParent.Tables.Add( "ActiveFSSContractsForBPAParentTable" );

                // add the common elements to the table
                DataColumn contractNumberColumn = new DataColumn( "CntrctNum", typeof( string ) );

                dtActiveFSSContractsForBPAParent.Columns.Add( contractNumberColumn );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractNumberColumn;

                // add the keys to the table
                dtActiveFSSContractsForBPAParent.PrimaryKey = primaryKeyColumns;

                dtActiveFSSContractsForBPAParent.Clear();

                // connect
                dbConnection.Open();

                // run
                daActiveFSSContractsForBPAParent.Fill( dsActiveFSSContractsForBPAParent, "ActiveFSSContractsForBPAParentTable" );

                RowsReturned = dsActiveFSSContractsForBPAParent.Tables[ "ActiveFSSContractsForBPAParentTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectActiveFSSContractsForBPAParent(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateContractVendorSocio( int contractId, string contractNumber, int socioBusinessSizeId, int socioVetStatusId, bool bSocioWomanOwned, bool bSocioSDB, bool bSocio8a, bool bHubZone )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractVendorSocio
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@SocioBusinessSizeId int, 
                //@SocioVetStatusId int, 
                //@SocioWomanOwned bit, 
                //@SocioSDB bit,
                //@Socio8a bit, 
                //@HubZone bit

                SqlCommand cmdUpdateContractVendorSocioQuery = new SqlCommand( "UpdateContractVendorSocio", dbConnection );
                cmdUpdateContractVendorSocioQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractVendorSocioQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractVendorSocioQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractVendorSocioQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmSocioBusinessSizeId = new SqlParameter( "@SocioBusinessSizeId", SqlDbType.Int );
                SqlParameter parmSocioVetStatusId = new SqlParameter( "@SocioVetStatusId", SqlDbType.Int );
                SqlParameter parmSocioWomanOwned = new SqlParameter( "@SocioWomanOwned", SqlDbType.Bit );
                SqlParameter parmSocioSDB = new SqlParameter( "@SocioSDB", SqlDbType.Bit );
                SqlParameter parmSocio8a = new SqlParameter( "@Socio8a", SqlDbType.Bit );
                SqlParameter parmSocioHubZone = new SqlParameter( "@HubZone", SqlDbType.Bit );

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmSocioBusinessSizeId.Value = socioBusinessSizeId;
                parmSocioVetStatusId.Value = socioVetStatusId;
                parmSocioWomanOwned.Value = bSocioWomanOwned;
                parmSocioSDB.Value = bSocioSDB;
                parmSocio8a.Value = bSocio8a;
                parmSocioHubZone.Value = bHubZone;

                cmdUpdateContractVendorSocioQuery.Parameters.Add( parmContractId );
                cmdUpdateContractVendorSocioQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractVendorSocioQuery.Parameters.Add( parmSocioBusinessSizeId );
                cmdUpdateContractVendorSocioQuery.Parameters.Add( parmSocioVetStatusId );
                cmdUpdateContractVendorSocioQuery.Parameters.Add( parmSocioWomanOwned );
                cmdUpdateContractVendorSocioQuery.Parameters.Add( parmSocioSDB );
                cmdUpdateContractVendorSocioQuery.Parameters.Add( parmSocio8a );
                cmdUpdateContractVendorSocioQuery.Parameters.Add( parmSocioHubZone );
 
                // connect
                dbConnection.Open();

                cmdUpdateContractVendorSocioQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractVendorSocio(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );

        }

        public bool UpdateContractVendorAttributes( int contractId, string contractNumber, string SAMUEI, string DUNS, string TIN, int vendorTypeId, bool bCreditCardAccepted, bool bHazardousMaterial )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractVendorAttributes
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@SAMUEI nvarchar(12), 
                //@DUNS nvarchar(9), 
                //@TIN nvarchar(9), 
                //@VendorTypeId int, 
                //@CreditCardAccepted bit, 
                //@HazardousMaterial bit

                SqlCommand cmdUpdateContractVendorAttributesQuery = new SqlCommand( "UpdateContractVendorAttributes", dbConnection );
                cmdUpdateContractVendorAttributesQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractVendorAttributesQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractVendorAttributesQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractVendorAttributesQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmSAMUEI = new SqlParameter( "@SAMUEI", SqlDbType.NVarChar, 12 );
                SqlParameter parmDUNS = new SqlParameter( "@DUNS", SqlDbType.NVarChar, 9 );
                SqlParameter parmTIN = new SqlParameter( "@TIN", SqlDbType.NVarChar, 9 );
                SqlParameter parmVendorTypeId = new SqlParameter( "@VendorTypeId", SqlDbType.Int );
                SqlParameter parmCreditCardAccepted = new SqlParameter( "@CreditCardAccepted", SqlDbType.Bit );
                SqlParameter parmHazardousMaterial = new SqlParameter( "@HazardousMaterial", SqlDbType.Bit );
       
                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmSAMUEI.Value = SAMUEI;
                parmDUNS.Value = DUNS;
                parmTIN.Value = TIN;
                parmVendorTypeId.Value = vendorTypeId;
                parmCreditCardAccepted.Value = bCreditCardAccepted;
                parmHazardousMaterial.Value = bHazardousMaterial;
              
                cmdUpdateContractVendorAttributesQuery.Parameters.Add( parmContractId );
                cmdUpdateContractVendorAttributesQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractVendorAttributesQuery.Parameters.Add( parmSAMUEI );
                cmdUpdateContractVendorAttributesQuery.Parameters.Add( parmDUNS );
                cmdUpdateContractVendorAttributesQuery.Parameters.Add( parmTIN );
                cmdUpdateContractVendorAttributesQuery.Parameters.Add( parmVendorTypeId );
                cmdUpdateContractVendorAttributesQuery.Parameters.Add( parmCreditCardAccepted );
                cmdUpdateContractVendorAttributesQuery.Parameters.Add( parmHazardousMaterial );

                // connect
                dbConnection.Open();

                cmdUpdateContractVendorAttributesQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractVendorAttributes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );

        }

        
        public bool UpdateContractVendorInsurancePolicy( int contractId, string contractNumber, DateTime insurancePolicyEffectiveDate, DateTime insurancePolicyExpirationDate )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );
                
                //UpdateContractVendorInsurancePolicy
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@InsurancePolicyEffectiveDate datetime,
                //@InsurancePolicyExpirationDate datetime

                SqlCommand cmdUpdateContractVendorInsurancePolicyQuery = new SqlCommand( "UpdateContractVendorInsurancePolicy", dbConnection );
                cmdUpdateContractVendorInsurancePolicyQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractVendorInsurancePolicyQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractVendorInsurancePolicyQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractVendorInsurancePolicyQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmInsurancePolicyEffectiveDate = new SqlParameter( "@InsurancePolicyEffectiveDate", SqlDbType.DateTime );
                SqlParameter parmInsurancePolicyExpirationDate = new SqlParameter( "@InsurancePolicyExpirationDate", SqlDbType.DateTime );
                
       
                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmInsurancePolicyEffectiveDate.Value = insurancePolicyEffectiveDate;
                parmInsurancePolicyExpirationDate.Value = insurancePolicyExpirationDate;
                              
                cmdUpdateContractVendorInsurancePolicyQuery.Parameters.Add( parmContractId );
                cmdUpdateContractVendorInsurancePolicyQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractVendorInsurancePolicyQuery.Parameters.Add( parmInsurancePolicyEffectiveDate );
                cmdUpdateContractVendorInsurancePolicyQuery.Parameters.Add( parmInsurancePolicyExpirationDate );
               
                // connect
                dbConnection.Open();

                cmdUpdateContractVendorInsurancePolicyQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractVendorInsurancePolicy(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );

        }

        public bool UpdateContractVendorWarrantyInformation( int contractId, string contractNumber, string warrantyDuration, string warrantyNotes )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );
                
                //UpdateContractVendorWarrantyInformation
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@WarrantyDuration nvarchar(20), 
                //@WarrantyNotes nvarchar(1000)

                SqlCommand cmdUpdateContractVendorWarrantyQuery = new SqlCommand( "UpdateContractVendorWarrantyInformation", dbConnection );
                cmdUpdateContractVendorWarrantyQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractVendorWarrantyQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractVendorWarrantyQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractVendorWarrantyQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmWarrantyDuration = new SqlParameter( "@WarrantyDuration", SqlDbType.NVarChar, 20 );
                SqlParameter parmWarrantyNotes = new SqlParameter( "@WarrantyNotes", SqlDbType.NVarChar, 1000 );
  

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmWarrantyDuration.Value = warrantyDuration;
                parmWarrantyNotes.Value = warrantyNotes;
               
                cmdUpdateContractVendorWarrantyQuery.Parameters.Add( parmContractId );
                cmdUpdateContractVendorWarrantyQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractVendorWarrantyQuery.Parameters.Add( parmWarrantyDuration );
                cmdUpdateContractVendorWarrantyQuery.Parameters.Add( parmWarrantyNotes );
               
                // connect
                dbConnection.Open();

                cmdUpdateContractVendorWarrantyQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractVendorWarrantyInformation(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );

        }

        public bool UpdateContractVendorReturnedGoodsPolicy( int contractId, string contractNumber, int returnedGoodsPolicyTypeId, string returnedGoodsPolicyNotes )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );
                
                //UpdateContractVendorReturnedGoodsPolicy
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@ReturnedGoodsPolicyTypeId int, 
                //@ReturnedGoodsPolicyNotes nvarchar(1000)


                SqlCommand cmdUpdateContractVendorWarrantyQuery = new SqlCommand( "UpdateContractVendorReturnedGoodsPolicy", dbConnection );
                cmdUpdateContractVendorWarrantyQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractVendorWarrantyQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractVendorWarrantyQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractVendorWarrantyQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmReturnedGoodsPolicyTypeId = new SqlParameter( "@ReturnedGoodsPolicyTypeId", SqlDbType.Int );
                SqlParameter parmReturnedGoodsPolicyNotes = new SqlParameter( "@ReturnedGoodsPolicyNotes", SqlDbType.NVarChar, 1000 );


                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmReturnedGoodsPolicyTypeId.Value = returnedGoodsPolicyTypeId;
                parmReturnedGoodsPolicyNotes.Value = returnedGoodsPolicyNotes;

                cmdUpdateContractVendorWarrantyQuery.Parameters.Add( parmContractId );
                cmdUpdateContractVendorWarrantyQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractVendorWarrantyQuery.Parameters.Add( parmReturnedGoodsPolicyTypeId );
                cmdUpdateContractVendorWarrantyQuery.Parameters.Add( parmReturnedGoodsPolicyNotes );

                // connect
                dbConnection.Open();

                cmdUpdateContractVendorWarrantyQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractVendorReturnedGoodsPolicy(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }
 
        public bool GetGeographicCoverage( string contractNumber, ref DataSet dsGeographicCoverage )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daGeographicCoverage = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetGeographicCoverageForContract
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(50)
                SqlCommand cmdSelectGeographicCoverageForContract = new SqlCommand( "GetGeographicCoverageForContract", dbConnection );
                cmdSelectGeographicCoverageForContract.CommandType = CommandType.StoredProcedure;
                cmdSelectGeographicCoverageForContract.CommandTimeout = 30;

                AddStandardParameter( cmdSelectGeographicCoverageForContract, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdSelectGeographicCoverageForContract, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 50 );
                parmContractNumber.Value = contractNumber;
                cmdSelectGeographicCoverageForContract.Parameters.Add( parmContractNumber );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daGeographicCoverage = new SqlDataAdapter();
                daGeographicCoverage.SelectCommand = cmdSelectGeographicCoverageForContract;

                dsGeographicCoverage = new DataSet( "GeographicCoverage" );
                DataTable dtGeographicCoverage = dsGeographicCoverage.Tables.Add( "GeographicCoverageTable" );

                // add the common elements to the table
                DataColumn geographicCoverageIdColumn = new DataColumn( "GeographicCoverageId", typeof( int ) );

                dtGeographicCoverage.Columns.Add( geographicCoverageIdColumn );

                dtGeographicCoverage.Columns.Add( new DataColumn( "Group52", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "Group51", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "Group50", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "Group49", typeof( bool ) ) );

                dtGeographicCoverage.Columns.Add( new DataColumn( "AL", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "AK", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "AZ", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "AR", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "CA", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "CO", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "CT", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "DE", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "DC", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "FL", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "GA", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "HI", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "ID", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "IL", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "IN", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "IA", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "KS", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "KY", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "LA", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "ME", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "MD", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "MA", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "MI", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "MN", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "MS", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "MO", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "MT", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NE", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NV", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NH", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NJ", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NM", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NY", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NC", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "ND", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "OH", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "OK", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "OR", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "PA", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "RI", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "SC", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "SD", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "TN", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "TX", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "UT", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "VT", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "VA", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "WA", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "WV", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "WI", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "WY", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "PR", typeof( bool ) ) );

                dtGeographicCoverage.Columns.Add( new DataColumn( "AB", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "BC", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "MB", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NB", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NF", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NT", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "NS", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "ON", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "PE", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "QC", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "SK", typeof( bool ) ) );
                dtGeographicCoverage.Columns.Add( new DataColumn( "YT", typeof( bool ) ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = geographicCoverageIdColumn;

                // add the keys to the table
                dtGeographicCoverage.PrimaryKey = primaryKeyColumns;

                dtGeographicCoverage.Clear();

                // connect
                dbConnection.Open();

                // run
                daGeographicCoverage.Fill( dsGeographicCoverage, "GeographicCoverageTable" );

                RowsReturned = dsGeographicCoverage.Tables[ "GeographicCoverageTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetGeographicCoverage(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );         
        }

        public bool UpdateContractVendorGeographicCoverage( int contractId, string contractNumber, int geographicCoverageId,
                    		bool bGroup52, 
			                bool bGroup51,
			                bool bGroup50,
			                bool bGroup49,
                            bool bAL,
                            bool bAK,
                            bool bAZ,
                            bool bAR,
                            bool bCA,
                            bool bCO,
                            bool bCT,
                            bool bDE,
                            bool bDC,
                            bool bFL,
                            bool bGA,
                            bool bHI,
                            bool bID,
                            bool bIL,
                            bool bIN,
                            bool bIA,
                            bool bKS,
                            bool bKY,
                            bool bLA,
                            bool bME,
                            bool bMD,
                            bool bMA,
                            bool bMI,
                            bool bMN,
                            bool bMS,
                            bool bMO,
                            bool bMT,
                            bool bNE,
                            bool bNV,
                            bool bNH,
                            bool bNJ,
                            bool bNM,
                            bool bNY,
                            bool bNC,
                            bool bND,
                            bool bOH,
                            bool bOK,
                            bool bOR,
                            bool bPA,
                            bool bRI,
                            bool bSC,
                            bool bSD,
                            bool bTN,
                            bool bTX,
                            bool bUT,
                            bool bVT,
                            bool bVA,
                            bool bWA,
                            bool bWV,
                            bool bWI,
                            bool bWY,
                            bool bPR,
                            bool bAB,
                            bool bBC,
                            bool bMB,
                            bool bNB,
                            bool bNF,
                            bool bNT,
                            bool bNS,
                            bool bON,
                            bool bPE,
                            bool bQC,
                            bool bSK,
                            bool bYT )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // UpdateGeographicCoverageForContract
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(50),
                //@Group52 bit=0,  -- 3
                //@Group51 bit=0,  -- 2
                //@Group50 bit=0,  -- 4
                //@Group49 bit=0,  -- 1
                //@AL bit=0,
                //@AK bit=0,
                //@AZ bit=0,
                //@AR bit=0,
                //@CA bit=0,
                //@CO bit=0,
                //@CT bit=0,
                //@DE bit=0,
                //@DC bit=0,
                //@FL bit=0,
                //@GA bit=0,
                //@HI bit=0,
                //@ID bit=0,
                //@IL bit=0,
                //@IN bit=0,
                //@IA bit=0,
                //@KS bit=0,
                //@KY bit=0,
                //@LA bit=0,
                //@ME bit=0,
                //@MD bit=0,
                //@MA bit=0,
                //@MI bit=0,
                //@MN bit=0,
                //@MS bit=0,
                //@MO bit=0,
                //@MT bit=0,
                //@NE bit=0,
                //@NV bit=0,
                //@NH bit=0,
                //@NJ bit=0,
                //@NM bit=0,
                //@NY bit=0,
                //@NC bit=0,
                //@ND bit=0,
                //@OH bit=0,
                //@OK bit=0,
                //@OR bit=0,
                //@PA bit=0,
                //@RI bit=0,
                //@SC bit=0,
                //@SD bit=0,
                //@TN bit=0,
                //@TX bit=0,
                //@UT bit=0,
                //@VT bit=0,
                //@VA bit=0,
                //@WA bit=0,
                //@WV bit=0,
                //@WI bit=0,
                //@WY bit=0,  -- 51

                //@PR bit=0,
                //@AB bit=0,
                //@BC bit=0,
                //@MB bit=0,
                //@NB bit=0,
                //@NF bit=0,
                //@NT bit=0,
                //@NS bit=0,
                //@ON bit=0,
                //@PE bit=0,
                //@QC bit=0,
                //@SK bit=0,
                //@YT bit=0  -- 64

                SqlCommand cmdUpdateGeographicCoverageQuery = new SqlCommand( "UpdateGeographicCoverageForContract", dbConnection );
                cmdUpdateGeographicCoverageQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateGeographicCoverageQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateGeographicCoverageQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateGeographicCoverageQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 50 );
    
                SqlParameter parmGroup52  = new SqlParameter( "@Group52", SqlDbType.Bit );
                SqlParameter parmGroup51   = new SqlParameter( "@Group51", SqlDbType.Bit );
                SqlParameter parmGroup50   = new SqlParameter( "@Group50", SqlDbType.Bit );
                SqlParameter parmGroup49   = new SqlParameter( "@Group49", SqlDbType.Bit );
                SqlParameter parmAL    = new SqlParameter( "@AL", SqlDbType.Bit );
                SqlParameter parmAK    = new SqlParameter( "@AK", SqlDbType.Bit );
                SqlParameter parmAZ    = new SqlParameter( "@AZ", SqlDbType.Bit );
                SqlParameter parmAR    = new SqlParameter( "@AR", SqlDbType.Bit );
                SqlParameter parmCA    = new SqlParameter( "@CA", SqlDbType.Bit );
                SqlParameter parmCO    = new SqlParameter( "@CO", SqlDbType.Bit );
                SqlParameter parmCT    = new SqlParameter( "@CT", SqlDbType.Bit );
                SqlParameter parmDE    = new SqlParameter( "@DE", SqlDbType.Bit );
                SqlParameter parmDC    = new SqlParameter( "@DC", SqlDbType.Bit );
                SqlParameter parmFL    = new SqlParameter( "@FL", SqlDbType.Bit );
                SqlParameter parmGA    = new SqlParameter( "@GA", SqlDbType.Bit );
                SqlParameter parmHI    = new SqlParameter( "@HI", SqlDbType.Bit );
                SqlParameter parmID    = new SqlParameter( "@ID", SqlDbType.Bit );
                SqlParameter parmIL    = new SqlParameter( "@IL", SqlDbType.Bit );
                SqlParameter parmIN    = new SqlParameter( "@IN", SqlDbType.Bit );
                SqlParameter parmIA    = new SqlParameter( "@IA", SqlDbType.Bit );
                SqlParameter parmKS    = new SqlParameter( "@KS", SqlDbType.Bit );
                SqlParameter parmKY    = new SqlParameter( "@KY", SqlDbType.Bit );
                SqlParameter parmLA    = new SqlParameter( "@LA", SqlDbType.Bit );
                SqlParameter parmME    = new SqlParameter( "@ME", SqlDbType.Bit );
                SqlParameter parmMD    = new SqlParameter( "@MD", SqlDbType.Bit );
                SqlParameter parmMA    = new SqlParameter( "@MA", SqlDbType.Bit );
                SqlParameter parmMI    = new SqlParameter( "@MI", SqlDbType.Bit );
                SqlParameter parmMN    = new SqlParameter( "@MN", SqlDbType.Bit );
                SqlParameter parmMS    = new SqlParameter( "@MS", SqlDbType.Bit );
                SqlParameter parmMO    = new SqlParameter( "@MO", SqlDbType.Bit );
                SqlParameter parmMT    = new SqlParameter( "@MT", SqlDbType.Bit );
                SqlParameter parmNE    = new SqlParameter( "@NE", SqlDbType.Bit );
                SqlParameter parmNV    = new SqlParameter( "@NV", SqlDbType.Bit );
                SqlParameter parmNH    = new SqlParameter( "@NH", SqlDbType.Bit );
                SqlParameter parmNJ    = new SqlParameter( "@NJ", SqlDbType.Bit );
                SqlParameter parmNM    = new SqlParameter( "@NM", SqlDbType.Bit );
                SqlParameter parmNY    = new SqlParameter( "@NY", SqlDbType.Bit );
                SqlParameter parmNC    = new SqlParameter( "@NC", SqlDbType.Bit );
                SqlParameter parmND    = new SqlParameter( "@ND", SqlDbType.Bit );
                SqlParameter parmOH    = new SqlParameter( "@OH", SqlDbType.Bit );
                SqlParameter parmOK    = new SqlParameter( "@OK", SqlDbType.Bit );
                SqlParameter parmOR    = new SqlParameter( "@OR", SqlDbType.Bit );
                SqlParameter parmPA    = new SqlParameter( "@PA", SqlDbType.Bit );
                SqlParameter parmRI    = new SqlParameter( "@RI", SqlDbType.Bit );
                SqlParameter parmSC    = new SqlParameter( "@SC", SqlDbType.Bit );
                SqlParameter parmSD    = new SqlParameter( "@SD", SqlDbType.Bit );
                SqlParameter parmTN    = new SqlParameter( "@TN", SqlDbType.Bit );
                SqlParameter parmTX    = new SqlParameter( "@TX", SqlDbType.Bit );
                SqlParameter parmUT    = new SqlParameter( "@UT", SqlDbType.Bit );
                SqlParameter parmVT    = new SqlParameter( "@VT", SqlDbType.Bit );
                SqlParameter parmVA    = new SqlParameter( "@VA", SqlDbType.Bit );
                SqlParameter parmWA    = new SqlParameter( "@WA", SqlDbType.Bit );
                SqlParameter parmWV    = new SqlParameter( "@WV", SqlDbType.Bit );
                SqlParameter parmWI    = new SqlParameter( "@WI", SqlDbType.Bit );
                SqlParameter parmWY    = new SqlParameter( "@WY", SqlDbType.Bit );  

                SqlParameter parmPR    = new SqlParameter( "@PR", SqlDbType.Bit );
                SqlParameter parmAB    = new SqlParameter( "@AB", SqlDbType.Bit );
                SqlParameter parmBC    = new SqlParameter( "@BC", SqlDbType.Bit );
                SqlParameter parmMB    = new SqlParameter( "@MB", SqlDbType.Bit );
                SqlParameter parmNB    = new SqlParameter( "@NB", SqlDbType.Bit );
                SqlParameter parmNF    = new SqlParameter( "@NF", SqlDbType.Bit );
                SqlParameter parmNT    = new SqlParameter( "@NT", SqlDbType.Bit );
                SqlParameter parmNS    = new SqlParameter( "@NS", SqlDbType.Bit );
                SqlParameter parmON    = new SqlParameter( "@ON", SqlDbType.Bit );
                SqlParameter parmPE    = new SqlParameter( "@PE", SqlDbType.Bit );
                SqlParameter parmQC    = new SqlParameter( "@QC", SqlDbType.Bit );
                SqlParameter parmSK    = new SqlParameter( "@SK", SqlDbType.Bit );
                SqlParameter parmYT = new SqlParameter( "@YT", SqlDbType.Bit );

                parmContractNumber.Value = contractNumber;
                parmGroup52.Value = bGroup52;
                parmGroup51.Value = bGroup51;
                parmGroup50.Value = bGroup50;
                parmGroup49.Value = bGroup49;
                parmAL.Value = bAL;
                parmAK.Value = bAK;
                parmAZ.Value = bAZ;
                parmAR.Value = bAR;
                parmCA.Value = bCA;
                parmCO.Value = bCO;
                parmCT.Value = bCT;
                parmDE.Value = bDE;
                parmDC.Value = bDC;
                parmFL.Value = bFL;
                parmGA.Value = bGA;
                parmHI.Value = bHI;
                parmID.Value = bID;
                parmIL.Value = bIL;
                parmIN.Value = bIN;
                parmIA.Value = bIA;
                parmKS.Value = bKS;
                parmKY.Value = bKY;
                parmLA.Value = bLA;
                parmME.Value = bME;
                parmMD.Value = bMD;
                parmMA.Value = bMA;
                parmMI.Value = bMI;
                parmMN.Value = bMN;
                parmMS.Value = bMS;
                parmMO.Value = bMO;
                parmMT.Value = bMT;
                parmNE.Value = bNE;
                parmNV.Value = bNV;
                parmNH.Value = bNH;
                parmNJ.Value = bNJ;
                parmNM.Value = bNM;
                parmNY.Value = bNY;
                parmNC.Value = bNC;
                parmND.Value = bND;
                parmOH.Value = bOH;
                parmOK.Value = bOK;
                parmOR.Value = bOR;
                parmPA.Value = bPA;
                parmRI.Value = bRI;
                parmSC.Value = bSC;
                parmSD.Value = bSD;
                parmTN.Value = bTN;
                parmTX.Value = bTX;
                parmUT.Value = bUT;
                parmVT.Value = bVT;
                parmVA.Value = bVA;
                parmWA.Value = bWA;
                parmWV.Value = bWV;
                parmWI.Value = bWI;
                parmWY.Value = bWY;
                parmPR.Value = bPR;
                parmAB.Value = bAB;
                parmBC.Value = bBC;
                parmMB.Value = bMB;
                parmNB.Value = bNB;
                parmNF.Value = bNF;
                parmNT.Value = bNT;
                parmNS.Value = bNS;
                parmON.Value = bON;
                parmPE.Value = bPE;
                parmQC.Value = bQC;
                parmSK.Value = bSK;
                parmYT.Value = bYT;

                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmContractNumber );
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmGroup52 ); //Group52;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmGroup51 ); //Group51;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmGroup50 ); //Group50;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmGroup49 ); //Group49;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmAL ); //AL;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmAK ); //AK;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmAZ ); //AZ;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmAR ); //AR;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmCA ); //CA;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmCO ); //CO;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmCT ); //CT;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmDE ); //DE;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmDC ); //DC;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmFL ); //FL;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmGA ); //GA;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmHI ); //HI;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmID ); //ID;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmIL ); //IL;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmIN ); //IN;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmIA ); //IA;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmKS ); //KS;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmKY ); //KY;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmLA ); //LA;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmME ); //ME;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmMD ); //MD;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmMA ); //MA;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmMI ); //MI;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmMN ); //MN;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmMS ); //MS;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmMO ); //MO;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmMT ); //MT;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNE ); //NE;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNV ); //NV;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNH ); //NH;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNJ ); //NJ;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNM ); //NM;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNY ); //NY;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNC ); //NC;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmND ); //ND;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmOH ); //OH;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmOK ); //OK;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmOR ); //OR;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmPA ); //PA;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmRI ); //RI;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmSC ); //SC;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmSD ); //SD;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmTN ); //TN;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmTX ); //TX;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmUT ); //UT;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmVT ); //VT;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmVA ); //VA;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmWA ); //WA;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmWV ); //WV;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmWI ); //WI;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmWY ); //WY;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmPR ); //PR;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmAB ); //AB;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmBC ); //BC;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmMB ); //MB;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNB ); //NB;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNF ); //NF;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNT ); //NT;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmNS ); //NS;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmON ); //ON;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmPE ); //PE;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmQC ); //QC;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmSK ); //SK;
                 cmdUpdateGeographicCoverageQuery.Parameters.Add( parmYT ); //YT;

                // connect
                dbConnection.Open();

                cmdUpdateGeographicCoverageQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractVendorGeographicCoverage(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateContractDetailsAttributes( int contractId, string contractNumber, 
                        decimal estimatedContractValue, string FPRFreeFormatDateString, int IffTypeId, string solicitationNumber, 
                    string trackingCustomerName, string ratio, string minimumOrder)
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractDetailsAttributes
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@EstimatedContractValue money, 
                //@FPRFreeFormatDateString nvarchar(255), 
                //@IffTypeId int,
                //@SolicitationNumber nvarchar(40),
                //@TrackingCustomerName nvarchar(255),
                //@Ratio nvarchar(255), 
                //@MinimumOrder nvarchar(255)


                SqlCommand cmdUpdateContractDetailsAttributesQuery = new SqlCommand( "UpdateContractDetailsAttributes", dbConnection );
                cmdUpdateContractDetailsAttributesQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractDetailsAttributesQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractDetailsAttributesQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractDetailsAttributesQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmEstimatedContractValue = new SqlParameter( "@EstimatedContractValue", SqlDbType.Money );
                SqlParameter parmFPRFreeFormatDateString = new SqlParameter( "@FPRFreeFormatDateString", SqlDbType.NVarChar, 255 );
                SqlParameter parmIffTypeId = new SqlParameter( "@IffTypeId", SqlDbType.Int );
                SqlParameter parmSolicitationNumber = new SqlParameter( "@SolicitationNumber", SqlDbType.NVarChar, 40 );
                SqlParameter parmTrackingCustomerName = new SqlParameter( "@TrackingCustomerName", SqlDbType.NVarChar, 255 );
                SqlParameter parmRatio = new SqlParameter( "@Ratio", SqlDbType.NVarChar, 255 );
                SqlParameter parmMinimumOrder = new SqlParameter( "@MinimumOrder", SqlDbType.NVarChar, 255 );

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmEstimatedContractValue.Value = estimatedContractValue;
                parmFPRFreeFormatDateString.Value = FPRFreeFormatDateString;

                // tbl_Cntrcts permits null values
                if( IffTypeId == -1 )
                    parmIffTypeId.Value = DBNull.Value;
                else
                    parmIffTypeId.Value = IffTypeId;

                parmSolicitationNumber.Value = solicitationNumber;
                parmTrackingCustomerName.Value = trackingCustomerName;
                parmRatio.Value = ratio;
                parmMinimumOrder.Value = minimumOrder; 

                cmdUpdateContractDetailsAttributesQuery.Parameters.Add( parmContractId );
                cmdUpdateContractDetailsAttributesQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractDetailsAttributesQuery.Parameters.Add( parmEstimatedContractValue );
                cmdUpdateContractDetailsAttributesQuery.Parameters.Add( parmFPRFreeFormatDateString );
                cmdUpdateContractDetailsAttributesQuery.Parameters.Add( parmIffTypeId );
                cmdUpdateContractDetailsAttributesQuery.Parameters.Add( parmSolicitationNumber );
                cmdUpdateContractDetailsAttributesQuery.Parameters.Add( parmTrackingCustomerName );
                cmdUpdateContractDetailsAttributesQuery.Parameters.Add( parmRatio );
                cmdUpdateContractDetailsAttributesQuery.Parameters.Add( parmMinimumOrder );
            
                // connect
                dbConnection.Open();

                cmdUpdateContractDetailsAttributesQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractDetailsAttributes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool UpdateContractDetailsDelivery( int contractId, string contractNumber, 
                        string deliveryTerms, string expeditedDeliveryTerms )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractDetailsDelivery
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@DeliveryTerms nvarchar(255),
                //@ExpiditedDeliveryTerms nvarchar(255)

                SqlCommand cmdUpdateContractDetailsDeliveryQuery = new SqlCommand( "UpdateContractDetailsDelivery", dbConnection );
                cmdUpdateContractDetailsDeliveryQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractDetailsDeliveryQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractDetailsDeliveryQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractDetailsDeliveryQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmDeliveryTerms = new SqlParameter( "@DeliveryTerms", SqlDbType.NVarChar, 255 );
                SqlParameter parmExpeditedDeliveryTerms = new SqlParameter( "@ExpiditedDeliveryTerms", SqlDbType.NVarChar, 255 );

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmDeliveryTerms.Value = deliveryTerms;
                parmExpeditedDeliveryTerms.Value = expeditedDeliveryTerms;

                cmdUpdateContractDetailsDeliveryQuery.Parameters.Add( parmContractId );
                cmdUpdateContractDetailsDeliveryQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractDetailsDeliveryQuery.Parameters.Add( parmDeliveryTerms );
                cmdUpdateContractDetailsDeliveryQuery.Parameters.Add( parmExpeditedDeliveryTerms );

                // connect
                dbConnection.Open();

                cmdUpdateContractDetailsDeliveryQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractDetailsDelivery(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateContractDetailsDiscount( int contractId, string contractNumber,
                 string basicDiscount,string additionalDiscount, string endOfYearDiscount,
                    string promptPayDiscount, string quantityDiscount, string creditCardDiscount )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );
            
                //UpdateContractDetailsDiscount
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@BasicDiscount nvarchar(255),
                //@AdditionalDiscount nvarchar(255), 
                //@EndOfYearDiscount nvarchar(255),
                //@PromptPayDiscount nvarchar(255), 
                //@QuantityDiscount nvarchar(255), 
                //@CreditCardDiscount nvarchar(255)

                SqlCommand cmdUpdateContractDetailsDiscountQuery = new SqlCommand( "UpdateContractDetailsDiscount", dbConnection );
                cmdUpdateContractDetailsDiscountQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractDetailsDiscountQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractDetailsDiscountQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractDetailsDiscountQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmBasicDiscount = new SqlParameter( "@BasicDiscount", SqlDbType.NVarChar, 255 );
                SqlParameter parmAdditionalDiscount = new SqlParameter( "@AdditionalDiscount", SqlDbType.NVarChar, 255 );
                SqlParameter parmEndOfYearDiscount = new SqlParameter( "@EndOfYearDiscount", SqlDbType.NVarChar, 255 );
                SqlParameter parmPromptPayDiscount = new SqlParameter( "@PromptPayDiscount", SqlDbType.NVarChar, 255 );
                SqlParameter parmQuantityDiscount = new SqlParameter( "@QuantityDiscount", SqlDbType.NVarChar, 255 );
                SqlParameter parmCreditCardDiscount = new SqlParameter( "@CreditCardDiscount", SqlDbType.NVarChar, 255 );

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmBasicDiscount.Value = basicDiscount;
                parmAdditionalDiscount.Value = additionalDiscount;
                parmEndOfYearDiscount.Value = endOfYearDiscount;
                parmPromptPayDiscount.Value = promptPayDiscount;
                parmQuantityDiscount.Value = quantityDiscount;
                parmCreditCardDiscount.Value = creditCardDiscount;

                cmdUpdateContractDetailsDiscountQuery.Parameters.Add( parmContractId );
                cmdUpdateContractDetailsDiscountQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractDetailsDiscountQuery.Parameters.Add( parmBasicDiscount );
                cmdUpdateContractDetailsDiscountQuery.Parameters.Add( parmAdditionalDiscount );
                cmdUpdateContractDetailsDiscountQuery.Parameters.Add( parmEndOfYearDiscount );
                cmdUpdateContractDetailsDiscountQuery.Parameters.Add( parmPromptPayDiscount );
                cmdUpdateContractDetailsDiscountQuery.Parameters.Add( parmQuantityDiscount );
                cmdUpdateContractDetailsDiscountQuery.Parameters.Add( parmCreditCardDiscount );

                // connect
                dbConnection.Open();

                cmdUpdateContractDetailsDiscountQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractDetailsDiscount(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateContractContact( int contractId, string contractNumber, string contactType, string name, string phone, string extension,
                            string fax, string email, string address1, string address2, string city, int countryId, string state, string zip, string webAddress )
          
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractContact
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@ContactType nvarchar(4),  -- 'ADM' - administrator; 'ALT' - alternate; 'TECH' - technical; 'EMER' - emergency; 'ORD' - ordering; 'SALE' - sales;  'BUS' - business address
                //@Name nvarchar(75),  -- length 30 for most person names, but need 75 for vendorname
                //@Phone nvarchar(15),
                //@Extension nvarchar(5),
                //@Fax nvarchar(15),
                //@Email nvarchar(50),
                //@Address1 nvarchar(100),
                //@Address2 nvarchar(100),
                //@City nvarchar(20),
                //@CountryId int,
                //@State nvarchar(2),
                //@Zip nvarchar(10),
                //@WebAddress nvarchar(50)

                SqlCommand cmdUpdateContractContactQuery = new SqlCommand( "UpdateContractContact", dbConnection );
                cmdUpdateContractContactQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractContactQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractContactQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractContactQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmContactType = new SqlParameter( "@ContactType", SqlDbType.NVarChar, 4 );
                SqlParameter parmName = new SqlParameter( "@Name", SqlDbType.NVarChar, 75 );
                SqlParameter parmPhone = new SqlParameter( "@Phone", SqlDbType.NVarChar, 15 );
                SqlParameter parmExtension = new SqlParameter( "@Extension", SqlDbType.NVarChar, 5 );
                SqlParameter parmFax = new SqlParameter( "@Fax", SqlDbType.NVarChar, 15 );
                SqlParameter parmEmail = new SqlParameter( "@Email", SqlDbType.NVarChar, 50 );

                SqlParameter parmAddress1 = new SqlParameter( "@Address1", SqlDbType.NVarChar, 100 );
                SqlParameter parmAddress2 = new SqlParameter( "@Address2", SqlDbType.NVarChar, 100 );
                SqlParameter parmCity = new SqlParameter( "@City", SqlDbType.NVarChar, 20 );
                SqlParameter parmCountryId = new SqlParameter( "@CountryId", SqlDbType.Int );
                SqlParameter parmState = new SqlParameter( "@State", SqlDbType.NVarChar, 2 );
                SqlParameter parmZip = new SqlParameter( "@Zip", SqlDbType.NVarChar, 10 );
                SqlParameter parmWebAddress = new SqlParameter( "@WebAddress", SqlDbType.NVarChar, 50 );


                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmContactType.Value = contactType;
                parmName.Value = name;
                parmPhone.Value = phone;
                parmExtension.Value = extension;
                parmFax.Value = fax;
                parmEmail.Value = email;
                parmAddress1.Value = address1;
                parmAddress2.Value = address2;
                parmCity.Value = city;
                parmCountryId.Value = countryId;
                parmState.Value = state;
                parmZip.Value = zip;
                parmWebAddress.Value = webAddress;

                cmdUpdateContractContactQuery.Parameters.Add( parmContractId );
                cmdUpdateContractContactQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractContactQuery.Parameters.Add( parmContactType );
                cmdUpdateContractContactQuery.Parameters.Add( parmName );
                cmdUpdateContractContactQuery.Parameters.Add( parmPhone );
                cmdUpdateContractContactQuery.Parameters.Add( parmExtension );
                cmdUpdateContractContactQuery.Parameters.Add( parmFax );
                cmdUpdateContractContactQuery.Parameters.Add( parmEmail );
                cmdUpdateContractContactQuery.Parameters.Add( parmAddress1 );
                cmdUpdateContractContactQuery.Parameters.Add( parmAddress2 );
                cmdUpdateContractContactQuery.Parameters.Add( parmCity );
                cmdUpdateContractContactQuery.Parameters.Add( parmCountryId );
                cmdUpdateContractContactQuery.Parameters.Add( parmState );
                cmdUpdateContractContactQuery.Parameters.Add( parmZip );
                cmdUpdateContractContactQuery.Parameters.Add( parmWebAddress );

                // connect
                dbConnection.Open();

                cmdUpdateContractContactQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractContact(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool UpdateContractPricelistVerification( int contractId, string contractNumber, DateTime pricelistVerificationDate, string pricelistVerifiedBy, string currentModNumber, bool bIsVerified )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // UpdateContractPricelistVerification
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@PricelistVerificationDate datetime,
                //@PricelistVerifiedBy nvarchar(25),
                //@CurrentModNumber nvarchar(20),
                //@IsPricelistVerified bit

                SqlCommand cmdUpdateContractPricelistVerificationQuery = new SqlCommand( "UpdateContractPricelistVerification", dbConnection );
                cmdUpdateContractPricelistVerificationQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractPricelistVerificationQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractPricelistVerificationQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractPricelistVerificationQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmPricelistVerificationDate = new SqlParameter( "@PricelistVerificationDate", SqlDbType.DateTime );
                SqlParameter parmPricelistVerifiedBy = new SqlParameter( "@PricelistVerifiedBy", SqlDbType.NVarChar, 25 );
                SqlParameter parmCurrentModNumber = new SqlParameter( "@CurrentModNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmIsPricelistVerified = new SqlParameter( "@IsPricelistVerified", SqlDbType.Bit );

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;

                if( pricelistVerificationDate.CompareTo( DateTime.MinValue ) == 0 )
                {
                    parmPricelistVerificationDate.SqlValue = DBNull.Value;
                }
                else
                {
                    parmPricelistVerificationDate.Value = pricelistVerificationDate;
                }

                parmPricelistVerifiedBy.Value = pricelistVerifiedBy;
                parmCurrentModNumber.Value = currentModNumber;
                parmIsPricelistVerified.Value = bIsVerified;
               

                cmdUpdateContractPricelistVerificationQuery.Parameters.Add( parmContractId );
                cmdUpdateContractPricelistVerificationQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractPricelistVerificationQuery.Parameters.Add( parmPricelistVerificationDate );
                cmdUpdateContractPricelistVerificationQuery.Parameters.Add( parmPricelistVerifiedBy );
                cmdUpdateContractPricelistVerificationQuery.Parameters.Add( parmCurrentModNumber );
                cmdUpdateContractPricelistVerificationQuery.Parameters.Add( parmIsPricelistVerified );
    
                // connect
                dbConnection.Open();

                cmdUpdateContractPricelistVerificationQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractPricelistVerification(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateContractPricelistNotes( int contractId, string contractNumber, string pricelistNotes )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // UpdateContractPricelistNotes
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@PricelistNotes nvarchar(255)

                SqlCommand cmdUpdateContractPricelistNotesQuery = new SqlCommand( "UpdateContractPricelistNotes", dbConnection );
                cmdUpdateContractPricelistNotesQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractPricelistNotesQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractPricelistNotesQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractPricelistNotesQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmPricelistNotes = new SqlParameter( "@PricelistNotes", SqlDbType.NVarChar, 255 );
            

                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmPricelistNotes.Value = pricelistNotes;
            
                cmdUpdateContractPricelistNotesQuery.Parameters.Add( parmContractId );
                cmdUpdateContractPricelistNotesQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractPricelistNotesQuery.Parameters.Add( parmPricelistNotes );
              
                // connect
                dbConnection.Open();

                cmdUpdateContractPricelistNotesQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractPricelistNotes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // currently, this is only the rebate required checkbox; for update of a rebate record see UpdateContractRebate
        public bool UpdateContractRebateHeader( int contractId, string contractNumber, bool bRebateRequired )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractRebateHeader
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@RebateRequired bit

                SqlCommand cmdUpdateContractRebateHeaderQuery = new SqlCommand( "UpdateContractRebateHeader", dbConnection );
                cmdUpdateContractRebateHeaderQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractRebateHeaderQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractRebateHeaderQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractRebateHeaderQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmRebateRequired = new SqlParameter( "@RebateRequired", SqlDbType.Bit );


                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmRebateRequired.Value = bRebateRequired;

                cmdUpdateContractRebateHeaderQuery.Parameters.Add( parmContractId );
                cmdUpdateContractRebateHeaderQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractRebateHeaderQuery.Parameters.Add( parmRebateRequired );

                // connect
                dbConnection.Open();

                cmdUpdateContractRebateHeaderQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractRebateHeader(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool SelectNewSalesQuartersForContract( ref DataSet dsNewSalesQuarters, ref bool bNoUnreportedQuarters, string contractNumber )
        {

            bool bSuccess = true;
            bNoUnreportedQuarters = false;
            SqlConnection dbConnection = null;
            SqlDataAdapter daNewSalesQuartersForContract = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectYearQuartersForEditContractSales
                //(
                //@UserId uniqueidentifier,
                //@ContractNumber nvarchar(20)

                SqlCommand cmdSelectNewSalesQuartersForContractQuery = new SqlCommand( "SelectYearQuartersForEditContractSales", dbConnection );
                cmdSelectNewSalesQuartersForContractQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectNewSalesQuartersForContractQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectNewSalesQuartersForContractQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                parmContractNumber.Value = contractNumber;

                cmdSelectNewSalesQuartersForContractQuery.Parameters.Add( parmContractNumber );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daNewSalesQuartersForContract = new SqlDataAdapter();
                daNewSalesQuartersForContract.SelectCommand = cmdSelectNewSalesQuartersForContractQuery;

                dsNewSalesQuarters = new DataSet( "NewSalesQuartersForContract" );
                DataTable dtNewSalesQuartersForContract = dsNewSalesQuarters.Tables.Add( "NewSalesQuartersForContractTable" );

                // 	select Quarter_ID, Year, Qtr, Title 

                // add the common elements to the table
                DataColumn quarterIdColumn = new DataColumn( "Quarter_ID", typeof( int ) );
                dtNewSalesQuartersForContract.Columns.Add( quarterIdColumn );

                dtNewSalesQuartersForContract.Columns.Add( "Year", typeof( int ) );
                dtNewSalesQuartersForContract.Columns.Add( "Qtr", typeof( int ) );
                dtNewSalesQuartersForContract.Columns.Add( "Title", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = quarterIdColumn;

                // add the keys to the table
                dtNewSalesQuartersForContract.PrimaryKey = primaryKeyColumns;

                dtNewSalesQuartersForContract.Clear();

                // connect
                dbConnection.Open();

                // run
                daNewSalesQuartersForContract.Fill( dsNewSalesQuarters, "NewSalesQuartersForContractTable" );

                RowsReturned = dsNewSalesQuarters.Tables[ "NewSalesQuartersForContractTable" ].Rows.Count;

                if( RowsReturned == 1 ) // this is the "--select--" prompt
                {
                    bNoUnreportedQuarters = true;
                }

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectNewSalesQuartersForContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public const string SBAPlanDetailsTableName = "SBAPlanDetailsTable";

        public bool GetSBAPlanInfo( ref DataSet dsOneSBAPlanRow, string contractNumber, int sbaPlanId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOneSBAPlanRow = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectSBAPlanDetails
                //(
                //@UserId uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@SBAPlanId int

                SqlCommand cmdSelectSBAPlanDetailsQuery = new SqlCommand( "SelectSBAPlanDetails", dbConnection );
                cmdSelectSBAPlanDetailsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectSBAPlanDetailsQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectSBAPlanDetailsQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmSBAPlanId = new SqlParameter( "@SBAPlanId", SqlDbType.Int );

                parmContractNumber.Value = contractNumber;
                parmSBAPlanId.Value = sbaPlanId;

                cmdSelectSBAPlanDetailsQuery.Parameters.Add( parmContractNumber );
                cmdSelectSBAPlanDetailsQuery.Parameters.Add( parmSBAPlanId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOneSBAPlanRow = new SqlDataAdapter();
                daOneSBAPlanRow.SelectCommand = cmdSelectSBAPlanDetailsQuery;

                dsOneSBAPlanRow = new DataSet( "SBAPlanDetails" );
                DataTable dtSBAPlanDetails = dsOneSBAPlanRow.Tables.Add( SBAPlanDetailsTableName );

                // add the common elements to the table
                DataColumn sbaPlanIdColumn = new DataColumn( "SBAPlanId", typeof( int ) );
                dtSBAPlanDetails.Columns.Add( sbaPlanIdColumn );

                dtSBAPlanDetails.Columns.Add( "PlanName", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanTypeId", typeof( int ) );
                dtSBAPlanDetails.Columns.Add( "PlanTypeDescription", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorName", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorAddress", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorCity", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorCountryId", typeof( int ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorCountryName", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorState", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorZip", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorPhone", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorFax", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanAdministratorEmail", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "PlanNotes", typeof( string ) );
                dtSBAPlanDetails.Columns.Add( "Comments", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = sbaPlanIdColumn;

                // add the keys to the table
                dtSBAPlanDetails.PrimaryKey = primaryKeyColumns;

                dtSBAPlanDetails.Clear();

                // connect
                dbConnection.Open();

                // run
                daOneSBAPlanRow.Fill( dsOneSBAPlanRow, SBAPlanDetailsTableName );

                RowsReturned = dsOneSBAPlanRow.Tables[ SBAPlanDetailsTableName ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetSBAPlanInfo(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool CreateSBAPlan( string contractNumber, string planName, int planTypeId, string planAdminName, string planAdminAddress, string planAdminCity, int planAdminCountryId, string planAdminState,
                                    string planAdminZip, string planAdminPhone, string planAdminFax, string planAdminEmail, string planNotes, string comments, ref int newSBAPlanId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            newSBAPlanId = -1;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //InsertSBAPlanDetails
                //(
                //@UserLogin nvarchar(120),
                //@UserId uniqueidentifier,

                //@ContractNumber nvarchar(20),
                //@PlanName	nvarchar(50),
                //@PlanTypeId 	int,
                //@PlanAdminName	nvarchar(50),
                //@PlanAdminAddress	nvarchar(50),
                //@PlanAdminCity	nvarchar(50),
                //@PlanAdminCountryId int,
                //@PlanAdminState nvarchar(2),
                //@PlanAdminZip	nvarchar(15),
                //@PlanAdminPhone	nvarchar(30),
                //@PlanAdminFax	nvarchar(15),
                //@PlanAdminEmail 	nvarchar(50),
                //@PlanNotes 	nvarchar(500),
                //@Comments  char(255),
                //@NewSBAPlanId int output

                SqlCommand cmdCreateSBAPlanQuery = new SqlCommand( "InsertSBAPlanDetails", dbConnection );
                cmdCreateSBAPlanQuery.CommandType = CommandType.StoredProcedure;
                cmdCreateSBAPlanQuery.CommandTimeout = 30;

                SqlParameter parmLoginId = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                AddStandardParameter( cmdCreateSBAPlanQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmPlanName = new SqlParameter( "@PlanName", SqlDbType.NVarChar, 50 );

                SqlParameter parmPlanTypeId = new SqlParameter( "@PlanTypeId", SqlDbType.Int );
                SqlParameter parmPlanAdminName = new SqlParameter( "@PlanAdminName", SqlDbType.NVarChar, 50 );
                SqlParameter parmPlanAdminAddress = new SqlParameter( "@PlanAdminAddress", SqlDbType.NVarChar, 50 );
                SqlParameter parmPlanAdminCity = new SqlParameter( "@PlanAdminCity", SqlDbType.NVarChar, 50 );
                SqlParameter parmPlanAdminCountryId = new SqlParameter( "@PlanAdminCountryId", SqlDbType.Int );
                SqlParameter parmPlanAdminState = new SqlParameter( "@PlanAdminState", SqlDbType.NVarChar, 2 );
                SqlParameter parmPlanAdminZip = new SqlParameter( "@PlanAdminZip", SqlDbType.NVarChar, 15 );
                SqlParameter parmPlanAdminPhone = new SqlParameter( "@PlanAdminPhone", SqlDbType.NVarChar, 30 );
                SqlParameter parmPlanAdminFax = new SqlParameter( "@PlanAdminFax", SqlDbType.NVarChar, 15 );
                SqlParameter parmPlanAdminEmail = new SqlParameter( "@PlanAdminEmail", SqlDbType.NVarChar, 50 );
                SqlParameter parmPlanNotes = new SqlParameter( "@PlanNotes", SqlDbType.NVarChar, 500 );
                SqlParameter parmPlanComments = new SqlParameter( "@Comments", SqlDbType.NVarChar, 255 );

                SqlParameter parmNewSBAPlanId = new SqlParameter( "@NewSBAPlanId", SqlDbType.Int );
                parmNewSBAPlanId.Direction = ParameterDirection.Output;


                parmLoginId.Value = _userName;
                parmContractNumber.Value = contractNumber;
                parmPlanName.Value = planName;
                parmPlanTypeId.Value = planTypeId;
                parmPlanAdminName.Value = planAdminName;
                parmPlanAdminAddress.Value = planAdminAddress;
                parmPlanAdminCity.Value = planAdminCity;
                parmPlanAdminCountryId.Value = planAdminCountryId;

                if( planAdminState.CompareTo( "--" ) == 0 )
                {
                    parmPlanAdminState.Value = DBNull.Value;
                }
                else
                {
                    parmPlanAdminState.Value = planAdminState;
                }
                
                parmPlanAdminZip.Value = planAdminZip;
                parmPlanAdminPhone.Value = planAdminPhone;
                parmPlanAdminFax.Value = planAdminFax;
                parmPlanAdminEmail.Value = planAdminEmail;
                parmPlanNotes.Value = planNotes;
                parmPlanComments.Value = comments;

                cmdCreateSBAPlanQuery.Parameters.Add( parmLoginId );
                cmdCreateSBAPlanQuery.Parameters.Add( parmContractNumber );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanName );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanTypeId );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanAdminName );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanAdminAddress );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanAdminCity );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanAdminCountryId );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanAdminState );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanAdminZip );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanAdminPhone );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanAdminFax );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanAdminEmail );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanNotes );
                cmdCreateSBAPlanQuery.Parameters.Add( parmPlanComments );
                cmdCreateSBAPlanQuery.Parameters.Add( parmNewSBAPlanId );

                // connect
                dbConnection.Open();

                cmdCreateSBAPlanQuery.ExecuteNonQuery();

                newSBAPlanId = int.Parse( cmdCreateSBAPlanQuery.Parameters[ "@NewSBAPlanId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.CreateSBAPlan(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateSBAPlan( string contractNumber, int sbaPlanId, string planName, int planTypeId, string planAdminName, string planAdminAddress, string planAdminCity, int planAdminCountryId, string planAdminState,
                                    string planAdminZip, string planAdminPhone, string planAdminFax, string planAdminEmail, string planNotes, string comments )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //UpdateSBAPlanDetails
                //(
                //@UserLogin nvarchar(120),
                //@CurrentUser uniqueidentifier,

                //@ContractNumber nvarchar(20),
                //@SBAPlanId int,
                //@PlanName	nvarchar(50),
                //@PlanTypeId 	int,
                //@PlanAdminName	nvarchar(50),
                //@PlanAdminAddress	nvarchar(50),
                //@PlanAdminCity	nvarchar(50),
                //@PlanAdminCountryId int,
                //@PlanAdminState nvarchar(2),
                //@PlanAdminZip	nvarchar(15),
                //@PlanAdminPhone	nvarchar(30),
                //@PlanAdminFax	nvarchar(15),
                //@PlanAdminEmail 	nvarchar(50)

                SqlCommand cmdUpdateSBAPlanQuery = new SqlCommand( "UpdateSBAPlanDetails", dbConnection );
                cmdUpdateSBAPlanQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateSBAPlanQuery.CommandTimeout = 30;

                SqlParameter parmLoginId = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                AddStandardParameter( cmdUpdateSBAPlanQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmSBAPlanId = new SqlParameter( "@SBAPlanId", SqlDbType.Int );

                SqlParameter parmPlanName = new SqlParameter( "@PlanName", SqlDbType.NVarChar, 50 );

                SqlParameter parmPlanTypeId = new SqlParameter( "@PlanTypeId", SqlDbType.Int );
                SqlParameter parmPlanAdminName = new SqlParameter( "@PlanAdminName", SqlDbType.NVarChar, 50 );
                SqlParameter parmPlanAdminAddress = new SqlParameter( "@PlanAdminAddress", SqlDbType.NVarChar, 50 );
                SqlParameter parmPlanAdminCity = new SqlParameter( "@PlanAdminCity", SqlDbType.NVarChar, 50 );
                SqlParameter parmPlanAdminCountryId = new SqlParameter( "@PlanAdminCountryId", SqlDbType.Int );
                SqlParameter parmPlanAdminState = new SqlParameter( "@PlanAdminState", SqlDbType.NVarChar, 2 );
                SqlParameter parmPlanAdminZip = new SqlParameter( "@PlanAdminZip", SqlDbType.NVarChar, 15 );
                SqlParameter parmPlanAdminPhone = new SqlParameter( "@PlanAdminPhone", SqlDbType.NVarChar, 30 );
                SqlParameter parmPlanAdminFax = new SqlParameter( "@PlanAdminFax", SqlDbType.NVarChar, 15 );
                SqlParameter parmPlanAdminEmail = new SqlParameter( "@PlanAdminEmail", SqlDbType.NVarChar, 50 );
                SqlParameter parmPlanNotes = new SqlParameter( "@PlanNotes", SqlDbType.NVarChar, 500 );
                SqlParameter parmPlanComments = new SqlParameter( "@Comments", SqlDbType.NVarChar, 255 );

                parmLoginId.Value = _userName;
                parmContractNumber.Value = contractNumber;
                parmSBAPlanId.Value = sbaPlanId;

                parmPlanName.Value = planName;
                parmPlanAdminAddress.Value = planAdminAddress;
                parmPlanTypeId.Value = planTypeId;
                parmPlanAdminName.Value = planAdminName;
                parmPlanAdminCity.Value = planAdminCity;
                parmPlanAdminCountryId.Value = planAdminCountryId;

                if( planAdminState.CompareTo( "--" ) == 0 )
                {
                    parmPlanAdminState.Value = DBNull.Value;
                }
                else
                {
                    parmPlanAdminState.Value = planAdminState;
                }

                parmPlanAdminZip.Value = planAdminZip;
                parmPlanAdminPhone.Value = planAdminPhone;
                parmPlanAdminFax.Value = planAdminFax;
                parmPlanAdminEmail.Value = planAdminEmail;
                parmPlanNotes.Value = planNotes;
                parmPlanComments.Value = comments;

                cmdUpdateSBAPlanQuery.Parameters.Add( parmLoginId );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmContractNumber );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmSBAPlanId );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanName );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanAdminAddress );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanTypeId );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanAdminName );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanAdminCity );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanAdminCountryId );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanAdminState );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanAdminZip );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanAdminPhone );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanAdminFax );
                cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanAdminEmail );
                //cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanNotes );
                //cmdUpdateSBAPlanQuery.Parameters.Add( parmPlanComments );

                // connect
                dbConnection.Open();

                cmdUpdateSBAPlanQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateSBAPlan(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string ActiveSBAPlansTableName = "ActiveSBAPlansTable";

        public bool SelectActiveSBAPlans( ref DataSet dsSBAPlans )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSBAPlans = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectActiveSBAPlans
                //(
                //@UserId uniqueidentifier

                SqlCommand cmdSelectActiveSBAPlansQuery = new SqlCommand( "SelectActiveSBAPlans", dbConnection );
                cmdSelectActiveSBAPlansQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectActiveSBAPlansQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectActiveSBAPlansQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSBAPlans = new SqlDataAdapter();
                daSBAPlans.SelectCommand = cmdSelectActiveSBAPlansQuery;

                dsSBAPlans = new DataSet( "SBAPlans" );
                DataTable dtActiveSBAPlans = dsSBAPlans.Tables.Add( ActiveSBAPlansTableName );

                // add the common elements to the table
                DataColumn sbaPlanIdColumn = new DataColumn( "SBAPlanId", typeof( int ) );
                dtActiveSBAPlans.Columns.Add( sbaPlanIdColumn );

                dtActiveSBAPlans.Columns.Add( "PlanName", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanTypeId", typeof( int ) );
                dtActiveSBAPlans.Columns.Add( "PlanTypeDescription", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorName", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorAddress", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorCity", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorCountryId", typeof( int ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorState", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorZip", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorPhone", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorFax", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorEmail", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanNotes", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "Comments", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = sbaPlanIdColumn;

                // add the keys to the table
                dtActiveSBAPlans.PrimaryKey = primaryKeyColumns;

                dtActiveSBAPlans.Clear();

                // connect
                dbConnection.Open();

                // run
                daSBAPlans.Fill( dsSBAPlans, ActiveSBAPlansTableName );

                RowsReturned = dsSBAPlans.Tables[ ActiveSBAPlansTableName ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectActiveSBAPlans(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool SelectActiveSBAPlans2( bool bIncludeSelect, bool bIncludeInactive, ref DataSet dsSBAPlans )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSBAPlans = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectActiveSBAPlans2
                //(
                //@CurrentUser uniqueidentifier,
                //@IncludeSelect bit,
                //@IncludeInactive bit

                SqlCommand cmdSelectActiveSBAPlansQuery = new SqlCommand( "SelectActiveSBAPlans2", dbConnection );
                cmdSelectActiveSBAPlansQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectActiveSBAPlansQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectActiveSBAPlansQuery, StandardParameterTypes.CurrentUser, _currentUserId );

                SqlParameter parmIncludeSelect = new SqlParameter( "@IncludeSelect", SqlDbType.Bit );
                SqlParameter parmIncludeInactive = new SqlParameter( "@IncludeInactive", SqlDbType.Bit );

                cmdSelectActiveSBAPlansQuery.Parameters.Add( parmIncludeSelect );
                cmdSelectActiveSBAPlansQuery.Parameters.Add( parmIncludeInactive );

                parmIncludeSelect.Value = bIncludeSelect;
                parmIncludeInactive.Value = bIncludeInactive;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSBAPlans = new SqlDataAdapter();
                daSBAPlans.SelectCommand = cmdSelectActiveSBAPlansQuery;

                dsSBAPlans = new DataSet( "SBAPlans" );
                DataTable dtActiveSBAPlans = dsSBAPlans.Tables.Add( ActiveSBAPlansTableName );

                // add the common elements to the table
                DataColumn sbaPlanIdColumn = new DataColumn( "SBAPlanId", typeof( int ) );
                dtActiveSBAPlans.Columns.Add( sbaPlanIdColumn );

                dtActiveSBAPlans.Columns.Add( "PlanName", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanTypeId", typeof( int ) );
                dtActiveSBAPlans.Columns.Add( "PlanTypeDescription", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorName", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorAddress", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorCity", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorCountryId", typeof( int ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorState", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorZip", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorPhone", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorFax", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanAdministratorEmail", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "PlanNotes", typeof( string ) );
                dtActiveSBAPlans.Columns.Add( "Comments", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = sbaPlanIdColumn;

                // add the keys to the table
                dtActiveSBAPlans.PrimaryKey = primaryKeyColumns;

                dtActiveSBAPlans.Clear();

                // connect
                dbConnection.Open();

                // run
                daSBAPlans.Fill( dsSBAPlans, ActiveSBAPlansTableName );

                RowsReturned = dsSBAPlans.Tables[ ActiveSBAPlansTableName ].Rows.Count;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectActiveSBAPlans(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateContractSBAHeader( int contractId, string contractNumber, bool bSBAPlanExempt, int SBAPlanId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractSBAHeader
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@SBAPlanExempt bit,
                //@SBAPlanId int

                SqlCommand cmdUpdateContractSBAHeaderQuery = new SqlCommand( "UpdateContractSBAHeader", dbConnection );
                cmdUpdateContractSBAHeaderQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractSBAHeaderQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractSBAHeaderQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractSBAHeaderQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmSBAPlanExempt = new SqlParameter( "@SBAPlanExempt", SqlDbType.Bit );
                SqlParameter parmSBAPlanId = new SqlParameter( "@SBAPlanId", SqlDbType.Int );


                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmSBAPlanExempt.Value = bSBAPlanExempt;

                // a -1 is used internally to represent no plan.  
                if( SBAPlanId != -1 )
                {
                    parmSBAPlanId.Value = SBAPlanId;
                }
                else
                {
                    parmSBAPlanId.SqlValue = DBNull.Value;
                }

                cmdUpdateContractSBAHeaderQuery.Parameters.Add( parmContractId );
                cmdUpdateContractSBAHeaderQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractSBAHeaderQuery.Parameters.Add( parmSBAPlanExempt );
                cmdUpdateContractSBAHeaderQuery.Parameters.Add( parmSBAPlanId );

                // connect
                dbConnection.Open();

                cmdUpdateContractSBAHeaderQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractSBAHeader(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }



        public bool UpdateContractComments( int contractId, string contractNumber, string contractComment )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //UpdateContractComment
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractId int,
                //@ContractNumber nvarchar(20),
                //@ContractComment nvarchar(800)

                SqlCommand cmdUpdateContractCommentQuery = new SqlCommand( "UpdateContractComment", dbConnection );
                cmdUpdateContractCommentQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateContractCommentQuery.CommandTimeout = 30;

                AddStandardParameter( cmdUpdateContractCommentQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdUpdateContractCommentQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );

                SqlParameter parmContractComment = new SqlParameter( "@ContractComment", SqlDbType.NVarChar, 800 );
  
                parmContractId.Value = contractId;
                parmContractNumber.Value = contractNumber;
                parmContractComment.Value = contractComment;

                cmdUpdateContractCommentQuery.Parameters.Add( parmContractId );
                cmdUpdateContractCommentQuery.Parameters.Add( parmContractNumber );
                cmdUpdateContractCommentQuery.Parameters.Add( parmContractComment );

                // connect
                dbConnection.Open();

                cmdUpdateContractCommentQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.UpdateContractComments(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // returns the date representing the last time a report definition was modified on the report server
        // potentially used to expire the cache
        public bool GetLastModifiedReportDate( out DateTime LastModifiedReportDate )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataReader reader = null;     
            LastModifiedReportDate = DateTime.Now.AddDays( -1 );

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //GetLastModifiedReportDate
                //()

                //returns datetime

                SqlCommand cmdGetLastModifiedReportDateQuery = new SqlCommand( "select dbo.[GetLastModifiedReportDate]()", dbConnection );
                cmdGetLastModifiedReportDateQuery.CommandType = CommandType.Text;
                cmdGetLastModifiedReportDateQuery.CommandTimeout = 30;

                // connect
                dbConnection.Open();

                reader = cmdGetLastModifiedReportDateQuery.ExecuteReader();
                reader.Read();

                LastModifiedReportDate = ( DateTime )reader.GetValue( 0 );

            }
            catch( SqlException s )
            {
                ErrorMessage = String.Format( "The following sql exception was encountered in ContractDB.GetLastModifiedReportDate(): {0}", s.Message );
                bSuccess = false;
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetLastModifiedReportDate(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool ValidateCheck( string contractNumber, int contractId, int quarterId, string checkNumber, decimal checkAmount, ref bool bIsValidated, ref string validationMessage  )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // ValidateCheck
                //(
                //@UserLogin nvarchar(120),
                //@ContractNumber nvarchar(50),
                //@ContractId int,
                //@QuarterId int,
                //@CheckNumber nvarchar(50),
                //@CheckAmount  decimal(18,2),
                //@IsValidated bit OUTPUT,
                //@ValidationMessage nvarchar(300) OUTPUT

                SqlCommand cmdValidateCheckQuery = new SqlCommand( "ValidateCheck", dbConnection );
                cmdValidateCheckQuery.CommandType = CommandType.StoredProcedure;
                cmdValidateCheckQuery.CommandTimeout = 30;

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 20 );
               
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmContractId  = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmQuarterId  = new SqlParameter( "@QuarterId", SqlDbType.Int );
                SqlParameter parmCheckNumber = new SqlParameter( "@CheckNumber", SqlDbType.NVarChar, 50 );
                SqlParameter parmCheckAmount = new SqlParameter( "@CheckAmount", SqlDbType.Decimal, 24 );
              
                SqlParameter parmIsValidated   = new SqlParameter( "@IsValidated", SqlDbType.Bit );
                parmIsValidated.Direction = ParameterDirection.Output;
                
                SqlParameter parmValidationMessage = new SqlParameter( "@ValidationMessage", SqlDbType.NVarChar, 300 );
                parmValidationMessage.Direction = ParameterDirection.Output;

                parmUserLogin.Value = _userName;
                parmContractNumber.Value = contractNumber;
                parmContractId.Value = contractId;
                parmQuarterId.Value = quarterId;
                parmCheckNumber.Value = checkNumber;
                parmCheckAmount.Value = checkAmount;

                cmdValidateCheckQuery.Parameters.Add( parmUserLogin );
                cmdValidateCheckQuery.Parameters.Add( parmContractNumber );
                cmdValidateCheckQuery.Parameters.Add( parmContractId );
                cmdValidateCheckQuery.Parameters.Add( parmQuarterId );
                cmdValidateCheckQuery.Parameters.Add( parmCheckNumber );
                cmdValidateCheckQuery.Parameters.Add( parmCheckAmount );
                cmdValidateCheckQuery.Parameters.Add( parmIsValidated );
                cmdValidateCheckQuery.Parameters.Add( parmValidationMessage );

                // connect
                dbConnection.Open();

                cmdValidateCheckQuery.ExecuteNonQuery();

                bIsValidated = bool.Parse( cmdValidateCheckQuery.Parameters[ "@IsValidated" ].Value.ToString() );              
                validationMessage = cmdValidateCheckQuery.Parameters[ "@ValidationMessage" ].Value.ToString();
            }
            catch ( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.ValidateCheck(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if ( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );

        }

        //public const string SBAPlanAssociatedContractsTableName = "SBAPlanAssociatedContractsTable";

        //public bool SelectSBAPlanAssociatedContracts( string contractNumber, int sbaPlanId, ref DataSet dsSBAPlanAssociatedContracts )
        //{

        //    bool bSuccess = true;
        //    SqlConnection dbConnection = null;
        //    SqlDataAdapter daSBAPlanAssociatedContracts = null;

        //    try
        //    {
        //        dbConnection = new SqlConnection( ConnectionString );

        //        // set up the call to the stored procedure
        //        //SelectSBAAssociatedContracts
        //        //(
        //        //@CurrentUser uniqueidentifier,
        //        //@ContractNumber nvarchar(20),
        //        //@SBAPlanId int

        //        SqlCommand cmdSelectSBAAssociatedContracts = new SqlCommand( "SelectSBAAssociatedContracts", dbConnection );
        //        cmdSelectSBAAssociatedContracts.CommandType = CommandType.StoredProcedure;
        //        cmdSelectSBAAssociatedContracts.CommandTimeout = 30;


        //        SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
        //        SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
        //        SqlParameter parmSBAPlanId = new SqlParameter( "@SBAPlanId", SqlDbType.Int );

        //        parmCurrentUser.Value = _currentUserId;
        //        parmContractNumber.Value = contractNumber;
        //        parmSBAPlanId.Value = sbaPlanId;

        //        cmdSelectSBAAssociatedContracts.Parameters.Add( parmCurrentUser );
        //        cmdSelectSBAAssociatedContracts.Parameters.Add( parmContractNumber );
        //        cmdSelectSBAAssociatedContracts.Parameters.Add( parmSBAPlanId );

        //        // create a data adapter and dataset to 
        //        // run the query and hold the results
        //        daSBAPlanAssociatedContracts = new SqlDataAdapter();
        //        daSBAPlanAssociatedContracts.SelectCommand = cmdSelectSBAAssociatedContracts;

        //        dsSBAPlanAssociatedContracts = new DataSet( "SBAPlanAssociatedContracts" );
        //        DataTable dtSBAPlanAssociatedContracts = dsSBAPlanAssociatedContracts.Tables.Add( SBAPlanAssociatedContractsTableName );

        //        // add the common elements to the table
        //        DataColumn contractIdColumn = new DataColumn( "ContractId", typeof( int ) );

        //        dtSBAPlanAssociatedContracts.Columns.Add( contractIdColumn );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "ContractNumber", typeof( string ) );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "ExpirationDate", typeof( DateTime ) );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "CompletionDate", typeof( DateTime ) );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "ContractorName", typeof( string ) );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "EstimatedContractValue", typeof( decimal ) );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "ContractingOfficerName", typeof( string ) );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "ScheduleName", typeof( string ) );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "ScheduleNumber", typeof( int ) );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "IsActive", typeof( bool ) );
        //        dtSBAPlanAssociatedContracts.Columns.Add( "IsResponsible", typeof( bool ) );

        //        // create array of primary key columns
        //        DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
        //        primaryKeyColumns[ 0 ] = contractIdColumn;

        //        // add the keys to the table
        //        dtSBAPlanAssociatedContracts.PrimaryKey = primaryKeyColumns;

        //        dtSBAPlanAssociatedContracts.Clear();

        //        // connect
        //        dbConnection.Open();

        //        // run
        //        daSBAPlanAssociatedContracts.Fill( dsSBAPlanAssociatedContracts, SBAPlanAssociatedContractsTableName );

        //        RowsReturned = dsSBAPlanAssociatedContracts.Tables[ SBAPlanAssociatedContractsTableName ].Rows.Count;

        //    }
        //    catch( Exception ex )
        //    {
        //        ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectSBAPlanAssociatedContracts(): {0}", ex.Message );
        //        bSuccess = false;
        //    }
        //    finally
        //    {
        //        if( dbConnection != null )
        //            dbConnection.Close();
        //    }

        //    return ( bSuccess );
        //}

        #region ISerializable Members

        [SecurityCritical]
        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );

            info.AddValue( "CurrentUserId", _currentUserId );
            info.AddValue( "UserName", _userName );
            info.AddValue( "OldUserId", _oldUserId );
        }

        protected ContractDB( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            _currentUserId = ( Guid )info.GetValue( "CurrentUserId", typeof( Guid ) );
            _userName = info.GetString( "UserName" );
            _oldUserId = info.GetInt32( "OldUserId" );
            RestoreDelegatesAfterDeserialization( this, "ContractDB" );
        }
        #endregion

    }
}
