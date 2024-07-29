using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // parms set when opening the chart window
    [Serializable]
    public class ChartWindowParms
    {
        private int _itemIndex = -1; // id of the row in the item grid that opened the window

        public enum ChartNames
        {
            SBAPercentages,
            Undefined
        }

        private ChartNames _chartName = ChartNames.Undefined;

        public ChartNames ChartName
        {
          get { return _chartName; }
          set { _chartName = value; }
        }

        public enum SupportedChartTypes
        {
            PieChart,
            Undefined
        }

        private SupportedChartTypes _chartType = SupportedChartTypes.Undefined;

        public SupportedChartTypes ChartType
        {
          get { return _chartType; }
          set { _chartType = value; }
        }

        private SBAPercentages _sbaPercentages = null;

        public SBAPercentages TheSBAPercentages
        {
            get { return _sbaPercentages; }
            set { _sbaPercentages = value; }
        }

        public ChartWindowParms( int itemIndex, ChartNames chartName, SupportedChartTypes chartType )
        {
            _itemIndex = itemIndex;
            _chartName = chartName;
            _chartType = chartType;           
        }

        public ChartWindowParms( int itemIndex, ChartNames chartName, SupportedChartTypes chartType, double totalSubConDollars, double sbDollars, double veteranOwnedDollars, double disabledVetDollars, double SDBDollars, double womenOwnedDollars, double hubZoneDollars, double HBCUDollars, DateTime startDate, DateTime endDate )
        {
            _itemIndex = itemIndex;
            _chartName = chartName;
            _chartType = chartType;

            _sbaPercentages = new SBAPercentages( totalSubConDollars, sbDollars, veteranOwnedDollars, disabledVetDollars, SDBDollars, womenOwnedDollars, hubZoneDollars, HBCUDollars, startDate, endDate );
        }

        [Serializable]
        public class SBAPercentages
        {
            private double _totalSubConDollarsPercentage = 0;

            public double TotalSubConDollarsPercentage
            {
                get { return _totalSubConDollarsPercentage; }
                set { _totalSubConDollarsPercentage = value; }
            }

            private double _sbDollarsPercentage = 0;

            public double SbDollarsPercentage
            {
                get { return _sbDollarsPercentage; }
                set { _sbDollarsPercentage = value; }
            }

            private double _veteranOwnedDollarsPercentage = 0;

            public double VeteranOwnedDollarsPercentage
            {
                get { return _veteranOwnedDollarsPercentage; }
                set { _veteranOwnedDollarsPercentage = value; }
            }

            private double _disabledVetDollarsPercentage = 0;

            public double DisabledVetDollarsPercentage
            {
                get { return _disabledVetDollarsPercentage; }
                set { _disabledVetDollarsPercentage = value; }
            }

            private double _SDBDollarsPercentage = 0;

            public double SDBDollarsPercentage
            {
                get { return _SDBDollarsPercentage; }
                set { _SDBDollarsPercentage = value; }
            }

            private double _womenOwnedDollarsPercentage = 0;

            public double WomenOwnedDollarsPercentage
            {
                get { return _womenOwnedDollarsPercentage; }
                set { _womenOwnedDollarsPercentage = value; }
            }

            private double _hubZoneDollarsPercentage = 0;

            public double HubZoneDollarsPercentage
            {
                get { return _hubZoneDollarsPercentage; }
                set { _hubZoneDollarsPercentage = value; }
            }

            private double _HBCUDollarsPercentage = 0;

            public double HBCUDollarsPercentage
            {
                get { return _HBCUDollarsPercentage; }
                set { _HBCUDollarsPercentage = value; }
            }

            private double _OtherPercentage = 0;

            public double OtherPercentage
            {
                get { return _OtherPercentage; }
                set { _OtherPercentage = value; }
            }

            private DateTime _startDate;

            public DateTime StartDate
            {
                get { return _startDate; }
                set { _startDate = value; }
            }

            private DateTime _endDate;

            public DateTime EndDate
            {
                get { return _endDate; }
                set { _endDate = value; }
            }

            public SBAPercentages( double totalSubConDollars, double sbDollars, double veteranOwnedDollars, double disabledVetDollars, double SDBDollars, double womenOwnedDollars, double hubZoneDollars, double HBCUDollars, DateTime startDate, DateTime endDate )
            {
                double totalCategorizedDollars = sbDollars + veteranOwnedDollars + disabledVetDollars + SDBDollars + womenOwnedDollars + hubZoneDollars + HBCUDollars;
                double otherDollars = totalSubConDollars - totalCategorizedDollars;

                _totalSubConDollarsPercentage = Math.Round( ( double )( ( totalSubConDollars / totalSubConDollars ) * 100.000 ), 2 );
                _sbDollarsPercentage = Math.Round( ( double )( ( sbDollars / totalSubConDollars ) * 100.000 ), 2 );
                _veteranOwnedDollarsPercentage = Math.Round( ( double )( ( veteranOwnedDollars / totalSubConDollars ) * 100.000 ), 2 );
                _disabledVetDollarsPercentage = Math.Round( ( double )( ( disabledVetDollars / totalSubConDollars ) * 100.000 ), 2 );
                _SDBDollarsPercentage = Math.Round( ( double )( ( SDBDollars / totalSubConDollars ) * 100.000 ), 2 );
                _womenOwnedDollarsPercentage = Math.Round( ( double )( ( womenOwnedDollars / totalSubConDollars ) * 100.000 ), 2 );
                _hubZoneDollarsPercentage = Math.Round( ( double )( ( hubZoneDollars / totalSubConDollars ) * 100.000 ), 2 );
                _HBCUDollarsPercentage = Math.Round( ( double )( ( HBCUDollars / totalSubConDollars ) * 100.000 ), 2 );
                _OtherPercentage = 100 - ( _sbDollarsPercentage + _veteranOwnedDollarsPercentage + _disabledVetDollarsPercentage + _SDBDollarsPercentage + _womenOwnedDollarsPercentage + _hubZoneDollarsPercentage + _HBCUDollarsPercentage );
                _startDate = startDate;
                _endDate = endDate;

            }

        }     
    }
}
