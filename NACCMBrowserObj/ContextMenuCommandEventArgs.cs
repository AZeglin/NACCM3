using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Collections;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class ContextMenuCommandEventArgs : CommandEventArgs
    {
        private GridViewRow _gridViewRow = null;
        private int _gridViewRowId = -1;

        public int GridViewRowId
        {
            get { return _gridViewRowId; }
            set { _gridViewRowId = value; }
        }

        public GridViewRow GridViewRow
        {
            get { return _gridViewRow; }
            set { _gridViewRow = value; }
        }

        public ContextMenuCommandEventArgs( string commandName, object argument )
            : base( commandName, argument )
        {
        }

        public ContextMenuCommandEventArgs( string commandName, object argument, int gridViewRowId, GridViewRow gridViewRow )
            : base( commandName, argument )
        {
            _gridViewRowId = gridViewRowId;
            _gridViewRow = gridViewRow;
        }

    }
}
