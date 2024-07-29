using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Threading;
using AjaxControlToolkit;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.ReportManager;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class Start : BaseDocumentEditorPage
    {
        protected new void Page_Load( object sender, EventArgs e )
        {
            //if( Session[ "StartCount" ] == null )
            //{
            //    // this is a hack due to a customer request to not start at the start screen
            //    Session[ "StartCount" ] = 1;
            //    Session[ "NACCMStartedProperly" ] = true;
            //    Response.Redirect( "~/ContractSelectBody.aspx" );   
            //}
            //else
            //{
            //    int startCount = ( int )Session[ "StartCount" ];
            //    Session[ "StartCount" ] = startCount++;
            //}
            Session[ "NACCMStartedProperly" ] = true;

            if( Page.IsPostBack == false && ( ( NACCM )Page.Master ).IsRedirected() == false )
            {
                ClearSessionVariables();

                // set up for report cache
                DateTime lastModifiedReportDate = DateTime.Now;
                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                if( contractDB != null )
                {
                    contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    contractDB.MakeConnectionString();
                    bool bSuccess = contractDB.GetLastModifiedReportDate( out lastModifiedReportDate );
                    if( bSuccess == true )
                    {
                        CMGlobals.SaveLastModifiedReportDate( Page, lastModifiedReportDate );
                    }
                }
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );
        }


        protected void ClearSessionVariables()
        {

        }

        public override void BindAfterShortSave()
        {
     
        }

        public override void RebindHeader()
        {
         
        }

        public override string GetValidationGroupName()
        {
            return ( "Start" );
        }

        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            return ( true );
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            return ( true );
        }

    }
}