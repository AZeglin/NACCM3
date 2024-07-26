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
    // provides ado.net access to drug item releated stored procedures in the DrugItem database
    [Serializable]
    public class DrugItemDB : DBCommon, ISerializable 
    {
     //   private NACLog _log = new NACLog();

        private Guid _currentUserId = Guid.Empty;
        private string _userName = string.Empty;
        private int _oldUserId = -1;

        public DrugItemDB()
            : base( DBCommon.TargetDatabases.DrugItem )
        {
            //_log.SetCategory( LogBase.Category.DB );
            //_log.SetContext( "DrugItemDB", this.GetType() );
            //_log.WriteLog( "Calling DrugItemDB() ctor 1", LogBase.Severity.InformLowLevel );
        }

        public DrugItemDB( Guid currentUserId, string userName, int oldUserId )
            : base( DBCommon.TargetDatabases.DrugItem )
        {
            //_log.SetCategory( LogBase.Category.DB );
            //_log.SetContext( "DrugItemDB", this.GetType() );
            //_log.WriteLog( "Calling DrugItemDB() ctor 2", LogBase.Severity.InformLowLevel );

            _currentUserId = currentUserId;
            _userName = userName;
            _oldUserId = oldUserId;
        }

        public bool GetDrugItemCount( string contractNumber, ref int itemCount )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetItemCountForDrugContract
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@ItemCount int OUTPUT
                //)

                SqlCommand cmdGetItemCountQuery = new SqlCommand( "GetItemCountForDrugContract", dbConnection );
                cmdGetItemCountQuery.CommandType = CommandType.StoredProcedure;
                cmdGetItemCountQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmItemCount = new SqlParameter( "@ItemCount", SqlDbType.Int );
                parmItemCount.Direction = ParameterDirection.Output;

                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;

                cmdGetItemCountQuery.Parameters.Add( parmCurrentUser );
                cmdGetItemCountQuery.Parameters.Add( parmContractNumber );
                cmdGetItemCountQuery.Parameters.Add( parmItemCount );

                // connect
                dbConnection.Open();

                cmdGetItemCountQuery.ExecuteNonQuery();

                itemCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@ItemCount" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.GetDrugItemCount(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // new
        public bool DiscontinueItem( string contractNumber, int drugItemId, DateTime discontinuationDate, string discontinuationReason, int modificationStatusId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //DiscontinueFSSDrugItemFromContract
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(20),
                //@DrugItemId int,
                //@DiscontinuationDate datetime,
                //@DiscontinuationReason nvarchar(512),
                //@ModificationStatusId int,
                //@LastModificationType nchar(1)

                SqlCommand cmdDiscontinueItemQuery = new SqlCommand( "DiscontinueFSSDrugItemFromContract", dbConnection ); // renamed from DiscontinueDrugItemFromContract on 7/13
                cmdDiscontinueItemQuery.CommandType = CommandType.StoredProcedure;
                cmdDiscontinueItemQuery.CommandTimeout = 30;

                AddStandardParameter( cmdDiscontinueItemQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdDiscontinueItemQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                //      SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );
                SqlParameter parmDiscontinuationDate = new SqlParameter( "@DiscontinuationDate", SqlDbType.DateTime );
                SqlParameter parmDiscontinuationReason = new SqlParameter( "@DiscontinuationReason", SqlDbType.NVarChar, 512 );
                SqlParameter parmModificationStatusId = new SqlParameter( "@ModificationStatusId", SqlDbType.Int );
                SqlParameter parmLastModificationType = new SqlParameter( "@LastModificationType", SqlDbType.NChar, 1 );

                //       parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;
                parmDrugItemId.Value = drugItemId;
                parmDiscontinuationDate.Value = discontinuationDate;
                parmDiscontinuationReason.Value = discontinuationReason;
                parmModificationStatusId.Value = modificationStatusId;
                parmLastModificationType.Value = "C"; // CO edit

                //        cmdDiscontinueItemQuery.Parameters.Add( parmCurrentUser );
                cmdDiscontinueItemQuery.Parameters.Add( parmContractNumber );
                cmdDiscontinueItemQuery.Parameters.Add( parmDrugItemId );
                cmdDiscontinueItemQuery.Parameters.Add( parmDiscontinuationDate );
                cmdDiscontinueItemQuery.Parameters.Add( parmDiscontinuationReason );
                cmdDiscontinueItemQuery.Parameters.Add( parmModificationStatusId );
                cmdDiscontinueItemQuery.Parameters.Add( parmLastModificationType );

                // connect
                dbConnection.Open();

                cmdDiscontinueItemQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.DiscontinueItem(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // old
        //public bool DiscontinueItem( string contractNumber, int drugItemId, DateTime discontinuationDate, int modificationStatusId )
        //{
        //    bool bSuccess = true;
        //    SqlConnection dbConnection = null;

        //    try
        //    {
        //        dbConnection = new SqlConnection( ConnectionString );

        //        // set up the call to the stored procedure
        //        //DiscontinueFSSDrugItemFromContract
        //        //(
        //        //@CurrentUser uniqueidentifier,
        //        //@SecurityServerName nvarchar(255),
        //        //@SecurityDatabaseName nvarchar(255),
        //        //@ContractNumber nvarchar(20),
        //        //@DrugItemId int,
        //        //@DiscontinuationDate datetime,
        //        //@ModificationStatusId int,
        //        //@LastModificationType nchar(1)

        //        SqlCommand cmdDiscontinueItemQuery = new SqlCommand( "DiscontinueFSSDrugItemFromContract", dbConnection ); // renamed from DiscontinueDrugItemFromContract on 7/13
        //        cmdDiscontinueItemQuery.CommandType = CommandType.StoredProcedure;
        //        cmdDiscontinueItemQuery.CommandTimeout = 30;

        //        AddStandardParameter( cmdDiscontinueItemQuery, StandardParameterTypes.CurrentUser, _currentUserId );
        //        AddStandardParameter( cmdDiscontinueItemQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

        //  //      SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
        //        SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
        //        SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );
        //        SqlParameter parmDiscontinuationDate = new SqlParameter( "@DiscontinuationDate", SqlDbType.DateTime );
        //        SqlParameter parmModificationStatusId = new SqlParameter( "@ModificationStatusId", SqlDbType.Int );
        //        SqlParameter parmLastModificationType = new SqlParameter( "@LastModificationType", SqlDbType.NChar, 1 );

        // //       parmCurrentUser.Value = _currentUserId;
        //        parmContractNumber.Value = contractNumber;
        //        parmDrugItemId.Value = drugItemId;
        //        parmDiscontinuationDate.Value = discontinuationDate;
        //        parmModificationStatusId.Value = modificationStatusId;
        //        parmLastModificationType.Value = "C"; // CO edit

        ////        cmdDiscontinueItemQuery.Parameters.Add( parmCurrentUser );
        //        cmdDiscontinueItemQuery.Parameters.Add( parmContractNumber );
        //        cmdDiscontinueItemQuery.Parameters.Add( parmDrugItemId );
        //        cmdDiscontinueItemQuery.Parameters.Add( parmDiscontinuationDate );
        //        cmdDiscontinueItemQuery.Parameters.Add( parmModificationStatusId );
        //        cmdDiscontinueItemQuery.Parameters.Add( parmLastModificationType );

        //        // connect
        //        dbConnection.Open();

        //        cmdDiscontinueItemQuery.ExecuteNonQuery();

        //    }
        //    catch( Exception ex )
        //    {
        //        ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.DiscontinueItem(): {0}", ex.Message );
        //        bSuccess = false;
        //    }
        //    finally
        //    {
        //        if( dbConnection != null )
        //            dbConnection.Close();
        //    }

        //    return ( bSuccess );
        //}

        public bool RestoreDiscontinuedItem( string contractNumber, int drugItemId, int modificationStatusId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                // RestoreDiscontinuedItem
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(20),
                //@DrugItemId int,
                //@ModificationStatusId int,
                //@LastModificationType nchar(1)

                SqlCommand cmdRestoreDiscontinuedItemQuery = new SqlCommand( "RestoreDiscontinuedItem", dbConnection ); 
                cmdRestoreDiscontinuedItemQuery.CommandType = CommandType.StoredProcedure;
                cmdRestoreDiscontinuedItemQuery.CommandTimeout = 30;

                AddStandardParameter( cmdRestoreDiscontinuedItemQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdRestoreDiscontinuedItemQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );
                SqlParameter parmModificationStatusId = new SqlParameter( "@ModificationStatusId", SqlDbType.Int );
                SqlParameter parmLastModificationType = new SqlParameter( "@LastModificationType", SqlDbType.NChar, 1 );

                parmContractNumber.Value = contractNumber;
                parmDrugItemId.Value = drugItemId;
                parmModificationStatusId.Value = modificationStatusId;
                parmLastModificationType.Value = "R"; // Restored

                cmdRestoreDiscontinuedItemQuery.Parameters.Add( parmContractNumber );
                cmdRestoreDiscontinuedItemQuery.Parameters.Add( parmDrugItemId );
                cmdRestoreDiscontinuedItemQuery.Parameters.Add( parmModificationStatusId );
                cmdRestoreDiscontinuedItemQuery.Parameters.Add( parmLastModificationType );

                // connect
                dbConnection.Open();

                cmdRestoreDiscontinuedItemQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.RestoreDiscontinuedItem(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool GetCoveredFCPCountForDrugContract( string contractNumber, ref int coveredCount, ref int fcpCount, ref int ppvCount )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetCoveredFCPCountForDrugContract
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@CoveredCount int OUTPUT,
                //@FCPCount int OUTPUT,
                //@PPVCount int OUTPUT

                SqlCommand cmdGetItemCountQuery = new SqlCommand( "GetCoveredFCPCountForDrugContract", dbConnection );
                cmdGetItemCountQuery.CommandType = CommandType.StoredProcedure;
                cmdGetItemCountQuery.CommandTimeout = 30;

                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmCoveredCount = new SqlParameter( "@CoveredCount", SqlDbType.Int );
                parmCoveredCount.Direction = ParameterDirection.Output;
                SqlParameter parmFCPCount = new SqlParameter( "@FCPCount", SqlDbType.Int );
                parmFCPCount.Direction = ParameterDirection.Output;
                SqlParameter parmPPVCount = new SqlParameter( "@PPVCount", SqlDbType.Int );
                parmPPVCount.Direction = ParameterDirection.Output;

                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;

                cmdGetItemCountQuery.Parameters.Add( parmCurrentUser );
                cmdGetItemCountQuery.Parameters.Add( parmContractNumber );
                cmdGetItemCountQuery.Parameters.Add( parmCoveredCount );
                cmdGetItemCountQuery.Parameters.Add( parmFCPCount );
                cmdGetItemCountQuery.Parameters.Add( parmPPVCount );

                // connect
                dbConnection.Open();

                cmdGetItemCountQuery.ExecuteNonQuery();

                coveredCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@CoveredCount" ].Value.ToString() );
                fcpCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@FCPCount" ].Value.ToString() );
                ppvCount = int.Parse( cmdGetItemCountQuery.Parameters[ "@PPVCount" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.GetCoveredFCPCountForDrugContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool GetDrugItemDetails( ref DataSet dsDrugItemDetails, string contractNumber, int drugItemId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataAdapter daDrugItemDetails = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //GetFSSDrugItemDetails
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@DrugItemId int

                SqlCommand cmdSelectDrugItemDetailsQuery = new SqlCommand( "GetFSSDrugItemDetails", dbConnection );
                cmdSelectDrugItemDetailsQuery.CommandType = CommandType.StoredProcedure;
                cmdSelectDrugItemDetailsQuery.CommandTimeout = 30;

                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );

                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;
                parmDrugItemId.Value = drugItemId;

                cmdSelectDrugItemDetailsQuery.Parameters.Add( parmCurrentUser );
                cmdSelectDrugItemDetailsQuery.Parameters.Add( parmContractNumber );
                cmdSelectDrugItemDetailsQuery.Parameters.Add( parmDrugItemId );

                // create a data adapter and dataset to 
                // run the query and hold the results
                daDrugItemDetails = new SqlDataAdapter();
                daDrugItemDetails.SelectCommand = cmdSelectDrugItemDetailsQuery;

                dsDrugItemDetails = new DataSet( "DrugItemDetails" );
                DataTable dtDrugItemDetails = dsDrugItemDetails.Tables.Add( "DrugItemDetailsTable" );

                // add the common elements to the table
                DataColumn drugItemIdColumn = new DataColumn( "DrugItemId", typeof( int ) );

                dtDrugItemDetails.Columns.Add( drugItemIdColumn );
                dtDrugItemDetails.Columns.Add( "DrugItemNDCId", typeof( int ) );
                dtDrugItemDetails.Columns.Add( "FdaAssignedLabelerCode", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "ProductCode", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "PackageCode", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "PackageDescription", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "Generic", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "TradeName", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "DiscontinuationDate", typeof( DateTime ) );
                dtDrugItemDetails.Columns.Add( "DiscontinuationEnteredDate", typeof( DateTime ) );
                dtDrugItemDetails.Columns.Add( "NDCLinkId", typeof( int ) );
                dtDrugItemDetails.Columns.Add( "Covered", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "PrimeVendor", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "PrimeVendorChangedDate", typeof( DateTime ) );
                dtDrugItemDetails.Columns.Add( "FCP", typeof( decimal ) );
                dtDrugItemDetails.Columns.Add( "CurrentFSSPrice", typeof( decimal ) );
                dtDrugItemDetails.Columns.Add( "PriceStartDate", typeof( DateTime ) );
                dtDrugItemDetails.Columns.Add( "PriceEndDate", typeof( DateTime ) );
                dtDrugItemDetails.Columns.Add( "DateEnteredMarket", typeof( DateTime ) );
                dtDrugItemDetails.Columns.Add( "PassThrough", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "DispensingUnit", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "VAClass", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "DualPriceDesignation", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "ExcludeFromExport", typeof( bool ) );
                dtDrugItemDetails.Columns.Add( "NonTAA", typeof( bool ) );
                dtDrugItemDetails.Columns.Add( "IncludedFETAmount", typeof( float ) );
                dtDrugItemDetails.Columns.Add( "LastModificationType", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "ModificationStatusId", typeof( int ) );
                dtDrugItemDetails.Columns.Add( "CreatedBy", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "CreationDate", typeof( DateTime ) );
                dtDrugItemDetails.Columns.Add( "LastModifiedBy", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "LastModificationDate", typeof( DateTime ) );
                dtDrugItemDetails.Columns.Add( "DrugItemPackageId", typeof( int ) );
                dtDrugItemDetails.Columns.Add( "UnitOfSale", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "QuantityInUnitOfSale", typeof( decimal ) );
                dtDrugItemDetails.Columns.Add( "UnitPackage", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "QuantityInUnitPackage", typeof( decimal ) );
                dtDrugItemDetails.Columns.Add( "UnitOfMeasure", typeof( string ) );
                dtDrugItemDetails.Columns.Add( "PriceMultiplier", typeof( int ) );
                dtDrugItemDetails.Columns.Add( "PriceDivider", typeof( int ) );
               
                
                // create array of primary key columns
                DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
                primaryKeyColumns[ 0 ] = drugItemIdColumn;

                // add the keys to the table
                dtDrugItemDetails.PrimaryKey = primaryKeyColumns;

                dtDrugItemDetails.Clear();

                // connect
                dbConnection.Open();

                // run
                daDrugItemDetails.Fill( dsDrugItemDetails, "DrugItemDetailsTable" );

                RowsReturned = dsDrugItemDetails.Tables[ "DrugItemDetailsTable" ].Rows.Count;

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.GetDrugItemDetails(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }




        public bool ValidatePriceAgainstOtherPrices( string contractNumber, int drugItemPriceId, int drugItemId, DateTime priceStartDate, DateTime priceStopDate, bool bIsTemporary,
            bool bIsFSS, bool bIsBIG4, bool bIsVA, bool bIsBOP, bool bIsCMOP, bool bIsDOD, bool bIsHHS,
            bool bIsIHS, bool bIsIHS2, bool bIsDIHS, bool bIsNIH, bool bIsPHS, bool bIsSVH, bool bIsSVH1,
            bool bIsSVH2, bool bIsTMOP, bool bIsUSCG, bool bIsFHCC, int drugItemSubItemId, ref bool bIsPriceOk, ref bool bUserCanOverride, ref string validationMessage )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //ValidatePriceAgainstOtherPrices
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(20),
                //@DrugItemPriceId int,
                //@DrugItemId int,
                //@PriceStartDate datetime,         
                //@PriceStopDate datetime,       
                //@IsTemporary bit,   
                //@IsFSS bit,                                           	                  
                //@IsBIG4 bit,                                          	                  
                //@IsVA bit,                                            	                  
                //@IsBOP bit,                                           	                  
                //@IsCMOP bit,                                          	                  
                //@IsDOD bit,                                           	                  
                //@IsHHS bit,                                           	                  
                //@IsIHS bit,                                           	                  
                //@IsIHS2 bit,                                          	                  
                //@IsDIHS bit,                                          	                  
                //@IsNIH bit,                                           	                  
                //@IsPHS bit,                                           	                  
                //@IsSVH bit,                                           	                  
                //@IsSVH1 bit,                                          	                  
                //@IsSVH2 bit,                                          	                  
                //@IsTMOP bit,                                          	                  
                //@IsUSCG bit,
                //@IsFHCC bit,
                //@DrugItemSubItemId int,
                //@IsPriceOk bit OUTPUT,
                //@UserCanOverride bit OUTPUT,
                //@ValidationMessage nvarchar(1250) OUTPUT                           	                  


                SqlCommand cmdValidatePriceQuery = new SqlCommand( "ValidatePriceAgainstOtherPrices", dbConnection );
                cmdValidatePriceQuery.CommandType = CommandType.StoredProcedure;
                cmdValidatePriceQuery.CommandTimeout = 30;

                AddStandardParameter( cmdValidatePriceQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdValidatePriceQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );

           //     SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmDrugItemPriceId = new SqlParameter( "@DrugItemPriceId", SqlDbType.Int );
                SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );
                SqlParameter parmPriceStartDate = new SqlParameter( "@PriceStartDate", SqlDbType.DateTime );
                SqlParameter parmPriceStopDate = new SqlParameter( "@PriceStopDate", SqlDbType.DateTime );
                SqlParameter parmIsTemporary = new SqlParameter( "@IsTemporary", SqlDbType.Bit );
                SqlParameter parmIsFSS = new SqlParameter( "@IsFSS", SqlDbType.Bit );
                SqlParameter parmIsBIG4 = new SqlParameter( "@IsBIG4", SqlDbType.Bit );
                SqlParameter parmIsVA = new SqlParameter( "@IsVA", SqlDbType.Bit );
                SqlParameter parmIsBOP = new SqlParameter( "@IsBOP", SqlDbType.Bit );
                SqlParameter parmIsCMOP = new SqlParameter( "@IsCMOP", SqlDbType.Bit );
                SqlParameter parmIsDOD = new SqlParameter( "@IsDOD", SqlDbType.Bit );
                SqlParameter parmIsHHS = new SqlParameter( "@IsHHS", SqlDbType.Bit );
                SqlParameter parmIsIHS = new SqlParameter( "@IsIHS", SqlDbType.Bit );
                SqlParameter parmIsIHS2 = new SqlParameter( "@IsIHS2", SqlDbType.Bit );
                SqlParameter parmIsDIHS = new SqlParameter( "@IsDIHS", SqlDbType.Bit );
                SqlParameter parmIsNIH = new SqlParameter( "@IsNIH", SqlDbType.Bit );
                SqlParameter parmIsPHS = new SqlParameter( "@IsPHS", SqlDbType.Bit );
                SqlParameter parmIsSVH = new SqlParameter( "@IsSVH", SqlDbType.Bit );
                SqlParameter parmIsSVH1 = new SqlParameter( "@IsSVH1", SqlDbType.Bit );
                SqlParameter parmIsSVH2 = new SqlParameter( "@IsSVH2", SqlDbType.Bit );
                SqlParameter parmIsTMOP = new SqlParameter( "@IsTMOP", SqlDbType.Bit );
                SqlParameter parmIsUSCG = new SqlParameter( "@IsUSCG", SqlDbType.Bit );
                SqlParameter parmIsFHCC = new SqlParameter( "@IsFHCC", SqlDbType.Bit );
                SqlParameter parmDrugItemSubItemId = new SqlParameter( "@DrugItemSubItemId", SqlDbType.Int );
                SqlParameter parmIsPriceOk = new SqlParameter( "@IsPriceOk", SqlDbType.Bit );
                parmIsPriceOk.Direction = ParameterDirection.Output;
                SqlParameter parmUserCanOverride = new SqlParameter( "@UserCanOverride", SqlDbType.Bit );
                parmUserCanOverride.Direction = ParameterDirection.Output;
                SqlParameter parmValidationMessage = new SqlParameter( "@ValidationMessage", SqlDbType.NVarChar, 1250 );
                parmValidationMessage.Direction = ParameterDirection.Output;

       //         parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;

                parmDrugItemPriceId.Value = drugItemPriceId;
                parmDrugItemId.Value = drugItemId;
                parmPriceStartDate.Value = priceStartDate;
                parmPriceStopDate.Value = priceStopDate;
                parmIsTemporary.Value = bIsTemporary;
                parmIsFSS.Value = bIsFSS;
                parmIsBIG4.Value = bIsBIG4;
                parmIsVA.Value = bIsVA;
                parmIsBOP.Value = bIsBOP;
                parmIsCMOP.Value = bIsCMOP;
                parmIsDOD.Value = bIsDOD;
                parmIsHHS.Value = bIsHHS;
                parmIsIHS.Value = bIsIHS;
                parmIsIHS2.Value = bIsIHS2;
                parmIsDIHS.Value = bIsDIHS;
                parmIsNIH.Value =  bIsNIH;
                parmIsPHS.Value = bIsPHS;
                parmIsSVH.Value = bIsSVH;
                parmIsSVH1.Value = bIsSVH1;
                parmIsSVH2.Value = bIsSVH2;
                parmIsTMOP.Value = bIsTMOP;
                parmIsUSCG.Value = bIsUSCG;
                parmIsFHCC.Value = bIsFHCC;
                if( drugItemSubItemId != -1 )
                    parmDrugItemSubItemId.Value = drugItemSubItemId;
                else
                    parmDrugItemSubItemId.SqlValue = DBNull.Value;

   //             cmdValidatePriceQuery.Parameters.Add( parmCurrentUser );
                cmdValidatePriceQuery.Parameters.Add( parmContractNumber );
                cmdValidatePriceQuery.Parameters.Add( parmDrugItemPriceId );
                cmdValidatePriceQuery.Parameters.Add( parmDrugItemId );
                cmdValidatePriceQuery.Parameters.Add( parmPriceStartDate );
                cmdValidatePriceQuery.Parameters.Add( parmPriceStopDate );
                cmdValidatePriceQuery.Parameters.Add( parmIsTemporary );
                cmdValidatePriceQuery.Parameters.Add( parmIsFSS );
                cmdValidatePriceQuery.Parameters.Add( parmIsBIG4 );
                cmdValidatePriceQuery.Parameters.Add( parmIsVA );
                cmdValidatePriceQuery.Parameters.Add( parmIsBOP );
                cmdValidatePriceQuery.Parameters.Add( parmIsCMOP );
                cmdValidatePriceQuery.Parameters.Add( parmIsDOD );
                cmdValidatePriceQuery.Parameters.Add( parmIsHHS );
                cmdValidatePriceQuery.Parameters.Add( parmIsIHS );
                cmdValidatePriceQuery.Parameters.Add( parmIsIHS2 );
                cmdValidatePriceQuery.Parameters.Add( parmIsDIHS );
                cmdValidatePriceQuery.Parameters.Add( parmIsNIH );
                cmdValidatePriceQuery.Parameters.Add( parmIsPHS );
                cmdValidatePriceQuery.Parameters.Add( parmIsSVH );
                cmdValidatePriceQuery.Parameters.Add( parmIsSVH1 );
                cmdValidatePriceQuery.Parameters.Add( parmIsSVH2 );
                cmdValidatePriceQuery.Parameters.Add( parmIsTMOP );
                cmdValidatePriceQuery.Parameters.Add( parmIsUSCG );
                cmdValidatePriceQuery.Parameters.Add( parmIsFHCC );
                cmdValidatePriceQuery.Parameters.Add( parmDrugItemSubItemId );
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
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.ValidatePriceAgainstOtherPrices(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool IsItemDualPricer( int drugItemId, out bool isItemDualPricer )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataReader reader = null;
            isItemDualPricer = false;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //GetItemDualPriceStatusForDrugItemId
                //(
                //@DrugItemId int
                //)

                //returns bit

                SqlCommand cmdIsItemDualPricerQuery = new SqlCommand( "select dbo.[GetItemDualPriceStatusForDrugItemId]( @DrugItemId )", dbConnection );
                cmdIsItemDualPricerQuery.CommandType = CommandType.Text;
                cmdIsItemDualPricerQuery.CommandTimeout = 30;


                SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );

                parmDrugItemId.Value = drugItemId;

                cmdIsItemDualPricerQuery.Parameters.Add( parmDrugItemId );

                // connect
                dbConnection.Open();

                reader = cmdIsItemDualPricerQuery.ExecuteReader();
                reader.Read();

                isItemDualPricer = ( bool )reader.GetValue( 0 );

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.IsItemDualPricer(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetItemIncludedFETAmount( int drugItemId, out Decimal FETAmount )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            SqlDataReader reader = null;
            FETAmount = 0;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //GetIncludedFETAmountForDrugItemId
                //(
                //@DrugItemId int
                //)

                //returns decimal

                SqlCommand cmdGetItemFETAmountQuery = new SqlCommand( "select dbo.[GetIncludedFETAmountForDrugItemId]( @DrugItemId )", dbConnection );
                cmdGetItemFETAmountQuery.CommandType = CommandType.Text;
                cmdGetItemFETAmountQuery.CommandTimeout = 30;


                SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );

                parmDrugItemId.Value = drugItemId;

                cmdGetItemFETAmountQuery.Parameters.Add( parmDrugItemId );

                // connect
                dbConnection.Open();

                reader = cmdGetItemFETAmountQuery.ExecuteReader();
                reader.Read();

                FETAmount = ( Decimal )reader.GetValue( 0 );

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.GetItemIncludedFETAmount(): {0}", ex.Message );
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
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.GetVAIFF(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // make an entry in the DrugItem database for a new contract
        // this is no longer used
        public bool CreateContract( string contractNumber, int newContractRecordId, int scheduleNumber, out int drugItemDBContractId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            drugItemDBContractId = -1;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //CreateDrugItemContract
                //(
                //@CurrentUser uniqueidentifier,
                //@ContractNumber nvarchar(20),
                //@ContractRecordId int,
                //@ModificationStatusId int,
                //@ScheduleNumber int,
                //@ContractId int OUTPUT

                SqlCommand cmdCreateContractQuery = new SqlCommand( "CreateDrugItemContract", dbConnection ); 
                cmdCreateContractQuery.CommandType = CommandType.StoredProcedure;
                cmdCreateContractQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmContractRecordId = new SqlParameter( "@ContractRecordId", SqlDbType.Int );
                SqlParameter parmModificationStatusId = new SqlParameter( "@ModificationStatusId", SqlDbType.Int );
                SqlParameter parmScheduleNumber = new SqlParameter( "@ScheduleNumber", SqlDbType.Int );
                SqlParameter parmContractId = new SqlParameter( "@ContractId", SqlDbType.Int );
                parmContractId.Direction = ParameterDirection.Output;

                parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;
                parmContractRecordId.Value = newContractRecordId;
                parmScheduleNumber.Value = scheduleNumber;
                parmModificationStatusId.Value = -1;

                cmdCreateContractQuery.Parameters.Add( parmCurrentUser );
                cmdCreateContractQuery.Parameters.Add( parmContractNumber );
                cmdCreateContractQuery.Parameters.Add( parmContractRecordId );
                cmdCreateContractQuery.Parameters.Add( parmModificationStatusId );
                cmdCreateContractQuery.Parameters.Add( parmScheduleNumber );
                cmdCreateContractQuery.Parameters.Add( parmContractId );

                // connect
                dbConnection.Open();

                cmdCreateContractQuery.ExecuteNonQuery();

                drugItemDBContractId = int.Parse( cmdCreateContractQuery.Parameters[ "@ContractId" ].Value.ToString() );
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.CreateContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool UpdateChangeRequestStatus( int changeRequestId, string changeRequestStatus )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //UpdateChangeRequestStatus
                //(
                //@CurrentUser uniqueidentifier,
                //@ChangeRequestId int,
                //@ChangeRequestStatus nchar(1)   -- D Draft, S Submitted, R Reviewed, A Accepted, X Rejected, I Implemented

                SqlCommand cmdUpdateChangeRequestStatusQuery = new SqlCommand( "UpdateChangeRequestStatus", dbConnection );
                cmdUpdateChangeRequestStatusQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateChangeRequestStatusQuery.CommandTimeout = 30;


                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmChangeRequestId = new SqlParameter( "@ChangeRequestId", SqlDbType.Int );
                SqlParameter parmChangeRequestStatus = new SqlParameter( "@ChangeRequestStatus", SqlDbType.NChar, 1 );

                parmCurrentUser.Value = _currentUserId;
                parmChangeRequestId.Value = changeRequestId;
                parmChangeRequestStatus.Value = changeRequestStatus;

                cmdUpdateChangeRequestStatusQuery.Parameters.Add( parmCurrentUser );
                cmdUpdateChangeRequestStatusQuery.Parameters.Add( parmChangeRequestId );
                cmdUpdateChangeRequestStatusQuery.Parameters.Add( parmChangeRequestStatus );

                // connect
                dbConnection.Open();

                cmdUpdateChangeRequestStatusQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.UpdateChangeRequestStatus(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        public bool GetParentDrugItemDescription( int parentDrugItemId, out string overallDescription )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;
            overallDescription = "";

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //GetParentDrugItemDescription
                //( 
                //@ParentDrugItemId int,
                //@OverallDescription nvarchar(200) OUTPUT

                SqlCommand cmdGetParentDrugItemDescriptionQuery = new SqlCommand( "GetParentDrugItemDescription", dbConnection );
                cmdGetParentDrugItemDescriptionQuery.CommandType = CommandType.StoredProcedure;
                cmdGetParentDrugItemDescriptionQuery.CommandTimeout = 30;

                SqlParameter parmParentDrugItemId = new SqlParameter( "@ParentDrugItemId", SqlDbType.Int );
                SqlParameter parmOverallDescription = new SqlParameter( "@OverallDescription", SqlDbType.NVarChar, 200 );
                parmOverallDescription.Direction = ParameterDirection.Output;

                parmParentDrugItemId.Value = parentDrugItemId;

                cmdGetParentDrugItemDescriptionQuery.Parameters.Add( parmParentDrugItemId );
                cmdGetParentDrugItemDescriptionQuery.Parameters.Add( parmOverallDescription );

                // connect
                dbConnection.Open();

                cmdGetParentDrugItemDescriptionQuery.ExecuteNonQuery();


                overallDescription = cmdGetParentDrugItemDescriptionQuery.Parameters[ "@OverallDescription" ].Value.ToString();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.GetParentDrugItemDescription(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // updates the parent contract info contained in the DrugItem database DI_Contracts table using parent info already present in that table
        public bool UpdateParentDrugItemContract( string contractNumber, string parentContractNumber )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //UpdateParentDrugItemContract
                //(
                //@ContractNumber nvarchar(20),
                //@ParentContractNumber nvarchar(20)

                SqlCommand cmdUpdateParentContractQuery = new SqlCommand( "UpdateParentDrugItemContract", dbConnection );
                cmdUpdateParentContractQuery.CommandType = CommandType.StoredProcedure;
                cmdUpdateParentContractQuery.CommandTimeout = 30;

                SqlParameter parmContractNumber= new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmParentContractNumber = new SqlParameter( "@ParentContractNumber", SqlDbType.NVarChar, 20 );

                parmContractNumber.Value = contractNumber;
                parmParentContractNumber.Value = parentContractNumber;

                cmdUpdateParentContractQuery.Parameters.Add( parmContractNumber );
                cmdUpdateParentContractQuery.Parameters.Add( parmParentContractNumber );

                // connect
                dbConnection.Open();

                cmdUpdateParentContractQuery.ExecuteNonQuery();
            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.UpdateParentDrugItemContract(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // Change NDC from item context menu 
        public bool ChangeNDC( string contractNumber, int drugItemId, string newFDAAssignedLablerCode, string newProductCode, string newPacakgeCode, DateTime discontinuationDateForOldNDC, DateTime effectiveDateForNewNDC, bool bCopyPricing, bool bCopySubItems, int modificationStatusId, out int newDrugItemId, out int newDrugItemNDCId, out int newDrugItemPackageId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            newDrugItemId = -1;
            newDrugItemNDCId = -1;
            newDrugItemPackageId = -1;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //ChangeNDC
                //(
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@NACCMServerName nvarchar(255),
                //@NACCMDatabaseName nvarchar(255),
                //@ContractNumber nvarchar(20),
                //@DrugItemId int,
                //@FdaAssignedLabelerCode char(5),
                //@ProductCode char(4),
                //@PackageCode char(2),
                //@DiscontinuationDate datetime NULL,
                //@EffectiveDate datetime,
                //@CopyPrices bit,
                //@CopySubItems bit,
                //@ModificationStatusId int,
                //@NewDrugItemId int OUTPUT,
                //@NewDrugItemNDCId int OUTPUT,
                //@NewDrugItemPackageId int OUTPUT


                SqlCommand cmdChangeNDCQuery = new SqlCommand( "ChangeNDC", dbConnection ); 
                cmdChangeNDCQuery.CommandType = CommandType.StoredProcedure;
                cmdChangeNDCQuery.CommandTimeout = 30;

                AddStandardParameter( cmdChangeNDCQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdChangeNDCQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );
                AddStandardParameter( cmdChangeNDCQuery, StandardParameterTypes.NACCMDatabase, Config.ContractManagerDatabaseServer, Config.ContractManagerDatabase );

         //       SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmContractNumber = new SqlParameter( "@ContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );
                SqlParameter parmFdaAssignedLabelerCode = new SqlParameter( "@FdaAssignedLabelerCode", SqlDbType.NVarChar, 5 );
                SqlParameter parmProductCode = new SqlParameter( "@ProductCode", SqlDbType.NVarChar, 4 );
                SqlParameter parmPackageCode = new SqlParameter( "@PackageCode", SqlDbType.NVarChar, 2 );
                SqlParameter parmDiscontinuationDate = new SqlParameter( "@DiscontinuationDate", SqlDbType.DateTime );
                SqlParameter parmEffectiveDate = new SqlParameter( "@EffectiveDate", SqlDbType.DateTime );
                SqlParameter parmCopyPrices = new SqlParameter( "@CopyPrices", SqlDbType.Bit );
                SqlParameter parmCopySubItems = new SqlParameter( "@CopySubItems", SqlDbType.Bit );
                SqlParameter parmModificationStatusId = new SqlParameter( "@ModificationStatusId", SqlDbType.Int );
                SqlParameter parmNewDrugItemId = new SqlParameter( "@NewDrugItemId", SqlDbType.Int );
                parmNewDrugItemId.Direction = ParameterDirection.Output;
                SqlParameter parmNewDrugItemNDCId = new SqlParameter( "@NewDrugItemNDCId", SqlDbType.Int );
                parmNewDrugItemNDCId.Direction = ParameterDirection.Output;
                SqlParameter parmNewDrugItemPackageId = new SqlParameter( "@NewDrugItemPackageId", SqlDbType.Int );
                parmNewDrugItemPackageId.Direction = ParameterDirection.Output;

         //       parmCurrentUser.Value = _currentUserId;
                parmContractNumber.Value = contractNumber;
                parmDrugItemId.Value = drugItemId;
                parmFdaAssignedLabelerCode.Value = newFDAAssignedLablerCode;
                parmProductCode.Value = newProductCode;
                parmPackageCode.Value = newPacakgeCode;
                parmCopyPrices.Value = bCopyPricing;
                parmCopySubItems.Value = bCopySubItems;
                parmModificationStatusId.Value = modificationStatusId;

                if( discontinuationDateForOldNDC.CompareTo( DateTime.MinValue ) == 0 )
                {
                    parmDiscontinuationDate.Value = DBNull.Value;
                }
                else
                {
                    parmDiscontinuationDate.Value = discontinuationDateForOldNDC;
                }

                parmEffectiveDate.Value = effectiveDateForNewNDC;
      //          cmdChangeNDCQuery.Parameters.Add( parmCurrentUser );
                cmdChangeNDCQuery.Parameters.Add( parmContractNumber );
                cmdChangeNDCQuery.Parameters.Add( parmDrugItemId );
                cmdChangeNDCQuery.Parameters.Add( parmFdaAssignedLabelerCode );
                cmdChangeNDCQuery.Parameters.Add( parmProductCode );
                cmdChangeNDCQuery.Parameters.Add( parmPackageCode );
                cmdChangeNDCQuery.Parameters.Add( parmDiscontinuationDate );
                cmdChangeNDCQuery.Parameters.Add( parmEffectiveDate );
                cmdChangeNDCQuery.Parameters.Add( parmCopyPrices );
                cmdChangeNDCQuery.Parameters.Add( parmCopySubItems );
                cmdChangeNDCQuery.Parameters.Add( parmModificationStatusId );
                cmdChangeNDCQuery.Parameters.Add( parmNewDrugItemId );
                cmdChangeNDCQuery.Parameters.Add( parmNewDrugItemNDCId );
                cmdChangeNDCQuery.Parameters.Add( parmNewDrugItemPackageId );

                // connect
                dbConnection.Open();

                cmdChangeNDCQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.ChangeNDC(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        // copy item from item context menu 
        public bool CopyItem( string copyType, string sourceContractNumber, int drugItemId, string newFDAAssignedLablerCode, string newProductCode, string newPacakgeCode, string tradeName, string genericName, string destinationContractNumber, bool bCopyPricing, bool bCopySubItems, int modificationStatusId, string dispensingUnit, string packageDescription, string unitOfSale, decimal quantityInUnitOfSale, string unitPackage, decimal quantityInUnitPackage, string unitOfMeasure, out int newDrugItemId, out int newDrugItemNDCId, out int newDrugItemPackageId, out int destinationContractId )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            newDrugItemId = -1;
            newDrugItemNDCId = -1;
            newDrugItemPackageId = -1;
            destinationContractId = -1;

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the stored procedure
                //CopyItem
                //(             
                //@CurrentUser uniqueidentifier,
                //@SecurityServerName nvarchar(255),
                //@SecurityDatabaseName nvarchar(255),
                //@NACCMServerName nvarchar(255),
                //@NACCMDatabaseName nvarchar(255),
                //@CopyType nvarchar(20),
                //@SourceContractNumber nvarchar(20),
                //@DrugItemId int,
                //@FdaAssignedLabelerCode char(5),
                //@ProductCode char(4),
                //@PackageCode char(2),
                //@TradeName nvarchar(45),
                //@GenericName nvarchar(64),
                //@DispensingUnit nvarchar(10),
                //@PackageDescription nvarchar(14),
                //@UnitOfSale nchar(2),
                //@QuantityInUnitOfSale decimal(5,0),
                //@UnitPackage nchar(2),
                //@QuantityInUnitPackage decimal(13,5),
                //@UnitOfMeasure nchar(2),
                //@DestinationContractNumber nvarchar(20),
                //@CopyPrices bit,
                //@CopySubItems bit,
                //@ModificationStatusId int,
                //@NewDrugItemId int OUTPUT,
                //@NewDrugItemNDCId int OUTPUT,
                //@NewDrugItemPackageId int OUTPUT,
                //@DestinationContractId int OUTPUT


                SqlCommand cmdChangeNDCQuery = new SqlCommand( "CopyItem", dbConnection );
                cmdChangeNDCQuery.CommandType = CommandType.StoredProcedure;
                cmdChangeNDCQuery.CommandTimeout = 30;

                AddStandardParameter( cmdChangeNDCQuery, StandardParameterTypes.CurrentUser, _currentUserId );
                AddStandardParameter( cmdChangeNDCQuery, StandardParameterTypes.SecurityDatabase, Config.SecurityDatabaseServer, Config.SecurityDatabase );
                AddStandardParameter( cmdChangeNDCQuery, StandardParameterTypes.NACCMDatabase, Config.ContractManagerDatabaseServer, Config.ContractManagerDatabase );

       //         SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmCopyType = new SqlParameter( "@CopyType", SqlDbType.NVarChar, 20 );
                SqlParameter parmSourceContractNumber = new SqlParameter( "@SourceContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );
                SqlParameter parmFdaAssignedLabelerCode = new SqlParameter( "@FdaAssignedLabelerCode", SqlDbType.NVarChar, 5 );
                SqlParameter parmProductCode = new SqlParameter( "@ProductCode", SqlDbType.NVarChar, 4 );
                SqlParameter parmPackageCode = new SqlParameter( "@PackageCode", SqlDbType.NVarChar, 2 );
                SqlParameter parmTradeName = new SqlParameter( "@TradeName", SqlDbType.NVarChar, 45 );
                SqlParameter parmGenericName = new SqlParameter( "@GenericName", SqlDbType.NVarChar, 64 );
                SqlParameter parmDispensingUnit = new SqlParameter( "@DispensingUnit", SqlDbType.NVarChar, 10 );
                SqlParameter parmPackageDescription = new SqlParameter( "@PackageDescription", SqlDbType.NVarChar, 14 );
                SqlParameter parmUnitOfSale = new SqlParameter( "@UnitOfSale", SqlDbType.NChar, 2 );
                SqlParameter parmQuantityInUnitOfSale = new SqlParameter( "@QuantityInUnitOfSale", SqlDbType.Decimal, 9 );
                SqlParameter parmUnitPackage = new SqlParameter( "@UnitPackage", SqlDbType.NChar, 2 );
                SqlParameter parmQuantityInUnitPackage = new SqlParameter( "@QuantityInUnitPackage", SqlDbType.Decimal, 9 );
                SqlParameter parmUnitOfMeasure = new SqlParameter( "@UnitOfMeasure", SqlDbType.NChar, 2 );
                SqlParameter parmDestinationContractNumber = new SqlParameter( "@DestinationContractNumber", SqlDbType.NVarChar, 20 );
                SqlParameter parmCopyPrices = new SqlParameter( "@CopyPrices", SqlDbType.Bit );
                SqlParameter parmCopySubItems = new SqlParameter( "@CopySubItems", SqlDbType.Bit );
                SqlParameter parmModificationStatusId = new SqlParameter( "@ModificationStatusId", SqlDbType.Int );
                SqlParameter parmNewDrugItemId = new SqlParameter( "@NewDrugItemId", SqlDbType.Int );
                parmNewDrugItemId.Direction = ParameterDirection.Output;
                SqlParameter parmNewDrugItemNDCId = new SqlParameter( "@NewDrugItemNDCId", SqlDbType.Int );
                parmNewDrugItemNDCId.Direction = ParameterDirection.Output;
                SqlParameter parmNewDrugItemPackageId = new SqlParameter( "@NewDrugItemPackageId", SqlDbType.Int );
                parmNewDrugItemPackageId.Direction = ParameterDirection.Output;
                SqlParameter parmDestinationContractId = new SqlParameter( "@DestinationContractId", SqlDbType.Int );
                parmDestinationContractId.Direction = ParameterDirection.Output;

      //          parmCurrentUser.Value = _currentUserId;
                parmCopyType.Value = copyType;
                parmSourceContractNumber.Value = sourceContractNumber;
                parmDrugItemId.Value = drugItemId;
                parmFdaAssignedLabelerCode.Value = newFDAAssignedLablerCode;
                parmProductCode.Value = newProductCode;
                parmPackageCode.Value = newPacakgeCode;
                parmTradeName.Value = tradeName;
                parmGenericName.Value = genericName;
                parmDispensingUnit.Value = dispensingUnit;
                parmPackageDescription.Value = packageDescription;
                parmUnitOfSale.Value = unitOfSale;
                parmQuantityInUnitOfSale.Value = quantityInUnitOfSale;
                parmUnitPackage.Value = unitPackage;
                parmQuantityInUnitPackage.Value = quantityInUnitPackage;
                parmUnitOfMeasure.Value = unitOfMeasure;
                parmDestinationContractNumber.Value = destinationContractNumber;
                parmCopyPrices.Value = bCopyPricing;
                parmCopySubItems.Value = bCopySubItems;
                parmModificationStatusId.Value = modificationStatusId;

      //          cmdChangeNDCQuery.Parameters.Add( parmCurrentUser );
                cmdChangeNDCQuery.Parameters.Add( parmCopyType );
                cmdChangeNDCQuery.Parameters.Add( parmSourceContractNumber );
                cmdChangeNDCQuery.Parameters.Add( parmDrugItemId );
                cmdChangeNDCQuery.Parameters.Add( parmFdaAssignedLabelerCode );
                cmdChangeNDCQuery.Parameters.Add( parmProductCode );
                cmdChangeNDCQuery.Parameters.Add( parmPackageCode );
                cmdChangeNDCQuery.Parameters.Add( parmTradeName );
                cmdChangeNDCQuery.Parameters.Add( parmGenericName );
                cmdChangeNDCQuery.Parameters.Add( parmDispensingUnit );
                cmdChangeNDCQuery.Parameters.Add( parmPackageDescription );
                cmdChangeNDCQuery.Parameters.Add( parmUnitOfSale );
                cmdChangeNDCQuery.Parameters.Add( parmQuantityInUnitOfSale );
                cmdChangeNDCQuery.Parameters.Add( parmUnitPackage );
                cmdChangeNDCQuery.Parameters.Add( parmQuantityInUnitPackage );
                cmdChangeNDCQuery.Parameters.Add( parmUnitOfMeasure );
                cmdChangeNDCQuery.Parameters.Add( parmDestinationContractNumber );
                cmdChangeNDCQuery.Parameters.Add( parmCopyPrices );
                cmdChangeNDCQuery.Parameters.Add( parmCopySubItems );
                cmdChangeNDCQuery.Parameters.Add( parmModificationStatusId );
                cmdChangeNDCQuery.Parameters.Add( parmNewDrugItemId );
                cmdChangeNDCQuery.Parameters.Add( parmNewDrugItemNDCId );
                cmdChangeNDCQuery.Parameters.Add( parmNewDrugItemPackageId );
                cmdChangeNDCQuery.Parameters.Add( parmDestinationContractId );

                // connect
                dbConnection.Open();

                cmdChangeNDCQuery.ExecuteNonQuery();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.CopyItem(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }

        public bool GetPackageInfoForItemCopy( int selectedDrugItemId, out string unitOfSale, out decimal quantityInUnitOfSale, out string unitPackage, out decimal quantityInUnitPackage, out string unitOfMeasure )
        {
            bool bSuccess = true;
            SqlConnection dbConnection = null;

            unitOfSale = "";
            quantityInUnitOfSale = 0;
            unitPackage = "";
            quantityInUnitPackage = 0;
            unitOfMeasure = "";

            try
            {
                dbConnection = new SqlConnection( ConnectionString );

                // set up the call to the function
                //GetPackageInfoForItemCopy
                //(
                //@CurrentUser uniqueidentifier,
                //@DrugItemId int,
                //@UnitOfSale nchar(2) OUTPUT,
                //@QuantityInUnitOfSale decimal(5,0) OUTPUT,
                //@UnitPackage nchar(2) OUTPUT,
                //@QuantityInUnitPackage decimal(13,5) OUTPUT,
                //@UnitOfMeasure nchar(2)  OUTPUT
                //)

                SqlCommand cmdGetPackageInfoForItemCopyQuery = new SqlCommand( "GetPackageInfoForItemCopy", dbConnection );
                cmdGetPackageInfoForItemCopyQuery.CommandType = CommandType.StoredProcedure;
                cmdGetPackageInfoForItemCopyQuery.CommandTimeout = 30;

                SqlParameter parmCurrentUser = new SqlParameter( "@CurrentUser", SqlDbType.UniqueIdentifier );
                SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );
                SqlParameter parmUnitOfSale = new SqlParameter( "@UnitOfSale", SqlDbType.NChar, 2 );
                parmUnitOfSale.Direction = ParameterDirection.Output;
                SqlParameter parmQuantityInUnitOfSale = new SqlParameter( "@QuantityInUnitOfSale", SqlDbType.Decimal, 9 );
                parmQuantityInUnitOfSale.Direction = ParameterDirection.Output;
                parmQuantityInUnitOfSale.Precision = 5;
                SqlParameter parmUnitPackage = new SqlParameter( "@UnitPackage", SqlDbType.NChar, 2 );
                parmUnitPackage.Direction = ParameterDirection.Output;
                SqlParameter parmQuantityInUnitPackage = new SqlParameter( "@QuantityInUnitPackage", SqlDbType.Decimal, 9 );
                parmQuantityInUnitPackage.Direction = ParameterDirection.Output;
                parmQuantityInUnitPackage.Precision = 13;
                parmQuantityInUnitPackage.Scale = 5;
                SqlParameter parmUnitOfMeasure = new SqlParameter( "@UnitOfMeasure", SqlDbType.NChar, 2 );
                parmUnitOfMeasure.Direction = ParameterDirection.Output;

                parmCurrentUser.Value = _currentUserId;
                parmDrugItemId.Value = selectedDrugItemId;

                cmdGetPackageInfoForItemCopyQuery.Parameters.Add( parmCurrentUser );
                cmdGetPackageInfoForItemCopyQuery.Parameters.Add( parmDrugItemId );
                cmdGetPackageInfoForItemCopyQuery.Parameters.Add( parmUnitOfSale );
                cmdGetPackageInfoForItemCopyQuery.Parameters.Add( parmQuantityInUnitOfSale );
                cmdGetPackageInfoForItemCopyQuery.Parameters.Add( parmUnitPackage );
                cmdGetPackageInfoForItemCopyQuery.Parameters.Add( parmQuantityInUnitPackage );
                cmdGetPackageInfoForItemCopyQuery.Parameters.Add( parmUnitOfMeasure );

                // connect
                dbConnection.Open();

                cmdGetPackageInfoForItemCopyQuery.ExecuteNonQuery();


                unitOfSale = cmdGetPackageInfoForItemCopyQuery.Parameters[ "@UnitOfSale" ].Value.ToString();
                quantityInUnitOfSale = decimal.Parse( cmdGetPackageInfoForItemCopyQuery.Parameters[ "@QuantityInUnitOfSale" ].Value.ToString() );
                unitPackage = cmdGetPackageInfoForItemCopyQuery.Parameters[ "@UnitPackage" ].Value.ToString();
                quantityInUnitPackage = decimal.Parse( cmdGetPackageInfoForItemCopyQuery.Parameters[ "@QuantityInUnitPackage" ].Value.ToString() );
                unitOfMeasure = cmdGetPackageInfoForItemCopyQuery.Parameters[ "@UnitOfMeasure" ].Value.ToString();

            }
            catch( Exception ex )
            {
                ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.GetPackageInfoForItemCopy(): {0}", ex.Message );
                bSuccess = false;
            }
            finally
            {
                if( dbConnection != null )
                    dbConnection.Close();
            }

            return ( bSuccess );
        }


        //public bool SelectSpecialtyDistributorsForDrugItem( ref DataSet dsSpecialtyDistributors, int drugItemId )
        //{
        //    bool bSuccess = true;
        //    SqlConnection dbConnection = null;
        //    SqlDataAdapter daSpecialtyDistributors = null;

        //    try
        //    {
        //        dbConnection = new SqlConnection( ConnectionString );

        //        // set up the call to the stored procedure
        //        //SelectSpecialtyDistributorsForDrugItem
        //        //(
        //        //@UserLogin nvarchar(120),
        //        //@CurrentUser uniqueidentifier,
        //        //@DrugItemId int
        //        //)

        //        SqlCommand cmdSelectSpecialtyDistributorsQuery = new SqlCommand( "SelectSpecialtyDistributorsForDrugItem", dbConnection );
        //        cmdSelectSpecialtyDistributorsQuery.CommandType = CommandType.StoredProcedure;
        //        cmdSelectSpecialtyDistributorsQuery.CommandTimeout = 30;

        //        AddStandardParameter( cmdSelectSpecialtyDistributorsQuery, StandardParameterTypes.CurrentUser, _currentUserId );

        //        SqlParameter parmUserLogin = new SqlParameter( "@UserLogin", SqlDbType.NVarChar, 120 );
        //        SqlParameter parmDrugItemId = new SqlParameter( "@DrugItemId", SqlDbType.Int );

        //        parmUserLogin.Value = _userName;
        //        parmDrugItemId.Value = drugItemId;

        //        cmdSelectSpecialtyDistributorsQuery.Parameters.Add( parmUserLogin );
        //        cmdSelectSpecialtyDistributorsQuery.Parameters.Add( parmDrugItemId );

        //        // create a data adapter and dataset to 
        //        // run the query and hold the results
        //        daSpecialtyDistributors = new SqlDataAdapter();
        //        daSpecialtyDistributors.SelectCommand = cmdSelectSpecialtyDistributorsQuery;

        //        dsSpecialtyDistributors = new DataSet( "SpecialtyDistributors" );
        //        DataTable dtSpecialtyDistributors = dsSpecialtyDistributors.Tables.Add( "SpecialtyDistributorsTable" );

        //        // add the common elements to the table
        //        DataColumn specialtyDistributorIdColumn = new DataColumn( "SpecialtyDistributorId", typeof( int ) );

        //        dtSpecialtyDistributors.Columns.Add( specialtyDistributorIdColumn );
        //        dtSpecialtyDistributors.Columns.Add( "SpecialtyDistributorName", typeof( string ) );


        //        // create array of primary key columns
        //        DataColumn[] primaryKeyColumns = new DataColumn[ 1 ];
        //        primaryKeyColumns[ 0 ] = specialtyDistributorIdColumn;

        //        // add the keys to the table
        //        dtSpecialtyDistributors.PrimaryKey = primaryKeyColumns;

        //        dtSpecialtyDistributors.Clear();

        //        // connect
        //        dbConnection.Open();

        //        // run
        //        daSpecialtyDistributors.Fill( dsSpecialtyDistributors, "SpecialtyDistributorsTable" );

        //        RowsReturned = dsSpecialtyDistributors.Tables[ "SpecialtyDistributorsTable" ].Rows.Count;

        //    }
        //    catch( Exception ex )
        //    {
        //        ErrorMessage = String.Format( "The following exception was encountered in DrugItemDB.SelectSpecialtyDistributors(): {0}", ex.Message );
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

        protected DrugItemDB( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
            _currentUserId = ( Guid )info.GetValue( "CurrentUserId", typeof( Guid ) );
            _userName = info.GetString( "UserName" );
            _oldUserId = info.GetInt32( "OldUserId" );
            RestoreDelegatesAfterDeserialization( this, "DrugItemDB" );
        }

        #endregion

 
    }
}
