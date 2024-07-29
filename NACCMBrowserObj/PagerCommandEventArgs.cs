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
    public class PagerCommandEventArgs : EventArgs
    {
        private string _commandName = "";

        private int _pageNumber = -1;
        private int _rowsPerPage = -1;
        private int _totalRowsInDataSet = -1;


        public string CommandName
        {
            get { return _commandName; }
            set { _commandName = value; }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
            set { _pageNumber = value; }
        }

        public int RowsPerPage
        {
            get { return _rowsPerPage; }
            set { _rowsPerPage = value; }
        }

        public int TotalRowsInDataSet
        {
            get { return _totalRowsInDataSet; }
            set { _totalRowsInDataSet = value; }
        }

        public PagerCommandEventArgs( string commandName, int totalRowsInDataSet )
            : base()
        {
            _commandName = commandName;
            _totalRowsInDataSet = totalRowsInDataSet;
        }

        public PagerCommandEventArgs( string commandName, int pageNumber, int rowsPerPage )
            : base()
        {
            _commandName = commandName;
            _pageNumber = pageNumber;
            _rowsPerPage = rowsPerPage;
        }

        public PagerCommandEventArgs( string commandName, int pageNumber, int rowsPerPage, int totalRowsInDataSet )
            : base()
        {
            _commandName = commandName;
            _pageNumber = pageNumber;
            _rowsPerPage = rowsPerPage;
            _totalRowsInDataSet = totalRowsInDataSet;
        }
    }
}
