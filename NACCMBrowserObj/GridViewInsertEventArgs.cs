using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    /// <summary>
    /// Provides data for the <see cref="GridView.RowInserting"/> event.
    /// </summary>
    [Serializable]
    public class GridViewInsertEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initialises a <see cref="GridViewInsertEventArgs"/>.
        /// </summary>
        /// <param name="row">The insertion row.</param>
        /// <param name="newValues">The values for the new row.</param>
        public GridViewInsertEventArgs( GridViewRow row, IOrderedDictionary newValues )
        {
            this._row = row;
            this._values = newValues;
        }

        private GridViewRow _row;

        /// <summary>
        /// The row containing the input controls.
        /// </summary>
        public GridViewRow Row
        {
            get { return this._row; }
        }
        private IOrderedDictionary _values;

        /// <summary>
        /// The values for the new row to insert.
        /// </summary>
        public IOrderedDictionary NewValues
        {
            get { return this._values; }
        }
    }
}
