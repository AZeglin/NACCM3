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
    public class CustomDocumentInsertingEventArgs : EventArgs
    {
        EditedDocumentContent _editedDocumentContent = null;

        public EditedDocumentContent EditedDocumentContent
        {
          get { return _editedDocumentContent; }
          set { _editedDocumentContent = value; }
        }

        public CustomDocumentInsertingEventArgs( EditedDocumentContent editedDocumentContent )
            : base()
        {
            _editedDocumentContent = editedDocumentContent;
        }
    }
}
