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

using VA.NAC.Logging;
using VA.NAC.Application.SharedObj;

namespace VA.NAC.NACCMBrowser.DBInterface
{
    // provides ado.net access to contract releated stored procedures in the NAC CM database
    public class ContractDBNext : DBCommon
    {
        private NACLog _log = new NACLog();

        private Guid _currentUserId = Guid.Empty;
        private string _userName = string.Empty;

        // constructor for retrieving monitor commands
        public ContractDBNext()
            : base()
        {
            _log.SetCategory( LogBase.Category.DB );
            _log.SetContext( "ContractDB", this.GetType() );
            _log.WriteLog( "Calling ContractDB() ctor 1", LogBase.Severity.InformLowLevel );
        }

        public ContractDBNext( Guid currentUserId, string userName )
            : base()
        {
            _log.SetCategory( LogBase.Category.DB );
            _log.SetContext( "ContractDB", this.GetType() );
            _log.WriteLog( "Calling ContractDB() ctor 2", LogBase.Severity.InformLowLevel );

            _currentUserId = currentUserId;
            _userName = userName;
        }

        /// <summary>
        /// Returns a list of contract headers. The dataset will vary based on the contractType.
        /// </summary>
        /// <param name="dsContractHeaders"></param>
        /// <param name="contractType"></param>
        /// <returns></returns>
        public bool SelectContractHeaders( ref DataSet dsContractHeaders, string contractType )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractHeaders = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractHeaders
                //(
                //@currentUser as uniqueidentifier,
                //@contractType as nchar(1)
                //)

                SqlCommand cmdSelectContractHeadersQuery = new SqlCommand( "SelectContractHeaders", dbConnection );
                cmdSelectContractHeadersQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractHeadersQuery.CommandTimeout = 30;
                
                SqlParameter parmUserId = new SqlParameter( "@currentUser", SqlDbType.UniqueIdentifier );
                parmUserId.Direction = ParameterDirection.Input;
                parmUserId.Value = _currentUserId;
                cmdSelectContractHeadersQuery.Parameters.Add( parmUserId );

                SqlParameter parmContractType = new SqlParameter( "@contractType", SqlDbType.NChar, 1 );
                parmContractType.Direction = ParameterDirection.Input;
                parmContractType.Value = contractType;
                cmdSelectContractHeadersQuery.Parameters.Add( parmContractType );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractHeaders = new SqlDataAdapter();
                daContractHeaders.SelectCommand = cmdSelectContractHeadersQuery;

                dsContractHeaders = new DataSet( "ContractHeaders" );
                DataTable dtContractHeaders = dsContractHeaders.Tables.Add( "ContractHeadersTable" );

                // add the common elements to the table
                DataColumn documentIdColumn = new DataColumn( "DocumentId", typeof( int ) );

                dtContractHeaders.Columns.Add( documentIdColumn );
                dtContractHeaders.Columns.Add( "ContractNumber", typeof( string ) );
                dtContractHeaders.Columns.Add( "ContractDescription", typeof( string ) );
                dtContractHeaders.Columns.Add( "ContractDetailsId", typeof( int ) );
                dtContractHeaders.Columns.Add( "AwardDate", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "EffectiveDate", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "ExpirationDate", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "CompletionDate", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "ContractingOfficerId", typeof( Guid ) );
                dtContractHeaders.Columns.Add( "DCreatedBy", typeof( string ) );
                dtContractHeaders.Columns.Add( "DCreationDate", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "DLastModifiedBy", typeof( string ) );
                dtContractHeaders.Columns.Add( "DLastModificationDate", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "ContractType", typeof( string ) );
                dtContractHeaders.Columns.Add( "ContractCategoryId", typeof( int ) );
                dtContractHeaders.Columns.Add( "ReleaseStatus", typeof( string ) );
                dtContractHeaders.Columns.Add( "EditingStatus", typeof( string ) );
                dtContractHeaders.Columns.Add( "MCreatedBy", typeof( string ) );
                dtContractHeaders.Columns.Add( "MCreationDate", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "MLastModifiedBy", typeof( string ) );
                dtContractHeaders.Columns.Add( "MLastModificationDate", typeof( DateTime ) );
                dtContractHeaders.Columns.Add( "VendorId", typeof( int ) );
                dtContractHeaders.Columns.Add( "CompanyNameOnContract", typeof( string ) );
                dtContractHeaders.Columns.Add( "SectionId", typeof( int ) );
                dtContractHeaders.Columns.Add( "DivisionId", typeof( int ) );

                // add the document type specific elements to the table
                if( contractType.CompareTo( "F" ) == 0 )
                {
                    dtContractHeaders.Columns.Add( "OfferId", typeof( int ) );
                    dtContractHeaders.Columns.Add( "FSSContractType", typeof( string ) );
                    dtContractHeaders.Columns.Add( "FCreatedBy", typeof( string ) );
                    dtContractHeaders.Columns.Add( "FCreationDate", typeof( DateTime ) );
                    dtContractHeaders.Columns.Add( "FLastModifiedBy", typeof( string ) );
                    dtContractHeaders.Columns.Add( "FLastModificationDate", typeof( DateTime ) );
                }
                else if( contractType.CompareTo( "N" ) == 0 )
                {
                    dtContractHeaders.Columns.Add( "NationalContractType", typeof( string ) );
                    dtContractHeaders.Columns.Add( "IsPrimeVendorProgram", typeof( bool ) );
                    dtContractHeaders.Columns.Add( "NCreatedBy", typeof( string ) );
                    dtContractHeaders.Columns.Add( "NCreationDate", typeof( DateTime ) );
                    dtContractHeaders.Columns.Add( "NLastModifiedBy", typeof( string ) );
                    dtContractHeaders.Columns.Add( "NLastModificationDate", typeof( DateTime ) );
                }
                else if( contractType.CompareTo( "A" ) == 0 )
                {
                    dtContractHeaders.Columns.Add( "FSSDocumentId", typeof( int ) );
                    dtContractHeaders.Columns.Add( "AgreementType", typeof( string ) );
                    dtContractHeaders.Columns.Add( "ACreatedBy", typeof( string ) );
                    dtContractHeaders.Columns.Add( "ACreationDate", typeof( DateTime ) );
                    dtContractHeaders.Columns.Add( "ALastModifiedBy", typeof( string ) );
                    dtContractHeaders.Columns.Add( "ALastModificationDate", typeof( DateTime ) );
                }
                else if( contractType.CompareTo( "T" ) == 0 )
                {
                    dtContractHeaders.Columns.Add( "ParentDocumentIdentifier", typeof( string ) );
                    dtContractHeaders.Columns.Add( "ParentDocumentSource", typeof( string ) );
                    dtContractHeaders.Columns.Add( "AgreementType", typeof( string ) );
                    dtContractHeaders.Columns.Add( "TCreatedBy", typeof( string ) );
                    dtContractHeaders.Columns.Add( "TCreationDate", typeof( DateTime ) );
                    dtContractHeaders.Columns.Add( "TLastModifiedBy", typeof( string ) );
                    dtContractHeaders.Columns.Add( "TLastModificationDate", typeof( DateTime ) );
                }

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = documentIdColumn;

                // add the keys to the table
                dtContractHeaders.PrimaryKey = primaryKeyColumns;

                dtContractHeaders.Clear();

                // connect
                dbConnection.Open();

                // run
                daContractHeaders.Fill( dsContractHeaders, "ContractHeadersTable" );

                RowsReturned = dsContractHeaders.Tables[ "ContractHeadersTable" ].Rows.Count;

                BytesReturned = GetByteCount( dsContractHeaders );
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


        public bool SelectTestNames( ref DataSet dsTestNames )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daNames = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectNames

                SqlCommand cmdSelectNamesQuery = new SqlCommand( "SelectNames", dbConnection );
                cmdSelectNamesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectNamesQuery.CommandTimeout = 30;

 
                // create a data adapter and dataset to 
                // run the query and hold the results
                daNames = new SqlDataAdapter();
                daNames.SelectCommand = cmdSelectNamesQuery;

                dsTestNames = new DataSet( "TestNames" );
                DataTable dtTestNames = dsTestNames.Tables.Add( "TestNamesTable" );

                DataColumn idColumn = new DataColumn( "Id", typeof( int ) );

                dtTestNames.Columns.Add( idColumn );
                dtTestNames.Columns.Add( "Name", typeof( string ) );
 

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = idColumn;

                // add the keys to the table
                dtTestNames.PrimaryKey = primaryKeyColumns;

                dtTestNames.Clear();

                // connect
                dbConnection.Open();

                // run
                daNames.Fill( dsTestNames, "TestNamesTable" );

                RowsReturned = dsTestNames.Tables[ "TestNamesTable" ].Rows.Count;

