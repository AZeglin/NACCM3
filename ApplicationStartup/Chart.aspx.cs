using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Drawing;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.DataVisualization;
using System.Xml.Linq;

using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.BrowserObj;

using Series = System.Web.UI.DataVisualization.Charting.Series;
using ChartArea = System.Web.UI.DataVisualization.Charting.ChartArea;
using DataPoint = System.Web.UI.DataVisualization.Charting.DataPoint;
using Legend = System.Web.UI.DataVisualization.Charting.Legend;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class Chart : System.Web.UI.Page
    {
        ChartWindowParms _chartWindowParms = null;
        ChartWindowParms.SupportedChartTypes _chartType = ChartWindowParms.SupportedChartTypes.Undefined;
        ChartWindowParms.ChartNames _chartName = ChartWindowParms.ChartNames.Undefined;
        Series _series1;
        ChartArea _chartArea1;
        Legend _legend1;

        protected void Page_Load( object sender, EventArgs e )
        {
            CMGlobals.CheckIfStartedProperly( this.Page );

            if( Session[ "ChartWindowParms" ] != null )
            {
                _chartWindowParms = ( ChartWindowParms )Session[ "ChartWindowParms" ];
            }

            _chartType = _chartWindowParms.ChartType;
            _chartName = _chartWindowParms.ChartName;

            ShowSelectedChart();

        }

        protected void ShowSelectedChart()
        {
            _series1 = Chart1.Series.Add( "ChartSeries1" );
            _chartArea1 = Chart1.ChartAreas.Add( "ChartArea1" );
            _legend1 = Chart1.Legends.Add( "Legend1" );

            _series1.ChartArea = "ChartArea1";
            _series1.Legend = "Legend1";
       
            if( _chartType == ChartWindowParms.SupportedChartTypes.PieChart )
            {
                _series1.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.Pie;
            }

            Color[] customPalette = new Color[ 8 ];
            customPalette[ 0 ] = ColorTranslator.FromHtml( "#73C167" );
            customPalette[ 1 ] = ColorTranslator.FromHtml( "#FFD24F" );
            customPalette[ 2 ] = ColorTranslator.FromHtml( "#F58426" );
            customPalette[ 3 ] = ColorTranslator.FromHtml( "#EF3E42" );
            customPalette[ 4 ] = ColorTranslator.FromHtml( "#D38CBD" );
            customPalette[ 5 ] = ColorTranslator.FromHtml( "#8177B7" );
            customPalette[ 6 ] = ColorTranslator.FromHtml( "#00AFDB" );
            customPalette[ 7 ] = ColorTranslator.FromHtml( "#3342B4" );

            Chart1.PaletteCustomColors = customPalette;
            _series1[ "PieLabelStyle" ] = "Outside";
            _series1[ "3DLabelLineSize" ] = "46";
            _series1.LegendText = "#AXISLABEL (#PERCENT)";   
            _series1.BorderWidth = 1;
            _series1.BorderColor = System.Drawing.Color.FromArgb( 26, 59, 105 );

            //// group smaller values
            //_series1[ "CollectedThreshold" ] = "0.5";
            //_series1[ "CollectedLabel" ] = "Other";
            //_series1[ "CollectedColor" ] = "Blue";

            _chartArea1.Area3DStyle.Enable3D = true;
            _chartArea1.Area3DStyle.Inclination = 10;
            _chartArea1.Position.Auto = false;
            _chartArea1.Position = new System.Web.UI.DataVisualization.Charting.ElementPosition( 0, 0, 100, 46 );

            _legend1.Position.Auto = false;
            _legend1.LegendStyle = System.Web.UI.DataVisualization.Charting.LegendStyle.Table;
            _legend1.TableStyle = System.Web.UI.DataVisualization.Charting.LegendTableStyle.Wide;
            _legend1.TextWrapThreshold = 20;
        //    _legend1.Docking = System.Web.UI.DataVisualization.Charting.Docking.Bottom; 
            _legend1.IsDockedInsideChartArea = false;
                        
            _legend1.Position = new System.Web.UI.DataVisualization.Charting.ElementPosition( 0, 47, 100, 54 );

            Chart1.DataManipulator.Sort( System.Web.UI.DataVisualization.Charting.PointSortOrder.Descending, _series1 );

            if( _chartName == ChartWindowParms.ChartNames.SBAPercentages )
            {
                Label chartCaption1 = ( Label )FindControl( "ChartCaption1" );
                if( chartCaption1 != null )
                {
                    chartCaption1.Text = "SBA Percentages";
                }

                Label chartCaption2 = ( Label )FindControl( "ChartCaption2" );
                if( chartCaption2 != null )
                {
                    chartCaption2.Text = string.Format( "From {0} to {1}", _chartWindowParms.TheSBAPercentages.StartDate.ToShortDateString(), _chartWindowParms.TheSBAPercentages.EndDate.ToShortDateString() );
                }

                ShowSBAPercentagesChart();
            }
            
        }

        protected  const int SBAPointCount = 8;
        string[] _sbaPercentagesLabels = new string[ SBAPointCount ] { "Small Business      ", "Veteran             ", "Disabled Veteran    ", "Small Disadvantaged ", 
                                                "Woman               ", "Hub Zone            ", "HBCU                ", "Other               " };

        protected double GetSubcontractingPercentageValue( int index )
        {
            double retVal = 0;
            switch( index )
            {
                case 0:
                    retVal = _chartWindowParms.TheSBAPercentages.SbDollarsPercentage;
                    break;
                case 1:
                    retVal = _chartWindowParms.TheSBAPercentages.VeteranOwnedDollarsPercentage;
                    break;
                case 2:
                    retVal = _chartWindowParms.TheSBAPercentages.DisabledVetDollarsPercentage;
                    break;
                case 3:
                    retVal = _chartWindowParms.TheSBAPercentages.SDBDollarsPercentage;
                    break;
                case 4:
                    retVal = _chartWindowParms.TheSBAPercentages.WomenOwnedDollarsPercentage;
                    break;
                case 5:
                    retVal = _chartWindowParms.TheSBAPercentages.HubZoneDollarsPercentage;
                    break;
                case 6:
                    retVal = _chartWindowParms.TheSBAPercentages.HBCUDollarsPercentage;
                    break;
                case 7:
                    retVal = _chartWindowParms.TheSBAPercentages.OtherPercentage;
                    break;
            }

            return ( retVal );
        }

        protected void ShowSBAPercentagesChart()
        {
            _series1.YValueType = System.Web.UI.DataVisualization.Charting.ChartValueType.Double;

            for( int i = 0; i < SBAPointCount; i++ )
            {
                DataPoint p = new DataPoint();

                p.AxisLabel = _sbaPercentagesLabels[ i ];

                Double[] valuesArray = new Double[ 1 ];
                valuesArray[ 0 ] = ( Double )GetSubcontractingPercentageValue( i );
                p.YValues = valuesArray;
             
                _series1.Points.Add( p );
            }
        }
    }
}