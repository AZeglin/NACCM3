using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VA.NAC.ReportManager
{
    [Serializable]
    public class CMReportParameter
    {
        private string _key = "";
        private string _value = "";

        public CMReportParameter( string key, string value )
        {
            _key = key;
            _value = value;
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

    }
}