                BytesReturned = GetByteCount( dsTestNames );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectTestNames(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool InsertTestName( string aName )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //InsertTestName
                //(
                //@newName as nvarchar(50)
                //)

                SqlCommand cmdInsertNameQuery = new SqlCommand( "InsertTestName", dbConnection );
                cmdInsertNameQuery.CommandType = CommandType.StoredProcedure;
                cmdInsertNameQuery.CommandTimeout = 30;

                SqlParameter parmAName = new SqlParameter( "@newName", SqlDbType.NVarChar, 50 );
                parmAName.Direction = ParameterDirection.Input;
                parmAName.Value = aName;
                cmdInsertNameQuery.Parameters.Add( parmAName );

                // connect
                dbConnection.Open();

                cmdInsertNameQuery.ExecuteNonQuery();

  
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.InsertTestName(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns contract details for a particular contract.
        /// </summary>
        /// <param name="dsContractDetails"></param>
        /// <param name="contractDetailsId"></param>
        /// <returns></returns>
        public bool SelectContractDetails( ref DataSet dsContractDetails, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractDetails = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractDetails
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdSelectContractDetailsQuery = new SqlCommand( "SelectContractDetails", dbConnection );
                cmdSelectContractDetailsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractDetailsQuery.CommandTimeout = 30;
                
                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;
                
                cmdSelectContractDetailsQuery.Parameters.Add( parmContractDetailsId );
                
                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractDetails = new SqlDataAdapter();
                daContractDetails.SelectCommand = cmdSelectContractDetailsQuery;

                dsContractDetails = new DataSet( "ContractDetails" );
                DataTable dtContractDetails = dsContractDetails.Tables.Add( "ContractDetailsTable" );
                
                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsContractDetails.EnforceConstraints = false;

                DataColumn contractDetailsIdColumn = new DataColumn( "ContractDetailsId", typeof( int ) );

                dtContractDetails.Columns.Add( contractDetailsIdColumn );
                dtContractDetails.Columns.Add( "ContractNumber", typeof( string ) );
        //        dtContractDetails.Columns.Add( "ContractingOfficerId", typeof( Guid ) );
                dtContractDetails.Columns.Add( "PrimeVendorParticipation", typeof( bool ) );
                dtContractDetails.Columns.Add( "VendorContactNotes", typeof( string ) );
                dtContractDetails.Columns.Add( "VeteranSpecialStatus", typeof( int ) );
                dtContractDetails.Columns.Add( "BusinessSize", typeof( string ) );
                dtContractDetails.Columns.Add( "DisadvantagedBusiness", typeof( bool ) );
                dtContractDetails.Columns.Add( "Business8A", typeof( bool ) );
                dtContractDetails.Columns.Add( "WomanOwnedBusiness", typeof( bool ) );
                dtContractDetails.Columns.Add( "HubZoneBusiness", typeof( bool ) );
                dtContractDetails.Columns.Add( "TrackingCustomer", typeof( string ) );
                dtContractDetails.Columns.Add( "MinimumOrder", typeof( string ) );
                dtContractDetails.Columns.Add( "CreditCardAccepted", typeof( bool ) );
                dtContractDetails.Columns.Add( "HazardousMaterials", typeof( bool ) );
                dtContractDetails.Columns.Add( "IFFType", typeof( string ) );
                dtContractDetails.Columns.Add( "Ratio", typeof( string ) );
                dtContractDetails.Columns.Add( "VendorType", typeof( string ) );
                dtContractDetails.Columns.Add( "EstimatedContractValue", typeof( decimal ) );

                dtContractDetails.Columns.Add( "AwardDate", typeof( DateTime ) );
                dtContractDetails.Columns.Add( "EffectiveDate", typeof( DateTime ) );
                dtContractDetails.Columns.Add( "ExpirationDate", typeof( DateTime ) );
                dtContractDetails.Columns.Add( "CompletionDate", typeof( DateTime ) );

                dtContractDetails.Columns.Add( "TotalOptionYears", typeof( int ) );
                dtContractDetails.Columns.Add( "IsVADOD", typeof( bool ) );
                dtContractDetails.Columns.Add( "TerminatedByConvenience", typeof( bool ) );
                dtContractDetails.Columns.Add( "TerminatedByDefault", typeof( bool ) );
                dtContractDetails.Columns.Add( "OnGSAAdvantage", typeof( bool ) );
                dtContractDetails.Columns.Add( "SBAPlanExempt", typeof( bool ) );

                dtContractDetails.Columns.Add( "InsurancePolicyEffectiveDate", typeof( DateTime ) );
                dtContractDetails.Columns.Add( "InsurancePolicyExpirationDate", typeof( DateTime ) );

                dtContractDetails.Columns.Add( "ContractDescription", typeof( string ) );
                dtContractDetails.Columns.Add( "OrderingInstructions", typeof( string ) );

                dtContractDetails.Columns.Add( "DCreatedBy", typeof( string ) );
                dtContractDetails.Columns.Add( "DCreationDate", typeof( DateTime ) );
                dtContractDetails.Columns.Add( "DLastModifiedBy", typeof( string ) );
                dtContractDetails.Columns.Add( "DLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractDetailsIdColumn;

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
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectContractDetails(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns geographic coverage for a particular contract.
        /// </summary>
        /// <param name="dsGeographicCoverage"></param>
        /// <param name="contractDetailsId"></param>
        /// <returns></returns>
        public bool SelectGeographicCoverage( ref DataSet dsGeographicCoverage, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daGeographicCoverage = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectGeographicCoverage
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdSelectGeographicCoverageQuery = new SqlCommand( "SelectGeographicCoverage", dbConnection );
                cmdSelectGeographicCoverageQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectGeographicCoverageQuery.CommandTimeout = 30;
                
                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;
                
                cmdSelectGeographicCoverageQuery.Parameters.Add( parmContractDetailsId );
                
                // create a data adapter and dataset to 
                // run the query and hold the results
                daGeographicCoverage = new SqlDataAdapter();
                daGeographicCoverage.SelectCommand = cmdSelectGeographicCoverageQuery;

                dsGeographicCoverage = new DataSet( "GeographicCoverage" );
                DataTable dtGeographicCoverage = dsGeographicCoverage.Tables.Add( "GeographicCoverageTable" );

                // add the common elements to the table
                DataColumn geographicCoverageIdColumn = new DataColumn( "GeographicCoverageId", typeof( int ) );

                dtGeographicCoverage.Columns.Add( geographicCoverageIdColumn );
                dtGeographicCoverage.Columns.Add( "GeographicId", typeof( int ) );

                dtGeographicCoverage.Columns.Add( "ContractDetailsId", typeof( int ) );
                dtGeographicCoverage.Columns.Add( "GCreatedBy", typeof( string ) );
                dtGeographicCoverage.Columns.Add( "GCreationDate", typeof( DateTime ) );
                dtGeographicCoverage.Columns.Add( "GLastModifiedBy", typeof( string ) );
                dtGeographicCoverage.Columns.Add( "GLastModificationDate", typeof( DateTime ) );

                dtGeographicCoverage.Columns.Add( "Country", typeof( string ) );
                dtGeographicCoverage.Columns.Add( "DescriptionOfEntity", typeof( string ) );
                dtGeographicCoverage.Columns.Add( "Abbreviation", typeof( string ) );

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
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectGeographicCoverage(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns discount info for a particular contract.
        /// </summary>
        /// <param name="dsDiscounts"></param>
        /// <param name="contractDetailsId"></param>
        /// <returns></returns>
        public bool SelectDiscounts( ref DataSet dsDiscounts, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daDiscounts = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectDiscounts
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdSelectDiscountsQuery = new SqlCommand( "SelectDiscounts", dbConnection );
                cmdSelectDiscountsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectDiscountsQuery.CommandTimeout = 30;

                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;

                cmdSelectDiscountsQuery.Parameters.Add( parmContractDetailsId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daDiscounts = new SqlDataAdapter();
                daDiscounts.SelectCommand = cmdSelectDiscountsQuery;

                dsDiscounts = new DataSet( "Discounts" );
                DataTable dtDiscounts = dsDiscounts.Tables.Add( "DiscountsTable" );

                // add the common elements to the table
                DataColumn discountIdColumn = new DataColumn( "DiscountId", typeof( int ) );

                dtDiscounts.Columns.Add( discountIdColumn );
                dtDiscounts.Columns.Add( "DiscountType", typeof( string ) );
                dtDiscounts.Columns.Add( "DiscountDescription", typeof( string ) );
                dtDiscounts.Columns.Add( "ContractDetailsId", typeof( int ) );

                dtDiscounts.Columns.Add( "DCreatedBy", typeof( string ) );
                dtDiscounts.Columns.Add( "DCreationDate", typeof( DateTime ) );
                dtDiscounts.Columns.Add( "DLastModifiedBy", typeof( string ) );
                dtDiscounts.Columns.Add( "DLastModificationDate", typeof( DateTime ) );

                dtDiscounts.Columns.Add( "DiscountTypeDescription", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = discountIdColumn;

                // add the keys to the table
                dtDiscounts.PrimaryKey = primaryKeyColumns;

                dtDiscounts.Clear();

                // connect
                dbConnection.Open();

                // run
                daDiscounts.Fill( dsDiscounts, "DiscountsTable" );

                RowsReturned = dsDiscounts.Tables[ "DiscountsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectDiscounts(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns warrantyTerm info for a particular contract.
        /// </summary>
        /// <param name="dsWarrantyTerms"></param>
        /// <param name="contractDetailsId"></param>
        /// <returns></returns>
        public bool SelectWarrantyTerms( ref DataSet dsWarrantyTerms, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daWarrantyTerms = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectWarrantyTerms
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdSelectWarrantyTermsQuery = new SqlCommand( "SelectWarrantyTerms", dbConnection );
                cmdSelectWarrantyTermsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectWarrantyTermsQuery.CommandTimeout = 30;

                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;

                cmdSelectWarrantyTermsQuery.Parameters.Add( parmContractDetailsId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daWarrantyTerms = new SqlDataAdapter();
                daWarrantyTerms.SelectCommand = cmdSelectWarrantyTermsQuery;

                dsWarrantyTerms = new DataSet( "WarrantyTerms" );
                DataTable dtWarrantyTerms = dsWarrantyTerms.Tables.Add( "WarrantyTermsTable" );

                // add the common elements to the table
                DataColumn warrantyTermsIdColumn = new DataColumn( "WarrantyTermsId", typeof( int ) );

                dtWarrantyTerms.Columns.Add( warrantyTermsIdColumn );
                dtWarrantyTerms.Columns.Add( "WarrantyDuration", typeof( string ) );
                dtWarrantyTerms.Columns.Add( "WarrantyNotes", typeof( string ) );
                dtWarrantyTerms.Columns.Add( "ContractDetailsId", typeof( int ) );

                dtWarrantyTerms.Columns.Add( "WCreatedBy", typeof( string ) );
                dtWarrantyTerms.Columns.Add( "WCreationDate", typeof( DateTime ) );
                dtWarrantyTerms.Columns.Add( "WLastModifiedBy", typeof( string ) );
                dtWarrantyTerms.Columns.Add( "WLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = warrantyTermsIdColumn;

                // add the keys to the table
                dtWarrantyTerms.PrimaryKey = primaryKeyColumns;

                dtWarrantyTerms.Clear();

                // connect
                dbConnection.Open();

                // run
                daWarrantyTerms.Fill( dsWarrantyTerms, "WarrantyTermsTable" );

                RowsReturned = dsWarrantyTerms.Tables[ "WarrantyTermsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectWarrantyTerms(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        /// <summary>
        /// Returns returned goods policy for a particular contract.
        /// </summary>
        /// <param name="dsReturnedGoodsPolicy"></param>
        /// <param name="contractDetailsId"></param>
        /// <returns></returns>
        public bool SelectReturnedGoodsPolicy( ref DataSet dsReturnedGoodsPolicy, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daReturnedGoodsPolicy = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectReturnedGoodsPolicy
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdSelectReturnedGoodsPolicyQuery = new SqlCommand( "SelectReturnedGoodsPolicy", dbConnection );
                cmdSelectReturnedGoodsPolicyQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectReturnedGoodsPolicyQuery.CommandTimeout = 30;

                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;

                cmdSelectReturnedGoodsPolicyQuery.Parameters.Add( parmContractDetailsId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daReturnedGoodsPolicy = new SqlDataAdapter();
                daReturnedGoodsPolicy.SelectCommand = cmdSelectReturnedGoodsPolicyQuery;

                dsReturnedGoodsPolicy = new DataSet( "ReturnedGoodsPolicy" );
                DataTable dtReturnedGoodsPolicy = dsReturnedGoodsPolicy.Tables.Add( "ReturnedGoodsPolicyTable" );

                // add the common elements to the table
                DataColumn returnedGoodsPolicyIdColumn = new DataColumn( "ReturnedGoodsPolicyId", typeof( int ) );

                dtReturnedGoodsPolicy.Columns.Add( returnedGoodsPolicyIdColumn );
                dtReturnedGoodsPolicy.Columns.Add( "ReturnedGoodsPolicyType", typeof( string ) );
                dtReturnedGoodsPolicy.Columns.Add( "ReturnedGoodsPolicyDescription", typeof( string ) );
                dtReturnedGoodsPolicy.Columns.Add( "ContractDetailsId", typeof( int ) );

                dtReturnedGoodsPolicy.Columns.Add( "RCreatedBy", typeof( string ) );
                dtReturnedGoodsPolicy.Columns.Add( "RCreationDate", typeof( DateTime ) );
                dtReturnedGoodsPolicy.Columns.Add( "RLastModifiedBy", typeof( string ) );
                dtReturnedGoodsPolicy.Columns.Add( "RLastModificationDate", typeof( DateTime ) );

                dtReturnedGoodsPolicy.Columns.Add( "PolicyTypeDescription", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = returnedGoodsPolicyIdColumn;

                // add the keys to the table
                dtReturnedGoodsPolicy.PrimaryKey = primaryKeyColumns;

                dtReturnedGoodsPolicy.Clear();

                // connect
                dbConnection.Open();

                // run
                daReturnedGoodsPolicy.Fill( dsReturnedGoodsPolicy, "ReturnedGoodsPolicyTable" );

                RowsReturned = dsReturnedGoodsPolicy.Tables[ "ReturnedGoodsPolicyTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectReturnedGoodsPolicy(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns delivery terms for a particular contract.
        /// </summary>
        /// <param name="dsReturnedGoodsPolicy"></param>
        /// <param name="contractDetailsId"></param>
        /// <returns></returns>
        public bool SelectDeliveryTerms( ref DataSet dsDeliveryTerms, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daDeliveryTerms = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectDeliveryTerms
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdSelectDeliveryTermsQuery = new SqlCommand( "SelectDeliveryTerms", dbConnection );
                cmdSelectDeliveryTermsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectDeliveryTermsQuery.CommandTimeout = 30;

                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;

                cmdSelectDeliveryTermsQuery.Parameters.Add( parmContractDetailsId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daDeliveryTerms = new SqlDataAdapter();
                daDeliveryTerms.SelectCommand = cmdSelectDeliveryTermsQuery;

                dsDeliveryTerms = new DataSet( "DeliveryTerms" );
                DataTable dtDeliveryTerms = dsDeliveryTerms.Tables.Add( "DeliveryTermsTable" );

                // add the common elements to the table
                DataColumn deliveryTermsIdColumn = new DataColumn( "DeliveryTermsId", typeof( int ) );

                dtDeliveryTerms.Columns.Add( deliveryTermsIdColumn );
                dtDeliveryTerms.Columns.Add( "DeliveryType", typeof( string ) );
                dtDeliveryTerms.Columns.Add( "DescriptionOfDeliveryTerms", typeof( string ) );
                dtDeliveryTerms.Columns.Add( "ContractDetailsId", typeof( int ) );

                dtDeliveryTerms.Columns.Add( "TCreatedBy", typeof( string ) );
                dtDeliveryTerms.Columns.Add( "TCreationDate", typeof( DateTime ) );
                dtDeliveryTerms.Columns.Add( "TLastModifiedBy", typeof( string ) );
                dtDeliveryTerms.Columns.Add( "TLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = deliveryTermsIdColumn;

                // add the keys to the table
                dtDeliveryTerms.PrimaryKey = primaryKeyColumns;

                dtDeliveryTerms.Clear();

                // connect
                dbConnection.Open();

                // run
                daDeliveryTerms.Fill( dsDeliveryTerms, "DeliveryTermsTable" );

                RowsReturned = dsDeliveryTerms.Tables[ "DeliveryTermsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectDeliveryTerms(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns sales for a particular contract.
        /// </summary>
        /// <param name="dsSales"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool SelectSales( ref DataSet dsSales, int documentId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSales = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectSales
                //(
                //@@DocumentId int
                //)

                SqlCommand cmdSelectSalesQuery = new SqlCommand( "SelectSales", dbConnection );
                cmdSelectSalesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectSalesQuery.CommandTimeout = 30;

                SqlParameter parmDocumentId = new SqlParameter( "@DocumentId", SqlDbType.Int );
                parmDocumentId.Direction = ParameterDirection.Input;
                parmDocumentId.Value = documentId;

                cmdSelectSalesQuery.Parameters.Add( parmDocumentId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSales = new SqlDataAdapter();
                daSales.SelectCommand = cmdSelectSalesQuery;

                dsSales = new DataSet( "Sales" );
                DataTable dtSales = dsSales.Tables.Add( "SalesTable" );

                // add the common elements to the table
                DataColumn salesIdColumn = new DataColumn( "SalesId", typeof( int ) );

                dtSales.Columns.Add( salesIdColumn );
                dtSales.Columns.Add( "QuarterId", typeof( int ) );
                dtSales.Columns.Add( "DocumentId", typeof( int ) );

                dtSales.Columns.Add( "ItemCategoryId", typeof( int ) );
                dtSales.Columns.Add( "VASales", typeof( decimal ) );
                dtSales.Columns.Add( "OGASales", typeof( decimal ) );
         //       dtSales.Columns.Add( "LocalGovernmentSales", typeof( decimal ) );
                dtSales.Columns.Add( "Comments", typeof( string ) );

                dtSales.Columns.Add( "SCreatedBy", typeof( string ) );
                dtSales.Columns.Add( "SCreationDate", typeof( DateTime ) );
                dtSales.Columns.Add( "SLastModifiedBy", typeof( string ) );
                dtSales.Columns.Add( "SLastModificationDate", typeof( DateTime ) );

                dtSales.Columns.Add( "FiscalYear", typeof( int ) );
                dtSales.Columns.Add( "Quarter", typeof( int ) );
                dtSales.Columns.Add( "YearQuarterDescription", typeof( string ) );
                dtSales.Columns.Add( "StartDate", typeof( DateTime ) );
                dtSales.Columns.Add( "EndDate", typeof( DateTime ) );
                dtSales.Columns.Add( "CalendarYear", typeof( int ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = salesIdColumn;

                // add the keys to the table
                dtSales.PrimaryKey = primaryKeyColumns;

                dtSales.Clear();

                // connect
                dbConnection.Open();

                // run
                daSales.Fill( dsSales, "SalesTable" );

                RowsReturned = dsSales.Tables[ "SalesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectSales(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns payments for a particular contract.
        /// </summary>
        /// <param name="dsSales"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool SelectPayments( ref DataSet dsPayments, int documentId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daPayments = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectPayments
                //(
                //@@DocumentId int
                //)

                SqlCommand cmdSelectPaymentsQuery = new SqlCommand( "SelectPayments", dbConnection );
                cmdSelectPaymentsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectPaymentsQuery.CommandTimeout = 30;

                SqlParameter parmDocumentId = new SqlParameter( "@DocumentId", SqlDbType.Int );
                parmDocumentId.Direction = ParameterDirection.Input;
                parmDocumentId.Value = documentId;

                cmdSelectPaymentsQuery.Parameters.Add( parmDocumentId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daPayments = new SqlDataAdapter();
                daPayments.SelectCommand = cmdSelectPaymentsQuery;

                dsPayments = new DataSet( "Payments" );
                DataTable dtPayments = dsPayments.Tables.Add( "PaymentsTable" );

                // add the common elements to the table
                DataColumn paymentIdColumn = new DataColumn( "PaymentId", typeof( int ) );

                dtPayments.Columns.Add( paymentIdColumn );
                dtPayments.Columns.Add( "QuarterId", typeof( int ) );
                dtPayments.Columns.Add( "DocumentId", typeof( int ) );
                dtPayments.Columns.Add( "Ordinality", typeof( int ) );

                dtPayments.Columns.Add( "CheckAmount", typeof( decimal ) );
                dtPayments.Columns.Add( "CheckNumber", typeof( string ) );
                dtPayments.Columns.Add( "DepositNumber", typeof( string ) );
                dtPayments.Columns.Add( "DateReceived", typeof( DateTime ) );
                dtPayments.Columns.Add( "Comments", typeof( string ) );

                dtPayments.Columns.Add( "PCreatedBy", typeof( string ) );
                dtPayments.Columns.Add( "PCreationDate", typeof( DateTime ) );
                dtPayments.Columns.Add( "PLastModifiedBy", typeof( string ) );
                dtPayments.Columns.Add( "PLastModificationDate", typeof( DateTime ) );

                dtPayments.Columns.Add( "FiscalYear", typeof( int ) );
                dtPayments.Columns.Add( "Quarter", typeof( int ) );
                dtPayments.Columns.Add( "YearQuarterDescription", typeof( string ) );
                dtPayments.Columns.Add( "StartDate", typeof( DateTime ) );
                dtPayments.Columns.Add( "EndDate", typeof( DateTime ) );
                dtPayments.Columns.Add( "CalendarYear", typeof( int ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = paymentIdColumn;

                // add the keys to the table
                dtPayments.PrimaryKey = primaryKeyColumns;

                dtPayments.Clear();

                // connect
                dbConnection.Open();

                // run
                daPayments.Fill( dsPayments, "PaymentsTable" );

                RowsReturned = dsPayments.Tables[ "PaymentsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectPayments(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns a list of defined year/quarter info.
        /// </summary>
        /// <param name="dsYearQuarter"></param>
        /// <returns></returns>
        public bool SelectYearQuarter( ref DataSet dsYearQuarter )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daYearQuarter = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectYearQuarter
                //(
                //)

                SqlCommand cmdSelectYearQuarterQuery = new SqlCommand( "SelectYearQuarter", dbConnection );
                cmdSelectYearQuarterQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectYearQuarterQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daYearQuarter = new SqlDataAdapter();
                daYearQuarter.SelectCommand = cmdSelectYearQuarterQuery;

                dsYearQuarter = new DataSet( "YearQuarter" );
                DataTable dtYearQuarter = dsYearQuarter.Tables.Add( "YearQuarterTable" );

                DataColumn quarterIdColumn = new DataColumn( "QuarterId", typeof( int ) );

                dtYearQuarter.Columns.Add( quarterIdColumn );

                dtYearQuarter.Columns.Add( "FiscalYear", typeof( int ) );
                dtYearQuarter.Columns.Add( "Quarter", typeof( int ) );
                dtYearQuarter.Columns.Add( "YearQuarterDescription", typeof( string ) );
                dtYearQuarter.Columns.Add( "StartDate", typeof( DateTime ) );
                dtYearQuarter.Columns.Add( "EndDate", typeof( DateTime ) );
                dtYearQuarter.Columns.Add( "CalendarYear", typeof( int ) );

                dtYearQuarter.Columns.Add( "YCreatedBy", typeof( string ) );
                dtYearQuarter.Columns.Add( "YCreationDate", typeof( DateTime ) );
                dtYearQuarter.Columns.Add( "YLastModifiedBy", typeof( string ) );
                dtYearQuarter.Columns.Add( "YLastModificationDate", typeof( DateTime ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = quarterIdColumn;

                // add the keys to the table
                dtYearQuarter.PrimaryKey = primaryKeyColumns;

                dtYearQuarter.Clear();

                // connect
                dbConnection.Open();

                // run
                daYearQuarter.Fill( dsYearQuarter, "YearQuarterTable" );

                RowsReturned = dsYearQuarter.Tables[ "YearQuarterTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectYearQuarter(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns a list of defined divisions
        /// </summary>
        /// <param name="dsDivisions"></param>
        /// <returns></returns>
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
                //)

                SqlCommand cmdSelectDivisionsQuery = new SqlCommand( "SelectDivisions", dbConnection );
                cmdSelectDivisionsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectDivisionsQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daDivisions = new SqlDataAdapter();
                daDivisions.SelectCommand = cmdSelectDivisionsQuery;

                dsDivisions = new DataSet( "Divisions" );
                DataTable dtDivisions = dsDivisions.Tables.Add( "DivisionsTable" );

                DataColumn divisionIdColumn = new DataColumn( "DivisionId", typeof( int ) );

                dtDivisions.Columns.Add( divisionIdColumn );

                dtDivisions.Columns.Add( "DivisionDescription", typeof( string ) );

                dtDivisions.Columns.Add( "VCreatedBy", typeof( string ) );
                dtDivisions.Columns.Add( "VCreationDate", typeof( DateTime ) );
                dtDivisions.Columns.Add( "VLastModifiedBy", typeof( string ) );
                dtDivisions.Columns.Add( "VLastModificationDate", typeof( DateTime ) );


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

        /// <summary>
        /// Returns a list of defined sections
        /// </summary>
        /// <param name="dsSections"></param>
        /// <returns></returns>
        public bool SelectSections( ref DataSet dsSections )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSections = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectSections
                //(
                //)

                SqlCommand cmdSelectSectionsQuery = new SqlCommand( "SelectSections", dbConnection );
                cmdSelectSectionsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectSectionsQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSections = new SqlDataAdapter();
                daSections.SelectCommand = cmdSelectSectionsQuery;

                dsSections = new DataSet( "Sections" );
                DataTable dtSections = dsSections.Tables.Add( "SectionsTable" );

                DataColumn sectionIdColumn = new DataColumn( "SectionId", typeof( int ) );

                dtSections.Columns.Add( sectionIdColumn );
                dtSections.Columns.Add( "DivisionId", typeof( int ) );
                dtSections.Columns.Add( "SectionDescription", typeof( string ) );

                dtSections.Columns.Add( "SCreatedBy", typeof( string ) );
                dtSections.Columns.Add( "SCreationDate", typeof( DateTime ) );
                dtSections.Columns.Add( "SLastModifiedBy", typeof( string ) );
                dtSections.Columns.Add( "SLastModificationDate", typeof( DateTime ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = sectionIdColumn;

                // add the keys to the table
                dtSections.PrimaryKey = primaryKeyColumns;

                dtSections.Clear();

                // connect
                dbConnection.Open();

                // run
                daSections.Fill( dsSections, "SectionsTable" );

                RowsReturned = dsSections.Tables[ "SectionsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectSections(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns a list of defined contract categories
        /// </summary>
        /// <param name="dsSections"></param>
        /// <returns></returns>
        public bool SelectContractCategories( ref DataSet dsContractCategories )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractCategories = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractCategories
                //(
                //)

                SqlCommand cmdSelectContractCategoriesQuery = new SqlCommand( "SelectContractCategories", dbConnection );
                cmdSelectContractCategoriesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractCategoriesQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractCategories = new SqlDataAdapter();
                daContractCategories.SelectCommand = cmdSelectContractCategoriesQuery;

                dsContractCategories = new DataSet( "ContractCategories" );
                DataTable dtContractCategories = dsContractCategories.Tables.Add( "ContractCategoriesTable" );

                DataColumn contractCategoryIdColumn = new DataColumn( "ContractCategoryId", typeof( int ) );


                dtContractCategories.Columns.Add( contractCategoryIdColumn );
                dtContractCategories.Columns.Add( "CategoryName", typeof( string ) );
                dtContractCategories.Columns.Add( "Elibrary", typeof( string ) );
                dtContractCategories.Columns.Add( "FPDSNAISCode", typeof( string ) );
                dtContractCategories.Columns.Add( "FPDSServiceCode", typeof( string ) );
                dtContractCategories.Columns.Add( "ActiveStatus", typeof( bool ) );
                dtContractCategories.Columns.Add( "MSPricelistSched", typeof( bool ) );
                dtContractCategories.Columns.Add( "SectionId", typeof( int ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractCategoryIdColumn;

                // add the keys to the table
                dtContractCategories.PrimaryKey = primaryKeyColumns;

                dtContractCategories.Clear();

                // connect
                dbConnection.Open();

                // run
                daContractCategories.Fill( dsContractCategories, "ContractCategoriesTable" );

                RowsReturned = dsContractCategories.Tables[ "ContractCategoriesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectContractCategories(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the item category summary for a particular contract.
        /// </summary>
        /// <param name="dsContractItemCategorySummary"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool SelectContractItemCategorySummary( ref DataSet dsContractItemCategorySummary, int documentId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractItemCategorySummary = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractItemCategorySummary
                //(
                //@@DocumentId int
                //)

                SqlCommand cmdSelectContractItemCategorySummaryQuery = new SqlCommand( "SelectContractItemCategorySummary", dbConnection );
                cmdSelectContractItemCategorySummaryQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractItemCategorySummaryQuery.CommandTimeout = 30;

                SqlParameter parmDocumentId = new SqlParameter( "@DocumentId", SqlDbType.Int );
                parmDocumentId.Direction = ParameterDirection.Input;
                parmDocumentId.Value = documentId;

                cmdSelectContractItemCategorySummaryQuery.Parameters.Add( parmDocumentId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractItemCategorySummary = new SqlDataAdapter();
                daContractItemCategorySummary.SelectCommand = cmdSelectContractItemCategorySummaryQuery;

                dsContractItemCategorySummary = new DataSet( "ContractItemCategorySummary" );
                DataTable dtContractItemCategorySummary = dsContractItemCategorySummary.Tables.Add( "ContractItemCategorySummaryTable" );

                DataColumn contractItemCategorySummaryIdColumn = new DataColumn( "ContractItemCategorySummaryId", typeof( int ) );

                dtContractItemCategorySummary.Columns.Add( contractItemCategorySummaryIdColumn );
                dtContractItemCategorySummary.Columns.Add( "ItemCategoryId", typeof( int ) );

                dtContractItemCategorySummary.Columns.Add( "Recoverable", typeof( bool ) );
                dtContractItemCategorySummary.Columns.Add( "DocumentId", typeof( int ) );
                dtContractItemCategorySummary.Columns.Add( "ContractCategoryId", typeof( int ) );
                dtContractItemCategorySummary.Columns.Add( "SIN", typeof( string ) );
                dtContractItemCategorySummary.Columns.Add( "Description", typeof( string ) );

                dtContractItemCategorySummary.Columns.Add( "FromOldDB", typeof( bool ) );
                dtContractItemCategorySummary.Columns.Add( "Inactive", typeof( bool ) );
                dtContractItemCategorySummary.Columns.Add( "Adhoc", typeof( bool ) );



                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractItemCategorySummaryIdColumn;

                // add the keys to the table
                dtContractItemCategorySummary.PrimaryKey = primaryKeyColumns;

                dtContractItemCategorySummary.Clear();

                // connect
                dbConnection.Open();

                // run
                daContractItemCategorySummary.Fill( dsContractItemCategorySummary, "ContractItemCategorySummaryTable" );

                RowsReturned = dsContractItemCategorySummary.Tables[ "ContractItemCategorySummaryTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectContractItemCategorySummary(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the item categories for a particular category.
        /// </summary>
        /// <param name="dsItemCategories"></param>
        /// <param name="contractCategoryId"> a -1 indicates select all</param>
        /// <returns></returns>
        public bool SelectItemCategories( ref DataSet dsItemCategories, int contractCategoryId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItemCategories = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectItemCategories
                //(
                //@ContractCategoryId int
                //)

                SqlCommand cmdSelectItemCategoriesQuery = new SqlCommand( "SelectItemCategories", dbConnection );
                cmdSelectItemCategoriesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectItemCategoriesQuery.CommandTimeout = 30;

                SqlParameter parmContractCategoryId = new SqlParameter( "@ContractCategoryId", SqlDbType.Int );
                parmContractCategoryId.Direction = ParameterDirection.Input;
                parmContractCategoryId.Value = contractCategoryId;

                cmdSelectItemCategoriesQuery.Parameters.Add( parmContractCategoryId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItemCategories = new SqlDataAdapter();
                daItemCategories.SelectCommand = cmdSelectItemCategoriesQuery;

                dsItemCategories = new DataSet( "ItemCategories" );
                DataTable dtItemCategories = dsItemCategories.Tables.Add( "ItemCategoriesTable" );

                DataColumn itemCategoryIdColumn = new DataColumn( "ItemCategoryId", typeof( int ) );

                dtItemCategories.Columns.Add( itemCategoryIdColumn );
                dtItemCategories.Columns.Add( "ContractCategoryId", typeof( int ) );

                dtItemCategories.Columns.Add( "SIN", typeof( string ) );
                dtItemCategories.Columns.Add( "Description", typeof( string ) );

                dtItemCategories.Columns.Add( "FromOldDB", typeof( bool ) );
                dtItemCategories.Columns.Add( "Inactive", typeof( bool ) );
                dtItemCategories.Columns.Add( "Adhoc", typeof( bool ) );



                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = itemCategoryIdColumn;

                // add the keys to the table
                dtItemCategories.PrimaryKey = primaryKeyColumns;

                dtItemCategories.Clear();

                // connect
                dbConnection.Open();

                // run
                daItemCategories.Fill( dsItemCategories, "ItemCategoriesTable" );

                RowsReturned = dsItemCategories.Tables[ "ItemCategoriesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectItemCategories(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the item list header record for a particular contract.
        /// </summary>
        /// <param name="dsItemList"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool GetItemList( ref DataSet dsItemList, int documentId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItemList = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetItemList
                //(
                //@@DocumentId int
                //)

                SqlCommand cmdGetItemListQuery = new SqlCommand( "GetItemList", dbConnection );
                cmdGetItemListQuery.CommandType = CommandType.StoredProcedure;
                cmdGetItemListQuery.CommandTimeout = 30;

                SqlParameter parmDocumentId = new SqlParameter( "@DocumentId", SqlDbType.Int );
                parmDocumentId.Direction = ParameterDirection.Input;
                parmDocumentId.Value = documentId;

                cmdGetItemListQuery.Parameters.Add( parmDocumentId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItemList = new SqlDataAdapter();
                daItemList.SelectCommand = cmdGetItemListQuery;

                dsItemList = new DataSet( "ItemList" );
                DataTable dtItemList = dsItemList.Tables.Add( "ItemListTable" );

                DataColumn itemListIdColumn = new DataColumn( "ItemListId", typeof( int ) );

                dtItemList.Columns.Add( itemListIdColumn );
                dtItemList.Columns.Add( "VerifiedBy", typeof( string ) );
                dtItemList.Columns.Add( "VerificationDate", typeof( DateTime ) );
                dtItemList.Columns.Add( "Notes", typeof( string ) );
                dtItemList.Columns.Add( "DocumentId", typeof( int ) );

                dtItemList.Columns.Add( "LCreatedBy", typeof( string ) );
                dtItemList.Columns.Add( "LCreationDate", typeof( DateTime ) );
                dtItemList.Columns.Add( "LLastModifiedBy", typeof( string ) );
                dtItemList.Columns.Add( "LLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = itemListIdColumn;

                // add the keys to the table
                dtItemList.PrimaryKey = primaryKeyColumns;

                dtItemList.Clear();

                // connect
                dbConnection.Open();

                // run
                daItemList.Fill( dsItemList, "ItemListTable" );

                RowsReturned = dsItemList.Tables[ "ItemListTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetItemList(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the items for a particular contract.
        /// </summary>
        /// <param name="dsItems"></param>
        /// <param name="itemListId"></param>
        /// <returns></returns>
        public bool SelectContractItems( ref DataSet dsItems, int itemListId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItems = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractItems
                //(
                //@@ItemListId int
                //)

                SqlCommand cmdSelectItemsQuery = new SqlCommand( "SelectContractItems", dbConnection );
                cmdSelectItemsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectItemsQuery.CommandTimeout = 30;

                SqlParameter parmItemListId = new SqlParameter( "@ItemListId", SqlDbType.Int );
                parmItemListId.Direction = ParameterDirection.Input;
                parmItemListId.Value = itemListId;

                cmdSelectItemsQuery.Parameters.Add( parmItemListId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItems = new SqlDataAdapter();
                daItems.SelectCommand = cmdSelectItemsQuery;

                dsItems = new DataSet( "Items" );
                DataTable dtItems = dsItems.Tables.Add( "ItemsTable" );

                // this was added to suppress an erroneous constraint error
                // there are no ado implemented constraints and the error was on a simple select
                dsItems.EnforceConstraints = false;

                DataColumn contractItemIdColumn = new DataColumn( "ContractItemId", typeof( int ) );

                dtItems.Columns.Add( contractItemIdColumn );
                dtItems.Columns.Add( "ItemListId", typeof( int ) );

                dtItems.Columns.Add( "ContractorCatalogNumber", typeof( string ) );
                dtItems.Columns.Add( "ContractItemDescription", typeof( string ) );
                dtItems.Columns.Add( "ContractPrice", typeof( decimal ) );
                dtItems.Columns.Add( "PackageSizePricedOnContract", typeof( string ) );

                dtItems.Columns.Add( "EffectiveDate", typeof( DateTime ) );
                dtItems.Columns.Add( "EndDate", typeof( DateTime ) );

                dtItems.Columns.Add( "ICreatedBy", typeof( string ) );
                dtItems.Columns.Add( "ICreationDate", typeof( DateTime ) );
                dtItems.Columns.Add( "ILastModifiedBy", typeof( string ) );
                dtItems.Columns.Add( "ILastModificationDate", typeof( DateTime ) );

                dtItems.Columns.Add( "OldLogNumber", typeof( int ) );
                dtItems.Columns.Add( "ContractItemItemCategoryId", typeof( int ) );
                dtItems.Columns.Add( "ContractCategoryId", typeof( int ) );

                dtItems.Columns.Add( "SIN", typeof( string ) );
                dtItems.Columns.Add( "Description", typeof( string ) );

                dtItems.Columns.Add( "FromOldDB", typeof( bool ) );
                dtItems.Columns.Add( "Inactive", typeof( bool ) );
                dtItems.Columns.Add( "Adhoc", typeof( bool ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractItemIdColumn;

                // add the keys to the table
                dtItems.PrimaryKey = primaryKeyColumns;

                dtItems.Clear();

                // connect
                dbConnection.Open();

                // run
                daItems.Fill( dsItems, "ItemsTable" );

                RowsReturned = dsItems.Tables[ "ItemsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectContractItems(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the items for a particular agreement.
        /// </summary>
        /// <param name="dsItems"></param>
        /// <param name="itemListId"></param>
        /// <returns></returns>
        public bool SelectAgreementItems( ref DataSet dsItems, int itemListId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItems = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectAgreementItems
                //(
                //@@ItemListId int
                //)

                SqlCommand cmdSelectAgreementItemsQuery = new SqlCommand( "SelectAgreementItems", dbConnection );
                cmdSelectAgreementItemsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectAgreementItemsQuery.CommandTimeout = 30;

                SqlParameter parmItemListId = new SqlParameter( "@ItemListId", SqlDbType.Int );
                parmItemListId.Direction = ParameterDirection.Input;
                parmItemListId.Value = itemListId;

                cmdSelectAgreementItemsQuery.Parameters.Add( parmItemListId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItems = new SqlDataAdapter();
                daItems.SelectCommand = cmdSelectAgreementItemsQuery;

                dsItems = new DataSet( "Items" );
                DataTable dtItems = dsItems.Tables.Add( "ItemsTable" );

                DataColumn agreementItemIdColumn = new DataColumn( "AgreementItemId", typeof( int ) );

                dtItems.Columns.Add( agreementItemIdColumn );
                dtItems.Columns.Add( "ItemListId", typeof( int ) );

                dtItems.Columns.Add( "AgreementItemDescription", typeof( string ) );
                dtItems.Columns.Add( "AgreementPrice", typeof( decimal ) );

                dtItems.Columns.Add( "EffectiveDate", typeof( DateTime ) );
                dtItems.Columns.Add( "EndDate", typeof( DateTime ) );

                dtItems.Columns.Add( "ContractItemId", typeof( int ) );
                dtItems.Columns.Add( "OldLogNumber", typeof( int ) );

                dtItems.Columns.Add( "ACreatedBy", typeof( string ) );
                dtItems.Columns.Add( "ACreationDate", typeof( DateTime ) );
                dtItems.Columns.Add( "ALastModifiedBy", typeof( string ) );
                dtItems.Columns.Add( "ALastModificationDate", typeof( DateTime ) );

                /* from parent item */
                dtItems.Columns.Add( "ContractorCatalogNumber", typeof( string ) );
                dtItems.Columns.Add( "ContractItemDescription", typeof( string ) );
                dtItems.Columns.Add( "ContractPrice", typeof( decimal ) );
                dtItems.Columns.Add( "PackageSizePricedOnContract", typeof( string ) );
                dtItems.Columns.Add( "ParentItemEffectiveDate", typeof( DateTime ) );
                dtItems.Columns.Add( "ParentItemEndDate", typeof( DateTime ) );

                /* parent item sin information */
                dtItems.Columns.Add( "ContractItemItemCategoryId", typeof( int ) );
                dtItems.Columns.Add( "ContractCategoryId", typeof( int ) );

                dtItems.Columns.Add( "SIN", typeof( string ) );
                dtItems.Columns.Add( "Description", typeof( string ) );

                dtItems.Columns.Add( "FromOldDB", typeof( bool ) );
                dtItems.Columns.Add( "Inactive", typeof( bool ) );
                dtItems.Columns.Add( "Adhoc", typeof( bool ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = agreementItemIdColumn;

                // add the keys to the table
                dtItems.PrimaryKey = primaryKeyColumns;

                dtItems.Clear();

                // connect
                dbConnection.Open();

                // run
                daItems.Fill( dsItems, "ItemsTable" );

                RowsReturned = dsItems.Tables[ "ItemsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectAgreementItems(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the items for a particular non-standard agreement.
        /// </summary>
        /// <param name="dsItems"></param>
        /// <param name="itemListId"></param>
        /// <returns></returns>
        public bool SelectNonStandardAgreementItems( ref DataSet dsItems, int itemListId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItems = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectNonStandardAgreementItems
                //(
                //@@ItemListId int
                //)

                SqlCommand cmdSelectNonStandardAgreementItemsQuery = new SqlCommand( "SelectNonStandardAgreementItems", dbConnection );
                cmdSelectNonStandardAgreementItemsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectNonStandardAgreementItemsQuery.CommandTimeout = 30;

                SqlParameter parmItemListId = new SqlParameter( "@ItemListId", SqlDbType.Int );
                parmItemListId.Direction = ParameterDirection.Input;
                parmItemListId.Value = itemListId;

                cmdSelectNonStandardAgreementItemsQuery.Parameters.Add( parmItemListId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItems = new SqlDataAdapter();
                daItems.SelectCommand = cmdSelectNonStandardAgreementItemsQuery;

                dsItems = new DataSet( "Items" );
                DataTable dtItems = dsItems.Tables.Add( "ItemsTable" );

                DataColumn agreementItemIdColumn = new DataColumn( "AgreementItemId", typeof( int ) );

                dtItems.Columns.Add( agreementItemIdColumn );
                dtItems.Columns.Add( "ItemListId", typeof( int ) );

                dtItems.Columns.Add( "AgreementItemDescription", typeof( string ) );
                dtItems.Columns.Add( "AgreementPrice", typeof( decimal ) );

                dtItems.Columns.Add( "EffectiveDate", typeof( DateTime ) );
                dtItems.Columns.Add( "EndDate", typeof( DateTime ) );

                dtItems.Columns.Add( "ContractItemId", typeof( int ) );
                dtItems.Columns.Add( "OldLogNumber", typeof( int ) );

                dtItems.Columns.Add( "ACreatedBy", typeof( string ) );
                dtItems.Columns.Add( "ACreationDate", typeof( DateTime ) );
                dtItems.Columns.Add( "ALastModifiedBy", typeof( string ) );
                dtItems.Columns.Add( "ALastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = agreementItemIdColumn;

                // add the keys to the table
                dtItems.PrimaryKey = primaryKeyColumns;

                dtItems.Clear();

                // connect
                dbConnection.Open();

                // run
                daItems.Fill( dsItems, "ItemsTable" );

                RowsReturned = dsItems.Tables[ "ItemsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectNonStandardAgreementItems(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the tiered price info for a particular item.
        /// </summary>
        /// <param name="dsItemTieredPrice"></param>
        /// <param name="contractItemId"></param>
        /// <returns></returns>
        public bool GetItemTieredPrice( ref DataSet dsItemTieredPrice, int contractItemId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItemTieredPrice = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetItemTieredPrice
                //(
                //@ContractItemId int
                //)

                SqlCommand cmdGetItemTieredPriceQuery = new SqlCommand( "GetItemTieredPrice", dbConnection );
                cmdGetItemTieredPriceQuery.CommandType = CommandType.StoredProcedure;
                cmdGetItemTieredPriceQuery.CommandTimeout = 30;

                SqlParameter parmContractItemId = new SqlParameter( "@ContractItemId", SqlDbType.Int );
                parmContractItemId.Direction = ParameterDirection.Input;
                parmContractItemId.Value = contractItemId;

                cmdGetItemTieredPriceQuery.Parameters.Add( parmContractItemId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItemTieredPrice = new SqlDataAdapter();
                daItemTieredPrice.SelectCommand = cmdGetItemTieredPriceQuery;

                dsItemTieredPrice = new DataSet( "ItemTieredPrice" );
                DataTable dtItemTieredPrice = dsItemTieredPrice.Tables.Add( "ItemTieredPriceTable" );

                DataColumn tieredPriceIdColumn = new DataColumn( "TieredPriceId", typeof( int ) );

                dtItemTieredPrice.Columns.Add( tieredPriceIdColumn );

                dtItemTieredPrice.Columns.Add( "ContractItemId", typeof( int ) );
                dtItemTieredPrice.Columns.Add( "EffectiveDate", typeof( DateTime ) );
                dtItemTieredPrice.Columns.Add( "EndDate", typeof( DateTime ) );
                dtItemTieredPrice.Columns.Add( "TierOnePrice", typeof( decimal ) );
                dtItemTieredPrice.Columns.Add( "TierTwoPrice", typeof( decimal ) );
                dtItemTieredPrice.Columns.Add( "TierThreePrice", typeof( decimal ) );
                dtItemTieredPrice.Columns.Add( "TierFourPrice", typeof( decimal ) );
                dtItemTieredPrice.Columns.Add( "TierFivePrice", typeof( decimal ) );

                dtItemTieredPrice.Columns.Add( "TierOneNote", typeof( string ) );
                dtItemTieredPrice.Columns.Add( "TierTwoNote", typeof( string ) );
                dtItemTieredPrice.Columns.Add( "TierThreeNote", typeof( string ) );
                dtItemTieredPrice.Columns.Add( "TierFourNote", typeof( string ) );
                dtItemTieredPrice.Columns.Add( "TierFiveNote", typeof( string ) );

                dtItemTieredPrice.Columns.Add( "TCreatedBy", typeof( string ) );
                dtItemTieredPrice.Columns.Add( "TCreationDate", typeof( DateTime ) );
                dtItemTieredPrice.Columns.Add( "TLastModifiedBy", typeof( string ) );
                dtItemTieredPrice.Columns.Add( "TLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = tieredPriceIdColumn;

                // add the keys to the table
                dtItemTieredPrice.PrimaryKey = primaryKeyColumns;

                dtItemTieredPrice.Clear();

                // connect
                dbConnection.Open();

                // run
                daItemTieredPrice.Fill( dsItemTieredPrice, "ItemTieredPriceTable" );

                RowsReturned = dsItemTieredPrice.Tables[ "ItemTieredPriceTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetItemTieredPrice(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the package info for a particular item.
        /// </summary>
        /// <param name="dsItemPackage"></param>
        /// <param name="contractItemId"></param>
        /// <returns></returns>
        public bool GetItemPackage( ref DataSet dsItemPackage, int contractItemId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItemPackage = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetItemPackage
                //(
                //@ContractItemId int
                //)

                SqlCommand cmdGetItemPackageQuery = new SqlCommand( "GetItemPackage", dbConnection );
                cmdGetItemPackageQuery.CommandType = CommandType.StoredProcedure;
                cmdGetItemPackageQuery.CommandTimeout = 30;

                SqlParameter parmContractItemId = new SqlParameter( "@ContractItemId", SqlDbType.Int );
                parmContractItemId.Direction = ParameterDirection.Input;
                parmContractItemId.Value = contractItemId;

                cmdGetItemPackageQuery.Parameters.Add( parmContractItemId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItemPackage = new SqlDataAdapter();
                daItemPackage.SelectCommand = cmdGetItemPackageQuery;

                dsItemPackage = new DataSet( "ItemPackage" );
                DataTable dtItemPackage = dsItemPackage.Tables.Add( "ItemPackageTable" );

                DataColumn itemPackageIdColumn = new DataColumn( "ItemPackageId", typeof( int ) );

                dtItemPackage.Columns.Add( itemPackageIdColumn );

                dtItemPackage.Columns.Add( "ContractItemId", typeof( int ) );
                dtItemPackage.Columns.Add( "EffectiveDate", typeof( DateTime ) );
                dtItemPackage.Columns.Add( "EndDate", typeof( DateTime ) );

                dtItemPackage.Columns.Add( "OuterPackUOM", typeof( string ) );
                dtItemPackage.Columns.Add( "OuterPackUnitConversionFactor", typeof( int ) );
                dtItemPackage.Columns.Add( "OuterPackUnitShippable", typeof( bool ) );
                dtItemPackage.Columns.Add( "OuterPackUPN", typeof( string ) );

                dtItemPackage.Columns.Add( "IntermediatePackUOM", typeof( string ) );
                dtItemPackage.Columns.Add( "IntermediatePackUnitConversionFactor", typeof( int ) );
                dtItemPackage.Columns.Add( "IntermediatePackUnitShippable", typeof( bool ) );
                dtItemPackage.Columns.Add( "IntermediatePackUPN", typeof( string ) );

                dtItemPackage.Columns.Add( "BasePackUOM", typeof( string ) );
                dtItemPackage.Columns.Add( "BasePackUnitConversionFactor", typeof( int ) );
                dtItemPackage.Columns.Add( "BasePackUnitShippable", typeof( bool ) );
                dtItemPackage.Columns.Add( "BasePackUPN", typeof( string ) );

                dtItemPackage.Columns.Add( "PCreatedBy", typeof( string ) );
                dtItemPackage.Columns.Add( "PCreationDate", typeof( DateTime ) );
                dtItemPackage.Columns.Add( "PLastModifiedBy", typeof( string ) );
                dtItemPackage.Columns.Add( "PLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = itemPackageIdColumn;

                // add the keys to the table
                dtItemPackage.PrimaryKey = primaryKeyColumns;

                dtItemPackage.Clear();

                // connect
                dbConnection.Open();

                // run
                daItemPackage.Fill( dsItemPackage, "ItemTieredPriceTable" );

                RowsReturned = dsItemPackage.Tables[ "ItemTieredPriceTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetItemTieredPrice(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns addresses for a particular contract.
        /// </summary>
        /// <param name="dsGeographicCoverage"></param>
        /// <param name="contractDetailsId"></param>
        /// <returns></returns>
        public bool SelectContractAddresses( ref DataSet dsContractAddresses, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractAddresses = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractAddresses
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdSelectContractAddressesQuery = new SqlCommand( "SelectContractAddresses", dbConnection );
                cmdSelectContractAddressesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractAddressesQuery.CommandTimeout = 30;

                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;

                cmdSelectContractAddressesQuery.Parameters.Add( parmContractDetailsId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractAddresses = new SqlDataAdapter();
                daContractAddresses.SelectCommand = cmdSelectContractAddressesQuery;

                dsContractAddresses = new DataSet( "ContractAddresses" );
                DataTable dtContractAddresses = dsContractAddresses.Tables.Add( "ContractAddressesTable" );

                // add the common elements to the table
                DataColumn contractAddressIdColumn = new DataColumn( "ContractAddressId", typeof( int ) );

                dtContractAddresses.Columns.Add( contractAddressIdColumn );
                dtContractAddresses.Columns.Add( "AddressId", typeof( int ) );

                dtContractAddresses.Columns.Add( "AddressType", typeof( string ) );
                dtContractAddresses.Columns.Add( "AddressTypeDescription", typeof( string ) );
                dtContractAddresses.Columns.Add( "AddressLine1", typeof( string ) );
                dtContractAddresses.Columns.Add( "AddressLine2", typeof( string ) );
                dtContractAddresses.Columns.Add( "City", typeof( string ) );
                dtContractAddresses.Columns.Add( "Zip", typeof( string ) );
                dtContractAddresses.Columns.Add( "ZipPlusFour", typeof( string ) );
                dtContractAddresses.Columns.Add( "StateProvinceCode", typeof( string ) );

                dtContractAddresses.Columns.Add( "Country", typeof( string ) );
                dtContractAddresses.Columns.Add( "WebPage", typeof( string ) );

                dtContractAddresses.Columns.Add( "ACreatedBy", typeof( string ) );
                dtContractAddresses.Columns.Add( "ACreationDate", typeof( DateTime ) );
                dtContractAddresses.Columns.Add( "ALastModifiedBy", typeof( string ) );
                dtContractAddresses.Columns.Add( "ALastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractAddressIdColumn;

                // add the keys to the table
                dtContractAddresses.PrimaryKey = primaryKeyColumns;

                dtContractAddresses.Clear();

                // connect
                dbConnection.Open();

                // run
                daContractAddresses.Fill( dsContractAddresses, "ContractAddressesTable" );

                RowsReturned = dsContractAddresses.Tables[ "ContractAddressesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectContractAddresses(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the vendor info for a particular contract.
        /// </summary>
        /// <param name="dsItemList"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool GetContractVendor( ref DataSet dsContractVendor, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractVendor = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetContractVendor
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdGetContractVendorQuery = new SqlCommand( "GetContractVendor", dbConnection );
                cmdGetContractVendorQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContractVendorQuery.CommandTimeout = 30;

                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;

                cmdGetContractVendorQuery.Parameters.Add( parmContractDetailsId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractVendor = new SqlDataAdapter();
                daContractVendor.SelectCommand = cmdGetContractVendorQuery;

                dsContractVendor = new DataSet( "ContractVendor" );
                DataTable dtContractVendor = dsContractVendor.Tables.Add( "ContractVendorTable" );

                DataColumn vendorIdColumn = new DataColumn( "VendorId", typeof( int ) );

                dtContractVendor.Columns.Add( vendorIdColumn );
                dtContractVendor.Columns.Add( "CompanyNameOnContract", typeof( string ) );
                dtContractVendor.Columns.Add( "MasterVendorId", typeof( int ) );
                dtContractVendor.Columns.Add( "CompanyName", typeof( string ) );
                dtContractVendor.Columns.Add( "ContractDetailsId", typeof( int ) );

                dtContractVendor.Columns.Add( "SAMUEI", typeof( string ) );
                dtContractVendor.Columns.Add( "DUNS", typeof( decimal ) );
                dtContractVendor.Columns.Add( "TIN", typeof( decimal ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = vendorIdColumn;

                // add the keys to the table
                dtContractVendor.PrimaryKey = primaryKeyColumns;

                dtContractVendor.Clear();

                // connect
                dbConnection.Open();

                // run
                daContractVendor.Fill( dsContractVendor, "ContractVendorTable" );

                RowsReturned = dsContractVendor.Tables[ "ContractVendorTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetContractVendor(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns all vendor info
        /// </summary>
        /// <param name="dsItemList"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool SelectContractVendors( ref DataSet dsContractVendors )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractVendors = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractVendors
                //(
                //)

                SqlCommand cmdGetContractVendorQuery = new SqlCommand( "SelectContractVendors", dbConnection );
                cmdGetContractVendorQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContractVendorQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractVendors = new SqlDataAdapter();
                daContractVendors.SelectCommand = cmdGetContractVendorQuery;

                dsContractVendors = new DataSet( "ContractVendors" );
                DataTable dtContractVendor = dsContractVendors.Tables.Add( "ContractVendorsTable" );

                DataColumn vendorIdColumn = new DataColumn( "VendorId", typeof( int ) );

                dtContractVendor.Columns.Add( vendorIdColumn );
                dtContractVendor.Columns.Add( "CompanyNameOnContract", typeof( string ) );
                dtContractVendor.Columns.Add( "MasterVendorId", typeof( int ) );
                dtContractVendor.Columns.Add( "CompanyName", typeof( string ) );
                dtContractVendor.Columns.Add( "ContractDetailsId", typeof( int ) );
                dtContractVendor.Columns.Add( "SAMUEI", typeof( string ) );
                dtContractVendor.Columns.Add( "DUNS", typeof( decimal ) );
                dtContractVendor.Columns.Add( "TIN", typeof( decimal ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = vendorIdColumn;

                // add the keys to the table
                dtContractVendor.PrimaryKey = primaryKeyColumns;

                dtContractVendor.Clear();

                // connect
                dbConnection.Open();

                // run
                daContractVendors.Fill( dsContractVendors, "ContractVendorsTable" );

                RowsReturned = dsContractVendors.Tables[ "ContractVendorsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectContractVendors(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns addresses for a particular vendor.
        /// </summary>
        /// <param name="dsVendorAddresses"></param>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public bool SelectVendorAddresses( ref DataSet dsVendorAddresses, int vendorId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daVendorAddresses = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectVendorAddresses
                //(
                //@VendorId int
                //)

                SqlCommand cmdSelectVendorAddressesQuery = new SqlCommand( "SelectVendorAddresses", dbConnection );
                cmdSelectVendorAddressesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectVendorAddressesQuery.CommandTimeout = 30;

                SqlParameter parmVendorId = new SqlParameter( "@VendorId", SqlDbType.Int );
                parmVendorId.Direction = ParameterDirection.Input;
                parmVendorId.Value = vendorId;

                cmdSelectVendorAddressesQuery.Parameters.Add( parmVendorId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daVendorAddresses = new SqlDataAdapter();
                daVendorAddresses.SelectCommand = cmdSelectVendorAddressesQuery;

                dsVendorAddresses = new DataSet( "VendorAddresses" );
                DataTable dtVendorAddresses = dsVendorAddresses.Tables.Add( "VendorAddressesTable" );

                // add the common elements to the table
                DataColumn vendorAddressIdColumn = new DataColumn( "VendorAddressId", typeof( int ) );

                dtVendorAddresses.Columns.Add( vendorAddressIdColumn );
                dtVendorAddresses.Columns.Add( "AddressId", typeof( int ) );

                dtVendorAddresses.Columns.Add( "AddressType", typeof( string ) );
                dtVendorAddresses.Columns.Add( "AddressTypeDescription", typeof( string ) );
                dtVendorAddresses.Columns.Add( "AddressLine1", typeof( string ) );
                dtVendorAddresses.Columns.Add( "AddressLine2", typeof( string ) );
                dtVendorAddresses.Columns.Add( "City", typeof( string ) );
                dtVendorAddresses.Columns.Add( "Zip", typeof( string ) );
                dtVendorAddresses.Columns.Add( "ZipPlusFour", typeof( string ) );
                dtVendorAddresses.Columns.Add( "StateProvinceCode", typeof( string ) );

                dtVendorAddresses.Columns.Add( "Country", typeof( string ) );
                dtVendorAddresses.Columns.Add( "WebPage", typeof( string ) );

                dtVendorAddresses.Columns.Add( "ACreatedBy", typeof( string ) );
                dtVendorAddresses.Columns.Add( "ACreationDate", typeof( DateTime ) );
                dtVendorAddresses.Columns.Add( "ALastModifiedBy", typeof( string ) );
                dtVendorAddresses.Columns.Add( "ALastModificationDate", typeof( DateTime ) );

                dtVendorAddresses.Columns.Add( "VCreatedBy", typeof( string ) );
                dtVendorAddresses.Columns.Add( "VCreationDate", typeof( DateTime ) );
                dtVendorAddresses.Columns.Add( "VLastModifiedBy", typeof( string ) );
                dtVendorAddresses.Columns.Add( "VLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = vendorAddressIdColumn;

                // add the keys to the table
                dtVendorAddresses.PrimaryKey = primaryKeyColumns;

                dtVendorAddresses.Clear();

                // connect
                dbConnection.Open();

                // run
                daVendorAddresses.Fill( dsVendorAddresses, "VendorAddressesTable" );

                RowsReturned = dsVendorAddresses.Tables[ "VendorAddressesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectVendorAddresses(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns addresses for a particular offer.
        /// </summary>
        /// <param name="dsOfferAddresses"></param>
        /// <param name="offerId"></param>
        /// <returns></returns>
        public bool SelectOfferAddresses( ref DataSet dsOfferAddresses, int offerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOfferAddresses = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectOfferAddresses
                //(
                //@OfferId int
                //)

                SqlCommand cmdSelectOfferAddressesQuery = new SqlCommand( "SelectOfferAddresses", dbConnection );
                cmdSelectOfferAddressesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferAddressesQuery.CommandTimeout = 30;

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );
                parmOfferId.Direction = ParameterDirection.Input;
                parmOfferId.Value = offerId;

                cmdSelectOfferAddressesQuery.Parameters.Add( parmOfferId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferAddresses = new SqlDataAdapter();
                daOfferAddresses.SelectCommand = cmdSelectOfferAddressesQuery;

                dsOfferAddresses = new DataSet( "OfferAddresses" );
                DataTable dtOfferAddresses = dsOfferAddresses.Tables.Add( "OfferAddressesTable" );

                // add the common elements to the table
                DataColumn offerAddressIdColumn = new DataColumn( "OfferAddressId", typeof( int ) );

                dtOfferAddresses.Columns.Add( offerAddressIdColumn );
                dtOfferAddresses.Columns.Add( "AddressId", typeof( int ) );

                dtOfferAddresses.Columns.Add( "AddressType", typeof( string ) );
                dtOfferAddresses.Columns.Add( "AddressTypeDescription", typeof( string ) );
                dtOfferAddresses.Columns.Add( "AddressLine1", typeof( string ) );
                dtOfferAddresses.Columns.Add( "AddressLine2", typeof( string ) );
                dtOfferAddresses.Columns.Add( "City", typeof( string ) );
                dtOfferAddresses.Columns.Add( "Zip", typeof( string ) );
                dtOfferAddresses.Columns.Add( "ZipPlusFour", typeof( string ) );
                dtOfferAddresses.Columns.Add( "StateProvinceCode", typeof( string ) );

                dtOfferAddresses.Columns.Add( "Country", typeof( string ) );
                dtOfferAddresses.Columns.Add( "WebPage", typeof( string ) );

                dtOfferAddresses.Columns.Add( "ACreatedBy", typeof( string ) );
                dtOfferAddresses.Columns.Add( "ACreationDate", typeof( DateTime ) );
                dtOfferAddresses.Columns.Add( "ALastModifiedBy", typeof( string ) );
                dtOfferAddresses.Columns.Add( "ALastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerAddressIdColumn;

                // add the keys to the table
                dtOfferAddresses.PrimaryKey = primaryKeyColumns;

                dtOfferAddresses.Clear();

                // connect
                dbConnection.Open();

                // run
                daOfferAddresses.Fill( dsOfferAddresses, "OfferAddressesTable" );

                RowsReturned = dsOfferAddresses.Tables[ "OfferAddressesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectOfferAddresses(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the list of solicitations
        /// </summary>
        /// <param name="dsSolicitations"></param>
        /// <returns></returns>
        public bool SelectSolicitations( ref DataSet dsSolicitations )
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
                //)

                SqlCommand cmdSelectSolicitationsQuery = new SqlCommand( "SelectSolicitations", dbConnection );
                cmdSelectSolicitationsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectSolicitationsQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSolicitations = new SqlDataAdapter();
                daSolicitations.SelectCommand = cmdSelectSolicitationsQuery;

                dsSolicitations = new DataSet( "Solicitations" );
                DataTable dtSolicitations = dsSolicitations.Tables.Add( "SolicitationsTable" );

                DataColumn solicitationIdColumn = new DataColumn( "SolicitationId", typeof( int ) );

                dtSolicitations.Columns.Add( solicitationIdColumn );

                dtSolicitations.Columns.Add( "SolicitationNumber", typeof( string ) );
                dtSolicitations.Columns.Add( "SolicitationType", typeof( string ) );
                dtSolicitations.Columns.Add( "Description", typeof( string ) );
                dtSolicitations.Columns.Add( "ActiveStatus", typeof( bool ) );
                dtSolicitations.Columns.Add( "ReleaseStatus", typeof( string ) );
                dtSolicitations.Columns.Add( "EditingStatus", typeof( string ) );
                dtSolicitations.Columns.Add( "OwnerGuid", typeof( Guid ) );
                dtSolicitations.Columns.Add( "SCreatedBy", typeof( string ) );
                dtSolicitations.Columns.Add( "SCreationDate", typeof( DateTime ) );
                dtSolicitations.Columns.Add( "SLastModifiedBy", typeof( string ) );
                dtSolicitations.Columns.Add( "SLastModificationDate", typeof( DateTime ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = solicitationIdColumn;

                // add the keys to the table
                dtSolicitations.PrimaryKey = primaryKeyColumns;

                dtSolicitations.Clear();

                // connect
                dbConnection.Open();

                // run
                daSolicitations.Fill( dsSolicitations, "SolicitationsTable" );

                RowsReturned = dsSolicitations.Tables[ "SolicitationsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectSolicitations(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns the list of offers
        /// </summary>
        /// <param name="dsOffers"></param>
        /// <returns></returns>
        public bool SelectOffers( ref DataSet dsOffers )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOffers = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectOffers
                //(
                //)

                SqlCommand cmdSelectOffersQuery = new SqlCommand( "SelectOffers", dbConnection );
                cmdSelectOffersQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOffersQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOffers = new SqlDataAdapter();
                daOffers.SelectCommand = cmdSelectOffersQuery;

                dsOffers = new DataSet( "Offers" );
                DataTable dtOffers = dsOffers.Tables.Add( "OffersTable" );

                DataColumn offerIdColumn = new DataColumn( "OfferId", typeof( int ) );

                dtOffers.Columns.Add( offerIdColumn );

                dtOffers.Columns.Add( "SolicitationId", typeof( int ) );
                dtOffers.Columns.Add( "SolicitationNumber", typeof( string ) );
                dtOffers.Columns.Add( "UserGuid", typeof( Guid ) );
                dtOffers.Columns.Add( "ContractCategoryId", typeof( int ) );
                dtOffers.Columns.Add( "ProposalTypeId", typeof( int ) );

                dtOffers.Columns.Add( "ProposalTypeDescription", typeof( string ) );
                dtOffers.Columns.Add( "MasterVendorId", typeof( int ) );
                dtOffers.Columns.Add( "CompanyName", typeof( string ) );
                dtOffers.Columns.Add( "ExpectedCompletionDate", typeof( DateTime ) );
                dtOffers.Columns.Add( "ExpirationDate", typeof( DateTime ) );

                dtOffers.Columns.Add( "AuditIndicator", typeof( bool ) );
                dtOffers.Columns.Add( "ReleaseStatus", typeof( string ) );
                dtOffers.Columns.Add( "EditingStatus", typeof( string ) );

                dtOffers.Columns.Add( "OCreatedBy", typeof( string ) );
                dtOffers.Columns.Add( "OCreationDate", typeof( DateTime ) );
                dtOffers.Columns.Add( "OLastModifiedBy", typeof( string ) );
                dtOffers.Columns.Add( "OLastModificationDate", typeof( DateTime ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerIdColumn;

                // add the keys to the table
                dtOffers.PrimaryKey = primaryKeyColumns;

                dtOffers.Clear();

                // connect
                dbConnection.Open();

                // run
                daOffers.Fill( dsOffers, "OffersTable" );

                RowsReturned = dsOffers.Tables[ "OffersTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectOffers(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns offer comment history for a particular offer
        /// </summary>
        /// <param name="dsOfferCommentHistory"></param>
        /// <param name="offerId"></param>
        /// <returns></returns>
        public bool SelectOfferCommentHistory( ref DataSet dsOfferCommentHistory, int offerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOfferCommentHistory = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectOfferCommentHistory
                //(
                //@OfferId int
                //)

                SqlCommand cmdSelectOfferCommentHistory = new SqlCommand( "SelectOfferCommentHistory", dbConnection );
                cmdSelectOfferCommentHistory.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferCommentHistory.CommandTimeout = 30;

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );
                parmOfferId.Direction = ParameterDirection.Input;
                parmOfferId.Value = offerId;

                cmdSelectOfferCommentHistory.Parameters.Add( parmOfferId );
                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferCommentHistory = new SqlDataAdapter();
                daOfferCommentHistory.SelectCommand = cmdSelectOfferCommentHistory;

                dsOfferCommentHistory = new DataSet( "OfferCommentHistory" );
                DataTable dtOfferCommentHistory = dsOfferCommentHistory.Tables.Add( "OfferCommentHistoryTable" );

                DataColumn offerCommentHistoryIdColumn = new DataColumn( "OfferCommentHistoryId", typeof( int ) );

                dtOfferCommentHistory.Columns.Add( offerCommentHistoryIdColumn );

                dtOfferCommentHistory.Columns.Add( "Ordinality", typeof( int ) );
                dtOfferCommentHistory.Columns.Add( "Comment", typeof( string ) );

                dtOfferCommentHistory.Columns.Add( "HCreatedBy", typeof( string ) );
                dtOfferCommentHistory.Columns.Add( "HCreationDate", typeof( DateTime ) );
                dtOfferCommentHistory.Columns.Add( "HLastModifiedBy", typeof( string ) );
                dtOfferCommentHistory.Columns.Add( "HLastModificationDate", typeof( DateTime ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerCommentHistoryIdColumn;

                // add the keys to the table
                dtOfferCommentHistory.PrimaryKey = primaryKeyColumns;

                dtOfferCommentHistory.Clear();

                // connect
                dbConnection.Open();

                // run
                daOfferCommentHistory.Fill( dsOfferCommentHistory, "OfferCommentHistoryTable" );

                RowsReturned = dsOfferCommentHistory.Tables[ "OfferCommentHistoryTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectOfferCommentHistory(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns offer action history for a particular offer
        /// </summary>
        /// <param name="dsOfferActionHistory"></param>
        /// <param name="offerId"></param>
        /// <returns></returns>
        public bool SelectOfferActionHistory( ref DataSet dsOfferActionHistory, int offerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOfferActionHistory = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectOfferActionHistory
                //(
                //@OfferId int
                //)

                SqlCommand cmdSelectOfferActionHistory = new SqlCommand( "SelectOfferActionHistory", dbConnection );
                cmdSelectOfferActionHistory.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferActionHistory.CommandTimeout = 30;

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );
                parmOfferId.Direction = ParameterDirection.Input;
                parmOfferId.Value = offerId;

                cmdSelectOfferActionHistory.Parameters.Add( parmOfferId );
                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferActionHistory = new SqlDataAdapter();
                daOfferActionHistory.SelectCommand = cmdSelectOfferActionHistory;

                dsOfferActionHistory = new DataSet( "OfferActionHistory" );
                DataTable dtOfferActionHistory = dsOfferActionHistory.Tables.Add( "OfferActionHistoryTable" );

                DataColumn offerActionHistoryIdColumn = new DataColumn( "OfferActionHistoryId", typeof( int ) );

                dtOfferActionHistory.Columns.Add( offerActionHistoryIdColumn );

                dtOfferActionHistory.Columns.Add( "ActionId", typeof( int ) );
                dtOfferActionHistory.Columns.Add( "ActionDescription", typeof( string ) );
                dtOfferActionHistory.Columns.Add( "IndicatesComplete", typeof( bool ) );
                dtOfferActionHistory.Columns.Add( "ActionDate", typeof( DateTime ) );
                dtOfferActionHistory.Columns.Add( "UserGuid", typeof( Guid ) );

                dtOfferActionHistory.Columns.Add( "HCreatedBy", typeof( string ) );
                dtOfferActionHistory.Columns.Add( "HCreationDate", typeof( DateTime ) );
                dtOfferActionHistory.Columns.Add( "HLastModifiedBy", typeof( string ) );
                dtOfferActionHistory.Columns.Add( "HLastModificationDate", typeof( DateTime ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerActionHistoryIdColumn;

                // add the keys to the table
                dtOfferActionHistory.PrimaryKey = primaryKeyColumns;

                dtOfferActionHistory.Clear();

                // connect
                dbConnection.Open();

                // run
                daOfferActionHistory.Fill( dsOfferActionHistory, "OfferActionHistoryTable" );

                RowsReturned = dsOfferActionHistory.Tables[ "OfferActionHistoryTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectOfferActionHistory(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns a list of offer action types
        /// </summary>
        /// <param name="dsOfferActionTypes"></param>
        /// <returns></returns>
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
                //)

                SqlCommand cmdSelectOfferActionTypesQuery = new SqlCommand( "SelectOfferActionTypes", dbConnection );
                cmdSelectOfferActionTypesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferActionTypesQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferActionTypes = new SqlDataAdapter();
                daOfferActionTypes.SelectCommand = cmdSelectOfferActionTypesQuery;

                dsOfferActionTypes = new DataSet( "OfferActionTypes" );
                DataTable dtOfferActionTypes = dsOfferActionTypes.Tables.Add( "OfferActionTypesTable" );

                DataColumn actionIdColumn = new DataColumn( "ActionId", typeof( int ) );

                dtOfferActionTypes.Columns.Add( actionIdColumn );

                dtOfferActionTypes.Columns.Add( "ActionDescription", typeof( string ) );
                dtOfferActionTypes.Columns.Add( "IndicatesComplete", typeof( bool ) );

                dtOfferActionTypes.Columns.Add( "TCreatedBy", typeof( string ) );
                dtOfferActionTypes.Columns.Add( "TCreationDate", typeof( DateTime ) );
                dtOfferActionTypes.Columns.Add( "TLastModifiedBy", typeof( string ) );
                dtOfferActionTypes.Columns.Add( "TLastModificationDate", typeof( DateTime ) );


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

        /// <summary>
        /// Returns offer assignment history for a particular offer
        /// </summary>
        /// <param name="dsOfferAssignmentHistory"></param>
        /// <param name="offerId"></param>
        /// <returns></returns>
        public bool SelectOfferAssignmentHistory( ref DataSet dsOfferAssignmentHistory, int offerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOfferAssignmentHistory = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectOfferAssignmentHistory
                //(
                //@OfferId int
                //)

                SqlCommand cmdSelectOfferAssignmentHistory = new SqlCommand( "SelectOfferAssignmentHistory", dbConnection );
                cmdSelectOfferAssignmentHistory.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferAssignmentHistory.CommandTimeout = 30;

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );
                parmOfferId.Direction = ParameterDirection.Input;
                parmOfferId.Value = offerId;

                cmdSelectOfferAssignmentHistory.Parameters.Add( parmOfferId );
                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferAssignmentHistory = new SqlDataAdapter();
                daOfferAssignmentHistory.SelectCommand = cmdSelectOfferAssignmentHistory;

                dsOfferAssignmentHistory = new DataSet( "OfferAssignmentHistory" );
                DataTable dtOfferAssignmentHistory = dsOfferAssignmentHistory.Tables.Add( "OfferAssignmentHistoryTable" );

                DataColumn offerAssignmentHistoryIdColumn = new DataColumn( "OfferAssignmentHistoryId", typeof( int ) );

                dtOfferAssignmentHistory.Columns.Add( offerAssignmentHistoryIdColumn );

                dtOfferAssignmentHistory.Columns.Add( "UserGuid", typeof( Guid ) );
                dtOfferAssignmentHistory.Columns.Add( "AssignmentEffectiveDate", typeof( DateTime ) );
                dtOfferAssignmentHistory.Columns.Add( "AssigningUserGuid", typeof( Guid ) );

                dtOfferAssignmentHistory.Columns.Add( "SCreatedBy", typeof( string ) );
                dtOfferAssignmentHistory.Columns.Add( "SCreationDate", typeof( DateTime ) );
                dtOfferAssignmentHistory.Columns.Add( "SLastModifiedBy", typeof( string ) );
                dtOfferAssignmentHistory.Columns.Add( "SLastModificationDate", typeof( DateTime ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerAssignmentHistoryIdColumn;

                // add the keys to the table
                dtOfferAssignmentHistory.PrimaryKey = primaryKeyColumns;

                dtOfferAssignmentHistory.Clear();

                // connect
                dbConnection.Open();

                // run
                daOfferAssignmentHistory.Fill( dsOfferAssignmentHistory, "OfferAssignmentHistoryTable" );

                RowsReturned = dsOfferAssignmentHistory.Tables[ "OfferAssignmentHistoryTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectOfferAssignmentHistory(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }
   
        /// <summary>
        /// Returns a list of offer proposal types
        /// </summary>
        /// <param name="dsOfferProposalTypes"></param>
        /// <returns></returns>
        public bool SelectOfferProposalTypes( ref DataSet dsOfferProposalTypes )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOfferProposalTypes = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectOfferProposalTypes
                //(
                //)

                SqlCommand cmdSelectOfferProposalTypesQuery = new SqlCommand( "SelectOfferProposalTypes", dbConnection );
                cmdSelectOfferProposalTypesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferProposalTypesQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferProposalTypes = new SqlDataAdapter();
                daOfferProposalTypes.SelectCommand = cmdSelectOfferProposalTypesQuery;

                dsOfferProposalTypes = new DataSet( "OfferProposalTypes" );
                DataTable dtOfferProposalTypes = dsOfferProposalTypes.Tables.Add( "OfferProposalTypesTable" );

                DataColumn proposalTypeIdColumn = new DataColumn( "ProposalTypeId", typeof( int ) );

                dtOfferProposalTypes.Columns.Add( proposalTypeIdColumn );

                dtOfferProposalTypes.Columns.Add( "ProposalTypeDescription", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = proposalTypeIdColumn;

                // add the keys to the table
                dtOfferProposalTypes.PrimaryKey = primaryKeyColumns;

                dtOfferProposalTypes.Clear();

                // connect
                dbConnection.Open();

                // run
                daOfferProposalTypes.Fill( dsOfferProposalTypes, "OfferProposalTypesTable" );

                RowsReturned = dsOfferProposalTypes.Tables[ "OfferProposalTypesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectOfferProposalTypes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns contacts for a particular offer.
        /// </summary>
        /// <param name="dsOfferContacts"></param>
        /// <param name="offerId"></param>
        /// <returns></returns>
        public bool SelectOfferContacts( ref DataSet dsOfferContacts, int offerId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daOfferContacts = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectOfferContacts
                //(
                //@OfferId int
                //)

                SqlCommand cmdSelectOfferContactsQuery = new SqlCommand( "SelectOfferContacts", dbConnection );
                cmdSelectOfferContactsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectOfferContactsQuery.CommandTimeout = 30;

                SqlParameter parmOfferId = new SqlParameter( "@OfferId", SqlDbType.Int );
                parmOfferId.Direction = ParameterDirection.Input;
                parmOfferId.Value = offerId;

                cmdSelectOfferContactsQuery.Parameters.Add( parmOfferId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daOfferContacts = new SqlDataAdapter();
                daOfferContacts.SelectCommand = cmdSelectOfferContactsQuery;

                dsOfferContacts = new DataSet( "OfferContacts" );
                DataTable dtOfferContacts = dsOfferContacts.Tables.Add( "OfferContactsTable" );

                // add the common elements to the table
                DataColumn offerContactIdColumn = new DataColumn( "OfferContactId", typeof( int ) );

                dtOfferContacts.Columns.Add( offerContactIdColumn );
                dtOfferContacts.Columns.Add( "ContactId", typeof( int ) );

                dtOfferContacts.Columns.Add( "ContactType", typeof( string ) );
                dtOfferContacts.Columns.Add( "ContactTypeDescription", typeof( string ) );

                dtOfferContacts.Columns.Add( "Name", typeof( string ) );
                dtOfferContacts.Columns.Add( "Phone", typeof( string ) );
                dtOfferContacts.Columns.Add( "Extension", typeof( string ) );
                dtOfferContacts.Columns.Add( "Fax", typeof( string ) );
                dtOfferContacts.Columns.Add( "Email", typeof( string ) );

                dtOfferContacts.Columns.Add( "CCreatedBy", typeof( string ) );
                dtOfferContacts.Columns.Add( "CCreationDate", typeof( DateTime ) );
                dtOfferContacts.Columns.Add( "CLastModifiedBy", typeof( string ) );
                dtOfferContacts.Columns.Add( "CLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = offerContactIdColumn;

                // add the keys to the table
                dtOfferContacts.PrimaryKey = primaryKeyColumns;

                dtOfferContacts.Clear();

                // connect
                dbConnection.Open();

                // run
                daOfferContacts.Fill( dsOfferContacts, "OfferContactsTable" );

                RowsReturned = dsOfferContacts.Tables[ "OfferContactsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectOfferContacts(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns contacts for a particular contract.
        /// </summary>
        /// <param name="dsContractContacts"></param>
        /// <param name="contractId"></param>
        /// <returns></returns>
        public bool SelectContractContacts( ref DataSet dsContractContacts, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContractContacts = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContractContacts
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdSelectContractContactsQuery = new SqlCommand( "SelectContractContacts", dbConnection );
                cmdSelectContractContactsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContractContactsQuery.CommandTimeout = 30;

                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;

                cmdSelectContractContactsQuery.Parameters.Add( parmContractDetailsId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContractContacts = new SqlDataAdapter();
                daContractContacts.SelectCommand = cmdSelectContractContactsQuery;

                dsContractContacts = new DataSet( "ContractContacts" );
                DataTable dtContractContacts = dsContractContacts.Tables.Add( "ContractContactsTable" );

                // add the common elements to the table
                DataColumn contractContactsIdColumn = new DataColumn( "ContractContactsId", typeof( int ) );

                dtContractContacts.Columns.Add( contractContactsIdColumn );
                dtContractContacts.Columns.Add( "ContactId", typeof( int ) );

                dtContractContacts.Columns.Add( "ContactType", typeof( string ) );
                dtContractContacts.Columns.Add( "ContactTypeDescription", typeof( string ) );

                dtContractContacts.Columns.Add( "Name", typeof( string ) );
                dtContractContacts.Columns.Add( "Phone", typeof( string ) );
                dtContractContacts.Columns.Add( "Extension", typeof( string ) );
                dtContractContacts.Columns.Add( "Fax", typeof( string ) );
                dtContractContacts.Columns.Add( "Email", typeof( string ) );

                dtContractContacts.Columns.Add( "CCreatedBy", typeof( string ) );
                dtContractContacts.Columns.Add( "CCreationDate", typeof( DateTime ) );
                dtContractContacts.Columns.Add( "CLastModifiedBy", typeof( string ) );
                dtContractContacts.Columns.Add( "CLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contractContactsIdColumn;

                // add the keys to the table
                dtContractContacts.PrimaryKey = primaryKeyColumns;

                dtContractContacts.Clear();

                // connect
                dbConnection.Open();

                // run
                daContractContacts.Fill( dsContractContacts, "ContractContactsTable" );

                RowsReturned = dsContractContacts.Tables[ "ContractContactsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectContractContacts(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns contacts for a particular vendor.
        /// </summary>
        /// <param name="dsVendorContacts"></param>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public bool SelectVendorContacts( ref DataSet dsVendorContacts, int vendorId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daVendorContacts = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectVendorContacts
                //(
                //@VendorId int
                //)

                SqlCommand cmdSelectVendorContactsQuery = new SqlCommand( "SelectVendorContacts", dbConnection );
                cmdSelectVendorContactsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectVendorContactsQuery.CommandTimeout = 30;

                SqlParameter parmVendorId = new SqlParameter( "@VendorId", SqlDbType.Int );
                parmVendorId.Direction = ParameterDirection.Input;
                parmVendorId.Value = vendorId;

                cmdSelectVendorContactsQuery.Parameters.Add( parmVendorId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daVendorContacts = new SqlDataAdapter();
                daVendorContacts.SelectCommand = cmdSelectVendorContactsQuery;

                dsVendorContacts = new DataSet( "VendorContacts" );
                DataTable dtVendorContacts = dsVendorContacts.Tables.Add( "VendorContactsTable" );

                // add the common elements to the table
                DataColumn vendorContactIdColumn = new DataColumn( "VendorContactId", typeof( int ) );

                dtVendorContacts.Columns.Add( vendorContactIdColumn );
                dtVendorContacts.Columns.Add( "ContactId", typeof( int ) );

                dtVendorContacts.Columns.Add( "ContactType", typeof( string ) );
                dtVendorContacts.Columns.Add( "ContactTypeDescription", typeof( string ) );

                dtVendorContacts.Columns.Add( "Name", typeof( string ) );
                dtVendorContacts.Columns.Add( "Phone", typeof( string ) );
                dtVendorContacts.Columns.Add( "Extension", typeof( string ) );
                dtVendorContacts.Columns.Add( "Fax", typeof( string ) );
                dtVendorContacts.Columns.Add( "Email", typeof( string ) );

                dtVendorContacts.Columns.Add( "CCreatedBy", typeof( string ) );
                dtVendorContacts.Columns.Add( "CCreationDate", typeof( DateTime ) );
                dtVendorContacts.Columns.Add( "CLastModifiedBy", typeof( string ) );
                dtVendorContacts.Columns.Add( "CLastModificationDate", typeof( DateTime ) );

                dtVendorContacts.Columns.Add( "VCreatedBy", typeof( string ) );
                dtVendorContacts.Columns.Add( "VCreationDate", typeof( DateTime ) );
                dtVendorContacts.Columns.Add( "VLastModifiedBy", typeof( string ) );
                dtVendorContacts.Columns.Add( "VLastModificationDate", typeof( DateTime ) );
                
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = vendorContactIdColumn;

                // add the keys to the table
                dtVendorContacts.PrimaryKey = primaryKeyColumns;

                dtVendorContacts.Clear();

                // connect
                dbConnection.Open();

                // run
                daVendorContacts.Fill( dsVendorContacts, "VendorContactsTable" );

                RowsReturned = dsVendorContacts.Tables[ "VendorContactsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in VendorDB.SelectVendorContacts(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns a list of predefined address types
        /// </summary>
        /// <param name="dsAddressTypes"></param>
        /// <returns></returns>
        public bool SelectAddressTypes( ref DataSet dsAddressTypes )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daAddressTypes = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectAddressTypes
                //(
                //)

                SqlCommand cmdSelectAddressTypesQuery = new SqlCommand( "SelectAddressTypes", dbConnection );
                cmdSelectAddressTypesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectAddressTypesQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daAddressTypes = new SqlDataAdapter();
                daAddressTypes.SelectCommand = cmdSelectAddressTypesQuery;

                dsAddressTypes = new DataSet( "AddressTypes" );
                DataTable dtVendorContacts = dsAddressTypes.Tables.Add( "AddressTypesTable" );

                // add the common elements to the table
                DataColumn addressTypeColumn = new DataColumn( "AddressType", typeof( string ) );

                dtVendorContacts.Columns.Add( addressTypeColumn );
                dtVendorContacts.Columns.Add( "AddressTypeDescription", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = addressTypeColumn;

                // add the keys to the table
                dtVendorContacts.PrimaryKey = primaryKeyColumns;

                dtVendorContacts.Clear();

                // connect
                dbConnection.Open();

                // run
                daAddressTypes.Fill( dsAddressTypes, "AddressTypesTable" );

                RowsReturned = dsAddressTypes.Tables[ "AddressTypesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in VendorDB.SelectAddressTypes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns a list of predefined contact types
        /// </summary>
        /// <param name="dsContactTypes"></param>
        /// <returns></returns>
        public bool SelectContactTypes( ref DataSet dsContactTypes )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContactTypes = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectContactTypes
                //(
                //)

                SqlCommand cmdSelectContactTypesQuery = new SqlCommand( "SelectContactTypes", dbConnection );
                cmdSelectContactTypesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectContactTypesQuery.CommandTimeout = 30;

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContactTypes = new SqlDataAdapter();
                daContactTypes.SelectCommand = cmdSelectContactTypesQuery;

                dsContactTypes = new DataSet( "ContactTypes" );
                DataTable dtVendorContacts = dsContactTypes.Tables.Add( "ContactTypesTable" );

                // add the common elements to the table
                DataColumn contactTypeColumn = new DataColumn( "ContactType", typeof( string ) );

                dtVendorContacts.Columns.Add( contactTypeColumn );
                dtVendorContacts.Columns.Add( "ContactTypeDescription", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contactTypeColumn;

                // add the keys to the table
                dtVendorContacts.PrimaryKey = primaryKeyColumns;

                dtVendorContacts.Clear();

                // connect
                dbConnection.Open();

                // run
                daContactTypes.Fill( dsContactTypes, "ContactTypesTable" );

                RowsReturned = dsContactTypes.Tables[ "ContactTypesTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in VendorDB.SelectContactTypes(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns an address given an addressId
        /// </summary>
        /// <param name="dsAddress"></param>
        /// <param name="addressId"></param>
        /// <returns></returns>
        public bool GetAddress( ref DataSet dsAddress, int addressId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daAddress = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetAddress
                //(
                //@addressId int
                //)

                SqlCommand cmdGetAddressQuery = new SqlCommand( "GetAddress", dbConnection );
                cmdGetAddressQuery.CommandType = CommandType.StoredProcedure;
                cmdGetAddressQuery.CommandTimeout = 30;

                SqlParameter parmAddressId = new SqlParameter( "@AddressId", SqlDbType.Int );
                parmAddressId.Direction = ParameterDirection.Input;
                parmAddressId.Value = addressId;

                cmdGetAddressQuery.Parameters.Add( parmAddressId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daAddress = new SqlDataAdapter();
                daAddress.SelectCommand = cmdGetAddressQuery;

                dsAddress = new DataSet( "Address" );
                DataTable dtAddress = dsAddress.Tables.Add( "AddressTable" );

                DataColumn addressIdColumn = new DataColumn( "AddressId", typeof( int ) );

                dtAddress.Columns.Add( addressIdColumn );
                dtAddress.Columns.Add( "AddressType", typeof( string ) );
                dtAddress.Columns.Add( "AddressTypeDescription", typeof( string ) );
                dtAddress.Columns.Add( "AddressLine1", typeof( string ) );
                dtAddress.Columns.Add( "AddressLine2", typeof( string ) );
                dtAddress.Columns.Add( "City", typeof( string ) );
                dtAddress.Columns.Add( "Zip", typeof( string ) );
                dtAddress.Columns.Add( "ZipPlusFour", typeof( string ) );
                dtAddress.Columns.Add( "StateProvinceCode", typeof( string ) );

                dtAddress.Columns.Add( "Country", typeof( string ) );
                dtAddress.Columns.Add( "WebPage", typeof( string ) );

                dtAddress.Columns.Add( "ACreatedBy", typeof( string ) );
                dtAddress.Columns.Add( "ACreationDate", typeof( DateTime ) );
                dtAddress.Columns.Add( "ALastModifiedBy", typeof( string ) );
                dtAddress.Columns.Add( "ALastModificationDate", typeof( DateTime ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = addressIdColumn;

                // add the keys to the table
                dtAddress.PrimaryKey = primaryKeyColumns;

                dtAddress.Clear();

                // connect
                dbConnection.Open();

                // run
                daAddress.Fill( dsAddress, "AddressTable" );

                RowsReturned = dsAddress.Tables[ "AddressTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetAddress(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns a contact given a contactId
        /// </summary>
        /// <param name="dsContact"></param>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public bool GetContact( ref DataSet dsContact, int contactId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daContact = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetContact
                //(
                //@contactId int
                //)

                SqlCommand cmdGetContactQuery = new SqlCommand( "GetContact", dbConnection );
                cmdGetContactQuery.CommandType = CommandType.StoredProcedure;
                cmdGetContactQuery.CommandTimeout = 30;

                SqlParameter parmContactId = new SqlParameter( "@ContactId", SqlDbType.Int );
                parmContactId.Direction = ParameterDirection.Input;
                parmContactId.Value = contactId;

                cmdGetContactQuery.Parameters.Add( parmContactId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daContact = new SqlDataAdapter();
                daContact.SelectCommand = cmdGetContactQuery;

                dsContact = new DataSet( "Contact" );
                DataTable dtContact = dsContact.Tables.Add( "ContactTable" );

                DataColumn contactIdColumn = new DataColumn( "ContactId", typeof( int ) );

                dtContact.Columns.Add( contactIdColumn );
                dtContact.Columns.Add( "ContactType", typeof( string ) );
                dtContact.Columns.Add( "ContactTypeDescription", typeof( string ) );
                dtContact.Columns.Add( "Name", typeof( string ) );
                dtContact.Columns.Add( "Phone", typeof( string ) );
                dtContact.Columns.Add( "Extension", typeof( string ) );
                dtContact.Columns.Add( "Fax", typeof( string ) );
                dtContact.Columns.Add( "Email", typeof( string ) );

                dtContact.Columns.Add( "CCreatedBy", typeof( string ) );
                dtContact.Columns.Add( "CCreationDate", typeof( DateTime ) );
                dtContact.Columns.Add( "CLastModifiedBy", typeof( string ) );
                dtContact.Columns.Add( "CLastModificationDate", typeof( DateTime ) );


                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = contactIdColumn;

                // add the keys to the table
                dtContact.PrimaryKey = primaryKeyColumns;

                dtContact.Clear();

                // connect
                dbConnection.Open();

                // run
                daContact.Fill( dsContact, "ContactTable" );

                RowsReturned = dsContact.Tables[ "ContactTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetContact(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns sba plans for a particular contract.
        /// </summary>
        /// <param name="dsSBAPlans"></param>
        /// <param name="contractDetailsId"></param>
        /// <returns></returns>
        public bool SelectSBAPlans( ref DataSet dsSBAPlans, int contractDetailsId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSBAPlans = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectSBAPlans
                //(
                //@ContractDetailsId int
                //)

                SqlCommand cmdSelectSBAPlansQuery = new SqlCommand( "SelectSBAPlans", dbConnection );
                cmdSelectSBAPlansQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectSBAPlansQuery.CommandTimeout = 30;

                SqlParameter parmContractDetailsId = new SqlParameter( "@ContractDetailsId", SqlDbType.Int );
                parmContractDetailsId.Direction = ParameterDirection.Input;
                parmContractDetailsId.Value = contractDetailsId;

                cmdSelectSBAPlansQuery.Parameters.Add( parmContractDetailsId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSBAPlans = new SqlDataAdapter();
                daSBAPlans.SelectCommand = cmdSelectSBAPlansQuery;

                dsSBAPlans = new DataSet( "SBAPlans" );
                DataTable dtSBAPlans = dsSBAPlans.Tables.Add( "SBAPlansTable" );

                DataColumn sbaPlanIdColumn = new DataColumn( "SBAPlanId", typeof( int ) );

                dtSBAPlans.Columns.Add( sbaPlanIdColumn );
                dtSBAPlans.Columns.Add( "SBAPlanDefinitionId", typeof( int ) );
                dtSBAPlans.Columns.Add( "PlanName", typeof( string ) );
                dtSBAPlans.Columns.Add( "PlanType", typeof( string ) );
                dtSBAPlans.Columns.Add( "PlanTypeDescription", typeof( string ) );
                dtSBAPlans.Columns.Add( "ContactId", typeof( int ) );
                dtSBAPlans.Columns.Add( "AddressId", typeof( int ) );

                dtSBAPlans.Columns.Add( "DCreatedBy", typeof( string ) );
                dtSBAPlans.Columns.Add( "DCreationDate", typeof( DateTime ) );
                dtSBAPlans.Columns.Add( "DLastModifiedBy", typeof( string ) );
                dtSBAPlans.Columns.Add( "DLastModificationDate", typeof( DateTime ) );

                dtSBAPlans.Columns.Add( "ContactType", typeof( string ) );
                dtSBAPlans.Columns.Add( "ContactTypeDescription", typeof( string ) );
                dtSBAPlans.Columns.Add( "Name", typeof( string ) );
                dtSBAPlans.Columns.Add( "Phone", typeof( string ) );
                dtSBAPlans.Columns.Add( "Extension", typeof( string ) );
                dtSBAPlans.Columns.Add( "Fax", typeof( string ) );
                dtSBAPlans.Columns.Add( "Email", typeof( string ) );

                dtSBAPlans.Columns.Add( "AddressType", typeof( string ) );
                dtSBAPlans.Columns.Add( "AddressTypeDescription", typeof( string ) );
                dtSBAPlans.Columns.Add( "AddressLine1", typeof( string ) );
                dtSBAPlans.Columns.Add( "AddressLine2", typeof( string ) );
                dtSBAPlans.Columns.Add( "City", typeof( string ) );
                dtSBAPlans.Columns.Add( "Zip", typeof( string ) );
                dtSBAPlans.Columns.Add( "ZipPlusFour", typeof( string ) );
                dtSBAPlans.Columns.Add( "StateProvinceCode", typeof( string ) );
                dtSBAPlans.Columns.Add( "Country", typeof( string ) );
                dtSBAPlans.Columns.Add( "WebPage", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = sbaPlanIdColumn;

                // add the keys to the table
                dtSBAPlans.PrimaryKey = primaryKeyColumns;

                dtSBAPlans.Clear();

                // connect
                dbConnection.Open();

                // run
                daSBAPlans.Fill( dsSBAPlans, "SBAPlansTable" );

                RowsReturned = dsSBAPlans.Tables[ "SBAPlansTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectSBAPlans(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns sba plan accomplishments for a particular plan.
        /// </summary>
        /// <param name="dsSBAAccomplishments"></param>
        /// <param name="sbaPlanDefinitionId"></param>
        /// <returns></returns>
        public bool SelectSBAAccomplishments( ref DataSet dsSBAAccomplishments, int sbaPlanDefinitionId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSBAAccomplishments = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectSBAAccomplishments
                //(
                //@SBAPlanDefinitionId int
                //)

                SqlCommand cmdSelectSBAAccomplishmentsQuery = new SqlCommand( "SelectSBAAccomplishments", dbConnection );
                cmdSelectSBAAccomplishmentsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectSBAAccomplishmentsQuery.CommandTimeout = 30;

                SqlParameter parmSBAPlanDefinitionId = new SqlParameter( "@SBAPlanDefinitionId", SqlDbType.Int );
                parmSBAPlanDefinitionId.Direction = ParameterDirection.Input;
                parmSBAPlanDefinitionId.Value = sbaPlanDefinitionId;

                cmdSelectSBAAccomplishmentsQuery.Parameters.Add( parmSBAPlanDefinitionId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSBAAccomplishments = new SqlDataAdapter();
                daSBAAccomplishments.SelectCommand = cmdSelectSBAAccomplishmentsQuery;

                dsSBAAccomplishments = new DataSet( "SBAAccomplishments" );
                DataTable dtSBAAccomplishments = dsSBAAccomplishments.Tables.Add( "SBAAccomplishmentsTable" );

                DataColumn sbaAccomplishmentsIdColumn = new DataColumn( "SBAAccomplishmentsId", typeof( int ) );

                dtSBAAccomplishments.Columns.Add( sbaAccomplishmentsIdColumn );
                dtSBAAccomplishments.Columns.Add( "VARatio", typeof( decimal ) );
                dtSBAAccomplishments.Columns.Add( "LargeBusinessDollars", typeof( decimal ) );
                dtSBAAccomplishments.Columns.Add( "SmallBusinessDollars", typeof( decimal ) );
                dtSBAAccomplishments.Columns.Add( "SDBDollars", typeof( decimal ) );
                dtSBAAccomplishments.Columns.Add( "WomanOwnedDollars", typeof( decimal ) );
                dtSBAAccomplishments.Columns.Add( "VeteranOwnedDollars", typeof( decimal ) );
                dtSBAAccomplishments.Columns.Add( "DisabledVeteranDollars", typeof( decimal ) );
                dtSBAAccomplishments.Columns.Add( "HubZoneDollars", typeof( decimal ) );
                dtSBAAccomplishments.Columns.Add( "HBDCDollars", typeof( decimal ) );
                dtSBAAccomplishments.Columns.Add( "VASalesReported", typeof( decimal ) );

                dtSBAAccomplishments.Columns.Add( "Comments", typeof( string ) );
                dtSBAAccomplishments.Columns.Add( "FiscalYear", typeof( int ) );
                dtSBAAccomplishments.Columns.Add( "AccomplishmentPeriod", typeof( string ) );
                dtSBAAccomplishments.Columns.Add( "SBAPlanDefinitionId", typeof( int ) );

                dtSBAAccomplishments.Columns.Add( "ACreatedBy", typeof( string ) );
                dtSBAAccomplishments.Columns.Add( "ACreationDate", typeof( DateTime ) );
                dtSBAAccomplishments.Columns.Add( "ALastModifiedBy", typeof( string ) );
                dtSBAAccomplishments.Columns.Add( "ALastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = sbaAccomplishmentsIdColumn;

                // add the keys to the table
                dtSBAAccomplishments.PrimaryKey = primaryKeyColumns;

                dtSBAAccomplishments.Clear();

                // connect
                dbConnection.Open();

                // run
                daSBAAccomplishments.Fill( dsSBAAccomplishments, "SBAAccomplishmentsTable" );

                RowsReturned = dsSBAAccomplishments.Tables[ "SBAAccomplishmentsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectSBAAccomplishments(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /// <summary>
        /// Returns sba plan projections for a particular plan.
        /// </summary>
        /// <param name="dsSBAProjections"></param>
        /// <param name="sbaPlanDefinitionId"></param>
        /// <returns></returns>
        public bool SelectSBAProjections( ref DataSet dsSBAProjections, int sbaPlanDefinitionId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daSBAProjections = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectSBAProjections
                //(
                //@SBAPlanDefinitionId int
                //)

                SqlCommand cmdSelectSBAProjectionsQuery = new SqlCommand( "SelectSBAProjections", dbConnection );
                cmdSelectSBAProjectionsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectSBAProjectionsQuery.CommandTimeout = 30;

                SqlParameter parmSBAPlanDefinitionId = new SqlParameter( "@SBAPlanDefinitionId", SqlDbType.Int );
                parmSBAPlanDefinitionId.Direction = ParameterDirection.Input;
                parmSBAPlanDefinitionId.Value = sbaPlanDefinitionId;

                cmdSelectSBAProjectionsQuery.Parameters.Add( parmSBAPlanDefinitionId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daSBAProjections = new SqlDataAdapter();
                daSBAProjections.SelectCommand = cmdSelectSBAProjectionsQuery;

                dsSBAProjections = new DataSet( "SBAProjections" );
                DataTable dtSBAProjections = dsSBAProjections.Tables.Add( "SBAProjectionsTable" );

                DataColumn sbaProjectionsIdColumn = new DataColumn( "SBAProjectionsId", typeof( int ) );

                dtSBAProjections.Columns.Add( sbaProjectionsIdColumn );
                dtSBAProjections.Columns.Add( "SmallBusinessDollars", typeof( decimal ) );
                dtSBAProjections.Columns.Add( "SDBDollars", typeof( decimal ) );
                dtSBAProjections.Columns.Add( "WomanOwnedDollars", typeof( decimal ) );
                dtSBAProjections.Columns.Add( "VeteranOwnedDollars", typeof( decimal ) );
                dtSBAProjections.Columns.Add( "DisabledVeteranDollars", typeof( decimal ) );
                dtSBAProjections.Columns.Add( "HubZoneDollars", typeof( decimal ) );
                dtSBAProjections.Columns.Add( "HBDCDollars", typeof( decimal ) );
                dtSBAProjections.Columns.Add( "TotalSubcontractingDollars", typeof( decimal ) );

                dtSBAProjections.Columns.Add( "Comments", typeof( string ) );
                dtSBAProjections.Columns.Add( "StartDate", typeof( DateTime ) );
                dtSBAProjections.Columns.Add( "EndDate", typeof( DateTime ) );
                dtSBAProjections.Columns.Add( "SBAPlanDefinitionId", typeof( int ) );

                dtSBAProjections.Columns.Add( "PCreatedBy", typeof( string ) );
                dtSBAProjections.Columns.Add( "PCreationDate", typeof( DateTime ) );
                dtSBAProjections.Columns.Add( "PLastModifiedBy", typeof( string ) );
                dtSBAProjections.Columns.Add( "PLastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = sbaProjectionsIdColumn;

                // add the keys to the table
                dtSBAProjections.PrimaryKey = primaryKeyColumns;

                dtSBAProjections.Clear();

                // connect
                dbConnection.Open();

                // run
                daSBAProjections.Fill( dsSBAProjections, "SBAProjectionsTable" );

                RowsReturned = dsSBAProjections.Tables[ "SBAProjectionsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.SelectSBAProjections(): {0}", ex.Message );
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