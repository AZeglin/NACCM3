using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class DataRelayInsertSuccessEventArgs : EventArgs
    {
        Page _currentPage = null;

        public Page CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; }
        }

        public DataRelayInsertSuccessEventArgs( Page currentPage )
            : base()
        {
            _currentPage = currentPage;
        }

    }
}
