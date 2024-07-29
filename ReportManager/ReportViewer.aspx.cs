using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;


using VA.NAC.Application.SharedObj;

namespace VA.NAC.ReportManager
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        ReportPathHelper _reportPathHelper = null;

        protected void Page_Load( object sender, EventArgs e )
        {

            CMGlobals.CheckIfStartedProperly( this.Page );
            CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
            _reportPathHelper = new ReportPathHelper();

            try
            {
                if( !IsPostBack )
                {
                    this.ReportViewer1.Reset();
                    this.ReportViewer1.ProcessingMode = ProcessingMode.Remote;
                    this.ReportViewer1.AsyncRendering = false;   // new for 2012
                    this.ReportViewer1.SizeToReportContent = true;  // new for 2012 this lost scrollbars even when needed

                    this.ReportViewer1.ServerReport.ReportServerUrl = new Uri( _reportPathHelper.ReportingServicesPath );

                    Report reportToShow = null;
                    reportToShow = ( Report )Session[ "ReportToShow" ];

                    if( reportToShow != null )
                    {
                        ShowReport( reportToShow );
                    }
                    else // for testing directly
                    {
                        reportToShow = new Report( "/Pharmaceutical/Reports/SelectItemsAndPricesForContract" );
                        reportToShow.AddParameter( "ContractNumber", "5775X" );
                        reportToShow.AddParameter( "FutureHistoricalSelectionCriteria", "B" );
                        ShowReport( reportToShow );
                    }
                }
            }
            catch( Exception ex )
            {
                string tmp = ex.Message;
            }
        }

        public void ShowReport( Report reportToShow )
        {
            string reportPath = reportToShow.ReportPath;
            ArrayList parameterList = reportToShow.ParameterList;

            this.ReportViewer1.ServerReport.ReportPath = reportPath;

            int parameterCount = parameterList.Count;
            if( parameterCount > 0 )
            {
                // set up parameters
                Microsoft.Reporting.WebForms.ReportParameter[] reportParms = new Microsoft.Reporting.WebForms.ReportParameter[ parameterCount ];

                for( int i = 0; i < parameterCount; i++ )
                {
                    Microsoft.Reporting.WebForms.ReportParameter reportParameter = new Microsoft.Reporting.WebForms.ReportParameter( ( ( CMReportParameter )parameterList[ i ] ).Key, ( ( CMReportParameter )parameterList[ i ] ).Value );
                    reportParms[ i ] = reportParameter;
                }
                this.ReportViewer1.ServerReport.SetParameters( reportParms );
            }

            // show the report
            this.ReportViewer1.ServerReport.Refresh();
        }
    }
}
