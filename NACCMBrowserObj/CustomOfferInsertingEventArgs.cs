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
    public class CustomOfferInsertingEventArgs : EventArgs
    {
        EditedOfferContent _editedOfferContent = null;

        public EditedOfferContent EditedOfferContent
        {
            get { return _editedOfferContent; }
            set { _editedOfferContent = value; }
        }

        public CustomOfferInsertingEventArgs( EditedOfferContent editedOfferContent )
            : base()
        {
            _editedOfferContent = editedOfferContent;
        }
    }
}
