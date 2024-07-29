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
	/// Provides data for the <see cref="GridView.RowInserted"/> delegate.
	/// </summary>
    [Serializable]
    public class GridViewInsertedEventArgs : EventArgs
    {
        /// <summary>
        /// Initialises a <see cref="GridViewInsertedEventArgs"/>.
        /// </summary>
        /// <param name="newValues">The values for the new row.</param>
        /// <param name="ex">The exception that occurred during the insert, if any.</param>
        public GridViewInsertedEventArgs( IOrderedDictionary newValues, Exception ex )
        {
            this._values = newValues;
            this._exception = ex;
        }


        private IOrderedDictionary _values;

        /// <summary>
        /// The values that were inserted into the new row.
        /// </summary>
        public IOrderedDictionary NewValues
        {
            get { return this._values; }
        }

        private Exception _exception;

        /// <summary>
        /// The exception that occurred during the insert, if any.
        /// </summary>
        public Exception Exception
        {
            get { return this._exception; }
        }

        private bool _exceptionHandled;

        /// <summary>
        /// Whether the exception that occurred during the insert has been handled.
        /// </summary>
        public bool ExceptionHandled
        {
            get { return this._exceptionHandled; }
            set { this._exceptionHandled = value; }
        }
    }
}
