using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Microsoft.ReportingServices.Common;
using Microsoft.ReportingServices.Interfaces;
using Microsoft.Reporting.Common;
using Microsoft.ReportingServices.WebServer;
using Microsoft.ReportingServices.Library;

using VA.NAC.Application.SharedObj;

namespace VA.NAC.ReportManager
{
    public class ReportCollection
    {
        ReportPathHelper _reportPathHelper = null;

        private Uri _reportingServiceUri = null;      

        public enum ReportCategories
        {
            Undefined,
            Contracts,
            Pharmaceutical,
            Fiscal,
            SBA,
            Sales,
            Offers,
            Security
        }

        public static ReportCategories GetReportCategoryEnumFromName( string categoryName )
        {
            ReportCategories reportCategory = ReportCategories.Undefined;

            if( categoryName.CompareTo( Enum.GetName( typeof( ReportCategories ), ReportCategories.Contracts ) ) == 0 )
            {
                reportCategory = ReportCategories.Contracts;
            }
            else if( categoryName.CompareTo( Enum.GetName( typeof( ReportCategories ), ReportCategories.Pharmaceutical ) ) == 0 )
            {
                reportCategory = ReportCategories.Pharmaceutical;
            }
            else if( categoryName.CompareTo( Enum.GetName( typeof( ReportCategories ), ReportCategories.Fiscal ) ) == 0 )
            {
                reportCategory = ReportCategories.Fiscal;
            }
            else if( categoryName.CompareTo( Enum.GetName( typeof( ReportCategories ), ReportCategories.SBA ) ) == 0 )
            {
                reportCategory = ReportCategories.SBA;
            }
            else if( categoryName.CompareTo( Enum.GetName( typeof( ReportCategories ), ReportCategories.Sales ) ) == 0 )
            {
                reportCategory = ReportCategories.Sales;
            }
            else if( categoryName.CompareTo( Enum.GetName( typeof( ReportCategories ), ReportCategories.Security ) ) == 0 )
            {
                reportCategory = ReportCategories.Security;
            }
            return ( reportCategory );
        }

        private string GetCacheNameFromCategory( ReportCategories reportCategory )
        {
            return ( string.Format( "{0}ReportNameCacheName", Enum.GetName( typeof( ReportCategories ), reportCategory ) ) );
        }

        public ReportCollection()
        {
        }

        public void Init()
        {
            _reportPathHelper = new ReportPathHelper();
            _reportingServiceUri = new Uri( string.Format( "{0}/ReportService2010.asmx", _reportPathHelper.ReportingServicesPath ) );         
        }

        public string GetReportExecutionPath( ReportCategories reportCategory, string reportName )
        {
                  
            // this worked for getting a directory listing but not launching the report
            // return ( String.Format( "http://{0}/Reports/Pages/Folder.aspx?ItemPath=/{1}/Reports/{2}&ViewMode=List", Config.ReportingServicesServerName, Enum.GetName( typeof( ReportCategories ), reportCategory ), reportName ) );
            // this launches the report
            return ( String.Format( "{0}/Pages/Report.aspx?ItemPath=/{1}/Reports/{2}", _reportPathHelper.ReportPath, Enum.GetName( typeof( ReportCategories ), reportCategory ), reportName ) );
        }

        public string GetReportDirectory( ReportCategories reportCategory )
        {
            return ( string.Format( "/{0}/Reports", Enum.GetName( typeof( ReportCategories ), reportCategory ) ) );
        }

        public ArrayList GetReportList( System.Web.UI.Page currentPage,  ReportCategories reportCategory )
        {
            ArrayList reportList = null;

            string cacheName = GetCacheNameFromCategory( reportCategory );
            DateTime lastReportModifiedDate = CMGlobals.GetLastModifiedReportDate( currentPage );

            if( HttpContext.Current.Cache[ cacheName ] != null )
            {
                if( CMGlobals.IsCacheDateExpired( currentPage, lastReportModifiedDate, cacheName ) == false )
                {
                    reportList = ( ArrayList )HttpContext.Current.Cache[ cacheName ];
                }
                else
                {
                    reportList = GetReportListHelper( reportCategory );
                    CMGlobals.CreateArrayListCache( currentPage, cacheName, reportList );
                }
            }
            else
            {
                reportList = GetReportListHelper( reportCategory );
                CMGlobals.CreateArrayListCache( currentPage, cacheName, reportList );
            }

            return ( reportList );
        }

        private ArrayList GetReportListHelper( ReportCategories reportCategory )
        {
            ArrayList reportList = new ArrayList();

            ReportingService2010 reportingService = new ReportingService2010();
            reportingService.Url = _reportingServiceUri.ToString();
            reportingService.Credentials = System.Net.CredentialCache.DefaultCredentials;          

            CatalogItem[] reports = null;

            reports = reportingService.ListChildren( GetReportDirectory( reportCategory ), false );

            foreach( CatalogItem report in reports )
            {
                if( GetReportExecutionPath( reportCategory, report.Name ).CompareTo( "/Reports" ) >= 0 )    
               {
                    
                    if( report.Hidden == false )
                    {
                        Report reportObj = new Report( report.Name, report.Path, GetReportExecutionPath( reportCategory, report.Name ) );
                        reportList.Add( reportObj );
                    }
                }
            }
        
            return ( reportList );
        }


    }
}