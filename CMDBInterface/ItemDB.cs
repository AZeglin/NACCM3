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
    // provides ado.net access to drug item releated stored procedures in the Item database
    [Serializable]
    public class ItemDB : DBCommon, ISerializable 
    {
     //   private NACLog _log = new NACLog();

        private Guid _currentUserId = Guid.Empty;
        private string _userName = string.Empty;
        private int _oldUserId = -1;

        public ItemDB()
            : base( DBCommon.TargetDatabases.Item )
        {
            //_log.SetCategory( LogBase.Category.DB );
            //_log.SetContext( "ItemDB", this.GetType() );
            //_log.WriteLog( "Calling ItemDB() ctor 1", LogBase.Severity.InformLowLevel );
        }

        public ItemDB( Guid currentUserId, string userName, int oldUserId )
            : base( DBCommon.TargetDatabases.Item )
        {
            //_log.SetCategory( LogBase.Category.DB );
            //_log.SetContext( "ItemDB", this.GetType() );
            //_log.WriteLog( "Calling ItemDB() ctor 2", LogBase.Severity.InformLowLevel );

            _currentUserId = currentUserId;
            _userName = userName;
            _oldUserId = oldUserId;
        }

        /* in R2.2, this version covers both parent and BPA items */
        public bool GetMedSurgItemCount2( int contractId, string contractNumber, ref int activeItemCount, ref int futureItemCount, ref int pricelessItemCount )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetItemCountForMedSurgContract2
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(50),
                //@ContractId int,
                //@ActiveItemCount int OUTPUT,
                //@FutureItemCount int OUTPUT,
                //@PricelessItemCount int OUTPUT                
                //)

                SqlCommand cmdGetItemCountQuery = new SqlCommand( "GetItemCountForMedSurgContract2", dbConnection );
                cmdGetItemCountQuery.CommandType = CommandType.StoredProcedure;
                cmdGetItemCountQuery.CommandTimeout = 30;


                AddStandardParameter( cmdGetItemCountQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmActiveItemCount = new SqlParameter( "@ActiveItemCount", SqlDbType.Int );
                parmActiveItemCount.Direction = ParameterDirection.Output;
                SqlParameter parmFutureItemCount = new SqlParameter( "@FutureItemCount", SqlDbType.Int );
                parmFutureItemCount.Direction = ParameterDirection.Output;
                SqlParameter parmPricelessItemCount = new SqlParameter( "@PricelessItemCount", SqlDbType.Int );
                parmPricelessItemCount.Direction = ParameterDirection.Output;

                parmContractNumber.Value = contractNumber;
                parmContractId.Value = contractId;

                cmdGetItemCountQuery.Parameters.Add( parmContractNumber );
                cmdGetItemCountQuery.Parameters.Add( parmContractId );
                cmdGetItemCountQuery.Parameters.Add( parmActiveItemCount );
                cmdGetItemCountQuery.Parameters.Add( parmFutureItemCount );
                cmdGetItemCountQuery.Parameters.Add( parmPricelessItemCount );

                // connect
                dbConnection.Open();

                cmdGetItemCountQuery.ExecuteNonQuery();

                activeItemCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@ActiveItemCount" ].Value.ToString() );
                futureItemCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@FutureItemCount" ].Value.ToString() );
                pricelessItemCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@PricelessItemCount" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.GetMedSurgItemCount(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetMedSurgItemCount( int contractId, string contractNumber, ref int activeItemCount, ref int futureItemCount )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            // contractId is not used

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetItemCountForMedSurgContract
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@ActiveItemCount int OUTPUT,
                //@FutureItemCount int OUTPUT  
                //)

                SqlCommand cmdGetItemCountQuery = new SqlCommand( "GetItemCountForMedSurgContract", dbConnection );
                cmdGetItemCountQuery.CommandType = CommandType.StoredProcedure;
                cmdGetItemCountQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmActiveItemCount = new SqlParameter( "@ActiveItemCount", SqlDbType.Int );
                parmActiveItemCount.Direction = ParameterDirection.Output;
                SqlParameter parmFutureItemCount = new SqlParameter( "@FutureItemCount", SqlDbType.Int );
                parmFutureItemCount.Direction = ParameterDirection.Output;


                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;

                cmdGetItemCountQuery.Parameters.Add( parmCurrentUser );
                cmdGetItemCountQuery.Parameters.Add( parmContractNumber );
                cmdGetItemCountQuery.Parameters.Add( parmActiveItemCount );
                cmdGetItemCountQuery.Parameters.Add( parmFutureItemCount );

                // connect
                dbConnection.Open();

                cmdGetItemCountQuery.ExecuteNonQuery();

                activeItemCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@ActiveItemCount" ].Value.ToString() );
                futureItemCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@FutureItemCount" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetMedSurgItemCount(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetMedSurgBPAItemCount( int contractId, string contractNumber, ref int activeItemCount, ref int futureItemCount )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            // contractId is not used

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                //GetItemCountForMedSurgBPAContract
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@ActiveItemCount int OUTPUT,
                //@FutureItemCount int OUTPUT  
                //)

                SqlCommand cmdGetItemCountQuery = new SqlCommand( "GetItemCountForMedSurgBPAContract", dbConnection );
                cmdGetItemCountQuery.CommandType = CommandType.StoredProcedure;
                cmdGetItemCountQuery.CommandTimeout = 30;


                AddStandardParameter( cmdGetItemCountQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmActiveItemCount = new SqlParameter( "@ActiveItemCount", SqlDbType.Int );
                parmActiveItemCount.Direction = ParameterDirection.Output;
                SqlParameter parmFutureItemCount = new SqlParameter( "@FutureItemCount", SqlDbType.Int );
                parmFutureItemCount.Direction = ParameterDirection.Output;

                parmContractNumber.Value = contractNumber;

                cmdGetItemCountQuery.Parameters.Add( parmContractNumber );
                cmdGetItemCountQuery.Parameters.Add( parmActiveItemCount );
                cmdGetItemCountQuery.Parameters.Add( parmFutureItemCount );

                // connect
                dbConnection.Open();

                cmdGetItemCountQuery.ExecuteNonQuery();

                activeItemCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@ActiveItemCount" ].Value.ToString() );
                futureItemCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@FutureItemCount" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ContractDB.GetMedSurgBPAItemCount(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool RestoreHistoricalItem( int itemHistoryId, int contractId, int itemId, int modificationStatusId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                // RestoreHistoricalItem
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ItemHistoryId int,
                //@ContractId int,
                //@ItemId int,
                //@ModificationStatusId int,
                //@LastModificationType nchar(1)

                SqlCommand cmdRestoreHistoricalItemQuery = new SqlCommand( "RestoreHistoricalItem", dbConnection );
                cmdRestoreHistoricalItemQuery.CommandType = CommandType.StoredProcedure;
                cmdRestoreHistoricalItemQuery.CommandTimeout = 30;

                AddStandardParameter( cmdRestoreHistoricalItemQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdRestoreHistoricalItemQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmItemHistoryId = new SqlParameter( "@ItemHistoryId", SqlDbType.Int );
                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmItemId = new SqlParameter( "@ItemId", SqlDbType.Int );
                SqlParameter parmModificationStatusId = new SqlParameter( "@ModificationStatusId", SqlDbType.Int );
                SqlParameter parmLastModificationType = new SqlParameter( "@LastModificationType", SqlDbType.NChar, 1 );

                parmItemHistoryId.Value = itemHistoryId;
                parmContractId.Value = contractId;
                parmItemId.Value = itemId;
                parmModificationStatusId.Value = modificationStatusId;
                parmLastModificationType.Value = "R"; // Restored

                cmdRestoreHistoricalItemQuery.Parameters.Add( parmItemHistoryId );
                cmdRestoreHistoricalItemQuery.Parameters.Add( parmContractId );
                cmdRestoreHistoricalItemQuery.Parameters.Add( parmItemId );
                cmdRestoreHistoricalItemQuery.Parameters.Add( parmModificationStatusId );
                cmdRestoreHistoricalItemQuery.Parameters.Add( parmLastModificationType );

                // connect
                dbConnection.Open();

                cmdRestoreHistoricalItemQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.RestoreHistoricalItem(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string MedSurgItemDetailsTableName = "ItemDetailsTable";

        /* used when retrieving details of parent item when displaying selected parent info in a BPA item during edit -- GRID ONLY, NOT DETAILS SCREEN */
        public bool GetParentItemDetails( ref DataSet dsItemDetails, string contractNumber, int itemId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItemDetails = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetMedSurgParentItemDetails
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@ItemId int

                SqlCommand cmdSelectItemDetailsQuery = new SqlCommand( "GetMedSurgParentItemDetails", dbConnection );
                cmdSelectItemDetailsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectItemDetailsQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectItemDetailsQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmItemId = new SqlParameter( "@ItemId", SqlDbType.Int );

                parmContractNumber.Value = contractNumber;
                parmItemId.Value = itemId;

                cmdSelectItemDetailsQuery.Parameters.Add( parmContractNumber );
                cmdSelectItemDetailsQuery.Parameters.Add( parmItemId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItemDetails = new SqlDataAdapter();
                daItemDetails.SelectCommand = cmdSelectItemDetailsQuery;

                dsItemDetails = new DataSet( "ItemDetails" );
                DataTable dtItemDetails = dsItemDetails.Tables.Add( MedSurgItemDetailsTableName );

                // add the common elements to the table
                DataColumn itemIdColumn = new DataColumn( "ItemId", typeof( int ) );

                dtItemDetails.Columns.Add( itemIdColumn );

                dtItemDetails.Columns.Add( "CatalogNumber", typeof( string ) );                            
                dtItemDetails.Columns.Add( "ItemDescription", typeof( string ) );
                dtItemDetails.Columns.Add( "SIN", typeof( string ) );              
                dtItemDetails.Columns.Add( "ServiceCategoryId", typeof( int ) );
                dtItemDetails.Columns.Add( "PackageAsPriced", typeof( string ) );             
                dtItemDetails.Columns.Add( "ParentItemId", typeof( int ) );
                dtItemDetails.Columns.Add( "ParentActive", typeof( bool ) );
                dtItemDetails.Columns.Add( "ParentHistorical", typeof( bool ) );
                dtItemDetails.Columns.Add( "ItemHistoryId", typeof( int ) );
                dtItemDetails.Columns.Add( "Restorable", typeof( bool ) );
                dtItemDetails.Columns.Add( "LastModificationType", typeof( string ) );
                dtItemDetails.Columns.Add( "ModificationStatusId", typeof( int ) );
                dtItemDetails.Columns.Add( "LastModifiedBy", typeof( string ) );
                dtItemDetails.Columns.Add( "LastModificationDate", typeof( DateTime ) );
                                             
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = itemIdColumn;

                // add the keys to the table
                dtItemDetails.PrimaryKey = primaryKeyColumns;

                dtItemDetails.Clear();

                // connect
                dbConnection.Open();

                // run
                daItemDetails.Fill( dsItemDetails, MedSurgItemDetailsTableName );

                RowsReturned = dsItemDetails.Tables[ MedSurgItemDetailsTableName ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.GetItemDetails(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        /* used when retrieving details for the selected item's item details screen */
        public bool GetMedSurgItemDetails( ref DataSet dsItemDetails, string contractNumber, int itemId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItemDetails = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetMedSurgItemDetails
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@ItemId int

                SqlCommand cmdSelectItemDetailsQuery = new SqlCommand( "GetMedSurgItemDetails", dbConnection );
                cmdSelectItemDetailsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectItemDetailsQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectItemDetailsQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmItemId = new SqlParameter( "@ItemId", SqlDbType.Int );

                parmContractNumber.Value = contractNumber;
                parmItemId.Value = itemId;

                cmdSelectItemDetailsQuery.Parameters.Add( parmContractNumber );
                cmdSelectItemDetailsQuery.Parameters.Add( parmItemId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItemDetails = new SqlDataAdapter();
                daItemDetails.SelectCommand = cmdSelectItemDetailsQuery;

                dsItemDetails = new DataSet( "ItemDetails" );
                DataTable dtItemDetails = dsItemDetails.Tables.Add( MedSurgItemDetailsTableName );

                // add the common elements to the table
                DataColumn itemIdColumn = new DataColumn( "ItemId", typeof( int ) );

                dtItemDetails.Columns.Add( itemIdColumn );

                dtItemDetails.Columns.Add( "CatalogNumber", typeof( string ) );
                dtItemDetails.Columns.Add( "ManufacturersCatalogNumber", typeof( string ) );
                dtItemDetails.Columns.Add( "ManufacturersName", typeof( string ) );
                dtItemDetails.Columns.Add( "LetterOfCommitmentDate", typeof( DateTime ) );
                dtItemDetails.Columns.Add( "CommercialListPrice", typeof( decimal ) );
                dtItemDetails.Columns.Add( "CommercialPricelistDate", typeof( DateTime ) );
                dtItemDetails.Columns.Add( "CommercialPricelistFOBTerms", typeof( string ) );
                dtItemDetails.Columns.Add( "ManufacturersCommercialListPrice", typeof( decimal ) );
                dtItemDetails.Columns.Add( "TrackingMechanism", typeof( string ) );
                dtItemDetails.Columns.Add( "AcquisitionCost", typeof( decimal ) );
                dtItemDetails.Columns.Add( "TypeOfContractor", typeof( string ) );
            //    dtItemDetails.Columns.Add( "CountryOfOrigin", typeof( string ) );

                dtItemDetails.Columns.Add( "ItemDescription", typeof( string ) );
                dtItemDetails.Columns.Add( "SIN", typeof( string ) );
                dtItemDetails.Columns.Add( "ServiceCategoryId", typeof( int ) );
                dtItemDetails.Columns.Add( "PackageAsPriced", typeof( string ) );
                dtItemDetails.Columns.Add( "ParentItemId", typeof( int ) );
                dtItemDetails.Columns.Add( "ParentActive", typeof( bool ) );
                dtItemDetails.Columns.Add( "ParentHistorical", typeof( bool ) );
                dtItemDetails.Columns.Add( "ItemHistoryId", typeof( int ) );
                dtItemDetails.Columns.Add( "Restorable", typeof( bool ) );
                dtItemDetails.Columns.Add( "LastModificationType", typeof( string ) );
                dtItemDetails.Columns.Add( "ModificationStatusId", typeof( int ) );
                dtItemDetails.Columns.Add( "LastModifiedBy", typeof( string ) );
                dtItemDetails.Columns.Add( "LastModificationDate", typeof( DateTime ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = itemIdColumn;

                // add the keys to the table
                dtItemDetails.PrimaryKey = primaryKeyColumns;

                dtItemDetails.Clear();

                // connect
                dbConnection.Open();

                // run
                daItemDetails.Fill( dsItemDetails, MedSurgItemDetailsTableName );

                RowsReturned = dsItemDetails.Tables[ MedSurgItemDetailsTableName ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.GetItemDetails(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string ServiceCategoryDetailsTableName = "ServiceCategoryDetailsTable";

        public bool GetServiceCategoryDetails( ref DataSet serviceCategoryDetailsDataSet, string contractNumber, int contractId, int serviceCategoryId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daServiceCategoryDetails = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetServiceCategoryDetails
                //(
                //@UserLogin nvarchar(120),
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@ContractId int,
                //@ServiceCategoryId int

                SqlCommand cmdSelectServiceCategoryDetailsQuery = new SqlCommand( "GetServiceCategoryDetails", dbConnection );
                cmdSelectServiceCategoryDetailsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectServiceCategoryDetailsQuery.CommandTimeout = 30;

                SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
                AddStandardParameter( cmdSelectServiceCategoryDetailsQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmServiceCategoryId = new SqlParameter( "@ServiceCategoryId", SqlDbType.Int );

                parmUserLogin.Value = _userName;
                parmContractNumber.Value = contractNumber;
                parmContractId.Value = contractId;
                parmServiceCategoryId.Value = serviceCategoryId;

                cmdSelectServiceCategoryDetailsQuery.Parameters.Add( parmUserLogin );
                cmdSelectServiceCategoryDetailsQuery.Parameters.Add( parmContractNumber );
                cmdSelectServiceCategoryDetailsQuery.Parameters.Add( parmContractId );
                cmdSelectServiceCategoryDetailsQuery.Parameters.Add( parmServiceCategoryId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daServiceCategoryDetails = new SqlDataAdapter();
                daServiceCategoryDetails.SelectCommand = cmdSelectServiceCategoryDetailsQuery;

                serviceCategoryDetailsDataSet = new DataSet( "ServiceCategoryDetails" );
                DataTable dtServiceCategoryDetails = serviceCategoryDetailsDataSet.Tables.Add( ServiceCategoryDetailsTableName );

                // add the common elements to the table
                DataColumn serviceCategoryIdColumn = new DataColumn( "ServiceCategoryId", typeof( int ) );

                dtServiceCategoryDetails.Columns.Add( serviceCategoryIdColumn );

                dtServiceCategoryDetails.Columns.Add( "ServiceCategoryDescription", typeof( string ) );
                dtServiceCategoryDetails.Columns.Add( "SIN", typeof( string ) );

                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = serviceCategoryIdColumn;

                // add the keys to the table
                dtServiceCategoryDetails.PrimaryKey = primaryKeyColumns;

                dtServiceCategoryDetails.Clear();

                // connect
                dbConnection.Open();

                // run
                daServiceCategoryDetails.Fill( serviceCategoryDetailsDataSet, ServiceCategoryDetailsTableName );

                RowsReturned = serviceCategoryDetailsDataSet.Tables[ ServiceCategoryDetailsTableName ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.GetServiceCategoryDetails(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

      

        public bool ValidateMedSurgPriceAgainstOtherPrices( string contractNumber, int contractId, int itemPriceId, int itemId, DateTime priceStartDate, DateTime priceStopDate, 
            bool bIsTemporary, ref bool bIsPriceOk, ref bool bUserCanOverride, ref string validationMessage )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //ValidateMedSurgPriceAgainstOtherPrices
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(20),
                //@ContractId int,
                //@ItemPriceId int,
                //@ItemId int,
                //@PriceStartDate datetime,         
                //@PriceStopDate datetime,     
                //@IsTemporary bit,
                //@IsPriceOk bit OUTPUT,
                //@UserCanOverride bit OUTPUT,
                //@ValidationMessage nvarchar(1250) OUTPUT                           	                  


                SqlCommand cmdValidatePriceQuery = new SqlCommand( "ValidateMedSurgPriceAgainstOtherPrices", dbConnection );
                cmdValidatePriceQuery.CommandType = CommandType.StoredProcedure;
                cmdValidatePriceQuery.CommandTimeout = 30;

                AddStandardParameter( cmdValidatePriceQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdValidatePriceQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );
        
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                SqlParameter parmItemPriceId = new SqlParameter( "@ItemPriceId", SqlDbType.Int );
                SqlParameter parmItemId = new SqlParameter( "@ItemId", SqlDbType.Int );
                SqlParameter parmPriceStartDate = new SqlParameter( "@PriceStartDate", SqlDbType.DateTime );
                SqlParameter parmPriceStopDate = new SqlParameter( "@PriceStopDate", SqlDbType.DateTime );
                SqlParameter parmIsTemporary = new SqlParameter( "@IsTemporary", SqlDbType.Bit );
                SqlParameter parmIsPriceOk = new SqlParameter( "@IsPriceOk", SqlDbType.Bit );
                parmIsPriceOk.Direction = ParameterDirection.Output;
                SqlParameter parmUserCanOverride = new SqlParameter( "@UserCanOverride", SqlDbType.Bit );
                parmUserCanOverride.Direction = ParameterDirection.Output;
                SqlParameter parmValidationMessage = new SqlParameter( "@ValidationMessage", SqlDbType.NVarChar, 1250 );
                parmValidationMessage.Direction = ParameterDirection.Output;

 
                parmContractNumber.Value = contractNumber;
                parmContractId.Value = contractId;

                parmItemPriceId.Value = itemPriceId;
                parmItemId.Value = itemId;
                parmPriceStartDate.Value = priceStartDate;
                parmPriceStopDate.Value = priceStopDate;
                parmIsTemporary.Value = bIsTemporary;

                cmdValidatePriceQuery.Parameters.Add( parmContractNumber );
                cmdValidatePriceQuery.Parameters.Add( parmContractId );
                cmdValidatePriceQuery.Parameters.Add( parmItemPriceId );
                cmdValidatePriceQuery.Parameters.Add( parmItemId );
                cmdValidatePriceQuery.Parameters.Add( parmPriceStartDate );
                cmdValidatePriceQuery.Parameters.Add( parmPriceStopDate );
                cmdValidatePriceQuery.Parameters.Add( parmIsTemporary );
                cmdValidatePriceQuery.Parameters.Add( parmIsPriceOk );
                cmdValidatePriceQuery.Parameters.Add( parmValidationMessage );
                cmdValidatePriceQuery.Parameters.Add( parmUserCanOverride );

                // connect
                dbConnection.Open();

                cmdValidatePriceQuery.ExecuteNonQuery();

                bIsPriceOk = bool.Parse( cmdValidatePriceQuery.Parameters[ "@IsPriceOk" ].Value.ToString() );
                bUserCanOverride = bool.Parse( cmdValidatePriceQuery.Parameters[ "@UserCanOverride" ].Value.ToString() );
                validationMessage = cmdValidatePriceQuery.Parameters[ "@ValidationMessage" ].Value.ToString();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.ValidateMedSurgPriceAgainstOtherPrices(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }



        public bool GetVAIFF( string contractNumber, DateTime effectiveDate, out decimal VAIFF )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataReader reader = null;
            VAIFF = 0;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //create function [GetVAIFF]
                //(
                //@ContractNumber nvarchar(20),
                //@EffectiveDate datetime
                //returns decimal( 18,4 )

                SqlCommand cmdGetVAIFFQuery = new SqlCommand( "select dbo.[GetVAIFF]( @ContractNumber, @EffectiveDate )", dbConnection );
                cmdGetVAIFFQuery.CommandType = CommandType.Text;
                cmdGetVAIFFQuery.CommandTimeout = 30;

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmEffectiveDate = new SqlParameter( "@EffectiveDate", SqlDbType.DateTime );

                parmContractNumber.Value = contractNumber;
                parmEffectiveDate.Value = effectiveDate;

                cmdGetVAIFFQuery.Parameters.Add( parmContractNumber );
                cmdGetVAIFFQuery.Parameters.Add( parmEffectiveDate );

                // connect
                dbConnection.Open();

                reader = cmdGetVAIFFQuery.ExecuteReader();
                reader.Read();

                VAIFF = ( decimal )reader.GetValue( 0 );

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.GetVAIFF(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

 
   

        public bool GetParentItemDescription( int parentItemId, out string overallDescription )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            overallDescription = "";

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //GetParentItemDescription
                //( 
                //@ItemId int,
                //@ItemDescription nvarchar(800) OUTPUT

                SqlCommand cmdGetParentItemDescriptionQuery = new SqlCommand( "GetParentItemDescription", dbConnection );
                cmdGetParentItemDescriptionQuery.CommandType = CommandType.StoredProcedure;
                cmdGetParentItemDescriptionQuery.CommandTimeout = 30;

                SqlParameter parmParentItemId = new SqlParameter( "@ItemId", SqlDbType.Int );
                SqlParameter parmItemDescription = new SqlParameter( "@ItemDescription", SqlDbType.NVarChar, 800 );
                parmItemDescription.Direction = ParameterDirection.Output;

                parmParentItemId.Value = parentItemId;

                cmdGetParentItemDescriptionQuery.Parameters.Add( parmParentItemId );
                cmdGetParentItemDescriptionQuery.Parameters.Add( parmItemDescription );

                // connect
                dbConnection.Open();

                cmdGetParentItemDescriptionQuery.ExecuteNonQuery();


                overallDescription = cmdGetParentItemDescriptionQuery.Parameters[ "@ItemDescription" ].Value.ToString();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.GetParentItemDescription(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public const string MedSurgItemCountriesOfOriginTableName = "ItemCountriesTable";

        public bool GetMedSurgItemCountriesOfOrigin( ref DataSet dsItemCountries, int itemId ) 
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daItemCountries = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //SelectCountriesOfOriginForItem
                //(
                //@CurrentUser uniqueidentifier,
                //@ItemId int

                SqlCommand cmdSelectItemCountriesQuery = new SqlCommand( "SelectCountriesOfOriginForItem", dbConnection );
                cmdSelectItemCountriesQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectItemCountriesQuery.CommandTimeout = 30;

                AddStandardParameter( cmdSelectItemCountriesQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                SqlParameter parmItemId = new SqlParameter( "@ItemId", SqlDbType.Int );

                parmItemId.Value = itemId;

                cmdSelectItemCountriesQuery.Parameters.Add( parmItemId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daItemCountries = new SqlDataAdapter();
                daItemCountries.SelectCommand = cmdSelectItemCountriesQuery;

                dsItemCountries = new DataSet( "ItemCountriesOfOrigin" );
                DataTable dtItemCountries = dsItemCountries.Tables.Add( MedSurgItemCountriesOfOriginTableName );

                //select c.CountryId, c.CountryName, case when ( j.CountryId is not null ) then 1 else 0 end as IsSelected, j.ItemCountryId
                // add the common elements to the table
                DataColumn countryIdColumn = new DataColumn( "CountryId", typeof( int ) );

                dtItemCountries.Columns.Add( countryIdColumn );
                
                dtItemCountries.Columns.Add( "ItemCountryId", typeof( int ) );
                dtItemCountries.Columns.Add( "CountryName", typeof( string ) );
                dtItemCountries.Columns.Add( "IsSelected", typeof( bool ) );
               
            
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = countryIdColumn;

                // add the keys to the table
                dtItemCountries.PrimaryKey = primaryKeyColumns;

                dtItemCountries.Clear();

                // connect
                dbConnection.Open();

                // run
                daItemCountries.Fill( dsItemCountries, MedSurgItemCountriesOfOriginTableName );

                RowsReturned = dsItemCountries.Tables[ MedSurgItemCountriesOfOriginTableName ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.GetMedSurgItemCountriesOfOrigin(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool InsertCountryOfOriginForItem( int itemId, int countryId, ref int itemCountryId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //InsertCountryOfOriginForItem
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar( 255 ),
                //@SecurityDatabaseName nvarchar(255),
                //@ItemId int,
                //@CountryId int,
                //@ModificationStatusId int,
                //@ModificationType nchar( 1 ),
                //@ItemCountryId int OUTPUT
                //)

                SqlCommand cmdInsertCountryOfOriginForItemQuery = new SqlCommand( "InsertCountryOfOriginForItem", dbConnection );
                cmdInsertCountryOfOriginForItemQuery.CommandType = CommandType.StoredProcedure;
                cmdInsertCountryOfOriginForItemQuery.CommandTimeout = 30;

                AddStandardParameter( cmdInsertCountryOfOriginForItemQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdInsertCountryOfOriginForItemQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmItemId = new SqlParameter( "@ItemId", SqlDbType.Int );
                SqlParameter parmCountryId = new SqlParameter( "@CountryId", SqlDbType.Int );
                SqlParameter parmModificationStatusId = new SqlParameter( "@ModificationStatusId", SqlDbType.Int );              
                SqlParameter parmModificationType = new SqlParameter( "@ModificationType", SqlDbType.NChar, 1 );
                SqlParameter parmItemCountryId = new SqlParameter( "@ItemCountryId", SqlDbType.Int );
                parmItemCountryId.Direction = ParameterDirection.Output;

                parmItemId.Value = itemId;
                parmCountryId.Value = countryId;
                parmModificationStatusId.Value = -1;
                parmModificationType.Value = "C";

                cmdInsertCountryOfOriginForItemQuery.Parameters.Add( parmItemId );
                cmdInsertCountryOfOriginForItemQuery.Parameters.Add( parmCountryId );
                cmdInsertCountryOfOriginForItemQuery.Parameters.Add( parmModificationStatusId );
                cmdInsertCountryOfOriginForItemQuery.Parameters.Add( parmModificationType );
                cmdInsertCountryOfOriginForItemQuery.Parameters.Add( parmItemCountryId );

                // connect
                dbConnection.Open();

                cmdInsertCountryOfOriginForItemQuery.ExecuteNonQuery();

                itemCountryId = int.Parse( cmdInsertCountryOfOriginForItemQuery.Parameters[ "@ItemCountryId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.InsertCountryOfOriginForItem(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool DeleteCountryOfOriginForItem( int itemId, int countryId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //DeleteCountryOfOriginForItem
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar( 255 ),
                //@SecurityDatabaseName nvarchar(255),
                //@ItemId int,
                //@CountryId int,
                //@ModificationStatusId int,
                //@ModificationType nchar( 1 )

                SqlCommand cmdDeleteCountryOfOriginForItemQuery = new SqlCommand( "DeleteCountryOfOriginForItem", dbConnection );
                cmdDeleteCountryOfOriginForItemQuery.CommandType = CommandType.StoredProcedure;
                cmdDeleteCountryOfOriginForItemQuery.CommandTimeout = 30;

                AddStandardParameter( cmdDeleteCountryOfOriginForItemQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdDeleteCountryOfOriginForItemQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmItemId = new SqlParameter( "@ItemId", SqlDbType.Int );
                SqlParameter parmCountryId = new SqlParameter( "@CountryId", SqlDbType.Int );
                SqlParameter parmModificationStatusId = new SqlParameter( "@ModificationStatusId", SqlDbType.Int );
                SqlParameter parmModificationType = new SqlParameter( "@ModificationType", SqlDbType.NChar, 1 );             

                parmItemId.Value = itemId;
                parmCountryId.Value = countryId;
                parmModificationStatusId.Value = -1;
                parmModificationType.Value = "C";

                cmdDeleteCountryOfOriginForItemQuery.Parameters.Add( parmItemId );
                cmdDeleteCountryOfOriginForItemQuery.Parameters.Add( parmCountryId );
                cmdDeleteCountryOfOriginForItemQuery.Parameters.Add( parmModificationStatusId );
                cmdDeleteCountryOfOriginForItemQuery.Parameters.Add( parmModificationType );

                // connect
                dbConnection.Open();

                cmdDeleteCountryOfOriginForItemQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in ItemDB.DeleteCountryOfOriginForItem(): {0}", ex.Message );
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

        protected ItemDB( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
            _currentUserId = ( Guid )info.GetValue( "CurrentUserId", typeof( Guid ) );
            _userName = info.GetString( "UserName" );
            _oldUserId = info.GetInt32( "OldUserId" );
            RestoreDelegatesAfterDeserialization( this, "ItemDB" );
        }

        #endregion

 
    }
}
